using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Web.Script.Serialization;
using System.Windows.Threading;

namespace AuctionServerWpf
{
    public class AuctionServer
    {
        static TcpListener tcpListener; // сервер для прослушивания
        List<ClientObject> clients = new List<ClientObject>(); // все подключения
        static Trade actualTrade;
        static Trade nextTrade;

        internal static Trade ActualTrade { get => actualTrade; set => actualTrade = value; }
        public static int TimeWithoutBets { get => timeWithoutBets; set => timeWithoutBets = value; }
        public static System.Timers.Timer Timer { get => timer; set => timer = value; }
        public static Trade NextTrade { get => nextTrade; set => nextTrade = value; }


        private static int timeWithoutBets = 0;

        public delegate void MethodContainer();

        public static event MethodContainer OnTradeFinish;

        private static System.Timers.Timer timer = null;

        private void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            if (timeWithoutBets <= 10)
            {
                timeWithoutBets++;
                MainWindow.Main.Dispatcher.Invoke(() =>
                {
                    Response response = new Response(methods.SetTimer(), timeWithoutBets.ToString());
                    FullBroadcastMessage(SerializeToString(response) + "%");
                    MainWindow.Main.TradeTimer.Text = timeWithoutBets.ToString();
                });
            }
        }

        public void SetTimer()
        {
            // Create a timer with a three second interval.
            timer = new System.Timers.Timer(3000);
            // Hook up the Elapsed event for the timer. 
            timer.Elapsed += OnTimedEvent;
            timer.AutoReset = true;
        }

        RequestMethods methods = new RequestMethods();

        public string DisconnectServer()
        {
            FullBroadcastMessage(Requester.CreateResponse(methods.DisconnectServer(), false));
            return null;
        }

        public bool StartTrade()
        {
            BackgroundWorker worker = new BackgroundWorker();
            using (AuctionContext db = new AuctionContext())
            {
                Product product = db.Products.Find(actualTrade.ProductId);
                MainWindow.Main.ProductName.Text = product.Name;
                MainWindow.Main.ProductId.Text = actualTrade.TradeId.ToString();
            }
            MainWindow.Main.IsTradeRuningNow = true;
            worker.DoWork += (o, ea) =>
            {
                timer.Start();
                while (true)
                {
                    if (timeWithoutBets >= 10)
                    {
                        if (timer != null)
                        {
                            timer.Stop();
                        }
                        MainWindow.Main.Dispatcher.Invoke(() =>
                        {
                            MainWindow.Main.IsTradeRuningNow = false;
                            MainWindow.Main.TradeTimer.Text = "STOP";
                        });
                        Response response = new Response(methods.SetTimer(), "STOP");
                        FullBroadcastMessage(SerializeToString(response) + "%");
                        break;
                    }
                }

            };
            worker.RunWorkerCompleted += (o, ea) =>
            {
                timeWithoutBets = 0;
                actualTrade.TradeFinishTime = DateTime.Now;
                using (AuctionContext db = new AuctionContext())
                {
                    if (ActualTrade.MaxBetAccountId != 0)
                    {
                        Trade trade = db.Trades.Find(ActualTrade.TradeId);
                        trade.ChangeProperties(ActualTrade);
                        var product = db.Products.Find(trade.ProductId);
                        product.IsSold = true;
                        product.Price = actualTrade.MaxBet;
                    }
                    else if (db.Trades.Count() > 1)
                    {
                        var trade = db.Trades.Remove(db.Trades.Find(ActualTrade.TradeId));
                        db.SaveChanges();
                        db.Trades.Add(trade);
                    }
                    db.SaveChanges();
                }
                OnTradeFinish?.Invoke();
            };
            worker.RunWorkerAsync();
            return true;
        }

        public static string SerializeToString<T>(T obj)
        {
            return new JavaScriptSerializer().Serialize(obj);
        }


        public static T DeserializeFromString<T>(string objectString)
        {
            return new JavaScriptSerializer().Deserialize<T>(objectString);
        }

        protected internal void AddConnection(ClientObject clientObject)
        {
            clients.Add(clientObject);
        }
        protected internal void RemoveConnection(string id)
        {
            // получаем по id закрытое подключение
            ClientObject client = clients.FirstOrDefault(c => c.Id == id);
            // и удаляем его из списка подключений
            if (client != null)
                clients.Remove(client);
        }
        // прослушивание входящих подключений
        protected internal void Listen()
        {
            try
            {
                tcpListener = new TcpListener(IPAddress.Any, 8888);
                tcpListener.Start();
                SetTimer();

                while (true)
                {
                    TcpClient tcpClient = tcpListener.AcceptTcpClient();

                    ClientObject clientObject = new ClientObject(tcpClient, this);
                    Thread clientThread = new Thread(new ThreadStart(clientObject.Process));
                    clientThread.Start();
                }
            }
            catch (Exception ex)
            {
                MainWindow.Main.Dispatcher.Invoke(() =>
                {
                    MainWindow.Main.ErrorText.Text = ex.Message;
                });
                Disconnect();
            }
        }

        protected internal void SingleMessage(string message, string id)
        {
            //byte[] data = Encoding.Unicode.GetBytes(message);
            for (int i = 0; i < clients.Count; i++)
            {
                if (clients[i].Id == id)
                {
                    BinaryWriter writer = new BinaryWriter(clients[i].Stream);
                    writer.Write(message); //передача данных
                    return;
                }
            }
        }

        protected internal void SendImageInBytes(byte[] data, string id)
        {
            for (int i = 0; i < clients.Count; i++)
            {
                if (clients[i].Id == id)
                {
                    BinaryWriter writer = new BinaryWriter(clients[i].Stream);
                    writer.Write(data); //передача данных
                    return;
                }
            }
        }

        protected internal void FullBroadcastMessage(string message)
        {
            for (int i = 0; i < clients.Count; i++)
            {
                if (clients[i].IsOpen)
                {
                    BinaryWriter writer = new BinaryWriter(clients[i].Stream);
                    writer.Write(message); //передача данных
                }
            }
        }

        protected internal void BroadcastMessage(string message, List<ClientObject> clientList)
        {
            foreach (ClientObject client in clientList)
            {
                if (client.IsOpen)
                {
                    BinaryWriter writer = new BinaryWriter(client.Stream);
                    writer.Write(message); 
                }
            }
        }

        // трансляция сообщения подключенным клиентам
        protected internal void BroadcastMessage(string message, string id)
        {
            for (int i = 0; i < clients.Count; i++)
            {
                if (clients[i].Id != id && clients[i].IsOpen) // если id клиента не равно id отправляющего
                {
                    BinaryWriter writer = new BinaryWriter(clients[i].Stream);
                    writer.Write(message); //передача данных
                }
            }
        }
        // отключение всех клиентов
        protected internal void Disconnect()
        {
            tcpListener.Stop(); //остановка сервера

            for (int i = 0; i < clients.Count; i++)
            {
                clients[i].Close(); //отключение клиента
            }
            Environment.Exit(0); //завершение процесса
        }
    }
}

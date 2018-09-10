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
using System.Windows;
using System.Windows.Threading;

namespace AuctionServerWpf
{
    public class AuctionServer
    {
        static TcpListener tcpListener; // сервер для прослушивания
        List<ClientObject> clients = new List<ClientObject>(); // все подключения


        public delegate void MethodContainer();

        public static event MethodContainer OnTradeFinish;

        RequestMethods methods = new RequestMethods();

        public string DisconnectServer()
        {
            FullBroadcastMessage(Requester.CreateResponse(methods.DisconnectServer(), false));
            return null;
        }


        public static string SerializeToString<T>(T obj)
        {
            try
            {
                return new JavaScriptSerializer().Serialize(obj);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }


        public static T DeserializeFromString<T>(string objectString)
        {
            try
            {
                return new JavaScriptSerializer().Deserialize<T>(objectString);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
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
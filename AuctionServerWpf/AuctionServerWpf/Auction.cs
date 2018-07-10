using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;

namespace AuctionServerWpf
{
    class Auction
    {
        private AuctionServer server;
        private readonly long auctionId;
        private Trade actualTrade;
        private bool isTradeStarted = false;

        private static List<Auction> auctionsList;

        static Auction()
        {
            auctionsList = auctionsList = new List<Auction>();
        }

        public static void AddAuction(Auction auction)
        {
            auctionsList.Add(auction);
        }

        public static bool TradeEnter(ClientObject client, int id)
        {
            foreach (Auction auction in auctionsList)
            {
                if (auction.AuctionId == id)
                {
                    auction.AddClient(client);
                    return true;
                }
            }
            return false;
        }

        public static bool IsAuctionRuning(int id)
        {
            try
            {
                foreach (Auction auction in auctionsList)
                {
                    if (auction.AuctionId == id)
                    {
                        return auction.IsTradeStarted;
                    }
                }
                throw new Exception($"Trade with id = {id} doesn't exist");
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
                return false;
            }
        }

        private List<ClientObject> clientList;

        public Auction(AuctionServer server, Trade trade)
        {
            this.server = server;
            clientList = new List<ClientObject>();
            actualTrade = trade;
            auctionId = trade.TradeId;
        }

        public void AddClient(ClientObject client)
        {
            clientList.Add(client);
        }

        //internal  Trade ActualTrade { get => actualTrade; set => actualTrade = value; }
        //public  int TimeWithoutBets { get => timeWithoutBets; set => timeWithoutBets = value; }
        //public Timer Timer { get => timer; set => timer = value; }

        private  int timeWithoutBets = 0;

        public delegate void MethodContainer();


        private Timer timer = null;
        private Timer methodStartTimer = null;

        private void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            if (timeWithoutBets <= 1000)
            {
                timeWithoutBets++;
                MainWindow.Main.Dispatcher.Invoke(() =>
                {
                    Response response = new Response(methods.SetTimer(), timeWithoutBets.ToString());
                    server.BroadcastMessage(AuctionServer.SerializeToString(response) + "%", clientList);
                    MainWindow.Main.TradeTimer.Text = timeWithoutBets.ToString();
                });
            }
        }

        private void OnCreateTradeEvent(Object source, ElapsedEventArgs e)
        {
            StartTrade();
        }

        public void SetTimer()
        {
            // Create a timer with a three second interval.
            timer = new Timer(3000);
            // Hook up the Elapsed event for the timer. 
            timer.Elapsed += OnTimedEvent;
            timer.AutoReset = true;
        }

        RequestMethods methods = new RequestMethods();

        public bool IsTradeStarted  => isTradeStarted;

        public long AuctionId => auctionId;

        public void CreateTimer()
        {
            double interval = actualTrade.TradeStartTime.TimeOfDay.TotalMilliseconds - DateTime.Now.TimeOfDay.TotalMilliseconds;
            methodStartTimer = new Timer(interval);
            timer.Elapsed += OnCreateTradeEvent;
            timer.Start();
        }

        private void StartTrade()
        {
            BackgroundWorker worker = new BackgroundWorker();
            using (AuctionContext db = new AuctionContext())
            {
                Product product = db.Products.Find(actualTrade.ProductId);
                MainWindow.Main.ProductName.Text = product.Name;
                MainWindow.Main.ProductId.Text = actualTrade.TradeId.ToString();
            }
            isTradeStarted = true;
            worker.DoWork += (o, ea) =>
            {
                timer.Start();
                while (true)
                {
                    if (timeWithoutBets >= 1000)
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
                        server.FullBroadcastMessage(AuctionServer.SerializeToString(response) + "%");
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
                    if (actualTrade.MaxBetAccountId != 0)
                    {
                        Trade trade = db.Trades.Find(actualTrade.TradeId);
                        trade.ChangeProperties(actualTrade);
                        var product = db.Products.Find(trade.ProductId);
                        product.IsSold = true;
                        product.Price = actualTrade.MaxBet;
                    }
                    else if (db.Trades.Count() > 1)
                    {
                        var trade = db.Trades.Remove(db.Trades.Find(actualTrade.TradeId));
                        db.SaveChanges();
                        db.Trades.Add(trade);
                    }
                    db.SaveChanges();
                }
                //OnTradeFinish?.Invoke();
            };
            worker.RunWorkerAsync();
            return;
        }
    }
}

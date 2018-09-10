using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Web.Script.Serialization;
using System.Windows;
using System.Windows.Threading;

namespace AuctionServerWpf
{
    public class Auction
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
                    client.Auction = auction;
                    return true;
                }
            }
            return false;
        }

        public bool TradeLeave(ClientObject client)
        {
            clientList.Remove(client);
            client.Auction = null;
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
            catch (Exception ex)
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

        private void AddClient(ClientObject client)
        {
            clientList.Add(client);
        }

        //internal  Trade ActualTrade { get => actualTrade; set => actualTrade = value; }
        //public  int TimeWithoutBets { get => timeWithoutBets; set => timeWithoutBets = value; }
        //public Timer Timer { get => timer; set => timer = value; }

        private int timeWithoutBets = 0;

        public delegate void MethodContainer();


        private Timer timer = null;
        private Timer methodStartTimer = null;

        private void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            if (timeWithoutBets <= 1000)
            {
                timeWithoutBets++;
                Response response = new Response(methods.SetTimer(), timeWithoutBets.ToString());
                server.BroadcastMessage(AuctionServer.SerializeToString(response) + "%", clientList);
                //MainWindow.Main.TradeTimer.Text = timeWithoutBets.ToString();
            }
            else
            {
                lock (locker)
                {
                    System.Threading.Monitor.Pulse(locker);
                }
                timer.Stop();
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
            timer.Start();
        }

        public static Auction GetTrade(int id)
        {
            return auctionsList.Where(n => n.AuctionId == id).First();
        }

        RequestMethods methods = new RequestMethods();

        public bool IsTradeStarted => isTradeStarted;

        public long AuctionId => auctionId;

        public int TimeWithoutBets { get => timeWithoutBets; set => timeWithoutBets = value; }

        public float ActualTradeMaxBet { get => actualTrade.MaxBet; set => actualTrade.MaxBet = value; }
        public Trade ActualTrade { get => actualTrade; set => actualTrade = value; }
        private Object locker = new object();

        public void CreateTimer()
        {
            //double interval = actualTrade.TradeStartTime.TimeOfDay.TotalMilliseconds - DateTime.Now.TimeOfDay.TotalMilliseconds;
            methodStartTimer = new Timer(100);
            methodStartTimer.Elapsed += OnCreateTradeEvent;
            methodStartTimer.AutoReset = false;
            methodStartTimer.Start();
        }

        private void StartTrade()
        {   
            try
            {
                using (AuctionContext db = new AuctionContext())
                {
                    Product product = db.Products.Find(actualTrade.ProductId);
                }
                isTradeStarted = true;
                SetTimer();
                lock (locker)
                {
                    System.Threading.Monitor.Wait(locker);
                }
                Response response = new Response(methods.SetTimer(), "STOP");
                server.FullBroadcastMessage(AuctionServer.SerializeToString(response) + "%");
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
                    else 
                    {
                        var trade = db.Trades.Remove(db.Trades.Find(actualTrade.TradeId));
                        db.SaveChanges();
                        db.Trades.Add(trade);
                    }
                    db.SaveChanges();
                }
                return;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}

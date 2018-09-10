using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Drawing;
using System.Timers;

namespace AuctionServerWpf
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Main = this;
        }

        static AuctionServer server; // сервер
        static Thread listenThread; // потока для прослушивания

        public static bool IsTradeRuning
        {
            get => !Main.IsEnabled;
            set => Main.IsEnabled = !value;
        }
        public static MainWindow Main { get; set; }
        public bool IsTradeRuningNow { get; set; } = false;

        private static int breakTimeSec = 300;

        private static Trade GetNextTrade()
        {
            using (AuctionContext db = new AuctionContext())
            {
                List<Trade> trades = db.Trades.Where(n => n.MaxBetAccountId == 0).ToList();
                if (trades.Count > 0)
                {
                    return trades.OrderBy(n => n.TradeId).First();
                }
                else
                {
                    IsTradeRuning = false;
                    Main.ErrorText.Text = "Нет текущих лотов";
                }
                return new Trade();
            }
        }

        private static void StartNewTrade(Trade trade)
        {
            Auction auction = new Auction(server, trade);
            Auction.AddAuction(auction);
            auction.CreateTimer();
            //AuctionServer.NextTrade = GetNextTrade();
            //if (AuctionServer.NextTrade.TradeId == -1) return;
            //AuctionServer.NextTrade.TradeStartTime = DateTime.Now;
            //if (IsTradeRuning) AuctionServer.ActualTrade.TradeStartTime.AddSeconds(breakTimeSec);
            //Main.StartTime.Text = AuctionServer.ActualTrade.TradeStartTime.ToShortTimeString();
            //if (IsTradeRuning) await Task.Delay(breakTimeSec * 1000);
            //AuctionServer.ActualTrade = AuctionServer.NextTrade;
            //server.StartTrade();
            //IsTradeRuning = true;

        }

        private void StartTrade_Click(object sender, RoutedEventArgs e)
        {
            //StartNewTrade();
        }

        private static void StartServer()
        {
            try
            {
                server = new AuctionServer();
                listenThread = new Thread(new ThreadStart(server.Listen));
                listenThread.Start(); //старт потока
                using (AuctionContext db = new AuctionContext())
                {
                    foreach(Trade trade in db.Trades)
                    {
                        if(trade.TradeStartTime.ToShortDateString().Equals(DateTime.Now.ToShortDateString()))// && trade.TradeStartTime > DateTime.Now)
                        {
                            StartNewTrade(trade);
                        }
                    }
                }
                //AuctionServer.ActualTrade = new Trade();
                //AuctionServer.NextTrade = GetNextTrade();
                //AuctionServer.OnTradeFinish += StartNewTrade;
            }
            catch (Exception ex)
            {
                server.Disconnect();
            }
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Requester.Work.Dispose();
            RequestMethods methods = new RequestMethods();
            string message = Requester.CreateResponse(methods.DisconnectServer());
            server.FullBroadcastMessage(message);
            //if (AuctionServer.Timer != null)
            //{
            //    AuctionServer.Timer.Stop();
            //    AuctionServer.Timer.Dispose();
            //}
            server.Disconnect();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DataContext = new MainWindowViewModel();
            StartServer();
            //AuctionServer.OnTradeFinish += StartNewTrade;
        }
    }
}

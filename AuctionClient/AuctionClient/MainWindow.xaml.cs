using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace AuctionClient
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public MainWindow(Account account)
        {
            Requester.OnTradeupdate += SetStatusBarTrade;
            Requester.OnStatusBarProductUpdate += SetStatusBarProduct;
            Requester.OnStatusBarTradeUpdate += SetStatusBarTrade;
            AuctionPageViewModel.OnStatusBarLocalTradeUpdate += SetLocalStatusBarTrade;
            InitializeComponent();
            if (account.IsAdmin) AdminP.Visibility = Visibility.Visible;
            DataContext = this;
            userAcc = account;
            mFrame = MainFrame;
            //FrameContent = new TradesMenu();
            Main = this;
        }

        Account userAcc;

        private Trade actualTrade;
        private Product actualProduct;
        private static Frame mFrame;
        private static MainWindow Main;
        private string windowStatus;
        private static Object frameContent;

        public event PropertyChangedEventHandler PropertyChanged;
        public static event PropertyChangedEventHandler StaticPropertyChanged;

        public static Frame MFrame { get => mFrame; set => mFrame = value; }
        public string WindowStatus
        {
            get => windowStatus;
            set
            {
                windowStatus = value;
                RaisePropertyChanged("WindowStatus");
            }
        }

        public static object FrameContent
        {
            get => frameContent;
            set
            {
                frameContent = value;
                OnStaticPropertyChanged("FrameContent");
            }
        }

        private void RaisePropertyChanged(string v)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(v));
        }

        private static void OnStaticPropertyChanged(string v)
        {
            StaticPropertyChanged?.Invoke(new object(), new PropertyChangedEventArgs(v));
        }

        private void SetStatusBarTrade(string response)
        {
            actualTrade = ClientAuction.DeserializeFromString<Trade>(response);
            Dispatcher.Invoke(() =>
            {
                if (actualTrade == null) return;
                ActualPrice.Text = actualTrade.MaxBet.ToString();
                if (StartTime.Text.Equals("-"))
                {
                    StartTime.Text = actualTrade.TradeStartTime.ToShortTimeString();
                }
            });
        }

        private void SetLocalStatusBarTrade(string response)
        {
            try
            {
                Dispatcher.Invoke(() =>
                {
                    ActualPrice.Text = response;
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void SetStatusBarProduct(string response)
        {
            try
            {
                actualProduct = ClientAuction.DeserializeFromString<Product>(response);
                Dispatcher.Invoke(() =>
                {
                    ActualLot.Text = actualProduct.Name;
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Auction_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Status.Text = "Auction";
                //if (CheckServer()) return;
                //RequestMethods methods = new RequestMethods();
                //Requester.CreateRequest(methods.CheckAuctionStatus());
                //bool isTradeRuning = await Requester.WaitResponseAsync<bool>();
                //if (isTradeRuning)
                //{
                //    MainFrame.Content = new AuctionPage(this, userAcc);
                //}
                //else
                //{
                FrameContent = new TradesMenu();
                //}
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка");
            }
        }

        private void CheckAuction()
        {
            if (MainFrame.Content is AuctionPage)
            {
                //((AuctionPage)MainFrame.Content).Dispose();
            }
        }

        private bool CheckServer()
        {
            if (ClientAuction.Stream == null)
            {
                if (!ServerConnector.ConnectToServer())
                {
                    return true;
                }
            }
            return false;
        }

        private void MyCab_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Status.Text = "Cabinet";
                if (CheckServer()) return;
                CheckAuction();
                MainFrame.Content = new MyCabPage(userAcc);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка");
            }
        }

        public static void Logout()
        {
            Main.Dispatcher.Invoke(() =>
            {
                EnteringWindow entering = new EnteringWindow();
                entering.Show();
            });
            //Requester.Timer?.Dispose();
            ClientAuction.ClearConnect();
            Main.Dispatcher.Invoke(() =>
            {
                Main.Closing -= Main.Window_Closing;
                Main.Close();
                Main.Closing += Main.Window_Closing;
            });
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            Logout();
        }

        private void AdminPanel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Status.Text = "Admin";
                if (CheckServer()) return;
                CheckAuction();
                MainFrame.Content = new AdminPanel();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка");
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            //Requester.Timer?.Dispose();
            ClientAuction.Disconnect();
            System.Windows.Threading.Dispatcher.ExitAllFrames();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Auction_Click(sender, e);
        }
    }
}

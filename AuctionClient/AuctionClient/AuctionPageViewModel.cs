using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace AuctionClient
{
    class AuctionPageViewModel : ViewModelBase, IDisposable
    {
        public AuctionPageViewModel()
        {
            Requester.OnTimer += TimerSetter;
            Requester.OnTradeupdate += TradeUpdate;
            SetActualTrade();
            ClientAuction.IsOpenMain = true;
            RaiseMaxBetCommand = new RelayCommand(RaiseMaxBet);
        }

        //private Account actualAccount;
        private Trade actualTrade;
        private Product actualProduct;
        private string timer;
        //private MainWindow mainWindow;

        //public Account ActualAccount { get => actualAccount; set => actualAccount = value; }
        public Trade ActualTrade { get => actualTrade; set => actualTrade = value; }
        public Product ActualProduct { get => actualProduct; set => actualProduct = value; }

        private float topPriceF;


        public string TopPrice => TopPriceF + "$";

        public string Timer
        {
            get => timer;
            set
            {
                timer = value;
                RaisePropertyChanged("Timer");
            }
        }

        public float TopPriceF
        {
            get => topPriceF;
            set
            {
                topPriceF = value;
                RaisePropertyChanged("TopPrice");
            }
        }

        public void TimerSetter(string response)
        {
            if (response != null)
            {
                Dispatcher.CurrentDispatcher.Invoke(() =>
                {
                    if (!"STOP".Equals(response))
                    {
                        Timer = response;
                    }
                    else
                    {
                        MainWindow.FrameContent = new AfterTrade();
                    }
                });
            }
        }

        public void TradeUpdate(string response)
        {
            actualTrade = ClientAuction.DeserializeFromString<Trade>(response);
            Dispatcher.CurrentDispatcher.Invoke(() =>
            {
                TopPriceF = ActualTrade.MaxBet;
            });
        }

        RequestMethods methods = new RequestMethods();

        private async void SetActualTrade()
        {
            try
            {
                actualTrade = TradeMenuViewModel.SelectedTrade;
                Requester.CreateRequest(methods.GetActualTimer());
                string stringResponse = await Requester.WaitResponseAsync<string>();
                if (!"STOP".Equals(stringResponse))
                {
                    Timer = stringResponse;
                }
                else
                {
                    Timer = stringResponse;
                    MainWindow.FrameContent = new AfterTrade();
                }
                Requester.CreateRequest(methods.GetActualProduct(), actualTrade.ProductId);
                actualProduct = await Requester.WaitResponseAsync<Product>();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка");
            }
        }

        public delegate void MethodContainer(string response);
        public static event MethodContainer OnStatusBarLocalTradeUpdate;

        public ICommand RaiseMaxBetCommand { get; private set; }


        private void RaiseMaxBet(object sender, RoutedEventArgs e)
        {
            Comma
        }

        private async void RaiseMaxBet_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string value = sender.ToString();
                if ("X2".Equals(value))
                {
                    actualTrade.MaxBet *= 2;
                }
                else
                {
                    actualTrade.MaxBet += int.Parse(sender.ToString());
                }
                TopPriceF = actualTrade.MaxBet;
                OnStatusBarLocalTradeUpdate?.Invoke(actualTrade.MaxBet + "$");
                RequestMethods methods = new RequestMethods();
                Requester.CreateRequest(methods.RaiseMaxBet(), value);
                bool stringResponse = await Requester.WaitResponseAsync<bool>();
                if (stringResponse)
                {
                    MessageBox.Show("Ставка сделана", "Уведомление");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка");
            }
        }

        public void Dispose()
        {
            Requester.OnTimer -= TimerSetter;
            Requester.OnTradeupdate -= TradeUpdate;
        }
    }
}

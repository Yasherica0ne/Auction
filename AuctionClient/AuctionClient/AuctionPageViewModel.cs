using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
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
            methods = RequestMethods.GetRequestMethods();
            Requester.OnTimer += TimerSetter;
            Requester.OnTradeupdate += TradeUpdate;
            SetActualTrade();
            ClientAuction.IsOpenMain = true;
        }

        private string timer;

        public Trade ActualTrade { get; set; }
        public Product ActualProduct { get; set; }

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
            get => ActualTrade.MaxBet;
            set
            {
                ActualTrade.MaxBet = value;
                RaisePropertyChanged("TopPrice");
            }
        }

        public void TimerSetter(string response)
        {
            try
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
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public void TradeUpdate(string response)
        {
            ActualTrade = ClientAuction.DeserializeFromString<Trade>(response);
            Dispatcher.CurrentDispatcher.Invoke(() =>
            {
                TopPriceF = ActualTrade.MaxBet;
            });
        }

        RequestMethods methods;

        private void SetActualTrade()
        {
            try
            {
                ActualTrade = TradeMenuViewModel.SelectedTrade;
                string stringResponse = methods.GetActualTimer(ActualTrade.TradeId);
                if (!"STOP".Equals(stringResponse))
                {
                    Timer = stringResponse;
                }
                else
                {
                    Timer = stringResponse;
                    MainWindow.FrameContent = new AfterTrade();
                }
                ActualProduct = methods.GetActualProduct(ActualTrade.ProductId);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка");
            }
        }

        public delegate void MethodContainer(string response);
        public static event MethodContainer OnStatusBarLocalTradeUpdate;


        private async void RaiseMaxBet(Object sender)
        {
            try
            {
                string value = sender.ToString();
                if ("X2".Equals(value))
                {
                    ActualTrade.MaxBet *= 2;
                }
                else
                {
                    ActualTrade.MaxBet += (int)sender;
                }
                TopPriceF = ActualTrade.MaxBet;
                OnStatusBarLocalTradeUpdate?.Invoke(ActualTrade.MaxBet + "$");
                bool stringResponse = await methods.RaiseMaxBetAsync(value);
                if (stringResponse)
                {
                    MessageBox.Show("Ставка сделана", "Уведомление");
                }
                else
                {
                    MessageBox.Show("Ставка не сделана", "Уведомление");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка");
            }
        }

        private RelayCommand<Object> raiseMaxBetCommand;
        public RelayCommand<Object> RaiseMaxBetCommand
        {
            get
            {
                return raiseMaxBetCommand ?? (raiseMaxBetCommand = new RelayCommand<Object>(RaiseMaxBet));
            }
        }

        private RelayCommand quitButtonCommand;
        public RelayCommand QuitButtonCommand
        {
            get
            {
                return quitButtonCommand ?? (quitButtonCommand = new RelayCommand(QuitButtonClick));
            }
        }

        public void QuitButtonClick()
        {
            methods.LeaveTrade();
            MainWindow.FrameContent = new TradesMenu();
        }

        public void Dispose()
        {
            Requester.OnTimer -= TimerSetter;
            Requester.OnTradeupdate -= TradeUpdate;
        }
    }
}

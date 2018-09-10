using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AuctionClient
{
    class TradeMenuViewModel : ViewModelBase
    {
        private ObservableCollection<Trade> tradesList;
        private static RequestMethods methods = RequestMethods.GetRequestMethods();
        public ObservableCollection<Trade> TradesList
        {
            get => tradesList;
            set
            {
                tradesList = value;
                RaisePropertyChanged("TradesList");
            }
        }

        public static event PropertyChangedEventHandler StaticPropertyChanged;

        private int listViewWidth;

        private static Trade selectedTrade;

        public int WrapPanelWidth => listViewWidth - 25;

        public int ListViewWidth
        {
            get => listViewWidth;
            set
            {
                listViewWidth = value;
                RaisePropertyChanged("ListViewWidth");
                RaisePropertyChanged("WrapPanelWidth");
            }
        }

        private static void SetSelectedProduct()
        {
            SelectedProduct = methods.GetNextProduct(selectedTrade.ProductId);
        }

        public static Trade SelectedTrade
        {
            get => selectedTrade;
            set
            {
                selectedTrade = value;
                SetSelectedProduct();
                OnStaticPropertyChanged("SelectedTrade");
                OnStaticPropertyChanged("SelectedProduct");
            }   
        }


        private static void OnStaticPropertyChanged(string v)
        {
            StaticPropertyChanged?.Invoke(new object(), new PropertyChangedEventArgs(v));
        }

        private RelayCommand enterToTradeCommand;

        public RelayCommand EnterToTradeCommand
        {
            get
            {
                return enterToTradeCommand ?? (enterToTradeCommand = new RelayCommand(EnterToTrade));
            }
        }

        public static Product SelectedProduct { get; set; }

        private void EnterToTrade()
        {
            try
            {
                bool isEntered = methods.EnterToTrade(SelectedTrade.TradeId);
                if (isEntered)
                {
                    MainWindow.FrameContent = new AuctionPage();
                    return;
                }
                else
                {
                    MainWindow.FrameContent = new AfterTrade();
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public TradeMenuViewModel()
        {
            //listViewWidth
            try
            {
                List<Trade> list = methods.GetActualTrades();
                TradesList = new ObservableCollection<Trade>(list);
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

    }
}

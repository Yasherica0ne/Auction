using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuctionClient
{
    class TradeMenuViewModel : BaseViewModel
    {
        public ObservableCollection<Trade> TradesList { get; set; }

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
                OnPropertyChanged("ListViewWidth");
                OnPropertyChanged("WrapPanelWidth");
            }
        }
        private static Product selectedProduct;

        private static async void SetSelectedProduct()
        {
            RequestMethods methods = new RequestMethods();
            Requester.CreateRequest(methods.GetNextProduct(), selectedTrade.ProductId);
            selectedProduct = await Requester.WaitResponseAsync<Product>();
        }

        public static Trade SelectedTrade
        {
            get => selectedTrade;
            set
            {
                selectedTrade = value;
                SetSelectedProduct();
                OnStaticPropertyChanged("SelectedTrade");
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
                return enterToTradeCommand ?? (enterToTradeCommand = new RelayCommand(obj => EnterToTrade()));
            }
        }

        public static Product SelectedProduct { get => selectedProduct; set => selectedProduct = value; }

        private void EnterToTrade()
        {
            RequestMethods methods = new RequestMethods();
            Requester.CreateRequest(methods.EnterToTrade(), SelectedTrade.TradeId);
            bool isEntered = Requester.WaitResponse<bool>();
            if (isEntered)
            {
                return;
            }
            else
            {
                MainWindow.FrameContent = new AfterTrade();
            }
        }

        public TradeMenuViewModel()
        {
            //listViewWidth
            RequestMethods methods = new RequestMethods();
            Requester.CreateRequest(methods.GetActualTrades());
            List<Trade> list = Requester.WaitResponse<List<Trade>>();
            TradesList = new ObservableCollection<Trade>(list);
        }

    }
}

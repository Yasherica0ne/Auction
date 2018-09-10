using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonServiceLocator;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using System.Windows;

namespace AuctionClient
{
    class AfterTradeViewModel : ViewModelBase
    {
        public AfterTradeViewModel()
        {
            nextTrade = TradeMenuViewModel.SelectedTrade;
        }

        private Trade nextTrade;

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                RequestMethods methods = RequestMethods.GetRequestMethods();
                Product nextProduct = await methods.GetNextProductAsync(nextTrade.ProductId);
                ProductVisual visual = new ProductVisual();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}

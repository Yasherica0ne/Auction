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
                RequestMethods methods = new RequestMethods();
                //Requester.CreateRequest(methods.GetNextTrade(), id);
                //Trade nextTrade = await Requester.WaitResponseAsync<Trade>();
                //if (nextTrade.TradeId == -1)
                //{
                //    TextBlock errortb = new TextBlock
                //    {
                //        Text = "Отсуствует",
                //        FontSize = 24,
                //        HorizontalAlignment = HorizontalAlignment.Center
                //    };
                //    BeforeTradeFrame.Content = errortb;
                //}
                //else
                //{
                Requester.CreateRequest(methods.GetNextProduct(), nextTrade.ProductId);
                Product nextProduct = await Requester.WaitResponseAsync<Product>();
                ProductVisual visual = new ProductVisual();
                //BeforeTradeFrame.Content = visual;
                //await visual.ChangeProduct(nextProduct);
                //}
                //Requester.CreateRequest(methods.GetActualTrade());
                //Trade trade = await Requester.WaitResponseAsync<Trade>();
                //if (trade.TradeId == -1)
                //{
                //    ProductName.Text = "Отсуствует";
                //    MaxBetAccountTB.Visibility = Visibility.Collapsed;
                //    MaxBetTB.Visibility = Visibility.Collapsed;
                //    return;
                //}
                //MaxBetTB.Text = trade.MaxBet + "$";
                //Requester.CreateRequest(methods.GetActualProduct());
                //Product product = await Requester.WaitResponseAsync<Product>();
                //ProductName.Text = product.Name;
                //Requester.CreateRequest(methods.GetWinnerName());
                //MaxBetAccountTB.Text = await Requester.WaitResponseAsync<string>();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}

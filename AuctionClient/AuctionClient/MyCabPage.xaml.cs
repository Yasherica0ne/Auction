using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AuctionClient
{
    /// <summary>
    /// Логика взаимодействия для MyCabPage.xaml
    /// </summary>
    public partial class MyCabPage : Page
    {
        Account account;
        RequestMethods methods = new RequestMethods();

        public MyCabPage(Account account)
        {
            this.account = account;
            InitializeComponent();
        }

        private void AddProdWindow_Click(object sender, RoutedEventArgs e)
        {
            AddProductWindow addProductWindow = new AddProductWindow(account);
            addProductWindow.Show();
        }

        private async void ExpandPurchase_Expanded(object sender, RoutedEventArgs e)
        {
            try
            {
                Requester.CreateRequest(methods.GetPurchaseList(), account.AccountId);
                List<Product> prodList = await Requester.WaitResponseAsync<List<Product>>();
                if (prodList.Count == 0) return;
                ProductList.ItemsSource = prodList;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка");
            }

        }

        private async void ExpandSales_Expanded(object sender, RoutedEventArgs e)
        {
            try
            {
                Requester.CreateRequest(methods.GetSalesList(), account.AccountId);
                List<Product> prodList = await Requester.WaitResponseAsync<List<Product>>();
                if (prodList.Count == 0) return;
                ProductList.ItemsSource = prodList;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка");
            }
        }

        private void SaleList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((ListBox)sender).SelectedItem == null) return;
            Product product = (Product)((ListBox)sender).SelectedItem;
            ((ProductVisualViewModel)((ProductVisual)ProductFrame.Content).DataContext).SelectedProduct = product;
            //await ((ProductVisual)ProductFrame.Content).ChangeProduct(product);
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ProductFrame.Content = new ProductVisual();
            ProductList.DisplayMemberPath = "Name";
        }
    }
}

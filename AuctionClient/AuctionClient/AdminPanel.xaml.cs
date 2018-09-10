using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// Логика взаимодействия для AdminPanel.xaml
    /// </summary>
    public partial class AdminPanel : Page
    {
        RequestMethods methods;

        private async void SetProductList()
        {
            try
            {
                ProductList.ItemsSource = await methods.GetNewProductListAsync();
                ProductList.DisplayMemberPath = "Name";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка");
            }
        }

        public AdminPanel()
        {
            methods = RequestMethods.GetRequestMethods();
            InitializeComponent();
            SetProductList();
        }


        private void ProductList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ProductList.SelectedItem == null) return;
            Product product = (Product)ProductList.SelectedItem;
            ((ProductVisualViewModel)((ProductVisual)ProductFrame.Content).DataContext).SelectedProduct = product;
            //await ((ProductVisual)ProductFrame.Content).ChangeProduct(product);
        }

        private async void Approve_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ProductList.SelectedItem == null) return;
                bool isTrueResponse = await methods.ApproveNewProductAsync(((Product)ProductList.SelectedItem).ProductID);
                if (isTrueResponse) MessageBox.Show("Товар одобрен", "Уведомление");
                else MessageBox.Show("Ошибка выполнения", "Уведомление");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка");
            }
        }

        private async void Cancell_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ProductList.SelectedItem == null) return;
                bool isTrueResponse = await methods.CancelNewProductAsync(((Product)ProductList.SelectedItem).ProductID);
                if (isTrueResponse) MessageBox.Show("Товар удалён", "Уведомление");
                else MessageBox.Show("Ошибка выполнения", "Уведомление");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка");
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ProductFrame.Content = new ProductVisual();
        }
    }
}

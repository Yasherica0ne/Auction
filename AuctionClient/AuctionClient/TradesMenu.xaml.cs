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
    /// Логика взаимодействия для TradesMenu.xaml
    /// </summary>
    public partial class TradesMenu : Page
    {
        public TradesMenu()
        {
            InitializeComponent();
            DataContext = new TradeMenuViewModel();
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ((TradeMenuViewModel)DataContext).ListViewWidth = (int)(Window.ActualWidth - RightColumn.Width.Value);
        }

        //private void TradeList_SizeChanged(object sender, SizeChangedEventArgs e)
        //{
        //    //TradeList.BeginInit();
        //    WrapPanel k = (WrapPanel)TradeList.ItemsPanel.FindName("WPanel", new ListView());
        //    k.Width = TradeList.Width;
        //}
    }
}

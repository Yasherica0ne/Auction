using System.Windows.Controls;

namespace AuctionClient
{
    /// <summary>
    /// Логика взаимодействия для AuctionPage.xaml
    /// </summary>
    public partial class AuctionPage : Page
    {
        public AuctionPage()
        {
            InitializeComponent();
            DataContext = new AuctionPageViewModel();
        }
    }
}

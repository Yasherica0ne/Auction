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
    /// Логика взаимодействия для RaiseMaxBetButton.xaml
    /// </summary>
    public partial class RaiseMaxBetButton : Button
    {
        public RaiseMaxBetButton()
        {
            InitializeComponent();
        }

        public enum Bets
        {
            Five, Ten, Fifty, Hundreed, Double 
        }

        private Bets bet;

        public Bets Bet { get => bet; set => bet = value; }
    }
}

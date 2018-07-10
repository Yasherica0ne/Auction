using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Threading;
using System.ComponentModel;
using System.Net.Sockets;
using System.Windows.Input;

namespace AuctionClient
{
    /// <summary>
    /// Логика взаимодействия для Window1.xaml
    /// </summary>
    public partial class EnteringWindow : Window
    {
        public EnteringWindow()
        {
            InitializeComponent();
            DataContext = new EnteringWindowViewModel(this);
        }
    }
}

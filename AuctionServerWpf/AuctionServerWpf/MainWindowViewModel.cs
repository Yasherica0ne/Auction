﻿using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuctionServerWpf
{
    class MainWindowViewModel : ViewModelBase
    {

        public ObservableCollection<Trade> TradesList { get; set; }

        public MainWindowViewModel()
        {
            using (AuctionContext db = new AuctionContext())
            {
                TradesList = new ObservableCollection<Trade>(db.Trades);
            }
        }
    }
}

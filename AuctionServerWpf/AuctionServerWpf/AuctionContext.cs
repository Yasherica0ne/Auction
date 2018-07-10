using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuctionServerWpf
{
    public class AuctionContext : DbContext
    {
        public AuctionContext() : base("DbConnection") { }
        private DbSet<Account> accounts;
        private DbSet<Product> products;
        private DbSet<Trade> trades;

        public DbSet<Account> Accounts { get => accounts; set => accounts = value; }
        public DbSet<Product> Products { get => products; set => products = value; }
        public DbSet<Trade> Trades { get => trades; set => trades = value; }
    }

}

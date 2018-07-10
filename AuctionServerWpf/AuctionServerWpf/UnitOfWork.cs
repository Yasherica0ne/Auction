using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuctionServerWpf
{
    class UnitOfWork : IDisposable
    {
        private AuctionContext db = new AuctionContext();
        private EFGenericRepository<Account> accountRepository;
        private EFGenericRepository<Trade> tradeRepository;
        private EFGenericRepository<Product> productRepository;
        public EFGenericRepository<Account> Accounts
        {
            get
            {
                if (accountRepository == null) accountRepository = new EFGenericRepository<Account>(db);
                return accountRepository;
            }
        }

        public EFGenericRepository<Trade> Trades
        {
            get
            {
                if (tradeRepository == null) tradeRepository = new EFGenericRepository<Trade>(db);
                return tradeRepository;
            }
        }

        public EFGenericRepository<Product> Products
        {
            get
            {
                if (productRepository == null) productRepository = new EFGenericRepository<Product>(db);
                return productRepository;
            }
        }

        public void Save()
        {
            db.SaveChanges();
        }

        public void Dispose()
        {
            db.Dispose();
        }
    }
}

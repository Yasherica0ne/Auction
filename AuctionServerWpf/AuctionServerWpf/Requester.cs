using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AuctionServerWpf
{
    public class Requester : RequestMethods
    {
        string parametr;
        ClientObject clientObject;
        RequestMethods methods = new RequestMethods();
        private static UnitOfWork work = new UnitOfWork();

        internal static UnitOfWork Work { get => work; set => work = value; }

        public Requester(string _parametr, ClientObject _clientObject)
        {
            parametr = _parametr;
            clientObject = _clientObject;
        }

        public override string SetAccount()
        {
            clientObject.Account = AuctionServer.DeserializeFromString<Account>(parametr);
            return null;
        }

        public static string CreateResponse<T>(string reciever, T obj)
        {
            string message = AuctionServer.SerializeToString(obj);
            return AuctionServer.SerializeToString(new Response(reciever, message)) + "%";
        }

        public static string CreateResponse(string reciever)
        {
            return CreateResponse(reciever, "");
        }

        public override string GetSalesList()
        {
            int ident = AuctionServer.DeserializeFromString<int>(parametr);
            List<Product> products = work.Products.Get(n => n.OwnerId == ident).ToList();
            clientObject.Server.SingleMessage(CreateResponse(methods.SendDefaultResponse(), products), clientObject.Id);
            return null;
        }

        public override string FindUser()
        {
            string login = AuctionServer.DeserializeFromString<string>(parametr);
            List<Account> accounts = work.Accounts.Get(n => n.Login.Equals(login)).ToList();
            clientObject.Server.SingleMessage(CreateResponse(methods.SendDefaultResponse(), accounts), clientObject.Id);
            return null;
        }

        private byte[] CreateImage(string filename)
        {
            return File.ReadAllBytes(filename);
        }

        public override string GetNewProductList()
        {
            int count = work.Products.GetCount();
            if (count > 10)
            {
                count = 10;
            }
            List<Product> list = work.Products.Get(n => !n.IsChecked).Take(count).ToList();
            clientObject.Server.SingleMessage(CreateResponse(methods.SendDefaultResponse(), list), clientObject.Id);
            return null;
        }

        public override string GetActualTrade()
        {
            clientObject.Server.SingleMessage(CreateResponse(methods.GetActualTrade(), AuctionServer.ActualTrade), clientObject.Id);
            //clientObject.IsOpen = true;
            return null;
        }

        public override string GetActualProduct()
        {
            Product product = work.Products.Get(n => n.ProductID.Equals(AuctionServer.ActualTrade.ProductId)).FirstOrDefault();
            clientObject.Server.SingleMessage(CreateResponse(methods.GetActualProduct(), product), clientObject.Id);
            return null;
        }

        public override string ApproveNewProduct()
        {
            int ident = AuctionServer.DeserializeFromString<int>(parametr);
            var product = work.Products.FindById(ident);
            product.IsChecked = true;
            Trade trade = new Trade(ident);
            work.Trades.Add(trade);
            work.Save();
            clientObject.Server.SingleMessage(CreateResponse(methods.SendDefaultResponse(), true), clientObject.Id);
            return null;
        }

        public override string GetPurchaseList()
        {
            int ident = AuctionServer.DeserializeFromString<int>(parametr);
            List<Trade> buffer = work.Trades.Get(n => n.MaxBetAccountId == ident).ToList();
            List<Product> prodList = new List<Product>();
            foreach (Trade trade in buffer)
            {
                prodList.Add(work.Products.Get(n => n.ProductID == trade.ProductId).FirstOrDefault());
            }
            clientObject.Server.SingleMessage(CreateResponse(methods.SendDefaultResponse(), prodList), clientObject.Id);
            return null;
        }

        public override string RaiseMaxBet()
        {
            if (parametr.Equals("X2"))
            {
                AuctionServer.ActualTrade.MaxBet *= 2;
            }
            else
            {
                AuctionServer.ActualTrade.MaxBet += AuctionServer.DeserializeFromString<float>(parametr);
            }
            AuctionServer.ActualTrade.MaxBetAccountId = clientObject.Account.AccountId;
            AuctionServer.TimeWithoutBets = 0;
            clientObject.Server.BroadcastMessage(CreateResponse(methods.SetTimer(), AuctionServer.TimeWithoutBets), clientObject.Id);
            clientObject.Server.BroadcastMessage(CreateResponse(methods.UpdateTrade(), AuctionServer.ActualTrade), clientObject.Id);
            clientObject.Server.SingleMessage(CreateResponse(methods.SendDefaultResponse(), true), clientObject.Id);
            return null;
        }

        public override string Registration()
        {
            Account account = AuctionServer.DeserializeFromString<Account>(parametr);
            IEnumerable<Account> accounts = work.Accounts.Get(n => n.Login.Equals(account.Login));
            if (accounts.Count() != 0)
                clientObject.Server.SingleMessage(CreateResponse(methods.SendDefaultResponse(), false), clientObject.Id);
            else
            {
                work.Accounts.Add(account);
                work.Save();
                clientObject.Server.SingleMessage(CreateResponse(methods.SendDefaultResponse(), true), clientObject.Id);
            }
            return null;
        }
        public override string GetActualTimer()
        {
            clientObject.Server.SingleMessage(CreateResponse(methods.SendDefaultResponse(), AuctionServer.TimeWithoutBets.ToString()), clientObject.Id);
            return null;
        }

        public override string CancelNewProduct()
        {
            int ident = AuctionServer.DeserializeFromString<int>(parametr);
            var product = work.Products.FindById(ident);
            work.Products.AttachItem(product);
            work.Products.Remove(product);
            work.Save();
            clientObject.Server.SingleMessage(CreateResponse(methods.SendDefaultResponse(), true), clientObject.Id);
            return null;
        }

        public override string AddProduct()
        {
            Product product = AuctionServer.DeserializeFromString<Product>(parametr);
            work.Products.Add(product);
            work.Save();
            clientObject.Server.SingleMessage(CreateResponse(methods.SendDefaultResponse(), product.ProductID), clientObject.Id);
            return null;
        }

        public override string SetProductPhoto()
        {
            using (AuctionContext db = new AuctionContext())
            {
                int id = AuctionServer.DeserializeFromString<int>(parametr);
                var product = db.Products.Where(n => n.ProductID == id).FirstOrDefault();
                string[] ext = product.ImgSrc.Split('.');
                string path = "Photos/" + id + "." + ext.Last();
                while (!clientObject.IsFullImage) ;
                File.WriteAllBytes(path, clientObject.ImageBytes);
                clientObject.IsFullImage = false;
                product.ImgSrc = path;
                db.SaveChanges();
                string message = CreateResponse(methods.SendDefaultResponse(), true);
                clientObject.Server.SingleMessage(message, clientObject.Id);
            }
            return null;
        }

        private static byte[] byteBuffer;

        public override string GetPhoto()
        {
            clientObject.IsOpen = false;
            int id = AuctionServer.DeserializeFromString<int>(parametr);
            Product product = work.Products.Get(n => n.ProductID == id).First();
            byteBuffer = CreateImage(product.ImgSrc);
            string message = CreateResponse(methods.GetPhoto(), byteBuffer.Length);
            clientObject.Server.SingleMessage(message, clientObject.Id);
            return null;
        }

        public override string GetPhotoSt2()
        {
            clientObject.Server.SendImageInBytes(byteBuffer, clientObject.Id);
            clientObject.IsOpen = true;
            return null;
        }

        public override string PreSetProductPhoto()
        {
            clientObject.ImageBytesCount = AuctionServer.DeserializeFromString<int>(parametr);
            clientObject.IsRecievingImage = true;
            clientObject.ImageBytes = null;
            return null;
        }

        public override string CheckAuctionStatus()
        {
            int id = AuctionServer.DeserializeFromString<int>(parametr);
            MainWindow.Main.Dispatcher.Invoke(() =>
            {
                clientObject.Server.SingleMessage(CreateResponse(methods.SendDefaultResponse(), MainWindow.Main.IsTradeRuningNow), clientObject.Id);
            });
            return null;
        }

        public override string GetNextProduct()
        {
            int id = AuctionServer.DeserializeFromString<int>(parametr);
            using (AuctionContext db = new AuctionContext())
            {
                Product product = work.Products.Get(n => n.ProductID == id).FirstOrDefault();
                clientObject.Server.SingleMessage(CreateResponse(methods.SendDefaultResponse(), product), clientObject.Id);
            }
            return null;
        }

        public override string GetNextTrade()
        {
            int id = AuctionServer.DeserializeFromString<int>(parametr);
            using (AuctionContext db = new AuctionContext())
            {
                Trade trade = db.Trades.Where(n => n.TradeId == id).First();
                clientObject.Server.SingleMessage(CreateResponse(methods.SendDefaultResponse(), trade), clientObject.Id);
            }
            return null;
        }

        public override string GetWinnerName()
        {
            Account account = work.Accounts.Get(n => n.AccountId == AuctionServer.ActualTrade.MaxBetAccountId).FirstOrDefault();
            clientObject.Server.SingleMessage(CreateResponse(methods.SendDefaultResponse(), account.Login), clientObject.Id);
            return null;
        }

        public override string GetEmail()
        {
            int id = AuctionServer.DeserializeFromString<int>(parametr);
            string email = work.Accounts.Get(n => n.AccountId == id).FirstOrDefault().Email;
            clientObject.Server.SingleMessage(CreateResponse(methods.SendDefaultResponse(), email), clientObject.Id);
            return null;
        }

        public override string EnterToTrade()
        {
            int id = AuctionServer.DeserializeFromString<int>(parametr);
            bool IsEntered = Auction.TradeEnter(clientObject, id);
            clientObject.Server.SingleMessage(CreateResponse(methods.SendDefaultResponse(), IsEntered), clientObject.Id);
            return null;
        }

        public override string GetProductById()
        {
            using (AuctionContext db = new AuctionContext())
            {
                int id = AuctionServer.DeserializeFromString<int>(parametr);
                Product product = db.Products.Find(id);
                clientObject.Server.SingleMessage(CreateResponse(reciever: methods.SendDefaultResponse(), obj: product), clientObject.Id);
            }
            return null;
        }

        public override string GetActualTrades()
        {
            using (AuctionContext db = new AuctionContext())
            {
                List<Trade> trades = db.Trades.Where(n => n.MaxBetAccountId == 0).ToList();
                clientObject.Server.SingleMessage(CreateResponse(methods.SendDefaultResponse(), trades), clientObject.Id);
            }
            return null;
        }
    }
}

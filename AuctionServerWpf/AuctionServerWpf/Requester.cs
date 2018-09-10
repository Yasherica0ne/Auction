using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace AuctionServerWpf
{
    public class Requester : RequestMethods
    {
        string parametr;
        ClientObject clientObject;
        RequestMethods methods = new RequestMethods();

        internal static UnitOfWork Work { get; set; } = new UnitOfWork();

        private T GetParametr<T>()
        {
            return AuctionServer.DeserializeFromString<T>(parametr);
        }

        private void SendSingleMessage<T>(string methodName, T parametr)
        {
            clientObject.Server.SingleMessage(CreateResponse(methodName, parametr), clientObject.Id);
        }

        public Requester(string _parametr, ClientObject _clientObject)
        {
            parametr = _parametr;
            clientObject = _clientObject;
        }

        public override string SetAccount()
        {
            try
            {
                clientObject.Account = GetParametr<Account>();
                SendSingleMessage(methods.SendDefaultResponse(), true);
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
                SendSingleMessage(methods.SendDefaultResponse(), false);
            }
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
            int ident = GetParametr<int>();
            List<Product> products = Work.Products.Get(n => n.OwnerId == ident).ToList();
            SendSingleMessage(methods.SendDefaultResponse(), products);
            return null;
        }

        public override string FindUser()
        {
            string login = GetParametr<string>();
            List<Account> accounts = Work.Accounts.Get(n => n.Login.Equals(login)).ToList();
            if(accounts.Count > 0)
            SendSingleMessage(methods.SendDefaultResponse(), accounts.First());
            return null;
        }

        private byte[] CreateImage(string filename)
        {
            return File.ReadAllBytes(filename);
        }

        public override string GetNewProductList()
        {
            int count = Work.Products.GetCount();
            if (count > 10)
            {
                count = 10;
            }
            List<Product> list = Work.Products.Get(n => !n.IsChecked).Take(count).ToList();
            SendSingleMessage(methods.SendDefaultResponse(), list);
            return null;
        }

        public override string GetActualTrade()
        {
            //SendSingleMessage(methods.GetActualTrade(), )
            //clientObject.Server.SingleMessage(CreateResponse(methods.GetActualTrade(), AuctionServer.ActualTrade), clientObject.Id);
            //clientObject.IsOpen = true;
            return null;
        }

        public override string GetActualProduct()
        {
            int ident = GetParametr<int>();
            Product product = Work.Products.Get(n => n.ProductID == ident).FirstOrDefault();
            SendSingleMessage(methods.GetActualProduct(), product);
            return null;
        }

        public override string ApproveNewProduct()
        {
            int ident = GetParametr<int>();
            var product = Work.Products.FindById(ident);
            product.IsChecked = true;
            Trade trade = new Trade(ident);
            Work.Trades.Add(trade);
            Work.Save();
            SendSingleMessage(methods.SendDefaultResponse(), true);
            return null;
        }

        public override string GetPurchaseList()
        {
            int ident = GetParametr<int>();
            List<Trade> buffer = Work.Trades.Get(n => n.MaxBetAccountId == ident).ToList();
            List<Product> prodList = new List<Product>();
            foreach (Trade trade in buffer)
            {
                prodList.Add(Work.Products.Get(n => n.ProductID == trade.ProductId).FirstOrDefault());
            }
            SendSingleMessage(methods.SendDefaultResponse(), prodList);
            return null;
        }

        public override string RaiseMaxBet()
        {
            Auction auction = clientObject.Auction;
            if (parametr.Equals("X2"))
            {
                auction.ActualTradeMaxBet *= 2;
            }
            else
            {
                auction.ActualTradeMaxBet += AuctionServer.DeserializeFromString<float>(parametr);
            }
            auction.ActualTrade.MaxBetAccountId = clientObject.Account.AccountId;
            auction.TimeWithoutBets = 0;
            clientObject.Server.BroadcastMessage(CreateResponse(methods.SetTimer(), auction.TimeWithoutBets), clientObject.Id);
            clientObject.Server.BroadcastMessage(CreateResponse(methods.UpdateTrade(), auction.ActualTrade), clientObject.Id);
            SendSingleMessage(methods.SendDefaultResponse(), true);
            return null;
        }

        public override string Registration()
        {
            Account account = GetParametr<Account>();
            IEnumerable<Account> accounts = Work.Accounts.Get(n => n.Login.Equals(account.Login));
            if (accounts.Count() != 0)
            {
                SendSingleMessage(methods.SendDefaultResponse(), false);
            }
            else
            {
                Work.Accounts.Add(account);
                Work.Save();
                SendSingleMessage(methods.SendDefaultResponse(), true);
            }
            return null;
        }
        public override string GetActualTimer()
        {
            int ident = GetParametr<int>();
            SendSingleMessage(methods.SendDefaultResponse(), Auction.GetTrade(ident).TimeWithoutBets);
            return null;
        }

        public override string CancelNewProduct()
        {
            int ident = GetParametr<int>();
            var product = Work.Products.FindById(ident);
            Work.Products.AttachItem(product);
            Work.Products.Remove(product);
            Work.Save();
            SendSingleMessage(methods.SendDefaultResponse(), true);
            return null;
        }

        public override string AddProduct()
        {
            Product product = GetParametr<Product>();
            Work.Products.Add(product);
            Work.Save();
            SendSingleMessage(methods.SendDefaultResponse(), product.ProductID);
            return null;
        }

        public override string SetProductPhoto()
        {
            using (AuctionContext db = new AuctionContext())
            {
                int id = GetParametr<int>();
                var product = db.Products.Where(n => n.ProductID == id).FirstOrDefault();
                string[] ext = product.ImgSrc.Split('.');
                string path = "Photos/" + id + "." + ext.Last();
                while (!clientObject.IsFullImage) ;
                File.WriteAllBytes(path, clientObject.ImageBytes);
                clientObject.IsFullImage = false;
                product.ImgSrc = path;
                db.SaveChanges();
                SendSingleMessage(methods.SendDefaultResponse(), true);
            }
            return null;
        }

        private static byte[] byteBuffer;

        public override string GetPhoto()
        {
            clientObject.IsOpen = false;
            int id = GetParametr<int>();
            Product product = Work.Products.Get(n => n.ProductID == id).First();
            byteBuffer = CreateImage(product.ImgSrc);
            SendSingleMessage(methods.GetPhoto(), byteBuffer.Length);
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
            clientObject.ImageBytesCount = GetParametr<int>();
            clientObject.IsRecievingImage = true;
            clientObject.ImageBytes = null;
            return null;
        }

        public override string CheckAuctionStatus()
        {
            int id = GetParametr<int>();
            MainWindow.Main.Dispatcher.Invoke(() =>
            {
                SendSingleMessage(methods.SendDefaultResponse(), Auction.IsAuctionRuning(id));
            });
            return null;
        }

        public override string GetNextProduct()
        {
            int id = GetParametr<int>();
            using (AuctionContext db = new AuctionContext())
            {
                Product product = Work.Products.Get(n => n.ProductID == id).FirstOrDefault();
                SendSingleMessage(methods.SendDefaultResponse(), product);
            }
            return null;
        }

        public override string GetWinnerName()
        {
            int id = GetParametr<int>();
            Account account = Work.Accounts.Get(n => n.AccountId == id).FirstOrDefault();
            SendSingleMessage(methods.SendDefaultResponse(), account.Login);
            return null;
        }

        public override string GetEmail()
        {
            int id = GetParametr<int>();
            string email = Work.Accounts.Get(n => n.AccountId == id).FirstOrDefault().Email;
            SendSingleMessage(methods.SendDefaultResponse(), email);
            return null;
        }

        public override string EnterToTrade()
        {
            int id = GetParametr<int>();
            bool IsEntered = Auction.TradeEnter(clientObject, id);
            SendSingleMessage(methods.SendDefaultResponse(), IsEntered);
            return null;
        }

        public override string LeaveTrade()
        {
            clientObject.Auction.TradeLeave(clientObject);
            return null;
        }

        public override string GetProductById()
        {
            using (AuctionContext db = new AuctionContext())
            {
                int id = GetParametr<int>();
                Product product = db.Products.Find(id);
                SendSingleMessage(methods.SendDefaultResponse(), product);
            }
            return null;
        }

        public override string GetActualTrades()
        {
            using (AuctionContext db = new AuctionContext())
            {
                List<Trade> trades = db.Trades.Where(n => n.MaxBetAccountId == 0).ToList();
                SendSingleMessage(methods.SendDefaultResponse(), trades);
            }
            return null;
        }

        public override string DisconnectServer()
        {
            clientObject.Server.RemoveConnection(clientObject.Id);
            clientObject.Close();
            return null;
        }
    }
}

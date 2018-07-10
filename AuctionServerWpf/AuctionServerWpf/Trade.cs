using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows;
using System.Windows.Media.Imaging;

namespace AuctionServerWpf
{
    public class Trade
    {
        private int tradeID;
        private int productID;
        private int maxBetAccountID;
        private float maxBet;
        private DateTime tradeStartTime;
        private DateTime tradeFinishTime;

        public int TradeId { get => tradeID; set => tradeID = value; }
        public int ProductId { get => productID; set => productID = value; }
        public int MaxBetAccountId { get => maxBetAccountID; set => maxBetAccountID = value; }
        public float MaxBet { get => maxBet; set => maxBet = value; }
        public DateTime TradeStartTime { get => tradeStartTime; set => tradeStartTime = value; }
        public DateTime TradeFinishTime { get => tradeFinishTime; set => tradeFinishTime = value; }


        public Trade()
        {
            tradeID = -1;
            tradeStartTime = new DateTime();
            tradeFinishTime = new DateTime();
        }

        public Trade(int _productID)
        {
            using (AuctionContext db = new AuctionContext())
            {
                this.maxBet = db.Products.Where(n => n.ProductID == _productID).FirstOrDefault().Price;
            }
            this.productID = _productID;
            this.tradeStartTime = new DateTime();
            this.tradeFinishTime = new DateTime();
        }

        public void ChangeProperties(Trade trade)
        {
            this.maxBetAccountID = trade.maxBetAccountID;
            this.maxBet = trade.maxBet;
            this.tradeStartTime = trade.tradeStartTime;
            this.tradeFinishTime = trade.tradeFinishTime;
        }

        [ScriptIgnore]
        public string ProductName
        {
            get
            {
                using (AuctionContext db = new AuctionContext())
                {
                    return db.Products.Where(n => n.ProductID == ProductId).FirstOrDefault().Name;
                }
            }
        }

        public static BitmapSource BitmapToBitmapSource(Bitmap source)
        {
            return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                          source.GetHbitmap(),
                          IntPtr.Zero,
                          Int32Rect.Empty,
                          BitmapSizeOptions.FromEmptyOptions());
        }

        [ScriptIgnore]
        public BitmapSource ImageSource
        {
            get
            {
                string path = null;
                using (AuctionContext db = new AuctionContext())
                {
                    path = db.Products.Where(n => n.ProductID == ProductId).FirstOrDefault().ImgSrc;
                }
                Bitmap bitmap = (Bitmap)Image.FromFile(path, true);
                return BitmapToBitmapSource(bitmap);
            }
        }

    }
}

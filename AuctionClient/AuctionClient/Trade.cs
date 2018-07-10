using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AuctionClient
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
            tradeStartTime = new DateTime();
            tradeFinishTime = new DateTime();
        }

        public Trade(int _productID)
        {
            productID = _productID;
            tradeStartTime = new DateTime();
            tradeFinishTime = new DateTime();
        }

        [ScriptIgnore]
        public string ProductName
        {
            get
            {
                RequestMethods methods = new RequestMethods();
                Requester.CreateRequest(methods.GetProductById(), ProductId);
                return Requester.WaitResponse<Product>().Name;
            }
        }

        [ScriptIgnore]
        public BitmapSource ImageSource => LoadImage.ImageSource(productID);

    }
}

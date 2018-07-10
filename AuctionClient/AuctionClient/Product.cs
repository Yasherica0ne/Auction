using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows.Media.Imaging;

namespace AuctionClient
{
    public class Product
    {
        private int productID;
        private string imgSrc;
        private float price;
        private string name;
        private string description;
        private int ownerID;
        private bool isChecked;
        private bool isSold;

        public string ImgSrc { get => imgSrc; set => imgSrc = value; }
        public float Price { get => price; set => price = value; }
        public string Name { get => name; set => name = value; }
        public string Description { get => description; set => description = value; }
        public int ProductID { get => productID; set => productID = value; }
        public int OwnerId { get => ownerID; set => ownerID = value; }
        public bool IsChecked { get => isChecked; set => isChecked = value; }
        public bool IsSold { get => isSold; set => isSold = value; }

        public Product() { }

        public Product(string imgSrc, float price, string name, string description, int owner)
        {
            this.imgSrc = imgSrc;
            this.price = price;
            this.name = name;
            this.description = description;
            ownerID = owner;
            isChecked = false;
            isSold = false;
        }

        [ScriptIgnore]
        public BitmapSource ImageSource => LoadImage.ImageSource(productID);
    }
}

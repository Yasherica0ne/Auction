using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuctionServerWpf
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

        public Product()
        {
            productID = -1;
        }

        public Product(string _imgSrc, float _price, string _name, string _description, int _owner)
        {
            this.imgSrc = _imgSrc;
            this.price = _price;
            this.name = _name;
            this.description = _description;
            this.ownerID = _owner;
            this.isChecked = false;
            this.isSold = false;
        }
    }
}

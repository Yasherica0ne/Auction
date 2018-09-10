using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;

namespace AuctionClient
{
    class ProductVisualViewModel : ViewModelBase
    {
        public ProductVisualViewModel()
        {
        }

        private Product selectedProduct;

        public Product SelectedProduct
        {
            get => selectedProduct;
            set
            {
                selectedProduct = value;
                RaisePropertyChanged("SelectedProduct");
            }
        }

        public string EmailText
        {
            get
            {
                RequestMethods requestMethods = RequestMethods.GetRequestMethods();
                return requestMethods.GetEmail(selectedProduct.OwnerId);
            }
        }
    }
}
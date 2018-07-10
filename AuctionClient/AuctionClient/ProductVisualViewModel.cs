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
                RequestMethods requestMethods = new RequestMethods();
                Requester.CreateRequest(requestMethods.GetEmail(), selectedProduct.OwnerId);
                return Requester.WaitResponse<string>();
            }
        }


        //public async Task<bool> ChangeProduct(Product product)
        //{
        //    RequestMethods methods = new RequestMethods();
        //    ProductName.Text = product.Name;
        //    TopPrice.Text = product.Price + "$";
        //    Description.Text = product.Description;
        //    Requester.CreateRequest(methods.GetPhoto(), product.ProductID);
        //    await Requester.WaitResponseAsync<bool>();
        //    ProdImage.Stretch = Stretch.Uniform;
        //    ProdImage.Source = (BitmapSource)new ImageSourceConverter().ConvertFrom(ClientAuction.ImageBytes);
        //    ClientAuction.IsFullImage = false;
        //    Requester.CreateRequest(methods.GetEmail(), product.OwnerId);
        //    EmailText.Text = await Requester.WaitResponseAsync<string>();
        //    return true;
        //    }
        //}
    }
}
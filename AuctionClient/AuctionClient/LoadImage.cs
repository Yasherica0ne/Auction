using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AuctionClient
{
    public class LoadImage
    {
        public static BitmapSource ImageSource(int productID)
        {
            RequestMethods methods = RequestMethods.GetRequestMethods();
            byte[] bytes = methods.GetPhoto(productID);
            return (BitmapSource)new ImageSourceConverter().ConvertFrom(bytes);
        }
    }
}

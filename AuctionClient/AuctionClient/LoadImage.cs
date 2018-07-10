using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AuctionClient
{
    public class LoadImage
    {
        public static BitmapSource ImageSource(int productID)
        {
            RequestMethods methods = new RequestMethods();
            Requester.CreateRequest(methods.GetPhoto(), productID);
            Requester.WaitResponse<bool>();
            ClientAuction.IsFullImage = false;
            return (BitmapSource)new ImageSourceConverter().ConvertFrom(ClientAuction.ImageBytes);
        }
    }
}

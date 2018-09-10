using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Threading;

namespace AuctionClient
{
    class Requester
    {
        List<Response> responseList;
        public delegate void MethodContainer(string response);
        public delegate void ParametrlessContainer();

        public static event MethodContainer OnTimer;
        public static event MethodContainer OnTradeupdate;
        public static event MethodContainer OnStatusBarTradeUpdate;
        public static event MethodContainer OnStatusBarProductUpdate;
        private RequestMethods methods;

        private string GetFirstMessage()
        {
            return responseList.FirstOrDefault().Message;
        }

        public Requester(List<Response> responseList)
        {
            this.responseList = responseList;
            methods = RequestMethods.GetRequestMethods();
        }

        //public override string GetActualTrade()
        //{
        //    response = GetFirstMessage();
        //    PulseLocker();
        //    OnStatusBarTradeUpdate?.Invoke(GetFirstMessage());
        //    return null;
        //}

        public string GetActualProduct()
        {
            methods.Response = GetFirstMessage();
            methods.PulseLocker();
            OnStatusBarProductUpdate?.Invoke(GetFirstMessage());
            return null;
        }

        public string SetTimer()
        {
            OnTimer?.Invoke(GetFirstMessage());
            return null;
        }

        public string UpdateTrade()
        {
            OnTradeupdate?.Invoke(GetFirstMessage());
            return null;
        }

        public string GetPhoto()
        {
            try
            {
                string message = GetFirstMessage();
                ClientAuction.IsRecieveImage = true;
                methods.CreateRequest("GetPhotoSt2");
                ClientAuction.RecieveImage(ClientAuction.DeserializeFromString<int>(message));
                methods.Response = "true";
                methods.PulseLocker();
                return null;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return null;
            }
        }

        public string DisconnectServer()
        {
            MainWindow.Logout();
            return null;
        }

        public string SendDefaultResponse()
        {
            methods.Response = GetFirstMessage();
            methods.PulseLocker();
            return null;
        }
    }
}

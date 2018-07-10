using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Threading;

namespace AuctionClient
{
    class Requester : RequestMethods
    {
        List<Response> responseList;
        public delegate void MethodContainer(string response);
        public delegate void ParametrlessContainer();

        public static event MethodContainer OnTimer;
        public static event MethodContainer OnTradeupdate;
        public static event MethodContainer OnStatusBarTradeUpdate;
        public static event MethodContainer OnStatusBarProductUpdate;



        private static string response;

        public static string Response { get => response; set => response = value; }
        public static Timer Timer { get => timer; set => timer = value; }

        private static Timer timer = null;
        private static bool isError = false;

        private string GetFirstMessage()
        {
            return responseList.FirstOrDefault().Message;
        }

        private static void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            isError = true;
            timer.Dispose();
        }

        public static void TimerSet()
        {
            // Create a timer with a three second interval.
            timer = new Timer(3000);
            // Hook up the Elapsed event for the timer. 
            timer.Elapsed += OnTimedEvent;
            timer.Start();
        }

        public static Task<T> WaitResponseAsync<T>()
        {
            try
            {
                bool key = true;
                //TimerSet();
                return Task.Run(() =>
                {
                    while (key)
                    {
                        if (isError) throw new Exception("Превышено время ожидания ответа от сервера"); ;
                        if (response != null) key = false;
                    }
                    //timer.Stop();
                    //timer.Close();
                    return ClientAuction.DeserializeFromString<T>(response);
                });
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        public static T WaitResponse<T>()
        {
            try
            {
                bool key = true;
                //TimerSet();
                while (key)
                {
                    if (isError) throw new Exception("Превышено время ожидания ответа от сервера"); ;
                    if (response != null) key = false;
                }
                //timer.Stop();
                //timer.Close();
                return ClientAuction.DeserializeFromString<T>(response);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        public static void CreateRequest<T>(string methodName, T reqObj)
        {
            try
            {
                if (!ClientAuction.Stream.CanWrite)
                {
                    if (!ServerConnector.ConnectToServer())
                    {
                        throw new Exception("Сервер не доступен");
                    }
                }
                string parametr = ClientAuction.SerializeToString(reqObj);
                Request request = new Request(parametr, methodName);
                response = null;
                ClientAuction.SendMessage(ClientAuction.SerializeToString(request));
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        public static void CreateRequest(string methodName)
        {
            CreateRequest(methodName, "");
        }

        public Requester(List<Response> responseList)
        {
            this.responseList = responseList;
        }

        public override string GetActualTrade()
        {
            response = GetFirstMessage();
            OnStatusBarTradeUpdate?.Invoke(GetFirstMessage());
            return null;
        }

        public override string GetActualProduct()
        {
            response = GetFirstMessage();
            OnStatusBarProductUpdate?.Invoke(GetFirstMessage());
            return null;
        }

        public override string SetTimer()
        {
            OnTimer?.Invoke(GetFirstMessage());
            return null;
        }

        public override string UpdateTrade()
        {
            OnTradeupdate?.Invoke(GetFirstMessage());
            return null;
        }

        public override string GetPhoto()
        {
            RequestMethods methods = new RequestMethods();
            string message = GetFirstMessage();
            ClientAuction.IsRecieveImage = true;
            CreateRequest(methods.GetPhotoSt2());
            ClientAuction.RecieveImage(ClientAuction.DeserializeFromString<int>(message));
            response = "true";
            return null;
        }

        public override string DisconnectServer()
        {
            MainWindow.Logout();
            return null;
        }

        public override string SendDefaultResponse()
        {
            response = GetFirstMessage();
            return null;
        }
    }
}

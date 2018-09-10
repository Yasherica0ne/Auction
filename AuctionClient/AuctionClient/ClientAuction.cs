using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Web.Script.Serialization;

namespace AuctionClient
{
    public class ClientAuction
    {
        private const string host = "127.0.0.1";
        private const int port = 8888;
        static List<string> sender = new List<string>();

        public static string UserName { get; set; }

        public static string Host => host;

        public static int Port => port;

        public static TcpClient Client { get; set; }
        public static NetworkStream Stream { get; set; }
        public static bool IsOpenMain { get; set; } = false;
        public static byte[] ImageBytes { get; set; } = null;
        public static bool IsFullImage { get; set; } = false;
        public static bool Key { get; set; } = true;
        public static bool IsRecieveImage { get; set; } = false;
        internal static Response LastMethod { get; set; }
        public static string Message { get; set; }
        private static ServerConnector serverConnector = new ServerConnector();
        

        // отправка сообщений
        public static void SendMessage(string message)
        {
            try
            {
                if (Stream != null)
                {
                    BinaryWriter writer = new BinaryWriter(Stream);
                    writer.Write(message + "%");
                }
                else if (ServerConnector.ConnectToServer())
                {
                    SendMessage(message);
                }
            }
            catch(Exception ex)
            {
                string messageXui = ex.Message;
            }
        }

        public static void SendMessageInBytes(byte[] data)
        {
            if (Stream != null)
            {
                BinaryWriter writer = new BinaryWriter(Stream);
                writer.Write(data);
            }
            else if (ServerConnector.ConnectToServer())
            {
                SendMessageInBytes(data);
            }
        }

        private static List<Response> responseList = new List<Response>();
        private static string notFullMessage = null;
        private static Requester methodInvoker = new Requester(responseList);

        private static void InvokeMethod(Response response)
        {
            try
            {
                MethodInfo methodInfo = typeof(Requester).GetMethod(response.Reciever);
                var thisParameter = Expression.Constant(methodInvoker);
                MethodCallExpression methodCall = Expression.Call(thisParameter, methodInfo);
                Expression<Func<string>> lambda = Expression.Lambda<Func<string>>(methodCall);
                Func<string> func = lambda.Compile();
                func();
                //System.Threading.Thread receiveThread = new System.Threading.Thread(new System.Threading.);
                //receiveThread.Start(); //старт потока
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        public static void RecieveImage(int bytesCount)
        {
            try
            {
                ImageBytes = new byte[bytesCount];
                BinaryReader reader = new BinaryReader(Stream);
                ImageBytes = reader.ReadBytes(bytesCount);
                IsFullImage = true;
                IsRecieveImage = false;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
                throw (ex);
            }
        }

        // получение сообщений
        public static void ReceiveMessage()
        {
            while (Key)
            {
                try
                {
                    BinaryReader reader = new BinaryReader(Stream);
                    StringBuilder builder = new StringBuilder();
                    do
                    {
                        builder.Append(reader.ReadString());
                    } while (Stream.DataAvailable);
                    if (builder.Length == 0) continue;
                    string message = builder.ToString();
                    string[] bufMesaage = message.Split('%');
                    for (int i = 0; i < bufMesaage.Length; i++)
                    {
                        if (i + 1 == bufMesaage.Length)
                        {
                            if (bufMesaage[i] != "")
                                notFullMessage = bufMesaage[i];
                            else
                                notFullMessage = null;
                            break;
                        }
                        responseList.Add(DeserializeFromString<Response>(bufMesaage[i]));
                    }
                    foreach (Response response in responseList)
                    {
                        LastMethod = response;
                        InvokeMethod(response);
                    }
                    responseList.Clear();
                }
                catch (Exception ex)
                {
                    //System.Windows.MessageBox.Show(ex.Message);
                    ClearConnect();
                }
            }
        }

        public static string SerializeToString<T>(T obj)
        {
            try
            {
                return new JavaScriptSerializer().Serialize(obj);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }


        public static T DeserializeFromString<T>(string objectString)
        {
            try
            {
                return new JavaScriptSerializer().Deserialize<T>(objectString);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        public static void ClearConnect()
        {
            if (Stream.CanWrite)
            {
                RequestMethods methods = RequestMethods.GetRequestMethods();
                methods.DisconnectServer();
                Key = false;
            }
            if (Stream != null)
                Stream.Close();//отключение потока
            if (Client != null)
                Client.Close();//отключение клиента
        }

        public static void Disconnect()
        {
            ClearConnect();
            Environment.Exit(0); //завершение процесса
        }
    }
}
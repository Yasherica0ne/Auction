using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Web.Script.Serialization;

namespace AuctionClient
{
    public class ClientAuction
    {
        static string userName;
        private const string host = "127.0.0.1";
        private const int port = 8888;
        static TcpClient client;
        static NetworkStream stream;
        static List<string> sender = new List<string>();
        static bool isOpenMain = false;

        public static string UserName { get => userName; set => userName = value; }

        public static string Host => host;

        public static int Port => port;

        public static TcpClient Client { get => client; set => client = value; }
        public static NetworkStream Stream { get => stream; set => stream = value; }
        public static bool IsOpenMain { get => isOpenMain; set => isOpenMain = value; }
        public static byte[] ImageBytes { get => imageBytes; set => imageBytes = value; }
        public static bool IsFullImage { get => isFullImage; set => isFullImage = value; }
        public static bool Key { get => key; set => key = value; }
        public static bool IsRecieveImage { get => isRecieveImage; set => isRecieveImage = value; }
        internal static Response LastMethod { get => lastMethod; set => lastMethod = value; }
        public static string Message { get => message; set => message = value; }
        private static ServerConnector serverConnector = new ServerConnector();
        

        // отправка сообщений
        public static void SendMessage(string message)
        {
            if (stream != null)
            {
                BinaryWriter writer = new BinaryWriter(Stream);
                writer.Write(message + "%");
            }
            else if (ServerConnector.ConnectToServer())
            {
                SendMessage(message);
            }
        }

        public static void SendMessageInBytes(byte[] data)
        {
            if (stream != null)
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
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        private static byte[] imageBytes = null;
        private static bool isFullImage = false;
        private static bool isRecieveImage = false;

        public static void RecieveImage(int bytesCount)
        {
            try
            {
                imageBytes = new byte[bytesCount];
                BinaryReader reader = new BinaryReader(stream);
                imageBytes = reader.ReadBytes(bytesCount);
                isFullImage = true;
                IsRecieveImage = false;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        private static Response lastMethod;

        private static bool key = true;
        private static string message;
        // получение сообщений
        public static void ReceiveMessage()
        {
            while (key)
            {
                try
                {
                    BinaryReader reader = new BinaryReader(stream);
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
                        lastMethod = response;
                        InvokeMethod(response);
                    }
                    responseList.Clear();
                }
                catch (Exception ex)
                {
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
            Key = false;
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
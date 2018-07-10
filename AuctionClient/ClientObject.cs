using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace AuctionClient
{
    public class ClientAuct
    {
        static string userName;
        private const string host = "127.0.0.1";
        private const int port = 8888;
        static TcpClient client;
        static NetworkStream stream;

        public static string UserName { get => userName; set => userName = value; }

        public static string Host => host;

        public static int Port => port;

        public static TcpClient Client { get => client; set => client = value; }
        public static NetworkStream Stream { get => stream; set => stream = value; }

        // отправка сообщений
        public static void SendMessage(string message)
        {
            //string message = Console.ReadLine();
            byte[] data = Encoding.Unicode.GetBytes(message);
            Stream.Write(data, 0, data.Length);
        }
        // получение сообщений
        public static void ReceiveMessage()
        {
            while (true)
            {
                try
                {
                    byte[] data = new byte[64]; // буфер для получаемых данных
                    StringBuilder builder = new StringBuilder();
                    int bytes = 0;
                    do
                    {
                        bytes = Stream.Read(data, 0, data.Length);
                        builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                    }
                    while (Stream.DataAvailable);

                    string message = builder.ToString();
                    Entering.Response = message;
                    //Console.WriteLine(message);//вывод сообщения
                }
                catch
                {
                    Console.WriteLine("Подключение прервано!"); //соединение было прервано
                    Console.ReadLine();
                    Disconnect();
                }
            }
        }

        public static string SerializeToString<T>(T obj)
        {
            return new JavaScriptSerializer().Serialize(obj);
        }


        public static T DeserializeFromString<T>(string objectString)
        {
            return new JavaScriptSerializer().Deserialize<T>(objectString);
        }

        public static void Disconnect()
        {
            if (Stream != null)
                Stream.Close();//отключение потока
            if (Client != null)
                Client.Close();//отключение клиента
            Environment.Exit(0); //завершение процесса
        }
    }
}
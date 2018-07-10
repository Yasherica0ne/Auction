using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AuctionServerWpf
{
    public class ClientObject
    {
        private string id;
        protected internal NetworkStream Stream { get; private set; }
        public bool IsOpen { get => isOpen; set => isOpen = value; }
        public AuctionServer Server { get => server; set => server = value; }
        protected internal string Id { get => id; private set => id = value; }
        public Account Account { get => account; set => account = value; }
        public byte[] ImageBytes { get => imageBytes; set => imageBytes = value; }
        public bool IsRecievingImage { get => isRecievingImage; set => isRecievingImage = value; }
        public int ImageBytesCount { get => imageBytesCount; set => imageBytesCount = value; }
        public bool IsFullImage { get => isFullImage; set => isFullImage = value; }

        Account account;
        TcpClient client;
        AuctionServer server; // объект сервера
        bool isOpen = false;

        public ClientObject(TcpClient tcpClient, AuctionServer serverObject)
        {
            Id = Guid.NewGuid().ToString();
            client = tcpClient;
            Server = serverObject;
            serverObject.AddConnection(this);
        }

        List<Request> requestList = new List<Request>();
        private static string notFullMessage = null;

        public void Process()
        {
            try
            {
                Stream = client.GetStream();
                // получаем имя пользователя
                string message = null;
                // в бесконечном цикле получаем сообщения от клиента
                while (true)
                {
                    try
                    {
                        message = GetMessage();
                        if (message.Length == 0) continue;
                        string[] bufMesaage = message.ToString().Split('%');
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
                            requestList.Add(AuctionServer.DeserializeFromString<Request>(bufMesaage[i]));
                        }
                        while(requestList.Count() != 0)
                        {
                            Request request = requestList.First();
                            MainWindow.Main.Dispatcher.Invoke(() =>
                            {
                                MainWindow.Main.LastRequestMethodName.Text = request.MethodName;
                            });
                            Requester methodInvoker = new Requester(request.Parametr, this);
                            MethodInfo methodInfo = typeof(Requester).GetMethod(request.MethodName);
                            // Create "thisParameter" needed to call instance methods.
                            var thisParameter = Expression.Constant(methodInvoker);
                            // Create an expression for the method call "Calculate" and specify its parameter(s).
                            // If the method was a static method, the "thisParameter" must be removed.
                            MethodCallExpression methodCall = Expression.Call(thisParameter, methodInfo);
                            // Create lambda expression from MethodCallExpression.
                            Expression<Func<string>> lambda = Expression.Lambda<Func<string>>(methodCall);
                            // Compile lambda expression to a Func<>.
                            Func<string> func = lambda.Compile();
                            // Dynamically call instance method by "name".
                            func();
                            requestList.Remove(request);
                        }
                    }
                    catch(Exception ex)
                    {
                        throw (ex);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                // в случае выхода из цикла закрываем ресурсы
                Server.RemoveConnection(this.Id);
                Close();
            }
        }

        private byte[] imageBytes = null;
        private bool isRecievingImage = false;
        private int imageBytesCount = 0;
        private bool isFullImage = false;

        // чтение входящего сообщения и преобразование в строку
        private string GetMessage()
        {
            BinaryReader reader = new BinaryReader(Stream);
            StringBuilder builder = new StringBuilder();
            do
            {
                if (!isRecievingImage)
                {
                    builder.Append(reader.ReadString());
                }
                else
                {
                    while (imageBytesCount == 0) ;
                    imageBytes = new byte[imageBytesCount];
                    imageBytes = reader.ReadBytes(imageBytesCount);
                    isFullImage = true;
                    imageBytesCount = 0;
                    isRecievingImage = false;
                }
            }
            while (Stream.DataAvailable);
            return builder.ToString();
        }

        // закрытие подключения
        protected internal void Close()
        {
            if (Stream != null)
                Stream.Close();
            if (client != null)
                client.Close();
        }
    }
}

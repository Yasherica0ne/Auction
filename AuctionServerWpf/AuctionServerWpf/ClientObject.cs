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
        public bool IsOpen { get; set; } = false;
        public AuctionServer Server { get; set; }
        protected internal string Id { get => id; private set => id = value; }
        public Account Account { get; set; }
        public byte[] ImageBytes { get; set; } = null;
        public bool IsRecievingImage { get; set; } = false;
        public int ImageBytesCount { get; set; } = 0;
        public bool IsFullImage { get; set; } = false;
        public Auction Auction { get; set; }

        TcpClient client;

        public ClientObject(TcpClient tcpClient, AuctionServer serverObject)
        {
            Id = Guid.NewGuid().ToString();
            client = tcpClient;
            Server = serverObject;
            serverObject.AddConnection(this);
            Auction = null;
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
                    while (requestList.Count() != 0)
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
            }
            catch (Exception ex)
            {
                //System.Windows.MessageBox.Show(ex.Message);
            }
            finally
            {
                // в случае выхода из цикла закрываем ресурсы
                Server.RemoveConnection(Id);
                Close();
            }
        }

        // чтение входящего сообщения и преобразование в строку
        private string GetMessage()
        {
            BinaryReader reader = new BinaryReader(Stream);
            StringBuilder builder = new StringBuilder();
            do
            {
                if (!IsRecievingImage)
                {
                    builder.Append(reader.ReadString());
                }
                else
                {
                    while (ImageBytesCount == 0) ;
                    ImageBytes = new byte[ImageBytesCount];
                    ImageBytes = reader.ReadBytes(ImageBytesCount);
                    IsFullImage = true;
                    ImageBytesCount = 0;
                    IsRecievingImage = false;
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

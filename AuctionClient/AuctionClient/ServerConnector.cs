using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace AuctionClient
{
    class ServerConnector
    {
        private static bool isConnectionOpened = false;

        public static bool IsConnectionOpened { get => isConnectionOpened; set => isConnectionOpened = value; }

        public static bool ConnectToServer()
        {
            ClientAuction.Client = new TcpClient();
            try
            {
                ClientAuction.Client.Connect(ClientAuction.Host, ClientAuction.Port); //подключение клиента
                ClientAuction.Stream = ClientAuction.Client.GetStream(); // получаем поток
                // запускаем новый поток для получения данных
                ClientAuction.Key = true;
                Thread receiveThread = new Thread(new ThreadStart(ClientAuction.ReceiveMessage));
                receiveThread.Start(); //старт потока
                //if (entering != null)
                //{
                //    entering.BorderBrush = System.Windows.Media.Brushes.LimeGreen;
                //}
                return isConnectionOpened = true;
            }
            catch (Exception ex)
            {
                //if (entering != null)
                //{
                //    entering.BorderBrush = System.Windows.Media.Brushes.OrangeRed;
                //}
                MessageBox.Show(ex.Message);
                return isConnectionOpened = false;
            }
        }
    }
}

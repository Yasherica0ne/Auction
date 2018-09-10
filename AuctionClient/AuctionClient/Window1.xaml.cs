using System.Windows;
using GalaSoft.MvvmLight.Messaging;
using System.Security;
using System;
using System.Net;

namespace AuctionClient
{
    /// <summary>
    /// Логика взаимодействия для Window1.xaml
    /// </summary>
    public partial class EnteringWindow : Window
    {
        public EnteringWindow()
        {
            try
            {
                InitializeComponent();
                Messenger.Default.Register<EnteringWindowViewModel>(this,
                    (msg) =>
                    {
                        Close();
                    });
                Messenger.Default.Register<SecureString>(this,
                    (msg) =>
                    {
                        Dispatcher.Invoke(() =>
                        {
                            PasswordBox.Password = new NetworkCredential("", msg).Password;
                        });
                    });
                DataContext = new EnteringWindowViewModel();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            SecureString password = new SecureString();
            foreach(char c in PasswordBox.Password)
            {
                password.AppendChar(c);
            }
            password.MakeReadOnly();
            ((EnteringWindowViewModel)DataContext).Password = password;
        }
    }
}

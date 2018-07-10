using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AuctionClient
{
    /// <summary>
    /// Логика взаимодействия для Registration.xaml
    /// </summary>
    public partial class RegistrationWindow : Window
    {
        public RegistrationWindow()
        {
            InitializeComponent();
        }

        private void SecondPass_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (SecondPass.Password.Equals("")) Registry.IsEnabled = false;
            else if (FirstPass.Password.Length != SecondPass.Password.Length)
            {
                if (!FirstPass.Password.Contains(SecondPass.Password)) Warning.Text = "Пароли не совпадают";
                else Warning.Text = "";
                Registry.IsEnabled = false;
            }
            else if (!FirstPass.Password.Equals(SecondPass.Password))
            {
                Warning.Text = "Пароли не совпадают";
                Registry.IsEnabled = false;
            }
            else
            {
                Warning.Text = "";
                Registry.IsEnabled = true;
            }
        }

        private bool IsCheckFields()
        {
            if (!RegLogin.Text.Equals("") && !FirstPass.Password.Equals("") && !SecondPass.Password.Equals("")) return true;
            else return false;
        }

        public bool IsValid(string emailaddress)
        {
            try
            {
                MailAddress m = new MailAddress(emailaddress);

                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        private async void Regisration_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!IsValid(Email.Text))
                {
                    Warning.Text = "Неверный email";
                    return;
                }
                if(Regex.IsMatch(FirstPass.Password, @"[^a-zA-Z0-9]+"))
                {
                    Warning.Text = "Неверный пароль";
                    return;
                }
                EnteringWindow entering = (EnteringWindow)Owner;
                if (!ServerConnector.IsConnectionOpened)
                {
                    ServerConnector.ConnectToServer();
                }
                if (ServerConnector.IsConnectionOpened)
                {
                    RequestMethods methods = new RequestMethods();
                    Account account = new Account(RegLogin.Text, Account.GetHashCode(FirstPass.Password), Email.Text);
                    Requester.CreateRequest(methods.Registration(), account);
                    string result = await Requester.WaitResponseAsync<string>();
                    if (result.Equals("false"))
                    {
                        MessageBox.Show("Такой аккаунт уже существует", "Ошибка", MessageBoxButton.OK);
                    }
                    else
                    {
                        //((EnteringWindowViewModel)Owner).EnterData(account.Login, account.Password);
                        Close();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка");
            }
        }

        private void RegLogin_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (RegLogin.Text.Equals("")) Registry.IsEnabled = false;
            else if (IsCheckFields()) Registry.IsEnabled = true;
        }

        private void FirstPass_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (!SecondPass.Password.Equals("")) SecondPass_PasswordChanged(sender, e);
            if (FirstPass.Password.Equals("")) Registry.IsEnabled = false;
        }
    }
}

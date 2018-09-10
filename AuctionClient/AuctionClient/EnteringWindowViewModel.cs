using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace AuctionClient
{
    class EnteringWindowViewModel : ViewModelBase
    {


        string fileName = @"Account.json";

        private string GetCryptPass(string pass, string login)
        {
            long code = 0;
            StringBuilder buffer = new StringBuilder();
            char[] charsLogin = login.ToCharArray();
            for (int i = 0; i < login.Length; i++)
                code += charsLogin[i];
            char[] charsPass = pass.ToCharArray();
            char[] chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789abcdefghijklmnopqrstuvwxyz".ToCharArray();
            for (int i = 0; i < pass.Length; i++)
            {
                double buf = code + charsPass[i];
                buf /= chars.Length;
                buffer.Append(chars[(code + charsPass[i]) % chars.Length]);
                buffer.Append(Math.Round(buf, 4));
                buffer.Append('>');
            }
            return buffer.ToString();
        }

        private SecureString GetEncryptPass(string cryptPass, string login)
        {
            long code = 0;
            SecureString buffer = new SecureString();
            char[] charsLogin = login.ToCharArray();
            for (int i = 0; i < login.Length; i++)
                code += charsLogin[i];
            string[] charsPass = cryptPass.Split('>');
            string Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789abcdefghijklmnopqrstuvwxyz";
            char[] chars = Chars.ToCharArray();
            for (int i = 0; i < charsPass.Length - 1; i++)
            {
                double buf;
                buf = double.Parse(charsPass[i].Substring(1));
                buf *= chars.Length;
                buf = Math.Round(buf);
                buf -= code;
                buffer.AppendChar((char)buf);
            }
            buffer.MakeReadOnly();
            return buffer;
        }

        private RelayCommand newAccountClickCommand;
        public RelayCommand NewAccountClickCommand
        {
            get
            {
                return newAccountClickCommand ?? (newAccountClickCommand = new RelayCommand(NewAccount_Click));
            }
        }

        private RelayCommand enterClickCommand;
        public RelayCommand EnterClickCommand
        {
            get
            {
                return enterClickCommand ?? (enterClickCommand = new RelayCommand(Enter_Click));
            }
        }

        private RelayCommand cancelClickCommand;
        public RelayCommand CancelClickCommand
        {
            get
            {
                return cancelClickCommand ?? (cancelClickCommand = new RelayCommand(Cancel_Click));
            }
        }

        private RelayCommand loginChangedCommand;
        public RelayCommand LoginChangedCommand
        {
            get
            {
                return loginChangedCommand ?? (loginChangedCommand = new RelayCommand(Login_TextChanged));
            }
        }

        private RelayCommand passwordChangedCommand;
        public RelayCommand PasswordChangedCommand
        {
            get
            {
                return passwordChangedCommand ?? (passwordChangedCommand = new RelayCommand(Password_PasswordChanged));
            }
        }

        //private static EnteringWindow entering;
        private bool isSavePassword;
        private bool indicatorIsBusy;
        private bool enterButtonIsEnabled;
        private bool savePasswordIsChecked;
        private string login;

        public EnteringWindowViewModel() //EnteringWindow enteringWindow)
        {
            //entering = enteringWindow;
            ServerConnector.ConnectToServer();
            if (System.IO.File.Exists(fileName))
            {
                Account account = Account.Deserialize<Account>(fileName);
                Login = account.Login;
                Password = GetEncryptPass(account.Password, account.Login);
                Task.Run(() =>
                {
                    Messenger.Default.Send(Password);
                    EnterButtonIsEnabled = true;
                });
                //IsSavePassword = true;
            }
        }

        //public EnteringWindow Entering
        //{
        //    get => entering;
        //    set
        //    {
        //        entering = value;
        //        RaisePropertyChanged("Login");
        //    }
        //}

        public string Login
        {
            get => login;
            set
            {
                login = value;
                RaisePropertyChanged("Login");
            }
        }
        public SecureString Password { get; set; }

        private string StrPassword { get => new NetworkCredential("", Password).Password; }

        public bool IsSavePassword
        {
            get => isSavePassword;
            set
            {
                isSavePassword = value;
                RaisePropertyChanged("IsSavePassword");
            }
        }

        public bool IndicatorIsBusy
        {
            get => indicatorIsBusy;
            set
            {
                indicatorIsBusy = value;
                RaisePropertyChanged("IndicatorIsBusy");
            }
        }

        public bool EnterButtonIsEnabled
        {
            get => enterButtonIsEnabled;
            set
            {
                enterButtonIsEnabled = value;
                RaisePropertyChanged("EnterButtonIsEnabled");
            }
        }

        public bool SavePasswordIsChecked
        {
            get => savePasswordIsChecked;
            set
            {
                savePasswordIsChecked = value;
                RaisePropertyChanged("SavePasswordIsChecked");
            }
        }

        //public void EnterData(string login, string password)
        //{
        //    entering.Visibility = Visibility.Visible;
        //    Login = login;
        //    entering.PasswordBox.Password = password;
        //}

        private void NewAccount_Click()
        {
            RegistrationWindow registration = new RegistrationWindow();
            registration.Show();
        }

        private bool IsCheckFields()
        {
            if (!Login.Equals("") && !StrPassword.Equals("")) return true;
            else return false;
        }


        private void Enter_Click()
        {
            try
            {
                if (!ServerConnector.IsConnectionOpened)
                {
                    ServerConnector.ConnectToServer();
                }
                if (ServerConnector.IsConnectionOpened)
                {
                    if (!IsCheckFields()) throw new Exception("Поле логина или пароля пусто");
                    BackgroundWorker worker = new BackgroundWorker();
                    RequestMethods methods = RequestMethods.GetRequestMethods();
                    Account account = null;
                    bool IsOpenMain = false;
                    worker.DoWork += (o, ea) =>
                    {
                        string login = null;
                        Dispatcher.CurrentDispatcher.Invoke(() =>
                        {
                            login = Login;

                        });
                        Account accs = methods.FindUser(login);
                        if (accs != null)
                        {
                            Dispatcher.CurrentDispatcher.Invoke(() =>
                            {
                                account = accs;
                            });
                        }
                    };

                    worker.RunWorkerCompleted += (o, ea) =>
                    {
                        if (!account.Password.Equals(Account.GetHashCode(StrPassword)))
                        {
                            IndicatorIsBusy = false;
                            MessageBox.Show("Неверный пароль", "Ошибка", MessageBoxButton.OK);
                        }
                        else IsOpenMain = true;
                        //}
                        //else
                        //{
                        //    IndicatorIsBusy = false;
                        //    MessageBox.Show("Неверное имя аккаунта", "Ошибка", MessageBoxButton.OK);
                        //}
                        if (IsOpenMain)
                        {
                            methods.SetAccount(account);
                            if (SavePasswordIsChecked)
                            {
                                Account.Serialize(new Account(Login, GetCryptPass(StrPassword, Login)), fileName);
                            }
                            MainWindow mainWindow = new MainWindow(account);
                            mainWindow.Show();
                            IndicatorIsBusy = false;
                            Messenger.Default.Send(this);
                        }
                    };
                    IndicatorIsBusy = true;
                    worker.RunWorkerAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка");
            }
        }

        private void Cancel_Click()
        {
            Messenger.Default.Send(this);
        }

        private void Login_TextChanged()
        {
            if (Login.Equals("")) EnterButtonIsEnabled = false;
            else if (IsCheckFields())
            {
                EnterButtonIsEnabled = true;
                //FocusManager.SetFocusedElement(MainGrid, Enter);
            }
        }

        private void Password_PasswordChanged()
        {
            if (StrPassword.Equals("")) EnterButtonIsEnabled = false;
            else if (IsCheckFields())
            {
                EnterButtonIsEnabled = true;
                //FocusManager.SetFocusedElement(MainGrid, Enter);
            }
        }
    }
}

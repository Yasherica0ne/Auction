using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace AuctionClient
{
    class EnteringWindowViewModel : BaseViewModel
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

        private string GetEncryptPass(string cryptPass, string login)
        {
            long code = 0;
            StringBuilder buffer = new StringBuilder();
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
                buffer.Append((char)buf);
            }
            return buffer.ToString();
        }

        private RelayCommand newAccountClickCommand;
        public RelayCommand NewAccountClickCommand
        {
            get
            {
                return newAccountClickCommand ?? (newAccountClickCommand = new RelayCommand(obj => NewAccount_Click()));
            }
        }

        private RelayCommand enterClickCommand;
        public RelayCommand EnterClickCommand
        {
            get
            {
                return enterClickCommand ?? (enterClickCommand = new RelayCommand(obj => Enter_Click()));
            }
        }

        private RelayCommand cancelClickCommand;
        public RelayCommand CancelClickCommand
        {
            get
            {
                return cancelClickCommand ?? (cancelClickCommand = new RelayCommand(obj => Cancel_Click()));
            }
        }

        private RelayCommand loginChangedCommand;
        public RelayCommand LoginChangedCommand
        {
            get
            {
                return loginChangedCommand ?? (loginChangedCommand = new RelayCommand(obj => Login_TextChanged()));
            }
        }

        private RelayCommand passwordChangedCommand;
        public RelayCommand PasswordChangedCommand
        {
            get
            {
                return passwordChangedCommand ?? (passwordChangedCommand = new RelayCommand(obj => Password_PasswordChanged()));
            }
        }
        
        private static EnteringWindow entering;
        private bool isSavePassword;
        private bool indicatorIsBusy;
        private bool enterButtonIsEnabled;
        private bool savePasswordIsChecked;
        private string login;

        public EnteringWindowViewModel(EnteringWindow enteringWindow)
        {
            entering = enteringWindow;
            ServerConnector.ConnectToServer();
            if (System.IO.File.Exists(fileName))
            {
                Account account = Account.Deserialize<Account>(fileName);
                login = account.Login;
                entering.PasswordBox.Password = GetEncryptPass(account.Password, account.Login);
                IsSavePassword = true;
                EnterButtonIsEnabled = true;
            }
        }

        public EnteringWindow Entering
        {
            get => entering;
            set
            {
                entering = value;
                OnPropertyChanged("Login");
            }
        }

        public string Login
        {
            get => login;
            set
            {
                login = value;
                OnPropertyChanged("Login");
            }
        }
        //public string Password
        //{
        //    get => Password;
        //    set
        //    {
        //        Password = value;
        //        //OnPropertyChanged(Password);
        //    }
        //}

        public bool IsSavePassword
        {
            get => isSavePassword;
            set
            {
                isSavePassword = value;
                OnPropertyChanged("IsSavePassword");
            }
        }

        public bool IndicatorIsBusy
        {
            get => indicatorIsBusy;
            set
            {
                indicatorIsBusy = value;
                OnPropertyChanged("IndicatorIsBusy");
            }
        }

        public bool EnterButtonIsEnabled
        {
            get => enterButtonIsEnabled;
            set
            {
                enterButtonIsEnabled = value;
                OnPropertyChanged("EnterButtonIsEnabled");
            }
        }

        public bool SavePasswordIsChecked
        {
            get => savePasswordIsChecked;
            set
            {
                savePasswordIsChecked = value;
                OnPropertyChanged("SavePasswordIsChecked");
            }
        }

        public void EnterData(string login, string password)
        {
            entering.Visibility = Visibility.Visible;
            Login = login;
            entering.PasswordBox.Password = password;
        }

        private void NewAccount_Click()
        {
            RegistrationWindow registration = new RegistrationWindow
            {
                Owner = entering
            };
            registration.Show();
        }

        private bool IsCheckFields()
        {
            if (!Login.Equals("") && !entering.PasswordBox.Password.Equals("")) return true;
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
                    RequestMethods methods = new RequestMethods();
                    Account account = null;
                    List<Account> accounts = null;
                    bool IsOpenMain = false;
                    worker.DoWork += (o, ea) =>
                    {
                        string login = null;
                        Dispatcher.CurrentDispatcher.Invoke(() =>
                        {
                            login = Login;

                        });
                        Requester.CreateRequest(methods.FindUser(), login);
                        while (Requester.Response == null) ;
                        List<Account> accs = Requester.WaitResponseAsync<List<Account>>().Result;
                        if (accs.Count != 0)
                        {
                            Dispatcher.CurrentDispatcher.Invoke(() =>
                            {
                                accounts = accs.ToList();
                            });
                        }
                    };

                    worker.RunWorkerCompleted += (o, ea) =>
                    {
                        if (accounts.Count() != 0)
                        {
                            account = accounts.First();
                            if (!account.Password.Equals(Account.GetHashCode(entering.PasswordBox.Password)))
                            {
                                IndicatorIsBusy = false;
                                MessageBox.Show("Неверный пароль", "Ошибка", MessageBoxButton.OK);
                            }
                            else IsOpenMain = true;
                        }
                        else
                        {
                            IndicatorIsBusy = false;
                            MessageBox.Show("Неверное имя аккаунта", "Ошибка", MessageBoxButton.OK);
                        }
                        if (IsOpenMain)
                        {
                            Requester.CreateRequest(methods.SetAccount(), account);
                            if (SavePasswordIsChecked)
                            {
                                Account.Serialize(new Account(Login, GetCryptPass(entering.PasswordBox.Password, Login)), fileName);
                            }
                            MainWindow mainWindow = new MainWindow(account);
                            mainWindow.Show();
                            IndicatorIsBusy = false;
                            entering.Close();
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
            entering.Close();
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
            if (entering.PasswordBox.Password.Equals("")) EnterButtonIsEnabled = false;
            else if (IsCheckFields())
            {
                EnterButtonIsEnabled = true;
                //FocusManager.SetFocusedElement(MainGrid, Enter);
            }
        }
    }
}

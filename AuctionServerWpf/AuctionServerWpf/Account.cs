using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace AuctionServerWpf
{
    public class Account
    {
        private int accountID;
        private string login;
        private string password;
        private bool isAdmin;
        private string email;

        public Account() { }

        public Account(string _login, string _password)
        {
            this.login = _login;
            this.password = _password;
            this.isAdmin = false;
        }

        public Account(string _login, string _password, string _email)
        {
            this.login = _login;
            this.password = _password;
            this.isAdmin = false;
            this.Email = _email;
        }

        public string Login { get => login; set => login = value; }
        public string Password { get => password; set => password = value; }
        public int AccountId { get => accountID; set => accountID = value; }
        public bool IsAdmin { get => isAdmin; set => isAdmin = value; }
        public string Email { get => email; set => email = value; }

        public static string GetHashCode(string pass)
        {
            byte[] hash = Encoding.ASCII.GetBytes(pass);
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] hashenc = md5.ComputeHash(hash);
            StringBuilder result = new StringBuilder();
            foreach (var b in hashenc)
            {
                result.Append(b.ToString("x2"));
            }
            return result.ToString();
        }
    }
}

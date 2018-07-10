using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Json;
using System.Runtime.Serialization;
using System.Security.Cryptography;

namespace AuctionClient
{
    [DataContract]
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

        [DataMember]
        public string Login { get => login; set => login = value; }
        [DataMember]
        public string Password { get => password; set => password = value; }
        [IgnoreDataMember]
        public int AccountId { get => accountID; set => accountID = value; }
        [IgnoreDataMember]
        public bool IsAdmin { get => isAdmin; set => isAdmin = value; }
        [IgnoreDataMember]
        public string Email { get => email; set => email = value; }

        public static void Serialize<T>(T obj, string fileName)
        {
            DataContractJsonSerializer formatter = new DataContractJsonSerializer(typeof(T));
            using (FileStream fs = new FileStream(fileName, FileMode.Truncate))
            {
                formatter.WriteObject(fs, obj);
            }
            return;
        }
        

        public static T Deserialize<T>(string fileName)
        {
            T obj;
            DataContractJsonSerializer formatter = new DataContractJsonSerializer(typeof(T));
            using (FileStream fs = new FileStream(fileName, FileMode.OpenOrCreate))
            {
                obj = (T)formatter.ReadObject(fs);
            }
            return obj;
        }

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

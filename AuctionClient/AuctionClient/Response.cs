using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuctionClient
{
    class Response
    {
        string reciever;
        string message;

        public Response() { }

        public Response(string reciever, string message)
        {
            this.reciever = reciever;
            this.message = message;
        }

        public string Reciever { get => reciever; set => reciever = value; }
        public string Message { get => message; set => message = value; }
    }
}

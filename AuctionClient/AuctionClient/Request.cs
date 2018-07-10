using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuctionClient
{
    class Request
    {
        private string methodName;
        private string parametr;

        public string MethodName { get => methodName; set => methodName = value; }
        public string Parametr { get => parametr; set => parametr = value; }

        public Request() { }

        public Request(string _methodName)
        {
            this.methodName = _methodName;
            this.parametr = "";
        }

        public Request(string _parametr, string _methodName)
        {
            this.methodName = _methodName;
            this.parametr = _parametr;
        }
    }
}
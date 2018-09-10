using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;

namespace AuctionClient
{
    public class RequestMethods
    {


        public  string Response { get; set; }
        public Timer Timer { get; set; } = null;

        private bool isError = false;

        private void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            isError = true;
            Timer.Dispose();
        }

        private void TimerSet()
        {
            // Create a timer with a three second interval.
            Timer = new Timer(3000);
            // Hook up the Elapsed event for the timer. 
            Timer.Elapsed += OnTimedEvent;
            Timer.AutoReset = false;
            Timer.Start();
        }

        private bool isBlocked = false;

        public void PulseLocker()
        {
            try
            {
                lock (locker)
                {
                    if (isBlocked)
                    {
                        System.Threading.Monitor.Pulse(locker);
                    }
                    else isBlocked = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                throw (ex);
            }
        }

        private static readonly Object locker = new Object();

        private static RequestMethods methods;

        private RequestMethods()
        {
        }

        public static RequestMethods GetRequestMethods()
        {
            lock (locker)
            {
                return methods ?? (methods = new RequestMethods());
            }
        }

        private Task<bool> PWaitResponseAsync()
        {
            try
            {
                return Task.Run(() =>
                {
                    lock (locker)
                    {
                        System.Threading.Monitor.Wait(locker);
                        return true;
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                throw (ex);
            }
        }
        public Task<T> WaitResponseAsync<T>()
        {
            try
            {
                //TimerSet();
                return Task.Run(async () =>
                {
                    if (!isBlocked)
                    {
                        isBlocked = true;
                        await PWaitResponseAsync();
                    }
                    isBlocked = false;
                    if (isError) throw new Exception("Превышено время ожидания ответа от сервера");
                    return ClientAuction.DeserializeFromString<T>(Response);
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                throw (ex);
            }
        }

        private T WaitResponse<T>()
        {
            try
            {
                lock (locker)
                {
                    if (!isBlocked)
                    {
                        isBlocked = true;
                        System.Threading.Monitor.Wait(locker);
                    }

                    isBlocked = false;
                    if (isError) throw new Exception("Превышено время ожидания ответа от сервера");
                    return ClientAuction.DeserializeFromString<T>(Response);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                throw (ex);
            }
        }

        public void CreateRequest<T>(string methodName, T reqObj)
        {
            try
            {
                if (!ClientAuction.Stream.CanWrite)
                {
                    if (!ServerConnector.ConnectToServer())
                    {
                        throw new Exception("Сервер не доступен");
                    }
                }
                string parametr = ClientAuction.SerializeToString(reqObj);
                Request request = new Request(parametr, methodName);
                Response = null;
                ClientAuction.SendMessage(ClientAuction.SerializeToString(request));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                throw (ex);
            }
        }

        public void CreateRequest(string methodName)
        {
            CreateRequest(methodName, "");
        }

        public async Task<bool> SetAccountAsync(Account account)
        {
            CreateRequest("SetAccount", account);
            return await WaitResponseAsync<bool>();
        }

        public bool SetAccount(Account account)
        {
            CreateRequest("SetAccount", account);
            return WaitResponse<bool>();
        }

        public async Task<List<Product>> GetSalesListAsync(int id)
        {
            CreateRequest("GetSalesList", id);
            return await WaitResponseAsync<List<Product>>();
        }

        public List<Product> GetSalesList(int id)
        {
            CreateRequest("GetSalesList", id);
            return WaitResponse<List<Product>>();
        }
        public async Task<Account> FindUserAsync(string login)
        {
            CreateRequest("FindUser", login);
            return await WaitResponseAsync<Account>();
        }

        public Account FindUser(string login)
        {
            CreateRequest("FindUser", login);
            return WaitResponse<Account>();
        }

        public async Task<List<Product>> GetNewProductListAsync()
        {
            CreateRequest("GetNewProductList");
            return await WaitResponseAsync<List<Product>>();
        }

        public List<Product> GetNewProductList()
        {
            CreateRequest("GetNewProductList");
            return WaitResponse<List<Product>>();
        }

        //public string GetActualTrade()
        //{
        //    return "GetActualTrade";
        //}
        public async Task<Product> GetActualProductAsync(int id)
        {
            CreateRequest("GetActualProduct", id);
            return await WaitResponseAsync<Product>();
        }

        public Product GetActualProduct(int id)
        {
            CreateRequest("GetActualProduct", id);
            return WaitResponse<Product>();
        }

        public async Task<bool> ApproveNewProductAsync(int id)
        {
            CreateRequest("ApproveNewProduct", id);
            return await WaitResponseAsync<bool>();
        }

        public bool ApproveNewProduct(int id)
        {
            CreateRequest("ApproveNewProduct", id);
            return WaitResponse<bool>();
        }

        public async Task<List<Product>> GetPurchaseListAsync(int id)
        {
            CreateRequest("GetPurchaseList", id);
            return await WaitResponseAsync<List<Product>>();
        }

        public List<Product> GetPurchaseList(int id)
        {
            CreateRequest("GetPurchaseList", id);
            return WaitResponse<List<Product>>();
        }

        public async Task<bool> RaiseMaxBetAsync(string bet)
        {
            CreateRequest("RaiseMaxBet", bet);
            return await WaitResponseAsync<bool>();
        }

        public bool RaiseMaxBet(string bet)
        {
            CreateRequest("RaiseMaxBet", bet);
            return WaitResponse<bool>();
        }

        public async Task<bool> RegistrationAsync(Account account)
        {
            CreateRequest("Registration", account);
            return await WaitResponseAsync<bool>();
        }

        public bool Registration(Account account)
        {
            CreateRequest("Registration", account);
            return WaitResponse<bool>();
        }

        public async Task<string> GetActualTimerAsync(int id)
        {
            CreateRequest("GetActualTimer", id);
            return await WaitResponseAsync<string>();
        }

        public string GetActualTimer(int id)
        {
            CreateRequest("GetActualTimer", id);
            return WaitResponse<string>();
        }

        public async Task<bool> CancelNewProductAsync(int id)
        {
            CreateRequest("CancelNewProduct", id);
            return await WaitResponseAsync<bool>();
        }
        public bool CancelNewProduct(int id)
        {
            CreateRequest("CancelNewProduct", id);
            return WaitResponse<bool>();
        }

        public async Task<int> AddProductAsync(Product product)
        {
            CreateRequest("AddProduct", product);
            return await WaitResponseAsync<int>();
        }
        public int AddProduct(Product product)
        {
            CreateRequest("AddProduct", product);
            return WaitResponse<int>();
        }

        public async Task<bool> SetProductPhotoAsync(int id, byte[] imageBt)
        {
            CreateRequest("PreSetProductPhoto", imageBt.Length);
            ClientAuction.SendMessageInBytes(imageBt);
            CreateRequest("SetProductPhoto", id);
            return await WaitResponseAsync<bool>();
        }
        public bool SetProductPhoto(int id, byte[] imageBt)
        {
            CreateRequest("PreSetProductPhoto", imageBt.Length);
            ClientAuction.SendMessageInBytes(imageBt);
            CreateRequest("SetProductPhoto", id);
            return WaitResponse<bool>();
        }

        public async Task<byte[]> GetPhotoAsync(int id)
        {
            CreateRequest("GetPhoto", id);
            await WaitResponseAsync<bool>();
            ClientAuction.IsFullImage = false;
            return ClientAuction.ImageBytes;
        }
        public byte[] GetPhoto(int id)
        {
            CreateRequest("GetPhoto", id);
            WaitResponse<bool>();
            ClientAuction.IsFullImage = false;
            return ClientAuction.ImageBytes;
        }

        public async Task<bool> CheckAuctionStatusAsync(int id)
        {
            CreateRequest("CheckAuctionStatus", id);
            return await WaitResponseAsync<bool>();
        }
        public bool CheckAuctionStatus(int id)
        {
            CreateRequest("CheckAuctionStatus", id);
            return WaitResponse<bool>();
        }

        public void DisconnectServer()
        {
            CreateRequest("DisconnectServer");
        }

        public async Task<Product> GetNextProductAsync(int id)
        {
            CreateRequest("GetNextProduct", id);
            return await WaitResponseAsync<Product>();
        }
        public Product GetNextProduct(int id)
        {
            CreateRequest("GetNextProduct", id);
            return WaitResponse<Product>();
        }

        public async Task<string> GetWinnerNameAsync(int id)
        {
            CreateRequest("GetWinnerName", id);
            return await WaitResponseAsync<string>();
        }
        public string GetWinnerName(int id)
        {
            CreateRequest("GetWinnerName", id);
            return WaitResponse<string>();
        }

        public async Task<string> GetEmailAsync(int id)
        {
            CreateRequest("GetEmail", id);
            return await WaitResponseAsync<string>();
        }
        public string GetEmail(int id)
        {
            CreateRequest("GetEmail", id);
            return WaitResponse<string>();
        }

        public async Task<bool> EnterToTradeAsync(int id)
        {
            CreateRequest("EnterToTrade", id);
            return await WaitResponseAsync<bool>();
        }
        public bool EnterToTrade(int id)
        {
            CreateRequest("EnterToTrade", id);
            return WaitResponse<bool>();
        }

        public async Task<bool> LeaveTradeAsync()
        {
            CreateRequest("LeaveTrade");
            return await WaitResponseAsync<bool>();
        }
        public bool LeaveTrade()
        {
            CreateRequest("LeaveTrade");
            return WaitResponse<bool>();
        }

        public async Task<Product> GetProductByIdAsync(int id)
        {
            CreateRequest("GetProductById", id);
            return await WaitResponseAsync<Product>();
        }
        public Product GetProductById(int id)
        {
            CreateRequest("GetProductById", id);
            return WaitResponse<Product>();
        }

        public async Task<List<Trade>> GetActualTradesAsync()
        {
            CreateRequest("GetActualTrades");
            return await WaitResponseAsync<List<Trade>>();
        }
        public List<Trade> GetActualTrades()
        {
            CreateRequest("GetActualTrades");
            return WaitResponse<List<Trade>>();
        }
    }
}

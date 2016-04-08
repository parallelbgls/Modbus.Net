using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client;
using Newtonsoft.Json;
using IdentityModel.Client;

namespace Modbus.Net.FBox
{
    public struct SignalRSigninMsg
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string UserId { get; set; }
        public string Password { get; set; }
        public string SigninAdditionalValues { get; set; }
        public SignalRServer SignalRServer { get; set; }
    }

    public class FBoxConnector : BaseConnector
    {
        private TokenClient _oauth2;
        private string _refreshToken; 

        private HttpClient _httpClient { get; set; }
        private HttpClient _httpClient2 { get; set; }
        private HubConnection _hubConnection { get; set; }
        protected SignalRSigninMsg Msg { get; }
        private DMonGroup _dataGroup { get; set; }
        private string _groupUid { get; set; }
        private string _boxUid { get; set; }
        private string _boxNo { get; set; }
        private int _boxSessionId { get; set; }
        private int _connectionState { get; set; }
        private Dictionary<string, double> _data { get; set; }
        private Dictionary<string, Type> _dataType { get; set; }

        private DateTime _timeStamp { get; set; } = DateTime.MinValue;

        public override string ConnectionToken { get; }

        //private AsyncLock _lock = new AsyncLock();

        private Timer _timer;

        private string MachineId => ConnectionToken.Split(',')[0];

        private string LocalSequence => ConnectionToken.Split(',')[1];

        private bool _connected;
        public override bool IsConnected => _connected;

        private Constants _constants;
        private Constants Constants => _constants ?? (_constants = new Constants());

        public FBoxConnector(string machineId, string localSequence, SignalRSigninMsg msg)
        {
            Constants.SignalRServer = msg.SignalRServer;
            //System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            ConnectionToken = machineId + "," + localSequence;
            Msg = msg;
            _data = new Dictionary<string, double>();
            _dataType = new Dictionary<string, Type>();
        }

        private async void ChangeToken(object sender)
        {
            try
            {
                var tokenResponse = await _oauth2.RequestRefreshTokenAsync(_refreshToken);
                _refreshToken = tokenResponse.RefreshToken;
                _httpClient.SetBearerToken(tokenResponse.AccessToken);

                _httpClient2.SetBearerToken(tokenResponse.AccessToken);
                
                _hubConnection.Headers["Authorization"] = "Bearer " + tokenResponse.AccessToken;
                await _hubConnection.Start();
                await
                    _httpClient2.PostAsync(
                        "dmon/group/" + _dataGroup.Uid + "/start", null);

            }
            catch (Exception e)
            {
                Console.WriteLine("Retoken failed." + e.Message);
            }
            Console.WriteLine("Retoken success.");
        }

        public override bool Connect()
        {
            return AsyncHelper.RunSync(ConnectAsync);
        }

        public override async Task<bool> ConnectAsync()
        {
            try
            {

                _oauth2 = new TokenClient(
                    Constants.TokenEndpoint,
                    Msg.ClientId,
                    Msg.ClientSecret
                    );
                var tokenResponse = await _oauth2.RequestResourceOwnerPasswordAsync
                    (
                        Msg.UserId,
                        Msg.Password,
                        Msg.SigninAdditionalValues
                    );
                if (tokenResponse != null)
                {
                    _refreshToken = tokenResponse.RefreshToken;
                    if (await CallService(Msg, tokenResponse.AccessToken))
                    {
                        await
                            _httpClient2.PostAsync(
                                "dmon/group/" + _dataGroup.Uid + "/start", null);
                        _connected = true;
                        _timer = new Timer(ChangeToken, null, 3600*1000*4, 3600*1000*4);
                        Console.WriteLine("SignalR Connected success");
                        return true;
                    }
                }
                return false;
            }
            catch (Exception e)
            {
                _oauth2 = null;
                Console.WriteLine("SignalR Connected failed " + e.Message);
                Clear();
                return false;
            }
        }

        private async Task<bool> CallService(SignalRSigninMsg msg, string token)
        {
            try
            {
                var guid = Guid.NewGuid().ToString();

                var baseAddress = Constants.AspNetWebApiSampleApi;

                _httpClient = new HttpClient
                {
                    BaseAddress = new Uri(baseAddress)
                };

                _httpClient.SetBearerToken(token);

                //var response = await _httpClient.GetStringAsync("device/spec");

                //List<DeviceSpecSource> deviceSpecs = JsonConvert.DeserializeObject<List<DeviceSpecSource>>(response);
                //deviceSpecs = deviceSpecs.OrderBy(p => p.Id).ToList();


                var response = await _httpClient.GetStringAsync("boxgroup");

                List<BoxGroup> boxGroups = JsonConvert.DeserializeObject<List<BoxGroup>>(response);
                if (boxGroups == null) return false;

                foreach (var boxGroup in boxGroups)
                {
                    var boxes = boxGroup.BoxRegs;
                    if (boxes == null) continue;
                    foreach (var box in boxes)
                    {
                        var sessionId = box.Box.CurrentSessionId;
                        var baseUrl = box.Box.CommServer.ApiBaseUrl;
                        var signalrUrl = box.Box.CommServer.SignalRUrl;
                        var boxUid = box.Box.Uid;
                        var boxNo = box.Box.BoxNo;
                        var connectionState = box.Box.ConnectionState;

                        if (boxNo != MachineId) continue;
                        if (connectionState != 1)
                        {
                            _connected = false;
                            return false;
                        }

                        _httpClient2 = new HttpClient
                        {
                            BaseAddress = new Uri(baseUrl)
                        };
                        _httpClient2.SetBearerToken(token);
                        _httpClient2.DefaultRequestHeaders.Add("X-FBox-ClientId", guid);

                        response = await _httpClient2.GetStringAsync("box/" + box.Box.Uid + "/dmon/def/grouped");


                        List<DMonGroup> dataGroups = JsonConvert.DeserializeObject<List<DMonGroup>>(response);
                        foreach (var dataGroup in dataGroups)
                        {
                            if (dataGroup.Name == LocalSequence)
                            {
                                _dataGroup = dataGroup;
                                break;
                            }
                        }

                        _boxSessionId = sessionId;
                        _boxNo = boxNo;
                        _connectionState = connectionState;

                        _hubConnection = new HubConnection(signalrUrl);
                        _hubConnection.Headers.Add("Authorization", "Bearer " + token);
                        _hubConnection.Headers.Add("X-FBox-ClientId", guid);
                        _hubConnection.Headers.Add("X-FBox-Session", sessionId.ToString());

                        IHubProxy dataHubProxy = _hubConnection.CreateHubProxy("clientHub");
                        dataHubProxy.On<int, List<GetValue>>("dMonUpdateValue",
                            (boxSessionId, values) =>
                            {

//#if DEBUG
                                //Console.WriteLine($"Box session {boxSessionId} return at {DateTime.Now}");
//#endif

                                _timeStamp = DateTime.Now;

                                foreach (var value in values)
                                {
                                    if (value.Status != 0)
                                    {
                                        lock (_data)
                                        {
                                            var dMonEntry =
                                                _dataGroup.DMonEntries.FirstOrDefault(
                                                    p => p.Uid == value.Id);
                                            if (dMonEntry != null)
                                            {
                                                if (_data.ContainsKey(dMonEntry.Desc))
                                                {
                                                    _data.Remove(dMonEntry.Desc);
                                                }
                                            }

                                        }
                                        return;
                                    }
                                    lock (_data)
                                    {
                                        if (_dataGroup.DMonEntries.Any(p => p.Uid == value.Id))
                                        {
                                            if (_data == null)
                                            {
                                                _data = new Dictionary<string, double>();
                                            }

                                            var dMonEntry = _dataGroup.DMonEntries.FirstOrDefault(
                                                p => p.Uid == value.Id);

                                            if (value.Value.HasValue && dMonEntry != null)
                                            {
                                                if (_data.ContainsKey(dMonEntry.Desc))
                                                {
                                                    _data[dMonEntry.Desc] = value.Value.Value;
                                                }
                                                else
                                                {
                                                    _data.Add(dMonEntry.Desc, value.Value.Value);
                                                }
                                            }
                                        }
                                    }
                                }
                            });

                        dataHubProxy.On<int, string, int, int>("boxConnectionStateChanged",
                            async (newConnectionToken, getBoxUid, oldStatus, newStatus) =>
                            {
#if DEBUG
                                Console.WriteLine(
                                $"Box uid {getBoxUid} change state at {DateTime.Now} new connectionToken {newConnectionToken} newStatus {newStatus}");
#endif
                                sessionId = newConnectionToken;
                                _boxSessionId = sessionId;

                                _connectionState = newStatus;

                                if (!IsConnected || _httpClient2 == null || _hubConnection == null)
                                {
                                    Clear();
                                    return;
                                }
                                try
                                {
                                    while (
                                        !_httpClient2.DefaultRequestHeaders.TryAddWithoutValidation("X-FBox-Session",
                                            sessionId.ToString()))
                                    {
                                        _httpClient2.DefaultRequestHeaders.Remove("X-FBox-Session");
                                    }
                                    _httpClient2.DefaultRequestHeaders.Add("X-FBox-Session", sessionId.ToString());

                                    
                                    _hubConnection.Headers["X-FBox-Session"] = sessionId.ToString();
                                    await _hubConnection.Start();

                                    if (newStatus == 1)
                                    {
                                        if (IsConnected)
                                        {
                                            await
                                                _httpClient2.PostAsync(
                                                    "dmon/group/" + _dataGroup.Uid + "/start", null);
                                        }
                                    }

                                    else
                                    {
                                        lock (_data)
                                        {
                                            _data.Clear();
                                        }
                                        //await DisconnectAsync();
                                        //_connected = false;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine("SignalR boxSessionId change error: " + ex.Message);
                                    await DisconnectAsync();
                                }
                            });

                        _hubConnection.Error += async ex =>
                        {
                            Console.WriteLine(@"SignalR error: {0}", ex.Message);
                            await DisconnectAsync();
                            _connected = false;
                        };

                        _hubConnection.Closed += () =>
                        {
                            _hubConnection.Dispose();
                            _connected = false;
                        };

                        ServicePointManager.DefaultConnectionLimit = 10;

                        if (_dataGroup == null) return false;

                        var groupUid = _dataGroup.Uid;
                        var groupName = _dataGroup.Name;

                        if (groupName != "(Default)" && groupName != "默认组" && _connectionState == 1)
                        {
                            _groupUid = groupUid;
                        }
                        if (groupName != "(Default)" && groupName != "默认组" && _connectionState == 1)
                        {
                            _boxUid = boxUid;
                        }

                        _dataType = new Dictionary<string, Type>();
                        if (_dataGroup.DMonEntries != null)
                        {
                            foreach (var dMonEntry in _dataGroup.DMonEntries)
                            {
                                Type type;
                                switch (dMonEntry.DataType)
                                {
                                    //位
                                    case 0:
                                    {
                                        type = typeof (bool);
                                        break;
                                    }
                                    //16位无符号
                                    case 1:
                                    {
                                        type = typeof (ushort);
                                        break;
                                    }
                                    //16位有符号
                                    case 2:
                                    {
                                        type = typeof (short);
                                        break;
                                    }
                                    //32位无符号
                                    case 11:
                                    {
                                        type = typeof (uint);
                                        break;
                                    }
                                    //32位有符号
                                    case 12:
                                    {
                                        type = typeof (int);
                                        break;
                                    }
                                    //16位BCD
                                    case 3:
                                    {
                                        type = typeof (short);
                                        break;
                                    }
                                    //32位BCD
                                    case 13:
                                    {
                                        type = typeof (int);
                                        break;
                                    }
                                    //浮点数
                                    case 16:
                                    {
                                        type = typeof (float);
                                        break;
                                    }
                                    //16位16进制
                                    case 4:
                                    {
                                        type = typeof (short);
                                        break;
                                    }
                                    //32位16进制
                                    case 14:
                                    {
                                        type = typeof (int);
                                        break;
                                    }
                                    //16位2进制
                                    case 5:
                                    {
                                        type = typeof (short);
                                        break;
                                    }
                                    //32位2进制
                                    case 15:
                                    {
                                        type = typeof (int);
                                        break;
                                    }
                                    default:
                                    {
                                        type = typeof (short);
                                        break;
                                    }
                                }

                                if (!_dataType.ContainsKey(dMonEntry.Desc))
                                {
                                    _dataType.Add(dMonEntry.Desc, type);
                                }
                                else
                                {
                                    _dataType[dMonEntry.Desc] = type;
                                }
                            }
                        }

                        await _hubConnection.Start();
                        await dataHubProxy.Invoke("updateClientId", guid);

                        return true;
                    }
                }
                return false;
            }
            catch
            {
                Clear();
                return false;
            }
        }

        public override bool Disconnect()
        {
            return AsyncHelper.RunSync(DisconnectAsync);
        }

        public async Task<bool> DisconnectAsync()
        {
            try
            {
                if (_httpClient2 != null)
                {
                    await
                        _httpClient2.PostAsync(
                            "dmon/group/" + _groupUid + "/stop",
                            null);
                }
                Clear();
                Console.WriteLine("SignalR Disconnect success");
                return true;

            }
            catch (Exception e)
            {
                Console.WriteLine("SignalR Disconnect failed " + e.Message);
                Clear();
                return false;
            }
        }

        private async void Clear()
        {
            try
            {
                if (_hubConnection != null)
                {
                    await Task.Run(() => _hubConnection.Stop(TimeSpan.FromSeconds(10)));
                    _hubConnection = null;
                }
            }
            catch (Exception)
            {
                _hubConnection = null;
                // ignored
            }
            try
            {
                if (_httpClient != null)
                {
                    _httpClient.Dispose();
                    _httpClient = null;
                }
            }
            catch (Exception)
            {
                //ignore
            }
            try
            {
                if (_httpClient2 != null)
                {
                    _httpClient2.Dispose();
                    _httpClient2 = null;
                }
            }
            catch (Exception)
            {
                //ignore
            }
            if (_data != null)
            {
                lock (_data)
                {
                    _data.Clear();
                    _dataType.Clear();
                }
            }
            _timeStamp = DateTime.MinValue;
            _connected = false;
            try
            {
                if (_timer != null)
                {
                    _timer.Dispose();
                    _timer = null;
                }
            }
            catch (Exception)
            {
                //ignore
            }
        }

        public override bool SendMsgWithoutReturn(byte[] message)
        {
            throw new NotImplementedException();
        }

        public override Task<bool> SendMsgWithoutReturnAsync(byte[] message)
        {
            throw new NotImplementedException();
        }

        public override byte[] SendMsg(byte[] message)
        {
            return AsyncHelper.RunSync(() => SendMsgAsync(message));
        }

        public override async Task<byte[]> SendMsgAsync(byte[] message)
        {
            if (_httpClient == null)
            {
                await DisconnectAsync();
                _connected = false;
                return null;
            }

            if (_hubConnection.State == ConnectionState.Disconnected)
            {
                await _hubConnection.Start();
            }

            var formater = new AddressFormaterFBox();
            var translator = new AddressTranslatorFBox();

            byte[] ans;

            //if (_connectionState != 1)
            //{
            //await DisconnectAsync();
            //_connected = false;
            //Console.WriteLine($"Return Value Rejected with connectionToken {ConnectionToken}");
            //return null;
            //}

            if (_timeStamp == DateTime.MinValue)
            {
                return Encoding.ASCII.GetBytes("NoData");
            }

            if (DateTime.Now - _timeStamp > TimeSpan.FromMinutes(2))
            {
                Console.WriteLine("SignalR Timeout: {0} {1} {2}", _timeStamp, DateTime.Now, ConnectionToken);
                if (_connectionState != 1)
                {
                    await DisconnectAsync();
                    _connected = false;
                    return null;
                }
            }

            Dictionary<string, double> machineDataValue;
            lock (_data)
            {
                machineDataValue = _data.ToDictionary(pair => pair.Key, pair => pair.Value);
            }
            var machineDataType = _dataType;
            if (machineDataType == null || machineDataType.Count == 0)
            {
                await DisconnectAsync();
                _connected = false;
                Console.WriteLine($"Return Value Rejected with connectionToken {ConnectionToken}");
                return null;
            }

            if (machineDataValue == null || machineDataValue.Count == 0)
            {
                return Encoding.ASCII.GetBytes("NoData");
            }

            int pos = 0;
            int area = BigEndianValueHelper.Instance.GetInt(message, ref pos);
            int address = BigEndianValueHelper.Instance.GetInt(message, ref pos);
            //short count = BigEndianValueHelper.Instance.GetShort(message, ref pos);              
            object[] dataAns = new object[1];
            try
            {
                dataAns[0] =
                    Convert.ChangeType(
                        machineDataValue[formater.FormatAddress(translator.GetAreaName(area), address)],
                        machineDataType[formater.FormatAddress(translator.GetAreaName(area), address)]);
            }
            catch (Exception)
            {
                if (machineDataValue.Count != machineDataType.Count)
                {
                    await
                        _httpClient2.PostAsync(
                            "dmon/group/" + _dataGroup.Uid + "/start", null);
                }
                return Encoding.ASCII.GetBytes("NoData");
                //dataAns[0] =
                //Convert.ChangeType(
                //0,
                //machineDataType[formater.FormatAddress(translator.GetAreaName(area), address)]);
            }
            finally
            {
                ans = BigEndianValueHelper.Instance.ObjectArrayToByteArray(dataAns);
            }

            return ans;
        }
    }
}

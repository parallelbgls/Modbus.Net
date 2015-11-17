using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DelinRemoteControlBoxTest;
using Microsoft.AspNet.SignalR.Client;
using Newtonsoft.Json;
using Thinktecture.IdentityModel.Client;

namespace ModBus.Net.FBox
{
    public class SignalRSigninMsg
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string UserId { get; set; }
        public string Password { get; set; }
        public string SigninAdditionalValues { get; set; }
        public SignalRServer SignalRServer { get; set; }
    }

    public class SignalRConnector : BaseConnector
    {
        private static OAuth2Client _oauth2;
        private HttpClient _httpClient;
        private readonly Dictionary<string, HttpClient> _httpClient2;
        private readonly Dictionary<string, HubConnection> _hubConnections;

        private Dictionary<string, string> GroupNameUid { get; }       
        private static Dictionary<string, Dictionary<string, double>> _machineData;
        private static Dictionary<string, Dictionary<string, Type>> _machineDataType; 

        public override string ConnectionToken { get; }

        private bool _connected;
        public override bool IsConnected { get { return _connected; } }

        private Constants _constants;
        private Constants Constants
        {
            get
            {
                if (_constants == null) _constants = new Constants();
                return _constants;
            }

        }

        public SignalRConnector(string machineId, SignalRSigninMsg msg)
        {
            Constants.SignalRServer = msg.SignalRServer;
            System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            ConnectionToken = machineId;
            if (_oauth2 == null)
            {
                _oauth2 = new OAuth2Client(
                    new Uri(Constants.TokenEndpoint),
                    msg.ClientId, 
                    msg.ClientSecret 
                );
                _hubConnections = new Dictionary<string, HubConnection>();
                _httpClient2 = new Dictionary<string, HttpClient>();
                _machineData = new Dictionary<string, Dictionary<string, double>>();
                _machineDataType = new Dictionary<string, Dictionary<string, Type>>();
                GroupNameUid = new Dictionary<string, string>();               
                var tokenResponse = _oauth2.RequestResourceOwnerPasswordAsync
                    (
                        msg.UserId,
                        msg.Password,
                        msg.SigninAdditionalValues
                    ).Result;
                if (tokenResponse != null)
                    AsyncHelper.RunSync(()=>CallService(tokenResponse.AccessToken));
                
            }
        }

        public override bool Connect()
        {
            return AsyncHelper.RunSync(ConnectAsync);
        }

        public override async Task<bool> ConnectAsync()
        {
            try
            {
                if (_hubConnections.ContainsKey(ConnectionToken) && _httpClient2.ContainsKey(ConnectionToken) && GroupNameUid.ContainsKey(ConnectionToken))
                {
                    await _httpClient2[ConnectionToken].PostAsync("dmon/group/" + GroupNameUid[ConnectionToken] + "/start",
                        null);
                    _connected = true;
                    Console.WriteLine("SignalR Connected success");
                    return true;
                }
                else
                {
                    Console.WriteLine("SignalR Connected failed");
                    return false;
                }
            }
            catch
            {
                Console.WriteLine("SignalR Connected failed");
                return false;
            }         
        }

        private async Task CallService(string token)
        {
            var guid = Guid.NewGuid().ToString();

            var baseAddress = Constants.AspNetWebApiSampleApi;

            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(baseAddress)
            };

            _httpClient.SetBearerToken(token);

            /*var response = await _httpClient.GetStringAsync("device/spec");

            List<DeviceSpecSource> deviceSpecs = JsonConvert.DeserializeObject<List<DeviceSpecSource>>(response);
            deviceSpecs = deviceSpecs.OrderBy(p => p.Id).ToList();
            */
            var response = await _httpClient.GetStringAsync("boxgroup");

            List<BoxGroup> boxGroups = JsonConvert.DeserializeObject<List<BoxGroup>>(response);

            foreach (var boxGroup in boxGroups)
            {
                var boxes = boxGroup.BoxRegs;
                foreach (var box in boxes)
                {
                    var sessionId = box.Box.CurrentSessionId;
                    var baseUrl = box.Box.CommServer.ApiBaseUrl;
                    var signalrUrl = box.Box.CommServer.SignalRUrl;
                    var boxUid = box.Box.Uid;
                    //var currentStat = box.Box.ConnectionState;

                    var client3 = new HttpClient
                    {
                        BaseAddress = new Uri(baseUrl)
                    };
                    client3.SetBearerToken(token);
                    client3.DefaultRequestHeaders.Add("X-FBox-ClientId", guid);

                    response = await client3.GetStringAsync("box/" + box.Box.Uid + "/dmon/def/grouped");

                    List<DMonGroup> dataGroups = JsonConvert.DeserializeObject<List<DMonGroup>>(response);
                    foreach (var dataGroup in dataGroups)
                    {
                        if (dataGroup == null) return;
                        var groupUid = dataGroup.Uid;
                        var groupName = dataGroup.Name;

                        if (groupName != "(Default)" && !GroupNameUid.ContainsKey(groupName))
                        {
                            GroupNameUid.Add(groupName, groupUid);
                        }

                        var client2 = new HttpClient
                        {
                            BaseAddress = new Uri(baseUrl)
                        };
                        if (groupName != "(Default)" && !_httpClient2.ContainsKey(groupName))
                        {
                            _httpClient2.Add(groupName, client2);
                        }

                        client2.SetBearerToken(token);
                        client2.DefaultRequestHeaders.Add("X-FBox-ClientId", guid);

                        var hubConnection = new HubConnection(signalrUrl);
                        if (groupName != "(Default)")
                            _hubConnections.Add(groupName, hubConnection);

                        if (!_machineDataType.ContainsKey(groupName))
                        {
                            _machineDataType.Add(groupName, new Dictionary<string, Type>());
                        }
                        foreach (var dMonEntry in dataGroup.DMonEntries)
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

                            if (!_machineDataType[groupName].ContainsKey(dMonEntry.Desc))
                            {
                                _machineDataType[groupName].Add(dMonEntry.Desc, type);
                            }
                            else
                            {
                                _machineDataType[groupName][dMonEntry.Desc] = type;
                            }
                        }

                        hubConnection.Headers.Add("Authorization", "Bearer " + token);
                        hubConnection.Headers.Add("X-FBox-ClientId", guid);
                        hubConnection.Headers.Add("X-FBox-Session", sessionId.ToString());

                        IHubProxy stockTickerHubProxy = hubConnection.CreateHubProxy("clientHub");
                        stockTickerHubProxy.On<int, List<GetValue>>("dMonUpdateValue",
                            (boxSessionId, values) =>
                            {
                                if (boxSessionId == sessionId)
                                {
                                    foreach (var value in values)
                                    {
                                        lock(_machineData)
                                        {
                                            if (dataGroup.DMonEntries.Any(p => p.Uid == value.Id))
                                            {
                                                if (!_machineData.ContainsKey(groupName))
                                                {
                                                    _machineData.Add(groupName, new Dictionary<string, double>());
                                                }
                                                if (_machineData[groupName] == null)
                                                {
                                                    _machineData[groupName] = new Dictionary<string, double>();
                                                }

                                                var dMonEntry =
                                                    dataGroup.DMonEntries.FirstOrDefault(p => p.Uid == value.Id);

                                                if (value.Value.HasValue && dMonEntry != null)
                                                {
                                                    if (_machineData[groupName].ContainsKey(dMonEntry.Desc))
                                                    {
                                                        _machineData[groupName][dMonEntry.Desc] = value.Value.Value;
                                                    }
                                                    else
                                                    {
                                                        _machineData[groupName].Add(dMonEntry.Desc, value.Value.Value);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            );
                        stockTickerHubProxy.On<int, string, int, int>("boxConnectionStateChanged",
                            async (newConnectionToken, getBoxUid, oldStatus, newStatus) =>
                            {
                                if (getBoxUid == boxUid)
                                {
                                    sessionId = newConnectionToken;
                                    client2.DefaultRequestHeaders.Remove("X-FBox-Session");
                                    client2.DefaultRequestHeaders.Add("X-FBox-Session", sessionId.ToString());
                                    if (newStatus == 1 && IsConnected)
                                    {
                                        await client2.PostAsync("dmon/group/" + groupUid + "/start", null);
                                    }
                                    else
                                    {
                                        if (_machineData.ContainsKey(groupName))
                                        {
                                            _machineData.Remove(groupName);
                                        }
                                        await client2.PostAsync("dmon/group/" + groupUid + "/stop", null);
                                    }
                                }
                            }
                            );
                        hubConnection.Error += ex => Console.WriteLine(@"SignalR error: {0}", ex.Message);
                        ServicePointManager.DefaultConnectionLimit = 10;
                        await hubConnection.Start();
                        await stockTickerHubProxy.Invoke("updateClientId", guid);

                        client2.DefaultRequestHeaders.Add("X-FBox-Session", sessionId.ToString());
                    }
                }            
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
                if (_hubConnections.ContainsKey(ConnectionToken) && _httpClient2.ContainsKey(ConnectionToken) && GroupNameUid.ContainsKey(ConnectionToken))
                {
                    await _httpClient2[ConnectionToken].PostAsync("dmon/group/" + GroupNameUid[ConnectionToken] + "/stop",
                        null);
                    _connected = false;
                    Console.WriteLine("SignalR Disconnect success");
                    return true;
                }
                else
                {
                    Console.WriteLine("SignalR Disconnect failed");
                    return false;
                }
            }
            catch
            {
                Console.WriteLine("SignalR Disconnect failed");
                return false;
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
            var formater = new AddressFormaterFBox();
            var translator = new AddressTranslatorFBox();
            
            byte[] ans;

            lock (_machineData)
            {
                if (!_machineData.ContainsKey(ConnectionToken) || !_machineDataType.ContainsKey(ConnectionToken))
                {
                    return null;
                }
                var machineDataValue = _machineData[ConnectionToken];
                var machineDataType = _machineDataType[ConnectionToken];
                int pos = 0;
                int area = ValueHelper.Instance.GetInt(message, ref pos);
                int address = ValueHelper.Instance.GetInt(message, ref pos);
                //short count = ValueHelper.Instance.GetShort(message, ref pos);              
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
                    dataAns[0] =
                        Convert.ChangeType(
                            0,
                            machineDataType[formater.FormatAddress(translator.GetAreaName(area), address)]);
                }
                finally
                {
                    ans = ValueHelper.Instance.ObjectArrayToByteArray(dataAns);
                }
            }
            
            return ans;
        }

        public override Task<byte[]> SendMsgAsync(byte[] message)
        {
            return Task.Factory.StartNew(() => SendMsg(message));
        }
    }
}

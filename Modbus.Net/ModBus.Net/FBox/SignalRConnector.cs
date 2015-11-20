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
    public struct SignalRSigninMsg
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
        private static Dictionary<SignalRSigninMsg, OAuth2Client> _oauth2;

        private static Dictionary<SignalRSigninMsg, HttpClient> _httpClient;
        private static Dictionary<string, HttpClient> _httpClient2;
        private static Dictionary<string, HubConnection> _hubConnections;

        private static Dictionary<string, int> _boxUidSessionId; 
        private static Dictionary<string, List<DMonGroup>> _boxUidDataGroups; 
        private static Dictionary<string, string> _groupNameUid;
        private static Dictionary<string, string> _groupNameBoxUid; 
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
                _httpClient = new Dictionary<SignalRSigninMsg, HttpClient>();
                _oauth2 = new Dictionary<SignalRSigninMsg, OAuth2Client>();
                _hubConnections = new Dictionary<string, HubConnection>();
                _httpClient2 = new Dictionary<string, HttpClient>();
                _machineData = new Dictionary<string, Dictionary<string, double>>();
                _machineDataType = new Dictionary<string, Dictionary<string, Type>>();
                _boxUidSessionId = new Dictionary<string, int>();
                _groupNameUid = new Dictionary<string, string>();  
                _groupNameBoxUid = new Dictionary<string, string>();
                _boxUidDataGroups = new Dictionary<string, List<DMonGroup>>();                       
            }

            if (!_oauth2.ContainsKey(msg))
            {
                _oauth2.Add(msg, new OAuth2Client(
                    new Uri(Constants.TokenEndpoint),
                    msg.ClientId,
                    msg.ClientSecret
                    ));
                var tokenResponse = _oauth2[msg].RequestResourceOwnerPasswordAsync
                    (
                        msg.UserId,
                        msg.Password,
                        msg.SigninAdditionalValues
                    ).Result;
                if (tokenResponse != null)
                    AsyncHelper.RunSync(() => CallService(msg, tokenResponse.AccessToken));
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
                if (_hubConnections.ContainsKey(_groupNameBoxUid[ConnectionToken]) && _httpClient2.ContainsKey(_groupNameBoxUid[ConnectionToken]) && _groupNameUid.ContainsKey(ConnectionToken))
                {
                    await _httpClient2[_groupNameBoxUid[ConnectionToken]].PostAsync("dmon/group/" + _groupNameUid[ConnectionToken] + "/start",
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

        private async Task CallService(SignalRSigninMsg msg, string token)
        {
            var guid = Guid.NewGuid().ToString();

            var baseAddress = Constants.AspNetWebApiSampleApi;

            if (!_httpClient.ContainsKey(msg))
            {
                _httpClient.Add(msg, new HttpClient
                {
                    BaseAddress = new Uri(baseAddress)
                });
            }

            _httpClient[msg].SetBearerToken(token);

            /*var response = await _httpClient.GetStringAsync("device/spec");

            List<DeviceSpecSource> deviceSpecs = JsonConvert.DeserializeObject<List<DeviceSpecSource>>(response);
            deviceSpecs = deviceSpecs.OrderBy(p => p.Id).ToList();
            */

            var response = await _httpClient[msg].GetStringAsync("boxgroup");

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

                    var client2 = new HttpClient
                    {
                        BaseAddress = new Uri(baseUrl)
                    };
                    client2.SetBearerToken(token);
                    client2.DefaultRequestHeaders.Add("X-FBox-ClientId", guid);

                    response = await client2.GetStringAsync("box/" + box.Box.Uid + "/dmon/def/grouped");

                    List<DMonGroup> dataGroups = JsonConvert.DeserializeObject<List<DMonGroup>>(response);
                    _boxUidDataGroups.Add(boxUid, dataGroups);

                    var hubConnection = new HubConnection(signalrUrl);
                    _hubConnections.Add(boxUid, hubConnection);
                    hubConnection.Headers.Add("Authorization", "Bearer " + token);
                    hubConnection.Headers.Add("X-FBox-ClientId", guid);
                    hubConnection.Headers.Add("X-FBox-Session", sessionId.ToString());

                    IHubProxy dataHubProxy = hubConnection.CreateHubProxy("clientHub");
                    dataHubProxy.On<int, List<GetValue>>("dMonUpdateValue",
                        (boxSessionId, values) =>
                        {
                            if (_boxUidSessionId.ContainsValue(boxSessionId))
                            {
                                var localBoxUid = _boxUidSessionId.FirstOrDefault(p => p.Value == boxSessionId).Key;
                                foreach (var value in values)
                                {
                                    lock (_machineData)
                                    {
                                        if (_boxUidDataGroups.ContainsKey(localBoxUid))
                                        {
                                            foreach (var dataGroupInner in _boxUidDataGroups[localBoxUid])
                                            {
                                                if (dataGroupInner.DMonEntries.Any(p => p.Uid == value.Id))
                                                {
                                                    if (!_machineData.ContainsKey(dataGroupInner.Name))
                                                    {
                                                        _machineData.Add(dataGroupInner.Name,
                                                            new Dictionary<string, double>());
                                                    }
                                                    if (_machineData[dataGroupInner.Name] == null)
                                                    {
                                                        _machineData[dataGroupInner.Name] =
                                                            new Dictionary<string, double>();
                                                    }

                                                    var dMonEntry =
                                                        dataGroupInner.DMonEntries.FirstOrDefault(p => p.Uid == value.Id);

                                                    if (value.Value.HasValue && dMonEntry != null)
                                                    {
                                                        if (_machineData[dataGroupInner.Name].ContainsKey(dMonEntry.Desc))
                                                        {
                                                            _machineData[dataGroupInner.Name][dMonEntry.Desc] =
                                                                value.Value.Value;
                                                        }
                                                        else
                                                        {
                                                            _machineData[dataGroupInner.Name].Add(dMonEntry.Desc,
                                                                value.Value.Value);
                                                        }
                                                    }
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        });

                    dataHubProxy.On<int, string, int, int>("boxConnectionStateChanged",
                        async (newConnectionToken, getBoxUid, oldStatus, newStatus) =>
                        {
                            if (_httpClient2.ContainsKey(getBoxUid))
                            {
                                sessionId = newConnectionToken;
                                _boxUidSessionId[getBoxUid] = sessionId;
                                _httpClient2[getBoxUid].DefaultRequestHeaders.Remove("X-FBox-Session");
                                _httpClient2[getBoxUid].DefaultRequestHeaders.Add("X-FBox-Session", sessionId.ToString());

                                if (newStatus == 1 && IsConnected)
                                {
                                    var localDataGroups = _boxUidDataGroups[getBoxUid];
                                    foreach (var localDataGroup in localDataGroups)
                                    {
                                        await
                                            _httpClient2[getBoxUid].PostAsync(
                                                "dmon/group/" + localDataGroup.Uid + "/start", null);
                                    }
                                }
                                else
                                {
                                    var localDataGroups = _boxUidDataGroups[getBoxUid];
                                    foreach (var localDataGroup in localDataGroups)
                                    {
                                        if (_machineData.ContainsKey(localDataGroup.Name))
                                        {
                                            _machineData.Remove(localDataGroup.Name);
                                        }
                                        await
                                            _httpClient2[getBoxUid].PostAsync(
                                                "dmon/group/" + localDataGroup.Uid + "/stop", null);
                                    }
                                }
                            }
                        }
                        );

                    hubConnection.Error += ex => Console.WriteLine(@"SignalR error: {0}", ex.Message);
                    ServicePointManager.DefaultConnectionLimit = 10;

                    foreach (var dataGroup in dataGroups)
                    {
                        if (dataGroup == null) return;
                        _boxUidSessionId.Add(boxUid, sessionId);
                        var groupUid = dataGroup.Uid;
                        var groupName = dataGroup.Name;

                        if (groupName != "(Default)" && !_groupNameUid.ContainsKey(groupName))
                        {
                            _groupNameUid.Add(groupName, groupUid);
                        }
                        if (groupName != "(Default)" && !_groupNameBoxUid.ContainsKey(groupName))
                        {
                            _groupNameBoxUid.Add(groupName, boxUid);
                        }
                        if (groupName != "(Default)" && !_httpClient2.ContainsKey(boxUid))
                        {
                            _httpClient2.Add(boxUid, client2);
                        }



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
                    }

                    await hubConnection.Start();
                    await dataHubProxy.Invoke("updateClientId", guid);
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
                if (_hubConnections.ContainsKey(_groupNameBoxUid[ConnectionToken]) && _httpClient2.ContainsKey(_groupNameBoxUid[ConnectionToken]) && _groupNameUid.ContainsKey(ConnectionToken))
                {
                    await _httpClient2[_groupNameBoxUid[ConnectionToken]].PostAsync("dmon/group/" + _groupNameUid[ConnectionToken] + "/stop",
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

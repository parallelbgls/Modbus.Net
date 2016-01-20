using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
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
        private static Dictionary<SignalRSigninMsg, string> _refreshToken; 

        private static Dictionary<SignalRSigninMsg, HttpClient> _httpClient;
        private static Dictionary<string, HttpClient> _httpClient2;
        private static Dictionary<string, HubConnection> _hubConnections;
        private static Dictionary<string, SignalRSigninMsg> _boxUidMsg; 
        private static Dictionary<string, int> _boxUidSessionId; 
        private static Dictionary<string, List<DMonGroup>> _boxUidDataGroups;
        private static Dictionary<string, int> _connectionTokenState;  
        private static Dictionary<string, string> _groupNameUid;
        private static Dictionary<string, string> _groupNameBoxUid; 
        private static Dictionary<string, Dictionary<string, double>> _machineData;
        private static Dictionary<string, Dictionary<string, Type>> _machineDataType; 
        private static Dictionary<string, string> _boxUidBoxNo;
        private static HashSet<string> _connectedDataGroupUid;

        public override string ConnectionToken { get; }

        private Timer _timer;

        private string MachineId => ConnectionToken.Split(',')[0];

        private string LocalSequence => ConnectionToken.Split(',')[1];

        private static readonly AsyncLock _lock = new AsyncLock();

        private bool _connected;
        public override bool IsConnected => _connected;
        private SignalRSigninMsg Msg { get; set;}

        private Constants _constants;
        private Constants Constants => _constants ?? (_constants = new Constants());

        public SignalRConnector(string machineId, string localSequence, SignalRSigninMsg msg)
        {
            Constants.SignalRServer = msg.SignalRServer;
            System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            ConnectionToken = machineId + "," + localSequence;
            if (_oauth2 == null)
            {
                _httpClient = new Dictionary<SignalRSigninMsg, HttpClient>();
                _oauth2 = new Dictionary<SignalRSigninMsg, OAuth2Client>();
                _refreshToken = new Dictionary<SignalRSigninMsg, string>();
                _boxUidMsg = new Dictionary<string, SignalRSigninMsg>();
                _hubConnections = new Dictionary<string, HubConnection>();
                _httpClient2 = new Dictionary<string, HttpClient>();
                _machineData = new Dictionary<string, Dictionary<string, double>>();
                _machineDataType = new Dictionary<string, Dictionary<string, Type>>();
                _boxUidSessionId = new Dictionary<string, int>();
                _connectionTokenState = new Dictionary<string, int>();
                _groupNameUid = new Dictionary<string, string>();  
                _groupNameBoxUid = new Dictionary<string, string>();
                _boxUidDataGroups = new Dictionary<string, List<DMonGroup>>();                       
                _boxUidBoxNo = new Dictionary<string, string>();
                _connectedDataGroupUid = new HashSet<string>();
                _timer = new Timer(ChangeToken, null, 3600 * 1000 * 4, 3600 * 1000 * 4);                     
            }
            Msg = msg;
            }

        private async void ChangeToken(object sender)
        {
            try
            {
                using (await _lock.LockAsync())
                {
                    var tokenResponse = await _oauth2[Msg].RequestRefreshTokenAsync(_refreshToken[Msg]);
                    _refreshToken[Msg] = tokenResponse.RefreshToken;
                    _httpClient[Msg].SetBearerToken(tokenResponse.AccessToken);

                    foreach (var boxUidMsg in _boxUidMsg)
                    {
                        if (boxUidMsg.Value.Equals(Msg))
                        {
                            if (_httpClient2.ContainsKey(boxUidMsg.Key) && _hubConnections.ContainsKey(boxUidMsg.Key))
                                _httpClient2[boxUidMsg.Key].SetBearerToken(tokenResponse.AccessToken);
                            _hubConnections[boxUidMsg.Key].Stop();
                            _hubConnections[boxUidMsg.Key].Headers["Authorization"] = "Bearer " +
                                                                                      tokenResponse.AccessToken;
                            await _hubConnections[boxUidMsg.Key].Start();
                            var localDataGroups = _boxUidDataGroups[boxUidMsg.Key];
                            foreach (var localDataGroup in localDataGroups)
                            {
                                if (_connectedDataGroupUid.Contains(localDataGroup.Uid))
                                {
                                    await
                                        _httpClient2[boxUidMsg.Key].PostAsync(
                                            "dmon/group/" + localDataGroup.Uid + "/start", null);
                                }
                            }
                        }
                    }
                }
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
                using (await _lock.LockAsync())
                {
                    if (!_oauth2.ContainsKey(Msg))
                    {
                        _oauth2.Add(Msg, new OAuth2Client(
                            new Uri(Constants.TokenEndpoint),
                                Msg.ClientId,
                                Msg.ClientSecret
                            ));
                        var tokenResponse = await _oauth2[Msg].RequestResourceOwnerPasswordAsync
                            (
                                Msg.UserId,
                                Msg.Password,
                                Msg.SigninAdditionalValues
                            );
                        if (tokenResponse != null)
                        {
                            _refreshToken.Add(Msg, tokenResponse.RefreshToken);
                            AsyncHelper.RunSync(()=> CallService(Msg, tokenResponse.AccessToken));                        
                        }                   
                    }

                
                    if (_groupNameBoxUid.ContainsKey(ConnectionToken) && _hubConnections.ContainsKey(_groupNameBoxUid[ConnectionToken]) &&
                        _httpClient2.ContainsKey(_groupNameBoxUid[ConnectionToken]) &&
                        _groupNameUid.ContainsKey(ConnectionToken))
                    {
                        await
                            _httpClient2[_groupNameBoxUid[ConnectionToken]].PostAsync(
                                "dmon/group/" + _groupNameUid[ConnectionToken] + "/start",
                                null);
                        _connectedDataGroupUid.Add(_groupNameUid[ConnectionToken]);
                        _connected = true;
                        Console.WriteLine("SignalR Connected success");
                        return true;
                    }
                    else
                    {
                        _connected = false;
                        Console.WriteLine("SignalR Connected failed");
                        return false;
                    }
                }
            }
            catch (Exception e)
            {
                if (_oauth2.ContainsKey(Msg))
            {
                    _oauth2.Remove(Msg);
                }
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

            //var response = await _httpClient.GetStringAsync("device/spec");

            //List<DeviceSpecSource> deviceSpecs = JsonConvert.DeserializeObject<List<DeviceSpecSource>>(response);
            //deviceSpecs = deviceSpecs.OrderBy(p => p.Id).ToList();
                

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
                    var boxNo = box.Box.BoxNo;

                    //var currentStat = box.Box.ConnectionState;

                    var client2 = new HttpClient
                    {
                        BaseAddress = new Uri(baseUrl)
                    };
                    client2.SetBearerToken(token);
                    client2.DefaultRequestHeaders.Add("X-FBox-ClientId", guid);

                    response = await client2.GetStringAsync("box/" + box.Box.Uid + "/dmon/def/grouped");

                    if (_boxUidDataGroups.ContainsKey(boxUid))
                    {
                        break;
                    }

                    List<DMonGroup> dataGroups = JsonConvert.DeserializeObject<List<DMonGroup>>(response);
                    _boxUidDataGroups.Add(boxUid, dataGroups);
                    lock (_boxUidSessionId)
                    {
                        _boxUidSessionId.Add(boxUid, sessionId);
                    }
                    _boxUidBoxNo.Add(boxUid, boxNo);
                    _boxUidMsg.Add(boxUid, Msg);

                    var hubConnection = new HubConnection(signalrUrl);
                    _hubConnections.Add(boxUid, hubConnection);
                    hubConnection.Headers.Add("Authorization", "Bearer " + token);
                    hubConnection.Headers.Add("X-FBox-ClientId", guid);
                    hubConnection.Headers.Add("X-FBox-Session", sessionId.ToString());

                    IHubProxy dataHubProxy = hubConnection.CreateHubProxy("clientHub");
                    dataHubProxy.On<int, List<GetValue>>("dMonUpdateValue",
                        (boxSessionId, values) =>
                        {
                            lock (_boxUidSessionId)
                            {
                                if (_boxUidSessionId.ContainsValue(boxSessionId))
                                {
                                    Console.WriteLine($"Box session {boxSessionId} return at {DateTime.Now}");
                                    var localBoxUid =
                                        _boxUidSessionId.FirstOrDefault(p => p.Value == boxSessionId).Key;
                                    var localBoxNo = _boxUidBoxNo[localBoxUid];

                                    foreach (var value in values)
                                    {
                                        if (value.Status != 0)
                                        {
                                            lock (_machineData)
                                            {
                                                if (_boxUidDataGroups.ContainsKey(localBoxUid))
                                                {
                                                    foreach (var dataGroupInner in _boxUidDataGroups[localBoxUid])
                                                    {
                                                        var dMonEntry =
                                                            dataGroupInner.DMonEntries.FirstOrDefault(
                                                                p => p.Uid == value.Id);
                                                        if (dMonEntry != null &&
                                                            _machineData.ContainsKey(localBoxNo + "," +
                                                                                     dataGroupInner.Name))
                                                        {
                                                            if (_machineData[localBoxNo + "," + dataGroupInner.Name]
                                                                .ContainsKey(dMonEntry.Desc))
                                                            {
                                                                _machineData[localBoxNo + "," + dataGroupInner.Name]
                                                                    .Remove(dMonEntry.Desc);
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                            return;
                                        }
                                        lock (_machineData)
                                        {
                                            if (_boxUidDataGroups.ContainsKey(localBoxUid))
                                            {
                                                foreach (var dataGroupInner in _boxUidDataGroups[localBoxUid])
                                                {
                                                    if (dataGroupInner.DMonEntries.Any(p => p.Uid == value.Id))
                                                    {
                                                        if (
                                                            !_machineData.ContainsKey(localBoxNo + "," +
                                                                                      dataGroupInner.Name))
                                                        {
                                                            _machineData.Add(localBoxNo + "," + dataGroupInner.Name,
                                                                new Dictionary<string, double>());
                                                        }
                                                        if (_machineData[localBoxNo + "," + dataGroupInner.Name] ==
                                                            null)
                                                        {
                                                            _machineData[localBoxNo + "," + dataGroupInner.Name] =
                                                                new Dictionary<string, double>();
                                                        }

                                                        var dMonEntry =
                                                            dataGroupInner.DMonEntries.FirstOrDefault(
                                                                p => p.Uid == value.Id);

                                                        if (value.Value.HasValue && dMonEntry != null)
                                                        {
                                                            if (
                                                                _machineData[localBoxNo + "," + dataGroupInner.Name]
                                                                    .ContainsKey(
                                                                        dMonEntry.Desc))
                                                            {
                                                                _machineData[localBoxNo + "," + dataGroupInner.Name]
                                                                    [dMonEntry.Desc] =
                                                                    value.Value.Value;
                                                            }
                                                            else
                                                            {
                                                                _machineData[localBoxNo + "," + dataGroupInner.Name]
                                                                    .Add(dMonEntry.Desc,
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
                            }
                        });

                    dataHubProxy.On<int, string, int, int>("boxConnectionStateChanged",
                        async (newConnectionToken, getBoxUid, oldStatus, newStatus) =>
                        {
                            using (await _lock.LockAsync())
                            {
                                if (_httpClient2.ContainsKey(getBoxUid))
                                {
                                    Console.WriteLine(
                                        $"Box uid {getBoxUid} change state at {DateTime.Now} new connectionToken {newConnectionToken} newStatus {newStatus}");
                                    sessionId = newConnectionToken;
                                    lock (_boxUidSessionId)
                                    {
                                        _boxUidSessionId[getBoxUid] = sessionId;
                                    }

                                    var localBoxNo = _boxUidBoxNo[getBoxUid];
                                    var localDataGroups = _boxUidDataGroups[getBoxUid];
                                    lock (_connectionTokenState)
                                    {
                                        foreach (var localDataGroup in localDataGroups)
                                        {
                                            if (
                                                !_connectionTokenState.ContainsKey(localBoxNo + "," +
                                                                                   localDataGroup.Name))
                                            {
                                                _connectionTokenState.Add(localBoxNo + "," + localDataGroup.Name,
                                                    newStatus);
                                            }
                                            else
                                            {
                                                _connectionTokenState[localBoxNo + "," + localDataGroup.Name] =
                                                    newStatus;
                                            }
                                        }
                                    }
                                    _httpClient2[getBoxUid].DefaultRequestHeaders.Remove("X-FBox-Session");
                                    _httpClient2[getBoxUid].DefaultRequestHeaders.Add("X-FBox-Session",
                                        sessionId.ToString());
                                    _hubConnections[getBoxUid].Headers["X-FBox-Session"] = sessionId.ToString();

                                    if (_hubConnections[getBoxUid].State == ConnectionState.Disconnected)
                                    {
                                        await _hubConnections[getBoxUid].Start();
                                    }

                                    if (newStatus == 1)
                                    {
                                        foreach (var localDataGroup in localDataGroups)
                                        {
                                            if (_connectedDataGroupUid.Contains(localDataGroup.Uid))
                                            {
                                                await
                                                    _httpClient2[getBoxUid].PostAsync(
                                                        "dmon/group/" + localDataGroup.Uid + "/start", null);
                                            }
                                        }
                                    }
                                }
                            }
                        });

                    hubConnection.Error += async ex =>
                    {
                        Console.WriteLine(@"SignalR error: {0}", ex.Message);
                        await ConnectRecovery(hubConnection);
                    };

                    hubConnection.Closed += async () =>
                    {
                        await ConnectRecovery(hubConnection);
                    };

                    ServicePointManager.DefaultConnectionLimit = 10;

                    foreach (var dataGroup in dataGroups)
                    {
                        if (dataGroup == null) return;

                        var groupUid = dataGroup.Uid;
                        var groupName = dataGroup.Name;

                        if (groupName != "(Default)" && groupName != "默认组" &&
                            !_connectionTokenState.ContainsKey(boxNo + "," + groupName))
                        {
                            _connectionTokenState.Add(boxNo + "," + groupName, 1);
                        }
                        if (groupName != "(Default)" && groupName != "默认组" &&
                            !_groupNameUid.ContainsKey(boxNo + "," + groupName))
                        {
                            _groupNameUid.Add(boxNo + "," + groupName, groupUid);
                        }
                        if (groupName != "(Default)" && groupName != "默认组" &&
                            !_groupNameBoxUid.ContainsKey(boxNo + "," + groupName))
                        {
                            _groupNameBoxUid.Add(boxNo + "," + groupName, boxUid);
                        }
                        if (groupName != "(Default)" && groupName != "默认组" && !_httpClient2.ContainsKey(boxUid))
                        {
                            _httpClient2.Add(boxUid, client2);
                        }

                        if (!_machineDataType.ContainsKey(boxNo + "," + groupName))
                        {
                            _machineDataType.Add(boxNo + "," + groupName, new Dictionary<string, Type>());
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

                            if (!_machineDataType[boxNo + "," + groupName].ContainsKey(dMonEntry.Desc))
                            {
                                _machineDataType[boxNo + "," + groupName].Add(dMonEntry.Desc, type);
                            }
                            else
                            {
                                _machineDataType[boxNo + "," + groupName][dMonEntry.Desc] = type;
                            }
                        }
                    }

                    await hubConnection.Start();
                    await dataHubProxy.Invoke("updateClientId", guid);
                }

            }
        }

        private async Task ConnectRecovery(HubConnection hubConnection)
        {
            using (await _lock.LockAsync())
            {
                string getBoxUid;
                lock (_boxUidSessionId)
                {
                    getBoxUid =
                        _boxUidSessionId.FirstOrDefault(
                            p => p.Value == int.Parse(hubConnection.Headers["X-FBox-Session"])).Key;
                }
                try
                {
                    if (hubConnection.State != ConnectionState.Connected)
                    {
                        try
                        {
                            hubConnection.Stop();
                        }
                        catch
                        {
                            // ignored
                        }

                        await hubConnection.Start();
                        var localDataGroups = _boxUidDataGroups[getBoxUid];
                        foreach (var localDataGroup in localDataGroups)
                        {
                            if (_connectedDataGroupUid.Contains(localDataGroup.Uid))
                            {
                                await
                                    _httpClient2[getBoxUid].PostAsync(
                                        "dmon/group/" + localDataGroup.Uid + "/start", null);
                            }
                        }
                    }
                }
                catch
                {
                    if (_boxUidBoxNo.ContainsKey(getBoxUid))
                    {
                        var localBoxNo = _boxUidBoxNo[getBoxUid];
                        lock (_machineData)
                        {
                            foreach (var machineDataUnit in _machineData)
                            {
                                if (machineDataUnit.Key.Contains(localBoxNo))
                                {
                                    _machineData.Remove(machineDataUnit.Key);
                                }
                            }
                        }
                    }
                    var localDataGroups = _boxUidDataGroups[getBoxUid];
                    foreach (var localDataGroup in localDataGroups)
                    {
                        _connectedDataGroupUid.RemoveWhere(p => p == localDataGroup.Uid);
                    }
                    _connected = false;
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
                using (await _lock.LockAsync())
                {
                    if (_groupNameBoxUid.ContainsKey(ConnectionToken) && _hubConnections.ContainsKey(_groupNameBoxUid[ConnectionToken]) &&
                        _httpClient2.ContainsKey(_groupNameBoxUid[ConnectionToken]) &&
                        _groupNameUid.ContainsKey(ConnectionToken))
                    {
                        await
                            _httpClient2[_groupNameBoxUid[ConnectionToken]].PostAsync(
                                "dmon/group/" + _groupNameUid[ConnectionToken] + "/stop",
                                null);
                        _connectedDataGroupUid.RemoveWhere(p => p == _groupNameUid[ConnectionToken]);
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
            if (!_httpClient.ContainsKey(Msg))
            {
                _connected = false;
                return null;
            }

            var formater = new AddressFormaterFBox();
            var translator = new AddressTranslatorFBox();
            
            byte[] ans;

            lock (_connectionTokenState)
            {
                if (_connectionTokenState.ContainsKey(ConnectionToken) && _connectionTokenState[ConnectionToken] != 1)
                {
                    _connected = false;
                    Console.WriteLine($"Return Value Rejected with connectionToken {ConnectionToken}");
                    return null;
                }
            }

            lock (_machineData)
            {
                if (!_machineData.ContainsKey(ConnectionToken) || !_machineDataType.ContainsKey(ConnectionToken))
                {
                    _connected = false;
                    Console.WriteLine($"Return Value Rejected with connectionToken {ConnectionToken}");
                    return null;
                }
                var machineDataValue = _machineData[ConnectionToken];
                var machineDataType = _machineDataType[ConnectionToken];
                if (machineDataValue != null && machineDataType.Count == 0)
                {
                    _connected = false;
                    Console.WriteLine($"Return Value Rejected with connectionToken {ConnectionToken}");
                    return null;
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
                    dataAns[0] =
                        Convert.ChangeType(
                            0,
                            machineDataType[formater.FormatAddress(translator.GetAreaName(area), address)]);
                }
                finally
                {
                    ans = BigEndianValueHelper.Instance.ObjectArrayToByteArray(dataAns);
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

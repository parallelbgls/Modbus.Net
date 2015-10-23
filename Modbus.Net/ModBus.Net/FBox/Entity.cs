using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace DelinRemoteControlBoxTest
{
    public class BoxGroup
    {
        [JsonProperty("boxRegs")]
        public List<BoxReg> BoxRegs { get; set; }
        [JsonProperty("children")]
        public List<BoxGroup> Children { get; set; }
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class BoxReg
    {
        [JsonProperty("box")]
        public Box Box { get; set; } 
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("alias")]
        public string Alias { get; set; }
        [JsonProperty("registrationDate")]
        public DateTime RegistrationDate { get; set; }
        [JsonProperty("Favorite")]
        public bool Favorite { get; set; }
    }

    public class Box
    {
        [JsonProperty("commServer")]
        public CommServer CommServer { get; set; }
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("uid")]
        public string Uid { get; set; }
        [JsonProperty("boxNo")]
        public string BoxNo { get; set; }
        [JsonProperty("connectionState")]
        public int ConnectionState { get; set; }
        [JsonProperty("allowedCommServerIds")]
        public List<int> AllowedCommserverIds { get; set; }
        [JsonProperty("currentSessionID")]
        public int CurrentSessionId { get; set; }
    }

    public class CommServer
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("serverId")]
        public int ServerId { get; set; }
        [JsonProperty("apiBaseUrl")]
        public string ApiBaseUrl { get; set; }
        [JsonProperty("signalrUrl")]
        public string SignalRUrl { get; set; }
        [JsonProperty("state")]
        public int State { get; set; }
        [JsonProperty("disabled")]
        public bool Disabled { get; set; }
    }

    public class GetValue
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("status")]
        public int Status { get; set; }
        [JsonProperty("value")]
        public double? Value { get; set; }
    }

    public class DMonGroup
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("dMonEntries")]
        public List<DMonEntry> DMonEntries { get; set; }
        [JsonProperty("uid")]
        public string Uid { get; set; }
    }

    public class DMonEntry
    {
        [JsonProperty("src")] 
        public DMonSource Source { get; set; }
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("uid")]
        public string Uid { get; set; }
        [JsonProperty("fracDigits")]
        public int FracDigits { get; set; }
        [JsonProperty("intDigits")]
        public int IntDigits { get; set; }
        [JsonProperty("padLeft")]
        public bool PadLeft { get; set; }
        [JsonProperty("padRight")]
        public bool PadRight { get; set; }
        [JsonProperty("updateInterval")]
        public int UpdateInterval { get; set; }
        [JsonProperty("privilege")]
        public int Privilege { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("desc")]
        public string Desc { get; set; }
        [JsonProperty("dataType")]
        public int DataType { get; set; }
    }

    public class DMonSource
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("uid")]
        public string Uid { get; set; }
        [JsonProperty("UpdateInterval")]
        public int UpdateInterval { get; set; }
        [JsonProperty("isDMon")]
        public bool IsDMon { get; set; }
        [JsonProperty("isAlarm")]
        public bool IsAlarm { get; set; }
        [JsonProperty("flag")]
        public int Flag { get; set; }
        [JsonProperty("regWidth")]
        public int RegWidth { get; set; }
        [JsonProperty("regId")]
        public int RegId { get; set; }
        [JsonProperty("mainAddr")]
        public int MainAddr { get; set; }
        [JsonProperty("subAddr")]
        public int SubAddr { get; set; }
        [JsonProperty("subIndex")]
        public int SubIndex { get; set; }
        [JsonProperty("serverId")]
        public int ServerId { get; set; }
        [JsonProperty("portNo")]
        public int PortNo { get; set; }
        [JsonProperty("stationNo")]
        public int StationNo { get; set; }
        [JsonProperty("deviceId")]
        public int DeviceId { get; set; }
        [JsonProperty("ip")]
        public string Ip { get; set; }
        [JsonProperty("port")]
        public int Port { get; set; }
    }

    public class DeviceSpecSource
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("defaultStationNo")]
        public int DefaultStationNo { get; set; }
        [JsonProperty("minStationNo")]
        public int MinStationNo { get; set; }
        [JsonProperty("maxStationNo")]
        public int MaxStationNo { get; set; }
        [JsonProperty("class")]
        public int Class { get; set; }
        [JsonProperty("comPortParams")]
        public ComPortParam ComPortParams { get; set; }
        [JsonProperty("ethParams")]
        public EthParam EthParams { get; set; }
        [JsonProperty("byteOrders")]
        public ByteOrder ByteOrders { get; set; }
        [JsonProperty("supportedPlcs")]
        public List<string> SupportedPlcs { get; set; }
        [JsonProperty("regs")]
        public List<AddressTypeReg> Regs { get; set; }
        [JsonProperty("boardcastNo")]
        public int BoardcastNo { get; set; }
        [JsonProperty("mfr")]
        public string Mfr { get; set; }
        [JsonProperty("connType")]
        public int ConnType { get; set; }
        [JsonProperty("driverFileMd5")]
        public string DriverFileMd5 { get; set; }
    }

    public class ComPortParam
    {
        [JsonProperty("baudRate")]
        public int BaudRate { get; set; }
        [JsonProperty("dataBits")]
        public int DataBits { get; set; }
        [JsonProperty("stopBits")]
        public int StopBits { get; set; }
        [JsonProperty("parity")]
        public int Parity { get; set; }
        [JsonProperty("workingMode")]
        public int WorkingMode { get; set; }
        [JsonProperty("plcResponseTimeout")]
        public int PlcResponseTimeout { get; set; }
        [JsonProperty("protocalTimeout1")]
        public int ProtocalTimeout1 { get; set; }
        [JsonProperty("protocalTimeout2")]
        public int ProtocalTimeout2 { get; set; }
        [JsonProperty("maxPacketsWordReg")]
        public int MaxPacketsWordReg { get; set; }
        [JsonProperty("maxPacketsBitReg")]
        public int MaxPacketsBitReg { get; set; }
        [JsonProperty("assembleIntervalBitReg")]
        public int AssembleIntervalBitReg { get; set; }
        [JsonProperty("listRead")]
        public bool ListRead { get; set; }
        [JsonProperty("maxList")]
        public int MaxList { get; set; }
        [JsonProperty("protocalInterval")]
        public int ProtocalInterval { get; set; }
    }

    public class EthParam
    {
        [JsonProperty("ip")]
        public string Ip { get; set; }
        [JsonProperty("port")]
        public int Port { get; set; }
        [JsonProperty("plcResponseTimeout")]
        public int PlcResponseTimeout { get; set; }
        [JsonProperty("protocalTimeout1")]
        public int ProtocalTimeout1 { get; set; }
        [JsonProperty("protocalTimeout2")]
        public int ProtocalTimeout2 { get; set; }
        [JsonProperty("maxPacketsWordReg")]
        public int MaxPacketsWordReg { get; set; }
        [JsonProperty("maxPacketsBitReg")]
        public int MaxPacketsBitReg { get; set; }
        [JsonProperty("assembleIntervalBitReg")]
        public int AssembleIntervalBitReg { get; set; }
        [JsonProperty("listRead")]
        public bool ListRead { get; set; }
        [JsonProperty("maxList")]
        public int MaxList { get; set; }
        [JsonProperty("protocalInterval")]
        public int ProtocalInterval { get; set; }
    }

    public class ByteOrder
    {
        [JsonProperty("u16")]
        public int U16 { get; set; }
        [JsonProperty("u32")]
        public int U32 { get; set; }
        [JsonProperty("float")]
        public int Float { get; set; }
    }

    public class AddressTypeReg
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("ioWidth")]
        public int IoWidth { get; set; }
        [JsonProperty("minMainAddr")]
        public int MinMainAddr { get; set; }
        [JsonProperty("maxMainAddr")]
        public int MaxMainAddr { get; set; }
        [JsonProperty("mainAddrType")]
        public int MainAddrType { get; set; }
        [JsonProperty("subAddrType")]
        public int SubAddrType { get; set; }
        [JsonProperty("isBigEndian")]
        public bool IsBigEndian { get; set; }
        [JsonProperty("subAddrLen")]
        public int SubAddrLen { get; set; }
        [JsonProperty("subIndexType")]
        public int SubIndexType { get; set; }
        [JsonProperty("minSubIndex")]
        public int MinSubIndex { get; set; }
        [JsonProperty("maxSubIndex")]
        public int MaxSubIndex { get; set; }
        [JsonProperty("hasSubIndex")]
        public bool HasSubIndex { get; set; }
    }
}

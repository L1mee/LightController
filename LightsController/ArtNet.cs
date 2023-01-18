using LXProtocols.ArtNet.Packets;
using LXProtocols.ArtNet.Sockets;
using System.Net.Sockets;
using System.Net;
using LXProtocols.Acn.Rdm;

namespace LightsController;

public class ArtNet : ISender
{
    #region Variables

    private readonly ArtNetDmxPacket _artNetDmxPacket = new();
    private ArtNetSocket? _artNet;

    private readonly List<byte> _universesToSend = new();

    #endregion

    #region Constructor

    public ArtNet(IEnumerable<byte>? universes = null)
    {
        if (universes == null) return;

        SetUniverseOut(universes);
    }

    #endregion

    #region ISender

    public void SetUniverseOut(IEnumerable<byte> universes)
    {
        _universesToSend.Clear();
        foreach (var b in universes) _universesToSend.Add(b);
    }

    public bool Start()
    {
        try
        {
            _artNet?.Close();
            _artNet = new ArtNetSocket(UId.Empty);
            var ip = GetLocalIP();
            _artNetDmxPacket.DmxData = new byte[512];
            _artNet.Open(ip, null);
            return true;
        }
        catch
        {
            return false;
        }
    }

    #region Start Helper Functions

    private static IPAddress GetLocalIP()
    {
        var address = IPAddress.None;

        var hostName = Dns.GetHostName();

        try
        {
            var localHost = Dns.GetHostEntry(hostName);
            foreach (var item in localHost.AddressList)
            {
                if (item.AddressFamily != AddressFamily.InterNetwork) continue;
                address = item;
                break;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("Failed to find IP for :\n host name = {0}\n exception={1}", hostName, e);
        }

        return address;
    }

    #endregion

    public void Send(Data data)
    {
        foreach (var b in _universesToSend)
        {
            try
            {
                UpdateArtNet(b, data.GetUniverse(b));
            }
            catch
            {
                Console.WriteLine($"Could not send universe {b}.");
            }
        }
    }

    #region Send Helper Functions

    private void UpdateArtNet(byte universe, byte[] universeData)
    {
        _artNetDmxPacket.Universe = universe;
        Buffer.BlockCopy(universeData, 0, _artNetDmxPacket.DmxData, 0, universeData.Length);
        _artNet!.Send(_artNetDmxPacket);
    }

    #endregion

    public void Quit()
    {
        _artNet?.Close();
        Console.WriteLine("ArtNet socket closed");
    }

    #endregion

    #region Universes to Send

    public void AddUniverseToSend(byte b)
    {
        if (_universesToSend.Contains(b)) return;
        _universesToSend.Add(b);
    }

    public void AddUniversesToSend(IEnumerable<byte> bs)
    {
        foreach (var b in bs)
        {
            AddUniverseToSend(b);
        }
    }

    public void RemoveUniverseToSend(byte b)
    {
        if (!_universesToSend.Contains(b)) return;
        _universesToSend.Remove(b);
    }

    public void SetUniversesToSend(IEnumerable<byte> universesToSend)
    {
        _universesToSend.Clear();
        foreach (var b in universesToSend) _universesToSend.Add(b);
    }

    #endregion

    //Trash
    #region Outdated

    //public void SendMultipleArtNetUniverses(IEnumerable<byte>? universesToSend, Data data)
    //{
    //    if (universesToSend != null)
    //    {
    //        _universesToSend.Clear();
    //        foreach (var b in universesToSend) _universesToSend.Add(b);
    //    }
    //    Send(data);
    //}

    #region Kommentare

    //Using
    //using System.Net.NetworkInformation;
    //using LXProtocols.Acn.Sockets;
    //using LXProtocols.ArtNet;

    //Variables
    //private string _remoteIP = "localhost";
    //private bool _localhost;
    //IPEndPoint remote;
    //private readonly ArtPollReplyPacket _pollReplayPacket = new()
    //{
    //    IpAddress = GetLocalIP().GetAddressBytes(),
    //    ShortName = "UnityArtNet",
    //    LongName = "UnityArtNet-IA",
    //    NodeReport = "#0000 [0000] UnityArtNet Art-Net Product. Good Boot.",
    //    MacAddress = GetLocalMAC()
    //};
    //replace with byte[].Length
    //private const int NumberOfUniverses = 8;
    //private readonly bool[] _receiveArtNet = new bool[NumberOfUniverses + 1];
    //private static bool[] _isServer = new bool[NumberOfUniverses + 1];
    //private Data? _artNetData;

    //Start
    //_artNetData = Data.Instance;
    //var IP = _localhost ? FindFromHostName("127.0.0.1") : GetLocalIP();
    //remote = new IPEndPoint(FindFromHostName(_remoteIP), ArtNetSocket.Port);
    //_artNetData.DmxUpdate += UpdateArtNet;
    //this updates ArtNet from here, I don't want that anymore
    //ArtNetReceiver(CallUpdate);
    //very likely useless since Data needs to be created first anyways (controller)
    //_artNetData.OnEnable();


    //ArtNetReceiver
    //private void ArtNetReceiver(Action callback)
    //{
    //    void OnArtNetOnNewPacket(object? sender, NewPacketEventArgs<ArtNetPacket> e)
    //    {
    //        switch (e.Packet.OpCode)
    //        {
    //            case ArtNetOpCodes.Dmx:
    //                //var packet = e.Packet as ArtNetDmxPacket;
    //                //var universe = packet!.Universe;
    //                //if (_receiveArtNet[universe])
    //                //{
    //                //    _artNetData!.SetData(universe, packet.DmxData);
    //                //}
    //                //callback();
    //                break;
    //            case ArtNetOpCodes.Poll:
    //                _artNet.Send(_pollReplayPacket);
    //                break;
    //            case ArtNetOpCodes.None:
    //                break;
    //            case ArtNetOpCodes.PollReply:
    //                break;
    //            case ArtNetOpCodes.TodRequest:
    //                break;
    //            case ArtNetOpCodes.TodData:
    //                break;
    //            case ArtNetOpCodes.TodControl:
    //                break;
    //            case ArtNetOpCodes.Rdm:
    //                break;
    //            case ArtNetOpCodes.RdmSub:
    //                break;
    //            default:
    //                throw new Exception("ArgumentOutOfRangeException");
    //        }
    //    }

    //    _artNet!.NewPacket += OnArtNetOnNewPacket;
    //}

    //UpdateArtNet
    //public void UpdateArtNet()
    //{
    //    for (var i = 0; i < NumberOfUniverses; i++)
    //    {
    //        if (_isServer[i])
    //        {
    //            Send((byte)i, _artNetData!.DmxDataMap![i]);
    //        }
    //    }
    //}

    //GetLocalMAC
    //private static byte[]? GetLocalMAC()
    //{
    //    var interfaces = NetworkInterface.GetAllNetworkInterfaces();
    //    NetworkInterface? net = interfaces.FirstOrDefault(i => i.OperationalStatus == OperationalStatus.Up);

    //    return net?.GetPhysicalAddress().GetAddressBytes();
    //}

    #endregion

    #endregion
}
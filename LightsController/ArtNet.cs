using LXProtocols.ArtNet.Packets;
using LXProtocols.ArtNet.Sockets;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Net;
using LXProtocols.Acn.Rdm;
using LXProtocols.Acn.Sockets;
using LXProtocols.ArtNet;

namespace LightsController;

public class ArtNet : ISendMode<byte[][]>
{
    public void Send(byte[][] data)
    {
        //make ArtNet Packet and call Send via IO
        throw new NotImplementedException();
    }

    public void Quit()
    {
        //close ArtNet IO
        throw new NotImplementedException();
    }

    #region CopyPaste

    private readonly ArtNetDmxPacket _dmxToSend = new();
    private ArtNetSocket? _artNet;

    //private string _remoteIP = "localhost";
    //private bool _localhost;
    //IPEndPoint remote;

    private readonly ArtPollReplyPacket _pollReplayPacket = new()
    {
        IpAddress = GetLocalIP().GetAddressBytes(),
        ShortName = "UnityArtNet",
        LongName = "UnityArtNet-IA",
        NodeReport = "#0000 [0000] UnityArtNet Art-Net Product. Good Boot.",
        MacAddress = GetLocalMAC()
    };

    //replace with byte[].Length
    private const int NumberOfUniverses = 8;
    private readonly bool[] _receiveArtNet = new bool[NumberOfUniverses + 1];
    private static bool[] _isServer = new bool[NumberOfUniverses + 1];

    private ArtNetData? _artNetData;

    public void Start()
    {
        _artNetData = ArtNetData.Instance;

        _artNetData.DmxUpdate += ArtNetSendUpdate!;

        _artNet?.Close();
        _artNet = new ArtNetSocket(UId.Empty);
        //var IP = _localhost ? FindFromHostName("127.0.0.1") : GetLocalIP();
        var ip = GetLocalIP();
        //remote = new IPEndPoint(FindFromHostName(_remoteIP), ArtNetSocket.Port);
        _dmxToSend.DmxData = new byte[512];
        _artNet.Open(ip, null);
        ArtNetReceiver(CallUpdate);

        _artNetData.OnEnable();
    }

    private void ArtNetReceiver(Action callback)
    {
        void OnArtNetOnNewPacket(object? sender, NewPacketEventArgs<ArtNetPacket> e)
        {
            switch (e.Packet.OpCode)
            {
                case ArtNetOpCodes.Dmx:
                {
                    var packet = e.Packet as ArtNetDmxPacket;
                    var universe = packet!.Universe;
                    if (_receiveArtNet[universe])
                    {
                        _artNetData!.SetData(universe, packet.DmxData);
                        //CallUpdate();
                    }

                    callback();
                    break;
                }
                case ArtNetOpCodes.Poll:
                    _artNet.Send(_pollReplayPacket);
                    break;
                case ArtNetOpCodes.None:
                    break;
                case ArtNetOpCodes.PollReply:
                    break;
                case ArtNetOpCodes.TodRequest:
                    break;
                case ArtNetOpCodes.TodData:
                    break;
                case ArtNetOpCodes.TodControl:
                    break;
                case ArtNetOpCodes.Rdm:
                    break;
                case ArtNetOpCodes.RdmSub:
                    break;
                default:
                    throw new Exception("ArgumentOutOfRangeException");
            }
        }

        _artNet!.NewPacket += OnArtNetOnNewPacket;
    }

    public void UpdateArtNet()
    {
        for (var i = 0; i < NumberOfUniverses; i++)
        {
            if (_isServer[i])
            {
                Send((byte)i, _artNetData!.DmxDataMap![i]);
            }
        }
    }

    private static byte[]? GetLocalMAC()
    {
        var interfaces = NetworkInterface.GetAllNetworkInterfaces();
        NetworkInterface? net = interfaces.FirstOrDefault(i => i.OperationalStatus == OperationalStatus.Up);

        return net?.GetPhysicalAddress().GetAddressBytes();
    }

    private static IPAddress GetLocalIP()
    {
        var address = IPAddress.None;

        var hostName = Dns.GetHostName();

        try
        {
            IPHostEntry localHost = Dns.GetHostEntry(hostName);
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
}
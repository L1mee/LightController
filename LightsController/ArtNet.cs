using LXProtocols.ArtNet.Packets;
using LXProtocols.ArtNet.Sockets;
using System.Net.Sockets;
using System.Net;
using LXProtocols.Acn.Rdm;
// ReSharper disable UnusedMember.Global

namespace LightController;

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
}
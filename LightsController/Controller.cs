using LXProtocols.ArtNet.Packets;
using LXProtocols.ArtNet.Sockets;
using System.Net.Sockets;
using System.Net;
using System.Timers;
using Timer = System.Timers.Timer;
using System.Net.NetworkInformation;
// ReSharper disable UnusedMember.Global

namespace LightsController;

public enum SendThrough
{
    Dmx,
    ArtNet
}

public class Controller
{
    #region Variables

    private Data _data;
    private Timer? _timer;

    public int SleepTime = 200;

    #endregion

    #region Singleton

    static Controller()
    {

    }

    private Controller()
    {

    }

    public static Controller Instance { get; } = new();

    #endregion

    #region Sender

    public void Run(SendThrough sendMode, byte[] whatToSend)
    {
        switch (sendMode)
        {
            case SendThrough.Dmx:
                if (whatToSend.Length != 2) return;
                RunDmx(whatToSend[0], whatToSend[1]);
                break;
            case SendThrough.ArtNet:
                RunArtNet();
                break;
            default:
                Console.WriteLine("Could not set mode.");
                break;
        }
    }

    public void Quit(SendThrough sendMode)
    {
        switch (sendMode)
        {
            case SendThrough.Dmx:
                StopDmx();
                break;
            case SendThrough.ArtNet:
                StopArtNet();
                break;
            default:
                Console.WriteLine("Could not set mode.");
                break;
        }
    }

    #endregion

    #region ArtNet

    private ArtNet? _artNet;

    public void StartArtNet()
    {
        _artNet = new ArtNet();
    }

    #region UniversesToSend

    public void AddUniversesToSend(IEnumerable<byte> universes)
    {
        foreach (var universe in universes)
        {
            AddUniverseToSend(universe);
        }
    }

    public void AddUniverseToSend(byte universe)
    {
        _artNet?.AddUniverseToSend(universe);
    }

    #endregion

    #region Update Start Stop Run

    public void RunArtNet(IEnumerable<byte>? universesToSend = null)
    {
        //Timer just like RunDmx
        _artNet?.SendMultipleArtNetUniverses(universesToSend, _data);
    }

    public void StopArtNet()
    {
        _artNet?.Quit();
    }

    #endregion

    #endregion

    #region DMX Usb Pro Mk2

    //DMX Usb Pro Mk2 has 2 ports.
    private Dmx? _dmxP1;
    private Dmx? _dmxP2;

    public void RunDmx(byte universeA, byte universeB)
    {
        if (!_data.ContainsUniverse(universeA) && !_data.ContainsUniverse(universeB)) return;

        StartDmx(universeA, universeB);

        _timer = new Timer(SleepTime);
        _timer.Elapsed += UpdateDmx;
        _timer.AutoReset = true;
        _timer.Enabled = true;
    }

    public void StopDmx()
    {
        CloseDmxPorts();
        _timer?.Stop();
    }

    private void StartDmx(byte universeA, byte universeB)
    {
        CloseDmxPorts();

        // ReSharper disable once RedundantArgumentDefaultValue
        _dmxP1 = new Dmx(Port.DmxPort1);
        _dmxP2 = new Dmx(Port.DmxPort2);

        _dmxP1.Universe = universeA;
        _dmxP2.Universe = universeB;
    }

    private void UpdateDmx(object? source, ElapsedEventArgs? e)
    {
        _dmxP1!.Send(_data.GetUniverse(_dmxP1.Universe));
        _dmxP2!.Send(_data.GetUniverse(_dmxP2.Universe));
    }

    private void CloseDmxPorts()
    {
        if (_dmxP1 != null)
        {
            _dmxP1.Quit();
            _dmxP1 = null;
        }
        // ReSharper disable once InvertIf
        if (_dmxP2 != null)
        {
            _dmxP2.Quit();
            _dmxP2 = null;
        }
    }

    #endregion

    #region Data

    public void ChangeData(Data data)
    {
        _data = data;
    }

    public void SetChannel(byte universe, (int channel, byte value) input)
    {
        _data.SetChannel(universe, input);
    }

    public void SetChannel(byte universe, int channel, byte value)
    {
        SetChannel(universe, (channel, value));
    }

    public void SetChannel(byte universe, byte[] channels)
    {
        if (channels.Length != Data.Length)
        {
            Console.WriteLine($"Array length differs from target length {Data.Length}.");
            return;
        }

        for (var i = 0; i < Data.Length; i++)
        {
            SetChannel(universe, (i, channels[i]));
        }
    }

    #endregion
}
using System.Timers;
using Timer = System.Timers.Timer;

namespace LightsController;

public enum SendThrough
{
    Dmx,
    ArtNet
}

public enum OnQuit
{
    Freeze,
    TurnOff
}

public class Controller
{
    #region Variables

    private readonly Data _data = new();
    private Timer? _timer;

    public int SleepTimeMs = 200;

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
    
    #region Run Controller using ISender

    private readonly List<ISender> _senderList = new();

    public void Run(SendThrough sendThrough, IEnumerable<byte> universeOutput, int sleepTimeMs = 0)
    {
        if (sleepTimeMs == 0) sleepTimeMs = SleepTimeMs;
        else SleepTimeMs = sleepTimeMs;

        StartUniverses(sendThrough, universeOutput.ToArray());

        if (_senderList.Any(sender => !sender.Start())) throw new Exception("Could not start Lights Controller.");

        _timer = new Timer(sleepTimeMs);
        _timer.Elapsed += Update;
        _timer.AutoReset = true;
        _timer.Enabled = true;
    }

    private void StartUniverses(SendThrough sendMode, byte[] universes)
    {
        _senderList.Clear();
        switch (sendMode)
        {
            case SendThrough.Dmx:
            {
                // ReSharper disable once RedundantArgumentDefaultValue
                var dmxP1 = new Dmx(Port.DmxPort1);
                var dmxP2 = new Dmx(Port.DmxPort2);
                _senderList.Add(dmxP1);
                _senderList.Add(dmxP2);

                if (universes.Length != 2) throw new Exception($"Inconsistent number of universes: {universes}\nShould be 2.");
                for (var i = 0; i < 2; i++)
                {
                    _senderList[i].SetUniverseOut(new[] { universes[i] });
                }

                break;
            }
            case SendThrough.ArtNet:
            {
                var artNet = new ArtNet();
                _senderList.Add(artNet);

                _senderList[0].SetUniverseOut(universes);

                break;
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(sendMode), sendMode, null);
        }
    }

    private void Update(object? source, ElapsedEventArgs? e)
    {
        foreach (var sender in _senderList)
        {
            sender.Send(_data);
        }
    }

    public void Quit(OnQuit onQuit = OnQuit.Freeze)
    {
        if (_timer == null) throw new Exception("Could not quit without starting first.");

        switch (onQuit)
        {
            case OnQuit.Freeze:
                _timer.Stop();
                break;
            case OnQuit.TurnOff:
                _data.BlackOut();
                _timer.Stop();
                Update(this, null);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(onQuit), onQuit, null);
        }

        foreach (var sender in _senderList)
        {
            sender.Quit();
        }

        _senderList.Clear();
    }

    #endregion

    #region Data

    public void SetChannel(byte universe, (int channel, byte value) input)
    {
        if (!_data.ContainsUniverse(universe)) _data.AddUniverse(universe);

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

    #region Outdated

    #region Old Functions

    //private void Start(SendThrough sendMode)
    //{
    //    _senderList.Clear();
    //    switch (sendMode)
    //    {
    //        case SendThrough.Dmx:
    //            // ReSharper disable once RedundantArgumentDefaultValue
    //            var dmxP1 = new Dmx(Port.DmxPort1);
    //            var dmxP2 = new Dmx(Port.DmxPort2);
    //            _senderList.Add(dmxP1);
    //            _senderList.Add(dmxP2);
    //            break;
    //        case SendThrough.ArtNet:
    //            var artNet = new ArtNet();
    //            _senderList.Add(artNet);
    //            break;
    //        default:
    //            Console.WriteLine($"{sendMode} is not a valid sender.");
    //            break;
    //    }

    //    _sendingThrough = sendMode;
    //}

    //private void SetUniverses(byte[] universes)
    //{
    //    switch (_sendingThrough)
    //    {
    //        case SendThrough.Dmx when universes.Length == 2:
    //        {
    //            for (var i = 0; i < 2; i++)
    //            {
    //                _senderList[i].SetUniverseOut(new[] { universes[i] });
    //            }
    //            break;
    //        }
    //        case SendThrough.ArtNet:
    //            _senderList[0].SetUniverseOut(universes);
    //            break;
    //        default:
    //            throw new Exception($"{_sendingThrough} is not a valid sender.");
    //    }
    //}

    #endregion

    #region Old ISender

    //public void OldRun(SendThrough sendMode, byte[] whatToSend)
    //{
    //    switch (sendMode)
    //    {
    //        case SendThrough.Dmx:
    //            if (whatToSend.Length != 2) return;
    //            RunDmx(whatToSend[0], whatToSend[1]);
    //            break;
    //        case SendThrough.ArtNet:
    //            RunArtNet();
    //            break;
    //        default:
    //            Console.WriteLine("Could not set mode.");
    //            break;
    //    }
    //}

    //public void OldQuit(SendThrough sendMode)
    //{
    //    switch (sendMode)
    //    {
    //        case SendThrough.Dmx:
    //            StopDmx();
    //            break;
    //        case SendThrough.ArtNet:
    //            StopArtNet();
    //            break;
    //        default:
    //            Console.WriteLine("Could not set mode.");
    //            break;
    //    }
    //}

    #endregion

    #region DMX Usb Pro Mk2

    //DMX Usb Pro Mk2 has 2 ports.
    //private Dmx? _dmxP1;
    //private Dmx? _dmxP2;

    //public void RunDmx(byte universeA, byte universeB)
    //{
    //    if (!_data.ContainsUniverse(universeA) && !_data.ContainsUniverse(universeB)) return;

    //    StartDmx(universeA, universeB);

    //    _timer = new Timer(SleepTimeMs);
    //    _timer.Elapsed += UpdateDmx;
    //    _timer.AutoReset = true;
    //    _timer.Enabled = true;
    //}

    //public void StopDmx()
    //{
    //    CloseDmxPorts();
    //    _timer?.Stop();
    //}

    //private void StartDmx(byte universeA, byte universeB)
    //{
    //    CloseDmxPorts();

    //    // ReSharper disable once RedundantArgumentDefaultValue
    //    _dmxP1 = new Dmx(Port.DmxPort1);
    //    _dmxP2 = new Dmx(Port.DmxPort2);

    //    _dmxP1.Universe = universeA;
    //    _dmxP2.Universe = universeB;
    //}

    //private void UpdateDmx(object? source, ElapsedEventArgs? e)
    //{
    //    _dmxP1!.Send(_data);
    //    _dmxP2!.Send(_data);
    //}

    //private void CloseDmxPorts()
    //{
    //    if (_dmxP1 != null)
    //    {
    //        _dmxP1.Quit();
    //        _dmxP1 = null;
    //    }
    //    // ReSharper disable once InvertIf
    //    if (_dmxP2 != null)
    //    {
    //        _dmxP2.Quit();
    //        _dmxP2 = null;
    //    }
    //}

    #endregion

    #region ArtNet

    //private ArtNet? _artNet;

    //public void StartArtNet()
    //{
    //    _artNet = new ArtNet();
    //}

    #region UniversesToSend

    //public void AddUniversesToSend(IEnumerable<byte> universes)
    //{
    //    foreach (var universe in universes)
    //    {
    //        AddUniverseToSend(universe);
    //    }
    //}

    //public void AddUniverseToSend(byte universe)
    //{
    //    _artNet?.AddUniverseToSend(universe);
    //}

    #endregion

    #region Update Start Stop Run

    //public void RunArtNet(IEnumerable<byte>? universesToSend = null)
    //{
    //    //Timer just like RunDmx
    //    _artNet?.SendMultipleArtNetUniverses(universesToSend, _data);
    //}

    //public void StopArtNet()
    //{
    //    _artNet?.Quit();
    //}

    #endregion

    #endregion

    #endregion
}
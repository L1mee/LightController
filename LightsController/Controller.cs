using System.Timers;
using Timer = System.Timers.Timer;
// ReSharper disable UnusedMember.Global

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
                Console.WriteLine("Before using DMX:\nRun DMX Pro Manager -> Exit -> Run Program");
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
}
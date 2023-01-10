using System.Timers;
using Timer = System.Timers.Timer;
// ReSharper disable UnusedMember.Global

namespace LightsController;

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

    #region DMX Usb Pro Mk2

    //DMX Usb Pro Mk2 has 2 ports.
    private Dmx? _dmxP1;
    private Dmx? _dmxP2;

    private byte _universeA;
    private byte _universeB;

    public void RunDmx(byte universeA, byte universeB)
    {
        if (!_data.ContainsUniverse(universeA) && !_data.ContainsUniverse(universeB)) return;

        _universeA = universeA;
        _universeB = universeB;

        StartDmx();

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

    private void StartDmx()
    {
        CloseDmxPorts();

        // ReSharper disable once RedundantArgumentDefaultValue
        _dmxP1 = new Dmx(Port.DmxPort1);
        _dmxP2 = new Dmx(Port.DmxPort2);
    }

    private void UpdateDmx(object? source, ElapsedEventArgs? e)
    {
        _dmxP1!.SendDmx(_data.GetUniverse(_universeA));
        _dmxP2!.SendDmx(_data.GetUniverse(_universeB));
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
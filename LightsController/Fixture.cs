using System.Drawing;

// ReSharper disable UnusedMember.Global

namespace LightsController;

public class Fixture
{
    #region Variables

    public byte Universe;
    private int _address;
    public int Address
    {
        get => _address;
        set
        {
            if (value is >= 0 and < 512)
            {
                _address = value;
            }
            else
            {
                Console.WriteLine($"{value} is not a valid address.");
            }
        }
    }
    private int _channelCount;
    public int ChannelCount
    {
        get => _channelCount;
        set
        {
            if (value is > 0 and <= 512)
            {
                _channelCount = value;
            }
            else
            {
                Console.WriteLine($"{value} is not a valid channel count.");
            }
        }
    }

    private readonly Controller _console = Controller.Instance;

    public Dictionary<string, int>? GetChannelAttributes { get; private set; }

    #region Rotation/Orientation Calculation

    //Math: Map real life degrees to byte settings

    //deg = possible rotation in degrees for 0b-255b (pan axis)
    //PanDivide = deg / 255
    //default: 540° / 255 ^= 2.11764705882
    public double PanDivide = 2.11764705882;

    //deg = possible rotation in degrees for 0b-255b (tilt axis)
    //TiltDivide = deg / 255
    //default: 180° / 255 ^= 0.70588235294
    public double TiltDivide = 0.70588235294;

    #endregion

    #endregion

    #region Constructor

    public Fixture(byte universe, int address, int channelCount)
    {
        Universe = universe;
        _address = address;
        _channelCount = channelCount;
    }

    public Fixture(byte universe, int address, IReadOnlyList<string> channelAttributes) : this(universe, address, channelAttributes.Count)
    {
        GetChannelAttributes = new Dictionary<string, int>();
        InitAttributesFromArray(channelAttributes);
    }

    #endregion

    #region SetChannel

    public void SetChannel(int channel, byte value)
    {
        try
        {
            _console.SetChannel(Universe, (_address - 1 + channel, value));
        }
        catch
        {
            Console.WriteLine("Couldn't set Channel.");
        }
    }

    public void SetChannels(byte[] array)
    {
        if (array.Length == _channelCount)
        {
            for (var i = 0; i < _channelCount; i++)
            {
                try
                {
                    _console.SetChannel(Universe, (_address - 1 + i, array[i]));
                }
                catch
                {
                    Console.WriteLine($"Couldn't set channel {i} to {array[i]}.");
                }
            }
        }
        else
        {
            Console.WriteLine("Array does not match channel count.");
        }
    }

    #endregion

    #region Attributes

    public void SetAttribute(string attribute, byte value)
    {
        try
        {
            _console.SetChannel(Universe, (_address - 1 + GetChannelAttributes![attribute.ToUpper()], value));
        }
        catch
        {
            Console.WriteLine($"Couldn't set {attribute}.");
        }
    }

    private void InitAttributesFromArray(IReadOnlyList<string> channels)
    {
        GetChannelAttributes = new Dictionary<string, int>();
        for (var i = 0; i < channels.Count; i++)
        {
            GetChannelAttributes.Add(channels[i].ToUpper(), i);
            Console.WriteLine($"Linked {channels[i].ToUpper()} to {i}");
        }
    }

    public void AddAttribute(byte channel, string attribute)
    {
        if (channel >= GetChannelAttributes!.Count)
        {
            throw new Exception("Can not add attribute to channel that does not exist.");
        }

        GetChannelAttributes.Add(attribute.ToUpper(), channel);
    }

    #endregion

    #region Specific Attributes

    public void SetColor(Color color)
    {
        try
        {
            _console.SetChannel(Universe, (_address - 1 + GetChannelAttributes!["RED"], color.R));
            _console.SetChannel(Universe, (_address - 1 + GetChannelAttributes["GREEN"], color.G));
            _console.SetChannel(Universe, (_address - 1 + GetChannelAttributes["BLUE"], color.B));
        }
        catch
        {
            Console.WriteLine("Couldn't set Color.");
        }
    }

    public void SetOrientation((float x, float y, float z) vector)
    {
        //This depends on the fixtures real orientation and rotation settings

        var pan = 0d;
        switch (vector.x)
        {
            case >= 0 when vector.y < 0:
                //0-90 Degrees
                pan = Math.Atan(Math.Abs(vector.y) / Math.Abs(vector.x)) * (180 / Math.PI);
                break;
            case < 0 when vector.y < 0:
                //90-180 Degrees
                pan = 90 + Math.Atan(Math.Abs(vector.x) / Math.Abs(vector.y)) * (180 / Math.PI);
                break;
            case < 0 when vector.y >= 0:
                //180-270 Degrees
                pan = 180 + Math.Atan(Math.Abs(vector.y) / Math.Abs(vector.x)) * (180 / Math.PI);
                break;
            case >= 0 when vector.y >= 0:
                //270-360 Degrees
                pan = 270 + Math.Atan(Math.Abs(vector.x) / Math.Abs(vector.y)) * (180 / Math.PI);
                break;
            default:
                Console.WriteLine("Couldn't set Orientation.");
                break;
        }

        var xy = Math.Sqrt(vector.x * vector.x + vector.y * vector.y);
        var tilt = Math.Atan(vector.z / xy) * (180 / Math.PI);

        SetOrientation((int)pan, (int)tilt);
    }

    public void SetOrientation((int pan, int tilt) rotation)
    {
        //This depends on the fixtures real orientation

        SetOrientation(rotation.pan, rotation.tilt);
    }

    public void SetOrientation(int pan, int tilt)
    {
        //This depends on the fixtures real orientation and rotation speed

        try
        {
            _console.SetChannel(Universe, (_address - 1 + GetChannelAttributes!["PANRUN"], (byte)(pan / PanDivide)));
            _console.SetChannel(Universe, (_address - 1 + GetChannelAttributes["TILTRUN"], (byte)(tilt / TiltDivide)));
        }
        catch
        {
            Console.WriteLine("Couldn't set Orientation.");
        }
    }

    public void SetDimmer(byte dimmer)
    {
        try
        {
            _console.SetChannel(Universe, (_address - 1 + GetChannelAttributes!["DIMMER"], dimmer));
        }
        catch
        {
            Console.WriteLine("Couldn't set Dimmer.");
        }
    }

    public void SetStrobe(byte strobe)
    {
        try
        {
            _console.SetChannel(Universe, (_address - 1 + GetChannelAttributes!["DIMMER"], strobe));
        }
        catch
        {
            Console.WriteLine("Couldn't set Strobe.");
        }
    }

    public void TurnOff()
    {
        try
        {
            for (var i = 0; i < _channelCount; i++)
            {
                _console.SetChannel(Universe, (_address - 1 + i, 0));
            }
        }
        catch
        {
            Console.WriteLine("This should never happen, I am sorry.");
        }
    }

    #endregion
}
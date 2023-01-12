using System.Drawing;

// ReSharper disable UnusedMember.Global

namespace LightsController;

public class Fixture
{
    public byte Universe;
    public int Address;
    public int ChannelCount;

    private readonly Controller _console = Controller.Instance;

    public Dictionary<string, int>? GetChannelFunctions { get; private set; }

    public Fixture(byte universe, int address, int channelCount)
    {
        DefaultFixture(universe, address, channelCount);
    }

    public Fixture(byte universe, int address, IReadOnlyList<string> channelFunctions)
    {
        DefaultFixture(universe, address, channelFunctions.Count);
        InitFunctionsFromArray(channelFunctions);
    }

    public void DefaultFixture(byte universe, int address, int channelCount)
    {
        Universe = universe;
        Address = address;
        ChannelCount = channelCount;

        GetChannelFunctions = new Dictionary<string, int>();
    }

    public void SetColor(Color color)
    {
        try
        {
            _console.SetChannel(Universe, (Address - 1 + GetChannelFunctions!["RED"], color.R));
            _console.SetChannel(Universe, (Address - 1 + GetChannelFunctions["GREEN"], color.G));
            _console.SetChannel(Universe, (Address - 1 + GetChannelFunctions["BLUE"], color.B));
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
            _console.SetChannel(Universe, (Address - 1 + GetChannelFunctions!["PANRUN"], (byte)(pan / 2.11764705882)));
            _console.SetChannel(Universe, (Address - 1 + GetChannelFunctions["TILTRUN"], (byte)(tilt / 0.70588235294)));
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
            _console.SetChannel(Universe, (Address - 1 + GetChannelFunctions!["DIMMER"], dimmer));
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
            _console.SetChannel(Universe, (Address - 1 + GetChannelFunctions!["DIMMER"], strobe));
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
            for (var i = 0; i < ChannelCount; i++)
            {
                _console.SetChannel(Universe, (Address - 1 + i, 0));
            }
        }
        catch
        {
            Console.WriteLine("This should never happen, I am sorry.");
        }
    }

    public void SetChannel(int channel, byte value)
    {
        try
        {
            _console.SetChannel(Universe, (Address - 1 + channel, value));
        }
        catch
        {
            Console.WriteLine("Couldn't set Channel.");
        }
    }

    public void SetAttribute(string attribute, byte value)
    {
        try
        {
            _console.SetChannel(Universe, (Address - 1 + GetChannelFunctions![attribute.ToUpper()], value));
        }
        catch
        {
            Console.WriteLine($"Couldn't set {attribute}.");
        }
    }

    private void InitFunctionsFromArray(IReadOnlyList<string> channels)
    {
        GetChannelFunctions = new Dictionary<string, int>();
        for (var i = 0; i < channels.Count; i++)
        {
            GetChannelFunctions.Add(channels[i].ToUpper(), i);
            Console.WriteLine($"Linked {channels[i].ToUpper()} to {i}");
        }
    }

    public void AddFunction(byte channel, string function)
    {
        if (channel >= GetChannelFunctions!.Count)
        {
            throw new Exception("Can not add function to channel that does not exist.");
        }

        GetChannelFunctions.Add(function.ToUpper(), channel);
    }

    public void SetChannels(byte[] array)
    {
        if (array.Length == ChannelCount)
        {
            for (int i = 0; i < ChannelCount; i++)
            {
                try
                {
                    _console.SetChannel(Universe, (Address - 1 + i, array[i]));
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
}
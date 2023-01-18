// ReSharper disable UnusedMember.Global

namespace LightsController;

public readonly struct Data
{
    #region Variables & Constructor

    private readonly Dictionary<byte, byte[]> _universes;
    public const int Length = 512;

    public Data()
    {
        _universes = new Dictionary<byte, byte[]>();
    }

    //for now this is useless, just more constructors with more options
    //public Data(Dictionary<byte, byte[]> universes)
    //{
    //    _universes = universes;
    //}

    //public Data(byte universe, byte[] channels)
    //{
    //    _universes = new Dictionary<byte, byte[]>();
    //    AddUniverse(universe, channels);
    //}

    #endregion

    #region DictionaryAccess

    public void AddUniverse(byte universe, byte[]? channels = null, bool replace = false)
    {
        channels ??= new byte[512];

        if (!_universes.ContainsKey(universe))
        {
            _universes.Add(universe, channels);
        }

        if (replace) _universes[universe] = channels;
    }

    public void RemoveUniverse(byte universe)
    {
        try
        {
            _universes.Remove(universe);
        }
        catch
        {
            Console.WriteLine($"Could not remove Universe {universe}.");
        }
    }

    public void SetChannel(byte universe, (int channel, byte value) input)
    {
        try
        {
            _universes[universe][input.channel] = input.value;
        }
        catch
        {
            Console.WriteLine($"Could not set channel {input.channel} in universe {universe} to {input.value}.");
        }
    }

    public byte[] GetUniverse(byte universe)
    {
        return _universes[universe];
    }

    public bool ContainsUniverse(byte universe)
    {
        return _universes.ContainsKey(universe);
    }

    public void ResetUniverse(byte universe)
    {
        for (var i = 0; i < Length; i++)
        {
            _universes[universe][i] = 0;
        }
    }

    public void BlackOut()
    {
        foreach (var key in _universes.Keys)
        {
            ResetUniverse(key);
        }
    }

    #endregion
}
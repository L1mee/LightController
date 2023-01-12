﻿using System.IO.Ports;

namespace LightsController;

public enum Port
{
    DmxPort1,
    DmxPort2
}

public class Dmx : IEquatable<Dmx>
{
    #region Variables

    //_port is mainly useful for IEquatable
    private readonly Port _port;

    //SendDmx
    private const int DmxIndexOffset = 5;
    private readonly int _dmxMessageOverhead;
    //const indicate what Port should be send through
    private const int DmxMessageOverheadP1 = 6;
    private const int DmxMessageOverheadP2 = 19;

    //how many channel a Dmx universe has (its 512)
    private const int NDmxChannels = Data.Length;

    //TxBuffer and DmxMessages
    private const byte DmxProStartMsg = 0x7E;
    private readonly byte _dmxProLabelDmx;
    private const byte DmxProLabelDmxP1 = 6;
    private const byte DmxProLabelDmxP2 = 19;
    private const byte DmxProStartCode = 0;
    private const byte DmxProEndMsg = 0xE7;

    //TxBuffer and DmxMessages
    private readonly int _txBufferLength;

    //Ports
    private static SerialPort? _serialPort;
    public string[] SerialPorts;
    //only useful for an exception, might be neglectable
    public int SerialPortIdx;

    //TxBuffer
    private readonly byte[] _txBuffer;

    #endregion

    #region Constructor

    public Dmx(Port port = Port.DmxPort1)
    {
        _port = port;
        switch (port)
        {
            case Port.DmxPort1:
                _dmxMessageOverhead = DmxMessageOverheadP1;
                _dmxProLabelDmx = DmxProLabelDmxP1;
                break;
            case Port.DmxPort2:
                _dmxMessageOverhead = DmxMessageOverheadP2;
                _dmxProLabelDmx = DmxProLabelDmxP2;
                break;
            default:
                Console.WriteLine("Could not set port.");
                break;
        }

        _txBufferLength = _dmxMessageOverhead + NDmxChannels;
        _txBuffer = new byte[_dmxMessageOverhead + NDmxChannels];

        Console.WriteLine("System started");
        SerialPorts = GetPortNames();
        OpenSerialPort();
        InitTxBuffer();

        var dmxThread = new Thread(ThreadStart);
        dmxThread.Start();
    }

    #endregion

    #region IEquateable

    public bool Equals(Dmx? other)
    {
        if (other is null) return false;
        return _port == other._port;
    }

    public override bool Equals(object? obj)
    {
        var other = obj as Dmx;
        return Equals(other);
    }

    public override int GetHashCode()
    {
        return _port.GetHashCode();
    }

    #endregion

    #region Dmx Messages

    private static void ThreadStart()
    {
        Console.WriteLine("Thread Start");

        //extend API and enable both ports
        if (_serialPort is not { IsOpen: true }) return;

        byte[] array = { DmxProStartMsg, 13, 0xCF, 0xAA, 05, 09, DmxProEndMsg };
        _serialPort.Write(array, 0, array.Length);
        Console.WriteLine("Trying to set API");
    }

    public void SendDmx(byte[] dmxData)
    {
        if (_serialPort is not { IsOpen: true }) return;

        Buffer.BlockCopy(dmxData, 0, _txBuffer, DmxIndexOffset, NDmxChannels);
        _serialPort.Write(_txBuffer, 0, _txBufferLength);
        Console.WriteLine($"Sending through ThreadedIO Port {_port}.");
    }

    private void InitTxBuffer()
    {
        for (var i = 0; i < _txBufferLength; i++) _txBuffer[i] = 255;

        _txBuffer[000] = DmxProStartMsg;
        _txBuffer[001] = _dmxProLabelDmx;
        _txBuffer[002] = NDmxChannels + 1 & 255;
        _txBuffer[003] = (NDmxChannels + 1 >> 8) & 255;
        _txBuffer[004] = DmxProStartCode;
        _txBuffer[517] = DmxProEndMsg;
    }

    #endregion

    #region Ports (Open/Close)

    private static string[] GetPortNames()
    {
        return SerialPort.GetPortNames();
    }

    public void OpenSerialPort()
    {
        if (_serialPort != null)
        {
            _serialPort.Close();
            _serialPort.Dispose();
        }

        _serialPort = new SerialPort(SerialPorts[^1], 57600, Parity.None, 8, StopBits.One);

        try
        {
            _serialPort.Open();
            _serialPort.ReadTimeout = 50;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            SerialPortIdx = 0;
        }
    }

    public static void Quit()
    {
        if (_serialPort != null)
        {
            _serialPort.Close();
            _serialPort.Dispose();
        }
        Console.WriteLine("DMX port closed");
    }

    #endregion
}
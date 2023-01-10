using System.IO.Ports;

namespace LightsController
{
    public enum Port
    {
        DmxPort1,
        DmxPort2
    }

    public class Dmx : IEquatable<Dmx>
    {
        public bool DevelopmentMode = true;
        private readonly Port _port;

        private const int DmxIndexOffset = 5;
        private readonly int _dmxMessageOverhead;
        private const int DmxMessageOverheadP1 = 6;
        private const int DmxMessageOverheadP2 = 19;

        private const int NDmxChannels = Data.Length;

        private const byte DmxProStartMsg = 0x7E;
        private readonly byte _dmxProLabelDmx;
        private const byte DmxProLabelDmxP1 = 6;
        private const byte DmxProLabelDmxP2 = 19;
        private const byte DmxProStartCode = 0;
        private const byte DmxProEndMsg = 0xE7;

        private readonly int _txBufferLength;

        private static SerialPort? _serialPort;
        public string[] SerialPorts;
        public int SerialPortIdx;

        private readonly byte[] _dmxLevels = new byte[NDmxChannels];
        private readonly byte[] _txBuffer;

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

            var dmxThread = new Thread(ThreadedIo);
            dmxThread.Start();
        }

        public int this[int index]
        {
            get
            {
                if (index is < 1 or > NDmxChannels)
                {
                    throw new Exception("Channel out of range: " + index);
                }

                return _dmxLevels[index - 1];
            }
            set
            {
                if (index is < 1 or > NDmxChannels)
                {
                    throw new Exception("Channel out of range: " + index);
                }

                if (value is < 0 or > 255)
                {
                    throw new Exception("Level out fo range");
                }

                _dmxLevels[index - 1] = (byte)Math.Clamp(value, 0, 255);
            }
        }

        #region IEquateable

        public bool Equals(Dmx? other)
        {
            return _port == other!._port;
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

        private void ThreadedIo()
        {
            Console.WriteLine("Thread Start");

            #region extend API and enable both ports

            if (!DevelopmentMode) return;
            if (_serialPort is not { IsOpen: true }) return;

            byte[] array = { DmxProStartMsg, 13, 0xCF, 0xAA, 05, 09, DmxProEndMsg };
            _serialPort.Write(array, 0, array.Length);
            Console.WriteLine("Trying to set API");

            #endregion
        }

        public void SendDmx(byte[] dmxData)
        {
            if (_serialPort is not { IsOpen: true }) return;

            Buffer.BlockCopy(dmxData, 0, _txBuffer, DmxIndexOffset, NDmxChannels);
            _serialPort.Write(_txBuffer, 0, _txBufferLength);
            Console.WriteLine($"Sending through ThreadedIO Port {_port}.");
        }

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

        public void Quit()
        {
            for (var i = 0; i < NDmxChannels; i++) _dmxLevels[i] = 0x00;

            if (_serialPort != null)
            {
                _serialPort.Close();
                _serialPort.Dispose();
            }
            Console.WriteLine("DMX port closed");
        }
    }
}
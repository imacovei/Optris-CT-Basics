// ----------------------------------------------------------------------------
// <copyright file="OptrisCtManager.cs" company="Private">
// Copyright (c) 2021 All Rights Reserved
// </copyright>
// <author>Iulian Macovei</author>
// <date>10/25/2021 11:02:28 AM</date>
// ----------------------------------------------------------------------------

#region License
// ----------------------------------------------------------------------------
// Copyright 2021 Iulian Macovei
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
#endregion

namespace OptrisCT
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO.Ports;
    using System.Linq;
    using System.Threading;

    /// <summary>
    /// Optris CT Manager
    /// </summary>
    public class OptrisCtManager : IDisposable
    {
        private SerialPort serialPort;
        private readonly List<byte> readBytes;
        private byte multiAddress;
        private const int WaitDelay = 30; // milliseconds to wait for data on the serial buffer
        private readonly List<byte> multiAddressList;
        private const int NO_OF_OPTRIS_DEVICES = 4;

        /// <summary>
        /// List of implemented Optris-CT commands
        /// </summary>
        public enum OptrisCommands : byte
        {
            Unknown = 0x00,
            ReadTemperature = 0x01,
            ReadTemperatureLineMode = 0x2E,
            ReadSerialNumber = 0x0E,
            ReadFwVersion = 0x0F,
            ReadEmissivity = 0x04,
            SetEmissivity = 0x84
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OptrisCtManager"/> class.
        /// </summary>
        /// <param name="comPort">Serial COM port to connect to</param>
        /// <param name="addressList">List of addresses of the Optris-CT devices</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="comPort"/> is <see langword="null"/></exception>
        public OptrisCtManager(string comPort, List<byte> addressList)
        {
            if (string.IsNullOrEmpty(comPort))
            {
                throw new ArgumentNullException(Resource.ERR_EMPTY_COM_PORT);
            }

            if (addressList.Count < 1)
            {
                throw new ArgumentException(Resource.ERR_EMPTY_ADDRESS_LIST);
            }

            foreach (byte mAddress in addressList.Where(mAddress => mAddress is < 1 or > NO_OF_OPTRIS_DEVICES))
            {
                throw new ArgumentException(string.Format(Resource.ERR_INVALID_ADDRESS_IN_LIST, NO_OF_OPTRIS_DEVICES, mAddress));
            }

            this.InitializeAndVerifyPort(comPort);
            this.multiAddressList = new List<byte>();

            foreach (byte mAddress in addressList)
            {
                this.multiAddressList.Add(mAddress);
            }

            this.readBytes = new List<byte>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OptrisCtManager"/> class.
        /// </summary>
        /// <param name="comPort">Serial COM port to connect to</param>
        /// <param name="multiAddress">Address of the Optris-CT device to connect to</param>
        public OptrisCtManager(string comPort, byte multiAddress)
        {
            if (string.IsNullOrEmpty(comPort))
            {
                throw new ArgumentNullException(Resource.ERR_EMPTY_COM_PORT);
            }

            if (multiAddress is < 1 or > NO_OF_OPTRIS_DEVICES)
            {
                throw new ArgumentException(string.Format(Resource.ERR_INVALID_ADDRESS_IN_LIST, NO_OF_OPTRIS_DEVICES, multiAddress));
            }

            this.InitializeAndVerifyPort(comPort);

            this.multiAddress = multiAddress;
            this.readBytes = new List<byte>();
        }

        /// <summary>
        /// Initializes the specified COM port and performs an open/close verification
        /// </summary>
        /// <param name="comPort">Name of the serial port</param>
        private void InitializeAndVerifyPort(string comPort)
        {
            this.serialPort = new SerialPort
            {
                PortName = comPort,
                BaudRate = 115200,
                Parity = Parity.None,
                DataBits = 8,
                StopBits = StopBits.One,
                Handshake = Handshake.None,
                ReadTimeout = 500,
                WriteTimeout = 500,
                DtrEnable = true,
                RtsEnable = true
            };

            try
            {
                this.serialPort.Open();
            }
            catch (Exception ex)
            {
                throw new ArgumentException(string.Format(Resource.ERR_CANNOT_OPEN_COM_PORT, ex.Message));
            }

            this.serialPort.Close();
        }

        /// <summary>
        /// Reads the serial number from device
        /// </summary>
        /// <returns>Read serial number</returns>
        public int ReadSerialNumber()
        {
            bool readResult = this.ExecuteReadCommand(OptrisCommands.ReadSerialNumber, 1000, 3, null);
            if (!readResult)
            {
                return 0;
            }

            this.readBytes.Reverse();
            this.readBytes.Add(0);
            return BitConverter.ToInt32(this.readBytes.ToArray(), 0);
        }

        /// <summary>
        /// Reads the firmware version from device
        /// </summary>
        /// <returns>Read firmware version</returns>
        public int ReadFwVersion()
        {
            bool readResult = this.ExecuteReadCommand(OptrisCommands.ReadFwVersion, 1000, 2, null);
            if (!readResult)
            {
                return 0;
            }

            this.readBytes.Reverse();
            return BitConverter.ToInt16(this.readBytes.ToArray(), 0);
        }

        /// <summary>
        /// Reads the emissivity value from device
        /// </summary>
        /// <returns>Read emissivity value</returns>
        public float ReadEmissivity()
        {
            bool readResult = this.ExecuteReadCommand(OptrisCommands.ReadEmissivity, 1000, 2, null);
            float readValue = ConvertToFloat(this.readBytes);

            // according to the documentation, the emissivity must be >= 0.0 and <= 1.1
            return (!readResult || readValue < 0.0 || readValue > 1.1) ? float.MinValue : readValue;
        }

        /// <summary>
        /// Writes the provided emissivity value in the device
        /// </summary>
        /// <param name="value">New emissivity value</param>
        /// <returns>true on success, otherwise false</returns>
        public bool SetEmissivity(float value)
        {
            // according to the documentation, the emissivity must be >= 0.0 and <= 1.1
            if (value < 0.0 || value > 1.1)
            {
                return false;
            }

            byte[] byteValue = BitConverter.GetBytes((short)(value * 1000));
            bool setResult = this.ExecuteWriteCommand(OptrisCommands.SetEmissivity, byteValue, 1000, 2);
            if (!setResult)
            {
                return false;
            }

            // get the response and validate the read value
            this.readBytes.Reverse();
            return this.readBytes.SequenceEqual(byteValue);
        }

        /// <summary>
        /// Monitors the temperature for the specified duration (in milliseconds)
        /// Use it on the objects that initialize one single address
        /// </summary>
        /// <param name="durationMs">For how long (in milliseconds) the temperature will be monitored</param>
        /// <returns>List of pairs time-temperature</returns>
        public Dictionary<long, decimal> MonitorTemperature(int durationMs)
        {
            Dictionary<long, decimal> measurements = new Dictionary<long, decimal>();

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            while (stopwatch.ElapsedMilliseconds < durationMs)
            {
                if (ExecuteReadCommand(OptrisCommands.ReadTemperature, 1000, 2, null))
                {
                    measurements.Add(stopwatch.ElapsedMilliseconds, ConvertToCelsius(this.readBytes));
                }
            }

            stopwatch.Stop();
            return measurements;
        }

        /// <summary>
        /// Monitors the temperature for the specified duration (in milliseconds)
        /// Use it on the objects that initialize a list of addresses
        /// </summary>
        /// <param name="durationMs">For how long (in milliseconds) the temperature will be monitored</param>
        /// <returns>For each address, a list of pairs time-temperature</returns>
        public Dictionary<byte, Dictionary<long, decimal>> MonitorTemperatureLineMode(int durationMs)
        {
            Dictionary<byte, Dictionary<long, decimal>> measurements = new Dictionary<byte, Dictionary<long, decimal>>();

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            // in case of line read we read the temperature from all Optris-CT devices in one go
            // and the multiAddress is a kind of broadcast address set to 0
            this.multiAddress = 0;

            while (stopwatch.ElapsedMilliseconds < durationMs)
            {
                // we read two bytes for each address
                short bytesToRead = (short)(2 * this.multiAddressList.Count);

                // the index of the last address to read from, so subtract the 0xB0 to get the index
                byte lastAddressIndex = (this.multiAddressList.Max());
                
                // if we read the temperature in line mode we need to provide the checksum
                // the Optris documentation specifies that the checksum is needed only for write commands
                // but this is not quite correct.
                // all multi-line read operations need the checksum too
                byte checksum = CalculateChecksum(new List<byte>() { (byte)OptrisCommands.ReadTemperatureLineMode, lastAddressIndex });

                List<byte> suffixBytes = new List<byte>() { lastAddressIndex, checksum };

                if (ExecuteReadCommand(OptrisCommands.ReadTemperatureLineMode, 1000, bytesToRead, suffixBytes.ToArray()))
                {
                    decimal correctionValue = 0;
                    long timestamp = stopwatch.ElapsedMilliseconds;
                    Dictionary<byte, decimal> allTemperatures = SplitReadTemperatures();
                    foreach ((byte address, decimal value) in allTemperatures)
                    {
                        if (!measurements.ContainsKey(address))
                        {
                            measurements.Add(address, new Dictionary<long, decimal>());
                        }

                        measurements[address].Add(timestamp, value + correctionValue);
                    }
                }
            }

            stopwatch.Stop();
            return measurements;
        }

        /// <summary>
        /// Executes a read command
        /// </summary>
        /// <param name="cmd">Command to be executed</param>
        /// <param name="waitTimeMs">Maximum time to wait until the response arrive</param>
        /// <param name="byteCount">Expected size of the response in bytes</param>
        /// <param name="suffixBytes">Additional bytes to be added to the command (used in line mode read)</param>
        /// <returns>true on success, otherwise false</returns>
        private bool ExecuteReadCommand(OptrisCommands cmd, int waitTimeMs, short byteCount, byte[] suffixBytes)
        {
            byte[] toSend = new byte[] { (byte)(0xB0 + this.multiAddress), (byte) cmd};

            if (suffixBytes?.Length > 0)
            {
                int originalLength = toSend.Length;
                Array.Resize(ref toSend, toSend.Length + suffixBytes.Length);
                Array.Copy(suffixBytes, 0, toSend, originalLength, suffixBytes.Length);
            }

            this.serialPort.DataReceived += DataReceivedHandler;
            this.serialPort.Open();
            this.readBytes.Clear();
            serialPort.Write(toSend, 0, toSend.Length);

            int sleepCount = 0;
            while (this.readBytes.Count != byteCount && sleepCount < waitTimeMs / WaitDelay)
            {
                sleepCount++;
                Thread.Sleep(WaitDelay);
            }

            this.serialPort.Close();
            return this.readBytes.Count == byteCount;
        }

        /// <summary>
        /// Executes a write command
        /// </summary>
        /// <param name="cmd">Command to be executed</param>
        /// <param name="value">Value to be written (as list of bytes)</param>
        /// <param name="waitTimeMs">Maximum time to wait until the response arrive</param>
        /// <param name="byteCount">Expected size of the response in bytes</param>
        /// <returns>true on success, otherwise false</returns>
        private bool ExecuteWriteCommand(OptrisCommands cmd, IEnumerable<byte> value, int waitTimeMs, short byteCount)
        {
            List<byte> toSend = new List<byte> {(byte)(0xB0 + this.multiAddress), (byte) cmd};
            toSend.AddRange(value.Reverse());
            byte checksum = CalculateChecksum(toSend.Skip(1));
            toSend.Add(checksum);

            this.serialPort.DataReceived += DataReceivedHandler;
            this.serialPort.Open();
            this.readBytes.Clear();
            serialPort.Write(toSend.ToArray(), 0, toSend.Count);

            int sleepCount = 0;
            while (this.readBytes.Count != byteCount && sleepCount < waitTimeMs / WaitDelay)
            {
                sleepCount++;
                Thread.Sleep(WaitDelay);
            }

            this.serialPort.Close();
            return this.readBytes.Count == byteCount;
        }

        /// <summary>
        /// Calculates the XOR checksum of the specified bytes
        /// </summary>
        /// <param name="input">Input bytes</param>
        /// <returns>Calculated checksum</returns>
        private static byte CalculateChecksum(IEnumerable<byte> input)
        {
            byte result = 0x0;
            IEnumerable<byte> enumerable = input.ToList();
            if (!enumerable.Any())
            {
                return result;
            }

            foreach (byte b in enumerable)
            {
                result = (byte)(result ^ b);
            }
            return result;
        }

        /// <summary>
        /// Groups the read temperatures by address
        /// When reading the temperature in line mode, we receive two bytes for each address
        /// The address starts with the index 1
        /// </summary>
        /// <returns>List of addresses and its own temperature</returns>
        private Dictionary<byte, decimal> SplitReadTemperatures()
        {
            Dictionary<byte, decimal> temperatures = new Dictionary<byte, decimal>();
            const int size = 2;

            byte address = 1;
            for (int i = 0; i < this.readBytes.Count / size; i++)
            {
                decimal temp = ConvertToCelsius(this.readBytes.Skip(i * size).Take(size).ToList());
                temperatures.Add(address, temp);
                address++;
            }

            return temperatures;
        }

        /// <summary>
        /// Converts a two byte sequence into Celsius degrees
        /// </summary>
        /// <param name="data">Input bytes</param>
        /// <returns>Temperature in °C</returns>
        private static decimal ConvertToCelsius(IReadOnlyList<byte> data)
        {
            if (data.Count != 2)
            {
                return decimal.MinValue;
            }

            return (decimal)(data[0] * 256 + data[1] - 1000) / 10;
        }

        /// <summary>
        /// Converts a two byte sequence into Celsius degrees
        /// </summary>
        /// <param name="data">Input bytes</param>
        /// <returns>Converted float value</returns>
        private static float ConvertToFloat(IReadOnlyList<byte> data)
        {
            if (data.Count != 2)
            {
                return float.MinValue;
            }

            return (float)(data[0] * 256 + data[1]) / 1000;
        }

        /// <summary>
        /// Data handler
        /// </summary>
        /// <param name="sender">Sender (the serial port)</param>
        /// <param name="e">Event</param>
        private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            if (!sp.IsOpen)
            {
                return;
            }

            byte[] b = new byte[1024];
            try
            {
                int btr = sp.Read(b, 0, sp.BytesToRead);
                for (int i = 0; i < btr; i++)
                {
                    this.readBytes.Add(b[i]);
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }

        /// <summary>
        /// Cleanup resources
        /// </summary>
        public void Dispose()
        {
            this.serialPort.DataReceived -= DataReceivedHandler;
            if (this.serialPort.IsOpen)
            {
                this.serialPort.Close();
            }
        }
    }
}

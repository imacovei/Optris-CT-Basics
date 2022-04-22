// ----------------------------------------------------------------------------
// <copyright file="OptrisCtManager.cs" company="Private">
// Copyright (c) 2021 All Rights Reserved
// </copyright>
// <author>Iulian Macovei</author>
// <date>04/22/2022 21:12:28 AM</date>
// ----------------------------------------------------------------------------

#region License
// ----------------------------------------------------------------------------
// Copyright 2022 Iulian Macovei
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

using CLI;
using IPC;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Globalization;
using System.IO;
using System.Text;

namespace OptrisCT.cmd.Commands
{
    public class ReadTemperatures: Command
    {
        public ReadTemperatures() : base("temperatures", "Reads the temperature from multiple address continuously for the specified amount of time")
        {
            this.AddOption(new CommonOptions.ComPortOption()
            {
                IsRequired = true
            });

            this.AddOption(new Option<string>(new[] { "-A", "--addresses" }, "The list of multi-address of the devices, semicolon separated")
            {
                IsRequired = true
            });

            this.AddOption(new CommonOptions.NumericOption<int>(new[] { "-d", "--duration" }, "The duration of the operation in milliseconds")
            {
                IsRequired = true
            });

            this.AddOption(new Option<string>(new[] { "-C", "--corrections" }, "The list of correction values for the read temperatures, semicolon separated"));
            this.AddOption(new CommonOptions.NumericOption<bool>(new[] { "-l", "--log" }, "Indicates if the measured data will be saved into a CSV log file"));
        }

        public static void ExecuteCommand(TemperaturesOptions options)
        {
            Response executionResponse = new Response();
            (bool splitResult, string splitErrMsg, List<byte> splitAddresses) = SplitAddresses(options.Addresses);
            if (!splitResult)
            {
                executionResponse.ErrorOccurred = true;
                executionResponse.ErrorMessage = new List<string> { splitErrMsg };
                return;
            }

            (bool cSplitResult, string cSplitErrMsg, Dictionary<byte, decimal> splitCorrections) = SplitCorrections(options.Corrections, splitAddresses);
            if (!cSplitResult)
            {
                executionResponse.ErrorOccurred = true;
                executionResponse.ErrorMessage = new List<string> { cSplitErrMsg };
                return;
            }


            if (splitAddresses.Count != splitCorrections.Count)
            {
                executionResponse.ErrorOccurred = true;
                executionResponse.ErrorMessage = new List<string> { $"The list of addresses and the list of correction values must have the same length. Provided have been {splitAddresses.Count} addresses and {splitCorrections.Count} correction values" };
                return;
            }

            try
            {
                OptrisCtManager optrisCtManager = new OptrisCtManager(options.Port, splitAddresses);
                Dictionary<byte, Dictionary<long, decimal>> lineModeTemperatures = optrisCtManager.MonitorTemperatureLineMode(options.Duration, splitCorrections);

                if (lineModeTemperatures.Count == 0)
                {
                    executionResponse.ErrorOccurred = true;
                    executionResponse.ErrorMessage.Add($"No temperatures could be read from the device connected on the port {options.Port}");
                }
                else
                {
                    executionResponse.Data = lineModeTemperatures;
                }
            }
            catch (Exception e)
            {
                executionResponse.Data = string.Empty;
                executionResponse.ErrorOccurred = true;
                executionResponse.ErrorMessage = new List<string> { e.Message };
            }
            finally
            {
                if (options.Log)
                {
                    SaveDataToCsv(executionResponse.Data);
                }
                Console.WriteLine(executionResponse.ToJson());
            }
        }

        private static (bool, string, List<byte>) SplitAddresses(string addresses)
        {
            List<byte> addrBytes = new List<byte>();

            if (!string.IsNullOrEmpty(addresses))
            {
                foreach (string addr in addresses.Split(";", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
                {
                    if (addr.Length != 1)
                    {
                        return (false, "Only addresses from 1 to 4 are allowed", null);
                    }
                    else
                    {
                        addrBytes.Add((byte)(addr[0] - 48));
                    }
                }
            }

            return (true, string.Empty, addrBytes);
        }

        private static (bool, string, Dictionary<byte, decimal>) SplitCorrections(string corrections, List<byte> addresses)
        {
            Dictionary<byte, decimal> corrBytes = new Dictionary<byte, decimal>();

            if (!string.IsNullOrEmpty(corrections))
            {
                byte index = 1;
                foreach (string corr in corrections.Split(";", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
                {
                    string cultureInvariantCorr = corr.Replace(",", ".");

                    if (!addresses.Contains(index))
                    {
                        return (false, $"A temperature correction value '{corr}' was provided for the address {index} but this address is not in the list of addresses", null);
                    }

                    if (decimal.TryParse(cultureInvariantCorr, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal decimalValue))
                    {
                        corrBytes.Add(index++, decimalValue);
                    }
                    else
                    {
                        return(false,$"Invalid value provided for the temperature correction {corr}", null);
                    }
                }
            }

            return (true, string.Empty, corrBytes);
        }


        public class TemperaturesOptions
        {
            public string Port { get; set; }

            public string Addresses { get; set; }

            public int Duration { get; set; }

            public string Corrections { get; set; }

            public bool Log { get; set; }

            public TemperaturesOptions(string port, string addresses, int duration, string corrections, bool log)
            {
                Port = port;
                Addresses = addresses;
                Duration = duration;
                Corrections = corrections;
                Log = log;
            }
        }

        private static void SaveDataToCsv(object data)
        {
            if (data == null)
            {
                return;
            }

            string filename = $"Temperature_Measurement_{DateTime.Now:yyyyMMdd'_'HHmmss}.csv";
            if (data is not Dictionary<long, decimal> dictionary)
            {
                return;
            }

            StringBuilder sb = new StringBuilder();
            foreach ((long key, decimal value) in dictionary)
            {
                sb.AppendLine($"{key};{value}");
            }

            try
            {
                File.AppendAllText(filename, sb.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

    }
}

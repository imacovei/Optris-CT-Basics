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
using System.IO;
using System.Text;

namespace OptrisCT.cmd.Commands
{
    public class ReadTemperature: Command
    {
        public ReadTemperature() : base("temperature", "Reads the temperature from one single address continuously for the specified amount of time")
        {
            this.AddOption(new CommonOptions.ComPortOption()
            {
                IsRequired = true
            });

            this.AddOption(new CommonOptions.NumericOption<byte>(new[] { "-a", "--address" }, "The multi-address of the device")
            {
                IsRequired = true
            });

            this.AddOption(new CommonOptions.NumericOption<int>(new[] { "-d", "--duration" }, "The duration of the operation in milliseconds")
            {
                IsRequired = true
            });

            this.AddOption(new CommonOptions.NumericOption<decimal>(new[] { "-c", "--correction" }, "The correction value for the read temperature"));
            this.AddOption(new CommonOptions.NumericOption<bool>(new[] { "-l", "--log" }, "Indicates if the measured data will be saved into a CSV log file"));
        }

        public static void ExecuteCommand(TemperatureOptions options)
        {
            Response executionResponse = new Response();
            try
            {
                OptrisCtManager optrisCtManager = new OptrisCtManager(options.Port, options.Address);
                Dictionary<long, decimal> temperatures = optrisCtManager.MonitorTemperature(options.Duration, options.Correction);

                if (temperatures.Count == 0)
                {
                    executionResponse.ErrorOccurred = true;
                    executionResponse.ErrorMessage.Add($"No temperatures could be read from the device connected on the port {options.Port}");
                }
                else
                {
                    executionResponse.Data = temperatures;
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

        public class TemperatureOptions
        {
            public string Port { get; set; }

            public byte Address { get; set; }

            public int Duration { get; set; }

            public decimal Correction { get; set; }

            public bool Log { get; set; }

            public TemperatureOptions(string port, byte address, int duration, decimal correction, bool log)
            {
                Port = port;
                Address = address;
                Duration = duration;
                Correction = correction;
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

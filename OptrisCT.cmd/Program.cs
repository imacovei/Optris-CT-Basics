// ----------------------------------------------------------------------------
// <copyright file="Program.cs" company="Private">
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

namespace OptrisCT.cmd
{
    using CommandLine;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Newtonsoft.Json;

    /// <summary>
    /// Command line options
    /// </summary>
    public class Options
    {
        [Option('p', "port", Required = true, HelpText = "Set the name of the serial port. E.g.: COM12")]
        public string Port { get; set; }

        [Option('o', "operation", Required = true, HelpText = "Set the name of the operation to be executed.\nAvailable operations are: 'temperature', 'serial-number', 'fw-version' and 'emissivity'")]
        public string Operation { get; set; }

        [Option('d', "duration", Required = false, HelpText = "Set the duration of the operation in milliseconds. Available only for the operation 'temperature'")]
        public int Duration { get; set; }

        [Option('a', "address", Required = false, HelpText = "Set the multi-address of the device")]
        public byte Address { get; set; }

        [Option('A', "addresses", Required = false, HelpText = "Set the list of multi-address of the devices, semicolon separated")]
        public string Addresses { get; set; }

        [Option('s', "set-value", Required = false, HelpText = "Set the value to be written in the device. If this parameter is set, then it transforms the read operation into a write operation. As of now the only operation accepting a value is 'emission'")]
        public string SetValue { get; set; }
    }

    class Program
    {
        /// <summary>
        /// Program entry point
        /// </summary>
        /// <param name="args">Command line arguments</param>
        static void Main(string[] args)
        {
            bool errorOccurred = false;
            List<string> errors = new List<string>();

            OptrisCtManager.OptrisCommands cmd = OptrisCtManager.OptrisCommands.Unknown;
            string port = string.Empty;
            int duration = int.MinValue;
            byte address = 1;
            List<byte> addresses = new List<byte>();
            float emissivity = float.MinValue;

            Parser.Default.ParseArguments<Options>(args).WithParsed<Options>(o =>
            {
                if (string.IsNullOrWhiteSpace(o.Port))
                {
                    errorOccurred = true;
                    errors.Add(Resource.ERR_EMPTY_COM_PORT);
                }
                else
                {
                    port = o.Port;
                }

                if (!string.IsNullOrWhiteSpace(o.Operation))
                {
                    cmd = o.Operation switch
                    {
                        "temperature-line-mode" => OptrisCtManager.OptrisCommands.ReadTemperatureLineMode,
                        "temperatures" => OptrisCtManager.OptrisCommands.ReadTemperatureLineMode,
                        "temps" => OptrisCtManager.OptrisCommands.ReadTemperatureLineMode,

                        "temperature" => OptrisCtManager.OptrisCommands.ReadTemperature,
                        "temp" => OptrisCtManager.OptrisCommands.ReadTemperature,

                        "serial-number" => OptrisCtManager.OptrisCommands.ReadSerialNumber,
                        "serial" => OptrisCtManager.OptrisCommands.ReadSerialNumber,
                        "serialnumber" => OptrisCtManager.OptrisCommands.ReadSerialNumber,

                        "sn" => OptrisCtManager.OptrisCommands.ReadSerialNumber,

                        "fw-version" => OptrisCtManager.OptrisCommands.ReadFwVersion,
                        "fw" => OptrisCtManager.OptrisCommands.ReadFwVersion,
                        "fwversion" => OptrisCtManager.OptrisCommands.ReadFwVersion,

                        "emissivity" => OptrisCtManager.OptrisCommands.ReadEmissivity,

                        _ => OptrisCtManager.OptrisCommands.Unknown
                    };

                    if (!string.IsNullOrEmpty(o.SetValue))
                    {
                        if (cmd == OptrisCtManager.OptrisCommands.ReadEmissivity)
                        {
                            if (float.TryParse(o.SetValue, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out emissivity) && emissivity is >= 0.0F and <= 1.1F)
                            {
                                cmd = OptrisCtManager.OptrisCommands.SetEmissivity;
                            }
                            else
                            {
                                errorOccurred = true;
                                errors.Add(string.Format(Resource.ERR_INVALID_EMISSIVITY_VALUE, o.SetValue));
                            }
                        }
                    }
                }
                else
                {
                    errorOccurred = true;
                    errors.Add(Resource.ERR_NO_OPERATION_PROVIDED);
                }

                duration = o.Duration;
                address = o.Address;

                if (!string.IsNullOrEmpty(o.Addresses))
                {
                    foreach (string addr in o.Addresses.Split(";", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
                    {
                        if (addr.Length != 1)
                        {
                            errorOccurred = true;
                        }
                        else
                        {

                            addresses.Add((byte)(addr[0] - 48));
                        }
                    }
                }
            }).WithNotParsed(HandleParseError);

            if (cmd == OptrisCtManager.OptrisCommands.Unknown)
            {
                errorOccurred = true;
                errors.Add(Resource.ERR_UNKNWON_CMD_LINE_ARG);
            }

            if (errors.Count > 0)
            {
                Response r = new Response
                {
                    ErrorMessage = errors,
                    ErrorOccurred = errorOccurred
                };
                Console.WriteLine(JsonConvert.SerializeObject(r));
                Environment.Exit(-1);
            }

            object data = null;
            try
            {
                OptrisCtManager optrisCtManager = cmd == OptrisCtManager.OptrisCommands.ReadTemperatureLineMode ?
                    new OptrisCtManager(port, addresses) : 
                    new OptrisCtManager(port, address);

                switch (cmd)
                {
                    case OptrisCtManager.OptrisCommands.ReadTemperatureLineMode:
                        Dictionary<byte, Dictionary<long, decimal>> lineModeTemperatures = optrisCtManager.MonitorTemperatureLineMode(duration);

                        if (lineModeTemperatures.Count == 0)
                        {
                            errorOccurred = true;
                            errors.Add(string.Format(Resource.ERR_CANNOT_READ_TEMPERATURE, port));
                        }
                        else
                        {
                            data = lineModeTemperatures;
                        }

                        break;
                    case OptrisCtManager.OptrisCommands.ReadTemperature:
                        Dictionary<long, decimal> temperatures = optrisCtManager.MonitorTemperature(duration);

                        if (temperatures.Count == 0)
                        {
                            errorOccurred = true;
                            errors.Add(string.Format(Resource.ERR_CANNOT_READ_TEMPERATURE, port));
                        }
                        else
                        {
                            data = temperatures;
                        }

                        break;
                    case OptrisCtManager.OptrisCommands.ReadSerialNumber:
                        data = optrisCtManager.ReadSerialNumber();
                        break;
                    case OptrisCtManager.OptrisCommands.ReadFwVersion:
                        data = optrisCtManager.ReadFwVersion();
                        break;
                    case OptrisCtManager.OptrisCommands.ReadEmissivity:
                        data = optrisCtManager.ReadEmissivity();
                        break;
                    case OptrisCtManager.OptrisCommands.SetEmissivity:
                        if (!optrisCtManager.SetEmissivity(emissivity))
                        {
                            errors.Add(string.Format(Resource.ERR_CANNOT_SET_EMISSIVITY, emissivity));
                            errorOccurred = true;
                        }

                        break;
                    default:
                        throw new ArgumentOutOfRangeException(Resource.ERR_UNKNWON_CMD_LINE_ARG);
                }
            }
            catch (Exception e)
            {
                errorOccurred = true;
                errors.Add(e.Message);
            }
            finally
            {
                Response r = new Response
                {
                    ErrorMessage = errors,
                    ErrorOccurred = errorOccurred,
                    Data = data
                };

                Console.WriteLine(JsonConvert.SerializeObject(r));
                Environment.ExitCode = errorOccurred ? -1 : 0;
            }
        }

        /// <summary>
        /// Handles the of the command line arguments
        /// </summary>
        /// <param name="errors"></param>
        private static void HandleParseError(IEnumerable<Error> errors)
        {
            Response r = new Response
            {
                ErrorMessage = errors.Select(error => error.ToString()).ToList(),
                ErrorOccurred = true
            };

            Console.WriteLine(JsonConvert.SerializeObject(r));
            Environment.Exit(-1);
        }
    }

    /// <summary>
    /// Defines the response object
    /// </summary>
    public class Response
    {
        internal bool ErrorOccurred;
        internal List<string> ErrorMessage;
        internal object Data;
    }
}

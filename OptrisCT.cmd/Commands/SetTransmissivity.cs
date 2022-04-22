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

namespace OptrisCT.cmd.Commands
{
    public class SetTransmissivity : Command
    {
        public SetTransmissivity() : base("set-transmissivity", "Sets the transmissivity")
        {
            this.AddOption(new CommonOptions.ComPortOption()
            {
                IsRequired = true
            });

            this.AddOption(new CommonOptions.NumericOption<byte>(new[] { "-a", "--address" }, "The multi-address of the device")
            {
                IsRequired = true
            });

            this.AddOption(new CommonOptions.NumericOption<float>(new[] { "-s", "--set-value" }, "Value to be set on the device")
            {
                IsRequired = true
            });
        }

        public static void ExecuteCommand(SetTransmissivityOptions options)
        {
            Response executionResponse = new Response();

            if (options.SetValue is < 0.0F or > 1.1F)
            {
                executionResponse.ErrorOccurred = true;
                executionResponse.ErrorMessage = new List<string> { $"Invalid value provided for setting the transmissivity: {options.SetValue}" };
                return;
            }

            try
            {
                OptrisCtManager optrisCtManager = new OptrisCtManager(options.Port, options.Address);
                if (!optrisCtManager.SetTransmissivity(options.SetValue))
                {
                    executionResponse.ErrorOccurred = true;
                    executionResponse.ErrorMessage = new List<string> { $"Error setting the SetTransmissivityOptions to: {options.SetValue}" };
                }
            }
            catch (Exception e)
            {
                executionResponse.ErrorOccurred = true;
                executionResponse.ErrorMessage = new List<string> { e.Message };
            }
            finally
            {
                Console.WriteLine(executionResponse.ToJson());
            }
        }

        public class SetTransmissivityOptions
        {
            public string Port { get; set; }

            public byte Address { get; set; }

            public float SetValue { get; set; }

            public SetTransmissivityOptions(string port, byte address, float setValue)
            {
                Port = port;
                Address = address;
                SetValue = setValue;
            }
        }
    }
}

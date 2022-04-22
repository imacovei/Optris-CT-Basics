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

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OptrisCT.cmd.Commands;
using System;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Hosting;
using System.CommandLine.NamingConventionBinder;
using System.CommandLine.Parsing;
using Microsoft.Extensions.Logging;

namespace OptrisCT.cmd
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Parser parser = BuildCommandLine()
                .UseHost(_ => Host.CreateDefaultBuilder(args), (builder) =>
                {
                    builder.ConfigureServices(services =>
                    {
                        services.AddSingleton<ReadTemperature>();
                        services.AddSingleton<ReadTemperatures>();
                        services.AddSingleton<ReadSerialNumber>();
                        services.AddSingleton<ReadEmissivity>();
                        services.AddSingleton<ReadTransmissivity>();
                        services.AddSingleton<SetEmissivity>();
                        services.AddSingleton<SetTransmissivity>();
                    }).ConfigureLogging((_, logging) =>
                    {
                        logging.ClearProviders();
                    });
                }).UseDefaults().Build();

            int r = parser.Invoke(args);
            Environment.ExitCode = r;
        }

        private static CommandLineBuilder BuildCommandLine()
        {
            RootCommand root = new RootCommand();

            root.AddCommand(new ReadTemperature()
            {
                Handler = CommandHandler.Create<ReadTemperature.TemperatureOptions>(ReadTemperature.ExecuteCommand)
            });

            root.AddCommand(new ReadTemperatures()
            {
                Handler = CommandHandler.Create<ReadTemperatures.TemperaturesOptions>(ReadTemperatures.ExecuteCommand)
            });

            root.AddCommand(new ReadSerialNumber()
            {
                Handler = CommandHandler.Create<ReadSerialNumber.SerialNumberOptions>(ReadSerialNumber.ExecuteCommand)
            });

            root.AddCommand(new ReadEmissivity()
            {
                Handler = CommandHandler.Create<ReadEmissivity.EmissivityOptions>(ReadEmissivity.ExecuteCommand)
            });

            root.AddCommand(new ReadTransmissivity()
            {
                Handler = CommandHandler.Create<ReadTransmissivity.TransmissivityOptions>(ReadTransmissivity.ExecuteCommand)
            });

            root.AddCommand(new SetEmissivity()
            {
                Handler = CommandHandler.Create<SetEmissivity.SetEmissivityOptions>(SetEmissivity.ExecuteCommand)
            });

            root.AddCommand(new SetTransmissivity()
            {
                Handler = CommandHandler.Create<SetTransmissivity.SetTransmissivityOptions>(SetTransmissivity.ExecuteCommand)
            });

            CommandLineBuilder builder = new CommandLineBuilder(root);
            return builder;
        }
    }
}


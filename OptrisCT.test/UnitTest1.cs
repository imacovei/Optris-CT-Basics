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

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Xunit;

// as we access the serial port as shared resource, we can't let xunit run the test in parallel
// ergo: disable the parallel execution of the tests:
[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace OptrisCT.test
{
    public class UnitTest1
    {
        private const string ComPort = "COM8";
        private const byte Address = 1;

        [Fact]
        public void TestSerialNumber()
        {
            int sn;
            using (OptrisCtManager mgr = new OptrisCtManager(ComPort, Address))
            {
                sn = mgr.ReadSerialNumber();
            }

            Assert.NotEqual(0, sn);
        }

        [Fact]
        public void TestFwVersion()
        {
            int fwVersion;
            using (OptrisCtManager mgr = new OptrisCtManager(ComPort, Address))
            {
                fwVersion = mgr.ReadFwVersion();
            }

            Assert.NotEqual(0, fwVersion);
        }

        [Fact]
        public void TestReadEmissivity()
        {
            float emissivity;
            using (OptrisCtManager mgr = new OptrisCtManager(ComPort, Address))
            {
                emissivity = mgr.ReadEmissivity();
            }

            Assert.NotEqual(0, emissivity);
        }

        [Fact]
        public void TestWriteEmissivity()
        {
            bool emissivity;
            using (OptrisCtManager mgr = new OptrisCtManager(ComPort, Address))
            {
                emissivity = mgr.SetEmissivity(0.99F);
            }

            Assert.True(emissivity);
        }

        [Fact]
        public void TestReadTransmissivity()
        {
            float transmissivity;
            using (OptrisCtManager mgr = new OptrisCtManager(ComPort, Address))
            {
                transmissivity = mgr.ReadTransmissivity();
            }

            Assert.NotEqual(0, transmissivity);
        }

        [Fact]
        public void TestWriteTransmissivity()
        {
            bool transmissivity;
            using (OptrisCtManager mgr = new OptrisCtManager(ComPort, Address))
            {
                transmissivity = mgr.SetTransmissivity(0.99F);
            }

            Assert.True(transmissivity);
        }

        [Fact]
        public void TestMonitorTemperatures()
        {
            Dictionary<long, decimal> temperatures;
            using (OptrisCtManager mgr = new OptrisCtManager(ComPort, Address))
            {
                temperatures = mgr.MonitorTemperature(1000, 0);
            }

            Assert.True(temperatures.Any());
        }
    }
}

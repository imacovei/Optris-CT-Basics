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

using System;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.Globalization;
using System.Linq;

namespace CLI
{
    public static class CommonOptions
    {
        public class ComPortOption : Option<string>
        {
            private const string description = "COM port";
            private static readonly string[] aliases = new[] { "-p", "--port" };

            public ComPortOption() : base(aliases, ParseComPortArgument, false, description)
            {
            }
        }

        public class NumericOption<T> : Option<T>
        {
            public NumericOption(string[] aliases, string description) 
                : base(aliases, ParseArgument<T>, false, description)
            {
            }
        }

        internal static string ParseComPortArgument(ArgumentResult argResult)
        {
            if (argResult.Tokens.Any())
            {
                // this is the user input, if any:
                string firstValue = argResult.Tokens.Single().Value;

                if (firstValue.StartsWith("COM", StringComparison.InvariantCultureIgnoreCase))
                {
                    return firstValue.ToUpperInvariant();
                }

                argResult.ErrorMessage = "Invalid value provided for the argument " + argResult.Argument;
                return null;
            }

            argResult.ErrorMessage = "COM port is missing";
            return null;
        }

        internal static T ParseArgument<T>(ArgumentResult argResult)
        {
            if (argResult.Tokens.Any())
            {
                // this is the user input, if any:
                string firstValue = argResult.Tokens.Single().Value;

                if (typeof(T) == typeof(int))
                {
                    if (int.TryParse(firstValue, NumberStyles.Any, CultureInfo.InvariantCulture, out int intValue))
                    {
                        return (T)(object)intValue;
                    }
                }
                else if (typeof(T) == typeof(long))
                {
                    if(long.TryParse(firstValue, NumberStyles.Any, CultureInfo.InvariantCulture, out long longValue))
                    {
                        return (T)(object)longValue;
                    }
                }
                else if (typeof(T) == typeof(decimal))
                {
                    if (decimal.TryParse(firstValue, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal decimalValue))
                    {
                        return (T)(object)decimalValue;
                    }
                }
                else if (typeof(T) == typeof(byte))
                {
                    if (byte.TryParse(firstValue, NumberStyles.Any, CultureInfo.InvariantCulture, out byte byteValue))
                    {
                        return (T)(object)byteValue;
                    }
                }
                else if (typeof(T) == typeof(bool))
                {
                    if (bool.TryParse(firstValue, out bool boolValue))
                    {
                        return (T)(object)boolValue;
                    }
                }
                else if (typeof(T) == typeof(float))
                {
                    if (float.TryParse(firstValue, NumberStyles.Any, CultureInfo.InvariantCulture, out float floatValue))
                    {
                        return (T)(object)floatValue;
                    }
                }
                else
                {
                    argResult.ErrorMessage = $"Unknown argument type provided: {typeof(T)}";
                    return default;
                }

                argResult.ErrorMessage = $"Invalid value provided for the argument {argResult.Argument}";
                return default;
            }

            argResult.ErrorMessage = "Integer argument is missing";
            return default;
        }
    }
}

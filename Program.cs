using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System.IO;

namespace JsonRazerBeam
{
    public class Program
    {
        [Params(@"/Users/BenjaminPinter/Projects/JsonRazerBeam/AAPL-20210104-Quotes.json")]
        public static string FileLocation { get; set; }

        public static void Main(string[] args)
        {
            BenchmarkRunner.Run(typeof(Program).Assembly);
        }


        [Benchmark]
        [Arguments(1609750824955201013, 1)]
        [Arguments(1609750824955201013, 10)]
        [Arguments(1609750824955201013, 100)]
        [Arguments(1609750824955201013, 1000)]
        public List<Quote> GetOrders(long epochStart, int numOrders)
        {
            int quotesFilled = 0;
            List<Quote> quotes = new List<Quote>(numOrders);
            ReadOnlySpan<byte> jsonReadOnlySpan = File.ReadAllBytes(FileLocation);
            byte[] searchValue = Encoding.UTF8.GetBytes("rs");

            var bom = new byte[] { 0xEF, 0xBB, 0xBF };
            if (jsonReadOnlySpan.StartsWith(bom))
            {
                jsonReadOnlySpan = jsonReadOnlySpan.Slice(bom.Length);
            }
            bool timeStampFound = false;
            bool timeStampOld = false;
            var reader = new Utf8JsonReader(jsonReadOnlySpan);
            while (reader.Read())
            {
                JsonTokenType tokenType = reader.TokenType;

                switch (tokenType)
                {
                    case JsonTokenType.StartObject:
                        break;
                    case JsonTokenType.PropertyName:
                        timeStampFound = false;
                        timeStampOld = false;
                        reader.Read();
                        string jsonString = string.Empty;
                        while (reader.Read() && reader.TokenType != JsonTokenType.StartObject)
                        {
                            if (timeStampOld)
                            {
                                while(reader.TokenType != JsonTokenType.StartObject){
                                    reader.Read();
                                }
                                break;
                            }
                            switch (reader.TokenType)
                            {
                                case JsonTokenType.PropertyName:
                                    string propertyName = reader.GetString();
                                    if (propertyName == "t")
                                    {
                                        timeStampFound = true;
                                    }
                                    jsonString = jsonString + ($"\"{propertyName}\":");
                                    break;
                                case JsonTokenType.String:
                                    jsonString = jsonString + ($"\"{reader.GetString()}\",");
                                    break;
                                case JsonTokenType.False:
                                    jsonString = jsonString + ("false,");
                                    break;
                                case JsonTokenType.True:
                                    jsonString = jsonString + ("true,");
                                    break;
                                case JsonTokenType.Null:
                                    jsonString = jsonString + ("null,");
                                    break;
                                case JsonTokenType.Number:
                                    if (timeStampFound)
                                    {
                                        if (reader.TryGetInt64(out var timestamp))
                                        {
                                            timeStampFound = false;
                                            if (timestamp < epochStart)
                                            {
                                                timeStampOld = true;
                                                continue;
                                            }
                                            else
                                            {
                                                jsonString = jsonString + ($"{timestamp},");
                                                break;
                                            }
                                        }
                                    }
                                    if (reader.TryGetDecimal(out var dub))
                                    {
                                        jsonString = jsonString + ($"{dub},");
                                        break;
                                    }
                                    else if (reader.TryGetInt16(out var iSixteen))
                                    {
                                        jsonString = jsonString + ($"{iSixteen},");
                                        break;
                                    }
                                    else if (reader.TryGetInt32(out var iThirtytwo))
                                    {
                                        jsonString = jsonString + ($"{iThirtytwo},");
                                        break;
                                    }
                                    else if (reader.TryGetInt64(out var iSixtyFour))
                                    {
                                        jsonString = jsonString + ($"{iSixtyFour},");
                                        break;
                                    }
                                    break;
                            }
                        }
                        if (!timeStampOld)
                        {
                            quotes.Add(JsonSerializer.Deserialize<Quote>("{" + jsonString + "}", new JsonSerializerOptions() { AllowTrailingCommas = true }));
                            quotesFilled = quotesFilled + 1;
                            if (quotesFilled == numOrders)
                            {
                                return quotes;
                            }
                        }
                        break;
                }
            }
            return quotes;
        }
    }

    public class Quote
    {
        public string rs;
        public bool isActive;
        public string T;
        public long t;
        public long f;
        public long y;
        public double P;
        public double S;
        public double p;
        public double s;
    }
}

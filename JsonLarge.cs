using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Text.Json;
using BenchmarkDotNet.Attributes;
using ConsoleApp1.JsonObjects;
using Newtonsoft.Json;

namespace ConsoleApp1
{
    public class JsonLarge
    {
        public List<T> Load<T>(string filepath, object[] filters = null)
        {
            var list = new List<T>();

            using var mmf = MemoryMappedFile.CreateFromFile(filepath, FileMode.Open);
            using var vs = mmf.CreateViewStream();
            using var mmv = vs.SafeMemoryMappedViewHandle;

            unsafe
            {
                byte* ptrMemMap = (byte*)0;
                mmv.AcquirePointer(ref ptrMemMap);
                var bytes = new ReadOnlySpan<byte>(ptrMemMap, (int)mmv.ByteLength);
                mmv.ReleasePointer();

                var reader = new Utf8JsonReader(bytes, new JsonReaderOptions
                {
                    AllowTrailingCommas = true,
                    CommentHandling = JsonCommentHandling.Skip
                });

                DoJsonReader(ref reader, list);
            }

            return list;
        }

        public List<T> Load<T>(Stream s, object[] filters = null)
        {
            return null;
        }

        static unsafe void DoJsonReader<T>(ref Utf8JsonReader reader, List<T> list, object[] filters = null)
        {
            var sJson = string.Empty;

            bool hasPropertyName = false;
            bool isWithinArray = false;

            while (reader.Read())
            {
                if (list.Count >= 1000)
                {
                    break;
                }

                if (reader.TokenType == JsonTokenType.StartObject)
                {
                    sJson += ",{";
                }
                else if (reader.TokenType == JsonTokenType.EndObject)
                {
                    sJson = sJson.Trim(',') + "}";

                    if (!isWithinArray)
                    {
                        list.Add(JsonConvert.DeserializeObject<T>(sJson));
                        sJson = string.Empty;
                    }
                }
                else if (!hasPropertyName)
                {
                    if (reader.TokenType == JsonTokenType.PropertyName)
                    {
                        sJson += $"\"{reader.GetString()}\":";

                        hasPropertyName = true;
                    }
                }
                else
                {
                    switch (reader.TokenType)
                    {
                        case JsonTokenType.String:
                            sJson += $"\"{reader.GetString()}\"";
                            break;

                        case JsonTokenType.Number:
                            if (reader.TryGetDecimal(out var dd))
                            {
                                sJson += $"{dd}";
                            }
                            else if (reader.TryGetInt16(out var i16))
                            {
                                sJson += $"{i16}";
                            }
                            else if (reader.TryGetInt32(out var i32))
                            {
                                sJson += $"{i32}";
                            }
                            else if (reader.TryGetInt64(out var i64))
                            {
                                sJson += $"{i64}";
                            }
                            break;

                        case JsonTokenType.Null:
                            sJson += "null";
                            break;

                        case JsonTokenType.True:
                            sJson += "true";
                            break;

                        case JsonTokenType.False:
                            sJson += "false";
                            break;

                        case JsonTokenType.StartArray:
                            sJson += "[";
                            isWithinArray = true;
                            break;

                        case JsonTokenType.EndArray:
                            sJson += "]";
                            isWithinArray = false;
                            break;
                    }

                    sJson += ",";

                    hasPropertyName = false;
                }
            }
        }

        [Benchmark]
        public void BenchmarkLoadByFile()
        {
            var quotes = Load<Quote>(@"C:\temp\cache\AAPL\AAPL-20210511-Quotes.json");
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;

public class Bencode
{
    public static object Decode(string input)
    {
        if (char.IsDigit(input[0]))
        {
            return DecodeString(input);
        }
        else if (input[0] == 'i')
        {
            return DecodeInt(input);
        }
        else if (input[0] == 'l')
        {
            return DecodeList(input);
        }
        else if (input[0] == 'd')
        {
            return DecodeDict(input);
        }
        else
        {
            throw new InvalidOperationException("Unhandled encoded value: " + input);
        }
    }

    public static string Encode(object input)
    {
        return input switch
        {
            int i => $"i{i}e",
            long l => $"i{l}e",
            string s => $"{s.Length}:{s}",
            object[] inputArray => $"l{string.Join("", inputArray.Select(x => Encode(x)))}e",
            Dictionary<string, object> dict => $"d{string.Join("", dict.Select(kv => Encode(kv.Key) + Encode(kv.Value)))}e",
            _ => throw new Exception($"Unknown type: {input.GetType().FullName}"),
        };
    }

    private static string DecodeString(string input)
    {
        int colonIndex = input.IndexOf(':');
        if (colonIndex != -1)
        {
            int strlen = int.Parse(input[..colonIndex]);
            return input.Substring(colonIndex + 1, strlen);
        }
        else
        {
            throw new InvalidOperationException("Invalid encoded value: " + input);
        }
    }

    private static long DecodeInt(string input)
    {
        int endIndex = input.IndexOf('e');
        if (endIndex == -1)
        {
            throw new InvalidOperationException("Invalid encoded integer: " + input);
        }
        return long.Parse(input[1..endIndex]);
    }

    private static object[] DecodeList(string input)
    {
        input = input[1..];
        var result = new List<object>();

        while (input.Length > 0 && input[0] != 'e')
        {
            var element = Decode(input);
            result.Add(element);
            int length = Encode(element).Length;
            input = input[length..];
        }

        if (input.Length == 0 || input[0] != 'e')
        {
            throw new InvalidOperationException("Invalid encoded list: " + input);
        }

        return result.ToArray();
    }

    private static Dictionary<string, object> DecodeDict(string input)
    {
        input = input[1..];
        var result = new Dictionary<string, object>();

        while (input.Length > 0 && input[0] != 'e')
        {
            string key = DecodeString(input);
            int keyLength = key.Length + key.Length.ToString().Length + 1;
            input = input[keyLength..];
            object value = Decode(input);
            int valueLength = Encode(value).Length;
            input = input[valueLength..];
            result[key] = value;
        }

        if (input.Length == 0 || input[0] != 'e')
        {
            throw new InvalidOperationException("Invalid encoded dictionary: " + input);
        }

        return result;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class Bencode
{
    public static object Decode(byte[] inputBytes)
    {
        var input = Encoding.UTF8.GetString(inputBytes);
        return Decode(ref input);
    }

    private static object Decode(ref string input)
    {
        if (char.IsDigit(input[0]))
        {
            return DecodeString(ref input);
        }
        else if (input[0] == 'i')
        {
            return DecodeInt(ref input);
        }
        else if (input[0] == 'l')
        {
            return DecodeList(ref input);
        }
        else if (input[0] == 'd')
        {
            return DecodeDict(ref input);
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
            Dictionary<string, object> dict => EncodeDictionary(dict),
            _ => throw new Exception($"Unknown type: {input.GetType().FullName}"),
        };
    }

    private static string EncodeDictionary(Dictionary<string, object> dict)
    {
        // Sort the dictionary keys lexicographically
        var sortedKeys = dict.Keys.OrderBy(k => k, StringComparer.Ordinal).ToList();
        
        // Build the encoded dictionary string
        var encodedDict = new StringBuilder("d");
        foreach (var key in sortedKeys)
        {
            string encodedKey = Encode(key);
            string encodedValue = Encode(dict[key]);
            encodedDict.Append(encodedKey).Append(encodedValue);
        }
        encodedDict.Append('e');

        return encodedDict.ToString();
    }

    private static string DecodeString(ref string input)
    {
        int colonIndex = input.IndexOf(':');
        if (colonIndex != -1)
        {
            int strlen = int.Parse(input[..colonIndex]);
            string result = input.Substring(colonIndex + 1, strlen);
            input = input[(colonIndex + 1 + strlen)..];
            return result;
        }
        else
        {
            throw new InvalidOperationException("Invalid encoded value: " + input);
        }
    }

    private static long DecodeInt(ref string input)
    {
        int endIndex = input.IndexOf('e');
        if (endIndex == -1)
        {
            throw new InvalidOperationException("Invalid encoded integer: " + input);
        }
        string intStr = input[1..endIndex];
        input = input[(endIndex + 1)..];
        return long.Parse(intStr);
    }

    private static object[] DecodeList(ref string input)
    {
        input = input[1..];
        List<object> result = [];

        while (input.Length > 0 && input[0] != 'e')
        {
            var element = Decode(ref input);
            result.Add(element);
        }

        if (input.Length == 0 || input[0] != 'e')
        {
            throw new InvalidOperationException("Invalid encoded list: " + input);
        }

        input = input[1..]; // Remove the 'e'
        return [.. result];
    }

    private static Dictionary<string, object> DecodeDict(ref string input)
    {
        input = input[1..];
        var result = new Dictionary<string, object>();

        while (input.Length > 0 && input[0] != 'e')
        {
            string key = DecodeString(ref input);
            object value = Decode(ref input);
            result[key] = value;
        }

        if (input.Length == 0 || input[0] != 'e')
        {
            throw new InvalidOperationException("Invalid encoded dictionary: " + input);
        }

        input = input[1..]; // Remove the 'e'
        return result;
    }
}

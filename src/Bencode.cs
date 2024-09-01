using System.Collections.Generic;

public class Bencode {
  public static object Decode(string input) {
    if (Char.IsDigit(input[0])) {
      return DecodeString(input);
    } else if (input[0] == 'i') {
      return DecodeInt(input);
    } else if (input[0] == 'l') {
      return DecodeList(input);
    } else if (input[0] == 'd') {
      return DecodeDict(input);  
    } else {
      throw new InvalidOperationException("Unhandled encoded value: " + input);
    }
  }
  
  public static string Encode(object input) {
    return Type.GetTypeCode(input.GetType()) switch
    {
        TypeCode.Int16 or TypeCode.Int32 or TypeCode.Int64 => $"i{input}e",
        TypeCode.String => $"{((string)input).Length}:{input}",
        TypeCode.Object =>
            input switch
            {
                object[] inputArray => $"l{string.Join("", inputArray.Select(x => Encode(x)))}e",
                Dictionary<string, object> dict => $"d{string.Join("", dict.Select(kv => Encode(kv.Key) + Encode(kv.Value)))}e",
                _ => throw new Exception($"Unknown type: {input.GetType().FullName}")
            },
        _ => throw new Exception($"Unknown type: {input.GetType().FullName}"),
    };
  }

  private static string DecodeString(string input) {
    int colonIndex = input.IndexOf(':');
    if (colonIndex != -1) {
      var strlen = int.Parse(input[..colonIndex]);
      return input.Substring(colonIndex + 1, strlen);
    } else {
      throw new InvalidOperationException("Invalid encoded value: " + input);
    }
  }

  private static long DecodeInt(string input) => long.Parse(input[1..input.IndexOf('e')]);
  private static object[] DecodeList(string input) {
    input = input[1..];
    var result = new List<object>();
    while (input.Length > 0 && input[0] != 'e') {
      var element = Decode(input);
      result.Add(element);
      input = input[Encode(element).Length..];
    }
    return result.ToArray();
  }

  private static Dictionary<string, object> DecodeDict(string input)
  {
      int index = 1; 
      var result = new Dictionary<string, object>();
      while (index < input.Length && input[index] != 'e')
      {
          // Decode the key (must be a string)
          string key = (string)DecodeString(input[index..]);
          index += key.Length + key.Length.ToString().Length + 1;  // Adjust the index after the key

          // Decode the value (can be any Bencode type)
          object value = Decode(input[index..]);
          
          // Add the key-value pair to the dictionary
          result[key] = value;

          // Adjust the index after the value
          if (value is string strValue)
          {
              index += strValue.Length + strValue.Length.ToString().Length + 1;
          }
          else if (value is long || value is int)
          {
              index = input.IndexOf('e', index) + 1;
          }
          else if (value is object[] || value is Dictionary<string, object>)
          {
              index += Encode(value).Length;
          }
      }

      return result;
  }
}
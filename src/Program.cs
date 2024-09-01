using System.Text.Json;

// Parse arguments
var (command, param) = args.Length switch
{
    0 => throw new InvalidOperationException("Usage: your_bittorrent.sh <command> <param>"),
    1 => throw new InvalidOperationException("Usage: your_bittorrent.sh <command> <param>"),
    _ => (args[0], args[1])
};

// Parse command and act accordingly
if (command == "decode") {
    var encodedValue = param;
    Console.WriteLine(JsonSerializer.Serialize(Bencode.Decode(encodedValue)));
} else {
  throw new InvalidOperationException($"Invalid command: {command}");
}
using System.Text.Json;
using System.Collections.Generic;

// Parse arguments
var (command, param) = args.Length switch
{
    0 => throw new InvalidOperationException("Usage: your_bittorrent.sh <command> <param>"),
    1 => throw new InvalidOperationException("Usage: your_bittorrent.sh <command> <param>"),
    _ => (args[0], args[1])
};

// Parse command and act accordingly
if (command == "decode")
{
    var encodedValue = param;
    var decodedObject = Bencode.Decode(encodedValue);
    Console.WriteLine(JsonSerializer.Serialize(decodedObject));
}
else if (command == "info")
{
    string fileContent;
    try
    {
        fileContent = File.ReadAllText(param);
    } catch (IOException ex)
    {
        Console.WriteLine($"Error reading file: {ex.Message}");
        return;
    }
    var decodedFile = Bencode.Decode(fileContent) as Dictionary<string, object> ?? throw new InvalidOperationException("Decoded file is not a dictionary.");
    if (!decodedFile.TryGetValue("info", out object? infoVal) || infoVal is not Dictionary<string, object> info)
    {
        throw new InvalidOperationException("The 'info' key is missing or is not a dictionary.");
    }
    if (!decodedFile.TryGetValue("announce", out object? announceValue))
    {
        throw new InvalidOperationException("The 'announce' key is missing.");
    }

    Console.WriteLine($"Tracker URL: {announceValue}\nLength: {info["length"]}");
}
else
{
    throw new InvalidOperationException($"Invalid command: {command}");
}
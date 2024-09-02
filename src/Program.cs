using System.Text.Json;
using System.Collections.Generic;
using System.Text;
using System.Net;

static Dictionary<string, object>? getDecodedFile(string param)
{
    string fileContent;
    try
    {
        fileContent = File.ReadAllText(param);
    }
    catch (IOException ex)
    {
        Console.WriteLine($"Error reading file: {ex.Message}");
        return null;
    }

    var decodedFile = Bencode.Decode(fileContent) as Dictionary<string, object> 
                      ?? throw new InvalidOperationException("Decoded file is not a dictionary.");
    return decodedFile;
}

static List<string> parsePeers(byte[] peersData)
{
    List<string> peersList = [];
    int peerCount = peersData.Length / 6;

    for (int i = 0; i < peerCount; i++)
    {
        byte[] ipBytes = new byte[4];
        Array.Copy(peersData, i * 6, ipBytes, 0, 4);
        string ipAddress = new IPAddress(ipBytes).ToString();

        byte[] portBytes = new byte[2];
        Array.Copy(peersData, i * 6 + 4, portBytes, 0, 2);
        int port = (portBytes[0] << 8) | portBytes[1];

        peersList.Add($"{ipAddress}:{port}");
    }

    return peersList;
}

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
    var decodedFile = getDecodedFile(param);
    if (decodedFile == null || !decodedFile.TryGetValue("info", out object? infoVal) || infoVal is not Dictionary<string, object> info)
    {
        throw new InvalidOperationException("The 'info' key is missing or is not a dictionary.");
    }

    if (!decodedFile.TryGetValue("announce", out object? announceValue))
    {
        throw new InvalidOperationException("The 'announce' key is missing.");
    }

    string hashedInfo = Hash.EncryptHashToHex(Bencode.Encode(info));
    List<string> pieceList = Hash.ListHashes(info);
    Console.WriteLine($"Tracker URL: {announceValue}\nLength: {info["length"]}\nInfo Hash: {hashedInfo}\nPiece Length: {info["piece length"]}\nPieces:\n{string.Join("\n", pieceList)}");
}
else if (command == "peers")
{
    var decodedFile = getDecodedFile(param);
    
    if (decodedFile == null || !decodedFile.TryGetValue("info", out object? infoVal) || infoVal is not Dictionary<string, object> info)
    {
        throw new InvalidOperationException("The 'info' key is missing or is not a dictionary.");
    }

    if (!decodedFile.TryGetValue("announce", out object? announceValue))
    {
        throw new InvalidOperationException("The 'announce' key is missing.");
    }

    byte[] infoHash = Hash.EncryptHash(Bencode.Encode(info));
    string peerId = "00112233445566778899";
    int port = 6881;
    int uploaded = 0;
    int downloaded = 0;
    long left = (long)info["length"];
    int compact = 1;

    Dictionary<string, string> queries = new()
    {
        { "info_hash", Web.UrlEncodeInfoHash(infoHash) },
        { "peer_id", peerId },
        { "port", port.ToString() },
        { "uploaded", uploaded.ToString() },
        { "downloaded", downloaded.ToString() },
        { "left", left.ToString() },
        { "compact", compact.ToString() },
    };

    string trackerUrl = (string)announceValue; 
    string? responseBody = await Web.GetAsync(trackerUrl, queries);
    if (responseBody == null)
    {
        Console.WriteLine("Failed to get a response from the tracker.");
        return;
    }

    var response = Bencode.Decode(responseBody) as Dictionary<string, object>
                   ?? throw new InvalidOperationException("Tracker response is not a dictionary.");
    
    foreach (KeyValuePair<string, object> kvp in response)
    {
        Console.WriteLine("Key = {0}, Value = {1}", kvp.Key, kvp.Value);
    }

    if (!response.TryGetValue("peers", out object? peersValue) || peersValue is not byte[] peersData)
    {
        throw new InvalidOperationException("The 'peers' key is missing or is not a byte array.");
    }

    List<string> peers = parsePeers(peersData);
    foreach (var peer in peers)
    {
        Console.WriteLine(peer);
    }
}
else
{
    throw new InvalidOperationException($"Invalid command: {command}");
}

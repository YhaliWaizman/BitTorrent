using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

public class Hash
{
    // Convert SHA-1 hash to a hexadecimal string
    public static string EncryptHashToHex(string input)
    {
        byte[] hashBytes = SHA1.HashData(Encoding.UTF8.GetBytes(input));
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
    }

    // Compute SHA-1 hash of the input string and return the raw byte array
    public static byte[] EncryptHash(string input)
    {
        return SHA1.HashData(Encoding.UTF8.GetBytes(input));
    }

    // Convert a pieces string into a list of hexadecimal hashes
    public static List<string> ListHashes(Dictionary<string, object> info)
    {
        var pieceList = new List<string>();
        if (info.TryGetValue("pieces", out var piecesObj) && piecesObj is string pieces)
        {
            byte[] piecesBytes = Encoding.UTF8.GetBytes(pieces); // This assumes pieces are UTF-8 encoded

            int pieceLength = 20; // Standard piece length for Bittorrent
            int totalLength = piecesBytes.Length;
            int numFullPieces = totalLength / pieceLength;
            int remainingBytes = totalLength % pieceLength;

            // Process all full 20-byte pieces
            for (int i = 0; i < numFullPieces * pieceLength; i += pieceLength)
            {
                var pieceHashBytes = new byte[pieceLength];
                Array.Copy(piecesBytes, i, pieceHashBytes, 0, pieceLength);
                string pieceHashHex = BitConverter.ToString(pieceHashBytes).Replace("-", "").ToLower();
                pieceList.Add(pieceHashHex);
            }

            // Process remaining bytes, if any
            if (remainingBytes > 0)
            {
                var remainingBytesArray = new byte[remainingBytes];
                Array.Copy(piecesBytes, numFullPieces * pieceLength, remainingBytesArray, 0, remainingBytes);
                string remainingHashHex = BitConverter.ToString(SHA1.Create().ComputeHash(remainingBytesArray)).Replace("-", "").ToLower();
                pieceList.Add(remainingHashHex);
            }
        }
        else
        {
            throw new InvalidOperationException("The 'pieces' key is missing or is not a string.");
        }

        return pieceList;
    }
}

using System.Security.Cryptography;
using System.Text;
public class Hash
{
    public static string encryptHash(string input)
    {
        return Convert.ToHexString(SHA1.HashData(Encoding.UTF8.GetBytes(input)));
    }
    public static List<string> listHashes(Dictionary<string, object> info)
    {
        List<string> pieceList = new List<string>();
        string pieces = (string)info["pieces"];
        for (int i = 0; i < pieces.Length; i += 20)
        {
            // Extract 20-byte piece hash
            var pieceHashBytes = new byte[20];
            Array.Copy(Encoding.UTF8.GetBytes(pieces), i, pieceHashBytes, 0, 20);

            // Convert hash to hex
            string pieceHashHex = BitConverter.ToString(pieceHashBytes).Replace("-", "").ToLower();
            pieceList.Add(pieceHashHex);
        }
        return pieceList;
    }
}


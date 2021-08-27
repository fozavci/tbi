using System;
using System.Text;
using System.IO;
public class Program
{

    public static void Main(string[] args)
    {
        if (args.Length < 3) {
            Console.WriteLine("Usage: xorenc inputfilename outputfilename encryptionkey");
            return;
        }
        
        string ifilename = args[0];
        string ofilename = args[1];
        string key = args[2];
        byte[] data = File.ReadAllBytes(ifilename);
        byte[] dataxor = XOREnc(data, key);
        File.WriteAllBytes(ofilename,dataxor);

    }
    public static byte[] XOREnc(byte[] data, string key)
    {
        byte[] xor = new byte[data.Length];

        for (int i = 0; i < data.Length; ++i)
        {
            xor[i] = (byte)(data[i] ^ key[i % key.Length]);
        }

        return xor;
    } 
}
using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using System.Security.Cryptography;

namespace Application 
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            string ifile;
            string ofile;
            string xorkey;
            byte[] aeskey;
            byte[] aesiv;
            byte[] ifilecontent;
            byte[] ofilecontent;

            if (args.Length == 0) { Help(); return; }

            switch (args[0])
            {
                case "base64encode-text":
                    Console.WriteLine(Convert.ToBase64String(Encoding.UTF8.GetBytes(String.Join(" ",args.Skip(1).ToArray()))));
                    break;
                case "base64encode-file":
                    ifile = args[1];
                    ofile = args[2];
                    ifilecontent = File.ReadAllBytes(ifile);                            
                    ofilecontent = Encoding.UTF8.GetBytes(Convert.ToBase64String(ifilecontent));
                    File.WriteAllBytes(ofile,ofilecontent);                            
                    break;
                case "base64decode-text":
                    Console.WriteLine(Encoding.UTF8.GetString(Convert.FromBase64String(String.Join(" ",args.Skip(1).ToArray()))));
                    break;
                case "base64decode-file":       
                    ifile = args[1];
                    ofile = args[2];
                    ifilecontent = File.ReadAllBytes(ifile);                            
                    ofilecontent = Convert.FromBase64String(Encoding.UTF8.GetString(ifilecontent));
                    File.WriteAllBytes(ofile,ofilecontent);                            
                    break;
                case "xor":
                    xorkey = args[1];
                    ifile = args[2];
                    ofile = args[3];
                    ifilecontent = File.ReadAllBytes(ifile);
                    ofilecontent = XOREnc(ifilecontent, xorkey);
                    File.WriteAllBytes(ofile,ofilecontent);
                    break;
                case "aes-encrypt-text":                                
                    aeskey = Encoding.UTF8.GetBytes(args[1]);
                    aesiv = Encoding.UTF8.GetBytes(args[2]);                    
                    byte[] aetext = AESEncrypt(Convert.FromBase64String(String.Join(" ",args.Skip(3).ToArray())),aeskey,aesiv);
                    Console.WriteLine(Encoding.UTF8.GetString(aetext));
                    break;
                case "aes-decrypt-text":                                
                    aeskey = Encoding.UTF8.GetBytes(args[1]);
                    aesiv = Encoding.UTF8.GetBytes(args[2]);                    
                    byte[] adtext = AESDecrypt(Convert.FromBase64String(args[3]),aeskey,aesiv);
                    Console.WriteLine(Encoding.UTF8.GetString(adtext));
                    break;                    
                default:
                    Help();
                    break;
            }
        }

        public static void Help()
        {
            Console.WriteLine(@"Usage examples:
* swissy base64encode-text CONTENTTOENCODE
* swissy base64encode-file inputfile outputfile
* swissy base64decode-text CONTENTTODECODE
* swissy base64decode-file inputfile outputfile
* swissy xor xorkey inputfile outputfile
* swissy aes-encrypt-text AESKey AESIV CONTENTTOENCRYPT
* swissy aes-decrypt-text AESKey AESIV CONTENTTODECRYPT
            ");
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

        public static byte[] AESEncrypt(byte[] inputbuffer, byte[] key, byte[] iv)
        {
            SymmetricAlgorithm algorithm = Aes.Create();
            ICryptoTransform transform = algorithm.CreateEncryptor(key, iv);
            byte[] outputBuffer = transform.TransformFinalBlock(inputbuffer, 0, inputbuffer.Length);
            return outputBuffer;

        }

        public static byte[] AESDecrypt(byte[] inputbuffer, byte[] key, byte[] iv)
        {
            SymmetricAlgorithm algorithm = Aes.Create();
            ICryptoTransform transform = algorithm.CreateDecryptor(key, iv);
            byte[] outputBuffer = transform.TransformFinalBlock(inputbuffer, 0, inputbuffer.Length);
            return outputBuffer;
        }
        
    }
}


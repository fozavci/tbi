using System;
using System.Text;
using System.IO;
using System.Linq;

namespace Application 
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            string imgfile;
            string xorkey;

            // check the number of parameters and instruction
            switch (args.Length)
            {
                case 5:
                    // load the parameters
                    imgfile = args[1];
                    string file = args[2];
                    xorkey = args[3];
                    string outputfile = args[4];
                    if (xorkey.Length < 3) {
                        Console.WriteLine("XOR key length must be 3 or bigger");
                        return;
                    }
                    Console.Write("{0}: Generating...",outputfile);
                    InjectDataToImage(imgfile, file, xorkey, outputfile);
                    break;
                case 3:
                    // load the parameters
                    imgfile = args[1];
                    xorkey = args[2];
                    if (xorkey.Length < 3) {
                        Console.WriteLine("XOR key length must be 3 or bigger");
                        return;
                    }
                    Console.WriteLine("{0}: Testing...",imgfile);
                    byte[] filecontent = LoadDataFromImage(imgfile, xorkey);
                    string tcontent = Encoding.UTF8.GetString(filecontent);
                    Console.WriteLine("Config:\n{0}",tcontent);
                    break;                
                default:
                    Console.WriteLine("Usage example:\nimagematic generateimage templateimage.png file xorkey outputfile\nimagematic testimage.png xorkey");
                    break;
            }
        }

        public static byte[] LoadDataFromImage(string imgfile, string xorkey)
        {
            byte[] imgcontent = File.ReadAllBytes(imgfile);
            
            byte[] separator = Encoding.UTF8.GetBytes("sep"+xorkey);
            int offset = separator.Length;
            int len = imgcontent.Length - offset;

            // Finding technique
            // 0-------------------KEY89-----99
            // When len hits 89, creates a new array of 10, and moves 90-99
            while (len > 0) {
                if (
                    // if you seek XOR key as separator
                    // you'll find it in the end on binary PE files due to \x00 bytes
                    // XORing \x00 and key gives the key
                    imgcontent[len] == separator[0] &&   //s
                    imgcontent[len+1] == separator[1] && //e
                    imgcontent[len+2] == separator[2] && //p
                    imgcontent[len+3] == separator[3] && //XOR key starts
                    imgcontent[len+4] == separator[4] &&
                    imgcontent[len+5] == separator[5] &&
                    imgcontent[len+6] == separator[6] &&
                    imgcontent[len+7] == separator[7] 
                ) {
                    //Console.WriteLine("Data found at {0} of {1}", len, imgcontent.Length);
                    byte[] enccontent = new byte[imgcontent.Length-len-separator.Length];
                    //Console.WriteLine("Creating a new array for {0}", enccontent.Length);
                    Array.Copy(imgcontent, len+separator.Length , enccontent, 0, enccontent.Length);
                    byte[] content = XOR(enccontent,xorkey);
                    return content;
                }
                else {
                    len -= 1;
                    
                }
            }
            return null;
        }

        public static void InjectDataToImage(string imgfile, string datafile, string xorkey, string outputfile)
        {
            // read the data and conver them to byte arrays
            byte[] imgcontent = File.ReadAllBytes(imgfile);            
            byte[] content = File.ReadAllBytes(datafile);            
            byte[] enccontent = XOR(content,xorkey);
            // if separator is only the key, XORing \x00 bytes gives the key
            // that's  problem for the last bytes of the binary PE files 
            byte[] separator = Encoding.UTF8.GetBytes("sep"+xorkey);
            // create the new byte array and offset            
            byte[] outputcontent = new byte[imgcontent.Length + separator.Length + enccontent.Length];
            int offset = 0;

            // combine the byte arrays
            Buffer.BlockCopy(imgcontent, 0, outputcontent, offset, imgcontent.Length);
            offset += imgcontent.Length;
            Buffer.BlockCopy(separator, 0, outputcontent, offset, separator.Length);
            offset += separator.Length;
            Buffer.BlockCopy(enccontent, 0, outputcontent, offset, enccontent.Length);

            File.WriteAllBytes(outputfile, outputcontent);
            Console.Write("generated...");
            bool result = CompareDataFromImage(datafile, outputfile, xorkey);
            if (result) {
                Console.WriteLine("the data is verified.");
            }
            else {
                Console.WriteLine("the data verification failed.");
            }
        }

        public static bool CompareDataFromImage(string orgfile, string imgfile, string xorkey)
        {
            byte[] orgcontent = File.ReadAllBytes(orgfile);
            byte[] imgcontent = File.ReadAllBytes(imgfile);
            
            byte[] separator = Encoding.UTF8.GetBytes("sep"+xorkey);
            // Console.WriteLine("XOR key bytes: {0}", BitConverter.ToString(separator));
            int offset = separator.Length;
            int len = imgcontent.Length - offset;
            // Console.WriteLine("Last 100 bytes of the original data: {0}",BitConverter.ToString(orgcontent.Skip(orgcontent.Length-100).Take(100).ToArray()));
            // Console.WriteLine("Last 100 bytes of the image: {0}",BitConverter.ToString(imgcontent.Skip(imgcontent.Length-100).Take(100).ToArray()));

            // Finding technique
            // 0-------------------KEY89-----99
            // When len hits 89, creates a new array of 10, and moves 90-99            
            while (len > 0) {
                if (
                    // if you seek XOR key as separator
                    // you'll find it in the end on binary PE files due to \x00 bytes
                    // XORing \x00 and key gives the key
                    imgcontent[len] == separator[0] &&   //s
                    imgcontent[len+1] == separator[1] && //e
                    imgcontent[len+2] == separator[2] && //p
                    imgcontent[len+3] == separator[3] && //XOR key starts
                    imgcontent[len+4] == separator[4] &&
                    imgcontent[len+5] == separator[5] &&
                    imgcontent[len+6] == separator[6] &&
                    imgcontent[len+7] == separator[7] 
                ) {
                    // Console.WriteLine("Configuration data found at {0} of {1}", len, imgcontent.Length);
                    byte[] encextractcontent = new byte[imgcontent.Length-len-separator.Length];
                    // Console.WriteLine("Size of original content is {0}", orgcontent.Length);
                    // Console.WriteLine("Creating a new array for {0}", encextractcontent.Length);
                    Array.Copy(imgcontent, len+separator.Length , encextractcontent, 0, encextractcontent.Length);
                    byte[] extractcontent = XOR(encextractcontent,xorkey);
                    for (int i = 0; i < extractcontent.Length; i++)
                    {
                        if (extractcontent[i] != orgcontent[i]) { 
                            Console.WriteLine("Original data and injected data do not match.", i);                                                  
                            return false;
                        }
                    }
                    return true;
                }
                else {
                    len -= 1;
                }
            }
            return false;
        }

        public static byte[] XOR(byte[] data, string xorkey)
        {
            byte[] xor = new byte[data.Length];

            for (int i = 0; i < data.Length; ++i)
            {
                xor[i] = (byte)(data[i] ^ xorkey[i % xorkey.Length]);
            }

            return xor;
        }
    }
}


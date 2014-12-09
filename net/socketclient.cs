using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace Neuro.Net {
public class SocketClient : BaseSocket {
    public event OnDataHandler OnData;
    public event OnCloseConnectionHandler OnCloseConnection;
    public event OnMessageHandler OnMessage;

    private string host;
    private string port;
    private string protocol;

    public Socket clientSocket;

    public SocketClient() {
        host		= "127.0.0.1";
        port		= "8888";
        protocol	= "tcp";
    }

    public string Host {
        get {
            return host;
        }
        set {
            host = value;
        }
    }

    public string Port {
        get {
            return port;
        }
        set {
            port = value;
        }
    }

    public string Protocol {
        get {
            return protocol;
        }
        set {
            protocol = value;
        }
    }

    public void Connect() {
        try {
            if (protocol.ToLower() == "tcp") {
                clientSocket = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            } else {
                clientSocket = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Udp);
            }

            IPAddress ip		= IPAddress.Parse(host);
            int ipPortNo		= System.Convert.ToInt32(port);
            IPEndPoint ipEnd	= new IPEndPoint(ip, ipPortNo);
            clientSocket.Connect(ipEnd);

            if (clientSocket.Connected) {
                WaitForData();
            }
        } catch (SocketException se) {
            Console.WriteLine(se.Message);
        }
    }

    public void SendText(string message) {
        try {
            // New code to send strings
            NetworkStream networkStream = new NetworkStream(clientSocket);
            StreamWriter streamWriter = new StreamWriter(networkStream);
            streamWriter.WriteLine(message);
            streamWriter.Flush();
        } catch(SocketException se) {
            // Handle se.Message;
        }
    }

    private void WaitForData() {
        try {
            if  (asyncCallBack == null )  {
                asyncCallBack = new AsyncCallback (OnDataReceived);
            }
            // SocketPacket socketPacket	= new SocketPacket ();
            SocketPacket socketPacket = (SocketPacket)asyncCallBack.AsyncState;
            socketPacket.thisSocket		= clientSocket;

            // Start listening to the data asynchronously
            asyncResult = clientSocket.BeginReceive (
                              socketPacket.dataBuffer,
                              0,
                              socketPacket.dataBuffer.Length,
                              SocketFlags.None,
                              asyncCallBack,
                              socketPacket
                          );
        } catch(SocketException se) {
            Console.WriteLine(se.Message);
        }
    }

    public void OnDataReceived(IAsyncResult asyn) {
        SocketPacket socketData = (SocketPacket)asyn.AsyncState;

        try {
            int rx  = socketData.currentSocket.EndReceive(asyn);
            char[] chars = new char[rx +  1];

            System.Text.Decoder d = System.Text.Encoding.UTF8.GetDecoder();
            int charLen = d.GetChars(
                              socketData.dataBuffer,
                              0,
                              rx,
                              chars,
                              0
                          );

            string data = new String(chars);

            if (OnData != null) {
                OnData(this, new DataEventArgs(data, socketData));
            }

            WaitForData();
        } catch (ObjectDisposedException ) {
            // System.Diagnostics.Debugger.Log(0,"1","\nOnDataReceived: Socket has been closed\n");
        } catch(SocketException se) {
            // MessageBox.Show (se.Message );
        }
    }

    public void Close() {
        clientSocket.Close();
        clientSocket = null;
    }
}
}

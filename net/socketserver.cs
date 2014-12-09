using System;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.Threading;

namespace Neuro.Net {
public class SocketServer : BaseSocket {
    public AsyncCallback workerCallBack;
    public AsyncCallback connectionWriter;

    // Do we really need two?
    public event OnConnectionHandler OnConnection;
    public event OnCloseConnectionHandler OnCloseConnection;

    public event OnMessageHandler OnMessage;

    public event OnDataHandler OnData;

    private System.Net.Sockets.Socket mainSocket;

    private ArrayList workerSocketList = ArrayList.Synchronized(new System.Collections.ArrayList());

    // Modifying this variable should be done in a thread safe manner
    private int clientCount = -1;

    private bool isListening 	= false;
    private string hostname		= "127.0.0.1";
    private string port			  = "8888";

    public SocketServer() {
    }

    public string Hostname {
        get {
            return hostname;
        }
        set {
            hostname = value;
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

    public bool Listening {
        get {
            return isListening;
        }
    }

    public void Start() {
        try {
            mainSocket = new System.Net.Sockets.Socket(
                AddressFamily.InterNetwork,
                SocketType.Stream,
                ProtocolType.Tcp
            );

            int lPort = System.Convert.ToInt32(port);

            IPEndPoint ipLocal = new IPEndPoint (IPAddress.Parse(hostname), lPort);
            mainSocket.Bind( ipLocal );
            mainSocket.Listen(4);
            mainSocket.BeginAccept(new AsyncCallback (OnClientConnect), null);
            isListening = true;
            // TimerCallback timerdelegate = new TimerCallback(this.CheckSockets);
        } catch(SocketException se) {
            Console.WriteLine(se.Message);
        }
    }

    public void OnClientConnect(IAsyncResult asyn) {
        try {
            System.Net.Sockets.Socket workerSocket = mainSocket.EndAccept(asyn);
            Interlocked.Increment(ref clientCount);
            workerSocketList.Add(workerSocket);

            // SendMsgToClient(msg, clientCount - 1);

            UpdateClientList();
            WaitForData(workerSocket, clientCount);
            mainSocket.BeginAccept(new AsyncCallback(OnClientConnect), null);
        } catch(ObjectDisposedException) {
            Console.WriteLine("OnClientConnection: Socket has been closed\n");
        } catch(SocketException se) {
            Console.WriteLine(se.Message);
        }
    }

    public void WaitForData(System.Net.Sockets.Socket soc, int clientNumber) {
        try {
            if (workerCallBack == null ) {
                workerCallBack = new AsyncCallback(OnDataReceived);
            }
            SocketPacket theSocPkt = new SocketPacket (soc, clientNumber);

            soc.BeginReceive(
                theSocPkt.dataBuffer,
                0,
                theSocPkt.dataBuffer.Length,
                SocketFlags.None,
                workerCallBack,
                theSocPkt
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

            WaitForData(socketData.currentSocket, socketData.clientNumber );
        } catch (ObjectDisposedException ) {
            Console.WriteLine("OnDataReceived: Socket has been closed\n");
        } catch (SocketException se) {
            if (
                (se.ErrorCode >= 10050) ||
                (se.ErrorCode <= 10054)
            ) {
                string msg = "Client " + socketData.clientNumber + " Disconnected" + "\n";
                Console.WriteLine(msg);
                RemoveConnection(socketData.clientNumber);
            } else {
                Console.WriteLine(se.Message + " errcode: " + se.ErrorCode.ToString());
            }
        }
    }

    private void RemoveConnection(int number) {
        try {
            UpdateClientList();
            workerSocketList[number] = null;
        } catch (Exception e) {
            Console.WriteLine(e.Message);
        }
    }

    public void Stop() {
        CloseSockets();
        isListening = false;
    }

    private void CloseSockets() {
        if (mainSocket != null) {
            mainSocket.Close();
        }
        System.Net.Sockets.Socket workerSocket = null;
        for (int i = 0; i < workerSocketList.Count; i++) {
            workerSocket = (System.Net.Sockets.Socket)workerSocketList[i];
            if (workerSocket != null) {
                workerSocket.Close();
                workerSocket = null;
            }
        }
    }

    public void UpdateClientList() {
        for (int i = 0; i < workerSocketList.Count; i++) {
            System.Net.Sockets.Socket workerSocket = (System.Net.Sockets.Socket)workerSocketList[i];
            if (workerSocket != null) {
                string clientKey = Convert.ToString(i+1);
                string clientIP  = ((IPEndPoint)workerSocket.RemoteEndPoint).Address.ToString();
                if (workerSocket.Connected) {
                    if (OnConnection != null) {
                        OnConnection(this, new ConnectionEventArgs(clientKey, clientIP));
                    }
                } else {
                    workerSocket.Close();
                    workerSocket			= null;
                    workerSocketList[i]		= null;
                    if (OnCloseConnection != null) {
                        OnCloseConnection(this, new ConnectionEventArgs(clientKey, clientIP));
                    }
                }
            }
        }
    }

    public void SendMsgToClient(string msg, int clientNumber) {
        try {
            byte[] byData = System.Text.Encoding.ASCII.GetBytes(msg);

            System.Net.Sockets.Socket workerSocket = (System.Net.Sockets.Socket)workerSocketList[clientNumber];
            if (workerSocket != null) {
                if (workerSocket.Connected) {
                    workerSocket.Send(byData);
                }
            }
        } catch (Exception e) {
            Console.WriteLine(e.Message);
        }
    }

    public void BroadcastMessage(string msg) {
        try {
            for (int i = 0; i < workerSocketList.Count; i++) {
                if (workerSocketList[i] != null) {
                    object[] state = new object[2];
                    state[0] = msg;
                    state[1] = i;

                    // Queue message sending
                    ThreadPool.QueueUserWorkItem(
                    delegate(object o) {
                        object[] parms = (object[]) o;
                        SendMsgToClient((string)parms[0], (int)parms[1]);
                    },
                    state
                    );
                    // SendMsgToClient(msg, i);
                }
            }
            Console.WriteLine("Broadcast to clients: " + msg);
        } catch (Exception e) {
            Console.WriteLine(e.Message);
        }
    }

    private void CheckSockets() {
        try {
            foreach (Connection c in this.workerSocketList) {

            }
        } catch (Exception e) {
            Console.WriteLine(e.Message);
        }
    }
} // Class SocketServer
} // Namespace Neuro.Net.Socket

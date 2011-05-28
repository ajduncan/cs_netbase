using System;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.Threading;
using System.Text; 
using Neuro.Net;

namespace Neuro.Net.NetBase  {
  public class NetBase : SocketServer {
    public delegate void HandleNewDataDelegate(string data, SocketPacket socketData);
    public delegate void HandleNewConnectionDelegate(string data, SocketPacket socketData);

    public event OnMessageHandler OnMessage;

    private ConnectionList connectionList = new ConnectionList();
        
    public NetBase() {
      Port          = "8888";
      Timer timer   = new Timer(OnTimer, new ServerState(), 5000, 30000);
      OnData        += new OnDataHandler(HandleNewData);
      OnConnection  += new OnConnectionHandler(HandleNewConnection);
    }

    public void OnTimer(object data) {
      try {
        BroadcastMessage("Timer Event\n");
      } catch (Exception e) {
        Console.WriteLine(e.Message);
      }
    }

    private void ParseNewData(string data, SocketPacket socketData) {
      try {
        // string msg = "Echoing(" + socketData.clientNumber + "): " + data + "\n";
        // Console.WriteLine(msg);

        if (OnMessage != null) {
          string hostname = ((IPEndPoint)socketData.currentSocket.RemoteEndPoint).Address.ToString();
          OnMessage(this, new MessageEventArgs(socketData.clientNumber.ToString(), hostname, data));
        }
                
        string replyMsg = "";
                
        // Console.WriteLine("Content Length: " + data.Length);
        if (data.Length >= 9) {
          if (data.Substring(0, 9).ToLower() == "username:") {
            string [] split = data.Split(new char [] {' ', ':'});
            foreach(string s in split) {
              if ((s.Trim() != "") && (s.ToLower() != "username")) {
	        // Fix up the nick.
	        StringBuilder sb = new StringBuilder(s);
	        sb.Replace("\r\n", "");
                sb.Replace("\n", "");

                replyMsg = connectionList[socketData.clientNumber].Nick + " is now known as " + sb.ToString().Trim();
                Connection old     = new Connection(connectionList[socketData.clientNumber].ID, connectionList[socketData.clientNumber].Hostname, connectionList[socketData.clientNumber].Nick);
                Connection changed = new Connection(connectionList[socketData.clientNumber].ID, connectionList[socketData.clientNumber].Hostname, sb.ToString().Trim());
                connectionList.Remove(old);
                connectionList.Add(changed);
              }
            }
          } else {
            replyMsg = "<" + connectionList[socketData.clientNumber].Nick + "> " + data;
          }
        } else {
          replyMsg = "<" + connectionList[socketData.clientNumber].Nick + "> " + data;
        }
        // if there was an error, don't broadcast (Todo)
        BroadcastMessage(replyMsg);
      } catch (Exception e) {
        Console.WriteLine(e.Message);
      }        	
    }  
      
    private void ParseNewConnection(string clientKey, string clientIP) {
      try {
        Connection nc = new Connection(clientKey, clientIP, "none");
        connectionList.Add(nc);
        		
        string msg = "<" + nc.Nick + "> has joined the chat.\n";
        BroadcastMessage(msg);
                
      } catch (Exception e) {
        Console.WriteLine(e.Message);
      }
    }
        
    public void HandleNewData(Object sender, DataEventArgs DE) {
      object[] state = new object[2];
      state[0] = DE.DEData;
      state[1] = DE.DESocketPacket;

      // Queue Parsing data
      ThreadPool.QueueUserWorkItem(
        delegate(object o) {
          object[] parms = (object[]) o;
          ParseNewData((string)parms[0], (SocketPacket)parms[1]);
        },
        state
      );
    }
        
    public void HandleNewConnection(Object sender, ConnectionEventArgs CE) {
      // Need to update our ConnectionList with a new connection for the ip, host and nickname (default of 'none')
      object[] state = new object[2];
      state[0] = CE.ClientKey;
      state[1] = CE.ClientIP;

      // Queue Parsing data
      ThreadPool.QueueUserWorkItem(
        delegate(object o) {
          object[] parms = (object[]) o;
          ParseNewConnection((string)parms[0], (string)parms[1]);
	},
        state
      );        	
    }  
  } // Class SocketServer
	
  public class NetBaseConnection : Connection {
    private string nickname;
    
    public void ChatConnection() {
      nickname = string.Empty;
    }
		
    public string Nickname {
      get { return nickname; }
      set { nickname = value; }
    }
  }
} 

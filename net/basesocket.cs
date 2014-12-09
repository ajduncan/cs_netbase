using System;
using System.Net;
using System.Collections;

namespace Neuro.Net {
public class BaseSocket {
    public class SocketPacket {
        public System.Net.Sockets.Socket currentSocket;
        public int clientNumber;
        public byte[] dataBuffer = new byte[1024];
        public string nick;

        public SocketPacket(System.Net.Sockets.Socket socket, int number) {
            currentSocket = socket;
            clientNumber  = number;
            nick		  = "Unknown" + clientNumber;
        }
    }

    public BaseSocket() {
    }

    public string GetIP() {
        string hostName 		= Dns.GetHostName();
        IPHostEntry iphostentry = Dns.GetHostEntry(hostName);

        string IP = "";
        foreach(IPAddress ipaddress in iphostentry.AddressList) {
            IP = ipaddress.ToString();
            return IP;
        }
        return IP;
    }
}

public class ServerState {
    private DateTime uptime = new DateTime();

    public ServerState() {
    }
}

public class Connection : IComparable {
    private string connectionID;
    private string hostname;
    private string nick;
    private DateTime ts;

    public Connection() {
        connectionID 	= string.Empty;
        hostname		= string.Empty;
        nick            = string.Empty;
        ts 				= new DateTime();
    }

    public Connection(string connid, string connhostname, string nickname) {
        connectionID	= connid;
        hostname		= connhostname;
        nick            = nickname;
    }

    public string ID {
        get {
            return connectionID;
        }
        set {
            connectionID = value;
        }
    }

    public string Hostname {
        get {
            return hostname;
        }
        set {
            hostname = value;
        }
    }

    public string Nick {
        get {
            return nick;
        }
        set {
            nick = value;
        }
    }

    public DateTime TS {
        get {
            return ts;
        }
        set {
            ts = value;
        }
    }

    public int CompareTo(object obj) {
        if (!(obj is Connection)) {
            throw new ArgumentException("Wrong Type");
        }
        Connection connection = (Connection)obj;

        // Only match on unique connection ID;
        int cmp1 = this.ID.CompareTo(connection.ID);
        return(cmp1);
    }
}

public class ConnectionList : CollectionBase {
    public Connection this[int i] {
        get {
            return (Connection)this.InnerList[i];
        }
    }

    public ConnectionList() {
    }

    public void Add(Connection connection) {
        this.InnerList.Add(connection);
    }

    public void Remove(Connection connection) {
        if (!this.Contains(connection)) {
            return;
        }

        for (int i = 0; i <= this.InnerList.Count; i++) {
            Connection x = (Connection)this.InnerList[i];
            if ((x.CompareTo(connection) == 0)) {
                this.RemoveAt(i);
                return;
            }
        }
    }

    public bool Contains(Connection connection) {
        //loop through all the items in the list
        foreach (Connection x in this.InnerList) {
            if (x.CompareTo(connection) == 0) {
                return true;
            }
        }
        return false;
    }

    protected override void OnValidate(object obj) {
        base.OnValidate(obj);
        if (!(obj is Connection)) {
            throw new ArgumentException("Wrong Type");
        }
    }

    public int IndexOf(Connection connection) {
        return(this.InnerList.IndexOf(connection));
    }

    public Connection[] ToArray() {
        return (Connection[])this.InnerList.ToArray(typeof(Connection));
    }
}
}

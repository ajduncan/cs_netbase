using System;

namespace Neuro.Net {
	public delegate void OnConnectionHandler(Object sender, ConnectionEventArgs e);
	public delegate void OnCloseConnectionHandler(Object sender, ConnectionEventArgs e);

  public class ConnectionEventArgs : EventArgs {   	
		private string clientKey;
		private string clientIP;
		private string clientNick;

		public ConnectionEventArgs(string key, string ip) {
			clientKey		= key;
			clientIP		= ip;
			clientNick      = string.Empty;
		}
		
		public ConnectionEventArgs(string key, string ip, string nick) {
			clientKey		= key;
			clientIP		= ip;
			clientNick      = nick;
		}
		
		public string ClientKey {
			get { return clientKey; }
		}
		
		public string ClientIP {
			get { return clientIP; }
		}
		
		public string ClientNick {
			get { return clientNick; }
		}
  }
}
	

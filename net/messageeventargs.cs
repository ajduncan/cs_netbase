using System;

namespace Neuro.Net {
	public delegate void OnMessageHandler(Object sender, MessageEventArgs e);

  public class MessageEventArgs : EventArgs {
		private string clientKey;
		private string clientIP;
		private string clientMessage;

		public MessageEventArgs(string key, string ip, string message) {
			clientKey		= key;
			clientIP		= ip;
			clientMessage   = message;
		}
    
		public string ClientKey {
      get { return clientKey; }
    }

    public string ClientIP {
      get { return clientIP; }
    }

    public string ClientMessage {
      get { return clientMessage; }
    }
  }
}
	

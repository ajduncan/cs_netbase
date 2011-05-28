using System;
using Neuro.Net;

namespace Neuro.Net {
	public delegate void OnDataHandler(Object sender, DataEventArgs e);

	public class DataEventArgs : EventArgs {   	
		private string deData;
		private BaseSocket.SocketPacket deSocketPacket;
	
		public DataEventArgs(string data, BaseSocket.SocketPacket socketPacket) {
			deData				= data;
			deSocketPacket		= socketPacket;
		}
		
		public string DEData {
			get { return deData; }
		}
		
		public BaseSocket.SocketPacket DESocketPacket {
			get { return deSocketPacket; }
		}
	}
}
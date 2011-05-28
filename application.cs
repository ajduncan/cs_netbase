using System;
using Neuro.Net.NetBase;

namespace Neuro {
  public class Application {
    [STAThread]
    public static void Main(string[] args) {
      NetBase nb = new NetBase();
      nb.Start();

      Console.WriteLine("Server running.\nPress enter to exit.\n");
      Console.ReadLine();

      nb.Stop();
    }
  }
} 

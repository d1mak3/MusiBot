using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MusiBot
{
    public class Program
    {
        public static Task Main(string[] args) => Startup.ExecuteAsync();
    }
}

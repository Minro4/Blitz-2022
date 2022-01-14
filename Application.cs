using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using Newtonsoft.Json;
using Blitz2022;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;

public static class Application
{
    public static void Main(string[] args)
    {
        var bot = new Bot();
        var client = new WebSocketClient(bot);
        Task task = client.run();
        task.Wait(); 
    }
}
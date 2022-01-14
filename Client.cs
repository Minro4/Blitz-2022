using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Converters;

namespace Blitz2022
{
    public class WebSocketClient
    {
        private Bot bot;
        private JsonSerializerSettings jsonSerializerSettings;

        public WebSocketClient(Bot bot)
        {
            this.bot = bot;

            DefaultContractResolver contractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy(),
            };
            jsonSerializerSettings = new JsonSerializerSettings
            {
                ContractResolver = contractResolver,
            };
            jsonSerializerSettings.Converters.Add(new StringEnumConverter());
        }

        public async Task run(string address = "127.0.0.1:8765")
        {
            using (ClientWebSocket webSocket = new ClientWebSocket())
            {
                Uri serverUri = new Uri("ws://" + address);

                await webSocket.ConnectAsync(serverUri, CancellationToken.None);

                string token = Environment.GetEnvironmentVariable("TOKEN");
                string registerPayload = token == null
                    ? registerPayload = JsonConvert.SerializeObject(new { type = "REGISTER", teamName = Bot.NAME })
                    : registerPayload = JsonConvert.SerializeObject(new { type = "REGISTER", token = token });


                await webSocket.SendAsync(
                    new ArraySegment<byte>(Encoding.UTF8.GetBytes(registerPayload)),
                    WebSocketMessageType.Text, true, CancellationToken.None);

                while (webSocket.State == WebSocketState.Open)
                {
                    GameMessage message = JsonConvert.DeserializeObject<GameMessage>(await readMessage(webSocket), jsonSerializerSettings);

                    if (message != null)
                    {
                        Console.WriteLine("Playing tick " + message.tick + " of " + message.totalTick);
                        List<string> errors = message.getTeamsMapById[message.teamId].errors;
                        errors.ForEach(error => Console.WriteLine("Your bot command error: " + error));

                        GameCommand command = bot.nextMove(message);
                        string serializedCommand = JsonConvert.SerializeObject(new { type = "COMMAND", actions = command.actions, tick = message.tick }, jsonSerializerSettings);

                        await webSocket.SendAsync(
                            Encoding.UTF8.GetBytes(serializedCommand),
                            WebSocketMessageType.Text,
                            true, CancellationToken.None);
                    }

                }
            }
        }

        public async static Task<string> readMessage(ClientWebSocket client)
        {
            string result = "";

            WebSocketReceiveResult receiveResult;
            do
            {
                ArraySegment<byte> segment = new ArraySegment<byte>(new byte[1024]);
                receiveResult = await client.ReceiveAsync(segment, CancellationToken.None);
                result += Encoding.UTF8.GetString(segment.Array);
            } while (!receiveResult.EndOfMessage);


            return result;
        }
    }
}
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBotTemplateNet7.Utility
{
    public class WebServerHandler
    {
        public WebServerHandler()
        {
            ConsoleColors.WriteLineWithColors("[ ^4WebHandler ^0] [ ^2Connected ^0] WebServer Successfully connected!");
            DiscordEmbedBuilder embed = new DiscordEmbedBuilder
            {
                Title = "Web Server Handler",
                Description = "WebServer Connected and initialized!",
                Color = DiscordColor.Green
            };
            string[] cahnnels =
            {
                "global",
                "webhandler"
            };
            Program._logger.SendEmbed(embed, cahnnels, null);
        }



        public async void ProcessRequest(HttpListenerContext context)
        {
            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;

            //Console.WriteLine($"{request.HttpMethod} {request.Url.PathAndQuery} from {request.RemoteEndPoint}");

            string[] segments = request.Url.AbsolutePath.Split('/');
            string responseString = "";
            List<string> channels = new List<string> { "webhandler", "global" };

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder
            {
                Title = "Web Server Handler",
                Description = "Processing HTTP Request",
                Color = DiscordColor.Green
            };

            embed.AddField("Request Method", request.HttpMethod);
            embed.AddField("Request URL", request.Url.ToString());
            embed.AddField("PathQuery", request.Url.PathAndQuery);
            embed.AddField("Remote Endpoint", request.RemoteEndPoint.ToString());


            string agent = request.UserAgent;


            if (Program.userAgenLock)
            {
                if (agent != "$MateHUN/Handler")
                {
                    embed.Color = DiscordColor.IndianRed;
                    embed.Description = $"Processing HTTP Request - ERROR\nInvalid UserAgent";
                }
            }

            if (agent == "$MateHUN/Handler")
                switch (segments[1].ToLower())
                {
                    case "savedata":
                        embed.AddField("Action", "Save Data");
                        break;
                    default:
                        embed.AddField("Action", "Unknown");
                        break;
                }

            responseString = "Hello, world!";


            string[] channelsArray = channels.ToArray();
            await Program._logger.SendEmbedAsync(embed, channelsArray);



            response.ContentType = "text/plain";
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
            response.ContentLength64 = buffer.Length;
            response.OutputStream.Write(buffer, 0, buffer.Length);
            response.OutputStream.Close();

        }
    }
}

using DiscordBotTemplateNet7.EventSystem;
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
            string requestBody = String.Empty;
            if (request.HasEntityBody)
            {
                using (Stream body = request.InputStream)
                {
                    using (StreamReader reader = new StreamReader(body))
                    {
                        requestBody = await reader.ReadToEndAsync();
                    }
                }
            }


            string[] segments = request.Url.AbsolutePath.Split('/');

            if (segments[1].ToLower().Contains("favicon"))
            {
                return;
            }

            string responseString = "";
            List<string> channels = new List<string> { "webhandler", "global" };

            string[] channelsArray;
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
                    channelsArray = channels.ToArray();
                    await Program._logger.SendEmbedAsync(embed, channelsArray);
                    response.OutputStream.Close();
                    return;
                }
            }

            EventPublisher publisher = Program.publisher;
            responseString = "NAN";
            try
            {
               
                switch (segments[1].ToLower())
                {
                    case "getpfp":
                        embed.AddField("Action", "Getting profile picture");
                        responseString = "OnGetPFP";
                        publisher.RaiseEvent(segments[1].ToLower(), requestBody);
                        break;
                    case "savedata":
                        embed.AddField("Action", "Save Data");
                        responseString = "OnSaveData";
                        publisher.RaiseEvent(segments[1].ToLower(), requestBody);
                        break;
                    default:
                        embed.AddField("Action", "Unknown");
                        responseString = "UNKNOW ERROR";
                        break;

                }
            } catch (Exception ex)
            {
                await Program._logger.LogErrorAsync(ex);
            } finally
            {
                channelsArray = channels.ToArray();
                await Program._logger.SendEmbedAsync(embed, channelsArray);


                response.ContentType = "text/plain";
                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
                response.ContentLength64 = buffer.Length;
                response.OutputStream.Write(buffer, 0, buffer.Length);
                response.OutputStream.Close();

            }
        }
    }
}

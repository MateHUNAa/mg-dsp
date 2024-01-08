// BotConfig.cs
using DiscordBotTemplateNet7.Config;
using Newtonsoft.Json;
using System.IO;
using System.Threading.Tasks;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace DiscordBotTemplateNet7.Config
{
    internal class BotConfig
    {
        public string Token { get; set; }
        public string Prefix { get; set; }

        public async Task ReadJSONAsync()
        {
            using (StreamReader sr = new StreamReader($"{AppDomain.CurrentDomain.BaseDirectory}/config.json"))
            {
                string json = await sr.ReadToEndAsync();
                JSONStruct obj = JsonConvert.DeserializeObject<JSONStruct>(json);

                this.Token = obj.token;
                this.Prefix = obj.prefix;
            }
        }
    }

    internal sealed class JSONStruct
    {
        public string token { get; set; }
        public string prefix { get; set; }
    }
}


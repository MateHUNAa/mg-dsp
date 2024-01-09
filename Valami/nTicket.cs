using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBotTemplateNet7.Valami
{


    public class nTicket
    {
        public enum Types
        {
            Support,
            BUG,
            Fractions,
            ADMIN_TGF
        }



        public ulong GuildID { get; set; }
        public ulong ChannelID { get; set; }
        public ulong DiscordUserID { get; set; }
        public Types Type { get; set; }

        public Types GetTypeFromCustomId(string customId)
        {
            switch (customId)
            {
                case "tamogatas":
                    return Types.Support;
                case "bug":
                    return Types.BUG;
                case "frakciok":
                    return Types.Fractions;
                case "admin_tgf":
                    return Types.ADMIN_TGF;
                default:
                    // Handle unknown CustomId, such as throwing an exception or returning a default type
                    return Types.Support;
            }
        }

    }
}

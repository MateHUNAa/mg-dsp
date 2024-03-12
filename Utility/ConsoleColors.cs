using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBotTemplateNet7.Utility
{
    public static class ConsoleColors
    {
        public static void WriteLineWithColors(string message)
        {

            var parts = message.Split('^');
            foreach (var part in parts)
            {
                if (part.Length < 2)
                {
                    Console.Write(part);
                    continue;
                }

                char colorCode = part[0];
                string text = part.Substring(1);

                ConsoleColor color = ConsoleColor.White;
                switch (colorCode)
                {
                    case '0':
                        color = ConsoleColor.White;
                        break;
                    case '4':
                        color = ConsoleColor.Blue;
                        break;
                    case '1':
                        color = ConsoleColor.Red;
                        break;
                    case '2':
                        color = ConsoleColor.Green;
                        break;
                    case '3':
                        color = ConsoleColor.Magenta;
                        break;
                }

                Console.ForegroundColor = color;
                Console.Write(text);
                Console.ResetColor();
            }
            Console.WriteLine();
        }
    }

}

using DiscordBotTemplateNet7.Utility;
using System.Reflection;
using System.Text.RegularExpressions;

namespace DiscordBotTemplateNet7.EventSystem
{
    public class EventPublisher
    {
        private Dictionary<string, EventHandler<EventArgs>> eventDictionary = new Dictionary<string, EventHandler<EventArgs>>();

        public event EventHandler<EventArgs> saveData;
        public event EventHandler<EventArgs> loadData;
        public event EventHandler<EventArgs> manageData;
        public event EventHandler<EventArgs> getpfp;

        public EventPublisher()
        {
            ConsoleColors.WriteLineWithColors("[ ^4EventPublisher ^0] [ ^3Info ^0] Event publisher initialized!");
            eventDictionary.Clear();
            Logger l = Program._logger;

            saveData = (sender, args) =>
            {
                CustomArgs customArgs = args as CustomArgs;

                if (customArgs != null)
                {
                    // Todo
                    string eventName = customArgs.EventName;
                    string eventData = customArgs.EventPayload;
                    cc($"[ ^4EventPublisher ^0] [ ^3Action ^0] [{eventName.ToUpper()}]: Event processing ... \n{eventData}");
                }

            };
            loadData = (sender, args) => { };
            manageData = (sender, args) => { };
            getpfp = (sender, args) =>
            {
                CustomArgs customArgs = args as CustomArgs;

                if (customArgs != null)
                {
                    string eventName = customArgs.EventName;
                    string eventData = customArgs.EventPayload;

                    cc($"[ ^4EventPublisher ^0] [ ^3Action ^0] [{eventName.ToUpper()}]: Event processing ... \n{eventData}");

                    string mentionPattern = "<@(.+?)>|<#(.+?)>";

                    var mentionMatches = Regex.Matches(eventData, mentionPattern, RegexOptions.IgnoreCase);

                    foreach (Match mentionMatch in mentionMatches)
                    {
                        string userID = mentionMatch.Groups[1].Value;

                        cc($"Discord mention found: UserID: {userID}");
                    }

                }
            };



            InitializeEvents();

        }

        private void cc(string msg)
        {
            ConsoleColors.WriteLineWithColors($"{msg}");
        }

        private void InitializeEvents()
        {
            foreach (var fieldInfo in GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (fieldInfo.FieldType == typeof(EventHandler<EventArgs>))
                {
                    var eventName = fieldInfo.Name.ToLower();
                    var eventHandler = (EventHandler<EventArgs>)fieldInfo.GetValue(this);
                    if (eventHandler != null)
                        AddEvent(eventName, eventHandler);
                    else
                        Console.WriteLine($"[ ^4EventPublisher ^0] [ ^1Error ^0] EventHandler Is NULL! {eventName}");
                }
            }
        }

        public void AddEvent(string eventName, EventHandler<EventArgs> eventHandler)
        {
            eventDictionary[eventName] = eventHandler;
        }

        public void RemoveEvent(string eventName)
        {
            eventDictionary.Remove(eventName);
        }

        public void RaiseEvent(string eventName, string eventData)
        {
            if (eventDictionary.TryGetValue(eventName, out var eventHandler))
            {
                CustomArgs cArgs = new CustomArgs(eventName, eventData);
                eventHandler?.Invoke(this, cArgs);
            } else
            {
                Console.WriteLine($"Event '{eventName}' not found");
            }
        }
    }
}

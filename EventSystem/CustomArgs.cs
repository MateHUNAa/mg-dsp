namespace DiscordBotTemplateNet7.EventSystem
{
    public class CustomArgs : EventArgs
    {
        public string EventName { get; }
        public string EventPayload { get; } 

        public CustomArgs(string eventName, string eventPayload)
        {
            EventName = eventName ?? string.Empty;
            EventPayload = eventPayload ?? string.Empty;
        }
    }
}

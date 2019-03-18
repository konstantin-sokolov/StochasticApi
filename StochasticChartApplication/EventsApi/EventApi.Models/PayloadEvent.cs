namespace EventApi.Models
{
    public struct PayloadEvent
    {
        public EventType EventType { get; private set; }
        public Payload Payload { get; set; }
        public long Ticks { get; set; }

        public PayloadEvent(EventType eventType, long ticks)
        {
            EventType = eventType;
            Ticks = ticks;
            Payload = new Payload();
        }
    }
}
namespace EventApi.Models
{
    public class DensityInfo
    {
        public long Start { get; set; }
        public long Stop { get; set; }
        public long EventsCount { get; set; }

        public long StartIndex { get; set; }
        public long StopIndex { get; set; }

        public DensityInfo Clone()
        {
            return new DensityInfo
            {
                Start = Start,
                Stop = Stop,
                EventsCount = EventsCount,
                StartIndex = StartIndex,
                StopIndex = StopIndex,
            };
        }
    }
}

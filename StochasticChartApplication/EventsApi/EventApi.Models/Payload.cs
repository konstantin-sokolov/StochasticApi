namespace EventApi.Models
{
    public struct Payload
    {
        public long first;
        public long second;
        public long third;
        public long fourth;

        public Payload(long first,long second,long third, long fourth)
        {
            this.first = first;
            this.second = second;
            this.third = third;
            this.fourth = fourth;
        }
    }
}

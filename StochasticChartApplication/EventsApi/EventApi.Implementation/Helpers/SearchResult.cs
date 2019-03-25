namespace EventApi.Implementation.Helpers
{
    internal struct SearchResult
    {
        public SearchResult(int compareResult,long foundIndex)
        {
            CompareResult = compareResult;
            FoundIndex = foundIndex;
        }
        public int CompareResult { get; }
        public long FoundIndex { get; }
    }
}

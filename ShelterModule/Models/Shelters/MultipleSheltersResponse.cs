namespace ShelterModule.Models.Shelters
{
    public sealed class MultipleSheltersResponse
    {
        public required IReadOnlyList<ShelterResponse> shelters;
        public int pageNumber;
        public int count;
    }
}

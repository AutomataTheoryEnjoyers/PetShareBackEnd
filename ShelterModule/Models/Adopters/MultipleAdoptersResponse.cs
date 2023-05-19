namespace ShelterModule.Models.Adopters
{
    public sealed class MultipleAdoptersResponse
    {
        public required IReadOnlyList<AdopterResponse> adopters;
        public int pageNumber;
        public int count;
    }
}

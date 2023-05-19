namespace ShelterModule.Models.Pets
{
    public sealed class MultiplePetsResponse
    {
        public required IReadOnlyList<PetResponse> pets;
        public int pageNumber;
        public int count;
    }
}

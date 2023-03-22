namespace ShelterModule.Models
{
    public class User
    {
        public Guid Id { get; init; }
        public string UserName { get; init; } = null!;
        public string PhoneNumber { get; init; } = null!;
        public string Email { get; init; } = null!;
    }
}

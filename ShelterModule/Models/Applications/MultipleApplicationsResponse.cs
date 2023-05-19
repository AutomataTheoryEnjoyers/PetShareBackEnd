
namespace ShelterModule.Models.Applications
{
    public class MultipleApplicationsResponse
    {
        public required IReadOnlyList<ApplicationResponse> applications;
        public int pageNumber;
        public int count;
    }
}

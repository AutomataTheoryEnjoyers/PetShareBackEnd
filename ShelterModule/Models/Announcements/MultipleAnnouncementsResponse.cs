namespace ShelterModule.Models.Announcements
{
    public sealed class MultipleAnnouncementsResponse
    {
        public required IReadOnlyList<AnnouncementResponse> announcements;
        public int pageNumber;
        public int count;
    }
}

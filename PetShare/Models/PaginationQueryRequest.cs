namespace PetShare.Models
{
    public sealed class PaginationQueryRequest
    {
        public int? PageNumber { get; set; }
        public int? PageCount { get; set; }
    }
}

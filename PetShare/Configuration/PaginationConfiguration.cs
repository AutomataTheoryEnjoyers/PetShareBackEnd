using System.ComponentModel.DataAnnotations;

namespace PetShare.Configuration
{
    public sealed class PaginationConfiguration
    {
        public const string SectionName = "Pagination";

        [Required]
        public required int DefaultPageSize { get; init; }
        [Required]
        public required int DefaultPageNumber { get; init; }
    }
}

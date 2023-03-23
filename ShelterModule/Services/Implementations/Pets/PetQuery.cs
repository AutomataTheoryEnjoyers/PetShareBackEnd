using Database;
using Microsoft.EntityFrameworkCore;
using ShelterModule.Models.Pets;
using ShelterModule.Models.Shelters;
using ShelterModule.Services.Interfaces.Pets;

namespace ShelterModule.Services.Implementations.Pets
{
    public class PetQuery : IPetQuery
    {
        private readonly PetShareDbContext _context;

        public PetQuery(PetShareDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<Pet>> GetAllAsync(CancellationToken token = default)
        {
            return (await _context.Pets.ToListAsync(token)).Select(Pet.FromEntity).ToList();
        }

        public async Task<Pet?> GetByIdAsync(Guid id, CancellationToken token = default)
        {
            var entity = await _context.Pets.FirstOrDefaultAsync(e => e.Id == id, token);
            return entity is null ? null : Pet.FromEntity(entity);
        }
    }
}

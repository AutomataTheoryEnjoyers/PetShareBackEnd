﻿using Database;
using Database.Entities;
using Microsoft.EntityFrameworkCore;
using ShelterModule.Models.Pets;
using ShelterModule.Services.Interfaces.Pets;

namespace ShelterModule.Services.Implementations.Pets;

public class PetCommand : IPetCommand
{
    private readonly PetShareDbContext _dbContext;
    private readonly IImageStorage _imageStorage;

    public PetCommand(PetShareDbContext dbContext, IImageStorage imageStorage)
    {
        _dbContext = dbContext;
        _imageStorage = imageStorage;
    }

    public async Task<Pet> AddAsync(Pet pet, CancellationToken token = default)
    {
        _dbContext.Pets.Add(pet.ToEntity());
        await _dbContext.SaveChangesAsync(token);
        return pet;
    }

    public async Task<Pet?> UpdateAsync(Guid id, PetUpdateRequest request, CancellationToken token = default)
    {
        var entity = await _dbContext.Pets.Where(e => e.Status != PetStatus.Deleted).
                                      FirstOrDefaultAsync(e => e.Id == id, token);
        if (entity is null)
            return null;

        entity.Name = request.Name ?? entity.Name;
        entity.Breed = request.Breed ?? entity.Breed;
        entity.Species = request.Species ?? entity.Species;
        entity.Birthday = request.Birthday ?? entity.Birthday;
        entity.Description = request.Description ?? entity.Description;
        entity.Status = request.Status is not null ? Enum.Parse<PetStatus>(request.Status) : entity.Status;

        await _dbContext.SaveChangesAsync(token);

        return Pet.FromEntity(entity);
    }

    public async Task<Pet?> SetPhotoAsync(Guid id, IFormFile photo, CancellationToken token = default)
    {
        var entity = await _dbContext.Pets.Where(e => e.Status != PetStatus.Deleted).
                                      FirstOrDefaultAsync(e => e.Id == id, token);
        if (entity is null)
            return null;

        entity.Photo = await _imageStorage.UploadImageAsync(photo);
        await _dbContext.SaveChangesAsync(token);

        return Pet.FromEntity(entity);
    }
}

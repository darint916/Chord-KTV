using ChordKTV.Data.Api.SongData;
using ChordKTV.Models.SongData;
using Microsoft.EntityFrameworkCore;

namespace ChordKTV.Data.Repo.SongData;

public class GeniusMetaDataRepo : IGeniusMetaDataRepo
{
    private readonly AppDbContext _context;

    public GeniusMetaDataRepo(AppDbContext context)
    {
        _context = context;
    }

    public async Task<GeniusMetaData?> GetGeniusMetaDataAsync(int geniusId)
    {
        return await _context.GeniusMetaData.FindAsync(geniusId);
    }

    public async Task AddGeniusMetaDataAsync(GeniusMetaData metaData)
    {
        await _context.GeniusMetaData.AddAsync(metaData);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateGeniusMetaDataAsync(GeniusMetaData metaData)
    {
        _context.GeniusMetaData.Update(metaData);
        await _context.SaveChangesAsync();
    }
}

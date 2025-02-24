using ChordKTV.Data.Api.SongData;
using ChordKTV.Models.SongData;
using Microsoft.EntityFrameworkCore;

namespace ChordKTV.Data.Repo.SongData;

public class GeniusMetaDataRepo : IGeniusMetaDataRepo
{
    private readonly AppDbContext _context;
    private readonly ILogger<GeniusMetaDataRepo> _logger;

    public GeniusMetaDataRepo(AppDbContext context, ILogger<GeniusMetaDataRepo> logger)
    {
        _context = context;
        _logger = logger;
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

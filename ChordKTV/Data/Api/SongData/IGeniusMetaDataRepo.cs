using ChordKTV.Models.SongData;

namespace ChordKTV.Data.Api.SongData;

public interface IGeniusMetaDataRepo
{
    public Task<GeniusMetaData?> GetGeniusMetaDataAsync(int geniusId);
    public Task AddGeniusMetaDataAsync(GeniusMetaData metaData);
    public Task UpdateGeniusMetaDataAsync(GeniusMetaData metaData);
} 
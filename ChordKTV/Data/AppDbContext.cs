namespace ChordKTV.Data;

using ChordKTV.Models.SongData;
using ChordKTV.Models.UserData;
using ChordKTV.Models.Quiz;
using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public DbSet<GeniusMetaData> GeniusMetaData { get; set; }
    public DbSet<Album> Albums { get; set; }
    public DbSet<Song> Songs { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Quiz> Quizzes { get; set; }
    public AppDbContext(DbContextOptions<AppDbContext> opt) : base(opt)
    {
    }
}


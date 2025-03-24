using ChordKTV.Models.SongData;
using ChordKTV.Models.UserData;
using ChordKTV.Models.Quiz;
using Microsoft.EntityFrameworkCore;

namespace ChordKTV.Data;

public class AppDbContext : DbContext
{
    public DbSet<GeniusMetaData> GeniusMetaData { get; set; }
    public DbSet<Album> Albums { get; set; }
    public DbSet<Song> Songs { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Quiz> Quizzes { get; set; }
    public DbSet<QuizQuestion> QuizQuestions { get; set; }
    public DbSet<QuizOption> QuizOptions { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> opt) : base(opt)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Quiz>()
            .HasMany(q => q.Questions)
            .WithOne(qq => qq.Quiz)
            .HasForeignKey(qq => qq.QuizId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<QuizQuestion>()
            .HasMany(qq => qq.Options)
            .WithOne(qo => qo.Question)
            .HasForeignKey(qo => qo.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<QuizOption>()
            .HasIndex(qo => new { qo.QuestionId, qo.OrderIndex })
            .IsUnique();
    }
}


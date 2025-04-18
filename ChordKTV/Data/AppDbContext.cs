using ChordKTV.Models.SongData;
using ChordKTV.Models.UserData;
using ChordKTV.Models.Quiz;
using ChordKTV.Models.Playlist;
using ChordKTV.Models.Handwriting;
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

    public DbSet<UserPlaylistActivity> UserPlaylistActivities { get; set; }
    public DbSet<UserSongActivity> UserSongActivities { get; set; }
    public DbSet<UserQuizResult> UserQuizzesDone { get; set; }
    public DbSet<UserHandwritingResult> UserHandwritingResults { get; set; }
    public DbSet<LearnedWord> LearnedWords { get; set; }

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

        modelBuilder.Entity<User>()
            .HasMany(u => u.PlaylistActivities)
            .WithOne()
            .HasForeignKey(pa => pa.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<User>()
            .HasMany(u => u.SongActivities)
            .WithOne()
            .HasForeignKey(sa => sa.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<User>()
            .HasMany(u => u.QuizResults)
            .WithOne()
            .HasForeignKey(qr => qr.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<User>()
            .HasMany(u => u.HandwritingResults)
            .WithOne()
            .HasForeignKey(hr => hr.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<User>()
            .HasMany(u => u.LearnedWords)
            .WithOne()
            .HasForeignKey(lw => lw.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<UserSongActivity>()
            .Property(x => x.PlayDates)
            .HasColumnType("timestamp with time zone[]");

        modelBuilder.Entity<UserPlaylistActivity>()
            .Property(x => x.PlayDates)
            .HasColumnType("timestamp with time zone[]");
    }
}


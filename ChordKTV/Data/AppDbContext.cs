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
    
    // Updated DbSets with correct namespaces
    public DbSet<UserPlaylistActivity> UserPlaylistActivities { get; set; }
    public DbSet<UserQuizResult> UserQuizResults { get; set; }
    public DbSet<UserSongPlay> UserSongPlays { get; set; }
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

        // Configure relationships for new entities with updated namespaces
        modelBuilder.Entity<UserPlaylistActivity>()
            .HasOne(upa => upa.User)
            .WithMany(u => u.PlaylistActivities)
            .HasForeignKey(upa => upa.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<UserQuizResult>()
            .HasOne(uqr => uqr.User)
            .WithMany(u => u.QuizResults)
            .HasForeignKey(uqr => uqr.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<UserSongPlay>()
            .HasOne(usp => usp.User)
            .WithMany(u => u.SongPlays)
            .HasForeignKey(usp => usp.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<UserHandwritingResult>()
            .HasOne(uhr => uhr.User)
            .WithMany(u => u.HandwritingResults)
            .HasForeignKey(uhr => uhr.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<LearnedWord>()
            .HasOne(lw => lw.User)
            .WithMany(u => u.LearnedWords)
            .HasForeignKey(lw => lw.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}


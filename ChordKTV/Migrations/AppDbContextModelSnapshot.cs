﻿// <auto-generated />
using System;
using System.Collections.Generic;
using ChordKTV.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ChordKTV.Migrations
{
    [DbContext(typeof(AppDbContext))]
    partial class AppDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.1")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("AlbumSong", b =>
                {
                    b.Property<Guid>("AlbumsId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("SongsId")
                        .HasColumnType("uuid");

                    b.HasKey("AlbumsId", "SongsId");

                    b.HasIndex("SongsId");

                    b.ToTable("AlbumSong");
                });

            modelBuilder.Entity("ChordKTV.Models.SongData.Album", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Artist")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<bool>("IsSingle")
                        .HasColumnType("boolean");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Albums");
                });

            modelBuilder.Entity("ChordKTV.Models.SongData.GeniusMetaData", b =>
                {
                    b.Property<int>("GeniusId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("GeniusId"));

                    b.Property<string>("HeaderImageThumbnailUrl")
                        .HasColumnType("text");

                    b.Property<string>("HeaderImageUrl")
                        .HasColumnType("text");

                    b.Property<int>("Language")
                        .HasColumnType("integer");

                    b.Property<string>("SongImageThumbnailUrl")
                        .HasColumnType("text");

                    b.Property<string>("SongImageUrl")
                        .HasColumnType("text");

                    b.HasKey("GeniusId");

                    b.ToTable("GeniusMetaData");
                });

            modelBuilder.Entity("ChordKTV.Models.SongData.Song", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.PrimitiveCollection<List<string>>("AlternateNames")
                        .IsRequired()
                        .HasColumnType("text[]");

                    b.PrimitiveCollection<List<string>>("AlternateYoutubeUrls")
                        .IsRequired()
                        .HasColumnType("text[]");

                    b.PrimitiveCollection<List<string>>("FeaturedArtists")
                        .IsRequired()
                        .HasColumnType("text[]");

                    b.Property<int>("GeniusMetaDataGeniusId")
                        .HasColumnType("integer");

                    b.Property<string>("Genre")
                        .HasColumnType("text");

                    b.Property<string>("LLMTranslation")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("LrcId")
                        .HasColumnType("integer");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("PlainLyrics")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("PrimaryArtist")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateOnly?>("ReleaseDate")
                        .HasColumnType("date");

                    b.Property<TimeSpan?>("SongDuration")
                        .HasColumnType("interval");

                    b.Property<string>("SyncLyrics")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("YoutubeUrl")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("GeniusMetaDataGeniusId");

                    b.ToTable("Songs");
                });

            modelBuilder.Entity("ChordKTV.Models.UserData.User", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("text");

                    b.PrimitiveCollection<List<string>>("FavoriteAlbums")
                        .IsRequired()
                        .HasColumnType("text[]");

                    b.PrimitiveCollection<List<string>>("FavoriteArtists")
                        .IsRequired()
                        .HasColumnType("text[]");

                    b.PrimitiveCollection<List<string>>("FavoritePlaylistLinks")
                        .IsRequired()
                        .HasColumnType("text[]");

                    b.PrimitiveCollection<List<string>>("FavoriteSongs")
                        .IsRequired()
                        .HasColumnType("text[]");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.PrimitiveCollection<List<string>>("PlaylistHistory")
                        .IsRequired()
                        .HasColumnType("text[]");

                    b.PrimitiveCollection<List<string>>("SongHistory")
                        .IsRequired()
                        .HasColumnType("text[]");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("AlbumSong", b =>
                {
                    b.HasOne("ChordKTV.Models.SongData.Album", null)
                        .WithMany()
                        .HasForeignKey("AlbumsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ChordKTV.Models.SongData.Song", null)
                        .WithMany()
                        .HasForeignKey("SongsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("ChordKTV.Models.SongData.Song", b =>
                {
                    b.HasOne("ChordKTV.Models.SongData.GeniusMetaData", "GeniusMetaData")
                        .WithMany()
                        .HasForeignKey("GeniusMetaDataGeniusId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("GeniusMetaData");
                });
#pragma warning restore 612, 618
        }
    }
}

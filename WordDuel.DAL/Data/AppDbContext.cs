using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using WordDuel.DAL.Models;

namespace WordDuel.DAL.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        public DbSet<MatchModel> Matches { get; set; } = null!;
        public DbSet<PlayerModel> Players { get; set; } = null!;
        public DbSet<RoundModel> Rounds { get; set; } = null!;
        public DbSet<MoveModel> Moves { get; set; } = null!;


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<MatchModel>(entity =>
            {
                entity.HasKey(m => m.Id);

                entity.Property(m => m.State)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(m => m.BestOf)
                    .IsRequired();

                entity.Property(m => m.TurnTimeSeconds)
                    .IsRequired();

                // Match -> CurrentRound
                entity.HasOne(m => m.CurrentRound)
                    .WithMany()
                    .HasForeignKey(m => m.CurrentRoundId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Match -> CurrentPlayer
                entity.HasOne(m => m.CurrentPlayer)
                    .WithMany()
                    .HasForeignKey(m => m.CurrentPlayerId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Match -> WinnerPlayer
                entity.HasOne(m => m.WinnerPlayer)
                    .WithMany()
                    .HasForeignKey(m => m.WinnerPlayerId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Match -> Players
                entity.HasMany(m => m.Players)
                    .WithOne(p => p.Match)
                    .HasForeignKey(p => p.MatchId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Match -> Rounds
                entity.HasMany(m => m.Rounds)
                    .WithOne(r => r.Match)
                    .HasForeignKey(r => r.MatchId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<PlayerModel>(entity =>
            {
                entity.HasKey(p => p.Id);

                entity.Property(p => p.Name)
                    .HasMaxLength(100);

                entity.Property(p => p.Score)
                    .IsRequired();

                // Player -> Moves
                entity.HasMany(p => p.Moves)
                    .WithOne(m => m.Player)
                    .HasForeignKey(m => m.PlayerId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<RoundModel>(entity =>
            {
                entity.HasKey(r => r.Id);

                entity.Property(r => r.RoundNumber)
                    .IsRequired();

                entity.Property(r => r.StartingWord)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(r => r.CurrentWord)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(r => r.State)
                    .IsRequired()
                    .HasMaxLength(50);

                // Round -> WinnerPlayer
                entity.HasOne(r => r.WinnerPlayer)
                    .WithMany()
                    .HasForeignKey(r => r.WinnerPlayerId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Round -> StartingPlayer
                entity.HasOne(r => r.StartingPlayer)
                    .WithMany()
                    .HasForeignKey(r => r.StartingPlayerId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Round -> Moves
                entity.HasMany(r => r.Moves)
                    .WithOne(m => m.Round)
                    .HasForeignKey(m => m.RoundId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<MoveModel>(entity =>
            {
                entity.HasKey(m => m.Id);

                entity.Property(m => m.Word)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(m => m.MoveNumber)
                    .IsRequired();
            });
        }


    }
}

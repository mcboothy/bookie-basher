using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace BookieBasher.Core.Database
{
    public partial class BBDBContext : DbContext
    {
        public BBDBContext()
        {
        }

        public BBDBContext(DbContextOptions<BBDBContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Averagestat> Averagestat { get; set; }
        public virtual DbSet<Competition> Competition { get; set; }
        public virtual DbSet<Fixtures> Fixtures { get; set; }
        public virtual DbSet<Leagueposition> Leagueposition { get; set; }
        public virtual DbSet<Leagueteams> Leagueteams { get; set; }
        public virtual DbSet<Match> Match { get; set; }
        public virtual DbSet<Matchstats> Matchstats { get; set; }
        public virtual DbSet<Season> Season { get; set; }
        public virtual DbSet<Team> Team { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                optionsBuilder.UseMySql("server=localhost;port=3306;user=root;password=admin;database=bookie_basher;treattinyasboolean=true", x => x.ServerVersion("8.0.11-mysql"));
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Averagestat>(entity =>
            {
                entity.ToTable("averagestat");

                entity.HasIndex(e => e.SeasonId)
                    .HasName("FK_Average_Season_idx");

                entity.HasIndex(e => e.TeamId)
                    .HasName("FK_Average_Team_idx");

                entity.Property(e => e.AverageStatId)
                    .HasColumnName("AverageStatID")
                    .HasColumnType("int(11)");

                entity.Property(e => e.SeasonId)
                    .HasColumnName("SeasonID")
                    .HasColumnType("int(11)");

                entity.Property(e => e.TeamId)
                    .HasColumnName("TeamID")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Type)
                    .IsRequired()
                    .HasColumnType("varchar(45)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.HasOne(d => d.Season)
                    .WithMany(p => p.Averagestat)
                    .HasForeignKey(d => d.SeasonId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Average_Season");

                entity.HasOne(d => d.Team)
                    .WithMany(p => p.Averagestat)
                    .HasForeignKey(d => d.TeamId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Average_Team");
            });

            modelBuilder.Entity<Competition>(entity =>
            {
                entity.ToTable("competition");

                entity.Property(e => e.CompetitionId)
                    .HasColumnName("CompetitionID")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Name)
                    .HasColumnType("varchar(60)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.Url)
                    .HasColumnName("URL")
                    .HasColumnType("varchar(80)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");
            });

            modelBuilder.Entity<Fixtures>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("fixtures");

                entity.Property(e => e.AwayTeam)
                    .HasColumnType("varchar(45)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.AwayTeamId)
                    .HasColumnName("AwayTeamID")
                    .HasColumnType("int(11)");

                entity.Property(e => e.AwayTeamStatsId)
                    .HasColumnName("AwayTeamStatsID")
                    .HasColumnType("int(11)");

                entity.Property(e => e.DateTime).HasColumnType("datetime");

                entity.Property(e => e.HomeTeam)
                    .HasColumnType("varchar(45)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.HomeTeamId)
                    .HasColumnName("HomeTeamID")
                    .HasColumnType("int(11)");

                entity.Property(e => e.HomeTeamStatsId)
                    .HasColumnName("HomeTeamStatsID")
                    .HasColumnType("int(11)");

                entity.Property(e => e.MatchId)
                    .HasColumnName("MatchID")
                    .HasColumnType("int(11)");

                entity.Property(e => e.SeasonId)
                    .HasColumnName("SeasonID")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Status)
                    .HasColumnType("enum('Fixture','Result','InPlay','Updating')")
                    .HasDefaultValueSql("'Updating'")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");
            });

            modelBuilder.Entity<Leagueposition>(entity =>
            {
                entity.ToTable("leagueposition");

                entity.HasIndex(e => e.SeasonId)
                    .HasName("FK_League_Season_idx");

                entity.HasIndex(e => e.TeamId)
                    .HasName("FK_League_Team_idx");

                entity.Property(e => e.LeaguePositionId)
                    .HasColumnName("LeaguePositionID")
                    .HasColumnType("int(11)");

                entity.Property(e => e.GoalsConceeded).HasColumnType("smallint(3)");

                entity.Property(e => e.GolsScored).HasColumnType("smallint(3)");

                entity.Property(e => e.MatchesDrawn).HasColumnType("smallint(2)");

                entity.Property(e => e.MatchesLost).HasColumnType("smallint(2)");

                entity.Property(e => e.MatchesPlayed).HasColumnType("smallint(2)");

                entity.Property(e => e.MatchesWon).HasColumnType("smallint(2)");

                entity.Property(e => e.Points).HasColumnType("smallint(3)");

                entity.Property(e => e.Position).HasColumnType("smallint(2)");

                entity.Property(e => e.SeasonId)
                    .HasColumnName("SeasonID")
                    .HasColumnType("int(11)");

                entity.Property(e => e.TeamId)
                    .HasColumnName("TeamID")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Type)
                    .IsRequired()
                    .HasColumnType("varchar(45)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.HasOne(d => d.Season)
                    .WithMany(p => p.Leagueposition)
                    .HasForeignKey(d => d.SeasonId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_League_Season");

                entity.HasOne(d => d.Team)
                    .WithMany(p => p.Leagueposition)
                    .HasForeignKey(d => d.TeamId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_League_Team");
            });

            modelBuilder.Entity<Leagueteams>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("leagueteams");

                entity.Property(e => e.LogoUrl)
                    .HasColumnName("LogoURL")
                    .HasColumnType("varchar(100)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.Name)
                    .HasColumnType("varchar(45)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.SeasonId)
                    .HasColumnName("SeasonID")
                    .HasColumnType("int(11)");

                entity.Property(e => e.TeamId)
                    .HasColumnName("TeamID")
                    .HasColumnType("int(11)");
            });

            modelBuilder.Entity<Match>(entity =>
            {
                entity.ToTable("match");

                entity.HasIndex(e => e.AwayTeamId)
                    .HasName("FK_AwayTeamID_idx");

                entity.HasIndex(e => e.AwayTeamStatsId)
                    .HasName("FK_AwayTeamStatsID_idx");

                entity.HasIndex(e => e.HomeTeamId)
                    .HasName("FK_HomeTeamID_idx");

                entity.HasIndex(e => e.HomeTeamStatsId)
                    .HasName("FK_HomeTeamStatsID_idx");

                entity.HasIndex(e => e.SeasonId)
                    .HasName("F_SeasonID_idx");

                entity.Property(e => e.MatchId)
                    .HasColumnName("MatchID")
                    .HasColumnType("int(11)");

                entity.Property(e => e.AwayTeamId)
                    .HasColumnName("AwayTeamID")
                    .HasColumnType("int(11)");

                entity.Property(e => e.AwayTeamStatsId)
                    .HasColumnName("AwayTeamStatsID")
                    .HasColumnType("int(11)");

                entity.Property(e => e.DateTime).HasColumnType("datetime");

                entity.Property(e => e.FsmatchId)
                    .IsRequired()
                    .HasColumnName("FSMatchID")
                    .HasColumnType("varchar(45)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.HomeTeamId)
                    .HasColumnName("HomeTeamID")
                    .HasColumnType("int(11)");

                entity.Property(e => e.HomeTeamStatsId)
                    .HasColumnName("HomeTeamStatsID")
                    .HasColumnType("int(11)");

                entity.Property(e => e.LastUpdated).HasColumnType("datetime");

                entity.Property(e => e.Postponed).HasColumnType("tinyint(4)");

                entity.Property(e => e.SeasonId)
                    .HasColumnName("SeasonID")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Status)
                    .HasColumnType("enum('Fixture','Result','InPlay','Updating')")
                    .HasDefaultValueSql("'Updating'")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.HasOne(d => d.AwayTeam)
                    .WithMany(p => p.MatchAwayTeam)
                    .HasForeignKey(d => d.AwayTeamId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_AwayTeamID");

                entity.HasOne(d => d.AwayTeamStats)
                    .WithMany(p => p.MatchAwayTeamStats)
                    .HasForeignKey(d => d.AwayTeamStatsId)
                    .HasConstraintName("FK_AwayTeamStatsID");

                entity.HasOne(d => d.HomeTeam)
                    .WithMany(p => p.MatchHomeTeam)
                    .HasForeignKey(d => d.HomeTeamId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_HomeTeamID");

                entity.HasOne(d => d.HomeTeamStats)
                    .WithMany(p => p.MatchHomeTeamStats)
                    .HasForeignKey(d => d.HomeTeamStatsId)
                    .HasConstraintName("FK_HomeTeamStatsID");

                entity.HasOne(d => d.Season)
                    .WithMany(p => p.Match)
                    .HasForeignKey(d => d.SeasonId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("F_SeasonID");
            });

            modelBuilder.Entity<Matchstats>(entity =>
            {
                entity.HasKey(e => e.StatId)
                    .HasName("PRIMARY");

                entity.ToTable("matchstats");

                entity.Property(e => e.StatId)
                    .HasColumnName("StatID")
                    .HasColumnType("int(11)");

                entity.Property(e => e.FirstHalfCards).HasColumnType("int(2)");

                entity.Property(e => e.FirstHalfGoals).HasColumnType("int(2)");

                entity.Property(e => e.TotalCards).HasColumnType("int(2)");

                entity.Property(e => e.TotalGoals).HasColumnType("int(2)");
            });

            modelBuilder.Entity<Season>(entity =>
            {
                entity.ToTable("season");

                entity.HasIndex(e => e.CompetitionId)
                    .HasName("FK_CompetitionII_idx");

                entity.Property(e => e.SeasonId)
                    .HasColumnName("SeasonID")
                    .HasColumnType("int(11)");

                entity.Property(e => e.CompetitionId)
                    .HasColumnName("CompetitionID")
                    .HasColumnType("int(11)");

                entity.Property(e => e.LastUpdated).HasColumnType("datetime");

                entity.Property(e => e.Status)
                    .HasColumnType("enum('Updated','Updating','Creating')")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.Year)
                    .HasColumnType("varchar(45)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.HasOne(d => d.Competition)
                    .WithMany(p => p.Season)
                    .HasForeignKey(d => d.CompetitionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_CompetitionII");
            });

            modelBuilder.Entity<Team>(entity =>
            {
                entity.ToTable("team");

                entity.Property(e => e.TeamId)
                    .HasColumnName("TeamID")
                    .HasColumnType("int(11)");

                entity.Property(e => e.LogoUrl)
                    .HasColumnName("LogoURL")
                    .HasColumnType("varchar(100)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.Name)
                    .HasColumnType("varchar(45)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}

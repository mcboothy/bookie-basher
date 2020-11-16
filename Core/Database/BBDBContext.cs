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

        public virtual DbSet<AverageStat> AverageStat { get; set; }
        public virtual DbSet<Competition> Competition { get; set; }
        public virtual DbSet<CompetitionAlias> CompetitionAlias { get; set; }
        public virtual DbSet<Country> Country { get; set; }
        public virtual DbSet<Error> Error { get; set; }
        public virtual DbSet<Fixtures> Fixtures { get; set; }
        public virtual DbSet<LeagueTeams> LeagueTeams { get; set; }
        public virtual DbSet<Log> Log { get; set; }
        public virtual DbSet<Match> Match { get; set; }
        public virtual DbSet<MatchStats> MatchStats { get; set; }
        public virtual DbSet<Season> Season { get; set; }
        public virtual DbSet<Team> Team { get; set; }
        public virtual DbSet<TeamAlias> TeamAlias { get; set; }
        public virtual DbSet<UnknownTeams> UnknownTeams { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                optionsBuilder.UseMySql("server=192.168.1.210;port=3306;user=bookie-basher-user;password=P@ssword12;database=Bookie-Basher;treattinyasboolean=true", x => x.ServerVersion("10.4.12-mariadb"));
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AverageStat>(entity =>
            {
                entity.HasIndex(e => e.SeasonId)
                    .HasName("FK_Average_Season_idx");

                entity.HasIndex(e => e.TeamId)
                    .HasName("FK_Average_Team_idx");

                entity.Property(e => e.AverageStatId)
                    .HasColumnName("AverageStatID")
                    .HasColumnType("int(11)");

                entity.Property(e => e.GamesPlayed).HasColumnType("int(2)");

                entity.Property(e => e.SeasonId)
                    .HasColumnName("SeasonID")
                    .HasColumnType("int(11)");

                entity.Property(e => e.TeamId)
                    .HasColumnName("TeamID")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Type)
                    .IsRequired()
                    .HasColumnType("enum('Home','Away','Overall')")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.HasOne(d => d.Season)
                    .WithMany(p => p.AverageStat)
                    .HasForeignKey(d => d.SeasonId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Average_Season");

                entity.HasOne(d => d.Team)
                    .WithMany(p => p.AverageStat)
                    .HasForeignKey(d => d.TeamId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Average_Team");
            });

            modelBuilder.Entity<Competition>(entity =>
            {
                entity.HasIndex(e => e.CountryId)
                    .HasName("FK_Competition_Country_idx");

                entity.Property(e => e.CompetitionId)
                    .HasColumnName("CompetitionID")
                    .HasColumnType("int(11)");

                entity.Property(e => e.BetfairId)
                    .HasColumnName("BetfairID")
                    .HasColumnType("int(11)");

                entity.Property(e => e.CountryId)
                    .HasColumnName("CountryID")
                    .HasColumnType("int(11)");

                entity.Property(e => e.EndDate).HasColumnType("datetime");

                entity.Property(e => e.FlashScoreUrl)
                    .HasColumnName("FlashScoreURL")
                    .HasColumnType("varchar(80)")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.Property(e => e.SoccerWikiId).HasColumnType("int(11)");

                entity.Property(e => e.Sponsor)
                    .HasColumnType("varchar(45)")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.Property(e => e.StartDate).HasColumnType("datetime");

                entity.Property(e => e.YearFounded)
                    .HasColumnType("varchar(45)")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.HasOne(d => d.Country)
                    .WithMany(p => p.Competition)
                    .HasForeignKey(d => d.CountryId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Competition_Country");
            });

            modelBuilder.Entity<CompetitionAlias>(entity =>
            {
                entity.HasKey(e => e.AliasId)
                    .HasName("PRIMARY");

                entity.HasIndex(e => e.CompetitionId)
                    .HasName("FK_Alias_Competition_idx");

                entity.Property(e => e.AliasId)
                    .HasColumnName("AliasID")
                    .HasColumnType("int(11)");

                entity.Property(e => e.CompetitionId)
                    .HasColumnName("CompetitionID")
                    .HasColumnType("int(11)");

                entity.Property(e => e.IsDefault).HasColumnType("tinyint(4)");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnType("varchar(45)")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.Property(e => e.Tag)
                    .IsRequired()
                    .HasColumnType("varchar(45)")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.HasOne(d => d.Competition)
                    .WithMany(p => p.CompetitionAlias)
                    .HasForeignKey(d => d.CompetitionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Alias_Competition");
            });

            modelBuilder.Entity<Country>(entity =>
            {
                entity.Property(e => e.CountryId)
                    .HasColumnName("CountryID")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnType("varchar(45)")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");
            });

            modelBuilder.Entity<Error>(entity =>
            {
                entity.HasKey(e => e.ErrorId)
                    .HasName("PRIMARY");

                entity.Property(e => e.ErrorId).HasColumnType("int(11)");

                entity.Property(e => e.Message)
                    .HasColumnType("longtext")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.Property(e => e.Request)
                    .IsRequired()
                    .HasColumnType("longtext")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");
            });

            modelBuilder.Entity<Fixtures>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("Fixtures");

                entity.Property(e => e.AwayTeam)
                    .IsRequired()
                    .HasColumnType("varchar(45)")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.Property(e => e.AwayTeamId)
                    .HasColumnName("AwayTeamID")
                    .HasColumnType("int(11)");

                entity.Property(e => e.AwayTeamStatsId)
                    .HasColumnName("AwayTeamStatsID")
                    .HasColumnType("int(11)");

                entity.Property(e => e.DateTime).HasColumnType("datetime");

                entity.Property(e => e.HomeTeam)
                    .IsRequired()
                    .HasColumnType("varchar(45)")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

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
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");
            });

            modelBuilder.Entity<LeagueTeams>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("LeagueTeams");

                entity.Property(e => e.LogoUrl)
                    .IsRequired()
                    .HasColumnName("LogoURL")
                    .HasColumnType("varchar(100)")
                    .HasDefaultValueSql("''")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnType("varchar(45)")
                    .HasDefaultValueSql("''")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.Property(e => e.SeasonId)
                    .HasColumnName("SeasonID")
                    .HasColumnType("int(11)");

                entity.Property(e => e.TeamId)
                    .HasColumnName("TeamID")
                    .HasColumnType("int(11)");
            });

            modelBuilder.Entity<Log>(entity =>
            {
                entity.HasKey(e => e.LogId)
                    .HasName("PRIMARY");

                entity.Property(e => e.LogId).HasColumnType("int(11)");

                entity.Property(e => e.Host)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasCharSet("latin1")
                    .HasCollation("latin1_swedish_ci");

                entity.Property(e => e.Message)
                    .IsRequired()
                    .HasColumnType("longtext")
                    .HasCharSet("latin1")
                    .HasCollation("latin1_swedish_ci");

                entity.Property(e => e.ServiceName)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasCharSet("latin1")
                    .HasCollation("latin1_swedish_ci");
            });

            modelBuilder.Entity<Match>(entity =>
            {
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
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

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
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

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

            modelBuilder.Entity<MatchStats>(entity =>
            {
                entity.HasKey(e => e.StatId)
                    .HasName("PRIMARY");

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
                    .HasColumnType("enum('Updated','Updating','Creating','Failed')")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.Property(e => e.Year)
                    .HasColumnType("varchar(45)")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.HasOne(d => d.Competition)
                    .WithMany(p => p.Season)
                    .HasForeignKey(d => d.CompetitionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_CompetitionII");
            });

            modelBuilder.Entity<Team>(entity =>
            {
                entity.HasIndex(e => e.SeasonId)
                    .HasName("FK_Team_SeasonID_idx");

                entity.Property(e => e.TeamId)
                    .HasColumnName("TeamID")
                    .HasColumnType("int(11)");

                entity.Property(e => e.LogoUrl)
                    .IsRequired()
                    .HasColumnName("LogoURL")
                    .HasColumnType("varchar(100)")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnType("varchar(45)")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.Property(e => e.SeasonId)
                    .HasColumnName("SeasonID")
                    .HasColumnType("int(11)");

                entity.HasOne(d => d.Season)
                    .WithMany(p => p.Team)
                    .HasForeignKey(d => d.SeasonId)
                    .HasConstraintName("FK_Team_SeasonID");
            });

            modelBuilder.Entity<TeamAlias>(entity =>
            {
                entity.HasKey(e => e.AliasId)
                    .HasName("PRIMARY");

                entity.HasIndex(e => e.TeamId)
                    .HasName("FK_Team_Alias_idx");

                entity.Property(e => e.AliasId)
                    .HasColumnName("AliasID")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Alias)
                    .IsRequired()
                    .HasColumnType("varchar(45)")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.Property(e => e.IsDefault).HasColumnType("tinyint(4)");

                entity.Property(e => e.Tag)
                    .IsRequired()
                    .HasColumnType("varchar(45)")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.Property(e => e.TeamId)
                    .HasColumnName("TeamID")
                    .HasColumnType("int(11)");

                entity.HasOne(d => d.Team)
                    .WithMany(p => p.TeamAlias)
                    .HasForeignKey(d => d.TeamId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Team_Alias");
            });

            modelBuilder.Entity<UnknownTeams>(entity =>
            {
                entity.HasIndex(e => e.SeasonId)
                    .HasName("FK_UnknownTeams_Season_idx");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Request)
                    .HasColumnType("text")
                    .HasCharSet("latin1")
                    .HasCollation("latin1_swedish_ci");

                entity.Property(e => e.SeasonId)
                    .HasColumnName("SeasonID")
                    .HasColumnType("int(11)");

                entity.Property(e => e.TeamsResponce)
                    .HasColumnType("text")
                    .HasCharSet("latin1")
                    .HasCollation("latin1_swedish_ci");

                entity.HasOne(d => d.Season)
                    .WithMany(p => p.UnknownTeams)
                    .HasForeignKey(d => d.SeasonId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_UnknownTeams_Season");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}

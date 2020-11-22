using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

#nullable disable

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

        public virtual DbSet<AverageStat> AverageStats { get; set; }
        public virtual DbSet<Competition> Competitions { get; set; }
        public virtual DbSet<CompetitionAlias> CompetitionAliases { get; set; }
        public virtual DbSet<Country> Countries { get; set; }
        public virtual DbSet<Error> Errors { get; set; }
        public virtual DbSet<Fixture> Fixtures { get; set; }
        public virtual DbSet<LeagueTeam> LeagueTeams { get; set; }
        public virtual DbSet<Log> Logs { get; set; }
        public virtual DbSet<Match> Matches { get; set; }
        public virtual DbSet<MatchStat> MatchStats { get; set; }
        public virtual DbSet<Season> Seasons { get; set; }
        public virtual DbSet<Team> Teams { get; set; }
        public virtual DbSet<TeamAlias> TeamAliases { get; set; }
        public virtual DbSet<UnknownTeam> UnknownTeams { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
                optionsBuilder.UseMySql("server=192.168.1.210;port=3306;user=bookie-basher-user;password=P@ssword12;database=Bookie-Basher;treattinyasboolean=true", Microsoft.EntityFrameworkCore.ServerVersion.FromString("10.4.12-mariadb"));
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AverageStat>(entity =>
            {
                entity.ToTable("AverageStat");

                entity.HasIndex(e => e.SeasonId, "FK_Average_Season_idx");

                entity.HasIndex(e => e.TeamId, "FK_Average_Team_idx");

                entity.Property(e => e.AverageStatId)
                    .HasColumnType("int(11)")
                    .HasColumnName("AverageStatID");

                entity.Property(e => e.GamesPlayed).HasColumnType("int(2)");

                entity.Property(e => e.SeasonId)
                    .HasColumnType("int(11)")
                    .HasColumnName("SeasonID");

                entity.Property(e => e.TeamId)
                    .HasColumnType("int(11)")
                    .HasColumnName("TeamID");

                entity.Property(e => e.Type)
                    .IsRequired()
                    .HasColumnType("enum('Home','Away','Overall')")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.HasOne(d => d.Season)
                    .WithMany(p => p.AverageStats)
                    .HasForeignKey(d => d.SeasonId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Average_Season");

                entity.HasOne(d => d.Team)
                    .WithMany(p => p.AverageStats)
                    .HasForeignKey(d => d.TeamId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Average_Team");
            });

            modelBuilder.Entity<Competition>(entity =>
            {
                entity.ToTable("Competition");

                entity.HasIndex(e => e.CountryId, "FK_Competition_Country_idx");

                entity.Property(e => e.CompetitionId)
                    .HasColumnType("int(11)")
                    .HasColumnName("CompetitionID");

                entity.Property(e => e.BetfairId)
                    .HasColumnType("int(11)")
                    .HasColumnName("BetfairID");

                entity.Property(e => e.CountryId)
                    .HasColumnType("int(11)")
                    .HasColumnName("CountryID");

                entity.Property(e => e.EndDate).HasColumnType("datetime");

                entity.Property(e => e.FlashScoreUrl)
                    .HasColumnType("varchar(80)")
                    .HasColumnName("FlashScoreURL")
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
                    .WithMany(p => p.Competitions)
                    .HasForeignKey(d => d.CountryId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Competition_Country");
            });

            modelBuilder.Entity<CompetitionAlias>(entity =>
            {
                entity.HasKey(e => e.AliasId)
                    .HasName("PRIMARY");

                entity.ToTable("CompetitionAlias");

                entity.HasIndex(e => e.CompetitionId, "FK_Alias_Competition_idx");

                entity.Property(e => e.AliasId)
                    .HasColumnType("int(11)")
                    .HasColumnName("AliasID");

                entity.Property(e => e.CompetitionId)
                    .HasColumnType("int(11)")
                    .HasColumnName("CompetitionID");

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
                    .WithMany(p => p.CompetitionAliases)
                    .HasForeignKey(d => d.CompetitionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Alias_Competition");
            });

            modelBuilder.Entity<Country>(entity =>
            {
                entity.ToTable("Country");

                entity.Property(e => e.CountryId)
                    .HasColumnType("int(11)")
                    .HasColumnName("CountryID");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnType("varchar(45)")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");
            });

            modelBuilder.Entity<Error>(entity =>
            {
                entity.ToTable("Error");

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

            modelBuilder.Entity<Fixture>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("Fixtures");

                entity.Property(e => e.AwayTeam)
                    .IsRequired()
                    .HasColumnType("varchar(45)")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.Property(e => e.AwayTeamId)
                    .HasColumnType("int(11)")
                    .HasColumnName("AwayTeamID");

                entity.Property(e => e.AwayTeamStatsId)
                    .HasColumnType("int(11)")
                    .HasColumnName("AwayTeamStatsID");

                entity.Property(e => e.DateTime).HasColumnType("datetime");

                entity.Property(e => e.HomeTeam)
                    .IsRequired()
                    .HasColumnType("varchar(45)")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.Property(e => e.HomeTeamId)
                    .HasColumnType("int(11)")
                    .HasColumnName("HomeTeamID");

                entity.Property(e => e.HomeTeamStatsId)
                    .HasColumnType("int(11)")
                    .HasColumnName("HomeTeamStatsID");

                entity.Property(e => e.MatchId)
                    .HasColumnType("int(11)")
                    .HasColumnName("MatchID");

                entity.Property(e => e.SeasonId)
                    .HasColumnType("int(11)")
                    .HasColumnName("SeasonID");

                entity.Property(e => e.Status)
                    .HasColumnType("enum('Fixture','Result','InPlay','Updating')")
                    .HasDefaultValueSql("'Updating'")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");
            });

            modelBuilder.Entity<LeagueTeam>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("LeagueTeams");

                entity.Property(e => e.LogoUrl)
                    .IsRequired()
                    .HasColumnType("varchar(100)")
                    .HasColumnName("LogoURL")
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
                    .HasColumnType("int(11)")
                    .HasColumnName("SeasonID");

                entity.Property(e => e.TeamId)
                    .HasColumnType("int(11)")
                    .HasColumnName("TeamID");
            });

            modelBuilder.Entity<Log>(entity =>
            {
                entity.ToTable("Log");

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
                entity.ToTable("Match");

                entity.HasIndex(e => e.AwayTeamId, "FK_AwayTeamID_idx");

                entity.HasIndex(e => e.AwayTeamStatsId, "FK_AwayTeamStatsID_idx");

                entity.HasIndex(e => e.HomeTeamId, "FK_HomeTeamID_idx");

                entity.HasIndex(e => e.HomeTeamStatsId, "FK_HomeTeamStatsID_idx");

                entity.HasIndex(e => e.SeasonId, "F_SeasonID_idx");

                entity.Property(e => e.MatchId)
                    .HasColumnType("int(11)")
                    .HasColumnName("MatchID");

                entity.Property(e => e.AwayTeamId)
                    .HasColumnType("int(11)")
                    .HasColumnName("AwayTeamID");

                entity.Property(e => e.AwayTeamStatsId)
                    .HasColumnType("int(11)")
                    .HasColumnName("AwayTeamStatsID");

                entity.Property(e => e.DateTime).HasColumnType("datetime");

                entity.Property(e => e.FsmatchId)
                    .IsRequired()
                    .HasColumnType("varchar(45)")
                    .HasColumnName("FSMatchID")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.Property(e => e.HomeTeamId)
                    .HasColumnType("int(11)")
                    .HasColumnName("HomeTeamID");

                entity.Property(e => e.HomeTeamStatsId)
                    .HasColumnType("int(11)")
                    .HasColumnName("HomeTeamStatsID");

                entity.Property(e => e.LastUpdated).HasColumnType("datetime");

                entity.Property(e => e.Postponed).HasColumnType("tinyint(4)");

                entity.Property(e => e.SeasonId)
                    .HasColumnType("int(11)")
                    .HasColumnName("SeasonID");

                entity.Property(e => e.Status)
                    .HasColumnType("enum('Fixture','Result','InPlay','Updating')")
                    .HasDefaultValueSql("'Updating'")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.HasOne(d => d.AwayTeam)
                    .WithMany(p => p.MatchAwayTeams)
                    .HasForeignKey(d => d.AwayTeamId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_AwayTeamID");

                entity.HasOne(d => d.AwayTeamStats)
                    .WithMany(p => p.MatchAwayTeamStats)
                    .HasForeignKey(d => d.AwayTeamStatsId)
                    .HasConstraintName("FK_AwayTeamStatsID");

                entity.HasOne(d => d.HomeTeam)
                    .WithMany(p => p.MatchHomeTeams)
                    .HasForeignKey(d => d.HomeTeamId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_HomeTeamID");

                entity.HasOne(d => d.HomeTeamStats)
                    .WithMany(p => p.MatchHomeTeamStats)
                    .HasForeignKey(d => d.HomeTeamStatsId)
                    .HasConstraintName("FK_HomeTeamStatsID");

                entity.HasOne(d => d.Season)
                    .WithMany(p => p.Matches)
                    .HasForeignKey(d => d.SeasonId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("F_SeasonID");
            });

            modelBuilder.Entity<MatchStat>(entity =>
            {
                entity.HasKey(e => e.StatId)
                    .HasName("PRIMARY");

                entity.Property(e => e.StatId)
                    .HasColumnType("int(11)")
                    .HasColumnName("StatID");

                entity.Property(e => e.FirstHalfCards).HasColumnType("int(2)");

                entity.Property(e => e.FirstHalfGoals).HasColumnType("int(2)");

                entity.Property(e => e.TotalCards).HasColumnType("int(2)");

                entity.Property(e => e.TotalGoals).HasColumnType("int(2)");
            });

            modelBuilder.Entity<Season>(entity =>
            {
                entity.ToTable("Season");

                entity.HasIndex(e => e.CompetitionId, "FK_CompetitionII_idx");

                entity.Property(e => e.SeasonId)
                    .HasColumnType("int(11)")
                    .HasColumnName("SeasonID");

                entity.Property(e => e.CompetitionId)
                    .HasColumnType("int(11)")
                    .HasColumnName("CompetitionID");

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
                    .WithMany(p => p.Seasons)
                    .HasForeignKey(d => d.CompetitionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_CompetitionII");
            });

            modelBuilder.Entity<Team>(entity =>
            {
                entity.ToTable("Team");

                entity.HasIndex(e => e.SeasonId, "FK_Team_SeasonID_idx");

                entity.Property(e => e.TeamId)
                    .HasColumnType("int(11)")
                    .HasColumnName("TeamID");

                entity.Property(e => e.LogoUrl)
                    .IsRequired()
                    .HasColumnType("varchar(100)")
                    .HasColumnName("LogoURL")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnType("varchar(45)")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.Property(e => e.SeasonId)
                    .HasColumnType("int(11)")
                    .HasColumnName("SeasonID");

                entity.HasOne(d => d.Season)
                    .WithMany(p => p.Teams)
                    .HasForeignKey(d => d.SeasonId)
                    .HasConstraintName("FK_Team_SeasonID");
            });

            modelBuilder.Entity<TeamAlias>(entity =>
            {
                entity.HasKey(e => e.AliasId)
                    .HasName("PRIMARY");

                entity.ToTable("TeamAlias");

                entity.HasIndex(e => e.TeamId, "FK_Team_Alias_idx");

                entity.Property(e => e.AliasId)
                    .HasColumnType("int(11)")
                    .HasColumnName("AliasID");

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
                    .HasColumnType("int(11)")
                    .HasColumnName("TeamID");

                entity.HasOne(d => d.Team)
                    .WithMany(p => p.TeamAliases)
                    .HasForeignKey(d => d.TeamId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Team_Alias");
            });

            modelBuilder.Entity<UnknownTeam>(entity =>
            {
                entity.HasIndex(e => e.SeasonId, "FK_UnknownTeams_Season_idx");

                entity.Property(e => e.Id)
                    .HasColumnType("int(11)")
                    .HasColumnName("ID");

                entity.Property(e => e.Request)
                    .HasColumnType("text")
                    .HasCharSet("latin1")
                    .HasCollation("latin1_swedish_ci");

                entity.Property(e => e.SeasonId)
                    .HasColumnType("int(11)")
                    .HasColumnName("SeasonID");

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

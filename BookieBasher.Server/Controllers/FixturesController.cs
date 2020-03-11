using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BookieBasher.Core.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;

namespace BookieBasher.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FixturesController : ControllerBase
    {
        private readonly ILogger<FixturesController> _logger;
        private readonly DbContextOptions<BBDBContext> _optons;
        public FixturesController(ILogger<FixturesController> logger, IConfiguration configuration)
        {
            string connectionString = configuration.GetValue<string>("ConnectionString");
            var optionsBuilder = new DbContextOptionsBuilder<BBDBContext>();
            optionsBuilder.UseMySql(connectionString);

            _optons = optionsBuilder.Options;
            _logger = logger;

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        [HttpGet]
        public async Task<FileResult> Get()
        {
            using (BBDBContext context = new BBDBContext(_optons))
            {
                var fixtures = await context.Fixtures.Where(m => m.DateTime.Date <= DateTime.Now.Date).ToListAsync();

                var map = new Dictionary<Fixtures, Dictionary<string, Stats>>();

                foreach (var match in fixtures)
                {
                    var homeStats = await context.Averagestat.Where(avg => avg.SeasonId == match.SeasonId &&
                                                                    avg.TeamId == match.HomeTeamId).ToListAsync();

                    var awayStats = await context.Averagestat.Where(avg => avg.SeasonId == match.SeasonId &&
                                                                    avg.TeamId == match.AwayTeamId).ToListAsync();

                    map.Add(match, new Dictionary<string, Stats>());

                    var stats = new List<Averagestat>(homeStats);
                    stats.AddRange(awayStats);

                    foreach (var stat in stats)
                    {
                        if (!map[match].ContainsKey(stat.Type))
                        {
                            map[match].Add(stat.Type, new Stats());
                        }

                        if (stat.TeamId == match.HomeTeamId)
                            map[match][stat.Type].Home = stat;
                        else
                            map[match][stat.Type].Away = stat;
                    }
                }

                var worksheets = map.Values.SelectMany(d => d.Keys).Distinct();

                using (var package = new ExcelPackage())
                {
                    foreach (var worksheet in worksheets.Select(w => package.Workbook.Worksheets.Add(w)))
                    {
                        int columnIndex = 1;
                        int rowIndex = 1;

                        //worksheet.Cells[1, 1].Value = "Long text";
                        //worksheet.Cells[1, 1].Style.Font.Size = 12;
                        //worksheet.Cells[1, 1].Style.Font.Bold = true;
                        //worksheet.Cells[1, 1].Style.Border.Top.Style = ExcelBorderStyle.Hair;

                        worksheet.Cells[rowIndex, columnIndex++].Value = "Home Team";
                        worksheet.Cells[rowIndex, columnIndex++].Value = "Away Team";
                        worksheet.Cells[rowIndex, columnIndex++].Value = "Point % Diff";
                        worksheet.Cells[rowIndex, columnIndex++].Value = "Total Goal Avg";
                        worksheet.Cells[rowIndex, columnIndex++].Value = "Total Goal fh Avg";
                        worksheet.Cells[rowIndex, columnIndex++].Value = "Total Goal sh Avg";
                        worksheet.Cells[rowIndex, columnIndex++].Value = "Total Card Avg";
                        worksheet.Cells[rowIndex, columnIndex++].Value = "Total Card fh Avg";
                        worksheet.Cells[rowIndex, columnIndex++].Value = "Total Card sh Avg";
                        worksheet.Cells[rowIndex, columnIndex++].Value = "Home Point %";
                        worksheet.Cells[rowIndex, columnIndex++].Value = "Away Point %";
                        worksheet.Cells[rowIndex, columnIndex++].Value = "Home Team Goal P/g Avg";
                        worksheet.Cells[rowIndex, columnIndex++].Value = "Away Team Goal P/g Avg";
                        worksheet.Cells[rowIndex, columnIndex++].Value = "Home Team Goal P/fh Avg";
                        worksheet.Cells[rowIndex, columnIndex++].Value = "Away Team Goal P/fh Avg";
                        worksheet.Cells[rowIndex, columnIndex++].Value = "Home Team Goal P/sh Avg";
                        worksheet.Cells[rowIndex, columnIndex++].Value = "Away Team Goal P/sh Avg";
                        worksheet.Cells[rowIndex, columnIndex++].Value = "Home Team Card P/g Avg";
                        worksheet.Cells[rowIndex, columnIndex++].Value = "Away Team Card P/g Avg";
                        worksheet.Cells[rowIndex, columnIndex++].Value = "Home Team Card P/fh Avg";
                        worksheet.Cells[rowIndex, columnIndex++].Value = "Away Team Card P/fh Avg";
                        worksheet.Cells[rowIndex, columnIndex++].Value = "Home Team Card P/sh Avg";
                        worksheet.Cells[rowIndex, columnIndex++].Value = "Away Team Card P/sh Avg";

                        foreach(var matchPair in map)
                        {
                            var match = matchPair.Key;

                            if (matchPair.Value.ContainsKey(worksheet.Name))
                            {
                                rowIndex++;
                                columnIndex = 1;

                                var statsPair = matchPair.Value[worksheet.Name];
                                var homeStat = statsPair.Home;
                                var awayStat = statsPair.Away;

                                worksheet.Cells[rowIndex, columnIndex++].Value = match.HomeTeam;
                                worksheet.Cells[rowIndex, columnIndex++].Value = match.AwayTeam;
                                if (homeStat != null && homeStat != null)
                                {
                                    worksheet.Cells[rowIndex, columnIndex++].Value = Math.Abs(homeStat.PointPercentage - awayStat.PointPercentage);
                                    worksheet.Cells[rowIndex, columnIndex++].Value = homeStat.TotalGoal + awayStat.TotalGoal;
                                    worksheet.Cells[rowIndex, columnIndex++].Value = homeStat.FirstHalfGoal + awayStat.FirstHalfGoal;
                                    worksheet.Cells[rowIndex, columnIndex++].Value = homeStat.SecondHalfGoal + awayStat.SecondHalfGoal;
                                    worksheet.Cells[rowIndex, columnIndex++].Value = homeStat.TotalCard + awayStat.TotalCard;
                                    worksheet.Cells[rowIndex, columnIndex++].Value = homeStat.FirstHalfCard + awayStat.FirstHalfCard;
                                    worksheet.Cells[rowIndex, columnIndex++].Value = homeStat.SecondHalfCard + awayStat.SecondHalfCard;
                                }
                                else
                                {
                                    worksheet.Cells[rowIndex, columnIndex++].Value = null;
                                    worksheet.Cells[rowIndex, columnIndex++].Value = null;
                                    worksheet.Cells[rowIndex, columnIndex++].Value = null;
                                    worksheet.Cells[rowIndex, columnIndex++].Value = null;
                                    worksheet.Cells[rowIndex, columnIndex++].Value = null;
                                    worksheet.Cells[rowIndex, columnIndex++].Value = null;
                                    worksheet.Cells[rowIndex, columnIndex++].Value = null;
                                    worksheet.Cells[rowIndex, columnIndex++].Value = null;
                                }
                                worksheet.Cells[rowIndex, columnIndex++].Value = homeStat?.PointPercentage;
                                worksheet.Cells[rowIndex, columnIndex++].Value = awayStat?.PointPercentage;
                                worksheet.Cells[rowIndex, columnIndex++].Value = homeStat?.TotalGoal;
                                worksheet.Cells[rowIndex, columnIndex++].Value = awayStat?.TotalGoal;
                                worksheet.Cells[rowIndex, columnIndex++].Value = homeStat?.FirstHalfGoal;
                                worksheet.Cells[rowIndex, columnIndex++].Value = awayStat?.FirstHalfGoal;
                                worksheet.Cells[rowIndex, columnIndex++].Value = homeStat?.SecondHalfGoal;
                                worksheet.Cells[rowIndex, columnIndex++].Value = awayStat?.SecondHalfGoal;
                                worksheet.Cells[rowIndex, columnIndex++].Value = homeStat?.TotalCard;
                                worksheet.Cells[rowIndex, columnIndex++].Value = awayStat?.TotalCard;
                                worksheet.Cells[rowIndex, columnIndex++].Value = homeStat?.FirstHalfCard;
                                worksheet.Cells[rowIndex, columnIndex++].Value = awayStat?.FirstHalfCard;
                                worksheet.Cells[rowIndex, columnIndex++].Value = homeStat?.SecondHalfCard;
                                worksheet.Cells[rowIndex, columnIndex++].Value = awayStat?.SecondHalfCard;
                            }
                        }
                    }

                    string mimeType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    return File(package.GetAsByteArray(), mimeType, "fixtures.xlsx");
                }
            }
        }

        private static void SetCellValue(ExcelWorksheet worksheet, int x, int y, object value)
        {
            worksheet.Cells[x, y].Value = value;
            worksheet.Cells[x, y].Style.Font.Size = 12;
            worksheet.Cells[x, y].Style.Font.Bold = true;
        }

        public class Stats
        {
            public Averagestat Home { get; set; }
            public Averagestat Away { get; set; }
        }
    }
}

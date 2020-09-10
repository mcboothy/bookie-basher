using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using BookieBasher.Core.IO;
using BookieBasher.Core.Database;

namespace BookieBasher.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FixturesController : ControllerBase
    {
        private readonly BBDBContext context;

        public FixturesController(BBDBContext context)
        {
            //this.logger = logger;
            this.context = context;
        }

        [HttpGet]
        public IEnumerable<JSMatch> Get()
        {
            return context.Fixtures.Select(m => new JSMatch() 
            {
                HomeTeam = m.HomeTeam,
                AwayTeam = m.AwayTeam,
                DateTime = m.DateTime, 
            }).Take(10);
        }
    }
}

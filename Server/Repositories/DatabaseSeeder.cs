using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorAppExampleSink.Server.Repositories
{
    public interface IDatabaseSeeder
    {
        void Initialize();
    }

    public class DatabaseSeeder : IDatabaseSeeder
    {
        private readonly ILogger<DatabaseSeeder> _logger;
        private readonly MyContext _db;


        public DatabaseSeeder(
            MyContext db,
            ILogger<DatabaseSeeder> logger)
        {
            _db = db;
            _logger = logger;
        }

        public void Initialize()
        {
            AddActivateSeedingAsync();
            _db.SaveChanges();
        }

        private void AddActivateSeedingAsync()
        {
            Task.Run(() =>
            {
                _logger.LogWarning("Check and Run Seeding.");  // Relevant?

            }).GetAwaiter().GetResult(); ;
        }
      
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileCheckingService.Entities.ConfigurationModels
{
    public class SqlConnectionConfig
    {
        public int MaxRetryCount { get; set; }
        public double RetryDelayInSeconds { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileCheckingService.Service.Contract
{
    public interface IScopedBackgroundService
    {
        Task DoWorkAsync(CancellationToken stoppingToken);
    }
}

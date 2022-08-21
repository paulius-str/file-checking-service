using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileCheckingService.Logging
{
    public class LoggerManager : ILoggerManager
    {
        private static ILogger _logger = LogManager.GetCurrentClassLogger();

        public void LogDebug(string message)
        {
            _logger.Debug(message);
        }   

        public void LogInfo(string message)
        {
            _logger.Info(message);
        }

        public void LogError(string message)
        {
            _logger.Error(message);
        }

        public void LogError(string message, Exception exception)
        {
            _logger.Error($"{message}, error message: {exception.Message}, stack trace: {exception.StackTrace}");
        }

        public void LogWarning(string message)
        {
            _logger.Warn(message);
        }
    }
}

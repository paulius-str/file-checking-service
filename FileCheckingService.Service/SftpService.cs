using FileCheckingService.Entities.ConfigurationModels;
using Microsoft.Extensions.Logging;
using Renci.SshNet.Sftp;
using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileCheckingService.Logging;
using Microsoft.Extensions.Configuration;
using FileCheckingService.Service.Contract;
using Renci.SshNet.Async;
using System.IO;

namespace FileCheckingService.Service
{
    public class SftpService : ISftpService
    {
        private readonly ILoggerManager _logger;
        private readonly SftpConfig _config;

        public SftpService(ILoggerManager logger, IConfiguration configuration)
        {
            _logger = logger;
            _config = configuration.GetRequiredSection("SftpConfiguration").Get<SftpConfig>();
        }

        // Makes connection with sftp server, gets files, closes connection to server. 
        public async Task<IEnumerable<SftpFile>> ListAllFilesAsync(string remoteDirectory = ".")
        {
            using var client = new SftpClient(_config.Host, _config.Port == 0 ? 22 : _config.Port, _config.Username, _config.Password);
            try
            {
                client.Connect();
                var files = await GetAllFilesAsync(client, remoteDirectory, new List<SftpFile>());
                return files;
            }
            catch (Exception exception)
            {
                _logger.LogError($"Failed in listing files under [{remoteDirectory}], message: {exception.Message}");
                return null;
            }
            finally
            {
                client.Disconnect();
            }
        }

        // Recursively gets all files in all directories
        private async Task<List<SftpFile>> GetAllFilesAsync(SftpClient sftpClient, string directory, List<SftpFile> files)
        {
            var allDirectories = await sftpClient.ListDirectoryAsync(directory);

            foreach (SftpFile sftpFile in allDirectories)
            {
                if (sftpFile.Name.StartsWith('.')) { continue; }

                if (sftpFile.IsDirectory)
                {
                    await GetAllFilesAsync(sftpClient, sftpFile.FullName, files);
                }
                else
                {
                    files.Add(sftpFile);
                }
            }
            return files;
        }

        // Connects to server, downloads file to local path, disconnects from server
        public async Task DownloadFileAsync(string remoteFilePath, string localFilePath)
        {
            using var client = new SftpClient(_config.Host, _config.Port == 0 ? 22 : _config.Port, _config.Username, _config.Password);
            try
            {
                client.Connect();
                using var s = File.Create(localFilePath);
                await client.DownloadAsync(remoteFilePath, s);
                _logger.LogInfo($"Finished downloading file [{localFilePath}] from [{remoteFilePath}]");
            }
            catch (Exception exception)
            {
                _logger.LogError($"Failed in downloading file [{localFilePath}] from [{remoteFilePath}], message: {exception.Message}");
            }
            finally
            {
                client.Disconnect();
            }
        }
    }
}

using FileCheckingService.Entities.ConfigurationModels;
using FileCheckingService.Entities.Models;
using FileCheckingService.Logging;
using FileCheckingService.Repository.Contract;
using FileCheckingService.Service.Contract;
using Microsoft.Extensions.Configuration;
using Renci.SshNet.Sftp;


namespace FileCheckingService.Service
{
    public class ScopedNewFilesCheckService : IScopedBackgroundService
    {
        private readonly ILoggerManager _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISftpService _sftpService;
        private readonly DownloadsConfig _downloadsConfig;

        public ScopedNewFilesCheckService(ILoggerManager logger, IUnitOfWork unitOfWork, ISftpService sftpService, IConfiguration configuration)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _sftpService = sftpService;
            _downloadsConfig = configuration.GetSection("Downloads").Get<DownloadsConfig>();
        }

        public async Task DoWorkAsync(CancellationToken stoppingToken)
        {
            _logger.LogInfo($"{nameof(ScopedNewFilesCheckService)} started working");

            try
            {
                await CheckForNewFiles();
                _logger.LogInfo($"{nameof(ScopedNewFilesCheckService)} finished working");
            }
            catch(Exception exception)
            {
                _logger.LogError($"Checking for new files has failed, will retry after selected interval", exception);
            }
        }

        // Checks if latest file in sftp server is more recent than latest file from db
        // if true proceeds to download new files
        private async Task CheckForNewFiles()
        {     
            var allFilesFromSftp = await _sftpService.ListAllFilesAsync();

            // Checks if sftp server contains any files
            if (!allFilesFromSftp.Any())
            {
                _logger.LogInfo("No files in sftp server found");
                return;
            }

            // Finds latest entries from db and sftp server
            SftpFile latestFileFromSftp = allFilesFromSftp.OrderByDescending(x => x.LastWriteTimeUtc).FirstOrDefault();
            FileEntity latestFileFromDb = await _unitOfWork.FileRepository.GetLatestFileAsync();

            // Checks if database table of files is not empty
            if (latestFileFromDb != null)
            {
                if(latestFileFromDb.DateCreated < latestFileFromSftp.LastWriteTimeUtc)
                {
                    // New or updated files found, generate a list of new files and download
                    IEnumerable<SftpFile> newFiles = NewOrUpdatedFiles(allFilesFromSftp, latestFileFromDb.DateCreated);
                    _logger.LogInfo("New files found on sftp server");
                    await DownloadAndSaveFiles(newFiles);
                }
                else
                {
                    _logger.LogInfo("No new files found on sftp server");
                }
            }
            else
            {
                // Download all files
                await DownloadAndSaveFiles(allFilesFromSftp);
            }
        }

        // Returns a list populated with new/updated files in sftp server 
        private IEnumerable<SftpFile> NewOrUpdatedFiles(IEnumerable<SftpFile> allFilesInSftpServer, DateTime latestRecordInDbCreationTime)
        {
            var newFiles = allFilesInSftpServer.Where(x => x.LastWriteTimeUtc > latestRecordInDbCreationTime);
            return newFiles;
        }

        // Downloads and saves to db all files from a list
        private async Task DownloadAndSaveFiles(IEnumerable<SftpFile> files)
        {
            foreach (SftpFile file in files)
            {
                await DownloadFileAndSavePathToDb(file);
            }
        }

        // Downloads file to local path, saves the path to db
        private async Task DownloadFileAndSavePathToDb(SftpFile file) 
        {      
            string fileLocalPath = _downloadsConfig.rootPath + $"{file.FullName}";
            CheckOrCreateDirectory(Path.GetDirectoryName(fileLocalPath));

            // Downloads file from sftp server to local path, overrides if old version of file is existing 
            await _sftpService.DownloadFileAsync(file.FullName, fileLocalPath);

            // Checks if file with identical path does not exist in database before creating new entry
            // This prevents duplicated entries caused by edited files in sftp server
            var existingFile = await _unitOfWork.FileRepository.GetByPath(fileLocalPath);

            if (existingFile == null)
            {
                FileEntity createdFile = new FileEntity()
                {
                    Path = fileLocalPath,
                    DateCreated = DateTime.UtcNow
                };

                _unitOfWork.FileRepository.CreateFile(createdFile);
                
                _logger.LogInfo($"File with path [{createdFile.Path}] added to database");
            }
            else
            {
                // Set new creation date for edited file path in the database
                // TODO Add new property DateEdited
                existingFile.DateCreated = DateTime.UtcNow;
                _logger.LogInfo($"Path is already in the database, only creation date was updated, file at path [{fileLocalPath}] was overriden with the new version");
            }

            await _unitOfWork.SaveAsync();
        }

        // Checks if required local directory exists, if not creates one 
        private void CheckOrCreateDirectory(string directory)
        {
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }
    }
}

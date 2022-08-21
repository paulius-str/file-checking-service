using Renci.SshNet.Sftp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileCheckingService.Service.Contract
{
    public interface ISftpService
    {
        Task<IEnumerable<SftpFile>> ListAllFilesAsync(string remoteDirectory = ".");
        Task DownloadFileAsync(string remoteFilePath, string localFilePath);
    }
}

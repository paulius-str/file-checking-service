using FileCheckingService.Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileCheckingService.Repository.Contract
{
    public interface IFileRepository
    {
        Task<IEnumerable<FileEntity>> GetAllFilesAsync();
        Task<FileEntity> GetLatestFileAsync();
        Task<FileEntity> GetByPath(string path);
        void CreateFile(FileEntity file);
    }
}

using FileCheckingService.Entities.Models;
using FileCheckingService.Repository.Contract;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileCheckingService.Repository
{
    public class FileRepository : RepositoryBase<FileEntity>, IFileRepository
    {
        public FileRepository(DatabaseContext databaseContext) : base(databaseContext)
        {

        }

        public void CreateFile(FileEntity file)
        {
            Create(file);
        }

        public async Task<IEnumerable<FileEntity>> GetAllFilesAsync()
        {
            return await GetAll().ToListAsync();
        }

        public async Task<FileEntity> GetByPath(string path)
        {
            return await GetByCondition(x => x.Path == path).FirstOrDefaultAsync();
        }

        public async Task<FileEntity> GetLatestFileAsync()
        {     
            return await GetAll().OrderByDescending(p => p.DateCreated).FirstOrDefaultAsync();
        }
    }
}

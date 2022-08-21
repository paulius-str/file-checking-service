using FileCheckingService.Repository.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileCheckingService.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly DatabaseContext _databaseContext;
        private readonly Lazy<IFileRepository> _fileRepository;

        public UnitOfWork(DatabaseContext databaseContext)
        {
            _databaseContext = databaseContext;
            // Creates new file repository only on the first request
            _fileRepository = new Lazy<IFileRepository>(() => new FileRepository(_databaseContext));
        }

        public IFileRepository FileRepository => _fileRepository.Value;

        public async Task SaveAsync()
        {
            await _databaseContext.SaveChangesAsync();
        }
    }
}

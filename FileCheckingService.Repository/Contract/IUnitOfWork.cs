using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileCheckingService.Repository.Contract
{
    public interface IUnitOfWork
    {
        public IFileRepository FileRepository { get; }
        public Task SaveAsync();
    }
}

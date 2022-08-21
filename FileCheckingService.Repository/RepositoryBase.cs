using FileCheckingService.Repository.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace FileCheckingService.Repository
{
    public class RepositoryBase<T> : IRepositoryBase<T> where T : class
    {
        private readonly DatabaseContext _databaseContext;

        public RepositoryBase(DatabaseContext databaseContext)
        {
           _databaseContext = databaseContext;
        }

        public void Create(T entity)
        {
            _databaseContext.Set<T>().Add(entity);
        }

        public void Delete(T entity)
        {
            _databaseContext.Set<T>().Remove(entity);
        }

        public IQueryable<T> GetAll()
        {
            return _databaseContext.Set<T>();
        }

        public IQueryable<T> GetByCondition(Expression<Func<T, bool>> expression)
        {
            return _databaseContext.Set<T>().Where(expression);
        }

        public void Update(T entity)
        {
            _databaseContext.Set<T>().Update(entity);
        }
    }
}

using System.Linq.Expressions;
using Database;
using Database.Base;
using Microsoft.EntityFrameworkCore;
using Shared.Domain.Interfaces;

namespace Shared.Infra
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly DbFactory _dbFactory;
        private DbSet<T> _dbSet;

        protected DbSet<T> DbSet
        {
            get => _dbSet ?? (_dbSet = _dbFactory.DbContext.Set<T>());
        }

        public Repository(DbFactory dbFactory, DbSet<T> dbSet)
        {
            _dbFactory = dbFactory;
            _dbSet = dbSet;
        }

        public void Add(T entity)
        {
            DbSet.Add(entity);
            _dbFactory.DbContext.SaveChanges();
        }

        public void Delete(T entity)
        {
            DbSet.Remove(entity);
            _dbFactory.DbContext.SaveChanges();
        }

        public void Update(T entity)
        {
            DbSet.Update(entity);
            _dbFactory.DbContext.SaveChanges();
        }

        public IQueryable<T> List(Expression<Func<T, bool>> expression)
        {
            return DbSet.Where(expression);
        }
    }
}

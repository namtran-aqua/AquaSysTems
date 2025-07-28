using AquaSolution.Data.Connection;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace AquaSolution.Data.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly AquaDbContext _context;
        private readonly DbSet<T> _dbSet;

        public Repository(AquaDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        public async Task<T?> GetByIdAsync(Guid id) => await _dbSet.FindAsync(id);

        public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.FirstOrDefaultAsync(predicate);
        }
        public Task<IQueryable<T>> GetQueryableAsync()
        {
            return Task.FromResult(_dbSet.AsQueryable());
        }
        public async Task<List<T>> WhereAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }
        public async Task<TResult?> GetMaxAsync<TResult>(Expression<Func<T, TResult>> selector)
        {
            return await _context.Set<T>().MaxAsync(selector);
        }
        public async Task<List<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
            => await _dbSet.Where(predicate).ToListAsync();

        public async Task InsertAsync(T entity) => await _dbSet.AddAsync(entity);
        public async Task AddRangeAsync(IEnumerable<T> entities) => await _dbSet.AddRangeAsync(entities);
        public void RemoveRange(IEnumerable<T> entities) => _dbSet.RemoveRange(entities);

        public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
            => await _dbSet.AnyAsync(predicate);

        public async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null)
            => predicate == null ? await _dbSet.CountAsync() : await _dbSet.CountAsync(predicate);

        public IQueryable<T> Query() => _dbSet.AsQueryable();
        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
        public async Task<bool> DeleteAsync(T entity)
        {
            if (entity == null)
                return false;

            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<bool> UpdateAsync(T entity)
        {
            try
            {
                _dbSet.Update(entity); 
                var result = await SaveChangesAsync(); 
                return result > 0;
            }
            catch
            {
                return false;
            }
        }
    }
}

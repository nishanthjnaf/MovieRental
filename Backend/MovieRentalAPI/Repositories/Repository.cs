using Microsoft.EntityFrameworkCore;
using MovieRentalAPI.Interfaces;
using MovieRentalModels;
using System.Linq.Expressions;

namespace MovieRentalAPI.Repositories
{
    public class Repository<K, T> : IRepository<K, T> where T : class
    {
        protected readonly MovieRentalContext _context;

        public Repository(MovieRentalContext context)
        {
            _context = context;
        }
        public async Task<T?> Add(T item)
        {
            _context.Add(item);
            await _context.SaveChangesAsync();
            return item;
        }

        public async Task<T?> Delete(K key)
        {
            var item = await Get(key);
            if (item != null)
            {
                _context.Remove(item);
                await _context.SaveChangesAsync();
                return item;
            }
            return null;
        }

        public async Task<T?> Get(K key)
        {
            var item = await _context.FindAsync<T>(key);
            return item != null ? item : null;
        }

        public async Task<IEnumerable<T>?> GetAll()
        {
            var items = await _context.Set<T>().AsNoTracking().ToListAsync();
            if (items.Any())
                return items;
            return null;
        }

        public void SaveChanges()
        {
            _context.SaveChanges();
        }

        public async Task<T?> Update(K key, T item)
        {
            var existingItem = await Get(key);
            if (existingItem != null)
            {
                _context.Entry(existingItem).CurrentValues.SetValues(item);
                await _context.SaveChangesAsync();
                return existingItem;
            }
            return null;
        }
        public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
        {
            return await _context.Set<T>().FirstOrDefaultAsync(predicate);
        }
        public async Task<IEnumerable<T>> GetAllIncluding(params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _context.Set<T>().AsNoTracking();

            foreach (var include in includes)
            {
                query = query.Include(include);
            }

            return await query.ToListAsync();
        }
        public async Task<T?> GetIncluding(int id, params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _context.Set<T>();

            foreach (var include in includes)
            {
                query = query.Include(include);
            }

            return await query.FirstOrDefaultAsync(e => EF.Property<int>(e, "Id") == id);
        }
        public async Task<T?> GetWithInclude(
            K key,
            params Expression<Func<T, object>>[] includes)
                {
                    IQueryable<T> query = _context.Set<T>();

                    foreach (var include in includes)
                        query = query.Include(include);

                    return await query.FirstOrDefaultAsync(e => EF.Property<K>(e, "Id").Equals(key));
                }
    }
}
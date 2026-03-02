using System.Linq.Expressions;

namespace MovieRentalAPI.Interfaces
{
    public interface IRepository<K, T> where T : class
    {
        Task<T?> Get(K key);
        Task<IEnumerable<T>?> GetAll();
        Task<T?> Add(T item);
        Task<T?> Update(K key, T item);
        Task<T?> Delete(K key);
        void SaveChanges();
        Task<T?> GetWithInclude(
            K key,
            params Expression<Func<T, object>>[] includes
        );
    }
}


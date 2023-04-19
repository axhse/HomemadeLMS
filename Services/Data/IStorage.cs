using System.Linq.Expressions;

namespace HomemadeLMS.Services.Data
{
    public interface IStorage<Tkey, TValue> where TValue : class
    {
        Task<TValue?> Find(Tkey key);

        Task<bool> HasKey(Tkey key);

        Task<IEnumerable<TValue>> Select(Expression<Func<TValue, bool>> selector);

        Task<bool> TryDelete(Tkey key);

        Task<bool> TryInsert(TValue value);

        Task<bool> TryInsert(Tkey key, TValue value);

        Task Update(TValue value);

        Task Update(Tkey key, TValue value);
    }
}
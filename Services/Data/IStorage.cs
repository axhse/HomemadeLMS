using System.Linq.Expressions;

namespace HomemadeLMS.Services.Data
{
    public interface IStorage<Tkey, TValue> where TValue : class
    {
        Task<TValue?> Find(Tkey key);

        Task<bool> HasKey(Tkey key);

        Task<List<TValue>> Select(Expression<Func<TValue, bool>> selector);

        Task<bool> TryDelete(TValue value);

        Task<bool> TryInsert(TValue value);

        Task Update(TValue value);
    }
}
using System.Linq.Expressions;

namespace HomemadeLMS.Services.Data
{
    public interface IStorage<Tkey, TValue> where TValue : class
    {
        TValue? Find(Tkey key);

        bool HasKey(Tkey key);

        IEnumerable<TValue> Select(Expression<Func<TValue, bool>> selector);

        bool TryDelete(Tkey key);

        bool TryInsert(TValue value);

        bool TryInsert(Tkey key, TValue value);

        void Update(TValue value);

        void Update(Tkey key, TValue value);
    }
}
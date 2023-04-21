using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace HomemadeLMS.Services.Data
{
    public class StorageException : Exception
    {
        public StorageException(Exception innerException) : base(string.Empty, innerException)
        { }
    }

    public class Storage<TPrimaryKey, TEntity> : IStorage<TPrimaryKey, TEntity>
        where TEntity : class
    {
        private readonly GenericContext<TEntity> context;

        public Storage(GenericContext<TEntity> context)
        {
            this.context = context;
        }

        public async Task<TEntity?> Find(TPrimaryKey key)
        {
            if (key is null)
            {
                return null;
            }
            try
            {
                return await context.Items.FindAsync(key);
            }
            catch (Exception exception)
            {
                throw new StorageException(exception);
            }
        }

        public async Task<bool> HasKey(TPrimaryKey key)
        {
            if (key is null)
            {
                return false;
            }
            try
            {
                return await Find(key) is not null;
            }
            catch (Exception exception)
            {
                throw new StorageException(exception);
            }
        }

        public Task<IEnumerable<TEntity>> Select(Expression<Func<TEntity, bool>> selector)
        {
            try
            {
                return Task.FromResult(context.Items.Where(selector).AsEnumerable());
            }
            catch (Exception exception)
            {
                return Task.FromException<IEnumerable<TEntity>>(new StorageException(exception));
            }
        }

        public async Task<bool> TryDelete(TPrimaryKey key)
        {
            if (key is null)
            {
                return false;
            }
            try
            {
                TEntity? entity = context.Items.Find(key);
                if (entity is null)
                {
                    return false;
                }
                context.Remove(entity);
                await context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                return false;
            }
            catch (Exception exception)
            {
                throw new StorageException(exception);
            }
        }

        public async Task<bool> TryInsert(TEntity entity)
        {
            try
            {
                await context.AddAsync(entity);
                await context.SaveChangesAsync();
                return true;
            }
            catch (InvalidOperationException)
            {
                return false;
            }
            catch (Exception exception)
            {
                throw new StorageException(exception);
            }
        }

        public async Task<bool> TryInsert(TPrimaryKey key, TEntity entity)
        {
            return await TryInsert(entity);
        }

        public async Task Update(TEntity entity)
        {
            try
            {
                context.Update(entity);
                await context.SaveChangesAsync();
            }
            catch (Exception exception)
            {
                throw new StorageException(exception);
            }
        }

        public async Task Update(TPrimaryKey key, TEntity entity)
        {
            await Update(entity);
        }
    }
}
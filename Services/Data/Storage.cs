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

        public Task<List<TEntity>> Select(Expression<Func<TEntity, bool>> selector)
        {
            if (selector is null)
            {
                return Task.FromResult(new List<TEntity>());
            }
            try
            {
                return Task.FromResult(context.Items.Where(selector).ToList());
            }
            catch (Exception exception)
            {
                return Task.FromException<List<TEntity>>(new StorageException(exception));
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
                TEntity? entity = await context.Items.FindAsync(key);
                if (entity is null)
                {
                    return false;
                }
                return await TryDeleteValue(entity);
            }
            catch (Exception exception)
            {
                throw new StorageException(exception);
            }
        }

        public async Task<bool> TryDeleteValue(TEntity entity)
        {
            if (entity is null)
            {
                return false;
            }
            try
            {
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
            if (entity is null)
            {
                return false;
            }
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
            if (entity is null)
            {
                return;
            }
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
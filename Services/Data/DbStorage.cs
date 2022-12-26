using System.Linq.Expressions;

namespace HomemadeLMS.Services.Data
{
    public class DbStorage<TPrimaryKey, TEntity> : IStorage<TPrimaryKey, TEntity>
        where TEntity : class
    {
        private readonly DbClient<TEntity> dbClient;

        public DbStorage(DbClient<TEntity> dbClient)
        {
            this.dbClient = dbClient;
        }

        public TEntity? Find(TPrimaryKey key)
        {
            if (key is null)
            {
                return null;
            }
            try
            {
                return dbClient.Items.Find(key);
            }
            catch (Exception exception)
            {
                throw;
                // TODO: throw standart exception + documment it
            }
        }

        public bool HasKey(TPrimaryKey key)
        {
            if (key is null)
            {
                return false;
            }
            try
            {
                return dbClient.Items.Find(key) is not null;
            }
            catch (Exception exception)
            {
                throw;
                // TODO: throw standart exception + documment it
            }
        }

        public IEnumerable<TEntity> Select(
            Expression<Func<TEntity, bool>> selector)
        {
            try
            {
                return dbClient.Items.Where(selector).AsEnumerable();
            }
            catch (Exception exception)
            {
                throw;
                // TODO: throw standart exception + documment it
            }
        }

        public bool TryDelete(TPrimaryKey key)
        {
            if (key is null)
            {
                return false;
            }
            try
            {
                TEntity? entity = dbClient.Items.Find(key);
                if (entity is null)
                {
                    return false;
                }
                dbClient.Remove(entity);
                dbClient.SaveChanges();
                return true;
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException)
            {
                return false;
            }
            catch (Exception exception)
            {
                throw;
                // TODO: throw standart exception + documment it
            }
        }

        public bool TryInsert(TEntity entity)
        {
            try
            {
                dbClient.Add(entity);
                dbClient.SaveChanges();
                return true;
            }
            catch (InvalidOperationException)
            {
                return false;
            }
            catch (Exception exception)
            {
                throw;
                // TODO: throw standart exception + documment it
            }
        }

        public bool TryInsert(TPrimaryKey key, TEntity entity)
        {
            return TryInsert(entity);
        }

        public void Update(TEntity entity)
        {
            try
            {
                dbClient.Update(entity);
                dbClient.SaveChanges();
            }
            catch (Exception exception)
            {
                throw;
                // TODO: throw standart exception + documment it
            }
        }

        public void Update(TPrimaryKey key, TEntity entity)
        {
            Update(entity);
        }
    }
}
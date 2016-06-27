using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xugl.ImmediatelyChat.Core
{
    public interface IDbContext
    {
        IDbSet<TEntity> Set<TEntity>() where TEntity : class;

        int SaveChanges();

        IList<TEntity> ExecuteStoredProduceList<TEntity>(string commandText, Func<TEntity, bool> autoDetectPredicate, params object[] parameters) where TEntity : class,new();

        IEnumerable<TElement> SqlQuery<TElement>(string sql, params object[] parameters);

        int ExecuteSqlCommand(string sql, bool doNotEnsureTrancation = false, int? timeout = null, params object[] parameters);

        DbEntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class;
             
    }
}

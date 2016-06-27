using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Xugl.ImmediatelyChat.Core
{
    public interface IRepository<T>
    {
        T Find(params object[] keyValues);

        T Find(Expression<Func<T, bool>> predicate);

        int Insert(T TEntity);

        int BatchInsert(IList<T> entitys);

        int Update(T TEntity);

        int BatchUpdate(IList<T> entitys);

        int Delete(T TEntity);

        int BatchDelete(IList<T> entitys);

        IQueryable<T> Table { get; }

        //IQueryable<T> TableNoTracking { get; }

    }
}

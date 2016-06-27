using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Xugl.ImmediatelyChat.Model.QueryCondition;

namespace Xugl.ImmediatelyChat.Model.ViewModel
{
    public class PagingDataSource<T>:PagingModel
    {
        public IList<T> DataSource { get; set; }

        public PagingDataSource(IList<T> dataSource,int pageIndex,int pageSize,int recordCount=0):base(pageIndex,pageSize,recordCount)
        {
            DataSource = dataSource;
        }

        public PagingDataSource(IList<T> dataSource,PagingModel pagingModel,int recordCount=0):
            this(dataSource,pagingModel.PageIndex,pagingModel.PageSize,recordCount)
        {

        }
    }

    public static class PagingDataSourceExtension
    {
        public static PagingDataSource<T> ToPagingDataSource<T>(IList<T> dataSource,PagingQuery pagingQuery,int recordCount=0)
        {
            return new PagingDataSource<T>(dataSource, pagingQuery.Page, pagingQuery.PageSize, recordCount);
        }

        public static PagingDataSource<T> TOpagingDataSource<T>(IList<T> dataSource,int pageIndex,int pageSize,int recordCount=0)
        {
            return new PagingDataSource<T>(dataSource, pageIndex, pageSize, recordCount);
        }

        public static PagingDataSource<T> ToPagingDataSource<T>(this IQueryable<T> queryableSource,PagingQuery pagingQuery,int recordCount=0)
        {
            if(recordCount==0 && pagingQuery.IsGetRecordCount)
            {
                recordCount = queryableSource.Count();
            }
            return ToPagingDataSource(queryableSource.Skip(pagingQuery.GetSkipCount()).Take(pagingQuery.PageSize).ToList(), pagingQuery, recordCount);
        }

        public static PagingDataSource<T> ToPagingDataSource<T>(this IQueryable<T> queryableSource,OrderPagingQuery<T> orderPagingQuery,int recordCount=0) where T:class
        {
            if(recordCount==0 && orderPagingQuery.IsGetRecordCount)
            {
                recordCount = queryableSource.Count();
            }

            bool ordered = false;
            foreach(OrderField<T> orderfield in orderPagingQuery.OrderFieldStore.OrderFields)
            {
                string orderType=string.Empty;
                if(orderfield.OrderType==OrderType.Descending)
                {
                    orderType = ordered ? "ThenByDescending" : "OrderByDescending";
                }
                else
                {
                    orderType = ordered ? "ThenBy" : "OrderBy";
                }

                if(!string.IsNullOrEmpty(orderType))
                {
                    Expression sortExpression = UnBox(orderfield.FieldExpression.Body);
                    Type sortType = sortExpression.Type;
                    Type funType = typeof(Func<,>);

                    funType = funType.MakeGenericType(typeof(T), sortType);

                    LambdaExpression lambda = Expression.Lambda(funType, sortExpression, orderfield.FieldExpression.Parameters);

                    MethodCallExpression method = Expression.Call(typeof(Queryable), orderType, 
                        new Type[] { typeof(T), sortExpression.Type }, queryableSource.Expression, lambda);

                    queryableSource = queryableSource.Provider.CreateQuery<T>(method);
                    ordered = true;
                }
            }
            return ToPagingDataSource(queryableSource.Skip(orderPagingQuery.GetSkipCount()).Take(orderPagingQuery.PageSize).ToList(),
                orderPagingQuery, recordCount);
        }

        private static Expression UnBox(Expression expression)
        {
            Expression result;
            UnaryExpression unaryExpression = expression as UnaryExpression;

            if(unaryExpression==null)
            {
                result = expression;
            }
            else
            {
                result = UnBox(unaryExpression.Operand);
            }
            return result;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Xugl.ImmediatelyChat.Model.QueryCondition
{
    public static class OrderKeyParser
    {
        public static string ParseOrderKey<T>(Expression<Func<T,object>> fieldExpression) where T:class
        {
            MemberExpression memberExpression;
            var operation = fieldExpression.Body as UnaryExpression;
            if(operation!=null)
            {
                memberExpression = operation.Operand as MemberExpression;
            }
            else
            {
                memberExpression = fieldExpression.Body as MemberExpression;
            }

            if(memberExpression ==null)
            {
                throw new ArgumentException("please use attribute expression", "fieldExpression");
            }
            return memberExpression.Member.Name;
        }
    }

    public class OrderField<TModel> where TModel : class
    {
        public OrderField(Expression<Func<TModel,object>> fieldExpression,OrderType orderType=OrderType.None,int priority=0,string fieldText="")
        {
            FieldExpression = fieldExpression;
            OrderType = orderType;
            Priority = priority;
            OriginalPriority = priority;
            FieldText = fieldText;
        }

        public Expression<Func<TModel, object>> FieldExpression { get; set; }

        private readonly string _orderkey;

        public OrderType OrderType { get; set; }

        public int Priority { get; set; }

        public int OriginalPriority { get; set; }

        public string FieldText { get; set; }
    }

    public class OrderFieldStroe<TModel> where TModel:class
    {
        private IList<OrderField<TModel>> _orderFields;

        public IList<OrderField<TModel>> OrderFields
        {
            get { return _orderFields; }
        }

        public OrderFieldStroe()
        {
            _orderFields = new List<OrderField<TModel>>();
        }

        public void Add(Expression<Func<TModel,object>> fieldExpression,OrderType orderType=OrderType.None)
        {
            _orderFields.Add(new OrderField<TModel>(fieldExpression, orderType, _orderFields.Count + 1));
        }
    }

    public enum OrderType
    {
        None,
        Ascending,
        Descending
    }
}

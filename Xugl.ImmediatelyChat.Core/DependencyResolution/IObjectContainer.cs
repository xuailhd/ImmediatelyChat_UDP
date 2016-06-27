using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xugl.ImmediatelyChat.Core.DependencyResolution
{
    public interface IObjectContainer
    {
        void Initialize();

        T Resolver<T>();

        object Resolver(Type modelType);

        IList<object> ResolverAll(Type modelType);

    }
}

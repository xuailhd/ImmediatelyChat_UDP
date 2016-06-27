using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Xugl.ImmediatelyChat.Core.DependencyResolution
{
    /// <summary>
    /// Single case of IObjectContainer
    /// </summary>
    public class ObjectContainerFactory
    {
        public static IObjectContainer CurrentContainer
        {
            get
            {
                if (Singleton<IObjectContainer>.Instance==null)
                    InitContainter();
                return Singleton<IObjectContainer>.Instance;
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static IObjectContainer InitContainter()
        {
            if(Singleton<IObjectContainer>.Instance==null)
            {
                Singleton<IObjectContainer>.Instance = new StructureMapContainer();
                Singleton<IObjectContainer>.Instance.Initialize();
            }

            return Singleton<IObjectContainer>.Instance;
        }
    }
}

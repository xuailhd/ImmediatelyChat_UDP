using StructureMap;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xugl.ImmediatelyChat.Core.DependencyResolution
{
    public class StructureMapContainer:IObjectContainer
    {
        private static bool _den_dependenciesRegistered;
        private static readonly object _lockObject = new object();

        private static Lazy<Container> _containerBuilder;

        public IContainer Container
        {
            get { return _containerBuilder.Value; }
        }

        public void Initialize()
        {
            EnsuerDependencisesRegistered();
        }

        private static void nameContainer(IContainer container)
        {
            container.Name = "Xugl.StructureMapContainer-" + container.Name;
        }

        private void RegisterDenpendecies()
        {
            //设置要扫描依赖注入的程序集
            string assembleSuffix = ConfigurationManager.AppSettings["ormAssembleSuffix"];
            string[] scanAssembleSuffix = string.IsNullOrWhiteSpace(assembleSuffix) ? null : assembleSuffix.Split(',');

            if (scanAssembleSuffix == null || scanAssembleSuffix.Length == 0)
                return;

            //Container.Configure(expression =>
            var container =new Container(x=>
            {
                x.Scan(assembleScanner =>
                    {
                        if (scanAssembleSuffix != null)
                        {
                            Array.ForEach(scanAssembleSuffix, suffix => assembleScanner.Assembly(suffix.Trim()));

                            // 扫描指定程序集，查找所有实现了Registry类的实现。
                            assembleScanner.LookForRegistries();

                            //扫描指定的程序集，查找符合 ISomething - Something 规则的接口和实现类.
                            assembleScanner.WithDefaultConventions();
                        }
                    });
            });

            _containerBuilder=new Lazy<Container>(()=>container);
            nameContainer(container);
        }

        private void EnsuerDependencisesRegistered()
        {
            if(!_den_dependenciesRegistered)
            {
                lock(_lockObject)
                {
                    if(!_den_dependenciesRegistered)
                    {
                        RegisterDenpendecies();
                    }
                }
            }

        }






        public T Resolver<T>()
        {
            EnsuerDependencisesRegistered();
            return Container.GetInstance<T>();
        }

        public object Resolver(Type modelType)
        {
            EnsuerDependencisesRegistered();
            return Container.GetInstance(modelType);
        }

        public IList<object> ResolverAll(Type modelType)
        {
            EnsuerDependencisesRegistered();
            return (List<object>)Container.GetAllInstances(modelType);
        }
    }
}

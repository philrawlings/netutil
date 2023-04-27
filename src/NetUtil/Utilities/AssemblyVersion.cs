using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NetUtil.Utilities
{
    internal sealed class AssemblyVersion
    {
        private readonly object _dictLocker;
        private IDictionary<Type, string> _versionDictionary;

        static readonly AssemblyVersion _instance = new AssemblyVersion();

        public static AssemblyVersion Instance
        {
            get { return _instance; }
        }

        private AssemblyVersion()
        {
            _dictLocker = new object();
            _versionDictionary = new Dictionary<Type, string>();
        }

        public string Get(Type type)
        {
            lock (_dictLocker)
            {
                if (_versionDictionary.ContainsKey(type))
                    return _versionDictionary[type];
                else
                {
                    var version = Assembly.GetAssembly(type).GetName().Version;
                    var versionStr = string.Format("{0}.{1}.{2}", version.Major, version.Minor, version.Build);
                    _versionDictionary.Add(type, versionStr);
                    return versionStr;
                }
            }
        }
    }
}

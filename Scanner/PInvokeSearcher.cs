using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;

using Mono.Cecil;

namespace Blackduck.Hub
{
    sealed  class PInvokeSearcher
    {
        public IEnumerable<String> findPInvokePaths(Assembly assebmly)
        {
            List<String> result = new List<String>();
            foreach (FileStream file in assebmly.GetFiles())
            {
                ModuleDefinition module = ModuleDefinition.ReadModule(file);
                var methods = module.GetTypes().SelectMany(t => t.Methods).Where(method => method.HasPInvokeInfo);

                foreach (var method in methods)
                {
                    ModuleReference reference = method.PInvokeInfo.Module;
                    result.Add(reference.Name);
                }
            }
            return result;
        }
    }
}

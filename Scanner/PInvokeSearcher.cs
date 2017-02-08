using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;

using Mono.Cecil;
using System.Diagnostics;

namespace Blackduck.Hub
{
    static class PInvokeSearcher
    {
        public sealed class PInvokeSearchResult
        {
            public IEnumerable<string> FoundPaths { get; private set; }
            public IEnumerable<string> NotFoundDllNames { get; private set; }

            public PInvokeSearchResult(ISet<string> foundPaths, ISet<string> notFoundDllNames)
            {
                FoundPaths = foundPaths;
                NotFoundDllNames = notFoundDllNames;
            }

        }


        /// <summary>
        /// Finds all the native libraries referenced from the provided assembly
        /// </summary>
        /// <param name="assebmly"></param>
        /// <returns></returns>
        public static PInvokeSearchResult findPInvokePaths(Assembly assembly)
        {
            var found = new HashSet<string>();
            var notFound = new HashSet<string>();
            foreach (FileStream file in assembly.GetFiles())
            {
                ModuleDefinition module = ModuleDefinition.ReadModule(file);
                var methods = module.GetTypes().SelectMany(t => t.Methods).Where(method => method.HasPInvokeInfo);

                foreach (var method in methods ?? Enumerable.Empty<MethodDefinition>())
                {
                    ModuleReference reference = method.PInvokeInfo?.Module;
                    if (reference != null)
                    {
                        String dllPath = findDll(reference.Name, Path.GetFullPath(assembly.Location));
                        if (!string.IsNullOrWhiteSpace(dllPath)) found.Add(dllPath);
                        else notFound.Add(reference.Name);
                    }


                }
            }
            return new PInvokeSearchResult(found, notFound);
        }

        /// <summary>
        /// Searches the Windows invocation path for a dynamic link library with the name <paramref name="dllName"/>.
        /// Attempts to find it in the same directory as the invoking assembly (<paramref name="invokerPath"/>) prior to searching system directories.
        /// </summary>
        /// <param name="dllName"></param>
        /// <param name="invokerPath"></param>
        /// <returns></returns>
        private static string findDll(string dllName, string invokerPath)
        {
            string localPath = Path.Combine(Path.GetDirectoryName(invokerPath), dllName);
            if (File.Exists(localPath)) return localPath;

            string systemPath = Path.Combine(Environment.SystemDirectory, dllName);
            if (File.Exists(systemPath)) return systemPath;

            //On Windows, we might also check the Windows directory
            string winDir = Environment.GetEnvironmentVariable("windir");
            if (!string.IsNullOrWhiteSpace(winDir))
            {
                string winDirPath = Path.Combine(winDir, dllName);
                if (File.Exists(winDirPath)) return winDirPath;
            }

            return null;
        }
    }
}

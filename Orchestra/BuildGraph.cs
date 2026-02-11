// Copyright (C) 2026 ychgen, all rights reserved.

using Moonquake.CodeGen;

namespace Moonquake.Orchestra
{
    public class BuildGraph
    {
        /// <summary>
        /// Topologically sorted from given root's modules by using relevant modules' dependencies.
        /// </summary>
        public List<BuildModule> Modules { get; }

        public BuildGraph(BuildRoot InRoot)
        {
            Modules = TopoSort(InRoot.Modules.Values);
        }

        public void Build()
        {
            foreach (BuildModule Module in Modules)
            {
                Build(Module);
            }
        }

        private void Build(BuildModule Module)
        {
            Console.WriteLine($"[MQ] Building module '{Module.Name}'...");
            if (!Directory.Exists(Module.ObjectPath))
            {
                Directory.CreateDirectory(Module.ObjectPath);
            }
            List<string> ObjectFiles = new();
            for (int i = 0; i < Module.TranslationUnits.Count; i++)
            {
                string TranslationUnit = Module.TranslationUnits[i];
                string ObjectFile = Path.Combine(Module.ObjectPath, Path.GetFileNameWithoutExtension(TranslationUnit) + ".obj");
                ObjectFiles.Add(ObjectFile);

                Console.WriteLine($"[{i+1}/{Module.TranslationUnits.Count}] {Path.GetFileName(TranslationUnit)}");
                if (!Backend.Instance.InvokeCompiler(TranslationUnit, ObjectFile, Module))
                {
                    throw new Exception($"Building of module '{Module.Name}' failed, compiler compilation error.");
                }
            }
            Console.WriteLine($"[MQ] Linking final {(Module.OutputType == ModuleOutputType.ConsoleExecutable || Module.OutputType == ModuleOutputType.WindowedExecutable ? "executable" : "binary")}...");
            if (!Backend.Instance.InvokeLinker(ObjectFiles, Module))
            {
                throw new Exception($"Building of module '{Module.Name}' failed, linker linkage error.");
            }
        }
        
        public static List<BuildModule> TopoSort(IEnumerable<BuildModule> Modules)
        {
            List<BuildModule>    Sorted   = new();
            HashSet<BuildModule> Visited  = new();
            HashSet<BuildModule> Visiting = new();

            void Visit(BuildModule Mod)
            {
                if (Visited.Contains(Mod))
                {
                    return;
                }
                if (Visiting.Contains(Mod))
                {
                    throw new Exception($"Cycle detected at module '{Mod.Name}' (shouldn't happen as FinalizeModule already checks, so given Modules must be invalid).");
                }

                Visiting.Add(Mod);
                foreach (BuildModule Dep in Mod.Dependencies.Values)
                {
                    Visit(Dep);
                }
                Visiting.Remove(Mod);
                
                Visited.Add(Mod);
                Sorted.Add(Mod);
            }

            foreach (BuildModule Mod in Modules)
            {
                Visit(Mod);
            }

            return Sorted;
        }
    }
}

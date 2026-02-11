// Copyright (C) 2026 ychgen, all rights reserved.

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
            Console.WriteLine($"[Moonquake] Building module '{Module.Name}'...");
            for (int i = 0; i < Module.TranslationUnits.Count; i++)
            {
                string TranslationUnit = Module.TranslationUnits[i];
                Console.WriteLine($"[{i+1} of {Module.TranslationUnits.Count}] {Path.GetFileName(TranslationUnit)}");
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

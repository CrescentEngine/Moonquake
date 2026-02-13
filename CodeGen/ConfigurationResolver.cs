// Copyright (C) 2026 ychgen, all rights reserved.

using Moonquake.DSL;
using Moonquake.DSL.Constructs;
using Moonquake.Orchestra;

namespace Moonquake.CodeGen
{
    public struct TestBuildConfig
    {
        public string Config;
        public Architectures Arch;
    }
    public class ConfigurationResolver
    {
        public string Filepath { get; }
        private CompoundAST RootNode;

        public ConfigurationResolver(string InFilepath, CompoundAST InRootNode)
        {
            Filepath = InFilepath;
            RootNode = (CompoundAST) InRootNode.Clone();
        }

        public Dictionary<TestBuildConfig, BuildRoot> ResolveForAll(DSL.ExecutionContext Context, Root CanonicalRoot)
        {
            Dictionary<TestBuildConfig, BuildRoot> All = new();
            foreach (string Config in CanonicalRoot.Arr(RootFieldNames.CONFIGS))
            {
                foreach (Architectures Arch in Enum.GetValues<Architectures>())
                {
                    TestBuildConfig Cfg = new TestBuildConfig
                    {
                        Config = Config,
                        Arch = Arch
                    };
                    if (All.ContainsKey(Cfg))
                    {
                        continue;
                    }
                    BuildOrder Order = Moonquake.BuildOrder.Clone();
                    Order.Configuration = Cfg.Config;
                    Order.Architecture = Cfg.Arch;
                    All[Cfg] = ResolveFor(CanonicalRoot.Name, Order);
                }
            }
            
            // All root resolutions must agree on the following fields:
            //   CONFIGS, ARCHS, PLATFORMS
            // They can diverge on others, such as MODULES and ENTRYMOD(dubious support on VS).
            // All common modules of the root must agree on canonical field:
            //   ORIGIN
            string[] CanonicalConfigurations = [];
            Architectures[] CanonicalArchitectures = [];
            Platforms[] CanonicalPlatforms = [];

            bool bFirstRun = true;
            Dictionary<string, string> ModOrigins = new();
            foreach (var Resolution in All)
            {
                TestBuildConfig Cfg = Resolution.Key;
                BuildRoot Root = Resolution.Value;

                if (bFirstRun)
                {
                    CanonicalConfigurations = Root.Configurations;
                    CanonicalArchitectures  = Root.Architectures;
                    CanonicalPlatforms      = Root.Platforms;
                    bFirstRun = false;
                }
                else
                {
                    if (!Root.Configurations.SequenceEqual(CanonicalConfigurations))
                    {
                        throw new Exception($"ConfigurationResolver.ResolveForAll() error: Root '{Root.Name}' has no canonical '{RootFieldNames.CONFIGS}' field. Each resolution of the root under all possible build orders must have the same exact layout of this field.");
                    }
                    if (!Root.Architectures.SequenceEqual(CanonicalArchitectures))
                    {
                        throw new Exception($"ConfigurationResolver.ResolveForAll() error: Root '{Root.Name}' has no canonical '{RootFieldNames.ARCHS}' field. Each resolution of the root under all possible build orders must have the same exact layout of this field.");
                    }
                    if (!Root.Platforms.SequenceEqual(CanonicalPlatforms))
                    {
                        throw new Exception($"ConfigurationResolver.ResolveForAll() error: Root '{Root.Name}' has no canonical '{RootFieldNames.PLATFORMS}' field. Each resolution of the root under all possible build orders must have the same exact layout of this field.");
                    }
                }

                foreach (BuildModule Mod in Root.Modules.Values)
                {
                    string? Origin;
                    if (ModOrigins.TryGetValue(Mod.Name, out Origin))
                    {
                        if (Origin != Mod.ModulePath)
                        {
                            throw new Exception($"Module '{Mod.Name}' of root '{Root.Name}' has different module origin paths (module construct field name '{ModuleFieldNames.ORIGIN}') based on different build resolutions. Module origins must always be the same no matter the execution environment.");
                        }
                    }
                    else
                    {
                        ModOrigins[Mod.Name] = Mod.ModulePath;
                    }
                }
            }

            return All;
        }
        private BuildRoot ResolveFor(string InRootName, BuildOrder Order)
        {
            BuildOrder PrevBuildOrder = Moonquake.BuildOrder;
            Moonquake.BuildOrder = Order;

            // Clear pattern cache, important
            // If we don't do ts we'll get wrong resolutions
            // TODO/FIXME: Perhaps move pattern maching cache to per-Visitor so this is never an issue. Debatable
            StringUtil.ClearPatternCache();

            CompoundAST UniqueRootNode = (CompoundAST) RootNode.Clone();
            Visitor Vis = new Visitor(Filepath);
            Vis.Visit(UniqueRootNode);

            BuildRoot Final;
            try
            {
                Final = BuildFinalizer.FinalizeRoot(Vis.Context, InRootName);
            }
            catch (Exception e)
            {
                Console.WriteLine($"ConfigurationResolver.ResolveFor() failed: Couldn't finalize root '{InRootName}'. {e.Message}");
                throw;
            }
            
            Moonquake.BuildOrder = PrevBuildOrder;
            return Final;
        }
    }
}

// Copyright (C) 2026 ychgen, all rights reserved.

using System.Diagnostics;
using Moonquake.Orchestra;

namespace Moonquake.CodeGen
{
    public class VisualStudioBackend : Backend
    {
        public const string COMPILER_EXECUTABLE = "cl.exe";
        public const string LINKER_EXECUTABLE   = "link.exe";
        public const string ARCHIVER_EXECUTABLE = "lib.exe";

        public override Backends GetBackendType() => Backends.VisualStudio;
        private readonly Dictionary<Architectures, string> CachedToolsPaths = new();
        private string? CachedVsInstallPath = null, CachedVsCmdDevPath = null;

        public override string GetCompilerInvocation(Architectures Arch)
        {
            string VCXToolsPath = GetVCXToolsPath(Arch);
            return Path.Combine(VCXToolsPath, COMPILER_EXECUTABLE);
        }

        public override string GetLinkerInvocation(Architectures Arch)
        {
            string VCXToolsPath = GetVCXToolsPath(Arch);
            return Path.Combine(VCXToolsPath, LINKER_EXECUTABLE);
        }

        public override string GetArchiverInvocation(Architectures Arch)
        {
            return Path.Combine(GetVCXToolsPath(Arch), ARCHIVER_EXECUTABLE);
        }

        public override bool InvokeCompiler(string TranslationUnitToCompile, string ObjectFileAsToCompile, BuildModule Module)
        {
            string VsDevCmd;
            string CompilerPath;
            try
            {
                VsDevCmd = GetVsDevCmdPath();
                CompilerPath = GetCompilerInvocation(Moonquake.BuildOrder.Architecture);
            }
            catch
            {
                return false;
            }
            
            List<string> Arguments =
            [
                "/nologo",
                "/c",
                $"\"{TranslationUnitToCompile}\"",
                $"/Fo\"{ObjectFileAsToCompile}\"",
                $"/std:{ModuleLanguageStandardToClStd(Module.CppStandard)}",
                "/EHsc",
                GetModuleRuntimeLibsFlag(Module.RuntimeLibraries),
                $"/O{GetOptimizationLevel(Module.OptimizationLevel)}",
                
            ];
            if (Module.bGenerateSymbols)
            {
                Arguments.Add("/Zi");
                Arguments.Add($"/Fd{Module.ObjectPath}/{Module.Name}.pdb");
            }
            foreach (string IncludePath in Module.IncludePaths)
            {
                Arguments.Add($"/I{IncludePath}");
            }
            foreach (string Definition in Module.Definitions)
            {
                Arguments.Add($"/D{Definition}");
            }

            string CommandLine = string.Join(" ", Arguments);
            string LaunchCommand = $"\"{VsDevCmd}\" -no_logo -arch={Moonquake.BuildOrder.Architecture} && \"{CompilerPath}\" {CommandLine}";

            ProcessStartInfo CmdStartInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c \"{LaunchCommand}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using Process Proc = Process.Start(CmdStartInfo)!;
            string Stdout = Proc.StandardOutput.ReadToEnd();
            string Stderr = Proc.StandardError.ReadToEnd();
            Proc.WaitForExit();

            // cl.exe's stdout output first line always contains filename.cpp (name of TU being compiled) which we don't want so we eat that and first endl.
            int FirstNewLine = Stdout.IndexOf('\n');
            Stdout = FirstNewLine >= 0
                ? Stdout.Substring(FirstNewLine + 1)
                : Stdout;

            if (!string.IsNullOrEmpty(Stdout))
            {
                Console.Write(Stdout);
            }
            if (!string.IsNullOrEmpty(Stderr))
            {
                Console.Write(Stderr);
            }

            return Proc.ExitCode == 0;
        }

        public override bool InvokeLinker(IEnumerable<string> ObjectFiles, BuildModule Module)
        {
            string FinalBin = Path.Combine(Module.OutputPath, Module.OutputName + GetOutputTypeExt(Module.OutputType));
            if (!Directory.Exists(Module.OutputPath))
            {
                Directory.CreateDirectory(Module.OutputPath);
            }

            switch (Module.OutputType)
            {
            case ModuleOutputType.ConsoleExecutable:
            {
                return LinkWithLinkExe("/SUBSYSTEM:CONSOLE");
            }
            case ModuleOutputType.WindowedExecutable:
            {
                return LinkWithLinkExe("/SUBSYSTEM:WINDOWS");
            }
            case ModuleOutputType.StaticLibrary:
            {
                string VsDevCmd = GetVsDevCmdPath();
                string ArchiverPath = GetArchiverInvocation(Moonquake.BuildOrder.Architecture);

                List<string> Arguments = [ "/nologo", .. ObjectFiles, $"/OUT:{FinalBin}" ];
                string CommandLine = string.Join(" ", Arguments);
                string LaunchCommand = $"\"{VsDevCmd}\" -no_logo -arch={Moonquake.BuildOrder.Architecture} && \"{ArchiverPath}\" {CommandLine}";

                ProcessStartInfo CmdStartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c \"{LaunchCommand}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using Process Proc = Process.Start(CmdStartInfo)!;
                string Stdout = Proc.StandardOutput.ReadToEnd();
                string Stderr = Proc.StandardError.ReadToEnd();
                Proc.WaitForExit();

                if (!string.IsNullOrEmpty(Stdout))
                {
                    Console.Write(Stdout);
                }
                if (!string.IsNullOrEmpty(Stderr))
                {
                    Console.Write(Stderr);
                }

                return true;
            }
            case ModuleOutputType.DynamicLibrary:
            {
                return LinkWithLinkExe("/DLL");
            }
            }

            bool LinkWithLinkExe(string InSpecificFlag)
            {
                string VsDevCmd = GetVsDevCmdPath();
                string LinkerPath = GetLinkerInvocation(Moonquake.BuildOrder.Architecture);

                List<string> Linkages = new();
                foreach (BuildModule Linkage in Module.Linkages.Values)
                {
                    Linkages.Add(Path.Combine(Linkage.OutputPath, Path.Combine(Linkage.OutputName) + GetOutputTypeExt(Linkage.OutputType)));
                }

                List<string> Arguments = ["/nologo", .. ObjectFiles, .. Module.Libraries, .. Linkages, $"/OUT:{FinalBin}", InSpecificFlag];
                if (Module.bGenerateSymbols)
                {
                    Arguments.Add("/DEBUG");
                    Arguments.Add($"/PDB:{Module.ObjectPath}/{Module.Name}.pdb");
                }
                Arguments.Add(Moonquake.BuildOrder.Architecture switch
                {
                    Architectures.x86   => "/MACHINE:X86",
                    Architectures.x64   => "/MACHINE:X64",
                    Architectures.ARM64 => "/MACHINE:ARM64",
                    _ => throw new NotImplementedException()
                });

                string CommandLine = string.Join(" ", Arguments);
                string LaunchCommand = $"\"{VsDevCmd}\" -no_logo -arch={Moonquake.BuildOrder.Architecture} && \"{LinkerPath}\" {CommandLine}";

                ProcessStartInfo CmdStartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c \"{LaunchCommand}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using Process Proc = Process.Start(CmdStartInfo)!;
                string Stdout = Proc.StandardOutput.ReadToEnd();
                string Stderr = Proc.StandardError.ReadToEnd();
                Proc.WaitForExit();

                if (!string.IsNullOrEmpty(Stdout))
                {
                    Console.Write(Stdout);
                }
                if (!string.IsNullOrEmpty(Stderr))
                {
                    Console.Write(Stderr);
                }

                return true;
            }

            return false;
        }

        public string GetVSInstallPath()
        {
            if (CachedVsInstallPath is not null)
            {
                return CachedVsInstallPath;
            }

            string vswhere = @"C:\Program Files (x86)\Microsoft Visual Studio\Installer\vswhere.exe";
            if (!File.Exists(vswhere))
            {
                throw new Exception("BackendUtils.GetVCXToolsPath() error: vswhere.exe not found! Ensure you have Visual Studio installed. (https://visualstudio.microsoft.com/downloads/)");
            }
            ProcessStartInfo vswhereStartInfo = new ProcessStartInfo
            {
                FileName = vswhere,
                Arguments = "-latest -products * -requires Microsoft.VisualStudio.Component.VC.Tools.x86.x64 -property installationPath",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using Process Proc = Process.Start(vswhereStartInfo)!;
            string InstallPath = Proc.StandardOutput.ReadLine()!;
            Proc.WaitForExit();

            if (string.IsNullOrEmpty(InstallPath))
            {
                throw new Exception("No suitable Visual Studio installation found. Make sure you installed a version of Visual Studio, not just the Visual Studio Installer.");
            }

            CachedVsInstallPath = InstallPath;
            return InstallPath;
        }

        public string GetVsDevCmdPath()
        {
            if (CachedVsCmdDevPath is not null)
            {
                return CachedVsCmdDevPath;
            }
            string Cmbpath = Path.Combine(GetVSInstallPath(), "Common7", "Tools", "VsDevCmd.bat");
            if (!File.Exists(Cmbpath))
            {
                throw new Exception($"VsDevCmd.bat batch script couldn't be located! Make sure you have a version of Visual Studio installed.");
            }
            CachedVsCmdDevPath = Cmbpath;
            return CachedVsCmdDevPath;
        }
        
        public string GetVCXToolsPath(Architectures Arch)
        {
            {
                string? Cached;
                if (CachedToolsPaths.TryGetValue(Arch, out Cached))
                {
                    return Cached; 
                }
            }

            string InstallPath = GetVSInstallPath();

            string VCToolsPath = Path.Combine(InstallPath, @"VC\Tools\MSVC");
            if (!Directory.Exists(VCToolsPath))
            {
                throw new Exception("MSVCX Tools (Microsoft Visual Studio C/C++) not found. Make sure you have a compiler toolchain installed.");
            }
            string Latest = Directory.GetDirectories(VCToolsPath).OrderByDescending(d => d).First();

            string HostFolder;
            string ArchFolder;
            switch (Arch)
            {
            case Architectures.x86:
            {
                HostFolder = "Hostx86";
                ArchFolder = "x86";
                break;
            }
            case Architectures.x64:
            {
                HostFolder = "Hostx64";
                ArchFolder = "x64";
                break;
            }
            case Architectures.ARM64:
            {
                HostFolder = "HostArm64";
                ArchFolder = "arm64";
                break;
            }
            default:
            {
                throw new Exception("Invalid Arch");
            }
            }
            
            string Final = Path.Combine(Latest, "bin", HostFolder, ArchFolder);
            if (!Directory.Exists(Final))
            {
                throw new Exception($"No suitable MSVCX toolchain for architecture '{Arch}' could be found. Make sure you have the proper compiler toolchain for the architecture installed.");
            }

            // Some validations
            string CompilerPath = Path.Combine(Final, COMPILER_EXECUTABLE);
            string LinkerPath = Path.Combine(Final, LINKER_EXECUTABLE);
            if (!File.Exists(CompilerPath))
            {
                throw new Exception($"{COMPILER_EXECUTABLE} missing, compiler not found even though all toolchain paths exist.");
            }
            if (!File.Exists(LinkerPath))
            {
                throw new Exception($"{LINKER_EXECUTABLE} missing, linker not found even though all toolchain paths exist.");
            }

            CachedToolsPaths[Arch] = Final;
            return Final;
        }

        private string ModuleLanguageStandardToClStd(ModuleLanguageStandard Std)
        {
            switch (Std)
            {
            case ModuleLanguageStandard.Cpp14: return "c++14";
            case ModuleLanguageStandard.Cpp17: return "c++17";
            case ModuleLanguageStandard.Cpp20: return "c++20";
            }
            throw new Exception("Ivld std");
        }

        private string GetModuleRuntimeLibsFlag(ModuleRuntimeLibraries RtLibs)
        {
            switch (RtLibs)
            {
            case ModuleRuntimeLibraries.UseDebug:   return "/MDd";
            case ModuleRuntimeLibraries.UseRelease: return "/MD";
            }
            throw new Exception("Ivld rtlibs");
        }

        private string GetOptimizationLevel(ModuleOptimization Olevel)
        {
            switch (Olevel)
            {
            case ModuleOptimization.Off:      return "d";
            case ModuleOptimization.Balanced: return "2";
            case ModuleOptimization.Smallest: return "1";
            case ModuleOptimization.Fastest:  return "2";
            case ModuleOptimization.Full:     return "x";
            }
            throw new Exception("Ivld olevel");
        }

        private string GetOutputTypeExt(ModuleOutputType Type) => Type switch
        {
            ModuleOutputType.ConsoleExecutable  => ".exe",
            ModuleOutputType.WindowedExecutable => ".exe",
            ModuleOutputType.StaticLibrary      => ".lib",
            ModuleOutputType.DynamicLibrary     => ".dll",

            _ => throw new NotImplementedException()
        };
    }
}

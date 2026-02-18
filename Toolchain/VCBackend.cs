// Copyright (C) 2026 ychgen, all rights reserved.

using System.Diagnostics;
using Moonquake.Orchestra;

namespace Moonquake.Toolchain
{
    public class VCBackend : Backend
    {
        public override Backends GetBackendType() => Backends.VC;

        public const string COMPILER_EXECUTABLE = "cl.exe";
        public const string ARCHIVER_EXECUTABLE = "lib.exe";
        public const string LINKER_EXECUTABLE   = "link.exe";

        private string VSInstallPath    = "";
        private string VSDevCmdFilepath = "";
        private readonly Dictionary<Architectures, string> CachedToolchainPaths = new();

        public VCBackend()
        {
            string vswhere = @"C:\Program Files (x86)\Microsoft Visual Studio\Installer\vswhere.exe";
            if (!File.Exists(vswhere))
            {
                throw new Exception("VCBackend error: vswhere.exe not found! Ensure you have Visual Studio installed. (https://visualstudio.microsoft.com/downloads/)");
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
            VSInstallPath = Proc.StandardOutput.ReadLine()!;
            Proc.WaitForExit();

            if (string.IsNullOrEmpty(VSInstallPath))
            {
                throw new Exception("No suitable Visual Studio installation found. Make sure you installed a version of Visual Studio, not just the Visual Studio Installer.");
            }
            
            VSDevCmdFilepath = Path.Combine(VSInstallPath, "Common7", "Tools", "VsDevCmd.bat");
            if (!File.Exists(VSDevCmdFilepath))
            {
                throw new Exception($"VsDevCmd.bat batch script couldn't be located! Make sure you have a version of Visual Studio installed.");
            }
        }

        public override bool InvokeCompiler(string TranslationUnitToCompile, string ObjectFileAsToCompile, BuildModule Module)
        {
            string CompilerPath;
            try
            {
                CompilerPath = Path.Combine(GetToolchainPath(Moonquake.BuildOrder.Architecture), COMPILER_EXECUTABLE);
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
                $"/std:{GetModuleLanguageStandardFlag(Module.CppStandard)}",
                "/EHsc",
                GetModuleRuntimeLibrariesFlag(Module.RuntimeLibraries),
                $"/O{GetOptimizationLevelFlag(Module.OptimizationLevel)}",
                
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
            string LaunchCommand = $"\"{VSDevCmdFilepath}\" -no_logo -arch={Moonquake.BuildOrder.Architecture} && \"{CompilerPath}\" {CommandLine}";

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
            string FinalBin = Path.Combine(Module.OutputPath, Module.OutputName + GetOutputTypeFileExtension(Module.OutputType));
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
                string ArchiverPath = Path.Combine(GetToolchainPath(Moonquake.BuildOrder.Architecture), ARCHIVER_EXECUTABLE);

                List<string> Arguments = [ "/nologo", .. ObjectFiles, $"/OUT:{FinalBin}" ];
                string CommandLine = string.Join(" ", Arguments);
                string LaunchCommand = $"\"{VSDevCmdFilepath}\" -no_logo -arch={Moonquake.BuildOrder.Architecture} && \"{ArchiverPath}\" {CommandLine}";

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
                string LinkerPath = Path.Combine(GetToolchainPath(Moonquake.BuildOrder.Architecture), LINKER_EXECUTABLE);

                List<string> Linkages = new();
                foreach (BuildModule Linkage in Module.Linkages.Values)
                {
                    // we link against .lib of DLL, we cannot link against a .dll
                    // .lib covers the loading of the .dll
                    // for static libs it's already .lib
                    Linkages.Add(Path.Combine(Linkage.OutputPath, Path.Combine(Linkage.OutputName) + ".lib"));
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
                string LaunchCommand = $"\"{VSDevCmdFilepath}\" -no_logo -arch={Moonquake.BuildOrder.Architecture} && \"{LinkerPath}\" {CommandLine}";

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
        
        public string GetToolchainPath(Architectures Arch)
        {
            {
                string? Cached;
                if (CachedToolchainPaths.TryGetValue(Arch, out Cached))
                {
                    return Cached; 
                }
            }

            string VCToolsPath = Path.Combine(VSInstallPath, @"VC\Tools\MSVC");

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

            CachedToolchainPaths[Arch] = Final;
            return Final;
        }

        private string GetModuleLanguageStandardFlag(ModuleLanguageStandard Standard) => Standard switch
        {
            ModuleLanguageStandard.Cpp14 => "c++14",
            ModuleLanguageStandard.Cpp17 => "c++17",
            ModuleLanguageStandard.Cpp20 => "c++20",
            _ => throw new Exception()
        };

        private string GetModuleRuntimeLibrariesFlag(ModuleRuntimeLibraries RuntimeLibraries) => RuntimeLibraries switch
        {
            ModuleRuntimeLibraries.UseDebug   => "/MDd",
            ModuleRuntimeLibraries.UseRelease => "/MD",
            _ => throw new Exception()
        };

        private string GetOptimizationLevelFlag(ModuleOptimization OptimizationLevel) => OptimizationLevel switch
        {
            ModuleOptimization.Off       => "d",
            ModuleOptimization.Balanced  => "2",
            ModuleOptimization.Smallest  => "1",
            ModuleOptimization.Fastest   => "2",
            ModuleOptimization.Full      => "x",
            _ => throw new Exception()
        };

        private string GetOutputTypeFileExtension(ModuleOutputType Type) => Type switch
        {
            ModuleOutputType.ConsoleExecutable  => ".exe",
            ModuleOutputType.WindowedExecutable => ".exe",
            ModuleOutputType.StaticLibrary      => ".lib",
            ModuleOutputType.DynamicLibrary     => ".dll",
            _ => throw new Exception()
        };
    }
}

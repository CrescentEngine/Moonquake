// Copyright (C) 2026 ychgen, all rights reserved.

using System.Diagnostics;
using Moonquake.Orchestra;

namespace Moonquake.CodeGen
{
    public enum Backends
    {
        Null = 0,
        VisualStudio,
        Makefile
    }
    public abstract class Backend
    {
        // We immediately assign to this in Moonquake.Main.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public static Backend Instance;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public abstract Backends GetBackendType();

        /// <summary>
        /// Gets path to compiler executable/binary. Given output is valid at all times.
        /// </summary>
        public abstract string GetCompilerInvocation(Architectures Arch);

        /// <summary>
        /// Gets path to linker executable/binary. Given output is valid at all times.
        /// </summary>
        public abstract string GetLinkerInvocation(Architectures Arch);

        /// <summary>
        /// Gets path to static library linker executable/binary. Given output is valid at all times.
        /// </summary>
        public abstract string GetArchiverInvocation(Architectures Arch);

        /// <summary>
        /// Invokes the compiler related to this backend and compiles a given TU to the given object filepath name.
        /// </summary>
        /// <param name="TranslationUnitToCompile">Which TU to compile.</param>
        /// <param name="ObjectFileAsToCompile">As to compile.</param>
        /// <param name="Module">Many flags will be deduced from the Module.</param>
        /// <returns>Whether or not the compilation was successful.</returns>
        public abstract bool InvokeCompiler(string TranslationUnitToCompile, string ObjectFileAsToCompile, BuildModule Module);

        /// <summary>
        /// Invokes the linker related to this backend to link multiple object files into the proper OutputType of Module.
        /// </summary>
        /// <param name="ObjectFiles">List of objects to link together.</param>
        /// <param name="Module">Flags & OutputType to be deduced from this.</param>
        /// <returns></returns>
        public abstract bool InvokeLinker(IEnumerable<string> ObjectFiles, BuildModule Module);
    }
    public class NullBackend : Backend
    {
        public override Backends GetBackendType() => Backends.Null;
        public override string GetCompilerInvocation(Architectures Arch) => "";
        public override string GetLinkerInvocation(Architectures Arch) => "";
        public override string GetArchiverInvocation(Architectures Arch) => "";
        public override bool InvokeCompiler(string TranslationUnitToCompile, string ObjectFileAsToCompile, BuildModule Module) => false;
        public override bool InvokeLinker(IEnumerable<string> ObjectFiles, BuildModule Module) => false;
    }
    /// <summary>
    /// Not yet implemented.
    /// </summary>
    public class MakefileBackend : Backend
    {
        public override Backends GetBackendType() => Backends.Makefile;
        public override string GetCompilerInvocation(Architectures Arch) => throw new NotImplementedException();
        public override string GetLinkerInvocation(Architectures Arch) => throw new NotImplementedException();
        public override string GetArchiverInvocation(Architectures Arch) => throw new NotImplementedException();
        public override bool InvokeCompiler(string TranslationUnitToCompile, string ObjectFileAsToCompile, BuildModule Module) => throw new NotImplementedException();
        public override bool InvokeLinker(IEnumerable<string> ObjectFiles, BuildModule Module) => throw new NotImplementedException();
    }
}

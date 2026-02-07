// Copyright (C) 2026 ychgen, all rights reserved.

using System;
using System.IO;
using System.Runtime.InteropServices;
using Moonquake.CodeGen;
using Moonquake.DSL;

namespace Moonquake
{
    public enum Architectures
    {
        x64,
        ARM64,
        x86
    }
    public enum Platforms
    {
        Windows,
        Linux
    }

    public struct BuildOrder
    {
        public string Root;
        public string Configuration;
        public Architectures Architecture;
        public Platforms Platform;
    }

    public static class Moonquake
    {
        public static BuildOrder BuildOrder = new BuildOrder();
        
        private static int Main(string[] Arguments)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                BuildOrder.Platform = Platforms.Windows;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                BuildOrder.Platform = Platforms.Linux;
            }

            //if (Arguments.Length == 0 || Arguments.Length > 2)
            //{
            //    Console.WriteLine("Moonquake usage: [BuildPath:String](Either path to .mqroot or a directory containing only a singular one.) [Backend:{\"VS\",\"Make\"}]<Optional>(Recommended backend will be chosen based on your OS and installed toolchains.)");
            //    return 1;
            //}
            //string BuildPath = Arguments[0];
            //Backend Backend = Backend.Null;
            //if (Arguments.Length == 2)
            //{
            //    switch (Arguments[1])
            //    {
            //    case "VS":   Backend = Backend.VisualStudio; break;
            //    case "Make": Backend = Backend.Make;         break;
            //    default:
            //        Console.WriteLine($"Error: Invalid Backend '{Backend}' was provided.");
            //        return 1;
            //    }
            //}

            string PathToRoot = ".\\Examples\\HelloWorld";
            string BuildPath = PathToRoot;
            //string PathToRoot = BuildPath;
            if (PathToRoot.EndsWith(".mqroot"))
            {
                if (!File.Exists(PathToRoot))
                {
                    Console.WriteLine($"Error: Given MQROOT file '{PathToRoot}' doesn't exist or is inaccessible.");
                    return 1;
                }
                PathToRoot = Path.GetFullPath(PathToRoot);
            }
            else
            {
                if (File.Exists(BuildPath))
                {
                    Console.WriteLine($"Error: Given MQROOT file '{BuildPath}' doesn't have the expected file extension of '.mqroot'.");
                    return 1;
                }
                if (!Directory.Exists(BuildPath))
                {
                    Console.WriteLine($"Error: Given build directory path '{BuildPath}' doesn't exist or is inaccessible.");
                    return 1;
                }

                var Roots = Directory.EnumerateFiles(BuildPath, "*.mqroot", SearchOption.TopDirectoryOnly);
                int NumRoots = 0;
                foreach (string Root in Roots)
                {
                    PathToRoot = Root;
                    NumRoots++;
                }
                if (NumRoots == 0)
                {
                    Console.WriteLine($"Error: Given build directory '{BuildPath}' contains no '.mqroot' file.");
                    return 1;
                }
                if (NumRoots > 1)
                {
                    Console.WriteLine($"Error: Given build directory '{PathToRoot}' contains {NumRoots} '.mqroot' files. When providing a directory path this must be only 1.");
                    return 1;
                }

                if (!File.Exists(PathToRoot))
                {
                    Console.WriteLine($"Error: Given MQROOT file '{PathToRoot}' (discovered from build path '{BuildPath}) doesn't exist or is inaccessible.");
                    return 1;
                }
                PathToRoot = Path.GetFullPath(PathToRoot);
                Console.WriteLine($"Detected Root File: {PathToRoot}");
            }
            
            Parser RootParser = new Parser(new Lexer(PathToRoot, File.ReadAllText(PathToRoot)));
            CompoundAST RootNode = RootParser.Parse();
            Visitor RootVisitor = new Visitor(PathToRoot);
            Directory.SetCurrentDirectory(Path.GetDirectoryName(PathToRoot)!);
            RootVisitor.Visit(RootNode);

            return 0;
        }
    }
}

// Copyright (C) 2026 ychgen, all rights reserved.

namespace Moonquake.Commands
{
    public static class DescriptionFileInputHelper
    {
        public static DSL.CompoundAST? Help(string Input, bool bDir, out string Filepath)
        {
            Filepath = Input;
            if (bDir)
            {
                if (!Directory.Exists(Filepath))
                {
                    Console.WriteLine($"No such directory as '{Filepath}' exists.");
                    return null;
                }
                try
                {
                    Filepath = Path.Combine(Filepath, Path.GetFileName(Filepath.TrimEnd(Path.DirectorySeparatorChar))) + ".mqroot";
                }
                catch
                {
                    Console.WriteLine($"Couldn't construct eventual path from given bDirectory path input '{Input}'.");
                    return null;
                }
            }
            if (!File.Exists(Filepath))
            {
                Console.WriteLine($"Description file '{Filepath}' doesn't exist.");
                return null;
            }
            
            string FileContent;
            try
            {
                FileContent = File.ReadAllText(Filepath);
            }
            catch
            {
                Console.WriteLine($"Failed to read description file '{Filepath}'.");
                return null;
            }

            // Get absolute path.
            Filepath = Path.GetFullPath(Filepath);
            // Switch working directory.
            Directory.SetCurrentDirectory(Path.GetDirectoryName(Filepath)!);

            DSL.Parser SrcParser;
            DSL.CompoundAST SourceRoot;
            try
            {
                SrcParser = new DSL.Parser(new DSL.Lexer(Filepath, FileContent));
                SourceRoot = SrcParser.Parse();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Parser failed, cannot continue execution without a valid source of truth. {e}");
                return null;
            }

            return SourceRoot;
        }
    }
}

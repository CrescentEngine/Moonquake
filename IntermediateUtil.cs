// Copyright (C) 2026 ychgen, all rights reserved.

using System;
using Moonquake.DSL;

namespace Moonquake
{
    public static class IntermediateUtil
    {
        public static void PrintAST(AST ast, int depth = 0)
        {
            if (ast == null)
            {
                Indent(depth);
                Console.WriteLine("NULL");
                return;
            }

            Indent(depth);
            Console.WriteLine($"Node {ast.Type}");

            switch (ast.Type)
            {
                case ASTType.Noop:
                    break;

                case ASTType.Compound:
                {
                    var compound = (CompoundAST)ast;
                    foreach (var node in compound.Compound)
                        PrintAST(node, depth + 1);
                    break;
                }

                case ASTType.String:
                {
                    var s = (StringAST)ast;
                    Indent(depth + 1);
                    Console.WriteLine($"LIT\"{s.Literal}\", RES\"{s.Resolved}\"");
                    break;
                }

                case ASTType.Directive:
                {
                    var d = (DirectiveAST)ast;

                    Indent(depth + 1);
                    Console.WriteLine($"Directive: {d.DirectiveName}");

                    Indent(depth + 1);
                    Console.WriteLine("Parameters:");
                    foreach (var p in d.Parameters.Compound)
                        PrintAST(p, depth + 2);

                    if (d.Body is not null)
                    {
                        Indent(depth + 1);
                        Console.WriteLine("Body:");
                        PrintAST(d.Body, depth + 2);
                    }
                    break;
                }

                case ASTType.FieldAssignment:
                {
                    var f = (FieldAssignmentAST)ast;
                    Indent(depth + 1);
                    Console.WriteLine($"Assign Field: \"{f.FieldName}\"");
                    PrintAST(f.Value, depth + 2);
                    break;
                }

                case ASTType.FieldAppendment:
                {
                    var f = (FieldAppendmentAST)ast;
                    Indent(depth + 1);
                    Console.WriteLine($"Append Field: \"{f.FieldName}\"");
                    PrintAST(f.Value, depth + 2);
                    break;
                }

                case ASTType.FieldErasure:
                {
                    var f = (FieldErasureAST)ast;
                    Indent(depth + 1);
                    Console.WriteLine($"Erase Field: \"{f.FieldName}\"");
                    PrintAST(f.Value, depth + 2);
                    break;
                }

                case ASTType.FieldUnassignment:
                {
                    var f = (FieldUnassignmentAST)ast;
                    Indent(depth + 1);
                    Console.WriteLine($"Unassign Field: \"{f.FieldName}\"");
                    break;
                }

                case ASTType.DubiousAssignment:
                {
                    var f = (FieldAssignmentAST)ast;
                    Indent(depth + 1);
                    Console.WriteLine($"Dubious Assign: \"{f.FieldName}\"");
                    PrintAST(f.Value, depth + 2);
                    break;
                }

                case ASTType.Array:
                {
                    var array = (ArrayAST)ast;
                    Indent(depth + 1);
                    Console.WriteLine("Array:");
                    foreach (var v in array.Value)
                    {
                        Indent(depth + 2);
                        Console.WriteLine($"LIT\"{v.Literal}\" RES\"{v.Resolved}\"");
                    }
                    break;
                }

                default:
                    Indent(depth + 1);
                    Console.WriteLine("(Unknown AST Type)");
                    break;
            }
        }

        private static void Indent(int depth)
        {
            Console.Write(new string(' ', depth * 2));
        }
    }
}

// Copyright (C) 2026 ychgen, all rights reserved.

using System.Collections.Immutable;
using System.Text;

namespace Moonquake.DSL
{
    enum EvaluationContext
    {
        GlobalScope,
        RootScope,
        SchemaScope,
        ModuleScope
    }
    struct VisitorContext
    {
        public EvaluationContext     EvalContext = EvaluationContext.GlobalScope;
        public Constructs.Construct? Construct   = null;

        public VisitorContext()
        {
        }

        public VisitorContext(VisitorContext Other)
        {
            EvalContext = Other.EvalContext;
            Construct = Other.Construct;
        }
    }

    enum DirectiveResult
    {
        // Executed successfully
        Successful,
        // Directive internal execution failed
        Failure,
        // No such overload exists based on given parameters
        InvalidOverload,
        // Directive functionality not yet implemented within BuildTool
        Unimplemented
    }
    struct DirectiveHandler
    {
        public Dictionary<ImmutableArray<ASTType>, Func<DirectiveAST, DirectiveResult>> Overloads;
        public EvaluationContext[] ValidContexts;
        public bool Bodily;

        public DirectiveHandler(Dictionary<ImmutableArray<ASTType>, Func<DirectiveAST, DirectiveResult>> InOverloads, EvaluationContext[] InValidContexts, bool InBodily)
        {
            Overloads = InOverloads;
            ValidContexts = InValidContexts;
            Bodily = InBodily;
        }
    }
    class DirectiveBuilder
    {
        private readonly Dictionary<ImmutableArray<ASTType>, Func<DirectiveAST, DirectiveResult>> _overloads = new(new AstTypeArrayComparer());
        private readonly List<EvaluationContext> _contexts = new();
        private bool _bodily = false;

        public DirectiveBuilder On(Func<DirectiveAST, DirectiveResult> fn, params ASTType[] args)
        {
            _overloads[ImmutableArray.CreateRange(args)] = fn;
            return this;
        }

        public DirectiveBuilder In(params EvaluationContext[] contexts)
        {
            _contexts.AddRange(contexts);
            return this;
        }

        public DirectiveBuilder Bodily()
        {
            _bodily = true;
            return this;
        }

        public DirectiveHandler Build() => new(_overloads, _contexts.ToArray(), _bodily);
    }

    public class Visitor
    {
        /// <summary>
        /// Context is only singular and not a stack because MQBDL doesn't support nested Constructs.
        /// Maybe update in the future if nested Constructs are added but it's highly unlikely.
        /// </summary>
        private VisitorContext Context = new VisitorContext();
        private Dictionary<string, DirectiveHandler> DirectiveDict = new();
        private LanguageVersion DeclaredVersion = LanguageVersion.Invalid;
        private string Filepath = "";

        private Dictionary<string, Constructs.Root>   Roots   = new();
        private Dictionary<string, Constructs.Module> Modules = new();
        private Dictionary<string, Constructs.Schema> Schemas = new();

        public Visitor(string InFilepath)
        {
            Filepath = InFilepath;

            DirectiveDict[Directives.DECLARE_VERSION] =
            new DirectiveBuilder().On(Dir_DECLARE_VERSION, ASTType.String).In(EvaluationContext.GlobalScope).Build();

            DirectiveDict[Directives.DECLARE_ROOT] =
            new DirectiveBuilder().On(Dir_DECLARE_ROOT, ASTType.String).In(EvaluationContext.GlobalScope).Bodily().Build();

            DirectiveDict[Directives.DECLARE_SCHEMA] =
            new DirectiveBuilder().On(Dir_DECLARE_SCHEMA_Dflt, ASTType.String)
                                  .On(Dir_DECLARE_SCHEMA_Inherit, ASTType.String, ASTType.String)
                                  .In(EvaluationContext.GlobalScope).Bodily().Build();

            DirectiveDict[Directives.DECLARE_MODULE] =
            new DirectiveBuilder().On(Dir_DECLARE_MODULE_Dflt, ASTType.String)
                                  .On(Dir_DECLARE_MODULE_Schemed, ASTType.String, ASTType.String)
                                  .In(EvaluationContext.GlobalScope).Bodily().Build();
            
            DirectiveDict[Directives.INCLUDE_MODULE] =
            new DirectiveBuilder().On(Dir_INCLUDE_MODULE, ASTType.String).In(EvaluationContext.GlobalScope).Build();
            
            DirectiveDict[Directives.MACRO_DEFINE] =
            new DirectiveBuilder().On(Dir_MACRO_DEFINE_Dflt, ASTType.String)
                                  .On(Dir_MACRO_DEFINE_Valued, ASTType.String, ASTType.String)
                                  .In(EvaluationContext.SchemaScope, EvaluationContext.ModuleScope).Build();
        }

        public Dictionary<string, Constructs.Root>   GetRoots()   => Roots;
        public Dictionary<string, Constructs.Module> GetModules() => Modules;
        public Dictionary<string, Constructs.Schema> GetSchemas() => Schemas;

        /// <summary>
        /// Resets the Visitor into a clean state.
        /// </summary>
        public void Reset()
        {
            Context.EvalContext = EvaluationContext.GlobalScope;
            Context.Construct = null;
            Roots.Clear();
        }

        public void Visit(AST Node)
        {
            switch (Node.Type)
            {
            case ASTType.Compound:
            {
                VisitCompound((CompoundAST) Node);
                break;
            }
            case ASTType.String:
            {
                VisitString((StringAST) Node);
                break;
            }
            case ASTType.Directive:
            {
                VisitDirective((DirectiveAST) Node);
                break;
            }
            case ASTType.FieldAssignment:
            {
                VisitFieldAssignment((FieldAssignmentAST) Node);
                break;
            }
            case ASTType.FieldAppendment:
            {
                VisitFieldAppendment((FieldAppendmentAST) Node);
                break;
            }
            case ASTType.FieldErasure:
            {
                VisitFieldErasure((FieldErasureAST) Node);
                break;
            }
            case ASTType.FieldUnassignment:
            {
                VisitFieldUnassignment((FieldUnassignmentAST) Node);
                break;
            }
            case ASTType.Array:
            {
                VisitArray((ArrayAST) Node);
                break;
            }
            }
        }

        private void VisitCompound(CompoundAST Node)
        {
            foreach (AST Subnode in Node.Compound)
            {
                Visit(Subnode);
            }
        }

        private void VisitString(StringAST Node)
        {
            // We swear by God don't touch shit if Resolved already contains anything, because it means it was resolved or hacked around for a good reason.
            // Parser never populates Resolved anyway, it only populates Literal.
            if (Node.Resolved != "")
            {
                return;
            }
            StringBuilder Builder = new StringBuilder(Node.Literal);

            if (Context.Construct is not null)
            {
                switch (Context.EvalContext)
                {
                case EvaluationContext.ModuleScope:
                {
                    Constructs.Module Mod = (Constructs.Module) Context.Construct;

                    Builder.Replace("%ModuleName%", Mod.Name);
                    Builder.Replace("%ModulePath%", Mod.Str("Path"));
                    Builder.Replace("%Configuration%", Moonquake.BuildOrder.Configuration);
                    Builder.Replace("%Platform%", Moonquake.BuildOrder.Platform.ToString());
                    Builder.Replace("%Architecture%", Moonquake.BuildOrder.Architecture.ToString());
                    
                    break;
                }
                default:
                {
                    break;
                }
                }
            }

            Node.Resolved = Builder.ToString();
        }

        private void VisitDirective(DirectiveAST Node)
        {
            if (DeclaredVersion == LanguageVersion.Invalid)
            {
                if (Node.DirectiveName != Directives.DECLARE_VERSION)
                {
                    throw new Exception(
                        $"Visitor.Visit() error: The first statement to be made within a MQBDL file must be a {Directives.DECLARE_VERSION}() directive."
                    );
                }
            }

            ASTType[] OverloadRequested = new ASTType[Node.Parameters.Compound.Count];
            for (int i = 0; i < Node.Parameters.Compound.Count; i++)
            {
                AST Parameter = Node.Parameters.Compound[i];
                OverloadRequested[i] = Parameter.Type;
                Visit(Parameter);
            }

            if (!DirectiveDict.ContainsKey(Node.DirectiveName))
            {
                throw new Exception($"Visitor.VisitDirective() error: Visiting directive named '{Node.DirectiveName}()', but no such directive exists at all.");
            }
            DirectiveHandler Handler = DirectiveDict[Node.DirectiveName];

            if (!Handler.ValidContexts.Contains(Context.EvalContext))
            {
                if (Handler.ValidContexts.Length == 1)
                {
                    throw new Exception($"Visitor.VisitDirective() error: Directive '{Node.DirectiveName}()' can only be executed within a '{Handler.ValidContexts[0]}' context, but code is currently in a '{Context.EvalContext}'.");
                }
                else
                {
                    string ValidContextsText = $"{Handler.ValidContexts[0]}";
                    for (int i = 1; i < Handler.ValidContexts.Length; i++)
                    {
                        string Context = Handler.ValidContexts[i].ToString();
                        if (i + 1 != Handler.ValidContexts.Length)
                        {
                            ValidContextsText += $", {Context}";
                        }
                        else
                        {
                            ValidContextsText += $" and {Context}";
                        }
                    }
                    throw new Exception($"Visitor.VisitDirective() error: Directive '{Node.DirectiveName}()' can only be executed within '{ValidContextsText}' contexts, but code is currently in a '{Context.EvalContext}'.");
                }
            }

            if (Handler.Bodily)
            {
                if (Node.Body is null)
                {
                    throw new Exception($"Visitor.VisitDirective() errror: Directive '{Node.DirectiveName}()' cannot have a body.");
                }
            }
            else if (Node.Body is not null)
            {
                throw new Exception($"Visitor.VisitDirective() errror: Directive '{Node.DirectiveName}()' must have a body.");
            }

            DirectiveResult Result = DirectiveResult.InvalidOverload;
            if (Handler.Overloads.ContainsKey(OverloadRequested.ToImmutableArray())) // TODO: Improve writing, I am not in the mood rn.
            {
                Result = Handler.Overloads[OverloadRequested.ToImmutableArray()](Node);
            }
            
            if (Result != DirectiveResult.Successful)
            {
                throw new Exception($"Visitor.VisitDirective() error: Directive '{Node.DirectiveName}()' failed with error code '{Result}'.");
            }
        }

        private void VisitFieldAssignment(FieldAssignmentAST Node)
        {
            if (Context.Construct is null)
            {
                throw new Exception($"Visitor.VisitFieldAssignment() error: Cannot assign to a field on global scope, no construct is in context.");
            }
            Visit(Node.Value); // Visit RHS (Right Hand Side, thing we are assigning the field.).
            Constructs.FieldMutationResult Result = Context.Construct.AssignField(Node.FieldName, Node.Value);
            if (Result != Constructs.FieldMutationResult.Successful)
            {
                throw new Exception($"Visitor.VisitFieldAssignment() error: Assignment to field '{Node.FieldName}' failed with error code '{Result}'.");
            }
        }

        private void VisitFieldAppendment(FieldAppendmentAST Node)
        {
            if (Context.Construct is null)
            {
                throw new Exception($"Visitor.VisitFieldAppendment() error: Cannot append to a field on global scope, no construct is in context.");
            }
            Visit(Node.Value); // Visit RHS (Right Hand Side, thing we are appending to the field.).
            Constructs.FieldMutationResult Result = Context.Construct.AppendToField(Node.FieldName, Node.Value);
            if (Result != Constructs.FieldMutationResult.Successful)
            {
                throw new Exception($"Visitor.VisitFieldAppendment() error: Appendment to field '{Node.FieldName}' failed with error code '{Result}'.");
            }
        }

        private void VisitFieldErasure(FieldErasureAST Node)
        {
            if (Context.Construct is null)
            {
                throw new Exception($"Visitor.VisitFieldErasure() error: Cannot erase from a field on global scope, no construct is in context.");
            }
            Visit(Node.Value); // Visit RHS (Right Hand Side, thing we are erasing from the field.).
            Constructs.FieldMutationResult Result = Context.Construct.EraseFromField(Node.FieldName, Node.Value);
            if (Result != Constructs.FieldMutationResult.Successful)
            {
                throw new Exception($"Visitor.VisitFieldErasure() error: Erasure from field '{Node.FieldName}' failed with error code '{Result}'.");
            }
        }

        private void VisitFieldUnassignment(FieldUnassignmentAST Node)
        {
            if (Context.Construct is null)
            {
                throw new Exception($"Visitor.VisitFieldUnassignment() error: Cannot unassign a field on global scope, no construct is in context.");
            }
            Constructs.FieldMutationResult Result = Context.Construct.UnassignField(Node.FieldName);
            if (Result != Constructs.FieldMutationResult.Successful)
            {
                throw new Exception($"Visitor.VisitFieldUnassignment() error: Unassignment of field '{Node.FieldName}' failed with error code '{Result}'.");
            }
        }

        private void VisitArray(ArrayAST Node)
        {
            foreach (StringAST String in Node.Value)
            {
                Visit(String);
            }
        }

        private DirectiveResult Dir_DECLARE_VERSION(DirectiveAST Directive)
        {
            if (DeclaredVersion != LanguageVersion.Invalid)
            {
                throw new Exception($"{Directives.DECLARE_VERSION}() directive error: Language version was already declared as '{DeclaredVersion}' before.");
            }

            string VersionString = ((StringAST) Directive.Parameters.Compound[0]).Resolved;
            try
            {
                DeclaredVersion = LanguageVersion.FromString(VersionString);
            }
            catch
            {
                throw new Exception($"{Directives.DECLARE_VERSION}() directive error: Language version string '{VersionString}' couldn't be parsed.");
            }

            return DirectiveResult.Successful;
        }

        private DirectiveResult Dir_DECLARE_ROOT(DirectiveAST Directive)
        {
            string ConstructName = ((StringAST) Directive.Parameters.Compound[0]).Resolved;
            if (Roots.ContainsKey(ConstructName))
            {
                throw new Exception($"{Directives.DECLARE_ROOT}() directive error: Attempting to declare a root with name '{ConstructName}', but a root with that name already exists.");
            }
            Constructs.Root Subject = new Constructs.Root
            {
                Name = ConstructName
            };

            VisitorContext PrevContext = DupeCtx();
            Context.EvalContext = EvaluationContext.RootScope;
            Context.Construct = Subject;

            Visit(Directive.Body!);
            Roots[ConstructName] = Subject;
            
            Context = PrevContext; // restore context
            return DirectiveResult.Successful;
        }

        private DirectiveResult Dir_DECLARE_SCHEMA_Dflt(DirectiveAST Directive)
        {
            string ConstructName = ((StringAST) Directive.Parameters.Compound[0]).Resolved;
            if (Schemas.ContainsKey(ConstructName))
            {
                throw new Exception($"{Directives.DECLARE_ROOT}() directive error: Attempting to declare a schema with name '{ConstructName}', but a schema with that name already exists.");
            }
            Constructs.Schema Subject = new Constructs.Schema
            {
                Name = ConstructName
            };

            VisitorContext PrevContext = DupeCtx();
            Context.EvalContext = EvaluationContext.SchemaScope;
            Context.Construct = Subject;

            Visit(Directive.Body!);
            Schemas[ConstructName] = Subject;
            
            Context = PrevContext; // restore context
            return DirectiveResult.Successful;
        }

        private DirectiveResult Dir_DECLARE_SCHEMA_Inherit(DirectiveAST Directive)
        {
            return DirectiveResult.Unimplemented;
        }

        private DirectiveResult Dir_DECLARE_MODULE_Dflt(DirectiveAST Directive)
        {
            string ConstructName = ((StringAST) Directive.Parameters.Compound[0]).Resolved;
            if (Modules.ContainsKey(ConstructName))
            {
                throw new Exception($"{Directives.DECLARE_ROOT}() directive error: Attempting to declare a module with name '{ConstructName}', but a module with that name already exists.");
            }
            Constructs.Module Subject = new Constructs.Module
            {
                Name = ConstructName
            };

            VisitorContext PrevContext = DupeCtx();
            Context.EvalContext = EvaluationContext.ModuleScope;
            Context.Construct = Subject;

            Visit(Directive.Body!);
            Modules[ConstructName] = Subject;
            
            Context = PrevContext; // restore context
            return DirectiveResult.Successful;
        }

        private DirectiveResult Dir_DECLARE_MODULE_Schemed(DirectiveAST Directive)
        {
            return DirectiveResult.Unimplemented;
        }

        private DirectiveResult Dir_INCLUDE_MODULE(DirectiveAST Directive)
        {
            Visit(Directive.Parameters);
            string ToInclude = ((StringAST) Directive.Parameters.Compound[0]).Resolved;

            if (File.Exists(ToInclude))
            {
                if (!ToInclude.EndsWith(".mqmod"))
                {
                    throw new Exception($"{Directives.INCLUDE_MODULE}() directive error: Filepath inclusions must end with '.mqmod'. Given '{ToInclude}' doesn't satisfy that.");
                }
                goto FileDetected;
            }

            if (!Directory.Exists(ToInclude))
            {
                throw new Exception($"{Directives.INCLUDE_MODULE}() directive error: No such directory '{ToInclude}' exists. If you meant to include a file, it must end in '.mqmod'.");
            }

            ToInclude = Path.Combine(ToInclude, Path.GetFileName(ToInclude.TrimEnd(Path.DirectorySeparatorChar))) + ".mqmod";
            if (!File.Exists(ToInclude))
            {
                throw new Exception($"{Directives.INCLUDE_MODULE}() directive error: File to include '{ToInclude}' (deduced from directory-file inclusion relation rule) doesn't exist.");
            }
        FileDetected:
            ToInclude = Path.GetFullPath(ToInclude);
            string Content = File.ReadAllText(ToInclude);
            Parser InclParser = new Parser(new Lexer(ToInclude, Content));
            Visit(InclParser.Parse());

            return DirectiveResult.Successful;
        }

        private DirectiveResult Dir_MACRO_DEFINE_Dflt(DirectiveAST Directive)
        {
            return DirectiveResult.Unimplemented;
        }

        private DirectiveResult Dir_MACRO_DEFINE_Valued(DirectiveAST Directive)
        {
            return DirectiveResult.Unimplemented;
        }

        private VisitorContext DupeCtx() => new VisitorContext(Context);
    }
}

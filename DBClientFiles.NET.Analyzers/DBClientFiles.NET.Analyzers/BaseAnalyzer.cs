using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using DBClientFiles.NET.Analyzers.Extenstions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DBClientFiles.NET.Analyzers
{
    public abstract class BaseAnalyzer : DiagnosticAnalyzer
    {
        private readonly SyntaxTreeValueProvider<bool> _treeValueProvider;
        private readonly HashSet<ClassDeclarationSyntax> _treeCallbackSet;

        protected BaseAnalyzer()
        {
            _treeValueProvider = new SyntaxTreeValueProvider<bool>(HasIndexProperty);
            _treeCallbackSet = new HashSet<ClassDeclarationSyntax>(ClassDeclarationSyntaxComparer.Instance);
        }

        public override void Initialize(AnalysisContext analysisContext)
        {
            analysisContext.RegisterSemanticModelAction(HandleSemanticAnalysis);
            analysisContext.RegisterCompilationStartAction(HandleCompilationStart);
        }

        private bool HasIndexProperty(SyntaxTree classInstance)
        {
            return GetIndex<PropertyDeclarationSyntax>(classInstance) != null;
        }

        private void HandleCompilationStart(CompilationStartAnalysisContext context)
        {
            context.RegisterSemanticModelAction(symbolContext =>
            {
                var semanticModel = symbolContext.SemanticModel;
                var syntaxTree = semanticModel.SyntaxTree;

                var typeDeclarations = syntaxTree.GetRoot().EnumerateNodes<ClassDeclarationSyntax>();
                foreach (var classInfo in typeDeclarations)
                {
                    var indexProperty = GetIndex<PropertyDeclarationSyntax>(ref symbolContext, classInfo);
                    var indexField = GetIndex<FieldDeclarationSyntax>(ref symbolContext, classInfo);

                    context.TryGetValue(classInfo, _treeValueProvider, out hasIndexProperty);
                }

            });
        }

        /// <summary>
        /// Returns the name of the collection in which we are targetting.
        /// </summary>
        /// <returns></returns>
        protected abstract string GetTargetCollectionType();

        /// <summary>
        /// Returns the target assembly on which to check the returned value of <see cref="GetTargetCollectionType"/>.
        /// </summary>
        /// <returns></returns>
        protected abstract string GetTargetAssembly();

        /// <summary>
        /// Returns the explicit key type as declared by the type arguments passed to the collection.
        /// </summary>
        /// <param name="typeArgs"></param>
        /// <returns></returns>
        protected abstract ITypeSymbol GetExplicitKeyType(ImmutableArray<ITypeSymbol> typeArgs);

        protected abstract DiagnosticDescriptor GetRule(DiagnosticSeverity severity);

        protected virtual Diagnostic CreateDiagnostic(DiagnosticSeverity severity, Location location, params object[] formatParameters)
        {
            return Diagnostic.Create(GetRule(severity), location, formatParameters);
        }

        /// <summary>
        /// Given a class declaration, look for member fields or properties with IndexAttribute.
        /// If none are found, return the first one that is named "ID" (case insensitive).
        /// If still none found, return the first member.
        /// If no members are found, return null.
        /// </summary>
        /// <typeparam name="T">Either <see cref="PropertyDeclarationSyntax"/> or <see cref="FieldDeclarationSyntax"/>.</typeparam>
        /// <param name="context"></param>
        /// <param name="classDeclarationSyntax"></param>
        /// <returns></returns>
        protected ISymbol GetIndex<T>(ref SemanticModelAnalysisContext context, ClassDeclarationSyntax classDeclarationSyntax)
            where T : MemberDeclarationSyntax
        {
            var members = classDeclarationSyntax.EnumerateNodes<T>().ToArray();
            foreach (var memberInfo in members)
            {
                var memberSymbol = context.SemanticModel.GetDeclaredSymbol(memberInfo, context.CancellationToken);
                var memberAttributes = memberSymbol.GetAttributes();
                foreach (var attrInfo in memberAttributes)
                {
                    var attrClass = attrInfo.AttributeClass;
                    if (attrClass.ContainingAssembly.Name != GetTargetAssembly())
                        continue;

                    if (attrClass.Name == "IndexAttribute")
                        return memberSymbol;
                }
            }

            if (members.Length != 0)
            {
                var memberInfo = members.FirstOrDefault(m =>
                    m.ChildNodes().OfType<IdentifierNameSyntax>().First().Identifier.ValueText.ToUpperInvariant() ==
                    "ID");

                if (memberInfo == null)
                    return context.SemanticModel.GetDeclaredSymbol(members[0], context.CancellationToken);

                return context.SemanticModel.GetDeclaredSymbol(memberInfo, context.CancellationToken);
            }

            // TODO: Throw a diagnostic if the structure is used by collections.
            return null;
        }

        private void HandleSemanticAnalysis(SemanticModelAnalysisContext context)
        {
            var semanticModel = context.SemanticModel;
            var syntaxTree = semanticModel.SyntaxTree;

            var typeDeclarations = syntaxTree.GetRoot().EnumerateNodes<ClassDeclarationSyntax>();
            foreach (var classInfo in typeDeclarations)
            {
                var indexProperty = GetIndex<PropertyDeclarationSyntax>(ref context, classInfo);
                var indexField    = GetIndex<FieldDeclarationSyntax>(ref context, classInfo);
            }

            var objectInstanciations = syntaxTree.GetRoot().EnumerateNodes<ObjectCreationExpressionSyntax>();
            foreach (var objectInstanciation in objectInstanciations)
            {
                var argumentSymbols = objectInstanciation.ArgumentList.Arguments.Select(
                    a => semanticModel.GetSymbolInfo(a.Expression)).ToArray();

                var objectTypeInfo = semanticModel.GetTypeInfo(objectInstanciation, context.CancellationToken);
                if (!(objectTypeInfo.Type is INamedTypeSymbol instanceType))
                    continue;
                
                if (instanceType.ContainingAssembly.Name != GetTargetAssembly())
                    continue;

                if (instanceType.Name != GetTargetCollectionType())
                    continue;

                var typeArgs = instanceType.TypeArguments;
                var specifiedKeyType = GetExplicitKeyType(typeArgs);

                var storageOptions = default(SymbolInfo);
                if (argumentSymbols.Length != 1)
                    storageOptions = context.SemanticModel.GetSymbolInfo(objectInstanciation.ArgumentList.Arguments.Last());
                
                // Create diagnostics if
                // 1. type mismatch between generic key and declared index (error)
                // 2. no declared index (warning)
            }
        }
    }
}

using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DBClientFiles.NET.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class StorageListAnalyzer : BaseAnalyzer
    {
        public const string DiagnosticAnalyzerID = "StorageListAnalyzer";
        private static readonly LocalizableString Title 
            = new LocalizableResourceString(nameof(Resources.ListTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat 
            = new LocalizableResourceString(nameof(Resources.ListMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description 
            = new LocalizableResourceString(nameof(Resources.ListDescription), Resources.ResourceManager, typeof(Resources));
        private const string Category = "DBClientFiles.NET";

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => throw new NotImplementedException();

        private static DiagnosticDescriptor WarningRule = new DiagnosticDescriptor(DiagnosticAnalyzerID, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);
        private static DiagnosticDescriptor ErrorRule = new DiagnosticDescriptor(DiagnosticAnalyzerID, Title, MessageFormat, Category, DiagnosticSeverity.Error, isEnabledByDefault: true, description: Description);

        protected override ITypeSymbol GetExplicitKeyType(ImmutableArray<ITypeSymbol> typeArgs)
        {
            return typeArgs.Length == 1 ? null : typeArgs[0];
        }

        protected override DiagnosticDescriptor GetRule(DiagnosticSeverity severity)
        {
            switch (severity)
            {
                case DiagnosticSeverity.Error:
                    return ErrorRule;
                case DiagnosticSeverity.Warning:
                    return WarningRule;
                default:
                    return null;
            }
        }

        protected override string GetTargetAssembly() => "DBFilesClient.NET";

        protected override string GetTargetCollectionType() => "StorageList";
    }
}
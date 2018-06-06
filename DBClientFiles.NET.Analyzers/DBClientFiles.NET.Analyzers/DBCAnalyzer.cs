using System.Collections.Immutable;
using System.Linq;
using DBClientFiles.NET.Analyzers.Extenstions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DBClientFiles.NET.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class DBCAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "DBCAnalyzer";

        // You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
        // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/Localizing%20Analyzers.md for more on localization
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        private const string Category = "DBClientFiles.NET";

        private static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext analysisContext)
        {
            analysisContext.RegisterSemanticModelAction(HandleSemantic);
        }

        private static void HandleSemantic(SemanticModelAnalysisContext context)
        {
            var semanticModel = context.SemanticModel;
            var syntaxTree = semanticModel.SyntaxTree;

            var typeDeclarations = syntaxTree.GetRoot().EnumerateNodes<ClassDeclarationSyntax>();
            foreach (var classInfo in typeDeclarations)
            {
                ISymbol indexProperty = null;
                ISymbol indexField = null;

                #region Find [Index] property
                var properties = classInfo.EnumerateNodes<PropertyDeclarationSyntax>().ToArray();

                foreach (var propInfo in properties)
                {
                    var propSymbol = semanticModel.GetDeclaredSymbol(propInfo, context.CancellationToken);
                    foreach (var attrInfo in propSymbol.GetAttributes())
                    {
                        INamedTypeSymbol attributeClass = attrInfo.AttributeClass;
                        if (attributeClass.ContainingAssembly.Name != "DBClientFiles.NET")
                            continue;

                        if (attributeClass.Name == "IndexAttribute")
                        {
                            indexProperty = propSymbol;
                            break;
                        }
                    }
                }

                if (indexProperty == null && properties.Length != 0)
                    indexProperty = semanticModel.GetDeclaredSymbol(properties.First());
                #endregion

                #region Find [Index] field
                var fields = classInfo.EnumerateNodes<FieldDeclarationSyntax>().ToArray();

                foreach (var fieldInfo in fields)
                {
                    var fieldSymbol = semanticModel.GetDeclaredSymbol(fieldInfo, context.CancellationToken);
                    foreach (var attrInfo in fieldSymbol.GetAttributes())
                    {
                        INamedTypeSymbol attributeClass = attrInfo.AttributeClass;
                        if (attributeClass.ContainingAssembly.Name != "DBClientFiles.NET")
                            continue;

                        if (attributeClass.Name == "IndexAttribute")
                        {
                            indexField = fieldSymbol;
                            break;
                        }
                    }
                }

                if (indexField == null && fields.Length != 0)
                    indexField = semanticModel.GetDeclaredSymbol(fields.First());
                #endregion

                if (indexProperty != null || indexField != null)
                    StructureStore.Instance.Cache(semanticModel.GetTypeInfo(classInfo, context.CancellationToken), indexField, indexProperty);

                foreach (var instanciationSyntax in classInfo.EnumerateNodes<ObjectCreationExpressionSyntax>())
                {
                    var argumentSymbols = instanciationSyntax.ArgumentList.Arguments.Select(
                        a => semanticModel.GetSymbolInfo(a.Expression, context.CancellationToken)).ToArray();

                    var instanciationTypeInfo = semanticModel.GetTypeInfo(instanciationSyntax, context.CancellationToken);
                    if (!(instanciationTypeInfo.Type is INamedTypeSymbol instanceType))
                        continue;

                    if (instanceType.ContainingAssembly.Name != "DBClientFiles.NET")
                        continue;
                    
                    var typeArgs = instanceType.TypeArguments.ToArray();
                    var matchedIndexField = StructureStore.Instance.GetIndexField(typeArgs.Last());
                    var matchedPropertyField = StructureStore.Instance.GetIndexProperty(typeArgs.Last());

                    ITypeSymbol expectedKeyType = null;
                    ISymbol selectedIndexMember = matchedPropertyField;
                    if (instanceType.Name == "StorageList" || instanceType.Name == "StorageEnumerable")
                        expectedKeyType = typeArgs.First();
                    else if (instanceType.Name == "StorageDictionary")
                        expectedKeyType = (typeArgs.Length == 3 ? typeArgs[1] : typeArgs[0]);
                    else
                        continue;

                    if (argumentSymbols.Length == 2)
                    {
                        var storageOptions = (INamedTypeSymbol)argumentSymbols[1].Symbol;
                    }

                    if (matchedPropertyField != null && matchedIndexField != null)
                    {

                    }
                    else if (matchedPropertyField != null)
                    {
                        
                    }
                    else if (matchedIndexField != null)
                    {

                    }
                }
            }
        }

        private static void AnalyzeSymbol(SymbolAnalysisContext context)
        {
            // TODO: Replace the following code with your own analysis, generating Diagnostic objects for any issues you find
            var namedTypeSymbol = (INamedTypeSymbol)context.Symbol;

            // Find just those named type symbols with names containing lowercase letters.
            if (namedTypeSymbol.Name.ToCharArray().Any(char.IsLower))
            {
                // For all such symbols, produce a diagnostic.
                var diagnostic = Diagnostic.Create(Rule, namedTypeSymbol.Locations[0], namedTypeSymbol.Name);

                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}

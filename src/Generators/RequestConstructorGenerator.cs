using MaSch.CodeAnalysis.CSharp.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using System.Threading;

namespace Rtfx.Generators;

[Generator]
public class RequestConstructorGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var recordDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: Predicate,
                transform: Transform)
            .Where(static x => x is not null);

        context.RegisterSourceOutput(recordDeclarations, Generate);
    }

    private static bool Predicate(SyntaxNode node, CancellationToken cancellationToken)
    {
        return node is RecordDeclarationSyntax r
            && r.Identifier.ValueText.EndsWith("Request")
            && r.Modifiers.Any(SyntaxKind.PartialKeyword)
            && !r.Members.OfType<ConstructorDeclarationSyntax>().Any(x => x.ParameterList.Parameters.Count == 0);
    }

    private static RecordData Transform(GeneratorSyntaxContext context, CancellationToken ct)
    {
        var recordDeclaration = (RecordDeclarationSyntax)context.Node;
        if (context.SemanticModel.GetDeclaredSymbol(recordDeclaration) is not INamedTypeSymbol typeSymbol)
            return null;

        return new RecordData(
            symbol: typeSymbol,
            parameters: recordDeclaration.ParameterList.Parameters.Select(p => context.SemanticModel.GetTypeInfo(p.Type).Type).Where(x => x is not null).ToArray()!);
    }

    private static void Generate(SourceProductionContext context, RecordData data)
    {
        var sb = SourceBuilder.Create();

        sb.EnsurePreviousLineEmpty()
          .AppendLine("#nullable enable")
          .AppendLine()
          .Append(Namespace(data.Symbol.ContainingNamespace.ToString()), n =>
          {
              var record = Record(data.Symbol.Name).AsPartial();
              foreach (var gp in data.Symbol.TypeParameters)
                  record.WithGenericParameter(gp.Name);

              n.Append(record, r =>
              {
                  var constructor = Constructor().AsPublic()
                      .WithCodeAttribute("global::System.ObsoleteAttribute", a => a.WithParameter("\"This constructor is only intended for use with FastEndpoints. Do not call this manually.\""))
                      .CallsThis(sc =>
                      {
                          foreach (var parameter in data.Parameters)
                              sc.WithParameter($"default({parameter.ToUsageString()})!");
                      });

                  r.Append(constructor, _ => { });
              });
          });

        context.AddSource($"{data.Symbol.Name}.g.cs", sb.ToSourceText());
    }

    private sealed class RecordData
    {
        public RecordData(INamedTypeSymbol symbol, ITypeSymbol[] parameters)
        {
            Symbol = symbol;
            Parameters = parameters;
        }

        public INamedTypeSymbol Symbol { get; set; }
        public ITypeSymbol[] Parameters { get; set; }
    }
}

using MaSch.CodeAnalysis.CSharp.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using System.Threading;

namespace Rtfx.Generators;

[Generator]
public class CustomErrorDefinitionGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(PostInitializationOutput);

        var recordDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: Predicate,
                transform: Transform)
            .Where(static x => x is not null);

        context.RegisterSourceOutput(recordDeclarations, Generate);
    }

    private static bool Predicate(SyntaxNode node, CancellationToken cancellationToken)
    {
        return node is ClassDeclarationSyntax c
            && c.Identifier.ValueText == "ErrorMessages";
    }

    private static INamedTypeSymbol Transform(GeneratorSyntaxContext context, CancellationToken ct)
    {
        var recordDeclaration = (ClassDeclarationSyntax)context.Node;
        if (context.SemanticModel.GetDeclaredSymbol(recordDeclaration) is not INamedTypeSymbol typeSymbol)
            return null;

        return typeSymbol;
    }

    [SuppressMessage("Critical Code Smell", "S2479:Whitespace and control characters in string literals should be explicit", Justification = "False positive")]
    [SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1118:Parameter should not span multiple lines", Justification = "Fine in this method")]
    private static void PostInitializationOutput(IncrementalGeneratorPostInitializationContext context)
    {
        context.AddSource(
            "NoValueAttribute.g.cs",
            """
            using System;
            namespace Rtfx.Server;

            [AttributeUsage(AttributeTargets.Parameter)]
            public class NoValueAttribute : Attribute { }
            """);
    }

    private static void Generate(SourceProductionContext context, INamedTypeSymbol errorMessagesSymbol)
    {
        var builder = SourceBuilder.Create();

        builder.AppendLine("#nullable enable").AppendLine();

        var methods = errorMessagesSymbol.GetMembers().OfType<IMethodSymbol>().Where(x => x.MethodKind == MethodKind.Ordinary).ToArray();
        const string ErrorsNamespace = "Rtfx.Server.Models.Errors";

        builder.Append(Namespace("Rtfx.Server"), n =>
        {
            n.Append(Class("Errors").AsPublicStaticPartial(), c =>
            {
#pragma warning disable S3267 // Loops should be simplified with "LINQ" expressions
                foreach (var method in methods)
                {
                    var typeName = $"global::{ErrorsNamespace}.{method.Name}Error";
                    c.Append(
                        Property(typeName, method.Name).AsPublicStatic().AsReadOnly().AsExpression(),
                        g => g.Append($"{typeName}.Instance"));
                }
#pragma warning restore S3267 // Loops should be simplified with "LINQ" expressions
            });
        });

        builder.Append(Namespace(ErrorsNamespace), n =>
        {
            foreach (var method in methods)
            {
                BuildErrorClass(n, method);
            }
        });

        context.AddSource($"Errors.g.cs", builder.ToSourceText());
    }

    private static void BuildErrorClass(INamespaceBuilder builder, IMethodSymbol getMessageMethod)
    {
        var errorCode = getMessageMethod.Name;
        var name = $"{errorCode}Error";

        builder.Append(Class(name).AsPublicSealedPartial().DerivesFrom("global::Rtfx.Server.Models.ErrorDefinition"), c =>
        {
            c.Append(Field($"{name}?", "_instance").AsPrivateStatic());
            c.Append(Constructor().AsPrivate().CallsBase(s => s.WithParameter($"\"{errorCode}\"")), ctor => { });
            c.Append(
                Property(name, "Instance").AsPublicStatic().AsReadOnly().AsExpression(),
                g => g.Append($"_instance ??= new {name}()"));
            BuildGetMessageMethod(c, getMessageMethod);
            BuildGetErrorMethod(c, getMessageMethod);
        });
    }

    private static void BuildGetMessageMethod(IClassBuilder builder, IMethodSymbol getMessageMethod)
    {
        var methodDefinition = Method("string", "GetMessage").AsPublic();
        foreach (var p in getMessageMethod.Parameters)
            methodDefinition.WithParameter(p.Type.ToUsageString(), p.Name);

        builder.Append(
            methodDefinition.AsExpression(),
            m => m.Append($"global::Rtfx.Server.ErrorMessages.{getMessageMethod.Name}({string.Join(", ", getMessageMethod.Parameters.Select(x => x.Name))})"));
    }

    private static void BuildGetErrorMethod(IClassBuilder builder, IMethodSymbol getMessageMethod)
    {
        var methodDefinition = Method("global::FluentValidation.Results.ValidationFailure", "GetError").AsPublic();
        foreach (var p in getMessageMethod.Parameters)
            methodDefinition.WithParameter(p.Type.ToUsageString(), p.Name);

        builder.Append(methodDefinition, m =>
        {
            m.AppendLine("return CreateFailure(");
            using (m.Indent())
            {
                m.AppendLine($"GetMessage({string.Join(", ", getMessageMethod.Parameters.Select(x => x.Name))}),");

                var valueParameters = getMessageMethod.Parameters.Where(x => !x.GetAttributes().Any(y => y.AttributeClass.Name == "NoValueAttribute")).ToArray();
                if (valueParameters.Length == 0)
                {
                    m.AppendLine("null);");
                }
                else if (valueParameters.Length == 1)
                {
                    m.AppendLine($"{valueParameters[0].Name});");
                }
                else
                {
                    m.AppendLine("new")
                     .AppendLine("{");
                    using (m.Indent())
                    {
#pragma warning disable S3267 // Loops should be simplified with "LINQ" expressions
                        foreach (var p in valueParameters)
                            m.AppendLine($"{GetPascalCase(p.Name)} = {p.Name},");
#pragma warning restore S3267 // Loops should be simplified with "LINQ" expressions
                    }

                    m.AppendLine("});");
                }
            }
        });
    }

    private static string GetPascalCase(string camelCase)
    {
        return $"{camelCase[0].ToString().ToUpperInvariant()}{camelCase.Substring(1)}";
    }
}
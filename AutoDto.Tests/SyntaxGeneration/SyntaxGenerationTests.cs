using AutoDto.SourceGen.DiagnosticMessages.Errors;
using AutoDto.Tests.TestHelpers;
using AutoDto.Tests.TestHelpers.CodeBuilder.Builders;
using AutoDto.Tests.TestHelpers.CodeBuilder.Elements;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;

namespace AutoDto.Tests.SyntaxGeneration;

public class SyntaxGenerationTests : BaseUnitTest
{
    private record SyntaxData(string Namespace, string ClassName, SyntaxKind Access);

    [Fact]
    public void DtoClassNotPartial()
    {
        var bl =
            new ClassBuilder("MyBl")
            .SetNamespace(BlNamespace)
            .AddMember(CommonProperties.Id_Int)
            .Build();

        var dto =
            new DtoClassBuilder("MyDto", DtoClassBuilder.DtoAttributeType.DtoFrom, bl)
            .SetNamespace(DtoNamespace)
            .SetPartial(false)
            .Build();

        RunWithAssert(new[] { bl, dto }, DoAssert);
        void DoAssert(Compilation compilation, ImmutableArray<Diagnostic> msgs)
        {
            Assert.Single(msgs);
            var msg = msgs[0];

            var exp = new DtoNotPartialError(dto.Name);
            Assert.Equal(DiagnosticSeverity.Error, msg.Severity);
            Assert.Equal(exp.Id, msg.Id);

            var generated = SyntaxChecker.FindAllClassDeclarationsByName(compilation, dto.Name)
                .Skip(1)
                .FirstOrDefault();

            Assert.Null(generated);
        }
    }

    [Theory]
    [InlineData(Visibility.Public)]
    [InlineData(Visibility.Internal)]
    public void OneFileDtoSyntaxTest(Visibility visibility)
    {
        var bl =
            new ClassBuilder("MyBl")
            .SetNamespace(BlNamespace)
            .AddMember(CommonProperties.Id_Int)
            .AddMember(CommonProperties.Name)
            .Build();

        var dto =
            new DtoClassBuilder("MyDto", DtoClassBuilder.DtoAttributeType.DtoFrom, bl)
            .SetNamespace(DtoNamespace)
            .SetAccessor(visibility)
            .Build();

        RunWithAssert(new[] { bl, dto }, DoAssert);
        void DoAssert(Compilation compilation, ImmutableArray<Diagnostic> msgs)
        {
            Assert.Empty(msgs);
            TestOneFile(compilation, dto);
        }
    }

    [Theory]
    [InlineData(Visibility.Public, Visibility.Public)]
    [InlineData(Visibility.Public, Visibility.Internal)]
    [InlineData(Visibility.Internal, Visibility.Public)]
    [InlineData(Visibility.Internal, Visibility.Internal)]
    public void ManyDtosInOneFileSyntaxTest(Visibility access1, Visibility access2)
    {
        var bl =
            new ClassBuilder("MyBl")
            .SetNamespace(BlNamespace)
            .AddMember(CommonProperties.Id_Int)
            .AddMember(CommonProperties.Name)
            .Build();

        var dto1 =
            new DtoClassBuilder("MyDto1", DtoClassBuilder.DtoAttributeType.DtoFrom, bl)
            .SetNamespace(DtoNamespace)
            .SetAccessor(access1)
            .Build();

        var dto2 =
            new DtoClassBuilder("MyDto2", DtoClassBuilder.DtoAttributeType.DtoFrom, bl)
            .SetNamespace(DtoNamespace)
            .SetAccessor(access2)
            .Build();

        RunWithAssert(new[] { bl, dto1, dto2 }, DoAssert);
        void DoAssert(Compilation compilation, ImmutableArray<Diagnostic> msgs)
        {
            Assert.Empty(msgs);

            TestOneFile(compilation, dto1);
            TestOneFile(compilation, dto2);
        }
    }

    private void TestOneFile(Compilation compilation, ClassElement dto)
    {
        var generated = SyntaxChecker.FindAllClassDeclarationsByName(compilation, dto.Name)
                .Skip(1)
                .Single();

        var accessKeyword = Visibility2SyntaxKind(dto.Visibility);

        var expected = new SyntaxData(dto.Namespace, dto.Name, accessKeyword);

        CheckOneFile(generated.SyntaxTree, expected);
    }

    private SyntaxKind Visibility2SyntaxKind(Visibility visibility)
    {
        return visibility switch
        {
            Visibility.Public => SyntaxKind.PublicKeyword,
            Visibility.Internal => SyntaxKind.InternalKeyword,
            _ => throw new NotImplementedException()
        };
    }

    private void CheckOneFile(SyntaxTree syntaxTree, SyntaxData expectedData)
    {
        var ns = AssertNamespace(syntaxTree, expectedData.Namespace);

        var cl = AssertClass(ns, expectedData.ClassName, expectedData.Access);

        AssertProperties(cl);
    }

    private BaseNamespaceDeclarationSyntax AssertNamespace(SyntaxTree syntaxTree, string nameSpace)
    {
        var namespaces = syntaxTree.GetRoot().DescendantNodesAndSelf().OfType<BaseNamespaceDeclarationSyntax>().ToList();
        Assert.Single(namespaces);
        var ns = namespaces[0];
        Assert.Equal(nameSpace, ns.Name.ToString());

        return ns;
    }

    private ClassDeclarationSyntax AssertClass(BaseNamespaceDeclarationSyntax namespaceSyntax, string className, SyntaxKind access)
    {
        var types = namespaceSyntax.DescendantNodesAndSelf().OfType<ClassDeclarationSyntax>();
        Assert.Single(types);
        var classSyntax = types.First();

        Assert.Equal(className, classSyntax.Identifier.Text);

        var modifiers = classSyntax.Modifiers;

        Assert.Equal(2, modifiers.Count);

        var publ = modifiers[0];
        var part = modifiers[1];

        Assert.Equal(access, publ.Kind());
        Assert.Equal(SyntaxKind.PartialKeyword, part.Kind());

        return classSyntax;
    }

    private void AssertProperties(ClassDeclarationSyntax classDeclaration)
    {
        foreach (var member in classDeclaration.Members)
        {
            Assert.IsType<PropertyDeclarationSyntax>(member);

            SyntaxChecker.AssertOnePropSyntax((PropertyDeclarationSyntax)member);
        }
    }
}


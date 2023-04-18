using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoDto.Tests.SyntaxGeneration.Models;
using static AutoDto.Tests.TestHelpers.DtoCodeCreator;
using AutoDto.Tests.TestHelpers;
using AutoDto.SourceGen.DiagnosticMessages.Errors;

namespace AutoDto.Tests.SyntaxGeneration;

public class SyntaxGenerationTests : BaseUnitTest
{
    private record SyntaxData(string Namespace, string ClassName, SyntaxKind Access);

    [Fact]
    public void DtoClassNotPatrial()
    {
        var type = typeof(SimpleType);
        var attr = DtoCreator.GetDtoFromAttr(type);

        var code = $@"
using {type.Namespace};
using {attr.nameSpace};

namespace {DtoCodeCreator.DtoTypNamespace};

{attr.definition}
public class MyDto
{{ }}
";

        var (compilations, msgs) = Generator.RunWithMsgs(code);

        Assert.Single(compilations.SyntaxTrees);

        Assert.Single(msgs);
        var msg = msgs[0];

        var exp = new DtoNotPartialError("MyDto");
        Assert.Equal(DiagnosticSeverity.Error, msg.Severity);
        Assert.Equal(exp.Id, msg.Id);
    }

    [Theory]
    [InlineData(typeof(SimpleType), SyntaxKind.PublicKeyword)]
    [InlineData(typeof(SimpleType), SyntaxKind.InternalKeyword)]
    [InlineData(typeof(EmptyType), SyntaxKind.PublicKeyword)]
    public void OneFileDtoSyntaxTest(Type type, SyntaxKind access)
    {
        var attr = DtoCreator.GetDtoFromAttr(type);
        var data = new SyntaxData(DtoCodeCreator.DtoTypNamespace, type.Name + "Dto", access);
        var dtoData1 = new DtoData(type, Setup.RelationStrategy.None, data.ClassName, null, data.Access);

        var compilation = RunForDtos(dtoData1);

        var trees = compilation.SyntaxTrees;

        Assert.Equal(2, trees.Count());

        CheckOneFile(trees.Last(), data);
    }

    [Theory]
    [InlineData(SyntaxKind.PublicKeyword, SyntaxKind.PublicKeyword)]
    [InlineData(SyntaxKind.PublicKeyword, SyntaxKind.InternalKeyword)]
    [InlineData(SyntaxKind.InternalKeyword, SyntaxKind.PublicKeyword)]
    [InlineData(SyntaxKind.InternalKeyword, SyntaxKind.InternalKeyword)]
    public void ManyDtosInOneFileSyntaxTest(SyntaxKind access1, SyntaxKind access2)
    {
        var type = typeof(SimpleType);
        var attr = DtoCreator.GetDtoFromAttr(type);
        var data = new[]
        {
            new SyntaxData(DtoCodeCreator.DtoTypNamespace, type.Name + "Dto1", access1),
            new SyntaxData(DtoCodeCreator.DtoTypNamespace, type.Name + "Dto2", access2)
        };

        var dtoData1 = new DtoData(type, Setup.RelationStrategy.None, data[0].ClassName, null, data[0].Access);
        var dtoData2 = new DtoData(type, Setup.RelationStrategy.None, data[1].ClassName, null, data[1].Access);

        var compilation = RunForDtos(dtoData1, dtoData2);

        var trees = compilation.SyntaxTrees.ToList();

        Assert.Equal(3, trees.Count());

        for (int i = 1; i < trees.Count(); i++)
            CheckOneFile(trees[i], data[i - 1]);
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


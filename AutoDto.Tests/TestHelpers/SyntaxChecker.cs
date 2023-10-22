using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Microsoft.CodeAnalysis.CSharp;
using System.Diagnostics.Metrics;

namespace AutoDto.Tests.TestHelpers;

public class SyntaxChecker
{
    public enum TypeType
    {
        Simple,
        Generic,
        Array
    }

    [DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
    public class TypeDescriptor
    {
        public TypeDescriptor(Type type)
        {
            Namespace = type.Namespace;
            Name = type.Name;

            Type = TypeType.Simple;
            if (type.IsArray)
            {
                Type = TypeType.Array;
                GenericArgs = new[] { new TypeDescriptor(type.GetElementType()) };
            }
            if (type.IsGenericType)
            {
                Type = TypeType.Generic;
                GenericArgs = type.GenericTypeArguments.Select(x => new TypeDescriptor(x)).ToArray();
            }
        }

        public TypeDescriptor(string nameSpace, string name, TypeType type, TypeDescriptor[] genericArgs)
        {
            Namespace = nameSpace;
            Name = name;
            Type = type;
            GenericArgs = genericArgs;
        }

        public string Namespace { get; set; }
        public string Name { get; set; }
        public TypeType Type { get; set; }
        public TypeDescriptor[] GenericArgs { get; set; }

        public string GetDebuggerDisplay()
        {
            return $"{Type}: {Namespace}.{Name}";
        }
    }

    [DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
    public class PropertyDescriptor 
    {
        public PropertyDescriptor(PropertyInfo propertyInfo) 
            : this(new TypeDescriptor(propertyInfo.PropertyType), propertyInfo.Name)
        { 
        }

        public PropertyDescriptor(Type returnType, string name) 
            : this (new TypeDescriptor(returnType), name) 
        { 
        }

        public PropertyDescriptor(TypeDescriptor type, string name)
        {
            Type = type;
            Name = name;
        }

        public TypeDescriptor Type { get; }
        public string Name { get; }

        private string GetDebuggerDisplay()
        {
            return $"{Type.GetDebuggerDisplay()} {Name}";
        }
    }

    public ClassDeclarationSyntax FindClassByName(Compilation compilation, string className)
    {
        var roots = compilation.SyntaxTrees.Select(x => x.GetRoot()).ToList();

        var @class = roots
            .Skip(1)
            .SelectMany(x => x.DescendantNodesAndSelf().OfType<ClassDeclarationSyntax>())
            .SingleOrDefault(x => x.Identifier.Text == className);

        return @class;
    }

    public IEnumerable<ClassDeclarationSyntax> FindAllClassDeclarationsByName(Compilation compilation, string className)
    {
        var roots = compilation.SyntaxTrees.Select(x => x.GetRoot()).ToList();

        var classes = roots
            //.Skip(1)
            .SelectMany(x => x.DescendantNodesAndSelf().OfType<ClassDeclarationSyntax>())
            .Where(x => x.Identifier.Text == className);

        return classes;
    }

    public void AssertOnePropSyntax(PropertyDeclarationSyntax property)
    {
        var modifiers = property.Modifiers;

        Assert.Single(modifiers);

        Assert.Equal(SyntaxKind.PublicKeyword, modifiers[0].Kind());

        var getSet = property.AccessorList.Accessors;

        Assert.Equal(2, getSet.Count);

        Assert.Equal(SyntaxKind.GetAccessorDeclaration, getSet[0].Kind());
        Assert.Equal(SyntaxKind.SetAccessorDeclaration, getSet[1].Kind());
    }

    public void TestOneClassDeclaration(ClassDeclarationSyntax classDeclaration, IEnumerable<PropertyDescriptor> expectedProps)
    {
        Assert.NotNull(classDeclaration);

        var members = classDeclaration.Members.Select(x => (PropertyDeclarationSyntax)x).ToList();

        Assert.Equal(expectedProps.Count(), members.Count);

        foreach (var expectedProp in expectedProps)
        {
            var generatedProp = members.Find(x => x.Identifier.Text == expectedProp.Name);
            Assert.NotNull(generatedProp);

            AssertOnePropSyntax(generatedProp);

            CheckTypeSyntax(generatedProp.Type, expectedProp.Type);
        }
    }

    private void ChectTypeNameAndNamespace(QualifiedNameSyntax nameSyntax, TypeDescriptor expType)
    {
        Assert.Equal(expType.Namespace, nameSyntax.Left.ToFullString().Trim());

        var genericSymbolindex = expType.Name.IndexOf('`');
        var name = expType.Name;
        if (genericSymbolindex != -1)
            name = name.Substring(0, genericSymbolindex);

        Assert.Equal(name, nameSyntax.Right.Identifier.Text);
    }

    private void CheckQualifiedNameSyntax(QualifiedNameSyntax nameSyntax, TypeDescriptor expType)
    {
        ChectTypeNameAndNamespace(nameSyntax, expType);

        if (nameSyntax.Right is not GenericNameSyntax genericSyntax)
            return;

        Assert.Equal(TypeType.Generic, expType.Type);

        var args = genericSyntax.TypeArgumentList.Arguments;

        for (int i = 0; i < args.Count; i++)
            CheckTypeSyntax(args[i], expType.GenericArgs[i]);
    }

    private void CheckArrayTypeSyntax(ArrayTypeSyntax arraySyntax, TypeDescriptor expType)
    {
        Assert.Equal(TypeType.Array, expType.Type);

        var expArrayElement = expType.GenericArgs[0];
        var actArrayElement = arraySyntax.ElementType;

        CheckTypeSyntax(actArrayElement, expArrayElement);
    }

    private void CheckTypeSyntax(TypeSyntax type, TypeDescriptor expType)
    {
        if (type is QualifiedNameSyntax qualified)
            CheckQualifiedNameSyntax(qualified, expType);
        else if (type is ArrayTypeSyntax arrayType)
            CheckArrayTypeSyntax(arrayType, expType);
        else
            throw new NotImplementedException();
    }
}


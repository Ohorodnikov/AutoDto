using AutoDto.SourceGen.DiagnosticMessages.Errors;
using AutoDto.SourceGen.DiagnosticMessages.Warnings;
using AutoDto.Tests.SourceGeneration.Models.HierarchyTestModels;
using AutoDto.Tests.TestHelpers.CodeBuilder.Builders;
using AutoDto.Tests.TestHelpers.CodeBuilder.Elements;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace AutoDto.Tests.SourceGeneration;

public class DtoWithHierarchyTests : BaseUnitTest
{
    [Fact]
    public void DtoHasEmptyParent()
    {
        var baseDtoNamespace = DtoNamespace + ".Base";
        var blMembers = new List<Member>
        {
            CommonProperties.Id_Int,
            new PropertyBuilder("Name1", typeof(string)).Build(),
            new PropertyBuilder("Name2", typeof(string)).Build(),
            new PropertyBuilder("Name3", typeof(string)).Build(),
            new PropertyBuilder("Name4", typeof(string)).Build(),
        };

        var blClass =
            new ClassBuilder("BlType")
            .SetNamespace(BlNamespace)
            .AddMembers(blMembers)
            .Build();

        var baseDtoClass =
            new ClassBuilder("BaseEmptyDto")
            .SetNamespace(baseDtoNamespace)
            .Build();

        var dtoClass =
            new DtoClassBuilder("DtoName", DtoClassBuilder.DtoAttributeType.DtoFrom, blClass)
            .SetNamespace(DtoNamespace)
            .AddBase(baseDtoClass)
            .Build();

        RunWithAssert(new[] { blClass, baseDtoClass, dtoClass }, DoAssert);

        void DoAssert(Compilation compilation, ImmutableArray<Diagnostic> msgs)
        {
            Assert.Empty(msgs);

            var generatedClass = SyntaxChecker.FindAllClassDeclarationsByName(compilation, dtoClass.Name)
                    .Skip(1) //skip declaration to get only generated
                    .Single();

            SyntaxChecker.TestOneClassDeclaration(generatedClass, Member2PropDescriptor(blMembers));
        }
    }

    [Fact]
    public void DtoHasParentWithProps_WithoutConflictBlNamesTest()
    {
        var baseDtoNamespace = DtoNamespace + ".Base";
        var blMembers = new List<Member>
        {
            CommonProperties.Id_Int,
            new PropertyBuilder("Name1", typeof(string)).Build(),
            new PropertyBuilder("Name2", typeof(string)).Build(),
            new PropertyBuilder("Name3", typeof(string)).Build(),
            new PropertyBuilder("Name4", typeof(string)).Build(),
        };

        var blClass =
            new ClassBuilder("BlType")
            .SetNamespace(BlNamespace)
            .AddMembers(blMembers)
            .Build();

        var baseDtoClass1 =
            new ClassBuilder("BaseEmptyDto")
            .SetNamespace(baseDtoNamespace)
            .Build();

        var baseDtoClass2 =
            new ClassBuilder("BaseDtoWithoutConflictProps")
            .SetNamespace(baseDtoNamespace)
            .AddBase(baseDtoClass1)
            .AddMember(new PropertyBuilder("SomeNotConflictProp", typeof(string)).Build())
            .Build();

        var dtoClass =
            new DtoClassBuilder("DtoName", DtoClassBuilder.DtoAttributeType.DtoFrom, blClass)
            .SetNamespace(DtoNamespace)
            .AddBase(baseDtoClass2)
            .Build();

        RunWithAssert(new[] { blClass, baseDtoClass1, baseDtoClass2, dtoClass }, DoAssert);

        void DoAssert(Compilation compilation, ImmutableArray<Diagnostic> msgs)
        {
            Assert.Empty(msgs);

            var generatedClass = SyntaxChecker.FindAllClassDeclarationsByName(compilation, dtoClass.Name)
                    .Skip(1) //skip declaration to get only generated
                    .Single();

            SyntaxChecker.TestOneClassDeclaration(generatedClass, Member2PropDescriptor(blMembers));
        }
    }

    [Fact]
    public void DtoHasParentWithProps_WithConflictBlNames_Warning_Test()
    {
        var baseDtoNamespace = DtoNamespace + ".Base";
        var conflictPropName = "ConflictProp";
        var notConflictedMembers = new List<Member>
        {
            CommonProperties.Id_Int,
            new PropertyBuilder("Name1", typeof(string)).Build(),
            new PropertyBuilder("Name2", typeof(string)).Build(),
            new PropertyBuilder("Name3", typeof(string)).Build(),
            new PropertyBuilder("Name4", typeof(string)).Build(),
        };

        var blClass =
            new ClassBuilder("BlType")
            .SetNamespace(BlNamespace)
            .AddMembers(notConflictedMembers)
            .AddMember(new PropertyBuilder(conflictPropName, typeof(string)).Build())
            .Build();

        var baseDtoClass1 =
            new ClassBuilder("BaseEmptyDto")
            .SetNamespace(baseDtoNamespace)
            .Build();

        var baseDtoClass2 =
            new ClassBuilder("BaseDtoWithConflictPropName")
            .SetNamespace(baseDtoNamespace)
            .AddBase(baseDtoClass1)
            .AddMember(new PropertyBuilder(conflictPropName, typeof(string)).Build())
            .Build();

        var dtoClass =
            new DtoClassBuilder("DtoName", DtoClassBuilder.DtoAttributeType.DtoFrom, blClass)
            .SetNamespace(DtoNamespace)
            .AddBase(baseDtoClass2)
            .Build();

        RunWithAssert(new[] { blClass, baseDtoClass1, baseDtoClass2, dtoClass }, DoAssert);

        void DoAssert(Compilation compilation, ImmutableArray<Diagnostic> msgs)
        {
            Assert.Single(msgs);
            var msg = msgs[0];

            var expMsg = new PropertyConflictWarning(conflictPropName, dtoClass.Name);

            Assert.Equal(DiagnosticSeverity.Warning, msg.Severity);
            Assert.Equal(expMsg.Id, msg.Id);

            var generatedClass = SyntaxChecker.FindAllClassDeclarationsByName(compilation, dtoClass.Name)
                    .Skip(1) //skip declaration to get only generated
                    .Single();

            SyntaxChecker.TestOneClassDeclaration(generatedClass, Member2PropDescriptor(notConflictedMembers));
        }
    }

    [Fact]
    public void DtoHasParentWithMethods_WithoutConflictBlNamesTest()
    {
        var baseDtoNamespace = DtoNamespace + ".Base";
        var blMembers = new List<Member>
        {
            CommonProperties.Id_Int,
            new PropertyBuilder("Name1", typeof(string)).Build(),
            new PropertyBuilder("Name2", typeof(string)).Build(),
            new PropertyBuilder("Name3", typeof(string)).Build(),
            new PropertyBuilder("Name4", typeof(string)).Build(),
        };

        var blClass =
            new ClassBuilder("BlType")
            .SetNamespace(BlNamespace)
            .AddMembers(blMembers)
            .Build();

        var baseDtoClass1 =
            new ClassBuilder("BaseEmptyDto")
            .SetNamespace(baseDtoNamespace)
            .Build();

        var baseDtoClass2 =
            new ClassBuilder("BaseDtoWithMethod")
            .SetNamespace(baseDtoNamespace)
            .AddBase(baseDtoClass1)
            .AddMember(new MethodBuilder("Do", typeof(void)).Build())
            .Build();

        var dtoClass =
            new DtoClassBuilder("DtoName", DtoClassBuilder.DtoAttributeType.DtoFrom, blClass)
            .SetNamespace(DtoNamespace)
            .AddBase(baseDtoClass2)
            .Build();

        RunWithAssert(new[] { blClass, baseDtoClass1, baseDtoClass2, dtoClass }, DoAssert);

        void DoAssert(Compilation compilation, ImmutableArray<Diagnostic> msgs)
        {
            Assert.Empty(msgs);

            var generatedClass = SyntaxChecker.FindAllClassDeclarationsByName(compilation, dtoClass.Name)
                    .Skip(1) //skip declaration to get only generated
                    .Single();

            SyntaxChecker.TestOneClassDeclaration(generatedClass, Member2PropDescriptor(blMembers));
        }
    }

    [Fact]
    public void DtoHasParentWithInternallyReservedName()
    {
        var baseDtoNamespace = DtoNamespace + ".Base";
        var blMembers = new List<Member>
        {
            CommonProperties.Id_Int,
            new PropertyBuilder("Name1", typeof(string)).Build(),
            new PropertyBuilder("Name2", typeof(string)).Build(),
            new PropertyBuilder("Name3", typeof(string)).Build(),
            new PropertyBuilder("Name4", typeof(string)).Build(),
        };

        var blClass =
            new ClassBuilder("BlType")
            .SetNamespace(BlNamespace)
            .AddMembers(blMembers)
            .Build();

        var baseDtoClass =
            new ClassBuilder("BaseDtoWithSpecialName")
            .SetNamespace(baseDtoNamespace)
            .AddMember(new FieldBuilder("get_Name1", typeof(string)).Build())
            .Build();

        var dtoClass =
            new DtoClassBuilder("DtoName", DtoClassBuilder.DtoAttributeType.DtoFrom, blClass)
            .SetNamespace(DtoNamespace)
            .AddBase(baseDtoClass)
            .Build();

        var inputClasses = new[] { blClass, baseDtoClass, dtoClass };

        RunWithAssert(inputClasses, DoAssert);

        void DoAssert(Compilation compilation, ImmutableArray<Diagnostic> msgs)
        {
            Assert.Single(msgs);

            var msg = msgs[0];
            var expId = new ReservedMemberConflicError("", dtoClass.Name).Id;
            Assert.Equal(DiagnosticSeverity.Error, msg.Severity);
            Assert.Equal(expId, msg.Id);

            Assert.Equal(inputClasses.Length, compilation.SyntaxTrees.Count());
        }
    }

    [Fact]
    public void DtoHasParentWithMethods_WithConflictBlNames_Error_Test()
    {
        var baseDtoNamespace = DtoNamespace + ".Base";
        var blMembers = new List<Member>
        {
            CommonProperties.Id_Int,
            new PropertyBuilder("Name1", typeof(string)).Build(),
            new PropertyBuilder("Name2", typeof(string)).Build(),
            new PropertyBuilder("Name3", typeof(string)).Build(),
            new PropertyBuilder("Name4", typeof(string)).Build(),
        };

        var blClass =
            new ClassBuilder("BlType")
            .SetNamespace(BlNamespace)
            .AddMembers(blMembers)
            .Build();

        var baseDtoClass1 =
            new ClassBuilder("BaseEmptyDto")
            .SetNamespace(baseDtoNamespace)
            .Build();

        var baseDtoClass2 =
            new ClassBuilder("BaseDtoWithConflictMethodName")
            .SetNamespace(baseDtoNamespace)
            .AddMember(new MethodBuilder("Name1", typeof(bool)).SetBody("return false;").Build())
            .Build();

        var dtoClass =
            new DtoClassBuilder("DtoName", DtoClassBuilder.DtoAttributeType.DtoFrom, blClass)
            .SetNamespace(DtoNamespace)
            .AddBase(baseDtoClass2)
            .Build();

        var inputClasses = new[] { blClass, baseDtoClass1, baseDtoClass2, dtoClass };

        RunWithAssert(inputClasses, DoAssert);

        void DoAssert(Compilation compilation, ImmutableArray<Diagnostic> msgs)
        {
            Assert.Single(msgs);
            var msg = msgs[0];

            var expMsg = new MemberConflictError("Name1", dtoClass.Name);

            Assert.Equal(DiagnosticSeverity.Error, msg.Severity);
            Assert.Equal(expMsg.Id, msg.Id);

            Assert.Equal(inputClasses.Length, compilation.SyntaxTrees.Count());
        }
    }

    [Fact]
    public void DtoHasParent_BlHasPropAsBaseDtoName_Test()
    {
        var baseDtoNamespace = DtoNamespace + ".Base";

        var baseDtoClass1 =
            new ClassBuilder("BaseEmptyDto")
            .SetNamespace(baseDtoNamespace)
            .Build();

        var blMembers = new List<Member>
        {
            CommonProperties.Id_Int,
            new PropertyBuilder(baseDtoClass1.Name, typeof(string)).Build(),
        };

        var blClass =
            new ClassBuilder("BlTypeWithPropNameAsBaseDtoCtor")
            .SetNamespace(BlNamespace)
            .AddMembers(blMembers)
            .Build();

        var baseDtoClass2 =
            new ClassBuilder("BaseDtoWithCtorName")
            .SetNamespace(baseDtoNamespace)
            .AddBase(baseDtoClass1)
            .Build();

        var dtoClass =
            new DtoClassBuilder("DtoName", DtoClassBuilder.DtoAttributeType.DtoFrom, blClass)
            .SetNamespace(DtoNamespace)
            .AddBase(baseDtoClass2)
            .Build();

        RunWithAssert(new[] { blClass, baseDtoClass1, baseDtoClass2, dtoClass }, DoAssert);

        void DoAssert(Compilation compilation, ImmutableArray<Diagnostic> msgs)
        {
            Assert.Empty(msgs);

            var generatedClass = SyntaxChecker.FindAllClassDeclarationsByName(compilation, dtoClass.Name)
                    .Skip(1) //skip declaration to get only generated
                    .Single();

            SyntaxChecker.TestOneClassDeclaration(generatedClass, Member2PropDescriptor(blMembers));
        }
    }

    [Fact]
    public void DtoHasParentWithMembers_WithConflictBlNames_Error_Test()
    {
        var baseDtoNamespace = DtoNamespace + ".Base";
        var blMembers = new List<Member>
        {
            CommonProperties.Id_Int,
            new PropertyBuilder("Name1", typeof(string)).Build(),
            new PropertyBuilder("Name2", typeof(string)).Build(),
            new PropertyBuilder("Name3", typeof(string)).Build(),
            new PropertyBuilder("Name4", typeof(string)).Build(),
        };

        var blClass =
            new ClassBuilder("BlType")
            .SetNamespace(BlNamespace)
            .AddMembers(blMembers)
            .Build();

        var baseDtoClass1 =
            new ClassBuilder("BaseEmptyDto")
            .SetNamespace(baseDtoNamespace)
            .Build();

        var baseDtoClass2 =
            new ClassBuilder("BaseDtoWithConflictedMembers")
            .SetNamespace(baseDtoNamespace)
            .AddMember(new FieldBuilder("Name1", typeof(bool)).Build())
            .AddMember(new ConstantBuilder("Name2", typeof(bool), "false").Build())
            .Build();

        var dtoClass =
            new DtoClassBuilder("DtoName", DtoClassBuilder.DtoAttributeType.DtoFrom, blClass)
            .SetNamespace(DtoNamespace)
            .AddBase(baseDtoClass2)
            .Build();

        var inputClasses = new[] { blClass, baseDtoClass1, baseDtoClass2, dtoClass };

        RunWithAssert(inputClasses, DoAssert);

        void DoAssert(Compilation compilation, ImmutableArray<Diagnostic> msgs)
        {
            Assert.Equal(2, msgs.Length);

            foreach (var msg in msgs)
            {
                var expMsg = new MemberConflictError("", dtoClass.Name);

                Assert.Equal(DiagnosticSeverity.Error, msg.Severity);
                Assert.Equal(expMsg.Id, msg.Id);
            }

            Assert.Equal(inputClasses.Length, compilation.SyntaxTrees.Count());
        }
    }

    [Fact]
    public void DtoHasParentWithMembers_WithConflictBlNames_Warning_Test()
    {
        var stringType = typeof(string);

        var baseDtoNamespace = DtoNamespace + ".Base";
        var conflictedPropName = "Name4";
        var blMembers = new List<Member>
        {
            CommonProperties.Id_Int,
            new PropertyBuilder("Name1", stringType).Build(),
            new PropertyBuilder("Name2", stringType).Build(),
            new PropertyBuilder("Name3", stringType).Build(),
            new PropertyBuilder(conflictedPropName, stringType).Build(),
            new PropertyBuilder("Name5", stringType).Build(),
            new PropertyBuilder("Name6", stringType).Build(),
            new PropertyBuilder("Name7", stringType).Build(),
            new PropertyBuilder("Name8", stringType).Build(),
        };

        var privateDtoMembers = new List<Member>
        {
            new FieldBuilder("Name1", stringType).SetAccessor(Visibility.Private).Build(),
            new PropertyBuilder("Name3", stringType).SetAccessor(Visibility.Private).Build(),
            new MethodBuilder("Name5", stringType).SetBody("return null;").SetAccessor(Visibility.Private).Build(),
            new ConstantBuilder("Name7", stringType, "\"SomeValue\"").SetAccessor(Visibility.Private).Build(),
        };

        var protectedDtoMembers = new List<Member>
        {
            new FieldBuilder("Name2", stringType).SetAccessor(Visibility.Protected).Build(),
            new PropertyBuilder(conflictedPropName, stringType).SetAccessor(Visibility.Protected).Build(),
            new MethodBuilder("Name6", stringType).SetBody("return null;").SetAccessor(Visibility.Protected).Build(),
            new ConstantBuilder("Name8", stringType, "\"SomeValue\"").SetAccessor(Visibility.Protected).Build(),
        };

        var blClass =
            new ClassBuilder("BlType")
            .SetNamespace(BlNamespace)
            .AddMembers(blMembers)
            .Build();

        var baseDtoClass1 =
            new ClassBuilder("BaseEmptyDto")
            .SetNamespace(baseDtoNamespace)
            .Build();

        var baseDtoClass2 =
            new ClassBuilder("BaseDtoWithConflictedMembers")
            .SetNamespace(baseDtoNamespace)
            .AddBase(baseDtoClass1)
            .AddMembers(privateDtoMembers)
            .AddMembers(protectedDtoMembers)
            .Build();

        var dtoClass =
            new DtoClassBuilder("DtoName", DtoClassBuilder.DtoAttributeType.DtoFrom, blClass)
            .SetNamespace(DtoNamespace)
            .AddBase(baseDtoClass2)
            .Build();

        RunWithAssert(new[] { blClass, baseDtoClass1, baseDtoClass2, dtoClass }, DoAssert);

        void DoAssert(Compilation compilation, ImmutableArray<Diagnostic> msgs)
        {
            Assert.Equal(protectedDtoMembers.Count, msgs.Length);

            Assert.Equal(protectedDtoMembers.Count, msgs.Where(x => x.Severity == DiagnosticSeverity.Warning).Count());

            var wrnIdMember = new MemberConflictWarning("", dtoClass.Name).Id;
            var wrnIdProp = new PropertyConflictWarning("", dtoClass.Name).Id;

            Assert.Single(msgs.Where(x => x.Id == wrnIdProp).ToList());
            Assert.Equal(3, msgs.Where(x => x.Id == wrnIdMember).Count());

            var generatedClass = SyntaxChecker.FindAllClassDeclarationsByName(compilation, dtoClass.Name)
                    .Skip(1) //skip declaration to get only generated
                    .Single();

            var expectedPropsInDto = Member2PropDescriptor(blMembers.Where(x => x.Name != conflictedPropName));
            SyntaxChecker.TestOneClassDeclaration(generatedClass, expectedPropsInDto);
        }
    }

    [Fact]
    public void DtoHasParentWithStatic()
    {
        var baseDtoNamespace = DtoNamespace + ".Base";
        var blMembers = new List<Member>
        {
            CommonProperties.Id_Int,
            new PropertyBuilder("Name1", typeof(string)).Build(),
            new PropertyBuilder("Name2", typeof(string)).Build(),
            new PropertyBuilder("Name3", typeof(string)).Build(),
            new PropertyBuilder("Name4", typeof(string)).Build(),
        };

        var visibleDtoMembers = new List<Member>
        {
            new PropertyBuilder("Name1", typeof(string)).SetStatic(true).SetAccessor(Visibility.Protected).Build(),
            new PropertyBuilder("Name2", typeof(string)).SetStatic(true).SetAccessor(Visibility.Public).Build(),
        };

        var blClass =
            new ClassBuilder("BlType")
            .SetNamespace(BlNamespace)
            .AddMembers(blMembers)
            .Build();

        var baseDtoClass1 =
            new ClassBuilder("BaseEmptyDto")
            .SetNamespace(baseDtoNamespace)
            .Build();

        var baseDtoClass2 =
            new ClassBuilder("BaseDtoWithStatic")
            .SetNamespace(baseDtoNamespace)
            .AddMembers(visibleDtoMembers)
            .AddMember(new PropertyBuilder("Name3", typeof(string)).SetStatic(true).SetAccessor(Visibility.Private).Build())
            .Build();

        var dtoClass =
            new DtoClassBuilder("DtoName", DtoClassBuilder.DtoAttributeType.DtoFrom, blClass)
            .SetNamespace(DtoNamespace)
            .AddBase(baseDtoClass2)
            .Build();

        var inputClasses = new[] { blClass, baseDtoClass1, baseDtoClass2, dtoClass };

        RunWithAssert(inputClasses, DoAssert);

        void DoAssert(Compilation compilation, ImmutableArray<Diagnostic> msgs)
        {
            Assert.Equal(visibleDtoMembers.Count, msgs.Length);
            var expId = new MemberConflictError("", dtoClass.Name).Id;

            Assert.Equal(DiagnosticSeverity.Error, msgs[0].Severity);
            Assert.Equal(DiagnosticSeverity.Error, msgs[1].Severity);

            Assert.Equal(expId, msgs[0].Id);
            Assert.Equal(expId, msgs[1].Id);

            Assert.Equal(inputClasses.Length, compilation.SyntaxTrees.Count());
        }
    }

    [Fact]
    public void DtoHasGenericParent()
    {
        var baseDtoNamespace = DtoNamespace + ".Base";
        var conflictedPropName = "Prop1";
        var blMembers = new List<Member>
        {
            CommonProperties.Id_Int,
            new PropertyBuilder("Name1", typeof(string)).Build(),
            new PropertyBuilder("Name2", typeof(string)).Build(),
            new PropertyBuilder("Name3", typeof(string)).Build(),
            new PropertyBuilder("Name4", typeof(string)).Build(),
        };

        var blClass =
            new ClassBuilder("BlType")
            .SetNamespace(BlNamespace)
            .AddMembers(blMembers)
            .AddMember(new PropertyBuilder(conflictedPropName, typeof(string)).Build())
            .Build();

        var baseDtoClass1 =
            new ClassBuilder("BaseEmptyDto")
            .SetNamespace(baseDtoNamespace)
            .Build();

        var baseDtoClass2 =
            new ClassBuilder("BaseDtoGeneric<T>")
            .SetNamespace(baseDtoNamespace)
            .AddBase(baseDtoClass1)
            .AddMember(new PropertyBuilder(conflictedPropName, "T").Build())
            .Build();

        var dtoClass =
            new DtoClassBuilder("DtoName", DtoClassBuilder.DtoAttributeType.DtoFrom, blClass)
            .SetNamespace(DtoNamespace)
            .AddBase("BaseDtoGeneric<object>", baseDtoNamespace)
            .Build();

        RunWithAssert(new[] { blClass, baseDtoClass1, baseDtoClass2, dtoClass }, DoAssert);

        void DoAssert(Compilation compilation, ImmutableArray<Diagnostic> msgs)
        {
            Assert.Single(msgs);
            var expId = new PropertyConflictWarning("", dtoClass.Name).Id;

            Assert.Equal(DiagnosticSeverity.Warning, msgs[0].Severity);

            Assert.Equal(expId, msgs[0].Id);

            var generatedClass = SyntaxChecker.FindAllClassDeclarationsByName(compilation, dtoClass.Name)
                    .Skip(1) //skip declaration to get only generated
                    .Single();

            SyntaxChecker.TestOneClassDeclaration(generatedClass, Member2PropDescriptor(blMembers));
        }
    }
}

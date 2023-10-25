using AutoDto.SourceGen.DiagnosticMessages.Errors;
using AutoDto.SourceGen.DiagnosticMessages.Warnings;
using AutoDto.Tests.TestHelpers.CodeBuilder.Builders;
using AutoDto.Tests.TestHelpers.CodeBuilder.Elements;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using static AutoDto.Tests.TestHelpers.SyntaxChecker;

namespace AutoDto.Tests.SourceGeneration;

public class DtoHasOwnMembersTests : BaseUnitTest
{
    [Fact]
    public void HasMembersWithoutConflictNames()
    {
        var blMembers = new Member[]
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

        var dtoClass =
            new DtoClassBuilder("MyDto", DtoClassBuilder.DtoAttributeType.DtoFrom, blClass)
            .SetNamespace(DtoNamespace)
            .AddMember(new PropertyBuilder("Name0", typeof(string)).Build())
            .Build();

        RunWithAssert(new[] { blClass, dtoClass }, DoAssert);

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
    public void DtoHasInternallyReservedName()
    {
        var blMembers = new Member[]
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

        var dtoClass =
            new DtoClassBuilder("MyDto", DtoClassBuilder.DtoAttributeType.DtoFrom, blClass)
            .SetNamespace(DtoNamespace)
            .AddMember(new FieldBuilder("get_Id", typeof(int)).Build())
            .Build()
            ;

        RunWithAssert(new[] { blClass, dtoClass }, DoAssert);

        void DoAssert(Compilation compilation, ImmutableArray<Diagnostic> msgs)
        {
            Assert.Single(msgs);

            var msg = msgs[0];
            var expId = new ReservedMemberConflicError("", dtoClass.Name).Id;
            Assert.Equal(DiagnosticSeverity.Error, msg.Severity);
            Assert.Equal(expId, msg.Id);
        }
    }

    [Theory]
    [InlineData(Visibility.Public, "SomeName1")]
    [InlineData(Visibility.Protected, "SomeName1")]
    [InlineData(Visibility.Private, "SomeName1")]

    [InlineData(Visibility.Public, "SomeName1", "SomeName2")]
    [InlineData(Visibility.Protected, "SomeName1", "SomeName2")]
    [InlineData(Visibility.Private, "SomeName1", "SomeName2")]
    public void HasPropsAsInBlTest(Visibility visibility, params string[] commonPropNames)
    {
        var blMembers = new List<Member>
        {
            CommonProperties.Id_Int,
            new PropertyBuilder("Name1", typeof(string)).Build(),
            new PropertyBuilder("Name2", typeof(string)).Build(),
            new PropertyBuilder("Name3", typeof(string)).Build(),
            new PropertyBuilder("Name4", typeof(string)).Build(),
        };

        var dtoMembers = new List<Member>();

        foreach (var commonName in commonPropNames)
        {
            blMembers.Add(new PropertyBuilder(commonName, typeof(string)).SetAccessor(Visibility.Public).Build());
            dtoMembers.Add(new PropertyBuilder(commonName, typeof(string)).SetAccessor(visibility).Build());
        }

        var blClass =
            new ClassBuilder("BlType")
            .SetNamespace(BlNamespace)
            .AddMembers(blMembers)
            .Build();

        var dtoClass =
            new DtoClassBuilder("MyDto", DtoClassBuilder.DtoAttributeType.DtoFrom, blClass)
            .SetNamespace(DtoNamespace)
            .AddMembers(dtoMembers)
            .Build();

        RunWithAssert(new[] { blClass, dtoClass }, DoAssert);

        void DoAssert(Compilation compilation, ImmutableArray<Diagnostic> msgs)
        {
            Assert.Equal(commonPropNames.Length, msgs.Length);

            var expWrn = new PropertyConflictWarning("", dtoClass.Name);

            foreach (var msg in msgs)
            {
                Assert.Equal(DiagnosticSeverity.Warning, msg.Severity);
                Assert.Equal(expWrn.Id, msg.Id);
            }

            var generatedClass = SyntaxChecker.FindAllClassDeclarationsByName(compilation, dtoClass.Name)
                    .Skip(1) //skip declaration to get only generated
                    .Single();

            var expectedGeneratedDtoProps = Member2PropDescriptor(blMembers.Where(x => !commonPropNames.Contains(x.Name))).ToList();

            SyntaxChecker.TestOneClassDeclaration(generatedClass, expectedGeneratedDtoProps);
        }
    }

    [Fact]
    public void BlHasPropAsDtoNameTest()
    {
        var dtoName = "MyDto";

        var blMembers = new List<Member>
        {
            CommonProperties.Id_Int,
            new PropertyBuilder(dtoName, typeof(string)).Build(),
        };

        var blClass =
            new ClassBuilder("BlType")
            .SetNamespace(BlNamespace)
            .AddMembers(blMembers)
            .Build();

        var dtoClass =
            new DtoClassBuilder(dtoName, DtoClassBuilder.DtoAttributeType.DtoFrom, blClass)
            .SetNamespace(DtoNamespace)
            .Build();

        RunWithAssert(new[] { blClass, dtoClass }, DoAssert);

        void DoAssert(Compilation compilation, ImmutableArray<Diagnostic> msgs)
        {
            Assert.Single(msgs);
            var msg = msgs[0];

            var exp = new MemberConflictError("", dtoClass.Name);
            Assert.Equal(DiagnosticSeverity.Error, msg.Severity);
            Assert.Equal(exp.Id, msg.Id);
        }
    }

    private void DtoHasMemberAsBlPropNameTest<TMember>(BaseMemberBuilder<TMember> dtoMemberBuilder, Visibility visibility, bool isStatic)
        where TMember : Member
    {
        var dtoMember = dtoMemberBuilder
            .SetAccessor(visibility)
            .SetStatic(isStatic)
            .Build();

        var blMembers = new List<Member>
        {
            CommonProperties.Id_Int,
            new PropertyBuilder("Name1", typeof(string)).Build(),
            new PropertyBuilder("Name2", typeof(string)).Build(),
            new PropertyBuilder("Name3", typeof(string)).Build(),
            new PropertyBuilder("Name4", typeof(string)).Build(),

            new PropertyBuilder(dtoMember.Name, dtoMember.ReturnType).Build(),
        };

        var blClass =
            new ClassBuilder("BlType")
            .SetNamespace(BlNamespace)
            .AddMembers(blMembers)
            .Build();

        var dtoClass =
            new DtoClassBuilder("MyDto", DtoClassBuilder.DtoAttributeType.DtoFrom, blClass)
            .SetNamespace(DtoNamespace)
            .AddMember(dtoMember)
            .Build();

        RunWithAssert(new[] { blClass, dtoClass }, DoAssert);

        void DoAssert(Compilation compilation, ImmutableArray<Diagnostic> msgs)
        {
            Assert.Single(msgs);

            var msg = msgs[0];
            var expId = new MemberConflictError("", dtoClass.Name).Id;
            Assert.Equal(DiagnosticSeverity.Error, msg.Severity);
            Assert.Equal(expId, msg.Id);
        }
    }

    [Theory]
    [InlineData(Visibility.Public, false)]
    [InlineData(Visibility.Protected, false)]
    [InlineData(Visibility.Private, false)]

    [InlineData(Visibility.Public, true)]
    [InlineData(Visibility.Protected, true)]
    [InlineData(Visibility.Private, true)]
    public void DtoHasMethodAsBlPropNameTest(Visibility visibility, bool isStatic)
    {
        var mb = new MethodBuilder("SomeName1", typeof(string)).SetBody("return null;");
        DtoHasMemberAsBlPropNameTest(mb, visibility, isStatic);
    }

    [Theory]
    [InlineData(Visibility.Public, false)]
    [InlineData(Visibility.Protected, false)]
    [InlineData(Visibility.Private, false)]

    [InlineData(Visibility.Public, true)]
    [InlineData(Visibility.Protected, true)]
    [InlineData(Visibility.Private, true)]
    public void DtoHasFieldAsBlPropNameTest(Visibility visibility, bool isStatic)
    {
        var fb = new FieldBuilder("SomeName1", typeof(string));
        DtoHasMemberAsBlPropNameTest(fb, visibility, isStatic);
    }

    [Theory]
    [InlineData(Visibility.Public)]
    [InlineData(Visibility.Protected)]
    [InlineData(Visibility.Private)]
    public void DtoHasStaticPropAsBlPropNameTest(Visibility visibility)
    {
        var pb = new PropertyBuilder("SomeName1", typeof(string));
        DtoHasMemberAsBlPropNameTest(pb, visibility, true);
    }
}

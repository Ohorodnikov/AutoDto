using AutoDto.SourceGen.DiagnosticMessages.Errors;
using AutoDto.SourceGen.DiagnosticMessages.Warnings;
using AutoDto.Tests.SourceGeneration.Models.HierarchyTestModels;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using static AutoDto.Tests.TestHelpers.SyntaxChecker;

namespace AutoDto.Tests.SourceGeneration;

public class DtoHasOwnMembersTests : BaseUnitTest
{
    private static string _dtoName = "MyDto";
    private static Type _blType = typeof(BlType);
    private record MemberDescriptor(Accessibility Accessibility, Type Type, string Name, SymbolKind Kind, bool IsStatic = false);

    private string GetKindDefinition(SymbolKind kind)
    {
        return kind switch
        {
            SymbolKind.Property => "{ get; set; }",
            SymbolKind.Field => ";",
            SymbolKind.Method => "(){}",
            _ => throw new NotImplementedException()
        };
    }

    private string GetDtoCode(Type blType, IEnumerable<MemberDescriptor> members)
    {
        var attr = DtoCreator.GetDtoFromAttr(blType);
        var dtoDef = DtoCreator.GetPublicDtoDef(_dtoName);

        var code = $@"
using {attr.nameSpace};
using {blType.Namespace};

namespace AutoDto.Tests.SourceGeneration.Dto;

{attr.definition}
{dtoDef}
{{
";
        foreach (var member in members)
        {
            var access = SyntaxFacts.GetText(member.Accessibility);
            var isStatic = member.IsStatic ? "static" : "";
            var type = member.Type == typeof(void) ? "void" :  member.Type.FullName;
            var kindDef = GetKindDefinition(member.Kind);

            code += Environment.NewLine + $"{access} {isStatic} {type} {member.Name} {kindDef}";
        }

        code += Environment.NewLine +  "}";

        return code;
    }

    private Compilation RunTest(IEnumerable<MemberDescriptor> members, Action<ImmutableArray<Diagnostic>> testDiagnosticMsgsAction)
    {
        var code = GetDtoCode(_blType, members);
        var (compilation, msgs) = Generator.RunWithMsgs(code);

        testDiagnosticMsgsAction(msgs);

        return compilation;
    }

    [Fact]
    public void HasMembersWithoutConflictNames()
    {
        var compilation = RunTest(new MemberDescriptor[0], (msgs) => Assert.Empty(msgs));

        var generated = SyntaxChecker.FindClassByName(compilation, _dtoName);

        Assert.NotNull(generated);

        var expected = _blType.GetProperties()
            .Select(x => new PropertyDescriptor(x))
            .ToList();

        SyntaxChecker.TestOneClassDeclaration(generated, expected);
    }

    [Fact]
    public void DtoHasInternalyReservedName()
    {
        var specialNameMember = _blType.GetMembers()
            .OfType<MethodInfo>()
            .Where(x => x.Attributes.HasFlag(MethodAttributes.SpecialName))
            .First();

        var members = new[]
        {
            new MemberDescriptor(Accessibility.Public, specialNameMember.ReturnType, specialNameMember.Name, SymbolKind.Field),
        };

        void TestDiagnostic(ImmutableArray<Diagnostic> msgs)
        {
            Assert.Single(msgs);

            var msg = msgs[0];
            var expId = new ReservedMemberConflicError("", _dtoName).Id;
            Assert.Equal(DiagnosticSeverity.Error, msg.Severity);
            Assert.Equal(expId, msg.Id);
        }

        RunTest(members, TestDiagnostic);
    }

    private void TestHasProperties(Accessibility accessibility, params string[] propNames)
    {
        var members = propNames.Select(x => new MemberDescriptor(accessibility, typeof(string), x, SymbolKind.Property)).ToList();

        void TestDiagnostic(ImmutableArray<Diagnostic> msgs)
        {
            Assert.Equal(members.Count, msgs.Length);

            var expWrn = new PropertyConflictWarning("", _dtoName);

            foreach (var msg in msgs)
            {
                Assert.Equal(DiagnosticSeverity.Warning, msg.Severity);
                Assert.Equal(expWrn.Id, msg.Id);
            }
        }

        var compilation = RunTest(members, TestDiagnostic);

        var generated = SyntaxChecker.FindClassByName(compilation, _dtoName);

        Assert.NotNull(generated);

        var expected = _blType.GetProperties()
            .Where(x => !propNames.Contains(x.Name))
            .Select(x => new PropertyDescriptor(x))
            .ToList();

        SyntaxChecker.TestOneClassDeclaration(generated, expected);
    }

    [Theory]
    [InlineData(Accessibility.Public)]
    [InlineData(Accessibility.Protected)]
    [InlineData(Accessibility.Private)]
    public void HasOnePropertyTest(Accessibility accessibility) 
        => TestHasProperties(accessibility, nameof(BlType.Name1));

    [Theory]
    [InlineData(Accessibility.Public)]
    [InlineData(Accessibility.Protected)]
    [InlineData(Accessibility.Private)]
    public void HasOneManyPropertiseTest(Accessibility accessibility) 
        => TestHasProperties(accessibility, nameof(BlType.Name1), nameof(BlType.Name2));

    [Fact]
    public void BlHasPropAsDtoNameTest()
    {
        var blType = typeof(BlTypeWithPropAsDtoCtor);

        var code = GetDtoCode(blType, new MemberDescriptor[0]);
        var (compilation, msgs) = Generator.RunWithMsgs(code);

        Assert.Single(msgs);
        var msg = msgs[0];

        Assert.Equal(DiagnosticSeverity.Error, msg.Severity);

        var exp = new MemberConflictError("", _dtoName);

        Assert.Equal(exp.Id, msg.Id);
    }

    private void TestWithMembers(Accessibility accessibility, SymbolKind kind, bool isStatic, params string[] memberNames)
    {
        var retType = kind == SymbolKind.Method ? typeof(void) : typeof(string);

        var members = memberNames.Select(x => new MemberDescriptor(accessibility, retType, x, kind, isStatic)).ToList();

        void TestDiagnostic(ImmutableArray<Diagnostic> msgs)
        {
            Assert.Equal(members.Count, msgs.Length);

            var expWrn = new MemberConflictError("", _dtoName);

            foreach (var msg in msgs)
            {
                Assert.Equal(DiagnosticSeverity.Error, msg.Severity);
                Assert.Equal(expWrn.Id, msg.Id);
            }
        }

        var compilation = RunTest(members, TestDiagnostic);

        Assert.Single(compilation.SyntaxTrees);
    }

    [Theory]

    [InlineData(Accessibility.Public, SymbolKind.Method)]
    [InlineData(Accessibility.Protected, SymbolKind.Method)]
    [InlineData(Accessibility.Private, SymbolKind.Method)]

    [InlineData(Accessibility.Public, SymbolKind.Field)]
    [InlineData(Accessibility.Protected, SymbolKind.Field)]
    [InlineData(Accessibility.Private, SymbolKind.Field)]
    public void HasMemberTest(Accessibility accessibility, SymbolKind kind)
    {
        TestWithMembers(accessibility, kind, false, nameof(BlType.Name1));
    }  

    [Theory]

    [InlineData(Accessibility.Public, SymbolKind.Property)]
    [InlineData(Accessibility.Protected, SymbolKind.Property)]
    [InlineData(Accessibility.Private, SymbolKind.Property)]

    [InlineData(Accessibility.Public, SymbolKind.Method)]
    [InlineData(Accessibility.Protected, SymbolKind.Method)]
    [InlineData(Accessibility.Private, SymbolKind.Method)]

    [InlineData(Accessibility.Public, SymbolKind.Field)]
    [InlineData(Accessibility.Protected, SymbolKind.Field)]
    [InlineData(Accessibility.Private, SymbolKind.Field)]
    public void HasStaticMembersTest(Accessibility accessibility, SymbolKind kind)
    {
        TestWithMembers(accessibility, kind, true, nameof(BlType.Name1));
    }
} 

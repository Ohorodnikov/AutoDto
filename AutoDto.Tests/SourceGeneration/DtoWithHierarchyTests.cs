using AutoDto.SourceGen.DiagnosticMessages.Errors;
using AutoDto.SourceGen.DiagnosticMessages.Warnings;
using AutoDto.Tests.SourceGeneration.Models.HierarchyTestModels;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Reflection;
using static AutoDto.Tests.TestHelpers.SyntaxChecker;

namespace AutoDto.Tests.SourceGeneration;

public class DtoWithHierarchyTests : BaseUnitTest
{
    Type _blType = typeof(BlType);
    string _dtoName = "MyDto";
    Action<ImmutableArray<Diagnostic>> _noDiagnosticMsgs = (msgs) => Assert.Empty(msgs);

    

    private string GetDtoCode(Type blType, Type baseDtoType)
    {
        var attr = DtoCreator.GetDtoFromAttr(blType);
        var dtoDef = DtoCreator.GetPublicDtoDef(_dtoName);

        var code = $@"

using {attr.nameSpace};
using {_blType.Namespace};
using {baseDtoType.Namespace};

namespace AutoDto.Tests.SourceGeneration.Dto;

{attr.definition}
{dtoDef} : {DtoCreator.GetTypeName(baseDtoType)}
{{

}}
";

        return code;
    }

    private Compilation RunAndCheckDiagnostic(Type blType, Type baseDtoType, Action<ImmutableArray<Diagnostic>> testDiagnosticMsgsAction)
    {
        var code = GetDtoCode(blType, baseDtoType);

        var (compilation, msgs) = Generator.RunWithMsgs(code);

        testDiagnosticMsgsAction(msgs);

        return compilation;
    }

    private void DoTest_Success(
        Type baseDtoType,
        IEnumerable<PropertyInfo> expectedProps,
        Action<ImmutableArray<Diagnostic>> testDiagnosticMsgsAction = null,
        Type blType = null)
    {
        var compilation = RunAndCheckDiagnostic(
            blType ?? _blType,
            baseDtoType,
            testDiagnosticMsgsAction ?? _noDiagnosticMsgs
            );

        Assert.Equal(2, compilation.SyntaxTrees.Count());

        var generatedClass = SyntaxChecker.FindClassByName(compilation, _dtoName);

        var expected = expectedProps.Select(x => new PropertyDescriptor(x));

        SyntaxChecker.TestOneClassDeclaration(generatedClass, expected);
    }

    private void DoTest_Error(
        Type baseDtoType,
        Action<ImmutableArray<Diagnostic>> testDiagnosticMsgsAction,
        Type blType = null)
    {
        var compilation = RunAndCheckDiagnostic(
            blType ?? _blType,
            baseDtoType,
            testDiagnosticMsgsAction
            );

        Assert.Equal(1, compilation.SyntaxTrees.Count());
    }

    [Fact]
    public void DtoHasEmptyParent()
    {
        DoTest_Success(typeof(BaseEmptyDto), _blType.GetProperties());
    }

    [Fact]
    public void DtoHasParentWithProps_WithoutConflictBlNamsesTest()
    {
        DoTest_Success(typeof(BaseDtoWithoutConflictProps), _blType.GetProperties());
    }

    [Fact]
    public void DtoHasParentWithProps_WithConflictBlNamses_Warning_Test()
    {
        var expectedProps = _blType.GetProperties()
                                   .Where(x => x.Name != nameof(BaseDtoWithConflictPropName.Name1));

        void TestDiagnostic(ImmutableArray<Diagnostic> msgs)
        {
            Assert.Single(msgs);
            var msg = msgs[0];

            var expMsg = new PropertyConflictWarning(nameof(BaseDtoWithConflictPropName.Name1), _dtoName);

            Assert.Equal(DiagnosticSeverity.Warning, msg.Severity);
            Assert.Equal(expMsg.Id, msg.Id);
        }

        DoTest_Success(typeof(BaseDtoWithConflictPropName), expectedProps, TestDiagnostic);
    }

    [Fact]
    public void DtoHasParentWithMethods_WithoutConflictBlNamesTest()
    {
        DoTest_Success(typeof(BaseDtoWithMethod), _blType.GetProperties());
    }

    [Fact]
    public void DtoHasParentWithInternalyReservedName()
    {
        void TestDiagnostic(ImmutableArray<Diagnostic> msgs)
        {
            Assert.Single(msgs);

            var msg = msgs[0];
            var expId = new ReservedMemberConflicError("", _dtoName).Id;
            Assert.Equal(DiagnosticSeverity.Error, msg.Severity);
            Assert.Equal(expId, msg.Id);
        }

        DoTest_Error(typeof(BaseDtoWithSpecialName), TestDiagnostic);
    }

    [Fact]
    public void DtoHasParentWithMethods_WithConflictBlNames_Error_Test()
    {        
        void TestDiagnostic(ImmutableArray<Diagnostic> msgs)
        {
            Assert.Single(msgs);
            var msg = msgs[0];

            var expMsg = new MemberConflictError(nameof(BaseDtoWithConflictMethodName.Name1), _dtoName);

            Assert.Equal(DiagnosticSeverity.Error, msg.Severity);
            Assert.Equal(expMsg.Id, msg.Id);
        }

        DoTest_Error(typeof(BaseDtoWithConflictMethodName), TestDiagnostic);

    }

    [Fact]
    public void DtoHasParent_BlHasPropAsBaseDtoName_Test()
    {
        var blType = typeof(BlTypeWithPropNameAsBaseDtoCtor);
        var expectedProps = blType.GetProperties();

        DoTest_Success(typeof(BaseDtoWithCtorName), expectedProps, _noDiagnosticMsgs, blType);
    }

    [Fact]
    public void DtoHasParentWithMembers_WithConflictBlNames_Error_Test()
    {
        var notExpectedNames = new[]
        {
            nameof(BaseDtoWithConflictedMembers.Name1),
            nameof(BaseDtoWithConflictedMembers.Name2),
        };

        void TestDiagnostic(ImmutableArray<Diagnostic> msgs)
        {
            Assert.Equal(notExpectedNames.Length, msgs.Length);

            foreach (var msg in msgs)
            {
                var expMsg = new MemberConflictError("", _dtoName);

                Assert.Equal(DiagnosticSeverity.Error, msg.Severity);
                Assert.Equal(expMsg.Id, msg.Id);
            }
        }

        DoTest_Error(typeof(BaseDtoWithConflictedMembers), TestDiagnostic);
    }

    [Fact]
    public void DtoHasParentWithMembers_WithConflictBlNames_Warning_Test()
    {
        var notExpectedNames = new[]
        {
            "Name2",
            "Name4", //prop
            "Name6",
            "Name8",
            //nameof(BaseDtoWithConflictedMembers_NotError.Name2),
            //nameof(BaseDtoWithConflictedMembers_NotError.Name4),
            //nameof(BaseDtoWithConflictedMembers_NotError.Name6),
            //nameof(BaseDtoWithConflictedMembers_NotError.Name8),
        };
        var expected = _blType.GetProperties().Where(x => x.Name != "Name4");
        
        void TestDiagnostic(ImmutableArray<Diagnostic> msgs)
        {
            Assert.Equal(notExpectedNames.Length, msgs.Length);

            Assert.Equal(notExpectedNames.Length, msgs.Where(x => x.Severity == DiagnosticSeverity.Warning).Count());

            var wrnIdMember = new MemberConflictWarning("", _dtoName).Id;
            var wrnIdProp = new PropertyConflictWarning("", _dtoName).Id;

            Assert.Equal(1, msgs.Where(x => x.Id == wrnIdProp).Count());
            Assert.Equal(3, msgs.Where(x => x.Id == wrnIdMember).Count());
        }

        DoTest_Success(typeof(BaseDtoWithConflictedMembers_NotError), expected, TestDiagnostic);
    }

    [Fact]
    public void DtoHasParentWithStatic()
    {
        void TestDiagnostic(ImmutableArray<Diagnostic> msgs)
        {
            Assert.Equal(2, msgs.Length);
            var expId = new MemberConflictError("", _dtoName).Id;

            Assert.Equal(DiagnosticSeverity.Error, msgs[0].Severity);
            Assert.Equal(DiagnosticSeverity.Error, msgs[1].Severity);

            Assert.Equal(expId, msgs[0].Id);
            Assert.Equal(expId, msgs[1].Id);
        }

        DoTest_Error(typeof(BaseDtoWithStatic), TestDiagnostic);
    }

    [Fact]
    public void DtoHasGenericParent()
    {
        var expected = _blType.GetProperties().Where(x => x.Name != nameof(BaseDtoGeneric<object>.Name1));

        void TestDiagnostic(ImmutableArray<Diagnostic> msgs)
        {
            Assert.Equal(1, msgs.Length);
            var expId = new PropertyConflictWarning("", _dtoName).Id;

            Assert.Equal(DiagnosticSeverity.Warning, msgs[0].Severity);

            Assert.Equal(expId, msgs[0].Id);
        }

        DoTest_Success(typeof(BaseDtoGeneric<DateTime>), expected, TestDiagnostic);

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoDto.Tests.SourceGeneration.Models.HierarchyTestModels;

public class BlType
{
    public int Id { get; set; }
    public string Name1 { get; set; }
    public string Name2 { get; set; }
    public string Name3 { get; set; }
    public string Name4 { get; set; }
    public string Name5 { get; set; }
    public string Name6 { get; set; }
    public string Name7 { get; set; }
    public string Name8 { get; set; }
}

public class BlTypeWithPropNameAsBaseDtoCtor
{
    public int Id { get; set; }
    public string BaseEmptyDto { get; set; }
}

public class BlTypeWithPropAsDtoCtor
{
    public int Id { get; set; }
    public string MyDto { get; set; }
}

public class BaseEmptyDto {}

public class BaseDtoWithoutConflictProps : BaseEmptyDto
{
    public string SomeNotConflictProp { get; set; }
}

public class BaseDtoWithConflictPropName : BaseEmptyDto
{
    public string Name1 { get; set; }
}

public class BaseDtoWithSpecialName
{
    public string get_Name1;
}

public class BaseDtoWithConflictMethodName : BaseEmptyDto
{
    public bool Name1() => false;
}

public class BaseDtoWithCtorName : BaseEmptyDto
{    
}

public class BaseDtoWithMethod : BaseEmptyDto
{
    public void Do() { }
}

public class BaseDtoWithConflictedMembers : BaseEmptyDto
{
    public bool Name1;
    public const string Name2 = "";
}

public class BaseDtoWithConflictedMembers_NotError : BaseEmptyDto
{
    private string Name1;
    protected string Name2;

    private string Name3 { get; set;}
    protected string Name4 { get; set;}

    private string Name5() => null;
    protected string Name6() => null;

    private const string Name7 = "";
    protected const string Name8 = "";
}

public class BaseDtoWithStatic : BaseEmptyDto
{
    private static string Name1 { get; set; }
    protected static string Name2 { get; set; }
    public static string Name3 { get; set; }
}

public class BaseDtoGeneric<T> : BaseEmptyDto
{
    public T Name1 { get; set; }
}



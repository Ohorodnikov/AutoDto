using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoDto.Tests.SourceGeneration.Models.HierarchyTestModels;

public class BaseBl
{
    public int Id { get; set; }
}

public class SimpleBl : BaseBl
{
    public int Name { get; set; }
}

public class BaseGenericBl<T> : BaseBl
{
    public T Name { get; set; }
}

public class GenericBl : BaseGenericBl<string>
{
    public string Name2 { get; set; }
}

public class BlWithNonPublicProp : BaseBl
{
    private string Name { get; set; }
    protected string Name2 { get; set; }
}

public class BlInheritedFromBlWithNonPublicProp : BlWithNonPublicProp
{
    public string Name3 { get; set; }
}

public class BlWithMembers : BaseBl
{
    public string Name { get; set; }

    public string MethodPublic() => string.Empty;
    protected string MethodProtected() => string.Empty;
    private string MethodPrivate() => string.Empty;

    public string FieldPublic;
    protected string FieldProtected;
#pragma warning disable CS0169 // The field 'BlWithMembers.FieldPrivate' is never used
    private string FieldPrivate;
#pragma warning restore CS0169 // The field 'BlWithMembers.FieldPrivate' is never used
}

public class BlInheritedFromBlWithMembers : BlWithMembers
{
    public string Name1 { get; set; }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoDto.Tests.SourceGeneration.Models;

public class TypeWithoutRelation
{
    public long Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
}

public class TypeWithoutId
{
    public string Name { get; set; }
    public string Description { get; set; }
}

public class TypeWithRelation
{
    public long Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }

    public TypeWithoutRelation WithId { get; set; }
}

public class TypeWithEnumerableRelation
{
    public long Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }

    public IEnumerable<TypeWithoutRelation> WithId { get; set; }
}

public class TypeWithArrayRelation
{
    public long Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }

    public TypeWithoutRelation[] WithId { get; set; }
}

public class TypeWithRelationWithoutId
{
    public long Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }

    public TypeWithoutId WithoutId { get; set; }
}

public class TypeWithEnumerableRelationWithoutId
{
    public long Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }

    public IEnumerable<TypeWithoutId> WithoutId { get; set; }
}

public class TypeWithArrayRelationWithoutId
{
    public long Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }

    public TypeWithoutId[] WithoutId { get; set; }
}

public class BaseBl<T>
{
    public T Id { get; set; }
}

public class BlWithHierarchy : BaseBl<string>
{
    public string Name { get; set; }
}

public class TypeWithRelWithHierarchy : BaseBl<int>
{
    public string Description { get; set; }
    public BlWithHierarchy Relation { get; set; }
}
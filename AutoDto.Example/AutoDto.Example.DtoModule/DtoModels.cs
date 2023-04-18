using AutoDto.Example.BlModule;
using AutoDto.Setup;
using System.Text.Json.Serialization;

namespace AutoDto.Example.DtoModule;

[DtoFrom(typeof(Entity1))]
[DtoIgnore(nameof(Entity1.B), "qq")]
public partial class Entity1Dto { }

[DtoFrom(typeof(Entity2))]
public partial class Entity2Dto { }

[DtoFrom(typeof(EntityWithSingleRel))]
public partial class EntityWithSingleRelDto_None { }

[DtoFrom(typeof(EntityWithSingleRel), RelationStrategy.ReplaceToIdProperty)]
public partial class EntityWithSingleRelDto_Replace2Id { }

[DtoFrom(typeof(EntityWithSingleRel), RelationStrategy.AddIdProperty)]
public partial class EntityWithSingleRelDto_AddId { }

[DtoFrom(typeof(EntityWithSingleRel), RelationStrategy.ReplaceToDtoProperty)]
public partial class EntityWithSingleRelDto_Replace2Dto { }

[DtoFrom(typeof(EntityWithEnumerableRel), RelationStrategy.AddIdProperty)]
public partial class EntityWithEnumerableRelDto_AddId { }

[DtoFrom(typeof(EntityWithEnumerableRel), RelationStrategy.ReplaceToIdProperty)]
public partial class EntityWithEnumerableRelDto_Replace2Id { }

[DtoFrom(typeof(EntityWithEnumerableRel), RelationStrategy.ReplaceToDtoProperty)]
public partial class EntityWithEnumerableRelDto_Replace2Dto
{
}
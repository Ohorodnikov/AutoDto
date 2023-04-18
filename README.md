# AutoDto

This tool allows to auto create DTO model from BL model in compile time.

It supports different strategies for relations, such as ReplaceToIdProperty, AddIdProperty and ReplaceToDtoProperty

## Setup

To use AutoDto, first install the [NuGet package](https://www.nuget.org/packages/AutoDto):
```shell
dotnet add package AutoDto
```

- Declare partial DTO type like `public partial SomeDto {}`
- Add attribute `[DtoFrom(typeof(SomeBlType))]`
- Build project

Full simple setup:

```csharp
[DtoFrom(typeof(SomeBlType))]
public partial SomeDto {}
```
 
AutoDto tool will generate partial SomeDto class with all public properties from SomeBlType.

## Relation Strategies

Relation strategy means what to do with relation property during generating DTO.

Supported strategies:
- None
- ReplaceToIdProperty
- AddIdProperty
- ReplaceToDtoProperty

### Usage

Set strategy in `[DtoFrom]` attribute after BL type.

```csharp
[DtoFrom(typeof(SomeBlType), RelationStrategy.AddIdProperty)]
public partial SomeDto {}
```

> If not specified - RelationStrategy.None will be used

### None
DTO will have property on BL type

### ReplaceToIdProperty
If BL relation property has `Id` property:
DTO will have only RelationPropName**Id** property of BL relation Id prop type.
If BlType has relation with array or enumerable - generated name will be RelationPropName**Ids**

> :exclamation: If no `Id` found in relation entity - result will be same as with None strategy

### AddIdProperty
DTO will have relation type property and `Id` property is found

### ReplaceToDtoProperty
Try find DTO, generated for RelationType and replace to RelationTypeDto.

> :exclamation: If many DTOs exists for RelationType - use `[MainDto]` attribute to mark which one should be used in `ReplaceToDtoProperty`


## Ignore properties

To avoid some properties from BlType, use `[DtoIgnore]` attribute:

```csharp
[DtoFrom(typeof(SomeBlType), RelationStrategy.AddIdProperty)]
[DtoIgnore(nameof(SomeBlType.PropName1), nameof(SomeBlType.PropName2))]
public partial SomeDto {}
```




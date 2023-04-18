namespace AutoDto.Setup;

/// <summary>
/// Strategy what to do with property on Bl entity
/// </summary>
public enum RelationStrategy
{
    /// <summary>
    /// Dto will have property to Bl
    /// 
    /// result:
    /// public MyBlRelation PropName {get; set;}
    /// </summary>
    None,
    /// <summary>
    /// Replace Bl relation on Id if possible
    /// 
    /// result:
    /// public int PropNameId {get; set;}
    /// </summary>
    ReplaceToIdProperty,
    /// <summary>
    /// Result will contain refence on Bl entity and Id prop
    /// 
    /// result:
    /// public MyBlRelation PropName {get; set;}
    /// public int PropNameId {get; set;}
    /// </summary>
    AddIdProperty,
    /// <summary>
    /// if Dto exists
    /// public MyBlRelationDto PropName {get; set;}
    /// else
    /// public MyBlRelation PropName {get; set;}
    /// 
    /// If more than one Dto on Bl entity exists - main Dto should be marked with [MainDto] attribute
    /// </summary>
    ReplaceToDtoProperty
}

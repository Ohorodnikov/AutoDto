namespace AutoDto.Example.BlModule;

public class Entity<T>
{
    public T Id { get; set; }
}

public class Entity1 : Entity<int>
{
    public string Name { get; set; }
    public string Description { get; set; }
    public int A { get; set; }
    public int B { get; set; }
}

public class Entity2 : Entity<int>
{
    public string Name2 { get; set; }
    public string Description2 { get; set; }
}

public class EntityWithSingleRel : Entity<int>
{
    public string Code { get; set; }
    public Entity1 Entity1 { get; set; }
}

public class EntityWithEnumerableRel : Entity<int>
{
    public string Code { get; set; }
    public IEnumerable<Entity2> Entities { get; set; }
}
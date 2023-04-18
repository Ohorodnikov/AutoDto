using AutoDto.Example.BlModule;
using AutoDto.Example.DtoModule;

var bl = new Entity1
{
    A = 1
};

var ent1Dto = new Entity1Dto
{
    A = bl.A,    
};

var withEnumer = new EntityWithEnumerableRelDto_Replace2Dto
{
    Entities = new Entity2Dto[]
    {
        new Entity2Dto()
    }
};



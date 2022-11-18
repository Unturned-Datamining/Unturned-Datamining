namespace SDG.Unturned;

public class AnimalSpawn
{
    private ushort _animal;

    public ushort animal => _animal;

    public AnimalSpawn(ushort newAnimal)
    {
        _animal = newAnimal;
    }
}

namespace SDG.Unturned;

public interface ISleekProxyImplementation : ISleekElement
{
    SleekWrapper GetWrapper();

    T GetWrapper<T>() where T : SleekWrapper
    {
        return GetWrapper() as T;
    }
}

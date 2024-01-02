namespace WirelessCom.Domain.Exceptions;

public class GenericConcurrentDictionaryAddException<TKey> : Exception where TKey : notnull
{
    public GenericConcurrentDictionaryAddException(TKey key) : base(
        $"Failed to add object with the key: {key} from the dictionary"
    )
    {
    }
}
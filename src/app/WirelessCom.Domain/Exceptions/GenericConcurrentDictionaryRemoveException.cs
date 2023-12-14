namespace WirelessCom.Domain.Exceptions;

public class GenericConcurrentDictionaryRemoveException<TKey> : Exception where TKey : notnull
{
    public GenericConcurrentDictionaryRemoveException(TKey key) : base(
        $"Failed to remove object with the key: {key} from the dictionary"
    )
    {
    }
}
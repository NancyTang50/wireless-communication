namespace WirelessCom.Domain.Exceptions;

public class GenericConcurrentDictionaryUpdateException<TKey> : Exception where TKey : notnull
{
    public GenericConcurrentDictionaryUpdateException(TKey key) : base(
        $"Failed to update object with the key: {key} from the dictionary"
    )
    {
    }
}
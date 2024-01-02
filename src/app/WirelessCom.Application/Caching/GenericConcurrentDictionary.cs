using System.Collections.Concurrent;
using WirelessCom.Domain.Exceptions;

namespace WirelessCom.Application.Caching;

/// <summary>
///     This contains a generic <see cref="ConcurrentDictionary{TKey,TValue}" /> and the methods to store and get objects
///     from it.
/// </summary>
/// <typeparam name="TKey">The key of the objects that you want to store.</typeparam>
/// <typeparam name="TValue">The objects that you want to store.</typeparam>
public class GenericConcurrentDictionary<TKey, TValue> where TValue : class where TKey : notnull
{
    public delegate Task? ItemAddedEventHandler(TKey key, TValue value);

    public delegate Task? ItemRemovedEventHandler(TKey key, TValue value);

    private readonly ConcurrentDictionary<TKey, TValue> _dictionary = new();

    /// <summary>
    ///     Get the amount of items in the <see cref="ConcurrentDictionary{TKey,TValue}" />.
    /// </summary>
    /// <returns>
    ///     A the amount of items in the <see cref="ConcurrentDictionary{TKey,TValue}" />.
    /// </returns>
    public virtual int Count => _dictionary.Count;

    public event ItemAddedEventHandler? ItemAdded;
    public event ItemRemovedEventHandler? ItemRemoved;

    /// <summary>
    ///     Adds a <see cref="TValue" /> to the <see cref="ConcurrentDictionary{TKey,TValue}" />.
    /// </summary>
    /// <exception cref="GenericConcurrentDictionaryUpdateException{TKey}">
    ///     Thrown when it fails to update the <see cref="TValue" /> to the <see cref="_dictionary" />.
    /// </exception>
    /// <exception cref="GenericConcurrentDictionaryAddException{TKey}">
    ///     Thrown when it fails to add the <see cref="TValue" />
    ///     to the <see cref="_dictionary" />.
    /// </exception>
    /// <typeparam name="TKey">The key of the objects that you want to store.</typeparam>
    /// <typeparam name="TValue">The objects that you want to store.</typeparam>
    public virtual void AddOrUpdate(TKey key, TValue value)
    {
        ItemAdded?.Invoke(key, value);

        if (_dictionary.ContainsKey(key))
        {
            var oldValue = Get(key);
            if (!_dictionary.TryUpdate(key, value, oldValue!))
            {
                throw new GenericConcurrentDictionaryUpdateException<TKey>(key);
            }

            return;
        }

        if (!_dictionary.TryAdd(key, value))
        {
            throw new GenericConcurrentDictionaryAddException<TKey>(key);
        }
    }

    /// <summary>
    ///     Get a <see cref="TValue" /> from the <see cref="ConcurrentDictionary{TKey,TValue}" />.
    /// </summary>
    /// <param name="key">The <see cref="TKey" /> of the <see cref="TValue" /> that you want to get.</param>
    /// <returns>
    ///     The requested <see cref="TValue" />.
    /// </returns>
    public virtual TValue? Get(TKey key)
    {
        return !_dictionary.ContainsKey(key) ? null : _dictionary[key];
    }

    /// <summary>
    ///     Get all the <see cref="TValue" />s in the <see cref="ConcurrentDictionary{TKey,TValue}" />.
    /// </summary>
    /// <returns>
    ///     A <see cref="List{T}" /> of <see cref="TValue" />s.
    /// </returns>
    public virtual List<KeyValuePair<TKey, TValue>> GetAll()
    {
        return _dictionary.ToList();
    }

    /// <summary>
    ///     Remove a <see cref="TValue" /> fro the <see cref="ConcurrentDictionary{TKey,TValue}" />.
    /// </summary>
    /// <param name="key">The <see cref="TKey" /> of the <see cref="TValue" /> that you want to remove.</param>
    /// <exception cref="GenericConcurrentDictionaryRemoveException{TKey}">
    ///     >Thrown when it failed to remove the
    ///     <see cref="TValue" /> to the <see cref="_dictionary" />.
    /// </exception>
    /// <returns>
    ///     Whether or not an item has been removed.
    /// </returns>
    public virtual bool Remove(TKey key)
    {
        if (!_dictionary.ContainsKey(key))
        {
            return false;
        }

        if (!_dictionary.TryRemove(key, out var value))
        {
            throw new GenericConcurrentDictionaryRemoveException<TKey>(key);
        }

        ItemRemoved?.Invoke(key, value);
        return true;
    }
}
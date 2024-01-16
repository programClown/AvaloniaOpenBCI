using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace AvaloniaOpenBCI.Helper.Cache;

[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public class LRUCache<TK, TV>
    where TK : notnull
{
    private readonly int _capacity;
    private readonly Dictionary<TK, LinkedListNode<LRUCacheItem<TK, TV>>> _cacheMap = new();
    private readonly LinkedList<LRUCacheItem<TK, TV>> _lruList = new();

    public LRUCache(int capacity)
    {
        this._capacity = capacity;
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public TV? Get(TK key)
    {
        if (_cacheMap.TryGetValue(key, out var node))
        {
            var value = node.Value.Value;
            _lruList.Remove(node);
            _lruList.AddLast(node);
            return value;
        }
        return default;
    }

    public bool Get(TK key, out TV? value)
    {
        value = Get(key);
        return value != null;
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Add(TK key, TV val)
    {
        if (_cacheMap.TryGetValue(key, out var existingNode))
        {
            _lruList.Remove(existingNode);
        }
        else if (_cacheMap.Count >= _capacity)
        {
            RemoveFirst();
        }

        var cacheItem = new LRUCacheItem<TK, TV>(key, val);
        var node = new LinkedListNode<LRUCacheItem<TK, TV>>(cacheItem);
        _lruList.AddLast(node);
        _cacheMap[key] = node;
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Remove(TK key)
    {
        if (!_cacheMap.TryGetValue(key, out var node))
            return;

        _lruList.Remove(node);
        _cacheMap.Remove(key);
    }

    private void RemoveFirst()
    {
        // Remove from LRUPriority
        var node = _lruList.First;
        _lruList.RemoveFirst();

        if (node == null)
            return;

        // Remove from cache
        _cacheMap.Remove(node.Value.Key);
    }
}

// ReSharper disable once InconsistentNaming
internal class LRUCacheItem<TK, TV>
{
    public LRUCacheItem(TK k, TV v)
    {
        Key = k;
        Value = v;
    }

    public TK Key;
    public TV Value;
}

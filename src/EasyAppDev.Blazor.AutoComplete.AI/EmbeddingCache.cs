using Microsoft.Extensions.AI;
using System.Collections.Concurrent;

namespace EasyAppDev.Blazor.AutoComplete.AI;

/// <summary>
/// Provides high-performance caching functionality for embeddings with LRU eviction to reduce API calls.
/// Uses lock-free concurrent collections for optimal performance under high concurrency.
/// </summary>
/// <typeparam name="TItem">The type of items to cache embeddings for.</typeparam>
public class EmbeddingCache<TItem> where TItem : notnull
{
    private readonly ConcurrentDictionary<TItem, CachedEmbedding> _cache = new();
    private readonly LinkedList<TItem> _lruList = new();
    private readonly object _lruLock = new();
    private readonly TimeSpan _ttl;
    private readonly int _maxSize;
    private long _hits;
    private long _misses;

    /// <summary>
    /// Initializes a new instance of the <see cref="EmbeddingCache{TItem}"/> class.
    /// </summary>
    /// <param name="ttl">Time-to-live for cached embeddings.</param>
    /// <param name="maxSize">Maximum number of embeddings to cache. Default is 10,000. Use -1 for unlimited.</param>
    public EmbeddingCache(TimeSpan ttl, int maxSize = 10_000)
    {
        if (maxSize == 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxSize), "Max size must be greater than 0 or -1 for unlimited.");
        }

        _ttl = ttl;
        _maxSize = maxSize;
    }

    /// <summary>
    /// Gets an embedding from cache or creates a new one using the provided factory.
    /// Implements LRU eviction policy when cache is full.
    /// </summary>
    /// <param name="item">The item to get or create an embedding for.</param>
    /// <param name="factory">Factory function to create a new embedding if not in cache.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The cached or newly created embedding.</returns>
    public async Task<Embedding<float>> GetOrCreateAsync(
        TItem item,
        Func<Task<Embedding<float>>> factory,
        CancellationToken cancellationToken)
    {
        // Fast path: check cache (lock-free read)
        if (_cache.TryGetValue(item, out var cached))
        {
            var age = DateTime.UtcNow - cached.Timestamp;
            if (age < _ttl)
            {
                Interlocked.Increment(ref _hits);
                TouchItem(item); // Update LRU position
                return cached.Embedding;
            }

            // Expired - remove it
            _cache.TryRemove(item, out _);
            lock (_lruLock)
            {
                _lruList.Remove(item);
            }
        }

        // Cache miss - generate new embedding
        Interlocked.Increment(ref _misses);
        var embedding = await factory();

        var newCached = new CachedEmbedding
        {
            Embedding = embedding,
            Timestamp = DateTime.UtcNow
        };

        // Add to cache with size management
        AddWithEviction(item, newCached);

        return embedding;
    }

    /// <summary>
    /// Adds an item to the cache, evicting the least recently used item if the cache is full.
    /// </summary>
    private void AddWithEviction(TItem item, CachedEmbedding cached)
    {
        _cache[item] = cached;

        lock (_lruLock)
        {
            // Remove if already in list (shouldn't happen in normal flow, but defensive)
            _lruList.Remove(item);

            // Add to end (most recently used)
            _lruList.AddLast(item);

            // Evict oldest items if over limit
            if (_maxSize > 0)
            {
                while (_lruList.Count > _maxSize)
                {
                    var oldest = _lruList.First!.Value;
                    _lruList.RemoveFirst();
                    _cache.TryRemove(oldest, out _);
                }
            }
        }
    }

    /// <summary>
    /// Updates the LRU position for an item (marks it as recently used).
    /// </summary>
    private void TouchItem(TItem item)
    {
        lock (_lruLock)
        {
            if (_lruList.Remove(item))
            {
                _lruList.AddLast(item);
            }
        }
    }

    /// <summary>
    /// Clears all cached embeddings.
    /// </summary>
    public Task ClearAsync()
    {
        _cache.Clear();
        lock (_lruLock)
        {
            _lruList.Clear();
        }
        Interlocked.Exchange(ref _hits, 0);
        Interlocked.Exchange(ref _misses, 0);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Gets the current number of cached embeddings (lock-free).
    /// </summary>
    public int Count => _cache.Count;

    /// <summary>
    /// Gets the cache hit rate (0.0 to 1.0).
    /// </summary>
    public double HitRate
    {
        get
        {
            var totalRequests = _hits + _misses;
            return totalRequests == 0 ? 0.0 : (double)_hits / totalRequests;
        }
    }

    /// <summary>
    /// Gets the total number of cache hits.
    /// </summary>
    public long Hits => Interlocked.Read(ref _hits);

    /// <summary>
    /// Gets the total number of cache misses.
    /// </summary>
    public long Misses => Interlocked.Read(ref _misses);

    /// <summary>
    /// Removes expired embeddings from the cache.
    /// </summary>
    public Task CleanupExpiredAsync()
    {
        var now = DateTime.UtcNow;
        var expiredKeys = _cache
            .Where(kvp => now - kvp.Value.Timestamp >= _ttl)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var key in expiredKeys)
        {
            _cache.TryRemove(key, out _);
        }

        // Clean up LRU list
        if (expiredKeys.Count > 0)
        {
            lock (_lruLock)
            {
                foreach (var key in expiredKeys)
                {
                    _lruList.Remove(key);
                }
            }
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Resets cache statistics (hits, misses).
    /// </summary>
    public void ResetStatistics()
    {
        Interlocked.Exchange(ref _hits, 0);
        Interlocked.Exchange(ref _misses, 0);
    }

    private class CachedEmbedding
    {
        public required Embedding<float> Embedding { get; init; }
        public required DateTime Timestamp { get; init; }
    }
}

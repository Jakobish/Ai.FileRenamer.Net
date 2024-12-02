using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;

namespace FileRenamerProject.Services;

public class NameSuggestionCache : INameSuggestionCache
{
    private readonly ConcurrentDictionary<string, string> _cache = new();
    private const int MaxCacheSize = 1000;

    public string GetCachedSuggestion(string content)
    {
        var hash = ComputeHash(content);
        return _cache.TryGetValue(hash, out var suggestion) ? suggestion : null;
    }

    public void CacheSuggestion(string content, string suggestion)
    {
        if (string.IsNullOrEmpty(content) || string.IsNullOrEmpty(suggestion))
            return;

        var hash = ComputeHash(content);
        
        // If cache is full, remove random entries
        while (_cache.Count >= MaxCacheSize)
        {
            var keyToRemove = _cache.Keys.First();
            _cache.TryRemove(keyToRemove, out _);
        }

        _cache.TryAdd(hash, suggestion);
    }

    public void Clear()
    {
        _cache.Clear();
    }

    private static string ComputeHash(string input)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(input);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }
}

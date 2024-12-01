namespace FileRenamerProject.Services;

public interface INameSuggestionCache
{
    string GetCachedSuggestion(string content);
    void CacheSuggestion(string content, string suggestion);
    void Clear();
}

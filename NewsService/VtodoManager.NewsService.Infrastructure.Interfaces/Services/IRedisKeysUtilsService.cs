namespace VtodoManager.NewsService.Infrastructure.Interfaces.Services;

internal interface IRedisKeysUtilsService
{
    IAsyncEnumerable<string> GetKeysAsync(string pattern);
    Task RemoveKeysByKeyroot(string keyRoot);
}
using DddApi.Models;
using Newtonsoft.Json;
namespace DddApi.Services;
public class RegiaoService : IRegiaoService
{
    private readonly IHttpClientFactory _httpClient;
    private List<Regiao> _cache = new();


    public RegiaoService(IHttpClientFactory httpClient)
    {
        _httpClient = httpClient;
    }
    public IEnumerable<Regiao> GetCachedRegioes() => _cache;
    public async Task<List<Regiao>> GetRegioesAsync()
    {
        var client = _httpClient.CreateClient("regiao");
        var response = await client.GetAsync("regiao");

        if (response.IsSuccessStatusCode)
        {
            var regioes = await response.Content.ReadFromJsonAsync<List<Regiao>>();
            if(regioes != null && regioes.Any())
                _cache = regioes;
        }
        return _cache;        
    }

    public async Task<bool> UpdateCache(EventTypes eventType, Regiao regiao)
    {
        if (!_cache.Any())
            await GetRegioesAsync();

        switch (eventType)
        {
            case EventTypes.CREATE:
                _cache.Add(regiao);
                return true;
            case EventTypes.UPDATE:
                var updated = _cache.FirstOrDefault(o => o.Id == regiao.Id);
                if(updated is Regiao)
                {
                    _cache.Remove(updated);
                    _cache.Add(regiao);
                    return true;
                }
                return false;
            case EventTypes.DELETE:
                var deleted = _cache.FirstOrDefault(o => o.Id == regiao.Id);
                if(deleted is Regiao)
                {
                    _cache.Remove(deleted);
                    return true;
                }
                return false;
            default:
                return false;

        } 
    }
}

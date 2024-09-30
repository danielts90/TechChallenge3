using DddApi.Models;

namespace DddApi.Services;
public interface IRegiaoService
{
    Task<List<Regiao>> GetRegioesAsync();
    bool UpdateCache(EventTypes eventType, Regiao regiao);
    IEnumerable<Regiao> GetCachedRegioes();
}
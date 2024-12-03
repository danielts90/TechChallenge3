using DddApi.Models;

namespace DDdApi.Models;
public class DddDto
{
    public int? Id { get; set;}
    public string? Code { get; set; }
    public int? RegiaoId {get; set;}
    public Regiao? Regiao {get; set;}
}
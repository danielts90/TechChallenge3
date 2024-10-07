using DDdApi.Models;

namespace DddApi.Entities;

public class Ddd
{
    public int? Id { get; set;}
    public string? Code { get; set; }
    public int? RegiaoId {get; set;}

    public static implicit operator DddDto(Ddd ddd)
    {
        DddDto dto = new();
        dto.Id = ddd.Id;
        dto.Code = ddd.Code;
        dto.RegiaoId = ddd.RegiaoId;

        return dto;
    }
}

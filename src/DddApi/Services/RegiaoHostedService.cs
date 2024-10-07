namespace DddApi.Services;

public class RegiaoHostedService : IHostedService
{
    private readonly IRegiaoService _regiaoService;

    public RegiaoHostedService(IRegiaoService regiaoService)
    {
        _regiaoService = regiaoService;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _regiaoService.GetRegioesAsync();
        }
        catch(Exception ex)
        {
            Console.WriteLine(ex.StackTrace);
            Console.WriteLine("Não foi possível obter a lista de regiões. " + ex.Message);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}

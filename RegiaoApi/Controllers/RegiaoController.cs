using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RegiaoApi.Context;
using RegiaoApi.Models;

namespace RegiaoApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RegiaoController : ControllerBase
    {
        private readonly ILogger<RegiaoController> _logger;
        private readonly RegiaoDb _db;
        private readonly IMessageProducer _producer;

        public RegiaoController(ILogger<RegiaoController> logger, IMessageProducer producer, RegiaoDb db)
        {
            _logger = logger;
             _producer = producer;
            _db = db;
        }
        
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var regioes = await _db.Regioes.ToArrayAsync();
            return Ok(regioes);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var regiao = await _db.Regioes.FindAsync(id);
            if (regiao is not null)
            {
                return Ok(regiao);
            }
            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Regiao regiao)
        {
            _db.Regioes.Add(regiao);
            await _db.SaveChangesAsync();

            var message = new Message<Regiao>(EventTypes.CREATE, regiao);
            _producer.SendMessageToQueue(message);

            return CreatedAtAction(nameof(GetById), new { id = regiao.Id }, regiao);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRegiao(int id, Regiao inputRegiao)
        {
            var regiao = await _db.Regioes.FindAsync(id);

            if (regiao is null) return NotFound();

            regiao.Name = inputRegiao.Name;
            await _db.SaveChangesAsync();

            var message = new Message<Regiao>(EventTypes.UPDATE, regiao);
            _producer.SendMessageToQueue(message);

            return Ok(regiao);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRegiao(int id)
        {
            if (await _db.Regioes.FindAsync(id) is Regiao regiao)
            {
                _db.Regioes.Remove(regiao);
                await _db.SaveChangesAsync();

                var message = new Message<Regiao>(EventTypes.DELETE, regiao);
                _producer.SendMessageToQueue(message);

                return Ok();
            }
            return NotFound();
        }
    }
}

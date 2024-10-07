using DddApi.Context;
using DddApi.Entities;
using DddApi.Models;
using DddApi.RabbitMq;
using DddApi.Services;
using DDdApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DddApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DddController : ControllerBase
    {
        private readonly DddDb _db;
        private readonly IMessageProducer _producer;
        private readonly IRegiaoService _regiaoService;

        public DddController(DddDb db, IMessageProducer producer, IRegiaoService regiaoService)
        {
            _db = db;
            _producer = producer;
            _regiaoService = regiaoService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var ddds = await _db.Ddds.ToArrayAsync();
            return Ok(ddds);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            DddDto ddd = await _db.Ddds.FindAsync(id);
            if (ddd == null)
            {
                return NotFound();
            }

            var regioes = _regiaoService.GetCachedRegioes();
            ddd.Regiao = regioes.FirstOrDefault(o => o.Id == ddd.RegiaoId);

            return Ok(ddd);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Ddd ddd)
        {
            _db.Ddds.Add(ddd);
            await _db.SaveChangesAsync();

            var message = new Message<Ddd>(EventTypes.CREATE, ddd);
            _producer.SendMessageToQueue(message);

            return CreatedAtAction(nameof(GetById), new { id = ddd.Id }, ddd);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Ddd inputDdd)
        {
            var ddd = await _db.Ddds.FindAsync(id);
            if (ddd == null)
            {
                return NotFound();
            }

            ddd.Code = inputDdd.Code;
            await _db.SaveChangesAsync();

            var message = new Message<Ddd>(EventTypes.UPDATE, ddd);
            _producer.SendMessageToQueue(message);

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var ddd = await _db.Ddds.FindAsync(id);
            if (ddd == null)
            {
                return NotFound();
            }

            _db.Ddds.Remove(ddd);
            await _db.SaveChangesAsync();

            var message = new Message<Ddd>(EventTypes.DELETE, ddd);
            _producer.SendMessageToQueue(message);

            return NoContent();
        }
    }
}

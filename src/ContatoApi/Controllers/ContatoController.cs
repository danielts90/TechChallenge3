using ContatoApi.Context;
using ContatoApi.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ContatoApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ContatoController : ControllerBase
    {
        private readonly ContatoDb _db;

        public ContatoController(ContatoDb db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var contatos = await _db.Contatos.ToArrayAsync();
            return Ok(contatos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var contato = await _db.Contatos.FindAsync(id);
            if (contato == null)
            {
                return NotFound();
            }
            return Ok(contato);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Contato contato)
        {
            _db.Contatos.Add(contato);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = contato.Id }, contato);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Contato inputContato)
        {
            var contato = await _db.Contatos.FindAsync(id);
            if (contato == null)
            {
                return NotFound();
            }

            contato.Nome = inputContato.Nome;
            contato.Email = inputContato.Email;
            contato.Telefone = inputContato.Telefone;

            await _db.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var contato = await _db.Contatos.FindAsync(id);
            if (contato == null)
            {
                return NotFound();
            }

            _db.Contatos.Remove(contato);
            await _db.SaveChangesAsync();

            return NoContent();
        }
    }
}

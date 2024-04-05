using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechChallenge_Fase01.Data;
using TechChallenge_Fase01.Models;
using TechChallenge_Fase01.Models.Requests;
using TechChallenge_Fase01.Validators;

namespace TechChallenge_Fase01.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContactsController(AppDbContext dbContext) : ControllerBase
    {
        private readonly AppDbContext _dbContext = dbContext;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Contact>>> GetAll([FromQuery] FilteredContactsRequest request)
        {
            FilteredContactsRequestValidator validator = new();
            var result = validator.Validate(request);

            if (!result.IsValid)
            {
                return BadRequest(result.Errors);
            }

            var ddd = request.DDD;
            var contacts = await _dbContext.Contacts
                .Where(x => ddd == null || x.DDD == ddd)
                .ToListAsync();

            return Ok(contacts);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Contact>> Get(int id)
        {
            var contact = await _dbContext.Contacts.FindAsync(id);

            if (contact is null)
            {
                return NotFound();
            }

            return contact;
        }

        [HttpPost]
        public async Task<ActionResult<Contact>> Post([FromBody] CreateContactRequest request)
        {
            var contact = new Contact()
            {
                Name = request.Name,
                Phone = request.Phone,
                Email = request.Email,
                DDD = request.DDD,
            };

            _dbContext.Contacts.Add(contact);
            await _dbContext.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { id = contact.Id }, request);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Put(int id, [FromBody] Contact request)
        {
            var contact = await _dbContext.Contacts.FindAsync(id);

            if (contact is null)
            {
                return NotFound();
            }

            contact.Name = request.Name;
            contact.Phone = request.Phone;
            contact.Email = request.Email;
            contact.DDD = request.DDD;

            await _dbContext.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var contact = await _dbContext.Contacts.FindAsync(id);

            if (contact is null)
            {
                return NotFound();
            }

            _dbContext.Contacts.Remove(contact);
            await _dbContext.SaveChangesAsync();

            return NoContent();
        }
    }
}

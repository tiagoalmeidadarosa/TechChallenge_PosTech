using FluentValidation.AspNetCore;
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
    public sealed class ContactsController(AppDbContext dbContext) : ControllerBase
    {
        private readonly AppDbContext _dbContext = dbContext;

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Contact>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<Contact>>> GetAll([FromQuery] FilteredContactsRequest request)
        {
            FilteredContactsRequestValidator validator = new();
            var result = validator.Validate(request);

            if (!result.IsValid)
            {
                result.AddToModelState(ModelState);
                return ValidationProblem();
            }

            var ddd = request.DDD;
            var contacts = await _dbContext.Contacts
                .Where(x => ddd == null || x.DDD == ddd)
                .ToListAsync();

            return Ok(contacts);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Contact), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Contact>> Get(int id)
        {
            var contact = await _dbContext.Contacts.FindAsync(id);

            if (contact is null)
            {
                return NotFound("Contact not found.");
            }

            return Ok(contact);
        }

        [HttpPost]
        [ProducesResponseType(typeof(Contact), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Contact>> Post([FromBody] ContactRequest request)
        {
            ContactRequestValidator validator = new();
            var result = validator.Validate(request);

            if (!result.IsValid)
            {
                result.AddToModelState(ModelState);
                return ValidationProblem();
            }

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
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> Put(int id, [FromBody] ContactRequest request)
        {
            ContactRequestValidator validator = new();
            var result = validator.Validate(request);

            if (!result.IsValid)
            {
                result.AddToModelState(ModelState);
                return ValidationProblem();
            }

            var contact = await _dbContext.Contacts.FindAsync(id);

            if (contact is null)
            {
                return NotFound("Contact not found.");
            }

            contact.Name = request.Name;
            contact.Phone = request.Phone;
            contact.Email = request.Email;
            contact.DDD = request.DDD;

            await _dbContext.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> Delete(int id)
        {
            var contact = await _dbContext.Contacts.FindAsync(id);

            if (contact is null)
            {
                return NotFound("Contact not found.");
            }

            _dbContext.Contacts.Remove(contact);
            await _dbContext.SaveChangesAsync();

            return NoContent();
        }
    }
}

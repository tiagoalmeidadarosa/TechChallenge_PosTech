using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using TechChallenge_Fase01.Interfaces;
using TechChallenge_Fase01.Models;
using TechChallenge_Fase01.Models.Requests;
using TechChallenge_Fase01.Validators;

namespace TechChallenge_Fase01.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public sealed class ContactsController(IContactRepository contactRepository, ILogger<ContactsController> logger, IMemoryCache cache) : ControllerBase
    {
        private readonly IContactRepository _contactRepository = contactRepository;
        private readonly ILogger<ContactsController> _logger = logger;
        private readonly IMemoryCache _cache = cache;

        private const string CacheKeyPrefix = "contacts_ddd_";

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

            var cacheKey = $"{CacheKeyPrefix}{request.DDD}";
            if (_cache.TryGetValue(cacheKey, out IList<Contact>? cachedContacts))
            {
                return Ok(cachedContacts);
            }

            var contacts = await _contactRepository.GetAllAsync(request.DDD);
            _cache.Set(cacheKey, contacts, TimeSpan.FromMinutes(5));

            return Ok(contacts);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Contact), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Contact>> Get(int id)
        {
            var contact = await _contactRepository.GetByIdAsync(id);

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

            await _contactRepository.CreateAsync(contact);
            InvalidateCache(request.DDD);

            _logger.LogInformation("Contact created: {ContactId}", contact.Id);

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

            var contact = await _contactRepository.GetByIdAsync(id);

            if (contact is null)
            {
                return NotFound("Contact not found.");
            }

            if (contact.DDD != request.DDD)
            {
                InvalidateCache(contact.DDD);
            }

            contact.Name = request.Name;
            contact.Phone = request.Phone;
            contact.Email = request.Email;
            contact.DDD = request.DDD;

            await _contactRepository.UpdateAsync(contact);
            InvalidateCache(request.DDD);

            _logger.LogInformation("Contact updated: {ContactId}", contact.Id);

            return NoContent();
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> Delete(int id)
        {
            var contact = await _contactRepository.GetByIdAsync(id);

            if (contact is null)
            {
                return NotFound("Contact not found.");
            }

            await _contactRepository.DeleteAsync(contact);
            InvalidateCache(contact.DDD);

            _logger.LogInformation("Contact deleted: {ContactId}", contact.Id);

            return NoContent();
        }

        private void InvalidateCache(int ddd)
        {
            _cache.Remove($"{CacheKeyPrefix}{ddd}");
            _cache.Remove(CacheKeyPrefix);
        }
    }
}

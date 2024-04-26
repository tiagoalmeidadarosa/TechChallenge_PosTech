using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using TechChallenge_Fase01.API.Models.Requests;
using TechChallenge_Fase01.API.Models.Responses;
using TechChallenge_Fase01.API.Validators;
using TechChallenge_Fase01.Core.Entities;
using TechChallenge_Fase01.Core.Interfaces;

namespace TechChallenge_Fase01.API.Controllers
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
        [ProducesResponseType(typeof(IEnumerable<ContactResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<ContactResponse>>> GetAll([FromQuery] FilteredContactsRequest request)
        {
            //Validate request model
            FilteredContactsRequestValidator validator = new();
            var result = validator.Validate(request);

            if (!result.IsValid)
            {
                result.AddToModelState(ModelState);
                return ValidationProblem();
            }

            //Verify if data is cached
            var cacheKey = $"{CacheKeyPrefix}{request.DDD}";
            if (_cache.TryGetValue(cacheKey, out IEnumerable<ContactResponse>? cachedContacts))
            {
                return Ok(cachedContacts);
            }

            //Return data from database
            var contacts = await _contactRepository.GetAllAsync(request.DDD);
            var contactsResponse = contacts.Select(x => new ContactResponse()
            {
                Id = x.Id,
                Name = x.Name,
                Phone = x.Phone,
                Email = x.Email,
                DDD = x.DDD,
            });

            //Set cache for a new request
            _cache.Set(cacheKey, contactsResponse, TimeSpan.FromMinutes(5));

            return Ok(contactsResponse);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ContactResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ContactResponse>> Get(int id)
        {
            var contact = await _contactRepository.GetByIdAsync(id);

            if (contact is null)
            {
                return NotFound("Contact not found.");
            }

            return Ok(new ContactResponse()
            {
                Id = contact.Id,
                Name = contact.Name,
                Phone = contact.Phone,
                Email = contact.Email,
                DDD = contact.DDD,
            });
        }

        [HttpPost]
        [ProducesResponseType(typeof(Contact), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ContactResponse>> Post([FromBody] ContactRequest request)
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

            return Created(nameof(Get), new ContactResponse { Id = contact.Id });
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

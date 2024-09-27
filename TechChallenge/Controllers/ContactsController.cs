using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using TechChallenge.API.Models.Requests;
using TechChallenge.API.Models.Responses;
using TechChallenge.API.Validators;
using TechChallenge.Core.Interfaces;

namespace TechChallenge.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public sealed class ContactsController(IContactRepository contactRepository) : ControllerBase
    {
        private readonly IContactRepository _contactRepository = contactRepository;

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
    }
}

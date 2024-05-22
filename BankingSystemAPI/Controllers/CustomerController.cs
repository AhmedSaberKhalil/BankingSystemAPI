using Domain.Models;
using Domain.Repository;
using Domain.UnitOfWork;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;

namespace BankingSystemAPI.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	[Authorize]
	public class CustomerController : ControllerBase
	{

		private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CustomerController> _logger;

        public CustomerController(IUnitOfWork unitOfWork, ILogger<CustomerController> logger)
		{
			this._unitOfWork = unitOfWork;
            this._logger = logger;
        }

		[HttpGet("GetAll")]
		[ResponseCache(Duration = 30)]
        public async Task<IActionResult> GetAll()
        {
            _logger.LogInformation("Fetching all customers.");

            try
            {
                var customers = await _unitOfWork.Customers.GetAllAsync(); 
                return Ok(customers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all customers.");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("GetById/{id}", Name = "CustomerDetailsRoute")]
        public async Task<IActionResult> GetById(int id)
        {
            _logger.LogInformation("Fetching customer with ID: {id}", id);

            var customer = await _unitOfWork.Customers.GetByIdAsync(id); // Async call
            if (customer == null)
            {
                _logger.LogWarning("Customer with ID: {id} not found.", id);
                return NotFound("Data Not Found");
            }

            return Ok(customer);
        }
        [HttpGet("Find")]
		public async Task<IActionResult> Find(string search )
		{
			Expression<Func<Customer, bool>> criteria = c =>
	          c.Name.Contains(search) ||
	          c.Email.Contains(search) ||
	          c.Address.Contains(search) ||
			  c.Phone.Contains(search);
			return Ok(await _unitOfWork.Customers.FindAsync(criteria));
		}

        [HttpPost("Add")]
        public async Task<IActionResult> AddAsync([FromBody] Customer customer)
        {
            _logger.LogInformation("Adding new customer.");

            try
            {
                if (ModelState.IsValid)
                {
                    await _unitOfWork.Customers.AddAsync(customer);
                     _unitOfWork.Complete();
                    _logger.LogInformation("Customer added successfully. ID: {Id}", customer.CustomerID);
                    string actionLink = Url.Link("CustomerDetailsRoute", new { id = customer.CustomerID });
                    return Created(actionLink, customer);
                }
                else
                {
                    _logger.LogWarning("Invalid customer data provided.");
                    return BadRequest(ModelState);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while adding customer.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error occurred while adding customer.");
            }
        }

        [HttpPut("Update/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Customer customer)
        {
            _logger.LogInformation("Updating customer with ID: {Id}", id);

            if (ModelState.IsValid)
            {
                if (id == customer.CustomerID)
                {
                    await _unitOfWork.Customers.UpdateAsync(id, customer);
                    _unitOfWork.Complete();
                    _logger.LogInformation("Customer with ID {Id} updated successfully.", id);
                    return StatusCode(StatusCodes.Status204NoContent);
                }
                _logger.LogWarning("Invalid data provided for customer update. ID mismatch: {Id} vs DTO ID: {CustomerId}", id, customer.CustomerID);
                return BadRequest("Invalid data");
            }
            else
            {
                _logger.LogWarning("Invalid model state for customer update with ID: {Id}.", id);
                return BadRequest(ModelState);
            }
        }


        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> DeleteById(int id)
        {
            _logger.LogInformation("Deleting customer with ID: {Id}", id);

            Customer customer =await _unitOfWork.Customers.GetByIdAsync(id);
            if (customer == null)
            {
                _logger.LogWarning("Customer with ID {Id} not found for deletion.", id);
                return NotFound("Data Not Found");
            }

            try
            {
                await _unitOfWork.Customers.DeleteAsync(customer);
                _unitOfWork.Complete();
                _logger.LogInformation("Customer with ID {Id} deleted successfully.", id);
                return StatusCode(StatusCodes.Status204NoContent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting customer with ID {Id}.", id);
                return BadRequest(ex.Message);
            }
        }
    }
}

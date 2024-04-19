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
	//[Authorize]
	public class CustomerController : ControllerBase
	{

		private readonly IUnitOfWork _unitOfWork;

		public CustomerController(IUnitOfWork unitOfWork)
		{
			this._unitOfWork = unitOfWork;
		}

		[HttpGet("GetAll")]
		[ResponseCache(Duration = 30)]
		public IActionResult GetAll()
		{
			return Ok(_unitOfWork.Customers.GetAll());
		}

		[HttpGet("GetById/{id}", Name = "CustomerDetailsRoute")]
		public IActionResult GetById(int id)
		{
			return Ok(_unitOfWork.Customers.GetById(id));
		}

		[HttpGet("Find")]
		public IActionResult Find(string search )
		{
			Expression<Func<Customer, bool>> criteria = c =>
	          c.Name.Contains(search) ||
	          c.Email.Contains(search) ||
	          c.Address.Contains(search) ||
			  c.Phone.Contains(search);
			return Ok(_unitOfWork.Customers.Find(criteria));
		}

		[HttpPost("Add/")]
		public IActionResult Add([FromBody] Customer customer)
		{
			if (ModelState.IsValid)
			{
				_unitOfWork.Customers.Add(customer);
				_unitOfWork.Complete();
				string actionLink = Url.Link("CustomerDetailsRoute", new { id = customer.CustomerID });
				return Created(actionLink, customer);
			}
			else
				return BadRequest(ModelState);
		}

		[HttpPut("Update/{id}")]
		public IActionResult Update(int id, [FromBody] Customer customer)
		{
			if (ModelState.IsValid)
			{
				if (id == customer.CustomerID)
				{
					_unitOfWork.Customers.Update(id, customer);
					_unitOfWork.Complete();
					return StatusCode(StatusCodes.Status204NoContent);

				}
				return BadRequest("Invalied data");

			}
			else
				return BadRequest(ModelState);
		}

		[HttpDelete("Delete/{id}")]
		public IActionResult DeleteById(int id)
		{
			Customer customer = _unitOfWork.Customers.GetById(id);
			if (customer == null)
			{
				return NotFound("Data Not Found");
			}
			else
			{
				try
				{
					_unitOfWork.Customers.Delete(customer);
					_unitOfWork.Complete();
					return StatusCode(StatusCodes.Status204NoContent);
				}
				catch (Exception ex)
				{
					return BadRequest(ex.Message);
				}
			}
		}
	}
}

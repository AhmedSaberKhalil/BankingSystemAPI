using AutoMapper;
using DataAccessEF.Repository;
using Domain.DTOs.DtoModels;
using Domain.Models;
using Domain.UnitOfWork;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BankingSystemAPI.Controllers
{
    [Route("api/[controller]")]
	[ApiController]
	//[Authorize]
	public class AccountController : ControllerBase
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapper;

		public AccountController(IUnitOfWork unitOfWork, IMapper mapper)
        {
			this._unitOfWork = unitOfWork;
			this._mapper = mapper;
		}
        [HttpGet("GetAll")]
		public IActionResult GetAll()
		{
			return Ok(_unitOfWork.Accounts.GetAll());
		}

		[HttpGet("GetById/{id}", Name = "AccountDetailsRoute")]
		public IActionResult GetById(int id)
		{
			return Ok(_unitOfWork.Accounts.GetById(id));
		}

		[HttpPost("Add/")]
		public IActionResult Add([FromBody] DtoAccount accountDto)
		{
			if (ModelState.IsValid)
			{
				var account = _mapper.Map<Account>(accountDto);
				_unitOfWork.Accounts.Add(account);
				_unitOfWork.Complete();
				string actionLink = Url.Link("AccountDetailsRoute", new { id = account.AccountID });
				return Created(actionLink, account);
			}
			else
				return BadRequest(ModelState);
		}

		[HttpPut("Update/{id}")]
		public IActionResult Update(int id, [FromBody] DtoAccount accounDto)
		{
			if (ModelState.IsValid)
			{
				if (id == accounDto.AccountID)
				{
					var account = _mapper.Map<Account>(accounDto);
					_unitOfWork.Accounts.Update(id, account);
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
			Account account = _unitOfWork.Accounts.GetById(id);
			if (account == null)
			{
				return NotFound("Data Not Found");
			}
			else
			{
				try
				{
					_unitOfWork.Accounts.Delete(account);
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

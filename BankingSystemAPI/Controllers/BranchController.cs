using DataAccessEF.Repository;
using Domain.DTOs.DtoModels;
using Domain.Models;
using Domain.Repository;
using Domain.UnitOfWork;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BankingSystemAPI.Controllers
{
    [Route("api/[controller]")]
	[ApiController]
	//[Authorize(Roles = "Administrator")]
	public class BranchController : ControllerBase
	{
		private readonly IUnitOfWork _unitOfWork;

		public BranchController(IUnitOfWork unitOfWork)
		{
			this._unitOfWork = unitOfWork;
		}

		[HttpGet("GetAll")]
		public IActionResult GetAll()
		{
			return Ok(_unitOfWork.Branchs.GetAll());
		}

		[HttpGet("GetById/{id}", Name = "BranchDetailsRoute")]
		public IActionResult GetById(int id)
		{
			return Ok(_unitOfWork.Branchs.GetById(id));
		}

		[HttpPost("Add/")]
		public IActionResult Add([FromBody] DtoBranch branchDto)
		{
			if (ModelState.IsValid)
			{
				Branch branch = MapToBranch(branchDto);
				_unitOfWork.Branchs.Add(branch);
				_unitOfWork.Complete();
				string actionLink = Url.Link("BranchDetailsRoute", new { id = branch.BranchID });
				return Created(actionLink, branch);
			}
			else
				return BadRequest(ModelState);
		}

		[HttpPut("Update/{id}")]
		public IActionResult Update(int id, [FromBody] DtoBranch branchDto)
		{
			if (ModelState.IsValid)
			{
				if (id == branchDto.BranchID)
				{
					Branch branch = MapToBranch(branchDto);

					_unitOfWork.Branchs.Update(id, branch);
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
			Branch branch = _unitOfWork.Branchs.GetById(id);
			if (branch == null)
			{
				return NotFound("Data Not Found");
			}
			else
			{
				try
				{
					_unitOfWork.Branchs.Delete(branch);
					_unitOfWork.Complete();
					return StatusCode(StatusCodes.Status204NoContent);
				}
				catch (Exception ex)
				{
					return BadRequest(ex.Message);
				}
			}
		}
		private Branch MapToBranch(DtoBranch branchDto)
		{
			return new Branch
			{
				Location = branchDto.Location,
				BranchName = branchDto.BranchName,
			};
		}

		[HttpGet("Branch&Employee/")]
		public IActionResult GetBranchtWithEmployee(int branchId)
		{
			Branch branch = _unitOfWork.Branchs.GetBranchNameWithEmployee(branchId);
			var dto = new BranchNameWithListOfEmployeeDto();
			if (branch == null)
			{
				return BadRequest("Invalied data");
			}
			else
			{
				dto.Id = branch.BranchID;
				dto.BranchName = branch.BranchName;
				foreach (var item in branch.Employees)
				{
					dto.Emp.Add(item.Name);
				}
			}
			return Ok(dto);

		}
	}
}

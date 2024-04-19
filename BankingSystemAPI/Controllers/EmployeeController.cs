using AutoMapper;
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
	//[Authorize]
	public class EmployeeController : ControllerBase
	{
		private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public EmployeeController(IUnitOfWork unitOfWork, IMapper mapper)
		{
			this._unitOfWork = unitOfWork;
            this._mapper = mapper;
        }

		[HttpGet("GetAll")]
		[ResponseCache(Duration = 30)]
		public IActionResult GetAll()
		{
			return Ok(_unitOfWork.Employees.GetAll());
		}

		[HttpGet("GetById/{id}", Name = "EmployeeDetailsRoute")]
		public IActionResult GetById(int id)
		{
			return Ok(_unitOfWork.Employees.GetById(id));
		}

		[HttpPost("Add/")]
		public IActionResult Add([FromBody] DtoEmployee employeeDto)
		{
			if (ModelState.IsValid)
			{
			//	Employee employee = MapToAccount(employeeDto);
                var employee = _mapper.Map<Employee>(employeeDto);

                _unitOfWork.Employees.Add(employee);
				_unitOfWork.Complete();
				string actionLink = Url.Link("EmployeeDetailsRoute", new { id = employee.EmployeeID });
				return Created(actionLink, employee);
			}
			else
				return BadRequest(ModelState);
		}

		[HttpPut("Update/{id}")]
		public IActionResult Update(int id, [FromBody] DtoEmployee employeeDto)
		{
			if (ModelState.IsValid)
			{
				if (id == employeeDto.EmployeeID)
				{
					//Employee employee = MapToAccount(employeeDto);
                    var employee = _mapper.Map<Employee>(employeeDto);
                    _unitOfWork.Employees.Update(id, employee);
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
			Employee employee = _unitOfWork.Employees.GetById(id);
			if (employee == null)
			{
				return NotFound("Data Not Found");
			}
			else
			{
				try
				{
					_unitOfWork.Employees.Delete(employee);
					_unitOfWork.Complete();
					return StatusCode(StatusCodes.Status204NoContent);
				}
				catch (Exception ex)
				{
					return BadRequest(ex.Message);
				}
			}
		}
		//private Employee MapToAccount(DtoEmployee employeeDto)
		//{
		//	return new Employee
		//	{
		//		Name = employeeDto.Name,
		//		Position = employeeDto.Position,
		//		BranchID = employeeDto.BranchID,
		//	};
		//}
	}
}

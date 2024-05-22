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
	[Authorize]
	public class EmployeeController : ControllerBase
	{
		private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<EmployeeController> _logger;

        public EmployeeController(IUnitOfWork unitOfWork, IMapper mapper, ILogger<EmployeeController> logger)
		{
			this._unitOfWork = unitOfWork;
            this._mapper = mapper;
            this._logger = logger;
        }

		[HttpGet("GetAll")]
		[Authorize(Policy = "EmployeeOnly")]
        [ResponseCache(Duration = 30)]
        public async Task<IActionResult> GetAll()
        {
            _logger.LogInformation("Fetching all employees.");

            try
            {
                var customers = await _unitOfWork.Employees.GetAllAsync(); 
                return Ok(customers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all employees.");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("GetById/{id}", Name = "EmployeeDetailsRoute")]
        [Authorize(Policy = "EmployeeMaleOnly")]
        [HttpGet("GetById/{id}", Name = "EmployeeDetailsRoute")]
        [Authorize(Policy = "EmployeeMaleOnly")]
        public async Task<IActionResult> GetById(int id)
        {
            _logger.LogInformation("Fetching employee by ID: {Id}", id);
            var employee =await _unitOfWork.Employees.GetByIdAsync(id);
            if (employee == null)
            {
                _logger.LogWarning("Employee with ID {Id} not found.", id);
                return NotFound();
            }
            return Ok(employee);
        }

        [HttpPost("Add")]
        public async Task<IActionResult> AddAsync([FromBody] DtoEmployee employeeDto)
        {
            _logger.LogInformation("Adding new employee.");

            try
            {
                if (ModelState.IsValid)
                {
                    var employee = _mapper.Map<Employee>(employeeDto);
                    await _unitOfWork.Employees.AddAsync(employee);
                     _unitOfWork.Complete();

                    _logger.LogInformation("Employee added successfully. ID: {Id}", employee.EmployeeID);
                    string actionLink = Url.Link("EmployeeDetailsRoute", new { id = employee.EmployeeID });
                    return Created(actionLink, employee);
                }
                else
                {
                    _logger.LogWarning("Invalid employee data provided.");
                    return BadRequest(ModelState);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while adding employee.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error occurred while adding employee.");
            }
        }

        [HttpPut("Update/{id}")]
        public async Task<IActionResult> UpdateAsync(int id, [FromBody] DtoEmployee employeeDto)
        {
            _logger.LogInformation("Updating employee with ID: {Id}", id);

            try
            {
                if (ModelState.IsValid)
                {
                    if (id == employeeDto.EmployeeID)
                    {
                        var employee = _mapper.Map<Employee>(employeeDto);
                        _unitOfWork.Employees.UpdateAsync(id, employee);
                         _unitOfWork.Complete();

                        _logger.LogInformation("Employee with ID {Id} updated successfully.", id);
                        return StatusCode(StatusCodes.Status204NoContent);
                    }
                    _logger.LogWarning("Invalid data provided for employee update. ID mismatch: {Id} vs DTO ID: {EmployeeId}", id, employeeDto.EmployeeID);
                    return BadRequest("Invalid data");
                }
                else
                {
                    _logger.LogWarning("Invalid model state for employee update with ID: {Id}.", id);
                    return BadRequest(ModelState);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating employee with ID: {Id}.", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error occurred while updating employee.");
            }
        }

        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> DeleteByIdAsync(int id)
        {
            _logger.LogInformation("Deleting employee with ID: {Id}", id);

            try
            {
                var employee = await _unitOfWork.Employees.GetByIdAsync(id);
                if (employee == null)
                {
                    _logger.LogWarning("Employee with ID {Id} not found for deletion.", id);
                    return NotFound("Data Not Found");
                }

                await _unitOfWork.Employees.DeleteAsync(employee);
                _unitOfWork.Complete();

                _logger.LogInformation("Employee with ID {Id} deleted successfully.", id);
                return StatusCode(StatusCodes.Status204NoContent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting employee with ID: {Id}.", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error occurred while deleting employee.");
            }
        }
    }
}

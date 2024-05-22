using AutoMapper;
using DataAccessEF.Repository;
using Domain.DTOs.DtoModels;
using Domain.Models;
using Domain.Repository;
using Domain.UnitOfWork;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace BankingSystemAPI.Controllers
{
    [Route("api/[controller]")]
	[ApiController]
	[Authorize(Roles = "Administrator")]
	public class BranchController : ControllerBase
	{
		private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IMemoryCache _cache;
        private readonly ILogger<BranchController> _logger;
        private const string branchListCacheKey = "BranchList";
        public BranchController(IUnitOfWork unitOfWork, IMapper mapper, IMemoryCache cache, ILogger<BranchController> logger)
		{
			this._unitOfWork = unitOfWork;
            this._mapper = mapper;
            this._cache = cache;
            this._logger = logger;
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            _logger.LogInformation("Trying to fetch the list of branches from cache.");

            if (!_cache.TryGetValue(branchListCacheKey, out IEnumerable<Branch> branches))
            {
                _logger.LogInformation("Branch list not found in cache. Fetching from database.");

                try
                {
                    branches = await _unitOfWork.Branchs.GetAllAsync(); 
                    var cacheEntryOptions = new MemoryCacheEntryOptions()
                        .SetSlidingExpiration(TimeSpan.FromSeconds(30))
                        .SetAbsoluteExpiration(TimeSpan.FromSeconds(30))
                        .SetPriority(CacheItemPriority.Normal)
                        .SetSize(1024);

                    _cache.Set(branchListCacheKey, branches, cacheEntryOptions);
                    _logger.LogInformation("Branch list fetched from database and cached.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while fetching the branch list from the database.");
                    return StatusCode(500, "Internal server error");
                }
            }
            else
            {
                _logger.LogInformation("Branch list found in cache.");
            }

            return Ok(branches);
        }

        [HttpGet("GetById/{id}", Name = "BranchDetailsRoute")]
        public async Task<IActionResult> GetById(int id)
        {
            _logger.LogInformation("Trying to fetch the branch with ID {BranchID} from cache.", id);

            if (!_cache.TryGetValue($"{branchListCacheKey}_{id}", out Branch branch))
            {
                _logger.LogInformation("Branch not found in cache. Fetching from database.");

                try
                {
                    branch = await _unitOfWork.Branchs.GetByIdAsync(id); 

                    if (branch == null)
                    {
                        _logger.LogInformation("Branch with ID {BranchID} not found in the database.", id);
                        return NotFound("Branch not found");
                    }

                    var cacheEntryOptions = new MemoryCacheEntryOptions()
                        .SetSlidingExpiration(TimeSpan.FromSeconds(30))
                        .SetAbsoluteExpiration(TimeSpan.FromSeconds(30))
                        .SetPriority(CacheItemPriority.Normal)
                        .SetSize(1024);

                    _cache.Set($"{branchListCacheKey}_{id}", branch, cacheEntryOptions);
                    _logger.LogInformation("Branch fetched from the database and cached.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while fetching the branch details from the database.");
                    return StatusCode(500, "Internal server error");
                }
            }
            else
            {
                _logger.LogInformation("Branch found in cache.");
            }

            return Ok(branch);
        }

        [HttpPost("Add/")]
        public async Task<IActionResult> Add([FromBody] DtoBranch branchDto)
        {
            if (ModelState.IsValid)
            {

                try
                {
                    var branch = _mapper.Map<Branch>(branchDto);
                    await _unitOfWork.Branchs.AddAsync(branch);
                    _unitOfWork.Complete();
                    string actionLink = Url.Link("BranchDetailsRoute", new { id = branch.BranchID });
                    _logger.LogInformation("Branch with ID {BranchID} added successfully.", branch.BranchID);
                    return Created(actionLink, branch);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while adding a new branch.");
                    return StatusCode(500, "Internal server error");
                }
            }
            else
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                _logger.LogInformation("Invalid model state for branch creation: {@Errors}", errors);
                return BadRequest(ModelState);
            }
        }

        [HttpPut("Update/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] DtoBranch branchDto)
        {
            if (ModelState.IsValid)
            {
                if (id == branchDto.BranchID)
                {

                    try
                    {
                        var branch = _mapper.Map<Branch>(branchDto);
                        await _unitOfWork.Branchs.UpdateAsync(id, branch);
                         _unitOfWork.Complete();
                        _logger.LogInformation("Branch with ID {BranchID} updated successfully.", id);
                        return NoContent();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error occurred while updating the branch with ID {BranchID}.", id);
                        return StatusCode(500, "Internal server error");
                    }
                }
                else
                {
                    _logger.LogWarning("Invalid data provided for branch update. ID mismatch: {Id} vs DTO ID: {BranchID}", id, branchDto.BranchID);
                    return BadRequest("Invalid data");
                }
            }
            else
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                _logger.LogInformation("Invalid model state for branch update with ID {BranchID}: {@Errors}", id, errors);
                return BadRequest(ModelState);
            }
        }

        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> DeleteById(int id)
        {
            try
            {
                var branch = await _unitOfWork.Branchs.GetByIdAsync(id); 

                if (branch == null)
                {
                    _logger.LogWarning("Branch with ID {BranchID} not found for deletion.", id);
                    return NotFound("Branch not found");
                }

                await _unitOfWork.Branchs.DeleteAsync(branch);
                _unitOfWork.Complete();
                _logger.LogInformation("Branch with ID {BranchID} deleted successfully.", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting the branch with ID {BranchID}.", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("Branch&Employee/")]
        public async Task<IActionResult> GetBranchWithEmployee(int branchId)
        {
            try
            {
                var branch =  _unitOfWork.Branchs.GetBranchNameWithEmployee(branchId); 
                var dto = new BranchNameWithListOfEmployeeDto();

                if (branch == null)
                {
                    _logger.LogWarning("Branch with ID {BranchID} not found.", branchId);
                    return BadRequest("Invalid data");
                }

                dto.Id = branch.BranchID;
                dto.BranchName = branch.BranchName;
                dto.Emp = branch.Employees.Select(e => e.Name).ToList();

                _logger.LogInformation("Branch with ID {BranchID} and its employees fetched successfully.", branchId);
                return Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching the branch with employees for ID {BranchID}.", branchId);
                return StatusCode(500, "Internal server error");
            }
        }
    }
}

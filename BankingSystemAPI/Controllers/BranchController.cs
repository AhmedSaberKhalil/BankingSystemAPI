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
		public IActionResult GetAll()
		{
            _logger.Log(LogLevel.Information, "Trying to fetch the list of branches from cache.");
            if (_cache.TryGetValue(branchListCacheKey, out IEnumerable<Branch> branches))
            {
                _logger.Log(LogLevel.Information, "Branch list found in cache.");
            }
            else
            {
                _logger.Log(LogLevel.Information, "Branch list not found in cache. Fetching from database.");
                branches = _unitOfWork.Branchs.GetAll();
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                        .SetSlidingExpiration(TimeSpan.FromSeconds(30))
                        .SetAbsoluteExpiration(TimeSpan.FromSeconds(30))
                        .SetPriority(CacheItemPriority.Normal)
                        .SetSize(1024);
                _cache.Set(branchListCacheKey, branches, cacheEntryOptions);
            }
            return Ok(branches);
        }

		[HttpGet("GetById/{id}", Name = "BranchDetailsRoute")]
		public IActionResult GetById(int id)
		{
            _logger.Log(LogLevel.Information, "Trying to fetch the list of accounts from cache.");
            if (_cache.TryGetValue(branchListCacheKey, out Branch branche))
            {
                _logger.Log(LogLevel.Information, "Branch found in cache.");
            }
            else
            {
                _logger.Log(LogLevel.Information, "Branch not found in cache. Fetching from database.");
                branche = _unitOfWork.Branchs.GetById(id);
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                        .SetSlidingExpiration(TimeSpan.FromSeconds(30))
                        .SetAbsoluteExpiration(TimeSpan.FromSeconds(30))
                        .SetPriority(CacheItemPriority.Normal)
                        .SetSize(1024);
                _cache.Set(branchListCacheKey, branche, cacheEntryOptions);
            }
            return Ok(branche);
        }

		[HttpPost("Add/")]
		public IActionResult Add([FromBody] DtoBranch branchDto)
		{
			if (ModelState.IsValid)
			{
			//	Branch branch = MapToBranch(branchDto);
                var branch = _mapper.Map<Branch>(branchDto);

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
				//	Branch branch = MapToBranch(branchDto);
                    var branch = _mapper.Map<Branch>(branchDto);


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
		//private Branch MapToBranch(DtoBranch branchDto)
		//{
		//	return new Branch
		//	{
		//		Location = branchDto.Location,
		//		BranchName = branchDto.BranchName,
		//	};
		//}

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

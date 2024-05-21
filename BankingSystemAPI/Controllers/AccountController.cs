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
using Microsoft.Extensions.Logging;

namespace BankingSystemAPI.Controllers
{
    [Route("api/[controller]")]
	[ApiController]
    [Authorize(Roles = "User")]
    public class AccountController : ControllerBase
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapper;
        private readonly IMemoryCache _cache;
        private readonly ILogger<AccountController> _logger;
        private const string accountListCacheKey = "accountList";
        public AccountController(IUnitOfWork unitOfWork, IMapper mapper, IMemoryCache cache, ILogger<AccountController> logger)
        {
			this._unitOfWork = unitOfWork;
			this._mapper = mapper;
			this._cache = cache;
			this._logger = logger;
		}
        [HttpGet("GetAll")]
		public IActionResult GetAll()
		{

            _logger.Log(LogLevel.Information, "Trying to fetch the list of accounts from cache.");
            if (_cache.TryGetValue(accountListCacheKey, out IEnumerable<Account> accounts))
            {
                _logger.Log(LogLevel.Information, "Account list found in cache.");
            }
            else
            {
                _logger.Log(LogLevel.Information, "Account list not found in cache. Fetching from database.");
                accounts = _unitOfWork.Accounts.GetAll();
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                        .SetSlidingExpiration(TimeSpan.FromSeconds(30))
                        .SetAbsoluteExpiration(TimeSpan.FromSeconds(30))
                        .SetPriority(CacheItemPriority.Normal)
                        .SetSize(1024);
                _cache.Set(accountListCacheKey, accounts, cacheEntryOptions);
            }
            return Ok(accounts);
        }

		[HttpGet("GetById/{id}", Name = "AccountDetailsRoute")]
		public IActionResult GetById(int id)
		{

            _logger.Log(LogLevel.Information, "Trying to fetch the account from cache.");
            if (_cache.TryGetValue(accountListCacheKey, out Account account))
            {
                _logger.Log(LogLevel.Information, "Account list found in cache.");
            }
            else
            {
                _logger.Log(LogLevel.Information, "Account not found in cache. Fetching from database.");
                account = _unitOfWork.Accounts.GetById(id);
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                        .SetSlidingExpiration(TimeSpan.FromSeconds(30))
                        .SetAbsoluteExpiration(TimeSpan.FromSeconds(30))
                        .SetPriority(CacheItemPriority.Normal)
                        .SetSize(1024);
                _cache.Set(accountListCacheKey, account, cacheEntryOptions);
            }
            return Ok(account);
		}

		[HttpPost("Add/")]
		public IActionResult Add([FromBody] DtoAccount accountDto)
		{
			if (ModelState.IsValid)
			{
                Account account = _mapper.Map<Account>(accountDto);
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

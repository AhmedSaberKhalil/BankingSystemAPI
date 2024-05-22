using AutoMapper;
using DataAccessEF.Repository;
using DataAccessEF.UnitOfWork;
using Domain.DTOs.DtoModels;
using Domain.Models;
using Domain.Repository;
using Domain.UnitOfWork;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Serilog;
using System;

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
        public async Task<IActionResult> GetAll()
        {
            _logger.LogInformation("Trying to fetch the list of accounts from cache.");

            if (!_cache.TryGetValue(accountListCacheKey, out IEnumerable<Account> accounts))
            {
                _logger.LogInformation("Account list not found in cache. Fetching from database.");

                try
                {
                    accounts =await  _unitOfWork.Accounts.GetAllAsync(); 
                    var cacheEntryOptions = new MemoryCacheEntryOptions()
                        .SetSlidingExpiration(TimeSpan.FromSeconds(30))
                        .SetAbsoluteExpiration(TimeSpan.FromSeconds(30))
                        .SetPriority(CacheItemPriority.Normal)
                        .SetSize(1024);

                    _cache.Set(accountListCacheKey, accounts, cacheEntryOptions);
                    _logger.LogInformation("Account list fetched from database and cached.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while fetching the account list from the database.");
                    return StatusCode(500, "Internal server error");
                }
            }
            else
            {
                _logger.LogInformation("Account list found in cache.");
            }

            return Ok(accounts);
        }
        [HttpGet("GetById/{id}", Name = "AccountDetailsRoute")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                _logger.LogInformation("Trying to fetch the account from cache.");

                if (!_cache.TryGetValue(accountListCacheKey, out Account account))
                {
                    _logger.LogInformation("Account not found in cache. Fetching from database.");

                    account =  await _unitOfWork.Accounts.GetByIdAsync(id); 

                    if (account == null)
                    {
                        _logger.LogInformation("Account not found in the database.");
                        return NotFound("Account not found");
                    }

                    var cacheEntryOptions = new MemoryCacheEntryOptions()
                        .SetSlidingExpiration(TimeSpan.FromSeconds(30))
                        .SetAbsoluteExpiration(TimeSpan.FromSeconds(30))
                        .SetPriority(CacheItemPriority.Normal)
                        .SetSize(1024);

                    _cache.Set(accountListCacheKey, account, cacheEntryOptions);
                    _logger.LogInformation("Account fetched from the database and cached.");
                }
                else
                {
                    _logger.LogInformation("Account found in cache.");
                }

                return Ok(account);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching the account details.");
                return StatusCode(500, "Internal server error");
            }
        }


        [HttpPost("Add/")]
    public async Task<IActionResult> Add([FromBody] DtoAccount accountDto)
    {
        if (ModelState.IsValid)
        {
            try
            {
                Account account = _mapper.Map<Account>(accountDto);
                await _unitOfWork.Accounts.AddAsync(account);
                _unitOfWork.Complete();

                string actionLink = Url.Link("AccountDetailsRoute", new { id = account.AccountID });
                _logger.LogInformation("Account with ID {AccountID} created successfully.", account.AccountID);
                return Created(actionLink, account);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating account.");
                return StatusCode(500, "Internal server error");
            }
        }
        else
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            _logger.LogInformation("Invalid model state for account creation: {@Errors}", errors);
            return BadRequest(ModelState);
        }
    }


        [HttpPut("Update/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] DtoAccount accountDto)
        {
            if (ModelState.IsValid)
            {
                if (id == accountDto.AccountID)
                {
                    try
                    {
                        Account account = _mapper.Map<Account>(accountDto);
                        await _unitOfWork.Accounts.UpdateAsync(id, account);
                        _unitOfWork.Complete(); 

                        _logger.LogInformation("Account with ID {AccountID} updated successfully.", id);
                        return NoContent(); // Use No Content (204) for successful updates without a response body
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error occurred while updating account with ID {AccountID}.", id);
                        return StatusCode(500, "Internal server error");
                    }
                }
                else
                {
                    _logger.LogWarning("Invalid data provided for account update. ID mismatch: {Id} vs DTO ID: {AccountID}", id, accountDto.AccountID);
                    return BadRequest("Invalid data");
                }
            }
            else
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                _logger.LogInformation("Invalid model state for account update with ID {AccountID}: {@Errors}", id, errors);
                return BadRequest(ModelState);
            }
        }

        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> DeleteById(int id)
        {
            try
            {
                Account account =await _unitOfWork.Accounts.GetByIdAsync(id);
                if (account == null)
                {
                    _logger.LogWarning("Account with ID {AccountID} not found for deletion.", id);
                    return NotFound("Data Not Found");
                }

                await _unitOfWork.Accounts.DeleteAsync(account);
                _unitOfWork.Complete(); 
                _logger.LogInformation("Account with ID {AccountID} deleted successfully.", id);
                return NoContent(); // Use No Content (204) for successful deletions without a response body
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting account with ID {AccountID}.", id);
                return StatusCode(500, "Internal server error");
            }
        }
    }
}

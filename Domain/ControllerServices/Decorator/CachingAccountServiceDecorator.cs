using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Domain.Models;
using Domain.UnitOfWork;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Domain.ControllerServices.Decorator
{
    public class CachingAccountServiceDecorator : IControllerService<Account>
    {
        private readonly ILogger<Account> _logger;
        private readonly IControllerService<Account> _innerService;
        private readonly IMemoryCache _cache;
        private const string accountListCacheKey = "accountList";
        private static readonly SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);

        public CachingAccountServiceDecorator(ILogger<Account> logger, IControllerService<Account> innerService, IMemoryCache cache)
        {
            this._logger = logger;
            _innerService = innerService;
            _cache = cache;
        }
        public async Task<Result<IEnumerable<Account>>> GetAllAsync()
        {
            _logger.LogInformation("Trying to fetch the list of accounts from cache.");

            if (_cache.TryGetValue(accountListCacheKey, out IEnumerable<Account> cachedAccounts))
            {
                _logger.LogInformation("Account list found in cache.");
                return Result<IEnumerable<Account>>.Success(cachedAccounts);
            }

            _logger.LogInformation("Account list not found in cache. Fetching from database.");
            await semaphore.WaitAsync();
            try
            {
                // Double-check cache after waiting
                if (_cache.TryGetValue(accountListCacheKey, out cachedAccounts))
                {
                    _logger.LogInformation("Account list found in cache after waiting.");
                    return Result<IEnumerable<Account>>.Success(cachedAccounts);
                }

                var result = await _innerService.GetAllAsync();
                if (result.IsSuccess)
                {
                    var cacheOptions = new MemoryCacheEntryOptions
                    {
                        SlidingExpiration = TimeSpan.FromSeconds(30),
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30),
                        Priority = CacheItemPriority.Normal,
                        Size = 1024
                    };

                    _cache.Set(accountListCacheKey, result.Data, cacheOptions);
                    _logger.LogInformation("Account list fetched from database and cached.");
                }
                else
                {
                    _logger.LogError("Failed to fetch accounts from the database: {Error}", result.Error);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while fetching accounts.");
                return Result<IEnumerable<Account>>.Failure("Unexpected error occurred.");
            }
            finally
            {
                semaphore.Release();
            }
        }
        public async Task<Result<Account>> GetByIdAsync(int id)
        {
            _logger.LogInformation("Trying to fetch account with ID {AccountId} from cache.", id);

            if (_cache.TryGetValue(accountListCacheKey, out Account cachedAccount))
            {
                _logger.LogInformation("Account with ID {AccountId} found in cache.", id);
                return Result<Account>.Success(cachedAccount);
            }

            _logger.LogInformation("Account with ID {AccountId} not found in cache. Fetching from database.", id);
            try
            {
                var result = await _innerService.GetByIdAsync(id);

                if (result.IsSuccess && result.Data != null)
                {
                    var cacheOptions = new MemoryCacheEntryOptions
                    {
                        SlidingExpiration = TimeSpan.FromSeconds(30),
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30),
                        Priority = CacheItemPriority.Normal,
                        Size = 1024
                    };

                    _cache.Set(accountListCacheKey, result.Data, cacheOptions);
                    _logger.LogInformation("Account with ID {AccountId} cached.", id);
                }
                else
                {
                    _logger.LogWarning("Account with ID {AccountId} not found in database.", id);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching account with ID {AccountId}.", id);
                return Result<Account>.Failure("Unexpected error occurred.");
            }
        }

        public async Task<Result<Account>> AddAsync(Account entity)
        {
            try
            {

                var result = await _innerService.AddAsync(entity);
                if (result.IsSuccess == false && result.Data == null) {
                    _logger.LogInformation("Can not add this account");

                }
                else
                {
                    _cache.Remove(accountListCacheKey);
                    _logger.LogInformation("Account with ID {AccountID} created successfully.", entity.AccountID);
                }
                return result;


            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating account.");
                return Result<Account>.Failure(ex.Message);
            }
        }
        public async Task<Result<bool>> UpdateAsync(int id, Account entity)
        {
            try
            {
                var result = await _innerService.UpdateAsync(id, entity);
                if(result.IsSuccess == false)
                {
                    _logger.LogWarning("Failed to update account with ID {AccountID}.", id);
                    return Result<bool>.Failure("Failed to update account.");
                }
                _logger.LogInformation("Account with ID {AccountID} updated successfully.", id);
                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating account with ID {AccountID}.", id);
                return Result<bool>.Failure(ex.Message);
            }

        }
       
        public async Task<Result<bool>> DeleteAsync(int id)
        {
           var result = await _innerService.DeleteAsync(id);
            if (result.IsSuccess == true)
            {
                _logger.LogInformation("Account with ID {AccountID} deleted successfully.", id);
                return Result<bool>.Success(true);
            }
            return Result<bool>.Failure("Failed to delete account.");

        }

       
    }
}

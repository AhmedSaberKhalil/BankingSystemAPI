using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Domain.DTOs.DtoModels;
using Domain.Models;
using Domain.UnitOfWork;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Domain.ControllerServices
{
    public class AccountControllerService : IControllerService<Account> 
    {
        private readonly IUnitOfWork _unitOfWork;
   

        public AccountControllerService(IUnitOfWork unitOfWork)
        {
            this._unitOfWork = unitOfWork;
        }

        public async Task<Result<IEnumerable<Account>>> GetAllAsync()
        {
            try
            {
                var accounts = await _unitOfWork.Accounts.GetAllAsync();
                if (accounts == null)
                    return Result<IEnumerable<Account>>.Failure("No accounts found.");

                return Result<IEnumerable<Account>>.Success(accounts);


            }
            catch (Exception ex)
            {
                return Result<IEnumerable<Account>>.Failure(ex.Message);
            }
        }

        public async Task<Result<Account>> GetByIdAsync(int id)
        {
            try
            {
                var account = await _unitOfWork.Accounts.GetByIdAsync(id);
                return account == null
                    ? Result<Account>.Failure("Not found")
                    : Result<Account>.Success(account);
            }
            catch (Exception ex)
            {
                return Result<Account>.Failure(ex.Message);
            }
        }
        public async Task<Result<Account>> AddAsync(Account entity)
        {
            try
            {
               var res =  await _unitOfWork.Accounts.AddAsync(entity);
                if (res == null)
                {
                    return Result<Account>.Failure("Failed to add account.");
                }
                _unitOfWork.Complete();
                return Result<Account>.Success(entity);
            }
            catch (Exception ex)
            {
                return Result<Account>.Failure(ex.Message);
            }
        }

        public async Task<Result<bool>> UpdateAsync(int id, Account enttity)
        {
            try
            {
                if (id != enttity.AccountID) return Result<bool>.Failure("ID mismatch");

               // var account = _mapper.Map<Account>(account);
                await _unitOfWork.Accounts.UpdateAsync(id, enttity);
                _unitOfWork.Complete();
                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return Result<bool>.Failure(ex.Message);
            }
        }
        public async Task<Result<bool>> DeleteAsync(int id)
        {
        //    Account entity = await _unitOfWork.Accounts.GetByIdAsync(id);

        //    if (entity == null)
        //    {
        //        throw new KeyNotFoundException($"Account with ID {id} not found.");
        //    }
        //     await _unitOfWork.Accounts.DeleteAsync(entity);
        //    _unitOfWork.Complete();
            try
            {
                var account = await _unitOfWork.Accounts.GetByIdAsync(id);
                if (account == null) return Result<bool>.Failure($"Account with id: {id} Not found");

                await _unitOfWork.Accounts.DeleteAsync(account);
                _unitOfWork.Complete();
                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return Result<bool>.Failure(ex.Message);
            }
        }


      
       
    }
}

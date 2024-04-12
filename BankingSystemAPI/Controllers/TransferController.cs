using DataAccessEF.Repository;
using Domain.DTOs.DtoModels;
using Domain.Models;
using Domain.UnitOfWork;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BankingSystemAPI.Controllers
{
    [Route("api/[controller]")]
	[ApiController]
	//[Authorize]
	public class TransferController : ControllerBase
	{
		private readonly IUnitOfWork _unitOfWork;

		public TransferController(IUnitOfWork unitOfWork)
		{
			this._unitOfWork = unitOfWork;
		}

		[HttpGet("GetAll")]
		public IActionResult GetAll()
		{
			return Ok(_unitOfWork.Transfers.GetAll());
		}

		[HttpGet("GetById/{id}", Name = "TransferDetailsRoute")]
		public IActionResult GetById(int id)
		{
			return Ok(_unitOfWork.Transfers.GetById(id));
		}
		[HttpPost("deposit")]
		public async Task<IActionResult> Deposit([FromBody] DtoTransfer transferDto)
		{
			try
			{
				Transfer transfer = MapToTransfer(transferDto);
				_unitOfWork.Transfers.Add(transfer);
				_unitOfWork.Complete();

				// Update ToAccount balance
				Account toAccount = _unitOfWork.Accounts.GetById(transferDto.AccountID);
				if (toAccount == null)
				{
					return NotFound($"ToAccount with ID {transferDto.AccountID} not found");
				}
				if (toAccount != null)
				{
					toAccount.Balance += transferDto.Amount;
				}

				 _unitOfWork.Complete();

				return Ok($"Deposit successful. Updated balance for ToAccount {transferDto.AccountID}: {toAccount.Balance}");
			}
			catch (Exception ex)
			{
				return StatusCode(500, $"Internal server error: {ex.Message}");
			}
		}

		[HttpPost("deposit/{id}")]
		public async Task<IActionResult> Withdraw(int id, [FromBody] DtoTransfer transferDto)
		{
			try
			{
				Transfer transfer = MapToTransfer(transferDto);
				_unitOfWork.Transfers.Update(id, transfer);
				_unitOfWork.Complete();

				// Update ToAccount balance
				Account toAccount = _unitOfWork.Accounts.GetById(transferDto.AccountID);
				if (toAccount == null)
				{
					return NotFound($"ToAccount with ID {transferDto.AccountID} not found");
				}
				if (toAccount != null)
				{
					toAccount.Balance -= transferDto.Amount;
				}

				_unitOfWork.Complete();

				return Ok($"Withdraw successful. Updated balance for ToAccount {transferDto.AccountID}: {toAccount.Balance}");
			}
			catch (Exception ex)
			{
				return StatusCode(500, $"Internal server error: {ex.Message}");
			}
		}
		[HttpDelete("Delete/{id}")]
		public IActionResult DeleteById(int id)
		{
			Transfer transfer = _unitOfWork.Transfers.GetById(id);
			if (transfer == null)
			{
				return NotFound("Data Not Found");
			}
			else
			{
				try
				{
					_unitOfWork.Transfers.Delete(transfer);
					return StatusCode(StatusCodes.Status204NoContent);
				}
				catch (Exception ex)
				{
					return BadRequest(ex.Message);
				}
			}
		}
		private Transfer MapToTransfer(DtoTransfer transferDto)
		{
			return new Transfer
			{
				Date = DateTime.Now,
				Amount = transferDto.Amount,
				AccountID = transferDto.AccountID,
			};
		}
	}
}

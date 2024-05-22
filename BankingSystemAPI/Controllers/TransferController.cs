using AutoMapper;
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
	[Authorize]
	public class TransferController : ControllerBase
	{
		private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<TransferController> _logger;

        public TransferController(IUnitOfWork unitOfWork, IMapper mapper, ILogger<TransferController> logger)
		{
			this._unitOfWork = unitOfWork;
            this._mapper = mapper;
            this._logger = logger;
        }

		[HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            _logger.LogInformation("Fetching all transfers.");

            try
            {
                var customers = await _unitOfWork.Transfers.GetAllAsync();
                return Ok(customers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all transfers.");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("GetById/{id}", Name = "TransferDetailsRoute")]
        public async Task<IActionResult> GetById(int id)
        {
            _logger.LogInformation("Fetching transfer by ID: {Id}", id);
            var transfer =await  _unitOfWork.Transfers.GetByIdAsync(id);
            if (transfer == null)
            {
                _logger.LogWarning("Transfer with ID {Id} not found.", id);
                return NotFound();
            }
            return Ok(transfer);
        }

        [HttpPost("Deposit")]
        public async Task<IActionResult> DepositAsync([FromBody] DtoTransfer transferDto)
        {
            _logger.LogInformation("Initiating deposit transaction.");

            try
            {
                var transfer = _mapper.Map<Transfer>(transferDto);
                await _unitOfWork.Transfers.AddAsync(transfer);
                _unitOfWork.Complete();

                var toAccount = await _unitOfWork.Accounts.GetByIdAsync(transferDto.AccountID);
                if (toAccount == null)
                {
                    _logger.LogWarning("ToAccount with ID {Id} not found.", transferDto.AccountID);
                    return NotFound($"ToAccount with ID {transferDto.AccountID} not found");
                }

                toAccount.Balance += transferDto.Amount;
                 _unitOfWork.Complete();

                _logger.LogInformation("Deposit successful. Updated balance for ToAccount {Id}: {Balance}", toAccount.AccountID, toAccount.Balance);
                return Ok($"Deposit successful. Updated balance for ToAccount {transferDto.AccountID}: {toAccount.Balance}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during deposit transaction.");
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("Withdraw/{id}")]
        public async Task<IActionResult> WithdrawAsync(int id, [FromBody] DtoTransfer transferDto)
        {
            _logger.LogInformation("Initiating withdrawal transaction.");

            try
            {
                var transfer = _mapper.Map<Transfer>(transferDto);
                _unitOfWork.Transfers.UpdateAsync(id, transfer);
                 _unitOfWork.Complete();

                var toAccount = await _unitOfWork.Accounts.GetByIdAsync(transferDto.AccountID);
                if (toAccount == null)
                {
                    _logger.LogWarning("ToAccount with ID {Id} not found.", transferDto.AccountID);
                    return NotFound($"ToAccount with ID {transferDto.AccountID} not found");
                }

                toAccount.Balance -= transferDto.Amount;
                _unitOfWork.Complete();

                _logger.LogInformation("Withdrawal successful. Updated balance for ToAccount {Id}: {Balance}", toAccount.AccountID, toAccount.Balance);
                return Ok($"Withdrawal successful. Updated balance for ToAccount {transferDto.AccountID}: {toAccount.Balance}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during withdrawal transaction.");
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }
        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> DeleteByIdAsync(int id)
        {
            _logger.LogInformation("Deleting transfer with ID: {Id}", id);

            try
            {
                var transfer = await _unitOfWork.Transfers.GetByIdAsync(id);
                if (transfer == null)
                {
                    _logger.LogWarning("Transfer with ID {Id} not found for deletion.", id);
                    return NotFound("Data Not Found");
                }

                await _unitOfWork.Transfers.DeleteAsync(transfer);
                _unitOfWork.Complete();

                _logger.LogInformation("Transfer with ID {Id} deleted successfully.", id);
                return StatusCode(StatusCodes.Status204NoContent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting transfer with ID: {Id}.", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error occurred while deleting transfer.");
            }
        }
    }
}

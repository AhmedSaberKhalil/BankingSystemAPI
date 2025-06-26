using Domain.ControllerServices;
using Domain.DTOs.DtoModels;
using Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BankingSystemAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NewAccountController : ControllerBase
    {
        private readonly IControllerService<Account> _controllerService;
        public NewAccountController(IControllerService<Account> service)
        {
            this._controllerService = service;
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _controllerService.GetAllAsync();
            return result.IsSuccess ? Ok(result.Data) : StatusCode(500, result.Error);
        }

        [HttpGet("GetById/{id}")]
       // [HttpGet("GetById/{id}", Name = "AccountDetailsRoute")]
        public async Task<IActionResult> GetById(int id)
        {
            if (ModelState.IsValid)
            {
                var result = await _controllerService.GetByIdAsync(id);
                return result.IsSuccess ? Ok(result.Data) : NotFound(result.Error);
            }
            return BadRequest(ModelState);

        }
        [HttpPost("Add/")]
        public async Task<IActionResult> Add([FromBody] DtoAccount accountDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            Account account = new Account
            {
                AccountID = accountDto.AccountID,
                Balance = accountDto.Balance,
                Type = accountDto.Type,
                CustomerID = accountDto.CustomerID
            };
            var result = await _controllerService.AddAsync(account);
            return result.IsSuccess
                ? CreatedAtAction(nameof(GetById), new { id = result.Data.AccountID }, result.Data)
                : StatusCode(500, result.Error);

        }
        [HttpPut("Update/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] DtoAccount accountDto)
        {

            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            Account account = new Account
            {
                AccountID = accountDto.AccountID,
                Balance = accountDto.Balance,
                Type = accountDto.Type,
                CustomerID = accountDto.CustomerID
            };

            var result = await _controllerService.UpdateAsync(id, account);
            return result.IsSuccess ? NoContent() : StatusCode(500, result.Error);
        }
        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> DeleteById(int id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var result = await _controllerService.DeleteAsync(id);
            return result.IsSuccess ? NoContent(): StatusCode(500, result.Error);

        }
    }
}


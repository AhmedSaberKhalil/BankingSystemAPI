
using Xunit;
using BankingSystemAPI.Controllers;
using Domain.ControllerServices;
using Domain.Models;
using FakeItEasy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http.HttpResults;

namespace BankingSystemAPI.Tests
{
    public class NewAccountControllerTest
    {
        [Fact]
        public async void GetAll_IfThereIsDataInDatabase_ReturnsOkResult()
        {
            // Act
            var accountRegistry = A.Fake<IControllerService<Account>>();
            A.CallTo(() => accountRegistry.GetAllAsync())
    .Returns(Task.FromResult(Result<IEnumerable<Account>>.Success(new List<Account>
    {
        new Account { AccountID = 1, Balance = 100, Type = "Savings", CustomerID = 1 }
    })));

            var sut = new NewAccountController(accountRegistry);

            // Arrange
            var result = await sut.GetAll();

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }
        [Fact]
        public async void GetAll_IfThereIsNoDataInDatabase_ReturnsStatusCode500()
        {
            // Act
            var accountRegistry = A.Fake<IControllerService<Account>>();
            A.CallTo(() => accountRegistry.GetAllAsync())
                .Returns(Task.FromResult(Result<IEnumerable<Account>>.Failure("No data found")));
            var sut = new NewAccountController(accountRegistry);
            // Arrange
            var result = await sut.GetAll();
            // Assert
            Assert.IsType<ObjectResult>(result);
            var objectResult = result as ObjectResult;
            Assert.Equal(500, objectResult.StatusCode);
        }
        [Fact]
        public async void GetById_IfTheIdIsexist_ReturnOKResult()
        {
            // Arrange
            int id = 1;
            var account = new Account { AccountID = id, Balance = 100, Type = "Savings", CustomerID = 1 };
            var accountRegistry = A.Fake<IControllerService<Account>>();
            A.CallTo(() => accountRegistry.GetByIdAsync(id))
                .Returns(Task.FromResult(Result<Account>.Success(account)));
            var sut = new NewAccountController(accountRegistry);
            // Act
            var result = await sut.GetById(id);
            // Assert
            Assert.IsType<OkObjectResult>(result);
        }
        [Fact]
        public async void GetById_IfTheIdIsNotExist_RetuenNotFound()
        {
            // Arrange
            int id = 1;
            var account = new Account { AccountID = id, Balance = 100, Type = "Savings", CustomerID = 1 };
            var accountRegistry = A.Fake<IControllerService<Account>>();
            A.CallTo(() => accountRegistry.GetByIdAsync(id))
                .Returns(Task.FromResult(Result<Account>.Failure("Account not found")));
            var sut = new NewAccountController(accountRegistry);
            // Act
            var result = await sut.GetById(id);
            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }
        [Fact]
        public async void Add_IfCreateAccountSuccess_ReturnsCreatedResult()
        {
            // Arrange
            var accountDto = new Domain.DTOs.DtoModels.DtoAccount
            {
                AccountID = 1,
                Balance = 100,
                Type = "Savings",
                CustomerID = 1
            };
            var accountRegistry = A.Fake<IControllerService<Account>>();
            var account = new Account
            {
                AccountID = accountDto.AccountID,
                Balance = accountDto.Balance,
                Type = accountDto.Type,
                CustomerID = accountDto.CustomerID
            };
            A.CallTo(() => accountRegistry.AddAsync(account))
                .Returns(Task.FromResult(Result<Account>.Success(account)));
            var sut = new NewAccountController(accountRegistry);
            // Act
            var result = await sut.Add(accountDto);
            // Assert
            Assert.IsType<ObjectResult>(result);
        }
        [Fact]
        public async void Add_IfCreateAccountNotSuccess_ReturnsStatusCode50()
        {
            // Arrange
            var accountDto = new Domain.DTOs.DtoModels.DtoAccount
            {
                AccountID = 1,
                Balance = 100,
                Type = "Savings",
                CustomerID = 1
            };
            var accountRegistry = A.Fake<IControllerService<Account>>();
            var account = new Account
            {
                AccountID = accountDto.AccountID,
                Balance = accountDto.Balance,
                Type = accountDto.Type,
                CustomerID = accountDto.CustomerID
            };
            A.CallTo(() => accountRegistry.AddAsync(account))
                .Returns(Task.FromResult(Result<Account>.Failure("Failed to create account")));
            var sut = new NewAccountController(accountRegistry);
            // Act
            var result = await sut.Add(accountDto);
            // Assert
            Assert.IsType<ObjectResult>(result);
            var objectResult = result as ObjectResult;
            Assert.Equal(500, objectResult.StatusCode);
        }
        [Fact]
        public async void Update_IfUpdateAccountSuccess_ReturnsOkResult()
        {
            // Arrange
            int id = 1;
            var accountDto = new Domain.DTOs.DtoModels.DtoAccount
            {
                AccountID = id,
                Balance = 200,
                Type = "Checking",
                CustomerID = 1
            };
            var accountRegistry = A.Fake<IControllerService<Account>>();
            var account = new Account
            {
                AccountID = accountDto.AccountID,
                Balance = accountDto.Balance,
                Type = accountDto.Type,
                CustomerID = accountDto.CustomerID
            };
            A.CallTo(() => accountRegistry.UpdateAsync(id, A<Account>._))
                .Returns(Task.FromResult(Result<bool>.Success(true)));
            var sut = new NewAccountController(accountRegistry);
            // Act
            var result = await sut.Update(id, accountDto);
            // Assert
            Assert.IsType<NoContentResult>(result);

        }
        [Fact]
        public async void Update_IfUpdateAccountNotSuccess_ReturnsStatusCode500()
        {
            // Arrange
            int id = 1;
            var accountDto = new Domain.DTOs.DtoModels.DtoAccount
            {
                AccountID = id,
                Balance = 200,
                Type = "Checking",
                CustomerID = 1
            };
            var accountRegistry = A.Fake<IControllerService<Account>>();
            A.CallTo(() => accountRegistry.UpdateAsync(id, A<Account>._))
                .Returns(Task.FromResult(Result<bool>.Failure("Failed to update account")));
            var sut = new NewAccountController(accountRegistry);
            // Act
            var result = await sut.Update(id, accountDto);
            // Assert
            Assert.IsType<ObjectResult>(result);
            var objectResult = result as ObjectResult;
            Assert.Equal(500, objectResult.StatusCode);
        }
    }
}

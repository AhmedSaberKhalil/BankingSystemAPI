using AutoMapper;
using BankingSystemAPI.Controllers;
using Domain.DTOs.DtoModels;
using Domain.Models;
using Domain.UnitOfWork;
using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace BankingSystemAPI.Tests
{
    public class AccountControllerTest
    {
        [Fact]
        public async void GetAll_IfThereIsDataInDatabase_ReturnsOkResult()
        {
            // Arrange
            var accounts = A.Fake<IEnumerable<Account>>();
            var _unitOfWork = A.Fake<IUnitOfWork>();
            var _mapper = A.Fake<IMapper>();
            var _cache = A.Fake<IMemoryCache>();
            var _logger = A.Fake<ILogger<AccountController>>();
            var cachentry = A.Fake<MemoryCacheEntryOptions>();
           
            var cacheKey = "AccountListCacheKey";
            A.CallTo(() => _unitOfWork.Accounts.GetAllAsync())
                .Returns(Task.FromResult(accounts));
           

            var controller = new AccountController(_unitOfWork, _mapper, _cache, _logger);

            // Act
            var resultTask =  controller.GetAll();
            var result = await resultTask;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<OkObjectResult>(result);
        }
        [Fact]
        public async void GetById_IfTheIdIsexist_ReturnOKResult()
        {
            //Arrange
            int id = 1;
            var accounts = A.Fake<IEnumerable<Account>>();
            var account = A.Fake<Account>();
            var _unitOfWork = A.Fake<IUnitOfWork>();
            var _mapper = A.Fake<IMapper>();
            var _cach = A.Fake<IMemoryCache>();
            var _loger = A.Fake<ILogger<AccountController>>();
            A.CallTo(() => _unitOfWork.Accounts.GetByIdAsync(id)).Returns(account);
            var controller = new AccountController(_unitOfWork, _mapper, _cach, _loger);

            //Act
            var resultTask = controller.GetById(id);
            var result = await resultTask;


            //Assert
            Assert.NotNull(result);
            Assert.IsType<OkObjectResult>(result);

        }
        [Fact]
        public async void Add_CreateAccount_ReturnCreatedResultAndNotNull() // Made the test async
        {
            // Arrange
            var _unitOfWork = A.Fake<IUnitOfWork>();
            var _mapper = A.Fake<IMapper>();
            var mappedAccount = A.Fake<Account>();
            var accountDto = A.Fake<DtoAccount>();
            var _cache = A.Fake<IMemoryCache>();
            var _logger = A.Fake<ILogger<AccountController>>();

            A.CallTo(() => _mapper.Map<Account>(accountDto)).Returns(mappedAccount);
            A.CallTo(() => _unitOfWork.Accounts.AddAsync(mappedAccount));//.ReturnsLazily(() => Task.FromResult(mappedAccount));
            A.CallTo(() => _unitOfWork.Complete()); 
            var urlHelper = new Mock<IUrlHelper>();
            urlHelper.Setup(x => x.Link(It.IsAny<string>(), It.IsAny<object>())).Returns("http://localhost");

            var controller = new AccountController(_unitOfWork, _mapper, _cache, _logger);
            controller.Url = urlHelper.Object;

            // Act
            var resultTask = controller.Add(accountDto); // Get the Task
            var result = await resultTask;  // Await the result

            // Assert
            Assert.NotNull(result);
            Assert.IsType<CreatedResult>(result);
        }
        [Fact]
        public async void Update_IfTheIdSendInQueryEqualsTheIdSendInBody_ReturnOKResultAndNotNull()
        {
            //Arrange
            int id = 1;
            var _unitOfWork = A.Fake<IUnitOfWork>();
            var _mapper = A.Fake<IMapper>();
            var mappedAccount = A.Fake<Account>();
            var _cach = A.Fake<IMemoryCache>();
            var _loger = A.Fake<ILogger<AccountController>>();
            var accountDto = new DtoAccount { AccountID = 1, Type = "saving", Balance = 1000, CustomerID = 1 }; //A.Fake<DtoAccount>();
            A.CallTo(() => _mapper.Map<Account>(accountDto)).Returns(mappedAccount);
            A.CallTo(() => _unitOfWork.Accounts.UpdateAsync(id, mappedAccount));//.ReturnsLazily(() => Task.FromResult(mappedAccount));
            A.CallTo(() => _unitOfWork.Complete());

            var controller = new AccountController(_unitOfWork, _mapper, _cach, _loger);

            //Act
            var resultTask = controller.Update(id, accountDto);
            var result = await resultTask;  // Await the result

            //Assert
            Assert.NotNull(result);
            Assert.IsType<NoContentResult>(result);
        }
        [Fact]
        public async void Update_IfTheIdSendInQueryNotEqualsTheIdSendInBody_ReturnBadRequest()
        {
            //Arrange
            int id = 1;
            var _unitOfWork = A.Fake<IUnitOfWork>();
            var _mapper = A.Fake<IMapper>();
            var accountDto = A.Fake<DtoAccount>();
            var _cach = A.Fake<IMemoryCache>();
            var _loger = A.Fake<ILogger<AccountController>>();
            var controller = new AccountController(_unitOfWork, _mapper, _cach, _loger);

            //Act
            var resultTask = controller.Update(id, accountDto);
            var result = await resultTask;

            //Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async void Delete_IfTheAccountEqualsNull_ReturnNotFoundObjectResult()
        {
            //Arrange
            int id = 1;
            var _unitOfWork = A.Fake<IUnitOfWork>();
            var _mapper = A.Fake<IMapper>();
            var mappedAccount = A.Fake<Account>();
            var accountDto = A.Fake<DtoAccount>();
            var _cach = A.Fake<IMemoryCache>();
            var _loger = A.Fake<ILogger<AccountController>>();
            A.CallTo(() => _unitOfWork.Accounts.GetByIdAsync(A<int>.Ignored)).Returns(Task.FromResult<Account>(null));

            var controller = new AccountController(_unitOfWork, _mapper, _cach, _loger);

            //Act
            var resultTask = controller.DeleteById(mappedAccount.AccountID);
            var result = await resultTask;

            //Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async void Delete_IfTheAccountIdIsExist_ReturnStatusCoderesult()
        {
            //Arrange
            int id = 1;
            var _unitOfWork = A.Fake<IUnitOfWork>();
            var _mapper = A.Fake<IMapper>();
            var mappedAccount = A.Fake<Account>();
            var accountDto = A.Fake<DtoAccount>();
            var _cach = A.Fake<IMemoryCache>();
            var _loger = A.Fake<ILogger<AccountController>>();
            A.CallTo(() => _unitOfWork.Accounts.GetByIdAsync(mappedAccount.AccountID));
            A.CallTo(() => _unitOfWork.Accounts.DeleteAsync(mappedAccount));
            A.CallTo(() => _unitOfWork.Complete());

            var controller = new AccountController(_unitOfWork, _mapper, _cach, _loger);

            //Act
            var resultTask = controller.DeleteById(mappedAccount.AccountID);
            var result = await resultTask;

            //Assert
            Assert.IsType<NoContentResult>(result);
        }


    }
}
using AutoMapper;
using BankingSystemAPI.Controllers;
using Domain.DTOs.DtoModels;
using Domain.Models;
using Domain.UnitOfWork;
using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace BankingSystemAPI.Tests
{
    public class AccountControllerTest
    {
        [Fact]
        public void GetAll_IfThereIsDataInDatabse_ReturnOKResult()
        {
            //Arrange
            var accounts = A.Fake<IEnumerable<Account>>();
            var _unitOfWork = A.Fake<IUnitOfWork>();
            var _mapper = A.Fake<IMapper>();

            A.CallTo(() => _unitOfWork.Accounts.GetAll()).Returns(accounts);
            var controller = new AccountController(_unitOfWork, _mapper);

            //Act
            var result = controller.GetAll();

            //Assert
            Assert.NotNull(result);
            Assert.IsType<OkObjectResult>(result);
        
        }
        [Fact]
        public void GetById_IfTheIdIsexist_ReturnOKResult()
        {
            //Arrange
            int id = 1;
            var accounts = A.Fake<IEnumerable<Account>>();
            var account = A.Fake<Account>();
            var _unitOfWork = A.Fake<IUnitOfWork>();
            var _mapper = A.Fake<IMapper>();

            A.CallTo(() => _unitOfWork.Accounts.GetById(id)).Returns(account);
            var controller = new AccountController(_unitOfWork, _mapper);

            //Act
            var result = controller.GetById(id);

            //Assert
            Assert.NotNull(result);
            Assert.IsType<OkObjectResult>(result);

        }
        [Fact]
        public void Add_CreateAcccount_ReturnOKResultAndNotNull()
        {
            //Arrange
            var _unitOfWork = A.Fake<IUnitOfWork>();
            var _mapper = A.Fake<IMapper>();
            var mappedAccount = A.Fake<Account>();
            var accountDto = A.Fake<DtoAccount>();
            A.CallTo(() => _mapper.Map<Account>(accountDto)).Returns(mappedAccount);
            A.CallTo(() => _unitOfWork.Accounts.Add(mappedAccount));
            A.CallTo(() => _unitOfWork.Complete());
            var urlHelper = new Mock<IUrlHelper>();
            urlHelper.Setup(x => x.Link(It.IsAny<string>(), It.IsAny<object>())).Returns("http://localhost");

            var controller = new AccountController(_unitOfWork, _mapper);

            controller.Url = urlHelper.Object;
            //Act
            var result = controller.Add(accountDto);

            //Assert
            Assert.NotNull(result);
            Assert.IsType<CreatedResult>(result);
        }
        [Fact]
        public void Update_IfTheIdSendInQueryEqualsTheIdSendInBody_ReturnOKResultAndNotNull()
        {
            //Arrange
            int id = 1;
            var _unitOfWork = A.Fake<IUnitOfWork>();
            var _mapper = A.Fake<IMapper>();
            var mappedAccount = A.Fake<Account>();
            var accountDto = new DtoAccount { AccountID = 1,Type = "saving", Balance = 1000, CustomerID = 1 }; //A.Fake<DtoAccount>();
            A.CallTo(() => _mapper.Map<Account>(accountDto)).Returns(mappedAccount);
            A.CallTo(() => _unitOfWork.Accounts.Update(id, mappedAccount));
            A.CallTo(() => _unitOfWork.Complete());
           
            var controller = new AccountController(_unitOfWork, _mapper);

            //Act
            var result = controller.Update(id, accountDto);

            //Assert
            Assert.NotNull(result);
            Assert.IsType<StatusCodeResult>(result);
        }
        [Fact]
        public void Update_IfTheIdSendInQueryNotEqualsTheIdSendInBody_ReturnBadRequest()
        {
            //Arrange
            int id = 1;
            var _unitOfWork = A.Fake<IUnitOfWork>();
            var _mapper = A.Fake<IMapper>(); 
            var accountDto = A.Fake<DtoAccount>();
            var controller = new AccountController(_unitOfWork, _mapper);

            //Act
            var result = controller.Update(id, accountDto);

            //Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public void Delete_IfTheAccountEqualsNull_ReturnNotFoundObjectResult()
        {
            //Arrange
            int id = 1;
            var _unitOfWork = A.Fake<IUnitOfWork>();
            var _mapper = A.Fake<IMapper>();
            var mappedAccount = A.Fake<Account>();
            var accountDto = A.Fake<DtoAccount>();
            A.CallTo(() => _unitOfWork.Accounts.GetById(A<int>.Ignored)).Returns(null);

            var controller = new AccountController(_unitOfWork, _mapper);

            //Act
            var result = controller.DeleteById(mappedAccount.AccountID);

            //Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public void Delete_IfTheAccountIdIsExist_ReturnStatusCoderesult()
        {
            //Arrange
            int id = 1;
            var _unitOfWork = A.Fake<IUnitOfWork>();
            var _mapper = A.Fake<IMapper>();
            var mappedAccount = A.Fake<Account>();
            var accountDto = A.Fake<DtoAccount>();
            A.CallTo(() => _unitOfWork.Accounts.GetById(mappedAccount.AccountID));
            A.CallTo(() => _unitOfWork.Accounts.Delete(mappedAccount));
            A.CallTo(() => _unitOfWork.Complete());

            var controller = new AccountController(_unitOfWork, _mapper);

            //Act
            var result = controller.DeleteById(mappedAccount.AccountID);

            //Assert
            Assert.IsType<StatusCodeResult>(result);
        }


    }
}
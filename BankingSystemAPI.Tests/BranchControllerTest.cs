using AutoMapper;
using BankingSystemAPI.Controllers;
using Domain.DTOs.DtoModels;
using Domain.Models;
using Domain.UnitOfWork;
using FakeItEasy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingSystemAPI.Tests
{
    public class BranchControllerTest
    {

        [Fact]
        public async void GetAll_IfThereIsDataInDatabase_ReturnsOkResult()
        {
            // Arrange
            var branches = A.Fake<IEnumerable<Branch>>();
            var _unitOfWork = A.Fake<IUnitOfWork>();
            var _mapper = A.Fake<IMapper>();
            var _cache = A.Fake<IMemoryCache>();
            var _logger = A.Fake<ILogger<BranchController>>();
            var cachentry = A.Fake<MemoryCacheEntryOptions>();

            var cacheKey = "AccountListCacheKey";
            A.CallTo(() => _unitOfWork.Branchs.GetAllAsync())
                .Returns(Task.FromResult(branches));


            var controller = new BranchController(_unitOfWork, _mapper, _cache, _logger);

            // Act
            var resultTask = controller.GetAll();
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
            var branches = A.Fake<IEnumerable<Branch>>();
            var branch = A.Fake<Branch>();
            var _unitOfWork = A.Fake<IUnitOfWork>();
            var _mapper = A.Fake<IMapper>();
            var _cach = A.Fake<IMemoryCache>();
            var _loger = A.Fake<ILogger<BranchController>>();
            A.CallTo(() => _unitOfWork.Branchs.GetByIdAsync(id)).Returns(branch);
            var controller = new BranchController(_unitOfWork, _mapper, _cach, _loger);

            //Act
            var resultTask = controller.GetById(id);
            var result = await resultTask;


            //Assert
            Assert.NotNull(result);
            Assert.IsType<OkObjectResult>(result);

        }
        [Fact]
        public async void Add_CreateBranch_ReturnCreatedResultAndNotNull() // Made the test async
        {
            // Arrange
            var _unitOfWork = A.Fake<IUnitOfWork>();
            var _mapper = A.Fake<IMapper>();
            var mappedBranch = A.Fake<Branch>();
            var branchDto = A.Fake<DtoBranch>();
            var _cache = A.Fake<IMemoryCache>();
            var _logger = A.Fake<ILogger<BranchController>>();

            A.CallTo(() => _mapper.Map<Branch>(branchDto)).Returns(mappedBranch);
            A.CallTo(() => _unitOfWork.Branchs.AddAsync(mappedBranch));//.ReturnsLazily(() => Task.FromResult(mappedAccount));
            A.CallTo(() => _unitOfWork.Complete());
            var urlHelper = new Mock<IUrlHelper>();
            urlHelper.Setup(x => x.Link(It.IsAny<string>(), It.IsAny<object>())).Returns("http://localhost");

            var controller = new BranchController(_unitOfWork, _mapper, _cache, _logger);
            controller.Url = urlHelper.Object;

            // Act
            var resultTask = controller.Add(branchDto); // Get the Task
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
            var mappedBranch = A.Fake<Branch>();
            var _cach = A.Fake<IMemoryCache>();
            var _loger = A.Fake<ILogger<BranchController>>();
            var branchDto = new DtoBranch { BranchID = 1, BranchName = "alexbank2", Location = "alex" }; //A.Fake<DtoAccount>();
            A.CallTo(() => _mapper.Map<Branch>(branchDto)).Returns(mappedBranch);
            A.CallTo(() => _unitOfWork.Branchs.UpdateAsync(id, mappedBranch));//.ReturnsLazily(() => Task.FromResult(mappedAccount));
            A.CallTo(() => _unitOfWork.Complete());

            var controller = new BranchController(_unitOfWork, _mapper, _cach, _loger);

            //Act
            var resultTask = controller.Update(id, branchDto);
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
            var branchDto = A.Fake<DtoBranch>();
            var _cach = A.Fake<IMemoryCache>();
            var _loger = A.Fake<ILogger<BranchController>>();
            var controller = new BranchController(_unitOfWork, _mapper, _cach, _loger);

            //Act
            var resultTask = controller.Update(id, branchDto);
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
            var mappedBranch = A.Fake<Branch>();
            var branchDto = A.Fake<DtoBranch>();
            var _cach = A.Fake<IMemoryCache>();
            var _loger = A.Fake<ILogger<BranchController>>();
            A.CallTo(() => _unitOfWork.Branchs.GetByIdAsync(A<int>.Ignored)).Returns(Task.FromResult<Branch>(null));

            var controller = new BranchController(_unitOfWork, _mapper, _cach, _loger);

            //Act
            var resultTask = controller.DeleteById(mappedBranch.BranchID);
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
            var mappedBranch = A.Fake<Branch>();
            var branchDto = A.Fake<DtoBranch>();
            var _cach = A.Fake<IMemoryCache>();
            var _loger = A.Fake<ILogger<BranchController>>();
            A.CallTo(() => _unitOfWork.Branchs.GetByIdAsync(mappedBranch.BranchID));
            A.CallTo(() => _unitOfWork.Branchs.DeleteAsync(mappedBranch));
            A.CallTo(() => _unitOfWork.Complete());

            var controller = new BranchController(_unitOfWork, _mapper, _cach, _loger);

            //Act
            var resultTask = controller.DeleteById(mappedBranch.BranchID);
            var result = await resultTask;

            //Assert
            Assert.IsType<NoContentResult>(result);
        }

    }
}

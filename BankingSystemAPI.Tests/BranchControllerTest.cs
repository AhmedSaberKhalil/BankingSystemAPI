using AutoMapper;
using BankingSystemAPI.Controllers;
using Domain.DTOs.DtoModels;
using Domain.Models;
using Domain.UnitOfWork;
using FakeItEasy;
using Microsoft.AspNetCore.Mvc;
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
        public void GetAll_IfThereIsDataInDatabse_ReturnOKResult()
        {
            //Arrange
            var branchs = A.Fake<IEnumerable<Branch>>();
            var _unitOfWork = A.Fake<IUnitOfWork>();
            var _mapper = A.Fake<IMapper>();

            A.CallTo(() => _unitOfWork.Branchs.GetAll()).Returns(branchs);
            var controller = new BranchController(_unitOfWork, _mapper);

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
            var branchs = A.Fake<IEnumerable<Branch>>();
            var branch = A.Fake<Branch>();
            var _unitOfWork = A.Fake<IUnitOfWork>();
            var _mapper = A.Fake<IMapper>();

            A.CallTo(() => _unitOfWork.Branchs.GetById(id)).Returns(branch);
            var controller = new BranchController(_unitOfWork, _mapper);

            //Act
            var result = controller.GetById(id);

            //Assert
            Assert.NotNull(result);
            Assert.IsType<OkObjectResult>(result);

        }
        [Fact]
        public void Add_CreateBranch_ReturnOKResultAndNotNull()
        {
            //Arrange
            var _unitOfWork = A.Fake<IUnitOfWork>();
            var _mapper = A.Fake<IMapper>();
            var mappedBranch = A.Fake<Branch>();
            var branchDto = A.Fake<DtoBranch>();
            A.CallTo(() => _mapper.Map<Branch>(branchDto)).Returns(mappedBranch);
            A.CallTo(() => _unitOfWork.Branchs.Add(mappedBranch));
            A.CallTo(() => _unitOfWork.Complete());
            var urlHelper = new Mock<IUrlHelper>();
            urlHelper.Setup(x => x.Link(It.IsAny<string>(), It.IsAny<object>())).Returns("http://localhost");

            var controller = new BranchController(_unitOfWork, _mapper);

            controller.Url = urlHelper.Object;
            //Act
            var result = controller.Add(branchDto);

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
            var mappedBranch = A.Fake<Branch>();
            var branchDto = new DtoBranch { BranchID = 1, BranchName = "cairo Branch", Location = "cairo" }; 
            A.CallTo(() => _mapper.Map<Branch>(branchDto)).Returns(mappedBranch);
            A.CallTo(() => _unitOfWork.Branchs.Update(id, mappedBranch));
            A.CallTo(() => _unitOfWork.Complete());

            var controller = new BranchController(_unitOfWork, _mapper);

            //Act
            var result = controller.Update(id, branchDto);

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
            var branchDto = A.Fake<DtoBranch>();
            var controller = new BranchController(_unitOfWork, _mapper);

            //Act
            var result = controller.Update(id, branchDto);

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
            var mappedBranch = A.Fake<Branch>();
            var branchDto = A.Fake<DtoBranch>();
            A.CallTo(() => _unitOfWork.Branchs.GetById(A<int>.Ignored)).Returns(null);

            var controller = new BranchController(_unitOfWork, _mapper);

            //Act
            var result = controller.DeleteById(mappedBranch.BranchID);

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
            var mappedBranch = A.Fake<Branch>();
            var branchDto = A.Fake<DtoBranch>();
            A.CallTo(() => _unitOfWork.Branchs.GetById(mappedBranch.BranchID));
            A.CallTo(() => _unitOfWork.Branchs.Delete(mappedBranch));
            A.CallTo(() => _unitOfWork.Complete());

            var controller = new BranchController(_unitOfWork, _mapper);

            //Act
            var result = controller.DeleteById(mappedBranch.BranchID);

            //Assert
            Assert.IsType<StatusCodeResult>(result);
        }
    }
}

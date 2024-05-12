using DataAccessEF.Data;
using DataAccessEF.Repository;
using Domain.Models;
using FakeItEasy;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.EntityFrameworkCore;

namespace DataAccessEF.Tests
{
    public class RepositoryTest
    {
        private async Task<ApplicationDbContext> GetDatabaseContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var databaseContext = new ApplicationDbContext(options);
            databaseContext.Database.EnsureCreated();
            if (await databaseContext.Account.CountAsync() <= 0)
            {
                for (int i = 1; i <= 5; i++)
                {
                    databaseContext.Account.Add(
                    new Account()
                    {
                        Balance = 1000,
                        Type ="Saving",
                        CustomerID = 1,
                        Customer = new Customer
                        {
                            Name = "ahmed saber khalil",
                            Address = "cairo",
                            Phone = "000",
                            Email = "ahmed@gmail.com"
                        }
                    });
                    await databaseContext.SaveChangesAsync();
                }
            }
            return databaseContext;
        }
        [Fact]
        public async void GetByIdAccount_ifIdExist_ReturnsAccount()
        {
            //Arrange
            var id = 1;
            var dbContext = await GetDatabaseContext();
            var accountRepository = new Repository<Account>(dbContext);

            //Act
            var result = accountRepository.GetById(id);

            //Assert
            Assert.NotNull(result);
            Assert.IsType<Account>(result);
        }
        [Fact]
        public async void GetByIdAccount_ifIdNotExist_ReturnArgumentNullException()
        {
            //Arrange
            var id = 6;
            var dbContext = await GetDatabaseContext();
            var accountRepository = new Repository<Account>(dbContext);

            //Act &Assert 
            Assert.Throws<ArgumentNullException>(() => accountRepository.GetById(id));
       
        }

        [Fact]
        public async void GetAllAccount_ifIdExist_ReturnsListOfAccounts()
        {
            //Arrange
            var dbContext = await GetDatabaseContext();
            var accountRepository = new Repository<Account>(dbContext);

            //Act
            var result = accountRepository.GetAll();

            //Assert
            Assert.NotNull(result);
            Assert.IsType<List<Account>>(result);
        }

        [Fact]
        public async void AddAccount_ReturnAddedAccount()
        {
            //Arrange
            var account = new Account
            {
                Balance = 1000,
                Type = "Saving",
                CustomerID = 1,
            };

            var dbContext = await GetDatabaseContext();
            var accountRepository = new Repository<Account>(dbContext);

            //Act
            var result = accountRepository.Add(account);

            //Assert
            Assert.NotNull(result);
            Assert.IsType<Account>(result);
        }
        [Fact]
        public async void EditAccount_ReturnUpdatedAccount()
        {
            //Arrange
            int id = 6;
            var account = new Account
            {
                AccountID = id,
                Balance = 2000,
                Type = "Saving",
                CustomerID = 1,
            };

            var dbContext = await GetDatabaseContext();
            var accountRepository = new Repository<Account>(dbContext);

            //Act
            var result = accountRepository.Update(id,account);

            //Assert
            Assert.NotNull(result);
            Assert.IsType<Account>(result);
        }

    }
}
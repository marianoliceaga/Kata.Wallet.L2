using Kata.Wallet.Database;
using Kata.Wallet.Domain;
using Kata.Wallet.Dtos;
using Kata.Wallet.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kata.Wallet.UnitTests
{
    [TestFixture]
    public class WalletServiceTests
    {
        private Mock<DataContext> _mockContext;
        private Mock<DbSet<Domain.Wallet>> _mockWallets;
        private Mock<ILogger<WalletService>> _mockLogger;
        private WalletService _service;
        private List<Domain.Wallet> _walletList;

        [SetUp]
        public void SetUp()
        {
            _walletList = new List<Domain.Wallet>
            {
                new Domain.Wallet { Id = 1, UserDocument = "123", Currency = Currency.USD, Balance = 100 },
                new Domain.Wallet { Id = 2, UserDocument = "456", Currency = Currency.EUR, Balance = 200 }
            };

            var walletQueryable = _walletList.AsQueryable();

            _mockWallets = new Mock<DbSet<Domain.Wallet>>();
            _mockWallets.As<IQueryable<Domain.Wallet>>().Setup(m => m.Provider).Returns(walletQueryable.Provider);
            _mockWallets.As<IQueryable<Domain.Wallet>>().Setup(m => m.Expression).Returns(walletQueryable.Expression);
            _mockWallets.As<IQueryable<Domain.Wallet>>().Setup(m => m.ElementType).Returns(walletQueryable.ElementType);
            _mockWallets.As<IQueryable<Domain.Wallet>>().Setup(m => m.GetEnumerator()).Returns(walletQueryable.GetEnumerator());

            _mockContext = new Mock<DataContext>(Mock.Of<Microsoft.Extensions.Configuration.IConfiguration>());
            _mockContext.Setup(c => c.Wallets).Returns(_mockWallets.Object);
            _mockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

            _mockLogger = new Mock<ILogger<WalletService>>();

            _service = new WalletService(_mockContext.Object, _mockLogger.Object);
        }

        [Test]
        public async Task CreateAsync_ValidWallet_ReturnsNewWallet()
        {
            var walletDto = new WalletDto
            {
                UserDocument = "789",
                Currency = Currency.EUR,
                Balance = 300
            };

            var result = await _service.CreateAsync(walletDto);

            Assert.IsNotNull(result);
            Assert.AreEqual(walletDto.UserDocument, result.UserDocument);
            Assert.AreEqual(walletDto.Currency, result.Currency);
            Assert.AreEqual(walletDto.Balance, result.Balance);
            _mockWallets.Verify(w => w.Add(It.IsAny<Domain.Wallet>()), Times.Once);
            _mockContext.Verify(c => c.SaveChangesAsync(default), Times.Once);
        }

        [Test]
        public void CreateAsync_NullWallet_ThrowsArgumentNullException()
        {
            var ex = Assert.ThrowsAsync<ArgumentNullException>(async () => await _service.CreateAsync(null));
            Assert.That(ex.ParamName, Is.EqualTo("wallet"));
        }

        [Test]
        public async Task GetAllAsync_ReturnsEmptyList()
        {
            var result = await _service.GetAllAsync();

            Assert.IsNotNull(result);
            Assert.IsEmpty(result);
        }

        [Test]
        public void GetByOptionalCurrencyUserDocumentAsync_NullWallet_ThrowsArgumentNullException()
        {
            var ex = Assert.ThrowsAsync<ArgumentNullException>(async () => await _service.GetByOptionalCurrencyUserDocumentAsync(null));
            Assert.That(ex.ParamName, Is.EqualTo("wallet"));
        }
    }
}
using Microsoft.Extensions.Logging;

namespace Kata.Wallet.UnitTests
{
    public class WalletServiceTests
    {
        private readonly ILogger<WalletService> _logger;
        protected DataContext Context { get; private set; }

        public WalletServiceTests() { }

        public WalletServiceTests(ILogger<WalletService> transactionService)
        {
            _logger = transactionService;
            
            var inMemorySettings = new Dictionary<string, string>();
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            var options = new DbContextOptionsBuilder<DataContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            Context = new DataContext(configuration);

            Context.Wallets.AddRange(
                new Domain.Wallet { UserDocument = "user1", Currency = Currency.USD, Balance = 100 },
                new Domain.Wallet { UserDocument = "user1", Currency = Currency.EUR, Balance = 200 },
                new Domain.Wallet { UserDocument = "user2", Currency = Currency.USD, Balance = 300 },
                new Domain.Wallet { UserDocument = "user2", Currency = Currency.USD, Balance = 300 }
            );
            Context.SaveChanges();
        }

        [Test]
        public async Task GetByOptionalCurrencyUserDocumentAsync_ShouldReturnFilteredWallets_ByUserDocumentAndCurrency()
        {
            // Arrange
            var service = new WalletService(Context, _logger);

            var filter = new WalletDto { UserDocument = "user1", Currency = Currency.USD };

            // Act
            var result = await service.GetByOptionalCurrencyUserDocumentAsync(filter);

            // Assert
            Assert.IsNotNull(result);
            Assert.That(result[0].UserDocument, Is.EqualTo("user1"));
            Assert.That(result[0].Currency, Is.EqualTo(Currency.USD));
        }

        [Test]
        public async Task GetByOptionalCurrencyUserDocumentAsync_ShouldReturnAllWallets_WhenNoFilter()
        {
            // Arrange
            Context.Wallets.AddRange(
                new Kata.Wallet.Domain.Wallet { UserDocument = "user1", Currency = Currency.USD, Balance = 100 },
                new Kata.Wallet.Domain.Wallet { UserDocument = "user2", Currency = Currency.EUR, Balance = 200 }
            );
            Context.SaveChanges();
            var service = new WalletService(Context, _logger);

            var filter = new WalletDto();

            // Act
            var result = await service.GetByOptionalCurrencyUserDocumentAsync(filter);

            // Assert
            Assert.That(result.Count, Is.EqualTo(2));
        }
    }
}

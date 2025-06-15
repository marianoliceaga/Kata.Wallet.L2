using Microsoft.Extensions.Logging;

namespace Kata.Wallet.UnitTests
{
    public class TransactionServiceTest
    {
        private readonly ILogger<TransactionService> _logger;
        protected DataContext Context { get; private set; }

        public TransactionServiceTest()
        {
            // Set up a default logger for tests that use the parameterless constructor
            _logger = new LoggerFactory().CreateLogger<TransactionService>();

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

        public TransactionServiceTest(ILogger<TransactionService> transactionService)
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
        public async Task GetByWalletId_ReturnsTransactionsForWallet()
        {
            // Arrange
            var walletId = 1;

            Context.Transactions.AddRange(
                new Transaction
                {
                    WalletIncoming = new Domain.Wallet { Id = walletId },
                    WalletOutgoing = new Domain.Wallet { Id = 2 }
                },
                new Transaction
                {
                    WalletIncoming = new Domain.Wallet { Id = 3 },
                    WalletOutgoing = new Domain.Wallet { Id = walletId }
                },
                new Transaction
                {
                    WalletIncoming = new Domain.Wallet { Id = 4 },
                    WalletOutgoing = new Domain.Wallet { Id = 5 }
                }
            );
            Context.SaveChanges();

            var service = new TransactionService(Context, _logger);

            // Act
            var result = await service.GetByWalletId(walletId);

            // Assert
            Assert.That(result.Count, Is.EqualTo(2));
            foreach (var t in result)
            {
                Assert.True(t.WalletIncoming.Id == walletId || t.WalletOutgoing.Id == walletId);
            }
        }

        [Test]
        public async Task CreateTransfence_SuccessfulTransfer_UpdatesBalancesAndCreatesTransaction()
        {
            // Arrange
            var walletFrom = new Domain.Wallet { Id = 1, Balance = 100m };
            var walletTo = new Domain.Wallet { Id = 2, Balance = 50m };
            Context.Wallets.AddRange(walletFrom, walletTo);
            Context.SaveChanges();

            var service = new TransactionService(Context, _logger);
            decimal amount = 30m;

            // Act
            var transaction = await service.CreateTransference(walletFrom.Id, walletTo.Id, amount);

            // Assert
            Assert.That(transaction, Is.Not.Null);
            Assert.That(transaction.Amount, Is.EqualTo(amount));
            Assert.That(transaction.WalletOutgoing.Id, Is.EqualTo(walletFrom.Id));
            Assert.That(transaction.WalletIncoming.Id, Is.EqualTo(walletTo.Id));
            Assert.That(walletFrom.Balance, Is.EqualTo(70m));
            Assert.That(walletTo.Balance, Is.EqualTo(80m));
            Assert.That(await Context.Transactions.CountAsync(), Is.EqualTo(1));
        }

        [Test]
        public void CreateTransfence_ThrowsIfWalletNotFound()
        {
            // Arrange
            var wallet = new Domain.Wallet { Id = 1, Balance = 100m };
            Context.Wallets.Add(wallet);
            Context.SaveChanges();

            var service = new TransactionService(Context, _logger);

            // Act & Assert
            var ex1 = Assert.ThrowsAsync<ArgumentException>(async () =>
                await service.CreateTransference(1, 999, 10m));
            Assert.That(ex1!.Message, Does.Contain("not found"));

            var ex2 = Assert.ThrowsAsync<ArgumentException>(async () =>
                await service.CreateTransference(999, 1, 10m));
            Assert.That(ex2!.Message, Does.Contain("not found"));
        }

        [Test]
        public void CreateTransfence_ThrowsIfInsufficientFunds()
        {
            // Arrange
            var walletFrom = new Domain.Wallet { Id = 1, Balance = 5m };
            var walletTo = new Domain.Wallet { Id = 2, Balance = 0m };
            Context.Wallets.AddRange(walletFrom, walletTo);
            Context.SaveChanges();

            var service = new TransactionService(Context, _logger);
    
            // Act & Assert
            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await service.CreateTransference(walletFrom.Id, walletTo.Id, 10m));
            Assert.That(ex!.Message, Does.Contain("Insufficient funds"));
        }
    }
}

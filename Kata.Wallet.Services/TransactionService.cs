using Kata.Wallet.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kata.Wallet.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly DataContext _context;
        private readonly ILogger<TransactionService> _logger;

        public TransactionService(DataContext context, ILogger<TransactionService> transactionService)
        {
            _context = context;
            _logger = transactionService;
        }

        public async Task<List<Domain.Transaction>> GetByWalletId(int walletId)
        {
            _logger.LogInformation("GetByWalletId called");
            return await _context.Transactions.Where(t => t.WalletIncoming.Id == walletId || t.WalletOutgoing.Id == walletId).ToListAsync();
        }

        public async Task<Domain.Transaction> CreateTransference(int walletIdFrom, int walletIdTo, decimal amount)
        {
            _logger.LogInformation("CreateTransference called");
            var walletFrom = await _context.Wallets.FindAsync(walletIdFrom);
            var walletTo = await _context.Wallets.FindAsync(walletIdTo);
            if (walletFrom == null || walletTo == null)
            {
                _logger.LogError("One or both wallets not found");
                throw new ArgumentException("One or both wallets not found.");
            }
            if (walletFrom.Balance < amount)
            {
                _logger.LogError("Insufficient funds in the source wallet");
                throw new InvalidOperationException("Insufficient funds in the source wallet.");
            }
            if(walletFrom.Currency != walletTo.Currency)
            {
                _logger.LogError("Currency mismatch between wallets");
                throw new InvalidOperationException("Currency mismatch between wallets.");
            }
            var transaction = new Domain.Transaction
            {
                WalletIncoming = walletTo,
                WalletOutgoing = walletFrom,
                Amount = amount,
                Date = DateTime.UtcNow
            };
            walletFrom.Balance -= amount;
            walletTo.Balance += amount;
            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Transference created");
            return transaction;
        }
    }
}

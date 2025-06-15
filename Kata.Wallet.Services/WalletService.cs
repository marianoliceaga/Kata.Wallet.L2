using Kata.Wallet.Database;
using Kata.Wallet.Domain;
using Kata.Wallet.Dtos;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kata.Wallet.Services
{
    public class WalletService : IWalletService
    {
        private readonly DataContext _context;
        private readonly ILogger<WalletService> _logger;

        public WalletService(DataContext context, ILogger<WalletService> walletService)
        {
            _context = context;
            _logger = walletService;
        }

        public async Task<Domain.Wallet> CreateAsync(WalletDto wallet)
        {
            _logger.LogInformation("CreateAsync called");
            if (wallet == null)
            {
                _logger.LogError("Wallet cannot be null");
                throw new ArgumentNullException(nameof(wallet), "Wallet cannot be null");
            }
            var newWallet = new Domain.Wallet
            {
                UserDocument = wallet.UserDocument,
                Currency = wallet.Currency ?? default,
                Balance = wallet.Balance
            };
            _context.Wallets.Add(newWallet);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Wallet created");
            return newWallet;
        }

        public async Task<List<Domain.Wallet>> GetAllAsync()
        {
            _logger.LogInformation("GetAllAsync called");
            var wallets = _context.Wallets;
            var result = await Task.FromResult(new List<Domain.Wallet>());
            return result;
        }

        public async Task<List<Domain.Wallet>> GetByOptionalCurrencyUserDocumentAsync(WalletDto wallet)
        {
            _logger.LogInformation("GetByOptionalCurrencyUserDocumentAsync called");
            if (wallet == null)
            {
                throw new ArgumentNullException(nameof(wallet), "Wallet cannot be null");
            }
            var query = _context.Wallets.AsQueryable();
            if (!string.IsNullOrEmpty(wallet.UserDocument))
            {
                _logger.LogInformation("GetByOptionalCurrencyUserDocumentAsync filtering by UserDocument");
                query = query.Where(w => w.UserDocument == wallet.UserDocument);
            }
            if (wallet.Currency != null)
            {
                _logger.LogInformation("GetByOptionalCurrencyUserDocumentAsync filtering by Currency");
                query = query.Where(w => w.Currency == wallet.Currency);
            }
            var result = query.ToList();
            return await Task.FromResult(result);
        }
    }
}

using Kata.Wallet.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kata.Wallet.Services
{
    public interface IWalletService
    {
        Task<List<Domain.Wallet>> GetAllAsync();
        Task<List<Domain.Wallet>> GetByOptionalCurrencyUserDocumentAsync(WalletDto wallet);
        Task<Domain.Wallet> CreateAsync(WalletDto wallet);
    }
}

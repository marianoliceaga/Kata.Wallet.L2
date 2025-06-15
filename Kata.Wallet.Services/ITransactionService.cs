using Kata.Wallet.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kata.Wallet.Services
{
    public interface ITransactionService
    {
        Task<List<Domain.Transaction>> GetByWalletId(int walletId);
        Task<Domain.Transaction> CreateTransference(int walletIdFrom, int walletIdTo, decimal amount);
    }
}

using Kata.Wallet.Dtos;
using Kata.Wallet.Services;
using Microsoft.AspNetCore.Mvc;

namespace Kata.Wallet.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TransactionController : ControllerBase
{
    private readonly ITransactionService _transactionService;
    private readonly ILogger<TransactionController> _logger;

    public TransactionController(ITransactionService transactionService, ILogger<TransactionController> logger)
    {
        _transactionService = transactionService;
        _logger = logger;
    }

    [HttpGet("GetByWalletId")]
    public async Task<ActionResult<List<Domain.Transaction>>> GetByWalletId(int walletId)
    {
        _logger.LogInformation("GetByWalletId called with walletId: {WalletId}", walletId);
        var transactionsByWalletId = new List<Domain.Transaction>();
        transactionsByWalletId = await _transactionService.GetByWalletId(walletId);
        return Ok(transactionsByWalletId);
    }

    [HttpPost]
    public async Task<ActionResult<Domain.Transaction>> CreateTransference(int walletIdFrom, int walletIdTo, decimal amount)
    {
        _logger.LogInformation("CreateTransference called with walletIdFrom: {WalletIdFrom}, walletIdTo: {WalletIdTo}, amount: {Amount}", walletIdFrom, walletIdTo, amount);
        try
        {
            var transaction = await _transactionService.CreateTransference(walletIdFrom, walletIdTo, amount);
            return CreatedAtAction(nameof(GetByWalletId), new { walletId = walletIdFrom }, transaction);
        }
        catch (ArgumentException ex)
        {
            _logger.LogError(ex, "ArgumentException in CreateTransference: {Message}", ex.Message);
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "InvalidOperationException in CreateTransference: {Message}", ex.Message);
            return Conflict(ex.Message);
        }
    }
}

using Kata.Wallet.Dtos;
using Kata.Wallet.Services;
using Microsoft.AspNetCore.Mvc;

namespace Kata.Wallet.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WalletController : ControllerBase
{
    private readonly IWalletService _walletService;

    public WalletController(IWalletService walletService)
    {
        _walletService = walletService;
    }

    [HttpGet]
    public async Task<ActionResult<List<Domain.Wallet>>> GetAll()
    {
        var wallets = await _walletService.GetAllAsync();
        return Ok(wallets);
    }

    [HttpPost]
    public async Task<ActionResult> Create([FromBody] WalletDto wallet)
    {
        if (wallet == null)
        {
            return BadRequest("Wallet cannot be null");
        }

        var createdWallet = await _walletService.CreateAsync(wallet);
        return CreatedAtAction(nameof(GetAll), new { id = createdWallet.Id }, createdWallet);
    }

    [HttpGet("GetByOptionalCurrencyUserDocument")]
    public async Task<ActionResult<List<Domain.Wallet>>> GetByOptionalCurrencyUserDocument([FromQuery] WalletDto wallet)
    {
        if (wallet == null)
        {
            return BadRequest("Wallet cannot be null");
        }
        var foundWallet = await _walletService.GetByOptionalCurrencyUserDocumentAsync(wallet);
        if (foundWallet == null)
        {
            return NotFound("Wallet not found");
        }
        return Ok(foundWallet);
    }
}
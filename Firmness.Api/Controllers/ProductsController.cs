using Firmness.Application.Common;
using Firmness.Application.DTOs.Products;
using Firmness.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Firmness.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(IProductService productService, ILogger<ProductsController> logger)
    {
        _productService = productService;
        _logger = logger;
    }

    // ====================== GET ALL ======================

    [HttpGet]
    [Authorize(Roles = "Admin,Customer")]
    [ProducesResponseType(typeof(IEnumerable<ProductDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var result = await _productService.GetAllAsync();
        return MapResult(result);
    }

    // ====================== GET BY ID ======================

    [HttpGet("{id:int}")]
    [Authorize(Roles = "Admin,Customer")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _productService.GetByIdAsync(id);
        return MapResult(result);
    }

    // ====================== SEARCH ======================

    [HttpGet("search")]
    [Authorize(Roles = "Admin,Customer")]
    [ProducesResponseType(typeof(IEnumerable<ProductDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Search([FromQuery] string term)
    {
        var result = await _productService.SearchAsync(term);
        return MapResult(result);
    }

    // ====================== CREATE ======================

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateProductDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _productService.CreateAsync(dto);

        if (!result.IsSuccess)
            return MapFailure(result);

        return CreatedAtAction(nameof(GetById), new { id = result.Data.Id }, result.Data);
    }

    // ====================== UPDATE ======================

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProductDto dto)
    {
        if (id != dto.Id)
            return BadRequest(new { error = "Route ID and body ID do not match." });

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _productService.UpdateAsync(dto);
        return MapResult(result);
    }

    // ====================== DELETE ======================

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _productService.DeleteAsync(id);

        if (!result.IsSuccess)
            return MapFailure(result);

        return NoContent();
    }

    // ======================================================
    // ===================== HELPERS ========================
    // ======================================================

    private IActionResult MapResult<T>(ResultOft<T> result)
    {
        if (result.IsSuccess)
            return Ok(result.Data);

        return MapFailure(result);
    }

    private IActionResult MapFailure<T>(ResultOft<T> result)
    {
        var errorObj = new { error = result.ErrorMessage };

        if (result.ErrorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("NotFound: {Msg}", result.ErrorMessage);
            return NotFound(errorObj);
        }

        if (result.ErrorMessage.Contains("already exists", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Conflict: {Msg}", result.ErrorMessage);
            return Conflict(errorObj);
        }

        _logger.LogWarning("BadRequest: {Msg}", result.ErrorMessage);
        return BadRequest(errorObj);
    }

    private IActionResult MapFailure(Result result)
    {
        var errorObj = new { error = result.ErrorMessage };

        if (result.ErrorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase))
            return NotFound(errorObj);

        return BadRequest(errorObj);
    }
}

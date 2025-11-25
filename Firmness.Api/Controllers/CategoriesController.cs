namespace Firmness.API.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Firmness.Application.Interfaces;
using Firmness.Application.DTOs.Categories;
using Firmness.Application.Common;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;
    private readonly ILogger<CategoriesController> _logger;

    public CategoriesController(
        ICategoryService categoryService,
        ILogger<CategoriesController> logger)
    {
        _categoryService = categoryService;
        _logger = logger;
    }

    // ============================================================
    // GET ALL — Customer & Admin
    // ============================================================
    [HttpGet]
    [Authorize(Roles = "Admin,Customer")]
    [ProducesResponseType(typeof(IEnumerable<CategoryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var result = await _categoryService.GetAllAsync();
        return MapResultToActionResult(result);
    }


    // ============================================================
    // GET BY ID — Customer & Admin
    // ============================================================
    [HttpGet("{id:int}")]
    [Authorize(Roles = "Admin,Customer")]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(int id)
    {
        var result = await _categoryService.GetByIdAsync(id);
        return MapResultToActionResult(result);
    }


    // ============================================================
    // CREATE — Solo Admin
    // ============================================================
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateCategoryDto dto)
    {
        var result = await _categoryService.CreateAsync(dto);

        if (!result.IsSuccess)
            return MapFailure(result);

        return CreatedAtAction(nameof(Get), new { id = result.Data.Id }, result.Data);
    }


    // ============================================================
    // UPDATE — Solo Admin
    // ============================================================
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCategoryDto dto)
    {
        var result = await _categoryService.UpdateAsync(id, dto);

        if (!result.IsSuccess)
            return MapFailure(result);

        return Ok(result.Data);
    }


    // ============================================================
    // DELETE — Solo Admin
    // ============================================================
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _categoryService.DeleteAsync(id);

        // 👇 Aseguramos que se llame al MapFailure(Result)
        if (!result.IsSuccess)
            return MapFailure(result);

        return NoContent();
    }



    // =====================================================================
    // =====================   HELPERS   ===================================
    // =====================================================================

    private IActionResult MapResultToActionResult<T>(ResultOft<T> result)
    {
        if (result.IsSuccess)
            return Ok(result.Data);

        return MapFailure(result);
    }

    // Para ResultOft<T>
    private IActionResult MapFailure<T>(ResultOft<T> result)
    {
        var error = new { error = result.ErrorMessage };

        if (result.ErrorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase))
            return NotFound(error);

        if (result.ErrorMessage.Contains("already exists", StringComparison.OrdinalIgnoreCase))
            return Conflict(error);

        return BadRequest(error);
    }

    // Para Result (NO genérico)
    private IActionResult MapFailure(Result result)
    {
        var message = result.ErrorMessage ?? "An error occurred";

        if (message.Contains("not found", StringComparison.OrdinalIgnoreCase))
            return NotFound(new { error = message });

        return BadRequest(new { error = message });
    }
}

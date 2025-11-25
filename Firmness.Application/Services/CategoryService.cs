namespace Firmness.Application.Services;

using AutoMapper;
using Firmness.Application.Common;
using Firmness.Application.DTOs.Categories;
using Firmness.Application.Interfaces;
using Firmness.Domain.Entities;
using Firmness.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System.Linq;

/// <summary>
/// Provides business logic operations for managing categories.
/// </summary>
public class CategoryService : ICategoryService
{
    private readonly IGenericRepository<Category> _categoryRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<CategoryService> _logger;

    public CategoryService(
        IGenericRepository<Category> categoryRepository,
        IMapper mapper,
        ILogger<CategoryService> logger)
    {
        _categoryRepository = categoryRepository;
        _mapper = mapper;
        _logger = logger;
    }

    // ==========================
    // GET ALL
    // ==========================
    public async Task<ResultOft<IEnumerable<CategoryDto>>> GetAllAsync()
    {
        try
        {
            var categories = await _categoryRepository.GetAllAsync();
            var dtos = _mapper.Map<IEnumerable<CategoryDto>>(categories);
            return ResultOft<IEnumerable<CategoryDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all categories");
            return ResultOft<IEnumerable<CategoryDto>>.Failure("Error loading categories. Please try again.");
        }
    }

    // ==========================
    // GET BY ID
    // ==========================
    public async Task<ResultOft<CategoryDto>> GetByIdAsync(int id)
    {
        try
        {
            var category = await _categoryRepository.GetByIdAsync(id);

            if (category == null)
                return ResultOft<CategoryDto>.Failure("Category not found");

            var dto = _mapper.Map<CategoryDto>(category);
            return ResultOft<CategoryDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving category");
            return ResultOft<CategoryDto>.Failure("Error loading category");
        }
    }

    // ==========================
    // CREATE
    // ==========================
    public async Task<ResultOft<CategoryDto>> CreateAsync(CreateCategoryDto createDto)
    {
        try
        {
            var category = _mapper.Map<Category>(createDto);

            await _categoryRepository.AddAsync(category);
            await _categoryRepository.SaveChangesAsync();

            var dto = _mapper.Map<CategoryDto>(category);

            _logger.LogInformation("Category '{Name}' created with ID {Id}", category.Name, category.Id);

            return ResultOft<CategoryDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating category");
            return ResultOft<CategoryDto>.Failure("Error creating category. Please try again.");
        }
    }

    // ==========================
    // UPDATE
    // ==========================
    public async Task<ResultOft<CategoryDto>> UpdateAsync(int id, UpdateCategoryDto updateDto)
    {
        try
        {
            var category = await _categoryRepository.GetByIdAsync(id);

            if (category == null)
                return ResultOft<CategoryDto>.Failure("Category not found");

            // Map updates
            _mapper.Map(updateDto, category);

            await _categoryRepository.UpdateAsync(category);
            await _categoryRepository.SaveChangesAsync();

            var dto = _mapper.Map<CategoryDto>(category);
            return ResultOft<CategoryDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating category");
            return ResultOft<CategoryDto>.Failure("Error updating category.");
        }
    }

    // ==========================
    // DELETE
    // ==========================
    public async Task<Result> DeleteAsync(int id)
    {
        try
        {
            var category = await _categoryRepository.GetByIdAsync(id);

            if (category == null)
                return Result.Failure("Category not found");

            await _categoryRepository.DeleteAsync(id);
            await _categoryRepository.SaveChangesAsync();

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting category");
            return Result.Failure("Error deleting category.");
        }
    }

    // ==========================
    // SEARCH
    // ==========================
    public async Task<ResultOft<IEnumerable<CategoryDto>>> SearchAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return ResultOft<IEnumerable<CategoryDto>>.Failure("Search term cannot be empty");

        try
        {
            var results = await _categoryRepository.FindAsync(c =>
                c.Name.ToLower().Contains(searchTerm.ToLower()));

            return ResultOft<IEnumerable<CategoryDto>>.Success(
                _mapper.Map<IEnumerable<CategoryDto>>(results)
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Search error");
            return ResultOft<IEnumerable<CategoryDto>>.Failure("Error searching categories");
        }
    }

    // ==========================
    // EXISTS
    // ==========================
    public async Task<bool> ExistsAsync(int id)
    {
        return await _categoryRepository.ExistsAsync(id);
    }
}

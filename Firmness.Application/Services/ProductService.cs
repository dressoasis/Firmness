namespace Firmness.Application.Services;

using AutoMapper;
using Firmness.Application.Common;
using Firmness.Application.DTOs.Products;
using Firmness.Application.Interfaces;
using Firmness.Domain.Entities;
using Firmness.Domain.Interfaces;
using Microsoft.Extensions.Logging;

/// <summary>
/// Provides business logic operations for managing products.
/// </summary>
public class ProductService : IProductService
{
    private readonly IGenericRepository<Product> _productRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<ProductService> _logger;

    public ProductService(
        IGenericRepository<Product> productRepository,
        IMapper mapper,
        ILogger<ProductService> logger)
    {
        _productRepository = productRepository;
        _mapper = mapper;
        _logger = logger;
    }

    // ==========================================================
    // GET ALL
    // ==========================================================
    public async Task<ResultOft<IEnumerable<ProductDto>>> GetAllAsync()
    {
        try
        {
            var products = await _productRepository.GetAllAsync();
            var dtos = _mapper.Map<IEnumerable<ProductDto>>(products);
            return ResultOft<IEnumerable<ProductDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all products");
            return ResultOft<IEnumerable<ProductDto>>.Failure("Error loading products. Please try again.");
        }
    }

    // ==========================================================
    // GET BY ID
    // ==========================================================
    public async Task<ResultOft<ProductDto>> GetByIdAsync(int id)
    {
        try
        {
            if (id <= 0)
                return ResultOft<ProductDto>.Failure("The product ID must be greater than 0");

            var product = await _productRepository.GetByIdAsync(id);

            if (product == null)
                return ResultOft<ProductDto>.Failure($"Product with ID {id} not found");

            var dto = _mapper.Map<ProductDto>(product);
            return ResultOft<ProductDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obtaining the product {ProductId}", id);
            return ResultOft<ProductDto>.Failure("Error loading the product. Please try again.");
        }
    }

    // ==========================================================
    // CREATE
    // ==========================================================
    public async Task<ResultOft<ProductDto>> CreateAsync(CreateProductDto createDto)
    {
        try
        {
            var all = await _productRepository.GetAllAsync();
            if (all.Any(p => p.Code.Equals(createDto.Code, StringComparison.OrdinalIgnoreCase)))
            {
                return ResultOft<ProductDto>.Failure(
                    $"A product with the code '{createDto.Code}' already exists.");
            }

            var product = _mapper.Map<Product>(createDto);
            product.CreatedAt = DateTime.UtcNow;
            product.IsActive = true;

            await _productRepository.AddAsync(product);
            await _productRepository.SaveChangesAsync();

            var dto = _mapper.Map<ProductDto>(product);
            return ResultOft<ProductDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product");
            return ResultOft<ProductDto>.Failure("Error creating product. Please try again.");
        }
    }

    // ==========================================================
    // UPDATE
    // ==========================================================
    public async Task<ResultOft<ProductDto>> UpdateAsync(UpdateProductDto updateDto)
    {
        try
        {
            if (updateDto.Id <= 0)
                return ResultOft<ProductDto>.Failure("The product ID must be greater than 0");

            var product = await _productRepository.GetByIdAsync(updateDto.Id);

            if (product == null)
                return ResultOft<ProductDto>.Failure($"Product with ID {updateDto.Id} not found");

            var all = await _productRepository.GetAllAsync();
            if (all.Any(p =>
                p.Id != updateDto.Id &&
                p.Code.Equals(updateDto.Code, StringComparison.OrdinalIgnoreCase)))
            {
                return ResultOft<ProductDto>.Failure(
                    $"A product with the code '{updateDto.Code}' already exists.");
            }

            _mapper.Map(updateDto, product);

            await _productRepository.UpdateAsync(product);
            await _productRepository.SaveChangesAsync();

            var dto = _mapper.Map<ProductDto>(product);
            return ResultOft<ProductDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Product update failed {ProductId}", updateDto.Id);
            return ResultOft<ProductDto>.Failure("Product update failed. Please try again.");
        }
    }

    // ==========================================================
    // DELETE
    // ==========================================================
    public async Task<Result> DeleteAsync(int id)
    {
        try
        {
            if (id <= 0)
                return Result.Failure("The product ID must be greater than 0");

            var exists = await _productRepository.ExistsAsync(id);
            if (!exists)
                return Result.Failure($"Product with ID {id} not found");

            await _productRepository.DeleteAsync(id);
            await _productRepository.SaveChangesAsync();

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting the product {ProductId}", id);
            return Result.Failure("Error deleting product. Please try again.");
        }
    }

    // ==========================================================
    // SEARCH
    // ==========================================================
    public async Task<ResultOft<IEnumerable<ProductDto>>> SearchAsync(string searchTerm)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return ResultOft<IEnumerable<ProductDto>>.Failure("The search term cannot be empty.");

            if (searchTerm.Length < 2)
                return ResultOft<IEnumerable<ProductDto>>.Failure("The search term must be at least 2 characters long");

            var products = await _productRepository.GetAllAsync();

            var filtered = products.Where(p =>
                p.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                p.Code.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));

            var dtos = _mapper.Map<IEnumerable<ProductDto>>(filtered);

            return ResultOft<IEnumerable<ProductDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching for products");
            return ResultOft<IEnumerable<ProductDto>>.Failure("Error searching for products. Please try again.");
        }
    }

    // ==========================================================
    // EXISTS
    // ==========================================================
    public async Task<bool> ExistsAsync(int id)
    {
        try
        {
            return await _productRepository.ExistsAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking product existence {ProductId}", id);
            return false;
        }
    }
}

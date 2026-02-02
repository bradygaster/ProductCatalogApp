using Microsoft.AspNetCore.Mvc;
using ProductServiceLibrary.Models;
using ProductServiceLibrary.Repositories;

namespace ProductServiceLibrary.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly ProductRepository _repository;

    public ProductsController(ProductRepository repository)
    {
        _repository = repository;
    }

    [HttpGet]
    public ActionResult<List<Product>> GetAllProducts()
    {
        try
        {
            return Ok(_repository.GetAllProducts());
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error retrieving all products: {ex.Message}");
        }
    }

    [HttpGet("{id}")]
    public ActionResult<Product> GetProductById(int id)
    {
        try
        {
            var product = _repository.GetProductById(id);
            if (product == null)
            {
                return NotFound($"Product with ID {id} not found");
            }
            return Ok(product);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error retrieving product: {ex.Message}");
        }
    }

    [HttpGet("category/{category}")]
    public ActionResult<List<Product>> GetProductsByCategory(string category)
    {
        try
        {
            return Ok(_repository.GetProductsByCategory(category));
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error retrieving products by category: {ex.Message}");
        }
    }

    [HttpGet("search")]
    public ActionResult<List<Product>> SearchProducts([FromQuery] string searchTerm)
    {
        try
        {
            return Ok(_repository.SearchProducts(searchTerm));
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error searching products: {ex.Message}");
        }
    }

    [HttpGet("pricerange")]
    public ActionResult<List<Product>> GetProductsByPriceRange([FromQuery] decimal minPrice, [FromQuery] decimal maxPrice)
    {
        try
        {
            return Ok(_repository.GetProductsByPriceRange(minPrice, maxPrice));
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error retrieving products by price range: {ex.Message}");
        }
    }

    [HttpPost]
    public ActionResult<Product> CreateProduct([FromBody] Product product)
    {
        try
        {
            var createdProduct = _repository.CreateProduct(product);
            return CreatedAtAction(nameof(GetProductById), new { id = createdProduct.Id }, createdProduct);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error creating product: {ex.Message}");
        }
    }

    [HttpPut("{id}")]
    public ActionResult UpdateProduct(int id, [FromBody] Product product)
    {
        try
        {
            if (id != product.Id)
            {
                return BadRequest("ID mismatch");
            }

            var result = _repository.UpdateProduct(product);
            if (!result)
            {
                return NotFound($"Product with ID {id} not found");
            }
            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error updating product: {ex.Message}");
        }
    }

    [HttpDelete("{id}")]
    public ActionResult DeleteProduct(int id)
    {
        try
        {
            var result = _repository.DeleteProduct(id);
            if (!result)
            {
                return NotFound($"Product with ID {id} not found");
            }
            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error deleting product: {ex.Message}");
        }
    }
}

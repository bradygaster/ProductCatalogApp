using Microsoft.AspNetCore.Mvc;
using ProductServiceLibrary.Models;
using ProductServiceLibrary.Repositories;

namespace ProductServiceLibrary.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly ProductRepository _repository;

    public CategoriesController(ProductRepository repository)
    {
        _repository = repository;
    }

    [HttpGet]
    public ActionResult<List<Category>> GetCategories()
    {
        try
        {
            return Ok(_repository.GetCategories());
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error retrieving categories: {ex.Message}");
        }
    }
}

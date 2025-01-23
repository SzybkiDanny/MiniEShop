namespace MiniEShop.Products.Controllers;

using MediatR;
using Microsoft.AspNetCore.Mvc;
using MiniEShop.Products.Features;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(IMediator mediator, ILogger<ProductsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<GetProducts.Response>> GetProducts([FromQuery] string? nameLike)
    {
        try
        {
            var query = new GetProducts.Query(nameLike);
            var response = await _mediator.Send(query);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting products with name filter: {NameFilter}", nameLike);
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<GetProductById.Response>> GetProductById(string id)
    {
        try
        {
            var query = new GetProductById.Query(id);
            var response = await _mediator.Send(query);

            if (response.Product == null)
            {
                return NotFound();
            }

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting product with ID: {ProductId}", id);
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }
}

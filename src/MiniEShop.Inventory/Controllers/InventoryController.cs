namespace MiniEShop.Inventory.Controllers;

using MediatR;
using Microsoft.AspNetCore.Mvc;
using MiniEShop.Inventory.Features;

[ApiController]
[Route("api/[controller]")]
public class InventoryController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<InventoryController> _logger;

    public InventoryController(IMediator mediator, ILogger<InventoryController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpGet("{productId}")]
    public async Task<ActionResult<GetInventoryByProductId.Response>> GetInventory(string productId)
    {
        try
        {
            var query = new GetInventoryByProductId.Query(productId);
            var response = await _mediator.Send(query);

            if (response.Inventory == null)
            {
                return NotFound();
            }

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting inventory for product {ProductId}", productId);
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }

    [HttpPost("reserve")]
    public async Task<ActionResult<ReserveInventory.Response>> ReserveInventory([FromBody] ReserveInventory.Command command)
    {
        try
        {
            var response = await _mediator.Send(command);
            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reserving inventory");
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }
}

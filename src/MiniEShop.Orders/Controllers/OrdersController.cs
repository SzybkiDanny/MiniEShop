namespace MiniEShop.Orders.Controllers;

using MediatR;
using Microsoft.AspNetCore.Mvc;
using MiniEShop.Orders.Features;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(IMediator mediator, ILogger<OrdersController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<GetOrders.Response>> GetOrders()
    {
        try
        {
            var userId = HttpContext.Session.GetString("SessionId");
            var query = new GetOrders.Query(userId);
            var response = await _mediator.Send(query);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting orders");
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }

    [HttpPost]
    public async Task<ActionResult<CreateOrder.Response>> CreateOrder([FromBody] CreateOrder.Command command)
    {
        try
        {
            var userId = HttpContext.Session.GetString("SessionId");
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("Session ID is required");
            }

            // Override the user ID with the session ID for security
            var secureCommand = command with { UserId = userId };
            var response = await _mediator.Send(secureCommand);

            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating order");
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }
}

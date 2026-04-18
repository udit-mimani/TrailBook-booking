using TrailBook.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace TrailBook.Api.Controllers;

[ApiController]
[Route("api/payments")]
public class PaymentsController : ControllerBase
{
    private readonly IBookingService _bookingService;
    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(IBookingService bookingService, ILogger<PaymentsController> logger)
    {
        _bookingService = bookingService;
        _logger = logger;
    }

    [HttpPost("webhook")]
    public async Task<IActionResult> PaymentWebhook(PaymentWebhookRequest request)
    {
        try
        {
            await _bookingService.ProcessPaymentWebhookAsync(
                request.BookingId,
                request.Status,
                request.IdempotencyKey);

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing webhook for booking {BookingId}", request.BookingId);
            // Always return 200 to payment provider
            return Ok();
        }
    }
}

public record PaymentWebhookRequest(Guid BookingId, string Status, string IdempotencyKey);
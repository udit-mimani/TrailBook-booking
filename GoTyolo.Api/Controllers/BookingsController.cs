using GoTyolo.Api.Services;
using GoTyolo.Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace GoTyolo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BookingsController : ControllerBase
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IBookingService _bookingService;

    public BookingsController(IBookingRepository bookingRepository, IBookingService bookingService)
    {
        _bookingRepository = bookingRepository;
        _bookingService = bookingService;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetBooking(Guid id)
    {
        var booking = await _bookingRepository.GetByIdAsync(id);
        if (booking == null)
            return NotFound();

        return Ok(booking);
    }

    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> CancelBooking(Guid id)
    {
        try
        {
            var booking = await _bookingService.CancelBookingAsync(id);
            return Ok(new
            {
                bookingId = booking.Id,
                state = booking.State.ToString(),
                refundAmount = booking.RefundAmount,
                cancelledAt = booking.CancelledAt
            });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
    }
}
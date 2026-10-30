using GymManagement.Data;
using GymManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace GymManagement.Services;

public class BookingService : IBookingService
{
    private readonly ApplicationDbContext _context;

    public BookingService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<GymClass>> GetUpcomingClassesAsync()
    {
        return await _context.GymClasses
            .Include(c => c.Trainer)
            .Include(c => c.Bookings)
            .Where(c => c.StartTime >= DateTime.UtcNow)
            .OrderBy(c => c.StartTime)
            .ToListAsync();
    }

    public async Task<(bool Success, string Message)> BookClassAsync(int memberId, int gymClassId)
    {
        var gymClass = await _context.GymClasses
            .Include(c => c.Bookings)
            .FirstOrDefaultAsync(c => c.Id == gymClassId);

        if (gymClass == null)
            return (false, "Class not found.");

        if (gymClass.Bookings.Count >= gymClass.MaxCapacity)
            return (false, "This class is fully booked.");

        var hasActiveSubscription = await _context.Subscriptions
            .AnyAsync(s => s.MemberId == memberId && s.StartDate <= DateTime.UtcNow && s.EndDate >= DateTime.UtcNow);

        if (!hasActiveSubscription)
            return (false, "You need an active subscription to book classes.");

        var alreadyBooked = gymClass.Bookings.Any(b => b.MemberId == memberId);
        if (alreadyBooked)
            return (false, "You are already booked for this class.");

        var booking = new ClassBooking
        {
            GymClassId = gymClassId,
            MemberId = memberId,
            BookedAt = DateTime.UtcNow
        };

        _context.ClassBookings.Add(booking);
        await _context.SaveChangesAsync();

        return (true, "Class successfully booked!");
    }

    public async Task<bool> CancelBookingAsync(int bookingId, int memberId)
    {
        var booking = await _context.ClassBookings
            .FirstOrDefaultAsync(b => b.Id == bookingId && b.MemberId == memberId);

        if (booking == null) return false;

        _context.ClassBookings.Remove(booking);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<ClassBooking>> GetMemberBookingsAsync(int memberId)
    {
        return await _context.ClassBookings
            .Include(b => b.GymClass)
                .ThenInclude(c => c.Trainer)
            .Where(b => b.MemberId == memberId)
            .OrderByDescending(b => b.GymClass.StartTime)
            .ToListAsync();
    }
}
using GymManagement.Models;

namespace GymManagement.Services;

public interface IBookingService
{
    Task<IEnumerable<GymClass>> GetUpcomingClassesAsync();
    Task<(bool Success, string Message)> BookClassAsync(int memberId, int gymClassId);
    Task<bool> CancelBookingAsync(int bookingId, int memberId);
    Task<IEnumerable<ClassBooking>> GetMemberBookingsAsync(int memberId);
}
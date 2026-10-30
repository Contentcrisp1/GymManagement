using GymManagement.Data;
using GymManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace GymManagement.Services;

public class SubscriptionService : ISubscriptionService
{
    private readonly ApplicationDbContext _context;

    public SubscriptionService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> IsSubscriptionActiveAsync(int memberId)
    {
        return await _context.Subscriptions
            .AnyAsync(s => s.MemberId == memberId && s.StartDate <= DateTime.UtcNow && s.EndDate >= DateTime.UtcNow);
    }

    public async Task<Subscription?> GetActiveSubscriptionAsync(int memberId)
    {
        return await _context.Subscriptions
            .Include(s => s.MembershipType)
            .Where(s => s.MemberId == memberId && s.StartDate <= DateTime.UtcNow && s.EndDate >= DateTime.UtcNow)
            .OrderByDescending(s => s.EndDate)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<MembershipType>> GetAvailablePlansAsync()
    {
        return await _context.MembershipTypes.ToListAsync();
    }

    public async Task<bool> SubscribeMemberAsync(int memberId, int membershipTypeId)
    {
        var plan = await _context.MembershipTypes.FindAsync(membershipTypeId);
        if (plan == null) return false;

        var startDate = DateTime.UtcNow;
        var endDate = startDate.AddDays(plan.DurationInDays);

        var subscription = new Subscription
        {
            MemberId = memberId,
            MembershipTypeId = membershipTypeId,
            StartDate = startDate,
            EndDate = endDate
        };

        _context.Subscriptions.Add(subscription);
        await _context.SaveChangesAsync();
        return true;
    }
}
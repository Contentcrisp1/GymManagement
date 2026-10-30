using GymManagement.Models;

namespace GymManagement.Services;

public interface ISubscriptionService
{
    Task<bool> IsSubscriptionActiveAsync(int memberId);
    Task<bool> SubscribeMemberAsync(int memberId, int membershipTypeId);
    Task<IEnumerable<MembershipType>> GetAvailablePlansAsync();
    Task<Subscription?> GetActiveSubscriptionAsync(int memberId);
}
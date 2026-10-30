using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using GymManagement.Models;

namespace GymManagement.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Member> Members { get; set; }
    public DbSet<Trainer> Trainers { get; set; }
    public DbSet<MembershipType> MembershipTypes { get; set; }
    public DbSet<Subscription> Subscriptions { get; set; }
    public DbSet<GymClass> GymClasses { get; set; }
    public DbSet<ClassBooking> ClassBookings { get; set; }
}
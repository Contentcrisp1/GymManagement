using GymManagement.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GymManagement.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();

        var context = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

        var userManager = scope.ServiceProvider
            .GetRequiredService<UserManager<IdentityUser>>();

        var roleManager = scope.ServiceProvider
            .GetRequiredService<RoleManager<IdentityRole>>();

        // =========================================================
        // DATABASE
        // =========================================================

        context.Database.EnsureCreated();

        // =========================================================
        // 1. ROLES
        // =========================================================

        string[] roles =
        [
            "Admin",
            "Trainer",
            "Member"
        ];

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                var result = await roleManager.CreateAsync(
                    new IdentityRole(role));

                if (!result.Succeeded)
                {
                    throw new Exception(
                        $"Could not create role '{role}': " +
                        string.Join(", ",
                            result.Errors.Select(e => e.Description)));
                }
            }
        }

        // =========================================================
        // 2. MEMBERSHIP TYPES
        // =========================================================

        if (!await context.MembershipTypes.AnyAsync())
        {
            context.MembershipTypes.AddRange(
                new MembershipType
                {
                    Name = "Basic Monthly",
                    Description =
                        "Access to gym floor during standard hours.",
                    Price = 29.99m,
                    DurationInDays = 30
                },

                new MembershipType
                {
                    Name = "VIP Unlimited",
                    Description =
                        "Full access 24/7 including all group fitness classes.",
                    Price = 59.99m,
                    DurationInDays = 30
                },

                new MembershipType
                {
                    Name = "Annual Pass",
                    Description =
                        "Full annual membership with a 20% discount.",
                    Price = 499.99m,
                    DurationInDays = 365
                }
            );

            await context.SaveChangesAsync();
        }

// =========================================================
// 3. ADMIN
// =========================================================

        var adminUser = await GetOrCreateUserAsync(
            userManager,
            "admin@gym.com",
            "Admin123!");

        await EnsureRoleAsync(
            userManager,
            adminUser,
            "Admin");

        var adminMember = await context.Members
            .FirstOrDefaultAsync(x =>
                x.IdentityUserId == adminUser.Id);

        if (adminMember == null)
        {
            context.Members.Add(new Member
            {
                FirstName = "Admin",
                LastName = "User",
                DateOfBirth = new DateTime(1990, 1, 1),
                JoinedDate = DateTime.UtcNow,
                IdentityUserId = adminUser.Id
            });

            await context.SaveChangesAsync();
        }

        // =========================================================
        // 4. MEMBER - member@gym.com
        // =========================================================

        var memberUser = await GetOrCreateUserAsync(
            userManager,
            "member@gym.com",
            "Member123!");

        await EnsureRoleAsync(
            userManager,
            memberUser,
            "Member");

        var member = await GetOrCreateMemberAsync(
            context,
            memberUser,
            "Test",
            "Member");

        await EnsureActiveSubscriptionAsync(
            context,
            member);

        // =========================================================
        // 5. MEMBER - info@gym.com
        // =========================================================

        var testUser = await GetOrCreateUserAsync(
            userManager,
            "info@gym.com",
            "Skopje+38970123456");

        await EnsureRoleAsync(
            userManager,
            testUser,
            "Member");

        var testMember = await GetOrCreateMemberAsync(
            context,
            testUser,
            "Test",
            "User");

        await EnsureActiveSubscriptionAsync(
            context,
            testMember);

        // =========================================================
        // 6. TRAINER 1
        // =========================================================

        var trainerUser1 = await GetOrCreateUserAsync(
            userManager,
            "john.doe@gym.com",
            "Trainer123!");

        await EnsureRoleAsync(
            userManager,
            trainerUser1,
            "Trainer");

        var trainer1 = await GetOrCreateTrainerAsync(
            context,
            trainerUser1,
            "John",
            "Doe",
            "HIIT & Bodybuilding");

        // =========================================================
        // 7. TRAINER 2
        // =========================================================

        var trainerUser2 = await GetOrCreateUserAsync(
            userManager,
            "sarah.smith@gym.com",
            "Trainer123!");

        await EnsureRoleAsync(
            userManager,
            trainerUser2,
            "Trainer");

        var trainer2 = await GetOrCreateTrainerAsync(
            context,
            trainerUser2,
            "Sarah",
            "Smith",
            "Yoga & Pilates");

        // =========================================================
        // 8. GYM CLASSES
        // =========================================================

        if (!await context.GymClasses.AnyAsync())
        {
            context.GymClasses.AddRange(
                new GymClass
                {
                    Name = "Morning Power Yoga",

                    StartTime = DateTime.UtcNow
                        .AddDays(1)
                        .Date
                        .AddHours(8),

                    DurationInMinutes = 60,
                    MaxCapacity = 15,
                    TrainerId = trainer2.Id
                },

                new GymClass
                {
                    Name = "HIIT Circuit Express",

                    StartTime = DateTime.UtcNow
                        .AddDays(1)
                        .Date
                        .AddHours(17),

                    DurationInMinutes = 45,
                    MaxCapacity = 20,
                    TrainerId = trainer1.Id
                }
            );

            await context.SaveChangesAsync();
        }

        // =========================================================
        // 9. TEST BOOKINGS
        // =========================================================
        //
        // This is the important part.
        //
        // We create bookings for member@gym.com so that:
        //
        // Admin -> Dashboard -> Manage Bookings
        //
        // actually has something to display.
        // =========================================================

        var yogaClass = await context.GymClasses
            .FirstOrDefaultAsync(x =>
                x.Name == "Morning Power Yoga");

        var hiitClass = await context.GymClasses
            .FirstOrDefaultAsync(x =>
                x.Name == "HIIT Circuit Express");

        if (yogaClass != null)
        {
            var alreadyBooked = await context.ClassBookings
                .AnyAsync(x =>
                    x.GymClassId == yogaClass.Id &&
                    x.MemberId == member.Id);

            if (!alreadyBooked)
            {
                context.ClassBookings.Add(
                    new ClassBooking
                    {
                        GymClassId = yogaClass.Id,
                        MemberId = member.Id,
                        BookedAt = DateTime.UtcNow
                    });
            }
        }

        if (hiitClass != null)
        {
            var alreadyBooked = await context.ClassBookings
                .AnyAsync(x =>
                    x.GymClassId == hiitClass.Id &&
                    x.MemberId == member.Id);

            if (!alreadyBooked)
            {
                context.ClassBookings.Add(
                    new ClassBooking
                    {
                        GymClassId = hiitClass.Id,
                        MemberId = member.Id,
                        BookedAt = DateTime.UtcNow
                    });
            }
        }

        await context.SaveChangesAsync();
    }

    // =============================================================
    // USER HELPERS
    // =============================================================

    private static async Task<IdentityUser> GetOrCreateUserAsync(
        UserManager<IdentityUser> userManager,
        string email,
        string password)
    {
        var user = await userManager.FindByEmailAsync(email);

        if (user != null)
        {
            if (!user.EmailConfirmed)
            {
                user.EmailConfirmed = true;
                await userManager.UpdateAsync(user);
            }

            return user;
        }

        user = new IdentityUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(
            user,
            password);

        if (!result.Succeeded)
        {
            throw new Exception(
                $"Could not create user '{email}': " +
                string.Join(", ",
                    result.Errors.Select(e => e.Description)));
        }

        return user;
    }

    private static async Task EnsureRoleAsync(
        UserManager<IdentityUser> userManager,
        IdentityUser user,
        string role)
    {
        if (!await userManager.IsInRoleAsync(user, role))
        {
            var result = await userManager.AddToRoleAsync(
                user,
                role);

            if (!result.Succeeded)
            {
                throw new Exception(
                    $"Could not assign role '{role}' to '{user.Email}': " +
                    string.Join(", ",
                        result.Errors.Select(e => e.Description)));
            }
        }
    }

    // =============================================================
    // MEMBER HELPERS
    // =============================================================

    private static async Task<Member> GetOrCreateMemberAsync(
        ApplicationDbContext context,
        IdentityUser user,
        string firstName,
        string lastName)
    {
        var member = await context.Members
            .FirstOrDefaultAsync(m =>
                m.IdentityUserId == user.Id);

        if (member != null)
        {
            return member;
        }

        member = new Member
        {
            FirstName = firstName,
            LastName = lastName,
            DateOfBirth = new DateTime(2000, 1, 1),
            JoinedDate = DateTime.UtcNow,
            IdentityUserId = user.Id
        };

        context.Members.Add(member);

        await context.SaveChangesAsync();

        return member;
    }

    // =============================================================
    // SUBSCRIPTION HELPERS
    // =============================================================

    private static async Task EnsureActiveSubscriptionAsync(
        ApplicationDbContext context,
        Member member)
    {
        var activeSubscription = await context.Subscriptions
            .FirstOrDefaultAsync(s =>
                s.MemberId == member.Id &&
                s.EndDate > DateTime.UtcNow);

        if (activeSubscription != null)
        {
            return;
        }

        var membershipType = await context.MembershipTypes
            .FirstOrDefaultAsync(m =>
                m.Name == "Basic Monthly");

        if (membershipType == null)
        {
            throw new Exception(
                "Basic Monthly membership type was not found.");
        }

        context.Subscriptions.Add(
            new Subscription
            {
                MemberId = member.Id,
                MembershipTypeId = membershipType.Id,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(
                    membershipType.DurationInDays)
            });

        await context.SaveChangesAsync();
    }

    // =============================================================
    // TRAINER HELPERS
    // =============================================================

    private static async Task<Trainer> GetOrCreateTrainerAsync(
        ApplicationDbContext context,
        IdentityUser user,
        string firstName,
        string lastName,
        string specialization)
    {
        var trainer = await context.Trainers
            .FirstOrDefaultAsync(t =>
                t.IdentityUserId == user.Id);

        if (trainer != null)
        {
            return trainer;
        }

        trainer = new Trainer
        {
            FirstName = firstName,
            LastName = lastName,
            Specialization = specialization,
            IdentityUserId = user.Id
        };

        context.Trainers.Add(trainer);

        await context.SaveChangesAsync();

        return trainer;
    }
}


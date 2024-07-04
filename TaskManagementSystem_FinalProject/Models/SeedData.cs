using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TaskManagementSystem_FinalProject.Data;
using TaskManagementSystem_FinalProject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaskManagementSystem_FinalProject.Models
{
    public class SeedData
    {
        public async static Task Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new ApplicationDbContext(serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>()))
            {
                var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                var userManager = serviceProvider.GetRequiredService<UserManager<AppUser>>();

                if (!context.Roles.Any())
                {
                    List<string> roles = new List<string> { "ProjectManager", "Developer" };

                    foreach (string role in roles)
                    {
                        await roleManager.CreateAsync(new IdentityRole(role));
                    }
                }

                if (!context.Users.Any(u => u.UserName == "projectmanager@mitt.ca"))
                {
                    var user1 = new AppUser
                    {
                        UserName = "projectmanager@mitt.ca",
                        Email = "projectmanager@mitt.ca",
                        EmailConfirmed = true,
                        DailySalary = 1000,
                        SecurityStamp = Guid.NewGuid().ToString()
                    };

                    await userManager.CreateAsync(user1, "P@ssword1");
                    await userManager.AddToRoleAsync(user1, "ProjectManager");
                }

                if (!context.Users.Any(u => u.UserName == "developer1@mitt.ca"))
                {
                    var user2 = new AppUser
                    {
                        UserName = "developer1@mitt.ca",
                        Email = "developer1@mitt.ca",
                        EmailConfirmed = true,
                        DailySalary = 200,
                        SecurityStamp = Guid.NewGuid().ToString()
                    };

                    await userManager.CreateAsync(user2, "P@ssword1");
                    await userManager.AddToRoleAsync(user2, "Developer");
                }

                if (!context.Users.Any(u => u.UserName == "developer2@mitt.ca"))
                {
                    var user3 = new AppUser
                    {
                        UserName = "developer2@mitt.ca",
                        Email = "developer2@mitt.ca",
                        EmailConfirmed = true,
                        DailySalary = 200,
                        SecurityStamp = Guid.NewGuid().ToString()
                    };

                    await userManager.CreateAsync(user3, "P@ssword1");
                    await userManager.AddToRoleAsync(user3, "Developer");
                }

                if (!context.Project.Any(p => p.Name == "Project1"))
                {
                    var project1 = new Project
                    {
                        Name = "Project1",
                        Budget = 1000000,
                        StartDate = new DateTime(2024, 3, 1, 0, 0, 0, DateTimeKind.Utc), // Ensure DateTimeKind is Utc
                        DeadLine = new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc)   // Ensure DateTimeKind is Utc
                    };

                    context.Project.Add(project1);
                }

                if (!context.Project.Any(p => p.Name == "Project2"))
                {
                    var project2 = new Project
                    {
                        Name = "Project2",
                        Budget = 2000000,
                        StartDate = new DateTime(2024, 2, 15, 0, 0, 0, DateTimeKind.Utc), // Ensure DateTimeKind is Utc
                        DeadLine = new DateTime(2024, 10, 15, 0, 0, 0, DateTimeKind.Utc)  // Ensure DateTimeKind is Utc
                    };

                    context.Project.Add(project2);
                }

                await context.SaveChangesAsync();

                if (!context.AppTask.Any())
                {
                    var project1Id = context.Project.First(p => p.Name == "Project1").Id;
                    var project2Id = context.Project.First(p => p.Name == "Project2").Id;

                    var task1 = new AppTask
                    {
                        Name = "Task1",
                        ProjectId = project1Id,
                        CompletePercentage = 30,
                        AppUserId = context.Users.First(u => u.UserName == "developer1@mitt.ca").Id
                    };

                    var task2 = new AppTask
                    {
                        Name = "Task2",
                        ProjectId = project1Id,
                        CompletePercentage = 70,
                        AppUserId = context.Users.First(u => u.UserName == "developer1@mitt.ca").Id
                    };

                    var task3 = new AppTask
                    {
                        Name = "Task3",
                        ProjectId = project1Id,
                        CompletePercentage = 100,
                        AppUserId = context.Users.First(u => u.UserName == "developer1@mitt.ca").Id
                    };

                    var task4 = new AppTask
                    {
                        Name = "Task4",
                        ProjectId = project2Id,
                        CompletePercentage = 10,
                        AppUserId = context.Users.First(u => u.UserName == "developer2@mitt.ca").Id
                    };

                    var task5 = new AppTask
                    {
                        Name = "Task5",
                        ProjectId = project2Id,
                        CompletePercentage = 40,
                        AppUserId = context.Users.First(u => u.UserName == "developer2@mitt.ca").Id
                    };

                    var task6 = new AppTask
                    {
                        Name = "Task6",
                        ProjectId = project2Id,
                        CompletePercentage = 100,
                        AppUserId = context.Users.First(u => u.UserName == "developer2@mitt.ca").Id
                    };

                    var task7 = new AppTask
                    {
                        Name = "Task7",
                        ProjectId = project2Id,
                        CompletePercentage = 70,
                        AppUserId = context.Users.First(u => u.UserName == "developer2@mitt.ca").Id
                    };

                    context.AppTask.AddRange(task1, task2, task3, task4, task5, task6, task7);
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}

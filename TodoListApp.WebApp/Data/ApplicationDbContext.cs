using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TodoListApp.WebApp.Models;

namespace TodoListApp.WebApp.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public DbSet<TodoList> TodoLists { get; set; }
    
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
}

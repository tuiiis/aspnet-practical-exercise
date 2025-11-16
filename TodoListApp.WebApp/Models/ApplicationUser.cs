using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace TodoListApp.WebApp.Models
{
    public class ApplicationUser : IdentityUser
    {
        public virtual ICollection<TodoList> TodoLists { get; set; } = new List<TodoList>();
    }
}
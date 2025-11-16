using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace TodoListApp.WebApp.Models
{
    public class TodoList
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string? Title { get; set; }

        // --- Foreign Key for the User ---
        [Required]
        public string? OwnerId { get; set; }
        public virtual ApplicationUser? Owner { get; set; }

        // --- Navigation Property for Tasks --- will be done in EP02
        // public virtual ICollection<TodoTask> Tasks { get; set; } = new List<TodoTask>();
    }
}
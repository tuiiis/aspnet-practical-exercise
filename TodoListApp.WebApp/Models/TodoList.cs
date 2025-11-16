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

        [Required]
        public string? OwnerId { get; set; }
        public virtual ApplicationUser? Owner { get; set; }
    }
}
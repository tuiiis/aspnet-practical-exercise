using System.ComponentModel.DataAnnotations;

namespace TodoListApp.WebApp.Models
{
    public class Comment
    {
        public int Id { get; set; }

        [Required]
        public string? Content { get; set; }
        public DateTime PostedDate { get; set; } = DateTime.UtcNow;

        // --- Foreign Key for Task ---
        public int TodoTaskId { get; set; }
        public virtual TodoTask? TodoTask { get; set; }

        // --- Foreign Key for User ---
        [Required]
        public string? AuthorId { get; set; }
        public virtual ApplicationUser? Author { get; set; }
    }
}
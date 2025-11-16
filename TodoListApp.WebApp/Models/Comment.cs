using System.ComponentModel.DataAnnotations;

namespace TodoListApp.WebApp.Models
{
    public class Comment
    {
        public int Id { get; set; }

        [Required]
        public string? Content { get; set; }
        public DateTime PostedDate { get; set; } = DateTime.UtcNow;

        public int TodoTaskId { get; set; }
        public virtual TodoTask? TodoTask { get; set; }

        [Required]
        public string? AuthorId { get; set; }
        public virtual ApplicationUser? Author { get; set; }
    }
}
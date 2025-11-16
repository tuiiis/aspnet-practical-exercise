using System.ComponentModel.DataAnnotations;

namespace TodoListApp.WebApp.Models
{
    public enum TaskStatus
    {
        Pending,
        InProgress,
        Completed
    }

    public class TodoTask
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string? Title { get; set; }

        public string? Description { get; set; }
        public TaskStatus Status { get; set; } = TaskStatus.Pending;
        public DateTime? DueDate { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // --- Foreign Key for TodoList ---
        public int TodoListId { get; set; }
        public virtual TodoList? TodoList { get; set; }

        // --- Navigation Properties for Tags and Comments ---
        public virtual ICollection<Tag> Tags { get; set; } = new List<Tag>();
        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }
}
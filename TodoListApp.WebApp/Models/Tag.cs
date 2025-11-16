using System.ComponentModel.DataAnnotations;

namespace TodoListApp.WebApp.Models
{
    public class Tag
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string? Name { get; set; }

        public virtual ICollection<TodoTask> Tasks { get; set; } = new List<TodoTask>();
    }
}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoListApp.WebApp.Data;
using TodoListApp.WebApp.Models;

namespace TodoListApp.WebApp.Controllers
{
    [Authorize]
    public class TagsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TagsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Tags
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return NotFound();
            }

            // Get all tags with task counts (only tasks user owns or is assigned to)
            var tags = await _context.Tags
                .Select(tag => new
                {
                    Tag = tag,
                    TaskCount = _context.TodoTasks
                        .Where(task => task.Tags.Any(t => t.Id == tag.Id) &&
                                      (task.TodoList!.OwnerId == userId || task.AssignedUserId == userId))
                        .Count()
                })
                .OrderBy(x => x.Tag.Name)
                .ToListAsync();

            var tagViewModels = tags.Select(x => new TagViewModel
            {
                Id = x.Tag.Id,
                Name = x.Tag.Name,
                TaskCount = x.TaskCount
            }).ToList();

            return View(tagViewModels);
        }

        // GET: Tags/TasksByTag/5
        public async Task<IActionResult> TasksByTag(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return NotFound();
            }

            var tag = await _context.Tags
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tag == null)
            {
                return NotFound();
            }

            var tasks = await _context.TodoTasks
                .Include(t => t.TodoList)
                    .ThenInclude(tl => tl!.Owner)
                .Include(t => t.AssignedUser)
                .Include(t => t.Tags)
                .Where(t => t.Tags.Any(tag => tag.Id == id) &&
                           (t.TodoList!.OwnerId == userId || t.AssignedUserId == userId))
                .OrderBy(t => t.CreatedDate)
                .ToListAsync();

            ViewData["TagName"] = tag.Name;
            ViewData["TagId"] = tag.Id;

            return View(tasks);
        }

        // POST: Tags/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string tagName)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return Json(new { success = false, message = "User not found" });
            }

            if (string.IsNullOrWhiteSpace(tagName))
            {
                return Json(new { success = false, message = "Tag name cannot be empty" });
            }

            var normalizedTagName = tagName.Trim();

            // Check if tag already exists (case-insensitive)
            var existingTag = await _context.Tags
                .FirstOrDefaultAsync(t => t.Name != null && t.Name.ToLower() == normalizedTagName.ToLower());

            if (existingTag != null)
            {
                return Json(new { success = false, message = "Tag already exists" });
            }

            // Create new tag
            var tag = new Tag { Name = normalizedTagName };
            _context.Tags.Add(tag);
            await _context.SaveChangesAsync();

            return Json(new { success = true, tagId = tag.Id, tagName = tag.Name });
        }

        // GET: Tags/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tag = await _context.Tags
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tag == null)
            {
                return NotFound();
            }

            // Get task count for this tag
            var userId = _userManager.GetUserId(User);
            var taskCount = await _context.TodoTasks
                .Where(task => task.Tags.Any(t => t.Id == id) &&
                              (task.TodoList!.OwnerId == userId || task.AssignedUserId == userId))
                .CountAsync();

            ViewData["TaskCount"] = taskCount;

            return View(tag);
        }

        // POST: Tags/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tag = await _context.Tags
                .Include(t => t.Tasks)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tag == null)
            {
                return NotFound();
            }

            // Remove tag associations from all tasks (many-to-many relationship)
            // Entity Framework will handle this automatically when we delete the tag
            _context.Tags.Remove(tag);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }

    public class TagViewModel
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public int TaskCount { get; set; }
    }
}


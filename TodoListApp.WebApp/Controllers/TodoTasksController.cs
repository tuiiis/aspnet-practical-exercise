using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TodoListApp.WebApp.Data;
using TodoListApp.WebApp.Models;

namespace TodoListApp.WebApp.Controllers
{
    [Authorize]
    public class TodoTasksController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TodoTasksController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: TodoTasks/Index/5 (list tasks for a todo list)
        public async Task<IActionResult> Index(int? todoListId)
        {
            if (todoListId == null)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);

            // Verify the todo list belongs to the user
            var todoList = await _context.TodoLists
                .Include(tl => tl.Owner)
                .FirstOrDefaultAsync(tl => tl.Id == todoListId && tl.OwnerId == userId);

            if (todoList == null)
            {
                return NotFound();
            }

            var tasks = await _context.TodoTasks
                .Include(t => t.AssignedUser)
                .Include(t => t.Tags)
                .Where(t => t.TodoListId == todoListId)
                .OrderBy(t => t.CreatedDate)
                .ToListAsync();

            // Count tasks by status
            var taskCounts = await _context.TodoTasks
                .Where(t => t.TodoListId == todoListId)
                .GroupBy(t => t.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            var pendingCount = taskCounts.FirstOrDefault(x => x.Status == Models.TaskStatus.Pending)?.Count ?? 0;
            var inProgressCount = taskCounts.FirstOrDefault(x => x.Status == Models.TaskStatus.InProgress)?.Count ?? 0;
            var completedCount = taskCounts.FirstOrDefault(x => x.Status == Models.TaskStatus.Completed)?.Count ?? 0;

            // Count overdue tasks (not completed and past due date)
            var overdueCount = await _context.TodoTasks
                .Where(t => t.TodoListId == todoListId
                    && t.DueDate.HasValue
                    && t.DueDate.Value < DateTime.UtcNow
                    && t.Status != Models.TaskStatus.Completed)
                .CountAsync();

            ViewData["TodoListId"] = todoListId;
            ViewData["TodoListTitle"] = todoList.Title;
            ViewData["CurrentUserId"] = userId;
            ViewData["PendingCount"] = pendingCount;
            ViewData["InProgressCount"] = inProgressCount;
            ViewData["CompletedCount"] = completedCount;
            ViewData["OverdueCount"] = overdueCount;

            return View(tasks);
        }

        // GET: TodoTasks/Details/5
        public async Task<IActionResult> Details(int? id, string? returnUrl)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);

            var todoTask = await _context.TodoTasks
                .Include(t => t.TodoList)
                    .ThenInclude(tl => tl!.Owner)
                .Include(t => t.AssignedUser)
                .Include(t => t.Tags)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (todoTask == null)
            {
                return NotFound();
            }

            // Verify the task belongs to a todo list owned by the user or is assigned to the user
            if (todoTask.TodoList?.OwnerId != userId && todoTask.AssignedUserId != userId)
            {
                return NotFound();
            }

            ViewData["ReturnUrl"] = returnUrl;

            return View(todoTask);
        }

        // GET: TodoTasks/Create
        public async Task<IActionResult> Create(int? todoListId, string? returnUrl)
        {
            if (todoListId == null)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);

            // Verify the todo list belongs to the user
            var todoList = await _context.TodoLists
                .FirstOrDefaultAsync(tl => tl.Id == todoListId && tl.OwnerId == userId);

            if (todoList == null)
            {
                return NotFound();
            }

            ViewData["TodoListId"] = todoListId;
            ViewData["TodoListTitle"] = todoList.Title;
            ViewData["ReturnUrl"] = returnUrl;

            return View();
        }

        // POST: TodoTasks/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Description,Status,DueDate,TodoListId")] TodoTask todoTask, string? returnUrl)
        {
            var userId = _userManager.GetUserId(User);

            // Verify the todo list belongs to the user
            var todoList = await _context.TodoLists
                .FirstOrDefaultAsync(tl => tl.Id == todoTask.TodoListId && tl.OwnerId == userId);

            if (todoList == null)
            {
                return NotFound();
            }

            ModelState.Remove("TodoList");
            ModelState.Remove("Tags");
            ModelState.Remove("AssignedUser");

            if (ModelState.IsValid)
            {
                todoTask.CreatedDate = DateTime.UtcNow;
                todoTask.AssignedUserId = userId; // Assign task to creator by default
                // Convert local time to UTC for storage
                if (todoTask.DueDate.HasValue)
                {
                    var localTime = DateTime.SpecifyKind(todoTask.DueDate.Value, DateTimeKind.Local);
                    todoTask.DueDate = localTime.ToUniversalTime();
                }
                _context.Add(todoTask);
                await _context.SaveChangesAsync();
                
                if (!string.IsNullOrEmpty(returnUrl))
                {
                    return Redirect(returnUrl);
                }
                
                return RedirectToAction(nameof(Index), new { todoListId = todoTask.TodoListId });
            }

            ViewData["TodoListId"] = todoTask.TodoListId;
            ViewData["TodoListTitle"] = todoList.Title;
            ViewData["ReturnUrl"] = returnUrl;

            return View(todoTask);
        }

        // GET: TodoTasks/Edit/5
        public async Task<IActionResult> Edit(int? id, string? returnUrl)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);

            var todoTask = await _context.TodoTasks
                .Include(t => t.TodoList)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (todoTask == null)
            {
                return NotFound();
            }

            // Verify the task belongs to a todo list owned by the user
            if (todoTask.TodoList?.OwnerId != userId)
            {
                return NotFound();
            }

            ViewData["ReturnUrl"] = returnUrl;

            return View(todoTask);
        }

        // POST: TodoTasks/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,Status,DueDate,TodoListId,CreatedDate")] TodoTask todoTask, string? returnUrl)
        {
            if (id != todoTask.Id)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);

            // Verify the task belongs to a todo list owned by the user or is assigned to the user
            var originalTask = await _context.TodoTasks
                .Include(t => t.TodoList)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (originalTask == null || (originalTask.TodoList?.OwnerId != userId && originalTask.AssignedUserId != userId))
            {
                return NotFound();
            }

            ModelState.Remove("TodoList");
            ModelState.Remove("Tags");

            if (ModelState.IsValid)
            {
                try
                {
                    originalTask.Title = todoTask.Title;
                    originalTask.Description = todoTask.Description;
                    originalTask.Status = todoTask.Status;
                    // Convert local time to UTC for storage
                    if (todoTask.DueDate.HasValue)
                    {
                        var localTime = DateTime.SpecifyKind(todoTask.DueDate.Value, DateTimeKind.Local);
                        originalTask.DueDate = localTime.ToUniversalTime();
                    }
                    else
                    {
                        originalTask.DueDate = null;
                    }
                    // CreatedDate should not be changed

                    _context.Update(originalTask);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.TodoTasks.Any(e => e.Id == todoTask.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                
                if (!string.IsNullOrEmpty(returnUrl))
                {
                    return Redirect(returnUrl);
                }
                
                return RedirectToAction(nameof(Index), new { todoListId = originalTask.TodoListId });
            }

            ViewData["ReturnUrl"] = returnUrl;

            return View(todoTask);
        }

        // GET: TodoTasks/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);

            var todoTask = await _context.TodoTasks
                .Include(t => t.TodoList)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (todoTask == null)
            {
                return NotFound();
            }

            // Verify the task belongs to a todo list owned by the user
            if (todoTask.TodoList?.OwnerId != userId)
            {
                return NotFound();
            }

            return View(todoTask);
        }

        // POST: TodoTasks/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = _userManager.GetUserId(User);

            var todoTask = await _context.TodoTasks
                .Include(t => t.TodoList)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (todoTask == null)
            {
                return NotFound();
            }

            // Verify the task belongs to a todo list owned by the user
            if (todoTask.TodoList?.OwnerId != userId)
            {
                return NotFound();
            }

            var todoListId = todoTask.TodoListId;

            _context.TodoTasks.Remove(todoTask);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { todoListId = todoListId });
        }

        // GET: TodoTasks/AssignedTasks
        public async Task<IActionResult> AssignedTasks(string? statusFilter, string? sortBy)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return NotFound();
            }

            var query = _context.TodoTasks
                .Include(t => t.TodoList)
                .Include(t => t.AssignedUser)
                .Include(t => t.Tags)
                .Where(t => t.AssignedUserId == userId);

            // Default filter: show only active tasks (Pending or InProgress)
            if (string.IsNullOrEmpty(statusFilter) || statusFilter == "Active")
            {
                query = query.Where(t => t.Status == Models.TaskStatus.Pending || t.Status == Models.TaskStatus.InProgress);
            }
            else if (statusFilter != "All")
            {
                if (Enum.TryParse<Models.TaskStatus>(statusFilter, out var status))
                {
                    query = query.Where(t => t.Status == status);
                }
            }

            // Sorting
            if (string.IsNullOrEmpty(sortBy) || sortBy == "DueDate")
            {
                query = query.OrderBy(t => t.DueDate.HasValue ? 0 : 1)
                             .ThenBy(t => t.DueDate ?? DateTime.MaxValue);
            }
            else if (sortBy == "Title")
            {
                query = query.OrderBy(t => t.Title);
            }

            var tasks = await query.ToListAsync();

            ViewData["StatusFilter"] = statusFilter ?? "Active";
            ViewData["SortBy"] = sortBy ?? "DueDate";

            return View(tasks);
        }

        // POST: TodoTasks/UpdateStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, Models.TaskStatus status, string? returnUrl)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return NotFound();
            }

            var task = await _context.TodoTasks
                .Include(t => t.TodoList)
                .FirstOrDefaultAsync(t => t.Id == id && (t.AssignedUserId == userId || t.TodoList!.OwnerId == userId));

            if (task == null)
            {
                return NotFound();
            }

            task.Status = status;
            _context.Update(task);
            await _context.SaveChangesAsync();

            if (!string.IsNullOrEmpty(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction(nameof(AssignedTasks));
        }

        // POST: TodoTasks/ToggleAssignment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleAssignment(int id, string? returnUrl)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return NotFound();
            }

            var task = await _context.TodoTasks
                .Include(t => t.TodoList)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (task == null)
            {
                return NotFound();
            }

            // Verify the task belongs to a todo list owned by the user
            if (task.TodoList?.OwnerId != userId)
            {
                return NotFound();
            }

            // Toggle assignment: if assigned to current user, unassign; otherwise assign
            if (task.AssignedUserId == userId)
            {
                task.AssignedUserId = null;
            }
            else
            {
                task.AssignedUserId = userId;
            }

            _context.Update(task);
            await _context.SaveChangesAsync();

            if (!string.IsNullOrEmpty(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction(nameof(Index), new { todoListId = task.TodoListId });
        }

        // GET: TodoTasks/Search
        public async Task<IActionResult> Search(string? searchTerm, int[]? tagIds)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return NotFound();
            }

            var query = _context.TodoTasks
                .Include(t => t.TodoList)
                    .ThenInclude(tl => tl!.Owner)
                .Include(t => t.AssignedUser)
                .Include(t => t.Tags)
                .Where(t => t.TodoList!.OwnerId == userId || t.AssignedUserId == userId);

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(t => t.Title != null && t.Title.Contains(searchTerm));
            }

            // Filter by tags (AND logic - task must have all selected tags)
            if (tagIds != null && tagIds.Length > 0)
            {
                foreach (var tagId in tagIds)
                {
                    var currentTagId = tagId; // Capture for closure
                    query = query.Where(t => t.Tags.Any(tag => tag.Id == currentTagId));
                }
            }

            var tasks = await query.OrderBy(t => t.CreatedDate).ToListAsync();

            // Get all available tags for the user's tasks
            var allTags = await _context.Tags
                .Where(tag => _context.TodoTasks
                    .Any(task => task.Tags.Any(t => t.Id == tag.Id) &&
                                (task.TodoList!.OwnerId == userId || task.AssignedUserId == userId)))
                .OrderBy(t => t.Name)
                .ToListAsync();

            ViewData["SearchTerm"] = searchTerm;
            ViewData["SelectedTagIds"] = tagIds ?? Array.Empty<int>();
            ViewData["AllTags"] = allTags;

            return View(tasks);
        }

        // GET: TodoTasks/SearchJson
        [HttpGet]
        public async Task<IActionResult> SearchJson(string? searchTerm, int[]? tagIds)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return Json(new { tasks = new List<object>() });
            }

            var query = _context.TodoTasks
                .Include(t => t.TodoList)
                .Include(t => t.AssignedUser)
                .Include(t => t.Tags)
                .Where(t => t.TodoList!.OwnerId == userId || t.AssignedUserId == userId);

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(t => t.Title != null && t.Title.Contains(searchTerm));
            }

            // Filter by tags (AND logic - task must have all selected tags)
            if (tagIds != null && tagIds.Length > 0)
            {
                foreach (var tagId in tagIds)
                {
                    var currentTagId = tagId; // Capture for closure
                    query = query.Where(t => t.Tags.Any(tag => tag.Id == currentTagId));
                }
            }

            var tasks = await query
                .OrderBy(t => t.CreatedDate)
                .Take(10)
                .Select(t => new
                {
                    id = t.Id,
                    title = t.Title,
                    status = t.Status.ToString(),
                    dueDate = t.DueDate.HasValue ? t.DueDate.Value.ToLocalTime().ToString("MM/dd/yyyy") : null,
                    isOverdue = t.DueDate.HasValue && t.DueDate.Value < DateTime.UtcNow && t.Status != Models.TaskStatus.Completed,
                    todoListTitle = t.TodoList != null ? t.TodoList.Title : null
                })
                .ToListAsync();

            return Json(new { tasks });
        }

        // GET: TodoTasks/GetTagsJson
        [HttpGet]
        public async Task<IActionResult> GetTagsJson()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return Json(new { tags = new List<object>() });
            }

            // Return all tags, not just those associated with tasks
            var tags = await _context.Tags
                .OrderBy(t => t.Name)
                .Select(t => new
                {
                    id = t.Id,
                    name = t.Name
                })
                .ToListAsync();

            return Json(new { tags });
        }

        // POST: TodoTasks/AddTag
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddTag(int taskId, string tagName)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(tagName))
            {
                return Json(new { success = false, message = "Tag name cannot be empty" });
            }

            var task = await _context.TodoTasks
                .Include(t => t.Tags)
                .Include(t => t.TodoList)
                .FirstOrDefaultAsync(t => t.Id == taskId);

            if (task == null)
            {
                return Json(new { success = false, message = "Task not found" });
            }

            // Verify user can edit the task
            if (task.TodoList?.OwnerId != userId && task.AssignedUserId != userId)
            {
                return Json(new { success = false, message = "You don't have permission to edit this task" });
            }

            // Find or create tag (case-insensitive)
            var normalizedTagName = tagName.Trim();
            var tag = await _context.Tags
                .FirstOrDefaultAsync(t => t.Name != null && t.Name.ToLower() == normalizedTagName.ToLower());

            if (tag == null)
            {
                tag = new Tag { Name = normalizedTagName };
                _context.Tags.Add(tag);
                await _context.SaveChangesAsync();
            }

            // Check if tag is already associated with the task
            if (!task.Tags.Any(t => t.Id == tag.Id))
            {
                task.Tags.Add(tag);
                await _context.SaveChangesAsync();
            }

            return Json(new { success = true, tagId = tag.Id, tagName = tag.Name });
        }

        // POST: TodoTasks/RemoveTag
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveTag(int taskId, int tagId)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return NotFound();
            }

            var task = await _context.TodoTasks
                .Include(t => t.Tags)
                .Include(t => t.TodoList)
                .FirstOrDefaultAsync(t => t.Id == taskId);

            if (task == null)
            {
                return Json(new { success = false, message = "Task not found" });
            }

            // Verify user can edit the task
            if (task.TodoList?.OwnerId != userId && task.AssignedUserId != userId)
            {
                return Json(new { success = false, message = "You don't have permission to edit this task" });
            }

            var tag = task.Tags.FirstOrDefault(t => t.Id == tagId);
            if (tag != null)
            {
                task.Tags.Remove(tag);
                await _context.SaveChangesAsync();
            }

            return Json(new { success = true });
        }
    }
}


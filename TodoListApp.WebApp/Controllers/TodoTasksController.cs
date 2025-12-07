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
                .Where(t => t.TodoListId == todoListId)
                .OrderBy(t => t.CreatedDate)
                .ToListAsync();

            ViewData["TodoListId"] = todoListId;
            ViewData["TodoListTitle"] = todoList.Title;

            return View(tasks);
        }

        // GET: TodoTasks/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);

            var todoTask = await _context.TodoTasks
                .Include(t => t.TodoList)
                    .ThenInclude(tl => tl!.Owner)
                .Include(t => t.Tags)
                .Include(t => t.Comments)
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

        // GET: TodoTasks/Create
        public async Task<IActionResult> Create(int? todoListId)
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

            return View();
        }

        // POST: TodoTasks/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Description,Status,DueDate,TodoListId")] TodoTask todoTask)
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
            ModelState.Remove("Comments");

            if (ModelState.IsValid)
            {
                todoTask.CreatedDate = DateTime.UtcNow;
                // Convert local time to UTC for storage
                if (todoTask.DueDate.HasValue)
                {
                    todoTask.DueDate = todoTask.DueDate.Value.ToUniversalTime();
                }
                _context.Add(todoTask);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index), new { todoListId = todoTask.TodoListId });
            }

            ViewData["TodoListId"] = todoTask.TodoListId;
            ViewData["TodoListTitle"] = todoList.Title;

            return View(todoTask);
        }

        // GET: TodoTasks/Edit/5
        public async Task<IActionResult> Edit(int? id)
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

        // POST: TodoTasks/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,Status,DueDate,TodoListId,CreatedDate")] TodoTask todoTask)
        {
            if (id != todoTask.Id)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);

            // Verify the task belongs to a todo list owned by the user
            var originalTask = await _context.TodoTasks
                .Include(t => t.TodoList)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (originalTask == null || originalTask.TodoList?.OwnerId != userId)
            {
                return NotFound();
            }

            ModelState.Remove("TodoList");
            ModelState.Remove("Tags");
            ModelState.Remove("Comments");

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
                        originalTask.DueDate = todoTask.DueDate.Value.ToUniversalTime();
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
                return RedirectToAction(nameof(Index), new { todoListId = originalTask.TodoListId });
            }

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
    }
}


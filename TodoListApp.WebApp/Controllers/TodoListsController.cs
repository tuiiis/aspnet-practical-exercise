using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoListApp.WebApp.Data;
using TodoListApp.WebApp.Models;

namespace TodoListApp.WebApp.Controllers
{
    [Authorize]
    public class TodoListsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TodoListsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /TodoLists
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            var userTodoLists = await _context.TodoLists
                                              .Where(list => list.OwnerId == userId)
                                              .ToListAsync();

            // Get all todo list IDs owned by the user
            var todoListIds = userTodoLists.Select(l => l.Id).ToList();

            // Count tasks by status across all lists
            var taskCounts = await _context.TodoTasks
                .Where(t => todoListIds.Contains(t.TodoListId))
                .GroupBy(t => t.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            var pendingCount = taskCounts.FirstOrDefault(x => x.Status == Models.TaskStatus.Pending)?.Count ?? 0;
            var inProgressCount = taskCounts.FirstOrDefault(x => x.Status == Models.TaskStatus.InProgress)?.Count ?? 0;
            var completedCount = taskCounts.FirstOrDefault(x => x.Status == Models.TaskStatus.Completed)?.Count ?? 0;

            // Count overdue tasks (not completed and past due date) across all lists
            var overdueCount = await _context.TodoTasks
                .Where(t => todoListIds.Contains(t.TodoListId)
                    && t.DueDate.HasValue
                    && t.DueDate.Value < DateTime.UtcNow
                    && t.Status != Models.TaskStatus.Completed)
                .CountAsync();

            ViewData["PendingCount"] = pendingCount;
            ViewData["InProgressCount"] = inProgressCount;
            ViewData["CompletedCount"] = completedCount;
            ViewData["OverdueCount"] = overdueCount;

            return View(userTodoLists);
        }

        // GET: TodoLists/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: TodoLists/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title")] TodoList todoList)
        {
            var userId = _userManager.GetUserId(User);
            todoList.OwnerId = userId!;

            ModelState.Remove("OwnerId");

            if (ModelState.IsValid)
            {
                _context.Add(todoList);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(todoList);
        }

        // GET: TodoLists/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);

            var todoList = await _context.TodoLists
                .FirstOrDefaultAsync(m => m.Id == id && m.OwnerId == userId);

            if (todoList == null)
            {
                return NotFound();
            }

            // Count tasks by status
            var taskCounts = await _context.TodoTasks
                .Where(t => t.TodoListId == id)
                .GroupBy(t => t.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            var pendingCount = taskCounts.FirstOrDefault(x => x.Status == Models.TaskStatus.Pending)?.Count ?? 0;
            var inProgressCount = taskCounts.FirstOrDefault(x => x.Status == Models.TaskStatus.InProgress)?.Count ?? 0;
            var completedCount = taskCounts.FirstOrDefault(x => x.Status == Models.TaskStatus.Completed)?.Count ?? 0;

            // Count overdue tasks (not completed and past due date)
            var overdueCount = await _context.TodoTasks
                .Where(t => t.TodoListId == id
                    && t.DueDate.HasValue
                    && t.DueDate.Value < DateTime.UtcNow
                    && t.Status != Models.TaskStatus.Completed)
                .CountAsync();

            ViewData["PendingCount"] = pendingCount;
            ViewData["InProgressCount"] = inProgressCount;
            ViewData["CompletedCount"] = completedCount;
            ViewData["OverdueCount"] = overdueCount;

            return View(todoList);
        }

        // GET: TodoLists/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);

            var todoList = await _context.TodoLists
                .FirstOrDefaultAsync(m => m.Id == id && m.OwnerId == userId);

            if (todoList == null)
            {
                return NotFound();
            }

            return View(todoList);
        }

        // POST: TodoLists/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title")] TodoList todoList)
        {
            if (id != todoList.Id)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);

            ModelState.Remove("OwnerId");
            ModelState.Remove("Title");

            if (ModelState.IsValid)
            {
                var originalList = await _context.TodoLists
                    .FirstOrDefaultAsync(l => l.Id == id && l.OwnerId == userId);

                if (originalList == null)
                {
                    return NotFound();
                }

                originalList.Title = todoList.Title;
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(todoList);
        }

        // POST: TodoLists/UpdateTitle/5 (AJAX endpoint for inline editing)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateTitle(int id, string title)
        {
            var userId = _userManager.GetUserId(User);

            var todoList = await _context.TodoLists
                .FirstOrDefaultAsync(l => l.Id == id && l.OwnerId == userId);

            if (todoList == null)
            {
                return Json(new { success = false, message = "List not found" });
            }

            if (string.IsNullOrWhiteSpace(title))
            {
                return Json(new { success = false, message = "Title cannot be empty" });
            }

            todoList.Title = title.Trim();
            await _context.SaveChangesAsync();

            return Json(new { success = true, title = todoList.Title });
        }

        // GET: TodoLists/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);

            var todoList = await _context.TodoLists
                .FirstOrDefaultAsync(m => m.Id == id && m.OwnerId == userId);

            if (todoList == null)
            {
                return NotFound();
            }

            return View(todoList);
        }

        // POST: TodoLists/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = _userManager.GetUserId(User);

            var todoList = await _context.TodoLists
                .FirstOrDefaultAsync(m => m.Id == id && m.OwnerId == userId);

            if (todoList == null)
            {
                return NotFound();
            }

            _context.TodoLists.Remove(todoList);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
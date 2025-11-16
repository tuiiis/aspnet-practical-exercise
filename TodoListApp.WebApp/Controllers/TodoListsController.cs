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
            todoList.OwnerId = userId;

            ModelState.Remove("OwnerId");

            if (ModelState.IsValid)
            {
                _context.Add(todoList);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
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
                try
                {
                    var originalList = await _context.TodoLists
                        .FirstOrDefaultAsync(l => l.Id == id && l.OwnerId == userId);

                    if (originalList == null)
                    {
                        return NotFound();
                    }

                    originalList.Title = todoList.Title;

                    _context.Update(originalList);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.TodoLists.Any(e => e.Id == todoList.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            return View(todoList);
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
    }
}
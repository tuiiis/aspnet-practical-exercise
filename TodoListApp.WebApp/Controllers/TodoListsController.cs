using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoListApp.WebApp.Data;
using TodoListApp.WebApp.Models;

namespace TodoListApp.WebApp.Controllers
{
    [Authorize] // This is crucial! It blocks access for anyone not logged in.
    public class TodoListsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        // Constructor to get the services we need (Db context, User manager)
        public TodoListsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        //
        // GET: /TodoLists
        // This is US01: View the list of my to-do lists.
        //
        public async Task<IActionResult> Index()
        {
            // 1. Get the ID of the currently logged-in user.
            var userId = _userManager.GetUserId(User);

            // 2. Find all TodoLists in the database where the OwnerId matches the current user's ID.
            var userTodoLists = await _context.TodoLists
                                              .Where(list => list.OwnerId == userId)
                                              .ToListAsync();

            // 3. Pass that list of data to the view.
            return View(userTodoLists);
        }

        // GET: TodoLists/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: TodoLists/Create
        // This [HttpPost] attribute means it only runs on a form POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title")] TodoList todoList)
        {
            var userId = _userManager.GetUserId(User);
            todoList.OwnerId = userId;

            // This line tells validation to stop worrying about OwnerId
            ModelState.Remove("OwnerId");

            if (ModelState.IsValid) // This will now pass (as long as Title is present)
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

            // 1. Get the current user's ID.
            var userId = _userManager.GetUserId(User);

            // 2. Find the list in the DB *only if* the ID matches AND the user is the owner.
            //    This is a crucial security check.
            var todoList = await _context.TodoLists
                .FirstOrDefaultAsync(m => m.Id == id && m.OwnerId == userId);

            if (todoList == null)
            {
                // If it's not found (or not owned by the user), return 404.
                return NotFound();
            }

            // 3. Show the form and pass in the list we found.
            return View(todoList);
        }

        // POST: TodoLists/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title")] TodoList todoList)
        {
            // This check makes sure the ID from the URL matches the ID from the form's hidden field
            if (id != todoList.Id)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);

            // We must remove OwnerId from validation, just like we did in Create
            // We'll also remove Title for a moment so we can fetch the real object first.
            ModelState.Remove("OwnerId");
            ModelState.Remove("Title");

            // Check if the (empty) model is valid so far
            if (ModelState.IsValid)
            {
                try
                {
                    // 1. Find the *original* list from the database
                    var originalList = await _context.TodoLists
                        .FirstOrDefaultAsync(l => l.Id == id && l.OwnerId == userId);

                    // 2. Security check: If it's null, it's not theirs or doesn't exist.
                    if (originalList == null)
                    {
                        return NotFound();
                    }

                    // 3. Update the original list's title with the new one from the form
                    originalList.Title = todoList.Title;

                    // 4. Save the changes to the database
                    _context.Update(originalList);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    // This is a rare error case, but it's good practice to have
                    if (!_context.TodoLists.Any(e => e.Id == todoList.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                // 5. Send the user back to the main list page
                return RedirectToAction(nameof(Index));
            }

            // If anything failed, show the form again
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

            // Find the list, ensuring it's owned by the current user
            var todoList = await _context.TodoLists
                .FirstOrDefaultAsync(m => m.Id == id && m.OwnerId == userId);

            if (todoList == null)
            {
                // Not found or not owned by user
                return NotFound();
            }

            // Pass the list to the view
            return View(todoList);
        }

        // POST: TodoLists/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = _userManager.GetUserId(User);

            // Find the list, ensuring it's owned by the current user
            var todoList = await _context.TodoLists
                .FirstOrDefaultAsync(m => m.Id == id && m.OwnerId == userId);

            if (todoList == null)
            {
                // Not found or not owned by user
                return NotFound();
            }

            // Remove the list from the context
            _context.TodoLists.Remove(todoList);

            // Save the change to the database
            await _context.SaveChangesAsync();

            // Send the user back to the main list page
            return RedirectToAction(nameof(Index));
        }
    }
}
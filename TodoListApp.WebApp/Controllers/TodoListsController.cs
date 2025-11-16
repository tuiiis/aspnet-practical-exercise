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
    }
}
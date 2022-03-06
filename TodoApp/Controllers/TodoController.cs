using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TodoApp.Data;
using TodoApp.Models;

namespace TodoApp.Controllers
{
    [Authorize]
    public class TodoController : Controller
    {
        private readonly ApplicationDbContext _context;
        public TodoController(ApplicationDbContext context)
            => _context = context;      
        public async Task<IActionResult> Index() {
            return View(await _context.Todos
                .AsNoTracking()
                .Where(x => x.User == User.Identity.Name)
                .ToListAsync());
        }

        public async Task<IActionResult> Details(int? id) {
            if (id == null) return NotFound();
            
            var todo = await _context.Todos
                .FirstOrDefaultAsync(m => m.Id == id);

            if (todo == null) return NotFound();
            if (todo.User != User.Identity.Name) return NotFound();
            
            return View(todo);
        }

        public IActionResult Create() {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title")] Todo todo) {
            if (ModelState.IsValid) {
                todo.User = User.Identity.Name;
                _context.Add(todo);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            if (todo.User != User.Identity.Name) return NotFound();

            return View(todo);
        }
        public async Task<IActionResult> Edit(int? id) {
            if (id == null) return NotFound();

            var todo = await _context.Todos.FindAsync(id);

            if (todo == null) return NotFound();

            return View(todo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Done")] Todo todo) {
            if (id != todo.Id) return NotFound();

            if (ModelState.IsValid) {
                try {
                    todo.User = User.Identity.Name;
                    todo.LastUpdateDate = DateTime.Now;
                    
                    _context.Update(todo);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException) {
                    if (!TodoExists(todo.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }

            if (todo.User != User.Identity.Name) return NotFound();

            return View(todo);
        }
        public async Task<IActionResult> Delete(int? id) {
            if (id == null) return NotFound();
        
            var todo = await _context.Todos
                .FirstOrDefaultAsync(m => m.Id == id);

            if (todo == null) return NotFound();
            if (todo.User != User.Identity.Name) return NotFound();
            
            return View(todo);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var todo = await _context.Todos.FindAsync(id);
            _context.Todos.Remove(todo);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TodoExists(int id) {
            return _context.Todos.Any(e => e.Id == id);
        }
    }
}

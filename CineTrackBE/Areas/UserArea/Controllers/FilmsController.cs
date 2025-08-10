using CineTrackBE.Models.Entities;
using CineTrackBE.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CineTrackBE.Areas.UserArea.Controllers;


[Area("UserArea")]
[Authorize(Roles = "Admin,User")]
public class FilmsController(IFilmService filmService) : Controller
{
    private readonly IFilmService _filmService = filmService;


    // INDEX //
    public async Task<IActionResult> Index()
    {
        return View(await _filmService.GetFilmList());
    }


    // DETAILS //
    public async Task<IActionResult> Details(string id)
    {
        if (int.TryParse(id, out int intId)) return NotFound();


        var film = await _filmService.GetFilm_Id(intId);
        if (film == null) return NotFound();

        return View(film);
    }


    // CREATE //
    public IActionResult Create() => View();


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,Name,Description,Director,Year")] Film film)
    {
        if (ModelState.IsValid)
        {
            _context.Add(film);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(film);
    }

    // GET: UserArea/Films/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var film = await _context.Films.FindAsync(id);
        if (film == null)
        {
            return NotFound();
        }
        return View(film);
    }

    // POST: UserArea/Films/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Director,Year")] Film film)
    {
        if (id != film.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(film);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FilmExists(film.Id))
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
        return View(film);
    }

    // GET: UserArea/Films/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var film = await _context.Films
            .FirstOrDefaultAsync(m => m.Id == id);
        if (film == null)
        {
            return NotFound();
        }

        return View(film);
    }

    // POST: UserArea/Films/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var film = await _context.Films.FindAsync(id);
        if (film != null)
        {
            _context.Films.Remove(film);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool FilmExists(int id)
    {
        return _context.Films.Any(e => e.Id == id);
    }
}

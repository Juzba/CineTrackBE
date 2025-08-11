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
        if (!int.TryParse(id, out int intId)) return NotFound();


        var film = await _filmService.GetFilm_Id(intId);
        if (film == null) return NotFound();

        return View(film);
    }


    // CREATE //
    [Authorize(Roles ="Admin")]
    public IActionResult Create() => View();


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,Name,Description,Director,Year")] Film film)
    {
        if (ModelState.IsValid)
        {
            await _filmService.AddFilm(film);
            await _filmService.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(film);
    }

    // EDIT //
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(string id)
    {
        if (!int.TryParse(id, out int intId)) return NotFound();

        var film = await _filmService.GetFilm_Id(intId);
        if (film == null) return NotFound();

        return View(film);
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Director,Year")] Film film)
    {
        if (id != film.Id) return NotFound();


        if (ModelState.IsValid)
        {
            try
            {
                _filmService.UpdateFilm(film);
                await _filmService.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_filmService.AnyFilm_Id(film.Id)) return NotFound();

                else throw;

            }
            return RedirectToAction(nameof(Index));
        }
        return View(film);
    }


    // DELETE //
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(string id)
    {
        if (!int.TryParse(id, out int intId)) return NotFound();

        var film = await _filmService.GetFilm_Id(intId);
        if (film == null) return NotFound();

        return View(film);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var film = await _filmService.GetFilm_Id(id);
        if (film != null)
        {
            _filmService.RemoveFilm(film);
        }

        await _filmService.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

}

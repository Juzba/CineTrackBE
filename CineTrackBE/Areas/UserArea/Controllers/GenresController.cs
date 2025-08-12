using CineTrackBE.Models.Entities;
using CineTrackBE.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CineTrackBE.Areas.UserArea.Controllers
{
    [Area("UserArea")]
    [Authorize(Roles = "Admin,User")]
    public class GenresController(IGenreService genreService) : Controller
    {
        private readonly IGenreService _genreService = genreService;

        // INDEX //
        public async Task<IActionResult> Index()
        {
            var genres = await _genreService.GetGenreList();
            return View(await genres.ToListAsync());
        }

        // DETAILS //
        public async Task<IActionResult> Details(string id)
        {
            if (!int.TryParse(id, out int intId)) return NotFound();

            var genre = await _genreService.GetGenreByIdAsync(intId);
            if (genre == null) return NotFound();

            return View(genre);
        }

        // CREATE //
        [Authorize(Roles = "Admin")]
        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name")] Genre genre)
        {
            if (ModelState.IsValid)
            {
                await _genreService.AddGenreAsync(genre);
                await _genreService.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(genre);
        }

        // EDIT //
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(string id)
        {
            if (!int.TryParse(id, out int intId)) return NotFound();

            var genre = await _genreService.GetGenreByIdAsync(intId);
            if (genre == null) return NotFound();

            return View(genre);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name")] Genre genre)
        {
            if (id != genre.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    await _genreService.UpdateGenreAsync(genre);
                    await _genreService.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _genreService.AnyGenreExistsAsync(genre.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(genre);
        }

        // DELETE //
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(string id)
        {
            if (!int.TryParse(id, out int intId)) return NotFound();

            var genre = await _genreService.GetGenreByIdAsync(intId);
            if (genre == null) return NotFound();

            return View(genre);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var genre = await _genreService.GetGenreByIdAsync(id);
            if (genre != null)
            {
                await _genreService.RemoveGenre(genre);
            }

            await _genreService.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
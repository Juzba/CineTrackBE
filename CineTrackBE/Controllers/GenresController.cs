using CineTrackBE.AppServices;
using CineTrackBE.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CineTrackBE.Controllers
{
    [Authorize(Roles = "Admin,User")]
    public class GenresController(IRepository<Genre> genreRepository) : Controller
    {
        private readonly IRepository<Genre> _genreRepository = genreRepository;

        // INDEX //
        public async Task<IActionResult> Index()
        {
            return View(await _genreRepository.GetList().ToListAsync());
        }

        // DETAILS //
        public async Task<IActionResult> Details(string id)
        {
            if (!int.TryParse(id, out int intId)) return NotFound();

            var genre = await _genreRepository.GetAsync_Id(intId);
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
                await _genreRepository.AddAsync(genre);
                await _genreRepository.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(genre);
        }

        // EDIT //
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(string id)
        {
            if (!int.TryParse(id, out int intId)) return NotFound();

            var genre = await _genreRepository.GetAsync_Id(intId);
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
                    _genreRepository.Update(genre);
                    await _genreRepository.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _genreRepository.AnyExistsAsync(genre.Id)) return NotFound();
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

            var genre = await _genreRepository.GetAsync_Id(intId);
            if (genre == null) return NotFound();

            return View(genre);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var genre = await _genreRepository.GetAsync_Id(id);
            if (genre != null)
            {
                _genreRepository.Remove(genre);
            }

            await _genreRepository.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
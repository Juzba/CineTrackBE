using CineTrackBE.AppServices;
using CineTrackBE.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CineTrackBE.Controllers
{
    [Authorize(Roles = "Admin,User")]
    public class GenresController(ILogger<GenresController> logger, IRepository<Genre> genreRepository) : Controller
    {
        private readonly IRepository<Genre> _genreRepository = genreRepository;
        private readonly ILogger<GenresController> _logger = logger;

        // INDEX //
        public async Task<IActionResult> Index()
        {
            return View(await _genreRepository.GetList().ToListAsync());
        }

        // DETAILS //
        public async Task<IActionResult> Details(string id)
        {
            if (!int.TryParse(id, out int intId))
            {
                _logger.LogWarning("Invalid genre ID format: {GenreId}", id);
                return NotFound();
            }

            var genre = await _genreRepository.GetAsync_Id(intId);
            if (genre == null)
            {
                _logger.LogWarning("Genre with ID {GenreId} not found.", intId);
                return NotFound();
            }

            return View(genre);
        }

        // CREATE //
        [Authorize(Roles = "Admin")]
        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name")] Genre genre)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Genre creation failed due to invalid model state.");
                return View(genre);
            }

            try
            {
                await _genreRepository.AddAsync(genre);
                await _genreRepository.SaveChangesAsync();

                _logger.LogInformation("New genre created with ID {GenreId}", genre.Id);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating a new genre.");
                ModelState.AddModelError(string.Empty, "An error occurred while creating the genre. Please try again.");
                return View(genre);
            }
        }


        // EDIT //
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(string id)
        {
            if (!int.TryParse(id, out int intId))
            {
                _logger.LogWarning("Invalid genre ID format: {GenreId}", id);
                return NotFound();
            }

            var genre = await _genreRepository.GetAsync_Id(intId);
            if (genre == null)
            {
                _logger.LogWarning("Genre with ID {GenreId} not found for editing.", intId);
                return NotFound();
            }

            return View(genre);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name")] Genre genre)
        {
            if (id != genre.Id)
            {
                _logger.LogWarning("Genre update failed: ID mismatch (route ID: {RouteId}, model ID: {ModelId}).", id, genre.Id);
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Genre update failed due to invalid model state for ID {GenreId}.", genre.Id);
                return View(genre);
            }

            try
            {
                _genreRepository.Update(genre);
                await _genreRepository.SaveChangesAsync();
                _logger.LogInformation("Genre with ID {GenreId} updated successfully.", genre.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating genre with ID {GenreId}.", genre.Id);
                ModelState.AddModelError(string.Empty, "An error occurred while updating the genre. Please try again.");
                return View(genre);
            }

            return RedirectToAction(nameof(Index));
        }


        // DELETE //
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(string id)
        {
            if (!int.TryParse(id, out int intId)) 
            {
                _logger.LogWarning("Invalid genre ID format: {GenreId}", id);
                return NotFound();
            }

            var genre = await _genreRepository.GetAsync_Id(intId);
            if (genre == null)
            {
                _logger.LogWarning("Genre with ID {GenreId} not found for deletion.", intId);
                return NotFound();
            }

            return View(genre);
        }


        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {

            if (id <= 0)
            {
                _logger.LogWarning("Invalid genre ID {GenreId} provided for deletion.", id);
                return BadRequest();
            }

            var genre = await _genreRepository.GetAsync_Id(id);

            if (genre == null)
            {
                _logger.LogWarning("Attempted to delete non-existent genre with ID {GenreId}.", id);
                return NotFound();
            }

            try
            {
                _genreRepository.Remove(genre);

                await _genreRepository.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting genre with ID {GenreId}.", id);
                ModelState.AddModelError(string.Empty, "An error occurred while deleting the genre. Please try again.");
                return View(genre);
            }
        }
    }
}
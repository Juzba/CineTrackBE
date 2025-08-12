using CineTrackBE.Models.Entities;
using CineTrackBE.Models.ViewModel;
using CineTrackBE.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace CineTrackBE.Controllers;

[Authorize(Roles = "Admin,User")]
public class FilmsController(IRepository<Film> filmRepository, IRepository<Genre> genreRepository, IDataService dataService) : Controller
{

    private readonly IRepository<Film> _filmRepository = filmRepository;
    private readonly IRepository<Genre> _genreRepository = genreRepository;
    private readonly IDataService _dataService = dataService;


    // INDEX //
    public async Task<IActionResult> Index()
    {
        return View(await _filmRepository.GetList().Include(p=>p.FilmGenres).ThenInclude(p=>p.Genre).ToListAsync());
    }

    // DETAILS //
    public async Task<IActionResult> Details(string id)
    {
        if (!int.TryParse(id, out int intId)) return NotFound();

        var film = await _filmRepository.GetAsync_Id(intId);
        if (film == null) return NotFound();

        return View(film);
    }

    // CREATE //
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create()
    {
        var allGenres = await _genreRepository.GetList().ToListAsync();

        return View(new FilmViewModel() { AllGenres = allGenres });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(FilmViewModel filmViewModel)
    {
        if (ModelState.IsValid)
        {

            // ZDE omezit model maximalne na tri genre //
            //if(filmViewModel.SelectedGenresId.Count > 3) ModelState.is


            await _filmRepository.AddAsync(filmViewModel.Film);
            await _filmRepository.SaveChangesAsync();


            
            await _dataService.AddGenresToFilmAsync(filmViewModel.Film, filmViewModel.SelectedGenresId);
            await _filmRepository.SaveChangesAsync();


            return RedirectToAction(nameof(Index));
        }
        return View(filmViewModel);
    }

    // EDIT //
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(string id)
    {
        if (!int.TryParse(id, out int intId)) return NotFound();

        var film = await _filmRepository.GetAsync_Id(intId);
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
                _filmRepository.Update(film);
                await _filmRepository.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _filmRepository.AnyExistsAsync(film.Id)) return NotFound();
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

        var film = await _filmRepository.GetAsync_Id(intId);
        if (film == null) return NotFound();

        return View(film);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var film = await _filmRepository.GetAsync_Id(id);
        if (film != null)
        {
            _filmRepository.Remove(film);
        }

        await _filmRepository.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}
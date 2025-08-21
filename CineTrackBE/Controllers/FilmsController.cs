using CineTrackBE.AppServices;
using CineTrackBE.Models.Entities;
using CineTrackBE.Models.ViewModel;
using CineTrackBE.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CineTrackBE.Controllers;

[Authorize(Roles = "Admin,User")]
public class FilmsController(UserManager<ApplicationUser> userManager,IRepository<Film> filmRepository, IRepository<Genre> genreRepository, IDataService dataService) : Controller
{

    private readonly IRepository<Film> _filmRepository = filmRepository;
    private readonly IRepository<Genre> _genreRepository = genreRepository;
    private readonly IDataService _dataService = dataService;
    private readonly UserManager<ApplicationUser> _userManager = userManager;


    // INDEX //
    public async Task<IActionResult> Index()
    {
        return View(await _filmRepository.GetList().Include(p => p.FilmGenres).ThenInclude(p => p.Genre).ToListAsync());
    }

    // DETAILS //
    public async Task<IActionResult> Details(string id)
    {
        if (!int.TryParse(id, out int intId)) return NotFound();

        var film = await _dataService.GetFilmAsync_InclFilmGenres(intId);
        if (film == null) return NotFound();

        var user = await _userManager.GetUserAsync(User);

        if(user?.FavoriteMovies != null && user.FavoriteMovies.Any(p => p == film.Id))
        {
            ViewBag.IsFavorite = true;
        }
        else
        {
            ViewBag.IsFavorite = false;
        }

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

            await _filmRepository.AddAsync(filmViewModel.Film);
            await _filmRepository.SaveChangesAsync();


            if (filmViewModel.Film != null && filmViewModel.SelectedGenresId != null && filmViewModel.SelectedGenresId.Count > 0)
            {
                await _dataService.AddGenresToFilmAsync(filmViewModel.Film, filmViewModel.SelectedGenresId);
                await _filmRepository.SaveChangesAsync();
            }


            return RedirectToAction(nameof(Index));
        }

        filmViewModel.AllGenres = await _genreRepository.GetList().ToListAsync();

        return View(filmViewModel);
    }


    // EDIT //
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(string id)
    {
        if (!int.TryParse(id, out int intId)) return NotFound();


        var allGenres = await _genreRepository.GetList().ToListAsync();
        var film = await _dataService.GetFilmAsync_InclFilmGenres(intId);
        if (film == null) return NotFound();


        var filmViewModel = new FilmViewModel()
        {
            Film = film,
            AllGenres = allGenres,
            SelectedGenresId = [.. film.FilmGenres.Select(p => p.GenreId)]
        };


        return View(filmViewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, FilmViewModel filmViewModel)
    {
        if (id != filmViewModel.Film.Id) return NotFound();


        if (ModelState.IsValid)
        {
            await _dataService.RemoveFilmGenres(id);
            await _filmRepository.SaveChangesAsync();
            await _dataService.AddGenresToFilmAsync(filmViewModel.Film, filmViewModel.SelectedGenresId);

            try
            {
                _filmRepository.Update(filmViewModel.Film);
                await _filmRepository.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _filmRepository.AnyExistsAsync(filmViewModel.Film.Id)) return NotFound();
                else throw;
            }
            return RedirectToAction(nameof(Index));
        }

        filmViewModel.AllGenres = await _genreRepository.GetList().ToListAsync();

        return View(filmViewModel);
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
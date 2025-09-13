using CineTrackBE.AppServices;
using CineTrackBE.Models.Entities;
using CineTrackBE.Models.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Elfie.Serialization;
using Microsoft.EntityFrameworkCore;
using System.Threading;

namespace CineTrackBE.Controllers;

[Authorize(Roles = "Admin,User")]
public class FilmsController( IRepository<FilmGenre> filmGenreRepository ,ILogger<FilmsController> logger, UserManager<ApplicationUser> userManager, IRepository<Film> filmRepository, IRepository<Genre> genreRepository, IDataService dataService) : Controller
{
    private readonly ILogger<FilmsController> _logger = logger;
    private readonly IRepository<Film> _filmRepository = filmRepository;
    private readonly IRepository<Genre> _genreRepository = genreRepository;
    private readonly IRepository<FilmGenre> _filmGenreRepository = filmGenreRepository;
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
        if (!int.TryParse(id, out int intId))
        { 
            _logger.LogWarning("Invalid film ID format: {FilmId}", id);
            return NotFound();
        }

         var film = await _filmRepository.GetList()
            .Include(p => p.FilmGenres)
            .ThenInclude(p => p.Genre)
            .FirstOrDefaultAsync(p => p.Id == intId);

        if (film == null)
        { 
            _logger.LogWarning("Film not found with ID: {FilmId}", intId);
            return NotFound();
        }

        var user = await _userManager.GetUserAsync(User);

        if (user?.FavoriteMovies != null && user.FavoriteMovies.Any(p => p == film.Id))
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
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid model state when creating a new film.");
            return View(filmViewModel);
        }

        if (filmViewModel.Film == null || filmViewModel.SelectedGenresId == null || filmViewModel.SelectedGenresId.Count <= 0)
        {
            _logger.LogWarning("Film or SelectedGenresId is null or empty when creating a new film.");
            ModelState.AddModelError(string.Empty, "Něco je špatně vyplněno.");
            return View(filmViewModel);
        }

        using var transaction = await _filmRepository.BeginTransactionAsync();
        try
        {
            await _filmRepository.AddAsync(filmViewModel.Film);
            await _filmRepository.SaveChangesAsync();


            await _dataService.AddGenresToFilmAsync(filmViewModel.Film, filmViewModel.SelectedGenresId);
           

            await transaction.CommitAsync();
            _logger.LogInformation("New film created with ID {FilmId}", filmViewModel.Film!.Id);
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error occurred while creating a new film.");
            ModelState.AddModelError(string.Empty, "Nastala chyba při ukládání filmu do databáze: " + ex.Message);
            return View(filmViewModel);
        }
    }


    // EDIT //
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(string id)
    {
        if (!int.TryParse(id, out int intId)) 
        {
            _logger.LogWarning("Invalid film ID format: {FilmId}", id);
            return NotFound();
        }

        var allGenres = await _genreRepository.GetList().ToListAsync();
        var film = await _filmRepository.GetList()
            .Include(p => p.FilmGenres)
            .ThenInclude(p => p.Genre)
            .FirstOrDefaultAsync(p => p.Id == intId);

        if (film == null) 
        { 
            _logger.LogWarning("Film not found with ID: {FilmId}", intId);
            return NotFound(); 
        }

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
        if (id != filmViewModel.Film.Id)
        {
            _logger.LogWarning("Film ID mismatch when editing film. Route ID: {RouteId}, Model ID: {ModelId}", id, filmViewModel.Film.Id);
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid model state when editing film with ID {FilmId}", id);
            return View(filmViewModel);
        }


        using var transaction = await _filmRepository.BeginTransactionAsync();
        try
        {
            // Remove existing genres
            var filmGenres = await _filmGenreRepository.GetList().Where(p => p.FilmId == id).ToListAsync();
             _filmGenreRepository.RemoveRange(filmGenres); 
            await _filmRepository.SaveChangesAsync();

            // Add selected genres
            await _dataService.AddGenresToFilmAsync(filmViewModel.Film, filmViewModel.SelectedGenresId);

            _filmRepository.Update(filmViewModel.Film);
            await _filmRepository.SaveChangesAsync();

            await transaction.CommitAsync();
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error occurred while editing film with ID {FilmId}", filmViewModel.Film.Id);
            ModelState.AddModelError(string.Empty, "Nastala chyba při ukládání filmu do databáze: " + ex.Message);
            return View(filmViewModel);
        }


    }

    // DELETE //
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(string id)
    {
        if (!int.TryParse(id, out int intId)) 
        {
            _logger.LogWarning("Invalid film ID format: {FilmId}", id);
            return NotFound();
        }

        var film = await _filmRepository.GetAsync(intId);
        if (film == null)
        { 
            _logger.LogWarning("Film not found with ID: {FilmId}", intId);
            return NotFound();
        }

        return View(film);
    }


    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var film = await _filmRepository.GetAsync(id);
        if (film == null)
        {
            _logger.LogWarning("Attempted to delete non-existent film with ID {FilmId}", id);
            return NotFound();
        }

        try
        {
            _filmRepository.Remove(film);

            await _filmRepository.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting film with ID {FilmId}", id);
            ModelState.AddModelError(string.Empty, "Nastala chyba při mazání filmu z databáze: " + ex.Message);
            return View(film);

        }
    }
}
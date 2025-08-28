using CineTrackBE.AppServices;
using CineTrackBE.Models.DTO;
using CineTrackBE.Models.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CineTrackBE.ApiControllers;

[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ApiController]

public class FilmApiController(IRepository<Film> filmRepository, IRepository<Rating> ratingRepository, IRepository<Comment> CommentRepository, IRepository<ApplicationUser> userRepository, IRepository<Genre> genreRepository, IDataService dataService) : ControllerBase
{
    private readonly IRepository<Film> _filmRepository = filmRepository;
    private readonly IRepository<Genre> _genreRepository = genreRepository;
    private readonly IRepository<ApplicationUser> _userRepository = userRepository;
    private readonly IRepository<Comment> _commentRepository = CommentRepository;
    private readonly IRepository<Rating> _ratingRepository = ratingRepository;
    private readonly IDataService _dataService = dataService;


    // Top 5 Latest Films //
    [HttpGet("LatestFilms")]
    public async Task<ActionResult<IEnumerable<FilmDto>>> GetLatestFilms()
    {
        var films = await _filmRepository.GetList().OrderByDescending(p => p.ReleaseDate).Take(5).ToListAsync();

        var filmsDTO = films.Select(p => new FilmDto()
        {
            Id = p.Id,
            Name = p.Name,
            Director = p.Director,
            ImageFileName = p.ImageFileName,
            Description = p.Description,
            ReleaseDate = p.ReleaseDate,
            Genres = [.. p.FilmGenres.Select(g => new GenreDto { Id = g.Genre.Id, Name = g.Genre.Name})]
        });

        return Ok(filmsDTO);
    }


    // Get all genres //
    [HttpGet("AllGenres")]
    public async Task<ActionResult<IEnumerable<GenreDto>>> GetAllGenres()
    {
        var genres = await _genreRepository.GetList().ToListAsync();

        var genresDto = genres.Select(p => new GenreDto()
        {
            Id = p.Id,
            Name = p.Name
        });

        return Ok(genresDto);
    }

    // SEARCH FILMS BY PARAMETERS //
    [HttpPost("CatalogSearch")]
    public async Task<ActionResult<IEnumerable<Film>>> CatalogPost([FromBody] SearchParametrsDto? searchParams)
    {
        // get film list from db
        var films = _filmRepository.GetList().Include(p => p.FilmGenres).ThenInclude(p => p.Genre).AsQueryable();

        // search by name
        if (!string.IsNullOrWhiteSpace(searchParams?.SearchText))
        {
            films = films.Where(p => p.Name.Contains(searchParams.SearchText));
        }

        // search by release date
        if (searchParams?.SearchByYear != null && searchParams.SearchByYear > 0)
        {
            films = films.Where(p => p.ReleaseDate.Year == searchParams.SearchByYear);
        }

        // search by genre
        if (searchParams?.GenreId != null && searchParams.GenreId > 0)
        {
            films = films.Where(p => p.FilmGenres.Any(p => p.GenreId == searchParams.GenreId));
        }

        // order
        switch (searchParams?.SearchOrder)
        {
            case "NameDesc":
                films = films.OrderByDescending(p => p.Name);
                break;
            case "NameAsc":
                films = films.OrderBy(p => p.Name);
                break;
            case "YearDesc":
                films = films.OrderByDescending(p => p.ReleaseDate);
                break;
            case "YearAsc":
                films = films.OrderBy(p => p.ReleaseDate);
                break;
            default:
                films = films.OrderByDescending(p => p.Id);
                break;
        }

        var searchResult = await films.ToListAsync();

        // data transfer object
        var filmsDTO = searchResult.Select(p => new FilmDto()
        {
            Id = p.Id,
            Name = p.Name,
            Director = p.Director,
            ImageFileName = p.ImageFileName,
            Description = p.Description,
            ReleaseDate = p.ReleaseDate,
            Genres = [.. p.FilmGenres.Select(g => new GenreDto { Id = g.Genre.Id, Name = g.Genre.Name })]
        });

        return Ok(filmsDTO);
    }


    // GET FILM DETAILS BY ID //
    [HttpGet("FilmDetails/{id}")]
    public async Task<ActionResult<FilmDto>> GetFilm(int id)
    {
        if (id <= 0) return BadRequest("Film ID must be greater than 0.");

        // get film from db
        var film = await _filmRepository.GetList().Include(p => p.FilmGenres).ThenInclude(p => p.Genre).FirstOrDefaultAsync(p => p.Id == id);
        if (film == null) return NotFound($"Film with ID {id} not found.");

        // is film user favorite?
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized("User not authenticated!");

        var user = await _userRepository.GetAsync_Id(userId);
        if (user == null) return Unauthorized("User not authenticated!");

        var isFavoriteFilm = user.FavoriteMovies.Any(p => p == film.Id);

        //Get avg rating
        var comments = await _commentRepository
            .GetList()
            .Where(p => p.FilmId == film.Id)
            .Include(p => p.Rating)
            .ToListAsync();


        double avgRating;
        if (comments.Count > 0) avgRating = comments.Select(p => p.Rating.UserRating).Average();
        else avgRating = 0;



        var filmsDTO = new FilmDto()
        {
            Id = film.Id,
            Name = film.Name,
            Director = film.Director,
            ImageFileName = film.ImageFileName,
            Description = film.Description,
            ReleaseDate = film.ReleaseDate,
            IsMyFavorite = isFavoriteFilm,
            AvgRating = avgRating,
            Genres = [.. film.FilmGenres.Select(g => new GenreDto { Id = g.Genre.Id, Name = g.Genre.Name})]
        };

        return Ok(filmsDTO);
    }


    // ADD OR REMOVE FILM FROM FAVORITES //
    [HttpGet("ToggleFavorite/{filmId}")]
    public async Task<ActionResult<bool>> ToggleFavorite(int filmId)
    {
        if (filmId <= 0) return BadRequest("Film ID must be greater than 0.");


        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId)) return Unauthorized("User not authenticated.");

        var user = await _userRepository.GetAsync_Id(userId);

        if (user == null) return Unauthorized("User not found.");

        if (user.FavoriteMovies.Any(p => p == filmId))
        {
            user.FavoriteMovies.Remove(filmId);
        }
        else
        {
            user.FavoriteMovies.Add(filmId);
        }

        _userRepository.Update(user);

        var result = await _userRepository.SaveChangesAsync();

        if (!result) return NotFound($"Film with ID {filmId} not found.");
        return Ok(result);
    }


    // ADD COMMENT //
    [HttpPost("AddComment")]
    public async Task<ActionResult<bool>> AddComment([FromBody] CommentWithRatingDto comment)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);


        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized("User not found.");


        var newComment = new Comment
        {
            AutorId = userId,
            FilmId = comment.FilmId,
            SendDate = DateTime.Now,
            Text = comment.Text,


        };

        await _commentRepository.AddAsync(newComment);
        var commentSaved = await _commentRepository.SaveChangesAsync();
        if (!commentSaved) return StatusCode(StatusCodes.Status500InternalServerError, "Failed to save comment");


        var newRating = new Rating
        {
            UserRating = comment.Rating,
            CommentId = newComment.Id
        };

        // Přidáme Rating
        await _ratingRepository.AddAsync(newRating);
        var ratingSaved = await _ratingRepository.SaveChangesAsync();
        if (!ratingSaved)
        {
            _commentRepository.Remove(newComment);
            await _commentRepository.SaveChangesAsync();
            return StatusCode(StatusCodes.Status500InternalServerError, "Failed to save rating");
        }

        return Ok(true);
    }


    // GET COMMENTS FOR FILM //
    [HttpGet("GetComments/{filmId}")]
    public async Task<ActionResult<IEnumerable<GenreDto>>> GetAllGenres(int filmId)
    {
        if (filmId <= 0) return BadRequest("Film ID must be greater than 0.");

        var comments = await _commentRepository.GetList()
            .Where(p => p.FilmId == filmId)
            .Include(p => p.Autor)
            .Include(p => p.Rating)
            .OrderByDescending(p => p.SendDate)
            .ToListAsync();

        var commentDto = comments.Select(p => new CommentDto
        {
            Id = p.Id,
            SendDate = p.SendDate,
            Text = p.Text,
            AuthorName = p.Autor.UserName,
            Rating = p.Rating.UserRating
        });

        return Ok(commentDto);
    }



    // GET USER PROFIL DATA AND STATISTICS //
    [HttpGet("UserProfilData")]
    public async Task<ActionResult<UserProfilDataDto>> GetUserProfilData()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized("User not found.");
        var user = await _userRepository.GetAsync_Id(userId);
        if (user == null) return Unauthorized("User not found.");

        // get all user favorite films
        var allFavoriteFilms = _filmRepository.GetList().OrderDescending()
            .Where(p => user.FavoriteMovies.Contains(p.Id));

        // user favorite film count
        var favoriteFilmsCount = allFavoriteFilms.Count();

        // get 20 favorite films
        var favoriteFilms = await allFavoriteFilms.Take(20).ToListAsync();

        // last favorite film
        var lastFavoritefilm = await _filmRepository.GetList().FirstOrDefaultAsync(p => user.FavoriteMovies.LastOrDefault() == p.Id);


        var favoriteFilmsDto = favoriteFilms.Select(p => new FavoriteFilms
        {
            Id = p.Id,
            Title = p.Name,
            ImagePath = p.ImageFileName,
        }).ToList();

        // get all user comments
        var comments = _commentRepository.GetList()
            .Where(p => p.AutorId == userId)
            .Include(p => p.Film)
            .Include(p => p.Rating)
            .OrderByDescending(p => p.SendDate);

        // user total comments
        var totalUserComments = comments.Count();

        // get top rating
        var topRating = await comments.AnyAsync() ? await comments.MaxAsync(p => p.Rating.UserRating) : 0;

        // get user average rating
        var myAvgRating = await comments.AnyAsync() ? ((int)await comments.Select(p => p.Rating.UserRating).AverageAsync()) : 0;

        // get latest comments
        var latestComments = await comments.Take(10).ToListAsync();

        var latestCommentsDto = latestComments.Select(p => new LatestComment
        {
            Id = p.Id,
            FilmTitle = p.Film.Name,
            CommentDate = p.SendDate,
            CommentText = p.Text,
            Rating = p.Rating.UserRating
        }).ToList();

        var userProfilData = new UserProfilDataDto
        {
            FavoriteFilms = favoriteFilmsDto,
            LatestComments = latestCommentsDto,
            AvgRating = myAvgRating,
            FavoriteFilmsCount = favoriteFilmsCount,
            LastFavoriteFilmTitle = lastFavoritefilm?.Name ?? "",
            TopRating = topRating,
            TotalComments = totalUserComments

        };
        return Ok(userProfilData);
    }
      
}

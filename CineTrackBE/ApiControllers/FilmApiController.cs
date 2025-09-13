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

public class FilmApiController(ILogger<FilmApiController> logger, IRepository<Film> filmRepository, IRepository<Rating> ratingRepository, IRepository<Comment> CommentRepository, IRepository<ApplicationUser> userRepository, IRepository<Genre> genreRepository) : ControllerBase
{
    private readonly ILogger<FilmApiController> _logger = logger;
    private readonly IRepository<Film> _filmRepository = filmRepository;
    private readonly IRepository<Genre> _genreRepository = genreRepository;
    private readonly IRepository<ApplicationUser> _userRepository = userRepository;
    private readonly IRepository<Comment> _commentRepository = CommentRepository;
    private readonly IRepository<Rating> _ratingRepository = ratingRepository;



    // Top 5 Latest Films //
    // User Favorite Films //
    [HttpGet("DashBoardFilms")]
    public async Task<ActionResult<DashBoardDto>> GetFilmsForDashBoard()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                _logger.LogWarning("User not authenticated when accessing dashboard films.");
                return Unauthorized("User not authenticated!");
            }

            var user = await _userRepository.GetAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("User not authenticated when accessing dashboard films.");
                return Unauthorized("User not authenticated!");
            }

            var favoriteFilms = await _filmRepository.GetList()
                .Include(p => p.FilmGenres)
                .ThenInclude(p => p.Genre)
                .Where(p => user.FavoriteMovies.Contains(p.Id))
                .Take(30)
                .ToListAsync();

            var latestFilms = await _filmRepository.GetList()
                .Include(p => p.FilmGenres)
                .ThenInclude(p => p.Genre)
                .OrderByDescending(p => p.ReleaseDate)
                .Take(5)
                .ToListAsync();


            var latestsFilmsDTO = latestFilms.Select(p => new FilmDto()
            {
                Id = p.Id,
                Name = p.Name,
                Director = p.Director,
                ImageFileName = p.ImageFileName,
                Description = p.Description,
                ReleaseDate = p.ReleaseDate,
                Genres = [.. p.FilmGenres.Select(g => new GenreDto { Id = g.Genre.Id, Name = g.Genre.Name })]
            });

            var favoriteFilmsDto = favoriteFilms
                .OrderByDescending(f => Array.IndexOf([.. user.FavoriteMovies], f.Id))
                .Select(p => new FilmDto()
                {
                    Id = p.Id,
                    Name = p.Name,
                    Director = p.Director,
                    ImageFileName = p.ImageFileName,
                    Description = p.Description,
                    ReleaseDate = p.ReleaseDate,
                    Genres = [.. p.FilmGenres.Select(g => new GenreDto { Id = g.Genre.Id, Name = g.Genre.Name })]
                });

            var DashBoardDto = new DashBoardDto
            {
                LatestFilms = [.. latestsFilmsDTO],
                FavoriteFilms = [.. favoriteFilmsDto]
            };

            if (!latestsFilmsDTO.Any())
            {
                _logger.LogWarning("No films found in the database.");
                return Ok(DashBoardDto);
            }

            _logger.LogInformation("Fetched latest films: {FilmsDTO}", latestsFilmsDTO);
            return Ok(DashBoardDto);

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching latest films from the database.");
            return StatusCode(500, "An error occurred while fetching latest films.");
        }
    }

    // Get all genres //
    [HttpGet("AllGenres")]
    public async Task<ActionResult<IEnumerable<GenreDto>>> GetAllGenres()
    {
        try
        {
            var genres = await _genreRepository.GetAllAsync();
            if (!genres.Any())
            {
                _logger.LogWarning("No genres found in the database.");
                return Ok(Enumerable.Empty<GenreDto>());
            }

            var genresDto = genres.Select(p => new GenreDto()
            {
                Id = p.Id,
                Name = p.Name
            });

            _logger.LogInformation("Fetched all genres: {GenresDto}", genresDto);
            return Ok(genresDto);

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching genres from the database.");
            return StatusCode(500, "An error occurred while fetching genres.");
        }
    }


    // SEARCH FILMS BY PARAMETERS //
    [HttpPost("CatalogSearch")]
    public async Task<ActionResult<IEnumerable<FilmDto>>> CatalogPost([FromBody] SearchParametrsDto? searchParams)
    {
        try
        {
            // get film list from db
            var films = _filmRepository.GetList().Include(p => p.FilmGenres).ThenInclude(p => p.Genre).AsQueryable();

            // search by name
            if (!string.IsNullOrWhiteSpace(searchParams?.SearchText))
            {
                films = films.Where(p => p.Name.ToLower().Contains(searchParams.SearchText.ToLower()));
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
            if (searchResult.Count == 0)
            {
                _logger.LogInformation("No films found for parameters: {@SearchParams}", searchParams);
                return Ok(Enumerable.Empty<FilmDto>());
            }


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

            _logger.LogInformation("Search films with parameters: {SearchParams}, found: {FilmsDTO}", searchParams, filmsDTO);
            return Ok(filmsDTO);

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching films with parameters: {SearchParams}", searchParams);
            return StatusCode(500, "An error occurred while searching films.");
        }
    }


    // GET FILM DETAILS BY ID //
    [HttpGet("FilmDetails/{id}")]
    public async Task<ActionResult<FilmDto>> GetFilm(int id)
    {
        try
        {

            if (User == null || User.Identity == null || !User.Identity.IsAuthenticated)
            {
                _logger.LogWarning("User not authenticated when accessing film ID {Id}.", id);
                return Unauthorized("User not authenticated!");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("User not authenticated when accessing film ID {Id}.", id);
                return Unauthorized("User not authenticated!");
            }

            var user = await _userRepository.GetAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("User with ID {UserId} not found when accessing film ID {Id}.", userId, id);
                return Unauthorized("User not authenticated!");
            }

            if (id <= 0)
            {
                _logger.LogWarning("Invalid film ID: {Id}", id);
                return BadRequest("Film ID must be greater than 0.");
            }


            // get film from db
            var film = await _filmRepository.GetList().Include(p => p.FilmGenres).ThenInclude(p => p.Genre).FirstOrDefaultAsync(p => p.Id == id);
            if (film == null)
            {
                _logger.LogWarning("Film with ID {Id} not found.", id);
                return NotFound($"Film with ID {id} not found.");
            }

            var isFavoriteFilm = user.FavoriteMovies.Any(p => p == film.Id);

            //Get avg rating
            var avgRating = await _commentRepository.GetList()
                .Where(p => p.FilmId == film.Id)
                .Include(p => p.Rating)
                .Select(p => p.Rating.UserRating)
                .DefaultIfEmpty()
                .AverageAsync();


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
                Genres = [.. film.FilmGenres.Select(g => new GenreDto { Id = g.Genre.Id, Name = g.Genre.Name })]
            };

            _logger.LogInformation("Fetched film details for ID {Id}: {FilmDTO}", id, filmsDTO);
            return Ok(filmsDTO);

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching film details for ID {Id}", id);
            return StatusCode(500, "An error occurred while fetching film details.");
        }
    }


    // ADD OR REMOVE FILM FROM FAVORITES //
    [HttpGet("ToggleFavorite/{filmId}")]
    public async Task<ActionResult<bool>> ToggleFavorite(int filmId)
    {
        try
        {
            if (User == null || User.Identity == null || !User.Identity.IsAuthenticated)
            {
                _logger.LogWarning("User not authenticated when toggling favorite for film ID {FilmId}.", filmId);
                return Unauthorized("User not authenticated!");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("User not authenticated when toggling favorite for film ID {FilmId}.", filmId);
                return Unauthorized("User not authenticated!");
            }

            var user = await _userRepository.GetAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("User not authenticated when toggling favorite for film ID {FilmId}.", filmId);
                return Unauthorized("User not authenticated!");
            }

            if (filmId <= 0)
            {
                _logger.LogWarning("Invalid film ID for toggling favorite: {FilmId}", filmId);
                return BadRequest("Film ID must be greater than 0.");
            }

            // Verify film exists
            var filmExists = await _filmRepository.AnyAsync(p => p.Id == filmId);
            if (!filmExists)
            {
                _logger.LogWarning("Film with ID {FilmId} not found.", filmId);
                return NotFound($"Film with ID {filmId} not found.");
            }


            // Initialize collection if null
            user.FavoriteMovies ??= [];
            bool isFavorite;

            if (user.FavoriteMovies.Any(p => p == filmId))
            {
                user.FavoriteMovies.Remove(filmId);
                isFavorite = false;
            }
            else
            {
                user.FavoriteMovies.Add(filmId);
                isFavorite = true;
            }

            await _userRepository.SaveChangesAsync();
            return Ok(isFavorite);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling favorite for film ID {FilmId} by user ID", filmId);
            return StatusCode(500, "An error occurred while updating favorite films.");
        }
    }


    // ADD COMMENT //
    [HttpPost("AddComment")]
    public async Task<ActionResult<CommentWithRatingDto>> AddComment([FromBody] CommentWithRatingDto comment)
    {

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid comment model state: {ModelState}", ModelState);
            return BadRequest(ModelState);
        }

        if (User == null || User.Identity == null || !User.Identity.IsAuthenticated)
        {
            _logger.LogWarning("User not authenticated when adding comment: {Comment}", comment);
            return Unauthorized("User not authenticated!");
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("User not authenticated when adding comment: {Comment}", comment);
            return Unauthorized("User not authenticated!");
        }

        var newComment = new Comment
        {
            AutorId = userId,
            FilmId = comment.FilmId,
            SendDate = comment.SendDate,
            Text = comment.Text,
        };


        using var transaction = await _commentRepository.BeginTransactionAsync();

        try
        {
            // Add Comment
            await _commentRepository.AddAsync(newComment);
            await _commentRepository.SaveChangesAsync();

            var newRating = new Rating
            {
                UserRating = comment.Rating,
                CommentId = newComment.Id
            };

            // Add Rating
            await _ratingRepository.AddAsync(newRating);
            await _ratingRepository.SaveChangesAsync();

            await transaction.CommitAsync();


            var result = new CommentWithRatingDto
            {
                Id = newComment.Id,
                FilmId = newComment.FilmId,
                Rating = newRating.UserRating,
                Text = newComment.Text,
                AutorName = User.Identity?.Name,
                SendDate = newComment.SendDate
            };

            _logger.LogInformation("Comment added successfully: {Comment}", comment);
            return Ok(result);

        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error adding comment: {Comment}", comment);
            return StatusCode(500, "An error occurred while adding the comment.");
        }

    }


    // GET COMMENTS FOR FILM //
    [HttpGet("GetComments/{filmId}")]
    public async Task<ActionResult<IEnumerable<CommentWithRatingDto>>> GetAllComments(int filmId)
    {
        try
        {
            if (filmId <= 0)
            {
                _logger.LogWarning("Invalid film ID for fetching comments: {FilmId}", filmId);
                return BadRequest("Film ID must be greater than 0.");
            }

            // Verify film exists
            var filmExists = await _filmRepository.AnyAsync(p => p.Id == filmId);
            if (!filmExists)
            {
                _logger.LogWarning("Film with ID {FilmId} not found when fetching comments.", filmId);
                return NotFound($"Film with ID {filmId} not found.");
            }

            var comments = await _commentRepository.GetList()
                .Where(p => p.FilmId == filmId)
                .Include(p => p.Autor)
                .Include(p => p.Rating)
                .OrderByDescending(p => p.SendDate)
                .ToListAsync();

            if (comments.Count == 0)
            {
                _logger.LogInformation("No comments found for film ID {FilmId}.", filmId);
                return Ok(Enumerable.Empty<CommentWithRatingDto>());
            }

            var commentWithRatingDto = comments.Select(p => new CommentWithRatingDto
            {
                Id = p.Id,
                SendDate = p.SendDate,
                Text = p.Text,
                AutorName = p.Autor.UserName,
                Rating = p.Rating.UserRating,
                FilmId = p.FilmId
            });

            _logger.LogInformation("Fetched comments for film ID {FilmId}: {Comments}", filmId, commentWithRatingDto);
            return Ok(commentWithRatingDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching comments for film ID {FilmId}", filmId);
            return StatusCode(500, "An error occurred while fetching comments.");
        }
    }


    // GET USER PROFIL DATA AND STATISTICS //
    [HttpGet("UserProfilData")]
    public async Task<ActionResult<UserProfilDataDto>> GetUserProfilData()
    {
        try
        {
            if (User == null || User.Identity == null || !User.Identity.IsAuthenticated)
            {
                _logger.LogWarning("User not authenticated when accessing profile data.");
                return Unauthorized("User not authenticated!");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("User not authenticated when accessing profile data.");
                return Unauthorized("User not authenticated!");
            }

            var user = await _userRepository.GetAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("User not authenticated when accessing profile data.");
                return Unauthorized("User not authenticated!");
            }

            // get all user favorite films ( user statistics page )
            var allFavoriteFilmsQuery = _filmRepository.GetList()
                .Where(p => user.FavoriteMovies.Contains(p.Id))
                .OrderDescending();

            // user favorite film count ( user statistics page )
            var favoriteFilmsCount = allFavoriteFilmsQuery.Count();

            // get 20 favorite films ( user statistics page )
            var favoriteFilms = await allFavoriteFilmsQuery.Take(20).ToListAsync();

            // last favorite film ( user statistics page)
            var lastFavoritefilm = await _filmRepository.GetList().FirstOrDefaultAsync(p => user.FavoriteMovies.LastOrDefault() == p.Id);


            var favoriteFilmsDto = favoriteFilms.Select(p => new FavoriteFilms
            {
                Id = p.Id,
                Title = p.Name,
                ImagePath = p.ImageFileName,
            }).ToList();

            // get all user comments ( user statistics page )
            var comments = _commentRepository.GetList()
                .Where(p => p.AutorId == userId)
                .Include(p => p.Film)
                .Include(p => p.Rating)
                .OrderByDescending(p => p.SendDate);

            // user total comments ( user statistics page )
            var totalUserComments = comments.Count();

            // get top rating ( user statistics page )
            var topRating = await comments.AnyAsync() ? await comments.MaxAsync(p => p.Rating.UserRating) : 0;

            // get user average rating ( user statistics page )
            var myAvgRating = await comments.AnyAsync() ? ((int)await comments.Select(p => p.Rating.UserRating).AverageAsync()) : 0;

            // get latest comments ( user statistics page )
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
            _logger.LogInformation("Fetched profile data for user ID {UserId}: {UserProfilData}", userId, userProfilData);
            return Ok(userProfilData);

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching user profile data for user ID");
            return StatusCode(500, "An error occurred while fetching user profile data.");
        }
    }
}

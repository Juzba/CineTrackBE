using CineTrackBE.Models.DTO;
using CineTrackBE.Tests.Helpers.Common;
using CineTrackBE.Tests.Helpers.TestSetups;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace CineTrackBE.Tests.Integration.ApiEndpoints
{
    public class AdminApiEndpointsTest2
    {
        // ADD FILM //
        [Fact]
        public async Task AddFilm__ShouldReturnCreatedAtActionResult_WithFilmDto()
        {
            // Arrange
            using var setup = AdminApiControllerTestSetup.Create();

            var genres = await Fakers.Genre.GenerateAndSaveAsync(2, setup.Context);
            var genresDto = genres.Select(p => new GenreDto { Id = p.Id, Name = p.Name });

            var filmDto = Fakers.FilmDto.Generate();
            filmDto.Genres.AddRange(genresDto);

            // Act
            var result = await setup.Controller.AddFilm(filmDto);

            // Assert
            var okResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
            var filmDtoResult = okResult.Value.Should().BeOfType<FilmDto>().Subject;
            filmDtoResult.Should().BeEquivalentTo(filmDto, o => o.Excluding(p => p.Id));
        }


        [Fact]
        public async Task AddFilm__ShouldAddFilmToDb_IncludingFilmGenres()
        {
            // Arrange
            using var setup = AdminApiControllerTestSetup.Create();

            var genres = await Fakers.Genre.GenerateAndSaveAsync(2, setup.Context);
            var genresDto = genres.Select(p => new GenreDto { Id = p.Id, Name = p.Name });

            var filmDto = Fakers.FilmDto.Generate();
            filmDto.Genres.AddRange(genresDto);

            // Act
            var result = await setup.Controller.AddFilm(filmDto);
            var filmsInDb = await setup.FilmRepository.GetAllAsync();
            var filmGenresInDb = await setup.FilmGenreRepository.GetAllAsync();

            // Assert
            filmsInDb.Should().NotBeNull();
            filmsInDb.Should().ContainSingle();
            filmsInDb.First().Director.Should().Be(filmDto.Director);

            filmGenresInDb.Should().NotBeNull();
            filmGenresInDb.Should().HaveCount(genres.Count);

            var filmId = filmsInDb.First().Id;
            filmGenresInDb.Should().OnlyContain(p => p.FilmId == filmId);
            //filmGenresInDb.Should().OnlyContain(p => p. == filmId);
          
         
        }








        // genres in dto does not exist in db should return badrequest
        // dopsat do programu
        // film add to db
        // modelstate invalid
        // db problem 500
        // transaction problem



















    }
}

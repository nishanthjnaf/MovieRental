using Moq;
using MovieRentalAPI.Exceptions;
using MovieRentalAPI.Interfaces;
using MovieRentalAPI.Models;
using MovieRentalAPI.Models.DTOs;
using MovieRentalAPI.Services;
using Xunit;

namespace MovieRentalAPI.Tests;

public class GenreServiceTests
{
    private readonly Mock<IRepository<int, Genre>> _genreRepo = new();
    private readonly Mock<IRepository<int, Movie>> _movieRepo = new();
    private readonly Mock<IRepository<int, Review>> _reviewRepo = new();
    private readonly GenreService _sut;

    public GenreServiceTests()
    {
        _sut = new GenreService(_genreRepo.Object, _movieRepo.Object, _reviewRepo.Object);
    }

    // ── AddGenre ────────────────────────────────────────────────────────────

    [Fact]
    public async Task AddGenre_ValidRequest_ReturnsDto()
    {
        _genreRepo.Setup(r => r.GetAll()).ReturnsAsync(new List<Genre>());
        _genreRepo.Setup(r => r.Add(It.IsAny<Genre>()))
            .ReturnsAsync((Genre g) => { g.Id = 1; return g; });

        var result = await _sut.AddGenre(new GenreRequestDto { Name = "Action", Description = "Action movies" });

        Assert.Equal("Action", result.Name);
        Assert.Equal(1, result.Id);
    }

    [Fact]
    public async Task AddGenre_EmptyName_ThrowsBadRequest()
    {
        await Assert.ThrowsAsync<BadRequestException>(() =>
            _sut.AddGenre(new GenreRequestDto { Name = "" }));
    }

    [Fact]
    public async Task AddGenre_WhitespaceName_ThrowsBadRequest()
    {
        await Assert.ThrowsAsync<BadRequestException>(() =>
            _sut.AddGenre(new GenreRequestDto { Name = "   " }));
    }

    [Fact]
    public async Task AddGenre_DuplicateName_ThrowsConflict()
    {
        _genreRepo.Setup(r => r.GetAll())
            .ReturnsAsync(new List<Genre> { new() { Id = 1, Name = "Action" } });

        await Assert.ThrowsAsync<ConflictException>(() =>
            _sut.AddGenre(new GenreRequestDto { Name = "action" }));
    }

    [Fact]
    public async Task AddGenre_AddReturnsNull_ThrowsException()
    {
        _genreRepo.Setup(r => r.GetAll()).ReturnsAsync(new List<Genre>());
        _genreRepo.Setup(r => r.Add(It.IsAny<Genre>())).ReturnsAsync((Genre?)null);

        await Assert.ThrowsAsync<Exception>(() =>
            _sut.AddGenre(new GenreRequestDto { Name = "Drama" }));
    }

    // ── GetAllGenres ────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAllGenres_ReturnsAll()
    {
        _genreRepo.Setup(r => r.GetAll())
            .ReturnsAsync(new List<Genre> { new() { Id = 1, Name = "Action" }, new() { Id = 2, Name = "Drama" } });

        var result = (await _sut.GetAllGenres()).ToList();
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetAllGenres_Empty_ThrowsNotFound()
    {
        _genreRepo.Setup(r => r.GetAll()).ReturnsAsync(new List<Genre>());
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetAllGenres());
    }

    [Fact]
    public async Task GetAllGenres_Null_ThrowsNotFound()
    {
        _genreRepo.Setup(r => r.GetAll()).ReturnsAsync((IEnumerable<Genre>?)null);
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetAllGenres());
    }

    // ── GetGenreById ────────────────────────────────────────────────────────

    [Fact]
    public async Task GetGenreById_Exists_ReturnsDto()
    {
        _genreRepo.Setup(r => r.Get(1)).ReturnsAsync(new Genre { Id = 1, Name = "Action" });
        var result = await _sut.GetGenreById(1);
        Assert.Equal(1, result.Id);
        Assert.Equal("Action", result.Name);
    }

    [Fact]
    public async Task GetGenreById_NotFound_ThrowsNotFound()
    {
        _genreRepo.Setup(r => r.Get(99)).ReturnsAsync((Genre?)null);
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetGenreById(99));
    }

    // ── UpdateGenre ─────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateGenre_Valid_ReturnsUpdated()
    {
        var genre = new Genre { Id = 1, Name = "Old" };
        _genreRepo.Setup(r => r.Get(1)).ReturnsAsync(genre);
        _genreRepo.Setup(r => r.Update(1, It.IsAny<Genre>()))
            .ReturnsAsync((int _, Genre g) => g);

        var result = await _sut.UpdateGenre(1, new GenreRequestDto { Name = "New", Description = "Desc" });
        Assert.Equal("New", result.Name);
    }

    [Fact]
    public async Task UpdateGenre_EmptyName_ThrowsBadRequest()
    {
        await Assert.ThrowsAsync<BadRequestException>(() =>
            _sut.UpdateGenre(1, new GenreRequestDto { Name = "" }));
    }

    [Fact]
    public async Task UpdateGenre_NotFound_ThrowsNotFound()
    {
        _genreRepo.Setup(r => r.Get(99)).ReturnsAsync((Genre?)null);
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _sut.UpdateGenre(99, new GenreRequestDto { Name = "X" }));
    }

    [Fact]
    public async Task UpdateGenre_UpdateReturnsNull_ThrowsException()
    {
        _genreRepo.Setup(r => r.Get(1)).ReturnsAsync(new Genre { Id = 1, Name = "Old" });
        _genreRepo.Setup(r => r.Update(1, It.IsAny<Genre>())).ReturnsAsync((Genre?)null);

        await Assert.ThrowsAsync<Exception>(() =>
            _sut.UpdateGenre(1, new GenreRequestDto { Name = "New" }));
    }

    // ── DeleteGenre ─────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteGenre_Exists_ReturnsTrue()
    {
        var genre = new Genre { Id = 1, Name = "Action" };
        _genreRepo.Setup(r => r.Get(1)).ReturnsAsync(genre);
        _genreRepo.Setup(r => r.Delete(1)).ReturnsAsync(genre);

        var result = await _sut.DeleteGenre(1);
        Assert.True(result);
    }

    [Fact]
    public async Task DeleteGenre_NotFound_ThrowsNotFound()
    {
        _genreRepo.Setup(r => r.Get(99)).ReturnsAsync((Genre?)null);
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.DeleteGenre(99));
    }

    [Fact]
    public async Task DeleteGenre_DeleteReturnsNull_ThrowsException()
    {
        _genreRepo.Setup(r => r.Get(1)).ReturnsAsync(new Genre { Id = 1, Name = "X" });
        _genreRepo.Setup(r => r.Delete(1)).ReturnsAsync((Genre?)null);
        await Assert.ThrowsAsync<Exception>(() => _sut.DeleteGenre(1));
    }

    // ── AssignGenreToMovie ──────────────────────────────────────────────────

    [Fact]
    public async Task AssignGenreToMovie_Valid_ReturnsTrue()
    {
        var movie = new Movie { Id = 1, Title = "Film", Genres = new List<Genre>() };
        var genre = new Genre { Id = 2, Name = "Drama" };
        _movieRepo.Setup(r => r.Get(1)).ReturnsAsync(movie);
        _genreRepo.Setup(r => r.Get(2)).ReturnsAsync(genre);
        _movieRepo.Setup(r => r.Update(1, It.IsAny<Movie>())).ReturnsAsync(movie);

        var result = await _sut.AssignGenreToMovie(1, 2);
        Assert.True(result);
    }

    [Fact]
    public async Task AssignGenreToMovie_MovieNotFound_ThrowsNotFound()
    {
        _movieRepo.Setup(r => r.Get(99)).ReturnsAsync((Movie?)null);
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.AssignGenreToMovie(99, 1));
    }

    [Fact]
    public async Task AssignGenreToMovie_GenreNotFound_ThrowsNotFound()
    {
        _movieRepo.Setup(r => r.Get(1)).ReturnsAsync(new Movie { Id = 1, Title = "Film", Genres = new List<Genre>() });
        _genreRepo.Setup(r => r.Get(99)).ReturnsAsync((Genre?)null);
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.AssignGenreToMovie(1, 99));
    }

    [Fact]
    public async Task AssignGenreToMovie_AlreadyAssigned_ThrowsConflict()
    {
        var genre = new Genre { Id = 2, Name = "Drama" };
        var movie = new Movie { Id = 1, Title = "Film", Genres = new List<Genre> { genre } };
        _movieRepo.Setup(r => r.Get(1)).ReturnsAsync(movie);
        _genreRepo.Setup(r => r.Get(2)).ReturnsAsync(genre);

        await Assert.ThrowsAsync<ConflictException>(() => _sut.AssignGenreToMovie(1, 2));
    }

    // ── GetMoviesByGenreName ────────────────────────────────────────────────

    [Fact]
    public async Task GetMoviesByGenreName_EmptyName_ThrowsBadRequest()
    {
        await Assert.ThrowsAsync<BadRequestException>(() => _sut.GetMoviesByGenreName(""));
    }

    [Fact]
    public async Task GetMoviesByGenreName_NotFound_ThrowsNotFound()
    {
        _genreRepo.Setup(r => r.GetAll()).ReturnsAsync(new List<Genre>());
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetMoviesByGenreName("Unknown"));
    }
}

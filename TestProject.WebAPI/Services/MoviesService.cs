using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using TestProject.WebAPI.Data;

namespace TestProject.WebAPI.Services
{
    public class MoviesService : IMoviesService
    {
        private readonly TestProjectContext _testProjectContext;
        private readonly IStringLocalizer _localizer;

        public MoviesService(TestProjectContext testProjectContext, IStringLocalizer localizer)
        {
            _testProjectContext = testProjectContext;
            _localizer = localizer;
        }

        public async Task<IEnumerable<Movie>> Get(int[] ids)
        {
            var movies = _testProjectContext.Movies.AsQueryable();

            if (ids != null && ids.Any())
                movies = movies.Where(x => ids.Contains(x.Id));
            var result = await movies.AsNoTracking().ToListAsync();

            result.ForEach(x=>x.Category = _localizer[x.Category]?.Value);

            return result;
        }

        public async Task<Movie> Add(Movie movie)
        {
            await _testProjectContext.Movies.AddAsync(movie);

            await _testProjectContext.SaveChangesAsync();
            return movie;
        }

        public async Task<Movie> Update(Movie movie)
        {
            var movieForChange = await _testProjectContext.Movies.SingleAsync(x => x.Id == movie.Id);

            movieForChange.Title = movie.Title;

            _testProjectContext.Movies.Update(movieForChange);
            await _testProjectContext.SaveChangesAsync();
            return movie;
        }

        public async Task<bool> Delete(Movie movie)
        {
            var movieForDelete = _testProjectContext.Movies.Single(x => x.Id == movie.Id);
            _testProjectContext.Movies.Remove(movieForDelete);
            await _testProjectContext.SaveChangesAsync();

            return true;
        }
    }

    public interface IMoviesService
    {
        Task<IEnumerable<Movie>> Get(int[] ids);

        Task<Movie> Add(Movie movie);

        Task<Movie> Update(Movie movie);

        Task<bool> Delete(Movie movie);
    }
}

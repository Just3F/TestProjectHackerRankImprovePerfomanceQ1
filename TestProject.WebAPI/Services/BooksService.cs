using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TestProject.WebAPI.Data;

namespace TestProject.WebAPI.Services
{
    public class BooksService : IBooksService
    {
        private readonly TestProjectContext _testProjectContext;

        public BooksService(TestProjectContext testProjectContext)
        {
            _testProjectContext = testProjectContext;
        }

        public async Task<IEnumerable<Song>> Get(int[] ids, Filters filters)
        {
            var books = _testProjectContext.Songs.AsQueryable();

            if (filters == null)
                filters = new Filters();

            if (filters.Body != null && filters.Body.Any())
                books = books.Where(x => filters.Body.Contains(x.Singer));

            if (filters.AuthorNames != null && filters.AuthorNames.Any())
                books = books.Where(x => filters.AuthorNames.Contains(x.Singer));

            if (filters.Title != null && filters.Title.Any())
                books = books.Where(x => filters.Title.Contains(x.Name));

            if (ids != null && ids.Any())
                books = books.Where(x => ids.Contains(x.Id));

            return await books.ToListAsync();
        }

        public async Task<Song> Add(Song song)
        {
            await _testProjectContext.Songs.AddAsync(song);
            song.ReleaseDate = DateTime.UtcNow;

            await _testProjectContext.SaveChangesAsync();
            return song;
        }

        public async Task<IEnumerable<Song>> AddRange(IEnumerable<Song> books)
        {
            await _testProjectContext.Songs.AddRangeAsync(books);
            await _testProjectContext.SaveChangesAsync();
            return books;
        }

        public async Task<Song> Update(Song song)
        {
            var bookForChanges = await _testProjectContext.Songs.SingleAsync(x => x.Id == song.Id);
            bookForChanges.Singer = song.Singer;
            bookForChanges.Name = song.Name;

            _testProjectContext.Songs.Update(bookForChanges);
            await _testProjectContext.SaveChangesAsync();
            return song;
        }

        public async Task<bool> Delete(Song song)
        {
            _testProjectContext.Songs.Remove(song);
            await _testProjectContext.SaveChangesAsync();

            return true;
        }
    }

    public interface IBooksService
    {
        Task<IEnumerable<Song>> Get(int[] ids, Filters filters);

        Task<Song> Add(Song song);

        Task<IEnumerable<Song>> AddRange(IEnumerable<Song> books);

        Task<Song> Update(Song song);

        Task<bool> Delete(Song song);
    }

    public class Filters
    {
        public string[] Body { get; set; }
        public string[] AuthorNames { get; set; }
        public string[] Title { get; set; }
    }
}

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

        public async Task<IEnumerable<Book>> Get(int[] ids, Filters filters)
        {
            var books = _testProjectContext.Books.AsQueryable();

            if (filters == null)
                filters = new Filters();

            if (filters.Body != null && filters.Body.Any())
                books = books.Where(x => filters.Body.Contains(x.Body));

            if (filters.AuthorNames != null && filters.AuthorNames.Any())
                books = books.Where(x => filters.AuthorNames.Contains(x.AuthorName));

            if (filters.Title != null && filters.Title.Any())
                books = books.Where(x => filters.Title.Contains(x.Title));

            if (ids != null && ids.Any())
                books = books.Where(x => ids.Contains(x.Id));

            await Task.Delay(2000);

            return await books.ToListAsync();
        }

        public async Task<Book> Add(Book book)
        {
            await _testProjectContext.Books.AddAsync(book);
            book.PublishedDate = DateTime.UtcNow;

            await _testProjectContext.SaveChangesAsync();
            return book;
        }

        public async Task<IEnumerable<Book>> AddRange(IEnumerable<Book> books)
        {
            await _testProjectContext.Books.AddRangeAsync(books);
            await _testProjectContext.SaveChangesAsync();
            return books;
        }

        public async Task<Book> Update(Book book)
        {
            var bookForChanges = await _testProjectContext.Books.SingleAsync(x => x.Id == book.Id);
            bookForChanges.Body = book.Body;
            bookForChanges.Title = book.Title;

            _testProjectContext.Books.Update(bookForChanges);
            await _testProjectContext.SaveChangesAsync();
            return book;
        }

        public async Task<bool> Delete(Book book)
        {
            _testProjectContext.Books.Remove(book);
            await _testProjectContext.SaveChangesAsync();

            return true;
        }
    }

    public interface IBooksService
    {
        Task<IEnumerable<Book>> Get(int[] ids, Filters filters);

        Task<Book> Add(Book book);

        Task<IEnumerable<Book>> AddRange(IEnumerable<Book> books);

        Task<Book> Update(Book book);

        Task<bool> Delete(Book book);
    }

    public class Filters
    {
        public string[] Body { get; set; }
        public string[] AuthorNames { get; set; }
        public string[] Title { get; set; }
    }
}

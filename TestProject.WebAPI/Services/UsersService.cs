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

        public async Task<IEnumerable<Book>> Get(int libraryId, int[] ids)
        {
            var users = _testProjectContext.Books.Where(x => x.LibraryId == libraryId).AsQueryable();

            if (ids != null && ids.Any())
                users = users.Where(x => ids.Contains(x.Id));

            return await users.ToListAsync();
        }

        public async Task<Book> Add(Book book)
        {
            await _testProjectContext.Books.AddAsync(book);

            await _testProjectContext.SaveChangesAsync();
            return book;
        }

        public async Task<Book> Update(Book book)
        {
            var userForChanges = await _testProjectContext.Books.SingleAsync(x => x.Id == book.Id);

            userForChanges.Name = book.Name;

            _testProjectContext.Books.Update(userForChanges);
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
        Task<IEnumerable<Book>> Get(int libraryId, int[] ids);

        Task<Book> Add(Book book);

        Task<Book> Update(Book book);

        Task<bool> Delete(Book book);
    }
}

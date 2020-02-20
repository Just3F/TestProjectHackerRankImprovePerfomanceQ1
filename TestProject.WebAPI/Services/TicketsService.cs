using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TestProject.WebAPI.Data;

namespace TestProject.WebAPI.Services
{
    public class TicketsService : ITicketsService
    {
        private readonly TestProjectContext _testProjectContext;

        public TicketsService(TestProjectContext testProjectContext)
        {
            _testProjectContext = testProjectContext;
        }

        public async Task<IEnumerable<Ticket>> Get(int[] ids, Filters filters)
        {
            var books = _testProjectContext.Tickets.AsQueryable();

            if (filters == null)
                filters = new Filters();

            if (filters.Descriptions != null && filters.Descriptions.Any())
                books = books.Where(x => filters.Descriptions.Contains(x.Description));

            if (filters.EventDates != null && filters.EventDates.Any())
                books = books.Where(x => filters.EventDates.Contains(x.EventDate));

            if (filters.Title != null && filters.Title.Any())
                books = books.Where(x => filters.Title.Contains(x.Title));

            if (ids != null && ids.Any())
                books = books.Where(x => ids.Contains(x.Id));

            return await books.ToListAsync();
        }

        public async Task<Ticket> Add(Ticket ticket)
        {
            await _testProjectContext.Tickets.AddAsync(ticket);

            await _testProjectContext.SaveChangesAsync();
            return ticket;
        }

        public async Task<IEnumerable<Ticket>> AddRange(IEnumerable<Ticket> books)
        {
            await _testProjectContext.Tickets.AddRangeAsync(books);
            await _testProjectContext.SaveChangesAsync();
            return books;
        }

        public async Task<Ticket> Update(Ticket ticket)
        {
            var bookForChanges = await _testProjectContext.Tickets.SingleAsync(x => x.Id == ticket.Id);
            bookForChanges.Description = ticket.Description;
            bookForChanges.EventDate = ticket.EventDate;
            bookForChanges.Title = ticket.Title;

            _testProjectContext.Tickets.Update(bookForChanges);
            await _testProjectContext.SaveChangesAsync();
            return ticket;
        }

        public async Task<bool> Delete(Ticket ticket)
        {
            _testProjectContext.Tickets.Remove(ticket);
            await _testProjectContext.SaveChangesAsync();

            return true;
        }
    }

    public interface ITicketsService
    {
        Task<IEnumerable<Ticket>> Get(int[] ids, Filters filters);

        Task<Ticket> Add(Ticket ticket);

        Task<IEnumerable<Ticket>> AddRange(IEnumerable<Ticket> books);

        Task<Ticket> Update(Ticket ticket);

        Task<bool> Delete(Ticket ticket);
    }

    public class Filters
    {
        public string[] Descriptions { get; set; }
        public DateTime[] EventDates { get; set; }
        public string[] Title { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TestProject.WebAPI.Data;

namespace TestProject.WebAPI.Services
{
    public class RoomsService : IRoomsService
    {
        private readonly TestProjectContext _testProjectContext;

        public RoomsService(TestProjectContext testProjectContext)
        {
            _testProjectContext = testProjectContext;
        }

        public async Task<IEnumerable<Room>> Get(int[] ids, Filters filters)
        {
            var books = _testProjectContext.Rooms.AsQueryable();

            if (filters == null)
                filters = new Filters();

            if (filters.Categories != null && filters.Categories.Any())
                books = books.Where(x => filters.Categories.Contains(x.Category));

            if (filters.Floors != null && filters.Floors.Any())
                books = books.Where(x => filters.Floors.Contains(x.Floor));

            if (ids != null && ids.Any())
                books = books.Where(x => ids.Contains(x.Id));

            return await books.ToListAsync();
        }

        public async Task<Room> Add(Room room)
        {
            await _testProjectContext.Rooms.AddAsync(room);
            room.AddedDate = DateTime.UtcNow;

            await _testProjectContext.SaveChangesAsync();
            return room;
        }

        public async Task<IEnumerable<Room>> AddRange(IEnumerable<Room> books)
        {
            await _testProjectContext.Rooms.AddRangeAsync(books);
            await _testProjectContext.SaveChangesAsync();
            return books;
        }

        public async Task<Room> Update(Room room)
        {
            var bookForChanges = await _testProjectContext.Rooms.SingleAsync(x => x.Id == room.Id);
            bookForChanges.IsAvailable = room.IsAvailable;
            bookForChanges.Category = room.Category;
            bookForChanges.Floor = room.Floor;
            bookForChanges.Number = room.Number;

            _testProjectContext.Rooms.Update(bookForChanges);
            await _testProjectContext.SaveChangesAsync();
            return room;
        }

        public async Task<bool> Delete(Room room)
        {
            _testProjectContext.Rooms.Remove(room);
            await _testProjectContext.SaveChangesAsync();

            return true;
        }
    }

    public interface IRoomsService
    {
        Task<IEnumerable<Room>> Get(int[] ids, Filters filters);

        Task<Room> Add(Room room);

        Task<IEnumerable<Room>> AddRange(IEnumerable<Room> books);

        Task<Room> Update(Room room);

        Task<bool> Delete(Room room);
    }

    public class Filters
    {
        public string[] Categories { get; set; }
        public int[] Floors { get; set; }
    }
}

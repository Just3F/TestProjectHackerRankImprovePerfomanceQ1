using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TestProject.WebAPI.Data;

namespace TestProject.WebAPI.Services
{
    public class NewsFeedService : INewsFeedService
    {
        private readonly TestProjectContext _testProjectContext;

        public NewsFeedService(TestProjectContext testProjectContext)
        {
            _testProjectContext = testProjectContext;
        }

        public async Task<IEnumerable<NewsFeedItem>> Get(int[] ids, Filters filters)
        {
            var newsItems = _testProjectContext.NewsFeedItems.AsQueryable();

            if (filters == null)
                filters = new Filters();

            if (filters.Body != null && filters.Body.Any())
                newsItems = newsItems.Where(x => filters.Body.Contains(x.Body));

            if (filters.AuthorNames != null && filters.AuthorNames.Any())
                newsItems = newsItems.Where(x => filters.AuthorNames.Contains(x.AuthorName));

            if (filters.Title != null && filters.Title.Any())
                newsItems = newsItems.Where(x => filters.Title.Contains(x.Title));

            if (ids != null && ids.Any())
                newsItems = newsItems.Where(x => ids.Contains(x.Id));

            await Task.Delay(2000);

            return await newsItems.ToListAsync();
        }

        public async Task<NewsFeedItem> Add(NewsFeedItem newsFeedItem)
        {
            await _testProjectContext.NewsFeedItems.AddAsync(newsFeedItem);
            newsFeedItem.DateCreated = DateTime.UtcNow;

            await _testProjectContext.SaveChangesAsync();
            return newsFeedItem;
        }

        public async Task<IEnumerable<NewsFeedItem>> AddRange(IEnumerable<NewsFeedItem> newsItems)
        {
            await _testProjectContext.NewsFeedItems.AddRangeAsync(newsItems);
            await _testProjectContext.SaveChangesAsync();
            return newsItems;
        }

        public async Task<NewsFeedItem> Update(NewsFeedItem newsFeedItem)
        {
            var newsItemForChanges = await _testProjectContext.NewsFeedItems.SingleAsync(x => x.Id == newsFeedItem.Id);
            newsItemForChanges.Body = newsFeedItem.Body;
            newsItemForChanges.Title = newsFeedItem.Title;
            newsItemForChanges.AllowComments = newsFeedItem.AllowComments;

            _testProjectContext.NewsFeedItems.Update(newsItemForChanges);
            await _testProjectContext.SaveChangesAsync();
            return newsFeedItem;
        }

        public async Task<bool> Delete(NewsFeedItem newsFeedItem)
        {
            _testProjectContext.NewsFeedItems.Remove(newsFeedItem);
            await _testProjectContext.SaveChangesAsync();

            return true;
        }
    }

    public interface INewsFeedService
    {
        Task<IEnumerable<NewsFeedItem>> Get(int[] ids, Filters filters);

        Task<NewsFeedItem> Add(NewsFeedItem newsFeedItem);

        Task<IEnumerable<NewsFeedItem>> AddRange(IEnumerable<NewsFeedItem> newsItems);

        Task<NewsFeedItem> Update(NewsFeedItem newsFeedItem);

        Task<bool> Delete(NewsFeedItem newsFeedItem);
    }

    public class Filters
    {
        public string[] Body { get; set; }
        public string[] AuthorNames { get; set; }
        public string[] Title { get; set; }
    }
}

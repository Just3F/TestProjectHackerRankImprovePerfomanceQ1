using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TestProject.WebAPI.Data;

namespace TestProject.WebAPI.Services
{
    public class DocumentsService : IDocumentsService
    {
        private readonly TestProjectContext _testProjectContext;

        public DocumentsService(TestProjectContext testProjectContext)
        {
            _testProjectContext = testProjectContext;
        }

        public async Task<IEnumerable<Document>> Get(int[] ids)
        {
            var projects = _testProjectContext.Documents.AsQueryable();

            if (ids != null && ids.Any())
                projects = projects.Where(x => ids.Contains(x.Id));

            return await projects.ToListAsync();
        }

        public async Task<Document> Add(Document document)
        {
            await _testProjectContext.Documents.AddAsync(document);

            await _testProjectContext.SaveChangesAsync();
            return document;
        }

        public async Task<IEnumerable<Document>> AddRange(IEnumerable<Document> projects)
        {
            await _testProjectContext.Documents.AddRangeAsync(projects);
            await _testProjectContext.SaveChangesAsync();
            return projects;
        }

        public async Task<Document> Update(Document document)
        {
            var projectForChanges = await _testProjectContext.Documents.SingleAsync(x => x.Id == document.Id);
            projectForChanges.Name = document.Name;
            projectForChanges.Body = document.Body;

            _testProjectContext.Documents.Update(projectForChanges);
            await _testProjectContext.SaveChangesAsync();
            return document;
        }

        public async Task<bool> Delete(Document document)
        {
            _testProjectContext.Documents.Remove(document);
            await _testProjectContext.SaveChangesAsync();

            return true;
        }
    }

    public interface IDocumentsService
    {
        Task<IEnumerable<Document>> Get(int[] ids);

        Task<Document> Add(Document document);

        Task<Document> Update(Document document);

        Task<bool> Delete(Document document);
    }
}

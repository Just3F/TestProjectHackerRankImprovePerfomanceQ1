using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TestProject.WebAPI.Data;

namespace TestProject.WebAPI.Services
{
    public class LibrariesService : ILibrariesService
    {
        private readonly TestProjectContext _testProjectContext;

        public LibrariesService(TestProjectContext testProjectContext)
        {
            _testProjectContext = testProjectContext;
        }

        public async Task<IEnumerable<Library>> Get(int[] ids)
        {
            var projects = _testProjectContext.Libraries.AsQueryable();

            if (ids != null && ids.Any())
                projects = projects.Where(x => ids.Contains(x.Id));

            return await projects.ToListAsync();
        }

        public async Task<Library> Add(Library library)
        {
            await _testProjectContext.Libraries.AddAsync(library);

            await _testProjectContext.SaveChangesAsync();
            return library;
        }

        public async Task<IEnumerable<Library>> AddRange(IEnumerable<Library> projects)
        {
            await _testProjectContext.Libraries.AddRangeAsync(projects);
            await _testProjectContext.SaveChangesAsync();
            return projects;
        }

        public async Task<Library> Update(Library library)
        {
            var projectForChanges = await _testProjectContext.Libraries.SingleAsync(x => x.Id == library.Id);
            projectForChanges.Name = library.Name;
            projectForChanges.Location = library.Location;

            _testProjectContext.Libraries.Update(projectForChanges);
            await _testProjectContext.SaveChangesAsync();
            return library;
        }

        public async Task<bool> Delete(Library library)
        {
            _testProjectContext.Libraries.Remove(library);
            await _testProjectContext.SaveChangesAsync();

            return true;
        }
    }

    public interface ILibrariesService
    {
        Task<IEnumerable<Library>> Get(int[] ids);

        Task<Library> Add(Library library);

        Task<Library> Update(Library library);

        Task<bool> Delete(Library library);
    }
}

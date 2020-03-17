using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TestProject.WebAPI.Data;

namespace TestProject.WebAPI.Services
{
    public class CompaniesService : ICompaniesService
    {
        private readonly TestProjectContext _testProjectContext;

        public CompaniesService(TestProjectContext testProjectContext)
        {
            _testProjectContext = testProjectContext;
        }

        public async Task<IEnumerable<Company>> Get(int[] ids)
        {
            var projects = _testProjectContext.Companies.AsQueryable();

            if (ids != null && ids.Any())
                projects = projects.Where(x => ids.Contains(x.Id));

            return await projects.ToListAsync();
        }

        public async Task<Company> Add(Company company)
        {
            await _testProjectContext.Companies.AddAsync(company);

            await _testProjectContext.SaveChangesAsync();
            return company;
        }

        public async Task<IEnumerable<Company>> AddRange(IEnumerable<Company> projects)
        {
            await _testProjectContext.Companies.AddRangeAsync(projects);
            await _testProjectContext.SaveChangesAsync();
            return projects;
        }

        public async Task<Company> Update(Company company)
        {
            var projectForChanges = await _testProjectContext.Companies.SingleAsync(x => x.Id == company.Id);
            projectForChanges.Name = company.Name;
            projectForChanges.Location = company.Location;

            _testProjectContext.Companies.Update(projectForChanges);
            await _testProjectContext.SaveChangesAsync();
            return company;
        }

        public async Task<bool> Delete(Company company)
        {
            _testProjectContext.Companies.Remove(company);
            await _testProjectContext.SaveChangesAsync();

            return true;
        }
    }

    public interface ICompaniesService
    {
        Task<IEnumerable<Company>> Get(int[] ids);

        Task<Company> Add(Company company);

        Task<Company> Update(Company company);

        Task<bool> Delete(Company company);
    }
}

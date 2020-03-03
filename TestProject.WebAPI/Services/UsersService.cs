using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TestProject.WebAPI.Data;

namespace TestProject.WebAPI.Services
{
    public class ReportsService : IReportsService
    {
        private readonly TestProjectContext _testProjectContext;

        public ReportsService(TestProjectContext testProjectContext)
        {
            _testProjectContext = testProjectContext;
        }

        public async Task<IEnumerable<Report>> Get(int documentId, int[] ids)
        {
            var users = _testProjectContext.Reports.Where(x => x.DocumentId == documentId).AsQueryable();

            if (ids != null && ids.Any())
                users = users.Where(x => ids.Contains(x.Id));

            return await users.ToListAsync();
        }

        public async Task<Report> Add(Report report)
        {
            await _testProjectContext.Reports.AddAsync(report);

            await _testProjectContext.SaveChangesAsync();
            return report;
        }

        public async Task<Report> Update(Report report)
        {
            var userForChanges = await _testProjectContext.Reports.SingleAsync(x => x.Id == report.Id);

            userForChanges.Name = report.Name;

            _testProjectContext.Reports.Update(userForChanges);
            await _testProjectContext.SaveChangesAsync();
            return report;
        }

        public async Task<bool> Delete(Report report)
        {
            _testProjectContext.Reports.Remove(report);
            await _testProjectContext.SaveChangesAsync();

            return true;
        }
    }

    public interface IReportsService
    {
        Task<IEnumerable<Report>> Get(int documentId, int[] ids);

        Task<Report> Add(Report report);

        Task<Report> Update(Report report);

        Task<bool> Delete(Report report);
    }
}

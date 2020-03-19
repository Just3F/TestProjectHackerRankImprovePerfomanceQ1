using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using TestProject.WebAPI.Data;

namespace TestProject.WebAPI.Services
{
    public class ReportsService : IReportsService
    {
        private readonly TestProjectContext _testProjectContext;
        private readonly IStringLocalizer _localizer;

        public static string[] Rows = {"Header1", "Header2", "Header3"};

        public ReportsService(TestProjectContext testProjectContext, IStringLocalizer localizer)
        {
            _testProjectContext = testProjectContext;
            _localizer = localizer;
        }

        public async Task<IEnumerable<Report>> Get(int[] ids)
        {
            var reports = _testProjectContext.Reports.AsQueryable();

            if (ids != null && ids.Any())
                reports = reports.Where(x => ids.Contains(x.Id));
            var result = await reports.ToListAsync();

            var translatedRows = Rows.Select(x => _localizer[x].Value).ToList();
            result.ForEach(x=>x.Rows = translatedRows);

            return result;
        }

        public async Task<Report> Add(Report report)
        {
            await _testProjectContext.Reports.AddAsync(report);

            await _testProjectContext.SaveChangesAsync();
            return report;
        }

        public async Task<Report> Update(Report report)
        {
            var reportForChange = await _testProjectContext.Reports.SingleAsync(x => x.Id == report.Id);

            reportForChange.Name = report.Name;

            _testProjectContext.Reports.Update(reportForChange);
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
        Task<IEnumerable<Report>> Get(int[] ids);

        Task<Report> Add(Report report);

        Task<Report> Update(Report report);

        Task<bool> Delete(Report report);
    }
}

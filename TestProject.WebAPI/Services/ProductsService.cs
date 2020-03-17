using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TestProject.WebAPI.Data;

namespace TestProject.WebAPI.Services
{
    public class ProductsService : IProductsService
    {
        private readonly TestProjectContext _testProjectContext;

        public ProductsService(TestProjectContext testProjectContext)
        {
            _testProjectContext = testProjectContext;
        }

        public async Task<IEnumerable<Product>> Get(int companyId, int[] ids)
        {
            var users = _testProjectContext.Products.Where(x => x.CompanyId == companyId).AsQueryable();

            if (ids != null && ids.Any())
                users = users.Where(x => ids.Contains(x.Id));

            return await users.ToListAsync();
        }

        public async Task<Product> Add(Product product)
        {
            await _testProjectContext.Products.AddAsync(product);

            await _testProjectContext.SaveChangesAsync();
            return product;
        }

        public async Task<Product> Update(Product product)
        {
            var userForChanges = await _testProjectContext.Products.SingleAsync(x => x.Id == product.Id);

            userForChanges.Name = product.Name;

            _testProjectContext.Products.Update(userForChanges);
            await _testProjectContext.SaveChangesAsync();
            return product;
        }

        public async Task<bool> Delete(Product product)
        {
            _testProjectContext.Products.Remove(product);
            await _testProjectContext.SaveChangesAsync();

            return true;
        }
    }

    public interface IProductsService
    {
        Task<IEnumerable<Product>> Get(int companyId, int[] ids);

        Task<Product> Add(Product product);

        Task<Product> Update(Product product);

        Task<bool> Delete(Product product);
    }
}

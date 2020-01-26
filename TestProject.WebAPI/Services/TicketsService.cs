using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TestProject.WebAPI.Data;

namespace TestProject.WebAPI.Services
{
    public class CarsService : ICarsService
    {
        private readonly TestProjectContext _testProjectContext;

        public CarsService(TestProjectContext testProjectContext)
        {
            _testProjectContext = testProjectContext;
        }

        public async Task<IEnumerable<Car>> Get(int[] ids, Filters filters)
        {
            var cars = _testProjectContext.Cars.AsQueryable();

            if (filters == null)
                filters = new Filters();

            if (filters.Years != null && filters.Years.Any())
                cars = cars.Where(x => filters.Years.Contains(x.Year));

            if (filters.Makes != null && filters.Makes.Any())
                cars = cars.Where(x => filters.Makes.Contains(x.Make));

            if (filters.Models != null && filters.Models.Any())
                cars = cars.Where(x => filters.Models.Contains(x.Model));

            if (ids != null && ids.Any())
                cars = cars.Where(x => ids.Contains(x.Id));

            await Task.Delay(4000);

            return await cars.ToListAsync();
        }

        public async Task<Car> Add(Car car)
        {
            await _testProjectContext.Cars.AddAsync(car);
            await _testProjectContext.SaveChangesAsync();
            return car;
        }

        public async Task<IEnumerable<Car>> AddRange(IEnumerable<Car> cars)
        {
            await _testProjectContext.Cars.AddRangeAsync(cars);
            await _testProjectContext.SaveChangesAsync();
            return cars;
        }

        public async Task<Car> Update(Car car)
        {
            var userForChanges = await _testProjectContext.Cars.SingleAsync(x => x.Id == car.Id);
            userForChanges.Price = car.Price;
            userForChanges.Year = car.Year;
            userForChanges.Make = car.Make;
            userForChanges.Model = car.Model;

            _testProjectContext.Cars.Update(userForChanges);
            await _testProjectContext.SaveChangesAsync();
            return car;
        }

        public async Task<bool> Delete(Car car)
        {
            _testProjectContext.Cars.Remove(car);
            await _testProjectContext.SaveChangesAsync();

            return true;
        }
    }

    public interface ICarsService
    {
        Task<IEnumerable<Car>> Get(int[] ids, Filters filters);

        Task<Car> Add(Car car);

        Task<IEnumerable<Car>> AddRange(IEnumerable<Car> cars);

        Task<Car> Update(Car car);

        Task<bool> Delete(Car car);
    }

    public class Filters
    {
        public uint[] Years { get; set; }
        public string[] Makes { get; set; }
        public string[] Models { get; set; }
    }
}

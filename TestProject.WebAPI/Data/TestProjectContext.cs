using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace TestProject.WebAPI.Data
{
    public class TestProjectContext : DbContext
    {
        public TestProjectContext(DbContextOptions<TestProjectContext> options)
            : base(options)
        { }

        public DbSet<Company> Companies { get; set; }
        public DbSet<Product> Products { get; set; }
    }

    public class Product
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        public string Category { get; set; }

        public int CompanyId { get; set; }
        public virtual Company Company { get; set; }
    }

    public class Company
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        public string Location { get; set; }
    }
}

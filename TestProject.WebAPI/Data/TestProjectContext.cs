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

        public DbSet<Ticket> Tickets { get; set; }
    }

    public class Ticket
    {
        [Key]
        public int Id { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public int Count { get; set; }

        public DateTime EventDate { get; set; }
    }
}

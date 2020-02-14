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

        public DbSet<Room> Rooms { get; set; }
    }

    public class Room
    {
        [Key]
        public int Id { get; set; }

        public string Category { get; set; }

        public int Number { get; set; }

        public int Floor { get; set; }

        public bool IsAvailable { get; set; }

        public DateTime AddedDate { get; set; }
    }
}

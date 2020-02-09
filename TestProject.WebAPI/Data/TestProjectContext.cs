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

        public DbSet<Song> Songs { get; set; }
        public DbSet<Singer> Singers { get; set; }
    }

    public class Song
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        public Singer Singer { get; set; }

        public DateTime ReleaseDate { get; set; }
    }

    public class Singer
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }
    }
}

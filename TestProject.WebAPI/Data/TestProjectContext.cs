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

        public DbSet<Book> Books { get; set; }
    }

    public class Book
    {
        [Key]
        public int Id { get; set; }

        public string Title { get; set; }

        public string AuthorName { get; set; }

        public string Body { get; set; }

        public DateTime PublishedDate { get; set; }
    }
}

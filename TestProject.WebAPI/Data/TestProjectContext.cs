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

        public DbSet<Document> Documents { get; set; }
        public DbSet<Report> Reports { get; set; }
    }

    public class Report
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        public string Category { get; set; }

        public int DocumentId { get; set; }
        public virtual Document Document { get; set; }
    }

    public class Document
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        public string Body { get; set; }
    }
}

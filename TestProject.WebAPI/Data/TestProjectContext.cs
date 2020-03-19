using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TestProject.WebAPI.Data
{
    public class TestProjectContext : DbContext
    {
        public TestProjectContext(DbContextOptions<TestProjectContext> options)
            : base(options)
        { }

        public DbSet<Report> Reports { get; set; }
    }

    public class Report
    {
        public Report()
        {
            Rows = new List<string>();
        }

        [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        public string Category { get; set; }

        [NotMapped]
        public List<string> Rows { get; set; }

    }
}

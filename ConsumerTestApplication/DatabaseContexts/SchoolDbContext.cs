using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ConsumerTestApplication.DomainModel;

namespace ConsumerTestApplication.DatabaseContexts
{
	public class SchoolDbContext : DbContext
	{
// Map our 'User' model by convention
		public DbSet<Student> Students{ get; set; }

		public SchoolDbContext() : base("Server=localhost;Database=DbContextScopeDemo;Trusted_Connection=true;")
		{}

		protected override void OnModelCreating(DbModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			// Overrides for the convention-based mappings.
			// We're assuming that all our fluent mappings are declared in this assembly.
			modelBuilder.Configurations.AddFromAssembly(Assembly.GetAssembly(typeof(SchoolDbContext)));
		}
	}
}

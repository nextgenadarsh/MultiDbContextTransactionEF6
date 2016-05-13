using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsumerTestApplication.DomainModel;

namespace ConsumerTestApplication.DatabaseContexts
{
	/// <summary>
	/// Defines the convention-based mapping overrides for the Student model. 
	/// </summary>
	public class StudentFluentMap : EntityTypeConfiguration<Student>
	{
		public StudentFluentMap()
		{
			Property(m => m.Name).IsRequired();
			Property(m => m.Email).IsRequired();
		}
	}
}

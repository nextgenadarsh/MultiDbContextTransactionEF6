using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsumerTestApplication.CommandModal;
using ConsumerTestApplication.DomainModel;
using ConsumerTestApplication.Repositories;
using MultiDbContextTransactionEF6.Core.Interface;

namespace ConsumerTestApplication.BusinessLogic
{
	/*
	 * Example business logic service implementing command functionalities (i.e. create / update actions).
	 */
	public class StudentCreationManager
	{
		private readonly IDbContextScopeFactory _dbContextScopeFactory;
		private readonly IRepository<Student> _studentRepository;

		public StudentCreationManager(IDbContextScopeFactory dbContextScopeFactory, IRepository<Student> studentRepository)
		{
			if (dbContextScopeFactory == null) throw new ArgumentNullException("dbContextScopeFactory");
			if (studentRepository == null) throw new ArgumentNullException("studentRepository");
			_dbContextScopeFactory = dbContextScopeFactory;
			_studentRepository = studentRepository;
		}

		public void CreateStudent(StudentCreationSpec studentToCreate)
		{
			if (studentToCreate == null)
				throw new ArgumentNullException("studentToCreate");

			studentToCreate.Validate();

			/*
			 * Typical usage of DbContextScope for a read-write business transaction. 
			 * It's as simple as it looks.
			 */
			using (var dbContextScope = _dbContextScopeFactory.Create())
			{
				//-- Build domain model
				var student = new Student()
				           {
							   Id = studentToCreate.Id,
							   Name = studentToCreate.Name,
							   Email = studentToCreate.Email,
							   WelcomeEmailSent = false,
					           CreatedOn = DateTime.UtcNow
				           };

				//-- Persist
				_studentRepository.Add(student);
				dbContextScope.SaveChanges();
			}
		}

		public void CreateListOfStudents(params StudentCreationSpec[] studentsToCreate)
		{
			/*
			 * Example of DbContextScope nesting in action. 
			 * 
			 * We already have a service method - CreateStudent() - that knows how to create a new student
			 * and implements all the business rules around the creation of a new student 
			 * (e.g. validation, initialization, sending notifications to other domain model objects...).
			 * 
			 * So we'll just call it in a loop to create the list of new students we've 
			 * been asked to create.
			 * 
			 * Of course, since this is a business logic service method, we are making 
			 * an implicit guarantee to whoever is calling us that the changes we make to 
			 * the system will be either committed or rolled-back in an atomic manner. 
			 * I.e. either all the students we've been asked to create will get persisted
			 * or none of them will. It would be disastrous to have a partial failure here
			 * and end up with some students but not all having been created.
			 * 
			 * DbContextScope makes this trivial to implement. 
			 * 
			 * The inner DbContextScope instance that the CreateStudent() method creates
			 * will join our top-level scope. This ensures that the same DbContext instance is
			 * going to be used throughout this business transaction.
			 * 
			 */

			using (var dbContextScope = _dbContextScopeFactory.Create())
			{
				foreach (var toCreate in studentsToCreate)
				{
					CreateStudent(toCreate);
				}

				// All the changes will get persisted here
				dbContextScope.SaveChanges();
			}
		}

		public void CreateListOfStudentsWithIntentionalFailure(params StudentCreationSpec[] studentsToCreate)
		{
			/*
			 * Here, we'll verify that inner DbContextScopes really join the parent scope and 
			 * don't persist their changes until the parent scope completes successfully. 
			 */

			var firstStudent = true;

			using (var dbContextScope = _dbContextScopeFactory.Create())
			{
				foreach (var toCreate in studentsToCreate)
				{
					if (firstStudent)
					{
						CreateStudent(toCreate);
						Console.WriteLine("Successfully created a new Student named '{0}'.", toCreate.Name);
						firstStudent = false;
					}
					else
					{
						// OK. So we've successfully persisted one student.
						// We're going to simulate a failure when attempting to 
						// persist the second student and see what ends up getting 
						// persisted in the DB.
						throw new Exception(String.Format("Oh no! An error occurred when attempting to create student named '{0}' in our database.", toCreate.Name));
					}
				}

				dbContextScope.SaveChanges();
			}
		}


	}
}

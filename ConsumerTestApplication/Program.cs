using System;
using System.Linq;
using ConsumerTestApplication.BusinessLogic;
using ConsumerTestApplication.CommandModal;
using ConsumerTestApplication.DatabaseContexts;
using MultiDbContextTransactionEF6.Core.Implementation;

namespace ConsumerTestApplication
{
	class Program
	{
		static void Main(string[] args)
		{
			//-- Poor-man DI - build our dependencies by hand for this demo
			var dbContextScopeFactory = new DbContextScopeFactory();
			var dbContextLocator = new DbContextLocator();
			var studentRepository = new StudentRepository(dbContextLocator);

			var studentCreationService = new StudentCreationManager(dbContextScopeFactory, studentRepository);
			var studentQueryService = new StudentQueryManager(dbContextScopeFactory, studentRepository);
			var studentEmailService = new StudentEmailManager(dbContextScopeFactory);
			var studentCreditScoreService = new StudentScoreManager(dbContextScopeFactory);

			try
			{
				Console.WriteLine("This demo application will create a database named DbContextScopeDemo in the default SQL Server instance on localhost. Edit the connection string in StudentManagementDbContext if you'd like to create it somewhere else.");
				Console.WriteLine("Press enter to start...");
				Console.ReadLine();

				//-- Demo of typical usage for read and writes
				Console.WriteLine("Creating a student called Mary...");
				var marysSpec = new StudentCreationSpec("Mary", "mary@example.com");
				studentCreationService.CreateStudent(marysSpec);
				Console.WriteLine("Done.\n");

				Console.WriteLine("Trying to retrieve our newly created student from the data store...");
				var mary = studentQueryService.GetStudent(marysSpec.Id);
				Console.WriteLine("OK. Persisted student: {0}", mary);

				Console.WriteLine("Press enter to continue...");
				Console.ReadLine();

				//-- Demo of nested DbContextScopes
				Console.WriteLine("Creating 2 new students called John and Jeanne in an atomic transaction...");
				var johnSpec = new StudentCreationSpec("John", "john@example.com");
				var jeanneSpec = new StudentCreationSpec("Jeanne", "jeanne@example.com");
				studentCreationService.CreateListOfStudents(johnSpec, jeanneSpec);
				Console.WriteLine("Done.\n");

				Console.WriteLine("Trying to retrieve our newly created students from the data store...");
				var createdStudents = studentQueryService.GetStudents(johnSpec.Id, jeanneSpec.Id);
				Console.WriteLine("OK. Found {0} persisted students.", createdStudents.Count());

				Console.WriteLine("Press enter to continue...");
				Console.ReadLine();

				//-- Demo of nested DbContextScopes in the face of an exception. 
				// If any of the provided students failed to get persisted, none should get persisted. 
				Console.WriteLine("Creating 2 new students called Julie and Marc in an atomic transaction. Will make the persistence of the second student fail intentionally in order to test the atomicity of the transaction...");
				var julieSpec = new StudentCreationSpec("Julie", "julie@example.com");
				var marcSpec = new StudentCreationSpec("Marc", "marc@example.com");
				try
				{
					studentCreationService.CreateListOfStudentsWithIntentionalFailure(julieSpec, marcSpec);
					Console.WriteLine("Done.\n");
				}
				catch (Exception e)
				{
					Console.WriteLine(e.Message);
					Console.WriteLine();
				}

				Console.WriteLine("Trying to retrieve our newly created students from the data store...");
				var maybeCreatedStudents = studentQueryService.GetStudents(julieSpec.Id, marcSpec.Id);
				Console.WriteLine("Found {0} persisted students. If this number is 0, we're all good. If this number is not 0, we have a big problem.", maybeCreatedStudents.Count());

				Console.WriteLine("Press enter to continue...");
				Console.ReadLine();

				//-- Demo of DbContextScope within an async flow
				Console.WriteLine("Trying to retrieve two students John and Jeanne sequentially in an asynchronous manner...");
				// We're going to block on the async task here as we don't have a choice. No risk of deadlocking in any case as console apps
				// don't have a synchronization context.
				var studentsFoundAsync = studentQueryService.GetTwoStudentsAsync(johnSpec.Id, jeanneSpec.Id).Result;
				Console.WriteLine("OK. Found {0} persisted students.", studentsFoundAsync.Count());

				Console.WriteLine("Press enter to continue...");
				Console.ReadLine();

				//-- Demo of explicit database transaction. 
				Console.WriteLine("Trying to retrieve student John within a READ UNCOMMITTED database transaction...");
				// You'll want to use SQL Profiler or Entity Framework Profiler to verify that the correct transaction isolation
				// level is being used.
				var studentMaybeUncommitted = studentQueryService.GetStudentUncommitted(johnSpec.Id);
				Console.WriteLine("OK. Student found: {0}", studentMaybeUncommitted);

				Console.WriteLine("Press enter to continue...");
				Console.ReadLine();

				//-- Demo of disabling the DbContextScope nesting behaviour in order to force the persistence of changes made to entities
				// This is a pretty advanced feature that you can safely ignore until you actually need it.
				Console.WriteLine("Will simulate sending a Welcome email to John...");

				using (var parentScope = dbContextScopeFactory.Create())
				{
					var parentDbContext = parentScope.DbContexts.Get<SchoolDbContext>();

					// Load John in the parent DbContext
					var john = parentDbContext.Students.Find(johnSpec.Id);
					Console.WriteLine("Before calling SendWelcomeEmail(), john.WelcomeEmailSent = " + john.WelcomeEmailSent);

					// Now call our SendWelcomeEmail() business logic service method, which will
					// update John in a non-nested child context
					studentEmailService.SendWelcomeEmail(johnSpec.Id);

					// Verify that we can see the modifications made to John by the SendWelcomeEmail() method
					Console.WriteLine("After calling SendWelcomeEmail(), john.WelcomeEmailSent = " + john.WelcomeEmailSent);

					// Note that even though we're not calling SaveChanges() in the parent scope here, the changes
					// made to John by SendWelcomeEmail() will remain persisted in the database as SendWelcomeEmail()
					// forced the creation of a new DbContextScope.
				}

				Console.WriteLine("Press enter to continue...");
				Console.ReadLine();

				//-- Demonstration of DbContextScope and parallel programming
				Console.WriteLine("Calculating and storing the credit score of all students in the database in parallel...");
				studentCreditScoreService.UpdateCreditScoreForAllStudents();
				Console.WriteLine("Done.");
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}

			Console.WriteLine();
			Console.WriteLine("The end.");
			Console.WriteLine("Press enter to exit...");
			Console.ReadLine();
		}
	}
}

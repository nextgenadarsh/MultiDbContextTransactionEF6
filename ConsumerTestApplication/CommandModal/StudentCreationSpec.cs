using System;

namespace ConsumerTestApplication.CommandModal
{

	/// <summary>
	/// Specifications of the CreateStudent command. Defines the properties of a new student.
	/// </summary>
	public class StudentCreationSpec
	{
		/// <summary>
		/// The Id automatically generated for this student.
		/// </summary>
		public Guid Id { get; protected set; }

		public string Name { get; protected set; }
		public string Email { get; protected set; }

		public StudentCreationSpec(string name, string email)
		{
			Id = Guid.NewGuid();
			Name = name;
			Email = email;
		}

		public void Validate()
		{
			// [...]
		}
	}
}

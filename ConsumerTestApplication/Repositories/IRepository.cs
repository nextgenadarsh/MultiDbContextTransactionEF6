using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsumerTestApplication.Repositories
{
	public interface IRepository<T>
	{
		T Get(Guid entityId);
		Task<T> GetAsync(Guid entityId);
		void Add(T entity);
	}
}

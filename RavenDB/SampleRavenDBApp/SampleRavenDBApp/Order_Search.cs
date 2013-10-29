using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Raven.Client.Indexes;

namespace SampleRavenDBApp
{
	//tag name for collection ==>orders
	class Order_Search : AbstractIndexCreationTask<Orders.Order>
	{
		public Order_Search()
		{
			this.Map = docs =>
				from doc in docs
				let emp = LoadDocument<Orders.Employee>(doc.Employee)
				select new
				{
					doc.Freight,
					Employee = emp.FirstName + " " + emp.LastName,
					EmployeeLastName = emp.LastName
				};
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Orders;
using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Indexes;
using Raven.Imports.Newtonsoft.Json;

namespace SampleRavenDBApp
{
	class Program
	{
		static void Main(string[] args)
		{
			var store = new DocumentStore()
			{
				Url = "http://localhost:8080",
				DefaultDatabase = "Northwind"
			};
			store.Initialize();
			//get all classes from currently executing assembly that derive from a specific superclass and create the corresponding indexes
			IndexCreation.CreateIndexes(Assembly.GetExecutingAssembly(), store);


			using (var session = store.OpenSession())
			{
				var query = session.Query<dynamic>().Where(d => d.Name);
			}
			using (var session = store.OpenSession())
			{
								//s:pkey
				//ravenjobject: json object
				//mtd: raven metedata
				//this method executed only for employees
				//deserialization type for orders is forced in include
				//perform this at the application root
				var original = store.Conventions.FindClrType;
				store.Conventions.FindClrType = (s, obj, mtd) =>
				{
					if (mtd.Value<string>("Raven-Clr-Type") == "Orders.Employee, Northwind")
					{
						return typeof(Employee).AssemblyQualifiedName;
					}
					return original(s, obj, mtd);
				};

				


				var query =
					session.Query<Employee>()
					.Where(e => e.FirstName.StartsWith("A"));

				//var p = session.Load<Product>("Product/1");
				//p.Description = "descr";
				//session.SaveChanges();

				//if id not initialised ===> generated on call to store
				//if id preinitialised ==> actual value generated AFTER data has been saved to db

				//customise id generation
				//s:key, commands:access db within current transaction
				//var tag=store.Conventions.FindTypeTagName(type).ToCamelCase();
				//tag+"/"+stuff+"/"+keygenerator.NextId()

				//store.Conventions.RegisterIdconvention<Interface>((s,commands,obj)=>{ }

				//prevents property initialisation
				//prevents constructor execution
				//used by Json.NET if no parameterless constructor found!
				//var obj = FormatterServices.GetUninitializedObject(typeof(Product));
				//var obj2 = FormatterServices.GetUninitializedObject(typeof(IFoo));
				var query2 = session.Query<Order>()
					.Where(o => o.Freight > 2)
					.Take(10);//no limit?==> 128
				//max limit=1024
				foreach (var order in query2)
				{
					Console.WriteLine(order.Employee);

				}
				Console.WriteLine("*");




				var query3 = session.Query<Order>()
				.Customize(c =>
				{
					//ducktyping example: server receives string:)
					//load 100% employee data
					c.Include<Order>(obj => obj.Employee);//same as c.Include("Employee") but better

				})
				.Where(o => o.Freight > 2)
					//.Where(o=>o.Freight<5)//are or'ed
					//.Where(o => o.Freight > 2 && o.Freight<10)//to and conditions :)
				.Take(10)
				.Select(o => new
				{//returns just selected attributes to client :)
					Employee = o.Employee,
					Freight = o.Freight
				})
				.OfType<Order>();//just docs with raved type =Orders
				;

				session.Advanced.LuceneQuery<Order>()
					.Where("Freight: [2 to 10]")
					//.SelectFields<Order>("Employee", "Freight")//not good, resto of order properties are null
					.SelectFields<OrderView>("Employee", "Freight")//good
					;
				//.WhereEquals("","")//expressions are or'ed
				//foreach (var order in query3)//bombs but why?
				//{
				//	var employee = session.Load<dynamic>(order.Employee);
				//	//no roundtrip because employee data has already been loaded by query statement
				//	Console.WriteLine(order.Employee);

				//}
				RavenQueryStatistics stats = null;
				var query4 = session.Query<Employee>()
					.Statistics(out stats)//gimme some statistics on my query!
					.Where(e => e.LastName.StartsWith("F"))
					.OrderBy(e => e.FirstName)
					.ThenBy(e => e.LastName);//if sort by freight then generates new index! (freight is decimal therefore not string and therefore does not exist an idex that supports it)
				//.Take(10);
				//copies index into memory and locks it, prevents any modification to index 
				var enumerable = session.Advanced.Stream(query4);

				//while (enumerable.MoveNext())
				//{
				//	var emp.Enumerable.Current();//server-side forward-only cursor!
				//}
				int u = 0;




			}
		}
	}
	public interface IFoo { }
	class OrderView
	{
		public string Employee { get; set; }
		public decimal Freight { get; set; }
	}
	public class Product
	{
		public Product()
		{

		}
		//[JsonProperty(PropertyName = "bla")]
		public string Id { get; private set; }
		public string Description { get; set; }
		public string Name { get; set; }
		public decimal Price { get; set; }
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Raven.Client.Document;
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
			using (var session = store.OpenSession())
			{
				var p = session.Load<Product>("Product/1");
				p.Description = "descr";
				session.SaveChanges();

				//prevents property initialisation
				//prevents constructor execution
				//used by Json.NET if no parameterless constructor found!
				//var obj = FormatterServices.GetUninitializedObject(typeof(Product));
				//var obj2 = FormatterServices.GetUninitializedObject(typeof(IFoo));

			}
		}
	}
	public interface IFoo { }

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

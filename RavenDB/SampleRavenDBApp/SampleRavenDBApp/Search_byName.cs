using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Raven.Client.Indexes;

namespace SampleRavenDBApp
{
	class Search_byName : AbstractMultiMapIndexCreationTask
	{
		public class MultiMapSearch
		{
			public string Name { get; set; }
			public string Type { get; set; }
		}
		public Search_byName()
		{
			this.AddMap<Orders.Company>(docs => from doc in docs
												select new MultiMapSearch
												{
													Name = doc.Name,
													Type = MetadataFor(doc).Value<string>("Raven-Clr-Type")
												});
			this.AddMap<Orders.Employee>(docs => from doc in docs
												 select new MultiMapSearch
												 {
													 Name = doc.FirstName + " " + doc.LastName,
													 Type = MetadataFor(doc).Value<string>("Raven-Clr-Type")
												 });
			this.AddMap<Orders.Supplier>(docs => from doc in docs
												 select new MultiMapSearch
												 {
													 Name = doc.Name,
													 Type = MetadataFor(doc).Value<string>("Raven-Clr-Type")
												 });

			//this.Store<MultiMapSearch>(
			//persist all fields contained in this search
			this.StoreAllFields(Raven.Abstractions.Indexing.FieldStorage.Yes);

		}
	}
}

using System;

namespace Argon.Movie.Indexer.MovieIndexer.Attributes
{


	[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
	public sealed class MoviesIndexerAttribute : Attribute
	{
		public string Name { get; set; }

		public string BaseUrl { get; set; }
		
		public MoviesIndexerAttribute(string name, string baseUrl)
		{
			Name = name;
			BaseUrl = baseUrl;
		}
	}
}

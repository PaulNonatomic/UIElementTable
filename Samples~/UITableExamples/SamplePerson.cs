using System.Collections.Generic;

namespace Nonatomic.UIElements.Examples
{
	/// <summary>
	/// Plain demo model used by the UITable examples and validation harness.
	/// Kept outside any Editor folder so a runtime sample scene can reuse it.
	/// </summary>
	public class SamplePerson
	{
		public string Name;
		public int Age;
		public string Country;

		public SamplePerson(string name, int age, string country)
		{
			Name = name;
			Age = age;
			Country = country;
		}

		public static List<SamplePerson> SetA() => new List<SamplePerson>
		{
			new SamplePerson("Alice", 30, "USA"),
			new SamplePerson("Bob", 25, "Canada"),
			new SamplePerson("Charlie", 35, "UK"),
		};

		public static List<SamplePerson> SetB() => new List<SamplePerson>
		{
			new SamplePerson("Dana", 41, "Australia"),
			new SamplePerson("Eli", 22, "Germany"),
			new SamplePerson("Farah", 38, "Egypt"),
			new SamplePerson("Gus", 50, "Brazil"),
			new SamplePerson("Hana", 29, "Japan"),
		};

		public static List<SamplePerson> SetC() => new List<SamplePerson>
		{
			new SamplePerson("Ivy", 33, "Ireland"),
			new SamplePerson("Jon", 28, "Norway"),
		};
	}
}

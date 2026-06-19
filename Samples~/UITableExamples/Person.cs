using System.Collections.Generic;

namespace Nonatomic.UIElements.Examples
{
	/// <summary>
	/// Plain demo model used by the UITable examples and validation harness.
	/// Kept outside any Editor folder so a runtime sample scene can reuse it.
	/// </summary>
	public class Person
	{
		public string Name;
		public int Age;
		public string Country;

		public Person(string name, int age, string country)
		{
			Name = name;
			Age = age;
			Country = country;
		}

		public static List<Person> SetA() => new List<Person>
		{
			new Person("Alice", 30, "USA"),
			new Person("Bob", 25, "Canada"),
			new Person("Charlie", 35, "UK"),
		};

		public static List<Person> SetB() => new List<Person>
		{
			new Person("Dana", 41, "Australia"),
			new Person("Eli", 22, "Germany"),
			new Person("Farah", 38, "Egypt"),
			new Person("Gus", 50, "Brazil"),
			new Person("Hana", 29, "Japan"),
		};

		public static List<Person> SetC() => new List<Person>
		{
			new Person("Ivy", 33, "Ireland"),
			new Person("Jon", 28, "Norway"),
		};
	}
}

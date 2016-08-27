# SQLite4Unity3dEntityGenerator
Generates simple classes for db tables.

For example:
using SQLite4Unity3d;

public class Person  {

	[PrimaryKey, AutoIncrement]
	public int Id { get; set; }
	public string Name { get; set; }
	public string Surname { get; set; }
	public int Age { get; set; }

}

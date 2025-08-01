// See https://aka.ms/new-console-template for more information

var p1 = new Person("Ram");
var p2 = new Person("Hari");
List<Person> people = [p1, p2];


Console.WriteLine($"The name is{p1.Fname} ");
Console.WriteLine(people.Count);
public class Person(string Name)
{
    public string Fname { get; } = Name;
}
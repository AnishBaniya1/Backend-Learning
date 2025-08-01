// See https://aka.ms/new-console-template for more information

var p1 = new Person("Ram");
var p2 = new Person("Hari");
List<Person> people = [p1, p2];

p1.Pets.Add(new Dog("Burno"));
p2.Pets.Add(new Cat("Jackey"));
p2.Pets.Add(new Dog("Kaley"));
foreach (var person in people)
{
    Console.WriteLine($"{person}");
    foreach (var pet in person.Pets)
    {
        Console.WriteLine($"{pet}");
    }
}
var dogOwners = from person in people where person.Pets.Any(pet => pet is Dog) select person;
Console.WriteLine("People who have dog:");
foreach (var person in dogOwners)
{
    Console.WriteLine($"{person.Fname}");
}

Console.WriteLine($"The name is {p1.Fname} ");

Console.WriteLine(people.Count);
public class Person(string Name)
{
    public string Fname { get; } = Name;
    public List<Pet> Pets { get; } = new();
    public override string ToString()
    {
        return Fname;
    }
}

public abstract class Pet(string Name)
{
    public string first { get; } = Name;
    public abstract string Noise();

    public override string ToString()
    {
        return $"{first}, I am a {GetType().Name} and I {Noise()}";
    }
}

public class Dog(string Name) : Pet(Name)
{
    public override string Noise() => "Bark";
}
public class Cat(string Name) : Pet(Name)
{
    public override string Noise() => "Meow";
}
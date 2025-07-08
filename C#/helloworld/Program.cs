// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");
//string and trim function
string firstfriend = "  Maria  ";
firstfriend = firstfriend.Trim();
string secondfriend = "Sage";
string friends = $"My friends are {firstfriend} and {secondfriend}";
//replace example
//this change is limited to inline ony i.e.
//if i reprint friends, then old value will be used
//for fix change you need define explicitly
Console.WriteLine(friends.Replace("Sage", "Hary"));
Console.WriteLine(friends.Contains("Sage"));

// Integer
int a = 18;
int b = 6;
int c = a + b;
Console.WriteLine(c);

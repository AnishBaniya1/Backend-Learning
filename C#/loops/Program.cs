// See https://aka.ms/new-console-template for more information
//if condition
int a = 5;
int b = 4;
// if (a + b > 10)
//     Console.WriteLine("Answer is greater than 10.");
int c = a + b;
bool myTest = c > 10;
if (myTest)
{
    Console.WriteLine("Answer is greater than 10.");
}
else
{
    Console.WriteLine("Answer is less than 10.");
}

//while loop
int counter = 10;
// while (counter < 5)
// {
//     Console.WriteLine(counter);
//     counter++;
// }
do
{
    Console.WriteLine(counter);
    counter++;
} while (counter < 5);

//for loop

for (int i = 0; i <= 5; i++)
{
    Console.WriteLine(i);
}
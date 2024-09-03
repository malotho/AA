// See https://aka.ms/new-console-template for more information
//https://learn.microsoft.com/en-us/dotnet/standard/parallel-programming/how-to-write-a-simple-parallel-foreach-loop
//https://learn.microsoft.com/en-us/dotnet/standard/parallel-programming/introduction-to-plinq
//https://learn.microsoft.com/en-us/dotnet/api/system.io.binarywriter?view=net-8.0
using System.Collections.Concurrent;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.Json;
using System.Xml.Serialization;

ConcurrentBag<int> globalList = new ConcurrentBag<int>();
int targetCount = 1000000;
int evenStartThreshold = 250000;


// Thread for generating random odd numbers
Task oddNumberTask = Task.Run(() => GenerateOddNumbers());

// Thread for generating negative prime numbers
Task primeNumberTask = Task.Run(() => GenerateNegativePrimes());

// Thread to monitor list size and start even number thread
Task monitorTask = Task.Run(() =>
{
    while (globalList.Count < evenStartThreshold) { }
    Task.Run(() => GenerateEvenNumbers());
});

// Wait for global list to reach exactly 1,000,000
while (globalList.Count < targetCount) { }

// Sort the list using parallel api
List<int> sortedList = globalList.AsParallel()
.OrderBy(n => n).ToList();

// Count and display the number of odd and even numbers
int oddCount = sortedList.Count(n => n % 2 != 0);
int evenCount = sortedList.Count(n => n % 2 == 0);
Console.WriteLine($"Odd Numbers: {oddCount}");
Console.WriteLine($"Even Numbers: {evenCount}");

// Serialize the list to binary and XML files
SerializeToBinary(sortedList, "globalList.bin");
SerializeToXML(sortedList, "globalList.xml");


void GenerateOddNumbers()
{
    Random rand = new Random();
    while (globalList.Count < targetCount)
    {
        int oddNumber = rand.Next(1, int.MaxValue / 2) * 2 + 1;
        globalList.Add(oddNumber);
    }
}

void GenerateNegativePrimes()
{
    int num = 2;
    while (globalList.Count < targetCount)
    {
        if (IsPrime(num))
        {
            globalList.Add(-num);
        }
        num++;
    }
}

void GenerateEvenNumbers()
{
    Random rand = new Random();
    while (globalList.Count < targetCount)
    {
        int evenNumber = rand.Next(1, int.MaxValue / 2) * 2;
        globalList.Add(evenNumber);
    }
}

bool IsPrime(int number)
{
    if (number < 2) return false;
    for (int i = 2; i <= Math.Sqrt(number); i++)
    {
        if (number % i == 0) return false;
    }
    return true;
}

void SerializeToBinary(List<int> list, string filePath)
{
    using (BinaryWriter binWriter =
    new BinaryWriter(File.Open(filePath, FileMode.Create)))
    {
        var bfData = JsonSerializer.SerializeToUtf8Bytes(list);
        binWriter.Write(bfData);

    }
    System.Console.WriteLine("SerializeToBinary");
}

void SerializeToXML(List<int> list, string filePath)
{
    using (FileStream fs = new FileStream(filePath, FileMode.Create))
    {
        XmlSerializer serializer = new XmlSerializer(typeof(List<int>));
        serializer.Serialize(fs, list);
    }
    System.Console.WriteLine("SerializeToXML");
}

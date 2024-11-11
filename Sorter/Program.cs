using Sorter;
using System.Diagnostics;

Console.WriteLine("Write a working folder:");
var folderPath = Console.ReadLine();
while (!Directory.Exists(folderPath))
{
    Console.WriteLine("This path does not exist. Write an existing folder:");
    folderPath = Console.ReadLine();
}

Console.WriteLine("Write a source text file name:");
var sourceFileName = Console.ReadLine();
while (!File.Exists($"{folderPath}\\{sourceFileName}.txt"))
{
    Console.WriteLine("This file does not exist. Please write an existing file:");
    sourceFileName = Console.ReadLine();
}
var sourceFilePath = $"{folderPath}\\{sourceFileName}.txt";
var destinationFilePath = $"{folderPath}\\{sourceFileName}-sorted.txt";

try
{
    int chunkSize = 10_000_000;
    Stopwatch stopwatch = new();
    stopwatch.Start();
    Console.WriteLine("Processing sorting...");
    SortingHelper.SortFile(sourceFilePath, destinationFilePath, chunkSize);
    stopwatch.Stop();

    Console.WriteLine($"File is successfully sorted! The destination file is {sourceFileName}-sorted.txt. Sorting time is {stopwatch.ElapsedMilliseconds / 1000} sec");
    Console.ReadKey();
}
catch (Exception e)
{
    Console.WriteLine(e.Message);
}

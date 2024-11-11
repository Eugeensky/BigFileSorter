using FileGenerator;

Console.WriteLine("Write the destination folder path:");
var folderPath = Console.ReadLine();
while (!Directory.Exists(folderPath))
{
    Console.WriteLine("This folder does not exist. Write an existing folder path:");
    folderPath = Console.ReadLine();
}

Console.WriteLine("Write a file name:");
var textFileName = Console.ReadLine();
var filePath = $"{folderPath}\\{textFileName}.txt";

Console.WriteLine("Write number of records:");
var recordNumberStr = Console.ReadLine();
int recordNumber;
while (!int.TryParse(recordNumberStr, out recordNumber) && recordNumber < 2)
{
    Console.WriteLine("Write a valid number of records:");
    recordNumberStr = Console.ReadLine();
}

try
{
    // Ensure that there will be some String part duplicates
    var duplicatesCount = Math.Ceiling(recordNumber * 0.05);
    var predefinedValues = new Dictionary<int, string>();
    for (int i = 0; i < duplicatesCount; i++)
    {
        var stringPart = Randomizer.GetRandomPhrase();

        var firstIndex = Randomizer.GetRandomNumber(recordNumber) - 1;
        while (predefinedValues.ContainsKey(firstIndex)) firstIndex = Randomizer.GetRandomNumber(recordNumber) - 1;

        var secondIndex = Randomizer.GetRandomNumber(recordNumber) - 1;
        while (secondIndex == firstIndex || predefinedValues.ContainsKey(secondIndex)) secondIndex = Randomizer.GetRandomNumber(recordNumber) - 1;

        predefinedValues[firstIndex] = stringPart;
        predefinedValues[secondIndex] = stringPart;
    }

    using StreamWriter sw = new(filePath);
    for (int i = 0; i < recordNumber; i++)
    {
        if (!predefinedValues.TryGetValue(i, out var stringPart)) stringPart = Randomizer.GetRandomPhrase();
        sw.WriteLine($"{Randomizer.GetRandomNumber(recordNumber)}.{stringPart}");
    }

    Console.WriteLine($"The {textFileName} file was successfully created!");
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
}

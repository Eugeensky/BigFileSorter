namespace Sorter;

public static class SortingHelper
{
    public static void SortFile(string sourceFilePath, string destinationFilePath, int chunkSize)
    {
        List<string> tempFilePaths = [];
        var duplicates = new Dictionary<string, List<int>>();
        var uniques = new Dictionary<string, int>();
        using (StreamReader reader = new(sourceFilePath))
        {
            string line;
            while ((line = reader.ReadLine()) is not null)
            {
                var dotIndex = line.IndexOf('.');
                var strPart = line[(dotIndex + 1)..];
                var numPart = int.Parse(line[..dotIndex]);
                if (duplicates.TryGetValue(strPart, out var nums))
                {
                    nums.Add(numPart);
                } 
                else if (uniques.TryGetValue(strPart, out var num))
                {
                    duplicates[strPart] = [num, numPart];
                    uniques.Remove(strPart);
                } 
                else
                {
                    uniques[strPart] = numPart;
                }

                if (duplicates.Count + uniques.Count >= chunkSize - 1)
                {
                    var tempFilePath = Path.GetTempFileName();
                    tempFilePaths.Add(tempFilePath);
                    
                    WriteLinesToTempFile(uniques, duplicates, tempFilePath);
                }
            }
        }
        
        if (uniques.Count > 0 || duplicates.Count > 0)
        {
            if (tempFilePaths.Count > 0)
            {
                // Sort the remaining lines and write to a temporary file
                var tempFilePath = Path.GetTempFileName();
                tempFilePaths.Add(tempFilePath);
                WriteLinesToTempFile(uniques, duplicates, tempFilePath);
                MergeTempFiles(tempFilePaths, destinationFilePath);

                foreach (var tempFile in tempFilePaths) File.Delete(tempFile);
            }
            else
            {
                // chunk size is bigger then document, so write directly to the dest file without chunking
                WriteLinesToDestFile(uniques, duplicates, destinationFilePath);
            }
        }
    }
    
    private static void MergeTempFiles(List<string> filePaths, string destinationFilePath)
    {
        var chunkReaderSet = new HashSet<ChunkReader>();
        foreach (var filePath in filePaths) chunkReaderSet.Add(new ChunkReader(new StreamReader(filePath)));

        using StreamWriter writer = new(destinationFilePath);
        while (chunkReaderSet.Count > 0)
        {
            var readNextLineChunk = chunkReaderSet.Min();
            writer.WriteLine(readNextLineChunk.CurrentLine);

            readNextLineChunk.ReadNext();
            if (!readNextLineChunk.HasLine)
            {
                readNextLineChunk.Dispose();
                chunkReaderSet.Remove(readNextLineChunk);
            }
        }
    }

    private static void WriteLinesToTempFile(Dictionary<string, int> unique, Dictionary<string, List<int>> duplicates, string tempFilePath)
    {
        var sorting = unique.Keys.Concat(duplicates.Keys)
            .AsParallel()
            .GroupBy(k => k[0])
            .OrderBy(g => g.Key.ToString().ToLower())
            .SelectMany(g => g.Order(StringComparer.OrdinalIgnoreCase));

        using StreamWriter writer = new(tempFilePath);
        foreach (var key in sorting)
        {
            // write string part and all the numbers into the tempfile in a following format:
            // StringPart.NumsPart1.NumPart2.NumPart3...etc
            if (unique.TryGetValue(key, out var value)) writer.WriteLine($"{key}.{value}");
            else if(duplicates.TryGetValue(key, out var values))
            {
                values.Sort();
                writer.WriteLine($"{key}.{string.Join('.', values)}");
            }            
        }  
        unique.Clear();
        duplicates.Clear();
    }

    private static void WriteLinesToDestFile(Dictionary<string, int> unique, Dictionary<string, List<int>> duplicates, string filePath)
    {
        var sorting = unique.Keys.Concat(duplicates.Keys)
            .AsParallel()
            .GroupBy(k => k[0])
            .OrderBy(g => g.Key.ToString().ToLower())
            .SelectMany(g => g.Order(StringComparer.OrdinalIgnoreCase));

        using StreamWriter writer = new(filePath);
        foreach (var key in sorting)
        {
            if (unique.TryGetValue(key, out var uniqueVal)) writer.WriteLine($"{key}.{uniqueVal}");
            else if (duplicates.TryGetValue(key, out var values))
            {
                values.Sort();
                foreach(var val in values)
                {
                    writer.WriteLine($"{key}.{val}");
                }
            }
        }
    }
}


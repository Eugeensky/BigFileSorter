namespace Sorter;

public static class SortingHelper
{
    public static void SortFile(string sourceFilePath, string destinationFilePath, int chunkSize)
    {
        List<string> tempFilePaths = [];
        var dict = new Dictionary<string, List<int>>();
        using (StreamReader reader = new(sourceFilePath))
        {
            string line;
            while ((line = reader.ReadLine()) is not null)
            {
                // split data into parts on this step to simplify the comparision process
                var dotIndex = line.IndexOf('.');
                var strPart = line[(dotIndex + 1)..];
                var numPart = int.Parse(line[..dotIndex]);
                if (dict.TryGetValue(strPart, out var nums)) nums.Add(numPart);
                else dict[strPart] = [numPart];

                if (dict.Count >= chunkSize - 1)
                {
                    var tempFilePath = Path.GetTempFileName();
                    tempFilePaths.Add(tempFilePath);

                    WriteLinesToTempFile(GetSorting(dict), tempFilePath);
                    dict.Clear();
                }
            }
        }
        
        if (dict.Count > 0)
        {
            if (tempFilePaths.Count > 0)
            {
                // Sort the remaining lines and write to a temporary file
                var tempFilePath = Path.GetTempFileName();
                tempFilePaths.Add(tempFilePath);

                WriteLinesToTempFile(GetSorting(dict), tempFilePath);
                dict.Clear();

                MergeTempFiles(tempFilePaths, destinationFilePath);

                foreach (var tempFile in tempFilePaths) File.Delete(tempFile);
            }
            else
            {
                // chunk size is bigger then document, so write directly to the dest file without chunking
                WriteLinesToDestFile(GetSorting(dict), destinationFilePath);
            }
        }
    }

    private static ParallelQuery<KeyValuePair<string, List<int>>> GetSorting(Dictionary<string, List<int>> dict)
    {
        return dict
            .AsParallel()
            .GroupBy(kv => kv.Key[0])
            .OrderBy(g => g.Key.ToString().ToLower())
            .SelectMany(g => g.OrderBy(k => k.Key, StringComparer.OrdinalIgnoreCase));
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

    private static void WriteLinesToTempFile(ParallelQuery<KeyValuePair<string, List<int>>> sorting, string tempFilePath)
    {
        using StreamWriter writer = new(tempFilePath);
        foreach (var (str, nums) in sorting)
        {
            // write string part and all the numbers into the tempfile in a following format:
            // StringPart.NumsPart1.NumPart2.NumPart3...etc
            nums.Sort();
            var numsStr = string.Join('.', nums.Select(num => num.ToString()));
            writer.WriteLine($"{str}.{numsStr}");
        }   
    }

    private static void WriteLinesToDestFile(ParallelQuery<KeyValuePair<string, List<int>>> sorting, string filePath)
    {
        using StreamWriter writer = new(filePath);
        foreach(var (str, nums) in sorting)
        {
            nums.Sort();
            foreach(var num in nums) writer.WriteLine($"{num}.{str}");
        }
    }
}


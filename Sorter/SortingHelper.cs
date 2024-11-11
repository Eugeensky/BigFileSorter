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
                var parts = line.Split('.');
                var strPart = parts[1];
                var numPart = int.Parse(parts[0]);
                if (dict.TryGetValue(strPart, out var nums)) nums.Add(numPart);
                else dict[strPart] = [numPart];

                if (dict.Count >= chunkSize - 1)
                {
                    var tempFilePath = Path.GetTempFileName();
                    tempFilePaths.Add(tempFilePath);
                    WriteLinesToTempFile(dict.AsParallel().OrderBy(x => x.Key), tempFilePath);
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
                WriteLinesToTempFile(dict.AsParallel().OrderBy(x => x.Key), tempFilePath);
                dict.Clear();
                MergeTempFiles(tempFilePaths, destinationFilePath);

                foreach (var tempFile in tempFilePaths) File.Delete(tempFile);
            }
            else
            {
                // chunk size is bigger then document, so write directly to the dest file without chunking
                WriteLinesToDestFile(dict.AsParallel().OrderBy(x => x.Key), destinationFilePath);
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

    private static void WriteLinesToTempFile(IEnumerable<KeyValuePair<string, List<int>>> sorted, string tempFilePath)
    {
        using StreamWriter writer = new(tempFilePath);
        foreach (var (str, nums) in sorted)
        {
            // write string part and all the numbers into the tempfile in a following format:
            // StringPart.NumsPart1.NumPart2.NumPart3...etc
            nums.Sort();
            var numsStr = string.Join('.', nums.Select(num => num.ToString()));
            writer.WriteLine($"{str}.{numsStr}");
        }   
    }

    private static void WriteLinesToDestFile(IEnumerable<KeyValuePair<string, List<int>>> lines, string filePath)
    {
        using StreamWriter writer = new(filePath);
        foreach(var (str, nums) in lines)
        {
            nums.Sort();
            foreach(var num in nums) writer.WriteLine($"{num}.{str}");
        }
    }
}


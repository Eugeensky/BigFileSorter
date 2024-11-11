namespace Sorter;

public class ChunkReader : IComparable<ChunkReader>
{
    private readonly StreamReader _Reader;
    private readonly Queue<int> _NumQueue = [];

    public ChunkReader(StreamReader reader)
    {
        _Reader = reader;
        ReadNext();
    }

    protected string StringPart { get; private set; }

    protected int NumberPart { get; private set; }

    public bool HasLine { get; private set; }

    public string CurrentLine { get => $"{NumberPart}.{StringPart}"; }

    public void ReadNext()
    {
        if (HasLine && _NumQueue.Count > 0)
        {
            NumberPart = _NumQueue.Dequeue();
            return;
        }

        var line = _Reader.ReadLine();
        HasLine = line != null;
        if (HasLine)
        {
            var parts = line.Split('.');
            StringPart = parts[0];
            NumberPart = int.Parse(parts[1]);

            for (int i = 2; i < parts.Length; i++) _NumQueue.Enqueue(int.Parse(parts[i]));
        }

    }

    public override int GetHashCode()
    {
        return _Reader.GetHashCode();
    }

    public void Dispose()
    {
        _Reader?.Close();
        _Reader?.Dispose();
    }

    public int CompareTo(ChunkReader other)
    {
        var res = StringPart.CompareTo(other.StringPart);
        if (res == 0) return NumberPart.CompareTo(other.NumberPart);
        return res;
    }
}

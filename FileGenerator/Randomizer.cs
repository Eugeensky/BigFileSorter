using CrypticWizard.RandomWordGenerator;

namespace FileGenerator;

public static class Randomizer
{
    private static readonly WordGenerator wg = new();
    private static readonly Random rnd = new();

    public static string GetRandomPhrase()
    {
        string phrase = string.Empty;
        while (string.IsNullOrEmpty(phrase))
        {
            try
            {
                // sometimes WordGenerator may throw an exception, looks like it's not a very stable package
                phrase = string.Join(" ", wg.GetWords(rnd.Next(3) + 1));
            }
            catch
            {
                continue;
            }
        }
        return phrase;
    }

    public static int GetRandomNumber(int max)
    {
        return rnd.Next(max) + 1;
    }
}


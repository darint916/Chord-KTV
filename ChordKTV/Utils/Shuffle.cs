
namespace ChordKTV.Utils;

public class Shuffle
{
    public static void FisherYatesShuffle<T>(IList<T> list)
    {
        var random = new Random();
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = random.Next(0, i + 1);
            (list[j], list[i]) = (list[i], list[j]);
        }
    }
}

namespace CarBazzar.Models;

public static class WishlistStore
{
    private static readonly HashSet<int> _ids = new();

    public static bool IsSaved(int id) => _ids.Contains(id);

    public static void Toggle(int id)
    {
        if (!_ids.Add(id))
        {
            _ids.Remove(id);
        }
    }

    public static IReadOnlyCollection<int> AllIds => _ids;
}


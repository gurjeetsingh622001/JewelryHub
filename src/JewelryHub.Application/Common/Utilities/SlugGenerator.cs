namespace JewelryHub.Application.Common.Utilities;

public static class SlugGenerator
{
    public static string FromName(string name)
    {
        var lowered = name.Trim().ToLowerInvariant();
        var chars = lowered.Select(c => char.IsLetterOrDigit(c) ? c : '-').ToArray();
        var slug = new string(chars);
        while (slug.Contains("--")) slug = slug.Replace("--", "-");
        return slug.Trim('-');
    }
}

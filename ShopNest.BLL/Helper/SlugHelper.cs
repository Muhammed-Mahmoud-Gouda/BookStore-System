using System.Text.RegularExpressions;

namespace ShopNest.BLL.Helpers
{
    public static class SlugHelper
    {
        public static string GenerateSlug(string title)
        {
            // Lower case
            var slug = title.ToLower();

            // Remove special characters
            slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");

            // Replace spaces with hyphens
            slug = Regex.Replace(slug, @"\s+", "-");

            // Remove multiple hyphens
            slug = Regex.Replace(slug, @"-+", "-");

            // Trim hyphens
            slug = slug.Trim('-');

            return slug;
        }
    }
}
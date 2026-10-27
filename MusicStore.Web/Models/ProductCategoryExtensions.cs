namespace MusicStore.Web.Models;

/// <summary>Русские названия категорий для отображения на страницах.</summary>
public static class ProductCategoryExtensions
{
    public static string GetDisplayName(this ProductCategory category) => category switch
    {
        ProductCategory.Guitars => "Гитары",
        ProductCategory.Strings => "Струны",
        ProductCategory.Pianos => "Пианино",
        ProductCategory.Drums => "Барабаны",
        ProductCategory.Vinyl => "Пластинки",
        ProductCategory.Amplifiers => "Усилители",
        _ => category.ToString(),
    };
}

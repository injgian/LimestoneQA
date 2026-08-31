using OpenQA.Selenium;

namespace Limestone.Tests.Ui.Pages;

public sealed class InventoryPage : BasePage
{
    private static readonly By Title = By.ClassName("title");
    private static readonly By CartLink = By.ClassName("shopping_cart_link");
    private static readonly By CartBadge = By.ClassName("shopping_cart_badge");
    private static readonly By InventoryItems = By.ClassName("inventory_item");

    public InventoryPage(IWebDriver driver) : base(driver) { }

    public string Heading => TextOf(Title);

    public bool IsLoaded => Driver.Url.Contains("inventory.html", StringComparison.OrdinalIgnoreCase)
                            && IsDisplayed(Title);

    /// <summary>
    /// SauceDemo derives the add-to-cart button id from the product name,
    /// so the locator is built from the item rather than hard-coded per product.
    /// </summary>
    public InventoryPage AddToCart(string productName)
    {
        Click(By.Id($"add-to-cart-{Slug(productName)}"));
        return this;
    }

    public int CartItemCount =>
        IsDisplayed(CartBadge) ? int.Parse(TextOf(CartBadge)) : 0;

    public int VisibleProductCount => WaitForAll(InventoryItems).Count;

    public CartPage OpenCart()
    {
        Click(CartLink);
        return new CartPage(Driver);
    }

    private static string Slug(string productName) =>
        productName.Trim().ToLowerInvariant().Replace(' ', '-').Replace(".", string.Empty);
}

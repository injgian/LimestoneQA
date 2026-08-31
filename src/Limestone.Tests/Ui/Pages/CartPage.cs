using OpenQA.Selenium;

namespace Limestone.Tests.Ui.Pages;

public sealed class CartPage : BasePage
{
    private static readonly By Title = By.ClassName("title");
    private static readonly By CartItems = By.ClassName("cart_item");
    private static readonly By ItemName = By.ClassName("inventory_item_name");
    private static readonly By ItemQuantity = By.ClassName("cart_quantity");
    private static readonly By ItemPrice = By.ClassName("inventory_item_price");

    public CartPage(IWebDriver driver) : base(driver) { }

    public bool IsLoaded => Driver.Url.Contains("cart.html", StringComparison.OrdinalIgnoreCase)
                            && IsDisplayed(Title);

    public IReadOnlyList<CartLine> Lines =>
        Driver.FindElements(CartItems)
              .Select(row => new CartLine(
                  row.FindElement(ItemName).Text.Trim(),
                  int.Parse(row.FindElement(ItemQuantity).Text.Trim()),
                  row.FindElement(ItemPrice).Text.Trim()))
              .ToList();
}

public sealed record CartLine(string Name, int Quantity, string Price);

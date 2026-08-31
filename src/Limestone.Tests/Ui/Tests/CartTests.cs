using Limestone.Tests.Core;
using Limestone.Tests.Core.Assertions;
using Limestone.Tests.TestData;
using Xunit;
using Xunit.Abstractions;

namespace Limestone.Tests.Ui.Tests;

[Trait(TestCategories.Key, TestCategories.Ui)]
[Trait(TestCategories.Key, TestCategories.Smoke)]
public sealed class CartTests : UiTestBase
{
    public CartTests(ITestOutputHelper output) : base(output) { }

    [Fact(DisplayName = "Standard user can log in, add an item and see it in the cart")]
    public void StandardUser_CanAddItemToCart() => Execute(() =>
    {
        var user = TestUserBuilder.A().Standard().Build();

        var inventory = LoginPage.Open().LogInAs(user.Username, user.Password);

        Assert.True(inventory.IsLoaded, "Login did not land on the inventory page.");

        inventory.AddToCart(Products.Backpack);

        Assert.True(inventory.CartItemCount == 1,
            $"Cart badge should show 1 after adding an item, but showed {inventory.CartItemCount}.");

        var cart = inventory.OpenCart();
        var lines = cart.Lines;

        Verify.All(
            () => Assert.True(cart.IsLoaded, "Cart page did not open."),
            () => Assert.Single(lines),
            () => Assert.Equal(Products.Backpack, lines[0].Name),
            () => Assert.Equal(1, lines[0].Quantity),
            () => Assert.StartsWith("$", lines[0].Price));
    });
}

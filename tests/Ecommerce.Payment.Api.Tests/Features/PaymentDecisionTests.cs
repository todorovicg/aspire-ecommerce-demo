using Ecommerce.Payment.Api.Features;

namespace Ecommerce.Payment.Api.Tests.Features;

public sealed class PaymentDecisionTests
{
    [Theory]
    [InlineData("4242424242424242", false)]
    [InlineData("4000000000000002", true)]
    [InlineData("1111222233330002", true)]
    [InlineData("4000000000000020", false)]
    public void IsDeclined_FollowsTheSuffixRule(string cardNumber, bool declined)
    {
        Assert.Equal(declined, PaymentDecision.IsDeclined(cardNumber));
    }
}

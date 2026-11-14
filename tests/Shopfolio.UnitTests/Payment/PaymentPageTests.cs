using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Infrastructure.Integrations;
using Shopfolio.Infrastructure.Routing;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Infrastructure.Status;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment;
using Shopfolio.Modules.Payment.Contracts;
using Shopfolio.Pages.Orders;
using Shopfolio.Pages.Payment;
using Shopfolio.UnitTests.Fakes;
using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Payment;

/// <summary>
/// The payment page's and the return's rules without a server (T510 KK3, KK4, KK7-KK9; 07 UC-29, UC-30; 09 15.2 item 4): the policy's two changes, the frame's source,
/// the start's face over each class of the start's answer, the start's values and the return page's address.
/// </summary>
public sealed class PaymentPageTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 14, 9, 0, 0, TimeSpan.Zero);

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public void ThePolicy_OpensTheFrameOnlyForTheSiteAndTheProvider_AndTheFramingOnlyForTheSite_LeavingScriptAndFormActionAlone_KK4_KK8()
    {
        string plain = ContentSecurityPolicy.Build("n");
        string payment = ContentSecurityPolicy.Build("n", new ContentSecurityPolicyOptions("https://form.example", false));
        string handOver = ContentSecurityPolicy.Build("n", new ContentSecurityPolicyOptions(null, true));

        Assert.Contains("frame-src 'none'", plain, StringComparison.Ordinal);
        Assert.Contains("frame-ancestors 'none'", plain, StringComparison.Ordinal);
        Assert.Equal(plain.Replace("frame-src 'none'", "frame-src 'self' https://form.example", StringComparison.Ordinal), payment);
        Assert.Equal(plain.Replace("frame-ancestors 'none'", "frame-ancestors 'self'", StringComparison.Ordinal), handOver);
        Assert.Equal(ContentSecurityPolicy.Build("n", ContentSecurityPolicyOptions.Default), plain);
    }

    [Fact]
    public void TheFrameSource_IsTheProvidersOrigin_AndHoldsOnlyAddressesUnderItsFormRoot_KK4()
    {
        ServiceCollection services = new();
        services.AddPaymentProvider();
        PaymentFrameSource source = (PaymentFrameSource)services.Single(d => d.ServiceType == typeof(PaymentFrameSource)).ImplementationInstance!;

        Assert.Equal(PayTrStartForm.FrameOrigin, source.Origin);
        Assert.Equal("https://www.paytr.com", source.Origin);
        Assert.True(source.Holds(new Uri(PayTrStartForm.FrameAddressRoot + "abc")));
        Assert.False(source.Holds(new Uri(PayTrStartForm.FrameAddressRoot)));
        Assert.False(source.Holds(new Uri("https://www.paytr.com.example.net/odeme/guvenli/abc")));
        Assert.False(source.Holds(new Uri("https://www.paytr.com/odeme/baska/abc")));
        Assert.Throws<ArgumentException>(() => new PaymentFrameSource("https://a.example", "https://b.example/x/"));
    }

    [Theory]
    [InlineData(0, PaymentStartDecision.OpenPaymentForm)]
    [InlineData(1, PaymentStartDecision.CancelAsUnavailable)]
    [InlineData(2, PaymentStartDecision.StayOnConfirmation)]
    public async Task TheStartsFace_GivesEachClassItsDecision_AndTheAddressOnlyWhenTheFormOpens_KK3_KK5_KK6(int kind, PaymentStartDecision expected)
    {
        Uri address = new(PayTrStartForm.FrameAddressRoot + "abc");
        PaymentStartResult answer = kind switch
        {
            0 => new PaymentStartResult.Started(address),
            1 => new PaymentStartResult.ConnectionFailed(ExternalCallFailure.TimeoutReason),
            _ => new PaymentStartResult.SecondTokenRejected("reuse"),
        };
        FakePaymentGateway gateway = new();
        gateway.EnqueueStart(answer);
        ManualTimeProvider time = new(Now);
        PaymentStartFace face = new(new PaymentStarts(new PaymentStartFailureSeries(time), new CardUnavailability(time), gateway));

        PaymentStartAnswer result = await face.StartAsync(Order(), Ct);

        Assert.Equal(expected, result.Decision);
        Assert.Equal(kind == 0 ? address : null, result.FrameAddress);
        Assert.Equal(TransitionReason.PaymentPageNotOpened, face.UnopenedCancellationReason);
        PaymentStartRequest sent = Assert.IsType<PaymentStartRequest>(Assert.Single(gateway.Calls).Argument);
        Assert.Equal(new PaymentStartRequest("2026-7K4M9P", 34.56m, "musteri@example.com", "firma@example.com", "Ayşe Yılmaz", "Bağdat Cad. 1", null, "2121234567", "203.0.113.7", Now.AddMinutes(30), "anahtar_1"), sent);
    }

    [Fact]
    public void TheStartsValues_ComeFromTheFrozenOrder_TheFirmsEmailStandingInForADisposedOne_KK3()
    {
        ConfirmationScreenFacts facts = new(Guid.NewGuid(), "2026-7K4M9P", true, PaymentMethod.Card, 34.56m, null, Now.AddMinutes(30), null, "firma@example.com", null, null, null, "2121234567", "anahtar_1");

        PaymentStartOrder order = ConfirmationPageModel.OrderOf(facts, "203.0.113.7", "anahtar_1");

        Assert.Equal(new PaymentStartOrder("2026-7K4M9P", 34.56m, "firma@example.com", "firma@example.com", string.Empty, string.Empty, null, "2121234567", "203.0.113.7", Now.AddMinutes(30), "anahtar_1"), order);
    }

    [Fact]
    public void TheReturnPage_IsTheLayersReturnRoot_WithTheAccessRootsKeyForm_AndItsPageCarriesTheTemplate_KK7_KK9()
    {
        string view = File.ReadAllText(Path.Combine(RepositoryRoot.Find(), "src/Shopfolio.Web/Pages/Payment/Return.cshtml"));

        Assert.StartsWith($"@page \"{ReturnModel.Template.Replace("\\\\", "\\", StringComparison.Ordinal)}\"", view, StringComparison.Ordinal);
        Assert.StartsWith(PayTrStartForm.ReturnPathRoot, ReturnModel.Template, StringComparison.Ordinal);
        Assert.Contains("{accessKey:regex(" + ReturnModel.KeyPattern + ")}", ReturnModel.Template, StringComparison.Ordinal);
        Assert.Contains("{accessKey:regex(" + ReturnModel.KeyPattern + ")}", OrderRoutes.AccessRoot, StringComparison.Ordinal);
        Assert.Contains("Layout = null;", view, StringComparison.Ordinal);
        Assert.Contains("target=\"_top\"", view, StringComparison.Ordinal);
        Assert.Equal(3, typeof(ReturnModel).GetCustomAttributes(false).Count(a => a is AllowFramingAttribute or KeyedLinkPageAttribute or NoStoreAttribute));
        Assert.Null(typeof(ConfirmationPageModel).GetCustomAttributes(typeof(AllowFramingAttribute), true).SingleOrDefault());
        Assert.NotNull(typeof(AccessConfirmationModel).GetCustomAttributes(typeof(AllowPaymentFrameAttribute), true).SingleOrDefault());
    }

    private static PaymentStartOrder Order() =>
        new("2026-7K4M9P", 34.56m, "musteri@example.com", "firma@example.com", "Ayşe Yılmaz", "Bağdat Cad. 1", null, "2121234567", "203.0.113.7", Now.AddMinutes(30), "anahtar_1");
}

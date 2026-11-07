using System.Reflection;

using Shopfolio.Infrastructure.Errors;
using Shopfolio.Modules.Settings;
using Shopfolio.Modules.Settings.Contracts;
using Shopfolio.Modules.Settings.Contracts.Gates;

namespace Shopfolio.UnitTests.InstallationSettings;

/// <summary>
/// The sales gate's rule as a pure function (T331; 02 3.1.5, 3.1.6, 6.1.4; 11 T331 criteria 1-3, 8; KK1-KK3, KK8): sales are open only when the four conditions hold together;
/// a broken condition closes the adding, the payment step and the confirmation and is named; the showcase, the existing cart and an open order are never closed by the gate;
/// the data-collecting entries have their own door (the privacy notice and the cookie policy) and the sign-in of an existing account is never behind it; a condition nobody has
/// bound is not met. The integration tests run the same rule through the real module.
/// </summary>
public sealed class SalesGateRulesTests
{
    private static readonly string[] MissingIdentity = ["KepAddress", "ChamberName"];

    private static readonly SalesGateOperation[] ClosedByTheGate =
    [
        SalesGateOperation.AddToCart, SalesGateOperation.ProceedToCheckout, SalesGateOperation.ConfirmOrder,
    ];

    private static readonly DataCollectionEntry[] BehindTheDataCollectionDoor =
    [
        DataCollectionEntry.AccountRegistration, DataCollectionEntry.NewAccountGoogleSignIn, DataCollectionEntry.ContactForm,
    ];

    public static TheoryData<SalesGateCondition> EveryCondition => [.. Enum.GetValues<SalesGateCondition>()];

    public static TheoryData<SalesGateOperation> EveryOperation => [.. Enum.GetValues<SalesGateOperation>()];

    public static TheoryData<DataCollectionEntry> EveryEntry => [.. Enum.GetValues<DataCollectionEntry>()];

    private static SalesGateFacts AllMet() => new([], true, false, true, true, true);

    /// <summary>The facts with exactly the given condition broken (the identity one also breaks the privacy notice by definition; the others break nothing else).</summary>
    private static SalesGateFacts Breaking(SalesGateCondition condition) => condition switch
    {
        SalesGateCondition.CompanyIdentity => AllMet() with { MissingIdentityFields = MissingIdentity },
        SalesGateCondition.PaymentMethod => AllMet() with { PaymentMethodOpen = false },
        SalesGateCondition.TemporaryClosure => AllMet() with { SalesTemporarilyClosed = true },
        SalesGateCondition.PrivacyNotice => AllMet() with { PrivacyNoticePublishedWithHostingLocation = false },
        SalesGateCondition.CookiePolicy => AllMet() with { CookiePolicyPublished = false },
        SalesGateCondition.ReturnAddress => AllMet() with { ReturnAddressEntered = false },
        _ => throw new ArgumentOutOfRangeException(nameof(condition), condition, "A condition without a way to break it."),
    };

    [Fact]
    public void WhenAllFourConditionsHold_SalesAreOpen_AndEverythingIsPermitted()
    {
        SalesGate gate = SalesGateRules.Evaluate(AllMet());

        Assert.True(gate.IsOpen);
        Assert.Empty(gate.UnmetConditions);
        Assert.Empty(gate.SetupChecklist);
        Assert.False(gate.ShowsClosedNotice);
        Assert.False(gate.IsTemporarilyClosed);
        Assert.True(gate.IsDataCollectionOpen);
        Assert.All(Enum.GetValues<SalesGateOperation>(), operation => Assert.True(gate.Permits(operation)));
        Assert.All(Enum.GetValues<DataCollectionEntry>(), entry => Assert.True(gate.Permits(entry)));
    }

    [Theory]
    [MemberData(nameof(EveryCondition))]
    public void EachConditionBroken_ClosesSales_AndIsNamed(SalesGateCondition condition)
    {
        SalesGate gate = SalesGateRules.Evaluate(Breaking(condition));

        Assert.False(gate.IsOpen);
        Assert.Contains(condition, gate.UnmetConditions);
        Assert.True(gate.ShowsClosedNotice);
        Assert.All(ClosedByTheGate, operation => Assert.False(gate.Permits(operation)));
    }

    [Theory]
    [MemberData(nameof(EveryCondition))]
    public void EachConditionBroken_NamesNothingButItself_ExceptTheIdentityWhichTheNoticeSharesItsDefinitionWith(SalesGateCondition condition)
    {
        SalesGate gate = SalesGateRules.Evaluate(Breaking(condition));

        SalesGateCondition[] expected = condition == SalesGateCondition.CompanyIdentity
            ? [SalesGateCondition.CompanyIdentity, SalesGateCondition.PrivacyNotice]
            : [condition];
        Assert.Equal(expected, gate.UnmetConditions);
    }

    [Theory]
    [MemberData(nameof(EveryOperation))]
    public void AClosedGate_RefusesTheAddingTheCheckoutAndTheConfirmation_AndNeverTheShowcaseTheCartOrAnOpenOrder(SalesGateOperation operation)
    {
        SalesGate gate = SalesGateRules.Evaluate(AllMet() with { PaymentMethodOpen = false, MissingIdentityFields = MissingIdentity });

        GateRefusal? refusal = gate.RefusalOf(operation);

        if (ClosedByTheGate.Contains(operation))
        {
            Assert.NotNull(refusal);
            Assert.Equal(ErrorCode.SalesClosed, refusal.Code);
            Assert.Equal(gate.UnmetConditions, refusal.UnmetConditions);
            Assert.Equal(MissingIdentity, refusal.MissingIdentityFields);
            Assert.Contains(SalesGateCondition.PaymentMethod, refusal.UnmetConditions);
            Assert.Contains(SalesGateCondition.CompanyIdentity, refusal.UnmetConditions);
        }
        else
        {
            Assert.Null(refusal);
            Assert.True(gate.Permits(operation));
        }
    }

    [Theory]
    [MemberData(nameof(EveryOperation))]
    public void AnOpenGate_RefusesNothing(SalesGateOperation operation)
    {
        Assert.Null(SalesGateRules.Evaluate(AllMet()).RefusalOf(operation));
    }

    [Fact]
    public void TheSwitchAlone_ClosesSales_ButIsNoSetupTask_AndTheDataCollectionStaysOpen()
    {
        SalesGate gate = SalesGateRules.Evaluate(Breaking(SalesGateCondition.TemporaryClosure));

        Assert.True(gate.IsTemporarilyClosed);
        Assert.Equal([SalesGateCondition.TemporaryClosure], gate.UnmetConditions);
        Assert.Empty(gate.SetupChecklist);
        Assert.True(gate.IsDataCollectionOpen);
        Assert.All(Enum.GetValues<DataCollectionEntry>(), entry => Assert.True(gate.Permits(entry)));
    }

    [Theory]
    [InlineData(SalesGateCondition.PrivacyNotice)]
    [InlineData(SalesGateCondition.CookiePolicy)]
    [InlineData(SalesGateCondition.CompanyIdentity)]
    public void ThePrivacyNoticeOrTheCookiePolicyMissing_ClosesTheDataCollectingEntries_ButNeverTheSignInOfAnExistingAccount(SalesGateCondition condition)
    {
        SalesGate gate = SalesGateRules.Evaluate(Breaking(condition));

        Assert.False(gate.IsDataCollectionOpen);
        foreach (DataCollectionEntry entry in BehindTheDataCollectionDoor)
        {
            GateRefusal? refusal = gate.RefusalOf(entry);
            Assert.NotNull(refusal);
            Assert.Equal(ErrorCode.DataCollectionClosed, refusal.Code);
            Assert.All(refusal.UnmetConditions, c => Assert.Contains(c, new[] { SalesGateCondition.PrivacyNotice, SalesGateCondition.CookiePolicy }));
            Assert.NotEmpty(refusal.UnmetConditions);
        }

        Assert.True(gate.Permits(DataCollectionEntry.ExistingAccountSignIn));
        Assert.Null(gate.RefusalOf(DataCollectionEntry.ExistingAccountSignIn));
    }

    [Theory]
    [InlineData(SalesGateCondition.PaymentMethod)]
    [InlineData(SalesGateCondition.ReturnAddress)]
    [InlineData(SalesGateCondition.TemporaryClosure)]
    public void AConditionOfTheSalesAlone_LeavesTheDataCollectingEntriesOpen(SalesGateCondition condition)
    {
        SalesGate gate = SalesGateRules.Evaluate(Breaking(condition));

        Assert.False(gate.IsOpen);
        Assert.True(gate.IsDataCollectionOpen);
        Assert.All(Enum.GetValues<DataCollectionEntry>(), entry => Assert.True(gate.Permits(entry)));
    }

    [Fact]
    public void TheDataCollectionRefusal_NamesWhichTextWaits_AndTheIdentityFieldsWhenTheyAreWhy()
    {
        SalesGate cookieOnly = SalesGateRules.Evaluate(Breaking(SalesGateCondition.CookiePolicy));
        SalesGate identity = SalesGateRules.Evaluate(Breaking(SalesGateCondition.CompanyIdentity));
        SalesGate both = SalesGateRules.Evaluate(AllMet() with { CookiePolicyPublished = false, PrivacyNoticePublishedWithHostingLocation = false });

        Assert.Equal([SalesGateCondition.CookiePolicy], cookieOnly.RefusalOf(DataCollectionEntry.ContactForm)!.UnmetConditions);
        Assert.Equal([SalesGateCondition.PrivacyNotice], identity.RefusalOf(DataCollectionEntry.AccountRegistration)!.UnmetConditions);
        Assert.Equal(MissingIdentity, identity.RefusalOf(DataCollectionEntry.AccountRegistration)!.MissingIdentityFields);
        Assert.Equal([SalesGateCondition.PrivacyNotice, SalesGateCondition.CookiePolicy], both.RefusalOf(DataCollectionEntry.NewAccountGoogleSignIn)!.UnmetConditions);
    }

    [Theory]
    [MemberData(nameof(EveryEntry))]
    public void EveryEntry_IsEitherBehindTheDoorOrTheExistingSignIn(DataCollectionEntry entry)
    {
        SalesGate closed = SalesGateRules.Evaluate(Breaking(SalesGateCondition.CookiePolicy));

        Assert.Equal(entry == DataCollectionEntry.ExistingAccountSignIn, closed.Permits(entry));
        Assert.Equal(BehindTheDataCollectionDoor.Contains(entry), !closed.Permits(entry));
    }

    [Fact]
    public void TheFourSurfaces_ReadOneResult_ForEveryCombinationOfTheFacts()
    {
        // The truth table of the six facts (the identity as complete or missing); the expected answer is the spec's own sentence (02 3.1.5), written independently of the rule.
        for (int bits = 0; bits < 64; bits++)
        {
            bool identityComplete = (bits & 1) != 0;
            bool payment = (bits & 2) != 0;
            bool switchOn = (bits & 4) != 0;
            bool privacy = (bits & 8) != 0;
            bool cookie = (bits & 16) != 0;
            bool returnAddress = (bits & 32) != 0;
            SalesGate gate = SalesGateRules.Evaluate(new SalesGateFacts(identityComplete ? [] : MissingIdentity, payment, switchOn, privacy, cookie, returnAddress));

            bool expectedOpen = identityComplete && payment && !switchOn && privacy && cookie && returnAddress;
            bool expectedDataCollection = identityComplete && privacy && cookie;

            Assert.Equal(expectedOpen, gate.IsOpen);
            Assert.Equal(expectedOpen, gate.Permits(SalesGateOperation.AddToCart));
            Assert.Equal(expectedOpen, gate.Permits(SalesGateOperation.ProceedToCheckout));
            Assert.Equal(expectedOpen, gate.Permits(SalesGateOperation.ConfirmOrder));
            Assert.Equal(!expectedOpen, gate.ShowsClosedNotice);
            Assert.Equal(expectedOpen, gate.SetupChecklist.Count == 0 && !gate.IsTemporarilyClosed);
            Assert.Equal(expectedDataCollection, gate.IsDataCollectionOpen);
            Assert.Equal(switchOn, gate.IsTemporarilyClosed);
            Assert.Equal(identityComplete, gate.MissingIdentityFields.Count == 0);
            Assert.True(gate.Permits(SalesGateOperation.BrowseShowcase));
            Assert.True(gate.Permits(SalesGateOperation.KeepExistingCart));
            Assert.True(gate.Permits(SalesGateOperation.ContinueOpenOrder));
            Assert.True(gate.Permits(DataCollectionEntry.ExistingAccountSignIn));
        }
    }

    [Fact]
    public async Task AnUnboundCondition_IsNotMet_SoTheGateStaysClosedUntilItsTaskBindsIt()
    {
        CancellationToken ct = TestContext.Current.CancellationToken;
        bool card = await new UnboundCardPaymentCondition().IsOpenAsync(ct);
        bool transfer = await new UnboundBankTransferCondition().IsOpenAsync(ct);
        bool notice = false; // bound by T366 (PrivacyNoticeCondition): with no published notice it is not met

        SalesGate gate = SalesGateRules.Evaluate(new SalesGateFacts([], card || transfer, false, notice, true, true));

        Assert.False(card);
        Assert.False(transfer);
        Assert.False(notice);
        Assert.Equal([SalesGateCondition.PaymentMethod, SalesGateCondition.PrivacyNotice], gate.UnmetConditions);
        Assert.False(gate.IsDataCollectionOpen);
    }

    [Fact]
    public void ASalesGate_HasNoPublicConstructorAndNoSetter_SoNoSurfaceBuildsOrCopiesOne()
    {
        Assert.Empty(typeof(SalesGate).GetConstructors(BindingFlags.Public | BindingFlags.Instance));
        Assert.All(
            typeof(SalesGate).GetProperties(BindingFlags.Public | BindingFlags.Instance),
            property => Assert.True(property.SetMethod is null, $"{property.Name} has a setter"));
    }

    [Fact]
    public void TheModuleSurface_GivesTheGateInOnePlace_AndTheSwitchChangesInTwoMethods()
    {
        MethodInfo[] methods = [.. typeof(ISettingsModule).GetMethods()];

        Assert.Single(methods, m => m.ReturnType == typeof(Task<SalesGate>));
        Assert.Equal(2, methods.Count(m => m.ReturnType == typeof(Task<SalesSwitchResult>)));
        Assert.All(methods.Where(m => m.ReturnType == typeof(Task<SalesSwitchResult>)), m => Assert.Contains(m.GetParameters(), p => p.ParameterType == typeof(bool)));
    }
}

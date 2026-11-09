using System.Reflection;

using Shopfolio.Infrastructure.Errors;
using Shopfolio.Modules.CorporateContent;
using Shopfolio.Modules.CorporateContent.Contracts;
using Shopfolio.Modules.CorporateContent.Contracts.Results;
using Shopfolio.Modules.Settings.Contracts;

namespace Shopfolio.UnitTests.CorporateContent;

/// <summary>
/// The contact form's closed shapes (T365; 02 3.32; 07 UC-15): the five topics, the fields the form carries and the ones it does not, the result that shows the same state for a send and
/// for a send the trap dropped, and the absence of a follow-up surface. The database-bound behavior is in the integration tests.
/// </summary>
public sealed class ContactFormTests
{
    /// <summary>The fields of 07 5.2 <c>SendContactRequestInput</c>: the five of 02 3.32.2 and the invisible trap.</summary>
    private static readonly string[] InputFields = ["Name", "Email", "Phone", "ContactTopic", "Message", "Website"];

    private static readonly string[] FollowUpWords = ["Reply", "Respond", "Thread", "Template", "Track", "Lookup", "Number", "Acknowledg", "Complaint", "Chat", "Live", "Attachment", "File", "OrderNumber"];

    [Fact]
    public void TheForm_CarriesTheFiveFieldsAndTheTrap_AndNoFileOrOrderNumberField_KK2()
    {
        string[] properties = [.. typeof(ContactFormInput).GetProperties().Select(p => p.Name).Order(StringComparer.Ordinal)];

        Assert.Equal(InputFields.Order(StringComparer.Ordinal), properties);
        Assert.DoesNotContain(properties, p => p.Contains("File", StringComparison.Ordinal) || p.Contains("Attachment", StringComparison.Ordinal) || p.Contains("Order", StringComparison.Ordinal));
    }

    [Fact]
    public void TheFieldNames_AreTheInputsProperties_AndTheTrapIsWebsite_KK8()
    {
        Assert.Equal("Website", ContactFormFields.TrapFieldName);
        string[] named = [.. typeof(ContactFormFields).GetFields(BindingFlags.Public | BindingFlags.Static).Select(f => (string)f.GetRawConstantValue()!)];

        Assert.Equal(InputFields.Order(StringComparer.Ordinal), named.Order(StringComparer.Ordinal));
    }

    [Fact]
    public void TheTopics_AreTheClosedFive_InTheOrderOfTheList_AndTheDefectClaimIsNotOne_KK3()
    {
        Assert.Equal(
            [ContactTopic.GeneralQuestion, ContactTopic.AboutOrder, ContactTopic.AboutProduct, ContactTopic.PersonalDataRequest, ContactTopic.Other],
            ContactFormSending.Topics);
        Assert.Equal(Enum.GetNames<ContactTopic>().Length, ContactFormSending.Topics.Count);
        Assert.DoesNotContain(Enum.GetNames<ContactTopic>(), n => n.Contains("Defect", StringComparison.Ordinal) || n.Contains("Complaint", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData("GeneralQuestion", ContactTopic.GeneralQuestion)]
    [InlineData("AboutOrder", ContactTopic.AboutOrder)]
    [InlineData(" AboutProduct ", ContactTopic.AboutProduct)]
    [InlineData("PersonalDataRequest", ContactTopic.PersonalDataRequest)]
    [InlineData("Other", ContactTopic.Other)]
    public void ATopicCodeOfTheList_IsRead_KK3(string code, ContactTopic expected)
    {
        Assert.Equal(expected, ContactFormSending.TopicOf(code));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("generalquestion")]
    [InlineData("1")]
    [InlineData("Genel soru")]
    [InlineData("DefectClaim")]
    [InlineData("GeneralQuestion,Other")]
    public void AValueOutsideTheList_IsNoTopic_KK3(string? code)
    {
        Assert.Null(ContactFormSending.TopicOf(code));
    }

    [Fact]
    public void ASendTheTrapDropped_ShowsTheSameStateAsASend_AndCarriesNoCodeOrField_KK8()
    {
        ContactFormResult sent = ContactFormResult.Sent();
        ContactFormResult dropped = ContactFormResult.Dropped();

        Assert.True(sent.ShowsSentState);
        Assert.True(dropped.ShowsSentState);
        Assert.Equal(sent.IsRejected, dropped.IsRejected);
        Assert.Equal(sent.Code, dropped.Code);
        Assert.Equal(sent.Fields, dropped.Fields);
        Assert.Equal(sent.Gate, dropped.Gate);
    }

    [Fact]
    public void TheRefusals_AreRejected_AndEachCarriesItsOwnCode_KK8()
    {
        ContactFormResult limit = ContactFormResult.LimitExceeded();
        ContactFormResult fields = ContactFormResult.FieldsRefused([new ContactFormFieldError("Name", ErrorCode.FieldRequired)]);
        ContactFormResult gate = ContactFormResult.GateClosed(new GateRefusal(ErrorCode.DataCollectionClosed, [], []));

        Assert.True(limit.IsRejected);
        Assert.Equal(ErrorCode.ContactRequestLimitExceeded, limit.Code);
        Assert.True(fields.IsRejected);
        Assert.Equal(ErrorCode.FieldRequired, fields.Code);
        Assert.True(gate.IsRejected);
        Assert.Equal(ErrorCode.DataCollectionClosed, gate.Code);
    }

    [Fact]
    public void TheContactSurface_HasNoReplyThreadTrackingComplaintOrChatMember_KK6_KK7_KK10()
    {
        string[] names =
        [
            .. typeof(ICorporateContentModule).GetMethods().Select(m => m.Name),
            .. typeof(ContactFormResult).GetProperties().Select(p => p.Name),
            .. typeof(ContactFormView).GetProperties().Select(p => p.Name),
            .. typeof(ContactFormInput).GetProperties().Select(p => p.Name),
            .. typeof(ContactFormOutcome).GetEnumNames(),
        ];

        foreach (string word in FollowUpWords)
        {
            Assert.DoesNotContain(names, n => n.Contains(word, StringComparison.OrdinalIgnoreCase));
        }
    }
}

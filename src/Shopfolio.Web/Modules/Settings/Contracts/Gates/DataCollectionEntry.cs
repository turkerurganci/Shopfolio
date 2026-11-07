namespace Shopfolio.Modules.Settings.Contracts.Gates;

/// <summary>An entry that collects personal data (02 3.13.19, 3.32.8, 3.33.9). The sign-in of an existing account collects nothing new and is never closed.</summary>
public enum DataCollectionEntry
{
    /// <summary>The account registration form.</summary>
    AccountRegistration,

    /// <summary>A Google sign-in that would open a new account.</summary>
    NewAccountGoogleSignIn,

    /// <summary>The contact form.</summary>
    ContactForm,

    /// <summary>A sign-in to an existing account, by password or by Google: not affected.</summary>
    ExistingAccountSignIn,
}

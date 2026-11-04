// Violation sample of the registration scan: Catalog and Ordering chained in one statement (line 3), Settings with an argument (line 9),
// Catalog a second time (line 10), Membership missing. Excluded from compilation; the scan test reads it as text.
builder.Services.AddCatalogModule().AddOrderingModule();
builder.Services.AddPaymentModule();
builder.Services.AddAfterSalesModule();
builder.Services.AddCorporateContentModule();
builder.Services.AddNotificationsModule();
builder.Services.AddAdministrationModule();
builder.Services.AddSettingsModule(options);
builder.Services.AddCatalogModule();

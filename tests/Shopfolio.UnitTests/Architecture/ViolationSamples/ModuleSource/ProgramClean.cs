// Clean sample of the registration scan: each module has one plain call. Excluded from compilation; the scan test reads it as text.
builder.Services.AddCatalogModule();
builder.Services.AddOrderingModule();
builder.Services.AddPaymentModule();
builder.Services.AddAfterSalesModule();
builder.Services.AddMembershipModule();
builder.Services.AddCorporateContentModule();
builder.Services.AddNotificationsModule();
builder.Services.AddAdministrationModule();
builder.Services.AddSettingsModule();

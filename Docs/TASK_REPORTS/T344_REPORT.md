# T344 — Adres: adres defteri, fatura adresi, kapalı il listesi, tipe göre istenen adresler, siparişe donma

**Faz:** F3 | **Durum:** ⏳ Devam ediyor (yapım tamam, doğrulama bekliyor) | **Tarih:** 2026-11-08

> Taslağı yapım chat'i doldurur; doğrulama chat'i finalize eder.

---

## Yapılan İşler

- **Settings:** `ProvinceList` (81 ilin okuması) ve `ISettingsModule.GetProvincesAsync`; il listesi tek yerden okunur, adres formu ve il denetimi aynı listeyi görür.
- **Membership:** `AddressFieldRules` (adres kurallarının tek evi: dört zorunlu alan, il listesi, telefon; `FieldRequiredAttribute` ve `InvalidPhoneAttribute` üstünden), `AddressBooks` (listele/al/oluştur/güncelle/sil ve `ResolveAsync`), `IMembershipModule` adres yöntemleri, sonuç ve istek kayıtları (`AddressRecords.cs`). Hiçbir yöntem `SaveChanges` çağırmaz; yazma işlemi dışında çağrılırsa `InvalidOperationException`.
- **Ordering:** `Order.FreezeAddresses(shipping?, billing)` adresleri siparişin kendi sütunlarına değer olarak yazar; siparişin deftere bağı yok.
- **Tipe göre istenen adres:** en az bir fiziksel kalem → teslimat + fatura (fatura teslimatla aynı olabilir); yalnız dijital/hizmet → yalnız fatura.
- **Testler:** `AddressFieldRulesTests` (birim), `AddressTests` (13 entegrasyon testi, gerçek PostgreSQL, 81 il).
- **Öz denetimde bulunup düzeltilen:** `DeleteAsync` `ExecuteDelete` ile işlemde hemen siliyordu; "yöntemler kaydetmez, işlemin sahibi kaydeder" kuralına (`09 §6` m.3, m.7) uysun diye izlenen varlığı `Remove` eder hâle getirildi ve "kaydetmeden commit" testiyle sınandı.

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Modules/Membership/` (`AddressBook.cs`, `AddressBooks.cs`, `AddressFieldRules.cs`, `Contracts/AddressRecords.cs`, `Contracts/IMembershipModule.cs`, `MembershipModule.cs`)
- `src/Shopfolio.Web/Modules/Settings/` (`ProvinceList.cs`, `Contracts/ProvinceRecords.cs`, `Contracts/ISettingsModule.cs`, `SettingsModule.cs`)
- `src/Shopfolio.Web/Modules/Ordering/Order.cs`
- `tests/Shopfolio.UnitTests/Membership/AddressFieldRulesTests.cs`, `tests/Shopfolio.IntegrationTests/Model/AddressTests.cs`, `MembershipTests.cs` (sahte ayara metot)
- `.claude/CONTEXT.md`, `.claude/memory/MEMORY.md`, `Docs/IMPLEMENTATION_STATUS.md` (yalnız tablo satırı), `Docs/STATUS_CHANGELOG.md`, `Docs/DEFERRED_BACKLOG.md` (D-435–D-437)

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Üye defterde birden çok adlandırılmış adres; misafirin defteri yok | ✓ | `AMember_KeepsSeveralNamedAddresses_OldestFirst_AndAGuestHasNoBook_KK1`, `AnEntryThatIsNotTheMembers_IsNotFound_...` |
| 2 | Yeni adres siparişe donar, deftere yalnız kaydetmeyi seçerse yazılır | ✓ | `ATypedAddress_GoesToTheBookOnlyWhenTheMemberChoseIt_...KK2`, `APickedBookEntry_IsUsedAsIs_...KK2` |
| 3 | Fatura varsayılan teslimatla aynı; ayrı fatura seçilebilir | ✓ | `TheBillingAddress_IsTheShippingOne_UnlessAnotherIsGiven_...KK3` |
| 4 | İl 81'lik kapalı listeden | ✓ | `TheProvince_IsOneOfTheClosedListOf81_...KK4` (81 il tek tek kaydedilir; bilinmeyen il `InvalidProvince`) |
| 5 | Fiziksel kalemli sipariş teslimat + fatura ister | ✓ | `AnOrderWithAPhysicalItem_AsksTwoAddresses_...KK5` |
| 6 | Yalnız dijital/hizmet teslimat alanı göstermez, yalnız fatura ister | ✓ | `ADigitalOrDeliveredByServiceOrder_HasNoShippingAddress_...KK6` |
| 7 | Sipariş adreslerini dondurur; defter değişikliği/silme dokunmaz | ✓ | `AnEditOrADeletionOfTheBookEntry_DoesNotChangeAnOrderThatFrozeIt_...KK7`; `orders`'ta `address_book` sütunu ve FK yok (information_schema) |
| 8 | Dört alan zorunlu, ilçe serbest metin | ✓ | `WhenOneOfTheFourFieldsIsEmpty_...KK8`; `AddressFieldRulesTests` |
| 9 | Teslimat telefonu zorunlu; defterde isteğe bağlı; fatura telefonsuz; posta kodu/T.C. yok | ✓ | `TheShippingAddress_AsksATelephone_...KK9`, `TheStoredAddresses_HaveNoPostalCode...KK9` |

### Öz denetim (bitiş kapısı 2)

| Madde (kabul kriteri / KK) | Sonuç | Kanıt (dosya:satır, test adı) |
|---|---|---|
| KK1 üye ikinci adresi adlandırır, misafirde defter yok | ✓ | `AddressTests` KK1 iki test; kimsesiz/misafir `Resolve` + `AddressBookId` → `IsBookEntryNotFound` |
| KK2 kaydetmeden kullanılan adres deftere girmez | ✓ | `AddressTests` KK2; reddedilen adres ve misafir de yazmaz |
| KK3 fatura seçilmezse teslimatla aynı | ✓ | `AddressTests` KK3 |
| KK4 listede olmayan il reddedilir, liste 81 | ✓ | `AddressTests` KK4 (`CountAsync("provinces") == 81`) |
| KK5 fiziksel siparişte iki adres, tüm hatalar birlikte | ✓ | `AddressTests` KK5 (9 alan hatası birlikte) |
| KK6 dijital siparişte teslimat yok, fatura var | ✓ | `AddressTests` KK6; "teslimatla aynı" kutusu dijitalde yok sayılır |
| KK7 sonradan düzenleme eski siparişi değiştirmez | ✓ | `AddressTests` KK7 (düzenleme ve silme sonrası sipariş sütunları aynı); "kaydetmeden commit" testi |
| KK8 dört alandan biri boşsa ret, satır yazılmaz | ✓ | `AddressTests` KK8 (oluşturma ve güncelleme) |
| KK9 telefon kuralları, fatura telefonsuz, posta kodu/T.C. alanı yok | ✓ | `AddressTests` KK9 iki test; `BillingAddressSnapshot` telefon alanı taşımaz |
| `09 §25` — modül sınırı, arayüz metodunda `SaveChanges` yok | ✓ | birim testi (mimari testler dahil) 4277/4277; `CommitWithoutSaving` testi |
| `09 §25` — tek işlev (telefon, zorunlu alan) | ✓ | `AddressFieldRules` `InvalidPhoneAttribute`, `FieldRequiredAttribute`, `RequestFormat.CompactPhone` çağırır; kendi telefon/zorunlu denetimi yok |
| Sık FAIL (1) tarama/bekçi istisnası geniş | ✓ | yeni tarama ya da istisna eklenmedi |
| Sık FAIL (2) tarama çağrı biçimi kaçırır | ✓ | yeni kaynak taraması yok |
| Sık FAIL (3) DB default/FK dizini | ✓ | şema değişmedi; `address_books` T107'den (06 §4.45), "Varsayılan" sütunu yok |
| Sık FAIL (4) aynı sayıyı anan öteki yerler | ✓ | `CONTEXT.md`, `MEMORY.md`, durum tablosu ve değişim günlüğü güncel; sayı/etiket değişmedi |
| Sık FAIL (5) tek kural evi | ✓ | yukarıdaki tek işlev satırı |
| Güvenlik taraması | ✓ | gizli bilgi yok, yeni bağımlılık yok, yetkilendirme: başkasının girdisi 404 (sahiplik sorguda), SQL parametreli (EF), kişisel veri kayda yazılmaz |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test --no-build -c Release` (UnitTests) | 4277/4277 (silme değişikliğinden önce; sonrasında yalnız modül kodu değişti, entegrasyon koştu) |
| Integration | ✓ | `--filter-class *AddressTests` ve `*MembershipTests` (postgres:18, `shopfolio-t344-pg`, 127.0.0.1:55437) | 13/13 ve 34/34 |
| Contract | — | — | bu task'ta sözleşme testi yok |
| Biçim | ✓ | `dotnet format --verify-no-changes` (Membership, AddressTests) | temiz |
| Derleme | ✓ | `dotnet build -c Release -warnaserror` (Web, UnitTests, IntegrationTests) | 0 uyarı, 0 hata |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | bekliyor |
| Bulgu sayısı | — |
| Düzeltme gerekli mi | — |
| Bloke-etmeyen not (K) sayısı | — |

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T344-adres`
- **PR:** #451
- **CI:** doğrulayıcı izler
- **Post-merge run'lar:** *(validator doldurur)*

## Dış Varsayımlar

Dış varsayım: yok. Entegrasyon testi için yerel `postgres:18` (CI'ın aynı imajı) kullanıldı.

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | `PlaceOrderInput`'ta deftere kaydederken ad alanı yok; kod `SavedAddressName` ekledi (boşsa alıcı adı) | D-435 | `07 §5.3` ve ödeme formu T509'un işi |
| 2 | Teslimat il kısıtı (`DeliveryProvinceNotServed`) | D-436 | T350'nin işi |
| 3 | `ResolveOrderAddressesAsync` + `FreezeAddresses` çağrısı sipariş akışında | D-437 | T355/T509/T518'in işi |

## Notlar

- **Working tree (Adım -1):** temiz.
- **Ana dal CI startup (Adım 0):** `gh run list --branch main`: Publish 42434282714 success, CI 42434282725 (T333 #442) devam ediyordu, Publish 42430286045 success. Önceki işçinin kapıları yazılmamıştı; bu devamda koşuldu.
- **Repo memory (bitiş kapısı 9):** güncellendi.
- **⚠ kolay kararlar:** (1) deftere kaydetmede ad: alıcı adı varsayılan, isteğe bağlı `SavedAddressName` (D-435); (2) misafir + `SaveToAddressBook` sessizce yok sayılır, misafir + `AddressBookId` 404; (3) girdi `customerId` hesapta yoksa misafir sayılır; (4) dijital sepette gönderilen teslimat değerleri okunmaz, "teslimatla aynı" kutusu yok sayılıp fatura yazılı istenir.
- `IMPLEMENTATION_STATUS.md` "Son güncelleme" satırına dokunulmadı (kural 2026-11-08).

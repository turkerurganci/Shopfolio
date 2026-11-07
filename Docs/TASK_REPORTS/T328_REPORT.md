# T328 — Bildirim haritası: tetikleyici eşlemesi, bildirim üretmeyen olaylar ve eşitlik denetimi

**Faz:** F3 | **Durum:** ✓ Tamamlandı (doğrulama tur 1 ✓ PASS) | **Tarih:** 2026-11-07

> Taslağı yapım chat'i doldurur; doğrulama chat'i finalize eder.

---

## Yapılan İşler

- Bildirim haritası kod olarak yazıldı: `NotificationMap` (aile, adres türü, `NoticeTrigger`, `NoNoticeRow`, `EventMapping`), 03 §7.1 (55), §7.2 (30), §7.3 (47) satırı `NotificationMap.Rows.cs` içinde. 7.1.37 (yeniden gönderim) türsüz satırdır.
- Dört kuyruk yolu haritaya bağlandı: `CustomerNoticeQueue.QueueAsync` (müşteri), `CompanyNoticeQueue.QueueCompanyAsync` (firma), `QueueAdmins` (yöneticiler), `AccountEmailQueue.Make` (hesap) satır yazmadan önce `NotificationMap.Require` çağırır; haritada olmayan tür ya da başka ailenin yolundan gelen tür veri yazmadan `InvalidOperationException` ile reddedilir.
- Eşitlik denetimi (Unit, Consistency): haritanın 7.1, 7.2, 7.3 satırları 03 ile iki yönde; her geçiş kodu (düzeltme hariç) bir 7.3 satırında; gönderim türü adları belgeyle; adres türü kuyruk ailesiyle; akış tablolarındaki B-n/F-n bildirimleri ile haritanın bağladığı bildirimler iki yönde.
- Kanal taraması (Unit, Architecture): SMS, anlık bildirim, bildirim merkezi, pazarlama/İYS/bülten/onay, işlem bildirimini kapatan ayar; `IEmailGateway` ve `DispatchRecord.Queue` yalnız tam dosya listesinde; ihlal örnek dosyası var.
- Integration: `NotificationMapQueueTests` gerçek kuyruklardan satır yazar ve saklanan adresi haritanın adlandırdığıyla karşılaştırır.
- Devam işçisi: derleme hatası (CA1859) düzeltildi; iki Consistency testi belgeye göre düzeltildi (aşağıda ⚠); Integration testi yazıldı.

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Modules/Notifications/NotificationMap.cs`, `NotificationMap.Rows.cs`, `CustomerNoticeQueue.cs`, `CompanyNoticeQueue.cs`, `AccountEmailQueue.cs`
- `tests/Shopfolio.UnitTests/Consistency/NotificationMapDocument.cs`, `NotificationMapConsistencyTests.cs`
- `tests/Shopfolio.UnitTests/Notifications/NotificationMapTests.cs`
- `tests/Shopfolio.UnitTests/Architecture/NotificationChannelSourceScan.cs`, `NotificationChannelSourceScanTests.cs`, `ViolationSamples/NotificationChannel/Misuse.cs`
- `tests/Shopfolio.IntegrationTests/Model/NotificationMapQueueTests.cs`
- `Docs/DEFERRED_BACKLOG.md` (D-295, D-296), `Docs/IMPLEMENTATION_STATUS.md`, `.claude/memory/MEMORY.md`

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Her geçiş ve olay tek eşleme tablosunda bildirim ya da "bildirim yok" satırına bağlı | ✓ | `NotificationMap.Events` (47 satır); `EveryTransition_HasARowOf7_3_AndEveryTransitionRowOf7_3_IsATransition`, `TheEvents_EqualTheRowsOf7_3_WithTheRowsTheyName_InBothDirections` |
| 2 | B-1…B-16, F-1…F-6, hesap ve yönetim e-postaları yalnız haritadaki tetikleyiciden | ✓ | `Require` üç kuyrukta + hesap kuyruğunda; `NotificationMapTests` (haritasız ve yanlış aile reddi); `NotificationChannelSourceScanTests` (kuyruk dışı yazım yok) |
| 3 | Bildirimsiz olaylar kaynağıyla "bildirim yok" satırı, tetikleyicisi yok | ✓ | `NotificationMap.NoNotices` (30 satır, kaynaklı); `TheNoNoticeRows_EqualTheRowsOf7_2_WithTheirSources_InBothDirections` |
| 4 | Tek kanal e-posta; SMS, push, bildirim merkezi, tercih ekranı yok | ✓ | `NotificationChannelSourceScanTests`; `EveryNoticeOf7_1_GoesByEmail_TheOnlyChannel` (Consistency) |
| 5 | İşlem bildirimleri kapatılamaz, ticari ileti değil; pazarlama/İYS/onay yok | ✓ | tarama kuralları 4–5 ve ihlal örneği `Misuse.cs`; 7.2.23 satırı "bildirim yok" |
| 6 | Adres kuralı (müşteri sipariş e-postası, B-15 eski adres, F-1…F-4 firma, F-5/F-6 her yönetici, hesap) | ✓ | `NotificationMapQueueTests` (3 test); `NotificationMapTests` adres türleri; `EveryTrigger_AddressesTheKindOfAddressItsQueuePathHas` |
| 7 | Eşitlik denetimi akış ↔ harita iki yönde; fazla/eksik CI'ı kırar | ✓ | `TheNoticesOfTheFlowTables_AndTheNoticesTheMapTiesToThem_AreTheSame_InBothDirections`; `TheExceptions_AreStillNeeded` (bayat istisna kırar) |

### Öz denetim (bitiş kapısı 2)

| Madde | Sonuç | Kanıt |
|---|---|---|
| Kabul 1–7 | ✓ | yukarıdaki tablo |
| KK1 her geçiş bir satıra bağlı | ✓ | `EveryTransition_HasARowOf7_3…`; 7.3'ün her 7.1 satırını andığı denetimi |
| KK2 eşleme dışı tetikleyiciden e-posta çıkmaz | ✓ | `Require` + `NotificationMapTests`; tarama: `IEmailGateway` / `DispatchRecord.Queue` / `DispatchRecords.Add` yalnız tam dosya listesinde |
| KK3 bildirimsiz olay ve olmayan bildirim kaynağıyla duruyor, tetikleyicisi yok | ✓ | `Assert.All(NoNotices, n => Assert.NotEmpty(n.Sources))`; `ARowOf7_3_ThatSaysNoNotice_NamesNoRowOf7_1`; 7.2 satırı `Triggers` içinde yok |
| KK4 SMS, push, merkez, tercih ekranı yok | ✓ | `NotificationChannelSourceScanTests`, ihlal örneği ayrı dosya |
| KK5 kapatan ayar, pazarlama tetikleyicisi, İYS yok | ✓ | aynı tarama (kural 4–5) |
| KK6 adres seçimi doğru, B-15 eski adres | ✓ | `NotificationMapQueueTests.TheCustomerNotices_AreWrittenToTheAddressTheMapNames…` (B-15 tek satır, eski adres), `…F1ToF4…`, `…F5AndF6…` |
| KK7 haritada olup akışta olmayan bildirim denetimi kırar | ✓ | akış ↔ harita testi "names … which no trigger row" ve "does not name it" iki yönü; test beklenen değerleri belgeden türetilir (sabit sayı yok) |

Sık FAIL nedenleri: (1) tarama istisnası yalnız tam dosya yolu, sözcük istisnası yok; (2) eşitlik denetimi iki yönlü, istisna üç çift ve her biri `TheExceptions_AreStillNeeded` ile bayatlayınca kırılır; (3) DB varsayılanı/FK: bu task'ta şema yok; (4) aynı sayıyı anan yerler: 06'da "B-1…B-16 (37 satır)" yazılı olan, 03 §7.1'in 55 satırından farklı bir sayımdır (06 türleri sayar); belgeye dokunulmadı; (5) test beklenen değerleri belgeden ve haritadan türetilir.

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit (tümü) | ✓ | `dotnet test tests/Shopfolio.UnitTests --no-build` | 3987 geçti, 0 kaldı |
| Unit (bildirim sınıfları) | ✓ | `--filter-class "*Notification*"` | 395 geçti |
| Integration | ✓ | `--filter-class "*NotificationMapQueueTests"` | 3 geçti |
| Integration (komşu) | ✓ | `*NoticeQueueTests`, `*AccountEmailQueueTests` | 39 geçti (Require çağrıları kırmadı) |

Kendi PostgreSQL konteyneri (`shopfolio-t328-pg`, 127.0.0.1:55371) adıyla silindi.

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | ✓ PASS (tur 1, 2026-11-07, head `0edb3bd`) |
| Bulgu sayısı | 0 |
| Düzeltme gerekli mi | Hayır |
| Bloke-etmeyen not (K) sayısı | 1 (K1: D-295 sayımı ve backlog aktif sayısı bu PR’da düzeltildi) |

**Validator kanıtları (tur 1):**

- Başlangıç kapıları: working tree temiz; `core.hooksPath` `scripts/git-hooks`; ana dal son üç run (42393301185, 42393301195, 42391220711) success; MEMORY'de T328 satırı var.
- Kriter 1–7 ve KK1–KK7 ✓. KK7 için iki mutasyon koşuldu: koda fazladan `7.1.99` (B-16) satırı eklenince üç Consistency testi kırıldı ("only in the model: 7.1.99"); `03`'te 7.1.36'nın Akış sütunundan `§5.3.1.2` silinince akış ↔ harita testi kırıldı ("5.3.1.2 names B16, which no trigger row of the map ties to it"). İkisi de geri alındı.
- KK2: `DispatchRecord.Queue` yalnız üç kuyruk dosyasında çağrılıyor (grep), dördü de (`QueueAsync`, `QueueCompanyAsync`, `QueueAdmins`, `AccountEmailQueue.Make`) önce `Require` çağırıyor; kaynak taraması kuyruk ve ağ geçidi dışını kırar.
- KK6: müşteri bildiriminin adresi sözleşme gereği çağırandan gelir (`INotificationsModule.QueueCustomerNoticeAsync`: B-1 düzeltmede yeni, B-15 eski adres); düzeltme akışı T375'in, yeniden gönderim T385'in işidir. Harita adres türünü belgeyle eşit tutar, kuyruk saklanan adresi doğru yazar.
- `12`: task'ın kaynak birimi `03 §7.1`–§7.3; matriste bu birimlere bağlı satır yok, `Durum` değişmedi.
- Yerel: restore, build `-warnaserror` (0 uyarı), format (3 proje) temiz; Unit Consistency 241, öteki 3746; betik testleri 4 dosya; Integration `NotificationMapQueueTests` 3, komşu kuyruk sınıfları 47 (kendi konteyneri `shopfolio-t328-val-pg`, adıyla silindi).
- Yapım raporuyla uyum: tam; iki küçük sayım hatası (D-295 "üç/dört yer" → iki/üç; "istisna dört çift" → üç çift) ve backlog "Aktif kalem" 125 → 127 bu turda düzeltildi.

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T328-bildirim-haritasi`
- **Commit:** PR'ın head SHA'sı PR gövdesinde
- **PR:** #440
- **CI:** dal run 42394956374 — Lint, Build, Unit, Integration, Contract, Migration, Paketleme, E2E, CI Gate yeşil
- **Post-merge run'lar:** squash `fe78e7b` (#440) — CI 42405001882 success, Publish 42405002224 success (T336 yapımında dolduruldu)

## Dış Varsayımlar

Dış varsayım: yok. Task yalnız uygulama içi kod ve belge eşitliğidir.

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | 03 §7.1 "Akış" sütunu: 8.3.3.4 (B-8) ve 10.1.2.6 (B-15) satırlarını anmıyor, 7.1.6'nın 4.2.11 hücresi kodsuz | D-295 | belge düzeltmesi bu task'ın kapsamı dışı; dört dar istisna testte gerekçeli |
| 2 | Hesap e-postaları akış tablolarında adla geçer; 7.3 hücre metni ile 7.1 türleri arası alt küme denetimi yok | D-296 | belgede kod yok; denetim genişletilemez |
| 3 | Yeniden gönderim (7.1.37) yazıcısı yok, haritada satır var | D-293 (T307) | gönderen task'ı |

## Notlar

- **Working tree (Adım -1):** temiz.
- **Ana dal CI startup (Adım 0):** `gh run list --branch main --limit 3`: son üç run (CI 42393301185, Publish 42393301195, Publish 42391220720) success.
- **Repo memory (bitiş kapısı 9):** güncellendi.
- **⚠ kolay kararlar (kurala en uygun seçenekle verildi):**
  - Akış ↔ harita karşılaştırması yalnız B-n/F-n kodları için; hesap e-postaları akışta adla geçtiğinden kapsam dışı (D-296).
  - `EveryRowOf7_3_NamesOnlyRowsThatExist…` testinin ters yönü yalnız 7.1 satırlarını zorunlu tutar: 03 §7.3 durum geçişi ve kayıt olaylarını sayar, "süre dolması", "iletişim formu", "pazarlama iletisi", "veri ihlali" gibi 7.2 satırlarını anmaz; bunlar geçiş değil sürekli ifadedir. 7.3'ün andığı her satır var olmalıdır (ileri yön kalır).
  - `AllRowIds` numaralı başlıkları da kimlik sayar: 7.1.29 "§2.10.4" ile bir bölümü anar, tablo satırını değil.
  - 7.1.37 (yeniden gönderim) türsüz satır; müşteri ailesinin bütün türlerini ve eski adresi kapsar.
  - Integration testinde hesap e-postaları için yeni test yazılmadı (link kaydı tohumlamak ağır); `AccountEmailQueueTests` kapsar.

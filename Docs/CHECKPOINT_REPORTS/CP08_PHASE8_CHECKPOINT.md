# Aşama 8 — Checkpoint (CP08)

**Tarih:** 2026-10-30 | **Aşama:** 8 — Kodlama kılavuzu | **Kapanış adımı:** K-437'nin 4. adımı (`/checkpoint`; `checklists/document-stage.md` §7)
**Girdi sürümleri:** karar kaydı v0.221 · Kodlama Kılavuzu (`09`) v0.23 · Ürün Gereksinimleri (`02`) v0.103 · Teknik Mimari (`05`) v0.40 · Veri Modeli (`06`) v0.42 · API Tasarımı (`07`) v0.32 · Entegrasyon Spesifikasyonları (`08`) v0.18 · MVP Kapsamı (`10`) v0.58 · `11` ve `12` şablon v0.1 (park blokları) · `DEPLOY_RUNBOOK` — `origin/main` @ `43f9d1a`
**Çıktı sürümleri:** karar kaydı v0.222 · `09` v0.24 · `06` v0.43 · öteki dokümanlar değişmedi
**Girdi raporu:** [Aşama 8 — Çakışma Taraması](PHASE8_CONFLICT_SCAN.md) (K-437'nin 3. adımı) — açık kalem durumu ve devredilen sınıf (özet ↔ ayrıntı; §8) oradan alındı; audit, deep review, üç cross-review turu ve etki yansıtmanın uygulanmış bulguları ikinci kez açılmadı.

> **Ne işe yarar:** Aşama kapanmadan önce Kodlama Kılavuzu'nun kendi içinde, Aşama 8'in geri beslediği dokümanlarla (`02`, `05`, `06`) ve park metinleriyle (`11`, `12`, `DEPLOY_RUNBOOK`) karar kaydına karşı tutarlı olduğunu doğrular; bulunan her uyumsuzluğu ya kapatır ya da adıyla proje sahibine bırakır. Sonuç öğrenim terfisinin (5. adım) ve arşiv işaretinin (6. adım) girdisidir.

---

## Checkpoint Sonucu — 2026-10-30

**Aşama:** 8 — Kodlama kılavuzu (kapanış sırası K-437: yazım turu ✓ · kalite döngüsü ✓ · çakışma taraması ✓ · **checkpoint** · öğrenim terfisi · arşiv işareti)
**Genel durum:** ⚠ Dikkat gerektiren noktalar — 11 mercek ve 3 betik 56 ayrı bulgu getirdi (55 ham bulgu ve bir doğrulayıcının gözlemi); her biri Opus / `high` doğrulayıcıdan geçti: **33 doğru**, **16 kısmen doğru**, **7 çürütüldü**. Doğrulanan 49 bulgunun 48'i bu checkpoint'in PR'ında kapandı; biri (S3-10) dayanağı okunamadığı için uygulanmadı ve proje sahibine sorulacak. Üç yeni karar var (K-1394, K-1395, K-1396); ikisi ⚠. Çok kritik konu yok. Yeniden doğrulama borcu yok.

### Kontrol Özeti

| # | Kontrol | Sonuç | Detay |
|---|---|---|---|
| 1 | Yol haritası | ✓ | Aşama 8'deyiz: yazım turu, kalite döngüsü (cross-review Faz 5 etki yansıtması #223) ve çakışma taraması (#224) bu sırayla kapandı; atlanan adım yok (git geçmişi ve `PHASE8_CONFLICT_SCAN.md`). |
| 2 | Doküman durumu | ✓ | Betikle: `02` v0.103, `05` v0.40, `06` v0.42, `07` v0.32, `08` v0.18, `09` v0.23 (⏳), `10` v0.58 — başlık, alt bilgi ve tracker §1 birebir. `11`, `12` şablon v0.1 (⬚; park blokları açık). `DEPLOY_RUNBOOK` sürüm alanı taşımayan operasyon dokümanıdır. Bulunan iki fark: `09` başlık notunda "Sırada: çakışma taraması" kalmıştı (S1-02); tracker §1'in `06` satırı bölüm numarasını yanlış gösteriyordu (M5-4). İkisi de kapandı; `09` v0.24, `06` v0.43 oldu. |
| 3 | Tutarsızlık | ⚠ → ✓ | Devredilen sınıf (özet ↔ ayrıntı) altı dilimli iç tutarlılık merceğiyle `09`'un tamamında arandı (S1–S6): 33 bulgu doğrulandı (§3). En ağırları: iki aşamanın tarama cümlesi "kalan eşleşme yoktur" derken hata satırının alanları ve tek yazıcının mimari testi eski hâlinde kalmıştı (M5-2, M5-3, S2-1, S2-2); kaynak taramalarının kendi izinli eşleşmelerini yasaklaması (S3-01, S3-02, S3-03, S4-2); oturumsuz çıkışın `07 §2.3.2` istisnasının yazılmaması (S4-3); `tr-TR` kültürü için iki karşıt "tek yer" (V-1). |
| 4 | Açık kararlar | ✓ | Tracker §4'te açık satır yok, A-19 açılmadı; dokümanlarda açık kalem yok. Bir yeni açık süreç maddesi eklendi: S3-10 (§5). |
| 5 | Aşama çıktıları | ✓ | `09` yazıldı (25 bölüm); geri besleme tablosunun on dört satırı hedeflerde duruyor (M5); `11` ve `12` park blokları var. Tabloya bir satır eklendi (`06 §4.63`). |
| 6 | Geriye dönük etki | ⚠ → ✓ | Etki sütunu ↔ hedef (M1; 101 aday ve 5 B adayı): ham 1 bulgu, çürütüldü (K-1319). Geri besleme: `06 §4.63` eski cümlesini taşıyordu (M5-1) — `06` v0.43. `05 §8.4.3` bulgusu (M3-1) çürütüldü. |
| 7 | Yeni alan/kural | ✓ | 586 ad, 379 aday: ham 1 bulgu (`UploadChunkSizeBytes`, ifade; kısmen). Değer tablosu (§3.5) ↔ `05 §8.4` ve `02 §11`: hız sınırı, piksel tavanı, indirme ömrü, zamanlayıcılar, yeniden deneme dizisi, disk eşiği tutarlı. Kütüphaneler tablosu ↔ gövde: iki bulgu (M4-1, M4-3). |

### Aksiyon Gerektiren Maddeler

- [x] 49 doğrulanan bulgunun 48'i düzeltmeyle kapandı — `09` v0.24, `06` v0.43, tracker §1, karar kaydı v0.222 (§3)
- [x] Yeni kararlar: K-1393 (düzeltmelerin kaydı), K-1394 (⚠), K-1395, K-1396 (⚠)
- [x] Dokunulan Aşama 8 kararlarının etki sütununa `(checkpoint — CP08 …)` işareti eklendi (§3.3)
- [x] **S3-10 — dışa aktarmada sitenin alan adı sütunu:** yönetici K-546'nın yansıması olarak uyguladı (§5)
- [x] **⚠ listesinin proje sahibine gösterilmesi** — 22 karar; kapı: arşiv işaretinden önce, tek mesajda (`INSTRUCTIONS.md` §2) — **yapıldı (2026-10-30, K-1400): itiraz yok**
- [x] Sırada öğrenim terfisi (K-437'nin 5. adımı) — tracker §20.1'in adayları (5. aday bu checkpoint'te eklendi, §6), Ö-23 ve belirteç ölçümünün karşılaştırması — **yapıldı (K-1397…K-1399)**

### Notlar

**Yöntem.** Mekanik kontroller betikle koştu (`scripts/doc-scans/09_checkpoint_paketle.py`, `09_checkpoint_dogrulama_paketle.py`, `09_checkpoint_sonuc_derle.py`): sürümler, K tekilliği, K-1302…K-1392'nin etki sütunundaki hedeflerin karar numarasını anması, `09`'un adlarının `06`/`07`'de aranması. Anlamsal tarama on bir salt okuma merceğiyle ve beşli dalgada koştu: **M1** etki sütunu ↔ hedef (1 ham bulgu) · **M2** adlar ↔ `06`/`07`/üst dokümanlar (1) · **M3** §3.5 değer tablosu ↔ `05 §8.4` ve `02 §11` (4) · **M4** Kütüphaneler tablosu ↔ gövde (3) · **M5** geri besleme tablosu ↔ hedeflerin metni (5) · **S1…S6** `09`'un altı diliminde özet ↔ ayrıntı (6, 6, 10, 12, 4, 3). 55 ham bulgu ve bir doğrulayıcı gözlemi (V-1) 56 ayrı bulguya indi. Doğrulama kademeliydi (yüksek tek, orta 2–4, düşük 5–8): **15 doğrulayıcı** (Opus / `high`), sınır 20; kademesi yükselen beş bulgu ve V-1 yeni kademede yeniden doğrulandı. Şartnameler betikle uygulandı: her eski parça hedef dosyada tam bir kez geçti (73 parça, hata sıfır; Park kısaltmalarının 26 çifti ayrı sınandı). **Model dağılımı:** mercekler Sonnet / `low` (11); doğrulayıcılar Opus / `high` (15).

**Mercek kapsamı.** M1 merceği 101 A adayının tamamını derin okumadı (geri besleme, genişletme ve daraltma taşıyanlar, park blokları ve B adayları okundu; kalanlarda hedefin varlığı ve terim çelişkisi arandı). S1 paketi sondan kesilmişti; mercek kalan kısmı dosyadan kendisi okudu. Bir paketteki "Kontrol edilip tutarlı bulunanlar" başlığı bulgu değildir (betik ayrıştırma artığı), sayıma girmedi.

**Süreç ihlalleri (heredoc).** Ortak kuralın (heredoc yasak) ihlalleri: bir mercek (kayıt eklerken) ve yedi doğrulayıcı (3, 4, 8, 9, 10, 12 ve 13) hüküm dosyasına metin eklerken heredoc kullandı. Hook yalnız betik ve yorumlayıcıya giden heredoc'u engelliyor, bu kullanımlar çalışma dosyasına yazdı; repoya dokunulmadı, sonuç etkilenmedi. Yürütücü bir denemede (`python -I … <<`) engellendi, komut çalışmadı. Doğrulayıcı 7 bir komutunun takıldığını bildirdi (durduruldu).

**Betikle doğrulanıp tutarlı çıkanlar:** sürümler (§Kontrol 2); karar kaydı K tekilliği (K-1393…K-1396 dahil tekrar yok) ve altı sütun; değişen dosyalarda CR, kontrol karakteri ve `$[0-9]` artığı sıfır.

---

## 3. Bulgular ve kapanışları

Seviye: **orta** — iki yer çelişiyor; kapalı liste, sayı ya da hedef yanlış; ya da kuralın evinde kural eksik; **düşük** — ifade, kapsam ya da atıf farkı. Hüküm: **D** doğru · **K** kısmen doğru. Bulgu kimlikleri çalışma dosyalarınındır; `09` yerleri bölüm numarasıyla verilir.

### 3.1 Doğrulanan ve uygulanan bulgular (48)

| # | Yer | Bulgu | Kapanış | Hüküm / seviye |
|---|---|---|---|---|
| M4-1 | `09` §15.6 madde 8 | Yedek kütüphaneye geçiş yalnız beş davranış kapısını sayıyordu; K-1375 (2) gömülü kütüphanenin kopyaleft çıkmasını da tutmamış kapı sayar | Madde 8'e koşul eklendi | K / düşük ⚠ |
| M4-3 | `09` §18 madde 4 | Serilog kod yorumu tabloda olmayan "Serilog.Settings" paketine karşı doğrulama diyordu | Serilog ve Serilog.Sinks.File | D / düşük |
| M5-1 | `06` §4.63 | "Veritabanı kendi saatini okumaz" cümlesi K-1374 (1) ile (gövde saati üst sınır olarak okur) çelişiyordu | Cümle karara eşitlendi (`06` v0.43); `09 §2` tablosuna satır | D / orta |
| M5-2 ≡ S2-2 | `09` §9.1 madde 8 | Mimari test "kalemin" durum yazan yöntemini çağıranı arıyordu; K-1390 kalemi çıkarmıştı | "kalem kaydının" | D / orta |
| M5-3 ≡ S2-1 | `09` §8.1 madde 7 | Hata satırının alan sayımı `05 §10.3.6`'nın K-1389 (f) öncesi hâliydi | İç istisna türleri ve SQLSTATE eklendi | D / orta |
| M5-4 | tracker §1 `06` satırı | K-1318'in bölüm numarası §2.10.2 kalmıştı | §2.10.1 | D / düşük |
| M5-5 | `09` §13.2 madde 5 | "Diske inme" iki yerde iki okumayla | §18.1 madde 5'e bağlandı, `Flush(true)` ayrıca belirtildi | K / düşük |
| S1-01 | `09` §6 madde 5 | Tek yazıcı "mimari testle sınanamaz" diyordu; §9.1 madde 8 sınar | Cümle K-1308 bağlamına daraltıldı | D / orta |
| S1-02 | `09` başlık notu | Yazım durumu çakışma taramasını sırada gösteriyordu | Güncellendi | D / düşük |
| S1-03 | `09` §3 ve §15.7 Kaynak | K-1392 Kaynak satırlarında yoktu | Üç yerde eklendi | K / düşük |
| S1-04 ≡ S3-09 | `09` §10.7 madde 1 | "Öteki satırların" listesi tablonun on üç satırını anmıyordu | "örneğin" | D / düşük |
| S1-05 | `09` gövdesi | Yazım konvansiyonu 4 park kısaltmalarını gövdeden dışlıyordu; 26 yerde duruyordu | 26 yerde çıkarıldı, konvansiyon değişmedi | K / düşük |
| S1-06 | `09` §10.10 madde 2 | `tr-TR` verisi "bulunmayabilir" gerekçesi §7.2 ile ters | Gerekçe hizalandı | D / düşük |
| S2-3 | `09` §8.1 madde 6 | Bastırma gerekçesi atfı madde 3'tü | Madde 5 | D / düşük |
| S2-4 | `09` §10.2 Test | Kilit sırası "test sınar" denmişti, test sınamıyordu | İki koşul teste eklendi | D / orta |
| S2-5 | `09` §8.2 madde 5 | Yanıt sınaması altı hâlle daraltılmıştı (K-1334 (11) her hâl) | 400, 413, 503 eklendi | D / düşük |
| S2-6 | `09` §10.2 Test | `FOR SHARE` taraması testte sayılmıyordu | Ayrı, `OrderLocks.cs`'e bağlı yazıldı | K / düşük |
| S3-01 | `09` §10.6 madde 11 | `audit_logs` taraması imha fonksiyonunun `DELETE`'ini ve `REVOKE` kalıbını yasaklıyordu | İzinli iki eşleşme adıyla yazıldı | D / orta |
| S3-02 | `09` §10.8 madde 7 | `REVOKE … FROM PUBLIC` kalıbı taramaya takılıyordu | Biçim adıyla yazıldı | K / orta |
| S3-03 | `09` §10.6 madde 11 | Tarama listesinde `VACUUM`, `ALTER SYSTEM`, `CREATE DATABASE` yoktu | Eklendi | D / orta |
| S3-04 | `09` §10.8 madde 7 | Salt okuma testi göç geçmişi tablosunda `SELECT` bekliyordu | İstisna yazıldı | K / düşük |
| S3-05 | `09` §12 madde 8 | `AuditOperation` kalıcı enum ama kısıtsız | İstisna yazıldı | D / düşük |
| S3-06 | `09` §12 madde 6 | Sağlayıcıya sipariş numarası biçiminin evi §11.1 değil `08 §2.1.4` | Atıf düzeltildi | D / düşük |
| S3-07 | `09` §11.3 | Süre ve tolerans değerleri §3.5'in "yinelemez" kuralına aykırı yinelenmişti | Atıfa çevrildi | D / düşük |
| S3-08 | `09` §11.4 madde 4 | Çağıranlar `Fill`'i doğrudan çağırır gibi yazılmıştı | `IOrderDocuments.GetTextAsync` | D / düşük |
| S4-1 | `09` §15.2 madde 4 | "Üç sayfa … her biri nitelikle" — iki sayfa niteliklidir | "İki" | D / orta |
| S4-2 | `09` §15.1 madde 1 | Anahtar taraması kararlarla yazılı dört kullanımı yasaklıyordu | Kapalı istisna listesi (K-1396) ⚠ | D / orta |
| S4-3 | `09` §15.1 madde 2 | Oturumsuz çıkışın `07 §2.3.2` istisnası yoktu | İstisna ve test eklendi; açık sayfa listesi değişmedi | K / orta |
| S4-4 | `09` §13.2 komut tablosu | `--filter` IP ve e-postayı argümana koyar gibi okunuyordu | Kişisel süzgeçler terminalden (K-1394) ⚠ | K / orta |
| S4-5 | `09` §13.2 madde 7 | Bin satır iki turda biter, "üç tur" yazıyordu | "iki" | D / orta |
| S4-6 | `09` §15.4 madde 5 | `AttemptCounter` eşikleri arasında P-37 sayılmıştı (L-7 bu sınıfı kullanmaz) | P-31…P-36 ve P-49 | D / düşük |
| S4-7 | `09` §15.3 madde 1 | Kaçış biçimi yerinde düz `<` yazılıydı | `<`, `>`, `&` | D / düşük |
| S4-8 | `09` §15.5 madde 4 | "Süren aktarım kesilmez" atfı §15.7'ye gidiyordu, kural `05 §7.7.4`'te | Atıf düzeltildi | K / düşük |
| S4-9 | `09` §14.1 madde 4 | "Ara adımda yuvarlama yoktur" mutlaktı; `02 §3.8.3` iki ara adımı yuvarlar | Başlık maddenin kendi cümlesine hizalandı | D / düşük |
| S4-11 | `09` §13.1 madde 10 | `Ignore` testi `Infrastructure/Jobs/` içinde aramıyordu | Mutlak yazıldı | K / düşük |
| S4-12 | `09` §11.1 madde 4 | Nabız isteği `UserWaiting` tanımının dışındaydı | Tanıma eklendi | K / düşük |
| S5-01 | `09` §15.6 madde 7 | Adet reddi `ImageLimitExceeded` (`07 §6.2`); `09` hepsini `InvalidImage` yazıyordu | İki kod ayrıldı | D / orta |
| S5-02 | `09` §17 madde 4 | "Üç adım" — sıra dört adımdır | "Dört" | D / düşük |
| S5-03 | `09` §19.2 madde 3 | Tarama ve mimari test ev bölümleri listesi eksikti | §3.4, §13, §16, §20 eklendi | K / düşük |
| S5-04 | `09` §15.7 madde 6 | Adım sırası `07 §4.31.2` ve kendi "Sıra" cümlesiyle ters | (b) ve (c) yer değiştirdi | D / düşük |
| S6-1 | `09` §24 madde 4 | Tek işlev tablosu §9.4'ün üç işlevini ve `SearchTerm`'i taşımıyordu | 13 → 17 satır | D / orta |
| S6-2 | `09` §23 giriş | "Tek listede sayar" — listede olmayan yasaklar var | "özetler; liste kapalı değildir" | K / düşük |
| S6-3 | `09` §23 | `Down` ve `now()` yasağının evi madde 1 | Atıf eklendi | D / düşük |
| `UploadChunkSizeBytes` | `09` §15.3 madde 4 | Tek ad iki tavanı kapsar gibi okunuyordu | İfade ayrıldı | K / düşük |
| V-1 | `09` §9.4 madde 5 | `tr-TR` için iki karşıt "tek yer" (K-1333 (6), K-1336 (11)) | `RequestFormat.Culture` (K-1395) | D / orta |

### 3.2 Uygulanmayan ve elenenler

| # | Bulgu | Durum |
|---|---|---|
| S3-10 | Dışa aktarmada sitenin alan adı sütunu eksik (`09` §10.10 madde 5); `Moment` türünün kullanılmaması kısmı çürütüldü | **Uygulanmadı — doğrulanamadı.** Doğrulayıcı gerekçeyi e-Arşiv faturanın internet satışı bilgisine bağladı. Resmî tebliğ (509 sıra no.lu VUK Genel Tebliği) iki kaynaktan (mevzuat.gov.tr ve ebelge.gib.gov.tr) indirildi, ancak PDF'in metni çıkarılamadı; bir arama özeti resmî metin sayılmadı. K-546 ve K-1342 (7) olduğu gibi kalır. **Sonradan uygulandı (yönetici):** K-546 sütuna zaten karar vermişti; uygulama kaynak kararın yansımasıdır, yasal dayanak yeniden açılmaz — `09 §10.10` madde 4 ve 5 |
| K-1319 | `05`'in K-1319'a dayanan cümlesinin etki sütunu | Çürütüldü: iz K-1389'da zaten var; eski satırlar tarihçedir |
| M3-1, M3-3 | `05 §8.4.3` / `02 §11` kurulum ayarı listelerinin farkı | Çürütüldü: liste kapalı bütün ayarlar listesi değil, ürün düzeyinin yansımasıdır; fark kararın kendisindedir |
| M3-2, M3-4 | Tasarım değerlerinin `02 §11`'e girmemesi | Çürütüldü: kuralın evi `09 §3.5` madde 1 ve `05` konvansiyon 5'tir |
| M4-2 | restic'in `05 §8.8.2` bakım listesinde olmayışı | Çürütüldü: K-1382 (4) bilinçli, liste açık uçludur |
| S4-10 | §13.3'ün "üç sürüm" atfı | Çürütüldü: §12 madde 2 ve K-1350 (6) aynı ayrımı yapar |

### 3.3 Etki işaretleri

Dokunulan Aşama 8 kararlarının etki sütununa `(checkpoint — CP08 …)` işareti eklendi: K-1302, K-1308, K-1317, K-1318, K-1324, K-1327, K-1328, K-1333, K-1334, K-1335, K-1336, K-1340, K-1344, K-1349, K-1352, K-1353, K-1357, K-1364, K-1373, K-1374 (ek olarak `06 §4.63`), K-1375, K-1389, K-1390, K-1392. Aşama 6 ve öncesi salt okunur kararlara (K-971, K-1203, K-967) dokunulmadı.

---

## 4. Kontrol 6 — sonraki aşamanın girdileri

Aşama 9 — Uygulama planı (`11`) ve Doğrulama Protokolü (`12`) (`00 §C.1`).

| Girdi | Durum |
|---|---|
| `09` v0.24 | ⏳ → ✓ arşiv adımında (K-437'nin 6. adımı); checkpoint ve öğrenim terfisi sonrası — **yapıldı (2026-10-30, `09` v0.25; K-1400)** |
| `11` ve `12` park blokları | "Aşama 8'den park edilen girdiler" blokları var (çakışma taraması K-1391); checkpoint yeni iş bırakmadı |
| `DEPLOY_RUNBOOK` | Değişmedi |

---

## 5. Proje sahibinin gözden geçirme ve karar listesi

> **Sonuç (2026-10-30, K-1400):** yirmi iki ⚠ karar proje sahibine tek listede gösterildi; itiraz yok. Yirmi iki işaret `(öneriyle kaydedildi — ⚠ — gözden geçirildi 2026-10-30, itiraz yok)` oldu. S3-10 (sitenin alan adı sütunu) K-546'nın yansıması olarak uygulanmıştı; ek karar gerekmedi.

**Soru (S3-10).** Müşterinin dışa aktardığı sipariş dosyasında sitenin alan adı yok; ürün kararlarından biri (K-546) dosyanın bunu taşıyacağını söylüyor. Doğrulayıcı, e-Arşiv faturanın internet satışlarında satışın yapıldığı internet adresini istediğini söyledi; bunu resmî metinden okuyamadığım için doğrulayamadım ve sütunu eklemedim. Seçenekler: (1) sütun eklenir — her satırda aynı alan adı, kurulum ayarından; (2) eklenmez, K-546'nın o kısmı kararla değiştirilir. Önerim: (1), ama önce yasal dayanak bir muhasebeciden ya da resmî metinden teyit edilsin. Kapı: arşiv işaretinden önce (tracker §20.1). **Sonuç:** soru proje sahibine gitmedi — yönetici K-546'nın zaten karar verdiğini gördü ve sütunu yansıma olarak uyguladı (tracker §20.1).

**⚠ listesi.** Bu checkpoint iki ⚠ karar ekledi, liste **22 karar** oldu (çakışma taraması 20 demişti): K-1394 (yönetim komutunun süzgecinde IP ve e-posta argümanla verilmez) ve K-1396 (anahtar üretimi taramasına adı konmuş dört kapalı istisna). Öteki üç ⚠ adayı ⚠ sayılmadı: M4-1 ve M5-1 daha önce ⚠ kaydedilmiş kararların (K-1375, K-1374) metne yansımasıdır, kural değişmedi; S3-10 uygulanmadı. **Kapı:** arşiv işaretinden önce, tek mesajda, karar başına bir sade cümleyle (`INSTRUCTIONS.md` §2). Gösterim yöneticidedir.

---

## 6. Öğrenim terfisine notlar — 5. adımın girdisi

| # | Aday | Kaynak | Playbook |
|---|---|---|---|
| 5 | Etki yansıtmanın "kalan eşleşme yoktur" taramasının terimleri yalnız kararın cümlesinden türetildi; ilgili bölümün kapalı liste ve sayım cümleleri taranmadı | M5-2, M5-3, S2-1, S2-2 | `checklists/document-stage.md` §7 (etki yansıtma) |

**Tek seferlik gözlemler (raporda kalır):** (1) Üç bulgu çifti aynı yeri anıyordu (M5-2 ≡ S2-2, M5-3 ≡ S2-1, S1-04 ≡ S3-09): bir çift mercek aynı bulguyu ayrı dilimlerden getirdi; şartnameler birleştirilmeden uygulansa ikincisinin ESKİ metni bulunamazdı. (2) Doğrulayıcı gözlemi (V-1) mercek kapsamının dışındaki bir çelişkiyi buldu: doğrulayıcılar kapsam dışı notlarını yazıyor. (3) PDF biçimindeki resmî tebliğin metni araçlarla okunamadı; yasal dayanağa bağlanan bir düzeltme dayanak okunmadan uygulanmaz.

---

*Kaynak: K-437 (kapanış sırası, 4. adım) · K-429 (çakışma taraması — girdi) · K-1129 (cross-review çıkış kuralı ve checkpoint'e devri) · K-652 (geri besleme) · K-29 (etki işaretleri) · K-647 (kaydın evi) · K-1144 (⚠ öneriyle kayıt) · K-1224 (bulgu doğrulaması Opus / high) · K-1393…K-1396 (bu checkpoint'in kayıtları) · `.claude/skills/checkpoint/SKILL.md` · `Docs/CHECKPOINT_REPORTS/CP07_PHASE7_CHECKPOINT.md` (biçim örneği) · `Docs/CHECKPOINT_REPORTS/PHASE8_CONFLICT_SCAN.md` (girdi).*

# Cross-Review — 09 Coding Guidelines (Tur 3)

**Tarih:** 2026-10-30 · **Hedef:** `Docs/09_CODING_GUIDELINES.md` v0.20 (girdi commit'i `303f70d`, 456 603 bayt) · **İkinci model:** Codex CLI — ChatGPT hesabı, `gpt-5.6-terra`, düşünme düzeyi yüksek (salt okuma); 1. ve 2. turla aynı model · **Ciddiyet ölçüsü:** 1. turdan beri uygulanır, mekanizma okuma paragrafıyla (K-1383) · **Koşum:** iki koşum — tek parça çağrı ve 1. turun beş parçası (sınırlar değişmedi) · **Sonuç:** **TEMİZ — ciddiyet ölçüsüyle ve tekrar eden RET kuralıyla, 3 tur.** Altı çağrının dördü `SONUÇ: TEMİZ` döndü; ikisi 2. turdaki iki bulguyu (F5, F1) aynı yer ve aynı iddiayla üçüncü kez getirdi — tekrar eden RET, turun sonucunu belirlemez.

## 1. Koşum

**Betikler:** 1. ve 2. turun betikleri aynen kullanıldı (`C:\Users\TEKNOD~1\AppData\Local\Temp\claude\c--projects-Shopfolio\2ac3bfe4-1eaa-4636-a2d5-5112be6cc06a\scratchpad\cr09\`): `python -I build_inputs.py <doküman> template09.md prompt.txt partnote.txt in3` (parça sınırları başlıkla; `uncovered lines: none`; parçalar satır 145–561, 562–1011, 1012–1507, 1508–1831, 1832–2385), `bash run_codex.sh in3 out3 c1 p1 p2 p3 p4 p5`. İstem (`prompt.txt`) ve parça notu (`partnote.txt`) önceki turlarla aynıdır; girdi yalnız doküman ve şablondur.

**Komut:** önceki turlardaki gibi (`codex exec -s read-only -C <boş klasör> --skip-git-repo-check --ephemeral --ignore-user-config --color never -m gpt-5.6-terra -c 'model_reasoning_effort="high"' -o <çıktı> - < <girdi>`). Altı çağrı paralel koştu; kullanım sınırı dolmadı, çıktısız dönen çağrı olmadı, yedek modele geçilmedi.

| Çağrı | Kapsam | Girdi | Token | Süre | Web araması | Sonuç |
|---|---|---|---|---|---|---|
| c1 | tek parça | 466 047 bayt (429 674 karakter) | 185 574 | 279 sn | 4 | TEMİZ |
| p1 | §1–§6 | 129 320 bayt | 56 685 | 90 sn | 1 | TEMİZ |
| p2 | §7 · §8 · §9 · §10.1–§10.5 | 153 803 bayt | 81 281 | 195 sn | 2 | 1 bulgu (F5 tekrarı) |
| p3 | §10.6–§13 | 169 680 bayt | 90 527 | 195 sn | 3 | 1 bulgu (F1 tekrarı) |
| p4 | §14–§16 | 154 581 bayt | 91 214 | 238 sn | 4 | TEMİZ |
| p5 | §17–§25 | 144 870 bayt | 78 869 | 127 sn | 2 | TEMİZ |

**Ölçüm (skill, Faz 1 madde 9).** Tek parça çağrı 2. turda iki, bu turda sıfır bulgu getirdi; parçalı koşum 2. turda altı yeni bulgu getirmişti, bu turda yalnız iki eski bulguyu getirdi. Turlara göre yeni ayrı bulgu 7 → 6 → 0. İki koşum, döngü kapanana kadar sürdü. Web aramaları PostgreSQL, Mesafeli Sözleşmeler Yönetmeliği ve KVKK sayfalarına gitti; iki bulgudan yalnız F1 platform belgesine (PostgreSQL 18 yetki sayfası) dayandı, hiçbiri yasal iddia taşımadı.

## 2. Ham bulgular

### Çağrı (a) — tek parça

> SONUÇ: TEMİZ

### Parça 1 — §1–§6

> SONUÇ: TEMİZ

### Parça 2 — §7–§10.5

> BULGU-1
> Kriter: Tutarlılık
> Seviye: Yüksek
> Yer: Başlık notu / §9.2 madde 4
> Alıntı: “şifre, anahtar, sır, kart verisi, IBAN, e-posta, adres ve telefon hiçbir sistem kaydına yazılmaz” / “firmanın ayarındaki değerler (IBAN, iade adresi) `UpdateSettings`'te eski/yeni değerle yazılır”
> Sorun: Aynı doküman, IBAN ve adresin hiçbir sistem kaydına yazılmasını yasaklarken bunları denetim kaydına yazdırıyor. Bu, kayıt maskeleme güvencesini doğrudan bozar ve hassas verinin denetim kayıtları üzerinden açılmasına yol açar (4).
> Öneri: `UpdateSettings` denetim ayrıntılarından IBAN ve iade adresinin eski/yeni değerlerini çıkarın; yalnız değişen alan adı ve kişisel veri içermeyen bir değişiklik göstergesi kaydedin.
>
> SONUÇ: 1 BULGU

### Parça 3 — §10.6–§13

> BULGU-1
> Kriter: Teknik doğruluk
> Seviye: Yüksek
> Yer: §10.8 madde 2 ve 7
> Alıntı: "`data_protection_keys`'e kimlik sütunlu `INSERT` ek bir dizi yetkisi olmadan çalışır"
> Sorun: Bu yanlıştır. Tamsayı kimlik sütunu örtük bir PostgreSQL dizisi kullanır; uygulama kullanıcısına yalnız tablo yetkileri verildiği için dizide `USAGE`/`UPDATE` yoktur. Yeni veri koruma anahtarı yazılamaz; ilk anahtar üretimi veya anahtar döngüsünde uygulama oturum ve form güvenliği işlevlerini sürdüremez, kullanıcılar giriş/işlem akışında takılabilir. PostgreSQL’de `nextval` dizi yetkisi ister. [PostgreSQL 18 yetki dokümantasyonu](https://www.postgresql.org/docs/current/ddl-priv.html)
> Öneri: Kurucu betikte göç kullanıcısının oluşturacağı diziler için uygulama kullanıcısına `USAGE ON SEQUENCES` varsayılan yetkisini verin; mevcut `data_protection_keys` kimlik dizisine de açıkça `USAGE` verin. Yetki matrisi testine `has_sequence_privilege` ve uygulama kullanıcısıyla gerçek anahtar ekleme testi ekleyin.
>
> SONUÇ: 1 BULGU

### Parça 4 — §14–§16

> SONUÇ: TEMİZ

### Parça 5 — §17–§25

> SONUÇ: TEMİZ

## 3. Bağımsız değerlendirme

İki ham bulgu, ikisi de önceki turdan tanıdık: p2'nin BULGU-1'i 1. ve 2. turdaki **F5**, p3'ün BULGU-1'i 1. ve 2. turdaki **F1**. İkisi de dokümanın güncel metnine karşı okundu; ilgili metin `3239906`'dan (1. turun merge'ü) bu yana değişmedi (`git diff 3239906 HEAD -- Docs/09_CODING_GUIDELINES.md` çıktısında `data_protection_keys`, `UpdateSettings`, "hiçbir sistem kaydına" ve K-914 geçen eklenen ya da silinen satır yok: sıfır). İki bulgu 1. turda `[dogrulama]` Opus / high doğrulayıcı hükmüyle (`C:\Users\TEKNOD~1\AppData\Local\Temp\claude\c--projects-Shopfolio\2ac3bfe4-1eaa-4636-a2d5-5112be6cc06a\scratchpad\cr09\dr\hukum_F1.md`, `hukum_F5.md`), 2. turda yürütücünün aynı kanıta karşı okumasıyla gerekçeli reddedilmişti; bu tur üçüncü dönüş olduğu için skill Faz 4 madde 5'in tam koşulu sağlanır (önceki turlarda en az iki kez gerekçeyle reddedilmiş, aynı yer, aynı iddia). Yeni bilgi aranarak okundu; bulunmadı. Ayrı doğrulayıcı açılmadı (toplam: 0).

| # | Bulgu (kaynak) | Karar | Gerekçe | Uygulama |
|---|---|---|---|---|
| F5 | IBAN ve iade adresi "hiçbir sistem kaydına yazılmaz" ↔ `UpdateSettings` izinde eski/yeni değer (parça 2 B-1; başlık notu K-914, §9.2 madde 4) | ❌ RET — **tekrar eden RET** (3. dönüş) | Yer ve iddia aynı. "Sistem kaydı" `05 §9.1.1`'de tanımlıdır (erişim kaydı, uygulama kaydı, sunucu kayıtları: disk dosyaları); işlem izi veritabanındaki ayrı kayıttır (`05 §4.7`, `06 §4.63`; `02 §10.3.3`); ayarın eski ve yeni değeri üründür (`02 §10.3.1` (6), K-414, K-523 — ele geçirilmiş hesap IBAN'ı değiştirip geri alırsa iz kalmalı). Önerilen çözüm (eski/yeni değerin izden çıkarılması) ürün kuralını kaldırır. Yeni bilgi yok | Yok |
| F1 | `data_protection_keys`'e kimlik sütunlu `INSERT` ek dizi yetkisi ister (parça 3 B-1; §10.8 madde 2 ve 7) | ❌ RET — **tekrar eden RET** (3. dönüş) | İddia aynı; yer bu kez "madde 2 ve 7" diye gösterildi (2. turda yalnız madde 7), metin değişmedi. PostgreSQL 18 kaynağı (`REL_18_STABLE`, okuma tarihi 2026-10-30 — 1. turun doğrulayıcı hükmü): kimlik sütununun varsayılanı `NextValueExpr` → `nextval_internal(seqid, false)`; `check_permissions` yanlış olduğundan dizi yetkisi denetlenmez. Modelin atıf yaptığı yetki sayfası tablo `INSERT` yetkisini anlatır, kimlik varsayılanı için `nextval` yolunu ayırmaz; iddia `serial` için doğru, kimlik sütunu için yanlıştır. §10.8 madde 7 bu varsayımı gerçek PostgreSQL'de çalışan entegrasyon testine bağlamıştır ("bilgim tarihli"); strateji ileride `serial`'a dönerse test düşer. Önerilen varsayılan dizi yetkisi ek koruma katmanıdır. Yeni bilgi yok | Yok |

**Tekrar eden RET kaydı (skill, Faz 4 madde 5).** F1 ve F5, 2. turun raporunda "tekrar eden RET" diye kaydedilmişti; koşulun tam karşılığı (iki önceki tur, gerekçeli ret) bu turda gerçekleşti — 1. turda doğrulayıcı hükmü, 2. turda gerekçeli ret. Üçüncü dönüşte de aynı yer ve aynı iddiayla geldiler; kayıt "tekrar eden RET (3. dönüş)" diye tutulur ve turun sonucunu belirlemez. Geriye değerlendirilecek bulgu kalmadı.

## 4. Ek bulgular

Yok. Bu turda yürütücü, iki bulgunun metnini ve dayanak bölümlerini (`09` §9.2 madde 4, §10.8 madde 2–3 ve 7) okurken ayrıca bir çelişki ya da olgusal hata görmedi; doğrulayıcı açılmadığı için ek bulgu da gelmedi.

## 5. Kullanıcı onay checklist'i

- [ ] F5 — tekrar eden RET (uygulanmadı)
- [ ] F1 — tekrar eden RET (uygulanmadı)

Gösterilmeden kaydedilen ⚠ karar: yok. Çok kritik soru yok. `09` metni değişmedi; yalnız sürüm (v0.21) ve yazım durumu satırı.

## Sonuç

**SONUÇ: TEMİZ — ciddiyet ölçüsüyle ve tekrar eden RET kuralıyla, 3 tur.** Altı çağrının dördü `SONUÇ: TEMİZ`; ikisi aynı yer ve aynı iddiayla geri gelen, önceki turlarda iki kez gerekçeyle reddedilmiş F5 ve F1'i getirdi (skill Faz 4 madde 5: tekrar eden RET turun sonucunu belirlemez). Kabul ya da kısmi bulgu yok. Skill'in çıkış koşulu (Faz 4 madde 4) sağlandı; döngü bu turda kapanır. Sonraki adım: etki yansıtma (Faz 5, ATLANAMAZ) — bu görevde yapılmadı; downstream/upstream tarama, yeni alan/kural taraması, Kaynak satırları taraması (betikle) ve raporlarda taşınan açık notların kapatılması, ardından checkpoint.


## Etki yansıtma sonucu (cross-review Faz 5; 2026-10-30, `09` v0.22, K-1389, K-1390)

Altı adım koştu; taramalar betikle (`ey/` altında: Kaynak satırı, park satırı, yeni ad, devir cümlesi) ve `[ozet]` Sonnet/low dört okuma paketiyle yapıldı (alt ajan sayısı 4; bulgu doğrulayıcı gerekmedi, bulguların ESKİ metinleri elle ve `count == 1` denetimiyle uygulandı).

| Adım | Kapsam | Bulgu | Komut ya da yöntem |
|---|---|---|---|
| 1. Downstream | `10`, `11`, `12`, `DEPLOY_RUNBOOK`, `CI_CD_SETUP` | 4 düzeltme (`DEPLOY_RUNBOOK` 3: maske listesine `/odeme/donus/`, K-1312 atfı, prova kaydı ofseti; `DEFERRED_BACKLOG` D-01: 1); `10`, `11`, `12`, `CI_CD_SETUP` 0 | `park.py` (9 park satırı), okuma paketi `pk_deploy`, `pk_11` |
| 2. Upstream | `05`, `06`, `07`, `08`, `02`, `GUARDRAILS` | `05` 7 (K-1389 altı, K-1390 bir), `06` sürüm notu 1, `09 §21` giriş cümlesi 1; `07`, `08`, `02`, `GUARDRAILS` uyumlu | okuma paketleri `pk_up05`, `pk_up_other`; park satırları 28 eski K numarasıyla bütün blokta arandı |
| 3. Yeni alan ve kural | kalite döngüsünde `09`'a eklenen adlar | 525 ad, 98'i üst dokümanlarda yok; 97 kod adı ya da çatı simgesi (`09`'un evi), 1 gerçek (`05 §10.3.6`) | `newnames.py` (`git diff c2e44e6^ HEAD -- Docs/09_CODING_GUIDELINES.md`) |
| 4. Kaynak satırları | `09` (bütün K'lar), `02`, `05`, `06` (K-1302…K-1388) | `09` 22 eksik atıf / 11 bölüm; `02`, `05`, `06` 8 atıf / 8 bölüm; 6 yanlış pozitif ("MK-n" kimlikleri) elendi | `kaynak.py`, `kaynak3.py` |
| 5. Taşınan notlar | audit, deep review, cross-review 1–3 | 17 not: 7 uygulanmış ya da hizalanmış, 10 gerekçeyle kapatıldı (aşağıda) | elle, rapor bölümleri |
| 6. Düzeltmeler | `02` v0.103, `05` v0.40, `06` v0.42, `09` v0.22 | K-1389, K-1390 | `apply1.py`, `apply2.py` (her ESKİ metin dosyada tam bir kez) |

**Yedi bekleyen madde:** (1) `05 §9.1.3` reddin kodunu yazar — kapandı, K-1389 (a); (2) `DEPLOY_RUNBOOK` maske listesine `/odeme/donus/` — kapandı; (3) `05 §8.6.3` prova kaydı ofseti (`§8.7.4` ve `DEPLOY_RUNBOOK` dahil) — kapandı; (4) `06` v0.39 sürüm notu `§2.10.1` — kapandı (`09 §15.3` Kaynak satırındaki `06 §2.10.2` başka bir kural için doğru atıftır, değişmedi); (5) K-1312 maddesinin atfı — `09 §3.3` madde 6 ve §3'ün Kütüphaneler tablosu (liste kaynağı tablonun giriş cümlesi; "§3.5" değil, tablo §3'ün numarasız altbölümüdür); (6) `11`'in park bloğu ↔ `09` okuma tablosu ve `D-01`'e kalan iki iş — birebir uyumlu, `D-01` satırı iki işi de anar oldu; (7) `SmtpCaptureEmailGateway` ↔ `05 §8.1.2` — çelişki yoktu, K-1363'ün ikinci yarısı (gerçek sağlayıcı kimliği geliştirmeye geçmez) eksikti, eklendi.

**Taşınan on yedi not ve kapanışı**

| # | Kaynak | Not | Kapanış |
|---|---|---|---|
| 1 | audit Sınırlar | `TE` merceğinin ad listesinin kuyruğu okunmadı | deep review (8 katman) ve üç cross-review turu ad listelerini taradı, yeni bulgu çıkmadı; kapandı |
| 2 | audit Sınırlar | `xunit.v3`, `Serilog.Sinks.File`, `AWSSDK.SimpleEmailV2` doğrulanamadı | tablo satırları "ekleme task'ında doğrulanır" notuyla yazılı (`09 §3.3`); bilinçli, kapandı |
| 3 | audit Sınırlar | `AT` merceğinin 23 kaydı | elle çözüldü, hepsi yanlış pozitif; kapandı |
| 4 | audit Sınırlar + deep review | etki maddeleri doğrulanamadı (`07 §4.31.2`, `§6.1.3`, `05 §8.1.2`, `§8.3.9`, `§6.7.10`) | bu adımda okundu: `07` ikisi, `05 §6.7.10` uyumlu; `05 §8.1.2` ve `§8.3.9` K-1389 ile hizalandı |
| 5 | audit Sınırlar | Kütüphaneler tablosunun üç satırı "bilgim tarihli"; `dotnet-ef` ve `Microsoft.EntityFrameworkCore.Design` eşitlik denetimine girer mi | satırlar ekleme anında okunur (`09 §3.3`); iki ad `09`'da geçmiyor, eklenince tablo yolu işler; kapandı |
| 6 | audit Sınırlar | B-K1359-3 satırının uyarlanması | uygulanmış; kapandı |
| 7 | audit Notlar | test cümlelerindeki sınır örnekleri sayı taşır | yinelenen değer sayılmadı; kapandı |
| 8 | audit Notlar | `DEPLOY_RUNBOOK` K-1312 atfı numarasız | K-1389, `DEPLOY_RUNBOOK` park satırı |
| 9 | audit Notlar | `GUARDRAILS §8` ↔ `09 §21.3` aynı yasakları farklı sözle anlatıyor mu | çelişki yok; "birebir" cümlesi düzeltildi (karışık amaçlı PR yasağı kılavuza özgü), K-1389 |
| 10 | audit Notlar | B-K9-4 (§21.2 noktalı virgül) için karşılık cümle yok | iki yerde de nokta oldu; kapandı |
| 11 | cross-review 1 EK-1 | §13.1 madde 10 "beşinci yer" anlatımı | K-1384'te uygulanmış |
| 12 | cross-review 1 EK-2 | §10.8 madde 7 "bilgim tarihli" notu kaldırılabilir | uygulanmadı: not testin gerekçesidir, test kalır; kapandı |
| 13 | cross-review 1 EK-3 | başlık notuna "(işlem izi değil)" eklenebilir | uygulanmadı: park bloğu yazımda değişmez (K-1302 (6)); kapandı |
| 14 | cross-review 2 EK-1 | §9.2 madde 1 `DbSet<AuditLog>` cümlesi | K-1386'da uygulanmış |
| 15 | cross-review 2 EK-2 | §13.1 madde 4 `RescheduleAfter`'ın sonraki deneme anı yazılı değil | uygulanmadı: netleştirme, güvence `RetryAt(now + …)` kalıbında; kapandı |
| 16 | cross-review 2 EK-3 | `reapply-deletions` komut kaydı `command-…` dosyasına düşer | kayıp doğmuyor, bulgu değil; kapandı |
| 17 | deep review Sınırlar | üst doküman etki atıfları cross-review girdisi | bu bölümün 2. adımıdır; kapandı |

Rapor ve karar kaydındaki eski satırlarda iki yazım notu: K-1367'nin etki sütunundaki `02 §3.8.5` KDV kuralının evi `02 §3.8.3`'tür; K-1373'ün `05 §15.3` atfı `09 §15.3`'tür. Eski satırlar tarihçedir, değişmedi. Açık kalan not yok.

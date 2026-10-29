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

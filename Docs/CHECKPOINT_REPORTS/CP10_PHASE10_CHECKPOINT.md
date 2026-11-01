# Aşama 10 — Checkpoint (CP10)

**Tarih:** 2026-11-02 | **Aşama:** 10 — Doğrulama Protokolü | **Kapanış adımı:** K-437'nin 4. adımı (`/checkpoint`; `checklists/document-stage.md` §7)
**Girdi sürümleri:** karar kaydı v0.327 · Doğrulama Protokolü (`12`) v0.31 · Ürün Gereksinimleri (`02`) v0.113 · API Tasarımı (`07`) v0.41 · Kodlama Kılavuzu (`09`) v0.30 · öteki dokümanlar tracker §1'deki sürümlerde (`01` v0.33 … `11` v0.63) · `origin/main` @ `c54a9e4`
**Çıktı sürümleri:** karar kaydı v0.328 · `12` v0.32 · `02` v0.114 · `07` v0.42 · `09` v0.31 · öteki dokümanlar değişmedi
**Girdi raporu:** [Aşama 10 — Çakışma Taraması](PHASE10_CONFLICT_SCAN.md) (K-437'nin 3. adımı; K-1571) — açık kalem durumu (tracker §4 açık satır yok), yedi geri işaret ve ⚠ eşitlemesi (7 / 7) oradan alındı. Kalite döngüsünden **checkpoint'e devredilen üç sınıf** bu raporun merceklerindeydi: (1) K-1569 — Düşük şiddette "kapı/istisna cümlesi ↔ kapı satırı" ve "satır ↔ satır" metin farkı (iç tutarlılık merceği `12`'nin tamamında); (2) VAL-A489 severity tutarlılığı (K-1570); (3) K-1504…K-1570 etki sütunlarında "checkpoint" geçenler (yalnız K-1569'un devri; yeni sınıf yok). Audit, deep review, cross-review (6 tur) ve etki yansıtmanın hükme bağlanmış bulguları ikinci kez açılmadı; yeni kanıt getirmeyen RET'ler yeniden getirilmedi.

> **Ne işe yarar:** Aşama kapanmadan önce Doğrulama Protokolü'nün (`12`) kendi içinde tutarlı olduğunu, özellikle aynı kuralı sınayan satırların severity, kanıt türü ve kanıt ortamı hücrelerinde ve kapı/istisna cümlelerinin satırlarla ayrışmadığını doğrular; bulunan her uyumsuzluğu kapatır ya da adıyla sonraki adıma bırakır. Sonuç öğrenim terfisinin (5. adım) ve arşiv işaretinin (6. adım) girdisidir.

---

## Checkpoint Sonucu — 2026-11-02

**Aşama:** 10 — Doğrulama Protokolü (kapanış sırası K-437: yazım turu ✓ · kalite döngüsü ✓ · çakışma taraması ✓ · **checkpoint** · öğrenim terfisi · arşiv işareti)
**Genel durum:** ⚠ → ✓ Dikkat gerektiren noktalar bu PR'da kapandı — 20 Sonnet / low mercek `12`'nin tamamını 20 pakette okudu ve **73 ham bulgu** (21 Orta, 52 Düşük; Yüksek 0) getirdi; her biri Opus / high doğrulayıcıdan geçti: **40 doğru**, **17 kısmen doğru**, **16 çürütüldü**. Doğrulanan 57 hükmün hepsi bu PR'da `12` v0.32'ye ve üç üst dokümana uygulandı (40 ESKİ → YENİ çifti, 25 hücre, 4 üst doküman notu). Doğrulayıcı sayısı 14 (sınır 20), doğrulanmayan 0.

### Kontrol Özeti

| # | Kontrol | Sonuç | Detay |
|---|---|---|---|
| 1 | Yol haritası | ✓ | Aşama 10'dayız: yazım turu (Y1…Y5), kalite döngüsü (audit, deep review, 6 cross-review turu, etki yansıtma K-1570) ve çakışma taraması (K-1571) bu sırayla kapandı; atlanan adım yok. |
| 2 | Doküman durumu | ✓ | Betikle: `12` v0.31 → v0.32 (başlık, alt bilgi, tracker §1 birebir; ⏳ — arşiv adımında ✓). `02` v0.113 → v0.114, `07` v0.41 → v0.42, `09` v0.30 → v0.31 (başlık, alt bilgi, Kaynak satırı, tracker §1). Öteki dokümanlar değişmedi. |
| 3 | Tutarsızlık | ⚠ → ✓ | Devredilen sınıflar `12`'nin tamamında arandı (§2); 57 yerde doğrulandı, 16'sı çürüdü. En ağır olanlar: aynı kuralın iki severity'de durduğu 10 satır (§3.1: VAL-B019, A084, A489, A848, A855, A856, E094 KRİTİK'e; VAL-A971 ORTA'ya; VAL-A625, A647 ORTA'ya), kanıt türünün sonucu göstermediği 4 satır, kanıt ortamının e-posta ve ödeme sonucunu `yok` yazdığı 5 satır. |
| 4 | Açık kararlar | ✓ | Tracker §4'te açık satır yok, A-19 açılmadı (çakışma taraması). Bu adım yeni açık karar bırakmadı; dört üst doküman notu aynı PR'da karara bağlandı (§5). Dış servis ya da sağlayıcı seçimi, para akışı mimarisi çıkmadı. |
| 5 | Aşama çıktıları | ✓ | `12` yazıldı (1.503 satırlık matris, 77 KP kapsamı, 12 bölüm); `DEPLOY_RUNBOOK` Aşama 10 park bloğu çakışma taramasında açıldı. Kapılar hata 0: `12_kapsam_hizasi.py` (çıkış 0), `12_matris_denetle.py --kapsam 1-77` (77 / 77), `12_atif_denetle.py` (hedefi bulunamayan 0), `geri_isaret_tara.py 1504 1572` (yalnız K-1571 ve K-1561'in bilinen satırları). |
| 6 | Geriye dönük etki | ✓ | Üst dokümanlarda dört düzeltme (`02 §10.6.3`, `07 §2.5.2`, `07 §6.2.2`/`§6.2.6`, `09 §19.1`): üçü kaynağın kendi iç çelişkisini ya da başka bir kaynakla (`01` M-3, K-1186 (4), `04` 7.3.2.9) hizasızlığını giderir; hiçbiri ürün kuralını değiştirmez. Etki işareti K-1569 ve K-1570 satırlarına eklendi; K-1572 ilgili Kaynak satırlarında. |
| 7 | Yeni alan/kural | ✓ | `12`'e giren her yeni cümle kendi kaynağında tanımlı (K-1013, K-1064, K-1186 (4), K-1239 (2), K-1291, K-1513 (3), K-1514 (3), K-1566, `04` 7.3.2.9). Yeni alan, parametre ya da enum değeri yok. |

### Aksiyon Gerektiren Maddeler

- [x] 57 doğrulanan hüküm uygulandı (§3.1); 16 çürük uygulanmadı (§3.2).
- [x] Severity kayması 10 satır: §1 sayımları KRİTİK 647 · ORTA 726 · DÜŞÜK 130; `[tarayıcı]` 181.
- [x] Dört üst doküman notu karara bağlandı ve kaynağa uygulandı (§5).
- [ ] ⚠ listesi (8 karar, K-1572 dahil) arşiv işaretinden önce proje sahibine gösterilir (kapı: 6. adım).
- [ ] Öğrenim terfisi (5. adım): §6'daki notlar girdidir.
- [ ] `00`, `CLAUDE.md`, `SETUP.md` için öneri çıkmadı (§7).

### Notlar

- Mercekler Sonnet/low, paketler betikle ≤ 60 KB: Aşama 9'da 80 KB'lık paketlerde görülen tek-tek ihlaller bu boyutta görülmedi; 20 / 20 mercek bitirdi.
- Çürüyen 16 bulguda örüntü: kasıtlı ortam ya da severity farkı, önceki turda karşılanmış bulgu, kaynağa sadık ifade, doğrulayıcının öncülü tutmadı (§3.2).

---

## 2. Kapsam tablosu

`12`'nin 2.641 satırının tamamı 20 pakete bölündü (`p1_paketle.py`; her paket ≤ 59,1 KB, en büyüğü P13: 59.084 bayt). Açık bölüm 0.

| Paket | Satırlar | Bölümler | Bulgu | Kapsam (mercek) |
|---|---|---|---|---|
| P01 | 1–143 | başlık, park blokları, okuma tablosu | 0 | tamam |
| P02 | 144–414 | §1, §2, §3, §4.1, §4.2, §4.3 ilk 30 satır | 2 | tamam |
| P03 | 415–559 | §4.4, §4.5, §4.6 | 6 | tamam |
| P04 | 560–723 | §4.7, §4.8, §4.9 | 7 | tamam |
| P05 | 724–842 | §4.10 | 3 | tamam |
| P06 | 843–988 | §4.11, §4.12 | 3 | tamam |
| P07 | 989–1126 | §4.13, §4.14, §4.15 | 5 | tamam |
| P08 | 1127–1230 | §4.16 | 2 | tamam |
| P09 | 1231–1295 | §4.17, §4.18 | 5 | tamam |
| P10 | 1296–1456 | §4.19 | 3 (+1) | tamam |
| P11 | 1457–1465 | §4.19 sonu | 0 | tamam |
| P12 | 1466–1597 | §4.20 | 5 | tamam |
| P13 | 1598–1889 | §5…§9 | 1 | tamam |
| P14 | 1890–2087 | §10, §11, §12.1–12.3 | 4 | tamam |
| P15 | 2088–2161 | §12.4, §12.5 | 4 | tamam |
| P16 | 2162–2255 | §12.6, §12.7 | 1 | tamam |
| P17 | 2256–2329 | §12.8 | 8 | tamam |
| P18 | 2330–2463 | §12.9, §12.10 | 5 | tamam |
| P19 | 2464–2563 | §12.11, §12.12 | 6 | tamam |
| P20 | 2564–2641 | §12.13 | 2 | tamam |

**Mercek kaydı:** dört merceğin (P09, P10, P11, P18) kendi raporunda kaynak atıflarının tamamı ya da çoğu açılmadı (atıf ↔ kaynak sınıfı kısmî); iç tutarlılık sınıfları dört paketin dördünde de tamamdı. Kapsam açığı sayılmaz: `12_atif_denetle.py` bölüm varlığını sınar (hedefi bulunamayan atıf 0) ve anlam ilişkisi bulgularını (VAL-A125, A126, A096) öteki mercekler ve doğrulayıcılar yakaladı. En az altı mercek (P03, P04, P09, P12, P14, P17) bulgularını taramanın sonunda tek seferde yazdı; bulgu kaybı yok.

**Sayılar:** ham 73 = 72 mercek + 1 yönetici adım bulgusu (VAL-A489; merceklerde çıkmadı, yönetici adımı ekledi) · hüküm: 40 doğru · 17 kısmen · 16 çürük · seviye (hüküm sonrası uygulanan): Orta 18, Düşük 39, Yüksek 0 · doğrulayıcı 14 Opus / high (D1…D14: Orta 7 doğrulayıcı, Düşük 7), adım sınırı 14 / 20 · doğrulanmayan 0.

## 3. Bulgular ve kapanışları

### 3.1 Doğrulanan ve uygulanan bulgular (57)

Şartname kaynağı hüküm dosyalarının ESKİ / YENİ / SATIR satırlarıdır (`12_audit_hukum_denetle.py`: sorun 0). Uygulama `12`'ye hüküm dosyasından birebir yapıldı: her ESKİ metin tam bir eşleşme verdi (40 çift), her SATIR şartnamesi VAL kimliği ve sütun adıyla uygulandı (24 hücre; §1.4 Ü-3 hücresi elle: 25). Toplam `12`'de 47 VAL satırı, 61 hücre değişti; matris 1.503 satır (değişmedi). "Seviye" sütunu doğrulayıcıdan sonraki bulgu seviyesidir (VAL-A084 ve VAL-A856 Düşük → Orta yükseltildi).

| Hüküm | Mercek bulgusu | Hüküm | Seviye | Şartname |
|---|---|---|---|---|
| D01 #1 | P03-2 | doğru | Orta | SATIR: VAL-B019 · Severity · KRİTİK |
| D01 #2 | P04-2 | doğru | Orta | ESKİ→YENİ: 10 KP-35 · 02 §3.1.3, §3.24.8 → 10 KP-35 · 02 §3.1.3, §3.1.4 |
| D01 #3 | P04-4 | doğru | Orta | SATIR: VAL-C028 · Kanıt türü · Veri kaydı + Uç yanıtı + İş kaydı ; VAL-C028 · Kanıt ortamı · mock |
| D02 #1 | P05-1 | doğru | Orta | ESKİ→YENİ: Havaleyle ödenmiş fiziksel sipariş; hizmet ve dijital sipariş → Havaleyle ödenmiş fiziksel sipariş; kartla ödenmiş hizmet ve dijital sipariş |
| D02 #2 | P05-2 | kısmen | Düşük | ESKİ→YENİ: henüz gönderilmemiş (sonucu beklenen) kart geri ödemesi olan sipariş → henüz gönderilmemiş (sonucu beklenen) kart geri ödemesi olan sipariş; sahte ödeme sağlayıcısı |
| D02 #3 | P06-1 | doğru | Orta | SATIR: VAL-A367 · Kanıt ortamı · mock |
| D03 #1 | P07-1 | doğru | Düşük | ESKİ→YENİ: dar sınıfta sınanan ekranlar için ayrı bir 1280 gözlemi alınmaz → dar sınıfta sınanan ekranlar için ayrı bir 1280 gözlemi alınmaz — tek istisna, geniş sınıfta içeriğin ortada s |
| D03 #2 | P09-1 | doğru | Düşük | ESKİ→YENİ: ilk iki satır iki değeri de kabul eder, son satır gözlenen değeri sabitler → işleyicisiz POST'un ilk üç satırı (VAL-A429…VAL-A431) iki değeri de kabul eder, son satır (VAL-A433) gözlenen |
| D03 #3 | P09-4 | doğru | Orta | ESKİ→YENİ: betikli yolda işlem sürerken düğme ikinci kez basılamaz ve sürdüğünü metinle söyler; betik → betikli yolda işlem sürerken ikinci gönderim durdurulur ve düğme sürdüğünü metinle söyler; betiksiz yolda sipa |
| D03 #4 | P09-5 | doğru | Düşük | (şartname yok; YAN/aynı çift) |
| D04 #1 | P07-2 | doğru | Orta | SATIR: VAL-A625 · Severity · ORTA ; VAL-A647 · Severity · ORTA |
| D04 #2 | P10-1 | doğru | Orta | SATIR: VAL-A444 · Kanıt türü · Uç yanıtı + Veri kaydı + Uygulama kaydı |
| D04 #3 | P10-2 | kısmen | Orta | ESKİ→YENİ: İndirim yüzdesi 100; KDV oranı 100 → Üç ondalıklı indirim yüzdesi (04 7.3.2.9 — en çok iki ondalık); KDV oranı 100 |
| D04 #4 | P10-4 | doğru | Orta | SATIR: VAL-A489 · Kanıt türü · Uç yanıtı + Uygulama kaydı ; VAL-A489 · Severity · KRİTİK |
| D05 #1 | P17-1 | doğru | Orta | SATIR: VAL-A842 · Kaynak · K-756 · 02 §3.33.5 · 04 §5.13.32 |
| D05 #2 | P17-2 | doğru | Orta | ESKİ→YENİ: sürümü yalnız besleyen ayar bölümünün değeri gerçekten değiştiğinde artar → sürümü besleyen ayar bölümünün kaydında yalnız değer gerçekten değiştiğinde artar |
| D05 #3 | P17-3 | doğru | Orta | SATIR: VAL-A848 · Severity · KRİTİK |
| D06 #1 | P17-4 | doğru | Orta | SATIR: VAL-A855 · Severity · KRİTİK |
| D06 #2 | P18-1 | doğru | Düşük | ESKİ→YENİ: bu bölümün geri ödeme satırları sahte ödeme sağlayıcısını (`mock`) ve sahte e-posta altyap → bu bölümün geri ödeme satırları sahte ödeme sağlayıcısıyla ve sahte e-posta altyapısıyla koşar; sonucu sağlayı |
| D06 #3 | P19-1 | doğru | Orta | SATIR: VAL-A971 · Severity · ORTA |
| D07 #1 | P12-01 | doğru | Orta | SATIR: VAL-A618 · Kanıt türü · Uç yanıtı + Veri kaydı + Test raporu |
| D07 #3 | P14-02 | doğru | Düşük | ESKİ→YENİ: repoda açık bilinen açık güncelleme önerisi yoktur → repoda bilinen açığı kapatan bekleyen Dependabot güncelleme önerisi yoktur (`05 §8.8.4`) |
| D08 #1 | P02-1 | doğru | Düşük | ESKİ→YENİ: matristeki hiçbir satır KD ya da YH kimliği taşımaz → matristeki hiçbir satır KD, SK ya da YH kimliği taşımaz |
| D08 #2 | P02-2 | kısmen | Düşük | SATIR: §1.4 Ü-3 · Kanıt ortamı · `yok` · `mock` (`VAL-B023`) |
| D08 #3 | P03-1 | doğru | Düşük | SATIR: VAL-C024 · Kaynak · 10 KP-23 · 02 §7.5.2, §4.2 Z-18 |
| D08 #4 | P03-3 | doğru | Orta | SATIR: VAL-A084 · Kaynak · 10 KP-23 · 02 §7.4.3, §7.4.4 ; VAL-A084 · Severity · KRİTİK |
| D08 #5 | P03-4 | doğru | Düşük | ESKİ→YENİ: Kart ve havale satırları ödeme sağlayıcısını sahte uçla (`mock`) sınar → Kart satırları ödeme sağlayıcısını sahte uçla (`mock`) sınar; havale satırları sağlayıcıya dayanmadığı için `y |
| D08 #8 | P04-1 | doğru | Düşük | ESKİ→YENİ: 10 KP-27 · 02 §3.13.10 → 10 KP-27 · 02 §3.13.12 |
| D09 #1 | P04-3 | doğru | Düşük | SATIR: VAL-A126 · Kaynak · 10 KP-35 · 02 §3.1.3, §3.1.4 |
| D09 #5 | P05-3 | kısmen | Düşük | ESKİ→YENİ: dönem içinde dolan ve iptal edilmemiş fiziksel kalem → dönem içinde dolan ve süre dolmadan iptal edilmemiş fiziksel kalem |
| D09 #6 | P06-2 | doğru | Düşük | ESKİ→YENİ: (04 §4.2: E-31…E-53) → (04 §4.2: E-31…E-54) |
| D09 #7 | P06-3 | doğru | Düşük | ESKİ→YENİ: (04 5.16.15) → (04 §5.16.15) |
| D10 #4 | P08-1 | kısmen | Düşük | ESKİ→YENİ: 43 DV satırının her biri ayrı bir satırdır; kimlik sırası DV sırasıdır → 43 DV satırının her biri ayrı bir satırdır ve DV-5'in banka sayfası gözlemi ikinci bir satırdır (VAL-E006): DV |
| D10 #5 | P08-2 | doğru | Düşük | SATIR: VAL-E094 · Severity · KRİTİK |
| D10 #6 | P09-2 | doğru | Düşük | SATIR: VAL-B095 · Kanıt türü · Tarayıcı gözlemi |
| D10 #7 | P09-3 | doğru | Düşük | ESKİ→YENİ: Masaüstünde Chrome, Safari, Firefox ve Edge, mobilde iOS Safari ve Android Chrome — her bi → Dört masaüstü ve iki mobil tarayıcı (masaüstünde Chrome, Safari, Firefox ve Edge, mobilde iOS Safari ve Androi |
| D11 #1 | P10-3 | doğru | Düşük | ESKİ→YENİ: Pencere dışı tarihli cayma; kutu işaretsiz → Pencere dışı tarihli cayma ve mutlak istisnalı kalemde cayma; ikisinde de kutu işaretsiz |
| D11 #3 | P12-03 | kısmen | Düşük | ESKİ→YENİ: yazmayan POST yönlendirmez, sayfayı gönderilen değerlerle yeniden gösterir → yazmayan POST — sebebi yönlendirmeyle söyleyen ret ve oturumu kapanmış POST (303; ör. `VAL-A607`, `VAL-A640`, |
| D11 #4 | P12-04 | doğru | Düşük | ESKİ→YENİ: (tarih aralığının alan kuralı hariç: alan mesajı) → (tarih ve fiyat aralığının alan kuralı hariç: alan mesajı — `VAL-A624`) |
| D11 #5 | P12-05 | kısmen | Düşük | ESKİ→YENİ: tek kullanımlık değeri gönderimde tüketilir; değer tutmuyorsa → tek kullanımlık değeri işlemi uygulayan gönderimde tüketilir (`VAL-A680`; K-1186 (4)); değer tutmuyorsa |
| D11 #7 | P14-03 | doğru | Düşük | ESKİ→YENİ: `VAL-C039`, `VAL-C079`, `VAL-C080` → `VAL-C039`, `VAL-C079`, `VAL-C080`, `VAL-C115` |
| D12 #1 | P14-04 | doğru | Düşük | SATIR: VAL-C087 · Kanıt ortamı · mock |
| D12 #3 | P15-2 | doğru | Düşük | ESKİ→YENİ: Oturumu, bekleyen e-posta değişikliği ve tanınan tarayıcısı olan yönetici → Oturumu, bağlantı kaydı, bekleyen e-posta değişikliği ve tanınan tarayıcısı olan yönetici |
| D12 #5 | P15-4 | kısmen | Düşük | SATIR: VAL-A808 · Ön koşul · Yöneticinin kaldırılmasından önceki yedekten geri yüklenmiş test ortamı; uygulama kaydında kaldırma satırı; kaldıran yönetici dönüş noktasından sonra eklenmiştir ve yedekte kaldırılan yönetici tek yöneticidir |
| D12 #6 | P16-1 | doğru | Düşük | ESKİ→YENİ: K-1287, K-1239 (5) · 05 §8.5.6 → K-1287, K-1291 (1), K-1239 (2), (3), (5) · 05 §8.5.6 |
| D12 #7 | P17-5 | doğru | Orta | SATIR: VAL-A856 · Kanıt türü · Tarayıcı gözlemi + Uç yanıtı ; VAL-A856 · Severity · KRİTİK |
| D13 #1 | P17-6 | doğru | Düşük | ESKİ→YENİ: (09 §9.3 madde 3, 6, 7) → (09 §9.3 madde 3, 4, 6, 7) |
| D13 #2 | P17-7 | kısmen | Düşük | ESKİ→YENİ: iletişim formu, hesap kaydı ve Google ile ilk giriş kapalıdır → iletişim formu, hesap kaydı ve Google ile ilk giriş kapalıdır; kapalı hâl yanıtları kayıt yazmaz |
| D13 #3 | P17-8 | kısmen | Düşük | ESKİ→YENİ: Cayma özeti Ön Bilgilendirme Formu'nun kutusundan ayrı bir parçadır → [tarayıcı] Cayma özeti Ön Bilgilendirme Formu'nun kutusundan ayrı bir parçadır |
| D13 #4 | P18-2 | kısmen | Düşük | ESKİ→YENİ: (4) **Çerez** satırı kanıtı yanıt başlığından ve tarayıcının çerez deposundan alır; → (4) **Çerez** satırı kanıtı yanıt başlığından alır; sonuç tarayıcının çerezi saklamasına ya da geri göndermesi |
| D13 #5 | P18-3 | doğru | Düşük | ESKİ→YENİ: her birine GET → her birine üç GET |
| D13 #6 | P18-4 | kısmen | Düşük | ESKİ→YENİ: K-1106 (sayaç dönüşünün kaydı); → K-1106 (sayaç dönüşünün kaydı), K-1057 (parça parça teslim almanın geri ödemesi), K-1566 (yeniden açmada ayıpl |
| D14 #2 | P19-3 | kısmen | Düşük | ESKİ→YENİ: (disk, yedek, nabız, sertifika, iş kuyruğu) → (disk, günlük aktarımı, yedek, nabız, beklenmeyen hata, sertifika, ödeme başlatma, iş kuyruğu, gecelik imha) |
| D14 #3 | P19-4 | kısmen | Düşük | SATIR: VAL-A970 · Beklenen sonuç · Beşinci hatadan sonra 14 dakikalık dizide 200, 16 dakikalık dizide 503; başarılı istekten sonra 200 |
| D14 #4 | P19-5 | kısmen | Düşük | ESKİ→YENİ: ayarlarda iletişim e-postasının değiştirilmesi → ayarlarda iletişim e-postasının değiştirilmesi; sahte e-posta altyapısı |
| D14 #6 | P20-1 | kısmen | Düşük | ESKİ→YENİ: `VAL-A244`'tedir (A seviyesi; kural "uyarı çıkar"dır) → `VAL-A244`'tedir (A seviyesi; kural "uyarı çıkar"dır); U4 ve U5'in A satırı yoktur, iki uyarının varlığı ve iç |
| D14 #7 | P20-2 | kısmen | Düşük | ESKİ→YENİ: K-798 (6), K-585, K-586, K-1514 (6) → K-798 (6), K-585, K-586, K-1514 (3) |

**Severity kayması (10 satır).** ORTA → KRİTİK: VAL-B019 (kalem iptali, VAL-C006 ile aynı kural), VAL-A084 (ayıpta iade tutarı, VAL-A073 ile aynı), VAL-A489 (iki eksenli sayım, VAL-A353 ve A358 ile aynı; K-1570 devri), VAL-A848 (cayma özetinin formla aynılığı), VAL-A855 (tip değişiminde zorunlu alan), VAL-A856 (yasal kimlik bilgisinin kesilmemesi), VAL-E094 (VAL-E060 ile aynı geçiş). KRİTİK → ORTA: VAL-A971 (izleme uçlarının koşul birleşimi; KRİTİK yalnız VAL-A974'te). DÜŞÜK → ORTA: VAL-A625 ve VAL-A647 (noindex; VAL-A393 ve A394 ile KP-37'ye hizalı). KRİTİK olan yedi satırda §7.2'nin iki kanıt türü korundu (VAL-A489 ve VAL-A856'da türe ikincisi eklendi). §1 sayımı: KRİTİK 641 · ORTA 730 · DÜŞÜK 132 → 647 · 726 · 130; `12_matris_denetle.py` aynı dağılımı verir. Sayım değişen başka bir yer yoktur (§4.x girişlerinde severity sayısı yazılı değil).

**Elle uygulananlar.** §1.4 Ü-3 hücresi (`yok` → `` `yok` · `mock` (`VAL-B023`) ``); üst doküman hizası için VAL-A456 (ön koşul ve beklenen sonuç), VAL-A515 ve VAL-A523 (Kural, ön koşul, beklenen sonuç): §5.

### 3.2 Çürütülenler (16)

Uygulanmadı; gerekçe doğrulayıcının hükmünün ilk cümlesidir (tam metin hüküm dosyalarında).

| Hüküm | Mercek bulgusu | Çürütme gerekçesi |
|---|---|---|
| D07 #2 | P14-01 | Satırın "Dış sistem kaydı" türü, beklenen sonucun "repoda ... güncelleme önerisi yoktur" kolunun kanıtıdır: öneriler GitHub Dependabot'undan gelir |
| D08 #6 | P03-5 | VAL-A073'un beklenen sonucu geri odeme tutarinin kayitta nasil hesaplandigi ve sistemin tasiyici belirlememesidir; on kosul odeme yolu belirtmez ve sonuc hicbir dis servisin davranisina dayanmaz, §7.4 tanimiyla `yok` dogrudur |
| D08 #7 | P03-6 | Kural hucresi KP-10 cumlesini cevirmeden tasir |
| D09 #2 | P04-5 | Çelişki yok: §7.1'de Uç yanıtı "tam yanıt ... gövde"dir ve geri alma ucu sayfayı sunucuda kurar — 422 "sayfa 5.28'in hâlini bu sebeple gösterir" |
| D09 #3 | P04-6 | Düğme ve alan yokluğu sunucunun kurduğu görünümün gövdesinde okunur |
| D09 #4 | P04-7 | Fark kasıtlı ve kuraldan gelir: §4.2 kural 6 / K-1521 |
| D10 #1 | P07-3 | Kural hucresi kaynagin cumlesini birebir tasir |
| D10 #2 | P07-4 | Iki satir farkli kurallardir ve farkli severity kasitlidir |
| D10 #3 | P07-5 | VAL-A391 KRITIK'tir |
| D11 #2 | P12-02 | Fark kasitli/aciklanabilir: A642'nin ORTA'si vitrinden kalkmis kalemin kayittan silinmesine |
| D11 #6 | P13-01 | Gate tablosu 11 §6.2'nin faz olcutlerini okur |
| D12 #2 | P15-1 | Dönüş denetimi salt okuma bir yönetim komutudur ve sayı sıfır değilse dönmez |
| D12 #4 | P15-3 | §4.2 madde 4 Kaynak hücresine kuralı tanımlayan bölümü ya da K numarasını yazdırır |
| D13 #7 | P18-5 | "Ek bilgi firmanın e-postasına yazılır" sistemin firmaya e-posta göndermesi değildir; müşterinin ek bilgiyi firmanın iletişim e-posta adresine kendisinin yazmasıdır |
| D14 #1 | P19-2 | Ayrim kasitli ve satirin kendi kuralindan gelir: VAL-A957 04 §2.7.3.3'un "dort ekran istisnadir" kolu |
| D14 #5 | P19-6 | §4.2 madde 4 Kaynak hucresine kurali tanimlayan bolumu ister; kurali 05 §10.3.6 tanimlar ve 500'u de kendisi yazar |

### 3.3 Etki işaretleri

K-1569 ve K-1570 satırlarının etki sütununa "(checkpoint — CP10: devredilen sınıf `12`'nin tamamında arandı ve karşılandı; K-1572)" eklendi. `geri_isaret_tara.py 1504 1572`: K-1572'den eski bir satıra eksik geri işaret yok; kalanlar K-1571'in bilinen satırlarıdır (çakışma taramasında çürütülmüş).

## 4. Kontrol 6 — sonraki aşamanın girdileri

Aşama 10 doküman döneminin son aşamasıdır; sonraki adım Implementation dönemidir. Bu checkpoint'ten devreden iş yok: `12` v0.32 kapanışa girer (arşiv adımında ✓ ve v1.0 kararı). `DEPLOY_RUNBOOK` Aşama 10 bloğu (üç madde: canlı kanıt satırları, ters vekilin gövde tavanı, DV gözlem kayıtları) çakışma taramasında yazıldı ve F6 kapısına bağlıdır. ⚠ listesi (K-1547, K-1557, K-1560, K-1563, K-1566, K-1568, K-1572) arşiv işaretinden önce proje sahibine tek mesajda gösterilir.

## 5. Dört üst doküman notunun hükmü (K-652: kaynak kazanır, geri besleme aynı PR)

Mercekler dört yerde `12`'nin dayandığı kaynağın kendi içinde çelişkili ya da `12`'yle hizasız olduğunu gösterdi. Hepsi ürün kuralını değiştirmeden tek yazımda karara bağlandı; dış servis ya da sağlayıcı seçimi, para akışı mimarisi gerektirmedi.

| # | Yer | Sorun | Karar | Uygulanan |
|---|---|---|---|---|
| 1 | `07 §6.2.2`, `§6.2.6` | %100 indirim ve %100 yüzdesel kupon üç hata koduna (`InvalidPercentage`, `DiscountedPriceZero`, `CouponValueInvalid`) birden düşüyordu | `04` 7.3.2.9'un alan kuralı (indirim ve yüzdesel kupon sıfırdan büyük, %100'den küçük) belirleyicidir: %100 `InvalidPercentage` alır; `DiscountedPriceZero` yalnız yuvarlanmış indirimli fiyatın sıfıra inmesi, `CouponValueInvalid` yalnız sabit tutarlı kuponun sıfır değeri ve asgari tutar kuralıdır. HTTP durumu (422) değişmedi | `07` iki satır; `12` VAL-A456 (ön koşula %100 indirim ve %100 yüzdesel kupon eklendi), VAL-A515, VAL-A523 |
| 2 | `02 §10.6.3` | Payda "iptal edilmemiş fiziksel kalem" der; gecikme feshine uğrayan kalem iptal sayıldığından aynı hücrenin sonraki cümlesiyle (K-484) çelişir | "süre dolmadan iptal edilmemiş" (`01` M-3 zaten böyle; `12` VAL-A280 aynı değişikliği taşır) | `02` bir cümle |
| 3 | `07 §2.5.2` | "gönderim değeri tüketir" koşulsuzdu; K-1186 (4) ve §4.25'in 422 satırları değerin yalnız işlemi uygulayan gönderimde tüketildiğini söyler | koşul yazıldı: doğrulama hatasında ve onay işaretsiz ilk gönderimde tüketilmez | `07` bir cümle; `12` VAL-A620 buna uyumlu (hüküm D11 #5) |
| 4 | `09 §19.1` madde 6 | Nesne depolamanın sahtesi/test hâli hiçbir yerde sayılmıyordu; VAL-A975'in "sahte nesne depolama"sı ve `12 §7.4` `mock` tanımı buna dayanıyor | nesne depolamanın uygulamada arayüzü yoktur ve madde 3'ün sahteleri arasında değildir; hedefi erişilemez kılma doğrulama ortamının işidir | `09` bir cümle |

Karar 1 doğrulayıcının önerisini izler. Bunun ürün davranışı değil API sözleşmesi hata kodu eşlemesi olduğu görüldü; kayıt "⚠" taşımaz, ama K-1572 satırı severity kaymasıyla zaten ⚠'dir ve dört madde onun içinde sayılır.

## 6. Öğrenim terfisine notlar — 5. adımın girdisi

1. **Mercek paketi boyutu:** Sonnet/low mercek 60 KB iç tutarlılık paketinde 20 / 20 bitirdi, açık bölüm yok (Aşama 9'da tek-tek ihlaller 80 KB'ta görülmüştü) — ≤ 60 KB kuralı doğrulandı.
2. **"Bulduğun anda ekle" kuralı zayıf:** 20 merceğin 7'si bulgularını sonda tek seferde yazdı; kayıp olmadı ama kural uygulanmıyor. Aday: bulgu dosyasını mercek başlığında ön yaratmak ya da talimatı somutlaştırmak.
3. **Aynı kural, farklı severity:** "VAL-A856 / A848 / A855 / A084 / B019 / E094" kalıbı 7 yerde çıktı: §8.1'in "yasal yükümlülük, tutar ya da başka müşterinin verisi" sınıfı satır yazımında tutarlı uygulanmıyor. L5 adayı: `12_matris_denetle.py`'ye "aynı Kural/Kaynak, farklı severity" uyarı sınaması.
4. **`SATIR:` biçimi etkili:** doğrulayıcı sonrası uygulanabilir `SATIR:` oranı yüksekti (48 / 73); tablo hücresi `12_audit_satir_uygula.py` tarzı VAL kimliği + sütun adıyla uygulanır, ESKİ'nin tekil olması gerekmez. Bu adımın betiği hüküm dosyalarını doğrudan ayrıştırdı (dönüş metni değil); 40 ESKİ'nin hepsi tek eşleşme verdi.
5. **Üst doküman çelişkisi aynı PR'da kapatılabildi:** dört notun dördü de tek yazımla (K-652) çözüldü; "Açık nokta" olarak bırakmaya gerek kalmadı. Aday: kalite döngüsü sonu kuralı "ürün kuralı değiştirmeyen üst doküman hizası checkpoint PR'ında uygulanır".
6. **Severity sayımı yerleri:** §1 tablosundaki sayımlar (KRİTİK/ORTA/DÜŞÜK, tür etiketleri) severity kaymasından sonra elle düzeltilir; betik `12_matris_denetle.py` dağılımı verir ama §1'in hücresiyle karşılaştırmaz. L5 adayı: §1 hücresiyle karşılaştırma.
7. **Çürüyen örüntü:** 16 çürükten çoğu "kasıtlı fark" ya da "önceki turda karşılanmış"; mercek talimatına "ortam ve severity farkı bölümün girişinde ya da §7.4'te açıklanmış olabilir" notu eklenebilir (ham bulgu gürültüsünü azaltır).
8. **Görev metni sayısı:** yönetici görev metni severity kaymasını 11 satır diyordu; hüküm dosyalarından ve `12` farkından sayım 10 çıktı (7 + 1 + 2). Sayıyı betikle (`b3_fark.py` benzeri) doğrulamak gerekir.

## 7. `00`, `CLAUDE.md`, `SETUP.md` önerileri

Bu adım üç dosya için değişiklik önerisi çıkarmadı. Öğrenim notları (§6) L3/L5 hedeflidir ve 5. adımda ele alınır.

---

*Kaynak: K-437 (kapanış sırası, 4. adım) · K-1569, K-1570 (devredilen sınıflar) · K-1571 (çakışma taraması) · K-652 (üst doküman geri beslemesi) · K-1572 (bu adımın kaydı) · `checklists/document-stage.md` §7 · `.claude/skills/checkpoint/SKILL.md`.*

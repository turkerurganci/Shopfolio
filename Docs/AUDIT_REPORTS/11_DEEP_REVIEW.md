# Deep Review Raporu — 11 Implementation Plan

**Tarih:** 2026-10-31 · **Hedef:** `Docs/11_IMPLEMENTATION_PLAN.md` v0.33 (bulgular v0.33'te üretildi, v0.34'te işlendi; taban `main` = 669acca) · **Bağlam:** `PRODUCT_DISCOVERY_STATUS.md` K-1401…K-1470; üst dokümanlar `02`–`10`; `AUDIT_REPORTS/11_AUDIT.md` · **Odak:** audit'ten sonra koşan deep review (skill Kurallar 7: kapsam ve tutarlılık yeniden tam koşmaz) — audit kararlarının doğurduğu yeni tutarsızlıklar, söz kayıpları ve altı derinlik katmanı.

### Koşum biçimi (K-431, K-1232; `deep-review` ve `audit` skill'leri, "Koşum biçimi")

Adım iki parçaya bölündü (`INSTRUCTIONS §9` (8)). **Parça A:** paketleyici (`scripts/doc-scans/11_dr_paketle.py`) mercek paketlerini üretti, mercekler okudu, `11_dr_birlestir.py` bulguları birleştirip doğrulayıcı görev paketlerini kurdu, doğrulayıcılar hükme bağladı; her bulgunun ESKİ → YENİ şartnamesi hüküm dosyasında tutuldu. **Parça B:** şartnameler `11_audit_uygula.py` ile tek seferde uygulandı, üst doküman geri beslemesi yapıldı, kapılar koşuldu, kayıt yazıldı. Çalışma dizini: `C:/Users/teknodeveloper/.claude/helper-state/shopfolio/kd-deep/` (yardımcı durum dizini, depoda değil).

- **Paketleyici.** Her mercek için satır numaralı dar paket (≤ 85 KB): kapsamdaki task blokları, 7.1'de bağlı kaynak birimlerin metni, ilgili K satırları. İki yönlü sına: taban okunamazsa uyarı, bozuk girdi (bölüm başlığı silinmiş) `SystemExit`.
- **Yirmi bir mercek** (18 paket + 3 kuyruk), hepsi `[ozet]` Sonnet / low, beşli dalga. Üç mercek paketini kısmen okudu (AY_02, VA_02, OB_01); kuyrukları AY_02b, VA_02b, OB_01b ile kapsandı.
- **Ham bulgu: 191** (Yüksek 12 · Orta 146 · Düşük 33).
- **Kademeli doğrulama — 20 doğrulayıcı, hepsi `[dogrulama]` Opus / high** (adım başına sınır; sayaç 20/20, bekçi olayı yok): D1…D12 Yüksek tek (12 bulgu) · D13…D19 Orta 4'lü (7 grup, 28 bulgu) · D20 söz kaybı grubu (8 bulgu, Orta ve Düşük karışık).
- **Kademe kuralından sapma (D20).** Söz kayıpları (K-1459, K-1463…K-1470'in bayt kısaltmaları) bu adımın zorunlu girdisiydi; 20'lik sınırda son doğrulayıcı yuvası onlara ayrıldı ve sekiz bulgu seviyesine bakılmadan tek grupta hükme bağlandı (kademe kuralı Orta'yı aynı bölümden 2–4'lü, Düşük'ü 5–8'li ister). Sekiz bulgu tek modülde değil beş modülde olduğu için grup içi okuma maliyeti yüksekti; hükümler bulgu başınadır.
- **Hüküm:** doğrulanan 48 bulgudan **12 doğru · 22 kısmen · 14 çürük**. Yinelenen bulgu hükmü yok; şartname çifti 82 (1 birleştirme: #136 ESKI'si #2'nin hükmüne bağlandı). Seviyesi **yükselen bulgu yok**; değişenlerin hepsi düştü (12 Yüksek → Orta ya da Düşük; 8 Orta → Düşük). `kd-deep/yeniden.md` oluşmadı.
- **Doğrulanmayan: 143 bulgu** (Orta ve Düşük). Dokümana uygulanmaz; `KD-deep-2a`, `2b` adımlarında doğrulanır (doğrulamasız uygulama, örnekleme ya da atlama yoktur).

### Mercek tablosu

| Mercek | Katman | Paket (bayt) | Ham bulgu |
|---|---|---|---|
| AY_01 (söz kayıpları + kayıp blokları) | 1–2 (dar) | 77.449 | 12 |
| AY_02 / AY_02b | 1–2 | 84.799 | 10 + 4 |
| AY_03 | 1–2 | 83.133 | 10 |
| AY_04 (audit öncesi v0.25 ile fark) | 1–2 | 42.001 | 7 |
| TD_F0F1 | 3 | 63.844 | 12 |
| TD_F2 | 3 | 72.832 | 12 |
| TD_F3_01 / _02 / _03 | 3 | 82.512 / 84.838 / 65.116 | 12 / 12 / 10 |
| TD_F4F6 | 3 | 28.965 | 11 |
| TD_F5_01 / _02 | 3 | 84.291 / 45.600 | 11 / 12 |
| GV_01 / GV_02 | 4 | 83.713 / 79.496 | 12 / 7 |
| HM_01 | 5 | 79.096 | 9 |
| VA_01 / VA_02 / VA_02b | 6 | 84.596 / 79.315 / — | 11 / 5 / 2 |
| OB_01 / OB_01b | 7–8 | 79.385 | 10 |

### Özet Skor Tablosu

| Katman | Ham | Hükme bağlanan | Doğru | Kısmen | Çürük | Doğrulanmadı |
|---|---|---|---|---|---|---|
| 1-2 Kapsam + tutarlılık (dar: audit sonrası) | 43 | 15 | 5 | 6 | 4 | 28 |
| 3 Teknik derinlik | 92 | 28 | 7 | 14 | 7 | 64 |
| 4 Güvenlik (OWASP) | 19 | 3 | 0 | 1 | 2 | 16 |
| 5 Hata modu | 9 | 0 | 0 | 0 | 0 | 9 |
| 6 Veri akışı | 18 | 1 | 0 | 1 | 0 | 17 |
| 7-8 Ölçeklenebilirlik + bağımlılık | 10 | 1 | 0 | 0 | 1 | 9 |
| **Toplam** | **191** | **48** | **12** | **22** | **14** | **143** |

Hükme bağlanan 48, kademe kuralına göre seçildi: 12 Yüksek bulgunun hepsi, sonra puana göre Orta (28) ve söz kayıpları (8). Katman payı, bulgunun mercek kaynağından; elenen yinelenenler kaynak katmanda sayıldı. Güvenlik ve hata modu katmanlarının bulguları Orta ve Düşük olduğu için bu adımda doğrulanmadı — Yüksek güvenlik ya da veri kaybı bulgusu çıkmadı (en yüksek 12 Yüksek bulgunun hepsi sıra/bağımlılık sorunudur); doğrulanmayanlar `KD-deep-2a`/`2b`'de önce güvenlik ve hata modu sırasıyla ele alınır.

### Doğrulayıcı tablosu

| Doğrulayıcı | Model | Kademe | Bulgu | Hüküm |
|---|---|---|---|---|
| D1…D12 | Opus / high | Yüksek tek | #23, #27, #82, #83, #84, #85, #94, #95, #118, #119, #120, #151 | 2 doğru (#82, #85) · 6 kısmen (#27, #84, #94, #119, #120, #151) · 4 çürük (#23, #83, #95, #118) |
| D13 | Opus / high | Orta 4'lü | #106, #1, #2, #116 | 1 doğru · 2 kısmen · 1 çürük |
| D14 | Opus / high | Orta 4'lü | #113, #108, #136, #143 | 1 doğru · 3 kısmen |
| D15 | Opus / high | Orta 4'lü | #19, #91, #112, #30 | 3 kısmen · 1 çürük |
| D16 | Opus / high | Orta 4'lü | #121, #177, #49, #78 | 1 kısmen · 3 çürük |
| D17 | Opus / high | Orta 4'lü | #127, #123, #153, #154 | 1 çürük · 2 kısmen · 1 doğru |
| D18 | Opus / high | Orta 4'lü | #86, #92, #45, #93 | 2 doğru · 1 kısmen · 1 çürük |
| D19 | Opus / high | Orta 4'lü | #11, #146, #147, #61 | 3 çürük · 1 kısmen |
| D20 | Opus / high | Söz kaybı 8'li | #3…#10 | 5 doğru (#4, #7, #8, #9, #10) · 3 kısmen (#3, #5, #6) |

Kesin sayı için aşağıdaki "Bulgu → hüküm → uygulama" tablosu esastır; bu tablo yalnız grup dökümüdür.

### Bulgu → hüküm → uygulama

Seviye sütunu doğrulayıcının nihai seviyesidir; "↓" mercekten düşüşü gösterir. Uygulama: şartname çifti sayısı yardımcı durum dizinindeki `uygulanan.md`'de (82 çift).

| # | Task | Bulgu (özet) | Hüküm | Seviye | Uygulandı |
|---|---|---|---|---|---|
| 23 | T312 | Toplamı aşan parça için madde 8 ile KK8 farklı kod der | çürük | Düşük ↓ | hayır (sartname yok) |
| 27 | T349 | Kargo ücreti iadesi "yalnız iki hâl"; S9 iptali ve fesih ayrı kural | kısmen | Orta ↓ | evet (11 + üst doküman 02 §3.19.6) |
| 82 | T06 | Kabul kriteri canlılık ucunu (T01) şart koşar, bağımlılıkta yok | doğru | Yüksek | evet |
| 83 | T101 | T03 iskeleti ve veritabanı altyapısına bağ yok | çürük | Düşük ↓ | hayır (sıra K-1470'te çözüldü) |
| 84 | T106 | Göç, T10'un rol ayarlarını gerektirir; F1 zinciri belirsiz | kısmen | Orta ↓ | evet |
| 85 | T10 | Göç geçmişi tablosunu kapatma adımı ilk göçte; kriter yok | doğru | Yüksek | evet |
| 94 | T225 | T225 ile T230 döngüsel bağlı (günlük kurulumu) | kısmen | Yüksek | evet |
| 95 | T213 | KK8 biçimleyiciyi T230'dan ister | çürük | Düşük ↓ | hayır |
| 118 | T348 | Havalede sepet "ödendi"ye kadar dolu kalır | çürük | Düşük ↓ | hayır |
| 119 | T342 | Google girişi kriteri T343'ten önce sınanıyor | kısmen | Yüksek | evet |
| 120 | T340 | Sipariş sayfası T354'te; T340 KK'ları sayfa gerektirir | kısmen | Orta ↓ | evet |
| 151 | T525 | Panel kaydetme ucunda 409 / 422 öncelik sırası ters | kısmen | Orta ↓ | evet |
| 106 | T304 | "Aşım" eşiği >=20 mi >20 mi | doğru | Düşük ↓ | evet |
| 1 | T305 | Yeniden gönderimde "tek cümle" ile T306/T307 çelişir | çürük | Düşük ↓ | hayır |
| 2 | T305 | `OriginalDispatchId` anlamı ve ayrımın örneği düştü | kısmen | Düşük ↓ | evet |
| 116 | T305 | 600 piksel, düz metin ve kontrast kriterleri | kısmen | Düşük ↓ | evet |
| 113 | T308 | "İç anahtar olayın değerindedir" belirsiz | kısmen | Düşük ↓ | evet |
| 108 | T309 | KK5 kriter 5'i sınamıyor; "yeni bağlantı üretilir" | kısmen | Orta | evet |
| 136 | T385 | Aralıklar birikimli mi, "üçü" ilk dahil mi | kısmen | Orta | evet (bir çift #2'ye birleştirildi) |
| 143 | T407 | "Üç izin" ile "üç ifade" | doğru | Orta | evet |
| 19 | T104 | Audit kısıtlarının KK karşılığı yok | kısmen | Düşük ↓ | evet |
| 91 | T105 | "Büyüktür" neyden büyük belirsiz | kısmen | Orta | evet |
| 112 | T312 | Range isteğinin ömür dolduktan sonraki hâli | kısmen | Düşük ↓ | evet |
| 30 | T332 | "Tablo yok" ve "ayar/kanca yok" kuralları düştü | çürük | Düşük ↓ | hayır |
| 121 | T335 | Dosya modeli T340'ta; KK4/KK5 sınanacak dosya yok | çürük | Orta | hayır |
| 177 | T335 | OrderItem varyant bağı silmede | kısmen | Orta | evet |
| 49 | T336 | Farklı KDV oranlı kalemlerde kargo oranı | çürük | Orta | hayır |
| 78 | T336 | Aynı konu (kargo KDV ayrıştırması) | çürük | Orta | hayır |
| 127 | T336 | Eşit parçada kuruş artığı | çürük | Düşük ↓ | hayır |
| 123 | T337 | KK'lar sipariş/ödeme task'larını gerektirir | kısmen | Orta | evet |
| 153 | T501 | T501 ürün sayfası kriterleri T503'ten önce | doğru | Orta | evet |
| 154 | T501 | Genişlik sınıfı kriterleri Unit testle sınanmaz | kısmen | Orta | evet |
| 86 | T102 | İstisnalı kural, KK4 istisnayı sınamıyor | doğru | Orta | evet |
| 92 | T119 | İki yasal metin kümesi belirsiz | kısmen | Düşük ↓ | evet |
| 45 | T120 | "Kısıtlayıcı" tanımı | çürük | Düşük ↓ | hayır (11:426 ve 06 §2.2.5 karşılıyor) |
| 93 | T120 | Denetimin `Consistency` özelliği ve adımı | doğru | Orta | evet + üst doküman 09 §21.4 |
| 11 | T601 | AC10 şu an sınanamaz | çürük | Düşük ↓ | hayır |
| 146 | T601 | Canlı kanıt bağımlılığı boş | çürük | Düşük ↓ | hayır |
| 147 | T601 | Senaryo kimlikleri henüz yok | çürük | Düşük ↓ | hayır |
| 61 | T602 | Geride kalmış kurulumun sürümü | kısmen | Düşük ↓ | evet |
| 3 | T379 | "Aynı adrese" düşünce kural | kısmen | Orta | evet |
| 4 | T377 | Kriter 1 turun zamanlanmış tetiğinden söz etmez | doğru | Orta | evet |
| 5 | T376 | "Acil yama" ölçülemez ifade | kısmen | Orta | evet |
| 6 | T373 | AC8 IBAN'ı koşulsuz siler, AC10 paylaşılanı silmez | kısmen | Düşük ↓ | evet |
| 7 | T373 | "Tarih" hangi tarih olduğunu söylemiyor | doğru | Düşük | evet |
| 8 | T341 | KK7 "doğrulama" e-posta doğrulaması gibi okunur | doğru | Düşük ↓ | evet |
| 9 | T380 | Sekiz alanın yazıcıları bağımlılıkta yok | doğru | Orta | evet |
| 10 | T356 | Kısaltmada bağlaç düşmüş, cümle bozuk | doğru | Düşük | evet |

(Tablodaki numaralar `kd-deep/bulgular.md` sıra numarasıdır; "Task" sütunu bulgunun ana task'ıdır.) Yüksek bulguların dağılımı: 12 Yüksek bulgudan 4'ü Yüksek kaldı (#82, #85 doğru; #94, #119 kısmen), 4'ü Orta'ya düştü (#27, #84, #120, #151), 4'ü Düşük'e düştü ve çürüdü (#23, #83, #95, #118).

**Uygulanan:** 34 bulgu (12 doğru + 22 kısmen), **82 şartname çifti** `11_audit_uygula.py` ile tek seferde uygulandı (sorun 0; elle satır yok, uygulanamayan çift yok). Çürütülen 14 bulgu dokümana dokunmadı.

### ⚠ ayrımı (proje sahibine bilgi)

Bu adımda **KARAR yok**: doğrulayıcılar hiçbir bulguda ürün kuralı seçimi gerektirmedi; 12 Yüksek bulgu sıra, bağımlılık ve kabul kriteri netliği sorunlarıdır. Üst doküman geri beslemesi (aşağıda) kural değişikliği değil, metin hizalamasıdır. `⚠` taşıyan yeni karar yok.

### Söz kayıplarının hükmü

Audit ve KD-audit-2'de blok bayt sınırı (2.560) için kısaltılan cümleler (K-1459, K-1463, K-1464, K-1465, K-1467, K-1468, K-1469, K-1470) AY_01 merceğince kayıp cümle ve güncel blokla yan yana okundu; sorun gördüğü 12 bulgudan 11'i doğrulandı (biri `dogrulanmadi`'da). T527 "kalem seçtirmez", T374 AC3/AC4, T330-T339 (K-1465) ve T306/T307 (K-1470) için mercek bulgu üretmedi (anlam kaybı yok).

| Kayıp | Bulgu | Hüküm | Uygulandı mı |
|---|---|---|---|
| T373 AC4 "ulaşma tarihiyle" (K-1459) | #7 | doğru (kural kaybı VAR) | evet — hangi tarih olduğu geri yazıldı |
| T373 IBAN (K-1459) | #6 | kısmen | evet — AC8 ile AC10 uzlaştırıldı |
| T341 KK7 "doğrulama istemez" (K-1465) | #8 | doğru | evet — "e-posta doğrulaması" açıkça yazıldı |
| T356 bozuk cümle ", hizmette" (K-1464) | #10 | doğru | evet — "ve" ile onarıldı |
| T380 bağımlılık (K-1469) | #9 | doğru | evet |
| T602 kriter 9 "tur gece çalışmaz" | #4 | doğru | evet |
| T602 "acil yama" | #5 | kısmen | evet — ölçülebilir ölçüt |
| T379 "aynı adrese" (K-1468) | #3 | kısmen | evet |
| T305 | #1 | çürük (kayıp anlam kaybı değil) | hayır |
| T305 / T385 | #2 | kısmen (`06 §4.58` atfı) | evet |
| T601 | #11 | çürük | hayır |
| T342 KK3 "iki ayrı biçimde" (K-1463) | #12 | doğrulanmadı (yer yok) | `KD-deep-2a`'ye |
| T527 KK6 "kalem seçtirmez" | — | mercek bulgu üretmedi | — |
| T374 AC3 / AC4 | — | mercek bulgu üretmedi | — |
| T330-T339 (K-1465), T306/T307 (K-1470) | — | mercek bulgu üretmedi | — |

**Bu adımın kendi kısaltmaları.** Şartnameler iki bloğu 2.560 baytın üstüne itti; ikisi anlam kaybı olmadan kısaltıldı: **T312** (2.565 → ≤ 2.560; KK4'te "isteği", "dolduktan" ve "tamamlanır" sözleri sadeleştirildi) ve **T385** (2.562 → ≤ 2.560; kriter 2'de "müşteriye giden bildirimde ilgili siparişin" → "müşteri bildiriminde siparişin"). Kural kaybı yok; söz kaybı yalnız dolgu sözcükleridir. `11_task_blok_denetle.py --tam` sorun 0.

### Üst dokümana geri besleme (K-652)

İki doğrulayıcı notu üst dokümanda gerçek bir tutarsızlık buldu; ikisi de **kural değişikliği değil, metin hizalamasıdır** (yeni kural numarası yok, ✓ korunur, kalite döngüsü yeniden açılmaz). Geri besleme tablosu: bu bölüm (11 çıktı dokümanının geri besleme satırı yoktur; sürüm satırı her iki dokümanın dipnotundadır).

| Not | Doküman | Düzeltme | Sürüm |
|---|---|---|---|
| hukum 2 (#27, T349) | `02 §3.19.6` — kargo ücreti iadesi "iki hâl" derken `02 §7.2.9`'un S9 (geri dönen gönderi) kuralı iptali ve gecikme feshini ayrıca geri ödetiyor (iç çelişki) | iade cümlesine "geri dönen gönderide (S9) iptal ve gecikme feshi §7.2.9'un ayrı kuralına bağlıdır" eklendi | `02` v0.106 |
| hukum 18 (#93, T120) | `09 §21.4` madde 1 — "beş denetim … Beşi" kapalı listesi; `11` model, uç, gezinme, durum × rol ve form envanteri eşitlik denetimlerini de (T120, T538, T546, T547) aynı sınıfta ister | "beş denetim" sözü kaldırıldı; liste "en az şu beşi" biçimine çevrildi, `11`'in denetimleri aynı klasörde, özellikle ve adımda koşar | `09` v0.26 |

**Anahtar terim taraması (K-1471).** `02 §3.19.6` için terimler: "kargo ücreti" + "geri öde" (nesne ve işlem), "geri dönen gönderi", "S9", "ödenmiş kargo ücreti". Eşleşmeler `02 §3.23.2`, §7.2.9, §7.4.6, `03` S9 akışı ve `07` ilgili uç yan etkileridir; hepsi ya §7.2.9'a atıf yapar ya kargo ücretini kalemin iptaliyle bağlar. §7.2.9 zaten S9 kuralını yazar ve değişmedi; `03` ve `07` kargo ücretinin geri ödenmesini "iki hâl" diye sayarak yinelemez, elenir. `09 §21.4` için terimler: "beş denetim", "Beşi", "eşitlik denetimleri", "kapalı liste"; `09`'da tek eşleşme budur, `05` ve `11` "eşitlik denetimi" sözünü kapalı sayı olmadan anar; elenen eşleşmelerin hiçbiri sayıyı yinelemez.

### Doğrulanmayan bulgular (sonraki adımın girdisi)

**143 bulgu** (Orta ve Düşük) bu adımda doğrulanmadı ve dokümana uygulanmadı — adım başına 20 Opus doğrulayıcı sınırı. Liste puana göre sıralı: `C:/Users/teknodeveloper/.claude/helper-state/shopfolio/kd-deep/dogrulanmadi.md` (yardımcı durum dizini). #12 (T342 KK3 "iki ayrı biçimde" söz kaybı) dahil. Sıradaki adımlar **KD-deep-2a, 2b**: Orta 4'lü, Düşük 8'li gruplarla; güvenlik (GV) ve hata modu (HM) bulguları önce, sonra veri akışı (VA), ölçeklenebilirlik (ÖB) ve teknik derinlik (TD).

### Aksiyon Planı

1. **Bu adım:** 34 bulgu / 82 şartname çifti `11` v0.34'e uygulandı; `02` v0.106, `09` v0.26 geri besleme.
2. **KD-deep-2a, 2b:** 143 doğrulanmamış bulgu doğrulanır ve uygulanır.
3. **Sonra:** cross-review (Aşama 9 kalite döngüsünün son adımı) ve aşama kapanışı.

### Sınırlar

- Mercekler Sonnet / low'dur ve bulgu kaçırabilir; altı mercek paketini kısmen okudu (kuyruk mercekleriyle kapsandı). Kapsam, mercek kapsamıyla sınırlıdır; plandaki 214 task'ın her cümlesi gözle okunmadı.
- Doğrulama adım başına 20 Opus doğrulayıcıyla sınırlıydı; 143 bulgu (%75) doğrulanmadı. Bu bulguların sayısı yüksektir, hiçbiri güvenlik ya da veri kaybı Yüksek'i değildir (merceğin seviyesidir; doğrulayıcı onaylamadı).
- Kapsam ve tutarlılık katmanı audit'te tam koştuğu için bu adımda dardır (Kurallar 7); audit'in kararlarının doğurduğu yan yüzleri AY merceği taradı.
- Söz kayıpları yalnız hükme bağlanan bulgular için kesindir; yer kalmayan #12 ve mercekçe bulgu üretilmeyen kayıplar için "anlam kaybı yok" mercek yargısıdır.
- Rapor biçimi `09_DEEP_REVIEW.md`'den uyarlandı; çürütülen bulguların tek tek gerekçesi hüküm dosyalarındadır (depoda değil).

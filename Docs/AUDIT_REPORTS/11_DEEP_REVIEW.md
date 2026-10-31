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

### Doğrulanmamış bulgular — KD-deep-2a

Plan (`scripts/doc-scans/11_dr_grupla2.py`) 143 bulguyu 39 doğrulayıcı grubuna ve iki adıma böldü (2a 19, 2b 20; adım başına sınır 20). **2a'da 19 doğrulayıcı koştu**, hepsi `[dogrulama]` Opus / high; bekçi olayı yok (sayaç 19/20; D19'un ilk açılışı eşzamanlı 20 ajan sınırına takıldı, hüküm yazılmadan reddedildi ve bir kez yeniden açıldı). Doğrulamasız uygulama, örnekleme ya da atlama yoktur.

| Doğrulayıcı | Model | Kademe, modül | Bulgular | Hüküm |
|---|---|---|---|---|
| D1 | Opus / high | Düşük 5'li, karma F3 | #12, #36, #138, #35, #189 | 2 doğru · 0 kısmen · 3 çürük |
| D2 | Opus / high | Düşük 6'li, karma F3 | #117, #71, #80, #129, #128, #139 | 1 doğru · 3 kısmen · 2 çürük |
| D3 | Opus / high | Düşük 3'li, karma F4 | #60, #150, #149 | 0 doğru · 1 kısmen · 2 çürük |
| D4 | Opus / high | Düşük 6'li, karma F5 | #161, #41, #42, #62, #160, #43 | 1 doğru · 3 kısmen · 2 çürük |
| D5 | Opus / high | Orta 3'li, Ödeme | #183, #65, #130 | 0 doğru · 1 kısmen · 2 çürük |
| D6 | Opus / high | Orta 4'li, Ödeme | #33, #140, #144, #142 | 0 doğru · 2 kısmen · 2 çürük |
| D7 | Opus / high | Orta 3'li, Ödeme | #145, #141, #66 | 1 doğru · 2 kısmen · 0 çürük |
| D8 | Opus / high | Orta 3'li, İptal, cayma ve iade | #107, #134, #155 | 0 doğru · 2 kısmen · 1 çürük |
| D9 | Opus / high | Orta 2'li, İptal, cayma ve iade | #37, #158 | 0 doğru · 1 kısmen · 1 çürük |
| D10 | Opus / high | Orta 4'li, Sepet ve sipariş | #48, #115, #110, #111 | 1 doğru · 2 kısmen · 1 çürük |
| D11 | Opus / high | Orta 4'li, Sepet ve sipariş | #176, #122, #29, #57 | 1 doğru · 0 kısmen · 3 çürük |
| D12 | Opus / high | Orta 4'li, Sepet ve sipariş | #178, #124, #131, #133 | 0 doğru · 2 kısmen · 2 çürük |
| D13 | Opus / high | Orta 4'li, Sepet ve sipariş | #187, #34, #157, #156 | 1 doğru · 0 kısmen · 3 çürük |
| D14 | Opus / high | Orta 3'li, Üyelik | #31, #52, #32 | 0 doğru · 1 kısmen · 2 çürük |
| D15 | Opus / high | Orta 3'li, Üyelik | #56, #188, #38 | 1 doğru · 2 kısmen · 0 çürük |
| D16 | Opus / high | Düşük 1'li, karma F1 | #22 | 0 doğru · 1 kısmen · 0 çürük |
| D17 | Opus / high | Orta 3'li, Ayarlar | #190, #164, #191 | 0 doğru · 1 kısmen · 2 çürük |
| D18 | Opus / high | Orta 2'li, Kurumsal içerik | #126, #125 | 0 doğru · 1 kısmen · 1 çürük |
| D19 | Opus / high | Orta 2'li, Modüller | #24, #179 | 0 doğru · 2 kısmen · 0 çürük |

**Hükümler (65 bulgu):** 9 doğru · 27 kısmen · 29 çürük. Şartname çifti 73 ham; **yükselen #117** (T306 kesinti sonrası gönderim, Düşük → Orta; 6 çift) bu adımda **uygulanmadı** (`kd-deep-2/yeniden.md`; KD-deep-2b'nin sonunda ya da `KD-deep-2r`'de yeniden doğrulanır). Uygulanan: **35 bulgu, 66 şartname çifti** `11_audit_uygula.py` ile tek seferde (67 çift − 1 birleştirme: #133'ün 2. şartnamesi, aynı k8 satırını aynı kısaltmayla değiştiren #187'nin 3. şartnamesinde eridi; sorun 0). **Seviyesi düşen 25 bulgu** (Orta → Düşük) uygulandı: #183, #65, #130, #33, #140, #107, #155, #37, #115, #110, #29, #178, #124, #34, #157, #156, #31, #52, #32, #188, #38, #164, #191, #126, #24. 29 çürük hükümde şartname yoktur (çoğu "zaten karşılandı: 11:<n>"). Elle satır: T331'in bağımlılık satırı (11:2457, `T330` → `T330, T380`) satır numarasıyla yapıldı (ESKI parça olarak üç yerde geçtiği için şartnamede tekil kurulamamıştı).

### Bulgu → hüküm → uygulama (KD-deep-2a)

Numaralar `kd-deep/bulgular.md` sıra numarasıdır; "Task" ana task'tır; "↓" mercekten düşüşü gösterir.

| # | Task | Bulgu (özet) | Hüküm | Seviye | Uygulandı |
|---|---|---|---|---|---|
| 12 | T342 | T342 AC8 bütün kuralları yönetici hesaplarına uygular, T521 yönetici y | doğru | Düşük | evet (1 çift) |
| 36 | T352 | Aynı kural iki task'ta kopyalanmış (KK8 ve T346 KK2 aynı davranışı sın | çürük | Düşük | hayır |
| 138 | T367 | Karışık siparişin kalan hatta düşmesi (k3'ün ikinci yarısı) hiçbir KK' | doğru | Düşük | evet (1 çift) |
| 35 | T383 | Günlük sayının ödemesi tamamlanmış siparişlerden oluştuğu kaydı düştü | çürük | Düşük | hayır |
| 189 | T384 | Yönetici e-posta düzeltmesinden sonra "donmuş" e-postanın yeni adrese | çürük | Düşük | hayır |
| 117 | T306 | Kesinti sonrası gönderim (kriter 5) ve hesabı silinmiş siparişte erişi | doğru | Orta ↑ | hayır (Orta'ya yükseldi → yeniden doğrulama) |
| 71 | T308 | F-1…F-4'ün yeniden denenip denenmediği T308'de açık değil (F-5/F-6 den | kısmen | Düşük | evet (4 çift) |
| 80 | T312 | "boy aşımı 400/422" hangi durumda 400 hangisinde 422 olduğunu söylemiy | çürük | Düşük | hayır |
| 129 | T354 | Adres/telefon/IBAN taşıyan sayfayı süresiz açan taşıyıcı bağlantı | çürük | Düşük | hayır |
| 128 | T361 | "Her zaman" ifadesi T359 k4'teki yedek davranışla çelişiyor | kısmen | Düşük | evet (1 çift) |
| 139 | T386 | Hangi bildirimin (B-11/B-12) hangi müdahalede gittiği kriterlerde adla | kısmen | Düşük | evet (4 çift) |
| 60 | T401 | DV-38/39/41 (ve T402'de DV-36) hem task'ın test modunda gözleyeceği he | kısmen | Düşük | evet (1 çift) |
| 150 | T401 | Sunucuda 80/443 önünde ters vekil varsa "istek anındaki adres" vekilin | çürük | Düşük | hayır |
| 149 | T409 | Kod değişiminde Google'ın kalıcı hata yanıtları (örn | çürük | Düşük | hayır |
| 161 | T501 | İki kriter elle gözlem/belge işi | çürük | Düşük | hayır |
| 41 | T503 | GET'te eksik seçim varsayılana düşerken AddCartItem'da "seçilmemiş" 42 | çürük | Düşük | hayır |
| 42 | T517 | Aynı kavram T517'de "tarayıcı işaretleri", T519'da "tanınan tarayıcı i | kısmen | Düşük | evet (1 çift) |
| 62 | T517 | Müşteri tarafı işaretlerin tamamını (değiştirenin tarayıcısı dahil) si | kısmen | Düşük | evet (0 çift) |
| 160 | T522 | "girişe iner" durum kodu ve hedefi (302/303, `/panel/giris`) vermiyor | kısmen | Düşük | evet (1 çift) |
| 43 | T529 | "İlki/ikincisi" işlem adlarına bağlı değil, ters okunması kolay | doğru | Düşük | evet (1 çift) |
| 183 | T353 | Kart için iptal sonrası gelen ödemenin geri ödenmesi tanımlı, havale i | çürük | Düşük ↓ | hayır |
| 65 | T370 | Sonucu bilinmeyen iadeyi kimin, ne zaman ve kaç kez sorgulayıp kapattı | çürük | Düşük ↓ | hayır |
| 130 | T370 | "ikisinden geç olanı" hangi iki tarihi kastettiği yazılı değil (beyan | kısmen | Düşük ↓ | evet (2 çift) |
| 33 | T401 | Sınıra uymayan (ör | çürük | Düşük ↓ | hayır |
| 140 | T402 | Kabul kriterlerinde "tanınmayan değer" sırasının dört kolu sayılmamış | çürük | Düşük ↓ | hayır |
| 144 | T404 | AC1 beş girdi sayar, KK1 "altı" der | kısmen | Orta | evet (3 çift) |
| 142 | T408 | "Pencere" iki şey olabilir | kısmen | Orta | evet (3 çift) |
| 145 | T404 | Cümle iki biçimde okunur | kısmen | Orta | evet (2 çift) |
| 141 | T406 | Bildirimde 3D'li ya da 3D'siz ayrımı yapan bir alan yok | doğru | Orta | evet (2 çift) |
| 66 | T510 | Müşteri ödeme sayfasını yenilerse yeni jeton istenir ve sağlayıcı ikin | kısmen | Orta | evet (2 çift) |
| 107 | T320 | İki yıllık sürenin başlangıcı (teslim, ilk açılış, son çözüm) yazılı d | kısmen | Düşük ↓ | evet (2 çift) |
| 134 | T374 | Bedel indiriminin tutarı kimden/nasıl gelir (firma girer mi, üst sınır | kısmen | Orta | evet (2 çift) |
| 155 | T512 | IBAN isteği ve "havale yolu açıldı" durumu T529'da (`RequestRefundIban | çürük | Düşük ↓ | hayır |
| 37 | T528 | Kısaltmada ölçülebilir eşik (bir yıl, Z-11) ve parametre adı düştü | çürük | Düşük ↓ | hayır |
| 158 | T528 | KK10'u karşılayan kabul kriteri yok (k10 yalnız Çözüldü/Açık yapmayı a | kısmen | Orta | evet (3 çift) |
| 48 | T301 | Gönderim kaydı alıcı adresini satırda taşıyorsa (T304 satır bazında ad | çürük | Orta | hayır |
| 115 | T301 | "Kilitten sonra koşulu yeniden okur" (seçim ile kilit arasında başka i | kısmen | Düşük ↓ | evet (1 çift) |
| 110 | T310 | "Okunabilirse" ölçülemez | kısmen | Düşük ↓ | evet (1 çift) |
| 111 | T319 | Yetersiz stokta düzeltmenin reddi ve Z-8/B-3 yeniden başlaması hiçbir | doğru | Orta | evet (3 çift) |
| 176 | T347 | Havalede "ödeme başarılı" ancak günler sonra "ödendi" işaretiyle olur | çürük | Orta | hayır |
| 122 | T348 | Kaybeden onayın sonucu hiçbir yerde yazılı değil | çürük | Orta | hayır |
| 29 | T349 | Aynı cümlede "taban bütün kalemler" ve "siparişe giren kalemlere bakar | doğru | Düşük ↓ | evet (1 çift) |
| 57 | T354 | FindOrder eşleşmesi siparişin üyeye bağlı olup olmadığını ayırmıyor | çürük | Orta | hayır |
| 178 | T354 | Anahtarın ne zaman üretildiği belirsiz (üyenin siparişinde de var mı | çürük | Düşük ↓ | hayır |
| 124 | T364 | Uyarıdan sonra üzerine yazılabilir mi (onayla yaz) yoksa kayıt reddedi | çürük | Düşük ↓ | hayır |
| 131 | T375 | T375 ve T379 işlem izine yazmayı kabul kriteri yapıyor, iz altyapısı i | kısmen | Orta | evet (6 çift) |
| 133 | T386 | "Sıra sorusu"nun içeriği, seçeneklerin sonuçları (pencere/süre nasıl d | kısmen | Orta | evet (3 çift) |
| 187 | T386 | Teslim tarihi sonradan ileri çekilirse, daha önce kaydedilmiş iade tes | doğru | Orta | evet (4 çift) |
| 34 | T390 | "Başka bekleyen geri ödeme IBAN'ı tutuyorsa silinmez" istisnası yalnız | çürük | Düşük ↓ | hayır |
| 157 | T527 | "yoksa" iki koşuldan hangisinin değilini kapsıyor belli değil (teslim | çürük | Düşük ↓ | hayır |
| 156 | T530 | Yanlış iptal (T528) ve geri ödeme/IBAN isteği (T529) olmadan k7-k8 (KK | çürük | Düşük ↓ | hayır |
| 31 | T343 | Dış sağlayıcıyı sahtelemeden akış testinin yapılacağı kuralı kayboldu | çürük | Düşük ↓ | hayır |
| 52 | T343 | Otomatik hesap bağlama güvenliği "Google adres için yetkili" ölçütüne | çürük | Düşük ↓ | hayır |
| 32 | T409 | Kod davranışı kodlama sırasında bilinmeyen canlı doğrulama sonucuna (D | kısmen | Düşük ↓ | evet (1 çift) |
| 56 | T515 | T515 tanınan tarayıcının hiç engellenmediğini söylüyor | doğru | Orta | evet (1 çift) |
| 188 | T516 | Doğrulanmamış (bekleyen) hesap için sıfırlama isteğinin ne yaptığı (ba | kısmen | Düşük ↓ | evet (1 çift) |
| 38 | T517 | "Üç işlem" task'ta adlandırılmıyor | kısmen | Düşük ↓ | evet (1 çift) |
| 22 | T117 | Hiç gönderilmeden `Undelivered` olan (boş gönderim anlı) satırın kabul | kısmen | Düşük | evet (1 çift) |
| 190 | T525 | İki panel kayıt işleminde (SaveProduct, SaveCoupon) 409 ile 422 sırası | çürük | Orta | hayır |
| 164 | T536 | KK10 kabul kriterinde olmayan bir davranışı (onay hâlinin iade malı be | çürük | Düşük ↓ | hayır |
| 191 | T536 | "İki metin" adı verilmemiş | kısmen | Düşük ↓ | evet (2 çift) |
| 126 | T361 | Kaç ürün gösterileceği ve "en yeni"nin neye göre (ilk yayın tarihi mi | çürük | Düşük ↓ | hayır |
| 125 | T362 | "Yeterli kontrast" ve "okunur" ölçülemez | kısmen | Orta | evet (2 çift) |
| 24 | T302 | Z-41 11:1683'te kişisel veri süresi olarak geçer | kısmen | Düşük ↓ | evet (3 çift) |
| 179 | T302 | Tek madde iki ayrı kaydı (ödeme bildirimi, ayıp talebi) karıştırıyor | kısmen | Orta | evet (0 çift) |

### KARAR satırları (KD-deep-2a; toplu onay modu, K-1472)

Üç KARAR doğrulayıcının önerisiyle kaydedildi; üçü de şartnamelerde uygulanan seçenektir.

- **#131 (K-1473, `öneriyle kaydedildi`):** T380 (işlem izi) T379'dan ayrıldı ve F3 yürütme sırasının başına alındı; T379, T331 ve T375 T380'e bağlandı (iz arayüzü ilk yazandan önce doğar, döngü kalkar). `11_geri_yon.py . --denetle` 214 = 214.
- **#187 (K-1474, `toplu onayla kaydedildi — ⚠`):** teslim tarihi kayıtlı bir iade ulaşma tarihinden sonraya düzeltilemez (422); firma önce ulaşma tarihini düzeltir. Cayma ve iade süresine dokunduğu için ⚠ grubunda sayılır; aşamanın ⚠ listesinde proje sahibine tek listede gösterilir. T386 bağımlılığına T373 eklendi, yürütme sırası `T375 → T373 → T386 → T374`.
- **#125 (K-1473, `öneriyle kaydedildi`):** marka rengi üzerindeki yazı rengi WCAG kontrast oranı yüksek olan taraftan seçilir (her renkte en az 4,5:1).

### Blok bayt sınırı (2.560) ve söz kayıpları — KD-deep-2a

Uygulama sonrası iki blok sınırı aştı: **T370** (2.582) ve **T386** (2.568). Bölme yok; ikisi de sözcük sadeleştirmesiyle kısaltıldı (kural kaybı yok):

| Blok | Kayıp | Kural kaybı |
|---|---|---|
| T370 kriter 2 | "sistem kendiliğinden ve onaysız başlatır" → "sistem onaysız başlatır" | yok ("onaysız" aynı anlamı taşır) |
| T370 kriter 8 | "IBAN girildikten sonra yeniden deneme kapalıdır" → "… sonra deneme kapalıdır" | yok |
| T370 kriter 9 | "havale yolu kapalıdır" → "havale yolu kapalı" | yok |
| T386 kriter 6 | "düzeltilmiş bilgi B-12'yle bildirilir" → "B-12'yle bildirilir" | yok |

Riskli T379 (2.555), T380 (2.550) ve hukum_10, hukum_6, hukum_7'nin blokları sınırın altında kaldı. `11_task_blok_denetle.py --tam` sorun 0.

### Üst dokümana geri besleme (K-652) — KD-deep-2a

Üç doğrulayıcı notu üst dokümanda eksik ya da belirsiz bir yer buldu; ikisi metin hizalamasıdır (#29, #125), biri bir ürün kuralının üst dokümanda tanımsız kalmış sınırıdır (#187, ⚠). Aynı PR'da işlendi; yeni kural numarası yoktur (✓ korunur).

| Not | Doküman | Düzeltme | Sürüm |
|---|---|---|---|
| hukum 11 (#29) | `02 §3.18.3` — "taban yine bütün kalemlerin toplamıdır" siparişe girmeyen kalemi de kapsıyor gibi okunur | "siparişe giren bütün kalemlerin toplamıdır" | `02` v0.107 |
| hukum 13 (#187) | `02 §10.4.5` — düzeltilmiş teslim tarihinin kayıtlı iade ulaşma tarihiyle ilişkisi tanımsız (ulaşma tarihinin alt sınırı teslim tarihidir, §7.4.7) | "kayıtlı bir iade ulaşma tarihinden sonraya düşemez (§7.4.7)" | `02` v0.107 |
| hukum 13 (#187) | `03 §8.4.4` — tarih sınırı yalnız kargo günü ve ileri tarih | aynı sınır ve "firma önce ulaşma tarihini düzeltir" | `03` v0.45 |
| hukum 13 (#187) | `04 7.2.8.1` — olay tarihi sınırı | düzeltmede kayıtlı iade ulaşma tarihinden sonra olamaz | `04` v0.48 |
| hukum 13 (#187) | `07 §9.9.27` UC-148 — 422 listesi | "teslim tarihi kayıtlı bir iade ulaşma tarihinden sonraya düşüyor" | `07` v0.36 |
| hukum 18 (#125) | `02 §3.29.5` — "yeterli kontrastı veren tarafı seçerek" ölçüsüz | "WCAG kontrast oranı yüksek olan tarafı seçerek; seçilen taraf her renkte en az 4,5:1 (hedef AA, §3.34.5)" | `02` v0.107 |
| hukum 18 (#125) | `04 2.1.3` — "yazıyı sistem siyah ya da beyaz seçer" | "kontrast oranı yüksek olan taraf olarak" | `04` v0.48 |

**Anahtar terim taraması (K-1471 biçimi).** `02 §3.18.3`: "taban yine", "bütün kalemlerin toplam" — `02 §3.18.3` dışında eşleşme yok. `02 §10.4.5`: "ulaşma tarihinden sonra", "kargoya verildiği günden önce", "ileri tarihli" — eşleşmeler `02` Z-12, S6, §3.20.11 (işaret anındaki giriş sınırı, düzeltmeyi anlatmaz), `03 §8.4.4`, `04 7.2.8.1`, `07 §9.9.5` (UC-126, işaret; ulaşma tarihi kaydı yoktur, kural değişmez) ve `07 §9.9.27`; işaret anı sınırı olanlar elenir, `03`, `04`, `07 §9.9.27` bu PR'da işlendi. `02 §3.29.5`: "kontrast", "siyah ya da beyaz", "siyah mı beyaz" — `02 §3.34.5` (erişilebilirlik tabanı, aynı hedefi taşır), `04 2.1.3` (işlendi), `04` 2.1'in (1) satırı ve `08` e-posta üst bilgisi ("kontrasta göre seçer") çelişmez; elenir.

**İlerleme:** KD-deep'in 143 doğrulanmamış bulgusundan **65'i bu adımda hükme bağlandı** (KD-deep-2: **65/143**, kalan **78** → KD-deep-2b). Yeniden doğrulamaya bekleyen: **#117** (2b'nin yükselenleri eklenir). Kapılar: `11_matris_kapsama.py` 1.501/1.501 sorun 0 · `11_task_blok_denetle.py --tam` 0 · `11_yazim_kapanis.py` 0 · `11_geri_yon.py . --denetle` 214 = 214 · `geri_isaret_tara.py 1473 1474` eksik 0.

### Doğrulanmamış bulgular — KD-deep-2b

Kalan 78 bulgu 20 doğrulayıcı grubuyla doğrulandı (plan: `scripts/doc-scans/11_dr_grupla2.py`, plan 2b adımı; adım başına sınır 20). **2b'de 20 doğrulayıcı koştu**, hepsi `[dogrulama]` Opus / high; bekçi olayı yok (sayaç 20/20; 2a ile birlikte 39 doğrulayıcı, plandaki grup sayısına eşit). Doğrulamasız uygulama, örnekleme ya da atlama yoktur.

| Doğrulayıcı | Model | Kademe, modül | Bulgular | Hüküm |
|---|---|---|---|---|
| D1 | Opus / high | Orta 4'li, Ortak altyapı | #67, #88, #15, #44 | 0 doğru · 2 kısmen · 2 çürük |
| D2 | Opus / high | Orta 4'li, Ortak altyapı | #87, #174, #13, #55 | 1 doğru · 0 kısmen · 3 çürük |
| D3 | Opus / high | Orta 4'li, Ortak altyapı | #90, #20, #89, #16 | 0 doğru · 4 kısmen · 0 çürük |
| D4 | Opus / high | Düşük 5'li, Ortak altyapı | #184, #68, #81, #21, #181 | 1 doğru · 2 kısmen · 2 çürük |
| D5 | Opus / high | Orta 4'li, Ortak altyapı | #14, #46, #76, #100 | 0 doğru · 2 kısmen · 2 çürük |
| D6 | Opus / high | Orta 4'li, Ortak altyapı | #18, #104, #99, #102 | 0 doğru · 3 kısmen · 1 çürük |
| D7 | Opus / high | Orta 4'li, Ortak altyapı | #25, #109, #114, #58 | 0 doğru · 4 kısmen · 0 çürük |
| D8 | Opus / high | Orta 4'li, Ortak altyapı | #54, #64, #97, #101 | 0 doğru · 4 kısmen · 0 çürük |
| D9 | Opus / high | Orta 4'li, Ortak altyapı | #63, #72, #73, #105 | 0 doğru · 3 kısmen · 1 çürük |
| D10 | Opus / high | Orta 4'li, Ortak altyapı | #77, #180, #182, #75 | 0 doğru · 3 kısmen · 1 çürük |
| D11 | Opus / high | Orta 4'li, Ortak altyapı | #96, #175, #50, #17 | 0 doğru · 1 kısmen · 3 çürük |
| D12 | Opus / high | Orta 4'li, Ortak altyapı | #98, #74, #47, #103 | 2 doğru · 1 kısmen · 1 çürük |
| D13 | Opus / high | Düşük 5'li, Ortak altyapı | #26, #69, #137, #167, #70 | 1 doğru · 1 kısmen · 3 çürük |
| D14 | Opus / high | Orta 4'li, Ortak altyapı | #79, #28, #132, #152 | 1 doğru · 1 kısmen · 2 çürük |
| D15 | Opus / high | Orta 4'li, Ortak altyapı | #165, #171, #172, #39 | 0 doğru · 2 kısmen · 2 çürük |
| D16 | Opus / high | Orta 4'li, Ortak altyapı | #166, #162, #163, #173 | 0 doğru · 4 kısmen · 0 çürük |
| D17 | Opus / high | Orta 4'li, Ortak altyapı | #169, #170, #40, #168 | 2 doğru · 2 kısmen · 0 çürük |
| D18 | Opus / high | Orta 4'li, Yönetim ve işlem izi | #53, #51, #185, #186 | 2 doğru · 0 kısmen · 2 çürük |
| D19 | Opus / high | Orta 3'li, Yönetim ve işlem izi | #59, #135, #159 | 0 doğru · 2 kısmen · 1 çürük |
| D20 | Opus / high | Orta 1'li, karma F6 | #148 | 0 doğru · 0 kısmen · 1 çürük |

**Hükümler (78 bulgu):** 10 doğru · 41 kısmen · 27 çürük. Şartname çifti 98 ham, birleştirme sonrası 97; **yükselen #167** (T546 k5 ve KK5 vitrin geçişlerinin sayısı, Düşük → Orta; 2 çift) bu adımda **uygulanmadı** (`kd-deep-2/yeniden.md`; `KD-deep-2r`'de #117 ile birlikte yeniden doğrulanır). Uygulanan: **50 bulgu, 95 şartname çifti** `11_audit_uygula.py` ile tek seferde (97 çift − #167'nin 2 çifti; sorun 0). Birleştirme: #58 (hukum 7) ve #79 (hukum 14) aynı T377 KK9 satırını değiştirdi; #58'in şartnamesi #79'unkine katıldı (birleşik KK9: "işaretsiz, uydurma ya da başka hesaba işaretli sayılır; işaretli ayrı"); #58 bu yüzden "0 çift"tir. **Seviyesi düşen 35 bulgu** (Orta → Düşük) uygulandı: #174, #13, #90, #20, #16, #14, #46, #76, #100, #104, #25, #109, #58, #54, #64, #97, #101, #77, #180, #182, #75, #96, #47, #28, #132, #165, #172, #39, #166, #169, #168, #59, #135, #159, #148. 27 çürük hükümde şartname yoktur (çoğu "zaten karşılandı: 11:<n>"); "0 çift" görünen kısmen hükümler kendi satırını başka bulgunun şartnamesinde karşılar (#67 → #88; #64, #97 → #54; #101 → #54; #180 → #77; #105 → #63) ya da güncel blok karşılıyordur. Elle satır yok; yalnız hukum 7 #58'in NOT satırı (birleştirme) okundu.

### Bulgu → hüküm → uygulama (KD-deep-2b)

Numaralar `kd-deep/bulgular.md` sıra numarasıdır; "Task" ana task'tır; "↓" mercekten düşüşü, "↑" yükselişi gösterir.

| # | Task | Bulgu (özet) | Hüküm | Seviye | Uygulandı |
|---|---|---|---|---|---|
| 67 | T07 | Yedeğin başlama saati ve azami süre tanımsız, "önce biter" ölçülemez;  | kısmen | Orta | evet (0 çift) |
| 88 | T07 | Kriter 5 ve 7 (gerçek nesne depolama yetkisi, kova yaşam döngüsü, kurt | kısmen | Orta | evet (3 çift) |
| 15 | T10 | F0'da göç yok; göç geçmişi tablosu T10 sınanırken henüz oluşmamış olab | çürük | Orta | hayır |
| 44 | T10 | T10 F0'dadır, göç geçmişi tablosu ilk göçle (F1) doğar; T10'un testi o | çürük | Orta | hayır |
| 87 | T05 | Kriter beş hizmet (dört + göç) tanımlar, KK dört sayar; hangi sayının  | doğru | Orta | evet (3 çift) |
| 174 | T05 | Yedek aracı veritabanına ulaşamıyorsa yedeği hangi yoldan aldığı tanım | çürük | Düşük ↓ | hayır |
| 13 | T06 | T06'nın kabul ve sınaması (Paketleme canlılık denemesi) T01'in ucuna d | çürük | Düşük ↓ | hayır |
| 55 | T06 | T06'nın kabul kriteri T01'in canlılık ucuna ihtiyaç duyar ama T01 bağı | çürük | Orta | hayır |
| 90 | T02 | "Bilgim tarihli" tanımsız; hangi ayarların bu sınıfa girdiği belirsiz  | kısmen | Düşük ↓ | evet (2 çift) |
| 20 | T04 | Bu maddeler iş akışı/imaj/kurulum davranışıdır; Unit/Integration (xUni | kısmen | Düşük ↓ | evet (2 çift) |
| 89 | T04 | "Bilinen açık taraması" için araç, eşik (hangi önem düzeyi düşürür) yo | kısmen | Orta | evet (2 çift) |
| 16 | T05 | Sınama tanımı yalnız Paketleme ve çıkış denemesini sayıyor; HSTS değer | kısmen | Düşük ↓ | evet (2 çift) |
| 184 | T05 | Ağ ayrımı, DOCKER-USER kuralı ve istemci adresi eşitliği kabul kriteri | kısmen | Düşük | evet (1 çift) |
| 68 | T212 | 413'ün sahibi T215 mi T203 mü belirsiz (genel tavan ile makine ucu tav | çürük | Düşük | hayır |
| 81 | T212 | Bildirim ucu için izinli durum kodları listesi 413 ve 429'u içermiyor, | doğru | Düşük | evet (1 çift) |
| 21 | T221 | Tam iki dakika sınırı üç yerde farklı sözle ("aşan", "eskiyken", "kısa | çürük | Düşük | hayır |
| 181 | T227 | 23:59:00 ile 23:59:59 arasındaki saniyelerin süreye dahil olup olmadığ | kısmen | Düşük | evet (2 çift) |
| 14 | T209 | T224 ödeme sağlayıcısı çerçevesini (dış betik/gömme) istisna sayıyor,  | çürük | Düşük ↓ | hayır |
| 46 | T215 | Canlılık/durum ucu IP başına sayfa sınırına tabi; sık yoklayan izleme  | kısmen | Düşük ↓ | evet (1 çift) |
| 76 | T215 | İstemci IP'sinin nereden okunduğu (ters vekil/yönlendirici arkasında g | kısmen | Düşük ↓ | evet (2 çift) |
| 100 | T216 | Dört harfin birbirine eşit sayılması invariant kültür karşılaştırmasın | çürük | Düşük ↓ | hayır |
| 18 | T205 | Söz kaybı: veritabanı denetimi başarısız olunca uygulamanın açılıp açı | çürük | Orta | hayır |
| 104 | T205 | İki task da eksik adını "yazar/kaydeder" ama kayıt altyapısı (T225/T23 | kısmen | Düşük ↓ | evet (2 çift) |
| 99 | T206 | Bu KK'lar sonraki task'lardaki sayfa/uç/özellikler olmadan sınanamaz;  | kısmen | Orta | evet (5 çift) |
| 102 | T208 | T208'de `Original` değeri tutmazsa ne olacağı (409, yazma yok) hiçbir  | kısmen | Orta | evet (2 çift) |
| 25 | T233 | Süre SET ile değiştirilmez denirken SET'in DatabaseConnection'da serbe | kısmen | Düşük ↓ | evet (1 çift) |
| 109 | T324 | Sorgu yineleme aralığı ve üst sınırı yok; sağlayıcı sonsuza dek yanıts | kısmen | Düşük ↓ | evet (2 çift) |
| 114 | T326 | Yeniden deneme mekanizması T385'e bırakılıyor ama T385 bağımlılıkta yo | kısmen | Orta | evet (3 çift) |
| 58 | T377 | İşaret L-1 e-posta eksenini devre dışı bırakıyor (k9) ama bütünlüğü (i | kısmen | Düşük ↓ | evet (#79'a birleştirildi) |
| 54 | T221 | "İş çalışırken kilit tut" ile "dış çağrı kilit içinde beklenmez" çeliş | kısmen | Düşük ↓ | evet (4 çift) |
| 64 | T221 | "Çalışırken kilit tutar" ifadesi hangi kilit olduğunu söylemiyor; satı | kısmen | Düşük ↓ | evet (0 çift) |
| 97 | T221 | Her iş türü kilit tutarken ödeme sağlayıcısına yapılan son sorgu işi d | kısmen | Düşük ↓ | evet (0 çift) |
| 101 | T221 | Kriter 7 çok parçalı; "gecelik iş bir kez çalışır" ve "iptal ertelenir | kısmen | Düşük ↓ | evet (0 çift) |
| 63 | T220 | Kuyruk en az bir kere teslim eder; iade çağrısı sağlayıcıya gitti ama  | kısmen | Orta | evet (2 çift) |
| 72 | T220 | Satır alındıktan sonra süreç çökerse ya da yeniden başlarsa "alınmış"  | kısmen | Orta | evet (3 çift) |
| 73 | T220 | Tek döngü, tek seferde bir satır ve 30 sn'ye varan dış çağrı süresi bi | çürük | Orta | hayır |
| 105 | T220 | Çalıştırıcının "gönderilmiş" durumunu nasıl bildiği tanımsız; iade iş  | kısmen | Orta | evet (0 çift) |
| 77 | T227 | İki en büyük parça/kalem eşit olduğunda hangisine yazılacağı belirsiz; | kısmen | Düşük ↓ | evet (1 çift) |
| 180 | T227 | Eşit büyüklükte iki parça olduğunda kalan kuruşun hangisine gideceği t | kısmen | Düşük ↓ | evet (0 çift) |
| 182 | T227 | Takvim kapsamı bitince resmi tatiller sessizce yok sayılır; havale son | çürük | Düşük ↓ | hayır |
| 75 | T229 | Bellek önbelleği ve süreç içi zamanlayıcılar tek uygulama örneği varsa | kısmen | Düşük ↓ | evet (1 çift) |
| 96 | T222 | Saf rastgele üretilen anahtar sırra bağlı değildir; "rastgele" ile "sı | kısmen | Düşük ↓ | evet (2 çift) |
| 175 | T224 | 3D Secure dönüşü sağlayıcıdan çoğunlukla çapraz site POST ile gelir; S | çürük | Orta | hayır |
| 50 | T225 | Ayar denetimi (T226) sistem kaydına yazar, sistem kayıtları (T225) ise | çürük | Orta | hayır |
| 17 | T227 | Para dağıtımında eşit en büyük parça olduğunda (ör. iki eşit kalem) ha | çürük | Orta | hayır |
| 98 | T201 | `02 §11` bir doküman; kodun onu "okuması" ve "kodda yinelenmemesi" nas | doğru | Orta | evet (4 çift) |
| 74 | T202 | Ödemeyi başlatma çağrısı zaman aşımına uğradığında sonuç sınıfı belli  | doğru | Orta | evet (4 çift) |
| 47 | T203 | İmza gövde üzerinden hesaplanıyorsa tavan imzadan önce uygulanmalıdır; | kısmen | Düşük ↓ | evet (2 çift) |
| 103 | T204 | Alıcı hatasında satırın ne olacağı (hemen "ulaşmadı" mı, hiç denenmez  | çürük | Orta | hayır |
| 26 | T230 | Satıra yol hiç yazılmıyorsa PathMasker'ın işi ve yazılan istek alanlar | çürük | Düşük | hayır |
| 69 | T314 | T314 sayaç ve nabız/yedek/kanal verisini okuyor ama bağımlılığı yok; ö | kısmen | Düşük | evet (2 çift) |
| 137 | T378 | T378 k3, k5 ve k9 T227'ye dayanıyor ama bağımlılık "—"; bağımlılık sır | çürük | Düşük | hayır |
| 167 | T546 | AC dört geçiş sayar, KK üçten söz eder; hangi üçü (ve "Panele dön"ün n | doğru | Orta ↑ | hayır (Orta'ya yükseldi → yeniden doğrulama) |
| 70 | T548 | 409 ile sürdürmenin üst sınırı yok; sunucu sürekli 409 dönerse betik s | çürük | Düşük | hayır |
| 79 | T377 | İşaret çerezinin değeri (rastgele, hesapla bağlı, sunucuda doğrulanan  | kısmen | Orta | evet (3 çift) |
| 28 | T378 | T378 "üç süre" der, T381 k6 altı parametreyi (P-5, P-6, P-7, P-8, P-9, | çürük | Düşük ↓ | hayır |
| 132 | T378 | T378 üç süreyi çitli sayar, T381 ise P-5, P-6, P-7, P-8, P-9, P-44 olm | çürük | Düşük ↓ | hayır |
| 152 | T504 | T504'ün KK2'si, sonradan gelen task'larda kurulan uçlara (sipariş eriş | doğru | Orta | evet (1 çift) |
| 165 | T538 | AC5'in ikinci yarısı (fazladan form alanı yok sayılır, aşırı gönderim/ | kısmen | Düşük ↓ | evet (1 çift) |
| 171 | T539 | AC1 bütün uygulamayı kapsıyor ama bağımlılık yalnız seçili ekranları s | kısmen | Orta | evet (2 çift) |
| 172 | T539 | "Dönem seçimi" AC2'de aynı formun (POST) ara gönderimi, AC3'te "satış  | çürük | Düşük ↓ | hayır |
| 39 | T540 | "Anlam yalnız renkle taşınmaz" ve "altbilgi kısaltılmaz" kuralları T54 | çürük | Düşük ↓ | hayır |
| 166 | T546 | "Tabloda olmayan geçiş koda sızmaz" yönü (kodda fazladan bağlantı/yönl | kısmen | Düşük ↓ | evet (1 çift) |
| 162 | T547 | Test beklentisi yalnız Unit ve denetim DB'siz/ağsızdır; ama sayfa üret | kısmen | Orta | evet (3 çift) |
| 163 | T547 | AC7 `04` form envanteri tablosunun biçimini anlatır; KK7 ise kodda sab | kısmen | Orta | evet (3 çift) |
| 173 | T548 | Üç sürdürme denemesi tükenince kullanıcıya ne gösterilir, ayrıca 401/4 | kısmen | Orta | evet (4 çift) |
| 169 | T540 | Tarayıcı tabanı, %200 yakınlaştırma, odak sırası, marka renginin yalnı | kısmen | Düşük ↓ | evet (3 çift) |
| 170 | T541 | Yönetici onayı "pencere" olarak tanımlı ama JavaScript'siz yolda onay  | doğru | Orta | evet (3 çift) |
| 40 | T544 | "Form içeriği değişince" hangi alanların değişimini kapsadığı belirsiz | doğru | Orta | evet (1 çift) |
| 168 | T544 | Saat dilimi belirtilmemiş ve "23.59'da biter" saniye/milisaniye düzeyi | kısmen | Düşük ↓ | evet (2 çift) |
| 53 | T106 | Uygulama kullanıcısından UPDATE/DELETE geri alınıyor, ama imha fonksiy | çürük | Orta | hayır |
| 51 | T303 | İlk yönetici komutu, T342'de yazılan şifre politikası işlevini kullanı | doğru | Orta | evet (2 çift) |
| 185 | T379 | İzi yazan T375 ve T379, iz altyapısını kuran T380'e bağlı değil; T380  | çürük | Orta | hayır |
| 186 | T384 | CSV aralığının hangi tarihe (oluşma, ödeme onayı, kargo) uyguladığı ya | doğru | Orta | evet (2 çift) |
| 59 | T387 | Webhook yolu `payment_amount` eşitliği ve `total_amount` >= ister; kom | kısmen | Düşük ↓ | evet (1 çift) |
| 135 | T388 | Sayım kayıt tarafında, dönüş koşulu "sipariş tarafında" diye anılıyor; | kısmen | Düşük ↓ | evet (2 çift) |
| 159 | T521 | Sayaç bağlantılarının hedefi olan sipariş süzgeçleri (T527) ve E-38 bu | çürük | Düşük ↓ | hayır |
| 148 | T602 | "Sorunsuz", "acil yama" ve "gece" ölçütleri tanımsız (hangi saat aralı | çürük | Düşük ↓ | hayır |

### KARAR satırları (KD-deep-2b; toplu onay modu, K-1472)

Dört KARAR doğrulayıcının önerisiyle kaydedildi; dördü de şartnamelerde uygulanan seçenektir.

- **#114 (K-1475, `öneriyle kaydedildi`):** T326 k7 ve KK7, Z-26'nın üç yeniden denemesini T385'e işaret eden maddeye çevrildi (b); T326'ya T385 bağımlılığı eklenmedi (T326, T327, T360 sırası değişmez). 7.1 satırı Z-26 → `T326, T385`, 7.2 `11_geri_yon.py . --yaz` ile yeniden türetildi.
- **#170 (K-1475, `öneriyle kaydedildi`):** betiksiz onay hâlinde sayfa onay bölümüyle açılır, odak "Vazgeç"te (`autofocus`) (a). T541 Dokümanlar'a `07 §2.2.4` eklendi.
- **#173 (K-1475, `öneriyle kaydedildi`):** yükleme ağ hatasında üç denemeden sonra durur ve genel hata gösterir; listelenmeyen yanıt (500) 400 ve 413 gibi ele alınır.
- **#59 (K-1476, `toplu onayla kaydedildi — ⚠`):** `--amount` panelin sipariş tutarıdır (taksit farkı hariç) ve `Order.Total`'a eşitlikle karşılaştırılır (A); webhook koşuluyla aynı. Para karşılaştırmasına dokunduğu için ⚠ grubunda sayılır; aşamanın ⚠ listesinde proje sahibine tek listede gösterilir.

### Blok bayt sınırı (2.560) ve söz kayıpları — KD-deep-2b

Uygulama sonrası iki blok sınırı aştı: **T215** (2.566) ve **T377** (2.571). Bölme yok; ikisi de sözcük sadeleştirmesiyle kısaltıldı (kural kaybı yok):

| Blok | Kayıp | Kural kaybı |
|---|---|---|
| T215 kriter 3 | "eksen ters vekilin ilettiği IP" → "eksen vekilin ilettiği IP" | yok (vekil, `09 §15.1` madde 8 atfıyla aynı kavramdır) |
| T215 KK8 | "uçta sayı yazılı değil" → "uçta sayı yok" | yok |
| T377 kriter 3 | "siparişin var olup olmadığına bakmaz" → "siparişin varlığına bakmaz" | yok |
| T377 kriter 4 | "ödeme sayfası hiç açılmamış sipariş" → "ödeme sayfası açılmamış sipariş" | yok |
| T377 kriter 6 | "panel girişine muafiyet yoktur" → "panel girişi muaf değildir" | yok |

Riskli T377 (#79 + birleşik KK9), hukum 8 bloğu (2.554), T548 (#173) ve hukum 3, hukum 16, hukum 17 blokları sınırın altında kaldı. `11_task_blok_denetle.py --tam` sorun 0.

### Üst dokümana geri besleme (K-652) — KD-deep-2b

Üç doğrulayıcı notu üst dokümanda eksik ya da belirsiz bir yer buldu: biri bir komut parametresinin tanımsız kalmış sınırı (#59, ⚠), ikisi davranış hizalamasıdır (#170, #173). Aynı PR'da işlendi; yeni kural numarası yoktur (✓ korunur).

| Not | Doküman | Düzeltme | Sürüm |
|---|---|---|---|
| hukum 19 (#59) | `05 §8.5.12` — "tutar `Order.Total`'a eşitse" taksitli ödemedeki iki tutardan hangisi olduğunu söylemiyor | "tutar (panelin sipariş tutarı, taksit farkı hariç — `08 §2.1.5`'in `payment_amount`'u) `Order.Total`'a eşitse" | `05` v0.42 |
| hukum 19 (#59) | `08 §2.1.2` — mutabakat cümlesi, aynı ifade | aynı tanım | `08` v0.19 |
| hukum 19 (#59) | `09 §13.2` — `--amount` yalnız biçimiyle tanımlı | "panelin sipariş tutarıdır (taksit farkı hariç, `payment_amount` karşılığı) ve `Order.Total`'a eşitlikle karşılaştırılır" | `09` v0.27 |
| hukum 17 (#170) | `04 2.4.4` — odak kuralı yalnız pencereyi anıyor, betiksiz onay hâli (`07 §2.2.4`) tanımsız | "Açıldığında — betiksiz yolda onay hâli gösterildiğinde de — odak Vazgeç'tedir" | `04` v0.49 |
| hukum 16 (#173) | `09 §16.2` madde 6.4 — ağ hatasında üç deneme tükenince betiğin tepkisi yazılı değil | "üçü de düşerse yükleme durur ve genel hata mesajı gösterilir; listelenmeyen yanıt (500) 400 ve 413 gibi ele alınır" | `09` v0.27 |
| hukum 16 (#173) | `09 §16.2` madde 7 — betik testi | "ağ hatasında 3 deneme ve sonra durma" | `09` v0.27 |

**Anahtar terim taraması (K-1471 biçimi).** `05 §8.5.12` / `08 §2.1.2` / `09 §13.2` için terimler: "Order.Total'a eşit", "elle uygula", "apply-payment-result", "--amount", "taksit farkı". Eşleşmeler `08 §2.1.5` (webhook tutar koşusu, "Order.Total'a eşit olmalıdır" — zaten sipariş tutarı olarak tanımlı, değişmedi), `08` K-1283 değişiklik notu (tarihçe, işlenmez) ve `02`/`03`/`07`'de eşleşme yok; üç yer bu PR'da işlendi. `04 2.4.4` için terimler: "odak", "Vazgeç'te", "autofocus", "betiksiz"; eşleşmeler `04 2.1.4` kural (3) (2.4.4'e atıf yapar, çelişmez) ve `07 §2.2.4` (onay hâlinin kendisi, odak kuralı yok — `04` kaynak olduğu için `07` değişmedi); elenir. `09 §16.2` için terimler: "ağ hatası", "3 deneme", "beklemeler 1, 2 ve 4", "yeniden denenmez"; eşleşmeler `09` §3.5 tablosu ve satır 348 (sürdürme denemesi sayısı ve beklemeler, durmayı söylemez, çelişmez) ve `04`/`07`'de yükleme betiği ağ hatası anlatımı yoktur; elenir.

**İlerleme:** KD-deep'in 143 doğrulanmamış bulgusunun **tamamı hükme bağlandı** (KD-deep-2: 2a 65 + 2b 78 = **143/143**). Toplam (KD-deep dahil): hükme bağlanan 48 + 143 = 191 ham bulgu; 2a+2b toplamı **19 doğru · 68 kısmen · 56 çürük**, uygulanan **85 bulgu / 161 şartname çifti** (2a 35/66, 2b 50/95). Yeniden doğrulamaya bekleyen: **#117, #167** (`KD-deep-2r`; ikisi de Düşük → Orta). Kapılar: `11_matris_kapsama.py` sorun 0 · `11_task_blok_denetle.py --tam` 0 · `11_yazim_kapanis.py` 0 · `11_geri_yon.py . --denetle` 214 = 214 · `geri_isaret_tara.py 1475 1476` eksik 0.

### Yeniden doğrulama — KD-deep-2r

2a ve 2b'de Düşük → Orta'ya yükselen iki bulgu (#117 ve #167) yeni kademede yeniden doğrulandı (paket: `scripts/doc-scans/11_deep_yeniden_paketle.py`; Orta için 2–4'lü kural, iki bulgu farklı modülde olduğundan her biri tek). **2r'de 2 doğrulayıcı koştu**, ikisi de `[dogrulama]` Opus / high; bekçi olayı yok. Doğrulamasız uygulama, örnekleme ya da atlama yoktur.

| Doğrulayıcı | Model | Kademe, modül | Bulgular | Hüküm |
|---|---|---|---|---|
| D1 | Opus / high | Orta 1'li, Bildirim | #117 | 1 doğru · 0 kısmen · 0 çürük |
| D2 | Opus / high | Orta 1'li, Ortak altyapı | #167 | 1 doğru · 0 kısmen · 0 çürük |

**Hükümler (2 bulgu):** 2 doğru · 0 kısmen · 0 çürük; ikisi de Orta kaldı, **Yüksek'e yükselen yok.** Uygulanan: **2 bulgu, 8 şartname çifti** `11_audit_uygula.py` ile tek seferde (sorun 0).

| # | Task | Bulgu (özet) | Hüküm | Seviye | Uygulandı |
|---|---|---|---|---|---|
| 117 | T306 (+ T324) | B-3 kesinti kolu koşulsuz "site dönünce gönderilir" der; `03 §4.1.9` ve `§7.1.6` "süre hâlâ işliyorsa gider, dolmuşsa gitmez" der; KK2 silinmiş hesapta anahtarlı bağlantıyı sınamaz | doğru | Orta | evet (6 çift) |
| 167 | T546 | k5 dört tetikleyici sayar, KK5 "üç vitrin geçişi aranır" der; `04 §3.7` beş satırdır (3.7.4 "Düzenlemeye dön", 3.7.5 oturumsuz taslak adresi → E-06) | doğru | Orta | evet (2 çift) |

**#117 (6 çift):** T306 k5 kesinti kolunu T324'e işaret eder (Z-9 zamanlamasının sahibi T324 k7'dir); T324 k7 ve KK7 kesintide kaçan hatırlatmanın yalnız süre hâlâ işliyorsa gittiğini yazar; T306 KK2 silinmiş hesapta anahtarlı bağlantıyı da sınar. T306 k3 ("12 puntodan küçük olmayan" → "en az 12 punto") ve k7 (`08 §2.2.6` B-5; `08 §2.4` → `08 §2.2.6` B-5, §2.4) anlam korunarak kısaltıldı. **#167 (2 çift):** T546 k5 ve KK5 `04 §3.7`'nin beş satırına bağlandı (liste kopyalanmaz, K-1445). Yan yüzler: T327 k3 (genel "kaçırdıkları sırayla çalışır"; B-3 istisnası T324'te kalır), T317 k8 (kesintiyle ilgisiz), T540 k9 (T546 eşitlik denetimiyle kapsanır) ve izlenebilirlik satırı 04 §3.7 → T546 değişmedi.

### KARAR satırları (KD-deep-2r)

KARAR yok; iki bulgu da şartnamesiyle uygulandı. K-1477 tek satırdır.

### Blok bayt sınırı (2.560) ve söz kayıpları — KD-deep-2r

Uygulama sonrası ölçüm: **T306 2.559**, **T324 2.442**, **T546 1.942**; `11_task_blok_denetle.py --tam` sorun 0. Bölme yok. T306'da yer şartnameyle açıldı (kesinti koşulu T324'e taşındı); kural kaybı yok.

| Blok | Kayıp | Kural kaybı |
|---|---|---|
| T306 kriter 3 | "12 puntodan küçük olmayan" → "en az 12 punto" | yok (aynı eşik) |
| T306 kriter 7 | "`08 §2.2.6` B-5; `08 §2.4`" → "`08 §2.2.6` B-5, §2.4" | yok (atıf biçimi) |
| T306 kriter 5 | kesinti kolunun koşulu T324'e işaret eder | yok (koşul T324 k7 ve KK7'de, daha tam) |

### Üst dokümana geri besleme (K-652) — KD-deep-2r

Doğrulayıcı notu üst dokümanda bir koşulun düşmüş olduğunu buldu; aynı PR'da işlendi, yeni kural numarası yoktur (✓ korunur). `04 §3.7` (#167) kaynak olduğundan değişmedi.

| Not | Doküman | Düzeltme | Sürüm |
|---|---|---|---|
| hukum 1 (#117) | `08 §2.2.6` B-3 Tetik — "kesintide zamanı gelmiş hatırlatma site dönünce" koşulsuz; `03 §7.1.6` süre hâlâ işliyorsa der | "… site dönünce, havale süresi hâlâ işliyorsa; dolmuşsa gitmez (`02 §3.21.6`, Z-9; `03 §7.1.6`)" | `08` v0.20 |

**Anahtar terim taraması (K-1471 biçimi).** `08 §2.2.6` B-3 için terimler: "kesintide zamanı gelmiş", "site dönünce", "havale süresi hâlâ işliyorsa", "dolmuşsa gitmez", "kaçırılırsa dönüşte". Eşleşmeler `02` §4.3 (Sitenin kesintisi: "Hatırlatma, havale süresi dönüşte hâlâ işliyorsa gönderilir, dolmuşsa gönderilmez") ve Z-9 satırı (kesintide ertelenen iptalde yeni hatırlatma gitmez), `03` 4.1.9, 7.1.6, 10.3.3 ("süre hâlâ işliyorsa"), `04` `03 §7.1.6` izlenebilirlik satırı, `05` başlık notu, §5.4.2–§5.4.3 ve MK-44 (hepsi koşulu zaten taşıyor; çelişmez). `07` izlenebilirlik tablosundaki `03 §4.1.9` satırı ("kesintide kaçırılırsa dönüşte") koşulu yazmaz ama `03 §4.1.9`'a atıf yapan özet satırıdır ve uç doğurmaz; elenir. Tek yer (`08` B-3 Tetik) bu PR'da işlendi.

**İlerleme:** Deep review'un **tamamı kapandı**. 191 ham bulgunun tamamı hükme bağlandı; yeniden doğrulama (2r) iki bulguyu teyit etti (bu ikisi 2a ve 2b'nin sayılarında zaten vardır, çift sayılmaz).

| Adım | Hükme bağlanan | Doğru · kısmen · çürük | Uygulanan bulgu / çift | `11` |
|---|---|---|---|---|
| KD-deep (ilk koşum) | 48 | 12 · 22 · 14 | 34 / 82 | v0.34 |
| KD-deep-2a | 65 | 9 · 27 · 29 | 35 / 66 | v0.35 |
| KD-deep-2b | 78 | 10 · 41 · 27 | 50 / 95 | v0.36 |
| KD-deep-2r (yeniden) | 2 (aynı #117, #167) | 2 · 0 · 0 | 2 / 8 | v0.37 |
| **Toplam** | **191** | **31 · 90 · 70** | **121 / 251** | |

Kapılar: `11_matris_kapsama.py` sorun 0 · `11_task_blok_denetle.py --tam` 0 · `11_yazim_kapanis.py` 0 · `11_geri_yon.py . --denetle` 214 = 214 · `geri_isaret_tara.py 1477 1477` eksik 0.

### Aksiyon Planı

1. **Bu adım:** 34 bulgu / 82 şartname çifti `11` v0.34'e uygulandı; `02` v0.106, `09` v0.26 geri besleme.
2. **KD-deep-2a (yapıldı):** 65 bulgu hükme bağlandı, 35 bulgu / 66 çift `11` v0.35'e uygulandı; `02` v0.107, `03` v0.45, `04` v0.48, `07` v0.36 geri besleme.
3. **KD-deep-2b (yapıldı):** 78 bulgu hükme bağlandı (143/143), 50 bulgu / 95 çift `11` v0.36'ya uygulandı; `04` v0.49, `05` v0.42, `08` v0.19, `09` v0.27 geri besleme.
4. **KD-deep-2r (yapıldı):** #117 ve #167 yeniden doğrulandı (2 doğru, Orta), 2 bulgu / 8 çift `11` v0.37'ye uygulandı; `08` v0.20 geri besleme. Deep review kapandı.
5. **Sonra:** cross-review (Aşama 9 kalite döngüsünün son adımı) ve aşama kapanışı.

### Sınırlar

- Mercekler Sonnet / low'dur ve bulgu kaçırabilir; altı mercek paketini kısmen okudu (kuyruk mercekleriyle kapsandı). Kapsam, mercek kapsamıyla sınırlıdır; plandaki 214 task'ın her cümlesi gözle okunmadı.
- Doğrulama adım başına 20 Opus doğrulayıcıyla sınırlıydı; ilk koşumda 143 bulgu (%75) doğrulanmadan kaldı, KD-deep-2a, 2b ve 2r'de (kademeli, Opus / high) hepsi hükme bağlandı; doğrulanmamış bulgu kalmadı. Hiçbiri güvenlik ya da veri kaybı Yüksek'i değildir.
- Kapsam ve tutarlılık katmanı audit'te tam koştuğu için bu adımda dardır (Kurallar 7); audit'in kararlarının doğurduğu yan yüzleri AY merceği taradı.
- Söz kayıpları yalnız hükme bağlanan bulgular için kesindir; yer kalmayan #12 ve mercekçe bulgu üretilmeyen kayıplar için "anlam kaybı yok" mercek yargısıdır.
- Rapor biçimi `09_DEEP_REVIEW.md`'den uyarlandı; çürütülen bulguların tek tek gerekçesi hüküm dosyalarındadır (depoda değil).

# Deep Review Raporu — 07 API Design

**Tarih:** 2026-10-19 · **Hedef:** `Docs/07_API_DESIGN.md` v0.16 → v0.17 (bulgular v0.16'da üretildi, v0.17'de işlendi; taban `main` = 6db661e) · **Bağlam:** `02_PRODUCT_REQUIREMENTS.md` v0.91, `03_USER_FLOWS.md` v0.38, `04_UI_SPECS.md` v0.38, `05_TECHNICAL_ARCHITECTURE.md` v0.27, `06_DATA_MODEL.md` v0.29, `10_MVP_SCOPE.md` v0.55 (bulgular bunlar üzerinde üretildi); `PRODUCT_DISCOVERY_STATUS.md` §2 (K-1144…K-1206) ve §16.1; [`07_AUDIT.md`](07_AUDIT.md) · **Odak:** Tam analiz — katman 3, 4, 5, 6, 7, 8 on dört kalite merceğinde; katman 1–2 tek hafif mercekte (audit'in düzeltmelerinin doğurduğu yeni tutarsızlıklar; skill kural 7)

### Koşum biçimi (K-431, K-1145, K-1207; `deep-review` skill'i, "Çıktı formatı")

Dokümanı yazan bağlamda koşmadı. Adımı yürüten yönetici, mercekleri, doğrulayıcıları ve uygulayıcıları ayrı alt ajanlar olarak başlattı. Her mercek bulgusunu bulduğu anda kendi dosyasına yazdı.

- **On beş mercek, üç dalgada.** Dalga 1: **L12** (katman 1 ve 2 — hafif) · **L3a** (§2, §3, §7, §8, §10) · **L3b** (§4.1–§4.16) · **L3c** (§4.17–§4.34) · **L3d** (§5, §6). Dalga 2: **L3e** (§9.1–§9.12) · **L3f** (§9.13–§9.26) · **L4a** (güvenlik, vitrin) · **L4b** (güvenlik, panel) · **L5a** (hata modu, vitrin). Dalga 3: **L5b** (hata modu, panel) · **L6a** (veri akışı, vitrin) · **L6b** (veri akışı, panel) · **L7** (ölçeklenebilirlik) · **L8** (bağımlılık). Katman 4 OWASP Top 10:2025'e göre koştu.
- **Hepsi Sonnet / low (K-1207).** Proje sahibi audit adımından sonra Opus ve Fable'ı kaldırdı. K-1145'in model ve efor maddeleri bu karardan sonra değişti; geri kalanı (yeniden işi azaltma, ucuzdan başlama, metin azaltma, ölçüm) sürer.
- **Audit'le çakışma yasağı.** Her mercek `07_AUDIT.md`'yi okudu ve audit'in bulduğunu yinelemedi. Audit'in düzeltmesinin doğurduğu yeni sorun ayrıca işaretlendi.
- **Katman 1–2 yeniden tam koşmadı (skill kural 7).** Tek hafif mercek (L12) yalnız audit'in düzeltmelerinin doğurduğu yeni tutarsızlıklara baktı.
- **Doğrulama iki aşamalı.** Birinci doğrulayıcı sekiz parti (V1–V8); ikinci doğrulayıcı ayrı, temiz bağlamlı dört parti (O1–O4; 38 bulgu — Sonnet'in doğruladığı yüksek ve orta önemli bulgular ile çürütülenler). İki doğrulayıcı ayrışırsa son hükmü adımı yürüten Sonnet ajanı verdi (K-1207).
- **Uygulama.** Dokümanda altı uygulayıcı sırayla çalıştı: **A1**, **A2a**, **A2b**, **A3**, **A4a**, **A4b**. **A5** matrisi ve geri besleme tablosunu, **A6** karar satırlarını, **A7** sürüm, durum ve hafızayı işledi.
- **Betikle yapılanlar.** Mercek dilimleri; audit'in atıf, uç-kalıbı ve matris betikleri uygulamadan önce ve sonra; karar kolu taraması.
- **Skor (skill).** Katman skoru ✓ / ⚠ / ✗ olarak verilir; katman birden çok mercekten birleşiyorsa herhangi biri ⚠ ise katman ⚠'dir.

**Karşı-doğrulama sonucu:** 97 ham bulgunun 95'i doğrulandı (birinci doğrulayıcı: 47 doğrulandı · 48 kısmen · 2 çürütüldü; yürütücü hükümleriyle iki bulgu çürütülmüş kaldı, bir bulgu çürütülmüşten kısmen doğrulandıya döndü). Uygulama sonucu: **91 uygulandı · 2 kısmen · 2 karşılandı**.

### Özet Skor Tablosu

"Ham" sütunu merceklerin bulduğu bulgu sayısıdır; skor hükümden sonraki durumdur (düzeltmeler bu adımda uygulandığından v0.17'ye geçerken kalan iş yoktur).

| # | Katman | Ham | Skor | Kritik bulgu | Güçlü yönler (özet) |
|---|---|---|---|---|---|
| 1 | Kapsam | 1 | ✓ | Yok. Tek eksik L12-9 (Low): iki matris satırının metin hücresi K-1200'ün yeni cümlesini taşımıyor | Audit'in sekiz kararı ve 86 düzeltmesinin hedef yerleri yerinde; K-1199'un üç yüzü aynı cümleyi söylüyor |
| 2 | Tutarlılık | 10 | ⚠ | Yok (High yok). L12-1 (Low): UC-29 ile §6.2.3 `PaymentNotPending` iki ayrı hedef yazıyor | Sayımlar doğru (§6.2 = 111, §6.3 = 22 kod, 108 → 111 hesabı tutuyor); üst doküman geri beslemeleri 07'nin yeni metniyle uyumlu |
| 3 | Teknik derinlik | 41 | ⚠ | L3a-1 (High): tutar ayrıştırması noktayı binlik ayırıcı sayıp "12.50"yi sessizce 1250 yapabilir; L3e-4 (High): yeniden gönderim isteğin içinde mi kuyrukta mı yazılı değil (L5b-3, L8-1 ile aynı sorun) | Her konvansiyon tablo satırı, alt bölüm ve "Elenenler"le çıkar; sipariş işlemlerinin ortak kuralı (sürüm → 422 → hepsi ya da hiç) açık; ayar bölümleri kendi sürümüyle; dışa aktarmanın sırası yazılı |
| 4 | Güvenlik | 14 | ⚠ | Yok (High yok). 7 Medium: CSV formül öneki (L4b-1 = L3f-9), `no-store` kuralının yokluğu (L4b-2 = L4a-3), misafir e-postasına e-posta, `search` sınırı, indirme kaynak tüketimi | Sahiplik reddi ve var olmayan kayıt aynı 404; `__Host-` önekli çerezler ve CSRF; hesap numaralandırma panelde kapalı; yönetici hesabı yaşam döngüsü eksiksiz |
| 5 | Hata modu | 8 | ⚠ | L5a-1 (High): ikinci ödeme oturumu ve Ödendi siparişe gelen ikinci başarılı ödeme çift tahsilata gidiyor (L6a-8 ile aynı sorun) | Sonucu belirsiz işlemler ve 15/30 sn zaman aşımı kuralı tutarlı; ödeme bildirimi önce kaydedilir sonra işlenir; kart iadesinde kayıt-önce-istek ve firmanın "gerçekleşmedi" çıkışı |
| 6 | Veri akışı | 16 | ⚠ | L6b-1 (High): UC-131 / UC-132 var olmayan `RefundId` bekliyor; L6a-8 (High, L5a-1 ile aynı) | Onay anahtarı akışı uçtan uca kapalı; erişim anahtarının yolculuğu tutarlı; snapshot ↔ referans ayrımı bölüm bölüm açık; dışa aktarmada iz satırının yeri doğru |
| 7 | Ölçeklenebilirlik | 2 | ✓ | Yok (iki Low) | Sayfa numaralı gezinme gerekçeli; durum veritabanında, "büyüdüğünde X'e geçilir" ifadeleri tetikleyicili; hız sınırı sayaçları ikinci örnekte sorun çıkarmaz |
| 8 | Bağımlılık | 5 | ⚠ | L8-1 (High): yeniden gönderme uçlarının zamanlama dili (L3e-4 ile aynı sorun) | Sağlayıcı adı hiçbir adreste, kodda ya da alan adında sızmıyor; sağlayıcıya bağlı kararlar koşullu ve "bilgim tarihli" işaretli; UC-80 tekrar gönderime dayanıklı |

Merceklerin Özet bölümüyle karşılaştırma: L12 (katman 1 ✓, katman 2 ⚠), L3a ⚠, L3b ⚠, L3c ⚠, L3d ⚠, L3e ⚠, L3f ⚠, L4a ⚠, L4b ⚠, L5a ⚠, L5b ✓, L6a ⚠, L6b ⚠, L7 ✓, L8 ⚠. Katman 3 altı mercekten, katman 4 iki mercekten, katman 5 ve 6 ikişer mercekten birleşir; birleşik katmanda herhangi biri ⚠ ise katman ⚠'dir (katman 5: L5a ⚠, L5b ✓ → ⚠). Ayrışan skor yok.

**Genel:** ⚠ İyileştirme gerekli — düzeltmeler bu adımda uygulandı; Critical yok.

### Sayılar

- **Ham:** 97 bulgu (mercek önerisi: High 5 · Medium 52 · Low 40). Katman dağılımı: katman 1: 1 · katman 2: 10 · katman 3: 41 (L3a 6 + L3b 7 + L3c 5 + L3d 7 + L3e 7 + L3f 9) · katman 4: 14 (L4a 7 + L4b 7) · katman 5: 8 (L5a 3 + L5b 5) · katman 6: 16 (L6a 10 + L6b 6) · katman 7: 2 · katman 8: 5.
- **Doğrulanmış:** 95 bulgu — **High 7 · Medium 26 · Low 62**; Critical yok. İki bulgu çürütüldü ve sayılmaz: **L3d-7** ve **L3e-3**.
- **Birinci doğrulayıcı dağılımı:** 47 doğrulandı · 48 kısmen · 2 çürütüldü. **İkinci doğrulayıcı:** 38 bulgu (dört parti).
- **Uygulama:** **91 uygulandı · 2 kısmen** (L12-8, L3f-7) **· 2 karşılandı** (L6a-9, L6b-4).
- **Ayrı sorun sayısı.** Aynı sorun birden çok mercekte bulundu: L3a-2 = L3b-5 (IT-6) · L3e-4 = L5b-3 = L8-1 · L5a-1 = L6a-8 · L4a-3 = L4b-2 · L3f-5 = L6b-4 · L3f-3 = L6b-6 · L3c-1 = L6a-10 · L3f-9 = L4b-1 · L5b-4 = L7-2. Yedi High **dört ayrı sorundur:** L3a-1 (tutar ayrıştırma) · L3e-4 + L5b-3 + L8-1 (e-postayı yeniden gönderme uçları istek içinde mi, kuyrukta mı) · L5a-1 + L6a-8 (ikinci ödeme oturumu ve Ödendi siparişe gelen ikinci ödeme) · L6b-1 (UC-131 / UC-132 var olmayan `RefundId` bekliyor).
- **Yürütücü hükmüyle değişen seviyeler.** L3a-1: birinci doğrulayıcı Medium, ikinci doğrulayıcı High, **High kaldı**. L12-2: birinci doğrulayıcı çürüttü, ikinci doğrulayıcı kısmen doğruladı, **Low**. L6a-3: Medium → **Low**. L3f-1: mercek High → **Medium**.
- **Kararlar:** K-1207…K-1217 (11 karar; K-1207 proje sahibinin kararı, on karar öneriyle; üçü ⚠).

### Katman Detayları

Tablolarda "Seviye" sütunu `mercek → doğrulanmış` biçimindedir (aynıysa tek değer; yürütücü hükmüyle değişenler **kalın**). "Öneri / sonuç" sütunu uygulanan düzeltmeyi, kararı (K) ya da park edilen dokümanı ve sonucu (uygulandı / kısmen / karşılandı / çürütüldü) verir. Düzeltmenin tam metni karar kaydında ve `07`'nin ilgili Kaynak satırındadır. Aynı sorunun çiftleri ayrı katmanlarda ayrı satırdır; eşi "=" ile gösterilir.

#### Katman 1 — Kapsam

Merceği L12 (hafif). Audit'in sekiz kararı ve düzeltmeleri okundu.

| # | Seviye | Bulgu | Kaynak | Öneri / sonuç |
|---|---|---|---|---|
| L12-9 | Low | `04` 5.12.15 ve 6.2.12.6 matris hücreleri K-1200'ün yeni cümlesini taşımıyor (sağlayıcıya bağlanılamayan dönüş) | matris `04` 5.12.15, 6.2.12.6; K-1200 | Matris metin hücreleri hizalandı (A5). Uygulandı |

**Güçlü yönler:** (1) Audit'in sekiz kararı ve 86 düzeltmesinin hedef yerleri (UC-27/28/29/19, UC-40/43, UC-83, UC-110/111, UC-134, §2.4–§2.5, §5, §6) yerinde. (2) K-1199'un üç yüzü (§2.5.1, UC-27, UC-28/UC-29) `05 §4.5.1`, `06 §4.17` ve başlık notuyla aynı cümleyi söylüyor. (3) K-1203 ve K-1204'ün `05`/`02` geri beslemeleri `07` cümleleriyle sözcüğü sözcüğüne örtüşüyor.

#### Katman 2 — Tutarlılık

Merceği L12. Bulgular audit'in düzeltmelerinin doğurduğu komşu kural çatışmalarıdır.

| # | Seviye | Bulgu | Kaynak | Öneri / sonuç |
|---|---|---|---|---|
| L12-1 | Medium → Low | §6.2.3 `PaymentNotPending` tek hedef yazıyor, UC-29 iki ayrı hedef yazıyor (kart değil / artık ödeme beklemiyor) | 07 §6.2.3, UC-29 hata; K-1199 (2) | `PaymentNotPending` "Nerede döner" hizalandı. Uygulandı |
| L12-2 | Low → **Low** (birinci doğrulayıcı çürütmüştü) | Aynı onay anahtarıyla ikinci gönderimin 303 → teyit hâlinin §6'da kodu yok | 07 §6.2.3, UC-27 hata; K-1198 (1), K-1199 | Hata değil başarı yolunun yinelenmesidir; kod açılmadı, §6.4.2 sonuna "kod taşımaz" cümlesi yazıldı. Uygulandı |
| L12-3 | Low | §6.3.3 `QuantityExceedsOpen` "gösterilenden farklı" kolunu da sayıyor, §6.2.7 satırı sayamıyor; UC-45 bu kola girmez | 07 §6.2.7, §6.3.3; K-1202 | §6.3 satırı UC-40 ve UC-43 koluyla sınırlandı. Uygulandı |
| L12-4 | Low | Başlık notu "adresi ve yöntemi değişen uç yok" diyor; UC-133'ün işleyicisi `ProcessPartialRefund` → `ProcessAmountBasedRefund` oldu | 07 başlık notu (audit bloğu); UC-133 | Notun son cümlesi düzeltildi. Uygulandı |
| L12-5 | Low | Sahte özetli doğrulama "iki şemada aynı rejim" (K-1204) diye ilan edilmiş, `07`'de yalnız UC-110 yazıyor | 07 UC-55, UC-57; K-1204 (1); 05 §6.1.4 | UC-55 hata ve UC-57 yan etkiler yazıldı; `05 §6.1.4` gerekçesi iki şemaya genişletildi; `12` park. Uygulandı |
| L12-6 | Low | Gösterilen açık adedin karşılaştırması hangi gönderimde yapılır ve gizli alan hangi değeri taşır belirsiz | 07 §2.4.3, UC-40, UC-43, §8.2.2; K-1202 | **K-1212 (1).** Uygulandı |
| L12-7 | Low | UC-83 Idempotency "ikinci parça 409 alır" diyor; K-1203'ün çökme çıkışı tek istisna ama anılmıyor | 07 UC-83; K-1203 | Idempotency'ye istisna parantezi yazıldı. Uygulandı |
| L12-8 | Low | §2.5.1'in üç yarışı bağladığı uçlarda (UC-175, §9.5.2) yarışın sonucu yazılı değil; silinmiş kategori seçiminin yanıtı belirsiz | 07 §2.5.1 → §9.20.2, §9.6.5, §9.5.2; K-1206 | UC-175 Idempotency yazıldı; silinmiş kategori için "422 ya da beklenmeyen hata yolu, `12`'de sabitlenir" diye yazıldı. **Kısmen** (kesin kod verilmedi) |
| L12-10 | Low | Konvansiyon 3'ün örneği `CreateCategory` düzenleme eşi `Update…` olmayan bir örnek; `Rename`/`Move`/`Change` seçimi yazılı değil | 07 yazım konvansiyonu 3; K-1205 | Örnek listesinden çıkarıldı. Uygulandı |
| L12-11 | Low | §2.5.8 kalan riskin "oturumsuz açık formlar" listesi giriş formlarını (UC-55, UC-110) saymıyor | 07 §2.5.8; K-1206 (2) | Listeye "giriş" eklendi. Uygulandı |

**Güçlü yönler:** (1) Sayımlar doğru: §6.2 = 111 satır, §6.3 = 22 kod, §8.2.2 on dört satır, matris 5.071 çift. (2) K-1202'nin gizli alan kalıbı (`OriginalOpenQuantity`) §2.4, UC-40/43, §5.2.1 ve §8.2.2'de aynı terimle işlenmiş. (3) Üst doküman geri beslemeleri (`02` v0.91, `04` v0.38, `05` v0.27, `06` v0.29) `07`'nin yeni metniyle uyumlu.

#### Katman 3 — Teknik derinlik

Altı mercek: L3a (§2, §3, §7, §8, §10), L3b (§4.1–§4.16), L3c (§4.17–§4.34), L3d (§5, §6), L3e (§9.1–§9.12), L3f (§9.13–§9.26). Katman skoru ⚠ (herhangi biri ⚠).

| # | Seviye | Bulgu | Kaynak | Öneri / sonuç |
|---|---|---|---|---|
| L3a-1 | High → **High** | Tutar alanları "Türkçe kültürle ayrıştırılır"; nokta ret mi kabul mü yazılı değil, "12.50" sessizce 1250 olur (100 kat sessiz hata) | 07 §2.9.4; `04` 8.2.3; K-1174 (2) | Kabul edilen biçim rakamlar ve en çok bir virgül; nokta, boşluk, "TL", işaret taşıyan değer ayrıştırılamayan sayılır (form: 422; sorgu: süzgeç uygulanmaz). `09` park (K-1174 satırı). Uygulandı |
| L3a-2 | Medium | `returnUrl` kuralı sipariş geçmişinden sipariş sayfasına dönüşü kapsıyor mu belirsiz (audit'in devrettiği IT-6; = L3b-5) | 07 §2.6.8, UC-33, UC-74; `04` 5.27.7, 2.13.3 | **K-1208** (seçenek b: `returnUrl` yalnız panelde). Uygulandı |
| L3a-3 | Low | Bilinmeyen ya da eksik `handler` değerinin durum kodu yazılı değil | 07 §2.2.1, §2.7.1 | §2.7.1'e yazıldı; `12` park (K-1163 satırı). Uygulandı |
| L3a-4 | Low | Oturumu kapanmış POST'un girişe yönlendirmesinin durum kodu yazılı değil | 07 §2.3.2, §2.7.1 | İki yerde yazıldı. Uygulandı |
| L3a-5 | Medium → Low | `search` sözcüğünün üst sınırı ve aşımda dönecek durum yazılı değil | 07 §2.6.10, §2.8.1; `04` 7.1.1.3 | §2.8.1'e yazıldı; DEPLOY_RUNBOOK park (K-1162 satırı). Uygulandı |
| L3a-6 | Low | `page` için int sınırını aşan sayı "sayı" mı "sayı olmayan" mı belirsiz | 07 §2.6.2 | Taşma cümlesi yazıldı (L3d-3 ile birlikte). Uygulandı |
| L3b-1 | Medium → Low | Ödeme adımının GET'i ve UC-23/24/25 boş sepetle ne döner yazılı değil (yalnız UC-27 yazıyor) | 07 §4.13.1–§4.13.5 | §4.13 girişine "303 → `/sepet`" yazıldı. Uygulandı |
| L3b-2 | Medium | UC-28'in "artık ödeme beklemiyor" yönlendirmesi yalnız kart siparişi için yazılı; iptal edilmiş havale siparişinde eski teyit adresi IBAN gösteriyor | 07 §4.14.1, §4.14.2; `04` 5.14.10 | **K-1209 (1).** Uygulandı |
| L3b-3 | Medium | UC-20: ürün arada yayından kalkarsa adet artırma/azaltma sonucu yazılı değil | 07 §4.12.2 | **K-1209 (2).** Uygulandı |
| L3b-4 | Medium → Low | UC-27 hata maddesi 422/429/303/404 hâllerini sıra vermeden sayıyor | 07 §4.13.6; `05 §4.2.2` | Hata maddesi başına sıra cümlesi yazıldı. Uygulandı |
| L3b-5 | Medium | = L3a-2 (IT-6, UC-33/UC-34 İstek) | 07 §4.16.1, §2.6.8; `04` 5.27.7 | **K-1208.** Uygulandı |
| L3b-6 | Low | Çerezi olmayan misafirin ilk eklemesi 422 alırsa sepetin ve çerezin açılıp açılmayacağı yazılı değil | 07 §4.4.2 UC-05 | Yan etkilere yazıldı. Uygulandı |
| L3b-7 | Low | Tuzak alanlı gönderimin L-4 sayacına sayılıp sayılmadığı yazılı değil | 07 §4.10.2 UC-15 | "L-4'e sayılmaz" yazıldı. Uygulandı |
| L3c-1 | Medium | UC-76 geri alma: eski adres o arada başka hesabın doğrulanmış adresi olduysa sonuç yazılı değil (= L6a-10) | 07 §4.28.2 UC-76, §4.21.2 UC-53; `02 §3.13.14`; `06 §4.42` | **K-1210 ⚠.** Uygulandı |
| L3c-2 | Medium → Low | `Items` içinde aynı `OrderItemId`'nin birden çok gelmesi yazılı değil | 07 §4.17.3 UC-40, §4.18.2 UC-43, §5.1.5 | §5.1.5'e tek yerde yazıldı. Uygulandı |
| L3c-3 | Medium | UC-83: `FileName`, `SizeBytes` ≤ 0, `Content-Range` bozuk hâllerinin yanıtı yazılı değil (`06 §4.70` kısıtı 500'e düşer) | 07 §4.31.2 UC-83; `06 §4.70` | 400 maddesi ve denetim sırası yazıldı; kod açılmadı. Uygulandı |
| L3c-4 | Low | UC-80: hız sınırı ile imza doğrulama sırası ve 413 belirsiz | 07 §4.30.1 UC-80 | 429 etiketi düzeltildi. Uygulandı |
| L3c-5 | Low | UC-37 aralık (`Range`) istekleri sırasında dosya değişirse davranış yazılı değil | 07 §4.31.1 UC-37 | Çatıya bırakılır, kalan risk olarak yazıldı. Uygulandı |
| L3d-1 | Medium | KDV oranı hem `InvalidPercentage` hem `ValueOutOfBounds` kapsamında | 07 §6.2.5, §6.2.6, §6.2.8; `04` 7.3.2.9 | `InvalidPercentage` aralığı eklendi, `ValueOutOfBounds`'tan KDV oranı çıkarıldı. Uygulandı |
| L3d-2 | Medium | GET listelerinin alan mesajı hâlleri (ters fiyat süzgeci, ileri tarih, ters tarih aralığı) kodsuz | 07 §6.2.2, §2.6.3, §2.6.6 | `InvalidDate` genişledi, yeni kod `InvalidPriceRange` (§6.2 = 112). Uygulandı |
| L3d-3 | Medium → Low | Sorguda ayrıştırılamayan tutar iki yerde iki hüküm alıyor (§2.6.6 ↔ §2.9.4) | 07 §2.6.6, §2.9.4, §6.2.2 | §2.9.4, `InvalidAmount` ve `InvalidDate` Anlamı hizalandı. Uygulandı |
| L3d-4 | Medium → Low | Boş değer ile yok değer; kırpma kuralı yazılı değil | 07 §5.1.3–§5.1.4, §2.9 | **K-1214 (1).** Yeni §2.9.10. Uygulandı |
| L3d-5 | Medium → Low | İç içe liste alanlarının istek gövdesindeki biçimi ve satır sayısı tavanı yazılı değil | 07 §5.1.2, §5.2.1, §5.2.2 | §5.1.5'e satır sayısı sınırı; `09` park (dizinli liste bağlama, boş satır atlama). Uygulandı |
| L3d-6 | Low | `UploadSizeExceeded` ile `PayloadTooLarge` ayrımı ve kaynak satırı yazılı değil | 07 §6.2.9, §6.1.2 | Anlamı hücresi düzeltildi. Uygulandı |
| L3d-7 | Low → **yok** | `SingletonContentExists` iki durumu tek kodda topluyor | 07 §6.2.6, §6.1.1 | İki doğrulayıcı çürüttü: iki durumun mesajı ortak değil, §6.1.1'in yasağı tetiklenmiyor. Çürütüldü |
| L3e-1 | Medium → Low | UC-114 üç ayrı yazımın (ürün, varyant, görsel/dosya bağı) hepsi-ya-da-hiç olup olmadığı ve 409 / 422 sırası yazılı değil | 07 §9.5.2, §2.4.3, §2.4.5; `05 §7.3` | UC-114 hata maddesine tek işlem ve ret sırası; §2.4.8'e UC-122 için 409 önce. Uygulandı |
| L3e-2 | Medium | UC-115 varyant dalı: varyantın kendi yayına alma koşulu hata listesinde yok | 07 §9.5.3 UC-115 | 422 maddesi ve Idempotency; §6.2 `PublishGateIncomplete` "Nerede döner" güncellendi. Uygulandı |
| L3e-3 | Medium → **çürütüldü** | Kategori silmenin "boş mu" denetimi ağaç kilidini almıyor | 07 §9.6.5 UC-121; `05 §7.3.5` | **Yürütücü çürüttü:** K-1206'nın elediği seçenek, K-1089'un açık seçimi; yeni olgu yok, bilinçli karara itiraz edilmez. Çürütüldü |
| L3e-4 | Medium → High | Yeniden gönderim isteğin içinde mi kuyrukta mı yapılıyor yazılı değil; 303'te sonuç var gibi okunuyor (= L5b-3, L8-1) | 07 §9.9.32 UC-153, §9.9.33 UC-154; `05 §4.4.3`, §5.1.4; `04` 2.5.4 | UC-153/154 yeniden yazıldı: gönderim kuyruktadır, sonuç sayfanın sonraki gösteriminde okunur; `04` 2.5.4 ve 9.9.33 açıklık cümlesi. Uygulandı |
| L3e-5 | Low | "En yeni önce" listelerde eşit zamanlı kayıtlar için ikincil anahtar yazılı değil (= L7-1) | 07 §9.4.1, §9.8.1, §9.10.1, §9.7.1, §2.6.5 | §2.6.5'e yön cümlesi. Uygulandı |
| L3e-6 | Low | UC-122 "asgari tutar kupon tutarından büyük değil" ifadesi iki okunur; sabit tutarlıda asgari tutar zorunluluğu 422 listesinde yok | 07 §9.7.2 UC-122; `03 §3.5.1.8`; `04` 9.7.4 | 422 maddesi düzeltildi. Uygulandı |
| L3e-7 | Low | UC-157 "aramanın adresi"ni nereden kuracağı belirsiz (istek arama değerini taşımıyor) | 07 §9.12.2 UC-157; `04` 2.7.3.3 | İstek/hata yazıldı; §5.2 `DeleteCustomerInput` satırına `SearchEmail` eklendi. Uygulandı |
| L3f-1 | High → **Medium** | Satış özetinin hazır dönemlerinin (Son 7/30 gün, Bu ay, Ölçüm penceresi) başlangıç ve bitiş günü yazılı değil | 07 §9.22.1 UC-105, §5.3.2; `04` 9.22.3; `02 §10.6.2` | **K-1215 (1).** Uygulandı |
| L3f-2 | Medium → Low | İptal ve iade sayısının tarih esası (sipariş kohortu mu, olay tarihi mi) yazılı değil | 07 §9.22.1 UC-105; `02 §10.6.2`–§10.6.3 | UC-105'e yazıldı: sayılar olay tarihinde, oran dönem kohortu (K-580 Kaynak). Uygulandı |
| L3f-3 | Medium | Beş ayar işleyicisi "üretilen iki metnin yeni sürümü" diyor, koşulsuz (= L6b-6) | 07 §9.16.2 UC-164, §9.18.2–§9.18.6; `02 §3.33.3`; `04` 9.18.9 | **K-1215 (2).** Uygulandı |
| L3f-4 | Medium → Low | Yayındaki sürümle birebir aynı metin "Yayına al" yapılırsa ne olur yazılı değil | 07 §9.19.3 UC-174 | "Her yayına alma yeni sürümdür" yazıldı (bilinçli seçenek). Uygulandı |
| L3f-5 | Medium | Dışa aktarma kapsamı ve satır tanesi yazılı değil (= L6b-4) | 07 §9.24.2 UC-182; `02 §10.7.1` | **K-1213 ⚠** (seçenek A elendi). Uygulandı |
| L3f-6 | Medium | Yeni kaydın `SortOrder`'ı nerede doğar; eşit değerde yukarı/aşağı nasıl çalışır | 07 §9.13.2 UC-158, §9.13.5 UC-161; `06 §4.47` | **K-1215 (3).** Uygulandı |
| L3f-7 | Medium → Low | Tutar üst sınırı, kargoya verme süresi alt sınırı, indirme hakkı üst sınırı yazılı değil | 07 §9.18.2, §9.18.4, §9.18.5; `04` 7.3.2.8; `06` | **K-1214 (2).** Tutar üst sınırı (para türü) ve alt çit yazıldı; P-8 üst çiti açılmadı. Kısmen |
| L3f-8 | Low | UC-177: kaldırılmış işlemcinin oturumu silindiğinden 422 sayfası kime gösterilir belirsiz | 07 §9.20.4 UC-177 | Tek sıra yazıldı (önce işlemcinin yönetici olması → giriş, sonra son yönetici 422). Uygulandı |
| L3f-9 | Low → Medium | CSV hücresinde `=`, `+`, `-`, `@` ile başlayan müşteri girdisi formül yürütür; kuralı kimse taşımıyor (= L4b-1) | 07 §9.24.2 UC-182; K-1195 (6) | **K-1217 (3).** `09` park, `12` senaryo cümlesi. Uygulandı |

**Güçlü yönler:** (1) §2'nin her konvansiyonu tablo satırı, alt bölüm ve "Elenenler"le çıkar; kenar durumlar (iki sekme, geri tuşu, çift tıklama, JavaScript kapalı yol) kalan risk olarak adlandırılmış. (2) Sınır değerleri yazılı: gövde tavanları, 60 saniyelik 6 dilimli kayan pencere, sayfa boyları. (3) Sipariş işlemlerinin ortak kuralı (9.9 girişi: sürüm → 422 sırası, hepsi ya da hiç, 404 sahipliği) 34 işleyicide tutarlı. (4) Her ayar bölümünün kendi sürümü ve 409'u var; yönetici kaldırmada danışma kilidi, son yönetici denetimi ve oturum basamaklaması ayrıntılı.

#### Katman 4 — Güvenlik

İki mercek: L4a (vitrin), L4b (panel); OWASP Top 10:2025. Bilinçli kararlar (tek rol, yönetici için iki adımlı doğrulama yok) bulgu sayılmadı.

| # | Seviye | Bulgu | Kaynak | Öneri / sonuç |
|---|---|---|---|---|
| L4a-1 | Medium → Low | UC-25 `Recalculate` oturumsuz misafire "bu e-posta kayıtlı" hatırlatmasını her gönderimde döndürüyor; L-3 sayacına sayılmıyor | 07 §4.13.4 UC-25; `02 §8.2` L-3 | Hatırlatma genel form sınırında, L-3'e sayılmaz; `02 §3.13.11`, §8.1.4. Uygulandı (L6a-9 bu cümleyle karşılandı) |
| L4a-2 | Medium | UC-27 misafir e-postasını doğrulamaz: başkasının adresine sipariş e-postası gider, adres bir hesapsa sipariş o hesaba düşer | 07 §4.13.6 UC-27; `02 §8.2.2`, §3.13.3 | **K-1217 (2)** (kalan risk). `02 §3.13.3`; `08` ve `09` park. Uygulandı |
| L4a-3 | Medium | Kişisel veri taşıyan oturumlu sayfalar için `Cache-Control: no-store` kuralı yok (= L4b-2) | 07 §2.5.6, §4 girişi; `05 §6.5.4` | **K-1217 (1).** Uygulandı |
| L4a-4 | Medium → Low | `search` içinde `%` ve `_` gibi LIKE meta karakterleri ve uzunluk sınırı yazılı değil | 07 §4.3.1 UC-03, §2.8.1; `04` 7.3.2.17; `05 §7.9.1` | UC-03 İstek (P-27, `%` ve `_` harfi harfine); `04` 7.3.2.17; `09` ve `12` park. Uygulandı |
| L4a-5 | Medium → Low | İndirme ucu yalnız sayfa isteği sınırında; tek bağlantı bir saat boyunca 500 MB'ı sınırsız kez verebilir | 07 §4.31.1 UC-37, §2.8.3; `05 §7.7.4` | §2.8.3 ve `05 §7.7.4` yazıldı; DEPLOY_RUNBOOK park. Uygulandı |
| L4a-6 | Low | UC-55 / UC-57 sahte özetli doğrulamayı yazmıyor (K-1204) | 07 §4.22.2 UC-55, §4.23.2 UC-57 | UC-55 hata ve UC-57 yan etkiler (= L12-5). Uygulandı |
| L4a-7 | Low | Veri koruma anahtar halkası beş değeri korur, "her kullanım ayrı amaç dizgesi" kuralı yazılı değil | 07 §4.20.1, §4.20.4, §4.31.1; `05 §7.6.3` | `05 §7.6.3` yazıldı; `09` ve `12` park (`07`'de değişiklik yok). Uygulandı |
| L4b-1 | Medium | = L3f-9 (CSV formül öneki; UC-182, UC-183, UC-184) | 07 §9.24.2–§9.24.4; `02 §10.7.1`; K-1195 (6) | **K-1217 (3).** Uygulandı |
| L4b-2 | Medium | = L4a-3 (panel GET'lerinde `no-store`) | 07 §2.5.6, §9 girişi; `05 §6.5.4` | **K-1217 (1).** Uygulandı |
| L4b-3 | Low | UC-114 İstek, ürün açıklaması için "kapalı biçim setiyle süzülür" cümlesini taşımıyor | 07 §9.5.2 UC-114; `05 §6.5.3` | Cümle eklendi. Uygulandı |
| L4b-4 | Low | "Açık sayfalar … doğrulama ekranlarıdır" cümlesi E-54'ün üç adresiyle çakışıyor (biri oturum ister) | 07 §2.3.1, §9 girişi Yetki; `05 §6.2.2` | Açık adresler adıyla yazıldı. Uygulandı |
| L4b-5 | Low | Panelin liste → kayıt → kaydet zincirindeki `returnUrl`'nin doğrulama kuralı `05 §6.5.6`'da kapsandığını söylemiyor | 07 §2.6.8, UC-116, UC-114; `05 §6.5.6` | §2.6.8, UC-116 ve `05 §6.5.6` başlığı hizalandı. Uygulandı |
| L4b-6 | Low | Tutar doğrulaması `numeric(12,2)` tavanı için giriş tavanı ve aşım yanıtı yazmıyor (500 riski) | 07 §9.5.2, §9.7.2, §9.18.2, UC-133; `06` | **K-1214 (2).** §2.9.4 ve UC-168 422. Uygulandı |
| L4b-7 | Low | Kupon kaydının işlem izine yazılıp yazılmadığı UC-122'de yazılı değil | 07 §9.7.2 UC-122; `02 §10.3.1` | "Kupon kaydı işlem izine yazılmaz (K-310)" yazıldı. Uygulandı |

**OWASP Top 10:2025 kategorisiyle işaretlenen güvenlik bulguları:**

| Kategori | Bulgular |
|---|---|
| A01 Erişim Kontrolü | L4b-4, L4b-5 |
| A02 Güvenlik Yanlış Yapılandırması | L4a-3, L4b-2 (önbellek başlığı) |
| A04 Kriptografik Hatalar | L4a-7 |
| A05 Enjeksiyon | L4b-1 (CSV formülü), L4a-4 (arama meta karakterleri; A06 ile birlikte), L4b-3 |
| A06 Güvensiz Tasarım | L4a-1, L4a-2, L4a-5 (kaynak tüketimi) |
| A07 Kimlik Doğrulama Hataları | L4a-1 (numaralandırma), L4a-6 |
| A09 Kayıt ve Uyarı | L4b-7 |
| A10 İstisnai Durumların Yanlış Ele Alınması | L4b-6, L4a-5 (A06 ile birlikte) |

**Güçlü yönler:** (1) Sahiplik reddi ve var olmayan kayıt aynı 404; iç anahtar yola girmez; sepet kimliği çerezde rastgele, veritabanında özetle. (2) `__Host-` önekli Secure / HttpOnly / Lax çerezler, her tarayıcı POST'unda doğrulama, açık yönlendirme kuralı. (3) Panel girişi hesap yokken de sahte özetle doğrulanır; sıfırlama yanıtı nötr. (4) Dışa aktarma doğru yetkilendirilmiş (yalnız POST, `no-store`, `attachment`, her indirme izde); işlem izi veritabanı düzeyinde yalnız-ekleme.

#### Katman 5 — Hata modu

İki mercek: L5a (vitrin), L5b (panel). Katman skoru ⚠ (L5a ⚠, L5b ✓).

| # | Seviye | Bulgu | Kaynak | Öneri / sonuç |
|---|---|---|---|---|
| L5a-1 | High | UC-29 her gönderimde ödemeyi yeniden başlatıyor; iki oturum da ödenirse ikinci tahsilatın karşılığı yok, "zaten Ödendi" hiçbir şey yapmıyor (= L6a-8) | 07 §4.14.2 UC-29 Idempotency; `05 §4.5.3` | **K-1211 ⚠.** Uygulandı |
| L5a-2 | Medium → Low | Hesap bağlantısı e-postaları P-38 çizelgesine girer; kanal düşünce eski ve yeni satırların sırası ve bağlantı ömrü yazılı değil | 07 §4.23.2 UC-57, §4.20.3; `05 §4.4.3`, §6.4.6 | `05 §4.4.3` yazıldı. Uygulandı |
| L5a-3 | Low | Hak UC-36'da düşer, dosya UC-37'de okunur; dosya depoda okunamıyorsa hakkın iadesi yazılı değil | 07 §4.16.3 UC-36, §4.31.1 UC-37 | UC-37 Yan etkiler yazıldı. Uygulandı |
| L5b-1 | Medium | Kart hattı yan etkileri "iade isteği sağlayıcıya gider" diyor; `05`'e göre istek kuyrukta, POST'tan sonra gider | 07 §9.9.10 UC-131, UC-133, UC-135, UC-129; `05 §4.6.1`, §4.3.2 | UC-131 yan etkiler (kart hattı sırası) ve atıflar; `04` 9.9.21 ve 6.3.9.19'a tek cümle. Uygulandı |
| L5b-2 | Medium → Low | Kart iadesinin iş satırı kayıt yazılamadan düşerse yöneticinin ekranında işareti ya da çıkışı yazılı değil | 07 §9.9.10 UC-131; `05 §4.3.4`, §9.4.16; `04` 6.3.9.19 | `05 §10.1.3` kalan risk cümlesi (`07` değişmedi). Uygulandı |
| L5b-3 | Medium → High | = L3e-4 (yeniden gönderimin sonucu 303'te değil kuyrukta) | 07 §9.9.32 UC-153, UC-154; `05 §4.4.1`, §4.4.3; `04` 9.9.33 | = L3e-4. Uygulandı |
| L5b-4 | Low | Dışa aktarma isteğin içinde, tek seferde üretiliyor; aralık büyüklüğü, süre ve bellek tavanı yok (= L7-2) | 07 §9.24.2 UC-182; `05 §6.8.4` | `07` değişmedi; `09` park (akışla yazma, süre ve boyut tavanı). Uygulandı |
| L5b-5 | Low | Betiksiz yolda P-23 + 1 MB'ı aşan gövde 413 alır ve form girdileri kaybolur | 07 §9.26.2 UC-117 | §2.8.7 ve UC-117 413 kalan risk cümlesi. Uygulandı |

**Güçlü yönler:** (1) Sonucu belirsiz işlemler (§2.7.1, §6.4.2) ve 15/30 sn zaman aşımı + kuyrukta yeniden deneme kuralı uçlarda tutarlı. (2) Sipariş onayı tek işlemde; onay anahtarı ve sepet kilidi çift siparişi önlüyor. (3) Kart iadesinde kayıt-önce-istek, "sonucu bekleniyor" ve firmanın "gerçekleşmedi" çıkışı var. (4) Parçalı yüklemenin çökme sonrası sürdürülebilirliği (K-1203) yazılı.

#### Katman 6 — Veri akışı

İki mercek: L6a (vitrin), L6b (panel).

| # | Seviye | Bulgu | Kaynak | Öneri / sonuç |
|---|---|---|---|---|
| L6a-1 | Medium | KDV oranı ürün düzleminde değişebilir ama özet parmak izi ve UC-27 özet farkı listesi KDV'yi saymıyor | 07 §4.13.6 UC-27, §4.13.1 UC-22; `05 §4.2.1`; `02 §3.17.2` | `05 §4.2.1` ve UC-27 / UC-22 yazıldı; `12` park. Uygulandı |
| L6a-2 | Medium | UC-26 "bütün formu" çereze yazar; `05 §6.7.9` yalnız e-posta, adres, kupon sayıyor; onay kutuları ve anahtarlar yazılmamalı | 07 §4.13.5 UC-26, UC-22; `05 §6.7.9` | UC-26 Yan etkiler ve `05 §6.7.9` hizalandı; `12` park. Uygulandı |
| L6a-3 | Medium → **Low** | Ödeme başarısında siparişe giren kalemlerin sepetten çıkması UC-80 / UC-123 yan etkilerinde yazılı değil | 07 §4.30.1 UC-80, UC-123; `05 §4.5.3`; `02 §3.16.12` | UC-80 ve UC-123 yan etkiler, `05 §4.5.3`. Uygulandı (kilit sırası çelişkisi yok) |
| L6a-4 | Low | "Altı işleyici aynı formu gönderir" der; ödeme adımının POST işleyicisi beştir | 07 §4.13 girişi | "Beş işleyici" düzeltildi. Uygulandı |
| L6a-5 | Medium → Low | Vitrin uçlarının Yan etkiler maddesi firmaya giden F-1, F-2, F-3, F-4 bildirimlerini saymıyor | 07 §4.13.6 UC-27, UC-15, UC-40, UC-41, UC-43, UC-45, UC-46, UC-80 | Yan etkilere F- bildirimleri yazıldı. Uygulandı |
| L6a-6 | Medium | UC-29 "sepetin ödeme işareti yazılır" der; hangi sepet olduğu belirsiz | 07 §4.14.2 UC-29; `06 §4.15` | **K-1212 (2).** Uygulandı |
| L6a-7 | Medium → Low | Google ile ilk girişte doğan hesaba geçmiş misafir siparişlerinin düşmesi UC-51 / UC-53 yazan-GET listesinde yok | 07 §4.22.3 UC-51, §2.2.5, §4.21.2 UC-53; `03` 9.1.6 | §2.2.5, UC-51 yan etkiler, UC-53 iki yer. Uygulandı |
| L6a-8 | Medium → High | = L5a-1 (Ödendi siparişe gelen ikinci başarılı ödeme) | 07 §4.14.2 UC-29; `05 §4.5.3`; `02 §6.2.16` | **K-1211 ⚠.** Uygulandı |
| L6a-9 | Low | UC-25'in e-posta hatırlatması hiçbir deneme limitine bağlı değil | 07 §4.13.4 UC-25, UC-22; `02 §8.1.4` | L4a-1 ile aynı cümlede çözüldü. Karşılandı |
| L6a-10 | Medium | = L3c-1 (UC-76 geri alma, adres başka hesaba geçtiyse) | 07 §4.28.2 UC-76; `02 §3.13.14`; `06 §4.36` | **K-1210 ⚠.** Uygulandı |
| L6b-1 | High | UC-131 / UC-132 var olmayan `RefundId` bekliyor: bekleyen geri ödeme saklanan satır değildir | 07 §9.9.10 UC-131, §9.9.11 UC-132, §5.2; `06 §4.33` | UC-131 / UC-132 İstek, 404 / 409; §5.2 iki model bölündü (`ProcessRefundInput`, `MarkBankTransferRefundFailedInput`). Uygulandı |
| L6b-2 | Medium → Low | UC-133 tavanı "gönderilmiş" geri ödemeyi düşer; başlatılmış olanlar da düşülmeli | 07 §9.9.12 UC-133; `02 §5.5`; `06 §4.33` | UC-133 hata parantezi. Uygulandı |
| L6b-3 | Medium | "Mal sonradan ulaşır — ödenmiş tutar düşülür" matris satırı UC-140 gövdesinde yok | 07 §9.9.19 UC-140, §9.9.21 UC-142; `02 §10.4.9`; `03 §5.2.2.5` | UC-131 "Tutar düşümü", UC-92, UC-140; matris hücresi UC-140 · UC-131. Uygulandı |
| L6b-4 | Medium | = L3f-5 (dışa aktarma kapsamı ve imha edilmiş sipariş) | 07 §9.24.2 UC-182; `02 §10.7.1`; `06 §9.2.1` | **K-1213 ⚠.** Karşılandı |
| L6b-5 | Medium → Low | Kupon hakkının geri dönüşü kümülatiftir; `UsedCount`'u hangi işlemin azalttığı yazılı değil | 07 §9.9.8 UC-129, §9.9.17 UC-138, UC-130, UC-140; `03 §1.4.4` | §9.9 girişine ortak cümle ve atıflar. Uygulandı |
| L6b-6 | Low | = L3f-3 (değişmeyen kayıt da yeni metin sürümü açıyor) | 07 §9.16.2, §9.18.2–§9.18.6; `02 §3.33.3` | **K-1215 (2).** Uygulandı |

**Güçlü yönler:** (1) Onay anahtarı akışı uçtan uca kapalı; iki sekmenin iki anahtarı sepet kilidiyle sıraya girer. (2) Erişim anahtarının yolculuğu (UC-27 → e-posta → UC-32 → UC-29/UC-30 → firma e-posta düzeltmesinde yenilenme → imhada 404) `06 §4.17` ve `02 §3.22.5` ile örtüşüyor. (3) Snapshot ↔ referans ayrımı bölüm bölüm açık: kupon, IBAN, iade adresi, kimlik ve kargo ayarı yalnız yeni siparişlere işler. (4) Dışa aktarmada iz satırı dosya üretildikten sonra ve gönderimden önce yazılır; boş kümede iz yok.

#### Katman 7 — Ölçeklenebilirlik

Merceği L7.

| # | Seviye | Bulgu | Kaynak | Öneri / sonuç |
|---|---|---|---|---|
| L7-1 | Low | Sayfa numaralı gezinmede "en yeni önce" düzeninin eşitlik anındaki ikincil anahtarı yazılı değil (= L3e-5) | 07 §2.6.5; `06 §7.5.1` | §2.6.5; `09` park. Uygulandı |
| L7-2 | Low | Dışa aktarma eşzamanlı POST; aralık, satır sayısı ve istek süresi üst sınırı yazılı değil (= L5b-4) | 07 §9.24.2–§9.24.4; `05 §6.8.4` | UC-182 yan etkiler tek cümle (akışlı üretim); tavan ve zaman aşımı `09` park. Uygulandı |

**Güçlü yönler:** (1) Sayfa numaralı gezinme gerekçeyle seçilmiş (§2.6.9); `Order` ve `Product` liste dizinleri `06 §7.5` ile örtüşüyor, arama trigram dizinli. (2) Durum veritabanında ve dosya deposunda; "büyüdüğünde X'e geçilir" ifadeleri ön koşullu ve tetikleyicili (`05` 11.3). (3) Satış özeti ve site haritası hesabı gerekçeli. (4) Hız sınırı sayaçları veritabanında (kayan pencere); ikinci örnekte paylaşım sorunu doğurmaz.

#### Katman 8 — Bağımlılık

Merceği L8 (resmî belgelere karşı okuma bu adımda yapılmadı; bkz. "Yasal dayanak ve teknik doğrulama günlüğü").

| # | Seviye | Bulgu | Kaynak | Öneri / sonuç |
|---|---|---|---|---|
| L8-1 | High | = L3e-4 / L5b-3 (303'ün sayfası gönderimin sonucunu taşıyor gibi okunuyor; `05` yeniden gönderimi kuyruğa koyar) | 07 §9.9.32 UC-153, UC-154; `05 §4.4.3`, §5.1.4, §10.1.1 | = L3e-4. Uygulandı |
| L8-2 | Medium → Low | "Sağlayıcıya bağlanılamadı" / "Google'a ulaşılamadı" ifadelerinin neyi kapsadığı (15 sn zaman aşımı, yeniden deneme yasağı) yazılı değil | 07 §4.14.2 UC-29, §4.22.3 UC-51, UC-62, §6.3 | UC-29 yan etkiler, UC-51 hata, UC-62 İstek; `08` park. Uygulandı |
| L8-3 | Medium | UC-80: imzası doğru ama gövdesi çözümlenemeyen ya da olay türü tanınmayan bildirimin yanıtı yazılı değil | 07 §4.30.1 UC-80; `05 §4.5.2`, §4.5.5 | **K-1216.** UC-80 başarı, `05 §4.5.5`. Uygulandı |
| L8-4 | Medium → Low | `07`'nin dört adresi (ödeme bildirimi, ödeme dönüşü, Google yönlendirmesi, dış erişilen adres) başka sistemlerde kayıtlı; adres değişirse kimlik kalır kuralı yazılı değil | 07 §2.1.9, §3 envanteri | §2.1.9; DEPLOY_RUNBOOK park. Uygulandı |
| L8-5 | Low | Canlılık ucunun veritabanı denetim zaman aşımı yazılı değil; veritabanı takılırsa uç 503 yerine asılı kalır | 07 §4.33.1 UC-81; `05 §5.4.1`, §9.4.1 | UC-81 (süre kurulum ayarı); `09` ve DEPLOY_RUNBOOK park. Uygulandı |

**Güçlü yönler:** (1) Sağlayıcı adı hiçbir adreste, hata kodunda ya da alan adında sızmıyor; yöntem, imza ve gövde biçimi `08`'e devredilmiş. (2) Sağlayıcı davranışına bağlı kararlar koşullu ve "bilgim tarihli; `08`'de doğrulanır" diye işaretli. (3) UC-80 kayıt-sonra-işle ve tekil bildirim kimliğiyle sağlayıcının tekrar göndermesine dayanıklı. (4) Dış kesintiler `05 §10.1`'in bağımlılık tablosuyla karşılanmış; `07` bunları adlandırılmış hata kodlarıyla ürün davranışına çeviriyor.

### Karar listesi (K-1207…K-1217)

Kayıt modu K-992 (⚠ öneriyle kayıt). Tam metinleri ve elenen seçenekleri karar kaydında (`PRODUCT_DISCOVERY_STATUS.md` §2). Üç ⚠ karar (K-1210, K-1211, K-1213) sorulmadı; çok kritik değildir, ayrıntı karar kaydında.

| K | Karar (tek cümle) | Bulgular | İşaret |
|---|---|---|---|
| K-1207 | Bu projede bütün alt ajanlar yalnız Sonnet ya da Haiku ile ve `low` eforla koşar; Opus ve Fable hiçbir işte kullanılmaz (K-1145'in model ve efor maddeleri değişti) | (süreç kararı) | — (proje sahibinin kararı) |
| K-1208 | `returnUrl` yalnız panelin listeleri için geçerlidir; vitrinde sipariş geçmişinden sipariş sayfasına dönüş yoktur | L3a-2, L3b-5 (IT-6) | öneriyle |
| K-1209 | Teyit ekranı ödeme yönteminden bağımsız olarak "artık ödeme beklemiyor" yönlendirmesini verir; vitrinden kalkan kalemin adedi artırılıp azaltılamaz (303 → `/sepet`) | L3b-2, L3b-3 | öneriyle |
| K-1210 | E-posta değişikliğini geri almada eski adres başka hesabın doğrulanmış adresi olmuşsa geri alma uygulanmaz (422, tüketim geri döner) | L3c-1, L6a-10 | ⚠ |
| K-1211 | Ödeme sağlayıcısında sipariş başına tek açık ödeme oturumu olur ve Ödendi siparişe gelen ikinci başarılı kart ödemesi iade edilir | L5a-1, L6a-8 | ⚠ |
| K-1212 | Gösterilen değerle karşılaştırmada gizli alan ilk formun değerini taşır; UC-29'un sepet işareti isteği yapan tarayıcının sepetine yazılır | L12-6, L6a-6 | öneriyle |
| K-1213 | Siparişlerin dışa aktarma dosyası yalnız ödemesi tamamlanmış siparişleri içerir; bir kalem bir satırdır, imha edilmiş siparişin kişisel sütunları boş gelir | L3f-5, L6b-4 | ⚠ |
| K-1214 | Metin alanları baş ve sondaki boşluktan arındırılır; tutar alanlarının üst sınırı `06`'daki para türünün (`numeric(12,2)`) sınırıdır, yeni parametre açılmaz | L3d-4, L4b-6, L3f-7 | öneriyle |
| K-1215 | Satış özeti hazır dönemleri Türkiye günüyle (Son 7 / 30 gün bugünü içerir); üretilen iki metnin yeni sürümü yalnız metni besleyen alan değiştiyse yazılır; yeni kaydın `SortOrder`'ı grubun en büyüğü + 1'dir | L3f-1, L3f-3, L6b-6, L3f-6 | öneriyle |
| K-1216 | İmzası doğru ama gövdesi çözümlenemeyen ya da olay türü tanınmayan ödeme bildirimi işaretli kaydedilir ve 200 döner | L8-3 | öneriyle |
| K-1217 | Kişisel veri taşıyan oturumlu sayfalar `no-store` döner; misafir e-postasına sipariş e-postası kalan risktir (sayaç `08`'e park); CSV hücresinin formül önekli değeri `'` ile nötrlenir | L4a-3, L4b-2, L4a-2, L3f-9, L4b-1 | öneriyle |

**⚠ olanlar (3):** K-1210, K-1211, K-1213. Üçü de öneriyle kaydedildi, çok kritik soru çıkmadı; proje sahibine tek mesajda gösterilmesi cross-review sonrasına bırakıldı.

### Yürütücünün hükümleri

İki doğrulayıcının ayrıştığı ya da hükmün kendi içinde çeliştiği yerlerde son hükmü adımı yürüten Sonnet ajanı verdi (K-1207).

- **L3a-1 (High).** Birinci doğrulayıcı Medium, ikinci doğrulayıcı High dedi. Hüküm: **High kaldı** — tutarda nokta sessizce 100 kat hata verir ve uygulayıcıyı yanlış koda götürür.
- **L12-2.** Birinci doğrulayıcı çürüttü, ikinci doğrulayıcı kısmen doğruladı. Hüküm: **kısmen doğrulandı, Low** — adı konmuş hâl ama hata değil, kod açılmadı; tek cümle yazıldı.
- **L6a-3.** Medium → **Low**: kilit sırası çelişkisi yok, eksik yalnız yan etki cümlesidir.
- **L3d-7.** İki doğrulayıcı birlikte çürüttü: iki durumun mesajı ortak değil, `07 §6.1.1`'in yasağı tetiklenmiyor. **Çürütüldü.**
- **L3e-3 (çürütüldü).** Sonnet ve ikinci doğrulayıcı "doğrulandı" demişti ve düzeltme önce uygulandı. Yürütücü hükmü: bu, **K-1206'nın elediği seçenektir ve K-1089'un açık seçimidir** (kategori silmesi kilide girmez; dolu kategorinin silinmemesi kısıtlayıcı bağla korunur; eşzamanlı yarış adıyla kalan risktir). Yeni olgu yok, bilinçli karara itiraz edilmez. Düzeltme geri alındı. Sonnet ve ikinci doğrulayıcı kararı atlamıştı.
- **L3f-1.** Hükmün iç çelişkisi çözüldü: "Son 7 / Son 30 gün" bugünü içerir (bugün−6 … bugün / bugün−29 … bugün). Mercek High → **Medium** (tek gün seçimi belirsiz değil).
- **IT-6 (audit'ten devredilen).** Hüküm seçenek **(b):** `returnUrl` yalnız panelin listelerinden kayda giden bağlantıda; vitrinde sipariş geçmişinden sipariş sayfasına dönüş yok (K-1208).
- **L3f-5.** Seçenek **(B)** (yalnız ödemesi alınmış siparişler) seçildi, (A) elendi; L6b-4 bu hükümde karşılandı (K-1213).

### Yasal dayanak ve teknik doğrulama günlüğü

- **Yeni yasal dayanaklı bulgu çıkmadı.** `07` mevzuatı adıyla anmaz; audit'in günlüğü (`07_AUDIT.md`, "Yasal dayanak") geçerlidir.
- **K-1213 (dışa aktarma kapsamı)** `02 §10.6.2`, `02 §10.7.1` ve K-823'e dayanır. Mevzuat bu adımda yeniden okunmadı.
- **Dış belge (ödeme sağlayıcısı, Google, PostgreSQL, Npgsql) bu adımda yeniden okunmadı — doğrulanamadı.** Sağlayıcıya özgü kararlar (dönüş yöntemi, yeniden gönderme, webhook şeması) "bilgim tarihli" işaretiyle `08`'de doğrulanacak şekilde durur (L8-2, L8-3).
- **OWASP Top 10:2025.** Güvenlik bulgularının kategori tablosu "Katman 4 — Güvenlik" bölümündedir. Kategori adları merceklerin atamasıdır; OWASP belgesi bu adımda yeniden okunmadı.

### Geri besleme — üst dokümanlara ve park satırlarına

Geri besleme aynı PR'da yapıldı (K-652). Yeni kural numarası açılmadı; üst dokümanların ✓ durumu korunur ve kalite döngüsü yeniden açılmaz. Her üst dokümanın deep review paragrafı ve `07 §1.3` tablosu aynı listeyi taşır.

| Doküman | Sürüm (eski → yeni) | Değişen bölümler (K) |
|---|---|---|
| `02_PRODUCT_REQUIREMENTS.md` | v0.91 → **v0.92** | §3.13.14 (K-1210) · §6.2.16 (K-1211) · §10.7.1 (K-1213) · §3.13.3 (K-1217); ayrıca §3.13.11, §8.1.4 (L4a-1, L12-5) |
| `03_USER_FLOWS.md` | v0.38 → **v0.39** | 3.2.1.16 (K-1211); matris hücreleri 2.5.3.1, 9.1.4, 5.2.2.5 |
| `04_UI_SPECS.md` | v0.38 → **v0.39** | 5.27.7, 2.13.3 (K-1208) · 3.3.39, 5.14.1, 5.14.10, 6.2.14.5 (K-1209) · 5.28.3 ve yeni 6.2.28.3 (K-1210) · 9.24.3 (1), 9.24.7 (K-1213) · 7.3.1, 7.3.2.8 (K-1214) · 9.22.3 (1), 9.13.11, 9.18.9 (K-1215) · 2.5.4, 9.9.33, 9.9.21, 6.3.9.19 (L3e-4, L5b-1 açıklık notları) · 7.3.2.17 (L4a-4) |
| `05_TECHNICAL_ARCHITECTURE.md` | v0.27 → **v0.28** | 6.4.6 (K-1210) · 3.6.3, 4.5.3 (K-1211) · 4.5.5 (K-1216) · 6.5.4, 6.5.5 (K-1217) · 4.2.1, 6.7.9, 4.4.3, 7.6.3, 7.7.4, 10.1.3, 6.1.4, 6.5.6 (K'sız bulgular) |
| `06_DATA_MODEL.md` | v0.29 → **v0.30** | §4.15 (K-1212) · §4.47 (K-1215) |
| `08_INTEGRATION_SPEC.md` | sürüm değişmez | park: tek açık oturum ve sipariş başına tekil referans (K-1211) · alıcı başına gönderim sayacı (K-1217) · zaman aşımı dili (L8-2) · webhook şema izleme (L8-3) |
| `09_CODING_GUIDELINES.md` | sürüm değişmez | park: tutar ayrıştırma (L3a-1) · dizinli liste bağlama (L3d-5) · arama meta karakteri (L4a-4) · anahtar halkası amaç dizgesi (L4a-7) · akışla yazma ve süre / boyut tavanı (L5b-4, L7-2) · CSV formül nötrleme (K-1217 (3)) · sıralama ikincil anahtarı (L7-1) · canlılık zaman aşımı (L8-5) |
| `12_VALIDATION_PROTOCOL.md` | sürüm değişmez | park: eşleşmeyen `handler` (L3a-3) · tek açık oturum / ikinci ödeme (K-1211) · tanınmayan bildirim (L8-3) · sepet işareti (L6a-6) · çerez içeriği, KDV parmak izi (L6a-2, L6a-1) · arama (L4a-4) · anahtar halkası (L4a-7) · sahte özet (L12-5) · dışa aktarma kapsamı ve CSV (K-1213, K-1217) · silinmiş kategori seçimi (L12-8) |
| `DEPLOY_RUNBOOK.md` | sürüm değişmez | park: `search` sınırı (L3a-5) · indirme eşzamanlı aktarım sınırı (L4a-5) · dış sistemlerde kayıtlı adresler (L8-4) · canlılık denetimi zaman aşımı (L8-5) |
| `PRODUCT_DISCOVERY_STATUS.md` | v0.162 → **v0.163** | Karar kaydı K-1207…K-1217; §1 tablo (02–07 satırları); §16.1 deep review maddesi `[x]`; K-1207 geçişi; ⚠ listesi; öğrenim adayı 5. Geri işaretler: K-840 (K-1209), K-1080 (K-1210), K-1089 (K-1216), K-1145 (K-1207), K-1167 (K-1208), K-1174 (K-1214), K-1183 (K-1212 (2)), K-1195 (K-1213, K-1217 (3)), K-1202 (K-1212 (1)) |
| `07_API_DESIGN.md` | v0.16 → **v0.17** | Başlık paragrafı; §1.3 geri besleme tablosu (10 satır, K-1208…K-1217) ve §1.1–§1.2 matris özetleri; gövde bölümleri yukarıdaki katman tablolarında |

Not: ilk satırdaki "K'sız bulgular" diye anılan yerler K numarası almayan bulguların yerleridir (apply log'larında K numarası yok); karar kaydına yansıyan değişiklikler K sütunundakilerdir.

### Aksiyon Planı

- **Critical:** yok.
- **High (7, dört ayrı sorun):** hepsi uygulandı — L3a-1; L3e-4 + L5b-3 + L8-1; L5a-1 + L6a-8; L6b-1.
- **Medium (26) ve Low (62):** hepsi bu adımda uygulandı, ikisi kısmen (L12-8, L3f-7) ve ikisi karşılandı (L6a-9, L6b-4).
- **Sonraki dokümanlara düşen işler (park satırları):**
  - `08` — tek açık ödeme oturumu ve tekil referans, alıcı başına e-posta sayacı, zaman aşımı dili, webhook şema izleme.
  - `09` — tutar ayrıştırma, dizinli liste bağlama, arama meta karakteri, anahtar halkası amaç dizgesi, akışla yazma ve tavanlar, CSV nötrleme, sıralama ikincil anahtarı, canlılık zaman aşımı.
  - `12` — yukarıdaki tablodaki senaryolar; L12-8'in "422 ya da beklenmeyen hata yolu" ifadesi senaryoyla sabitlenecek.
  - `DEPLOY_RUNBOOK` — `search` sınırı, indirme sınırı, dış adresler, canlılık denetimi.
- **Sıradaki adım:** `cross-review` (kalite döngüsünün son adımı).

### Cross-reference Haritası (bu adımın değişiklikleri)

| Değişiklik | `07` | Üst doküman / park |
|---|---|---|
| K-1208 `returnUrl` kapsamı | §2.6.8, UC-33, UC-74 | `04` 5.27.7, 2.13.3 |
| K-1209 teyit ekranı, UC-20 | UC-28, UC-29, UC-20, UC-21, §6.2.3 | `04` 3.3.39, 5.14.1, 5.14.10, 6.2.14.5 |
| K-1210 UC-76 422 | UC-76, §6 `EmailTakenByOtherAccount` | `02 §3.13.14`; `04` 5.28.3, 6.2.28.3; `05 §6.4.6` |
| K-1211 tek açık oturum, ikinci ödeme | UC-29, UC-80 | `02 §6.2.16`; `03` 3.2.1.16; `05 §3.6.3`, §4.5.3; `08`, `12` park |
| K-1212 gizli alan, sepet işareti | §2.4.3, UC-40, UC-43, §8.2.2, UC-29 | `06 §4.15` |
| K-1213 dışa aktarma kapsamı | UC-182 | `02 §10.7.1`; `04` 9.24.3, 9.24.7; `12` |
| K-1214 kırpma, tutar sınırı | §2.9.4, §2.9.10, UC-168, UC-170 | `04` 7.3.1, 7.3.2.8 |
| K-1215 dönemler, metin sürümü, sıra | UC-105, §9.18 girişi, UC-164, UC-168–UC-172, UC-158, UC-161, UC-96 | `04` 9.22.3, 9.18.9, 9.13.11; `06 §4.47` |
| K-1216 tanınmayan bildirim | UC-80 | `05 §4.5.5` |
| K-1217 `no-store`, CSV, misafir e-postası | §2.5.6, §4 ve §9 girişleri, UC-182 | `05 §6.5.4`, §6.5.5; `02 §3.13.3`; `09`, `12` park |
| Sayı güncellemeleri | §6.2 = 112 (`InvalidPriceRange`); §5.2 tablo satırı 84 → 86 | Matris (`07 §1.1`) |

### Mekanik denetimler (kapanış)

- **`atif.py`** (atıf denetimi): K: 1 · KA: 0 · UC: 9 · E: 0 · SEC: 101 — uygulamadan önceki ile aynı yanlış pozitifler.
- **`defcheck.py`** (tanım blokları): 180 tanım bloğu, 10 blok bilinçli okunuş, belirsiz sözcük sıfır.
- **Matrisin §1.1.1 betiği, önce ve sonra:** `aday07`, `deger07`, `durum07`, `e1a`, `e1b`, `e1d`, `e1e`, `gap07` aynı; `geri07`, `tab07`, `e1c`, `e1f` ve özet değişti. Çiftler 5.071 → **5.073** (`04` 2.961 → 2.962 · `03` 1.579 → 1.580), satır 2.318 → **2.319** (iki yeni satır: `04` 6.2.28.3 UC-76'ya, `05 §6.5` m6 `no-store` maddesi konvansiyon); aday sayısı 187 aynı. §6.2 111 → **112** kod (`InvalidPriceRange`); §6.3 22 kod (değişmedi); §5.2 tablo satırı 84 → 86; model sayıları 96 + 19 aynı. Uç eklenmedi ve çıkmadı.
- **Karar kolu taraması:** K-1207…K-1217 kolları kayıtla uyumlu.
- **K tekilliği:** K-1207…K-1217 karar kaydında satır başı birer kez.
- **CR / kontrol karakteri / `$rakam` taramaları** (yönetici kapanışta koşar): sonuç: 0 · 0 · 0 (apply günlüğü 9 dosyada 0 / 0 / 0 verdi).
- **Ölçüm satırı:** mercekler 15 · Sonnet / low · ~2,60 M token; birinci doğrulayıcılar 8 · Sonnet / low · ~0,88 M; ikinci doğrulayıcılar 4 · Sonnet / low · ~0,53 M; uygulayıcılar 9 (A1…A7) · Sonnet / low · ~1,14 M; rapor taslağı 1 · Sonnet / low · ~0,19 M. Toplam ~5,35 M token, 37 alt ajan.

### Uygulanmayanlar

| Bulgu | Durum | Gerekçe |
|---|---|---|
| L3d-7 | Çürütüldü | `SingletonContentExists` iki uçta aynı hâli ("tekil kayıt zaten var") döner; iki durumun mesajı ortak değil (biri mesajsız yönlendirme). `§6.1.1`'in yasağı tetiklenmiyor; çok değerli HTTP sütunu kalıbı (`SalesClosed` 422 · 303) yerleşik |
| L3e-3 | Çürütüldü (yürütücü) | K-1206'nın elediği seçenek, K-1089'un açık seçimi; yeni olgu yok. Önce uygulanan düzeltme geri alındı |
| L12-8 | Kısmen | Hüküm kesin kod vermiyor: silinmiş kategori seçiminin sonucu "422 ya da beklenmeyen hata yolu" diye yazıldı, `12`'de senaryoyla sabitlenecek |
| L3f-7 | Kısmen | Tutar üst sınırı (K-1214) ve alt çit işlendi; P-8 üst çiti ve tutar tavanı için ayrı P açılmadı — ürün parametresi kararı bu adımda alınmadı |
| L6a-9 | Karşılandı | L4a-1 ile aynı cümlede (UC-25 hatırlatması) |
| L6b-4 | Karşılandı | L3f-5 ile aynı düzeltmede (K-1213) |
| L5b-4 + L7-2 | `07`'de değişiklik yok (L7-2'de tek cümle) | Dışa aktarma tavanı (aralık, süre, bellek) `07`'ye sayı yazmadı; `09` park cümlesi: akışla yazma ve süre / boyut tavanı |
| L4b-6 / L3f-7 | P-8 üst çiti ve tutar tavanı P'si açılmadı | Fiyat tavanı P-sabiti (1.000.000 TL) yeni parametre ve sayım değişikliği ister; kazancı yalnız yönetici hatasını erken yakalamaktır (K-1214 elenen seçenek) |

### Cross-review için notlar

- **Boyut.** `07` ~1,17 MB; cross-review skill'inin 400 KB eşiğinin çok üstündedir. Her turda iki koşum yapılır (K-953).
- **Bu adımda en çok değişen yerler:** UC-153 / UC-154 ve §9.9.32–33 (yeniden gönderim kuyruğa alındı); UC-131 / UC-132 / UC-133 ve §5.2 (`RefundId` kalktı, iki model bölündü); UC-29 ve UC-80 (tek açık oturum, ikinci ödeme, tanınmayan bildirim, sepet işareti); UC-76 (422); UC-182 (kapsam, satır tanesi, CSV nötrleme); UC-105 (hazır dönemler); §2.9.4 (tutar ayrıştırma, üst sınır); §2.5.6 ve `05 §6.5.4`'ün `no-store`'u; §6.2.2 (`InvalidDate`, `InvalidPriceRange`).
- **Açık gözlemler:**
  - Apply log'larındaki "A5'e" / "A6'ya" dışı notlar: A4a'nın L5b-1 geri besleme notu A4b'de `04` 9.9.21 ve 6.3.9.19'a yazıldı; A2a'nın `12` senaryo notları (L3b-3, L3b-7) park satırı olarak yazılmadı.
  - L12-8'in "422 ya da beklenmeyen hata yolu" ifadesi `12`'de senaryoyla sabitlenecek.
  - K-1210 / K-1211 / K-1213 ⚠ satırlarının proje sahibine tek mesajda gösterilmesi.
  - IT-6 (L3a-2 = L3b-5) A1 log'unda "kısmen" (matris hücreleri A5'e kalmıştı), nihai tabloda "uygulandı"; matris hücreleri A5'te hizalandı.
- **Etki yansıtma.** `02` v0.92, `03` v0.39, `04` v0.39, `05` v0.28, `06` v0.30'u yeniden tarar (geri besleme bölümündeki listeler başlangıç noktasıdır).

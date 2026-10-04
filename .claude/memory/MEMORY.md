# Proje Hafızası

> **Bu dosya indeks + güncel durum snapshot'ıdır.** İçerik gömülmez — her hafıza ayrı bir dosyadır.
> Ne buraya girer / ne L1–L5'e terfi eder: [`README.md`](README.md).
> **Otoriter kaynak değildir** — task durumu için [`../../Docs/IMPLEMENTATION_STATUS.md`](../../Docs/IMPLEMENTATION_STATUS.md).

---

## Proje Özeti

- **Ad:** Shopfolio
- **Tanım:** Herhangi bir firmanın kendi ürünlerini sergileyip çevrimiçi satabileceği ve kurumsal tanıtımını yapabileceği; üyelik (sipariş için zorunlu değil), Google ile giriş, sipariş, ödeme ve sipariş takibi içeren web uygulaması.
- **Dönem:** Doküman üretimi
- **Doküman dili:** Türkçe · **Kod dili:** İngilizce

---

## Güncel Durum

> Kısa tut. Tarihsel detay `MEMORY_ARCHIVE.md`'ye taşınır (00 §G.2).

- **AŞAMA 2 AÇIK — Kullanıcı akışları → Kullanıcı Akışları (`03`), 2026-10-04** (`00 §C.1`; rol Product Owner / Business Analyst; girdi `01`, `02`). Aşama 1 2026-10-03'te kapandı (K-437'nin altı adımı, PR #39–#55).
- **Son tamamlanan — açılış (karar kaydı v0.54):** devir §7'nin iki sorusu kapandı. **K-647:** karar kaydı dönem boyu tek dosyadır (`00 §B`, §G.1) — Aşama 2 kararları `PRODUCT_DISCOVERY_STATUS.md` §2'ye K-647'den, açık detaylar §4'e A-19'dan; aşamanın planı §8. Aşama 1'in kayıtları (K-01…K-646, A-01…A-18, §6, §7) salt okunur, tek istisna geri işaret; dosyanın tamamı dönemin son aşamasında arşivlenir (K-436'nın kapsamı daraltıldı, checklist §7'nin 5. ve 6. adımı hizalandı). **K-648:** ⚠ öneriyle kayıt ve `docs:` PR'larında CI yeşilse merge yetkisi Aşama 2 boyunca açık. **Öğrenim terfisi adayları** aşama boyunca tracker §8.1'e yazılır (Aşama 1'de §6.1); iki aday kayıtlı — şablonun karar kaydı başlığındaki iki okumaya açık cümle · devir notundaki sorunun şablona karşı taranmadan sorulması (v0.55). **K-649:** project-playbook'a geri gidecek her öğrenim `Docs/PLAYBOOK_FEEDBACK.md`'de toplanır (39 satır); proje tamamlanınca proje sahibi gönderir (v0.56).
- **Önceki adım — girdilerin okunması ve konu planı (karar kaydı v0.57):** `01`, `02` (tamamı), `10`, devir §7 ve beş devir kararı okundu; plan tracker §8.2–§8.3'te **taslak**: 12 blok (`AK0`…`AK11`, `03` şablonunun bölümlerine bire bir), **62 konu** — 6 workshop (W), 3 plan önerisi (P), 53 türetim (T); 12 ★. Konu kimliği `AKn-mm` (Aşama 1'in `Bn-mm`'siyle ve `02`'nin B-1…B-15'iyle çakışmaz). W konuları: AK3-06 ayrılmış adede dokunan katalog işlemi · AK5-03 ⚠ iade malının reddi ve değer kaybı · AK6-04 doğrulama bağlantısının yeniden istenmesine limit · AK8-09 ⚠ bekleyen yönetici davetinin geri çekilmesi · AK8-10 ⚠ donmayan ayarların (havale IBAN'ı, iade adresi) yürüyen siparişe etkisi · AK10-03 ⚠ site kesintisinde süreler. Öğrenim adayları 3–4 → PF-40, PF-41. Plan kabul edildi ve §8.1'in ikinci maddesi işaretlendi (2026-10-04); üç plan önerisi öneriyle kaydedildi: **K-650** iskelet (§2 müşteri tarafı, §8 firma ve panel, §9 üyelik) · **K-651** uçtan uca anlatılar §2'nin son alt bölümü · **K-652** `02`'ye geri besleme K satırı + aynı PR'da sürüm artışıyla; ✓ ve kalite döngüsü yeniden açılmaz.
- **Önceki adım — workshop oturumu (karar kaydı v0.58, 2026-10-04):** altı W konusu K-648 modunda işlendi ve kapandı, **K-653…K-668** öneriyle kaydedildi. AK3-06: stok ve kontenjan ayrılmış adedin altına inmez (K-653), kupon adedi ayrılmış hakların altına inmez (K-654), açık siparişli ürün silinebilir (K-655) · AK5-03 ⚠: koşullu istisnada iade reddi kaydı (K-656) ve malın firmanın bedeliyle dönüşü + B-16 (K-657), kullanılmış ya da hasarlı dönen malda tam geri ödeme — değer kaybı ürün dışında (K-668) · AK6-04: L-9 doğrulama bağlantısının yeniden istenmesi, P-49 (K-658), yeni bağlantı öncekileri geçersiz kılar ve ömrü uzatmaz (K-659) · AK8-09 ⚠: davet geri çekilir (K-660), aynı adrese tek davet (K-661), gönderen kaldırılınca davetleri düşer (K-662) · AK8-10 ⚠: havale IBAN'ı siparişe donar (K-663), kapının hiçbir koşulu açık siparişi etkilemez (K-664), iade adresi beyana yazılır (K-665) · AK10-03 ⚠: süreler dondurulmaz (K-666), kesintide dolan havale süresinde iptal ertelenir (K-667; `05`'e ilk Aşama 2 park satırı). Geri besleme: Ürün Gereksinimleri **v0.48**, MVP Kapsamı **v0.30** (K-652). ⚠ listesi tracker §8.1'de.
- **Önceki adım — yazım turu, 1. oturum (karar kaydı v0.59, 2026-10-04):** Kullanıcı Akışları (`03`) **v0.2** — §0 yazım konvansiyonları ve bölüm haritası (AK0), §1 durum makinesi (AK1), §11'in ilk on üç satırı (workshop'un altı konusu dahil). **Konvansiyonlar sonraki oturumları bağlar:** alt bölüm haritası §0.2'dedir (K-670) · satır kimlikleri bölüm numaralı, `03 §2.4.3` (K-669) · adım tablosu §0.4.1, hata tablosu §0.4.2 · kaynak önce `02 §`, Aşama 2 kararlarında K · sayı yazılmaz, Z-/L-/P- kimliği yazılır · §1.4.3 kapanış kuralı ve §1.11 durum × rol × işlem tablosu akışların dayanağıdır. Yazım yedi boşluk buldu ve öneriyle kapattı: K-671 hizmet tamamlama ödeme onaylanmışken (Kısmen geri ödendi dahil) · K-672 teslim edilmiş kalemi olmayan siparişin Alındı/Hazırlanıyor'da cayma ya da çıkarmayla kendiliğinden kapanışı · K-673 ⚠ hizmette cayma düğmesi ödeme onayından sonra · K-674 son Yayında varyant silinemez · K-675 panel işaretlerinin kalkışı · K-676 kalem başına tek ayıp talebi · K-677 S8 geri alınamaz onay. Geri besleme: Ürün Gereksinimleri **v0.49**, MVP Kapsamı **v0.31** (KP-22).
- **Önceki adım — yazım turu, 2. oturum (karar kaydı v0.60, 2026-10-04):** Kullanıcı Akışları (`03`) **v0.3** — §2 müşteri tarafının ana akışları (2.1 vitrin · 2.2 kurumsal içerik ve iletişim formu · 2.3 sepet · 2.4 ödeme adımı ve onay · 2.5 ödeme ve teslim · 2.6 takip · 2.7 iptal ve gecikme feshi · 2.8 cayma ve iade · 2.9 ayıp talebi) ve 2.10 uçtan uca anlatılar (ana akış + dört anlatı); §11'e dört satır. Yazımın dört boşluğu öneriyle kapandı: K-678 ⚠ iletişim formunu gönderene alındı e-postası yok · K-679 siparişe tek kupon kodu · K-680 ⚠ üyenin ödeme adımında yazdığı yeni adres isterse deftere · K-681 ⚠ müşterinin ayıp talebini yeniden açması B-9 üretir. §1 hizalandı (§1.9.1, §1.11.10; S9 ve S11'in "Akış" sütunu). Geri besleme: Ürün Gereksinimleri **v0.50**, MVP Kapsamı **v0.32** (KP-12, KP-66). **Konvansiyon eki:** §2'nin satırları §3'ün hata tablolarının "Akış adımı" sütununa bağlanacak kimliklerdir — yeniden numaralanmaz; alt başlıklı alt bölümlerde satır dört düzeylidir (2.5.1.3).
- **Önceki adım — yazım turu, 3. oturum (karar kaydı v0.61, 2026-10-04):** Kullanıcı Akışları (`03`) **v0.4** — §3 hata akışları (`02 §6`'nın 6 ilkesi ve 138 senaryosu, her biri tam bir satır — betikle sayıldı), §4 zaman aşımları (Z-1…Z-47 ve kendiliğinden işleyen anlar, kesinti dahil), §5 itiraz ve anlaşmazlık, §6 kötüye kullanım ve inceleme; §11'e altı satır. **Konvansiyon (K-682):** §3.4 ve §3.5'in hata satırları §8 ve §9 yazılana kadar "Akış adımı"nda alt bölümü ve §1 satırını taşır — *§8.3 (§1.11.21)*; adım tablosunu yazan oturum hücreyi satır numarasına çevirir · hata tablolarında "—" yalnız kayıt doğurmayan ekran reddi içindir. Yazımın altı boşluğu öneriyle kapandı: K-683 ⚠ beş hata ve sistem olayı bildirim üretmez · K-684 ⚠ geç gelen kart ödemesinin iadesinde müşteriye B-8 · K-685 düzeltmeyle yeniden başlayan havale süresinde B-3 bir kez daha · K-686 ⚠ havale hattında IBAN'ı olmayan geri ödemede yönetici IBAN ister (yeni §1.11.39) · K-687 ⚠ giriş kaydı panelde görünmez (`05`'e park) · K-688 ⚠ cayma beyanı geri alınmaz. §1 ve §2 hizalandı. Geri besleme: Ürün Gereksinimleri **v0.51**, MVP Kapsamı **v0.33** (KP-15, KP-48, KP-66).
- **Önceki adım — yazım turu, 4. oturum (karar kaydı v0.62, 2026-10-04):** Kullanıcı Akışları (`03`) **v0.5** — §7 bildirim haritası (7.1 tek tablo: B-1…B-16, F-1…F-6 ve hesap e-postaları her tetikleyicisiyle · 7.2 bildirim üretmeyen olaylar ve olmayan bildirimler · 7.3 geçiş ve olay bazlı eşleme) ve §8 firma tarafının yönetim akışları (8.1 katalog · 8.2 olağan hat · 8.3 firma iptali, iade teslim alma, geri ödeme · 8.4 dokuz müdahale ve ayıp talebi · 8.5 manuel adım bütçesi · 8.6 kurumsal içerik · 8.7 mağaza ayarları · 8.8 yönetici hesapları · 8.9 bekleyen işler ve dışa aktarma); §11'e üç satır. Harita akışların bildirim hücreleriyle betikle iki yönde karşılaştırıldı; §3'ün §8'e bağlanan "Akış adımı" hücreleri satır numarasına çevrildi (K-682). Kararlar: **K-689** §7'nin alt bölümleri (tek tablo; §0.2 güncellendi) · **K-690 ⚠** havale hattında ödenmiş kalemin firma iptalinde ve kalem çıkarmasında IBAN isteği kayıtla düşer, B-14 · **K-691** F-4 her kendiliğinden kart iadesinde (fesih, çıkarma dahil) · **K-692 ⚠** yönetici daveti ve kaldırma bütün yöneticilere bildirilir — yeni F-6. Geri besleme: Ürün Gereksinimleri **v0.52**, MVP Kapsamı **v0.34** (KP-47, KP-48, KP-62, KP-67).
- **Önceki adım — yazım turu, 5. oturum (karar kaydı v0.63, 2026-10-04):** Kullanıcı Akışları (`03`) **v0.6** — §9 üyelik (9.1 kayıt, doğrulama ve Google ile ilk giriş · 9.2 giriş, oturum, şifre sıfırlama, yeniden doğrulama, panel girişi · 9.3 profil ve adres defteri · 9.4 hesap silme ve kişisel veri başvurusu · 9.5 tercih yönetimi — altı "yoktur"), §10 operasyonel akışlar (10.1 ödeme sağlayıcısı, e-posta altyapısı, Google kesintisi · 10.2 bakım ve güncelleme · 10.3 sitenin kesintisi — §4.2.11–§4.2.12'ye işaret eder · 10.4 panele erişimin kaybı) ve §11'in kapanışı (otuz satır). **Yazım turu tamamlandı — `03`'ün bütün bölümleri yazıldı.** §3.4'ün ve §7.1'in §9'a giden hücreleri satır numarasına çevrildi (K-682, K-689); `02 §6` 140 senaryo, "tam bir kez" sayımı 146 (betikle). Kararlar: **K-693 ⚠** hesap silindiğinde hesabın adresine "hesabınız silindi" e-postası (devir kapandı) · **K-694 ⚠** hesap olaylarında bildirim yokluğu · **K-695** şifre değiştirme öteki oturumları kapatır · **K-696 ⚠** Google ile ilk girişte doğrulanmamış bekleyen kayıt devralınmaz · **K-697** Google doğrulanmamış e-posta verirse hesap açılmaz · **K-698 ⚠** geri alma bağlantısı açıkken hesap silinmez · **K-699 ⚠** panele erişim kaybında kurtarma kurulum tarafının işidir (`DEPLOY_RUNBOOK`'a ilk park bloğu) · **K-700** iki aktör adı (Kullanıcı, Kurulum tarafı). Geri besleme: Ürün Gereksinimleri **v0.53**, MVP Kapsamı **v0.35** (KP-26, KP-27, KP-29, KP-77, ÖK-7). Tamamlama protokolü (`00 §C.4`) ve devir taraması betikle koştu; sonuç PR açıklamasında.
- **Son adım — kalite döngüsü: audit ve deep review (karar kaydı v0.64, 2026-10-04):** Kullanıcı Akışları (`03`) **v0.7**. On mercek iki dalgada (ilk paralel deneme oturum limitinde düştü; en çok beş paralel) ve iki şüpheci (kanıt, bağlam); seksen bir ham bulgu — yetmiş dokuzu doğrulandı, ikisi tartışmalı (YASAL-7, DR-8) ve işlenmedi, çürütülen yok; yetmiş altısı uygulandı. Rapor `Docs/AUDIT_REPORTS/03_AUDIT.md`, `03_DEEP_REVIEW.md`. On yedi karar K-648 modunda öneriyle: **K-701** atıf dizisinde önsüz `§` devralır (§0.3.4) · **K-702** aktörü olmayan olay satırı (§0.3.2) · **K-703 ⚠** düzeltme ↔ kalem kayıtları · **K-704 ⚠** sitede sipariş teyidi (HSY m.9/1) · **K-705 ⚠** sonucu belirsiz kart iadesi (`08`'e park) · **K-706 ⚠** başka kanaldan gecikme feshi · **K-707 ⚠** "Sipariş hakkında" talebi üç yıl · **K-708 ⚠** imha kaydı anlık silmeler · **K-709 ⚠** caymada süreye uygunluk gönderimle · **K-710 ⚠** IBAN girişi bildirimsiz · **K-711 ⚠** yeniden doğrulama L-1'e · **K-712 ⚠** ulaşma tarihinin düzeltilmesi · **K-713 ⚠** indirme hakkı donar · **K-714 ⚠** sipariş arama · **K-715** iki geri alınamaz onay · **K-716 ⚠** ayıpta derhâl geri ödeme · **K-717** Kargoya verildi/Teslim edilemedi'de son kalemin kapanışı. Geri besleme: Ürün Gereksinimleri **v0.54**, MVP Kapsamı **v0.36**; iki dokümanın dosya sonu dipnotu başlığa hizalandı. Öğrenim adayları 5–7 (PF-42…PF-44). Ön sayım düzeltmesi: etki sütunu `03`'ü gösteren karar 57'dir (Aşama 1'den 5, Aşama 2'den 52).
- **SIRADA — `03`'ün cross-review'ı** (`checklists/document-stage.md` §6; `cross-review` skill'i): girdi yalnız doküman ve şablon (K-431); ardından etki yansıtma (`02` ve `10`'u yeniden tarar — K-652). `03` ✓ döngü TEMİZ olduktan sonra; ardından Aşama 2 kapanışının adımları (K-437).
- **Proje sahibine öneri — avukat teyidi (kalan hukuki risk):** K-491 (teslimden sonraki caymada, taşıyıcı belirtilmediğinde geri ödeme süresinin başlangıcı) ve K-627 (KEP üç tipte zorunlu). Önerilen kapı: ilk gerçek kurulumdan (`10` ÖK-12) önce.
- **Dokümanlar:** Proje Vizyonu (`01`) **v0.32** · Ürün Gereksinimleri (`02`) **v0.54** · MVP Kapsamı (`10`) **v0.36** — üçü ✓ Tamamlandı. Kullanıcı Akışları (`03`) ⏳ **v0.7** (audit ✓, deep review ✓; cross-review bekliyor). Karar kaydı **v0.64**, K-01…K-717. Metodoloji (`00`) **v1.0.4**.
- **Oturum kapanışı:** kural `INSTRUCTIONS §7`'de (K-33, K-38) — her oturum ve kapanış adımı bu bloğu kendi `docs:` PR'ında günceller, alt ajan dahil.
- **Gate durumu:** Implementation başlamadı, faz yok. Doküman: **01, 02, 10 ✓** · 03 ⏳ · 04–09, 11–12 ⬚. Aşama 1 blok planı: **0–9 ✓** — 111 konunun 111'i kapandı. Aşama 2 konu planı: kabul edildi (tracker §8.2–§8.3, 62 konu; K-650…K-652); workshop: altı W konusu kapandı (K-653…K-668); yazım turu: **5/5 oturum — tamamlandı**: AK0, AK1 (K-669…K-677), AK2 (K-678…K-681), AK3–AK6 (K-682…K-688), AK7–AK8 (K-689…K-692), AK9–AK11 (K-693…K-700); kalite döngüsü: audit ✓, deep review ✓ (K-701…K-717), cross-review ⬚.
- **İleriye bağlayıcı kayıtlar:** **K-352** — `B9-05` ölçüm eklerse K-348 ve K-349 yeniden açılır (koşul gerçekleşmedi, K-397) · **K-400** — `B9-10` yalnız sipariş verisinden türetir · **K-420** — yol haritası yalnız `10 §3`'ten beslenir, `01 §7`'den asla · **K-424**'ün üç bağımlılık zinciri (öneri→analitik→çerez bandı · stokta haber ver ve terk edilmiş sepet→İYS onayı · SMS→yeni dış ön koşul; K-473 kargo ve havale otomasyonunu aynı kalıba koydu) · **K-439** — `01 §7`'nin sınırı ile aynı alandaki yol haritası adayı çelişmez: sınır kimliktir, aday özellik · **K-474** — tetikleyici gerçekleşince kargo entegrasyonu ve havale eşleştirmesi öne çekilir.
- **Aşağı dokümanlara devirler:** devir girdilerinin tek tablosu karar kaydının §7'sinde; kararların tam dizini çakışma taramasının §6'sında (`Docs/CHECKPOINT_REPORTS/PHASE1_CONFLICT_SCAN.md`) — kalite döngüsünün 147 kararı `03`–`08`, `12` ve `DEPLOY_RUNBOOK`'a 292 devir; daha önceki devirler tracker'ın etki sütunlarında.
- **Son güncelleme:** 2026-10-04

---

## Task Changelog (son N task)

| Task | Durum | Özet | Commit | PR |
|---|---|---|---|---|
| | | | | |

> Bu tablo şişmeye başladığında (yaklaşık 20 satırı geçince) eski satırlar `MEMORY_ARCHIVE.md`'ye taşınır.

---

## Proje

- `<project_*.md dosyaları buraya tek satır olarak listelenir>`

## Kullanıcı

- `<user_*.md>`

## Çalışma Tercihleri (feedback)

- `<feedback_*.md>`

## Referanslar

- `<reference_*.md>`

---

## Terfi Edenler

> Bir zamanlar hafızada yaşayıp artık L1–L5'te olan kurallar. Buraya yalnız işaretçi yazılır.

| Kural | Terfi ettiği yer | Tarih |
|---|---|---|
| Bir konunun alt parçaları da tek tek sorulur | `INSTRUCTIONS.md` §2 | 2026-08-18 |
| Seçenekler sade dille, somut sonuç üzerinden yazılır | `INSTRUCTIONS.md` §2 | 2026-08-18 |
| Sorular ve seçenekler numaralandırılır | `INSTRUCTIONS.md` §2 | 2026-08-30 |
| Konu kapanışında tamlık taraması | `INSTRUCTIONS.md` §2 | 2026-09-15 |
| Kritik olmayan workshop soruları öneriyle kaydedilir | `00` §C.2 · `INSTRUCTIONS.md` §2 · `GUARDRAILS.md` §6 · `checklists/document-stage.md` §3 | 2026-09-17 |
| PR base'i her zaman `main`'dir | `INSTRUCTIONS.md` §3.2 · `.github/workflows/ci.yml` (base filtresi kaldırıldı) | 2026-09-17 |
| Workshop konusu tek mesajda, numaralı öneri listesi olarak sunulur; kritik maddeler ⚠ ile işaretlenir | `INSTRUCTIONS.md` §2 · `checklists/document-stage.md` §3 | 2026-09-18 |
| Tamlık taraması liste hazırlanırken koşulur; blok sonunda bloğun tamamına genişler | `INSTRUCTIONS.md` §2 · `checklists/document-stage.md` §3 | 2026-09-19 |
| Taslağı yazılmış bölüme dokunan karar `(taslak güncellenecek)` işaretini alır; bloğun son konusunda işaretsiz eşleşme taranır (K-29) | `checklists/document-stage.md` §3 | 2026-09-19 |
| Dokümanlar adıyla anılır; özetler kısa ve sade | `INSTRUCTIONS.md` §2 (PR #37) | 2026-10-02 |
| Toplu onay ve ⚠ öneriyle kayıt — iki mod, işaretleri ve gösterilmeyen kararların listesinin kapısı | `INSTRUCTIONS.md` §2 · `GUARDRAILS.md` §6 · `checklists/document-stage.md` §1, §7 | 2026-10-03 |
| Soru açmadan önce kural katmanları taranır; öncül önce anlatılır | `INSTRUCTIONS.md` §2 | 2026-10-03 |
| CI'da sessizlik yeşil değildir — check-run sayısı doğrulanır | `INSTRUCTIONS.md` §3.2 | 2026-10-03 |
| Her oturum ve kapanış adımı PR'ı Güncel Durum'u taşır (K-38); K numarası tekildir | `INSTRUCTIONS.md` §7 · `checklists/document-stage.md` §3 | 2026-10-03 |
| Yazım tek bağlamda; çok mercekli denetim kalite döngüsünde | `checklists/document-stage.md` §4 · `skills/audit` | 2026-10-03 |
| Aşama 1'in dokuz deseni; döngü yakınsamıyorsa ciddiyet ölçüsü | `00` §N.1, §C.5 | 2026-10-03 |
| Aşama 1'in süreç kararları (K-28, K-30, K-33…K-38, K-429…K-432, K-437) ve Aşama 1 öğrenimleri | `checklists/document-stage.md` §3, §4, §6, §7 · `skills/cross-review`, `audit`, `deep-review` · `scripts/git-hooks/pre-commit` | 2026-10-03 |

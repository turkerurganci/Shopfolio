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

- **AŞAMA 3 AÇILDI — UI/UX tasarım → Arayüz Tanımları (`04`), 2026-10-04.** Rol Senior Product Designer / UX Architect; girdi `02`, `03`, `10` (üçü ✓); düzey wireframe; **izlenebilirlik matrisi zorunlu**. Aşama 1 2026-10-03'te, Aşama 2 2026-10-04'te kapandı (adımları `MEMORY_ARCHIVE.md`'de).
- **Son adım — yazım turunun 3. oturumu (karar kaydı v0.82, `04` v0.10, 2026-10-04; K-797…K-808):** `04 §9.1`–§9.9 yazıldı — panelin ilk dokuz ekranı (E-29…E-37: panel girişi ve şifre sıfırlama, davet kabulü, panel ana sayfası, ürün listesi, ürün formu, kategori ağacı, kuponlar, sipariş listesi, sipariş ayrıntısı); E-37 kırk iki maddedir — birincil düğme hattın sıradaki zorunlu adımı, dört panel işareti, kalemin kayıtları ve adetleri, iade teslim alma ve ret, geri ödeme, ayıp talepleri bölümü, e-posta geçmişi, dokuz müdahale. **Matris:** 364 `03` satırı tam okundu, 793 satır betikle tarandı; on bir satır düzeldi (yedi eksik ikinci ekran, dört yanlış eşleme — E-32'den çıkan silme satırları); 1.193 satır, dağılım değişmedi. **Kararlar:** onay düğmeleri ve cümleleri (K-797) · yasal uyarı metinleri (K-798 ⚠) · engel ve uyarı satırı mesajları (K-799) · eşzamanlı uyarının iki yolu (K-800) · stok kodu (K-801) · panel listeleri ve sipariş listesinin araması, süzgeci, kartı (K-802) · sipariş ayrıntısının kurgusu (K-803) · e-posta geçmişi (K-804) · kategoriler alfabetik (K-805 ⚠) · kupon kodu tekil, kupon silinmez (K-806 ⚠) · davet kabulünden sonra girişe (K-807) · ayıpta sözleşmeden dönmenin panel yolu (K-808 ⚠). **Geri beslendi:** Ürün Gereksinimleri v0.61 (§3.4.1, §3.10.5), Kullanıcı Akışları v0.17 (2.1.2, 2.4.4, 8.1.12, 8.1.14, 1.11.28, 8.3.2.1). Manuel adım bütçesi değişmedi.
- **SIRADA — yazım turunun 4. oturumu:** `04 §9.10`–§9.25 (E-38…E-54: Talepler, iletişim talebi ayrıntısı, üye kaydı görünümü, kurumsal içerik, ana sayfa ve menü, marka, firma kimliği ve satış, ödeme yöntemleri, kargo, süreler ve varsayılanlar, yasal metinler, yönetici hesapları, yöneticinin kendi hesabı, satış özeti, işlem izi, dışa aktarma, yönetici hesabının doğrulama ekranları). Biçim §5 ve §9.1–§9.9'un biçimidir; 4. oturuma kalan devirler karar kaydı §10.1'in devir maddesinde (satış özetinin dönemi ve sıfır payda, onay cümleleri K-662…K-665 ve K-752, E-40'ın onayı — metni K-797'de —, eşzamanlı uyarının iki yolu ayarlarda K-800'le, E-54'ün kalıptan farkları). Ardından 5. oturum §6–§8 → kalite döngüsü → kapanış.
- **Açık süreç maddeleri (karar kaydı §10.1):** ⚠ öneriyle kayıt listesi — **yirmi iki ⚠ karar gösterilecek:** matrisin 2. oturumundan dört (K-732, K-733, K-736, K-737), workshop'tan beş (K-747, K-754, K-756, K-759, K-760), yazım turunun 2a oturumundan üç (K-770, K-771, K-774), 2b oturumundan iki (K-785, K-786), kısmi adet kararından dört (K-788, K-789, K-794, K-796), yazım turunun 3. oturumundan dört (K-798, K-805, K-806, K-808); kapı: en geç Aşama 3'ün arşiv işaretinden önce · Ö-23 — checklist'in skill'e dönüşmesi (kapı: Aşama 3'ün öğrenim terfisi) · öğrenim terfisi adayları (dört aday; kapı: kapanışın 5. adımı) · yazım turunun sonraki oturumlarına kalan devirler (kapı: ekranını yazan oturum). Konu planı karar kaydının §10.2–§10.3'ünde: on blok (`UI0`…`UI9`), 57 konu — 7 W ✓, 4 P ✓, 46 T (otuz dördü ✓, on ikisi yazım turunun kalan oturumlarında — 4, 5); çalışma modları K-723 (⚠ öneriyle kayıt, merge yetkisi, yönetici + alt ajan düzeni — Aşama 3 boyunca); plan kararları K-724…K-727; matris kararları K-728…K-739; workshop kararları K-740…K-761; yazım turunun 1. oturumunun kararları K-762…K-769; 2a oturumunun kararları K-770…K-778; 2b oturumunun kararları K-779…K-786; kısmi adet kararları K-787…K-796; yazım turunun 3. oturumunun kararları K-797…K-808.
- **Dokümanlar:** Proje Vizyonu (`01`) **v0.33** · Ürün Gereksinimleri (`02`) **v0.61** · MVP Kapsamı (`10`) **v0.41** · Kullanıcı Akışları (`03`) **v0.17** — dördü ✓ Tamamlandı (`02` ve `03`'ün son sürümü 3. oturumun geri beslemesi — K-805, K-806, K-808; `01` ve `10`'unki kısmi adet kararının — K-787…K-794) · Arayüz Tanımları (`04`) ⏳ **v0.10** (başlık notu, §1–§5 ve §9.1–§9.9 yazılı; §9.10–§9.25 ve §6–§8 kapı satırıyla — yazım turunun 4. ve 5. oturumları). Karar kaydı **v0.82**, K-01…K-808 (Aşama 1 ve 2'nin kayıtları salt okunur; Aşama 3 K-723'ten, açık kalem A-19'dan). Metodoloji (`00`) **v1.0.4**.
- **Oturum kapanışı:** kural `INSTRUCTIONS §7`'de (K-33, K-38) — her oturum ve kapanış adımı bu bloğu kendi `docs:` PR'ında günceller, alt ajan dahil.
- **Gate durumu:** Implementation başlamadı, faz yok. Doküman: **01, 02, 03, 10 ✓** · **04 ⏳** · 05–09, 11–12 ⬚. Aşama 1 blok planı: **0–9 ✓** (111 konu). Aşama 2 konu planı: **AK0–AK11 ✓** (62 konu; tracker §8.2–§8.3). Aşama 3 konu planı: **UI0, UI1, UI2, UI3, UI4 ✓ · yedi W konusu ✓ · UI5 ✓ (2a, 2b) · kısmi adet kararı ✓ · UI9'un altı T konusu ✓ (3. oturum)**, UI9'un kalan beş T konusu ve UI6…UI8 yazım turunun kalan oturumlarını bekliyor (57 konu; tracker §10.2–§10.3).
- **İleriye bağlayıcı kayıtlar:** **K-352** — `B9-05` ölçüm eklerse K-348 ve K-349 yeniden açılır (koşul gerçekleşmedi, K-397) · **K-400** — `B9-10` yalnız sipariş verisinden türetir · **K-420** — yol haritası yalnız `10 §3`'ten beslenir, `01 §7`'den asla · **K-424**'ün üç bağımlılık zinciri (öneri→analitik→çerez bandı · stokta haber ver ve terk edilmiş sepet→İYS onayı · SMS→yeni dış ön koşul; K-473 kargo ve havale otomasyonunu aynı kalıba koydu) · **K-439** — `01 §7`'nin sınırı ile aynı alandaki yol haritası adayı çelişmez: sınır kimliktir, aday özellik · **K-474** — tetikleyici gerçekleşince kargo entegrasyonu ve havale eşleştirmesi öne çekilir.
- **Aşağı dokümanlara devirler:** devir girdilerinin tek tablosu — Aşama 2'ye karar kaydının §7'sinde, **Aşama 3'e §9'unda**; Aşama 1 kararlarının tam dizini çakışma taramasının §6'sında (`Docs/CHECKPOINT_REPORTS/PHASE1_CONFLICT_SCAN.md`) — kalite döngüsünün 147 kararı `03`–`08`, `12` ve `DEPLOY_RUNBOOK`'a 292 devir; daha önceki devirler tracker'ın etki sütunlarında. **Aşama 2'nin dizini:** `Docs/CHECKPOINT_REPORTS/PHASE2_CONFLICT_SCAN.md` §6 — 63 karar, 137 atıf, adsız 86 atfın işinin adı orada; hedef dokümanların park bloğu dizine işaret eder.
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
| Devir notunun sorusu da katmanlara karşı taranır; açık maddeleri yeni aşamanın listesine geçer | `INSTRUCTIONS.md` §2 · `checklists/document-stage.md` §1 | 2026-10-04 |
| Aşamalar da adıyla anılır | `INSTRUCTIONS.md` §2 | 2026-10-04 |
| Merge yetkisi ve yönetici düzeni — biçimi (yetkinin kendisi aşamayla sınırlı, K-648) | `INSTRUCTIONS.md` §9 · `GUARDRAILS.md` §3, §6 | 2026-10-04 |
| Güncel Durum değiştirilir, eklenmez; önceki adım arşive | `INSTRUCTIONS.md` §7 · `memory/MEMORY_ARCHIVE.md` | 2026-10-04 |
| Aşama 2'nin süreç kararları (K-652, K-670, K-682, K-718) ve öğrenimleri | `checklists/document-stage.md` §1, §3, §4, §7 · `skills/audit`, `checkpoint`, `cross-review` | 2026-10-04 |

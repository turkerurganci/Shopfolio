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
- **Son adım — Aşama 2'nin öğrenim terfisi (karar kaydı v0.70, 2026-10-04):** K-437'nin 5. adımı; rapor `Docs/CHECKPOINT_REPORTS/PHASE2_LEARNING_PROMOTION.md`, karar **K-719** (öneriyle kaydedildi). Otuz dört aday — tracker §8.1'in on ikisi, karar kaydında yaşayan süreç kararları (yedi satır), bu adımda bulunan dört aday, kullanıcı hafızasının on bir notu. **L3–L4 terfileri bu PR'da:** checklist §1 (devir notu kurala karşı okunur, açık maddeleri yeni listeye geçer · şablon taraması · aşamaya özgü konu öneki), §3 (sonraki dokümana giden etki atfı işin adını taşır · park satırına geri işaret · geri besleme maddesi K-652 — kuralın anahtar terimi ve Kaynak satırı betikle), §4 (çok oturumlu yazımda alt bölüm haritası ve geçici bağ · devir taraması gövde cümlelerini okur), §7 (park satırı taraması K numarasına dayanır); `INSTRUCTIONS §2` (aşamalar adıyla · devir notunun sorusu da taranır), §7 (Güncel Durum değiştirilir, eklenmez — arşiv `MEMORY_ARCHIVE.md`), §9 (merge yetkisi ve yönetici düzeni); `GUARDRAILS §3`, §6; `audit` (betik örneklenir · beşli dalga, bulgu anında dosyaya, ön plan), `checkpoint`, `cross-review` (ölçünün ikinci koşulu akış dokümanını kapsar). **Ö-23:** checklist şimdi skill'e dönüşmez — §2 (izlenebilirlik) hiç işletilmedi ve checklist bu kapanışta da yapısal değişiklik aldı; kapı Aşama 3'ün öğrenim terfisi. **`00`'a iki öneri** (§N.1'e Aşama 2'nin altı deseni · §C.7'nin skill ölçütü) proje sahibinin onayını bekliyor (rapor §4). Playbook listesi 53 satır. Aşama 2'nin önceki adımları (açılış → checkpoint) `MEMORY_ARCHIVE.md`'de.
- **SIRADA — proje sahibine tek mesaj:** otuz dokuz ⚠ karar (CP02 §5.1), avukat teyidi önerisi (CP02 §5.2) ve `00`'ın iki önerisi (öğrenim raporu §4; tracker §8.1); ardından arşiv işareti ve Aşama 3'e devir (K-437'nin 6. adımı; `03` ✓ orada; devir notu Ö-23'ün kapısını ve `00` önerilerinin sonucunu taşır).
- **Proje sahibine öneri — avukat teyidi (kalan hukuki risk):** K-491 (teslimden sonraki caymada, taşıyıcı belirtilmediğinde geri ödeme süresinin başlangıcı) ve K-627 (KEP üç tipte zorunlu). Kapı tracker §8.1'de (çakışma taraması): ⚠ listesiyle aynı mesajda, Aşama 2'nin arşiv işaretinden önce proje sahibine sunulur; kabul edilirse `DEFERRED_BACKLOG.md`'ye girer, hedef ilk gerçek kurulumdan (`10` ÖK-12) önce.
- **Dokümanlar:** Proje Vizyonu (`01`) **v0.32** · Ürün Gereksinimleri (`02`) **v0.55** · MVP Kapsamı (`10`) **v0.37** — üçü ✓ Tamamlandı. Kullanıcı Akışları (`03`) ⏳ **v0.11** (audit ✓, deep review ✓, cross-review ✓ 3 turda TEMİZ, etki yansıtma ✓, checkpoint ✓). Karar kaydı **v0.70**, K-01…K-719. Metodoloji (`00`) **v1.0.4**.
- **Oturum kapanışı:** kural `INSTRUCTIONS §7`'de (K-33, K-38) — her oturum ve kapanış adımı bu bloğu kendi `docs:` PR'ında günceller, alt ajan dahil.
- **Gate durumu:** Implementation başlamadı, faz yok. Doküman: **01, 02, 10 ✓** · 03 ⏳ · 04–09, 11–12 ⬚. Aşama 1 blok planı: **0–9 ✓** — 111 konunun 111'i kapandı. Aşama 2 konu planı: kabul edildi (tracker §8.2–§8.3, 62 konu; K-650…K-652); workshop: altı W konusu kapandı (K-653…K-668); yazım turu: **5/5 oturum — tamamlandı**: AK0, AK1 (K-669…K-677), AK2 (K-678…K-681), AK3–AK6 (K-682…K-688), AK7–AK8 (K-689…K-692), AK9–AK11 (K-693…K-700); kalite döngüsü: audit ✓, deep review ✓ (K-701…K-717), cross-review ✓ 3 turda TEMİZ (K-718), etki yansıtma ✓; kapanış: çakışma taraması ✓ (K-437'nin 3. adımı), checkpoint ✓ (4. adım, CP02), öğrenim terfisi ✓ (5. adım, K-719).
- **İleriye bağlayıcı kayıtlar:** **K-352** — `B9-05` ölçüm eklerse K-348 ve K-349 yeniden açılır (koşul gerçekleşmedi, K-397) · **K-400** — `B9-10` yalnız sipariş verisinden türetir · **K-420** — yol haritası yalnız `10 §3`'ten beslenir, `01 §7`'den asla · **K-424**'ün üç bağımlılık zinciri (öneri→analitik→çerez bandı · stokta haber ver ve terk edilmiş sepet→İYS onayı · SMS→yeni dış ön koşul; K-473 kargo ve havale otomasyonunu aynı kalıba koydu) · **K-439** — `01 §7`'nin sınırı ile aynı alandaki yol haritası adayı çelişmez: sınır kimliktir, aday özellik · **K-474** — tetikleyici gerçekleşince kargo entegrasyonu ve havale eşleştirmesi öne çekilir.
- **Aşağı dokümanlara devirler:** devir girdilerinin tek tablosu karar kaydının §7'sinde; kararların tam dizini çakışma taramasının §6'sında (`Docs/CHECKPOINT_REPORTS/PHASE1_CONFLICT_SCAN.md`) — kalite döngüsünün 147 kararı `03`–`08`, `12` ve `DEPLOY_RUNBOOK`'a 292 devir; daha önceki devirler tracker'ın etki sütunlarında. **Aşama 2'nin dizini:** `Docs/CHECKPOINT_REPORTS/PHASE2_CONFLICT_SCAN.md` §6 — 63 karar, 137 atıf, adsız 86 atfın işinin adı orada; hedef dokümanların park bloğu dizine işaret eder.
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

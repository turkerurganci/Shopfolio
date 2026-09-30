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

- **Son tamamlanan:** **Blok 9 ✓ (2026-09-30) — Aşama 1'in workshop turu bitti.** Tek oturumda, **elli dört karar** (K-385…K-438). On sekiz konu; `B9-01` (kapsam listesi) içeriği `B9-02`…`B9-14`'ün toplamı olduğu için **sona alındı** (K-435, konu kimliği K-32 gereği değişmedi). Başlıklar: kapsam dışı aday paket (yorum, istek listesi, öneri, canlı destek — hepsi yok) · mobil uygulama ve PWA yok, tek site duyarlı tasarım · tarayıcı tabanı son iki sürüm, erişilebilirlikte **hedef AA ama beyan yok**, yedi zorunlu kural · **ziyaretçi analitiği yok** (K-352'nin koşulu gerçekleşmedi, çerez bandı gelmedi), panelde satış özeti var · manuel adım envanteri ve **bütçe kuralı: sipariş başına üçü geçmez** · günlük dayanak ortalama 10, tepe 50 · uptime taahhüdü yok, veri kaybı toleransı sipariş tarafında sıfır, **veri firmanındır** · parametrelerde **üç katman + atama ölçütü** · iki eksenli başarı kriterleri, **kabul kapısı yalnız ürün düzeyi** · on kalıcı sınır ve yedi varsayım · on altı post-MVP adayı, üç sıralı ölçüt, üç bağımlılık zinciri · sipariş dışa aktarma **var**, içe aktarma ve toplu güncelleme yok · kalite döngüsü doküman başına, `01`→`02`→`10`, kilitli sıra · aşama kapanışı altı adım. **Tarama on dört boşluk buldu** (on üçü konu öncesi, biri blok kapanışı). **K-29 taraması: 54 satır, kaçak sıfır.**
- **Toplu onay gözden geçirildi (2026-10-01) — yirmi iki karar, itiraz sıfır.** Blok 8'in on iki ve Blok 9'un on kararı *"sonuna kadar git"* onayıyla ⚠ grubuna girdiği hâlde **gösterilmeden** kaydedilmişti; oturum başındaki durum sorusunda tek listede (karar başına bir cümle) gösterildi, proje sahibi *"tamam"* dedi. İşaretler silinmedi, `(toplu onayla kaydedildi — gözden geçirildi 2026-10-01, itiraz yok)` oldu. Gözden geçirme bir kapıya bağlı değildi ve iki blok kapanışını da kaydı olmadan geçti — `B9-17`'nin **birinci terfi adayına** (K-438) kanıt olarak eklendi. K-438'in "dokuz karar" sayımı ona düzeltildi. Ayrıntı: tracker §6.1.
- **Önceki:** Blok 8 ✓ (2026-09-26) — doksan yedi karar (K-288…K-384); A-06…A-10 orada kapandı. `02 §2, §3, §5, §6, §7` taslak yazımı ✓ (2026-09-19, `02` v0.7). Ayrıntı: tracker §6.1.
- **SIRADA — yazım turu (K-432, K-437'nin 1. adımı).** Blok 9 kapandığı için K-28'in kapısı açıldı. Tek bir turda, **K-30 gereği workshop'tan ayrı oturumlarda**, girdi karar kaydı: (1) **K-29 güncellemeleri** — `01 §1–§5` (dört satır: K-417, K-419, K-421, K-433) ve `02 §1–§3, §5–§7` (Blok 8'den 95, Blok 9'dan 50 satır); (2) **ilk yazım** — `01 §6–§8` · `02 §4, §8–§13` · `10 §1–§5`. Sözlüğe Blok 8'den altı terim girecek (`ContactRequest`, `ContactRequestStatus`, `Closed`, `AdminInvitation`, `AuditLog`, `Notification` — altısı da karar anında İngilizce karşılığıyla yazıldı). **A-05'in vadesi bu turdadır.** İşaretler turda `(taslak güncellendi — vX.Y)` olur. **İlk oturum `01`** (proje sahibi 2026-10-01'de onayladı): dört K-29 satırı + `01 §6–§8`; A-02, A-03, A-04 orada kapanır.
- **Sonra:** kalite döngüsü (K-430, K-431) — `01` → `02` → `10`, doküman başına bir kez, **biri TEMİZ olmadan sonrakine geçilmez**; audit ve deep-review **çok mercekli** koşulur ve bulgular karşı-doğrulanır, cross-review `cursor-agent` ile. Ardından çakışma taraması (K-429), checkpoint, öğrenim terfisi, arşiv işareti (K-436) ve Aşama 2'ye devir — sıra K-437'de kilitli.
- **Taslaklar:** `01` **v0.5** (§1–§5) · `02` **v0.7** (§1–§3, §5–§7) · `10` henüz yazılmadı.
- **Yöntem (`INSTRUCTIONS §2`):** konu başına tek mesaj, numaralı öneri listesi, ⚠ işaretli üç grup; tamlık taraması liste hazırlanırken koşar, blok sonunda bloğun tamamına genişler. Blok 8 ve 9'un ikisi de tek oturumda kapandı, itiraz sıfır.
- **İleriye bağlayıcı üç kayıt:** **K-352** — `B9-05` ölçüm eklerse K-348 ve K-349 yeniden açılır (koşul gerçekleşmedi, K-397) · **K-400** — `B9-10` yalnız sipariş verisinden türetir · **K-420** — `B9-13` yalnız `10 §3`'ten beslenir, `01 §7`'den asla. Bunlara **K-424**'ün üç bağımlılık zinciri eklendi (öneri→analitik→çerez bandı · stokta haber ver ve terk edilmiş sepet→İYS onayı · SMS→yeni dış ön koşul).
- **Aşağı dokümanlara devirler:** `04`–`09`, `12` ve `DEPLOY_RUNBOOK` satırları tracker'ın etki sütunlarındadır. **Blok 9'un ekledikleri:** `04`'e satış özeti ekranı (K-398), "bekleyen işlerim" sayaç bloğu (K-405), sipariş dışa aktarma (K-426) · `05`'e genel istek hızı (K-331), yedekleme ve kurtarma (K-407), erişim kayıtları (K-397) · `10 §4`'e üç dış ön koşul zaten vardı, `10 §5` yol haritasını alır (K-422) · `DEPLOY_RUNBOOK`'a yedek erişimi ve çıkış hakkı (K-427).
- **Açık kararlar:** **A-02, A-03, A-04** — `01` yazım oturumunun boşlukları; vadesi `01 §6–§8` yazımı, yani **sıradaki turda**. A-04'ün yönü K-433 ile belirlendi (`01 §1`'in sayısal iddiası ürün düzeyi eksenine bağlanır; satış hacmi iddiası yapılamaz — K-417). **A-05** — sözlükte 36 türetilmiş İngilizce karşılık; vadesi `02`'nin son yazım oturumu. **A-06…A-10 kapandı** (2026-09-26). Ayrıntı: tracker §4.
- **Gate durumu:** Implementation başlamadı, faz yok. Doküman: **01 ⏳ (v0.5)** · **02 ⏳ (v0.7)** · 10 ⬚ · 03–09, 11–12 ⬚. Blok planı: **0–9 ✓ — tamamı kapandı.** Plandaki **111 konunun 111'i** kapandı; 2026-08-17'den bu yana **438 karar**.
- **Oturum kapanışı (K-38):** doküman döneminde bu blok `/handoff` beklenmeden oturum sonu `docs:` PR'ının içinde güncellenir. Üç kez kaçırıldı (Blok 3 oturum 1 ve 4, Blok 5 oturum 1), üçünü de INSTRUCTIONS §3.0 yakaladı. Blok 6, 7, `02` yazım oturumu, Blok 8 ve **Blok 9** kurala uygun kapandı.
- **Son güncelleme:** 2026-10-01

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

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

- **Son tamamlanan:** **Blok 8 ✓ (2026-09-26)** — tek oturumda, **doksan yedi karar** (K-288…K-384). Oturum A-06…A-10'u kapatarak açıldı (K-288…K-299), sonra `B8-01`…`B8-16` sırayla işlendi. Başlıklar: iletişim/talep formu ve iki durumu · yönetim yetki modeli, davetle hesap açma, değiştirilemez işlem izi, yönetici vitrinde müşteri olamaz · bildirim matrisi (müşteriye dokuz, firmaya dört), sözleşme metinleri onay e-postasında · pazarlama iletisi ve İYS kapsam dışı · yedi kötüye kullanım limiti · saat dilimi, iş günü, süre sayımı · KVKK aydınlatma sürümlü metin, **açık rıza hiç toplanmaz** · yalnız zorunlu çerez, **onay bandı yok** · sekiz satırlık saklama tablosu ve otomatik imha · yurt dışına aktarım yalnız kurulumdan doğar · dört yasal metnin sürümlenmesi, geçmişe yeniden onay yok · uyuşmazlık bilgisi sözleşmede, rakamsız · altı yönetici müdahalesi, **elle sipariş yok** · e-posta gönderen kimliği ve yanıtlanabilir adres · veri ihlalinde ayrı modül yok. **Tarama on yedi boşluk buldu** — on beşi konu sunulmadan önce, ikisi blok kapanışında; hiçbiri yeni konu kimliği açmadı, plan **111 konu**. **K-29 taraması mekanik koşuldu: 95 satır, kaçak sıfır.**
- **⚠ TOPLU ONAY — gözden geçirilecek:** proje sahibi `B8-06`'dan sonra *"sonuna kadar git, hiçbirine itirazım yok"* dedi; kalan konuların ⚠ maddeleri proje sahibine **gösterilmeden** kaydedildi ve `(toplu onayla kaydedildi)` işaretini taşıyor — **K-341 · K-345 · K-346 · K-349 · K-353 · K-358 · K-363 · K-366 · K-369 · K-370 · K-377 · K-380**. Gerekçe ve ölçüm: tracker §6.1.
- **Önceki:** `02 §2, §3, §5, §6, §7` taslak yazımı ✓ (2026-09-19, `02` **v0.7**) — beş açık buldu (A-06…A-10), beşi de Blok 8'de kapandı. Blok 7 ✓ (2026-09-19, K-237…K-287) · Blok 6 ✓ (2026-09-18, K-159…K-236). Ayrıntı: tracker §6.1 ve §6.2.
- **Yöntem (`INSTRUCTIONS §2`):** konu başına tek mesaj — bütün kararlar numaralı öneri listesi, sorulmaya devam eden üç grup ⚠ ile işaretli, proje sahibi yalnız itiraz eder; tamlık taraması **liste hazırlanırken** koşar, bloğun son konusunda bloğun tamamına genişler. Blok 8'de on altı konu bir oturumda kapandı, itiraz sıfır.
- **Taslaklar:** `01` **v0.5** (§1–§5) · `02` **v0.7** (§1–§3, §5–§7) · `10` henüz yazılmadı.
- **AÇIK KAPI — Blok 8'in taslak güncellemesi (K-29) yapılmadı.** Doksan beş kararın `02 §1, §2, §3, §5, §6, §7`'ye yansıması bekliyor; sözlüğe altı yeni terim girecek — `ContactRequest` · `ContactRequestStatus` · `Closed` · `AdminInvitation` · `AuditLog` · `Notification`; altısının da İngilizce karşılığı K-17 gereği karar anında yazıldı, **A-05 büyümedi**. Hacim inkremental bir düzeltme değil **yazım işidir** → K-30 gereği ayrı oturum. `checklists/document-stage.md` §3'ün *"taslak aynı bloğun PR'ında güncellenir"* cümlesiyle K-30 arasındaki gerilim tracker §6.1'e kaydedildi; çözümü Aşama 1 kapanışında (`00 §K`).
- **Sırada:** **Blok 9** — MVP kapsam kapanışı, parametreler ve aşama kapanışı; **on sekiz konu**, tahmin iki oturum. Hedef doküman: `10 §1–§5` · `01 §6–§8` · `02 §3, §10, §11, §13`. `B9-15` bu dosyanın değil **tracker §4**'ün çakışma taramasını yapar; `B9-17` aşama kapanışı ve öğrenim terfisi. **Blok 9 açılmadan önce Blok 8'in taslak kapısı değerlendirilir.**
- **Açık devirler (Blok 9'a):** `B9-01` (ilk kurulum rehberi — K-271; yasal metin kapısı — K-365; aktarım beyanı — K-360) · `B9-02` (ürün yorumu, istek listesi, canlı destek — K-249, K-256; **stokta haber ver — K-327**) · `B9-04` (erişilebilirlik — K-93, K-252, K-262, K-267) · `B9-05` (**bağlayıcı: ölçüm eklenirse K-348 ve K-349 yeniden açılır — K-352**) · `B9-06` (manuel adımlar: havale işareti — K-159 · hizmet tamamlama — K-175 · iadenin stoğa eklenmesi — K-215 · **havale iadesi — K-291** · **teslim işareti — K-288**) · `B9-07` (çok kalemli sepet — K-154) · `B9-08` (**Blok 8'de genişledi:** yedi limitin sayısı ve penceresi — K-328, K-330, K-334 · davet bağlantısı ömrü — K-308 · imha aralığı — K-354 · e-posta yeniden deneme — K-321 · süre çitleri — K-339 · iletişim talebi ve bildirim saklama süreleri — K-353; **yasal süreler envantere girmez**) · `B9-13` (post-MVP: kapıda ödeme — K-160 · e-Arşiv — K-217 · fatura yükleme — K-218 · **SMS ve bildirim merkezi — K-315** · **pazarlama ve İYS — K-323** · **elle sipariş — K-370** · **rol modeli — K-383**) · `B9-14` (toplu içe aktarma — K-88 · sipariş dışa aktarma — K-217 · **ihlal bildiriminde adres listesi — K-382**).
- **Aşağı dokümanlara devirler:** `04`–`08`, `12` ve `DEPLOY_RUNBOOK` satırları tracker'ın etki sütunlarındadır. **Blok 8'in eklediği başlıklar:** `04`'e panel ekranları (işlem izi listesi, yönetici listesi ve davet, iletişim talebi listesi, iade adresi ayarı, iç not, müdahale işlemleri, yasal metin düzenleme) · `06`'ya altı yeni varlık · `05`'e genel istek hızı sınırı (K-331), zamanlanmış imha işi (K-354), limit mekanizması (K-328) · `10 §4`'e **üç dış ön koşul** (e-posta kurulumu — K-378 · barındırma ve e-posta altyapısının konumu — K-358 · yasal metinlerin doldurulması — K-365).
- **Açık kararlar:** **A-02, A-03, A-04** — `01` yazım oturumunun boşlukları (A-03'ün kapsamı dört satır, K-100); vadesi `01 §6–§8` yazımı, Blok 9 kapandıktan sonra. **A-05** — sözlükte **36 türetilmiş** İngilizce karşılık (78 terimin 42'si kayıtta); vadesi `02`'nin son yazım oturumu, `03`/`04`'ten önce. **A-06…A-10 kapandı** (2026-09-26 — K-288…K-299). Ayrıntı: tracker §4.
- **Gate durumu:** Implementation başlamadı, faz yok. Doküman: **01 ⏳ (v0.5)** · **02 ⏳ (v0.7)** · 10 ⏳ · 03–09, 11–12 ⬚. Blok planı: **0–8 ✓** · 9 ⬚. Plandaki 111 konunun **93'ü** kapandı; kalan **18 konu** (Blok 9).
- **Oturum kapanışı (K-38):** doküman döneminde bu blok `/handoff` beklenmeden oturum sonu `docs:` PR'ının içinde güncellenir. Üç kez kaçırıldı (Blok 3 oturum 1 ve 4, Blok 5 oturum 1), üçünü de INSTRUCTIONS §3.0 yakaladı. Blok 6, Blok 7, `02` yazım oturumu ve **Blok 8** kurala uygun kapandı.
- **Son güncelleme:** 2026-09-26

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

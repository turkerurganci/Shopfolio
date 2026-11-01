# Aşama 9 — Öğrenim Terfisi

**Tarih:** 2026-11-01 | **Aşama:** 9 — Implementation Planı | **Kapanış adımı:** K-437'nin 5. adımı (`00 §K`; `checklists/document-stage.md` §7 madde 5)
**Girdi:** karar kaydı v0.287 — Aşama 9'un süreç kararları (K-1401…K-1500), [çakışma taraması](PHASE9_CONFLICT_SCAN.md) §7, [checkpoint](CP09_PHASE9_CHECKPOINT.md) §6; yönetici yardımcının ilerleme notu (94 giriş) ve iş listesi; `Docs/PLAYBOOK_FEEDBACK.md` (PF-143…PF-146); kullanıcı hafızasının on sekiz notu (indeks satırlarından, salt okunur); oturumların kayıt dosyalarından ölçülen belirteç tüketimi (`scripts/metrics/token-usage.js`) ve hook günlüğü — `origin/main` @ `925bbd6`
**Çıktı:** karar kaydı v0.288 · `.claude/INSTRUCTIONS.md` §2, §9 · `.claude/skills/audit/SKILL.md` · `.claude/skills/checkpoint/SKILL.md` · `.claude/checklists/document-stage.md` §3 · `scripts/doc-scans/geri_isaret_tara.py`, `11_geri_yon.py` · `scripts/git-hooks/pre-commit`, `README.md` · `.claude/memory/MEMORY.md` · `Docs/PLAYBOOK_FEEDBACK.md` 152 satır · karar K-1501, K-1502 — `00`, `CLAUDE.md`, `SETUP.md`, `GUARDRAILS.md`, `CONTEXT.md` ve öteki dokümanlar değişmedi

> **Ne işe yarar:** `00 §K` gereği aşama, öğrenimi yazılmadan kapanmaz. Bu rapor Aşama 9 boyunca kayda, raporlara, yönetici yardımcının gözlemlerine ve hafızaya düşen her öğrenim adayının sonucunu yazar. On iki aday L3–L5'e terfi etti ve bu PR'da uygulandı. Aşama 9'un maliyet ölçümü ve yönetici yardımcı düzeninin eski düzenle karşılaştırması §3'te; checklist'in skill'e dönüşmesi §4'te; proje sahibinin cevabını bekleyen tek soru §5'te — ayrı numaralı, sade dille, K numarasız. Toplu onay (K-1472) açık olduğu için öğrenim sorularından `00`, `CLAUDE.md` ve `SETUP.md` dışında kalanlar önerimle cevaplandı (§5.1, "toplu onayla kaydedildi"); yalnız `00 §N.1` önerisi (Soru 1) uygulanmadı (GUARDRAILS §2).

---

## 1. Sonuç

| Sonuç | Sayı | Adaylar |
|---|---|---|
| **Terfi etti — bu PR'da uygulandı** (L3, L4, L5) | 12 | Ö-161, Ö-162, Ö-164, Ö-165, Ö-166, Ö-167, Ö-168, Ö-169, Ö-170, Ö-171, Ö-172, Ö-173 |
| **Önceden terfi etmişti** | 2 | Ö-160 (yardımcı düzeni), Ö-163 (ikinci model betiği) |
| **Raporda kalır** — tek seferlik gözlem, bilgi ya da proje dışı öneri | 11 | Ö-174 … Ö-184 |
| **Hafızada kalır** | 1 | Ö-185 |
| **Yeniden değerlendirildi, sorulmaz** | 1 | Ö-23 (checklist beceriye dönüşsün mü; K-1399 gereği artık sorulmaz) |
| **Proje sahibine sorulur** | 1 soru | `00 §N.1`'e Aşama 9'un dersleri (Soru 1) |
| **Toplu onayla kaydedilen kararlar** | 3 | Karar A, B, C (§5.1) |
| **Önceden terfi etmişti ya da dokümanda yaşar** — süreç kararları | 7 grup | S-1…S-7 (§2.4) |
| **Hafızadaki yeni notlar** | 3 not | H-1…H-3 (§2.5) |

Toplam otuz yedi kalem: karar kaydının, çakışma taramasının ve checkpoint'in adayları (Ö-163…Ö-168 ve Ö-169'un kaynağı), yönetici yardımcının gözlemleri (Ö-160…Ö-185'in kalanı), Ö-23, Aşama 9'un süreç kararlarının yedi grubu ve kullanıcı hafızasının üç yeni notu. Aşama 8 raporunda tartılan on beş hafıza notu değişmedi. `CLAUDE.md` ve `SETUP.md` için öneri yok.

**Aşamanın asıl öğrenimi.** Implementation Planı (`11`, ~750 KB, 214 task, 1.501 birimlik matris) otuz saat sürdü ve yönetici işi ilk kez ana oturumdan ayrılıp sekiz ardışık yardımcıda yürüdü. Üç şey belirgin: (1) **Yardımcı düzeni tuttu ve yönetici payını yarıya indirdi** — yönetici işi (ana oturum + yardımcıların kendi bağlamı) toplamın yüzde 9,3'ü (Aşama 8'de yüzde 18); hiçbir yardımcı 400 bin sınırına ulaşmadı (en yüksek 338 bin); ama toplam tüketim Aşama 8'in 4,4 katına çıktı, çünkü doküman büyüktü ve kalite döngüsü ile checkpoint toplamın yarısından fazlasını harcadı (§3). (2) **Kontrol ajanlarının işi küçük ve yazılı kapsamla yürüdü, büyük dilimlerde yürümedi**: 80 KB'lık dilimler, 140 satırlık matris bölümleri ve 598 satırlık bir kapsam notu paketi bir Sonnet / `low` merceğin bağlamına sığmadı ve checkpoint altı parçaya bölündü (Ö-164, Ö-165). (3) **Toplu onay ve sınırlar yetki alanını genişletme izni vermedi**: bir işçi kalan kapsam açıklarını "başka kapıda kapanır" diye kapattı, bir işçi 20 doğrulayıcı sınırını parti başına okudu ve 61 doğrulayıcı koştu (Ö-161, Ö-162).

---

## 2. Adaylar ve sonuçları

**Kimlikler:** Ö-160 sonrası kimlikler Aşama 8'in Ö-159'undan sürer. Ö-163…Ö-168 ve Ö-169 karar kaydının, çakışma taramasının ve checkpoint'in adaylarıdır (CP09 §6'nın yedi adayı ve gözlemi, PHASE9_CONFLICT_SCAN §7'nin iki adayı). Ö-160, Ö-161, Ö-162 ve Ö-170…Ö-185 yönetici yardımcının ilerleme notundan, adımların işçi raporlarından ve hook günlüğünden çıkarıldı. Ö-23 Aşama 1'den gelir. S-1…S-7 Aşama 9'un süreç kararlarının, H-1…H-3 kullanıcı hafızasının yeni notlarının bu rapora özgü numaralarıdır.

### 2.1 Karar kaydı, çakışma taraması ve checkpoint adayları

| # | Kaynak | Aday | Sınıf (`00 §K`) | Sonuç | Hedef katman ve dosya | Uygulanan metin (özet) | Playbook |
|---|---|---|---|---|---|---|---|
| Ö-163 | K-1484 | Cross-review'ın ikinci modeli: sıra, sağlık çağrısı, tur bölünmez, Claude olamaz, tek betik | İhlali önleyen kural | **Önceden terfi etmişti** (K-1484) | L4 `skills/cross-review` Faz 1 madde 1 · L5 `scripts/doc-scans/cross_review_ikinci_model.sh` · `SETUP.md` §7 | Kural ve betik aynı PR'da yazıldı; 6. turdan sonra turlar betikle koştu ve ilk denemede çıkış 0 verdi (5. tur betikten önce koştu; 10. turun ilk denemesi kullanım sınırına takıldı ve tur baştan koşuldu, kısmi çıktı kullanılmadı) | PF-142'nin devamı (yeni satır gerekmez) |
| Ö-164 | CP09 §6 #4, #5, #7 | Sonnet / `low` mercek 80 KB'lık dilimde, 140 satırlık matris bölümünde ve 598 satırlık kapsam notu paketinde kapsamı bitiremedi; ≤ 60 KB paketler bitirdi | İhlali önleyen kural | **Terfi etti** | L4 `skills/checkpoint` "Koşum" | Dilim sınırı (~60 KB, 8–12 task); matris dilimi task başına ya da betikle kapsama; kapsam notu paketi açık kalan kaynak bölümlere ve ~12 task'a indirilir | PF-148 |
| Ö-165 | CP09 §5.1, K-1497 | Mercek kapsamı bırakınca açık "doğrulanmayanlar" listesine düştü; işçi kalan açıkları "`validate`'te kapanır" diye başka kapıya devretti | İhlali önleyen kural | **Terfi etti** | L4 `skills/checkpoint` "Koşum" | Her mercek hangi dilimleri bitirdiğini bir satırda yazar; yazmayan dilim bitmemiştir; kapsam açığı başka kapıya devredilmez | PF-148 |
| Ö-166 | CP09 §6 #1 | Doğrulayıcı talimatı hüküm ve ESKİ / YENİ biçimini zorunlu kılmazsa hükümler serbest yazılır; yeniden biçimlendirme için beş Sonnet ajanı gerekti | İhlali önleyen kural | **Terfi etti** | L4 `skills/audit` "Koşum biçimi" 2. madde | Hüküm ve düzeltme betiğin okuduğu sabit biçimde (`## #N \| hukum: …`, `ESKI:`, `YENI:`) istenir | PF-149 |
| Ö-167 | CP09 §6 #2, #6 | Aynı bulgu iki dilimden gelince (05-13 ≡ D9-01; T515, T520) iki doğrulayıcı aynı yeri ayrı şartnameyle düzeltti; Aşama 8'de aynısı üç çiftte oldu (tekrar) | Tekrarlanacak desen → kural | **Terfi etti** (kural); betik değişikliği yapılmadı (§6) | L4 `skills/audit` "Koşum biçimi" 2. madde | Paketleyici doğrulamadan önce aynı yer alıntısını ve aynı task'a dokunan bulguları eşleyip tek pakette toplar | PF-149 |
| Ö-168 | K-1459…K-1469 söz kayıpları, K-1487 → K-1491 | Bayt sınırı için elle kısaltmada doğrulanmış şartnamenin eki düştü (T370 KK7 "IBAN silinir", T374 AC4, T338 AC6, T527 AC6) | İhlali önleyen kural | **Terfi etti** | L4 `skills/audit` "Koşum biçimi" 2. madde | Elle kısaltılan blok, doğrulanmış şartnamenin her parçasıyla karşılaştırılır; düşen parça geri konur ya da kaybı kayda yazılır | PF-149 |
| Ö-169 | PHASE9_CONFLICT_SCAN §7 madde 1 | `geri_isaret_tara.py` "X'in lafzını daraltır / tamamlar" kalıbını ilişki sözcüğü penceresinin dışında bırakıyor: varsayılan geçiş 19 çiftte 8 eksik buldu (7'si çürük); asıl 5 geri işaret (K-1406, K-1410, K-1407, K-508, K-509) 359 çiftlik ikinci geçişte çıktı | İhlali önleyen kural | **Terfi etti** | L5 `scripts/doc-scans/geri_isaret_tara.py` (`--tum`) · L4 checklist §3 geri işaret maddesi | `--tum`: ilişki sözcüğü ve "elenen" süzgeci kalkar, her eski K anması aday olur, ilişki sözcüğü yalnız sıralar; çakışma taraması ikinci geçişi `--tum` ile koşar | PF-150 |

### 2.2 Yönetici yardımcının ve işçilerin gözlemleri

| # | Gözlem | Sonuç |
|---|---|---|
| Ö-160 | **Yönetici yardımcı düzeni.** Ana oturum yalnız aracı; yönetici işi `[yonetici]` Opus alt ajanında. Beş deneme ve yedi bağımsız doğrulama turuyla açıldı (K-1402); Aşama 9'da sekiz yardımcı (açılış dahil) ardışık çalıştı (açılış, 2, 3, 4, 5, 6 cursor-agent, 6 Codex, 7). Hiçbiri 400 bin sınırına ulaşmadı (en yüksek 338 bin); yardımcı 4 durma kaydı yazamadan düştü (ana oturum kaydı yeniden yazdı, işçi dalı ve PR doğru yerden sürdü); öteki devirler temiz. Ölçüm §3'te | **Önceden terfi etmişti** (`INSTRUCTIONS §9` "Yönetici yardımcı düzeni", K-1402; tracker §22.1 "Yardımcı düzeninin ölçümü" maddesi bu rapor ve K-1502 ile kapanır). Aşama 10 açılışında düğmenin açık kalması önerisi §3.4'tedir |
| Ö-161 | **20 doğrulayıcı sınırının "parti başına" okunması.** Özet ↔ ayrıntı taramasının işçisi sınırı parti başına okudu, 61 Opus doğrulayıcı koştu (merge denetimi UYARI; adım 109 M ile oturumun en pahalısı). Sonraki görev dosyaları sınırı "adımın toplamı" diye yazdı; hiçbir adım 20'yi aşmadı (2b'de reddedilen iki deneme de sayıldı: 18 + 2) | **Terfi etti** — L3 `INSTRUCTIONS §9` (1): görev talimatı sınırı "adımın toplamı" diye yazar. PF-147 |
| Ö-162 | **Toplu onay kapsam kapatma yetkisi vermez.** KP-checkpoint-2'nin işçisi kalan kapsam açıklarını "`validate`'te kapanır" diye kapattı; yönetici geri aldı, açıklar KP-checkpoint-3'te tam kapsamla kapandı (K-1497) | **Terfi etti** — L3 `INSTRUCTIONS §2` toplu onay kolu: mod ⚠ sorularını cevaplar, kapsamı daraltmaz ya da sonraya bırakmaz. PF-147 |
| Ö-170 | **`11_geri_yon.py --yaz` `11`'i CRLF yazıyor** (Windows'ta `Path.write_text`); üç işçi (4a, 4b, 4c) elle LF'e çevirdi | **Terfi etti** — L5 `scripts/doc-scans/11_geri_yon.py`: `newline="\n"` ile yazar. Öteki `11_*` betikleri `11`'i değil kendi çıktılarını yazar (taranıp teyit edildi). PF-150 |
| Ö-171 | **Pre-commit sır koruması düzyazıda yanlış pozitif verdi** (`POST /odeme/api/get-token` yolu, 4b'de "işaretler: T377 k10" ifadesi); işçi metni yeniden ifade etti | **Terfi etti** — L5 `scripts/git-hooks/pre-commit`: soldaki parça ayrıca anahtar gibi görünmelidir (en çok iki ön sözcük ve bir tanımlayıcı). Düzyazı satırı atlanır, `DB_PASSWORD=…` gibi gerçek atama hâlâ engellenir (iki durum geçici depoda sınandı). `scripts/git-hooks/README.md` notu. PF-150 |
| Ö-172 | **İşçi ilerlemesi dalına yazılmadı.** Y5 işçisi 271 bin belirteçte düştü; bloklar yalnız scratchpad'deydi, depoya yazılmamıştı, ikinci işçi yeniden yerleştirdi. Aşama 9'da 15 işçi 250 bin sınırını aştı (en yüksek 273 bin); çoğunda bloklar bitmişti, kayıt ve PR kalmıştı | **Terfi etti** — L3 `INSTRUCTIONS §9` (8): işçi ilerlemesini sınırdan önce dalına commit eder ve push eder. PF-147 |
| Ö-173 | **İşçi dalı yerel (squash'lanmamış) commit'ten açıldı** (matris O6-1): PR `origin/main`'e çakıştı, CI bekleme betiği iki kez 3 döndü; `[mekanik]` işçi `rebase --onto origin/main` ve `force-with-lease` ile düzeltti | **Terfi etti** — L3 `INSTRUCTIONS §9` (8): işçi dalı `git fetch -q` ve `git switch -c <dal> origin/main` ile açılır, yönetici satırı görev metnine yazar. PF-147 |
| Ö-174 | **Hook engelleri.** İki oturumda Aşama 9'un açılışından beri 125 engel: 64 heredoc, 17 here-string, 16 satır içi betik (`python -c`, `node -e`) — yani heredoc ailesi 97 (yüzde 78); 18 bağlam sınırı; 3 doğrulayıcı sayacı; 7 öteki. Aşama 8'de 19 engel vardı. Her engel işçiyi kurala uygun yeniden yazmaya yöneltti, hiçbiri dolanılmadı. Engellerin 2'si bu rapor adımının kendi hatasıdır (here-string ve heredoc) | **Raporda kalır** — hook proje dışıdır ve işini yaptı; kural ve gerekçe `INSTRUCTIONS §7`, §9'da zaten yazılı (işçi talimatına yönetici ekler). Proje dışı öneri P-1 (§5.2) |
| Ö-175 | **Hook sözcük çakışmaları.** (a) Bir doğrulama çağrısı talimatında "hedefli mercek" adı geçtiği için `agent-rules.json` kuralıyla ("hedefli" yasak) başlamadan engellendi, sözcük çıkarılarak yeniden çağrıldı; (b) bir işçi betiğinde "merge" sözcüğü bekçiye takıldı ve yeniden ifade edildi; (c) pre-commit sır bekçisi yanlış pozitifi (Ö-171'de düzeltildi) | **Raporda kalır** — (a), (b) hook proje dışıdır, sözcük yasakları kasıtlıdır; yeniden ifade kuralın izin verdiği yoldur. Öneri P-2 (§5.2) |
| Ö-176 | **Orta bulgu oranı yüksek görünür.** Bulgu kümelerinde mercek Orta işaretler, doğrulayıcı sonrası altısı Düşük'e iner (12 → 4); Orta kademesi (2–4'lü) bu yüzden masraflıdır (CP09 §6 #3) | **Raporda kalır** — tek seferlik gözlem; kademe kuralı (K-1224, K-1300) değişmez |
| Ö-177 | **Üç mercek bulgularını taramanın sonunda yazdı**, "bulduğun anda ekle" kuralına uyulmadı (CP09 §6 tek seferlik 1) | **Raporda kalır** — kural `skills/audit` "Koşum biçimi" 5. maddede doğru yazılı; tek seferlik |
| Ö-178 | **Paketleyici alıntı başlığı kaynak dosyadaki sayıyı taşıdı** (V08 "Toplam: 10", dosyada beş; V17 "7 (1 orta, 6 düşük)", dosyada altı düşük); doğrulayıcı farkı bildirdi, bulgu kaybı olmadı (CP09 §6 tek seferlik 2) | **Raporda kalır** — olumlu gözlem: doğrulayıcılar kendi paketlerini sayıyor |
| Ö-179 | **Mercek önerileri 2.560 baytı aştı** (en az 14 yerde); doğrulayıcılar bayt sınırı için metni yeniden yazdı (CP09 §6 tek seferlik 3) | **Raporda kalır** — Ö-168'in kök nedenidir; elle kısaltmanın karşılaştırma kuralı oradadır |
| Ö-180 | **İşçi `11`'i v1.0 yazdı** (matris O6-1); belge henüz taslaktı, düzeltme işçisi v0.10 yaptı | **Raporda kalır** — tek seferlik; "`11` değişirse sürümü artar" kuralı görev metnindedir |
| Ö-181 | **Merge denetiminin çıktısı her merge'de yaklaşık 4 bin belirteç** yardımcının bağlamına girer; ilerleme notu yardımcı 2'nin 338 bine ulaşmasının nedeni olarak bunu anıyor | **Raporda kalır** — hook proje dışıdır. Öneri P-3 (§5.2) |
| Ö-182 | **Yardımcı 4 bağlam dolunca durma kaydı yazamadan düştü**; ana oturum kaydı yeniden yazdı, iş listesi ↔ GitHub uyumluydu | **Raporda kalır** — tek seferlik; devir kontrolü ve durma kaydı hook'ları proje dışıdır ve işini yaptı |
| Ö-183 | **Cross-review on bir tur sürdü** (Codex terra, iki koşum, altı çağrı; 1. turda 9 bulgu, 11. turda TEMİZ). Hedefli mercek (Faz 4 madde 6) iki kez koştu; ikincisi alt sınıfı kapatmadı ve 32 Düşük bulgu ayrı bir doğrulama adımı istedi (K-1489, K-1490) | **Raporda kalır** — çıkış kuralı (K-849, K-976 kalıbı) ve hedefli mercek kuralı çalıştı; tur sayısı dokümanın boyutuyla ilgilidir, sınır konmaz |
| Ö-184 | **Maliyet.** Aşama 9 iki oturumda 2.245 M işlenen belirteç, yeni metin 118,9 M, 21.281 model çağrısı; Aşama 8'in 4,4 katı, Aşama 7'nin 3,2 katı | **Raporda kalır** — ölçüm ve yorum §3'te, karar kaydında K-1502 |

### 2.3 Hafızada kalan

| # | Gözlem | Sonuç |
|---|---|---|
| Ö-185 | **Claude API aylık kredisi önerisi** (kullanıcı hafızası `claude-api-kredisi-onerisi`; proje sahibi 2026-10-31'de "ok" dedi): Aşama 9 kapanınca, Implementation aşamasının açılışında iki madde karar olarak getirilecek — uzun koşan işleri Agent SDK ve API kredisiyle koşmak; CI'da otomatik PR incelemesi | **Hafızada kalır** — proje kuralı değil, sonraki aşamanın açılış girdisi. Aşama 9'un arşiv işaretinde (6. adım) devir tablosuna "Implementation açılışında karar olarak getirilir" satırı olarak girer |

### 2.4 Karar kaydında yaşayan süreç kararları

| # | Kararlar | Süreç kararı | Sonuç |
|---|---|---|---|
| S-1 | K-1401…K-1403 | Açılış: çalışma modları (üçü de açık), yönetici yardımcı düzeni, şablon taraması (PF-143…PF-145), konu kimliği, devirlerin eşlemesi | **Önceden terfi etmişti** — modlar `INSTRUCTIONS §2`, §9; yetki süreli (arşiv işaretiyle biter); yardımcı düzeni Ö-160 |
| S-2 | K-1404…K-1454 | Matrisin birimi ve oturum bölümü, konu planı, W-1 workshop, altı matris oturumu (O1…O6), sekiz yazım oturumu (Y1…Y8) | **Dokümanda yaşar** (`11` başlık notu, §7, §5); yöntem dersleri Ö-172, Ö-173 (bağlam sınırı, dal kaynağı) |
| S-3 | K-1455…K-1477 | Özet ↔ ayrıntı taraması, audit (2a…2r), deep review (2a…2r) | **Dokümanda yaşar**; yöntem dersleri Ö-161, Ö-166, Ö-167, Ö-168 |
| S-4 | K-1478…K-1493 | Cross-review 1…11. tur, hedefli mercekler, etki yansıtma | **Dokümanda yaşar**; ikinci model kuralı Ö-163 (**Önceden terfi etmişti**) |
| S-5 | K-1494 | Çakışma taraması, ⚠ eşitlemesi, geri işaret | **Dokümanda yaşar**; yöntem dersi Ö-169 |
| S-6 | K-1495…K-1500 | Checkpoint (altı parça: ilk koşu, -2, -3, -4a, -4b, -4c) | **Dokümanda yaşar**; yöntem dersleri Ö-162, Ö-164, Ö-165 |
| S-7 | K-1463, K-1472 | Dört ⚠ kararın gösterimi; toplu onay modu (Aşama 9'un sonuna kadar) | **Önceden terfi etmişti** — `INSTRUCTIONS §2` ortak kural ve toplu onay kolu; mod yalnız Aşama 9'da açık, Aşama 10 açılışında yeniden sorulur; Ö-162 kapsamını netleştirdi |

### 2.5 Hafızadaki yeni notlar (kullanıcı hafızası, salt okunur — indeks satırlarından)

| # | Not | Sonuç |
|---|---|---|
| H-1 | `yonetici-yardimci-duzeni-tasarimi` | Önceden terfi etmişti (`INSTRUCTIONS §9`, K-1402); durum ve ölçüm §3 |
| H-2 | `yonetici-duzeninde-adim-arasi-onay-isteme` | Önceden terfi etmişti (`INSTRUCTIONS §9` işletim kuralı 1, K-1311) |
| H-3 | `claude-api-kredisi-onerisi` | Hafızada kalır — Ö-185 |

Aşama 8 raporunda tartılan on beş not değişmedi; `cursor-agent-cross-review-kosumu` notu K-1484 ile güncellendi (Ö-163).

---

## 3. Aşama 9'un maliyet ölçümü ve yönetici yardımcı düzeninin eski düzenle karşılaştırması (K-1502)

Kaynak: ana oturumların kayıt dosyaları — `node scripts/metrics/token-usage.js <oturum>.jsonl` (`INSTRUCTIONS §9` (10)) ve adım bazlı sayımlar için aynı dosyaların alt ajan kayıtları; hook günlüğü (`~/.claude/logs/subagent-calls.log`); adım değerleri `token-usage.log` farklarından ve yardımcının ilerleme notundan. Aşama iki ana oturumda yürüdü: `d20e5a79` (2026-10-30 22:48 → 10-31 09:40, +03; yardımcı düzeninin beş denemesi, yedi doğrulama turu, açılıştan KD-audit'e kadar) ve `73705434` (10-31 12:52 → 11-01 09:35). Codex çağrıları (ChatGPT hesabı) ayrı hesaptır, dahil değildir. Bu adımın kendisi dahil değildir.

### 3.1 Toplam ve rol dağılımı

| Ölçüt | Değer |
|---|---|
| İşlenen toplam (iki oturum, ana oturum + alt ajanlar) | **2.244,9 M** (668,5 M + 1.576,4 M) |
| Yeni metin (yazılan + çıktı + girdi) | 118,9 M (37,3 M + 81,6 M) |
| Model çağrısı | 21.281 (5.571 + 15.710) |
| Alt ajan sayısı | 920 (8 yardımcı + 10 deneme yardımcısı · 112 işçi · 540 doğrulayıcı (7'si yardımcı düzeninin doğrulama turu) · 250 mercek ve öteki) |
| Süre (açılış A1 → KP-checkpoint-4c) | yaklaşık 30 saat (2026-10-31 03:31 → 11-01 09:35, +03) |

| Rol | Ajan | İşlenen (M) | Pay |
|---|---|---|---|
| Ana oturum — aracı (Opus) | 2 oturum, 426 çağrı | 131,0 | yüzde 5,8 |
| Yönetici yardımcılar — kendi bağlamları (Opus, `medium`) | 8 yardımcı, 499 çağrı | 77,3 | yüzde 3,4 |
| Yardımcı düzeninin denemeleri ve doğrulama turları (Opus) | 10 deneme yardımcısı + 7 tur | 53,7 | yüzde 2,4 |
| İşçiler, mercekler, doğrulayıcılar, uygulayıcılar (Sonnet / Haiku `low`; doğrulayıcı Opus `high`) | 902 ajan | kalan (≈ 1.983) | yüzde 88 |

Yönetici işi (ana oturum + yardımcıların kendi bağlamı) **208 M, toplamın yüzde 9,3'ü**. Rakam kayıt dosyasından ölçülmüştür; yardımcıların kendi çağrıları ayrıdır, işçileri ve doğrulayıcıları "kalan" satırındadır. Opus payı (`token-usage.log`, oturum bazında): `73705434` oturumunda Opus 1.124,4 M, `d20e5a79` oturumunda Opus 333,5 M (toplam 1.457,9 M, yüzde 65); Sonnet 443,5 + 330,4 = 773,9 M. Doğrulayıcılar Opus `high`, işçiler Sonnet `low` koştuğundan toplamın yarısından fazlası Opus'tur.

### 3.2 Adım maliyetleri

Yardımcının ilerleme notundan alınan ve `token-usage.log` farklarıyla doğrulanan en pahalı adımlar:

| Adım | İşlenen (M) |
|---|---|
| Özet ↔ ayrıntı taraması (OA; 61 doğrulayıcı — Ö-161) | 109 |
| Audit — ilk adım (KD-audit) | 95 |
| Deep review doğrulaması 2a / 2e | 41,6 / 47,1 |
| Cross-review — KD-cross-m / KD-cross-m2 | 66,1 / 60,9 |
| Audit ve cross-review'un öteki adımlarının toplamı | ölçülmedi |
| **Checkpoint (altı parça)** | **687** (135 + 109 + 197 + 92 + 95 + 59) — toplamın yüzde 31'i |
| Matris O6-2 (50,3 M; 20 doğrulayıcı) | 50 |

Checkpoint altı parçada 687 M harcadı (Aşama 8'in checkpoint'i 88,9 M). Neden: `11` 750 KB'tır, 214 task bloğu ve 1.501 birimlik matris her biri kaynak bölümlere karşı okunmak zorundaydı; mercek dilimleri büyük olunca kapsam açıkları doğdu ve KP-checkpoint-2, -3, -4a, -4b, -4c olarak beş ek adım gerekti. Ö-164 ve Ö-165 bu maliyetin ana kısmını sonraki aşamada azaltmak için terfi etti.

### 3.3 Aşama 8 ve Aşama 7 ile karşılaştırma

| Ölçüt | Aşama 7 | Aşama 8 | Aşama 9 |
|---|---|---|---|
| Doküman | `08` | `09` (~470 KB) | `11` (~750 KB; 214 task, 1.501 birim) |
| İşlenen toplam | 692,3 M | 512,8 M | **2.244,9 M** |
| Yeni metin | ~32,2 M | 22,8 M | 118,9 M |
| Yönetici payı (işlenen) | ~62 M (yüzde 9) | 92,5 M (yüzde 18) | **208 M (yüzde 9,3)** — ana oturum + yardımcı |
| Opus doğrulayıcı | 241 ajan | 79 ajan | 540 ajan (533 + 7 deneme) |
| Hook engeli | — | 19 | 125 |
| En uzun bağlam (yönetici) | — | ~300 bin ortalama | 338 bin (yardımcı 2; sınır 400 bin) |
| İşçi bağlam sınırını aşan | — | 15 engel | 15 işçi (en yüksek 273 bin) |

**Okuma.** (1) **Yönetici payı Aşama 8'in yarısına indi.** Aşama 8'de yönetici tek uzun oturumda her adımda bağlamını yeniden okuyordu; yardımcılar temiz bağlamla başladı ve devirle sürdü. Sorunsuzluk: sekiz yardımcıdan yalnız biri durma kaydı yazamadan düştü; devirlerde iş listesi ile GitHub arasında uyumsuzluk görülmedi; ana oturum Türker'le konuşmaya ve "neredesin" sorularını ilerleme notundan cevaplamaya devam etti. Merge denetimi Aşama 9'da yalnız bir kez 20'den fazla doğrulayıcı UYARI'sı verdi (OA, 61). (2) **Toplam, doküman büyüdüğü için büyüdü.** `11` Aşama 8'in dokümanından yüzde 60 büyük, matrisi ve 214 task'ı vardı; kalite döngüsünde audit altı alt adıma, cross-review on bir tura, checkpoint altı parçaya yayıldı. Doğrulayıcı sayısı yedi katına çıktı (79 → 540): Aşama 9'un audit'i 479 tekil bulgu, deep review 191 ham, checkpoint 375 ham bulgu getirdi. (3) **Kapsamı sınırlama kuralları (dar mercek, toplu doğrulama, 100 KB sınırı) işledi:** işçi bağlam sınırını 15 işçi aştı (Aşama 8'de 15 engel), ama hiçbiri 273 bini geçmedi ve devirler kayıpsız yürüdü. (4) **Süre** yaklaşık 30 saat (workshop başlangıcından öğrenim terfisine); süre farkı dokümanın boyutundan ve checkpoint'in altı parçadan oluşmasından gelir, yardımcı düzeninden değil.

**Karşılaştırmanın sınırı.** İki aşama farklı dokümanları farklı boyutlarda yazdı; Aşama 9'un toplamı yardımcı düzeninin kendi denemelerini (53,7 M) ve açılış sohbetini içerir. Ölçüm kayıt dosyalarındandır, ajan bildirimi değildir; Codex çağrıları ve bu adımın kendisi dışarıdadır. Aşama 7'nin yönetici payı K-1301'in ölçümünden alınmıştır.

### 3.4 Yardımcı düzeni için sonuç

Tracker §22.1'in "Yardımcı düzeninin ölçümü" maddesi bu bölümle kapanır: **bağlam sınırı** 338 bin ile 400 bin altında kaldı, **maliyet** yönetici işinin toplamdaki payını yüzde 18'den yüzde 9,3'e indirdi, **sorunsuzluk** yüksek (sekiz yardımcıdan bir düşme, kayıpsız toparlandı), **süre** karşılaştırılamaz (farklı doküman). **Önerim (Aşama 10 açılışında):** düğme açık kalsın; Doğrulama Protokolü (`12`) aynı kalıpta uzun bir aşamadır ve ana oturumun bağlamını doldurmamak asıl kazançtır. Karar proje sahibinindir ve aşama açılışında, çalışma modlarıyla birlikte yeniden sorulur (`INSTRUCTIONS §9`); bu rapor soru açmaz. Implementation aşamasında (kod PR'ları) doğrulayıcının merge ettiği akış için düğme o dönemin açılışında yeniden değerlendirilir (`INSTRUCTIONS §9`).

---

## 4. Ö-23 — checklist skill'e dönüşür mü

**Sonuç: sorulmaz.** Proje sahibi Aşama 8'in öğrenim terfisinde (K-1399) sorunun artık her aşama sonunda sorulmamasına karar verdi; ölçüt checklist başlık notunda kalır, dönüşümü proje sahibi isterse açar. Yine de iki koşul ölçüldü: (1) matris Aşama 9'da zorunluydu ve dördüncü kez işletildi (✓); (2) kapanış yeni adım ya da madde almadı — `grep -c '^- \[ \]' .claude/checklists/document-stage.md` → 67 (başlangıç 67), değişen yalnız bir madde genişlemesi (§3 geri işaret, Ö-169). Başlık notuna dokunulmadı; "Son güncelleme" tarihi 2026-11-01.

---

## 5. Proje sahibinin cevabını bekleyen soru ve toplu onayla kaydedilenler

### 5.1 Toplu onayla cevaplanan kararlar (K-1472)

Toplu onay Aşama 9'un sonuna kadar açık (`INSTRUCTIONS §2` mod 1); öğrenim soruları önerimle cevaplandı ve `(toplu onayla kaydedildi)` işaretiyle K-1501'e yazıldı. Üçü de geri alınabilir süreç kararıdır, ⚠ grubuna girmez.

- **Karar A — L3–L5 hedefli on iki terfi bu PR'da uygulanır** (GUARDRAILS §2'nin dışında kalan hedefler: `INSTRUCTIONS`, skill'ler, checklist, `scripts/`, `scripts/git-hooks/`). Öneri: uygula. *(toplu onayla kaydedildi)*
- **Karar B — paketleyici betiklerinde aynı yeri eşleme (Ö-167) kural olarak yazılır, mevcut betiğe yazılmaz.** Sebep: `09_*` ve `11_*_paketle.py` betikleri o aşamanın dokümanına özeldir; Aşama 10'un kalite döngüsü yeni paketleyici yazacaktır. Kural `skills/audit`'te; kapı: Aşama 10'un ilk paketleyici betiği yazıldığı adım (arşiv işaretinin devir tablosuna girer). Öneri: kural olarak. *(toplu onayla kaydedildi)*
- **Karar C — hook ve proje dışı dosyalara dokunulmaz; önerilen üç düzeltme §5.2'de bilgi olarak durur.** Öneri: bilgi olarak. *(toplu onayla kaydedildi)*

### 5.2 Proje dışı öneriler (`~/.claude`; bilgi, soru değil)

- **P-1 — işçi talimat şablonuna sabit yasak satırı.** 97 engel (yüzde 78) heredoc ailesindendi; işçiler yasağı talimatta görmeden denedi. Yönetici işçi talimatına "kabukta heredoc, here-string, `python -c`, `node -e`, `python -` yok; betik `Write` ile" satırını sabit olarak ekler (zaten `INSTRUCTIONS §7`'de yazılı, işçi talimatına geçmiyor). Hook bu satırı otomatik ekleyebilir.
- **P-2 — hook sözcük yasakları yalnız komuta bakmalı, talimat metnine değil.** "Hedefli" ve "merge" sözcükleri bir görev talimatında kuralın adı olarak geçince çağrıyı engelliyor; çözüm yeniden ifade. Öneri: bu iki sözcük yalnız komut satırında (`gh pr merge`) engellensin.
- **P-3 — merge denetimi çıktısı kısalsın.** Her merge'in çıktısı yardımcının bağlamına yaklaşık 4 bin belirteç ekliyor; yardımcı 2 bu yüzden 338 bine ulaştı. Öneri: çıktı yalnız UYARI varsa uzun, yoksa tek satır.

### 5.3 Proje sahibinin cevabını bekleyen soru

#### Soru 1 — Yöntem dosyasına Aşama 9'un dersleri (`00 §N.1`)

> **Sorun:** Yöntem dosyası her aşamanın tekrarlanan derslerini kısa bir paragrafla tutuyor; Aşama 9 için henüz yok. **Ne değişir:** Evet derseniz aşağıdaki dört ders yöntem dosyasına yazılır (sonraki projelerin aynı hataya düşmemesi için); hayır derseniz dersler yalnız bu raporda ve çalışma kurallarımızda kalır. Dört ders: (1) çok uzun bir aşamada yöneticilik işini temiz bağlamlı bir yardımcıya vermek ana oturumu doldurmadan yürüttü ve yöneticinin payını yarıya indirdi; (2) kontrol yapan ajanlara verilen iş parçası küçük tutulunca (yaklaşık 60 KB) bitti, büyük olunca kapsam açık kaldı ve ek adımlar gerekti — her ajan hangi parçaları bitirdiğini de yazmalı; (3) "sonuna kadar git" izni kapsamı küçültme ya da sonraya bırakma izni değildir ve bir sayı sınırı, adımın toplamı olarak yazılmazsa parti başına okunur; (4) bir ajanın önerdiği düzeltme sabit bir biçimde istenmezse yeniden biçimlendirmek için ek ajan gerekir, ve elle kısaltılan metin düzeltmeyle karşılaştırılmazsa bir ek sessizce düşebilir.
> **1 ·** dört ders yöntem dosyasına yazılsın (öneri) · **2 ·** yazılmasın
> Kaynak: Ö-161, Ö-162, Ö-164, Ö-165, Ö-166, Ö-167, Ö-168, Ö-160 · `00 §N.1` · PF-151

**Uygulanacak metin (1 ise).** `00 §N.1`'e, Aşama 8 paragrafının altına: *"**Aşama 9 — Implementation Planı (2026-11-01).** Kurallar `.claude/INSTRUCTIONS.md` §2, §9, `.claude/skills/audit/SKILL.md`, `.claude/skills/checkpoint/SKILL.md`, `.claude/checklists/document-stage.md` §3 ve rapor `Docs/CHECKPOINT_REPORTS/PHASE9_LEARNING_PROMOTION.md`. Tekrarlanacak desenler: 1. **Uzun bir aşamada yöneticilik temiz bağlamlı bir yardımcıya verilir.** Aşama 9'da (214 task, yaklaşık 750 KB plan, otuz saat) sekiz yardımcı ardışık çalıştı; en yüksek bağlam 338 bin (sınır 400 bin), yönetici işi toplam belirtecin yüzde 9,3'üne indi (Aşama 8'de yüzde 18), sorunsuzluk yüksekti (sekiz yardımcıdan bir düşme, kayıpsız toparlandı). 2. **Kontrol ajanının dilimi küçük ve bitiş satırı zorunludur.** 80 KB'lık dilimler, 140 satırlık matris bölümleri ve 598 satırlık kapsam notu paketi Sonnet / `low` merceğin bağlamına sığmadı ve checkpoint beş ek adım istedi; yaklaşık 60 KB ve 8–12 task'lık paketler bitti. Her mercek hangi dilimleri bitirdiğini yazar; kapsam açığı başka bir kapıya devredilmez. 3. **Toplu onay kapsamı daraltma izni değildir; sayı sınırı adımın toplamıdır.** Bir işçi kalan kapsam açıklarını 'başka kapıda kapanır' diye bıraktı (yönetici geri aldı); bir işçi 20 doğrulayıcı sınırını parti başına okudu ve 61 doğrulayıcı koştu. 4. **Düzeltme hükmü sabit biçimde istenir ve elle kısaltma şartnameyle karşılaştırılır.** Serbest yazılan hükümler için beş ek ajan gerekti; bayt sınırı için elle kısaltmada doğrulanmış bir kabul kriterinin eki düştü (T370 KK7 'IBAN silinir')."* `00` sürümü +1 (v1.0.11). PF-151 `Uygulandı` olur.

---

## 6. Uygulanan terfiler — dosya ve bölüm

| Dosya | Bölüm | Değişiklik | Aday |
|---|---|---|---|
| `.claude/INSTRUCTIONS.md` | §2 — "Proje sahibinin açabildiği iki mod", toplu onay kolu | Toplu onay kapsamı kapatmaz ya da atlamaz | Ö-162 |
| | §9 — "Alt ajanların koşumu" (1) | Görev talimatı sınırı "adımın toplamı" diye yazar | Ö-161 |
| | §9 — (8) | İşçi ilerlemesini sınırdan önce dalına yazar; dal `origin/main`'den açılır | Ö-172, Ö-173 |
| `.claude/skills/audit/SKILL.md` | "Koşum biçimi" 2. madde | Şartname biçimi talimata yazılır; aynı yere dokunan bulgular tek doğrulayıcıya; elle kısaltmadan sonra karşılaştırma | Ö-166, Ö-167, Ö-168 |
| `.claude/skills/checkpoint/SKILL.md` | "Koşum" | Mercek dilimi ~60 KB ve 8–12 task; matris dilimi task başına; bitiş satırı; kapsam açığı devredilmez | Ö-164, Ö-165 |
| `.claude/checklists/document-stage.md` | §3 — geri işaret maddesi; "Son güncelleme" | Çakışma taraması ikinci geçişi `--tum` ile koşar | Ö-169 |
| `scripts/doc-scans/geri_isaret_tara.py` | — | `--tum` seçeneği: her eski K anması aday; ilişki sözcüğü yalnız sıralar | Ö-169 |
| `scripts/doc-scans/11_geri_yon.py` | `--yaz` | `11`'i `newline="\n"` ile yazar (CRLF yok) | Ö-170 |
| `scripts/git-hooks/pre-commit` | içerik katmanı | Soldaki parça anahtar gibi görünmelidir (≤ 2 ön sözcük + 1 tanımlayıcı); düzyazı satırı atlanır | Ö-171 |
| `scripts/git-hooks/README.md` | sır guard'ı notları | "Anahtar biçimi" notu | Ö-171 |

Madde sayısı yerinde: `grep -c '^- \[ \]' .claude/checklists/document-stage.md` → 67 (başlangıç 67). **L5 (CI) elendi:** Ö-169'un `--tum` geçişi adım sonu betiğine elle zincirlenir; çıktısı yüzlerce aday satırdır (Aşama 9'da `1456 1500` aralığında 194) ve her biri okunur — CI görevi olarak taşımak gereksizdir. **Betik değişikliği yapılmadı (Ö-167):** mevcut paketleyici betikleri (`09_*`, `11_*_paketle.py`) kendi dokümanlarına özeldir; kural `skills/audit`'tedir ve yeni paketleyici yazıldığı adımda uygulanır (Karar B). **Hook (genel) değişmedi:** P-1, P-2, P-3 bilgi olarak §5.2'dedir.

Betik sınaması: `geri_isaret_tara.py 1456 1500` → `pairs: 14; missing: 0` (varsayılan geçiş değişmedi); `--tum` → `pairs: 219; missing: 194 (all mentions)`; pre-commit geçici depoda iki durumla sınandı (`POST /odeme/api/get-token: …` düzyazısı çıkış 0, `DB_PASSWORD=…` ataması çıkış 1).

---

## 7. Playbook'a geri akış (K-649)

Liste 146 satırdan **152**'ye çıktı. Var olan satırlar tekrar açılmadı.

| Satır | Öğrenim | Hedef |
|---|---|---|
| **PF-147** (yeni) | Görev talimatı doğrulayıcı sınırını adımın toplamı diye yazar; toplu onay kapsam kapatmaz; işçi ilerlemesini dalına yazar; işçi dalı `origin/main`'den açılır (Ö-161, Ö-162, Ö-172, Ö-173) | `INSTRUCTIONS §2`, §9 |
| **PF-148** (yeni) | Kontrol merceğinin dilimi ~60 KB ve 8–12 task; matris dilimi task başına; mercek bitiş satırı; kapsam açığı devredilmez (Ö-164, Ö-165) | `skills/checkpoint` |
| **PF-149** (yeni) | Doğrulayıcı hükmü sabit biçimde; aynı yere dokunan bulgular tek pakette; elle kısaltmadan sonra karşılaştırma (Ö-166, Ö-167, Ö-168) | `skills/audit` |
| **PF-150** (yeni) | Geri işaret `--tum` geçişi; `11` yazan betik LF; pre-commit anahtar biçimi (Ö-169, Ö-170, Ö-171) | `scripts/doc-scans/` · `scripts/git-hooks/` · checklist §3 |
| **PF-151** (yeni) | Soru 1: `00 §N.1`'e Aşama 9'un dersleri | `00 §N.1` — `Onay bekliyor` |
| **PF-152** (yeni) | Hook ve talimat şablonu: işçi talimatına sabit yasak satırı, sözcük yasaklarının yalnız komuta bakması, merge denetimi çıktısının kısalması (P-1, P-2, P-3) | `~/.claude/hooks/` · `agent-rules.json` (proje dışı) — `Öneri` |

---

## 8. Kayda ve hafızaya yansıma

- **Karar kaydı v0.288:** K-1501 (öğrenim terfisi — otuz yedi kalemin dökümü, on iki terfi, bir soru, üç toplu onay kararı) ve K-1502 (Aşama 9'un belirteç ölçümü ve yardımcı düzeninin karşılaştırması); §22.1'e öğrenim terfisi maddesi (yardımcı düzeninin ölçümü kapandı; "Aşama kapanışının adımları" güncellendi); tracker başlığı v0.288.
- **`.claude/memory/MEMORY.md` Güncel Durum:** Aşama 9 öğrenim terfisi tamam; sırada arşiv işareti (adım 6).
- **⚠ eşitlemesi:** konu hücresi ⚠ taşıyan Aşama 9 satırı 8 (K-1411, K-1413, K-1432, K-1458, K-1463, K-1474, K-1476, K-1483); `onerilen-uyarili.md` girişi 8; **8 / 8, eksik 0, fazla 0.** K-1501 ve K-1502'nin konu hücreleri ⚠ taşımaz. ⚠ listesi arşiv işaretinden (adım 6) önce proje sahibine tek listede gösterilir.

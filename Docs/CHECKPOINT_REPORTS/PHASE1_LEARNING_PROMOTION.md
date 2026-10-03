# Aşama 1 — Öğrenim Terfisi

**Tarih:** 2026-10-03 | **Aşama:** 1 — Product Discovery | **Kapanış adımı:** K-437'nin 5. adımı (`00 §K`, K-438)
**Girdi:** karar kaydı v0.50 — K-438'in dört adayı ve §6.1'de biriken adaylar ([çakışma taraması §7](PHASE1_CONFLICT_SCAN.md)'nin on altı satırlık dizini), [checkpoint](CP01_PHASE1_CHECKPOINT.md)'in B-31 maddesi, proje hafızası (`.claude/memory/`) ve kullanıcı hafızasının on bir notu — `origin/main` @ `d6c772e`
**Çıktı:** karar kaydı v0.51 · `.claude/INSTRUCTIONS.md`, `.claude/GUARDRAILS.md`, `.claude/checklists/document-stage.md`, `.claude/skills/cross-review/`, `audit/`, `deep-review/`, `.claude/memory/`, `scripts/git-hooks/pre-commit` ve `README.md`, `Docs/CHECKPOINT_REPORTS/README.md` · karar K-646 — Proje Vizyonu, Ürün Gereksinimleri ve MVP Kapsamı değişmedi

> **Ne işe yarar:** `00 §K` gereği aşama, öğrenimi yazılmadan kapanmaz. Bu rapor aşama boyunca kayda ve hafızaya düşen her öğrenim adayının sonucunu yazar: terfi etti mi, hangi katmana ve dosyaya, metni ne. L3–L5 hedefli terfiler bu raporun PR'ında uygulandı; `00`, `CLAUDE.md` ve `SETUP.md` hedefliler GUARDRAILS §2 gereği uygulanmadı, önerilen tam metinleriyle §4'te proje sahibinin onayını bekliyor.

---

## 1. Sonuç

| Sonuç | Sayı | Adaylar |
|---|---|---|
| **Terfi etti — bu PR'da uygulandı** (L3, L4, L5) | 21 | Ö-1…Ö-9, Ö-11…Ö-16, Ö-18…Ö-22 · Ö-10'un L4 parçası |
| **Önceden terfi etmişti** | 2 | Ö-17 (SETUP, PR #38) · H-9 (`INSTRUCTIONS §2`, PR #37) |
| **Hafızadan terfi etti** (tamamı ya da süreç parçası) | 6 | H-1, H-2, H-3, H-6, H-8 ve proje hafızasının K-38 maddesi (P-1) — H-4 ve H-11 sırasıyla Ö-1 ve Ö-18 ile birlikte |
| **Hafızada kalır** — kişisel tercih ya da süreli yetki | 3 | H-5, H-7, H-10 |
| **Bekletildi** — kapısıyla | 1 | Ö-23 — kapı: Aşama 2'nin öğrenim terfisi |
| **Proje sahibinin onayını bekliyor** (`00`) | 3 öneri | §4: `00 §N.1`'in dokuz deseni (Ö-10, Ö-11 ve aşamanın desenleri) · `00 §C.5`'e bir cümle (Ö-15) · `00`'ın sürüm satırları (Ö-24, checkpoint B-31) |

`CLAUDE.md` ve `SETUP.md` için terfi önerisi yok: CLAUDE.md'nin oturum başlangıcı ve katman kuralı aşamanın hiçbir öğrenimiyle çelişmiyor; SETUP'ın ikinci AI yedek satırı (Ö-17) zaten yazılı.

**Aşamanın asıl öğrenimi (Ö-21).** Aşama 1'in süreç kararlarının önemli bir kısmı — yazım oturumunun ayrılması (K-30), taslak ve yazım turu (K-28, K-432), satır biçimi ve etki sütunu (K-34, K-35), sonraki aşamaya park (K-36), vade kapısı (K-37), oturum sonu PR'ı ve içeriği (K-33, K-38), kalite döngüsünün birimi, sırası ve koşum biçimi (K-430, K-431), çakışma taraması (K-429) ve kapanış sırası (K-437) — **yalnız karar kaydında** yaşıyordu. Kayıt 6. adımda arşivlenince otoriterliği biter (K-436); bu kurallar L1–L5'te olmasaydı Aşama 2 onları bilmeyecekti. Hepsi bu PR'da checklist'e ve skill'lere taşındı.

---

## 2. Adaylar ve sonuçları

**Kimlikler:** Ö-1…Ö-16 çakışma taramasının §7 dizinindeki sırayı korur; Ö-17 orada `—` satırıdır; Ö-18…Ö-24 bu adımda bulunan adaylardır. H-1…H-11 kullanıcı hafızasının notlarıdır (`~/.claude/projects/c--projects-Shopfolio/memory/`), P-1 proje hafızasının (`.claude/memory/MEMORY.md`) maddesidir.

### 2.1 Karar kaydındaki adaylar

| # | Aday | Kaynak | Sınıf (`00 §K`) | Sonuç | Hedef katman ve dosya | Uygulanan metin (özet) |
|---|---|---|---|---|---|---|
| Ö-1 | `(toplu onayla kaydedildi)` işaretinin işlenişi ve listenin **ne zaman** gösterileceği | K-438 (1); §6.1 2026-09-26, 2026-10-01; çakışma taraması §4.1 | İhlali önleyen kural | **Terfi etti** | L3 `INSTRUCTIONS.md` §2 · L3 `GUARDRAILS.md` §6 · L4 `checklists/document-stage.md` §1, §7 | §2'ye "Proje sahibinin açabildiği iki mod" maddesi: toplu onay ve ⚠ öneriyle kayıt yalnız proje sahibinin sözüyle açılır, aşamayla sınırlıdır; gösterilmeden kaydedilen ⚠ kararlar tek listede, karar başına bir cümleyle gösterilir — toplu onayda blok kapanış mesajında, ⚠ öneriyle kayıtta adım sonunda, en geç arşiv işaretinden önce; gözden geçirilen işaret `(… — gözden geçirildi YYYY-AA-GG, itiraz yok)` olur. GUARDRAILS §6 istisnaları bu iki moda genişletir ve listeyi göstermeden aşamayı kapatmayı yasaklar. Checklist §1: yeni aşamanın açılışında modlar yeniden sorulur; §7 adım 6: liste arşivden önce |
| Ö-2 | K-404'ün manuel adım bütçesinin süreç tarafı | K-438 (2) | İhlali önleyen kural | **Terfi etti** | L4 `checklists/document-stage.md` §3 | Firmaya sipariş başına yeni bir elle adım ekleyen karar bütçeyle birlikte okunur (K-404, `02 §10`); tavanı aşacaksa bütçe kararıyla birlikte yeniden açılır — `03`–`12`'nin workshop ve yazımında da |
| Ö-3 | K-432'nin yazım/döngü sıralaması checklist §7'ye | K-438 (3) | İhlali önleyen kural | **Terfi etti** | L4 `checklists/document-stage.md` §4, §7 | §4 "Ne zaman": blok tetikli taslak (K-28), blokların tamamı kapanınca tek yazım turu, kalite döngüsü turdan sonra (K-432). §7 kapanışın altı adımı ve sırası (K-437); §3'ün "taslak aynı bloğun PR'ında güncellenir" cümlesi K-432'ye hizalandı |
| Ö-4 | Blok planının "Hedef doküman" sütununun blok kapanışında etki birleşimine hizalanması | K-438 (4); §6.1 2026-09-19, 2026-09-30 | İhlali önleyen kural | **Terfi etti** | L4 `checklists/document-stage.md` §3 | Blok kapanışında sütun bloğun etki sütunlarının birleşimine betikle hizalanır — sütun taslak kapısını belirler (K-28) |
| Ö-5 | K-29'un mekanik taraması etki sütununa yazılmamış dokunuşları göremez | §6.1 yazım turu `01` | İhlali önleyen kural | **Terfi etti** | L4 `checklists/document-stage.md` §4 | "Anlamsal tarama": taslağı yazılmış bölümler kaydın tamamına karşı okunur; bulunan karara `XX §N (taslak güncellendi — vX.Y)` eklenir (kanıt: 22 + 50 + 11 karar) |
| Ö-6 | K-29'un blok kapanış taraması işaretin satırdaki **her** bölümü kapsayıp kapsamadığına bakmıyor | §6.1 yazım turu `02` (1) | İhlali önleyen kural | **Terfi etti** | L4 `checklists/document-stage.md` §3 | İşaret satırın taşıdığı her taslağı yazılmış bölümü kapsar; blok kapanışında işareti o bölümü kapsamayan satır da K-29 kaçağıdır (kanıt: dokuz satır) |
| Ö-7 | Kapanmamış devirler — tarama devrin varlığını görüyor, kapanışını görmüyor | §6.1 yazım turu `02` (2); çakışma taraması §3.3 | İhlali önleyen kural | **Terfi etti** | L4 `checklists/document-stage.md` §3, §4 | Blok kapanışında bloğun devraldığı her iş karara bağlandı mı — devralan konu yalnız biçimi kurup değeri yazmadıysa devir açıktır; yazımda "Devir taraması" |
| Ö-8 | Adsız devir — aday listesi işin türünü değil adını görür | §6.1 yazım turu `10` (1) | İhlali önleyen kural | **Terfi etti** | L4 `checklists/document-stage.md` §3 | Devir yazılırken işin adı yazılır ("otomasyon" değil "kargo şirketi entegrasyonu ve havale eşleştirmesi") |
| Ö-9 | Sonraki karar önceki kuralı daralttığında önceki satır iz taşımıyor | §6.1 yazım turu `10` (2) | İhlali önleyen kural | **Terfi etti** | L4 `checklists/document-stage.md` §3 | Önceki kuralı daraltan ya da değiştiren karar önceki satırın etki sütununa geri işaret bırakır (kanıt: K-435 → K-25 → A-11). Bu raporun kararı K-646 kuralı K-438, K-615 ve K-644'te uyguladı |
| Ö-10 | Proje Vizyonu ve MVP Kapsamı'nın şablonunda açık kararlar bölümü yok; K-428 ve K-429 var olmayan bölüme işaret ediyor | §6.1 yazım turu `10` (3) | Kural (L4) + desen (L1) | **Terfi etti (L4); L1 parçası onay bekliyor** | L4 `checklists/document-stage.md` §7 adım 3 · L1 `00 §N.1` (§4, desen 9) | Çakışma taramasında: şablonunda açık kararlar bölümü olmayan dokümanın açık kalemi tracker §4'te kalır ve devir notuna yazılır. Şablon önerisi `00 §N.1`'e — şablonlar bu reponun değil playbook'un konusu |
| Ö-11 | Vizyon dokümanına kural ayrıntısı taşınmaz, kurala işaret edilir | §6.1 kalite döngüsü (1) | Kural (L4) + desen (L1) | **Terfi etti** | L4 `checklists/document-stage.md` §4 ve aşama-spesifik tablo · L1 `00 §N.1` (§4, desen 4) | "Kural ayrıntısı evinde kalır": doküman başka bir dokümanın kuralına bölüm numarasıyla işaret eder, ayrıntıyı kopyalamaz; Product Discovery satırına aynı not |
| Ö-12 | Pre-commit sır guard'ı düz metindeki bir sözcüğü sır ataması sanıyor | §6.1 kalite döngüsü (2) | İhlali önleyen kural (L5) | **Terfi etti** | L5 `scripts/git-hooks/pre-commit` · `scripts/git-hooks/README.md` | İçerik katmanı satırı ancak sır anahtarı ilk `=` ya da `:` işaretinin **solunda** duruyorsa atama sayar; ayırıcısız satır atama değildir, bu yüzden `.netrc` yol desenine eklendi. README'ye gerekçe ve test matrisine üç satır. On beş senaryoyla denendi (§3) |
| Ö-13 | İkinci model bilinçli karara tekrar tekrar itiraz eder — talimat cümlesi | §6.1 kalite döngüsü (4) | İhlali önleyen kural | **Terfi etti** | L4 `skills/cross-review/SKILL.md` Faz 1 madde 4 | Talimat her turda şu cümleyi taşır: *"Dokümanda açıkça bilinçli karar, elenen seçenek ya da kabul edilmiş risk olarak yazılmış bir seçimi, yalnız seçime katılmadığın için bulgu yapma. Böyle bir karar dokümanın kendi içinde çelişki, olgusal ya da hukuki hata doğuruyorsa yaz."* |
| Ö-14 | Kural ayrıntısının yasal dayanağı cross-review'da sarsılabilir | §6.1 kalite döngüsü (5); A-14 → K-491 | İhlali önleyen kural | **Terfi etti** | L4 `skills/audit/SKILL.md` "Koşum biçimi" madde 3 · `skills/deep-review/SKILL.md` kural 7 | Yasal dayanağa yaslanan her kural resmî metnin güncel hâline karşı doğrulanır; mevzuat, madde ve okunduğu tarih rapora yazılır; doğrulanamayan dayanak "doğrulanamadı" diye raporlanır |
| Ö-15 | Ciddiyet ölçüsü `cross-review` skill'inin istemine ve çıkış koşuluna (K-615, K-644) | §6.1 kalite döngüsü (6); K-615, K-644 | İhlali önleyen kural | **Terfi etti** | L4 `skills/cross-review/SKILL.md` Faz 1 "Ciddiyet ölçüsü", Faz 4 · L1 `00 §C.5`'e bir cümle (§4, öneri 2) | Dört sınıf (mevzuata aykırılık · para ya da hak kaybı · çıkışı olmayan akış · iç çelişki). **Varsayılan değildir, valftir:** döngü yakınsamıyorsa (art arda iki turda kabul edilen bulgular dar kenar durumlarda ve doküman büyüyor) sonraki turdan, ayrıntısı başka dokümanda yaşayan kapsam/özet dokümanında ilk turdan uygulanır; hangi turdan uygulandığı rapor başlığına ve karar kaydına yazılır, aşamayla sınırlıdır. Çıkış koşulu: TEMİZ o ölçüdedir. **Elenen:** ölçüyü her dokümana ilk turdan uygulamak — `05`–`09` gibi teknik dokümanlarda "teknik doğruluk" bulguları dört sınıfın dışında kalır ve kalite döngüsü zayıflardı |
| Ö-16 | Kaynak satırlarının mekanik taraması `cross-review` Faz 5'e | §6.1 kalite döngüsü (7); çakışma taraması §7 gözlemi | İhlali önleyen kural | **Terfi etti** | L4 `skills/cross-review/SKILL.md` Faz 5 madde 4, 5 | Gövdede anılan her karar numarası bölümün Kaynak satırında var mı — betikle; turların "önceki turlardan açık kalanlar" notu ya etki sütununa bağlanır ya da kapanışıyla yazılır |
| Ö-17 | İkinci AI'nın hesap limiti — yedek yöntem (Codex CLI) | §6.1 kalite döngüsü (3) | Kural (L1-dışı, SETUP) | **Önceden terfi etmişti** (SETUP, PR #38) | SETUP kayıt tablosu "İkinci AI — yedek yöntem" · L4 `skills/cross-review/SKILL.md` Faz 1 madde 1 | Skill'e tek cümle: birincil yöntemin limiti dolarsa SETUP'ın yedeğiyle sürdürülür, rapor başlığı modeli yazar. SETUP değişmedi |
| Ö-18 | ⚠ öneriyle karar yetkisi — *"kolay soruları sorma, çok kritik konuları sor"* (2026-10-03) | Proje sahibinin talimatı; kalite döngüsü `02`, `10` (K-499…K-525, K-561…K-615'in kırk ikisi, K-616, K-618…K-629); kullanıcı hafızası H-11 | İhlali önleyen kural | **Terfi etti** | L3 `INSTRUCTIONS.md` §2 (iki modun ikincisi) · L3 `GUARDRAILS.md` §6 · L4 `checklists/document-stage.md` §1 · L4 `skills/cross-review/SKILL.md` Faz 3 | ⚠ grubunda belirgin öneri ve yakın seçeneklerde öneriyle karar, işaret `(öneriyle kaydedildi — ⚠)`; yine de sorulanlar: geri dönüşü zor ya da kimliği değiştiren varlık kararları, ciddi para veya hukuki risk taşıyan ve seçenekleri ayrışan konular; onay sorusu sorulmaz. **Kapsam:** mod açıldığı aşamayla sınırlıdır; Aşama 2'nin açılışında yeniden sorulur |
| Ö-19 | K numarası çakışmasının önlenmesi | Aşama 1 kapanışında adımların ayrı dallarda ve alt ajanlarla yürümesi | Önleyici kural | **Terfi etti** | L3 `INSTRUCTIONS.md` §7 · L4 `checklists/document-stage.md` §3 | K numarası `origin/main`'deki kaydın son numarasından devam eder; aynı anda iki `docs:` dalı karar yazıyorsa aralıklar baştan ayrılır; merge'den önce çift numara taraması (komut checklist'te). **Bugünkü durum:** kayıtta çift numara yok (K-01…K-646, betikle sayıldı) |
| Ö-20 | K-38 kaçağı — kapanış adımlarının PR'ları repo hafızasının Güncel Durum bloğunu güncellemedi | PR #40…#52 (on üç adım PR'ı); proje hafızası P-1 | İhlali önleyen kural | **Terfi etti** | L3 `INSTRUCTIONS.md` §7 · L4 `checklists/document-stage.md` §3 | Doküman döneminde her oturum ve kapanış adımı kendi `docs:` PR'ıyla kapanır (K-33); PR'a Güncel Durum, sürüm başlığı ve blok tablosu girer (K-38); alt ajana verilen adımlar dahil. Güncel Durum bu PR'da Aşama 1 kapanışının bugünkü hâline getirildi |
| Ö-21 | Karar kaydında yaşayan süreç kararları — arşivle otoriterlik biter | K-28, K-30, K-33, K-34, K-35, K-36, K-37, K-38, K-429, K-430, K-431, K-432, K-437; K-436 | İhlali önleyen kural | **Terfi etti** | L4 `checklists/document-stage.md` §3, §4, §6, §7 · `skills/audit/SKILL.md` "Koşum biçimi" · `skills/cross-review/SKILL.md` Faz 1 madde 2 | Satır biçimi, park ve vade kapısı (§3); ayrı yazım oturumu, taslak ve yazım turu (§4); döngünün birimi, sırası ve koşum biçimi (§6); kapanışın altı adımı, çakışma taraması ve "karar kaydındaki süreç kararları da öğrenim adayıdır" (§7); audit ve deep review'ın çok mercekli, karşı-doğrulamalı koşumu; cross-review girdisinin yalnız doküman ve şablon olması |
| Ö-22 | `cross-review` Faz 3 "kullanıcı karar verir" ↔ öneriyle kayıt modları | Kalite döngüsü `02`, `10` | Hizalama | **Terfi etti** | L4 `skills/cross-review/SKILL.md` Faz 3 madde 4 | Proje sahibinin açtığı mod varsa bulgu kararları modun işaretiyle kaydedilir ve tur sonunda tek listede bildirilir |
| Ö-23 | Checklist'in skill'e dönüştürülmesi (`00 §C.7`: "bir kez gerçek bir projede işletildikten sonra") | `00 §C.7` | Gözlem | **Bekletildi** — kapı: Aşama 2'nin öğrenim terfisi | — | Checklist yalnız bir aşamada işletildi ve bu adımda büyük ölçüde değişti; Aşama 2–10 tek çıktılı ve teknik aşamalardır. Dondurmak için ikinci bir aşamanın işletimi beklenir |
| Ö-24 | `00`'ın başlığı v1.0.3, alt bilgisi v1.0.1 | Checkpoint B-31 | Düzeltme (L1) | **Onay bekliyor** | L1 `00` başlık ve alt bilgi (§4, öneri 3) | — |

### 2.2 Hafızadaki adaylar

CLAUDE.md: *"Bir süreç kuralı yalnız hafızada yaşıyorsa kırılgandır — L1–L5'e terfi eder."* `00 §G.3`: hafızada yalnız **kişisel çalışma tercihleri** kalır.

| # | Not | Tür | Sonuç | Hedef | Hafızada kalan |
|---|---|---|---|---|---|
| H-1 | Workshop sorusu öncesi template taraması | Süreç kuralı | **Terfi etti** | L3 `INSTRUCTIONS.md` §2 — "Soru açmadan önce kural katmanlarını tara" | İşaretçi |
| H-2 | Kısa diyalog tarzı workshop sorusu | Süreç kuralı + kişisel üslup | **Kısmen terfi etti** | L3 `INSTRUCTIONS.md` §2 — "Seçenekleri sade dille yaz" maddesine: öncül önce anlatılır, yapı sorusunda iki yer gösterilir, K numarası ya da `§` soruyu taşıyorsa soru soyuttur | Soru uzunluğu ve biçimi (~15 satır, somut örnek → seçenek → öneri → soru) — kişisel üslup |
| H-3 | CI beklerken izleyiciye güvenme | Süreç kuralı | **Terfi etti** | L3 `INSTRUCTIONS.md` §3.2 — "Sessizlik yeşil değildir" | İşaretçi |
| H-4 | Toplu onayda ⚠ maddeleri ayrı işaretle | Süreç kuralı | **Terfi etti** (Ö-1 ile) | L3 `INSTRUCTIONS.md` §2 | İşaretçi |
| H-5 | Limit dolarsa kaldığın yerden devam | Kişisel çalışma tercihi | **Hafızada kalır** | — | Tamamı — kesintiden sonra sormadan sürdürme ve ilerleme notu, proje sahibinin kişisel tercihi; onay gerektiren aksiyonları zaten dışarıda bırakıyor |
| H-6 | Yazım oturumunda tek bağlam | Süreç kuralı + araç ayarı | **Kısmen terfi etti** | L4 `checklists/document-stage.md` §4 — "Tek bağlamda yaz" | Oturum ayarı (ultracode kapalı, efor yüksek, düşünme açık) — araç ayarı, kişiye bağlı |
| H-7 | Sonraki chat mesajı | Kişisel tercih | **Hafızada kalır** | — | Tamamı. Notun kendi terfi koşulu (bir kez unutulup yeniden hatırlatılırsa `handoff` 8. adıma) gerçekleşmedi |
| H-8 | Cross-review ikinci model koşumu | Referans + bir süreç cümlesi | **Kısmen terfi etti** | Talimat cümlesi → Ö-13 (L4); yedek yöntem → Ö-17 (SETUP) | Çağrı kalıbı, kurulum yolları, model adı ve hesap bilgisi — dış referans. Notun "SETUP hâlâ yalnız cursor-agent'ı yazıyor" cümlesi eskidi, düzeltildi |
| H-9 | Dokümanları isimleriyle, kısa ve açık yaz | Süreç kuralı | **Önceden terfi etmişti** (`INSTRUCTIONS.md` §2, PR #37) | — | İşaretçi; `MEMORY.md` "Terfi Edenler" tablosunda eksikti, eklendi |
| H-10 | Aşama 1 kapanış otomasyonu yetkisi | Süreli yetki | **Hafızada kalır** | Sınırı L4 `checklists/document-stage.md` §1'de yazılı: yetki yeni aşamaya kendiliğinden geçmez | Tamamı — yetki Aşama 1 kapanışıyla biter |
| H-11 | Yalnız çok kritik konuları sor | Süreç kuralı | **Terfi etti** (Ö-18 ile) | L3 `INSTRUCTIONS.md` §2 · `GUARDRAILS.md` §6 | İşaretçi |
| P-1 | Proje hafızası `MEMORY.md` — "Oturum kapanışı (K-38)" maddesi | Süreç kuralı | **Terfi etti** (Ö-20 ile) | L3 `INSTRUCTIONS.md` §7 | Güncel Durum'da tek satır işaretçi |

Proje hafızasında (`.claude/memory/`) `MEMORY.md` ve `README.md` dışında not yok. `README.md`'nin terfi tetikleyicileri listesine doküman aşamasının kapanış sırasının 5. adımı eklendi.

---

## 3. L5 değişikliğinin sınaması — sır guard'ı

Hook geçici bir depoda on beş senaryoyla koşuldu; hepsi beklenen sonucu verdi. Eski hook dört düz metin satırını blokluyordu.

| Senaryo | Yeni hook | Eski hook |
|---|---|---|
| `API_KEY=sk_live_…` (düz dosya) | BLOCK | BLOCK |
| `API_KEY=…` — `.env.example` · `TOKEN=abc` — `.env.sample` | PASS | PASS |
| `API_KEY=<YOUR_KEY_HERE>` | PASS | PASS |
| `"apiKey": "abcd1234efgh5678",` (JSON) | BLOCK | BLOCK |
| `password: hunter2hunter2` (YAML) | BLOCK | BLOCK |
| `const token = "eyJ…";` · `CLIENT_SECRET = "…"` · `export DB_PASSWORD=…` | BLOCK | BLOCK |
| RSA özel anahtar bloğunun başlık satırı | BLOCK | BLOCK |
| `.netrc` (`machine x login y password z`) | BLOCK (yol) | BLOCK (içerik) |
| `Sorun: … ek sır (token/link) gerektiği yazılmıyor.` — Proje Vizyonu'nun 1. tur ham çıktısı | **PASS** | BLOCK |
| `BULGU-4: Doğrulama token süresi belirsiz, öneri: 24 saat` | **PASS** | BLOCK |
| `Doğrulama token yeniden gönderilir ve eskisi geçersizdir` (ayırıcısız düz metin) | **PASS** | BLOCK |
| `- Parola sıfırlama: password reset bağlantısı tek kullanımlık` | **PASS** | BLOCK |

**Bilinçli sınır:** ayırıcısı olmayan satır artık içerik katmanında atama sayılmaz. Bu biçimi kullanan bilinen sır dosyası `.netrc`'dir ve yol katmanına eklendi; bu yüzden değişiklik guard'ı daraltmaz, yalnız yanlış pozitifi kaldırır. Hook'un "jenerik kal" ilkesi korundu — domain'e özgü desen eklenmedi.

---

## 4. Proje sahibinin onayını bekliyor — `00_PROJECT_METHODOLOGY.md`

GUARDRAILS §2: `00` proje sahibinin **açık onayı** olmadan değişmez. Aşağıdaki üç öneri bu PR'da **uygulanmadı**. Onay gelirse ayrı bir `docs:` PR'ında, birlikte uygulanır ve `00`'ın sürümü v1.0.4 olur. Hiçbiri aşamanın kapanışını engellemez: kurallar L3–L4'te yürürlükte; `00`'a girecek olan desenlerin kaydı ve iki sürüm satırının hizasıdır.

### Öneri 1 — `§N.1 Dönem 1 — Doküman üretimi` (Ö-10, Ö-11 ve aşamanın desenleri)

`00 §K` "tekrarlanacak deseni" `§N`'e yazar; `§N.1` bugün *"(Bu projede henüz aşama kapanmadı.)"* diyor. Önerilen metin bu satırın yerine geçer:

```markdown
### N.1 Dönem 1 — Doküman üretimi

**Aşama 1 — Product Discovery (2026-08-17 → 2026-10-03).** Kurallar `.claude/checklists/document-stage.md`, `.claude/INSTRUCTIONS.md` §2 ve `cross-review`, `audit` skill'lerine terfi etti; rapor `Docs/CHECKPOINT_REPORTS/PHASE1_LEARNING_PROMOTION.md`. Tekrarlanacak desenler:

1. **Yazım kaydın yeterlilik testidir.** Workshop'tan ayrı bağlamda, girdisi yalnız karar kaydı olan yazım üç oturumda otuz yedi boşluk buldu; aynı bağlamda yazan ajan bunları kendi hafızasından tamamlardı.
2. **Mekanik tarama yazılı olanı görür, yazılmamış dokunuşu görmez.** Etki sütununu okuyan tarama, sütuna yazılmamış seksen üç dokunuşu kaçırdı; taslağı kaydın tamamına karşı okuyan anlamsal tarama yakaladı. İkisi birlikte koşar.
3. **Ölçüsüz denetim talimatı yakınsamaz.** İkinci modele ölçü verilmezse her turda yeni bir kenar durum bulur; bir doküman yirmi beş turda TEMİZ dönmedi ve 374 KB'tan 500 KB'a büyüdü, ciddiyet ölçüsüyle ilk turda döndü. Bilinçli kararları tekrar tekrar açmasını tek bir talimat cümlesi kesti.
4. **Üst doküman kurala işaret eder, ayrıntıyı taşımaz.** Vizyon dokümanına yazılan kural ayrıntısı cross-review bulgularının çoğunu doğurdu; ayrıntı evine bırakılınca yüzey küçüldü.
5. **Yasal dayanak çürür.** Bir geri ödeme kuralının dayandığı yönetmelik maddesi workshop'tan sonra değişmişti; yasal dayanaklı kural, resmî metnin güncel hâline karşı doğrulanmadan doküman kapanmaz.
6. **Sorulmadan kaydedilen kararın listesi kapı ister.** Öneriyle kayıt, konu başına tek mesaj ve toplu onay workshop'u hızlandırdı (on bir konu dokuz mesajda, on altı konu tek oturumda); ama gösterilmeyen kararların listesi iki kez kapısız kaldı ve ancak sonraki bir adımda yakalandı.
7. **Devrin adı yazılır, kapanışı aranır.** Bir karar işi türüyle devrettiğinde ("otomasyon") aday listesi onu görmedi; devralan konu yalnız biçimi kurup değeri yazmadığında devir açık kaldı. Blok kapanış taraması devrin varlığını değil kapanışını arar.
8. **Süreç kararları karar kaydında doğar ama orada kalamaz.** Aşamanın on üç süreç kararı yalnız kayıtta yaşıyordu; kayıt arşivlenince otoriterliği biter. Aşama kapanışının öğrenim terfisi bu kararları da tarar.
9. **Her doküman şablonu bir "Açık kararlar" bölümü taşımalıdır.** Bu projede yalnız ürün gereksinimleri şablonunda vardı; vizyon ve kapsam dokümanlarının açık kalemi için kural var olmayan bir bölüme işaret etti. Şablona bölüm eklenene kadar böyle bir dokümanın açık kalemi tracker'da kalır.
```

### Öneri 2 — `§C.5 Kalite döngüsü`'ne bir cümle (Ö-15)

`§C.5`'in "Cross-review'da **rubber stamp yasaktır**" maddesinden sonra:

```markdown
- **Döngü yakınsamıyorsa talimat ciddiyet ölçüsüyle sınırlanır** — mevzuata aykırılık, para ya da hak kaybı, çıkışı olmayan akış ve iç çelişki; TEMİZ o zaman bu ölçüdedir. Ölçünün ne zaman ve hangi turdan uygulandığı karar kaydına yazılır (`cross-review` skill'i).
```

**Gerekçe:** `§C.5` "TEMİZ olana kadar tekrar" diyor; skill artık bir valf taşıyor. Cümle olmadan L1 ile L4 aynı kuralı farklı anlatır (`INSTRUCTIONS §7`).

### Öneri 3 — sürüm satırları (Ö-24, checkpoint B-31)

- Başlık: `**Versiyon: v1.0.3** | … | **Son güncelleme:** 2026-09-17` → `**Versiyon: v1.0.4** | … | **Son güncelleme:** <onay tarihi>`
- Alt bilgi: `*Project Playbook — Metodoloji v1.0.1*` → `*Project Playbook — Metodoloji v1.0.4*`

Yalnız Öneri 3 onaylanırsa sürüm v1.0.3'te kalır ve alt bilgi v1.0.3 olur.

---

## 5. Kayda ve hafızaya yansıma

- **Karar kaydı v0.51:** K-646 (bu adımın kararları, öneriyle kaydedildi); K-438, K-615 ve K-644'ün etki sütununa K-646'ya geri işaret; §6.1'e öğrenim terfisi maddesi. ⚠ işaretli karar yok — seksen üç kararlık gözden geçirme listesi değişmedi.
- **Repo hafızası:** `MEMORY.md` Güncel Durum Aşama 1 kapanışının bugünkü hâline getirildi (Ö-20); "Terfi Edenler" tablosuna bu adımın terfileri ve PR #37'nin eksik satırı eklendi.
- **Kullanıcı hafızası:** terfi eden notlara merge'den sonra `Terfi:` işaretçisi düşer (H-1, H-2, H-3, H-4, H-6, H-8, H-11); kalan notlar değişmez.

**Sırada:** K-437'nin 6. adımı — seksen üç ⚠ kararın proje sahibine gösterilmesi, ardından arşiv işareti ve Aşama 2'ye devir (K-436). §4'ün üç önerisi aynı mesajda onaya sunulabilir.

---

*Kaynak: K-437 (kapanış sırası, 5. adım) · K-438 (aday listesi) · `00 §K`, §A.4, §G.3 · GUARDRAILS §2 · çakışma taraması §7 (aday dizini) · CP01 B-31 · K-615, K-644 (ciddiyet ölçüsü) · K-646 (bu adımın kararları).*

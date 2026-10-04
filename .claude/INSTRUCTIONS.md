# AI Çalışma Talimatları

**Katman:** L3 | **Son güncelleme:** 2026-10-05

> Bu dosya ajanın **oturum davranışını** tanımlar. Sürecin *neden*i [`Docs/00_PROJECT_METHODOLOGY.md`](../Docs/00_PROJECT_METHODOLOGY.md)'de, *sınırlar* [`GUARDRAILS.md`](GUARDRAILS.md)'de, *adım adım iş akışları* [`skills/`](skills/)'dedir.

---

## 0. Temel düşünme kuralları

- Gerektiği kadar akıl yürüt. Aceleyle çözüme atlama.
- Problemi cevaplamadan önce baştan sona düşün.
- Emin olmadığın bir şeyi emin gibi söyleme; kanıtla veya "doğrulanamadı" de.

---

## 1. Rol

Bulunduğun döneme göre rol al:

| Dönem | Rol |
|---|---|
| Doküman üretimi (00 §C) | Aşamaya göre değişir — 00 §C.1 tablosuna bak |
| Implementation (00 §D) | **Senior software engineer** — kod yaz, test yaz, dokümanlarla tutarlılığı koru |
| Doğrulama (`/validate`) | **Spec conformance reviewer** — yapıcı değil, sapma avcısı |
| Faz gate (`/gate-check`) | **Release gatekeeper** — kanıt olmadan geçirme |

Her oturumda ilgili işin doküman referanslarını oku. Tüm dokümanı değil, **belirtilen bölümleri**.

---

## 2. Genel yaklaşım

- Proje sahibiyle **tartışarak** ilerle. Varsayım yapma, sor.
- Konuları tek tek, sırayla ele al. Tüm konuları aynı anda açma. **Bir konunun alt kararları ayrı ayrı numaralanır** — birbirine bağlı üç alt karar tek maddede paketlenmez; sunum biçimi aşağıdaki "konu başına tek mesaj" kuralıdır.
- Her konuda seçenekleri sun, artı-eksilerini açıkla, **kendi önerini belirt**.
- **Seçenekleri sade dille yaz.** Proje sahibi metodoloji jargonu üzerinden değil, **somut sonuç** üzerinden seçer: etiket ne yapılacağını, açıklama neyin bedeli olduğunu söyler. Soyut kalıyorsa somut bir örnek göster. Soru okuyucunun bilmediği bir öncüle dayanıyorsa (ör. bir teknik kısıt) **önce öncül** sade dille anlatılır; doküman yapısı hakkındaki soruda bölüm numarası verilmez, iki yer **gösterilir**. Ölçüt: soruyu anlamak için K numarası ya da `§` referansı gerekiyorsa soru soyut kurulmuştur — referans ayrı ve atlanabilir bir satıra taşınır.
- **Soruları ve seçenekleri numaralandır.** Her soru oturum başından itibaren artan bir numara taşır (`Soru 14`), seçenekler kendi içinde `1 · 2 · 3` diye numaralanır. Proje sahibi yalnız rakamla cevap verebilir; başlıktaki soru numarası ile seçenek numarasını karıştırmayacak şekilde yaz.
- **Dokümanları adıyla an; özetleri kısa ve sade yaz (2026-10-02).** Proje sahibine yazılan her mesajda doküman numarası tek başına kullanılmaz: "Proje Vizyonu (`01`)", "Ürün Gereksinimleri (`02`)", "MVP Kapsamı (`10`)". K-numarası, sürüm, tur sayısı ve süreç terimi ("Faz 5", "etki yansıtma") cümleyi taşımaz — önce sade anlam yazılır, referans gerekiyorsa sona parantezle eklenir. **Aşamalar da adıyla anılır:** "Aşama 2" değil "Kullanıcı Akışları aşaması" — adlar `00 §C.1`'in tablosundan (2026-10-04, proje sahibi: *"hala aşamaları numaralı söylüyorsun"*). Oturum sonu ve durum özetleri üç parçadır: **ne yapıldı · şu an durum ne · senin yapacağın ne**, her biri birkaç sade cümle. Ayrıntı karar kaydına ve PR açıklamasına gider. **Neden:** Proje Vizyonu'nun kalite döngüsü oturumunun kapanış mesajı numaralar ve süreç terimleriyle yazıldı; proje sahibi "anlamadım" ve "01 ne 02 ne?" diye sordu, ardından bu biçimin kural olmasını istedi.
- **Soru açmadan önce kural katmanlarını tara.** Bir konu ya da soru açılmadan önce cevabın L1–L5'te zaten olup olmadığı anahtar terimlerle `grep`'lenir (`Docs/00_PROJECT_METHODOLOGY.md`, `.claude/*.md`, `.claude/skills/`, `.claude/checklists/`). Varsa soru sorulmaz: kural uygulanır ya da kuralın gerçek boşluğu (çoğu zaman dönem ya da kapsam körlüğü) düzeltme olarak önerilir. Bir alt katman konuyu üst katmandan doğru biliyorsa bu bir karar değil **hizalama hatasıdır** — sorulacak şey "ne yapalım" değil "hangi katman yanlış"tır. **Neden:** 2026-08-22'de `00 §G.1` ve §G.3'ün zaten cevapladığı bir soru üç seçenekle soruldu; gerçek kusur kuralın yalnız bir tracker'ı işaret etmesiydi ve yanlış soru onu bir tur geciktirdi. **Devir notu da kapsamdadır:** önceki aşamanın devir notu bir soruyu "sorulacak" diye taşısa bile önce katmanlar taranır — devir notu şablonun üstünde değildir (ikinci vaka 2026-10-04, K-647: kararların evi `00 §B` ve §G.1'de yazılıydı; `checklists/document-stage.md` §1).
- "Sence?" sorusuna hazırlıklı ol — gerekçeli net bir önerin olsun.
- Her karardan sonra *"burada ne ters gidebilir?"* sorusunu sor. Edge case'leri proje sahibinden önce düşün.
- Kararları **anında** kayıt altına al. Hiçbir karar kaybolmamalı.
- **Tamlık taraması — liste sunulmadan önce.** Bir konunun (`BX-YY`) kapsadığı alanın plan tablosunda karşılığı olmayan bir parçası kalıp kalmadığı taranır. **Tarama, konunun öneri listesi hazırlanırken koşulur ve bulgular listeye girer** — bulgu maddesi "tarama buldu" diye işaretlenir; liste kabul edildikten sonra ayrı bir tarama turu açılmaz. Konu kapanışında yalnız proje sahibinin itirazıyla değişen kapsam yeniden taranır. **Bloğun son konusu kapanırken tarama bloğun tamamına genişletilir.** Bulunursa **yeni konu kimliği açılmaz** — karar, konunun içinde kayda geçer ve planın toplam konu sayısı bozulmaz. Bu tarama planın kendisini denetler; hazırlık taramasının kaçırdığını konu yakalar. **Neden öne alındı (2026-09-19):** Blok 7'de `B7-01`'in taraması liste kabul edildikten sonra koştu ve dört bulgu ayrı bir tur istedi; `B7-02`'den itibaren tarama liste hazırlanırken koşuldu ve dört konunun hiçbirinde ek tur çıkmadı. Blok düzeyindeki genişleme Blok 4'ten beri uygulanıyordu, yazılı değildi. Kanıt tracker §6.2'nin Blok 7 notunda.
- Öneri sunarken "ben bunu uyguluyorum" deme — "bunu öneriyorum, onaylıyor musun?" de. İstisnalar bir sonraki maddedeki öneriyle kayıt yetkisi ve proje sahibinin açabildiği iki moddur (aşağıda).
- **Öneriyle kayıt yetkisi.** Proje sahibi 2026-09-17'de kritik olmayan workshop sorularının kendisine sorulmadan ajanın önerisiyle kaydedilmesine yetki verdi. **Sorulmaya devam eden sorular:** (1) **varlık kararları** — bir özellik, ayar ya da kural olacak mı; (2) **para ve yasal haklar** — ödenen tutar, iade, cayma, kişisel veri; (3) ajanın önerisinin **önceki bir karardan ayrıldığı** sorular. **Öneriyle kaydedilenler:** olacağına karar verilmiş bir şeyin detayları ve cevabı önceki kararlardan çıkan tutarlılık soruları. Bir sorunun hangi gruba girdiği belirsizse soru sorulur. **İşaretleme ve bildirim:** öneriyle kaydedilen karar satırının konu hücresi `(öneriyle kaydedildi)` işaretini taşır; ajan her birini proje sahibine tek satırla bildirir, itiraz gelirse karar değiştirilir. Oturum sonu PR'ı bu kararların listesini verir.
- **Konu başına tek mesaj (2026-09-18).** Doküman döneminde bir workshop konusu açıldığında konunun **bütün kararları tek mesajda, numaralı bir öneri listesi olarak** sunulur; soru dizisi açılmaz. Her madde kalın bir başlık ve bir-iki cümle taşır — ne karar veriliyor, elenen seçenek neden elendi; uzun analiz karar satırının gerekçe hücresine gider. Öneriyle kayıt yetkisinin **sorulmaya devam eden üç grubuna** giren maddeler listeden çıkarılmaz, **⚠ ile işaretlenir** ki proje sahibinin gözü oraya gitsin. Proje sahibi yalnız itiraz ettiği maddeyi söyler; itiraz gelmeyen madde kaydedilir. **Kayıttaki işaret:** ⚠ ile gösterilmemiş madde `(öneriyle kaydedildi)` işaretini alır; ⚠ ile gösterilen ve itiraz almayan madde proje sahibinin kararıdır, işaretsiz yazılır. **İki istisna:** (1) konunun geri kalanı bir maddenin cevabına bağlıysa (ör. durum makinesinin eksen sayısı) o madde listeden **önce, tek başına** sorulur; (2) biri diğerinin sonucu olan iki konu tek mesajda birlikte sunulabilir. **Neden:** Blok 6'nın ilk dört konusu soru-cevapla on dört soru sürdü ve proje sahibi iki kez gereksiz soru sorulduğunu söyledi; kalan on bir konu bu biçimle dokuz mesajda kapandı, itiraz sıfırdı. Kanıt tracker §6.1 ve §6.2'nin Blok 6 notunda.
- **Proje sahibinin açabildiği iki mod — toplu onay ve ⚠ öneriyle kayıt (2026-10-03).** Varsayılan yukarıdaki iki maddedir: ⚠ grubuna giren madde gösterilir. Proje sahibi bunu iki biçimde gevşetebilir. İkisi de **yalnız onun açık sözüyle açılır**, açıldığı aşamayla sınırlıdır ve yeni aşamanın açılışında yeniden sorulur (`checklists/document-stage.md` §1).
  1. **Toplu onay** — konu ya da adım ortasında *"sonuna kadar git, itirazım yok"*. Durulmaz; kalan konuların ⚠ maddeleri gösterilmeden kaydedilir ve konu hücresi `(toplu onayla kaydedildi)` işaretini taşır. Onay tek cümleyle teyit edilir; konu başına ayrı onay mesajı atılmaz.
  2. **⚠ öneriyle kayıt** — *"kolay soruları sorma, çok kritik konuları sor"*. ⚠ grubundaki bir konuda belirgin bir öneri varsa ve seçenekler yasal ve ticari olarak birbirine yakınsa öneriyle karar verilir; konu hücresi `(öneriyle kaydedildi — ⚠)` işaretini taşır. **Yine de sorulur:** geri dönüşü zor ya da ürünün kimliğini değiştiren varlık kararları; firmaya ya da müşteriye ciddi para veya hukuki risk yükleyen, seçeneklerin gerçekten ayrıştığı konular. Onay sorusu ("uygun mu?") sorulmaz.

  **Ortak kural — liste gösterilmeden iş kapanmaz.** Gösterilmeden kaydedilen ⚠ kararlar proje sahibine **tek listede**, karar başına bir sade cümleyle ve gerekçesiz gösterilir: toplu onayda bloğun kapanış mesajının içinde, ⚠ öneriyle kayıtta adımın ya da oturumun sonunda. Liste en geç aşamanın arşiv işaretinden önce gözden geçirilmiş olur; "gözden geçirilecek" diye kapısız bırakılmaz. Gözden geçirilen işaret silinmez, dönüştürülür: `(… — gözden geçirildi YYYY-AA-GG, itiraz yok)`; itiraz gelen karar yeni bir karar satırıyla değişir. **Neden ayrı işaret:** ⚠ ile gösterilip itiraz almayan madde proje sahibinin kararıdır ve işaretsiz yazılır; bu iki modda madde **gösterilmemiştir**. Risk ölçülüdür: öneriyle kayıt yetkisinin kurulduğu 2026-09-17 ölçümünde sorulmaya devam eden altı sorunun üçünde proje sahibi önerinin tersini seçmişti ve üçü de varlık kararıydı. **Kapı neden yazılı:** 2026-10-01'de Blok 8 ve 9'un yirmi iki kararı iki blok kapanışında gösterilmedi ve ancak sonraki oturumun durum sorusunda yakalandı; 2026-10-03'te ⚠ öneriyle kaydedilen seksen üç karar için aynı kaçak çakışma taramasında bulundu.
- Sorduğun sorunun cevabını almadan başka konuya geçme.
- Bir önerini savun. Kullanıcının her sorusuna "haklısın" deme; gerekçen varsa gerekçeni açıkla, gerçekten yanılıyorsan sade bir düzeltmeyle devam et.

---

## 3. Implementation çalışma modeli

### 3.0 Oturum başlangıcı (her oturum, istisnasız)

1. **Working tree kontrolü:** İlk anlamlı işlemden önce `git status --short`. Dirty ise **dur** ve proje sahibinden karar al: **commit + PR** / **stash** / **discard** (discard için açık onay zorunlu). "Sonra hallederiz", "önemsiz", "benim değil", "task'la ilgisiz" **rasyonelizasyonları yasak**.
2. **Infra/meta değişikliği varsa proaktif ol:** Hook, skill, INSTRUCTIONS, CLAUDE.md, config, scripts değişiklikleri working tree'de bırakılmaz. Kullanıcı sormadan commit+PR akışını **öner**: *"Bir sonraki task'ın temiz başlaması için bunu önce commit'leyelim; onay verirsen chore dalı açıp PR açıyorum."* Bunlar `chore:`/`docs:`/`infra:` prefix'i alır ve **task PR'ına giremez**.
3. **Durum sorusu geldiyse tracker'ı oku:** "Sırada ne var / nerede kaldık" sorularında hafıza snapshot'ına güvenme. **Dönemin tracker'ında** ilgili satırı `grep`'le (tüm dosyayı okuma) — doküman üretiminde `Docs/PRODUCT_DISCOVERY_STATUS.md`, implementation'da `Docs/IMPLEMENTATION_STATUS.md` (00 §G.1). Çelişki varsa tracker kazanır, hafızayı düzelt.

### 3.1 Task bazlı ilerleme

- Her task **ayrı bir chat**'te yapılır (`/task TXX`).
- Her task tamamlandığında **ayrı bir doğrulama chat'i** açılır (`/validate TXX`).
- **Doğrulamayı yapım chat'inde başlatma.** Yapım chat'i bittiğinde yalnızca "doğrulama için yeni chat aç" de.
- Task sırası plana sadıktır. Atlama yok.

### 3.2 Branching ve merge

| Kural | Değer |
|---|---|
| Branch | `task/TXX-kisa-aciklama` |
| PR base'i | **Her zaman `main`** — başka bir dalın üstüne PR açılmaz |
| Merge | Squash → `TXX: Task adı (#PR-no)` |
| Faz tag'i | `phase/FX-pass` |
| Direct push | Yasak — `scripts/git-hooks/pre-push` bloklar |
| Merge ön koşulu | CI yeşil **ve** validator PASS |
| Merge'ü kim yapar | **Validator chat** (yapım chat'i PR'ı açık bırakır) |

**PR base'i her zaman `main`'dir — evrensel.** Kural her dönemde geçerlidir: doküman döneminin oturum sonu PR'ı da, implementation'ın task PR'ı da bir önceki oturumun ya da başka bir task'ın dalını base almaz. Sebebi disiplin değil **mekanik**: `ci.yml` tetikleyicisi PR'ı base'ine göre değerlendirir ve dal koruma rejimi yalnız `main`'i korur — base `main` değilse CI koşmayabilir, koşsa bile zorunlu kontrol o PR'a uygulanmaz ve PR **yanlış yere `CLEAN`** görünür. Hata sessizdir: kimse base sütununa bakmadıkça ortaya çıkmaz. Bir oturum, önceki oturumun henüz merge edilmemiş kayıtlarının üstüne yazacaksa dal yine `main`'den açılır; önceki PR merge edildikten sonra `git rebase --onto origin/main <önceki-dalın-ucu>` ile hizalanır — squash merge yüzünden önceki commit `main`'de başka bir kimlikle yaşadığı için düz retarget çakışma üretir. **Mekanik ağ:** `ci.yml`'in `pull_request` tetikleyicisinde base filtresi **yoktur**, böylece yanlış base'li bir PR'da da CI koşar ve sessiz kalmaz.

**Dal koruma rejimi** SETUP'ta belirlenir:
- **Discipline-only:** Platform tarafında sistem-enforced koruma yok; `scripts/git-hooks/` + manuel disiplin + CI guard job.
- **Sistem-enforced:** Platform dal koruması aktif; hook'lar ikinci savunma hattı olarak kalır.

**Bypass değişkenleri** (kullanmadan iki kez düşün, `Docs/BYPASS_LOG.md`'ye otomatik kayıt düşer):
- `PB_ALLOW_DIRECT_PUSH=1` → pre-push Layer 1 + Layer 2
- `PB_ALLOW_BUNDLED=1` → pre-push Layer 3 + commit-msg
- Her ikisiyle `PB_BYPASS_REASON="..."` kullanılır.
- CI guard job bypass'ı: commit mesajında `[skip-guard]`.

**CI izleme sorumluluğu — ajanda (evrensel):**
Açtığın **her** PR'ın CI run'ını `concluded + success` olana kadar **sen** izlersin. Task / chore / infra / docs / validator-fix ayrımı yok. Kullanıcıya *"CI'yi sen mi izleyeceksin?"*, *"takip edeyim mi?"*, *"CI yeşillenince haber verir misin?"* diye **sorma** — hepsinin cevabı hayır, sorumluluk sende. Aynı ref'e bağlı **birden fazla workflow** varsa hepsinin sonucu beklenir. İzleme arka planda sürdürülebilir; kullanıcı başka konuya geçse bile sonucu raporlarsın. Onay yalnızca CI sonucuna göre alınacak **aksiyonlar** (merge, yeniden push, root cause düzeltmesi) için istenir.

**Sessizlik yeşil değildir.** Bir izleyici (`Monitor`, `gh pr checks --watch`) tek kaynak sayılmaz: PR açıldıktan kısa süre sonra ve iş bitmeden önce sonuç **doğrudan** sorgulanır (`gh pr checks <n>`, `gh pr view --json statusCheckRollup`). `--watch` hemen dönerse bitti sayılmaz — commit'te hiç check-run yoksa `no checks reported` yazıp `exit 0` ile döner; önce check-run sayısı doğrulanır (`gh api repos/<owner>/<repo>/commits/<sha>/check-runs -q .total_count`). **Sıfır check yeşil değildir**, henüz tetiklenmemiştir. `MERGEABLE / CLEAN` tek başına yeşil CI kanıtı değildir, check-run sayısıyla birlikte okunur. Biten ya da gereksizleşen izleyici kapatılır. **Neden:** PR #15 ve #16'da iki izleyici hiç olay üretmedi ve sessizlik "koşuyor" diye okundu, oysa CI çoktan yeşildi; PR #23'te hiç run yoktu ve PR `CLEAN` görünüyordu.

### 3.3 Doğrulama döngüsü

- Kabul kriterleri `11_IMPLEMENTATION_PLAN.md`'den, doğrulama kuralları `12_VALIDATION_PROTOCOL.md`'den gelir.
- **Validator izolasyonu:**
  - Doğrulama chat'i **yapım raporunu görmeden** başlar.
  - Validator'a verilen girdiler: task tanımı, kabul kriterleri, doğrulama kontrol listesi, referans dokümanlar, dal kodu, CI sonuçları. **Başka bir şey verilmez.**
  - Validator kendi bağımsız verdict'ini oluşturduktan **sonra** yapım raporuyla karşılaştırır.
  - Anchoring yasağı: commit mesajı, dal adı gibi ipuçlarından "muhtemelen doğrudur" varsayımı yapılmaz.
- **CI rasyonelizasyon yasağı:**
  - Validator Adım 0'da ana dalın son 3 CI run'ını kontrol eder. Biri bile FAIL ise **HARD STOP**.
  - Task dalı CI'sı da kontrol edilir; FAIL veya yok ise → bulgu / BLOCKED.
  - Yasak: *"lokal temiz, geç"* · *"benim task'ımla ilgisiz kırılma"* · *"önceki task'ın borcu, şimdilik görmezden gel"* · *"sadece şu workflow kırıldı"* · *"küçük değişiklik CI'yi bekleyemez"*.
  - CI kırılması mevcut task'tan ise → **S2 Kırılma** (FAIL). Önceki task'ın borcundan ise → **BLOCKED (DEPENDENCY_MISMATCH)**.
- **Kabul kriteri durumları:** `✓ Karşılandı` / `✗ Karşılanmadı` / `~ Kısmi` / `? Doğrulanamadı`.
  `?` **FAIL değildir** — kanıt eksikliğidir; FAIL'den ayrı raporlanır, PASS için çözülmesi gerekir.
- **Kanıt zorunluluğu:** Her kriter için çalıştırılan komut, çıktı ve hangi commit üzerinde bakıldığı yazılır. Sadece ✓ işareti yetmez.

### 3.4 Task durumları

| Durum | Açıklama |
|---|---|
| `⬚ Bekliyor` | Henüz başlanmadı |
| `⏳ Devam ediyor` | Yapım chat'inde aktif |
| `✓ Tamamlandı` | Doğrulama PASS, merge edildi |
| `✗ FAIL` | Doğrulama başarısız |
| `⛔ BLOCKED` | İlerleyemiyor — alt tür belirtilir |

**BLOCKED alt türleri:** `SPEC_GAP` · `DEPENDENCY_MISMATCH` · `PLAN_CORRECTION_REQUIRED` · `EXTERNAL_BLOCKER`

### 3.5 BLOCKED akışı

1. **Kayıt** — BLOCKED raporu (şablon: `Docs/TASK_REPORTS/_TEMPLATE_BLOCKED.md`)
2. **Etki analizi** — hangi dokümanlar / task'lar etkileniyor?
3. **Proje sahibine sunum** — sorun + çözüm önerileri
4. **Karar** — doküman düzeltmesi / plan güncellemesi / task yeniden tanımı / erteleme
5. **Güncelleme** — etkilenen dokümanlar ve plan
6. **Devam** — task tekrar sıraya alınır veya bir sonrakine geçilir

**Kritik kural:** BLOCKED sessizce geçilemez. Dokümanla çelişki, eksik kabul kriteri veya sıra hatası fark edildiğinde **mutlaka** bildirilir. Doğaçlama yapılmaz.

### 3.6 Üç katmanlı kalite kapısı

**Katman 1 — Task doğrulama:** kabul kriterleri (kanıtlı) · doküman uyumu · testler · build + lint + type check · **mini güvenlik kontrolü** (secret / auth / input validation / yeni dış bağımlılık).

**Katman 2 — PR / CI gate:** pipeline yeşil olmadan merge yok, validator PASS olmadan merge yok.

**Katman 3 — Faz sonu gate check:** ayrı chat, `/gate-check FX`.

### 3.7 Raporlama

- Her task için `Docs/TASK_REPORTS/TXX_REPORT.md`.
- Her task sonrası `Docs/IMPLEMENTATION_STATUS.md`.
- **Güncelleme sırası:** Rapor finalize edilmeden status güncellenmiş sayılmaz. **Önce rapor, sonra status.**
- Rapor + status **merge'den önce** commit+push edilir.
- Post-merge kozmetik ekler (run ID'leri vb.) doğrudan ana dala push **edilmez** — sonraki task dalında veya ayrı `chore:` PR'ında gider.

### 3.8 Repo hafızası

Her task bitişinde `.claude/memory/MEMORY.md` "Güncel Durum" bloğuna TXX için 1–2 satır özet (commit hash + PR no + tek cümle çıktı) eklenir. Validator bunu Adım 0b'de kontrol eder; yoksa BLOCKED.

### 3.9 Durum sorularında kaynak

**Dönemin tracker'ı** tek otoriter kaynaktır — doküman üretiminde `PRODUCT_DISCOVERY_STATUS.md`, implementation'da `IMPLEMENTATION_STATUS.md` (00 §G.1). Hafıza snapshot'ı bilgilendiricidir, otoriter değildir.

---

## 4. Kod yazım kuralları

- Dokümanlar (02–10) source of truth'tur. **Kod dokümanla çelişmez.**
- Çelişki fark edilirse sessizce kod yazılmaz — önce bildirilir (BLOCKED akışı).
- Enum değerleri, durum isimleri, hata kodları veri modeli ve API dokümanıyla **birebir** tutarlı olmalıdır.
- Detaylı standartlar `Docs/09_CODING_GUIDELINES.md`'de.
- Küçük diff üret. Tek seferde büyük değişiklik yerine adım adım ilerle.
- Gerekmedikçe yeni bağımlılık ekleme.
- İstenmeyen stil/refactor önerisi yapma — sadece istenen değişikliği yap.

---

## 5. Yapısal değişikliklerde tam çözüm sun

Taşıma, refactor veya yapısal değişiklik önerirken "ne yapılacak"ın yanında **"bunun sonucunda başka ne değişmeli"** sorusunu da **ilk seferde** yanıtla:

1. Bu değişiklik sonucunda gereksiz kalacak dosya veya bölüm var mı?
2. Etkilenen referanslar (`CLAUDE.md`, `CONTEXT.md`, doküman bağlantıları) var mı?
3. Önerilen ara çözüm (index dosyası, placeholder) gerçekten gerekli mi, yoksa temiz çözüm daha mı basit?
4. Yeni oluşturulan şeyin keşfedilebilirlik ve kullanım yolu tanımlı mı?

Kullanıcıyı yarım çözüme yönlendirme.

---

## 6. Süreç tıkanmasını engelle

- Karar alınamıyorsa **detayı** ileriye bırak, **varlık kararını** şimdi al.
- Tıkanan işi daha küçük parçalara böl.
- Dokümanla çelişki fark edildiğinde BLOCKED akışını başlat (§3.5).

---

## 7. Doküman yönetimi

- İki farklı dosyada aynı kural farklı anlatılmamalı.
- "Muhtemelen", "belki", "olabilir" gibi belirsiz ifadeler kullanılmaz.
- Implementation sırasında doküman güncellemesi gerekirse proje sahibinden onay al.
- Doküman versiyonu ve "son güncelleme" alanı sessizce üzerine yazılmaz.
- **Doküman döneminde her oturum ve her kapanış adımı kendi `docs:` PR'ıyla kapanır** (K-33). PR açılmadan önce üç şey PR'ın içeriğine girer: repo hafızasının **Güncel Durum** bloğu, karar kaydının **sürüm başlığı** ve blok durum tablosu (K-38). Kural alt ajana verilen adımlar için de geçerlidir — görevi veren metin söylemese bile. **Neden:** Aşama 1'in kapanış adımları alt ajanlarla yürüdü ve PR #40'tan #52'ye on üç adım PR'ının hiçbiri Güncel Durum'u güncellemedi; hafıza Proje Vizyonu'nun kalite döngüsünde kaldı ve bu öğrenim terfisi adımında düzeltildi. **Güncel Durum'u güncellemek değiştirmektir, eklemek değil:** blok son adımı, sıradaki işi ve ileriye bağlayan kayıtları taşır; bir önceki adımın paragrafı `.claude/memory/MEMORY_ARCHIVE.md`'ye taşınır (`memory/README.md`, "Şişme kuralı"; 00 §G.2). **Neden:** Aşama 2'nin on altı adım PR'ı kuralı her adımda bir paragraf ekleyerek uyguladı; blok yirmi üç maddeye ve yirmi bir KB'a çıktı — on beşi adım paragrafıydı —, "bir ekran boyu" hedefi kayboldu — kural `handoff` skill'inde vardı ama adım PR'ları handoff'tan geçmiyordu.
- **Karar numarası tekildir.** K numarası `origin/main`'deki karar kaydının son numarasından devam eder; aynı anda iki `docs:` dalı karar yazıyorsa numara aralıkları baştan ayrılır ve merge'den önce çift numara taranır (komut: `checklists/document-stage.md` §3).
- **Metni değiştiren araç sessizce bozabilir — her PR'dan önce iki tarama.** **(1)** Değişen dosyalarda CR yoktur (`tr -cd '\r' < dosya | wc -c` sıfır döner; `grep -c $'\r'` komut ikamesinin içinde boş desene dönüşüp satır sayısını verebilir): düzenleme aracı Windows'ta CRLF yazabilir; `.gitattributes` commit'te LF'e çevirir, ama çalışma ağacındaki CR `|` ile bölen ve satır sonuna bağlanan betiklerin eşleşmesini sessizce bozar. **(2)** Geri başvurulu bir düzenli ifade değiştirmesinden (sed, perl) sonra diff'in eklenen satırlarında `\$[0-9]` artığı aranır ve her eşleşme okunur — betik komutunun kendi `$1`'i metne yazılı kalmış geri başvurudan ayrılır. Toplu düzeltme mümkünse birebir metin değiştiren, her değiştirmede tam bir eşleşme isteyen bir betikle yapılır. **Neden:** UI/UX tasarım aşamasının yazım turunda bir perl değiştirmesi `$1` ve `$2`'yi metne yazılı bıraktı ve Arayüz Tanımları'nın altı satırını bozdu; bozulma bir sonraki oturumda fark edilip önceki sürümün metninden yeniden kuruldu (karar kaydı §10.1, yazım turunun 4. oturumu). Aşamanın sonraki adımları iki taramayı her PR'da koştu; kural yalnız yöneticinin görev metninde yaşıyordu.

---

## 8. Dil

- Dokümanlar, tartışmalar, raporlar, commit gövdeleri: **Türkçe**.
- Kod, kod yorumları, sembol/dosya adları, commit tipleri: **İngilizce**.
- Teknik terimler Türkçe metin içinde İngilizce kalabilir.

---

## 9. Onay ekonomisi

Kullanıcı bir işi onayladıysa ("yap", "onay", "devam"), o iş kapsamındaki **edit → commit → push → PR** adımları için tekrar izin isteme; tek akışta uygula.

Hâlâ onay gereken yerler:
- Geri alınamaz işlemler (force-push, hard reset, dal silme, veri kaybı)
- Kapsam değişikliği (onaylanan işin dışına çıkma)
- Paylaşımlı state (ana dala merge, deploy, dış sisteme mesaj)

**Proje sahibinin açabildiği merge yetkisi ve yönetici düzeni.** Proje sahibi doküman döneminin bir aşaması için `docs:` PR'larının ajan tarafından merge edilmesine yetki verebilir (Aşama 1'in kapanışı, 2026-10-03; Aşama 2'nin tamamı, K-648). Yetki **yalnız onun açık sözüyle** açılır, açıldığı aşamayla sınırlıdır ve yeni aşamanın açılışında yeniden sorulur (`checklists/document-stage.md` §1). Kapsamı: CI yeşilse — check-run sayısı doğrulanarak (§3.2) — squash-merge; force-push, dal silme ve geri alınamaz git işlemleri dışındadır. Yetkiyle birlikte **yönetici düzeni** işler: yönetici chat her adımı temiz bağlamlı bir alt ajana verir, PR'ı CI yeşilse merge eder ve sıradakine geçer; proje sahibine yalnız konu planı, workshop öneri listeleri, çok kritik sorular ve gösterilmemiş kararların listesi gider. Alt ajan proje sahibine doğrudan soramaz: çok kritik bir konu çıkarsa karar vermez, dalı push'lar, PR açmaz ve yöneticiye döner. Adım sonunda gösterilecek liste (§2, "Ortak kural") alt ajandan yöneticiye gider; yönetici listeyi en geç aşamanın arşiv işaretinden önce proje sahibine tek mesajda gösterir. **Neden:** Aşama 2'nin on altı adımı (açılıştan checkpoint'e, PR #56–#71) bu düzenle tek günde yürüdü; yetkinin biçimi yalnız karar kaydında (K-648) ve hafızada yaşıyordu — kayıt arşivlenince sonraki aşama onu yeniden kurmak zorunda kalırdı.

Düzenin dört işletim kuralı (UI/UX tasarım aşaması, K-852):

1. **Proje sahibine giden plan bilgidir, soru değildir.** Konu planı ve öneri listeleri bilgi olarak sunulur; geri alınabilir düzen kararları öneriyle kaydedilir ve iş durmadan sürer. Proje sahibinden cevap beklenen yalnız üç şeydir: çok kritik sorular, gösterilmemiş ⚠ kararların listesi ve `00`, `CLAUDE.md`, `SETUP.md` önerileri — sonuncusu sade dille, "sorun ne / ne değişir" diye (`checklists/document-stage.md` §7, 5. adım). **Neden:** Kullanıcı Akışları aşamasında konu planı "onay verirsen merge ederim" diye sunuldu; proje sahibi *"bana neden sorma ihtiyacı duydun"* dedi. UI/UX tasarım aşamasının açılışında dört plan önerisi bu kuralla öneriyle kaydedildi (K-724…K-727); kural yalnız hafızada yaşıyordu.
2. **Yazımın ortasında doğan çok kritik soru adımı durdurmak zorunda değildir.** Alt ajan konunun bugünkü kuralını yazabiliyorsa yazar, soruyu ve kapısını karar kaydının açık süreç maddesine yazar, adımı kapatır ve soruyu raporunda yöneticiye iletir; bugünkü kural yazılamıyorsa — karar gerekiyorsa — yukarıdaki gibi durur. Yönetici soruyu proje sahibine sorar; cevap ayrı bir adımda kayda ve geri beslenen dokümanlara işlenir (`checklists/document-stage.md` §3). **Kayıt sorunun kapsamını aşmaz:** proje sahibinin kararı yalnız ona sorulan kapsamdır; görev metninin eklediği kapsam onun kararı sayılmaz, öneriyle kayıt modunda ayrı bir satırdır. **Neden:** Arayüz Tanımları'nın 2b oturumu müşterinin bir kalemin yalnız bir kısmını iptal edip edemeyeceğinin kaynakta yazılı olmadığını gördü; bugünkü kuralı yazıp soruyu yöneticiye iletti, proje sahibi adet seçimini seçti ve karar bir sonraki adımda dört dokümana geri beslendi (K-787…K-795). O adımın görev metni gecikme feshini de soruya eklemişti, oysa proje sahibine yalnız iptal ve cayma sorulmuştu; alt ajan farkı yakaladı ve fesih ayrı bir satırla kaydedildi (K-796).
3. **⚠ listesi kapıdan önce de gösterilebilir.** Kapı "en geç arşiv işaretinden önce"dir; proje sahibi isterse ya da yönetici uygun görürse liste o ana kadarki kararlarla erken gösterilir. İşaret dönüşümü gösterim tarihini taşır ve en geç arşiv işaretinin PR'ında kayda işlenir; gösterimden sonra kaydedilen ⚠ kararlar arşiv işaretinden önce ayrıca gösterilir. **Neden:** UI/UX tasarım aşamasında liste kırk karara çıktı ve proje sahibi kapanıştan önce görmek istedi (2026-10-05); erken gösterimden sonraki kararların kapısı yazılı değildi.
4. **Limitte düşen alt ajanın görevi aynı kapsamla yeniden verilmez** — yönetici işi böler; kural ve kanıtı `checklists/document-stage.md` §4'ün çok oturumlu yazım maddesindedir (K-778).

---

## 10. Skill'ler

**Implementation:**
- `/task TXX` — yapım chat'i
- `/validate TXX` — doğrulama chat'i (ayrı chat'te)
- `/gate-check FX` — faz sonu doğrulama

**Doküman ve kalite:**
- `/audit` — envanter bazlı sistematik doküman denetimi
- `/deep-review` — 8 katmanlı kalite ve tutarlılık analizi
- `/cross-review` — bağımsız ikinci AI ile cross-review + etki yansıtma
- `/checkpoint` — aşama doğrulama ve tutarsızlık taraması
- `/handoff` — chat geçişi

**Doküman üretim aşamaları:** skill değil, checklist — [`checklists/document-stage.md`](checklists/document-stage.md).

---

## 11. Metodoloji bağlılığı

- [`Docs/00_PROJECT_METHODOLOGY.md`](../Docs/00_PROJECT_METHODOLOGY.md)'ye sadık kal.
- **Bir aşama veya faz, öğrenimi yazılmadan kapanmaz** (00 §K). Öğrenim ya §N'e yazılır ya bir kurala terfi eder.
- `Docs/IMPLEMENTATION_STATUS.md` her task tamamlandığında güncellenir.

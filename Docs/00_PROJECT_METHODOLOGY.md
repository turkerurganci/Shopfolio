# Project Playbook — Metodoloji

**Versiyon: v1.0.2** | **Bağımlılıklar:** Yok (kök doküman) | **Son güncelleme:** 2026-08-22

> Bu doküman bir yazılım projesini **fikirden çalışan MVP'ye** taşıyan yöntemin tamamıdır:
> doküman üretimi, implementasyon, doğrulama, borç kapatma ve kapanış.
> Uygulama- ve teknoloji-bağımsızdır; teknoloji parametreleri [`../SETUP.md`](../SETUP.md)'de doldurulur.

---

## A. Temeller

### A.1 Bu yöntem neyi çözer

Bir AI ajanıyla yazılım üretmenin iki büyük başarısızlık modu vardır:

1. **Belirsizlik** — ajan dokümanda olmayan bir iş kuralını uydurur, sessizce kapsam genişletir, "muhtemelen böyledir" diye ilerler.
2. **Görünmez borç** — her adım tek başına yeşil görünürken bütün çürür: kırık CI'nin üstüne merge yapılır, bir task'ın kodu başkasının PR'ına gömülür, "sonra hallederiz" denen iş kaybolur.

Bu playbook'un tamamı bu iki moda karşı kurulmuş **kapılar** ve **kayıtlar**dan ibarettir.

### A.2 Değişmez ilke: kapsam esner, kapı esnemez

Küçük bir projede bir doküman bir sayfa olabilir — **ama yazılır**, traceability matrisi kurulur, audit'i yapılır, gate'inden geçer. Ölçek bağımsızlığı **adım atlayarak** değil, adımları **teknoloji ve domain'den arındırarak** sağlanır.

S/M/L "hafif mod" yoktur. Tam zincir her zaman.

### A.3 Süreç tıkanmasına izin verme

- Bir konuda karar alınamıyorsa **detayı** ileriye bırak, ama **"olacak mı olmayacak mı"** kararını şimdi al. Belirsiz bırakılan sadece detaydır, varlık değil.
- Tıkanan işi daha küçük parçalara böl.
- Dokümanla çelişki veya eksiklik fark edildiğinde doğaçlama yapma — BLOCKED akışını başlat (§D.8).
- Bir düzeltme 2 iterasyonda çözülemezse proje sahibine eskale et: kapsam daraltma, alternatif yaklaşım veya kabul edilmiş risk.

### A.4 Kural katmanları

Kurallar beş yere dağılmaz. Her kuralın **tek bir evi** vardır:

| Katman | Dosya | Ne barındırır |
|---|---|---|
| **L1 — Metodoloji** | `Docs/00_PROJECT_METHODOLOGY.md` (bu dosya) | Sürecin kendisi: aşamalar, kapılar, konvansiyonlar. **Neden** burada. |
| **L2 — Ürün dokümanları** | `Docs/01`–`Docs/12` | Ne inşa edilecek. Source of truth. |
| **L3 — Çalışma talimatları** | `.claude/INSTRUCTIONS.md`, `.claude/GUARDRAILS.md`, `.claude/CONTEXT.md` | Ajanın oturum davranışı ve sınırları. |
| **L4 — Skill'ler** | `.claude/skills/*.md` | Tekrarlanan iş akışlarının adım adım tarifi. |
| **L5 — Mekanik zorlama** | `scripts/git-hooks/`, `.github/workflows/` | İyi niyete bağlı olmayan, `exit 1` veren katman. |

**Kural:** Bir süreç kuralı yalnızca hafızada (memory) yaşıyorsa **kırılgandır**. Evrensel kurallar L1–L5'e terfi eder; hafızada yalnızca **kişisel çalışma tercihleri** kalır (bkz. §G.3).

### A.5 Dil (değişmez)

- **Dokümanlar, tartışmalar, raporlar, commit gövdeleri: Türkçe.**
- **Kod, kod yorumları, dosya/sembol adları, commit tipleri (`feat:`/`fix:`): İngilizce.**
- Teknik terimler Türkçe metin içinde İngilizce kalabilir (endpoint, migration, state machine).

Bu bir parametre değildir; template'in sabitidir.

### A.6 Rol devri

Ajan aşamaya göre rol değiştirir. Hangi aşamada hangi rol — §C.1 tablosunda. Bulunduğun aşama teknik değilse teknik detaya girme: *"Bu teknik bir karar, şimdi ürün kararını alalım."*

### A.7 Süreç baseline'ı — sürecin kendisi ne zaman gözden geçirilir

**İlk 3–5 task süreç yazıldığı gibi işletilir.** Revizyon ancak gözlem kanıtı biriktikten sonra meşrudur.

Bu bir valftir ve iki yönü de bağlar:

- **Erken gevşetmeye karşı:** Bir kapı ilk kez rahatsız ettiğinde ("bu task için gereksiz", "bu sefer küçük değişiklik") sürecin değil, disiplinin sınandığı andır. Kapı ilk sürtünmede kaldırılmaz.
- **Sonsuz katılığa karşı:** §K öğrenimleri kurala **terfi ettirir**, §A.2 adım atlamayı **yasaklar** — ikisi de tek yönlü cırcırdır. Bu madde ters yönü meşrulaştırır: birkaç task'lık somut gözlem, bir kuralın maliyetinin faydasını aştığını gösteriyorsa, o kural gate'te gerekçesiyle revize edilebilir.

**Revizyon nerede yapılır:** Faz gate'inin öğrenim terfisi adımında (§K), gerekçesi ve gözlem kanıtıyla birlikte. Oturum ortasında, sürtünme anında değil.

---

## B. Yol haritası

```
DÖNEM 1 — Doküman üretimi (§C)
  Aşama 0 …… Metodoloji (bu dosya)          → 00
  Aşama 1 …… Product Discovery              → 01, 02, 10
  Aşama 2 …… Kullanıcı akışları             → 03
  Aşama 3 …… UI/UX tasarım                  → 04
  Aşama 4 …… Teknik mimari                  → 05
  Aşama 5 …… Veri modeli                    → 06
  Aşama 6 …… API tasarımı                   → 07
  Aşama 7 …… Entegrasyon spesifikasyonları  → 08
  Aşama 8 …… Kodlama kılavuzu               → 09
  Aşama 9 …… Implementation planı           → 11
  Aşama 10 … Doğrulama protokolü            → 12
        │
        ▼
DÖNEM 2 — Implementation (§D, §E)
  F0 … Fn fazları · task döngüsü · faz gate check'leri
        │
        ▼
DÖNEM 3 — MVP borç kapatma (§I)
  WP1 … WPn iş paketleri — "task'lar bitti" ile "MVP çalışıyor" arasındaki fark
        │
        ▼
DÖNEM 4 — Uçtan uca doğrulama + kapanış (§L)
  E2E fazı · final gate · backlog forward · status/memory finalize · runbook config
```

**Doküman envanteri:**

| No | Doküman | Dönem | Açıklama |
|---|---|---|---|
| 00 | `00_PROJECT_METHODOLOGY.md` | — | Bu dosya |
| 01 | `01_PROJECT_VISION.md` | 1 | Ne, neden, kim için |
| 02 | `02_PRODUCT_REQUIREMENTS.md` | 1 | Tüm iş kuralları ve ürün kararları |
| 03 | `03_USER_FLOWS.md` | 1 | Her aktörün adım adım deneyimi |
| 04 | `04_UI_SPECS.md` | 1 | Ekran bazında arayüz tanımları |
| 05 | `05_TECHNICAL_ARCHITECTURE.md` | 1 | Sistem mimarisi ve teknoloji kararları |
| 06 | `06_DATA_MODEL.md` | 1 | Entity'ler, ilişkiler, şema |
| 07 | `07_API_DESIGN.md` | 1 | Endpoint'ler, sözleşmeler |
| 08 | `08_INTEGRATION_SPEC.md` | 1 | Üçüncü parti entegrasyonlar |
| 09 | `09_CODING_GUIDELINES.md` | 1 | Kod standartları, git/CI kuralları, yasaklar |
| 10 | `10_MVP_SCOPE.md` | 1 | Ne var, ne yok, sınırlar |
| 11 | `11_IMPLEMENTATION_PLAN.md` | 1 | Sıralı task listesi, bağımlılıklar, kabul kriterleri |
| 12 | `12_VALIDATION_PROTOCOL.md` | 1 | Doğrulama seviyeleri, kanıt standardı, severity |

**Süreç dosyaları** (dönem 2–4 boyunca yaşar):

| Dosya | Rolü |
|---|---|
| `PRODUCT_DISCOVERY_STATUS.md` | Dönem 1'in karar kaydı — her karar anında yazılır |
| `IMPLEMENTATION_STATUS.md` | **Task durumu için tek doğru kaynak** |
| `STATUS_CHANGELOG.md` | Status dosyasının tarihsel arşivi (§G.2) |
| `DEFERRED_BACKLOG.md` | Ertelenen her işin tek izlenebilir listesi (§H) |
| `DEBT_CLOSURE_PLAN.md` | MVP borç kapatma iş paketleri (§I) |
| `BYPASS_LOG.md` | Her disiplin bypass'ının otomatik kaydı (§F.2) |
| `CI_CD_SETUP.md`, `DEPLOY_RUNBOOK.md`, `INTEGRATION_RUNBOOKS/` | Runbook katmanı (§M) |
| `TASK_REPORTS/`, `CHECKPOINT_REPORTS/`, `AUDIT_REPORTS/`, `CROSS_REVIEW_REPORTS/` | Kanıt arşivi |

---

## C. Dönem 1 — Doküman üretimi

### C.1 Aşama tablosu

Her aşama bir öncekinin çıktısına dayanır. **Sırayı atlamak tutarsızlık yaratır.**

| Aşama | Rol | Girdi | Çıktı | Traceability zorunlu mu |
|---|---|---|---|---|
| 1 — Product Discovery | Product Manager / Discovery Facilitator | — | 01, 02, 10 | Hayır (kök) |
| 2 — Kullanıcı akışları | Product Owner / Business Analyst | 01, 02 | 03 | Hayır (türetim) |
| 3 — UI/UX tasarım | Senior Product Designer / UX Architect | 02, 03, 10 | 04 | **Evet** (02+03 → ekranlar) |
| 4 — Teknik mimari | Senior Software Architect | 01–04, 10 | 05 | Hayır |
| 5 — Veri modeli | Data / Domain Modeler | 02, 03, 05, 10 | 06 | **Evet** (02+03 → entity/field) |
| 6 — API tasarımı | API Designer | 02–06, 10 | 07 | **Evet** (04+03 → endpoint) |
| 7 — Entegrasyon spec | Integration Engineer | 02, 03, 05, 06, 07 | 08 | Hayır |
| 8 — Kodlama kılavuzu | Tech Lead | 02, 04, 05, 06, 07, 08, 10 | 09 | Hayır |
| 9 — Implementation planı | Delivery Lead | 02–10 | 11 | **Evet** (tüm çıktılar → task) |
| 10 — Doğrulama protokolü | QA / Verification Lead | 02–11 | 12 | Hayır |

### C.2 Workshop formatı (tüm aşamalarda)

- **Soru bazlı ilerleme:** Konular sırayla, tek tek. Her konu netleşmeden bir sonrakine geçilmez. Tüm konuları aynı anda açmak hem tutarlılığı bozar hem proje sahibini bunaltır.
- **Kararları zincirleme bağlama:** Her karar bir sonrakini şekillendirir. Bağımlılık zincirini takip et.
- **Edge case'i anında ele al:** Her karardan sonra *"burada ne ters gidebilir?"* sorulur. Bu soruyu proje sahibinden **önce** ajan sorar.
- **İnkremental kayıt:** Karar alındığı anda `PRODUCT_DISCOVERY_STATUS.md`'ye yazılır. Hiçbir karar kaybolmaz.
- **Seçenek + öneri + onay:** Ajan seçenekleri artı-eksileriyle sunar, **kendi önerisini belirtir**, ama "ben bunu uyguluyorum" demez — onay ister.
- **Cevapsız soru bırakma:** Proje sahibine sorulan soru cevaplanmadan başka konuya geçilmez.

### C.3 Traceability Matrix kuralı

Bir aşamanın çıktısı önceki aşamaların girdilerinden türetiliyorsa, **çıktıyı üretmeden önce** iki yönlü izlenebilirlik matrisi kurulur:

- **İleri izlenebilirlik:** Her kaynak madde (gereksinim, akış adımı) → hangi çıktıya eşlendi? Eşlenmeyen madde = **boşluk (GAP)**.
- **Geri izlenebilirlik:** Her çıktı → hangi kaynaktan besleniyor? Kaynağı olmayan çıktı = **gerekçesiz ekleme**.
- Tespit edilen GAP'ler proje sahibine sunulur, karar alınır, **ardından** çıktı üretilir.

> **Vaka:** Bir projede ekran envanteri yazılmadan önce kurulan matris 7 boşluk yakaladı — hiçbiri o ana kadar hiçbir dokümanda adreslenmemiş ama arayüzde cevap gerektiren sorulardı (sözleşme kabul adımı, davet linki karşılama sayfası, bildirim kanal tercihleri). Aynı yöntem veri modelinde 7, API tasarımında 8 boşluk daha yakaladı. Matris **çıktıdan sonra** kurulsaydı bunlar geriye dönük düzeltme olarak çıkacaktı.

### C.4 Doküman Tamamlama Protokolü

Bir doküman `✓ Tamamlandı` işareti almadan önce üç kontrol uygulanır. **Tutarlılığın birincil savunma hattı checkpoint değil, doküman yazım anıdır.**

1. **Çapraz referans doğrulaması:** Başka dokümandan alınan her bilgi (enum değeri, sayısal parametre, iş kuralı, terim) kaynakla **birebir** eşleşmeli. Mümkünse kaynak referansı yazılmalı (ör. "02 §4.7 ile tutarlı").
2. **İç tutarlılık kontrolü:** Dokümanın bir bölümü diğer bölümüyle çelişmemeli — özellikle **özet tabloları ile detay bölümleri** arasında.
3. **Bağımlılık dokümanları taraması:** Header'daki her bağımlılık dokümanı hedefli taranır: orada tanımlı olup burada farklı ifade edilen veya eksik kalan kural var mı?

> **Vaka:** Bir projede enum isimleri kaynak dokümandan farklı yazıldı. Kural ("kaynakla birebir") doğruydu ama **yazım anında uygulanmadı**; hatayı sonraki audit yakaladı. Protokol bunu yazım anına çeker.

### C.5 Kalite döngüsü

Her doküman için sırayla:

```
yazım → /audit (envanter bazlı sistematik denetim)
      → /deep-review (8 katmanlı kalite analizi)
      → /cross-review (bağımsız ikinci AI) → TEMİZ olana kadar tekrar
      → etki yansıtma (değişikliklerin diğer dokümanlara etkisi)
      → /checkpoint (aşama geçiş taraması)
      → ✓ Tamamlandı
```

- **Audit** ve **deep-review** aynı ajanın iç denetimidir; **cross-review** farklı bir modele okutmaktır. Üçü birbirinin yerine geçmez.
- Cross-review'da **rubber stamp yasaktır**: %100 kabul şüphelidir. Her bulgu için KABUL / RET / KISMİ kararı ve RET için somut gerekçe zorunludur.
- **Etki yansıtma atlanamaz:** İkinci AI her dokümanı izole okur, cross-document uyumsuzlukları yakalamaz. TEMİZ sonrası downstream/upstream tarama yapılır.

> **Vaka:** Bir kodlama kılavuzu 7 tur cross-review sonunda 21 düzeltme aldı (3 kritik). Aynı süreçte eklenen 3 yeni alan, veri modeli dokümanında tanımlı değildi — bunu ne audit ne cross-review, **checkpoint** yakaladı. Kalite döngüsünün tamamı gerekli.

### C.6 Agent'a katmanlı doküman verme

Ajan gereksiz bilgiyle boğulmaz; görevine göre yalnız ihtiyacı olanı alır:

| Katman | Dokümanlar | Ne zaman |
|---|---|---|
| Ürün bağlamı | 01, 02, 10 | "Ne yapıyoruz" gerektiğinde |
| Akış ve tasarım | 03, 04 | Kullanıcı deneyimi görevlerinde |
| Teknik | 05, 06, 07, 08 | Kodlama görevlerinde |
| Standartlar | 09 | **Her** kodlama görevinde |
| Görev planı | 11 | Task atama ve takipte |
| Doğrulama | 12 | Cross-check görevlerinde |

**Kaynak-güdümlü ek:** Bir kontrol maddesinin kaynak sütununda referans verilen doküman katman setinde yoksa **yine de eklenir** — ajanın kaynağı okumadan değerlendirmesi engellenir.

### C.7 Aşama işletim checklist'i

Doküman üretim aşamalarının adım adım işletimi: [`.claude/checklists/document-stage.md`](../.claude/checklists/document-stage.md).

> Bu bilinçli olarak bir skill değil, checklist'tir. Doküman üretimi projeden projeye en çok değişen dönemdir; skill'e dönüştürme, checklist bir kez gerçek bir projede işletildikten **sonra** yapılır.

---

## D. Dönem 2 — Implementation çalışma modeli

### D.1 Rol

Implementation döneminde ajan **senior software engineer** gibi davranır: kod yazar, test yazar, dokümanlarla tutarlılığı korur. Dokümanlar (02–10) source of truth'tur; **kod dokümanla çelişmez**. Çelişki fark edilirse sessizce kod yazılmaz, önce bildirilir.

### D.2 Task-per-chat

- Her task **ayrı bir chat**'te yapılır (`/task TXX`).
- Her task tamamlandığında **ayrı bir doğrulama chat'i** açılır (`/validate TXX`) — yapan ≠ denetleyen.
- **Doğrulama asla yapım chat'inde yapılmaz.** Aynı chat'te doğrulama anchoring yaratır, bağımsızlığı bozar.
- Task sırası plana sadık kalır. Atlama yapılmaz.

Her task başlamadan önce üç şey net olmalı:
1. Etkilenen modüller / dosyalar
2. Beklenen çıktı / artifact
3. "Bu task bittiğinde sistemde tam olarak ne değişmiş olacak?"

### D.3 Task durumları

| Durum | Açıklama |
|---|---|
| `⬚ Bekliyor` | Henüz başlanmadı |
| `⏳ Devam ediyor` | Yapım chat'inde aktif |
| `✓ Tamamlandı` | Doğrulama PASS, ana dala merge edildi |
| `✗ FAIL` | Doğrulama başarısız, düzeltme bekleniyor |
| `⛔ BLOCKED` | İlerleyemiyor — alt tür belirtilir (§D.8) |

`✓ Tamamlandı` işaretini **yalnızca doğrulama chat'i** koyabilir. Yapım chat'i en fazla `⏳` yapar.

### D.4 Branching ve PR disiplini

| Kural | Değer |
|---|---|
| Feature branch | `task/TXX-kisa-aciklama` |
| Merge | **Squash**, ana dala |
| Squash commit mesajı | `TXX: Task adı (#PR-no)` |
| Faz tag'i | `phase/FX-pass` |
| Ana dal | Her zaman çalışır durumda |
| Direct push | **Yasak** (mekanik olarak bloklanır — §F.2) |
| Merge ön koşulu | CI yeşil **ve** validator PASS |
| Merge'ü kim yapar | **Validator chat**, PASS verdikten sonra (§D.9) |

**Commit tipleri:** `feat` · `fix` · `refactor` · `test` · `docs` · `chore` · `migration`.
Task commit'leri `TXX: ...` ile başlar; task'a ait olmayan işler `chore:`/`docs:`/`infra:` ile başlar ve **ayrı PR** ister.

**PR kuralları:** PR tek bir amaca hizmet eder. Karışık değişiklik yasak. **Bundled-PR yasağı:** başka bir task'ın PR'ına "tek commit daha ne olur" diyerek gömmek yasaktır — küçük görünen düzeltmeler bile ayrı PR ister. Tek istisna: aynı TXX'in düzeltmeleri aynı dala.

> **Vaka:** İki task kendi PR'ını hiç açmadı; kodları bir faz gate check PR'ına gömüldü ve 3 PR boyunca fark edilmedi. Üç task daha bir sonraki task'ın dalına gömülüp tek PR olarak geldi. Sonuç: beş task hiç bağımsız doğrulanmadı. Bunun mekanik karşılığı §F.2'dedir.

### D.5 Task döngüsü

```
/task TXX  (yapım chat'i)
  Adım -1  Working tree hygiene check ......... HARD STOP
  Adım  0  Ana dal CI startup check ........... HARD STOP
  Adım  1  Task tanımını oku (11'den)
  Adım  2  Bağımlılık kontrolü (status'ten)
  Adım  3  Doküman referanslarını oku (yalnız ilgili bölümler)
  Adım  4  Dış varsayım ön-uçuş kontrolü ...... kanıt zorunlu
  Adım  5  Scope netleştirme → proje sahibine sun
  Adım  6  Ana dalı güncelle
  Adım  7  Branch aç
  Adım 8-12 Kod + test + build + mini güvenlik + self-check
  Adım 13-16 Commit/push + rapor taslağı + status ⏳ + push
  Bitiş Kapısı (8 madde) .................... hepsi ✓ olmadan bitmez
        │
        ▼
/validate TXX  (AYRI chat)
  Adım -1  Working tree hygiene check ......... HARD STOP
  Adım  0  Ana dal CI startup check ........... HARD STOP
  Adım  0b Repo memory drift check ............ HARD STOP
  Adım 1-10 Bağımsız doğrulama (rapor GÖRÜLMEDEN)
  Adım 11-12 Verdict + bulgu sınıflaması
  Adım 13-14 Yapım raporuyla karşılaştır + finalize
  Adım 15-16 Status ✓ + commit/push (merge ÖNCESİ)
  Adım 17  Squash merge
  Adım 18  Post-merge CI watch (tüm workflow'lar) . ZORUNLU
```

### D.6 Üç katmanlı kalite kapısı

| Katman | Ne zaman | İçerik |
|---|---|---|
| **1 — Task doğrulama** | Her task sonrası | Kabul kriterleri (kanıtlı) · doküman uyumu · unit/integration test · build + lint + type check · **mini güvenlik kontrolü** |
| **2 — PR / CI gate** | Her merge öncesi | Otomatik pipeline: lint → build → unit → integration → contract → migration dry-run → paketleme. CI PASS **ve** validator PASS olmadan merge yok. |
| **3 — Faz sonu gate check** | Her faz sonrası | Ayrı chat. Regresyon + önceki faz testleri + temiz ortam ayağa kaldırma + traceability boşluk taraması + migration rehearsal + güvenlik özeti + öğrenim terfisi + backlog forward (§E) |

**Mini güvenlik kontrolü (4 madde, her task ve her PR'da):**
- Secret sızıntısı var mı? (API key, private key, connection string kodda mı?)
- Auth/authorization etkisi var mı? (Yeni yüzey korumasız mı?)
- Input validation etkisi var mı? (Yeni kullanıcı girdisi sanitize ediliyor mu?)
- Yeni dış bağımlılık eklendi mi? (Güvenilirliği değerlendirildi mi?)

### D.7 Kabul kriteri doğrulama durumları

| Sonuç | Anlamı |
|---|---|
| `✓ Karşılandı` | Kanıtla doğrulandı |
| `✗ Karşılanmadı` | Eksik veya hatalı |
| `~ Kısmi` | Kısmen karşılandı, detay açıklanır |
| `? Doğrulanamadı` | Kanıt yetersiz — **bu FAIL değildir.** Kalite zayıflığı değil, kanıt/doğrulama yöntemi eksikliğidir. Raporda FAIL'den ayrı tutulur; PASS için çözülmesi gerekir. |

**Kanıt zorunluluğu:** Her kriter için çalıştırılan komut, çıktı ve hangi commit üzerinde bakıldığı yazılır. **Sadece ✓ işareti yetmez.** Kanıt doğrulama anında üretilmiş olmalıdır; eski veya başka ortamdan alınan kanıt kabul edilmez.

### D.8 BLOCKED akışı

| Alt tür | Ne zaman |
|---|---|
| `SPEC_GAP` | Doküman yetersiz veya belirsiz |
| `DEPENDENCY_MISMATCH` | Önceki task'ın çıktısı yetersiz/uyumsuz |
| `PLAN_CORRECTION_REQUIRED` | Task sırası veya tanımı yanlış |
| `EXTERNAL_BLOCKER` | Dış bağımlılık (erişim, hesap, kota) |

Akış: **kayıt** (BLOCKED raporu) → **etki analizi** (hangi doküman/task etkileniyor) → **proje sahibine sunum** (çözüm önerileriyle) → **karar** (doküman düzeltmesi / plan güncellemesi / task yeniden tanımı / erteleme) → **güncelleme** → **devam**.

**Kritik kural:** Model BLOCKED durumu sessizce geçemez. Doğaçlama yapılmaz, varsayımla ilerlenmez.

### D.9 Raporlama ve merge sırası

1. Her task için `TASK_REPORTS/TXX_REPORT.md` (şablon: [`_TEMPLATE.md`](TASK_REPORTS/_TEMPLATE.md)).
2. **Güncelleme sırası kuralı:** Rapor finalize edilmeden status güncellenmiş sayılmaz. **Önce rapor, sonra status.** Bu kural drift'i önler.
3. Rapor + status **merge'den önce** commit+push edilir — aksi halde squash merge bu değişiklikleri içermez.
4. Merge'ü validator yapar (yalnız PASS durumunda).
5. **Merge teyidi doğrudan ana dala push edilemez.** Post-merge run ID'leri gibi kozmetik ekler ya sonraki task dalında ya ayrı bir `chore:` PR'ında gider.

> **Vaka:** Bir task chat'i CI yeşilini görünce kendi PR'ını merge etti; validator yalnız post-hoc finalize yapabildi. Bu akış resmîleştirilseydi validator'ın **merge-öncesi veto yetkisi** tamamen kalkacaktı — nadiren kullanılan ama "sapma avcısı" rolünün gerçek kapısı olan yetki. Karar: tempo kaybına rağmen veto yetkisi korunur.

> **Vaka:** Post-merge 1 satırlık status düzeltmesi "PR overkill" gerekçesiyle direct-push ile yapıldı. Sonuç: CI guard job'ı kırmızı yandı, hook otomatik bypass kaydı yazdı, o kayıt working tree'yi kirletti, onu commit'lemek için yeni bypass gerekti — **döngü**. Post-merge drift yalnızca `chore:` PR ile kapatılır.

### D.10 Hata sınıflandırması

| Seviye | Tanım | Tespit |
|---|---|---|
| **S1 — Sapma** | Task tamamlandı ama dokümanla uyumsuz | Task doğrulama |
| **S2 — Kırılma** | Bir task başka bir task'ın çıktısını bozuyor | Regresyon testi (gate check) |
| **S3 — Eksik** | Traceability'de eşlenmiş öğe implement edilmemiş | Boşluk taraması (gate check) |

**Kritik kural:** Düzeltme task'ı bir sonraki faza ertelenmez — hatanın oluştuğu fazda çözülür.

---

## E. Faz yapısı ve gate check

### E.1 Fazlar

Task'lar fazlara ayrılır; her faz bir öncekinin üstüne oturur. Faz sayısı ve isimleri projeye göre değişir, ama **şekil sabittir**: iskelet → veri → çekirdek servisler → iş mantığı → entegrasyonlar → arayüz → uçtan uca doğrulama.

Faz tanımları `11_IMPLEMENTATION_PLAN.md` §3'te yapılır. Gate check PASS vermeden sonraki faza geçilmez.

### E.2 Gate check kapsamı

1. **Ön kontrol** — fazın tüm task'ları `✓`, raporlar mevcut ve finalize, status ile tutarlı
2. **Regresyon** — mevcut faz + **önceki tüm fazların** testleri
3. **Build ve ortam** — temiz build + tüm servisleri sıfırdan ayağa kaldırma (fresh environment) + health check
4. **Migration rehearsal** — temiz veritabanında migration + idempotency (ikinci kez çalıştır) + seed doğrulama
5. **Traceability ve boşluk taraması** — matrisin "implemented" kolonu; eşlenip implement edilmeyen öğe = S3
6. **Doküman uyumu** — enum/sözleşme/alan adları
7. **Güvenlik özeti** — açık bulgu, yeni dış bağımlılıklar, auth değişiklikleri
8. **Öğrenim terfisi** (§K) ve **backlog forward** (§H)

### E.3 Verdict

- **PASS:** Testler geçiyor, build temiz, ortam ayağa kalkıyor, traceability boşluğu yok, S2 kırılma yok.
- **FAIL:** En az bir kritik bulgu. Bulgular S1/S2/S3 sınıflaması + düzeltme planıyla sunulur.

**Bloke-etmeyen bulgular** ayrı bir tabloda listelenir ve ya gate'in chore PR'ında kapatılır ya `DEFERRED_BACKLOG.md`'ye forward edilir. **Kaydı olmayan bulgu yoktur.**

### E.4 PASS ritüeli

Tek bir `chore:` PR'ında:
1. Gate raporu → `CHECKPOINT_REPORTS/GATE_CHECK_FX.md`
2. `IMPLEMENTATION_STATUS.md` faz satırı `✓ PASS`
3. Repo memory faz özeti + "sıradaki" satırı
4. Backlog forward'ları
5. Öğrenim terfileri (§K)

Merge + CI yeşil + tüm workflow'lar teyit edildikten **sonra** ana dal üzerinde tag: `git tag phase/FX-pass && git push origin phase/FX-pass`.

---

## F. Savunma katmanları

> **Tek bir katman yeterli değildir.** Katmanlar birbirini tamamlar: startup check chat başlamadan önce yakalar, hook push öncesinde yakalar, validator task sonunda yakalar, bitiş kapısı bundled PR'ı yakalar.

### F.1 Startup kapıları (HARD STOP)

Her yapım ve doğrulama chat'i üç kapıdan geçerek başlar:

| # | Kapı | Kontrol | Başarısızlıkta |
|---|---|---|---|
| -1 | **Working tree hygiene** | `git status --short` boş mu | Değişiklikleri listele → **commit / stash / discard** kararını proje sahibinden al. Karar olmadan başlama. |
| 0 | **Ana dal CI** | Son 3 tamamlanmış run `success` mi | HARD STOP. Root cause çözülmeden başlama. |
| 0b | **Repo memory drift** *(yalnız validator)* | Memory'de TXX satırı var mı | BLOCKED — yapım chat'i bitiş kapısını ihlal etmiş. |

**Yasak rasyonelizasyonlar** (üçü için de): *"lokal temiz"* · *"benim işimle ilgisiz kırılma"* · *"önceki task'ın borcu"* · *"sadece şu workflow kırıldı"* · *"küçük değişiklik, bekleyemez"* · *"sonra hallederiz"* · *"önemsiz"* · *"benim değil"*.

> **Vaka:** Ana dalın CI'ı bir dönem 5 task üst üste kırık kaldı. 7'den fazla merge kırmızı CI üzerine indi ve "merge için CI PASS zorunlu" disiplini fiilen düştü — çünkü CI zaten kırıktı, kimse yeşil beklemiyordu. O dönemin faz gate'i bile "lokal temiz" gerekçesiyle PASS aldı; borç ancak bir sonraki fazda, başka bir validator tarafından ortaya çıkarıldı. **Validator'ın kanıt standardı lokal temizlikten yüksektir.**

> **Vaka:** Bir oturumun başında working tree'de üç dosyalık uncommitted değişiklik vardı. Kimse fark etmeden 3 PR boyunca yaşadılar. Yeni bir task dalı açılsaydı `git add` sırasında task PR'ına bundle olabilirlerdi — ve commit-msg hook bunu **yakalayamazdı**, çünkü dosya değişiklikleri commit subject'inde task referansı oluşturmaz.

### F.2 Mekanik zorlama (hook + CI)

| Katman | Mekanizma | Bloklar |
|---|---|---|
| **pre-commit hook** | Staged içerikte sır kalıbı (sır dosyası yolu, PEM bloğu, bilinen anahtar adına gerçek değer ataması) | Sır sızıntısı — **commit anında** |
| **commit-msg hook** | `task/TXX-*` dalındaki commit subject'i farklı bir TXX referansı içeriyorsa | Bundled commit — **commit anında** |
| **pre-push Layer 1** | Korunan dallara (`main`, varsa `develop`) direct push | Direct push |
| **pre-push Layer 2** | Push edilen dalın son CI run'ı `failure`/`cancelled`/`timed_out` | Kırık CI üstüne push |
| **pre-push Layer 3** | Dalda yabancı TXX commit'i (`git log main..HEAD` taraması) | Bundled PR — **push anında** |
| **CI job 0 — guard** | Ana dala gelen commit mesajında PR referansı `(#NN)` yok | Server-side görünür uyarı (push'u engelleyemez ama iz bırakır) |
| **Oturum kapısı — hook kurulumu** | `git config core.hooksPath` boş | Yukarıdaki **tüm** hook katmanlarının sessizce yok olması |
| **Bitiş kapısı manuel check** | Aynı `git log` taraması, hook kurulu değilse son savunma | Bundled PR — **PR öncesi** |

> **Hook'lar kurulu değilse bu tablonun ilk beş satırı yoktur.** `install.sh` çalıştırılmamış her klonda ve her efemeral ortamda (bulut oturumu, geçici konteyner, CI çalışma alanı) koruma **sıfırdır** ve bu **gözlemlenebilir değildir** — tıpkı kırık CI'nin sessizce sürmesi gibi. Bu yüzden `core.hooksPath` kontrolü her yapım ve doğrulama oturumunun **Adım -1**'ine gömülüdür, tek seferlik bir kurulum adımı değildir.

**Opsiyonel oturum hook'ları:** Ajan harness'ı destekliyorsa, kuralları *hatırlatan* (bloklamayan) oturum hook'ları eklenebilir — örneğin merge algılandığında çalışan workflow ID'lerini bağlama enjekte eden bir hatırlatıcı. Bunlar L5 sayılmaz (`exit 1` vermezler), platforma bağlıdırlar ve **kuralın yerine geçmezler**; kayıt yerleri `.claude/settings.json` + `.claude/hooks/`'tur.

**Bypass:** `PB_ALLOW_DIRECT_PUSH=1` (Layer 1+2) ve `PB_ALLOW_BUNDLED=1` (Layer 3 + commit-msg). Her ikisi de `PB_BYPASS_REASON="..."` ile kullanılır ve hook `BYPASS_LOG.md`'ye otomatik satır düşer (`[direct-push]` / `[ci-failure]` / `[bundled-pr]`). CI guard bypass'ı: commit mesajında `[skip-guard]`.

**Kural:** Bypass kullanıldıktan sonraki **ilk normal commit'te** log dosyasındaki değişiklik commit'lenir. Bypass'a başvurmadan iki kez düşün — bypass reflexine direnç, disipline en ucuz yatırımdır.

### F.3 Dış varsayım ön-uçuş kontrolü

Kod yazmadan ve scope'u kesinleştirmeden önce, task'ın dayandığı dış varsayımlar **listelenir ve her biri somut kanıtla doğrulanır**.

**Adım 1 — Varsayımı fark et (tetikleyici tarama).** Kabul kriterlerini ve plan metnini şu terimler için tara; biri geçiyorsa orada bir dış varsayım vardır:

> plan tier · ücretsiz / paid / trial / subscription / Pro / Team / Enterprise · kota · rate limit · API key · onay süreci · dal koruması · zorunlu review · runner / dakika / depolama limiti · paket sürümü · registry · sandbox / test ortamı · bölge kısıtı · SLA

Kaynak kuralın doğduğu vakada sorun kanıt üretememek değil, **varsayımın var olduğunun fark edilmemesiydi**. Kategori listesi bir sınıflandırmadır; tarama listesi tespit aracıdır — ikisi aynı şey değildir.

**Adım 2 — Doğrula.** Tipik kategoriler:
- **Plan tier / feature availability** — ücretli mi ücretsiz mi, hangi plan gerekli
- **Paket sürüm uyumu** — planda geçen sürüm gerçekten var mı (registry sorgusu)
- **Platform/OS farkı** — path, tip, base image, geliştirme ≠ CI ortamı
- **API/sözleşme varsayımı** — dış servis belgelendiği gibi cevap veriyor mu
- **Repo/ortam state'i** — runner kapasitesi, secret'lar, ortam değişkenleri

**Her varsayım için bir satır kanıt zorunludur:** komut çıktısı, resmî doküman bağlantısı veya manuel test sonucu. *"Sanırım"*, *"muhtemelen"*, *"genelde böyle"* yetersizdir.

- Varsayım kırıksa **DURULUR** — bu scope'u etkileyen bir karardır. Seçenekler proje sahibine sunulur, gerekirse BLOCKED tetiklenir.
- *"Bu task'ta dış varsayım yok"* geçerli bir sonuçtur — ama rapora **açıkça yazılır**. Audit trail'in eksikliği ≠ varsayımın yokluğu.

**Zaman kutusu — kapının maliyeti sınırsız değildir.** Bir varsayımın doğrulanması **~5 dakikalık** bir iştir (komut çalıştır, dokümana bak, tek bir çağrı yap). Daha uzun sürecekse (derin doküman okuma, ücretli kayıt, onay bekleyen erişim) **durulur ve proje sahibine taşınır**: *"Bu varsayımı doğrulamam ~X dakika alacak, devam edeyim mi?"*

> Bu üçüncü yol olmadan kapının önünde yalnız iki seçenek kalır: tavşan deliğine düşmek veya rasyonelize edip atlamak. İkincisi tam olarak bu kapının önlemek için var olduğu davranıştır — kaçış yolu olmayan kapı atlanır.

> **Vaka:** Bir CI/CD task'ı, platformun dal koruma özelliğinin mevcut planda kullanılabilir olduğunu varsaydı. Implementasyonun ortasında API 403 "yükseltme gerekli" döndü. Bedel: 3 düzeltme commit'i, gerçekleşmemiş iddialar içeren bir kapanış commit'i, re-validation talebi, iki dokümanda rejim değişikliği. Plan aşamasında **5 dakikalık** bir kontrol bunu önlerdi.

> **Vaka:** Bir sidecar task'ı, dokümanda yazan paket sürümünün registry'de var olduğunu varsaydı; yoktu. Ön-uçuş kontrolü bunu kod yazılmadan yakalar.

**Reaktif katmanlar (startup check, bitiş kapısı) gerçekleşen hatayı yakalar; ön-uçuş kontrolü hatanın gerçekleşmesini engeller.**

### F.4 CI izleme sorumluluğu

- **Ajan tarafından açılan her PR'ın CI run'ı, `concluded + success` olana kadar ajan tarafından izlenir.** Task PR / chore PR / infra PR / docs PR / validator-fix PR ayrımı **yoktur**.
- Ajan kullanıcıya *"CI'yi sen mi izleyeceksin?"*, *"takip edeyim mi?"* türü soru **sormaz**. İzleme varsayılan davranıştır; onay yalnızca CI sonucuna göre alınacak **aksiyonlar** için istenir.
- **Workflow tipi ayrımı yoktur:** Aynı ref'e bağlı birden fazla workflow varsa **hepsinin** conclusion'ı beklenir. Biri success diğeri devam ederken "bitti" denmez.
- Sonuç ajan tarafından raporlanır: `success` → merge hazır; aksi → root cause + düzeltme önerisi.
- İzleme arka planda sürdürülebilir; kullanıcı başka konuya geçse bile sonuç rapor edilir.

> **Vaka:** Bir merge sonrası ana pipeline izlendi ama ikinci workflow (paket yayınlama) izlenmedi. Son on çalıştırma şans eseri yeşildi; disiplin olarak yanlıştı.

### F.5 Bitiş kapısı (yapım chat'i)

Sekiz maddenin **hepsi ✓** olmadan task "yapım bitti" sayılmaz ve doğrulama chat'ine geçilmez:

1. Branch push edildi
2. PR açıldı, numara geri geldi
3. PR numarası task raporuna yazıldı
4. Rapor + status commit'lendi ve push edildi
5. CI run **tamamlandı** (`in_progress`/`queued` beklenir — "başladı" yeterli değildir)
6. CI run sonucu **`success`**
7. Branch izolasyon check temiz (dalda yalnız kendi TXX'i)
8. Repo memory'de TXX satırı eklendi/güncellendi

**Otomatik BLOCKED trigger:** Raporda "PR: Henüz oluşturulmadı", "PR: TBD", "PR: —" veya boş PR alanı görülürse otomatik BLOCKED (`DEPENDENCY_MISMATCH`) — yapım chat'i açık kalır, sonraki task'a geçilmez.

**Concurrency notu:** Hızlı art arda push'ta yeni run öncekini iptal edebilir; bu `failure` sayılmaz — **son tamamlanmış** run'a bakılır.

---

## G. Durum takibi ve hafıza

### G.1 Tek doğru kaynak

Durum için **tek otoriter kaynak tracker'dır**. Hangi dosya olduğu **döneme bağlıdır**:

| Dönem | Tracker | Neyin durumunu tutar |
|---|---|---|
| Dönem 1 — Doküman üretimi | `PRODUCT_DISCOVERY_STATUS.md` | Blok ve konu durumu, karar kaydı, açık kararlar |
| Dönem 2 — Implementation | `IMPLEMENTATION_STATUS.md` | Task ve faz durumu |

Her ikisi de Git'te tutulur; her commit'te güncellenir.

> **Vaka:** Kural uzun süre yalnız `IMPLEMENTATION_STATUS.md`'yi işaret etti. Doküman döneminde *"nerede kaldık"* sorulduğunda ajan boş şablona yönlendirildi; tracker'ın konu hakkında görüşü olmadığı için §G.3'ün *"çelişirse tracker kazanır"* kuralı işlemedi ve doğru cevap Git diff'inden çıkarılmak zorunda kaldı. **Ders:** Otoriter kaynağı adlandıran her kural dönemi de söyler.

### G.2 Snapshot ≠ arşiv

**Kural:** Güncel snapshot kısa kalır; tarihsel detay arşiv dosyasına taşınır.

- `IMPLEMENTATION_STATUS.md` → yalnız **en son** güncelleme özeti + task tabloları
- `STATUS_CHANGELOG.md` → tarihsel güncelleme geçmişi
- Repo memory → güncel snapshot; ayrıntılı task changelog'u ayrı arşiv dosyasında

> **Vaka:** Status dosyasının "Son güncelleme" başlığı prepend-only changelog'a dönüştü ve **63 KB'lık tek satıra** büyüdü; okuma aracı satırı truncate etti, grep "uzun satır atlandı" dedi. Aynı anda repo memory 326 KB'a çıkıp okuma sınırını tümden aştı. Sonuç: *"başka bir oturumda nerede kaldığımızı bulamıyorum"*. **Ders:** Kaynak dosyaları prepend-only changelog'a çevirme.

### G.3 Hafıza katmanları

| Katman | Nerede | Ne barındırır |
|---|---|---|
| **Süreç kuralları** | L1–L5 (§A.4) | Evrensel kurallar. Hafızada **yaşamaz**. |
| **Proje hafızası** | `.claude/memory/MEMORY.md` + `memory/*.md` | Güncel durum snapshot'ı, proje kararları, dış referanslar |
| **Kişisel tercihler** | `.claude/memory/*.md` (feedback tipi) | Yanıt dili, öneri savunma tarzı, onay isteme sınırları — **kişiye bağlı, projeye değil** |

**Terfi kuralı:** Bir hafıza notu ikinci kez bir süreç ihlalini önlemek için kullanılıyorsa, o not artık bir **kuraldır** — L1–L5'e terfi eder, hafızada yalnız işaretçisi kalır.

**Durum sorusu kuralı:** *"Sırada ne var?"*, *"nerede kaldık?"* sorularında hafıza snapshot'ına güvenilmez. Önce tracker okunur (ilgili satır `grep`'lenir, tüm dosya açılmaz). İkisi çelişirse **tracker kazanır** ve hafıza düzeltilir.

> **Vaka:** Hafızadan konuşularak yanlış "sıradaki task" söylendi; gerçekte o task tamamlanmış, bir sonrakine geçilmişti. Hafıza snapshot'ı her zaman bir adım geride olabilir.

### G.4 Memory drift kapısı

Repo memory'nin güncelliği **gözlemlenebilir değildir** — hiçbir kapı kontrol etmezse sessizce geride kalır. Bu yüzden üç yerde kontrol edilir: yapım bitiş kapısı (madde 8), validator Adım 0b, gate check PASS ritüeli.

---

## H. Ertelenmiş işler backlog'u

`DEFERRED_BACKLOG.md`, bilinçli olarak ertelenen **her** somut işin tek izlenebilir listesidir.

**Neden gerekli:** Validator ve gate bulgularının bir kısmı "gerçek ama şimdi değil" kararıyla kapanır. Bu karar yalnızca task raporunun içinde kalırsa **buharlaşır** — 100 rapor sonra kimse o satırı bulamaz.

**Kayıt kuralları:**
- Her kalemin **ID**'si, **kanıtı** (kod referansı veya rapor bağlantısı), **tipi**, **önceliği** ve **"neyi bloklar"** alanı vardır.
- Tip: `task` · `backend-gap` · `k-note` (validator notu) · `code-todo` · `doc-drift` · `test-gap` · `bypass`
- Bir kalem çözüldüğünde satır **silinmez**, `✅ Çözüldü → <nerede>` olarak işaretlenir. İzlenebilirlik korunur.
- **Backlog forward:** Gate check'te ortaya çıkan bloke-etmeyen her bulgu ya o gate'in chore PR'ında kapatılır ya backlog'a forward edilir. Üçüncü seçenek yoktur.

**Hijyen taraması:** Her faz gate'inde açık kalemler kod düzeyinde yeniden doğrulanır — bir kısmı araya giren işlerle çözülmüş, bir kısmı stale olmuş olur.

---

## I. MVP borç kapatma aşaması (WP)

> **"Plandaki task'lar bitti" ≠ "MVP çalışıyor".**

### I.1 Neden var

Task-bazlı doğrulama **modülü** doğrular, uçtan uca akışı doğrulamaz. Her task kendi kabul kriterlerini karşılar, validator'ı PASS verir, gate'i geçer — ve buna rağmen ürünün uçtan uca akışı tamamlanmamış olabilir: bir tarafta üretilen olay hiçbir yerde tüketilmiyordur, bir servis yazılmıştır ama hiçbir yerden çağrılmıyordur, bir durum geçişi tanımlıdır ama tetikleyicisi yoktur.

Bu **yapısal** bir boşluktur, dikkatsizlik değil: kabul kriterleri "X servisi yazıldı ve test edildi" der, "X servisi üretimde çağrılıyor" demez.

> **Vaka:** Tüm uygulama fazları ✓ PASS almış bir projede, uçtan uca test fazına başlamadan önce yapılan tarama, ana akışın kendisinin tamamlanamadığını gösterdi: kritik bir durum geçişi hiçbir üretim kodundan tetiklenmiyordu (`.Fire(Complete)` araması = 0 sonuç). Ertelenen işlerin çoğu "isteğe bağlı temizlik" değil, **ürünün gerçekte çalışması için eksik kalan motor bağlantıları**ydı. Bunlar kapanmadan E2E testleri zaten geçemezdi.

### I.2 Ne zaman ve nasıl

**Ne zaman:** Uygulama fazları bittikten sonra, uçtan uca doğrulama fazından **önce**.

**Nasıl:**

1. **Tarama** — üç kaynak birleştirilir: (a) `DEFERRED_BACKLOG.md`, (b) kod düzeyinde "üretilen ama tüketilmeyen / yazılan ama çağrılmayan" taraması, (c) task raporlarının "known limitations" bölümleri.
2. **Sınıflandırma** — her kalem üç kovadan birine: **MVP-içi borç** (kapatılacak) · **MVP-dışı** (kapsam gereği hariç, erteleme değil) · **by-design / imkânsız**.
3. **Paketleme** — MVP-içi borç `WP1…WPn` iş paketlerine bağlanır. Her paket: hangi yeteneği tamamladığı, hangi backlog ID'lerini kapattığı, **kod kanıtı**, hangi doğrulama senaryosunu açtığı, migration taşıyıp taşımadığı, efor.
4. **Sıralama** — bağımlılık zinciri kurulur; para/veri akışının temeli olan paket ilk sırada.
5. **İşletim** — her WP **normal bir task gibi** işlenir: plan → uygula → ayrı chat'te validate → ayrı PR → CI yeşil.

**Çıktı:** `DEBT_CLOSURE_PLAN.md`.

### I.3 Kapsam çiti

Her WP'ye **scope-fence** yazılır: MVP kapsamının neresine kadar. Borç kapatma aşaması gold-plating'e en açık dönemdir — "madem dokunuyoruz" refleksi burada frenlenir.

---

## J. Numaralandırma konvansiyonları

| Biçim | Anlamı | Ne zaman |
|---|---|---|
| `TXXa`, `TXXb` | Planı bozmadan araya task sokma | Cross-review veya gate yeni bir task gerektirdiğinde, faz aralığını ve bağımlılık zincirini bozmamak için |
| `TXX.1`, `TXX.2` | Aynı task'ın devam turu | Close-out işleri, sonradan eklenen savunma katmanları |
| `TXXb` (bölme) | Bir task'ın yarısının ayrı task'a ayrılması | "Salt frontend" sanılan bir task'ın backend eksiği çıktığında |
| `WPna`, `WPnb` | Borç paketinin ikiye bölünmesi | Paket tek PR'a sığmadığında |
| `PR-1/3`, `PR-2/3` | Büyük task'ın birden fazla PR'a bölünmesi | Aynı TXX, aynı dal, sıralı PR'lar |
| `chore:`, `docs:`, `infra:`, `hooks:` | TXX iddiasında bulunmayan işler | Task PR'ına giremeyecek meta değişiklikler — **ayrı PR** |

Bu biçimler hook'ların TXX çıkarma deseniyle uyumludur (`^T[0-9]+(\.[0-9]+)?[a-z]?`).

---

## K. Öğrenim → kural terfisi

**Bir aşama, öğrenimi yazılmadan kapanmaz.** Bu bir gate maddesidir, iyi niyet değil.

Her faz gate check'inde ve her doküman aşaması kapanışında:

1. **Öğrenim çıkar** — bu aşamada ne beklenmedik şekilde çalıştı, ne çalışmadı?
2. **Sınıfla:**
   - **Tek seferlik gözlem** → yalnız aşama raporunda kalır
   - **Tekrarlanacak desen** → §N'e (bu dokümanın öğrenim bölümü) yazılır
   - **İhlali önleyen kural** → **terfi eder**: hangi katmana (L1–L5) ve hangi dosyaya gideceği belirtilir, aynı PR'da uygulanır
3. **Terfi kaydı** — gate raporunda "Öğrenim terfileri" tablosu: öğrenim → hedef katman → uygulanan değişiklik.

> **Vaka:** Bir metodoloji dokümanının "öğrenimler" bölümü dört ay boyunca *"aşama tamamlandığında doldurulacak"* satırıyla boş kaldı; o dönemde 114 task, 20 iş paketi, 7 faz gate'i ve 210 PR yaşandı ve **hiçbiri metodolojiye yansımadı**. Terfi elle ve tesadüfi işlerse çürür — bu yüzden gate maddesidir.

---

## L. Kapanış ritüeli

MVP'nin son fazı (uçtan uca doğrulama) PASS aldığında:

| # | Adım | Çıktı |
|---|---|---|
| 1 | Final gate check | `CHECKPOINT_REPORTS/GATE_CHECK_FX.md` — "bu MVP'nin son uygulama fazıdır" notu ile |
| 2 | Backlog forward | Kalan tüm açık kalemler `DEFERRED_BACKLOG.md`'de post-MVP olarak sınıflanır; hiçbir bulgu raporda esir kalmaz |
| 3 | Status finalize | `IMPLEMENTATION_STATUS.md` — tüm fazlar ✓, "sırada ne var" alanı **açıkça** post-MVP backlog ile deploy hazırlığı arasındaki seçimi proje sahibine bırakır |
| 4 | Memory finalize | Repo memory + oturum hafızası güncel snapshot'a çekilir; arşiv ayrılır |
| 5 | Runbook config | `DEPLOY_RUNBOOK.md` zorunlu ayar/secret listesiyle tamamlanır — **prod'da hangi ayar set edilmezse sistem açılmaz** ve **hangisi set edilmezse bir kural sessizce devre dışı kalır** ayrımı yazılır |
| 6 | **Gerçek konfigürasyonla boot provası** | Runbook **yazılmakla kalmaz, bir kez çalıştırılır**: gerçek dağıtım konfigürasyonu (test/E2E konfigürasyonu **değil**) ile sistem sıfırdan ayağa kaldırılır, sağlık uçları doğrulanır, sonuç runbook'a yazılır. Detay: §L.1 |
| 7 | Öğrenim terfileri | §K |
| 8 | Tag | `phase/FX-pass` |

> **Vaka:** Bir projede bir güvenlik kuralı kod olarak tamamdı ama iki üretim ayarı varsayılan değerlerinde bırakılırsa **sessizce devre dışı** kalıyordu. Bu, kod borcu değil **deploy runbook** borcuydu; kapanış ritüelinin 5. adımı bunu yakalar.

### L.1 Boot provası — "hangi konfigürasyonla" sorusu

Faz gate'leri *"tüm servisleri sıfırdan ayağa kaldır"* der (§E.2 adım 3). Bu cümlenin sessiz boşluğu şudur: **hangi konfigürasyonla?**

Test/E2E konfigürasyonu bu gereği "tatmin eder" — sahte servisler, sabit test değerleri, ayrı bir compose dosyası. Gerçek dağıtım konfigürasyonu ise hiç denenmemiş olabilir.

> **Vaka:** Bir projede son faz gate'i ✓ PASS aldı: sekiz uçtan uca senaryo yeşildi, ortam ayağa kalkıyordu, migration temizdi. Ama o ortam **E2E harness'ının kendi konfigürasyonuydu**. Gerçek dağıtım konfigürasyonu ilk kez kapanıştan sonra denendi ve **hiç açılmadı**: zorunlu ayarlar servise geçirilmiyordu (fail-fast), servisler arası kimlik anahtarı iletilmiyordu, kimlik sağlayıcı geri dönüş adresleri ölü bir alan adına bakıyordu. Hiçbir test bunu yakalayamazdı — çünkü hiçbir test o konfigürasyonu kullanmıyordu.

**Kural:** Kapanış ritüelinde sistem **gerçek dağıtım konfigürasyonuyla** en az bir kez ayağa kaldırılır. Prova şunları kanıtlar:

1. Zorunlu ayarların **tamamı** ortama gerçekten geçiyor (§A listesi çalışıyor)
2. Servisler/bileşenler arası kimlik ve adresler gerçek değerlerle çözülüyor
3. Sağlık uçları gerçek konfigürasyonda yanıt veriyor
4. Boot reçetesi runbook'ta yazılı olduğu gibi **tekrarlanabilir** (ikinci kişi aynı adımlarla açabilir)

Prova sonucu `DEPLOY_RUNBOOK.md`'ye yazılır: ne çalıştı, ne kırıldı, hangi ayar eksikti. **Kırılan her şey bir runbook satırına dönüşür** — bu provanın asıl çıktısı budur.

**Kapsam notu:** Bu playbook prod operasyon pratiğini (sürüm döngüsü, incident yönetimi, canlı izleme) **içermez** — o dönem yaşanmadan yazılmaz. Runbook katmanı deploy'a kadar götürür; ötesi bir sonraki sürümün konusudur.

---

## M. Runbook katmanı

İhtiyaç anında doğan ama gün 0'da yeri tanımlı olması gereken dosyalar:

| Dosya | Ne zaman doldurulur | İçerik |
|---|---|---|
| `CI_CD_SETUP.md` | İlk CI task'ında | Workflow envanteri, repo ayarları, secret listesi, dal koruma rejimi, yükseltme yolu |
| `DEPLOY_RUNBOOK.md` | Entegrasyon fazından itibaren, kapanışta finalize | Zorunlu ayarlar (yoksa açılmaz) · operasyonel secret'lar · önerilen ayarlar (yoksa kural sessiz kalır) · deploy sonrası doğrulama |
| `INTEGRATION_RUNBOOKS/<SERVIS>.md` | Her dış servis entegrasyonunda | Hesap açma, key alma, kota, webhook kurulumu, test etme, sorun giderme |
| `BYPASS_LOG.md` | Otomatik (hook) | Her disiplin bypass'ı |

**Kural:** Bir dış servise ilk kez bağlanan task, aynı PR'da o servisin runbook'unu da yazar. "Sonra yazarız" denen kurulum bilgisi ikinci kez lazım olduğunda kaybolmuş olur.

---

## N. Öğrenimler

> Bu bölüm §K uyarınca **her aşama ve her faz kapanışında** doldurulur. Boş bırakılmış bir öğrenim satırı, kapanmamış bir gate demektir.

### N.1 Dönem 1 — Doküman üretimi

*(Bu projede henüz aşama kapanmadı.)*

### N.2 Dönem 2 — Implementation

*(Bu projede henüz faz kapanmadı.)*

### N.3 Dönem 3–4 — Borç kapatma ve kapanış

*(Bu projede henüz dönem kapanmadı.)*

---

## O. Altın kurallar (özet)

1. Ajana "tüm sistemi yaz" deme — küçük, bağımsız, test edilebilir parçalar ver.
2. Her task kendi başına test edilebilir olmalı.
3. Bağımlılıklar açıkça tanımlı olmalı.
4. Yapan ≠ denetleyen. Her zaman.
5. Kanıtsız PASS geçersiz.
6. Belirsizlik ajanın düşmanıdır — "muhtemelen"ler kodlamaya geçmeden karara dönüşür.
7. Tıkanma durumunda işi böl, erteleme.
8. Kırık CI üstüne iş yapılmaz.
9. Ertelenen iş yazılmadan ertelenmiş sayılmaz.
10. Öğrenim yazılmadan aşama kapanmaz.

---

*Project Playbook — Metodoloji v1.0.1*

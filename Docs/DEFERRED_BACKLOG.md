# Shopfolio — Ertelenmiş İşler Backlog'u

**Son güncelleme:** 2026-11-01 | **Aktif kalem:** 1 | **Gate'i bloklayan:** 1

> **Amaç:** Bilinçli olarak ertelenen **her** somut işin tek izlenebilir listesi.
>
> **Neden gerekli:** Validator ve gate bulgularının bir kısmı "gerçek ama şimdi değil" kararıyla kapanır. Bu karar yalnızca task raporunun içinde kalırsa **buharlaşır** — yüz rapor sonra kimse o satırı bulamaz.
>
> **Kritik ayrım:**
> - **Ertelenen iş** → buraya. MVP kapsamında ama sonraya bırakıldı.
> - **Kapsam dışı** → [`10_MVP_SCOPE.md`](10_MVP_SCOPE.md) §3. Erteleme değil, MVP tanımı gereği hariç.
> - **Tamamlanan iş** → [`IMPLEMENTATION_STATUS.md`](IMPLEMENTATION_STATUS.md).
>
> Bu ayrım borç kapatma aşamasında (00 §I) hayati önem kazanır: neyin kapatılacağı buradan çıkar.

---

## Lejant

- **Öncelik:** 🔴 high · 🟡 medium · ⚪ low · ✅ çözüldü
- **Tip:** `task` (planlı görev) · `backend-gap` (kod boşluğu) · `k-note` (validator notu) · `code-todo` · `doc-drift` (doküman ↔ kod uyumsuzluğu) · `test-gap` · `bypass`
- **Bloklar mı:** kalemin engellediği şey; "—" = hiçbir şeyi bloklamıyor

---

## Kayıt kuralları

1. **Kanıt zorunlu.** Her kalem bir kod referansı (dosya:satır) veya rapor bağlantısı taşır. Kanıtsız kalem bir süre sonra doğrulanamaz hâle gelir.
2. **Satır silinmez.** Çözülen kalem `✅ Çözüldü → <nerede>` olarak işaretlenir. İzlenebilirlik korunur.
3. **Backlog forward zorunlu.** Validator ve gate'te çıkan bloke-etmeyen her bulgu ya o PR'da kapatılır ya buraya yazılır. **Üçüncü seçenek yoktur.**
4. **Hijyen taraması.** Her faz gate'inde açık kalemler kod düzeyinde yeniden doğrulanır — bir kısmı araya giren işlerle çözülmüş, bir kısmı stale olmuş olur (açıklama artık gerçeği yansıtmıyor).

---

## 🔝 Öne çıkanlar (dikkat gerektiren)

| Önc. | ID | Özet | Bloklar mı |
|---|---|---|---|
| 🔴 | D-01 | SETUP §2 + §4 kurulumda tamamlanmadı — `SETUP_COMPLETE=false` | Dönem 2 girişi (ilk implementation task'ı) |
| ✅ | D-02 | ✅ Çözüldü → K-1505 (`INSTRUCTIONS.md` §3.3) — yardımcı izole doğrulama ajanını açar, PASS + CI yeşilse merge eder | — |

---

## 1. Kurulum (SETUP.md)

| Önc. | ID | Açıklama | Tip | Hedef | Kanıt / kaynak |
|---|---|---|---|---|---|
| 🔴 | D-01 | **SETUP §2 + §4 ertelendi.** Doldurulmayanlar: (a) §2 — teknoloji yığını, `.gitignore`/`.gitattributes` yığın uyarlaması, projeye özgü sır dosyası desenleri, `.env.example`; (b) §4 — `LINT_CMD` (`Consistency` özelliğiyle işaretli beş eşitlik denetimi Lint'te koşar, Unit'te dışarıda kalır — `09 §21.4` madde 1; K-1356), `BUILD_CMD`, `UNIT_TEST_CMD`, `INTEGRATION_TEST_CMD`, `CONTRACT_TEST_CMD`, `MIGRATION_DRYRUN_CMD`, `PACKAGE_BUILD_CMD`, `E2E_CMD`, `ci.yml` runtime kurulum adımları, `publish.yml` silinsin mi kararı (silinmezse: etiket imaja basılmadan önce "yama sürümü göç taşımaz" denetimi — `09 §21.4` madde 2; K-1356), imaj paketlerinin ilk yayından sonra bir kez açık yapılması (Teknik Mimari `05 §8.2.5`; K-981); (c) doğrulanacaklar (bilgim tarihli; `05 §2.3.8`) — yedek araçlarının güncel nesneyi silme yetkisi (`05 §7.6.10`; K-961), pgBackRest'in resmî konteyner imajı (`05 §8.2.2`; K-937), veritabanının işlemlerin kesinleşme anını tutması (`05 §8.5.11`; K-962), paketlerin görünürlüğü (`05 §8.2.5`; K-981), konteyner trafiğinin güvenlik duvarındaki zinciri ve pgBackRest'in yerel soket bağlantısı (`05 §3.2.5`; K-987); ardından `SETUP_COMPLETE=true`. **Gerekçe:** yığın Aşama 4'ün kararıdır (SETUP §2 uyarısı) — kurulumda seçmek, teknik kararı doküman aşamasından kaçırmak olurdu. **Sonuç:** CI iskeleti şu an yeşil ama hiçbir şey çalıştırmıyor; §8 kontrol 1 ve 2 bu kalem kapanana kadar geçmez. | `task` | Aşama 4 kapanışından sonra, **ilk implementation task'ından önce** | [`.github/ci-commands.env:19`](../.github/ci-commands.env#L19) (`SETUP_COMPLETE=false`) · [`SETUP.md`](../SETUP.md) §2 uyarısı, §4, §8 · **Aşama 4'ün kararları** (karar kaydı §2; Teknik Mimari `05 §2.3` — D-01'in her parçasına verilenler, yazım turunun 1. oturumu, v0.2 · §8.3 — yazım turunun 2. oturumu): yığın K-866, K-867 · paketleme ve kayıt defteri K-920 · CI adımlarının işi, `publish.yml`'in kalması ve sürüm etiketi K-921 · `.env.example` K-910 · `CONTEXT.md` §3'ün kod klasörleri K-932 (Kodlama Kılavuzu ile) · veritabanı imajı ve yedek aracı imajı K-937 · CI'ın göç adımı K-965 — Teknik mimari aşamasının kapanışının çakışma taramasında tamamlandı (`CHECKPOINT_REPORTS/PHASE4_CONFLICT_SCAN.md` §6.4) · paketlerin görünürlüğü ve CI'ın geri alma sınaması K-981 · doğrulanacaklar K-961, K-962, K-987 — Teknik mimari aşamasının kapanışının checkpoint'inde eklendi (`CHECKPOINT_REPORTS/CP04_PHASE4_CHECKPOINT.md`) |
| ✅ | D-02 | **✅ Çözüldü → K-1505 (2026-11-01; `INSTRUCTIONS.md` §3.1, §3.2, §3.3; `skills/validate`, `skills/task`; genel `SubAgentIndependentValidation` skill'i §0).** Validator yardımcının açtığı izole doğrulama ajanı olarak koşar (talimat gösterilmez, "başlat" beklenmez), 16. adımda verdict'i rapor dosyasına yazıp biter; yardımcı rapor dosyasında PASS okur, dal CI'ı yeşilse merge eder. Bekçi değişmedi. **Önceki kayıt:** **Yönetici yardımcı düzeninin implementation akışıyla uyumu.** Düzen implementation döneminde de açıktır (K-1504). Düğme açıkken yalnız yardımcı ve ana oturum `gh pr merge` çalıştırabilir; task PR'ını ise validator merge eder (`INSTRUCTIONS.md` §3.2, `skills/validate`). Kurulacak: validator'ın hangi bağlamda koştuğu (ayrı ana oturum ya da yardımcının işçisi) ve buna göre bekçinin merge kuralı — validator işçi olarak koşacaksa PASS sonrası merge'ü yardımcı yapar ya da bekçiye validator istisnası eklenir; yapanın denetlememesi kuralı (`skills/validate`) korunur. Bekçi proje dışıdır (`~/.claude/hooks/`, `~/.claude/rules/agent-rules.json` > `helperMode`); değişiklik staging + `deploy.js` ile yayına alınır. **Gerekçe:** doküman döneminde merge'ü yönetici yapıyordu; implementation'da merge eden rol değişiyor ve bekçi bunu bilmiyor. | `task` | Doküman dönemi kapandıktan sonra, **ilk task'ın doğrulamasından önce** | Karar kaydı K-1504 · `INSTRUCTIONS.md` §9 ("Yönetici yardımcı düzeni") · `~/.claude/rules/agent-rules.json` > `helperMode` |

---

## Post-MVP parkı

> MVP kapanışında (00 §L adım 2) buraya taşınan, bilinçli olarak sonraki sürüme bırakılan kalemler.

| Önc. | ID | Açıklama | Tip | Kanıt / kaynak |
|---|---|---|---|---|

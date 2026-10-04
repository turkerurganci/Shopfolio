# Shopfolio — Playbook Geri Bildirimi

**Son güncelleme:** 2026-10-04 | **Satır:** 46 | **Gönderilen:** 0

> **Amaç:** Bu projede öğrenilip [project-playbook](https://github.com/turkerurganci/project-playbook)'a (bu repo v1.1.0'dan kuruldu) geri gitmesi gereken **her** şeyin tek listesi.
>
> **Gönderim:** Proje tamamlandıktan sonra, proje sahibi gönderir (K-649). O güne kadar liste yalnız büyür.

---

## Kayıt kuralları

1. **Ne girer:** playbook'tan gelen bir dosyaya dokunan ya da dokunması gereken her öğrenim — bu repoda uygulanmış olsun olmasın. Bu dosyalar: `00`, `CLAUDE.md`, `SETUP.md`, `.claude/` (talimatlar, checklist'ler, skill'ler, hook'lar), `scripts/`, `.github/`, doküman ve rapor şablonları. Yalnız Shopfolio'nun ürününe ait kural girmez.
2. **Ne zaman:** öğrenim doğduğunda. En geç aşama kapanışının öğrenim terfisi adımında (`checklists/document-stage.md` §7, 5. adım) ya da faz gate'inin öğrenim adımında (`skills/gate-check` Adım 7).
3. **İçerik kopyalanmaz:** satır öğrenimin adını, kaynağını, bu repoda uygulandığı yeri ve playbook'taki hedefi yazar. Metin kaynağındadır.
4. **Satır silinmez.** Gönderilen satırın durumu `Gönderildi — <playbook PR'ı ya da sürümü>` olur.
5. **Tamlık ağı — gönderimden önce:** kurulumdan bu yana playbook dosyalarındaki bütün değişiklikler listeyle karşılaştırılır; listede karşılığı olmayan değişiklik satır olarak eklenir.

   ```
   git log --oneline e74883f..HEAD -- Docs/00_PROJECT_METHODOLOGY.md CLAUDE.md SETUP.md .claude/INSTRUCTIONS.md .claude/GUARDRAILS.md .claude/CONTEXT.md .claude/checklists .claude/skills .claude/hooks scripts .github
   ```

**Durum sözlüğü:** `Uygulandı` — bu repoda uygulandı, playbook'a gidecek · `Aday` — henüz karara bağlanmadı, kapısı satırda · `Onay bekliyor` — metodoloji değişikliği, proje sahibinin onayında · `Projeye özgü?` — gönderirken ayıklanacak · `Gönderildi`

---

## Liste

| # | Öğrenim | Kaynak | Bu repoda | Playbook'ta hedef | Durum |
|---|---|---|---|---|---|
| PF-01 | Bir konunun alt kararları ayrı numaralanır, tek soruda paketlenmez | PR #6 (2026-08-18) | `INSTRUCTIONS §2` | `.claude/INSTRUCTIONS.md` §2 | Uygulandı |
| PF-02 | Seçenekler sade dille, somut sonuç üzerinden yazılır | PR #6; H-2 | `INSTRUCTIONS §2` | `.claude/INSTRUCTIONS.md` §2 | Uygulandı |
| PF-03 | Durum sorusu kuralı döneme bağlı tracker'ı gösterir — beş yer yalnız `IMPLEMENTATION_STATUS.md` diyordu | PR #9 (2026-08-22) | `00 §G.1` (tablo ve vaka) · `INSTRUCTIONS §3.0`, §3.9 · `CLAUDE.md` · `memory/README.md` | Aynı dosyalar | Uygulandı |
| PF-04 | Soru açmadan önce kural katmanlarını tara | 2026-08-22; H-1 | `INSTRUCTIONS §2` | `.claude/INSTRUCTIONS.md` §2 | Uygulandı |
| PF-05 | Sorular ve seçenekler numaralanır | PR #12 (2026-08-30) | `INSTRUCTIONS §2` | `.claude/INSTRUCTIONS.md` §2 | Uygulandı |
| PF-06 | Konu kapanışında tamlık taraması; liste hazırlanırken koşulur, bloğun son konusunda bloğa genişler | PR #16, #27 | `INSTRUCTIONS §2` · checklist §3 | `.claude/INSTRUCTIONS.md` §2 · `checklists/document-stage.md` §3 | Uygulandı |
| PF-07 | Öneriyle kayıt yetkisi — kritik olmayan workshop soruları önerilerle kaydedilir | PR #22 (2026-09-17) | `INSTRUCTIONS §2` · `00 §C.2` | Aynı dosyalar | Uygulandı |
| PF-08 | PR base'i her zaman `main`; `ci.yml`'in `pull_request` tetikleyicisinde base filtresi yok | PR #24, #25 | `INSTRUCTIONS §3.2` · `.github/workflows/ci.yml` | Aynı dosyalar | Uygulandı |
| PF-09 | Konu başına tek mesaj — bütün kararlar numaralı öneri listesi, ⚠ işaretli | PR #26 (2026-09-18) | `INSTRUCTIONS §2` · checklist §3 | Aynı dosyalar | Uygulandı |
| PF-10 | Sessizlik yeşil değildir — CI izleyicisi tek kaynak sayılmaz, check-run sayısı sorulur | PR #15, #16, #23; H-3 | `INSTRUCTIONS §3.2` | `.claude/INSTRUCTIONS.md` | Uygulandı |
| PF-11 | Taslağı yazılmış bölümü etkileyen karar işaret taşır; blok kapanışında işaretsiz eşleşme taranır (K-29) | PR #29 (2026-09-19) | checklist §3 | `checklists/document-stage.md` §3 | Uygulandı |
| PF-12 | Dokümanlar adıyla anılır; özetler kısa ve sade, üç parça | PR #37; H-9 | `INSTRUCTIONS §2` | `.claude/INSTRUCTIONS.md` §2 | Uygulandı |
| PF-13 | Cross-review için yedek ikinci AI yöntemi | PR #38; Ö-17 | `SETUP.md` · `skills/cross-review` Faz 1 | `SETUP.md` (araç adı projeye özgü, "yedek yöntem" satırı genel) · `skills/cross-review` | Uygulandı |
| PF-14 | Template'ten devralınan, playbook'un kendi repo'suna ait `BYPASS_LOG` satırı | `Docs/BYPASS_LOG.md` kurulum notu (2026-08-11) | Satır temizlendi | `Docs/BYPASS_LOG.md` şablonu | Uygulandı |
| PF-15 | Toplu onay ve ⚠ öneriyle kayıt modları; gösterilmeyen ⚠ listesinin kapısı | Ö-1, Ö-18; H-4, H-11 | `INSTRUCTIONS §2` · `GUARDRAILS §6` · checklist §1, §7 | Aynı dosyalar | Uygulandı |
| PF-16 | Firmaya elle adım ekleyen kararın bütçeyle okunması | Ö-2 | checklist §3 | — | Projeye özgü? |
| PF-17 | Yazım ve kalite döngüsünün sıralaması; kapanışın altı adımı | Ö-3, Ö-21 | checklist §4, §7 | `checklists/document-stage.md` | Uygulandı |
| PF-18 | Blok planının "Hedef doküman" sütunu etki birleşimine betikle hizalanır | Ö-4 | checklist §3 | `checklists/document-stage.md` §3 | Uygulandı |
| PF-19 | K-29'un anlamsal taraması; işaret satırın her bölümünü kapsar | Ö-5, Ö-6 | checklist §3, §4 | `checklists/document-stage.md` | Uygulandı |
| PF-20 | Devrin adı yazılır, kapanışı aranır | Ö-7, Ö-8 | checklist §3, §4 · `00 §N.1` (7) | `checklists/document-stage.md` · `00 §N.1` | Uygulandı |
| PF-21 | Önceki kuralı daraltan karar eski satıra geri işaret bırakır | Ö-9 | checklist §3 | `checklists/document-stage.md` §3 | Uygulandı |
| PF-22 | Proje Vizyonu ve MVP Kapsamı şablonlarında açık kararlar bölümü yok | Ö-10 | checklist §7 (3. adım) · `00 §N.1` (9) | `Docs/01_PROJECT_VISION.md` ve `Docs/10_MVP_SCOPE.md` şablonları | Uygulandı (yalnız çevre kural; şablon değişmedi) |
| PF-23 | Vizyon dokümanına kural ayrıntısı taşınmaz, kurala işaret edilir | Ö-11 | checklist §4 · `00 §N.1` (4) | `checklists/document-stage.md` · `00 §N.1` | Uygulandı |
| PF-24 | Pre-commit sır guard'ı düz metni sır ataması sanıyor | Ö-12 | `scripts/git-hooks/pre-commit` · `README.md` | Aynı dosyalar | Uygulandı |
| PF-25 | İkinci model bilinçli karara itiraz etmesin — talimat cümlesi | Ö-13 | `skills/cross-review` Faz 1 | `skills/cross-review/SKILL.md` | Uygulandı |
| PF-26 | Kuralın yasal dayanağı resmî metnin güncel hâline karşı doğrulanır | Ö-14 | `skills/audit`, `skills/deep-review` | Aynı skill'ler | Uygulandı |
| PF-27 | Cross-review ciddiyet ölçüsü ve çıkış koşulu | Ö-15 | `skills/cross-review` Faz 1, Faz 4 · `00 §C.5` | Aynı dosyalar | Uygulandı |
| PF-28 | Kaynak satırlarının mekanik taraması | Ö-16 | `skills/cross-review` Faz 5 | `skills/cross-review/SKILL.md` | Uygulandı |
| PF-29 | K numarası tekil; paralel dallarda aralık ayrılır, çift numara taranır | Ö-19 | `INSTRUCTIONS §7` · checklist §3 | Aynı dosyalar | Uygulandı |
| PF-30 | Her oturum ve kapanış PR'ı Güncel Durum'u günceller, alt ajan dahil | Ö-20 | `INSTRUCTIONS §7` · checklist §3 | Aynı dosyalar | Uygulandı |
| PF-31 | Cross-review Faz 3'ün proje sahibinin açtığı modlarla hizalanması | Ö-22 | `skills/cross-review` Faz 3 | `skills/cross-review/SKILL.md` | Uygulandı |
| PF-32 | Checklist'in skill'e dönüştürülmesi | Ö-23 | — | `skills/` | Aday — kapı: Aşama 2'nin öğrenim terfisi |
| PF-33 | Metodolojinin başlık ve alt bilgi sürümü ayrışmış | Ö-24; checkpoint B-31 | `00` başlık ve alt bilgi | `00` | Uygulandı (PR #54) |
| PF-34 | Aşama 1 dersleri — `00 §N.1`'in dokuz deseni | PR #54 (2026-10-03) | `00 §N.1` | `00 §N.1` | Uygulandı |
| PF-35 | Yazım oturumu tek bağlamda yürür | H-6 | checklist §4 | `checklists/document-stage.md` §4 | Uygulandı |
| PF-36 | Karar kaydının başlığı dosyayı Aşama 1'e aitmiş gibi okutuyor; arşiv aşama sonunda değil dönem sonunda | K-647; tracker §8.1 (1) | Tracker başlığı · checklist §7 (5. ve 6. adım) | `Docs/PRODUCT_DISCOVERY_STATUS.md` şablon başlığı · `checklists/document-stage.md` §7 | Uygulandı |
| PF-37 | Devir notundaki soru şablona karşı taranmadan proje sahibine soruldu | K-647; tracker §8.1 (2) | — | `checklists/document-stage.md` §1 | Aday — kapı: Aşama 2'nin öğrenim terfisi |
| PF-38 | Playbook'a geri akışın evi yok — `CHANGELOG.md` "öğrenimler buraya geri akar" diyor, adımı ve dosyası tanımlı değil | K-649 | Bu dosya · checklist §7 (5. adım) · `skills/gate-check` Adım 7 | `00 §K` ve §L · bu dosyanın şablonu | Uygulandı |
| PF-39 | Proje sahibinin açtığı modlar ve yetkiler aşamayla sınırlı; yeni aşamada yeniden sorulur | Ö-1; H-10; K-648 | checklist §1 | `checklists/document-stage.md` §1 | Uygulandı |
| PF-40 | Kullanıcı Akışları şablonu: §0 anlatılar için "ayrı bölüm" ister ama şablonda o bölüm yok; açık kararlar bölümü de yok (PF-22'nin bu şablondaki hâli) | Tracker §8.1 öğrenim adayı 3 (2026-10-04) | Tracker §8.2 plan önerisi 2 (AK0-02) · §8.3 AK0-04 | `Docs/03_USER_FLOWS.md` şablonu | Aday — kapı: Aşama 2'nin öğrenim terfisi |
| PF-41 | Aşama planının konu kimliği aşamayı taşımıyor; karar kaydı dönem boyu tek dosya olunca sonraki aşamanın planı çakışır | Tracker §8.1 öğrenim adayı 4 (2026-10-04) | Tracker §8.2 (`AKn-mm`) | `checklists/document-stage.md` §1 | Aday — kapı: Aşama 2'nin öğrenim terfisi |
| PF-42 | Mekanik ön sayımın betiği mercekler başlamadan örneklenmedi; `\b03\b` kimliklere ve tarihlere takıldı ve envanter 57 yerine 131 satır oldu | Tracker §8.1 öğrenim adayı 5 (2026-10-04) | — | `skills/audit` "Koşum biçimi" 4. madde | Aday — kapı: Aşama 2'nin öğrenim terfisi |
| PF-43 | Paralel mercek sayısı oturum limitine bağlı; on mercek aynı anda düştü, beşli dalga ve bulgu buldukça dosyaya yazma kesintisiz bitti | Tracker §8.1 öğrenim adayı 6 (2026-10-04) | — | `skills/audit` "Koşum biçimi" | Aday — kapı: Aşama 2'nin öğrenim terfisi |
| PF-44 | Sonraki dokümana giden etki atıfları çıplak doküman numarası kalıyor ("06 · 12"); devir taraması neyin devredildiğini okuyamaz | Tracker §8.1 öğrenim adayı 7 (2026-10-04) | — | `checklists/document-stage.md` §3 | Aday — kapı: Aşama 2'nin öğrenim terfisi |
| PF-45 | Park satırı kaynak kararı değişince güncellenmiyor; geri işaret kuralı karar satırını kapsıyor, park satırını kapsamıyor (K-06 → K-97, dört doküman) | Tracker §8.1 öğrenim adayı 9 (2026-10-04, Aşama 2 çakışma taraması) | `04`, `06`, `07`, `12` park satırları hizalandı | `checklists/document-stage.md` §3 ve §7 (3. adım) | Aday — kapı: Aşama 2'nin öğrenim terfisi |
| PF-46 | Salt okunur aşama kaydındaki açık süreç maddesi sonraki aşamanın açık listesine geçmiyor (avukat teyidi önerisi, Ö-23) | Tracker §8.1 öğrenim adayı 10 (2026-10-04, Aşama 2 çakışma taraması) | Tracker §8.1 (iki madde taşındı) | `checklists/document-stage.md` §1 | Aday — kapı: Aşama 2'nin öğrenim terfisi |

# Otonom koşu betiği

**Katman:** L5 | **Son güncelleme:** 2026-10-17

Doküman döneminin adımlarını sırayla, her adımı ayrı ve kilitli bir Claude oturumuna vererek yürütür. Her adımın sonucunu mekanik olarak denetler. Denetimden geçen adımı PR olarak açar, CI'ı bekler ve aşamanın merge yetkisi açıksa merge eder. Kural değiştirmez: yönetici düzeninin (`INSTRUCTIONS §9`) mekanik kısmını yapar. **Oturum yalnız durdurabilir; devam etmeye ve merge etmeye betik karar verir.**

## Kim ne yapar

| Kim | Ne yapar |
|---|---|
| **Proje sahibi** | Yönetici chat'e "şu adımdan şu noktaya kadar git" der. Durduğunda sorulan soruyu cevaplar. Koşunun sonunda ⚠ listesini görür. |
| **Yönetici chat** | Plan dosyasını yazar ve betiği arka planda başlatır. Her adımın PR'ını okur. Betik durunca nedenini proje sahibine sade dille iletir ve cevabı betiğe verir. |
| **Betik** (`run.mjs`) | Her adım için dalı açar ve kilitli oturumu başlatır. Sonucu denetler; commit, PR, CI bekleme, merge ve merge sonrası CI'ı yapar. Repo hafızasını yerine koyar. Günlük tutar. |
| **Adım oturumu** | Adımın işini yapar (skill'ler, mercekler, karar kaydı, rapor) ve yalnız dosyaları düzenler. Proje sahibine soramaz. Karar gerekiyorsa durur. |

## Bir adım nasıl yürür

1. **Ön kontrol:** kilit testi bu Claude sürümüyle geçmiş mi, ana dalın son CI koşuları yeşil mi (GUARDRAILS §8), başka bir `docs:` PR'ı açık mı (K numarası çakışması), hook'lar kurulu mu. Sonra dal `origin/main`'den, ayrı bir çalışma klasöründe (`<repo>-otonom`) açılır. Senin çalışma klasörüne dokunulmaz.
2. **Kilitli oturum:** yeni bir `claude -p` oturumu açılır. Önceki konuşmaları, senin otomatik hafızanı ve başka oturumları göremez. Elinde git ve GitHub anahtarı yoktur. İzin listesi `lib/perm.mjs`'de.
3. **Kanıt:** izin redleri ve oturum kayıtları (alt ajanlarınki dahil) okunur. Yasak bir işlem denendiyse adım durur. Her alt ajanın hangi modelle çalıştığına bakılır.
4. **Oturum DUR dediyse** soru proje sahibine gider ve betik bekler.
5. **Denetim** (`lib/checks.mjs`):
   - **Dosya çiti:** yalnız `Docs/` değişebilir. `00`, `BYPASS_LOG`, `.claude/`, `scripts/` ve `.github/` dışarıda kalır. Silme ya da yeni doküman açma da durdurur.
   - **Rapor:** beklenen rapor yerinde mi.
   - **Sürümler:** karar kaydının sürümü artmış mı, değişen her dokümanın sürümü artmış mı.
   - **K numaraları:** ana daldaki son numaradan boşluksuz devam ediyor mu, çift numara var mı.
   - **Satır sonları:** CR (Windows satır sonu) var mı.
   - **`$1` artığı:** kod bloğu dışında bir yerde kalmış mı.
   - **AI atıf satırı** var mı.
   - **Aşama sınırı:** adım arşiv işareti koymuş ya da yeni aşama açmış mı.
   - **⚠ kararlar:** gösterilmeden kaydedilenler listeye alınır. Oturum birini listelemese bile karar kaydındaki işaretten bulunur.
6. **Commit ve PR:** betik commit'ler, böylece hook'lar (sır taraması dahil) çalışır. Push eder ve PR açar.
7. **CI:** check sayısı sıfırdan büyük olmalı, hepsi bitmeli ve `CI Gate` yeşil olmalı (`INSTRUCTIONS §3.2`, "Sessizlik yeşil değildir").
8. **Merge:** yalnız denetlenen commit merge edilir (`--match-head-commit`). Merge yetkisi kapalıysa betik PR'ı açık bırakır ve durur. Dal silinmez; dal silme yetkinin dışındadır.
9. **Merge sonrası:** ana daldaki CI ve Publish yeşil olmalı.

## Nerede durur

Betik aşağıdaki durumlarda durur, projeye bir şey eklemez ve durma nedenini yazar:

1. **Karar gerekiyor.** Oturum çok kritik bir soruda durdu. Soru proje sahibine gider; cevap aynı oturuma verilir ve oturum kaldığı yerden sürer.
2. **Bir kontrol tutmadı.** Yukarıdaki denetimlerden biri geçmedi.
3. **CI yeşil değil** (adımın PR'ında ya da merge sonrasında). Betik kendi başına düzeltme denemez.
4. **Cross-review yakınsamadı.** Döngü belirlenen tur sayısında (varsayılan 10) TEMİZ dönmedi. Ciddiyet ölçüsü ya da devam kararı proje sahibinindir.
5. **Bir sınır doldu.** Tur, süre ya da harcama sınırı doldu, ya da Opus kullanılamıyor. Daha zayıf bir modele geçilmez.
6. **Merge yetkisi kapalı.** PR hazır, merge proje sahibinde.
7. **Ön kontrol tutmadı.** Kilit testi bu sürümle geçmemiş, ana dal kırmızı ya da başka bir `docs:` PR'ı açık.
8. **Plan bitti** ya da durdurma isteği geldi.

Betik şu adımları **hiç koşmaz**: aşama açılışı, workshop, öğrenim terfisi ve arşiv işareti. Bunlar ya proje sahibine soru sorar ya da `.claude/` dosyalarını değiştirir; yönetici chat'le, bugünkü gibi yürür. Arşiv işaretinden önce ⚠ listesinin gösterilmesi de bu yüzden elle kalır.

## Modeller ve ayarlar

| İş | Model | Düşünme |
|---|---|---|
| Her adım oturumu (yazım, audit, deep review, cross-review değerlendirmesi, etki yansıtma, çakışma taraması, checkpoint) | Opus 5.5 | high |
| Bulgu bulan, doğrulayan ya da karar veren alt ajanlar (mercekler, şüpheciler) | Opus 5.5 (ana oturumdan) | — |
| Mekanik alt işler: sayma, listeleme, bir atfın var olup olmadığına bakma (`.claude/agents/mekanik.md`) | Haiku 5.5 | low |
| Metin taslağı: PR açıklaması, hafıza paragrafı (`.claude/agents/yazici.md`) | Sonnet 5.5 | medium |
| İkinci model (cross-review) — yalnız `codex.mjs` ile, sabit bayraklarla | Codex `gpt-5.6-terra` | high |

Betik, Opus dışında çalışan her alt ajanı oturum kayıtlarından bulur. Bu alt ajan yukarıdaki iki türden değilse adım durur. Adım başına sınırlar `lib/config.mjs`'de: 500 tur, 200 $ (API eşdeğeri tahmin) ve 300 dakika. Plan bunları adım başına değiştirebilir.

## Kurallardan bilinçli ayrılan iki nokta

1. **Zararsız izin redleri adımı durdurmaz.** Oturum izin listesinde olmayan bir komut biçimi denerse (ör. `cd x && grep …`) reddedilir. Bu redler özete yazılır, adım sürer. Yasak bir işlem denemesi ise adımı durdurur: push, commit, `gh`, `--no-verify`, başka oturumların kayıtları, `codex`'i doğrudan çağırmak. **Neden:** ön denemede adım üç kez zararsız `cd` birleşimleri yüzünden durdu. Zararsız redler istenmeyen hiçbir şeyi geçirmiyor: izin listesi zaten reddediyor ve oturumun anahtarı yok.
2. **Henüz yazılmamış dokümanda (v0.1) sürüm artışı aranmaz.** Yazılmamış dokümanlara park satırı sürüm artışı olmadan ekleniyor. Ana daldaki son 60 PR'ın 25'inde böyle oldu; yazılmış dokümanların sürümü ise her değişiklikte arttı. Sürümü artmadan değişen v0.1 doküman günlüğe yazılır.

## Yönetici chat için komutlar

Komutlar ana çalışma klasöründen çalıştırılır:

```bash
node scripts/autopilot/kilit-testi.mjs                      # Claude güncellendikten sonra bir kez
node scripts/autopilot/run.mjs start --plan plan.json --detach
node scripts/autopilot/run.mjs monitor                      # izleme ekranı: http://127.0.0.1:4317/
node scripts/autopilot/run.mjs wait --until step            # bir adım bitene ya da betik durana kadar bekler
node scripts/autopilot/run.mjs status                       # özet
node scripts/autopilot/run.mjs answer "1 · …" --detach      # proje sahibinin cevabı; aynı oturum sürer
node scripts/autopilot/run.mjs resume --detach              # düzeltmeden ya da elle merge'den sonra
node scripts/autopilot/run.mjs stop                         # adım sınırında dur
node scripts/autopilot/run.mjs reset-step                   # adımın commit'lenmemiş işini siler — yalnız proje sahibinin onayıyla
```

Günlük, durum ve her adımın oturum kaydı `.autopilot/runs/<koşu>/` altındadır. Bu klasör Git'e girmez. `ozet.md` proje sahibine gösterilecek özettir: adımlar, PR'lar, ⚠ listesi, çok kritik sorular ve zararsız redler.

### Plan dosyası

```json
{
  "title": "Veri Modeli kalite döngüsü",
  "stage": 5,
  "stageName": "Veri modeli",
  "mergeAuthority": "K-992",
  "document": "Docs/06_DATA_MODEL.md",
  "steps": [
    { "id": "06-audit-deep", "type": "audit-deep-review" },
    { "id": "06-cross-review", "type": "cross-review", "maxRounds": 10 },
    { "id": "kapanis-cakisma", "type": "conflict-scan" },
    { "id": "kapanis-checkpoint", "type": "checkpoint" }
  ]
}
```

- **Adım türleri:** `writing`, `audit`, `deep-review`, `audit-deep-review`, `cross-review`, `conflict-scan`, `checkpoint` ve `custom`. `writing` ve `custom` bir `task` metni ister; `custom` ayrıca `label` ve isteğe bağlı `reports` alır.
- **Ek alanlar:** her adım `note` (yöneticinin notu), `model`, `effort`, `maxTurns`, `maxBudgetUsd` ve `timeoutMin` alabilir.
- **`mergeAuthority`:** karar kaydında o aşama için merge yetkisi veren K satırıdır; betik satırı ana dalda okuyup doğrular. Alan yoksa her PR proje sahibinin merge'ünü bekler.

## Dosyalar

| Dosya | Ne |
|---|---|
| `run.mjs` | Betik: plan, adımlar, komutlar, izleme sunucusu |
| `lib/config.mjs` | Modeller, sınırlar, adım türleri, dosya çiti |
| `lib/perm.mjs` | Adım oturumunun izin listesi ve yalıtım kilitleri |
| `lib/checks.mjs` | Denetimler (birim testli: `node --test "scripts/autopilot/test/*.test.mjs"`) |
| `lib/session.mjs` | Oturumu başlatma, anahtarsız ortam, oturum kayıtlarını okuma |
| `lib/github.mjs` | PR, CI bekleme, merge, merge sonrası |
| `lib/prompts.mjs` | Adım oturumuna verilen kurallar ve görev metinleri |
| `codex.mjs` | İkinci modelin sabit bayraklı sarmalayıcısı; her çağrı SHA-256 ile kayda geçer |
| `kilit-testi.mjs` | Yalıtım testi: altı kontrol (konuşma, hafıza yazma/okuma, eski kayıt, oturum ağı, alt ajan, GitHub anahtarı) |
| `izle.html` | Canlı izleme ekranı |

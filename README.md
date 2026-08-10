# Project Playbook

**Fikirden çalışan MVP'ye, bir AI ajanıyla.** Uygulama- ve teknoloji-bağımsız bir GitHub template.

**Sürüm:** v1.1.0 · **Doküman dili:** Türkçe · **Kod dili:** İngilizce

---

## Bu nedir

Bir yazılım projesini fikir aşamasından çalışan bir MVP'ye taşıyan **tam bir çalışma sistemi**: 10 doküman üretim aşaması (13 doküman), faz bazlı implementasyon döngüsü, bağımsız doğrulama protokolü, MVP borç kapatma aşaması ve kapanış ritüeli — hepsi kapılarla, kayıtlarla ve `exit 1` veren mekanik katmanlarla birlikte.

Bu bir "best practices" listesi değildir. Gerçek bir projede fikirden MVP'ye kadar **fiilen uygulanmış** bir yöntemin son hâlidir; buradaki her savunma katmanı, o projede gerçekleşmiş somut bir başarısızlığın karşılığıdır.

---

## Neyi çözer

Bir AI ajanıyla yazılım üretmenin iki büyük başarısızlık modu:

1. **Belirsizlik** — ajan dokümanda olmayan bir kuralı uydurur, sessizce kapsam genişletir, "muhtemelen böyledir" diye ilerler.
2. **Görünmez borç** — her adım tek başına yeşil görünürken bütün çürür: kırık CI'nin üstüne merge yapılır, bir task'ın kodu başkasının PR'ına gömülür, "sonra hallederiz" denen iş kaybolur.

Playbook'un tamamı bu iki moda karşı kurulmuş kapılar ve kayıtlardan ibarettir.

---

## Değişmez ilke

> **Kapsam esner, kapı esnemez.**

Küçük bir projede bir doküman bir sayfa olabilir — ama yazılır, izlenebilirlik matrisi kurulur, audit'i yapılır, gate'inden geçer. S/M/L "hafif mod" yoktur; ölçek bağımsızlığı adım atlayarak değil, adımları teknoloji ve domain'den arındırarak sağlanır.

---

## Hızlı başlangıç

1. **Use this template** → yeni repo (temiz geçmişle doğar)
2. [`SETUP.md`](SETUP.md)'yi baştan sona işlet — proje adı, teknoloji parametreleri, dal koruma rejimi, CI komutları, git hook'ları
3. İlk oturumu [`.claude/checklists/document-stage.md`](.claude/checklists/document-stage.md) ile başlat (Aşama 1 — Product Discovery)

---

## Yapı

```
CLAUDE.md                    AI giriş noktası
SETUP.md                     0. gün kurulum checklist'i

Docs/
  00_PROJECT_METHODOLOGY.md  ★ Sürecin kendisi — aşamalar, kapılar, konvansiyonlar
  01…12                      Ürün/tasarım/teknik/plan doküman iskeletleri
  IMPLEMENTATION_STATUS.md   Task durumu için tek otoriter kaynak
  DEFERRED_BACKLOG.md        Ertelenen her işin tek listesi
  DEBT_CLOSURE_PLAN.md       MVP borç kapatma iş paketleri
  DEPLOY_RUNBOOK.md          Prod ayar/secret listesi
  BYPASS_LOG.md              Disiplin bypass kayıtları (otomatik)
  …_REPORTS/                 Kanıt arşivi + şablonlar

.claude/
  INSTRUCTIONS.md            Oturum davranışı
  GUARDRAILS.md              Sınırlar ve yasaklar
  CONTEXT.md                 Klasör haritası (dosya listesi değil)
  skills/<ad>/SKILL.md       task · validate · gate-check · audit · deep-review
                             cross-review · checkpoint · handoff  (slash command)
  checklists/                Doküman üretim aşaması işletim checklist'i
  memory/                    Proje hafızası + terfi kuralı
  settings.json              Paylaşılan ajan ayarları (izlenir)
  hooks/                     Opsiyonel oturum hook'ları (hatırlatıcı, L5 değil)

scripts/git-hooks/           pre-commit (sır guard'ı) + commit-msg
                             + pre-push (3 katman) + kurulum
.github/                     CI iskeleti + PR şablonu
```

---

## Çekirdek döngü

```
/task TXX      →  yapım chat'i (8 maddelik bitiş kapısı)
/validate TXX  →  AYRI chat, rapor görülmeden, kanıtla; merge burada yapılır
/gate-check FX →  AYRI chat, faz kapanışı + öğrenim terfisi + backlog hijyeni
```

**Yapan ≠ denetleyen.** Her zaman.

---

## Öne çıkan mekanizmalar

| Mekanizma | Ne yapar |
|---|---|
| **Başlangıç kapıları** | Kirli working tree, kurulmamış hook, kırık ana dal CI veya güncellenmemiş hafıza ile hiçbir oturum başlamaz |
| **Dört katmanlı hook** | Sır sızıntısı · direct push · kırık CI üstüne push · bir task'ın kodunun başka bir PR'a gömülmesi — dördü de `exit 1` |
| **Boot provası** | "Servisleri ayağa kaldır" test konfigürasyonuyla tatmin edilebilir; kapanışta **gerçek dağıtım konfigürasyonu** zorunludur |
| **Dış varsayım ön-uçuş kontrolü** | Kod yazılmadan önce plan/sürüm/platform/sözleşme varsayımları **kanıtla** doğrulanır |
| **Bitiş kapısı** | PR açılmadan, CI **sonuçlanmadan** ve hafıza güncellenmeden task "bitti" sayılmaz |
| **Ertelenmiş işler backlog'u** | "Şimdi değil" kararı yazılmadan erteleme sayılmaz |
| **MVP borç kapatma aşaması** | "Plandaki task'lar bitti" ≠ "MVP çalışıyor" — aradaki farkı kapatan isimlendirilmiş aşama |
| **Öğrenim terfisi** | Bir faz, öğrenimi yazılmadan kapanmaz; tekrarlanacak dersler kurala dönüşür |
| **Eksik komut kapısı** | Kurulum tamamlandı işaretlendikten sonra boş bırakılmış bir CI adımı sessizce geçemez |

---

## Katkı ve sürümleme

Template kendi sürümünü taşır ([`CHANGELOG.md`](CHANGELOG.md)). Yeni projelerde ortaya çıkan öğrenimler buraya geri akar: bir projede kurala terfi eden bir ders, playbook'un bir sonraki sürümünde gün 0'da bulunur.

## Lisans

[MIT](LICENSE). "Use this template" ile doğan projeler serbestçe kullanır, değiştirir ve dağıtır.

# Shopfolio

Herhangi bir firmanın kendi ürünlerini sergileyip çevrimiçi satabileceği ve kurumsal tanıtımını yapabileceği; üyelik, Google/Facebook ile giriş, sipariş, ödeme ve sipariş takibi içeren web uygulaması.

**Doküman dili:** Türkçe · **Kod dili:** İngilizce

---

## Durum

> **Dönem: Doküman üretimi · Aşama 1 (Product Discovery)**
>
> Henüz uygulama kodu yok. Teknoloji yığını **bilinçli olarak seçilmedi** — o karar Aşama 4'e (`Docs/05_TECHNICAL_ARCHITECTURE.md`) aittir.

| | |
|---|---|
| Playbook sürümü | [project-playbook](https://github.com/turkerurganci/project-playbook) **v1.1.0** |
| Kurulum | SETUP §1, §3, §5, §6, §7 tamam · §2, §4 Aşama 4'e ertelendi ([`DEFERRED_BACKLOG.md`](Docs/DEFERRED_BACKLOG.md) D-01) |
| Dal koruma rejimi | **B — sistem-enforced** (ruleset aktif, `CI Gate` required) |
| CI | İskelet yeşil, `SETUP_COMPLETE=false` — adımlar henüz komut çalıştırmıyor |

---

## Kapsam (fikir düzeyi)

Bu bir **jenerik / white-label** uygulamadır: belirli bir firmaya bağlı değildir, kurulan her örnek kendi ürün kataloğunu ve kurumsal içeriğini taşır.

- Ürün listeleme ve ürün detayı
- Firma tanıtımı / kurumsal sayfalar
- Üyelik; Google veya Facebook hesabıyla giriş
- Sipariş oluşturma
- Ödeme
- Sipariş takibi

> Bunlar fikrin ham hâlidir, gereksinim değildir. Problem tanımı, hedef kullanıcı, kabul kriterleri ve MVP sınırı Aşama 1–10'da üretilir; **otoriter kaynak dokümanlardır**, bu liste değil.

---

## Nasıl çalışılıyor

Bu proje [Project Playbook](https://github.com/turkerurganci/project-playbook) v1.1.0 üzerine kuruludur: fikirden MVP'ye 10 doküman üretim aşaması, faz bazlı implementasyon döngüsü, bağımsız doğrulama protokolü ve `exit 1` veren mekanik kapılar.

**Çekirdek döngü** — *yapan ≠ denetleyen*, her zaman:

```
/task TXX      →  yapım chat'i (8 maddelik bitiş kapısı)
/validate TXX  →  AYRI chat, rapor görülmeden, kanıtla; merge burada yapılır
/gate-check FX →  AYRI chat, faz kapanışı + öğrenim terfisi + backlog hijyeni
```

| Nereye bakmalı | Ne için |
|---|---|
| [`CLAUDE.md`](CLAUDE.md) | AI giriş noktası — her oturumda okunacak dosyalar |
| [`Docs/00_PROJECT_METHODOLOGY.md`](Docs/00_PROJECT_METHODOLOGY.md) | Sürecin kendisi: aşamalar, kapılar, konvansiyonlar |
| [`Docs/PRODUCT_DISCOVERY_STATUS.md`](Docs/PRODUCT_DISCOVERY_STATUS.md) | Doküman üretim döneminin karar kaydı |
| [`Docs/IMPLEMENTATION_STATUS.md`](Docs/IMPLEMENTATION_STATUS.md) | Task durumu için tek otoriter kaynak |
| [`Docs/CI_CD_SETUP.md`](Docs/CI_CD_SETUP.md) | Pipeline, repo ayarları, dal koruma kanıtları |
| [`SETUP.md`](SETUP.md) | 0. gün kurulum kaydı ve proje parametreleri |

---

## Klonladıysanız: ilk iş

```bash
bash scripts/git-hooks/install.sh
git config core.hooksPath          # beklenen: scripts/git-hooks
```

**Hook kurulmamış bir klonda sır guard'ı, bundled-PR kontrolü ve kırık-CI kontrolü yoktur.** Sunucu tarafındaki ruleset yalnız direct push'u ve force-push'u durdurur; diğer üç katman yalnız lokalde yaşar.

---

## Lisans

[MIT](LICENSE).

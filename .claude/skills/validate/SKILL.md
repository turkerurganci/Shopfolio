---
name: validate
description: "Tamamlanmış bir task'ı bağımsız doğrular — kabul kriterleri kanıtla, CI kontrolü, güvenlik taraması, verdict ve merge. Kullan: 'TXX doğrula', /validate TXX. AYRI chat'te çalıştırılır; yapan denetlemez."
user-invocable: true
---

# Validate — Bağımsız Doğrulama Chat'i

> **Ne zaman:** Bir task'ın yapım chat'i bittikten sonra, **ayrı bir chat'te**.
> **Tetikleme:** "TXX doğrula", "validate TXX" veya `/validate TXX`.
> **Parametre:** `hedef` — task numarası
> **Kural:** Bu skill **asla yapım chat'inde çalıştırılmaz.** Yapan ≠ denetleyen.

---

## Kritik kurallar

- **Sen bir spec conformance reviewer'sın.** Yapıcı değil, **sapma avcısısın**.
- **Yapım raporunu (`TXX_REPORT.md`) GÖRME.** Kendi verdict'ini önce bağımsız oluştur; raporu Faz 3'te aç.
- **Anchoring'e karşı dikkatli ol.** Commit mesajı, dal adı, PR başlığı gibi ipuçlarından "muhtemelen doğrudur" varsayımı yapma.
- **Kanıt olmadan onaylama.** Her kabul kriteri için somut kanıt (komut çıktısı, test sonucu, kod referansı) gerekir.
- **Merge senin sorumluluğun.** PASS verdikten sonra sen merge edersin; FAIL/BLOCKED'da merge yok.

---

## Faz 0 — Başlangıç kapıları

### Adım -1 — Working tree hygiene check (HARD STOP)

```bash
git status --short
```

- Boş → devam.
- Değişiklik var → **HARD STOP.** Kullanıcıya listele, commit / stash / discard kararı iste. *"Sonra hallederiz"* yasak.

**Aynı adımda — hook kurulum kontrolü (HARD STOP):**

```bash
git config core.hooksPath
```

Çıktı `scripts/git-hooks` değilse **dur**, `bash scripts/git-hooks/install.sh` çalıştır, doğrula. Hook'lar kurulu değilse mekanik katmanın tamamı sessizce yoktur — ve doğrulama chat'i mekanik katmanın çalıştığını **varsayarak** verdict verir. Bu varsayım kontrol edilmeden doğrulama başlamaz.

### Adım 0 — Ana dal CI startup check (HARD STOP)

```bash
gh run list --branch main --limit 3 --json databaseId,conclusion,status,displayTitle,createdAt
```

- Üç tamamlanmış run'ın hepsi `success` → devam.
- Biri bile başarısız → **HARD STOP.**
  - **Yasak rasyonelizasyonlar:** *"lokal temiz"*, *"ilgisiz kırılma"*, *"önceki task'ın borcu"*, *"sadece şu workflow kırıldı"*.
  - Kırılma mevcut task'tan ise → **S2 Kırılma** bulgusu, FAIL verdict.
  - Kırılma önceki task'ın borcundan ise → **BLOCKED (`DEPENDENCY_MISMATCH`)** — *"önceki task yeşil bırakmadığı için bu task doğrulanamaz."*

**Kanıt:** 3 run ID + conclusion doğrulama bölümüne yazılır.

### Adım 0b — Repo memory drift check (HARD STOP)

**Neden:** Repo memory'nin güncelliği gözlemlenebilir değildir — hiçbir kapı kontrol etmezse sessizce geride kalır. Validator son kapıdır.

```bash
grep -nE "\bTXX\b" .claude/memory/MEMORY.md
```

- En az bir satır varsa → devam.
- Hiç satır yoksa → **HARD STOP / BLOCKED (`DEPENDENCY_MISMATCH`).**
  - Bulgu: *"Repo memory drift — TXX için satır yok. Yapım chat'i memory'i güncellemeden validate'e geçti (task bitiş kapısı madde 8 ihlali)."*
  - Düzeltme: yapım chat'ine dön → memory güncelle → `chore: memory — TXX yansıt` commit+push (aynı task PR'ına dahil edilebilir veya ayrı chore PR). Sonra validator yeniden başlar.
  - *"Sonra ekleriz"*, *"önemsiz"* yasak.

---

## Faz 1 — Bağımsız doğrulama

1. **Task tanımını oku** — `Docs/11_IMPLEMENTATION_PLAN.md`'den kabul kriterleri, test beklentisi, doğrulama kontrol listesi, doküman referansları.

2. **Referans dokümanları oku** — task tanımında belirtilen bölümler. Bunlar source of truth.

3. **Remote'u güncelle** — `git fetch origin`. Fetch yapılmazsa eski dal state'i incelenir.

4. **Dal kodunu incele** — hangi dosyalar değişmiş/oluşturulmuş, değişiklikler dokümanlarla uyumlu mu.

5. **Kabul kriterlerini tek tek doğrula** — her kriter için ilgili kodu bul ve oku, gerekli komutu **çalıştır**, çıktıyı kaydet, verdict ver:
   - `✓ Karşılandı` — kanıtla doğrulandı
   - `✗ Karşılanmadı` — eksik veya hatalı, detay yaz
   - `~ Kısmi` — ne eksik, detaylı açıkla
   - `? Doğrulanamadı` — kanıt üretilemedi (**FAIL değil**, kanıt eksikliği)

6. **Doğrulama kontrol listesini çalıştır** — plandaki maddeleri tek tek geç.

7. **Testleri çalıştır** — proje test komutları. Sonuçları kaydet.

8. **Build kontrolü** — tüm bileşenler temiz build veriyor mu?

   **8a. Task dalı CI kontrolü (zorunlu):**
   ```bash
   gh run list --branch task/TXX-* --limit 3 --json databaseId,conclusion,status
   ```
   En az bir run `success` olmalı. Hiç run yoksa veya son run `failure` ise → **bu bir bulgudur, sessizce geçilemez.**
   - Başarısız adım (Lint / Build / Unit / Integration / Contract / Migration / Paketleme) bulguda belirtilir.
   - *"Lokal makinemde geçiyor"* kabul edilemez — lokal temizlik CI'yi ikame etmez.
   - Task'ın hiç CI run'ı yoksa (dal push edilmemiş, PR açılmamış) → **BLOCKED** (yapım bitiş kapısı çiğnenmiş).

9. **Mini güvenlik kontrolü** — secret sızıntısı · auth/authorization etkisi · input validation etkisi · yeni dış bağımlılık.

10. **Doküman uyumu** — enum değerleri, alan adları, iş kuralları referans dokümanlarla birebir mi?

---

## Faz 2 — Verdict

11. **Genel verdict:**
    - **PASS** — tüm kriterler `✓` veya kabul edilebilir `~`, güvenlik temiz, testler geçiyor, CI yeşil
    - **FAIL** — en az bir `✗`, kritik güvenlik bulgusu veya kırık test/CI
    - **BLOCKED** — doğrulama yapılamıyor (kod eksik, dal yok, önceki task borcu)

12. **Bulguları sınıfla:**
    - `S1 Sapma` — tamamlandı ama dokümanla uyumsuz
    - `S2 Kırılma` — mevcut işlevselliği bozan değişiklik
    - `S3 Eksik` — kabul kriterinde tanımlı ama implement edilmemiş

    **Bloke-etmeyen gözlemler** ayrı listelenir (K-notu). Her K-notu ya bu PR'da kapatılır ya `Docs/DEFERRED_BACKLOG.md`'ye **forward edilir**. Üçüncü seçenek yoktur.

---

## Faz 3 — Karşılaştırma ve finalize

13. **Şimdi yapım raporunu oku** — kendi verdict'inle karşılaştır, uyuşmazlıkları belirt.

14. **Raporu finalize et** — doğrulama bölümünü doldur (durum, bulgu sayısı, düzeltme gerekli mi), kabul kriterleri tablosunu validator kanıtlarıyla güncelle.

15. **Status güncelle** (yalnız PASS) — `Docs/IMPLEMENTATION_STATUS.md`'de `✓ Tamamlandı`.
    **Kural:** Rapor finalize edilmeden status güncellenmiş sayılmaz.

16. **Rapor + status + memory commit ve push** — **merge'den önce**. Aksi hâlde squash merge bu değişiklikleri içermez.
    **Not:** Post-merge teyit bilgileri (merge sonrası run ID'leri) bu commit'e **giremez** — merge henüz olmadı. Onlar bir sonraki task dalında veya ayrı bir `chore:` PR'ında eklenir; **doğrudan ana dala push edilmez.**

17. **Merge** (yalnız PASS) — squash merge, mesaj `TXX: Task adı (#NN)`.
    **Dikkat:** Bazı CLI'lar `--subject` verildiğinde `(#NN)` sonekini otomatik eklemez. PR referansı olmayan squash commit'i ana dal CI'ının **guard job**'unu FAIL'ler. Ya numarayı elle ekle ya `--subject` bayrağını hiç kullanma.

18. **Post-merge CI watch (ZORUNLU çıkış kapısı)**
    - Squash merge ana dalda yeni commit yaratır → o commit için **tüm** workflow'lar tetiklenir.
    - `gh run list --branch main --limit 5 --json databaseId,status,workflowName,headSha` ile yeni SHA'ya ait run'ları topla.
    - Her biri için `gh run watch <ID> --exit-status` — **concluded** olana kadar izle.
    - Hepsi `success` → raporla. Biri başarısız → root cause (`gh run view <ID> --log-failed`) + düzeltme önerisi.
    - **Atlanması yasak.** Task/chore/docs PR ayrımı yok; workflow tipi ayrımı yok. Adım 18 yapılmadan bu skill tamamlanmış sayılmaz.

---

## Çıktı formatı

```
## Doğrulama Sonucu — TXX [Task Adı]
**Tarih:** YYYY-AA-GG   **Branch:** task/TXX-...   **Commit:** <hash>

### Verdict: ✓ PASS / ✗ FAIL / ⛔ BLOCKED

### Başlangıç Kapıları
| Kapı | Sonuç | Kanıt |
|---|---|---|
| Working tree | temiz / N dosya → karar | git status |
| Ana dal CI (son 3) | ✓ | run ID'leri + conclusion |
| Memory drift | ✓ | grep sonucu |

### Kabul Kriterleri
| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | ... | ✓/✗/~/? | komut + çıktı |

### Doğrulama Kontrol Listesi
- [x] / [ ] madde

### Test Sonuçları
| Tür | Sonuç | Komut | Çıktı |
|---|---|---|---|

### Güvenlik Kontrolü
- Secret sızıntısı: Temiz / Bulgu
- Auth etkisi: Temiz / Bulgu
- Input validation: Temiz / Bulgu
- Yeni bağımlılık: Yok / [liste]

### Bulgular
| # | Seviye | Açıklama | Etkilenen dosya |
|---|---|---|---|
| 1 | S1/S2/S3 | ... | ... |

### Bloke-etmeyen Notlar (K-notları)
| # | Not | Karar |
|---|---|---|
| K1 | ... | bu PR'da kapatıldı / DEFERRED_BACKLOG'a forward |

### Yapım Raporu Karşılaştırması
- Uyum: Tam uyumlu / N uyuşmazlık
- [detaylar]

### Merge ve Post-merge
- Squash: `<hash>` (#NN)
- Post-merge run'lar: <workflow adı + ID + conclusion> ×N
```

---

## FAIL durumunda

- Bulguları proje sahibine sun.
- **Dal merge edilmez.**
- Düzeltme için yeni yapım chat'i açılır; düzeltme sonrası **yeni** doğrulama chat'i.

## "Doğrulanamadı" durumunda

- `?` alan kriterler için ek kanıt üretme yöntemi öner veya doğrulama yönteminin revize edilmesi gerektiğini belirt.
- Bu FAIL sayılmaz ama PASS için çözülmesi gerekir.

# Shopfolio — Status Changelog

> [`IMPLEMENTATION_STATUS.md`](IMPLEMENTATION_STATUS.md)'nin **tarihsel arşivi**.
>
> **Neden ayrı dosya:** Güncel snapshot kısa kalmalı, tarihsel detay büyüyebilir (00 §G.2).
> Status dosyasının "son güncelleme" bloğu bir sonraki güncellemeye yer açarken **eski giriş buraya taşınır** — status dosyasında birikmez.
>
> **Sıra:** En yeni giriş en üstte.

---

## YYYY-AA-GG — <özet başlık>

`<Taşınan status girişi buraya olduğu gibi yazılır.>`

---

## 2026-11-04 — T103 yapım bitti

2026-11-04 — T103 (Başlangıç listeleri, firma ve yasal metin sürümü) yapım bitti: altı tablo tek göçle, tekil satır dizinleri, kontrol kısıtları, kalıcı satır bekçisi, göç geçmişi tablosu kapatma; Unit 217/217 + Integration 119/119 (PostgreSQL 18); D-37 (a), D-46 (a)(b), D-48 (b) çözüldü. Önceki giriş (T102 doğrulama tur 1) `STATUS_CHANGELOG.md`'ye taşındı.

---

## 2026-11-03 — T102 doğrulama tur 1 PASS

2026-11-03 — T102 (Enum tanımları ve değer dönüşümü) bağımsız doğrulama tur 1: ✓ PASS (head `8277427`; kriter 1–6 ve KK1–KK6 ✓; Unit 199/199, Integration 94/94 PostgreSQL 18; dal CI `41712894839` yeşil; bulgu 0, K1–K2 → D-48; D-47 hedefleri düzeltildi). PR #359 merge ve post-merge CI yardımcıda.

---

## 2026-11-03 — T102 yapım bitti

2026-11-03 — T102 (Enum tanımları ve değer dönüşümü) yapım bitti: 42 enum, metin dönüşümü, izinli değer kısıtı, kilit dosyaları; Unit 199/199, Integration 94/94 (PostgreSQL 18); doğrulama bekliyor.

---

## 2026-11-03 — T101 doğrulama tur 1 PASS

2026-11-03 — T101 (Veri modeli temeli) bağımsız doğrulama tur 1: ✓ PASS (head `f7b33c7`; kriter 1–8 ve KK1–KK8 ✓; Unit 128/128, Integration 81/81 PostgreSQL 18.6; dal CI `41706961892` yeşil; bulgu 0, K1–K3 → D-46). PR #358 merge ve post-merge CI yardımcıda.

---

## 2026-11-03 — T101 yapım bitti

2026-11-03 — T101 (Veri modeli temeli) yapım bitti, PR #358, doğrulama bekliyor.

---

## 2026-11-03 — F0 faz kapısı PASS

2026-11-03 — F0 (Proje iskeleti) faz kapısı: ✓ PASS (`GATE_CHECK_F0.md`; ana dal `93ba91f`; testler, üç imaj, yerel referans kurulumda taze ortam ve canlılık 200, Lint kırmızı→yeşil; `11 §7.1`'in F0'a bağlı 66 satırı dolduruldu; backlog hijyeni: D-07, D-11, D-18, D-23 ✅, yeni D-43, D-44). Sırada: gate PR'ının merge'ü ve `phase/F0-pass` etiketi (yardımcı), sonra F1. Önceki giriş `STATUS_CHANGELOG.md`'ye taşındı (T09 doğrulama tur 2 PASS).

---

## 2026-11-03 — T09 doğrulama tur 2 PASS

2026-11-03 — T09 (İlk iniş provası: yerel referans kurulumda canlılık) bağımsız doğrulama tur 2: ✓ PASS (`303c074`; kapsam tam; kriter 1–7 ve KK1–KK7 ✓ — kurulum validator tarafından reçeteyle sıfırdan kuruldu; dal CI run 41692602124 yeşil; tur 1 bulgu 1 ve K1–K5 kapandı; K-notları D-42). Sırada: merge (yardımcı), sonra F0 gate. Önceki giriş `STATUS_CHANGELOG.md`'ye taşındı (T09 doğrulama tur 1 FAIL).

---

## 2026-11-03 — T09 doğrulama tur 1 FAIL

2026-11-03 — T09 (İlk iniş provası: yerel referans kurulumda canlılık) bağımsız doğrulama tur 1: ✗ FAIL (`a4632b1`; kapsam tam; kriter 1–7 ve KK1–KK7 ✓ — kurulum validator tarafından reçeteyle sıfırdan kuruldu; dal CI run 41687547294 yeşil; bulgu 1 S1 DÜŞÜK: `E2E_CMD=NOT_APPLICABLE` gerekçesi "uçtan uca faz yok" `11` F6 (Uçtan uca doğrulama) ve `05 §8.3.11` ile çelişir, bağlama izlenmiyor; K1–K5 düzeltme turunda). Sırada: T09 düzeltme 1. Önceki giriş `STATUS_CHANGELOG.md`'ye taşındı (T09 yapım bitti).

---

## 2026-11-03 — T09 yapım bitti

2026-11-03 — T09 (İlk iniş provası: yerel referans kurulumda canlılık) yapım bitti, ⏳ doğrulama bekliyor: yerel referans kurulum (dind sunucu, MinIO, Caddy iç sertifikası, sahte sırlar) iki kez sıfırdan koştu; canlılık 200, sır ve kullanıcı doğrulamaları, `DOCKER-USER` çıkış denemesi, IPv4 ve IPv6 istemci adresi, yedek ve kurtarma provası, imaj kimlikleri `DEPLOY_RUNBOOK` §K'ya yazıldı; `SETUP_COMPLETE=true`, D-01 ✅, gerçek sunucu artıkları D-40/T603. Önceki giriş `STATUS_CHANGELOG.md`'ye taşındı (T09 plan düzeltmesi).

---

## 2026-11-03 — T09 plan düzeltmesi

2026-11-03 — T09 plan düzeltmesi (PLAN_CORRECTION_REQUIRED; PR #353; proje sahibinin kararı): T09 "İlk iniş provası: yerel referans kurulumda canlılık" oldu (Docker içinde ayrıcalıklı Linux konteyneri, MinIO, sahte sırlar) ve ⬚ Bekliyor'a döndü; gerçek sunucuya ilk iniş yeni F6 task'ı T603'e taşındı (⬚); D-01'in gerçek sunucu artıkları `DEFERRED_BACKLOG.md` D-40'ta. Toplam 215 task. T01–T08 ve T10 ✓. Önceki giriş `STATUS_CHANGELOG.md`'ye taşındı (T09 BLOCKED OWNER_INPUT).

---

## 2026-11-03 — T09 BLOCKED (OWNER_INPUT)

2026-11-03 — T09 (İlk yayın ve referans kuruluma ilk iniş) ⛔ BLOCKED (OWNER_INPUT): kabul kriteri 3–8 referans sunucuyu (Linux, iptables, IPv4 + IPv6), firmanın nesne depolama kovası ve anahtarlarını, alan adını ve kurulum sırlarını ister; bunlar proje sahibinden beklenir. T09 bu girdiler gelince yapılır; F0 gate'i T09'dan sonra (`00`: gate PASS olmadan sonraki faza geçilmez). T01–T08 ve T10 ✓.

---

## 2026-11-03 — T07 doğrulama tur 4 PASS

2026-11-03 — T07 (Yedek düzeni) bağımsız doğrulama tur 4 (düzeltme 2 sonrası, `main` ile birleşik hâl): ✓ PASS (`aa1cfe6`; kapsam tam; 7/7 kabul kriteri, KK1–KK7 ✓; dal CI run 41670381320 yeşil; tur 3'ün bulgusu (`VAL-F016`, prova son anı) kapandı — yerel Docker'da gerçek imajla prova kurtarmanın bitmesini bekledi, `last-moment` birincil sunucudaki son işlemle aynı, günlükler `--repo=2` ile dış depodan; bulgu 0; K-notları: `core.hooksPath` yine mutlak yol (düzeltildi), D-39 (d) kapandı). Merge yardımcıda (PR #351). Önceki giriş `STATUS_CHANGELOG.md`'ye taşındı (T07 doğrulama tur 3 FAIL).

---

## 2026-11-03 — T07 doğrulama tur 3 FAIL

2026-11-03 — T07 (Yedek düzeni) bağımsız doğrulama tur 3 (tur 2 PASS'ten sonra `main` ile birleşik hâl): ✗ FAIL (`c929163`; kapsam tam; kriter 1–6 ✓, 7 ✗; dal CI run 41665382289 yeşil; bulgu 1 S1 ORTA `VAL-F016`: `database-restore-drill.sh` sıcak beklemede açılan kopyada son anı günlük uygulaması bitmeden okur — kayıt son tam yedeğin anını ve yalnız temel yedeğin indirme süresini taşır; K-notları: `core.hooksPath` mutlak yol (düzeltildi), PR açıklamasındaki test sayıları; iki doğrulayıcıyla, ilki bölündü). Düzeltme turu gerekli; merge yok (PR #351). Önceki giriş `STATUS_CHANGELOG.md`'ye taşındı (T07 doğrulama tur 2 PASS).

---

## 2026-11-03 — T07 doğrulama tur 2 PASS

2026-11-03 — T07 (Yedek düzeni) bağımsız doğrulama tur 2: ✓ PASS (`501c24b`; kapsam tam; 7/7 kabul kriteri, KK1–KK7; dal CI run 41660924636 yeşil; yerel Docker'da gerçek iki imajla dosya yedeğinin dört adımı `success`, prova üç kayıt alanını yazdı; tur 1'in dört bulgusu ve K1–K7 kapandı; bulgu 0; K-notu D-39 (f); iki doğrulayıcıyla, ilki bölündü); merge yardımcıda (PR #351). F0 sırada sonraki: T10. Önceki giriş `STATUS_CHANGELOG.md`'ye taşındı (T07 yapım bitti).

---

## 2026-11-03 — T10 doğrulama tur 2 PASS

2026-11-03 — T10 (Veritabanı kullanıcıları) bağımsız doğrulama tur 2: ✓ PASS (`76979dc`; kapsam tam; 8/8 kabul kriteri, KK1–KK8; dal CI run 41661056056 yeşil; gerçek veritabanı imajında `log_statement=all` ile ilk kurulum — günlükte açık parola yok, yalnız üç SCRAM özeti; tur 1'in bulgu 1'i kapandı; bulgu 0; K-notları D-38); merge yardımcıda (PR #350). F0'da kalan: T07, ardından T09 (T09 T07'ye bağlı). Önceki giriş `STATUS_CHANGELOG.md`'ye taşındı (T10 yapım bitti).

---

## 2026-11-03 — T07 yapım bitti, doğrulama bekliyor

2026-11-03 — T07 (Yedek düzeni) yapım bitti, PR açıldı, bağımsız doğrulama bekliyor: gecelik tam yedek (pgBackRest, iki depo) ve dosya yedeği (restic, dört adım) betikleri, `archive_timeout` 50 s, kova kuralı ve anahtar politikası, kurtarma provası runbook adımı (`DEPLOY_RUNBOOK` §J); betik testleri ve 15 eşitlik denetimi yerelde yeşil, imaj çalıştırılarak sınandı; D-28, D-30 (b), D-34 (a) kapandı, D-39 açıldı. Önceki giriş `STATUS_CHANGELOG.md`'ye taşındı (T08 doğrulama tur 4 PASS).

---

## 2026-11-03 — T10 yapım bitti

2026-11-03 — T10 (Veritabanı kullanıcıları) yapım bitti, doğrulama bekliyor: kurucu betik (`deploy/images/database/initdb/10-bootstrap.sh`; SQL'ler ve `pg_hba`/`pg_ident` şablonları `bootstrap/`'ta) dört kullanıcıyı, veritabanını (UTF-8, ICU tr-TR), yetkileri, `pg_trgm`'i ve üç rol ayarını kurar; yedek durum kullanıcısı `peer` + `pg_ident`, kurucu ağdan `reject`, `trust` yok; yetki matrisi ve kullanıcı testleri gerçek PostgreSQL 18'de betiğin SQL'leriyle koşar; CI Paketleme adımı ilk kurulumu sınar; D-01 ve D-31'e not. Önceki giriş `STATUS_CHANGELOG.md`'ye taşındı (T08 doğrulama tur 4 PASS).

---

## 2026-11-03 — T08 doğrulama tur 4 PASS

2026-11-03 — T08 (Sır düzeni) bağımsız doğrulama tur 4: ✓ PASS (`363dd4d`; kapsam tam; 6/6 kabul kriteri, KK1–KK6; dal CI run 41646506249 yeşil; yerel Docker'da 15 sır dosyası × 3 kullanıcı ve dalın yayını uygulama imajında — dosya ortamı yener, geliştirme yolu ortamdan, ikisi yoksa açılmaz; tur 3'ün iki bulgusu ve K1–K5 kapandı; bulgu 0; K-notları D-36); merge yardımcıda (PR #349). F0 sırada sonraki: T07. Önceki giriş `STATUS_CHANGELOG.md`'ye taşındı (T05 doğrulama tur 2 PASS).

---

## 2026-11-03 — T05 doğrulama tur 2 PASS

2026-11-03 — T05 (Compose geliştirme ortamı) bağımsız doğrulama tur 2: ✓ PASS (9/9 kabul kriteri, KK1–KK9; dal CI run 41627237288 yeşil; yerel Docker'da üç imaj dal Dockerfile'larından derlendi — yalıtım, HTTP→HTTPS + bir yıllık HSTS, Mailpit, `DOCKER-USER` kuralı atılık konteynerde sınandı; plan düzeltmesi `11`/`12`/D-31'e tutarlı; bulgu 0; K-notları D-34; iki doğrulayıcıyla, ilki bölündü); merge yardımcıda (PR #348). F0 sırada sonraki: T08. Önceki giriş `STATUS_CHANGELOG.md`'ye taşındı (T05 yapım bitti).

---

## 2026-11-03 — T05 yapım bitti, doğrulama bekliyor

2026-11-03 — T05 (Compose geliştirme ortamı) yapım bitti, PR #348 açık, doğrulama bekliyor (⏳); `deploy/` altında Compose, geliştirme bindirmesi, Caddyfile, sürüm dosyası ve güvenlik duvarı betikleri; yerelde Docker kapalıydı, `docker compose config` geçerli, daemon gereken sınamalar (KK2, KK4, KK8, KK9) D-31 ile T09'a bağlandı; D-01 K-987 doğrulamaları yazıldı (`SETUP_COMPLETE` false), D-29 (c) kapandı, D-31..D-33 açıldı; önceki giriş `STATUS_CHANGELOG.md`'ye taşındı (T06 doğrulama tur 2 PASS).

---

## 2026-11-03 — T06 doğrulama tur 2 PASS

2026-11-03 — T06 (Üç imaj ve kayıt defteri) bağımsız doğrulama tur 2: ✓ PASS (8/8 kabul kriteri, KK1–KK8; dal CI run 41594485717 yeşil — Paketleme 7/7: üç imaj, yanlış restic özeti `did NOT match` ile düşer, kullanıcılar kök değil, canlılık 200, `track_commit_timestamp` on; taban imaj özetleri ve restic özeti/lisansı kayıt defterlerinden doğrulandı; yerelde Docker kapalıydı; K-notları D-29, D-30); merge yardımcıda (PR #347). F0 sırada sonraki: T05.

---

## 2026-11-03 — T06 yapım bitti, doğrulama bekliyor

2026-11-03 — T06 (Üç imaj ve kayıt defteri) yapım bitti, PR #347 açık, doğrulama bekliyor (⏳); D-01 kalemleri (PACKAGE_BUILD/PUBLISH, doğrulanacaklar) ve D-19 kararı raporda; önceki giriş `STATUS_CHANGELOG.md`'ye taşındı (T01 doğrulama tur 4 PASS).

---

## 2026-11-03 — T01 doğrulama tur 4 PASS

2026-11-03 — T01 (Canlılık ucu) bağımsız doğrulama tur 4: ✓ PASS (6/6 kabul kriteri; 62/62 test — Lint 9, Unit 24, Integration 29 PostgreSQL 18.6'da; Kestrel'de dondurulmuş veritabanında 503 ayarın süresinde; dal CI yeşil; K-notları D-25, D-26); merge yardımcıda (PR #338). F0 sırada sonraki: T06.

---

## 2026-11-03 — T01 yapım bitti, doğrulama bekliyor

**Son güncelleme:** 2026-11-03 — T01 (Canlılık ucu) yapım bitti, PR açık, doğrulama bekliyor (⏳); önceki: T03 (Test altyapısı) bağımsız doğrulama tur 3: ✓ PASS (9/9 kabul kriteri; 36/36 test PostgreSQL 18.6'da, kilit dosyaları koşu sonrası değişmiyor; K-notu D-17'ye eklendi); merge yardımcıda (PR #337). F0 sırada sonraki: T01.

---

## 2026-11-03 — T04 doğrulama tur 2 PASS

T04 (CI hattı ve eşitlik denetimleri) bağımsız doğrulama tur 2: ✓ PASS (9/9 kabul kriteri; dal CI'ı run 41548360499 yeşil — Lint 9, Unit 15, Integration 14 PostgreSQL 18.6 hizmetinde, dry-run atlama satırları; sondajlar geçici kopyada; tur 1'in iki bulgusu kapandı; K-notları D-23, D-24); merge yardımcıda (PR #339).

---

## 2026-11-03 — T03 doğrulama tur 3 PASS

**Son güncelleme:** 2026-11-03 — T03 (Test altyapısı) bağımsız doğrulama tur 3: ✓ PASS (9/9 kabul kriteri; 36/36 test PostgreSQL 18.6'da, kilit dosyaları koşu sonrası değişmiyor; K-notu D-17'ye eklendi); merge yardımcıda (PR #337). F0 sırada sonraki: T01.

---

## 2026-11-03 — T03 yapım bitti, doğrulama bekliyor

2026-11-03 — T03 (Test altyapısı) yapım bitti, ⏳ doğrulama bekliyor (PR #337). T02 ✓ (PR #335).

---

## 2026-11-02 — Implementation dönemi açıldı

Implementation dönemi açıldı: tracker `11`'in faz ve task listesiyle dolduruldu (7 faz, 214 task, hepsi ⬚); sıradaki iş `/task T02` (önce D-01 kapısı, `Docs/PRODUCT_DISCOVERY_STATUS.md` §25).

---

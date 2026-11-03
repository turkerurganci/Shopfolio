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

## 2026-11-03 — T10 yapım bitti

2026-11-03 — T10 (Veritabanı kullanıcıları) yapım bitti, doğrulama bekliyor: kurucu betik (`deploy/images/database/initdb/10-bootstrap.sh`; SQL'ler ve `pg_hba`/`pg_ident` şablonları `bootstrap/`'ta) dört kullanıcıyı, veritabanını (UTF-8, ICU tr-TR), yetkileri, `pg_trgm`'i ve üç rol ayarını kurar; yedek durum kullanıcısı `peer` + `pg_ident`, kurucu ağdan `reject`, `trust` yok; yetki matrisi ve kullanıcı testleri gerçek PostgreSQL 18'de betiğin SQL'leriyle koşar; CI Paketleme adımı ilk kurulumu sınar; D-01 ve D-31'e not. Önceki giriş `STATUS_CHANGELOG.md`'ye taşındı (T08 doğrulama tur 4 PASS).

---

## 2026-11-03 — T08 doğrulama tur 4 PASS

2026-11-03 — T08 (Sır düzeni) bağımsız doğrulama tur 4: ✓ PASS (`363dd4d`; kapsam tam; 6/6 kabul kriteri, KK1–KK6; dal CI run 41646506249 yeşil; yerel Docker'da 15 sır dosyası × 3 kullanıcı ve dalın yayını uygulama imajında — dosya ortamı yener, geliştirme yolu ortamdan, ikisi yoksa açılmaz; tur 3'ün iki bulgusu ve K1–K5 kapandı; bulgu 0; K-notları D-36); merge yardımcıda (PR #349). F0 sırada sonraki: T07.

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

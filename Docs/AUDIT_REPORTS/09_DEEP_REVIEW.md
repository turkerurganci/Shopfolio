# Deep Review Raporu — 09 Coding Guidelines

**Tarih:** 2026-10-30 · **Hedef:** `Docs/09_CODING_GUIDELINES.md` v0.17 (bulgular v0.17'de üretildi, v0.18'de işlendi; taban `main` = c2e44e6) · **Bağlam:** `PRODUCT_DISCOVERY_STATUS.md` §20.1 ve K-1302…K-1371; üst dokümanlar `02`, `04`, `05`, `06`, `07`, `08`; `AUDIT_REPORTS/09_AUDIT.md` · **Odak:** Tam analiz, betikle çıkarılmış dar kapsamlı paketlerle (`09` yaklaşık 425 KB; bütün okunmaz). Kapsam ve tutarlılık katmanı audit'in geniş koşumundan sonra dar tutuldu (skill, kural 7).

### Koşum biçimi (K-431, K-1232; `deep-review` ve `audit` skill'leri, "Koşum biçimi")

Doküman yazan bağlamda koşmadı; iki parçaya bölündü. **D-a:** paketleyici betik (`scripts/doc-scans/09_dr_paketle.py`) mercek paketlerini üretti, mercekler okudu, doğrulayıcılar hükme bağladı; her bulgunun ESKİ → YENİ şartnamesi sonuç dosyasında tutuldu, dokümana yazılmadı. **D-b:** seviyesi yükselen altı bulgu yeniden doğrulandı, şartnameler tek betikle uygulandı, eşzamanlı güncellemeler yapıldı, karar kaydına işlendi.

- **Paketleyici betik.** `09`'un §3–§25 metni yaklaşık 370 KB'tır ve paket sınırı 90 KB'tır; bu yüzden teknik derinlik katmanı üç yerine **beş dilime** bölündü.
- **On iki mercek.** L12 (dar tutarlılık) · L3-1…L3-5 (teknik derinlik) · L4a ve L4b (güvenlik) · L5 (hata modu) · L6 (veri akışı) · L7 (ölçeklenebilirlik) · L8 (bağımlılık). Mercekler `[ozet]` Sonnet / low (`INSTRUCTIONS §9` (1)).
- **Ham bulgu: 105** (Yüksek 1 · Orta 55 · Düşük 49).
- **Kademeli doğrulama — 20 doğrulayıcı, hepsi `[dogrulama]` Opus / high.** dv1 yüksek (1 bulgu) · dv2…dv15 orta (14 doğrulayıcı, 2–4 ana bulgu) · dv16…dv20 düşük (5 doğrulayıcı × 8 bulgu). Aynı satırı gösteren dokuz düşük bulgu ayrı sayılmadı, yinelendiği orta bulgunun grubunda hükme bağlandı.
- **Yeniden doğrulama — 5 doğrulayıcı, hepsi `[dogrulama]` Opus / high.** Doğrulayıcı seviyeyi yükselttiğinde bulgu yeni kademede yeniden doğrulanır (`INSTRUCTIONS §9` (1)): Yüksek'e çıkan dört bulgu (B-L3-1-7, B-L3-3-4, B-L3-4-5, B-L6-3) birer doğrulayıcıyla, Orta'ya çıkan iki bulgu (B-L3-3-7, B-L6-5) tek doğrulayıcıyla. Toplam doğrulayıcı: **25**.
- **Hüküm:** 105 bulgudan 45 doğru, 36 kısmen, 24 çürük; uygulanabilir 81.
- **Hiçbir mercek ya da doğrulayıcı `Docs/` dosyasını değiştirmedi** (`git status` her aşamada temiz).

### Sapmalar

1. **Kademe aşımı.** Dört doğrulayıcı grubunda 2–4'lü orta kademe aşıldı: 5, 5, 7 ve 5 bulgu. Gerekçe: aynı yeri gösteren bulgular ayrı doğrulayıcıya bölünürse aynı satır için çelişen iki hüküm çıkardı; yinelenen bulgular tek hükümle birlikte ele alındı.
2. **Yinelenen dokuz düşük bulgu orta grubunda hükme bağlandı** (B-L3-2-10, B-L3-2-9, B-L3-2-6, B-L3-4-10, B-L5-6, B-L4b-6, B-L4b-8, B-L6-6, B-L8-8); ayrı doğrulayıcı sayısına eklenmedi.
3. **Katman 3 beş dilime bölündü** (üç değil): paket sınırı 90 KB, §3–§25 metni yaklaşık 370 KB.
4. **Yeniden doğrulama D-a'da yapılmadı.** D-a toplam 20 doğrulayıcıyla sınırlıydı; seviye yükselmeleri D-b'ye bırakıldı ve D-b'de beş doğrulayıcıyla kapandı. Altı bulgu da yeni kademede ayakta kaldı; altısının da şartnamesi yeniden doğrulayıcının metniyle değişti (aşağıda).
5. **Çakışan şartnameler.** Üç yerde iki bulgunun ESKİ metni aynı yeri gösterdi (B-L3-1-8 ile B-L3-2-1: iki çift; B-L4a-2 ile B-L4b-5: bir çift). Daha kapsamlı olan uygulandı, öteki çift düşürüldü (B-L4a-2 bulgusu B-L4b-5'in birleştirilmiş çiftiyle karşılandı).

### Özet Skor Tablosu

Skor: ✓ doğrulanmış bulgu yok ya da yalnız Düşük · ⚠ doğrulanmış Orta · ✗ doğrulanmış Yüksek.

| # | Katman | Skor | Doğrulanan (Y/O/D) | Kritik Bulgu |
|---|---|---|---|---|
| 1-2 | Kapsam + tutarlılık (dar: audit sonrası) | ⚠ | 0/1/6 | — |
| 3 | Teknik derinlik | ✗ | 3/18/27 | B-L3-1-7, B-L3-3-4, B-L3-4-5 |
| 4 | Güvenlik (OWASP) | ⚠ | 0/4/7 | — |
| 5 | Hata modu | ✓ | 0/0/3 | — |
| 6 | Veri akışı | ✗ | 2/2/1 | B-L6-1, B-L6-3 |
| 7 | Ölçeklenebilirlik | ✓ | 0/0/1 | — |
| 8 | Bağımlılık riski | ⚠ | 0/3/3 | — |
| | **Toplam** | | **5/28/48** (81 bulgu) | |

**Genel:** ⚠ İyileştirme gerekli — Yüksek bulguların hepsi bu adımda düzeltildi (şartnameler uygulandı); ürün kuralı koyan ya da proje sahibine sorulması gereken (çok kritik) madde çıkmadı.
**Güçlü yönler:** (1) Çürütülen 24 bulgunun çoğu kılavuzun kendi ya da üst belgenin kararını doğruladı — sorulan şeyin cevabı yazılıydı (kayan oturum süresi bilinçli ürün kararı, sabit zamanlı imza karşılaştırması zaten yazılı, görüntü işleme soyutlaması yazılı, süre sonu olmayışı bilinçli). (2) Yüksek bulgular yeni yazılacak kural değil, var olan üst belge güvencesinin kılavuza taşınmasıdır (`05 §3.4.2`, `§7.5.1`, `§6.4.8`). (3) Audit'in 69 çiftlik düzeltmesi yeni bir tutarsızlık yaratmadı: kapsam ve tutarlılık katmanında yalnız yedi bulgu çıktı ve biri Orta'dır.

### Katman Detayları

#### Katman 1-2 — Kapsam + tutarlılık (dar: audit sonrası)

| Bulgu | Hüküm | Seviye (mercek → son) | Karar | Yer | Çift | Kayıt |
|---|---|---|---|---|---|---|
| B-L12-2 | doğru | Orta → Orta | sıradan | §4.x geliştirme ortamı (satır 435) ↔ §11.1 madde 1 (satır 1284, 1319, 1334) | 2 | K-1376 |
| B-L12-1 | doğru | Orta → Düşük | sıradan | §11.1 SES katmanı, test maddesi (paket satır 1336) ↔ madde 4 (satır 1334) | 1 | K-1376 |
| B-L12-3 | doğru | Düşük → Düşük | sıradan | §3.5 / §10.7 / §15 profil adı (satır 1485) ↔ madde 2 (satır 1068) | 1 | K-1376 |
| B-L12-4 | doğru | Düşük → Düşük | sıradan | §24/kontrol tablosu, kalıcı değer satırı (satır 2253) ↔ §12 (satır 1378) | 1 | K-1376 |
| B-L12-5 | kısmen | Düşük → Düşük | sıradan | §3.5 madde 2 (satır 308) | 1 | K-1376 |
| B-L12-6 | doğru | Düşük → Düşük | sıradan | §15.x yükleme başlatma (satır 1722) ↔ yasak listesi (satır 2172) | 3 | K-1376 |
| B-L12-7 | kısmen | Düşük → Düşük | sıradan | §11.x çerez yazımı (satır 1577) ve §3 kütüphane girişi (satır 339) | 2 | K-1376 |

#### Katman 3 — Teknik derinlik

| Bulgu | Hüküm | Seviye (mercek → son) | Karar | Yer | Çift | Kayıt |
|---|---|---|---|---|---|---|
| B-L3-1-7 | doğru | Orta → Yüksek | ⚠ | §9.1 madde 3 (satır 689) | 2 | K-1377 |
| B-L3-3-4 | kısmen | Orta → Yüksek | ⚠ | §13.2 madde 3, satır 1482 (ve `reapply-deletions`/`rollback-check`/`read-logs` seçenekleri) | 1 | K-1372 |
| B-L3-4-5 | doğru | Orta → Yüksek | sıradan | §15.6 madde 2 (a), satır 1684 ve kod örneği satır 1692 | 2 | K-1380 |
| B-L3-1-1 | doğru | Orta → Orta | sıradan | §3.4 madde 1 ve §5.1 örnekleri (satır 268-277) | 1 | K-1377 |
| B-L3-1-2 | doğru | Orta → Orta | sıradan | §3.1 madde 4 (satır 212) ile §6 madde 3, §7.1, §8.1, §9.1 örnekleri | 1 | K-1377 |
| B-L3-1-4 | doğru | Orta → Orta | sıradan | §6 madde 1, madde 7; §5.2 madde 2; §9.1 madde 8; §10 testleri | 2 | K-1377 |
| B-L3-1-8 | kısmen | Orta → Orta | sıradan | §9.1 madde 4 örneği ve madde 5 (satır 693-704, 704) | 4 | K-1377 |
| B-L3-2-1 | doğru | Orta → Orta | sıradan | §10.2 madde 5, kod bloğu, satır 875-879 | 3 | K-1378 |
| B-L3-2-2 | doğru | Orta → Orta | sıradan | §9.4 madde 3, eşzamanlı çakışma, satır 777 | 1 | K-1378 |
| B-L3-3-1 | doğru | Orta → Orta | sıradan | §11.1 madde 2, satır 1285 (ve kod örneği satır 1299-1305) | 2 | K-1379 |
| B-L3-3-3 | kısmen | Orta → Orta | sıradan | §11.3 madde 2-3, satır 1343-1344 | 1 | K-1379 |
| B-L3-3-5 | doğru | Orta → Orta | sıradan | §12 madde 5, satır 1382 | 1 | K-1379 |
| B-L3-3-7 | kısmen | Düşük → Orta | sıradan | §10.10 madde 7, satır 1260 ve örnek 1262-1270 | 2 | K-1379 |
| B-L3-4-2 | kısmen | Orta → Orta | sıradan | §15.1 madde 2, satır 1573 | 1 | K-1380 |
| B-L3-4-3 | kısmen | Orta → Orta | sıradan | §15.1 madde 6, satır 1577 | 1 | K-1380 |
| B-L3-4-4 | doğru | Orta → Orta | sıradan | §15.4 madde 4, satır 1638 | 1 | K-1380 |
| B-L3-4-9 | doğru | Orta → Orta | sıradan | §17 madde 5, satır 1849 ve JSON örneği | 2 | K-1380 |
| B-L3-5-1 | doğru | Orta → Orta | sıradan | §18.1 madde 4 kod örneği, satır 1897-1898 | 2 | K-1381 |
| B-L3-5-2 | doğru | Orta → Orta | sıradan | §18.1 madde 2 ve madde 4 kod örneği, satır 1884-1886, 1893-1899 | 1 | K-1381 |
| B-L3-5-3 | kısmen | Orta → Orta | sıradan | §18.2 madde 1 alan tablosu, satır 1915-1927 | 2 | K-1381 |
| B-L3-5-7 | doğru | Orta → Orta | sıradan | §19.2 madde 6 "Doğru" kod örneği, satır 2010 | 1 | K-1381 |
| B-L3-1-10 | kısmen | Düşük → Düşük | sıradan | §7.2 madde 3 örneği (satır 609-613) ve §5.2 madde 5 örneği (satır 498) | 1 | K-1377 |
| B-L3-1-11 | kısmen | Düşük → Düşük | sıradan | §3.5 madde 1 örneği (satır 303) ve açıklama (298) | 2 | K-1377 |
| B-L3-1-3 | doğru | Orta → Düşük | sıradan | §5.1 madde 7 (satır 474), §5.3 madde 3, §4.1 madde 3, §3.1 madde 4 | 1 | K-1377 |
| B-L3-1-5 | kısmen | Orta → Düşük | sıradan | §6 madde 2 ve madde 7 (satır 522, 558) ile §9.1 madde 3 | 1 | K-1377 |
| B-L3-1-6 | kısmen | Orta → Düşük | sıradan | §8.1 madde 2-4 (satır 628, 638, 639) | 2 | K-1377 |
| B-L3-2-3 | doğru | Orta → Düşük | sıradan | §9.2 madde 1, satır 711 | 1 | K-1378 |
| B-L3-2-4 | kısmen | Orta → Düşük | sıradan | §10.1 madde 3-4, satır 828-829 | 1 | K-1378 |
| B-L3-2-5 | doğru | Düşük → Düşük | sıradan | §10.5 madde 3, satır 992 | 2 | K-1378 |
| B-L3-2-7 | doğru | Düşük → Düşük | sıradan | §10.7 madde 6, kod bloğu, satır 1111 | 4 | K-1378 |
| B-L3-2-8 | doğru | Düşük → Düşük | sıradan | §9.3 madde 5, satır 769 | 2 | K-1378 |
| B-L3-2-9 | kısmen | Düşük → Düşük | sıradan | §10.8 madde 7, yetki matrisi, satır 1175 | 1 | K-1378 |
| B-L3-3-2 | doğru | Orta → Düşük | sıradan | §10.9 madde 2, satır 1191 | 1 | K-1379 |
| B-L3-3-6 | kısmen | Orta → Düşük | sıradan | §10.10 madde 1-2, satır 1223 ve 1227 | 2 | K-1379 |
| B-L3-3-8 | kısmen | Düşük → Düşük | sıradan | §14.2 madde 1, satır 1544 | 1 | K-1379 |
| B-L3-3-9 | kısmen | Düşük → Düşük | sıradan | §13.3 madde 2, satır 1493 | 2 | K-1379 |
| B-L3-4-1 | kısmen | Orta → Düşük | sıradan | §15.4 madde 3, satır 1637 | 1 | K-1380 |
| B-L3-4-10 | doğru | Düşük → Düşük | sıradan | §15.1 madde 8, satır 1579 | 1 | K-1380 |
| B-L3-4-11 | doğru | Düşük → Düşük | sıradan | §15.3 madde 3, satır 1607 | 1 | K-1380 |
| B-L3-4-12 | doğru | Düşük → Düşük | sıradan | §17 madde 3, satır 1847 | 1 | K-1380 |
| B-L3-4-6 | kısmen | Orta → Düşük | sıradan | §15.6 madde 2 (e), satır 1687 ve kod satırı 1700 | 3 | K-1380 |
| B-L3-4-7 | doğru | Orta → Düşük | sıradan | §16.2 madde 3, satır 1785 ve kod örneği satır 1789 | 2 | K-1380 |
| B-L3-4-8 | doğru | Orta → Düşük | sıradan | §16.2 madde 4, satır 1794 | 1 | K-1380 |
| B-L3-5-4 | doğru | Orta → Düşük | sıradan | §18.2 madde 2 kod örneği, satır 1937 ve madde 3 | 1 | K-1381 |
| B-L3-5-5 | kısmen | Orta → Düşük | sıradan | §19.1 madde 5 ile §19.2 yasak paragrafı, satır 1966 ve 2015 | 1 | K-1381 |
| B-L3-5-6 | doğru | Orta → Düşük | sıradan | §19.1 madde 4, şablon veritabanı, satır 1963 | 1 | K-1381 |
| B-L3-5-8 | doğru | Düşük → Düşük | sıradan | §21.4 madde 2, satır 2108 | 1 | K-1381 |
| B-L3-5-9 | doğru | Düşük → Düşük | sıradan | §19.2 madde 6, satır 1998 | 1 | K-1381 |

#### Katman 4 — Güvenlik (OWASP)

| Bulgu | Hüküm | Seviye (mercek → son) | Karar | Yer | Çift | Kayıt |
|---|---|---|---|---|---|---|
| B-L4a-1 | doğru | Orta → Orta | sıradan | §15.1 madde 8, satır 1579 | 1 | K-1380 |
| B-L4a-2 | kısmen | Orta → Orta | sıradan | §15.1 madde 4, satır 1575 (ve §11.4) | 1 | K-1380 |
| B-L4b-2 | doğru | Orta → Orta | ⚠ | §10.8 madde 1, satır 1133 | 1 | K-1373 |
| B-L4b-4 | kısmen | Orta → Orta | ⚠ | §11.4 madde 2, satır 1353; §16.1 madde 9, satır 1771 | 2 | K-1373 |
| B-L4a-4 | kısmen | Düşük → Düşük | sıradan | §15.1 madde 5, satır 1576 | 1 | K-1380 |
| B-L4a-6 | kısmen | Düşük → Düşük | sıradan | §15.6 (madde 8 öncesi), satır 1708 | 1 | K-1380 |
| B-L4a-7 | doğru | Düşük → Düşük | sıradan | §15.2 madde 5, satır 1594 | 2 | K-1380 |
| B-L4b-10 | doğru | Düşük → Düşük | ⚠ | §10.8 madde 3, satır 1155-1161 | 2 | K-1373 |
| B-L4b-5 | doğru | Orta → Düşük | sıradan | §11.4 madde 2, satır 1353 | 2 | K-1380 |
| B-L4b-8 | doğru | Düşük → Düşük | sıradan | §16.1 madde 9, satır 1771 | 1 | K-1380 |
| B-L4b-9 | kısmen | Düşük → Düşük | sıradan | §16.2 madde 6.4, satır 1803 | 1 | K-1380 |

#### Katman 5 — Hata modu

| Bulgu | Hüküm | Seviye (mercek → son) | Karar | Yer | Çift | Kayıt |
|---|---|---|---|---|---|---|
| B-L5-3 | kısmen | Orta → Düşük | sıradan | §8.1 madde 3 tablo ve madde 5, satır 634, 640 | 1 | K-1382 |
| B-L5-4 | kısmen | Düşük → Düşük | sıradan | §13.3 madde 2, satır 1493 | 0 | K-1379 |
| B-L5-5 | kısmen | Düşük → Düşük | sıradan | §13.1 madde 6 ve 9, satır 1443, 1446 | 1 | K-1379 |

#### Katman 6 — Veri akışı

| Bulgu | Hüküm | Seviye (mercek → son) | Karar | Yer | Çift | Kayıt |
|---|---|---|---|---|---|---|
| B-L6-1 | doğru | Yüksek → Yüksek | ⚠ | §9.2 madde 6-7, satır 731 ve 743 | 6 | K-1374 |
| B-L6-3 | doğru | Orta → Yüksek | sıradan | §18.2 madde 2(b) ve madde 7, satır 1930 ve 1943 | 2 | K-1381 |
| B-L6-2 | kısmen | Orta → Orta | sıradan | §9.2 madde 4, satır 729 | 3 | K-1381 |
| B-L6-5 | doğru | Düşük → Orta | ⚠ | §18.2 madde 2(c), satır 1930 | 2 | K-1374 |
| B-L6-4 | kısmen | Orta → Düşük | sıradan | §11.1 madde 6, satır 1311 (ve §11.2 madde 4 ile çelişki) | 1 | K-1381 |

#### Katman 7 — Ölçeklenebilirlik

| Bulgu | Hüküm | Seviye (mercek → son) | Karar | Yer | Çift | Kayıt |
|---|---|---|---|---|---|---|
| B-L7-3 | kısmen | Düşük → Düşük | sıradan | §10.4 madde 3 (satır 961) | 1 | K-1382 |

#### Katman 8 — Bağımlılık riski

| Bulgu | Hüküm | Seviye (mercek → son) | Karar | Yer | Çift | Kayıt |
|---|---|---|---|---|---|---|
| B-L8-1 | kısmen | Orta → Orta | ⚠ | §3.3 madde 1 ve 7, satır 255 ve 261 | 2 | K-1375 |
| B-L8-3 | kısmen | Orta → Orta | ⚠ | Kütüphaneler tablosu, `Magick.NET-Q8-AnyCPU` satırı, satır 350; §3.3 madde 2 ve 4 | 2 | K-1375 |
| B-L8-4 | doğru | Orta → Orta | sıradan | §3.2 son paragraf (satır 249) ile Kütüphaneler tablosu | 1 | K-1382 |
| B-L8-5 | kısmen | Düşük → Düşük | sıradan | §3.2, satır 234 | 1 | K-1382 |
| B-L8-6 | kısmen | Düşük → Düşük | sıradan | Kütüphaneler tablosu, restic satırı, satır 361; §3.3 madde 7 | 1 | K-1382 |
| B-L8-8 | doğru | Düşük → Düşük | sıradan | §3.2, satır 234 | 1 | K-1382 |

### Çürütülen bulgular (uygulanmadı)

| Bulgu | Mercek seviyesi | Gerekçe (doğrulayıcının hükmünden) |
|---|---|---|
| B-L3-1-9 | Düşük | çürük. Kütüphane ekleme yolu zaten yazılı. 339 ve §3.3, her paketin tabloya §3.3'ün kanıtıyla girdiğini söylüyor; ajan xUnit eklenti paketini isterse tanımlı yol bu ve bu yanlış kod değil. |
| B-L3-2-10 | Düşük | çürük. `06 §4.63` (06 satır 4522-4523) ikisini de tanımlıyor: `TargetRecordId` `uuid` ve boş olabilir; `Details` `jsonb` değer listesidir (§2.5. |
| B-L3-2-6 | Düşük |  |
| B-L4a-3 | Orta | çürük. "Liste yalnız genişler" 09'un kendi kuralı değil, üst belgenin (`06 §2.10.2`, satır 2824) kuralı. 09 satır 1609 yalnız onu uyguluyor. Üst belgeye istisna eklemek kılavuzun işi değil. |
| B-L4a-5 | Düşük | çürük. 09 ürün kuralını doğru uyguluyor. `02 §11.2` P-15 "2 saat, son işlemden" ve P-16 "30 gün, son işlemden" diyor (K-108, K-459), yani kayan süre bilinçli bir ürün kararı. |
| B-L4a-8 | Düşük | çürük. Sabit zamanlı karşılaştırma §11.1 madde 5'te (satır 1310) zaten yazılı: "Hesaplanan imza gelenle `CryptographicOperations.FixedTimeEquals` ile karşılaştırılır". |
| B-L4a-9 | Düşük | çürük. Uygulamanın kayıtları için §18.2 madde 2 (satır 1930) `PathMasker.Mask` ile `/indir/{imzalı değer}` yolunu açıkça maskeliyor ve madde 7'nin testinde (satır 1943) `/indir/<değer>` satırı var. |
| B-L4b-1 | Orta |  |
| B-L4b-3 | Orta | çürük. Satır 1345'in "Google hata yanıtı verirse `Unavailable`" kuralı `08 §2. |
| B-L4b-6 | Düşük | çürük. |
| B-L4b-7 | Düşük | çürük. Değerin biçimi başka katmanda kesin olarak kısıtlı: `06 §4. |
| B-L5-1 | Orta | çürük. |
| B-L5-2 | Orta | çürük. |
| B-L5-6 | Düşük | çürük. |
| B-L6-6 | Düşük | çürük. `email_verified`'ın ne yapılacağı yazılıdır ve bilinçli olarak çekirdektedir: `08 §3. |
| B-L6-7 | Düşük | çürük. Doğrulanmamış bekleyen kaydın dosyaya girmesi `07 §9.24.3`'ün bilinçli kararıdır ("hesap tek tiptir ve ulaşma listesi bir kişiyi dışarıda bırakmaz"; `02 §3.13.4`; `06 §4.36`; K-823). |
| B-L6-8 | Düşük | çürük. Karşılanmış: `PlaceholderFiller.Fill`'in kuralı (satır 767) "`null` değer boş metin olur (imha edilmiş alan)" der; onay anındaki değer kuralı "imhadan sonra alanlar `null` → boş olur" der ve §9. |
| B-L7-1 | Orta | çürük. Bu bir üst belge kararıdır: `05 §4.3.2` (05 satır 514) "Döngü satırları sırayla, birer birer işler — görsel türevleri dahil" der ve gecikmenin sonucu değiştirmediğini kabul eder; `05 §5.1. |
| B-L7-2 | Düşük | çürük. Süre sonunun olmaması bilinçli bir karar: K-1339 (2) "süre sonu yoktur, tek uçuş kilidi yoktur" diyor ve satır 950 da bunu yazıyor. |
| B-L7-4 | Düşük | çürük. |
| B-L7-5 | Düşük | çürük. Tek uçuşun olmaması bilinçli bir karar: K-1339 (2) ve satır 950'nin gerekçesi (küçük sunucuda çift yük, kilidin bekletmesinden ucuz). `05 §3.7. |
| B-L7-6 | Düşük | çürük. Ölçümün kaynağı zaten var: erişim kaydı her istek için süreyi yazar (`05 §9.1.2`, "süre" alanı) ve sorgu aşımı `DbTimeout` olayıyla uygulama kaydına düşer (satır 1939). |
| B-L8-2 | Orta | çürük. Soyutlama katmanı §15.6 madde 1'de (satır 1682) yazılıdır: "Tek yer: `Infrastructure/Imaging/ImageProcessor`. |
| B-L8-7 | Düşük | çürük. Genel çıkış yolu zaten yazılı: §3.3 (7) "bakımı bitiyorsa yerine açık lisanslı bir aday için task açılır (`05 §8.8.2`, §8.8.4)" (satır 261) her tablo satırına uygulanır. |

### Yeniden doğrulama (seviyesi yükselen altı bulgu)

| Bulgu | İlk doğrulayıcı | Yeniden doğrulama | Şartname |
|---|---|---|---|
| B-L3-1-7 | doğru, Orta → Yüksek | doğru, Yüksek onaylandı | Sıkılaştırıldı: kalıcı kalması gereken yazımın dayanağı 09'un kendi kuralı oldu (deneme kaydının `RecordAsync` ve `ForgetAsync`'i), ret testi regresyon satırı aldı |
| B-L3-3-4 | kısmen, Orta → Yüksek | kısmen, Yüksek onaylandı | Genişletildi: ofsetli an UTC'ye çevrilir, test maddesine ikinci çift eklendi (+1 çift) |
| B-L3-4-5 | doğru, Orta → Yüksek | doğru, Yüksek onaylandı | Sıkılaştırıldı: tür kütüphaneye girmeden `ImageKinds.Sniff` ile sezilir, `MagickImageInfo` dahil kütüphaneye hiç verilmez |
| B-L6-3 | doğru, Orta → Yüksek | doğru, Yüksek onaylandı | Düzeltildi: ESKİ metin noktalı virgüle kadar uzatıldı (yan cümleye yapışma), 'kuramsal test' terimi kaldırıldı |
| B-L3-3-7 | kısmen, Düşük → Orta | kısmen, Orta onaylandı | Yeniden yazıldı: akış `await using` değil `RegisterForDisposeAsync`; çatının dosya yanıtı başlıkları yazar (+1 çift) |
| B-L6-5 | doğru, Düşük → Orta | doğru, Orta onaylandı | Genişletildi: `stackTrace` alanı `ToString()` değil yığın izi; kaynak taraması `Include Error Detail` (+1 çift) |

Yeniden doğrulamada hiçbir bulgu çürümedi; altısı da yeniden doğrulayıcının şartnamesiyle uygulandı.

### Aksiyon Planı

**Critical (Yüksek, uygulandı):** B-L3-1-7 (red işlemi geri almıyordu) · B-L3-3-4 (ofsetsiz an dönüş noktasını kaydırıyordu) · B-L3-4-5 (görsel türü kütüphane sezmesine bırakılmıştı) · B-L6-1 (imha fonksiyonunun zaman parametresi üst sınırsızdı) · B-L6-3 (yol maskesinin örnekleri `07` ile çelişiyordu) — hepsi `09` v0.18'de.
**High / Medium (Orta, uygulandı):** 28 Orta bulgu `09`'da.
**Üst dokümanlara kalan iki fark** (şartname değil, farkındalık; cross-review'un girdisi): (1) `DEPLOY_RUNBOOK` ters vekil kayıt maskesi listesinde `/odeme/donus/` yok, oysa yol erişim anahtarı taşıyor (`07 §2.1.9`); (2) `05 §8.6.3` dönüş noktasını 'sunucunun saatiyle' prova kaydına yazdırıyor, kayıt ofset taşımalı.
**Low (Düşük, uygulandı):** 48 bulgu `09`'da; çoğu kod örneği düzeltmesi ya da eksik kalıp.

### ⚠ ayrımı (proje sahibine bilgi)

Dokuz bulgu para, yasal hak, güvenlik güvencesi ya da geri dönüşü zor bir karar taşıdığı için ⚠ öneriyle kaydedildi (K-1372…K-1375); tek makul öneri yazıldı, çok kritik soru çıkmadı:

| Bulgu | Seviye | Alınan karar | Elenen seçenek | Kayıt |
|---|---|---|---|---|
| B-L3-1-7 | Yüksek | İş red dönerse işlem geri alınır, red çağırana aynen döner | Reddi istisnaya çevirmek (hata yönetimi kuralına aykırı) | K-1372 |
| B-L3-3-4 | Yüksek | Ofsetsiz an ve yalnız tarih olan dönüş noktası kullanım hatasıdır | Ofsetsiz anı Türkiye saati saymak (sessiz kayma) | K-1372 |
| B-L4b-2 | Orta | Yedek kullanıcısının `pg_hba.conf` satırı yalnız onu adlandırır, `all` kapsamlı `trust` kalkar | Yerel soketlerde `trust`'ı bırakmak | K-1373 |
| B-L4b-10 | Düşük | Göç geçmişi tablosu uygulama kullanıcısına kapalıdır | Varsayılan yetkide bırakmak | K-1373 |
| B-L4b-4 | Orta | Ham çıktının ikinci yeri yapılandırılmış veri bloğu; mimari test bunu sınar | Yasağı yalnız e-posta ve belge ad alanlarına bağlı bırakmak | K-1373 |
| B-L6-1 | Yüksek | İmha fonksiyonu `as_of`'u veritabanı saatine üstten kırpar (tek istisna) | Parametreyi kaldırıp fonksiyona `now()` okutmak | K-1374 |
| B-L6-5 | Orta | İstisna iletisi örüntüyle maskelenir (e-posta, IBAN, telefon) | İletiyi hiç yazmamak, yalnız tür adı | K-1374 |
| B-L8-1 | Orta | Açığı kapatan sürüm acil yamadır; bulgu yalnız açık kimliğiyle ve aday task'ı anılarak bastırılır | Bulguyu hiç bastırmamak | K-1375 |
| B-L8-3 | Orta | Gömülü yerel kütüphanenin lisansı ilk task'ta okunur, kopyaleft çıkarsa kapı tutmamış sayılır | Dinamik bağlı yerel kütüphane için istisna tanımak | K-1375 |

### Cross-reference Haritası

Bulguların dayandığı ya da düzeltmelerin atıf yaptığı üst dokümanlar (şartname metinlerinde geçen atıf sayısı):

| Üst doküman | Atıf sayısı | Durum |
|---|---|---|
| `02` | 2 | değişmedi |
| `05` | 18 | değişmedi (v0.39) |
| `06` | 2 | değişmedi |
| `07` | 2 | değişmedi |
| `08` | 2 | değişmedi |

Hiçbir şartname üst dokümana dokunmadı; `09 §2` geri besleme tablosuna satır eklenmedi.

### Uygulama

- **Betik:** `scratchpad` altındaki tek uygulama betiği; her ESKİ metnin dokümanda uygulama öncesi ve sırasında **tam bir kez** geçtiğini doğruladı (sorunlu çift 0).
- **Uygulanan çift: 124** (81 bulgu). Mercek sonucundaki 124 çiftten yeniden doğrulamanın şartname değişiklikleri (+3 çift) eklendi ve çakışan üç çift düşürüldü.
- **Eşzamanlı güncellemeler (checklist §4):** §23 ('Veri, zaman ve para' satırı: SQL'de zaman okuma yasağı ve tek izinli satır), §24 ('Sipariş durum geçişi' satırı: ret işlemi geri alır), §25 ('Geçişte işlem sırası' satırı: ret işlemi geri alıyor mu); §3.5, Kütüphaneler tablosu (`Magick.NET` ve restic satırları) ve başlık notu ilgili şartnamelerin içinde güncellendi; okuma tablosunun bölüm eşlemesi değişmedi.
- **Sürümler:** `09` v0.17 → v0.18; karar kaydı v0.215 → v0.216; K-1372…K-1382 (11 satır). Üst doküman sürümü artmadı.
- **Tarama:** CR, kontrol karakteri, eklenen satırlarda `$` + rakam artığı, çift K, tablo sütun sayısı — PR öncesi koştu.

### Sınırlar

- Mercekler ucuz modelle üretildi; 24 bulgu çürütüldü, yani ham bulgunun yaklaşık dörtte biri yanlış alarmdı. Bulgu doğrulaması bu yüzden zorunludur.
- Mercek paketleri yalnız `09`'u ve ilgili karar satırlarını verir; üst dokümanlardaki doğrulanamayan etki atıfları audit'ten devredilen sınır olarak kaldı ve cross-review'un girdisidir.
- Belirteç tüketimi bu raporda ölçülmedi; sayılar mercek ve doğrulayıcı sayılarıdır.

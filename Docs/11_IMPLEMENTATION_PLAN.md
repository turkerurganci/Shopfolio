# Shopfolio — Implementation Plan

**Versiyon: v0.9** | **Bağımlılıklar:** `02`–`10` | **Son güncelleme:** 2026-10-31

> **Aşama:** 9 — Implementation Planı · **Rol:** Delivery Lead
> **Traceability zorunlu: EVET** — §7 tamamlanmadan §5'e (task listesi) geçilmez.
> **Not:** Bu doküman **tüketici**dir — `02`–`10`'u tüketir, onlara geriye etki yaratmaz. Buradaki düzeltmeler yalnız bu dokümanı hizalar.

> **Aşama 8'den park edilen girdiler** — kaynak: [`PRODUCT_DISCOVERY_STATUS.md`](PRODUCT_DISCOVERY_STATUS.md) §2, mekanizma: K-36 (yeri: §20.1, yazım turu Y-6b).
> Bu satırlar **talimattır, karar değildir** — bağlayıcı olan kaynak karardır; bu aşamada karara bağlanacak olan, talimatın nasıl uygulanacağıdır.
>
> - **K-1302 (4) · K-1357 · Okunacak `09` bölümleri:** Kodlama Kılavuzu (`09`) 100 KB'ı aştığı için her görevde bütün okunmaz (`INSTRUCTIONS §9` (7)); `00 §C.6` ("her kodlama görevinde") değişmedi, okunma biçimi bölüm bölümdür. Bu doküman her task satırında okunacak `09` bölümlerini yazar: **çekirdek yedi bölüm** (§1, §2, §5, §6, §23, §24, §25 — toplamı 100 KB'ın altındadır) her satırda "çekirdek" diye anılır ve ayrıca sayılmaz; çekirdeğe **ek** bölümler task'ın dokunduğu alana göre `09` başlık notunun **okuma tablosundan** alınır (ör. göç yazan task §10 ve §12; dış servis çağıran task §11 ve §15; C# yazan her task §3.1 ve §3.2). Task satırının kabul kriterleri ilgili bölümlerin Test maddelerine ve §25'in ilgili satırlarına dayanır; yapım PR'ı okunan bölümleri açıklamasında yazar (`09 §21.3`, `§24` madde 1).
> - **K-1356 · D-01 — eşitlik denetimlerinin Lint adımına girmesi:** `09 §21.4` madde 1'in beş eşitlik denetimi (kütüphane tablosu, parametre envanteri, hata kodu envanteri, geçiş listesi, işaretli işlem listesi) `UnitTests/Consistency/` klasöründe `Consistency` özelliğiyle işaretli birim testleridir; Lint adımı onları koşar, Unit adımı dışarıda bırakır. Özelliğe göre süzen komut D-01'in işidir ve `.github/ci-commands.env`'e yazılır (`05 §2.3.5`); komutların değerini `09` ikinci kez yazmaz.
> - **K-1356 · D-01 — yama sürümü denetiminin `publish.yml`'e girmesi:** "Yama sürümü göç taşımaz" denetimi yayın akışında, etiket imaja basılmadan önce koşar (`09 §21.4` madde 2; `§10.6` madde 7); PR'da koşmaz, çünkü PR sürüm etiketini bilmez. İstisna — veri bozan bir göçün düzeltmesi — etiket mesajındaki `migration-fix: K-<numara>` satırıdır. İlk yayından önce son yayımlanan sürüm bulunmadığından geri alma sınaması atlanır ve atlandığı CI çıktısında yazılır (`09 §21.4` madde 3).
> - **K-1355 · Test projeleri — sahtelerin paylaşımı:** `UnitTests/Fakes/` altındaki sahteleri `IntegrationTests` bağlantılı dosya olarak derler (ikinci kopya yok, iki projeyi bağlayan proje başvurusu yok); çalıştırılabilir bir test projesine başvuru xUnit v3'te sorun çıkarabilir (bilgim tarihli) — test altyapısını kuran ilk task'ta sınanır (`09 §19.1`).
> - **K-1354 · Yükleme betiği, kayıt biçimleyicisi ve maskeleme testleri:** Kodlama Kılavuzu (`09`) üç yapım işi bırakır ve her birinin bu planda bir task karşılığı bulunur: (1) parçalı yükleme betiği — başlatma–parça–durum akışını, 409/404/400/413 yanıtlarını ve ağ hatasında üç kez sürdürmeyi kapsar (`09 §16.2`; `07 §4.31.2`). (2) Uygulama kayıtlarının üç yazıcısı ve satır biçimleyicisi `ApplicationLogFormatter` — alan adlarını ve kapalı olay listesini (`LogEvents`) kapsar (`09 §17`, `§18.1`, `§18.2`; `05 §9.1`). (3) Maskelemenin üç katmanının testleri: yazım, `PathMasker`, biçimleyici ve `ExceptionMasker` (`09 §18.2`). Bu işler izlenebilirlik matrisinin (§7.1) kaynak öğelerinde adıyla geçmeyebilir; task'ın kaynağı `09` bölümüdür. Task'ın nasıl bölünüceğine bu aşama karar verir.

---

## 1. Amaç ve kapsam

> **Ne yazılır:** Tüm işi küçük, bağımsız, sıralı task'lara bölmek. Her task'ın ne olduğu, neye bağımlı olduğu, hangi dokümanlarla yapılacağı ve nasıl kabul edileceği.

## 2. Kaynak envanteri özeti

> **Ne yazılır:** Her kaynak dokümandan çıkarılan öğe sayısı. Traceability matrisinin denetlenebilirliği bu sayılara dayanır.

| Doküman | Öğe sayısı | Kapsam |
|---|---|---|

## 3. Faz tanımları

> **Ne yazılır:** Fazlar ve task aralıkları. Faz sayısı projeye göre değişir; **şekil sabittir**: iskelet → veri → çekirdek servisler → iş mantığı → entegrasyonlar → arayüz → uçtan uca doğrulama.

| Faz | Ad | Kapsam | Task aralığı |
|---|---|---|---|
| F0 | Proje iskeleti | | |
| F1 | Veri katmanı | | |
| F2 | Çekirdek servisler | | |
| F3 | İş mantığı | | |
| F4 | Entegrasyonlar | | |
| F5 | Kullanıcı arayüzü | | |
| F6 | Uçtan uca doğrulama | | |

### 3.1 Faz bağımlılıkları

> **Ne yazılır:** Zincir ve varsa kontrollü paralellik istisnaları.

## 4. Hata sınıflandırması ve çözüm akışı

| Seviye | Tanım | Tespit |
|---|---|---|
| **S1 — Sapma** | Task tamamlandı ama dokümanla uyumsuz | Task doğrulama |
| **S2 — Kırılma** | Bir task başka bir task'ın çıktısını bozuyor | Regresyon (gate check) |
| **S3 — Eksik** | Eşlenmiş öğe implement edilmemiş | Boşluk taraması (gate check) |

**Kritik kural:** Düzeltme task'ı bir sonraki faza ertelenmez — hatanın oluştuğu fazda çözülür.

## 5. Task listesi

### Task yapısı

```
Task TXX: [Task adı]
  Bağımlılık:              [Önceden tamamlanmış olması gereken task'lar]
  Dokümanlar:              [Ajana verilecek dosyalar ve bölümler]
  Kabul kriterleri:        [Ne olduğunda "tamam"]
  Test beklentisi:         [Unit / Integration / Contract / E2E / Yok]
  Doğrulama kontrol listesi: [Validator'ın neye bakacağı]
```

> **Numaralandırma:** Araya task gerektiğinde `TXXa`/`TXXb` (faz aralığını bozmadan). Aynı task'ın devam turu `TXX.1`. Konvansiyonların tamamı: `00_PROJECT_METHODOLOGY.md` §J.
> **Forward pointer:** Henüz yazılmamış bir dokümana verilen referans **açıkça** öyle işaretlenir — bu bir eksik bağımlılık değildir.

### F0 — <Faz adı>

#### T01: <Task adı>
- **Bağımlılık:**
- **Dokümanlar:**
- **Kabul kriterleri:**
- **Test beklentisi:**
- **Doğrulama kontrol listesi:**

## 6. Faz geçiş kapıları

### 6.1 Genel gate check (tüm fazlarda)

> Detaylı prosedür: `.claude/skills/gate-check/SKILL.md`. Burada faz-spesifik ek kontroller listelenir.

### 6.2 Faz-spesifik kontroller

| Faz | Ek kontrol |
|---|---|

## 7. Traceability Matrix (ÖNCE BU)

> **Ne yazılır:** Matris **çift yönlüdür** (00 §C.3). Tek yön yeterli değildir — ileri yön eksik işi, geri yön gereksiz işi yakalar.

### 7.1 İleri izlenebilirlik (kaynak öğe → task)

Tüm önceki doküman çıktıları (entity, endpoint, ekran, entegrasyon, iş kuralı) → task eşlemesi.
**Eşlenmeyen çıktı = eksik task.**

**Alt bölüm haritası (K-1412).** Matris yedi oturumda kurulur (K-1404); O1 haritayı sabitledi. Her alt bölüm `####` başlığıdır; yeni bölüm numarası açılmaz (K-1406 (2)(c)).

| Alt bölüm | İçerik | Kaynak ailesi (birim) | Oturum | Durum |
|---|---|---|---|---|
| 7.1.1 | Veri modeli → task | `06` (117: entity 79 · enum alt bölümü 6 · alt bölüm 32) | O1 | ✓ |
| 7.1.2 | API → task | `07` (208: uç 187 · alt bölüm 21) | O2 | ✓ |
| 7.1.3 | Entegrasyon → task | `08` (30: entegrasyon 3 · çağrısı olmayan bağlantı 1 · alt bölüm 26) | O1 | ✓ |
| 7.1.4 | Arayüz → task | `04` (102: ekran 53 · ortak bileşen 20 · alt bölüm 29) ve `03` (55 alt bölüm) | O3 | ✓ |
| 7.1.5 | İş kuralı → task — kalın etiketli beş grup: **`02`** · **`05`** · **`09`** · **`10`** · **Devir satırları** | `02 §1`–§3 (297) O4a; `02 §4`–§8 (193) O4b 1/2; `02 §9`–§13 (176) O4b 2/2; `05` (136) O5 1/2; `09` (25) · `10` (143) · devir (19) O5 | O4a, O4b, O5 | ⬚ (`02` §1–§13 ✓ O4a, O4b; `05` ✓ O5 1/2; `09`, `10`, devir ✓ O5 2/2 — ileri yön 1.501/1.501) |
| 7.2, §8 | Geri izlenebilirlik, gerekçesiz eklemeler, GAP kararları | task → kaynak | O6 | ⬚ |

**Kaynak kimliğinin biçimi (K-1412).** Satırın ilk hücresi kimlikle başlar; kimliğin ardından başlık ya da ad gelir (en çok 500 bayt/satır). Kapsama betiği yalnız kimliği okur.

| Aile | Kimlik | Örnek |
|---|---|---|
| `06` entity | `06:§4.<n> <Entity>` — `n` §3.1 tablosunun "§4" sütunundan; aynı alt bölümü paylaşan entity'ler adla ayrılır | `06:§4.17 Order` |
| `06` enum alt bölümü ve alt bölüm | `06:§<n.m> <başlık>` | `06:§5.2 Durum setleri ve geçiş kimlikleri` |
| `08` entegrasyon, çağrısı olmayan bağlantı, alt bölüm | `08:§<n.m[.k]> <başlık>` | `08:§2.1.4 …`, `08:§3.1 …` |
| `02` kural ve alt bölüm (O4a) | `02:§3.<n.m> <kural başlığı>` (kalın başlık cümlesi), `02:§<n.m> <başlık>` — `02 §1`–§3 | `02:§3.6.7 Stok ve hizmet kontenjanı…`, `02:§3.6 Stok ve tükenme` |
| `02` kimlikli satır (O4b 1/2) | `02:<Z-n \| L-n> <satır adı>` — `02 §4.2` (Z-1…Z-47) ve `§8.2` (L-1…L-9); `02 §4`–§8 kural ve alt bölümü O4a biçiminde | `02:Z-7 Kart ödeme süresi`, `02:L-5 Başarısız misafir sipariş takibi sorgusu` |
| `05` karar ve alt bölüm (O5 1/2) | `05:MK-<nn>` (§12 tablosu), `05:§<n.m>` | `05:MK-31`, `05:§6.1` |
| `09` bölüm (O5 2/2) | `09:§<n>` — `## n.` başlığı, n = 1…25 | `09:§10 Veri erişimi` |
| `10` satır ve alt bölüm (O5 2/2) | `10:KP-<n>` (§2), `10:KD-<n>` (§3, negatif denetim), `10:§4.1`, `10:§4.2` | `10:KP-15 …`, `10:KD-3 …` |
| Devir satırı (O5 2/2) | `devir:K-<n>` (karar), `devir:P8.<n>` (park maddesi), `devir:<doküman>-<satır>` (gövde anması), `devir:D-01` | `devir:K-1356 …`, `devir:04-1948 …` |

**Satır sütunları (K-1412).** **Kaynak öğe** (kimlik + ad) · **Task** (aday task kimliği; birden çok ise virgülle; task gerektirmeyen birim `—`) · **Implement edildi** (boş — gate'te dolar) · **Kanıt** (boş — gate'te dolar). Kapsama betiği son iki sütunun bu aşamada boş olduğunu da denetler.

**Task gerektirmeyen birim.** `08:§5.1` — `07`'ye geri yansıyan uçların kaydı: yeni uç çıkmamıştır ve üç mevcut ucun (UC-29, UC-30, UC-80) işi `07` satırlarının task'larındadır (O2: UC-29 ve UC-30 T510'da, UC-80 T311'de; ikisi de `08:§2.1.4` ve `08:§2.1.5` satırlarına eklendi). GAP değildir.

`03:§9.5` — Tercih yönetimi: bölüm tercih yönetiminin ürünün içinde karşılığı olmadığını yazar (işlem bildirimleri kapatılamaz, bildirim tercihi ekranı yoktur — `02 §9.1.3`); yapılacak iş çıkmaz, "Task" hücresi `—` (O3). GAP değildir.

`02:§1.1` — Sözlük kuralı ve `02:§1.2` — Terim sözlüğü: doküman yazım konvansiyonudur ("tek ad, İngilizce karşılık Aşama 1'de, `06`/`07`/`09` isim icat etmez"); kod adları `06` ve `07` satırlarındaki task'larda (T101–T120, T206–T217, T502–T539) ve her task'ın kendi kavramının adında yaşar, yapılacak ayrı iş çıkmaz — "Task" hücresi `—` (O4a). GAP değildir. `02:§3.26` — İptal, cayma, iade ve ayıp talebi: bölüm kuralların `§7`'de olduğunu söyleyen bir işarettir, kendi kuralı yoktur; kuralları `02 §7` satırlarında eşlenir (O4b) — "Task" hücresi `—` (O4a). GAP değildir.

Kaynak (O4b 2/2): `02 §9`–§13 (176: iş kuralı 80 `02:§n.m.k` · kimlikli satır 75 — B-1…B-16, F-1…F-6, P-1…P-49 ve H-1…H-4 · alt bölüm 21 `02:§n.m`; §9 37 · §10 50 · §11 56 · §12 33 · §13 0). `§13` tablosu boştur ve numaralı kural taşımaz (`§13.1`–§13.4 iki basamaklıdır, K-1404'ün kalıp dışı içerik kuralı): açık karar yoktur. `§11`'in P satırları parametre kimliğidir: her satır T381'e (ayar katmanları ve envanter) ve "Nerede kullanılır" sütununun işaret ettiği kuralların task'larına (en çok beş) bağlanır; `P-2` ve `P-11` "Kaldırıldı" satırlarıdır. `§12.5` firma yükümlülükleri tablosunun satırları birim değildir; alt bölüm satırı onların task birleşimini taşır. Alt bölüm satırı altındaki birimlerin task birleşimidir (`§9.4`, `§9.5` ve `§12.5` kural taşımadığı için açık yazılır).

Kaynak (O5 1/2): `05` (136: mimari karar 78 `05:MK-nn` · `###` alt bölümü 58 `05:§n.m`; §3 8 · §4 7 · §5 4 · §6 10 · §7 10 · §8 9 · §9 4 · §10 3 · §11 3). `### 6.x Secret yönetimi` başlığı kimlikte `05:§6 6.x Secret yönetimi` diye yazılır (betik `05:§6` okur). Bir mimari karar birden çok task alır; alt bölüm satırı o bölümün task birleşimini taşır (K-1431). Yeni aday 23: F0 T02–T09 (iskele), F2 T218–T229 (ortak altyapı), F3 T387–T388 (yönetim komutları), F6 T602 (güncelleme komutu) — K-1432 (⚠). **Task gerektirmeyen dört birim (GAP değil):** `05:MK-77` (H-3, H-4 hacim kestirimleri; sistem hiçbirini uygulamaz — `02 §11.3`), `05:MK-78` (büyüme yolunun sırası ve tetikleyicileri — bugün alınan kararların açtığı kapılar), `05:§11.2` (bilinçli tekil bırakılanlar) ve `05:§11.3` (büyüme yolu) — "Task" hücresi `—` (O5 1/2).

Kaynak (O5 2/2): `09` (25: `## n.` bölümü), `10` (143: KP satırı 77 · KD satırı 64 · alt bölüm 2 — `§4.1`, `§4.2`) ve devir satırları (19: karar 9 · park maddesi 5 · gövde anması 4 · D-01 1; `11_devir_esle.py` A–F). **`09` bölümü task üretmez; task'ın "Dokümanlar" alanının kaynağıdır** (K-1404): her satırın "Task" hücresi, bölümün kurallarını koda yazan ya da denetleyen task'lardır. **Çekirdek yedi bölüm** (`§1`, `§2`, `§5`, `§6`, `§23`, `§24`, `§25`) her task'ın "Dokümanlar" alanında "çekirdek" diye anılır ve ayrıca sayılmaz (park bloğu K-1302 (4), K-1357); `§5`, `§6` ve `§23`'ün kuralları ayrıca kod ya da denetim işi taşıdığından ilgili task'a da bağlıdır. **`KD` satırı negatif denetimdir:** task açılmaz; satır, "yoktur" kararının kabul kriterinde ya da testinde korunduğu kapsamdaki task'a bağlanır. **Yeni aday 3:** F2 `T230` (kayıt yazıcıları ve biçimleyici), F2 `T231` (maskeleme testleri), F5 `T548` (parçalı yükleme betiği) — park bloğunun üç yapım işi (K-1354, K-1391; kararın biçimi K-1406 (6)). **Task gerektirmeyen birim (GAP değil):** `09:§1`, `§2`, `§22`, `§24`, `§25` — yazım ve süreç kuralı; her task'ın "Dokümanlar" alanı ve kabul kriteri `§5`'in task yapısındadır (`IP5-03`), gate ve doğrulama skill'lerinin işidir; ayrı iş çıkmaz. Devirden `—`: `K-423`, `K-424` (yol haritası adayları, `§9` parkı ve `10 §5`; iş bırakmaz), `K-1302`, `K-1357`, `P8.1`, `09-117`, `09-2249` (okunacak `09` bölümleri — `§5`'in "Dokümanlar" alanı biçimi, `IP5-03`), `12-177` (kabul kriterleri ve kontrol listesi `§5` task yapısında tanımlıdır). `04-1948` (GA-7) ★: yasal metinlerin içeriği `02 §3.24.3`, §7.4.4, §12.2.5, §3.33.2 satırlarının task'larına (T356, T366, T373) bağlıdır — metinlerin yazımı `02 §3.33.1`–§3.33.2'de yazılı olduğundan soru açılmaz (K-1423 (3), K-1430 (3)). `D-01` F0'ın yedi task'ına dağılır (T02–T07, T09). `05:§9.1` satırına T230 ve T231 eklendi.

**Task gerektirmeyen birimler (O4b 2/2, GAP değil).** (1) `02:P-2` ve `02:P-11` — "Kaldırıldı" satırları: kimlik yeniden kullanılmaz, uygulanacak değer yoktur. (2) `02:H-1`, `02:H-3`, `02:H-4` — hacim kabulleri parametre değildir ve "sistem hiçbirini uygulamaz" (§11.3): tasarım tabanıdır; H-3'ün yanıt süresi ve H-4 `05`'in tasarım girdisidir; H-2 ise satış özetinin günlük sayısını okuyan tetikleyiciye dayandığı için T383'e bağlıdır. (3) `02:§10.5.5` — hacim varsayımı, "bir hedef değil, tasarım varsayımıdır". (4) `02:§12.1.12` ve `02:§12.2.11` — ürün ne satıldığını ve kurumsal içerikte yayımlanan üçüncü kişi verisinin iznini denetlemez; yükümlülük firmanındır (§12.5), yazılacak kod yoktur (Z-25 ve Z-37'nin kalıbı, K-1426 (3)). Bu sekiz birimin "Task" hücresi `—`.

**Aday task kimliği (K-1412).** "Task" sütunu aday kimliktir; adı ve gövdesi `§5`'te yazılır. Kurallar: (1) kimlik K-1407'nin faz aralığından gelir ve aşağıdaki aday tablosunda tanımlıdır (kapsama betiği denetler). (2) F1'in kimlikleri baştan ayrıldı; öteki fazlarda her matris oturumu kendi satırlarının gerek duyduğu kimliği sayaçtaki sonraki boş numaradan alır ve tabloya ekler. (3) Kimlikler faz içi sıra gözetmeden verilebilir; sıra `§5`'in işidir, araya `TXXa` girer (`00 §J`); gerekirse O6 betikli tek bir yeniden numaralama yapar ve kapsama betiği öncesini ve sonrasını koşar. (4) Var olan satıra aday kimlik eklenebilir (ör. UC-80'in F3 task'ı `08:§2.1.5` satırına girer); satır silinmez. (5) F1'in sırası modül akış sırasını (K-1410) bağların kurduğu sırayla daraltır: bir göç, bağlandığı tabloların göçünden sonra gelir (`06 §7.2`'nin 130 bağı betikle sınandı — sıra ihlali 0); Yönetim ve Üyelik ile iptal–cayma–iade ve Ödeme bu yüzden akış sırasından ayrılır.

| Faz | Son ayrılan | Sonraki boş |
|---|---|---|
| F0 | T09 | T10 |
| F1 | T120 | T121 |
| F2 | T229 | T230 |
| F3 | T388 | T389 |
| F4 | T410 | T411 |
| F5 | T547 | T548 |
| F6 | T602 | T603 |

**Aday task tablosu (O1).** F1 eksiksiz ayrıldı (79 entity, en çok 8 entity/göç — K-1409; sıra yukarıdaki (5)); öteki fazlarda yalnız `06`, `07` ve `08` satırlarının gerek duyduğu task'lar vardır (O2 55 aday ekledi: F0 1, F2 12, F3 4, F5 38; toplam 102; O3 23 aday ekledi: F3 15 `T315`–`T329`, F5 8 `T540`–`T547`; toplam 125; O4a 37 aday ekledi: F3 `T330`–`T366`, `02 §3`'ün iş kuralı task'ları; toplam 162; O4b 1/2 12 aday ekledi: F3 `T367`–`T378`, `02 §4`–§8'in iş kuralı task'ları; toplam 174; O4b 2/2 8 aday ekledi: F3 `T379`–`T386`, `02 §9`–§12'nin iş kuralı task'ları; toplam 182); O5 1/2 23 aday ekledi: F0 `T02`–`T09`, F2 `T218`–`T229`, F3 `T387`–`T388`, F6 `T602`, `05`in mimari kararları; toplam 205); O5 2/2 3 aday ekledi: F2 `T230`, `T231`, F5 `T548`, `09`'un park bloğunun üç yapım işi; toplam 208. Sayaçlar: F0 T10 · F2 T232 · F3 T389 · F4 T411 · F5 T549 · F6 T603) — O6 yalnız geri yönde yeni aday gerekirse ekler.

| Aday | Faz | Kısa ad | Modül | Kaynak birimi |
|---|---|---|---|---|
| T01 | F0 | Canlılık ucu: uygulama ve veritabanı yanıtı, 200 ya da 503, gövdesiz | Ortak altyapı | `07` UC-81 (1) |
| T02 | F0 | Çözüm ve proje düzeni: C#/.NET 10 tek uygulama, Razor Pages, düz JavaScript ve CSS (npm hattı yok), yığının kuruluma yansıması (D-01) | Ortak altyapı | `05 §2.1`–§2.3, §3.1, §3.8 (MK-01…MK-05) |
| T03 | F0 | Test altyapısı: birim, mimari ve gerçek veritabanıyla entegrasyon testi projeleri | Ortak altyapı | `05 §8.3`, §3.3 (mimari test) (MK-15, MK-70) |
| T04 | F0 | CI hattı ve eşitlik denetimleri: Lint, `publish.yml` denetimleri, sürüm etiketi, göç adımının önceki imajla sınaması | Ortak altyapı | `05 §8.3`, §8.7.4 (MK-12, MK-70) |
| T05 | F0 | Compose geliştirme ortamı: dört konteyner, ayrı iç ağlar, ters vekil, güvenlik duvarı ve açık kapılar | Ortak altyapı | `05 §3.2`, §8.1, §6.8 (MK-06, MK-08, MK-10, MK-14, MK-68, MK-76) |
| T06 | F0 | Üç imaj ve kayıt defteri: uygulama, pgBackRest'li veritabanı ve yedek aracı; özetle sabitleme, tek eser sürüm | Ortak altyapı | `05 §8.2`, §3.2 (MK-02, MK-09…MK-11, MK-69) |
| T07 | F0 | Yedek düzeni: sürekli günlük aktarımı, gecelik tam ve dosya yedeği, iki anahtarla şifreleme, saklama ve silmeye karşı koruma | Ortak altyapı | `05 §7.6.1`–§7.6.6 (MK-09, MK-25, MK-26) |
| T08 | F0 | Sır düzeni: bileşen başına dosya sırları, paylaşılan sır yok, döndürme | Ortak altyapı | `05 §6.x` (MK-59, MK-60) |
| T09 | F0 | İlk yayın ve referans kuruluma ilk iniş: boş uygulama imajı canlılık ucuna yanıt verir | Ortak altyapı | `05 §8.1`, §8.2 (MK-68) · K-1407 (4) |
| T101 | F1 | Veri modeli temeli: anahtarlar, türler, adlandırma, sürüm sütunu, metin karşılaştırma, enum biçimi, bağların silme davranışı | Ortak | `06 §2.1`–§2.6, §2.8–§2.10, §5.1 (10) |
| T102 | F1 | Enum tanımları ve değer dönüşümü | Ortak | `06 §5.2`–§5.6 (5) |
| T103 | F1 | Başlangıç listeleri, firma ve yasal metin sürümü: Province, Country, Company, CompanySettings, CompanyDeliveryProvince, LegalTextVersion | Ayarlar | 6 entity |
| T104 | F1 | Ürün çekirdeği ve görsel: Image, Product, Option, OptionValue, Variant, Category, ProductCategory, ProductImage | Katalog | 8 entity |
| T105 | F1 | Fiyat, kupon ve dijital dosya: PriceHistory, Discount, Coupon, DigitalFile | Katalog | 4 entity |
| T106 | F1 | Yönetici hesabı, davet ve işlem izi: Admin, AdminInvitation, AuditLog | Yönetim ve işlem izi | 3 entity |
| T107 | F1 | Müşteri hesabı: Customer, AddressBook, PendingEmailChange | Üyelik | 3 entity |
| T108 | F1 | Oturum, bağlantı, deneme ve giriş kayıtları: Session, LinkRecord, AttemptRecord, RecognizedBrowserMark, LoginRecord | Ortak altyapı | 5 entity |
| T109 | F1 | Sepet ve sipariş çekirdeği: Cart, CartItem, Order, OrderItem, TransitionRecord, InternalNote, CheckboxAcceptance, CheckboxAcceptanceOrderItem | Sepet ve sipariş | 8 entity |
| T110 | F1 | Ayırma ve düzeltme geçmişleri: StockReservation, OrderEmailHistory, OrderAddressHistory | Sepet ve sipariş | 3 entity |
| T111 | F1 | Kalem kayıtları: RefundIban, DefectClaim, Cancellation, ItemRemoval, LateDeliveryTermination, Withdrawal, Return, NotReturnedClosure | İptal, cayma ve iade | 8 entity |
| T112 | F1 | Ödeme ve geri ödeme kayıtları: PaymentRecord, Refund, RefundRecord, InboundNotificationRecord | Ödeme | 4 entity |
| T113 | F1 | İade reddi ve IBAN isteği: ReturnRejection, RefundIbanRequest | İptal, cayma ve iade | 2 entity |
| T114 | F1 | İçerik kayıtları ve marka: AboutPage, Offering, PortfolioItem, FaqItem, CustomPage, Announcement, SocialLink, Branding | Kurumsal içerik | 8 entity |
| T115 | F1 | Şube, içerik–ürün bağları ve iletişim talebi: Branch, OfferingProduct, PortfolioItemProduct, ContactRequest | Kurumsal içerik | 4 entity |
| T116 | F1 | Galeri bağları: AboutPageImage, OfferingImage, PortfolioItemImage, FaqItemImage, CustomPageImage | Kurumsal içerik | 5 entity |
| T117 | F1 | Gönderim kaydı ve kanal hâli: DispatchRecord, ChannelStateRecord | Bildirim | 2 entity |
| T118 | F1 | Zamanlayıcı, yedek, imha ve anahtar kayıtları: Job, HeartbeatRecord, OutageRecord, BackupStatusRecord, DataProtectionKey, DisposalRecord | Ortak altyapı | 6 entity |
| T119 | F1 | Başlangıç verisi göçü ve seed olmayanların sınırı | Ortak | `06 §8.1`, §8.4 (2) |
| T120 | F1 | Model eşitlik denetimleri: ilişki diyagramı, tekillik, yabancı anahtar, kontrol kısıtı, dizin, denormalizasyon envanteri | Ortak | `06 §2.7`, §6.1–§6.6, §7.1–§7.6 (13) |
| T201 | F2 | İmha düzenleyicisi, süre ayarları ve gecelik temizlik | Ortak altyapı | `06 §9.1`, §9.4, §9.5 (3) |
| T202 | F2 | Dış servislerin bağlantı katmanı: ortak yüz, hata sınıfı, zaman aşımı, sahte sağlayıcılar | Ortak altyapı | `08 §3.1` |
| T203 | F2 | İmzalı makine ucu altyapısı: imza denetimi, gövde tavanı | Ortak altyapı | `08 §3.2` |
| T204 | F2 | Dış servis çökmesi davranışı ve sahtelerle sınama | Ortak altyapı | `08 §3.3` |
| T205 | F2 | Açılışta veritabanı fail-fast denetimi | Ortak altyapı | `06 §8.2` |
| T206 | F2 | Adres yapısı, önekler ve sayfa–form işleyici kalıbı: gönder–yönlendir–göster, statik dosya sürüm izi | Ortak altyapı | `07 §2.1`, `§2.2`, `§2.10` (3) |
| T207 | F2 | Kimlik şemaları ve erişim reddi: panel ve müşteri şeması, oturumsuz geliş, sahiplik ve erişim anahtarlı kök | Ortak altyapı | `07 §2.3` (1) |
| T208 | F2 | Sürümün ve gösterilen değerin formda taşınması, koşullu güncelleme | Ortak altyapı | `07 §2.4` (1) |
| T209 | F2 | Tek kullanımlık değerler, tekrar gönderim, sahteciliğe karşı koruma ve güvenlik başlıkları | Ortak altyapı | `07 §2.5` (1) |
| T210 | F2 | Liste kalıbı: sayfalama, süzgeç parametreleri, geçersiz değer, arama motorlarına kapalı sayfa | Ortak altyapı | `07 §2.6` (1) |
| T211 | F2 | Model bağlama kalıbı: form girdi, sorgu ve makine ucu modeli türleri | Ortak altyapı | `07 §5.1` (1) |
| T212 | F2 | Hata gösterimi ve HTTP durumları: sayfa, form ve makine ucu; özel hâller | Ortak altyapı | `07 §2.7`, `§6.1`, `§6.4` (3) |
| T213 | F2 | Hata kodu kayıt defteri | Ortak altyapı | `07 §6.2` (1) |
| T214 | F2 | Kısa süreli bildirim kodları | Ortak altyapı | `07 §6.3` (1) |
| T215 | F2 | Hız sınırı, deneme limitleri ve gövde tavanı | Ortak altyapı | `07 §2.8` (1) |
| T216 | F2 | Tarih, sayı ve para biçimi: istek ve gösterim | Ortak altyapı | `07 §2.9` (1) |
| T217 | F2 | Yapılabilir işlemlerin sunucuda üretimi ve POST işleyicisinde yeniden denetim | Ortak altyapı | `07 §8.1` (1) |
| T218 | F2 | Dokuz modülün iskeleti ve Contracts: açık arayüz, eşzamanlı ve aynı işlemde erişim, mimari testle sınır | Ortak altyapı | `05 §3.3`, §3.4 (MK-13, MK-15) |
| T219 | F2 | Arama motoru verileri kalıbı: adres üretimi, JSON-LD, Open Graph, kapalı sayfada `noindex` | Ortak altyapı | `05 §3.7` (MK-18) |
| T220 | F2 | İş kuyruğu çalıştırıcısı: iş satırı, sıralı işleme, beş yeniden deneme, başarısız durum, geçişle aynı işlemde iç yan etki | Ortak altyapı | `05 §4.3`, §5.1, §5.2 (MK-37, MK-42) |
| T221 | F2 | Zamanlayıcı: dakikalık işler, nabız döngüsü, gecelik iş, işin zamanı kendi kaydında, kesinti tespiti ve dönüş sırası | Ortak altyapı | `05 §5.1`–§5.4 (MK-41, MK-43, MK-44) |
| T222 | F2 | Anahtarlar ve tek kullanımlık bağlantılar: kriptografik üreteç, türetme sırrı, özetli bağlantı kaydı, açılışın işlem yapmaması | Ortak altyapı | `05 §6.4` (MK-50, MK-51) |
| T223 | F2 | Dosya deposu: arayüz, görselin yeniden kodlanması, WebP türevleri, yetim görsel imhası, yüklenen dosya sınırları | Ortak altyapı | `05 §6.6`, §7.7.1, §7.7.2 (MK-28, MK-29, MK-54) |
| T224 | F2 | Çerez politikası: birinci taraf, `__Host-` önekli çerezler, ödeme adımı çerezi, çerez başlığı ara katmanı | Ortak altyapı | `05 §6.7` (MK-55…MK-57) |
| T225 | F2 | Sistem kayıtları: kaynaklar, istek kimliği, maskeleme, saklama süresi ve imha kaydı satırı | Ortak altyapı | `05 §9.1`, §9.2 (MK-64, MK-65) |
| T226 | F2 | Ayar katmanları, tasarım değerleri ve açılışta ayar denetimi | Ortak altyapı | `05 §8.4`, §11.1 (MK-71) |
| T227 | F2 | Zaman ve para: UTC saklama, Türkiye saatiyle sayım, tek yuvarlama işlevi | Ortak altyapı | `05 §7.2` (MK-19) |
| T228 | F2 | Eşzamanlılık denetimi: kayıt sürümü, sipariş sürümü ve satır kilidi, kilit sırası | Ortak altyapı | `05 §7.3` (MK-20, MK-21) |
| T229 | F2 | Uygulama belleğinde önbellek: kapsam ve geçersiz kılma | Ortak altyapı | `05 §7.10` (MK-34) |
| T230 | F2 | Uygulama kayıtlarının üç yazıcısı ve satır biçimleyicisi: alan adları ve kapalı olay listesi (`LogEvents`) | Ortak altyapı | `09 §18.1`, §18.2; K-1354, K-1391 |
| T231 | F2 | Maskelemenin üç katmanının testleri: yazım, yol maskeleyici, biçimleyici ve istisna maskeleyici | Ortak altyapı | `09 §18.2`; K-1354, K-1391 |
| T301 | F3 | Siparişin imha parçası | Sepet ve sipariş | `06 §9.2` |
| T302 | F3 | Öteki kayıtların süreyle imhası | Modüller | `06 §9.3` |
| T303 | F3 | İlk yönetici yönetim komutu | Yönetim ve işlem izi | `06 §8.3` |
| T304 | F3 | Alıcı başına bildirim gönderim sınırı | Bildirim | `08 §2.2.4` |
| T305 | F3 | E-posta ortak kalıbı ve gövde üretimi | Bildirim | `08 §2.2.5` |
| T306 | F3 | Müşteri bildirimleri B-1…B-8 | Bildirim | `08 §2.2.6` (yarısı) |
| T307 | F3 | Müşteri bildirimleri B-9…B-16 | Bildirim | `08 §2.2.6` (yarısı) |
| T308 | F3 | Firma bildirimleri F-1…F-6 | Bildirim | `08 §2.2.7` |
| T309 | F3 | Hesap ve yönetim e-postaları | Bildirim | `08 §2.2.8` |
| T310 | F3 | Kargo şirketi listesi ve takip bağlantısı kalıpları | Sepet ve sipariş | `08 §2.4` |
| T311 | F3 | Ödeme sağlayıcısının bildirim ucu: imza, alan eşlemesi ve durum makinesine aktarım | Ödeme | `07` UC-80 (1) |
| T312 | F3 | Dosya uçları: imzalı indirme ucu ve parçalı yükleme ucu | Katalog | `07` UC-37, UC-83 (2) · `07 §5.4` (1) |
| T313 | F3 | Site haritası ve robots.txt | Ortak altyapı | `07` UC-79 (1) |
| T314 | F3 | Durum ucu: uyarı koşulları ve 500 sayacı | Ortak altyapı | `07` UC-82 (1) |
| T315 | F3 | Durum makinesi çekirdeği: envanter, geçiş tablosu ve kimlikleri, durum makinesi olmayan varlıkların sınırı | Sepet ve sipariş | `03 §1.1`, §1.10 (2) |
| T316 | F3 | Sipariş durumu geçişleri: sevkiyat ekseni | Sepet ve sipariş | `03 §1.2` |
| T317 | F3 | Ödeme durumu geçişleri: ödeme ekseni | Ödeme | `03 §1.3` |
| T318 | F3 | İki eksenin bağı, tipe göre hatlar ve manuel adım bütçesi | Sepet ve sipariş | `03 §1.4`, §1.5, §8.5 (3) |
| T319 | F3 | Geri alınamaz geçişler, onay gerekleri ve durum düzeltmesi | Sepet ve sipariş | `03 §1.6` · `02 §10.4.6`, §10.4.8 (2) |
| T320 | F3 | Kalem kayıtları, ayıp talebi durumu, iade reddi ve panel işaretleri | İptal, cayma ve iade | `03 §1.7`, §1.9, §5.3 (3) |
| T321 | F3 | Ürün yayın durumları | Katalog | `03 §1.8` |
| T322 | F3 | İçerik yayın durumları ve iletişim talebi durumları | Kurumsal içerik | `03 §1.8`, §1.9 |
| T323 | F3 | Durum × rol × işlem tablosu | Sepet ve sipariş | `03 §1.11` |
| T324 | F3 | Zaman aşımı akışları: hesap, oturum, sepet ve ödeme süreleri | Ortak altyapı | `03 §4.1` (Z-1…Z-47'nin ilk iki grubu) |
| T325 | F3 | Zaman aşımı akışları: teslim, cayma, geri ödeme ve ayıp talebi süreleri | Ortak altyapı | `03 §4.1` (üçüncü ve dördüncü grup) |
| T326 | F3 | Zaman aşımı akışları: katalog, kupon, içerik, iletişim ve bildirim süreleri | Ortak altyapı | `03 §4.1` (beşinci ve altıncı grup; yedinci grup saklama ve imha T201, T301, T302'dedir) |
| T327 | F3 | Kendiliğinden işleyen anlar ve sitenin kesintisinde süreler | Sepet ve sipariş | `03 §4.2`, §10.3 (2) |
| T328 | F3 | Bildirim haritası: tetikleyici eşlemesi, bildirim üretmeyen olaylar ve eşitlik denetimi | Bildirim | `03 §7.1`–§7.3 (3) |
| T329 | F3 | İlk uçtan uca akış testi: ürün, sepet, sipariş, sahte ödeme sonucu, kargo, teslim (servis düzeyi) | Sepet ve sipariş | `03 §2.10` (K-1410) |
| T330 | F3 | Firma kimliği: tek firma, tip ve tipe göre zorunlu alanlar, yasal kimlik bilgilerinin erişilebilirliği, iade adresi | Ayarlar | `02 §3.1.1`–§3.1.4, §3.1.7 (5) |
| T331 | F3 | Satış kapısı: dört koşul, veri toplayan girişlerin kapısı, satışın geçici kapatılması | Ayarlar | `02 §3.1.5`, §3.1.6 (2) |
| T332 | F3 | Satış modeli, ürün tipleri, ürün–varyant–seçenek yapısı ve stok kodu | Katalog | `02 §3.2.1`–§3.3.6 (9) |
| T333 | F3 | Kategori ağacı, site içi arama ve liste süzgeçleri | Katalog | `02 §3.4.1`–§3.5.3 (9) |
| T334 | F3 | Stok ve hizmet kontenjanı: takip, tükenme, ayrılmış adedin altına inmeme | Katalog | `02 §3.6.1`–§3.6.7 (7) |
| T335 | F3 | Ürün yayın durumu: yayın kapısı, arşiv, silme ve engelleme kalıbı | Katalog | `02 §3.7.1`–§3.7.8 (8) |
| T336 | F3 | Fiyat ve KDV: kuruş hassasiyeti, kalem bazlı hesap sırası, yuvarlama yönü, fiyat etiketi bilgileri | Katalog | `02 §3.8.1`–§3.8.5 (5) |
| T337 | F3 | İndirim ve referans fiyat: tarihli indirim, fiyat geçmişinden referans, kaydı engelleyen koşullar | Katalog | `02 §3.9.1`–§3.9.6 (6) |
| T338 | F3 | Kupon: tanım, kalem payı, sınırlar, kullanım hakkının ayrılması | Katalog | `02 §3.10.1`–§3.10.7 (7) |
| T339 | F3 | Ürün görseli ve metni: ana görsel, kısıtlar, alternatif metin, metin biçimi seti | Katalog | `02 §3.11.1`–§3.11.7 (7) |
| T340 | F3 | Dijital ürün: dosya, indirme hakkı, hak yenileme, yeniden satın alma | Katalog | `02 §3.12.1`–§3.12.9 (9) |
| T341 | F3 | Üyelik: kayıt, e-posta doğrulama kapısı, misafir siparişinin hesaba bağlanması, hesap adı, giriş kaydı | Üyelik | `02 §3.13.1`–§3.13.5, §3.13.18–§3.13.21 (9) |
| T342 | F3 | Giriş ve oturum: oturum ömrü, şifre politikası, sıfırlama, yeniden doğrulama, yönetici hesaplarına aynı rejim, giriş limitleri | Üyelik | `02 §3.13.9`–§3.13.13, §3.13.15–§3.13.17 (8) |
| T343 | F3 | Google ile giriş ve hesap bağlama, e-posta değişikliği ve geri alma | Üyelik | `02 §3.13.6`–§3.13.8, §3.13.14 (4) |
| T344 | F3 | Adres: adres defteri, fatura adresi, kapalı il listesi, tipe göre istenen adresler, siparişe donma | Üyelik | `02 §3.14.1`–§3.14.6 (6) |
| T345 | F3 | Hesabın silinmesi ve kişisel veri başvurusu | Üyelik | `02 §3.15.1`–§3.15.6 (6) |
| T346 | F3 | Sepet: sahiplik, giriş anında birleşme, canlı kalem, siparişe girmeyen kalemler | Sepet ve sipariş | `02 §3.16.1`–§3.16.7 (7) |
| T347 | F3 | Sepet adet kuralları: stok ve sipariş başına sınır, boşalma, istek listesi yokluğu | Sepet ve sipariş | `02 §3.16.8`–§3.16.13 (6) |
| T348 | F3 | Sipariş onayı: oluşma anı, özet eşitliği, stok ayırma, ödeme süresi, kendiliğinden iptal | Sepet ve sipariş | `02 §3.17.1`–§3.17.8 (8) |
| T349 | F3 | Asgari sipariş tutarı, kargo ücreti ve ücretsiz kargo eşiği | Sepet ve sipariş | `02 §3.18.1`–§3.19.6 (9) |
| T350 | F3 | Teslim: il kısıtı, tek yol, kargoya verme süresi ve sözü, kargoya verildi işareti, kargo şirketi | Sepet ve sipariş | `02 §3.20.1`–§3.20.8 (8) |
| T351 | F3 | Teslim işaretleri: kalem tipine göre teslim, karışık sipariş, fiziksel teslim tarihi | Sepet ve sipariş | `02 §3.20.9`–§3.20.12 (4) |
| T352 | F3 | Ödeme: satıcı ve kart verisi, iki yöntem, 3D Secure, taksit, ters ibraz | Ödeme | `02 §3.21.1`–§3.21.4, §3.21.9–§3.21.11 (7) |
| T353 | F3 | Havale/EFT: IBAN ve siparişe donması, ödendi işareti, hatırlatma, süresi dolmuş sipariş | Ödeme | `02 §3.21.5`–§3.21.8 (4) |
| T354 | F3 | Sipariş numarası ve takip erişimi: numara, üç giriş yolu, erişim anahtarı | Sepet ve sipariş | `02 §3.22.1`–§3.22.5 (5) |
| T355 | F3 | Sipariş anında donan değerler | Sepet ve sipariş | `02 §3.23.1`–§3.23.5 (5) |
| T356 | F3 | Onay adımı: onay kutuları, Ön Bilgilendirme Formu içeriği, işlem rehberi | Sepet ve sipariş | `02 §3.24.1`–§3.24.8 (8) |
| T357 | F3 | Fatura verisi ve dışa aktarmada donmuş kimlik | Yönetim ve işlem izi | `02 §3.25.1`–§3.25.3 (3) |
| T358 | F3 | Kurumsal içerik tipleri: Hakkımızda, hizmet tanıtımı, referans iş, SSS, şube, genel sayfa, sosyal bağlantılar | Kurumsal içerik | `02 §3.27.1`–§3.27.9, §3.27.12 (10) |
| T359 | F3 | Kurumsal içerik: ürün bağı, elle sıralama, zorunlu alanlar, alan tavanları, kapsam dışı yetenekler | Kurumsal içerik | `02 §3.27.10`, §3.27.11, §3.27.13–§3.27.20 (10) |
| T360 | F3 | Duyuru şeridi, boş kurulum, içerik yayın durumu, önizleme ve silme | Kurumsal içerik | `02 §3.27.21`–§3.27.28 (8) |
| T361 | F3 | Ana sayfa düzeni ve üst menü | Kurumsal içerik | `02 §3.28.1`–§3.28.6 (6) |
| T362 | F3 | Marka kimliği: logo, marka adı, site simgesi, marka rengi, platform imzası | Kurumsal içerik | `02 §3.29.1`–§3.29.6 (6) |
| T363 | F3 | Sayfa adresleri, arama motoru bilgileri, kapalı sayfalar ve paylaşım | Kurumsal içerik | `02 §3.30.1`–§3.30.7 (7) |
| T364 | F3 | Panelde eşzamanlı düzenleme ve siparişte çakışan işlemler | Sepet ve sipariş | `02 §3.31.1`, §3.31.2 (2) |
| T365 | F3 | İletişim talebi: form, konu tipi, durumlar, saklama, istenmeyen ileti koruması | Kurumsal içerik | `02 §3.32.1`–§3.32.10 (10) |
| T366 | F3 | Yasal metinler: dört metin, sürüm artışı, aydınlatma bağlantıları, e-postada tam metin | Ayarlar | `02 §3.33.1`–§3.33.9 (9) |
| T367 | F3 | Müşteri iptali: kalem ve adet düzeyi, kargoya verilene kadar, dijital ve hizmet sınırı, sayaçların dönmesi, kısmi iptalde kupon | İptal, cayma ve iade | `02 §7.1.1`, §7.1.4, §7.2.1, §7.2.2, §7.2.5–§7.2.7 (7) |
| T368 | F3 | Gecikme feshi ve firma iptali: iki fesih yolu, kanuni faiz uyarısı, kapalı sebep listesi, ifanın imkânsızlaşması bildirimi | İptal, cayma ve iade | `02 §7.2.3`, §7.2.4 (2) |
| T369 | F3 | İşlemin adet birimi ve geri ödeme tutarı: açık adet, kupon payı, KDV, kargo ücretinin iptalde ve caymada ayrılması | İptal, cayma ve iade | `02 §7.1.3`, §7.1.6, §7.1.7, §7.2.9, §7.4.6 (5) |
| T370 | F3 | Geri ödeme hattı: ödemenin geldiği yol, kart ve havale, IBAN'ın ömrü, geri ödeme süresi, kart iadesi hataları | Ödeme | `02 §7.2.8`, §7.4.1, §7.4.5, §8.4.2 (4) |
| T371 | F3 | Cayma hakkı: pencere, dijital ve hizmet istisnası, ürün istisna işareti, tek rejim | İptal, cayma ve iade | `02 §7.1.2`, §7.3.1–§7.3.4 (5) |
| T372 | F3 | Cayma beyanı: sipariş sayfasından ve başka kanaldan, beyanın etkisi, kargoya verilmemiş kalemde iptal yolu, firmanın teyidi | İptal, cayma ve iade | `02 §7.3.5`–§7.3.7, §8.3.8 (4) · `02 §10.4.10` (1) |
| T373 | F3 | İade: iade kargo bedeli, ödemeli gönderim ve iade adresi, teslim alma adımı, stoğa ekleme, koşullu istisnada ret, kullanılmış mal | İptal, cayma ve iade | `02 §7.4.2`–§7.4.4, §7.4.7–§7.4.10 (7) · `02 §10.4.9` (1) |
| T374 | F3 | Ayıp talebi: kanal, iki yıllık süre, durumlar, çözüm, dijital dosya güncellemesi | İptal, cayma ve iade | `02 §7.5.1`–§7.5.4 (4) |
| T375 | F3 | Yöneticinin sipariş düzeltmeleri: ileri yönlü kayıt, tutar değişmez, firmanın teyidiyle e-posta ve adres düzeltmesi, cayma beyanı geri alınmaz | Sepet ve sipariş | `02 §7.6.6`, §7.6.7, §8.3.6, §8.3.7 (4) · `02 §10.4.1`, §10.4.2, §10.4.11 (3) |
| T376 | F3 | Deneme limitleri: dokuz limit, kayan pencere, hesap kilitlenmez, nötr mesaj | Ortak altyapı | `02 §8.2.1`, L-1…L-9 (10) |
| T377 | F3 | Limit ilkeleri: iki eksen, ödenmemiş sipariş sayımı, yönetici muafiyeti yok, tanınan tarayıcı işareti | Ortak altyapı | `02 §8.2.2`–§8.2.6 (5) |
| T378 | F3 | Zaman sayımı: tek saat dilimi, iş günü ve resmî tatil listesi, takvim ve iş günü sayımı, gün sonu, tarih aralığı, çitler | Ortak altyapı | `02 §4.1.1`–§4.1.5 (5) |
| T379 | F3 | Yönetici hesapları: ilk yönetici ve davet, kaldırma kuralları, tek rol, ayrı hesap türü, davetin geri çekilmesi, erişim kaybında kurtarma sınırı | Yönetim ve işlem izi | `02 §10.2.1`–§10.2.7 (7) |
| T380 | F3 | İşlem izi: sekiz alan, kim-ne zaman-ne değişti, değiştirilemez ve silinemez, sistem olayları ize girmez, düz liste, on yıl | Yönetim ve işlem izi | `02 §10.3.1`–§10.3.5 (5) |
| T381 | F3 | Ayar katmanları ve parametre envanteri: firma ayarı, kurulum ayarı, ürün sabiti, varsayılan ve çit, değişikliğin ize yazılması | Ayarlar | `02 §10.1.3` (1) · `02 §11.1`, §11.2 P-1…P-49 (47) |
| T382 | F3 | Panel ana sayfası verisi: altı bekleyen iş sayacı, kanal ve firma bildirimi uyarıları, kurulum kontrol listesi | Yönetim ve işlem izi | `02 §10.6.1`, §10.8.2 (2) |
| T383 | F3 | Satış özeti ve ölçüler: dönem seçimi, ciro, sayılar, dört mağaza ölçüsü, sıfır payda, tepe gün sayısı | Yönetim ve işlem izi | `02 §10.6.2`, §10.6.3, H-2 (3) |
| T384 | F3 | Dışa aktarma: sipariş CSV (fatura verisi), üye ve iletişim talebi listesi, işlem izine yazım, imha edilmiş siparişin boş sütunları | Yönetim ve işlem izi | `02 §10.7.1`–§10.7.4 (4) |
| T385 | F3 | Gönderim yeniden denemesi ve "e-posta ulaşmadı" işareti: üç deneme, işaretin düşüşü, panelden yeniden gönderim, işaretsiz bildirimler | Bildirim | `02 §9.1.6` (1) · P-38 |
| T386 | F3 | Sipariş müdahaleleri: kalem adedi azaltma ve çıkarma, kargo bilgisinin ve teslim tarihinin düzeltilmesi, iç not | Sepet ve sipariş | `02 §10.4.3`–§10.4.5, §10.4.7 (4) |
| T387 | F3 | Kurulum tarafı yönetim komutları: iş satırını yeniden kuyruğa alma ya da kapatma, anahtar yenilemesi sonrası ödeme sonucunun uygulanması, kayıt okuma komutu | Yönetim ve işlem izi | `05 §8.5`, §9.3, §6.9 (MK-66, MK-72) |
| T388 | F3 | Dönüş denetimi komutu ve dönüşten sonra silmelerin yeniden uygulanması | Yönetim ve işlem izi | `05 §8.5`, §8.7 (MK-72, MK-74) |
| T401 | F4 | PayTR: anahtarlar, ödemenin başlatılması, dönüş ve çerçeve | Ödeme | `08 §2.1`, §2.1.1, §2.1.2, §2.1.4 |
| T402 | F4 | PayTR: bildirim imzası ve alan eşlemesi | Ödeme | `08 §2.1.5` |
| T403 | F4 | PayTR: son sorgu ve tek tahsilat | Ödeme | `08 §2.1.6`, §2.1.7 |
| T404 | F4 | PayTR: bağlantı hatası ve ayrım | Ödeme | `08 §2.1.8` |
| T405 | F4 | PayTR: kart iadesi | Ödeme | `08 §2.1.9` |
| T406 | F4 | PayTR: 3D Secure kurulum şartı, test ve canlı kanıt | Ödeme | `08 §2.1.3`, §2.1.10 |
| T407 | F4 | SES: gönderim ve sonuç sınıfları | Bildirim | `08 §2.2`, §2.2.1, §2.2.2 |
| T408 | F4 | SES: geri dönen adreslerin öğrenilmesi | Bildirim | `08 §2.2.3` |
| T409 | F4 | Google: giriş, kayıt ve yeniden doğrulama bağlayıcısı | Üyelik | `08 §2.3` |
| T410 | F4 | Dış olgu varsayımlarının doğrulama kanıtları (DV) | Ortak | `08 §4.1`, §5.2 |
| T501 | F5 | Yerli üretim logosu varlığı ve gösterimi | Katalog | `08 §4.3` |
| T502 | F5 | Vitrin: ana sayfa ve kategori sayfası (E-01, E-02) | Katalog | `07` UC-01, UC-02 (2) |
| T503 | F5 | Vitrin: arama sonuçları, ürün sayfası ve sepete ekleme (E-03, E-04; arşivdeki E-05 aynı ucun hâlidir) | Katalog | `07` UC-03…UC-05 (3) |
| T504 | F5 | Hata sayfaları: bulunamayan adres ve beklenmeyen hata (E-06, 500) | Ortak altyapı | `07` UC-06, UC-78 (2) |
| T505 | F5 | İçerik liste ve içerik sayfaları (E-07, E-08) | Kurumsal içerik | `07` UC-07…UC-12 (6) |
| T506 | F5 | Sık sorulan sorular ve iletişim sayfası ve formu (E-09, E-10) | Kurumsal içerik | `07` UC-13…UC-15 (3) |
| T507 | F5 | Yasal metin sayfaları ve panelde yönetimi (E-11, E-47) | Ayarlar | `07` UC-16…UC-18, UC-102, UC-173, UC-174 (6) |
| T508 | F5 | Sepet (E-12) | Sepet ve sipariş | `07` UC-19…UC-21 (3) |
| T509 | F5 | Ödeme adımı (E-13) | Sepet ve sipariş | `07` UC-22…UC-27 (6) |
| T510 | F5 | Sipariş teyit ve bekleme ekranı, ödeme sağlayıcısına geçiş ve dönüş (E-14) | Ödeme | `07` UC-28…UC-30 (3) |
| T511 | F5 | Sipariş takibi girişi, sipariş sayfası ve indirme bağlantısı (E-15, E-16) | Sepet ve sipariş | `07` UC-31…UC-34, UC-36 (5) |
| T512 | F5 | İptal ve gecikme feshi ekranı, geri ödeme IBAN girişi (E-17, E-16 IBAN formu) | İptal, cayma ve iade | `07` UC-35, UC-38…UC-41 (5) |
| T513 | F5 | Cayma beyanı ve ayıp talebi ekranları (E-18, E-19) | İptal, cayma ve iade | `07` UC-42…UC-46 (5) |
| T514 | F5 | Kayıt ve doğrulama bağlantısının inişi (E-20, E-21) | Üyelik | `07` UC-47…UC-49, UC-52, UC-53 (5) |
| T515 | F5 | Müşteri girişi ve Google ile giriş (E-22) | Üyelik | `07` UC-50, UC-51, UC-54, UC-55 (4) |
| T516 | F5 | Şifre sıfırlama ve yeniden doğrulama (E-23, E-24) | Üyelik | `07` UC-56…UC-62 (7) |
| T517 | F5 | Hesap — profil ve güvenlik (E-25) | Üyelik | `07` UC-63…UC-69 (7) |
| T518 | F5 | Adres defteri ve sipariş geçmişi (E-26, E-27) | Üyelik | `07` UC-70…UC-74 (5) |
| T519 | F5 | E-posta değişikliğini geri alma ve müşteri çıkışı (E-28) | Üyelik | `07` UC-75…UC-77 (3) |
| T520 | F5 | Panel girişi, şifre sıfırlama, davet kabulü ve yönetici çıkışı (E-29, E-30) | Yönetim ve işlem izi | `07` UC-84, UC-85, UC-109…UC-113 (7) |
| T521 | F5 | Panel ana sayfası ve yönetici hesabının doğrulama ekranları (E-31, E-54) | Yönetim ve işlem izi | `07` UC-86, UC-108, UC-185…UC-187 (5) |
| T522 | F5 | Yönetici hesapları ve işlem izi (E-49, E-52) | Yönetim ve işlem izi | `07` UC-103, UC-106, UC-175…UC-177 (5) |
| T523 | F5 | Yöneticinin kendi hesabı (E-50) | Yönetim ve işlem izi | `07` UC-104, UC-178…UC-181 (5) |
| T524 | F5 | Satış özeti ve dışa aktarma (E-51, E-53) | Yönetim ve işlem izi | `07` UC-105, UC-107, UC-182…UC-184 (5) |
| T525 | F5 | Ürün listesi ve ürün formu, görsel yükleme (E-32, E-33) | Katalog | `07` UC-87, UC-88, UC-114…UC-117 (6) |
| T526 | F5 | Kategori ağacı ve kuponlar (E-34, E-35) | Katalog | `07` UC-89, UC-90, UC-118…UC-122 (7) |
| T527 | F5 | Sipariş listesi, sipariş ayrıntısı ve durum geçişleri (E-36, E-37) | Sepet ve sipariş | `07` UC-91, UC-92, UC-123…UC-128 (8) |
| T528 | F5 | Sipariş ayrıntısı: firma iptali, bildirim kaydı, kalem çıkarma, iade malı ve ayıp talebi (E-37) | İptal, cayma ve iade | `07` UC-129, UC-130, UC-138, UC-140…UC-142, UC-144, UC-145 (8) |
| T529 | F5 | Sipariş ayrıntısı: geri ödeme işlemleri (E-37) | Ödeme | `07` UC-131…UC-137 (7) |
| T530 | F5 | Sipariş ayrıntısı: adres, kargo, teslim tarihi, durum geçişi ve e-posta düzeltmeleri (E-37) | Sepet ve sipariş | `07` UC-146…UC-151 (6) |
| T531 | F5 | Sipariş ayrıntısı: hizmet kalemi, indirme hakkı, iç not ve e-posta yeniden gönderim (E-37) | Sepet ve sipariş | `07` UC-139, UC-143, UC-152…UC-154 (5) |
| T532 | F5 | Talepler ve iletişim talebi ayrıntısı (E-38, E-39) | Kurumsal içerik | `07` UC-93, UC-94, UC-155, UC-156 (4) |
| T533 | F5 | Üye kaydı görünümü (E-40) | Üyelik | `07` UC-95, UC-157 (2) |
| T534 | F5 | Kurumsal içerik ve marka yönetimi (E-41, E-43) | Kurumsal içerik | `07` UC-96, UC-98, UC-158…UC-161, UC-163 (7) |
| T535 | F5 | Ana sayfa ve menü, firma kimliği ve satış (E-42, E-44) | Ayarlar | `07` UC-97, UC-99, UC-162, UC-164…UC-166 (6) |
| T536 | F5 | Ödeme yöntemleri, kargo, süreler ve varsayılanlar (E-45, E-46) | Ayarlar | `07` UC-100, UC-101, UC-167…UC-172 (8) |
| T537 | F5 | Vitrin çerçevesinin modeli: marka, menü, duyuru şeridi, hesap alanı, sepet sayısı | Ortak altyapı | `07 §5.5` (1) |
| T538 | F5 | Uç ve model eşitlik denetimleri: uç envanteri, form girdi ve sorgu modelleri | Ortak altyapı | `07 §5.2`, `§5.3` (2) |
| T539 | F5 | Betiksiz yol: JavaScript kapalıyken temel akışlar ve betikli iyileştirme | Ortak altyapı | `07 §8.2` (1) |
| T540 | F5 | Tasarım tabanı, vitrin ve panel çerçevesi, yönetici şeridi ve Taslak bandı (OB-01, OB-02, OB-09) | Ortak altyapı | `04 §2.1`, §2.2, §2.9, §3.1, §3.2, §8.3 (3 OB + 3 alt bölüm) |
| T541 | F5 | Mesaj, onay, boş ve hata hâlleri ve satış kapalı hâli (OB-03, OB-04, OB-07, OB-08) | Ortak altyapı | `04 §2.3`, §2.4, §2.7, §2.8 (4 OB) |
| T542 | F5 | Rozet ve işaretler, sayaç ve süre göstergeleri, eşzamanlı düzenleme uyarısı (OB-05, OB-06, OB-10) | Ortak altyapı | `04 §2.5`, §2.6, §2.10 (3 OB) |
| T543 | F5 | Form alanı ve düzeni, adres formu, IBAN alanı, sebep seçimi ve ortak alan kuralları (OB-12…OB-15) | Ortak altyapı | `04 §2.12.1`–§2.12.4, §7.3 (4 OB + 1) |
| T544 | F5 | Tarih alanı, kupon alanı, onay kutuları, görsel ve dosya yükleme arayüzü (OB-16…OB-19) | Ortak altyapı | `04 §2.12.5`–§2.12.8 (4 OB) |
| T545 | F5 | Liste ve tablo, ürün kartı (OB-20, OB-11) | Ortak altyapı | `04 §2.11`, §2.13 (2 OB) |
| T546 | F5 | Gezinme ve geçiş eşitlik denetimleri: iskeletler, geçişler, dışarıdan girişler, dönüşler | Ortak altyapı | `04 §3.3`–§3.7 (5) |
| T547 | F5 | Durum × rol matrisi ve form envanteri eşitlik denetimleri | Ortak altyapı | `04 §6.1`–§6.3, §7.1, §7.2 (5) |
| T548 | F5 | Parçalı yükleme betiği: başlatma–parça–durum akışı, 409/404/400/413 yanıtları, ağ hatasında üç kez sürdürme | Ortak altyapı | `09 §16.2`; `07 §4.31.2`; K-1354 |
| T601 | F6 | `DEPLOY_RUNBOOK` provası, dış sistemde kayıtlı adresler ve yansıyan tablolar | Ortak | `06 §8.5`, `08 §4.2` |
| T602 | F6 | Güncelleme komutu: referans kurulumdan başlayıp sırayla, geride kalmış kurulumun ara sürümlerden geçişi, geri alma ve yedekten dönüş koşulu | Ortak | `05 §8.6`, §8.7 (MK-73, MK-74) |

**Kapsama komutları (Git Bash; depo kökünden).** Sayımlar aşağıdaki sayılarla aynı çıkmalı, her iki betik de çıkış kodu 0 vermelidir (eksik, fazla, yinelenen 0).

```sh
python -I scripts/doc-scans/11_matris_envanter.py . /tmp/envanter.txt    # kaynak envanteri: 06 117, 07 208, 08 30 (ve öteki aileler)
python -I scripts/doc-scans/11_matris_kapsama.py . /tmp/kapsama.txt      # matris satırları ↔ beklenen küme (dolu aileler: 06, 07, 08, 04, 03, 02 §1–§13, 05, 09, 10, devir — 1.501/1.501)
```

**Okuma derinliği (O1).** `06`: §3.1 entity tablosu bütün, §2/§5–§9 alt bölüm başlıkları ve ilk paragrafları, §8 bütün, §7.2'nin 130 bağı betikle; §4 entity tanımları okunmadı — kaynak birim §3.1'in satırıdır. `08`: §1 bütün, §2–§5 başlıkları ve her başlığın ilk iki paragrafı (satır başına ilk 300–380 karakter; kesilerek okundu).

**Okuma derinliği (O3).** `04`: §4 ekran envanteri (53 satır) ve §4.3, §4.4 bütün; §2'nin bileşen özet tablosu (20 satır, her hücre) ve alt bölüm başlıkları; §3, §6, §7, §8 alt bölüm başlıkları ve ilk paragrafları. Okunmadı: §5 ve §9 ekran tanımları, §2 bileşen tanımlarının gövdesi, §6.2–§6.3 matris satırları, §7.1–§7.3 alan tabloları. `03`: bütün `###` başlıkları (55); §2.10, §4, §5, §6, §7, §8.2, §8.5, §9.5, §10 bölümlerinin girişi ve ilk tablo satırları (satır başına ilk 250–420 karakter); §1, §2.1–§2.9, §3, §8.1, §8.3, §8.4, §8.6–§8.9, §9.1–§9.4 yalnız başlık ve ilk paragraf. Eşleme ekran adı ve `07`'nin F5 gruplarından türer; gövdesi okunmayan alt bölümlerde (özellikle `03 §3.5`, §6.2) task listesi sonraki oturumda örneklemle doğrulanır.

**Okuma derinliği (O4a).** `02 §3`'ün 257 kuralı ve 34 alt bölümü bütün okundu — her kural satırı tamamıyla (§3.1–§3.34, satır 497–1090); §2 (§2.1 envanter tablosu, §2.2 ve §2.3 bütün) ve §1.1, §1.3 bütün okundu. **Okunmadı:** §1.2 terim sözlüğünün tablo satırları (birim değildir; alt bölüm satırı karşılar), her bölümün sonundaki `Kaynak:` satırları (karar atıfları; birim değildir). Okunan kuralların hiçbirinde kesme yapılmadı.

**Okuma derinliği (O4b 1/2).** `02 §4`–§8 (satır 1094–1802): her kural satırı, Z ve L satırı ve `###` başlığı tamamıyla okundu; satırların uzun olanları ilk 430–700 karakter ve kalan parçalar ayrı ikinci okumayla (kesilmiş satır kalmadı). Okunmadı: her bölümün sonundaki `Kaynak:` satırlarının K-no sütunu (karar atıfları; birim değildir) ve tablo satırlarındaki K-no sütunu. `§6`'nın `6.2.n`…`6.9.n` satırları (birim değildir) alt bölüm satırının task birleşimi için okundu. §9–§13 bu oturumda okunmadı.

**Okuma derinliği (O4b 2/2).** `02 §9`–§13 (satır 1803–2241): her kural satırı, B ve F satırı, P ve H satırı ve `###` başlığı tamamıyla okundu (uzun satırlar kesilmeden, `§10.4` ve `§12.2.7` dahil); `§12.5` tablosunun 32 satırı bütün okundu; `§13` bütün. Okunmadı: bölüm sonlarındaki `Kaynak:` satırlarının K-no listeleri (karar atfı; birim değildir) ve dosya sonundaki sürüm geçmişi. Eşleme `02` metninden elle yapıldı; betikle üretilen yalnız satır metni, başlık, sayım ve `§11` P satırlarının "Nerede kullanılır" atıflarından çıkan task adayıdır. Örneklem: `§5` yazım oturumu her F3 task'ının kural kümesini kaynak satırlarıyla yeniden okur, ayrışma %10'u aşarsa genişletir (K-1424 (4)'ün kalıbı).

**Okuma derinliği (O5 1/2).** `05 §12` (satır 1440–1529): 78 MK satırının her hücresi (karar, gerekçe, geri alınabilirlik) tamamıyla, kesme yapılmadan; `###` alt bölümlerinin 58 başlığı; `§11.1`, `§11.2` ve `§11.3`'ün girişi. Okunmadı: `§3`–§11 alt bölüm gövdeleri, `§0`–§2 gövdeleri, `§12`'nin `Kaynak:` satırı ve dosya sonundaki sürüm geçmişi (alt bölüm birimi başlık satırıdır; task birleşimi MK satırlarının `(§n.m)` atıflarından kuruldu). Gövde okuması `§5` yazım oturumuna kalır (K-1433).

**Okuma derinliği (O5 2/2).** `10 §2` (satır 135–233): 77 KP satırının her hücresi (özellik, neden, kaynak) tamamıyla, kesme yapılmadan; `§3` (satır 237–324): 64 KD satırının özellik ve kaynak hücresi, uzun gerekçe hücrelerinin ilk 470 ve son 140 karakteri (620 karakteri aşan satırlarda) ve girişin biçim paragrafları; `§4.1` ve `§4.2` başlıkları, girişleri ve ÖK/SK satırlarının ilk 330 karakteri. Okunmadı: KD gerekçe hücrelerinin ortası, `§4.1`/`§4.2` satırlarının sonları ve `Kaynak:` satırları, `§1`, `§5`. `09`: 25 bölümün başlığı ve `§1`, `§2`, `§17`, `§20`, `§22`, `§24`, `§25`'in girişi ve ilk maddeleri (ilk 340 karakter); öteki bölümler başlık düzeyinde (alt bölüm başlık listesi) ve `09` başlık notunun okuma tablosuna göre eşlendi — gövde okunmadı (465 KB). Devir: `11` park bloğu (P8.1–P8.5) tam, `DEFERRED_BACKLOG` D-01 satırı tam, `04` 1948 (GA-7) karar kaydı §22.1'in ★ paragrafı üzerinden (K-1403). Gövde okuması `§5` yazım oturumuna kalır (K-1436).

#### 7.1.1 Veri modeli → task

| Kaynak öğe | Task | Implement edildi | Kanıt |
|---|---|---|---|
| 06:§2.1 Şablonun altı dersi ve bu projedeki yeri | T101 | | |
| 06:§2.2 Silme kategorileri ve bağların silme davranışı | T101 | | |
| 06:§2.3 Anahtarlar ve dışa görünen kimlikler | T101 | | |
| 06:§2.4 Adlandırma ve şema | T101 | | |
| 06:§2.5 Türler ve sayısal ölçek | T101 | | |
| 06:§2.6 Eşzamanlılık — sürüm değeri ve koşullu güncelleme | T101 | | |
| 06:§2.7 Denormalizasyon envanteri | T120 | | |
| 06:§2.8 Metin karşılaştırma, sıralama, oluşturulma anı ve boş değer | T101 | | |
| 06:§2.9 Enum'ların biçimi | T101 | | |
| 06:§2.10 Uzun metnin saklanışı | T101 | | |
| 06:§4.1 Company | T103 | | |
| 06:§4.2 CompanySettings | T103 | | |
| 06:§4.2 CompanyDeliveryProvince | T103 | | |
| 06:§4.3 Province | T103 | | |
| 06:§4.4 Country | T103 | | |
| 06:§4.5 Product | T104 | | |
| 06:§4.6 Variant | T104 | | |
| 06:§4.7 Option | T104 | | |
| 06:§4.8 OptionValue | T104 | | |
| 06:§4.9 Category | T104 | | |
| 06:§4.10 ProductCategory | T104 | | |
| 06:§4.11 PriceHistory | T105 | | |
| 06:§4.12 Discount | T105 | | |
| 06:§4.13 Coupon | T105 | | |
| 06:§4.14 StockReservation | T110 | | |
| 06:§4.15 Cart | T109 | | |
| 06:§4.16 CartItem | T109 | | |
| 06:§4.17 Order | T109 | | |
| 06:§4.18 OrderItem | T109 | | |
| 06:§4.19 TransitionRecord | T109 | | |
| 06:§4.20 InternalNote | T109 | | |
| 06:§4.21 CheckboxAcceptance | T109 | | |
| 06:§4.21 CheckboxAcceptanceOrderItem | T109 | | |
| 06:§4.22 Cancellation | T111 | | |
| 06:§4.23 ItemRemoval | T111 | | |
| 06:§4.24 LateDeliveryTermination | T111 | | |
| 06:§4.25 Withdrawal | T111 | | |
| 06:§4.26 Return | T111 | | |
| 06:§4.27 ReturnRejection | T113 | | |
| 06:§4.28 NotReturnedClosure | T111 | | |
| 06:§4.29 RefundIbanRequest | T113 | | |
| 06:§4.30 DefectClaim | T111 | | |
| 06:§4.31 RefundIban | T111 | | |
| 06:§4.32 PaymentRecord | T112 | | |
| 06:§4.33 Refund | T112 | | |
| 06:§4.34 RefundRecord | T112 | | |
| 06:§4.35 InboundNotificationRecord | T112 | | |
| 06:§4.36 Customer | T107 | | |
| 06:§4.37 Admin | T106 | | |
| 06:§4.38 AdminInvitation | T106 | | |
| 06:§4.39 Session | T108 | | |
| 06:§4.40 PendingEmailChange | T107 | | |
| 06:§4.41 RecognizedBrowserMark | T108 | | |
| 06:§4.42 LinkRecord | T108 | | |
| 06:§4.43 AttemptRecord | T108 | | |
| 06:§4.44 LoginRecord | T108 | | |
| 06:§4.45 AddressBook | T107 | | |
| 06:§4.46 AboutPage | T114 | | |
| 06:§4.47 Offering | T114 | | |
| 06:§4.48 PortfolioItem | T114 | | |
| 06:§4.49 FaqItem | T114 | | |
| 06:§4.50 Branch | T115 | | |
| 06:§4.51 CustomPage | T114 | | |
| 06:§4.52 Announcement | T114 | | |
| 06:§4.53 OfferingProduct | T115 | | |
| 06:§4.53 PortfolioItemProduct | T115 | | |
| 06:§4.54 Branding | T114 | | |
| 06:§4.55 SocialLink | T114 | | |
| 06:§4.56 LegalTextVersion | T103 | | |
| 06:§4.57 ContactRequest | T115 | | |
| 06:§4.58 DispatchRecord | T117 | | |
| 06:§4.59 Job | T118 | | |
| 06:§4.60 ChannelStateRecord | T117 | | |
| 06:§4.61 OrderEmailHistory | T110 | | |
| 06:§4.62 OrderAddressHistory | T110 | | |
| 06:§4.63 AuditLog | T106 | | |
| 06:§4.64 DisposalRecord | T118 | | |
| 06:§4.65 HeartbeatRecord | T118 | | |
| 06:§4.66 OutageRecord | T118 | | |
| 06:§4.67 BackupStatusRecord | T118 | | |
| 06:§4.68 DataProtectionKey | T118 | | |
| 06:§4.69 Image | T104 | | |
| 06:§4.70 DigitalFile | T105 | | |
| 06:§4.71 ProductImage | T104 | | |
| 06:§4.71 AboutPageImage | T116 | | |
| 06:§4.71 OfferingImage | T116 | | |
| 06:§4.71 PortfolioItemImage | T116 | | |
| 06:§4.71 FaqItemImage | T116 | | |
| 06:§4.71 CustomPageImage | T116 | | |
| 06:§5.1 Biçim, kaynak ve okunuş | T101 | | |
| 06:§5.2 Durum setleri ve geçiş kimlikleri | T102 | | |
| 06:§5.3 Sözlüğün listeleri | T102 | | |
| 06:§5.4 Teknik kayıtların listeleri | T102 | | |
| 06:§5.5 Kimlikli ürün listeleri | T102 | | |
| 06:§5.6 Kayıt düzeyindeki kapalı değerler | T102 | | |
| 06:§6.1 Okunuş | T120 | | |
| 06:§6.2 Firma, ayarlar ve katalog | T120 | | |
| 06:§6.3 Sipariş, kalem kayıtları ve ödeme | T120 | | |
| 06:§6.4 Hesaplar ve oturum | T120 | | |
| 06:§6.5 İçerik, yasal metinler ve dosyalar | T120 | | |
| 06:§6.6 Bağsız sistem kayıtları | T120 | | |
| 06:§7.1 Tekillikler | T120 | | |
| 06:§7.2 Yabancı anahtarlar ve silme davranışları | T120 | | |
| 06:§7.3 Kontrol kısıtları | T120 | | |
| 06:§7.4 Koşullu güncellemelerin dayandığı kısıtlar | T120 | | |
| 06:§7.5 Sorgu dizinleri | T120 | | |
| 06:§7.6 Hacim kabulleri | T120 | | |
| 06:§8.1 Seed | T119 | | |
| 06:§8.2 Fail-fast zorunlu | T205 | | |
| 06:§8.3 Kuruluma özgü | T303 | | |
| 06:§8.4 Veritabanında olmayanlar | T119 | | |
| 06:§8.5 `DEPLOY_RUNBOOK`'a yansıma | T601 | | |
| 06:§9.1 Sürelerin başlangıç değerleri | T201 | | |
| 06:§9.2 Siparişin imha parçası — alan listesi | T301 | | |
| 06:§9.3 Öteki kayıtların süreyle imhası | T302 | | |
| 06:§9.4 Anlık silmeler ve gecelik temizlik | T201 | | |
| 06:§9.5 Arşiv tablosu yoktur; yedeklerdeki kopyalar | T201 | | |

#### 7.1.2 API → task

> **Eşleme kuralı (K-1415, K-1416).** Sayfa ve form ucu (181 uç) kendi ekranının F5 task'ına girer: en çok 2 ekran ve 8 uç, tek modül (K-1409); sipariş ayrıntısı (E-37) uç sınırı ve modül ölçütü yüzünden beş task'a bölünür (T527–T531). Sayfasız uçlar F3'tedir (T311–T314), canlılık ucu F0'dadır (T01; K-1407 F0 çıkışı). `§2`, `§5` ve `§6`'nın ortak kalıpları F2'dedir (T206–T217); `§2.10` T206'ya, `§5.2`–`§5.3` eşitlik denetimi T538'e girer. İş mantığı bu satırlarda değil `02` satırlarındadır (O4a, O4b).

**Okuma derinliği (O2).** `07 §3.2`: 187 uç satırı bütün (her hücre). `§3.3`, `§3.4` ve `§4`'ün girişi bütün. `§4.33`, `§4.34` bütün. `§2`, `§5`, `§6`, `§8`: alt bölüm başlıkları, ilk paragraflar ve kalın maddelerin başlıkları (kesilerek okundu); `§2.10` bütün. **Okunmadı:** `§4` ve `§9`'un uç tanımları (`§4.33` ve `§4.34` dışında), `§5.2`–`§5.5` ve `§6.2`–`§6.3` tablolarının satırları — kaynak birim `§3.2`'nin satırı ve alt bölümün başlığıdır; sonraki oturum örneklemle doğrular.

| Kaynak öğe | Task | Implement edildi | Kanıt |
|---|---|---|---|
| 07:§2.1 Adres yapısı ve önekler | T206 | | |
| 07:§2.2 Sayfa ve form ucunun biçimi | T206 | | |
| 07:§2.3 Kimlik doğrulama ve erişim reddi | T207 | | |
| 07:§2.4 Sürümün ve gösterilen değerin formda taşınması | T208 | | |
| 07:§2.5 Tek kullanımlık değerler, tekrar gönderim ve sahteciliğe karşı koruma | T209 | | |
| 07:§2.6 Listeler | T210 | | |
| 07:§2.7 Hata gösterimi ve HTTP durumları | T212 | | |
| 07:§2.8 Hız sınırı ve gövde tavanı | T215 | | |
| 07:§2.9 Tarih, sayı ve para biçimi | T216 | | |
| 07:§2.10 Sürümleme | T206 | | |
| 07 UC-01 GET `/` — Ana sayfa | T502 | | |
| 07 UC-02 GET `/kategori/{Slug}` — Kategori sayfası | T502 | | |
| 07 UC-03 GET `/arama` — Arama sonuçları | T503 | | |
| 07 UC-04 GET `/urun/{Slug}` — Ürün adresi | T503 | | |
| 07 UC-05 POST `/urun/{Slug}?handler=AddCartItem` — Sepete ekleme | T503 | | |
| 07 UC-06 — her isteğin yanıtı (404; §2.7.1) — Bulunamayan adres | T504 | | |
| 07 UC-07 GET `/hizmetler` — Hizmetlerimiz listesi | T505 | | |
| 07 UC-08 GET `/referanslar` — Referanslarımız listesi | T505 | | |
| 07 UC-09 GET `/hakkimizda` — Hakkımızda | T505 | | |
| 07 UC-10 GET `/hizmet/{Slug}` — Hizmet tanıtımı sayfası | T505 | | |
| 07 UC-11 GET `/referans/{Slug}` — Referans iş sayfası | T505 | | |
| 07 UC-12 GET `/sayfa/{Slug}` — Genel sayfa | T505 | | |
| 07 UC-13 GET `/sss` — Sık sorulan sorular | T506 | | |
| 07 UC-14 GET `/iletisim` — İletişim sayfası | T506 | | |
| 07 UC-15 POST `/iletisim?handler=SendContactRequest` — İletişim formunun gönderimi | T506 | | |
| 07 UC-16 GET `/aydinlatma-metni` — Aydınlatma metni | T507 | | |
| 07 UC-17 GET `/cerez-politikasi` — Çerez politikası | T507 | | |
| 07 UC-18 GET `/islem-rehberi` — İşlem rehberi | T507 | | |
| 07 UC-19 GET `/sepet` — Sepet | T508 | | |
| 07 UC-20 POST `/sepet?handler=UpdateCartItem` — Sepet kaleminin adedini değiştirme | T508 | | |
| 07 UC-21 POST `/sepet?handler=RemoveCartItem` — Sepet kalemini çıkarma | T508 | | |
| 07 UC-22 GET `/odeme` — Ödeme adımı | T509 | | |
| 07 UC-23 POST `/odeme?handler=ApplyCoupon` — Kuponu uygulama ya da değiştirme | T509 | | |
| 07 UC-24 POST `/odeme?handler=RemoveCoupon` — Kuponu kaldırma | T509 | | |
| 07 UC-25 POST `/odeme?handler=Recalculate` — Ödeme adımının ara gönderimi | T509 | | |
| 07 UC-26 POST `/odeme?handler=Leave` — Ödeme adımından girişe ya da sepete | T509 | | |
| 07 UC-27 POST `/odeme?handler=PlaceOrder` — Siparişi onaylama | T509 | | |
| 07 UC-28 GET `{kök}/teyit` — Sipariş teyit ve bekleme ekranı | T510 | | |
| 07 UC-29 POST `{kök}/teyit?handler=StartPayment` — Ödeme sağlayıcısına geçiş | T510 | | |
| 07 UC-30 GET `/odeme/donus/{erişim anahtarı}` — Ödeme sağlayıcısından dönüş | T510 | | |
| 07 UC-31 GET `/siparis-takibi` — Sipariş takibi girişi | T511 | | |
| 07 UC-32 POST `/siparis-takibi?handler=FindOrder` — Sipariş takibi sorgusu | T511 | | |
| 07 UC-33 GET `/siparis/{sipariş numarası}` — Sipariş sayfası | T511 | | |
| 07 UC-34 GET `/siparis/erisim/{erişim anahtarı}` — Erişim anahtarlı sipariş adresi | T511 | | |
| 07 UC-35 POST `{kök}?handler=SaveRefundIban` — Geri ödeme IBAN'ının girişi ve düzeltmesi | T512 | | |
| 07 UC-36 POST `{kök}?handler=DownloadDigitalFile` — İndirme bağlantısının üretimi | T511 | | |
| 07 UC-37 GET `/indir/{imzalı değer}` — İndirme ucu | T312 | | |
| 07 UC-38 GET `{kök}/iptal` ve 1 adres daha — İptal ve gecikme feshi ekranı | T512 | | |
| 07 UC-39 POST `{kök}/iptal?handler=CancelOrder` — Siparişin iptali | T512 | | |
| 07 UC-40 POST `{kök}/iptal?handler=CancelOrderItems` — Kalem iptali | T512 | | |
| 07 UC-41 POST `{kök}/gecikme-feshi?handler=SubmitLateDeliveryTermination` — Gecikme feshi | T512 | | |
| 07 UC-42 GET `{kök}/cayma` — Cayma beyanı ekranı | T513 | | |
| 07 UC-43 POST `{kök}/cayma?handler=SubmitWithdrawal` — Cayma beyanı | T513 | | |
| 07 UC-44 GET `{kök}/ayip-talebi` — Ayıp talebi ekranı | T513 | | |
| 07 UC-45 POST `{kök}/ayip-talebi?handler=SubmitDefectClaim` — Ayıp talebinin gönderimi | T513 | | |
| 07 UC-46 POST `{kök}/ayip-talebi?handler=ReopenDefectClaim` — Ayıp talebinin yeniden açılması | T513 | | |
| 07 UC-47 GET `/kayit` — Kayıt ekranı | T514 | | |
| 07 UC-48 POST `/kayit?handler=Register` — Kayıt | T514 | | |
| 07 UC-49 POST `/kayit?handler=ResendVerificationLink` ve 1 adres daha — Kayıt doğrulama bağlantısını yeniden isteme | T514 | | |
| 07 UC-50 POST `/kayit?handler=StartSocialLogin` ve 1 adres daha — Google ile giriş | T515 | | |
| 07 UC-51 GET `/giris/google` — Google dönüşü | T515 | | |
| 07 UC-52 GET `/dogrulama/{anahtar}` — Doğrulama bağlantısının inişi | T514 | | |
| 07 UC-53 POST `/dogrulama/{anahtar}?handler=VerifyEmail` — Doğrulama | T514 | | |
| 07 UC-54 GET `/giris` — Müşteri girişi ekranı | T515 | | |
| 07 UC-55 POST `/giris?handler=SignIn` — Müşteri girişi | T515 | | |
| 07 UC-56 GET `/sifre-sifirlama` — Şifre sıfırlama | T516 | | |
| 07 UC-57 POST `/sifre-sifirlama?handler=RequestPasswordReset` — Şifre sıfırlama isteği | T516 | | |
| 07 UC-58 GET `/sifre-sifirlama/{anahtar}` — Sıfırlama bağlantısının inişi | T516 | | |
| 07 UC-59 POST `/sifre-sifirlama/{anahtar}?handler=ResetPassword` — Yeni şifrenin kurulması | T516 | | |
| 07 UC-60 GET `/hesap/yeniden-dogrulama` — Yeniden doğrulama ekranı | T516 | | |
| 07 UC-61 POST `/hesap/yeniden-dogrulama?handler=Reauthenticate` — Şifreyle yeniden doğrulama | T516 | | |
| 07 UC-62 POST `/hesap/yeniden-dogrulama?handler=StartSocialReauthentication` — Google ile yeniden doğrulama | T516 | | |
| 07 UC-63 GET `/hesap` — Hesap | T517 | | |
| 07 UC-64 POST `/hesap?handler=ChangeName` — Adı değiştirme | T517 | | |
| 07 UC-65 POST `/hesap?handler=ChangePassword` — Şifreyi değiştirme | T517 | | |
| 07 UC-66 POST `/hesap?handler=ChangeEmail` — E-postayı değiştirme | T517 | | |
| 07 UC-67 POST `/hesap?handler=ResendEmailChangeLink` — Bekleyen e-posta değişikliğinin bağlantısını yeniden isteme | T517 | | |
| 07 UC-68 POST `/hesap?handler=DeleteAccount` — Hesabı silme | T517 | | |
| 07 UC-69 GET `/hesap/silindi` — Hesap silmenin sonuç hâli | T517 | | |
| 07 UC-70 GET `/hesap/adresler` — Adres defteri | T518 | | |
| 07 UC-71 POST `/hesap/adresler?handler=CreateAddressBook` — Adres ekleme | T518 | | |
| 07 UC-72 POST `/hesap/adresler?handler=UpdateAddressBook` — Adres düzenleme | T518 | | |
| 07 UC-73 POST `/hesap/adresler?handler=DeleteAddressBook` — Adres silme | T518 | | |
| 07 UC-74 GET `/hesap/siparisler` — Sipariş geçmişi | T518 | | |
| 07 UC-75 GET `/geri-al/{anahtar}` — Geri alma bağlantısının inişi | T519 | | |
| 07 UC-76 POST `/geri-al/{anahtar}?handler=RevertPendingEmailChange` — E-posta değişikliğini geri alma | T519 | | |
| 07 UC-77 POST `/cikis?handler=SignOut` — Müşteri çıkışı | T519 | | |
| 07 UC-78 — sayfa isteğinin yanıtı (500; §2.7.1) — Beklenmeyen hata sayfası | T504 | | |
| 07 UC-79 GET `/sitemap.xml` ve 1 adres daha — Site haritası ve robots.txt | T313 | | |
| 07 UC-80 POST `/sistem/odeme-bildirimi` — Ödeme sağlayıcısının bildirim ucu | T311 | | |
| 07 UC-81 GET `/sistem/canlilik` — Canlılık ucu | T01 | | |
| 07 UC-82 GET `/sistem/durum` — Durum ucu | T314 | | |
| 07 UC-83 POST `/panel/yukleme` (başlatma) ve 2 adres daha — Parçalı dosya yükleme ucu | T312 | | |
| 07 UC-84 GET `/panel/giris` ve 2 adres daha — Panel girişi ve şifre sıfırlama | T520 | | |
| 07 UC-85 GET `/panel/davet/{anahtar}` — Yönetici daveti kabul ekranı | T520 | | |
| 07 UC-86 GET `/panel` — Panel ana sayfası | T521 | | |
| 07 UC-87 GET `/panel/urunler` — Ürün listesi | T525 | | |
| 07 UC-88 GET `/panel/urunler/yeni` ve 1 adres daha — Ürün formu | T525 | | |
| 07 UC-89 GET `/panel/kategoriler` — Kategori ağacı | T526 | | |
| 07 UC-90 GET `/panel/kuponlar` ve 2 adres daha — Kuponlar | T526 | | |
| 07 UC-91 GET `/panel/siparisler` — Sipariş listesi | T527 | | |
| 07 UC-92 GET `/panel/siparisler/{iç anahtar}` — Sipariş ayrıntısı | T527 | | |
| 07 UC-93 GET `/panel/talepler` — Talepler | T532 | | |
| 07 UC-94 GET `/panel/talepler/{iç anahtar}` — İletişim talebi ayrıntısı | T532 | | |
| 07 UC-95 GET `/panel/uyeler` — Üye kaydı görünümü | T533 | | |
| 07 UC-96 GET `/panel/kurumsal-icerik` ve 2 adres daha — Kurumsal içerik | T534 | | |
| 07 UC-97 GET `/panel/ana-sayfa-ve-menu` — Ana sayfa ve menü | T535 | | |
| 07 UC-98 GET `/panel/marka` — Marka | T534 | | |
| 07 UC-99 GET `/panel/firma-kimligi-ve-satis` — Firma kimliği ve satış | T535 | | |
| 07 UC-100 GET `/panel/odeme-yontemleri` — Ödeme yöntemleri | T536 | | |
| 07 UC-101 GET `/panel/kargo-sureler-ve-varsayilanlar` — Kargo, süreler ve varsayılanlar | T536 | | |
| 07 UC-102 GET `/panel/yasal-metinler` — Yasal metinler | T507 | | |
| 07 UC-103 GET `/panel/yonetici-hesaplari` — Yönetici hesapları | T522 | | |
| 07 UC-104 GET `/panel/hesabim` — Yöneticinin kendi hesabı | T523 | | |
| 07 UC-105 GET `/panel/satis-ozeti` — Satış özeti | T524 | | |
| 07 UC-106 GET `/panel/islem-izi` — İşlem izi | T522 | | |
| 07 UC-107 GET `/panel/disa-aktarma` — Dışa aktarma | T524 | | |
| 07 UC-108 GET `/panel/yeniden-dogrulama` ve 2 adres daha — Yönetici hesabının doğrulama ekranları | T521 | | |
| 07 UC-109 POST `/panel/cikis?handler=SignOut` — Yönetici çıkışı | T520 | | |
| 07 UC-110 POST `/panel/giris?handler=SignIn` — Panel girişi | T520 | | |
| 07 UC-111 POST `/panel/sifre-sifirlama?handler=RequestPasswordReset` — Panel şifre sıfırlama isteği | T520 | | |
| 07 UC-112 POST `/panel/sifre-sifirlama/{anahtar}?handler=ResetPassword` — Panelde yeni şifrenin kurulması | T520 | | |
| 07 UC-113 POST `/panel/davet/{anahtar}?handler=AcceptAdminInvitation` — Davetle yönetici hesabını açma | T520 | | |
| 07 UC-114 POST `/panel/urunler/yeni?handler=SaveProduct` ve 1 adres daha — Ürünü kaydetme | T525 | | |
| 07 UC-115 POST `/panel/urunler/{iç anahtar}?handler=ChangeProductPublishStatus` ve 1 adres daha — Ürünün ya da varyantın yayın geçişi | T525 | | |
| 07 UC-116 POST `/panel/urunler/{iç anahtar}?handler=DeleteProduct` ve 1 adres daha — Ürünü ya da varyantı kalıcı silme | T525 | | |
| 07 UC-117 POST `…?handler=UploadImage` — Görsel yükleme | T525 | | |
| 07 UC-118 POST `/panel/kategoriler?handler=CreateCategory` — Kategori ekleme | T526 | | |
| 07 UC-119 POST `/panel/kategoriler?handler=RenameCategory` — Kategoriyi yeniden adlandırma | T526 | | |
| 07 UC-120 POST `/panel/kategoriler?handler=MoveCategory` — Kategoriyi taşıma | T526 | | |
| 07 UC-121 POST `/panel/kategoriler?handler=DeleteCategory` — Kategoriyi silme | T526 | | |
| 07 UC-122 POST `/panel/kuponlar/yeni?handler=SaveCoupon` ve 1 adres daha — Kuponu kaydetme | T526 | | |
| 07 UC-123 POST `/panel/siparisler/{iç anahtar}?handler=MarkPaid` — Havale siparişini "Ödendi" işaretleme | T527 | | |
| 07 UC-124 POST `/panel/siparisler/{iç anahtar}?handler=MarkShipped` — Kargoya verme | T527 | | |
| 07 UC-125 POST `/panel/siparisler/{iç anahtar}?handler=MarkReshipped` — Kargoya yeniden verme | T527 | | |
| 07 UC-126 POST `/panel/siparisler/{iç anahtar}?handler=MarkDelivered` — Teslim işareti ve tarihi | T527 | | |
| 07 UC-127 POST `/panel/siparisler/{iç anahtar}?handler=MarkDeliveryFailed` — "Teslim edilemedi" işareti | T527 | | |
| 07 UC-128 POST `/panel/siparisler/{iç anahtar}?handler=CloseDeliveryFailed` — Teslim edilemedi siparişini kapatma | T527 | | |
| 07 UC-129 POST `/panel/siparisler/{iç anahtar}?handler=CancelOrderByCompany` — Firma iptali | T528 | | |
| 07 UC-130 POST `/panel/siparisler/{iç anahtar}?handler=RecordWithdrawal` ve 1 adres daha — Başka kanaldan gelen cayma ya da gecikme feshi bildiriminin kaydı | T528 | | |
| 07 UC-131 POST `/panel/siparisler/{iç anahtar}?handler=ProcessRefund` — Geri ödemeyi işleme | T529 | | |
| 07 UC-132 POST `/panel/siparisler/{iç anahtar}?handler=MarkBankTransferRefundFailed` — "Havale gerçekleşmedi" | T529 | | |
| 07 UC-133 POST `/panel/siparisler/{iç anahtar}?handler=ProcessAmountBasedRefund` — Tutar bazlı kısmi geri ödeme | T529 | | |
| 07 UC-134 POST `/panel/siparisler/{iç anahtar}?handler=RequestRefundIban` — IBAN isteme | T529 | | |
| 07 UC-135 POST `/panel/siparisler/{iç anahtar}?handler=RetryCardRefund` — Kart iadesini yeniden deneme | T529 | | |
| 07 UC-136 POST `/panel/siparisler/{iç anahtar}?handler=SwitchRefundToBankTransfer` — Havale yolunu açma | T529 | | |
| 07 UC-137 POST `/panel/siparisler/{iç anahtar}?handler=MarkCardRefundFailed` — Sonucu beklenen kart iadesini "gerçekleşmedi" işaretleme | T529 | | |
| 07 UC-138 POST `/panel/siparisler/{iç anahtar}?handler=RemoveOrderItem` — Kalem çıkarma ya da adet azaltma | T528 | | |
| 07 UC-139 POST `/panel/siparisler/{iç anahtar}?handler=CompleteServiceItem` — Hizmet kalemini tamamlama | T531 | | |
| 07 UC-140 POST `/panel/siparisler/{iç anahtar}?handler=ReceiveReturn` ve 1 adres daha — İade malını teslim alma ya da reddetme | T528 | | |
| 07 UC-141 POST `/panel/siparisler/{iç anahtar}?handler=RestockReturn` — İade malını stoğa ekleme | T528 | | |
| 07 UC-142 POST `/panel/siparisler/{iç anahtar}?handler=CloseNotReturned` — "Mal dönmedi" kapanışı | T528 | | |
| 07 UC-143 POST `/panel/siparisler/{iç anahtar}?handler=ResetDownloadCount` — İndirme hakkını yenileme | T531 | | |
| 07 UC-144 POST `/panel/siparisler/{iç anahtar}?handler=ResolveDefectClaim` — Ayıp talebini çözüldü işaretleme | T528 | | |
| 07 UC-145 POST `/panel/siparisler/{iç anahtar}?handler=ReopenDefectClaim` — Ayıp talebini yeniden açma | T528 | | |
| 07 UC-146 POST `/panel/siparisler/{iç anahtar}?handler=CorrectAddress` — Adres düzeltmesi | T530 | | |
| 07 UC-147 POST `/panel/siparisler/{iç anahtar}?handler=CorrectCarrier` — Kargo şirketi ve takip numarası düzeltmesi | T530 | | |
| 07 UC-148 POST `/panel/siparisler/{iç anahtar}?handler=CorrectDeliveryDate` — Teslim tarihi düzeltmesi | T530 | | |
| 07 UC-149 POST `/panel/siparisler/{iç anahtar}?handler=CorrectArrivalDate` — Ulaşma tarihi düzeltmesi | T530 | | |
| 07 UC-150 POST `/panel/siparisler/{iç anahtar}?handler=CorrectTransition` — Durum geçişi düzeltmesi | T530 | | |
| 07 UC-151 POST `/panel/siparisler/{iç anahtar}?handler=CorrectOrderEmail` — Misafir siparişinin e-postasını düzeltme | T530 | | |
| 07 UC-152 POST `/panel/siparisler/{iç anahtar}?handler=SaveInternalNote` — İç notu kaydetme | T531 | | |
| 07 UC-153 POST `/panel/siparisler/{iç anahtar}?handler=ResendNotification` — Ulaşmayan e-postayı yeniden gönderme | T531 | | |
| 07 UC-154 POST `/panel/siparisler/{iç anahtar}?handler=ResendAllNotifications` — Ulaşmayan bütün e-postaları yeniden gönderme | T531 | | |
| 07 UC-155 POST `/panel/talepler/{iç anahtar}?handler=CloseContactRequest` — İletişim talebini kapatma | T532 | | |
| 07 UC-156 POST `/panel/talepler/{iç anahtar}?handler=ReopenContactRequest` — İletişim talebini yeniden açma | T532 | | |
| 07 UC-157 POST `/panel/uyeler?handler=DeleteCustomer` — Üye hesabını silme | T533 | | |
| 07 UC-158 POST `/panel/kurumsal-icerik/{tip}/yeni?handler=SaveContent` ve 3 adres daha — İçerik kaydını kaydetme | T534 | | |
| 07 UC-159 POST `/panel/kurumsal-icerik/{tip}/{iç anahtar}?handler=ChangePublishStatus` — İçerik kaydının yayın geçişi | T534 | | |
| 07 UC-160 POST `/panel/kurumsal-icerik/{tip}/{iç anahtar}?handler=DeleteContent` — İçerik kaydını kalıcı silme | T534 | | |
| 07 UC-161 POST `/panel/kurumsal-icerik?handler=ReorderContent` — İçerik kaydının elle sırası | T534 | | |
| 07 UC-162 POST `/panel/ana-sayfa-ve-menu?handler=SaveHomepage` — Ana sayfa ve menü ayarlarını kaydetme | T535 | | |
| 07 UC-163 POST `/panel/marka?handler=SaveBranding` — Marka ayarlarını kaydetme | T534 | | |
| 07 UC-164 POST `/panel/firma-kimligi-ve-satis?handler=SaveCompany` — Firma kimliğini kaydetme | T535 | | |
| 07 UC-165 POST `/panel/firma-kimligi-ve-satis?handler=SuspendSales` — Satışı geçici olarak kapatma | T535 | | |
| 07 UC-166 POST `/panel/firma-kimligi-ve-satis?handler=ResumeSales` — Satışı yeniden açma | T535 | | |
| 07 UC-167 POST `/panel/odeme-yontemleri?handler=SavePaymentMethods` — Ödeme yöntemlerini kaydetme | T536 | | |
| 07 UC-168 POST `/panel/kargo-sureler-ve-varsayilanlar?handler=SaveShippingFee` — Kargo ücreti ve eşikleri kaydetme | T536 | | |
| 07 UC-169 POST `/panel/kargo-sureler-ve-varsayilanlar?handler=SaveDeliveryProvinces` — Teslimat illerini kaydetme | T536 | | |
| 07 UC-170 POST `/panel/kargo-sureler-ve-varsayilanlar?handler=SavePeriods` — Süreleri kaydetme | T536 | | |
| 07 UC-171 POST `/panel/kargo-sureler-ve-varsayilanlar?handler=SaveProductDefaults` — Ürün varsayılanlarını kaydetme | T536 | | |
| 07 UC-172 POST `/panel/kargo-sureler-ve-varsayilanlar?handler=SaveReturnAddress` — İade adresini kaydetme | T536 | | |
| 07 UC-173 POST `/panel/yasal-metinler?handler=SaveLegalTextDraft` — Yasal metnin taslağını kaydetme | T507 | | |
| 07 UC-174 POST `/panel/yasal-metinler?handler=PublishLegalText` — Yasal metni yayına alma | T507 | | |
| 07 UC-175 POST `/panel/yonetici-hesaplari?handler=SendAdminInvitation` — Yönetici daveti gönderme | T522 | | |
| 07 UC-176 POST `/panel/yonetici-hesaplari?handler=RevokeAdminInvitation` — Daveti geri çekme | T522 | | |
| 07 UC-177 POST `/panel/yonetici-hesaplari?handler=RemoveAdmin` — Yöneticiyi kaldırma | T522 | | |
| 07 UC-178 POST `/panel/hesabim?handler=ChangeName` — Yöneticinin adını değiştirme | T523 | | |
| 07 UC-179 POST `/panel/hesabim?handler=ChangeEmail` — Yöneticinin e-postasını değiştirme | T523 | | |
| 07 UC-180 POST `/panel/hesabim?handler=ResendEmailChangeLink` — Yönetici e-posta değişikliğinin bağlantısını yeniden isteme | T523 | | |
| 07 UC-181 POST `/panel/hesabim?handler=ChangePassword` — Yöneticinin şifresini değiştirme | T523 | | |
| 07 UC-182 POST `/panel/disa-aktarma?handler=ExportOrders` — Siparişlerin dışa aktarılması | T524 | | |
| 07 UC-183 POST `/panel/disa-aktarma?handler=ExportCustomers` — Üye listesinin dışa aktarılması | T524 | | |
| 07 UC-184 POST `/panel/disa-aktarma?handler=ExportContactRequests` — İletişim taleplerinin dışa aktarılması | T524 | | |
| 07 UC-185 POST `/panel/yeniden-dogrulama?handler=Reauthenticate` — Yöneticinin şifreyle yeniden doğrulaması | T521 | | |
| 07 UC-186 POST `/panel/dogrulama/{anahtar}?handler=VerifyEmail` — Yöneticinin yeni adresinin doğrulanması | T521 | | |
| 07 UC-187 POST `/panel/geri-al/{anahtar}?handler=RevertPendingEmailChange` — Yöneticinin e-posta değişikliğini geri alması | T521 | | |
| 07:§5.1 Okunuş ve bağlama | T211 | | |
| 07:§5.2 Form girdi modelleri | T538 | | |
| 07:§5.3 Sorgu modelleri | T538 | | |
| 07:§5.4 Makine uçlarının gövdeleri | T312, T538 | | |
| 07:§5.5 Çerçevenin modeli | T537 | | |
| 07:§6.1 Okunuş — kodun biçimi | T212 | | |
| 07:§6.2 Hata kodları | T213 | | |
| 07:§6.3 Kısa süreli bildirim kodları | T214 | | |
| 07:§6.4 Özel hâller | T212 | | |
| 07:§8.1 Yapılabilir işlemler sunucuda | T217 | | |
| 07:§8.2 JavaScript kapalıyken temel akışlar | T539 | | |

#### 7.1.3 Entegrasyon → task

| Kaynak öğe | Task | Implement edildi | Kanıt |
|---|---|---|---|
| 08:§2.1 Ödeme sağlayıcısı — PayTR | T401 | | |
| 08:§2.1.1 Sağlayıcının seçimi | T401 | | |
| 08:§2.1.2 Anahtarlar ve değişim | T401 | | |
| 08:§2.1.3 3D Secure | T406 | | |
| 08:§2.1.4 Ödemenin başlatılması, dönüş ve çerçeve | T401, T510 | | |
| 08:§2.1.5 Bildirimin kabulü ve alan eşlemesi | T402, T311 | | |
| 08:§2.1.6 Son sorgu | T403 | | |
| 08:§2.1.7 Tek tahsilat | T403 | | |
| 08:§2.1.8 Bağlantı hatası ve ayrım | T404 | | |
| 08:§2.1.9 Kart iadesi | T405 | | |
| 08:§2.1.10 Test ve canlı kanıt | T406 | | |
| 08:§2.2 E-posta gönderim altyapısı — Amazon SES | T407 | | |
| 08:§2.2.1 Servisin seçimi | T407 | | |
| 08:§2.2.2 Gönderim ve sonuç sınıfları | T407 | | |
| 08:§2.2.3 Geri dönen adreslerin öğrenilmesi | T408 | | |
| 08:§2.2.4 Alıcı başına gönderim sınırı ve kaçış | T304 | | |
| 08:§2.2.5 Ortak kalıp | T305 | | |
| 08:§2.2.6 Müşteri bildirimleri B-1…B-16 | T306, T307 | | |
| 08:§2.2.7 Firma bildirimleri F-1…F-6 | T308 | | |
| 08:§2.2.8 Hesap ve yönetim e-postaları | T309 | | |
| 08:§2.3 Google ile giriş, kayıt ve yeniden doğrulama | T409 | | |
| 08:§2.4 Çağrısı olmayan dış bağlantılar — kargo şirketi listesi | T310 | | |
| 08:§3.1 Bağlantı katmanının ortak yüzü | T202 | | |
| 08:§3.2 İmzalı makine uçlarının ortak ilkesi | T203 | | |
| 08:§3.3 Bir dış servis çökerse | T204 | | |
| 08:§4.1 Dış olgu varsayımları | T410 | | |
| 08:§4.2 Dış sistemde kayıtlı adresler | T601 | | |
| 08:§4.3 Ürünle gelen dış kaynaklı varlık — yerli üretim logosu | T501 | | |
| 08:§5.1 `07`'ye geri yansıyan uçlar | — | | |
| 08:§5.2 Üst dokümanlara geri besleme | T410 | | |

#### 7.1.4 Arayüz → task

Kaynak: `04` (102: ekran 53 `04:E-nn` · ortak bileşen 20 `04:OB-nn` · alt bölüm 29 `04:§n.m`) ve `03` (55 alt bölüm, `03:§n.m`). Ekran birimleri O2'nin F5 ekran gruplu task'larına (T502–T539) bağlandı; ortak bileşenler, gezinme ve form eşitliği için F5'te sekiz, durum makinesi, süreler ve bildirim eşlemesi için F3'te on beş aday açıldı (K-1418).

| Kaynak öğe | Task | Implement edildi | Kanıt |
|---|---|---|---|
| 04:E-01 Ana sayfa | T502 | | |
| 04:E-02 Kategori sayfası | T502 | | |
| 04:E-03 Arama sonuçları | T503 | | |
| 04:E-04 Ürün sayfası | T503 | | |
| 04:E-05 Arşivlenmiş ürün sayfası | T503 | | |
| 04:E-06 "Sayfa bulunamadı" sayfası | T504 | | |
| 04:E-07 İçerik liste sayfası | T505 | | |
| 04:E-08 İçerik sayfası | T505 | | |
| 04:E-09 Sık sorulan sorular sayfası | T506 | | |
| 04:E-10 İletişim sayfası ve formu | T506 | | |
| 04:E-11 Yasal metin sayfası | T507 | | |
| 04:E-12 Sepet | T508 | | |
| 04:E-13 Ödeme adımı | T509 | | |
| 04:E-14 Sipariş teyit ve bekleme ekranı | T510 | | |
| 04:E-15 Sipariş takibi girişi | T511 | | |
| 04:E-16 Sipariş sayfası | T511, T512 | | |
| 04:E-17 İptal ve gecikme feshi ekranı | T512 | | |
| 04:E-18 Cayma beyanı ekranı | T513 | | |
| 04:E-19 Ayıp talebi ekranı | T513 | | |
| 04:E-20 Kayıt ekranı | T514 | | |
| 04:E-21 Doğrulama bağlantısının iniş ekranı | T514 | | |
| 04:E-22 Müşteri girişi | T515 | | |
| 04:E-23 Şifre sıfırlama ekranları | T516 | | |
| 04:E-24 Yeniden doğrulama ekranı | T516 | | |
| 04:E-25 Hesap — profil ve güvenlik | T517 | | |
| 04:E-26 Adres defteri | T518 | | |
| 04:E-27 Sipariş geçmişi | T518 | | |
| 04:E-28 E-posta değişikliğini geri alma ekranı | T519 | | |
| 04:E-29 Panel girişi ve şifre sıfırlama | T520 | | |
| 04:E-30 Yönetici daveti kabul ekranı | T520 | | |
| 04:E-31 Panel ana sayfası | T521 | | |
| 04:E-32 Ürün listesi | T525 | | |
| 04:E-33 Ürün formu | T525 | | |
| 04:E-34 Kategori ağacı | T526 | | |
| 04:E-35 Kuponlar | T526 | | |
| 04:E-36 Sipariş listesi | T527 | | |
| 04:E-37 Sipariş ayrıntısı | T527, T528, T529, T530, T531 | | |
| 04:E-38 Talepler | T532 | | |
| 04:E-39 İletişim talebi ayrıntısı | T532 | | |
| 04:E-40 Üye kaydı görünümü | T533 | | |
| 04:E-41 Kurumsal içerik | T534 | | |
| 04:E-42 Ana sayfa ve menü | T535 | | |
| 04:E-43 Marka | T534 | | |
| 04:E-44 Firma kimliği ve satış | T535 | | |
| 04:E-45 Ödeme yöntemleri | T536 | | |
| 04:E-46 Kargo, süreler ve varsayılanlar | T536 | | |
| 04:E-47 Yasal metinler | T507 | | |
| 04:E-49 Yönetici hesapları | T522 | | |
| 04:E-50 Yöneticinin kendi hesabı | T523 | | |
| 04:E-51 Satış özeti | T524 | | |
| 04:E-52 İşlem izi | T522 | | |
| 04:E-53 Dışa aktarma | T524 | | |
| 04:E-54 Yönetici hesabının doğrulama ekranları | T521 | | |
| 04:OB-01 Tasarım tabanı | T540 | | |
| 04:OB-02 Çerçeve | T540, T537 | | |
| 04:OB-03 Mesaj | T541 | | |
| 04:OB-04 Onay | T541 | | |
| 04:OB-05 Rozet ve işaretler | T542 | | |
| 04:OB-06 Sayaç ve süre göstergeleri | T542 | | |
| 04:OB-07 Boş, yükleniyor ve hata hâlleri | T541 | | |
| 04:OB-08 Satış kapalı hâli | T541 | | |
| 04:OB-09 "Taslak" bandı ve yönetici şeridi | T540 | | |
| 04:OB-10 Eşzamanlı düzenleme uyarısı | T542 | | |
| 04:OB-11 Ürün kartı | T545 | | |
| 04:OB-12 Form alanı ve form düzeni | T543 | | |
| 04:OB-13 Adres formu | T543 | | |
| 04:OB-14 IBAN alanı | T543 | | |
| 04:OB-15 Sebep seçimi | T543 | | |
| 04:OB-16 Tarih alanı | T544 | | |
| 04:OB-17 Kupon alanı | T544 | | |
| 04:OB-18 Onay kutuları | T544 | | |
| 04:OB-19 Görsel ve dosya yükleme | T544 | | |
| 04:OB-20 Liste ve tablo | T545 | | |
| 04:§2.1 Tasarım tabanı | T540 | | |
| 04:§2.2 Çerçeve | T540, T537 | | |
| 04:§2.3 Mesaj | T541 | | |
| 04:§2.4 Onay | T541 | | |
| 04:§2.5 Rozet ve işaretler | T542 | | |
| 04:§2.6 Sayaç ve süre göstergeleri | T542 | | |
| 04:§2.7 Boş, yükleniyor ve hata hâlleri | T541 | | |
| 04:§2.8 Satış kapalı hâli | T541 | | |
| 04:§2.9 "Taslak" bandı ve yönetici şeridi | T540 | | |
| 04:§2.10 Eşzamanlı düzenleme uyarısı | T542 | | |
| 04:§2.11 Ürün kartı | T545 | | |
| 04:§2.12 Form bileşenleri | T543, T544 | | |
| 04:§2.13 Liste ve tablo | T545 | | |
| 04:§3.1 Vitrinin gezinme iskeleti | T540 | | |
| 04:§3.2 Panelin gezinme iskeleti | T540 | | |
| 04:§3.3 Müşteri tarafının geçişleri | T546 | | |
| 04:§3.4 Panelin geçişleri | T546 | | |
| 04:§3.5 Sitenin dışından girişler | T546 | | |
| 04:§3.6 Girişten ve çıkıştan dönüş | T546 | | |
| 04:§3.7 Panel ile vitrin arasındaki geçişler | T546 | | |
| 04:§6.1 Kapsam ve okunuş — durum taşıyan ekranlar | T547 | | |
| 04:§6.2 Müşteri tarafı | T547 | | |
| 04:§6.3 Panel | T547 | | |
| 04:§7.1 Müşteri tarafının formları | T547 | | |
| 04:§7.2 Panelin formları | T547 | | |
| 04:§7.3 Ortak alan kuralları | T543 | | |
| 04:§8.1 Tek dil ve dil seçiminin yokluğu | T216 | | |
| 04:§8.2 Türkçenin biçimleri — tarih, sayı, para | T216 | | |
| 04:§8.3 Metin uzunluğunun yerleşime etkisi | T540 | | |
| 03:§1.1 Envanter | T315 | | |
| 03:§1.2 Sevkiyat ekseni — Sipariş durumu | T316 | | |
| 03:§1.3 Ödeme ekseni — Ödeme durumu | T317 | | |
| 03:§1.4 İki eksenin bağı | T318 | | |
| 03:§1.5 Tipe göre hatlar | T318 | | |
| 03:§1.6 Geri alınamaz geçişler, onay pencereleri ve düzeltme | T319 | | |
| 03:§1.7 Kalem kayıtları ve panel işaretleri | T320 | | |
| 03:§1.8 Yayın durumları | T321, T322 | | |
| 03:§1.9 Talep durumları | T320, T322 | | |
| 03:§1.10 Durum makinesi olmayan varlıklar | T315 | | |
| 03:§1.11 Durum × rol × işlem | T323, T217 | | |
| 03:§2.1 Ziyaretçi: vitrinde gezinme ve ürünü bulma | T502, T503 | | |
| 03:§2.2 Ziyaretçi: kurumsal içerik ve iletişim formu | T505, T506 | | |
| 03:§2.3 Müşteri: sepet | T508 | | |
| 03:§2.4 Müşteri: ödeme adımı ve sipariş onayı | T509, T510 | | |
| 03:§2.5 Müşteri: ödeme ve teslim | T510, T311, T316, T317 | | |
| 03:§2.6 Müşteri: sipariş takibi (akış 2) | T511 | | |
| 03:§2.7 Müşteri: iptal ve gecikme feshi | T512 | | |
| 03:§2.8 Müşteri: cayma ve iade | T513 | | |
| 03:§2.9 Müşteri: ayıp talebi | T513 | | |
| 03:§2.10 Uçtan uca anlatılar | T329 | | |
| 03:§3.1 Bütün akışlara uygulanan ilkeler | T212, T541 | | |
| 03:§3.2 Satın alma ve sipariş takibi | T508, T509, T510, T511 | | |
| 03:§3.3 İptal, cayma ve iade (akış 3) | T512, T513 | | |
| 03:§3.4 Üyelik (akış 4) | T514, T515, T516, T517 | | |
| 03:§3.5 Firma tarafı | T525, T527, T528, T529, T530, T534, T535, T536 | | |
| 03:§4.1 Süre envanterinden zaman aşımı akışları | T324, T325, T326, T201 | | |
| 03:§4.2 Kendiliğinden işleyen anlar | T327 | | |
| 03:§5.1 Üç yol ve itirazın yeri | T511, T512, T513, T506 | | |
| 03:§5.2 Ürünün dışında çözülen anlaşmazlıklar | T506, T528, T529, T530 | | |
| 03:§5.3 İade reddi ve değer kaybı | T320, T528 | | |
| 03:§6.1 Deneme limitleri | T215, T509, T515, T516 | | |
| 03:§6.2 Teyit ve kandırılma akışları | T519, T527, T530 | | |
| 03:§6.3 Firmanın inceleme akışları | T521, T527 | | |
| 03:§7.1 Bildirimler ve tetikleyicileri | T306, T307, T308, T309, T328 | | |
| 03:§7.2 Bildirim üretmeyen olaylar ve olmayan bildirimler | T328 | | |
| 03:§7.3 Geçiş ve olay bazlı eşleme | T328 | | |
| 03:§8.1 Katalog yönetimi (akış 5) | T525, T526 | | |
| 03:§8.2 Sipariş yürütümü: olağan hat (akış 6) | T316, T318, T527 | | |
| 03:§8.3 Firma iptali, iade teslim alma ve geri ödeme | T528, T529 | | |
| 03:§8.4 Sipariş müdahaleleri | T528, T530, T531 | | |
| 03:§8.5 Manuel adım bütçesi | T318 | | |
| 03:§8.6 Kurumsal içerik, marka, duyuru ve iletişim talepleri (akış 7'nin panel yüzü) | T532, T534 | | |
| 03:§8.7 Mağaza ayarları, satış kapısı ve kurulum kontrol listesi (akış 8) | T521, T535, T536 | | |
| 03:§8.8 Yönetici hesapları ve davet | T520, T522 | | |
| 03:§8.9 Bekleyen işler, satış özeti, işlem izi ve dışa aktarma | T521, T522, T524 | | |
| 03:§9.1 Kayıt, e-posta doğrulama ve Google ile ilk giriş | T409, T514, T515 | | |
| 03:§9.2 Giriş, oturum, şifre sıfırlama ve yeniden doğrulama | T515, T516, T520 | | |
| 03:§9.3 Profil ve adres defteri | T517, T518 | | |
| 03:§9.4 Hesabın silinmesi ve kişisel veri başvurusu | T517, T533 | | |
| 03:§9.5 Tercih yönetimi | — | | |
| 03:§10.1 Dış servis kesintileri | T204, T407, T509 | | |
| 03:§10.2 Bakım ve güncelleme | T535 | | |
| 03:§10.3 Sitenin kesintisi | T327 | | |
| 03:§10.4 Panele erişimin kaybı | T520 | | |

#### 7.1.5 İş kuralı → task

> Doldurulacak: O4a (`02 §1`–§3; yazıldı), O4b 1/2 (`02 §4`–§8; yazıldı), O4b 2/2 (`02 §9`–§13; yazıldı), O5 1/2 (`05`; yazıldı), O5 2/2 (`09`, `10`, devir; yazıldı). Beş kalın etiketli grup, her birinin kendi tablosu: **`02`** (O4a: §1–§3; O4b: §4–§13) · **`05`** · **`09`** · **`10`** · **Devir satırları** (O5).

Kaynak: `02 §1`–§3 (297: iş kuralı 257 `02:§3.n.m` · alt bölüm 40 `02:§n.m` — §1'in 3'ü, §2'nin 3'ü, §3'ün 34'ü). Kural satırı kuralın kalın başlık cümlesidir; kuralın iş mantığı F3'ün modül task'larındadır (T330–T366, K-1421), ekranda görünen yüzü F5'in ekran task'larındadır — bir kural birden çok task alabilir. `02 §3.26` ve `§1.1`–§1.2 task gerektirmez (gerekçesi yukarıda).

Kaynak (O4b 1/2): `02 §4`–§8 (193: iş kuralı 101 `02:§n.m.k` · kimlikli satır 56 — Z-1…Z-47 ve L-1…L-9 · alt bölüm 36 `02:§n.m`; §4 55 · §5 31 · §6 15 · §7 50 · §8 42). `§6`'nın `6.2.n`…`6.9.n` senaryo satırları birim değildir: alt bölüm satırı o bölümün task birleşimini taşır. `§5.4` ve `§5.5`'in geçiş tabloları (S1–S11, Ö1–Ö5) T315–T319'a bağlıdır (K-1419 (5)); yeni durum makinesi task'ı açılmadı. `§4.2`'nin Z satırları süre akışlarına T324–T326'dan (O3), saklama satırları T201, T301, T302'den gelir. İptal, cayma, iade ve ayıp (`§7`) için F3'ün modül task'ları T367–T375'tir (K-1425); sınır deneme limitleri ve zaman sayımı T376–T378'dir. Kuralın ekranda görünen yüzü F5 task'larındadır (T511–T513, T528–T531, T541, T543). **Task gerektirmeyen iki birim:** `02:Z-25` (KVKK başvurusuna cevap, 30 gün) ve `02:Z-37` (veri ihlalinde Kurul'a bildirim, 72 saat) — satır "sistemde sayaç tutulmaz; yükümlülük firmanındır" der, ürün bir süre işletmez; "Task" hücresi `—` (K-1426). GAP değildir. `02:§3.26`'nın işaret ettiği kurallar `§7.2`–§7.5 satırlarında eşlendi.

**`02`** — `§1`–§8 (O4a: `§1`–§3; O4b 1/2: `§4`–§8)

| Kaynak öğe | Task | Implement edildi | Kanıt |
|---|---|---|---|
| 02:§1.1 Sözlük kuralı | — | | |
| 02:§1.2 Terim sözlüğü | — | | |
| 02:§1.3 Aktörler | T106, T107, T207 | | |
| 02:§2.1 Akış envanteri | T329, T511, T512, T513, T514, T525, T527, T534, T536 | | |
| 02:§2.2 Uçtan uca ana akış | T329, T509, T510 | | |
| 02:§2.3 Kurumsal içerik akışı | T358, T360, T361, T534 | | |
| 02:§3.1 Firma, kurulum ve satış kapısı | T330, T331 | | |
| 02:§3.1.1 Uygulama kurulum başına tek bir firmaya hizmet eder | T330, T103 | | |
| 02:§3.1.2 Firmanın kimliği panelden yönetilir; altyapısı kurulumdan gelir | T330, T535 | | |
| 02:§3.1.3 Firma tipi seçilir ve zorunlu kimlik alanları tipe göre değişir | T330, T535 | | |
| 02:§3.1.4 Yasal kimlik bilgileri sitede sürekli erişilebilir durur | T330, T506, T537 | | |
| 02:§3.1.5 Satış yalnız dört koşul birlikte sağlandığında açıktır | T331, T503, T509, T535 | | |
| 02:§3.1.6 Firma satışı panelden geçici olarak kapatabilir | T331, T508, T535 | | |
| 02:§3.1.7 Firmanın iade adresi zorunlu bir firma ayarıdır | T330, T513, T535 | | |
| 02:§3.2 Satış modeli ve ürün tipleri | T332 | | |
| 02:§3.2.1 Satış yalnız doğrudandır | T332, T503 | | |
| 02:§3.2.2 Katalog üç tipte ürün taşır: fiziksel ürün, dijital ürün ve hizmet | T332, T525 | | |
| 02:§3.2.3 Tip ürün düzleminde yaşar ve üç değerli kapalı bir alandır | T332 | | |
| 02:§3.3 Ürün, varyant ve seçenek | T332 | | |
| 02:§3.3.1 Satılan birim varyanttır; ürün vitrin ve katalog birimidir | T332, T503 | | |
| 02:§3.3.2 Bir ürün en fazla iki seçenek boyutu taşır | T332, T525 | | |
| 02:§3.3.3 İki boyutun birleşim matrisinin tam olması gerekmez | T332, T503 | | |
| 02:§3.3.4 Her varyant zorunlu ve tekil bir stok kodu (`SKU`) taşır; | T332, T525 | | |
| 02:§3.3.5 Ürün başına asgari adet yoktur | T332 | | |
| 02:§3.3.6 Ürün sayfasında yorum, puan ve öneri bloğu yoktur | T332, T503 | | |
| 02:§3.4 Kategori | T333 | | |
| 02:§3.4.1 Kategori ağacı en fazla üç seviyedir | T333, T526 | | |
| 02:§3.4.2 Ürün ağacın herhangi bir düğümüne asılabilir; | T333, T502 | | |
| 02:§3.4.3 Kategori sayfası alt dallarındaki ürünleri de gösterir | T333, T502 | | |
| 02:§3.4.4 Bir ürün birden fazla kategoriye asılabilir; asıldığı kategorilerden biri ana kategoridir | T333, T525 | | |
| 02:§3.4.5 İçinde ürün ya da alt kategori bulunan kategori silinemez | T333, T526 | | |
| 02:§3.4.6 Kategori taşımak serbesttir, iki sınırla | T333, T526 | | |
| 02:§3.5 Arama, süzgeç ve liste düzeni | T333 | | |
| 02:§3.5.1 Site içi arama yalnız ürün adında çalışır | T333, T503 | | |
| 02:§3.5.2 Ürün listeleri iki sabit eksende süzülür: fiyat aralığı ve stok durumu | T333, T502, T503 | | |
| 02:§3.5.3 Ziyaretçiye sıralama seçeneği sunulmaz | T333, T502, T503 | | |
| 02:§3.6 Stok ve tükenme | T334 | | |
| 02:§3.6.1 Stok, varyantın sayısal adedidir | T334, T525 | | |
| 02:§3.6.2 Stok takibi tipe göre değişir | T334, T525 | | |
| 02:§3.6.3 Tükenmiş varyant seçenek listesinde "Tükendi" işaretiyle görünür ve seçilemez; | T334, T503 | | |
| 02:§3.6.4 Bütün varyantları tükenmiş ürün vitrinde kalır, | T334, T503 | | |
| 02:§3.6.5 Stok adedi ziyaretçiye hiçbir yerde gösterilmez | T334, T503 | | |
| 02:§3.6.6 İptal edilen adet stoğa kendiliğinden döner; iade edilen fiziksel adedi firma teslim alıp kontrol ettikten sonra panelden ekler | T334, T528 | | |
| 02:§3.6.7 Stok ve hizmet kontenjanı, ödemesi beklenen siparişlerin ayırdığı adedin altına indirilemez | T334, T525 | | |
| 02:§3.7 Ürünün yayın durumu, arşiv ve silme | T335 | | |
| 02:§3.7.1 Ürün ve varyant yayın durumunu ayrı ayrı taşır: Taslak, Yayında, Arşiv | T335, T321, T525 | | |
| 02:§3.7.2 Arşiv silme değildir | T335 | | |
| 02:§3.7.3 Bir ürün yayına ancak adı, ana kategorisi ve tipi girilmişse ve en az bir Yayında varyantı varsa alınır | T335, T525 | | |
| 02:§3.7.4 Dijital ürünün yayın kapısı bir koşul daha taşır | T335, T340, T525 | | |
| 02:§3.7.5 Taslak ürünün adresi ziyaretçiye "sayfa bulunamadı" döner; giriş yapmış firma yöneticisine ürün sayfasını "Taslak" bandıyla gösterir | T335, T503 | | |
| 02:§3.7.6 Arşivlenmiş ürünün adresi çalışmaya devam eder ve "Bu ürün artık satılmıyor" sayfası döner | T335, T503 | | |
| 02:§3.7.7 Yönetici ürünü kalıcı olarak silebilir | T335, T525 | | |
| 02:§3.7.8 Yayındaki bir ürünün son Yayında varyantı arşivlenemez, taslağa çekilemez ve kalıcı olarak silinemez | T335, T525 | | |
| 02:§3.8 Fiyat ve KDV | T336 | | |
| 02:§3.8.1 Fiyat varyantta yaşar ve KDV dahil girilir | T336, T525 | | |
| 02:§3.8.2 KDV oranı ürün düzleminde tutulur ve varyantlar devralır; | T336, T525 | | |
| 02:§3.8.3 KDV kalem bazında hesaplanır ve kuruşa orada yuvarlanır | T336, T355 | | |
| 02:§3.8.4 Fiyatın hassasiyeti kuruştur — iki ondalık; | T336, T525 | | |
| 02:§3.8.5 Ürün sayfası fiyat etiketinin zorunlu bilgilerini taşır | T336, T501, T503, T525 | | |
| 02:§3.9 İndirim ve referans fiyat | T337 | | |
| 02:§3.9.1 İndirim ürün düzleminde yüzde olarak tanımlanır ve bütün varyantlara iner; | T337, T525 | | |
| 02:§3.9.2 İndirim tarihlidir | T337, T502, T503 | | |
| 02:§3.9.3 İndirim beyanı referans fiyata dayanır ve referansı sistem hesaplar; | T337 | | |
| 02:§3.9.4 10 günlük geçmişi olmayan malda referans, | T337 | | |
| 02:§3.9.5 İndirimli fiyat referansın altına inmiyorsa sistem indirimi kaydetmez | T337, T525 | | |
| 02:§3.9.6 Beyan edilen referans fiyat sipariş kalemine donar | T337, T355 | | |
| 02:§3.10 Kupon | T338 | | |
| 02:§3.10.1 Kupon, ödeme adımında girilen ve sepet toplamına inen bir koddur | T338, T509 | | |
| 02:§3.10.2 Kupon yüzde ya da sabit tutar olarak tanımlanır | T338, T526 | | |
| 02:§3.10.3 Kuponun payı kalem kalem belirlenir | T338 | | |
| 02:§3.10.4 Kupon siparişi sıfır tutara indiremez: sepet toplamına eşit ya da onu aşan kupon reddedilir | T338, T526 | | |
| 02:§3.10.5 Kuponun sınırı tarih aralığı ve toplam kullanım adedidir | T338, T526 | | |
| 02:§3.10.6 Kuponun kullanım hakkı stok gibi ayrılır | T338, T348, T526 | | |
| 02:§3.10.7 Kupon, ücretsiz kargo eşiğinin ve asgari sipariş tutarının tabanını düşürmez; | T338, T349 | | |
| 02:§3.11 Ürün görseli ve metni | T339 | | |
| 02:§3.11.1 Görsel ürün düzleminde yaşar; | T339, T525 | | |
| 02:§3.11.2 Ayrı bir "ana görsel" işareti vardır ve galeri sırasından bağımsızdır; | T339, T525 | | |
| 02:§3.11.3 Görsele üç kısıt uygulanır | T339, T525 | | |
| 02:§3.11.4 Görselin alternatif metni isteğe bağlıdır ve geri düşüşlüdür | T339 | | |
| 02:§3.11.5 Ürün adına karakter tavanı konur; açıklama serbesttir | T339, T525 | | |
| 02:§3.11.6 Ürün açıklaması, kurumsal içeriğin uzun metinleri ve firmanın düzenlediği iki yasal metin tek bir kapalı metin biçimi setini kullanır | T339, T534 | | |
| 02:§3.11.7 Garanti bilgisinin ayrı alanı yoktur | T339 | | |
| 02:§3.12 Dijital ürün | T340 | | |
| 02:§3.12.1 Dijital ürünü sistem teslim eder | T340, T312, T511 | | |
| 02:§3.12.2 Dosya ürün düzleminde yaşar; varyant isterse kendi dosyasını taşır, taşımıyorsa ürününkini devralır | T340, T525 | | |
| 02:§3.12.3 Tek dosyanın boyut tavanı vardır; | T340, T312 | | |
| 02:§3.12.4 Dijital üründe adet her zaman 1'dir | T340, T508 | | |
| 02:§3.12.5 İndirmenin adet sınırı vardır, süre sınırı yoktur | T340, T312, T511 | | |
| 02:§3.12.6 Hakkı dolan müşteri firmaya başvurur; firma paneldeki sipariş sayfasından o kalemin indirme hakkını yeniler ve sayaç sıfırlanır | T340, T531 | | |
| 02:§3.12.7 Sipariş sayfası, kalemin bağlı olduğu varyantın o anki dosyasını verir | T340, T312 | | |
| 02:§3.12.8 Dosya güncellemesi indirme hakkını değiştirmez; | T340 | | |
| 02:§3.12.9 Daha önce alınmış dijital varyant yeniden satın alınabilir | T340, T508 | | |
| 02:§3.13 Üyelik, e-posta doğrulama ve oturum | T341, T342, T343 | | |
| 02:§3.13.1 Sipariş vermek için üyelik zorunlu değildir | T341, T509 | | |
| 02:§3.13.2 Hesap açıldığında, o e-postaya ait geçmiş misafir siparişleri hesaba düşer | T341 | | |
| 02:§3.13.3 Hesabı olan kişi giriş yapmadan misafir olarak sipariş verirse sipariş kabul edilir ve hesaba anında düşer; | T341, T509 | | |
| 02:§3.13.4 E-posta doğrulaması sert bir kapıdır | T341, T514 | | |
| 02:§3.13.5 Doğrulanmamış hesap geçicidir | T341, T514 | | |
| 02:§3.13.6 Sosyal giriş yalnız Google ile yapılır; | T343, T409, T515 | | |
| 02:§3.13.7 Aynı e-posta iki giriş yolundan gelirse tek hesaba bağlanır | T343, T409 | | |
| 02:§3.13.8 Google ile açılmış hesap şifresiz başlar ve sonradan şifre konabilir; | T343, T516 | | |
| 02:§3.13.9 Şifre sıfırlama bağlantısının bir ömrü vardır ve bağlantı tek kullanımlıktır | T342, T516 | | |
| 02:§3.13.10 Şifre sıfırlandığında hesabın bütün cihazlardaki oturumları kapanır; | T342, T516, T517 | | |
| 02:§3.13.11 Ekranlar hesabın varlığını iki farklı biçimde ele verir | T342, T514, T516 | | |
| 02:§3.13.12 Oturum ömrünü kullanıcı seçer | T342, T515 | | |
| 02:§3.13.13 Üç işlem, oturum açık olsa bile yeniden doğrulama ister | T342, T517 | | |
| 02:§3.13.14 Hesabın e-posta adresi, yeni adres doğrulanmadan değişmez | T343, T517, T519 | | |
| 02:§3.13.15 Şifre politikası uzunluk tabanlıdır | T342, T517 | | |
| 02:§3.13.16 Bu grubun kimlik doğrulama kuralları firma yöneticisi hesaplarına da aynen ve aynı değerlerle uygulanır | T342, T520, T523 | | |
| 02:§3.13.17 Giriş, şifre sıfırlama, hesap kaydı ve doğrulama bağlantısının yeniden istenmesi limitlidir | T342, T215 | | |
| 02:§3.13.18 Kayıtta ve hesapta pazarlama onayı yoktur | T341, T514 | | |
| 02:§3.13.19 Hesap kaydı ve Google ile ilk giriş veri toplayan girişlerin kapısına bağlıdır | T341, T331 | | |
| 02:§3.13.20 Her giriş denemesi giriş kaydına yazılır | T341, T108 | | |
| 02:§3.13.21 Hesap bir ad taşır | T341, T517 | | |
| 02:§3.14 Adres | T344 | | |
| 02:§3.14.1 Üye müşterinin adres defteri vardır | T344, T509, T518 | | |
| 02:§3.14.2 Fatura adresi teslimat adresinden ayrı seçilebilir | T344, T509 | | |
| 02:§3.14.3 Adreste il serbest metin değildir; | T344, T103 | | |
| 02:§3.14.4 Siparişin istediği adresler kalemlerin tipine bağlıdır | T344, T509 | | |
| 02:§3.14.5 Sipariş, adreslerini kendi içine dondurur; | T344, T355 | | |
| 02:§3.14.6 Adresin alanları | T344, T509, T518 | | |
| 02:§3.15 Hesabın silinmesi ve kişisel veri başvurusu | T345 | | |
| 02:§3.15.1 Hesap silindiğinde hesap kapanır, sipariş kaydı yasal saklama süresi boyunca durur | T345, T309, T517 | | |
| 02:§3.15.2 Yürüyen sipariş hesap silmeyi engellemez | T345 | | |
| 02:§3.15.3 Silinmiş bir hesabın siparişleri hiçbir hesaba bağlanmaz; | T345 | | |
| 02:§3.15.4 Ürünün sunduğu başvuru kanalı iletişim formudur | T345, T365 | | |
| 02:§3.15.5 Uygulama içi veri indirme yoktur | T345, T533 | | |
| 02:§3.15.6 E-posta değişikliğini geri alma bağlantısı açıkken hesap silinmez | T345, T517, T533 | | |
| 02:§3.16 Sepet | T346, T347 | | |
| 02:§3.16.1 Üyenin sepeti hesabında durur ve her cihazda aynıdır; misafirin sepeti bulunduğu tarayıcıya bağlıdır | T346, T508 | | |
| 02:§3.16.2 Sepet kendiliğinden boşalmaz | T346 | | |
| 02:§3.16.3 Giriş anında iki sepet birleşir; aynı varyant toplanmaz | T346, T508 | | |
| 02:§3.16.4 Birleştirme, müşterinin o ziyarette görmediği bir ürünü ya da adedi sepete eklediyse müşteriye bir kez söylenir | T346, T508 | | |
| 02:§3.16.5 Sepet kalemi canlıdır | T346, T508 | | |
| 02:§3.16.6 Vitrinde görünen ama satın alınamayan kalem sepette "Tükendi" işaretiyle kalır ve siparişe girmez | T346, T508 | | |
| 02:§3.16.7 Vitrinden kalkan kalem sepetten çıkar ve müşteriye bir kez söylenir | T346, T508 | | |
| 02:§3.16.8 Stoktan fazla adet sepete eklenmez ve stok adedi söylenmez | T347, T503, T508 | | |
| 02:§3.16.9 Firma bir üründen bir siparişte alınabilecek adedi sınırlayabilir | T347, T503, T525 | | |
| 02:§3.16.10 Sonradan aşılan sınırda kalemler sepette mesajıyla kalır | T347, T508 | | |
| 02:§3.16.11 Sepetteki kalem sayısına tavan yoktur | T347 | | |
| 02:§3.16.12 Sepet, ödeme başarılı olduğunda boşalır | T347, T348 | | |
| 02:§3.16.13 İstek listesi ve sepet hatırlatması yoktur | T347 | | |
| 02:§3.17 Sipariş onayı ve stok ayırma | T348 | | |
| 02:§3.17.1 Sipariş, müşterinin ödeme adımında siparişi onayladığı anda oluşur | T348, T329, T509, T510 | | |
| 02:§3.17.2 Onaylanan özet ile oluşan sipariş birebir aynıdır | T348, T509 | | |
| 02:§3.17.3 Sipariş onaylandığında kalemlerin adedi stoktan ayrılır ve müşteriye bir ödeme süresi tanınır | T348 | | |
| 02:§3.17.4 Ödeme süresi yönteme göre ayrıdır | T348, T324, T536 | | |
| 02:§3.17.5 Ödeme süresi dolan ya da kart ödemesi başarısız olan sipariş kendiliğinden iptal olur | T348, T324 | | |
| 02:§3.17.6 Başarısız ödemenin tekrarı yeni bir siparişle olur | T348, T510 | | |
| 02:§3.17.7 Müşteri aynı sepetten yeni bir siparişi onayladığında, o sepete bağlı ödenmemiş önceki siparişi kendiliğinden iptal edilir | T348 | | |
| 02:§3.17.8 Ödemesi hiç başarılı olmamış ve kendiliğinden iptal edilmiş sipariş — ödeme süresi dolmuş, kart ödemesi başarısız olmuş ya da aynı sepetten yeni… | T348, T215, T511 | | |
| 02:§3.18 Asgari sipariş tutarı | T349 | | |
| 02:§3.18.1 Firma isteğe bağlı tek bir asgari sipariş tutarı girebilir; | T349, T508, T509, T536 | | |
| 02:§3.18.2 Karşılaştırma tabanı ücretsiz kargo eşiğininkiyle aynıdır | T349 | | |
| 02:§3.18.3 Asgari tutar yalnız en az bir fiziksel kalem içeren siparişe uygulanır | T349 | | |
| 02:§3.19 Kargo ücreti ve ücretsiz kargo eşiği | T349 | | |
| 02:§3.19.1 Kargo ücreti sipariş başına sabittir | T349, T536 | | |
| 02:§3.19.2 Kargo ücreti KDV dahil tek tutar olarak girilir; KDV'si siparişteki fiziksel kalemlerin oranını izler | T349, T536 | | |
| 02:§3.19.3 Firma isteğe bağlı bir ücretsiz kargo eşiği girebilir | T349, T536 | | |
| 02:§3.19.4 Eşiğin tabanı kupondan önceki tutardır | T349 | | |
| 02:§3.19.5 Eşik tanımlıysa ve siparişte fiziksel kalem varsa, sepette ve ödeme özetinde eşiğe kalan tutar yazar | T349, T508, T509 | | |
| 02:§3.19.6 Kargo ücreti ve ücretsiz kargo kararı sipariş düzleminde donar; | T349, T355 | | |
| 02:§3.20 Teslim: bölge, yol, kargoya verme ve takip | T350, T351 | | |
| 02:§3.20.1 Firma teslimat yaptığı illeri seçer; varsayılan seçim tüm Türkiye'dir | T350, T509, T536 | | |
| 02:§3.20.2 Tek teslimat yolu kargodur | T350 | | |
| 02:§3.20.3 Sipariş tek parça gider ve tek takip numarası taşır; | T350 | | |
| 02:§3.20.4 Firmanın teslimat sözü kargoya verme süresidir | T350, T503, T536 | | |
| 02:§3.20.5 Kargoya verme süresinin firma varsayılanı vardır; ürün kendi süresini taşıyabilir | T350, T525, T536 | | |
| 02:§3.20.6 Kargoya verme süresi ödemenin onaylandığı anda başlar | T350, T325 | | |
| 02:§3.20.7 Sipariş "kargoya verildi" olarak işaretlenirken ya kargo şirketi + takip numarası girilir ya da "kendi aracımızla teslim" seçeneği işaretlenir; | T350, T310, T527 | | |
| 02:§3.20.8 Kargo şirketi ürünle gelen bir listeden seçilir; | T350, T310 | | |
| 02:§3.20.9 Kalemin teslimi tipine göre işler | T351, T531 | | |
| 02:§3.20.10 Karışık siparişte sevkiyat tek eksende kalır ve iptal edilmemiş fiziksel kalem kaldığı sürece fiziksel hattı izler; | T351, T316 | | |
| 02:§3.20.11 Fiziksel teslimi firma işaretler ve teslim tarihini kendisi girer | T351, T527 | | |
| 02:§3.20.12 Fiziksel kalemi olmayan sipariş kargo adımlarından geçmez | T351, T316 | | |
| 02:§3.21 Ödeme | T352, T353 | | |
| 02:§3.21.1 Satıcı firmadır; platform ticari zincirde yer almaz | T352, T401 | | |
| 02:§3.21.2 İki ödeme yöntemi vardır: kart ve havale/EFT | T352, T536 | | |
| 02:§3.21.3 Her kart ödemesi istisnasız 3D Secure doğrulamasından geçer; | T352, T406 | | |
| 02:§3.21.4 Kart yönteminin panelde açık/kapalı anahtarı yoktur | T352, T401 | | |
| 02:§3.21.5 Havale/EFT panelden açılıp kapatılır; açıkken IBAN zorunludur | T353, T536 | | |
| 02:§3.21.6 Havale ile siparişi onaylayan müşteri ekranda firmanın siparişe donan IBAN'ını (§3.21.5) ve sipariş numarasını görür; | T353, T307 | | |
| 02:§3.21.7 Firma havalede gelen tutarı sisteme girmez | T353 | | |
| 02:§3.21.8 Süresi dolup iptal edilmiş bir sipariş sonradan "ödendi" işaretlenemez | T353, T317 | | |
| 02:§3.21.9 Ürün taksiti bilmez | T352 | | |
| 02:§3.21.10 Sipariş hazırlığa ancak ödeme onaylandığında geçer | T352, T316 | | |
| 02:§3.21.11 Ters ibraz (harcama itirazı) ürünün dışındadır | T352, T529 | | |
| 02:§3.22 Sipariş numarası ve takip erişimi | T354 | | |
| 02:§3.22.1 Sipariş numarası tahmin edilemez bir koddur | T354 | | |
| 02:§3.22.2 Numara sipariş oluştuğu anda verilir ve sipariş iptal edilse bile tekrar kullanılmaz | T354 | | |
| 02:§3.22.3 Sipariş sayfasına üç yoldan girilir | T354, T511, T215 | | |
| 02:§3.22.4 Sipariş sayfası her bilgiyi taşır | T354, T511 | | |
| 02:§3.22.5 Sipariş e-postasındaki bağlantı, misafir alıcının ve hesabı silinmiş müşterinin siparişinde, sipariş numarasından ayrı, siparişe özgü ve tahmin… | T354, T511 | | |
| 02:§3.23 Sipariş anında donan değerler | T355 | | |
| 02:§3.23.1 Sipariş kalemi, ödenen tutarı üreten her değeri ve ürünün o anki tanımını dondurur | T355 | | |
| 02:§3.23.2 Donan her değer sipariş kaleminde düz olarak yaşar | T355 | | |
| 02:§3.23.3 Sipariş, teslimat ve fatura adreslerini, iletişim e-postasını, firmanın o günkü yasal kimliğini (§3.1.3'ün tipe göre kimlik alanları, adres,… | T355, T530 | | |
| 02:§3.23.4 Donma anı siparişin oluştuğu andır | T355 | | |
| 02:§3.23.5 Marka ayarları siparişe donmaz | T355 | | |
| 02:§3.24 Onay adımı ve yasal metinler | T356 | | |
| 02:§3.24.1 Sipariş iki ayrı onay kutusuyla onaylanır | T356, T509, T544 | | |
| 02:§3.24.2 Sepette dijital kalem varsa üçüncü bir kutu çıkar | T356, T509, T544 | | |
| 02:§3.24.3 Ön Bilgilendirme Formu onay özetiyle birebir aynıdır | T356, T366 | | |
| 02:§3.24.4 Yaş için ayrı beyan istenmez | T356 | | |
| 02:§3.24.5 Misafir alıcının e-postası doğrulama koduyla sınanmaz | T356, T509 | | |
| 02:§3.24.6 Onaylanan metinlerin sürümü ve firmanın kimliği siparişe donar | T356, T355 | | |
| 02:§3.24.7 Ödeme adımında müşteri notu alanı yoktur | T356 | | |
| 02:§3.24.8 Sitede "İşlem rehberi" başlıklı sabit bir sayfa vardır | T356, T507 | | |
| 02:§3.25 Fatura | T357 | | |
| 02:§3.25.1 Ürün fatura kesmez; e-Arşiv ve e-Fatura entegrasyonu yoktur | T357, T524 | | |
| 02:§3.25.2 Fatura sistemde saklanmaz ve gösterilmez | T357 | | |
| 02:§3.25.3 Dışa aktarmada siparişin donmuş firma kimliği kullanılır, | T357, T524 | | |
| 02:§3.26 İptal, cayma, iade ve ayıp talebi | — | | |
| 02:§3.27 Kurumsal içerik | T358, T359, T360 | | |
| 02:§3.27.1 Kurumsal içerik hazır tiplerle ve düzeni sabit bir genel sayfayla girilir; | T358 | | |
| 02:§3.27.2 Beş hazır tip vardır: Hakkımızda, hizmet tanıtımı, referans iş, sık sorulan soru ve şube; | T358 | | |
| 02:§3.27.3 Hakkımızda tek kayıttır | T358, T534 | | |
| 02:§3.27.4 Hizmet tanıtımı fiyatsızdır ve satın alınmaz | T358, T505 | | |
| 02:§3.27.5 Referans iş, firmanın yaptığı bir işi anlatır | T358, T505 | | |
| 02:§3.27.6 Sık sorulan soru, soru ve cevaptan oluşur; | T358, T506 | | |
| 02:§3.27.7 Şubeler İletişim sayfasında listelenir; | T358, T506 | | |
| 02:§3.27.8 İletişim sayfası ayrı bir içerik tipi değildir | T358, T506 | | |
| 02:§3.27.9 Genel sayfa başlık, metin ve görsellerden oluşur, düzeni sabittir ve adet sınırı yoktur | T358, T534 | | |
| 02:§3.27.10 Hizmet tanıtımı ve referans iş katalogdan ürün bağlayabilir | T359, T505 | | |
| 02:§3.27.11 Hizmet tanıtımı, referans iş, SSS, şube ve genel sayfa listelerini firma panelde elle sıralar; | T359, T534 | | |
| 02:§3.27.12 Firma isteğe bağlı sosyal medya hesap bağlantıları ve bir WhatsApp numarası girer | T358, T534 | | |
| 02:§3.27.13 Hiçbir kurumsal içerik tipi zorunlu değildir; | T359, T534 | | |
| 02:§3.27.14 Hakkımızda boşken ya da yayında değilken ana sayfanın kurumsal bloğu marka adını ve logoyu gösterir; | T359, T502 | | |
| 02:§3.27.15 Kurumsal içeriğin görselleri ve kısa alanları ürünün kurallarına tabidir | T359, T534 | | |
| 02:§3.27.16 Uzun metinler §3.11.6'nın metin biçimi setini kullanır | T359, T339 | | |
| 02:§3.27.17 Kurumsal içerikte gömülü video oynatıcı yoktur | T359 | | |
| 02:§3.27.18 Kurumsal içeriğe dosya eki (PDF vb.) eklenmez | T359 | | |
| 02:§3.27.19 Müşteri görüşleri ayrı bir içerik tipi değildir | T359 | | |
| 02:§3.27.20 Blog yoktur | T359 | | |
| 02:§3.27.21 Duyuru şeridi vardır: tek, kısa, tarihli | T360, T534 | | |
| 02:§3.27.22 Site boş kurulur | T360 | | |
| 02:§3.27.23 Her içerik kaydı kendi yayın durumunu taşır: Taslak ya da Yayında; | T360, T322 | | |
| 02:§3.27.24 Taslak kayıt yalnız adla ya da başlıkla — SSS'de soruyla — kaydedilebilir; yayına almak §3.27.13'ün zorunlu alanlarını ister | T360, T534 | | |
| 02:§3.27.25 Taslak içeriğin sayfası ziyaretçiye "sayfa bulunamadı" döner; giriş yapmış yöneticiye sayfayı "Taslak" bandıyla gösterir | T360, T505 | | |
| 02:§3.27.26 İçerikte sürüm geçmişi yoktur | T360 | | |
| 02:§3.27.27 Kurumsal içerik kalıcı olarak silinebilir; silme geri alınamaz işlem uyarısıyla yapılır | T360, T534 | | |
| 02:§3.27.28 Kurumsal içerikte ileri tarihli yayın yoktur; | T360 | | |
| 02:§3.28 Ana sayfa ve menü | T361 | | |
| 02:§3.28.1 İki hazır ana sayfa düzeni vardır ve firma panelden birini seçer: tanıtım öncelikli ya da mağaza öncelikli | T361, T535, T502 | | |
| 02:§3.28.2 Ana sayfanın kurumsal bloğu her zaman Hakkımızda'dır | T361, T502 | | |
| 02:§3.28.3 Hizmet tanıtımı ve referans iş bir "ana sayfada göster" işareti taşır; | T361, T534, T502 | | |
| 02:§3.28.4 Menü düzenleyici yoktur | T361, T535, T537 | | |
| 02:§3.28.5 Genel sayfalar altbilgide listelenir; | T361, T537 | | |
| 02:§3.28.6 Yayında ürünü olmayan kategori menüde görünmez | T361, T333 | | |
| 02:§3.29 Marka kimliği | T362 | | |
| 02:§3.29.1 Firma görünümde dört şeyi ayarlar: logo, marka adı, site simgesi ve tek bir marka rengi | T362, T534 | | |
| 02:§3.29.2 Logo zorunlu değildir | T362, T537 | | |
| 02:§3.29.3 Marka adı unvandan ayrı ve zorunlu bir alandır; | T362, T534 | | |
| 02:§3.29.4 Site simgesi isteğe bağlıdır; | T362 | | |
| 02:§3.29.5 Marka rengi okunurluğu bozamaz | T362 | | |
| 02:§3.29.6 Sitenin altbilgisinde "Shopfolio ile kuruldu" platform imzası varsayılan olarak açıktır; firma panelden kapatabilir | T362, T534 | | |
| 02:§3.30 Sayfa adresleri, arama motorları ve paylaşım | T363 | | |
| 02:§3.30.1 Ürünün adresi kategoriye bağlı değildir; | T363 | | |
| 02:§3.30.2 Her sayfanın adresi kaydın adından üretilir ve ilk yayında sabitlenir | T363 | | |
| 02:§3.30.3 Yönlendirme sistemi yoktur | T363 | | |
| 02:§3.30.4 Var olmayan, silinmiş ve yeni bir kayda verilmemiş ya da ziyaretçiye kapalı (taslak) her adres "sayfa bulunamadı" sayfasına düşer | T363, T504 | | |
| 02:§3.30.5 Arama motoru bilgileri otomatik üretilir; | T363, T313 | | |
| 02:§3.30.6 Arama motorlarına kapalı sayfalar | T363, T313 | | |
| 02:§3.30.7 Ürün ve kurumsal içerik sayfalarında tek bir "Paylaş" düğmesi vardır | T363, T503, T505 | | |
| 02:§3.31 Panelde eşzamanlı düzenleme | T364 | | |
| 02:§3.31.1 Sessiz üzerine yazma yoktur | T364, T208 | | |
| 02:§3.31.2 Siparişe yönelik her işlem siparişin güncel hâline karşı uygulanır | T364, T527 | | |
| 02:§3.32 İletişim talebi | T365 | | |
| 02:§3.32.1 İletişim formu girişsiz açıktır | T365, T506 | | |
| 02:§3.32.2 Formun alanları | T365, T506 | | |
| 02:§3.32.3 Konu tipi kapalı beş değerden seçilir | T365, T506 | | |
| 02:§3.32.4 Talebin iki durumu vardır: Açık ve Kapatıldı | T365, T322, T532 | | |
| 02:§3.32.5 Sistem talebi kaydeder ve iletir, yazışmayı yürütmez | T365, T308, T532 | | |
| 02:§3.32.6 Müşteri talebini takip etmez | T365 | | |
| 02:§3.32.7 Form spam'e karşı iki mekanizmayla korunur | T365, T215 | | |
| 02:§3.32.8 Aydınlatma metni tamamlanmış ve yayına alınmış değilse ya da çerez politikası yayına alınmamışsa iletişim formu kapalıdır | T365, T331 | | |
| 02:§3.32.9 Ayrı bir şikâyet kanalı ve canlı destek yoktur | T365 | | |
| 02:§3.32.10 İndirme hakkını yenileme başvurusu da bu formdan, "Sipariş hakkında" tipiyle gelir | T365, T531 | | |
| 02:§3.33 Yasal metinler | T366 | | |
| 02:§3.33.1 Dört yasal metin vardır | T366, T507 | | |
| 02:§3.33.2 Firmanın düzenlediği iki metin zorunludur ve boşaltılamaz | T366, T507 | | |
| 02:§3.33.3 Sürüm kendiliğinden artar | T366 | | |
| 02:§3.33.4 Metin değiştiğinde yeniden onay alınmaz | T366 | | |
| 02:§3.33.5 Aydınlatma metninin bağlantısı yeni bir kişisel veri alanı açan her ekranda görünür | T366, T540 | | |
| 02:§3.33.6 Ön Bilgilendirme Formu ve Mesafeli Satış Sözleşmesi müşteriye e-postayla da gönderilir | T366, T306 | | |
| 02:§3.33.7 Uyuşmazlık bilgisi iki metnin zorunlu parçasıdır | T366 | | |
| 02:§3.33.8 Ayarlarla çelişen serbest metin firmanın sorumluluğundadır | T366 | | |
| 02:§3.33.9 Dört metin tamamlanmadan satış açılmaz; | T366, T331 | | |
| 02:§3.34 Site geneli: cihaz, tarayıcı, erişilebilirlik ve süreklilik | T540, T327, T118 | | |
| 02:§3.34.1 Tek site, duyarlı tasarım | T540 | | |
| 02:§3.34.2 Vitrin mobil önceliklidir, panel masaüstü önceliklidir; panelde hiçbir işlev mobilde kapatılmaz | T540 | | |
| 02:§3.34.3 Tarayıcı tabanı son iki büyük sürümdür | T540 | | |
| 02:§3.34.4 En dar desteklenen ekran genişliği bir sayıdır; | T540 | | |
| 02:§3.34.5 Erişilebilirliğin tabanı Aile ve Sosyal Hizmetler Bakanlığı'nın "Web Siteleri ve Mobil Uygulamaların Erişilebilirliği Kontrol Listesi — A Seviyesi"… | T540, T543 | | |
| 02:§3.34.6 Sitede ziyaretçi ölçümü yoktur | T540 | | |
| 02:§3.34.7 Ürün çalışma süresi taahhüdü vermez | T327 | | |
| 02:§3.34.8 Sipariş tarafında veri kaybı toleransı sıfırdır | T118, T327 | | |
| 02:§4.1 Sayım kuralları | T324, T325, T337, T338, T350, T353, T360, T378 | | |
| 02:§4.1.1 Tek saat dilimi Türkiye saatidir | T378, T101, T216 | | |
| 02:§4.1.2 İş günü pazartesi–cumadır, resmî tatiller hariç | T378, T350, T353 | | |
| 02:§4.1.3 İki süre iş günüyle, gerisi takvim günüyle sayılır | T378, T324, T325 | | |
| 02:§4.1.4 Gün olarak belirlenen süre, başlangıç gününün ertesi günü başlar ve son günün 23:59'unda dolar | T378, T337, T338, T360 | | |
| 02:§4.1.5 Üç süre çitlidir: | T378, T350, T353, T536 | | |
| 02:§4.2 Süreler | T324, T325, T326, T301, T302, T201 | | |
| 02:Z-1 E-posta doğrulama bağlantısının ömrü | T324, T341 | | |
| 02:Z-2 Şifre sıfırlama bağlantısının ömrü | T324, T342, T343 | | |
| 02:Z-3 Yönetici daveti bağlantısının ömrü | T324, T106, T520 | | |
| 02:Z-4 Kısa oturum — "beni hatırla" işaretsiz | T324, T342 | | |
| 02:Z-5 Uzun oturum — "beni hatırla" işaretli | T324, T342 | | |
| 02:Z-6 Sepetin ömrü | T324, T346 | | |
| 02:Z-7 Kart ödeme süresi — stok ayırma bu süre boyunca sürer | T324, T348, T352, T311 | | |
| 02:Z-8 Havale ödeme süresi — stok ayırma bu süre boyunca sürer | T324, T348, T353, T327 | | |
| 02:Z-9 Havale ödeme hatırlatması | T324, T353, T306 | | |
| 02:Z-10 Kargoya verme süresi — firmanın vitrindeki sözü | T325, T350 | | |
| 02:Z-11 Yasal teslim üst sınırı — kişiye özel üretilen mallar dışında | T325, T350, T368 | | |
| 02:Z-12 Teslim tarihi | T325, T351 | | |
| 02:Z-39 İfa süresi — hizmet kalemi; firmanın hizmetteki sözü | T325, T332, T355, T367, T368 | | |
| 02:Z-13 Cayma penceresi — fiziksel kalem | T325, T371, T351 | | |
| 02:Z-14 Cayma penceresi — hizmet kalemi | T325, T371 | | |
| 02:Z-15 Cayma hakkı — dijital kalem | T325, T371 | | |
| 02:Z-16 Geri ödeme — cayma | T325, T370, T373 | | |
| 02:Z-42 Müşterinin iade malını gönderme süresi | T325, T373 | | |
| 02:Z-17 Geri ödeme — iptal edilen ödenmiş sipariş ya da kalem | T325, T370 | | |
| 02:Z-47 İfanın imkânsızlaştığının bildirimi | T325, T368, T370, T306 | | |
| 02:Z-18 Ayıp talebinin iki yıllık zamanaşımı | T325, T374 | | |
| 02:Z-19 İndirme hakkı | T325, T340 | | |
| 02:Z-20 İndirimin tarih aralığı | T326, T337 | | |
| 02:Z-21 Referans fiyatın geriye bakış penceresi | T326, T337 | | |
| 02:Z-22 Kuponun tarih aralığı | T326, T338 | | |
| 02:Z-23 Duyurunun tarih aralığı | T326, T360 | | |
| 02:Z-24 İleri tarihli yayın | T326, T335, T360 | | |
| 02:Z-25 KVKK başvurusuna cevap | — | | |
| 02:Z-26 Gönderilemeyen e-postanın yeniden denenmesi | T326, T305 | | |
| 02:Z-27 Deneme limitlerinin penceresi | T326, T377 | | |
| 02:Z-28 Sipariş, ödeme ve fatura verisi; sözleşme ve ön bilgilendirme kayıtları; siparişe donmuş yasal… | T301 | | |
| 02:Z-41 Ödemesi hiç alınmamış (Başarısız) siparişin kişisel verisi | T301 | | |
| 02:Z-29 Ayıp talebi kaydı | T302, T374 | | |
| 02:Z-30 İşlem izi | T302, T106 | | |
| 02:Z-31 Hesap verisi — giriş bilgileri, şifre, açık oturumlar, adres defteri | T345 | | |
| 02:Z-32 Sepet | T345 | | |
| 02:Z-33 İletişim talebi — KVKK talebi dahil | T302, T365 | | |
| 02:Z-34 Bildirim gönderim kaydı — hesap ve yönetim e-postaları ile firmaya giden bildirimler; yeniden… | T302, T305 | | |
| 02:Z-43 Bildirim gönderim kaydı — siparişe bağlı müşteri bildirimleri (B-1…B-16); yeniden denemeler ve… | T302, T301, T305 | | |
| 02:Z-35 Havale hattında müşterinin geri ödeme için girdiği IBAN | T302, T370 | | |
| 02:Z-36 Periyodik imha | T201 | | |
| 02:Z-40 Giriş kaydı ve sistem kayıtları | T302, T341 | | |
| 02:Z-44 İmha kaydı — her periyodik imha çalışmasının ve kişisel veriyi hemen silen her işlemin (hesabın… | T201 | | |
| 02:Z-45 E-posta değişikliğini geri alma bağlantısının ömrü | T302, T343 | | |
| 02:Z-46 Girişte tanınan tarayıcı işareti | T302, T377 | | |
| 02:Z-37 Veri ihlalinde Kurul'a bildirim | — | | |
| 02:Z-38 Malı dönmeyen caymada havale hattı IBAN'ının kendiliğinden silinmesi | T327, T370, T373 | | |
| 02:§4.3 Kendiliğinden işleyen anlar | T327, T348, T370 | | |
| 02:§5.1 Ürün ve varyantın yayın durumu | T321, T335 | | |
| 02:§5.2 Kurumsal içeriğin yayın durumu | T322, T360, T335 | | |
| 02:§5.3 Sipariş: iki eksen ve oluşma anı | T315, T316, T317, T318, T345, T348, T355, T376 | | |
| 02:§5.3.1 Sipariş iki bağımsız eksende durum taşır: | T315, T318 | | |
| 02:§5.3.2 Sipariş, müşteri onayladığı anda Alındı + Bekliyor olarak doğar | T348, T355, T316, T317 | | |
| 02:§5.3.3 Onay anında özet değişmişse sipariş hiç doğmaz | T348 | | |
| 02:§5.3.4 Her ödeme denemesi ayrı bir sipariştir | T348, T376 | | |
| 02:§5.3.5 Sipariş hesabın kaderinden bağımsızdır: | T345 | | |
| 02:§5.4 Sevkiyat ekseni — Sipariş durumu | T315, T316, T319, T320 | | |
| 02:§5.5 Ödeme ekseni — Ödeme durumu | T315, T317, T319, T370 | | |
| 02:§5.6 İki eksenin bağı ve kendiliğinden geçişler | T311, T316, T317, T318, T324, T348, T372 | | |
| 02:§5.6.1 İki eksen tek bir kapı koşuluyla bağlıdır: | T318, T316, T317 | | |
| 02:§5.6.2 Yalnız dijital siparişte ödemenin onayı sevkiyatı doğrudan Teslim edildi'ye alır | T318, T316 | | |
| 02:§5.6.3 Ödeme süresi dolduğunda ya da kart ödemesi başarısız olduğunda — hangisi önce gelirse — sipariş kendiliğinden İptal edildi, ödemesi Başarısız olur; | T348, T318, T317, T324, T311 | | |
| 02:§5.6.4 Müşteri aynı sepetten yeni bir siparişi onayladığında o sepete bağlı ödenmemiş önceki sipariş kendiliğinden iptal edilir | T348 | | |
| 02:§5.6.5 Ödenmeden iptal edilen siparişin ödemesi Başarısız olur | T317, T318 | | |
| 02:§5.6.6 Cayma beyanı kendi başına hiçbir ekseni değiştirmez: | T318, T372 | | |
| 02:§5.7 Tipe göre sevkiyat hattı | T318, T350 | | |
| 02:§5.8 Sipariş kalemi düzeyindeki kayıtlar | T320, T351, T367, T368, T372, T373, T374 | | |
| 02:§5.9 Geri alınamaz geçişler | T319, T367, T368, T372, T345 | | |
| 02:§5.10 Ayıp talebinin durumu | T320, T374 | | |
| 02:§5.11 İndirim ve kuponun zamana ve kullanıma bağlı geçerliliği | T337, T338, T355, T367 | | |
| 02:§5.11.1 İndirim tarih aralığında kendiliğinden işler: | T337 | | |
| 02:§5.11.2 Kupon, tarih aralığının içinde ve kullanım hakkı kaldıkça geçerlidir | T338 | | |
| 02:§5.11.3 Kuponun kullanım hakkının ömrü: | T338, T367 | | |
| 02:§5.11.4 Kuponun kalemlere dağıtılan payı sipariş anında donar | T338, T355 | | |
| 02:§5.12 Hesap ve oturum | T341, T342, T343, T376 | | |
| 02:§5.12.1 Hesabın durum makinesi yoktur | T341 | | |
| 02:§5.12.2 Oturum "beni hatırla" seçimine göre kısa ya da uzun ömürlü açılır | T342, T343 | | |
| 02:§5.12.3 Deneme limiti hesabın durumunu değiştirmez | T376 | | |
| 02:§5.13 İletişim talebinin durumu | T322, T365 | | |
| 02:§6.1 Bütün akışlara uygulanan ilkeler | T305, T306, T307, T311, T324, T331, T346, T348, T352, T354, T356, T375, T376, T377 | | |
| 02:§6.1.1 Hiçbir sipariş ya da talep akışı bir e-postanın ulaşmasına bağlı değildir | T305, T306, T307, T354, T375, T511, T531 | | |
| 02:§6.1.2 Sistem, müşterinin ödediği bir siparişi kendi bilgisizliği yüzünden iptal etmez | T348, T352, T311, T324 | | |
| 02:§6.1.3 Onaylanan özet ile oluşan sipariş birebir aynıdır | T348, T356 | | |
| 02:§6.1.4 Sepet hatalarda korunur | T346, T352, T331 | | |
| 02:§6.1.5 Limit aşımı nötr konuşur | T376, T377, T541 | | |
| 02:§6.1.6 Hata ve uyarının anlamı yalnız renkle taşınmaz; | T540, T541 | | |
| 02:§6.2 Satın alma (akış 1) | T346, T347, T348, T349, T350, T352, T353, T334, T338, T340, T331, T376, T356, T354 | | |
| 02:§6.3 Sipariş takibi (akış 2) | T354, T375, T376, T306, T307, T348 | | |
| 02:§6.4 İptal, cayma ve iade (akış 3) | T367, T368, T369, T370, T371, T372, T373, T374, T351, T350, T316 | | |
| 02:§6.5 Üyelik (akış 4) | T341, T342, T343, T345, T376, T309, T366 | | |
| 02:§6.6 Katalog yönetimi (akış 5) | T333, T334, T335, T336, T337, T338, T340, T350, T364 | | |
| 02:§6.7 Sipariş yürütümü (akış 6) | T305, T306, T316, T317, T319, T340, T351, T353, T364, T368, T375 | | |
| 02:§6.8 Kurumsal içerik (akış 7) | T358, T359, T360, T361, T364, T365 | | |
| 02:§6.9 Mağaza ayarları (akış 8) | T330, T331, T350, T352, T353, T362, T366, T106, T520 | | |
| 02:§7.1 Üç yol ve ortak ilkeler | T306, T307, T365, T366, T367, T369, T371, T372, T374 | | |
| 02:§7.1.1 Müşterinin satın almadan sonra başvurabileceği üç ayrı yol vardır | T367, T371, T374 | | |
| 02:§7.1.2 Tek akış tüketici akışıdır | T371 | | |
| 02:§7.1.3 İptal ve iade kalem düzeyinde ve adetle işler | T369, T367 | | |
| 02:§7.1.4 Üç yol da sipariş sayfasından kullanılır; | T367, T372, T374, T306, T307, T511 | | |
| 02:§7.1.5 İtirazın sistemdeki yeri | T365, T366 | | |
| 02:§7.1.6 Kalemin işlem birimi adettir; kalem parça parça kapanabilir (K-787, K-790) | T369, T367, T372 | | |
| 02:§7.1.7 İşleme giren adetlerin tutarı kalemin donmuş değerlerinden ayrılır (K-788) | T369 | | |
| 02:§7.2 İptal | T306, T307, T316, T325, T334, T338, T340, T349, T350, T351, T353, T367, T368, T369, T370 | | |
| 02:§7.2.1 Müşteri fiziksel kalemi, sipariş "Kargoya verildi"ye geçene kadar kendi iptal eder | T367, T316 | | |
| 02:§7.2.2 İptal kalem ve adet düzeyindedir | T367, T316 | | |
| 02:§7.2.3 Teslimat gecikmesinde fesih iki yoldan işler | T368, T350, T325 | | |
| 02:§7.2.4 Firma da iptal edebilir | T368, T306, T307 | | |
| 02:§7.2.5 Dijital kalem ödeme onayından sonra iptal edilemez: | T367, T340, T351 | | |
| 02:§7.2.6 İptalde sayaçlar kendiliğinden döner | T367, T334, T338 | | |
| 02:§7.2.7 Kısmi iptalde kupon yeniden hesaplanmaz | T367, T369 | | |
| 02:§7.2.8 İptal edilen ödenmiş kalemin parası, ödemenin geldiği yoldan geri ödenir | T370, T369, T353 | | |
| 02:§7.2.9 İptalde kargo ücreti sebebe ve kapsama göre ayrılır | T369, T349 | | |
| 02:§7.3 Cayma | T306, T307, T318, T325, T330, T332, T351, T356, T367, T371, T372 | | |
| 02:§7.3.1 Cayma penceresi on dört gündür ve sabittir; | T371, T351, T325 | | |
| 02:§7.3.2 Dijital üründe cayma hakkı yoktur; koşulu ayrı bir onaydır | T371, T356 | | |
| 02:§7.3.3 Hizmette cayma hakkı, müşterinin onayıyla başlayan ifa tamamlanınca düşer | T371, T356, T351 | | |
| 02:§7.3.4 Fiziksel üründe cayma istisnası ürün düzleminde isteğe bağlı bir işarettir | T371, T332, T356 | | |
| 02:§7.3.5 Cayma sipariş sayfasından bildirilir | T372, T330, T306, T307 | | |
| 02:§7.3.6 Cayma beyanı siparişi kapatmaz | T372, T318 | | |
| 02:§7.3.7 Kargoya verilmemiş fiziksel kalemde tüketicinin yolu iptaldir | T372, T367 | | |
| 02:§7.4 İade ve geri ödeme | T307, T325, T330, T334, T338, T349, T356, T366, T369, T370, T371, T373, T374 | | |
| 02:§7.4.1 Geri ödemenin on dört günü, cayma beyanı kalemin teslim tarihinden önce yapıldıysa ya da kalem hizmetse beyandan, teslimden sonra yapıldıysa iade… | T370, T373, T325 | | |
| 02:§7.4.2 Cayma hâlinde iade kargo bedeli firmaya aittir; | T373, T356 | | |
| 02:§7.4.3 Ayıplı malda iade kargo bedeli her hâlükârda firmaya aittir | T373, T374 | | |
| 02:§7.4.4 Müşteri malı firmanın iade adresine karşı ödemeli gönderir | T373, T330, T356, T366 | | |
| 02:§7.4.5 Geri ödeme, ödemenin geldiği yoldan gider | T370, T543 | | |
| 02:§7.4.6 Kısmi iadede kargo ücreti ve ücretsiz kargo kararı yeniden hesaplanmaz | T369, T349, T373 | | |
| 02:§7.4.7 İade edilen fiziksel kalemin adedi stoğa kendiliğinden eklenmez | T373, T334 | | |
| 02:§7.4.8 Caymada hizmet kontenjanı cayılan adet kadar havuza döner; kuponun kullanım hakkı yalnız siparişin tamamı — bütün kalemlerinin bütün adetleri — iptal… | T373, T334, T338 | | |
| 02:§7.4.9 Koşullu istisna kaleminde koruyucu ambalajı açılmış dönen mal reddedilebilir | T373, T371, T307 | | |
| 02:§7.4.10 Kullanılmış ya da hasar görmüş dönen malda geri ödeme tam tutarla yapılır; değer kaybı talebi ürünün dışındadır | T373 | | |
| 02:§7.5 Ayıp talebi | T320, T325, T340, T370, T374 | | |
| 02:§7.5.1 Ayıp talebi üçüncü yoldur | T374, T325 | | |
| 02:§7.5.2 Kanal sipariş sayfasındaki "sorun bildir"dir | T374, T513 | | |
| 02:§7.5.3 Sistem talebi kaydeder ve iletir, çözümü yürütmez | T374, T320, T370 | | |
| 02:§7.5.4 Ayıplı dijital üründe doğal çözüm dosya güncellemesidir | T374, T340 | | |
| 02:§7.6 Geri alma sınırları | T316, T317, T319, T345, T353, T360, T372, T375 | | |
| 02:§7.6.1 Panel "geri al" düğmesi sunmaz | T319 | | |
| 02:§7.6.2 Süresi dolup iptal edilmiş siparişe gelen ödeme işlenemez; | T353, T317 | | |
| 02:§7.6.3 Müşteri akışında iptal edilmiş ya da teslim edilmiş sipariş yeniden açılmaz; | T316, T319 | | |
| 02:§7.6.4 Kurumsal içerikte eski hâle dönülmez ve silme geri alınmaz: | T360 | | |
| 02:§7.6.5 Silinmiş bir hesabın siparişleri hiçbir hesaba geri bağlanmaz | T345 | | |
| 02:§7.6.6 Yöneticinin sipariş düzeltmeleri ileri yönlü kayıtlardır ve tutarı değiştirmez | T375 | | |
| 02:§7.6.7 Cayma beyanı geri alınmaz ve düzeltilmez | T372, T375 | | |
| 02:§8.1 Kimlik doğrulama tabanı | T341, T342, T343, T375, T376 | | |
| 02:§8.1.1 Doğrulanmamış hesapla giriş ve sipariş yoktur | T341, T342 | | |
| 02:§8.1.2 Bir siparişin ya da bir giriş yolunun hesaba bağlanmasının tek dayanağı doğrulanmış e-postadır | T341, T343, T375 | | |
| 02:§8.1.3 Bağlantılar süreli ve tek kullanımlıktır | T341, T342, T343, T520 | | |
| 02:§8.1.4 Ekranlar hesabın varlığını kayıtta, ödeme adımının hatırlatmasında ve hesabın e-posta değişikliğinde ele verir | T341, T342, T376 | | |
| 02:§8.1.5 Üç işlem oturum açıkken de yeniden doğrulama ister: | T342, T343 | | |
| 02:§8.1.6 Şifre politikası uzunluk tabanlıdır | T342 | | |
| 02:§8.1.7 Yönetici tarafı aynı rejimdedir | T342, T106, T520 | | |
| 02:§8.2 Deneme limitleri | T338, T341, T342, T343, T348, T354, T365, T376, T377 | | |
| 02:L-1 Başarısız giriş denemesi — şifreyle; yeniden doğrulamadaki başarısız şifre denemesi de (§3.13.13;… | T376, T342 | | |
| 02:L-2 Şifre sıfırlama talebi | T376, T342 | | |
| 02:L-3 Hesap kaydı | T376, T341 | | |
| 02:L-4 İletişim formu gönderimi | T376, T365 | | |
| 02:L-5 Başarısız misafir sipariş takibi sorgusu | T376, T354 | | |
| 02:L-6 Geçersiz kupon kodu denemesi | T376, T338 | | |
| 02:L-7 Kendiliğinden iptal edilen ödenmemiş sipariş | T376, T348 | | |
| 02:L-8 Aynı anda açık ödenmemiş sipariş | T376, T348 | | |
| 02:L-9 Doğrulama bağlantısının yeniden istenmesi | T376, T341, T343 | | |
| 02:§8.2.1 Limit aşıldığında yalnız o işlem geçici olarak engellenir | T376 | | |
| 02:§8.2.2 İki eksen ayrı sayılır | T377 | | |
| 02:§8.2.3 L-7 tekrarı ölçer, tek denemeyi değil | T377, T348 | | |
| 02:§8.2.4 Yönetici de aynı limitlere aynı değerlerle tabidir; | T377, T342 | | |
| 02:§8.2.5 Sepet ve stok tarafında ürün düzeyinde limit yoktur | T377 | | |
| 02:§8.2.6 Girişin e-posta ekseni tanınan tarayıcıyı engellemez | T377, T342 | | |
| 02:§8.3 Form ve site güvenliği | T307, T352, T354, T365, T368, T372, T375, T377 | | |
| 02:§8.3.1 İletişim formu üçüncü taraf captcha kullanmaz | T365, T215 | | |
| 02:§8.3.2 Siteye kod eklenmez | T209, T540 | | |
| 02:§8.3.3 Sitede üçüncü taraf gömme içerik yoktur: | T209, T540, T352 | | |
| 02:§8.3.4 Ürün kart verisini görmez, saklamaz, loglamaz | T352, T202 | | |
| 02:§8.3.5 Sipariş erişimi tahmin edilemez anahtarlara dayanır | T354, T377 | | |
| 02:§8.3.6 Misafir siparişinin e-postası yalnız firmanın teyidiyle değişir | T375, T354, T307 | | |
| 02:§8.3.7 Siparişin adresi yalnız firmanın teyidiyle düzeltilir | T375, T307 | | |
| 02:§8.3.8 Başka kanaldan gelen cayma bildirimi yalnız firmanın teyidiyle işlenir | T372, T368, T307 | | |
| 02:§8.4 Veri asgariliği | T301, T302, T341, T356, T365, T370, T373 | | |
| 02:§8.4.1 Ürün yalnız işlediği kişisel veriyi toplar | T356, T365, T341, T540 | | |
| 02:§8.4.2 IBAN yalnız gerektiğinde istenir ve iş bitince silinir | T370, T373, T201 | | |
| 02:§8.4.3 Serbest metin alanları kayda geçen bir risk taşır | T301, T356, T531 | | |
| 02:§8.4.4 Çerezler yalnız zorunludur ve — ödeme sağlayıcısının çerçevesi dışında — birinci taraftır; onay bandı yoktur | T209, T540 | | |
| 02:§8.4.5 Saklama süreleri §4'te, imhanın biçimi ve kişisel veri yükümlülükleri §12'dedir | T201, T301, T302 | | |
| 02:§8.5 Kayıtlar ve veri ihlali | T302, T341 | | |
| 02:§8.5.1 Ürün ayrı bir ihlal kaydı tutmaz | T341, T302, T106, T201 | | |
| 02:§8.5.2 İşlem izi on yıllık saklama süresi boyunca değiştirilemez ve silinemez; | T106, T302, T201 | | |
| 02:§9.1 Kanal ve ilkeler | T201, T304, T305, T328, T330, T362, T382, T385, T407, T511, T531, T601 | | |
| 02:§9.1.1 Tek kanal e-postadır | T305, T328 | | |
| 02:§9.1.2 Hiçbir sipariş ya da talep akışı bir e-postanın ulaşmasına bağlı değildir | T305, T328, T511 | | |
| 02:§9.1.3 İşlem bildirimleri ticari elektronik ileti değildir ve kapatılamaz | T305, T328 | | |
| 02:§9.1.4 Gönderen ve yanıt | T305, T330, T362 | | |
| 02:§9.1.5 Çalışan bir e-posta gönderim yapılandırması her kurulumun dış ön koşuludur; | T407, T601 | | |
| 02:§9.1.6 Gönderilemeyen e-posta üç kez, artan aralıkla yeniden denenir | T385, T304, T407, T382, T531, T201 | | |
| 02:§9.2 Müşteriye giden bildirimler | T305, T306, T307, T311, T316, T340, T348, T350, T353, T367, T368, T370, T372, T373, T374, T375 | | |
| 02:B-1 Sipariş alındı — firma misafir siparişinin e-postasını düzelttiğinde yeni adrese yeniden gider (§10.… | T306, T305, T348, T375 | | |
| 02:B-2 Havale/EFT seçildi | T306, T353 | | |
| 02:B-3 Havale ödeme hatırlatması — havale "ödendi" işaretinin düzeltilmesiyle yeniden başlayan sürede bir k… | T306, T353, T375 | | |
| 02:B-4 Ödeme onaylandı | T306, T311, T340 | | |
| 02:B-5 Kargoya verildi | T306, T350 | | |
| 02:B-6 Teslim edilemedi | T306, T316 | | |
| 02:B-7 İptal edildi — sipariş ya da tek bir kalem | T306, T367, T368 | | |
| 02:B-8 Geri ödeme işlendi — sistemin iptal edilmiş siparişe gelen kart ödemesini kendiliğinden geri ödemesi… | T306, T370 | | |
| 02:B-9 Cayma beyanı, gecikme feshi ya da ayıp talebi alındı — firmanın başka kanaldan gelen cayma ya da gec… | T307, T372, T368, T374 | | |
| 02:B-10 Teslimat ya da fatura adresi düzeltildi | T307, T375 | | |
| 02:B-11 Kalem çıkarıldı ya da adedi azaltıldı | T307, T375 | | |
| 02:B-12 Kargo şirketi ya da takip numarası düzeltildi | T307, T375 | | |
| 02:B-13 Yanlış durum geçişi düzeltildi | T307, T375, T353 | | |
| 02:B-14 Havale hattında IBAN silindikten sonra iade edilen mal firmaya ulaştı — firma teslim alma adımını iş… | T307, T370, T373, T372 | | |
| 02:B-15 Firma misafir siparişinin e-postasını düzeltti (§10.4.11) — eski adrese gider | T307, T375 | | |
| 02:B-16 İade reddedildi — firma koşullu istisna kaleminde koruyucu ambalajı açılmış dönen malı reddetti (§7.… | T307, T373 | | |
| 02:§9.3 Firmaya giden bildirimler | T308, T330, T348, T353, T365, T370, T372, T374, T379, T382, T385 | | |
| 02:F-1 Yeni sipariş | T308, T348 | | |
| 02:F-2 Yeni iletişim talebi | T308, T365 | | |
| 02:F-3 Yeni cayma beyanı, gecikme feshi ya da ayıp talebi — müşterinin çözülmüş ayıp talebini yeniden açmas… | T308, T372, T374 | | |
| 02:F-4 Sistemin kendiliğinden yaptığı karta geri ödeme — iptal edilmiş siparişe gelen ödemenin geri ödenmes… | T308, T370 | | |
| 02:F-5 Havale/EFT'nin IBAN'ı panelde girildi ya da değiştirildi — bütün yöneticilerin kendi adreslerine gid… | T308, T353 | | |
| 02:F-6 Yönetici daveti gönderildi ya da bir yönetici kaldırıldı — bütün yöneticilerin kendi adreslerine gid… | T308, T379 | | |
| 02:§9.3.1 Firmaya giden bildirimler firma kimliğindeki iletişim e-postasına gider | T308, T330, T382 | | |
| 02:§9.3.2 Firmanın kendi yaptığı işlem kendisine bildirilmez; | T308, T382 | | |
| 02:§9.3.3 Havale IBAN'ının değişikliği bütün yöneticilere bildirilir (F-5) | T308, T353, T385 | | |
| 02:§9.3.4 Yönetici daveti gönderildiğinde ya da bir yönetici kaldırıldığında bütün yöneticilere bildirim gider… | T308, T379, T385 | | |
| 02:§9.4 Hesap ve yönetim e-postaları | T309, T341, T342, T343, T345, T379 | | |
| 02:§9.5 Olmayan bildirimler | T328, T308, T365, T305 | | |
| 02:§10.1 Panelin kapsamı | T308, T323, T330, T364, T375, T379, T381, T522, T540, T547, T601 | | |
| 02:§10.1.1 Panel tek firmayı yönetir; firma seçimi yoktur | T330 | | |
| 02:§10.1.2 Yöneticinin yapabildikleri ve yapamadıkları | T323, T547, T379, T375, T522 | | |
| 02:§10.1.3 Parametreler üç katmandadır | T381, T601 | | |
| 02:§10.1.4 Panelde hiçbir işlev mobilde kapatılmaz; | T364, T540 | | |
| 02:§10.1.5 Firmaya giden bildirimler | T308 | | |
| 02:§10.2 Yönetici hesapları | T303, T308, T309, T324, T341, T342, T343, T379, T380, T520, T601 | | |
| 02:§10.2.1 İlk yönetici kurulumda doğar, sonrakiler davetle açılır | T379, T303, T309, T520 | | |
| 02:§10.2.2 Bir yönetici başka bir yöneticiyi kaldırabilir; son yönetici kaldırılamaz ve hiç kimse kendi hesabın… | T379, T308, T380 | | |
| 02:§10.2.3 Bütün yöneticiler aynı yetkiye sahiptir | T379 | | |
| 02:§10.2.4 Kimlik doğrulama müşteri tarafıyla aynıdır | T342, T343, T379, T380 | | |
| 02:§10.2.5 Yönetici hesabının sepeti ve siparişi olmaz | T379, T341, T342, T343 | | |
| 02:§10.2.6 Kullanılmamış davet geri çekilebilir; bir adres için aynı anda tek geçerli davet vardır | T379, T308, T380, T324 | | |
| 02:§10.2.7 Panele erişimin kaybında kurtarma ürünün dışındadır ve kurulum tarafının işidir | T379, T303, T601 | | |
| 02:§10.3 İşlem izi | T106, T201, T341, T345, T375, T379, T380, T381, T384, T522 | | |
| 02:§10.3.1 İz sekiz alanda tutulur | T380, T379, T381, T384, T375, T345 | | |
| 02:§10.3.2 İz saklama süresi boyunca (§10.3.5) değiştirilemez ve silinemez | T380, T106 | | |
| 02:§10.3.3 Sistem olayları ize yazılmaz | T380, T201, T341 | | |
| 02:§10.3.4 İz panelde düz bir liste olarak görünür; | T380, T522 | | |
| 02:§10.3.5 İz on yıl saklanır; | T380, T201 | | |
| 02:§10.4 Sipariş müdahaleleri | T301, T307, T316, T317, T319, T334, T341, T350, T351, T353, T354, T355, T367, T368, T370, T371, T372, T373, T375, T380, T382, T383, T386 | | |
| 02:§10.4.1 Dokuz müdahale vardır | T375, T386, T380 | | |
| 02:§10.4.2 Adres yalnız "Kargoya verildi"ye kadar düzeltilir | T375, T355, T307 | | |
| 02:§10.4.3 Kalem adedi azaltılabilir ya da kalem çıkarılabilir; kalem eklenemez ve fiyat değiştirilemez | T386, T367, T370, T319, T334 | | |
| 02:§10.4.4 Kargo şirketi ve takip numarası düzeltilebilir; | T386, T350, T307 | | |
| 02:§10.4.5 Teslim tarihi düzeltilebilir; | T386, T351, T371, T373 | | |
| 02:§10.4.6 Yanlış yapılmış durum geçişi yeni bir geçişle düzeltilir | T319, T316, T317, T353, T334, T307 | | |
| 02:§10.4.7 İç not müşteriye hiçbir yerde görünmez | T386, T301 | | |
| 02:§10.4.8 Firma iptalinin sebep listesi §7.2.4'te, | T368, T319 | | |
| 02:§10.4.9 Malı dönmemiş cayma kalemi "mal dönmedi" gerekçesiyle kapatılabilir | T373, T370, T382, T383 | | |
| 02:§10.4.10 Başka bir kanaldan gelen cayma bildirimi panelden kaydedilir | T372, T368, T370, T307 | | |
| 02:§10.4.11 Misafir siparişinin iletişim e-postası müşterinin talebi üzerine düzeltilir | T375, T354, T341, T307 | | |
| 02:§10.5 Manuel adımlar ve bütçe | T318, T357 | | |
| 02:§10.5.1 Sipariş başına sistem içi zorunlu elle adımlar | T318 | | |
| 02:§10.5.2 Koşullu adımlar — her biri bir adım | T318 | | |
| 02:§10.5.3 Fatura sipariş başına bir sistem dışı adımdır | T357 | | |
| 02:§10.5.4 Bütçe hat başınadır: tek tipli bir siparişin hattında sistem içi zorunlu elle adım üçü geçmez | T318 | | |
| 02:§10.5.5 Hacim varsayımı | — | | |
| 02:§10.6 Bekleyen işler ve satış özeti | T348, T350, T353, T370, T373, T382, T383, T385 | | |
| 02:§10.6.1 Panel ana sayfası bekleyen işleri sayar | T382, T385, T370, T373, T353 | | |
| 02:§10.6.2 Panelde bir satış özeti vardır ve analitik değildir | T383 | | |
| 02:§10.6.3 Ölçülerin tanımı | T383, T348, T350 | | |
| 02:§10.7 Toplu veri işlemleri | T302, T341, T357, T384, T525 | | |
| 02:§10.7.1 Siparişler dışa aktarılır | T384, T357, T302 | | |
| 02:§10.7.2 Ürün içe aktarma ve toplu güncelleme yoktur | T525, T384 | | |
| 02:§10.7.3 Katalog ve içerik için ürün içinde dışa aktarma yoktur; | T384 | | |
| 02:§10.7.4 Sipariş dışındaki kişilere ulaşma listesi de dışa aktarılır | T384, T341 | | |
| 02:§10.8 İlk kurulum kontrol listesi | T331, T360, T366, T382 | | |
| 02:§10.8.1 Site boş kurulur; | T360 | | |
| 02:§10.8.2 Panel ana sayfasında bir kurulum kontrol listesi durur | T382, T331, T366 | | |
| 02:§11.1 Firma ayarları | T312, T324, T325, T327, T332, T335, T336, T340, T347, T348, T349, T350, T353, T355, T381, T503, T508, T509, T511, T525, T536 | | |
| 02:P-1 Kargo ücreti — sipariş başına, KDV dahil | T381, T349, T536 | | |
| 02:P-2 Kaldırıldı — kargo ücretinin KDV'si fiziksel kalemlerin oranını izler, firma ayarı değildir (§3.19.2… | — | | |
| 02:P-3 Ücretsiz kargo eşiği | T381, T349, T508, T509, T536 | | |
| 02:P-4 Asgari sipariş tutarı | T381, T349 | | |
| 02:P-5 Kargoya verme süresi — firma varsayılanı | T381, T325, T350, T503, T525, T536 | | |
| 02:P-6 Kargoya verme süresi — ürün düzeyi | T381, T350, T525, T536 | | |
| 02:P-7 Havale ödeme süresi | T381, T324, T327, T348, T353, T536 | | |
| 02:P-8 İndirme hakkı — sipariş kalemi başına | T381, T312, T340, T355, T511 | | |
| 02:P-9 Bir siparişte en fazla adet — ürün başına | T381, T347, T503, T525 | | |
| 02:P-10 KDV oranı varsayılanı | T381, T336, T525 | | |
| 02:P-11 Kaldırıldı — duyuru metni firma ayarı değil, kurumsal içerik kaydıdır (§3.27.21); kimlik yeniden kul… | — | | |
| 02:P-44 İfa süresi — hizmet ürünü | T381, T325, T332, T335, T340, T525 | | |
| 02:§11.2 Ürün sabitleri | T106, T108, T201, T215, T302, T305, T311, T312, T324, T326, T327, T332, T333, T338, T339, T340, T341, T342, T343, T348, T352, T353, T354, T358, T359, T360, T362, T365, T370, T373, T376, T377, T381, T385, T409, T506, T511, T514, T515, T516, T517, T519, T520, T525, T526, T534, T536, T540, T543 | | |
| 02:P-12 E-posta doğrulama bağlantısının ömrü | T381, T324, T341, T514 | | |
| 02:P-13 Şifre sıfırlama bağlantısının ömrü — Google girişinin teyit bağlantısı da bu ömrü taşır | T381, T324, T342, T343, T409, T516 | | |
| 02:P-14 Yönetici daveti bağlantısının ömrü | T381, T106, T324, T520 | | |
| 02:P-47 E-posta değişikliğini geri alma bağlantısının ömrü | T381, T302, T343, T517, T519 | | |
| 02:P-15 Kısa oturum — "beni hatırla" işaretsiz | T381, T324, T342, T515 | | |
| 02:P-16 Uzun oturum — "beni hatırla" işaretli | T381, T324, T342, T515 | | |
| 02:P-48 Girişte tanınan tarayıcı işaretinin ömrü | T381, T302, T342, T377 | | |
| 02:P-17 Asgari şifre uzunluğu | T381, T342, T517 | | |
| 02:P-18 Kart ödeme süresi | T381, T311, T324, T348, T352, T536 | | |
| 02:P-19 Sipariş numarasının rastgele blok uzunluğu | T381, T354 | | |
| 02:P-20 Kategori ağacının derinliği | T381, T333, T526 | | |
| 02:P-21 Ürün başına seçenek boyutu | T381, T332, T525 | | |
| 02:P-22 Kabul edilen görsel biçimleri | T381, T339, T359, T362, T525, T534 | | |
| 02:P-23 Tek görsel dosyasının boyut tavanı | T381, T339, T359, T362, T525, T534 | | |
| 02:P-24 Ürün başına görsel adedi — ürünün ve varyantlarının görselleri birlikte | T381, T339, T525 | | |
| 02:P-25 Kurumsal içerik kaydı başına görsel adedi | T381, T359, T534 | | |
| 02:P-26 Dijital ürün dosyasının boyut tavanı | T381, T312, T340 | | |
| 02:P-27 Ürün adının ve kurumsal içerikte ad ya da başlığın karakter tavanı | T381, T339, T359, T525, T534 | | |
| 02:P-28 SSS sorusunun, kısa açıklamanın ve şubenin çalışma saatlerinin karakter tavanı | T381, T358, T359, T506, T534 | | |
| 02:P-29 Hakkımızda kısa tanıtımının karakter tavanı | T381, T358, T359, T534 | | |
| 02:P-30 Duyuru metninin karakter tavanı | T381, T360, T534 | | |
| 02:P-31 L-1 Başarısız giriş denemesi | T381, T215, T342, T376 | | |
| 02:P-32 L-2 Şifre sıfırlama talebi | T381, T215, T342, T376 | | |
| 02:P-33 L-3 Hesap kaydı | T381, T215, T341, T342, T376 | | |
| 02:P-34 L-4 İletişim formu gönderimi | T381, T215, T365, T376 | | |
| 02:P-35 L-5 Başarısız misafir sipariş takibi sorgusu | T381, T215, T354, T376, T511 | | |
| 02:P-36 L-6 Geçersiz kupon kodu denemesi | T381, T338, T376, T526 | | |
| 02:P-37 L-7 Kendiliğinden iptal edilen ödenmemiş sipariş | T381, T215, T348, T376, T511 | | |
| 02:P-46 L-8 Aynı anda açık ödenmemiş sipariş | T381, T215, T348, T353, T376, T511 | | |
| 02:P-49 L-9 Doğrulama bağlantısının yeniden istenmesi | T381, T341, T343, T376, T514 | | |
| 02:P-38 Gönderilemeyen e-postanın yeniden deneme aralıkları | T381, T385, T305, T326 | | |
| 02:P-39 Periyodik imha aralığı | T381, T201 | | |
| 02:P-40 İletişim talebinin saklama süresi | T381, T302, T365 | | |
| 02:P-41 Bildirim gönderim kaydının saklama süresi — hesap ve yönetim e-postaları ile firmaya giden bildiriml… | T381, T302, T305 | | |
| 02:P-43 Malı dönmeyen caymada havale hattı IBAN'ının saklama süresi | T381, T327, T370, T373, T543 | | |
| 02:P-45 Giriş kaydının ve sistem kayıtlarının saklama süresi | T381, T106, T108, T201, T302, T341 | | |
| 02:P-42 En dar desteklenen ekran genişliği | T381, T540 | | |
| 02:§11.3 Hacim kabulleri | T383 | | |
| 02:H-1 Katalog | — | | |
| 02:H-2 Sipariş hacmi | T383 | | |
| 02:H-3 Sepet | — | | |
| 02:H-4 Eşzamanlı ziyaretçi | — | | |
| 02:§12.1 Ticari çerçeve ve tüketici rejimi | T216, T305, T306, T328, T330, T336, T337, T348, T350, T352, T355, T356, T357, T366, T368, T370, T371, T372, T373, T374, T384, T401, T501, T503, T535 | | |
| 02:§12.1.1 Tek pazar Türkiye'dir; | T216, T330 | | |
| 02:§12.1.2 Ürün tüketiciye satış için kurulmuştur | T356, T366 | | |
| 02:§12.1.3 Satıcı firmadır | T352, T401 | | |
| 02:§12.1.4 ETBİS kaydı ve yasal kimlik | T330, T535 | | |
| 02:§12.1.5 Fiyat tüketiciye KDV dahil gösterilir | T336, T501, T503 | | |
| 02:§12.1.6 İndirim beyanı referans fiyata dayanır | T337, T355 | | |
| 02:§12.1.7 Mesafeli satış | T356, T366, T348, T355, T305, T306 | | |
| 02:§12.1.8 Teslim | T368, T350, T370 | | |
| 02:§12.1.9 Cayma, iade ve ayıp | T371, T372, T373, T374, T370 | | |
| 02:§12.1.10 Fatura ürünün dışındadır | T357, T384 | | |
| 02:§12.1.11 Ticari elektronik ileti gönderilmez | T328, T305 | | |
| 02:§12.1.12 Ürün ne satıldığını denetlemez | — | | |
| 02:§12.2 Kişisel verilerin korunması | T201, T209, T301, T302, T331, T341, T342, T345, T346, T356, T365, T366, T370, T375, T377, T379, T380, T384, T401, T409, T507, T601 | | |
| 02:§12.2.1 Veri sorumlusu firmadır | T366, T601 | | |
| 02:§12.2.2 Aydınlatma yükümlülüğü | T366, T507, T356 | | |
| 02:§12.2.3 Hukuki sebepler KVKK m.5/2'nin kapalı sayımıyla yazılır; açık rıza alınmaz | T366, T301, T302 | | |
| 02:§12.2.4 Ürün hiçbir noktada açık rıza toplamaz | T356, T366 | | |
| 02:§12.2.5 Çerezler | T366, T331, T346, T209, T342, T377 | | |
| 02:§12.2.6 İletişim formunun "KVKK talebi" tipi ürünün sunduğu başvuru kanalıdır | T365, T345 | | |
| 02:§12.2.7 Saklama ve imha | T201, T301, T302, T345, T370, T375 | | |
| 02:§12.2.8 Kişisel verinin yurt dışına aktarımı | T366, T209, T401, T409, T601 | | |
| 02:§12.2.9 Veri ihlali | T380, T384, T341 | | |
| 02:§12.2.10 Yönetim tarafında veri erişimi ayrımı yoktur | T379 | | |
| 02:§12.2.11 Kurumsal içerikte başkasına ait kişisel veriyi yayımlamanın sorumluluğu firmadadır | — | | |
| 02:§12.2.12 Veri asgariliği | T356, T365, T341 | | |
| 02:§12.3 Veri sahipliği ve süreklilik | T327, T384, T601 | | |
| 02:§12.3.1 Veri firmanındır | T601 | | |
| 02:§12.3.2 Çıkış hakkı barındırma düzleminde karşılanır | T384, T601 | | |
| 02:§12.3.3 Ürün çalışma süresi taahhüdü vermez ve sipariş tarafında veri kaybına tolerans sıfırdır — tek istisn… | T327, T601 | | |
| 02:§12.4 Erişilebilirlik | T540, T543, T544 | | |
| 02:§12.4.1 Erişilebilirlik yasal bir yükümlülüktür; taban Kontrol Listesi A Seviyesi ile WCAG 2.2'dir, hedef WC… | T540, T543, T544 | | |
| 02:§12.5 Firmanın yükümlülükleri | T330, T357, T366, T370, T372, T373, T375, T384, T601 | | |

**`05`** — Teknik Mimari (O5 1/2: MK-01…MK-78 kararı 78 · `###` alt bölümü 58)

| Kaynak öğe | Task | Implement edildi | Kanıt |
|---|---|---|---|
| 05:MK-01 C# / .NET; tek uygulama, sayfalar sunucuda üretilir | T02 | | |
| 05:MK-02 ASP.NET Core; .NET 10, yalnız uzun süreli destekli (LTS) sürümler | T02, T06 | | |
| 05:MK-03 Entity Framework Core; şema değişiklikleri onun göçleriyle | T02, T101 | | |
| 05:MK-04 Razor Pages; bütün sayfalar sunucuda tam HTML, gönder–yönlendir–göster; ayrı JSON API katmanı yok | T02, T206 | | |
| 05:MK-05 Düz JavaScript modülleri ve düz CSS; ön yüz çatısı ve npm hattı yok, JavaScript yalnız iyileştirir | T02, T540, T539 | | |
| 05:MK-06 PostgreSQL 18; tek örnek, aynı sunucuda kendi konteynerinde | T05, T06, T101 | | |
| 05:MK-07 Firmanın adına kiralanan sanal sunucu; Linux konteynerleri | T05, T601 | | |
| 05:MK-08 Caddy 2 | T05 | | |
| 05:MK-09 Veritabanının yedeği pgBackRest'le, veritabanı konteynerinde; dosyaların ve sistem kayıtlarının yedeği bir… | T06, T07 | | |
| 05:MK-10 Tek Compose tanımı; her imaj etiket ve özetle sabit | T05, T06 | | |
| 05:MK-11 GitHub Container Registry | T06 | | |
| 05:MK-12 GitHub Actions; iskelet değişmez, `publish.yml` kalır, kurulumlara otomatik dağıtım yok | T04 | | |
| 05:MK-13 Tek dağıtılabilir uygulama | T218 | | |
| 05:MK-14 Sunucuda dört konteyner | T05 | | |
| 05:MK-15 Dokuz modül; modüller arası erişim yalnız açık arayüzden, eşzamanlı ve aynı işlemde; uygulama içi olay yayı… | T218 | | |
| 05:MK-16 Vitrin ve panel aynı alan adında; panel ayrı bir yol alanı | T206, T207 | | |
| 05:MK-17 Uygulamanın çağırdığı üç dış servis bir bağlantı katmanının arkasında; ürün kuralı çekirdekte; dış serviste… | T202, T203 | | |
| 05:MK-18 Adres addan sunucuda üretilir; arama motoru verileri JSON-LD ve Open Graph ile ilk yanıtta; kapalı sayfalar… | T219, T206 | | |
| 05:MK-19 Zaman UTC'de tutulur, Türkiye saatiyle sayılır; para ondalıklı sayıyla; yuvarlama tek işlevden | T227, T101 | | |
| 05:MK-20 Kayıt varyantı: iyimser eşzamanlılık, sürüm değeri, kilit yok | T228, T208 | | |
| 05:MK-21 Sipariş varyantı: sürüm ve kısa satır kilidi; kilit sırası bütün işlemlerde aynı | T228, T348 | | |
| 05:MK-22 Tek gecelik imha işi; süre veriden hesaplanır; imha kaydı ayrı tabloda | T201 | | |
| 05:MK-23 İmha fiziksel silmedir; siparişte ve ona bağlı kayıtlarda kişisel alanlar boşaltılır | T201, T301, T302 | | |
| 05:MK-24 İz tablosunda uygulama kullanıcısı yalnız ekler ve okur; süresi dolan satırı veritabanı fonksiyonu siler | T106, T380 | | |
| 05:MK-25 Sürekli günlük aktarımı | T07, T601 | | |
| 05:MK-26 Yedeğin kapsamı, yeri, iki anahtarla şifrelenmesi, her kopyada otuz günlük saklaması | T07 | | |
| 05:MK-27 Kurtarma hedefi aynı gün | T601, T07 | | |
| 05:MK-28 Dosyalar sunucunun diskinde, dosya deposu arayüzüyle; dijital dosya genel adresle sunulmaz | T223 | | |
| 05:MK-29 Yüklenen görsel yeniden kodlanır | T223, T339 | | |
| 05:MK-30 Dijital ürün dosyası parçalı yüklenir; her yükleme yeni bir dosya kaydıdır | T312, T340 | | |
| 05:MK-31 İndirme hakkı düğmeye basılıp gönderim başlayınca bir kez düşer; bir saatlik bağlantı, ömrü isteğin kabulün… | T312, T340, T531 | | |
| 05:MK-32 Önerilen stok kodu ürünün adresinden ve varyantın sırasından; kaydedilmemiş üründe o anki addan, ad boşsa ö… | T332 | | |
| 05:MK-33 Site içi arama veritabanında, sadeleştirilmiş ad üzerinde içerir eşleşmesiyle | T333 | | |
| 05:MK-34 Önbellek uygulamanın belleğinde; ayrı önbellek sunucusu yok | T229 | | |
| 05:MK-35 Durum makinesi tek geçiş biriminde; beyaz liste | T315, T319 | | |
| 05:MK-36 Onay isteği müşterinin gördüğünü taşır; onay tek işlemde, koşullu ayırmayla | T348 | | |
| 05:MK-37 İç yan etkiler geçişle aynı işlemde, dış yan etkiler veritabanındaki iş kuyruğunda, e-posta gönderim kaydın… | T220, T305 | | |
| 05:MK-38 Her e-posta gönderim kaydında bir satır olarak doğar; gövde gönderim anında üretilir | T305, T220, T117 | | |
| 05:MK-39 Tek işlenme: onay anahtarı, önce kaydedilen sağlayıcı bildirimi | T311, T348, T403 | | |
| 05:MK-40 Karta para gönderen istekten önce geri ödeme kaydı; sonucu bilinmeyen istek yeniden gönderilmez, sonuç bekl… | T370, T404, T405 | | |
| 05:MK-41 Uygulamanın içinde tek zamanlayıcı | T221 | | |
| 05:MK-42 İşlerin envanteri | T220, T221 | | |
| 05:MK-43 Hatırlatma 08:00, imha 03:00, son sorgu beş dakikada bir | T221 | | |
| 05:MK-44 Kesinti nabızla ölçülür; dönüşte havale iptali ertelenir, kaçırılmış hatırlatması düşer | T221, T327 | | |
| 05:MK-45 Oturum sunucuda tutulur; iki hesap türü iki kimlik şeması | T207, T342 | | |
| 05:MK-46 Şifre çatının özetleyicisiyle; Google ile giriş OpenID Connect'in yetki kodu akışı | T342, T343, T409 | | |
| 05:MK-47 Yetki sunucuda, varsayılan kapalı, kayıt düzeyinde süzülerek | T207, T323 | | |
| 05:MK-48 Deneme limitleri veritabanında, deneme kaydıyla; IPv6'da /64 | T215, T108 | | |
| 05:MK-49 Genel istek hızı sınırı uygulamada | T215, T203 | | |
| 05:MK-50 Anahtarlar kriptografik üreteçle | T222, T108 | | |
| 05:MK-51 E-postayla gelen bağlantının açılması işlem yapmaz; işlem ekrandaki düğmeyle | T222, T342 | | |
| 05:MK-52 Her POST sahteciliğe karşı korumadan geçer | T209 | | |
| 05:MK-53 Güvenlik başlıkları: içerik güvenlik politikası | T209 | | |
| 05:MK-54 Görselin türü içerikten; piksel tavanı; dijital dosyada tür sınırı yok, ek olarak sunulur | T223, T339 | | |
| 05:MK-55 Bütün çerezler birinci taraf, zorunlu, Secure, HttpOnly, SameSite=Lax, `__Host-` önekli | T224 | | |
| 05:MK-56 Ödeme adımında yazılanlar oturumluk, şifreli bir çerezde; sunucuda kayıt yok | T224, T356 | | |
| 05:MK-57 Çerez politikası yayına alınana kadar vitrinin yanıtlarından çerez başlıklarını çıkaran ara katman | T224 | | |
| 05:MK-58 Yalnız HTTPS ve bir yıllık HSTS; kart verisi ürüne gelmez; alan düzeyinde şifreleme yok | T209, T05 | | |
| 05:MK-59 Sırlar bileşen başına dosyalarda, yalnız kullanan konteynere; paylaşılan sır yok | T08 | | |
| 05:MK-60 Döndürme yılda bir ve olay üzerine; iki yedeğin şifreleme anahtarı döndürülmez; sırların ayrı yedeği yok | T08 | | |
| 05:MK-61 Dış çağrı süreli | T202 | | |
| 05:MK-62 Kanal uyarısı art arda beş kanal hatası ve on beş dakikada; firma bildirimleri uyarısı gönderim kaydından,… | T314, T382 | | |
| 05:MK-63 Beklenmeyen hata tek yakalama katmanında üç hâle ayrılır; sonucu belirsiz işlemler kodda adıyla işaretli | T212 | | |
| 05:MK-64 Sistem kayıtları üç kaynak | T225 | | |
| 05:MK-65 Sistem kayıtları P-45 kadar saklanır; kayıt dosyalarını imha işi siler; sunucunun kayıtları kendi döngüleriyle | T225, T201 | | |
| 05:MK-66 Kayıtları kurulumu yapan okuma komutuyla okur; sonuç şifreli iletilir ve sunucudan silinir | T387 | | |
| 05:MK-67 Canlılık ve durum uçları | T314, T01 | | |
| 05:MK-68 Dört ortam; ayrı hazırlık sunucusu yok, referans kurulum öncüdür | T05, T09 | | |
| 05:MK-69 Bir sürüm tek eser | T06 | | |
| 05:MK-70 CI adımlarının işi | T04 | | |
| 05:MK-71 Üç ayar katmanı ve tasarım değerleri; açılışta ayar denetimi | T226 | | |
| 05:MK-72 Kurulum tarafının işlemleri uygulama imajındaki yönetim komutlarıyla | T387, T388, T303 | | |
| 05:MK-73 Güncellemeyi kurulumu yapan tek komutla, önce referans kurulumda, sonra teker teker başlatır; kurulumlar ke… | T602 | | |
| 05:MK-74 Göçler genişlet–daralt | T602, T04, T101 | | |
| 05:MK-75 İşletim sistemi yamaları kendiliğinden, gerekirse 04:30'da yeniden başlatma; aylık bakım sürümü ve acil yama | T601 | | |
| 05:MK-76 Sunucuya yalnız SSH ve anahtarla, kurulum başına anahtar; açık kapılar 80, 443 ve SSH | T05, T601 | | |
| 05:MK-77 H-3: isteklerin %95'i bir saniyede; H-4: dakikada 100 farklı ziyaretçi | — | | |
| 05:MK-78 Büyüme yolunun sırası ve tetikleyicileri | — | | |
| 05:§3.1 Uygulamanın biçimi | T02, T218 | | |
| 05:§3.2 Sunucudaki bileşenler ve ağ | T05, T06 | | |
| 05:§3.3 Modüller | T218 | | |
| 05:§3.4 Modüller arası iletişim kuralı | T218 | | |
| 05:§3.5 Vitrin ve panel | T206, T207 | | |
| 05:§3.6 Dış servislerin sınırı — bağlantı katmanı | T202, T203 | | |
| 05:§3.7 Sayfa adresleri ve arama motoru verileri | T219, T313, T206 | | |
| 05:§3.8 Ön yüz ve tasarım tabanının uygulanması | T540, T539, T02 | | |
| 05:§4.1 Durum makinesinin yeri — geçiş birimi | T315, T319 | | |
| 05:§4.2 Sipariş onayı | T348 | | |
| 05:§4.3 Yan etkiler ve iş kuyruğu — olay kaybı | T220 | | |
| 05:§4.4 E-postanın gönderimi ve gönderim kaydı | T305, T220, T117 | | |
| 05:§4.5 Tek işlenme — sipariş onayı, sağlayıcı bildirimi ve son sorgu | T311, T348, T403 | | |
| 05:§4.6 Karta para gönderen isteğin tek işlenmesi | T370, T404, T405 | | |
| 05:§4.7 İşlem izi | T380, T106 | | |
| 05:§5.1 Çalıştırıcılar — zamanlayıcı ve iş kuyruğu | T220, T221 | | |
| 05:§5.2 İşlerin envanteri | T221, T201, T302 | | |
| 05:§5.3 İşlerin saatleri ve gecenin sırası | T221 | | |
| 05:§5.4 Kesintinin tespiti ve dönüşteki sıra | T221, T327 | | |
| 05:§6.1 Kimlik doğrulama ve oturum | T207, T342, T343, T409 | | |
| 05:§6.2 Yetkilendirme | T207, T323 | | |
| 05:§6.3 Deneme limitleri ve istek hızı | T215 | | |
| 05:§6.4 Anahtarlar ve tek kullanımlık bağlantılar | T222 | | |
| 05:§6.5 Form ve site güvenliği | T209 | | |
| 05:§6.6 Yüklenen dosya | T223 | | |
| 05:§6.7 Çerezler ve tarayıcıda tutulan veri | T224 | | |
| 05:§6.8 Hassas verinin taşınması ve saklanması | T209, T05, T07 | | |
| 05:§6.9 Kurulum tarafının işlemleri ve bakım erişimi | T387, T601 | | |
| 05:§6 6.x Secret yönetimi (bu alt bölüm zorunludur) | T08 | | |
| 05:§7.1 Veritabanı ve işlem bütünlüğü | T101, T228 | | |
| 05:§7.2 Zamanın ve paranın temsili | T227 | | |
| 05:§7.3 Eşzamanlılık kontrolü | T228, T208 | | |
| 05:§7.4 Saklama ve imha | T201, T301, T302 | | |
| 05:§7.5 İşlem izinin korunması | T106, T380, T201 | | |
| 05:§7.6 Yedekleme ve kurtarma | T07, T601 | | |
| 05:§7.7 Dosya depolama ve dijital teslim | T223, T312, T340 | | |
| 05:§7.8 Önerilen stok kodu | T332 | | |
| 05:§7.9 Site içi arama | T333 | | |
| 05:§7.10 Önbellek | T229 | | |
| 05:§8.1 Ortamlar | T05, T09 | | |
| 05:§8.2 Paketleme ve kayıt defteri | T06 | | |
| 05:§8.3 Dal stratejisi ve CI/CD hattı | T04, T03 | | |
| 05:§8.4 Konfigürasyon yönetimi | T226 | | |
| 05:§8.5 Yönetim komutları ve ilk kayıtlar | T303, T387, T388 | | |
| 05:§8.6 Güncellemenin kurulumlara ulaşması | T602 | | |
| 05:§8.7 Göçler ve geri alma | T602, T04, T388 | | |
| 05:§8.8 Güvenlik güncellemeleri | T601 | | |
| 05:§8.9 Sunucuya bakım erişimi | T601, T05 | | |
| 05:§9.1 Sistem kayıtları — kaynaklar, içerik ve maskeleme | T225, T230, T231 | | |
| 05:§9.2 Kayıtların saklanması ve korunması | T225, T201 | | |
| 05:§9.3 Kayıtların okuma yolu | T387 | | |
| 05:§9.4 İzleme ve uyarı | T314, T01 | | |
| 05:§10.1 Dış bağımlılıklar — "çökerse ne olur" | T204, T202 | | |
| 05:§10.2 E-posta kanalının ve firma bildirimlerinin uyarısı | T314, T382, T385 | | |
| 05:§10.3 Beklenmeyen hata | T212 | | |
| 05:§11.1 Tasarım değerleri | T226 | | |
| 05:§11.2 Bilinçli tekil bırakılanlar | — | | |
| 05:§11.3 Büyüme yolu | — | | |

**`09`** — Kodlama Kılavuzu (O5 2/2: `## n.` bölümü 25)

| Kaynak öğe | Task | Implement edildi | Kanıt |
|---|---|---|---|
| 09:§1 Temel ilkeler | — | | |
| 09:§2 Source of truth hiyerarşisi | — | | |
| 09:§3 Genel kod yazım kuralları | T02, T03, T04, T211, T226 | | |
| 09:§4 Proje yapısı ve klasör organizasyonu | T02, T05, T06 | | |
| 09:§5 Adlandırma konvansiyonları | T101, T120, T206, T213, T224 | | |
| 09:§6 Mimari sınırlar | T03, T218 | | |
| 09:§7 Zaman ve biçim standartları | T216, T227, T378 | | |
| 09:§8 Hata yönetimi | T205, T212, T213, T214 | | |
| 09:§9 Domain kuralları | T106, T201, T301, T315, T348, T380 | | |
| 09:§10 Veri erişimi | T101, T118, T119, T208, T210, T228, T229 | | |
| 09:§11 Entegrasyon kuralları | T202, T203, T305, T401, T407, T409 | | |
| 09:§12 Sözleşme ve olay sürümleme | T203, T220, T311, T538 | | |
| 09:§13 Arka plan iş kuralları | T220, T221, T303, T387, T388 | | |
| 09:§14 Hassas hesaplama kuralları | T227, T336, T369, T370, T405 | | |
| 09:§15 Güvenlik kuralları | T207, T209, T215, T222, T223, T224, T312 | | |
| 09:§16 İstemci tarafı kurallar | T539, T543, T544, T548 | | |
| 09:§17 Yardımcı servis / sidecar standartları (varsa) | T06, T07, T08 | | |
| 09:§18 Loglama ve gözlemlenebilirlik | T225, T230, T231, T314 | | |
| 09:§19 Test kuralları | T03, T04, T120, T329, T538, T546, T547 | | |
| 09:§20 Performans kuralları | T210, T220, T223, T229 | | |
| 09:§21 Git ve CI/CD kuralları | T04, T06, T09 | | |
| 09:§22 Refactor kuralları | — | | |
| 09:§23 Yasaklar | T03, T04 | | |
| 09:§24 AI çalışma kuralları | — | | |
| 09:§25 Review checklist | — | | |

**`10`** — MVP Kapsamı (O5 2/2: KP satırı 77 · KD satırı 64 · alt bölüm 2)

| Kaynak öğe | Task | Implement edildi | Kanıt |
|---|---|---|---|
| 10:KP-1 Ziyaretçi kategori ağacında gezer; alt dal ürünleri listelenir, boş kategori menüde görünmez | T333, T361, T502 | | |
| 10:KP-2 Ürün adında arama; büyük-küçük harfe ve Türkçe karaktere duyarsız | T333, T503 | | |
| 10:KP-3 Fiyat aralığı ve stok durumu süzgeci; sabit düzen, en yeni önce | T210, T333, T503 | | |
| 10:KP-4 Varyant seçimi (en çok iki boyut), ürün başına tek kart, seçilemeyen kombinasyon | T332, T334, T503 | | |
| 10:KP-5 Ürün sayfası: görsel, KDV dahil fiyat, tarih, üretim yeri, yerli logo, birim fiyat, referans fiyat | T332, T336, T337, T339, T501, T503 | | |
| 10:KP-6 Üç ürün tipi (fiziksel, dijital, hizmet) tek sepette; hizmetin ifa süresi | T332, T351, T356, T503 | | |
| 10:KP-7 Arşivlenmiş, taslak ve silinmiş ürün adresi sayfaları | T321, T335, T363, T503, T504 | | |
| 10:KP-8 Hesapsız sipariş; misafir e-postası onay özetinde, firma düzeltmesi | T341, T348, T356, T386, T509 | | |
| 10:KP-9 Sepete ekleme: güncel fiyat, adet sınırları, dijital ürün uyarısı | T340, T346, T347, T508 | | |
| 10:KP-10 Üye ve misafir sepeti, girişte birleşme, kendiliğinden boşalmama | T346, T508 | | |
| 10:KP-11 Kupon kodu girişi; sıfıra indirememe | T338, T348, T508 | | |
| 10:KP-12 Teslimat ve fatura adresi, adres defteri, alan kümesi | T344, T509, T543 | | |
| 10:KP-13 Kargo ücreti, ücretsiz eşik, asgari tutar, kapalı il | T344, T349, T350, T509 | | |
| 10:KP-14 Onay özeti, Ön Bilgilendirme Formu, onay kutuları, sürüm değişince sipariş oluşmaz | T348, T356, T366, T509 | | |
| 10:KP-15 Kart (3D Secure) ve havale/EFT ile ödeme; yarım kalan ödeme, geç gelen ödemenin geri ödenmesi | T311, T348, T352, T353, T370, T401, T509, T510 | | |
| 10:KP-16 Siparişin stoğunun, kontenjanın ve kupon hakkının ayrılması | T334, T338, T348 | | |
| 10:KP-17 Sipariş numarası ve sipariş anında donan değerler | T348, T354, T355, T375 | | |
| 10:KP-18 Sipariş takibi: üç giriş yolu, durum, geri ödeme, takip, indirme, IBAN | T317, T354, T511 | | |
| 10:KP-19 Kargoya verilme süresi vitrinde, kargo şirketi ve takip bağlantısı | T310, T350, T511 | | |
| 10:KP-20 Dijital ürünün indirilmesi: hak, güncel dosya | T312, T340, T511 | | |
| 10:KP-21 Müşteri iptali ve gecikme nedeniyle fesih | T367, T368, T369, T370, T512 | | |
| 10:KP-22 Cayma: pencere, beyan, iade gönderimi, geri ödeme süresi, istisnalar | T369, T370, T371, T372, T373, T513 | | |
| 10:KP-23 Ayıp talebi: kalem, süre, çözüm, yeniden açma | T320, T374, T513 | | |
| 10:KP-24 Tek kalemde ve adet düzeyinde iptal ya da iade; kupon payı | T367, T369, T373 | | |
| 10:KP-25 Ad, e-posta ve şifreyle kayıt; doğrulanmadan giriş yok | T331, T341, T366, T514 | | |
| 10:KP-26 Google ile giriş ve hesap eşleme | T343, T409, T515 | | |
| 10:KP-27 Uzun oturum, şifre, ad ve e-posta değişikliği, geri alma, yeniden doğrulama | T342, T343, T344, T515, T516, T517, T519 | | |
| 10:KP-28 Misafir siparişlerinin hesaba düşmesi | T341, T375 | | |
| 10:KP-29 Hesabın silinmesi (müşteri ve firma); ticari kayıt kalır | T345, T517, T533 | | |
| 10:KP-30 Kişisel veri başvurusu: iletişim formunun "KVKK talebi" tipi | T345, T365, T506 | | |
| 10:KP-31 Ana sayfada tanıtım ve vitrin; düzen seçimi | T361, T502, T535 | | |
| 10:KP-32 Hakkımızda, hizmet, referans iş, SSS, genel sayfa; ilgili ürünler | T358, T359, T505 | | |
| 10:KP-33 İletişim sayfası: bilgiler, şubeler, harita bağlantısı, form | T358, T506 | | |
| 10:KP-34 İletişim formundan talep gönderme | T308, T365, T506 | | |
| 10:KP-35 Yasal kimlik ve iletişim bilgileri, ETBİS bandı, altbilgi bağlantıları | T330, T356, T366, T537 | | |
| 10:KP-36 Sosyal bağlantılar, WhatsApp, duyuru şeridi, paylaş düğmesi | T358, T360, T363, T537 | | |
| 10:KP-37 Arama motoru verileri, kapalı sayfalar, sabit adresler | T219, T313, T363 | | |
| 10:KP-38 Satış kapalıyken vitrin yayında, sepet ve ödeme kapalı | T331, T508, T541 | | |
| 10:KP-39 Ürün tanımı ve yayına alma: varyant, stok, fiyat, KDV, ölçü, ifa süresi | T332, T334, T335, T336, T525 | | |
| 10:KP-40 Ürün ve varyant durumları, önizleme, kalıcı silme | T321, T335, T525 | | |
| 10:KP-41 Kategori ağacı kurma, birden çok kategori, silme sınırı | T333, T526 | | |
| 10:KP-42 Ürün görselleri, alternatif metin, kapalı metin biçimi seti | T223, T312, T339, T525 | | |
| 10:KP-43 İndirim, referans fiyat ve kupon tanımı | T337, T338, T525, T526 | | |
| 10:KP-44 Dijital ürün dosyası ve indirme hakkının yenilenmesi | T312, T340, T525, T531 | | |
| 10:KP-45 Cayma hakkı istisnası işareti ve kapalı sebep listesi | T371, T525 | | |
| 10:KP-46 Sipariş listesi, havale ödendi, kargoya verme, teslim, teslim edilemedi, hizmet tamamlandı | T316, T318, T319, T350, T353, T527 | | |
| 10:KP-47 Firma iptali, kart ve havale geri ödemesi, iade teslim alma, cayma bildirimi kaydı | T368, T370, T372, T373, T528, T529 | | |
| 10:KP-48 Siparişe müdahale: adres, kalem, tarih, durum düzeltmesi, iç not, e-posta düzeltmesi | T375, T386, T530, T531 | | |
| 10:KP-49 Ayıp talebi ve iletişim talebinin çözülmesi, kapatılması, yeniden açılması | T365, T374, T532 | | |
| 10:KP-50 Panel ana sayfasında altı sayaç ve kanal uyarıları | T314, T382, T521 | | |
| 10:KP-51 Manuel adım bütçesi: en çok üç sistem içi adım | T318 | | |
| 10:KP-52 Siparişlerin, üyelerin ve iletişim taleplerinin dışa aktarılması | T357, T384, T524 | | |
| 10:KP-53 Satış özeti ve mağaza ölçüleri | T383, T524 | | |
| 10:KP-54 Kurumsal içerik girişi: taslak, önizleme, yayın, sıralama, ürün bağı, silme | T358, T359, T360, T534 | | |
| 10:KP-55 Ana sayfa düzeni, menü adları, sosyal bağlantılar, WhatsApp | T361, T535 | | |
| 10:KP-56 Logo, marka adı, site simgesi, marka rengi, platform imzası | T362, T534 | | |
| 10:KP-57 Duyuru şeridi: taslak, önizleme, tarih aralığı | T360, T534 | | |
| 10:KP-58 Firma kimliği: tip, zorunlu alanlar, KEP, ETBİS | T330, T535 | | |
| 10:KP-59 Havale/EFT ve IBAN, kargo ücreti, eşik, asgari tutar, iller, süreler, iade adresi | T331, T349, T350, T353, T536 | | |
| 10:KP-60 Satışın geçici kapatılması; açık siparişler etkilenmez | T331, T535 | | |
| 10:KP-61 Aydınlatma metni, çerez politikası, üretilen iki metin, sürümlü dört metin | T366, T507 | | |
| 10:KP-62 Yönetici daveti, kaldırma, ayrı hesap türü, e-posta değişikliği | T379, T520, T522, T523 | | |
| 10:KP-63 İşlem izi: kapsam, süzgeç, on yıl, değiştirilemez | T380, T522 | | |
| 10:KP-64 İşletme parametreleri: on firma ayarı, ürün sabitleri | T226, T381, T535, T536 | | |
| 10:KP-65 Kurulum kontrol listesi, boş kurulum | T382, T521 | | |
| 10:KP-66 Müşteri bildirimleri: on altı olay, e-posta içeriği | T305, T306, T307, T309, T328, T407 | | |
| 10:KP-67 Firmaya giden bildirimler ve yöneticilere giden bildirimler | T308, T309, T328 | | |
| 10:KP-68 Yanıtlanabilir e-posta, üç yeniden deneme, "ulaşmadı" işareti, yeniden gönderim | T220, T305, T385 | | |
| 10:KP-69 Duyarlı site; panelin her işi telefondan | T540, T541 | | |
| 10:KP-70 Desteklenen tarayıcı tabanı | T539, T540 | | |
| 10:KP-71 Erişilebilirlik: A Seviyesi, WCAG 2.2, yedi ayrıca sınanan kural | T540, T543, T544 | | |
| 10:KP-72 Deneme limitleri, ödenmemiş sipariş tavanı, giriş kaydı | T215, T376, T377 | | |
| 10:KP-73 Çerez onayı gerektirmeyen site: yalnız zorunlu birinci taraf çerezler | T224, T366 | | |
| 10:KP-74 Aydınlatma bağlantısı, günlük imha, imha kaydı | T201, T225, T301, T302 | | |
| 10:KP-75 Kayıtların saklama süresince kaybolmaması; yedek penceresi | T07, T118, T327 | | |
| 10:KP-76 Eşzamanlı düzenleme uyarısı; siparişte önce tamamlanan geçerli | T208, T228, T364, T542 | | |
| 10:KP-77 Salt okunur üye kaydı görünümü ve firma tarafından hesap silme | T345, T380, T533 | | |
| 10:KD-1 Fatura kesilmez ve saklanmaz; e-Arşiv, e-Fatura ve fatura yükleme yok | T357, T384 | | |
| 10:KD-2 Kargo şirketi entegrasyonu yok | T310, T350 | | |
| 10:KD-3 Havale banka hareketiyle eşleştirilmez | T353 | | |
| 10:KD-4 Kapıda ödeme yok | T352 | | |
| 10:KD-5 Ödeme adımında müşteri notu ya da teslimat talimatı yok | T509 | | |
| 10:KD-6 Ürün yorumu, puanı ve yıldızı yok | T503 | | |
| 10:KD-7 İstek listesi yok | T347 | | |
| 10:KD-8 Öneri blokları yok; ilgili ürünler firmanın elle kurduğu bağdır | T359, T503 | | |
| 10:KD-9 "Stokta haber ver" yok | T334, T503 | | |
| 10:KD-10 Sepet hatırlatma e-postası yok | T328, T346 | | |
| 10:KD-11 Facebook ile giriş yok | T343, T409 | | |
| 10:KD-12 Toplu içe aktarma ve toplu fiyat ya da stok güncelleme yok | T525 | | |
| 10:KD-13 Yönetici müşteri adına elle sipariş oluşturamaz | T375, T527 | | |
| 10:KD-14 Blog yok | T358 | | |
| 10:KD-15 Canlı destek yok | T224, T506 | | |
| 10:KD-16 SMS, anlık bildirim ve bildirim merkezi yok | T305, T328 | | |
| 10:KD-17 Pazarlama iletisi, İYS bağı ve pazarlama onayı yok | T305, T328 | | |
| 10:KD-18 Rol, yetki matrisi ve iki adımlı doğrulama yok | T207, T379 | | |
| 10:KD-19 Ziyaretçi analitiği yok | T224 | | |
| 10:KD-20 Çok dilli değil | T206, T216 | | |
| 10:KD-21 Çok kiracılı kullanım ve operatör paneli yok | T218, T330 | | |
| 10:KD-22 Mobil uygulama ve PWA yok | T539, T540 | | |
| 10:KD-23 Taksit bilgisi gösterilmez | T352, T510 | | |
| 10:KD-24 Parçalı gönderim yok | T316, T350 | | |
| 10:KD-25 Mağazadan teslim ve adlandırılmış teslimat seçenekleri yok | T350 | | |
| 10:KD-26 Kargo ücreti ağırlığa, ölçüye ya da ile göre hesaplanmaz | T349 | | |
| 10:KD-27 Teslimat bölgesi ilçe düzeyinde kısıtlanamaz | T344 | | |
| 10:KD-28 Alkol, tütün, ilaç ve silah gibi ürünlerin ek satış akışı yok | T332, T335 | | |
| 10:KD-29 Yaş için beyan kutusu ve doğum tarihi alanı yok | T356 | | |
| 10:KD-30 Arama açıklamada, kategori adında ve seçenek değerlerinde yapılmaz | T333 | | |
| 10:KD-31 Seçenek değerine göre süzgeç yok | T333 | | |
| 10:KD-32 Ziyaretçiye sıralama seçeneği ve elle sıralama yok | T210, T333 | | |
| 10:KD-33 Stok adedi ve eşik uyarısı gösterilmez | T334, T503 | | |
| 10:KD-34 Ürün sayfasında ters yönlü bağ yok | T359, T503 | | |
| 10:KD-35 Asgari adet tanımlanamaz | T332, T347 | | |
| 10:KD-36 "Verilerimi indir" düğmesi yok | T345, T517 | | |
| 10:KD-37 Üç ve üzeri seçenek boyutu ve dördüncü kategori seviyesi yok | T332, T333 | | |
| 10:KD-38 İçerik ileri tarihe zamanlanarak yayımlanamaz | T322, T335, T360 | | |
| 10:KD-39 Sayfa kurucu ve menü düzenleyici yok | T358, T361 | | |
| 10:KD-40 Kayan afiş ve kampanya görseli yok | T361, T502 | | |
| 10:KD-41 Gömülü harita, video ve sosyal medya akışı yok | T224, T358 | | |
| 10:KD-42 İçeriğe dosya eki ve müşteri dosya yüklemesi yok | T358, T365, T374 | | |
| 10:KD-43 Müşteri görüşleri ayrı tip ya da alıntı bloğu değil | T358 | | |
| 10:KD-44 Referans işte süzgeç, SSS'de başlık bölme ve şube sayfası yok | T358, T359 | | |
| 10:KD-45 Kurumsal içerikte sürüm geçmişi yok | T358, T360 | | |
| 10:KD-46 Yazı tipi, renk paleti, tema ve koyu tema seçimi yok | T362, T540 | | |
| 10:KD-47 Firma siteye kod ekleyemez | T209, T362 | | |
| 10:KD-48 Elle doldurulan SEO alanları ve elle adres düzenleme yok | T219, T363 | | |
| 10:KD-49 Adres yönlendirme ve eski siteden adres taşıma yok | T363 | | |
| 10:KD-50 İletişim talebi takibi ve panelden yanıt yok | T365, T532 | | |
| 10:KD-51 Ayrı şikâyet kanalı yok | T365, T532 | | |
| 10:KD-52 İşlem bildirimleri kapatılamaz; bildirim tercihi ekranı yok | T309, T328 | | |
| 10:KD-53 Ayarlardan üretilen bilgi sayfası ve çelişki denetimi yok | T366, T507 | | |
| 10:KD-54 Satış özeti, işlem izi, katalog ve içerik dışa aktarılamaz | T384, T524 | | |
| 10:KD-55 Üçüncü taraf çerez, izleme pikseli ve çerez bandı yok | T224, T366 | | |
| 10:KD-56 Açık rıza yönetimi yok | T201, T366 | | |
| 10:KD-57 İletişim formunda üçüncü taraf captcha yok | T209, T365 | | |
| 10:KD-58 Veri ihlali modülü ve toplu ihlal bildirimi yok | T225, T380 | | |
| 10:KD-59 Sepete ekleme ve stok yoklamada ürün düzeyinde limit yok | T215, T347 | | |
| 10:KD-60 Planlı bakım penceresi ve bakım modu yok | T314, T331 | | |
| 10:KD-61 Garanti süresi alanı ve garanti belgesi yok | T332, T374 | | |
| 10:KD-62 Ters ibraz ürüne girmez | T317, T352 | | |
| 10:KD-63 Ayrı galeri içerik tipi yok | T339, T358, T359 | | |
| 10:KD-64 İkinci ödeme sağlayıcısı desteklenmez | T202, T352, T401 | | |
| 10:§4.1 Kurulum kontrol listesi (ÖK-1…ÖK-12) | T09, T303, T330, T331, T382, T401, T407, T409, T601 | | |
| 10:§4.2 Kısıtlar ve kabuller (SK-1…SK-10) | T226, T318, T330, T332, T333, T381 | | |

**Devir satırları** — Aşama 8'den ve Aşama 1'den gelen devirler (O5 2/2: karar 9 · park maddesi 5 · gövde anması 4 · D-01 1 = 19; `11_devir_esle.py` A–F)

| Kaynak öğe | Task | Implement edildi | Kanıt |
|---|---|---|---|
| devir:K-423 Yol haritası adaylarının sıralama ölçütü | — | | |
| devir:K-424 Yol haritası adayları arası bağımlılık zincirleri | — | | |
| devir:K-1302 Okunacak 09 bölümleri ve okuma düzeni | — | | |
| devir:K-1354 Yükleme betiği, kayıt biçimleyicisi ve maskeleme testleri | T230, T231, T548 | | |
| devir:K-1355 Test projelerinde sahtelerin paylaşımı ve xUnit v3 sınaması | T03 | | |
| devir:K-1356 Eşitlik denetimlerinin Lint adımına ve yama sürümü denetiminin publish.yml'e girmesi | T02, T04 | | |
| devir:K-1357 Okunacak 09 bölümleri (çekirdek yedi bölüm) | — | | |
| devir:K-1358 Yama sürümü denetimi ve ilk yayında geri alma sınamasının atlanması | T04, T09 | | |
| devir:K-1391 Yükleme betiği, kayıt biçimleyicisi ve maskeleme testleri (açılış) | T230, T231, T548 | | |
| devir:P8.1 Park maddesi 1 — okunacak 09 bölümleri | — | | |
| devir:P8.2 Park maddesi 2 — eşitlik denetimlerinin Lint adımına girmesi | T04 | | |
| devir:P8.3 Park maddesi 3 — yama sürümü denetiminin publish.yml'e girmesi | T04, T09 | | |
| devir:P8.4 Park maddesi 4 — sahtelerin paylaşımı, xUnit v3 sınaması | T03 | | |
| devir:P8.5 Park maddesi 5 — yükleme betiği, kayıt biçimleyicisi, maskeleme testleri | T230, T231, T548 | | |
| devir:04-1948 Gövde anması: yasal metinlerin içeriği (GA-7) ★ | T356, T366, T373 | | |
| devir:09-117 Gövde anması: okunacak 09 bölümleri | — | | |
| devir:09-2249 Gövde anması: okunacak 09 bölümleri (§24 madde 1) | — | | |
| devir:12-177 Gövde anması: kabul kriterleri ve kontrol listesi 11'de tanımlı olmadan doğrulama başlamaz | — | | |
| devir:D-01 SETUP §2 ve §4 kurulumda tamamlanmadı (DEFERRED_BACKLOG) | T02, T03, T04, T05, T06, T07, T09 | | |

### 7.2 Geri izlenebilirlik (task → kaynak öğe)

Her task → hangi kaynak öğeden besleniyor?
**Kaynağı olmayan task = gerekçesiz iş / sessiz kapsam genişlemesi.**

| Task | Beslendiği kaynak öğe(ler) | Durum |
|---|---|---|

> **Neden geri yön de gerekli:** İleri yön yalnız **eksik** task'ı yakalar (S3). Planın kendisine sızmış, hiçbir gereksinime dayanmayan işi ise yalnız geri yön yakalar — ve o iş bir kez plana girdiğinde faz boyunca "planda var" gerekçesiyle sorgulanmadan yapılır.

> **Gate check kullanımı:** "Implement edildi" kolonu her faz sonunda kontrol edilir; eşlenip implement edilmeyen öğe **S3 Eksik** bulgusudur. Kaynağı olmayan task gate'te **kapsam bulgusu** olarak raporlanır.

## 8. Boşluk raporu

| # | Boşluk | Durum | Kapatan task |
|---|---|---|---|

## 9. Post-MVP parkı

> **Ne yazılır:** Plan yazılırken ortaya çıkan ama MVP'ye girmeyen işler. Ertelenen işlerden farklıdır — bunlar hiç başlanmamış, bilinçli olarak kapsam dışı bırakılmış kalemlerdir.

---

*Shopfolio — Implementation Plan v0.3*

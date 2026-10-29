# Shopfolio — Implementation Plan

**Versiyon: v0.1** | **Bağımlılıklar:** `02`–`10` | **Son güncelleme:** YYYY-AA-GG

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

Alt bölümler: **7.1.1** Veri modeli → task · **7.1.2** API → task · **7.1.3** Entegrasyon → task · **7.1.4** Arayüz → task · **7.1.5** İş kuralı → task

| Kaynak öğe | Task | Implement edildi | Kanıt |
|---|---|---|---|

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

*Shopfolio — Implementation Plan v0.1*

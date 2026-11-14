# T548 — Parçalı yükleme betiği: başlatma–parça–durum akışı, 409/404/400/413 yanıtları, ağ hatasında üç kez sürdürme

**Faz:** F5 | **Durum:** ⏳ Devam ediyor (PR açıldı, doğrulama bekliyor) | **Tarih:** 2026-11-14

---

## Yapılan İşler

- **Yükleme betiği (KK1-KK10):** `src/Shopfolio.Web/wwwroot/js/pages/panel/product-form.js` — `data-js="digital-file-upload"` kancasındaki dosya girdisi seçilince başlatma (`POST /panel/yukleme`, JSON `{ fileName, sizeBytes }`, `RequestVerificationToken` başlığı), parça (`Blob.slice`, `Content-Range` `end` dahil) ve durum (`GET`) akışı. Parça boyu 201 yanıtından okunur. 409 gövdedeki `receivedBytes`'tan sürer; 404 aynı dosyayla bir kez baştan başlatır, ikinci 404, 400, 413 ve 500 durur; başlatmada 422 iki mesajı `maxSizeBytes` ile ayırır; `redirect: "manual"` ile yönlendirme yakalanır; ağ hatasında durum isteğinin baytından 1, 2, 4 sn arayla en çok üç kez sürülür. Tamamlanınca kimlik hedef gizli alana, ad ve boy durum satırına yazılır. Yükleme sürerken form düğmeleri `aria-disabled` alır ve gönderim durur.
- **Olay devri:** dinleyici `document` üzerindedir; betikle eklenen varyant satırının girdisi de bağlanır (D-1559).
- **Durum satırı kancası:** `_ProductFormBody.cshtml` ve `_ProductVariantEntry.cshtml` "Dosya yok/ekli" öğesine `data-js="file-state"` eklendi (betik ad ve boyu oraya yazar).
- **Yinelenen kimlik (D-1567):** entries fieldset'i ve `data-list` `product-variant-entries` oldu; tablo `product-variants`'ı korur.
- **Belge adı (D-1568):** `09 §16.2` madde 6 ve `11` T548 kabul kriteri 1'de modül adı `pages/panel/product-form.js` oldu (sayfa yolu kuralı, `09 §16.1` madde 2; sayfa yolu değişmedi).
- **Tarama istisnaları (dar):** `FrameSourceScanTests` — `fetch(` yalnız `product-form.js`te ve tek çağrı; `AddressSourceScan` — yalnız `redirect: "manual"` biçimi "yönlendirme sistemi" taramasından muaf (yakın ıskalama ve ihlal örnekleri eklendi).
- **Testler:** `chunked-upload.test.mjs` (Node, 14 senaryo) + `ChunkedUploadScriptTests` (Node'u çalıştırır, `5242880`/P-26 sayısı ve ayrılma çağrısı taraması).

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/wwwroot/js/pages/panel/product-form.js`, `src/Shopfolio.Web/Pages/Panel/_ProductFormBody.cshtml`, `_ProductVariantEntry.cshtml`
- `tests/Shopfolio.UnitTests/Scripts/` (yeni), `Architecture/FrameSourceScanTests.cs`, `AddressSourceScan.cs`, `AddressSourceScanTests.cs`; `tests/Shopfolio.IntegrationTests/Web/ProductFormPageTests.cs`
- `Docs/09_CODING_GUIDELINES.md` (§16.2 madde 6 adı), `Docs/11_IMPLEMENTATION_PLAN.md` (T548 KK1), backlog, değişiklik parçası, durum satırı

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Modül ve kanca, hedef alan `data-target-field`'dan | ✓ | `product-form.js` `onFileChosen`; node testi "KK1" (iki test) |
| 2 | Seçimde boy karşılaştırılmaz; kayıt `aria-disabled`, gönderim durur | ✓ | node testi "KK2" (5 TB dosya, başlatma gider; `uploads` sayacı, `aria-disabled`, `preventDefault`) |
| 3 | Başlatma gövdesi ve başlık; parça boyu yanıttan | ✓ | node testi "KK3, KK4, KK5" |
| 4 | Parça `Content-Range` ilk/orta/son/tek | ✓ | node testi "KK3…" (`0-3`, `4-7`, `8-9/10`) ve "KK4" (`0-2/3`, tam kat `0-3`, `4-7/8`) |
| 5 | 200'de `receivedBytes`; tamamlanınca alan ve önizleme | ✓ | node testi "KK5" + "KK3…" (hedef alan `u1`, metinde ad ve `10 B`) |
| 6 | 409'dan sürme; tamamlandıysa biter | ✓ | node testi "KK6" |
| 7 | 404'te baştan; yeni 404, 400, 413, 500 durur | ✓ | node testi "KK7" (iki test) |
| 8 | 422'de iki mesaj | ✓ | node testi "KK8" |
| 9 | Yönlendirmede durma; 1/2/4 sn, en çok üç sürdürme | ✓ | node testi "KK9" (üç test: bekleme `[1000,2000,4000]`, toparlanma, son parça yanıtı kaybı) |
| 10 | Vazgeçme ve ayrılma uç çağırmaz; `5242880` ve P-26 sayısı yok | ✓ | node testi "KK10"; `ChunkedUploadScriptTests.TheModule_HoldsNoPartSizeNoSizeLimit_AndSendsNothingOnLeaving_KK10` |

### Öz denetim (bitiş kapısı 2)

| Madde | Sonuç | Kanıt |
|---|---|---|
| KK1 kanca ve hedef alan | ✓ | node "KK1"; `ProductFormPageTests.ADigitalProductsForm_CarriesTheFileFields_HookedForTheChunkedUpload_KK2` kancayı ve `data-target-field`'ı sınar |
| KK2 boyut karşılaştırması yok; kayıt kilidi | ✓ | node "KK2"; `product-form.js`te `file.size` yalnız gövde, aralık ve ilerlemede |
| KK3 başlatma gövdesi/başlık; parça boyu yanıttan | ✓ | node "KK3…"; `chunkSizeBytes` yanıttan (`startSession`) |
| KK4 dört parçada `Content-Range` ve başlık | ✓ | node "KK3…": ilk, orta, son; "KK4": tek ve tam kat |
| KK5 tamamlanma; hedef alan | ✓ | node "KK5" |
| KK6 409'dan sürme | ✓ | node "KK6" |
| KK7 404'te baştan; ikinci 404, 400, 413, 500'de durma | ✓ | node "KK7" |
| KK8 iki 422 mesajı | ✓ | node "KK8" |
| KK9 yönlendirmede durma; üç deneme sonra durma | ✓ | node "KK9" |
| KK10 kaynak taraması temiz | ✓ | `ChunkedUploadScriptTests` (ihlal ve yakın ıskalama örnekleri dahil), `FrameSourceScanTests` KK4 |
| `09 §25` ilgili satırlar: satır içi betik/olay niteliği yok, `innerHTML` yok | ✓ | `FrameSourceScanTests` tüm betikleri tarar (metin `textContent`; `fetch(` istisnası tek çağrı, `redirect: "manual"` doğrulanır) |
| Güvenlik: sahtecilik değeri yalnız formun gizli alanından; sır yok; başlık adı `CookieSetup.AntiforgeryHeaderName` ile aynı | ✓ | `product-form.js` `FORGERY_HEADER`; `CookieSetup.cs:11` |
| Sık FAIL 1 — istisna geniş değil | ✓ | `fetch(` yalnız bir modülde ve tek çağrı; `AddressSourceScan` yalnız `redirect: "manual"` (ihlal örnekleri: `redirect: "follow"`, `redirect: lookup(path)`) |
| Sık FAIL 2 — çağrı biçimi kaçırma | ✓ | `fetch (`, `fetch(` ve `5 * 1024 * 1024`/`500 * 1_048_576`/`500 MB` biçimleri örnekle sınanır |
| Sık FAIL 4 — hedefi T548 olan backlog | ✓ | D-1399, D-1559, D-1567, D-1568 kapandı (kanıt satırlarda; D-1567: `ProductFormPageTests` kimlik tekilliği) |
| Sık FAIL 5 — 09 metni karar kaydısız | ✓ | yalnız modül adı düzeltildi (`09 §16.1` madde 2 kuralının sayfa yolu); kural değişmedi |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test --project tests/Shopfolio.UnitTests --no-build` | 7708/7708 (Node betik testi 14 senaryo dahil) |
| Integration | ✓ | `ProductFormPageTests` — TAM: CI | 37/37 |
| Format | ✓ | `dotnet format Shopfolio.slnx --no-restore --include <değişen .cs>` | değişiklik yok |
| Derleme | ✓ | `dotnet build -warnaserror` (Unit, Integration) | 0 uyarı, 0 hata |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | bekliyor |
| Bulgu sayısı | — |
| Düzeltme gerekli mi | — |
| Bloke-etmeyen not (K) sayısı | — |

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok (betik testi çalıştırıcıda hazır gelen `node`'u kullanır; kütüphane eklenmedi)

## Commit & PR

- **Branch:** `task/T548-parcali-yukleme-betigi`
- **Commit:** `65d9c542` — yapım
- **PR:** #577
- **CI:** yapım işçisi izlemez
- **Post-merge run'lar:** *(validator doldurur)*

## Dış Varsayımlar

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| 1 | `fetch` `redirect: "manual"` ile yönlendirmede `type === "opaqueredirect"` yanıtı verir (bilgim tarihli) | tarayıcıda sınanmadı; sahte yanıtla sınandı | D-1573 |
| 2 | `form.elements.namedItem(ad)` köşeli parantezli alan adlarını (`Input.Variants[0].DigitalFileId`) bulur | HTML standardı (ad eşleşir); tarayıcıda sınanmadı | D-1573 |
| 3 | CI koşucusunda `node` kuruludur (`ubuntu-latest`) | `ci.yml` Unit işi `ubuntu` koşucusu; yerelde sınandı | CI'da görülecek |

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | Tarayıcıda uçtan uca yükleme (VAL-B118…B128) | D-1573 | `12`'nin tarayıcı senaryosu |
| 2 | Durum ve hata metinleri betikte; hata satırının stili yok | D-1574 | ⚠ sunucudan metin taşıma ve `admin.css` T548 kapsamı dışında |

## Notlar

- **Working tree (Adım -1):** temiz
- **⚠ Kolay kararlar:** (1) Modül adı: sayfa yolu kuralı (`product-form.js`) kazandı, belge adı düzeltildi — D-1568 böyle öneriyordu. (2) Durum satırı için `data-js="file-state"` kancası eklendi (D-1399 "form tarafında değişiklik gerekmez" diyordu; ayrı bir satır "Dosya yok" ile yan yana kalırdı). (3) Kayıt kilidi formun bütün gönderim düğmelerini kapsar (galeri yükleme dahil). (4) 404 sonrası yeniden başlatma, ağ sürdürmesindeki durum isteğinin 404'üne de uygulanır. (5) D-1567 (yinelenen `product-variants` kimliği) hedefi T548 olduğu için kapatıldı: entries fieldset'i `product-variant-entries` oldu.
- **Repo memory (bitiş kapısı 9):** parça dosyalarına yazıldı (STATUS/MEMORY ana satırlarına dokunulmadı)

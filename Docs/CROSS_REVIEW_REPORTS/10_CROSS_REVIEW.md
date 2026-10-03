# Cross-Review — 10 MVP Scope (Tur 1)

**Tarih:** 2026-10-03 · **Hedef:** `Docs/10_MVP_SCOPE.md` v0.11 · **İkinci model:** Codex CLI — ChatGPT hesabı, `gpt-5.6-terra`, düşünme düzeyi yüksek (salt okuma) · **Girdi:** yalnız doküman ve boş şablonu (`e74883f`), yeni ve izole bir çalışma alanında; karar kaydı ve raporlar verilmedi (K-431)

## 1. Ham bulgular

İkinci modelin çıktısı, düzenlenmeden (alıntı bloğu içinde). **İstem baştan daraltılmış biçimdedir (K-644):** yedi kriter, bulgu biçimi, "bilinçli karara yalnız katılmadığın için itiraz etme" cümlesi ve `02`'nin 26. turunda giren paragraf (K-615): *"Yalnız CİDDİ sorunları yaz: (1) yürürlükteki mevzuata aykırılık, (2) müşteriye ya da firmaya para kaybı veya hak kaybı doğuran kural boşluğu, (3) kullanıcının takılıp kaldığı, çıkışı olmayan bir akış, (4) dokümanın iki yerinin birbiriyle çelişmesi. Nadir kenar durumları, iyileştirme önerilerini, savunmada derinlik önerilerini, üslup ve ifade tercihlerini bulgu yapma. Böyle bir sorun yoksa SONUÇ: TEMİZ yaz."* İstem ayrıca belgenin bir MVP kapsam dokümanı olduğunu söyledi: iş kurallarının ayrıntısı ayrı bir Ürün Gereksinimleri dokümanındadır.

> ```text
> BULGU-1
> Kriter: Tutarlılık
> Seviye: Yüksek
> Yer: §2 / KP-63 ve KP-74
> Alıntı: “iz değiştirilemez ve silinemez.” / “saklama süresi dolan kişisel veri her gün kendiliğinden imha edilir.”
> Sorun: İşlem izi, yönetici kimliği ile eski/yeni değerleri içerir; dolayısıyla kişisel veri barındırabilir. KP-63 izleri süresiz silinemez kılarken KP-74 saklama süresi dolan tüm kişisel verinin günlük imhasını zorunlu tutuyor. Bu, hem doğrudan çelişki hem de saklama süresi sonunda hukuka aykırı veri tutma riski yaratır.
> Öneri: KP-63’ü, işlem izinin yetkisiz kullanıcılarca değiştirilemeyeceği; ancak saklama süresi sonunda KP-74 uyarınca imha veya gerekli ölçüde anonimleştirmeye tabi olduğu şeklinde düzeltin.
> ```

Model sonuca varmadan önce iki konuyu web aramasıyla kontrol etti (koşum kaydında görünür, çıktıya girmedi): Mesafeli Sözleşmeler Yönetmeliği'nde cayma, iade kargo bedeli ve on dört günlük geri ödeme; ETBİS Tebliği m.5'te kayıt yükümlülüğü ve esnaf. İkisi için de bulgu yazmadı.

## 2. Bağımsız değerlendirme

| # | Karar | Gerekçe | Uygulanan düzeltme |
|---|---|---|---|
| BULGU-1 | ⚠️ KISMİ | **Çelişki `10`'un metninde gerçek.** KP-63 *"iz değiştirilemez ve silinemez"* diyerek süre koymuyordu; KP-74 saklama süresi dolan kişisel verinin imhasını yazıyor. İz satırları yönetici adı ve e-posta adresi taşır, yani kişisel veridir (K-355'in gerekçesi). Süresiz saklama KVKK m.4/2-d'nin *"ilgili mevzuatta öngörülen veya işlendikleri amaç için gerekli olan süre kadar muhafaza edilme"* ilkesine aykırı olurdu. **Kural zaten kararlı:** K-355 izi on yıl saklar ve *"bu süre içinde değiştirilemez ve silinemez"* der, süre dolunca satırlar imha edilir (K-354). `02 §4` Z-30 aynısını yazar: süre satırın yazıldığı takvim yılının sonundan başlar (K-449). `10` satırı K-355'in süre kaydını düşürmüştü. **Önerinin iki parçası uygulanmadı.** "Yetkisiz kullanıcılarca değiştirilemez" iz için bilinçli kuralı (K-310, `02 §8.5.2`) zayıflatır: izi yönetici de değiştiremez ve silemez, yanlış işlem yeni bir satırla düzeltilir. Anonimleştirme seçeneği gereksizdir: Z-30 süre sonunda imhayı yazar, iki yol açmak yeni bir karar ister. | KP-63: *"iz on yıllık saklama süresi boyunca değiştirilemez ve silinemez, süre dolunca kendiliğinden imha edilir (KP-74)"*. Kaynak sütununa K-355 ve `02 §4` Z-30 eklendi. Yeni karar yok: K-355'in sözü geri yazıldı. |

**Dağılım:** 0 KABUL · 1 KISMİ · 0 RET.

## 3. Ek bulgular

- **Mekanik tarama:** `10`'da anılan bütün karar numaraları (en yükseği K-644) karar kaydında satır olarak var. Sürüm başlığı ve alt bilgi aynı sürümü gösteriyor (v0.12).
- **Ciddi bir sorun görülmedi.** Bu turda ikinci modelin listesinin dışında ciddi bir sorun saptanmadı.
- **Etki yansıtmaya devredilen:** `02`'nin sözlüğündeki "İşlem izi" satırı ve §8.5.2 de izin *"değiştirilemez ve silinemez"* olduğunu süre belirtmeden yazıyor. `02`'de çelişki yok, çünkü Z-30 süreyi ve imhayı aynı dokümanda taşıyor ve `02`'nin cross-review'ı TEMİZ döndü. Ama aynı okuma orada da mümkün. İki yerin Z-30'a işaret etmesi etki yansıtma adımında değerlendirilir.

## 4. Kullanıcı onay checklist'i

Proje sahibinin talimatıyla ("kolay soruları bana sorma sen karar ver, çok çok kritik konuları sadece sor") yönetici onayladı ve uygulandı (`10` v0.12). Bu turda yeni karar satırı açılmadı. Turdan önce süreç kararı K-644 öneriyle kaydedildi: K-615'in daraltılmış talimatı `10`'un cross-review'ına baştan uygulanır.

- [x] BULGU-1 (kısmi — KP-63'e K-355'in saklama süresi geri yazıldı; önerinin "yetkisiz kullanıcı" ve anonimleştirme parçaları uygulanmadı)

**Hukuki kontrol** (2026-10-03): KVKK m.4/2-d, kişisel verinin ilgili mevzuatta öngörülen ya da işlendiği amaç için gerekli süre kadar muhafaza edilmesini ister. İşlem izinin on yıllık süresi ticari kaydın yanında tutulur (K-355) ve süre dolunca imha edilir. Düzeltme bu ilkeyle uyumludur.

**Kaynak (hukuki kontrol):** [6698 sayılı Kişisel Verilerin Korunması Kanunu — Adalet Bakanlığı yayımı](https://mgm.adalet.gov.tr/Resimler/SayfaDokuman/51120191031036698%20KVKK.pdf)

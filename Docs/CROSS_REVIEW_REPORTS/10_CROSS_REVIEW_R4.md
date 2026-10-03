# Cross-Review — 10 MVP Scope (Tur 4)

**Tarih:** 2026-10-03 · **Hedef:** `Docs/10_MVP_SCOPE.md` v0.14 · **İkinci model:** Codex CLI — ChatGPT hesabı, `gpt-5.6-terra`, düşünme düzeyi yüksek (salt okuma) · **Girdi:** yalnız doküman ve boş şablonu (`e74883f`), yeni ve izole bir çalışma alanında; karar kaydı, önceki turlar ve raporlar verilmedi (K-431)

## 1. Ham bulgular

İkinci modelin çıktısı, düzenlenmeden (alıntı bloğu içinde). İstem önceki turlarla aynıdır: daraltılmış talimat (K-615, K-644), yedi kriter, bulgu biçimi ve "bilinçli karara yalnız katılmadığın için itiraz etme" cümlesi.

> ```text
> BULGU-1  
> Kriter: Hukuki uyum  
> Seviye: Yüksek  
> Yer: §2, KP-47  
> Alıntı: “bildirimin siparişin sahibinden geldiğini siparişin kendi kanalına — iletişim e-postasına ya da teslimat telefonuna — dönerek teyit ettikten sonra, bildirimin ulaştığı tarihle panelden kaydeder”  
> Sorun: E-posta veya mektupla yapılan cayma bildirimi, tüketicinin siparişteki e-postaya ya da telefona erişememesi/yanıt verememesi hâlinde kayda alınamaz. Oysa süresinde satıcıya yöneltilen yazılı veya kalıcı veri saklayıcısındaki açık cayma beyanı yeterlidir; ek teyit cayma hakkını ve iade süresini şartlı hâle getiremez. Bu, müşterinin kanuni cayma hakkını fiilen kaybetmesine yol açabilir. [Mesafeli Sözleşmeler Yönetmeliği m.11](https://tuketici.ticaret.gov.tr/data/5e81982d13b876a1b04c7a42/2023-6502%20Say%C4%B1l%C4%B1%20T%C3%BCketicinin%20Korunmas%C4%B1%20Hakk%C4%B1nda%20Kanun.pdf)  
> Öneri: Bildirimi ulaştığı anda tarih damgasıyla kaydedip müşteriye teyit edin; kimlik doğrulamasını, caymanın geçerliliğini veya yasal iade süresini durdurmayan ayrı bir dolandırıcılık incelemesi olarak tanımlayın.
> ```

Model sonuca varmadan önce web aramasıyla üç konuya baktı (koşum kaydında görünür, çıktıya girmedi): Mesafeli Sözleşmeler Yönetmeliği'nde cayma süresi ve iade kargo bedeli, 2025/10 sayılı Cumhurbaşkanlığı Genelgesi'nin Erişilebilirlik Kontrol Listesi, ETBİS doğrulama bağlantısı. Bunlar için bulgu yazmadı. İki biçim notu: "Hukuki uyum" yedi kriterden biri değildir (en yakını teknik doğruluk), ve m.11 diye verilen bağlantı Yönetmeliğe değil 6502 sayılı Kanun'un metnine gider. Değerlendirme hükmü Yönetmeliğin kendi metninden okudu (§4).

## 2. Bağımsız değerlendirme

| # | Karar | Gerekçe | Uygulanan düzeltme |
|---|---|---|---|
| BULGU-1 | ⚠️ KISMİ | **Önerinin kendisi bilinçli olarak elenmiş bir seçenektir.** Konu `02`'nin 21. turunda geldi ve K-607 ile kapandı. K-607 *"Bildirimi hemen kaydedip ayrı bir 'kimlik teyidi bekliyor' durumunda tutmak (Codex'in önerisi)"* seçeneğini eledi: yeni bir kalem durumu ve elle adım açar, geçmişe dönük tarih (K-510) bildirimin tarihini zaten korur. Teyidin sebebi somut bir saldırıdır: sipariş numarasını ve adresi bilen biri mektupla kendi IBAN'ını taşıyan bir cayma gönderir, teslimden önceki caymada para on dört gün içinde gider (m.12/2). **Hukuki iddia da yerinde değil.** m.11/1 bildirimin süresinde ve yazılı ya da kalıcı veri saklayıcısıyla yöneltilmesini yeterli sayar. Satıcının bildirimin tüketiciden geldiğini doğrulamasını yasaklamaz; m.11/4 caymanın ispatını zaten tüketiciye yükler. Teyit tüketiciye bir şekil şartı da yüklemez, çünkü siparişin kanalına firma kendisi döner. Müşterinin hakkı kaybolmaz: kaydedilmeyen gerçek bir bildirimin sonucu firmadadır. **Ama okumanın sebebi `10`'un metnindeydi.** KP-47 *"teyit ettikten sonra … kaydeder"* diyordu ve K-607'nin öbür yarısını yazmıyordu. Teyit kaydın tarihini kaydırmaz, teyitte geçen gün geri ödeme süresinden düşer, sahibine ulaşılamazsa kaydedilmeyen gerçek bildirimin sonucu firmadadır. Bu yazılmayınca teyit caymanın ön şartı, ulaşılamayan müşterinin hakkı da kaybolmuş gibi okunuyordu. Satır ayrıca teyidi her bildirime uyguluyordu; K-607'de siparişin iletişim e-postasından gelen bildirimde teyit bildirimin kendisidir. 3. turdaki KP-47 bulgusuyla aynı kalıp: satır kararın yalnız yarısını taşıyordu. | KP-47 Özellik: *"… caymayı bildirimin firmaya ulaştığı tarihle panelden kaydeder; bildirim siparişin iletişim e-postasından gelmediyse, siparişin sahibinden geldiğini siparişin kendi kanalına … dönerek teyit eder. Teyit kaydın tarihini kaydırmaz: teyitte geçen gün geri ödeme süresinden düşer ve sahibine ulaşılamadığı için kaydedilmeyen gerçek bir bildirimin sonucu firmadadır."* KP-47 Gerekçe: teyit başkasının siparişinde cayma kaydettirmeyi önler; caymanın yasal şartı değildir, süresinde yazılı ya da kalıcı veri saklayıcısıyla yöneltilen bildirim yeterlidir (m.11); "teyit bekliyor" durumu elendi, çünkü yeni bir durum ve elle adım açar. K-607 Kaynak sütununda zaten vardı. Yeni karar yok: K-607'nin sözü yazıldı. Teyitten önce kaydetme önerisi uygulanmadı. |

**Dağılım:** 0 KABUL · 1 KISMİ · 0 RET.

## 3. Ek bulgular

- **Mekanik tarama:** `10`'da anılan bütün karar numaraları karar kaydında satır olarak var. Sürüm başlığı ve alt bilgi aynı sürümü gösteriyor (v0.15). Kapsam satırı sayısı değişmedi (77).
- **`02` ile hizalı:** `02 §10.4.10` ve §8.3.8 K-607'nin iki yarısını zaten yazıyor. Bu turun düzeltmesi `10`'u bunlara hizaladı. `02`'de değişiklik gerekmiyor.
- **Yönetmelik metninde yeni bir kayıt (bulgu değil):** mevzuat.gov.tr'nin konsolide metni, Danıştay Onuncu Dairesi'nin 6/5/2026 tarihli ve E.2022/5534, K.2026/2753 sayılı kararıyla m.15/1'in (ı), (j) ve (k) bentlerinin iptal edildiğini gösteriyor: tescili zorunlu taşınırlar, canlı müzayede, kurulum ve montajı yapılan mallar. `10` ve `02` bu bentleri cayma istisnası olarak kullanmıyor. `02`'nin istisna listesinde montaj ya da tescil bendi yok. Etki yok.
- **Ciddi bir sorun görülmedi.** Bu turda ikinci modelin listesinin dışında ciddi bir sorun saptanmadı.
- **Etki yansıtmaya devredilen:** yeni bir iş yok. 1. ve 2. turdan devreden `02` notları yerinde duruyor: "İşlem izi" sözlük satırı, §8.5.2 ve §3.34.8.

## 4. Kullanıcı onay checklist'i

Proje sahibinin talimatıyla ("kolay soruları bana sorma sen karar ver, çok çok kritik konuları sadece sor") yönetici onayladı ve uygulandı (`10` v0.15). Bu turda yeni karar satırı açılmadı.

- [x] BULGU-1 (kısmi: KP-47 kaydın bildirimin firmaya ulaştığı tarihle yapıldığını, teyidin yalnız siparişin iletişim e-postası dışından gelen bildirimde gerektiğini ve tarihi kaydırmadığını, teyitte geçen günün geri ödeme süresinden düştüğünü ve kaydedilmeyen gerçek bildirimin sonucunun firmada olduğunu yazar. Gerekçe teyidin sebebini ve yasal şart olmadığını yazar. Teyitten önce kaydetme önerisi K-607 gereği uygulanmadı)

**Hukuki kontrol** (2026-10-03): Mesafeli Sözleşmeler Yönetmeliği'nin mevzuat.gov.tr'deki konsolide metni okundu. Yönetmelik 27.11.2014 tarihli ve 29188 sayılı Resmî Gazete'de yayımlandı; son değişiklik 24.05.2025 tarihli ve 32909 sayılı Resmî Gazete'dedir. m.11/1 (23.08.2022, 31932 ile değişik ibare): *"Cayma hakkının kullanıldığına dair bildirimin cayma hakkı süresi dolmadan, yazılı olarak veya kalıcı veri saklayıcısı ile satıcı, sağlayıcı veya aracı hizmet sağlayıcıya yöneltilmesi yeterlidir."* m.11/4: *"Bu maddede geçen cayma hakkının kullanımına ilişkin ispat yükümlülüğü tüketiciye aittir."* m.12/2: teslimden önceki caymada geri ödeme *"cayma hakkının kullanıldığına ilişkin bildirimin kendisine ulaştığı tarihten itibaren on dört gün içinde"* yapılır. Hüküm bildirimin şeklini sayar, satıcının bildirimi kimin gönderdiğini doğrulamasını yasaklamaz. Süre bildirimin ulaştığı tarihten işler. `10`'un düzeltilmiş metni de bunu söylüyor. Codex'in *"ek teyit cayma hakkını ve iade süresini şartlı hâle getiremez"* okuması doğrudur. Hükümden çıkardığı *"teyit gerekmeden kaydedilmeli"* sonucu ise hükmün sözünde yoktur: ürün teyidi hakkın şartı yapmıyor, tarihi korur ve kaydedilmeyen gerçek bildirimin sonucunu firmaya bırakır.

**Kaynaklar (hukuki kontrol):**
- [Mesafeli Sözleşmeler Yönetmeliği — mevzuat.gov.tr konsolide metin (PDF)](https://www.mevzuat.gov.tr/MevzuatMetin/yonetmelik/7.5.20237.pdf)

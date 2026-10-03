# Cross-Review — 10 MVP Scope (Tur 3)

**Tarih:** 2026-10-03 · **Hedef:** `Docs/10_MVP_SCOPE.md` v0.13 · **İkinci model:** Codex CLI — ChatGPT hesabı, `gpt-5.6-terra`, düşünme düzeyi yüksek (salt okuma) · **Girdi:** yalnız doküman ve boş şablonu (`e74883f`), yeni ve izole bir çalışma alanında; karar kaydı, önceki turlar ve raporlar verilmedi (K-431)

## 1. Ham bulgular

İkinci modelin çıktısı, düzenlenmeden (alıntı bloğu içinde). İstem önceki turlarla aynıdır: daraltılmış talimat (K-615, K-644), yedi kriter, bulgu biçimi ve "bilinçli karara yalnız katılmadığın için itiraz etme" cümlesi.

> ```text
> BULGU-1
> Kriter: Tutarlılık
> Seviye: Yüksek
> Yer: §2 / KP-47
> Alıntı: "Firma yöneticisi bir kalemi ya da siparişi kapalı listeden sebep seçerek iptal eder — \"stokta bulunamadı\" seçildiğinde panel stok yokluğunun yasal bir imkânsızlık sayılmadığını hatırlatır."
> Sorun: Stok yokluğunun yasal imkânsızlık olmadığını kabul ederken firmaya bu sebeple tek taraflı iptal imkânı veriyor. Bu, Mesafeli Sözleşmeler Yönetmeliği m.16/4 ile çelişir ve tüketicinin sözleşmenin ifası ile indirimli fiyattan alma hakkını kaybetmesine yol açabilir. [Resmî metin](https://tuketici.ticaret.gov.tr/data/5e819a8e13b876a1b04c7a4a/Mesafeli%20S%C3%B6zle%C5%9Fmeler%20Y%C3%B6netmeli%C4%9Fi.pdf)
> Öneri: “Stokta bulunamadı” sebebini firma kaynaklı iptal seçeneğinden çıkarın; bu durumda siparişin ifası zorunlu kalsın veya yalnız müşterinin seçtiği iptal/fesih akışı işletilebilsin.
> ```

Model sonuca varmadan önce beş konuyu web aramasıyla kontrol etti (koşum kaydında görünür, çıktıya girmedi): Mesafeli Sözleşmeler Yönetmeliği'nde cayma, geri ödeme ve malın geri gönderildiğinin ispatı; ETBİS kayıt yükümlülüğü; yerli üretim logosu; Garanti Belgesi Yönetmeliği'nde belgenin verilmesi; indirimli satışın referans fiyatı. Bunlar için bulgu yazmadı.

## 2. Bağımsız değerlendirme

| # | Karar | Gerekçe | Uygulanan düzeltme |
|---|---|---|---|
| BULGU-1 | ⚠️ KISMİ | **Önerinin kendisi bilinçli olarak elenmiş bir seçenektir.** Konu `02`'nin 23. turunda aynı öneriyle gelmişti. K-609 *"Stokta bulunamadı listeden çıkarılır"* seçeneğini eledi: firma elinde olmayan malı yine gönderemez ve iptali "diğer" sebebiyle yapar. Böylece K-373'ün sebebe göre kargo ayrımı ve işlem izinin doğruluğu kaybolur. Önerinin ikinci yarısı da işlemez: *"siparişin ifası zorunlu kalsın"* bir yazılımın sağlayabileceği bir şey değildir. Malı olmayan firmayı panel teslime zorlayamaz. Sipariş yalnız askıda kalır, müşterinin parası da onunla birlikte bekler. m.16/4 firmanın iptal etmesini yasaklamaz. Stok yokluğunu imkânsızlık saymaz, yani firma bu iptalle sorumluluktan kurtulamaz. Müşterinin ifa ya da tazminat talebi firmaya karşı durur. Ürün bu talebi kaldırmaz, karara da bağlamaz. **Ama okumanın bir sebebi `10`'un metnindeydi.** KP-47 uyarının yalnız ilk yarısını yazıyordu: stok yokluğu imkânsızlık değildir. K-609'un ve `02 §7.2.4`'ün ikinci yarısını yazmıyordu: iptalin sonucu firmadadır. Sebebin neden listede kaldığını da yazmıyordu. Tek başına okununca ürün, yasanın kabul etmediği bir iptali firmaya hak olarak tanıyor gibi duruyordu. Model aynı konuya `02`'nin 23. turunda da bu okumayla gelmişti. | KP-47 Özellik: panel *"onaydan önce stok yokluğunun yasal bir imkânsızlık sayılmadığını ve iptalin sonucunun firmada olduğunu"* hatırlatır. KP-47 Gerekçe: *"'Stokta bulunamadı' sebebi bilinçli olarak listede kalır: firma elinde olmayan malı gönderemez, sebep çıkarılırsa iptal 'diğer'le yapılır ve sebebe göre kargo ayrımı kaybolur; iptal, müşterinin firmaya karşı yasal taleplerini ortadan kaldırmaz."* K-609 Kaynak sütununda zaten vardı. Yeni karar yok: K-609'un sözü yazıldı. Sebebi listeden çıkarma önerisi uygulanmadı. |

**Dağılım:** 0 KABUL · 1 KISMİ · 0 RET.

## 3. Ek bulgular

- **Mekanik tarama:** `10`'da anılan bütün karar numaraları karar kaydında satır olarak var. Sürüm başlığı ve alt bilgi aynı sürümü gösteriyor (v0.14). Kapsam satırı sayısı değişmedi (77).
- **`02` ile hizalı:** `02 §6.7.6`, §7.2.4 ve §12.5 aynı kuralı zaten yazıyor: uyarının iki yarısı, sebebin listede kalma gerekçesi ve firmanın sonucu üstlenmesi. Bu turun düzeltmesi `10`'u bunlara hizaladı. `02`'de değişiklik gerekmiyor.
- **Ciddi bir sorun görülmedi.** Bu turda ikinci modelin listesinin dışında ciddi bir sorun saptanmadı.
- **Etki yansıtmaya devredilen:** yeni bir iş yok. 1. ve 2. turdan devreden `02` notları yerinde duruyor: "İşlem izi" sözlük satırı, §8.5.2 ve §3.34.8.

## 4. Kullanıcı onay checklist'i

Proje sahibinin talimatıyla ("kolay soruları bana sorma sen karar ver, çok çok kritik konuları sadece sor") yönetici onayladı ve uygulandı (`10` v0.14). Bu turda yeni karar satırı açılmadı.

- [x] BULGU-1 (kısmi: KP-47'nin "stokta bulunamadı" uyarısı iptalin sonucunun firmada olduğunu da söyler; gerekçe sebebin neden listede kaldığını ve iptalin müşterinin yasal taleplerini kaldırmadığını yazar. Sebebi listeden çıkarma önerisi K-609 gereği uygulanmadı)

**Hukuki kontrol** (2026-10-03): Mesafeli Sözleşmeler Yönetmeliği m.16/4, 23.08.2022 tarihli ve 31932 sayılı Resmî Gazete'deki değişiklik yönetmeliğinin 14. maddesiyle değişik metin, değişiklik yönetmeliğinin Resmî Gazete metninden okundu: *"Sipariş konusu mal ya da hizmet ediminin yerine getirilmesinin imkansızlaştığı hallerde satıcı veya sağlayıcının ... bu durumu öğrendiği tarihten itibaren üç gün içinde tüketiciye yazılı olarak veya kalıcı veri saklayıcısı ile bildirmesi ve varsa teslimat masrafları da dâhil olmak üzere tahsil edilen tüm ödemeleri bildirim tarihinden itibaren en geç on dört gün içinde iade etmesi zorunludur. Malın stokta bulunmaması durumu, mal ediminin yerine getirilmesinin imkânsızlaşması olarak kabul edilmez."* Sonraki değişiklikler m.16'ya dokunmadı: 04.11.2023 (32359) ve 10.08.2024 (32628) tarihli değişiklikler yalnız geçici maddelere, 24.05.2025 tarihli değişiklik (32909) m.5, 12, 13 ve 15'e dokunur. Hüküm iptali yasaklamaz: stok yokluğunu imkânsızlık saymayarak satıcının bu iptalle sorumluluktan kurtulamayacağını söyler. Codex'in m.16/4 okuması doğrudur. Hükümden çıkardığı *"iptal seçeneği verilemez"* sonucu ise hükmün sözünde yoktur.

**Kaynaklar (hukuki kontrol):**
- [Mesafeli Sözleşmeler Yönetmeliğinde Değişiklik Yapılmasına Dair Yönetmelik — Resmî Gazete, 23.08.2022, sayı 31932 (TÜRMOB arşivi)](https://www.turmob.org.tr/arsiv/mbs/resmigazete/31932-1.pdf)
- [Mesafeli Sözleşmeler Yönetmeliğinde Değişiklik — Resmî Gazete, 24.05.2025, sayı 32909 (alomaliye.com yayımı)](https://www.alomaliye.com/2025/05/24/mesafeli-sozlesmeler-yonetmeliginde-degisiklik-24-05-2025/amp/)
- [Mesafeli Sözleşmeler Yönetmeliğinde Değişiklik — Resmî Gazete, 10.08.2024, sayı 32628 (alomaliye.com yayımı)](https://www.alomaliye.com/2024/08/10/mesafeli-sozlesmeler-yonetmeliginde-degisiklik-10-08-2024/amp/)
- [Mesafeli Sözleşmeler Yönetmeliğinde Değişiklik — Resmî Gazete, 04.11.2023, sayı 32359 (alomaliye.com yayımı)](https://www.alomaliye.com/2023/11/04/mesafeli-sozlesmeler-yonetmeliginde-degisiklik-04-11-2023/amp/)

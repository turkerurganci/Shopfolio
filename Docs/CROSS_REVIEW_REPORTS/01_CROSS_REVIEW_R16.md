# Cross-Review — 01 Project Vision (Tur 16)

**Tarih:** 2026-10-03 · **Hedef:** `Docs/01_PROJECT_VISION.md` v0.24 · **İkinci model:** Codex CLI — ChatGPT hesabı, `gpt-5.6-terra` (salt okuma) · **Girdi:** yalnız doküman ve boş şablonu, yeni ve izole bir çalışma alanında; model önceki turları görmedi (K-431)

## 1. Ham bulgular

İkinci modelin çıktısı, düzenlenmeden (alıntı bloğu içinde).

> ```text
> BULGU-1
> Kriter: Teknik doğruluk
> Seviye: Yüksek
> Yer: §6, mağaza düzeyi ölçülerin gerekçesi
> Alıntı: "geri ödeme çatısı cayma beyanından itibaren on dört gündür ve firma malın dönmesini beklerken de durmaz"
> Sorun: Mesafeli Sözleşmeler Yönetmeliği uyarınca satıcı, kural olarak cayma bildirimi sonrası 14 gün içinde bedeli iade eder; ancak malın iadesini veya tüketicinin gönderdiğine ilişkin kanıtı alana kadar iade yükümlülüğünü bekletebilir (malı kendisi almayı teklif etmedikçe). Doküman bu imkânı kaldırarak hukuken yanlış bir zorunluluk tanımlıyor.
> Öneri: Cümleyi, 14 günlük sürenin cayma bildiriminden başladığını; fakat satıcının, uygulanabildiği durumlarda malın iadesi veya gönderim kanıtına kadar iadeyi bekletebileceğini belirtecek şekilde düzeltin.
> 
> BULGU-2
> Kriter: Tutarlılık
> Seviye: Orta
> Yer: §8, “Doğrulama anı” ve §6, M-4
> Alıntı: "Ölçüme dayanan satırlar o kurulumun ilk üç ayında okunur;" / "oran, bu pencereler teslim edilen ya da ifası tamamlanan her siparişte kapandığında okunur."
> Sorun: §8, ölçüme dayalı varsayımların ilk üç ay içinde okunacağını söylerken M-4, dönem sonundaki siparişlerin cayma ve iade pencereleri kapandıktan sonra okunur. Bu kapanış üç aylık dönemin sonrasına taşabilir; ölçümün ne zaman kesinleştiği çelişkili kalıyor.
> Öneri: §8’i, verinin ilk üç aylık satış döneminde toplandığını; M-4’ün ise ilgili kapanış pencereleri tamamlandıktan sonra değerlendirildiğini açıkça söyleyecek şekilde güncelleyin.
> 
> SONUÇ: 2 BULGU
> ```

## 2. Bağımsız değerlendirme

| # | Karar | Gerekçe | Uygulanan düzeltme |
|---|---|---|---|
| BULGU-1 | ⚠️ KISMİ | Bulgu, 15. turda benim §6'ya eklediğim cümleye yönelik. Cümle K-209'u doğru aktarıyordu, ama K-209'un kendisi tartışmalı hâle geldi: ikinci model, satıcının ödemeyi bekletme hakkının süreyi askıya aldığını söylüyor; kısa bir arama da Yönetmelik'in 13. maddesinin son yıllarda değiştiğini gösterdi. Bunu Proje Vizyonu'nun cross-review'ında çözmek yanlış yer olur — kuralın evi Ürün Gereksinimleri §7.4. Vizyon dokümanından yasal ayrıntı çıkarıldı, doğrulama açık kalem olarak devredildi. | §6: "Üç ayın gerekçesi iade takvimidir: cayma penceresi ve geri ödeme süresi bir aylık ölçümde dolmaz … Sürelerin kendisi `02 §7.3` ve `§7.4`'tedir (K-203, K-209)." Tracker'da A-14 açıldı. |
| BULGU-2 | ✅ KABUL | §8 "ölçüme dayanan satırlar ilk üç ayda okunur" diyordu; M-4 ise iade süreleri kapandıktan sonra, yani üç aydan sonra okunuyor. Veri toplama penceresi ile okuma anı ayrı şeylerdir. | §8: "… verisi o kurulumun ilk üç ayında toplanır … Satır, kendi 'Nasıl doğrulanacak' hücresinin söylediği anda okunur — iptal ve iadeye dayanan ölçü, iade süreleri kapandıktan sonra (§6 M-4)." |

**Dağılım:** 1 KABUL · 1 KISMİ · 0 RET.

## 3. Ek bulgular

Değerlendirme sırasında yapılan kısa web araması (2026-10-03): Mesafeli Sözleşmeler Yönetmeliği'nin 13. maddesi değişiklik geçirmiş; üçüncü fıkrası 1 Ocak 2026 itibarıyla yürürlükten kalkmış, birinci fıkradaki süre ifadesiyle üçüncü fıkra için 1 Ocak 2025'e kadar geçiş hükmü işlemiş. Güncel metin resmî kaynaktan okunmadı; bu yüzden ne ikinci modelin iddiası ne K-209 burada doğrulanmış sayıldı. Kaynaklar: [hukukihaber.net — 2025 değişikliği](https://www.hukukihaber.net/mesafeli-sozlesmeler-yonetmeliginde-degisiklik-2025/amp), [alomaliye.com — 2022 değişikliği](https://www.alomaliye.com/2022/08/23/mesafeli-sozlesmeler-yonetmeliginde-degisiklik-23-08-2022/), [lexpera — Yönetmelik metni](https://www.lexpera.com.tr/mevzuat/yonetmelikler/mesafeli-sozlesmeler-yonetmeligi).

## 4. Kullanıcı onay checklist'i

Oturum açılışındaki talimat gereği uygulandı (`01` v0.25). Yeni karar satırı açılmadı; tracker §4'te A-14 açıldı (K-209'un güncel mevzuata uyumu, vadesi Ürün Gereksinimleri'nin kalite döngüsü).

- [x] BULGU-1 · [x] BULGU-2

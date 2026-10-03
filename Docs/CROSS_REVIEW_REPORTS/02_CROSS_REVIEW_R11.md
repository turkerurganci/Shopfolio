# Cross-Review — 02 Product Requirements (Tur 11)

**Tarih:** 2026-10-03 · **Hedef:** `Docs/02_PRODUCT_REQUIREMENTS.md` v0.26 · **İkinci model:** Codex CLI — ChatGPT hesabı, `gpt-5.6-terra`, düşünme düzeyi yüksek (salt okuma) · **Girdi:** yalnız doküman ve boş şablonu (`e74883f`), yeni ve izole bir çalışma alanında; karar kaydı, önceki turlar ve raporlar verilmedi (K-431)

## 1. Ham bulgular

İkinci modelin çıktısı, düzenlenmeden (alıntı bloğu içinde; satır sonlarındaki boşluklar atıldı). İstem önceki turlarınkiyle aynıdır: yedi kriter, bulgu biçimi ve "bilinçli karara yalnız katılmadığın için itiraz etme" cümlesi.

> ```text
> BULGU-1
> Kriter: Tutarlılık
> Seviye: Yüksek
> Yer: §6.2.16 — İptal edilmiş siparişe geç gelen kart ödemesi
> Alıntı: "Sistem ödemeyi sağlayıcı üzerinden kendiliğinden geri öder ... siparişin durumları değişmez."
> Sorun: Bu sipariş Ödeme durumu olarak `Başarısız` kalır; ancak para fiilen tahsil edilmiş ve geri ödenmiştir. Buna rağmen Z-41, `Başarısız` siparişi “ödemesi hiç alınmamış” kabul ederek üç yıllık saklama rejimine sokar. Finansal hareket, geri ödeme kaydı, saklama ve raporlama aynı olayı birbirinden farklı sınıflandırır.
> Öneri: Geç gelen başarılı kart ödemesi için sipariş durumundan bağımsız bir tahsilat/geri ödeme kaydı tanımlayın; bu kayıtların saklama, dışa aktarma ve satış özeti kurallarını açıkça belirleyin. Z-41’i fiilen tahsil edilip geri ödenen siparişleri dışlayacak biçimde düzeltin.
>
> BULGU-2
> Kriter: Edge case
> Seviye: Yüksek
> Yer: §5.4 — S8, Teslim edilemedi → Kargoya verildi
> Alıntı: "Firma yeniden gönderir"
> Sorun: S8, Teslim edilemedi durumundaki her sipariş için yeniden gönderime izin veriyor. Oysa kargodayken cayılmış ürünün firmaya dönmesi §6.4.22’de iade teslim alma akışına yönlendirilmiş; gecikme feshi yapılmış fiziksel kalemler de yeniden gönderilmemelidir. Beyaz listedeki S8’in koşulu bu kayıtları dışlamadığı için sistem, cayılmış veya feshedilmiş malı yeniden gönderebilir.
> Öneri: S8’i yalnız aktif, iptal edilmemiş, cayma beyanı ve gecikme feshi kaydı olmayan fiziksel kalemler için izinli yapın. Cayılmış ya da feshedilmiş kalemlerde Teslim edilemedi durumundan yalnız iade teslim alma/geri ödeme akışına geçileceğini açıkça yazın.
>
> SONUÇ: 2 BULGU
> ```

## 2. Bağımsız değerlendirme

| # | Karar | Gerekçe | Uygulanan düzeltme |
|---|---|---|---|
| BULGU-1 | ⚠️ KISMİ | Sorun gerçek, ama Codex'in çözümü fazla geniş. K-519 on yıllık süreyi (Z-28) *"ödemesi alınmış, muhasebe kaydına dönüşen"* siparişe, üç yılı (Z-41) *"ödemesi hiç alınmamış (Başarısız)"* siparişe bağladı. İkisini de ödeme durumuyla eşitledi. K-233'ün durumunda bu eşitlik bozuluyor: sipariş Başarısız'dır, ama para firmanın sağlayıcı hesabına girmiş ve çıkmıştır. Tahsilat ve iade firmanın defterine kayıt doğurur, sipariş kaydı da o kaydın dayanağıdır. Türk Ticaret Kanunu m.82 böyle dayanak belgeleri on yıl saklatır. K-519'un *"on yılın dayanağı ödemesi alınmamış siparişi kapsamaz"* gerekçesi bu siparişe uymuyor. Ayrıca §6.2.16 *"siparişin durumları değişmez"* diyordu, ama ödemenin ve iadesinin nerede okunacağını yazmıyordu. Kısmi kabulün sebebi: Codex sipariş durumundan bağımsız ayrı bir tahsilat ve geri ödeme kaydı istedi. Hareket tek bir siparişe bağlı olduğu için siparişin ödeme kaydı onu taşıyabilir; yeni bir varlık gerekmiyor. Satış özetinde sınıflandırma çelişkisi de yoktu: sipariş satış değildir, Başarısız olarak kalması doğru. | K-589 `(öneriyle kaydedildi — ⚠)`. Sipariş İptal edildi ve Başarısız kalır, satış özetine ve ödeme tamamlama oranına ödenmiş sipariş olarak girmez. Geç gelen ödeme ve geri ödemesi tarih ve tutarla siparişin ödeme kaydında durur, panelde görünür ve dışa aktarmada sipariş satırıyla çıkar. Sipariş Z-28'i izler. Havale hattındaki karşılık ürünün dışında yürür, ürün onu bilmez ve sipariş Z-41'i izler. Güncellenen yerler: §3.17.8, §4.2 Z-28, Z-41, §5.5, §6.2.16, §10.7.1, §12.2.7. |
| BULGU-2 | ✅ KABUL | Doğru, ve sorun Codex'in gösterdiğinden geniş. K-499 gecikme feshini kargodaki siparişe açtı ve *"kargodaki mal firmanın iade adresine döner"* dedi, ama feshedilmiş kalemin sevkiyat hattındaki yerini yazmadı. S8 koşulsuzdu. S5 yalnız iptal edilmemiş kalemi soruyordu; fesih Hazırlanıyor'da da yapılabildiği için feshedilmiş mal kargoya verilebiliyordu. S10 ve S11 feshi saymıyordu, bu yüzden fiziksel kalemleri feshedilmiş sipariş kapanamıyordu. S9 da yalnız cayma beyanlı kalemi firma iptalinden ayırıyordu: feshedilmiş kalemde firma iptali, feshin geri ödemesinin yanına ikinci bir geri ödeme açabiliyordu. K-503 aynı boşluğu kargodayken cayılan kalem için kapatmıştı. Bulgu bu kalıbın feshe uygulanmadığını gösteriyor. Codex'in *"Teslim edilemedi'den yalnız iade teslim alma/geri ödeme akışına geçilir"* önerisi ise kayıtla birebir örtüşmüyor: geri ödeme fesih damgasından zaten işler (§7.2.3), teslim alma adımı onu başlatmaz. Önerinin bu kısmı K-503'ün kalıbına göre okundu. | K-590 `(öneriyle kaydedildi — ⚠)`. Feshedilen kalem cayma beyanıyla kapanan kalemin kalıbıyla sayılır: kargoya verilmez (S5) ve yeniden gönderilmez (S8). S10 ve S11 onu beklemez. Firma iptali (S4, S9) ona uygulanmaz ve siparişin kapanışında kalem iptal edilmiş kalem gibi sayılır. Kargoya verilmemiş kalemin stoğu kendiliğinden döner; geri dönen malı firma teslim alma adımıyla işler ve stoğa ekler, bu adım geri ödemeyi başlatmaz. "Kargoya verilecek" sayacı feshedilmiş kalemi saymaz. Güncellenen yerler: §5.4 (S4, S5, S8, S9, S10, S11), §5.7, §5.8, §6.7.3, yeni satır 6.7.16, §7.2.3, §10.6.1. |

**Dağılım:** 1 KABUL · 1 KISMİ · 0 RET.

İki bulgu da bilinçli bir kararın elediği bir seçeneğe yönelmiyor. Her biri bir kararın kapsamında yazılmamış bir parçayı gösteriyor:

- **BULGU-1:** K-233 ile K-519 arasındaki sınır. K-519 süreyi ödeme durumuna bağladı; K-233'ün durumu bu bağı bozuyor.
- **BULGU-2:** K-503'ün kalıbı K-499'un feshine taşınmamıştı.

Önceki turlarda ikisi de gelmedi. 10. turun konularıyla (e-posta geçmişi, eşzamanlılık, hesap türleri) ilgileri yok.

## 3. Ek bulgular

- **Fesihten sonra kargonun malı yine de teslim etmesi (kendi kontrolüm, işlenmedi):** Kargodaki siparişte fesihten sonra kargo malı firmaya döndürmeyip müşteriye teslim ederse ne olacağı yazılı değil. Sipariş bu durumda Kargoya verildi'de kalır ve feshedilmiş kaleme teslim işaretinin konup konmayacağı yazılı değil. Sonraki turda ya da etki yansıtmada bakılmalı. Bu turda işlenmedi, çünkü K-499 malın döndüğünü kabul ediyor ve durumu ayrı bir karar istiyor.
- **Etki yansıtma için not:** `10 §2`'de iki satır hizalanacak. KP-47'nin (iptal, iade teslim alma) ve geri dönen gönderinin yeniden gönderimi gecikme feshine uğramış kalemi kapsam dışı bırakmalı (K-590). Dışa aktarmanın içeriği geç gelen kart ödemesini taşımalı (K-589). `01`'de K-233'ün durumu ve S8 geçmiyor; M-1'in *"ödemesi tamamlanmış sipariş"* tanımı K-589 ile uyumlu (`grep` ile kontrol edildi). Bu turda `01` ve `10`'a dokunulmadı.

## 4. Kullanıcı onay checklist'i

Proje sahibinin talimatıyla ("kolay soruları bana sorma sen karar ver, çok çok kritik konuları sadece sor") yönetici onayladı ve uygulandı (`02` v0.27). İki karar satırı açıldı:

- K-589 `(öneriyle kaydedildi — ⚠)`: kişisel verinin saklama süresine dokunuyor; K-519'un sınırını netleştiriyor.
- K-590 `(öneriyle kaydedildi — ⚠)`: paraya (ikinci geri ödeme) ve yasal hakka (feshedilen malın yeniden gönderilmesi) dokunuyor; K-503'ün kalıbını K-499'a taşıyor.

- [x] BULGU-1 · [x] BULGU-2

**Hukuki kontrol:**

- **BULGU-1:** Türk Ticaret Kanunu m.82'ye göre ticari defterlere kayıtlar için dayanak oluşturan belgeler on yıl saklanır. Süre, belgenin oluştuğu takvim yılının sonundan işler; bu Z-28'in başlangıcıyla aynı. Mesafeli Sözleşmeler Yönetmeliği m.20/1'in üç yılı (K-519) ispat yükümlülüğüdür ve ödemesi gerçekten hiç alınmamış sipariş için geçerli kalıyor. Kişisel verinin amaçla sınırlı saklanması KVKK m.4/2-d'dir. On yıllık saklama burada kanunda öngörülen süreye dayanır.
- **BULGU-2:** Mesafeli Sözleşmeler Yönetmeliği m.16/2–3 (K-499'da konsolide metne karşı doğrulanmış). Fesih sözleşmeyi o kalemler için sona erdirir; satıcı tahsil edilen bedeli fesih bildiriminden itibaren on dört gün içinde bir kez geri öder. Feshedilen malın yeniden gönderilmesi ve ikinci bir geri ödeme bu hükümle çelişir.

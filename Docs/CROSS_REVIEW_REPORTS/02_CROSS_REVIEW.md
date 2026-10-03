# Cross-Review — 02 Product Requirements (Tur 1)

**Tarih:** 2026-10-03 · **Hedef:** `Docs/02_PRODUCT_REQUIREMENTS.md` v0.15 · **İkinci model:** Codex CLI — ChatGPT hesabı, `gpt-5.6-terra`, düşünme düzeyi yüksek (salt okuma) · **Girdi:** yalnız doküman ve boş şablonu (`e74883f`), yeni ve izole bir çalışma alanında; karar kaydı ve önceki raporlar verilmedi (K-431)

## 1. Ham bulgular

İkinci modelin çıktısı, düzenlenmeden (alıntı bloğu içinde). İstem yedi kriteri, bulgu biçimini ve `01`'in döngüsünde eklenen "bilinçli karara yalnız katılmadığın için itiraz etme" cümlesini taşıdı.

> ```text
> BULGU-1
> Kriter: eksiklik
> Seviye: Yüksek
> Yer: §3.14 Adres
> Alıntı: "Adreste il serbest metin değildir; 81 ilden oluşan kapalı listeden seçilir"
> Sorun: Teslimat ve fatura adresinin zorunlu alanları hiç tanımlanmamış. Özellikle misafir siparişinde alıcı adı, ayrıntılı adres, ilçe ve gerektiğinde telefonun nasıl alınacağı belirsizdir; kargo teslimatı ve fatura verisi üretilemez.
> Öneri: Teslimat ve fatura adresi için zorunlu/isteğe bağlı alanları, doğrulamalarını, teslim alıcı adını ve siparişe donan alanları açıkça tanımlayın.
>
> BULGU-2
> Kriter: güvenlik
> Seviye: Orta
> Yer: §3.22 Sipariş numarası ve takip erişimi
> Alıntı: "sipariş e-postasındaki bağlantı sipariş sayfasını doğrudan açar ve ayrıca e-posta sormaz"
> Sorun: E-postadaki doğrudan erişim bağlantısının ömrü, iptal edilme koşulları ve e-posta değişimi/hesap silinmesi sonrası davranışı tanımlı değil. Süresiz bir bearer bağlantı, iletilmiş veya sızmış bir e-postayla sipariş ve kişisel verilere kalıcı erişim verebilir.
> Öneri: Bağlantının süreli ve yüksek entropili bir erişim belirteci olduğunu; süresi dolunca sipariş numarası + e-posta yoluna dönüleceğini; hangi olaylarda geçersizleşeceğini tanımlayın.
>
> BULGU-3
> Kriter: teknik doğruluk
> Seviye: Orta
> Yer: §3.9 İndirim ve referans fiyat
> Alıntı: "10 günlük geçmişi olmayan malda referans, ürünün yayına girdiğinden beri uygulanmış en düşük fiyattır"
> Sorun: Referans fiyat varyant düzeyinde hesaplanırken başlangıç ürüne bağlanmış. Ürün yayındayken sonradan eklenen bir varyantın, ürünün önceki döneminde kendisine uygulanmış bir fiyatı olmaz; hesap tanımsız kalır.
> Öneri: Geçmişi eksik varyantlarda başlangıcı, varyantın ilk kez satışa sunulduğu/yayına alındığı veya ilk fiyatının uygulandığı an olarak tanımlayın.
>
> BULGU-4
> Kriter: kullanıcı deneyimi
> Seviye: Orta
> Yer: §3.11.4 Ürün görseli ve metni
> Alıntı: "firma yazmazsa ürün adı kullanılır; çıktı hiçbir zaman boş kalmaz"
> Sorun: Ürün adını tüm görsellere otomatik alternatif metin yapmak, farklı açı, renk veya ayrıntı gösteren görselleri ekran okuyucu kullanıcıları için ayırt edilemez kılar. Bu yaklaşım §3.34.5’teki WCAG 2.1 AA hedefini de pratikte karşılamaz.
> Öneri: Bilgi taşıyan her görsel için anlamlı alternatif metin zorunlu kılın; dekoratif görselleri ayrıca işaretleyip ekran okuyucudan gizleme kuralını tanımlayın.
>
> SONUÇ: 4 BULGU
> ```

## 2. Bağımsız değerlendirme

| # | Karar | Gerekçe | Uygulanan düzeltme |
|---|---|---|---|
| BULGU-1 | ✅ KABUL | Doğru: `02` adresin varlığını (K-111), fatura–teslimat ayrımını (K-112), il listesini (K-133) ve tipe göre istenişini (K-194) yazıyor ama alanlarını hiç yazmıyor; şube (§3.27.7) ve iletişim formu (§3.32.2) alan setlerini taşırken adres taşımıyor. Karar kaydında da alan seti yok; K-357 sipariş kaydının kişisel verisini "ad, adres, e-posta, telefon" diye sayıyor ama telefonun nereden geldiği yazılı değildi. Hukuki kontrol: nihai tüketiciye düzenlenen faturada T.C. kimlik numarası zorunlu değildir; e-Arşiv'in teknik alanına 11111111111 yazılır — bu yüzden istenmez. | K-561 (öneriyle kaydedildi — ⚠, kişisel veri): yeni §3.14.6 — ad-soyad, il, ilçe (serbest metin) ve açık adres zorunlu; teslimat adresinde telefon zorunlu, fatura adresinde yok; posta kodu ve T.C. kimlik numarası istenmez; adres defterinde telefon isteğe bağlı, teslimatta eksikse ödeme adımında istenir. §1.2'nin iki adres satırı ve §12.2.3'ün sipariş verisi listesi hizalandı. |
| BULGU-2 | ⚠️ KISMİ | Önerinin çekirdeği (süreli bağlantı, süre dolunca numara + e-posta) K-187'nin bilinçle kabul ettiği riski geri alır ve §3.22.4'ün "her akış sipariş sayfasında" kuralını — dijital ürünün yeniden indirilmesi (K-143) dahil — zayıflatır; önceki karardan ayrılmayı gerektirir, uygulanmadı. Hesap silinince ya da e-posta değişince geçersizleşme de yanlış olur: sipariş iletişim e-postasını dondurur ve hesaptan bağımsız yaşar (§3.15.2, §3.23.3). **Ama altta gerçek bir boşluk var:** K-187 bağlantının tahmin edilemezliğini sipariş numarasına dayandırıyordu. Bağlantı yalnız numarayı taşısaydı, faturada ve iletişim talebinde dolaşan (§3.32.2 numaranın mesaja yazılmasını söyler) numarayı bilen herkes sayfayı e-posta sorulmadan açardı ve IP başına deneme limiti de bu yola işlemezdi. `02` ayrıca bağlantının ömrünü ve kalan riski hiç yazmıyordu. | K-562 (öneriyle kaydedildi — ⚠, kişisel veri): yeni §3.22.5 — bağlantı numaradan ayrı, siparişe özgü, tahmin edilemez bir erişim anahtarı taşır; numara tek başına sayfayı açmaz; bağlantının kendi ömrü yoktur, siparişin kişisel verileri imha edilene kadar (Z-28, Z-41) çalışır ve hesap olaylarından etkilenmez; iletme riski adıyla kalan risk olarak yazıldı. §8.3.5 hizalandı. |
| BULGU-3 | ✅ KABUL | Doğru: §3.9.3 referansı varyant düzeyinde tanımlıyor ("o varyanta uygulanmış"), §3.9.4 ise kısa geçmişin başlangıcını ürünün yayına girişine bağlıyordu; ürün yayındayken eklenen varyantın o dönemde uygulanmış fiyatı yok. Cevap önceki kararlardan çıkıyor: fiyat varyantta yaşar (K-39), varyantın kendi yayın durumu vardır (K-55, §5.1). Fiyat Etiketi Yönetmeliği m.11/1 kısa geçmişi düzenlemiyor; K-67 bir ürün politikasıdır ve yasal pencereyi (K-508) değiştirmiyor. | K-563 (öneriyle kaydedildi): §3.9.4 — başlangıç, varyantın satışa sunulduğu, yani kendisinin ve ürününün birlikte Yayında olduğu ilk andır; diğer varyantların fiyatı referans olmaz. §1.2 Referans fiyat ve §4.2 Z-21 hizalandı. |
| BULGU-4 | ⚠️ KISMİ | "Alan zorunlu olsun" önerisi K-93'ün bilinçle elediği seçenektir (çok görselli üründe "resim1, resim2" üretir — ürün adından kötü) ve uygulanmadı; "dekoratif görsel işareti" ürün ve içerik görselleri bilgi taşıdığı için firmaya gereksiz bir seçim yükler. Ama sorunun çekirdeği gerçek: geri düşüş her görsele aynı metni veriyordu, ekran okuyucu kullanıcısı beş görseli aynı adla duyar ve renkleri ayırt edemez. K-395'in ikinci kuralı karşılanıyor ama işlevi zayıf. Ayrıca `02` alanın neden zorunlu olmadığını yazmıyordu; bu, aynı bulgunun sonraki turlarda dönmesine yol açardı. | K-564 (öneriyle kaydedildi): §3.11.4 — üretilen metin görselleri ayırır: varyant görselinde ürün adı + seçenek değerleri, galeride ürün adı + sıra; vitrin kartında yalnız ürün adı; zorunlu olmamanın gerekçesi tek cümleyle yazıldı. §3.27.15 kurumsal içeriğe aynı kuralı uygular. |

**Dağılım:** 2 KABUL · 2 KISMİ · 0 RET.

## 3. Ek bulgular

Yok. BULGU-1'in düzeltmesi sırasında §12.2.3'ün hukuki sebep listesinin telefonu saymadığı görüldü; aynı kararla (K-561) kapandı.

## 4. Kullanıcı onay checklist'i

Proje sahibinin talimatıyla ("kolay soruları bana sorma sen karar ver, çok çok kritik konuları sadece sor") yönetici onayladı ve uygulandı (`02` v0.16). Dört karar satırı açıldı: K-561 ve K-562 `(öneriyle kaydedildi — ⚠)` — kişisel veriye dokundukları için proje sahibinin gözden geçirme listesine girer; K-563 ve K-564 `(öneriyle kaydedildi)`. `10`'un KP-12 ve KP-42 satırlarına dokunan etki yansıtma sonraki adımda toplu yapılır; bu turda bariz bir çelişki doğmadı.

- [x] BULGU-1 · [x] BULGU-2 · [x] BULGU-3 · [x] BULGU-4

**Kaynak (hukuki kontrol):** [TÜRMOB — e-Arşiv Uygulama Kılavuzu](https://www.turmob.org.tr/arsiv/mbs/resmigazete/-e-arsivkılavuz.pdf) · [Faturaport — nihai tüketiciye fatura](https://faturaport.com/blog/e-donusum/nihai-tuketici-nedir-nihai-tuketiciye-fatura-nasil-kesilir--faturaport)

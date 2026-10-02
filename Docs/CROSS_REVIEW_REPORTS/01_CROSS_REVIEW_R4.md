# Cross-Review — 01 Project Vision (Tur 4)

**Tarih:** 2026-10-02 · **Hedef:** `Docs/01_PROJECT_VISION.md` v0.12 · **İkinci model:** `cursor-agent` (salt okuma) · **Girdi:** yalnız doküman ve boş şablonu, yeni ve izole bir çalışma alanında; model önceki turları görmedi (K-431)

## 1. Ham bulgular

İkinci modelin çıktısı, düzenlenmeden (alıntı bloğu içinde). Model son bulguyu ve sonuç satırını cevabın başında bir kez daha tekrar etti.

> ```text
> BULGU-1
> Kriter: Tutarlılık
> Seviye: Yüksek
> Yer: §6 Ü-3
> Alıntı: "Bir siparişin hattında sistem içi zorunlu elle adım üçü geçmez (K-404). Bütçe tek tipli siparişin hattına uygulanır; karışık siparişte hatların adımları toplanır (K-454)" / "havale ile ödenen fiziksel + hizmet siparişi dört adımdır (K-454)"
> Sorun: Aynı kriter önce üst sınırı üç diye koyuyor; hemen ardından karışık siparişte dört adımı geçerli sayıyor. Kabul kapısı olan Ü-3'ün üç mü, yoksa tek tipte üç / karışıkta toplam mı ölçtüğü çelişiyor.
> Öneri: Ü-3'ün ilk cümlesini §1'deki gibi yalnızca tek tipli hatta bağlayın; karışık sipariş için ayrı kuralı (toplamın kabulü veya ayrı tavan) tek cümlede netleştirin.
> 
> BULGU-2
> Kriter: Tutarlılık
> Seviye: Orta
> Yer: §8 V-2 (kendi kuralları: §8 giriş maddeleri)
> Alıntı: "her satırın \"Nasıl doğrulanacak\" hücresi neye bakılacağını ve neyin görülmesi hâlinde varsayımın yanlışlanmış sayılacağını yazar (K-487)" / "tetikleyici gerçekleşirse yanlışlanır" / "Üç aylık bir dönemin en az iki ayında en yoğun gün 50 siparişi aşarsa kargo şirketi entegrasyonu ve havale eşleştirmesi post-MVP'den öne çekilir"
> Sorun: §8 kendi kuralına göre yanlışlama koşulu "Nasıl doğrulanacak" hücresinde olmalı; V-2'de koşul "Yanlışsa ne olur" sütununa yazılmış, "Nasıl" hücresi yalnızca tanımsız "tetikleyici"ye gönderme yapıyor.
> Öneri: 50 sipariş / iki ay koşulunu "Nasıl doğrulanacak" hücresine taşıyın; "Yanlışsa ne olur"da yalnız sonucu (darboğaz, öne çekilecek adaylar) bırakın.
> 
> BULGU-3
> Kriter: Kullanıcı deneyimi
> Seviye: Orta
> Yer: §2; §7 S-2
> Alıntı: "Fiyat sorusu olan ziyaretçi iletişim formunu kullanır: form bir iletişim talebi kaydı açar ve bu bir satış hattı değildir (K-11, K-300)." / "İletişim formu bir satış hattı değildir"
> Sorun: Yayındaki her ürünün fiyatı varken ziyaretçi "fiyat sorusu" için forma yönlendiriliyor; formun satış/teklif üretmediği de aynı nefeste söyleniyor. Ziyaretçi için sorunun nerede sonuçlanacağı belirsiz ve çelişkili.
> Öneri: Ya fiyat sorusunu katalog/satın alma dışına çıkaran somut bir gerekçe yazın (ör. katalog dışı iş) ya da bu yönlendirme cümlesini kaldırıp iletişim formunu yalnızca genel iletişimle sınırlayın.
> 
> BULGU-4
> Kriter: Belirsizlik
> Seviye: Orta
> Yer: §3.2; §3.1
> Alıntı: "ürün 6502 sayılı Kanun'un tüketici korumalarını istisnasız her siparişe uygular (K-07)" / "Mesafeli Sözleşmeler Yönetmeliği'nin istisnaları da üründe kuruludur: dijital ürün, ifası tamamlanmış hizmet ve istisna işaretli fiziksel ürün"
> Sorun: "İstisnasız" hem alıcı sıfatına bakılmaksızın hem de cayma/iade istisnasız tüm korumalar anlamında okunabilir; ikinci okuma §3.1'deki istisnalarla çelişir.
> Öneri: "İstisnasız"ı "alıcının sıfatına bakılmaksızın" diye daraltın; cayma istisnalarının saklı olduğunu aynı cümlede çaprazlayın.
> 
> BULGU-5
> Kriter: Belirsizlik
> Seviye: Orta
> Yer: §3.2 Misafir alıcı; §2
> Alıntı: "Yeniden satın almak için geri dönmesi beklenmez: tanımı gereği sürekliliği yoktur — bir kez alır, gider (K-100)." / "bunun için üye olması gerekmez"
> Sorun: Aktör tanımı misafiri tek seferlik alıcı gibi kuruyor; çözüm ise üyeliksiz siparişi genel olarak açık bırakıyor. İkinci (üçüncü…) misafir siparişinin bu aktör tanımına uyup uymadığı, "bir kez"in kural mı yoksa beklenti mi olduğu belirsiz.
> Öneri: "Bir kez alır, gider"i "hesap sürekliliği yoktur; her misafir siparişi bağımsızdır" diye değiştirin; tekrar misafir alışverişinin mümkün olduğunu açıkça yazın.
> 
> BULGU-6
> Kriter: Belirsizlik
> Seviye: Düşük
> Yer: §8 V-1; §1
> Alıntı: "fiziksel üründe katalog birkaç yüz ürüne kadardır" / "yayındaki fiziksel ürün sayısı 1.000'i aşarsa ... yanlışlanır"
> Sorun: Varsayım "birkaç yüz" iken yanlışlama eşiği 1.000; 500–999 aralığında varsayımın hâlâ geçerli mi sayılacağı ölçülemiyor.
> Öneri: Ya varsayımı "1.000'in altında" diye eşiğe hizalayın ya da yanlışlama eşiğini "birkaç yüz"ün üst sınırına (ör. 500) indirin.
> 
> SONUÇ: 6 BULGU
> ```

## 2. Bağımsız değerlendirme

| # | Karar | Gerekçe | Uygulanan düzeltme |
|---|---|---|---|
| BULGU-1 | ✅ KABUL | Ü-3'ün ilk cümlesi "bir siparişin hattında" dese de kriter adı ve ardından gelen dört adımlı örnek, okuyucunun tavanın sipariş mi hat mı başına olduğunu çözmesini istiyordu. K-454 açık: bütçe hat başınadır, karışık siparişin toplamı tavana bağlı değildir. | Ü-3: "Bütçe hat başınadır: tek tipli bir siparişin hattında … üçü geçmez. Karışık siparişte hatların adımları toplanır ve toplam bu tavana bağlı değildir — her hat kendi tavanını korur (K-454)" |
| BULGU-2 | ✅ KABUL | §8'in v0.9'da eklenen kendi kuralı (K-487) yanlışlama koşulunu "Nasıl doğrulanacak" sütununa koyuyor; V-2 bu kurala uymuyordu, koşul "Yanlışsa ne olur" sütunundaydı. | V-2: koşul (üç aylık dönemin en az iki ayında en yoğun gün 50'yi aşması) "Nasıl doğrulanacak" hücresine taşındı; "Yanlışsa ne olur"da yalnız sonuç kaldı. |
| BULGU-3 | ⚠️ KISMİ | Yönlendirme cümlesi kaldırılamaz: K-11'in kendisidir. Ama gerekçesi eksikti — K-11'in gerekçe hücresi bu yolun ısmarlama iş satan KOBİ için olduğunu söylüyor; katalogdaki her ürünün fiyatı zaten görünür. | §2: "Katalogda olmayan, ısmarlama bir iş için fiyat soran ziyaretçi iletişim formunu kullanır … katalogdaki her ürün fiyatıyla doğrudan satın alınır (K-11, K-300)." |
| BULGU-4 | ✅ KABUL | Tur 2'de cümle ürün politikasına çevrilmişti, ama "istisnasız" kelimesi §3.1'deki cayma istisnalarıyla çelişik okunabiliyordu. Kelimenin K-07'deki anlamı alıcının sıfatıdır. | §3.2: "… tüketici korumalarını, alıcının sıfatına bakmaksızın her siparişe uygular; Mesafeli Sözleşmeler Yönetmeliği'nin cayma istisnaları bu rejimin içindedir (§3.1 — K-07)." |
| BULGU-5 | ✅ KABUL | K-100'ün "bir kez alır, gider" cümlesi süreklilik beklentisidir, sipariş sayısına konan bir kural değildir; misafir alışverişinin tekrarını hiçbir karar kısıtlamıyor (K-97). Okuyucu bunu kural sanabiliyordu. | §3.2 misafir alıcı: "Bu bir beklentidir, kural değildir: misafir yeniden sipariş verebilir, ama her misafir siparişi bağımsızdır ve öncekilere bağlanmaz (K-97)." |
| BULGU-6 | ⚠️ KISMİ | Eşik 500'e indirilmez: K-487 eşiği bilinçli olarak "birkaç yüz"ün bir büyüklük mertebesi üstüne koydu — yanlışlama, tasarımı yeniden düşündürecek bir sapmayı ölçer, varsayımdan her küçük sapmayı değil. Eksik olan aradaki bölgenin adlandırılmasıydı. | V-1: "… 1.000'i aşarsa — 'birkaç yüz' ile 1.000 arası varsayımın içinde sayılır, eşik tasarımın yeniden düşünülmesini gerektiren büyüklüktür — …" |

**Dağılım:** 4 KABUL · 2 KISMİ · 0 RET.

## 3. Ek bulgular

Yok.

## 4. Kullanıcı onay checklist'i

Oturum açılışındaki talimat gereği uygulandı (`01` v0.13). Yeni karar satırı açılmadı.

- [x] BULGU-1 · [x] BULGU-2 · [x] BULGU-3 · [x] BULGU-4 · [x] BULGU-5 · [x] BULGU-6

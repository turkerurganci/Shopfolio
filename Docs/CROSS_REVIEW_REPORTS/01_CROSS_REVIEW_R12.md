# Cross-Review — 01 Project Vision (Tur 12)

**Tarih:** 2026-10-02 · **Hedef:** `Docs/01_PROJECT_VISION.md` v0.20 · **İkinci model:** Codex CLI — ChatGPT hesabı, `gpt-5.6-terra` (salt okuma) · **Girdi:** yalnız doküman ve boş şablonu, yeni ve izole bir çalışma alanında; model önceki turları görmedi (K-431)

## 1. Ham bulgular

İkinci modelin çıktısı, düzenlenmeden (alıntı bloğu içinde).

> ```text
> BULGU-1  
> Kriter: Güvenlik  
> Seviye: Yüksek  
> Yer: §3.2, Misafir alıcı  
> Alıntı: "Hesap açarsa doğrulanmış e-postasına ait geçmiş misafir siparişleri hesabına düşer ve üye müşterinin sürekliliğine geçer (K-98); adres el değiştirmişse eski siparişlerin yeni sahibine görünmesi bilinçle kabul edilmiş bir risktir (K-489)"  
> Sorun: E-posta adresinin sonradan başka bir kişiye geçmesi, eski siparişler ve kişisel verilerin yetkisiz kişiye açılmasına yol açar. Riskin “bilinçle kabul edilmiş” olması, kişisel veriye yetkisiz erişimi güvenli hâle getirmez.  
> Öneri: Geçmiş misafir siparişlerini yalnız e-posta eşleşmesiyle otomatik bağlama. Her sipariş için ek doğrulama gerektir veya geçmiş sipariş aktarımını açık, ayrı onaylı bir süreç yap.
> 
> BULGU-2  
> Kriter: Teknik doğruluk  
> Seviye: Yüksek  
> Yer: §3.2, Aktör olmayanlar — Ticari / kurumsal alıcı  
> Alıntı: "Ürünün alıcısı tüketicidir ve bu ürünün kalıcı sınırıdır (§7 S-6 — K-470): ürün tek bir akış sunar ve bu akış 6502 sayılı Kanun'un tüketiciye tanıdığı korumaları — Mesafeli Sözleşmeler Yönetmeliği'nin cayma istisnalarıyla birlikte (§3.1) — her siparişte işletir (K-07)."  
> Sorun: Aynı paragraf ticari amaçla alan kişinin de aynı akıştan geçebileceğini ve tüketici sayılıp sayılmayacağını mevzuatın belirlediğini söyler. Buna rağmen 6502 kapsamındaki korumaların “her siparişte” işletildiği iddiası, kanuni tüketici olmayan alıcılara bu korumaların kendiliğinden uygulanacağı izlenimini verir.  
> Öneri: Tek akışın yalnız kullanıcı deneyimi olduğunu açıkça ayır; kanuni tüketici korumalarının hukuken tüketici sayılan alıcılara uygulandığını yaz. Ticari alıcılara da aynı haklar gönüllü olarak verilecekse bunu kanuni koruma değil, açık bir ürün/sözleşme politikası olarak tanımla.
> 
> BULGU-3  
> Kriter: Edge case  
> Seviye: Orta  
> Yer: §2 ve §7 S-5  
> Alıntı: "yayındaki her ürünün fiyatı vardır ve satın alınabilir" / "stok varyant başına tutulan ve sipariş geldikçe azalan bir sayıdır"  
> Sorun: Stoku sıfıra inen veya eşzamanlı siparişler nedeniyle yeterli stoku kalmayan yayındaki fiziksel ürünün davranışı tanımlı değildir. Ürün yayında kalırsa §2’deki “satın alınabilir” kuralıyla fazla satış riski doğar; yayından kalkarsa bunun hangi koşulda ve nasıl olacağı belirsizdir.  
> Öneri: Stok sıfırda ve yetersiz stokta ürünün satın alınabilirliğini açıkça tanımla; örneğin ilgili varyantın sepete eklenememesi, ürünün yayında fakat “tükendi” görünmesi ve eşzamanlı siparişlerde stok ayırma kuralı.
> 
> BULGU-4  
> Kriter: Eksiklik  
> Seviye: Orta  
> Yer: §6, M-1–M-4  
> Alıntı: "Mağaza düzeyi ölçüler bir başarı eşiği değil, post-MVP sıralamasını besleyen izleme ölçüleridir ve hedef değer taşımaz"  
> Sorun: Şablonun başarı kriterleri için istediği ölçülebilir hedefler, M-1–M-4 için verilmemiştir. “Düşük” veya “yüksek” sayılacak değerler yalnız `10 §5`e bırakıldığından, bu doküman kendi başına mağaza sonuçlarının nasıl yorumlanacağını belirlemez.  
> Öneri: Her mağaza ölçüsü için burada karar eşiği tanımla veya bu satırları başarı kriterlerinden çıkarıp ayrı bir “izleme ölçüleri” bölümüne taşı.
> 
> SONUÇ: 4 BULGU
> ```

## 2. Bağımsız değerlendirme

| # | Karar | Gerekçe | Uygulanan düzeltme |
|---|---|---|---|
| BULGU-1 | ❌ RET | Tur 8 ve 10'un tekrarı. Proje sahibi bu riski bu oturumda tarttı ve kuralın kalmasına karar verdi (K-489); önerilen "otomatik bağlamayı kaldır" seçeneği orada adıyla elendi. Doküman riski gizlemiyor, bilinçle kabul edildiğini yazıyor. | — |
| BULGU-2 | ⚠️ KISMİ | Konu beşinci kez geldi; her turda cümle biraz daha ayrıştı. Kalan pürüz "korumaları işletir" ifadesiydi: ticari alıcıya kanunun kendiliğinden uygulandığı izlenimini veriyordu. Doğru ayrım şu: ürün tek bir sözleşme akışı sunar ve bu akışın hakları her siparişin sözleşmesinde yer alır (K-07'nin "istisnasız her siparişte" özü); kanuni korumanın kime uygulandığını mevzuat belirler. Ticari alıcıya ayrı akış açmak bir varlık kararıdır ve K-07'ye aykırıdır. | §3.2: "… bu akış tüketici mevzuatına göre kurulmuştur: ön bilgilendirme, mesafeli satış sözleşmesi, cayma ve iade … her siparişin parçasıdır … ticari amaçla alan biri de aynı akıştan geçer ve aynı sözleşmeyi kurar. Bu bir ürün ve sözleşme politikasıdır: kanuni tüketici korumalarının kime uygulandığını ürün değil, mevzuat belirler." |
| BULGU-3 | ✅ KABUL | §2 "yayındaki her ürün satın alınabilir" diyordu; stoğu biten ürünün durumu yazılı değildi. Kayıt cevabı veriyor: tükenmiş varyant seçilemez, bütün varyantları tükenmiş ürün vitrinde "Tükendi" olarak kalır ve sepete eklenemez (K-51, K-52); eşzamanlı siparişte ayrılmış adetler düşülür (`02 §3.6.3`). | §2: "… stoğu olduğu sürece satın alınabilir — stoğu biten varyant ve ürün vitrinde kalır, 'Tükendi' görünür ve sepete eklenemez (K-51, K-52)" |
| BULGU-4 | ❌ RET | Tur 1 ve 8'in tekrarı. Mağaza ölçülerinin hedefsizliği bilinçlidir ve dokümanda yazılıdır: kabul kapısı değildirler (K-418), iki eksen bilinçli olarak tek tabloda tutuldu (K-433) ve "düşük/yüksek" eşiği sıralama ölçütünün evinde (`10 §5`) açık bir kalem olarak izleniyor (A-13). | — |

**Dağılım:** 1 KABUL · 1 KISMİ · 2 RET. İki ret, ikinci modelin göremediği değil, dokümanda zaten yazılı olan bilinçli kararlara itirazdır. Bir sonraki turun talimatına bunu ayıran bir cümle eklendi (aşağıda).

## 3. Ek bulgular

Yok.

## 4. Kullanıcı onay checklist'i

Oturum açılışındaki talimat gereği uygulandı (`01` v0.21). Yeni karar satırı açılmadı.

- [x] BULGU-1 · [x] BULGU-2 · [x] BULGU-3 · [x] BULGU-4

**Talimatta yapılan değişiklik (13. turdan itibaren):** Son dört turda ikinci model, dokümanda bilinçli karar ya da kabul edilmiş risk olarak açıkça yazılmış üç seçime yeniden itiraz etti: misafir siparişlerinin bağlanması (K-489), mağaza ölçülerinin hedefsizliği (K-418, K-433) ve tek tüketici akışı (K-07). Bunlar dokümanın kusuru değil, ürün kararlarına katılmamaktır. Talimata şu cümle eklendi: *"Dokümanda açıkça bilinçli karar, elenen seçenek ya da kabul edilmiş risk olarak yazılmış bir seçimi, yalnız seçime katılmadığın için bulgu yapma. Böyle bir karar dokümanın kendi içinde çelişki, olgusal ya da hukuki hata doğuruyorsa yaz."* Girdi değişmedi: yine yalnız doküman ve şablon (K-431); karar kaydı verilmedi.

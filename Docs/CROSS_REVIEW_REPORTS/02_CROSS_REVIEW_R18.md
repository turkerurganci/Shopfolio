# Cross-Review — 02 Product Requirements (Tur 18)

**Tarih:** 2026-10-03 · **Hedef:** `Docs/02_PRODUCT_REQUIREMENTS.md` v0.33 · **İkinci model:** Codex CLI — ChatGPT hesabı, `gpt-5.6-terra`, düşünme düzeyi yüksek (salt okuma) · **Girdi:** yalnız doküman ve boş şablonu (`e74883f`), yeni ve izole bir çalışma alanında; karar kaydı, önceki turlar ve raporlar verilmedi (K-431)

## 1. Ham bulgular

İkinci modelin çıktısı, düzenlenmeden (alıntı bloğu içinde; satır sonlarındaki boşluklar atıldı). İstem önceki turlarınkiyle aynıdır: yedi kriter, bulgu biçimi ve "bilinçli karara yalnız katılmadığın için itiraz etme" cümlesi.

> ```text
> BULGU-1
> Kriter: Tutarlılık
> Seviye: Orta
> Yer: §3.7 Ürünün yayın durumu, arşiv ve silme
> Alıntı: "Üç durum arasındaki her geçiş serbesttir"
> Sorun: Bu ifade, §3.7.8’deki “son Yayında varyant”ın Taslak veya Arşiv’e alınmasının engellenmesiyle çelişir. Geçiş kuralını uygulayan ekip, genel ifadeyi veya istisnayı önceliklendirmek zorunda kalır.
> Öneri: §3.7.1’i “Geçişler, yayın kapısını ve §3.7.8’deki son Yayında varyant kısıtını korumak koşuluyla serbesttir” şeklinde düzeltin; §5.1’de de aynı istisnayı ana geçiş kuralına bağlayın.
>
> BULGU-2
> Kriter: Edge case
> Seviye: Orta
> Yer: §6.2 Satın alma
> Alıntı: "ödeme sayfasına hiç gidilmemiş siparişte ayrılmış bir şey varsa hemen serbest kalır"
> Sorun: Sipariş onayında stok, kontenjan ve kupon hakkı ayrılır; fakat bu senaryoda ayrımın serbest bırakılmasından sonra siparişin/ödemenin hangi duruma geçeceği yazılı değildir. Sipariş Alındı + Bekliyor kalırken ayırmasının kalkması, daha sonra ödeme sonucu gelirse stok ve kupon kurallarını bozabilir.
> Öneri: Sağlayıcıya geçilemeden hata oluşursa atomik sonucu tanımlayın: siparişi İptal edildi + Başarısız’a geçirip tüm ayırmaları serbest bırakın ya da siparişi hiç oluşturmayın. Sepet, bildirim ve yeniden deneme davranışını da bu seçime bağlayın.
>
> BULGU-3
> Kriter: Teknik doğruluk
> Seviye: Yüksek
> Yer: §10.4.5 Teslim tarihi düzeltmesi
> Alıntı: "cayma penceresi ve ayıp talebi süresi düzeltilmiş tarihten işler"
> Sorun: Yönetici teslim tarihini sonradan değiştirerek müşterinin hâlihazırda gördüğü cayma ve ayıp başvuru süresini geriye dönük kısaltabilir. Teslim tarihi müşterinin fiilî teslimine ilişkin bir olgudur; yalnız paneldeki değiştirilebilir değer olamaz. Ayrıca düzeltme müşteriye bildirim üretmez.
> Öneri: İlk girilen tarihi ve düzeltme geçmişini saklayın; düzeltme için gerekçe/kanıt gerektirin. Müşteriye gösterilmiş aktif süre geriye dönük kısaltılmamalı; zorunlu düzeltmede müşteri bilgilendirilmeli ve ihtilaflı talepler manuel inceleme yoluna alınmalıdır.
>
> BULGU-4
> Kriter: Eksiklik
> Seviye: Yüksek
> Yer: §3.8 Fiyat ve KDV / §3.10 Kupon
> Alıntı: "KDV kalem bazında hesaplanır ve kuruşa orada yuvarlanır."
> Sorun: Kuponun kalemlere dağıtıldığı yazılı, ancak kupon sonrası kalem matrahı ve KDV’sinin nasıl yeniden hesaplanacağı ile yuvarlama sırası tanımlı değildir. Kupon öncesi KDV’nin korunması, müşterinin ödediği toplam ile fatura KDV/matrah dökümünün uyuşmamasına yol açabilir.
> Öneri: Sıralamayı açıkça tanımlayın: ürün indirimi sonrası tutar → kuponun kalem bazında dağıtılması → kupon sonrası kalem KDV/matrah hesabı ve kuruş yuvarlaması. Kalan kuruşun hem kupon hem KDV dağıtımında hangi kaleme yazılacağını ve siparişte donacak tutarları belirtin.
>
> SONUÇ: 4 BULGU```

## 2. Bağımsız değerlendirme

| # | Karar | Gerekçe | Uygulanan düzeltme |
|---|---|---|---|
| BULGU-1 | ✅ KABUL | Doğru ve küçük. §3.7.1 *"Üç durum arasındaki her geçiş serbesttir"* diyordu; §5.1 de *"yasak geçiş yoktur … Tek koşul yayın kapısıdır"* diyordu. Ama §3.7.8 ve §5.1'in bir sonraki maddesi yayındaki ürünün son Yayında varyantının arşivlenmesini ve taslağa çekilmesini engelliyor (K-299, K-538). Kural tek bir kaynaktan geliyor: yayın kapısı yalnız Yayında'ya geçişte değil, ürün yayındayken de korunuyor. Ama "tek koşul" cümlesi kapıyı yalnız Yayında'ya geçişin koşulu gibi okutuyordu. Dijital ürünün son dosyasının silinmesindeki engel de (§3.7.4) aynı ilkeden gelir. | Yeni karar satırı açılmadı. §3.7.1: tek sınır yayın kapısıdır; kapı ürün yayındayken de korunur ve onu bozan işlem engellenir (§3.7.4, §3.7.8). §5.1'in "Geçişler serbesttir" maddesi aynı biçimde düzeltildi: "kendi başına yasak bir geçiş yoktur", tek sınır yayın kapısıdır. |
| BULGU-2 | ✅ KABUL | Boşluk gerçekti. §6.2.15 *"ödeme sayfasına hiç gidilmemiş siparişte ayrılmış bir şey varsa hemen serbest kalır"* diyordu, ama siparişin durumunu söylemiyordu. Sipariş onay anında oluşur ve stok o anda ayrılır (§3.17.1, §3.17.3). Bu yüzden ayırmanın serbest kaldığı bir sipariş vardır ve onun ne olduğu yazılı olmalıdır. Cevap karar kaydında zaten var. K-180 iki tetikleyici sayar: *"ödeme süresinin dolması ya da kart ödemesinin başarısızlığı"*. Sonucu da yazar: *"Sipariş kendiliğinden İptal edildi'ye geçer, ödeme durumu Başarısız olur"*; ayırmalar aynı anda serbest kalır. Sağlayıcıya bağlanılamaması da bir kart ödemesi başarısızlığıdır. K-556 da §6.2.15'in "hemen serbest kalır" ifadesini bu durumla sınırlamıştı. **Önerinin "siparişi hiç oluşturmayın" kolu alınmadı:** sipariş onay anında oluşur ve sağlayıcıya ancak ondan sonra gidilir (K-80, K-128). | Yeni karar satırı açılmadı. §6.2.15: sipariş onaylandıktan sonra sağlayıcıya bağlanılamazsa §6.2.1'in sonucu işler. Sipariş İptal edildi olur, ödemesi Başarısız olur, ayrılanlar aynı anda serbest kalır ve sipariş müşterinin listesinde görünmez (§3.17.5, §3.17.8; K-184). Yeniden deneme yeni bir siparişle olur (§3.17.6; K-181). Geç gelen başarı bildirimi §6.2.16 ile karşılanır. Kaynak sütununa K-180, K-181 ve K-184 eklendi. |
| BULGU-3 | ⚠️ KISMİ | **Önerinin ana kolu bilinçli kararla çelişiyor.** Codex'in ana önerisi şuydu: *"Müşteriye gösterilmiş aktif süre geriye dönük kısaltılmamalı"*. Bu öneri K-340'ın ve §7.3.1'in bilinçli seçimine karşıdır: *"Pencere işaret anından yeniden başlatılmaz — bilinçli bir seçimdir: yasal pencere fiilî teslimden işler ve doğru girilmiş tarihte süre gerçekten dolmuştur; yanlış girilmiş tarihin riski ve ispat yükü firmadadır."* Düzeltme kaydı gerçek teslime yaklaştırır. Eski tarihle korunan bir pencere ise kaydı gerçek teslimden yeniden ayırır. Aynı yön 3. turun BULGU-3'ünde de gelmişti ve aynı gerekçeyle reddedilmişti. **Bildirimin olmaması da bilinçli:** K-375 teslim tarihinin düzeltilmesini bildirimden bilerek çıkardı (*"zaten bildirim üretmeyen bir işaretin tarihidir"*). Teslim işaretinin kendisi de bildirim üretmez (K-288, K-317), çünkü müşteri malı teslim aldığı günü kendisi bilir. **Gerekçe ya da kanıt zorunluluğu ve manuel inceleme alınmadı:** ürün teslimin belgesini toplamaz (K-288: kargo entegrasyonu yok). İspat yükü zaten firmadadır (K-07). Müşterinin itirazının yolu da yazılıdır: caymayı başka bir kanaldan bildirir ve firma onu panelden kaydeder (§7.3.5, §10.4.10; K-510). **Haklı kısım:** ilk girilen tarih ve düzeltme geçmişi K-369'un *"her müdahale işlem izine yazılır — kim, ne zaman, ne değişti"* kuralıyla zaten tutuluyor, ama §10.4.5'te yazılı değildi. Düzeltmenin pencereyi kısaltabileceği ve bunun neden bilinçli olduğu da orada yazılı değildi. Bulgunun dönmesinin sebebi bu eksiklikti. | Karar değişmedi (K-288, K-340, K-375). §10.4.5'e üç şey yazıldı. (1) Düzeltme işlem izinde eski ve yeni tarihle durur; tarih kişisel veri değildir (§10.3.1). (2) Düzeltme pencereyi kısaltabilir ve açık düğmeyi kapatabilir. Bu bilinçlidir: pencere fiilî teslimden işler ve teslim işaretinin kendisi de bildirim üretmez. Pencere ne düzeltme anından yeniden başlar ne de eski tarihle korunur. (3) Yanlış tarihin riski ve ispat yükü firmadadır; itiraz eden müşteri caymayı başka bir kanaldan bildirir (§7.3.5, §10.4.10). |
| BULGU-4 | ✅ KABUL | Doğru. §3.8.3 KDV'nin kalemde hesaplanıp yuvarlandığını söylüyordu, §3.10.3 de sabit tutarlı kuponun payını kalemlere dağıtıyordu. Ama KDV'nin kupon payından önce mi sonra mı hesaplandığı ve yüzdesel kuponun payının nasıl belirlendiği yazılı değildi. Cevap karar kaydında var, dokümana taşınmamıştı. K-71 *"yüzde kuponda her kalem kendi içinde küçülür, KDV'si K-61'in kuralıyla kalemde yeniden hesaplanır"* der. K-72'nin gerekçesi de kuponun KDV'yi düşürdüğünü varsayar: *"100 TL %20'lik kalemden inerse ~16,67 TL … vergi eksilir"*. Hukuken de doğru sıra budur: KDV Kanunu m.25/a, teslim anında yapılan ve faturada gösterilen iskontoları matraha sokmaz. Kupon da satış anında verilir ve fatura verisinde payıyla durur (§3.25.1). **Önerinin "kalan kuruşun KDV dağıtımında hangi kaleme yazılacağı" kolu gerekmedi:** KDV dağıtılmaz, her kalemde ayrı hesaplanır. Toplam da yuvarlanmış kalemlerin toplamıdır (K-61). | Yeni karar satırı açılmadı (K-61, K-71, K-72 yeterli). §3.8.3'e hesap sırası yazıldı. (1) Kalem tutarı, birim fiyatın (indirimliyse kuruşa yuvarlanmış indirimli fiyatın) adetle çarpımıdır. (2) Kalemin kupon payı düşülür. (3) KDV kalan tutardan geriye ayrılır ve kuruşa yuvarlanır; matrah farktır. Kupon KDV matrahına girmez (KDV Kanunu m.25/a). §3.10.3 iki kupon biçimini kapsayacak biçimde yeniden yazıldı. Yüzdesel kuponda her kalem kendi içinde küçülür ve payı kalemde kuruşa yuvarlanır. Sabit tutarlı kuponun kuralı değişmedi. |

**Dağılım:** 3 KABUL · 1 KISMİ · 0 RET.

- **BULGU-1, 2 ve 4:** Daha önce gelmemişti. Üçünün cevabı da karar kaydında vardı; yeni karar gerekmedi, yalnız metin karara hizalandı.
- **BULGU-3:** 3. turun BULGU-3'ünün (geç işaretlenen teslim) yakın akrabasıdır; bu kez konu teslim tarihinin düzeltilmesiydi. Karar yine değişmedi. Gerekçe §10.4.5'e yazıldı; aynı soru artık metinden cevaplanır.
- **RET yok:** Bu tur kararın kendisinde bir hata bulmadı. Dokümanın kararla aynı şeyi söylemediği yerleri buldu. Her bulgu karar kaydına karşı tek tek kontrol edildi. BULGU-3'te önerinin ana kolu reddedildi, BULGU-2 ve BULGU-4'te de birer kol alınmadı.

## 3. Ek bulgular

- **Kargo ücretinin oranlara bölünmesinde kuponun yeri:** §3.19.2'ye göre, fiziksel kalemler farklı oranlardaysa kargo ücreti *"bu kalemlerin tutarına orantılı"* bölünür. Bu tutarın kupondan önceki mi sonraki mi olduğu yazılı değil. Etkisi kuruş düzeyindedir ve yalnız kuponlu, çok oranlı siparişte doğar. Etki yansıtmada K-500 ile birlikte bakılmalı.
- **K-180'e bağlı kendiliğinden iptalde B-7:** B-7'nin kaynak sütunu K-180'i sayıyor. Ama ödemesi hiç alınmamış ve kendiliğinden iptal edilmiş siparişte B-7'nin gidip gitmediği satırda açık değil. Bu sipariş müşterinin listesinde görünmüyor (§3.17.8). Bu turda dokunulmadı; etki yansıtmada ya da sonraki turda bakılmalı.
- **Önceki turlardan açık kalanlar:** fesihten sonra kargonun malı yine de teslim etmesi (11. tur) ve şifre değişikliğinde oturumların ve tanınan tarayıcı işaretlerinin durumu (14. ve 17. tur). Bu turda da gelmedi.
- **Etki yansıtma için not:** `01` ve `10`'da bu turun değiştirdiği cümlelerin karşılığı yok. `06` ve `12` kalemin hesap sırasını (§3.8.3) birebir taşımalı.

## 4. Kullanıcı onay checklist'i

Proje sahibinin talimatıyla ("kolay soruları bana sorma sen karar ver, çok çok kritik konuları sadece sor") yönetici onayladı ve uygulandı (`02` v0.34). Bu turda yeni karar satırı açılmadı; ⚠ işaretli konu yok.

- [x] BULGU-1 · [x] BULGU-2 · [x] BULGU-3 (kısmi; karar değişmedi, gerekçe eklendi) · [x] BULGU-4

**Hukuki kontrol** (2026-10-03):
- **BULGU-4:** KDV Kanunu (3065) m.25/a'ya göre teslim ve hizmet işlemlerinde fatura ve benzeri belgelerde gösterilen, ticari teamüllere uygun iskontolar matraha girmez. Şartı, iskontonun teslim anında yapılıp faturada gösterilmesidir. Kupon satış anında verilir ve fatura verisinde payıyla durur (§3.25.1). Kaynak: TÜRMOB e-kütüphanesi ve muhasebe yayınlarındaki m.25/a açıklamaları (2026-10-03'te tarandı).
- **BULGU-3:** Mesafeli Sözleşmeler Yönetmeliği m.9 pencereyi malın teslim alındığı günden başlatır. Bu, K-340'ta ve 3. turda resmî metinden okunmuştu. Bu turda metne yeni bir hukuki iddia eklenmedi.
- **BULGU-1 ve BULGU-2:** Hukuki iddia içermiyor.

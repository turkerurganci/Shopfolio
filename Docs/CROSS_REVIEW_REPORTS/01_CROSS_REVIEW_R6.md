# Cross-Review — 01 Project Vision (Tur 6)

**Tarih:** 2026-10-02 · **Hedef:** `Docs/01_PROJECT_VISION.md` v0.14 · **İkinci model:** `cursor-agent` (salt okuma) · **Girdi:** yalnız doküman ve boş şablonu, yeni ve izole bir çalışma alanında; model önceki turları görmedi (K-431)

## 1. Ham bulgular

İkinci modelin çıktısı, düzenlenmeden (alıntı bloğu içinde). Model son bulguyu ve sonuç satırını cevabın başında bir kez daha tekrar etti.

> ```text
> BULGU-1
> Kriter: Teknik doğruluk
> Seviye: Yüksek
> Yer: §6 Mağaza düzeyi girişi; M-4 (satır 206, 221)
> Alıntı: "cayma penceresi on dört gün (K-203), geri ödeme çatısı on dört gündür (K-209)" / "dönem bittikten 28 gün sonra okunur — cayma penceresi (K-203) ve geri ödeme çatısı (K-209) dönemin son siparişleri için de kapanmış olsun diye (K-488)"
> Sorun: 28 gün, cayma ve geri ödemenin dönem sonundaki sipariş için peş peşe ve teslimatsız işlediği varsayımına dayanır (14+14). Mesafeli Sözleşmeler Yönetmeliği’nde fiziki mallarda cayma süresi kural olarak malın teslimiyle başlar; kargoya verme süresi de eklenince dönemin son günü verilen bir siparişte iade/geri ödeme penceresi dönem+28’den sonra hâlâ açık kalabilir. M-4 bu gecikmeyle okunursa oran eksik (erken) hesaplanır.
> Öneri: Okuma gecikmesini “azami kargoya verme sözü + teslim + 14 gün cayma + 14 gün geri ödeme” olarak tanımla; ya da paydaya yalnızca her iki penceresi kapanmış siparişleri al ve bunu M-4 satırında yaz.
> 
> BULGU-2
> Kriter: Tutarlılık
> Seviye: Orta
> Yer: §2 Çözüm (satır 69) ↔ §3.2 aktör tablosu (satır 99–102)
> Alıntı: "Ziyaretçi aynı site içinde firmayı tanır, ürüne bakar, sepetini oluşturur, kartla ya da havale/EFT ile öder (K-159) ve siparişini takip eder; bunun için üye olması gerekmez — misafir alıcı siparişini tahmin edilemez sipariş numarası ve e-postasıyla izler"
> Sorun: §3.2’de Ziyaretçi ile Misafir alıcı ayrı aktörlerdir; sipariş verip takip eden üyesiz kişi Misafir alıcıdır. §2 aynı akışın öznesini “Ziyaretçi” yapıp ardından “misafir alıcı”ya kayıyor — aktör kimlikleri çelişiyor.
> Öneri: §2’de ödeme ve takip öznesini “misafir alıcı (veya üye müşteri)” yap; Ziyaretçi’yi tanıtım/katalog ile sınırla.
> 
> BULGU-3
> Kriter: Teknik doğruluk
> Seviye: Orta
> Yer: §3.1 Kapsam dışı bırakılan segmentler (satır 92)
> Alıntı: "Mesafeli Sözleşmeler Yönetmeliği'nin istisnaları da üründe kuruludur: dijital ürün, ifası tamamlanmış hizmet ve istisna işaretli fiziksel ürün"
> Sorun: Yönetmelikte dijital içerik ve ifası tamamlanmış hizmet istisnaları kategorinin kendisinden otomatik doğmaz; tüketicinin ifadan önce açık onayı ve cayma hakkını kaybedeceğine dair bilgilendirilmesi gerekir. Cümle bunları atlayınca “dijital ürün = cayma yok” okunuşuna açıktır ve hukuken eksik/yanlış bir ürün iddiası üretir.
> Öneri: İstisna kalemlerini “yalnız yönetmeliğin aradığı onay ve bilgilendirme koşulları sağlandığında” diye bağla; koşulsuz kategori istisnası yazma.
> 
> BULGU-4
> Kriter: Belirsizlik
> Seviye: Orta
> Yer: §6 M-1 (satır 218)
> Alıntı: "aynı sepetten art arda verilen siparişler tek deneme sayılır (K-416, K-457)"
> Sorun: “Art arda”nın sınırı yok — süre, oturum, sepet değişimi veya başarısız deneme sonrası yeniden deneme mi olduğu belli değil. Aynı kayıtlardan iki farklı ödeme tamamlama oranı çıkarılabilir; ölçü tek anlamlı değil.
> Öneri: Tek deneme kuralını işlet: örn. aynı sepet kimliğiyle, ödeme başarısızlığı/zaman aşımından sonra X dakika içinde veya sepet içeriği değişmeden verilen sonraki siparişler tek denemedir; aksi yeni denemedir.
> 
> SONUÇ: 4 BULGU
> ```

## 2. Bağımsız değerlendirme

| # | Karar | Gerekçe | Uygulanan düzeltme |
|---|---|---|---|
| BULGU-1 | ✅ KABUL | Haklı ve tur 5'te benim önerdiğim kuralın kusuru: cayma penceresi fiziksel üründe teslimden başlar (K-203), teslim de kargoya verme süresi ve taşımaya bağlıdır; sabit 28 gün dönemin son siparişlerinde pencere kapanmadan okuyabilir. Kural olaya bağlandı, böylece süre tahmini gerekmez. | M-4 "Ne zaman ölçülür": "dönemde ödemesi tamamlanan siparişlerin hepsinde cayma penceresi (K-203 — fiziksel üründe teslimden başlar) ve geri ödeme çatısı (K-209) kapandığında okunur (K-488)". K-488 satırı ve `02 §10.6.3` aynı biçimde düzeltildi. |
| BULGU-2 | ✅ KABUL | §2 akışın öznesini "ziyaretçi" tutup ödeme ve takibi de ona veriyordu; §3.2'de ödeme yapan kişi misafir alıcı ya da üye müşteridir (K-97, K-06). | §2: "… sepetini oluşturur; satın alan — hesabıyla alırsa üye müşteri, hesapsız alırsa misafir alıcı — kartla ya da havale/EFT ile öder ve siparişini takip eder" |
| BULGU-3 | ✅ KABUL | Hukuken doğru ve kayıtla birebir: K-204 "istisna kendiliğinden işlemez" diyor ve ayrı bir onay kutusu kuruyor; K-205 hakkı ifanın tamamlanmasına bağlıyor; K-206 istisnayı firmanın işaretine ve ön bilgilendirmede gösterilmesine bağlıyor. Deep review bulgusu YS-1 ile eklenen cümle kategorileri koşulsuz saymıştı. | §3.1: "… istisnaları da üründe kuruludur ve hiçbiri ürünün tipinden kendiliğinden doğmaz:" ardından üç istisnanın koşulu (K-204, K-205, K-206). |
| BULGU-4 | ⚠️ KISMİ | Süre ya da oturum sınırı eklenmez: "art arda" K-457'de iki durumla zaten tanımlı — başarısız ödemenin yeni siparişle tekrarı (K-181) ve aynı sepetten verilen yeni siparişin önceki ödenmemiş siparişi iptal ettirmesi (K-182). Bağ sepetin kimliğidir, zaman değil. Eksik olan tanımın `01`'e taşınmamasıydı. | M-1: "aynı sepetten art arda verilen siparişler — başarısız ödemenin yeni siparişle tekrarı ve aynı sepetten verilen yeni siparişin iptal ettirdiği önceki sipariş — tek deneme sayılır (K-181, K-182, K-416, K-457)" |

**Dağılım:** 3 KABUL · 1 KISMİ · 0 RET.

## 3. Ek bulgular

Yok.

## 4. Kullanıcı onay checklist'i

Oturum açılışındaki talimat gereği uygulandı (`01` v0.15). Yeni karar satırı açılmadı; tur 5'te öneriyle kaydedilen K-488'in okunma anı bu turun BULGU-1'iyle aynı oturumda düzeltildi ve satırına düzeltme notu düşüldü.

- [x] BULGU-1 · [x] BULGU-2 · [x] BULGU-3 · [x] BULGU-4

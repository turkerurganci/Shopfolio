# Cross-Review — 01 Project Vision (Tur 7)

**Tarih:** 2026-10-02 · **Hedef:** `Docs/01_PROJECT_VISION.md` v0.15 · **İkinci model:** `cursor-agent` (salt okuma) · **Girdi:** yalnız doküman ve boş şablonu, yeni ve izole bir çalışma alanında; model önceki turları görmedi (K-431)

## 1. Ham bulgular

İkinci modelin çıktısı, düzenlenmeden (alıntı bloğu içinde). Model son bulguyu ve sonuç satırını cevabın başında bir kez daha tekrar etti.

> ```text
> Denetimi §1–§8 ve şablon beklentilerine göre tamamladım; somut bulgular aşağıda.
> 
> BULGU-1
> Kriter: Teknik doğruluk
> Seviye: Yüksek
> Yer: §3.1 (cayma istisnaları paragrafı)
> Alıntı: "Mesafeli Sözleşmeler Yönetmeliği'nin istisnaları da üründe kuruludur ve hiçbiri ürünün tipinden kendiliğinden doğmaz: dijital üründe cayma hakkı, alıcının ayrı bir kutuyla hakkının düşeceğini kabul etmesiyle düşer (K-204); hizmette hak, firma ifayı tamamlandı işaretleyene kadar sürer (K-205); fiziksel üründe istisna, firmanın işaretleyip sebebini ön bilgilendirmede gösterdiği üründe geçerlidir (K-206)"
> Sorun: Hizmet maddesi, Yönetmelik'teki hizmet istisnasının tetikleyicisi olan "tüketicinin onayıyla ifaya başlama" anını yansıtmıyor; hakkı "ifa tamamlandı" işaretine bağlamak, yönetmelik istisnasının yanlış özeti (veya istisna ile ifa sonu kuralının birbirine karıştırılmasıdır).
> Öneri: Hizmet satırını Yönetmelik'teki ifaya başlama + bilgilendirme/onay koşullarıyla yaz; ürün bilerek daha koruyucu bir rejim seçiyorsa bunu "yasal istisnanın özeti" olarak değil, yasanın üzerinde kalan ürün politikası olarak ayır.
> 
> BULGU-2
> Kriter: Edge case
> Seviye: Orta
> Yer: §6, M-4 (Ne zaman ölçülür)
> Alıntı: "İlk gerçek kurulumun ilk üç ayı; dönemde ödemesi tamamlanan siparişlerin hepsinde cayma penceresi (K-203 — fiziksel üründe teslimden başlar) ve geri ödeme çatısı (K-209) kapandığında okunur (K-488)"
> Sorun: Fiziksel sipariş teslim edilmezse cayma penceresi başlamaz; hizmette hak "tamamlandı"ya kadar sürüyorsa (§3.1) tamamlanmayan siparişte pencere kapanmaz. Bu siparişler dönem içinde kalırsa M-4'ün okunma koşulu sonsuza kadar sağlanmayabilir; iptal edilip iade edilen ama hiç teslim edilmeyen siparişte "cayma penceresi kapandı"nın sayılıp sayılmadığı da tanımsızdır.
> Öneri: Okuma kuralına teslim edilmeyen / ifası bitmeyen / iptal-iade ile kapanan siparişler için açık kapanış halleri ekle (ör. iptal+geri ödeme çatısının kapanması; N iş günü teslim edilmezse paydayan çıkar veya "uyulmadı/kapandı" say).
> 
> BULGU-3
> Kriter: Tutarlılık
> Seviye: Orta
> Yer: §6 giriş paragrafı ile Ü-3 satırı
> Alıntı: "Dört kriterin dördü de `12`'de sınanır ve MVP'nin kabul koşuludur" / "K-401'in envanteriyle sayılır: kart + fiziksel sipariş 2 · havale + fiziksel 3 · hizmet 1 · dijital 0; ... Faturayı kesmek sipariş başına bir sistem dışı adımdır"
> Sorun: Ü-1, Ü-2 ve Ü-4 ölçümü açıkça `12` senaryosuna/kanıtına bağlanırken Ü-3 yalnızca K-401 envanteriyle sayılır ve `12`'de bir senaryo olduğunu söylemez; üstteki "dördü de 12'de sınanır" iddiasıyla çelişir. Ü-4'teki tek gerçek kartlı sipariş de havale+fiziksel gibi envanterin diğer hatlarını kapsamaz.
> Öneri: Ya Ü-3 için `12`'de hat tipine göre adım sayımı senaryoları yaz (en azından en ağır hat + bir karışık örnek), ya da giriş cümlesini "Ü-1/2/4 `12`'de; Ü-3 K-401 envanteri ile kabulde doğrulanır" diye daralt.
> 
> BULGU-4
> Kriter: Kullanıcı deneyimi
> Seviye: Düşük
> Yer: §6, Ü-1 (satış kapısı koşulları)
> Alıntı: "satış kapısının koşulları: firma kimliği (K-14), en az bir açık ödeme yöntemi (K-163), yasal metinler (K-365) ve iade adresi (K-465)"
> Sorun: İade adresi, katalogda yalnız dijital ürün ve/veya hizmet varken de satış kapısı için zorunlu görünür; fiziksel iade adresi olmayan firmaya anlamsız bir kurulum adımı dayatır ve kapının neden açılmadığı kafa karıştırır.
> Öneri: İade adresini fiziksel ürün (veya fiziksel hat içeren katalog) yayındayken zorunlu kıl; yalnızca dijital/hizmet katalogunda satış kapısı koşulundan çıkar veya "isteğe bağlı" diye yaz.
> 
> SONUÇ: 4 BULGU
> ```

## 2. Bağımsız değerlendirme

| # | Karar | Gerekçe | Uygulanan düzeltme |
|---|---|---|---|
| BULGU-1 | ⚠️ KISMİ | Haklı bir hukuki ayrım: Yönetmelik'in hizmet istisnası, cayma süresi bitmeden tüketicinin onayıyla ifaya başlanmasına bağlıdır. K-205 hakkı ifa tamamlanana kadar sürdürür; bu, yasanın üzerinde ve tüketicinin lehine bir ürün politikasıdır. Tur 6'da `01`'e yazılan cümle bunu "Yönetmelik istisnası" başlığı altında anlatıyordu. Çözüm, kural ayrıntısını `01`'den çıkarıp evine (`02 §7.3`) işaret etmek; `02`'deki anlatım `02`'nin döngüsüne devredildi (aşağıda). | §3.1: "… cayma hakkının dijital ürün, hizmet ve istisna işaretli fiziksel ürün için sınırları da üründe kuruludur. Hiçbiri ürünün tipinden kendiliğinden doğmaz; her birinin koşulu `02 §7.3`'tedir." |
| BULGU-2 | ✅ KABUL | Tur 6'da olaya bağlanan okunma kuralı, teslim edilmeyen ya da ifası tamamlanmayan siparişte hiç sağlanmayabiliyordu. Kural tamamlandı: yalnız teslim edilen ya da ifası tamamlanan siparişler beklenir; diğerleri o ana kadarki iptal ve iadesiyle sayılır. Kuralın evi `02 §10.6.3`'tür. | `02 §10.6.3` ve K-488: "… teslim edilen ya da ifası tamamlanan siparişlerin hepsinde cayma penceresi ve geri ödeme çatısı kapandığı gün okunur; o güne kadar teslim edilmemiş ya da ifası tamamlanmamış sipariş beklenmez …". `01` M-4: "okunma anının kuralı `02 §10.6.3`'tedir (K-488)". |
| BULGU-3 | ✅ KABUL | §6 girişi dört ürün düzeyi kriterinin dördünün de `12`'de sınandığını söylüyor; Ü-3'ün satırı bunu göstermiyordu. Dayanak hazır: `10` KP-51 bir kapsam satırıdır ve K-24 gereği `12`'de senaryoya döner. | Ü-3 "Nasıl ölçülür": "`12`'de her hat ve bir karışık sipariş için ayrı senaryoyla, K-401'in envanteriyle sayılır" |
| BULGU-4 | ❌ RET | İade adresi K-465 ile satış kapısının koşuludur ve bir ürün kararıdır; `01` yalnız onu yansıtıyor. Öneri ayrıca hatalı bir öncüle dayanıyor: hizmette de cayma hakkı vardır (K-205 — ifa tamamlanana kadar) ve üretilen ön bilgilendirme formu ile mesafeli satış sözleşmesi iade adresini her siparişte taşır (K-465; `10` ÖK-11). Yalnız dijital ürün satan bir katalog teorik olarak mümkündür, ama koşulu kaldırmak bir varlık kararıdır ve `01`'in cross-review'ının konusu değildir. | — |

**Dağılım:** 2 KABUL · 1 KISMİ · 1 RET. Bu turun gözlemi: tur 5–7'nin bulgularının çoğu, `01`'e taşınan kural ayrıntısından (cayma istisnalarının koşulları, M-4'ün okunma kuralı) doğdu. Vizyon dokümanı kuralın kendisini değil yerini söylemeli — ayrıntı `02`'ye bırakıldı ve `01` oraya işaret ediyor.

## 3. Ek bulgular

Yok.

## 4. Kullanıcı onay checklist'i

Oturum açılışındaki talimat gereği uygulandı (`01` v0.16). Yeni karar satırı açılmadı; K-488'in okunma kuralı teslim edilmeyen siparişi kapsayacak biçimde düzeltildi.

- [x] BULGU-1 · [x] BULGU-2 · [x] BULGU-3 · [x] BULGU-4

**`02`'nin kalite döngüsüne devir:** `02 §7.3.3`, hizmette hakkın "tamamlandı" işaretine kadar sürmesini Yönetmelik'in istisnasının işleyişi gibi anlatıyor. Yönetmelik'te istisna, cayma süresi bitmeden tüketicinin onayıyla ifaya başlanmasına bağlıdır. K-205'in seçtiği an bundan daha geç olduğu için tüketicinin lehinedir ve yasaya aykırı değildir; ama metin bunu bir ürün politikası olarak söylemelidir. `02`'nin audit'inde ele alınır.

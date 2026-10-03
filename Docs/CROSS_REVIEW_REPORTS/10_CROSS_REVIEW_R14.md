# Cross-Review — 10 MVP Scope (Tur 14)

**Tarih:** 2026-10-03 · **Hedef:** `Docs/10_MVP_SCOPE.md` v0.24 · **İkinci model:** Codex CLI — ChatGPT hesabı, `gpt-5.6-terra`, düşünme düzeyi yüksek (salt okuma) · **Girdi:** yalnız doküman ve boş şablonu (`e74883f`), yeni ve izole bir çalışma alanında; karar kaydı, önceki turlar ve raporlar verilmedi (K-431)

## 1. Ham bulgular

İkinci modelin çıktısı, düzenlenmeden (alıntı bloğu içinde; yalnız satır sonu boşlukları atıldı). İstem önceki turlarla aynıdır: daraltılmış talimat (K-615, K-644), yedi kriter, bulgu biçimi ve "bilinçli karara yalnız katılmadığın için itiraz etme" cümlesi.

> ```text
> BULGU-1
> Kriter: Teknik doğruluk
> Seviye: Yüksek
> Yer: KP-22
> Alıntı: “firma iade taşıyıcısı belirlemez, her iade öngörülenin dışında bir taşıyıcıyla yapılır”
> Sorun: Taşıyıcı belirtilmemiş iadelerde geri ödeme süresini malın firmaya ulaşmasına bağlamak, müşterinin kargoya teslim tarihinden başlayan kanuni süreyi sistematik olarak geciktirebilir. Bu, müşterinin bedeline geç ulaşmasına ve firmanın iade süresi ihlaline yol açar. [Ticaret Bakanlığı rehberi](https://tuketici.ticaret.gov.tr/yayinlar/tuketici-bilgi-rehberi/mesafeli-sozlesmeler-hakkinda-bilgilendirme)
> Öneri: Taşıyıcı belirtilmiyorsa fiziksel mal iadelerinde geri ödeme sayacını müşterinin taşıyıcıya teslim tarihinden başlatın; ulaşıma bağlama kuralını yalnız ön bilgilendirmede belirlenmiş taşıyıcı dışındaki iadeler için uygulayın.
>
> BULGU-2
> Kriter: Tutarlılık
> Seviye: Yüksek
> Yer: KP-63; A-17 kapanış notu
> Alıntı: “IBAN, iade adresi ve teslimat illeri dahil, eski ve yeni değerle — … iz on yıllık saklama süresi boyunca değiştirilemez ve silinemez”
> Sorun: IBAN’ın geri ödeme tamamlanınca veya belirlenen diğer anlarda silineceği yazılırken, aynı IBAN’ın işlem izinde eski-yeni değer olarak on yıl silinemez tutulması öngörülüyor. Bu iki kural birlikte uygulanamaz; IBAN ya silinmez ya da işlem izi değiştirilemezlik şartını kaybeder.
> Öneri: İşlem izinde tam IBAN yerine maskeli değer veya geri döndürülemez doğrulama bilgisi tutun; tam IBAN’ı işlem izinden de IBAN imha kuralıyla eşzamanlı kaldırın.
> ```

Model sonuca varmadan önce iki web araması yaptı (koşum kaydında görünür, çıktıya girmedi): Mesafeli Sözleşmeler Yönetmeliği m.12'nin 1/1/2026'dan beri uygulanan metni ve Yönetmelik'in 2026'daki değişiklikleri. İki bulgu yazdı: biri önceki turlarda gelip reddedilen konudur, öteki bu döngüde ilk kez geliyor.

## 2. Bağımsız değerlendirme

| # | Karar | Gerekçe | Uygulanan düzeltme |
|---|---|---|---|
| BULGU-1 | ❌ RET | **Konu sekizinci kez geliyor ve doküman doğru** (5., 6., 8., 9., 10., 11., 13. ve bu tur). Bulgu 13. turun BULGU-1'iyle aynı alıntıyı, aynı öneriyi ve aynı kaynağı taşıyor. **Gösterilen rehber bu kuralı koymaz:** rehber bu turda yeniden açıldı. Genel kural *"Bu süre, tüketicinin malı kargoya teslim ettiği tarihten itibaren başlar"*, istisna *"İade işlemi, ön bilgilendirmede belirtilenden farklı bir kargo ile yapılmışsa bu süre, ürünün satıcıya ulaştığı tarihte başlar"* diye yazılıdır; taşıyıcı hiç belirtilmeyen hâlde sürenin başlangıcına dair bir cümle yoktur. Yönetmelik de bu hâlin süresini düzenlemez: m.12/1 iki hâli düzenler, m.12/5 taşıyıcı belirtilmeyen hâlde yalnız masrafı düzenler (11. turda RG 32909'un resmî metninden okundu). Bu turda yapılan aramada 24/5/2025 tarihli değişiklikten sonra çıkmış yeni bir değişiklik bulunmadı. **İddia KP-22'de kalan risk olarak yazılı:** satırın gerekçesi taşıyıcı belirtilmediğinde bu sonucun açık hükmü olmadığını, tüketici lehine bir yorumun başlangıcı kargoya veriliş gününe çekebileceğini ve firmanın iki okumada da süreyi nasıl kaçırmayacağını yazar. Model bu cümleyi yine alıntılamadı. **Öneri elenmiş seçenektir:** taşıyıcı belirlemek K-293 ve K-491'de elendi; sistem kargo entegrasyonu olmadan kargoya veriliş tarihini öğrenemez (K-132). Dönüşün sebebi metindeki bir belirsizlik değildir; 13. turun gerekçesi geçerlidir. | Uygulanmadı. |
| BULGU-2 | ⚠️ KISMİ | **Çelişki yoktur, ama satırın sözü yanlış okunmaya açıktı.** KP-63'ün işlem izine eski ve yeni değeriyle yazdığı IBAN, **firmanın panelde girdiği kendi havale IBAN'ıdır** (KP-59; K-523). Müşterinin geri ödeme IBAN'ı bir panel ayarı değildir: müşteri onu sipariş sayfasından girer, *"firma IBAN'ı panelden girmez"* (KP-47, K-498). İşlem izinin sipariş satırları müşterinin kişisel verisini değer olarak taşımaz; IBAN isteğinin satırı yalnız adımı yapan yöneticiyi ve isteğin tarihini taşır (`02 §10.3.1`). Bu yüzden müşterinin IBAN'ının silinmesi (K-356, K-497) ile izin on yıl değiştirilemez olması (K-310, K-355) aynı veriye dokunmaz. **Yine de bulgu bir boşluğu gösteriyor:** `02 §10.3.1` IBAN'ı *"ödeme yöntemlerinin açılıp kapanması ve IBAN"* diye havale ayarının yanında anar; KP-63 ise yalnız "IBAN" der ve satırda müşterinin IBAN'ını dışarıda bırakan bir söz yoktur. Bağımsız bir okuyucu bunu müşterinin IBAN'ı sanabilir. **Düzeltme:** KP-63 "havale IBAN'ı" der; bu, KP-67'nin ve `02 §9.3.3`'ün kullandığı addır. **Öneri uygulanmadı:** firmanın IBAN'ını maskelemek, ele geçirilmiş bir hesapla yapılan IBAN değişikliğinin izini okunamaz kılar (K-616'nın gerekçesi). Müşterinin IBAN'ı ize hiç yazılmadığı için onu izden kaldırmaya gerek yoktur. | `10` KP-63: "IBAN" → "havale IBAN'ı". Kural değişmedi; yeni K açılmadı. |

**Dağılım:** 0 KABUL · 1 KISMİ · 1 RET. BULGU-1 kayıtlı bir kararı (K-491) yeni bir resmî dayanak olmadan sekizinci kez yeniden açıyor. BULGU-2 iddia ettiği çelişkiyi göstermiyor; ama bu döngüde ilk kez gelen bir konu ve modelin yanlış okuyabildiği bir sözü gösterdi, söz netleştirildi. Kural değişmedi.

## 3. Ek bulgular

- **Mekanik tarama:** sürüm başlığı, sürüm notu ve alt bilgi aynı sürümü gösteriyor (v0.25). Kapsam satırı sayısı değişmedi (77), kapsam dışı satırı sayısı değişmedi (63).
- **Aynı sözün diğer evleri:** `02 §10.3.1` IBAN'ı havale ayarının yanında anar ve sipariş satırlarının müşterinin kişisel verisini değer olarak taşımadığını yazar; düzeltme gerektirmez. `01`'de işlem izini IBAN'la birlikte anan bir cümle yoktur. Etki yansıtmaya devreden iş yok.
- **Tekrarlayan konular:** KP-22'nin geri ödeme başlangıcı sekiz turda (5., 6., 8., 9., 10., 11., 13., 14.) geldi. Bu tur KP-39 ve KD-28 gelmedi. Ciddi sorun sınıfında son altı turda (9.–14.) kabul edilen bir kural değişikliği yok; bu turdaki tek düzeltme bir sözü netleştiriyor.
- **Ciddi bir sorun görülmedi.** Bu turda ikinci modelin listesinin dışında ciddi bir sorun saptanmadı.
- **Önceki turlardan devreden notlar** yerinde duruyor: "İşlem izi" sözlük satırı, §8.5.2, §3.34.8 ve §12.1.12'ye 4733 m.8/5-k dayanağı (isteğe bağlı); `02 §3.1.4`'teki "kayıt yükümlülüğü her firmada doğmaz" gerekçesi (8. tur, etki yansıtmaya devredildi).

## 4. Kullanıcı onay checklist'i

Proje sahibinin talimatıyla ("kolay soruları bana sorma sen karar ver, çok çok kritik konuları sadece sor") yönetici onayladı ve uygulandı (`10` v0.25). Bu turda yeni karar satırı açılmadı: KP-63'ün düzeltmesi bir karar değil, satırın sözünü `02 §10.3.1`'e ve K-523'e hizalayan bir netleştirmedir. K-356, K-491, K-497 ve K-523 kayıtlı kararlardır; bu tur hiçbirini değiştirmedi.

- [x] BULGU-1 (ret: sekizinci kez gelen konu; Yönetmelik de Bakanlık rehberi de taşıyıcı belirtilmeyen hâlde sürenin başlangıcını düzenlemez, iddia KP-22'de ve K-491'de kalan risk olarak yazılı; taşıyıcı belirleme K-293 ve K-491'de elendi. Doküman değişmedi)
- [x] BULGU-2 (kısmi: çelişki yok — izdeki IBAN firmanın havale IBAN'ıdır, müşterinin IBAN'ı panelden girilmez ve ize değer olarak yazılmaz; KP-63 "havale IBAN'ı" der. Maskeleme önerisi uygulanmadı)

**Hukuki kontrol** (2026-10-03):
- **Ticaret Bakanlığı tüketici rehberi — mesafeli sözleşmeler** (bu turda yeniden açıldı): *"Bu süre, tüketicinin malı kargoya teslim ettiği tarihten itibaren başlar."* · *"İade işlemi, ön bilgilendirmede belirtilenden farklı bir kargo ile yapılmışsa bu süre, ürünün satıcıya ulaştığı tarihte başlar."* Taşıyıcı hiç belirtilmeyen hâlde sürenin başlangıcını düzenleyen bir cümle yoktur.
- **Mesafeli Sözleşmeler Yönetmeliği m.12** (11. turda RG 24/5/2025-32909'un resmî metni ve konsolide metin okundu): m.12/1 belirtilen taşıyıcıya teslimi ve öngörülenin dışındaki taşıyıcıyı düzenler; m.12/5 taşıyıcı belirtilmeyen hâlde yalnız masrafı düzenler. **Sonraki değişiklik taraması** bu turda yinelendi: 24/5/2025 tarihli değişiklikten (yürürlük 1/1/2026) sonra Yönetmelik'i değiştiren yeni bir Resmî Gazete metni bulunmadı.
- BULGU-2 hukuki bir iddia taşımıyor. KVKK açısından ayrı bir değerlendirme gerekmedi, çünkü müşterinin IBAN'ı işlem izine hiç girmiyor.

**Kaynaklar (hukuki kontrol):**
- [Ticaret Bakanlığı — Mesafeli Sözleşmeler Hakkında Bilgilendirme](https://tuketici.ticaret.gov.tr/yayinlar/tuketici-bilgi-rehberi/mesafeli-sozlesmeler-hakkinda-bilgilendirme)
- [Resmî Gazete 24/5/2025-32909 — Mesafeli Sözleşmeler Yönetmeliğinde Değişiklik Yapılmasına Dair Yönetmelik](https://www.resmigazete.gov.tr/eskiler/2025/05/20250524-2.htm)
- [Mesafeli Sözleşmeler Yönetmeliği — mevzuat.gov.tr konsolide metin (PDF)](https://mevzuat.gov.tr/File/GeneratePdf?mevzuatNo=20237&mevzuatTur=KurumVeKurulusYonetmeligi&mevzuatTertip=5)

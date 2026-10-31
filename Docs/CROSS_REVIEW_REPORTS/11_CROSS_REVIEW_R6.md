# Cross-Review — 11 Implementation Planı (Tur 6)

**Tarih:** 2026-10-31 · **Hedef:** `Docs/11_IMPLEMENTATION_PLAN.md` v0.43 (girdi commit'i `0176019`, 759 712 bayt girdi, 7 760 satır) · **İkinci model:** Codex CLI — ChatGPT hesabı, `gpt-5.6-terra`, düşünme düzeyi yüksek (salt okuma); betiğin `status.txt` satırı: `model=codex gpt-5.6-terra high` · **Ciddiyet ölçüsü:** 1. turdan itibaren, mekanizma okuma paragrafıyla (K-1399; `11_CROSS_REVIEW.md` §1) · **Koşum:** iki koşum — tek parça çağrı ve beş parçalı kontrol koşumu · **Sonuç:** **TEMİZ DEĞİL** — 11 ham bulgu, 9 ayrı bulgu; 0 doğru · 1 kısmen · 8 çürük (altısı önceki turlarda reddedilmiş iddianın dönüşü); 8 şartname çifti uygulandı (T04 altı, T06 iki satır; v0.43 → v0.44); kabullerden biri Yüksek (task sırası ↔ kabul kriteri), Faz 4 madde 5 çıkış kuralı uygulanamaz; tur 7 gerekir.

## 1. Koşum

Betikler, istem, parça notu, şablon ve **parça sınırları 1. turdakinin aynısıdır** (`11_CROSS_REVIEW.md` §1; K-1478 — sınır başlıkla tanımlıdır, değişmez). İstem tek karakter değiştirilmeden kullanıldı. Girdi `build_inputs.py` ile kuruldu (`uncovered lines: none`). **İkinci model çağrısı bu turdan itibaren tek betikle yapıldı** (K-1484): `bash scripts/doc-scans/cross_review_ikinci_model.sh in out c1 p1 p2 p3 p4 p5`; betik asıl modeli (Codex `gpt-5.6-terra`, yüksek) sağlık çağrısıyla seçti (`used: codex gpt-5.6-terra`), altı çağrının hepsi aynı modelle, tek komutta, ilk denemede `rc=0` döndü (çıkış kodu 0; çıktı dosyaları ayrıca okundu). Yedek model (Cursor) devreye girmedi, tur bölünmedi, kullanım sınırı dolmadı. Çalışma dizini: `C:\Users\teknodeveloper\.claude\helper-state\shopfolio\kd-cross\r6\` (girdiler `in/`, ham çıktılar `out/`, doğrulayıcı paketleri `dogrulayici_<k>.md`, hükümler `hukum_D<k>.md`, talimat `talimat_dogrulayici.md`; paket betiği `betik/paket6.py`).

| Çağrı | Bölümler | Girdi | Token | Süre | Sonuç |
|---|---|---|---|---|---|
| Tek parça | tüm doküman | 759 712 bayt | 390 609 | 153 sn | 2 bulgu |
| Parça 1 | §1–§4 · §5 Task yapısı · F0 · F1 (satır 65–866) | 96 393 bayt | 51 963 | 156 sn | 3 bulgu |
| Parça 2 | F2 · F3 T301–T345 (867–2841) | 197 089 bayt | 100 718 | 69 sn | 1 bulgu |
| Parça 3 | F3 T346–T390 · F4 (2842–4219) | 150 962 bayt | 100 686 | 373 sn | 3 bulgu |
| Parça 4 | F5 · F6 · §6 (4220–5633) | 149 650 bayt | 58 738 | 111 sn | 1 bulgu |
| Parça 5 | §7 · §8 · §9 (5634–dosya sonu) | 254 332 bayt | 121 306 | 96 sn | 1 bulgu |

Hiçbir çağrı TEMİZ dönmedi. İki koşum bu turda da sürdü; iki koşum arasındaki örtüşme: tek parça B-1 = parça 1 B-1 (geri alma/`Down`), tek parça B-2 = parça 2 B-1 (ETBİS).

## 2. Ham bulgular

İkinci modelin çıktıları, satır sonu boşlukları dışında düzenlenmeden (alıntı blokları içinde).

**(a) Tek parça çağrı:**

> BULGU-1
> Kriter: Tutarlılık
> Seviye: Orta
> Yer: §3 F1; T103; T602
> Alıntı: “göçler ileri, idempotent ve geri alınabilir” / “Göç tek konudur; `Down` ve işlem dışı adım yoktur”
> Sorun: F1 çıkışı göçlerin geri alınabilir olmasını şart koşarken task’lar `Down` işlemini yasaklıyor; ayrıca T602 şemanın geri alınmayacağını belirliyor. Faz kapısının talep ettiği güvence, planın seçtiği mekanizmayla sağlanamıyor. Bu bir iç çelişkidir (4).
> Öneri: F1 çıkışı ve gate metnini “ileri yönlü, tekrar uygulanabilir ve yedekten kurtarma ile doğrulanmış” olarak değiştirin; geri alma yalnız önceki imaja dönüş ve kurtarma anlamındaysa bunu açıkça tanımlayın.
>
> BULGU-2
> Kriter: Eksiklik
> Seviye: Yüksek
> Yer: T330, kabul kriteri 7 ve KK7
> Alıntı: “ETBİS kaydı yapılmaz; kayıt bilgisi isteğe bağlı alandır, satış kapısını tutmaz”
> Sorun: Kendi e-ticaret ortamında tüketiciye satış yapan hizmet sağlayıcının faaliyete başlamadan önce ETBİS’e kayıt olması gerekir. Plan, kaydı bilinçli olarak yapmıyor ve satışın açılmasını buna bağlamıyor; bu, yürürlükteki yükümlülüğe aykırı biçimde faaliyete geçmeye yol açar (1). [Ticaret Bakanlığı ETBİS SSS](https://ticaret.gov.tr/ic-ticaret/sikca-sorulan-sorular/elektronik-ticaret)
> Öneri: ETBİS kaydını F0/F6 dış ön koşulu ve satış açılış kapısı yapın; kayıt kanıtının `DEPLOY_RUNBOOK` içinde doğrulanmasını, T330/T331 kabul kriterlerinde zorunlu tutun.
>
> SONUÇ: 2 BULGU

**(b) Parça 1 (§1–§4 · §5 Task yapısı · F0 · F1):**

> BULGU-1
> Kriter: Tutarlılık
> Seviye: Yüksek
> Yer: §3 F1; T103; T04 KK5
> Alıntı: "Göçler ileri, idempotent ve geri alınabilir" / "Göç tek konudur; `Down` ve işlem dışı adım yoktur"
> Sorun: F1’in çıkış koşulu ve CI’daki geri alma sınaması, göçlerin geri alınabilmesini zorunlu tutuyor; ilk gerçek göç dahil task’lar ise `Down` yöntemini yasaklıyor. EF Core’da geri alma için ilgili migration’ın `Down` işlemi gerekir. Bu, faz çıkış kriteri ile task kabul kriterinin doğrudan çelişmesidir (4).
> Öneri: Ya F1/T04’teki “geri alınabilir / geri alma sınaması” güvencesini, açıkça tanımlanmış ileri-yönlü düzeltme stratejisiyle değiştirin; ya da migration task’larındaki `Down` yasağını kaldırıp geri alma davranışını test edin.
>
> BULGU-2
> Kriter: Tutarlılık
> Seviye: Yüksek
> Yer: T04, T06; F0 yürütme sırası
> Alıntı: "`publish.yml` ana dala her merge'de imajı commit kimliğiyle, sürüm etiketinde sürüm numarasıyla yayımlar"
> Sorun: T04 yalnız T03’e bağlıdır ve T06’dan önce yürütülür. Ancak T04’ün her ana dal merge’inde üç imajı yayımlama kabul kriteri, T06’nın üreteceği Dockerfile ve imaj tanımlarına dayanır. T04 merge edilince yayın akışı henüz var olmayan imaj girdileri nedeniyle başarısız olur; T04, kendisinden sonraki T06 çıktısına dayanır (4).
> Öneri: T06’yı T04’ten önceye alın ve T04’ü T06’ya bağımlı yapın; alternatif olarak T04’te yayın işini imajlar hazır olana dek açıkça devre dışı bırakıp, “her merge’de yayımlar” kriterini T06 sonrası etkinleştirin.
>
> BULGU-3
> Kriter: Tutarlılık
> Seviye: Orta
> Yer: T117, kabul kriteri 3 ve KK3
> Alıntı: "`Undelivered`'da yalnız gönderildikten sonra geri dönen satırda doludur" / "gönderim anı dolu da boş da `Undelivered` satır kabul edilir"
> Sorun: Kabul kriteri, teslim edilemeyen kaydın ancak gönderimden sonra oluşabileceğini belirtirken doğrulama listesi gönderilmemiş kaydı da kabul ediyor. Validator, kabul kriterine aykırı durumdaki gönderim kayıtlarını yeşil sayabilir; bu doğrudan kriter–test çelişkisidir (4).
> Öneri: Tek davranışı seçin. `Undelivered` yalnız gönderim sonrası oluşacaksa `SentAt` zorunlu olmalı ve KK3 boş `SentAt` değerini reddetmelidir.
>
> SONUÇ: 3 BULGU

**(c) Parça 2 (F2 · F3 T301–T345):**

> BULGU-1
> Kriter: Eksiklik
> Seviye: Yüksek
> Yer: T330, kabul kriteri 7
> Alıntı: "ETBİS kaydı yapılmaz; kayıt bilgisi isteğe bağlı alandır, satış kapısını tutmaz"
> Sorun: Kendi elektronik ticaret ortamında tüketiciye satış yapan hizmet sağlayıcının faaliyete başlamadan ETBİS’e kayıt olması gerekir. Plan kayıt yapılmamasını ve satış kapısından bağımsız olmasını emrederek mevzuata aykırı faaliyete ve idari para cezası riskine yol açar. [Ticaret Bakanlığı](https://ticaret.gov.tr/ic-ticaret/sikca-sorulan-sorular/elektronik-ticaret)
> Öneri: ETBİS kaydını yayına/satışa açılış ön koşulu yapın; kayıt ve gerekli bildirimlerin tamamlandığına dair kanıtı faz geçiş kapısına ekleyin. Uygulama içinde yalnız kayıt bilgisi gösterilecekse bunu dış yükümlülüğün yerine geçmeyen salt görünüm olarak tanımlayın.
>
> SONUÇ: 1 BULGU

**(d) Parça 3 (F3 T346–T390 · F4):**

> BULGU-1
> Kriter: Tutarlılık
> Seviye: Orta
> Yer: T378, Kabul kriterleri 2–3
> Alıntı: "İş günü pazartesi–cumadır, resmî tatiller hariç" / "listenin kapsamadığı yılda yalnız hafta sonu dışlanır"
> Sorun: Resmî tatilin hafta içine geldiği ancak listenin kapsamadığı yılda aynı gün hem iş günü dışı hem iş günü sayılır. Bu, havale ve kargoya verme sürelerini çelişkili hesaplatır; müşteri teslimat ve ödeme süreleri bakımından hak kaybına uğrayabilir. Dört sınıftan (4).
> Öneri: Resmî tatil verisini yürütme ufkunun tamamı için resmî kaynaktan paketleyin/güncelleyin; kapsanmayan yıl için hafta sonu dışındaki günleri otomatik iş günü sayan geri dönüşü kaldırın.
>
> BULGU-2
> Kriter: Eksiklik
> Seviye: Yüksek
> Yer: T368, Kabul kriteri 6; T370
> Alıntı: "Panel fesih satırında ve taahhüt süresi sonrası iptalde kanuni faiz uyarısı gösterir; sistem faiz hesaplamaz."
> Sorun: Teslim veya ifanın gerçekleşmemesi nedeniyle fesihte tahsil edilen bedellerin kanuni faiziyle iadesi gerekir. Salt uyarı, faizin hesaplanıp geri ödemeye eklenmesini güvenceye bağlamaz; T370’de de bu tutarın oluşturulmasına ilişkin kabul kriteri yoktur. Bu, tüketicinin alacağının eksik ödenmesine yol açar; yürürlükteki resmî açıklama da bu durumda tüm ödemelerin yasal faiziyle iadesini belirtir ([Ticaret Bakanlığı](https://tuketici.ticaret.gov.tr/yayinlar/tuketici-bilgi-rehberi/mesafeli-sozlesmeler-hakkinda-bilgilendirme)). Dört sınıftan (1).
> Öneri: T368/T370’ye fesih tarihinden ödeme anına kadar kanuni faizi hesaplayan, geri ödeme tutarına ekleyen ve testle doğrulanan bir akış ekleyin.
>
> BULGU-3
> Kriter: Teknik doğruluk
> Seviye: Orta
> Yer: Başlık notu “Okuma tablosu ve boyut”; T348
> Alıntı: "task bloğu en çok 2,5 KB"
> Sorun: T348 bloğu, uzun kaynak listesi, sekiz kabul kriteri ve doğrulama listesiyle bu üst sınırı aşmaktadır. Buna rağmen doküman "`11_task_blok_denetle.py --tam` sorunsuz koşar" demektedir. Bu, ajanların hedef task’ı sınırlandırılmış okuma kuralıyla eksiksiz okuyabilmesi güvencesiyle çelişir. Dört sınıftan (4).
> Öneri: T348’i bağımsız ve izlenebilir alt task’lara bölün ya da tekrarları kaynak kimliklerine taşıyarak bloğu 2,5 KB altına indirin; denetim çıktısının gerçek byte ölçümünü gate’e bağlayın.
>
> SONUÇ: 3 BULGU

**(e) Parça 4 (F5 · F6 · §6):**

> BULGU-1
> Kriter: Tutarlılık
> Seviye: Orta
> Yer: Başlık notu “Okuma tablosu ve boyut”; T525
> Alıntı: "**Boyut hedefi:** bütün dosya 1,5 MB'ın altında; task bloğu en çok 2,5 KB"
> Sorun: T525 bloğu, ayrıntılı kabul kriterleri ve kontrol listesiyle tek başına 2,5 KB sınırını aşar. Bu, task hedefinin sınırlı okunabilmesini sağlayan dokümanın kendi yazım ve denetim güvencesiyle çelişir (sınıf 4).
> Öneri: T525’i bağımsız, kaynakları ve testleri açık alt task’lara bölün; ya da kabul edilen blok boyutu sınırını ve denetim betiğinin kuralını aynı PR’da gerçekçi biçimde güncelleyin.
>
> SONUÇ: 1 BULGU

**(f) Parça 5 (§7 · §8 · §9):**

> BULGU-1
> Kriter: Tutarlılık
> Seviye: Yüksek
> Yer: §7.1 Aday task tablosu T548; §7.2 T548
> Alıntı: “`09 §16.2`; `07 §4.31.2`; K-1354”
> Sorun: T548’in aday tablosu `07 §4.31.2`yi kaynak birimi olarak gösterirken ileri izlenebilirlikte bu kaynak için satır, geri izlenebilirlikte de T548 için bu kaynak bulunmuyor. Bu, dokümanın “7.1 ile 7.2 arasında … yalnız bir yönde olan çift yok” iddiasıyla çelişir ve yükleme akışının API kaynağı üzerinden izlenmesini ortadan kaldırır. (4)
> Öneri: `07 §4.31.2 → T548` bağını §7.1 API tablosuna ekleyin; T548’in §7.2 satırına aynı kaynağı ekleyin; envanter/kapsama sayımlarını ve çift yön eşitlik çıktısını buna göre güncelleyin.
>
> SONUÇ: 1 BULGU

## 3. Bağımsız değerlendirme

Ham bulgular numaralandı: tek parça B-1…B-2, parça 1 B-1…B-3, parça 2 B-1, parça 3 B-1…B-3, parça 4 B-1, parça 5 B-1 (11 ham). İki çift aynı iddiadır (tek parça B-1 = parça 1 B-1; tek parça B-2 = parça 2 B-1) → 9 ayrı bulgu (F1–F9; sıra: doğrulayıcı sırası). Sekiz `[dogrulama]` Opus / high doğrulayıcıya verildi: Yüksek bulgular tek tek (D1: F1, D2: F2, D3: F3, D4: F4, D5: F5), Orta bulgular tek ya da aynı bölümden gruplu (D6: F6 + F7, blok bayt sınırı; D7: F8; D8: F9); toplam 8 doğrulayıcı (bekçi sınırı 20; bekçi olayı yok). Önceki turlarda reddedilen iddianın olabileceği bulgularda (F1, F2, F4, F5, F6, F7, F8) paketle birlikte önceki hükmün tam dosyası verildi ve talimata "yeni bilgi yoksa kısa gerekçeyle tekrar eden RET" kuralı yazıldı. Hükümler `11_audit_hukum_denetle.py` ile denetlendi: sorun 0.

| # | Bulgu (kaynak) | Karar | Gerekçe | Uygulama |
|---|---|---|---|---|
| F1 | F1 çıkışı ve T04 KK5 "geri alınabilir" der, T103 `Down` yöntemini yasaklar, T602 şemanın geri alınmayacağını söyler (tek parça B-1, parça 1 B-1; T103, T04, T602) | ❌ RET (tekrar eden: 2. tur F1, 3. tur F1, 4. tur F1; Yüksek/Orta → Düşük) | Yeni bilgi yok; `05` ve `09` son değişikliğinden (#266) beri aynı. Projede "geri alma" şemayı `Down` ile geri almak değil önceki imaja dönmektir ve CI göç adımında sınanır (son yayımlanan sürümün imajı yeni şemaya karşı): `05` §8.7.1, §8.7.3, MK-74; `09` §10.6 madde 1 ("`Down` yöntemi silinir — şema geri alınmaz"), §21.4 madde 3. Parça 1'in EF Core gerekçesi ancak geri alma şemayı geri almak olsaydı geçerli olurdu. | Yok |
| F2 | T330 k7 "ETBİS kaydı yapılmaz"; kayıt satış kapısını tutmaz (tek parça B-2, parça 2 B-1; T330) | ❌ RET (tekrar eden: 2. tur F6, 3. tur F3, 4. tur F1; Yüksek → Düşük) | Yeni bilgi yok; gösterilen kaynak yine Ticaret Bakanlığı SSS sayfası. "Kayıt yapılmaz" ürünün kayıt yapmadığını söyler; kayıt firmanın yükümlülüğüdür ve alan satış kapısını tutmaz, çünkü ürün kaydın varlığını denetleyemez (`02` §3.1.4, §12.1.4; K-574, K-628; `10` ÖK-5). T330 k7 kaynak cümleyle birebir; firmanın ön koşulu T601 k9'da (ÖK-1…ÖK-12) runbook'a bağlı. Satışı ETBİS'e bağlamak `02 §3.1.4`'e aykırı olurdu. | Yok |
| F3 | T04 k8 her ana dal birleşmesinde üç imajın yayımlanmasını ister; imaj tanımları T06'da doğar, sıra T04 → T06 (parça 1 B-2; T04, T06) | KISMİ (Yüksek) | Çelişki gerçek: yürütme sırasında T04 T06'dan önce (11:177), `Bağımlılık` yalnız T03; k8 (ve Paketleme komutu, k1 + k6) imaj tanımı olmadan T04'ün kendi PR'ında kırmızı kapı verir (`publish.yml` `ci-run.sh PUBLISH_CMD` koşar, dolu komut + Dockerfile yok = adım düşer). Önerinin ilk seçeneği (T06'yı öne almak) geçersiz: T06 T04'e bağlıdır ve CI Paketleme adımını ister, döngü oluşur. Doğru düzeltme ikinci seçeneğin daraltılmışı: Paketleme ve yayın komutları T06'da imajlarla girer; o güne dek boş komut uyarıyla atlanır (`SETUP_COMPLETE=false`, `.github/ci-run.sh`); yayımlama davranışı T06 k8'e taşınır. Matris değişmez (T06 `05 §8.3`'ü zaten taşır); T04 bloğu 2 560 → 2 542 bayt. | T04 k1, k6, k8, KK1, KK6, KK8; T06 k8, KK8 (8 çift) |
| F4 | T368 k6 fesih satırında kanuni faiz için yalnız uyarı gösterir, faizi hesaplamaz; T370'te faiz tutarı yok (parça 3 B-2; T368, T370) | ❌ RET (tekrar eden: 2. tur F9, 3. tur F5, 4. tur F1; Yüksek → Düşük) | Yeni bilgi yok. Mevzuat okuması doğru (MSY m.16/3 faizli iadeyi satıcıya yükler), ama üst doküman bu yükü bilinçli kararla firmaya bırakır: `02` §7.2.3, §12.1.8, §12.5 tablosu ("sistem faizi hesaplamaz … Kanuni faizi hesaplar ve müşteriye öder"; K-499, K-571). T368 k6 kaynak cümlenin aynısı; T370'te tutar kriteri olmaması da üst dokümanlarla kapalı (`06` "faiz alanı yok", `07` faiz hesabı yok). Gösterilen Bakanlık sayfası resmî metin değil, türetilmiş açıklama. | Yok |
| F5 | T548'in aday tablo kaynağı `07 §4.31.2`; §7.1'de bu kaynak için satır, §7.2'de T548 için bu kaynak yok (parça 5 B-1; T548) | ❌ RET (tekrar eden: 1. tur F8, 5. tur F13; Yüksek → Düşük) | Yeni bilgi yok. `07`'nin matris birimi uçtur; §4.31.2'nin birimi `07 UC-83` satırıdır ve T312'ye eşlidir (11:6163, 11:7582). T548'in `09 §16` + devir ile bağlanması K-1435'in açık hükmüdür (K-1354, 11:16). "Yalnız bir yönde olan çift yok" cümlesi §7.1 "Task" hücreleri ile §7.2 eşitliğini anlatır (11:7743, 11:5937); aday tablonun "Kaynak birimi" sütunu §7.2'nin kopyası değildir (T230'da da farklıdır). T548 → API bağı `Dokümanlar` (11:5496) ve KK6'dadır (11:5503). | Yok |
| F6 | T348 bloğu 2,5 KB sınırını aşar (parça 3 B-3; T348) | ❌ RET (Orta → Düşük) | Olgu yanlış: T348 bloğu denetim betiğinin ölçüsüyle 2 534 bayttır (sınır 2 560, `11_task_blok_denetle.py` `MAKS_BAYT`, K-1406 (4); 11:41, 11:169 madde 8); `--tam` TOPLAM SORUN 0. Bayt ölçümü zaten kapıya bağlıdır. Aynı kalıp 3. tur F13 (T509) ve 5. tur F11 (T04) olarak da reddedildi; ikinci model 2,5 KB'ı 2 500 bayt saymış olmalı. | Yok |
| F7 | T525 bloğu 2,5 KB sınırını aşar (parça 4 B-1; T525) | ❌ RET (Orta → Düşük) | Olgu yanlış: T525 bloğu 2 559 bayt (sınır 2 560); `--tam` TOPLAM SORUN 0. F6 ile aynı gerekçe. Not (bulgu değil): T525 sınıra 1 bayt uzaktır; bu bloğa sonradan eklenecek her metin bloğu bölmeyi (K-1409 (2)) ya da kısaltmayı gerektirir. | Yok |
| F8 | T117 k3 `Undelivered`'da gönderim anı yalnız gönderim sonrası geri dönen satırda dolu; KK3 dolu da boş da kabul eder (parça 1 B-3; T117) | ❌ RET (tekrar eden: 1. tur D3 #2, 3. tur #6, 5. tur F10; Orta → Düşük) | Yeni bilgi yok; metin değişmedi (11:781, 11:789). Kabul kriteri 3 gönderim anını zorunlu kılmaz, iki hâli birlikte tarif eder: hiç gönderilmemiş (denemeleri tükenen) satırda boş, gönderilip geri dönen satırda dolu (`06 §4.58` Kısıtlar, K-1120; `08` SentAt kalır). KK3 kuralın doğrudan karşılığıdır; alanı zorunlu yapmak `06 §4.58` ile çelişirdi. | Yok |
| F9 | T378 k2–3: listenin kapsamadığı yılda resmî tatil hafta içine gelirse aynı gün hem iş günü dışı hem iş günü sayılır (parça 3 B-1; T378) | ❌ RET (Orta → Düşük) | Çelişki yok: listenin kapsamadığı yılda yalnız hafta sonunun dışlanması üst dokümanın bilinçli kuralıdır (`02 §4.1.2`, K-558; `05 §7.2.3`), sonucu firmaya bırakılan, liste bakım sürümleriyle yenilenen bilinçli kalan risk olarak kayıtlıdır (K-481, `05` satır 1146). Öneri üst dokümanın kararını değiştirirdi; 11 üst dokümana uymak zorundadır (K-1406 (2)(d)). Yan yüzler (11:1518, 11:1527, 11:3633) üst dokümanla uyumlu. | Yok |

**Tekrar eden RET (turun sonucunu belirlemez):** F1 (2., 3., 4. tur), F2 (2., 3., 4. tur), F4 (2., 3., 4. tur), F5 (1. ve 5. tur), F8 (1., 3., 5. tur) — aynı yer ve aynı iddiayla döndü; F6 ve F7 aynı sınıfın (blok bayt ölçüsü) dönüşüdür. **Seviye düşenler:** Yüksek → Düşük: F1, F2, F4, F5; Orta → Düşük: F6, F7, F8, F9 (F3 Yüksek kaldı). **Kısmi kalış:** F3 (öneri daraltıldı: T06'yı öne alma kolu döngü yaratır).

**F3 — uygulama ve gözlem.** Sekiz çift `11_audit_uygula.py` ile uygulandı; yazım kapanışı, matris kapsama, task blok denetimi (`--tam`), geri yön denetimi sorun 0. T04'ün k6'sı artık "Paketleme ve yayın komutları imajlarla T06'da girer; o güne dek boş komut uyarıyla atlanır" der; T04 k9 ("boş komut kapıyı geçmez") `SETUP_COMPLETE=true` hâlindedir ve çelişmez (KK9: "boş komutla `SETUP_COMPLETE=true` CI'ı düşürür"). Bu yeni cümleyi tur 7 ikinci modelle sınar.

## 4. Ek bulgular

- Doğrulayıcılardan `11`'i ya da üst dokümanları etkileyen ek bir tutarsızlık bildirilmedi. Üst doküman notu yok; `00`/`CLAUDE.md`/`SETUP.md` önerisi yok.
- F7 için gözlem: T525 bloğu 2 559 / 2 560 baytta; bir sonraki düzeltmede bu bloğa eklenecek metin bloğu bölmeyi gerektirir.

## 5. Kullanıcı onay checklist'i

- [ ] F1 — RET, tekrar eden (uygulanmadı)
- [ ] F2 — RET, tekrar eden (uygulanmadı)
- [ ] F3 — KISMİ (uygulandı: T04 k1, k6, k8, KK1, KK6, KK8; T06 k8, KK8)
- [ ] F4 — RET, tekrar eden (uygulanmadı)
- [ ] F5 — RET, tekrar eden (uygulanmadı)
- [ ] F6 — RET (uygulanmadı)
- [ ] F7 — RET (uygulanmadı)
- [ ] F8 — RET, tekrar eden (uygulanmadı)
- [ ] F9 — RET (uygulanmadı)

⚠ karar: yok. Önerilen `00`/`CLAUDE.md`/`SETUP.md` değişikliği yok.

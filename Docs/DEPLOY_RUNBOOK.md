# Shopfolio — Deploy Runbook

**Son güncelleme:** YYYY-AA-GG | **Durum:** ⬚ Doldurulmayı bekliyor

> **Ne zaman doldurulur:** Entegrasyon fazından itibaren parça parça; **kapanış ritüelinde finalize edilir** (00 §L adım 5).
>
> **Bu dokümanın varlık sebebi:** Kod tamam olsa bile, prod'da bir ayar eksikse sistem ya **hiç açılmaz** ya da bir kural **sessizce devre dışı kalır**. İkinci durum tehlikelidir çünkü hiçbir test bunu yakalamaz — yalnız bu doküman yakalar.

> **Aşama 2'den park edilen girdiler** — kaynak: [`PRODUCT_DISCOVERY_STATUS.md`](PRODUCT_DISCOVERY_STATUS.md) §2, mekanizma: K-36 (yeri: §8.3 AK0-04).
> Bu satırlar **talimattır, karar değildir** — bağlayıcı olan kaynak karardır; doldurulurken karara bağlanacak olan, talimatın nasıl uygulanacağıdır.
>
> - **K-699 · Panele erişimin kaybında kurtarma:** hiçbir yönetici panele giremediğinde — tek yöneticinin şifresi ve e-posta kutusu kaybolmuşsa ya da ele geçirilmiş bir hesap tek yönetici kalmışsa — kurulumu yapan, ilk yöneticiyi açtığı yolla (§H; `10` ÖK-7) yeni bir yönetici hesabı açar; hesap ele geçirilmişse o hesabın oturumlarını sonlandırır ve şifresini geçersiz kılar. Ürün içinde kurtarma yolu yoktur (`02 §10.2.7`, `03 §10.4`). Adımların kendisi, kurtarmayı kimin isteyebileceği ve isteğin nasıl doğrulanacağı bu dokümanın kararıdır.
> - **Dizin — Aşama 2 kararlarının bu dokümana devirleri:** karar kaydında K-647…K-718'in etki sütunlarında bu dokümanı gösteren bir atıf (bir karar) ve Kullanıcı Akışları'nın (`03`) gövdesinde bu dokümana iş bırakan cümleler [`CHECKPOINT_REPORTS/PHASE2_CONFLICT_SCAN.md`](CHECKPOINT_REPORTS/PHASE2_CONFLICT_SCAN.md) §6'dadır. Bir iş yalnız `03`'ün gövdesinde yaşar, karar satırı yoktur: ödeme sağlayıcısının anahtarlarının ya da sağlayıcının değişmesinde geçişin yöntemi (`03 §10.1.1.7`). Devir taraması (`checklists/document-stage.md` §4) buradan başlar; devrin evi karar satırıdır, dizin onu ikinci kez kaydetmez. Aşama 1'in dizini: `PHASE1_CONFLICT_SCAN.md` §6.

> **Aşama 3'ten park edilen girdiler** — kaynak: [`PRODUCT_DISCOVERY_STATUS.md`](PRODUCT_DISCOVERY_STATUS.md) §2, mekanizma: K-36 (yeri: §10.3 UI0-04).
> Bu satırlar **talimattır, karar değildir** — bağlayıcı olan kaynak karardır; doldurulurken karara bağlanacak olan, talimatın nasıl uygulanacağıdır.
>
> - **K-736 · İlk yöneticinin adı:** yönetici hesabı zorunlu bir ad taşır ve ilk yöneticinin adı kurulumda verilir — ilk yöneticiyi açan yol (§H) e-posta ve şifrenin yanında adı da alır; kurtarmada (K-699) açılan yönetici hesabı için de aynısı geçerlidir (`02 §10.2.1`, §10.2.4, §10.2.7; `10 §4.1` ÖK-7). Ad yönetici listesinde, işlem izinde ve yöneticilere giden bildirimlerde okunur. Adın nasıl verildiği bu dokümanın kararıdır (satır Aşama 3'ün çakışma taramasında açıldı).
> - **Dizin — Aşama 3 kararlarının bu dokümana devirleri:** karar kaydında K-723…K-850'nin etki sütunlarında bu dokümanı gösteren bir atıf (bir karar) [`CHECKPOINT_REPORTS/PHASE3_CONFLICT_SCAN.md`](CHECKPOINT_REPORTS/PHASE3_CONFLICT_SCAN.md) §6'dadır; Arayüz Tanımları'nın (`04`) gövdesinde bu dokümana iş bırakan cümle yoktur. Devir taraması (`checklists/document-stage.md` §4) buradan başlar; devrin evi karar satırıdır, dizin onu ikinci kez kaydetmez. Önceki aşamaların dizinleri: `PHASE1_CONFLICT_SCAN.md` §6, `PHASE2_CONFLICT_SCAN.md` §6.

> **Aşama 4'ten park edilen girdiler** — kaynak: [`PRODUCT_DISCOVERY_STATUS.md`](PRODUCT_DISCOVERY_STATUS.md) §2, mekanizma: K-36 (yeri: §12.3 TM0-04).
> Bu satırlar **talimattır, karar değildir** — bağlayıcı olan kaynak karardır; doldurulurken karara bağlanacak olan, talimatın nasıl uygulanacağıdır.
>
> - **K-863 · K-864 · K-865 · Sunucunun seçimi ve kurulumu:** kurulum firmanın adına kiralanan bir sanal sunucuda konteyner olarak kurulur — tek desteklenen hedef; sunucu Linux, en az 2 sanal işlemci, 4 GB bellek, 60 GB SSD ve sabit bir genel IP adresi taşır; seçim ölçütleri: sağlayıcı diski kendi tarafında yedekli tutar, veri merkezi Türkiye'dedir (kurulum önerisi, ürün kuralı değil — yurt dışı seçimde aktarımın şartları firmanın yükümlülüğüdür ve konum aydınlatma metnine yazılır), aylık bedel tavanda yedeğe ve e-postaya yer bırakır, anlık görüntü alınabilir (Teknik Mimari `05 §0`, §8; `10 §4.1` ÖK-2). Adımların kendisi bu dokümanın kararıdır.
> - **K-862 · Kurulumun aylık gider kalemleri:** tavan yaklaşık 10–20 $'dır ve sunucuyu, sunucunun dışındaki yedek alanını, e-posta gönderimini ve dış erişilebilirlik kontrolünü taşır; ödeme sağlayıcısının komisyonu ve alan adı tavanın dışındadır (K-861; `05 §0`). Kalemlerin kurulumdaki listesi bu dokümanın kararıdır.
> - **K-872 · Kurulumun sırası:** sunucuda dört konteyner — uygulama, veritabanı, ters vekil (Caddy), yedek aracı — tek bir Compose tanımıyla kurulur; alan adı sunucuya yönlendirilmeden (`10 §4.1` ÖK-1) sertifika alınamaz (`05 §3`). Adımların sırası bu dokümanın kararıdır.
> - **K-870 · Veritabanının inceleme araçları:** veritabanı PostgreSQL'dir; arızada kullanılacak inceleme ve sorgu araçları bu dokümana yazılır (`05 §2`). Araçlar ve erişim yolu bu dokümanın kararıdır.
> - **K-876 · K-878 · K-879 · K-880 · K-888 · Yedek, kurtarma ve prova:** veritabanının işlem günlüğü en geç bir dakikada bir, tam yedeği her gece pgBackRest ile sunucunun dışındaki, firmanın hesabındaki S3 uyumlu nesne depolamaya şifreli gider; dosyalar her gece eşitlenir; saklama otuz gündür ve nesne depolamada silmeye karşı koruma açılır; yedek anahtarı kurulumda firmaya teslim edilir ve kurulumu yapanda da durur; kurtarma hedefi yeni bir sunucuda aynı gündür; prova kurulumda ve her güncellemede yapılır ve sonucu buraya yazılır; en son andan eski bir ana dönüşten sonra imha işi çalıştırılır ve firma bilgilendirilir (`05 §7`; `10 §4.1` ÖK-2). Adımlar, aracın ayarları, anahtarın teslim biçimi ve prova kaydı bu dokümanın kararıdır. Sunucunun diski kalıcı olarak kaybolursa en çok son bir dakikanın kayıtları yedeğe ulaşmamış olabilir — proje sahibi bu pencereyi kabul etti (K-888); kurtarmadan sonra kurulumu yapan kurtarılan son anı firmaya bildirir ve ürünün tanımadığı sağlayıcı bildirimlerini gelen bildirim kaydından firmaya iletir; firmanın adımları `02 §3.34.8`'dedir.
> - **K-896 · K-898 · Sunucu saati ve sağlık ucu:** sunucunun saati ağ zaman servisiyle (NTP) eşitlenir — bütün süreler ona bağlıdır; uygulama sitenin genel adresindeki sağlık ucuna kendi sunucusundan ters vekil üzerinden istek yapar ve kesintiyi bununla ölçer — ters vekilin ve güvenlik duvarının bu isteği geçirmesi gerekir (`05 §5`). Adımlar bu dokümanın kararıdır.
> - **K-910 · K-911 · Sırlar:** sırlar sunucuda, kök kullanıcının okuyabildiği bileşen başına ayrı dosyalarda durur ve yalnız onu kullanan konteynere bağlanır; §B'nin envanteri — uygulama, göç, yedek aracı ve veritabanı için ayrı satırlar, "Hangisi ALMAMALI" sütunu dahil — `05 §6.x`'e göre doldurulur; veritabanı parolaları ve nesne depolama anahtarı yılda bir ve olay üzerine, sağlayıcı anahtarları olay üzerine döner; yedeğin şifreleme anahtarı döndürülmez; kurtarmada sırlar vereninden yeniden alınır ya da yeniden üretilir (`05 §6.x`). Envanter, döndürme ve kurtarma adımları bu dokümanın kararıdır.

---

## 0. Hızlı özet

| Soru | Cevap |
|---|---|
| Sistem hangi ayarlar olmadan **açılmaz**? | §A |
| Hangi secret'lar **zorunlu**? | §B |
| Hangi ayarlar set edilmezse bir kural **sessizce devre dışı kalır**? | §C ⚠ |
| Hangi ayarlar yeniden başlatma gerektirir? | §D |
| Hangileri çalışırken değiştirilebilir? | §E |
| Deploy sonrası ne doğrulanır? | §F |
| Geri alma | §G |
| Seed edilemeyen ilk kayıtlar (ilk yönetici vb.) nasıl oluşur? | §H |
| Gerçek konfigürasyonla boot provası | §I ⚠ |

---

## A. Zorunlu ayarlar (yoksa sistem açılmaz)

> **Kaynak:** `06_DATA_MODEL.md` §8 fail-fast listesi.
> Bunlar bilinçli olarak varsayılan değer **taşımaz** — iş-kritik bir değerin sessizce yanlış varsayılanla çalışması, açılmamasından daha tehlikelidir.

| # | Ayar | Tip | Örnek değer | Ne işe yarar |
|---|---|---|---|---|

## B. Operasyonel secret'lar (zorunlu)

> Strateji `05_TECHNICAL_ARCHITECTURE.md §6`'da; burada **envanter** tutulur.
> **Dağıtım yarıçapı kolonu boş bırakılmaz** — "hepsine verilir" bir cevap değil, bir karardır ve gerekçesi yazılır.

| # | Secret | Nereden alınır | **Hangi bileşen alır** | **Hangisi ALMAMALI** | Rotation | Runbook |
|---|---|---|---|---|---|---|

## C. ⚠ Prod'da ayarlanması gereken ayarlar (yoksa kural sessiz kalır)

> **En kritik bölüm.** Buradaki bir ayar eksikse sistem sorunsuz açılır, testler geçer, ama bir iş kuralı **hiç çalışmaz**.
> Her satır için: varsayılan neden yetersiz + hangi kuralın devre dışı kalacağı.

| # | Ayar | Varsayılan | Varsayılanla ne olur | Prod'da ne olmalı |
|---|---|---|---|---|

## D. Bileşenler arası konfigürasyon eşleşmesi

> **Ne yazılır:** Aynı değerin birden fazla yerde tanımlandığı durumlar (ör. bir zamanlama aralığı hem arka planda hem yardımcı serviste). Hangisi otoriter, değişince ne yeniden başlatılmalı.

| Ayar | Nerelerde tanımlı | Otoriter olan | Değişince |
|---|---|---|---|

## E. Çalışırken değiştirilebilen ayarlar

| # | Ayar | Etki süresi | Not |
|---|---|---|---|

## F. Deploy sonrası doğrulama

- [ ] Sağlık uçları yanıt veriyor
- [ ] Zorunlu ayarların hepsi yüklü (§A)
- [ ] §C ayarları **açıkça** kontrol edildi (varsayılanda kalan yok)
- [ ] Arka plan işleri kayıtlı ve çalışıyor
- [ ] Dış entegrasyonlar canlı uçlara bağlanabiliyor
- [ ] Loglar akıyor, uyarı kanalı çalışıyor
- [ ] Geri alma (rollback) yolu denendi

## G. Geri alma

> **Ne yazılır:** Bir deploy geri alınacaksa hangi adımlar; şema değişikliği geri alınabilir mi, alınamıyorsa ne yapılır.

---

## H. Seed edilemeyen ilk kayıtlar

> **Ne yazılır:** Migration ve seed'in kapsayamayacağı, **kuruluma özgü** ilk kayıtlar ve bunları oluşturan **izlenen, sırsız** bootstrap script'i.

Yetkilendirmesi olan her sistem ilk deploy'da aynı duvara çarpar: *"sistem açıldı, migration geçti, testler yeşil — ama kimse yönetici değil ve yönetici yaratacak bir uç yok."* Bu an geldiğinde çözüm elle bir veritabanı komutu olur: izlenmez, tekrarlanamaz, ikinci ortamda baştan keşfedilir ve genelde sır içeren bir komut geçmişi bırakır.

**Kural:** Bu kayıtlar için repo'da **izlenen** bir bootstrap script'i bulunur. Değerler script'e **gömülmez** — parametre veya ortam değişkeni olarak dışarıdan verilir.

| # | Kayıt | Neden seed edilemez | Bootstrap script'i | Parametreler |
|---|---|---|---|---|
| 1 | İlk yönetici + rol ataması | Kimin yönetici olacağı kuruluma özgüdür; kodda varsayılanı olamaz | | |
| 2 | Operatör / servis hesabı | Aynı | | |

---

## I. Gerçek konfigürasyonla boot provası ⚠

> **Ne zaman:** Kapanış ritüelinde, **zorunlu** (`00 §L` adım 6, detay `00 §L.1`).
> **Neden:** Faz gate'leri "tüm servisleri ayağa kaldır" der ama test/E2E konfigürasyonuyla tatmin edilebilir. Bu prova **gerçek dağıtım konfigürasyonunu** ilk kez sınar.

### I.1 Ön koşullar
- [ ] §A zorunlu ayarların tamamı gerçek değerlerle hazır
- [ ] §B secret'ları hedef ortamda tanımlı
- [ ] §H bootstrap parametreleri belirlenmiş

### I.2 Boot reçetesi

> **Ne yazılır:** Adım adım, ikinci bir kişinin tekrarlayabileceği açıklıkta. "Bilinen" adımlar da yazılır.

### I.3 Prova sonucu

| # | Adım | Sonuç | Kırıldıysa: kök neden | Runbook'a eklenen satır |
|---|---|---|---|---|

> **Provanın asıl çıktısı bu tablodur.** Kırılan her şey bir runbook satırına dönüşür; prova "başarılı oldu" diye kapanmaz, **runbook'u tamamladı** diye kapanır.

### I.4 Doğrulama
- [ ] Sağlık uçları gerçek konfigürasyonda yanıt veriyor
- [ ] Bileşenler arası kimlik ve adresler gerçek değerlerle çözülüyor
- [ ] §C ayarları varsayılanda kalmadı (kural sessiz değil)
- [ ] Reçete ikinci kez, sıfırdan tekrarlandı

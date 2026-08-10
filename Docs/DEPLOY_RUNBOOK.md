# <PROJE> — Deploy Runbook

**Son güncelleme:** YYYY-AA-GG | **Durum:** ⬚ Doldurulmayı bekliyor

> **Ne zaman doldurulur:** Entegrasyon fazından itibaren parça parça; **kapanış ritüelinde finalize edilir** (00 §L adım 5).
>
> **Bu dokümanın varlık sebebi:** Kod tamam olsa bile, prod'da bir ayar eksikse sistem ya **hiç açılmaz** ya da bir kural **sessizce devre dışı kalır**. İkinci durum tehlikelidir çünkü hiçbir test bunu yakalamaz — yalnız bu doküman yakalar.

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

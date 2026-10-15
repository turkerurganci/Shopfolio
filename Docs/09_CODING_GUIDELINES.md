# Shopfolio — Coding Guidelines

**Versiyon: v0.1** | **Bağımlılıklar:** `02`, `04`, `05`, `06`, `07`, `08`, `10_MVP_SCOPE.md` | **Son güncelleme:** YYYY-AA-GG

> **Aşama:** 8 — Kodlama Kılavuzu · **Rol:** Tech Lead
> **Amaç:** Ajanın tutarlı kod üretmesi. Bu doküman **her kodlama görevinde** ajana verilir.
> **Uyarı:** Bu doküman diğerlerinden zordur — hem çok sayıda dokümanla tutarlılık hem implementasyon düzeyi detay ister; kalite döngüsü genelde daha uzun sürer. Burada eklenen her yeni alan/parametre `06`'da tanımlı mı diye **kontrol edilmelidir**.

> **Aşama 1'den park edilen girdiler** — kaynak: [`PRODUCT_DISCOVERY_STATUS.md`](PRODUCT_DISCOVERY_STATUS.md) §2, mekanizma: K-36.
> Bu satırlar **talimattır, karar değildir** — bağlayıcı olan kaynak karardır; bu aşamada karara bağlanacak olan, talimatın nasıl uygulanacağıdır.
>
> - **K-17 · Terim sözlüğü:** `02 §1` sözlüğü **birebir devralınır**; sembol, dosya ve tip adları sözlüğün İngilizce karşılıklarıyla — kapalı liste değerleri Veri Modeli'nin (`06`) verdiği adlarla (K-531) — üretilir. Eş anlamlı ad kullanımı yasaktır — bir kavramın tek adı vardır.

> **Aşama 4'ten park edilen girdiler** — kaynak: [`PRODUCT_DISCOVERY_STATUS.md`](PRODUCT_DISCOVERY_STATUS.md) §2, mekanizma: K-36 (yeri: §12.3 TM0-04).
> Bu satırlar **talimattır, karar değildir** — bağlayıcı olan kaynak karardır; bu aşamada karara bağlanacak olan, talimatın nasıl uygulanacağıdır.
>
> - **K-866 · K-867 · Dil, çatı ve kütüphane ekleme:** kod C# ile, .NET 10 (LTS), ASP.NET Core ve Entity Framework Core üzerinde yazılır; ürün yalnız LTS sürümlerinde yürür; eklenen her kütüphanenin lisansı ücretsiz ve ticari kullanıma açıktır ve bakımı süren bir projedir (Teknik Mimari `05 §2`). C# kod standartları ve kütüphane eklemenin işletimi bu aşamanın kararıdır.
> - **K-868 · K-869 · Sayfa ve ön yüz düzeni:** sayfalar Razor Pages ile sunucuda üretilir, form gönderimi gönder–yönlendir–göster düzenindedir; temel akışlar JavaScript kapalıyken de tamamlanır; ön yüz çatısı ve npm derleme hattı yoktur — düz JavaScript modülleri ve düz CSS; üçüncü taraf betik ve yazı tipi sunucusu kullanılmaz (`05 §2`). Dosya düzeni ve adlandırma bu aşamanın kararıdır.
> - **K-871 · K-881 · K-887 · Modül sınırları, dosya deposu ve önbellek:** ürün tek dağıtılabilir modüler monolittir, modüller birbirinin tablosuna doğrudan yazmaz; dosyalara bir dosya deposu arayüzü üzerinden erişilir; önbellek uygulamanın belleğindedir ve ilgili kayıt kaydedildiğinde geçersiz kılınır (`05 §3`, §7). Sınırların kodda nasıl korunduğu, arayüzün kullanımı ve geçersiz kılmanın kalıbı bu aşamanın kararıdır.
> - **K-882 · Görüntü işleme kütüphanesi:** yüklenen görsel sunucuda yeniden kodlanır, meta verisi atılır ve WebP türevleri üretilir (`05 §7`); kütüphanenin lisansı ücretsiz ve ticari kullanıma açıktır. Kütüphanenin seçimi bu aşamanın kararıdır.
> - **K-889 · K-891 · K-895 · Geçiş birimi, iş kuyruğu ve zaman:** siparişin durum alanlarına yalnız geçiş birimi yazar; dış yan etkiler iş kuyruğuna yazılır ve işleyiciler aynı satırı ikinci kez gördüğünde sonucu değiştirmez; zaman tek bir kaynaktan okunur (Teknik Mimari `05 §4`, §5). Kuralların kodda nasıl korunduğu — mimari testler dahil — ve zamanın sınanabilirliği bu aşamanın kararıdır.
> - **K-899 · K-900 · K-901 · K-903 · K-909 · K-912 · Güvenlik kalıpları:** oturum sunucuda tutulur, şifre çatının özetleyicisiyle saklanır; istemci IP'si yalnız ters vekilin ilettiği başlıktan okunur, IPv6'da /64; anahtarlar kriptografik rastgele üreteçle üretilir; şifre, anahtar, sır, kart verisi, IBAN, e-posta, adres ve telefon hiçbir kayda yazılmaz, bağlantı anahtarı taşıyan adres maskelenir; yetki sunucuda, varsayılan kapalı politikayla ve kayıt düzeyinde süzülerek denetlenir (`05 §6`). Kalıpların kodda kullanımı bu aşamanın kararıdır.
> - **K-914 · K-915 · Hata yakalama ve kayıt:** beklenmeyen hata tek bir yakalama katmanında üç hâle ayrılır ve sonucu belirsiz işlemler kodda adıyla işaretlidir; uygulama kaydı yapılandırılmıştır (her satır bir JSON nesnesi), istek kimliğini erişim kaydıyla aynı değerle taşır, hesabı iç kimlikle anar ve şifre, anahtar, sır, kart verisi, IBAN, e-posta, adres, telefon, form gövdesi ve çerez değerini hiçbir düzeyde yazmaz (Teknik Mimari `05 §9`, §10). Kayıt kütüphanesi, alanların adları ve maskelemenin kodda uygulanışı bu aşamanın kararıdır.
> - **K-919 · K-920 · K-921 · Geliştirme ortamı, imaj ve testler:** geliştirme ortamı kurulumun Compose tanımının bir varyantıdır — aynı veritabanı sürümü, ödeme sağlayıcısının test ortamı ve e-postaları yakalayan yerel bir araç; uygulama imajı çok aşamalı derlenir, kök olmayan kullanıcıyla çalışır ve göç demetini taşır; birim testleri modül sınırlarının mimari testlerini içerir, entegrasyon testleri gerçek PostgreSQL 18'e karşı koşar (`05 §8`). Dosya düzeni, test projelerinin ayrımı ve araçlar bu aşamanın kararıdır.
> - **K-922 · K-923 · Ayarlar ve yönetim komutları:** ayarlar açılışta doğrulanır — zorunlu ayar eksikse uygulama açılmaz; ürün sabitleri kodda tek bir yerdedir ve ayar dosyası onları değiştiremez; Teknik Mimari'nin tasarım değerleri varsayılanla kodda, değiştirilebilir olarak kurulum ayar dosyasındadır; kurulum tarafının işlemleri uygulama imajındaki yönetim komutlarıdır ve web'den çağrılamaz (`05 §8`). Ayar sınıflarının ve komutların kodda düzeni bu aşamanın kararıdır.
> - **K-925 · Göçlerin yazılışı:** göçler genişlet–daralt kuralıyla yazılır — bir sürümün göçü önceki sürümün kodunu bozmaz; yama sürümü göç taşımaz; büyük veri dönüşümleri göçte değil, kuyruktaki bir arka plan işiyle parça parça yapılır (`05 §8`). Kuralın kodda ve incelemede nasıl denetlendiği bu aşamanın kararıdır.

---

## 1. Temel ilkeler

- Somut örnekle tanımla. "Hata yönetimi yapılmalı" değil, "hata yönetimi şöyle yapılır: (somut örnek)".
- Tek kaynak, tek doğru. Bir kural iki yerde farklı anlatılmaz.

## 2. Source of truth hiyerarşisi

> **Ne yazılır:** Çelişki durumunda hangi doküman kazanır. Örn. iş kuralı → `02`; şema → `06`; sözleşme → `07`.

## 3. Genel kod yazım kuralları

## 4. Proje yapısı ve klasör organizasyonu

## 5. Adlandırma konvansiyonları

## 6. Mimari sınırlar

> **Ne yazılır:** Katman sınırları, modüller arası iletişim kuralı, iş kuralının hangi katmanda yaşayacağı.

## 7. Zaman ve biçim standartları

> **Ne yazılır:** Zaman damgası kaynağı (yerel saat kullanımı yasağı), tarih/sayı biçimleri, zaman dilimi politikası.

## 8. Hata yönetimi

## 9. Domain kuralları

> **Ne yazılır:** Durum makinesi kullanımı, geçiş koruma, yan etki yerleşimi, olay yayını, denetim kaydı.

## 10. Veri erişimi

> **Ne yazılır:** Sorgu kuralları, sayfalama zorunluluğu, izleme kapalı sorgular, işlem sınırları.

## 11. Entegrasyon kuralları

## 12. Sözleşme ve olay sürümleme

## 13. Arka plan iş kuralları

## 14. Hassas hesaplama kuralları

> **Ne yazılır:** Para/oran/ölçü hesaplamalarında tip seçimi, yuvarlama politikası (**ara adımda yuvarlama yok**), test zorunluluğu.

## 15. Güvenlik kuralları

## 16. İstemci tarafı kurallar

## 17. Yardımcı servis / sidecar standartları (varsa)

## 18. Loglama ve gözlemlenebilirlik

## 19. Test kuralları

> **Ne yazılır:** Test katmanları, hangi davranış hangi katmanda test edilir, test verisi stratejisi, izolasyon.
> **Ek kural:** `skip` / `only` / boş assertion içeren test **yazılmaz**; geçen ama hiçbir şey doğrulamayan test, boşluğun gizlenmiş hâlidir (gate check bunu tarar).

## 20. Performans kuralları

## 21. Git ve CI/CD kuralları

### 21.1 Dal stratejisi

> **Kural:** Bu bölüm `.claude/INSTRUCTIONS.md §3.2` ile **birebir aynı** olmalıdır. İki farklı yerde iki farklı dal stratejisi, bu dokümanların en sık çelişkisidir.

| Dal | Amaç |
|---|---|
| `main` | Çalışır durumda kod. Doğrudan push **yasak**. |
| `task/TXX-*` | Tek bir task. `main`'den ayrılır, `main`'e squash merge olur. |
| `chore/*`, `docs/*`, `infra/*` | Task'a ait olmayan işler — ayrı PR. |

### 21.2 Commit mesaj formatı

```
{tip}: {kısa açıklama}      # task dışı
TXX: {kısa açıklama}        # task commit'i
```

Tipler: `feat` · `fix` · `refactor` · `test` · `docs` · `chore` · `migration`

### 21.3 PR kuralları

- PR tek bir amaca hizmet eder. Karışık değişiklik yasak.
- **Bundled-PR yasağı** — başka bir task'ın PR'ına commit gömülmez.
- CI geçmeden merge yasak. Bağımsız validator PASS olmadan merge yasak.
- Merge'ü validator chat yapar.

### 21.4 CI pipeline sıralaması

```
0. Guard (direct push)
1. Lint
2. Build
3. Unit test
4. Integration test
5. Contract test
6. Migration dry-run
7. Paketleme doğrulaması
8. E2E (advisory)
→ CI Gate (toplu kapı)
```

Komutlar: `.github/ci-commands.env` · Kurulum: `Docs/CI_CD_SETUP.md`

### 21.5 Savunma katmanları

> Tek bir savunma katmanı yetmez. Katmanların tam listesi ve gerekçeleri: `00_PROJECT_METHODOLOGY.md` §F.
> Bu bölüm o listeye referans verir; **kuralı burada tekrar tanımlama** — iki yerde iki farklı ifade çelişki üretir.

## 22. Refactor kuralları

- Refactor davranış değiştirmeden yapılır. Davranış değişecekse **ayrı commit/PR**.
- Refactor PR'ı feature PR'ıyla karıştırılmaz.
- Kritik akışa dokunan kod, test veya koruma olmadan taşınmaz.

## 23. Yasaklar

> **Ne yazılır:** Koşulsuz yasaklar — iş kuralı uydurma, sessiz kapsam genişletme, katman ihlali, secret gömme, hassas veri loglama, exception yutma, sınırsız liste dönme, testsiz kritik davranış değişikliği, CI geçmeden merge.
> Süreç yasakları için ayrıca: `.claude/GUARDRAILS.md` §8.

## 24. AI çalışma kuralları

> **Ne yazılır:** Kod yazmadan önce hangi source-of-truth kontrol edilir · küçük diff · mevcut mimariyi bozmama · gereksiz bağımlılık eklememe · kritik akışlara özel kontroller (durum geçişi eklerken guard + yan etki + olay + denetim kaydının tamlığı gibi).

## 25. Review checklist

> **Ne yazılır:** Her kod tesliminde sorulacak sorular — doküman uyumu, mimari, durum/olay, güvenlik, test, performans.

---

*Shopfolio — Coding Guidelines v0.1*

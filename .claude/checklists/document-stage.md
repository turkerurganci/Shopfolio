# Doküman Üretim Aşaması — İşletim Checklist'i

**Katman:** L4 (checklist) | **Son güncelleme:** 2026-07-27

> **Bu neden skill değil?**
> Doküman üretimi projeden projeye en çok değişen dönemdir ve bu adımlar bir referans projede
> **fiilen böyle işletildi** — ama hiç skill'e dönüştürülmedi. Yaşanmamış bir soyutlamayı skill
> olarak dondurmak yerine, yaşananı birebir checklist olarak koyuyoruz. Bir kez gerçek bir projede
> işletildikten sonra skill'e dönüştürülebilir. (Karar: yeni skill icat edilmez.)

---

## Her aşamada, sırayla

### 1. Aşama açılışı

- [ ] `Docs/00_PROJECT_METHODOLOGY.md` §C.1'den bu aşamanın **rolünü** al ve o role gir
- [ ] Aşamanın **girdi dokümanlarını** tam olarak oku (00 §C.6 katman tablosu)
- [ ] Aşamanın **çıktısını** ve beklenen kapsamını proje sahibiyle netleştir
- [ ] Bu aşamada **konuşulmayacak** konuları hatırla (aşama disiplini — GUARDRAILS §1)

### 2. Traceability matrisi (zorunluysa)

00 §C.1 tablosunda "Traceability zorunlu: **Evet**" ise, **çıktı üretilmeden önce**:

- [ ] Kaynak dokümanlardan öğe envanteri çıkar (numaralandırılmış)
- [ ] **İleri izlenebilirlik:** her kaynak madde → hangi çıktıya eşlenecek?
- [ ] **Geri izlenebilirlik:** her planlanan çıktı → hangi kaynaktan besleniyor?
- [ ] Eşlenmeyen kaynak madde = **GAP** → listele
- [ ] Kaynağı olmayan çıktı = **gerekçesiz ekleme** → listele
- [ ] GAP'leri proje sahibine sun, **karar al**, kararları kaydet
- [ ] Ancak bundan sonra çıktı üretimine başla

### 3. Workshop döngüsü (her konu için)

- [ ] Konuyu tanıt: neden bu konuyu şimdi konuşuyoruz
- [ ] Seçenekleri artı-eksileriyle sun
- [ ] **Kendi önerini** gerekçesiyle belirt
- [ ] Proje sahibinden karar al (onay veya farklı yön)
- [ ] **Edge case kontrolü:** *"bu kararın yaratacağı risk veya boşluk var mı?"*
- [ ] Kararı **anında** `Docs/PRODUCT_DISCOVERY_STATUS.md`'ye yaz
- [ ] Sonraki konuya geç (cevapsız soru bırakma)

### 4. Doküman yazımı

- [ ] Header'ı doldur: **Versiyon** + **Bağımlılıklar** + **Son güncelleme**
- [ ] Konvansiyonları/ortak kararları **önce** yaz, detayları sonra
      *(Ortak konvansiyonlar baştan sabitlenirse detaylar hem hızlı hem tutarlı yazılır.)*
- [ ] Özet tabloları ile detay bölümlerini **eşzamanlı** güncelle
- [ ] Belirsiz ifade kullanma ("muhtemelen", "gereksinime göre belirlenecek")
- [ ] Ölü placeholder bırakma — "sonra doldurulacak" satırı bir kapıya bağlı değilse yazma

### 5. Doküman Tamamlama Protokolü (00 §C.4)

`✓ Tamamlandı` işaretinden **önce**:

- [ ] **Çapraz referans doğrulaması** — başka dokümandan alınan her enum/sayı/kural/terim kaynakla birebir mi? Kaynak referansı yazıldı mı?
- [ ] **İç tutarlılık** — özet tablolar ile detay bölümler çelişiyor mu?
- [ ] **Bağımlılık taraması** — header'daki her bağımlılık dokümanı hedefli tarandı mı? Orada tanımlı olup burada farklı/eksik kalan kural var mı?

### 6. Kalite döngüsü (00 §C.5)

- [ ] `/audit XX` — envanter bazlı sistematik denetim; sayılar tutuyor mu?
- [ ] Audit bulgularını uygula
- [ ] `/deep-review XX` — 8 katman
- [ ] Deep review bulgularını uygula
- [ ] `/cross-review Docs/XX_....md` — TEMİZ olana kadar tur tekrarla
- [ ] **Etki yansıtma** (cross-review Faz 5) — downstream + upstream + yeni alan taraması
- [ ] `/checkpoint` — aşama geçiş taraması

### 7. Aşama kapanışı

- [ ] Doküman durumu tablosunda `✓ Tamamlandı` işaretle
- [ ] Versiyon ve "son güncelleme" alanlarını yükselt
- [ ] **Öğrenim yaz** (00 §K) — bu adım atlanamaz:
      - Tek seferlik gözlem → aşama notunda kalır
      - Tekrarlanacak desen → `00_PROJECT_METHODOLOGY.md` §N
      - İhlali önleyen kural → **terfi et** (L1–L5, hedef dosya belirtilerek)
- [ ] Bir sonraki aşamanın girdi bağımlılıklarını kontrol et
- [ ] `/handoff` ile oturumu kapat

---

## Aşama-spesifik hatırlatmalar

| Aşama | Sık kaçan nokta |
|---|---|
| Product Discovery | "MVP'de yok" kararları da **gerekçesiyle** kaydedilir; kapsam dışı bırakma bilinçli bir karardır |
| Kullanıcı akışları | Durum makinesi (state) tanımları **arayüz tasarımından önce** netleşmeli; bildirim haritası akışlardan ayrı bir tabloda özetlenmeli; hata akışları happy path kadar detaylı |
| UI/UX | Ortak bileşen kütüphanesi **erken** tanımlanmalı; ekran navigasyon haritası ekran tanımlarından **önce**; {ekran × rol × durum} matrisi eksik varyantları yakalar; yönetim ekranları toplam ekranların yarısı kadar olabilir — ikincil endişe sayma |
| Teknik mimari | "MVP olarak düşünme, sonrası için de düşün" — sonradan düzeltilmesi çok pahalı kararları (olay kaybı, denetim izi) baştan doğru kur; maliyet kısıtı varsa **erken** söylenmeli, mimariyi doğrudan etkiler |
| Veri modeli | Enum'lar kaynak dokümandan **birebir**, şablondan kopyalanmaz; silme/saklama stratejisi erken; eşzamanlılık kontrolü baştan; denormalizasyon kararı "nerede güncellenir, tutarsızlık riski ne?" ile birlikte alınır |
| API tasarımı | Konvansiyonlar (URL, zarf, auth, sayfalama, hata) endpoint'lerden **önce** sabitlenir; durum × rol iş mantığını sunucuda tutup istemciye "yapılabilir aksiyonlar" göndermek istemci karmaşıklığını azaltır |
| Entegrasyon spec | Alan eşlemesi yazılırken veri modeli **açık tutulur**, her alan adı kontrol edilir; "ücretsiz seçenek MVP'ye yeter mi?" sorusu **önce** sorulur; bu doküman doğası gereği yeni endpoint ihtiyacı doğurur — API dokümanına geri yazım beklenir |
| Kodlama kılavuzu | En çok tur alan dokümandır (hem çok dokümanla tutarlılık hem implementasyon-düzeyi detay); burada eklenen her yeni alan **veri modelinde** tanımlı mı diye kontrol edilmeli |
| Implementation planı | Her task'ta **test beklentisi** alanı olmalı; ara task ihtiyacında `TXXa`/`TXXb` ile faz aralığı bozulmaz; henüz yazılmamış dokümana verilen referans **forward pointer** olarak açıkça işaretlenir (eksik bağımlılık değildir) |
| Doğrulama protokolü | Kapsam MVP kapsamıyla **birebir** hizalı olmalı; MVP'de olmayan özellik doğrulama kriteri olamaz, MVP'de olan çıkarılamaz |

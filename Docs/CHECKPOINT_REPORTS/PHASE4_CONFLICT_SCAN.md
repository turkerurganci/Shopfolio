# Aşama 4 — Çakışma Taraması

**Tarih:** 2026-10-17 | **Aşama:** 4 — Teknik mimari | **Kapanış adımı:** K-437'nin 3. adımı (K-429; `checklists/document-stage.md` §7)
**Girdi sürümleri:** karar kaydı v0.112 · Teknik Mimari (`05`) v0.14 · Ürün Gereksinimleri (`02`) v0.74 · Kullanıcı Akışları (`03`) v0.26 · Arayüz Tanımları (`04`) v0.26 · MVP Kapsamı (`10`) v0.49 · Proje Vizyonu (`01`) v0.33 · `06`–`09`, `11`, `12` ve `DEPLOY_RUNBOOK`'un park blokları
**Çıktı sürümleri:** karar kaydı v0.113 · `05` v0.15 · `02` v0.75 · `03` v0.27 · `04` v0.27 · `10` v0.50 · `01` değişmedi · `06`, `07`, `08`, `09`, `12` ve `DEPLOY_RUNBOOK`'un park blokları · `DEFERRED_BACKLOG.md` (D-01'in kanıtı)

> **Ne işe yarar:** Aşama kapanmadan önce açık kalemlerin her birinin **tek bir evi** ve **gözlemlenebilir bir kapısı** olduğunu doğrular; vadesi geçmiş açıkları adıyla listeler (K-429). Önceki aşamaların ve bu aşamanın park satırlarını, kaynak kararlarını sonradan değiştiren kararlara karşı okur. Aşama 4 kararlarının birbirleriyle ve önceki aşamaların kararlarıyla çakışmasını, geri işaretlerini ve olgusal öncüllerini denetler (checklist §3). Aşama 4'ün üst dokümanlara geri beslediği kuralların bütün yüzlerde aynı hâlde olduğunu denetler (K-652, betik 1). Aşama 4 kararlarının sonraki dokümanlara devirlerini tek dizinde toplar (§6) — **Veri modeli aşamasının açılışı bu dizini okur.** Sonuç checkpoint'in (4. adım) girdisidir.

---

## 1. Sonuç

| Kural | Sonuç |
|---|---|
| **(1) Aynı açık iki yerde durmaz** | ✓ — açık karar yok. Tracker §4'ün on sekiz satırının hepsi kapalı ve Aşama 4'te A-19 açılmadı. Teknik Mimari'nin şablonunda açık kararlar bölümü yok (K-857; konvansiyon 11 — açık kalem tracker §4'te A-19'dan sürer); dokümanda "açık" işaretli madde yok. Ürün Gereksinimleri §13 boş; Kullanıcı Akışları, Arayüz Tanımları, MVP Kapsamı ve Proje Vizyonu'nda açık kalem yok (§3). |
| **(2) Her açık satır gözlemlenebilir bir kapı taşır** (K-37) | ✓ — tracker'ın açık beş süreç maddesinin beşi de kapılı (§4). ⚠ öneriyle kayıt listesinin kapısı "en geç arşiv işaretinden önce" diyordu; gösterimin hazır metni eklendi (§4.1). Teknik Mimari'nin gövdesinde kapısız duran bir çok kritik soru adayı — desteklenen ödeme sağlayıcısının sayısı — Entegrasyon Spesifikasyonu'nun park bloğuna satır olarak girdi; kapısı o aşamanın açılışıdır (§4). |
| **(3) Vadesi geçmiş ve açık satır adıyla listelenir** | ✓ — **yok.** Süreç maddelerinin kapısı (checkpoint, öğrenim terfisi, arşiv işareti) henüz gelmedi. |

**Çakışmalar:** altı salt okuma merceği ve betikler **54 bulgu** döndürdü — **6 orta, 48 düşük** — hepsi bu PR'da kapandı. **Üç karar** (§5.1): **K-977** — Teknik Mimari'nin doğurduğu teknik kavramların ve kapalı listelerinin kod adlarını Veri Modeli verir (sözlük kuralının kapsamı; Ürün Gereksinimleri §1.1'e geri beslendi) · **K-978 ⚠** — firmanın "geri ödeme gerçekleşmedi" işaretinden sonra iadenin geç sonucunun uyarısı sağlayıcının bildirimine bağlıdır (kalan risk; K-940'ın öncülü) · **K-979 ⚠** — gecelik temizliğin kişisel veri taşıyan geçici kayıtlarının silinmesi imha kaydına yazılır (K-944'ün ilkesi). **Park satırları** (§5.3): Aşama 4'ün park satırlarında kalite döngüsünün daraltmalarının taşınmamış **32 yüzü** hizalandı; Aşama 1–3'ün park satırlarında **üç** hizasızlık (`DEPLOY_RUNBOOK` K-736, `08` K-705, `06` K-787); park etiketi ↔ etki sütunu betiği bir eksik buldu (K-900 → `08`) ve on yedi karar–hedef çifti etki sütunlarına eklendi. **Geri işaret** (§5.4): dokuz eksik. **Geri beslemenin yan yüzleri ve `05`'in iç tutarlılığı** (§5.2): beş — Ürün Gereksinimleri §10.4.6 kuralın evi olduğu hâlde K-940'ın engelini taşımıyordu. **Devir dizini** (§6): Aşama 4'ün 111 kararı sonraki dokümanlara 204 atıf taşır; **hiçbiri yalnız doküman numarası değildir**; karar satırı olmadan devredilen iki küçük iş var (§6.2). **⚠ listesi:** kayıtla birebir — 41 / 41 (§4.1). **Öğrenim adayları:** üç yeni (§7). **Çok kritik soru çıkmadı.**

---

## 2. Yöntem

Tarama gözle değil betikle yapıldı (K-429); anlamsal okuma altı salt okuma merceğiyle, ayrı alt ajanlarda ve ön planda koştu — beşli bir dalga ve ardından bir mercek; bulgular bulundukça çalışma dosyalarına yazıldı, her bulgu düzeltmeden önce güncel metne karşı yeniden okundu. Ortam: Git Bash ve node, Python yok.

1. **Tracker §4 ve §12.1:** `## 4.` ile `## 5.` arasındaki `| A-NN |` satırları ayrıştırıldı ve durum hücresi "Kapandı"ya karşı denetlendi; A-19 ve sonrası dosyanın tamamında arandı (eşleşmelerin hepsi "A-19'dan sürer" kural cümlesi). §12.1'in işaretsiz maddeleri listelendi. ⚠ listesi: §12.1'in listesindeki K numaraları ile karar kaydında konu hücresi `(öneriyle kaydedildi — ⚠` taşıyan K-855…K-979 satırları iki yönde karşılaştırıldı.
2. **Dokümanların açık kalemleri:** `05`, `01`, `02`, `03`, `04` ve `10`'da "açık karar", "açık kalem", "belirlenecek", "netleşecek", "karara bağlanacak", "TBD", "(downstream)", "sonra doldurulacak", "muhtemelen", "gereksinime göre" desenleri; her eşleşme okundu ("açık kalem" `02`, `03` ve `04`'te çoğunlukla siparişin açık kalemi terimidir).
3. **Park satırları:** hedef dokümanların (`04`–`09`, `12`, `DEPLOY_RUNBOOK`) "Aşama N'den park edilen girdiler" bloklarındaki `> - **…**` satırları betikle ayrıştırıldı (120 satır) ve etiketlerinden kaynak K'lar çıkarıldı (aralıklar açıldı; 166 kaynak). Her kaynak için numarası kaynaktan büyük bütün karar satırlarında — Aşama 1–3 kaynaklarında K-851'den itibaren, çünkü öncesini Aşama 3'ün taraması okudu — tam eşleşme (`K-NN` ardından rakam yok) arandı; ilişki sözcüğüne bakılmadı. 419 benzersiz eşleşme mercek başına dosyalara alıntısıyla döküldü (satır başına 569) ve **her eşleşme okundu**: M1 (Aşama 1–3'ün 66 satırı — 56 eşleşme), M2 (Veri Modeli, API Tasarımı, Doğrulama Protokolü'nün Aşama 4 satırları — 150), M3a (Entegrasyon Spesifikasyonu ve Kodlama Kılavuzu — 169), M3b (`DEPLOY_RUNBOOK` — 194). Ayrıca her Aşama 4 park satırının andığı `05 §…` maddeleri `05` v0.14'ün metnine karşı okundu. Park satırı etiketleri ile K-855…K-979'un etki sütunları iki yönde betikle karşılaştırıldı.
4. **Kararların çakışması (mercek Mc):** K-855…K-976'nın her satırında anılan daha küçük numaralı K'lardan eski satırın etki sütununda yeni numarayı taşımayanlar betikle çıkarıldı (632 çift; 94'ü alıntıda ilişki sözcüğü taşıyor) ve hepsi okundu; aynı konuyu sırayla daraltan on iki zincir eski karar hücresi, çelişen cümle ve `05`'in son hâli açısından okundu; 39 ⚠ kararın ve K-937…K-976'nın olgusal öncülleri kuralın evinde okundu (checklist §3).
5. **Geri beslemenin yan yüzleri (mercek Md, K-652 betik 1):** `05` başlık notunun geri besleme tablosundaki on yedi satırın kuralları nesne ve işlem terimleriyle, ek varyantları ve ünsüz değişimiyle `01`, `02`, `03`, `04`, `10` ve park bloklarında arandı — §12.4'ün terim listelerinin dışında kalan varyantlar öncelikle; sözlük, zaman satırları, bildirim listeleri, matris özetleri ve KP/ÖK satırları ayrıca okundu (~310 eşleşme). Kalite döngüsünün geri beslenmemiş kararlarının ürün yüzü ayrıca arandı.
6. **Devirler:** K-855…K-979'un etki sütunu ` · ` ile parçalandı; `06`–`09`, `11`, `12` ya da `DEPLOY_RUNBOOK` ile başlayan parçalar alındı, virgülle birleşik hedefler açıldı ve parçanın parantezi işin adı olarak okundu. `05`'in §0–§11 gövdesinde sonraki dokümanı — numarasıyla ya da adıyla — anan maddeler çıkarıldı ve hedef dokümanın Aşama 4 park bloğunun o bölümü anıp anmadığı betikle denetlendi; anmayanlar tek tek okundu. D-01'e (SETUP §2/§4, `CONTEXT.md`) giden maddeler ayrıca çıkarıldı.

---

## 3. Açık kalemler

### 3.1 Tracker §4

| Kapsam | Satır | Açık | Sonuç |
|---|---|---|---|
| A-01…A-18 (Aşama 1, salt okunur) | 18 | 0 | Aşama 1'in taraması §3.1 |
| A-19 ve sonrası (Aşama 4) | 0 | 0 | Aşama 4'te açık detay doğmadı: workshop'un, yazımın ve kalite döngüsünün bulduğu her boşluk oturumunda bir K satırıyla kapandı (K-856…K-976); bu tarama da K-977…K-979 ile kapattı |

### 3.2 Dokümanlar

| Kaynak | Bulunan | Sonuç |
|---|---|---|
| Teknik Mimari (`05`) | Şablonda açık kararlar bölümü yok (K-857); konvansiyon 11 açık kalemi tracker §4'e, sonraki aşamaya talimatı park bloğuna gönderir ve "bu dokümanda 'açık' işaretli madde bırakılmaz" der. Desen eşleşmeleri: "açık karar" ve "açık kalem" konvansiyon 11'in kural cümleleri; "karara bağlanacak" üç kez park bloklarının kalıp cümlesi; "(downstream)" konvansiyon 11; "muhtemelen" konvansiyon 10'un yasağı; "açık kalemi kalmayan sipariş" terimdir | Açık kalem yok |
| Ürün Gereksinimleri §13 | Tablo boş | Açık karar yok; Aşama 4'ün geri beslemesi (v0.67…v0.75) §13'e satır açmadı |
| Kullanıcı Akışları, Arayüz Tanımları, MVP Kapsamı | "açık karar" eşleşmeleri kural cümleleri ve eski başlık notları; "açık kalem" eşleşmeleri siparişin açık kalemi terimi | Açık kalem yok |
| Proje Vizyonu | "Açık karar" bir eşleşme — eski sürüm notu | — |

### 3.3 Raporlarda kalmış açık not

Yok. Teknik Mimari'nin cross-review'ının dokuzuncu raporu raporlarda taşınan açık notları kapalı sayar (`05_CROSS_REVIEW_R9.md` §7, madde 5): 1.–8. turların "önceki turlardan açık kalanlar" listeleri kapandı; iki tekrar eden RET (K-951, K-900) kararla kapalıdır. Güvence merceğinin iki koşumunun kalan riskleri `05`'in gövdesinde adıyla yazılıdır; uygulanmayan üç bulgusu (M1-5, M1-6, M2-13) raporda gerekçeyle kapalıdır. Audit'in uygulanmayan tek bulgusu (A7-1) çok kritik soru olarak K-951 ile kapandı. Cross-review'ın çıkış kuralıyla devrettiği sınıf §12.1'in checkpoint maddesidir (§4).

---

## 4. Süreç maddeleri ve kapıları

Tracker'ın işaretsiz maddeleri bu taramadan önce beşti, hepsi §12.1'de. Devir §11'in girdileri, `05`'in gövdesi ve hafızanın Güncel Durum bloğu ayrıca tarandı.

| Madde | Ev | Kapı | Sonuç |
|---|---|---|---|
| Checkpoint'e devredilen sınıf — iki yerin metin farkı ve güvence cümlesi ile mekanizmanın farkı (K-976) | Tracker §12.1 | Aşama 4'ün checkpoint'i | Kapı var; bu raporun §8'i girdisidir |
| Bekletilen öğrenim Ö-23 — checklist'in skill'e dönüşmesi | Tracker §12.1 · PF-32 · devir §11'in 7. satırı | Aşama 4'ün öğrenim terfisi | Kapı var |
| Öğrenim adayı 13 — tek "evet" her öneri için ayrı karar sayılmaz | Tracker §12.1 · devir §11'in 7. satırı | Aşama 4'ün öğrenim terfisi | Kapı var |
| ⚠ öneriyle kayıt listesi (K-855) | Tracker §12.1 | Arşiv işaretinden (6. adım) önce proje sahibine tek mesajda | **Kapı somutlaştı** — hazır metin (§4.1) |
| Öğrenim terfisi adayları 1–7 | Tracker §12.1 | Aşama 4 kapanışının öğrenim terfisi | Kapı var; bu taramadan 8–10 eklendi (§7) |
| **Çok kritik soru adayı — desteklenen ödeme sağlayıcısının sayısı** (K-932; `05 §3.6.4`, §2.2.2) | Bu taramaya kadar yalnız `05`'in gövdesinde | Entegrasyon spesifikasyonları aşamasının açılışı | **Kapı aldı** — `08`'in Aşama 4 park bloğuna K-932 satırı açıldı; açılış soruyu sormadan önce L1–L5'e karşı tarar |
| Sağlayıcının kaydedilemeyen bildirimi yeniden gönderip göndermediği ve kart iadesinin sonucunu bildirimle gönderip göndermediği (K-974, K-978) | `08`'in bildirimler satırı | Entegrasyon Spesifikasyonu'nun yazımı | Kapı var; iki kalan risk `05 §4.6.2` ve §10.1.3'te yazılıdır |
| Playbook listesinin gönderimi (K-649) | `PLAYBOOK_FEEDBACK.md` | Proje tamamlandıktan sonra, proje sahibi | Kapı var |
| D-01 — kurulumun ertelenen parçası | `DEFERRED_BACKLOG.md` | Aşama 4 kapanışından sonra, ilk implementation task'ından önce | Kapı var; kanıt hücresine K-910, K-932, K-937, K-965 eklendi (§6.4) |

Devir §11'in öteki girdileri kapandı: açılış sorusu K-855 ile cevaplandı; dört çok kritik soru (K-860, K-861, K-863, K-866), veri kaybı penceresi (K-888) ve 6563 m.11/3 (K-951) proje sahibince karara bağlandı; devirlerin kapanışı §12.2'nin kapanış taramasında doğrulandı.

### 4.1 ⚠ öneriyle kayıt listesi — doğrulama ve proje sahibine gösterilecek metin

**Betikle doğrulama.** Konu hücresi `(öneriyle kaydedildi — ⚠)` taşıyan Aşama 4 satırı (K-855…K-979) **41**; §12.1'in listesindeki K numarası **41** — iki küme birebir, eksik 0, fazla 0. Aşama 4'ün 125 satırının işaret dağılımı: ⚠ 41 · `(öneriyle kaydedildi)` 78 · işaretsiz 6 (K-855, K-861, K-863, K-866, K-888, K-951 — proje sahibinin kararları). `(toplu onayla kaydedildi)` yok.

| Adım | Kararlar | Sayı |
|---|---|---|
| Workshop 1. oturum | K-864 · K-874 · K-875 · K-876 · K-878 · K-879 · K-882 · K-884 | 8 |
| Workshop 2. oturum | K-893 · K-894 · K-898 · K-904 · K-905 · K-907 · K-909 | 7 |
| Workshop 3. oturum | K-915 · K-916 · K-917 · K-924 · K-925 · K-926 · K-927 | 7 |
| Yazım turu 1. oturum | K-934 | 1 |
| Audit ve deep review | K-938 · K-939 · K-940 · K-942 · K-944 · K-950 | 6 |
| Cross-review 1., 3., 5., 6. tur | K-954 · K-955 · K-956 · K-959 · K-961 | 5 |
| Güvence merceği ve 7. tur | K-962 · K-964 | 2 |
| Cross-review 8. tur | K-968 · K-969 | 2 |
| Güvence merceği, 2. koşum | K-974 | 1 |
| Çakışma taraması | K-978 · K-979 | 2 |
| **Toplam** | | **41** |

**Kapı.** Liste arşiv işaretinden (6. adım) önce proje sahibine **tek mesajda**, karar başına bir sade cümleyle ve gerekçesiz gösterilir (`INSTRUCTIONS.md` §2, "Ortak kural"); yöneticiden proje sahibine gider (§9). İtiraz gelmeyen satırın işareti `(öneriyle kaydedildi — ⚠ — gözden geçirildi YYYY-AA-GG, itiraz yok)` olur; itiraz gelen karar yeni bir satırla değişir. Checkpoint ya da öğrenim terfisi yeni bir ⚠ karar kaydederse liste arşiv işaretinden önce onunla birlikte gösterilir.

**Hazır metin — proje sahibine gösterilecek liste** (konulara göre; dokümanlar adıyla):

*Sunucu, yedek ve veri kaybı*
- K-864 — Sunucunun Türkiye'de olması bir kurulum önerisidir, ürünün kuralı değildir; yurt dışı seçilirse aktarımın şartları firmanın yükümlülüğüdür.
- K-878 — Veritabanının değişiklikleri en geç dakikada bir sunucunun dışına taşınır, tam yedek her gece alınır.
- K-879 — Yedek firmanın hesabındaki ayrı bir depoda şifreli durur ve otuz gün saklanır.
- K-939 — Sunucunun dışına taşıma dururken disk de kaybolursa kaybolabilecek süre bir dakikayı aşar; bu kalan bir risktir ve kurulumu yapana uyarı gider.
- K-961 — Sunucudan silinen yedek otuz gün içinde eski sürümlerden geri getirilir; fark edilmezse otuz günün sonunda kalıcı olarak gider.
- K-964 — Dosya yedeği silinen eski kopyaların verisini depodan da siler; bazı depolarda otuz günden eski yedek parçaları sunucuyu ele geçiren biri tarafından silinebilir.
- K-938 — Güncelleme sırasında veri bozulursa veritabanı eski hâline yalnız o andan sonra sipariş tarafında yazım yoksa döndürülür; varsa sorun ileriye doğru düzeltilir.
- K-956 — Bu denetim müşterinin iletişim talepleri dahil sipariş tarafının bütün kayıtlarına bakar; geri dönüşte firma dönüşün anıyla bilgilendirilir.
- K-962 — Veritabanının eski bir ana döndürülmesi hangi sebeple olursa olsun aynı koşula bağlıdır ve bir komutla denetlenir.
- K-955 — Eski bir ana dönüşte geri gelen hesap silmeleri bir komutla yeniden uygulanır.
- K-969 — Bir silme tamamlandığı anda sunucu çökerse ve sonra eski bir ana dönülürse o silme geri gelebilir; bu kalan bir risktir.
- K-974 — Üç kalan risk: iptal edilmiş siparişe gelen geç ödemenin bildirimi kaybolursa otomatik iade doğmaz; silmenin kaydı yazılamazsa eski bir ana dönüşte silinen kayıt geri gelebilir; bir uyarı açıkken yeni bir sorunun uyarısı ayrıca gitmez.

*Silme ve kayıtlar*
- K-875 — Saklama süresi dolan veriyi her gece tek bir iş siler ve silmeyi ayrı bir imha kaydına yazar.
- K-876 — Silme gerçek silmedir; yedeklerdeki kopyalar otuz gün içinde kendiliğinden kaybolur.
- K-915 — Sistem kayıtlarının neyi yazdığı tanımlıdır, kişisel veri maskelenir ve kayıtlar gecelik yedeğe de girer.
- K-916 — Sistem kayıtları altı ay saklanır.
- K-917 — Sistem kayıtlarını firmanın yazılı isteğiyle kurulumu yapan okur ve dosya olarak iletir.
- K-944 — Sunucunun kendi kayıtları da altı ay tutulur, yedeğe girer ve silinmeleri imha kaydına yazılır.
- K-954 — Hiçbir siparişe bağlanamayan ödeme sağlayıcısı bildirimi on yıl saklanır; kişisel veri taşımaz.
- K-979 *(bu taramada eklendi)* — Gece temizlenen ve kişisel veri taşıyan geçici kayıtların — giriş denemeleri, oturumlar, tanınan tarayıcılar — silinmesi de imha kaydına yazılır.

*Ödeme ve geri ödeme*
- K-874 — Aynı sipariş üzerinde aynı anda yapılan işlemler sıraya girer; ikincisi siparişin güncel hâline göre denetlenir.
- K-893 — Sipariş onayı, ödeme sağlayıcısının bildirimi ve son sorgu bir kez işlenir; aynı ödeme iki kez uygulanmaz.
- K-894 — Karta para gönderen geri ödeme isteği bir kez gönderilir; sonucu bilinmezse yeniden gönderilmez, sonuç sorulur.
- K-940 — Sonucu bilinmeyen kart iadesi panelde "geri ödeme sonucu bekleniyor" diye görünür; firma sağlayıcının panelinde gerçekleşmediğini görürse "gerçekleşmedi" diye işaretleyebilir.
- K-959 — Ödeme sağlayıcısının kart iadesinin sonucunu sorgulama imkânı sunması şarttır; sunmayan sağlayıcıyla kartla ödeme kurulmaz.
- K-978 *(bu taramada eklendi)* — Firma iadeyi "gerçekleşmedi" diye işaretledikten sonra iade sonradan gerçekleşirse ürün bunu yalnız ödeme sağlayıcısı bildirirse görür; bildirmezse çift ödeme yalnız sağlayıcının panelinde görünür.
- K-898 — Site kesintisi sitenin otuz saniyede bir yazdığı işaretle ölçülür; kesintide dolan havale ödeme süresinin iptali ertelenir.
- K-934 — Tek bir tutarda yarım kuruş yukarı yuvarlanır; parçalara bölünen tutarda aşağı yuvarlanır ve kalan kuruş en büyük parçaya eklenir.

*Hesap, giriş ve çerezler*
- K-904 — E-postadaki bağlantının açılması tek başına bir işlem yapmaz; doğrulama ve geri alma açılan ekrandaki düğmeyle olur.
- K-968 — Google ile yeniden doğrulama yalnız o anda yapılmış taze bir Google girişini kabul eder; Google'da oturumu açık olan kullanıcı Google'dan çıkıp yeniden girer ya da şifre belirler.
- K-907 — Ödeme adımında yazılanlar yalnız o tarayıcıda, oturumluk şifreli bir çerezde tutulur.
- K-942 — Google ile girişin geçici çerezi zorunlu çerezler listesine girer.
- K-950 — Misafir sepetinin kalıcı çerezinin hukuki dayanağı satış sözleşmesinin kurulmasıdır.
- K-909 — Site yalnız HTTPS ile çalışır; şifreler ve bağlantı anahtarları özet olarak saklanır, veritabanındaki alanlar ayrıca şifrelenmez.

*Dosyalar*
- K-882 — Yüklenen görsellerin konum gibi gizli bilgileri silinir.
- K-884 — Dijital ürünün indirme hakkı müşteri indirme düğmesine basıp indirme başladığında bir kez düşer; yarıda kalan indirmeyi sürdürmek yeni hak harcamaz.
- K-905 — Yüklenen görselin türü içeriğinden okunur ve aşırı büyük görsel reddedilir; dijital ürün dosyasında tür sınırı yoktur.

*Güncelleme ve bakım*
- K-924 — Güncellemeyi kurulumu yapan tek bir komutla başlatır; kurulumlar kendiliğinden güncellenmez.
- K-925 — Veritabanı değişiklikleri geri alınabilir yazılır; yedekten dönüş yalnız site yeni sürümle trafiğe açılmadan önce yapılır.
- K-926 — İşletim sistemi güvenlik güncellemelerini kendiliğinden alır ve gerekirse gece yeniden başlar; uygulama aylık bakım sürümüyle güncellenir.
- K-927 — Sunucuya yalnız SSH ve kurulum başına ayrı bir anahtarla girilir; dışarıya yalnız web ve SSH kapıları açıktır.

---

## 5. Çakışmalar

### 5.1 Karar isteyen çakışmalar — K-977, K-978 ⚠, K-979 ⚠

| Yer | Çakışma | Karar |
|---|---|---|
| `06`, `07`, `09`'un K-17 park satırları · `02 §1.1` ↔ `05` konvansiyon 4 ve Aşama 4'ün kayıt kararları (mercek M1, bulgu M1-2 — orta) | Sözlük kuralı "`06`, `07` ve `09` bu sözlüğü birebir devralır, isim icat etmez" der ve tek istisnası `02`'nin kapalı liste değerleridir (K-531). Teknik Mimari kod adı yazmaz (K-930) ve on üç teknik kavramı — iş kuyruğu, gönderim kaydı, gelen bildirim kaydı, geri ödeme kaydı, kanal hâli, nabız, kesinti kaydı, deneme kaydı, bağlantı kaydı, onay anahtarı, imha kaydı, geçiş kaydı, sürüm değeri — ve dört kapalı listeyi `06`'ya devretti. Kuralın sözüyle `06` ya onları adsız bırakır ya da kuralı çiğner | **K-977 (öneriyle kaydedildi):** sözlük ürünün terimlerini taşır; teknik kavramların ve kapalı listelerinin kod adlarını `06` kavramın `05`'teki adından birebir verir, `07` ve `09` `06`'dan alır; ürüne görünen kavram sözlüğe girer. Elenen: teknik kavramların `02 §1.2`'ye girmesi (ürün dokümanı ürün dışı terimle dolar) · adı `05`'in vermesi (konvansiyon 4) |
| `05 §4.6.2`, MK-40 ↔ K-940, K-970 (4), ÖK-4 (mercek Mc, bulgu Mc-10 — öncül) | "Ürünün payı geri ödemenin satırındaki kalıcı uyarıdır" cümlesi uyarının her sırada doğduğunu varsayıyordu. Firmanın işareti sorguyu durdurduğu için iadenin geç sonucu ürüne yalnız sağlayıcının bildirimiyle ulaşır; sağlayıcıdan beklenen yetenekler iade sonucunu **sorgulamayı** sayar, **bildirmeyi** saymaz. Bildirim kaydedilemez ve yeniden gönderilmezse ya da hiç gönderilmezse çift ödeme ürüne görünmez | **K-978 (öneriyle kaydedildi — ⚠):** kalan risk olarak yazılır (`05 §4.6.2`, §10.1.3, MK-40); `08` bildirimin varlığını ve yeniden gönderimini doğrular. Elenen: işaretten sonra sorgunun seyrek sürmesi (K-940'ı ve `02 §7.2.8`'i değiştirir) · ÖK-4'e ek yetenek (sağlayıcı seçimini daraltır, risk kalır). Çok kritik değil: bugünkü kural değişmez, güvencenin sınırı yazılır |
| `05 §5.2.9`, §7.4.4 ↔ K-944, `05 §9.2.2`, `02 §12.2.7` (mercek Mc, bulgu Mc-11 — öncül) | K-944 "imhaya ilişkin bütün işlemler kayda alınır" ilkesiyle sunucunun kayıtlarının döngüyle silinmesini bile imha kaydına yazdı; gecelik temizlik ise IP ve e-posta taşıyan deneme kayıtlarını, oturum ve tanınan tarayıcı kayıtlarını imha kaydına yazmadan siliyordu — ayrım gerekçelendirilmemişti | **K-979 (öneriyle kaydedildi — ⚠):** kişisel veri taşıyan geçici kayıtların silinmesi imha kaydına kayıt türü ve sayıyla yazılır; kişisel veri taşımayanlar ve istek içinde sayımın parçası olarak silinen deneme satırları yazılmaz. Elenen: deneme kaydını ilkenin dışında tutmak (yönetmelik silinen verinin türüne bakar) · bütün geçici kayıtları yazmak (gürültü). Çok kritik değil: bedelsiz ve geri alınabilir |

**Kuralın yüzleri (betik 1) — K-977:** terimler "isim icat", "birebir devral", "kod karşılığ", "kod adı" — `02 §1.1` (güncellendi), `06`, `07`, `09`'un K-17 satırları (güncellendi); `02 §1.2`'nin sözlük satırları ürün terimleridir, değişmez; `05` konvansiyon 4 değişmez. **K-978:** "kalıcı uyarı", "geç gelen başarılı", "işaret sorguyu durdurur" — `05 §4.6.2`, §10.1.3, MK-40 ve `08`'in park satırı güncellendi; `02 §7.2.8` uyarıyı "bilgisi gelirse" koşuluyla yazar, değişmez; `06`'nın park satırı alanı taşır, uyarının kaynağını değil. **K-979:** "geçici kayıtların temizliği", "imha kaydı", "deneme kayd" — `05 §5.2.9`, §7.4.2, §7.4.4 ve `06`'nın imha satırı güncellendi; `02 §12.2.7` "her imha çalışması … imha kaydına yazılır" der ve geceyi kapsar, Z-44'ün tanımı "her periyodik imha çalışmasının" sayısını taşır — ikisi de değişmez. **Betik 2:** gövdede anılan K-977 `02 §1`'in, K-978 `05 §4.6`'nın ve §10.1'in, K-979 `05 §5.2`'nin ve §7.4'ün Kaynak satırındadır.

### 5.2 Hizalanan çakışmalar — geri beslemenin yan yüzleri ve `05`'in iç tutarlılığı

Hepsi var olan bir kararın kuralının değişmeden kaldığı yüzlere taşınmasıdır; yeni karar değildir. Üst dokümanların ✓ durumu korunur, kalite döngüsü yeniden açılmaz (K-652). Üst dokümana dönenler `05`'in başlık notunun geri besleme tablosunda satırdır.

| # | Yer | Eski hâl | Karar | Düzeltme | Şiddet |
|---|---|---|---|---|---|
| Md-1 | `02 §10.4.6` ve özetleri §5.4, §6.7 | Yanlış iptalin düzeltilmesinin koşulu kart iadesinin sonucu beklenirken düzeltmeyi yasaklamıyordu; `03` 1.6.2.2 ve `04`'ün başlık notu yasağı bu maddeye dayandırıyordu | K-940 | Sonuç sağlayıcıdan beklenirken düzeltme yapılamaz — iade gerçekleşmiş olabilir | orta |
| Md-2 | `10 §2` KP-50 | "İade ve geri ödeme bekleyen" sayacı yalnız başarısız kart iadesini sayıyordu | K-940 | Sonucu sağlayıcıdan beklenen kart iadesi de sayılır (`02 §10.6.1`, `03` 8.9.1 ile aynı) | düşük |
| Md-3 | `03` 9.1.5 · `04 §1.1`'in `03 §9.1.5` satırı | Doğrulanmamış kaydın silinmesinin koşulu "bağlantıyı açmaz" | K-904 | Hesabı doğrulamaz — bağlantıyı açmaz ya da açılan ekrandaki düğmeye basmaz | düşük |
| Md-4 | `05 §7.4.5`, §10.3.7 · `DEPLOY_RUNBOOK`'un yedek satırı | Firmanın iletişim talebini elle sildiği varsayılıyordu; `04` 9.11.7 bu işlemi "yoktur" diye sayar | K-955 (öncül) | Anlık silmeler hesabın ve üye hesabının silinmesidir; yeni kayıt açan formun ikinci talebi elle silinmez, süresinde imha edilir | düşük |
| M2-12 | `05 §7.4.2` | İmha kaydına "o günün satırının sayısına ekleme" yolu açıktı — aynı maddenin "satırı güncellemez" kuralıyla çelişiyordu | K-975 | Her parça kendi satırını yazar, var olan satırın sayısını güncellemez | düşük |

**Geri işaret ve etki sütunu:** K-940'ın ve K-904'ün etki sütunlarına yansıma `(taslak güncellendi — vX.Y)` ile, K-955'in ve K-975'in etki sütunlarına hizalama notu eklendi. **Betik 1 ve 2:** karar kaydının §12.4'ündedir — terim listeleri, kuralın evinin Kaynak hücrelerinden geri izlenmesi ve gerekçeyle elenenler.

### 5.3 Park satırları

**(a) Aşama 1, 2 ve 3'ün park satırları — numaraya dayalı tarama (mercek M1).** 66 satır, 61 kaynak; K-851…K-976'daki **56 eşleşmenin hepsi okundu** (beşi `MK-nn` kimliğinin kuyruğudur). Numara taşımayan on dört ilişki ve `05`'in tüketilmiş üç bloğunun on notu ayrıca okundu — notların hepsi doğru bölüme gider.

| # | Park satırı | Değiştiren karar | Hizasızlık | Düzeltme | Geri işaret |
|---|---|---|---|---|---|
| M1-1 | `DEPLOY_RUNBOOK` — K-736 | K-923, K-971 | "Adın nasıl verildiği bu dokümanın kararıdır" — `05 §8.5.1`, §8.5.2 adı ve e-postayı komutun etkileşimli girişine bağladı, parametre yolu elendi (orta) | Ad e-postayla birlikte etkileşimli girişten okunur; §H'nin adımları runbook'un kararıdır | K-736'ya eklendi |
| M1-2 | `06`, `07`, `09` — K-17 (· K-531) | — | Teknik kavramların kod adının evi yok (orta) | **K-977** (§5.1) | K-17, K-531'e eklendi |
| M1-3 | `08` — K-705 | K-894, K-893, K-941 | (1)'in "iade isteğinin tek işlenmesi bu aşamanın kararıdır"ı — ilke `05 §4.6`'ya taşındı; Aşama 4 satırı yalnız işaret cümlesini kapatıyordu | Aşama 4 satırına K-705'in iki kolunun okunuşu yazıldı | K-705'e eklendi |
| M1-4 | `06` — K-787 · K-790 · K-793 · K-794 | K-874, K-889 | "Eşzamanlı iki işlemin nasıl engellendiği bu aşamanın kararıdır" — mekanizma `05 §4.1.2`, §7.3.2'de | Satır kilidiyle sıraya girer, koşul güncel açık adede karşı denetlenir | K-787, K-874, K-889'a eklendi |

**Değişiklik gerekmeyen adaylar:** K-10 ← K-864, K-871, K-929 · K-12 ← K-862, K-864, K-909, K-919 · K-23 ← K-919 · K-666, K-667 ← K-875, K-898 · K-687 ← K-917 · K-699 ← K-923 · K-745 ← K-893, K-914 · K-758 ← K-907 · K-759 ← K-892 · K-760 ← K-913, K-918 · K-800 ← K-873 · K-801 ← K-885 · K-804 ← K-892, K-951 · K-834, K-850 ← K-908 · K-839 ← K-904, K-971 · K-852 ve K-854'ün eşleşmeleri liste ya da atıftır. `05`'in Aşama 3 dizinindeki "genişlik sınıflarının sınırlarının uygulanması" `05 §3.8.2`'de karşılandı.

**(b) Aşama 4'ün park satırları (mercekler M2, M3a, M3b).** 54 satır; mercek başına toplam 151 kaynak ve **513 eşleşme** — hepsi okundu; her satırın `05 §…` atıfları `05` v0.14'ün metnine karşı okundu; sayısal değerlerin hepsi `05` ile birebir (otuz gün, elli saniye, bir dakika, 03:00, 04:30, on dakika, beş dakika, yirmi altı saat, altı ay, 2 sanal işlemci / 4 GB / 60 GB, 10–20 $). Kalite döngüsü her kararında park satırını "güncellendi" diye işaretlemişti; bulgular kararın satıra taşınmamış kollarıdır.

| # | Park satırı | Kaynak karar | Taşınmamış yüz | Şiddet |
|---|---|---|---|---|
| M2-1 | `06` — iş kuyruğu, gönderim kaydı, gelen bildirim ve geri ödeme kaydı | K-970 (4) | İşaret anında yanıtı beklenen sorgunun başarılı sonucu geri ödeme kaydında işaretli kalır — satır yalnız gelen bildirim kaydını sayıyordu | orta |
| M2-2 | `12` — eşzamanlılık, tek işlenme ve kesinti (4) | K-975 (2) | Kesintinin ilk iki dakikasına düşen son an ertelenmez (kalan risk); yalnız ters vekil durduğunda öteki işler sürer | düşük |
| M2-3 | `12` — aynı satır (2), (3) | K-970 | Onay anahtarı ilk adımda; iptal edilmiş siparişte tek kart iadesi; aynı işlemin ayrı olayları | düşük |
| M2-4 | `07` — hata yanıtları ve sağlık uçları | K-974 (1) | "Gönderilmezse sonucu son sorgu bulur" Bekliyor siparişe daralır; iptal edilmiş siparişin geç ödemesi kalan risktir | düşük |
| M2-5 | `07` — formlar, kimlik şemaları ve erişim | K-970 (2) | Tekrar eden bildirim kaydedilmez, işlenmez, yine "alındı" alır | düşük |
| M2-6 | `12` — uyarılar ve hata hâlleri (4) | K-975 (8) | Okunamayan koşul açık sayılır, uç 503 döner | düşük |
| M2-7 | `12` — bağlantılar ve çerezler (1) | K-971 (6) | Açılış bağlantının, hesabın ve siparişin kaydını değiştirmez; oturumun son işlem anı, form çerezi, hız sayacı ve erişim kaydı dışında | düşük |
| M2-8 | `06` — kanal hâli ve yedeğin durum kaydı | K-963 | Durum kaydı sunucu dışı tam yedeğe bakar — yerel kopyanınki sayılmaz | düşük |
| M2-9 | `12` — on yıllık saklama (2) | K-951 | "Adres boşalır" → alıcı ve olayın değerleri boşalır | düşük |
| M2-10 | `06` — kanal hâli ve yedeğin durum kaydı | — | Gövdede etikette olmayan K-910 — içerik K-937'nin; atıf düştü | düşük |
| M2-11 | `07` — sözleşmelerin biçimi | MK-04 | Makine uçları listesinde canlılık ve durum uçları; yeni imzalı uç karar kaydıyla girer | düşük |
| M2-13 | `07` — formlar | K-971 (3) | Yeniden doğrulama formunun tek kullanımlık değeri | düşük |
| M3a-1 | `08` — Google ile giriş | K-973 (4) | Akış değerleri çerezde ya da korumalı `state`'te; çerezin yolu kök | düşük |
| M3a-2 | `08` — bildirimler ve ödeme sağlayıcısı | K-970 (4) | Kart iadesi sorgusu sonuç ya da işarete kadar yinelenir; işaret anındaki sorgunun sonucu | düşük |
| M3a-3 | `08` — e-posta gönderim altyapısı | — | H-2'nin kaynağı (`02 §11.3`) | düşük |
| M3a-4 | `09` — veritabanı imajı ve yedek araçları | K-964 | "restic'te budamanın kullanılmayan veri payı sıfırdır" araç özelliği gibi yazılıydı; `05 §7.6.6` varsayılanın veri bıraktığını ve ayarın `09`'un işi olduğunu söyler — varsayılanla bırakılırsa silinen veri otuz günden sonra da depoda kalırdı | orta |
| M3a-5 | `09` — ayarlar ve yönetim komutları | K-971 (7) | Komutlar kişisel veriyi etkileşimli girişten okur | düşük |
| M3a-6 | `09` — form ve site güvenliği | K-935 | Tek kullanımlık değer yalnız bloğu kapsar; renk öğenin stil niteliğiyle yazılmaz | düşük |
| M3a-7 | `09` — görüntü işleme kütüphanesi | K-905, K-973 | Görev listesinde piksel tavanı ve ilk kare sınırının kütüphanede uygulanışı | düşük |
| M3a-8 | `08` — Aşama 4 bloğu | K-932 | Bağlantı katmanı, bağlantı hatasının ayırt edilmesi ve desteklenen ödeme sağlayıcısının sayısı — **çok kritik soru adayı** — hiçbir park satırında yoktu | orta |
| M3b-1 | `DEPLOY_RUNBOOK` — yedek, kurtarma ve prova | K-966, K-974 (3) | Geride kalan aktarım da durmuş sayılır; açık bir koşul yeni uyarıyı örter | düşük |
| M3b-2 | aynı satır | K-937 | Yedek anahtarı kovanın ayarlarını okuyabilir — provanın kural denetimi buna dayanır | düşük |
| M3b-3 | aynı satır | K-974 (2) | Anlık silmenin istisnası satırın yazım hatasını da kapsar | düşük |
| M3b-4 | aynı satır | K-944 | Gövde K-944'ü taşıyor, etikette yoktu | düşük |
| M3b-5 | `DEPLOY_RUNBOOK` — sunucunun seçimi | K-963 | Provanın nesne depolamadan çıkış trafiği seçim ölçütünde yoktu | düşük |
| M3b-6 | `DEPLOY_RUNBOOK` — kurulumun sırası | K-972 (3) | Güven ters vekilin iç ağıyla tanımlanır | düşük |
| M3b-7 | `DEPLOY_RUNBOOK` — kurulumun yapısı ve ilk kayıtlar | K-971 (7) | Ad ve e-posta etkileşimli girişten | düşük |
| M3b-8 | `DEPLOY_RUNBOOK` — sırlar | K-971, `05 §6.x.18` | Salt okuma parolasının alıcıları dönüş denetimini saymıyordu | düşük |
| M3b-9 | `DEPLOY_RUNBOOK` — veritabanının inceleme araçları | K-927 | "Erişim yolu bu dokümanın kararıdır" — `05 §8.9.5` salt okuma kullanıcısını bağladı | düşük |
| M3b-10 | `DEPLOY_RUNBOOK` — sistem kayıtları | K-915 | Veritabanının kaydı sorgu parametrelerini yazmaz | düşük |
| M3b-11 | `DEPLOY_RUNBOOK` — gider kalemleri | K-861 | Gövde tavanın değerini taşıyor, etikette K-861 yoktu | düşük |
| M3b-12 | `DEPLOY_RUNBOOK` — tepe dakikanın okunması | K-929 | Gövde büyüme yolunun tetikleyicilerini taşıyor, etikette K-929 yoktu | düşük |

**(c) Park satırı ↔ etki sütunu (iki yönlü betik).** Düzeltmeden önce: K-900 Entegrasyon Spesifikasyonu'nun Google satırının etiketindeydi ama etki sütunu `08`'i saymıyordu (cross-review 9. turunun Faz 5'i satırı "park gerekmez" yanlış pozitifi sanmıştı) — eklendi. Düzeltmelerle on yedi karar–hedef çifti etki sütunlarına girdi (§6.1'de "bu taramada eklendi"): K-861, K-929 → `DEPLOY_RUNBOOK` · K-900, K-932, K-978 → `08` · K-963, K-979 → `06` · K-970 → `07`, `12` · K-971 → `07`, `09`, `12` · K-974 → `07` · K-975 → `12` · K-977 → `06`, `07`, `09`. Son koşum: etiketteki her kararın etki sütunu hedefi taşır; ters yönde Aşama 4 bloklarında karşılığı olmayan dokuz çiftin altısı etki sütununda "park gerekmez" der (K-911 → `08`, K-932 → `07`, K-933 → `07` ve `12`, K-934 → `12`, K-969 → `09`), K-977'nin üç hedefi Aşama 1 bloklarının K-17 satırlarıdır.

### 5.4 Geri işaret (mercek Mc)

632 çiftin hepsi okundu; on iki daraltma zincirinde `05` her yerde zincirin son hâlini yazıyor — madde, §0 kısıt satırı, §1.2 bileşen satırı ve §12 özet satırı. Dokuz eksik geri işaret eski satırların etki sütunlarına eklendi (Aşama 1 satırı K-351 dahil — salt okunur kaydın tek istisnası):

| # | Eski satır | Sonraki karar | İlişki |
|---|---|---|---|
| Mc-1 | K-876 · K-962 | K-969, K-974 | K-876'nın etki sütunu K-955 için "kalan risk değildir" diyordu; iki karar istisnayı kalan risk yazdı |
| Mc-2 | K-925 | K-962 | (4)'ün "yalnız bir göç bozduğunda" koşulu her sebebe genişledi |
| Mc-3 | K-915 | K-937, K-961, K-964 | "Ele geçirilse de silmeye karşı korunan yedekte durur" otuz günle sınırlandı |
| Mc-4 | K-911 | K-937 | Tek yedek anahtarı iki şifreleme anahtarına bölündü |
| Mc-5 | K-906 | K-942 | Çerez envanterine Google akış çerezi girdi |
| Mc-6 | K-351 | K-906 | Sepet çerezinin "tarayıcı verisi silinene kadar" ömrü 400 güne bağlandı |
| Mc-7 | K-923 | K-946, K-955, K-962 | Komut listesine üç komut girdi |
| Mc-8 | K-875 | K-944, K-979 | İmha kaydının "döngüyle silindi" satırı ve gecelik temizliğin satırları |
| Mc-9 | K-897 | K-922 | Gecelik saatler "ürün sabiti" değil Teknik Mimari'nin tasarım değeridir (terim) |

### 5.5 Betikle aranan ve temiz dönen çakışma sınıfları

| # | Sınıf | Sonuç |
|---|---|---|
| 1 | **⚠ listesi ↔ kayıt** | ✓ — 41 / 41 |
| 2 | **K numarası tekilliği** — çift K taraması | ✓ — boş; K-01…K-979 |
| 3 | **Karar satırının biçimi** — altı sütun | ✓ — 979 satırın hepsi |
| 4 | **Sayısal tasarım değerleri** — `DEPLOY_RUNBOOK` park satırları ↔ `05` | ✓ — birebir (M3b) |
| 5 | **`05`'in tüketilmiş park bloklarının "karara bağlandı" notları** | ✓ — on notun hepsi doğru K ve bölüm |
| 6 | **Daraltma zincirlerinde `05`'in son hâli** | ✓ — on iki zincir |
| 7 | **Geri beslenen on yedi kuralın yüzleri** | On beşi temiz; ikisinin (K-940, K-904) üç yüzü §5.2'de düzeldi (Md-1, Md-2, Md-3) |
| 8 | **Gövdedeki yeni K ↔ Kaynak satırı (betik 2)** | ✓ — `05` ve `02`'de K-977…K-979 eksiksiz; `02 §5.4`'e K-940 eklendi |
| 9 | **Sürümler** — tracker §1, başlıklar, dosya sonu notları | ✓ — taramadan önce `05` v0.14, `02` v0.74, `03` v0.26, `04` v0.26, `10` v0.49 birebir; bu PR'da birlikte arttı |

---

## 6. Devir dizini — Aşama 4 kararlarının sonraki dokümanlara devirleri

**Tek ev:** devrin evi karar satırının etki sütunu ya da park satırıdır; bu dizin devri ikinci kez kaydetmez. **Aşama 4'te yalnız numara taşıyan atıf yoktur** — Aşama 2'nin öğrenim terfisinin kuralı (checklist §3, "işin adı yazılır") ikinci aşamada da işledi. Her hedef dokümanın "Aşama 4'ten park edilen girdiler" bloğu bu bölüme işaret eden bir dizin satırı taşır (bu taramada eklendi). **Veri modeli aşamasının açılışı** devir taramasını (checklist §4) buradan ve önceki aşamaların dizinlerinden başlatır.

- **Önceki aşamaların dizinleri:** `PHASE1_CONFLICT_SCAN.md` §6, `PHASE2_CONFLICT_SCAN.md` §6, `PHASE3_CONFLICT_SCAN.md` §6.
- **`11` Uygulama Planı:** Aşama 4'ten devir yok — `11` tüketicidir ve `05`'i kendi matrisinde okur; `05`'te `11`'e iş bırakan cümle yoktur.
- **`DEFERRED_BACKLOG.md`:** Aşama 4'ten yeni kalem yok; D-01'in kanıtı tamamlandı (§6.4).
- **Süreç kararları** (K-855…K-859, K-930, K-931, K-952, K-953, K-976) sonraki dokümana iş bırakmaz.

### 6.1 Etki sütunlarındaki devirler (K-855…K-979)

| Hedef | Atıf | Karar | Park satırında | Park gerekmez |
|---|---|---|---|---|
| `06` Veri Modeli | 41 | 39 | 39 | 0 |
| `07` API Tasarımı | 20 | 20 | 18 | 2 (K-932, K-933) |
| `08` Entegrasyon Spesifikasyonu | 21 | 20 | 19 | 1 (K-911) |
| `09` Kodlama Kılavuzu | 44 | 44 | 43 | 1 (K-969) |
| `12` Doğrulama Protokolü | 20 | 20 | 18 | 2 (K-933, K-934) |
| `DEPLOY_RUNBOOK.md` | 58 | 54 | 54 | 0 |
| **Toplam** | **204** | **111 karar** (197 karar–doküman çifti) | **191** | **6** |

İşin adı etki sütunundan alınmıştır; "park satırı" ve tarih ekleri kısaltıldı. *Bu taramada eklendi* işaretli çiftler §5.3 (c)'dedir.

#### `06` Veri Modeli — 39 karar

| Karar | İş | Park satırında |
|---|---|---|
| K-870 | zaman, tarih ve para alanlarının türleri | ✓ |
| K-873 | düzenlenebilir kayıtların sürüm alanı; ürünün sürümünün varyantları kapsaması; K-800'ün Aşama 3 park satırı da orada | ✓ |
| K-874 | siparişin sürüm alanı ve sürümü artıran değişiklikler · aynı adedi kullanan eşzamanlı iki işlem satır kilidiyle sıraya girer — Aşama 3'ün K-787 park satırı hizalandı | ✓ |
| K-875 | imha kaydının tablosu; sürelerin başlangıç değerleri | ✓ |
| K-876 | fiziksel silme; üretilen belgelerin saklanma biçimi ve imha kapsamı | ✓ |
| K-877 | iz tablosunun yetkileri ve imha fonksiyonu | ✓ |
| K-883 | dosya kaydı; varyanta ve kaleme bağı | ✓ |
| K-884 | kalemin indirme sayacı ve indirme bağlantısı | ✓ |
| K-886 | ürünün arama biçimi alanı | ✓ |
| K-889 | geçiş kaydı · aynı adedi kullanan eşzamanlı iki işlemin sırası — Aşama 3'ün K-787 park satırı hizalandı | ✓ |
| K-890 | ayrılmış adet alanları ve koşullu güncelleme | ✓ |
| K-891 | iş kuyruğu tablosu | ✓ |
| K-892 | gönderim kaydının alanları | ✓ |
| K-893 | onay anahtarı; gelen bildirim kaydı ve işlem kimliğinin tekilliği | ✓ |
| K-894 | geri ödeme kaydının hâlleri ve istek kimliği | ✓ |
| K-895 | kayıtlardaki zaman alanları | ✓ |
| K-898 | nabız ve kesinti kaydı; ertelenen son an | ✓ |
| K-899 | oturum kaydı; iki hesap türünün tabloları | ✓ |
| K-901 | deneme kaydı; sepetin birleşmesinde sayaç | ✓ |
| K-903 | bağlantı kaydı; erişim anahtarı ve numaranın tekilliği | ✓ |
| K-906 | tanınan tarayıcı kaydı; misafir sepetinin son erişim anı | ✓ |
| K-913 | kanal hâli kaydı; gönderim denemesinin sonuç sınıfı | ✓ |
| K-918 | yedek aracının durum kaydı | ✓ |
| K-937 | yedeğin durum kaydı | ✓ |
| K-940 | geri ödeme kaydının hâlleri | ✓ |
| K-941 | gelen bildirim kaydı, uç, imzalı makine uçları | ✓ |
| K-944 | imha | ✓ |
| K-945 | oturum ve bağlantı kayıtları | ✓ |
| K-946 | iş kuyruğu, zaman, uyarılar | ✓ |
| K-948 | dosya kaydı | ✓ |
| K-951 | gönderim kaydının iki kolu, siparişin ödemesi alınmış sayılmasının okunduğu alan, boşaltılan alanlar, talebin tipe göre süresi | ✓ |
| K-954 | gelen bildirim kaydının saklanması | ✓ |
| K-963 | yedeğin durum kaydı — sunucu dışı tam yedek — *bu taramada eklendi* | ✓ |
| K-970 | iş kuyruğu, gönderim kaydı, gelen bildirim ve geri ödeme kaydı | ✓ |
| K-971 | oturum, deneme, bağlantı ve tanınan tarayıcı kayıtları | ✓ |
| K-972 | oturum, deneme, bağlantı ve tanınan tarayıcı kayıtları | ✓ |
| K-975 | imha | ✓ |
| K-977 | K-17 park satırları — Aşama 1 blokları hizalandı — *bu taramada eklendi* | ✓ |
| K-979 | imha — *bu taramada eklendi* | ✓ |

#### `07` API Tasarımı — 20 karar

| Karar | İş | Park satırında |
|---|---|---|
| K-868 | sözleşmelerin biçimi: sayfa ve form uçları, makine uçları | ✓ |
| K-873 | düzenleme formlarının sürümü taşıması | ✓ |
| K-874 | sipariş işlemlerinin sürümü taşıması; "sipariş değişti" yanıtı | ✓ |
| K-883 | parçalı yükleme ucu | ✓ |
| K-884 | indirme ucu | ✓ |
| K-893 | ödeme adımı formunun onay anahtarı; bildirim ucunun hemen "alındı" demesi | ✓ |
| K-899 | iki kimlik şeması ve çerez adları | ✓ |
| K-902 | uç başına gövde tavanı ve 429 yanıtı | ✓ |
| K-904 | doğrulama adımının POST ucu | ✓ |
| K-912 | sayfaların erişim kuralı ve "yapılabilir işlemler" | ✓ |
| K-914 | sayfa ve form uçlarının hata yanıtı; makine uçlarında 500 | ✓ |
| K-918 | canlılık ve durum uçları | ✓ |
| K-932 | panelin yol alanının adı — park gerekmez: `05 §3.5.1` işaret eder ve adreslerin önekleri `02 §3.30.1` gereği zaten `07`'nindir | park gerekmez |
| K-933 | site haritası ve robots.txt uçları — park gerekmez: K-868'in park satırı site haritasını makine ucu olarak zaten taşır | park gerekmez |
| K-941 | gelen bildirim kaydı, uç, imzalı makine uçları | ✓ |
| K-947 | gövde tavanı ve hız sınırı | ✓ |
| K-970 | tekrar eden bildirimin yanıtı — *bu taramada eklendi* | ✓ |
| K-971 | yeniden doğrulama formunun tek kullanımlık değeri — *bu taramada eklendi* | ✓ |
| K-974 | makine ucunun 500'ü — *bu taramada eklendi* | ✓ |
| K-977 | K-17 park satırları — Aşama 1 blokları hizalandı — *bu taramada eklendi* | ✓ |

#### `08` Entegrasyon Spesifikasyonu — 20 karar

| Karar | İş | Park satırında |
|---|---|---|
| K-862 | e-posta gönderim altyapısının tavanın içinde seçilmesi | ✓ |
| K-892 | gövdenin gönderim anında üretilmesi ve şablonun değerleri | ✓ |
| K-893 | bildirimin doğrulanması ve gelen kutusu | ✓ |
| K-894 | iade isteğinin anahtarı ve sonucun sorgulanması; Aşama 2'nin K-705 satırı bu ilkeyle okunur | ✓ |
| K-897 | son sorgunun aralığı ve sağlayıcının kotası | ✓ |
| K-900 | Google ile girişin akışı ve yeniden doğrulamanın tazeliği; K-942 ve K-968 ile aynı satır — *bu taramada eklendi* | ✓ |
| K-906 | ödeme sağlayıcısından dönüşün oturum çerezine dayanmaması | ✓ |
| K-911 | sağlayıcı anahtarının değişmesinin açık siparişlere etkisi — Aşama 2'nin devri; değişmez, park gerekmez | park gerekmez |
| K-913 | sağlayıcı yanıtlarının sonuç sınıflarına eşlenmesi; geri dönme bildirimi | ✓ |
| K-932 | bağlantı katmanı, bağlantı hatasının ayırt edilmesi ve desteklenen ödeme sağlayıcısının sayısı — Entegrasyon spesifikasyonları aşamasının açılışında çok kritik soru adayı — *bu taramada eklendi* | ✓ |
| K-935 | ödeme sağlayıcısının çerçevesinin ve yönlendirmesinin içerik güvenlik politikası izinleri | ✓ |
| K-940 | sorgunun biçimi | ✓ |
| K-941 | gelen bildirim kaydı, uç, imzalı makine uçları | ✓ |
| K-942 | Google ile giriş — yeni park satırı; içerik güvenlik politikası | ✓ |
| K-959 | kart iadesinin sonucunun sorgulanması | ✓ |
| K-968 | Google ile giriş | ✓ |
| K-970 | bildirimler ve ödeme sağlayıcısı | ✓ |
| K-973 | ödeme sağlayıcısından dönüş · Google akış değerlerinin yeri | ✓ |
| K-974 | bildirimler ve ödeme sağlayıcısı | ✓ |
| K-978 | bildirimler ve ödeme sağlayıcısı — iade sonucunun bildirimi — *bu taramada eklendi* | ✓ |

#### `09` Kodlama Kılavuzu — 44 karar

| Karar | İş | Park satırında |
|---|---|---|
| K-866 | C# kod standartları | ✓ |
| K-867 | kütüphane ekleme ölçütü; EF Core'un kullanımı | ✓ |
| K-868 | Razor Pages düzeni | ✓ |
| K-869 | JavaScript ve CSS düzeni | ✓ |
| K-871 | modül sınırlarının kodda korunması | ✓ |
| K-881 | dosya deposu arayüzünün kullanımı | ✓ |
| K-882 | görüntü işleme kütüphanesinin seçimi ve lisans ölçütü | ✓ |
| K-887 | önbelleğin geçersiz kılınması | ✓ |
| K-889 | geçiş biriminin tek yazıcı olmasının kodda korunması | ✓ |
| K-891 | iş kuyruğunun kalıbı ve işleyicilerin tekrar güvenliği | ✓ |
| K-895 | zamanın tek kaynaktan okunması ve sınanabilirliği | ✓ |
| K-899 | kimlik bileşenlerinin kullanımı | ✓ |
| K-900 | kimlik bileşenlerinin kullanımı | ✓ |
| K-901 | istemci IP'sinin ters vekilden okunması | ✓ |
| K-903 | rastgele üretim ve anahtarların kayıtlarda maskelenmesi | ✓ |
| K-909 | kayıtlarda maskelenecek alanlar | ✓ |
| K-912 | varsayılan kapalı yetki politikası ve kayıt düzeyinde süzme | ✓ |
| K-914 | tek hata yakalama katmanı; sonucu belirsiz işlemlerin kodda işaretlenmesi | ✓ |
| K-915 | kayıt kütüphanesi, yapılandırılmış alanlar, maskelemenin kodda uygulanması, istek kimliği | ✓ |
| K-919 | geliştirme ortamının Compose varyantı | ✓ |
| K-920 | Dockerfile ve göç demetinin üretimi | ✓ |
| K-921 | test projelerinin ayrımı; mimari testler | ✓ |
| K-922 | ayar sınıflarının açılışta doğrulanması; ürün sabitlerinin tek yeri | ✓ |
| K-923 | yönetim komutlarının yeri ve sınanması | ✓ |
| K-925 | genişlet–daralt kuralı; yama sürümünün göç taşımaması; büyük veri dönüşümünün arka plan işiyle yapılması | ✓ |
| K-932 | modül listesi ve iletişim kuralının kodda korunması; K-871'in satırı genişletildi | ✓ |
| K-934 | tek yuvarlama işlevi ve yönün açıkça yazılması | ✓ |
| K-935 | sahteciliğe karşı korumanın varsayılanı, izinli etiket süzgecinin kütüphanesi, içerik güvenlik politikası ve tek kullanımlık değerin üretimi | ✓ |
| K-936 | dış çağrının zaman aşımı ve HTTP katmanında kendiliğinden yeniden denemenin olmaması | ✓ |
| K-937 | veritabanı imajı ve yedek araçları | ✓ |
| K-941 | gelen bildirim kaydı, uç, imzalı makine uçları | ✓ |
| K-942 | güvenlik kalıpları; form ve site güvenliği | ✓ |
| K-946 | iş kuyruğu, zaman, uyarılar | ✓ |
| K-962 | kayıt — silme satırının yazılma sırası; yönetim komutları — dönüş denetiminin tablo listesi; veritabanı imajı — kesinleşme anı | ✓ |
| K-963 | veritabanı imajı ve yedek araçları — gecelik tam yedeğin deposu | ✓ |
| K-964 | veritabanı imajı ve yedek araçları — budamanın ayarı ve bütünlük denetimi | ✓ |
| K-965 | göçlerin yazılışı — değer kuralı, iz tablosu, yarıda kalan göç, CI'ın göç adımı, yamanın tek istisnası | ✓ |
| K-969 | satırın sırası K-962'nin park satırında, değişmedi | park gerekmez |
| K-971 | yönetim komutlarının girdisi — *bu taramada eklendi* | ✓ |
| K-972 | güvenlik kalıpları | ✓ |
| K-973 | görüntü işleme kütüphanesi; güvenlik kalıpları; form ve site güvenliği | ✓ |
| K-974 | hata yakalama ve kayıt | ✓ |
| K-975 | hata yakalama ve kayıt | ✓ |
| K-977 | K-17 park satırları — Aşama 1 blokları hizalandı — *bu taramada eklendi* | ✓ |

#### `12` Doğrulama Protokolü — 20 karar

| Karar | İş | Park satırında |
|---|---|---|
| K-884 | hakkın düştüğü an ve devam isteği | ✓ |
| K-890 | aynı son adedi aynı anda onaylayan iki müşteri | ✓ |
| K-893 | çift gönderilen onay ve iki kez gelen bildirim | ✓ |
| K-898 | kesintide dolan havale süresinin ertelenmesi ve kaçan işlerin sırası | ✓ |
| K-904 | bağlantının açılmasının doğrulamadığı | ✓ |
| K-907 | girişten dönüşte yazılanların korunması ve onaydan sonra çerezin silinmesi | ✓ |
| K-908 | ara katmanın doğrulanması | ✓ |
| K-913 | kanal uyarısının açılıp kalkması; firma bildirimleri uyarısının iki kalkış yolu | ✓ |
| K-914 | üç hâlin senaryoları | ✓ |
| K-918 | durum ucunun uyarı koşulları | ✓ |
| K-928 | iki değerin yük senaryosu | ✓ |
| K-933 | kapalı sayfaların `noindex`'i — park gerekmez: doğrulama `02 §3.30.6`'nın kuralından türer | park gerekmez |
| K-934 | park gerekmez — yön `02 §3.8.3`'te kuralın kendisidir | park gerekmez |
| K-946 | iş kuyruğu, zaman, uyarılar | ✓ |
| K-951 | on yıllık saklamanın zaman ilerletilerek doğrulanması | ✓ |
| K-957 | aynı sepetle yeniden onayda önceki siparişin ayırdığı son adet | ✓ |
| K-967 | eşzamanlı onay senaryosu | ✓ |
| K-970 | tek işlenme senaryoları — *bu taramada eklendi* | ✓ |
| K-971 | bağlantının açılmasının değiştirmediği kayıtlar — *bu taramada eklendi* | ✓ |
| K-975 | kesinti ve durum ucu senaryoları — *bu taramada eklendi* | ✓ |

#### `DEPLOY_RUNBOOK.md` — 54 karar

| Karar | İş | Park satırında |
|---|---|---|
| K-861 | kurulumun aylık gider kalemleri — tavanın değeri; park satırının etiketi — *bu taramada eklendi* | ✓ |
| K-862 | kurulumun aylık gider kalemleri | ✓ |
| K-863 | sunucunun seçimi ve kurulumu | ✓ |
| K-864 | konum önerisi | ✓ |
| K-865 | sunucu seçim ölçütleri | ✓ |
| K-870 | veritabanının inceleme araçları | ✓ |
| K-872 | kurulumun sırası: alan adı, sertifika | ✓ |
| K-876 | eski bir ana dönüşten sonra imhanın yeniden uygulanması ve firmanın bilgilendirilmesi | ✓ |
| K-878 | yedeğin kurulumu ve aracın ayarları | ✓ |
| K-879 | yedek anahtarının firmaya teslimi; nesne depolamanın silmeye karşı koruması | ✓ |
| K-880 | kurtarma adımları ve prova kaydı | ✓ |
| K-888 | kurtarmadan sonra kurtarılan son anın firmaya bildirilmesi | ✓ |
| K-896 | sunucu saatinin eşitlenmesi | ✓ |
| K-898 | sağlık ucunun ters vekilden açık olması | ✓ |
| K-910 | sır envanteri, dağıtım yarıçapı sütunları | ✓ |
| K-911 | sırların döndürülmesi ve kurtarmada yeniden kurulması | ✓ |
| K-915 | kayıt dosyalarının yeri ve döngüsü; eşitlemeye girmesi | ✓ |
| K-916 | sunucunun günlüğünün süresi | ✓ |
| K-917 | okuma komutunun kullanımı; dışa aktarma dosyasının firmaya iletimi ve silinmesi | ✓ |
| K-918 | dış kontrolün kurulumu; durum komutu; konteynerlerin yeniden başlama politikası | ✓ |
| K-919 | referans kurulumun öncü rolü | ✓ |
| K-920 | Compose tanımının kurulumu, adlandırılmış bölümler, çekme belirteci | ✓ |
| K-922 | zorunlu, koşullu ve teknik ayarların listesi | ✓ |
| K-923 | yönetim komutları; ilk yöneticinin ve kurtarmanın adımları | ✓ |
| K-924 | güncelleme turunun komutu ve adımları; kurulum listesi; prova kaydı | ✓ |
| K-925 | geri alma adımları | ✓ |
| K-926 | işletim sisteminin kendiliğinden güncellemesi ve yeniden başlatma saati; aylık bakım sürümü; veritabanının ana sürüm geçişi | ✓ |
| K-927 | SSH ve güvenlik duvarı; kurulum başına anahtar; anahtarın kaldırılması · veritabanının inceleme araçları — salt okuma kullanıcısı | ✓ |
| K-928 | tepe dakikanın okunması | ✓ |
| K-929 | büyüme yolunun güncelleme turundaki okumaları — tepe dakika satırının etiketi — *bu taramada eklendi* | ✓ |
| K-937 | yedek, kurtarma ve prova; sırlar; sistem kayıtları | ✓ |
| K-938 | güncelleme ve geri alma; sunucunun seçimi | ✓ |
| K-939 | yedek, kurtarma ve prova | ✓ |
| K-941 | yedek, kurtarma ve prova | ✓ |
| K-943 | kurulumun yapısı | ✓ |
| K-944 | sistem kayıtları; yedek | ✓ |
| K-946 | izleme | ✓ |
| K-949 | sunucunun seçimi; kurulumu yapanın uyarısı | ✓ |
| K-955 | eski bir ana dönüşten sonraki adımlar | ✓ |
| K-956 | güncelleme ve geri alma | ✓ |
| K-958 | güncelleme ve geri alma | ✓ |
| K-960 | güncelleme turu — geride kalmış kurulumun sırayla geçirilmesi | ✓ |
| K-961 | eski sürümlerden geri getirmenin adımları — silme işaretlerinin ve sonraki sürümlerin kaldırılması | ✓ |
| K-962 | yedek, kurtarma ve prova; güncelleme ve geri alma — dönüşün koşulu, dönüş noktasının kaydı ve denetim | ✓ |
| K-963 | yedek, kurtarma ve prova — depo seçimi, prova kaydı, kurtarmada eski sunucunun durdurulması · sunucunun seçimi — provanın çıkış trafiği | ✓ |
| K-964 | yedek — yaşam döngüsü kuralının iki eylemi, nesne kilidi yolunun sınırı | ✓ |
| K-965 | güncelleme ve geri alma — çalışan sürümün okunması, dönüşten sonraki adımlar, ana sürüm geçişi, imajların saklanması | ✓ |
| K-966 | yedek — arşiv zaman aşımının değeri | ✓ |
| K-969 | yedek, kurtarma ve prova | ✓ |
| K-971 | sırlar · ilk kayıtlar | ✓ |
| K-972 | kurulumun sırası | ✓ |
| K-973 | sırlar | ✓ |
| K-974 | izleme · yedek, kurtarma ve prova — istisnanın sözü ve açık koşulun yeni uyarıyı örtmesi | ✓ |
| K-975 | sistem kayıtları; izleme | ✓ |

### 6.2 Teknik Mimari'nin gövdesinde sonraki dokümanı anan cümleler

Mekanik çıkarım (`05 §0`–§11; başlık notu ve §12 hariç): sonraki dokümanı numarasıyla ya da adıyla anan 114 madde. Betik her maddenin hedef dokümanın Aşama 4 park bloğunda bölümüyle anılıp anılmadığını denetledi (88 anılıyor); anılmayan 26 madde tek tek okundu.

| Hedef | Madde | Park satırında (betik) | Okunarak: başka bir park satırı ya da devir | Anma ya da genel ilke | Karar satırı olmadan devredilen |
|---|---|---|---|---|---|
| `06` | 9 | 4 | 2 (§7.2.5 → K-870 satırı; §7.3.1 → K-873 satırı) | 2 (§7'nin girişi: "tabloların, alanların ve türlerin kendisi `06`'nın işidir") | 1 — **sayfa adresinin veride tutuluşu** (`05 §3.7.1`) |
| `07` | 7 | 3 | 2 (§3.5.1 → K-932 "park gerekmez"; §6.7.1 → `08`'in K-906 satırı, "bu aşamanın ve `07`'nin kararıdır") | — | 2 — **sayfa adreslerinin önekleri** (`05 §3.7.1`) · **parçalı yüklemede doğrulama değerinin istek başlığındaki biçimi** (`05 §6.5.1`) |
| `08` | 24 | 19 | 5 (1.2.28 ve §4.4.2 → K-862, K-892 satırları; §5.3.3, §10.3.5 → K-897, K-974 satırı; 6.x.11 → Aşama 2'nin devri, K-911 "park gerekmez") | — | — |
| `09` | 28 | 25 | 3 (MK-02 → K-867 satırı; 2.3.2 → K-932 satırı ve D-01; 7.10.1 → K-887 satırı) | — | — |
| `12` | 2 | 0 | 1 (11.1.5 → K-928 satırı) | 1 (3.8.4 — KP-71'in doğrulaması; `12` bütün KP satırlarını doğrular) | — |
| `DEPLOY_RUNBOOK` | 44 | 37 | 6 (2.2.5 → izleme satırı; 2.2.6 → sunucunun seçimi; 5.2.14 ve 5.4.1 → sunucu saati ve sağlık ucu; 6.9.2 → Aşama 2'nin K-699 satırı; 11.1.4 → tepe dakika) | 1 (§8'in girişi: "kurulumun adımları, sır envanteri ve ayarların listesi `DEPLOY_RUNBOOK`'un işidir") | — |

**Karar satırı olmadan devredilen iki iş** (üç cümle): (a) sayfa adresinin veride tutuluşu → `06` ve adreslerin önekleri → `07` (`05 §3.7.1`; varlık kararı `02 §3.30.2`'de; adres biçiminin işi Aşama 3'ün dizininde `04 §3.5` devriyle `07`'ye bırakılmıştı, K-933 "park gerekmez" der); (b) parçalı yüklemede sahteciliğe karşı doğrulama değerinin istek başlığındaki biçimi → `07` ve `09` (`05 §6.5.1`; ilke K-935, `09`'un form ve site güvenliği satırı aynı cümleyi taşır). İkisi de detaydır — varlık kararları kayıtlıdır — ve kapısı hedef dokümanın yazımıdır. Yeni karar istemedi; hedef dokümanların park bloğundaki dizin satırı onları adıyla anar. Kural 1 açısından açık kalem değildir: tek evleri bu cümlelerdir.

### 6.3 Doğrulama ve soru devirleri

Teknik Mimari "bilgim tarihli" notuyla dış dünyaya ait olguları implementation'a ya da kuruluma bırakır (konvansiyon 10); bunlar devir değil doğrulamadır ve evleri park satırları ile D-01'dir (§6.4). Aşama açılışına giden **çok kritik soru adayı** tektir: desteklenen ödeme sağlayıcısının sayısı → Entegrasyon spesifikasyonları aşamasının açılışı (`08`'in K-932 satırı; `05 §3.6.4`). Entegrasyon Spesifikasyonu'nun yazımında doğrulanacak iki sağlayıcı davranışı K-974 ve K-978'in kalan risklerinin büyüklüğünü belirler: kaydedilemeyen bildirimin yeniden gönderilmesi ve kart iadesinin sonucunun bildirimle gönderilmesi (`08`'in bildirimler satırı).

### 6.4 D-01'e devredilenler — kurulumun ertelenen parçası

D-01 (`DEFERRED_BACKLOG.md`) Aşama 4'ün kapanışından sonra, ilk implementation task'ından önce kapanır; `SETUP.md`'nin değişikliği proje sahibinin onayıyla yapılır (GUARDRAILS §2). Teknik Mimari'nin verdiği **`05 §2.3`**'ün tablosundadır:

| D-01'in parçası | Teknik Mimari'nin verdiği | Karar |
|---|---|---|
| `CONTEXT.md` §1 — teknoloji yığını | C# / .NET 10, ASP.NET Core Razor Pages, EF Core, PostgreSQL 18, düz JavaScript ve CSS; Linux konteynerleri, Compose, Caddy 2, pgBackRest ve dosya yedeği aracı; GitHub Actions ve GitHub Container Registry (2.3.1) | K-866, K-867, K-937 |
| `CONTEXT.md` §3 — kod klasörleri | Modüller `05 §3.3`'te; klasör düzeni Kodlama Kılavuzu ile birlikte (2.3.2) | K-932 |
| `.gitignore`, `.gitattributes` | .NET derleme çıktısı, kuruluma özgü ayar ve sır dosyaları repoya girmez; yazı tipi ve WebP ikili (2.3.3) | K-866, K-910 |
| `.env.example` | Kurulumun sır olmayan ayar dosyasının şablonu (2.3.4) | K-910 |
| `ci-commands.env`'in komutları | Lint, Build (bilinen açık taraması dahil), Unit (mimari testler), Integration (PostgreSQL 18), Contract `NOT_APPLICABLE`, Migration dry-run, Package build (üç imaj), E2E (2.3.5; `05 §8.3`) | K-921, K-965 |
| `ci.yml`'in runtime adımları | .NET 10 SDK; PostgreSQL 18 hizmeti; konteyner derlemesi (2.3.6) | K-921 |
| `publish.yml` | Kalır; imajlar GitHub Container Registry'ye, yazma izni D-01'de (2.3.7) | K-920, K-921 |
| D-01'de doğrulanacaklar ("bilgim tarihli") | Yedek araçlarının saklamayı uygulamak için güncel nesneyi silme yetkisine ihtiyacı (`05 §7.6.10`) · pgBackRest'in resmî konteyner imajının olmadığı (`05 §8.2.2`) · veritabanının işlemlerin kesinleşme anını tutması (`track_commit_timestamp`, `05 §8.5.11`) | K-937, K-961, K-962 |

**Bu taramada:** D-01'in kanıt hücresi yalnız K-866, K-867, K-920 ve K-921'i sayıyordu; K-910 (`.env.example`), K-932 (kod klasörleri), K-937 (veritabanı ve yedek aracı imajları) ve K-965 (CI'ın göç adımı) eklendi.

---

## 7. Öğrenim terfisi adayları — 5. adımın girdisi

Metinler tracker §12.1'dedir; bu dizin onları tek yerde gösterir.

| # | Aday | Kaynak | Durum |
|---|---|---|---|
| 1 | İzlenebilirlik matrisi zorunlu olmayan aşamada devir yükü matris kadar büyük | Açılış | Bekliyor |
| 2 | Devir notu kısıt sorularını workshop'a bıraktı; checklist kısıtın erken söylenmesini ister | Açılış | Bekliyor |
| 3 | Çok kritik sorunun öncülü de kural katmanlarına karşı taranır | K-860 | Bekliyor |
| 4 | Yalnız numaralı atfın işi gerekçede saklı kalabiliyor | K-916 | Bekliyor |
| 5 | Terim taraması Türkçe ses değişimini görmez | K-916 | Bekliyor |
| 6 | Cross-review'da iki koşumun eşiği 400 KB'ın altında da gerekli | K-953 | Bekliyor |
| 7 | Güvence cümleleri yazımda mekanizmaya karşı okunur | K-962…K-975 | Bekliyor |
| 8 | **Kalite döngüsünde güncellenen park satırı kararın bütün kollarını taşımıyor** | **Bu tarama** (§5.3 b, c — 32 yüz, 17 çift) | Bekliyor |
| 9 | **Geri beslemede kuralın evi alt dokümanın Kaynak hücresinden bulunmuyor** | **Bu tarama** (§5.2 — Md-1) | Bekliyor |
| 10 | **Mimari doküman ürünün tanımadığı bir işlemi varsayıyor** — öncülün bir türü ürünün işlem envanteridir | **Bu tarama** (§5.2 — Md-4) | Bekliyor |
| 13 | Tek "evet" her öneri için ayrı karar sayılmaz | Aşama 3'ten (K-853) | Bekliyor — kapı Aşama 4'ün öğrenim terfisi |
| Ö-23 | Checklist'in skill'e dönüştürülmesi | Aşama 1'den | Bekliyor — kapı Aşama 4'ün öğrenim terfisi; ikinci koşul (kapanışta yapısal değişiklik almamak) orada okunur |

**Bu taramanın gözlemi — kurala kanıt:** Aşama 3'ün öğrenim terfisinin iki kuralı bu aşamada işledi — park satırı etiketi ile etki sütununun iki yönlü eşleşmesi (checklist §3) Teknik Mimari'nin her kalite döngüsü adımında koştu ve bu taramada yalnız bir eksik bıraktı (K-900); kararın gerekçesindeki olgusal öncülün kuralın evinde okunması (checklist §3) iki yeni öncül buldu (K-978, K-979). Kalite döngüsünün güncellediği park satırlarının içindeki eksik kollar ise yeni bir sınıftır (aday 8).

---

## 8. Checkpoint'e notlar

- Tracker §4'te açık satır yok, A-19 açılmadı; Teknik Mimari'de, Ürün Gereksinimleri §13'te ve öteki dokümanlarda açık kalem yok — checkpoint'in açık karar kontrolü bu raporla karşılanır.
- ⚠ listesi **41 karar** (§4.1) — hazır metin aynı yerde; arşiv işaretinden önce proje sahibine tek mesajda gösterilir.
- **Devredilen sınıf** (§12.1, K-976): iki yerin metin farkı ve güvence cümlesi ile mekanizmanın farkı checkpoint'in iç tutarlılık merceğindedir. Bu taramanın kanıtları: §5.3'ün 32 yüzü park satırlarında aynı sınıftır (kararın bir kolu yazılmış, öteki yazılmamış); §5.2'nin M2-12'si `05`'in bir maddesindeki iki yolun çelişkisidir; K-978 bir güvence cümlesinin ("kalıcı uyarı") mekanizmasıyla farkıdır.
- K-977, K-978 ve K-979 bu taramanın kararlarıdır; üçü de `05`'te ve park satırlarında yazılıdır. Checkpoint'in iç tutarlılık merceği K-978'in yüzlerini (`05 §4.6.2`, §10.1.3, MK-40, `08`'in bildirimler satırı) ve K-979'unkileri (`05 §5.2.9`, §7.4.2, §7.4.4, `06`'nın imha satırı) bir kez daha okuyabilir.
- Aşama 1–3'ün park satırları K-851…K-976'ya, Aşama 4'ünküler numarası kaynaktan büyük bütün kararlara karşı numaraya dayalı tam okumayla bir kez tarandı (569 eşleşme). Checkpoint yeniden taramaz; yalnız bu PR'ın değiştirdiği satırları okur.
- 6. adımda Veri modeli aşamasına devrin tablosu bu raporun §6'sını ve hedef dokümanların park bloklarındaki dizin satırını gösterir; öğrenim adayları dizini (§7) 5. adımın girdisidir.

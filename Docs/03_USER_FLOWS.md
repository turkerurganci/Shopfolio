# Shopfolio — User Flows

**Versiyon: v0.21** | **Bağımlılıklar:** `01_PROJECT_VISION.md`, `02_PRODUCT_REQUIREMENTS.md`, `10_MVP_SCOPE.md` (kapsam), `PRODUCT_DISCOVERY_STATUS.md` (Aşama 2 kararları) | **Son güncelleme:** 2026-10-05

> **Aşama:** 2 — Kullanıcı Akışları · **Rol:** Product Owner / Business Analyst
> **Traceability zorunlu:** Hayır (doğrudan türetim) — ama akışlar yazıldıktan sonra `02`'ye **geri dönülür**, tutarsızlık varsa düzeltilir.

> **Yazım durumu (K-28, K-30, K-432):** doküman Aşama 2'nin **tek yazım turunda**, beş oturumda yazılır; sıra ve konular `PRODUCT_DISCOVERY_STATUS.md` §8.2–§8.3'tedir. Girdi karar kaydı, bu dokümanın şablonu ve üst dokümanlardır — workshop'un sohbet geçmişi değil.
> - **1. oturum (2026-10-04, v0.2):** §0 — yazım konvansiyonları ve bölüm haritası (blok AK0) · §1 — durum makinesi (blok AK1) · §11 — geri besleme tablosunun ilk satırları. Oturum dokuz karar aldı: ikisi yazım konvansiyonu (K-669, K-670), yedisi yazımın bulduğu boşluk (K-671…K-677); yedisi `02`'ye, biri `10`'a da döndü (§11).
> - **2. oturum (2026-10-04, v0.3):** §2 — müşteri tarafının ana akışları ve uçtan uca anlatılar (blok AK2) · §11'e dört satır. Oturum yazımın bulduğu dört boşluğu karara bağladı (K-678…K-681); dördü `02`'ye, ikisi `10`'a da döndü (§11). Aynı PR'da §1 hizalandı: ayıp talebinin müşteri tarafından yeniden açılmasının bildirimi (§1.9.1, §1.11.10 — K-681) ve S9 ile S11'in "Akış" sütunu.
> - **3. oturum (2026-10-04, v0.4):** §3 — hata akışları; `02 §6`'nın altı ilkesi ve yüz otuz sekiz senaryosu birer satırla (blok AK3) · §4 — zaman aşımları; Z-1…Z-47 ve kendiliğinden işleyen anlar, kesinti dahil (AK4) · §5 — itiraz ve anlaşmazlık (AK5) · §6 — kötüye kullanım ve inceleme (AK6) · §11'e altı satır. Oturum yedi karar aldı: biri yazım konvansiyonu (K-682), altısı yazımın bulduğu boşluk (K-683…K-688); altısı `02`'ye, ikisi `10`'a da döndü (§11). Aynı PR'da §1 ve §2 hizalandı (§1.3, §1.6.2, §1.7.1.9, §1.11.31, §1.11.38, yeni §1.11.39; §2.5.2.3, §2.8.4.3, §2.9.4).
> - **4. oturum (2026-10-04, v0.5):** §7 — bildirim haritası: B-1…B-16, F-1…F-6 ve hesap e-postaları her tetikleyicisiyle tek tabloda, bildirim üretmeyen olaylar ve geçiş bazlı eşleme (blok AK7) · §8 — firma tarafının yönetim akışları: katalog, sipariş yürütümü, firma iptali, iade ve geri ödeme, dokuz müdahale, manuel adım bütçesi, kurumsal içerik, mağaza ayarları, yönetici hesapları, bekleyen işler ve dışa aktarma (blok AK8) · §11'e üç satır. Harita akışların bildirim hücreleriyle betikle iki yönde karşılaştırıldı. Oturum dört karar aldı: biri yerleşim (K-689), üçü yazımın bulduğu boşluk (K-690…K-692); üçü `02`'ye, ikisi `10`'a da döndü (§11). Aynı PR'da §3'ün §8'e bağlanan "Akış adımı" hücreleri satır numarasına çevrildi (K-682); §0.2, §0.3.3, §1.7.1.9, §1.10.7, §1.11.21, §1.11.22, §3.3.9, §3.3.31, §3.5.2.6, §6.2.3.2 ve §6.3.2.4 hizalandı, §6.2.6.4 eklendi.
> - **5. oturum (2026-10-04, v0.6):** §9 — üyelik: kayıt ve doğrulama, giriş ve şifre sıfırlama, profil ve adres defteri, hesap silme ve kişisel veri başvurusu, tercih yönetimi (blok AK9) · §10 — operasyonel akışlar: dış servis kesintileri, bakım ve güncelleme, sitenin kesintisi, panele erişimin kaybı (blok AK10) · §11 — dört satır ve tablonun kapanışı (blok AK11). Oturum sekiz karar aldı: biri yazım konvansiyonu (K-700), yedisi yazımın bulduğu boşluk (K-693…K-699); yedisi `02`'ye, altısı `10`'a da döndü (§11). Aynı PR'da §3.4'ün "Akış adımı" hücreleri ve §7.1'in §9'a giden "Akış" hücreleri satır numarasına çevrildi (K-682, K-689); §3.4'e iki satır girdi (3.4.15, 3.4.16 — `02 §6`'nın senaryoları 140); §0.3.2, §1.10.5, §1.10.6, §2.6.6, §3 girişi, §3.1.1, §3.2.1.15, §4.1, §4.2, §6.1.1, §6.1.3, §6.2.5.1, §6.2.6.4, §7, §8 girişi, §8.5.7, §8.6.4.3, §8.7.5.1, §8.8 ve §8.8.2 hizalandı.
> - **Kalite döngüsü — audit ve deep review (2026-10-04, v0.7):** on mercek ve iki şüpheci (K-431); seksen bir ham bulgunun yetmiş dokuzu doğrulandı, ikisi tartışmalı kaldı ve işlenmedi. Rapor `Docs/AUDIT_REPORTS/03_AUDIT.md` ve `03_DEEP_REVIEW.md`'dedir. On yedi karar (K-701…K-717): iki yazım konvansiyonu — atıf dizisinde önsüz `§`'in okunuşu (§0.3.4), aktörü olmayan olay satırı (§0.3.2) —, on beş `02`'ye dönen karar (§11.31…§11.45; on üçü ⚠). Akışlarda öne çıkanlar: Teslim edilemedi'de firmanın kalem iptali ve S11 (1.11.21, 8.3.1.1), Kargoya verildi ve Teslim edilemedi'de son kalemin kapanışı (§1.4.3, yeni 1.11.40), birleşim tablosunun iki dipnotu (§1.4.1), müşteri aralıklarının açık kaleme bağlanması (§1.11), başka kanaldan gecikme feshi (yeni 1.11.41, 8.4.8), sitede sipariş teyidi (2.4.9, 2.5.1.2), sipariş arama (yeni 8.2.11), yanlış adrese giden davet (yeni 8.8.6), sağlayıcı anahtarlarının değişmesi (yeni 10.1.1.7), yeniden doğrulama limiti (yeni 3.4.17), §7.2'ye iki satır (7.2.29, 7.2.30); §1.4.4 ve §1.5'e satır kimliği, §0.2'ye §0 satırı, §0.1.4. Geri besleme: Ürün Gereksinimleri v0.54, MVP Kapsamı v0.36.
> - **Kalite döngüsü — cross-review, 1. tur (2026-10-04, v0.8):** ikinci model Codex CLI (`gpt-5.6-terra`), ciddiyet ölçüsüyle 1. turdan (K-718); girdi yalnız doküman ve şablon (K-431). Üç bulgu, üçü de iki yerin çelişmesi: kart dönüşünde ödeme durumunun koşulu (2.5.1.2), kesintide dolan havale süresinin iptalinin dönüşte ertelenmesi (4.2.11, 10.3.3 — 4.2.12 ve 10.3.4 ile hizalandı), müşteri kaynaklı dönen gönderinin iptalinde teslim edilmiş kalem varken S11 (8.3.1.2 — 8.3.1.1, 1.11.21 ve 3.3.9 ile hizalandı). `02`'ye dönen karar yok; §11'e satır girmedi. Rapor `Docs/CROSS_REVIEW_REPORTS/03_CROSS_REVIEW.md`.
> - **Kalite döngüsü — cross-review, 2. tur (2026-10-04, v0.9):** aynı model ve ölçü (K-718); girdi yalnız doküman ve şablon (K-431). Bir bulgu, iki yerin çelişmesi: gecikme feshinin geri ödemesinin on dört günü iki satırda yasal teslim sınırının kimliğiyle (Z-11) anılıyordu (2.7.8, 8.4.8); 2.7.8 sürenin `02 §7.2.3`'teki yerini yazar, 8.4.8 ona işaret eder, 4.1.11 feshin geri ödemesini 2.7.8'e bağlar. `02`'ye dönen karar yok; §11'e satır girmedi. Rapor `Docs/CROSS_REVIEW_REPORTS/03_CROSS_REVIEW_R2.md`.
> - **Kalite döngüsü — cross-review, 3. tur ve etki yansıtma (2026-10-04, v0.10):** aynı model ve ölçü (K-718); girdi yalnız doküman ve şablon (K-431). **Model `SONUÇ: TEMİZ` döndürdü — TEMİZ ciddiyet ölçüsündedir;** döngü üç turda kapandı. Etki yansıtma üç turun düzeltmelerini Ürün Gereksinimleri'ne, MVP Kapsamı'na, Proje Vizyonu'na, sonraki dokümanların park bloklarına ve karar kaydına karşı taradı: `02`'ye ve `10`'a dönen değişiklik yok, §11'e satır girmedi. Kaynak sütunu betikle tarandı: gövdesinde Aşama 2 kararı anıp Kaynak hücresinde taşımayan otuz bir satır tamamlandı (§0.4.1, §0.5.2); 4.2.11 ve 10.3.3 K-667'yi ve `02 §3.21.8`'i, 8.3.1.2 `02 §5.4`'ü Kaynak'ta taşır. Rapor `Docs/CROSS_REVIEW_REPORTS/03_CROSS_REVIEW_R3.md`.
> - **Aşama 2 checkpoint'i (2026-10-04, v0.11 — K-437'nin 4. adımı):** rapor `Docs/CHECKPOINT_REPORTS/CP02_PHASE2_CHECKPOINT.md`'dedir. Atıf dizisinde devralma kuralına (K-701) uymayan beş hücre düzeltildi — 7.2.15 ve §11'in 11.2, 11.23, 11.26, 11.36 satırları · 8.3.3.2 kalan süreyi yalnız süresi olan hatta gösterir (K-716) · 6.1.2.2'nin aktör hücresi olay cümlesidir (K-702) · 7.2.16 başka kanaldan gecikme feshi kaydını da sayar (K-706) · §0.3.2'nin "Kullanıcı" adı MVP Kapsamı'nın satırlarındaki ortak addan ayrıldı (K-638, K-700) · §11'e checkpoint notu. Yeni karar yok; doküman Ürün Gereksinimleri v0.55 ve MVP Kapsamı v0.37 ile tutarlıdır.
> - **Cross-review, 4. tur — ölçüsüz yoklama (2026-10-04, v0.12 — K-720):** proje sahibinin sorusu üzerine aynı model ve girdiyle (K-431), ciddiyet ölçüsü olmadan tek tur; amaç ölçünün dışarıda bıraktığını görmekti, 5. tur koşulmaz. Üç bulgu geldi — misafir siparişinin e-postayla hesaba düşmesi, iade taşıyıcısının ön bilgilendirmesi, iade paketinin kalemle eşleştirilmesi —; ikisi reddedildi (bilinçli karar `02 §3.13.3`, K-99 · Yönetmelik m.12/5'in güncel metni taşıyıcı belirtilmemesini öngörür, `02 §7.4.2`, K-492, K-493), biri yoklama süzgecinde alınmadı (iyileştirme önerisi). Akış içeriği değişmedi; K-718'in TEMİZ'i (3. tur) çıkış koşulu olarak geçerlidir. Rapor `Docs/CROSS_REVIEW_REPORTS/03_CROSS_REVIEW_R4.md`.
> - **Aşama 2 kapanışı — arşiv işareti (2026-10-04, v0.13 — K-437'nin 6. adımı):** doküman **✓ Tamamlandı**; kapanış sürümü budur. Bu sürümde yalnız başlık notu ve dosya sonu dipnotu değişti — akış içeriği v0.12 ile aynıdır. Karar kaydında Aşama 2'nin kayıtları salt okunurdur; bu dokümanın dayandığı Aşama 2 kararlarının otoriter kaynağı artık bu doküman ve geri beslendiği Ürün Gereksinimleri ile MVP Kapsamı'dır (`PRODUCT_DISCOVERY_STATUS.md` başlık notu; K-647). Sonraki aşamalara devir: `PRODUCT_DISCOVERY_STATUS.md` §9.
> - **Aşama 3 — izlenebilirlik matrisinin geri beslemesi (2026-10-04, v0.14 — K-652, K-726):** Arayüz Tanımları'nın (`04`) izlenebilirlik matrisinin bulduğu beş boşluğun kararı bu dokümana işlendi; ✓ durumu korunur, kalite döngüsü yeniden açılmaz ve yeni satır yoktur — satır sayısı 793'te kalır. Değişen yerler: §1.6.1'in kapanış paragrafı — müşterinin geri alınamaz dört işleminin onayı (K-732) · 9.2.2 — bağlanmış Google girişi üye tarafından kaldırılamaz (K-733) · 9.3.4 — bekleyen e-posta değişikliği hesap ekranında görünür, yeni istek yerine geçer; 6.1.1.9 — yeni istek L-9'a sayılır (K-734) · 8.8.2 ve 9.3.7 — yönetici hesabının adı (K-736) · 8.1.1 — ürün listesinde arama ve yayın durumu süzgeci (K-737). Kararlar ve gerekçeleri `PRODUCT_DISCOVERY_STATUS.md` §2 ve §3'te, geri besleme tablosu `04 §1.3`'tedir.
> - **Aşama 3 — workshop'un geri beslemesi (2026-10-04, v0.15 — K-652, K-726):** Arayüz Tanımları'nın (`04`) workshop oturumunun üç konusu bir ürün kuralı doğurdu ya da değiştirdi ve bu dokümana işlendi; ✓ durumu korunur, kalite döngüsü yeniden açılmaz ve yeni satır yoktur — satır sayısı 793'te kalır. Değişen yerler: 2.1.1 — ana sayfanın ürün vitrini yayındaki en yeni ürünleri gösterir (K-754) · 7.1.37 ve 8.9.2 — işareti düşüren on altı müşteri bildiriminin hepsi panelden yeniden gönderilir; "en az B-1" ifadesi kalktı (K-759) · 1.7.3.1, 4.1.29, 8.9.1 ve 10.1.2.2 — firmaya giden bildirim satıra işaret düşürmez, panelin ana sayfasında uyarı çıkar (K-760) · 8.9.1 ve 10.1.2.6 — işaretli siparişler ana sayfadaki satırdan bulunur (K-761). Kararlar ve gerekçeleri `PRODUCT_DISCOVERY_STATUS.md` §2'de, geri besleme tablosu `04 §1.3`'tedir.
> - **Aşama 3 — kısmi adet kararının geri beslemesi (2026-10-04, v0.16 — K-652, K-726):** proje sahibinin kararıyla iptal, cayma ve iadede müşteri adedi birden büyük kalemin kaç adedini işleme sokacağını seçer (K-787 — K-178'i genişletir); kural Ürün Gereksinimleri v0.60'tadır (`02 §7.1.6`, §7.1.7). ✓ durumu korunur, kalite döngüsü yeniden açılmaz. Durum adları değişmedi; kalem adetle okunur. Değişen yerler: §1.4.3 (açık kalem adetle), 1.4.4.4, 1.4.4.6, 1.7.1.2, 1.7.1.4–1.7.1.6, 1.7.1.8, 1.7.1.10, 1.7.3.5, yeni 1.7.4 (kalemin adedi), §1.11'in girişi, 1.11.6, 1.11.21, 1.11.28, 1.11.30, 2.7.2, 2.7.3, 2.7.6, 2.7.7, 2.8.1.2, 2.8.1.5, 2.8.2.2, 2.8.4.1, 2.9.2, 2.9.4, 4.2.6, 7.1.13, 7.1.14, 7.1.20, 8.3.1.1, 8.3.2.1–8.3.2.3, 8.3.3.3, 8.4.2, 8.4.7, 8.4.8, 8.4.4, 8.4.10, 8.5.6 ve §8.5'in bütçe notu, 3.3.10, 3.3.23, 3.3.28, §11'in kapanış notu; yeni satır yok — satır sayısı 793'te kalır, yeni kural 1.7.4 bir paragraftır (K-787…K-794).
> - **Aşama 3 — yazım turunun 3. oturumunun geri beslemesi (2026-10-04, v0.17 — K-652, K-726):** Arayüz Tanımları'nın (`04`) panel ekranlarının ilk dokuzu yazılırken üç karar bu dokümana işlendi; ✓ durumu korunur, kalite döngüsü yeniden açılmaz ve yeni satır yoktur — satır sayısı 793'te kalır. Değişen yerler: 2.1.2 ve 8.1.14 — aynı seviyedeki kategoriler alfabetik sıradadır, elle sıralanmaz (K-805; `02 §3.4.1`) · 2.4.4 ve 8.1.12 — kupon kodu tekildir ve büyük-küçük harf ayrımı olmadan eşleşir, kupon silinmez, değişiklik yalnız yeni siparişlere işler (K-806; `02 §3.10.5`) · 1.11.28 ve 8.3.2.1 — teslim alma adımının aralığı ayıp talebinde sözleşmeden dönmenin geri alınan malını da sayar; kural `02 §7.5.3`'te ve 8.4.10'da vardı, aralık onunla hizalandı (K-808). Geri besleme tablosu `04 §1.3`'tedir.
> - **Aşama 3 — yazım turunun 5. oturumunun geri beslemesi (2026-10-04, v0.18 — K-652, K-726):** Arayüz Tanımları'nın (`04`) durum × rol matrisi kurulurken sipariş sayfasının geri ödemeyi yalnız ödeme ekseninin rozetiyle gösterdiği bulundu; karar bu dokümana işlendi. ✓ durumu korunur, kalite döngüsü yeniden açılmaz ve yeni satır yoktur — satır sayısı 793'te kalır. Değişen yer: 2.6.4 — sayfanın taşıdıkları gerçekleşmiş geri ödemeleri de sayar; tarih, tutar ve yolla (K-826; `02 §3.22.4`'ün "her bilgiyi taşır" kuralının yüzü). Geri besleme tablosu `04 §1.3`'tedir.
> - **Aşama 3 — Arayüz Tanımları'nın kalite döngüsünün (audit ve deep review) geri beslemesi (2026-10-04, v0.19 — K-652, K-726):** iki karar ve iki hizalama bu dokümana işlendi; ✓ durumu korunur, kalite döngüsü yeniden açılmaz. 2.8.1.2 — kargodaki kalemde beyan ekranı önce kargonun teslim alınmayan malı firmaya döndürdüğünü söyler, gönderme yükümlülüğü mal teslim alınırsa doğar (K-838) · 9.3.6 — geri alma bağlantısının açılması değil ekrandaki tek düğme geri almadır (K-839) · 2.1.5 — ürün sayfası üretim yerini dijital üründe de — doluysa — gösterir (`02 §3.8.5`'e hizalandı) · 8.7.2.3, 8.7.4.2 — teslimat illeri üretilen metinleri besleyen ayarlar arasındadır (`02 §3.24.3`, §3.33.3'e hizalandı). Geri besleme tablosu `04 §1.3`'tedir.
> - **Aşama 3 — Arayüz Tanımları'nın cross-review etki yansıtması (2026-10-05, v0.20 — K-652, K-726):** bir hizalama, yeni satır yok; ✓ durumu korunur, kalite döngüsü yeniden açılmaz. 2.3.1 ve 3.2.1.9 — daha önce alınmış dijital varyantın uyarısı Ürün Gereksinimleri'nin tam metniyle yazılır: "Bu ürünü daha önce aldınız — sipariş sayfanızdan indirebilirsiniz" (`02 §3.12.9`, §6.2.9; K-236).
> - **Aşama 3 — çakışma taraması (2026-10-05, v0.21 — K-437'nin 3. adımı; K-652):** bir karar ve dört hizalama; ✓ durumu korunur, kalite döngüsü yeniden açılmaz. **K-850:** veri toplayan girişlerin kapısına çerez politikasının yayını da girer — 2.2.7, 3.4.12, 3.4.16, 3.5.3.9, 3.5.4.10, 9.1.1, 9.1.6, 9.2.2. **Hizalamalar:** 8.4.8 ve 3.3.25 — başka kanaldan gelen bildirimin kaydında caymada cayılan adet, gecikme feshinde seçim yok (K-787, K-796) · 7.1.21–7.1.24 — B-9 kayıt kopyası kalemleri ve adetleri taşır (`02 §9.2`; K-790) · 2.4.6 — onay bölümünde kutuların üstündeki cayma özeti (K-831) · §7 girişi, 3.1.1, 1.7.3.1, 1.11.36, 7.1.37, 8.9.2, 10.1.2.6 — firma bildirimi ve davet yeniden gönderilmez, yeniden gönderim siparişin ayrıntısındadır (K-759, K-760, K-761). Rapor `Docs/CHECKPOINT_REPORTS/PHASE3_CONFLICT_SCAN.md`; geri besleme tablosu `04 §1.3`'tedir.
> - **Yazım turu tamamlandı:** `03`'ün bütün bölümleri yazıldı. Kalite döngüsü — audit, deep review, cross-review, etki yansıtma — v0.10'da, checkpoint v0.11'de tamamlandı; ✓ arşiv adımında konuldu (v0.13; K-437, `checklists/document-stage.md` §7).

---

## 0. Nasıl yazılır

- **Aktör bazlı ilerleme:** Her aktör için ayrı akış seti — yerleşim §0.2'dedir.
- **Normal akış + hata akışları:** Önce "her şey yolunda", sonra *"ne ters gidebilir?"* ile hata ve alternatif akışlar. **Hata akışları happy path kadar önemlidir.**
- **Durum makinesi mantığı:** Her adım bir durumdan diğerine geçiştir. Durum isimleri `02 §5` ile birebir aynı; durum makinesi §1'dedir ve akışlar ona atıf yapar.
- **Bildirim entegrasyonu:** Her adımda *"burada kime bildirim gider?"* sorulur; tetikleyiciler akışa gömülür ve §7'de özetlenir.
- **Narrative gerektiren senaryolar:** if/then ile anlatılamayan uçtan uca senaryolar §2'nin son alt bölümünde — §2.10 "Uçtan uca anlatılar" — yazılır (K-651).

### 0.1 Kapsam ve kaynaklar

**0.1.1 Bu doküman kural koymaz; `02`'nin kurallarını dört aktörün adım adım deneyimine çevirir.** Aktörler `02 §1.3`'tedir: ziyaretçi · misafir alıcı · üye müşteri · firma yöneticisi. Kural, sayı ve kapsam kararlarının otoriter kaynağı Ürün Gereksinimleri (`02`) ve MVP Kapsamı'dır (`10`); Aşama 1'in kayıtları salt okunurdur ve dokümanla ayrışırsa doküman kazanır (K-647).

**0.1.2 `02`'nin ya da `10 §2`'nin saymadığı bir yetenek akışa girmez.** Akış yazılırken bir yetenek, kural ya da terim eksik çıkarsa bu bir **boşluktur:** sessizce eklenmez ve uydurulmaz; karar kaydında bir K satırıyla karara bağlanır, `02`'ye — `10`'un satırına dokunuyorsa `10`'a da — aynı PR'da sürüm artışıyla yazılır ve §11'e satır girer (K-652, §0.6).

**0.1.3 Kapsam dışı kalemler "yoktur" diye anılır.** Şablonun ya da akışın beklediği ama ürünün bilinçli olarak sunmadığı bir adım — bildirim tercihi, elle sipariş, bakım modu — akışta tek cümleyle ve `02` ya da `10 §3` atfıyla "yoktur" diye yazılır; boş bırakılmaz.

**0.1.4 `10 §2`'nin akış doğurmayan satırları.** KP-37'nin arama motorlarına verdiği veriler — sipariş sayfasının arama motorlarına kapalı olması (2.6.4) dışında —, KP-70'in tarayıcı tabanı, KP-71'in erişilebilirliği ve KP-69'un vitrin duyarlılığı bir akış adımı doğurmaz; karşılıkları ekran tasarımında (`04`) ve mimaride (`05`), doğrulamaları Doğrulama Protokolü'ndedir (`12`).

### 0.2 Bölüm haritası

Şablonun bölümleri korunur; alt bölüm numaraları aşağıdaki tabloyla sabittir (K-650, K-651, K-670). Bir yazım oturumu bir alt bölümü bölmek ya da birleştirmek zorunda kalırsa tabloyu ve ona atıf yapan satırları aynı PR'da günceller. Tablonun "Oturum" sütunu yazım turunun planıdır (`PRODUCT_DISCOVERY_STATUS.md` §8.2).

| Bölüm | Alt bölümler | Plan konuları | Oturum |
|---|---|---|---|
| §0 Nasıl yazılır | 0.1 Kapsam ve kaynaklar · 0.2 Bölüm haritası · 0.3 Adlar ve kimlikler · 0.4 Akış adımının biçimi · 0.5 Kural ayrıntısı ve kaynak gösterme · 0.6 Boşluk, geri besleme ve devir | AK0-01…AK0-04 | 1 ✓ |
| §1 Durum makinesi | 1.1 Envanter · 1.2 Sevkiyat ekseni · 1.3 Ödeme ekseni · 1.4 İki eksenin bağı · 1.5 Tipe göre hatlar · 1.6 Geri alınamaz geçişler, onay pencereleri ve düzeltme · 1.7 Kalem kayıtları ve panel işaretleri · 1.8 Yayın durumları · 1.9 Talep durumları · 1.10 Durum makinesi olmayan varlıklar · 1.11 Durum × rol × işlem | AK1-01…AK1-09 | 1 ✓ |
| §2 Ana akışlar — müşteri tarafı | 2.1 Ziyaretçi: vitrinde gezinme ve ürünü bulma · 2.2 Ziyaretçi: kurumsal içerik ve iletişim formu · 2.3 Müşteri: sepet · 2.4 Müşteri: ödeme adımı ve sipariş onayı · 2.5 Müşteri: ödeme ve teslim · 2.6 Müşteri: sipariş takibi (akış 2) · 2.7 Müşteri: iptal ve gecikme feshi · 2.8 Müşteri: cayma ve iade · 2.9 Müşteri: ayıp talebi · 2.10 Uçtan uca anlatılar | AK2-01…AK2-10 | 2 ✓ |
| §3 Hata akışları | 3.1 Bütün akışlara uygulanan ilkeler · 3.2 Satın alma ve sipariş takibi · 3.3 İptal, cayma ve iade · 3.4 Üyelik · 3.5 Firma tarafı | AK3-01…AK3-06 | 3 ✓ |
| §4 Zaman aşımı yönetimi | 4.1 Süre envanterinden zaman aşımı akışları · 4.2 Kendiliğinden işleyen anlar | AK4-01, AK4-02 | 3 ✓ |
| §5 İtiraz ve anlaşmazlık | 5.1 Üç yol ve itirazın yeri · 5.2 Ürünün dışında çözülen anlaşmazlıklar · 5.3 İade reddi ve değer kaybı | AK5-01…AK5-03 | 3 ✓ |
| §6 Kötüye kullanım ve inceleme | 6.1 Deneme limitleri · 6.2 Teyit ve kandırılma akışları · 6.3 Firmanın inceleme akışları | AK6-01…AK6-04 | 3 ✓ |
| §7 Bildirim haritası | 7.1 Bildirimler ve tetikleyicileri — tek tablo (K-689) · 7.2 Bildirim üretmeyen olaylar ve olmayan bildirimler · 7.3 Geçiş ve olay bazlı eşleme | AK7-01…AK7-03 | 4 ✓ |
| §8 Yönetim akışları — firma tarafı | 8.1 Katalog yönetimi (akış 5) · 8.2 Sipariş yürütümü: olağan hat (akış 6) · 8.3 Firma iptali, iade teslim alma ve geri ödeme · 8.4 Sipariş müdahaleleri · 8.5 Manuel adım bütçesi · 8.6 Kurumsal içerik, marka, duyuru ve iletişim talepleri (akış 7'nin panel yüzü) · 8.7 Mağaza ayarları, satış kapısı ve kurulum kontrol listesi (akış 8) · 8.8 Yönetici hesapları ve davet · 8.9 Bekleyen işler, satış özeti, işlem izi ve dışa aktarma | AK8-01…AK8-11 | 4 ✓ |
| §9 Destekleyici akışlar — üyelik (akış 4) | 9.1 Kayıt, e-posta doğrulama ve Google ile ilk giriş · 9.2 Giriş, oturum, şifre sıfırlama ve yeniden doğrulama · 9.3 Profil ve adres defteri · 9.4 Hesabın silinmesi ve kişisel veri başvurusu · 9.5 Tercih yönetimi | AK9-01…AK9-05 | 5 ✓ |
| §10 Operasyonel akışlar | 10.1 Dış servis kesintileri · 10.2 Bakım ve güncelleme · 10.3 Sitenin kesintisi · 10.4 Panele erişimin kaybı | AK10-01…AK10-04 | 5 ✓ |
| §11 `02`'ye geri besleme | tek tablo | AK11-01 | her oturum satır ekledi; 5. oturum kapattı ✓ |

Akış 6'nın yönetici müdahaleleri §8'de akışın parçası olarak kalır (K-468, K-650); fiziksel siparişin uçtan uca ana akışı (akış 1 + 6, `02 §2.2`) §2.10'da birleşir (K-651).

### 0.3 Adlar ve kimlikler

**0.3.1 Adlar `02` ile birebirdir.** Durum adları `02 §5`'ten, terimler ve İngilizce kod karşılıkları `02 §1.2`'den alınır; eş anlamlı kullanılmaz (K-17). Kod karşılığı yalnız `02`'nin verdiği yerde ve yalnız §1'in tablolarında yazılır — bu doküman kod adı **icat etmez**; yeni bir terim gerekirse §0.1.2'nin boşluğudur. Panel işaretlerinin metni (*"e-posta ulaşmadı"* gibi) `02`'deki tırnaklı biçimiyle yazılır.

**0.3.2 Aktör yazımı.** Adım tablolarının aktörü şu adlardan biridir: **Ziyaretçi** · **Misafir alıcı** · **Üye** (üye müşterinin kısaltması) · **Müşteri** (misafir alıcı ya da üye — `02 §1.3`) · **Yönetici** (firma yöneticisinin paneldeki işlemi) · **Sistem** (ürünün kendiliğinden yaptığı işlem) · **Kullanıcı** (üyelikte hesabı henüz açılmamış ya da hesabına giremeyen kişi — doğrulama, sıfırlama ve geri alma bağlantısını açan; §9 — MVP Kapsamı'nın satırlarındaki "Kullanıcı" ortak adından ayrıdır: `10 §2`, K-638) · **Kurulum tarafı** (kurulumu yapan; panelin ve ürünün dışındaki işlem — `10 §4.1`; §10) (K-700). "Firma" satıcının kendisidir: yükümlülük ve sorumluluk cümlelerinde kullanılır, panel adımının aktörü değildir. **Aktörü olmayan dış olay** — kargonun, bankanın, sağlayıcının, saldırganın, üçüncü bir kişinin ya da zamanın getirdiği — aktör hücresinde olay cümlesiyle yazılır ve sistemin cevabı "Sistem" sütunundadır; panel adımının aktörü her zaman Yönetici, panelin dışındaki kurulum işlemi Kurulum tarafıdır (K-702).

**0.3.3 `02`'nin kimlikleri olduğu gibi kullanılır** (K-532): sevkiyat geçişleri **S1…S11**, ödeme geçişleri **Ö1…Ö5** (`02 §5.4`, §5.5) · müşteri bildirimleri **B-1…B-16**, firma bildirimleri **F-1…F-6** (`02 §9`) · süreler **Z-1…Z-47** (`02 §4.2`) · deneme limitleri **L-1…L-9** (`02 §8.2`) · parametreler **P-1…P-49** (`02 §11`) · hacim kabulleri **H-** (`02 §11.3`). `10`'un kimlikleri: kapsam satırları **KP-**, kapsam dışı satırları **KD-**, dış ön koşullar **ÖK-**, kısıtlar ve kabuller **SK-**, yol haritası adayları **YH-** (`10` başlık notu). Proje Vizyonu'nun (`01`) kimliklerine atıf her yerde `01 §…` diye nitelenir.

**0.3.4 Bu dokümanın kendi satır kimlikleri bölüm numaralıdır** (K-669): bir alt bölümün adım ya da satır tablosu `n.m.k` biçiminde numaralanır ve dışarıdan `03 §n.m.k` diye anılır — ör. §1.11'in on dokuzuncu satırı `03 §1.11.19` — `02 §6`'nın satır kalıbı (6.4.16). Harf önekli yeni bir kimlik ailesi açılmaz. Bir alt bölüm ya adım tablosu ya da kendi alt başlıklarını taşır, ikisini birden taşımaz. Bu doküman içinde önsüz `§` bu dokümanı gösterir; başka dokümana her atıf numarasıyla yazılır (`02 §7.3.4`). **Atıf dizisinde devralma (K-701):** bir hücrede ya da cümlede `0X §` ile başlayan dizideki sonraki önsüz `§`'ler o dokümanı gösterir — `` `02 §7.2.1`, §7.2.8 `` ikisi de `02`'dir —; dizi ` · `, `;`, parantezin kapanışı ya da cümle sonuyla biter ve ondan sonraki önsüz `§` yeniden bu dokümanı gösterir.

**0.3.5 Durum yazımı.** Siparişin iki ekseni birlikte `sevkiyat + ödeme` diye yazılır: *Alındı + Bekliyor*. Geçiş, kimliğiyle birlikte yazılır: *→ Hazırlanıyor + Ödendi (S1, Ö1)*. Kalem düzeyindeki değişiklik kaydın adıyla yazılır (§1.7): *kalem: cayma beyanı*.

### 0.4 Akış adımının biçimi

**0.4.1 Ana akışlar (§2, §8, §9) adım tablosuyla yazılır.** Şablonun dört sorusu sütundur:

| # | Aktör ne yapar | Sistem ne denetler, ne yapar | Durum | Bildirim | Kaynak |
|---|---|---|---|---|---|

- **#** — §0.3.4'ün numarası.
- **Durum** — değişiklik yoksa "—"; varsa §0.3.5'in biçimi.
- **Bildirim** — kimlik ve alıcı: *B-5 → müşteri*, *F-1 → firma*; bildirim yoksa "—". Bildirim üretmeyen bir olay `02 §9.2`'nin "bildirim üretmeyen olaylar" listesindeyse "— (`02 §9.2`)", hesap olaylarında "— (`02 §9.4`)" yazılır; yokluğun kaynağı başka bir maddeyse o madde yazılır ("— (`02 §9.5`)"). Kaynaksız "—" yalnız kayıt doğurmayan adımdadır. Müşteri bildirimi siparişin iletişim e-postasına, firma bildirimi firmanın iletişim e-postasına gider; iki istisna F-5 ve F-6'dır — bütün yöneticilerin kendi adreslerine gider (`02 §9.3.1`; K-692).
- **Kaynak** — `02 §…`; Aşama 2'nin kararlarında K numarası da (§0.5.2).

**0.4.2 Hata akışları (§3) tabloyla yazılır** ve ana akışın adımına bağlanır:

| # | Akış adımı | Ne ters gider | Sistemin cevabı | Durum | Bildirim | Kaynak |
|---|---|---|---|---|---|---|

"Akış adımı" ana akışın satır numarasıdır (`§n.m.k`); Kaynak `02 §6`'nın satırını (`02 §6.2.15`) gösterir. Her hata satırı bir ana akış adımına bağlanır; bağlanamayan hata bir boşluktur (§0.1.2).

**0.4.3 Zaman aşımları (§4) süre kimliğiyle yazılır:** süre (Z-) · başlangıç · varsa uyarı · sona erişte sistemin işlemi · normale dönüş. Değer yazılmaz, kimlik yazılır (§0.5.3).

**0.4.4 Her adımın durumu §1'e uyar.** Bir adım §1'in izin vermediği bir geçiş ya da birleşim üretiyorsa ya akış yanlıştır ya §1; ikisi aynı PR'da hizalanır ve fark `02`'ye dokunuyorsa §0.1.2 işler.

**0.4.5 Yeni elle adım bütçeyle okunur.** Akış yazılırken yöneticiye sipariş başına yeni bir zorunlu elle adım doğuyorsa öneri `02 §10.5`'in bütçesiyle birlikte okunur ve sayıyı geçirecekse karar kaydına döner (K-404, K-454; §8.5).

### 0.5 Kural ayrıntısı ve kaynak gösterme

**0.5.1 Kural ayrıntısı evinde kalır.** Bir adım bir kurala dayanıyorsa kuralın koşullarını kopyalamaz; ne olduğunu bir cümleyle yazar ve kurala bölüm numarasıyla işaret eder. Ayrıntıyı kopyalayan metin `02` ile ayrıştığı anda iki doğru üretir (`checklists/document-stage.md` §4).

**0.5.2 Kaynak önce dokümandır.** Aşama 1'in kararları `02 §…` ve `10` KP- atfıyla gösterilir — K numarası gerekmez, kararın evi dokümandır (K-647). **Aşama 2'nin kararları** (K-647 ve sonrası) K numarasıyla gösterilir; kararın `02`'ye döndüğü yerde `02 §…` de yanına yazılır.

**0.5.3 Sayısal değer yazılmaz, kimlik yazılır.** Süre, limit ve parametre değerleri `02 §4`, §8.2 ve §11'de yaşar; akış *"Z-8 dolunca"*, *"L-1 aşılırsa"*, *"P-8 kadar"* diye yazar. Yalnız yasal sabit — cayma penceresinin on dört günü gibi, `02 §11`'e girmeyen — metin içinde adıyla anılır ve süre kimliği yanına yazılır.

**0.5.4 Belirsiz ifade kullanılmaz.** *"Muhtemelen"*, *"belki"*, *"gerekirse karar verilir"* yazılmaz; yetkili bir seçimi anlatan cümle *"… yapabilir"* yerine işlemin kendisini ve koşulunu yazar (`GUARDRAILS §5`).

### 0.6 Boşluk, geri besleme ve devir

**0.6.1 Boşluk oturumda karara bağlanır.** Yazımın bulduğu boşluk K satırıyla, `checklists/document-stage.md` §3'ün satır biçimiyle ve K-648'in kayıt moduyla kapanır; ⚠ grubundaki karar `PRODUCT_DISCOVERY_STATUS.md` §8.1'in ⚠ listesine girer. Bu dokümanda "açık" işaretli adım bırakılmaz.

**0.6.2 Açık kalem ve devir.** Bu dokümanın şablonunda açık kararlar bölümü yoktur: karara bağlanmış bir varlığın yalnız **detayı** açık kalabilir ve tracker §4'te **A-19**'dan sürer, gözlemlenebilir bir kapıyla (K-37, `00 §N.1`). Sonraki aşamaya talimat bırakan karar hedef dokümanın başındaki **"Aşama 2'den park edilen girdiler"** bloğuna satır olarak girer — blok yoksa `05`'teki biçimiyle açılır (K-36; ilk örnek K-666, K-667). Aşama içi ileri referans karar satırının etki sütununda `(downstream)` olarak kalır.

**0.6.3 Geri besleme tablosu §11'dir.** `02`'ye ya da `10`'a dönen her karar §11'e bir satırla girer: bulgu, karar numarası, değişen bölümler ve sürüm.

---

## 1. Durum makinesi

> **Ne yazılır:** Tüm durumlar, izin verilen geçişler, yasak geçişler, her geçişin tetikleyicisi. **UI tasarımından önce netleşmelidir** — ekran varyantları buradan türetilir.

Durum adları, kodları ve geçişlerin koşulları `02 §5`'tedir ve burada tekrarlanmaz. Bu bölüm o makineyi akışa bağlar: her geçişi **kimin** tetiklediğini, **hangi akışta** gerçekleştiğini ve **kime bildirim** gittiğini yazar; durum çiftlerini, tip hatlarını, kalem kayıtlarını ve her rolün hangi durumda hangi işlemi yapabildiğini (§1.11) tek yerde toplar. "Akış" sütunları §0.2'nin alt bölümlerini gösterir.

### 1.1 Envanter

| Varlık | Durum seti | Kaynak | Bu bölümde |
|---|---|---|---|
| Sipariş — sevkiyat ekseni (`OrderStatus`) | Alındı · Hazırlanıyor · Kargoya verildi · Teslim edildi · Teslim edilemedi · İptal edildi | `02 §5.4` | §1.2 |
| Sipariş — ödeme ekseni (`PaymentStatus`) | Bekliyor · Ödendi · Başarısız · Kısmen geri ödendi · Geri ödendi | `02 §5.5` | §1.3 |
| Ürün ve varyant (`PublishStatus`) | Taslak · Yayında · Arşiv | `02 §5.1` | §1.8 |
| Kurumsal içerik kaydı (`PublishStatus`) | Taslak · Yayında | `02 §5.2` | §1.8 |
| Ayıp talebi (`DefectClaimStatus`) | Açık · Çözüldü | `02 §5.10` | §1.9 |
| İletişim talebi — KVKK talebi dahil (`ContactRequestStatus`) | Açık · Kapatıldı | `02 §5.13` | §1.9 |

**Durum makinesi taşımayanlar:** sipariş kalemi — kayıtlar taşır (§1.7) · indirim, kupon, stok ayırma, hesap, oturum ve yönetici daveti — zamana ya da kullanıma bağlı geçerlilik taşır (§1.10); duyuru yayın durumu taşır (§1.8) ve tarih aralığı ona ek bir görünürlük koşuludur (§1.10.4) · mağazanın satış açıklığı — dört koşullu bir kapıdır, varlık durumu değildir (`02 §3.1.5`, §5).

### 1.2 Sevkiyat ekseni — Sipariş durumu

| Durum | Anlamı | Sonraki olası durumlar | Tetikleyici |
|---|---|---|---|
| Alındı (`Placed`) | Sipariş müşterinin onayıyla doğdu; ödeme ya da — fiziksel açık kalemi olmayan siparişte — kalemlerin teslim işaretleri bekleniyor | Hazırlanıyor (S1) · Teslim edildi (S2) · İptal edildi (S3) | Ödeme onayı · son teslim işareti ya da kapanış · iptal |
| Hazırlanıyor (`Preparing`) | Ödenmiş, fiziksel kalem taşıyan sipariş hazırlanıyor; fiziksel kalemleri kapanmışsa kalan kalemlerin teslim işaretini bekler (§1.4.3). Kargoya verme süresi (Z-10) açık fiziksel kalem varken işler | Kargoya verildi (S5) · İptal edildi (S4) · Teslim edildi (S10) | Yöneticinin kargoya vermesi · iptal ya da kapanış · fiziksel kalemlerin kapanması |
| Kargoya verildi (`Shipped`) | Gönderi yolda; fiziksel kalemin iptal yolu kapandı, cayma düğmesi açıldı | Teslim edildi (S6) · Teslim edilemedi (S7) | Yöneticinin işareti |
| Teslim edilemedi (`DeliveryFailed`) | Gönderi firmaya geri döndü | Kargoya verildi (S8) · İptal edildi (S9) · Teslim edildi (S11) | Yöneticinin yeniden gönderimi · iptal ya da kapanış · kalemlerin kapanması |
| Teslim edildi (`Delivered`) | **Terminal.** Fiziksel siparişte teslim işareti ve tarihi; fiziksel açık kalemi olmayan siparişte açık kalemlerin tamamının teslim işareti | — | Yalnız yöneticinin düzeltmesi (§1.6) |
| İptal edildi (`Cancelled`) | **Terminal.** Teslimattan önce iptal edilmiş ya da açık kalemi kalmadan ve hiçbir kalemi teslim edilmeden kapanmış sipariş | — | Yalnız yöneticinin düzeltmesi (§1.6) |

**Geçişler — kim tetikler, hangi akışta, kime bildirim gider.** Koşullar `02 §5.4`'ün beyaz listesindedir; listede olmayan her geçiş yasaktır.

| # | Geçiş | Tetikleyen | Akış | Bildirim |
|---|---|---|---|---|
| S1 | Alındı → Hazırlanıyor | **Sistem**, ödeme onayıyla aynı anda (Ö1) — kartta sağlayıcının başarı bildirimi, havalede yöneticinin "ödendi" işareti; sipariş en az bir fiziksel kalem taşır | §2.5, §8.2 | B-4 → müşteri (Ö1'in); Hazırlanıyor'a geçiş ayrıca bildirim üretmez (`02 §9.2`) |
| S2 | Alındı → Teslim edildi | **Kapanış kuralını tamamlayan son olay** (§1.4.3) — yalnız dijital siparişte ödeme onayı (sistem); yöneticinin son hizmet kalemini "tamamlandı" işaretlemesi; son açık kalemin müşteri ya da yönetici tarafından iptali, çıkarılması ya da cayma beyanıyla kapanması | §2.5, §2.7, §2.8, §8.2–§8.4 | Ödeme onayında B-4; teslim işareti bildirim üretmez (`02 §9.2`); kapanışta olayın kendi bildirimi (B-7, B-9, B-11) |
| S3 | Alındı → İptal edildi | **Müşteri** siparişi iptal eder — ödeme beklenirken bütünüyle, ödenmişse son açık kalemi · **Yönetici** sebep seçerek iptal eder · **Sistem:** ödeme süresi dolar (Z-7, Z-8), kart ödemesi başarısız olur ya da aynı sepetten yeni sipariş onaylanır · **Kapanış** — son açık kalem cayma beyanıyla ya da çıkarmayla kapanır ve hiçbir kalem teslim edilmemiştir (K-672) | §2.4, §2.5, §2.7, §2.8, §4.1, §8.3, §8.4 | B-7 → müşteri; kapanışta B-7 gitmez — müşteri olayın kendi bildirimini almıştır (B-9, B-11) |
| S4 | Hazırlanıyor → İptal edildi | **Müşteri** ya da **yönetici** son açık kalemi iptal eder · **Kapanış** — gecikme feshi siparişte açık kalem bırakmaz (`02 §5.8`) ya da son açık kalem cayma beyanıyla veya çıkarmayla kapanır (K-672); kapanış sistemin, olayla aynı anda | §2.7, §2.8, §8.3, §8.4 | B-7 → müşteri; kapanışta gitmez (B-9, B-11) |
| S5 | Hazırlanıyor → Kargoya verildi | **Yönetici** kargoya verir — geri alınamaz onayıyla (§1.6); yalnız açık fiziksel kalemler gider | §8.2 | B-5 → müşteri |
| S6 | Kargoya verildi → Teslim edildi | **Yönetici** teslim işaretini ve teslim tarihini girer; kendiliğinden geçiş yoktur | §8.2 | — (`02 §9.2`) |
| S7 | Kargoya verildi → Teslim edilemedi | **Yönetici** geri dönen gönderiyi işaretler | §8.2 | B-6 → müşteri |
| S8 | Teslim edilemedi → Kargoya verildi | **Yönetici** açık fiziksel kalemleri yeniden gönderir — geri alınamaz onayıyla (K-677) | §8.2 | B-5 → müşteri, yeniden |
| S9 | Teslim edilemedi → İptal edildi | **Yönetici** sebep seçerek iptal eder · **Kapanış** — açık kalem kalmamışsa ve hiçbir kalem teslim edilmemişse yönetici S7'den sonra siparişi sebepsiz kapatır; son kalem hangi olayla kapanmış olursa olsun — cayma, gecikme feshi, kalem iptali ya da çıkarma (`02 §5.4` S9, §5.8; K-717) | §2.7, §2.10, §8.3 | B-7 → müşteri; kapanışta gitmez (B-9) |
| S10 | Hazırlanıyor → Teslim edildi | **Kapanış kuralını tamamlayan son olay** (§1.4.3) — son açık fiziksel kalemin iptali, çıkarılması ya da gecikme feshi; son açık hizmet kaleminin iptali, çıkarılması, "tamamlandı" işareti ya da cayma beyanı | §2.7, §2.8, §8.2–§8.4 | Olayın kendi bildirimi |
| S11 | Teslim edilemedi → Teslim edildi | **Kapanış kuralını tamamlayan son olay** (§1.4.3) — S7'nin kendisi dahil: geri dönen fiziksel kalemler iptal, cayma beyanı ya da fesihle kapanmışsa | §2.7, §2.8, §2.10, §8.2, §8.3, §8.4 | Olayın kendi bildirimi (S7'de B-6) |

**Yasak geçişler:** geri yönlü her geçiş — Kargoya verildi → Hazırlanıyor, Teslim edildi'den ve İptal edildi'den herhangi bir duruma — müşteri akışında yoktur (`02 §5.4`, §7.6.3). Yöneticinin düzeltmesi beyaz listenin dışındadır ve sınırları §1.6'dadır.

### 1.3 Ödeme ekseni — Ödeme durumu

| Durum | Anlamı | Sonraki olası durumlar | Tetikleyici |
|---|---|---|---|
| Bekliyor (`Pending`) | Ödeme gelmedi | Ödendi (Ö1) · Başarısız (Ö2) | Ödeme onayı · ödenmeden iptal |
| Ödendi (`Paid`) | Ödeme onaylandı | Kısmen geri ödendi (Ö3) · Geri ödendi (Ö4) | Geri ödeme |
| Başarısız (`Failed`) | **Terminal.** Ödeme alınmadan kapandı — sebep iptal kaydındadır | — | — |
| Kısmen geri ödendi (`PartiallyRefunded`) | Paranın bir kısmı geri ödendi | Geri ödendi (Ö5) | Geri ödeme |
| Geri ödendi (`Refunded`) | **Terminal.** Paranın tamamı geri ödendi | — | — |

| # | Geçiş | Tetikleyen | Akış | Bildirim |
|---|---|---|---|---|
| Ö1 | Bekliyor → Ödendi | **Sistem** — kartta sağlayıcının başarı bildirimi ya da ödeme süresi dolduğunda son sorgunun olumlu cevabı (`02 §6.1.2`) · **Yönetici** — havalede "ödendi" işareti, geri alınamaz onayıyla. Eşzamanlı geçişler §1.4.2'de | §2.5, §8.2 | B-4 → müşteri |
| Ö2 | Bekliyor → Başarısız | S3 ile aynı anda — ödenmemiş siparişin her iptali: sistemin, müşterinin ya da yöneticinin | §2.5, §2.7, §4.1, §8.3 | S3'ün bildirimi (B-7) |
| Ö3 | Ödendi → Kısmen geri ödendi | **Sistem** — iptal edilen, çıkarılan ya da gecikme feshine uğrayan ödenmiş kart kaleminin iadesi sağlayıcıda gerçekleşir · **Yönetici** — havale hattında geri ödemeyi işler; caymada iki hatta da işler; ayıp talebinin para gerektiren çözümünde ve tutar bazlı kısmi geri ödemede işler — geri alınamaz onayıyla | §2.7–§2.9, §8.3, §8.4 | B-8 → müşteri; sistemin kendiliğinden yaptığı kart iadesinde F-4 → firma |
| Ö4 | Ödendi → Geri ödendi | Ö3'ün tetikleyenleri; siparişin parasının tamamı geri ödenir | §2.7–§2.9, §8.3 | B-8 → müşteri; sistemin kart iadesinde F-4 → firma |
| Ö5 | Kısmen geri ödendi → Geri ödendi | Ö3'ün tetikleyenleri; kalan paranın tamamı geri ödenir | §2.7–§2.9, §8.3 | B-8 → müşteri; sistemin kart iadesinde F-4 → firma |

**Ekseni değiştirmeyen olaylar** (`02 §5.5`): kart iadesinin sağlayıcıda başarısız olması — panel işareti düşer (§1.7); müşteriye ayrı bildirim gitmez (K-683) · iptal edilmiş siparişe gelen kart ödemesi ve sistemin onu kendiliğinden geri ödemesi — ödeme kaydında durur, B-8 → müşteri (K-684), F-4 → firma (`02 §6.2.16`) · ters ibraz — ürünün dışındadır (`02 §3.21.11`) · cayma beyanı, gecikme feshi ve iade malının teslim alınması — para fiilen gönderilene kadar (`02 §5.6.6`). **Yasak geçiş:** Başarısız → Ödendi. Ödeme ekseninin tek düzeltmesi yanlış konmuş havale "ödendi" işaretidir (§1.6).

### 1.4 İki eksenin bağı

**1.4.1 Birleşim tablosu.** Her siparişin her an iki durumu vardır (`02 §5.3.1`). ✓ izinli, ✗ ulaşılamaz ya da yasak çifttir; harfler tablonun altındadır.

| Sevkiyat ↓ · Ödeme → | Bekliyor | Ödendi | Başarısız | Kısmen geri ödendi | Geri ödendi |
|---|---|---|---|---|---|
| Alındı | ✓ (a) | ✓ (b) | ✗ (e) | ✓ (b) | ✓ (f) |
| Hazırlanıyor | ✗ (c) | ✓ | ✗ (e) | ✓ | ✓ (f) |
| Kargoya verildi | ✗ (c) | ✓ | ✗ (e) | ✓ | ✓ (f) |
| Teslim edilemedi | ✗ (c) | ✓ | ✗ (e) | ✓ | ✓ (f) |
| Teslim edildi | ✗ (d) | ✓ | ✗ (d) | ✓ | ✓ |
| İptal edildi | ✗ (g) | ✓ (h) | ✓ | ✓ (h) | ✓ |

(a) Yeni sipariş; ödeme bekleniyor. (b) Yalnız fiziksel açık kalemi olmayan sipariş — kalemlerin teslim işaretleri bekleniyor; fiziksel kalemli sipariş ödeme onayıyla aynı anda Hazırlanıyor'a geçer (S1). (c) Kapı koşulu: Hazırlanıyor'a ancak ödeme onaylanınca geçilir; ödeme Bekliyor'a yalnız havale işaretinin düzeltilmesiyle ve sevkiyatla birlikte döner (`02 §5.6.1`; §1.6). (d) `02 §5.6.1`'in adıyla yasakladığı iki birleşim. (e) Başarısız yalnız ödenmeden iptalle, İptal edildi'ye geçişle aynı anda doğar (`02 §5.6.3`, §5.6.5). (f) Sipariş kapanmadan siparişin bütün parası geri ödenmişse: Alındı ve Hazırlanıyor'da açık kalem varken tutar bazlı geri ödemeyle — tutar ödenmiş ve geri ödenmemiş tutarı aşamaz (`02 §5.5`); Kargoya verildi ve Teslim edilemedi'de ayrıca kargodayken cayılan ya da feshedilen kalemlerin geri ödemesiyle — geri ödeme sevkiyattan bağımsızdır ve sipariş yöneticinin S7 ve S9 adımını bekler (`02 §5.6.1`, §5.8). (g) Ödenmeden iptal ödemeyi Başarısız yapar (`02 §5.6.5`); kart son sorgusu yanıtsız kaldığında sipariş iptal edilmez, Alındı + Bekliyor'da kalır (`02 §6.1.2`). (h) Ödendi sütununda: geri ödemesi henüz gerçekleşmemiş iptal ya da kapanış — havalede yöneticinin işlemesi bekleniyor, kartta iade sağlayıcıda gerçekleşmedi; yanlış firma iptalinin düzeltilmesi yalnız bu hâlde yapılır (§1.6.2.2; `02 §10.4.6`). Kısmen geri ödendi sütununda: kısmen geri ödenmiş siparişin iptali ya da kapanışı — geri ödemenin kalanı bekleniyor — ya da gidiş kargosu geri ödenmeyen "gönderi teslim edilemedi — müşteri kaynaklı" iptali, kalıcı son hâl (`02 §7.2.9`); düzeltilmez.

**1.4.2 Eşzamanlı geçişler** — iki ekseni ya da ekseni ve kalemi aynı anda değiştiren olaylar (`02 §5.6`):

| # | Olay | Aynı anda olan |
|---|---|---|
| 1.4.2.1 | Ödeme onayı (Ö1) | Fiziksel açık kalemi olan siparişte S1 · yalnız dijital siparişte S2 · her siparişte dijital kalemlerin teslim işareti — fiziksel kalemsiz siparişte kapanış kuralı (§1.4.3) |
| 1.4.2.2 | Ödenmemiş siparişin iptali — sistemin, müşterinin ya da yöneticinin | S3 + Ö2 |
| 1.4.2.3 | Teslim işareti almamış son açık kalemin kapanması ya da teslim işareti alması | Kapanış kuralı (§1.4.3) |
| 1.4.2.4 | Havale "ödendi" işaretinin düzeltilmesi (yönetici) | Ödendi → Bekliyor ve Hazırlanıyor → Alındı birlikte (§1.6) |

**1.4.3 Kapanış kuralı.** "Açık kalem" `02 §5.8`'in tanımıdır: iptal edilmemiş, çıkarılmamış, cayma beyanıyla ya da gecikme feshiyle kapanmamış kalem. Tanım adetle okunur: kalem açık adedi kaldıkça açıktır — adetlerinin bir kısmı kapanmış kalem açık kalır — ve aşağıdaki "kapanır" ile "son açık kalem" kalemin son açık adedini anlatır (§1.7.4; `02 §7.1.6`; K-790). Fiziksel açık kalem taşıyan sipariş fiziksel hattı izler ve Teslim edildi'ye S6 ile geçer; dijital ve hizmet kalemleri onu beklemez (`02 §5.7`). **Fiziksel açık kalemi olmayan siparişte** — baştan fiziksel kalemsiz ya da fiziksel kalemleri kapanmış — teslim işareti almamış son açık kalem kapandığı ya da teslim işaretini aldığı anda sipariş hattını bitirir. Akışlardaki "son açık kalem" bu kısaltmayla okunur: teslim işareti almamış son açık kalem.

- **En az bir kalem teslim edilmişse** → Teslim edildi: Alındı'dan S2, Hazırlanıyor'dan S10, Teslim edilemedi'den S11 (`02 §5.4`, §5.6.6).
- **Hiçbir kalem teslim edilmemişse** → İptal edildi. Alındı ve Hazırlanıyor'da son olay müşterinin ya da yöneticinin kalem iptaliyse geçiş o iptaldir (S3, S4) ve B-7 gider; son olay cayma beyanı, gecikme feshi ya da çıkarmaysa geçiş bir **kapanıştır**: sebep seçilmez, iptal kaydı ve ikinci bir geri ödeme açılmaz, B-7 gitmez — kapanış sistemin, olayla aynı anda işler (S3, S4 — `02 §5.8`, K-672). **Kargoya verildi ve Teslim edilemedi'de** sipariş gönderinin dönüşünü bekler. Teslim edilemedi'de yöneticinin geri dönen kalemleri sebep seçerek iptal etmesi firma iptalidir (S9, B-7 — 1.11.21). Son açık kalem başka bir olayla — cayma beyanı, gecikme feshi, hizmet kaleminin iptali ya da çıkarılması — kapanmışsa geçiş yöneticinin kapanışıdır: Kargoya verildi'de önce S7'yi işaretler, ardından S9'un kapanışıyla kapatır (1.11.40); kapanışın sebebi kalemlerin kendi kayıtlarıdır ve B-7 gitmez — kalem iptalinin B-7'si iptal anında gitmiştir (`02 §5.4` S9, §5.8; K-717).

Kart son sorgusu yanıtsızken ve kesintide dolan havale süresinde iptal ertelenir; o arada sipariş Alındı + Bekliyor'da kalır (`02 §6.1.2`, §3.21.8; K-667).

**1.4.4 Geçişlerin sayaçlara etkisi** — stok, hizmet kontenjanı ve kupon hakkı (`02 §4.3`):

| # | An | Stok ve hizmet kontenjanı | Kupon hakkı | Kaynak |
|---|---|---|---|---|
| 1.4.4.1 | Sipariş oluşur (Alındı + Bekliyor) | Ayrılır | Ayrılır | `02 §3.17.3`, §5.11.3 |
| 1.4.4.2 | Ödeme onayı (Ö1) | Ayrılan kesin düşer | Kullanılmış sayılır | `02 §4.3` |
| 1.4.4.3 | Ödenmeden iptal (S3 + Ö2) | Serbest kalır | Geri döner | `02 §5.6.3` |
| 1.4.4.4 | Ödenmiş kalemin iptali ve çıkarma | İptal edilen ya da çıkarılan adet stoğa, kontenjan havuza kendiliğinden döner | Yalnız siparişin tamamı — bütün kalemlerin bütün adetleri — iptal edildiğinde döner | `02 §7.2.6`, §10.4.3 · K-791 |
| 1.4.4.5 | Gecikme feshi | Kargoya verilmemiş kalemde kendiliğinden döner; kargodan dönen mal yöneticinin teslim alma adımıyla eklenir | Sipariş kapanışla İptal edildi'ye geçerse döner — feshedilen kalem kapanışta iptal edilmiş sayılır; teslim edilmiş kalem varsa dönmez | `02 §5.8`, §5.11.3, §7.2.3, §7.2.7 |
| 1.4.4.6 | Cayma ve iade | Fiziksel mal yöneticinin teslim aldığı adet kadar, eklemesiyle döner, reddedilen mal dönmez; hizmet kontenjanı caymada cayılan adet kadar döner | Yalnız siparişin tamamı — bütün kalemlerin bütün adetleri — iade edildiğinde döner | `02 §7.4.7`–§7.4.9 · K-791 |
| 1.4.4.7 | Yanlış iptalin düzeltilmesi | Yeniden ayrılır; yetmezse düzeltme yapılamaz | Yeniden kullanılmış sayılır | `02 §10.4.6` |
| 1.4.4.8 | Havale işaretinin düzeltilmesi | Tutulu kalır | Tutulu kalır | `02 §10.4.6` |
| 1.4.4.9 | Varyantın kalıcı silinmesi | Silinen varyantın ayırması düşer | — | `02 §3.7.7` · K-655 |

### 1.5 Tipe göre hatlar

Hatların tanımı `02 §5.7`'dedir; tipler için ayrı durum seti yoktur. Tablo her hattın olağan dizisini, adımı kimin attığını ve yöneticinin zorunlu elle adım sayısını gösterir (`02 §10.5.1`; bütçe §8.5).

| # | Hat | Olağan dizi | Zorunlu elle adım |
|---|---|---|---|
| 1.5.1 | Fiziksel — kart | Alındı + Bekliyor → *(sistem: Ö1, S1)* Hazırlanıyor + Ödendi → *(yönetici: S5)* Kargoya verildi → *(yönetici: S6)* Teslim edildi | 2 |
| 1.5.2 | Fiziksel — havale | Kart hattı; Ö1'i yöneticinin "ödendi" işareti tetikler | 3 |
| 1.5.3 | Yalnız dijital | Alındı + Bekliyor → *(Ö1, S2 — kartta sistem, havalede yönetici)* Teslim edildi + Ödendi | 0 · havalede 1 |
| 1.5.4 | Yalnız hizmet | Alındı + Bekliyor → *(Ö1)* Alındı + Ödendi → *(yönetici: son "tamamlandı", S2)* Teslim edildi | 1 · havalede 2 |
| 1.5.5 | Dijital + hizmet | Alındı + Bekliyor → *(Ö1; dijital kalemler teslim)* Alındı + Ödendi → *(yönetici: son "tamamlandı", S2)* Teslim edildi | 1 · havalede 2 |
| 1.5.6 | Karışık, fiziksel kalemli | Fiziksel hat; dijital kalem ödeme onayında, hizmet kalemi "tamamlandı" işaretinde kalem düzeyinde teslim edilir | Hatların adımları toplanır |
| 1.5.7 | Fiziksel kalemleri kapanmış karışık | Bulunduğu durumda — Alındı, Hazırlanıyor ya da Teslim edilemedi — kapanış kuralıyla biter (§1.4.3) | Kalan hatların adımları |

### 1.6 Geri alınamaz geçişler, onay pencereleri ve düzeltme

**1.6.1 Geri alınamaz onay isteyen işlemler.** Yönetici işlemi onaylamadan önce sonucunu tek cümleyle görür; panel "geri al" düğmesi sunmaz (`02 §5.9`, §7.6.1).

| # | İşlem | Geçiş ya da kayıt | Akış |
|---|---|---|---|
| 1.6.1.1 | Havale "ödendi" işareti | Ö1 ve eşzamanlı geçişleri; dijital kalemli siparişte işaretin düzeltilemeyeceği önceden söylenir (`02 §10.4.6`) | §8.2 |
| 1.6.1.2 | Kargoya verme ve yeniden gönderim | S5, S8 (K-677) | §8.2 |
| 1.6.1.3 | Firma iptali — sipariş ya da kalem | S3, S4, S9 ya da kalemin iptal kaydı; "stokta bulunamadı" sebebinde yasal uyarı (`02 §7.2.4`) | §8.3 |
| 1.6.1.4 | Geri ödemenin yönetici tarafından işlenmesi — tutar bazlı kısmi geri ödeme dahil | Ö3–Ö5 | §8.3, §8.4 |
| 1.6.1.5 | Kart hattında karta para gönderen kalem çıkarma | Çıkarma kaydı ve sistemin kart iadesi | §8.4 |
| 1.6.1.6 | İade reddi | Kalemin iade reddi kaydı — onay sonucunu tek cümleyle söyler (`02 §7.4.9`; K-656) | §8.3 |
| 1.6.1.7 | Başka kanaldan gelen cayma ya da gecikme feshi bildiriminin kaydı | Kalemin cayma beyanı ya da gecikme feshi — kayıt geri alınmaz ve düzeltilmez (`02 §5.9`; K-688, K-715) | §8.4 |
| 1.6.1.8 | Üye hesabının talep üzerine silinmesi | Hesap silinir — geri alınmaz (`02 §5.9`; K-715) | §9.4 |

Sistemin iptalde kendiliğinden başlattığı kart iadesi bir onay penceresi değildir; firmaya F-4 ile bildirilir (`02 §5.9`). Siparişe dokunmayan onaylar — kalıcı silme, yönetici kaldırma, ayar değişiklikleri — kendi akışlarındadır (§8.1, §8.7, §8.8). **Müşterinin geri alınamaz dört işlemi** — siparişin ya da kalemin iptali (§2.7.1–§2.7.3), gecikme nedeniyle fesih (§2.7.7), cayma beyanı (§2.8.1.2, §2.8.2.2) ve hesabın silinmesi (§9.4.1) — de tek dokunuşla tamamlanmaz: müşteri son adımda sonucu tek cümleyle görür ve işlemi onaylayarak tamamlar. Onay işlemin kendi ekranının son adımıdır; bekleme süresi, vazgeçirme metni ya da ikinci bir kanal istemez (`02 §5.9`; K-732).

**1.6.2 Düzeltme.** Yanlış yapılmış geçiş yöneticinin panelden, kapalı listeden sebep seçerek yaptığı **yeni bir geçişle** düzeltilir; hem yanlış geçiş hem düzeltme işlem izinde kalır, dış dünyaya çıkmış sonuç — gönderilen e-posta, yola çıkan paket, karta gönderilen para — geri alınmaz ve her düzeltme müşteriye B-13 ile bildirilir (`02 §10.4.6`). Akış: §8.4.

| # | Düzeltilen | Sipariş nereye döner | Koşul |
|---|---|---|---|
| 1.6.2.1 | Erken ya da yanlış konmuş teslim işareti | Teslim edildi'den bir önceki duruma; teslim işareti kalkar, cayma penceresi ve ayıp süresi yeni tarihe kadar işlemez | `02 §10.4.6` |
| 1.6.2.2 | Yanlış iptal | İptal edildi'den iptalden önceki duruma | Yanlış siparişte yapılmış firma iptali, ödeme Ödendi'de kalmışsa; ayırmalar yeniden yapılır, yetmezse düzeltme yapılamaz. Kapanış — açık kalemi kalmamış siparişin İptal edildi'ye geçişi — bu düzeltmenin konusu değildir — `02 §10.4.6` · K-703 |
| 1.6.2.3 | Yanlış konmuş Kargoya verildi ya da Teslim edilemedi işareti | Yanlış geçişten önceki duruma | Eksen bağını bozmaz; o aralıkta doğmuş cayma beyanı ve ayıp talebi kalır, açık kalem kalmamışsa sipariş düzeltmeyle aynı anda kapanış kuralından geçer (§1.4.3) — `02 §5.4`, §10.4.6 · K-703 |
| 1.6.2.4 | Para gelmeden konmuş havale "ödendi" işareti | Ödendi → Bekliyor ve Hazırlanıyor → Alındı, birlikte | Kargoya verilmemiş, geri ödeme yapılmamış, hiçbir kalem teslim işareti almamış ve hiçbir kalemde iptal, çıkarma, gecikme feshi ya da cayma kaydı yokken — kayıt varsa düzeltme yapılamaz (K-703); Z-8 düzeltme anından yeniden başlar ve hatırlatma (B-3) yeni sürenin son iş gününde bir kez daha gider — `02 §10.4.6` · K-685, K-703 |

Kart hattında ödeme ekseninin düzeltmesi yoktur. **Cayma beyanı düzeltilmez ve geri alınmaz** — müşterinin sipariş sayfasından yaptığı da, yöneticinin başka kanaldan kaydettiği de; kandırılmayla ya da yanlış kalemde yapılmış kaydın sonucu firmadadır (K-688; akış §6.2.3). Teslim tarihinin, iade malının ulaşma tarihinin (K-712), adresin ve takip bilgisinin düzeltilmesi geçiş değil müdahaledir (§1.11, §8.4).

### 1.7 Kalem kayıtları ve panel işaretleri

**1.7.1 Kalemin kayıtları.** Kalem durum makinesi taşımaz; aşağıdaki kayıtlar onun yolunu anlatır (`02 §5.8`). "Hatta sayılışı" kaydın sevkiyat ekseninin kapanış kuralına (§1.4.3) etkisidir.

| # | Kayıt | Doğduğu an — kim | Hatta sayılışı | Bildirim | Kaynak |
|---|---|---|---|---|---|
| 1.7.1.1 | Teslim işareti ve teslim tarihi | Fiziksel: yöneticinin S6'sı, sipariş düzeyinde bir tarih · dijital: ödeme onayı, sistem · hizmet: yöneticinin "tamamlandı" işareti | Teslim edilmiş | Dijitalde B-4; öteki hâllerde — (`02 §9.2`) | `02 §5.8`, §3.20.9 |
| 1.7.1.2 | İptal kaydı | Müşteri ya da yönetici — yöneticide sebepli; iptal edilen adetle (K-794) | Kapanmış | B-7 | `02 §7.2` · K-794 |
| 1.7.1.3 | Çıkarma kaydı | Yönetici | İptal edilmiş gibi | B-11 | `02 §10.4.3` |
| 1.7.1.4 | Gecikme feshi | Müşteri sipariş sayfasından ya da yönetici başka kanaldan gelen bildirimi kaydederek (K-706); teslim edilmemiş fiziksel kalemlerin açık adetlerinin tamamına — seçim yoktur | Kapanmış — kargoya verilmez, yeniden gönderilmez, firma iptali uygulanmaz | B-9 → müşteri · müşterinin feshinde F-3 → firma; yöneticinin kaydında firmaya — (`02 §9.3.2`) | `02 §5.8`, §7.2.3, §10.4.10 · K-706 |
| 1.7.1.5 | Cayma beyanı | Müşteri sipariş sayfasından ya da yönetici başka kanaldan gelen bildirimi kaydederek; cayılan adetle ve fiziksel kalemde iade adresiyle — aynı kalemde ardışık beyanlar ayrı kayıtlardır (K-790) | Teslim işaretsiz kalemde cayılan adetleri kapatır; teslim edilmiş kalemde iade sürecini başlatır | B-9 → müşteri · müşterinin beyanında F-3 → firma; yöneticinin kaydında firmaya — (`02 §9.3.2`) | `02 §5.8`, §7.3, §10.4.10 · K-790 |
| 1.7.1.6 | İade teslim alma | Yönetici — ulaşma tarihiyle ve ulaşan adetle; parça parça ulaşan malda her teslim alma ayrı kayıttır (K-794); tarih müdahaleyle düzeltilir (K-712); stoğa ekleme ayrı adımdır | — | — · IBAN silinmişse B-14 | `02 §7.4.7` · K-712, K-794 |
| 1.7.1.7 | İade reddi | Yönetici — koşullu istisna kaleminde, teslim alma adımında | — | B-16 | `02 §7.4.9` · K-656 |
| 1.7.1.8 | "Mal dönmedi" kapanışı | Yönetici — Z-42 geçtikten sonra, beyanın ulaşmamış adetlerine (K-794) | — | — (`02 §9.2`) | `02 §10.4.9` · K-794 |
| 1.7.1.9 | IBAN isteği | Sistem — yöneticinin şu adımlarıyla birlikte: teslim alma (IBAN silinmişse) · "havale gerçekleşmedi" bildirimi · IBAN'sız cayma ya da gecikme feshi kaydı (K-706) · kart iadesinde havale yolunu açma · havale hattında ödenmiş kalemin firma iptali ya da çıkarması (K-690). **Yönetici** — havale hattında IBAN'ı olmayan bir geri ödeme için isteği kendisi açar (1.11.39; K-686) | — | B-14 | `02 §5.8`, §7.4.5 · K-686, K-690, K-706 |
| 1.7.1.10 | Ayıp talebi | Müşteri — ayıplı adetle (K-793) | Hattı etkilemez; durumu §1.9 | B-9 → müşteri · F-3 → firma | `02 §5.8`, §7.5 · K-793 |
| 1.7.1.11 | İndirme sayacı | Sistem her indirmede; yönetici yeniler | — | — | `02 §3.12.5`, §3.12.6 |

**1.7.2 Kalemin yolu tipe göre.**

- **Fiziksel kalem:** oluşur → ödemeden sonra iptal ya da çıkarma — Kargoya verildi'ye kadar; Z-11 dolunca gecikme feshi — Hazırlanıyor'da da → kargoya verilir → kargodayken gecikme feshi ya da cayma beyanı → teslim işareti → pencere içinde cayma beyanı → iade teslim alma · iade reddi · "mal dönmedi" kapanışı — kapanıştan sonra ulaşan mal teslim almayla kalemi yeniden açar. Ayıp talebi Kargoya verildi'den açılır; süre teslim tarihinden iki yıldır (Z-18).
- **Dijital kalem:** oluşur → ödeme onayında teslim işareti ve indirme. İptal ve cayma yoktur; ayıp talebi açıktır.
- **Hizmet kalemi:** oluşur → ödemeden sonra iptal, çıkarma ya da cayma beyanı — "tamamlandı" işaretine kadar; cayma ayrıca Z-14'ün dolmasına kadar, hangisi önce gelirse → "tamamlandı" işareti. Ayıp talebi tamamlamadan sonra açılır.

**1.7.3 Panel işaretleri.** İşaret bir durum değildir; doğuran koşul sürdükçe görünür ve koşul kalkınca kalkar (K-675).

| # | İşaret | Nerede | Düşer | Kalkar | Kaynak |
|---|---|---|---|---|---|
| 1.7.3.1 | "e-posta ulaşmadı" | Siparişin satırı — ayıp talebinin bildiriminde talebin satırında da görünür — ya da davetin satırı; firmaya giden bildirim satıra işaret düşürmez | Müşteriye giden bildirimde ya da davette üç yeniden deneme başarısız olunca (Z-26) | Siparişte: aynı e-posta panelden yeniden gönderilip gönderim başarılı olunca; yeniden gönderilmezse kalır. Davette: davet yeni davetle ya da geri çekilerek geçersizleşince satırıyla birlikte kalkar — davet e-postası yeniden gönderilmez (§8.8.1) | `02 §9.1.6`, §9.4 · K-675, K-759, K-760 |
| 1.7.3.2 | "ödeme sonucu alınamadı" | Siparişin satırı | Kart son sorgusu yanıtsız kalınca | Sorgunun sonucu gelince — olağan geçiş (Ö1 ya da S3 + Ö2) işler | `02 §6.1.2` · K-675 |
| 1.7.3.3 | "geri ödeme gerçekleşmedi" | Siparişin satırı | Kart iadesi sağlayıcıda başarısız olunca | Kart iadesi gerçekleşince, müşterinin IBAN'ına geri ödeme işlenince ya da yanlış iptal düzeltilince | `02 §5.5`, §7.2.8, §10.4.6 · K-675 |
| 1.7.3.4 | "IBAN bekleniyor" | Kalem — panelde liste | IBAN isteğiyle | Müşteri IBAN'ı sipariş sayfasından girince | `02 §7.4.5` |
| 1.7.3.5 | "iade malı bekleniyor" | Kalem | Teslimden sonraki cayma beyanıyla | İade teslim alma, iade reddi ya da "mal dönmedi" kapanışıyla — beyanın bütün adetleri için; bir kısmı teslim alınmışsa kalan adetler için sürer | `02 §7.4.1`, §7.4.7 · K-675, K-794 |
| 1.7.3.6 | "e-posta düzeltildi" | Siparişin satırı | Misafir siparişinin e-postası düzeltilince | Kalkmaz — siparişin e-posta geçmişiyle birlikte imha edilir | `02 §10.4.11` · K-675 |

Bekleyen işler sayaçları — hangi kalemin "iade ve geri ödeme bekleyen" sayılıp hangisinin sayılmadığı — `02 §10.6.1`'dedir; akışı §8.9.

**1.7.4 Kalemin adedi (K-787, K-790).** Adedi birden büyük kalemde işlemin birimi adettir: müşteri iptalde, caymada ve ayıp talebinde, yönetici firma iptalinde, çıkarmada, iade teslim almada, iade reddinde ve "mal dönmedi" kapanışında işleme giren adedi seçer; seçilebilecek adet o işleme açık adettir ve aşan değer kabul edilmez. 1.7.1'in kayıtları — teslim işareti ve indirme sayacı dışında — kapsadıkları adedi taşır ve aynı türden kayıt aynı kalemde birden çok kez doğar: önce bir adetten, sonra bir adetten daha caymak iki ayrı beyandır; her biri kendi damgasını, iade adresini, gönderme süresini (Z-42), ulaşma tarihini ve geri ödemesini taşır. Kalemin **açık adedi**, adedinden iptal, çıkarma, gecikme feshi ve cayma beyanıyla kapanmış adetler düşülerek okunur; "Hatta sayılışı" sütunu kaydın kapsadığı adetlere uygulanır ve kalem açık adedi sıfıra inince kapanmış sayılır. Yeni durum yoktur: sevkiyat ve ödeme durumlarının adları değişmez (`02 §5.4`, §5.5) ve kalem durum taşımaz (K-179). **Gecikme feshinde seçim yoktur** — fesih teslim edilmemiş fiziksel kalemlerin açık adetlerinin tamamına uygulanır (`02 §7.2.3`; K-571, K-796). Dijital kalemin adedi birdir (`02 §3.12.4`); hizmet kaleminde "tamamlandı" işareti açık adetlerin tamamına düşer (K-792). Tutar `02 §7.1.7`'yle ayrılır (K-788); kargo ücreti adede bölünmez (K-789). Kural `02 §7.1.6`'dadır.

### 1.8 Yayın durumları

**1.8.1 Ürün ve varyant.** Ürün ve varyant üç değeri ayrı ayrı taşır; bütün geçişler yöneticinin panel işlemidir, bildirim üretmez ve işlem izine yazılır (`02 §3.7.1`, §5.1).

| Durum | Anlamı | Sonraki olası durumlar | Tetikleyici |
|---|---|---|---|
| Taslak (`Draft`) | Vitrinde yok; adresi ziyaretçiye "sayfa bulunamadı" döner, yöneticiye "Taslak" bandıyla görünür | Yayında · Arşiv | Yönetici — Yayında'ya geçiş yayın kapısına bağlıdır (`02 §3.7.3`, §3.7.4) |
| Yayında (`Published`) | Vitrinde, listelerde ve aramada; tükenmişlik ayrı bir durum değildir | Taslak · Arşiv | Yönetici — yayın kapısını bozan geçiş engellenir (1.8.2) |
| Arşiv (`Archived`) | Vitrinden çıkmış, kaydı korunan ürün; adresi "Bu ürün artık satılmıyor" sayfası döner | Taslak · Yayında | Yönetici — Yayında'ya geçiş yayın kapısına bağlıdır |

**1.8.2 Yayın kapısını bozan işlem engellenir;** panel işlemi durdurur, sebebini söyler ve yöneticiyi önce ürünü taslağa ya da arşive almaya yönlendirir: yayındaki ürünün son Yayında varyantının arşive ya da taslağa alınması · dijital üründe yayındaki bir varyantı dosyasız bırakacak dosya silme · **yayındaki ürünün son Yayında varyantının kalıcı silinmesi** (`02 §3.7.1`, §3.7.4, §3.7.8; K-674). Ürünün kendisinin kalıcı silinmesi bir durum değildir: kayıt katalogdan kalkar, açık siparişler donmuş kalemleriyle sürer ve panel onaydan önce onların sayısını söyler (`02 §3.7.7`). Akış: §8.1.

**1.8.3 Etkileri.** Vitrinden kalkan — arşive ya da taslağa alınan, kalıcı silinen — kalem sepetten çıkar ve müşteriye bir kez söylenir (`02 §3.16.7`; akış §2.3). Yayın durumu siparişi etkilemez; satışın geçici kapatılması yayın durumunu değiştirmez (`02 §5.1`).

**1.8.4 Kurumsal içerik kaydı.** İki değer kullanılır; arşiv, ileri tarihli yayın ve sürüm geçmişi yoktur (`02 §5.2`). Akış: §2.2 (ziyaretçi), §8.6 (yönetici).

| Durum | Anlamı | Sonraki olası durumlar | Tetikleyici |
|---|---|---|---|
| Taslak (`Draft`) | Sitede yok; yönetici "Taslak" bandıyla önizler | Yayında | Yönetici — zorunlu alanlar doluysa (`02 §3.27.13`) |
| Yayında (`Published`) | Sitede görünür; düzenleme kaydedildiği anda yayındadır | Taslak | Yönetici — yayın kapısını bozan düzenleme kaydedilmez (`02 §5.2`) |

Duyuru yayında olsa bile tarihleri girilmişse yalnız aralığında görünür (Z-23). Hakkımızda silinmez, yalnız taslağa alınır; öteki kayıtların silinmesi geri alınamaz uyarısıyla yapılır (`02 §5.2`, §7.6.4).

### 1.9 Talep durumları

**1.9.1 Ayıp talebi.** Kalem başına tek ayıp talebi kaydı vardır (K-676): kalemde talep yokken müşteri "sorun bildir"i kullanır; talep Açık'ken yeni talep açılmaz; talep Çözüldü'yken müşteri ve yönetici onu yeniden açar ve yeni sorun da aynı kayıtta bildirilir. Akış: §2.9 (müşteri), §8.4 (yönetici).

| Durum | Anlamı | Sonraki olası durumlar | Tetikleyici | Bildirim |
|---|---|---|---|---|
| Açık (`Open`) | Müşterinin bildirdiği, firmanın henüz çözmediği talep; talep bu durumda doğar | Çözüldü | Yönetici "çözüldü" işaretler | Doğuşta B-9 → müşteri, F-3 → firma; çözüldü işareti — (`02 §9.2`) |
| Çözüldü (`Resolved`) | Firmanın çözüldü işaretlediği talep | Açık | Müşteri ya da yönetici yeniden açar — kalemin iki yıllık süresi (Z-18) içinde | Müşterinin yeniden açmasında B-9 → müşteri ve F-3 → firma (K-681); yöneticininkinde — (`02 §9.2`) |

Seçimlik hakların yürütümü sistem dışıdır; para gerektiren çözüm §1.3'ün geri ödeme geçişleriyle işler (`02 §7.5.3`).

**1.9.2 İletişim talebi — KVKK talebi dahil.** Ayrı durum makinesi ve süre sayacı yoktur (`02 §5.13`). Akış: §2.2 (ziyaretçi), §8.6 (yönetici).

| Durum | Anlamı | Sonraki olası durumlar | Tetikleyici | Bildirim |
|---|---|---|---|---|
| Açık (`Open`) | Formdan gelen, firmanın kapatmadığı talep; talep bu durumda doğar | Kapatıldı | Yönetici kapatır | Doğuşta F-2 → firma |
| Kapatıldı (`Closed`) | Firmanın kapattığı talep | Açık | Yönetici yeniden açar | — |

Saklama süresi son kapatılıştan, hiç kapatılmamış talepte açılıştan işler (Z-33).

### 1.10 Durum makinesi olmayan varlıklar

Bu varlıkların durumu yoktur; geçerlilikleri zamana ya da kullanıma bağlıdır ve aşağıdaki olaylarla değişir.

| # | Varlık | Geçerliliği | Değiştiren olaylar | Kaynak · akış |
|---|---|---|---|---|
| 1.10.1 | İndirim | Tarih aralığının içinde (Z-20) | Sistem başlangıçta başlatır, bitişte bitirir; onay anında başlaması ya da bitmesi özet farkıdır | `02 §5.11.1` · §8.1 |
| 1.10.2 | Kupon | Tarih aralığının içinde (Z-22) ve kullanım hakkı kaldıkça | Kullanım adedi kullanılmış ve ayrılmış hakların altına indirilemez | `02 §5.11.2`, §3.10.6 · K-654 · §8.1 |
| 1.10.3 | Kupon hakkı ve stok ayırma | Ayrılır → kesin düşer ya da kullanılmış sayılır → serbest kalır ya da geri döner | §1.4.4'ün anları | `02 §3.17.3`, §5.11.3 |
| 1.10.4 | Duyurunun tarih aralığı | Yayın durumuna (§1.8) ek koşul: duyuru Yayında'yken — tarihleri girilmişse — aralığında görünür (Z-23) | Yöneticinin yayına alması ya da taslağa çekmesi | `02 §5.2` · §8.6 |
| 1.10.5 | Hesap — müşteri ve yönetici | Doğrulanmamış kayıt bir bekleme aşamasıdır: e-posta doğrulanana kadar giriş ve sipariş yoktur; doğrulanmış hesap silinene ya da kaldırılana kadar | Doğrulama; doğrulama bağlantısının ömrünün dolması (Z-1) — kayıt silinir, yeniden istenen bağlantı ömrü uzatmaz (L-9; K-659); e-posta değişikliği yeni adres doğrulanana kadar bekler — bekleyen değişiklik ilk bağlantının ömrüyle düşer (K-659) —, geçerli olduktan sonra eski adresteki bağlantıyla Z-45 boyunca geri alınır; Google ile ilk girişte eşleşen bekleyen kayıt devralınmaz — silinir ve hesap Google ile doğar (K-696); müşteri hesabının silinmesi — geri alma bağlantısı açıkken yapılmaz (K-698); yöneticinin kaldırılması; panele erişim kaybolduğunda kurulum tarafının yeni yönetici hesabı açması (K-699). Deneme limiti hesabın durumunu değiştirmez | `02 §5.12`, §3.13, §3.15, §10.2 · K-659, K-696, K-698, K-699 · §9.1–§9.4, §8.8, §10.4 |
| 1.10.6 | Oturum | "Beni hatırla" seçimine göre Z-4 ya da Z-5 | Şifre sıfırlama, hesabın silinmesi, e-posta değişikliğinin geri alınması ve yöneticinin kaldırılması hesabın bütün oturumlarını kapatır; şifre değiştirme değişikliği yapan dışındaki oturumları kapatır (K-695) | `02 §5.12.2` · K-695 · §9.2 |
| 1.10.7 | Yönetici daveti | Gönderiminden Z-3 boyunca, kullanılana kadar; tek kullanımlık | Kullanılınca yönetici hesabı doğar · geçersizleşir: Z-3 dolunca, geri çekilince, aynı adrese yeni davet gidince, gönderen yönetici kaldırılınca. Gönderim ve geri çekme işlem izine yazılır; gönderim ve göndereni kaldıran işlem bütün yöneticilere F-6 üretir (K-692) | `02 §10.2.1`, §10.2.2, §10.2.6 · K-660, K-661, K-662, K-692 · §8.8 |
| 1.10.8 | Mağazanın satış açıklığı | Dört koşulun hepsi sağlanırken | Bir koşulun düşmesi ya da geçici kapatma yeni siparişi durdurur; açık siparişleri etkilemez | `02 §3.1.5`, §3.1.6 · K-664 · §8.7 |

### 1.11 Durum × rol × işlem

Her rolün hangi durumda hangi işlemi yapabildiği. Ekran varyantları bu tablodan türetilir; işlemin kuralı "Kaynak"tadır. Bir işlemin açık olduğu aralığın dışında sipariş sayfası ve panel o işlemi **sunmaz**.

**Müşteri — sipariş sayfası** (akış §2.6–§2.9). İptal, cayma ve gecikme feshi yalnız **açık kalemde** (§1.4.3) sunulur; adedi birden büyük kalemde iptal, cayma ve ayıp talebi işleme giren adedin seçimiyle yapılır — seçilebilecek adet o işleme açık adettir; gecikme feshinde seçim yoktur (§1.7.4; K-787, K-796); fiziksel kalemde cayma ve ayıp talebi yalnız **kargoya verilmiş** kalemde sunulur — iptal, çıkarma ya da kargodan önce fesihle kapanmış kalemde sunulmaz (`02 §5.8`, §5.4 S5).

| # | İşlem | Açık olduğu aralık | Sonucu | Kaynak |
|---|---|---|---|---|
| 1.11.1 | Siparişi bütünüyle iptal etmek | Alındı + Bekliyor; ödeme onaylanınca kalem düzeyine geçer | S3 + Ö2 · B-7 | `02 §7.2.2` |
| 1.11.2 | Fiziksel kalemi iptal etmek | Ödeme onaylandıktan sonra, sipariş Kargoya verildi'ye geçene kadar | İptal kaydı · kapanış kuralı · geri ödeme Z-17 · B-7 | `02 §7.2.1`, §7.2.8 |
| 1.11.3 | Hizmet kalemini iptal etmek | Ödeme onaylandıktan sonra, kalem "tamamlandı" işaretini alana kadar — siparişin sevkiyat durumundan bağımsız | İptal kaydı · kapanış kuralı · B-7 | `02 §7.2.5` |
| 1.11.4 | Dijital kalemi iptal etmek | Yoktur — kalem ödeme onayında teslim edilir; önce yalnız 1.11.1 | — | `02 §7.2.5` |
| 1.11.5 | Fiziksel kalemden caymak | Sipariş Kargoya verildi'ye geçtikten sonra — Kargoya verildi, Teslim edilemedi, Teslim edildi — teslim tarihinden on dört gün dolana kadar (Z-13); teslim tarihi girilmemişse açık. Mutlak istisnada yoktur, koşullu istisnada koşulla açıktır | Cayma beyanı · teslim işaretsiz kalemde kapanış kuralı · B-9 | `02 §7.3.1`, §7.3.4, §7.3.7 |
| 1.11.6 | Hizmet kaleminden caymak | Ödeme onaylandıktan sonra (K-673), "tamamlandı" işaretine ya da Z-14'ün dolmasına kadar — hangisi önce gelirse | Cayma beyanı · cayılan adetler kapanır, kapanış kuralı · B-9 | `02 §7.3.3` · K-673, K-792 |
| 1.11.7 | Dijital kalemden caymak | Yoktur — hak ödeme onayında üçüncü onay kutusuyla düşer | — | `02 §7.3.2` |
| 1.11.8 | Gecikme nedeniyle fesih | Siparişin firmaya ulaşmasından Z-11 geçmiş ve teslim tarihi girilmemiş açık fiziksel kalemi varken — Hazırlanıyor, Kargoya verildi, Teslim edilemedi | Gecikme feshi kaydı · kapanış kuralı (§1.4.3) — Hazırlanıyor'da S4 kapanışı; Kargoya verildi ve Teslim edilemedi'de yöneticinin S7 ve S9 kapanışı (K-717) · B-9 | `02 §7.2.3`, §5.8 · K-717 |
| 1.11.9 | Ayıp talebi açmak ("sorun bildir") | Fiziksel kalemde sipariş Kargoya verildi'ye geçtikten, dijitalde ödeme onayından, hizmette "tamamlandı" işaretinden sonra; kalemde talep yokken (K-676); teslim işaretinin tarihinden iki yıl (Z-18) — teslim tarihi girilmemişse süre işlemez | Ayıp talebi — Açık · B-9 | `02 §7.5.1`, §7.5.2 · K-676 |
| 1.11.10 | Ayıp talebini yeniden açmak | Talep Çözüldü'yken, Z-18 içinde | Çözüldü → Açık · B-9 · F-3 | `02 §5.10`, §9.2 · K-681 |
| 1.11.11 | Geri ödeme IBAN'ını girmek | Havale hattında iptal, gecikme feshi ya da cayma beyanı sırasında; IBAN isteğinde ("IBAN bekleniyor"); kart iadesi gerçekleşmeyip yönetici havale yolunu açtığında — müşterinin seçimi | IBAN isteği kalkar | `02 §7.4.5`, §7.2.8 |
| 1.11.12 | Girilmiş IBAN'ı düzeltmek | IBAN girildikten sonra, yönetici geri ödemeyi işleyene kadar | — | `02 §7.4.5` |
| 1.11.13 | Havale bilgisini görmek | Havale siparişinde Alındı + Bekliyor — siparişe donmuş IBAN | — | `02 §3.21.5`, §3.22.4 · K-663 |
| 1.11.14 | Dijital dosyayı indirmek | Ödeme onaylandıktan sonra, kalemin indirme hakkı (P-8) kaldıkça | İndirme sayacı | `02 §3.12.5` |

**Yönetici — panel** (akış §8.2–§8.4)

| # | İşlem | Açık olduğu aralık | Sonucu | Kaynak |
|---|---|---|---|---|
| 1.11.15 | Havale "ödendi" işareti | Havale siparişinde Alındı + Bekliyor; iptalden sonra yoktur | Ö1 ve eşzamanlı geçişler · geri alınamaz · B-4 | `02 §3.21`, §5.9 |
| 1.11.16 | Kargoya vermek | Hazırlanıyor, açık fiziksel kalem varken | S5 · geri alınamaz · B-5 | `02 §5.4` |
| 1.11.17 | Teslim işaretini ve tarihini koymak | Kargoya verildi | S6 · — | `02 §3.20.11` |
| 1.11.18 | "Teslim edilemedi" işaretlemek | Kargoya verildi | S7 · B-6 | `02 §5.4` |
| 1.11.19 | Yeniden göndermek | Teslim edilemedi, açık fiziksel kalem varken | S8 · geri alınamaz (K-677) · B-5 | `02 §5.4` · K-677 |
| 1.11.20 | Hizmet kalemini "tamamlandı" işaretlemek | Ödeme onaylanmışken — Bekliyor ve Başarısız dışında (K-671) —, kalem açıkken | Teslim işareti · kapanış kuralı | `02 §3.20.9` · K-671 |
| 1.11.21 | Siparişi ya da kalemi iptal etmek (firma iptali) | Ödeme beklenirken yalnız siparişin tamamı (S3); ödemeden sonra kalem ve adet düzeyinde (K-794), kalem tipinin iptal sınırı içinde (1.11.2–1.11.4); Teslim edilemedi'de geri dönen açık fiziksel kalem de kalem düzeyinde iptal edilir — hiçbir kalem teslim edilmemişse sipariş S9 ile İptal edildi'ye, teslim edilmiş kalem varsa kapanış kuralıyla S11 ile Teslim edildi'ye geçer (`02 §5.4` S11). Cayma beyanlı ve feshedilmiş kaleme uygulanmaz | İptal kaydı, sebep · geri alınamaz · B-7 · havale hattında ödenmiş kalemde IBAN isteği ve B-14 (K-690) | `02 §7.2.4`, §5.4 · K-690, K-794 |
| 1.11.22 | Kalem çıkarmak ya da adedini azaltmak | Ödeme onaylandıktan sonra; fiziksel kalemde Kargoya verildi'ye kadar, dijital ve hizmet kaleminde teslim işaretine kadar | Çıkarma kaydı · kart hattında geri alınamaz · B-11 · havale hattında IBAN isteği ve B-14 (K-690) | `02 §10.4.3` · K-690 |
| 1.11.23 | Adresi düzeltmek | Sipariş Kargoya verildi'ye geçene kadar; fiziksel kalemsiz siparişte fatura adresi Teslim edildi'ye kadar | B-10 | `02 §10.4.2` |
| 1.11.24 | Kargo şirketini ve takip numarasını düzeltmek | Sipariş kargoya verildikten sonra | B-12 | `02 §10.4.4` |
| 1.11.25 | Teslim tarihini ya da iade malının ulaşma tarihini düzeltmek | Teslim tarihinde teslim işaretinden sonra; ulaşma tarihinde iade teslim almadan sonra (K-712) | Pencereler ya da geri ödeme süresi (Z-16) yeni tarihten işler · — | `02 §10.4.5` · K-712 |
| 1.11.26 | Durum geçişini düzeltmek | §1.6.2'nin satırları | B-13 | `02 §10.4.6` |
| 1.11.27 | İç not yazmak | Her durumda | — | `02 §10.4.7` |
| 1.11.28 | İade malını teslim almak | Cayma beyanlı ya da feshedilmiş fiziksel kalemin malı ulaşınca — ulaşan adetle; kalan adetler ulaşınca yeniden — "mal dönmedi" ile kapatılmış kalemde de (kalemi yeniden açar); ayıp talebinde sözleşmeden dönmede talebin ayıplı adetlerinin malı ulaşınca da (`02 §7.5.3`; K-808) | İade teslim alma · IBAN silinmişse IBAN isteği ve B-14 | `02 §7.4.7`, §7.5.3, §10.4.9 · K-794, K-808 |
| 1.11.29 | İade malını reddetmek | Koşullu istisna kaleminde, teslim alma adımında | İade reddi · geri alınamaz · B-16 | `02 §7.4.9` · K-656 |
| 1.11.30 | "Mal dönmedi" kapanışı | Teslimden sonraki caymada Z-42 geçmiş ve beyanın adetlerinden ulaşmamış olan varken — yalnız ulaşmamış adetlere | Kapanış · — | `02 §10.4.9` · K-794 |
| 1.11.31 | Geri ödemeyi işlemek | Havale hattında IBAN varken — yoksa önce 1.11.39; caymada iki hatta; ayıp talebinin para gerektiren çözümünde; tutar bazlı kısmi geri ödemede ödenmiş ve geri ödenmemiş tutar kaldıkça | Ö3–Ö5 · geri alınamaz · B-8 | `02 §5.5`, §7.4 · K-686 |
| 1.11.32 | Kart iadesini yeniden denemek ya da havale yolunu açmak | "geri ödeme gerçekleşmedi" işaretliyken; müşteri IBAN girdikten sonra kart iadesi yeniden denenmez | İşaret kalkar ya da IBAN isteği · B-14 | `02 §7.2.8` |
| 1.11.33 | "Havale gerçekleşmedi" bildirmek | Geri ödeme havalesi bankada gerçekleşmediğinde, geri ödeme işlenmeden önce | IBAN silinir, IBAN isteği · B-14 | `02 §7.4.5` |
| 1.11.34 | Başka kanaldan gelen caymayı kaydetmek | Fiziksel kalemde sipariş Kargoya verildi'ye geçtikten sonra — önce "müşteriyle anlaşıldı" iptaliyle; hizmet kaleminde ödeme onaylandıktan sonra — önce siparişin bütünüyle iptaliyle (K-673). Pencere dışında — süresinde gönderilmiş ve geç ulaşmış bildirim ya da eksik bilgilendirme (K-709) — ve mutlak istisnada ayrıca onayla; pencerenin bitiminden bir yıl (Z-13'ün yasal uzaması) sonrasına tarihli kayıt reddedilir | Cayma beyanı · geri alınamaz (K-715) · B-9 · IBAN'sızsa B-14 | `02 §10.4.10` · K-673, K-709, K-715 |
| 1.11.35 | Misafir siparişinin e-postasını düzeltmek | Giriş yapılmadan verilmiş siparişte, kişisel veriler imha edilene kadar | B-1 → yeni adres · B-15 → eski adres | `02 §10.4.11` |
| 1.11.36 | E-postayı yeniden göndermek | Sipariş "e-posta ulaşmadı" işaretliyken, ulaşmayan müşteri bildirimi (B-1…B-16) için — davet satırında yeniden gönderim yoktur, yol yeni davettir (§8.8.1) | İşaret kalkar (§1.7.3) | `02 §9.1.6` · K-759 |
| 1.11.37 | Ayıp talebini "çözüldü" işaretlemek ya da yeniden açmak | Açık ya da Çözüldü talepte | — | `02 §5.10` |
| 1.11.38 | İndirme hakkını yenilemek | Dijital kalemde | Sayaç sıfırlanır · — (K-683) | `02 §3.12.6` · K-683 |
| 1.11.39 | Geri ödeme için müşteriden IBAN istemek | Havale hattında IBAN'ı olmayan bir geri ödeme işlenecekken — ayıp talebinin para gerektiren çözümü, tutar bazlı kısmi geri ödeme, iade reddinden ya da "mal dönmedi" kapanışından sonra ödeme kararı | IBAN isteği · B-14 | `02 §7.4.5` · K-686 |
| 1.11.40 | Siparişi kapatmak (S9 kapanışı) | Teslim edilemedi'de, açık kalem kalmamış ve hiçbir kalem teslim edilmemişken — son kalem hangi olayla kapanmış olursa olsun | S9 · sebep seçilmez, iptal kaydı ve geri ödeme açılmaz · B-7 gitmez | `02 §5.4` S9, §5.8 · K-717 |
| 1.11.41 | Başka kanaldan gelen gecikme feshi bildirimini kaydetmek | 1.11.8'in aralığında — teslim tarihi girilmemiş açık fiziksel kalem varken, Z-11 geçmişken | Gecikme feshi kaydı · geri alınamaz (K-715) · B-9 · IBAN'sızsa B-14 | `02 §10.4.10`, §7.2.3 · K-706, K-715 |

## 2. Ana akışlar (aktör bazlı)

> **Ne yazılır:** Adım adım. Her adımda: kullanıcı ne yapar · sistem ne kontrol eder · durum ne olur · kime bildirim gider.

Bu bölüm müşteri tarafının akışlarını yazar: satın alma (akış 1), sipariş takibi (akış 2), iptal, cayma ve iade (akış 3) ve kurumsal içeriğin vitrin yüzü (akış 7) — `02 §2.1` (K-650). Üyelik (akış 4) §9'da, firma tarafının akışları §8'dedir. Fiziksel siparişin uçtan uca ana akışı (akış 1 + 6) ve if/then'e sığmayan senaryolar §2.10'dadır (K-651).

- **Biçim:** adım tablosu §0.4.1'dir. Her adımın durumu §1'e uyar (§0.4.4); bir işlemin açık olduğu aralık §1.11'dedir ve burada tekrarlanmaz.
- **Aktör:** §2.1 ve §2.2'nin adımları giriş gerektirmez; aktörleri Ziyaretçi'dir ve adımlar müşteriye de aynen işler. §2.3–§2.9'un aktörü Müşteri'dir; üyenin farkı — hesaptaki sepet, adres defteri, sipariş geçmişi, ön dolu form — satır içinde yazılır (K-650). Yöneticinin müşteri akışını ilerleten adımları burada tek satırla anılır; akışları §8'dedir.
- **Dallar:** her alt bölüm olağan yolu ve müşterinin önüne çıkan dalları yazar; hataların tamamı §3'te bu bölümün satır numaralarına bağlanır (§0.4.2). Zaman aşımları §4'te, bildirimlerin özeti §7'dedir.

### 2.1 Ziyaretçi: vitrinde gezinme ve ürünü bulma

Akış 1'in başıdır (`02 §2.2` adım 1; K-221). Satış kapısı kapalıyken vitrin ve ürünler görünür kalır; yalnız sepete ekleme ve ödeme kapanır (2.1.9).

| # | Aktör ne yapar | Sistem ne denetler, ne yapar | Durum | Bildirim | Kaynak |
|---|---|---|---|---|---|
| 2.1.1 | Ziyaretçi ana sayfayı açar | Firmanın seçtiği düzeni gösterir — tanıtım öncelikli ya da mağaza öncelikli; iki düzende de kurumsal tanıtım ve ürün vitrini birlikte durur (kurumsal yüzü §2.2). Ürün vitrini yayındaki en yeni ürünleri gösterir; firma ana sayfa için ürün seçmez. Duyuru şeridi her sayfanın en üstündedir; tarihleri girilmişse yalnız aralığında görünür (Z-23) | — | — | `02 §3.28.1`, §3.5.3, §3.27.21 · K-754 |
| 2.1.2 | Ziyaretçi menüden bir kategoriye girer | Menü sabit iskeletten kurulur; kategoriler her seviyede adlarının alfabetik sırasıyla dizilir (K-805); yayında ürünü olmayan kategori — alt dallarında da yoksa — menüde görünmez. Kategori sayfası kendisine asılı ürünleri ve bütün alt dallarındakileri tek listede, en yeni önce gösterir; ziyaretçiye sıralama seçeneği sunulmaz. Kırıntı yolu ürünün ana kategorisinden üretilir. Boş kategorinin adresi boş kategori sayfası döner. İndirimdeki ürünün kartı referans fiyatı ve indirimin tarihlerini taşır (`02 §3.9.2`); kartın düzeni `04`'ün işidir | — | — | `02 §3.4.1`, §3.4.3, §3.4.4, §3.5.3, §3.28.4, §3.28.6, §3.9.2 · K-805 |
| 2.1.3 | Ziyaretçi ürün adıyla arar | Yalnız ürün adında, büyük-küçük harfe ve Türkçe karaktere duyarsız arar; açıklama, kategori adı ve seçenek değerleri aranmaz. Sonuç kategori sayfasının sabit düzeniyle gelir | — | — | `02 §3.5.1`, §3.5.3 |
| 2.1.4 | Ziyaretçi listeyi süzer | İki sabit eksen vardır: fiyat aralığı ve stok durumu; stok süzgecinin varsayılanı tükenmiş ürünleri de gösterir. Seçenek değerine göre süzme yoktur | — | — | `02 §3.5.2` |
| 2.1.5 | Ziyaretçi ürün kartından ürün sayfasını açar | Ürün başına tek kart vardır. Sayfa görselleri, açıklamayı, KDV dahil fiyatı ve fiyatın uygulanmaya başladığı tarihi, üretim yerini — fiziksel üründe, dijital üründe doluysa; fiziksel üründe Türkiye'yse yerli üretim logosunu —, doluysa birim fiyatı, indirimdeyse referans fiyatı ve indirimin tarihlerini, fiziksel üründe kargoya verme süresini, hizmette ifa süresini gösterir. Stok adedi hiçbir yerde gösterilmez. Yorum, puan ve öneri bloğu yoktur; sayfa "Paylaş" düğmesini taşır | — | — | `02 §3.2.2`, §3.3.1, §3.3.6, §3.6.5, §3.8.5, §3.9.2, §3.20.4, §3.30.7 |
| 2.1.6 | Ziyaretçi varyantı seçer — en fazla iki seçenek boyutu | Tükenmiş varyant "Tükendi" işaretiyle görünür ve seçilemez; firmanın açmadığı kombinasyon da görünür ve seçilemez; taslak varyant görünmez. Satın alınabilirlik ayrılmış adetler düşülerek hesaplanır: ödemesi beklenen bir siparişin ayırdığı son parça başka müşteriye "Tükendi" görünür ve o ödeme gerçekleşmezse yeniden satın alınabilir olur | — | — | `02 §3.3.2`, §3.3.3, §3.6.3, §3.7.5 |
| 2.1.7 | Ziyaretçi bütün varyantları tükenmiş bir ürüne gelir | Ürün vitrinde "Tükendi" işaretiyle kalır ve sepete eklenemez. "Stokta haber ver" ve istek listesi yoktur | — | — (`02 §9.5`) | `02 §3.6.4`, §3.16.13, §9.5 |
| 2.1.8 | Ziyaretçi varyantı adediyle sepete ekler | Sepet akışına geçer (§2.3) | — | — | `02 §2.2` adım 2 |
| 2.1.9 | Ziyaretçi satış kapalıyken gezer | Kapının dört koşulundan biri sağlanmıyorsa ya da firma satışı geçici olarak kapattıysa ürünler ve kurumsal içerik görünür; sepete ekleme ve ödeme kapalıdır ve ziyaretçi satışın kapalı olduğunu görür. Var olan sepet korunur ve içeriği görünür. Bakım modu yoktur | — | — | `02 §3.1.5`, §3.1.6 |
| 2.1.10 | Ziyaretçi arşivlenmiş bir ürünün adresine gelir — paylaşılmış bir bağlantıyla | "Bu ürün artık satılmıyor" sayfası döner: ürün adı, görseli ve durum bilgisi; fiyat ve sepete ekleme yoktur. Arşivlenmiş ürün listelerde, kategori sayfalarında ve aramada görünmez | — | — | `02 §3.7.6` |
| 2.1.11 | Ziyaretçi taslak ya da kalıcı silinmiş bir ürünün adresine gelir | "Sayfa bulunamadı" sayfası döner — sitenin görünümünde, ana sayfaya ve ürünlere dönüş yoluyla. Giriş yapmış yöneticiye taslak ürün "Taslak" bandıyla görünür (§8.1) | — | — | `02 §3.7.5`, §3.7.7, §3.30.4 |

### 2.2 Ziyaretçi: kurumsal içerik ve iletişim formu

Akış 7'nin vitrin yüzü ve üçüncü kolu — iletişim talebi (`02 §2.3`; K-285, K-468). Kurumsal taraf satış kapısına bağlı değildir. Yayın durumları §1.8.4'te, iletişim talebinin durumu §1.9.2'de, yönetimi §8.6'dadır.

| # | Aktör ne yapar | Sistem ne denetler, ne yapar | Durum | Bildirim | Kaynak |
|---|---|---|---|---|---|
| 2.2.1 | Ziyaretçi ana sayfanın kurumsal bloklarını görür | Kurumsal blok her zaman Hakkımızda'dır: kısa tanıtımı ve ana görseli. Hakkımızda boşken ya da yayında değilken blok marka adını ve logoyu gösterir; boş kutu görünmez. "Ana sayfada göster" işaretli hizmet tanıtımları ve referans işler kendi bloklarında, elle sıralarıyla görünür; işaretli kayıt yoksa blok hiç görünmez | — | — | `02 §3.27.14`, §3.28.2, §3.28.3 |
| 2.2.2 | Ziyaretçi menüden bir içerik tipine girer | Üst menü ürünlerin yanında içerik tiplerini — Hizmetlerimiz, Referanslarımız, Hakkımızda, SSS — ve İletişim'i taşır; yayında kaydı olmayan tip menüde görünmez. Genel sayfalar altbilgide, "menüde göster" işaretliyse üst menüde de durur. Liste sayfaları firmanın elle sırasını izler | — | — | `02 §3.27.11`, §3.28.4, §3.28.5 |
| 2.2.3 | Ziyaretçi bir içerik sayfasını okur | Hakkımızda, hizmet tanıtımı, referans iş ve genel sayfa kendi sayfasındadır; sık sorulan sorular tek sayfada, başlıksız listelenir. Hizmet tanıtımı fiyatsızdır ve satın alınmaz; sayfası "Bize ulaşın" düğmesini taşır (2.2.7). Video kendi platformunda açılan bir bağlantıdır; gömülü video, dosya eki ve blog yoktur | — | — | `02 §3.27.2`–§3.27.6, §3.27.9, §3.27.17, §3.27.18, §3.27.20 |
| 2.2.4 | Ziyaretçi hizmet tanıtımında ya da referans işte "İlgili ürünler"den bir ürüne geçer | Bağlı ürünlerden yalnız yayındakiler vitrin kartı olarak görünür; kart ürün sayfasını açar (2.1.5). Ürün sayfasında ters yön — "bu ürünün geçtiği referanslar" — yoktur | — | — | `02 §3.27.10` |
| 2.2.5 | Ziyaretçi İletişim sayfasını açar | Firma kimliğinin iletişim bilgileri ve yasal kimlik bilgileri — KEP adresi dahil, "İletişim" başlığı altında —, şubeler ve iletişim formu bir aradadır; firma ayrıca metin girmez. Şube "Haritada aç" bağlantısı taşır, gömülü harita yoktur | — | — | `02 §3.1.4`, §3.27.7, §3.27.8 |
| 2.2.6 | Ziyaretçi her sayfanın üst ve alt bölümünü kullanır | Sosyal medya bağlantıları ve WhatsApp bağlantısı üst ve alt bölümdedir; canlı destek ve sohbet penceresi yoktur. Altbilgi aydınlatma metninin, çerez politikasının ve "İşlem rehberi"nin bağlantılarını, genel sayfaları ve — firma kapatmadıysa — platform imzasını taşır. ETBİS doğrulama bilgisi girilmişse doğrulama bandı her sayfada görünür. Çerez onay bandı yoktur | — | — | `02 §3.1.4`, §3.24.8, §3.27.12, §3.29.6, §3.33.5, §8.4.4 |
| 2.2.7 | Ziyaretçi ya da müşteri iletişim formunu açar — İletişim sayfasından ya da bir hizmet tanıtımının "Bize ulaşın" düğmesinden | Form girişsiz açıktır ve aydınlatma metninin bağlantısını gösterir. Aydınlatma metni tamamlanıp yayına alınmadıysa ya da çerez politikası yayına alınmadıysa form kapalıdır; kurumsal sayfalar yayında kalır. Üye girişliyse ad ve e-posta ön dolu ve düzenlemeye açık gelir | — | — | `02 §3.32.1`, §3.32.8, §3.33.5 · K-850 |
| 2.2.8 | Ziyaretçi ad, e-posta, konu tipi ve mesajı — isteğe bağlı telefonu — yazar ve gönderir | Konu tipi kapalı beş değerden seçilir: Genel soru · Sipariş hakkında · Ürün hakkında · KVKK talebi · Diğer. Fiyat sorusu, indirme hakkının yenilenmesi ve siparişe dair özel istek de bu formdan gelir; ayıp talebi listede yoktur — sipariş sayfasından gider (§2.9). Dosya eki yoktur; sipariş numarası mesaja yazılır. Görünmez tuzak alan doluysa gönderim hata vermeden sessizce düşer; gönderim IP başına L-4 ile limitlidir. Geçen gönderim bir iletişim talebi kaydı açar | İletişim talebi: Açık | F-2 → firma; gönderene alındı e-postası gitmez (K-678) | `02 §3.2.1`, §3.12.6, §3.32.2, §3.32.3, §3.32.7, §3.32.10, §5.13, §9.5 · K-678 |
| 2.2.9 | Ziyaretçi cevabı bekler | Talep yalnız firmanın panelinde yaşar: gönderene numara, "taleplerim" sayfası ya da durum sorgusu verilmez. Firma cevabı sistemin dışında e-postayla verir ve talebi kapatır (§8.6) | Firma kapatınca İletişim talebi: Kapatıldı | — (`02 §3.32.6`) | `02 §3.32.5`, §3.32.6, §5.13 |
| 2.2.10 | Ziyaretçi taslak ya da silinmiş bir içeriğin adresine gelir | "Sayfa bulunamadı" sayfası döner; taslak kayıt menüde, ana sayfada ve listelerde görünmez | — | — | `02 §3.27.25`, §3.27.27, §3.30.4 |

### 2.3 Müşteri: sepet

Sepet kendiliğinden boşalmaz (Z-6); sepet hatırlatması ve istek listesi yoktur (`02 §3.16.13`).

| # | Aktör ne yapar | Sistem ne denetler, ne yapar | Durum | Bildirim | Kaynak |
|---|---|---|---|---|---|
| 2.3.1 | Müşteri varyantı adediyle sepete ekler | Satış kapalıysa eklenmez (2.1.9). Adet stoğu — ayrılmış adetler düşülerek — aşarsa eklenmez ve "Bu adette stok yok" yazar; adet söylenmez. Ürünün "bir siparişte en fazla" sınırı (P-9) bütün varyantlarının toplamına uygulanır; aşılırsa eklenmez ve sınır sayısıyla söylenir — hem sınır hem stok aşılıyorsa sınır söylenir. Dijital üründe adet 1'dir ve aynı dijital varyant ikinci kez eklenmez ("Bu ürün zaten sepetinde"). Üye daha önce aldığı dijital varyantı eklerse "Bu ürünü daha önce aldınız — sipariş sayfanızdan indirebilirsiniz" uyarısını görür ve isterse yine de alır; misafirde uyarı yoktur. Sepete eklemek stok ayırmaz; kalem varyantın ilk eklendiği andaki birim fiyatı hatırlar | — | — | `02 §3.12.4`, §3.12.9, §3.16.5, §3.16.8, §3.16.9, §3.17.3 |
| 2.3.2 | Müşteri sepeti açar | Üyenin sepeti hesabındadır ve her cihazda aynıdır; misafirinki tarayıcıya bağlıdır. Kalem fiyatını ve satın alınabilirliğini güncel üründen okur; güncel fiyat ilk eklenme anındakinden farklıysa satırda "Sepete eklediğinden beri fiyatı değişti" yazar — eski fiyat ve değişimin yönü gösterilmez. Sepette fiziksel kalem varsa: firma eşik tanımladıysa ücretsiz kargoya kalan tutar ya da "Kargo ücretsiz" yazar; asgari sipariş tutarı tanımlıysa ve sepet altındaysa eksik tutar yazar. Teslimat il kısıtı varsa adres adımından önce görünür. Taksit bilgisi gösterilmez | — | — | `02 §3.16.1`, §3.16.5, §3.18.1, §3.18.3, §3.19.5, §3.20.1, §3.21.9 |
| 2.3.3 | Müşteri adedi değiştirir ya da kalemi çıkarır | Adet artırma 2.3.1'in denetiminden geçer | — | — | `02 §3.16.8` |
| 2.3.4 | Sistem sepetteki bir kalemi satın alınamaz bulur — kalem tükenmiş, kontenjanı dolmuş, stok sepetteki adedin altına düşmüş ya da firma ürünün sınırını düşürmüş | Kalem sepette kalır ve siparişe girmez: tükenende "Tükendi", stoğun altındakinde "Bu adette stok yok", sınırı aşanda sınır mesajı yazar; adet kendiliğinden düşürülmez. Stok geri gelince ya da müşteri adedi düşürünce kalem yeniden siparişe girer. Siparişe girmeyen kalem tutara, kuponun asgari tutarına, kargo hesabına ve eşiklere girmez | — | — | `02 §3.16.6`–§3.16.8, §3.16.10 |
| 2.3.5 | Sistem vitrinden kalkan kalemi bulur — ürün ya da varyant arşive ya da taslağa alınmış veya kalıcı silinmiş | Kalem sepetten çıkar ve müşteriye bir kez söylenir (§1.8.3) | — | — | `02 §3.16.7` |
| 2.3.6 | Misafir alıcı sepetle giriş yapar | İki sepet tek sepete iner: aynı varyantta adetler toplanmaz, büyük olan kalır; farklı varyantlar yan yana durur. Birleşme müşterinin o ziyarette görmediği bir ürünü ya da adedi eklediyse bu bir kez, ürünler adıyla söylenir. Birleşen sepet 2.3.1'in adet, stok ve sınır kurallarından geçer; geçersiz kupon sayacı (L-6) iki sepetin büyüğünü taşır | — | — | `02 §3.16.3`, §3.16.4, §8.2 L-6 |
| 2.3.7 | Üye çıkış yapar | Sepet hesapla gider; o tarayıcıda sepet boş görünür | — | — | `02 §3.16.1`, §3.16.2 |
| 2.3.8 | Müşteri ödeme adımına geçer | Siparişe girebilecek en az bir kalem olmalıdır; yoksa ödeme adımı açılmaz ve müşteri sebebini sepette görür. Satış kapalıysa geçilemez. Sepette fiziksel kalem varsa asgari sipariş tutarı aranır — taban siparişe giren kalemlerin kupondan önceki, indirimli toplamıdır. Geçiş §2.4'e | — | — | `02 §3.1.6`, §3.16.7, §3.18.1–§3.18.3 |

### 2.4 Müşteri: ödeme adımı ve sipariş onayı

Ödeme adımı misafir alıcıya ve üyeye aynı adımlarla açılır; giriş duvarı yoktur (`02 §3.13.1`, §3.13.3). Sipariş onay anında doğar (`02 §5.3.2`).

| # | Aktör ne yapar | Sistem ne denetler, ne yapar | Durum | Bildirim | Kaynak |
|---|---|---|---|---|---|
| 2.4.1 | Müşteri ödeme adımını açar | Üye girişliyse siparişin iletişim e-postası hesabın doğrulanmış e-postasıdır. Misafir alıcı e-postasını yazar; adres doğrulama koduyla sınanmaz ve ikinci kez yazdırılmaz. Yazılan e-posta kayıtlı bir müşteri hesabınınsa "bu e-posta kayıtlı, giriş yaparsanız adresleriniz dolu gelir" hatırlatması görünür; müşteri giriş yaparsa sepetler 2.3.6'ya göre birleşir | — | — | `02 §3.13.3`, §3.24.5, §10.4.11 |
| 2.4.2 | Müşteri teslimat adresini girer — sepette fiziksel kalem varsa | Üye adres defterinden seçer ya da yeni bir adres yazar; yeni adresi isterse aynı adımda deftere kaydeder (K-680). Misafir alıcı adresini her siparişte yazar. Alanlar: alıcının adı ve soyadı, il — 81 ilden —, ilçe, açık adres ve zorunlu telefon; posta kodu ve T.C. kimlik numarası istenmez. Telefonu olmayan defter kaydı seçilirse telefon burada istenir. Firmanın teslimat yapmadığı ildeki adres kabul edilmez ve sebebi söylenir; defterdeki o adres seçilemez. Sepette fiziksel kalem yoksa teslimat adresi alanları gösterilmez ve il denetimi yapılmaz. Teslimat seçeneği yoktur: tek teslimat yolu kargodur — mağazadan teslim alma ve adlandırılmış kargo seçenekleri bulunmaz | — | — | `02 §3.14.1`, §3.14.4, §3.14.6, §3.20.1, §3.20.2 · `10` KD-25 · K-680 |
| 2.4.3 | Müşteri fatura adresini girer | Fatura adresi varsayılan olarak teslimat adresiyle aynıdır; "fatura adresim farklı" ile ayrı adres seçilir ya da yazılır. Fiziksel kalemsiz siparişte de istenir. Telefon taşımaz; kurumsal fatura alanı yoktur | — | — | `02 §3.14.2`, §3.14.4, §3.14.6, §3.25.1 |
| 2.4.4 | Müşteri kupon kodu girer — isteğe bağlı | Siparişe en fazla bir kod uygulanır; müşteri kodu değiştirir ya da kaldırır (K-679). Kod büyük-küçük harf ayrımı olmadan eşleşir (K-806). Kod tarih aralığındaysa (Z-22), kullanım hakkı — kullanılmış ve ayrılmış haklar düşülerek — kalmışsa (K-654) ve kupon `02 §3.10`'un öteki koşullarını — asgari sepet tutarı dahil — karşılıyorsa kabul edilir; payı kalemlere dağıtılır ve özet güncellenir. Kupon ücretsiz kargo eşiğinin ve asgari sipariş tutarının tabanını düşürmez. Art arda geçersiz kod denemesi L-6 ile limitlidir | — | — | `02 §3.10`, §3.18.2, §3.19.4, §8.2 L-6 · K-654, K-679, K-806 |
| 2.4.5 | Müşteri ödeme yöntemini seçer | Kart, kurulumda sağlayıcı anahtarları tanımlıysa görünür; sağlayıcıya erişilemiyorsa "şu an kullanılamıyor" olarak görünür ve seçilemez. Havale/EFT, firma açtıysa görünür; aynı IP ya da — giriş yapmış üyede — aynı e-posta için açık ödenmemiş sipariş tavanı (L-8) doluysa seçilemez, mesaj nötrdür ve kart yolu açık kalır. Kapıda ödeme yoktur; taksit yalnız sağlayıcının kart ekranındadır | — | — | `02 §3.21.2`–§3.21.5, §3.21.9, §6.2.15, §8.2 L-8 |
| 2.4.6 | Müşteri onay özetini okur | Özet siparişe girecek kalemleri, tutarın dökümünü — KDV, indirim, kuponun payı, kargo ücreti ya da ücretsiz kargo — ve ödenecek toplamı gösterir. Misafir alıcıda siparişin gideceği e-posta adresi açıkça yazar ("Siparişiniz şu adrese gönderilecek: …") ve onaydan önce düzeltmeye açıktır. Ön Bilgilendirme Formu özetle birebir aynıdır ve üzerine yasal bilgileri ekler; içeriği `02 §3.24.3`'tedir. Sabit "sipariş nasıl kurulur" metni ve aydınlatma metninin bağlantısı görünür. Kutuların hemen üstünde, Ön Bilgilendirme Formu'ndan ayrı kısa bir cayma özeti durur; içeriği `02 §3.24.3`'tedir | — | — | `02 §3.17.2`, §3.24.3, §3.24.5, §3.33.5 · K-831 |
| 2.4.7 | Müşteri onay kutularını işaretler | Her siparişte iki kutu: Ön Bilgilendirme Formu'nun okunduğu ve Mesafeli Satış Sözleşmesi'nin onayı. Sepette dijital kalem varsa üçüncü kutu — indirmenin ödeme onaylandığında açılacağı ve cayma hakkının düşeceği; hizmet kalemi varsa bir kutu daha — ifanın cayma süresi dolmadan başlaması ve ifa tamamlanınca hakkın düşmesi. Kutular işaretlenmeden sipariş onaylanamaz. KVKK rıza kutusu, yaş beyanı ve sipariş notu alanı yoktur | — | — | `02 §3.24.1`, §3.24.2, §3.24.4, §3.24.7 |
| 2.4.8 | Müşteri "Siparişi onayla — ödeme yükümlülüğü doğar" düğmesine basar | Kullanıcı L-7'nin eşiğindeyse yeni sipariş onaylanamaz; mesaj nötrdür. Sepet onay anında yeniden değerlendirilir: ödenecek tutarı, dökümü ya da siparişe girecek kalemleri değiştiren her fark — fiyat, indirimin başlaması ya da bitmesi, kuponun geçersizleşmesi, ayrılamayan kalem, kargo ücreti, eşik, teslimat illeri — ve Ön Bilgilendirme Formu'nun ya da sözleşmenin sürüm artışı siparişin oluşmasını durdurur. Müşteri neyin değiştiğini söyleyen güncel özeti görür ve yeniden onaylar — metin değiştiyse kutuları yeniden işaretler; ayrılamayan kalem için sayı söylenmez. Siparişe girebilecek kalem kalmadıysa sipariş oluşmaz ve müşteri sepete döner | — · fark varsa sipariş doğmaz | — | `02 §3.16.6`, §3.16.7, §3.17.2, §3.24.1, §5.3.3, §8.2 L-7 |
| 2.4.9 | Sistem siparişi oluşturur — özet değişmemişse | Tahmin edilemez sipariş numarası verilir. Kalemler — tutarları, kupon payları, ürün tipi, cayma istisnası ve ifa süresi dahil —, adresler, iletişim e-postası, firmanın yasal kimliği, onaylanan metin sürümleri, onay kutularının kaydı, havalede firmanın IBAN'ı ve kargoya verme sözü donar. Stok, hizmet kontenjanı ve kupon hakkı ayrılır; ödeme süresi işlemeye başlar (Z-7, Z-8). Aynı sepete bağlı ödenmemiş önceki sipariş kendiliğinden iptal edilir ve ayırması serbest kalır. Giriş yapılmadan verilen sipariş, e-postası doğrulanmış bir müşteri hesabınınsa o hesaba anında düşer. Ekran siparişin alındığını ve sipariş numarasını gösterir — kartta yönlendirmeden önce (K-704); kartta müşteri sağlayıcının sayfasına geçer (§2.5.1), havalede IBAN'ı görür (§2.5.2) | Alındı + Bekliyor · önceki sipariş: → İptal edildi + Başarısız (S3, Ö2) | B-1 → müşteri — iki yasal metin gövdede · havalede B-2 → müşteri · F-1 → firma · önceki siparişte B-7 → müşteri (§1.2 S3) | `02 §3.13.3`, §3.17.1, §3.17.3, §3.17.7, §3.22.1, §3.23, §3.33.6, §5.3.2, §9.2, §9.3 · K-663, K-704 |

### 2.5 Müşteri: ödeme ve teslim

Ödemenin onaylandığı an iki ekseni ve kalemleri birlikte değiştirir (§1.4.2.1); tipe göre hatlar §1.5'tedir. Yöneticinin teslim adımları §8.2'dedir.

#### 2.5.1 Kart ödemesi

| # | Aktör ne yapar | Sistem ne denetler, ne yapar | Durum | Bildirim | Kaynak |
|---|---|---|---|---|---|
| 2.5.1.1 | Müşteri sağlayıcının sayfasında ya da çerçevesinde kart bilgisini girer | Kart bilgisi sağlayıcıda girilir; ürün kart verisini görmez, saklamaz, loglamaz. Her ödeme istisnasız 3D Secure'dan geçer. Taksit sağlayıcının ekranındadır; sipariş tek tutar taşır | Alındı + Bekliyor | — | `02 §3.21.1`, §3.21.3, §3.21.9 |
| 2.5.1.2 | Müşteri 3D Secure'ı tamamlar ve siteye döner | Dönüş sipariş sayfasına iner. Ödeme sağlayıcının başarı bildirimiyle onaylanır; sonuçları §2.5.3'tedir. Bildirim henüz gelmemişse sayfa ödemenin beklendiğini söyler ve sonuç gelince güncellenir; gelmezse 2.5.1.4 işler. Bildirimin doğrulanması ve tutarının siparişle eşleşmesi `08`'in işidir (K-705) | Başarı bildirimiyle → Ödendi (Ö1) · bildirim gelene kadar Alındı + Bekliyor | Ö1'de B-4 → müşteri | `02 §3.21.3`, §3.17.1, §5.5 · K-704, K-705 |
| 2.5.1.3 | Doğrulama başarısız olur ya da kart reddedilir | Ödeme gerçekleşmez; sipariş kendiliğinden iptal edilir ve ayrılanlar aynı anda serbest kalır. Sipariş müşterinin listesinde görünmez, panelde görünür ve L-7'ye sayılır. Sipariş sayfası ödemenin gerçekleşmediğini söyler (K-704). Sepet olduğu gibi durur; müşteri sepetten yeni bir siparişle yeniden dener (2.4.8). Ayrıntı §3.2 | → İptal edildi + Başarısız (S3, Ö2) | B-7 → müşteri (§1.2 S3) | `02 §3.16.12`, §3.17.5, §3.17.6, §3.17.8, §6.2.1 · K-704 |
| 2.5.1.4 | Müşteri banka ekranını kapatır ya da ödeme yarıda kalır | Sepet olduğu gibi durur. Kart ödeme süresi (Z-7) dolunca, iptalden önce sağlayıcıya sonuç son bir kez sorulur: ödeme alınmışsa onaylanır; alınmamışsa sipariş kendiliğinden iptal edilir; sorgu yanıtsızsa sipariş iptal edilmez ve panelde "ödeme sonucu alınamadı" işaretiyle bekler. Müşteri o arada aynı sepetten yeni sipariş onaylarsa önceki sipariş 2.4.9'a göre iptal edilir. Ayrıntı §3.2, §4 | → Ödendi (Ö1) ya da → İptal edildi + Başarısız (S3, Ö2) ya da Alındı + Bekliyor | Ö1'de B-4 · iptalde B-7 → müşteri · yanıtsız sorguda — (`02 §9.2`; K-683) | `02 §5.6.3`, §6.1.2, §6.2.2 · K-683 |

#### 2.5.2 Havale/EFT ödemesi

| # | Aktör ne yapar | Sistem ne denetler, ne yapar | Durum | Bildirim | Kaynak |
|---|---|---|---|---|---|
| 2.5.2.1 | Müşteri siparişi havaleyle onaylar | Ekranda firmanın siparişe donan IBAN'ı ve sipariş numarası görünür; aynı bilgi B-2'de ve sipariş sayfasındadır. Havale ödeme süresi (Z-8) sipariş onayından işler | Alındı + Bekliyor | B-2 → müşteri (2.4.9'da) | `02 §3.21.6`, §3.22.4 · K-663 |
| 2.5.2.2 | Müşteri bankasından havale yapar | Sistem dışıdır: ürün gelen tutarı bilmez ve karşılaştırmaz; eksik ya da fazla gelen havalede kararı firma ürünün dışında verir | — | — | `02 §3.21.7` |
| 2.5.2.3 | Sistem hatırlatma gönderir | Ödeme süresinin son iş gününün başında, bir kez (Z-9); havale işaretinin düzeltilmesiyle yeniden başlayan sürede bir kez daha (§4.1.9) | — | B-3 → müşteri | `02 §3.21.6` · K-685 |
| 2.5.2.4 | Yönetici parayı hesabında görür ve "ödendi" işaretler — panel siparişin donmuş IBAN'ını gösterir, onay geri alınamaz (§8.2) | Ödeme onaylanır; sonuçları §2.5.3'tedir | → Ödendi (Ö1) | B-4 → müşteri | `02 §3.21.5`, §3.21.6, §5.9 · K-663 |
| 2.5.2.5 | Sistem süre dolduğunda ödemeyi işaretlenmemiş bulur | Sipariş kendiliğinden iptal edilir ve ayrılanlar serbest kalır; iptal edilmiş sipariş sonradan "ödendi" yapılamaz — gelen parayı firma geri öder ya da müşteriden yeni sipariş ister. Süre sitenin kesintisinde dolduysa iptal ertelenir (§4; K-667) | → İptal edildi + Başarısız (S3, Ö2) | B-7 → müşteri | `02 §3.17.5`, §3.21.8, §4.2 Z-8 · K-667 |
| 2.5.2.6 | Müşteri ödeme beklenirken vazgeçer ya da kartla ödemek ister | Siparişi bütünüyle iptal eder (2.7.1); kalem düzeyinde iptal yoktur. Kartla ödemek isteyen müşteri aynı sepetten yeni sipariş verir; önceki sipariş kendiliğinden iptal edilir (2.4.9) | — | — | `02 §3.17.7`, §7.2.2 |

#### 2.5.3 Ödeme onayı ve teslim

| # | Aktör ne yapar | Sistem ne denetler, ne yapar | Durum | Bildirim | Kaynak |
|---|---|---|---|---|---|
| 2.5.3.1 | Sistem ödeme onayını işler | Ayrılan stok ve kontenjan kesin düşer, kupon hakkı kullanılmış sayılır; siparişe giren kalemler sepetten çıkar — ödeme sürerken sepete eklenen kalem kalır. Fiziksel kalemli siparişte kargoya verme süresi (Z-10), hizmet kaleminde ifa süresi (Z-39) başlar. Dijital kalemler teslim edilir | Fiziksel kalem varsa → Hazırlanıyor + Ödendi (S1) · yalnız dijitalse → Teslim edildi + Ödendi (S2) · öteki hâllerde Alındı + Ödendi · dijital kalem: teslim işareti | B-4 → müşteri — dijital kalem varsa indirmenin hazır olduğu | `02 §3.16.12`, §3.20.6, §4.3, §5.6, §5.7 |
| 2.5.3.2 | Müşteri dijital dosyayı sipariş sayfasından indirir | Kalemin indirme hakkı kaldıkça indirir — hak sipariş anında kaleme donar (P-8; K-713); süre sınırı yoktur. Firma dosyayı güncellediyse yeni hâli iner ve o da haktan düşer. Hakkı dolan müşteri iletişim formundan "Sipariş hakkında" tipiyle başvurur ve yönetici hakkı yeniler (§1.11.38); sipariş sayfasında "hakkımı yenile" düğmesi yoktur | kalem: indirme sayacı | — · hakkın yenilenmesinde — (`02 §9.2`; K-683) | `02 §3.12.5`–§3.12.8, §3.23.1, §3.32.10 · K-683, K-713 |
| 2.5.3.3 | Yönetici siparişi kargoya verir — kargo şirketi ve takip numarasıyla ya da "kendi aracımızla teslim" beyanıyla (§8.2) | Yalnız açık fiziksel kalemler gider; sipariş tek parça ve tek takip numarası taşır. Sipariş sayfası şirketi, takip numarasını ve listedeki şirkette "Takip et" bağlantısını ya da araç beyanını gösterir. Fiziksel kalemin iptal yolu kapanır; cayma düğmesi ve "sorun bildir" açılır | → Kargoya verildi (S5) | B-5 → müşteri | `02 §3.20.3`, §3.20.7, §3.20.8, §7.3.7, §7.5.2 |
| 2.5.3.4 | Kargo malı ulaştıramaz; yönetici "teslim edilemedi" işaretler (§8.2) | Gönderi firmaya döner; yönetici yeniden gönderir ya da iptal eder (§8.2, §8.3). Kargodayken cayılmış kalemde §2.10.3 işler | → Teslim edilemedi (S7) · ardından S8, S9 ya da S11 | B-6 → müşteri · yeniden gönderimde B-5 | `02 §5.4` |
| 2.5.3.5 | Mal müşteriye ulaşır; yönetici teslim işaretini ve teslim tarihini girer (§8.2) | Tarih teslimin gerçekleştiği gündür ve işaret gününden önce de olur; kargoya verildiği günden önceki ve ileri tarih reddedilir. Tarih fiziksel kalemlerin tamamına yazılır. Kendiliğinden geçiş yoktur: işaretlenmeyen sipariş Kargoya verildi'de kalır ve cayma penceresi başlamaz. Cayma penceresi (Z-13) ve ayıp talebinin süresi (Z-18) bu tarihten işler | → Teslim edildi (S6) | — (`02 §9.2`) | `02 §3.20.11`, §5.4 |
| 2.5.3.6 | Yönetici hizmet kalemini "tamamlandı" işaretler — ödeme onaylanmışken (§8.2) | Kalem teslim edilmiş olur; hizmetten cayma hakkı düşer, "sorun bildir" açılır. Randevu ve takvim yoktur; ifa süresi aşılırsa kendiliğinden bir işlem başlamaz. Fiziksel açık kalemi olmayan siparişte son açık kalemse sipariş kapanış kuralıyla hattını bitirir (§1.4.3) | kalem: teslim işareti · son açık kalemse → Teslim edildi (S2, S10, S11) | — (`02 §9.2`) | `02 §3.20.9`, §7.3.3, §5.4 · K-671 |
| 2.5.3.7 | Müşteri fiziksel kalemi olmayan siparişini izler | Sipariş Hazırlanıyor ve Kargoya verildi'yi kullanmaz; Alındı'da bekler ve açık kalemlerin tamamı teslim işaretini aldığında Teslim edildi'ye geçer (§1.5) | Alındı + Ödendi → Teslim edildi (S2) | — (`02 §9.2`) | `02 §3.20.12`, §5.7 |

### 2.6 Müşteri: sipariş takibi (akış 2)

Sipariş sayfasına üç yoldan girilir ve sayfa her bilgiyi taşır; hiçbir akış bir e-postanın ulaşmasına bağlı değildir (`02 §3.22.3`, §6.1.1; K-187).

| # | Aktör ne yapar | Sistem ne denetler, ne yapar | Durum | Bildirim | Kaynak |
|---|---|---|---|---|---|
| 2.6.1 | Üye giriş yapıp sipariş geçmişinden siparişi açar | Liste hesaba bağlı siparişleri — hesaba düşmüş misafir siparişleri dahil — gösterir. Ödemesi hiç alınmamış ve kendiliğinden iptal edilmiş sipariş listede görünmez; müşterinin ya da firmanın iptal ettiği ödenmemiş sipariş görünür | — | — | `02 §3.13.2`, §3.17.8, §3.22.3 |
| 2.6.2 | Misafir alıcı sipariş numarası ve e-postasıyla sorgular | İkisi eşleşirse sipariş sayfası açılır; numara tek başına açmaz. Başarısız sorgular aynı IP'den ve sorguya yazılan aynı e-postaya yönelik olarak ayrı ayrı limitlidir (L-5); e-posta ekseninde engellenen gerçek sahip e-postadaki bağlantıdan girmeye devam eder | — | — | `02 §3.22.3`, §3.22.5, §8.2.2 |
| 2.6.3 | Müşteri sipariş e-postasındaki bağlantıyı açar | Bağlantının erişim anahtarı sayfayı e-posta sormadan açar. Anahtarın kendi ömrü yoktur — siparişin kişisel verileri imha edilene kadar çalışır; hesabın silinmesinden ve hesabın e-posta değişikliğinden etkilenmez. Dijital ürünün indirme bağlantısı aynı kapıdır. Kalan risk bilinçlidir: bağlantıyı başkasına ileten müşteri sipariş sayfasını da paylaşmış olur (`02 §3.22.5`) | — | — | `02 §3.22.3`, §3.22.5 |
| 2.6.4 | Müşteri sipariş sayfasını okur | Sayfa iki eksenin durumunu, donmuş kalemleri ve tutarları, gerçekleşmiş geri ödemeleri — tarih, tutar ve yolla —, adresleri, takip bilgisini, indirme düğmesini, havalede siparişe donmuş IBAN'ı ve son ödeme gününü, onaylanan Ön Bilgilendirme Formu ve sözleşme sürümlerini, onay kutularının kaydını ve o anda açık olan işlemleri (§1.11.1–§1.11.14) taşır. Fatura sayfada yoktur — firma onu kendi kanalından iletir. Sayfa arama motorlarına kapalıdır | — | — | `02 §3.22.4`, §3.23.3, §3.24.6, §3.25.2, §3.30.6, §4.2 Z-8 · K-663, K-826 |
| 2.6.5 | Müşteriye bir sipariş e-postası ulaşmaz | Müşteri her bilgiye sipariş sayfasından ulaşır. Üç başarısız yeniden denemeden (Z-26) sonra siparişin panel satırına "e-posta ulaşmadı" işareti düşer ve yönetici e-postayı panelden yeniden gönderir (§1.11.36) | — | — | `02 §6.1.1`, §9.1.6 |
| 2.6.6 | Üye sipariş yürürken hesabını siler ya da hesabının e-postasını değiştirir (§9.3.5, §9.4.2) | Sipariş kendi kaydıyla sürer. Hesap silindiyse takip, iptal, cayma ve iade misafir yolundan — sipariş numarası ve siparişe donmuş e-posta — ya da e-postadaki bağlantıyla yürür; silinmiş hesabın siparişleri hiçbir hesaba bağlanmaz. E-posta değiştiyse bağlanmış siparişler hesapta kalır | — | — | `02 §3.13.14`, §3.15.2, §3.15.3 |
| 2.6.7 | Misafir alıcı e-postasını yanlış yazdığını onaydan sonra fark eder | Sipariş sayfasına giden iki yol da o adrese bağlıdır; müşteri firmaya telefonla, iletişim formuyla ya da e-postayla ulaşır ve yönetici kimliği teyit edip siparişin e-postasını düzeltir. Akış §2.10.5'tedir | — | B-1 → yeni adres · B-15 → eski adres | `02 §6.2.13`, §10.4.11 |

### 2.7 Müşteri: iptal ve gecikme feshi

Satın almadan sonraki üç yolun ilkidir (`02 §7.1.1`). İptal firma onayı beklemez; açık olduğu aralıklar §1.11.1–§1.11.4 ve §1.11.8'dedir. Müşterinin kendi iptali firmaya e-postayla bildirilmez — firma onu panelin sayaçlarında görür (`02 §9.5`).

| # | Aktör ne yapar | Sistem ne denetler, ne yapar | Durum | Bildirim | Kaynak |
|---|---|---|---|---|---|
| 2.7.1 | Müşteri ödeme beklenirken siparişi iptal eder | Ödeme onayından önce iptal yalnız sipariş bütünüyledir — dijital kalem dahil. Ayrılan stok, kontenjan ve kupon hakkı serbest kalır. Sipariş müşterinin listesinde görünmeye devam eder | → İptal edildi + Başarısız (S3, Ö2) | B-7 → müşteri | `02 §3.17.8`, §5.6.5, §7.2.2 |
| 2.7.2 | Müşteri ödenmiş siparişte bir fiziksel kalemi ya da kalemin bir kısmını (adet) iptal eder — sipariş Kargoya verildi'ye geçene kadar | Kalem iptal edilen adetle iptal kaydını alır ve o adet stoğa kendiliğinden döner — varyant silinmişse dönecek stok yoktur (`02 §3.7.7`); kalan kalemler ve adetler yoluna devam eder (K-787, K-791). Kupon hakkı yalnız siparişin tamamı iptal edilirse döner. Havale hattında müşteri geri ödeme IBAN'ını bu adımda girer (2.7.5). Kargoya verme süresi (Z-10) aşıldıktan sonraki iptal gecikmede feshin yoludur: panel firmaya kanuni faiz uyarısını gösterir | kalem: iptal kaydı · son açık kalemse kapanış kuralı (§1.4.3) — → İptal edildi (S4) ya da → Teslim edildi (S10) | B-7 → müşteri | `02 §7.2.1`–§7.2.3, §7.2.6, §7.2.7 · K-787, K-791 |
| 2.7.3 | Müşteri ödenmiş siparişte hizmet kalemini ya da kalemin bir kısmını (adet) iptal eder — kalem "tamamlandı" işaretini alana kadar | Siparişin sevkiyat durumundan bağımsızdır; kontenjan iptal edilen adet kadar havuza döner (K-792). İfa süresi (Z-39) aşıldıktan sonraki iptalde panel firmaya kanuni faiz uyarısını gösterir | kalem: iptal kaydı · son açık kalemse kapanış kuralı (§1.4.3) | B-7 → müşteri | `02 §3.20.9`, §7.2.5, §7.2.6 · K-792 |
| 2.7.4 | Müşteri ödenmiş dijital kalemi iptal etmek ister | Yoktur: kalem ödeme onayında teslim edilmiştir ve cayma hakkı ön onayla düşmüştür. Sebepli sorun ayıp talebi yolundan yürür (§2.9) | — | — | `02 §7.2.5`, §7.3.2 |
| 2.7.5 | Müşteri havale hattında geri ödeme IBAN'ını girer — iptal, fesih ya da cayma sırasında ya da IBAN isteğiyle açılan alanda (B-14) | Sipariş sayfası IBAN'ı biçim ve sağlama basamağıyla denetler ve aydınlatma metninin bağlantısını gösterir. IBAN geri ödeme işlenene kadar sipariş sayfasında düzeltmeye açıktır. Kart hattında IBAN istenmez | — | — (`02 §9.2`; K-710) | `02 §3.33.5`, §7.4.5 · K-710 |
| 2.7.6 | Sistem ya da yönetici iptalin geri ödemesini yapar | Para ödemenin geldiği yoldan, iptalden itibaren on dört gün içinde (Z-17) gider: kart hattında sistem iadeyi kendiliğinden başlatır — onay penceresi yoktur; havale hattında yönetici müşterinin IBAN'ına gönderip panelden işler (§8.3). Tutar iptal edilen adetlerden `02 §7.1.7`'nin kuralıyla ayrılır (K-788). Siparişin fiziksel kalemlerinin bütün adetleri kargodan önce iptal edildiyse ödenmiş kargo ücreti son iptalin geri ödemesine eklenir; bir adet bile kargoyla gidecekse eklenmez (K-789). Kart iadesi sağlayıcıda gerçekleşmezse 2.8.4.3 işler | → Kısmen geri ödendi ya da Geri ödendi (Ö3, Ö4, Ö5) | B-8 → müşteri · kartta F-4 → firma | `02 §5.5`, §7.1.7, §7.2.8, §7.2.9 · K-788, K-789 |
| 2.7.7 | Müşteri gecikme nedeniyle fesheder — siparişin firmaya ulaşmasından Z-11 geçmiş ve teslim tarihi girilmemiş fiziksel kalemi varken | Fesih teslim edilmemiş fiziksel kalemlerin açık adetlerinin tamamına uygulanır — kalem ve adet seçilmez (§1.7.4); cayma istisnası ve kişiye özel üretim işaretli kalem dahil; teslim edilmiş dijital ve hizmet kalemleri ile tamamlanmamış hizmet kalemi yoluna devam eder. Havale hattında IBAN fesih sırasında girilir (2.7.5). Feshedilen kalem kargoya verilmez, yeniden gönderilmez ve firma iptaline konu olmaz; kargoya verilmemişse stoğu kendiliğinden döner. Panel fesih satırında firmaya kanuni faiz uyarısını gösterir. Feshi başka bir kanaldan bildiren müşterinin feshini yönetici kaydeder (§8.4.8; K-706) | kalem: gecikme feshi · Hazırlanıyor'da açık kalem kalmazsa → İptal edildi (S4 kapanışı) · teslim edilmiş kalem varsa → Teslim edildi (S10) · gönderi kargodaysa 2.7.8 | B-9 → müşteri · F-3 → firma | `02 §5.8`, §7.2.3 · K-706 · §1.11.8 |
| 2.7.8 | Sistem ya da yönetici feshin geri ödemesini yapar; kargodaki mal firmaya döner | Feshedilen kalemlerin ödenmiş bedeli ve kargo ücreti, fesih bildiriminin tarih damgasından itibaren on dört gün içinde (`02 §7.2.3` — yasal süredir, kendi süre kimliği yoktur; `02` onu Z-11'in sonucu olarak yazar) ödemenin geldiği yoldan geri ödenir — kartta sistem kendiliğinden başlatır, havalede yönetici işler. Kargodaki mal firmanın iade adresine döner, masrafı firmadadır; yönetici dönen malı teslim alma adımıyla işler ve stoğa ekler — adım geri ödemeyi başlatmaz. Açık kalem kalmadıysa yönetici siparişi S7'den sonra S9'un kapanışıyla kapatır (§8.3) | Ö3–Ö5 · kargodaysa → Teslim edilemedi (S7) → İptal edildi (S9 kapanışı) ya da → Teslim edildi (S11) | B-8 → müşteri · kartta F-4 → firma · S7'de B-6 → müşteri; kapanışta B-7 gitmez | `02 §5.8`, §7.2.3, §7.2.9 |

### 2.8 Müşteri: cayma ve iade

Sebepsiz ikinci yoldur (`02 §7.3`, §7.4). Düğmelerin aralıkları §1.11.5–§1.11.7'de, kalemin kayıtları §1.7.1'dedir. Cayma beyanı hiçbir ekseni değiştirmez; ödeme ekseni para gönderildiğinde değişir (`02 §5.6.6`). Yöneticinin iade ve geri ödeme adımları §8.3'tedir.

#### 2.8.1 Fiziksel kalem

| # | Aktör ne yapar | Sistem ne denetler, ne yapar | Durum | Bildirim | Kaynak |
|---|---|---|---|---|---|
| 2.8.1.1 | Müşteri sipariş sayfasında cayma düğmesini görür | Düğme sipariş Kargoya verildi'ye geçtiği andan teslim tarihinden on dört gün (Z-13) dolana kadar görünür; teslim tarihi girilmemişse açıktır — mal yoldayken de. Kargoya verilmeden önce yol iptaldir (2.7.2). Mutlak istisna işaretli kalemde düğme yoktur; koşullu istisnada açıktır ve beyan ekranı "koruyucu ambalajı açılmamışsa cayabilirsiniz" koşulunu yazar. İstisna, sebebi ve koşulu kaleme donmuştur | — | — | `02 §3.23.1`, §7.3.1, §7.3.4, §7.3.7 |
| 2.8.1.2 | Müşteri kalemi ve — adedi birden büyükse — cayılan adedi seçip cayma beyanında bulunur; aynı kalemde ardışık beyan ayrı kayıttır (§1.7.4) | Beyan ekranı firmanın güncel iade adresini ve malı on dört gün (Z-42) içinde gönderme yükümlülüğünü gösterir — teslim işaretini almamış kalemde önce malı teslim almazsa kargonun onu firmaya döndürdüğünü söyler (§2.10.3); yükümlülük mal teslim alınırsa doğar ve süresi beyandan işler (K-838); adres beyanla birlikte kayda yazılır ve sonradan değişmez. Havale hattında müşteri IBAN'ını girer (2.7.5). Beyan tarih damgasıyla kayda geçer; pencerenin içinde olup olmadığı damgadan okunur. Teslim edilmiş kalemde iade süreci başlar ve panel kalemi "iade malı bekleniyor" gösterir; teslim işaretini almamış kalemde — kargodayken — beyan cayılan adetleri kapatır (§2.10.3) | kalem: cayma beyanı · teslim işaretsiz son açık kalemse kapanış kuralı (§1.4.3) — Teslim edilemedi'de teslim edilmiş kalem varsa → Teslim edildi (S11), yoksa yöneticinin S9 kapanışı (K-717); öteki hâllerde eksenler değişmez | B-9 → müşteri — beyanın tarihi, kalemleri ve adetleri, iade adresi · F-3 → firma | `02 §5.6.6`, §5.8, §7.1.6, §7.3.5, §7.4.4 · K-665, K-717, K-787, K-790, K-838 |
| 2.8.1.3 | Yönetici teslimi beyanla aynı güne işaretler — ya da teslim tarihi sonradan o güne düzeltilir (§8.2) | Panel teslimin beyandan önce mi sonra mı olduğunu sorar; cevap beyanın teslimden önce mi sonra mı sayılacağını ve geri ödeme süresinin başlangıcını belirler | — | — | `02 §3.20.11`, §7.4.1 |
| 2.8.1.4 | Müşteri malı gönderir — beyandan itibaren Z-42 içinde | Taşıyıcıyı müşteri seçer ve malı firmanın iade adresine karşı ödemeli gönderir; masrafı teslim alırken firma öder. Ürün iade etiketi üretmez, taşıyıcı ya da takip numarası istemez ve gönderimi denetlemez | — | — | `02 §7.4.2`, §7.4.4 |
| 2.8.1.5 | Yönetici iade malını teslim alır ve ulaşma tarihini ve ulaşan adedi girer (§8.3) | Teslimden sonraki caymada geri ödemenin on dört günü (Z-16) ulaşan adetler için ulaşma tarihinden — mal beyandan önce ulaştıysa beyandan — işler. Stok, yönetici malı kontrol edip eklediğinde döner; kupon hakkının dönüşü §1.4.4'tedir. "İade malı bekleniyor" ulaşan adetler için kalkar, ulaşmayanlar için sürer | kalem: iade teslim alma | — (`02 §9.2`) | `02 §7.4.1`, §7.4.7 · K-794 |
| 2.8.1.6 | Koşullu istisna kaleminin malı koruyucu ambalajı açılmış olarak döner | Yönetici teslim alma adımında iade reddini seçer (§8.3): geri ödeme yapılmaz, havale hattında IBAN silinir, mal stoğa girmez ve firmanın bedeliyle siparişteki teslimat adresine geri gönderilir; ret geri alınmaz. İtirazın yeri §5.3 | kalem: iade reddi | B-16 → müşteri | `02 §7.4.9` · K-656, K-657 |
| 2.8.1.7 | Mal kullanılmış, hasarlı ya da eksik döner — kalem reddedilebilecek bir kalem değildir | Ret ve kesinti yoktur; kalemin ödenmiş bedeli tam geri ödenir. Değer kaybı talebi ürünün dışındadır (§5.3) | — | — | `02 §7.4.10` · K-668 |
| 2.8.1.8 | Müşteri malı hiç göndermez | Geri ödeme süresi başlamaz; panel kalemi beyandan geçen gün ve gönderme süresiyle gösterir. Z-42 geçtikten sonra yöneticiye isteğe bağlı "mal dönmedi" kapanışı açılır (§1.11.30); havale hattında IBAN kapatmayla ya da Z-38 dolunca silinir. Mal sonradan ulaşırsa kalem yeniden açılır — §2.10.4 | kalem: "mal dönmedi" kapanışı | — (`02 §9.2`) | `02 §7.4.1`, §7.4.5, §10.4.9 |

#### 2.8.2 Hizmet kalemi

| # | Aktör ne yapar | Sistem ne denetler, ne yapar | Durum | Bildirim | Kaynak |
|---|---|---|---|---|---|
| 2.8.2.1 | Müşteri hizmet kaleminin cayma düğmesini görür | Düğme ödeme onayından sonra açılır ve kalem "tamamlandı" işaretini alana ya da Z-14 dolana kadar görünür — hangisi önce gelirse. Pencere sipariş tarihinden, onay anında fiziksel kalem taşıyan siparişte siparişin teslim tarihinden işler; teslim tarihi girilmemişse ya da fiziksel kalemlerin tamamı teslimden önce kapandıysa hak tamamlanma işaretine kadar sürer. Ödeme beklenirken yol siparişin bütünüyle iptalidir (2.7.1) | — | — | `02 §7.3.3` · K-673 |
| 2.8.2.2 | Müşteri hizmet kaleminden cayar | Beyan ekranı iade adresi ve gönderme yükümlülüğü göstermez — geri gönderilecek mal yoktur. Havale hattında IBAN girilir (2.7.5). Ürün ifanın başladığını izlemez: kısmen ifa edilmiş hizmetten cayma da tam caymadır ve cayılan adetlerin ödenmiş bedelinin tamamı geri ödenir. Beyan cayılan adetleri kapatır; kontenjan cayılan adet kadar havuza döner; geri ödemenin on dört günü beyandan işler (Z-16) | kalem: cayma beyanı · son açık kalemse kapanış kuralı (§1.4.3) — → Teslim edildi (S2, S10, S11) ya da hiçbir kalem teslim edilmemişse → İptal edildi (S3, S4); Kargoya verildi ve Teslim edilemedi'de yöneticinin S7 ve S9 kapanışı (K-717) | B-9 → müşteri · F-3 → firma | `02 §5.6.6`, §7.3.3, §7.3.5, §7.4.1, §7.4.8 · K-672, K-717, K-792 |

#### 2.8.3 Dijital kalem

| # | Aktör ne yapar | Sistem ne denetler, ne yapar | Durum | Bildirim | Kaynak |
|---|---|---|---|---|---|
| 2.8.3.1 | Müşteri dijital kalemden caymak ister | Yoktur: hak, onay adımındaki üçüncü kutuyla ödeme onaylandığı anda düşmüştür. Ayıplı dosyada yol ayıp talebidir (§2.9) | — | — | `02 §3.24.2`, §7.3.2 |

#### 2.8.4 Geri ödeme ve başka kanaldan cayma

| # | Aktör ne yapar | Sistem ne denetler, ne yapar | Durum | Bildirim | Kaynak |
|---|---|---|---|---|---|
| 2.8.4.1 | Yönetici caymanın geri ödemesini işler — kart ve havale hattında (§8.3) | Para ödemenin geldiği yoldan gider: karta sağlayıcı üzerinden, havalede müşterinin IBAN'ına; havaleyi yönetici bankada gerçekleştikten sonra işler ve onay geri alınamaz. Teslim alma işareti geri ödemeyi kendiliğinden başlatmaz. Tutar cayılan adetlerden `02 §7.1.7`'nin kuralıyla ayrılır (K-788). Kargoyla gönderilen fiziksel kalemlerin bütün adetlerinden cayıldıysa — ardışık beyanlarla da — kargo ücreti son caymanın geri ödemesine eklenir; bir adet bile müşteride kalıyorsa eklenmez (K-789). Kupon hakkı yalnız siparişin tamamı iade edilince döner | → Kısmen geri ödendi ya da Geri ödendi (Ö3, Ö4, Ö5) | B-8 → müşteri | `02 §5.5`, §7.1.7, §7.4.5, §7.4.6, §7.4.8 · K-788, K-789 |
| 2.8.4.2 | Geri ödeme havalesi bankada gerçekleşmez | Yönetici geri ödemeyi işlemez, panelden "havale gerçekleşmedi" der: IBAN silinir ve sipariş sayfasında IBAN alanı yeniden açılır; müşteri yeni IBAN'ı girer. Geri ödemenin süresi durmaz | kalem: IBAN isteği | B-14 → müşteri | `02 §7.4.5` |
| 2.8.4.3 | Kart iadesi sağlayıcıda gerçekleşmez — iptalde, fesihte ya da caymada | Ödeme ekseni değişmez; panelde "geri ödeme gerçekleşmedi" işareti düşer. Yönetici yeniden dener ya da müşteriye havale yolunu açar; açınca sipariş sayfası kart iadesinin gerçekleşmediğini söyler ve IBAN alanı açar — IBAN girmek müşterinin seçimidir. IBAN girildikten sonra kart iadesi yeniden denenmez; süre durmaz | havale yolu açılınca kalem: IBAN isteği | Başarısızlıkta müşteriye ayrı bildirim yok (K-683) · havale yolu açılınca B-14 → müşteri | `02 §5.5`, §7.2.8 · K-683 |
| 2.8.4.4 | Müşteri caymayı sipariş sayfası yerine e-postayla, mektupla ya da örnek cayma formuyla bildirir | Yönetici bildirimin siparişin sahibinden geldiğini teyit eder ve panelden kaydeder (§8.4); beyanın tarihi bildirimin firmaya ulaştığı tarihtir. Havale hattında IBAN yalnız siparişin iletişim e-postasından gelen bildirimden aktarılır; öteki hâllerde kayıt IBAN'sız yapılır ve sipariş sayfasında IBAN alanı açılır. Kargoya verilmemiş fiziksel kalemde ve ödemesi beklenen siparişin hizmet kaleminde cayma kaydı açılmaz — yönetici "müşteriyle anlaşıldı" sebebiyle iptal eder (§2.7). Pencere kapandıktan sonra ulaşan bildirim — süresinde gönderilmiş ve geç ulaşmış ya da eksik bilgilendirme gerekçesiyle gelen — ve mutlak istisna kalemindeki bildirim firmanın değerlendirmesidir; kayıt ayrıca onayla yapılır (§5.2.3; K-709) | kalem: cayma beyanı | B-9 → müşteri · IBAN'sız kayıtta B-14 → müşteri; firmaya — (`02 §9.3.2`) | `02 §7.3.5`, §8.3.8, §9.3.2, §10.4.10 · K-673, K-709 |

### 2.9 Müşteri: ayıp talebi

Sebepli üçüncü yoldur (`02 §7.5`). Talebin durumları §1.9.1'de, firmanın adımları §8.4'tedir.

| # | Aktör ne yapar | Sistem ne denetler, ne yapar | Durum | Bildirim | Kaynak |
|---|---|---|---|---|---|
| 2.9.1 | Müşteri sipariş sayfasında "sorun bildir"i görür | Fiziksel kalemde sipariş Kargoya verildi'ye geçtiği andan, dijital kalemde ödeme onayından, hizmet kaleminde "tamamlandı" işaretinden sonra ve kalemde talep yokken görünür. Süre kalemin teslim işaretinin tarihinden iki yıldır (Z-18); fiziksel kalemde teslim tarihi girilmemişse işlemez. Süre dolunca kanal kapanır; hak iletişim formundan sürer. Ayıp talebi iletişim formunun konu tiplerinde yoktur | — | — | `02 §7.5.1`, §7.5.2 · K-676 |
| 2.9.2 | Müşteri kalemi ve — adedi birden büyükse — ayıplı adedi seçer ve sorunu açıklar | Talep ayıplı adedi taşır (K-793). Fotoğraf ve dosya yüklenmez — firma kanıtı e-postayla ister. Fiziksel kalemde ekran firmanın güncel iade adresini gösterir ve adres talebe yazılır. Ekran aydınlatma metninin bağlantısını gösterir. Talep kalem ve sipariş bağlamıyla panele düşer | kalem: ayıp talebi — Açık · sevkiyat hattı değişmez | B-9 → müşteri — talebin tarihi ve iade adresi · F-3 → firma | `02 §3.1.7`, §3.33.5, §5.8, §7.5.2 · K-665, K-793 |
| 2.9.3 | Müşteri talep açıkken aynı kalemde yeni bir sorun bildirmek ister | Yeni talep açılmaz; sipariş sayfası "sorun bildir" yerine talebin durumunu gösterir. Ek bilgi firmanın e-postasına yazılır | — | — | `02 §5.10` · K-676 |
| 2.9.4 | Firma çözümü müşteriyle sistemin dışında yürütür | Seçimlik haklar — onarım, değişim, bedel indirimi, sözleşmeden dönme — ürünün dışındadır. Geri gönderim gerekiyorsa müşteri malı iade adresine karşı ödemeli gönderir; masraf firmanındır. Para gerekiyorsa sözleşmeden dönmede geri alınan adetlerin iade hattı — tutarı `02 §7.1.7`'yle —, bedel indiriminde tutar bazlı kısmi geri ödeme işler (§8.4); havale hattında IBAN müşteriden sipariş sayfasıyla istenir (§1.11.39). Ayıplı dijital üründe doğal çözüm dosya güncellemesidir | Para gönderilirse Ö3–Ö5 · havalede kalem: IBAN isteği | Geri ödemede B-8 → müşteri · IBAN isteğinde B-14 | `02 §7.4.3`, §7.4.4, §7.5.3, §7.5.4 · K-686, K-788, K-793 |
| 2.9.5 | Yönetici talebi "çözüldü" işaretler | Çözümü firma müşteriye kendi cevabıyla yazmıştır | Ayıp talebi: Açık → Çözüldü | — (`02 §9.2`) | `02 §5.10` |
| 2.9.6 | Müşteri çözüm tutmadığında talebi yeniden açar — Z-18 içinde | Yeni talep açılmaz; ayıbın geçmişi tek kayıtta kalır. Aynı kalemin yeni sorunu da bu yolla bildirilir | Ayıp talebi: Çözüldü → Açık | B-9 → müşteri — yeniden açmanın tarihi (K-681) · F-3 → firma | `02 §5.10`, §9.2, §9.3 · K-681 |

### 2.10 Uçtan uca anlatılar

If/then'e sığmayan senaryolar ve fiziksel siparişin uçtan uca ana akışı (K-651). Anlatı önceki alt bölümlerin satırlarına işaret eder; kural ve süre tekrarlanmaz.

#### 2.10.1 Fiziksel siparişin ana akışı (akış 1 + 6)

`02 §2.2`'nin sekiz adımı, aktörü ve durumuyla. Kart hattında yöneticinin zorunlu elle adımı iki, havale hattında üçtür (§1.5; `02 §10.5.1`).

| # | Aktör ne yapar | Sistem ne denetler, ne yapar | Durum | Bildirim | Kaynak |
|---|---|---|---|---|---|
| 2.10.1.1 | Ziyaretçi ürünü kategori gezinmesiyle ya da aramayla bulur ve varyantını seçer | 2.1.2–2.1.6 | — | — | `02 §2.2` adım 1 |
| 2.10.1.2 | Müşteri varyantı adediyle sepete ekler | Sepet canlıdır; stok ayrılmaz (2.3.1) | — | — | `02 §2.2` adım 2 |
| 2.10.1.3 | Müşteri ödeme adımında adresleri, varsa kuponu ve ödeme yöntemini girer | 2.4.1–2.4.5 | — | — | `02 §2.2` adım 3 |
| 2.10.1.4 | Müşteri özeti ve Ön Bilgilendirme Formu'nu görür, iki kutuyu işaretler ve onaylar | Onay anında yeniden değerlendirme (2.4.6–2.4.8) | — | — | `02 §2.2` adım 4 |
| 2.10.1.5 | Sistem siparişi oluşturur | Numara, donma, ayırma (2.4.9) | Alındı + Bekliyor | B-1 → müşteri — havalede B-2 de · F-1 → firma | `02 §2.2` adım 5 |
| 2.10.1.6 | Müşteri öder — kartta 3D Secure'la; havalede yönetici "ödendi" işaretler | Ödeme onayı (2.5.1, 2.5.2, 2.5.3.1); kargoya verme süresi başlar | → Hazırlanıyor + Ödendi (Ö1, S1) | B-4 → müşteri | `02 §2.2` adım 6 |
| 2.10.1.7 | Yönetici siparişi hazırlar ve kargoya verir | 2.5.3.3; §8.2 | → Kargoya verildi (S5) | B-5 → müşteri | `02 §2.2` adım 7 |
| 2.10.1.8 | Mal ulaşır; yönetici teslim işaretini ve tarihini girer | 2.5.3.5; cayma penceresi ve ayıp talebinin süresi bu tarihten işler | → Teslim edildi (S6) | — (`02 §9.2`) | `02 §2.2` adım 8 |

Müşteri her adımı sipariş sayfasından izler (§2.6). Sipariş Teslim edildi'de kalır; bundan sonraki yollar cayma (§2.8) ve ayıp talebidir (§2.9).

#### 2.10.2 Karışık sipariş — fiziksel, dijital ve hizmet kalemi

Üye bir masa lambası (fiziksel), lambanın kurulum kılavuzu (dijital) ve bir saatlik çevrim içi aydınlatma danışmanlığı (hizmet) alır ve kartla öder.

1. **Onay.** Sepette dijital ve hizmet kalemi olduğu için iki kutunun yanında iki kutu daha çıkar (2.4.7). Sipariş Alındı + Bekliyor doğar; B-1 gider (2.4.9).
2. **Ödeme onayı.** Sağlayıcının başarı bildirimiyle Ö1 ve S1 birlikte işler: sipariş Hazırlanıyor + Ödendi olur. Kılavuz o anda teslim edilir ve B-4 indirmenin hazır olduğunu söyler; kılavuzun iptal ve cayma yolu kapanır. Kargoya verme süresi lamba için, ifa süresi danışmanlık için başlar (2.5.3.1; §1.4.2.1).
3. **Danışmanlığın cayma penceresi.** Sipariş onay anında fiziksel kalem taşıdığı için danışmanlığın penceresi siparişin teslim tarihinden işler; teslim tarihi girilene kadar hak açıktır (2.8.2.1).
4. **Olağan yol.** Yönetici lambayı kargoya verir (S5, B-5) ve teslimi işaretler (S6): sipariş Teslim edildi olur. Danışmanlık kalemi siparişin hattını beklemez; yönetici danışmanlığı verince "tamamlandı" işaretler ve danışmanlıktan cayma hakkı düşer (2.5.3.6).
5. **Dal — lamba kargodan önce iptal edilir.** Müşteri sipariş Hazırlanıyor'dayken lambayı iptal eder (2.7.2): B-7 gider; sistem kart iadesini kendiliğinden başlatır (F-4) ve fiziksel kalemlerin tamamı kargodan önce iptal edildiği için kargo ücreti de iadeye eklenir; ödeme Kısmen geri ödendi olur (Ö3, B-8). Kupon hakkı dönmez — sipariş yaşamaktadır. Sipariş fiziksel kalemsiz hatta düşer ve Hazırlanıyor'da bekler (§1.5). Teslim tarihi hiç oluşmayacağı için danışmanlığın penceresi sipariş tarihine geri çekilmez: hak "tamamlandı" işaretine kadar sürer (`02 §7.3.3`). Yönetici danışmanlığı verip işaretler — ödeme Kısmen geri ödendi'deyken de (K-671) —; danışmanlık son açık kalemdir ve kılavuz teslim edilmiş olduğundan sipariş Teslim edildi'ye geçer (S10). Müşteri danışmanlıktan caysaydı sipariş yine Teslim edildi'ye — beyanla aynı anda — geçerdi (§1.4.3).

#### 2.10.3 Kargodayken cayılıp geri dönen gönderi

Misafir alıcı tek fiziksel kalemli siparişini havaleyle ödemiştir; sipariş Kargoya verildi + Ödendi.

1. **Beyan.** Müşteri mal yoldayken sipariş sayfasından cayar (2.8.1.2): iptal yolu kapanmıştır, cayma düğmesi açıktır. Beyan IBAN'ı ve iade adresini kayda yazar; B-9 müşteriye, F-3 firmaya gider. Kalem teslim işareti almadığı için beyan kalemi kapatır ve geri ödemenin on dört günü (Z-16) beyandan işler — malın dönüşü beklenmez (`02 §7.4.1`). Açık kalem kalmamıştır ama gönderi kargodadır: sipariş Kargoya verildi'de kalır.
2. **Dönüş.** Müşteri malı teslim almaz ve kargo onu firmaya döndürür. Yönetici "teslim edilemedi" işaretler (S7, B-6). Açık kalem kalmadığı için yönetici siparişi S9'un kapanışıyla kapatır: sebep seçmez, iptal kaydı ve ikinci bir geri ödeme açılmaz, B-7 gitmez (§1.4.3). Siparişte teslim edilmiş bir kalem — ör. bir dijital kalem — olsaydı S7 kapanışı tamamlar ve sipariş Teslim edildi'ye geçerdi (S11).
3. **Teslim alma.** Yönetici dönen malı teslim alma adımıyla işler — teslim tarihi olmadığı için ulaşma tarihinin alt sınırı uygulanmaz — ve kontrol edip stoğa ekler. Firma iptali bu kaleme uygulanmaz (§1.11.21).
4. **Geri ödeme.** Yönetici parayı müşterinin IBAN'ına gönderir ve panelden işler: kargoyla gönderilen fiziksel kalemlerin tamamından cayıldığı için kargo ücreti de eklenir; ödeme Geri ödendi olur (Ö4) ve B-8 gider. Süre beyandan işlemektedir; teslim alma adımı onu başlatmaz ve geri ödeme malın dönüşünü beklemez.

#### 2.10.4 IBAN silindikten sonra ulaşan iade malı

Üye havaleyle ödediği fiziksel kalemin tesliminden sonra cayar ve IBAN'ını beyanla girer.

1. **Beyan.** Kalem "iade malı bekleniyor" görünür; geri ödeme süresi mal ulaşmadan başlamaz (2.8.1.2, 2.8.1.8).
2. **Mal gelmez.** Müşteri malı Z-42 içinde göndermez; sistem kendiliğinden bir işlem başlatmaz. Yönetici kalemi "mal dönmedi" gerekçesiyle kapatırsa IBAN kapatmayla silinir, kalem bekleyen işlerden düşer ve müşteriye bildirim gitmez; kapatmazsa IBAN Z-38 dolunca kendiliğinden silinir, kalem açık kalır ve cayma geçerliliğini korur (`02 §7.4.5`, §10.4.9).
3. **Mal ulaşır.** Yönetici teslim alma adımında ulaşma tarihini girer; kapatılmış kalem yeniden açılır. IBAN silinmiş olduğu için adımla birlikte IBAN isteği düşer: müşterinin sipariş sayfasında o kalem için IBAN alanı açılır ve B-14 gider. Yönetici IBAN'ı panelden girmez.
4. **Bekleme.** Geri ödemenin on dört günü (Z-16) ulaşma tarihinden işler ve IBAN beklenirken durmaz; panel kalemi "IBAN bekleniyor" olarak, kalan süreyle gösterir.
5. **Kapanış.** Müşteri IBAN'ı sipariş sayfasından girer; yönetici geri ödemeyi işler (Ö3–Ö5, B-8) ve IBAN geri ödeme tamamlanınca silinir. Firma "mal dönmedi" kapanışından sonra müşterinin bildirimi üzerine ödemeye karar vermişse ödediği tutar kalemin geri ödemesinden düşülür (`02 §10.4.9`). Müşteri IBAN'ı hiç girmezse kalem kapanmaz ve borç sürer; kalem "IBAN bekleniyor" listesinde görünür ama bekleyen işler sayacında sayılmaz (`02 §7.4.5`).

#### 2.10.5 Misafir siparişinin e-posta düzeltmesi

Misafir alıcı ödeme adımında e-postasını yanlış yazar ve havaleyle sipariş verir.

1. **Yanlış adres.** Onay özeti adresi açıkça gösterir ama müşteri hatayı görmez (2.4.6). B-1 ve B-2 yanlış adrese gider; yanlış adres bir müşteri hesabınınsa sipariş o hesaba düşer (`02 §3.13.3`).
2. **Ödeme.** Müşteri IBAN'ı ve sipariş numarasını ekranda görmüştür ve havaleyi yapar; yönetici "ödendi" işaretler (2.5.2.4) ve B-4 de yanlış adrese gider. Sipariş sayfasına giden iki yol da yanlış adrese bağlı olduğu için müşteri sayfayı açamaz (2.6.7).
3. **Teyit.** Müşteri firmaya telefonla, iletişim formuyla ya da e-postayla ulaşır. Yönetici kimliği siparişteki bilgilerle — ad, teslimat telefonu, kalemler ve tutar — teyit eder; sipariş numarası tek başına teyit değildir (§6.2, §8.4).
4. **Düzeltme.** Yönetici e-postayı düzeltir: yeni erişim anahtarı üretilir ve eski bağlantılar geçersizleşir; yeni adrese B-1 — donmuş sürümüyle ve yeni bağlantıyla —, eski adrese B-15 gider. Sipariş yanlış adresin hesabına düşmüşse bağ kesilir; yeni adres doğrulanmış bir müşteri hesabınınsa sipariş o hesaba düşer. Panel siparişin satırında "e-posta düzeltildi" işaretini gösterir; eski ve yeni adres siparişin e-posta geçmişinde durur. Durumlar, süreler, kalemler ve IBAN değişmez (`02 §10.4.11`).
5. **Devam.** Müşteri yeni bağlantıyla sipariş sayfasına girer ve akışı sürdürür. Düzeltmeyi gerçek sahip istemediyse B-15 onu uyarır; yönetici e-postayı yeniden düzeltir, ama arada yapılan işlemler — iptal, cayma, IBAN girişi — geri alınmaz ve kalem kayıtlarından ve e-posta geçmişinden okunur — işlem izi yalnız yönetici işlemlerini tutar (K-710); kandırılmanın sonucu firmadadır (§6.2; `02 §8.3.6`).

## 3. Hata akışları

> **Ne yazılır:** Timeout, iptal, doğrulama hatası, dış servis hatası senaryoları.

Bu bölüm `02 §6`'nın hata ve istisna envanterini akışa bağlar: her satır bir ana akış adımından ayrılır ve sistemin cevabını, durumun ne olduğunu ve kime bildirim gittiğini yazar. `02 §6`'nın her satırı burada **tam bir kez** "Kaynak" olur — altı ilke (§3.1) ve yüz kırk bir senaryo (§3.2–§3.5); diziliş `02 §6`'nınkidir. Kuralın tam metni `02`'dedir (§0.5.1). Sürelerin kendisi §4'te, itiraz ve ürünün dışında çözülen anlaşmazlıklar §5'te, kötüye kullanım ve teyit §6'dadır; buradaki satır oraya işaret eder.

- **Biçim:** §0.4.2. "Akış adımı" §2'nin, §8'in ya da §9'un satırıdır. §8 ve §9 yazılana kadar firma tarafının ve üyeliğin hücreleri hedef alt bölümü ve §1 satırını taşıyordu (K-682); §8'e bağlanan hücreler 4. oturumda (v0.5), §9'a bağlananlar 5. oturumda (v0.6) satır numarasına çevrildi.
- **Bildirim:** "—" ekranda reddedilen ve kayıt doğurmayan işlem içindir. Bir kaydı ya da durumu değiştiren olay bildirim kimliğini ya da yokluğunun kaynağını taşır; hata olayın bildirimini değiştirmiyorsa hücre "olağan" der ve ana akışın satırını gösterir (§0.4.1; K-682).
- **Durum:** §1'e uyar (§0.4.4); "—" sevkiyat ekseninin, ödeme ekseninin ve kalem kaydının değişmediğini söyler.

### 3.1 Bütün akışlara uygulanan ilkeler

| # | İlke | Akışta nerede işler | Kaynak |
|---|---|---|---|
| 3.1.1 | Hiçbir sipariş ya da talep akışı bir e-postanın ulaşmasına bağlı değildir: sipariş sayfası her bilgiyi taşır; ulaşmayan müşteri bildirimi Z-26'nın denemelerinden sonra siparişin panel satırına "e-posta ulaşmadı" işaretini düşürür ve yönetici e-postayı yeniden gönderir; firmaya giden bildirim satıra işaret düşürmez, panelin ana sayfasında uyarı çıkarır (§8.9.1). Hesap ve yönetim bağlantıları adresin sahibini kanıtlamak için bilerek kuralın dışındadır — kullanıcı bağlantıyı ekrandan yeniden ister (L-9), davette yeni davet gönderilir | §2.6.3–§2.6.5, §2.10.5 · §1.7.3.1 · §3.5.2.13 · §9.1.3, §9.2.4 · §8.8.1 · §10.1.2 | `02 §6.1.1`, §9.1.6, §9.4 · K-675, K-760 |
| 3.1.2 | Sistem müşterinin ödediği siparişi kendi bilgisizliği yüzünden iptal etmez: kart ödeme süresi dolunca iptalden önce son sorgu yapılır; sorgu yanıtsızsa sipariş Alındı + Bekliyor'da "ödeme sonucu alınamadı" işaretiyle bekler; sitenin kesintisinde dolan havale süresinde iptal ertelenir | §2.5.1.4, §2.5.2.5 · §1.4.3, §1.7.3.2 · §3.2.1.2, §3.5.2.17 · §4.2.11, §4.2.12 | `02 §6.1.2`, §4.3 · K-667 |
| 3.1.3 | Onaylanan özet ile oluşan sipariş birebir aynıdır; özeti değiştiren her fark siparişin oluşmasını durdurur ve müşteriye neyin değiştiği söylenir | §2.4.8 · §3.2.1.3, §3.2.1.4 | `02 §6.1.3`, §3.17.2 |
| 3.1.4 | Sepet hatalarda korunur — ödeme yarıda kaldığında, ödeme sağlayıcısına erişilemediğinde ve satış kapalıyken | §2.1.9, §2.5.1.3, §2.5.1.4 · §3.2.1.1, §3.2.1.2, §3.2.1.15, §3.2.1.17 | `02 §6.1.4` |
| 3.1.5 | Limit aşımı nötr konuşur: mesaj kalan deneme sayısını, engelin süresini ve ekseni söylemez; hesap kilitlenmez | §6.1 · §3.2.1.18, §3.2.1.19, §3.2.1.21, §3.2.2.2, §3.4.11, §3.4.14, §3.5.3.8 | `02 §6.1.5`, §8.2.1 |
| 3.1.6 | Hata ve uyarının anlamı yalnız renkle taşınmaz — "Tükendi" işareti dahil her hata ve uyarı metin taşır | Bütün akışlar; biçimi `04`'ün işidir | `02 §6.1.6`, §3.34.5 |

### 3.2 Satın alma ve sipariş takibi

#### 3.2.1 Satın alma (akış 1)

| # | Akış adımı | Ne ters gider | Sistemin cevabı | Durum | Bildirim | Kaynak |
|---|---|---|---|---|---|---|
| 3.2.1.1 | §2.5.1.3 | Kart ödemesinde 3D Secure doğrulaması başarısız olur ya da kart reddedilir | Ödeme gerçekleşmez; sipariş kendiliğinden iptal edilir ve ayrılanlar aynı anda serbest kalır. Sepet olduğu gibi durur; müşteri sepetten yeni bir siparişle yeniden dener (§2.4.8). İptal L-7'ye sayılır; sipariş müşterinin listesinde görünmez, panelde görünür | → İptal edildi + Başarısız (S3, Ö2) | B-7 → müşteri (§1.2 S3) | `02 §6.2.1`, §3.17.5, §3.17.8 |
| 3.2.1.2 | §2.5.1.4 | Müşteri banka ekranını kapatır ya da ödeme yarıda kalır | Sepet olduğu gibi durur. Z-7 dolunca son sorgu yapılır: ödeme alınmışsa onaylanır, alınmamışsa sipariş kendiliğinden iptal edilir, sorgu yanıtsızsa sipariş bekler (3.1.2). Müşteri o arada aynı sepetten yeniden onaylarsa önceki sipariş hemen iptal edilir ve ayırması serbest kalır — müşteri kendi ayırdığı stoğa takılmaz | → Ödendi (Ö1) · → İptal edildi + Başarısız (S3, Ö2) · ya da Alındı + Bekliyor | Ö1'de B-4 · iptalde B-7 → müşteri · yanıtsız sorguda — (K-683) | `02 §6.2.2`, §6.1.2 · K-683 |
| 3.2.1.3 | §2.4.8 | Onay anında fiyat, indirim, kupon, kargo ücreti, ücretsiz kargo eşiği ya da teslimat illeri değişmiştir, bir kalem ayrılamaz ya da Ön Bilgilendirme Formu'nun veya sözleşmenin sürümü artmıştır | Sipariş oluşmaz; müşteri neyin değiştiğini söyleyen güncel özeti ve metinleri görür ve yeniden onaylar — metin değiştiyse kutuları yeniden işaretler. Ayrılamayan kalemde sayı söylenmez | — · sipariş doğmaz | — | `02 §6.2.3`, §3.17.2 |
| 3.2.1.4 | §2.3.4 · §2.4.8 | Sepetteki kalem tükenmiştir ya da hizmetin kontenjanı dolmuştur | Kalem "Tükendi" işaretiyle sepette kalır ve siparişe girmez; fark güncel özette söylenir, özet gösterildikten sonra oluşan fark siparişi durdurur. Siparişe girebilecek kalem kalmadıysa ödeme adımı açılmaz; onay anında oluştuysa sipariş oluşmaz ve müşteri sepete döner | — | — | `02 §6.2.4`, §3.16.6, §3.16.7 |
| 3.2.1.5 | §2.3.5 | Sepetteki ürün ya da varyant arşive ya da taslağa alınmış veya kalıcı silinmiştir | Kalem sepetten çıkar ve müşteriye bir kez söylenir | — | — | `02 §6.2.5`, §3.16.7 |
| 3.2.1.6 | §2.3.1 · §2.3.4 | Müşteri stoktan fazla adet seçer ya da stok sepetteki adedin altına düşer | "Bu adette stok yok" — adet söylenmez; kalem adet düşürülene kadar siparişe girmez | — | — | `02 §6.2.6`, §3.16.8 |
| 3.2.1.7 | §2.3.1 · §2.3.4 | Müşteri bir siparişte alınabilecek adedi (P-9) aşar | Ürün sepete eklenmez ve sınır sayısıyla söylenir; sınır sonradan düşerse kalemler mesajla sepette kalır | — | — | `02 §6.2.7`, §3.16.9, §3.16.10 |
| 3.2.1.8 | §2.3.1 | Aynı dijital varyant sepete ikinci kez eklenir | "Bu ürün zaten sepetinde" | — | — | `02 §6.2.8`, §3.12.4 |
| 3.2.1.9 | §2.3.1 | Üye daha önce aldığı dijital varyantı yeniden sepete ekler | Engellenmez; "Bu ürünü daha önce aldınız — sipariş sayfanızdan indirebilirsiniz" uyarısı gösterilir | — | — | `02 §6.2.9`, §3.12.9 |
| 3.2.1.10 | §2.4.2 | Teslimat adresi firmanın teslimat yapmadığı bir ildedir | Adres kabul edilmez ve sebebi söylenir; defterdeki o adres seçilemez | — | — | `02 §6.2.10`, §3.20.1 |
| 3.2.1.11 | §2.3.8 | Fiziksel kalemli sepetin tutarı asgari sipariş tutarının altındadır | Ödeme adımına geçilmez; eksik tutar gösterilir | — | — | `02 §6.2.11`, §3.18.1 |
| 3.2.1.12 | §2.4.4 | Kupon toplamı sıfıra indirir, tarih aralığının dışındadır ya da kullanım hakkı dolmuştur | Kupon kabul edilmez | — | — | `02 §6.2.12`, §3.10.4, §3.10.5 |
| 3.2.1.13 | §2.4.1 · §2.4.6 · §2.6.7 | Misafir alıcı e-posta adresini yanlış yazar | Özet adresi açıkça gösterir ve onaydan önce düzeltmeye açıktır; doğrulama kodu istenmez. Hata onaydan sonra fark edilirse müşteri sipariş sayfasına giremez; firmaya ulaşır, yönetici kimliği teyit edip siparişin e-postasını düzeltir (§2.10.5; teyit §6.2.1). Yanlış adres bir müşteri hesabınınsa sipariş o hesaba düşmüştür; düzeltmede bağ kesilir | Durumlar değişmez | Düzeltmede B-1 → yeni adres · B-15 → eski adres | `02 §6.2.13`, §10.4.11 |
| 3.2.1.14 | §2.4.1 · §2.4.9 | Hesabı olan kişi giriş yapmadan misafir olarak sipariş verir | Sipariş kabul edilir ve hesaba anında düşer; ödeme adımında giriş hatırlatması gösterilir | Olağan (§2.4.9) | Olağan (§2.4.9) | `02 §6.2.14`, §3.13.3 |
| 3.2.1.15 | §2.4.5 · §2.4.9 | Ödeme sağlayıcısına erişilemez | Onaydan önce: kart "şu an kullanılamıyor" görünür ve seçilemez; havale açıksa müşteri ona yönelir, değilse sipariş verilemez ve müşteri sepetine döner. Onaydan sonra sağlayıcıya bağlanılamaz ve ödeme sayfası hiç açılmazsa bu kart ödemesinin başarısızlığıdır: sipariş kendiliğinden iptal edilir, ayrılanlar serbest kalır ve sipariş müşterinin listesinde görünmez; iptal L-7'ye **sayılmaz**. Sonradan başarı bildirimi gelirse 3.2.1.16 işler. Erişilemezliğin tespiti `08`'in işidir; kesinti akışı §10.1.1 | Onaydan sonra → İptal edildi + Başarısız (S3, Ö2) | Onaydan sonra B-7 → müşteri (§1.2 S3) | `02 §6.2.15`, §8.2.3 |
| 3.2.1.16 | §2.4.9 · §2.5.1.4 | İptal edilmiş bir siparişe sağlayıcıdan başarılı ödeme bildirimi gelir — iki sekmeden aynı sepetle ödeme, kendiliğinden iptalden sonra tamamlanan eski deneme | Sistem ödemeyi sağlayıcı üzerinden kendiliğinden geri öder. Sipariş İptal edildi + Başarısız kalır ve satış özetinde ödenmiş sayılmaz; ödeme ve geri ödemesi siparişin ödeme kaydında durur ve sipariş Z-28'i izler. Havale hattındaki karşılığı 3.5.2.2'dir | — (ekseni değiştirmez — §1.3) | B-8 → müşteri (K-684) · F-4 → firma | `02 §6.2.16`, §5.5 · K-684 |
| 3.2.1.17 | §2.1.9 · §2.3.8 | Satış kapalıdır — kapının bir koşulu eksiktir ya da firma satışı geçici olarak kapatmıştır | Ürünler görünür; sepete ekleme ve ödeme kapalıdır ve ziyaretçi satışın kapalı olduğunu görür; sepet korunur. Açık siparişler etkilenmez (§1.10.8) | — | — | `02 §6.2.17`, §3.1.5 · K-664 |
| 3.2.1.18 | §2.4.4 | Sepette art arda geçersiz kupon kodu denenir | L-6 aşılınca o sepetin kupon alanı geçici olarak kapanır; mesaj nötrdür (§6.1) | — | — | `02 §6.2.18`, §8.2 |
| 3.2.1.19 | §2.4.8 | Aynı IP'den ya da — giriş yapmış üyede — aynı e-postadan kendiliğinden iptal edilen ödenmemiş siparişler L-7'nin eşiğini aşar | Kullanıcı bir süre yeni sipariş onaylayamaz; mesaj nötrdür; firma o siparişleri panelde görür. Misafir siparişi e-posta ekseninde, ödeme sayfası hiç açılmamış sipariş hiçbir eksende sayılmaz (§6.1.4) | — | — | `02 §6.2.19`, §8.2.2, §8.2.3 |
| 3.2.1.20 | §2.4.7 | Müşteri siparişe not ya da teslimat talimatı eklemek ister | Ödeme adımında not alanı yoktur; müşteri iletişim formunun "Sipariş hakkında" tipiyle yazar (§2.2.8) | — | — | `02 §6.2.20`, §3.24.7 |
| 3.2.1.21 | §2.4.5 | Açık ödenmemiş sipariş tavanı (L-8) doluyken havale seçilmek istenir | Havale seçilemez ve mesaj nötrdür; kart yolu açık kalır. Açık siparişlerden biri ödenince ya da iptal edilince tavan boşalır | — | — | `02 §6.2.21`, §8.2 |
| 3.2.1.22 | §2.5.3.1 — ödenmiş kart siparişi | Müşteri bankası üzerinden harcama itirazı yapar ve sağlayıcı tutarı firmanın hesabından geri alır | Ürün bunu bilmez; ödeme ekseni ve satış özeti değişmez. Akış §5.2.1 | — | — (ürün olayı bilmez — `02 §3.21.11`) | `02 §6.2.22`, §3.21.11 |

#### 3.2.2 Sipariş takibi (akış 2)

| # | Akış adımı | Ne ters gider | Sistemin cevabı | Durum | Bildirim | Kaynak |
|---|---|---|---|---|---|---|
| 3.2.2.1 | §2.6.5 | Sipariş e-postası müşteriye ulaşmaz | Müşteri her bilgiye sipariş sayfasından ulaşır — misafir alıcı numara ve e-postayla girer. E-posta yanlış yazıldığı için ulaşmıyorsa bu yol da kapalıdır ve 3.2.1.13 işler | — | — · panelde "e-posta ulaşmadı" işareti (`02 §9.1.6`) | `02 §6.3.1`, §6.1.1 |
| 3.2.2.2 | §2.6.2 | Misafir alıcı sipariş numarasını ya da e-postayı yanlış girer | Sayfa açılmaz. Başarısız sorgular IP'den ve sorguya yazılan e-postaya yönelik olarak ayrı ayrı L-5 ile limitlidir; e-posta ekseninde engellenen gerçek sahip e-postadaki bağlantıdan ya da üyeyse hesabından girer | — | — | `02 §6.3.2`, §8.2.2 |
| 3.2.2.3 | §2.6.6 | Hesap silinmiştir ama sipariş yürümektedir | Takip, cayma ve iade misafir yolundan — numara ve siparişe donmuş e-posta — ya da e-postadaki bağlantıyla sürer | — | — | `02 §6.3.3`, §3.15.2 |
| 3.2.2.4 | §2.6.1 | Müşterinin ödemesi hiç alınmamış ve kendiliğinden iptal edilmiş siparişleri vardır | Müşterinin listesinde görünmezler, panelde görünürler; müşterinin ya da firmanın iptal ettiği ödenmemiş sipariş listede görünür | — | — | `02 §6.3.4`, §3.17.8 |
| 3.2.2.5 | §2.6.7 · §2.10.5 | Misafir siparişinin e-postası gerçek sahibi istemeden düzeltilir | Eski adrese B-15 gider. Gerçek sahip firmaya ulaşır; yönetici e-postayı yeniden düzeltir ve yeni bir anahtar üretilir. Arada yapılan işlemler geri alınmaz, kalem kayıtlarından ve e-posta geçmişinden okunur (K-710). Akış §6.2.4 | Durumlar değişmez | B-15 → eski adres · yeniden düzeltmede B-1 → yeni adres, B-15 → eski adres | `02 §6.3.5`, §8.3.6 · K-710 |
| 3.2.2.6 | §2.6.4 · §8.4.1 | Siparişin teslimat adresi gerçek sahibi istemeden düzeltilir | Yönetici talebi siparişin kanalına dönerek teyit eder; düzeltme B-10'la siparişin e-postasına bildirilir. Sahibi Kargoya verildi'den önce ulaşırsa yönetici adresi geri düzeltir; paket yola çıktıysa düzeltilemez ve sonuç firmadadır. Akış §6.2.2 | — | B-10 → müşteri | `02 §6.3.6`, §10.4.2 |

### 3.3 İptal, cayma ve iade (akış 3)

| # | Akış adımı | Ne ters gider | Sistemin cevabı | Durum | Bildirim | Kaynak |
|---|---|---|---|---|---|---|
| 3.3.1 | §2.7.2 · §2.8.1.1 | Müşteri, sipariş kargoya verildikten sonra fiziksel kalemi iptal etmek ister | İptal düğmesi sunulmaz; yol caymadır — cayma düğmesi Kargoya verildi'yle açılmıştır | — | — | `02 §6.4.1`, §7.3.7 |
| 3.3.2 | §2.7.2 · §2.7.7 | Firma kargoya verme süresini (Z-10) ya da yasal teslim sınırını (Z-11) aşar | Z-10 aşıldığında sipariş kargoya verilmemiştir; müşterinin iptal düğmesi açıktır ve fesih onunla kullanılır — panel firmaya kanuni faiz uyarısını gösterir. Z-11 aşıldığında — sipariş kargodayken de — gecikme feshi düğmesi açılır; kişiye özel üretim işaretli üründe de sözleşmenin taahhüdü olarak açılır (§4.1.11) | İptalde kalem: iptal kaydı · fesihte kalem: gecikme feshi (§2.7.7) | İptalde B-7 → müşteri · fesihte B-9 → müşteri, F-3 → firma | `02 §6.4.2`, §7.2.3 |
| 3.3.3 | §2.7.4 · §2.8.3.1 | Müşteri ödemesi onaylanmış dijital kalemi iptal etmek ya da ondan caymak ister | İki düğme de sunulmaz: kalem teslim edilmiş, hak ön onayla düşmüştür. Ayıplı dosyada yol ayıp talebidir | — | — | `02 §6.4.3`, §7.2.5, §7.3.2 |
| 3.3.4 | §2.8.1.1 · §2.8.1.2 | Müşteri cayma istisnası işaretli bir kalemden cayar | Mutlak sebepte düğme yoktur. Koşullu sebepte düğme açıktır, beyan ekranı koruyucu ambalaj koşulunu yazar ve beyan kayda geçer; koruyucu ambalajı açılmış dönen malın reddi yöneticinin teslim alma adımındaki seçimidir (3.3.35, §5.3.1) | Koşullu sebepte kalem: cayma beyanı | Koşullu sebepte B-9 → müşteri, F-3 → firma | `02 §6.4.4`, §7.3.4 |
| 3.3.5 | §2.8.2.1 | Müşteri tamamlanmış bir hizmetten caymak ister | Düğme sunulmaz; hak "tamamlandı" işaretiyle düşmüştür. Sebepli sorun ayıp talebinden yürür (§2.9) | — | — | `02 §6.4.5`, §7.3.3 |
| 3.3.6 | §2.8.1.1 | Cayma penceresi (Z-13) geçmiştir | Düğme kapanır; önceki beyan geçerli kalır. Sebepli sorun ayıp talebinden yürür; eksik bilgilendirme iddiası §5.2.3'tedir | — | — | `02 §6.4.6`, §7.3.1 |
| 3.3.7 | §2.7.5 · §2.8.1.2 | Havale ile ödenmiş siparişin parası geri ödenecektir | Müşteri IBAN'ını iptal, gecikme feshi ya da cayma beyanı sırasında girer; kart hattında IBAN istenmez. Bu üç olayın dışında doğan havale geri ödemesinde IBAN müşteriden istenir (K-686) | — | — | `02 §6.4.7`, §7.4.5 · K-686 |
| 3.3.8 | §2.8.1.5 | Firma teslimden sonraki caymada geri ödemeyi iade malı ulaşana kadar yapmaz | Süre başlamamıştır: Z-16 ulaşma tarihinden işler. Teslimden önceki caymada ve hizmette süre beyandan işler ve mal beklenmez; panel kalan süreyi gösterir | — | — | `02 §6.4.8`, §7.4.1 |
| 3.3.9 | §2.5.3.4 · §8.3.1.1, §8.3.1.2 | Gönderi müşterinin adresi yanlış vermesi ya da teslimatta bulunmaması yüzünden geri döner ve sipariş iptal edilir | Yönetici "gönderi teslim edilemedi — müşteri kaynaklı" sebebini seçer: ürün bedeli geri ödenir, gidiş kargo bedeli ödenmez. Öteki sebeplerde gidiş kargosu da geri ödenir | → İptal edildi (S9) — teslim edilmiş kalem varsa geri dönen kalemlerin iptaliyle → Teslim edildi (S11) · geri ödemede Ö3–Ö5 · havale hattında kalem: IBAN isteği | B-7 → müşteri · geri ödemede B-8; kartta sistemin iadesinde F-4 → firma · havale hattında B-14 (K-690) | `02 §6.4.9`, §7.2.9 · K-690 |
| 3.3.10 | §2.7.2 · §2.8.4.1 | Kısmi iptal ya da iadeden sonra — kalemin bir kısmının (adet) iptali ya da iadesinden sonra da — kalan tutar kuponun asgari tutarının ya da ücretsiz kargo eşiğinin altına düşer | Kupon ve kargo yeniden hesaplanmaz; müşteriden bir şey geri istenmez. Kargo ücreti yalnız iki tetikte geri ödenir — kargodan önce bütün fiziksel kalemlerin bütün adetlerinin iptali, kargoyla gönderilenlerin bütün adetlerinden cayma; bir adet bile kargoyla gidiyor ya da müşteride kalıyorsa ödenmez (§1.4.4, §1.7.4) | — | — | `02 §6.4.10`, §7.2.9, §7.4.6 · K-789 |
| 3.3.11 | §2.8.1.2 · §2.10.3 | Müşteri, sipariş kargoya verildikten sonra ve mal eline ulaşmadan vazgeçer | İptal yolu kapalıdır, cayma beyanı açıktır; beyan teslim işaretsiz kalemi kapatır ve geri ödeme süresi beyandan işler | kalem: cayma beyanı · eksenler değişmez | B-9 → müşteri · F-3 → firma | `02 §6.4.11`, §7.3.1 |
| 3.3.12 | §2.5.3.5 · §2.8.1.1 | Firma teslimi geç işaretler ve girdiği tarihe göre pencere işaret anında dolmuştur | Düğme kapalıdır; önceki beyan geçerlidir ve gecikmenin riski firmadadır. Pencere işaret anından yeniden başlamaz; tarihe itiraz §5.2.4'tedir | → Teslim edildi (S6) | — (`02 §9.2` — teslim işareti) | `02 §6.4.12`, §7.3.1 |
| 3.3.13 | §2.8.4.1 | Müşteri siparişin bir kaleminden cayar | Gönderilen fiziksel kalemlerden biri müşteride kalıyorsa kargo ücreti geri ödemeye girmez; tamamından cayıldığında — ardışık beyanlarla da — son caymanın geri ödemesine girer | — | — | `02 §6.4.13`, §7.4.6 |
| 3.3.14 | §2.8.1.4 | Müşteri iade kargo ücretini önden ödemek istemez | Malı iade adresine karşı ödemeli gönderir; bedeli teslim alırken firma öder | — | — | `02 §6.4.14`, §7.4.4 |
| 3.3.15 | §2.9.6 · §8.4.11 | Çözülmüş ayıp tekrarlar — onarılan mal yine bozulur | Müşteri ya da yönetici talebi yeniden açar; yeni talep açılmaz | Ayıp talebi: Çözüldü → Açık | Müşterinin açmasında B-9 → müşteri, F-3 → firma · yöneticininkinde — (`02 §9.2`) | `02 §6.4.15`, §5.10 · K-681 |
| 3.3.16 | §2.8.1.8 | Müşteri teslimden sonra cayar ama malı hiç göndermez | Geri ödeme süresi başlamaz; panel "iade malı bekleniyor"u beyandan geçen gün ve Z-42 ile gösterir; sistem kaleme dokunmaz. Z-42 geçince yönetici isteğe bağlı "mal dönmedi" kapanışını yapar: geri ödeme yapılmaz, havale hattında IBAN kapatmayla — kapatılmazsa Z-38 dolunca — silinir (§4.1.18, §4.1.47) | Yönetici kapatırsa kalem: "mal dönmedi" kapanışı | — (`02 §9.2`) | `02 §6.4.16`, §10.4.9 |
| 3.3.17 | §2.8.1.5 · §8.3.2.1 | Firma iade malının teslim alma işaretini geciktirir | Süre girilen ulaşma tarihinden işler; tarih geçmişe dönük girilir. Geç ya da uydurulmuş tarihin riski ve ispatı firmadadır | kalem: iade teslim alma | — (`02 §9.2`) | `02 §6.4.17`, §7.4.1 |
| 3.3.18 | §2.10.4 · §8.3.2.1 | Firma kalemi "mal dönmedi" diye kapattıktan sonra mal ulaşır | Teslim alma adımı kalemi yeniden açar; geri ödemenin on dört günü (Z-16) ulaşma tarihinden işler. Kapatma cayma beyanını geçersiz kılmamıştır | kalem: iade teslim alma | — (`02 §9.2`) · IBAN silinmişse 3.3.19 | `02 §6.4.18`, §10.4.9 |
| 3.3.19 | §2.10.4 | Havale hattında IBAN silindikten sonra mal ulaşır | Teslim alma adımıyla IBAN isteği düşer: sipariş sayfasında o kalem için IBAN alanı açılır; yönetici IBAN'ı panelden girmez. Süre ulaşma tarihinden işler ve IBAN beklenirken durmaz; panel kalemi "IBAN bekleniyor" gösterir | kalem: iade teslim alma · IBAN isteği | B-14 → müşteri | `02 §6.4.19`, §7.4.5 |
| 3.3.20 | §2.10.4 | Müşteri yeniden istenen IBAN'ı geç girer ya da e-posta ona ulaşmaz | Süre durmaz; sipariş sayfası alanı e-postadan bağımsız gösterir. IBAN isteğinin tarihi işlem izinde durur — gecikmenin müşteriden kaynaklandığının kanıtı | — | — | `02 §6.4.20`, §7.4.5 |
| 3.3.21 | §2.8.2.2 | Müşteri tamamlanmamış bir hizmet kaleminden cayar ve hizmet hiç ifa edilmez | Beyan kalemi kapatır; geri ödemenin on dört günü (Z-16) beyandan işler; kalem siparişin son açık kalemiyse sipariş kapanış kuralıyla hattını bitirir (§1.4.3) | kalem: cayma beyanı · son açık kalemse → Teslim edildi (S2, S10, S11) ya da → İptal edildi (S3, S4); Kargoya verildi ve Teslim edilemedi'de yöneticinin S7 ve S9 kapanışı (K-717) | B-9 → müşteri · F-3 → firma | `02 §6.4.21`, §5.6.6 · K-672, K-717 |
| 3.3.22 | §2.10.3 | Kargodayken cayılan gönderi teslim edilemeyip firmaya döner | Yönetici cayma beyanlı kalemi iptal etmez; dönen malı teslim alma adımıyla işler. Geri ödeme tek hattan — caymanınkinden — yürür; kargoyla gönderilenlerin tamamından cayıldıysa kargo ücreti eklenir. Firma iptalinin ikinci bir geri ödemesi açılmaz | → Teslim edilemedi (S7) → İptal edildi (S9 kapanışı) ya da → Teslim edildi (S11) | S7'de B-6 → müşteri · kapanışta B-7 gitmez | `02 §6.4.22`, §5.4 |
| 3.3.23 | §2.5.2.6 · §2.7.1 | Havale bekleyen müşteri siparişin yalnız bir kalemini — ya da bir kalemin bir adedini — iptal etmek ister | Ödeme onayından önce kalem ve adet iptali sunulmaz; müşteri siparişi bütünüyle iptal eder ve yeniden verir | İptal ederse → İptal edildi + Başarısız (S3, Ö2) | İptalde B-7 → müşteri | `02 §6.4.23`, §7.2.2 |
| 3.3.24 | §2.8.4.3 · §8.3.3.4, §8.3.3.5 | Kart hattındaki geri ödeme sağlayıcıda başarısız olur — sağlayıcı reddeder ya da ulaşılamaz | Ödeme ekseni değişmez; panelde "geri ödeme gerçekleşmedi" işareti düşer, kalem "iade ve geri ödeme bekleyen" sayacına girer ve firmaya panelde uyarı çıkar. Sistem kendiliğinden yeniden denemez. Yönetici yeniden dener ya da havale yolunu açar — havale müşterinin seçimidir; IBAN girildikten sonra kart iadesi yeniden denenmez. Z-16 ve Z-17 işlemeye devam eder | — · havale yolu açılınca kalem: IBAN isteği · kart iadesi gerçekleşince Ö3–Ö5 | Başarısızlıkta müşteriye ayrı bildirim yok, firmaya panel işareti ve sayaç (K-683) · havale yolu açılınca B-14 · kart iadesi gerçekleşince B-8 | `02 §6.4.24`, §7.2.8 · K-683 |
| 3.3.25 | §2.8.4.4 · §8.4.8 | Müşteri caymayı sipariş sayfası yerine e-postayla, mektupla ya da örnek formla bildirir — düğmesi kapanmış olsa da süresi içinde | Yönetici bildirimin siparişin sahibinden geldiğini teyit eder (§6.2.1) ve panelden kaydeder: kalemi, — adedi birden büyükse — cayılan adedi ve bildirimin firmaya ulaştığı tarihi girer — geçmişe dönük olabilir, ileri olamaz. Havale hattında IBAN yalnız siparişin iletişim e-postasından gelen bildirimden aktarılır; öteki hâllerde kayıt IBAN'sız yapılır. Kayıt bir cayma beyanıdır ve geri alınmaz (K-688). Süresinde gönderilmiş ama pencere kapandıktan sonra ulaşmış bildirim de aynı onayla kaydedilir; süreye uygunluk gönderimle ölçülür, kaydın tarihi ulaşma tarihidir (K-709) | kalem: cayma beyanı · IBAN'sızsa IBAN isteği | B-9 → müşteri · IBAN'sızsa B-14 → müşteri · firmaya — (`02 §9.3.2`) | `02 §6.4.25`, §10.4.10 · K-688, K-709, K-787 |
| 3.3.26 | §2.10.4 | Havale hattında IBAN yeniden istenmiş ama müşteri hiç girmez | Kalem kapanmaz ve borç sürer; "IBAN bekleniyor" listesinde görünür ama bekleyen işler sayacında sayılmaz; yönetici siparişin iletişim bilgisini görür. Süre işler | — | — | `02 §6.4.26`, §7.4.5 |
| 3.3.27 | §2.7.5 · §2.8.4.2 | Müşteri IBAN'ı yanlış girer ya da banka geri ödeme havalesini reddeder | Geri ödeme işlenmeden önce müşteri IBAN'ı sipariş sayfasından düzeltir. Havale bankada gerçekleşmezse yönetici geri ödemeyi işlemez, "havale gerçekleşmedi" der: IBAN silinir, alan yeniden açılır, süre durmaz. İşlenmiş geri ödemeden sonra dönen havalede ödeme ekseni düzeltilmez; yönetici müşteriye ulaşır ve parayı yeniden gönderir | Havale gerçekleşmezse kalem: IBAN isteği | B-14 → müşteri | `02 §6.4.27`, §7.4.5 |
| 3.3.28 | §2.8.2.2 | Müşteri kısmen ifa edilmiş bir hizmet kaleminden cayar | Hak "tamamlandı" işaretine kadar sürer; beyan cayılan adetleri kapatır ve onların ödenmiş bedelinin tamamı geri ödenir — yapılmış kısım için kesinti yoktur | kalem: cayma beyanı | B-9 → müşteri · F-3 → firma | `02 §6.4.28`, §7.3.3 · K-792 |
| 3.3.29 | §2.8.2.1 · §2.10.2 | Fiziksel ve hizmet kalemli siparişte fiziksel kalemlerin tamamı teslimden önce iptal edilir, çıkarılır ya da feshedilir | Teslim tarihi oluşmaz; hizmetin penceresi başlamaz ve sipariş tarihine geri çekilmez — hak ve düğme "tamamlandı" işaretine kadar sürer (§4.1.15) | — | — | `02 §6.4.29`, §4.2 Z-14 |
| 3.3.30 | §2.8.1.8 | Müşteri malı süresinde bir taşıyıcıya verdiğini ve gönderinin yolda kaybolduğunu bildirir | Ürün taşıyıcı belgesini almaz; uyuşmazlık ürünün dışında çözülür. Akış §5.2.2 | — | Firma ödemeye karar verirse B-8 | `02 §6.4.30`, §10.4.9 · K-686 |
| 3.3.31 | §2.8.4.4 · §8.3.1.1 | Müşteri, sipariş kargoya verilmeden bir fiziksel kalemden caydığını başka kanaldan bildirir | Cayma kaydı açılmaz; yönetici bildirimi teyit eder (§6.2.1) ve kalemi "müşteriyle anlaşıldı (müşteri talebi)" sebebiyle iptal eder — ödeme onayından önce sipariş bütünüyle. Yasal on dört gün (Z-16) bildirimin ulaştığı tarihten işler; yönetici iptali o gün yapar | kalem: iptal kaydı · ödenmemişse → İptal edildi + Başarısız (S3, Ö2) · havale hattında ödenmişse IBAN isteği | B-7 → müşteri · havale hattında ödenmişse B-14 (K-690) | `02 §6.4.31`, §7.3.7 · K-690 |
| 3.3.32 | §2.8.1.1 · §8.4.8 | Müşteri pencere kapandıktan sonra ya da mutlak istisna kaleminden, bilgilendirilmediğini söyleyerek caymak ister | Düğme açılmaz; değerlendirme firmanındır — süresinde gönderilmiş ve geç ulaşmış bildirim de bu yoldan kaydedilir (K-709). Akış §5.2.3 | Kayıtta kalem: cayma beyanı | Kayıtta B-9 → müşteri | `02 §6.4.32`, §7.3.1 · K-709 |
| 3.3.33 | §2.8.4.4 | Siparişin sahibi olmayan biri, siparişin bilgilerini bilerek başka kanaldan cayma bildirimi gönderir — havale hattında kendi IBAN'ıyla | Yönetici kaydetmeden önce siparişin kanalına dönerek teyit eder; sahibi bildirimi yapmadığını söylerse kayıt yapılmaz. Bildirimdeki IBAN aktarılmaz. Akış §6.2.3 | Teyitte — · teyit atlanırsa kalem: cayma beyanı | Teyit atlanırsa B-9 → siparişin iletişim e-postası | `02 §6.4.33`, §8.3.8 · K-688 |
| 3.3.34 | §2.8.1.3 | Sipariş sabah kargodayken müşteri cayar; kargo malı aynı gün teslim eder ve teslim o tarihle işaretlenir | Panel işareti tamamlamadan önce teslimin beyandan önce mi sonra mı olduğunu sorar ve beyanın saatini gösterir; cevap geri ödeme süresinin başlangıcını belirler ve işlem izinde durur | → Teslim edildi (S6) | — (`02 §9.2`) | `02 §6.4.34`, §7.4.1 |
| 3.3.35 | §2.8.1.6 | Koruyucu ambalaj koşuluyla cayılabilen kalemin malı ambalajı açılmış olarak döner | Yönetici teslim alma adımında iade reddini seçer; ret geri alınmaz. Akış §5.3.1 | kalem: iade reddi | B-16 → müşteri | `02 §6.4.35`, §7.4.9 · K-656, K-657 |
| 3.3.36 | §2.8.1.7 | Cayılan mal kullanılmış, hasar görmüş ya da eksik döner ve kalem reddedilebilecek bir kalem değildir | Ret ve kesinti yoktur; kalemin ödenmiş bedeli tam geri ödenir. Akış §5.3.2 | kalem: iade teslim alma | — (`02 §9.2`) · geri ödemede B-8 | `02 §6.4.36`, §7.4.10 · K-668 |

### 3.4 Üyelik (akış 4)

| # | Akış adımı | Ne ters gider | Sistemin cevabı | Durum | Bildirim | Kaynak |
|---|---|---|---|---|---|---|
| 3.4.1 | §9.1.5 | Doğrulama bağlantısının süresi (Z-1) dolmuştur | Doğrulanmamış kayıt silinir; bağlantıya tıklayan "bağlantı geçersiz, yeniden kayıt olun" mesajını görür; e-posta yeniden kayda açıktır | Hesap: doğrulanmamış kayıt silinir | — (K-683) | `02 §6.5.1`, §3.13.5 · K-683 |
| 3.4.2 | §9.2.1 | Kullanıcı e-postasını doğrulamadan giriş yapmaya çalışır | Giriş yapılamaz; ekran doğrulamanın beklendiğini söyler ve bağlantıyı yeniden istemenin yolunu gösterir (9.1.3, L-9); Z-1 dolmuşsa kayıt silinmiştir ve kullanıcı yeniden kaydolur (3.4.1). Mesajın hesabın varlığını ele vermeme sınırı `02 §8.1.4`'tedir | — | — | `02 §6.5.2`, §3.13.4, §8.1.4 |
| 3.4.3 | §9.1.2 | Kayıtta girilen e-posta zaten kayıtlıdır | "Bu e-posta zaten kayıtlı" — ekranın hesabın varlığını bilerek ele verdiği yerdir; L-3 sorgulamayı daraltır | — | — | `02 §6.5.3`, §8.1.4 |
| 3.4.4 | §9.2.4 | Şifre sıfırlamada girilen e-posta kayıtlı değildir | Nötr mesaj: "bu adres kayıtlıysa sıfırlama bağlantısını gönderdik" | — | — | `02 §6.5.4`, §3.13.11 |
| 3.4.5 | §9.2.5 | Şifre sıfırlama bağlantısı kullanılmış ya da süresi (Z-2) dolmuştur | Bağlantı geçersizdir; kullanıcı yeni talep açar — L-2 içinde | — | — | `02 §6.5.5`, §3.13.9 |
| 3.4.6 | §9.1.2 · §9.3.3 | Kullanıcı asgari uzunluğun altında ya da çok yaygın bir şifre seçer | Şifre reddedilir | — | — | `02 §6.5.6`, §3.13.15 |
| 3.4.7 | §9.1.7 | Kimlik sağlayıcısı e-postayı doğrulanmamış verir | Mevcut bir hesapla bağlama yapılmaz ve yeni hesap açılmaz; kullanıcı e-posta ve şifreyle kayda yönlendirilir | — | — | `02 §6.5.7`, §3.13.7 · K-697 |
| 3.4.8 | §9.2.4 | Google ile açılmış, şifresi olmayan hesap "şifremi unuttum" der | Sıfırlama akışı "şifre belirle" işlevi görür | — | Şifre sıfırlama bağlantısı (`02 §9.4`) | `02 §6.5.8`, §3.13.8 |
| 3.4.9 | §9.3.4 | Kullanıcı yeni e-posta adresini doğrulamaz | Değişiklik geçerli olmaz; hesap eski adresle çalışır; bekleyen değişiklik ilk bağlantının ömrüyle düşer | — | — | `02 §6.5.9`, §3.13.14 · K-659 |
| 3.4.10 | §9.4.1 · §2.6.6 | Yürüyen siparişi olan kullanıcı hesabını siler | Engellenmez; sipariş kendi kaydıyla ve misafir yolundan sürer (§9.4.3) | Hesap silinir; siparişin durumları değişmez | Olağan (§9.4.2) — hesabın silindiği bildirimi → hesabın adresi (K-693) | `02 §6.5.10`, §3.15.2 · K-693 |
| 3.4.11 | §9.1.2 · §9.2.1, §9.2.4 | Giriş, şifre sıfırlama ya da hesap kaydı denemeleri eşiği aşar (L-1, L-2, L-3) | O işlem o eksende geçici olarak engellenir ve kendiliğinden açılır; mesaj nötrdür, hesap kilitlenmez. Girişin e-posta ekseni tanınan tarayıcıyı engellemez. Akış §6.1.3 | — | — | `02 §6.5.11`, §8.2.6 |
| 3.4.12 | §9.1.1, §9.1.6 | Aydınlatma metni tamamlanmamışken ya da çerez politikası yayına alınmamışken ziyaretçi hesap açmak ya da ilk kez Google ile girmek ister | Kayıt ekranı ve yeni hesap açacak Google ile giriş kapalıdır; mevcut hesapların girişi etkilenmez | — | — | `02 §6.5.12`, §3.13.19 · K-850 |
| 3.4.13 | §9.3.5 | Hesabın e-posta adresi sahibinin haberi olmadan değiştirilir — açık kalmış ya da ele geçirilmiş bir oturumdan, şifre bilinerek | Eski adrese giden bildirim değişikliği söyler; sahibi "bu değişikliği ben yapmadım" bağlantısıyla Z-45 içinde geri alır. Akış §6.2.5 | — | Eski adrese değişiklik bildirimi (`02 §9.4`) | `02 §6.5.13`, §3.13.14 |
| 3.4.14 | §9.1.3 | Biri bir adrese art arda doğrulama bağlantısı göndertir ya da kullanıcı bağlantıyı defalarca ister | L-9 aşılınca istek geçici olarak engellenir; mesaj nötrdür. Her yeni bağlantı öncekileri geçersiz kılar ve kaydın ömrünü uzatmaz | — | — | `02 §6.5.14`, §3.13.5 · K-658, K-659 |
| 3.4.15 | §9.4.1 · §9.4.6 | E-posta değişikliğini geri alma bağlantısı açıkken hesap silinmek istenir — üyenin kendisi ya da yönetici talep üzerine | Silme yapılmaz; hesap ekranı ve üye kaydı görünümü sebebini ve bağlantının bitiş tarihini söyler. Z-45 dolunca ya da bağlantı kullanılınca silme açılır | — | — | `02 §6.5.15`, §3.15.6 · K-698 |
| 3.4.16 | §9.1.6 | Google'ın doğrulanmış verdiği e-posta doğrulanmamış bekleyen bir kayda eşleşir — kaydı adresin sahibi açmamış olabilir | Kayıt devralınmaz: bekleyen kayıt adı ve şifresiyle silinir ve hesap Google ile şifresiz doğar. Veri toplayan girişlerin kapısı kapalıysa giriş yapılmaz; bekleyen kayıt Z-1'in sonuna kadar durur | Hesap: bekleyen kayıt silinir · hesap doğrulanmış doğar | — (`02 §9.4`; K-694) | `02 §6.5.16`, §3.13.7 · K-694, K-696 |
| 3.4.17 | §9.2.6 | Oturumu açık biri yeniden doğrulama ekranında şifreyi art arda yanlış girer | Denemeler L-1'e sayılır; eşik aşılınca o eksende şifreyle yeniden doğrulama ve giriş geçici olarak engellenir ve engel süre dolunca kendiliğinden kalkar; işlem yapılmaz (§6.1) | — | — | `02 §6.5.17`, §3.13.13, §8.2 L-1 · K-711 |

### 3.5 Firma tarafı

Firma tarafının hataları panelin kurallarıdır: çoğu, panelin değeri sebebiyle reddettiği ya da işlemi durdurduğu satırlardır. Siparişe dokunan hataların müşteri yüzü §2'nin satırıdır.

#### 3.5.1 Katalog yönetimi (akış 5)

| # | Akış adımı | Ne ters gider | Sistemin cevabı | Durum | Bildirim | Kaynak |
|---|---|---|---|---|---|---|
| 3.5.1.1 | §8.1.11 | İndirimli fiyat referans fiyatın altına inmez | İndirim kaydedilmez; panel yöneticiyi sebebiyle uyarır | — | — | `02 §6.6.1`, §3.9.5 |
| 3.5.1.2 | §8.1.14 | İçinde ürün ya da alt kategori bulunan kategori silinmek istenir | Silme engellenir; engelleyenler sayısıyla gösterilir | — | — | `02 §6.6.2`, §3.4.5 |
| 3.5.1.3 | §8.1.14 | Taşıma üç seviyeyi aşan bir dal üretir ya da kategori kendi alt ağacına taşınır | Taşıma yapılmaz | — | — | `02 §6.6.3`, §3.4.6 |
| 3.5.1.4 | §8.1.7 | Yayın kapısının bir koşulu eksiktir | Ürün yayına alınamaz; panel eksik koşulu gösterir | — | — | `02 §6.6.4`, §3.7.3 |
| 3.5.1.5 | §8.1.7 | Dijital ürünün yayındaki bir varyantı dosyasızdır | Ürün yayına alınamaz; panel dosyası eksik varyantı gösterir | — | — | `02 §6.6.5`, §3.7.4 |
| 3.5.1.6 | §8.1.6 | Yayındaki dijital ürünün dosyasını silmek bir varyantı dosyasız bırakır | Silme engellenir; yönetici önce yeni dosyayı yükler ya da varyantı yayından çeker | — | — | `02 §6.6.6`, §3.7.4 |
| 3.5.1.7 | §8.1.2, §8.1.10 | Fiyata ikiden fazla ondalık girilir | Panel kabul etmez | — | — | `02 §6.6.7`, §3.8.4 |
| 3.5.1.8 | §8.1.12 | Sabit tutarlı kuponun asgari sepet tutarı kupon tutarına eşit ya da altındadır; yüzdesel kupon %100 girilir | Girilemez | — | — | `02 §6.6.8`, §3.10.4 |
| 3.5.1.9 | §8.1.4 | Ürünün kargoya verme süresi üst çiti aşar | Panel çitin üstündeki değeri kabul etmez | — | — | `02 §6.6.9`, §3.20.5 |
| 3.5.1.10 | §8.1.1 | İki yönetici aynı kaydı aynı anda düzenler | İkinci kaydeden kaydın değiştiğini görür ve üzerine yazmadan önce uyarılır | — | — | `02 §6.6.10`, §3.31.1 |
| 3.5.1.11 | §8.1.8 | Yayındaki ürünün son Yayında varyantı arşivlenmek ya da taslağa çekilmek istenir | İşlem engellenir; panel yöneticiyi önce ürünü taslağa ya da arşive almaya yönlendirir | — | — | `02 §6.6.11`, §3.7.8 |
| 3.5.1.12 | §8.1.2, §8.1.10 | Varyanta 0,00 TL fiyat girilir | Panel kabul etmez | — | — | `02 §6.6.12`, §3.8.4 |
| 3.5.1.13 | §8.1.11 | İndirim %100 girilir ya da yuvarlanmış indirimli fiyat bir varyantta 0,00 TL'ye iner | İndirim kaydedilmez; panel sebebini söyler. İndirim yürürken varyanta böyle bir fiyat da girilemez | — | — | `02 §6.6.13`, §3.9.1 |
| 3.5.1.14 | §8.1.3 · §8.3.1.1 | Yönetici varyantın stoğunu ya da hizmetin kontenjanını ödemesi beklenen siparişlerin ayırdığı adedin altına indirmek ister — ör. mal hasar görmüştür | Panel değeri sebebiyle ve ayrılmış adetle reddeder, ayırmayı tutan siparişleri gösterir. Mal gerçekten yoksa yönetici o siparişi firma iptaliyle kapatır — ödeme onayından önce sipariş bütünüyle —; ayırma serbest kalır ve stok ardından düşürülür | İptalde → İptal edildi + Başarısız (S3, Ö2) | İptalde B-7 → müşteri | `02 §6.6.14`, §3.6.7 · K-653 |
| 3.5.1.15 | §8.1.13 | Yönetici kuponun toplam kullanım adedini kullanılmış ve ayrılmış hakların altına çekmek ister | Panel değeri sebebiyle reddeder. Kampanyayı durdurmanın yolu bitiş tarihini öne çekmektir: yeni siparişte kupon kabul edilmez, ayrılmış haklar onaylanmış siparişlerde kalır | — | — | `02 §6.6.15`, §3.10.6 · K-654 |
| 3.5.1.16 | §8.1.9 | Yönetici ödemesi beklenen ya da ödenip tamamlanmamış siparişi olan bir ürünü ya da varyantı siler | Silme engellenmez; panel onaydan önce açık sipariş sayısını söyler. Sipariş donmuş kalemiyle sürer; silinen varyantın ayırması varyantla düşer; sepetteki kalem çıkar (§2.3.5). Yayındaki ürünün son Yayında varyantı silinemez (3.5.1.11'in kalıbı) | Siparişin durumları değişmez | — (`02 §9.3.2`) | `02 §6.6.16`, §3.7.7 · K-655, K-674 |

#### 3.5.2 Sipariş yürütümü (akış 6)

| # | Akış adımı | Ne ters gider | Sistemin cevabı | Durum | Bildirim | Kaynak |
|---|---|---|---|---|---|---|
| 3.5.2.1 | §2.5.2.4 · §8.2.2 | Havalede gelen tutar sipariş tutarından farklıdır | Sistem tutar sormaz; yönetici "ödendi" işaretler ya da farkı müşteriyle sistemin dışında çözer | İşaretlenirse → Ödendi (Ö1) | İşarette B-4 → müşteri | `02 §6.7.1`, §3.21.7 |
| 3.5.2.2 | §2.5.2.5 · §2.4.9 | Havale, sipariş iptal edildikten sonra gelir — ödeme süresi dolduğu ya da müşteri aynı sepetten yeniden onayladığı için iptal edilmiş siparişin numarasıyla | İptal edilmiş sipariş "ödendi" işaretlenemez; yönetici parayı sistemin dışında geri öder ya da müşteriden yeni sipariş ister. Aynı müşterinin ödemesi beklenen yeni siparişi varsa yönetici onu 3.5.2.1'e göre işaretler — tutar farkını sistemin dışında çözer — ya da bu satırın yolunu izler (`02 §3.21.7`). Paranın gelişi ve iadesi ürünün dışında yürür; sipariş Z-41'i izler | — (İptal edildi + Başarısız kalır) | — (ürün olayı bilmez — `02 §6.2.16`) | `02 §6.7.2`, §3.21.8 |
| 3.5.2.3 | §2.5.3.4 · §8.2.7, §8.2.8 | Kargo gönderiyi ulaştıramayıp firmaya geri döndürür | Yönetici Teslim edilemedi işaretler; buradan yeniden gönderir ya da iptal eder — siparişi bütünüyle ya da geri dönen kalemleri kalem düzeyinde (1.11.21). Cayma beyanlı ya da feshedilmiş kalem yeniden gönderilmez ve iptal edilmez, teslim alma adımıyla işlenir. Sipariş "kargoya verilecek" sayacında görünür | → Teslim edilemedi (S7) → Kargoya verildi (S8), İptal edildi (S9) ya da Teslim edildi (S11) | S7'de B-6 · S8'de B-5 · S9'da B-7, kapanışta gitmez | `02 §6.7.3`, §5.4 |
| 3.5.2.4 | §2.5.3.3 · §8.2.5 | Yönetici kargoya verirken takip numarası girmez | Kargo şirketi ve takip numarası ya da "kendi aracımızla teslim" beyanı olmadan işaret konamaz | — | — | `02 §6.7.4`, §3.20.7 |
| 3.5.2.5 | §8.4.5 | Yönetici bir geçişi yanlışlıkla yapar | Panel "geri al" sunmaz; düzeltme sebep seçerek yapılan yeni bir geçiştir ve sınırları §1.6.2'dedir; dış dünyaya çıkmış sonuç geri alınmaz. Havale "ödendi" işaretinin düzeltilmesinde Z-8 düzeltme anından yeniden işler ve havale hatırlatması yeni sürenin son iş gününde bir kez daha gider (K-685) | §1.6.2'nin satırları | B-13 → müşteri · havale işaretinin düzeltilmesinde ayrıca B-3 (K-685) | `02 §6.7.5`, §10.4.6 · K-685 |
| 3.5.2.6 | §8.3.1.1 | Firma siparişi karşılayamaz — stok hatası, ürünün bulunamaması, teslimatın mümkün olmaması | Yönetici sebep seçerek firma iptali yapar; sebep iptal anında e-postayla bildirilir — kalem iptalinde de — ve geri ödemenin on dört günü (Z-17) iptalden işler. "Stokta bulunamadı" seçilirse panel onaydan önce yasal uyarıyı gösterir | → İptal edildi (S3, S4, S9) ya da kalem: iptal kaydı — teslim edilmiş kalem varsa son kalemin iptaliyle → Teslim edildi (S2, S10, S11) · ödenmişse Ö3–Ö5 · havale hattında ödenmişse IBAN isteği | B-7 → müşteri · kart iadesinde F-4 → firma · havale hattında ödenmişse B-14 (K-690) | `02 §6.7.6`, §7.2.4 · K-690 |
| 3.5.2.7 | §2.5.3.2 · §8.2.10 | Müşterinin dijital kalemdeki indirme hakkı dolar | Müşteri iletişim formundan başvurur; yönetici hakkı panelden yeniler; müşteri dosyayı yine sipariş sayfasından indirir | kalem: indirme sayacı sıfırlanır | Yenileme bildirim üretmez — cevap firmanın e-postasıdır (K-683) | `02 §6.7.7`, §3.12.6 · K-683 |
| 3.5.2.8 | §2.9.4 · §8.1.6 | Satılan dijital dosya ayıplıdır | Yönetici dosyayı düzeltip yeniden yükler; yeni hâl eski alıcılara da sipariş sayfasından iner | — | Dosya güncellemesi bildirim üretmez (K-683) | `02 §6.7.8`, §7.5.4 · K-683 |
| 3.5.2.9 | §2.4.9 · §2.5.1.4 | İptal edilmiş siparişe kart ödemesi gelir | Sistem ödemeyi kendiliğinden geri öder (3.2.1.16) | — | B-8 → müşteri (K-684) · F-4 → firma | `02 §6.7.9`, §6.2.16 · K-684 |
| 3.5.2.10 | §2.5.3.5 · §8.2.6 | Yönetici kargoya verilmiş siparişin teslimini hiç işaretlemez | Kendiliğinden geçiş yoktur; sipariş Kargoya verildi'de kalır ve cayma penceresi başlamaz; cayma düğmesi ve "sorun bildir" açık kalır ama Z-18 işlemez — yükü firma taşır. Sipariş "teslim işareti bekleyen" sayacındadır | — | — | `02 §6.7.10`, §3.20.11 |
| 3.5.2.11 | §8.4.3, §8.4.4 | Yönetici teslim tarihini ya da kargo şirketi ve takip numarasını yanlış girer | Yönetici panelden düzeltir; düzeltme işlem izine yazılır. Düzeltilmiş teslim tarihinden pencereler yeniden işler | — | Takip bilgisinde B-12 → müşteri · teslim tarihinde — (`02 §9.2`) | `02 §6.7.11`, §10.4.4, §10.4.5 |
| 3.5.2.12 | §8.4.1 | Siparişin teslimat ya da fatura adresi yanlıştır | Yönetici müşterinin talebiyle Kargoya verildi'ye kadar düzeltir; talep siparişin e-postasından gelmediyse kanalına dönerek teyit eder (§6.2.1). Sonrasında düzeltme yapılmaz | — | B-10 → müşteri | `02 §6.7.12`, §10.4.2 |
| 3.5.2.13 | §2.6.5 · §8.9.2 | Bir e-posta üç yeniden denemeden sonra da müşteriye ulaşmaz | Siparişin panel satırına "e-posta ulaşmadı" işareti düşer; akış etkilenmez. Yönetici e-postayı yeniden gönderir; başarılı gönderimle işaret kalkar | — | — · panel işareti (`02 §9.1.6`) | `02 §6.7.13`, §6.1.1 |
| 3.5.2.14 | §2.5.3.6 · §8.2.9 | Yönetici ödenmemiş siparişte hizmet kalemini "tamamlandı" işaretlemek ister | Panel işareti sunmaz; işaret ödeme onaylandıktan sonra — ödeme Bekliyor ya da Başarısız değilken — konur | — | — | `02 §6.7.14`, §3.20.9 · K-671 |
| 3.5.2.15 | §8.2.2, §8.2.5, §8.3.1.1, §8.3.3.2, §8.4.2 | İki yönetici aynı siparişte aynı anda işlem yapar ya da yöneticinin işlemi müşterinin iptaliyle, IBAN düzeltmesiyle veya kendiliğinden bir geçişle çakışır | Önce tamamlanan işlem uygulanır; sonraki, sipariş arada değiştiyse uygulanmaz ve panel güncel hâli gösterir — müşterinin IBAN düzeltmesi de bir değişikliktir: panel IBAN'ı gösterdikten sonra alan değiştiyse geri ödemenin işlenmesi uygulanmaz ve güncel IBAN gösterilir; para eski IBAN'a gitmişse sonucu ürünün dışında çözülür. Aynı geçiş ya da geri ödeme kaydı iki kez işlenmez | Önce tamamlanan işleminki | Önce tamamlanan işleminki | `02 §6.7.15`, §3.31.2 |
| 3.5.2.16 | §2.7.7 · §2.7.8 · §8.3.2.1, §8.3.2.5 | Müşteri gecikme feshi yapar; sipariş kargoya verilmemiştir ya da gönderi sonradan firmaya döner | Feshedilen kalem kargoya verilmez, yeniden gönderilmez, firma iptaline konu olmaz ve ikinci bir geri ödeme açılmaz. Kargoya verilmemiş kalemin stoğu kendiliğinden döner; dönen mal teslim alma adımıyla işlenir. Açık kalem kalmazsa sipariş Hazırlanıyor'da fesihle aynı anda kapanır; dönen gönderide yönetici S9'un kapanışını yapar | → İptal edildi (S4 kapanışı) · → Teslim edilemedi (S7) → İptal edildi (S9 kapanışı) · teslim edilmiş kalem varsa → Teslim edildi (S10, S11) | Fesihte B-9 → müşteri, F-3 → firma · S7'de B-6 · kapanışta B-7 gitmez | `02 §6.7.16`, §5.8 |
| 3.5.2.17 | §2.5.2.5 | Site kesintideyken havale siparişinin ödeme süresi dolar; müşteri parayı göndermiştir ama yönetici "ödendi" işaretini koyamamıştır | Sipariş dönüşte hemen iptal edilmez: iptal sitenin dönüşünden sonraki ilk iş gününün sonuna ertelenir, ayrılanlar tutulur ve yönetici o ana kadar "ödendi" işaretler; sipariş sayfası yeni son günü gösterir (§4.2.12) | Alındı + Bekliyor · işaretlenirse → Ödendi (Ö1) · işaretlenmezse ertelenen anda → İptal edildi + Başarısız (S3, Ö2) | Ertelemede — (`02 §4.2` Z-8) · Ö1'de B-4 · iptalde B-7 | `02 §6.7.17`, §4.3 · K-666, K-667 |

#### 3.5.3 Kurumsal içerik (akış 7)

| # | Akış adımı | Ne ters gider | Sistemin cevabı | Durum | Bildirim | Kaynak |
|---|---|---|---|---|---|---|
| 3.5.3.1 | §8.6.1.4 | Zorunlu alanı eksik kayıt yayına alınmak istenir | Yayına alınamaz; panel eksik alanı gösterir | — | — | `02 §6.8.1`, §3.27.24 |
| 3.5.3.2 | §8.6.1.7 | Yönetici Hakkımızda'yı silmek ister | Hakkımızda silinmez, yalnız taslağa alınır | — | — | `02 §6.8.2`, §3.27.3 |
| 3.5.3.3 | §2.2.1 | Hakkımızda boştur ya da yayında değildir | Ana sayfanın kurumsal bloğu marka adını ve logoyu gösterir; boş kutu görünmez | — | — | `02 §6.8.3`, §3.27.14 |
| 3.5.3.4 | §2.2.4 · §8.1.8, §8.1.9 | İçeriğe bağlı ürün taslağa ya da arşive alınır veya silinir | Taslakta ve arşivde ürün bağda kalır ama görünmez; silinen ürün bağdan kalkar | — | — | `02 §6.8.4`, §3.27.10 |
| 3.5.3.5 | §2.2.10 | Ziyaretçi silinmiş ya da taslaktaki bir içeriğin adresine gelir | "Sayfa bulunamadı" | — | — | `02 §6.8.5`, §3.30.4 |
| 3.5.3.6 | §8.6.1.6 | İki yönetici aynı kaydı aynı anda düzenler | İkinci kaydeden uyarılır | — | — | `02 §6.8.6`, §3.31.1 |
| 3.5.3.7 | §2.2.8 | Bir bot iletişim formunu doldurur ve görünmez tuzak alanı da doldurur | Gönderim hata vermeden sessizce düşer; talep kaydı oluşmaz | — | — | `02 §6.8.7`, §3.32.7 |
| 3.5.3.8 | §2.2.8 | Aynı IP'den çok sayıda form gönderilir (L-4) | Eşik aşılınca gönderim geçici olarak engellenir; mesaj nötrdür | — | — | `02 §6.8.8`, §8.2 |
| 3.5.3.9 | §2.2.7 | Aydınlatma metni tamamlanmamışken ya da çerez politikası yayına alınmamışken ziyaretçi formu açar | Form çalışmaz; kurumsal sayfalar yayında kalır | — | — | `02 §6.8.9`, §3.32.8 · K-850 |
| 3.5.3.10 | §2.2.9 · §8.6.4.2 | Müşteri firmanın e-postayla verdiği cevabı yanıtlar | Yanıt firma kimliğindeki iletişim e-postasına düşer; sisteme girmez ve yeni talep açmaz | — | — | `02 §6.8.10`, §9.1.4 |
| 3.5.3.11 | §8.6.1.6 | Yayındaki bir kaydın zorunlu alanı düzenlemede boşaltılır | Düzenleme kaydedilmez; panel eksik alanı gösterir ve yöneticiyi önce kaydı taslağa almaya yönlendirir | — | — | `02 §6.8.11`, §5.2 |

#### 3.5.4 Mağaza ayarları (akış 8)

| # | Akış adımı | Ne ters gider | Sistemin cevabı | Durum | Bildirim | Kaynak |
|---|---|---|---|---|---|---|
| 3.5.4.1 | §8.7.2.1 · §8.7.1.2 | Firma tipinin zorunlu kimlik alanları eksiktir ya da firma tipi değişmiş ve yeni setin alanları boştur | Satış açılmaz; kurumsal taraf yayında kalır; açık siparişler etkilenmez | — | — | `02 §6.9.1`, §3.1.5 · K-664 |
| 3.5.4.2 | §8.7.3.1 | Havale açılmak istenir ama IBAN boştur | Havale açılamaz | — | — | `02 §6.9.2`, §3.21.5 |
| 3.5.4.3 | §8.7.3.1 · §2.4.5 | Ödeme sağlayıcı anahtarları kurulumda tanımlı değildir | Kart yöntemi görünmez; havale açıksa satış yalnız havaleyle açıktır | — | — | `02 §6.9.3`, §3.21.4 |
| 3.5.4.4 | §8.7.3.3 · §8.7.1.2 | Hiçbir ödeme yöntemi açık değildir | Satış açılmaz | — | — | `02 §6.9.4`, §3.1.5 |
| 3.5.4.5 | §8.7.4.3 | Havale ödeme süresi çitlerin dışındadır ya da varsayılan kargoya verme süresi üst çiti aşar | Panel değeri kabul etmez ve sebebini söyler | — | — | `02 §6.9.5`, §4.1.5 |
| 3.5.4.6 | §2.1.1 · §8.6.2.4 | Marka adı girilmemiştir | Site alan adını gösterir | — | — | `02 §6.9.6`, §3.29.3 |
| 3.5.4.7 | §8.6.2.4 | Logo yüklenmemiştir | Marka adı yazıyla gösterilir | — | — | `02 §6.9.7`, §3.29.2 |
| 3.5.4.8 | §8.6.2.4 | Site simgesi yüklenmemiştir | Marka adının baş harfi marka rengi zemin üzerinde gösterilir | — | — | `02 §6.9.8`, §3.29.4 |
| 3.5.4.9 | §8.6.2.4 | Marka rengi seçilmemiştir ya da üstündeki yazıyı okunmaz kılacak bir renk seçilmiştir | Ürünün varsayılan rengi kullanılır; üstündeki yazının rengini sistem kontrasta göre seçer | — | — | `02 §6.9.9`, §3.29.5 |
| 3.5.4.10 | §8.7.2.2 · §8.7.1.2 · §2.2.7 · §9.1.1, §9.1.6 | Aydınlatma metni tamamlanmamış ya da yayına alınmamıştır, çerez politikası yayına alınmamıştır ya da iade adresi girilmemiştir | Satış açılmaz ve panel eksik olanı adıyla gösterir; aydınlatma metni tamamlanmamışken ya da çerez politikası yayına alınmamışken iletişim formu, hesap kaydı ve Google ile ilk giriş de kapalıdır | — | — | `02 §6.9.10`, §3.1.5 · K-850 |
| 3.5.4.11 | §8.8.5 | Son yönetici kaldırılmak istenir ya da yönetici kendi hesabını kaldırmak ister | İşlem engellenir ve panel sebebini söyler | — | — | `02 §6.9.11`, §10.2.2 |
| 3.5.4.12 | §8.8.2 | Yönetici davetinin bağlantısı kullanılmış, süresi (Z-3) dolmuş, geri çekilmiş, aynı adrese giden yeni bir davetle ya da daveti gönderenin kaldırılmasıyla geçersizleşmiştir | Bağlantı geçersizdir ve hesap açılmaz; tıklayan süresi dolmuş davetin mesajını görür. Mevcut bir yönetici yeni davet gönderir | — | — | `02 §6.9.12`, §10.2.6 · K-660, K-661, K-662 |
| 3.5.4.13 | §8.7.2.3 | Firmanın SSS ya da sayfa metni sonradan değişen bir ayarla çelişir | Ürün çelişkiyi tespit etmez; bağlayıcı olan ayarlardan üretilen form ve sözleşmedir. Panelde ilgili ayarın yanında hatırlatma satırı durur | — | — | `02 §6.9.13`, §3.33.8 |
| 3.5.4.14 | §8.7.3.2 · §2.5.2.1 | Ödemesi beklenen havale siparişleri varken havale IBAN'ı değişir — hesap kapanmıştır ya da değişikliği yetkisiz biri yapmıştır | Yeni IBAN yalnız yeni siparişlere işler; değişikliğin onayı bekleyen siparişlerin sayısını söyler ve onlar donmuş IBAN'ı göstermeye devam eder. Eski hesap kullanılamıyorsa ya da değişiklik yetkisizse yönetici bu siparişleri firma iptaliyle ("diğer", açıklamayla) kapatır. Yetkisiz değişiklik akışı §6.2.6 | İptalde → İptal edildi + Başarısız (S3, Ö2) | F-5 → bütün yöneticiler · iptalde B-7 → müşteri | `02 §6.9.14`, §3.21.5 · K-663 |
| 3.5.4.15 | §8.7.3.3 · §8.7.1.2 | Ödemesi beklenen havale siparişleri varken havale kapatılır ya da satış kapısının başka bir koşulu düşer | Açık siparişler etkilenmez: bekleyen havale siparişi donmuş IBAN'ıyla sürer, hatırlatma gider, para gelirse yönetici "ödendi" işaretler, süre dolarsa sipariş kendiliğinden iptal olur. Panel kapatma anında sipariş sayısını söyler; o siparişleri kapatmak isteyen yönetici firma iptalini kullanır | — | — (`02 §9.3.2`) | `02 §6.9.15`, §3.1.6 · K-664 |
| 3.5.4.16 | §8.7.4.5 · §2.8.1.2 · §2.9.2 | İade malı beklenen cayma ya da ayıp kalemleri varken iade adresi değişir | Yapılmış beyanın adresi değişmez — sipariş sayfası beyandaki adresi gösterir; yeni beyanlar güncel adresi alır. Eski adrese gönderilen malı teslim almak firmanın yükümlülüğüdür ve ulaşma tarihi malın o adrese ulaştığı tarihtir; panel değişiklik anında bekleyen kalem sayısını söyler | — | — (`02 §9.3.2`) | `02 §6.9.16`, §3.1.7 · K-665 |

## 4. Zaman aşımı yönetimi

> **Ne yazılır:** Tüm timeout akışları **tek bölümde** — aktör akışlarına gömülmez. Uyarı, dondurma ve normale dönüş adımları dahil.

Ürünün bütün süreleri (`02 §4.2`, Z-1…Z-47) ve kendiliğinden işleyen anları (`02 §4.3`) burada akışa bağlanır; §2, §3, §8 ve §9 süreye kimliğiyle işaret eder, sonucunu tekrarlamaz. Biçim §0.4.3'tür: değer yazılmaz, kimlik yazılır (§0.5.3). Sayım kuralları — tek saat dilimi, iş günü, başlangıç gününün ertesi günü başlayan sayım ve çitler — `02 §4.1`'dedir.

- **Uyarı:** ürünün bir süre dolmadan önce gönderdiği tek uyarı havale hatırlatmasıdır (B-3, Z-9). "Uyarı" sütununda "yok" yazan süre dolarken kimseye önceden haber gitmez; panelin kalan süre göstergesi ve sipariş sayfasının son ödeme günü bir göstergedir, uyarı değildir.
- **Dondurma yoktur:** süreler sitenin kesintisinde de işler; tek istisna havalenin kendiliğinden iptalidir (4.2.11, 4.2.12; K-666, K-667). **Yasal süreler hiçbir durumda dondurulmaz** ve firmanın ayarıyla uzayıp kısalmaz.
- **Kendiliğinden işlem:** "Sona erişte" sütunu sistemin süre dolunca kendiliğinden ne yaptığını yazar. "Kendiliğinden işlem yok" yazan süre firmanın ya da müşterinin bir yükümlülüğüdür; sistem onu izler ve gösterir, yerine getirmez.

### 4.1 Süre envanterinden zaman aşımı akışları

| # | Süre | Başlangıç | Uyarı | Sona erişte | Normale dönüş | Akış |
|---|---|---|---|---|---|---|
| | **Hesap ve oturum** | | | | | |
| 4.1.1 | Z-1 E-posta doğrulama bağlantısının ömrü | İlk bağlantının gönderimi — yeniden istenen bağlantı öncekileri geçersiz kılar ve ömrü uzatmaz | Yok | Bağlantı geçersizleşir; doğrulanmamış kayıt silinir; hesabın yeni e-posta adresinde bekleyen değişiklik düşer | E-posta yeniden kayda açılır — kullanıcı yeniden kayıt olur ya da değişikliği yeniden başlatır | §9.1.3, §9.1.5, §9.3.4 · §3.4.1, §3.4.9 · K-659 |
| 4.1.2 | Z-2 Şifre sıfırlama bağlantısının ömrü | Talep anı | Yok | Bağlantı geçersizleşir; bağlantı ayrıca tek kullanımlıktır | Kullanıcı yeni talep açar — L-2 içinde | §9.2.4, §9.2.5 · §3.4.5 |
| 4.1.3 | Z-3 Yönetici daveti bağlantısının ömrü | Davetin gönderimi | Yok | Bağlantı geçersizleşir ve hesap açılmaz. Süre dolmadan da geçersizleşir: geri çekilince, aynı adrese yeni davet gidince, gönderen yönetici kaldırılınca | Mevcut bir yönetici yeni davet gönderir | §8.8 · §1.10.7 · §3.5.4.12 · K-660, K-661, K-662 |
| 4.1.4 | Z-4 Kısa oturum | Son işlem | Yok | Oturum kapanır | Kullanıcı yeniden giriş yapar; üyenin sepeti hesaptadır (Z-6) | §9.2.1, §9.2.3 · §1.10.6 |
| 4.1.5 | Z-5 Uzun oturum — "beni hatırla" | Son işlem | Yok | Oturum kapanır | Kullanıcı yeniden giriş yapar | §9.2.1, §9.2.3 · §1.10.6 |
| | **Sepet ve ödeme** | | | | | |
| 4.1.6 | Z-6 Sepetin ömrü | — | — | Dolmaz: sepet kendiliğinden boşalmaz | — | §2.3 |
| 4.1.7 | Z-7 Kart ödeme süresi — ayırma bu süre boyunca sürer | Sipariş onayı | Yok | İptalden önce sağlayıcıya son sorgu: ödeme alınmışsa Ö1 işler; alınmamışsa sipariş kendiliğinden iptal edilir ve ayrılanlar serbest kalır; sorgu yanıtsızsa sipariş iptal edilmez, "ödeme sonucu alınamadı" işaretiyle bekler ve sorgu yinelenir | Sepet korunmuştur; müşteri aynı sepetten yeni sipariş verir. Yanıt gelince olağan geçiş işler ve işaret kalkar | §2.5.1.4 · §3.2.1.2 · §1.7.3.2 |
| 4.1.8 | Z-8 Havale ödeme süresi — ayırma bu süre boyunca sürer | Sipariş onayı; havale işaretinin düzeltilmesinde düzeltme anı | B-3 (4.1.9) | Sipariş kendiliğinden iptal edilir ve ayrılanlar serbest kalır; iptal edilmiş sipariş sonradan "ödendi" yapılamaz. Süre sitenin kesintisinde dolduysa iptal ertelenir (4.2.12) | Gelen parayı firma sistemin dışında geri öder ya da müşteriden yeni sipariş ister (§3.5.2.2) | §2.5.2.5 · §3.5.2.17 · §1.6.2.4 |
| 4.1.9 | Z-9 Havale ödeme hatırlatması | Sipariş onayı; havale işaretinin düzeltilmesinde düzeltme anı (K-685) | — (kendisi uyarıdır) | Havale ödeme süresinin son iş gününün başında B-3 bir kez gider; düzeltmeyle yeniden başlayan sürede bir kez daha gider. Kesintide kaçırılırsa dönüşte, süre hâlâ işliyorsa gider, dolmuşsa gitmez | — | §2.5.2.3 · §3.5.2.5 · 4.2.11 · K-685 |
| | **Teslim** | | | | | |
| 4.1.10 | Z-10 Kargoya verme süresi — firmanın sözü | Ödeme onayı; havale işaretinin düzeltilmesinden sonra yeni ödeme onayı | Yok — sipariş panelde "kargoya verilecek" sayacındadır | Kendiliğinden işlem yok. Müşterinin iptal düğmesi açıktır ve gecikmede fesih bu düğmeyle kullanılır; süre aşıldıktan sonraki iptalde panel firmaya kanuni faiz uyarısını gösterir | Kargoya verilince süre biter; söze uyum satış özetinde ölçülür | §2.7.2 · §3.3.2 · §8.2 |
| 4.1.11 | Z-11 Yasal teslim üst sınırı | Sipariş onayı | Yok | Teslim tarihi girilmemiş fiziksel kalem varken gecikme feshi düğmesi açılır — sipariş kargodayken de; kişiye özel üretim işaretli üründe sözleşmenin taahhüdü olarak | Fesih §2.7.7'nin akışıdır; feshin geri ödemesi fesih bildiriminden on dört gün içinde — 2.7.8 | §2.7.7, §2.7.8 · §3.3.2 |
| 4.1.12 | Z-12 Teslim tarihi | — (yöneticinin girdiği tarih) | — | Süre değildir; tarih girilmezse sipariş Kargoya verildi'de kalır, Z-13 ve Z-18 başlamaz | Yönetici teslimi işaretler | §2.5.3.5 · §3.5.2.10 |
| 4.1.13 | Z-39 İfa süresi — hizmet kalemi | Ödeme onayı | Yok — kalem "tamamlanmayı bekleyen hizmet" sayacındadır | Kendiliğinden işlem yok; müşterinin iptal düğmesi açıktır ve süre aşıldıktan sonraki iptalde panel kanuni faiz uyarısını gösterir | "Tamamlandı" işaretiyle biter | §2.5.3.6 · §2.7.3 |
| | **Cayma, geri ödeme ve ayıp talebi** | | | | | |
| 4.1.14 | Z-13 Cayma penceresi — fiziksel kalem | Kalemin teslim tarihi; düzeltilirse düzeltilmiş tarih | Yok | Cayma düğmesi kapanır; önceki beyan geçerli kalır. Eksik bilgilendirmede yasal uzama §5.2.3'tedir | Yol ayıp talebidir (§2.9) | §2.8.1.1 · §3.3.6, §3.3.12 |
| 4.1.15 | Z-14 Cayma penceresi — hizmet kalemi | Sipariş tarihi; onay anında fiziksel kalem taşıyan siparişte teslim tarihi — girilmemişse ya da fiziksel kalemler teslimden önce kapandıysa başlamaz | Yok | Hak düşer — ya da "tamamlandı" işaretiyle, hangisi önce gelirse | Yol ayıp talebidir | §2.8.2.1 · §3.3.29 |
| 4.1.16 | Z-15 Cayma hakkı — dijital kalem | — | — | Ödeme onayında üçüncü onay kutusuyla düşer | — | §2.8.3.1 |
| 4.1.17 | Z-16 Geri ödeme — cayma | Teslimden önceki caymada ve hizmette beyanın tarih damgası; teslimden sonraki caymada malın ulaşma tarihi — beyandan önce ulaştıysa beyan | Yok — panel kalan süreyi, mal ulaşana kadar "iade malı bekleniyor"u gösterir | Kendiliğinden işlem yok; yükümlülük firmanındır. Süre IBAN beklenirken de işler | — | §2.8.1.5, §2.8.2.2, §2.8.4.1 · §3.3.8 |
| 4.1.18 | Z-42 Müşterinin iade malını gönderme süresi | Cayma beyanı | Yok | Kendiliğinden işlem yok: kalem "iade malı bekleniyor"da kalır; yöneticiye isteğe bağlı "mal dönmedi" kapanışı açılır; havale hattında Z-38 işlemeye başlar | Mal sonradan ulaşırsa teslim alma adımı kalemi işler ya da yeniden açar | §2.8.1.8 · §3.3.16, §3.3.18 |
| 4.1.19 | Z-17 Geri ödeme — iptal edilen ödenmiş sipariş ya da kalem | İptal anı | Yok — havale hattında panel kalan süreyi gösterir | Kendiliğinden işlem yok: kart hattında sistem iadeyi iptal anında başlatmıştır; havale hattında yükümlülük firmanındır | — | §2.7.6 · §3.3.24 |
| 4.1.20 | Z-47 İfanın imkânsızlaştığının bildirimi | Firmanın durumu öğrendiği an | — | Sayaç tutulmaz; firma iptali bildirimi iptal anında sebebiyle gönderir (B-7) | — | §8.3 · §3.5.2.6 |
| 4.1.21 | Z-18 Ayıp talebinin iki yıllık süresi | Kalemin teslim işaretinin tarihi | Yok | Sipariş sayfasındaki kanal kapanır: yeni talep açılmaz, çözülmüş talep yeniden açılmaz | Hak iletişim formundan sürer (§5.1.2) | §2.9.1, §2.9.6 |
| 4.1.22 | Z-19 İndirme hakkı | — | — | Süre yoktur; adet (P-8) dolunca indirme kapanır | Yönetici hakkı yeniler (§3.5.2.7) | §2.5.3.2 |
| | **Katalog, kupon ve içerik** | | | | | |
| 4.1.23 | Z-20 İndirimin tarih aralığı | Girilen başlangıç | Yok | Sistem indirimi kendisi bitirir; onay anında bitmesi özet farkıdır (§3.2.1.3) | — | §8.1 · §1.10.1 |
| 4.1.24 | Z-21 Referans fiyatın geriye bakış penceresi | İndirimin başladığı an | — | Hesap penceresidir; dolması bir işlem başlatmaz | — | §8.1 |
| 4.1.25 | Z-22 Kuponun tarih aralığı | Girilen başlangıç | Yok | Kupon yeni siparişte kabul edilmez; ayrılmış haklar onaylanmış siparişlerde kalır | — | §2.4.4 · §8.1 · §3.5.1.15 |
| 4.1.26 | Z-23 Duyurunun tarih aralığı | İsteğe bağlı başlangıç | Yok | Duyuru görünmez; yöneticinin elle kaldırmasına gerek kalmaz | — | §2.1.1 · §8.6 |
| 4.1.27 | Z-24 İleri tarihli yayın | Yoktur — yayın anındadır | — | — | — | §8.1, §8.6 |
| | **İletişim, bildirim ve kötüye kullanım** | | | | | |
| 4.1.28 | Z-25 KVKK başvurusuna cevap | Başvurunun firmaya ulaştığı an — talebin kaydında durur | Yok | Sayaç tutulmaz; yükümlülük firmanındır | — | §8.6 |
| 4.1.29 | Z-26 Gönderilemeyen e-postanın yeniden denenmesi | İlk başarısız gönderim | — | Üç deneme de başarısızsa müşteriye giden bildirimde siparişin, davette davetin satırına "e-posta ulaşmadı" düşer; firmaya giden bildirimde panelin ana sayfasında uyarı çıkar (§8.9.1); hesap ve yönetim e-postalarında işaret düşmez, kullanıcı bağlantıyı yeniden ister | Yönetici e-postayı panelden yeniden gönderir; başarılı gönderimle işaret kalkar | §2.6.5 · §3.1.1, §3.5.2.13 |
| 4.1.30 | Z-27 Deneme limitlerinin penceresi | Kayan pencere | — | O işlemin geçici engeli; hesap kilitlenmez | Pencere içindeki deneme sayısı eşiğin altına inince engel kendiliğinden açılır | §6.1 |
| | **Saklama ve imha** | | | | | |
| 4.1.31 | Z-28 Sipariş, ödeme ve fatura verisi; sözleşme ve onay kayıtları | Siparişin oluştuğu takvim yılının sonu | — | Periyodik imha siparişin kişisel verilerini imha eder ve hesapla bağı keser; kalan alanlar ticari kayıttır | — | 4.2.9 · §3.2.1.16 |
| 4.1.32 | Z-41 Ödemesi hiç alınmamış siparişin kişisel verisi | Siparişin Başarısız olduğu an | — | Kişisel veri imha edilir; kayıt ölçüler için kişisel verisiz kalır | — | 4.2.9 |
| 4.1.33 | Z-29 Ayıp talebi kaydı | — | — | Bağlı olduğu siparişle birlikte imha edilir | — | 4.2.9 |
| 4.1.34 | Z-30 İşlem izi | Satırın yazıldığı takvim yılının sonu | — | Satırlar imha edilir; süre boyunca değiştirilemez ve silinemez | — | §6.3.2 · §8.9 |
| 4.1.35 | Z-31 Hesap verisi | — | — | Hesap silindiğinde hemen silinir | — | §9.4.2 |
| 4.1.36 | Z-32 Sepet | — | — | Hesapla birlikte silinir | — | §9.4.2 |
| 4.1.37 | Z-33 İletişim talebi | Son kapatılış; hiç kapatılmamış talepte açılış | — | Talep silinir — kapatılmamış açık talep de; süre "Sipariş hakkında" tipinde daha uzundur (P-40; K-707) | — | §1.9.2 · §8.6 |
| 4.1.38 | Z-34 Bildirim gönderim kaydı — hesap, yönetim ve firma bildirimleri | Gönderim anı | — | Kayıt silinir | — | 4.2.9 |
| 4.1.39 | Z-43 Bildirim gönderim kaydı — B-1…B-16 | Gönderim anı | — | Kayıt silinir; o güne kadar bilgilendirmenin kanıtıdır (§5.1.7) | — | 4.2.9 |
| 4.1.40 | Z-35 Havale hattında müşterinin geri ödeme IBAN'ı | Beyan ya da IBAN girişi | — | Geri ödeme tamamlanınca silinir ve silme imha kaydına yazılır (Z-44; K-708); malı dönmeyen caymada kapatmayla ya da Z-38'le, iade reddinde retle daha erken | Mal silmeden sonra ulaşırsa IBAN sipariş sayfasından yeniden istenir (§3.3.19) | §2.7.5 · §2.10.4 · §5.3.1 |
| 4.1.41 | Z-36 Periyodik imha | — | — | Süresi dolan veriyi kendiliğinden çalışan bir iş imha eder; panelde düğme yoktur | — | 4.2.9 |
| 4.1.42 | Z-40 Giriş kaydı | Kaydın oluştuğu an | — | Periyodik imhayla silinir | — | §9.2.1, §9.2.2, §9.2.7 · §6.3.2 · K-687 |
| 4.1.43 | Z-44 İmha kaydı | Kaydın yazıldığı an | — | Silinir; kişisel veri içermez | — | 4.2.9 |
| 4.1.44 | Z-45 E-posta değişikliğini geri alma bağlantısının ömrü | Değişikliğin geçerli olduğu an | Yok | Bağlantı geçersizleşir; e-postanın yeniden değiştirilmesinin önündeki engel kalkar ve ürün içinde geri alma yolu kalmaz | — | §9.3.5, §9.3.6 · §6.2.5 |
| 4.1.45 | Z-46 Tanınan tarayıcı işareti | O tarayıcıdan o hesaba son başarılı giriş ya da şifrenin sıfırlama bağlantısıyla yenilenmesi | — | İşaret geçersizleşir; tarayıcının denemeleri yeniden e-posta ekseninde sayılır. Şifre sıfırlama ve şifre değiştirme — işlemi yapan tarayıcınınki dışında —, geri alma bağlantısı ve hesap silme işareti süre dolmadan düşürür | Kullanıcı başarıyla girince işaret yeniden doğar | §9.2.1, §9.2.5, §9.3.3, §9.4.2 · §6.1.3 · K-695 |
| 4.1.46 | Z-37 Veri ihlalinde Kurul'a bildirim | İhlalin öğrenildiği an | — | Sayaç tutulmaz; yükümlülük firmanındır | — | §6.3.2 |
| 4.1.47 | Z-38 Malı dönmeyen caymada havale IBAN'ının kendiliğinden silinmesi | Z-42'nin dolduğu gün | — | Mal hâlâ ulaşmamışsa IBAN silinir; kalem kapanmaz, cayma geçerli kalır; aynı IBAN'ı bekleyen başka bir geri ödeme varsa IBAN onun tamamlanmasına kadar kalır | Mal sonra ulaşırsa IBAN yeniden istenir (§3.3.19) | §2.10.4 · 4.2.8 |

### 4.2 Kendiliğinden işleyen anlar

Süresi olmayan ama zamanı kural olan anlar ve kesintideki davranış (`02 §4.3`). Kuralın tam metni gösterilen bölümlerdedir.

| # | An | Sistem ne yapar | Bildirim | Akış | Kaynak |
|---|---|---|---|---|---|
| 4.2.1 | Sipariş onayı | Numara verir; kalemleri, adresleri, firma kimliğini, metin sürümlerini, havalede IBAN'ı ve kargoya verme sözünü dondurur; stok, kontenjan ve kupon hakkını ayırır; aynı sepete bağlı ödenmemiş önceki siparişi iptal eder | B-1, havalede B-2 → müşteri · F-1 → firma · önceki siparişte B-7 | §2.4.9 | `02 §4.3`, §3.17, §3.23 · K-663 |
| 4.2.2 | Ödeme onayı | Ayrılanı kesin düşer, kupon hakkını kullanılmış sayar, siparişe giren kalemleri sepetten çıkarır, Z-10 ve Z-39'u başlatır, dijital kalemi teslim eder | B-4 → müşteri | §2.5.3.1 | `02 §4.3` |
| 4.2.3 | Kendiliğinden iptal — Z-7 ya da Z-8 dolunca, kart ödemesi başarısız olunca, aynı sepetten yeni sipariş onaylanınca | Sipariş İptal edildi + Başarısız olur ve ayrılanlar aynı anda serbest kalır; kart ödeme süresinde iptalden önce son sorgu yapılır | B-7 → müşteri | §2.5.1.3, §2.5.1.4, §2.5.2.5, §2.4.9 | `02 §3.17.5`, §6.1.2 |
| 4.2.4 | İptal, çıkarma ve gecikme feshi | Kalemin stoğu ve kontenjanı anında döner — feshte kargoya verilmemiş kalemde; kupon hakkı yalnız siparişin tamamı iptal edildiğinde döner; son açık kalemde kapanış kuralı işler (§1.4.3) | Olayın kendi bildirimi (B-7, B-9, B-11) | §2.7 · §1.4.4 | `02 §4.3`, §7.2.6 · K-672 |
| 4.2.5 | İade | Fiziksel kalemin stoğu kendiliğinden dönmez — yönetici teslim alıp ekler; caymada hizmet kontenjanı döner; kupon hakkı yalnız siparişin tamamı iade edildiğinde döner | — | §2.8 · §1.4.4 | `02 §4.3`, §7.4.7, §7.4.8 |
| 4.2.6 | Kısmi iptal ve iade — kalemin bir kısmının (adet) da | Kupon ve kargo ücreti yeniden hesaplanmaz; kargodan önce bütün fiziksel kalemlerin bütün adetleri iptal edildiğinde ya da gönderilenlerin bütün adetlerinden cayıldığında kargo ücreti son işlemin geri ödemesine eklenir | — | §2.7.6, §2.8.4.1 · §3.3.10 | `02 §4.3`, §7.2.9, §7.4.6 · K-789 |
| 4.2.7 | Kart hattında kendiliğinden geri ödeme — iptal edilen ödenmiş kart kalemi ya da iptal edilmiş siparişe gelen kart ödemesi | Sistem iadeyi sağlayıcı üzerinden başlatır; onay penceresi yoktur. Gerçekleşince Ö3–Ö5 işler — geç gelen ödemede eksen değişmez; başarısız olursa "geri ödeme gerçekleşmedi" işareti düşer ve sistem yeniden denemez | B-8 → müşteri · F-4 → firma · başarısızlıkta müşteriye ayrı bildirim yok (K-683) | §2.7.6 · §3.2.1.16, §3.3.24 | `02 §7.2.8`, §6.2.16 · K-683, K-684 |
| 4.2.8 | Malı dönmeyen caymada IBAN'ın silinmesi | "Mal dönmedi" kapanışında kapatmayla, kapatılmamışsa Z-38 dolunca kendiliğinden; silme kalemi kapatmaz | — (`02 §9.2`) | §2.10.4 · 4.1.47 | `02 §4.3`, §7.4.5 |
| 4.2.9 | Periyodik imha (Z-36) | Süresi dolan veriyi imha eder ve her çalışmanın imha kaydını yazar (Z-44); hesabın silinmesi, bekleyen kaydın silinmesi ve IBAN'ın silinmesi de kayda yazılır (K-708) | — (`02 §12.2.7`; 7.2.29) | 4.1.31–4.1.43 | `02 §4.2`, §12.2.7 · K-708 |
| 4.2.10 | E-postanın yeniden denenmesi (Z-26) | Başarısız gönderimi artan aralıkla üç kez yeniden dener; art arda başarısızlıkta panel ana sayfasında kanal uyarısı çıkar | — · panel işareti ve uyarısı (`02 §9.1.6`) | §3.1.1 · §10.1.2 | `02 §6.1.1`, §9.1.6 |
| 4.2.11 | Sitenin kesintisi | Süreler kesintide de işler — bağlantı ömürleri, oturumlar, ödeme ve kargoya verme süreleri, saklama süreleri; ürün kesintiyi sürelere eklemez. Kesintide zamanı gelen kendiliğinden işler — 4.2.3 (havale ödeme süresi kesintide dolduysa iptal ertelenir — 4.2.12), B-3 (4.1.9), 4.2.8, 4.2.9, 4.2.10 — site döndüğünde **kaçırdıkları sırayla** hemen çalışır; kart hattında iptal öncesi son sorgu kesintide gerçekleşmiş ödemeyi bulur. Yasal süreler dondurulmaz; müşterinin başka kanaldan bildirim yolu açıktır (§2.8.4.4) | Dönüşte çalışan işin kendi bildirimi | §10.3.2, §10.3.3 | `02 §4.3`, §3.34.7, §3.21.8 · K-666, K-667 |
| 4.2.12 | Havale ödeme süresinin kesintide dolması | Kendiliğinden iptal, sitenin dönüşünden sonraki ilk iş gününün sonuna ertelenir: ayrılanlar tutulur, para gelmişse yönetici o ana kadar "ödendi" işaretler ve sipariş sayfası yeni son günü gösterir; ödeme işaretlenmezse iptal ertelenen anda işler. Kesintinin tespiti `05`'in işidir | Ertelemede — (`02 §4.2` Z-8) · iptalde B-7 | §2.5.2.5 · §3.5.2.17 · §10.3.4 | `02 §4.2` Z-8, §4.3 · K-667 |

## 5. İtiraz / anlaşmazlık akışları

Ürünün ayrı bir şikâyet ya da itiraz kanalı yoktur (`02 §3.32.9`, §7.1.5). Bu bölüm müşterinin itirazının ürünün içinde nereye düştüğünü (§5.1), ürünün dışında çözülen anlaşmazlıklarda ürünün tuttuğu izi ve ürüne geri dönüş yolunu (§5.2) ve iade reddiyle değer kaybını (§5.3) yazar. **Ürün uyuşmazlığa karar vermez ve yasal sonuç uydurmaz** (GUARDRAILS §8): kararı firma ya da yetkili merci verir, ürün kararın sonucunu panelin var olan işlemleriyle uygular. Biçim §0.4.1'in adım tablosudur; yöneticinin adımları §8'de ayrıntılanır.

### 5.1 Üç yol ve itirazın yeri

| # | Aktör ne yapar | Sistem ne denetler, ne yapar | Durum | Bildirim | Kaynak |
|---|---|---|---|---|---|
| 5.1.1 | Müşteri satın almadan sonra bir sorunu firmaya iletmek ister | Sipariş sayfası üç yolu sunar: iptal (§2.7), cayma (§2.8), ayıp talebi (§2.9). Üçü ayrı düğmedir ve ayrı kayıt taşır; her birinin açık olduğu aralık §1.11'dedir | Yolun kendi durumu | Yolun kendi bildirimi | `02 §7.1.1`, §7.1.4 |
| 5.1.2 | Müşterinin konusu üç yola girmez — gecikmenin sebebi, kargo hasarı, fatura, düğmesi kapanmış bir hakkın tartışması — ya da ayıp talebi süresi dolmuştur | Müşteri iletişim formunu "Sipariş hakkında" tipiyle kullanır (§2.2.8); ayrı şikâyet listesi, canlı destek ve talep takibi yoktur | İletişim talebi: Açık | F-2 → firma · gönderene — (`02 §9.5`) | `02 §3.32.9`, §7.5.1 · K-678 |
| 5.1.3 | Yönetici talebi okur ve müşteriye e-postayla cevap verir (§8.6) | Cevap sistemden yazılmaz. Sonuç bir sipariş işlemi gerektiriyorsa yönetici onu panelin var olan işlemiyle yapar — başka kanaldan cayma kaydı, firma iptali, tutar bazlı geri ödeme, teslim tarihinin düzeltilmesi (§1.11) — ve işlemin kendi bildirimi gider | İletişim talebi: → Kapatıldı | İşlemin bildirimi | `02 §3.32.5`, §10.4 |
| 5.1.4 | Müşteri çözülmüş ayıbın tekrarladığını bildirir | Talep yeniden açılır — yeni talep açılmaz (§2.9.6); Z-18 dolduysa yol iletişim formudur | Ayıp talebi: Çözüldü → Açık | B-9 → müşteri · F-3 → firma | `02 §5.10`, §7.5.1 · K-681 |
| 5.1.5 | Müşteri firmanın cevabını kabul etmez | Ürün uyuşmazlığa karar vermez. Tüketici hakem heyeti ve — dava açılmadan önce arabulucuya başvurma şartıyla — tüketici mahkemesi yolu siparişe donmuş Ön Bilgilendirme Formu'nda ve sözleşmede yazar; ikisi sipariş sayfasında ve B-1'in gövdesindedir. Parasal görev sınırı rakamla yazılmaz | — | — | `02 §3.33.7`, §7.1.5 |
| 5.1.6 | Firma ya da merci müşteri lehine karar verir; yönetici sonucu uygular | Para gerekiyorsa tutar bazlı kısmi geri ödeme ya da kalemin iade hattı işler — havale hattında IBAN müşteriden sipariş sayfasıyla istenir (K-686); cayma gerekiyorsa başka kanaldan kayıt. Ürün kararı ayrıca kaydetmez; yönetici iç not yazar | Para gönderilirse Ö3–Ö5 · havalede kalem: IBAN isteği | B-8 → müşteri · IBAN isteğinde B-14 | `02 §5.5`, §10.4.7 · K-686 |
| 5.1.7 | Yönetici itiraza karşı kanıt hazırlar | Dayanaklar ürünün kayıtlarıdır: siparişe donmuş metinler ve onay kutularının kaydı, B-1…B-16'nın gönderim kaydı (Z-43), işlem izi (Z-30), beyanların tarih damgası ve siparişin e-posta geçmişi. Ürün fotoğraf, belge ve kanıt dosyası almaz; ispat yükü firmadadır | — | — | `02 §3.23.3`, §10.3, §7.3.1 |

### 5.2 Ürünün dışında çözülen anlaşmazlıklar

#### 5.2.1 Ters ibraz (harcama itirazı)

| # | Aktör ne yapar | Sistem ne denetler, ne yapar | Durum | Bildirim | Kaynak |
|---|---|---|---|---|---|
| 5.2.1.1 | Müşteri kart harcamasına bankası üzerinden itiraz eder; sağlayıcı tutarı firmanın hesabından geri alır | Ürün bilmez: sağlayıcıdan ters ibraz bilgisi almaz; ödeme ekseni, satış özeti ve dışa aktarma değişmez | — | — (ürün olayı bilmez) | `02 §3.21.11`, §6.2.22 |
| 5.2.1.2 | Yönetici itirazı sağlayıcının panelinde cevaplar | Ürünün geri ödeme kaydı ve işlem izi firmanın kanıtıdır; yönetici sonucu iç nota yazar (§1.11.27) | — | — | `02 §3.21.11`, §10.4.7 |
| 5.2.1.3 | Yönetici aynı siparişte bir geri ödeme adımı işleyecektir — başarısız kart iadesini yeniden denemek, havale yolunu açmak, havale geri ödemesini işlemek | Önce sağlayıcının panelinde ters ibraza bakar: parası ters ibrazla dönmüş kalem için ikinci geri ödeme yapılmaz. "geri ödeme gerçekleşmedi" işaretindeki uyarı bunu hatırlatır | — | — | `02 §7.2.8`, §3.21.11 |
| 5.2.1.4 | Müşteri kart hattında iptal eder ve sağlayıcı ters ibraz edilmiş işlemde kendiliğinden başlayan iadeyi reddetmez | Para ikinci kez gider — kendiliğinden iade ters ibrazı beklemez. Yönetici fazlayı ürünün dışında, sağlayıcının süreciyle ve müşteriyle çözer. Kalan risktir | Ö3–Ö5 | B-8 → müşteri · F-4 → firma | `02 §3.21.11` |

#### 5.2.2 Yolda kaybolan iade gönderisi

| # | Aktör ne yapar | Sistem ne denetler, ne yapar | Durum | Bildirim | Kaynak |
|---|---|---|---|---|---|
| 5.2.2.1 | Müşteri teslimden sonra cayar ve malı süresinde bir taşıyıcıya verir; gönderi firmaya ulaşmaz | Kalem "iade malı bekleniyor"dadır; geri ödeme süresi mal ulaşmadan başlamaz (§2.8.1.8) | — | — | `02 §6.4.30`, §7.4.1 |
| 5.2.2.2 | Müşteri gönderinin kaybolduğunu iletişim formundan bildirir | Ürün taşıyıcı belgesini almaz ve değerlendirmez — formda dosya eki yoktur; yazışma sistemin dışındadır | İletişim talebi: Açık | F-2 → firma | `02 §10.4.9` |
| 5.2.2.3 | Yönetici Z-42'den sonra kalemi "mal dönmedi" gerekçesiyle kapatır — isteğe bağlıdır | Kapatma geri ödeme borcuna karar vermez; havale hattında IBAN silinir; kalem sayaçtan düşer | kalem: "mal dönmedi" kapanışı | — (`02 §9.2`) | `02 §10.4.9` |
| 5.2.2.4 | Firma — kendi değerlendirmesiyle ya da bir hakem heyeti kararıyla — ödemeye karar verir | Kaleme bağlı tutar bazlı geri ödeme işlenir; kalem kapalı kalır. Havale hattında IBAN silinmişse yönetici IBAN isteği açar ve müşteri IBAN'ı sipariş sayfasından girer (K-686) | Ö3–Ö5 · havalede kalem: IBAN isteği | B-8 → müşteri · IBAN isteğinde B-14 | `02 §5.5`, §10.4.9 · K-686 |
| 5.2.2.5 | Mal sonradan ulaşır | Teslim alma adımı kalemi yeniden açar; 5.2.2.4'te ödenmiş tutar kalemin geri ödenecek tutarından düşülür | kalem: iade teslim alma | — (`02 §9.2`) | `02 §10.4.9` |

#### 5.2.3 Eksik bilgilendirmede cayma süresinin uzaması

| # | Aktör ne yapar | Sistem ne denetler, ne yapar | Durum | Bildirim | Kaynak |
|---|---|---|---|---|---|
| 5.2.3.1 | Müşteri pencere kapandıktan sonra ya da mutlak istisna kaleminden, cayma hakkı konusunda gerektiği gibi bilgilendirilmediğini söyleyerek caymak ister — formdan, e-postayla ya da mektupla | Sipariş sayfasındaki düğme açılmaz; ürün bilgilendirmenin eksik kalıp kalmadığına karar vermez | Formdan geldiyse İletişim talebi: Açık | Formdan geldiyse F-2 → firma | `02 §6.4.32`, §7.3.1 |
| 5.2.3.2 | Yönetici değerlendirir ve bilgilendirmenin eksik kaldığına karar verir; bildirimi başka kanaldan cayma kaydıyla alır (§8.4) | Teyit §6.2.1. Panel tarihin pencerenin dışında olduğunu ya da kalemin istisna taşıdığını söyler ve kaydı ayrıca onaylatır; pencerenin bitiminden bir yıldan (Z-13'ün yasal uzaması) sonraya tarihli kaydı reddeder. Süresinde gönderilmiş ama pencere kapandıktan sonra ulaşmış bildirim de aynı onayla kaydedilir — süreye uygunluk gönderimle ölçülür (K-709). Kayıt geri alınmaz ve düzeltilmez (§1.6.2; K-688) | kalem: cayma beyanı · IBAN'sızsa IBAN isteği | B-9 → müşteri · IBAN'sızsa B-14 | `02 §10.4.10` · K-688, K-709 |
| 5.2.3.3 | Kayıttan sonra | Olağan cayma hattı işler (§2.8) | — | — | `02 §10.4.10` |
| 5.2.3.4 | Yönetici bilgilendirmenin eksik olmadığına karar verir | Kayıt yapılmaz; cevap firmanın e-postasıdır ve uyuşmazlık 5.1.5'in yolundan yürür | — | — | `02 §7.3.1` |

#### 5.2.4 Teslim tarihine itiraz

| # | Aktör ne yapar | Sistem ne denetler, ne yapar | Durum | Bildirim | Kaynak |
|---|---|---|---|---|---|
| 5.2.4.1 | Müşteri, girilen teslim tarihinin gerçek teslimden önce olduğunu ve cayma düğmesinin erken kapandığını söyler | Düğme açılmaz; pencere fiilî teslimden işler ve tarihin ispat yükü firmadadır | — | — | `02 §10.4.5`, §7.3.1 |
| 5.2.4.2 | Müşteri caymayı başka kanaldan bildirir | Yönetici bildirimi teyit eder (§6.2.1) | — | Formdan geldiyse F-2 → firma | `02 §7.3.5` |
| 5.2.4.3 | Yönetici teslim tarihini düzeltir ya da bildirimi kaydeder (§8.4) | Düzeltilmiş tarihten pencereler yeniden işler; tarih bir beyanın günüyle çakışırsa panel teslimin sırasını sorar. Kayıt bir cayma beyanıdır | Kayıtta kalem: cayma beyanı | Tarih düzeltmesinde — (`02 §9.2`) · kayıtta B-9 → müşteri | `02 §10.4.5`, §10.4.10 |
| 5.2.4.4 | Yönetici tarihin doğru olduğunu söyler | Ürün karar vermez; uyuşmazlık 5.1.5'in yolundan yürür ve yanlış tarihin riski firmadadır | — | — | `02 §10.4.5` |

### 5.3 İade reddi ve değer kaybı

#### 5.3.1 İade reddi — koşullu istisna kalemi

| # | Aktör ne yapar | Sistem ne denetler, ne yapar | Durum | Bildirim | Kaynak |
|---|---|---|---|---|---|
| 5.3.1.1 | Müşteri koşullu istisna sebebi kaleme donmuş fiziksel kalemden cayar | Beyan ekranı "koruyucu ambalajı açılmamışsa cayabilirsiniz" koşulunu yazar (§2.8.1.1) | kalem: cayma beyanı | B-9 → müşteri · F-3 → firma | `02 §7.3.4` |
| 5.3.1.2 | Mal koruyucu ambalajı açılmış döner; yönetici teslim alma adımında "iade reddedildi — koruyucu ambalaj açılmış" kaydını seçer (§8.3) | Ret yalnız bu kalemde sunulur. Onay sonucu tek cümleyle söyler ve geri alınmaz (§1.6.1.6); kayıt ulaşma tarihini taşır ve işlem izine yazılır | kalem: iade reddi | B-16 → müşteri — ret sebebi, geri ödeme yapılmayacağı, malın geri gönderileceği ve uyuşmazlık yolları | `02 §7.4.9` · K-656, K-657 |
| 5.3.1.3 | Sistem reddin sonuçlarını işler | Geri ödeme yapılmaz ve süresi işlemez; kalem "iade ve geri ödeme bekleyen" sayacından düşer ve "iade malı bekleniyor" kalkar; havale hattında IBAN silinir; mal stoğa girmez; iptal ve iade oranı değişmez. Sevkiyat ve ödeme ekseni değişmez | — | — | `02 §7.4.9`, §10.6 |
| 5.3.1.4 | Yönetici malı müşterinin teslimat adresine, kendi seçtiği taşıyıcıyla ve kendi bedeliyle geri gönderir | Sistemin dışındadır; ürün izlemez ve takip numarası istemez | — | — (B-16 gitmiştir) | `02 §7.4.9` · K-657 |
| 5.3.1.5 | Müşteri reddi kabul etmez | Yol iletişim formu ve uyuşmazlık yoludur (5.1.2, 5.1.5); B-16 yolları adıyla söylemiştir. Ürün ret için kanıt almaz — ispat firmadadır | — | — | `02 §7.4.9` |
| 5.3.1.6 | Firma kararından döner ya da hakem heyeti aksine karar verir | Kaleme bağlı tutar bazlı geri ödeme işlenir; ret kaydı izde kalır. Havale hattında IBAN retle silinmiştir: yönetici IBAN isteği açar, müşteri IBAN'ı sipariş sayfasından girer (K-686) | Ö3–Ö5 · havalede kalem: IBAN isteği | B-8 → müşteri · IBAN isteğinde B-14 | `02 §7.4.9`, §5.5 · K-686 |

#### 5.3.2 Kullanılmış, hasarlı ya da eksik dönen mal

| # | Aktör ne yapar | Sistem ne denetler, ne yapar | Durum | Bildirim | Kaynak |
|---|---|---|---|---|---|
| 5.3.2.1 | Mal kullanılmış, hasar görmüş ya da eksik döner; kalem reddedilebilecek bir kalem değildir — istisnası yoktur ya da koruyucu ambalajı açılmamıştır | Yönetici teslim alma adımını olağan biçimde işler; ret seçeneği sunulmaz | kalem: iade teslim alma | — (`02 §9.2`) | `02 §7.4.10` · K-668 |
| 5.3.2.2 | Yönetici geri ödemeyi işler (§8.3) | Kalemin ödenmiş bedeli tam ödenir; ürün kesinti alanı açmaz. Stoğa ekleme yöneticinin kontrolüne bağlıdır | Ö3–Ö5 | B-8 → müşteri | `02 §7.4.10`, §7.4.7 |
| 5.3.2.3 | Firma aykırı kullanımdan doğan değer kaybını müşteriden ister | Ürünün dışında yürür — yazışmayla, gerekirse hakem heyeti ya da mahkeme yoluyla; ürün değer kaybını değerlendirmez ve kaydetmez, yönetici iç not yazar. Küçük tutarlı değer kaybı fiilen firmada kalır | — | — | `02 §7.4.10`, §10.4.7 · K-668 |

## 6. Kötüye kullanım ve inceleme akışları

Ürün düzeyindeki önlemler `02 §8`'dedir; teknik güvenlik — genel istek hızı sınırı, dosya güvenliği, depolama — `05`'in işidir. Bu bölüm önlemleri akışa bağlar: limitin hangi adımda sayıldığını ve gerçek kullanıcının çıkışını (§6.1), firmanın kandırılmaya karşı teyidini ve kandırılmadan sonraki yolu (§6.2) ve firmanın inceleme yollarını (§6.3) yazar. **Ürün teyidi denetlemez; teyidin ispatı ve kandırılmanın sonucu firmadadır** (`02 §8.3.6`–§8.3.8). Biçim §0.4.1'in adım tablosudur; §6.1.1 limitlerin satır tablosudur.

### 6.1 Deneme limitleri

#### 6.1.1 Limitler

Eşikler ve pencereler `02 §11.2`'dedir; pencere kayan penceredir (Z-27), L-8 ise aynı anda açık kayıt sayar.

| # | Limit | Sayıldığı adım | Eksen | Aşılınca | Kaynak |
|---|---|---|---|---|---|
| 6.1.1.1 | L-1 Başarısız şifreli giriş ve yeniden doğrulama — müşteri ve yönetici | §9.2.1, §9.2.6, §9.2.7 | IP · e-posta — yalnız o hesap için tanınmayan tarayıcılar · tanınan tarayıcı başına ayrı sayaç | Şifreyle giriş ve şifreyle yeniden doğrulama o eksende engellenir; Google ile giriş ve yeniden doğrulama L-1'e girmez | `02 §8.2`, §8.2.6 · K-711 |
| 6.1.1.2 | L-2 Şifre sıfırlama talebi | §9.2.4, §9.2.8 | IP · e-posta | Talep engellenir | `02 §8.2` |
| 6.1.1.3 | L-3 Hesap kaydı | §9.1.2 | IP · e-posta | Kayıt engellenir | `02 §8.2`, §8.1.4 |
| 6.1.1.4 | L-4 İletişim formu gönderimi | §2.2.8 | IP | Gönderim engellenir; tuzak alanı dolduran gönderim ayrıca sessizce düşer (§3.5.3.7) | `02 §8.2`, §8.3.1 |
| 6.1.1.5 | L-5 Başarısız misafir sipariş takibi sorgusu | §2.6.2 | IP · sorguya yazılan e-posta | Sorgu o eksende engellenir; e-postadaki bağlantı ve üyenin hesabı açık kalır | `02 §8.2.2` |
| 6.1.1.6 | L-6 Geçersiz kupon kodu | §2.4.4 | Sepet — girişte birleşen sepet iki sayacın büyüğünü taşır | O sepetin kupon alanı kapanır | `02 §8.2` |
| 6.1.1.7 | L-7 Kendiliğinden iptal edilen ödenmemiş sipariş | §2.4.8 — sayılan iptaller §2.5.1.3, §2.5.1.4, §2.5.2.5 | IP · e-posta — yalnız giriş yapmış üyenin siparişleri | Yeni sipariş onaylanamaz; ödeme sayfası hiç açılmamış sipariş sayılmaz | `02 §8.2.2`, §8.2.3 |
| 6.1.1.8 | L-8 Aynı anda açık ödenmemiş sipariş | §2.4.5 | IP · e-posta — yalnız giriş yapmış üyenin siparişleri | Havale seçilemez; kart yolu açık kalır | `02 §8.2.3` |
| 6.1.1.9 | L-9 Doğrulama bağlantısının yeniden istenmesi — kayıt ve yeni e-posta adresi; müşteri ve yönetici | §9.1.3, §9.3.4 | IP · e-posta | İstek engellenir; kayıt silinmez, önceki bağlantı ömrü içinde geçerli kalır. Yönetici davetinin yeniden gönderilmesi limite girmez. Bekleyen e-posta değişikliğinin yerine geçen yeni istek de sayılır (§9.3.4; K-734) | `02 §8.2` · K-658, K-734 |

#### 6.1.2 Engel ve normale dönüş

| # | Aktör ne yapar | Sistem ne denetler, ne yapar | Durum | Bildirim | Kaynak |
|---|---|---|---|---|---|
| 6.1.2.1 | Eşiği aşan bir deneme yapılır — ziyaretçi, müşteri ya da yönetici | Yalnız o işlem, o eksende geçici olarak engellenir; mesaj nötrdür (§3.1.5). Hesap kilitlenmez ve panele iş düşmez | — | — | `02 §8.2.1`, §6.1.5 |
| 6.1.2.2 | Zaman geçer ya da — L-8'de — açık siparişlerden biri kapanır | Pencere içindeki deneme sayısı eşiğin altına inince engel kendiliğinden açılır (Z-27); L-8'de açık siparişlerden biri ödenince ya da iptal edilince tavan boşalır | — | — | `02 §8.2.1` |
| 6.1.2.3 | Yönetici limite takılır | Panel girişine muafiyet yoktur; yönetici aynı değerlere tabidir | — | — | `02 §8.2.4` |
| 6.1.2.4 | Ürün düzeyinde limiti olmayan bir işlem tekrarlanır — sepete kalem eklemek, stok yoklamak | Ürün düzeyinde limit yoktur; genel istek hızı sınırı `05`'in işidir | — | — | `02 §8.2.5` |

#### 6.1.3 Girişe yönelik saldırıda gerçek kullanıcı

| # | Aktör ne yapar | Sistem ne denetler, ne yapar | Durum | Bildirim | Kaynak |
|---|---|---|---|---|---|
| 6.1.3.1 | Saldırgan hedefin e-postasıyla — tek IP'den ya da dağıtık — şifre dener | L-1'in e-posta ekseni o hesap için işaretsiz tarayıcılardan gelen denemeleri sayar ve onları engeller; IP ekseni ayrıca sayar | — | — | `02 §8.2.6` |
| 6.1.3.2 | Gerçek kullanıcı daha önce başarıyla girdiği tarayıcıdan girer | Tanınan tarayıcı e-posta ekseninde engellenmez; denemeleri kendi sayacında sayılır ve IP ekseni ona da işler | Oturum açılır | Olağan (§9.2.1) | `02 §8.2.6` · Z-46 |
| 6.1.3.3 | Gerçek kullanıcı yeni ya da çerezi silinmiş bir tarayıcıdan girmek ister | Tarayıcı e-posta ekseninde engellidir; kullanıcı şifre sıfırlama bağlantısını kullanır — bağlantı e-postanın sahibine gider. Şifre yenilenir, hesabın bütün oturumları kapanır, sıfırlayan tarayıcı tanınan tarayıcı olur ve öteki işaretler düşer | Şifre yenilenir | Şifre sıfırlama bağlantısı (`02 §9.4`) | `02 §8.2.6`, §3.13.10 |
| 6.1.3.4 | Saldırgan şifre sıfırlama taleplerini de doldurur | L-2 o eksende talebi engeller; gönderilen bağlantılar hedefin posta kutusuna gitmiştir. Gerçek kullanıcı engel açılınca talep eder — kalan risktir | — | — | `02 §8.2.6` |
| 6.1.3.5 | Kullanıcı Google ile girer | Tahmin edilen bir şifre yoktur; L-1'in engeli Google ile girişi kapatmaz | Oturum açılır | Olağan (§9.2.2) | `02 §8.2.6` |

#### 6.1.4 Stok ve kupon kilitleme girişimi

| # | Aktör ne yapar | Sistem ne denetler, ne yapar | Durum | Bildirim | Kaynak |
|---|---|---|---|---|---|
| 6.1.4.1 | Biri ödemeden art arda sipariş açarak stoğu ve kupon haklarını ayırır | Her ödenmemiş sipariş süresi dolunca kendiliğinden iptal edilir ve ayırmasını bırakır; L-7 iptallerin tekrarını sayar ve yeni sipariş onayını engeller | İptallerde → İptal edildi + Başarısız (S3, Ö2) | İptallerde B-7 → siparişin e-postası | `02 §8.2.3` |
| 6.1.4.2 | Biri havale siparişleriyle bir ürünün stoğunu iş günleri boyunca kilitler | L-8 aynı anda açık ödenmemiş siparişleri sayar ve tavan doluyken havaleyi kapatır; kart yolu açıktır | — | — | `02 §8.2.3` |
| 6.1.4.3 | Saldırgan çok sayıda IP'den misafir havale siparişi açar | Ürün kendiliğinden durdurmaz; firmanın inceleme yolu §6.3.1'dedir | — | — | `02 §8.2.3` |
| 6.1.4.4 | Paylaşılan bir IP'nin arkasındaki gerçek müşteri başkasının denemeleriyle IP ekseninde engellenir | Engel kendiliğinden açılır; L-8'de kart yolu açıktır; ayrı muafiyet ya da kurtarma yolu yoktur — kalan risktir | — | — | `02 §8.2.2` |

### 6.2 Teyit ve kandırılma akışları

#### 6.2.1 Siparişin kendi kanalına dönerek teyit — ortak kalıp

| # | Aktör ne yapar | Sistem ne denetler, ne yapar | Durum | Bildirim | Kaynak |
|---|---|---|---|---|---|
| 6.2.1.1 | Yönetici siparişe dokunan bir talep alır — adres düzeltmesi, başka kanaldan cayma bildirimi, "müşteriyle anlaşıldı (müşteri talebi)" iptali | Talep siparişin iletişim e-postasından geldiyse teyit budur | — | — | `02 §10.4.2`, §8.3.8 |
| 6.2.1.2 | Talep telefonla, iletişim formuyla, başka bir adresten ya da mektupla gelir | Yönetici siparişi numarasıyla ya da e-postasıyla bulur (8.2.11; K-714) ve siparişin kendi kanallarından birine döner: siparişin iletişim e-postasına yazar ya da teslimat telefonunu arar. Talebin geldiği numara ya da adres ve sipariş numarası tek başına teyit değildir | — | — | `02 §10.4.2`, §10.4.10 · K-714 |
| 6.2.1.3 | Misafir siparişinin e-posta düzeltmesi istenir | Siparişin e-postası yanlış olduğu için kimlik siparişteki bilgilerle — ad, fiziksel kalemli siparişte teslimat telefonu, kalemler ve tutar — teyit edilir | — | — | `02 §8.3.6`, §10.4.11 |
| 6.2.1.4 | Siparişin sahibine ulaşılamaz | Kaydın ya da düzeltmenin yapılıp yapılmayacağı firmanın değerlendirmesidir; kaydedilmeyen gerçek bir bildirimin sonucu da firmadadır. Teyit kaydın tarihini değiştirmez — tarih bildirimin firmaya ulaştığı tarihtir | — | — | `02 §10.4.10` |

#### 6.2.2 Adres çevirtme girişimi

| # | Aktör ne yapar | Sistem ne denetler, ne yapar | Durum | Bildirim | Kaynak |
|---|---|---|---|---|---|
| 6.2.2.1 | Biri firmaya ulaşıp siparişin teslimat adresini kendi adresine çevirtmek ister | Yönetici 6.2.1'in kalıbıyla teyit eder | — | — | `02 §8.3.7` |
| 6.2.2.2 | Teyit atlanır ya da kandırılır; yönetici adresi düzeltir (§1.11.23) | Düzeltme işlem izine alan ve işlem olarak yazılır — adres değeri ize girmez | — | B-10 → siparişin iletişim e-postası | `02 §10.4.2`, §10.3.1 |
| 6.2.2.3 | Gerçek sahip B-10'u görür ve firmaya ulaşır | Sipariş Kargoya verildi'ye geçmediyse yönetici adresi geri düzeltir; paket yola çıktıysa adres düzeltilemez ve kandırılmanın sonucu firmadadır | — | Geri düzeltmede B-10 | `02 §6.3.6`, §8.3.7 |

#### 6.2.3 Sahte cayma ya da iptal bildirimi

| # | Aktör ne yapar | Sistem ne denetler, ne yapar | Durum | Bildirim | Kaynak |
|---|---|---|---|---|---|
| 6.2.3.1 | Siparişin sahibi olmayan biri siparişin bilgilerini bilerek başka kanaldan cayma ya da iptal ister — havale hattında kendi IBAN'ıyla | Yönetici 6.2.1'in kalıbıyla teyit eder; sahibi bildirimi yapmadığını söylerse kayıt yapılmaz | — | — | `02 §6.4.33`, §8.3.8 |
| 6.2.3.2 | Teyit atlanır; yönetici caymayı kaydeder ya da kalemi "müşteriyle anlaşıldı (müşteri talebi)" sebebiyle iptal eder | Bildirimdeki IBAN aktarılmaz — bildirim siparişin iletişim e-postasından gelmemiştir. Para siparişin kendi yoluna gider: kartta karta, havalede yalnız sipariş sayfasından girilen IBAN'a | kalem: cayma beyanı ya da iptal kaydı | B-9 ya da B-7 → siparişin iletişim e-postası · IBAN'sız kayıtta ve havale hattında ödenmiş kalemin iptalinde B-14 (K-690) | `02 §8.3.8`, §10.4.10 · K-690 |
| 6.2.3.3 | Gerçek sahip bildirimi görür ve firmaya ulaşır | Yanlış iptal yalnız ödeme Ödendi'de kalmışken düzeltilir (§1.6.2.2). **Cayma beyanı geri alınmaz** — ürünün düzeltmeleri durum geçişleri içindir (§1.6.2): teslim edilmiş kalemde mal dönmez ve yönetici Z-42'den sonra "mal dönmedi" kapanışını yapar; teslim işaretsiz kalemde beyan kalemi kapatmıştır ve geri ödeme siparişin kendi yoluna gider. Kandırılmanın sonucu firmadadır (K-688) | Yanlış iptalin düzeltilmesinde §1.6.2.2 | Düzeltmede B-13 → müşteri | `02 §8.3.8`, §10.4.6 · K-688 |

#### 6.2.4 Misafir siparişinin e-postasını çevirtme

| # | Aktör ne yapar | Sistem ne denetler, ne yapar | Durum | Bildirim | Kaynak |
|---|---|---|---|---|---|
| 6.2.4.1 | Biri firmaya ulaşıp misafir siparişinin e-postasını kendi adresine çevirtmek ister | Yönetici kimliği siparişteki bilgilerle teyit eder (6.2.1.3) | — | — | `02 §8.3.6` |
| 6.2.4.2 | Teyit kandırılır; yönetici e-postayı düzeltir (§1.11.35) | Yeni erişim anahtarı üretilir, eski bağlantılar açılmaz; sipariş yanlış hesaptan kopar, yeni adres doğrulanmış bir hesabınsa o hesaba düşer. Panel "e-posta düzeltildi" işaretini gösterir; iki adres e-posta geçmişindedir | — · "e-posta düzeltildi" işareti | B-1 → yeni adres · B-15 → eski adres | `02 §10.4.11` |
| 6.2.4.3 | Kandıran sipariş sayfasına girer — iptal eder, cayar, IBAN girer ya da düzeltir | Para siparişin yoluna gider: kartta karta; havalede sipariş sayfasından girilen IBAN'a — sayfaya kandıran girdiği için IBAN'ı da o girer | Yapılan işlemin durumu | İşlemin bildirimi — yeni adrese | `02 §8.3.6` |
| 6.2.4.4 | Gerçek sahip B-15'i görür ve firmaya ulaşır | Yönetici e-postayı yeniden düzeltir; yeni bir anahtar üretilir. Arada yapılan işlemler geri alınmaz, kalem kayıtlarından ve e-posta geçmişinden okunur — işlem izi yalnız yönetici işlemlerini tutar (K-710); geri ödeme işlenmeden önceyse gerçek sahip IBAN'ı sipariş sayfasından düzeltir (§1.11.12) | — | B-1 → gerçek sahibin adresi · B-15 → kandıranın adresi | `02 §6.3.5`, §7.4.5 · K-710 |

#### 6.2.5 Hesabın e-postasının ele geçirilmesi

| # | Aktör ne yapar | Sistem ne denetler, ne yapar | Durum | Bildirim | Kaynak |
|---|---|---|---|---|---|
| 6.2.5.1 | Biri açık kalmış ya da ele geçirilmiş bir oturumdan, şifreyi bilerek hesabın e-postasını değiştirir | İşlem yeniden doğrulama ister — şifre ya da Google; değişiklik yeni adres doğrulanınca geçerli olur (§9.3.4, §9.3.5) | Hesabın e-postası değişir | Yeni adrese doğrulama bağlantısı · eski adrese değişiklik bildirimi ve geri alma bağlantısı (`02 §9.4`) | `02 §8.1.5`, §3.13.14 |
| 6.2.5.2 | Gerçek sahip eski adresteki "bu değişikliği ben yapmadım" bağlantısını Z-45 içinde kullanır | E-posta geri alınır; hesabın bütün oturumları kapanır, tanınan tarayıcı işaretleri düşer ve şifre eski adrese giden bağlantıyla yeniden belirlenir. Bağlantı açıkken e-posta yeniden değiştirilemez | Hesabın e-postası geri döner | Şifre sıfırlama bağlantısı → eski adres (`02 §9.4`) | `02 §3.13.14`, §8.2.6 |
| 6.2.5.3 | Z-45 dolmuştur ya da eski posta kutusu da ele geçirilmiştir | Ürün içinde geri alma yolu yoktur — kalan risktir. Siparişlerin erişim anahtarı hesabın e-posta değişikliğinden etkilenmez: gerçek sahip sipariş sayfalarına e-postalarındaki bağlantıyla girmeye devam eder (§2.6.3) | — | — | `02 §8.1.5`, §3.22.5 |

#### 6.2.6 Havale IBAN'ının yetkisiz değişikliği

| # | Aktör ne yapar | Sistem ne denetler, ne yapar | Durum | Bildirim | Kaynak |
|---|---|---|---|---|---|
| 6.2.6.1 | Ele geçirilmiş bir yönetici hesabıyla havale IBAN'ı değiştirilir | Değişiklik işlem izine eski ve yeni değerle yazılır. Yeni IBAN yalnız yeni siparişlere işler; ödemesi beklenen siparişler donmuş IBAN'ı göstermeye devam eder | — | F-5 → bütün yöneticilerin kendi adresleri — değişikliği yapan, tarih, saat ve yeni IBAN'ın son dört hanesi | `02 §9.3.3`, §3.21.5 · K-663 |
| 6.2.6.2 | Bir yönetici F-5'i görür ve değişikliği tanımaz | Panele girip IBAN'ı düzeltir ve şifresini değiştirir; ürün değişikliği kendiliğinden geri almaz ve havaleyi kapatmaz. Firma bu anda bir ihlali öğrenmiş olabilir: §6.3.2'nin akışı ve Z-37 bu andan işler (§6.3.2.1) | — | Düzeltmede F-5 → bütün yöneticiler | `02 §9.3.3` |
| 6.2.6.3 | Değişiklikle düzeltme arasında onaylanmış siparişler yanlış IBAN'ı taşır | Yönetici onları firma iptaliyle ("diğer", açıklamayla) kapatır; müşteri yeni sipariş verir. Yanlış hesaba ödeme yapmış müşterinin parası ürünün dışında çözülür ve sonucu firmadadır (K-663) | → İptal edildi + Başarısız (S3, Ö2) | B-7 → müşteri | `02 §6.9.14` · K-663 |
| 6.2.6.4 | Saldırgan, ele geçirilmiş yönetici hesabıyla IBAN'ı değiştirmeden önce öteki yöneticileri kaldırır — F-5 yalnız ona gidecektir | Her kaldırma, kaldırılan dahil bütün yöneticilere bildirilir; kaldırılan yöneticiler oturumlarını kaybettiklerini ve kimin kaldırdığını e-postadan öğrenir. Son yönetici kaldırılamadığı için saldırgan kendisini kaldıramaz; panele erişimi kaybeden firmanın yolu kurulum tarafının kurtarmasıdır (§10.4.5; K-699). Z-37 firmanın durumu öğrendiği andan işler, kurtarma beklenirken de (§6.3.2.1) | — | F-6 → bütün yöneticiler, kaldırılan dahil (K-692) | `02 §9.3.4`, §10.2.2 · K-692, K-699 |

### 6.3 Firmanın inceleme akışları

#### 6.3.1 Şüpheli açık havale siparişleri

| # | Aktör ne yapar | Sistem ne denetler, ne yapar | Durum | Bildirim | Kaynak |
|---|---|---|---|---|---|
| 6.3.1.1 | Yönetici bir ürünün stoğunun, kontenjanının ya da kuponun son haklarının açık havale siparişlerince kilitlendiğini görür | "Ödeme onayı bekleyen" sayacı süzülmüş listeye götürür; stok alanının yanında ayrılmış adet ve onu tutan siparişler görünür (§3.5.1.14) | — | — | `02 §10.6.1`, §8.2.3 · K-653 |
| 6.3.1.2 | Yönetici şüpheli siparişleri firma iptaliyle kapatır — "diğer", açıklamayla | Ayrılanlar aynı anda serbest kalır; ürün şüpheyi değerlendirmez | → İptal edildi + Başarısız (S3, Ö2) | B-7 → siparişin iletişim e-postası | `02 §8.2.3`, §7.2.4 |
| 6.3.1.3 | Yönetici havaleyi geçici olarak kapatır | Yeni siparişte havale görünmez, kart açık kalır. Açık siparişler etkilenmez — kilidi çözen iptaldir, kapatma değil | — | — (`02 §9.3.2`) | `02 §3.21.5`, §8.2.3 · K-664 |

#### 6.3.2 Veri ihlalinde kapsam

| # | Aktör ne yapar | Sistem ne denetler, ne yapar | Durum | Bildirim | Kaynak |
|---|---|---|---|---|---|
| 6.3.2.1 | Firma bir ihlalden şüphelenir ya da öğrenir | Ürün ihlali tespit etmez ve uyarı üretmez; panelde ihlal ekranı yoktur. Kurul'a bildirim (Z-37) ve ilgili kişilere bildirim firmanın yükümlülüğüdür | — | — (`02 §9.5`) | `02 §8.5.1`, §12.2.9 |
| 6.3.2.2 | Yönetici işlem izini tarih ve yönetici süzgeciyle okur (§8.9) | İz hangi yöneticinin neye dokunduğunu söyler — dışa aktarmalar dahil; iz değiştirilemez ve silinemez | — | — | `02 §10.3` |
| 6.3.2.3 | Yönetici giriş kaydına ve sistem kayıtlarına bakmak ister | Giriş kaydı panelde görünmez; sistem kayıtlarıyla birlikte kurulumun barındırma tarafında okunur — okuma yolu `05`'in işidir (K-687). Limit sayaçları kaynak değildir | — | — | `02 §8.5.1`, §3.13.20 · K-687 |
| 6.3.2.4 | Yönetici ele geçirilmiş bir yönetici hesabını kaldırır | Kaldırma §8.8.5'in yoludur: oturumları anında sonlanır, gönderdiği kullanılmamış davetler düşer; son yönetici kaldırılamaz | — | F-6 → bütün yöneticiler, kaldırılan dahil (K-692) | `02 §10.2.2`, §8.1.7 · K-662, K-692 |

#### 6.3.3 Etkilenen kişilere ulaşma

| # | Aktör ne yapar | Sistem ne denetler, ne yapar | Durum | Bildirim | Kaynak |
|---|---|---|---|---|---|
| 6.3.3.1 | Yönetici etkilenen kişilerin listesini çıkarır | Sipariş dışa aktarması siparişin donmuş iletişim e-postasını ve alıcı adını taşır; üye listesi ve iletişim talepleri ayrıca dışa aktarılır. Her dışa aktarma işlem izine yazılır ve dosyanın sorumluluğu firmaya geçer | — | — | `02 §10.7.1`, §12.2.9 |
| 6.3.3.2 | Firma kişilere yazar | Ürünün toplu e-posta hattı yoktur; firma kendi e-posta aracını kullanır. Ulaşılamayan kişilere sitede yayım gerekiyorsa firmanın sitedeki araçları duyuru şeridi (8.6.3.1) ve genel sayfadır (8.6.1); içerik ve karar firmanındır (`02 §12.2.9`) | — | — (`02 §9.5`) | `02 §9.5`, §12.2.9 |

## 7. Bildirim haritası

> **Ne yazılır:** Akışlardan çıkarılmış, tek tabloda özetlenmiş bildirim listesi. Bildirim sistemi tasarımını bu tablo besler.

Bu bölüm §1, §2, §4–§6 ve §8–§10'un bildirim hücrelerinden çıkarılmıştır — §3'ün hata satırları ana akışın satırına bağlandığı için haritada ayrıca gösterilmez — ve `02 §9`'un olay bazlı matrisini akışın satırlarına bağlar. Bildirimi olan her olay §7.1'in tek tablosunda bir satırdır; bildirimi olmayan olaylar §7.2'de kaynağıyla durur; §7.3 aynı eşlemeyi tersinden — durum geçişinden ve olaydan bildirime — okur (K-689). Haritada olup akışta olmayan ya da akışta olup haritada olmayan bildirim kalmaz; iki yön betikle karşılaştırılır.

- **Kanal:** tek kanal e-postadır; SMS, anlık bildirim ve uygulama içi bildirim merkezi yoktur. İşlem bildirimleri ticari ileti değildir ve kapatılamaz — bildirim tercihi ekranı yoktur. Gönderen adı firmanın marka adıdır ve e-postalar yanıtlanabilir; yanıt firma kimliğindeki iletişim e-postasına düşer (`02 §9.1`).
- **Adres:** müşteri bildirimi siparişin iletişim e-postasına, firma bildirimi firma kimliğindeki iletişim e-postasına gider; F-5 ve F-6 her yönetici hesabının kendi adresine, hesap ve yönetim e-postaları hesabın ya da davetlinin adresine gider — hesabın silindiği bildirimi silinen hesabın adresine (`02 §9.3.1`, §9.3.3, §9.4; K-693).
- **Ulaşmayan e-posta:** üç kez yeniden denenir (Z-26). Müşteri bildiriminde (B-1…B-16) siparişin satırına "e-posta ulaşmadı" düşer ve yönetici e-postayı siparişin ayrıntısından yeniden gönderir (8.9.2; K-759); firmaya giden bildirimde (F-1…F-4) satıra işaret düşmez, panelin ana sayfasında "firma bildirimleri ulaşmıyor" uyarısı çıkar (8.9.1; K-760); F-5, F-6 ve hesap e-postaları yeniden gönderilmez — davette işaret davetin satırına düşer ve yol yeni davettir (8.8.1, 10.1.2.2). Hiçbir sipariş ya da talep akışı bir e-postanın ulaşmasına bağlı değildir (§3.1.1).
- **İçerik özeti** e-postanın taşıdığı bilgiyi söyler; metni, şablonu ve gönderim biçimi `08`'in işidir. Gönderim kaydının saklama süresi B-1…B-16'da Z-43, öteki e-postalarda Z-34'tür.
- **Akış sütunu** §1, §2, §4, §5, §6, §8, §9 ve §10'un satırlarını gösterir; §9'a giden hücreler §9 yazılana kadar alt bölümü gösteriyordu ve 5. oturumda satır numarasına çevrildi (K-682, K-689).
- **Bildirim sütunundaki kısa ad** gezinme etiketidir; bildirimin tanımı `02 §9`'un olay sütunudur ve e-postanın adı oradan alınır (`08`).

### 7.1 Bildirimler ve tetikleyicileri

| # | Bildirim | Tetikleyici (durum geçişi / olay) | Akış | Alıcı | İçerik özeti | Kanal |
|---|---|---|---|---|---|---|
| | **Müşteriye giden bildirimler** (`02 §9.2`) | | | | | |
| 7.1.1 | B-1 Sipariş alındı | Sipariş oluşur — Alındı + Bekliyor | §2.4.9, §2.10.1.5, §4.2.1 | Müşteri | Sipariş numarası, sipariş sayfasının bağlantısı ve siparişe donan Ön Bilgilendirme Formu ile Mesafeli Satış Sözleşmesi — gövdede tam metin | E-posta · siparişin iletişim e-postası |
| 7.1.2 | B-1 Sipariş alındı | Yönetici misafir siparişinin e-postasını düzeltir | §8.4.9, §2.6.7, §6.2.4.2, §6.2.4.4 | Müşteri — yeni adres | 7.1.1'in donmuş içeriği; bağlantı yeni erişim anahtarını taşır | E-posta · düzeltilmiş adres |
| 7.1.3 | B-2 Havale/EFT seçildi | Sipariş havaleyle oluşur | §2.4.9, §2.5.2.1, §2.10.1.5, §4.2.1 | Müşteri | Firmanın siparişe donan IBAN'ı ve sipariş numarası | E-posta · siparişin iletişim e-postası |
| 7.1.4 | B-3 Havale ödeme hatırlatması | Havale ödeme süresinin son iş gününün başı — süre başına bir kez (Z-9) | §2.5.2.3, §4.1.9 | Müşteri | Havale ödemesinin beklendiğinin hatırlatması | E-posta · siparişin iletişim e-postası |
| 7.1.5 | B-3 Havale ödeme hatırlatması | Havale "ödendi" işaretinin düzeltilmesiyle yeniden başlayan sürenin son iş günü — bir kez daha | §8.4.5, §1.6.2.4, §4.1.9 | Müşteri | 7.1.4'ün içeriği | E-posta · siparişin iletişim e-postası |
| 7.1.6 | B-3 Havale ödeme hatırlatması | Kesintide zamanı gelmiş hatırlatma — site dönünce, havale süresi hâlâ işliyorsa; süre dolmuşsa ve kesintide ertelenen iptalde gitmez | §4.2.11, §4.1.9 | Müşteri | 7.1.4'ün içeriği | E-posta · siparişin iletişim e-postası |
| 7.1.7 | B-4 Ödeme onaylandı | Kart ödemesi onaylanır — sağlayıcının başarı bildirimi ya da Z-7 dolduğunda son sorgunun olumlu cevabı (Ö1) | §2.5.1.2, §2.5.1.4, §2.5.3.1, §2.10.1.6, §4.2.2, §8.2.3, §10.1.1.4 | Müşteri | Ödemenin onaylandığı; siparişte dijital kalem varsa indirmenin hazır olduğu, aynı e-postada | E-posta · siparişin iletişim e-postası |
| 7.1.8 | B-4 Ödeme onaylandı | Yönetici havale "ödendi" işaretini koyar (Ö1) — kesintide ertelenen iptalin ertelendiği ana kadar dahil | §8.2.2, §2.5.2.4, §2.10.1.6, §10.3.4 | Müşteri | 7.1.7'nin içeriği | E-posta · siparişin iletişim e-postası |
| 7.1.9 | B-5 Kargoya verildi | Yönetici siparişi kargoya verir (S5) | §8.2.5, §2.5.3.3, §2.10.1.7 | Müşteri | Kargo şirketi, takip numarası ve listedeki şirketlerde "Takip et" bağlantısı; "kendi aracımızla teslim"de bu beyan | E-posta · siparişin iletişim e-postası |
| 7.1.10 | B-5 Kargoya verildi | Yönetici açık fiziksel kalemleri yeniden gönderir (S8) | §8.2.8, §2.5.3.4 | Müşteri | 7.1.9'un içeriği — yeni takip bilgisiyle | E-posta · siparişin iletişim e-postası |
| 7.1.11 | B-6 Teslim edilemedi | Yönetici geri dönen gönderiyi işaretler (S7) | §8.2.7, §2.5.3.4, §2.7.8 | Müşteri | Gönderinin firmaya geri döndüğü | E-posta · siparişin iletişim e-postası |
| 7.1.12 | B-7 İptal edildi | Müşteri ödeme beklenirken siparişi iptal eder (S3, Ö2) | §2.7.1 | Müşteri | İptal — müşterinin kendi işleminin kayıt kopyası | E-posta · siparişin iletişim e-postası |
| 7.1.13 | B-7 İptal edildi | Müşteri ödenmiş siparişte fiziksel ya da hizmet kalemini iptal eder — son açık kalemse S3, S4 ya da S10 ile | §2.7.2, §2.7.3, §4.2.4 | Müşteri | İptal edilen kalem ve adedi — kayıt kopyası | E-posta · siparişin iletişim e-postası |
| 7.1.14 | B-7 İptal edildi | Yönetici siparişi ya da kalemi sebep seçerek iptal eder (S3, S4, S9) — "müşteriyle anlaşıldı" iptali dahil | §8.3.1.1, §8.3.1.2, §6.2.3.2, §6.2.6.3, §6.3.1.2, §10.4.6 | Müşteri | Seçilen sebep ve kalem iptalinde iptal edilen kalem ve adedi — ifanın imkânsızlaşmasının yasal bildirimi, iptal anında | E-posta · siparişin iletişim e-postası |
| 7.1.15 | B-7 İptal edildi | Sistem siparişi kendiliğinden iptal eder (S3, Ö2) — Z-7 ya da Z-8 dolar, kart ödemesi başarısız olur ya da sağlayıcıya bağlanılamaz, son sorgu ödemenin alınmadığını söyler, aynı sepetten yeni sipariş onaylanır, kesintide ertelenen iptal işler | §2.4.9, §2.5.1.3, §2.5.1.4, §2.5.2.5, §4.2.1, §4.2.3, §4.2.12, §6.1.4.1, §10.1.1.2, §10.1.1.4, §10.3.4 | Müşteri | İptal ve sebebi | E-posta · siparişin iletişim e-postası |
| 7.1.16 | B-8 Geri ödeme işlendi | Sistemin başlattığı kart iadesi sağlayıcıda gerçekleşir — iptal edilen ödenmiş kalem, gecikme feshi, kalem çıkarma (Ö3–Ö5) | §8.3.3.1, §2.7.6, §2.7.8, §4.2.7, §5.2.1.4 | Müşteri | Geri ödemenin işlendiği | E-posta · siparişin iletişim e-postası |
| 7.1.17 | B-8 Geri ödeme işlendi | Yönetici geri ödemeyi işler — havale hattında iptal, fesih ve cayma; kart hattında cayma; tutar bazlı kısmi geri ödeme; ayıp talebinin para gerektiren çözümü (Ö3–Ö5) | §8.3.3.2, §8.3.3.3, §8.3.3.7, §2.8.4.1, §2.9.4, §5.1.6, §5.2.2.4, §5.3.1.6, §5.3.2.2 | Müşteri | Geri ödemenin işlendiği | E-posta · siparişin iletişim e-postası |
| 7.1.18 | B-8 Geri ödeme işlendi | Yönetici kart iadesini yeniden dener ve iade gerçekleşir (Ö3–Ö5) | §8.3.3.5, §10.1.1.5 | Müşteri | Geri ödemenin işlendiği | E-posta · siparişin iletişim e-postası |
| 7.1.19 | B-8 Geri ödeme işlendi | Sistem iptal edilmiş siparişe gelen kart ödemesini kendiliğinden geri öder — eksenler değişmez | §4.2.7, §10.1.1.4 | Müşteri | İptal edilmiş siparişe gelen ödemenin geri ödendiği | E-posta · siparişin iletişim e-postası |
| 7.1.20 | B-9 Cayma beyanı alındı | Müşteri sipariş sayfasından cayma beyanında bulunur — fiziksel ya da hizmet kalemi | §2.8.1.2, §2.8.2.2, §5.3.1.1 | Müşteri | Beyanın tarih damgası, kalemleri ve adetleri — kayıt kopyası; fiziksel kalemde beyana yazılan iade adresi | E-posta · siparişin iletişim e-postası |
| 7.1.21 | B-9 Cayma beyanı alındı | Yönetici başka kanaldan gelen cayma bildirimini kaydeder | §8.4.8, §2.8.4.4, §5.2.3.2, §5.2.4.3, §6.2.3.2 | Müşteri | Beyanın tarihi — bildirimin firmaya ulaştığı tarih —, kalemleri ve adetleri; fiziksel kalemde iade adresi (K-790) | E-posta · siparişin iletişim e-postası |
| 7.1.22 | B-9 Gecikme feshi alındı | Müşteri gecikme nedeniyle fesheder ya da yönetici başka kanaldan gelen fesih bildirimini kaydeder (K-706) | §2.7.7, §4.2.4, §8.4.8 | Müşteri | Feshin tarih damgası, kapsadığı kalemler ve açık adetleri — kayıt kopyası (K-790, K-796) | E-posta · siparişin iletişim e-postası |
| 7.1.23 | B-9 Ayıp talebi alındı | Müşteri ayıp talebi açar | §2.9.2 | Müşteri | Talebin tarih damgası, kalemi ve ayıplı adedi; fiziksel kalemde talebe yazılan iade adresi (K-790, K-793) | E-posta · siparişin iletişim e-postası |
| 7.1.24 | B-9 Ayıp talebi alındı | Müşteri çözülmüş ayıp talebini yeniden açar | §2.9.6, §5.1.4 | Müşteri | Yeniden açmanın tarih damgası, kalemi ve ayıplı adedi; fiziksel kalemde talebe yazılı iade adresi (K-790, K-793) | E-posta · siparişin iletişim e-postası |
| 7.1.25 | B-10 Adres düzeltildi | Yönetici teslimat ya da fatura adresini düzeltir — geri düzeltme dahil | §8.4.1, §6.2.2.2, §6.2.2.3 | Müşteri | Düzeltilen adres; değişikliği istemediyse firmaya ulaşması gerektiği | E-posta · siparişin iletişim e-postası |
| 7.1.26 | B-11 Kalem çıkarıldı | Yönetici kalemi çıkarır ya da adedini azaltır | §8.4.2, §4.2.4 | Müşteri | Değişen kalem | E-posta · siparişin iletişim e-postası |
| 7.1.27 | B-12 Takip bilgisi düzeltildi | Yönetici kargo şirketini ya da takip numarasını düzeltir | §8.4.3 | Müşteri | Düzeltilmiş takip bilgisi | E-posta · siparişin iletişim e-postası |
| 7.1.28 | B-13 Durum düzeltildi | Yönetici yanlış durum geçişini düzeltir | §8.4.5, §6.2.3.3 | Müşteri | Siparişin düzeltilmiş durumu — havale işaretinin düzeltilmesinde ödemenin alınmadığı, havale bilgisi ve yeni son ödeme günü | E-posta · siparişin iletişim e-postası |
| 7.1.29 | B-14 IBAN gerekli | Havale hattında IBAN silindikten sonra iade malı ulaşır ve yönetici teslim alma adımını işler | §8.3.2.1, §2.10.4 | Müşteri | Geri ödeme için IBAN'ın sipariş sayfasından girilmesi gerektiği ve iade malının ulaştığı; sipariş sayfasının bağlantısı | E-posta · siparişin iletişim e-postası |
| 7.1.30 | B-14 IBAN gerekli | Kart iadesi sağlayıcıda gerçekleşmez ve yönetici havale yolunu açar | §8.3.3.5, §2.8.4.3, §10.1.1.5 | Müşteri | Kart iadesinin gerçekleşmediği ve IBAN girmenin müşterinin seçimi olduğu; bağlantı | E-posta · siparişin iletişim e-postası |
| 7.1.31 | B-14 IBAN gerekli | Yönetici geri ödeme havalesinin bankada gerçekleşmediğini bildirir | §8.3.3.8, §2.8.4.2 | Müşteri | Havalenin girilen IBAN'a gerçekleşmediği; bağlantı | E-posta · siparişin iletişim e-postası |
| 7.1.32 | B-14 IBAN gerekli | Yönetici havale hattında IBAN'sız cayma ya da gecikme feshi kaydı yapar (K-706) | §8.4.8, §2.8.4.4, §5.2.3.2, §6.2.3.2 | Müşteri | Bildirimin IBAN taşımadığı; bağlantı | E-posta · siparişin iletişim e-postası |
| 7.1.33 | B-14 IBAN gerekli | Yönetici havale hattında IBAN'ı olmayan bir geri ödeme için IBAN ister — ayıp talebinin çözümü, tutar bazlı kısmi geri ödeme, ret ya da "mal dönmedi" kapanışından sonraki ödeme | §8.3.3.6, §2.9.4, §5.1.6, §5.2.2.4, §5.3.1.6 | Müşteri | Geri ödeme için IBAN'ın girilmesi gerektiği; bağlantı | E-posta · siparişin iletişim e-postası |
| 7.1.34 | B-14 IBAN gerekli | Havale hattında ödenmiş kalemde yönetici firma iptali ya da kalem çıkarması yapar | §8.3.1.1, §8.4.2 | Müşteri | İptalin ya da çıkarmanın geri ödemesi için IBAN'ın girilmesi gerektiği; bağlantı | E-posta · siparişin iletişim e-postası |
| 7.1.35 | B-15 E-posta adresi değiştirildi | Yönetici misafir siparişinin e-postasını düzeltir | §8.4.9, §2.6.7, §6.2.4.2, §6.2.4.4 | Müşteri — eski adres | Siparişin e-posta adresinin değiştirildiği, sipariş numarası ve tarih; istemediyse firmaya ulaşması gerektiği. Yeni adresi ve bağlantıyı taşımaz | E-posta · eski adres |
| 7.1.36 | B-16 İade reddedildi | Yönetici koşullu istisna kaleminde iade malını reddeder | §8.3.2.3, §2.8.1.6, §5.3.1.2 | Müşteri | Reddedilen kalem ve sebebi; geri ödeme yapılmayacağı; malın teslimat adresine firmanın bedeliyle geri gönderileceği; uyuşmazlık yolları | E-posta · siparişin iletişim e-postası |
| 7.1.37 | B-1…B-16 (`02 §9.1.6`; K-759) | Yönetici "e-posta ulaşmadı" işaretli siparişin ayrıntısından e-postayı yeniden gönderir (K-761); işareti düşüren on altı müşteri bildiriminin hepsi yeniden gönderilir, firma bildirimleri ve hesap e-postaları yeniden gönderilmez (K-759, K-760) | §8.9.2 | İlk alıcı | İlk e-postanın içeriği — sipariş e-postasında donmuş sürüm | E-posta · ilk adres |
| | **Firmaya ve yöneticilere giden bildirimler** (`02 §9.3`) | | | | | |
| 7.1.38 | F-1 Yeni sipariş | Sipariş oluşur | §2.4.9, §2.10.1.5, §4.2.1, §8.2.1 | Firma | Yeni sipariş | E-posta · firma kimliğindeki iletişim e-postası |
| 7.1.39 | F-2 Yeni iletişim talebi | İletişim formu gönderilir — KVKK talebi dahil | §2.2.8, §5.1.2, §5.2.2.2, §5.2.3.1, §5.2.4.2, §8.6.4.1, §9.4.4 | Firma | Yeni iletişim talebi | E-posta · firma kimliğindeki iletişim e-postası |
| 7.1.40 | F-3 Yeni cayma beyanı | Müşteri sipariş sayfasından cayma beyanında bulunur | §2.8.1.2, §2.8.2.2, §5.3.1.1 | Firma | Yeni cayma beyanı | E-posta · firma kimliğindeki iletişim e-postası |
| 7.1.41 | F-3 Yeni gecikme feshi | Müşteri gecikme nedeniyle fesheder | §2.7.7 | Firma | Yeni gecikme feshi | E-posta · firma kimliğindeki iletişim e-postası |
| 7.1.42 | F-3 Yeni ayıp talebi | Müşteri ayıp talebi açar | §2.9.2, §8.4.10 | Firma | Yeni ayıp talebi | E-posta · firma kimliğindeki iletişim e-postası |
| 7.1.43 | F-3 Yeni ayıp talebi | Müşteri çözülmüş ayıp talebini yeniden açar | §2.9.6, §5.1.4 | Firma | Yeni ayıp talebi gibi — yeni talep açılmaz | E-posta · firma kimliğindeki iletişim e-postası |
| 7.1.44 | F-4 Kendiliğinden kart iadesi | Sistem kart hattında geri ödemeyi kendiliğinden yapar — iptal edilen ödenmiş kalem (müşteri ya da firma iptali), gecikme feshi, kalem çıkarma | §8.3.3.1, §2.7.6, §2.7.8, §4.2.7, §5.2.1.4 | Firma | Sistemin karta yaptığı geri ödeme | E-posta · firma kimliğindeki iletişim e-postası |
| 7.1.45 | F-4 Kendiliğinden kart iadesi | Sistem iptal edilmiş siparişe gelen kart ödemesini geri öder | §4.2.7, §10.1.1.4 | Firma | 7.1.44'ün içeriği | E-posta · firma kimliğindeki iletişim e-postası |
| 7.1.46 | F-5 Havale IBAN'ı değişti | Havale IBAN'ı panelde girilir ya da değiştirilir — ilk giriş ve düzeltme dahil | §8.7.3.1, §8.7.3.2, §6.2.6.1, §6.2.6.2, §10.4.6 | Bütün yöneticiler — değişikliği yapan dahil | Değişikliğin yapıldığı, tarihi ve saati, yapan yönetici ve yeni IBAN'ın son dört hanesi; tam IBAN'ı ve geri alma bağlantısını taşımaz | E-posta · her yönetici hesabının kendi adresi |
| 7.1.47 | F-6 Yönetici hesapları değişti | Yönetici daveti gönderilir | §8.8.1, §10.4.3, §10.4.6 | Bütün yöneticiler — gönderen dahil | Davetin gönderildiği, davet edilen adres, gönderen yönetici, tarih ve saat | E-posta · her yönetici hesabının kendi adresi |
| 7.1.48 | F-6 Yönetici hesapları değişti | Bir yönetici kaldırılır | §8.8.5, §8.8.6, §6.2.6.4, §6.3.2.4, §10.4.3, §10.4.6 | Bütün yöneticiler — kaldıran ve kaldırılan dahil | Kaldırmanın yapıldığı, kaldırılan ve kaldıran yönetici, tarih ve saat | E-posta · her yönetici hesabının kendi adresi |
| | **Hesap ve yönetim e-postaları** (`02 §9.4`) — matrisin dışında, akışın parçası olan bağlantılı e-postalar | | | | | |
| 7.1.49 | E-posta doğrulama bağlantısı | Hesap kaydı ve kullanıcının bağlantıyı yeniden istemesi (L-9) | §9.1.2, §9.1.3 | Kayıt olan | Doğrulama bağlantısı (Z-1); yeniden istenen bağlantı öncekileri geçersiz kılar; kaydı kendisi yapmadıysa bağlantıya tıklamaması ve kaydın Z-1 dolunca silineceği (K-696) | E-posta · kaydın adresi |
| 7.1.50 | Şifre sıfırlama bağlantısı | Şifre sıfırlama talebi — Google ile açılmış hesapta şifre belirleme ve e-posta değişikliğinin geri alınması dahil | §9.2.4, §9.2.8, §9.3.6, §6.1.3.3, §6.2.5.2, §10.1.3.2, §10.4.1 | Hesap sahibi | Tek kullanımlık sıfırlama bağlantısı (Z-2) | E-posta · hesabın adresi — geri almada eski adres |
| 7.1.51 | Yeni e-posta adresinin doğrulanması | Hesabın e-posta adresi değiştirilir — müşteri ve yönetici hesabı | §9.3.4, §9.3.7, §6.2.5.1 | Hesap sahibi | Yeni adresin doğrulama bağlantısı | E-posta · yeni adres |
| 7.1.52 | E-posta değişikliğinin eski adrese bildirimi | Değişiklik yeni adres doğrulanınca geçerli olur | §9.3.5, §9.3.7, §6.2.5.1 | Hesap sahibi — eski adres | Adresin değiştirildiği ve tarihi; "bu değişikliği ben yapmadım" bağlantısı (Z-45). Yeni adresi taşımaz | E-posta · eski adres |
| 7.1.53 | Yönetici daveti | Yönetici bir adrese davet gönderir — yeniden göndermek yeni davettir | §8.8.1, §10.4.3, §10.4.6 | Davetli | Tek kullanımlık davet bağlantısı (Z-3) | E-posta · davetlinin adresi |
| 7.1.54 | Hesabın silindiği bildirimi | Müşteri hesabı silinir — üyenin kendi silmesi ya da yöneticinin talep üzerine silmesi | §9.4.2, §9.4.6 | Hesap sahibi — silinen hesabın adresi | Silmenin tarihi ve silinenler; siparişlerin yasal saklama süresince sürdüğü ve açık siparişlere sipariş e-postasındaki bağlantıyla ya da sipariş numarası ve e-postayla girileceği. Bağlantı ve geri alma yolu taşımaz (K-693) | E-posta · silinen hesabın adresi |

### 7.2 Bildirim üretmeyen olaylar ve olmayan bildirimler

Kayıt ya da durum değiştiren ama bildirim üretmeyen her olay kaynağıyla buradadır; akışın bildirim hücresi "— (`02 §9.2`)" ya da benzeri bir kaynakla bu satırlara işaret eder (§0.4.1; K-682). Panelin ve vitrinin ekranda söylediği mesajlar — sepetten çıkan kalem, limit mesajı — bildirim değildir.

| # | Olay | Akış | Kaynak ve gerekçe |
|---|---|---|---|
| 7.2.1 | Hazırlanıyor'a geçiş (S1) | §2.5.3.1, §8.2.3 | Ödeme onayının e-postası (B-4) gitmiştir — `02 §9.2` |
| 7.2.2 | Teslim işareti — fiziksel kalemde teslim tarihi (S6), hizmette "tamamlandı" işareti; dijital kalemin teslimi B-4'ün içindedir | §8.2.6, §8.2.9, §2.5.3.5, §2.5.3.6 | Tarih çoğu zaman geriye dönük girilir — `02 §9.2` |
| 7.2.3 | Teslim tarihinin ve iade malının ulaşma tarihinin düzeltilmesi (K-712) · iç not | §8.4.4, §8.4.6 | `02 §9.2` · K-712 |
| 7.2.4 | İade malının teslim alınması — IBAN isteğinin düştüğü hâl (7.1.29) dışında | §8.3.2.1, §2.8.1.5 | `02 §9.2` |
| 7.2.5 | Malı dönmemiş cayma kaleminin "mal dönmedi" gerekçesiyle kapatılması | §8.4.7, §2.8.1.8 | Kapatma müşterinin hakkını değiştirmez — `02 §9.2` |
| 7.2.6 | Havale hattında IBAN'ın silinmesi — kapatmayla ya da Z-38 dolunca | §4.2.8, §8.4.7 | `02 §9.2` |
| 7.2.7 | Ayıp talebinin "Çözüldü" işareti ve yöneticinin talebi yeniden açması | §8.4.11, §2.9.5 | Çözümü firma kendi cevabıyla yazmıştır — `02 §9.2` |
| 7.2.8 | Kart iadesinin sağlayıcıda başarısız olması | §8.3.3.4, §2.8.4.3 | Müşteri iadeyi B-8'le, havale yolunu B-14'le öğrenir; firma panel işaretini ve uyarıyı görür — `02 §9.2` · K-683 |
| 7.2.9 | Kart son sorgusunun yanıtsız kalması | §2.5.1.4, §4.1.7 | Sonuç gelince B-4 ya da B-7 gider — `02 §9.2` · K-683 |
| 7.2.10 | İndirme hakkının yenilenmesi | §8.2.10, §2.5.3.2 | Başvurunun cevabı firmanın e-postasıdır — `02 §9.2` · K-683 |
| 7.2.11 | Dijital dosyanın güncellenmesi | §8.1.6 | Yeni hâl sipariş sayfasından iner — `02 §9.2` · K-683 |
| 7.2.12 | Doğrulanmamış kaydın Z-1 dolunca silinmesi | §9.1.5, §4.1.1 | Adres doğrulanmamıştır — `02 §9.4` · K-683 |
| 7.2.13 | Açık kalemi kalmamış siparişin cayma beyanı, gecikme feshi ya da çıkarmayla İptal edildi'ye kapanışı (S3, S4, S9 kapanışı) | §1.4.3, §8.3.2.5, §2.7.8, §2.10.3 | B-7 gitmez — müşteri olayın kendi bildirimini (B-9, B-11) almıştır — `02 §9.2` B-7 · K-672 |
| 7.2.14 | Havale ödeme süresinin kesintide dolması ve iptalin ertelenmesi | §4.2.12, §3.5.2.17 | Sipariş sayfası yeni son günü gösterir; ayrı bildirim gitmez — `02 §4.2` Z-8 · K-667 |
| 7.2.15 | Sürelerin aşılması ve dolması — kargoya verme süresi (Z-10), ifa süresi (Z-39), yasal teslim sınırı (Z-11), cayma penceresi, ayıp süresi, Z-42; bekleyen işler | §4.1, §8.9.1 | Süre dolmadan giden tek uyarı B-3'tür; süre dolunca işlem başlatan hâllerde işlemin bildirimi gider (7.1.15). Bekleyen işler e-postayla hatırlatılmaz, panel sayaçlarla gösterir — `02 §9` (matriste süre aşımının bildirimi yoktur; tek uyarı B-3), `02 §4.2`, §9.3.2, §10.6.1 |
| 7.2.16 | Firmanın panelde kendi yaptığı işlem — havale onayı, kargoya verme, teslim işareti, hizmet tamamlama, firma iptali, başka kanaldan cayma ya da gecikme feshi kaydı, müdahaleler, katalog, içerik ve ayar değişiklikleri, satışın kapatılması | §8 | Firmaya bildirim gitmez; panelde görünür — `02 §9.3.2`. İstisnalar F-5 ve F-6'dır (7.1.46–7.1.48) |
| 7.2.17 | Müşterinin kendi iptali — firmaya | §2.7 | Havale geri ödemesi sayaca düşer, kart iadesi F-4'le bildirilir — `02 §9.5` |
| 7.2.18 | İletişim talebinin kapatılması ve yeniden açılması | §8.6.4.4, §2.2.9 | Talep müşteriye görünmez ve takip edilmez — `02 §3.32.6` |
| 7.2.19 | İletişim formunu gönderene alındı e-postası | §2.2.8 | Formun e-postası doğrulanmaz — `02 §9.5` · K-678 |
| 7.2.20 | Davetin geri çekilmesi — davetliye · davetin süresinin dolması | §8.8.3, §8.8.4 | Bağlantı zaten geçersizdir; yabancıya firma hakkında bilgi verilmez — `02 §10.2.6` · K-660. Davetin gönderilmesi F-6'yı üretir (7.1.47) |
| 7.2.21 | Duyuru | §8.6.3.1 | Sitede durur, kimseye iletilmez — `02 §9.5` |
| 7.2.22 | Yasal metnin değişmesi | §8.7.2.2, §8.7.2.3 | Güncel metin altbilgidedir; yürüyen sipariş donmuş sürümüne tabidir — `02 §9.5`, §3.33.4 |
| 7.2.23 | Pazarlama iletisi, terk edilmiş sepet hatırlatması ve "stokta haber ver" | §2.1.7, §2.3 | Ticari elektronik iletidir; ürün İYS'ye bağlanmaz — `02 §9.5` |
| 7.2.24 | Veri ihlalinde etkilenen kişilere toplu bildirim | §6.3.3.2 | Firma kendi e-posta aracıyla ulaşır — `02 §9.5` |
| 7.2.25 | Bildirim tercihi | §9.5.1, §9.5.6 | Yoktur: işlem bildirimleri kapatılamaz — `02 §9.1.3` |
| 7.2.26 | Hesap olayları — hesabın doğrulanması, Google ile hesabın açılması ve Google girişinin bağlanması, giriş, çıkış ve oturumun kendiliğinden kapanması, şifrenin değiştirilmesi ve sıfırlama bağlantısıyla yenilenmesi, adın ve adres defterinin değişmesi, davetlinin yönetici hesabını açması | §9.1.4, §9.1.6, §9.2.1–§9.2.3, §9.2.5, §9.2.7, §9.3.1–§9.3.3, §8.8.2, §3.4.16 | Kullanıcının kendi ekranında tamamladığı işlemdir; kurtarma yolu e-postadır ve adresi değiştiren işlem eski adrese bildirilir; davet F-6 ile bildirilmiştir — `02 §9.4` · K-694 |
| 7.2.27 | Kurulum tarafının panele erişimi kurtarması — yeni yönetici hesabının açılması, ele geçirilmiş hesabın oturumlarının sonlandırılması | §10.4.4, §10.4.5 | Panel işlemi değildir: işlem izine yazılmaz ve bildirim üretmez; yeni yöneticinin paneldeki kaldırması F-6'yı üretir (7.1.48) — `02 §10.2.7` · K-699 |
| 7.2.28 | Dış servisin ya da sitenin kesintisi ve normale dönüş | §10.1.1.6, §10.3.6 | Ürün kesinti bildirimi göndermez — kesinti bildirimi `02 §9`'un matrisinde yoktur; firmanın yolu duyuru şerididir (7.2.21) ve duyuru şeridi gönderilmez (`02 §9.5`) |
| 7.2.29 | Periyodik imha ve kişisel veriyi hemen silen işlemlerin imha kaydı; e-postanın yeniden denenmesi | §4.2.9, §4.2.10 | Sistemin iç işidir; ulaşmayan e-postada panel işareti düşer — `02 §12.2.7`, §9.1.6 · K-708 |
| 7.2.30 | Müşterinin geri ödeme IBAN'ını girmesi ve düzeltmesi | §2.7.5 | Sipariş sayfasının işlemidir; sayfaya erişimin kalan riski `02 §3.22.5`'tedir — `02 §9.2` · K-710 |

### 7.3 Geçiş ve olay bazlı eşleme

Her durum geçişinin ve kalem, talep, panel ya da hesap olayının hangi bildirimi ürettiği (şablon §0; plan AK7-03). "Harita" sütunu §7.1 ya da §7.2'nin satırıdır; durum geçişlerinin tetikleyicisi ve akışı §1.2 ve §1.3'tedir.

| # | Geçiş ya da olay | Bildirim | Harita |
|---|---|---|---|
| | **Sevkiyat ekseni** (§1.2) | | |
| 7.3.1 | S1 Alındı → Hazırlanıyor | Ayrıca yok — Ö1'in B-4'ü | 7.2.1 |
| 7.3.2 | S2 Alındı → Teslim edildi | Yalnız dijital siparişte ödeme onayında B-4; son teslim işaretinde yok; kapanışta olayın kendi bildirimi | 7.1.7, 7.1.8, 7.2.2 · kapanışta 7.1.13, 7.1.14, 7.1.20, 7.1.21, 7.1.26 |
| 7.3.3 | S3 Alındı → İptal edildi | B-7; kapanışta yok — olayın B-9'u ya da B-11'i | 7.1.12–7.1.15, 7.2.13 |
| 7.3.4 | S4 Hazırlanıyor → İptal edildi | B-7; kapanışta yok | 7.1.13, 7.1.14, 7.2.13 |
| 7.3.5 | S5 Hazırlanıyor → Kargoya verildi | B-5 | 7.1.9 |
| 7.3.6 | S6 Kargoya verildi → Teslim edildi | Yok | 7.2.2 |
| 7.3.7 | S7 Kargoya verildi → Teslim edilemedi | B-6 | 7.1.11 |
| 7.3.8 | S8 Teslim edilemedi → Kargoya verildi | B-5, yeniden | 7.1.10 |
| 7.3.9 | S9 Teslim edilemedi → İptal edildi | B-7; kapanışta yok | 7.1.14, 7.2.13 |
| 7.3.10 | S10 Hazırlanıyor → Teslim edildi | Olayın kendi bildirimi — B-7, B-9, B-11 ya da hizmet tamamlamada yok | 7.1.13, 7.1.14, 7.1.20, 7.1.22, 7.1.26, 7.2.2 |
| 7.3.11 | S11 Teslim edilemedi → Teslim edildi | Olayın kendi bildirimi — S7'de B-6 | 7.1.11, 7.1.20 |
| | **Ödeme ekseni** (§1.3) | | |
| 7.3.12 | Ö1 Bekliyor → Ödendi | B-4 | 7.1.7, 7.1.8 |
| 7.3.13 | Ö2 Bekliyor → Başarısız | S3'ün B-7'si | 7.1.12, 7.1.14, 7.1.15 |
| 7.3.14 | Ö3–Ö5 geri ödeme geçişleri | B-8; sistemin kart iadesinde ayrıca F-4 | 7.1.16–7.1.18, 7.1.44 |
| 7.3.15 | İptal edilmiş siparişe gelen kart ödemesinin geri ödenmesi — eksen değişmez | B-8 · F-4 | 7.1.19, 7.1.45 |
| 7.3.16 | Kart iadesinin başarısız olması — eksen değişmez | Yok | 7.2.8 |
| | **Sipariş oluşumu ve kendiliğinden işler** (§4.2) | | |
| 7.3.17 | Sipariş oluşur | B-1 · havalede B-2 · F-1 · aynı sepetin önceki siparişinde B-7 | 7.1.1, 7.1.3, 7.1.38, 7.1.15 |
| 7.3.18 | Havale hatırlatması (Z-9) | B-3 | 7.1.4–7.1.6 |
| 7.3.19 | Kart son sorgusu yanıtsız kalır · havale süresi kesintide dolar | Yok | 7.2.9, 7.2.14 |
| 7.3.20 | IBAN'ın silinmesi · periyodik imha · e-postanın yeniden denenmesi · doğrulanmamış kaydın silinmesi | Yok — e-posta ulaşmazsa panel işareti | 7.2.6, 7.2.12, 7.2.29 |
| | **Kalem kayıtları** (§1.7.1) | | |
| 7.3.21 | İptal kaydı | B-7 · kart hattında ödenmiş kalemde B-8 ve F-4 · havale hattında firma iptalinde B-14 | 7.1.13, 7.1.14, 7.1.16, 7.1.34, 7.1.44 |
| 7.3.22 | Çıkarma kaydı | B-11 · kartta B-8 ve F-4 · havale hattında B-14 | 7.1.16, 7.1.26, 7.1.34, 7.1.44 |
| 7.3.23 | Gecikme feshi | B-9 · müşterinin feshinde F-3 · kartta B-8 ve F-4 · havale hattında IBAN'sız kayıtta B-14 | 7.1.16, 7.1.22, 7.1.32, 7.1.41, 7.1.44 |
| 7.3.24 | Cayma beyanı | B-9; müşterinin beyanında F-3; IBAN'sız kayıtta B-14 | 7.1.20, 7.1.21, 7.1.40, 7.1.32 |
| 7.3.25 | İade teslim alma | Yok; IBAN silinmişse B-14 | 7.2.4, 7.1.29 |
| 7.3.26 | İade reddi | B-16 | 7.1.36 |
| 7.3.27 | "Mal dönmedi" kapanışı | Yok | 7.2.5 |
| 7.3.28 | IBAN isteği | B-14 | 7.1.29–7.1.34 |
| 7.3.29 | Ayıp talebi — doğuş ve müşterinin yeniden açması | B-9 · F-3 | 7.1.23, 7.1.24, 7.1.42, 7.1.43 |
| 7.3.30 | Ayıp talebi — "Çözüldü" işareti ve yöneticinin yeniden açması | Yok | 7.2.7 |
| 7.3.31 | İndirme sayacı — yenileme · dosya güncellemesi | Yok | 7.2.10, 7.2.11 |
| | **Müdahaleler** (§8.4) | | |
| 7.3.32 | Adres düzeltmesi | B-10 | 7.1.25 |
| 7.3.33 | Takip bilgisinin düzeltilmesi | B-12 | 7.1.27 |
| 7.3.34 | Teslim tarihinin ve iade malının ulaşma tarihinin düzeltilmesi · iç not | Yok | 7.2.3 |
| 7.3.35 | Durum geçişinin düzeltilmesi | B-13 · havale işaretinde ayrıca B-3 | 7.1.28, 7.1.5 |
| 7.3.36 | Misafir siparişinin e-posta düzeltmesi | B-1 → yeni adres · B-15 → eski adres | 7.1.2, 7.1.35 |
| | **Talepler, panel ve hesaplar** | | |
| 7.3.37 | İletişim talebi doğar | F-2 | 7.1.39 |
| 7.3.38 | İletişim talebi kapatılır ya da yeniden açılır | Yok | 7.2.18 |
| 7.3.39 | Havale IBAN'ının girilmesi ya da değişmesi | F-5 | 7.1.46 |
| 7.3.40 | Yönetici daveti gönderilir · bir yönetici kaldırılır | F-6 · davette davet e-postası | 7.1.47, 7.1.48, 7.1.53 |
| 7.3.41 | Davet geri çekilir ya da süresi dolar | Yok | 7.2.20 |
| 7.3.42 | Katalog, içerik, marka, ayar ve yayın durumu değişiklikleri · satışın kapatılması · duyuru | Yok | 7.2.16, 7.2.21, 7.2.22 |
| 7.3.43 | Hesap kaydı, şifre sıfırlama ve e-posta değişikliği | Hesap ve yönetim e-postaları | 7.1.49–7.1.52 |
| 7.3.44 | Ulaşmayan e-postanın yeniden gönderilmesi | İlk bildirim, yeniden | 7.1.37 |
| 7.3.45 | Hesabın silinmesi — üyenin ya da yöneticinin | Hesabın silindiği bildirimi | 7.1.54 |
| 7.3.46 | Hesabın doğrulanması, Google ile açılması ve bağlanması, giriş ve çıkış, şifrenin değiştirilmesi, ad ve adres defteri, davetlinin hesabı açması | Yok | 7.2.26 |
| 7.3.47 | Kurulum tarafının kurtarması · dış servisin ya da sitenin kesintisi | Yok | 7.2.27, 7.2.28 |

## 8. Yönetim (admin) akışları

> **Ne yazılır:** Kullanıcı akışlarıyla **aynı derinlikte**. Yönetim akışları genellikle beklenenden karmaşıktır.

Bu bölüm firma tarafının akışlarını yazar: katalog yönetimi (akış 5), sipariş yürütümü ve müdahaleler (akış 6), kurumsal içeriğin panel yüzü (akış 7) ve mağaza ayarları ile yönetici hesapları (akış 8) — `02 §2.1` (K-650). Yönetici müdahaleleri akış 6'nın parçasıdır (§8.4; K-468). Üye kaydı görünümü ve hesabın talep üzerine silinmesi §9.4.5 ve §9.4.6'dadır.

- **Biçim ve aktör:** adım tablosu §0.4.1'dir; aktör Yönetici'dir, sistemin kendiliğinden yaptığı adımlar Sistem'dir. Panel tek roldür: her işlemi her yönetici yapar ve her yönetici bütün müşteri verisini görür (`02 §10.2.3`). Panelde hiçbir işlev mobilde kapatılmaz (`02 §10.1.4`). Bir işlemin açık olduğu aralık §1.11'de, geri alınamaz onaylar §1.6.1'dedir ve burada tekrarlanmaz.
- **Bildirim:** sütun müşteriye ve yöneticilere gideni yazar. **Firmanın panelde kendi yaptığı işlem firmaya bildirim üretmez** — kargoya verme, havale onayı, hizmet tamamlama, firma iptali panelde görünür (`02 §9.3.2`); istisnalar F-5 ve F-6'dır (`02 §9.3.3`, §9.3.4). Bildirimlerin özeti §7'dedir.
- **İz:** "iz" işlem izinin sekiz alanından birine yazılan işlemdir (`02 §10.3.1`); fiyat düzenlemesi, stok girişi ve içeriğin metin düzenlemesi ize yazılmaz.
- **Elle adım:** sipariş başına zorunlu ve koşullu elle adımlar §8.5'te sayılır; bir adımın bütçeye girip girmediği orada yazar (§0.4.5).
- **Dallar:** hataların tamamı §3.5'te — siparişe dokunanlar §3.2 ve §3.3'te de — bu bölümün satırlarına bağlanır (§0.4.2); süreler §4'tedir.

### 8.1 Katalog yönetimi (akış 5)

Ürün, varyant, stok, fiyat, indirim, kupon ve kategori panelden tek tek yönetilir (`02 §2.1`, §3.3–§3.12). Yayın durumları §1.8'de, indirim ve kuponun geçerliliği §1.10'dadır. Katalog değişikliği verilmiş siparişe dokunmaz: kalem kendi donmuş değerlerini taşır (`02 §3.23.1`).

| # | Aktör ne yapar | Sistem ne denetler, ne yapar | Durum | Bildirim | Kaynak |
|---|---|---|---|---|---|
| 8.1.1 | Yönetici ürün oluşturur ya da düzenler — tipini seçer, adını yazar, ürünü kategorilere asar ve birini ana kategori yapar | Tip üç değerli kapalı bir alandır ve ürün düzleminde yaşar; varyantlar devralır. Ad P-27 ile sınırlıdır. Yeni ürün Taslak doğar. Var olan ürüne ürün listesinden varılır: liste ürün adıyla aranır ve yayın durumuna göre süzülür (K-737). İki yönetici aynı kaydı aynı anda düzenlerse ikinci kaydeden kaydın değiştiğini görür ve üzerine yazmadan önce uyarılır | Ürün: Taslak | — | `02 §3.2.2`, §3.2.3, §3.4.4, §3.11.5, §3.31.1, §10.1.2 · K-737 |
| 8.1.2 | Yönetici varyantları tanımlar — en fazla iki seçenek boyutuyla (P-21), yalnız gerçekten var olan kombinasyonlar | Her varyant tekil bir stok kodu (`SKU`) ve KDV dahil, iki ondalıklı, sıfırdan büyük bir fiyat taşır; KDV oranı ürün düzlemindedir, varsayılanı P-10'dur. Açılmamış kombinasyon vitrinde görünür ama seçilemez. Varyant kendi yayın durumunu taşır | Varyant: Taslak | — | `02 §3.3.2`–§3.3.4, §3.8.1–§3.8.4, §3.7.1 |
| 8.1.3 | Yönetici stoğu ya da hizmet kontenjanını girer | Fiziksel üründe sayısal stok zorunludur, dijital üründe stok alanı yoktur, hizmette kontenjan isteğe bağlıdır. Stok alanının yanında ödemesi beklenen siparişlerin ayırdığı adet ve onu tutan siparişler görünür; ayrılmış adedin altına inen değer sebebiyle reddedilir — mal gerçekten yoksa yönetici ayırmayı tutan siparişi firma iptaliyle kapatır (8.3.1.1). Artırmak ve kontenjanı boşaltmak serbesttir; stok girişi ize yazılmaz | — | — | `02 §3.6.1`, §3.6.2, §3.6.7, §10.3.1 · K-653 |
| 8.1.4 | Yönetici tipe özgü alanları girer | Fiziksel üründe üretim yeri zorunludur; ölçü birimi ve net miktar isteğe bağlıdır ve panel ölçüyle satan firmanın yükümlülüğünü hatırlatır. Ürünün kargoya verme süresi (P-6) P-5'in üst çitini aşamaz. Cayma istisnası işareti ve kapalı listeden sebebi — mutlak ya da koşullu — ürün düzlemindedir. Hizmette ifa süresi (P-44) zorunludur. Ürün başına "bir siparişte en fazla" adet (P-9) isteğe bağlıdır. Bu alanların değişikliği yalnız yeni siparişlere işler | — | — | `02 §3.8.5`, §3.20.5, §7.3.4, §3.2.2, §3.16.9, §3.23.1 |
| 8.1.5 | Yönetici görselleri yükler, sıralar, ana görseli işaretler ve açıklamayı yazar | Görsel biçimi, boyutu ve adedi P-22, P-23 ve P-24 ile sınırlıdır; varyant kendi görselini taşımıyorsa ürününkini devralır. Alternatif metin isteğe bağlıdır ve boşsa sistem üretir. Açıklama kapalı bir metin biçimi setini kullanır; garanti bilgisinin ayrı alanı yoktur | — | — | `02 §3.11` |
| 8.1.6 | Yönetici dijital ürünün dosyasını yükler ya da günceller | Ürün ya da varyant düzleminde tek dosya vardır (P-26). Güncellenen dosya geçmiş alıcılara da sipariş sayfasından iner ve indirme hakkından düşer. Yayındaki bir varyantı dosyasız bırakacak silme engellenir | — | Dosya güncellemesi — (`02 §9.2`) | `02 §3.12.2`, §3.12.3, §3.12.7, §3.12.8, §3.7.4 · K-683 |
| 8.1.7 | Yönetici taslak ürünü sitede "Taslak" bandıyla önizler ve yayına alır | Yayın kapısı: ad, ana kategori, tip ve en az bir Yayında varyant; dijitalde her yayındaki varyantın dosyası, hizmette ifa süresi. Eksik koşul panelde gösterilir. Zamanlanmış yayın yoktur; ürünün adresi ilk yayında sabitlenir. Yayın durumu değişikliği ize yazılır | Ürün: Taslak → Yayında | — | `02 §3.7.3`–§3.7.5, §3.30.2, §10.3.1 |
| 8.1.8 | Yönetici ürünü ya da varyantı taslağa ya da arşive alır, yeniden yayına alır | Geçişler serbesttir; yayındaki ürünün son Yayında varyantının arşive ya da taslağa alınması engellenir (§1.8.2). Vitrinden kalkan kalem sepetlerden çıkar (§2.3.5); arşivlenen ürünün adresi "artık satılmıyor" sayfasını döner. İze yazılır | §1.8.1'in geçişleri | — | `02 §3.7.1`, §3.7.6, §3.7.8, §3.16.7 |
| 8.1.9 | Yönetici ürünü ya da varyantı kalıcı olarak siler | Onaydan önce panel silinen kayda bağlı açık siparişlerin sayısını söyler; silme engellenmez ve açık sipariş donmuş kalemiyle sürer, silinen varyantın ayırması düşer. Yayındaki ürünün son Yayında varyantı silinemez. Dijital dosyanın son hâli geçmiş siparişler için saklanır; adres serbest kalır ve ürün içerik bağlarından kalkar | Kayıt katalogdan kalkar; siparişlerin durumları değişmez | — | `02 §3.7.7`, §3.7.8, §3.12.7, §3.27.10, §3.30.2 · K-655, K-674 |
| 8.1.10 | Yönetici varyantın fiyatını değiştirir | Fiyat geçmişe yazılır — referans fiyat buradan hesaplanır. Sepette "fiyatı değişti" satırı görünür; onay anına denk gelen değişiklik özet farkıdır (§2.4.8). Fiyat düzenlemesi ize yazılmaz | — | — | `02 §3.8`, §3.9.3, §3.16.5, §3.17.2 |
| 8.1.11 | Yönetici ürüne tarihli, yüzde olarak indirim tanımlar | İndirim bütün varyantlara iner; referans fiyatı sistem hesaplar (Z-21). İndirimli fiyat referansın altına inmiyorsa, %100 girildiyse ya da bir varyantta 0,00 TL'ye iniyorsa kaydedilmez ve panel sebebini söyler. Sistem indirimi başlangıçta başlatır, bitişte bitirir (Z-20) | — | — | `02 §3.9`, §5.11.1 |
| 8.1.12 | Yönetici kupon tanımlar | Kupon yüzde ya da sabit tutardır; sabit tutarlı kuponun asgari sepet tutarı kupon tutarından büyüktür, yüzdesel kupon %100'e ulaşamaz. Sınırları tarih aralığı (Z-22) ve toplam kullanım adedidir; kişiye özel kod adedi 1 olan koddur. Kod kuponlar arasında tekildir ve büyük-küçük harf ayrımı olmadan eşleşir; kupon silinmez — durdurmanın yolu 8.1.13'tür — ve alanlarının değişikliği yalnız yeni siparişlere işler (K-806) | — | — | `02 §3.10.2`, §3.10.4, §3.10.5 · K-806 |
| 8.1.13 | Yönetici kuponun kullanım adedini değiştirir ya da kampanyayı durdurur | Panel kullanılmış ve ayrılmış hakları gösterir; adet bu toplamın altına indirilemez. Kampanyayı durdurmanın yolu bitiş tarihini öne çekmektir: yeni siparişte kupon kabul edilmez, ayrılmış haklar onaylanmış siparişlerde kalır | — | — | `02 §3.10.6` · K-654 |
| 8.1.14 | Yönetici kategori ağacını kurar, kategoriyi taşır ya da siler | Ağaç en fazla üç seviyedir (P-20); aynı seviyedeki kategoriler adlarının alfabetik sırasıyla dizilir ve elle sıralanmaz (K-805); taşıma üç seviyeyi aşan dal üretemez ve kategori kendi alt ağacına taşınamaz. İçinde ürün ya da alt kategori bulunan kategori silinemez — engelleyenler sayısıyla gösterilir. Menüdeki görünürlük §2.1.2'dedir | — | — | `02 §3.4`, §3.28.6 · K-805 |
| 8.1.15 | Yönetici ürünleri topluca yüklemek ya da sıralamak ister | Yoktur: CSV ile içe aktarma, liste ekranından toplu fiyat ve stok güncellemesi ve ürünlerin elle sıralanması bulunmaz; ürünler tek tek eklenir ve güncellenir | — | — | `02 §10.7.2`, §3.5.3 |

### 8.2 Sipariş yürütümü: olağan hat (akış 6)

Siparişin vitrinden teslimata yolculuğunun firma tarafı (`02 §2.2` adım 6–8). Tipe göre hatlar §1.5'te, adım sayıları §8.5'tedir. Müşterinin aynı adımlardaki yüzü §2.5'tedir.

| # | Aktör ne yapar | Sistem ne denetler, ne yapar | Durum | Bildirim | Kaynak |
|---|---|---|---|---|---|
| 8.2.1 | Yönetici yeni siparişi öğrenir | Firmanın iletişim e-postasına bildirim gider; sipariş panelin sipariş listesindedir, havale siparişi "ödeme onayı bekleyen" sayacındadır | Alındı + Bekliyor | F-1 → firma (§2.4.9) | `02 §9.3`, §10.6.1 |
| 8.2.2 | Yönetici havale parasını hesabında görür ve "ödendi" işaretler — zorunlu elle adım | Panel siparişin donmuş IBAN'ını ve tutarını gösterir; gelen tutar sorulmaz — eksik ya da fazla gelen havalede kararı firma sistemin dışında verir. Onay geri alınamaz ve dijital kalemli siparişte işaretin düzeltilemeyeceğini önceden söyler (§1.6.1.1). İptal edilmiş sipariş işaretlenemez; sitenin kesintisinde ertelenen iptalde ertelenen ana kadar işaretlenir (§4.2.12). İze yazılır | → Ödendi (Ö1) ve eşzamanlı geçişler (§1.4.2.1) | B-4 → müşteri | `02 §3.21.5`–§3.21.8, §5.9, §10.3.1 · K-663, K-667 |
| 8.2.3 | Sistem ödeme onayını işler — kartta sağlayıcının bildirimiyle, havalede 8.2.2 ile | Ayrılan kesin düşer; kargoya verme süresi (Z-10) ve hizmetin ifa süresi (Z-39) başlar; dijital kalem teslim edilir. Fiziksel kalemli sipariş "kargoya verilecek", hizmet kalemi "tamamlanmayı bekleyen hizmet" sayacına girer | §2.5.3.1'in geçişleri | B-4 → müşteri (§2.5.3.1) | `02 §4.3`, §10.6.1 |
| 8.2.4 | Yönetici siparişi hazırlar ve faturayı kendi aracıyla keser | Hazırlık ve fatura sistemin dışındadır; panel kargoya verme sözünü ve kalan süreyi gösterir. Z-10 aşılırsa kendiliğinden işlem başlamaz: müşterinin iptal düğmesi açıktır ve süre aşıldıktan sonraki iptalde panel kanuni faiz uyarısını gösterir. Faturanın verisi sipariş dışa aktarmasındadır (8.9.5) | Hazırlanıyor + Ödendi | — | `02 §3.20.4`, §3.25, §7.2.3, §10.5.3 |
| 8.2.5 | Yönetici siparişi kargoya verir — kargo şirketi listeden (listede yoksa "Diğer" ve adı) ve takip numarasıyla ya da "kendi aracımızla teslim" beyanıyla; zorunlu elle adım | İkisinden biri olmadan işaret konmaz. Onay geri alınamaz ve sonucunu tek cümleyle söyler. Yalnız açık fiziksel kalemler gider — iptal edilmiş, çıkarılmış, feshedilmiş ve cayma beyanıyla kapanmış kalem gitmez; sipariş tek parça ve tek takip numarasıyla gider. İze yazılır | → Kargoya verildi (S5) | B-5 → müşteri | `02 §3.20.3`, §3.20.7, §3.20.8, §5.9 |
| 8.2.6 | Yönetici teslim işaretini ve teslim tarihini girer — zorunlu elle adım | Tarih geçmişe dönük olabilir; ileri tarihli ve siparişin kargoya verildiği günden önceki tarih reddedilir. Tarih sipariş düzeyindedir ve fiziksel kalemlerin tamamına yazılır. Tarih bir cayma beyanının günüyle çakışırsa panel teslimin beyandan önce mi sonra mı olduğunu sorar (§2.8.1.3). Kendiliğinden geçiş yoktur; sipariş işaretlenene kadar "teslim işareti bekleyen" sayacındadır. İze yazılır | → Teslim edildi (S6) · Z-13 ve Z-18 başlar | — (`02 §9.2`) | `02 §3.20.11`, §7.4.1, §10.6.1 |
| 8.2.7 | Yönetici geri dönen gönderiyi "teslim edilemedi" işaretler | Sipariş yeniden "kargoya verilecek" sayacına girer; yönetici yeniden gönderir (8.2.8) ya da iptal eder (8.3.1.1). Cayma beyanlı ve feshedilmiş kalemin dönen malı teslim alma adımıyla işlenir (8.3.2.1); açık kalem kalmadıysa kapanış 8.3.2.5'tedir. İze yazılır | → Teslim edilemedi (S7) · S7 kapanışı tamamlıyorsa → Teslim edildi (S11) | B-6 → müşteri | `02 §5.4`, §5.8, §10.6.1 |
| 8.2.8 | Yönetici açık fiziksel kalemleri yeniden gönderir — yeni takip bilgisiyle | Onay geri alınamaz (K-677). Cayma beyanlı ve feshedilmiş kalem yeniden gönderilmez. İze yazılır | → Kargoya verildi (S8) | B-5 → müşteri, yeniden | `02 §5.4`, §5.9 · K-677 |
| 8.2.9 | Yönetici hizmet kalemini "tamamlandı" işaretler — zorunlu elle adım | İşaret ödeme onaylanmışken — Bekliyor ve Başarısız dışında — sunulur (K-671); tek işlemdir, randevu ve takvim yoktur ve ifanın başladığı an izlenmez. Hizmetten cayma hakkı düşer. Z-39 aşılırsa kendiliğinden işlem başlamaz. İze yazılır | kalem: teslim işareti · son açık kalemse kapanış kuralı (§1.4.3) — S2, S10, S11 | — (`02 §9.2`) | `02 §3.20.9`, §7.3.3, §5.4 · K-671 |
| 8.2.10 | Yönetici müşterinin başvurusu üzerine dijital kalemin indirme hakkını yeniler | Başvuru iletişim formundan "Sipariş hakkında" tipiyle gelir (8.6.4.2); yönetici paneldeki siparişte kalemin hakkını yeniler ve sayaç sıfırlanır. Müşteri dosyayı sipariş sayfasından indirir | kalem: indirme sayacı sıfırlanır | — (`02 §9.2`) | `02 §3.12.6`, §10.5.2 · K-683 |
| 8.2.11 | Yönetici siparişi bulur — sipariş numarasıyla ya da siparişin iletişim e-postasıyla arar, durumuna göre süzer | Teyit akışları (§6.2.1), iletişim talebindeki numara (8.6.4.2) ve telefonla ulaşan müşteri bu yoldan siparişe varır; ekranın biçimi `04`'ün işidir | — | — | `02 §10.1.2` · K-714 |

### 8.3 Firma iptali, iade teslim alma ve geri ödeme

Akış 6'nın iptal, iade ve geri ödeme kolları (`02 §2.1`). Müşterinin aynı süreçlerdeki yüzü §2.7 ve §2.8'de, ayıp talebinin para gerektiren çözümü §8.4'tedir.

#### 8.3.1 Firma iptali

| # | Aktör ne yapar | Sistem ne denetler, ne yapar | Durum | Bildirim | Kaynak |
|---|---|---|---|---|---|
| 8.3.1.1 | Yönetici siparişi ya da kalemi iptal eder ve kapalı listeden sebep seçer — stokta bulunamadı · ürün hasarlı ya da satılamaz durumda · teslimat adresine gönderim yapılamıyor · gönderi teslim edilemedi — müşteri kaynaklı · müşteriyle anlaşıldı (müşteri talebi) · diğer, açıklamayla | Ödeme beklenirken yalnız siparişin tamamı iptal edilir; ödemeden sonra kalem ve adet düzeyinde — yönetici iptal edilen adedi seçer (K-794) —, kalem tipinin iptal sınırı içinde; Teslim edilemedi'de sipariş S9 ile ya da geri dönen açık fiziksel kalemler kalem düzeyinde (1.11.21). Cayma beyanlı ve feshedilmiş kaleme — o adetlere — uygulanmaz. "Stokta bulunamadı"da panel onaydan önce stok yokluğunun yasal imkânsızlık sayılmadığını söyler; Z-10 ya da Z-39 aşıldıktan sonraki iptalde kanuni faiz uyarısı görünür. "Müşteriyle anlaşıldı" iptalinde talep siparişin e-postasından gelmediyse yönetici siparişin kanalına dönerek teyit eder (§6.2.1). Onay geri alınamaz. İptal edilen adet stoğa ve kontenjana anında döner, kupon hakkı yalnız siparişin tamamı iptal edilince. Ödenmiş kalemde geri ödeme 8.3.3'ün hattından, tutarı `02 §7.1.7`'yle (K-788), iptalden itibaren Z-17 içinde; havale hattında IBAN isteği iptal kaydıyla düşer (K-690). İze yazılır | Ödenmemişse → İptal edildi + Başarısız (S3, Ö2) · ödenmişse kalem: iptal kaydı · son açık kalemse kapanış kuralı (§1.4.3) — S2, S3, S4, S10 · Teslim edilemedi'de → İptal edildi (S9) ya da teslim edilmiş kalem varsa → Teslim edildi (S11) · havale hattında ödenmiş kalemde kalem: IBAN isteği | B-7 → müşteri — sebebiyle, kalemi ve adediyle, iptal anında · havale hattında ödenmiş kalemde B-14 → müşteri (K-690) | `02 §7.2.2`, §7.2.4, §7.2.6, §7.2.8, §7.2.9, §5.9 · K-690, K-788, K-794 |
| 8.3.1.2 | Yönetici gönderi müşteri kaynaklı döndüğü için siparişi ya da geri dönen kalemleri iptal eder — "gönderi teslim edilemedi — müşteri kaynaklı" | Ürün bedeli geri ödenir, gidiş kargo bedeli ödenmez; öteki sebeplerde gidiş kargosu da geri ödenir — ayrım sebep kaydından okunur | → İptal edildi (S9) — teslim edilmiş kalem varsa geri dönen kalemlerin iptaliyle → Teslim edildi (S11) (8.3.1.1) · Ö3–Ö5 geri ödemede | B-7 → müşteri (8.3.1.1) | `02 §7.2.9`, §5.4 |

#### 8.3.2 İade malının teslim alınması ve reddi

| # | Aktör ne yapar | Sistem ne denetler, ne yapar | Durum | Bildirim | Kaynak |
|---|---|---|---|---|---|
| 8.3.2.1 | Yönetici dönen malı teslim alır, ulaşma tarihini ve ulaşan adedi girer — cayma beyanlı ya da feshedilmiş kalemin malı, "mal dönmedi" ile kapatılmış kalemin sonradan gelen malı ve ayıp talebinde sözleşmeden dönmeyle geri alınan adetlerin malı da (`02 §7.5.3`; K-808) | Tarih geçmişe dönük olabilir; ileri tarihli ve kalemin teslim tarihinden önceki tarih reddedilir — teslim tarihi yoksa alt sınır uygulanmaz. Adedin varsayılanı beyanın — ya da feshin — henüz ulaşmamış adedinin tamamıdır, ayıpta talebin ayıplı adedinden henüz teslim alınmamış olandır; daha azı girilirse kalan adetler "iade malı bekleniyor"da kalır ve sonradan ayrı bir teslim almayla ya da 8.4.7'yle kapanır (K-794). "İade malı bekleniyor" ulaşan adetler için kalkar. Teslimden sonraki caymada geri ödemenin on dört günü (Z-16) ulaşma tarihinden — mal beyandan önce ulaştıysa beyandan — işler; ayıpta süre sayacı yoktur (`02 §12.5`; K-716); adım geri ödemeyi başlatmaz. Kapatılmış kalem yeniden açılır. Yanlış girilen ulaşma tarihi 8.4.4'ün müdahalesiyle düzeltilir (K-712); yanlış kalemde yapılmış teslim almanın geri alınması yoktur — sonucu firmadadır. Havale hattında IBAN silinmişse adımla IBAN isteği düşer. Beyandaki eski iade adresine gelen mal da teslim alınır. İze yazılır | kalem: iade teslim alma · IBAN silinmişse IBAN isteği | — (`02 §9.2`) · IBAN silinmişse B-14 → müşteri | `02 §7.4.1`, §7.4.5, §7.4.7, §7.5.3, §10.4.9, §3.1.7 · K-665, K-712, K-794, K-808 |
| 8.3.2.2 | Yönetici malı kontrol eder ve stoğa ekler — teslim alınan adet kadar (K-791) | Stok kendiliğinden dönmez; ekleme yöneticinin kontrolüne bağlıdır — varyant silinmişse stoğa ekleme sunulmaz (`02 §3.7.7`). Kullanılmış, hasarlı ya da eksik dönen mal da teslim alınır ve geri ödeme tam tutarla yapılır; değer kaybı talebi ürünün dışındadır ve yönetici durumu iç nota yazar (8.4.6; §5.3.2). Hizmet kontenjanı caymada kendiliğinden döner | — | — | `02 §7.4.7`, §7.4.8, §7.4.10 · K-668, K-791 |
| 8.3.2.3 | Yönetici koşullu istisna kaleminde koruyucu ambalajı açılmış malı reddeder — teslim alma adımında, teslim alınan adetle (K-794), "iade reddedildi — koruyucu ambalaj açılmış" | Ret yalnız koşullu istisna sebebi kaleme donmuş kalemde sunulur. Onay sonucunu tek cümleyle söyler ve geri alınmaz (§1.6.1.6). Kayıt ulaşma tarihini taşır. Geri ödeme yapılmaz ve süresi işlemez; kalem "iade ve geri ödeme bekleyen" sayacından düşer ve "iade malı bekleniyor" kalkar; havale hattında IBAN silinir; mal stoğa girmez; iptal ve iade oranı değişmez. Firma kararından dönerse tutar bazlı geri ödeme (8.3.3.7) işler, havalede IBAN 8.3.3.6 ile istenir. İze yazılır | kalem: iade reddi | B-16 → müşteri | `02 §7.4.9`, §10.6.1 · K-656, K-657, K-794 |
| 8.3.2.4 | Yönetici reddedilen malı müşterinin siparişteki teslimat adresine, kendi seçtiği taşıyıcıyla ve kendi bedeliyle geri gönderir | Sistemin dışındadır; ürün izlemez ve takip numarası istemez | — | — (B-16 gitmiştir) | `02 §7.4.9`, §10.5.2 · K-657 |
| 8.3.2.5 | Yönetici açık kalemi kalmamış ve gönderisi dönmüş siparişi kapatır — son kalem cayma beyanı, gecikme feshi, hizmet kaleminin iptali ya da çıkarılmasıyla kapanmışsa, S7'den sonra (K-717) | Kapanış firma iptali değildir: sebep seçilmez, iptal kaydı ve ikinci bir geri ödeme açılmaz. Teslim edilmiş bir kalem varsa sipariş S7'yle Teslim edildi'ye geçmiştir (8.2.7) ve kapanış gerekmez. Dönen mal 8.3.2.1 ile işlenir | → İptal edildi (S9 kapanışı) | — · B-7 gitmez (§1.4.3) | `02 §5.8`, §5.4 S9, §7.2.3 · K-672, K-717 |

#### 8.3.3 Geri ödeme

| # | Aktör ne yapar | Sistem ne denetler, ne yapar | Durum | Bildirim | Kaynak |
|---|---|---|---|---|---|
| 8.3.3.1 | Sistem kart hattında geri ödemeyi kendiliğinden başlatır — iptal edilen ödenmiş kalemde (müşteri ya da firma iptali), gecikme feshinde, kalem çıkarmasında ve iptal edilmiş siparişe gelen kart ödemesinde | Onay penceresi yoktur. Kargo ücreti `02 §7.2.9`'un tetiğiyle eklenir. Para sağlayıcıda gerçekleşince ödeme ekseni değişir — iptal edilmiş siparişe gelen ödemede değişmez (§1.3) | Ö3–Ö5 | B-8 → müşteri · F-4 → firma (K-691) | `02 §7.2.8`, §7.2.3, §10.4.3, §6.2.16 · K-684, K-691 |
| 8.3.3.2 | Yönetici havale hattındaki geri ödemeyi bankasından gönderir ve panelden işler — iptal, gecikme feshi, cayma ve tutar bazlı geri ödeme | Süresi olan hatta panel kalan süreyi (Z-16, Z-17) gösterir; tutar bazlı geri ödemede süre sayacı yoktur (8.3.3.6). İşlenecek kalemde müşterinin sipariş sayfasından girdiği IBAN durur; yoksa önce IBAN istenir (8.3.3.6). Yönetici geri ödemeyi havale bankada gerçekleştikten sonra işler ve o anda alanda duran IBAN kullanılır — panel IBAN'ı gösterdikten sonra müşteri IBAN'ı düzelttiyse işlem uygulanmaz ve güncel IBAN gösterilir (3.5.2.15); onay geri alınamaz. IBAN geri ödeme tamamlanınca silinir (Z-35). Kalem "iade ve geri ödeme bekleyen" sayacından düşer. İze yazılır | Ö3–Ö5 | B-8 → müşteri | `02 §7.4.5`, §7.2.8, §5.9, §10.6.1, §12.5 · K-716 |
| 8.3.3.3 | Yönetici caymanın geri ödemesini kart hattında işler | Para sağlayıcı üzerinden karta gider; teslim alma işareti geri ödemeyi kendiliğinden başlatmaz. Cayılan adetlerin ödenmiş bedeli tam ödenir — tutarı `02 §7.1.7`'yle (K-788); kargoyla gönderilen fiziksel kalemlerin bütün adetlerinden cayıldıysa kargo ücreti son caymanın geri ödemesine eklenir (K-789); kupon hakkı yalnız siparişin tamamı iade edilince döner. Onay geri alınamaz. İze yazılır | Ö3–Ö5 | B-8 → müşteri | `02 §7.4.5`, §7.4.6, §7.4.8, §7.4.10, §5.9 · K-788, K-789 |
| 8.3.3.4 | Kart iadesi sağlayıcıda gerçekleşmez — sağlayıcı reddeder ya da ulaşılamaz | Sistem kendiliğinden yeniden denemez. Siparişin panel satırına "geri ödeme gerçekleşmedi" işareti düşer, kalem "iade ve geri ödeme bekleyen" sayacına girer ve panelde uyarı çıkar — uyarı, havale yolunu açmadan ya da yeniden denemeden önce sağlayıcının panelinde ters ibraza ve iadenin kendisinin gerçekleşip gerçekleşmediğine bakmayı hatırlatır: sağlayıcıya ulaşılamayan bir iade orada gerçekleşmiş olabilir ve çift ödemenin riski firmadadır (K-705). Z-16 ve Z-17 işlemeye devam eder | — | — (`02 §9.2`) — müşteriye ayrı bildirim gitmez | `02 §7.2.8`, §3.21.11 · K-683, K-705 |
| 8.3.3.5 | Yönetici kart iadesini yeniden dener ya da müşteriye havale yolunu açar | Yol açılınca sipariş sayfası kart iadesinin gerçekleşmediğini söyler ve kalem için IBAN alanı açar; IBAN girmek müşterinin seçimidir. IBAN girilene kadar yönetici kart iadesini yeniden deneyebilir; iade gerçekleşirse IBAN alanı kapanır, IBAN girildikten sonra kart iadesi yeniden denenmez. Kart iadesi gerçekleşince ya da IBAN'a geri ödeme işlenince işaret kalkar. İze yazılır | Kart iadesi gerçekleşince Ö3–Ö5 · yol açılınca kalem: IBAN isteği | Kart iadesi gerçekleşince B-8 → müşteri · yol açılınca B-14 → müşteri | `02 §7.2.8`, §3.21.11 · K-675 |
| 8.3.3.6 | Yönetici havale hattında IBAN'ı olmayan bir geri ödeme için müşteriden IBAN ister — ayıp talebinin para gerektiren çözümü, tutar bazlı kısmi geri ödeme, iade reddinden ya da "mal dönmedi" kapanışından sonraki ödeme kararı | Müşterinin sipariş sayfasında IBAN alanı açılır. İstek "IBAN bekleniyor" listesinde görünür — süresi olan hatta kalan süresiyle; ayıpta ve tutar bazlı geri ödemede süre sayacı yoktur ve ayıpta para derhâl geri ödenir (`02 §12.5`; K-716) — ve bekleyen işler sayacında sayılmaz; müşteri IBAN'ı girince kalem işlenecek geri ödeme olarak sayaca girer ve 8.3.3.2 işler. Yönetici IBAN'ı panelden girmez. İstek ize yazılır | kalem: IBAN isteği | B-14 → müşteri | `02 §7.4.5`, §10.6.1, §12.5 · K-686, K-716 |
| 8.3.3.7 | Yönetici tutar bazlı kısmi geri ödeme işler — ayıp talebinde bedel indirimi, fiyat yerine indirim, "mal dönmedi" kapanışından sonra ya da iade reddinden dönüşte ödemeye karar verilen tutar | Tutar kalemden bağımsızdır ya da bir kaleme bağlıdır ve siparişte ödenmiş ve henüz geri ödenmemiş tutarı aşamaz. Kartta sağlayıcı üzerinden, havalede IBAN'a (8.3.3.2) gider; onay geri alınamaz. Kapatılmış kalem kapalı kalır; mal sonradan ulaşırsa ödenmiş kısım kalemin geri ödeneceğinden düşülür. İze yazılır | Ö3–Ö5 | B-8 → müşteri | `02 §5.5`, §10.4.3, §10.4.9, §7.4.9, §7.5.3 · K-686 |
| 8.3.3.8 | Yönetici bankada gerçekleşmeyen geri ödeme havalesini bildirir — "havale gerçekleşmedi" | Geri ödeme işlenmeden önce yapılır: kalemin IBAN'ı silinir, sipariş sayfasında IBAN alanı yeniden açılır; kalem "IBAN bekleniyor" listesine döner ve geri ödemenin süresi durmaz. İşlenmiş geri ödemeden sonra dönen havalede ödeme ekseni düzeltilmez; yönetici müşteriye siparişin iletişim bilgisinden ulaşır ve parayı yeniden gönderir. İze yazılır | kalem: IBAN isteği | B-14 → müşteri | `02 §7.4.5` |
| 8.3.3.9 | Müşteri IBAN isteğini hiç cevaplamaz | Kalem kapanmaz ve borç sürer; kendiliğinden kapanma yoktur. Kalem "IBAN bekleniyor" listesinde kalır; yönetici siparişin iletişim bilgisini görür. Süre işler; IBAN isteğinin tarihi izde gecikmenin müşteriden kaynaklandığının kanıtıdır | — | — | `02 §7.4.5`, §10.6.1 |

### 8.4 Sipariş müdahaleleri

`02 §10.4`'ün dokuz müdahalesi (8.4.1–8.4.9) ve ayıp talebinin yönetimi (8.4.10, 8.4.11). Müdahaleler akış 6'nın parçasıdır (K-468); her müdahale ize yazılır. **Elle sipariş oluşturma, kalem ekleme ve fiyat değiştirme yoktur** — onaylanan tutar bağlayıcıdır; indirim yapmak isteyen firma tutar bazlı kısmi geri ödeme kullanır (8.3.3.7; `02 §10.4.1`, §7.6.6). Müdahalelerin hiçbiri zorunlu elle adım değildir (§8.5).

| # | Aktör ne yapar | Sistem ne denetler, ne yapar | Durum | Bildirim | Kaynak |
|---|---|---|---|---|---|
| 8.4.1 | Yönetici teslimat ya da fatura adresini — teslimat telefonu dahil — müşterinin talebiyle düzeltir | Talep siparişin iletişim e-postasından gelmediyse yönetici siparişin kanalına dönerek teyit eder (§6.2.1); firma kendi öğrendiği bir eksikte de aynı kanallardan ulaşır. Sınır Kargoya verildi'dir; fiziksel kalemsiz siparişte fatura adresi Teslim edildi'ye kadar düzeltilir. İz alanı ve işlemi yazar, adres değerini yazmaz | — | B-10 → müşteri | `02 §10.4.2`, §10.3.1 |
| 8.4.2 | Yönetici kalem adedini azaltır ya da kalemi çıkarır | Ödeme onayından sonra; fiziksel kalemde Kargoya verildi'ye kadar, dijital ve hizmet kaleminde teslim işaretine kadar. Sebep listesi yoktur. Azaltılan adedin tutarı `02 §7.1.7`'yle ayrılır (K-788). Kart hattında karta para gönderen çıkarma geri alınamaz onay ister (§1.6.1.5). Para iptalin hattından geri ödenir (8.3.3.1, 8.3.3.2) — havale hattında IBAN isteği çıkarma kaydıyla düşer (K-690); stok, kontenjan ve kupon hakkı iptaldeki rejimle döner; kargo ücreti `02 §7.2.9`'un tetiğiyle | kalem: çıkarma kaydı · son açık kalemse kapanış kuralı — hiçbir kalem teslim edilmemişse kapanışla S3 ya da S4 — Kargoya verildi ve Teslim edilemedi'de yöneticinin S7 ve S9 kapanışı (K-717) —, edilmişse S2, S10 ya da S11 · havale hattında kalem: IBAN isteği | B-11 → müşteri · havale hattında B-14 → müşteri (K-690); kapanışta B-7 gitmez | `02 §10.4.3`, §5.8, §5.9 · K-672, K-690, K-717, K-788 |
| 8.4.3 | Yönetici kargo şirketini ve takip numarasını düzeltir | Sipariş kargoya verildikten sonra | — | B-12 → müşteri | `02 §10.4.4` |
| 8.4.4 | Yönetici teslim tarihini ya da iade malının ulaşma tarihini düzeltir | Teslim işaretinden sonra, sipariş düzeyinde; kargoya verildiği günden önceki ve ileri tarih reddedilir. Cayma penceresi ve ayıp süresi düzeltilmiş tarihten işler — düzeltme pencereyi kısaltabilir ve açık bir düğmeyi kapatabilir; yapılmış beyan geçerli kalır. Tarih bir beyanın günüyle çakışırsa panel teslimin sırasını sorar. İz eski ve yeni tarihi yazar. Ulaşma tarihi aynı kalıpla düzeltilir — teslim alma kaydı düzeyinde (parça parça ulaşan malda her teslim almanın tarihi ayrıdır — K-794), iade teslim almadan sonra, geçmişe dönük ve ileri olmayan; teslimden sonraki caymanın geri ödeme süresi (Z-16) düzeltilmiş tarihten işler (K-712) | — | — (`02 §9.2`) | `02 §10.4.5`, §7.4.1 · K-712, K-794 |
| 8.4.5 | Yönetici yanlış durum geçişini düzeltir — kapalı listeden sebep seçerek: yanlış siparişte işlem yapıldı · işlem gerçekleşmeden işaretlendi · diğer, açıklamayla | Panel "geri al" sunmaz; düzeltme yeni bir geçiştir ve sınırları §1.6.2'dedir; dış dünyaya çıkmış sonuç geri alınmaz. Havale "ödendi" işaretinin düzeltilmesinde Z-8 düzeltme anından yeniden işler ve hatırlatma yeni sürenin son iş gününde bir kez daha gider | §1.6.2'nin satırları | B-13 → müşteri · havale işaretinin düzeltilmesinde ayrıca B-3 → müşteri (K-685) | `02 §10.4.6` · K-685 |
| 8.4.6 | Yönetici siparişe iç not yazar | Not müşteriye hiçbir yerde görünmez; sipariş sayfasına, e-postalara ve sözleşmeye girmez. Yazılması ve değiştirilmesi ize yazılır, metni yazılmaz | — | — (`02 §9.2`) | `02 §10.4.7` |
| 8.4.7 | Yönetici malı dönmemiş cayma kalemini "mal dönmedi" gerekçesiyle kapatır — isteğe bağlı | Z-42 geçmiş ve mal ulaşmamışken sunulur; kapatma beyanın ulaşmamış adetlerine uygulanır — bir kısmı teslim alınmış beyanda yalnız kalanlara (K-794). Geri ödeme yapılmaz ve kapatma geri ödeme borcuna karar vermez; havale hattında IBAN silinir; kalem "iade ve geri ödeme bekleyen" sayacından düşer. Mal sonradan ulaşırsa 8.3.2.1 kalemi yeniden açar; firma ödemeye karar verirse 8.3.3.7 işler — havalede IBAN 8.3.3.6 ile istenir | kalem: "mal dönmedi" kapanışı | — (`02 §9.2`) | `02 §10.4.9` · K-686, K-794 |
| 8.4.8 | Yönetici başka kanaldan gelen cayma ya da gecikme feshi bildirimini kaydeder — caymada kalemi ve cayılan adedi (K-787), ikisinde de bildirimin firmaya ulaştığı tarihi girer; gecikme feshinde kalem ve adet seçilmez (1.7.4; K-796) | Yönetici bildirimin siparişin sahibinden geldiğini teyit eder (§6.2.1); teyit kaydın tarihini değiştirmez. Tarih geçmişe dönük olabilir, ileri olamaz; pencere dışındaki — süresinde gönderilmiş ve geç ulaşmış bildirim ya da eksik bilgilendirme (K-709) — ya da mutlak istisna kalemindeki kayıt ayrıca onaylatılır, pencerenin bitiminden bir yıl (Z-13'ün yasal uzaması) sonrasına tarihli kayıt reddedilir; teslim günüyle çakışan tarihte panel teslimin sırasını sorar. Havale hattında IBAN yalnız siparişin iletişim e-postasından gelen ve denetimden geçen bildirimden aktarılır; öteki hâllerde kayıt IBAN'sız yapılır. Kargoya verilmemiş fiziksel kalemde ve ödemesi beklenen siparişin hizmet kaleminde kayıt açılmaz — yönetici "müşteriyle anlaşıldı" iptalini yapar (8.3.1.1). Kayıt geri alınamaz onay ister (§1.6.1.7; K-715), geri alınmaz ve düzeltilmez. **Gecikme feshi bildirimi** aynı yoldan kaydedilir: kayıt teslim edilmemiş fiziksel kalemlerin açık adetlerinin tamamına uygulanır ve yalnız 1.11.8'in aralığında — teslim tarihi girilmemiş açık fiziksel kalem, Z-11 geçmişken — açılır ve sipariş sayfasından yapılmış fesih gibi işler; geri ödemenin on dört günü (2.7.8) ve kanuni faiz girilen tarihten işler, panel kanuni faiz uyarısını gösterir (K-706) | kalem: cayma beyanı ya da gecikme feshi · IBAN'sızsa IBAN isteği · feshin kapanışı 2.7.7'nin dallarıyla | B-9 → müşteri · IBAN'sızsa B-14 → müşteri · firmaya — (`02 §9.3.2`) | `02 §10.4.10`, §7.3.5, §7.6.7, §7.2.3 · K-673, K-688, K-706, K-709, K-715, K-787, K-796 |
| 8.4.9 | Yönetici misafir siparişinin iletişim e-postasını müşterinin talebiyle düzeltir | Yalnız giriş yapılmadan verilmiş siparişte ve siparişin kişisel verileri imha edilene kadar. Yönetici kimliği siparişteki bilgilerle teyit eder (§6.2.1.3). Yeni erişim anahtarı üretilir, eski bağlantılar açılmaz; hesap bağı yeni adrese göre kurulur ya da kesilir. Satırda "e-posta düzeltildi" işareti görünür; eski ve yeni adres siparişin e-posta geçmişinde durur, izde değer olarak durmaz. Durumlar, süreler, kalemler ve IBAN değişmez. Akış §2.10.5 | — | B-1 → yeni adres · B-15 → eski adres | `02 §10.4.11`, §10.3.1 |
| 8.4.10 | Yönetici yeni ayıp talebini okur ve çözümü müşteriyle sistemin dışında yürütür | Talep kalem, ayıplı adet ve sipariş bağlamıyla panele düşer ve "açık talep" sayacındadır (K-793); fiziksel kalemde talebe yazılı iade adresi görünür. Kanıt gerekiyorsa yönetici e-postayla ister — ürün dosya almaz. Para gerekiyorsa sözleşmeden dönmede geri alınan adetlerin iade hattı (8.3.2, 8.3.3; tutar `02 §7.1.7`), bedel indiriminde tutar bazlı kısmi geri ödeme (8.3.3.7) işler; havalede IBAN 8.3.3.6 ile istenir. Ayıplı dijital üründe çözüm dosya güncellemesidir (8.1.6) | Ayıp talebi: Açık | F-3 → firma (§2.9.2) | `02 §7.5.2`–§7.5.4, §10.6.1 · K-686, K-788, K-793 |
| 8.4.11 | Yönetici ayıp talebini "çözüldü" işaretler ya da yeniden açar | Çözümü firma müşteriye kendi cevabıyla yazmıştır. Yeniden açma Z-18 içinde yapılır; yeni talep açılmaz ve ayıbın geçmişi tek kayıtta kalır | Ayıp talebi: Açık → Çözüldü · Çözüldü → Açık | — (`02 §9.2`) | `02 §5.10`, §7.5.3 · K-676 |

### 8.5 Manuel adım bütçesi

Sipariş başına sistem içi zorunlu elle adımlar ve bu bölümün satırlarındaki karşılıkları (`02 §10.5`; K-401, K-404, K-454). **Tavan:** tek tipli bir siparişin hattında zorunlu elle adım üçü geçmez; en ağır hat — havale ile ödenen fiziksel sipariş — üçtür (`02 §10.5.4`). Yazım turunda doğan her yeni elle adım önerisi bu tabloyla okunur (§0.4.5).

| # | Hat ya da adım türü | Adım | §8'deki satırlar | Kaynak |
|---|---|---|---|---|
| 8.5.1 | Kart ile ödenen fiziksel sipariş | **2** | Kargoya verme (8.2.5) · teslim işareti ve tarihi (8.2.6) | `02 §10.5.1` |
| 8.5.2 | Havale ile ödenen fiziksel sipariş | **3** | "Ödendi" işareti (8.2.2) · 8.5.1'in iki adımı | `02 §10.5.1` |
| 8.5.3 | Hizmet siparişi | **1** — havalede 2 | "Tamamlandı" işareti (8.2.9) · havalede "ödendi" (8.2.2) | `02 §10.5.1` |
| 8.5.4 | Dijital sipariş | **0** — havalede 1 | Sistem teslim eder (8.2.3) · havalede "ödendi" (8.2.2) | `02 §10.5.1` |
| 8.5.5 | Karışık sipariş | Hatların adımları toplanır; "ödendi" işareti siparişte bir kez sayılır — havale ile ödenen fiziksel + hizmet siparişi dört adımdır | 8.2.2, 8.2.5, 8.2.6, 8.2.9 | `02 §10.5.1` |
| 8.5.6 | Koşullu adımlar — her biri bir adım | Firma iptali ve sebep seçimi (8.3.1.1) · geri dönen gönderiyi "teslim edilemedi" işaretleme ve yeniden gönderme (8.2.7, 8.2.8) · iade malını teslim alma, ulaşma tarihi ve stoğa ekleme (8.3.2.1, 8.3.2.2) · geri ödemeyi elle işleme — havale hattında ve caymada kart hattında, tutar bazlı kısmi geri ödeme dahil (8.3.3.2, 8.3.3.3, 8.3.3.7) · ayıp talebini "çözüldü" işaretleme (8.4.11) · indirme hakkını yenileme (8.2.10). Adet seçimi bu adımların bir alanıdır; ardışık beyanda ve parça parça ulaşan iadede adım yinelenir, yeni adım türü doğmaz (K-794) | — | `02 §10.5.2` · K-794 |
| 8.5.7 | Bütçeye girmeyen işler | Müdahaleler (8.4.1–8.4.9) · iade reddi — teslim almanın bir seçeneğidir (8.3.2.3) · kart iadesinin yeniden denenmesi ve havale yolunun açılması (8.3.3.5) · IBAN isteği (8.3.3.6) · "havale gerçekleşmedi" (8.3.3.8) · ulaşmayan e-postanın yeniden gönderilmesi (8.9.2) · üye hesabının talep üzerine silinmesi (§9.4.6). Kendiliğinden düşen IBAN isteği (8.3.1.1, 8.4.2, 8.3.2.1, 8.4.8) bir adım değildir | — | `02 §10.5.2` · K-686, K-690 |
| 8.5.8 | Sistem dışı adımlar | Fatura — sipariş başına bir adım, ürün izlemez (8.2.4) · reddedilen malın geri gönderilmesi (8.3.2.4) · havale hattında paranın bankadan gönderilmesi (8.3.3.2) | — | `02 §10.5.2`, §10.5.3 |

**Aşama 2 kararlarının bütçeye etkisi.** Workshop'un, yazım turunun ve kalite döngüsünün kararlarının — K-653…K-717 — hiçbiri sipariş başına zorunlu bir elle adım eklemez; her karar satırı bütçeyi gerekçesiyle okur (`checklists/document-stage.md` §3). Tavan korunur. **Kısmi adet kararı da (K-787…K-794) zorunlu adım eklemez:** adet var olan adımların alanıdır; hat başına sayılar (8.5.1–8.5.5) değişmez (`02 §10.5.2`; K-794). Bütçe H-2'nin hacmine göre kuruludur; aşımın sonucu `10` SK-3'tedir (`02 §11.3`).

### 8.6 Kurumsal içerik, marka, duyuru ve iletişim talepleri (akış 7'nin panel yüzü)

Yedinci akışın adımları ve üç kolu — marka ayarları, duyuru, iletişim talebi (`02 §2.3`; K-285, K-468). Kurumsal taraf satış kapısına bağlı değildir. Ziyaretçinin yüzü §2.2'de, yayın durumları §1.8.4'te, talep durumları §1.9.2'dedir.

#### 8.6.1 Kurumsal içerik kaydı

| # | Aktör ne yapar | Sistem ne denetler, ne yapar | Durum | Bildirim | Kaynak |
|---|---|---|---|---|---|
| 8.6.1.1 | Yönetici içerik tipini seçer ve kaydı açar — Hakkımızda, hizmet tanıtımı, referans iş, sık sorulan soru, şube ya da genel sayfa | Hakkımızda tek kayıttır. Kayıt adla ya da başlıkla — SSS'de soruyla — taslak kaydedilir; Hakkımızda ve duyuru boş da kaydedilir. Galeri, blog ve müşteri görüşü tipi yoktur | Kayıt: Taslak | — | `02 §2.3` adım 1–2, §3.27.1–§3.27.9, §3.27.19, §3.27.20, §3.27.24 |
| 8.6.1.2 | Yönetici içeriği girer — metin, görseller, video bağlantısı; hizmet tanıtımına ve referans işe katalogdan ürün bağlar | Kısa alanlar P-27, P-28 ve P-29 ile, görseller P-22, P-23 ve P-25 ile sınırlıdır; uzun metin kapalı biçim setini kullanır. Gömülü video ve dosya eki yoktur. Bağlı ürünlerden yalnız yayındakiler vitrinde görünür. Metin düzenlemesi ize yazılmaz | — | — | `02 §3.27.10`, §3.27.15–§3.27.18, §3.11.6 |
| 8.6.1.3 | Yönetici taslağı "Taslak" bandıyla önizler | Kendi sayfası olmayan kayıtlar — SSS sorusu, şube, duyuru — göründükleri yerde aynı işaretle önizlenir | — | — | `02 §2.3` adım 3, §3.27.25 |
| 8.6.1.4 | Yönetici kaydı yayına alır | Tipin zorunlu alanları dolmadan yayına alınmaz ve panel eksik alanı gösterir; ikinci bir onay yoktur; ileri tarihli yayın yoktur. Sayfanın adresi ilk yayında sabitlenir. İze yazılır | Kayıt: Taslak → Yayında | — | `02 §2.3` adım 4, §3.27.13, §3.27.23, §3.27.28, §3.30.2 |
| 8.6.1.5 | Yönetici hizmet tanıtımını ya da referans işi ana sayfada gösterir, genel sayfayı menüde gösterir ve listeleri elle sıralar | İşaretli kayıt yoksa ana sayfa bloğu görünmez; SSS, şube ve genel sayfa ana sayfaya çıkmaz. Liste sayfaları, ana sayfa blokları ve menü elle sırayı izler | — | — | `02 §2.3` adım 5, §3.27.11, §3.28.3, §3.28.5 |
| 8.6.1.6 | Yönetici yayındaki kaydı düzenler | Kaydedilen değişiklik anında yayındadır; sürüm geçmişi ve eski hâle dönme yoktur. Yayın kapısını bozan düzenleme kaydedilmez ve panel yöneticiyi önce taslağa almaya yönlendirir. Aynı anda düzenlemede ikinci kaydeden uyarılır | — | — | `02 §2.3` adım 6, §3.27.24, §3.27.26, §3.31.1 |
| 8.6.1.7 | Yönetici kaydı taslağa alır ya da siler | Taslağa alınan kayıt menüden, ana sayfadan ve listelerden kalkar. Silme geri alınamaz uyarısıyla yapılır; ana sayfa işareti, menü yeri ve ürün bağları kalkar, adres serbest kalır. Hakkımızda silinmez, yalnız taslağa alınır. Yayın durumu değişikliği ize yazılır | Kayıt: Yayında → Taslak · ya da silinir | — | `02 §2.3` adım 7, §3.27.3, §3.27.27, §3.30.2 |

#### 8.6.2 Ana sayfa, menü ve marka

| # | Aktör ne yapar | Sistem ne denetler, ne yapar | Durum | Bildirim | Kaynak |
|---|---|---|---|---|---|
| 8.6.2.1 | Yönetici iki hazır ana sayfa düzeninden birini seçer | Tanıtım öncelikli ya da mağaza öncelikli; iki düzende de kurumsal tanıtım ve ürün vitrini birlikte durur. Serbest sayfa kurgusu yoktur. Ayar değişikliği eski ve yeni değerle ize yazılır | — | — | `02 §3.28.1`, §10.3.1 |
| 8.6.2.2 | Yönetici menüdeki adları değiştirir | Menünün iskeleti sabittir; yayında kaydı olmayan tip menüde görünmez. Menü düzenleyici yoktur | — | — | `02 §3.28.4` |
| 8.6.2.3 | Yönetici sosyal medya bağlantılarını ve WhatsApp numarasını girer | Platform listesi kapalıdır; bağlantılar sitenin üst ve alt bölümünde görünür, siparişe donmaz; sohbet penceresi yoktur | — | — | `02 §3.27.12` |
| 8.6.2.4 | Yönetici marka ayarlarını yapar — logo, marka adı, site simgesi, marka rengi — ve platform imzasını kapatır | Logo ve site simgesi isteğe bağlıdır; marka adı zorunludur ve girildikten sonra boşaltılamaz — girilene kadar site alan adını gösterir. Marka renginin üstündeki yazının rengini sistem kontrasta göre seçer. Marka vitrine ve e-postalara uygulanır, siparişe donmaz. Yazı tipi, düzen ve marka rengi dışındaki renkler değiştirilmez; siteye kod — analitik, reklam etiketi, piksel — eklenmez (`02 §8.3.2`, §10.1.2). İze yazılır | — | — | `02 §3.29`, §3.23.5, §8.3.2, §10.1.2, §10.3.1 |

#### 8.6.3 Duyuru

| # | Aktör ne yapar | Sistem ne denetler, ne yapar | Durum | Bildirim | Kaynak |
|---|---|---|---|---|---|
| 8.6.3.1 | Yönetici duyuruyu hazırlar, önizler ve yayına alır — tek, kısa bir metin (P-30), isteğe bağlı bağlantı ve isteğe bağlı başlangıç ve bitiş tarihi | Aynı anda tek duyuru vardır. Yayın durumu ize yazılır, metin düzenlemesi yazılmaz. Duyuru kimseye gönderilmez | Duyuru: Taslak → Yayında | — (`02 §9.5`) | `02 §3.27.21`, §3.27.23, §9.5 |
| 8.6.3.2 | Sistem duyuruyu tarih aralığında gösterir | Tarih girilmişse duyuru yalnız aralığında görünür (Z-23); yöneticinin elle kaldırmasına gerek kalmaz. Yönetici duyuruyu taslağa alarak da kaldırır | — | — | `02 §3.27.21`, §5.2 |

#### 8.6.4 İletişim talepleri

| # | Aktör ne yapar | Sistem ne denetler, ne yapar | Durum | Bildirim | Kaynak |
|---|---|---|---|---|---|
| 8.6.4.1 | Yönetici yeni iletişim talebini öğrenir | Talep panele düşer ve "açık talep" sayacındadır; firmanın iletişim e-postasına bildirim gider | İletişim talebi: Açık | F-2 → firma (§2.2.8) | `02 §3.32.5`, §10.6.1 |
| 8.6.4.2 | Yönetici talebi okur ve müşteriye kendi e-postasıyla cevap verir | Panelde yanıt ekranı, yazışma dizisi ve hazır şablon yoktur; müşterinin yanıtı firma kimliğindeki iletişim e-postasına düşer ve yeni talep açmaz. Talep bir sipariş işlemi gerektiriyorsa yönetici panelin var olan işlemini yapar — indirme hakkının yenilenmesi (8.2.10), adres düzeltmesi (8.4.1), başka kanaldan cayma kaydı (8.4.8), misafir siparişinin e-posta düzeltmesi (8.4.9), "müşteriyle anlaşıldı" iptali (8.3.1.1) — ve işlemin kendi bildirimi gider | — | İşlemin bildirimi | `02 §3.32.5`, §9.1.4 |
| 8.6.4.3 | Yönetici KVKK talebini işler | Talep iletişim talebinin iki durumunu kullanır; süre sayacı yoktur — başvurunun firmaya ulaştığı an talebin kaydında durur (Z-25). Başvurunun unsurlarının tamamlatılması ve başvuranın kimliğinin doğrulanması firmanın yükümlülüğüdür. Cevabın dayanağı sipariş listesi ve üye kaydı görünümüdür (§9.4.5); hesabın talep üzerine silinmesi §9.4.6'dadır | — | — | `02 §3.15.4`, §3.15.5, §12.2.6 |
| 8.6.4.4 | Yönetici talebi kapatır ya da yeniden açar | Saklama süresi son kapatılıştan işler (Z-33) — "Sipariş hakkında" tipinde daha uzundur (K-707); talep müşteriye görünmez | İletişim talebi: Açık → Kapatıldı · Kapatıldı → Açık | — (`02 §3.32.6`) | `02 §3.32.4`, §5.13 · K-707 |

### 8.7 Mağaza ayarları, satış kapısı ve kurulum kontrol listesi (akış 8)

Firmanın kimliği, ödeme yöntemleri, kargo, eşikler, iade adresi ve yasal metinler panelden yönetilir; altyapı kurulumdan gelir (`02 §3.1.2`, §10.1.3). **Kurulumun dış ön koşulları** — alan adı, barındırma, e-posta gönderim yapılandırması, ödeme sağlayıcı sözleşmesi ve anahtarları, ETBİS kaydı, Google uygulaması, ilk yönetici hesabı — panelin değil `10 §4.1`'in kurulum kontrol listesinin konusudur (ÖK-1…ÖK-7; panelde tamamlanan satış kapısı koşulları ÖK-8…ÖK-11 8.7.1.1'dedir); kurulum ayarları panelden değişmez. Panelden değişen her ayar eski ve yeni değerle ize yazılır (`02 §10.3.1`). Donan değerler yalnız yeni siparişlere işler (`02 §3.23`).

#### 8.7.1 Kurulum kontrol listesi ve satış kapısı

| # | Aktör ne yapar | Sistem ne denetler, ne yapar | Durum | Bildirim | Kaynak |
|---|---|---|---|---|---|
| 8.7.1.1 | Yönetici panele ilk kez girer | Site boş kurulur; firma ayarları dolu gelir. Panelin ana sayfasındaki kurulum kontrol listesi satışın açılması için eksik olanları adıyla sayar: firma tipinin zorunlu kimlik alanları — KEP adresi dahil (ÖK-8) · en az bir açık ödeme yöntemi — havale açıksa IBAN (ÖK-9) · aydınlatma metni ve çerez politikası, ikisi de yayında (ÖK-10) · iade adresi (ÖK-11). Sihirbaz ve adım sırası yoktur; madde tamamlandıkça işaretlenir ve hepsi tamamlanınca liste kaybolur | Satış kapalı | — | `02 §10.8`, §3.27.22, §10.1.3 · `10 §4.1` |
| 8.7.1.2 | Sistem satış kapısını sürekli denetler | Dört koşul birlikte sağlandığında satış açıktır (§1.10.8); biri düşerse sepete ekleme ve ödeme kapanır, vitrin ve kurumsal içerik yayında kalır ve panel eksik koşulu adıyla gösterir. Açık siparişler etkilenmez. Aydınlatma metni tamamlanmamışken iletişim formu, hesap kaydı ve Google ile ilk giriş de kapalıdır. Dört koşulun ilk kez birlikte sağlandığı gün ölçüm penceresi başlar (8.9.3) | Satış açıklığı değişir | — | `02 §3.1.5`, §3.1.6, §10.6.3 · K-664 |

#### 8.7.2 Firma kimliği ve yasal metinler

| # | Aktör ne yapar | Sistem ne denetler, ne yapar | Durum | Bildirim | Kaynak |
|---|---|---|---|---|---|
| 8.7.2.1 | Yönetici firma tipini seçer ve kimlik alanlarını doldurur ya da düzenler — isteğe bağlı meslek odası, işletme adı ya da tescilli marka, meslekle ilgili davranış kuralları ve ETBİS doğrulama bilgisi dahil | Zorunlu alanlar tipe göredir ve dolduktan sonra boşaltılamaz; kimlik kaydı silinemez. Tip değişirse kapı yeni tipin setini denetler. Panel ETBİS alanının yanında kaydın firmanın yükümlülüğü olduğunu hatırlatır; alan doluysa doğrulama bandı sitede görünür. Kimlik siparişe donar ve üretilen metinlerin sürümünü artırır (8.7.2.3). İze yazılır | — | — | `02 §3.1.2`–§3.1.4, §3.23.3, §10.3.1 |
| 8.7.2.2 | Yönetici aydınlatma metnini ve çerez politikasını ürünün taslağından düzenler ve yayına alır | Aydınlatma metni barındırma ve e-posta altyapısının konumu bölümü ve firma kimliği alanları dolmadan tamamlanmış sayılmaz; iki metin zorunludur ve boşaltılamaz. Yayına alınınca sürüm kendiliğinden artar; yeniden onay alınmaz ve bildirim gitmez. Sürüm değişikliği ize yazılır | — | — (`02 §9.5`) | `02 §3.33.1`–§3.33.5, §10.3.1 |
| 8.7.2.3 | Sistem Ön Bilgilendirme Formu'nu ve Mesafeli Satış Sözleşmesi'ni ayarlardan üretir | Besleyen bir ayar — kimlik, kargo ücreti, kargoya verme süresi, teslimat illeri, iade adresi — değişince sürüm artar; onay adımı açık bir müşteride artış siparişin oluşmasını durdurur (§2.4.8). Eski sürümler silinmez. Panel, SSS ya da sayfa metinlerinin ayarlarla çelişebileceğini ilgili ayarın yanında hatırlatır; ürün çelişkiyi tespit etmez | — | — (`02 §9.5`) | `02 §3.24.3`, §3.33.3, §3.33.8, §3.17.2 |

#### 8.7.3 Ödeme yöntemleri

| # | Aktör ne yapar | Sistem ne denetler, ne yapar | Durum | Bildirim | Kaynak |
|---|---|---|---|---|---|
| 8.7.3.1 | Yönetici havale/EFT'yi açar ve IBAN girer | Havale açıkken IBAN zorunludur. Kartın panelde anahtarı yoktur: sağlayıcı anahtarları kurulumda tanımlıysa kart açıktır, değilse görünmez. IBAN'ın ilk girişi de bütün yöneticilere bildirilir. İze eski ve yeni değerle yazılır | — | F-5 → bütün yöneticilerin kendi adresleri | `02 §3.21.4`, §3.21.5, §9.3.3, §10.3.1 |
| 8.7.3.2 | Yönetici havale IBAN'ını değiştirir | Onay ödemesi beklenen havale siparişlerinin sayısını ve bu siparişlerin eski IBAN'ı göstermeye devam edeceğini söyler; yeni IBAN yalnız yeni siparişlere işler. Eski hesap kullanılamıyorsa ya da değişiklik yetkisiz yapılmışsa yönetici bu siparişleri firma iptaliyle ("diğer", açıklamayla) kapatır (8.3.1.1). Yetkisiz değişikliğin akışı §6.2.6'dadır | — | F-5 → bütün yöneticilerin kendi adresleri | `02 §3.21.5`, §9.3.3 · K-663 |
| 8.7.3.3 | Yönetici havaleyi kapatır | Yeni siparişte havale görünmez; panel ödemesi beklenen havale siparişlerinin sayısını söyler. Açık siparişler etkilenmez — bekleyen havale siparişi donmuş IBAN'ıyla sürer. Açık başka yöntem yoksa satış kapanır (8.7.1.2) | — | — (`02 §9.3.2`) | `02 §3.21.5`, §3.1.6 · K-664 |

#### 8.7.4 Kargo, teslimat, süreler, eşikler ve iade adresi

| # | Aktör ne yapar | Sistem ne denetler, ne yapar | Durum | Bildirim | Kaynak |
|---|---|---|---|---|---|
| 8.7.4.1 | Yönetici kargo ücretini, ücretsiz kargo eşiğini ve asgari sipariş tutarını girer (P-1, P-3, P-4) | Kargonun KDV oranı bir ayar değildir. Değerler siparişe donar; değişiklik sepette görünür ve onay anına denk gelirse özet farkıdır | — | — | `02 §3.18`, §3.19, §3.23.2, §11.1 |
| 8.7.4.2 | Yönetici teslimat yaptığı illeri seçer | Varsayılan tüm Türkiye'dir; kısıt il düzeyindedir ve müşteriye adres adımından önce görünür. Kısıt Ön Bilgilendirme Formu'nun içeriğidir; değişiklik üretilen metinlerin sürümünü artırır (8.7.2.3) | — | — | `02 §3.20.1`, §3.24.3, §3.33.3 |
| 8.7.4.3 | Yönetici kargoya verme süresinin varsayılanını (P-5) ve havale ödeme süresini (P-7) girer | Çitlerin dışındaki değer sebebiyle reddedilir. Kargoya verme sözü siparişe donar; değişiklik üretilen metinlerin sürümünü artırır | — | — | `02 §3.17.4`, §3.20.4, §3.20.5, §11.1 |
| 8.7.4.4 | Yönetici indirme hakkını (P-8) ve KDV oranının varsayılanını (P-10) değiştirir | İndirme hakkı sipariş anında kaleme donar; değişiklik yalnız yeni siparişlere işler (K-713). Ürün formundaki firma ayarları — P-6, P-9, P-44 — 8.1.4'tedir; on firma ayarının tamamı kurulumda dolu gelir, P-44 dışında | — | — | `02 §3.12.5`, §3.23.1, §3.8.2, §11.1 · K-713 |
| 8.7.4.5 | Yönetici iade adresini girer ya da değiştirir | İade adresi zorunlu bir ayardır ve satış kapısının koşuludur. Değişiklikte panel "iade malı bekleniyor" kalemlerin sayısını söyler; yapılmış beyanların adresi değişmez, yeni beyanlar güncel adresi alır; eski adrese gönderilen malı teslim almak firmanın yükümlülüğüdür (8.3.2.1). Üretilen metinlerin sürümü artar | — | — (`02 §9.3.2`) | `02 §3.1.7`, §7.4.4 · K-665 |

#### 8.7.5 Satışın geçici kapatılması

| # | Aktör ne yapar | Sistem ne denetler, ne yapar | Durum | Bildirim | Kaynak |
|---|---|---|---|---|---|
| 8.7.5.1 | Yönetici satışı geçici olarak kapatır | Sepete ekleme ve ödeme kapanır; vitrin ve kurumsal içerik yayında kalır, müşterinin sepeti korunur. Açık siparişler etkilenmez — yönetici onları yürütür, havale onayı dahil. Bakım modu ve ilan edilen bakım penceresi yoktur (§10.2.1, §10.2.3) | Satış kapalı | — (`02 §9.3.2`) | `02 §3.1.6` · K-664 |
| 8.7.5.2 | Yönetici satışı yeniden açar | Satış, kapının öteki üç koşulu da sağlanıyorsa açılır | Satış açık | — | `02 §3.1.5` |

### 8.8 Yönetici hesapları ve davet

İlk yönetici kurulumda doğar; sonrakiler davetle açılır (`02 §10.2.1`). Bütün yöneticiler aynı yetkiye sahiptir ve bütün müşteri verisini görür; yetki matrisi yoktur (`02 §10.2.3`). Yönetici hesabı müşteri hesabından ayrı bir hesap türüdür ve sepeti ve siparişi olmaz — kendi mağazasından alışveriş yapacak yönetici ayrı bir müşteri hesabı açar ya da misafir alıcı olur (`02 §10.2.5`). Davetin geçerliliği §1.10.7'de, Z-3 §4.1.3'tedir. Panel girişi, şifre sıfırlama ve e-posta değişikliği müşteriyle aynı kurallarla işler; akışları §9.2.7, §9.2.8 ve §9.3.7'dedir (`02 §10.2.4`). Hiçbir yönetici panele giremediğinde yol §10.4'tür (K-699).

| # | Aktör ne yapar | Sistem ne denetler, ne yapar | Durum | Bildirim | Kaynak |
|---|---|---|---|---|---|
| 8.8.1 | Yönetici bir e-posta adresine yönetici daveti gönderir | Bir yönetici hesabına ait adrese davet gönderilemez; müşteri hesabına ait adrese giden davet ayrı bir yönetici hesabı açar. Aynı adrese yeni davet öncekini geçersiz kılar ve ömrü kendi gönderiminden işler (Z-3). Davet yönetici listesinde bir satır olarak görünür; e-posta ulaşmazsa satıra "e-posta ulaşmadı" düşer ve yol yeni davettir. Davetin gönderimi ize yazılır | Yönetici daveti: geçerli | Yönetici daveti → davetlinin kendi adresi (`02 §9.4`) · F-6 → bütün yöneticilerin kendi adresleri (K-692) | `02 §10.2.1`, §10.2.5, §10.2.6, §9.4 · K-660, K-661, K-692 |
| 8.8.2 | Yönetici (davetli) bağlantıdan girer, adını yazar ve şifresini kurar | Bağlantı geçerliyse hesap o anda doğar; ad zorunludur (K-736), şifre kuralları müşteriyle aynıdır. Kendi kendine yönetici kaydı ve panelde Google ile giriş yoktur. Bağlantı kullanılmış, süresi dolmuş ya da geçersizleşmişse hesap açılmaz ve tıklayan süresi dolmuş davetin mesajını görür. Hesabın açılması ize yazılır | Davet kullanılır · yönetici hesabı doğar | — (`02 §9.4`; K-694) | `02 §10.2.1`, §10.2.5, §6.9.12 · K-694, K-736 |
| 8.8.3 | Yönetici kullanılmamış daveti geri çeker | Her yönetici, yönetici listesindeki davet satırından geri çeker; bağlantı o anda geçersizleşir. Geri çekme ize yazılır | Yönetici daveti: geçersiz | Davetliye — (`02 §10.2.6`) | `02 §10.2.6` · K-660 |
| 8.8.4 | Sistem süresi dolan daveti geçersiz kılar | Z-3 dolunca bağlantı çalışmaz; yönetici yeni davet gönderir | Yönetici daveti: geçersiz | — (`02 §10.2.6`) | `02 §4.2` Z-3 |
| 8.8.5 | Yönetici başka bir yöneticiyi kaldırır | Son yönetici ve yöneticinin kendi hesabı kaldırılamaz; panel sebebini söyler. Onay geri alınamaz ve kaldırılanın gönderdiği kullanılmamış davetlerin sayısını söyler; davetler kaldırmayla geçersizleşir. Kaldırılanın açık oturumları anında sonlanır; izdeki satırları adıyla ve e-posta adresiyle okunur kalır. Pasifleştirme yoktur. Kaldırma ize yazılır | Yönetici hesabı kalkar · davetleri geçersiz | F-6 → bütün yöneticilerin kendi adresleri, kaldırılan dahil (K-692) | `02 §10.2.2` · K-662, K-692 |
| 8.8.6 | Davet yanlış adrese gitmiştir — yazım hatası ya da eski bir çalışanın adresi | Yöneticiler durumu F-6'dan ya da yönetici listesinden fark eder. Davet kullanılmamışsa yönetici onu geri çeker (8.8.3); kullanılmışsa açılan hesabı kaldırır (8.8.5), işlem izini tarih ve yönetici süzgeciyle okur (8.9.4) ve ihlal yükümlülüğünü değerlendirir (§6.3.2.1). Davetlinin hesap açması ayrıca bildirilmez (K-694) | Davet geçersiz ya da yönetici hesabı kalkar | Kaldırmada F-6 → bütün yöneticiler (K-692) | `02 §10.2.2`, §10.2.6, §12.2.9 · K-660, K-692, K-694 |

### 8.9 Bekleyen işler, satış özeti, işlem izi ve dışa aktarma

Panelin ana sayfası, raporları ve toplu veri işlemleri (`02 §10.3`, §10.6, §10.7). Bekleyen işler e-postayla hatırlatılmaz (`02 §10.6.1`).

| # | Aktör ne yapar | Sistem ne denetler, ne yapar | Durum | Bildirim | Kaynak |
|---|---|---|---|---|---|
| 8.9.1 | Yönetici panelin ana sayfasında bekleyen işleri görür | Altı sayaç: ödeme onayı bekleyen · kargoya verilecek — geri dönmüş Teslim edilemedi siparişleri dahil · teslim işareti bekleyen · tamamlanmayı bekleyen hizmet · açık talep — iletişim ve ayıp talepleri · iade ve geri ödeme bekleyen — sağlayıcıda başarısız kart iadesi dahil. Her sayaç kendi süzülmüş listesine götürür. "IBAN bekleniyor" listesi ayrıdır ve sayaçta sayılmaz. Art arda başarısız e-posta gönderiminde kanal uyarısı çıkar; firmaya giden bildirimler (F-1…F-4) ulaşmadığında ayrı bir "firma bildirimleri ulaşmıyor" uyarısı çıkar ve "e-posta ulaşmadı" işaretli sipariş varken bir satır sipariş listesinin bu işarete süzülmüş hâline götürür — satır sayaç değildir. Sayaçlar mevcut durumlardan hesaplanır, saklanmaz | — | — (`02 §9.3.2`, §10.6.1) | `02 §10.6.1`, §9.1.6 · K-686, K-760, K-761 |
| 8.9.2 | Yönetici "e-posta ulaşmadı" işaretli siparişin ayrıntısında ulaşmayan e-postayı yeniden gönderir | İşaret Z-26'nın üç denemesi başarısız olunca siparişin satırına düşer; sipariş ayrıntısı ulaşmayan e-postaları tek tek gösterir ve her biri ayrı yeniden gönderilir (K-761); işareti düşüren her müşteri bildirimi — B-1…B-16 — yeniden gönderilir. Firmaya giden bildirim satıra işaret düşürmez ve yeniden gönderilmez (8.9.1; `02 §9.1.6`). Sipariş e-postasında (B-1) gönderilen metin siparişe donmuş sürümdür. Başarılı gönderimle işaret kalkar; gönderilmezse kalır. Yeniden gönderim ize yazılır | — | Yeniden gönderilen bildirim, ilk alıcısına | `02 §9.1.6`, §3.33.6, §10.3.1 · K-675, K-759, K-760, K-761 |
| 8.9.3 | Yönetici satış özetini seçtiği dönem için okur | Sipariş sayısı ve cirosu, en çok satan ürünler, ödeme yöntemi dağılımı, iptal ve iade sayısı ve dört ölçü — ödeme tamamlama, iletişim talebi sayısı, kargoya verme sözüne uyum, iptal ve iade oranı. Analitik değildir: ziyaretçi verisi kullanmaz, saklanmaz ve dışa aktarılmaz; paydası sıfır olan oran "değerlendirilemez" sayılır | — | — | `02 §10.6.2`, §10.6.3 |
| 8.9.4 | Yönetici işlem izini tarih aralığı ve yönetici süzgeciyle okur | İz düz bir listedir; arama, gruplama ve dışa aktarma yoktur. Satırlar kim, ne zaman, ne değişti üçlüsünü taşır, kişisel veriyi değer olarak taşımaz; Z-30 boyunca değiştirilemez ve silinemez. Sistem olayları ize yazılmaz; giriş kaydı panelde görünmez | — | — | `02 §10.3` · K-687 |
| 8.9.5 | Yönetici siparişleri tarih aralığıyla CSV olarak dışa aktarır | Dosya ürünün tuttuğu fatura verisini taşır — kalem dökümü, KDV, indirim ve kupon payı, kargo ücreti ve bölünmüş KDV'si, donmuş fatura adresi ve firma kimliği, ödeme yöntemi ve tarihi, kargoya verme tarihi ve kargo şirketi —; ihlalde ulaşma için siparişin iletişim e-postasını ve alıcı adını; geç gelip geri ödenen kart ödemesinin tarih ve tutarını. Dışa aktarma ize yazılır ve dosyanın sorumluluğu firmaya geçer | — | — | `02 §10.7.1`, §3.25 |
| 8.9.6 | Yönetici üye listesini ve iletişim taleplerini dışa aktarır | Ad ve e-posta CSV olarak iner; düğme olağandır ve her dışa aktarma ize yazılır | — | — | `02 §10.7.4` |
| 8.9.7 | Yönetici kataloğu, içeriği, işlem izini ya da satış özetini dışa aktarmak ister | Yoktur: katalog ve içerik için ürün içinde dışa aktarma yoktur — firmanın çıkış hakkı barındırma düzlemindedir; iz ve özet dışa aktarılmaz | — | — | `02 §10.7.3` |

## 9. Destekleyici akışlar

> **Ne yazılır:** Kayıt, profil yönetimi, hesap silme, tercih yönetimi.

Bu bölüm üyelik akışını (akış 4) yazar: kayıt ve e-posta doğrulama → giriş → hesap yönetimi → hesap silme (`02 §2.1`; K-650). Üyelik siparişin ön koşulu değildir — misafir alıcı hesap açmadan sipariş verir ve izler (§2.4, §2.6; `02 §3.13.1`). Kimlik doğrulama kuralları yönetici hesabına da aynen ve aynı değerlerle işler (`02 §3.13.16`, §10.2.4); panelin farkları satır içinde yazılır, yönetici hesabının davetle açılması ve kaldırılması §8.8'dedir.

- **Biçim ve aktör:** adım tablosu §0.4.1'dir. Aktör kayıttan önce Ziyaretçi, hesabı olan kişi Üye'dir; doğrulama, sıfırlama ve geri alma bağlantısını açan kişi — hesabı henüz açılmamıştır ya da kişi hesabına giremez — Kullanıcı diye yazılır. Hesap durum makinesi taşımaz; geçerliliği §1.10.5'te, oturum §1.10.6'dadır.
- **Bildirim:** hesap ve yönetim e-postaları bildirim matrisinin (B-, F-) dışındadır ve adıyla yazılır (`02 §9.4`); hepsi hesabın — ya da kaydın, yeni adresin, eski adresin — kendi adresine gider. Bildirim üretmeyen hesap olayları `02 §9.4`'ün listesindedir (K-694); hücre "— (`02 §9.4`)" der. Hesap e-postası ulaşmazsa "e-posta ulaşmadı" işareti düşmez: kullanıcı bağlantıyı ekrandan yeniden ister (§3.1.1).
- **Dallar:** hatalar §3.4'te, limitler §6.1'de, süreler §4.1'in "Hesap ve oturum" satırlarında (Z-1…Z-5), Z-45 ve Z-46'da; e-posta değişikliğinin ele geçirilmesi §6.2.5'tedir.

### 9.1 Kayıt, e-posta doğrulama ve Google ile ilk giriş

| # | Aktör ne yapar | Sistem ne denetler, ne yapar | Durum | Bildirim | Kaynak |
|---|---|---|---|---|---|
| 9.1.1 | Ziyaretçi kayıt ekranını açar | Aydınlatma metni tamamlanıp yayına alınmadıysa ya da çerez politikası yayına alınmadıysa (K-850) kayıt ekranı ve yeni hesap açacak Google ile giriş kapalıdır; mevcut hesapların girişi etkilenmez. Ekran aydınlatma metninin bağlantısını gösterir. Pazarlama onayı kutusu yoktur (§9.5.2). "Google ile giriş" düğmesi yalnız Google uygulaması kurulumda tanımlıysa görünür | — | — | `02 §3.13.6`, §3.13.18, §3.13.19, §3.33.5 |
| 9.1.2 | Ziyaretçi adını, e-postasını ve şifresini yazar ve kaydolur | Ad zorunludur. Şifre asgari uzunluğu (P-17) karşılar ve ürünle gelen çok yaygın şifreler listesinde değildir; büyük harf, rakam ve sembol dayatılmaz. E-posta bir müşteri hesabına — doğrulanmamış bekleyen kayıt dahil — aitse ekran "bu e-posta zaten kayıtlı" der; kayıt L-3 ile limitlidir. Geçen kayıt doğrulanmamış bir kayıt açar: e-posta doğrulanana kadar giriş ve sipariş yoktur | Hesap: doğrulanmamış kayıt | E-posta doğrulama bağlantısı → kaydın adresi (`02 §9.4`) — kaydı yapmadıysa bağlantıya tıklamamasını da söyler (K-696) | `02 §3.13.4`, §3.13.11, §3.13.15, §3.13.21, §8.2 L-3 · K-696 |
| 9.1.3 | Kullanıcıya bağlantı ulaşmaz; bağlantıyı ekrandan yeniden ister | Yeni bağlantı öncekileri geçersiz kılar ve kaydın ömrünü uzatmaz — Z-1 ilk gönderimden işler. İstek L-9 ile limitlidir; mesaj nötrdür | — | E-posta doğrulama bağlantısı → kaydın adresi, yeniden | `02 §3.13.5`, §8.2 L-9 · K-658, K-659 |
| 9.1.4 | Kullanıcı doğrulama bağlantısını Z-1 içinde açar | Hesap doğrulanır; giriş ve sipariş açılır. O e-postaya ait geçmiş misafir siparişleri hesaba düşer ve "Siparişlerim"de görünür — silinmiş bir hesabın siparişleri hariç | Hesap: doğrulanmış | — (`02 §9.4`) | `02 §3.13.2`, §3.13.4, §3.15.3 · K-694 |
| 9.1.5 | Kullanıcı bağlantıyı Z-1 içinde açmaz | Kayıt silinir ve e-posta yeniden kayda açılır; süresi dolmuş bağlantıyı açan "bağlantı geçersiz, yeniden kayıt olun" mesajını görür (§3.4.1) | Hesap: doğrulanmamış kayıt silinir | — (`02 §9.4`) | `02 §3.13.5`, §4.2 Z-1 · K-683 |
| 9.1.6 | Ziyaretçi "Google ile giriş"le ilk kez girer — Google'ın doğrulanmış verdiği e-postayla eşleşen müşteri hesabı yoktur | Veri toplayan girişlerin kapısı açıksa (`02 §3.1.5`) hesap o anda doğrulanmış olarak, şifresiz ve Google'ın verdiği adla doğar; o e-postaya ait geçmiş misafir siparişleri hesaba düşer. E-posta doğrulanmamış bekleyen bir kayda eşleşiyorsa kayıt devralınmaz: adı ve şifresiyle silinir — silme imha kaydına yazılır (Z-44; K-708) — ve hesap Google ile doğar (K-696). Kapı kapalıysa giriş yapılmaz — bekleyen kayıt Z-1'in sonuna kadar durur. Ekran aydınlatma metninin bağlantısını gösterir | Hesap: doğrulanmış · bekleyen kayıt varsa silinir | — (`02 §9.4`) | `02 §3.13.2`, §3.13.6, §3.13.7, §3.13.8, §3.13.19, §3.13.21, §12.2.7 · K-694, K-696, K-708 |
| 9.1.7 | Ziyaretçi Google ile girer; Google e-postayı doğrulanmamış verir | Hesap açılmaz ve mevcut bir hesaba bağlama yapılmaz; ekran kullanıcıyı e-posta ve şifreyle kayda yönlendirir (9.1.2) | — | — | `02 §3.13.7` · K-697 |

### 9.2 Giriş, oturum, şifre sıfırlama ve yeniden doğrulama

| # | Aktör ne yapar | Sistem ne denetler, ne yapar | Durum | Bildirim | Kaynak |
|---|---|---|---|---|---|
| 9.2.1 | Üye e-postası ve şifresiyle girer; "beni hatırla"yı işaretler ya da işaretlemez | Vitrin girişi yalnız müşteri hesaplarını arar; doğrulanmamış kayıtla giriş yapılmaz. "Beni hatırla" işaretliyse uzun (Z-5), değilse kısa (Z-4) oturum açılır. Başarısız denemeler L-1 ile limitlidir; deneme — başarılı ya da başarısız — giriş kaydına yazılır (Z-40; `02 §3.13.20`); tarayıcıya o hesap için tanınan tarayıcı işareti bırakılır ya da işaret yenilenir (Z-46). Misafir sepeti hesabın sepetiyle birleşir (§2.3.6) | Oturum açılır | — (`02 §9.4`) | `02 §3.13.4`, §3.13.12, §8.2 L-1, §8.2.6, §10.2.5 · K-694 |
| 9.2.2 | Üye Google ile girer — Google'ın doğrulanmış verdiği e-posta mevcut bir müşteri hesabınınkiyle eşleşir | Kullanıcı o hesaba girer ve hesap bundan sonra iki giriş yolunu taşır. Google ile giriş L-1'e girmez ve giriş kaydına yazılır (`02 §3.13.20`); veri toplayan girişlerin kapısı yalnız yeni hesap açan girişi kapatır. Üye bağlanmış Google girişini hesabından kaldıramaz; bağ yalnız e-posta değişikliğinin geri alınmasında kalkar (9.3.6; K-733) | Oturum açılır · hesaba Google girişi bağlanır | — (`02 §9.4`) | `02 §3.13.7`, §3.13.19, §8.2.6 · K-694, K-733 |
| 9.2.3 | Üye çıkış yapar ya da oturumu kendiliğinden kapanır (Z-4, Z-5) | Sepet hesapta kalır; o tarayıcıda sepet boş görünür (§2.3.7). Kullanıcı yeniden girer | Oturum kapanır | — (`02 §9.4`) | `02 §3.13.12`, §3.16.2 · K-694 |
| 9.2.4 | Kullanıcı "şifremi unuttum" der ve e-postasını yazar | Ekran nötr konuşur: "bu adres kayıtlıysa sıfırlama bağlantısını gönderdik". Talep L-2 ile limitlidir. Bağlantı tek kullanımlıktır ve Z-2 boyunca geçerlidir. Google ile açılmış, şifresi olmayan hesapta aynı akış "şifre belirle" işlevi görür | — | Şifre sıfırlama bağlantısı → hesabın adresi (`02 §9.4`) | `02 §3.13.8`, §3.13.9, §3.13.11, §8.2 L-2 |
| 9.2.5 | Kullanıcı bağlantıyı açar ve yeni şifresini kurar | Şifre politikası işler (9.1.2). Hesabın bütün oturumları kapanır; sıfırlamayı yapan tarayıcı o hesap için tanınan tarayıcı olur, öteki işaretler düşer — e-posta ekseninde engellenen gerçek kullanıcının çıkış yolu budur (§6.1.3.3). Kullanıcı yeni şifreyle girer; Google ile açılmış hesap bundan sonra iki giriş yolunu taşır | Şifre yenilenir · oturumlar kapanır | — (`02 §9.4`) | `02 §3.13.8`, §3.13.10, §8.2.6 · K-694 |
| 9.2.6 | Üye yeniden doğrulama isteyen bir işlemi başlatır — şifre değiştirme, e-posta değiştirme, hesap silme | Oturum açık olsa da şifresini ya da — şifresi olmayan hesapta — Google'ı yeniden doğrular; başka hiçbir işlem yeniden doğrulama istemez. Başarısız şifre denemesi L-1'e sayılır (K-711; hata 3.4.17) | — | — | `02 §3.13.13`, §8.2 L-1 · K-711 |
| 9.2.7 | Yönetici panele girer | Panel girişi yalnız yönetici hesaplarını arar ve şifreyle yapılır; panelde Google ile giriş ve kendi kendine yönetici kaydı yoktur. Oturum, "beni hatırla", L-1 ve tanınan tarayıcı işareti 9.2.1'in kurallarıyla, aynı değerlerle işler; panele limit muafiyeti yoktur | Oturum açılır | — (`02 §9.4`) | `02 §3.13.16`, §8.1.7, §8.2.4, §10.2.4, §10.2.5 · K-694 |
| 9.2.8 | Yönetici panel şifresini unutur | Panelin şifre sıfırlaması yalnız yönetici hesaplarında işler; 9.2.4 ve 9.2.5'in kuralları aynen geçerlidir. Hiçbir yönetici panele giremiyorsa yol §10.4.4'tür | — | Şifre sıfırlama bağlantısı → yöneticinin kendi adresi (`02 §9.4`) | `02 §10.2.4`, §10.2.5 |

### 9.3 Profil ve adres defteri

| # | Aktör ne yapar | Sistem ne denetler, ne yapar | Durum | Bildirim | Kaynak |
|---|---|---|---|---|---|
| 9.3.1 | Üye adını değiştirir | Yeniden doğrulama istemez. Ad iletişim formunun ön doldurmasında, panelin üye kaydı görünümünde ve ulaşma listesinde kullanılır; siparişin alıcı adı değildir — geçmiş siparişe dokunmaz | — | — (`02 §9.4`) | `02 §3.13.21` · K-694 |
| 9.3.2 | Üye adres defterine adres ekler, adlandırır, düzenler ya da siler | Kayıt teslimat adresinin alanlarını taşır; il kapalı listedendir, telefon isteğe bağlıdır. Defterdeki değişiklik ve silme geçmiş siparişe dokunmaz — sipariş adreslerini dondurmuştur. Ödeme adımında yazılan yeni adres üye isterse deftere de kaydedilir (§2.4.2) | — | — (`02 §9.4`) | `02 §3.14.1`, §3.14.3, §3.14.5, §3.14.6 · K-680, K-694 |
| 9.3.3 | Üye şifresini değiştirir | Yeniden doğrulamadan geçer (9.2.6); şifre politikası işler. Hesabın öteki bütün oturumları kapanır ve öteki tarayıcıların tanınan tarayıcı işaretleri düşer; değişikliği yapan oturum ve tarayıcı korunur. Google ile açılmış hesaba şifre ilk kez 9.2.4'ün akışıyla konur | Öteki oturumlar kapanır | — (`02 §9.4`) | `02 §3.13.10`, §3.13.13, §3.13.15, §8.2.6 · K-694, K-695 |
| 9.3.4 | Üye e-posta adresini değiştirmek ister ve yeni adresi yazar | Yeniden doğrulamadan geçer (9.2.6). Yeni adres müşteri hesapları içinde tekildir; başka bir müşteri hesabına ait adres kabul edilmez. Önceki bir değişikliğin geri alma bağlantısı açıkken (Z-45) e-posta yeniden değiştirilemez. Değişiklik yeni adres doğrulanana kadar geçerli olmaz ve hesap eski adresle çalışır; bağlantının yeniden istenmesi L-9 ile limitlidir ve bekleyen değişiklik ilk bağlantının ömrüyle (Z-1) düşer. Bekleyen değişiklik hesap ekranında yeni adresiyle görünür; üye yeni bir adres yazarsa — yeniden doğrulamayla — yeni istek bekleyenin yerine geçer, önceki bağlantı geçersizleşir ve yeni bağlantının ömrü kendi gönderiminden işler; istek L-9'a sayılır. Ayrı bir "vazgeç" işlemi yoktur (K-734) | — | Yeni e-posta adresinin doğrulanması → yeni adres (`02 §9.4`) | `02 §3.13.5`, §3.13.13, §3.13.14, §10.2.5 · K-658, K-659, K-734 |
| 9.3.5 | Üye yeni adresteki bağlantıyı açar | Değişiklik geçerli olur. Bağlama kuralı yeni adres için işler — o adrese ait geçmiş misafir siparişleri hesaba düşer —; eski adrese bağlı siparişler hesapta kalır. Öteki oturumlar kapanmaz. Eski adrese tek kullanımlık "bu değişikliği ben yapmadım" bağlantısı gider; bağlantı açıkken (Z-45) e-posta yeniden değiştirilemez ve hesap silinemez (K-698). Siparişlerin erişim anahtarları değişiklikten etkilenmez | Hesabın e-postası değişir | E-posta değişikliğinin eski adrese bildirimi → eski adres — geri alma bağlantısıyla (`02 §9.4`) | `02 §3.13.2`, §3.13.14, §3.22.5, §4.2 Z-45 · K-698 |
| 9.3.6 | Kullanıcı — adresin önceki sahibi — eski adresteki geri alma bağlantısını Z-45 içinde açar ve ekrandaki tek geri alma düğmesine basar — bağlantının açılması geri almaz (K-839) | Hesabın e-postası eski adrese döner; hesabın bütün oturumları kapanır, tanınan tarayıcı işaretleri düşer, değişiklik süresince bağlanmış Google girişi kaldırılır ve şifre varsa geçersizleşir. Değişiklik süresince bağlanmış siparişler çözülmez. Ele geçirme akışı §6.2.5'tedir | Hesabın e-postası geri döner · oturumlar kapanır | Şifre sıfırlama bağlantısı → eski adres (`02 §9.4`) | `02 §3.13.14`, §8.2.6 · K-839 |
| 9.3.7 | Yönetici kendi e-posta adresini değiştirir | 9.3.4–9.3.6'nın kuralları aynen işler; yeni adres yönetici hesapları içinde tekildir. Değişiklik ve geri alınması işlem izine yazılır. Yöneticinin e-postası firma kimliğindeki iletişim e-postası değildir — firmaya giden bildirimlerin adresi değişmez. Yönetici kendi adını da hesabından değiştirir; ad değişikliği yeniden doğrulama istemez (K-736) | Hesabın e-postası değişir | 9.3.4 ve 9.3.5'in e-postaları | `02 §3.13.16`, §10.2.4, §10.3.1 · K-736 |

### 9.4 Hesabın silinmesi ve kişisel veri başvurusu

| # | Aktör ne yapar | Sistem ne denetler, ne yapar | Durum | Bildirim | Kaynak |
|---|---|---|---|---|---|
| 9.4.1 | Üye hesabını silmek ister | Yeniden doğrulamadan geçer (9.2.6). E-posta değişikliğinin geri alma bağlantısı açıkken hesap silinmez: ekran sebebini ve bağlantının bitiş tarihini söyler (K-698). Yürüyen sipariş silmeyi engellemez. Silme geri alınmaz | — | — | `02 §3.13.13`, §3.15.2, §3.15.6 · K-698 |
| 9.4.2 | Sistem hesabı siler | Hesabın adı, giriş bilgileri — şifre ve Google bağı —, açık oturumları, adres defteri ve sepeti hemen silinir (Z-31, Z-32) ve silme imha kaydına yazılır (Z-44; K-708); tanınan tarayıcı işaretleri geçersizleşir, bekleyen bir e-posta değişikliği düşer. Siparişler kendi kaydıyla durur; kişisel verileri yasal saklama süresi dolunca imha edilir (Z-28) ve hiçbir hesaba yeniden bağlanmaz | Hesap silinir · siparişlerin durumları değişmez | Hesabın silindiği bildirimi → hesabın adresi (`02 §9.4`; K-693) | `02 §3.15.1`, §3.15.3, §8.2.6, §12.2.7 · K-693, K-708 |
| 9.4.3 | Müşteri silinmiş hesabın açık siparişini izler | Takip, iptal, cayma, iade ve ayıp talebi misafir yolundan — sipariş numarası ve siparişe donmuş e-posta — ya da sipariş e-postasındaki bağlantıyla yürür (§2.6.6) | — | Siparişin olağan bildirimleri — siparişin iletişim e-postasına | `02 §3.15.2`, §3.22.3 |
| 9.4.4 | Ziyaretçi ya da müşteri — ilgili kişi olarak — kişisel verisiyle ilgili başvurur | Ürünün sunduğu kanal iletişim formunun "KVKK talebi" tipidir (§2.2.8); Tebliğ'in öteki başvuru yolları açık kalır ve aydınlatma metninde sayılır. Ayrı KVKK modülü, süre sayacı ve uygulama içi veri indirme yoktur; otuz günlük cevap süresi (Z-25) firmanın yükümlülüğüdür. Başvurunun unsurlarını tamamlatmak ve başvuranın kimliğini cevaptan önce doğrulamak firmanın yükümlülüğüdür | İletişim talebi: Açık | F-2 → firma (§2.2.8) | `02 §3.15.4`, §3.15.5, §12.2.6 |
| 9.4.5 | Yönetici üye kaydı görünümünde e-postayla arar | Görünüm salt okunurdur: hesabın varlığı ve adı, adres defteri, giriş yolları (şifre, Google) ve bağlı siparişler görünür; siparişler sipariş listesinden okunur. Cevabı firma sistemin dışında e-postayla verir ve talebi kapatır (§8.6.4.3, §8.6.4.4) | — | — | `02 §3.15.5`, §10.1.2 |
| 9.4.6 | Yönetici ilgili kişinin talebi üzerine hesabı siler | Silme 9.4.2'nin rejimiyle yapılır ve geri alınamaz onay ister (§1.6.1.8; K-715). E-posta değişikliğinin geri alma bağlantısı açıkken silinmez: görünüm sebebini ve bağlantının bitiş tarihini söyler (K-698). Silme işlem izine yazılır — satır hesabı değil, işlemi ve tarihini adlandırır. Sipariş başına elle adım değildir (§8.5.7) | Hesap silinir | Hesabın silindiği bildirimi → hesabın adresi (K-693) | `02 §3.15.5`, §3.15.6, §10.3.1, §5.9 · K-693, K-698, K-715 |
| 9.4.7 | Ziyaretçi aynı e-postayla yeniden kaydolur | Yeni bir hesaptır (9.1.2); silinmiş hesabın siparişleri ona bağlanmaz | — | 9.1.2'nin e-postası | `02 §3.15.3` |

### 9.5 Tercih yönetimi

Şablonun saydığı tercih yönetiminin ürünün içinde karşılığı yoktur; her tercih "yoktur" diye ve gerekçesinin evine işaret edilerek yazılır (§0.1.3).

| # | Aktör ne yapar | Sistem ne denetler, ne yapar | Durum | Bildirim | Kaynak |
|---|---|---|---|---|---|
| 9.5.1 | Üye hangi bildirimleri alacağını seçmek ya da bildirimleri kapatmak ister | Yoktur: işlem bildirimleri ticari elektronik ileti değildir ve kapatılamaz; hesapta bildirim tercihi ekranı yoktur. Tek kanal e-postadır | — | — (`02 §9.1.3`) | `02 §9.1.1`, §9.1.3 |
| 9.5.2 | Üye kampanya ve bülten e-postası almak ister | Yoktur: kayıtta ve hesapta pazarlama onayı alanı tutulmaz; ürün pazarlama iletisi göndermez ve İYS'ye bağlanmaz | — | — | `02 §3.13.18`, §9.5 |
| 9.5.3 | Ziyaretçi sitenin dilini seçmek ister | Yoktur: tek dil Türkçedir; dil bir ayar değildir | — | — | `02 §12.1.1` · `10` SK-2 |
| 9.5.4 | Ziyaretçi çerez tercihini seçmek ister | Yoktur: sitede yalnız zorunlu çerezler vardır ve çerez onay bandı yoktur; çerez politikası altbilgidedir (§2.2.6) | — | — | `02 §3.33.5`, §12.2.5 |
| 9.5.5 | Üye verisini hesabından indirmek ister | Yoktur: uygulama içi veri indirme yoktur; yol kişisel veri başvurusudur (9.4.4) | — | — | `02 §3.15.5` |
| 9.5.6 | Yönetici firma bildirimlerinin adresini ya da hangilerinin geleceğini seçmek ister | Yoktur: firmaya giden bildirimler firma kimliğindeki iletişim e-postasına gider ve kapatılamaz; panelde bildirim tercihi ve ayrı bildirim adresi yoktur. F-5 ve F-6 her yöneticinin kendi adresine gider (§7.1.46–§7.1.48) | — | — (`02 §9.1.3`) | `02 §9.1.3`, §9.3.1 |

## 10. Operasyonel akışlar

> **Ne yazılır:** Platform bakımı, dış servis kesintisi, bakım modu. Kesinti sırasında sürelerin dondurulması, kullanıcı bilgilendirmesi ve normale dönüş adımları.

Bu bölüm ürünün dış servisleri — ödeme sağlayıcısı, e-posta altyapısı, Google — çalışmadığında, firma satışı durdurmak istediğinde, sitenin kendisi kesintideyken ve firma panele erişimini kaybettiğinde akışların nasıl sürdüğünü yazar. Ürün çalışma süresi taahhüdü vermez; kesinti olağan kabul edilir ve barındırma kurulum tarafındadır (`02 §3.34.7`). **Sürelerin kesintideki kaderi yalnız §4'tedir** (4.2.11, 4.2.12; K-666, K-667): bu bölüm ona işaret eder, tekrarlamaz.

- **Biçim:** adım tablosu §0.4.1'dir; alt başlıklı alt bölümde satır dört düzeylidir (10.1.1.1). Hataların müşteri yüzü §3'tedir; buradaki satır oraya işaret eder.
- **Kullanıcı bilgilendirmesi:** ürün kesinti, bakım ya da normale dönüş için bildirim göndermez — kesinti bildirimi `02 §9`'un matrisinde yoktur ve duyuru şeridi gönderilmez (`02 §9.5`); firmanın ziyaretçiye söyleme yolu duyuru şerididir (§8.6.3). Kesintinin teknik tespiti — sağlayıcının erişilemezliği `08`'in, sitenin kesintisi `05`'in işidir (K-667'nin park satırı).

### 10.1 Dış servis kesintileri

#### 10.1.1 Ödeme sağlayıcısı

| # | Aktör ne yapar | Sistem ne denetler, ne yapar | Durum | Bildirim | Kaynak |
|---|---|---|---|---|---|
| 10.1.1.1 | Müşteri ödeme adımında kartı seçmek ister; sağlayıcıya erişilemez | Kart "şu an kullanılamıyor" olarak görünür ve seçilemez. Havale açıksa müşteri ona yönelir; değilse sipariş verilemez ve müşteri daha sonra denemesi söylenerek sepetine döner — sepet korunur (§3.2.1.15). Erişilemezliğin tespiti `08`'in işidir | — | — | `02 §6.2.15`, §6.1.4 |
| 10.1.1.2 | Sipariş onaylandıktan sonra sağlayıcıya bağlanılamaz; ödeme sayfası açılmaz | Bu kart ödemesinin başarısızlığıdır: sipariş kendiliğinden iptal edilir, ayrılanlar serbest kalır ve sipariş müşterinin listesinde görünmez; iptal L-7'ye sayılmaz. Sepet korunur | → İptal edildi + Başarısız (S3, Ö2) | B-7 → müşteri | `02 §6.2.15`, §8.2.3 |
| 10.1.1.3 | Kart ödeme süresi (Z-7) dolar; sağlayıcı son sorguya yanıt vermez | Sipariş iptal edilmez: Alındı + Bekliyor'da kalır, ayrılanlar tutulur, panelde "ödeme sonucu alınamadı" işareti düşer ve sorgu sağlayıcı yanıt verene kadar yinelenir. **Kalan risk:** uzun kesintide stok ve kupon hakları kesinti boyunca kilitli kalır | Alındı + Bekliyor | — (`02 §9.2`; K-683) | `02 §6.1.2` · K-683 |
| 10.1.1.4 | Sağlayıcı geri döner | Bekleyen sorgunun sonucu gelir: ödeme alınmışsa sipariş onaylanır, alınmamışsa kendiliğinden iptal edilir; işaret kalkar. Kart ödeme adımında yeniden görünür ve seçilir. İptal edilmiş bir siparişe sonradan başarı bildirimi düşerse sistem ödemeyi kendiliğinden geri öder (§3.2.1.16) | → Ödendi (Ö1) ya da → İptal edildi + Başarısız (S3, Ö2) | Ö1'de B-4 · iptalde B-7 → müşteri · geç gelen ödemenin iadesinde B-8 → müşteri, F-4 → firma | `02 §6.1.2`, §6.2.16 · K-675, K-684 |
| 10.1.1.5 | Kesintide sistemin başlattığı kart iadesi sağlayıcıda gerçekleşmez | Ödeme ekseni değişmez; panelde "geri ödeme gerçekleşmedi" işareti düşer ve sistem kendiliğinden yeniden denemez. Sağlayıcı dönünce yönetici önce sağlayıcının panelinde iadenin gerçekleşip gerçekleşmediğine bakar — kesintide gönderilen istek orada işlenmiş olabilir (K-705) —, ardından iadeyi yeniden dener ya da müşteriye havale yolunu açar (§8.3.3.4, §8.3.3.5); geri ödemenin süresi işlemeye devam eder | — · kart iadesi gerçekleşince Ö3–Ö5 | Başarısızlıkta — (`02 §9.2`; K-683) · havale yolu açılınca B-14 · kart iadesi gerçekleşince B-8 → müşteri | `02 §7.2.8` · K-683, K-705 |
| 10.1.1.6 | Yönetici kesinti sürerken satışı sürdürmek ya da durdurmak ister | Havale açıksa satış havaleyle sürer. Satışı durdurmanın yolu geçici kapatmadır (§8.7.5.1); ziyaretçiye söylemenin yolu duyuru şerididir (§8.6.3.1). Ürün kesintiyi müşterilere bildirmez | — | — (§7.2.28) | `02 §3.1.6`, §3.21.5, §9.5 |
| 10.1.1.7 | Kurulum tarafı ödeme sağlayıcısının anahtarlarını ya da sağlayıcıyı değiştirir | Panel işlemi değildir. Açık kart siparişlerine — bekleyen son sorgulara, geç gelen ödemelere, kendiliğinden kart iadelerine — etkisi ve geçişin yöntemi `08`'in ve `DEPLOY_RUNBOOK`'un işidir | — | — | `02 §3.21.4`, §10.1.3 |

#### 10.1.2 E-posta altyapısı

| # | Aktör ne yapar | Sistem ne denetler, ne yapar | Durum | Bildirim | Kaynak |
|---|---|---|---|---|---|
| 10.1.2.1 | Sistem bir e-postayı gönderemez | Gönderim Z-26'nın aralıklarıyla üç kez yeniden denenir. Sipariş, ödeme ve durum geçişleri etkilenmez; sipariş sayfası her bilgiyi — onaylanan Ön Bilgilendirme Formu ve sözleşme sürümleri dahil — taşır (§2.6.4, §3.1.1) | — | — | `02 §6.1.1`, §9.1.6 |
| 10.1.2.2 | Üç yeniden deneme de başarısız olur | Müşteriye giden bildirimde siparişin, davette davetin panel satırına "e-posta ulaşmadı" işareti düşer ve hangi e-postanın ulaşmadığını söyler; firma bildirimleri (F-1…F-4) satıra işaret düşürmez — panelin ana sayfasında "firma bildirimleri ulaşmıyor" uyarısı çıkar ve bildirimlerin gittiği adresi gösterir (§8.9.1; `02 §9.1.6`). Hesap e-postalarında, F-5'te ve F-6'da işaret düşmez — bağlanacağı satır yoktur; kullanıcı hesap bağlantısını ekrandan yeniden ister (9.1.3, 9.2.4) | — | — | `02 §9.1.6`, §9.3.3, §9.3.4, §9.4 · K-760 |
| 10.1.2.3 | Gönderim art arda başarısız olur | Panelin ana sayfasında kanal düzeyinde uyarı çıkar: *"e-posta gönderilemiyor — kurulum ayarını kontrol edin"*. Uyarının eşiği `05`'in tasarım girdisidir. Uyarı satış kapısına bağlı değildir; satışı durdurmanın yolu geçici kapatmadır (§8.7.5.1) | — | — | `02 §6.1.1`, §9.1.6, §10.6.1 |
| 10.1.2.4 | Müşteri kesinti sürerken sipariş verir ya da siparişini izler | Sipariş akışı e-postaya bağlı değildir: üye hesabından, misafir alıcı sipariş numarası ve e-postasıyla sipariş sayfasına girer (§2.6.1, §2.6.2) | — | Gönderilemeyen e-posta 10.1.2.1'in yolundan | `02 §6.1.1`, §3.22.3 |
| 10.1.2.5 | Kullanıcı kesinti sürerken bir hesap bağlantısı bekler — kayıt doğrulaması, şifre sıfırlama, yeni adresin doğrulanması — ya da yönetici daveti gönderilir | Bağlantılar adresin sahibini kanıtlamak için bilerek e-postaya bağlıdır; bağlantı ulaşmadan işlem tamamlanmaz. Kullanıcı altyapı dönünce bağlantıyı yeniden ister (L-9, L-2); Z-1 dolmuşsa kayıt silinmiştir ve kullanıcı yeniden kaydolur. Ulaşmayan davetin yolu yeni davettir (§8.8.1). **Kalan risk:** kesintide şifresini unutan kullanıcı — yönetici de — altyapı dönene kadar giremez | — | — | `02 §6.1.1`, §9.4 |
| 10.1.2.6 | Altyapı döner; yönetici "e-posta ulaşmadı" işaretli satırları görür | Üç denemeden sonra ürün kendiliğinden yeniden göndermez. Yönetici işaretli siparişleri panelin ana sayfasındaki satırdan, sipariş listesinin süzülmüş hâliyle bulur (§8.9.1) ve e-postayı siparişin ayrıntısından yeniden gönderir (§8.9.2) — sipariş e-postasında donmuş sürüm gider —; başarılı gönderimle işaret kalkar, gönderilmeyenin işareti kalır | — | Yeniden gönderilen bildirim, ilk alıcısına | `02 §6.1.1`, §9.1.6, §10.6.1 · K-675, K-761 |

#### 10.1.3 Google

| # | Aktör ne yapar | Sistem ne denetler, ne yapar | Durum | Bildirim | Kaynak |
|---|---|---|---|---|---|
| 10.1.3.1 | Üye Google ile girmek ister; Google'a ulaşılamaz | Google ile giriş tamamlanmaz. Şifresi olan hesap şifreyle girer (9.2.1); sipariş girişsiz de verilir (§2.4) | — | — | `02 §3.13.7`, §3.13.8, §3.13.1 |
| 10.1.3.2 | Google ile açılmış, şifresi olmayan hesabın sahibi Google'a ulaşamadan girmek ister | "Şifremi unuttum" ile şifre belirler (9.2.4, 9.2.5) — e-posta altyapısı çalışıyorsa — ve bundan sonra iki giriş yolunu kullanır. Şifresi olmayan hesapta yeniden doğrulama Google ile yapıldığından şifre konana ya da Google dönene kadar yeniden doğrulama isteyen işlemler (9.2.6) yapılamaz | — | Şifre sıfırlama bağlantısı → hesabın adresi (`02 §9.4`) | `02 §3.13.8`, §3.13.13 |
| 10.1.3.3 | Google uygulaması kurulumda tanımlı değildir ya da tanımı kaldırılmıştır | "Google ile giriş" düğmesi görünmez ve üyelik e-posta ve şifreyle yürür; şifresi olmayan hesap 10.1.3.2'nin yoluyla şifre koyar. Panelde Google girişinin açık/kapalı anahtarı yoktur | — | — | `02 §3.13.6`, §10.1.3 |
| 10.1.3.4 | Yönetici Google kesintisinde panele girer | Panel etkilenmez: panelde Google ile giriş yoktur (9.2.7) | — | — | `02 §10.2.5` |

### 10.2 Bakım ve güncelleme

| # | Aktör ne yapar | Sistem ne denetler, ne yapar | Durum | Bildirim | Kaynak |
|---|---|---|---|---|---|
| 10.2.1 | Yönetici bir süre yeni sipariş almamak ister — sayım, tatil, tedarik | Satışı geçici olarak kapatır (§8.7.5.1): sepete ekleme ve ödeme kapanır, vitrin ve kurumsal içerik yayında kalır, müşterinin sepeti korunur; açık siparişler yürür. **Bakım modu — sitenin tamamen kapanması — yoktur**; panelde siteyi kapatan bir yol bulunmaz | Satış kapalı | — (`02 §9.3.2`) | `02 §3.1.6`, §10.1.2 · `10` KD-60 |
| 10.2.2 | Yönetici ziyaretçiyi bir kapanış ya da gecikme konusunda bilgilendirmek ister | Duyuru şeridini — isteğe bağlı başlangıç ve bitiş tarihiyle (Z-23) — yayına alır (§8.6.3.1). Duyuru sitede durur; kimseye e-postayla gönderilmez | Duyuru: Yayında | — (`02 §9.5`) | `02 §3.27.21`, §9.5 |
| 10.2.3 | Kurulum tarafı ürünü günceller | İlan edilen, sitenin kapatıldığı planlı bakım penceresi yoktur; güncelleme sırasındaki kısa kesinti kesinti kabulünün içindedir ve 10.3'ün akışı işler. Güncelleme panelin işi değildir | — | — | `02 §3.34.7`, §3.1.6 |
| 10.2.4 | Kurulum tarafı veriyi yedekler ya da yedekten geri döner | Ürünün akışı değildir. Sipariş tarafında veri kaybı toleransı sıfırdır — onaylanmış sipariş, ödeme, durum geçişleri, kalem kayıtları, iletişim talepleri, donmuş yasal metinler, onay kutularının kaydı ve işlem izi saklama süreleri boyunca kaybolmaz; sepet ve oturum kaybı kabul edilir. Yedeklemenin sıklığı, yöntemi ve kurtarma süresi `05`'in ve `DEPLOY_RUNBOOK`'un işidir; veritabanı ve yedekler firmanın elindedir | — | — | `02 §3.34.8` · `10` ÖK-2 |

### 10.3 Sitenin kesintisi

Sürelerin ve kendiliğinden işlerin kesintideki kuralı §4.2.11 ve §4.2.12'dedir; ödeme sağlayıcısının kesintisi 10.1.1'dedir. Satırlar o kuralların akıştaki yerini gösterir.

| # | Aktör ne yapar | Sistem ne denetler, ne yapar | Durum | Bildirim | Kaynak |
|---|---|---|---|---|---|
| 10.3.1 | Ziyaretçi, müşteri ya da yönetici siteye ulaşamaz — barındırma kesintisi ya da güncelleme | Ürün çalışmaz: vitrin, sipariş sayfası, panel ve kendiliğinden işler durur. Ürünün içinden bilgilendirme yoktur — erişilebilirlik barındırmaya bağlıdır | — | — | `02 §3.34.7` |
| 10.3.2 | Kesinti sürer | Süreler dondurulmaz — bağlantı ömürleri, oturumlar, ödeme ve kargoya verme süreleri, saklama süreleri işler (§4.2.11). Yasal süreler hiçbir durumda dondurulmaz; müşterinin caymayı ve gecikme feshini başka kanaldan bildirme yolu açıktır ve tarih bildirimin firmaya ulaştığı tarihtir (§2.8.4.4, §8.4.8; K-706) | — | — | `02 §3.34.7`, §4.3, §10.4.10 · K-666, K-706 |
| 10.3.3 | Site döner | Kesintide zamanı gelen kendiliğinden işler kaçırdıkları sırayla hemen çalışır — kendiliğinden iptal (kart hattında iptal öncesi son sorguyla; havale ödeme süresi kesintide dolduysa iptal ertelenir — 10.3.4), havale hatırlatması (süre hâlâ işliyorsa), IBAN'ın silinmesi, periyodik imha ve e-postanın yeniden denenmesi (§4.2.11) | İşin kendi geçişi | Dönüşte çalışan işin kendi bildirimi (§7.1.6, §7.1.15) | `02 §4.3`, §3.21.8 · K-666, K-667 |
| 10.3.4 | Havale siparişinin ödeme süresi kesinti sırasında dolmuştur | İptal sitenin dönüşünden sonraki ilk iş gününün sonuna ertelenir; ayrılanlar tutulur ve sipariş sayfası yeni son günü gösterir. Yönetici parayı görmüşse o ana kadar "ödendi" işaretler (§8.2.2); işaretlenmezse iptal ertelenen anda işler (§4.2.12) | Alındı + Bekliyor · → Ödendi (Ö1) ya da → İptal edildi + Başarısız (S3, Ö2) | Ertelemede — (`02 §4.2` Z-8; K-667) · Ö1'de B-4 · iptalde B-7 → müşteri | `02 §3.21.8`, §4.2 Z-8 · K-667 |
| 10.3.5 | Yönetici dönüşte panele girer | Bekleyen işler sayaçları mevcut durumlardan hesaplanır ve güncel hâli gösterir (§8.9.1). Kesinti sırasında başka kanaldan gelen cayma bildirimi firmaya ulaştığı tarihle kaydedilir (§8.4.8) | — | — | `02 §10.6.1`, §10.4.10 |
| 10.3.6 | Yönetici kesintiyi müşterilere duyurmak ister | Ürün kesinti ya da normale dönüş bildirimi göndermez; ertelenen havale iptali ayrıca bildirilmez (§7.2.14). Firmanın yolu duyuru şerididir (§8.6.3.1) | — | — (§7.2.28) | `02 §9`, §4.2 Z-8 · K-667 |

### 10.4 Panele erişimin kaybı

Platform operatörü ve operatör paneli yoktur (`02 §10.1.1`; `10` KD-21); panelin tek kapısı yönetici hesaplarıdır. Panele erişim kaybolduğunda kurtarma ürünün dışındadır ve kurulum tarafının işidir (`02 §10.2.7`; K-699); yöntemi `DEPLOY_RUNBOOK`'tadır (park).

| # | Aktör ne yapar | Sistem ne denetler, ne yapar | Durum | Bildirim | Kaynak |
|---|---|---|---|---|---|
| 10.4.1 | Yönetici şifresini unutur | Panelin şifre sıfırlamasını kullanır (9.2.8); bağlantı yöneticinin kendi adresine gider | Şifre yenilenir | Şifre sıfırlama bağlantısı → yöneticinin adresi (`02 §9.4`) | `02 §10.2.4` |
| 10.4.2 | Yönetici girişte L-1'in engeline takılır — biri onun e-postasıyla şifre denemektedir | Tanınan tarayıcısından girmeye devam eder; yeni bir tarayıcıda şifre sıfırlama bağlantısını kullanır; engel kendiliğinden açılır (§6.1.3) | — | — | `02 §8.2.4`, §8.2.6 |
| 10.4.3 | Bir yönetici hesabına erişimini kaybeder; panele giren başka bir yönetici vardır | Kalan yönetici yeni adrese davet gönderir (§8.8.1) ve erişimi kaybedilen hesabı kaldırır (§8.8.5); kurtarma gerekmez | Yönetici daveti · yönetici hesabı kalkar | Yönetici daveti → davetli · F-6 → bütün yöneticiler | `02 §10.2.1`, §10.2.2, §10.2.7 · K-692 |
| 10.4.4 | **Kurulum tarafı** firmanın başvurusu üzerine yeni bir yönetici hesabı açar — hiçbir yönetici panele giremez: tek yöneticinin şifresi ve e-posta kutusu kaybolmuştur ya da yönetici firmadan ayrılmıştır | Ürünün içinde kurtarma kodu, operatör paneli ve "yönetici erişimimi kurtar" akışı yoktur. Hesap ilk yöneticinin açıldığı yolla açılır (`10` ÖK-7). Kurulum tarafının işlemi panel işlemi değildir: işlem izine yazılmaz, kaydı barındırma tarafındadır ve bildirim üretmez (K-702). **Kalan risk:** kurulumu yapana ulaşamayan firma panele dönemez | Yönetici hesabı doğar | — (`02 §10.2.7`; K-699) | `02 §10.2.7` · K-699, K-702 |
| 10.4.5 | **Kurulum tarafı** yeni bir yönetici hesabı açar, ele geçirilmiş hesabın oturumlarını sonlandırır ve şifresini geçersiz kılar — ele geçirilmiş bir yönetici hesabı öteki yöneticileri kaldırmış ve tek yönetici kalmıştır (§6.2.6.4) | Kaldırılan yöneticiler kaldırılışı F-6'dan öğrenmiş ve firma kurulumu yapana başvurmuştur (K-699). Z-37 firmanın durumu öğrendiği andan işler, kurtarma beklenirken de (§6.3.2.1) | Yönetici hesabı doğar · ele geçirilmiş hesabın oturumları kapanır | — (`02 §10.2.7`; K-699) | `02 §10.2.7`, §9.3.4 · K-692, K-699 |
| 10.4.6 | Yeni yönetici panele girer ve paneli geri alır | Ele geçirilmiş ya da erişimi kaybedilmiş hesabı kaldırır (§8.8.5) — gönderdiği kullanılmamış davetler düşer —; havale IBAN'ını denetler, yetkisiz değişmişse düzeltir (§8.7.3.2), yetkisiz IBAN'la onaylanmış siparişleri firma iptaliyle kapatır (§6.2.6.3); bekleyen davetleri geri çeker (§8.8.3) ve kaldırılmış yöneticileri yeniden davet eder (§8.8.1). İşlem izini tarih ve yönetici süzgeciyle okur (§6.3.2.2) ve ihlal yükümlülüklerini değerlendirir (§6.3.2.1) | Yönetici hesabı kalkar · ödenmemiş siparişlerin iptalinde → İptal edildi + Başarısız (S3, Ö2) | Kaldırmada ve davette F-6 → bütün yöneticiler · davette yönetici daveti → davetli · IBAN düzeltmesinde F-5 → bütün yöneticiler · iptalde B-7 → müşteri | `02 §10.2.2`, §10.2.6, §10.2.7, §9.3.3, §6.9.14 · K-663, K-692, K-699 |

## 11. `02`'ye geri besleme

> **Ne yazılır:** Akışlar yazılırken fark edilen gereksinim boşlukları ve `02`'de yapılan düzeltmeler.

Her satır bir karar grubudur; karar kaydı satırı ayrıntıyı, sürüm notu değişen yerleri taşır (K-652).

| # | Bulgu | `02`'de yapılan değişiklik |
|---|---|---|
| 11.1 | Workshop, AK3-06: yöneticinin ayrılmış adede dokunan katalog işlemlerinin sonucu yazılı değildi (K-653, K-654, K-655) | v0.48 — yeni `02 §3.6.7`, §3.10.6, §3.7.7, yeni 6.6.14…6.6.16 · `10` v0.30 KP-39, KP-40, KP-43 |
| 11.2 | Workshop, AK5-03 ⚠: iade malının reddi ve değer kaybıyla dönen mal yazılı değildi (K-656, K-657, K-668) | v0.48 — `02 §1.2` yeni terim, Z-35, Z-43, §5.8, 6.4.4, yeni 6.4.35, yeni 6.4.36, §7.3.4, §7.4.7, yeni §7.4.9, yeni §7.4.10, §9.1.6, yeni B-16, §10.1.2, §10.3.1, §10.5.2, §10.6.1, §10.6.3 · `10` v0.30 KP-47, KP-66 |
| 11.3 | Workshop, AK6-04: doğrulama bağlantısının yeniden istenmesinin limiti yoktu (K-658, K-659) | v0.48 — `02 §3.13.5`, §3.13.17, §8.2.2, L-9, §11 girişi, P-49, Z-1, yeni 6.5.14 · `10` v0.30 KP-64, KP-72 |
| 11.4 | Workshop, AK8-09 ⚠: bekleyen yönetici davetinin geri çekilmesi, ikinci davet ve gönderenin kaldırılması yazılı değildi (K-660, K-661, K-662) | v0.48 — `02 §1.2`, yeni §10.2.6, §10.1.2, §10.2.2, §10.3.1, Z-3, 6.9.12 · `10` v0.30 KP-62, KP-63 |
| 11.5 | Workshop, AK8-10 ⚠: donmayan ayarların — havale IBAN'ı, satış kapısı, iade adresi — yürüyen siparişe etkisi yazılı değildi (K-663, K-664, K-665) | v0.48 — `02 §3.21.5`, §3.21.6, §3.22.4, §3.23.3, §3.1.6, §3.1.7, §5.8, §7.3.5, §7.4.4, B-2, B-9, yeni 6.9.14…6.9.16 · `10` v0.30 KP-59, KP-60 |
| 11.6 | Workshop, AK10-03 ⚠: sitenin kesintisinde sürelerin ve kendiliğinden işlerin kaderi yazılı değildi (K-666, K-667) | v0.48 — `02 §3.34.7`, §4.3, §3.21.8, Z-8, yeni 6.7.17 · `05` park satırı |
| 11.7 | Yazım, §1.2: hizmet tamamlamanın ve S2'nin "ödeme Ödendi iken" koşulu, kısmi geri ödemeden sonra hizmetin tamamlanmasını kapatıyor ve siparişi çıkışsız bırakıyordu (K-671) | v0.49 — `02 §3.20.9`, §5.4, §5.6.1 (S2) |
| 11.8 | Yazım, §1.4.3: Alındı ya da Hazırlanıyor'da son açık kalem cayma beyanıyla ya da çıkarmayla kapandığında, hiçbir kalemi teslim edilmemiş siparişin kapanışı yazılı değildi — sipariş çıkışsız kalıyordu (K-672) | v0.49 — `02 §5.4`, §5.6.6, §5.8, §9.2 (S3, S4, B-7) |
| 11.9 | Yazım, §1.11: hizmet kaleminin cayma düğmesinin ödeme beklenirken açık olup olmadığı yazılı değildi (K-673, ⚠) | v0.49 — `02 §7.3.3`, §10.4.10 · `10` v0.31 KP-22 |
| 11.10 | Yazım, §1.8: yayındaki ürünün son Yayında varyantının kalıcı silinmesi yayın kapısını bozuyordu (K-674) | v0.49 — `02 §3.7.1`, §3.7.8, §5.1 |
| 11.11 | Yazım, §1.7: panel işaretlerinin ne zaman kalktığı yazılı değildi (K-675) | v0.49 — `02 §6.1.1`, §6.1.2, §7.2.8, §7.4.1, §10.4.11 |
| 11.12 | Yazım, §1.9: bir kalemde ikinci ayıp talebinin açılıp açılamayacağı yazılı değildi (K-676) | v0.49 — `02 §5.10` |
| 11.13 | Yazım, §1.6: yeniden gönderim (S8) paketi yola çıkarır ve takip bilgisi gönderir ama geri alınamaz onayı yalnız S5'te yazılıydı (K-677) | v0.49 — `02 §5.9` |
| 11.14 | Yazım, §2.2: iletişim formunu gönderen kişiye alındı e-postası gidip gitmediği yazılı değildi — olay `02 §9.2`'nin matrisinde de "bildirim üretmeyen olaylar"da da yoktu (K-678, ⚠) | v0.50 — `02 §3.32.6`, §9.5 |
| 11.15 | Yazım, §2.4: bir siparişe birden çok kupon kodunun uygulanıp uygulanamayacağı yazılı değildi; sipariş düzleminde tek "kupon kodu" alanı vardı (K-679) | v0.50 — `02 §3.10.1` |
| 11.16 | Yazım, §2.4: üyenin ödeme adımında yazdığı yeni adresin adres defterine girip girmediği yazılı değildi (K-680, ⚠) | v0.50 — `02 §3.14.1` · `10` v0.32 KP-12 |
| 11.17 | Yazım, §2.9: müşterinin çözülmüş ayıp talebini yeniden açmasında kendisine kayıt kopyası gidip gitmediği yazılı değildi — firmaya F-3 gidiyordu (K-681, ⚠) | v0.50 — `02 §9.2` B-9 · `10` v0.32 KP-66 |
| 11.18 | Yazım, §3: beş hata ve sistem olayı — kart iadesinin başarısız olması, son sorgunun yanıtsız kalması, indirme hakkının yenilenmesi, dosya güncellemesi, doğrulanmamış kaydın silinmesi — ne bildirim matrisinde ne "bildirim üretmeyen olaylar"daydı (K-683, ⚠) | v0.51 — `02 §9.2`, §9.4 |
| 11.19 | Yazım, §3.2: sistemin iptal edilmiş siparişe gelen kart ödemesini geri ödemesi müşteriye bildirilmiyordu (K-684, ⚠) | v0.51 — `02 §6.2.16`, §9.2 B-8 · `10` v0.33 KP-15, KP-66 |
| 11.20 | Yazım, §4: havale işaretinin düzeltilmesiyle yeniden başlayan ödeme süresinde hatırlatmanın gidip gitmediği yazılı değildi (K-685) | v0.51 — `02 §3.21.6`, Z-9, §9.2 B-3, §10.4.6 |
| 11.21 | Yazım, §5: havale hattında IBAN'ı olmayan geri ödemelerde — ayıp talebinin çözümü, tutar bazlı kısmi geri ödeme, ret ya da "mal dönmedi" kapanışından sonraki ödeme — IBAN'ın nasıl alınacağı yazılı değildi (K-686, ⚠) | v0.51 — `02 §5.5`, §7.4.5, §7.4.9, §7.5.3, §8.4.2, §9.2 B-14, §10.1.2, §10.3.1, §10.4.9, §10.6.1 · `10` v0.33 KP-48, KP-66 |
| 11.22 | Yazım, §6.3: giriş kaydının nerede okunduğu yazılı değildi (K-687, ⚠) | v0.51 — `02 §8.5.1`, §12.2.9 · `05` park satırı |
| 11.23 | Yazım, §6.2: kandırılmayla ya da yanlışlıkla kaydedilmiş cayma beyanının geri alınıp alınamayacağı yazılı değildi (K-688, ⚠) · hizalama: `02 §6.7.14` hizmet tamamlamanın koşulunu K-671'den önceki diliyle yazıyordu (K-671) | v0.51 — yeni `02 §7.6.7`, §8.3.8, §10.4.10 · `02 §6.7.14` |
| 11.24 | Yazım, §8.3: havale hattında ödenmiş kalemin firma iptalinde ve kalem çıkarmasında geri ödemenin IBAN'ının nereden geleceği yazılı değildi — müşteri IBAN'ı yalnız kendi iptalinde, feshinde ve caymasında giriyordu (K-690, ⚠) | v0.52 — `02 §5.8`, §7.2.8, §7.4.5, §9.2 B-14, §10.4.3, §10.6.1 · `10` v0.34 KP-47, KP-48 |
| 11.25 | Yazım, §7: F-4'ün olayı iptal edilen kalemin iadesini sayıyordu; gecikme feshinin ve kalem çıkarmasının kendiliğinden kart iadesi — §2.7.8 ve §8.3.3.1 — kaynaksız kalıyordu (K-691) | v0.52 — `02 §9.3` F-4 |
| 11.26 | Yazım, §8.8: yönetici daveti ve yöneticinin kaldırılması kimseye bildirilmiyordu; ele geçirilmiş bir hesap öteki yöneticileri kaldırıp IBAN'ı değiştirdiğinde F-5 yalnız ona gidiyordu (K-692, ⚠) | v0.52 — `02 §1.2`, §9.3 yeni F-6, §9.3.1, §9.3.2, yeni §9.3.4, §10.2.2, §10.2.6 · `10` v0.34 KP-62, KP-67 |
| 11.27 | Yazım, §9.4: hesabın silinmesinin bildirimi yazılı değildi — olay `02 §9.4`'te de "bildirim üretmeyen olaylar"da da yoktu (K-693, ⚠) · e-posta değişikliğini geri alma bağlantısı açıkken hesabın silinmesi eski adresin geri alma yolunu boşa düşürüyordu (K-698, ⚠) | v0.53 — `02 §3.13.14`, §3.15.1, §3.15.5, yeni §3.15.6, §9.4, §10.1.2, yeni 6.5.15 · `10` v0.35 KP-29, KP-77 |
| 11.28 | Yazım, §9.1–§9.3: hesap olaylarının — doğrulama, Google ile açılış ve bağlama, giriş ve çıkış, şifre değişikliği, ad ve adres defteri, davetlinin hesap açması — bildirimi ya da yokluğu yazılı değildi (K-694, ⚠) · hesaptan şifre değiştirmenin oturumlara ve tanınan tarayıcı işaretlerine etkisi yazılı değildi (K-695) | v0.53 — `02 §1.2`, §3.13.10, Z-46, §8.2.6, §9.4 · `10` v0.35 KP-27 |
| 11.29 | Yazım, §9.1: Google ile ilk girişin doğrulanmamış bekleyen bir kayda eşleşmesi kaydı açanın şifresini hesaba taşıyabiliyordu (K-696, ⚠) · Google'ın e-postayı doğrulanmamış verdiği girişte yeni hesabın açılıp açılmadığı yazılı değildi (K-697) | v0.53 — `02 §1.2`, §3.13.7, §9.4, 6.5.7, yeni 6.5.16 · `10` v0.35 KP-26 |
| 11.30 | Yazım, §10.4: hiçbir yöneticinin panele giremediği hâlde — K-692'nin senaryosu, ele geçirilmiş hesabın tek yönetici kalması dahil — panele dönüş yolu yazılı değildi (K-699, ⚠) | v0.53 — `02 §10.1.1`, yeni §10.2.7 · `10` v0.35 ÖK-7 · `DEPLOY_RUNBOOK` park satırı |
| 11.31 | Kalite döngüsü, §1.6.2: havale "ödendi" işaretinin düzeltilmesi ödenmemiş siparişte bir iptal kaydı ve hiç gelmemiş paranın geri ödeme borcunu bırakabiliyordu; yanlış aralıkta doğan cayma ve ayıp kayıtlarının ve kapanışın düzeltmedeki yeri yazılı değildi (K-703, ⚠) | v0.54 — `02 §10.4.6` |
| 11.32 | Kalite döngüsü, §2.4–§2.5: kart hattında sitede siparişin alındığı ve sipariş numarası gösterilmiyordu (Hizmet Sağlayıcılar Yönetmeliği m.9/1); 3D Secure dönüşünde müşterinin ne gördüğü yazılı değildi (K-704, ⚠) | v0.54 — `02 §3.17.1` · `10` v0.36 KP-15 |
| 11.33 | Kalite döngüsü, §8.3.3: sağlayıcıya ulaşılamayan kart iadesi sonucu bilinmeyen bir istekti; yeniden deneme ya da havale yolu parayı iki kez gönderebilirdi (K-705, ⚠) | v0.54 — `02 §7.2.8` · `08` park satırı |
| 11.34 | Kalite döngüsü, §2.7, §8.4: gecikme feshinin düğme dışında yolu yoktu — e-postayla, mektupla ya da kesintide bildirilen fesih kaydedilemiyordu (K-706, ⚠) | v0.54 — `02 §7.2.3`, §10.4.1, §10.4.10 · `10` v0.36 KP-21, KP-48 |
| 11.35 | Kalite döngüsü, §4.1: cayma ve teslimat bildirimlerinin geldiği iletişim talebi iki yılda siliniyordu; Yönetmelik m.20/1 üç yıl ister (K-707, ⚠) | v0.54 — `02 §3.32.4`, Z-33, P-40, §12.2.7 |
| 11.36 | Kalite döngüsü, §9.4: imha kaydı yalnız periyodik çalışmayı kaydediyordu; hesap silme, bekleyen kaydın silinmesi ve IBAN silme kayda girmiyordu (K-708, ⚠) | v0.54 — `02 §4.2` Z-44, §12.2.7 · `10` v0.36 KP-74 |
| 11.37 | Kalite döngüsü, §5.2.3, §8.4.8: başka kanaldan gelen caymada süreye uygunluk ulaşma tarihinden okunuyordu; yasa bildirimin süresinde yöneltilmesini yeterli sayar (K-709, ⚠) | v0.54 — `02 §10.4.10` |
| 11.38 | Kalite döngüsü, §2.7.5, §6.2.4: müşterinin IBAN girişinin bildirimi ne vardı ne bilinçli olarak yoktu; kandırılma akışları kaydı olmayan bir izi kanıt gösteriyordu (K-710, ⚠) | v0.54 — `02 §9.2`, 6.3.5, §10.4.11 |
| 11.39 | Kalite döngüsü, §9.2.6: yeniden doğrulamadaki başarısız şifre denemesi hiçbir limitte sayılmıyordu (K-711, ⚠) | v0.54 — `02 §3.13.13`, §8.2 L-1, yeni 6.5.17 · `10` v0.36 KP-72 |
| 11.40 | Kalite döngüsü, §8.4.4: iade malının ulaşma tarihi — teslimden sonraki caymanın geri ödeme süresini başlatan elle kayıt — düzeltilemiyordu (K-712, ⚠) | v0.54 — `02 §9.2`, §10.4.1, §10.4.5 · `10` v0.36 KP-48 |
| 11.41 | Kalite döngüsü, §8.7.4: indirme hakkının (P-8) değişmesinin verilmiş siparişe etkisi yazılı değildi (K-713, ⚠) | v0.54 — `02 §3.12.5`, §3.23.1, P-8 · `10` v0.36 KP-17 |
| 11.42 | Kalite döngüsü, §8.2: teyit akışları yöneticinin siparişi bulmasıyla başlıyordu ama sipariş listesinde arama ve süzgeç yoktu (K-714, ⚠) | v0.54 — `02 §10.1.2` · `10` v0.36 KP-46 |
| 11.43 | Kalite döngüsü, §1.6.1: başka kanaldan bildirimin kaydı ve hesabın talep üzerine silinmesi geri alınamaz olduğu hâlde onay listesinde yoktu (K-715) | v0.54 — `02 §5.9` |
| 11.44 | Kalite döngüsü, §8.3.3.6: ayıpta geri ödemenin zamanı yazılı değildi ve akış kaynaksız bir "kalan süre" gösteriyordu (K-716, ⚠) | v0.54 — `02 §12.5` |
| 11.45 | Kalite döngüsü, §1.4.3: Kargoya verildi ve Teslim edilemedi'de son kalemin iptalle ya da çıkarmayla kapanması S9'un kapanış cümlesinde yoktu (K-717) | v0.54 — `02 §5.4` S9 |

**Tablonun yazım turundaki kapanışı (5. oturum, v0.6).** Yazım turunun beş oturumu ve workshop `02`'ye otuz satırlık geri besleme verdi: Ürün Gereksinimleri v0.48…v0.53, MVP Kapsamı v0.30…v0.35; her satır `02`'de — dokunduysa `10`'da — yapılan değişikliği ve sürümü taşır. Konvansiyon ve yerleşim kararları (K-669, K-670, K-682, K-689, K-700) `02`'ye dönmediği için satır almaz. Bundan sonra tabloya satırı kalite döngüsünün kararları ekler; etki yansıtması (cross-review Faz 5) iki dokümanı yeniden tarar (K-652).

**Kalite döngüsü — audit ve deep review (v0.7).** On beş satır (11.31…11.45): Ürün Gereksinimleri v0.54, MVP Kapsamı v0.36. İki yazım konvansiyonu kararı (K-701, K-702) `02`'ye dönmediği için satır almaz. Aynı PR'da workshop satırlarının (11.2–11.5) değişen bölüm listeleri `02` v0.48'in sürüm notuna hizalandı ve iki dokümanın dosya sonu dipnotu başlıktaki sürüme çekildi. Rapor `Docs/AUDIT_REPORTS/03_AUDIT.md` ve `03_DEEP_REVIEW.md`'dedir.

**Kalite döngüsü — cross-review ve etki yansıtma (v0.8…v0.10).** Cross-review'ın üç turu (K-718'in ölçüsüyle; 3. tur TEMİZ) `02`'ye dönen karar almadı; düzeltmeler bu dokümanın iki yerini birbirine ya da `02`'nin mevcut kuralına hizaladı. Etki yansıtma (K-652) iki dokümanı yeniden taradı: Ürün Gereksinimleri v0.54 ve MVP Kapsamı v0.36 bu dokümanın v0.10'uyla çelişmez; tabloya satır girmedi. Sonuç `Docs/CROSS_REVIEW_REPORTS/03_CROSS_REVIEW_R3.md` §5'tedir.

**Aşama 2 checkpoint'i (v0.11).** Checkpoint bu dokümanı, Ürün Gereksinimleri'ni, MVP Kapsamı'nı ve Proje Vizyonu'nu birbirine karşı taradı ve Aşama 2 kararlarının iki dokümanda eksik kalan yansımalarını işledi: Ürün Gereksinimleri v0.55, MVP Kapsamı v0.37. Düzeltmeler var olan kararların yansımasıdır; yeni karar yoktur ve tabloya satır girmez — her satır bir karar grubudur (K-652). Değişen yerler iki dokümanın sürüm notunda, bulgular raporun §3'ündedir (`Docs/CHECKPOINT_REPORTS/CP02_PHASE2_CHECKPOINT.md`).

**Aşama 3 kararlarının geri beslemesi (v0.14…v0.16).** Arayüz Tanımları aşamasının `02`'ye dönen kararları bu tabloya satır olarak girmez; geri besleme tablosu `04 §1.3`'tür ve karar bu dokümana yalnız yansır (K-652, K-726). **v0.16 — kısmi adet (K-787…K-794):** iptal, cayma ve iadede adedi birden büyük kalemin işleme giren adedi seçilir; kural Ürün Gereksinimleri v0.60'ta (`02 §7.1.6`, §7.1.7), MVP Kapsamı v0.41'de yazılıdır; bu dokümanda değişen yerler başlık notundaki v0.16 maddesindedir.

---

*Shopfolio — User Flows v0.21 — ✓ Tamamlandı (v0.21 Aşama 3'ün çakışma taramasıdır — `04 §1.3`, K-850 ve dört hizalama; v0.20 Aşama 3 Arayüz Tanımları cross-review'ının etki yansıtmasıdır — `04 §1.3`, K-236'nın metni; v0.19 Aşama 3 Arayüz Tanımları kalite döngüsünün — audit ve deep review — geri beslemesidir — `04 §1.3`, K-838, K-839; v0.18 Aşama 3 yazım turunun 5. oturumunun geri beslemesidir — `04 §1.3`, K-826; v0.17 Aşama 3 yazım turunun 3. oturumunun geri beslemesidir — `04 §1.3`, K-805, K-806, K-808; v0.16 Aşama 3 kısmi adet kararının geri beslemesidir — `04 §1.3`, K-787…K-794; v0.15 Aşama 3 workshop'unun geri beslemesidir — `04 §1.3`, K-754, K-759…K-761; v0.14 izlenebilirlik matrisinin geri beslemesidir — K-732…K-734, K-736, K-737; Aşama 2 kapanışı, K-437'nin 6. adımı; §0–§11 yazıldı; kalite döngüsü: audit ✓, deep review ✓, cross-review ✓ 3 turda TEMİZ, etki yansıtma ✓ — K-431, K-432, K-718; ölçüsüz yoklama turu, bulgu uygulanmadı — K-720; checkpoint ✓ — K-437'nin 4. adımı, `Docs/CHECKPOINT_REPORTS/CP02_PHASE2_CHECKPOINT.md`)*

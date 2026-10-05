# Aşama 3 — Çakışma Taraması

**Tarih:** 2026-10-05 | **Aşama:** 3 — UI/UX tasarım | **Kapanış adımı:** K-437'nin 3. adımı (K-429; `checklists/document-stage.md` §7)
**Girdi sürümleri:** karar kaydı v0.90 · Arayüz Tanımları (`04`) v0.17 · Ürün Gereksinimleri (`02`) v0.64 · Kullanıcı Akışları (`03`) v0.20 · MVP Kapsamı (`10`) v0.42 · Proje Vizyonu (`01`) v0.33 · `05`–`09`, `11`, `12` ve `DEPLOY_RUNBOOK`'un park blokları
**Çıktı sürümleri:** karar kaydı v0.91 · `04` v0.18 · `02` v0.65 · `03` v0.21 · `10` v0.43 · `01` değişmedi · `05`, `06`, `07`, `08`, `12` ve `DEPLOY_RUNBOOK`'un park blokları

> **Ne işe yarar:** Aşama kapanmadan önce açık kalemlerin her birinin **tek bir evi** ve **gözlemlenebilir bir kapısı** olduğunu doğrular; vadesi geçmiş açıkları adıyla listeler (K-429). Önceki aşamaların ve bu aşamanın park satırlarını, kaynak kararlarını sonradan değiştiren kararlara karşı okur. Aşama 3'ün üst dokümanlara geri beslediği kuralların bütün yüzlerde aynı hâlde olduğunu denetler (K-652, betik 1). Aşama 3 kararlarının sonraki dokümanlara devirlerini tek dizinde toplar (§6). Sonuç checkpoint'in (4. adım) girdisidir.

---

## 1. Sonuç

| Kural | Sonuç |
|---|---|
| **(1) Aynı açık iki yerde durmaz** | ✓ — açık karar yok. Tracker §4'ün on sekiz satırının hepsi kapalı ve Aşama 3'te A-19 açılmadı. Arayüz Tanımları'nın şablonunda açık kararlar bölümü yok (PF-55); dokümanda açık kalem yok, açık kalem kuralı konvansiyon 15'tedir. Ürün Gereksinimleri §13 tablosu boş; Kullanıcı Akışları ve MVP Kapsamı'nda açık kalem yok; raporlarda taşınan açık not yok (§3.3). |
| **(2) Her açık satır gözlemlenebilir bir kapı taşır** (K-37) | ✓ — açık dört süreç maddesinin dördü de kapılı (§4). ⚠ öneriyle kayıt listesinin kapısı "en geç arşiv işaretinden önce" diyordu; gösterimin biçimi ve hazır metni eklendi (§4.1). |
| **(3) Vadesi geçmiş ve açık satır adıyla listelenir** | ✓ — **yok.** Dört süreç maddesinin kapısı (checkpoint, öğrenim terfisi, arşiv işareti) henüz gelmedi. |

**Çakışmalar:** dört salt okuma merceği **27 bulgu** döndürdü. **Biri karar istedi — K-850 ⚠** (§5.1): veri toplayan girişlerin kapısına çerez politikasının yayını da girer; K-834'ün öncülü — "bu sürede veri toplayan girişler kapalıdır" — aydınlatma metni önce yayına alınınca tutmuyordu. **Yirmi üç hizalama** var olan kararların kuralını öteki yüzlerine taşıdı (§5.2). **Park satırları** (§5.3): Aşama 1–2'nin üç park satırı hizalandı (`08` K-12, `06` K-17 · K-531, `12` K-22); Aşama 3'ün park satırlarında hizasız satır çıkmadı; beş kararın etki sütununa park hedefi eklendi ve `12`'nin aralık etiketi daraltıldı. **Betikle temiz dönen sınıf: dokuz** (§5.4). **Devir dizini:** Aşama 3'ün 40 kararı sonraki dokümanlara 53 atıf taşır; **hiçbiri yalnız doküman numarası değildir**; Arayüz Tanımları'nın gövdesinde dört iş karar satırı olmadan devredilmiş (§6). **⚠ listesi:** kayıtla birebir — 39 / 39, K-850 ile 40 / 40 (§4.1). **Öğrenim adayları:** üç yeni (§7). **Çok kritik soru çıkmadı.**

---

## 2. Yöntem

Tarama gözle değil betikle yapıldı (K-429); anlamsal okuma dört salt okuma merceğiyle, ön planda koştu — bulgular bulundukça dosyaya yazıldı, her bulgu düzeltmeden önce güncel metne karşı yeniden okundu. Ortam: `LC_ALL=C.UTF-8`, Python yok.

1. **Tracker §4 ve §10.1:** `## 4.` ile `## 5.` arasındaki `| A-NN |` satırları ayrıştırıldı, durum hücresi "Kapandı"ya karşı denetlendi; `A-19` ve sonrası dosyanın tamamında arandı (on eşleşmenin hepsi "A-19'dan sürer" kural cümlesi). §10.1'in işaretsiz maddeleri listelendi. ⚠ listesi: §10.1'in ⚠ maddesindeki numaralı K'lar (`(n) K-NNN` kalıbı) ile karar kaydında konu hücresi `(öneriyle kaydedildi — ⚠` taşıyan K-723…K-849 satırları `comm` ile iki yönde karşılaştırıldı.
2. **Dokümanların açık kalemleri:** `01`, `02`, `03`, `10` ve `04`'te "açık karar", "belirlenecek", "netleşecek", "karara bağlanacak", "TBD", "(downstream)", "sonra doldurulacak", "muhtemelen", "gereksinime göre" desenleri; her eşleşme okundu.
3. **Park satırları:** hedef dokümanların (`04`–`09`, `12`, `DEPLOY_RUNBOOK`) "Aşama N'den park edilen girdiler" bloklarındaki `> - **K-…` satırlarının etiketinden kaynak K'lar çıkarıldı (aralıklar açıldı). Her kaynak için numarası kaynaktan büyük bütün karar satırlarında tam eşleşme (`K-NN[^0-9]`) arandı — ilişki sözcüğüne bakılmadı — ve **her eşleşme okundu** (mercek 1: Aşama 1–2 kaynakları; Aşama 3 kaynakları tek bağlamda). Ayrıca Aşama 3 park satırlarının etiketleri ile kararların etki sütunları iki yönde karşılaştırıldı.
4. **Kuralların yüzleri (K-652, betik 1):** Aşama 3'ün geri beslediği on kural — kısmi adet, ayıp talebinin adet sınırı (mercek 2) · cayma özeti, okunabilirlik, geri alma düğmesi, çerez (mercek 3) · dışa aktarmanın yeri, yönetici adı, ürün listesi araması, yeniden gönderim kapsamı (mercek 4) — anahtar terimlerle `01`, `02`, `03`, `10`, `04` ve sonraki dokümanların park bloklarında tarandı; her eşleşme okundu ve "uyumlu · ilgisiz · kayıt · eski hâlde" diye sınıflandı. K-850'nin kuralı için aynı betik ayrıca koştu (§5.1).
5. **Kimlik sınıfları:** B-, F-, Z-, L-, P-, H- kimlikleri `02`'deki kümeye karşı (`comm`); S1…S11, Ö1…Ö5; `E-nn` öteki dokümanlarda; KP satırları ↔ matris; durum ve aktör adlarının yanlış biçimleri; K atıflarının kayıtta varlığı; etki sütunu `04`'ü gösteren Aşama 3 kararlarının `04`'te anılması; sürümler.
6. **Devirler:** K-723…K-850'nin etki sütunu ` · ` ile parçalandı; `05`–`09`, `11`, `12` ya da `DEPLOY_RUNBOOK` ile başlayan parçalar alındı, virgülle birleşik hedefler açıldı ve parçanın parantezi işin adı olarak okundu. `04`'ün §1.3 ve §2–§9 gövdesinde sonraki dokümanı anan cümleler (`` `05` ``…`` `12` ``, adlarıyla) çıkarıldı.

---

## 3. Açık kalemler

### 3.1 Tracker §4

| Kapsam | Satır | Açık | Sonuç |
|---|---|---|---|
| A-01…A-18 (Aşama 1, salt okunur) | 18 | 0 | Aşama 1'in taraması §3.1 |
| A-19 ve sonrası (Aşama 3) | 0 | 0 | Aşama 3'te açık detay doğmadı: matrisin, workshop'un, yazımın ve kalite döngüsünün bulduğu her boşluk oturumunda bir K satırıyla kapandı (K-724…K-849); bu tarama da K-850 ile kapattı |

### 3.2 Dokümanlar

| Kaynak | Bulunan | Sonuç |
|---|---|---|
| Arayüz Tanımları (`04`) | Şablonda açık kararlar bölümü yok (PF-55); konvansiyon 15 açık kalemi tracker §4'e, sonraki aşamaya talimatı park bloğuna gönderir. Desen eşleşmeleri: "açık karar" bir (konvansiyon 15), "karara bağlanacak" iki (park bloklarının kalıp cümlesi), "gereksinime göre" bir (konvansiyon 14'ün yasağı) | Açık kalem yok |
| Ürün Gereksinimleri §13 | Tablo boş; §13.4 tablonun neden boş olduğunu yazar | Açık karar yok; Aşama 3'ün geri beslemesi (v0.56…v0.65) §13'e satır açmadı |
| Kullanıcı Akışları | "açık karar" bir eşleşme — §0.6.2'nin kural cümlesi; "(downstream)" bir — aynı yer | Açık kalem yok |
| MVP Kapsamı | "açık karar" iki eşleşme — v0.2'nin başlık notu ve onun okunuş notu | Açık karar yok |
| Proje Vizyonu | Desen eşleşmesi yok | — |

### 3.3 Raporlarda kalmış açık not

Yok. Arayüz Tanımları'nın cross-review'ının beşinci raporu "açık not kalmadı" der (`04_CROSS_REVIEW_R5.md`): 1.–4. turun notları kapandı; audit'in uygulanmayan sekiz bulgusu (YASAL-6, DR-2, DR-4, KAPSAM-K-7, K-9, K-13, FORM-14, FORM-17) audit raporunda gerekçeyle kapalıdır; FORM-16'nın birim listesi `06`'nın park bloğundadır. Tracker §3'ün on bir GAP'i karara bağlıdır.

---

## 4. Süreç maddeleri ve kapıları

Tracker'ın işaretsiz maddeleri bu taramadan önce dörttü, hepsi §10.1'de. Devir §9'un girdileri ve hafızanın Güncel Durum bloğu ayrıca tarandı.

| Madde | Ev | Kapı | Sonuç |
|---|---|---|---|
| Checkpoint maddesi — K-849'un devri (iki yerin metin farkı sınıfı) | Tracker §10.1 | Aşama 3'ün checkpoint'i | Kapı var; bu raporun §8'i girdisidir |
| Bekletilen öğrenim Ö-23 — checklist'in skill'e dönüşmesi | Tracker §10.1 · PF-32 · devir §9'un 7. satırı | Aşama 3'ün öğrenim terfisi | Kapı var |
| ⚠ öneriyle kayıt listesi (K-723) | Tracker §10.1 | Arşiv işaretinden (6. adım) önce proje sahibine tek mesajda | **Kapı somutlaştı** — gösterimin biçimi ve hazır metin (§4.1) |
| Öğrenim terfisi adayları 1–7 | Tracker §10.1 · PF-54…PF-58 | Aşama 3 kapanışının öğrenim terfisi | Kapı var; bu taramadan 8–10 eklendi (§7) |
| Playbook listesinin gönderimi (K-649) | `PLAYBOOK_FEEDBACK.md` | Proje tamamlandıktan sonra, proje sahibi | Kapı var |
| D-01 — kurulumun ertelenen parçası | `DEFERRED_BACKLOG.md` | Aşama 4 kapanışından sonra, ilk implementation task'ından önce | Kapı var; Aşama 3'ten yeni kalem yok |

Devir §9'un öteki girdileri kapandı: yeniden gönderilebilen e-postaların kapsamı K-759 ile karara bağlandı; açılış sorusu K-723 ile cevaplandı.

### 4.1 ⚠ öneriyle kayıt listesi — doğrulama ve proje sahibine gösterilecek metin

**Betikle doğrulama.** Konu hücresi `(öneriyle kaydedildi — ⚠)` taşıyan Aşama 3 satırı (K-723…K-849) **39**; §10.1'in listesindeki K numarası **39** — iki küme birebir, eksik 0, fazla 0. Bu taramanın kararı K-850 de ⚠'dir; liste ve kayıt **40 / 40**. Aşama 3'ün 128 satırının işaret dağılımı: ⚠ 40 · `(öneriyle kaydedildi)` 86 · işaretsiz 2 (K-723 — proje sahibinin açılış kararı, K-787 — proje sahibinin kısmi adet kararı). `(toplu onayla kaydedildi)` yok.

| Adım | Kararlar | Sayı |
|---|---|---|
| Matrisin 2. oturumu | K-732 · K-733 · K-736 · K-737 | 4 |
| Workshop | K-747 · K-754 · K-756 · K-759 · K-760 | 5 |
| Yazım turu — 2a, 2b | K-770 · K-771 · K-774 · K-785 · K-786 | 5 |
| Kısmi adet kararı | K-788 · K-789 · K-794 · K-796 | 4 |
| Yazım turu — 3., 4., 5. oturum | K-798 · K-805 · K-806 · K-808 · K-811 · K-814 · K-815 · K-818 · K-821 · K-823 · K-826 | 11 |
| Audit ve deep review | K-830 · K-831 · K-832 · K-833 · K-834 · K-835 · K-836 · K-837 · K-838 · K-839 | 10 |
| Çakışma taraması | K-850 | 1 |
| **Toplam** | | **40** |

**Kapı.** Liste arşiv işaretinden (6. adım) önce proje sahibine **tek mesajda**, karar başına bir sade cümleyle ve gerekçesiz gösterilir (`INSTRUCTIONS.md` §2, "Ortak kural"); yöneticiden proje sahibine gider (§9). İtiraz gelmeyen satırın işareti `(öneriyle kaydedildi — ⚠ — gözden geçirildi YYYY-AA-GG, itiraz yok)` olur; itiraz gelen karar yeni bir satırla değişir.

**Kapının sonucu — 2026-10-05 (K-437'nin 6. adımı; K-854):** kapı işledi. Liste proje sahibinin isteğiyle arşiv işaretinden önce, aşağıdaki hazır metinle tek mesajda gösterildi (`INSTRUCTIONS.md` §9, işletim kuralı 3); K-848 ve K-849 bilgi olarak aynı mesajda. İtiraz yok. Kırk satırın konu hücresindeki işaret `(öneriyle kaydedildi — ⚠ — gözden geçirildi 2026-10-05, itiraz yok)` oldu; sayım betikle doğrulandı (40 / 40 — bu listenin, CP03 §5.1'in ve kaydın kümeleri birebir); karar kaydının §3'ündeki GAP-1, GAP-2, GAP-8 ve GAP-9 satırlarının işareti de dönüştü. Gösterimden sonra kaydedilen ⚠ karar yok. Madde karar kaydının §10.1'inde kapandı; arşiv işareti aynı PR'da düştü (v0.95).

**Hazır metin — proje sahibine gösterilecek liste** (konulara göre; dokümanlar adıyla):

*Ödeme adımı ve siparişin onayı*
- K-756 — Ödeme adımı tek ekrandır; sipariş özeti, iki yasal metnin tamamı, onay kutuları ve "ödeme yükümlülüğü doğar" yazan düğme aynı bölümde art arda durur.
- K-831 — Onay kutularının hemen üstünde, Ön Bilgilendirme Formu'ndan ayrı kısa bir cayma özeti durur.
- K-832 — İki yasal metin ekranda ve e-postada en az on iki punto karşılığı boyutla, küçültülmeden gösterilir.
- K-833 — Onay anında fiyat ya da kargo gibi bir fark çıkarsa müşteri iki yasal metnin kutularını yeniden işaretler.
- K-771 — Dijital ürün ve hizmet için onay kutularında Ürün Gereksinimleri'ndeki taslak metin son metin olur; kutu hangi ürünleri kapsadığını adıyla sayar.
- K-837 — Aynı sepetten ödenmemiş eski bir sipariş varken ödeme adımı onun iptal edileceğini önceden söyler.

*Müşterinin iptal, cayma ve iade işlemleri*
- K-732 — Müşteri iptalde, gecikme nedeniyle fesihte, caymada ve hesap silmede son adımda sonucu tek cümleyle görür ve onaylayarak tamamlar.
- K-785 — Bu son adımda düğme işlemin adını taşır ("Siparişi iptal et", "Cayma beyanını gönder", "Hesabımı sil" gibi).
- K-838 — Kargodaki üründen cayan müşteriye ekran önce kargonun ürünü firmaya geri götüreceğini söyler.
- K-826 — Müşterinin sipariş sayfası yapılan her geri ödemeyi tarih, tutar ve yolla (karta ya da IBAN'a) gösterir; henüz yapılmamış geri ödeme ayrıca gösterilmez.

*Bir üründen birkaç adet alındığında*
- K-788 — Bir ürünün bazı adetleri iptal edilince ya da onlardan cayılınca o adetlerin parası geri ödenir; kuponun indirimi adetlere bölünür, artan kuruş son işlemde ödenir.
- K-789 — Siparişten bir adet bile gönderilmiş ya da müşteride kalmışsa kargo ücreti geri ödenmez; hepsi kargodan önce iptal edilirse ya da hepsi iade edilirse ödenir.
- K-794 — Firma da bir ürünün bazı adetlerini iptal edebilir ve iadeyi gelen adet kadar teslim alır; firmaya yeni zorunlu adım eklenmez.
- K-796 — Gecikme nedeniyle fesihte ürün ve adet seçilmez; fesih teslim edilmemiş bütün ürünlere uygulanır.
- K-830 — Ayıp talebinde müşteri ayıplı adedi seçer; ürün kargoya verilmişse teslim işareti beklenmeden talep açılır.
- K-808 — Ayıp talebinde sözleşmeden dönülürse firma ürünü iade teslim alma adımıyla alır ve parayı ardından öder.

*Vitrin ve yasal bilgiler*
- K-747 — Firmanın yasal kimlik ve iletişim bilgilerinin tamamı ve ETBİS doğrulama bandı vitrinin her sayfasının altında, ayrıca İletişim sayfasında durur.
- K-754 — Ana sayfanın ürün vitrini yayındaki en yeni ürünleri gösterir; firma ana sayfa için ürün seçmez.
- K-774 — Vitrindeki ürün kartında sepete ekleme yoktur; renk, beden gibi seçenek ürün sayfasında seçilir.
- K-835 — Sepetteki ücretsiz kargo eşiği satırı süreli kampanya sayılmaz, tarih göstermez.
- K-770 — Firma aydınlatma metnini ya da çerez politikasını ilk kez yayına alana kadar o metnin bağlantısı görünmez; ürünün taslak metni ziyaretçiye gösterilmez.
- K-834 — Çerez politikası ilk kez yayına alınana kadar site ziyaretçiye çerez yazmaz.
- K-850 *(bu taramada eklendi)* — Aydınlatma metni ve çerez politikası ikisi de yayına alınmadan üyelik, Google ile ilk giriş ve iletişim formu açılmaz.

*Müşteri hesabı ve giriş*
- K-733 — Üye hesabına bağlanmış Google girişini kendisi kaldıramaz.
- K-786 — E-posta adresini başka bir müşterinin kayıtlı adresine çevirmek isteyen üye, adresin başka bir hesapta kayıtlı olduğunu görür.
- K-839 — E-posta değişikliğini geri alma bağlantısı açılınca kendiliğinden geri almaz; geri alma ekrandaki tek düğmeyle olur.

*E-postalar*
- K-759 — Müşteriye ulaşmayan her sipariş e-postası (on altı bildirimin hepsi) panelden yeniden gönderilebilir.
- K-760 — Firmaya giden bildirim ulaşmadığında siparişe işaret konmaz; panelin ana sayfasında "firma bildirimleri ulaşmıyor" uyarısı çıkar.

*Yönetim paneli*
- K-736 — Yönetici hesabı bir ad taşır; davetli hesabını açarken yazar, sonra kendi hesabından değiştirir.
- K-737 — Paneldeki ürün listesinde ürün adıyla arama ve yayın durumuna göre süzme vardır.
- K-798 — Firma "stokta bulunamadı" diye iptal ederken, gecikmede, süresi geçmiş ya da istisnalı üründe cayma kaydederken ve karta iade gerçekleşmediğinde panel yasal sonucu söyleyen sabit uyarılar gösterir; süresi geçmiş kayıt ayrıca bir kutuyla onaylanır.
- K-805 — Kategoriler her yerde ada göre alfabetik dizilir; firma sırayı elle değiştiremez.
- K-806 — Kupon kodu tektir ve büyük-küçük harf fark etmeden çalışır; kupon silinemez, bitiş tarihi öne çekilerek durdurulur.
- K-811 — Panelde üye arandığında e-postası doğrulanmamış kayıt da bulunur; geri alma bağlantısı açıkken hesap silinemez ve ekran bunu tarihiyle söyler.
- K-814 — Ayar değişikliklerinin onayı sonucu sayıyla söyler; iade adresi değişirken eski adrese gelen ürünü teslim almanın firmanın yükümlülüğü olduğu yazar.
- K-815 — Firma kimliği ekranı satışın neden kapalı olduğunu tek tek gösterir; ETBİS alanının yanında kaydın firmanın yükümlülüğü olduğu yazar.
- K-818 — Aydınlatma metni ve çerez politikası taslakta düzenlenip "Yayına al" ile yayınlanır, yayından çekilmez; metin yalnız kalın, liste, ara başlık gibi sınırlı biçimlerle yazılır.
- K-821 — Satış özetindeki ciro onaylanan siparişlerin toplamıdır; sonradan yapılan geri ödemeler cirodan düşülmez, iptal ve iade ayrıca sayılır.
- K-823 — Dışa aktarma ekranının başında dosyadaki kişisel verinin sorumluluğunun firmada olduğu yazar; üye ve talep listesi her zaman tamamıyla iner.
- K-836 — Üye ve talep listesi yalnız panelin dışa aktarma ekranından indirilir.

*Süreç kararları — ⚠ değil, bilgi için (Arayüz Tanımları'nın ikinci model incelemesi)*
- K-848 — Arayüz Tanımları büyük olduğu için ikinci model incelemesinde her tur doküman hem tek seferde hem dört parçaya bölünerek okutuldu; tur ancak hepsi temiz dönerse temiz sayıldı.
- K-849 — İnceleme beşinci turda, kabul edilen bulgu kalmayınca kapatıldı; aynı yerde tekrar tekrar reddedilen bir itiraz sonucu belirlemedi, kalan küçük metin farklarına sıradaki genel kontrol (checkpoint) bakar.

---

## 5. Çakışmalar

### 5.1 Karar isteyen çakışma — K-850 (⚠)

| Yer | Çakışma | Karar |
|---|---|---|
| `04 §5.11.9` · `05` ve `12`'nin K-834 park satırları | K-834 ("çerez politikası ilk kez yayına alınana kadar vitrin çerez yazmaz") gerekçesinde "bu sürede satış ve veri toplayan girişler kapalıdır" der. Oysa veri toplayan girişlerin kapısı **yalnız aydınlatma metnine** bağlıydı (K-517; `02 §3.1.5`, §3.13.19, §3.32.8; `10 §4.1` ÖK-10: *"çerez politikası yalnız satış kapısını tutar"*). Firma aydınlatmayı çerez politikasından önce yayına alırsa aradaki sürede hesap kaydı, Google ile ilk giriş ve iletişim formu açıktı ve bu girişler zorunlu çerez ister (`02 §12.2.5`). Kural ya bu girişleri bozar ya da çerezler, aydınlatması olan çerez politikası yayında değilken yazılırdı | **K-850 (öneriyle kaydedildi — ⚠):** veri toplayan girişlerin kapısına çerez politikasının yayını da girer. Elenen: aydınlatmanın yayınına sıra kuralı (dolaylı, aydınlatmanın düzeltilmesini kilitler) · K-834'ü ilk kuruluma daraltmak (aradaki pencerede risk kalır). Bedel: iki metin kurulum kontrol listesinin aynı maddesidir. Çok kritik değildir: önerilen seçenek riski kaldırır, bedeli bir kurulum adımının sırasıdır |

**Kuralın yüzleri (betik 1) — K-850:** terimler "veri toplayan giriş", "aydınlatma kapı", "aydınlatma metni tamamlan", "çerez politikası yalnız", K-517, K-834, "ilk kez yayına alınana kadar", "çerez yazmaz" ve kapının koşul cümleleri. **Güncellenen 35 yüz:** `02 §3.1.5`, §3.13.7, §3.13.19, §3.32.8, §6.5.12, §6.5.16, §6.8.9, §6.9.10, §10.8.2, §12.2.2, §12.2.5 · `03` 2.2.7, 3.4.12, 3.4.16, 3.5.3.9, 3.5.4.10, 9.1.1, 9.1.6, 9.2.2 · `10 §2` KP-25, KP-26, KP-34, §4.1 ÖK-10 · `04` 2.8.3, 5.10.12, 5.11.9, 5.20.10, 9.3.4, 9.10.11, 9.19.7, 9.19.10, §1.1'in `03 §2.2.7` ve §9.1.1 özetleri · `05` ve `12` park satırları · bölüm sonu Kaynak satırları. **Elenenler:** `01` iki eşleşme — satış kapısının yasal metin koşulu (değişmedi); `02` sürüm notları ve Kaynak satırları (kayıt); `04` 5.22.10, 6.1.5.2, 6.2.10.x, 6.2.20.x, 6.2.22.5 kapıyı adıyla anar ve tanımı 2.8.3'tedir; `04 §1.3`'ün K-834 satırı (kayıt). Kapının adı "aydınlatma kapısı"ndan "veri toplayan girişlerin kapısı"na çevrildi (`02 §3.13.7`, §3.13.19, §6.5.16; `03` 3.4.16, 9.1.6, 9.2.2; `04 §1.1`). **Betik 2:** gövdede anılan K-850 ve K-834 bölüm sonu Kaynak satırlarında (`02 §3`, §6, §10, §12; `04 §2.8`, §5.10, §5.11, §5.20, §9.3, §9.10, §9.19); `03` ve `10`'da satırın Kaynak sütununda.

### 5.2 Hizalanan çakışmalar — var olan kararın öteki yüzleri

Hepsi var olan bir kararın kuralının, değişmeden kaldığı yüzlere taşınmasıdır; yeni karar değildir. Üst dokümanların ✓ durumu korunur, kalite döngüsü yeniden açılmaz (K-652). Tablo `04 §1.3`'te de satırdır.

| # | Yer | Eski hâl | Karar | Düzeltme | Şiddet |
|---|---|---|---|---|---|
| A-1 | `02 §10.4.10` | Başka kanaldan gecikme feshi kaydında "firma kalemi seçer" | K-796 | Kalem ve adet seçilmez; kayıt teslim edilmemiş fiziksel kalemlerin açık adetlerinin tamamına uygulanır | orta |
| A-2 | `03` 8.4.8 | "Kalemi … girer" iki kayda birden okunuyordu | K-796 | Caymada kalem ve adet; fesihte seçim yok | düşük |
| A-3 | `02 §7.1.1`, §7.5.1 | Ayıp kanalı "teslimden itibaren" açık | K-830 (`02 §7.5.2`) | Kanal fiziksel kalemde kargoya verildiği andan açık; iki yıl teslimden işler | düşük |
| A-4 | `02 §6.4.25` · `03` 3.3.25 | Başka kanaldan caymanın kaydında adet yok | K-787 | Cayılan adet seçilir | düşük |
| A-5 | `03` 7.1.21–7.1.24 · `04` 5.18.10, 5.19.6 | B-9'un içeriğinde kalem ve adet yok | K-790 (`02 §9.2`) | Kalemler ve adetler; ayıpta ayıplı adet | düşük |
| A-6 | `04` 6.2.16.24 | Teslim tarihi girilmemiş fiziksel kalemde ayıp talebi aksiyonlarda yok | K-830 | "Ayıp talebi açmak" — kargoya verildiği andan, Z-18 işlemez | düşük |
| R-2 | `04` 5.28.3, 6.2.28.1 | Geri alma adımı "ilk kez açıldıysa" | K-839 | "Henüz kullanılmamışsa — kaç kez açıldığına bakılmaz" | orta |
| R-3 | `04` 9.25.2 | E-54'ün ilk göreninde geri alma düğmesi yok | K-839 | Geçerli bağlantıda tek cümle ve tek düğme | orta |
| R-4 | `04 §4.1` E-28, §4.2 E-54 | Amaç cümlesi "bağlantının sonucunu söyler" | K-839 | Değişikliği tek düğmeyle geri aldırır | düşük |
| R-5 | `04 §1.1` (`03 §9.3.6`) | Özet "bağlantı açılır" | K-839 | Geri alma ekrandaki tek düğmeyle | düşük |
| R-6 | `03` 2.4.6 | Onay adımında cayma özeti yok | K-831 | Kutuların üstünde kısa cayma özeti | orta |
| R-7 | `10 §2` KP-14 · `04 §1.1` (KP-14) | Onay adımının içeriğinde cayma özeti yok | K-831 | Cayma özeti sayılır | düşük |
| R-8 | `04` 7.1.5.9 | Kutunun işareti yalnız sürüm değişiminde kalkar | K-833 | Özet farkı formu değiştirirse de kalkar | düşük |
| R-9 | `04` 3.1.19 | Altbilgi bağlantısı "her sayfada" | K-770 | İlk yayına kadar o metnin bağlantısı yok | düşük |
| S-1 | `03 §7` girişi | Ulaşmayan her e-posta satıra işaret düşürür ve yeniden gönderilir | K-759, K-760 | Müşteri bildirimi işaret ve yeniden gönderim; firma bildirimi ana sayfa uyarısı; davet ve hesap e-postası yeniden gönderilmez | orta |
| S-2 | `03` 3.1.1 | Aynı genelleme | K-760 | Firma bildirimi istisnası | düşük |
| S-3 | `03` 1.7.3.1 | Davetin işareti de yeniden gönderimle kalkar | K-759 | Davette işaret davet geçersizleşince kalkar | düşük |
| S-4 | `03` 1.11.36 | "İşaretli satırda" — davet satırı dahil | K-759 | Sipariş işaretliyken, müşteri bildirimi için | düşük |
| S-5 | `03` 8.9.2, 7.1.37, 10.1.2.6 | Yeniden gönderim "sipariş satırından" | K-761 | Siparişin ayrıntısından | düşük |
| S-6 | `DEPLOY_RUNBOOK` | İlk yöneticinin adı kurulum yüzünde yok | K-736 | Aşama 3 bloğu açıldı, K-736 satırı | düşük |
| S-7 | `10 §4.1` ÖK-7 | İlk yönetici hesabı adsız anlatılıyor | K-736 | Kurulumda adı, e-postası ve şifresiyle | düşük |
| S-8 | `04 §1.1.10` (F-1…F-4'ün sekiz satırı) | "İşaretin biçimi UI9-01" — workshop öncesi hâl | K-760 | Satıra işaret düşmez, ana sayfanın uyarısı; ekran hücresine E-31; §1.2'de E-31 67 → 75 | düşük |
| S-9 | `04 §1.1` (`03 §8.8.2`, §9.3.7, §8.1.1, §8.9.1) | Geri beslenen satırların özetleri eski metin | K-736, K-737, K-760, K-761 | Özetler güncel metne | düşük |

**Geri işaret ve etki sütunu:** düzeltilen her yüz ilgili kararın etki sütununa `(taslak güncellendi — vX.Y)` ile eklendi (K-736, K-737, K-759, K-760, K-761, K-770, K-787, K-790, K-796, K-830, K-831, K-833, K-839). **Betik 2:** gövdede yeni anılan K numaraları Kaynak satırlarında — `02 §10` (K-796), §6 (K-787), `04 §4` (K-839), §5.19 (K-790), §7.1 (K-833); `03` ve `10`'da satırın Kaynak sütunu.

### 5.3 Park satırları

**(a) Aşama 1 ve 2'nin park satırları — numaraya dayalı tarama (mercek 1).** 21 kaynak karar (K-06, K-10, K-12, K-13, K-14, K-17, K-22, K-23, K-24, K-27, K-97, K-531, K-574, K-599, K-601, K-616, K-666, K-667, K-687, K-699, K-705). Kaba eşleşme 273 — 23'ü başka kimliklerin kuyruğu (`ÖK-10`, `SK-10`, bulgu kimlikleri) —; **250 eşleşmenin hepsi okundu**, 65'i kaynağı değiştiriyor, daraltıyor, genişletiyor ya da yerleşimini taşıyor. Aşama 2'nin taraması bu satırları ilişki sözcüğüyle aramıştı; numaraya dayalı tam okuma ilk kez yapıldı.

| # | Park satırı | Değiştiren karar | Hizasızlık | Düzeltme | Geri işaret |
|---|---|---|---|---|---|
| P-1 | `08` — K-12 | K-159, K-163, K-378 — kart ödemesi havaleyle ikame edilebilir | "Ödeme sağlayıcı sözleşmesi canlıya çıkış öncesi dış ön koşuldur" — kartsız canlı kurulum tanımlı bir hâldir (`10 §4.1` ÖK-4) | Sözleşme ve anahtarlar kart yönteminin ön koşuludur; tanımlı değilse satış havaleyle açılır | K-12'ye eklendi |
| P-2 | `06` — K-17 · K-531 | K-617 — Hesap türü kapalı listesi | Kapalı listelerin sayımında Hesap türü yok; değerleri sözlükten devralınacak gibi kalıyordu | Liste Hesap türü'nü sayar, K-617 anılır | K-531'e eklendi |
| P-3 | `12` — K-22 | K-415, K-417, K-418 — çıtanın ölçülebilir karşılığı; sipariş hacmi kriter değildir | Yalnız "düzenli sipariş alır" kalın; elenmiş hacim kriterine yöneltebilir | Sınanan karşılık Ü-1…Ü-4; hacim senaryoya çevrilmez | K-22'ye eklendi |

**Değişiklik gerekmeyen adaylar:** K-06 ← K-308, K-309, K-312, K-383, K-526, K-638 (tek rolün ayrıntısı; aktör sayısı değişmedi) · K-10 ← K-440, K-446, K-470, K-471 (yerleşim; içerik değişmedi) · K-12 ← K-359, K-518 (kart verisi sınırı korunur) · K-14 ← K-480, K-627, K-628 (satır tip sayısı yazmaz) · K-17 ← K-526, K-645 · K-23 ← K-415, K-418, K-445, K-637 ("kabul kanıtı" terimi Ü-4 ile aynı) · K-24 ← K-435, K-638 · K-27 ← K-237…K-258, K-746, K-753, K-755, K-813 (taban kural korunur) · K-599 ← K-603, K-698, K-733, K-734, K-782 (bildirimin içeriği değişmedi) · K-616 ← K-692 (F-6 ayrı e-posta) · K-666, K-667 ← K-685, K-706 (satırda var ya da konusu dışında). Numara taşımayan iki ilişki ayrıca okundu: K-754 (boş katalogda ürün vitrini görünmez — `04`'ün K-27 satırı K-754'ü anar) ve K-839 (`08`'in K-735 satırı bağlantıyı anar, tetiği yazmaz).

**(b) Aşama 3'ün park satırları.** 37 kaynak karar, 44 eşleşme; hepsi okundu. Değiştirenler: K-756 → K-831, K-833 (satırın etiketinde zaten var) · K-793 → K-830 ("açık adet" okunuşu satırda var; K-830'un etki sütunu "değişmedi" der) · K-787 → K-796 (`07` ve `12` satırları feshi doğru yazar) · K-745 → K-765, K-840 · K-758 → K-768, K-844 · K-747 → K-842 · K-760 → K-769 · K-823 → K-836 — hiçbiri park satırının talimatını değiştirmiyor. **Hizasız satır yok.** K-834'ün `05` ve `12` satırları bu taramanın K-850'siyle güncellendi.

**(c) Park satırı ↔ etki sütunu (iki yönlü betik).** Beş karar bir park satırının etiketindeydi ama etki sütunu hedefi saymıyordu: **K-741 → `12`, K-760 → `08`, K-791 → `12`, K-794 → `06`, K-795 → `07`** — etki sütunlarına işin adıyla eklendi. `12`'nin "K-787…K-796" etiketi senaryosu olmayan kararları (K-792, K-793, K-795) da kapsıyordu; etiket **"K-787…K-791 · K-794 · K-796"** oldu — senaryoların (1)–(9) kararları. Ters yönde: etki sütununda park diyen her atfın park satırı var; üç atıf bilerek park satırı açmaz (K-780 → `05`, K-825 → `06`: "park gerekmez — `04` işaret eder"; K-830 → `06`, `07`: K-793'ün satırları). `06`'nın ölçü birimi satırının kaynağı karar satırı değil, Ürün Gereksinimleri §3.8.5 ve audit bulgusu FORM-16'dır.

### 5.4 Betikle aranan ve temiz dönen çakışma sınıfları

| # | Sınıf | Sonuç |
|---|---|---|
| 1 | **Kimlikler** — `04`, `03`, `10` ve park bloklarının B-, F-, Z-, L-, P-, H- kimlikleri `02`'deki kümede | ✓ — `comm` farkı boş; en büyükler B-16, F-6, Z-47, L-9, P-49 (`04`'te P-44), H-4; kaldırılmış P-2 ve P-11 hiçbir yerde geçmiyor; `04`'te S1…S11 ve Ö1…Ö5 dışında geçiş kimliği yok |
| 2 | **Ekran kimliği** — `E-nn` öteki dokümanlarda | ✓ — `01`, `02`, `03`, `10` ve park bloklarında `E-nn` yok (kimlik yalnız `04`'te ve karar kaydında); `04`'te E-01…E-54, düşen E-48 yalnız düştüğü yerlerde |
| 3 | **KP satırları ↔ matris** | ✓ — `10 §2`'nin 77 KP satırının 77'si `04 §1.1`'de; KP-14'ün özeti cayma özetini aldı (R-7) |
| 4 | **Durum adları** — yanlış biçimler ("Kargoda", "Teslim alındı", "Ödenmedi", "Arşivlendi", "Kısmi geri ödendi", "Kapandı" …) | ✓ — "Kargoda" yedi eşleşmesi "kargodaki/kargodayken" sıfatıdır; öteki biçimler sıfır |
| 5 | **Aktör adları** — "misafir müşteri", "kayıtlı müşteri", "üye kullanıcı", "site yöneticisi" | ✓ — sıfır; `04`'te iki "admin" şablonun bölüm başlığıdır (konvansiyon 7) |
| 6 | **K atıfları** — `01`, `02`, `03`, `10`, `04` ve park bloklarındaki her K kayıtta | ✓ — eksik yok; etki sütunu `04`'ü gösteren 127 Aşama 3 kararının 127'si `04`'te anılır |
| 7 | **Geri beslenen on kuralın yüzleri** | Okunabilirlik (K-832), dışa aktarmanın yeri (K-836), ürün listesi araması (K-737 — gövde) temiz; öteki yedisinin eski yüzleri §5.2'de düzeldi |
| 8 | **Sürümler** — tracker §1, başlıklar, dosya sonu notları | ✓ — taramadan önce `01` v0.33, `02` v0.64, `03` v0.20, `04` v0.17, `10` v0.42 birebir; bu PR'da birlikte arttı |
| 9 | **Kısmi adetin müşteri yüzü** — fesihte adet seçtiren, kalemi bölünmez sayan ya da kupon hakkını adetle döndüren yüz | ✓ — yok (mercek 2: `02` ~152, `03` ~124, `04` ~111 uyumlu satır) |

---

## 6. Devir dizini — Aşama 3 kararlarının sonraki dokümanlara devirleri

**Tek ev:** devrin evi karar satırının etki sütunu ya da park satırıdır; bu dizin devri ikinci kez kaydetmez. Aşama 2'de 137 atfın 86'sı yalnız doküman numarasıydı; **Aşama 3'te yalnız numara taşıyan atıf yoktur** — Aşama 2'nin öğrenim terfisinin kuralı (checklist §3, "işin adı yazılır") işledi. Her hedef dokümanın "Aşama 3'ten park edilen girdiler" bloğu bu bölüme işaret eden bir dizin satırı taşır; `DEPLOY_RUNBOOK`'ta blok bu taramada açıldı.

- **Önceki aşamaların dizinleri:** `PHASE1_CONFLICT_SCAN.md` §6 ve `PHASE2_CONFLICT_SCAN.md` §6.
- **`09` Kodlama Kılavuzu ve `11` Uygulama Planı:** Aşama 3'ten devir yok; `11` tüketicidir ve `04`'ü kendi matrisinde okur (`04` GA-7'nin notu).
- **`DEFERRED_BACKLOG.md`:** Aşama 3'ten kalem yok.
- **Toplandığı yer (6. adım, 2026-10-05):** Aşama 4'e devrin bütün girdileri — bu dizin dahil — karar kaydının §11'inde tek tabloda.
- **Süreç kararları** (K-723…K-731, K-738, K-762, K-763, K-778, K-847…K-849) sonraki dokümana iş bırakmaz.

### 6.1 Etki sütunlarındaki devirler (K-723…K-850)

| Hedef | Atıf | Karar | Park satırında | Park gerekmez / anma |
|---|---|---|---|---|
| `05` Teknik Mimari | 9 | 9 | 8 | 1 (K-780) |
| `06` Veri Modeli | 14 | 14 | 12 | 2 (K-825, K-830) |
| `07` API Tasarımı | 6 | 6 | 5 | 1 (K-830) |
| `08` Entegrasyon Spesifikasyonu | 5 | 5 | 5 | 0 |
| `12` Doğrulama Protokolü | 18 | 18 | 18 | 0 |
| `DEPLOY_RUNBOOK.md` | 1 | 1 | 1 | 0 |
| **Toplam** | **53** | **40 karar** | **49** | **4** |

#### `05` Teknik Mimari — 9 atıf

| Karar | İş |
|---|---|
| K-745 | beklenmeyen hatanın üç hâle ayrılması ve kaydı |
| K-758 | onaylanmamış ödeme adımında yazılanların tutulduğu yer |
| K-760 | firma bildirimleri uyarısının çıktığı ve kalktığı anın tespiti |
| K-780 | doğrulama bağlantısının hâlinin tespiti — park gerekmez, `04` 5.21.9 işaret eder |
| K-800 | eşzamanlı düzenlemenin mekanizması (`06` ile birlikte) |
| K-801 | önerilen stok kodunun biçimi ve üretimi |
| K-834 | ilk kurulumda çerez yazılmamasının güvencesi |
| K-839 | geri alma bağlantısının açılmasının durum değiştirmemesi |
| K-850 | ilk kurulumda çerez — veri toplayan girişlerin kapısının çerez politikasını beklemesi |

#### `06` Veri Modeli — 14 atıf

| Karar | İş |
|---|---|
| K-736 | yönetici hesabının ad alanı |
| K-787 | kalem kayıtlarında işlem görmüş adedin tutulması |
| K-788 | işlem başına ayrılan kupon payı ve KDV'nin tutulması |
| K-790 | kalem kayıtlarında adet, açık adedin okunuşu, aynı adedi kullanan eşzamanlı işlemin önlenmesi |
| K-791 | sayaçların adetle dönüşü |
| K-793 | ayıp talebinde adet |
| K-794 | kalem kayıtlarında firma işlemlerinin adedi — bu taramada eklendi |
| K-800 | araya giren kaydın karşılaştırılan hâli |
| K-804 | gönderim kaydının siparişe bağlanması |
| K-806 | kupon kodunun tekilliği |
| K-811 | doğrulanmamış kaydın tutulması ve aramada bulunması |
| K-817 | iade adresinin alanları |
| K-825 | telefon ve IBAN alanının saklanan biçimi — park gerekmez, `04` 7.3.2 işaret eder |
| K-830 | ayıplı adedin üst sınırı ("açık adet") — K-793'ün satırı değişmedi |

#### `07` API Tasarımı — 6 atıf

| Karar | İş |
|---|---|
| K-737 | ürün listesinin arama ve süzme parametreleri |
| K-787 | müşteri işlemlerinin adet parametresi |
| K-794 | yöneticinin kalem işlemlerinin adet parametresi |
| K-795 | açık adedin ve varsayılanın ekrana sunulması — bu taramada eklendi |
| K-802 | sipariş listesinin arama ve süzgeç parametreleri |
| K-830 | ayıplı adedin üst sınırı — K-793'ün satırı (`07`'de K-787 satırı) değişmedi |

#### `08` Entegrasyon Spesifikasyonu — 5 atıf

| Karar | İş |
|---|---|
| K-735 | üç bildirimin metni — e-posta değişikliği bildirimi, B-10, F-5 |
| K-759 | yeniden gönderilen bildirimin içeriği |
| K-760 | firma bildiriminin ulaşmadığının anlaşılması — bu taramada eklendi |
| K-824 | tarih, saat ve tutarın e-postadaki biçimi |
| K-832 | yasal metinlerin e-postadaki okunabilirliği |

#### `12` Doğrulama Protokolü — 18 atıf

| Karar | İş |
|---|---|
| K-740 | üç genişlik sınıfının her birinde doğrulama |
| K-741 | iki tarafın sınıflara göre düzeni — bu taramada eklendi |
| K-747 | kimlik bloğu ve ETBİS bandının her vitrin sayfasında doğrulanması |
| K-756 | onay bölümünün sırası ve metinlerin görünürlüğü |
| K-787 | kısmi adet senaryoları — kısmi iptal, ardışık cayma |
| K-788 | kısmi adet senaryoları — kuruş yuvarlaması |
| K-789 | kısmi adet senaryoları — kargo ücretinin iki tetiği |
| K-790 | kısmi adet senaryoları — ardışık işlemler, kapanış kuralı |
| K-791 | kısmi adet senaryoları — kupon hakkının yalnız bütün adetlerle dönmesi — bu taramada eklendi |
| K-794 | kısmi adet senaryoları — firma iptali, eksik ulaşan iade |
| K-796 | kısmi adet senaryoları — feshin açık adetlerin tamamına uygulanması |
| K-798 | panelin yasal uyarılarının göründüğü hâller |
| K-808 | ayıpta sözleşmeden dönme |
| K-823 | dışa aktarmanın iz satırı ve boş sonuç |
| K-831 | onay bölümünde cayma özeti |
| K-833 | özet farkında iki yasal metnin kutularının işaretinin kalkması |
| K-834 | ilk kurulumda çerez yazılmaması |
| K-850 | aydınlatma önce yayına alınsa da veri toplayan girişlerin kapalı kalması |

#### `DEPLOY_RUNBOOK.md` — 1 atıf

| Karar | İş |
|---|---|
| K-736 | ilk yöneticinin adının kurulumda verilmesi (kurtarmada açılan hesap dahil) — blok ve satır bu taramada açıldı |

### 6.2 Arayüz Tanımları'nın gövdesinde sonraki dokümanı anan cümleler

Mekanik çıkarım (`04 §1.3` ve §2–§9); başlık notu ve matrisin eşleme hücreleri hariç. **Devir:** işi o dokümana bırakan cümle. **Anma:** park satırını ya da devri yalnız gösterir. **Dayanak:** devrin karar satırı ya da üst dokümandaki evi; **yalnız `04`** — tek yazılı yeri bu cümledir.

| `04` | Hedef | Tür | İş | Dayanak |
|---|---|---|---|---|
| 2.1.1 | 05 | devir | **üç genişlik sınıfının sınırlarının nasıl uygulandığı** | **yalnız `04`** — K-740 yalnız `12`'ye park eder |
| 2.1.3 | 08 | devir | **marka renginin e-postadaki yeri** | **yalnız `04`** — K-742'nin etki sütununda `08` yok |
| 2.1.4 | 12 | devir | Erişilebilirlik Kontrol Listesi maddelerinin doğrulamada eşlenmesi | `02 §3.34.5`, §12.4.1 (K-625) |
| 2.10 | 05, 06 | devir | eşzamanlı düzenlemenin mekanizması | K-800 — park |
| §3.5 girişi | 08 | devir | e-postanın metni ve bağlantının biçimi | `03 §7` girişi; K-689 |
| §3.5 girişi | 07 | devir | **sitenin dışından gelinen sayfa adreslerinin biçimi** | **yalnız `04`** — adresin kaydın adından üretilmesi `02 §3.30.2`'de, matriste "mimari (`05`)" |
| §3.5 (F-1…F-6 notu) | 08 | devir | **firma bildirimlerinin taşıdığı bağlantının biçimi ve panelde indiği ekran** | **yalnız `04`** — `03 §7` girişi e-postanın biçimini genel olarak bırakır |
| 4.4.1 | 08 | anma | e-postaların metni ve şablonu | `03 §7` girişi; K-735 |
| 4.4.2, 4.4.3 | 08 | anma | ödeme sağlayıcısının ve Google'ın sayfası | Aşama 1 (K-12, K-103) |
| 4.4.5 | 05 | devir | arama motoru verileri, site haritası, paylaşım önizlemesi | `10 §2` KP-37, `02 §3.30.5` — Aşama 2'nin dizini (`03` 0.1.4) |
| 5.21.9, 9.25.8 | 05 | devir | doğrulama ve geri alma bağlantısının hâlinin tespiti | K-780 — etki sütunu |
| 5.28.6 | 08 | anma | geri alma bildiriminin metni | K-735 — park |
| 7.2.4.13 | 06 | devir | ölçü birimlerinin listesi | FORM-16 — park |
| 8.2 (biçim notu) | 08 | devir | e-postadaki tarih, saat ve tutar biçimi | K-824 — park |
| §1.3 (GAP-4, GAP-5, GAP-10, K-759, K-760, K-831/K-832, K-839, K-850 satırları) | 05, 08, 12 | anma | park satırlarının işaretleri | K-758, K-735, K-745, K-759, K-760, K-832, K-839, K-850 |
| §1.2 (GA-7) | 11 | anma | yasal metin kuralı Uygulama Planı'nın matrisine kendiliğinden girer | — |

**Karar satırı olmadan devredilen dört iş:** (a) genişlik sınıflarının uygulanması → `05`; (b) marka renginin e-postadaki yeri → `08`; (c) sayfa adreslerinin biçimi → `07`; (d) firma bildirimlerinin bağlantısının biçimi → `08`. Dördü de detaydır — varlık kararları kayıtlıdır (K-740, K-742, `02 §3.30.2`, K-760) — ve kapısı hedef dokümanın yazımıdır. Yeni karar istemedi; hedef dokümanların park bloğundaki dizin satırı onları adıyla anar. Kural 1 açısından açık kalem değildir: tek evleri bu cümlelerdir.

### 6.3 Matrisin ekran dışı değerleri

`04 §1.1`'in "ekran dışı" değer kümesi (`04 §1.1.2`) kaynak satırlarını sonraki dokümana eşler: e-posta (`08`) **151 satır**, mimari (`05`) **13**, doğrulama (`12`) **4**. Bunlar devir değil eşlemedir: satırların devri kaynak kararlarındadır ve dizinleri Aşama 1 ile Aşama 2'nin raporlarındadır. `08`'e giden 151 satırın 130'u `03 §7`'nin bildirim haritası (126) ve `02 §9`'dur (4) — K-689'un "e-posta şablonları haritadan türer" devri.

---

## 7. Öğrenim terfisi adayları — 5. adımın girdisi

Metinler tracker §10.1'dedir; bu dizin onları tek yerde gösterir.

| # | Aday | Kaynak | Playbook | Durum |
|---|---|---|---|---|
| 1 | Şablonun ekran kimliği başka bir kimlik ailesiyle çakışıyor | K-725 | PF-54 | Bekliyor |
| 2 | Arayüz Tanımları şablonunun üç eksiği | K-724, K-726 | PF-55…PF-57 | Bekliyor |
| 3 | Girdiler tek bağlama sığmıyor | Açılış | — | Bekliyor |
| 4 | Kesilerek okunan matris hücresi ikinci ekranı kaçırıyor | K-738, K-739 | — | Bekliyor |
| 5 | Şablonun iki metin hatası ve checklist'in aynı cümlesi | Audit (ATIF-7, ATIF-9) | PF-58 | Bekliyor |
| 6 | Büyük dokümanda tek parça cross-review çağrısının dikkat sınırı | K-848 | — | Bekliyor |
| 7 | Büyük dokümanda cross-review'ın çıkış koşulu | K-849 | — | Bekliyor |
| 8 | **Geri besleme betiği 1 kuralın yan yüzlerini kaçırıyor** | **Bu tarama** (§5.2) | — | Bekliyor |
| 9 | **Park satırının etiketi etki sütunuyla iki yönlü eşlenmiyor** | **Bu tarama** (§5.3 c) | — | Bekliyor |
| 10 | **Kararın gerekçesindeki olgusal öncül kuralın evine karşı doğrulanmadı** | **Bu tarama** (§5.1 — K-850) | — | Bekliyor |
| Ö-23 | Checklist'in skill'e dönüştürülmesi | Aşama 1'den | PF-32 | Bekliyor — kapı Aşama 3'ün öğrenim terfisi |

**Bu taramanın gözlemi — kurala kanıt:** Aşama 2'nin öğrenim terfisinin iki kuralı bu aşamada işledi — numaraya dayalı park taraması Aşama 1'in üç hizasız satırını buldu (P-1…P-3; ilişki sözcüğü taşımayan K-417 ve K-617 dahil) ve "devir yazılırken işin adı yazılır" kuralı yalnız numaralı atfı sıfıra indirdi (Aşama 2: 137'de 86).

---

## 8. Checkpoint'e notlar

- Tracker §4'te açık satır yok, A-19 açılmadı; Ürün Gereksinimleri §13 boş; `04`, `03`, `10` ve `01`'de açık kalem yok — checkpoint'in açık karar kontrolü bu raporla karşılanır.
- ⚠ listesi **40 karar** (§4.1) — hazır metin aynı yerde; K-848 ve K-849 süreç kararı olarak listenin sonunda. Arşiv işaretinden önce proje sahibine tek mesajda gösterilir.
- K-850 bu taramanın tek kararıdır ve bir ürün kuralını değiştirdi (veri toplayan girişlerin kapısı). Checkpoint'in iç tutarlılık merceği kapının yüzlerini (§5.1'in listesi) bir kez daha okuyabilir.
- K-849'un devri — iki yerin metin farkı sınıfı — checkpoint'in iç tutarlılık merceğindedir; bu taramanın yirmi üç hizalamasının çoğu aynı sınıftandır (genel giriş maddesi ↔ ayrıntı satırı, matris özeti ↔ gövde) ve kanıt olarak kullanılabilir.
- Aşama 1–2'nin park satırları bu raporda numaraya dayalı tam okumayla bir kez tarandı (250 eşleşme); Aşama 3'ün park satırları da (44 eşleşme). Checkpoint yeniden taramaz; yalnız bu PR'ın değiştirdiği satırları okur.
- 6. adımda Aşama 4'e devrin tablosu bu raporun §6'sını ve hedef dokümanların park bloklarındaki dizin satırını gösterir; öğrenim adayları dizini (§7) 5. adımın girdisidir.

# Aşama 6 — Çakışma Taraması

**Tarih:** 2026-10-28 | **Aşama:** 6 — API tasarımı | **Kapanış adımı:** K-437'nin 3. adımı (K-429; `checklists/document-stage.md` §7)
**Girdi sürümleri:** karar kaydı v0.169 · API Tasarımı (`07`) v0.23 · Ürün Gereksinimleri (`02`) v0.93 · Kullanıcı Akışları (`03`) v0.40 · Arayüz Tanımları (`04`) v0.40 · Teknik Mimari (`05`) v0.29 · Veri Modeli (`06`) v0.31 · MVP Kapsamı (`10`) v0.55 · Proje Vizyonu (`01`) v0.33 · `08`, `09`, `12` ve `DEPLOY_RUNBOOK`'un park blokları
**Çıktı sürümleri:** karar kaydı v0.170 · `07` v0.24 · `04` v0.41 · `06` v0.32 · `02`, `03`, `05`, `10` ve `01` değişmedi · `08`, `09`, `12` ve `DEPLOY_RUNBOOK`'un park blokları (sürüm alanı değişmez)

> **Ne işe yarar:** Aşama kapanmadan önce açık kalemlerin her birinin **tek bir evi** ve **gözlemlenebilir bir kapısı** olduğunu doğrular; vadesi geçmiş açıkları adıyla listeler (K-429). Önceki aşamaların park satırlarını ve bu aşamanın park satırlarını, kaynak kararlarını sonradan değiştiren kararlara karşı okur. Aşama 6 kararlarının (K-1144…K-1228) önceki aşamaların kararlarına geri işaretlerini, birbirleriyle zincirlerini denetler. Gösterilmemiş ⚠ kararların hazır metnini verir (§4.1). Aşama 6 kararlarının sonraki dokümanlara devirlerini tek dizinde toplar (§6) — **Entegrasyon spesifikasyonları aşamasının açılışı bu dizini okur.** Sonuç checkpoint'in (4. adım) girdisidir.

---

## 1. Sonuç

| Kural | Sonuç |
|---|---|
| **(1) Aynı açık iki yerde durmaz** | ✓ — açık karar yok. Tracker §4'ün on sekiz satırının hepsi kapalı; Aşama 6'da A-19 açılmadı (31 geçişin hepsi "A-19'dan sürer" kural cümlesi). API Tasarımı'nın şablonunda açık kararlar bölümü yoktur ve gövdesinde "açık" işaretli madde yok; Ürün Gereksinimleri §13 tablosu boş (§3). |
| **(2) Her açık satır gözlemlenebilir bir kapı taşır** (K-37) | ✓ — tracker §16.1'in açık dört maddesinin dördü de kapılı (§4): Ö-23 → öğrenim terfisi; maliyet düzeni denemesi → öğrenim terfisi (ölçüm karşılaştırması); ⚠ listesi → arşiv işaretinden önce (hazır metin §4.1); öğrenim adayları → öğrenim terfisi (bu taramadan 7–9 eklendi — §7). |
| **(3) Vadesi geçmiş ve açık satır adıyla listelenir** | ✓ — **yok.** Kapıların hiçbiri henüz gelmedi (checkpoint, öğrenim terfisi, arşiv işareti). |

'
**Çakışmalar:** park satırları, geri işaretler ve zincirler betikle çıkarıldı; bulgu arayan okuma **15 Sonnet / low** dilim okuyucusunda yapıldı (14 kayıt ve park dilimi + 1 gövde dilimi) ve 49 aday satır çıkardı; aynı yeri anan adaylar 20 bulguda toplandı (üç gövde adayı betikle elendi) ve her bulgu bir **Opus / high** doğrulayıcısı tarafından güncel metne karşı okundu: **4 doğru, 8 kısmen, 8 çürütüldü**. Doğrulanan ve kısmen doğrulanan 12 bulgunun hepsi bu PR'da hedefli düzeltildi — **10'u yeni karar istemedi** (eksik geri işaret, eskimiş etiket ya da cümle), **ikisi karar istedi** (§5.1): **K-1227** — Arayüz Tanımları'nın yönetici geri alma ekranı, eski adres başka bir yönetici hesabına geçtiyse uygulanmayan geri almayı da gösterir · **K-1228** — listelerin sırası eşit anahtarda iç anahtarla biter ve ürün dizini `Id`'yi taşır. Ek olarak okuma sırasında rastlanan dört küçük hizalama da işlendi (§5.2'nin son dört satırı). **Park satırları** (§5.3): Aşama 1–5'in 197 maddesinde K-1144…K-1226'ya geçen 373 eşleşme okundu — 1 hizasız satır (`08`'in dönüşün yolu satırı) düzeldi; Aşama 6'nın 26 maddesinde etiket ↔ etki sütunu iki yönde temiz (düzeltmelerden sonra). **Geri işaret** (§5.4): 402 çiftin 398'i dilimlerde okundu, dördü yazım anında işaretlendi — 9 eksik geri işaret kapandı. **Devir dizini** (§6): Aşama 6'nın **44 kararı** sonraki dokümanlara **76 atıf** taşır; 07 gövdesinde sonraki dokümanı anan 56 satırın 31'i okundu — karar satırı olmadan devredilen **iki iş** park satırı aldı. **⚠ listesi:** kayıtla 9 / 9 (§4.1) — tracker §16.1'in listesi K-1192'yi saymıyordu, düzeltildi. **Öğrenim adayları:** üç yeni (§7). **Çok kritik soru çıkmadı.**

---

## 2. Yöntem

Tarama gözle değil betikle yapıldı (K-429); anlamsal okuma ayrı alt ajanlarda ve ön planda koştu. Ortam: Git Bash ve Python 3.12 (`python -X utf8`). **Model dağılımı (K-1207, K-1224):** bulgu arayan dilim okuyucuları Sonnet / low (15); bulgu doğrulayıcıları Opus / high (20, bulgu başına bir); tarama, sayım ve karşılaştırma betikleri ve düzeltmelerin işlenmesi yönetici adım ajanında. Her okuyucuya yalnız kendi dilimi ayrı bir dosyada verildi; karar kaydı hiçbir ajana bütünüyle okutulmadı.

1. **Tracker §4 ve §16.1:** `## 4.` ile `## 5.` arasındaki `| A-NN |` satırları ayrıştırıldı (18 satır, durum hücresi "Kapandı"ya karşı), A-19 ve sonrası dosyada arandı. §16.1'in `- [ ]` maddeleri çıkarıldı (4) ve her birinin gövdesinde "Kapı:" cümlesi arandı. Dokümanların açık kalem desenleri (`açık karar`, `açık kalem`, `belirlenecek`, `netleşecek`, `karara bağlanacak`, `TBD`, `(downstream)`, `sonra doldurulacak`, `A-19`) `01`, `02`, `03`, `04`, `05`, `06`, `07` ve `10`'da arandı; her eşleşmenin bağlamı okundu.
2. **⚠ listesi:** K-1144…K-1228'in konu hücresinde `⚠` taşıyan satırlar betikle çıkarıldı ve §16.1'in listesindeki `K-n` girişleriyle karşılaştırıldı.
3. **Park satırları — Aşama 1–5:** `04`, `05`, `06`, `07`, `08`, `09`, `12` ve `DEPLOY_RUNBOOK`'un Aşama 1–5 bloklarındaki `> - **…**` maddeleri ayrıştırıldı (197 madde); etiketlerinden kaynak K'lar çıkarıldı (aralıklar açıldı). Her kaynak için **K-1144…K-1226**'da — K-1143'e kadarını önceki taramalar okudu — tam eşleşme arandı (ilişki sözcüğüne bakılmadı): **373 eşleşme**, beş dilime bölünüp okundu (`04`/`05`/`06` · `07` · `08` · `09` · `12` ve `DEPLOY_RUNBOOK`).
4. **Park satırları — Aşama 6:** `08`, `09`, `12` ve `DEPLOY_RUNBOOK`'un Aşama 6 bloklarının maddeleri etikete ve metne göre ayrıştırıldı (24 → 26 madde); etiketteki her kararın etki sütunu hedefi anıyor mu, etki sütunu hedefi anan her karar bir etikette geçiyor mu denetlendi (iki yön).
5. **Geri işaret ve zincirler:** K-1144…K-1228'in konu, karar ve gerekçe hücrelerinde anılan eski K'lar çıkarıldı (402 çift: 199 Aşama 1–5, 203 Aşama 6 içi); her çift için eski satırın etki sütununda yeni numara (ya da aralığı) aranıp **geri işaret var / yok** işaretlendi. 398 çift (K-1227 ve K-1228'in dört çifti hariç — bunlar yazım anında işaretlendi) dokuz dilimde okundu: eski karar ile yeni karar arasında değişiklik / daraltma / genişletme / tersine çevirme / çelişki var mı.
6. **Devir dizini:** K-1144…K-1228'in etki sütunu ` · ` ile (parantez içi korunarak) parçalandı; `08`, `09`, `11`, `12` ya da `DEPLOY_RUNBOOK`'u anan parçalar alındı (yazım biçimleri: `08 (…)`, `09 / 12 (park)`, `DEPLOY_RUNBOOK (…)`) ve kararın hedef dokümanın Aşama 6 bloğundaki yeri bulundu. `07`'nin §2–§10 gövdesinde sonraki dokümanı numarasıyla ya da adıyla anan satırlar çıkarıldı (56); satırdaki K'lardan biri ya da "park" sözü hedefin devir listesinde yoksa 31 satır bir okuyucuya ve gerekirse bir doğrulayıcıya verildi.
7. **Doğrulama:** her aday bulgu için bir Opus / high ajanı güncel metne karşı okudu (`[dogrulama]`; bulgu başına bir, ikinci doğrulayıcı yok); hüküm DOĞRU / KISMEN / ÇÜRÜTÜLDÜ ve gereken düzeltme dosyalara yazıldı. Doğrulanan düzeltmeler yönetici ajanında hedefli işlendi; her düzeltme bir betikle yapıldı ve uygulanan metin eski metne karşı `assert` edildi.

---

## 3. Açık kalemler

### 3.1 Tracker §4

| Kapsam | Satır | Açık | Sonuç |
|---|---|---|---|
| A-01…A-18 (Aşama 1, salt okunur) | 18 | 0 | Aşama 1'in taraması §3.1 |
| A-19 ve sonrası | 0 | 0 | Aşama 6'da açık detay doğmadı: matrisin, workshop'un, yazımın ve kalite döngüsünün bulduğu her boşluk bir K satırıyla kapandı (K-1144…K-1226); bu tarama K-1227 ve K-1228 ile kapattı |

### 3.2 Dokümanlar

| Kaynak | Bulunan | Sonuç |
|---|---|---|
| API Tasarımı (`07`) | Şablonda açık kararlar bölümü yok. Desen eşleşmeleri: "açık karar" 1 (kural cümlesi), "karara bağlanacak" 5 (park bloklarının kalıp cümlesi), "açık kalem" 30 (22'si siparişin açık kalemi terimi; kalanı "son açık kalem" gibi kalem durumları) | Açık kalem yok |
| Ürün Gereksinimleri §13 | Tablo boş; 13.1–13.4 kural cümleleri ve kapatılmış A-05, A-12 kaydı | Açık karar yok |
| Kullanıcı Akışları, Arayüz Tanımları, Teknik Mimari, Veri Modeli, MVP Kapsamı | "açık karar" eşleşmeleri kural cümleleri ve eski başlık notları; "açık kalem" eşleşmeleri siparişin açık kalemi terimi; "karara bağlanacak" park bloklarının kalıp cümlesi; "(downstream)" konvansiyon cümlesi; "A-19" "A-19'dan sürer" kuralı | Açık kalem yok |
| Proje Vizyonu | Eşleşme yok | — |

### 3.3 Raporlarda kalmış açık not

Yok. Kalite döngüsünün devredilen üç açık notu (hakkın düştüğü an, §2.7.1'in genel 409 cümlesi, matrisin özet satırlarının niteleyicisi) etki yansıtmada kapandı (K-1225); cross-review TEMİZ döndü (5 tur, çıkış kuralıyla). Audit, deep review ve cross-review raporlarında açık kalem yok.

---

## 4. Süreç maddeleri ve kapıları

Tracker §16.1'in işaretsiz (`- [ ]`) maddeleri dörttür. Devir §15'in girdileri, `07`'nin gövdesi ve hafızanın Güncel Durum bloğu ayrıca tarandı.

| Madde | Ev | Kapı | Sonuç |
|---|---|---|---|
| Bekletilen öğrenim Ö-23 — checklist'in skill'e dönüştürülmesi | Tracker §16.1 · PF-32 · devir §15'in 7. satırı | API tasarımı aşamasının öğrenim terfisi (5. adım) | Kapı var; proje sahibine yeniden sorulur (K-991, K-1143) |
| Maliyet düzeni denemesi (K-1145; K-1207 ve K-1224 ile değişti) | Tracker §16.1 | Öğrenim terfisi — ölçüm tablosu Veri modeli aşamasının tabanıyla karşılaştırılır, işe yarayan maddeler terfi önerisi olarak sunulur | **Kapı var** (gövdede açık yazılı); bu tarama kendi satırını ölçüm tablosuna ekledi |
| ⚠ öneriyle kayıt listesi (K-1144) | Tracker §16.1 | Aşama 6'nın arşiv işaretinden (6. adım) önce proje sahibine tek mesajda | **Kapı somutlaştı** — hazır metin §4.1 |
| Öğrenim terfisi adayları | Tracker §16.1 | Öğrenim terfisi (5. adım) | Kapı var; 1–6 tracker'da, bu taramadan 7–9 eklendi (§7) |
| Playbook listesinin gönderimi (K-649) | `PLAYBOOK_FEEDBACK.md` | Proje tamamlandıktan sonra, proje sahibi | Kapı var |
| D-01 — kurulumun ertelenen parçası | `DEFERRED_BACKLOG.md` | İlk implementation task'ından önce | Kapı var; Aşama 6'dan yeni kalem yok |
| Desteklenen ödeme sağlayıcısının sayısı (çok kritik soru adayı; K-932) | `08`'in Aşama 4 park satırı | Entegrasyon spesifikasyonları aşamasının açılışı | Kapı var; API tasarımı aşamasının sorusu değildir |

### 4.1 ⚠ öneriyle kayıt listesi — doğrulama ve proje sahibine gösterilecek metin

> **Sonuç (2026-10-29, K-1231):** bu dokuz karar ve checkpoint'in K-1229'u proje sahibine tek mesajda gösterildi; itiraz yok. On işaret `(öneriyle kaydedildi — ⚠ — gözden geçirildi 2026-10-29, itiraz yok)` oldu.

**Betikle doğrulama.** Konu hücresi `(öneriyle kaydedildi — ⚠)` taşıyan Aşama 6 satırı (K-1144…K-1228) **9**; tracker §16.1'in listesindeki giriş **8** (K-1161, K-1164, K-1185, K-1199, K-1202, K-1210, K-1211, K-1213); **fark K-1192** — satırın konu hücresi ⚠ taşıyor, §16.1'in "yazım turunun 3a oturumunun listesi" ise "boş" yazıyordu (K-1192 K-1164 (3)'ün ⚠ kuralını uygular; kuralı sorulmadan kaydeden karar K-1164'tür ama satır işaretlidir). Kayıtla birebir olması için K-1192 listeye alındı ve §16.1'in 3a girişi düzeltildi: **9 / 9, eksik 0, fazla 0.** Aşama 6'nın 85 satırının işaret dağılımı: ⚠ 9 · `(öneriyle kaydedildi)` 72 · işaretsiz 4 (K-1144, K-1145, K-1207, K-1224 — proje sahibinin kararları). `(toplu onayla kaydedildi)` yok. Bu taramanın kararları K-1227 ve K-1228 `(öneriyle kaydedildi)` taşır, ⚠ değildir (yöntem ve ayrıntı kararları; seçenekler yakın, hukuki ya da ticari risk yok).

| Adım | Kararlar | Sayı |
|---|---|---|
| Matrisin 2. oturumu | K-1161 | 1 |
| Workshop | K-1164 | 1 |
| Yazım turunun 2b oturumu | K-1185 | 1 |
| Yazım turunun 3a oturumu | K-1192 | 1 |
| Audit | K-1199 · K-1202 | 2 |
| Deep review | K-1210 · K-1211 · K-1213 | 3 |
| **Toplam** | | **9** |

**Kapı.** Liste arşiv işaretinden (6. adım) önce proje sahibine **tek mesajda**, karar başına bir sade cümleyle ve gerekçesiz gösterilir (`INSTRUCTIONS.md` §2, "Ortak kural"); yöneticiden proje sahibine gider (§9). İtiraz gelmeyen satırın işareti `(öneriyle kaydedildi — ⚠ — gözden geçirildi YYYY-AA-GG, itiraz yok)` olur; itiraz gelen karar yeni bir satırla değişir. Checkpoint ya da öğrenim terfisi yeni bir ⚠ karar kaydederse liste onunla birlikte gösterilir. Bu adımda işaret dönüştürülmedi.

**Hazır metin — proje sahibine gösterilecek liste** (konulara göre; K numarası cümleyi taşımaz):

*Sepet, ödeme ve sipariş*
- Ödeme adımında "Sepete dön" denince müşterinin yazdıkları da tarayıcıda saklanır; ödeme adımına geri gelince alanlar dolu görünür. (K-1161)
- Ödeme sağlayıcısından dönen müşteri, oturum çerezine bakılmadan siparişinin sayfasına iner; siparişin erişim anahtarı sağlayıcının kayıtlarında ve tarayıcı geçmişinde kalır, e-postadaki bağlantıyla aynı risktir. (K-1185)
- Aynı sipariş onayı ikinci kez gönderilirse yeni sipariş açılmaz, müşteri teyit ekranına gider ve kart siparişinde ödemeyi oradan başlatabilir. (K-1199)
- İptal ve cayma formu ekranda gösterilen açık adedi taşır; arada açık adet değiştiyse — istenen adedi aşmasa bile — istek uygulanmaz ve ekran güncel hâliyle yeniden açılır. (K-1202)
- Ödeme sağlayıcısında bir sipariş için aynı anda tek ödeme oturumu açık kalır; ödenmiş siparişe ikinci bir oturumdan gelen başarılı ödeme müşteriye otomatik iade edilir. (K-1211)

*Erişim ve hesap*
- Başkasına ait bir kaydı ya da hiç olmayan bir kaydı isteyen kişi aynı cevabı alır: "sayfa bulunamadı"; cevap kaydın var olup olmadığını ve kime ait olduğunu söylemez. Sipariş numarası ve e-postayla siparişini sorgulayan misafir, e-postadaki bağlantıyla aynı sipariş sayfasına yönlenir ve bunun için çerez ya da kayıt açılmaz. (K-1164)
- Silinmiş (imha edilmiş) bir iletişim talebinin adresi yönetici panelinde "sayfa bulunamadı" gösterir ve yönetici listeye döner. (K-1192)
- E-posta değişikliğini geri alma bağlantısı kullanıldığında eski adres bu arada başka bir müşterinin doğrulanmış adresi olmuşsa geri alma uygulanmaz; müşteri adresin başka bir hesaba ait olduğunu görür ve hiçbir şey değişmez. (K-1210)

*Yönetici paneli*
- Yöneticinin siparişleri dışa aktardığı dosyada yalnız ödemesi tamamlanmış siparişler olur. (K-1213)

---

## 5. Çakışmalar

### 5.1 Karar isteyen çakışmalar — K-1227, K-1228

| Yer | Çakışma | Karar |
|---|---|---|
| `04` 9.25.5 ↔ K-1218, K-1210 (bulgu F16) | K-1218 yönetici kolunun (UC-187) sayfasının "`04` 9.25.5'in hâlini" bu sebeple göstereceğini söylüyordu; 9.25.5 yalnız "geri alındı" ve "bağlantı geçersiz" hâllerini yazıyordu — eski adres başka bir yönetici hesabına geçtiyse ekranın ne göstereceği Arayüz Tanımları'nda yoktu | **K-1227 (öneriyle kaydedildi):** üçüncü hâl 9.25.5'e yazıldı (geri alma uygulanmadı; adres başka hesaba ait; bağlantının tüketimi geri döner); 6.3.25.4'ün hâl hücresi ve Kaynak satırı K-1210 ve K-1218'i taşır. Elenen: hâli yalnız `07`'de bırakmak |
| `07 §2.6.5` ↔ `06` §4.5, §4.47, §7.5.1, `09` (bulgu F19) | "Eşit anahtarda sıra kaydın iç kimliğiyle kesinleşir; ürün dizinine kimlik eklenmesi `06`/Kodlama Kılavuzu işidir" — cümle deep review'da K'sız girmişti; `06`'nın ürün dizini `Id` içermiyordu, `09`'da ve `12`'de karşılığı yoktu; sayfa sınırında bir ürün iki kez görünebilir ya da kaybolabilirdi | **K-1228 (öneriyle kaydedildi):** eşit anahtarda iç anahtar son anahtardır; ürün dizini (`PublishStatus`, `CreatedAt`, `Id`); kurumsal içeriğin eşit `SortOrder`'ında oluşturulma anından sonra iç anahtar; `09`'a ORDER BY kuralı ve testi park satırı. Elenen: yalnız sorguya `Id` eklemek (dizinden okunamaz) · kuralı yalnız `07`'de bırakmak |

### 5.2 Hizalanan çakışmalar — yeni karar istemeyen

Hepsi var olan bir kararın kuralının değişmeden kaldığı yüzlerin ya da geri işaretlerin hizalanmasıdır; ✓ durumları korunur, kalite döngüsü yeniden açılmaz (K-652).

| # | Yer | Eski hâl | Düzeltme | Şiddet |
|---|---|---|---|---|
| F3 | K-992, K-1141 | Üç çalışma düzeni "Aşama 5 boyunca açık" / "Aşama 5'le sınırlı"; K-1144 ve K-1145 Aşama 6'da yeniden açtı; eski satırlarda geri işaret yok | K-992'ye K-1144; K-1141'e K-1144, K-1145, K-1207, K-1224 geri işareti. `INSTRUCTIONS §9`'un koşum paragrafı (Opus) Aşama 6'da uygulanmıyor — paragrafa not öğrenim terfisine bırakıldı (aday 5) | düşük |
| F4 | K-1145 ↔ K-1224 | K-1145'in etki sütununda K-1224 yok; K-1224 K-1145'in "4–8, 10–12" maddelerini değiştirdiğini söylüyordu — gerçek kapsam 5–8 ve 12'nin doğrulama hükmü kolu | K-1145'e K-1224 geri işareti; K-1224'ün etki hücresi ve §16.1'in K-1224 paragrafı gerçek kapsama çekildi | düşük |
| F5 | K-1179 ↔ K-1191 | Güncel hâlle karşılaştırılmayan işlemlerin listesine K-1191 panelin yeni kayıt formlarını ekledi; K-1179'da geri işaret yok (`07 §2.5.1` güncel) | K-1179'a K-1191 geri işareti | düşük |
| F6 | `07 §2.2.2` ↔ K-1171 (5), K-1163 | "Tek istisna dışa aktarmadır" — görsel yükleme (UC-117) de kayıt yazmayan bir POST olarak 200 döner; K-1171'den K-1163'e geri işaret yok | §2.2.2'nin yazmayan POST listesine görsel yükleme; cümle "Yazan POST'un tek istisnası dışa aktarmadır"; Kaynak satırı; K-1171'e K-1163 geri işareti | düşük |
| F7 | K-1165 ↔ K-1202 | "Müşterinin işlemleri sürüm taşımaz" ile K-1202'nin gizli alandaki açık adedi çelişmiyor (sürüm değil, gösterilen değer); K-1165'te geri işaret yok | K-1165'e K-1202 geri işareti | düşük |
| F8 | K-823 ↔ K-1213; `04` 6.3.24.2 | Dışa aktarmanın kümesi yalnız ödemesi tamamlanmış siparişler; K-823'te geri işaret yok, K-1213'ün etki hücresi K-1193'ü anıyordu (yanlış numara), `04` 6.3.24.2'nin boş hâli "aralıkta sipariş yok" diyordu | K-823'e K-1213 geri işareti; K-1213'te K-1193 atfı kalktı; 6.3.24.2 "dosyaya girecek (ödemesi tamamlanmış) sipariş yok (9.24.3 (1)'in kümesi)" | düşük |
| F9 | K-562 ↔ K-585 (2), K-1185 | Siparişin erişim anahtarı firmanın e-posta düzeltmesinde yenilenir (K-585); K-562'de geri işaret yok; K-1185'in Edge cümlesi bu kurala yeni bir söz getirmez | K-562'ye K-585 geri işareti | düşük |
| F11 | `07 §1.1` ↔ K-1155 | Matrisin okunuş paragrafı konvansiyonun evini yalnız "`07 §2` ya da §5, §6, §8" diye yazıyordu; K-1155 uç sınıfı kurallarının kendi sınıfında yaşamasını ekledi | §1.1'in cümlesi K-1155'e hizalandı | düşük |
| F16 | K-1210 ↔ K-1218 | K-1210'da K-1218 geri işareti yok | K-1210'a K-1218 ve K-1227 geri işareti; ekranın hâli için K-1227 | düşük |
| F18 | `08` Aşama 4 park satırı (K-906 · K-973) ↔ K-1185 | Satır dönüşün yolunu ve yöntemini hâlâ "bu aşamanın ve `07`'nin kararı" diye açık bırakıyordu; K-1185 yolu kapattı (etki sütunu satırı "devir kapandı" sayıyordu, metin güncellenmemişti) | Satır: yöntem `08`'in, yol ve iniş `07 §4.29.1`'de kapandı; `07`'nin iki matris hücresi (`04` 3.5.8; `08` park A4-3) satıra hizalandı; K-1185'in etki hücresi güncellendi | düşük |
| F20 | `07 §4.31.1` ↔ `09` | 206 yanıtının güçlü `ETag` / tutmayan `If-Range` için tam dosya (200) kuralı "`09` devri" diye yazılmış, `09`'da karşılığı yoktu; karar yok (deep review L3c-5 ile gövdeye girmiş); kural gövdede zaten yazılı | `09`'un Aşama 6 bloğuna "K-884 · K-1171 · İndirme ucunun aralık yanıtları" park satırı; K-1171'in etki hücresi satırı anar | düşük |
| — | `07 §2.3.2` | Oturumsuz gelen GET'in girişe yönlendirme kodu "303" (§2.7.1, UC-33 ve 4696. satır "302" diyor) | "302" (§2.7.1 zaten kurar) | düşük |
| — | `07 §1.3` (K-1200 satırı) | Satırın konusu yalnız "ödeme yöntemi kalmadığı için" diyordu; K-1200 teyit ekranından sağlayıcıya bağlanılamamasını da kapsar | Konu iki durumu söyler | düşük |
| — | `12` Aşama 6 bloğu etiketleri; K-1192'nin etki sütunu; `09` etiketi | Metinde geçen K-1199, K-1200, K-1202, K-1203, K-1213 etiketlerde yoktu; K-1192'nin etki sütunu `12`'yi saymıyordu; `09`'un yeni satırının etiketi kaynak olmayan K-1167'yi taşıyordu | Etiketler ve etki sütunu iki yönde eşlendi | düşük |
| — | `07` Kaynak satırları; geri besleme tablosu | K-1228'in (§2.6.5) ve K-1171 (5)'in (§2.2) gövde atfı Kaynak satırında yoktu | Kaynak satırları ve §1.3'e iki satır | düşük |

### 5.3 Park satırları

**(a) Aşama 1–5'in park satırları — numaraya dayalı tarama.** 197 madde; K-1144…K-1226'da kaynak K'ların **373 eşleşmesi** beş dilimde okundu (`04`/`05`/`06` — 28 madde · `07` — 14 · `08` — 6 · `09` — 14 · `12` ve `DEPLOY_RUNBOOK` — 13). Okuyucular 4 aday çıkardı; doğrulama sonucu:

| # | Park satırı | Değiştiren karar | Hüküm |
|---|---|---|---|
| P-1 | `06` Aşama 4 (K-891 · K-892 · K-893 · …) | K-1199 | **Çürütüldü (F13)** — park metni ikinci gönderimin hedefini hiç yazmıyor; hedefi yazan bütün yüzler teyit ekranı diyor; K-893'te K-1199 geri işareti var |
| P-2 | `05` Aşama 3 (K-758 · Ödeme adımında yazılanlar) | K-1161 | **Çürütüldü (F1)** — K-1161 çerezin işlevi, ömrü ve dayanağını değiştirmez; K-757 ve K-758 satırlarının sözü ("giriş"e ayrılış) doğru kalır; `04` 5.13.19 zaten ikisini birleştirir |
| P-3 | `08` Aşama 4 (K-906 · K-973 · Ödeme sağlayıcısından dönüş) | K-1185 | **Doğru (F18)** — yol kapandı, metin açık diyordu; düzeltildi (§5.2) |
| P-4 | `09` Aşama 4 (K-884) ← `07 §4.31.1` | — (karar yok) | **Doğru (F20)** — gövde cümlesinin devri karşılıksızdı; park satırı eklendi (§5.2) |

Geri kalan 369 eşleşme yalnız atıf, "değişmez" ya da uygulanış kaydıdır. Okuyuculardan ikisi park metinlerinin kesik göründüğünü bildirdi (dilim 650 karakter); kesik kalan sonlar için yalnız doğrulayıcıya giden P-1 ve P-2 tam metinle okundu.

**(b) Aşama 6'nın park satırları (iki yönlü betik).** `08` 4 · `09` 11 · `12` 7 · `DEPLOY_RUNBOOK` 4 = **26 madde** (bu taramadan önce 24; `09`'a iki madde eklendi). **A yönü** — etiketteki her kararın etki sütunu hedef dokümanı anıyor: ilk koşumda iki fark — `12`'nin etiketinde K-1192 vardı, K-1192'nin etki sütunu `12`'yi saymıyordu (düzeltildi); yeni `09` satırının etiketi kaynak olmayan K-1167'yi taşıyordu (düzeltildi). **B yönü** — etki sütunu hedefi anan her karar bir etikette geçiyor: ilk koşumda `12` için K-1199, K-1200, K-1202, K-1203, K-1213 (metinde geçiyor, etikette yok — etiketlere eklendi) ve `08`, `09`, `12` için K-1226 (kalite döngüsünün etki yansıtması; park doğurmaz, mevcut satırları günceller — süreç kararı); K-1145 ve K-1207 `11` ve `12` eşleşmesi yanlış pozitiftir (etki hücresinde "10, 11, 12, 13" madde listesi); K-1161 `12`'nin Aşama 5 bloğunda "etikete K-1161, (2). kol" olarak durur. Son koşum: A yönü boş, B yönünde yalnız bu üç açıklanmış kalem.

**(c) Aşama 6 kararlarının önceki aşamaların park satırlarını güncellemesi.** 373 eşleşmeden 3'ünde güncelleme gerekti (P-3, P-4 ve yukarıdaki iki çürütülen aday); Aşama 5'in taramasının 25 hizasızlığıyla karşılaştırılabilir değildir — o tarama K-977…K-1131'in yüzlerce kararını okuyordu, bu tarama 83 kararı.

### 5.4 Geri işaret, zincirler ve öncüller

402 çift (199 Aşama 1–5, 203 Aşama 6 içi); eski satırın etki sütununda yeni numara **olmayan** çiftler: Aşama 1–5 için 187/199 (okuma sırasında 191; dördü yazım anında işaretlendi), Aşama 6 içi 145/203. Okuyucular çiftlerden **41 aday satır** çıkardı; bunlar 18 kümede Opus / high doğrulayıcılarına verildi (F1–F18, §5.6) — **9 gerçek eksik geri işaret** ve 1 eksik hâl (K-1227) çıktı. Doğrulayıcıya verilmeyen 11 aday eski satırında geri işareti VAR olan ve kuralın değiştiği yeni satırda açıkça yazılı çiftlerdir; kayıt (eski satırda geri işaret + yeni satırda gerekçe) yeterli sayıldı:

| Çift | İlişki |
|---|---|
| K-1172 → K-1149; K-1163 → K-1161; K-1158 → K-1151; K-1161 → K-907 | genişletir; kuralın değişikliği yeni satırda açık |
| K-1179 → K-1163; K-1183 → K-1163; K-1186 → K-1163 | GET'in yan etkisizliği listesine istisna eklenir (`07 §2.2.5`) |
| K-1203 → K-1171; K-1205 → K-1173; K-1225 → K-884; K-1210 → K-1080 | netleştirir / daraltır; yeni satır "değişmez" ya da "netleştirir" der |

Not: bu 11 aday doğrulayıcıya verilmedi; yönetici isterse verilebilir.

**Zincirler.** Model ve efor kuralı (K-1145 → K-1207 → K-1224) ve "tek istisna" zinciri (K-1163 → K-1195 → K-1171 (5)) bu taramada tamamlandı; GET'in yan etkisizliği zinciri (K-1163 (5) → K-1179 → K-1183 → K-1186) ve tekrar gönderim listesi (K-1166 → K-1179 → K-1191 → K-1195) `07`'nin gövdesinde son hâlini söylüyor (§2.2.5, §2.5.1; F5 yalnız geri işaret eksiğiydi).

### 5.5 Betikle aranan ve temiz dönen çakışma sınıfları

| # | Sınıf | Sonuç |
|---|---|---|
| 1 | **⚠ listesi ↔ kayıt** | ✓ — 9 / 9 (tracker §16.1'in listesi K-1192'yi saymıyordu — düzeltildi) |
| 2 | **K numarası tekilliği** — çift K taraması | ✓ — boş; K-01…K-1228, boşluk yok |
| 3 | **Karar satırının biçimi** — altı sütun | ✓ — 1.228 satırın hepsi |
| 4 | **Park etiketi ↔ etki sütunu** (Aşama 6 blokları, iki yön) | ✓ — son koşum temiz (§5.3 b) |
| 5 | **Tracker §4** | ✓ — 18 / 18 kapalı |
| 6 | **Aşama 6 etki sütunlarında yalnız doküman numarası taşıyan devir atfı** | ✓ — 0 / 76 (§6) |
| 7 | **Değişen dosyalarda CR, kontrol karakteri ve `$[0-9]` artığı** | ✓ — sıfır |

### 5.6 Bağımsız doğrulama (Opus / high, bulgu başına bir)

| # | Aday | Hüküm | Sonuç |
|---|---|---|---|
| F1 | K-1161 ↔ K-758, K-757, K-739 ve `05` park satırı | Çürütüldü | Kapsam çakışması yok; K-1161 "değişmez" diyor ve sözü tutuyor |
| F2 | K-1194 ↔ K-663/664/665, K-1163 (4), K-1165 | Çürütüldü | Koşullu onay eski onay cümlelerini daraltmıyor; geri işaret var |
| F3 | K-1144, K-1145 ↔ K-992, K-1141 | Kısmen | Çalışma modları çakışmıyor; üç geri işaret eksikti — işlendi |
| F4 | K-1224 ↔ K-1145, K-1207 | Doğru | K-1145'te K-1224 geri işareti ve K-1224'ün kapsam sözü düzeltildi |
| F5 | tekrar gönderim listesi | Kısmen | Dokümanlar tutarlı; K-1179'da K-1191 geri işareti eklendi |
| F6 | K-1195 ↔ K-1163 "tek istisna" | Kısmen | Görsel yükleme de 200 döner; `07 §2.2.2` düzeltildi |
| F7 | K-1202 ↔ K-1165, K-1170 | Kısmen | İçerik çelişkisi yok; K-1165'te geri işaret eklendi |
| F8 | K-1213 ↔ K-823 | Kısmen | K-823 geri işareti, K-1213'ün yanlış atfı, `04` 6.3.24.2 |
| F9 | K-1185 ↔ K-562 | Kısmen | Yeni kural yok; K-562'ye K-585 geri işareti |
| F10 | K-1151, K-1160 ↔ K-1002, K-1005 | Çürütüldü | Her aşama kendi yöntemini seçer; eski kararlar Aşama 5'e özgü |
| F11 | K-1155 ↔ K-1153 | Kısmen | `07 §1.1`'in cümlesi hizalandı |
| F12 | K-1184 ↔ K-1175 (7) | Çürütüldü | UC-69 ayrı bir uç (`GET /hesap/silindi`); 187 tutarlı |
| F13 | K-1199 ↔ K-893 ve `06` park satırı | Çürütüldü | Park metni hedefi yazmıyor; geri işaret var |
| F14 | K-1200 ↔ K-1183 | Çürütüldü | Bütün yüzler aynı anlamı söylüyor (üç dar etiket: `07 §1.3` düzeltildi) |
| F15 | K-1208 ↔ K-1167, K-1164 (2) | Çürütüldü | İki ayrı `returnUrl` kullanımı; yan bulgu — `07 §2.3.2` 303/302 — düzeltildi |
| F16 | K-1218 ↔ K-1210, `04` 9.25.5 | Kısmen | Geri işaret eksik; `04`'te ekranın hâli yazılı değildi — K-1227 |
| F17 | K-1196 ↔ K-1168 (3) | Çürütüldü | Kod gövdede taşınmıyor, yalnız sınır değeri |
| F18 | `08` park satırı ↔ K-1185 | Doğru | Satır güncellendi |
| F19 | `07 §2.6.5` eşit anahtar | Doğru | K-1228 |
| F20 | `07 §4.31.1` aralık yanıtı | Doğru | `09` park satırı |

---

## 6. Devir dizini — Aşama 6 kararlarının sonraki dokümanlara devirleri

**Tek ev:** devrin evi karar satırının etki sütunu ya da park satırıdır; bu dizin devri ikinci kez kaydetmez. **Aşama 6'da yalnız numara taşıyan atıf yoktur** (betik: 0 / 76). Her hedef dokümanın "Aşama 6'dan park edilen girdiler" bloğu bu bölüme işaret eder (`08`, `09`, `12`, `DEPLOY_RUNBOOK`; bu taramada dizin satırı eklenmedi — blokların başındaki kaynak cümlesi karar kaydına gider, devir dizini bu rapordur). **Entegrasyon spesifikasyonları aşamasının açılışı** devir taramasını (checklist §4) buradan ve önceki aşamaların dizinlerinden başlatır.

- **Önceki aşamaların dizinleri:** `PHASE1_CONFLICT_SCAN.md` §6, `PHASE2_CONFLICT_SCAN.md` §6, `PHASE3_CONFLICT_SCAN.md` §6, `PHASE4_CONFLICT_SCAN.md` §6, `PHASE5_CONFLICT_SCAN.md` §6.
- **`11` Uygulama Planı:** Aşama 6'dan devir yok — `11` tüketicidir; `07`'de `11`'e iş bırakan cümle yoktur (betik: 0 satır).
- **`DEFERRED_BACKLOG.md`:** Aşama 6'dan yeni kalem yok.
- **Süreç kararları** (K-1144, K-1145, K-1146…K-1150, K-1207, K-1224, K-1226) sonraki dokümana iş bırakmaz.

### 6.1 Etki sütunlarındaki devirler (K-1144…K-1228)

| Hedef | Atıf = karar | Aşama 6 park bloğunda | Başka park satırında | Park gerekmez |
|---|---|---|---|---|
| `08` Entegrasyon Spesifikasyonu | 8 | 7 | — | 1 (K-1226) |
| `09` Kodlama Kılavuzu | 25 | 25 | — | — |
| `12` Doğrulama Protokolü | 38 | 37 | 1 (K-1161) | — |
| `DEPLOY_RUNBOOK.md` | 5 | 5 | — | — |
| `11` Uygulama Planı | 0 | — | — | — |
| **Toplam** | **76 atıf = 76 karar-doküman çifti** (44 ayrı karar) | **74** | **1** | **1** |

İşin adı etki sütunundan alınmıştır; parantez içi park satırının etiketidir. Tablo kararları azalmayan K sırasıyla verir.

#### `08` Entegrasyon Spesifikasyonu — 8 karar

| Karar | İş | Park satırı |
|---|---|---|
| K-1166 | dönüşün yönteminin belirlenmesi; K-906 · K-973 satırı değişmez — yolu API4-02, yazım | Aşama 6 bloğu |
| K-1169 | bildirimin gövde boyunun doğrulanması | Aşama 6 bloğu |
| K-1175 | Google ile girişin dönüş adresi | Aşama 6 bloğu |
| K-1185 | dönüş adresinin biçimi ve sabit adres hâli | Aşama 6 bloğu |
| K-1211 | park — sağlayıcı oturum davranışı | Aşama 6 bloğu |
| K-1216 | Tanınmayan ödeme bildiriminin kaydı | Aşama 6 bloğu |
| K-1217 | park — alıcı başına sayaç | Aşama 6 bloğu |
| K-1226 | Kalite döngüsünün etki yansıtması — atlanmış yüzler, kararlar özeti, park satırlarının iki yönü… | süreç kararı — park gerekmez |

#### `09` Kodlama Kılavuzu — 25 karar

| Karar | İş | Park satırı |
|---|---|---|
| K-1162 | yönlendirme tablosu, harf duyarsız eşleme ve anahtar bölümlerinin istisnası | Aşama 6 bloğu |
| K-1163 | işleyicilerin ve onay işaretinin kod kalıbı | Aşama 6 bloğu |
| K-1164 | alan düzeyinde varsayılan yetki politikası, çerez adları | Aşama 6 bloğu |
| K-1165 | `Original` önekli alanların ve bölüm sürümünün bağlanışı | Aşama 6 bloğu |
| K-1166 | doğrulama süzgecinin yeniden gösterimi, yükleme betiğinin başlığı | Aşama 6 bloğu |
| K-1168 | hata kodlarının kod düzeni | Aşama 6 bloğu |
| K-1169 | hız sınırlama ara katmanının ayarı | Aşama 6 bloğu |
| K-1171 | yükleme protokolünün sunucu ve betik kodu; indirme ucunun aralık yanıtının `ETag` ve `If-Range`'i K-884 · K-1171 | Aşama 6 bloğu |
| K-1172 | model bağlamanın kalıbı, varlığın bağlanmaması | Aşama 6 bloğu |
| K-1173 | işleyici adlarının biçimi | Aşama 6 bloğu |
| K-1174 | Türkçe kültürle ayrıştırma ve tarih alanının bağlanışı — kod kalıbı | Aşama 6 bloğu |
| K-1183 | Bir kez söylenen mesajın kaynağı kayıtta olmayan iki hâli — girişteki sepet bi | Aşama 6 bloğu |
| K-1184 | Hesap silmenin sonuç hâlinin adresi ve bir kez gösterimi — UC-69; K-1175 (7)'yi değiştirir (yazım tu | Aşama 6 bloğu |
| K-1186 | korumalı değerin amacı ve biçimi; izin değerinin adreste taşınması; Google dönüşünün yazımları | Aşama 6 bloğu |
| K-1189 | betiksiz formun boş girdilerinin bağlanışı, önerinin üretildiği an | Aşama 6 bloğu |
| K-1190 | davetin korumalı değerinin amacı ve biçimi | Aşama 6 bloğu |
| K-1193 | içerik formunun ara gönderiminin ve tekil kaydın yeni kolunun kalıbı | Aşama 6 bloğu |
| K-1195 | CSV'nin iç biçimi, dosyanın geçici üretimi ve izin anı | Aşama 6 bloğu |
| K-1196 | model adları, paylaşılan model, görsel yüklemenin iki alanı | Aşama 6 bloğu |
| K-1198 | `notice`'in kapalı listesi ve uygulama kaydının kodu | Aşama 6 bloğu |
| K-1205 | park — işleyici adları, K-1173 · K-1174 satırı; etki yansıtma — K-1226 | Aşama 6 bloğu |
| K-1211 | UC-29 — ikinci ödeme oturumu ve Ödendi siparişe gelen ikinci başarılı ödeme (deep review, bulgular… | Aşama 6 bloğu |
| K-1217 | Güvenlik — önbellek başlığı, misafir e-postasına e-posta yağdırma, CSV formül öneki (deep review… | Aşama 6 bloğu |
| K-1226 | Kalite döngüsünün etki yansıtması — atlanmış yüzler, kararlar özeti, park satırlarının iki yönü… | süreç kararı — park gerekmez |
| K-1228 | ORDER BY kalıbı ve testi | Aşama 6 bloğu |

#### `12` Doğrulama Protokolü — 38 karar

| Karar | İş | Park satırı |
|---|---|---|
| K-1161 | GAP-14 — ödeme adımından "Sepete dön"le ayrılışta yazılanların korunması (matrisin 2. oturumu; aday | Aşama 5 — K-904 · K-907 · K-908 · K-971 · K-1161 (güncellendi) |
| K-1163 | onay işaretsiz uygulanmama, GET'in yan etkisizliği | Aşama 6 bloğu |
| K-1164 | sahiplik reddi ile var olmayan kaydın aynı yanıtı; misafirin erişim anahtarlı yolu | Aşama 6 bloğu |
| K-1165 | çakışmanın iki varyantı | Aşama 6 bloğu |
| K-1166 | korumanın dışındaki uçların listesi; doğrulama hatasında yazılmama | Aşama 6 bloğu |
| K-1167 | geçersiz ve sınır dışı parametrenin davranışı | Aşama 6 bloğu |
| K-1168 | durum kodlarının senaryoları | Aşama 6 bloğu |
| K-1169 | 429 ve 413'ün senaryoları | Aşama 6 bloğu |
| K-1170 | aşan adedin bütün reddi; gecikme feshinde adedin yok sayılması | Aşama 6 bloğu |
| K-1171 | sürdürme, başa dönme, silinmiş yüklemeyle kayıt, aralık isteği | Aşama 6 bloğu |
| K-1172 | fazla alanın yok sayılması | Aşama 6 bloğu |
| K-1178 | Ürün sayfasında varyant seçiminin parametreleri ve sepete eklemenin varyant alanı — K-117 | Aşama 6 bloğu |
| K-1179 | Tekrar gönderimin güncel hâlle karşılaştırılmayan işlemleri, kaydı bulunmayan sepet kalem | Aşama 6 bloğu |
| K-1181 | Ödeme adımının uçlarının okunuşu — kupon ve ara gönderim, ayrılışın hedefi, açılamayan ödeme adımı | Aşama 6 bloğu |
| K-1183 | Bir kez söylenen mesajın kaynağı kayıtta olmayan iki hâli — girişteki sepet bi | Aşama 6 bloğu |
| K-1184 | Hesap silmenin sonuç hâlinin adresi ve bir kez gösterimi — UC-69; K-1175 (7)'yi değiştirir (yazım tu | Aşama 6 bloğu |
| K-1185 | Ödeme sağlayıcısından dönüşün yolu ve sipariş sayfasına oturum çerezine dayanmadan iniş — `08`'in K | Aşama 6 bloğu |
| K-1186 | Hesap uçlarının okunuşu — bağlantı ekranlarının sonuç hâli, kayıttan sonraki h | Aşama 6 bloğu |
| K-1188 | yöneticinin aşan adedinin bütün reddi; gecikme feshi kaydında adedin yok sayılması | Aşama 6 bloğu |
| K-1189 | yayın geçişinin sürümsüz denetimi, önerinin kayıtta yazılması | Aşama 6 bloğu |
| K-1190 | korumalı değer çözülmüyorsa form boş gelir; kullanılmış davet bağlantısı sonuç göstermez | Aşama 6 bloğu |
| K-1191 | yeni kayıt formunun tekrar gönderimi | Aşama 6 bloğu |
| K-1192 | imha edilen iletişim talebinin adresinin 404'ü | Aşama 6 bloğu |
| K-1193 | yan sütunun sürümsüz denetimi, tekil kaydın ikinci yeni kolunda 409, elle sıranın 409'u | Aşama 6 bloğu |
| K-1194 | anahtarın güncel hâli, koşullu onay, yayına almanın bütün reddi | Aşama 6 bloğu |
| K-1195 | 200 ve başlıklar, boş kümede 422 ve izin yazılmaması, tekrar gönderimde ikinci iz | Aşama 6 bloğu |
| K-1196 | tuzak alanın yanıtı, parçalı yüklemenin 422 gövdesi | Aşama 6 bloğu |
| K-1198 | kodlar senaryoların birimi, `02 §6`'nın kapsama tablosu, başarı yolu satırları, §6.4.3'ün kalan riskleri; audit,… | Aşama 6 bloğu |
| K-1199 | onay anahtarının ikinci gönderimi | Aşama 6 bloğu |
| K-1200 | park — sepete dönüşün mesajı; K-1226 | Aşama 6 bloğu |
| K-1202 | aşan adedin reddi | Aşama 6 bloğu |
| K-1203 | parçalı yüklemenin eşzamanlı parçası | Aşama 6 bloğu |
| K-1211 | UC-29 — ikinci ödeme oturumu ve Ödendi siparişe gelen ikinci başarılı ödeme (deep review, bulgular… | Aşama 6 bloğu |
| K-1212 | park — gösterilen değerle karşılaştırma ve sepet işareti | Aşama 6 bloğu |
| K-1213 | park — dışa aktarmanın kapsamı | Aşama 6 bloğu |
| K-1216 | Tanınmayan ödeme bildiriminin kaydı | Aşama 6 bloğu |
| K-1217 | Güvenlik — önbellek başlığı, misafir e-postasına e-posta yağdırma, CSV formül öneki (deep review… | Aşama 6 bloğu |
| K-1226 | Kalite döngüsünün etki yansıtması — atlanmış yüzler, kararlar özeti, park satırlarının iki yönü… | süreç kararı — park gerekmez |

#### `DEPLOY_RUNBOOK.md` — 5 karar

| Karar | İş | Park satırı |
|---|---|---|
| K-1162 | ters vekilin kayıt maskesinin yolları | Aşama 6 bloğu |
| K-1169 | ters vekilin uç başına gövde tavanı | Aşama 6 bloğu |
| K-1175 | ters vekilin tavanı "Sınır" sütunundan, Google'a kayıtlı dönüş adresi | Aşama 6 bloğu |
| K-1185 | kayıt maskesine `/odeme/donus/` | Aşama 6 bloğu |
| K-1211 | UC-29 — ikinci ödeme oturumu ve Ödendi siparişe gelen ikinci başarılı ödeme (deep review, bulgular… | Aşama 6 bloğu |

### 6.2 API Tasarımı'nın gövdesinde sonraki dokümanı anan cümleler

Mekanik çıkarım (`07 §2`–§10; başlık notu, matris ve Kaynak satırları hariç): sonraki dokümanı numarasıyla ya da adıyla anan **56 satır** — `08` 35, `09` 11, `12` 5, `DEPLOY_RUNBOOK` 5, `11` 0. Betik her satırın K numaralarından biri hedefin devir listesinde (§6.1) ya da satırda "park" sözü geçiyor mu denetledi (25 satır geçiyor); geçmeyen 31 satır bir okuyucuya ve gerekirse bir doğrulayıcıya verildi.

| Hedef | Satır | Betik (K ya da "park") | Okunarak: başka park satırı | Karar satırı olmadan devredilen |
|---|---|---|---|---|
| `08` | 35 | 13 | 22 — hepsi `08`'in Aşama 4–6 satırlarında karşılık buldu (K-892 · … · K-978 satırı "yeniden gönderip göndermediği"ni anar) | — |
| `09` | 11 | 4 | 5 — `09`'un Aşama 4–6 satırları; "kod kalıbı `09`'undur" cümleleri K-1162 · K-1163 · K-1164 satırında | **iki — bu taramada park satırı aldı:** eşit anahtarda sıra (`07 §2.6.5` → K-1228 ve yeni `09` satırı) ve indirme ucunun aralık yanıtı (`07 §4.31.1` → K-884 · K-1171 satırı) |
| `12` | 5 | 4 | 1 | — |
| `DEPLOY_RUNBOOK` | 5 | 4 | 1 | — |

**Sonuç:** karar satırı olmadan devredilen iş kalmadı. İki iş okuma ve Opus / high doğrulamasıyla bulundu (F19, F20); ikisi de deep review'un gövdeye eklediği cümleydi (aday 7).

### 6.3 Doğrulama ve soru devirleri

`07`, dış dünyaya ait olguları Entegrasyon Spesifikasyonu'na bırakır (`08`'in park satırları: sağlayıcının 500'den sonra bildirimi yeniden gönderip göndermediği, dönüşün yöntemi, Google ile girişin dönüş adresi); bunlar devir değil doğrulamadır ve evleri park satırlarıdır. **Çok kritik soru adayı yoktur.** Teknik Mimari'nin bıraktığı aday — desteklenen ödeme sağlayıcısının sayısı — Entegrasyon spesifikasyonları aşamasının açılışındadır (`08`'in K-932 satırı).

---

## 7. Öğrenim terfisi adayları — 5. adımın girdisi

Metinler tracker §16.1'dedir (1–6) ve bu taramanın gözlemleri (7–9); bu dizin onları tek yerde gösterir.

| # | Aday | Kaynak | Durum |
|---|---|---|---|
| 1 | Bir dokümana bırakılan karar başka bir dokümanın park satırında durabilir | Açılış | Bekliyor |
| 2 | API Tasarımı şablonu ayrı bir JSON API'yi varsayıyor | Şablon taraması | Bekliyor |
| 3 | Açılışın sayımı gövde cümlelerini yine dizinden aldı | Matrisin 1a oturumu | Bekliyor |
| 4 | Konvansiyon tablosu karar satırının genel cümlesini kopyalıyor; istisnalar sonraki kararlarla geliyor | Özet ↔ ayrıntı taraması | Bekliyor |
| 5 | Bu projede alt ajanlar yalnız Sonnet / Haiku ve `low` eforla koşar (K-1207); K-1224 istisnası; **bu taramadan:** `INSTRUCTIONS §9`'un koşum paragrafı (K-1141) Aşama 6'da uygulanmıyor ve paragrafta deneme notu yok | Audit sonrası · F3 | Bekliyor — proje sahibine sorulur |
| 6 | Genel alt ajan kuralının hook'u ile proje kuralı çatıştığında dolanma yerine yöneticiye bildirim | Cross-review 5. tur | Bekliyor |
| **7** | **Deep review'un düzeltmesi gövdeye karar satırı olmadan kural ve devir yazabilir.** `07 §2.6.5`'in eşit anahtar cümlesi ve `07 §4.31.1`'in "`09` devri" cümlesi deep review'un commit'iyle girdi; ikisinde de K numarası, kaynak satırı ve park satırı yoktu; etki yansıtmanın betikleri K numarasına dayandığı için görmedi — bu tarama gövdede sonraki dokümanı anan cümleleri K'dan bağımsız tarayarak buldu. **Aday hedef:** `checklists/document-stage.md` §6 (etki yansıtma) — gövdede "…işidir", "…devri", "`NN` işidir" diyen her cümle için K ve hedef park satırı betikle aranır | **Bu tarama** (§6.2) | Bekliyor |
| **8** | **Geri işaret taraması eksik geri işaretten çok "kuralın değişmediği" çiftleri okutuyor.** 402 çiftten 332'si eski satırın etki sütununda yeni numarayı taşımıyordu; okuyucular 41 aday satır çıkardı, 18 kümede doğrulandı: 9 gerçek eksik geri işaret ve 1 eksik hâl çıktı, 8 küme çürütüldü. **Aday hedef:** tek seferlik gözlem — raporda kalır; terfi gerekmez | **Bu tarama** (§5.4, §5.6) | Bekliyor |
| **9** | **Park etiketi ↔ etki sütunu betiği etiketi okur, metni okumaz.** `12`'nin beş kararı metinde geçiyor, etikette yoktu; betik B yönünü bu yüzden işaretledi — bir kez okunarak yanlış pozitif ayıklandı ("10, 11, 12, 13" madde listesi `11` ve `12` diye okunuyordu). **Aday hedef:** iki yönlü betik etki sütununda doküman numarasını `(?:^| · )NN ` kalıbıyla arar ve madde listelerini dışlar; etiket ve metin ayrı sayılır | **Bu tarama** (§5.3 b) | Bekliyor |
| Ö-23 | Checklist'in skill'e dönüştürülmesi | Aşama 1'den | Bekliyor — kapı Aşama 6'nın öğrenim terfisi |

---

## 8. Checkpoint'e notlar

- Tracker §4'te açık satır yok, A-19 açılmadı; dokümanlarda açık kalem yok — checkpoint'in açık karar kontrolü bu raporla karşılanır.
- ⚠ listesi **9 karar** (§4.1) — hazır metin aynı yerde; arşiv işaretinden önce proje sahibine tek mesajda gösterilir (yönetici).
- K-1227 ve K-1228 bu taramanın kararlarıdır; yüzleri §5.1'dedir. Checkpoint'in iç tutarlılık merceği `04` 9.25.5 ile `07` UC-76 / UC-187'nin, `06` §4.5 / §7.5.1 ile `07 §2.6.5`'in aynı hâlde olduğunu bir kez daha okuyabilir.
- **Devredilen sınıf** (K-1129): iki yerin metin farkı — özet hücresi ya da genel tanım cümlesi ile ayrıntının kuralı. Bu taramanın bulguları aynı sınıftadır: `07 §2.2.2`'nin "tek istisna" cümlesi, `07 §1.1`'in konvansiyon tanımı, `07 §1.3`'ün K-1200 satırının konusu ve `08`'in park satırı özet-ayrıntı farkıydı.
- Aşama 1–5'in park satırları K-1144…K-1226'ya bir kez tarandı (373 eşleşme; 197 madde). Checkpoint yeniden taramaz; yalnız bu PR'ın değiştirdiği satırları okur: `08` (Aşama 4 K-906 · K-973 satırı), `09` (iki yeni madde), `12` (dört etiket), `04` (9.25.5, 6.3.25.4, 6.3.24.2), `06` (§4.5, §4.47, §7.5.1), `07` (§1.1, §1.3, §2.2.2, §2.3.2, §2.6.5, iki matris hücresi).
- 6. adımda Entegrasyon spesifikasyonları aşamasına devrin tablosu bu raporun §6'sını ve hedef dokümanların park bloklarını gösterir; öğrenim adayları dizini (§7) 5. adımın girdisidir.

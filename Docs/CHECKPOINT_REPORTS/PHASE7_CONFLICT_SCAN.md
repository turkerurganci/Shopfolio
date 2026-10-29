# Aşama 7 — Çakışma Taraması

**Tarih:** 2026-10-29 | **Aşama:** 7 — Entegrasyon spesifikasyonları | **Kapanış adımı:** K-437'nin 3. adımı (K-429; `checklists/document-stage.md` §7)
**Girdi sürümleri:** karar kaydı v0.189 · Entegrasyon Spesifikasyonları (`08`) v0.15 · Ürün Gereksinimleri (`02`) v0.100 · Kullanıcı Akışları (`03`) v0.43 · Arayüz Tanımları (`04`) v0.46 · Teknik Mimari (`05`) v0.35 · Veri Modeli (`06`) v0.37 · API Tasarımı (`07`) v0.31 · MVP Kapsamı (`10`) v0.56 · Proje Vizyonu (`01`) v0.33 · `09`, `12` ve `DEPLOY_RUNBOOK`'un park blokları
**Çıktı sürümleri:** karar kaydı v0.190 · `08` v0.16 · `10` v0.57 · `01`, `02`, `03`, `04`, `05`, `06`, `07` değişmedi · `09`, `12` ve `DEPLOY_RUNBOOK`'un park blokları (sürüm alanı değişmez)

> **Ne işe yarar:** Aşama kapanmadan önce açık kalemlerin her birinin **tek bir evi** ve **gözlemlenebilir bir kapısı** olduğunu doğrular; vadesi geçmiş açıkları adıyla listeler (K-429). Önceki aşamaların park satırlarını ve bu aşamanın park satırlarını, kaynak kararlarını sonradan değiştiren kararlara karşı okur. Aşama 7 kararlarının (K-1233…K-1292) önceki aşamaların kararlarına geri işaretlerini, birbirleriyle zincirlerini denetler. Gösterilmemiş ⚠ kararlarının hazır metnini verir (§4.1). Aşama 7 kararlarının sonraki dokümanlara devirlerini tek dizinde toplar (§6). **Yönetici kararıyla bu taramanın ek işi:** ikinci ödeme sağlayıcısı devri geri besleme olarak yapıldı (§5.1). Sonuç checkpoint'in (4. adım) girdisidir.

---

## 1. Sonuç

| Kural | Sonuç |
|---|---|
| **(1) Aynı açık iki yerde durmaz** | ✓ — açık karar yok. Tracker §4'ün on sekiz satırının hepsi kapalı; Aşama 7'de A-19 açılmadı. Entegrasyon Spesifikasyonu'nun şablonunda açık kararlar bölümü yoktur (PF-117) ve gövdesinde açık işaretli madde yok; açık kalem tracker'dadır (§3). |
| **(2) Her açık satır gözlemlenebilir bir kapı taşır** (K-37) | ✓ — tracker §18.1'in 18 maddesinden 13'ü kapalı, **5'i açık ve beşinin de kapısı yazılı** (§4). İkinci ödeme sağlayıcısı devrinin kapısı bu adımdı: geri besleme olarak yapıldı (K-1293). |
| **(3) Vadesi geçmiş ve açık satır adıyla listelenir** | ⚠ — **bir madde:** "Girdilerin tarayarak okunan ve okunmayan kısımları" (kapı: kalite döngüsünün audit'i) kapısı geçtiği hâlde açık kalmıştı; okumalar kayıtlıydı, madde kapatıldı (§3.3). Başka vadesi geçmiş açık yok. |

**Çakışmalar:** park satırları, geri işaretler ve zincirler betikle çıkarıldı; bulgu arayan okuma **17 Sonnet / low** dilim okuyucusunda yapıldı (7 park, 8 geri işaret, 1 açık kalem deseni, 1 gövde) ve 39 aday çıkardı; aynı yeri anan adaylar ve üç betik bulgusu ile üç doğrulayıcı yan gözlemi 44 ayrı bulguda toplandı. Her bulgu bir **Opus / high** doğrulayıcısı tarafından güncel metne karşı okundu: **16 doğru, 20 kısmen, 8 çürütüldü**. Doğrulanan ve kısmen doğrulanan 36 bulgunun hepsi bu PR'da düzeltildi — **35'i yeni karar istemedi** (eksik geri işaret, eksik park maddesi ya da etiket), **biri karar istedi** (§5.1): **K-1294** — sağlayıcı kıyasının (b) ölçütü: `08 §2.1.1`'in "ikisini ayırmaz" yargısı düzeltilir, seçim değişmez, kalan risk adıyla yazılır. İkinci ödeme sağlayıcısı devri (yönetici kararı) **K-1293** ile geri besleme olarak yapıldı: `10 §3`'e KD-64, `10 §5`'e YH-23. Her ikisi `(öneriyle kaydedildi)` taşır; K-1294 ayrıca ⚠'dir.

---

## 2. Yöntem

Tarama gözle değil betikle yapıldı (K-429); anlamsal okuma ayrı alt ajanlarda ve ön planda koştu. Ortam: Git Bash ve Python 3.12 (`python -X utf8`). **Model dağılımı (K-1207, K-1224):** bulgu arayan dilim okuyucuları ve ⚠ metninin taslağı Sonnet / low (17 + 1); bulgu doğrulayıcıları Opus / high (44, bulgu başına bir); tarama, sayım ve karşılaştırma betikleri ve düzeltmelerin işlenmesi yönetici adım ajanında. Her okuyucuya yalnız kendi dilimi ayrı bir dosyada verildi; karar kaydı hiçbir ajana bütünüyle okutulmadı. Hükümler geldiği anda scratchpad'de bir klasöre yazıldı.

1. **Tracker §4 ve §18.1:** `## 4.` ile `## 5.` arasındaki `| A-NN |` satırları ayrıştırıldı (18 satır, durum hücresi "Kapandı"ya karşı), A-19 ve sonrası dosyada arandı. §18.1'in `- [ ]` ve `- [x]` maddeleri çıkarıldı (18; 13 kapalı, 5 açık) ve her açık maddenin gövdesinde "Kapı:" ya da kapıyı adlandıran cümle arandı. Dokümanların açık kalem desenleri (`açık karar`, `açık kalem`, `belirlenecek`, `netleşecek`, `karara bağlanacak`, `TBD`, `(downstream)`, `sonra doldurulacak`, `A-19`, `A-2n`, `açık soru`, `bekliyor`, `kapı:`) `01`, `02`, `03`, `04`, `05`, `06`, `07`, `08` ve `10`'da arandı (eşleşmeler 118 bağlamlı kayıt; siparişin açık kalemi terimi betikle elendi) ve tek dilimde okundu.
2. **⚠ listesi:** K-1233…K-1292'nin konu hücresinde `⚠` taşıyan satırlar betikle çıkarıldı (29) ve §18.1'in listesindeki `K-n` girişleriyle karşılaştırıldı.
3. **Park satırları — Aşama 1–6:** `04`, `05`, `06`, `07`, `08`, `09`, `12` ve `DEPLOY_RUNBOOK`'un Aşama 1–6 bloklarındaki `> - **…**` maddeleri ayrıştırıldı (224 madde); etiketlerinden kaynak K'lar çıkarıldı (aralıklar açıldı). Her kaynak için **K-1227…K-1292**'de — K-1226'ya kadarını önceki taramalar okudu — tam eşleşme arandı (ilişki sözcüğüne bakılmadı): 224 maddenin 90'ında **255 eşleşme**, yedi dilime bölünüp okundu (`08` ikiye bölünmüş · `09` · `05`/`04` · `12` · `DEPLOY_RUNBOOK` · `07`/`06`).
4. **Park satırları — Aşama 7:** `09` (3 madde), `12` (1) ve `DEPLOY_RUNBOOK`'un (3) Aşama 7 bloklarının maddeleri etikete ve metne göre ayrıştırıldı (7 madde); etiketteki her kararın etki sütunu hedefi anıyor mu, etki sütunu hedefi anan her karar bir etikette ya da metinde geçiyor mu denetlendi (iki yön; etki sütunundaki arka tırnaklar önce atıldı).
5. **Geri işaret ve zincirler:** K-1233…K-1292'nin konu, karar ve gerekçe hücrelerinde anılan eski K'lar çıkarıldı (349 çift: 222 Aşama 1–6, 127 Aşama 7 içi); her çift için eski satırın etki sütununda yeni numara (ya da aralığı) aranıp **geri işaret var / yok** işaretlendi. 346 çift (üçünde geri işaret zaten vardı) sekiz dilimde okundu: eski karar ile yeni karar arasında değişiklik / daraltma / genişletme / tersine çevirme / çelişki var mı.
6. **Devir dizini:** K-1233…K-1292'nin etki sütunu ` · ` ile parçalandı; `09`, `11`, `12` ya da `DEPLOY_RUNBOOK`'u anan parçalar alındı ve kararın hedef dokümanın Aşama 7 bloğundaki yeri bulundu. `08`'in §1–§5 gövdesinde (Kaynak satırları hariç) sonraki dokümanı numarasıyla ya da adıyla anan **59 cümle** çıkarıldı (`DEPLOY_RUNBOOK` 33, `12` 19, `09` 5, `11` 1, runbook anan 9; kesişimli sayım) ve tek okuyucuya hedeflerin park blokları ile birlikte verildi.
7. **Doğrulama:** her aday bulgu için bir Opus / high ajanı güncel metne karşı okudu (`[dogrulama]`; bulgu başına bir, ikinci doğrulayıcı yok); hüküm DOĞRU / KISMEN / ÇÜRÜTÜLDÜ ve gereken düzeltme dosyalara yazıldı. Doğrulanan düzeltmeler yönetici ajanında işlendi; her düzeltme bir betikle yapıldı ve uygulanan metin eski metne karşı `assert` edildi. Doğrulayıcıların kendi yan gözlemleri (üç) ayrı bulgu olarak yeniden doğrulandı (S1…S3); doğrulanmamış bulgu uygulanmadı.

---

## 3. Açık kalemler

### 3.1 Tracker §4

| Kapsam | Satır | Açık | Sonuç |
|---|---|---|---|
| A-01…A-18 (Aşama 1, salt okunur) | 18 | 0 | Aşama 1'in taraması §3.1 |
| A-19 ve sonrası | 0 | 0 | Aşama 7'de açık detay doğmadı: workshop'un, yazımın ve kalite döngüsünün bulduğu her boşluk bir K satırıyla kapandı (K-1233…K-1292); bu tarama K-1293 ve K-1294 ile kapattı |

### 3.2 Dokümanlar

| Kaynak | Bulunan | Sonuç |
|---|---|---|
| Entegrasyon Spesifikasyonu (`08`) | Şablonda açık kararlar bölümü yok (PF-117; yazım konvansiyonu 10 açık kalemi tracker'a bağlar). Desen eşleşmeleri: "açık karar" 1 (konvansiyon cümlesi), "açık kalem" 3 (siparişin açık kalemi terimi), "karara bağlanacak" 6 (park bloklarının kalıp cümlesi), "sonra doldurulacak" 1 (yazım konvansiyonu 9'un yasak cümlesi), "bekliyor" 2 (DV tablosunun "Bekliyor — ilk bağlanan task" durumu: kapısı yazılı), "kapı:" 6 (kapıların kendisi), "A-19" 1 (kural cümlesi) | Açık kalem yok |
| Ürün Gereksinimleri (`02`), Kullanıcı Akışları (`03`), Arayüz Tanımları (`04`), Teknik Mimari (`05`), Veri Modeli (`06`), API Tasarımı (`07`), MVP Kapsamı (`10`) | "açık karar" eşleşmeleri kural cümleleri ve eski başlık notları; "açık kalem" eşleşmeleri siparişin açık kalemi terimi; "karara bağlanacak" park bloklarının kalıp cümlesi; "(downstream)" konvansiyon cümlesi; "A-19" "A-19'dan sürer" kuralı; "bekliyor" eşleşmeleri `06` ve `10`'da kapısı yazılı işler | Açık kalem yok |
| Proje Vizyonu (`01`) | Eşleşme yok | — |

Okuyucu 118 bağlamlı eşleşmeyi tek dilimde okudu: **0 aday**.

### 3.3 Vadesi geçmiş açık madde

**Bir madde:** "Girdilerin tarayarak okunan ve okunmayan kısımları — kapı: W konularının listesi" (§18.1). Kapısı (kalite döngüsünün audit'i) geçmişti, madde `[ ]` kalmıştı. Kayıtlı okumalar: `02 §9`, `04 §2.1.3`, §3.5 ve §8.2 audit'in K2 merceğinde okundu (`AUDIT_REPORTS/08_AUDIT.md`, mercek tablosu); `03 §1.7.3.2` ve §2.6.3 etki yansıtmanın K-1256 ve K-1291 satırlarında okunup işlendi. Madde bu adımda kapatıldı. Başka vadesi geçmiş açık yok; kapıların beşi henüz gelmedi (§4).

### 3.4 Raporlarda kalmış açık not

Yok. Kalite döngüsünün devredilen notları etki yansıtmada kapandı (K-1291, K-1292); cross-review 4. turda TEMİZ döndü. Audit, deep review ve cross-review raporlarında açık kalem yok.

---

## 4. Süreç maddeleri ve kapıları

Tracker §18.1'in işaretsiz (`- [ ]`) maddeleri beştir (bu taramadan önce yediydi). Devir §17'nin girdileri, `08`'in gövdesi ve hafızanın Güncel Durum bloğu ayrıca tarandı.

| Madde | Ev | Kapı | Sonuç |
|---|---|---|---|
| Aşama kapanışının adımları (K-437) | Tracker §18.1 | Dört adım sırada: checkpoint, öğrenim terfisi, arşiv işareti ve devir | Kapı var; 3. adımın durumu maddeye yazıldı |
| Bekletilen öğrenim Ö-23 — checklist'in skill'e dönüştürülmesi | Tracker §18.1 · PF-32 · devir §17'nin 7. satırı | Aşama 7'nin öğrenim terfisi (5. adım) | Kapı var; proje sahibine yeniden sorulur (Aşama 6'da üç kez dönüşmemesine karar verildi — K-1232) |
| ⚠ öneriyle kayıt listesi (K-1233) | Tracker §18.1 | Aşama 7'nin arşiv işaretinden (6. adım) önce proje sahibine tek mesajda (hazır metin §4.1) | **Kapı somutlaştı; liste eksikti:** konu hücresi ⚠ taşıyan 30 karardan 19'unu sayıyordu, 11 eklendi (30 / 30) |
| Öğrenim terfisi adayları | Tracker §18.1 | Öğrenim terfisi (5. adım) | Kapı var; 1–4 tracker'da, bu taramadan 5–6 eklendi (§7) |
| Ertelenmiş iş D-01 | `DEFERRED_BACKLOG.md` | İlk implementation task'ından önce | Kapı var; Aşama 7'den yeni kalem yok |

Bu taramanın kapattığı maddeler (artık `[x]`): **İkinci ödeme sağlayıcısı — yol haritası adayı** (kapı: bu adım; K-1293) ve **Girdilerin tarayarak okunan ve okunmayan kısımları** (§3.3).

Tracker'da yer alan, §18.1 dışı iki dış kapı (`PLAYBOOK_FEEDBACK.md` — proje tamamlandıktan sonra proje sahibi; `DEFERRED_BACKLOG.md` D-01) değişmedi.

### 4.1 ⚠ öneriyle kayıt listesi — doğrulama ve proje sahibine gösterilecek metin

**Betikle doğrulama.** Konu hücresi `(öneriyle kaydedildi — ⚠)` taşıyan Aşama 7 satırı **30** (K-1238, K-1250, K-1251, K-1254, K-1255, K-1256, K-1259…K-1272, K-1275, K-1278, K-1279, K-1281, K-1282, K-1283, K-1284, K-1287, K-1291, K-1294); tracker §18.1'in listesindeki giriş bu taramadan önce **19** idi. **Fark 11 karar** — K-1267…K-1272 (deep review), K-1283, K-1284, K-1287 (cross-review 2. ve 3. tur), K-1291 (etki yansıtma) ve bu taramanın K-1294'ü — listeye alındı: **30 / 30, eksik 0, fazla 0.** K-1233 ve K-1234 liste metninde ⚠ olmadıkları için anılır (süreç ve yerleşim kararları). Aşama 7'nin 62 satırının işaret dağılımı: ⚠ 30 · `(öneriyle kaydedildi)` 18 (K-1293 dahil) · işaretsiz 14 (proje sahibinin ve süreç kararları). `(toplu onayla kaydedildi)` yok.

**Kapı.** Liste arşiv işaretinden (6. adım) önce proje sahibine **tek mesajda**, karar başına bir sade cümleyle ve gerekçesiz gösterilir (`INSTRUCTIONS.md` §2, "Ortak kural"); yöneticiden proje sahibine gider (§9). İtiraz gelmeyen satırın işareti `(öneriyle kaydedildi — ⚠ — gözden geçirildi YYYY-AA-GG, itiraz yok)` olur; itiraz gelen karar yeni bir satırla değişir. Checkpoint ya da öğrenim terfisi yeni bir ⚠ karar kaydederse liste onunla birlikte gösterilir. Bu adımda işaret dönüştürülmedi.

**Hazır metin — proje sahibine gösterilecek liste** (konulara göre; K numarası cümleyi taşımaz, parantez içi yalnız kayıt izidir):

*Ödeme ve sipariş*
- Kart iadesinin sonucu ödeme sağlayıcısından belli aralıklarla sorulur ve sonuç gelmeden iade ikinci kez gönderilmez; müşteriye aynı para iki kez gitmez. (K-1238)
- Kartla ödemede ödeme sağlayıcısı olarak PayTR seçildi, yedek olarak Sipay duruyor. (K-1254)
- Sağlayıcının ödeme kıyasında, geç gelen iade sonucunun görülmesi ölçütü düzeltildi: PayTR'de firma iadeyi "gerçekleşmedi" diye işaretledikten sonra o iadenin geç gerçekleşmesi ürüne ulaşmaz; seçim değişmedi, bu bilinen bir risk olarak kaldı. (K-1294)
- Müşteri ödemeyi sitenin kendi ödeme sayfasındaki PayTR formunda yapar ve ödeme yalnız gerçekten kartla başarılı olduysa sipariş "ödendi" olur. (K-1255)
- Ödeme bitince müşteri, sayfa iç içe açılmadan düzgünce sipariş sayfasına yönlendirilir. (K-1259)
- Ödeme sağlayıcısı ikinci bir ödeme oturumunu reddederse sipariş iptal edilmez, müşteri sipariş onay ekranında kalır; gerçek bağlantı hatasında sipariş iptal edilir ve müşteri sepetine döner. (K-1260)
- Teslimat adresi olmayan (dijital ya da hizmet) siparişlerde ödeme sağlayıcısına firmanın telefonu gönderilir, müşteriden yeni bilgi istenmez. (K-1261)
- Ödeme sağlayıcısına giden telefon, ad, adres ve e-posta bilgileri sağlayıcının kabul ettiği biçim ve uzunluğa getirilir; sığmayan bilgi kısaltılarak gider ama sipariş kaydı değişmez. (K-1269)
- Ödeme sağlayıcısına ulaşılamazsa kart seçeneği kısa süre "şu an kullanılamıyor" görünür, uzun süren bozuklukta kurulumu yapana uyarı gider ve sipariş kendiliğinden iptal edilmez. (K-1270)
- Ödeme sağlayıcısının bildirim imzası güvenli biçimde kontrol edilir ve bazı garanti cümleleri gerçek sınırlarına göre daraltıldı. (K-1267)
- Ödeme sağlayıcısıyla ilgili bazı bilgiler (test kartı, taksit farkı, yazılı teyit şartı) sağlayıcının resmî belgesine göre düzeltildi. (K-1268)
- Taksitli ödemede taksit farkı iade edilmezse firma yasal yükümlülüğe aykırı kalabilir; bu konu açık bırakıldı, hukukçu teyidi ve test sonucuna bağlı. (K-1275)
- Aynı sipariş numarasıyla ikinci bir ödeme alınabiliyorsa, bunu yakalayıp iade edecek düzen kurulana kadar kartla ödeme açılmaz ve satış havaleyle sürer. (K-1284)
- Anahtar yenilemesinden sonra sonucu öğrenilemeyen kartlı siparişte, kurulumu yapan kişi PayTR panelindeki kayda bakıp sonucu elle işler. (K-1283)
- Firma kartla ödemeyi bırakıp anahtarları kaldıracaksa, geri ödemesi doğabilecek kartlı sipariş kalmamış olmalıdır. (K-1287)
- Bu kartlı sipariş sayısı yalnız kurulumu yapanın göreceği durum bilgisinde yer alır, yönetici paneline yeni sayaç eklenmez; kaldırma sonrası PayTR panelinden elle yapılan iade sistemde "gerçekleşti" görünmez ve bu bilinen bir risktir. (K-1291)

*E-posta*
- Sipariş e-postasında iki yasal metin (Ön Bilgilendirme Formu ve Mesafeli Satış Sözleşmesi) en az 12 punto, kısaltılmadan ve açılır kutu olmadan tam yazılır; firma iptalinde yasal iade bildirimi de eklenir. (K-1250)
- IBAN isteyen e-posta IBAN taşımaz ve IBAN istemez; müşteriyi yalnız sipariş sayfasına yönlendirir ve IBAN'ı e-postayla göndermemesini söyler. (K-1251)
- Üye müşterinin sipariş e-postasındaki bağlantı giriş yapılmadan açılmaz; misafir müşteri için bağlantı özel erişim anahtarlıdır. (K-1256)
- "Bu e-posta yeniden gönderilmiştir" cümlesi yalnızca gerçekten aynı e-postanın tekrarında çıkar, yeni bir meşru olayda yanlışlıkla çıkmaz. (K-1262)
- E-posta şablonlarında renk, sebep yazıları, tarih biçimi ve süre ifadeleri yasal metinlerle ve ürünün diğer yerleriyle uyumlu hale getirildi. (K-1263)
- E-posta servisinden geri dönen adres bilgisi düzenli aralıklarla kontrol edilir ve tanınmayan bildirimler uygulama kaydına yazılır. (K-1264)
- E-posta gönderimi için yeniden deneme zamanları, gönderilemeyen adresler, gönderen adı güvenliği ve alan adı koruması (DMARC) netleştirildi. (K-1271)
- Bir müşteriye günde en fazla 20 e-posta gider; bu sınır ilk sipariş e-postasını da sayar ve e-posta ulaşmadıysa siparişin alındığını yeniden bildirmek firmanın yükümlülüğüdür. (K-1278)
- Hesabı Google ile bağlama onayı için müşterinin e-postasına tek düğmeli bir onay bağlantısı gider, şifre sorulmaz ve bağlantı 60 dakika geçerlidir. (K-1282)
- E-posta servisinin yurt dışına veri aktarımında gerekli resmî sözleşme ve bildirim firmanın sorumluluğundadır; PayTR'nin lisanslı bir ödeme kuruluşu olduğu doğrulandı. (K-1266)

*Hesap ve Google girişi*
- Google bir ad göndermezse müşterinin hesap adı e-posta adresinin @ işaretinden önceki kısmı olur ve müşteri adı sonradan değiştirebilir. (K-1265)
- Google ile girişin kimlik kontrolleri, oturum süresi ayarı ve kalan riskler netleştirildi. (K-1272)
- Google adresin sahibi sayılmayan bir hesapla girilirse, var olan hesaba otomatik bağlanmaz ve hesabın e-postasına onay bağlantısı gider. (K-1279)
- Google adresin sahibi olarak tanındığında (Gmail ya da kurumsal hesap) bağlama doğrudan yapılır, değilse e-postayla onay istenir; bir kez bağlandıktan sonra adresin el değiştirmesi bilinen bir risk olarak kalır. (K-1281)

Sade cümlelerin ilk taslağı Sonnet / low ajanında yazıldı (29 karar); K-1294'ün cümlesi yönetici ajanında eklendi. K-1279 ile K-1281 aynı konunun iki aşamasıdır: K-1279 kuralı koydu, K-1281 ve K-1282 (yönetici kararı, proje sahibine gösterilmiş sayılmaz) teyit biçimine çevirdi.

---

## 5. Çakışmalar

### 5.1 Karar isteyen çakışmalar — K-1293, K-1294

| Yer | Çakışma | Karar |
|---|---|---|
| Tracker §18.1 "İkinci ödeme sağlayıcısı" ↔ `10 §3`, `10 §5` (devir; K-1235 (5)) | K-1235 ikinci sağlayıcının yol haritası adayı olduğunu söyledi ama `10 §5` listeyi yalnız `10 §3`'ün "Evet" satırlarından besler (K-420); satır yazılmadığı için aday listede yoktu ve tetikleyici açık bırakılmıştı. Kapı: bu tarama (ya geri besleme ya da gerekçesiyle eleme) | **K-1293 (öneriyle kaydedildi):** yönetici kararıyla geri besleme olarak yapıldı — `10 §3`'e KD-64 ("Evet"; sağlayıcı adı `10`'a yazılmaz, tek evi `08 §2.1`), `10 §5`'e YH-23 (dış ön koşul: yeni sağlayıcının sözleşmesi — K-424 kalıbı; tetikleyici: ilk kurulumun sağlayıcısı dışında bir sözleşmeyle kartla satış talebi; üç sıralama ölçütünden hiçbirini taşımaz), bağımlılık zincirinin dış ön koşul cümlesi, kapsam sayıları (§3 63 → 64, "Evet" 22 → 23, §5 22 → 23), `10` v0.57; `08 §1`'in envanter sınırı ve `08 §5.2` satır 13 hizalandı. Elenen: adayı gerekçesiyle elemek (K-1235'in cevabının yarısını geri almak olurdu) · yalnız `10 §5`'e yazmak (K-420) · tetikleyiciyi açık bırakmak |
| `08 §2.1.1` Sonuç (4)(b) ↔ `08 §2.1.9`, K-978, K-1237 (2)(b), K-1238 (6) (bulgu F12) | Sağlayıcı kıyasının (b) ölçütü PayTR'de "geç sonuç sorguyla görülür, ölçüt ikisini ayırmaz" diyordu; aynı dokümanın §2.1.9'u işaretten sonra sorgunun durduğunu ve PayTR'nin (b)'yi karşılamadığını söylüyordu. Audit, deep review ve beş cross-review turu görmedi | **K-1294 (öneriyle kaydedildi — ⚠):** (b) iki adayı **kısmen** ayırır — Sipay lehine; sağlayıcı seçimi değişmez (ölçütler sırayla okunur, (a) PayTR'yi ayırır; (c) ve belgelenmiş yetenek bedeli de PayTR yönündedir); `08 §2.1.1`'in tablosundaki (b) satırı ve Sonuç (4)(b) düzeltildi; çift ödemenin yalnız PayTR panelinde görünmesi kalan risk olarak ⚠ listesinde gösterilir. Elenen: seçimi yeniden açmak · K-1254'ün gerekçe hücresini değiştirmek (kayıt değişmez, düzeltme yeni satırdadır) |

### 5.2 Hizalanan çakışmalar — yeni karar istemeyen

Hepsi var olan bir kararın kuralının değişmeden kaldığı yüzlerin ya da geri işaretlerin hizalanmasıdır; ✓ durumları korunur, kalite döngüsü yeniden açılmaz (K-652). Etki sütunlarına eklenen parçalar, eski kararın karar ve gerekçe hücrelerine dokunmaz.

| # | Yer | Eski hâl | Düzeltme | Şiddet |
|---|---|---|---|---|
| H1 | K-1211 ↔ K-1284 · `08` Aşama 6 park satırı (K-1211 · K-1216 · K-1217) madde (1) (F03, F28) | K-1284 iade güvencesini PayTR için daralttı; K-1211'de geri işaret yok; `08` park satırı "ikinci başarılı ödeme iade edilir" derken sonuç notunu taşımıyordu | K-1211'e K-1284 geri işareti; park satırına "Sonuç (PayTR, K-1284)" cümlesi | orta |
| H2 | K-913 ← K-1242 (3) (F07) | Alıcı hatası sınıfı yalnız servis reddi ve geri dönmeyle doğuyordu; sınır aşımı üçüncü yol | K-913'e geri işaret | düşük |
| H3 | K-1217 ← K-1242 (F08) | K-1217 (2) alıcı sayacını `08`'e park ediyordu; park kapandı | K-1217'ye "park kapandı; `08 §2.2.4`" geri işareti | düşük |
| H4 | K-142, K-1123 ← K-1243 (5) (F09, F10) | Listeden çıkan şirkette "Takip et" bağlantısı gösterilmez; iki eski satırda işaret yok | İki geri işaret | düşük |
| H5 | K-1240 ← K-1249, K-1262 (F11, F17); K-1249 ← K-1263 (F18) | Hesabı silinmiş sipariş misafir koluna girer; beş bildirimde ikinci cümle; yeniden gönderim cümlesinin mekanizması satır kimliği; B-6 ve B-2 listelere girdi | K-1240'a iki, K-1249'a bir geri işaret | düşük |
| H6 | K-1237 ← K-1254 (F13) | "Sağlayıcının adı proje sahibinin cevabını bekler"; ad kapandı | K-1237'ye K-1254 geri işareti (karar metni değişmez) | düşük |
| H7 | K-562 ← K-1256 (F15) | "Bağlantı erişim anahtarı taşır, hesabın silinmesinden etkilenmez" genel kural; üyede bağlantı hesap adresi oldu | K-562'ye daralma geri işareti | orta |
| H8 | K-1258, K-1255 ← K-1259 (F16, S1) | Dönüşte sipariş sayfası çerçevede açılır ve `frame-ancestors 'self'` ister; K-1259 geçiş sayfasına taşıdı | İki geri işaret | orta |
| H9 | K-1112 ← K-1270 (6) (F19) | Kapanmış siparişin sonradan doğan kart iadesi Veri Modeli'ne parktı; yönü kapandı | K-1112'ye geri işaret | düşük |
| H10 | K-1239 ← K-1270 (6), (8) · K-1287 (F20, S2, F30, F31) | Sızıntıda anahtar geçişi, anahtar kaldırma şartı, kaldırma sonrası iade — dört kol tek satırda eksik | K-1239'a iki geri işaret parçası | orta |
| H11 | K-1248 ← K-1271 (3), K-1288 (F21, F32) | Hata eşlemesi ve askıdaki liste kuralı sonraki iki kararla genişledi / daraldı | K-1248'e iki geri işaret | düşük |
| H12 | K-1262 ← K-1273 (F22) | "Yeni alan gerekmez" ve olay değerlerinde taşıma; K-1273 sütuna çıkardı | K-1262'ye geri işaret | düşük |
| H13 | K-1270 ← K-1275 (10), K-1284 (4) (F23, F29) | "Kalan risk" adlandırması ve çift tahsilat kalan riski; ilki yasal yükümlülüğe, ikincisi canlıya alma kapısına döndü | K-1270'e iki geri işaret | orta |
| H14 | K-1061 ← K-1281 (2) · K-1279 ← K-1281 (F26, F27) | "Bağ eşleşen ilk girişin işleminde yazılır" ve "bugünkü kural değişmedi (seçenek a)"; Google yetkili değilse teyitle kurulur | İki geri işaret | orta |
| H15 | K-1287 ← K-1291 (1) (F33) | "Panelin bekleyen işler sayaçları bu siparişleri sayar"; altı sayaç kuralıyla çelişiyordu, geri alındı | K-1287'ye geri işaret | düşük |
| H16 | `DEPLOY_RUNBOOK` Aşama 7 park bloğu — PayTR (6) ve (3), SES (1) ve (3) (F34, F35, F36, F39) | Canlı kanıtta taksitli ödeme, SES deneme iletisi, askıdaki listeden çıkarma adımı, PayTR panelinde alan adı kaydı — `08` gövdesi iş bırakıyordu, blokta yoktu | Dört işe karşılık dört cümle | orta |
| H17 | `09` Aşama 7 park bloğu ← `08 §2.2.5` madde 4 (F38) | Değişken metnin kaçışı ve donmuş kopyada saklama seçimi `09`'a bırakılmış, blokta yoktu | Yeni park maddesi (K-1027 · K-1271); `08 §5.2` satır 10 | orta |
| H18 | K-1257, K-1245, K-1272, K-1281 → `09` etki sütunu (D09) | Etiketteki dört karar `09` park maddelerinde içerik taşıyor, etki sütunu `09`'u anmıyordu | Dört etki sütununa `09` parçası; K-1271'in etki sütununa yeni maddenin parçası | düşük |
| H19 | K-1259 → `12` etiketi · K-1283 etiket fazlası · K-1284 etiketi (D12, S3) | K-1259 içeriği `12` maddesinin (4) kolunda ama etki sütunu anmıyor; K-1283 ve K-1284 etikette ama içeriği o maddede yok | K-1259'a `12` parçası; etiketten K-1283 ve K-1284 çıkarıldı; K-1284'ün etki sütunu Aşama 6 maddesini adıyla gösterir | düşük |
| H20 | K-1245, K-1255, K-1284 → `DEPLOY_RUNBOOK` etki sütunu (DDR) | Etiketteki üç karar kurulum maddesinde içerik taşıyor, etki sütunu anmıyordu | Üç etki sütununa parça | düşük |

### 5.3 Park satırları

**(a) Aşama 1–6'nın park satırları — numaraya dayalı tarama.** 224 madde; K-1227…K-1292'de kaynak K'ların **255 eşleşmesi** (90 maddede) yedi dilimde okundu (`08` — 23 madde, iki dilim · `09` — 20 · `05`/`04` — 15 · `12` — 12 · `DEPLOY_RUNBOOK` — 11 · `07`/`06` — 9). Okuyucular 4 aday çıkardı; doğrulama sonucu:

| # | Park satırı | Değiştiren karar | Hüküm |
|---|---|---|---|
| P-1 | `08` Aşama 4 (K-892 · … · K-978 — gönderim ve iade sorgusu) | K-1270 (3), (5), K-1254 (3b) | **Çürütüldü (F01)** — park "sağlayıcıdan hata yanıtı alınca ne yaptığı bu aşamanın kararıdır" dedi; K-1270 (3) talimatın cevabı, (5) koşullu not |
| P-2 | `08` Aşama 4 (K-900 · K-942 · K-968 · K-973 — Google ile giriş) | K-1281, K-1245 | **Çürütüldü (F02)** — park talimat; K-1281 satırın kaynaklarını değiştirmiyor (K-900, K-1061 yeniden açılmaz) |
| P-3 | `08` Aşama 6 (K-1211 · K-1216 · K-1217 — madde 1) | K-1284 | **Kısmen (F03)** — satır iz bakımından eksikti; "Sonuç (PayTR, K-1284)" eklendi (H1) |
| P-4 | `07` Aşama 4 (K-914 · K-918 · K-974 · K-978) | K-1270 (5) | **Çürütüldü (F04)** — niteleyici koşullu; `08 §2.1.9` park satırıyla aynı şeyi söylüyor |

Geri kalan 251 eşleşme yalnız atıf, "değişmez" ya da uygulanış kaydıdır. `08`'in Aşama 1, 3, 4 ve 6 bloklarının kaynak kararları (81 eşleşme, en büyük dilim) `08`'in yazımının girdisiydi: satırların işinin yazılmış olması tek başına aday sayılmadı.

**(b) Aşama 7'nin park satırları (iki yönlü betik).** `09` 3 · `12` 1 · `DEPLOY_RUNBOOK` 3 = **7 madde** (bu taramadan önce 6; `09`'a bir madde eklendi). **A yönü** — etiketteki her kararın etki sütunu hedef dokümanı anıyor: ilk koşumda 9 fark (`09` için K-1245, K-1257, K-1272, K-1281; `12` için K-1259 ve K-1283; `DEPLOY_RUNBOOK` için K-1245, K-1255, K-1284); yeni `09` maddesiyle K-1271 eklendi. Doğrulama sonucu: 9 etki sütunu eksikliği (H18…H20) ve 2 etiket fazlası (`12` etiketinde K-1283 ve K-1284 — H19). **B yönü** — etki sütunu hedefi anan karar bir etikette ya da metinde geçiyor: ilk koşumda `09` için K-1242, `12` için K-1281 ve K-1292, `DEPLOY_RUNBOOK` için K-1257 ve K-1292 — doğrulayıcılar hepsini çürüttü: K-1242 Aşama 6'nın bir maddesini kapatır (değişiklik yok); K-1281'in içeriği `12` maddesinin (5) kolundadır ve metinde adıyla anılır; K-1257 ve K-1292 "değişmedi" ya da süreç kararlarıdır. Son koşumda B yönünde kalan fark yalnız bu gerekçeli altı atıftır (K-1242, K-1281, K-1284, K-1292 iki hedefte, K-1257).

**(c) Aşama 7 kararlarının önceki aşamaların park satırlarını güncellemesi.** 255 eşleşmeden 1'inde güncelleme gerekti (P-3); Aşama 6'nın taramasındaki 4 adayla karşılaştırılabilir değildir — o tarama 83 kararı okudu, bu tarama 66'sını (K-1227…K-1292).

### 5.4 Geri işaret, zincirler ve öncüller

349 çift (222 Aşama 1–6, 127 Aşama 7 içi); eski satırın etki sütununda yeni numara **olmayan** çiftler: 346 (Aşama 1–6 için 221/222, Aşama 7 içi 125/127). Okuyucular çiftlerden **29 aday satır** çıkardı; hepsi birer Opus / high doğrulayıcısına verildi (F05…F33, §5.6) — **9 doğru, 15 kısmen** (toplam 24 eksik geri işaret ya da hâl; bazıları aynı satırda birleşti) ve 5 çürütülen: yeni karar eski kuralı gerçekten değiştirmiyor, yalnız atıf ya da uygulanış.

| Çift | İlişki |
|---|---|
| K-1240 → K-1249, K-1262; K-1239 → K-1270, K-1287; K-1270 → K-1275, K-1284; K-1248 → K-1271, K-1288 | genişletir / daraltır / netleştirir; kuralın değişikliği yeni satırda açık, eski satırda işaret yoktu |
| K-1237 → K-1254; K-1258, K-1255 → K-1259; K-1287 → K-1291 | yeni satır eski satırın sözünü kapattı ya da geri aldı |
| K-1061, K-1279 → K-1281; K-562 → K-1256; K-1112 → K-1270 | yeni karar eski genel kuralı daraltır; eski satır kuralı genel biçiminde söylüyordu |

Çürütülen beş küme: K-894 ← K-1238 (soru K-940 ile kapanmıştı, geri işaret zaten var) · K-759 ← K-1242 (yeniden gönderim kuralı değişmedi, "sayılır" sayacın tanımıdır) · K-13 ← K-1255 (2) (3D Secure kuralı aynen geçerli, yalnız denetim yolu değişti) · K-1242 ← K-1278 (muafiyet K-1242'nin kendi kuralıdır) · K-900 ← K-1281 (K-900'ün cümlesi yeniden doğrulama kuralıdır, bağ kurma kuralı değil).

**Zincirler.** Sağlayıcı seçimi zinciri (K-1235 → K-1237 → K-1254 → K-1294) bu taramada tamamlandı; çift tahsilat zinciri (K-1211 → K-1270 (4) → K-1284) ve anahtar kaldırma zinciri (K-1239 → K-1270 (8) → K-1287 → K-1291) `08 §2.1.2`, §2.1.7 ve `05 §8.5.6`'da son hâlini söylüyor; eksik olan yalnız kayıttaki geri işaretlerdi. Dönüşün çerçeve zinciri (K-1255 (1) → K-1258 (2) → K-1259) `07 §4.29.1` ve `05 §6.5.4`'te güncel.

### 5.5 Betikle aranan ve temiz dönen çakışma sınıfları

| # | Sınıf | Sonuç |
|---|---|---|
| 1 | **⚠ listesi ↔ kayıt** | ✓ düzeltildi — 19 / 30 → 30 / 30 (§4.1; öğrenim adayı 5) |
| 2 | **K numarası tekilliği** — çift K taraması | ✓ — boş; K-01…K-1294, boşluk yok |
| 3 | **Karar satırının biçimi** — altı sütun | ✓ — 1.294 satırın hepsi (K-1151 ve K-1220'de hücre içi `\|` kaçışlıdır ve kod aralığındadır) |
| 4 | **Park etiketi ↔ etki sütunu** (Aşama 7 blokları, iki yön) | ✓ — son koşum temiz; kalan altı B yönü atıfı gerekçelidir (§5.3 b) |
| 5 | **Tracker §4** | ✓ — 18 / 18 kapalı |
| 6 | **Aşama 7 etki sütunlarında yalnız doküman numarası taşıyan devir atfı** | ✓ — 8 atıf (7 karar: K-1258, K-1272, K-1281, K-1283, K-1287, K-1291, K-1292); hepsinin işi park bloğunda adıyla bulundu (§6.1) |
| 7 | **Değişen dosyalarda CR, kontrol karakteri ve `$[0-9]` artığı** | ✓ — sıfır (PR öncesi üç tarama) |
| 8 | **Kapsam sayıları** — `10`'un "Evet" ve toplam satırı, `YH` aralığı | ✓ — 63 → 64 satır, "Evet" 22 → 23, "Açık" 41; YH-1…YH-23 (23 satır); `10`'un canlı metninde sayıyı taşıyan başka cümle yok (v0.2 başlık notundaki "altmış satır" tarihsel kayıttır) |

### 5.6 Bağımsız doğrulama (Opus / high, bulgu başına bir)

| # | Aday | Hüküm | Sonuç |
|---|---|---|---|
| F01 | `08` park (K-892…K-978) ↔ K-1270, K-1254 | Çürütüldü | Park talimatı K-1270 (3) ile uygulandı |
| F02 | `08` park (K-900…K-973) ↔ K-1281, K-1245 | Çürütüldü | Park talimat; kaynaklar değişmedi |
| F03 | `08` park (K-1211…K-1217) ↔ K-1284 | Kısmen | Park satırına sonuç cümlesi; K-1211 geri işareti |
| F04 | `07` park (K-914…K-978) ↔ K-1270 (5) | Çürütüldü | Koşullu not |
| F05 | K-894 ← K-1238 (4) | Çürütüldü | Soru K-940 ile kapanmıştı |
| F06 | K-759 ← K-1242 (4) | Çürütüldü | Kural değişmedi |
| F07 | K-913 ← K-1242 (3) | Kısmen | Geri işaret |
| F08 | K-1217 ← K-1242 | Kısmen | Park kapandı — geri işaret |
| F09 | K-142 ← K-1243 (5) | Kısmen | Geri işaret |
| F10 | K-1123 ← K-1243 (5) | Kısmen | Geri işaret |
| F11 | K-1240 ← K-1249 | Doğru | Geri işaret |
| F12 | K-978 ← K-1254 (3)(b) | Kısmen | K-978 değişmez; (b) gerekçesi yanlış — **K-1294** |
| F13 | K-1237 ← K-1254 | Kısmen | Geri işaret (ad kapandı) |
| F14 | K-13 ← K-1255 (2) | Çürütüldü | 3D kuralı aynen |
| F15 | K-562 ← K-1256 | Doğru | Geri işaret |
| F16 | K-1258 ← K-1259 | Doğru | Geri işaret |
| F17 | K-1240 ← K-1262 | Doğru | Geri işaret |
| F18 | K-1249 ← K-1263 | Kısmen | Geri işaret |
| F19 | K-1112 ← K-1270 (6) | Doğru | Geri işaret |
| F20 | K-1239 ← K-1270 (8) | Kısmen | Geri işaret |
| F21 | K-1248 ← K-1271 (3) | Kısmen | Geri işaret |
| F22 | K-1262 ← K-1273 | Kısmen | Geri işaret |
| F23 | K-1270 ← K-1275 (10) | Doğru | Geri işaret |
| F24 | K-1242 ← K-1278 | Çürütüldü | Muafiyet K-1242'nin kendi kuralı |
| F25 | K-900 ← K-1281 | Çürütüldü | Yeniden doğrulama kuralı, bağ kuralı değil |
| F26 | K-1061 ← K-1281 (2) | Doğru | Geri işaret |
| F27 | K-1279 ← K-1281 | Kısmen | Geri işaret |
| F28 | K-1211 ← K-1284 | Doğru | Geri işaret (F03 ile aynı satır) |
| F29 | K-1270 ← K-1284 (4) | Doğru | Geri işaret |
| F30 | K-1235 ← K-1287 | Kısmen | Doğru yer K-1239'dur — geri işaret orada |
| F31 | K-1239 ← K-1287 (5) | Kısmen | Geri işaret (F30 ile aynı parça) |
| F32 | K-1248 ← K-1288 | Kısmen | Geri işaret |
| F33 | K-1287 ← K-1291 (1) | Kısmen | Geri işaret |
| F34 | `08` gövdesi → `DEPLOY_RUNBOOK` PayTR canlı kanıt (taksitli ödeme) | Doğru | Park cümlesi |
| F35 | `08` gövdesi → `DEPLOY_RUNBOOK` SES deneme iletisi | Doğru | Park cümlesi |
| F36 | `08` gövdesi → `DEPLOY_RUNBOOK` SES askıdaki listeden çıkarma (iki okuyucu bloğu) | Doğru | Park cümlesi |
| F38 | `08` gövdesi → `09` değişken metnin kaçışı | Doğru | Yeni park maddesi ve `08 §5.2` satır 10 |
| F39 | `08` gövdesi → `DEPLOY_RUNBOOK` PayTR alan adı kaydı | Doğru | Park cümlesi |
| D09 | `09` park etiketi ↔ etki sütunu | Kısmen | Dört etki parçası; K-1258, K-1292, K-1242 çürütüldü |
| D12 | `12` park etiketi ↔ etki sütunu | Kısmen | K-1259 etki parçası; K-1283 fazla; K-1281, K-1292 çürütüldü |
| DDR | `DEPLOY_RUNBOOK` park etiketi ↔ etki sütunu | Kısmen | Üç etki parçası; K-1257, K-1292 çürütüldü |
| S1 | K-1255 ← K-1259 (F16'nın yan gözlemi) | Doğru | Geri işaret |
| S2 | K-1239 ← K-1270 (6) (F20'nin yan gözlemi) | Doğru | Geri işaret |
| S3 | `12` etiketindeki K-1284 (D12'nin yan gözlemi) | Kısmen | Etiketten çıkarıldı; Aşama 6 maddesi adıyla gösterildi |

(F37, F36'ya katıldı — aynı sorunun ikinci okuyucu bloğu.) Toplam: **44 ayrı bulgu — 16 doğru, 20 kısmen, 8 çürütüldü.**

---

## 6. Devir dizini — Aşama 7 kararlarının sonraki dokümanlara devirleri

**Tek ev:** devrin evi karar satırının etki sütunu ya da park satırıdır; bu dizin devri ikinci kez kaydetmez. Aşama 7'nin etki sütunlarında yalnız numara taşıyan sekiz atıf vardır; hepsinin işi aşağıdaki tabloda adıyla yazılıdır (betik: §5.5, sınıf 6). Her hedef dokümanın "Aşama 7'den park edilen girdiler" bloğu bu bölüme işaret eder (`09`, `12`, `DEPLOY_RUNBOOK`).

- **Önceki aşamaların dizinleri:** `PHASE1_CONFLICT_SCAN.md` §6, `PHASE2_CONFLICT_SCAN.md` §6, `PHASE3_CONFLICT_SCAN.md` §6, `PHASE4_CONFLICT_SCAN.md` §6, `PHASE5_CONFLICT_SCAN.md` §6, `PHASE6_CONFLICT_SCAN.md` §6.
- **`11` Uygulama Planı:** Aşama 7'den devir yok — `11` tüketicidir; etki sütunlarında `11`'i anan karar ve `08`'in gövdesinde `11`'e iş bırakan cümle yok (betik: 0 karar; gövdedeki tek anma bir konvansiyon cümlesidir).
- **`DEFERRED_BACKLOG.md`:** Aşama 7'den yeni kalem yok.
- **Süreç kararları** (K-1233, K-1234, K-1235…K-1237, K-1240…K-1243 gibi workshop ve yerleşim kararları, K-1289, K-1290, K-1292) sonraki dokümana kendi içeriği için iş bırakmaz; iş bırakan kollar aşağıda adlandırılmıştır.

### 6.1 Etki sütunlarındaki devirler (K-1233…K-1294)

| Hedef | Atıf = karar | Aşama 7 park bloğunda | Başka park satırında / metinde | Park gerekmez |
|---|---|---|---|---|
| `09` Kodlama Kılavuzu | 8 | 7 (3 madde) | — | 1 (K-1242 — Aşama 6 maddesini kapatır) |
| `11` Uygulama Planı | 0 | — | — | — |
| `12` Doğrulama Protokolü | 8 | 5 (1 madde) | 1 (K-1281 — aynı maddenin (5) kolunda adıyla anılır) · 1 (K-1284 — Aşama 6 maddesi, K-1204 maddesi) | 1 (K-1292 — süreç) |
| `DEPLOY_RUNBOOK.md` | 17 | 15 (3 madde) | — | 2 (K-1257 — değişmedi; K-1292 — süreç) |
| **Toplam** | **33 atıf = 33 karar-doküman çifti** | **27** | **2** | **4** |

İşin adı etki sütunundan alınmıştır; parantez içi park satırının etiketidir. Tablo kararları azalmayan K sırasıyla verir.

#### `09` Kodlama Kılavuzu — 8 karar, 3 park maddesi

| Karar | İş | Park satırı |
|---|---|---|
| K-1257 · K-1258 | bağlantı katmanının kodlama kalıpları (`08 §3.1`), imzalı uç (`08 §3.2`) | Aşama 7, satır 67 |
| K-1245 · K-1272 · K-1281 · K-1292 | Google kimlik belirtecinin doğrulanması (`08 §2.3`): sabit adres, `exp`, `hd`, `sub` saklanmaz | Aşama 7, satır 68 |
| K-1271 | değişken metnin kaçışı (`08 §2.2.5` madde 4, §2.2.4) — bu taramada eklendi (F38) | Aşama 7, satır 69 |
| K-1242 | alıcı sayacı (Aşama 6 park maddesi kapandı — değişiklik yok) | Aşama 6 bloğu |

#### `12` Doğrulama Protokolü — 8 karar, 1 park maddesi (beş kollu)

| Karar | İş | Park satırı |
|---|---|---|
| K-1257 · K-1258 · K-1259 · K-1272 · K-1291 | dış servislerin doğrulanacak davranışları (`08 §3`, §4): 43 DV satırının kanıtı, canlı kanıt, bildirimin senaryoları, servis çökme senaryoları, geçiş sayfası ((4) kolu) | Aşama 7, satır 72 |
| K-1281 | Google ile girişte var olan hesaba bağın teyit senaryoları ((5) kolu, DV-43) | Aşama 7, satır 72 (metinde anılır) |
| K-1284 | DV-11 — ikinci ödeme oturumunun doğrulaması | Aşama 6 bloğu, satır 67 (K-1204 maddesi) |
| K-1292 | süreç kararı — etki yansıtma | park gerekmez |

#### `DEPLOY_RUNBOOK.md` — 17 karar, 3 park maddesi

| Karar | İş | Park satırı |
|---|---|---|
| K-1247 · K-1248 · K-1266 · K-1271 | E-posta altyapısının (Amazon SES) kurulumu: alan adı kimliği, DNS kayıtları, askıdaki liste, üretim erişimi, gönderim kimliği, listeden çıkarma | Aşama 7, satır 60 |
| K-1239 · K-1254 · K-1255 · K-1266 · K-1270 · K-1283 · K-1284 · K-1287 · K-1291 | Ödeme sağlayıcısının (PayTR) kurulumu: sözleşme ve mağaza hesabı, anahtarlar ve sır döndürme, bildirim adresi, 3D Secure teyidi, canlı kanıt, DV-11 kapısı, anahtar kaldırma şartı, alan adı kaydı | Aşama 7, satır 61 |
| K-1245 · K-1268 · K-1272 | Google uygulamasının kurulumu: uygulama, yetkili yönlendirme adresi, istemci kimliği ve sırrı, yayın durumu | Aşama 7, satır 62 |
| K-1257 | değişmedi | park gerekmez |
| K-1292 | süreç kararı | park gerekmez |

### 6.2 Entegrasyon Spesifikasyonu'nun gövdesinde sonraki dokümanı anan cümleler

Mekanik çıkarım (`08 §1`–§5; başlık notu, Kaynak satırları ve matris hariç): sonraki dokümanı numarasıyla ya da adıyla anan **59 cümle** — `DEPLOY_RUNBOOK` 33, `12` 19, `09` 5, `11` 1; runbook anan 9 (kesişimli sayım). Bir okuyucu her cümlenin işi bırakıp bırakmadığını ve hedefin park bloğunda karşılık bulup bulmadığını okudu: 6 aday, doğrulamada 5 ayrı iş — **karar satırı olmadan devredilen iş beş:** canlı kanıtta taksitli ödeme, SES deneme iletisi, SES askıdaki listeden çıkarma (iki cümle), PayTR panelinde alan adı kaydı (`DEPLOY_RUNBOOK`), değişken metnin kaçışı (`09`); hepsi bu taramada park satırı aldı (H16, H17). Geri kalan cümleler ya bir DV satırının kanıtı olarak `12`'nin maddesinde, ya "ilk bağlanan task runbook yazar" konvansiyonunda (yazım konvansiyonu 11) karşılık buluyor.

| Hedef | Cümle | Okunarak karşılık bulan | Karar satırı olmadan devredilen |
|---|---|---|---|
| `DEPLOY_RUNBOOK` | 33 | 28 | **4 iş** — F34, F35, F36, F39 |
| `12` | 19 | 19 | — |
| `09` | 5 | 4 | **1 iş** — F38 |
| `11` | 1 | 1 (konvansiyon cümlesi) | — |

### 6.3 Doğrulama ve soru devirleri

`08`, dış dünyaya ait olguları implementation'a bırakır: 43 DV satırı (`08 §4.1`) kurulumu yapanın ve ilk bağlanan task'ın doğrulayacağı davranışlardır; evi `12`'nin Aşama 7 maddesidir. **Çok kritik soru adayı yoktur.** Çakışma taramasının kendi bulduğu açık konu yoktur; K-1275'in açık bıraktığı yasal yükümlülük (taksit farkının iadesi) DV-10'a bağlıdır ve ⚠ listesinde proje sahibine gösterilir.

---

## 7. Öğrenim terfisi adayları — 5. adımın girdisi

Metinler tracker §18.1'dedir (1–6); bu dizin onları tek yerde gösterir.

| # | Aday | Kaynak | Durum |
|---|---|---|---|
| 1 | Şablon taraması sekizinci şablonda aynı eksiği buldu — açık kararlar bölümü yok | Açılış (PF-117) | Bekliyor |
| 2 | Devir dizinleri Aşama 1'in workshop kararlarını `08` tablosunda taşımıyor | Açılış | Bekliyor |
| 3 | Gövde anmalarının sayımı matrisin hücre değerini düzyazıdan ayırmalı | Açılış | Bekliyor |
| 4 | Bulgu doğrulamasının maliyeti — seviyeye göre toplu doğrulama (proje sahibinin isteğiyle kapanışta sorulur) | Kalite döngüsü | Bekliyor — proje sahibine sorulur |
| **5** | **⚠ listesinin elle tutulması kayıt satırlarının gerisinde kaldı.** Aşama 6'da liste bir kararı saymıyordu (K-1192); Aşama 7'de 30 kararın 11'i listede yoktu. **Aday hedef:** `checklists/document-stage.md` §7 ve `INSTRUCTIONS §2` — her adımın kapanışı konu hücresi ⚠ taşıyan satırlarla listeyi betikle eşitler | **Bu tarama** (§4.1) | Bekliyor |
| **6** | **Kıyas tablosunun sonuç cümlesi aynı dokümanın uygulama bölümüyle çelişebilir; geri işaret okuması buldu** (K-1294). **Aday hedef:** tek seferlik gözlem — kalite döngüsünün güvence merceği ölçüt tablolarının sonuç cümlelerini uygulama bölümüyle karşılaştırır; raporda kalır, terfi gerekmez | **Bu tarama** (§5.1) | Bekliyor |
| Ö-23 | Checklist'in skill'e dönüştürülmesi | Aşama 1'den | Bekliyor — kapı Aşama 7'nin öğrenim terfisi |

**Ölçüm satırı (maliyet düzeni):** bu tarama 17 Sonnet / low okuyucu + 1 Sonnet / low taslak (≈1,6 milyon belirteç) ve 44 Opus / high doğrulayıcı (≈2,8 milyon belirteç) kullandı; doğrulayıcı sayısı bulgu sayısına eşittir (44 ayrı bulgu). Aşama 7'nin tablosu öğrenim terfisinde yazılır (aday 4).

---

## 8. Checkpoint'e notlar

- Tracker §4'te açık satır yok, A-19 açılmadı; dokümanlarda açık kalem yok — checkpoint'in açık karar kontrolü bu raporla karşılanır.
- ⚠ listesi **30 karar** (§4.1) — hazır metin aynı yerde; arşiv işaretinden önce proje sahibine tek mesajda gösterilir (yönetici). Listeye bu taramanın K-1294'ü dahildir.
- K-1293 ve K-1294 bu taramanın kararlarıdır; yüzleri §5.1'dedir. Checkpoint'in iç tutarlılık merceği `10 §3` KD-64 ile `10 §5` YH-23'ün, `08 §2.1.1`'in (b) satırı ile `08 §2.1.9`'un, `08 §1`'in envanter sınırı ile `08 §5.2` satır 13'ün aynı hâlde olduğunu bir kez daha okuyabilir.
- **Devredilen sınıf** (K-1129): iki yerin metin farkı — özet hücresi ya da kıyas cümlesi ile uygulama bölümü. Bu taramanın bulguları aynı sınıftadır: `08 §2.1.1`'in (b) cümlesi, `08`'in park satırı (K-1211…), `DEPLOY_RUNBOOK`'un dört cümlesi.
- Aşama 1–6'nın park satırları K-1227…K-1292'ye bir kez tarandı (255 eşleşme; 224 madde). Checkpoint yeniden taramaz; yalnız bu PR'ın değiştirdiği satırları okur: `08` (§1 envanter sınırı, §2.1.1, Aşama 6 park satırı madde (1), §5.2 satır 10 ve 13), `09` (yeni park maddesi), `12` (etiket), `DEPLOY_RUNBOOK` (SES (1) ve (3), PayTR (3) ve (6)), `10` (KD-64, YH-23, bağımlılık zinciri cümlesi).
- 6. adımda sonraki aşamaya (Kodlama Kılavuzu `09`) devrin tablosu bu raporun §6'sını ve hedef dokümanların park bloklarını gösterir; öğrenim adayları dizini (§7) 5. adımın girdisidir.

# Aşama 5 — Çakışma Taraması

**Tarih:** 2026-10-18 | **Aşama:** 5 — Veri modeli | **Kapanış adımı:** K-437'nin 3. adımı (K-429; `checklists/document-stage.md` §7)
**Girdi sürümleri:** karar kaydı v0.140 · Veri Modeli (`06`) v0.24 · Ürün Gereksinimleri (`02`) v0.87 · Kullanıcı Akışları (`03`) v0.36 · Arayüz Tanımları (`04`) v0.34 · Teknik Mimari (`05`) v0.23 · MVP Kapsamı (`10`) v0.54 · Proje Vizyonu (`01`) v0.33 · `07`, `08`, `09`, `11`, `12` ve `DEPLOY_RUNBOOK`'un park blokları
**Çıktı sürümleri:** karar kaydı v0.141 · `06` v0.25 · `02` v0.88 · `03` v0.37 · `04` v0.35 · `05` v0.24 · `10` ve `01` değişmedi · `07`, `08`, `09`, `12` ve `DEPLOY_RUNBOOK`'un park blokları

> **Ne işe yarar:** Aşama kapanmadan önce açık kalemlerin her birinin **tek bir evi** ve **gözlemlenebilir bir kapısı** olduğunu doğrular; vadesi geçmiş açıkları adıyla listeler (K-429). Önceki aşamaların ve bu aşamanın park satırlarını, kaynak kararlarını sonradan değiştiren kararlara karşı okur. Aşama 5 kararlarının önceki aşamaların kararlarına geri işaretlerini, birbirleriyle zincirlerini ve olgusal öncüllerini denetler (checklist §3). Aşama 5'in üst dokümanlara geri beslediği kuralların bütün yüzlerde aynı hâlde olduğunu denetler (K-652, betik 1). Gösterilmemiş ⚠ kararların hazır metnini verir (§4.1). Aşama 5 kararlarının sonraki dokümanlara devirlerini tek dizinde toplar (§6) — **API tasarımı aşamasının açılışı bu dizini okur.** Sonuç checkpoint'in (4. adım) girdisidir.

---

## 1. Sonuç

| Kural | Sonuç |
|---|---|
| **(1) Aynı açık iki yerde durmaz** | ✓ — açık karar yok. Tracker §4'ün on sekiz satırının hepsi kapalı; Aşama 5'te A-19 açılmadı. Veri Modeli'nin şablonunda açık kararlar bölümü yok (K-994; konvansiyon 11 — açık kalem tracker §4'te A-19'dan sürer) ve dokümanda "açık" işaretli madde yok. Ürün Gereksinimleri §13 boş; Kullanıcı Akışları, Arayüz Tanımları, Teknik Mimari, MVP Kapsamı ve Proje Vizyonu'nda açık kalem yok (§3). |
| **(2) Her açık satır gözlemlenebilir bir kapı taşır** (K-37) | ✓ — tracker §14.1'in açık dört süreç maddesinin dördü de kapılı (§4): ⚠ listesi → arşiv işaretinden önce (hazır metin §4.1); checkpoint'e devredilen metin farkı sınıfı → checkpoint (etki yansıtmanın eklediği örneğin evi bu taramada belirlendi ve kapandı — §5.2); öğrenim adayları 1–19 → öğrenim terfisi (bu taramadan 20–23 eklendi — §7); Ö-23 → öğrenim terfisi. |
| **(3) Vadesi geçmiş ve açık satır adıyla listelenir** | ✓ — **yok.** Süreç maddelerinin kapısı (checkpoint, öğrenim terfisi, arşiv işareti) henüz gelmedi. |

**Çakışmalar:** on salt okuma merceği (iki dalga, beşer) ve betikler **84 bulgu satırı** döndürdü — **20 orta, 64 düşük**; üçü aynı yerin ikinci mercekten bulunuşudur (81 ayrı düzeltme). Hepsi bu PR'da kapandı; uygulamanın ardından bir şüpheci doğrulayıcı değişiklikleri güncel metne karşı okudu ve on iki düzeltme daha getirdi — biri orta (§5.6). **Beş karar** (§5.1): **K-1132** — Kodlama Kılavuzu'nun iş kuyruğu satırı varsayılan yeniden denemeyi Teknik Mimari'nin bugünkü sözüyle yazar · **K-1133 ⚠** — gizli anahtarın döndürülmesinde ve kurtarmada e-posta değişikliğini geri alma bağlantısı açık kalır · **K-1134 ⚠** — e-posta değişikliği isteğinde doğrulanmamış bekleyen kayıt hesap sayılmaz · **K-1135 ⚠** — bekleyen kaydın istek içindeki silmeleri doğuran işlemin parçasıdır · **K-1136 ⚠** — kandırılmış e-posta düzeltmesinden sonra belgedeki onay anı e-postası kalan risktir. **Park satırları** (§5.3): Aşama 1–4'ün 108 maddesinde K-977…K-1131'in **831 eşleşmesi** okundu — **25 hizasızlık** (Veri Modeli'nin tüketilmiş bloklarında on bir); Aşama 5'in 30 maddesinde **21** taşınmamış kol, iki yeni satır (`09`'a K-1124 ve K-1095); park etiketi ↔ etki sütunu son koşumda iki yönde temiz. **Geri işaret** (§5.4): 535 Aşama 1–4 çiftinin ve 91 Aşama 5 içi çiftin hepsi okundu — **24 eksik geri işaret**, üç zincir bulgusu, beş öncül bulgusu. **Geri beslemenin yan yüzleri** (§5.2): altı — Ürün Gereksinimleri'nin kalan ticari kayıt listesi, Teknik Mimari'nin dosya düzlemi ve referans fiyatın satışa kapalı aralıkları dahil; uygulamanın terim taraması beş yüz daha buldu. **Devir dizini** (§6): Aşama 5'in **70 kararı** sonraki dokümanlara **93 atıf** taşır; **hiçbiri yalnız doküman numarası değildir**; `06`'nın gövdesinde sonraki dokümanı anan 45 maddenin hepsi bir park satırında karşılık bulur. **⚠ listesi:** kayıtla birebir — 30 / 30, bu taramayla **34** (§4.1). **Öğrenim adayları:** dört yeni (§7). **Çok kritik soru çıkmadı.**

---

## 2. Yöntem

Tarama gözle değil betikle yapıldı (K-429); anlamsal okuma on salt okuma merceğiyle, ayrı alt ajanlarda ve ön planda koştu — iki beşli dalga; bulgular çalışma dosyalarına yazıldı, hükmü verilmiş düzeltmeler dört uygulama ajanına dosya başına bölünerek verildi ve uygulama ajanı her düzeltmeden önce hedef metni güncel hâliyle yeniden okudu; ardından bir şüpheci doğrulayıcı diff'i okudu. Ortam: Git Bash ve Python 3.12 (`python -I -X utf8`). **Model dağılımı:** mercekler ve doğrulayıcı Opus; tarama, sayım ve karşılaştırma betikleri yöneticide; düzeltmelerin uygulanması dört Sonnet ajanı (karar kaydının etki sütunları · Veri Modeli · Ürün Gereksinimleri, Kullanıcı Akışları, Arayüz Tanımları ve Teknik Mimari · park blokları).

1. **Tracker §4 ve §14.1:** `## 4.` ile `## 5.` arasındaki `| A-NN |` satırları ayrıştırıldı ve durum hücresi "Kapandı"ya karşı denetlendi; A-19 ve sonrası dosyanın tamamında arandı (eşleşmelerin hepsi "A-19'dan sürer" kural cümlesi). §14.1'in işaretsiz maddeleri listelendi ve her birinin "Kapı:" cümlesi okundu. ⚠ listesi: K-992…K-1131'in konu hücresinde `(öneriyle kaydedildi — ⚠` taşıyan satırlar, §14.1'in listesindeki `K-n — ` girişleri ve yöneticinin sayımı (30) üç yönde karşılaştırıldı.
2. **Dokümanların açık kalemleri:** `06`, `01`, `02`, `03`, `04`, `05` ve `10`'da "açık karar", "açık kalem", "belirlenecek", "netleşecek", "karara bağlanacak", "TBD", "(downstream)", "sonra doldurulacak", "muhtemelen", "gereksinime göre", "A-19" desenleri; her eşleşme okundu ("açık kalem" `02`, `03`, `04` ve `06`'da çoğunlukla siparişin açık kalemi terimidir; `06`'nın "karara bağlanacak" eşleşmeleri park bloklarının kalıp cümlesi).
3. **Park satırları — Aşama 1–4:** sekiz dokümanın (`04`–`09`, `12`, `DEPLOY_RUNBOOK`) "Aşama N'den park edilen girdiler" bloklarındaki `> - **…**` maddeleri ayrıştırıldı (dizin maddeleri hariç **108 madde** — `06` 23, `07` 11, `08` 14, `09` 14, `12` 19, `DEPLOY_RUNBOOK` 14, `04` 3, `05` 10) ve etiketlerinden kaynak K'lar çıkarıldı (aralıklar açıldı; **319 kaynak**). Her kaynak için **K-977…K-1131**'de — K-976'ya kadarını Aşama 4'ün taraması okudu — tam eşleşme (`K-NN` ardından rakam yok, önünde harf ya da tire yok) arandı; ilişki sözcüğüne bakılmadı. **831 eşleşme** mercek başına dosyalara hücre adıyla alıntılandı ve **her eşleşme okundu**: M1a (`06` — 275), M1b (`07`, `08`, `04`, `05` — 153), M1c (`09`, `12` — 253), M1d (`DEPLOY_RUNBOOK` — 150). `06`'nın blokları bu aşamanın girdisiydi ve tüketildi: kapanışları `06 §1.1.11`'in `park An-m` satırlarındadır (23 satır, 21 "eşlendi", 2 "veri doğurmaz" — betikle); metinleri yine de sonraki okuyucu için kaynak kararın son hâline hizalandı.
4. **Park satırları — Aşama 5:** `07`, `08`, `09`, `12` ve `DEPLOY_RUNBOOK`'un Aşama 5 bloklarının 30 maddesi, etikette geçen ya da etki sütunu hedefi anan her kararın karar ve etki hücresiyle birlikte döküldü (M2a: `07`, `08`, `12`; M2b: `09`, `DEPLOY_RUNBOOK`). Betik (`f5_park.py`, ardından bütün blokları kapsayan `s9_twoway.py`) etiket ↔ etki sütununu iki yönde numara düzeyinde karşılaştırdı; mercekler her kararın bütün numaralı kollarını, kalite döngüsünün o karara dokunan kararlarını, "park gerekmez" gerekçelerini ve satırın andığı `06 §…`, `05 §…` maddelerini güncel metne karşı okudu.
5. **Geri işaret ve öncül (mercekler Mc1, Mc2, Mc3):** K-992…K-1131'in her satırında anılan, numarası K-992'den küçük K'lardan eski satırın etki sütununda yeni numarayı taşımayanlar betikle çıkarıldı (**535 çift, 463'ü işaretsiz**; 284'ü alıntıda ilişki sözcüğü taşıyor) ve hepsi okundu; Mc3 kalite döngüsünün kararlarının aynı aşamanın K-992…K-1074'üne geri işaretlerini kendi betiğiyle çıkardı (91 çift, 54'ü işaretsiz). Dört daraltma zinciri (IBAN'ın silinme anı, saklama, eski bir ana dönüş, geç gelen ödeme) eski karar hücresi ve `06`'nın son hâli açısından okundu. 30 ⚠ kararın ve öncülü başka bir kurala dayanan öteki kararların öncülleri kuralın evinde okundu; Mc2 `06 §4.36`…§4.72'nin andığı 24 kullanıcı ve firma işleminin varlığını `03` ve `04`'te okudu (hepsi var).
6. **Geri beslemenin yan yüzleri (mercek Md, K-652 betik 1):** etki yansıtmanın dört kümelik taramasının artığı arandı — etki yansıtmanın devrettiği somut örnek (`02 §12.2.7`), öteki merceklerin rastladığı iki üst doküman yüzü, sınıfı ölçüt alan kapalı listeler (on yıl, elektronik ticaret kayıtları, imha kaydına yazılan silmeler, anonimlik) ve Aşama 5'te sözlüğe giren yirmi terimin `06` adları. On terim kümesi, ~385 eşleşme; terim listeleri bulgu dosyasının başındadır ve özetleri §5.2'de.
7. **Devirler:** K-992…K-1136'nın etki sütunu ` · ` ile parçalandı (parantez içi korunarak); `07`, `08`, `09`, `11`, `12` ya da `DEPLOY_RUNBOOK`'u anan parçalar alındı, parçanın parantezi işin adı olarak okundu ve kararın hedef dokümanın park bloklarındaki yeri (Aşama 5 bloğu ya da güncellenen önceki aşama satırı) betikle bulundu. `06`'nın §2–§9 gövdesinde sonraki dokümanı — numarasıyla (`` `07` ``, `07 §`) ya da adıyla — anan maddeler çıkarıldı ve hedef dokümanın Aşama 5 park bloğunun o bölümü anıp anmadığı denetlendi; anmayanlar tek tek okundu.

---

## 3. Açık kalemler

### 3.1 Tracker §4

| Kapsam | Satır | Açık | Sonuç |
|---|---|---|---|
| A-01…A-18 (Aşama 1, salt okunur) | 18 | 0 | Aşama 1'in taraması §3.1 |
| A-19 ve sonrası (Aşama 5) | 0 | 0 | Aşama 5'te açık detay doğmadı: matrisin, workshop'un, yazımın ve kalite döngüsünün bulduğu her boşluk oturumunda bir K satırıyla kapandı (K-993…K-1131); bu tarama da K-1132…K-1136 ile kapattı |

### 3.2 Dokümanlar

| Kaynak | Bulunan | Sonuç |
|---|---|---|
| Veri Modeli (`06`) | Şablonda açık kararlar bölümü yok (K-994; PF-86); konvansiyon 11 açık kalemi tracker §4'e, sonraki aşamaya talimatı park bloğuna gönderir. Desen eşleşmeleri: "açık karar" konvansiyon 11 ve matrisin `02 §13` satırları; "karara bağlanacak" dört kez park bloklarının kalıp cümlesi; "(downstream)" konvansiyon 11; "gereksinime göre" konvansiyon 12'nin yasağı; "açık kalem" yirmi bir kez siparişin açık kalemi terimi | Açık kalem yok |
| Ürün Gereksinimleri §13 | Tablo boş | Açık karar yok; Aşama 5'in geri beslemesi (v0.77…v0.88) §13'e satır açmadı |
| Kullanıcı Akışları, Arayüz Tanımları, Teknik Mimari, MVP Kapsamı | "açık karar" eşleşmeleri kural cümleleri ve eski başlık notları; "açık kalem" eşleşmeleri siparişin açık kalemi terimi | Açık kalem yok |
| Proje Vizyonu | Eşleşme yok | — |

### 3.3 Raporlarda kalmış açık not

Yok. Veri Modeli'nin cross-review'ının beşinci raporu raporlarda taşınan açık notları kapalı sayar (`06_CROSS_REVIEW_R5.md` §7): 1.–4. turların reddedilen bulguları kararla ya da gerekçeyle kapalıdır; deep review'ın uygulanmayan beş bulgusu §14.1'in maddesinde gerekçeyle kapandı (cross-review TEMİZ döndüğünde). Cross-review'ın çıkış kuralıyla devrettiği sınıf §14.1'in checkpoint maddesidir (§4). Kalite döngüsünün açık maddeleri (yazım turunun 3b ve 3c oturumlarından) audit raporunda kapandı.

---

## 4. Süreç maddeleri ve kapıları

Tracker'ın işaretsiz maddeleri bu taramadan önce dörttü, hepsi §14.1'de. Devir §13'ün girdileri, `06`'nın gövdesi ve hafızanın Güncel Durum bloğu ayrıca tarandı.

| Madde | Ev | Kapı | Sonuç |
|---|---|---|---|
| Checkpoint'e devredilen sınıf — iki yerin metin farkı: özet hücresi ya da genel tanım cümlesi ile ayrıntının kuralı (K-1129) | Tracker §14.1 | Aşama 5'in checkpoint'i | Kapı var; etki yansıtmanın eklediği örneğin (`02 §12.2.7` ↔ `06 §9.2`) evi bu taramada belirlendi ve örnek kapandı (§5.2, Md-1); bu raporun §8'i checkpoint'in girdisidir |
| Bekletilen öğrenim Ö-23 — checklist'in skill'e dönüşmesi | Tracker §14.1 · PF-32 · devir §13'ün 7. satırı | Aşama 5'in öğrenim terfisi | Kapı var |
| ⚠ öneriyle kayıt listesi (K-992) | Tracker §14.1 | Arşiv işaretinden (6. adım) önce proje sahibine tek mesajda | **Kapı somutlaştı** — hazır metin (§4.1) |
| Öğrenim terfisi adayları 1–19 | Tracker §14.1 | Aşama 5 kapanışının öğrenim terfisi | Kapı var; bu taramadan 20–23 eklendi (§7) |
| Playbook listesinin gönderimi (K-649) | `PLAYBOOK_FEEDBACK.md` | Proje tamamlandıktan sonra, proje sahibi | Kapı var |
| D-01 — kurulumun ertelenen parçası | `DEFERRED_BACKLOG.md` | İlk implementation task'ından önce | Kapı var; Aşama 5'ten yeni kalem yok |
| Desteklenen ödeme sağlayıcısının sayısı (çok kritik soru adayı; K-932) | `08`'in Aşama 4 park satırı | Entegrasyon spesifikasyonları aşamasının açılışı | Kapı var; Veri modeli aşamasının sorusu değildir (devir §13) |

Devir §13'ün öteki girdileri kapandı: açılış sorusu K-992 ile cevaplandı; çok kritik soru (net miktarın düzlemi) proje sahibince karara bağlandı (K-1019); PF-74 açılışta kapandı (K-996); girdilerin tarayarak okunan kısımları maddesi matrisin oturumları ve yirmi W konusuyla kapandı.

### 4.1 ⚠ öneriyle kayıt listesi — doğrulama ve proje sahibine gösterilecek metin

**Betikle doğrulama.** Konu hücresi `(öneriyle kaydedildi — ⚠)` taşıyan Aşama 5 satırı (K-992…K-1131) **30**; §14.1'in listesindeki giriş **30**; yöneticinin sayımı **30** — üç küme birebir, eksik 0, fazla 0. Aşama 5'in 140 satırının işaret dağılımı: ⚠ 30 · `(öneriyle kaydedildi)` 108 · işaretsiz 2 (K-992 ve K-1019 — proje sahibinin kararları). `(toplu onayla kaydedildi)` yok. **Bu taramayla:** K-1133…K-1136 ⚠, K-1132 `(öneriyle kaydedildi)` — Aşama 5'in 145 satırında ⚠ **34** · öneriyle 109 · işaretsiz 2.

| Adım | Kararlar | Sayı |
|---|---|---|
| Matrisin 2b oturumu | K-1008 | 1 |
| Workshop 1. oturum | K-1010 · K-1016 · K-1018 | 3 |
| Workshop 2. oturum | K-1027 | 1 |
| Yazım turu 2b1 · 2b2 · 3a | K-1052 · K-1056 · K-1057 · K-1058 · K-1064 · K-1065 | 6 |
| Audit | K-1075 · K-1076 · K-1077 · K-1078 · K-1080 · K-1081 · K-1082 · K-1086 · K-1092 · K-1093 · K-1095 | 11 |
| Deep review | K-1104 · K-1105 · K-1110 · K-1114 · K-1115 · K-1117 · K-1122 | 7 |
| Cross-review 2. tur | K-1127 | 1 |
| Çakışma taraması | K-1133 · K-1134 · K-1135 · K-1136 | 4 |
| **Toplam** | | **34** |

**Kapı.** Liste arşiv işaretinden (6. adım) önce proje sahibine **tek mesajda**, karar başına bir sade cümleyle ve gerekçesiz gösterilir (`INSTRUCTIONS.md` §2, "Ortak kural"); yöneticiden proje sahibine gider (§9). İtiraz gelmeyen satırın işareti `(öneriyle kaydedildi — ⚠ — gözden geçirildi YYYY-AA-GG, itiraz yok)` olur; itiraz gelen karar yeni bir satırla değişir. Checkpoint ya da öğrenim terfisi yeni bir ⚠ karar kaydederse liste arşiv işaretinden önce onunla birlikte gösterilir. Deep review'ın bilgi notu (IBAN'ın girilmesinin ve düzeltilmesinin müşteriye bildirilmesi seçeneği — eski bir kararla elenmişti, çok kritik değildir) gösterimde anılır (§14.1).

**Kapının sonucu — 2026-10-18 (K-437'nin 6. adımı; K-1142):** kapı işledi — liste arşiv işaretinden önce iki gösterimle verildi (`INSTRUCTIONS.md` §9 işletim kuralı 3): aşağıdaki hazır metnin otuz dört kararı yönetici aracılığıyla erken gösterildi (Soru 5) — *"itiraz yok"*; checkpoint'in üç kararı (K-1137, K-1138, K-1140) ve K-1092 ile K-1135'in düzeltilmiş cümleleri (aşağıdaki "Retro Güncelleme"; CP05 §5.2) ikinci kez gösterildi (Soru 6) — *"itiraz yok"*. Otuz yedi satırın konu hücresindeki işaret `(öneriyle kaydedildi — ⚠ — gözden geçirildi 2026-10-18, itiraz yok)` oldu; sayım betikle doğrulandı (37 / 37 — bu listenin, CP05 §5.2'nin ve kaydın kümeleri birebir). Deep review'ın bilgi notu gösterimde anıldı; proje sahibi açmadı — K-710 değişmedi. Gösterimden sonra kaydedilen ⚠ karar yok. Madde karar kaydının §14.1'inde kapandı; arşiv işareti aynı PR'da düştü (v0.144).

**Hazır metin — proje sahibine gösterilecek liste** (konulara göre; dokümanlar adıyla):

*Silme ve saklama*
- K-1010 — Her kayıt türü beş silme biçiminden birine bağlıdır: hiç silinmez, bir işlemle hemen silinir, süresi dolunca silinir, süresi dolunca kişisel bilgileri boşaltılır ya da her gece temizlenir; "silindi" diye işaretlenip saklanan kayıt yoktur ve kaldırılan yöneticinin adı işlediği siparişlerde yazılı kalır.
- K-1052 — Siparişin kişisel bilgileri süresi dolunca silinirken siparişin sepetle ve yöneticiyle bağı ve düzeltmeye yazılan açıklama da silinir; bunun bedeli, eski dönemin ödeme tamamlama oranının biraz düşük okunmasıdır.
- K-1064 — Bir yöneticinin panelden kaldırılması, hesap silmesi gibi silme kaydına yazılır ve veritabanı yedekten eski bir ana dönerse yeniden uygulanır; üyenin adres defterinden adres silmesi bu kayda girmez; süresi dolan yönetici davetleri her gece silinir.
- K-1065 — E-posta adresi değişince eski adres, "bu değişikliği ben yapmadım" bağlantısının yedi günlük ömrü boyunca saklanır, sonra silinir.
- K-1092 — Ayıp talebi siparişten bağımsız olarak son değiştiği andan on yıl saklanır; ödemesi alınmış siparişe gelen ödeme bildirimi de geldiği andan on yıl saklanır. *(Cümle düzeltildi — aşağıda "Retro Güncelleme — 2026-10-18".)*
- K-1093 — Silmeden sonra kalan ticari kayıt, kargo şirketinin ve ödeme kuruluşunun aynı siparişe ait kayıtları da silinmedikçe anonim sayılmaz; firmanın saklama politikası bu ikisini de anar.
- K-1095 — Kişisel bilgileri silinmiş bir siparişe iç not yazılamaz.
- K-1135 *(bu taramada eklendi)* — Doğrulanmamış bir kaydın, yeni e-posta adresinin doğrulanması ya da Google ile ilk giriş sırasında silinmesi, veritabanı yedekten eski bir ana dönerse o işlemle birlikte geri gelir ve ayrıca yeniden uygulanmaz. *(Cümle düzeltildi — aşağıda "Retro Güncelleme — 2026-10-18".)*

*Sipariş belgeleri ve adres*
- K-1008 — Firma siparişin adresini düzelttiğinde eski ve yeni adres siparişin kaydında tutulur; panelde yöneticiye görünür, müşteriye görünmez ve siparişin kişisel bilgileriyle birlikte silinir.
- K-1027 — Siparişin sözleşme belgeleri müşterinin adı, adresi ve e-postası olmadan saklanır, gösterilirken onay anındaki bilgilerle doldurulur; firma adresi ya da e-postayı sonradan düzeltse de belge onaylanan hâliyle görünür.
- K-1136 *(bu taramada eklendi)* — Firma kandırılıp bir siparişin e-postasını başka birine çevirirse, sipariş sayfasına erişen kişi sözleşme belgelerinde siparişin ilk e-posta adresini de görebilir; bu, kalan risk olarak yazıldı.

*İade, geri ödeme ve IBAN*
- K-1056 — Firma yanlışlıkla yaptığı bir iptali düzeltince o iptal için başlatılmış ama henüz gönderilmemiş geri ödeme ve müşteriden istenen IBAN kapanır; müşterinin girdiği IBAN başka bir geri ödemede kullanılmıyorsa silinir.
- K-1057 — Cayılan ürün parça parça geri gelirse her parçanın geri ödemesindeki kupon indirimi ve KDV payı aynı oranla ayrılır, son parça kalan kuruşu alır.
- K-1058 — Müşterinin geri ödeme için girdiği IBAN'ın girildiği an IBAN'la birlikte saklanır ve IBAN silinince silinir.
- K-1075 — İptal edilmiş siparişe geç gelen ödeme karta geri gönderilemezse havaleyle iade yolu açılır; müşteriden IBAN istenir ve müşteriye bildirim gider.
- K-1076 — Teslimden sonra cayılan siparişte kargo ücreti, son ürünleri teslim alan geri ödemeye eklenir; iadesi reddedilen ya da hiç geri dönmeyen ürün için eklenmez.
- K-1077 — Ayıp yüzünden sözleşmeden dönülen ürün adedi sonradan cayma ya da iptale konu olamaz.
- K-1078 — Karta iade isteği yalnız geri ödeme hâlâ geçerliyse ve aynı geri ödeme için açık ya da başarılı başka bir istek yoksa gönderilir; hiçbir siparişe bağlanamayan ödeme bildirimi işaretli kalır, sonradan siparişe bağlanmaz.
- K-1104 — Ayıp yüzünden dönülen adedi olan bir üründe kalan kupon indirimi ve KDV payını, ürünün son adetlerini kapatan işlem alır.
- K-1105 — "Ürün geri dönmedi" kapanışı geri açılınca ona bağlı tutar iadeleri sonraki geri ödemelerden düşülür.
- K-1110 — Müşterinin IBAN'ı, onu kullanan son bekleyen geri ödeme tamamlanana kadar silinmez.
- K-1127 — İade reddinde ve "ürün geri dönmedi" kapanışında müşterinin IBAN'ı, onu kullanan başka bir bekleyen geri ödeme varken silinmez.

*Hesap, e-posta ve güvenlik*
- K-1016 — E-posta, kupon kodu ve stok kodu karşılaştırılırken büyük-küçük harf ve "I, İ, ı, i" farkı yok sayılır.
- K-1080 — Doğrulanmamış bir kayıt, başkasının e-posta değişikliğini geri almasını ya da yeni adresin doğrulanmasını engellemez; o kayıt silinir.
- K-1134 *(bu taramada eklendi)* — Üye e-posta adresini, yalnız doğrulanmamış bir kaydın tuttuğu bir adrese değiştirmek isterse istek reddedilmez; yalnız doğrulanmış başka bir hesabın adresi reddedilir.
- K-1081 — Hesap e-postalarındaki bağlantının gizli değeri hiçbir yerde saklanmaz, e-posta hazırlanırken sunucudaki bir gizli anahtardan yeniden üretilir.
- K-1133 *(bu taramada eklendi)* — Bu gizli anahtar güvenlik şüphesiyle değiştirildiğinde ya da kurtarmada yeniden üretildiğinde e-posta değişikliğini geri alma bağlantısı geçerli kalır; öteki kullanılmamış bağlantılar kapanır.
- K-1082 — Dosya indirme bağlantılarını koruyan anahtarlar veritabanında şifresiz durur; veritabanını okuyabilen biri indirme bağlantısı üretebilir — bu kalan risk olarak yazıldı.
- K-1114 — Sipariş sayfasının erişim anahtarı veritabanında açık durur; veritabanını okuyabilen biri sipariş sayfasının işlemlerini yapabilir — bu kalan risk olarak yazıldı.
- K-1115 — Ulaşmayan bir e-postanın yeniden gönderimi siparişin güncel e-posta adresine gider.
- K-1117 — E-posta değişikliği doğrulanınca hesabın Google ile giriş bağı kalkar.

*Ürün, sepet ve metin*
- K-1018 — Ölçü birimleri kilogram, gram, litre, mililitre, metre, santimetre, metrekare, metreküp ve adettir; birim fiyat 1 kilogram, 1 litre, 1 metre, 1 metrekare, 1 metreküp ya da 1 adet için gösterilir.
- K-1086 — Misafirken verilmiş ve ödemesi beklenen bir siparişin sepeti girişte hesabın sepetiyle birleşirse iki sipariş birbirini iptal etmez; son adette biri ötekini bekletebilir — bu kalan risk olarak yazıldı.
- K-1122 — Firmanın yazdığı uzun metinler gösterilirken de temizlenir; saklanan metin değiştirilmez.

---

## 5. Çakışmalar

### 5.1 Karar isteyen çakışmalar — K-1132, K-1133 ⚠, K-1134 ⚠, K-1135 ⚠, K-1136 ⚠

| Yer | Çakışma | Karar |
|---|---|---|
| `09`'un "Geçiş birimi, iş kuyruğu ve zaman" park satırı ↔ `05 §4.3.4`, `08`'in kardeş satırı (mercek M1c, M1c-4) | Satır varsayılan yeniden denemeyi "işleyicinin kod hatasında" diye sayıyordu; K-1111 bu sözü bilerek bırakmış, ardından K-1130 (2) aynı kolu `08`'in satırına yazmıştı — iki kardeş satır aynı kuralı iki genişlikte söylüyordu | **K-1132 (öneriyle kaydedildi):** satır `05`'in bugünkü sözünü yazar (kod hatası ya da geri alınan veritabanı işlemi; kart iadesinin kaydı yazılmadan düşen denemesi dahil). Elenen: K-1111'in hâlini korumak. Kural değişmez |
| `05 §6.x.20`, §6.x.15, `06 §4.42`, `DEPLOY_RUNBOOK`'un K-1081 satırı ↔ K-1113'ün ilkesi, `02 §3.13.14` (mercek Mc3, Mc3-15) | Türetme sırrının döndürülmesi kullanılmamış bütün hesap bağlantılarının ömrünü çekiyordu — e-posta değişikliğini geri alma bağlantısı dahil; K-1113 aynı soruyu yalnız eski bir ana dönüş için çözmüştü; kuyruktaki geri alma e-postası da yeni sırla eşleşmezdi | **K-1133 (öneriyle kaydedildi — ⚠):** geri alma bağlantısı döndürmede ve kurtarmada açık kalır, kuyruktaki geri alma e-postasının özeti yeni sırla yazılır, kalan risk adıyla. Elenen: `02 §3.13.14`'e istisna · eski sırrı tutmak. Çok kritik değil: ürün kuralı değişmez |
| `02 §3.13.11`, `06 §4.40` ↔ `02 §3.13.7`, K-1080'in Edge'i (mercek Mc3, Mc3-17) | İstek anındaki denetim bekleyen kaydı hesap sayıyor ve değişikliği Z-1 boyunca engelliyordu; K-1080 yan yüzü sonraki adıma devretmiş, devir kapanmamıştı | **K-1134 (öneriyle kaydedildi — ⚠):** yalnız doğrulanmış hesap sayılır. Elenen: bugünkü engel. Geri besleme `02 §3.13.11`, §8.1.4, `03` 9.3.4, `04` 5.25.11, 6.2.25.5 |
| `05 §7.4.5`, §5.2.7, `06 §9.4.1` ↔ K-1080 (1), K-696 (mercek Md, Md-6) | "Her anlık silme uygulama kaydına satır yazar" cümlesi bekleyen kaydın istek içindeki silmelerini de kapsıyordu; K-1080'in geri beslemesi bu silmeyi zamanlayıcının satırına yazmıştı | **K-1135 (öneriyle kaydedildi — ⚠):** iki istek içi silme doğuran işlemin parçasıdır — dönüşle geri gider, satır yazmaz, yeniden uygulanmaz. Elenen: satır yazıp yeniden uygulamak |
| `02 §8.3.6` ↔ K-1027 (5), K-586 (mercek Mc1, Mc1-5) | Belge onay anındaki e-postayı gösterir; kandırılmış düzeltmede erişen kişi gerçek sahibin e-postasını görür — K-1027 bu yüzü tartmamıştı | **K-1136 (öneriyle kaydedildi — ⚠):** kalan risk `02 §8.3.6` ve §10.4.11'e adıyla yazılır; belge onaylanan hâliyle kalır. Elenen: e-postayı maskelemek · güncel değerle göstermek |

**Kuralın yüzleri (betik 1) — K-1134:** "başka bir hesab", "başka bir müşteri hesab", "zaten kayıtlı", "e-posta değişikliği" + "kabul edilmez" — `02 §3.13.11`, §8.1.4, `03` 9.3.4, `04` 5.25.11, 6.2.25.5 güncellendi; `04` 5.21.8 ve 2.12'nin doğrulama anı cümleleri K-1080'le zaten hizalıdır. **K-1136:** "kalan risk" + "kandır" — `02 §8.3.6`, §10.4.11 güncellendi. **K-1133:** "döndürme", "ömrü … çekilir", "geri alma bağlantısı" — `05 §6.x.20`, §6.x.15, `06 §4.42`, `DEPLOY_RUNBOOK`'un K-1081 satırı güncellendi; `06 §9.5.2` ve `05 §8.7.4` eski ana dönüşün istisnasını K-1113'ten zaten taşır. **K-1135:** "anlık silme", "uygulama kaydına", "yeniden uygula" — `05 §7.4.5`, §5.2.7, `06 §9.4.1` güncellendi. **Betik 2:** gövdede anılan yeni K'lar ilgili bölümlerin Kaynak satırlarındadır (`02 §3`, §8, §10; `05 §5.2`, §6, §7.4; `06 §4.40`, §4.42, §9.4).

### 5.2 Hizalanan çakışmalar — geri beslemenin yan yüzleri (mercek Md) ve örneğin evi

Hepsi var olan bir kararın kuralının değişmeden kaldığı yüzlere taşınmasıdır. Üst dokümanların ✓ durumu korunur, kalite döngüsü yeniden açılmaz (K-652). Üst dokümana dönenler `06 §1.3`'ün geri besleme tablosunda satırdır.

| # | Yer | Eski hâl | Karar | Düzeltme | Şiddet |
|---|---|---|---|---|---|
| Md-1 | `02 §12.2.7` | "Kalan ticari kayıt yalnız … taşır" listesi K-1030 (2)'nin genişlettiği listeyi taşımıyordu: kupon kodu, onay anahtarı, iki belgenin kişisel verisiz kopyası, sürüm bağları, onay kutusu kaydı, firmanın havale IBAN'ı ve iade adresi, kalem işlemlerinin ve ödemelerin kayıtları, iki işaret | K-1030 (2), K-1093 | Liste `06 §9.2`'nin değerleriyle tamamlandı; hiçbiri kişiye götürmez (bulgu dosyasının değerlendirmesi) | orta |
| Md-2 | `05 §7.4.3` (3) | Belgenin biçimi "Veri Modeli'nin kararıdır" diye açık duruyordu | K-1027 (4)–(6) | Kişisel verisiz kopya; imha kopyaya dokunmaz | düşük |
| Md-3 | `05 §7.7.3`, §7.4.4, §5.2.8 | Dosya yalnız varyanta bağlıydı; "hiçbir varyanta bağlı olmayan dosya silinir" harfiyen ürüne bağlı dosyayı sildirirdi (M1a'nın bildirdiği) | K-1029 (3)–(4), K-1069 | Dosya ürüne ya da varyanta bağlanır; bağsız dosya silinir | orta |
| Md-4 | `02 §3.9.3`, §4.2 Z-21; `06 §4.11` | Referans fiyatın on günlük penceresinde satışa kapalı aralıkların sayıldığı kural yalnız kararın hücresindeydi (M2a'nın bildirdiği) | K-1023 (2) | Pencere içindeki her fiyat sayılır, kapalı aralıklar dahil | orta |
| Md-5 | `05 §7.4.4` | Gecelik temizliğin imha kaydına giren listesinde yönetici daveti ve geri alma bağlantısının eski adresi yoktu | K-1064 (3), K-1065 (3) | Listeye girdi | düşük |
| Md-6 | `05 §7.4.5`, §5.2.7; `06 §9.4.1` | İstek içi silmenin sınıfı | — | **K-1135** (§5.1) | düşük |
| — | `02 §10.2.5`, §3.10.5 (Mc1-1, Mc1-2) | Yalnız "büyük-küçük harf ayrımı olmadan" | K-1016 | "I, İ, ı, i de tek harf sayılır" ve kuralın evine (`02 §3.3.4`) işaret | düşük |
| — | `05 §4.2.2` (3) (Mc3-18) | "Müşteri kendi ayırdığı stoğa takılmaz" istisnasız | K-1086 | Girişte birleşen misafir sepetinin siparişi istisnadır; kalan risk `02 §3.17.7`'de | düşük |

**Örneğin evi (checkpoint'e devredilen sınıfın etki yansıtmada eklenen örneği).** Kuralın evi **Ürün Gereksinimleri `02 §12.2.7`**'dir: kalan ticari kaydın kapalı listesi ürün kuralıdır — "ürünün içinde kişiye götüren bir alan kalmaz" ölçütü ve anonimliğin koşulu orada yaşar —; alan düzeyindeki liste Veri Modeli `06 §9.2`'nin işidir ve kurala bölüm numarasıyla işaret eder; `05 §7.4.3` (2) `02 §12.2.7`'ye işaret ettiği için düzeltmeyi kendiliğinden alır. Kök: listeyi genişleten K-1030 (2)'nin etki sütunu `02`'yi saymıyordu ve yazımın 2b1 oturumunun geri besleme taraması "kalan ticari kaydın kapalı listesi değişmedi" diye kaydetmişti. **Elenen:** `02`'nin kapalı listesini kaldırıp `06 §9.2`'ye işaret etmek — ürün kuralının kapalı listesi ürün dokümanında kalır, `02`'nin gövdesi `06`'ya işaret etmez. Örnek bu PR'da kapandı; sınıfın kalan riski checkpoint'tedir (§8).

**Terim taraması (betik 1, mercek Md ve uygulama):** kalan ticari kayıt — "kalan ticari kay", "ticari kayıt olarak", "ticari kayd", "kişisel verisiz kay/kayd", "kişiye götür" (28 eşleşme; `02` Z-28, Z-41, §3.15, §12.5, `03` 4.1.31–32, `04`'ün imha edilmiş sipariş ekranı, `05 §7.4.3` (2), `10` KP-29 liste saymaz ya da işaret eder — değişmez) · dosya düzlemi — "hiçbir varyanta", "o anki dosya", "dosya kayd" (23) · referans fiyat — "satışa kapalı", "referans fiyat", "10 gün", "en düşük fiyat" (90; `02` sözlük satırı ve §12.1.6 kısa özettir, çelişmez) · sınıfı ölçüt alan listeler — "on yıl", "beş/dört kayıt", "6563" (152), "imha kaydına", "anlık silme", "gecelik temizli", "Z-44" (147), "anonim" (11), ayıp talebinin silinmesi (22), "Z-29" (12) — hepsi okundu; yeni yüzler §5.3'ün park satırlarında (`06` A4-3, A4-7; `12`'nin on yıllık saklama satırı) · sözlük — Aşama 5'te `02 §1.2`'ye giren yirmi terimin İngilizce adları ve `MeasurementUnit`, `SocialPlatform` değer kodları `06` ile birebir.

### 5.3 Park satırları

**(a) Aşama 1–4'ün park satırları — numaraya dayalı tarama (mercekler M1a, M1b, M1c, M1d).** 108 madde, 319 kaynak; K-977…K-1131'deki **831 eşleşmenin hepsi okundu**; alıntının yetmediği 113 kararın satırı tam okundu. Bulguların çoğu kalite döngüsünün kararlarının **kaynak kararın önceki aşama park satırını güncellememesidir** (checklist §3, "kaynak kararın park satırları da sayılır"; öğrenim adayı 20).

| # | Park satırı | Değiştiren karar | Hizasızlık | Şiddet |
|---|---|---|---|---|
| M1a-1 | `06` A4-3 — imha | K-1092 (1) | Ayıp talebi siparişin imha parçasında silinir diyordu — kendi on yılıyla, ayrı parçada | orta |
| M1a-2 | `06` A4-7 — gelen bildirim kaydı (Mc3-14, Mc1-4 aynı yer) | K-1030 (5), K-1092 (3) | "Siparişe bağlanan satır siparişle silinir" — oluşturulma anından on yıl; matrisin K-954 satırı da | orta |
| M1a-3 | `06` A4-4 — görsel yüklemesi (Mc2-1 aynı yer) | K-1029 (2), K-1069 (3), K-1098 (1) | Kaydedilmeden bırakılan yükleme geçici temizlikte değil, imhanın görsel parçasında; matrisin K-948 satırı da | orta |
| M1a-4 | `06` A4-9 — bağlantı kaydı | K-1062 (1), K-1030 (8), K-1064 (3), K-1065 | Davetin bağlantı kaydı davetle gecelik temizlikte silinir; eski adres boşaltılır | düşük |
| M1a-5 | `06` A4-3 — gecelik imha kaydı listesi | K-1030 (8), K-1064 (3), K-1065 (3) | Yönetici daveti ve eski adres | düşük |
| M1a-6 | `06` A4-2 — siparişin sürümü | K-1029 (4), K-1098 (3) | Sürümü artırmayan sistem yazımları | düşük |
| M1a-7 | `06` A4-4 — dosyanın düzlemi | K-1029 (3)–(4), K-1069 (4) | Dosya ürüne ya da varyanta bağlıdır | düşük |
| M1a-8 | `06` A3-1 — açık adet | K-1024 (2), K-1077; K-1129'un sınıfı | Açık adet dört kayıt türünden okunur; ayıpta dönülen adet düşülür | düşük |
| M1a-9 | `06` A3-3 — sayaçların dönüşü | K-1106 | Stok, gecikme feshi ve kupon hakkının dönüşü | düşük |
| M1a-10 | `06` A4-8 — zaman alanları | K-1109 | IBAN'ın silineceği anın yeniden hesaplanması | düşük |
| M1a-11 | `06` A1-2 — terim sözlüğü | K-1006, K-1009, K-1034 | Teknik kavram listesine dört kayıt; tam liste `06 §3.3` | düşük |
| M1b-1 | `07` A1 — terim sözlüğü (K-17) | K-1036, K-1038 | Alan adları `06 §4`'ten, kayıt düzeyindeki kapalı değerlerin kod adları `06 §5`'ten; iki kararın "`02 §1.1` m4; park gerekmez" öncülü tutmuyordu | orta |
| M1b-2 | `07` A4 — sürümün taşınması (K-873) | K-1014, K-1021, K-1051, K-1063 | Sürüm taşımayan kayıtlar; değere karşı yazılanlar; alanın adı `06 §4`'te | orta |
| M1b-3 | `07` A3 — işlemlerin adedi | K-1077 | "O işleme açık adet" | düşük |
| M1b-4 | `08` A4 — bildirimler ve ödeme sağlayıcısı (K-892) | K-1081 | Bağlantının anahtarı gövde üretilirken türetilir | düşük |
| M1c-1 | `09` A4 — ayarlar ve yönetim komutları | K-1096 | Dönüş denetimi gelen bildirim kaydını da sayar — satır kalsaydı K-1096'nın kapattığı yol açılırdı | orta |
| M1c-2 | `09` A4 — güvenlik kalıpları | K-1081 | Hesap bağlantısının anahtarı saklanmaz, türetilir | orta |
| M1c-3 | `12` A4 — on yıllık saklama (K-951) | K-1092 | Ayıp talebi ve ödemesi alınmış siparişin gelen bildirimi senaryo listesine | orta |
| M1c-4 | `09` A4 — iş kuyruğu | K-1111, K-1130 (2) | **K-1132** (§5.1) | düşük |
| M1c-5 | `09` A1 — terim sözlüğü | K-1038 | Alan adları `06 §4`'ten | düşük |
| M1d-1 | `DEPLOY_RUNBOOK` A4 — güncelleme ve geri alma | K-1096 | Dönüş denetiminin kayıt listesinde gelen bildirim kaydı | orta |
| M1d-2 | `DEPLOY_RUNBOOK` A4 — yedek ve kurtarma | K-1064 | Geri gelen anlık silmelerde yöneticinin kaldırılması | düşük |
| M1d-3 | aynı iki satır | K-1083, K-1113 | Dönüşten sonraki hesap güvenliği adımlarına işaret; kapısız yeniden uygulama | düşük |
| M1d-4 | `DEPLOY_RUNBOOK` A4 — güncelleme turu | K-1121 | Dönüşüm bitmeden tur ilerlemez — Aşama 5 satırına işaret | düşük |
| M1d-5 | karar kaydı K-937, K-987 | K-1068, K-1098 | Durum kaydını yazan zamanlayıcının ayrı kullanıcısı — geri işaret | düşük |

**Değişiklik gerekmeyen adaylar:** mercek dosyalarında `K-kaynak ← K-sonraki` listeleriyle (M1a 200'ü aşkın, M1b ~145, M1c ~245, M1d ~140 eşleşme). `06`'nın 23 park maddesinin matris kapanışı ve 59 `02`/`05` atfı yerinde; dizin maddelerinin sayıları (`06`, `07`, `08`, `09`, `12`, `DEPLOY_RUNBOOK`) ilgili aşamaların §6 tablolarıyla tutuyor; `04` ve `05`'in tüketilmiş bloklarında hizasızlık yok.

**(b) Aşama 5'in park satırları (mercekler M2a, M2b).** 30 madde, 77 karar satırı; betiğin "park gerekmez" dediği on iki çiftin onu doğru — **K-1041 → `12`** (gerekçe "K-1023'ün park satırı kapsar" diyordu, kapsamıyordu) ve **K-1036, K-1038 → `07`, `09`** (öncül "`02 §1.1` m4" teknik kavramlarla sınırlıdır) düzeltildi. Betiğin numara düzeyinde göremediği iki yön kırığı: **K-1124 (3)** `09`'a iş bırakan kolu karar hücresinde taşıyordu (yeni satır açıldı) ve **K-1031** kargo şirketi listesinin içeriğini `08`'e bırakıyordu (etiket ve etki sütunu).

| # | Park satırı | Kaynak karar | Taşınmamış yüz | Şiddet |
|---|---|---|---|---|
| M2a-1 | `08` — K-1078 (2) | K-1078 | Satır kararın elediği seçeneği uygulanacak sonuç diye yazıyordu — tekilliğin kapsamı Veri Modeli'ne döner | orta |
| M2a-2, M2a-3 | `07` — K-1011 | K-1011 (3), K-1068 | Panelde iç anahtar serbest; anahtar türünün tek istisnası | düşük |
| M2a-4 | `07` — K-1100 · K-1118 | K-1100 | Silinmiş parçalı yüklemeye sürdürme isteğinin yanıtı | düşük |
| M2a-5 | `08` — K-1123 | K-1031 | Kargo listesinin içeriği; etiket | düşük |
| M2a-6 | `12` — sayaçların değişmezi | K-1043 (2), K-1042 (2) | Silinmiş varyantın ayırma satırı; panel yazımının koşulu | düşük |
| M2a-7 | `12` — birim fiyat | K-1020 (3), (5) | Birim değişikliği; gösterilen varyantın birim fiyatı | düşük |
| M2a-8 | `12` — tarih aralığı ve referans penceresi | K-1023 (2), (4), K-1041 | Sınır anı, geç yazılan satır, kapalı aralıklar; atıf `06 §2.5.2` | düşük |
| M2a-9 | `12` — kalem kayıtları | K-1131 | IBAN'ın son geri ödemede silinmesi; etiket | düşük |
| M2a-10 | `12` — hesap tarafının silmeleri | K-1083 | Yeniden uygulamanın kapıları ve dönüş denetimi | düşük |
| M2b-1 | `09` — yeni satır | K-1124 (3) | Göç demetinin eklentiyi bildirmesi ve operatör sınıfı — `09`'da hiçbir satırda yoktu (ilk kurulumda göç durur) | orta |
| M2b-2 | `09` — işlem kod adları (K-1067) | `06 §4.63` | İmha fonksiyonunun göçteki yazım kalıbı — karar satırsız devir | düşük |
| M2b-3 | `09` — yeni satır | K-1095; `06 §4.20` | Paylaşımlı kilidin "bilgim tarihli" doğrulaması — doğrulama yeri yazılı değildi | düşük |
| M2b-4, M2b-5 | `09` — türler, sürüm, sayaçlar | K-1089, K-1119 | Kategori yazımının kilidi; sürümü artırmayan yazımların tam listesi | düşük |
| M2b-6 | `09` — metin temizleyicisi | K-1026 | İzin listesi yalnız genişler | düşük |
| M2b-7, M2b-8 | `09` — veri koruma paketi; anahtar | K-1098 (4), K-1068 | Ayrı yerel soket kullanıcısı; UUID kuralının tek istisnası | düşük |
| M2b-9 | `09` — daraltma göçü | K-1121 | Etiketteki K-965 ve K-981'in içeriği satırda yoktu | düşük |
| M2b-10 | `DEPLOY_RUNBOOK` — K-1083 · K-1113 | — | Kaynağı olmayan mekanizma ("uygulama kaydındaki satırlarından okuyup") | düşük |
| M2b-11 | `DEPLOY_RUNBOOK` — K-1081 | K-1081, **K-1133** | Kuyruktaki e-postaların bağlantısı; geri alma bağlantısının istisnası | düşük |

**(c) Park satırı ↔ etki sütunu (iki yönlü betik).** İlk koşum (K-992…K-1131, Aşama 5 blokları): A yönünde on üç parça, hepsi "park gerekmez" — gerekçeleri okundu (yukarıda); B yönü boş. Düzeltmelerden sonra son koşum bütün bloklarda (Aşama 1–5) ve K-977…K-1136'da: etiketteki her kararın etki sütunu hedefi taşır; etki sütunu hedefi anan her karar bir park satırındadır ya da "park gerekmez" der (tek kalan eşleşme K-1132'nin "`08` kalıbı" anmasıdır, devir değildir); park metninde geçen her kararın etki sütunu hedefi taşır. Bu taramada etki sütunlarına giren karar–hedef çiftleri §6.1'de "*bu taramada*" işaretlidir.

### 5.4 Geri işaret, zincirler ve öncüller (mercekler Mc1, Mc2, Mc3)

535 Aşama 1–4 çiftinin ve 91 Aşama 5 içi çiftin hepsi okundu; betik listesinde olmayan üç çift zincir okumasında bulundu (yeni karar eski satırı anmıyordu). Eksik geri işaretler eski satırların etki sütunlarına eklendi (Aşama 1–4 satırları salt okunur — geri işaret tek istisnadır):

| # | Eski satır | Sonraki karar | İlişki |
|---|---|---|---|
| Mc1-1, Mc1-2 | K-982, K-806 | K-1016 | Karşılaştırma "ı" ile "i"yi de birleştirir — genişletir |
| Mc1-3 | K-508 | K-1023 (2) | Referans penceresinde satışa kapalı aralıklar |
| Mc1-4 | K-954 | K-1030 (5) | Ödemesi alınmamış siparişin bildirimi on yılda |
| Mc1-5 | K-586, K-1008 | K-1027 (5), **K-1136** | "Müşteriye görünmez" geçmişin listesi içindir |
| Mc1-Ö1 | K-1025 | K-1030 (8) | Gerekçenin öncülü tutmuyordu — geçersizleşen davet panel listesinden çıkar (K-820) |
| Mc2-1 | K-948 | K-1029 (2), K-1069 (3), K-1098 (1) | Görsel yüklemesinin silinme yeri |
| Mc2-2 | K-889 | K-1050 (5), K-1053 | "Her geçiş geçiş kaydına yazar" siparişin durumuna daralır |
| Mc2-3…Mc2-6 | K-356, K-984, K-531, K-280 | K-1056, K-1044, K-1036 (8), K-1070 (5) | IBAN'ın ek silme anı; birleşmede silinen sepet; Teslim tarihi kapalı liste değil; boş adresin kökü |
| Mc2-7 | K-1054 (3), K-1047 (2), K-1066 | K-1092 | Ayıp talebinin siparişin imha parçasında silindiği öncülü |
| Mc3-1 | K-941 (2), K-893 | K-1078 (4) | Siparişten önce gelen bildirim artık beklemez — kural tersine döndü; K-1078 iki satırı "değişmez" sayıyordu |
| Mc3-2 | K-946 (1), K-891 | K-1111 | "Kendiliğinden yeniden denenmez" gönderilmiş isteğe daralır |
| Mc3-3…Mc3-13 | K-970, K-356, K-1024, K-941 (3), K-925, K-960, K-716, K-789, K-895, K-701, K-1031, K-913, K-1075 | K-1078, K-1111, K-1110, K-1127, K-1131, K-1092, K-1121, K-1108, K-1076, K-1109, K-1102, K-1123, K-1120 | Mercek dosyasındaki ilişki tipleriyle |
| K-1132…K-1136 | K-1111, K-1081, K-1113, K-786, K-1080, K-696, K-955, K-1027, K-586, K-1008 | bu taramanın kararları | §5.1 |

**Zincirler.** IBAN'ın silinme anı (K-1024 (7) → K-1056 → K-1110 → K-1127 → K-1131), saklama (K-941 (3) → K-954 → K-1030 → K-1092), dönüş (K-1083 → K-1096 → K-1113) ve geç gelen ödeme (K-1075 → K-1108 → K-1131): `06` gövdede her yerde zincirin son hâlini yazıyor; iki eski yüz düzeldi — `06`'nın A4-7 park satırı ve matrisin K-954 satırı (Mc3-14), §4.29'un "kalan süresiyle" sözü (Mc3-16). **Öncüller.** 30 ⚠ kararın öncülleri evinde tutuyor; tutmayanlar K-1025'in gerekçesi (geri işaretle), K-1080'in Edge'i (K-1134) ve `05 §4.2.2` (3)'ün istisnasız sonucu (Mc3-18); kalan riskler (K-1082, K-1114) `05` ve `06`'da aynı sözle yazılı. Mc3-19: K-1130'un "`06`'da kararların özet tablosu yoktur" öncülü yalnız kısmen tutar — matrisin K anahtarlı satırları özet yeridir; bayat tek satır düzeldi, sınıf checkpoint'in merceğindedir (§8). **İşlem envanteri:** `06 §4.36`…§4.72'nin andığı 24 işlemin hepsi Kullanıcı Akışları'nda ve Arayüz Tanımları'nda var.

### 5.5 Betikle aranan ve temiz dönen çakışma sınıfları

| # | Sınıf | Sonuç |
|---|---|---|
| 1 | **⚠ listesi ↔ kayıt** | ✓ — 30 / 30 / 30; bu taramayla 34 |
| 2 | **K numarası tekilliği** — çift K taraması | ✓ — boş; K-01…K-1136, boşluk yok |
| 3 | **Karar satırının biçimi** — altı sütun | ✓ — 1.136 satırın hepsi |
| 4 | **Park etiketi ↔ etki sütunu** (bütün bloklar, iki yön) | ✓ — son koşum temiz (§5.3 c) |
| 5 | **`06` park maddelerinin matris kapanışı** | ✓ — 23 / 23 |
| 6 | **Sözlük ↔ `06` adları** (Aşama 5'in yirmi terimi) | ✓ — birebir |
| 7 | **Yalnız doküman numarası taşıyan devir atfı** | ✓ — 0 / 93 |
| 8 | **Gövdedeki yeni K ↔ Kaynak satırı (betik 2)** | ✓ — dokunulan bölümlerde eksiksiz; `06 §1.1.11`'in Kaynak satırı yoktur (matris satırları kaynağı hücrede taşır) |
| 9 | **Değişen dosyalarda CR, kontrol karakteri ve `$[0-9]` artığı** | ✓ — sıfır |

### 5.6 Şüpheci doğrulama

Uygulamadan sonra bir Opus doğrulayıcı diff'i güncel metne karşı okudu (doğruluk, yeni çelişki, biçim; 81 geri işaretin 32'si kaynak kararın hücresiyle tek tek). **12 bulgu — 1 orta, 11 düşük**; hepsi bu PR'da uygulandı, yeni karar istemedi. Temiz dönenler: `02 §12.2.7`'nin yeni listesi `06 §9.2` ile alan alan birebir ve Z-28, `02 §3.23.3`, `05 §7.4.3` ile çelişmiyor; K-1134'ün altı yüzü; K-1023 (2)'nin üç yüzü; sürüm başlıkları.

| # | Yer | Düzeltme | Şiddet |
|---|---|---|---|
| V1-1 | `05 §7.7.3`, §7.4.4, §5.2.8 | Dosyanın silinme ölçütü "hiçbir kalemin indiremeyeceği" değil "sipariş sayfası açılabilen hiçbir kalemin bağı olmayan" (`06 §4.70`, K-1029 (4)) — harfiyen okunursa hakkı dolmuş kalemin dosyası silinir ve hak yenileme yolu (`02 §3.12.6`) kırılırdı | orta |
| V1-2 | `02 §3.3.4` | "I, İ, ı, i" kuralı evine yazıldı — §3.10.5 ve §10.2.5'in işareti döngüdeydi (K-1016) | düşük |
| V1-3, V1-4 | `05 §6.x.15`, `06 §4.42` | K-1133'ün özet yeniden yazımı kurtarmada da, her döndürmede (yalnız sızıntı şüphesinde değil) | düşük |
| V1-5 | `05 §8.7.4` | "Anlık silmeler yeniden uygulanır" uygulama kaydına satır yazan iki silmeye daraldı (K-1135) | düşük |
| V1-6…V1-8 | `05` dipnotu ve v0.24 notu; `06 §4.20` ve v0.25 notu | Fazla parantez, karar sayısı, çift K-1095 | düşük |
| V1-9, V1-10 | karar kaydı K-1132, K-1089, K-1119 | Etki hücresindeki park etiketi `09`'daki etiketle | düşük |
| V1-11 | `02 §10.4.2`, §10.4.11 | "Müşteriye görünmez" geçmişin listesine daraldı; ilk değer belgede görünür (K-1136) | düşük |
| V1-12 | `05 §7.4`'ün Kaynak satırı | K-696, K-1080 (betik 2) | düşük |

---

## 6. Devir dizini — Aşama 5 kararlarının sonraki dokümanlara devirleri

**Tek ev:** devrin evi karar satırının etki sütunu ya da park satırıdır; bu dizin devri ikinci kez kaydetmez. **Aşama 5'te yalnız numara taşıyan atıf yoktur.** Her hedef dokümanın "Aşama 5'ten park edilen girdiler" bloğu bu bölüme işaret eden bir dizin satırı taşır (bu taramada eklendi). **API tasarımı aşamasının açılışı** devir taramasını (checklist §4) buradan ve önceki aşamaların dizinlerinden başlatır; izlenebilirlik matrisi API tasarımı aşamasında zorunludur (`00 §C.1`) — devir dizinleri ve park satırları matrisin kaynak envanterine girer (Aşama 3'te `04 §1.1.1`, Aşama 5'te `06 §1.1.11` böyle girdi).

- **Önceki aşamaların dizinleri:** `PHASE1_CONFLICT_SCAN.md` §6, `PHASE2_CONFLICT_SCAN.md` §6, `PHASE3_CONFLICT_SCAN.md` §6, `PHASE4_CONFLICT_SCAN.md` §6.
- **`11` Uygulama Planı:** Aşama 5'ten devir yok — `11` tüketicidir ve `06`'yı kendi matrisinde okur; `06`'da `11`'e iş bırakan cümle yoktur.
- **`DEFERRED_BACKLOG.md`:** Aşama 5'ten yeni kalem yok.
- **Toplandığı yer (6. adım, 2026-10-18):** Aşama 6'ya devrin bütün girdileri — bu dizin dahil — karar kaydının §15'inde tek tabloda.
- **Süreç kararları** (K-992…K-1005, K-1009, K-1032, K-1033, K-1037, K-1045, K-1060, K-1125, K-1126, K-1129) sonraki dokümana iş bırakmaz.

### 6.1 Etki sütunlarındaki devirler (K-992…K-1136)

| Hedef | Atıf | Karar | Aşama 5 park bloğunda | Önceki aşamanın park satırında (güncellendi) | Park gerekmez |
|---|---|---|---|---|---|
| `07` API Tasarımı | 16 | 16 | 6 | 7 | 3 (K-1034, K-1062, K-1066) |
| `08` Entegrasyon Spesifikasyonu | 10 | 10 | 7 | 2 | 1 (K-1036) |
| `09` Kodlama Kılavuzu | 34 | 34 | 26 | 5 | 3 (K-1034, K-1044, K-1088) |
| `12` Doğrulama Protokolü | 22 | 22 | 21 | 1 | 0 |
| `DEPLOY_RUNBOOK.md` | 11 | 11 | 9 | 2 | 0 |
| `11` Uygulama Planı | 0 | 0 | — | — | — |
| **Toplam** | **93** | **70 karar** (93 karar–doküman çifti) | **69** | **17** | **7** |

İşin adı etki sütunundan ve park satırının etiketinden alınmıştır; "park satırı" ve tarih ekleri kısaltıldı. *Bu taramada* işaretli çiftlerin etki sütunu girdisi ya da park satırı bu PR'dadır (§5.3).

#### `07` API Tasarımı — 16 karar

| Karar | İş | Park satırı |
|---|---|---|
| K-1011 | iç anahtarın adreste kullanımı ve sahiplik denetimi; panelde iç anahtar serbest — *bu taramada genişledi* | Aşama 5 — K-1011 · K-1068 |
| K-1014 | sürümün taşınması — sürüm taşımayan kayıtlar, alanın adı — *bu taramada* | Aşama 4 — K-873 (güncellendi) |
| K-1021 | geçici kapatma anahtarının istekte taşıdığı değer — *bu taramada* | Aşama 4 — K-873 (güncellendi) |
| K-1034 | teknik kavramların kod adlarının `06 §3.3`'ten alınması | park gerekmez (`02 §1.1` m4) |
| K-1036 | kayıt düzeyindeki kapalı değerlerin ve kimlikli listelerin kod adları `06 §5`'ten — *bu taramada* | Aşama 1 — K-17 (güncellendi) |
| K-1038 | sözlükte terimi olmayan alanların adlarının `06 §4`'ten alınması — *bu taramada* | Aşama 1 — K-17 (güncellendi) |
| K-1051 | iç notun gösterilen metni | Aşama 4 — K-873 (güncellendi) |
| K-1062 | oturum, bağlantı ve yeniden doğrulamanın uçları | park gerekmez — alanlar `06 §4`'te |
| K-1063 | kurumsal içeriğin elle sırasının istekte taşıdığı değer — *bu taramada* | Aşama 4 — K-873 (güncellendi) |
| K-1066 | gönderim kaydının alanları | park gerekmez — alanlar `06 §4.58`'de |
| K-1068 | anahtar türünün istisnası (veri koruma tablosu) — *bu taramada* | Aşama 5 — K-1011 · K-1068 |
| K-1070 | öneklerin birbirinden ve sabit sayfaların adreslerinden ayrıklığı | Aşama 5 — K-1070 |
| K-1077 | işlemlerin adedi — "o işleme açık adet" — *bu taramada* | Aşama 3 — K-787 (güncellendi) |
| K-1100 | formun kaydının hata yanıtı; parçalı yüklemenin sürdürme yanıtı — *bu taramada genişledi* | Aşama 5 — K-1100 · K-1118 |
| K-1118 | yükleme türüne göre silinme yolu | Aşama 5 — K-1100 · K-1118 |
| K-1130 | ekrandaki hâlin yeri (`04` 2.12.8.7) | Aşama 5 — K-1100 · K-1118 |

#### `08` Entegrasyon Spesifikasyonu — 10 karar

| Karar | İş | Park satırı |
|---|---|---|
| K-1027 | B-1'in gövdesindeki iki metnin kopyadan ve onay anındaki değerlerden üretilmesi | Aşama 5 — K-1027 |
| K-1031 | kargo şirketi listesinin içeriği ve kod adları (K-142'nin devri) — *bu taramada* | Aşama 5 — K-1031 · K-1123 |
| K-1036 | `DispatchType`'ın değerleri bildirim kimlikleridir | park gerekmez |
| K-1059 | gelen bildirimin ve kart iadesi isteğinin alan eşlemesi | Aşama 5 — K-1059 · K-1112 |
| K-1078 | tek başarılı tahsilat, iade sonucunun işlem kimliği; tekilliğin kapsamı Veri Modeli'ne döner — *bu taramada düzeldi* | Aşama 5 — K-1078 |
| K-1081 | hesap bağlantısının anahtarı gövde üretilirken türetilir — *bu taramada* | Aşama 4 — K-892 (güncellendi) |
| K-1111 | varsayılan deneme geri alınan veritabanı işleminde de | Aşama 4 — K-970 (etki yansıtmada güncellendi) |
| K-1112 | ödeme kaydının tek sağlayıcı varsayımı ve dönüş yolu | Aşama 5 — K-1059 · K-1112 |
| K-1120 | sonradan bildirilen geri dönmenin veri yeri | Aşama 5 — K-913 · K-1120 |
| K-1123 | kargo şirketi kod adlarının kalıcılığı | Aşama 5 — K-1031 · K-1123 |

#### `09` Kodlama Kılavuzu — 34 karar

| Karar | İş | Park satırı |
|---|---|---|
| K-1010 | ilişkilerin silme davranışının açıkça yazılması | Aşama 5 — K-1010 · K-1038 |
| K-1011 | anahtarın uygulamada üretilmesi; tek istisna | Aşama 5 — K-1011 · K-1012 · K-1017 |
| K-1012 | adlandırma kuralının çatıda uygulanışı | Aşama 5 — K-1011 · K-1012 · K-1017 |
| K-1013 | para ve yüzdenin kodda karşılığı | Aşama 5 — türler, sürüm, sayaçlar |
| K-1014 | sürümün artırılmasının çatıdaki kalıbı | Aşama 5 — türler, sürüm, sayaçlar |
| K-1015 | sayaçların ve türetilmiş sütunların tek yazım yolu | Aşama 5 — türler, sürüm, sayaçlar |
| K-1016 | normalleştirme işlevinin tek yeri | Aşama 5 — türler, sürüm, sayaçlar |
| K-1017 | enum'un metne dönüşümü | Aşama 5 — K-1011 · K-1012 · K-1017 |
| K-1021 | bölüm başına sürüm sütununun çatıdaki eşlenişi | Aşama 5 — K-1021 |
| K-1026 | izinli etiket temizleyicisi ve bağlantı adreslerinin kuralı; izin listesi yalnız genişler — *bu taramada genişledi* | Aşama 5 — K-1026 · K-1122 |
| K-1027 | kopyanın üretimi ve yer tutucuların doldurulması tek işlevde | Aşama 5 — K-1027 |
| K-1034 | teknik kavramların kod adlarının `06 §3.3`'ten alınması | park gerekmez |
| K-1035 | anahtar halkasının kalıcılığının doğrulanması | Aşama 5 — K-1035 · K-1068 · K-1082 · K-1098 |
| K-1036 | kod adları `06 §5`'ten — *bu taramada* | Aşama 1 — K-17 (güncellendi) |
| K-1038 | kısıtlayıcı bağın çatıdaki eşlenişi; alan adları `06 §4`'ten — *bu taramada genişledi* | Aşama 5 — K-1010 · K-1038; Aşama 1 — K-17 |
| K-1040 | kategorinin düzeyinin tek yazım yolu | Aşama 5 — türler, sürüm, sayaçlar |
| K-1044 | anahtar özetinin yöntemi | park gerekmez (`05 §6.4`'ün kalıbı) |
| K-1047 | sürümün tablo düzeyindeki listesi | Aşama 5 — türler, sürüm, sayaçlar |
| K-1067 | işlemlerin kod adlarının listesi; imha fonksiyonunun göçteki yazım kalıbı — *bu taramada genişledi* | Aşama 5 — K-1067 |
| K-1068 | veri koruma paketinin anahtar türünün ve adlandırmasının doğrulanması; durum kaydını yazan kullanıcının yetkisi | Aşama 5 — K-1035 · K-1068 · K-1082 · K-1098 |
| K-1070 | sadeleştirme işlevi ve boş sonucun kökü | Aşama 5 — K-1070 |
| K-1081 | hesap bağlantısı anahtarının türetilmesi — *bu taramada* | Aşama 4 — güvenlik kalıpları (güncellendi) |
| K-1082 | halkanın şifresizliğinin ve kalıcılığının doğrulanması, salt okuma yetkisi | Aşama 5 — K-1035 · K-1068 · K-1082 · K-1098 |
| K-1088 | kilit `05`'in mekanizmasıdır | park gerekmez |
| K-1089 | kategori yazımının danışma kilidi — *bu taramada* | Aşama 5 — türler, sürüm, sayaçlar |
| K-1095 | paylaşımlı kilidin kalıbı ve kilit uyuşmazlığının doğrulanması — *bu taramada* | Aşama 5 — K-1095 (yeni) |
| K-1096 | dönüş denetiminin kayıt listesi — *bu taramada* | Aşama 4 — ayarlar ve yönetim komutları (güncellendi) |
| K-1098 | durum kaydını yazan ayrı yerel soket kullanıcısı — *bu taramada* | Aşama 5 — K-1035 · K-1068 · K-1082 · K-1098 |
| K-1111 | varsayılan deneme veritabanı hatasında da | Aşama 4 — iş kuyruğu (güncellendi, K-1132) |
| K-1119 | sürümü artırmayan sistem yazımları — *bu taramada* | Aşama 5 — türler, sürüm, sayaçlar |
| K-1121 | daraltma göçünün ön denetimi | Aşama 5 — K-925 · K-1121 |
| K-1122 | gösterimde aynı izin listesiyle süzme | Aşama 5 — K-1026 · K-1122 |
| K-1124 | göç demetinin eklentiyi bildirmesi, dizinin operatör sınıfı — *bu taramada* | Aşama 5 — K-1124 (yeni) |
| K-1132 | varsayılan deneme — *bu taramada* | Aşama 4 — iş kuyruğu (güncellendi) |

#### `12` Doğrulama Protokolü — 22 karar

| Karar | İş | Park satırı |
|---|---|---|
| K-1015 | sayaçların değişmezi | Aşama 5 — sayaçların değişmezi |
| K-1018 | birim fiyatın hesabı ve gösterimi | Aşama 5 — birim fiyat |
| K-1019 | birim fiyatın senaryoları | Aşama 5 — birim fiyat |
| K-1020 | birim fiyatın senaryoları; birim değişikliği ve gösterilen varyant — *bu taramada genişledi* | Aşama 5 — birim fiyat |
| K-1023 | tarih aralığının sınırlarının ve referans fiyat penceresinin senaryoları; kapalı aralıklar — *bu taramada genişledi* | Aşama 5 — K-1023 · K-1041 |
| K-1027 | kopyanın ve gösterimin aynılığı; düzeltilmiş siparişte onay anındaki değer | Aşama 5 — K-1027 |
| K-1041 | kesintide geç yazılan satırın sınır anı — *bu taramada* | Aşama 5 — K-1023 · K-1041 |
| K-1042 | sayaçların değişmezi | Aşama 5 — sayaçların değişmezi |
| K-1043 | iki düzeltmenin ve kontenjansız hizmetin senaryoları | Aşama 5 — sayaçların değişmezi |
| K-1054 | ayıp talebinin geçmişi | Aşama 5 — kalem kayıtları |
| K-1056 | düzeltmenin senaryoları | Aşama 5 — kalem kayıtları |
| K-1057 | parça parça geri ödemenin toplamı | Aşama 5 — kalem kayıtları |
| K-1064 | hesap tarafının silmeleri | Aşama 5 — K-1064 · K-1065 · K-1083 |
| K-1065 | eski adres | Aşama 5 — K-1064 · K-1065 · K-1083 |
| K-1076 | kargo ücretinin değişmezi | Aşama 5 — kalem kayıtları |
| K-1077 | dönülen adedin değişmezi | Aşama 5 — kalem kayıtları |
| K-1079 | düzeltmenin değişmezi | Aşama 5 — kalem kayıtları |
| K-1083 | kaldırmanın yeniden uygulanmasının kapıları ve dönüş denetimi — *bu taramada* | Aşama 5 — K-1064 · K-1065 · K-1083 |
| K-1091 | silinmiş varyantın kalemi | Aşama 5 — sayaçların değişmezi |
| K-1092 | ayıp talebinin ve gelen bildirimin on yılı — *bu taramada* | Aşama 4 — on yıllık saklama (güncellendi) |
| K-1104 | ayıpta dönülmüş adedi olan kalemde cayma geri ödemesinin tavana takılmaması | Aşama 5 — kalem kayıtları |
| K-1131 | düzeltmede IBAN'ın silinme anı — *bu taramada etikete girdi* | Aşama 5 — kalem kayıtları |

#### `DEPLOY_RUNBOOK.md` — 11 karar

| Karar | İş | Park satırı |
|---|---|---|
| K-1016 | veritabanının UTF-8 ve Türkçe ICU sıralamasıyla açılması | Aşama 5 — K-1016 · K-1087 |
| K-1031 | §A — veritabanı tarafının fail-fast denetimi; §H — ilk yönetici, operatör hesabı yok | Aşama 5 — K-1031 |
| K-1064 | geri gelen anlık silmelerde yöneticinin kaldırılması — *bu taramada* | Aşama 4 — yedek, kurtarma ve prova (güncellendi) |
| K-1081 | türetme sırrının envanteri, üretimi ve döndürmesi | Aşama 5 — K-1081 · K-1133 |
| K-1083 | dönüşten sonraki hesap güvenliği adımları; K-955 ve K-956'nın satırları buraya işaret eder — *bu taramada* | Aşama 5 — K-1083 · K-1113 |
| K-1087 | sıralama sürümünün denetimi | Aşama 5 — K-1016 · K-1087 |
| K-1096 | dönüş denetiminin saydığı kayıtlar — *bu taramada* | Aşama 4 — güncelleme ve geri alma (güncellendi) |
| K-1113 | dönüşten sonraki hesap güvenliği adımları | Aşama 5 — K-1083 · K-1113 |
| K-1121 | çok adımlı turda dönüşüm taşıyan adım | Aşama 5 — K-960 · K-1121 |
| K-1124 | eklentinin ilk kurulumdaki adımı | Aşama 5 — K-1124 |
| K-1133 | döndürmenin ve kurtarmanın adımı — geri alma bağlantısının istisnası — *bu taramada* | Aşama 5 — K-1081 · K-1133 |

### 6.2 Veri Modeli'nin gövdesinde sonraki dokümanı anan cümleler

Mekanik çıkarım (`06 §2`–§9; başlık notu ve §1 hariç): sonraki dokümanı numarasıyla ya da adıyla anan **45 madde**. Betik her maddenin hedef dokümanın Aşama 5 park bloğunda bölümüyle anılıp anılmadığını denetledi (37 anılıyor); anılmayan 8 madde tek tek okundu (mercekler M2a, M2b).

| Hedef | Madde | Park satırında (betik) | Okunarak: başka bir park satırı | Karar satırı olmadan devredilen |
|---|---|---|---|---|
| `07` | 7 | 5 | 2 — §3.3 ve §5.4.3 → Aşama 1'in K-17 satırı (K-1034 ve K-1036 "park gerekmez") | — |
| `08` | 9 | 8 | 1 — §8.4.1 (kargo listesinin içeriği) → K-1031 · K-1123 satırı | — |
| `09` | 19 | 15 | 4 — §3.3, §5.4.3 → K-17 satırı; §4.56 → K-1027 satırı; §8.3.2 → K-1035 · K-1068 · K-1082 · K-1098 satırı | **iki — bu taramada park satırı aldı:** imha fonksiyonunun göçteki yazım kalıbı (`06 §4.63` → K-1067 satırı) ve paylaşımlı kilidin "bilgim tarihli" doğrulaması (`06 §4.20` → yeni K-1095 satırı) |
| `12` | 1 | 1 | — | — |
| `DEPLOY_RUNBOOK` | 9 | 8 | 1 — §8.2.3 → Aşama 5'in K-1031 satırı (veritabanı tarafı) ve Aşama 4'ün K-922 satırı (ayar tarafı) | — |

**Sonuç:** karar satırı olmadan devredilen iş kalmadı. `06`'daki "bilgim tarihli" işaretlerinin dördü de Kodlama Kılavuzu'nun bir park satırında doğrulanır (§4.68'in üçü K-1035 · K-1068 · K-1082 · K-1098 satırında, §4.20'ninki K-1095 satırında).

### 6.3 Doğrulama ve soru devirleri

Veri Modeli "bilgim tarihli" notuyla dış dünyaya ait olguları Kodlama Kılavuzu'na bırakır (konvansiyon 12); bunlar devir değil doğrulamadır ve evleri park satırlarıdır (§6.2). **Çok kritik soru adayı yoktur.** Teknik Mimari'nin bıraktığı aday — desteklenen ödeme sağlayıcısının sayısı — Entegrasyon spesifikasyonları aşamasının açılışındadır (`08`'in K-932 satırı); Veri modeli aşaması ona dokunmadı (K-1112 ödeme kaydının tek sağlayıcı varsayımını `08`'in Aşama 5 satırına yazdı — aynı açılışın girdisi).

---

## 7. Öğrenim terfisi adayları — 5. adımın girdisi

Metinler tracker §14.1'dedir; bu dizin onları tek yerde gösterir.

| # | Aday | Kaynak | Durum |
|---|---|---|---|
| 1–19 | Açılıştan etki yansıtmaya kadar kayda düşenler (§14.1) | Aşama 5 | Bekliyor |
| 20 | **Kalite döngüsünün kararları kaynak kararın önceki aşama park satırını güncellemedi** — kural checklist §3'te yazılı ("kaynak kararın park satırları da sayılır"), yine de Aşama 1–4'ün park satırlarında yirmi beş hizasızlık; etki yansıtmanın iki yönlü betiği yalnız Aşama 5 bloklarını taradı | **Bu tarama** (§5.3 a) | Bekliyor |
| 21 | **Kararın kolu başka dokümana işi karar hücresinde bırakıyor, etki sütununa yazmıyor** — iki yönlü betik numarayı etki sütununda arar; K-1124 (3) ve K-1031'in kolları ancak okumayla bulundu | **Bu tarama** (§5.3 b) | Bekliyor |
| 22 | **"Park gerekmez" gerekçesinin öncülü sınanmıyor** — K-1036 ve K-1038'in "`02 §1.1` m4", K-1041'in "K-1023'ün satırı kapsar" öncülleri tutmuyordu; betik "park gerekmez" sözünü görünce çifti kapatır | **Bu tarama** (§5.3 b) | Bekliyor |
| 23 | **Geri beslemenin taraması "değişmedi" diye yanlış kaydedebilir** — 2b1 "kalan ticari kaydın kapalı listesi değişmedi" dedi; K-1030 (2)'nin listesi `02`'ninkinden genişti ve etki sütunu `02`'yi saymıyordu | **Bu tarama** (§5.2, Md-1) | Bekliyor |
| Ö-23 | Checklist'in skill'e dönüştürülmesi | Aşama 1'den | Bekliyor — kapı Aşama 5'in öğrenim terfisi |

**Bu taramanın gözlemi — kurala kanıt:** Aşama 4'ün öğrenim terfisinin iki kuralı bu aşamada işledi — park satırı etiketi ile etki sütununun iki yönlü eşleşmesi Veri Modeli'nin kalite döngüsünde ve etki yansıtmada koştu ve bu taramanın ilk koşumunda Aşama 5 bloklarında B yönünü boş buldu; işlem envanteri öncülü (checklist §3) `06`'nın andığı yirmi dört işlemin hepsini ürün dokümanlarında buldu. Kaçan sınıf öncekidir: kural yazılıyken kalite döngüsünün geri beslemesi kaynak kararın önceki aşama park satırına uzanmadı (aday 20).

---

## 8. Checkpoint'e notlar

- Tracker §4'te açık satır yok, A-19 açılmadı; Veri Modeli'nde, Ürün Gereksinimleri §13'te ve öteki dokümanlarda açık kalem yok — checkpoint'in açık karar kontrolü bu raporla karşılanır.
- ⚠ listesi **34 karar** (§4.1) — hazır metin aynı yerde; arşiv işaretinden önce proje sahibine tek mesajda gösterilir.
- **Devredilen sınıf** (§14.1, K-1129): iki yerin metin farkı — özet hücresi ya da genel tanım cümlesi ile ayrıntının kuralı — checkpoint'in iç tutarlılık merceğindedir. Etki yansıtmanın eklediği örnek (`02 §12.2.7` ↔ `06 §9.2`) bu taramada kapandı (ev `02 §12.2.7`, §5.2). Bu taramanın kanıtları aynı sınıftadır: park satırlarının 46 bulgusu park metni ile kararın son hâlinin farkıdır; matrisin iki K anahtarlı satırı (K-954, K-948) özet hücresinin eskimesidir — Mc3-19'a göre checkpoint matrisin K anahtarlı satırlarını özet yeri sayar; `05 §7.7.3` ile `06 §4.70`'in farkı ve `05 §7.4.5`'in genel cümlesi (K-1135) aynı sınıfın üst doküman yüzüdür.
- K-1132…K-1136 bu taramanın kararlarıdır; yüzleri §5.1'dedir. Checkpoint'in iç tutarlılık merceği K-1133'ün (`05 §6.x.20`, §6.x.15, `06 §4.42`, `DEPLOY_RUNBOOK`'un K-1081 satırı) ve K-1135'in (`05 §7.4.5`, §5.2.7, `06 §9.4.1`) yüzlerini bir kez daha okuyabilir.
- Aşama 1–4'ün park satırları K-977…K-1131'e, Aşama 5'inkiler bütün kollarıyla bir kez tarandı (831 eşleşme; 30 madde). Checkpoint yeniden taramaz; yalnız bu PR'ın değiştirdiği satırları okur.
- 6. adımda API tasarımı aşamasına devrin tablosu bu raporun §6'sını ve hedef dokümanların park bloklarındaki dizin satırını gösterir; öğrenim adayları dizini (§7) 5. adımın girdisidir.

## Retro Güncelleme — 2026-10-18

Aşama 5'in checkpoint'i (`CP05_PHASE5_CHECKPOINT.md` §5.1) §4.1'in hazır metninde iki cümlenin kararın kapsamından dar olduğunu buldu. §4.1'in metni değiştirilmedi; liste 2026-10-18'de §4.1'in cümleleriyle gösterildi — bu iki düzeltilmiş cümle arşiv işaretinden önce checkpoint'in üç ⚠ kararıyla birlikte yeniden gösterilir:

- K-1092 — Ayıp talebi son değiştiği andan on yıl saklanır ve siparişin kişisel bilgilerinden önce silinmez; ödemesi alınmış siparişe gelen ödeme bildirimi de geldiği andan on yıl saklanır ve siparişin kişisel bilgilerinden önce silinmez.
- K-1135 *(bu taramada eklendi)* — Doğrulanmamış bir kaydın, yeni e-posta adresinin doğrulanması, "bu değişikliği ben yapmadım" bağlantısıyla e-posta değişikliğinin geri alınması ya da Google ile ilk giriş sırasında silinmesi, veritabanı yedekten eski bir ana dönerse o işlemle birlikte geri gelir ve ayrıca yeniden uygulanmaz.

Gerekçe: K-1092 (1) ve (3) iki kaydı da siparişin imha parçasından önce sildirmez — saklama iki sürenin geç olanıdır; K-1135 silmenin üçüncü anını — e-posta değişikliğinin geri alınması (K-1080 (1)) — sayar. Sayım değişmez: 34 karar.

**Sonuç (6. adım, 2026-10-18; K-1142):** iki düzeltilmiş cümle checkpoint'in üç kararıyla birlikte arşiv işaretinden önce gösterildi (Soru 6); itiraz yok.

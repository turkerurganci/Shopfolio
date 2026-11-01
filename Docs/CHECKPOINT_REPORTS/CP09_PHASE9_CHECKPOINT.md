# Aşama 9 — Checkpoint (CP09)

**Tarih:** 2026-11-01 | **Aşama:** 9 — Implementation Planı | **Kapanış adımı:** K-437'nin 4. adımı (`/checkpoint`; `checklists/document-stage.md` §7)
**Girdi sürümleri:** karar kaydı v0.281 · Implementation Planı (`11`) v0.52 · Ürün Gereksinimleri (`02`) v0.108 · Kullanıcı Akışları (`03`) v0.46 · Arayüz Tanımları (`04`) v0.50 · Teknik Mimari (`05`) v0.43 · Veri Modeli (`06`) v0.45 · API Tasarımı (`07`) v0.37 · Entegrasyon Spesifikasyonları (`08`) v0.20 · Kodlama Kılavuzu (`09`) v0.28 · MVP Kapsamı (`10`) v0.59 · `12` ve `DEPLOY_RUNBOOK` (Aşama 9 park blokları) — `origin/main` @ `7be455a`
**Çıktı sürümleri:** karar kaydı v0.282 · `11` v0.53 · öteki dokümanlar değişmedi
**Girdi raporu:** [Aşama 9 — Çakışma Taraması](PHASE9_CONFLICT_SCAN.md) (K-437'nin 3. adımı) — açık kalem durumu ve altı geri işaret oradan alındı; K-1492 (5)'in devrettiği sınıf bu raporun iç tutarlılık merceğinde arandı. Audit, deep review, on bir cross-review turu, hedefli mercekler ve etki yansıtmanın hükme bağlanmış bulguları ikinci kez açılmadı; RET edilmiş iddialar yeniden getirilmedi.

> **Ne işe yarar:** Aşama kapanmadan önce Implementation Planı'nın (`11`) kendi içinde ve üst dokümanlarla tutarlı olduğunu, özellikle task bloklarındaki kabul kriteri ve doğrulama kontrol listesi (KK) maddelerinin kaynak cümleyi yanlış okutmadığını doğrular; bulunan her uyumsuzluğu ya kapatır ya da adıyla sonraki adıma bırakır. Sonuç öğrenim terfisinin (5. adım) ve arşiv işaretinin (6. adım) girdisidir.

---

## Checkpoint Sonucu — 2026-11-01

**Aşama:** 9 — Implementation Planı (kapanış sırası K-437: yazım turu ✓ · kalite döngüsü ✓ · çakışma taraması ✓ · **checkpoint** · öğrenim terfisi · arşiv işareti)
**Genel durum:** ⚠ → ✓ Dikkat gerektiren noktalar bu PR'da kapandı — 10 Sonnet / low mercek `11`'in tamamını 11 dilimde okudu ve **78 ham bulgu** (77 ayrı) getirdi; her biri Opus / high doğrulayıcıdan geçti: **41 doğru**, **23 kısmen doğru**, **14 çürütüldü**. Doğrulanan 63 ayrı bulgunun hepsi bu PR'da `11` v0.53'e uygulandı (124 şartname çifti). Doğrulayıcı sınırı (20) aşılmadı: 17 doğrulayıcı kullanıldı. Kapsam açıkları ve doğrulayıcıların yan gözlemleri (10 madde) `KP-checkpoint-2` adımına devredildi. Yeni karar: K-1495 (yeni kural kararı yok; ⚠ yok). Çok kritik konu yok.

### Kontrol Özeti

| # | Kontrol | Sonuç | Detay |
|---|---|---|---|
| 1 | Yol haritası | ✓ | Aşama 9'dayız: yazım turu (Y1…Y8), kalite döngüsü (audit, deep review, 11 cross-review turu, hedefli mercekler, etki yansıtma K-1493) ve çakışma taraması (#284, K-1494) bu sırayla kapandı; atlanan adım yok. |
| 2 | Doküman durumu | ✓ | Betikle: `02` v0.108, `03` v0.46, `04` v0.50, `05` v0.43, `06` v0.45, `07` v0.37, `08` v0.20, `09` v0.28, `10` v0.59 — başlık, alt bilgi ve tracker §1 birebir. `11` v0.52 → v0.53 (başlık, alt bilgi, tracker §1; ⏳ — arşiv adımında ✓). `12` şablon ⬚ (park blokları açık); `DEPLOY_RUNBOOK` sürüm alanı taşımayan operasyon dokümanıdır. |
| 3 | Tutarsızlık | ⚠ → ✓ | **Devredilen sınıf** (K-1492 (5): task bloğunda kabul kriterinin ya da KK'nin, kaynak cümlenin yanlış okunabilen kısaltması olması) `11`'in tamamında arandı (§3.1); 63 yerde doğrulandı. K-1456…K-1494'ün etki sütunlarında "checkpoint" geçen başka sınıf yok (yalnız K-1492 (5); K-1493 ve K-1494 aynı devri anar, yeni sınıf eklemez). En ağırları: T222 KK5'in kaynağın istisnasını tersine sınaması, T380 k2'nin "her satır eski ve yeni değeri taşır" demesi, T515 k8'in kapı kapalıyken mevcut hesabın Google girişini de reddeder gibi okunması, T538 KK4'ün modelsiz işleyiciyi koşulsuz reddetmesi. |
| 4 | Açık kararlar | ✓ | Tracker §4'te açık satır yok, A-19 açılmadı (çakışma taraması §3); dokümanlarda açık kalem yok. Bu adım yeni açık karar bırakmadı; 10 madde `KP-checkpoint-2`'ye devredildi (§5). |
| 5 | Aşama çıktıları | ✓ | `11` yazıldı (214 task, izlenebilirlik matrisi 7.1 ve 7.2, boşluk raporu, Post-MVP parkı); `12` ve `DEPLOY_RUNBOOK` park blokları var (çakışma taraması §6). Kapılar: matris kapsama, task blok denetimi (--tam), yazım kapanışı, geri yön — sorun 0. |
| 6 | Geriye dönük etki | ✓ | 63 düzeltmenin hepsi `11`'in kaynak dokümanlarla hizalanmasıdır; üst dokümanda kural değişmedi, `02`–`10`'a dokunulmadı. Kaynak kararların etki sütunlarına `(checkpoint — CP09 …)` işareti eklendi (§3.3). |
| 7 | Yeni alan/kural | ✓ | `11`'e giren her yeni cümle kendi kaynağında tanımlı: T104 `Image` genişlik ve yükseklik (`06 §4.69`), T214 üç yönlendirme kodu (`07 §6.3.1`), T216 KK4 sınama değerleri (`09 §7.2` madde 6), T405 sorgu aralığı (K-1238 (3)), T548 bekleme dizisi (K-1354), T206 / T212 yönlendirme istisnaları (`07 §2.2.2`, §2.7.1). Yeni alan, parametre ya da enum değeri eklenmedi. |

### Aksiyon Gerektiren Maddeler

- [x] 63 doğrulanan bulgu `11` v0.53'e uygulandı — 49 task bloğu ve 13 matris / geri yön satırı (§3.1)
- [x] Yeni karar: K-1495 (düzeltmelerin kaydı; toplu onayla kaydedildi; ⚠ yok)
- [x] Dokunulan kararların etki sütununa `(checkpoint — CP09 …)` işareti eklendi (§3.3)
- [ ] **`KP-checkpoint-2`** — doğrulanmayan 10 madde (kapsam açıkları ve yan gözlemler, §5): yeni mercek ve doğrulayıcı; çıktı `CP09_PHASE9_CHECKPOINT.md`'ye eklenir
- [ ] ⚠ listesinin proje sahibine gösterilmesi — 8 karar (K-1411, K-1413, K-1432, K-1458, K-1463, K-1474, K-1476, K-1483); kapı: arşiv işaretinden önce, tek mesajda (`INSTRUCTIONS.md` §2). Bu adım ⚠ kararı eklemedi (8 / 8)
- [ ] Sırada öğrenim terfisi (K-437'nin 5. adımı) — §6'nın adayları

### Notlar

**Yöntem.** Mekanik kontroller betikle koştu: sürümler, karar kaydı K tekilliği, K-1456…K-1494'ün etki sütunlarında "checkpoint" taraması, `11` kapıları (`11_matris_kapsama.py`, `11_task_blok_denetle.py --tam`, `11_yazim_kapanis.py`, `11_geri_yon.py . --denetle`, `geri_isaret_tara.py`). Anlamsal tarama `11`'i 80 KB'a kadar **11 dilime** (satır 1–774, 775–1678, 1679–2555, 2556–3349, 3350–4223, 4224–5095, 5096–5820, 5821–6473, 6474–7115, 7116–7726, 7727–7761; ilk yedisi ağırlıkla task bloğu, sonraki dördü matris, boşluk raporu ve Post-MVP parkı) bölüp **10 salt okuma merceğiyle** (iki dilim bir mercekte) ve iki dalgada (5 + 5) koştu. Mercek, dilimini baştan sona okudu ve kriteri / KK'yi kaynak cümleyle (`02`–`10`; karar kaydı değil) karşılaştırdı. Ham bulgu dilim başına: 10, 14, 3, 4, 16, 9, 9, 1, 3, 7, 2 = **78** (77 ayrı: 05-13 ile D9-01 aynı yer). Doğrulama kademeliydi: Orta altı doğrulayıcıda (2–4'lü; bir tane tek), Düşük on bir doğrulayıcıda (5–8'li) — **17 doğrulayıcı** (Opus / high), sınır 20. Doğrulayıcı talimatında kapsamı daraltan ifade yoktu; her bulgu tam doğrulandı. Hükümler ESKİ → YENİ şartnameye çevrildi (beş Sonnet / low dönüştürücü), şartname betikle (`11_audit_uygula.py`) uygulandı: her ESKİ dosyada tam bir kez, çakışan aralık yok; tek çakışma (05-13 ile D9-01'in T386 şartnamesi) doğrulayıcı hükmüyle çözüldü (05-13 uygulandı, D9-01'in şartnamesi atıldı). **Model dağılımı:** mercekler Sonnet / low (10); doğrulayıcılar Opus / high (17); şartname dönüştürücüler Sonnet / low (5).

**Seviye dağılımı.** Mercek: Orta 12, Düşük 66. Doğrulayıcı sonrası: **Orta 4** (T222 KK5, T380 k2, T515 k8, T538 k4 / KK4), Düşük 59 (altı Orta bulgu Düşük'e indi, ikisi çürüdü). Yüksek bulgu, mevzuata aykırılık ya da para / hak kaybı yok.

**Mercek kapsamı.** Üç mercek bulgularını taramanın sonunda tek seferde yazdı (talimat bulunduğu anda ekle diyordu); dilim 4, 6 ve 9 mercekleri kaynak karşılaştırmasını örneklemeyle yaptı ve bunu bildirdi; dilim 10–11 merceği bağlam sınırında "BÖLÜNDÜ" dedi. Bu açıklar §5'te `KP-checkpoint-2`'ye devredildi.

**Süreç olayları.** Bekçi: ajan çağrılarında engel olmadı (sınır 17 / 20). Yürütücüde bir kez, `python -I -` ile betik çalıştırma denemesi heredoc kuralı nedeniyle engellendi (komut çalışmadı); betik Write ile dosyaya yazılıp çalıştırıldı. Mercekler ve doğrulayıcılar repoya dokunmadı.

**Betikle doğrulanıp tutarlı çıkanlar:** `11` kapıları (sorun 0; 214 = 214 geri yön eşitliği, `--yaz` sonrası); karar kaydı K tekilliği ve altı sütun (K-1495 dahil, tekrar yok); değişen dosyalarda CR, kontrol karakteri ve `$[0-9]` artığı sıfır; blok sınırı: en büyük blok 2 560 bayt, aşan yok.

---

## 3. Bulgular ve kapanışları

Seviye: **orta** — iki yer çelişiyor; kapalı liste, sayı ya da hedef yanlış; ya da kuralın evinde kural eksik; **düşük** — ifade, kapsam ya da atıf farkı. Hüküm: **D** doğru · **K** kısmen doğru. Bulgu kimlikleri çalışma dosyalarınındır; `11` yerleri task ve kriter numarasıyla verilir. Seviye doğrulayıcının verdiğidir.

### 3.1 Doğrulanan ve uygulanan bulgular (63)

| # | Yer (`11`) | Bulgu | Kapanış | Hüküm / seviye |
|---|---|---|---|---|
| B1-01 | T103 k5, KK5 | Yayın anı "birlikte dolu" diyordu; taslak (ikisi boş), tür sınırı ve tür başına tek taslak düşmüştü; T119 k6'nın iki taslağı kriterin harfine takılıyordu | Taslak koşulu ve tür sınırı eklendi | D / düşük |
| B1-02 | T104 k7 | `Image` "boyutu" bayt gibi okunuyordu; kısıt piksel cinsinden en ve boy | Genişlik ve yükseklik, 2560 piksel | D / düşük |
| B1-04 | T110 k4 | İl bağı iki geçmişe birden yüklenmişti; e-posta geçmişinde il sütunu yok | Adres geçmişinde iki il bağı | K / düşük |
| B1-05 | T112 k3, KK3 | Kargo ücreti işaretinin yalnız dört türde doğru olabileceği kısıt düşmüştü | Kısıt kriterde ve KK3'te | K / düşük |
| B1-06 | T109 k6 | Düzeltme alanlarının "yalnız orada dolu" yönü bir yöne indirgenmişti | İki yönlü cümle | D / düşük |
| B1-09 | T08 KK1 | Salt okuma parolasının alıcıları yalnız okuma komutu gibi yazılmıştı | Üç alıcı ve veritabanı ilk kurulumu | K / düşük |
| B1-10 | T02 k2 | `wwwroot/` atfı `09 §4.1` madde 2'de değil, madde 4'te | "madde 2, 4" | K / düşük |
| B02-1 | T206 k7, KK7 | "Yazan POST 303, yazmayan yönlendirmez" iki istisnayı atıyordu: dışa aktarmanın 200'ü ve kapanmış oturumun 303'ü | İstisnalar eklendi (`07 §2.2.2`, §2.7.1) | D / düşük |
| B02-2 | T222 k5, KK5 | KK5 kaynağın istisna saydığı oturum son işlem anı ve hız sınırı sayacının değişmemesini sınıyordu (kaynağın tersi) | k5 ve KK5 istisnalarla yazıldı | D / **orta** |
| B02-3 | T230 k9 | "Yalnız iç kimlik" `kind` ve `command` alanlarını yasaklıyor gibi okunuyordu | "iç kimlikle yazılır, alan değeri taşımaz" | D / düşük |
| B02-4 | T204 k7 | "Hiçbir süre servise bağlı değildir"; ödemede süre işler, iptal ertelenir (`08 §3.3`) | Kapsam servislere bölündü | K / düşük |
| B02-6 | T209 KK1 | Giriş gibi her gönderimde yeni değer üreten işlemler kuralın dışında; KK1 bunu söylemiyordu | KK1 | K / düşük |
| B02-7 | T212 k3 | 303 ve 200 hâlleri (`07 §2.7.1` aynı paragrafta) alınmamıştı | k3 | D / düşük |
| B02-8 | T214 k6, KK6 | Beş kodun üçü (`InvitationSent`, `ContactRequestClosed`, `ContactRequestReopened`) `11`'de yoktu | Kodlar ve KK6 | D / düşük |
| B02-9 | T213 k5, KK5 | Forma özgü kuralın kendi kodu olduğu düşmüştü (K-1198 (2)) | k5 ve KK5 | D / düşük |
| B02-10 | T208 k8, KK8 | "Çift tıklama" yöneticiyi söylemiyordu; müşterinin açık adedi T512 / T513'te | "yönetici gönderimi" | D / düşük |
| B02-11 | T232 k5, KK5 | "Anahtar halkada yoksa" `Invalid` hâli düşmüştü | k5 ve KK5 | D / düşük |
| B02-12 | T216 k4, KK4 | Geniş ayrıştırmada `12.50` 1250 olur; kaynak bu tuzağa karşı (`09 §7.2` madde 3, 6) | Kabul edilen biçim ve sınama değerleri | D / düşük |
| B02-13 | T119 k8 | Seed olmayanların yedi kaleminden üçü yazılmıştı | Tam liste | D / düşük |
| CP3-1 | T303 k9 | "Adı … ad ile e-posta içermez": ad iki anlamda (komutun adı / kişinin adı); "istisna düşmüş" iddiası çürüktü | İfade ayrıldı | K / düşük |
| CP3-2 | T329 k4 | "Ödeme bildirim ucu Ö1'i işler" T311 k7 ve `07 UC-80` ile (uç durumu yazmaz, kuyruk işi işler) sözcük sözcük çelişiyordu | "uçta kaydedilir, Ö1 kuyruk işinde işlenir" | K / düşük |
| CP3-3 | T332 k7, KK7 | "Asgari varyant" kaynakta yok ve yayın kapısıyla (en az bir canlı varyant) ters okunuyordu | "asgari sipariş adedi yoktur" | D / düşük |
| CP4-1 | T338 k5 | %100 sınırı sabit kupona bağlı okunuyordu | Sabit / yüzdesel ayrıldı | K / düşük |
| CP4-3 | T353 k5, KK5 | Yalın "bir kez" K-685'in ayırdığı yeniden başlayan süreyi geri getiriyordu | "süre başına" ve atıf | K / düşük |
| CP4-4 | T355 k4 | Dondurulan onay kutularının "dijital ve hizmet kalemi için işaretlenen" niteleyicisi düşmüştü (b kolu çürük) | Niteleyici | K / düşük |
| 05-01 | T368 k4 | Her durumda "İptal edildi" diyordu; kargodaki kalemde sipariş dönüşteki kapanışla kapanır (K-592) | k4 | D / düşük |
| 05-02 | T368 k5 | "Süre" hangi süre belli değildi (geri ödeme on dört günü); düğme açıkken koşulu düşmüştü | k5 | D / düşük |
| 05-03 | T368 k6 | "Taahhüt süresi" iki süreyi (kargoya verme, ifa) saydığı tanımı yitirmişti | k6 | K / düşük |
| 05-04 | T372 k4 | "Beyan ekseni" diye eksen yok; kapanış kuralı yolunu kapatan öneri reddedildi | "son açık kalemde kapanış kuralı işler" | K / düşük |
| 05-05 | T372 k2 | K-838'in kargodaki kalem dalı (teslim alınmayan mal firmaya döner) `11`'de yoktu | k2 | D / düşük |
| 05-08 | T375 k8, KK8 | Silinmiş hesap istisnası kriterde yoktu | k8 ve KK8 | D / düşük |
| 05-09 | T380 k2, KK2 | "Her satır eski ve yeni değeri taşır"; `02 §10.3.1` yalnız kim, ne zaman, ne değişti ister; eski–yeni değer yalnız ayar satırlarında | k2 ve KK2 | D / **orta** |
| 05-10 | T382 k8 | IBAN her durumda isteniyor gibiydi; kaynakta yalnız havale açıksa | k8 | K / düşük |
| 05-11 | T384 k4 | "Kargoya verme tarihi" sütununun hizmet ve dijital kalemde taşıdığı yazmıyordu (K-546) | Parantez | D / düşük |
| 05-12 | T386 k2 | "Kargodan sonra" üç kalem tipine de bağlanabiliyordu (K-577) | Fiziksel kargodan, dijital ve hizmet teslim işaretinden | D / düşük |
| 05-13 ≡ D9-01 | T386 k9, KK9 | İade malının ulaşma tarihinin düzeltilmesi (K-712) F3'te hiçbir kriterde yoktu (F5'te T530 k5 sonucunu sınıyor) | k9 ve KK9 | K / düşük |
| 05-14 | T390 KK6 | KK6 ikinci geri ödemenin açılmadığını sınamıyordu | KK6 | D / düşük |
| 05-15 | T403 k6, KK6 | Ayraçsız tam lira tutar (`10`) kriterin harfinde tanınmıyordu (`08 §2.1.6`, `09 §14.2`) | k6 ve örnekler | D / düşük |
| 05-16 | T405 k7 | Dört sayı, zaman aşımından iki dakika sonraki sorguyu düşürüyordu (K-1238 (3)) | k7 | K / düşük |
| CP6-1 | T515 k8 | "Eşleşme yok ya da kapı kapalı 303 → `/giris`": koşullar bağımsız okunuyor, kapı kapalıyken mevcut hesabın Google girişi de reddedilir gibiydi; k7, KK3 ve `04` 5.22.10 ile çelişiyordu | "kapı kapalıyken yeni hesap gerekirse" | D / **orta** |
| CP6-2 | T507 k10 | Satış kapısının dört koşulundan yalnız biri "kapıyı açar" diye yazılmıştı | "koşuludur" | D / düşük |
| CP6-3 | T520 k6 | Boş e-postada 422 düşmüştü | k6 | K / düşük |
| CP6-4 | T522 k5 | "Davet geçerli değilse" (kullanılmış, dolmuş, geri çekilmiş) | k5 | D / düşük |
| CP6-6 | T529 k2 | IBAN onu kullanan son geri ödemeyle silinir (K-1110) | k2 | D / düşük |
| CP6-7 | T516 k10 | "Google tanımsızsa" 422 sebebi düşmüştü | k10 | D / düşük |
| CP6-8 | T523 k9 | UC-180'in 429'u eksikti | "bekleyen yoksa 422, 429 (L-9)" | K / düşük |
| B07-1 | T538 k4, KK4 | KK4 "modelsiz uç testi kırar" koşulsuzdu; `07 §5.2.4`'ün on iki işleyicisi girdi okumaz (K-1196) | "girdi okuyup modelsiz bağlayan uç" | K / **orta** |
| B07-2 | T540 k8 | "İkisinin verisini" belirsizdi; çerçevesiz E-29, E-30, E-54 kimlikle yazıldı | k8 | K / düşük |
| B07-3 | T545 k3 | Referans fiyat indirimli fiyatın yanındadır; fiyat satırının altındakiler yalnız tarihler | k3 | D / düşük |
| B07-4 | T537 k6 | Platform imzasının "firma kapatmadıysa" koşulu düşmüştü | k6 | D / düşük |
| B07-5 | T540 k5 | "Ödeme adımı dışında" istisnası düşmüştü | k5 | D / düşük |
| B07-6 | T535 k9 | Açma ucunda doğru durum "zaten açık" (K-1194 (1)) | k9 | D / düşük |
| B07-7 | T547 k5 | "Taslak" etiketi ve E-05 istisnası düşmüştü | k5 | D / düşük |
| B07-9 | T548 k9 | K-1354'ün 1, 2 ve 4 saniyelik beklemeleri yoktu | k9 | D / düşük |
| D9-02 | Matris 7.1.5 `02:Z-35` | Satır yalnız T370'i gösteriyordu; "mal dönmedi" kapanışı ve iade reddinde IBAN silme T373'te (k10, k8) | `T370, T373` | D / düşük |
| D9-03 | Matris 7.1.5 `02:§8.1.4` | T517 (e-posta değişikliği reddi) ve T509 (ödeme adımı hatırlatması) satırda yoktu | İkisi eklendi | D / düşük |
| B10-1 | Matris 7.1.5 `10:KP-51` | "En çok üç sistem içi adım" her sipariş için tavan gibi okunuyordu; K-454: karışık siparişte hatlar toplanır | Tek tipli / karışık ayrımı | K / düşük |
| B10-2 | Matris `10:KD-28` | "Ürünlerin ek satış akışı" upsell gibi okunuyordu | Kaynağın tamlaması | D / düşük |
| B10-3 | Matris `10:KD-55` | Ödeme çerçevesinin zorunlu çerezleri istisnası adda yoktu | "tek istisna" kalıbı | D / düşük |
| B10-4 | Matris `10:KP-73` | B10-3 ile aynı kök (K-518) | Ad hücresi | D / düşük |
| B10-5 | Matris `10:KD-38` | "Ürün ve kurumsal" niteleme ve duyuru tarih aralığı istisnası düşmüştü | Ad hücresi | D / düşük |
| B10-6 | Matris `10:KD-26` | "ya da ile göre" bozuk cümle | Ad hücresi | D / düşük |
| B11-2 | §8 GAP-17 satırı, aday tablosu T232 | "İstisnasız" "her durumda" gibi okunuyordu | İki yerde ifade | K / düşük |

**Bayt payı.** Düzeltmelerin çoğu aynı blokta anlam kaybı olmadan kısaltma gerektirdi (T368 ve T372'de pay 1–3 bayt kaldı; T540 k5, T548 k9 sınırda). Toplamda **124 şartname çifti** betikle, **iki elle kısaltma** (T386 KK1 ve KK6; çakışan iki şartnamenin birleşmesinden doğan yedi baytlık aşım) uygulandı. Sonuç: en büyük blok 2 560 bayt, aşan yok.

### 3.2 Çürütülenler (14)

| # | Bulgu | Gerekçe |
|---|---|---|
| 05-06 | T372 k6 / KK6 ayrı onay ve bir yıl sınırı | KK6'nın sınadığı, k6'nın işaret ettiği `02 §10.4.10`'un kendi cümleleridir; `11` konvansiyon 5 (K-1445 (3)) ayrıntının kritere kopyalanmamasını ister; 3. tur M61 RET'inin dönüşü |
| 05-07 | T374 k8 / KK8 tutar sınırı | Aynı gerekçe (`02 §5.5`); `Dokümanlar` yan gözlemi §5'te |
| B1-03 | T114 k4 bitiş tarihi | "Bitiş başlangıçtan önce olamaz" iki tarihin dolu olduğunu varsayar; kısıt boş tarihte kendiliğinden geçer |
| B1-07 | T102 k3 atıf | `09 §10.1` madde 3'ün devamı istisnayı açıkça yazar |
| B1-08 | T07 KK6 | `05 §7.6.6` iki fiil kullanır (siler / durdurur); KK6 K-1488 (3) uygulamasıdır |
| B02-5 | T215 KK8 sınır ↔ limit | Blok iki sözcüğü tutarlı ayırır (`07 §2.9.7`) |
| B02-14 | T201 k7 süreyle silinmez | Kaynakla uyumlu; kişisel alanlar T201 k1 ve k6 üzerinden T301'e bağlı |
| CP4-2 | T353 k2 / KK2 | Müşteriye özgü kısıt zaten yazılı; sayım eksenleri T377 k1 / k5, T376 k8, T215 k7 |
| CP6-5 | T512 k9 kargo ücreti | `02 §7.2.9` ve K-789 ile uyumlu: bir adet bile kargoya verildiyse kargo ücreti iade edilmez |
| CP6-9 | T520 k5 muafiyet | Yöneticinin limitlerden muaf olmaması (T377 k6); tarayıcı istisnası T377 k9'un ortak kuralı |
| B07-8 | T544 k8 "iki yasal kutu" | `02 §3.17.2` ve K-833 ile uyumlu |
| B08-1 | T534 aday tablosu satırı UC-117 | Tablo her task'ın hangi kaynak birimden doğduğunu kaydeder; T534'e bağ K-1440 (4)'ten gelir ve matriste / T534 bloğunda var |
| B11-1 | §9 madde 1 "64 satır" | "Adayların kaynağı olan" ifadesi `10 §3`'ü niteler; K-1454 (1)'in birebir yansıması |
| B10-7 | Matris `10:KP-1` "boş kategori" | Kaynak dokümanların kendi terimi (`02 §3.28.6`, `04` §5.2.9) |

### 3.3 Etki işaretleri

Düzeltmelerin dayandığı ve `11`'e yansıttığı kararların etki sütununa `(checkpoint — CP09, 11 Txxx; K-1495)` işareti eklendi: K-454, K-546, K-577, K-592, K-685, K-696, K-706, K-712, K-838, K-850, K-971, K-1110, K-1171, K-1194, K-1195, K-1196, K-1198, K-1202, K-1238, K-1333, K-1354, K-1480. Aşama 8 ve öncesi salt okunur kararlara bu ölçüde dokunuldu: işaret yalnız etki sütununa girer, karar ve gerekçe hücreleri değişmez (K-652 mekanizması).

---

## 4. Kontrol 6 — sonraki aşamanın girdileri

Aşama 9'un çıktısı `11` son doküman değildir: sırada `12` (Doğrulama Protokolü, Aşama 10) vardır; `DEPLOY_RUNBOOK` implementation sırasında doldurulur.

| Girdi | Durum |
|---|---|
| `11` v0.53 | ⏳ → ✓ arşiv adımında (K-437'nin 6. adımı); öğrenim terfisi sonrası |
| `12` ve `DEPLOY_RUNBOOK` park blokları | "Aşama 9'dan park edilen girdiler" blokları var (çakışma taraması K-1494); checkpoint yeni iş bırakmadı |
| `DEFERRED_BACKLOG` | Değişmedi; D-01 ilk implementation task'ından önce, task karşılığı F0'dadır (T02–T07, T09) |

---

## 5. Doğrulanmayanlar — `KP-checkpoint-2`'ye devredilen 10 madde

Bu adım 17 doğrulayıcı kullandı (sınır 20). Aşağıdakiler bulgu olarak hüküm beklemiyor; mercek kapsam açıkları ve doğrulayıcıların yan gözlemleridir. `KP-checkpoint-2` yeni mercek koşar, çıkan bulguları doğrulatır ve bu rapora ekler.

**Kapsam açıkları (yeni mercek koşar):**

1. **Dilim 4 (T335–T367, satır 2556–3349):** 33 task bloğu okundu, kaynak cümleleri seçili bölümlerde açıldı (4 bulgu); kalan kriterlerin kaynakla karşılaştırması yapılmadı.
2. **Dilim 6 (T501–T532 civarı, satır 4224–5095):** kaynak doğrulaması `07`'den ~40 UC ile örneklendi; `02`, `04`, `05`, `06`, `08` cümleleri açılmadı. Örneklemeye girmeyenler: T501–T506, T508, T509, T513, T524–T528, T530, T532'nin bazı kriterleri.
3. **Dilim 9 (matris 7.1.5 ikinci yarısı, satır 6474–7115):** `05`, `09`, `10`, devir ve P satırlarına bakılmadı.
4. **Dilim 11 (§8 boşluk raporu, §9):** mercek bağlam sınırında bölündü; yapılmayan üç kontrol: GAP-15'teki `CommandHost`, "sıralama sürümü denetimi", T303 / T389 / T222 / T233 blokları.
5. **Dilim 1 ve 5:** ham bulgu sayıları yüksek (10 ve 16); örnekleme ek kontrol gerektiriyor.

**Doğrulayıcıların yan gözlemleri (bulgu sayılmadı):**

6. **T366 çerez politikası — ödeme sağlayıcısı çerezi yer tutucusu:** `02 §12.2.5` ve K-518'e göre çerez politikası taslağı ödeme sağlayıcısının çerezleri için bir yer tutucu taşımalı; `11`'in hiçbir bloğunda kabul kriteri olarak yok (T366 ~2 556 bayt; ekleme kısaltma ister).
7. **T374 `Dokümanlar` alanında `02 §5.5` yok:** tutar sınırı yalnız orada yazılı; 3. turun M62 RET'i "kural §7.5.3'te eksiksiz" demişti, doğrulayıcı §7.5.3'ün sınırı söylemediğini bildirdi.
8. **T380 k3:** kişisel veri istisnası olarak misafir e-posta düzeltmesi sayılmıyor (eklenirse blok sınırı aşılır).
9. **T513 k1:** kargodaki kalem dalı açık yazılmıyor (kaynağı `Dokümanlar`'da).
10. **T515:** `SocialLoginFailed` hâli (Google'a ulaşılamadı ya da kullanıcı vazgeçti) kriterlerde açıkça geçmiyor.

---

## 6. Öğrenim terfisine notlar — 5. adımın girdisi

| # | Aday | Kaynak | Playbook |
|---|---|---|---|
| 1 | Doğrulayıcı talimatı ESKİ / YENİ şartname biçimini (`ESKI:` ve `YENI:` satırları, `## #N | hukum: dogru`) zorunlu kılmazsa hükümler serbest yazılır ve yeniden biçimlendirme için ek ajan (beş Sonnet / low) gerekir; format `11_audit_uygula.py`'nin girdisidir | Bu adım; Aşama 9 audit'inde talimat biçimi vardı | `audit` skill'i "Koşum biçimi" 2. madde; `checkpoint` skill'i "Koşum" paragrafı |
| 2 | Aynı bulgu iki ayrı dilimden gelirse (05-13 ≡ D9-01) iki doğrulayıcı aynı yeri ayrı şartnameyle düzeltir; betik çakışmayı yakalar ama biri atılmalıdır. Aşama 8'de aynısı üç çiftte oldu | Bu adım; CP08 §6 tek seferlik gözlem 1 | Tekrar etti: paketleyici (`scripts/doc-scans/`) doğrulama öncesi aynı yer alıntısını eşleyip birleştirsin (L5) |
| 3 | Bulgu kümelerinde Orta bulgu oranı yüksek görünür ama doğrulayıcı sonrası altısı Düşük'e iner (12 → 4); mercek seviyesi kaba. Kademe (Orta 2–4'lü) bu yüzden masraflı | Bu adım | Tek seferlik gözlem (raporda kalır) |
| 4 | 80 KB'lık dilim bir Sonnet / low mercek için kaynak karşılaştırmasını örneklemeyle bırakıyor (dilim 4, 6, 9); kapsam açığı doğrulanmayan listeye düşüyor. Dilim ≤ 40 KB ya da dilim başına kaynak aile sınırı ölçülebilir | Bu adım; §5 madde 1–4 | `checkpoint` skill'i "Koşum" (dilim büyüklüğü) |

**Tek seferlik gözlemler (raporda kalır):** (1) Üç mercek bulgularını taramanın sonunda yazdı; "bulduğun anda ekle" kuralına uyulmadı. (2) Bir doğrulayıcı kendi paket başlığındaki sayı farkını bildirdi (V08: "Toplam: 10 bulgu", dosyada beş; V17: "7 (1 orta, 6 düşük)", dosyada altı düşük) — paketleyicinin alıntı başlığı kaynak dosyadaki sayıyı taşıyordu, bulgu kaybı olmadı. (3) Doğrulayıcılar mercek önerisinin metnini bayt sınırı nedeniyle çoğunlukla yeniden yazdı; mercek önerileri 2 560 baytı aşıyordu (en az 14 yerde).

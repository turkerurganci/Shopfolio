# Aşama 2 — Öğrenim Terfisi

**Tarih:** 2026-10-04 | **Aşama:** 2 — Kullanıcı Akışları | **Kapanış adımı:** K-437'nin 5. adımı (`00 §K`; `checklists/document-stage.md` §7 madde 5)
**Girdi:** karar kaydı v0.69 — §8.1'in on iki öğrenim adayı (Ö-23 dahil), Aşama 2'nin süreç kararları (K-647…K-718), [çakışma taraması](PHASE2_CONFLICT_SCAN.md) §4 ve §7, [checkpoint](CP02_PHASE2_CHECKPOINT.md) §6, `Docs/PLAYBOOK_FEEDBACK.md` (PF-32, PF-36…PF-48), yöneticinin koşum gözlemi, proje hafızası (`.claude/memory/`) ve kullanıcı hafızasının on bir notu — `origin/main` @ `c0aa7bf`
**Çıktı:** karar kaydı v0.70 · `.claude/checklists/document-stage.md`, `.claude/INSTRUCTIONS.md`, `.claude/GUARDRAILS.md`, `.claude/skills/audit/`, `checkpoint/`, `cross-review/`, `.claude/memory/MEMORY.md` ve yeni `MEMORY_ARCHIVE.md` · `Docs/PLAYBOOK_FEEDBACK.md` 53 satır · karar K-719 — Proje Vizyonu, Ürün Gereksinimleri, MVP Kapsamı ve Kullanıcı Akışları değişmedi

> **Ne işe yarar:** `00 §K` gereği aşama, öğrenimi yazılmadan kapanmaz. Bu rapor Aşama 2 boyunca kayda ve hafızaya düşen her öğrenim adayının sonucunu yazar: terfi etti mi, hangi katmana ve dosyaya, metni ne. L3–L5 hedefli terfiler bu raporun PR'ında uygulandı; `00` hedefli iki öneri GUARDRAILS §2 gereği uygulanmadı ve tam metinleriyle §4'te proje sahibinin onayını bekliyor.

---

## 1. Sonuç

| Sonuç | Sayı | Adaylar |
|---|---|---|
| **Terfi etti — bu PR'da uygulandı** (L3, L4) | 16 | Ö-26…Ö-35 · K-648 (biçimi) · K-652 · yazım konvansiyonları (K-669, K-670, K-682, K-700…K-702) · K-718 · Ö-36 · Ö-37 |
| **Hafızadan terfi etti** (notun süreç parçası) | 3 | H-1 (ikinci vaka), H-9 (aşamaların adı), H-10 (merge yetkisinin biçimi) |
| **Önceden terfi etmişti** | 3 | Ö-25 (tracker başlığı ve checklist §7 — K-647, PR #56) · K-647 · K-649 (PR #58) |
| **Raporda ya da dokümanda kalır** | 2 | K-650, K-651, K-689 (Kullanıcı Akışları §0.2'de yaşar) · Ö-39 (tek seferlik) |
| **`00 §N` önerisi** — desen | 1 | Ö-38 (ve öteki adayların desen yüzleri — §4, Öneri 1) |
| **Bekletildi** — kapısıyla | 1 | Ö-23 — kapı: Aşama 3'ün öğrenim terfisi (§3) |
| **Hafızada kalır** — kişisel tercih, referans ya da süreli yetki | 8 | H-2, H-3, H-4, H-5, H-6, H-7, H-8, H-11 |
| **Proje sahibinin onayını bekliyor** (`00`) | 2 öneri | §4: `00 §N.1`'e Aşama 2'nin altı deseni · `00 §C.7`'ye skill ölçütü (ve sürüm v1.0.5) — **2026-10-04: ikisi de reddedildi (K-722); `00` v1.0.4 kalır** |

Toplam otuz dört aday: karar kaydının §8.1'inden on iki, süreç kararlarından yedi satır, bu adımda bulunan dört, kullanıcı hafızasından on bir not. `CLAUDE.md` ve `SETUP.md` için terfi önerisi yok: CLAUDE.md'nin oturum başlangıcı ve katman kuralı aşamanın hiçbir öğrenimiyle çelişmiyor; SETUP'ın parametreleri değişmedi.

**Aşamanın asıl öğrenimi.** Aşama 2'nin kalite döngüsü dokümanı kendi içinde temizledi (cross-review üç turda TEMİZ), ama geri beslenen dokümanlarda kuralın öteki yüzlerini ve Kaynak satırlarını checkpoint buldu (otuz sekiz bulgu). Kaçağın kökü Aşama 2'nin kendi süreç kararlarındaydı: geri beslemenin işletimi (K-652), çok oturumlu yazımın geçici bağları (K-670, K-682) ve merge yetkisinin biçimi (K-648) yalnız karar kaydında yaşıyordu; bunlar Aşama 3'te de işleyecek. Hepsi bu PR'da checklist'e, talimatlara ve skill'lere taşındı; geri besleme maddesi iki betikle (kuralın anahtar terimi, Kaynak satırı) birlikte yazıldı.

---

## 2. Adaylar ve sonuçları

**Kimlikler:** Ö-25…Ö-35 karar kaydı §8.1'in adaylarıdır (Aşama 1'in Ö numaralandırması sürer; 8. aday Aşama 1'den gelen Ö-23'tür). Ö-36…Ö-39 bu adımda bulunan adaylardır. H-1…H-11 kullanıcı hafızasının notlarıdır — Aşama 1 raporunun numaralandırmasıyla aynı.

### 2.1 Karar kaydındaki adaylar (§8.1)

| # | §8.1 | Aday | Sınıf (`00 §K`) | Sonuç | Hedef katman ve dosya | Uygulanan metin (özet) | Playbook |
|---|---|---|---|---|---|---|---|
| Ö-25 | 1 | Şablonun karar kaydı başlığı iki okumaya açık — arşiv aşama sonunda değil dönem sonunda | Kural + şablon kusuru | **Önceden terfi etmişti** (PR #56) | Tracker başlığı · L4 checklist §7 (5. ve 6. adım) | Değişiklik yok; şablon başlığı playbook'a gider | PF-36 (Uygulandı) |
| Ö-26 | 2 | Devir notundaki soru şablona karşı taranmadan soruldu — aynı ihlalin ikinci vakası (ilki 2026-08-22) | İhlali önleyen kural | **Terfi etti** | L4 checklist §1 · L3 `INSTRUCTIONS.md` §2 | Checklist §1'e "Devir notunu kurala karşı oku": devir notu bir aşama kaydıdır, şablonun üstünde değildir; her sorusu proje sahibine gitmeden önce L1–L5'e karşı taranır, cevap şablondaysa soru sorulmaz. `INSTRUCTIONS §2`'nin "Soru açmadan önce kural katmanlarını tara" maddesine: devir notunun taşıdığı soru da kapsamdadır | PF-37 (Uygulandı) |
| Ö-27 | 3 | Kullanıcı Akışları şablonunun iki eksiği — anlatı bölümü yok, açık kararlar bölümü yok | Kural (L4) + şablon kusuru | **Terfi etti** (çevre kural; şablon değişmedi) | L4 checklist §1 | "Şablonu tara": çıktı dokümanının şablonunda açık kararlar bölümü ve şablonun kendi istediği bölümler var mı; eksik konu planına plan önerisi olarak girer, şablonun numaraları değişmez, eksik playbook listesine yazılır. Aynı eksik iki aşamada üç şablonda çıktı (PF-22, PF-40) | PF-40 (Uygulandı — çevre kural) |
| Ö-28 | 4 | Aşama planının konu kimliği aşamayı taşımıyor | İhlali önleyen kural | **Terfi etti** | L4 checklist §1 | "Konu kimliği aşamaya özgü bir önek taşır"; aday önek önceki aşamaların ve dokümanların kimlik ailelerine karşı betikle taranır (kanıt: önerilen `B2-..` Aşama 1'in Blok 2'siydi) | PF-41 (Uygulandı) |
| Ö-29 | 5 | Ön sayım betiğinin eşleşmesi doğrulanmadan kullanıldı (`\b03\b`: 131 satır, doğrusu 57) | İhlali önleyen kural | **Terfi etti** | L4 `skills/audit/SKILL.md` "Koşum biçimi" 4. madde | Betik mercekler başlamadan doğrulanır: eşleşmelerin birkaçı elle örneklenir, desen kimliklere, tarihlere ya da başka ailelere takılıyor mu diye bakılır | PF-42 (Uygulandı) |
| Ö-30 | 6 | Paralel mercek sayısı oturum limitine bağlı — **yöneticinin gözlemi birleştirildi:** on paralel mercek limiti doldurdu ve hiçbiri sonuç üretmedi; beşli dalga, ön plan koşum ve bulguyu bulunduğu anda dosyaya yazma işe yaradı; arka plana atılan alt ajanların bitiş bildirimleri orkestratör alt ajana değil üst yöneticiye gidiyor | İhlali önleyen kural | **Terfi etti** | L4 `skills/audit/SKILL.md` "Koşum biçimi" 5. madde · L4 `skills/checkpoint/SKILL.md` "Koşum" | "Dalga ve kesintiye dayanıklılık": en çok beşli dalga; her mercek bulgusunu bulduğu anda dosyaya yazar; mercekleri yöneten alt ajan onları ön planda başlatır — arka plandaki alt ajanın bitiş bildirimi en üst bağlama gider. Checkpoint skill'i aynı kurala işaret eder (CP02 aynı düzenle koştu). Yeni bir PF açılmadı, PF-43 genişletildi | PF-43 (Uygulandı) |
| Ö-31 | 7 | Sonraki dokümana giden etki atıfları adsız kalıyor ("06 · 12") — çakışma taramasının kanıt eki: gövdede karar satırı olmadan devredilen iki iş | İhlali önleyen kural | **Terfi etti** | L4 checklist §3 (satır biçimi) · §4 (devir taraması) | §3: hedef doküman henüz yazılmamışsa etki atfı bölüm numarası yerine parantez içinde işin adını taşır — `06 (iade reddi kaydının alanı)`. §4: devir taraması önceki dokümanların gövdesinde bu dokümanı anan cümleleri ve önceki aşamaların devir dizinini de okur | PF-44 (Uygulandı) |
| Ö-23 | 8 | Checklist'in skill'e dönüştürülmesi (`00 §C.7`) | Karar | **Bekletildi** — kapı: Aşama 3'ün öğrenim terfisi | L4 checklist başlık notu, §7 (5. adım) · L1 `00 §C.7` (§4, Öneri 2) | §3'te gerekçesiyle | PF-32 (Bekletildi) |
| Ö-32 | 9 | Park satırı kaynak kararı değişince güncellenmedi; park taraması ilişki sözcüğüyle arandığında iki çifti daha kaçırdı (CP02) | İhlali önleyen kural | **Terfi etti** | L4 checklist §3 (geri işaret maddesi) · §7 (3. adım) | §3: geri işaret maddesi kaynak kararın park satırlarını da sayar — park satırı aynı PR'da güncellenir. §7 adım 3: çakışma taraması önceki aşamaların park satırlarını, kaynak K numarasının sonraki satırlarda geçmesine göre tarar, ilişki sözcüğüne göre değil | PF-45 (Uygulandı) |
| Ö-33 | 10 | Salt okunur aşama kaydındaki açık süreç maddesi (avukat teyidi, Ö-23) sonraki aşamanın listesine geçmedi | İhlali önleyen kural | **Terfi etti** (Ö-26 ile aynı madde) | L4 checklist §1 | "Devir notunu kurala karşı oku" (2): devir notunun kapısı açık her maddesi yeni aşamanın açık süreç listesine satır olarak girer; kapısı doküman döneminin ötesindeyse `DEFERRED_BACKLOG.md`'ye | PF-46 (Uygulandı) |
| Ö-34 | 11 | Bir kuralı genişleten geri besleme kararı kuralın bütün yüzlerine yansımadı (K-706: dokuz yer) | İhlali önleyen kural | **Terfi etti** (K-652 ile) | L4 checklist §3 (yeni geri besleme maddesi) | Geri besleme PR'ı kararın genişlettiği ya da daralttığı kuralın anahtar terimini üst dokümanlarda betikle arar; her eşleşme ya güncellenir ya gerekçesiyle elenir | PF-47 (Uygulandı) |
| Ö-35 | 12 | Geri beslemede bölüm sonu Kaynak satırları güncellenmedi — CP01 B-20'nin tekrarı | İhlali önleyen kural | **Terfi etti** (K-652 ile) | L4 checklist §3 (aynı madde) | Geri besleme PR'ı gövdede anılan her K numarasını bölüm sonu Kaynak satırıyla betikle karşılaştırır (cross-review Faz 5 madde 4'ün betiği) | PF-48 (Uygulandı) |

### 2.2 Karar kaydında yaşayan süreç kararları

Checklist §7 madde 5: aşamanın kayıtları arşivlenince otoriterliği biter (K-436, K-647); kural L1–L5'te değilse sonraki aşama onu bilmez. Aşama 2'nin yetmiş iki kararından süreç ya da yazım biçimine dokunanlar tarandı.

| Karar | Konu | Sonuç | Hedef | Uygulanan metin (özet) |
|---|---|---|---|---|
| K-647 | Karar kaydı dönem boyu tek dosya; arşiv işareti yalnız aşamanın kayıtlarını salt okunur yapar | **Önceden terfi etmişti** (PR #56) | L4 checklist §7 (5. ve 6. adım) · tracker başlığı | — |
| K-648 | İki mod Aşama 2 boyunca açık: ⚠ öneriyle kayıt ve `docs:` PR'larında merge yetkisi | **Biçimi terfi etti; yetkinin kendisi süreli** | L3 `INSTRUCTIONS.md` §9 · L3 `GUARDRAILS.md` §3, §6 · L4 checklist §1 (işaretçi) | ⚠ öneriyle kayıt `INSTRUCTIONS §2`'de zaten yazılıydı. Merge yetkisi ve yönetici düzeni yalnız kayıtta ve hafızada yaşıyordu; §9'a "Proje sahibinin açabildiği merge yetkisi ve yönetici düzeni": yalnız onun sözüyle açılır, aşamayla sınırlıdır, kapsamı CI yeşil + check-run sayısı, geri alınamaz işlemler dışarıda; yönetici her adımı temiz bağlamlı alt ajana verir; alt ajan çok kritik konuda karar vermez, dalı push'lar, PR açmaz, yöneticiye döner. GUARDRAILS §3 merge'ün tek istisnasını, §6 yetkiyi anar |
| K-649 | Playbook'a geri akışın evi `PLAYBOOK_FEEDBACK.md` | **Önceden terfi etmişti** (PR #58) | L4 checklist §7 (5. adım) · `skills/gate-check` Adım 7 · `CONTEXT.md` | — |
| K-650, K-651, K-689 | Kullanıcı Akışları'nın iskeleti, anlatıların yeri, bildirim haritasının alt bölümleri | **Dokümanda yaşar** — süreç kuralı değil | Kullanıcı Akışları §0.2 | K-651'in şablon yüzü Ö-27'dedir |
| K-652 | Önceki aşamanın ✓ dokümanına geri beslemenin işletimi | **Terfi etti** | L4 checklist §3 (yeni madde) | "Önceki aşamanın dokümanına dokunan karar o dokümana aynı PR'da geri beslenir": karar bir K satırıdır, üst dokümana sürüm artışıyla yazılır, çıktının geri besleme tablosuna satır girer; ✓ ve kalite döngüsü yeniden açılmaz, etki yansıtma yeniden tarar; iki betik (Ö-34, Ö-35). Aşama 3'te Arayüz Tanımları (`04`) Ürün Gereksinimleri'ne ve Kullanıcı Akışları'na aynı yoldan dönecek |
| K-669, K-670, K-682, K-700, K-701, K-702 | Kullanıcı Akışları'nın yazım konvansiyonları: bölüm numaralı satır kimliği, alt bölüm haritası, yazılmamış bölüme geçici bağ, aktör listesi, atıf dizisinde önsüz `§` | **Genel biçimi terfi etti; ayrıntısı dokümanda yaşar** | L4 checklist §4 | "Konvansiyonları önce yaz" maddesi satır kimliğini, atıf okunuşunu ve aktör listesini sayar. Yeni madde: yazım birden çok oturuma bölünüyorsa ilk oturum alt bölüm haritasını sabitler; yazılmamış bölüme bağlanan hücre geçici olarak alt bölümü taşır ve o bölümü yazan oturum aynı PR'da satır numarasına çevirir |
| K-718 | Ciddiyet ölçüsü Kullanıcı Akışları'na 1. turdan — skill'in ikinci koşulu | **Hizalama terfisi** — kural skill'de vardı | L4 `skills/cross-review/SKILL.md` "Ciddiyet ölçüsü" | İkinci koşul "kapsam, özet ya da akış dokümanında — kuralı başka dokümana bırakan, 'bu doküman kural koymaz' diyen doküman" olur; kanıt: K-644, K-718 (üç turda TEMİZ, 3 → 1 → 0). Arayüz Tanımları (`04`) da bir türetim dokümanıdır; koşulun ona uyup uymadığı o dokümanın cross-review'ından önce gerekçesiyle kaydedilir (K-718'in kalıbı) |

### 2.3 Bu adımda bulunan adaylar

| # | Aday | Kaynak | Sınıf | Sonuç | Hedef | Uygulanan metin (özet) | Playbook |
|---|---|---|---|---|---|---|---|
| Ö-36 | **Güncel Durum bloğu adım PR'larıyla şişti** — her adım bir "Önceki adım" paragrafı ekledi; blok yirmi üç maddeye ve yirmi bir KB'a çıktı (on beşi adım paragrafı); `memory/README.md`'nin "bir ekran boyu" hedefi ve `handoff` skill'inin şişme kontrolü vardı ama adım PR'ları handoff'tan geçmiyordu | `.claude/memory/MEMORY.md` (v0.69 hâli) | İhlali önleyen kural | **Terfi etti ve uygulandı** | L3 `INSTRUCTIONS.md` §7 · L4 `.claude/memory/MEMORY_ARCHIVE.md` (yeni) | §7'nin K-38 cümlesine: Güncel Durum'u güncellemek değiştirmektir — blok son adımı, sıradaki işi ve ileriye bağlayan kayıtları taşır; önceki adımın paragrafı `MEMORY_ARCHIVE.md`'ye taşınır. Bu PR'da on dört adım paragrafı arşive taşındı; blok yirmi bir KB'tan beş KB'a indi | PF-50 |
| Ö-37 | **Adım sonu listesi alt ajan düzeninde yöneticiye gitti, proje sahibine değil** — `INSTRUCTIONS §2` ⚠ öneriyle kayıtta listeyi "adımın ya da oturumun sonunda" proje sahibine gösterir; Aşama 2'nin yedi adımında liste yöneticiye gösterildi ve proje sahibine gitmedi (çakışma taraması §4.1). Kapı (arşivden önce) tuttu; kural düzene yazılı değildi | Çakışma taraması §4.1 | Hizalama | **Terfi etti** (K-648 satırıyla) | L3 `INSTRUCTIONS.md` §9 | Adım sonunda gösterilecek liste alt ajandan yöneticiye gider; yönetici gösterilmemiş kararları en geç aşamanın arşiv işaretinden önce proje sahibine tek mesajda gösterir | PF-49 |
| Ö-38 | **Akış yazımı üst dokümanın yeterlilik testidir** — altmış iki konunun altısı workshop konusuydu; yazım beş oturumda yirmi yedi boşluk buldu (K-671…K-677, K-678…K-681, K-683…K-688, K-690…K-692, K-693…K-699) ve hepsi Ürün Gereksinimleri'ne geri döndü (v0.48…v0.55) | Karar kaydı §8.1 (yazım turu maddeleri) | Tekrarlanacak desen | **`00 §N.1` önerisi** | L1 `00 §N.1` (§4, Öneri 1, desen 1) | Kural yüzü K-652'nin terfisindedir (§2.2) | PF-53 |
| Ö-39 | Kayıt §1'in "`03` sürüm geçmişi" notu v0.10'u taşımadı — PR #69 sürüm başlığını ve tabloyu güncelledi, notu atladı (CP02 U-36) | CP02 §6 | Tek seferlik gözlem | **Raporda kalır** | — | K-38'in kuralı vardı; ikinci kez görülürse terfi adayıdır | — |

### 2.4 Hafızadaki adaylar

CLAUDE.md: *"Bir süreç kuralı yalnız hafızada yaşıyorsa kırılgandır — L1–L5'e terfi eder."* `00 §G.3`: hafızada yalnız kişisel çalışma tercihleri kalır. Proje hafızasında (`.claude/memory/`) `MEMORY.md` ve `README.md` dışında not yoktu; Güncel Durum'un şişmesi Ö-36'dadır.

| # | Not | Aşama 2'deki değişiklik | Sonuç | Hedef | Hafızada kalan |
|---|---|---|---|---|---|
| H-1 | Workshop sorusu öncesi template taraması | İkinci vaka (2026-10-04, K-647): devir notunun "sorulacak" dediği soru | **Terfi genişledi** (Ö-26 ile) | L3 `INSTRUCTIONS.md` §2 · L4 checklist §1 | İşaretçi; nota `Terfi` satırı eklendi |
| H-2 | Kısa diyalog tarzı workshop sorusu | — | **Hafızada kalır** | — | Soru biçimi ve uzunluğu — kişisel üslup |
| H-3 | CI beklerken izleyiciye güvenme | — | **Önceden terfi etmişti** (`INSTRUCTIONS §3.2`) | — | Hatırlatıcı |
| H-4 | Toplu onayda ⚠ maddeleri ayrı işaretle | — | **Önceden terfi etmişti** (`INSTRUCTIONS §2`) | — | Hatırlatıcı |
| H-5 | Limit dolarsa kaldığın yerden devam | — | **Hafızada kalır** — kişisel tercih | — | Tamamı. Kesintiye dayanıklı mercek çıktısı (Ö-30) aynı hattadır ama ayrı kuraldır |
| H-6 | Yazım oturumunda tek bağlam | — | **Hafızada kalır** (kuralı checklist §4'te) | — | Oturum ayarı — araç ayarı |
| H-7 | Sonraki chat mesajı | — | **Hafızada kalır** — kişisel tercih | — | Tamamı; notun kendi terfi koşulu gerçekleşmedi |
| H-8 | Cross-review ikinci model koşumu | — | **Hafızada kalır** — referans | — | Çağrı kalıbı, model ve hesap bilgisi |
| H-9 | Dokümanları isimleriyle, kısa ve açık yaz | 2026-10-04 eki: *"hala aşamaları numaralı söylüyorsun"* — aşamalar da adıyla | **Terfi etti** (ek) | L3 `INSTRUCTIONS.md` §2 | İşaretçi; nota `Terfi` satırı eklendi |
| H-10 | Aşama 1 kapanış otomasyonu yetkisi (Aşama 2 için yenilendi) | Yetki Aşama 2 boyunca açık; yönetici + alt ajan düzeni sürdü | **Biçimi terfi etti** (K-648 ile); **yetki hafızada kalır** — süreli | L3 `INSTRUCTIONS.md` §9 | Yetkinin kendisi ve proje sahibinin düzen tercihi ("adım adım iş yükleme"); nota `Terfi` satırı eklendi |
| H-11 | Yalnız çok kritik konuları sor | Aşama 2 boyunca açık (K-648) | **Hafızada kalır** — süreli; kuralı `INSTRUCTIONS §2`'de | — | Tamamı |

---

## 3. Ö-23 — checklist skill'e dönüşür mü

**Karar: şimdi dönüşmez.** Karar her doküman aşamasının öğrenim terfisinde yeniden verilir; ilk kapı **Aşama 3'ün öğrenim terfisi**. Ölçüt iki koşuldur ve ikisi birlikte aranır:

1. **Checklist izlenebilirlik matrisi zorunlu bir aşamada da işletilmiş olmalı.** Checklist'in §2'si (traceability matrisi) hiç işletilmedi — Aşama 1 ve 2'de zorunlu değildi (`00 §C.1`). Matris Aşama 3, 5, 6 ve 9'da zorunludur; bir skill, işletilmemiş bir bölümü dondurmuş olur.
2. **Bir aşama kapanışında yapısal değişiklik almamış olmalı.** Aşama 1'in kapanışında on üç süreç kararı checklist'e girdi; Aşama 2'nin açılışı §7'yi değiştirdi (K-647, K-649) ve bu adım beş yeni madde ve altı genişletme ekledi (§1'e üç, §3'e bir, §4'e bir madde). Metin hâlâ değişiyor.

**Elenen — şimdi dönüştürmek:** değişmekte olan bir metni skill olarak dondurmak her kapanışta skill'i ve `00 §C.7`'yi birlikte değiştirmeyi gerektirir; dönüştürmenin kendisi `00 §C.7`'nin metnini değiştirir (GUARDRAILS §2). **Elenen — kararı proje sonuna, playbook'a bırakmak:** checklist izlenebilirlikli bir aşamadan sonra kararlı hâle gelirse sonraki aşamalar skill'in kapılarından yararlanır; kararı her kapanışta yeniden vermek ucuzdur.

**Uygulanan (L4):** checklist'in başlık notu ölçütü ve bu kararı yazar; §7'nin 5. adımına "bu checklist'in skill'e dönüşmesi bu adımda yeniden değerlendirilir" maddesi girdi. **Önerilen (L1):** `00 §C.7`'nin *"bir kez gerçek bir projede işletildikten sonra"* cümlesi iki okumaya açık (bir aşama mı, bütün dönem mi) — Aşama 1 onu "bir aşama" diye okudu ve bekletti; ölçütün L1'deki hâli §4, Öneri 2'dedir. Öneri reddedilirse L4'teki ölçüt yürürlükte kalır; L1 ile çelişmez, onu daraltır. **Sonuç (2026-10-04, K-722):** öneri reddedildi — ölçüt L4'te, checklist'in başlık notunda yürürlükte; `00 §C.7`'ye girmedi.

**Kapının taşınması:** karar kaydı dönem sonunda arşivlenir (K-647), Aşama 3'ün planı aynı dosyada sürer. Kapı checklist'in başlık notunda ve PF-32'de yazılı; 6. adımın devir notu onu Aşama 3'e taşır ve Aşama 3'ün açılışı onu açık süreç listesine geçirir (checklist §1, Ö-33'ün kuralı).

---

## 4. Proje sahibinin onayını bekliyor — `00_PROJECT_METHODOLOGY.md`

> **Sonuç — 2026-10-04 (K-437'nin 6. adımı; K-722):** iki öneri ⚠ listesiyle aynı mesajda proje sahibine sunuldu ve **ikisi de reddedildi** (proje sahibinin kararı). `00` değişmedi, sürümü **v1.0.4** kalır; v1.0.5 PR'ı açılmaz. Öneri 1'in altı deseni Aşama 2'nin dersi olarak bu raporda kalır; Öneri 2'nin ölçütü checklist'in başlık notunda (L4) yürürlükte kalır. Playbook listesinde PF-53 ve PF-32 bu sonucu taşır (K-649). Aşağıdaki metinler kayıt için olduğu gibi bırakıldı.
>
> **2026-10-05 düzeltmesi — K-853:** yukarıdaki sonucun Öneri 1'e ilişkin kısmı yanlış kaydedilmişti. Proje sahibi *"Ben ayrı raporda kalsın demedim!"* dedi — tek *"evet"* cevabı, birden çok öneriyi taşıyan mesajda ret diye okunmuştu. **Öneri 1 uygulandı:** altı desen `00 §N.1`'e "Aşama 2 — Kullanıcı akışları" paragrafıyla girdi; `00` **v1.0.5** (2026-10-05). Metin aşağıdakiyle aynıdır; yalnız kural yolları hizalandı (`.claude/GUARDRAILS.md` §3, §6 eklendi) ve Aşama 1'in dersini tekrarlayan üç desene (1, 3, 5) "Aşama 1'in N. deseninin ikinci kanıtı" bağı eklendi. **Öneri 2 için onay verilmedi:** ölçüt checklist'in başlık notunda (L4) kalır, `00 §C.7` değişmez; istenirse sonra açılır. PF-53 bu sonucu taşır. K-722 Aşama 2'nin salt okunur kaydıdır ve değişmedi; düzeltme K-853'tür.

GUARDRAILS §2: `00` proje sahibinin **açık onayı** olmadan değişmez. Aşağıdaki iki öneri bu PR'da **uygulanmadı**. Onay gelirse ayrı bir `docs:` PR'ında uygulanır ve `00`'ın sürümü **v1.0.5** olur (başlık ve alt bilgi; son güncelleme onay tarihi). Hiçbiri aşamanın kapanışını engellemez: kurallar L3–L4'te yürürlükte; `00`'a girecek olan desenlerin kaydı ve skill ölçütünün L1'deki hâlidir.

### Öneri 1 — `§N.1 Dönem 1 — Doküman üretimi`'ne Aşama 2'nin desenleri

`00 §K` tekrarlanacak deseni `§N`'e yazar. Önerilen metin `§N.1`'in Aşama 1 paragrafının ve dokuz maddesinin **ardına** eklenir:

```markdown
**Aşama 2 — Kullanıcı akışları (2026-10-04).** Kurallar `.claude/checklists/document-stage.md` §1, §3, §4, §7, `.claude/INSTRUCTIONS.md` §2, §7, §9 ve `audit`, `checkpoint`, `cross-review` skill'lerine terfi etti; rapor `Docs/CHECKPOINT_REPORTS/PHASE2_LEARNING_PROMOTION.md`. Tekrarlanacak desenler:

1. **Türetim aşamasında workshop küçülür, yazım büyür.** Altmış iki konunun altısı workshop konusuydu; yazım beş oturumda yirmi yedi boşluk buldu ve her biri üst dokümana aynı PR'da geri döndü — ürün gereksinimleri dokümanı aşama boyunca sekiz sürüm aldı. Türetilen dokümanın yazımı, kaynağının yeterlilik testidir.
2. **Kalite döngüsü dokümanı kendi içinde temizler, geri beslenen kuralın öteki yüzlerini görmez.** Cross-review TEMİZ döndükten sonra checkpoint üst dokümanda otuz sekiz yansıma kaçağı buldu; en genişi bir kuralın dokuz yerde eski hâlinde kalmasıydı. Kuralı değiştiren karar, kuralın adını üst dokümanda betikle arar.
3. **Ölçü baştan verilince döngü kısa kalır.** Kural koymayan akış dokümanında ciddiyet ölçüsü ilk turdan uygulandı; 365 KB'lık doküman üç turda TEMİZ döndü (3 → 1 → 0 bulgu). Aşama 1'de ölçüsüz başlayan doküman yirmi beş turda dönmemişti.
4. **Paralel denetimin sınırı oturum limitidir.** On mercek aynı anda başlatılınca hiçbiri sonuç üretmedi; beşli dalgalar ve bulguyu bulduğu anda dosyaya yazan mercekler kesintisiz bitti. Arka plandaki alt ajanın bitiş bildirimi onu başlatana değil en üst bağlama gider; mercekleri yöneten alt ajan onları ön planda koşar.
5. **Adsız devir sonraki aşamada okunamaz; kaynağı değişen park satırı eski kalır.** Sonraki dokümanlara giden yüz otuz yedi atfın seksen altısı yalnız doküman numarasıydı ve çakışma taraması her birine sonradan ad vermek zorunda kaldı; kaynak kararı değişen park satırları ilişki sözcüğüyle aranınca kaçtı, karar numarasıyla aranınca bulundu.
6. **Yönetici ve alt ajan düzeni bir aşamayı bir günde kapattı, bedelini tek kapıda topladı.** Açılıştan checkpoint'e on altı adım temiz bağlamlı alt ajanlarla ve CI yeşilse merge yetkisiyle yürüdü; proje sahibine yalnız konu planı, kritik sorular ve gösterilmemiş kararların listesi gitti. Sorulmadan kaydedilen otuz dokuz karar arşivden önce tek mesajda gösterilir; her adımın eklediği hafıza paragrafı ise durum snapshot'ını yirmi bir KB'a şişirdi — güncellemek değiştirmektir, eklemek değil.
```

**Kaynak:** Ö-38 (desen 1) · Ö-34, Ö-35 (2) · K-718 (3) · Ö-30 (4) · Ö-31, Ö-32 (5) · K-648, Ö-36, Ö-37 (6).

### Öneri 2 — `§C.7 Aşama işletim checklist'i`: skill'e dönüşme ölçütü (Ö-23)

Bugünkü metin:

```markdown
> Bu bilinçli olarak bir skill değil, checklist'tir. Doküman üretimi projeden projeye en çok değişen dönemdir; skill'e dönüştürme, checklist bir kez gerçek bir projede işletildikten **sonra** yapılır.
```

Önerilen metin:

```markdown
> Bu bilinçli olarak bir skill değil, checklist'tir. Doküman üretimi projeden projeye en çok değişen dönemdir; skill'e dönüştürme, checklist gerçek bir projede **kararlı hâle geldikten sonra** yapılır. Ölçüt iki koşuldur: checklist izlenebilirlik matrisi zorunlu bir aşamada da işletilmiş olmalı (§C.1) ve bir aşama kapanışında yapısal değişiklik — yeni adım ya da yeni madde — almamış olmalıdır. Karar her doküman aşamasının öğrenim terfisinde yeniden verilir (§K).
```

**Gerekçe:** "bir kez gerçek bir projede işletildikten sonra" iki okumaya açık — bir aşama mı, bütün doküman dönemi mi. Aşama 1 "bir aşama" diye okudu ve kararı bekletti; Aşama 2 aynı soruyla yeniden karşılaştı. Ölçüt gözlemlenebilir iki koşula bağlanınca her kapanış kararı tek bakışta verir (§3).

### Sürüm satırları

- Başlık: `**Versiyon: v1.0.4** | … | **Son güncelleme:** 2026-10-03` → `**Versiyon: v1.0.5** | … | **Son güncelleme:** <onay tarihi>`
- Alt bilgi: `*Project Playbook — Metodoloji v1.0.4*` → `*Project Playbook — Metodoloji v1.0.5*`

Yalnız bir öneri onaylanırsa sürüm yine v1.0.5 olur.

---

## 5. Playbook'a geri akış (K-649)

Playbook'tan gelen bir dosyaya dokunan ya da dokunması gereken her öğrenim `Docs/PLAYBOOK_FEEDBACK.md`'dedir; liste 48 satırdan **53**'e çıktı. Var olan satırlar tekrar açılmadı, güncellendi:

| Satır | Değişiklik |
|---|---|
| PF-37, PF-41, PF-42, PF-44, PF-45, PF-46, PF-47, PF-48 | `Aday` → `Uygulandı`; bu repodaki yer ve hedef yazıldı |
| PF-40 | `Aday` → `Uygulandı (çevre kural; şablon değişmedi)` — checklist §1'in şablon taraması |
| PF-43 | `Aday` → `Uygulandı`; yöneticinin gözlemi (ön plan, bildirimlerin gittiği yer) ve checkpoint skill'i eklendi |
| PF-32 | Ö-23'ün kararı: `Bekletildi — kapı: Aşama 3'ün öğrenim terfisi; 00 §C.7 ölçütü onay bekliyor` |
| PF-12 | Aşamaların adı eklendi (H-9) |
| PF-27 | Ciddiyet ölçüsünün ikinci koşulu akış dokümanını kapsar (K-718) |
| **PF-49** (yeni) | Merge yetkisi ve yönetici düzeni (K-648, Ö-37) — `INSTRUCTIONS §9`, GUARDRAILS |
| **PF-50** (yeni) | Güncel Durum değiştirilir, eklenmez; `MEMORY_ARCHIVE.md` (Ö-36) |
| **PF-51** (yeni) | Geri beslemenin işletimi (K-652) — checklist §3 |
| **PF-52** (yeni) | Çok oturumlu yazımın alt bölüm haritası ve geçici bağları; konvansiyon listesi (K-669, K-670, K-682, K-700…K-702) — checklist §4 |
| **PF-53** (yeni) | Aşama 2 dersleri — `00 §N.1`'e altı desen (onay bekliyor) |

---

## 6. Kayda ve hafızaya yansıma

- **Karar kaydı v0.70:** K-719 (bu adımın kararları, öneriyle kaydedildi — ⚠ değildir: süreç kararıdır, ürün kuralına dokunmaz). K-646, K-648, K-652, K-670, K-682 ve K-718'in etki sütununa K-719'a geri işaret (K-646 Aşama 1 kaydıdır; salt okunur kaydın tek istisnası). §8.1'in öğrenim adayları maddesi işaretlendi; öğrenim terfisi maddesi ve `00` önerilerinin açık maddesi (kapı: ⚠ listesiyle aynı mesaj) girdi. §5'te CP02'nin 2. aksiyon maddesi kapandı. ⚠ listesi değişmedi (39).
- **Repo hafızası:** `MEMORY.md` Güncel Durum bu adımın hâline getirildi ve yeni kurala göre kısaltıldı — Aşama 2'nin önceki on dört adım paragrafı yeni `.claude/memory/MEMORY_ARCHIVE.md`'ye taşındı; "Terfi Edenler" tablosuna bu adımın beş satırı girdi.
- **Kullanıcı hafızası:** terfi eden üç nota (H-1, H-9, H-10) kısa bir `Terfi` satırı eklendi; notlar silinmedi. Süreli yetki notları (H-10'un yetkisi, H-11) hafızada kalır.

**Sonuç (6. adım, 2026-10-04):** tek mesaj gönderildi — ⚠ listesine itiraz yok, avukat teyidi önerisi reddedildi (K-721), `00` önerileri reddedildi (K-722); arşiv işareti düştü, devir karar kaydının §9'unda. Aşağıdaki satır 5. adımın hâlidir.

**Sırada:** K-437'nin 6. adımı — otuz dokuz ⚠ kararın (CP02 §5.1), avukat teyidi önerisinin (CP02 §5.2) ve §4'ün iki önerisinin proje sahibine **tek mesajda** gösterilmesi; ardından arşiv işareti (`03` ✓) ve Aşama 3'e devir — devir notu Ö-23'ün kapısını ve `00` önerilerinin sonucunu taşır.

---

*Kaynak: K-437 (kapanış sırası, 5. adım) · `00 §K`, §A.4, §G.3, §C.7 · GUARDRAILS §2 · karar kaydı §8.1 (aday listesi) · çakışma taraması §4, §7 · CP02 §6 · K-647…K-718 (süreç kararları) · K-646 (Ö-23'ün bekletilmesi) · K-649 (playbook'a geri akış) · K-719 (bu adımın kararları) · `Docs/CHECKPOINT_REPORTS/PHASE1_LEARNING_PROMOTION.md` (yapı örneği).*

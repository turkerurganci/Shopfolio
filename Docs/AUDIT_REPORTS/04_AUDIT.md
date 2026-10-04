# Audit Raporu — 04 UI Specs

**Tarih:** 2026-10-04 · **Hedef:** `Docs/04_UI_SPECS.md` v0.12 (bulgular v0.12'de üretildi, v0.13'te işlendi; taban `main` = e585e6a) · **Bağlam:** `PRODUCT_DISCOVERY_STATUS.md` §2 (K-01…K-829; Aşama 3 kararları K-723…K-829), §3, §10; `02_PRODUCT_REQUIREMENTS.md` v0.62; `03_USER_FLOWS.md` v0.18; `10_MVP_SCOPE.md` v0.41; `01_PROJECT_VISION.md` v0.33; `04` şablonu (`git show 95a168e:Docs/04_UI_SPECS.md`); `CHECKPOINT_REPORTS/PHASE1_CONFLICT_SCAN.md` §6 ve `PHASE2_CONFLICT_SCAN.md` §6; resmî mevzuat metinleri (mevzuat.gov.tr, resmigazete.gov.tr, kvkk.gov.tr — 2026-10-04'te okundu) · **Odak:** Tam denetim

### Koşum biçimi (K-431)

Dokümanı yazan bağlamda koşmadı. Yönetici bağlamı mercekleri ve şüphecileri alt ajan olarak başlattı ve hükümlerini verdi; bulguların uygulanması, kararların kaydı ve raporlar temiz bağlamlı ayrı bir alt ajanda yapıldı. **Dokuz mercek iki dalgada** — beş + dört (`audit` skill'i, "Koşum biçimi" 5. madde): (1) kapsama ve kararlar — Aşama 3 kararları, konu planı, GAP satırları, park blokları ve iki çakışma taramasının devir dizinleri (`KAPSAM-K`) · (2) durum × rol matrisi (`DURUM`) · (3) form ve validasyon envanteri (`FORM`) · (4) atıf, tırnak içi metin, belirsiz ifade (`ATIF`) · (5) iç tutarlılık — ortak bileşen ↔ ekran, navigasyon ↔ giriş-çıkış, envanter, metin kalıpları, wireframe düzeyi (`IC`) · (6) yasal uyum (`YASAL`) · (7) deep review — tasarım kalitesi, edge case, erişilebilirlik (`DR`) · (8) müşteri ekranlarının akışa karşı derinliği (`EKRAN-M`) · (9) panel ekranlarının akışa karşı derinliği (`EKRAN-P`). Mercekler **salt okuma** koştu; her biri bulgusunu **bulduğu anda** kendi dosyasına yazdı ve dosyasının sonunda betikle sayılmış bir envanter özeti bıraktı. Ortak brif: bilinçli bir karara (K satırı olan yerleşim ya da metin) "daha iyisi olurdu" itirazı bulgu sayılmaz; kural ayrıntısı evinde kalır — `04`'te kopyalanmış değer de bulgudur (konvansiyon 10).

**Karşı-doğrulama:** 109 ham bulgu. **Kanıt şüphecisi** her bulguyu güncel metne karşı yeniden okudu: 90 doğru · 12 kısmi · 7 çürük; şiddeti yeniden değerlendirdi (yüksek 2 · orta 18 · düşük 82; çürükler hariç) ve on bir birleştirme grubu kurdu (G-A…G-K). **Bağlam şüphecisi** her bulguyu projenin karar bağlamına (öneriyle kayıt, ⚠ grubu, K-652, K-36, konvansiyonlar) karşı okudu: 79 kabul (9'u tekrar) · 29 kısmi · 1 ret; sınıf (a) metin düzeltmesi · (b) ekran kurgusu kararı · (c) üst doküman — K-652 · (d) park · (k) karar kaydının biçimi.

**Yönetici hükümleri (bağlayıcı):** (1) kanıt şüphecisinin çürüttüğü yedi bulgu ve bağlam şüphecisinin reddettiği bir bulgu **uygulanmaz** — raporda gerekçeleriyle durur; (2) kalan bulgular birleştirme gruplarında bir kez ve **iki şüphecinin önerisinden küçük olanıyla** uygulanır, ayrışmada karar uygulayan ajandadır; (3)–(7) beş yasal ya da önceki karardan ayrılan konu öneriyle kaydedilir (cayma özeti, okunabilirlik, geri alma bağlantısı, ayıp talebinin adet sınırı, kargodaki kalemde cayma); (8) çok kritik soru eşiği. Çok kritik soru çıkmadı.

**Sonuç:** 109 bulgu — **74 uygulandı · 27 kısmen uygulandı · 8 uygulanmadı**; birleştirme sonrasında **95 tekil kök**. On yedi karar (K-830…K-846; onu ⚠). Bu rapor altı merceğin 72 bulgusunu taşır; durum, yasal ve deep review merceklerinin 37 bulgusu [`04_DEEP_REVIEW.md`](04_DEEP_REVIEW.md)'dedir — envanterleri sayım bütün kalsın diye aşağıda burada da sayılır (03'ün raporlarının düzeni).

**Mekanik sayımlar betikle yapıldı:** envanter sayımları (her merceğin dosyası); K numarası ↔ kayıt; `04`'ün değişen satırlarındaki atıfların çözülmesi (K-701'in dizi okunuşuyla); `E-nn`/`OB-nn` tanımı; tırnak içi metnin kaynakta ya da K satırında birebir bulunması; matris 1.193 satır ve yinelenen kimlik; gövdedeki K numarası ↔ Kaynak satırı (K-652'nin ikinci betiği); K tekilliği; CR ve `$[0-9]` artığı. Sonuçlar PR açıklamasındadır.

### Envanter Özeti

Dokuz merceğin envanteri; her satır merceğin dosyasının sonundaki betikli sayımdır.

| Kaynak | Mercek | Toplam | ✓ | ⚠ | ✗ |
|---|---|---|---|---|---|
| Karar kaydı K-723…K-829 (107) · §10 plan konuları (57) · §3 Aşama 3 GAP satırları (11) · `04` başlığındaki park satırları (4) · Aşama 1 devirleri (80) · Aşama 2 atıfları (35) · Aşama 2 gövde cümleleri (7) | `KAPSAM-K` | 301 | 275 | 26 | 0 |
| §6'nın matris satırları (282) + `03 §1.11`'in işlemleri (41) | `DURUM` | 323 | 282 | 34 | 7 |
| §7'nin alan satırları (164) + §7.3.2 alan türleri (19) + §7.3.3 limitler (9) | `FORM` | 192 | 173 | 15 | 4 |
| `04`'ün §1 dışı tekil (satır, atıf) çiftleri (10.003) · (satır, tırnaklı ifade) çiftleri (794) · belirsiz ifade, placeholder, `$n`, CR eşleşmeleri (5) | `ATIF` | 10.802 | 10.714 | 68 | 20 |
| OB × ekran çifti (212) · §3 geçiş satırı (121) · ekranın giriş-çıkışı (53) · envanter kimliği (54) · metin kalıbı (40) · özet ↔ ayrıntı (24) · wireframe düzeyi (6) | `IC` | 510 | 492 | 17 | 1 |
| `04`'te yasal iddia ya da yasal yüzey taşıyan madde | `YASAL` | 89 | 73 | 7 | 9 |
| 53 ekran × şablonun sekiz alanı (424) · 14 edge case · 20 odaklı deep review öğesi | `DR` | 458 | 427 | 30 | 1 |
| (ekran, kaynak matris satırı) çifti — E-01…E-28 | `EKRAN-M` | 772 | 756 | 14 | 2 |
| (ekran, kaynak matris satırı) çifti — E-29…E-47, E-49…E-54 | `EKRAN-P` | 896 | 864 | 23 | 9 |
| **Toplam** | | **14.343** | **14.056** | **234** | **53** |

**Sayım tutuyor:** her merceğin Faz 1 envanteri ile Faz 2 eşleştirmesi aynı öğe listesidir; toplam 14.343 = 14.056 + 234 + 53. Atıf merceğinde öğe (satır, atıf) çiftidir — aynı atıf iki satırda geçiyorsa iki öğedir; ekran merceklerinde öğe (ekran, kaynak satır) çiftidir. Bir mercekte ⚠ alıp bulguya dönüşmeyen öğeler başka bir bulgunun kökünün tekrarıdır ya da konvansiyon gereği sınır durumdur (ör. E-48'in düşen kimlik olarak anılması).

### Bulgular (audit mercekleri)

Toplam 72 bulgu. Kanıt şüphecisi: 61 doğru · 6 kısmi · 5 çürük. Bağlam şüphecisi: 54 kabul (6'sı tekrar) · 18 kısmi · 0 ret. Şiddet (kanıt şüphecisinin düzeltmesinden sonra, çürükler hariç): yüksek 0 · orta 8 · düşük 59. **Sonuç: 51 uygulandı · 16 kısmen uygulandı · 5 uygulanmadı.**

| # | Yer | Şiddet | Bulgu | Kanıt · Bağlam | Sonuç | Uygulama / karar |
|---|---|---|---|---|---|---|
| KAPSAM-K-1 | karar kaydı K-824…K-829 | orta | Altı satır yedi hücre taşıyordu — görüntüde etki sütunu kayboluyordu | DOĞRU · KABUL | Uygulandı | Fazladan tarih hücresi silindi; altı satır altı sütun |
| KAPSAM-K-2 | 9.16.3, 9.20.2, 9.20.5, 9.21.4, 9.21.5; §6.3 | düşük | Dört tırnaklı düğme adı kaynakta ve kayıtta yok (G-A) | DOĞRU · KABUL | Uygulandı | K-846 dokuz adı sabitledi; Kaynak satırlarına K-846 |
| KAPSAM-K-3 | 9.18.13 | düşük | "kargo ücreti 0 TL" değeri kopyalanmış (G-B) | DOĞRU · KABUL | Uygulandı | "P-1'in varsayılanıyla" |
| KAPSAM-K-4 | K-736…K-823'ün on ikisi ↔ `05`/`06`/`07`/`12` park blokları | düşük | On iki karar sonraki dokümana iş bırakıyor, park satırı yok | DOĞRU · KABUL | Uygulandı | On iki park satırı; on iki etki hücresine "park satırı, 2026-10-04" |
| KAPSAM-K-5 | 5.27.3 | düşük | K-784'ün sayfa boyu (25 sipariş) ekranda yazılı değil | DOĞRU · KABUL | Uygulandı | 5.27.3'e "sayfa başına 25 sipariş (K-784)" |
| KAPSAM-K-6 | 9.12.2, 9.13.2, 9.13.14, 9.20.5, 9.24.2–9.24.5; §6.3 | düşük | Beş tırnaklı düğme adı daha kaynakta yok (G-A) | DOĞRU · KABUL | Uygulandı | K-846 |
| KAPSAM-K-7 | 2.2.4, 2.11.1, 8.3.2 | — | Kesilme istisnaları kuralı taşımayan K satırına bağlanmış | ÇÜRÜK · KISMİ | Uygulanmadı | Kanıt: kural K-741, K-746, K-749'un satırında (gerekçenin Edge notu) yazılı ve K-824 8.3.2'nin başlığında anılıyor — atıf kuralın doğduğu satırı gösteriyor (yönetici hükmü 1) |
| KAPSAM-K-8 | 7.3.1 | düşük | "(8.2.7)" hitabı göstermiyor | DOĞRU · KABUL | Uygulandı | "(8.2.9)" |
| KAPSAM-K-9 | 5.13.8 | — | "Tek yöntem seçili gelir" K-757'ye dayandırılmış | ÇÜRÜK · KABUL | Uygulanmadı | Kanıt: K-757'nin Edge notu "tek ödeme yöntemi açıksa seçili gelir" der — atıf doğru (yönetici hükmü 1) |
| KAPSAM-K-10 | 5.13.31 ↔ 2.7.3.3 | düşük | Sipariş onayında üye sipariş geçmişine gidiyor; bileşen sipariş sayfası diyor (G-J) | DOĞRU · KISMİ | Kısmen uygulandı | Hedef değişmedi; 2.7.3.3'e iki ekran istisnası yazıldı (sipariş onayı, hesap silme) |
| KAPSAM-K-11 | 5.4.5, 5.12.3, 5.13.6 | düşük | K-773'ün satır biçimi ekranda yok | DOĞRU · KABUL | Uygulandı | 5.4.5'e biçim (kısa liste, ilk beş + kalan sayı, yerinde açılma); 5.12.3 ve 5.13.6 5.4.5'e işaret eder |
| KAPSAM-K-12 | 5.21.8 | düşük | "Kullanılmış bağlantıda değişiklik zaten geçerlidir" K-780'in mesajıyla çelişiyor | DOĞRU · KISMİ | Kısmen uygulandı | Ek cümle silindi; K-780'in hâli ikiye bölünmedi (bağlam: bölmek önceki karardan ayrılırdı) |
| KAPSAM-K-13 | 2.5.4, 9.9.33 | — | K-759'un "ilk alıcıya" kuralı ekranda yok | ÇÜRÜK · KISMİ | Uygulanmadı | Kanıt: kural `02 §9.1.6`'da yazılı ve 2.5.4 ona işaret ediyor; kopyalamak konvansiyon 12'ye aykırı (yönetici hükmü 1) |
| KAPSAM-K-14 | 5.12.7 | düşük | K-775'in "sepette varyant değiştirme yoktur" cümlesi yok | DOĞRU · KABUL | Uygulandı | 5.12.7'nin "Yoktur" maddesine |
| ATIF-1 | 9.16.3, 9.20.x, 9.21.x; §6.3 | düşük | Dört düğme adı kaynaksız (G-A, KAPSAM-K-2 ile aynı) | DOĞRU · KABUL (tekrar) | Uygulandı | K-846 |
| ATIF-2 | 9.20.5, 9.24.x, 9.13.x | düşük | Üç düğme adı daha kaynaksız (G-A) | DOĞRU · KABUL (tekrar) | Uygulandı | K-846 |
| ATIF-3 | 5.13.31 | düşük | Ekranın söylemediği metin kaynaksız tırnakla ("sipariş verilmedi") | DOĞRU · KABUL | Uygulandı | K-745'in metni: ekran "işlem yapılmadı" demez |
| ATIF-4 | §2.12.1 Kaynak | düşük | Dizide § işaretsiz "1.7.4" çözülmüyor | DOĞRU · KABUL | Uygulandı | "`03 §8.7.4.3`, §1.7.4" |
| ATIF-5 | 5.19, 7.1, 7.2, 7.3, 9.22, 9.23 Kaynak | düşük | Eski K numaraları "devir:" öneki olmadan anılıyor | KISMİ · KISMİ | Kısmen uygulandı | İki şüpheci de konvansiyonu yasak okumadı; küçük olan kanıt şüphecisinin önerisi seçildi — konvansiyon 11'e yarım cümle: ek olarak anılan Aşama 1–2 kararı "devir:" taşımaz (bağlamın yirmi atıflık betik düzeltmesi daha büyük) |
| ATIF-6 | dokuz tırnaklı ifade | düşük | Kaynakta küçük harfle başlayan metin büyük harfle başlıyor | DOĞRU · KISMİ | Kısmen uygulandı | Konvansiyon 9'a yarım cümle: düğme ve etiket büyük harfle başlar, gerisi birebir; dokuz yer değişmedi |
| ATIF-7 | başlık notu | düşük | "§3'e (ekran envanteri)" — envanter §4'tür; şablon hatası | DOĞRU · KABUL | Uygulandı | "§4'e"; `PLAYBOOK_FEEDBACK` PF-58, tracker §10.1 öğrenim adayı 5 |
| ATIF-8 | konvansiyon 1 | düşük | `02`'den tırnaklı aktarma birebir değil | DOĞRU · KABUL | Uygulandı | Kaynağın metni birebir |
| ATIF-9 | §9 notu | düşük | "olabilir" — şablon artığı belirsiz ifade | DOĞRU · KABUL | Uygulandı | Sayıyla yazıldı (25 / 53); PF-58 |
| ATIF-10 | başlık notu, park K-14 · K-574 | düşük | "yazımı §2 ve §3'tedir" — İletişim sayfası §5.10'da | DOĞRU · KABUL | Uygulandı | "2.2.5, 3.1.21 ve 5.10.3" |
| IC-1 | §4.3 | düşük | E-19'un amacı ve E-29'un aktörü tabloda olmadan değişmiş | DOĞRU · KABUL | Uygulandı | §4.3'ün satırına E-19, E-29; gerekçe K-787 ve konvansiyon 7 |
| IC-2 | 3.3.12, 5.2.1 | düşük | Nereye hücresi "ürünler" — kimlik değil (G-K) | DOĞRU · KABUL | Uygulandı | "E-01 · E-02 (birinci seviye kategoriler — 2.7.1.2)"; 5.2.1'in girişine 3.3.12 |
| IC-3 | §2.6 ve §3.3 Kaynak satırları | düşük | İtalik kapanmıyor | DOĞRU · KABUL | Uygulandı | İki satırın sonuna `*` |
| IC-4 | 9.10.10 ↔ 7.2.3 (G-H) | düşük | E-38 "form yoktur" der, §7 süzgeci alan sayar | DOĞRU · KABUL (tekrar) | Uygulandı | 9.10.10: süzgeçler kapalı değerler, envanter 7.2.3; 7.2.3 başlığına 9.10.10 |
| IC-5 | OB-04 tablo satırı | düşük | Tablo E-16'yı sayıyor, bileşenin alt bölümü saymıyor | DOĞRU · KABUL | Uygulandı | Tablodan E-16 çıktı |
| IC-6 | 9.10.7, 9.12.8 ↔ `02 §10.7.4` | düşük | Dışa aktarmanın yeri `02`'nin tersini gösteren atıfla değişmiş | KISMİ · KABUL | Uygulandı | Ayrışma: kanıt şüphecisi yer kararını K-749/K-764/K-823'te kayıtlı saydı; bağlam şüphecisi K-514'e karşı tartılmadığını gösterdi. `02`'nin aynı PR'da hizalanması bir K satırına bağlanır (K-652) — bağlamın önerisi uygulandı: **K-836 (⚠)**, `02 §10.7.4` v0.63, K-514'e geri işaret |
| IC-7 | §3.3, 5.14.1, 5.12.1 | düşük | E-14 → E-12 geçişi haritada yok | DOĞRU · KABUL | Uygulandı | 3.3.38; 5.14.1'in çıkışı, 5.12.1'in girişi |
| IC-8 | 6.2.24.5 | düşük | Matris E-24'ten doğrudan E-23'e götürüyor | DOĞRU · KABUL | Uygulandı | "Vazgeç" → E-25, oradan E-23 (3.3.36) |
| IC-9 | 9.2.4, 6.3.2.2 | düşük | Davet kabulünün geçersiz bağlantı hâli çıkışsız | DOĞRU · KABUL | Uygulandı | Mesajın altında panel girişi bağlantısı; 9.2.1, 6.3.2.2, 3.4.18 |
| IC-10 | §3.2 üst satır | düşük | "üç geçiş" + dört satır | DOĞRU · KABUL | Uygulandı | "üç bölgede dört geçiş" |
| IC-11 | §4.1–§4.3 | düşük | Alt bölümlerin Kaynak satırı yok | DOĞRU · KISMİ | Kısmen uygulandı | §4.4'ün satırı "Kaynak (§4.1–§4.4)" diye adlandırıldı; K-787 eklendi |
| IC-12 | 2.4.5, 2.7.3.3 | düşük | Bileşen kuralı hesap silmeyi kapsamıyor (G-J) | DOĞRU · KABUL | Uygulandı | 2.4.5: hesap silmede "Vazgeç" hesap ekranında; 2.7.3.3: hesap silmede hesap ya da giriş |
| IC-13 | 3.3.25 | düşük | E-19'un tetikleyicisi "son adımın onayı" değil | DOĞRU · KABUL | Uygulandı | Tetikleyici ayrıldı; Kaynak'a `03 §2.9.2`, §2.9.6 |
| IC-14 | 8.2.9 ↔ 5.4.10 (G-I) | düşük | Hitap özeti ürün sayfasının iki hitabını söylemiyor | DOĞRU · KISMİ | Kısmen uygulandı | 8.2.9'a "mesajlar kaynağın metnidir, hitabı kaynağı izler"; `02`'nin iki metni (§3.12.9 ↔ §6.2.9) hizalanmadı — fark `02`'nin içindedir ve `04` iki kaynağın birini birebir izler; cross-review'ın etki yansıtmasına not edildi (karar kaydı §10.1, `MEMORY.md`) |
| FORM-1 | 7.1.12, 7.3.3.9 (G-G) | düşük | E-22'nin bağlantıyı yeniden isteme işlemi §7'de yok | DOĞRU · KABUL | Uygulandı | 7.1.12.4; 7.3.3.9'a 7.1.12.4 |
| FORM-2 | 7.3.3.9 (G-G) | düşük | L-9'a sayılan yeni e-posta isteği tabloda yok | DOĞRU · KABUL | Uygulandı | 7.3.3.9'a 7.1.15.2, 7.2.24.2 |
| FORM-3 | 7.1.1.1 | düşük | Ters aralık kuralı `02 §3.5.2`'ye bağlanmış | DOĞRU · KABUL | Uygulandı | "(5.2.7; OB-12)" |
| FORM-4 | 7.3.2.9 | düşük | %100 yasağı yanlış alt bölüme bağlanmış | DOĞRU · KABUL | Uygulandı | "`02 §3.9.1`, §3.10.4" |
| FORM-5 | 7.3.2.7 | düşük | IBAN'da boşluk kuralının kararı K-825 anılmıyor | DOĞRU · KABUL | Uygulandı | "K-817, K-825" |
| FORM-6 | 7.2.4.15 | düşük | İfa süresi "Zorunlu" — yayın kapısıdır | DOĞRU · KABUL | Uygulandı | "Yayın için zorunlu — hizmette (P-44)" |
| FORM-7 | 7.2.4.12, 9.5.6, 9.5.22, 6.3.5.6, 5.4.4 (G-E) | orta | Dijital üründe üretim yeri düşmüş (`02 §3.8.5`) | DOĞRU · KABUL | Uygulandı | Beş yerde dijitalde isteğe bağlı; `03 §2.1.5` `02`'ye hizalandı (v0.19) |
| FORM-8 | 7.3.2.12 | düşük | İki kural 2.12.5.3'e bağlanmış ama orada yok | DOĞRU · KISMİ | Kısmen uygulandı | Atıf "(2.12.5.3; 9.22.9, 9.23.9, 9.24.8)"; bileşene kural eklenmedi |
| FORM-9 | 7.1.5.7 | düşük | "En fazla bir kod" K-806'ya bağlanmış | DOĞRU · KABUL | Uygulandı | "(`02 §3.10.1`)" |
| FORM-10 | 7.2.4.16, 7.2.21.5, 7.2.5.2 | düşük | Parametre değerleri kimliksiz | DOĞRU · KISMİ | Kısmen uygulandı | "P-9'un çiti", "P-8'in çitinde"; 7.2.5.2'de "üç seviye" kaynağın metni olarak kaldı, yanına "(P-20)" |
| FORM-11 | 7.2.4.8, 9.5.21 (G-F) | orta | İndirim yürürken sıfıra inen varyant fiyatı ekranda yok | DOĞRU · KABUL | Uygulandı | İki yerde kural; mesajı K-799'da |
| FORM-12 | 7.3.2.15 | düşük | Biçim seti bütün panel metinlerine genellenmiş | DOĞRU · KABUL | Uygulandı | Yalnız `02 §3.11.6`'nın saydığı alanlarda |
| FORM-13 | 7.3.2.6, 7.3.2.16 | düşük | Kullanım listeleri eksik | DOĞRU · KABUL | Uygulandı | 7.2.14; 7.2.3, 7.2.7, 7.2.10 |
| FORM-14 | 7.2.19 | — | Geçici kapatma anahtarı envanterde yok | ÇÜRÜK · KABUL | Uygulanmadı | Kanıt: anahtar formun alanı değildir — kendi onayıyla, formun kaydından bağımsız işler (9.16.2) ve doğrulaması yoktur; §7'nin tanımına uygun (yönetici hükmü 1) |
| FORM-15 | 7.2.3.3 ↔ 9.10.10 (G-H) | düşük | E-38 "form yoktur" ↔ süzgeç satırı | DOĞRU · KABUL | Uygulandı | IC-4 ile birlikte |
| FORM-16 | 7.2.4.13 | düşük | Ölçü birimi ve net miktarın kuralı boş | DOĞRU · KISMİ | Kısmen uygulandı | İkisi birlikte girilir (`02 §3.8.5`'in tek alanı), net miktar sıfırdan büyük; birimin listesi Veri Modeli'nin park bloğuna — K satırı açılmadı |
| FORM-17 | 7.2.22 | — | Alıcı grupları bölümü envanterde yok | ÇÜRÜK · KABUL | Uygulanmadı | Kanıt: alıcı grupları metin taslağının bir bölümüdür, alan değildir (`02 §3.33.2`; 9.19.3) — 7.2.22.1'in düzenlenen metninin içindedir (yönetici hükmü 1) |
| EKRAN-M-1 | 5.13.18 | düşük | Aynı sepetin ödenmemiş önceki siparişinin iptali ekranda yok | KISMİ · KISMİ | Uygulandı | Ayrışma: kanıt yalnız 5.13.18 cümlesini doğru saydı, bağlam ⚠ öneriyle bir ön uyarı önerdi; yönetici ⚠ listesine aldı — **K-837 (⚠)**: 5.13.9 (a) ve 5.13.18 |
| EKRAN-M-2 | 5.4.10 (G-I) | düşük | Uyarı metni `02 §3.12.9`'un yarısı | KISMİ · KISMİ | Kısmen uygulandı | Metin değişmedi (iki kaynakla birebir); Kaynak satırına `02 §3.12.9` |
| EKRAN-M-3 | 5.16.12, 6.2.16.20 | düşük | Kart iadesi yeniden denemesiyle IBAN alanının kapanması yok | DOĞRU · KABUL | Uygulandı | İki yerde `03 §8.3.3.5` |
| EKRAN-M-4 | §1.1 iki matris satırı | düşük | E-16'ya eşlenmiş hâlin müşteri yüzü yok | DOĞRU · KISMİ | Kısmen uygulandı | İki satırdan E-16 çıktı; E-16'nın §1.2 satırı betikle yenilendi (227 → 225); matris 1.193 |
| EKRAN-M-5 | 5.18.5 | düşük | Kargodaki kalemde iade bölümü gönderme süresini koşulsuz söylüyor | KISMİ · KABUL | Kısmen uygulandı | Yönetici hükmü 7 — kanıt şüphecisinin okuması (m.13/1: süre bildirimden işler): **K-838 (⚠)** — önce kargonun malı döndürdüğü, süre mal teslim alınırsa; `02 §7.3.5`, `03 §2.8.1.2` (K-652) |
| EKRAN-M-6 | 5.13.15, 2.12.6.3, 7.1.5.7 | düşük | Kupon reddi tek mesaj | DOĞRU · KISMİ | Kısmen uygulandı | **K-845** — yalnız asgari tutar ayrı mesaj; öteki retler sebep ayırt etmez |
| EKRAN-M-7 | 5.13.21 | düşük | "Hiçbir yöntem kullanılamıyor" hâlinin tetiği yok | DOĞRU · KABUL | Uygulandı | Tetik ve L-8 + kart bileşimi |
| EKRAN-P-1 | 9.9.6 | orta | Süre aşımından sonraki müşteri iptalinde kanuni faiz uyarısı yok | DOĞRU · KABUL | Uygulandı | K-798'in uyarısı iptal kaydında; `12` park satırı (K-798) |
| EKRAN-P-2 | 9.20.9 | orta | "Yeni davet gönder" yanlış adresin yolu sayılmış | DOĞRU · KABUL | Uygulandı | "E-posta ulaşmadı"nın yolu; yanlış adres için geri çekme + doğru adrese davet (`03 §8.8.6`) |
| EKRAN-P-3 | 9.9.35 | düşük | "tek koşullu adım" belirsiz | KISMİ · KISMİ | Kısmen uygulandı | "tek bir koşullu adım sayılır"; öteki koşullu adımlar `03 §8.5.6`'ya işaret — liste kopyalanmadı |
| EKRAN-P-4 | 9.18.13 (G-B) | düşük | "0 TL" | DOĞRU · KABUL (tekrar) | Uygulandı | KAPSAM-K-3 ile birlikte |
| EKRAN-P-5 | 9.8.4 | orta | Sayaç = satır sayısı eşitliği kaynaksız | DOĞRU · KISMİ | Kısmen uygulandı | Eşitlik cümlesi silindi; sayacın birimi için karar açılmadı |
| EKRAN-P-6 | 9.9.38 (G-D) | orta | Kapalı işlem listesi matrisle çelişiyor | DOĞRU · KABUL (tekrar) | Uygulandı | Liste `03 §1.11`'e ve 6.3.9.8, 6.3.9.10'a bağlandı |
| EKRAN-P-7 | 9.5.21 (G-F) | orta | Sıfıra inen fiyatın mesajı yok | DOĞRU · KABUL (tekrar) | Uygulandı | FORM-11 ile birlikte |
| EKRAN-P-8 | 9.19.7 | düşük | Çerez politikası da veri toplayan girişlerin kapısı gibi okunuyor | DOĞRU · KABUL | Uygulandı | Kapı yalnız aydınlatma metni (`02 §3.1.5`) |
| EKRAN-P-9 | 9.9.30 | düşük | Ayıplı dijital kalemin çözümü (dosya güncellemesi) anılmıyor | DOĞRU · KISMİ | Kısmen uygulandı | Tek cümle (E-33, 9.5.19); yeni geçiş açılmadı |
| EKRAN-P-10 | 9.9.27 | düşük | Teslim almanın IBAN isteği sonucu yazılmamış | DOĞRU · KABUL | Uygulandı | `03 §8.3.2.1`, §1.11.28 |

### Birleştirme grupları

Kanıt şüphecisinin on bir grubu esas alındı (aynı kök, tek düzeltme). Bağlam şüphecisinin grupları bunlarla örtüşür; ayrıca daha gevşek üç grup kurdu — G-3 "kuralı taşımayan K'ye atıf" (KAPSAM-K-7, K-9 çürük; FORM-5, FORM-9 ayrı ayrı uygulandı), G-4'e FORM-10'u ekledi, G-11 "Mesafeli Sözleşmeler Yönetmeliği m.6" (YASAL-1, YASAL-2 — iki ayrı karar) — bunlar tekil kök sayımına girmedi.

| Grup | Kök | Bulgular | Tek düzeltme |
|---|---|---|---|
| G-A | Kaynakta tırnaklı karşılığı olmayan düğme adları (konvansiyon 9) | KAPSAM-K-2, KAPSAM-K-6, ATIF-1, ATIF-2 | K-846 |
| G-B | 9.18.13 "kargo ücreti 0 TL" (konvansiyon 10) | KAPSAM-K-3, DR-12, EKRAN-P-4 | "P-1'in varsayılanıyla" |
| G-C | K-793'ün "teslim edilmiş adet" sınırı ↔ `02 §7.5.2` | DURUM-1, YASAL-3 | K-830 (⚠); 5.19.3, 7.1.10.2 |
| G-D | 9.9.38'in kapalı işlem listesi ↔ 6.3.9.8 | DURUM-2, EKRAN-P-6 | 9.9.38 matrise bağlandı |
| G-E | Dijital üründe üretim yeri düşmüş | FORM-7, YASAL-5 | Beş yer + `03 §2.1.5` |
| G-F | İndirim sürerken sıfıra inen varyant fiyatı | FORM-11, EKRAN-P-7 | 7.2.4.8, 9.5.21 |
| G-G | L-9 envanteri eksik | FORM-1, FORM-2 | 7.1.12.4, 7.3.3.9 |
| G-H | E-38 süzgeci ↔ "form yoktur" | FORM-15, IC-4 | 9.10.10, 7.2.3 |
| G-I | "Bu ürünü daha önce aldınız" — `02`'nin iki metni, hitap | EKRAN-M-2, IC-14 | Kaynak satırı + 8.2.9 |
| G-J | 2.7.3.3'ün hedefi — ekran istisnaları | KAPSAM-K-10, IC-12 | 2.7.3.3, 2.4.5 |
| G-K | "Ürünlere dönüş" yolunun tanımı | IC-2, DR-11 | 2.7.1.2'de tek tanım; 3.3.12 |

25 bulgu 11 grupta → 109 − 14 = **95 tekil kök**.

### Aksiyon Planı

- **Critical / High:** audit merceklerinde yok (iki yüksek bulgu durum ve yasal merceklerindedir — deep review raporu).
- **Medium → `04`:** v0.13'te kapandı — karar kaydının biçimi (KAPSAM-K-1), dijital üründe üretim yeri (FORM-7), sıfıra inen fiyat (FORM-11, EKRAN-P-7), kanuni faiz uyarısı (EKRAN-P-1), yanlış adrese davet (EKRAN-P-2), sayaç eşitliği (EKRAN-P-5), kapalı işlem listesi (EKRAN-P-6).
- **Medium → `02`, `03`:** `02` v0.63 §10.7.4 (K-836); `03` v0.19 §2.1.5 (hizalama), §2.8.1.2 (K-838). **→ sonraki dokümanlar:** on iki eksik park talimatı (KAPSAM-K-4) ve FORM-16'nın birim listesi — `05`, `06`, `07`, `12`'nin park blokları.
- **Low:** v0.13'te kapandı ya da kısmen uygulandı (gerekçeler tabloda). Beş bulgu uygulanmadı (kanıt şüphecisi çürüttü). Kalan tek iz: `02 §3.12.9` ile §6.2.9'un metin farkı — cross-review'ın etki yansıtması tarar.

### Mevzuat doğrulama günlüğü

Audit merceklerinin bulgularından yasal dayanağa yaslanan yalnız EKRAN-M-5'tir (m.13/1 — aşağıda); tam günlük deep review raporundadır. Uygulayan ajan Mesafeli Sözleşmeler Yönetmeliği'nin konsolide metnini mevzuat.gov.tr'den (GeneratePdf, mevzuatNo 20237) 2026-10-04'te kendisi indirip okudu: m.5/1-g, -h · m.6/1 · m.6/2-a · m.7/1 · m.13/1 — merceğin ve kanıt şüphecisinin aktardığı metinle birebir.

### Okunmayan ya da kesilerek okunan kısımlar (merceklerin beyanı)

- **KAPSAM-K:** karar ve etki hücreleri tam; gerekçe hücreleri yalnız bulguya dokunanlarda okundu — KAPSAM-K-7 ve K-9'u kanıt şüphecisinin çürütmesinin sebebi budur (kural gerekçenin Edge notundaydı).
- **DURUM:** `02 §5`'in iki uzun satırının sonu kesik; karar kaydından yalnız K-793 açıldı; ekran tanımlarının aksiyon maddeleri yalnız E-16…E-19 ve E-37'de tam okundu.
- **FORM:** `02` satırları 900–1600 karakterde kesildi (§3.14.6, §3.8.5, §7.4.5, §10.4.10'un sonu görülmedi); `03 §3.3` yalnız önizlemeyle; K-772, K-779, K-786, K-813, K-817, K-819…K-822'nin metinleri açılmadı.
- **ATIF:** §1 matrisi kapsam dışıdır; anlam denetimi 185 atıflık rastgele örneklemle (hepsi doğru) ve betikli denetimlerle; tırnak araması 02 + 03 + 10 + K satırlarında sözcük sınırlı birebir.
- **IC:** §1.1.3–§1.1.11 ve §1.3'ün satırları tek tek okunmadı; §5 ve §9 betikle tarandı, tam metinle okunanlar E-01…E-12, E-30, E-31, E-54; §6 ve §7 yalnız bölümler arası çelişki için örneklendi.
- **EKRAN-M:** `10 §2` KP satırları matris özetiyle; devir dizininin on dokuz K satırı açılmadı; E-20…E-28'in `03 §7.1.4x–§7.1.5x` satırları tam okunmadı — bu öğeler ✓ sayıldı ama ekran tanımının atfına dayanır.
- **EKRAN-P:** `02`'nin §3.2–§3.12, §5, §6.6–§6.9, §7, §9'u tam okunmadı (E-33 ve E-37'nin ✓ hükümleri `03` satırları üzerinden); KP-39, KP-46, KP-47, KP-48, KP-62, KP-64'ün sonu kesik; E-37'de betiğin 340 çifti ile brifin 333'ü arasındaki fark doğrulanmadı.

### Karar listesi

Audit merceklerinden doğan kararlar (ayrıntı karar kaydının §2'sinde; deep review merceklerinin kararları ikinci rapordadır):

| Karar | Bulgu | Özet | ⚠ |
|---|---|---|---|
| K-836 | IC-6 | Ulaşma listesi yalnız panelin dışa aktarma ekranından iner; `02 §10.7.4` hizalandı, K-514'e geri işaret | ⚠ |
| K-837 | EKRAN-M-1 | Sepete bağlı ödenmemiş önceki siparişin iptali onaydan önce söylenir | ⚠ |
| K-838 | EKRAN-M-5 | Kargodaki kalemden caymada iade bölümü önce kargonun malı döndürdüğünü söyler | ⚠ |
| K-845 | EKRAN-M-6 | Kupon reddinde yalnız asgari tutar ayrı mesaj | — |
| K-846 | KAPSAM-K-2, K-6, ATIF-1, ATIF-2 | Panelin dokuz düğme adı sabitlendi | — |

Karar gerektirmeyen düzeltmeler (karar kaydının biçimi, park satırları, atıflar, matris eşlemesi) K satırı açmadı; tablo yukarıdadır.

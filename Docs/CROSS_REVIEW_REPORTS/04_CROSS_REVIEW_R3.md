# Cross-Review — 04 UI Specs (Tur 3)

**Tarih:** 2026-10-04 · **Hedef:** `Docs/04_UI_SPECS.md` v0.15 (girdi commit'i `2eedcbe`, 847 241 bayt, 5184 satır) · **İkinci model:** Codex CLI — ChatGPT hesabı, `gpt-5.6-terra`, düşünme düzeyi yüksek (salt okuma) · **Ciddiyet ölçüsü:** 1. turdan itibaren uygulanır (K-847) — bu tur ölçüyle koştu · **Koşum:** iki biçimde, beş çağrı — tek parça çağrı ve dört parçalı kontrol koşumu (K-848); tur ancak beş çağrının beşi de `SONUÇ: TEMİZ` dönerse TEMİZ'dir · **Girdi:** yalnız doküman ve boş şablonu (`95a168e`), stdin'den tek metin, yeni, boş ve izole bir çalışma klasöründe; karar kaydı, önceki turların raporları, Ürün Gereksinimleri, Kullanıcı Akışları, MVP Kapsamı ve audit/deep review raporları verilmedi, önceki turlardan hiçbir şey taşınmadı (K-431)

## 1. Koşum

**İstem 1. ve 2. turla aynıdır** — `04_CROSS_REVIEW.md` §1'in alıntı bloğundan betikle çıkarıldı ve değiştirilmeden kullanıldı: yedi kriter, bulgu biçimi (`BULGU-N` — Kriter · Seviye · Yer · Alıntı · Sorun · Öneri — ya da `SONUÇ: TEMİZ`), `cross-review` skill'inin Faz 1 madde 4'teki bilinçli karar cümlesi, ciddiyet ölçüsünün paragrafı ve belgenin kural koymadığı notu. Parçalı çağrılara aynı rapordaki parça notu eklendi; `<bölümler>` yerine parçanın bölümleri yazıldı (`§1` · `§2–§4` · `§5` · `§6–§9`). Girdi `istem + "=== DOKÜMAN ===" + doküman + "=== ŞABLON ===" + git show 95a168e:Docs/04_UI_SPECS.md` biçimindedir; her parça başlık notunu (satır 1–77: sürüm notları, park blokları, on beş yazım konvansiyonu, alt bölüm haritası) taşır. Parça sınırları bölüm başlıklarıdır (`## 1.` · `## 2.`–`## 4.` · `## 5.` · `## 6.`–`## 9.`). Beş çağrı paralel koştu; her biri kendi boş klasöründe.

**Komut** (Git Bash; çalışma klasörü boş): `codex exec -s read-only -C <boş klasör> --skip-git-repo-check --ephemeral --ignore-user-config --color never -m gpt-5.6-terra -c 'model_reasoning_effort="high"' -o <çıktı> - < <girdi>`.

| Parça | Bölümler | Satırlar (`2eedcbe`) | Girdi | Token | Süre | Web araması | Sonuç |
|---|---|---|---|---|---|---|---|
| — | Tek parça — dokümanın tamamı | 1–5184 | 855 625 bayt | 409 047 | 130 sn | 1 | 1 bulgu |
| 1 | §1 izlenebilirlik matrisi | 78–2247 | 323 514 bayt | 118 606 | 68 sn | 0 | `SONUÇ: TEMİZ` |
| 2 | §2 ortak bileşenler · §3 navigasyon · §4 envanter | 2248–2935 | 139 072 bayt | 58 338 | 131 sn | 0 | 1 bulgu |
| 3 | §5 müşteri tarafının ekran tanımları | 2936–3518 | 166 843 bayt | 64 608 | 104 sn | 0 | `SONUÇ: TEMİZ` |
| 4 | §6 durum × rol · §7 form · §8 lokalizasyon · §9 panel ekranları | 3519–5184 | 329 651 bayt | 125 455 | 95 sn | 0 | `SONUÇ: TEMİZ` |

**Koşum kayıtlarından iki not.** (1) Tek parça çağrının kaydında 2. turdaki gibi bir kez `context compacted` satırı var: model web aramasından sonra kendi bağlamını özetledi ve 409 bin token kullandı. (2) Tek web araması tek parça çağrınındır: `site:ticaret.gov.tr Mesafeli Sözleşmeler Yönetmeliği cayma hakkı iade kargo bedeli tüketici 2026`; bulgusu bu aramadan doğdu (§3).

**Kapsam sınırı (1. turla aynı).** Parça öteki bölümleri görmez; parçalar arası çelişkiyi yalnız tek parça çağrı yakalar. Bütün iddialar dokümanın tamamına, Ürün Gereksinimleri'ne ve karar kaydına karşı okundu (§3).

## 2. Ham bulgular

İkinci modelin çıktıları, düzenlenmeden (alıntı blokları içinde; yalnız satır sonu boşlukları silindi).

**Tek parça çağrı:**

> ```text
> BULGU-1
> Kriter: Teknik doğruluk
> Seviye: Yüksek
> Yer: §5.18.6 — E-18 Cayma beyanı ekranı
> Alıntı: “teslimden sonraki caymada malın firmaya ulaşmasından işler”
> Sorun: Mevzuata aykırılık ve müşterinin geri ödeme hakkının gecikmesi riski vardır. Caymada, tüketici malı satıcının belirttiği taşıyıcıya teslim ettiğinde iade süresi başlar; doküman ise tüm teslim sonrası caymaları malın firmaya ulaşmasına bağlamaktadır. [Ticaret Bakanlığı açıklaması](https://tuketici.ticaret.gov.tr/yayinlar/tuketici-bilgi-rehberi/mesafeli-sozlesmeler-hakkinda-bilgilendirme)
> Öneri: İade taşıyıcısını ön bilgilendirmede açıkça belirtin; bu taşıyıcı kullanıldığında geri ödeme süresinin tüketicinin ürünü taşıyıcıya teslimiyle başladığını yazın. Başka taşıyıcı seçilirse başlangıcı teslim alma olarak koşullu belirtin.
>
> SONUÇ: 1 BULGU
> ```

**Parça 1 — §1:**

> ```text
> SONUÇ: TEMİZ
> ```

**Parça 2 — §2–§4:**

> ```text
> BULGU-1
> Kriter: Kullanıcı deneyimi
> Seviye: Yüksek
> Yer: §2.7.3.3; §3.3.21; §4.1 E-15
> Alıntı: “sipariş onayında numara henüz bilinmeyebileceği için üye sipariş geçmişine, misafir alıcı sipariş takibi girişine gider”
> Sorun: Misafir alıcı, numaranın henüz bilinmediği belirsiz sipariş sonucunda E-15’e gönderiliyor; ancak E-15 sipariş sayfasını açmak için sipariş numarası ve e-postanın eşleşmesini zorunlu tutuyor. Kullanıcı numarayı bilmediğinden sonucu doğrulayamaz ve akışta çıkışsız kalır. Bu, 3. sınıf ciddi sorundur.
> Öneri: Belirsiz sipariş sonucunda misafir alıcıyı sipariş numarası gerektirmeyen, sonucu bekleyen ve sonuç netleşince güvenli E-16 erişimi veren bir hâle yönlendir; E-15’e yönlendirmeyi ancak sipariş numarası kullanıcıya zaten verilmişse kullan.
>
> SONUÇ: 1 BULGU
> ```

**Parça 3 — §5:**

> ```text
> SONUÇ: TEMİZ
> ```

**Parça 4 — §6–§9:**

> ```text
> SONUÇ: TEMİZ
> ```

## 3. Bağımsız değerlendirme

Her bulgu dokümanın tamamına (v0.15), Ürün Gereksinimleri'ne (`02` v0.63) ve karar kaydına karşı okundu; parça 2'nin iddiası dokümanın bütününde arandı.

| # | Karar | Gerekçe | Uygulanan düzeltme |
|---|---|---|---|
| Tek parça · BULGU-1 | ❌ RET | **Mevzuata aykırılık yok; kural güncel resmî metne karşı doğrulanmış bir karardır ve dokümanın kendi içinde koşuluyla birlikte durur.** Mesafeli Sözleşmeler Yönetmeliği m.12/1 (RG 23/8/2022-31932 ile değişik, 1/1/2026'dan beri uygulanır) iki cümledir: satıcının ön bilgilendirmede belirttiği taşıyıcıya teslimde süre teslimden, *"öngörülenin haricinde bir taşıyıcı ile"* iadede malın satıcıya ulaşmasından işler. Firma iade taşıyıcısı belirlemez, taşıyıcıyı müşteri seçer (K-293; `02 §7.4.2`, K-492, K-493) — bu yüzden teslimden sonraki her iade ikinci cümleye girer ve süre malın firmaya ulaşmasıyla başlar (K-491; `02 §7.4.1`). Doküman bu koşulu kendi içinde de taşır: 5.18.6'nın hemen üstündeki 5.18.5 malın *"müşterinin seçtiği taşıyıcıyla, iade adresine karşı ödemeli"* gönderileceğini yazar. Modelin "tüm teslim sonrası caymaları" itirazı bu satırı görmemiştir; önerisi (firmanın iade taşıyıcısı belirtmesi) K-293'ün ve K-492'nin elediği seçenektir — iade masrafının firmada kalmasının yasal dayanağı da taşıyıcı belirtilmemesidir (m.12/5). **Modelin gösterdiği kaynak** (Ticaret Bakanlığı tüketici rehberi, 17 Ağustos 2026 tarihli sayfa; 2026-10-04'te açıldı) aynı ayrımı yazar: belirtilenden farklı kargoyla iadede süre *"ürünün satıcıya ulaştığı tarihte"* başlar. Taşıyıcı hiç belirtilmediğinde bu okumanın metnin açık hükmü değil lafzından çıkan bir sonuç olduğu kayıtta adıyla yazılı kalan bir risktir; avukat teyidi önerisi proje sahibine sunuldu ve reddedildi (K-721). Bulgu yeni bir bilgi taşımıyor; karar yeniden açılmaz ve çok kritik soru doğmaz. | Yok. |
| P2 · BULGU-1 | ❌ RET | **Çıkışı olmayan akış değil; çıkış parçanın kendi bölümünde yazılı.** 1. turun P3 · BULGU-1'inin aynısıdır (o tur somut yerlerle reddedildi); bu tur §5'i gören parça 3 bulguyu yazmadı, §5'i görmeyen parça 2 yazdı. (a) Sipariş onay anında doğduysa sipariş e-postasındaki bağlantı sipariş sayfasını e-posta sormadan açar — 3.5.1 (*"Sipariş e-postası · Sipariş sayfasının bağlantısı (erişim anahtarı) · E-16, e-posta sormadan"*; `02 §3.22.5`), parça 2'nin içindedir; E-15 bu yolu ekranın üstünde hatırlatır (5.15.3'ün (4). bölgesi). (b) 2.7.3.3 misafir alıcının yönlendirmesini 5.13.31'e bağlar; orada sepet ve yazılanlar durur; aynı sepetten yeniden onayda, sipariş doğmuşsa onay bölümü önceki ödenmemiş siparişin numarasını ve bu onayla iptal edileceğini önceden söyler (5.13.9 (a), 5.13.18; K-837) — çift sipariş doğmaz. (c) Belirsiz onay anında ödeme alınmamıştır (5.14.3, 5.14.6). Parça notu verilmeyen bölümde yaşayan içeriğin görünmemesini bulgu saymaz; modelin önerisi (denemeye bağlı bekleme hâli ve güvenli erişim) yeni bir mekanizmadır ve bilinçli kararı (K-745 — numara bilinmeyebileceği için misafir alıcı sipariş takibi girişine gider) gerekçesiz değiştirir. | Yok. |

**Dağılım:** 0 KABUL · 0 KISMİ · 2 RET (parça 1, 3 ve 4 TEMİZ). **%100 ret savunmacılık mı?** İki bulgu da yeniden okundu ve küçük bir düzeltme yolu arandı. Tek parça bulgusu için 5.18.6'ya "firma iade taşıyıcısı belirtmediği için" yan cümlesi düşünüldü; reddedildi: koşul bir satır üstte (5.18.5) yazılı, kural ayrıntısı evinde kalır (konvansiyon 12) ve `02 §7.4.1` de koşulu aynı biçimde §7.4.2'ye bırakır — eklemek `04`'ü `02`'den farklı bir metne taşırdı. Parça 2 bulgusu için 2.7.3.3'e e-posta bağlantısının işaretini eklemek düşünüldü; reddedildi: 2.7.3.3 ortak bileşenin kuralıdır ve ekrana özgü çıkışı 5.13.31'e bağlar, çıkışın kendisi parçanın içinde 3.5.1'de durur — 1. tur aynı iddiayı aynı gerekçeyle reddetmişti ve dokümanın o yerleri değişmedi. İki RET'in her biri bir karar satırına (K-491, K-293, K-721; K-745, K-837) ve dokümanın somut maddesine (5.18.5; 3.5.1, 5.13.9, 5.13.18, 5.15.3) dayanır. Bulgular bilinçli konvansiyonlara (1–15; K-726, K-762) dokunmuyor; 1. ve 2. turun KABUL ve KISMİ ile kapanan yedi bulgusundan hiçbiri geri dönmedi.

## 4. Ek bulgular

- **Aynı kalıbın taraması (tek parça · BULGU-1):** dokümanda geri ödeme süresinin başlangıcını anan öteki yerler — §1.1'in K-491 satırı (E-37, E-31, süre ve tarih bileşenleri), 9.9.27 (iade malını teslim alma), 2.12.5.1 (iade malının ulaşma tarihi) ve §6'nın cayma satırı 6.2.18.2 — aynı kuralı `02 §7.4.1`'e işaretle taşır; taşıyıcıya teslimden süre başlatan ya da firmanın iade taşıyıcısı belirlediğini söyleyen yer yok.
- **Aynı kalıbın taraması (P2 · BULGU-1):** misafir alıcının sipariş sayfasına giden yolları 3.3.19, 3.3.21, 3.5.1–3.5.3, 5.14 ve 5.15.3 aynı biçimde sayar; numarasız misafiri E-15'te bırakıp e-posta yolunu anmayan yer yok.
- **Mekanik tarama:** dokümanda anılan karar numaralarının hepsi karar kaydında satır olarak var; karar kaydında çift numara yok. Doküman bu turda değişmedi — sürüm v0.15 kalır, Kaynak satırlarına dokunulmadı.
- **Ciddi bir sorun görülmedi.** Bulguların çevresi (2.7.3, §3.3, §3.5, 5.13, 5.14, 5.15, 5.18 ve §1.1'in geri ödeme satırları) okunurken ölçünün dört sınıfından birine giren başka bir sorun saptanmadı.
- **Önceki turlardan açık kalanlar:** 1. ve 2. turun bulguları işlendi; 1. turun RET'i (P3 · BULGU-1) bu turda geri döndü ve yine reddedildi. Audit'in etki yansıtmaya bıraktığı iz — Ürün Gereksinimleri'nin `§3.12.9` ile §6.2.9 arasındaki "Bu ürünü daha önce aldınız" metin farkı — TEMİZ turunun Faz 5'inde kapanır; bu tur TEMİZ olmadığı için taşınır.

## 5. Kullanıcı onay checklist'i

K-723 kararıyla (Aşama 3 boyunca ⚠ öneriyle kayıt, CI yeşilse `docs:` PR merge yetkisi ve yönetici + alt ajan düzeni) alt ajan değerlendirdi. Bu turda doküman değişmedi (`04` v0.15 kalır), karar satırı açılmadı; ⚠ karar yok. Ürün Gereksinimleri'ne, Kullanıcı Akışları'na ve MVP Kapsamı'na dönen değişiklik yok, `04 §1.3`'ün geri besleme tablosuna satır girmedi.

- [x] Tek parça · BULGU-1 (ret — K-491'in kuralı m.12/1'in ikinci cümlesidir; taşıyıcıyı müşteri seçer, 5.18.5; kalan risk K-721'de kapandı)
- [x] P2 · BULGU-1 (ret — 1. turun reddedilen bulgusu; çıkış 3.5.1'de, 5.13.9, 5.13.18 ve 5.15.3'te)
- [ ] Etki yansıtma (Faz 5) — döngü TEMİZ döndükten sonra, son turun raporunda

**Hukuki kontrol** (2026-10-04): tek parça bulgusu yasal dayanaklıdır. Dayanak hükmün metni K-491'de resmî konsolide metinden (mevzuat.gov.tr, MevzuatNo 20237) alıntılıdır ve m.12/1'in iki cümlesini taşır; modelin gösterdiği Ticaret Bakanlığı sayfası (17 Ağustos 2026) aynı ayrımı yazar. Kuralın taşıyıcı hiç belirtilmediği hâle uygulanışı K-491'in kaydında ve K-721'de adıyla yazılı kalan risktir; proje sahibi doğrulama önerisini reddetti. Bu tur yeni hukuki içerik eklemedi.

**Yakınsama ölçüsü:** bulgu sayısı 4 → 4 → 2 (bu turda tek parça 1 · parçalar 0 · 1 · 0 · 0); kabul edilen ya da kısmen kabul edilen 3 → 4 → 0. Doküman bu turda büyümedi (847 241 bayt). İki bulgu da metin farkı değildir — biri bilinçli ve doğrulanmış bir hukuki karara, biri 1. turda reddedilmiş bir iddiaya döndü. Skill'in valfi (art arda dar kenar durumlara inen ve dokümanı büyüten kabuller) karşılanmıyor. **Sonraki turun girdisi değişmeyecek:** doküman v0.15 olarak kalır; 4. tur aynı metni okur ve iki koşumun birinde bu turun iki bulgusundan birinin yeniden dönmesi olasıdır. Bu bir yönetici kararıdır ve rapora not olarak yazıldı.

## 6. Sonuç ve 4. tur

**Sonuç: TEMİZ DEĞİL** (ölçünün dört sınıfında) — beş çağrının üçü TEMİZ, ikisi birer bulgu; ikisi de reddedildi, doküman değişmedi. Faz 5 (etki yansıtma) bu turda koşmaz; TEMİZ turunda koşar.

**4. tur** 1. turun raporundaki istemle (`04_CROSS_REVIEW.md` §1'in alıntı bloğu, değiştirilmeden) ve iki koşumla yürür (K-848): (1) tek parça — istem, `=== DOKÜMAN ===`, güncel `04`, `=== ŞABLON ===`, `95a168e` şablonu; (2) dört parça — aynı istem, parça notu (`04_CROSS_REVIEW.md` §1), başlık notu ve şablonla; parça sınırları bölüm başlıklarıdır (`## 1.` · `## 2.`–`## 4.` · `## 5.` · `## 6.`–`## 9.`). Önceki turların raporları, karar kaydı ve audit raporları verilmez (K-431). Tur, beş çağrının beşi de `SONUÇ: TEMİZ` dönerse TEMİZ'dir.

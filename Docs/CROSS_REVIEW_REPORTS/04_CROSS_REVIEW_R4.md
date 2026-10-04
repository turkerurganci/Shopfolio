# Cross-Review — 04 UI Specs (Tur 4)

**Tarih:** 2026-10-04 · **Hedef:** `Docs/04_UI_SPECS.md` v0.15 (girdi commit'i `2fc123b`, 847 241 bayt, 5184 satır — 3. turla aynı metin) · **İkinci model:** Codex CLI — ChatGPT hesabı, `gpt-5.6-terra`, düşünme düzeyi yüksek (salt okuma) · **Ciddiyet ölçüsü:** 1. turdan itibaren uygulanır (K-847) — bu tur ölçüyle koştu · **Koşum:** iki biçimde, beş çağrı — tek parça çağrı ve dört parçalı kontrol koşumu (K-848); tur ancak beş çağrının beşi de `SONUÇ: TEMİZ` dönerse TEMİZ'dir · **Girdi:** yalnız doküman ve boş şablonu (`95a168e`), stdin'den tek metin, yeni, boş ve izole bir çalışma klasöründe; karar kaydı, önceki turların raporları, Ürün Gereksinimleri, Kullanıcı Akışları, MVP Kapsamı ve audit/deep review raporları verilmedi, önceki turlardan hiçbir şey taşınmadı (K-431)

## 1. Koşum

**İstem 1.–3. turla aynıdır** — `04_CROSS_REVIEW.md` §1'in iki alıntı bloğundan (istem ve parça notu) betikle çıkarıldı ve değiştirilmeden kullanıldı; parça notunda `<bölümler>` yerine parçanın bölümleri yazıldı (`§1` · `§2–§4` · `§5` · `§6–§9`). Girdi `istem + "=== DOKÜMAN ===" + doküman + "=== ŞABLON ===" + git show 95a168e:Docs/04_UI_SPECS.md` biçimindedir; her parça başlık notunu (satır 1–77) taşır. Parça sınırları bölüm başlıklarıdır (`## 1.` · `## 2.`–`## 4.` · `## 5.` · `## 6.`–`## 9.`). Doküman 3. turda değişmediği için beş girdinin baytı 3. turunkiyle birebir aynıdır. Beş çağrı paralel koştu; her biri kendi boş klasöründe (klasörler koşumdan sonra da boştu).

**Komut** (Git Bash; çalışma klasörü boş): `codex exec -s read-only -C <boş klasör> --skip-git-repo-check --ephemeral --ignore-user-config --color never -m gpt-5.6-terra -c 'model_reasoning_effort="high"' -o <çıktı> - < <girdi>`.

| Parça | Bölümler | Satırlar (`2fc123b`) | Girdi | Token | Süre | Web araması | Sonuç |
|---|---|---|---|---|---|---|---|
| — | Tek parça — dokümanın tamamı | 1–5184 | 855 625 bayt | 315 156 | 65 sn | 0 | 1 bulgu |
| 1 | §1 izlenebilirlik matrisi | 78–2247 | 323 514 bayt | 124 096 | 120 sn | 0 | 1 bulgu |
| 2 | §2 ortak bileşenler · §3 navigasyon · §4 envanter | 2248–2935 | 139 072 bayt | 54 546 | 77 sn | 0 | `SONUÇ: TEMİZ` |
| 3 | §5 müşteri tarafının ekran tanımları | 2936–3518 | 166 843 bayt | 65 727 | 98 sn | 0 | 1 bulgu |
| 4 | §6 durum × rol · §7 form · §8 lokalizasyon · §9 panel ekranları | 3519–5184 | 329 651 bayt | 161 317 | 160 sn | 4 | 2 bulgu |

**Koşum kayıtlarından iki not.** (1) Tek parça çağrının kaydında bu turda `context compacted` satırı yok (2. ve 3. turda birer kez vardı); 315 bin token, 1. turun düzeyi. (2) Web aramaları yalnız parça 4'ündür: ETBİS kayıt yükümlülüğünün istisnaları (üç kez) ve KVKK m.10'un alıcı grupları (bir kez); BULGU-1 ikinci konudan doğdu (§3).

**Kapsam sınırı (1. turla aynı).** Parça öteki bölümleri görmez; parçalar arası çelişkiyi yalnız tek parça çağrı yakalar. Bütün iddialar dokümanın tamamına, Ürün Gereksinimleri'ne, MVP Kapsamı'na ve karar kaydına karşı okundu (§3).

## 2. Ham bulgular

İkinci modelin çıktıları, düzenlenmeden (alıntı blokları içinde; yalnız satır sonu boşlukları silindi).

**Tek parça çağrı:**

> ```text
> BULGU-1
> Kriter: Tutarlılık
> Seviye: Yüksek
> Yer: §2.12.3.1–§2.12.3.2; §5.16.12
> Alıntı: “Alan yalnız havale hattında görünür; kart hattında IBAN alanı yoktur ve istenmez.” / “kart iadesi gerçekleşmeyip havale yolu açıldığında … IBAN girmek müşterinin seçimidir”
> Sorun: Aynı bileşen kart hattında IBAN’ı kesin olarak yasaklarken, başarısız kart iadesinde havale yoluna geçmek için IBAN girişini zorunlu akış seçeneği olarak tanımlıyor. İlk kural uygulanırsa kart iadesi başarısız müşterinin havale iadesine geçişi engellenebilir; bu para iadesinin tamamlanamamasına yol açar.
> Öneri: §2.12.3.1’i, IBAN’ın normalde havale iadesinde kullanıldığını; ancak kart iadesi başarısız olup yönetici havale yolunu açtığında kart siparişinde de isteğe bağlı gösterildiğini açıkça belirtecek biçimde düzeltin. §5.16.12 ve §7.1.7 ile aynı ifadeyi kullanın.
>
> SONUÇ: 1 BULGU
> ```

**Parça 1 — §1:**

> ```text
> BULGU-1
> Kriter: Tutarlılık
> Seviye: Orta
> Yer: §1.1.8 — `03 §1.7.3.1` ve `03 §7.1.37`
> Alıntı: `"e-posta ulaşmadı" işareti siparişin — ayıp talebinde talebin de — ... satırına düşer`; `"yönetici "e-posta ulaşmadı" işaretli sipariş satırından e-postayı yeniden gönderir; işareti düşüren on altı müşteri bildiriminin hepsi yeniden gönderilir"`
> Sorun: Ayıp talebine ilişkin müşteri bildirimi B-9’dur ve işaretin talep satırında (E-38) durduğu belirtilmiştir; ancak yeniden gönderim yalnız işaretli sipariş satırından (E-36/E-37) tanımlanmıştır. Bu, aynı bildirimin başarısız tesliminde yöneticinin hangi yüzeyden yeniden gönderim yapacağını çelişkili bırakır.
> Öneri: Ayıp talebi B-9 için E-38’de yeniden gönderim aksiyonunu açıkça tanımlayın veya işareti ve yeniden gönderim aksiyonunu ilişkili sipariş satırında tek yerde toplayıp E-38’den bu satıra geçişi belirtin.
>
> SONUÇ: 1 BULGU
> ```

**Parça 2 — §2–§4:**

> ```text
> SONUÇ: TEMİZ
> ```

**Parça 3 — §5:**

> ```text
> BULGU-1
> Kriter: Kullanıcı deneyimi
> Seviye: Yüksek
> Yer: §5.13.31; §5.15.4
> Alıntı: "müşteriyi sonucu görmeye götürür — üyeyi sipariş geçmişine, misafir alıcıyı sipariş takibi girişine (§2.7.3.3); yazılanlar ve sepet durur."
> Sorun: Sonucu belirsiz kalan onaydan sonra misafir alıcı E-15’e gönderilir; ancak E-15 sipariş sayfasını açmak için sipariş numarası ve e-posta eşleşmesini zorunlu tutar. Onayın gerçekten sipariş üretip üretmediği belirsizken misafir alıcının sipariş numarası yoktur. Bu, müşteriyi sonucu doğrulayamadığı bir akışta bırakır ve yeniden sipariş vererek mükerrer sipariş/ödeme riski doğurur.
> Öneri: Onay isteğine bağlı, tek kullanımlık ve süreli bir sonuç erişim anahtarı oluşturun. Belirsiz sonuçta misafir alıcı bu anahtarla doğrudan mevcut siparişin E-16 durumuna veya “sipariş oluşmadı” sonucuna götürülmelidir; E-15’te sipariş numarası istenmemelidir.
>
> SONUÇ: 1 BULGU
> ```

**Parça 4 — §6–§9:**

> ```text
> BULGU-1
> Kriter: Güvenlik
> Seviye: Yüksek
> Yer: §9.19.3, §9.19.7
> Alıntı: “**alıcı grupları** bölümünü firma adıyla doldurur — **kapıya girmez**”
> Sorun: Aydınlatma metninin yayımlanması ve veri toplayan girişlerin açılması, alıcı grupları doldurulmadan mümkün kalıyor. KVKK kapsamında aktarımın amacı ve alıcı grupları aydınlatmanın zorunlu unsurlarıdır; bu boşluk, eksik aydınlatmayla kişisel veri toplanmasına yol açar. [KVKK açıklaması](https://www.kvkk.gov.tr/Icerik/6765/AYDINLATMA-YUKUMLULUGUNUN-YERINE-GETIRILMESI-HAKKINDA-KAMUOYU-DUYURUSU)
> Öneri: Alıcı gruplarını, aydınlatma metninin tamamlanma ve yayına alma kapısına dahil edin; eksik veya yalnız yer tutuculuysa veri toplayan girişleri açmayın.
>
> BULGU-2
> Kriter: Tutarlılık
> Seviye: Orta
> Yer: Yazım konvansiyonu 4; §9.3.2
> Alıntı: “Ekran başına tek birincil düğme vardır (§2.1.3).” / “Ekranın birincil düğmesi yoktur”
> Sorun: Konvansiyon her ekran için tam olarak bir birincil düğme zorunlu kılarken panel ana sayfası açıkça hiç birincil düğme olmadığını söyler. Aynı istisna liste ve rapor ekranlarında da kullanıldığından uygulama ekibinin hangi kuralı izleyeceği belirsizdir; bu, doküman içi çelişkidir.
> Öneri: Konvansiyonu “birincil eylemi olan ekranlarda tek birincil düğme vardır” diye değiştirin ve dashboard/liste/rapor ekranlarının istisna olduğunu açıkça tanımlayın.
>
> SONUÇ: 2 BULGU
> ```

## 3. Bağımsız değerlendirme

Her bulgu dokümanın tamamına (v0.15), Ürün Gereksinimleri'ne (`02` v0.63), MVP Kapsamı'na (`10` v0.41) ve karar kaydına karşı okundu.

| # | Karar | Gerekçe | Uygulanan düzeltme |
|---|---|---|---|
| Tek parça · BULGU-1 | ✅ KABUL | **Ortak bileşenin iki maddesi birbirini tutmuyordu.** 2.12.3.1 *"Alan yalnız havale hattında görünür; kart hattında IBAN alanı yoktur ve istenmez"* diye mutlak yazıyordu; hemen altındaki 2.12.3.2 ve 5.16.12 ise kart iadesi sağlayıcıda gerçekleşmeyip firma havale yolunu açtığında alanın kart siparişinin sipariş sayfasında açıldığını ve IBAN girmenin müşterinin seçimi olduğunu yazar (`02 §7.4.5`, §7.2.8; K-521, K-569); 7.1.7.1 ve 6.2.16.20 de aynı hâli taşır. `02 §7.4.5`'te "kart hattında IBAN alanı yoktur ve istenmez" cümlesi beyan sırasındaki girişe bağlıdır ve istisna aynı paragrafta yazılıdır; 2.12.3.1 cümleyi bu bağlamından ayırmıştı (ölçünün (4). sınıfı). Modelin etkisi — müşterinin havale yoluna geçişinin "engellenebileceği" — abartılı: yol 2.12.3.2, 5.16.12, 6.2.16.20 ve 7.1.7.1'de yazılıdır. Düzeltme önerinin kendisidir; ifade 5.16.12'ye hizalandı. | 2.12.3.1: *"Alan havale hattında görünür; kart hattında IBAN alanı yoktur ve istenmez — tek istisna kart iadesinin sağlayıcıda gerçekleşmeyip firmanın havale yolunu açtığı kalemdir: alan orada sipariş sayfasında açılır ve isteğe bağlıdır (2.12.3.2; K-569)."* K-569 bölümün Kaynak satırında zaten var. Yeni karar yok. |
| P1 · BULGU-1 | ❌ RET | **Çelişki yok; ayıp talebinin bildirimi ulaşmadığında işaret iki yere düşer ve yeniden gönderim siparişten yapılır.** Modelin alıntıladığı satır (`03 §1.7.3.1`) işaretin *"siparişin — ayıp talebinde talebin de — … satırına"* düştüğünü söyler: sipariş satırına ve ek olarak talebin satırına, birinin yerine değil. Yeniden gönderim işaretin sipariş ayrıntısındaki listesindedir — her ulaşmayan e-postanın satırında "Yeniden gönder" (2.5.3.1'in E-37 sütunu; 9.9.4) —; talebin satırındaki işaret yalnız gösterimdir (2.5.3.1'in "Başka yerde" sütunu; 9.10.4) ve ayıp talebinin satırı E-37'nin ayıp talepleri bölümüne götürür (9.10.1, 9.10.9). Yönetici için iki yüzey yoktur. Parça §2 ve §9'u görmedi; §1'in iki satırı kendi içinde de çelişmiyor. Önerinin ikinci yolu — işaret ve yeniden gönderim sipariş satırında, E-38'den geçiş — dokümanda zaten yazılıdır. | Yok. |
| P2 | — | `SONUÇ: TEMİZ`. | — |
| P3 · BULGU-1 | ❌ RET — **tekrar eden RET** | **1. turun P3 · BULGU-1'i ve 3. turun P2 · BULGU-1'i aynı yer ve aynı iddiayla üçüncü kez döndü** (5.13.31 — belirsiz onaydan sonra numarasını bilmeyen misafir alıcının sipariş takibi girişinde "çıkışsız" kalması; öneri yine onaya bağlı tek kullanımlık erişim anahtarı). Gerekçe 1. turunkiyle aynıdır (`04_CROSS_REVIEW.md` §3; `04_CROSS_REVIEW_R3.md` §3): (a) sipariş onay anında doğduysa sipariş e-postasındaki bağlantı sipariş sayfasını e-posta sormadan açar (3.5.1; `02 §3.22.5`) ve E-15 bu yolu ekranın üstünde hatırlatır (5.15.3'ün (4). bölgesi); (b) sepet ve yazılanlar durur, aynı sepetten yeniden onayda sipariş doğmuşsa onay bölümü önceki ödenmemiş siparişin numarasını ve bu onayla iptal edileceğini önceden söyler (5.13.9 (a), 5.13.18; K-837) — çift sipariş doğmaz; (c) belirsiz onay anında ödeme alınmamıştır (5.14.3, 5.14.6). Bu kez §5'i gören parça yazdı; dokümanın o yerleri 1. turdan beri değişmedi. Öneri yeni bir mekanizmadır ve bilinçli kararı (K-745) gerekçesiz değiştirir. | Yok. |
| P4 · BULGU-1 | ⚠️ KISMİ | **Mevzuata aykırılık yok, ama 9.19.3'ün cümlesi kararın dayanağını taşımıyordu.** Alıcı gruplarının adıyla doldurulmasının kapıya girmemesi bilinçli bir karardır: aydınlatma taslağının alıcı grupları bölümü grupları kategori olarak zaten sayar — kargo ve teslimat, fatura ve muhasebe, ödeme kuruluşu, barındırma ve e-posta altyapısı, bakım ve destek (`02 §3.33.2`; K-553) — ve KVKK m.10/1-c ile Aydınlatma Tebliği alıcıyı grup olarak ister; adlarla doldurmak firmanın sorumluluğudur (`02 §3.1.5`, §12.5; `10 §4.1` ÖK-10; K-576). Aynı öneri — adları kapı koşulu yapmak — MVP Kapsamı'nın cross-review'ının 6. turunda da gelmiş ve uygulanmamıştı (`10` v0.17'nin sürüm notu). **Modelin gösterdiği kaynak** (KVKK kamuoyu duyurusu, 26 Haziran 2020; 2026-10-04'te açıldı) alıcıyı da *"alıcı grubu ya da grupları"* diye anar; kategori okumasını destekler. Ama `04`'ün cümlesi — *"alıcı grupları bölümünü firma adıyla doldurur — kapıya girmez"* — bölümün taslakta boş geldiği izlenimini veriyordu; kapının hukuki yeterliliği tam bu bilgiye dayanır. Öneri (alıcı adlarını kapıya almak) K-576'nın elediği seçenektir, reddedildi; cümleye kararın dayanağı eklendi. | 9.19.3: *"**alıcı grupları** bölümü grupları taslakta kategori olarak zaten anar, firma onları adıyla doldurur — adlar kapıya girmez (`02 §3.1.5`, §3.33.2)"*. Yeni karar yok. |
| P4 · BULGU-2 | ✅ KABUL | **Konvansiyon 4'ün cümlesi birçok ekranın yazdığıyla çelişiyordu.** Konvansiyon 4 *"Ekran başına tek birincil düğme vardır"* diyordu; 5.2.2, 5.3.2, 5.5.2, 9.3.2, 9.8.2, 9.10.2, 9.22.2, 9.23.2 gibi yirmiden fazla "İlk görülen" maddesi birincil düğmenin olmadığını açıkça yazar ve dayandığı §2.1.3 bir üst sınırdır (*"ekranın birincil eylem düğmesi — ekran başına bir tane"*, K-742). Niyet "en çok bir"dir; konvansiyonun metni "tam bir" diye okunuyordu (ölçünün (4). sınıfı, dar bir metin farkı). Önerinin "dashboard/liste/rapor istisnası" kısmı gereksiz — her ekran kendi maddesinde yazar. | Konvansiyon 4: *"Ekran başına en çok bir birincil düğme vardır; olmayan ekran bunu yazar (§2.1.3)."* Yeni karar yok. |

**Dağılım:** 2 KABUL · 1 KISMİ · 2 RET (parça 2 TEMİZ). **%100 kabul ya da ret değil:** iki KABUL'ün ikisinde de modelin etki iddiası abartılı bulundu ve düzeltme en küçük biçimde uygulandı; KISMİ'de hukuki iddia ve önerinin kendisi reddedildi; iki RET somut yerlere (2.5.3.1, 9.9.4, 9.10.9; 3.5.1, 5.13.9, 5.13.18, 5.15.3) ve karar satırlarına (K-745, K-837) dayanır. Üç düzeltme de bir cümleyi aynı dokümandaki ya da `02`'deki kurala hizalar; yeni yetenek, yeni alan, yeni satır ya da yeni kural eklemez. Bulgular bilinçli konvansiyonların içeriğine dokunmuyor (konvansiyon 4'ün yalnız ifadesi düzeldi); 1.–3. turun KABUL ve KISMİ ile kapanan yedi bulgusundan hiçbiri geri dönmedi.

## 4. Ek bulgular

- **Aynı kalıbın taraması (tek parça · BULGU-1):** "IBAN alanı yoktur", "IBAN istenmez" ve "IBAN yok" geçen öteki yerler — 5.17.4, 6.2.17.1 (ödeme onaylanmamış iptalde IBAN yok), 7.1.8.3 (beyanla giriş — kart hattında yok), 6.3.9.18 ve 9.9.21 (havale hattında IBAN bekleniyor) — beyan ya da havale bağlamındadır; kart hattının istisnasını dışlayan başka mutlak cümle yok.
- **Aynı kalıbın taraması (P4 · BULGU-2):** "tek birincil düğme" 5.13.2 ve 5.28.2'de geçer; ikisi de o ekranın tek düğmesini anlatır, kural değildir. §2.1.3'ün "ekran başına bir tane"si marka renginin yerlerini sayar ve üst sınır olarak okunur; değişmedi.
- **Aynı kalıbın taraması (P4 · BULGU-1):** "alıcı grup" dokümanda yalnız 9.19.3'te geçer.
- **Aynı kalıbın taraması (P1 · BULGU-1):** "e-posta ulaşmadı" işaretinin talep satırını anan yerler — 2.5.3.1, 6.3.10.1, 9.10.4 — işareti gösterim olarak taşır ve satırdan E-37'ye geçişi yazar; talep satırında yeniden gönderim düğmesi söyleyen yer yok.
- **Mekanik tarama:** düzeltmeler gövdeye yeni K numarası eklemedi (2.12.3.1'in K-569'u §2.12.3'ün Kaynak satırında zaten var). Sürüm başlığı, başlık notunun yeni sürüm notu ve dosya sonu dipnotu v0.16'yı gösteriyor; karar kaydında çift numara yok.
- **Ciddi bir sorun görülmedi.** Bulguların çevresi (2.12.3, 5.16, 7.1.7–7.1.8, 2.5.3–2.5.4, 9.10, 9.19, konvansiyonlar) okunurken ölçünün dört sınıfından birine giren başka bir sorun saptanmadı.
- **Önceki turlardan açık kalanlar:** 1.–3. turun bulguları işlendi; 1. turun RET'i (P3 · BULGU-1) bu turda üçüncü kez döndü ve yine reddedildi (tekrar eden RET). Audit'in etki yansıtmaya bıraktığı iz — Ürün Gereksinimleri'nin `§3.12.9` ile §6.2.9 arasındaki "Bu ürünü daha önce aldınız" metin farkı — TEMİZ turunun Faz 5'inde kapanır; bu tur TEMİZ olmadığı için taşınır.

## 5. Kullanıcı onay checklist'i

K-723 kararıyla (Aşama 3 boyunca ⚠ öneriyle kayıt, CI yeşilse `docs:` PR merge yetkisi ve yönetici + alt ajan düzeni) alt ajan değerlendirdi ve uyguladı (`04` v0.16). Bu turda karar satırı açılmadı; ⚠ karar yok. Ürün kuralına dokunan değişiklik yok; Ürün Gereksinimleri'ne, Kullanıcı Akışları'na ve MVP Kapsamı'na dönen değişiklik yok, `04 §1.3`'ün geri besleme tablosuna satır girmedi.

- [x] Tek parça · BULGU-1 (kabul — 2.12.3.1 kart hattının tek istisnasını yazar; K-569)
- [x] P1 · BULGU-1 (ret — işaret sipariş ve talep satırında, yeniden gönderim siparişte; 2.5.3.1, 9.9.4, 9.10.9)
- [x] P3 · BULGU-1 (tekrar eden ret — 1. ve 3. turun bulgusu; çıkış 3.5.1'de, 5.13.9, 5.13.18 ve 5.15.3'te)
- [x] P4 · BULGU-1 (kısmi — 9.19.3 alıcı gruplarının taslakta kategori olarak yazılı olduğunu söyler; adları kapıya alma önerisi K-576 gereği reddedildi)
- [x] P4 · BULGU-2 (kabul — konvansiyon 4 "en çok bir birincil düğme" der)
- [ ] Etki yansıtma (Faz 5) — döngü TEMİZ döndükten sonra, son turun raporunda

**Hukuki kontrol** (2026-10-04): P4 · BULGU-1 yasal dayanaklıdır (KVKK m.10/1-c, alıcı grupları). Kural `02 §3.1.5`'te ve `10 §4.1` ÖK-10'da gerekçesiyle yazılıdır (K-553, K-576) ve MVP Kapsamı'nın 6. cross-review turunda aynı itirazla sınanmıştı; modelin gösterdiği KVKK duyurusu (26 Haziran 2020) alıcıyı grup olarak anar. Kural değişmedi; 9.19.3'e eklenen yan cümle yeni hukuki içerik değil, `02 §3.1.5`'in gerekçesinin özetidir. Öteki bulgularda yasal iddia yok; ETBİS aramalarından bulgu doğmadı.

**Yakınsama ölçüsü:** bulgu sayısı dört turda 4 → 4 → 2 → 5; kabul edilen ya da kısmen kabul edilen 3 → 4 → 0 → 3. Bu turun beş bulgusundan ikisi yeni metin farkıdır (2.12.3.1, konvansiyon 4), biri yeni bir yerde bilinçli bir hukuki karara döner (9.19.3 — kısmen kabul, karar değişmedi), biri parçanın görmediği bölümde yazılı olanı eksik sayar (P1), biri tekrar eden RET'tir (P3). Kabul edilen üç düzeltme dar kenar duruma inmiyor ve yüzey eklemiyor: doküman 847 241 bayttan 848 470 bayta büyüdü (+1,2 KB), artışın büyük kısmı başlıktaki sürüm notudur; tablolara satır girmedi. Skill'in valfi (art arda iki turda dar kenar durumlara inen ve dokümanı büyüten kabuller) karşılanmıyor. Gözlem: model her turda dokümanın farklı bir yerinde bir metin farkı buluyor — 845 KB'lık bir metinde örnekleme davranışı; aynı iddianın dönüşü (P3) yalnız bir yerdedir ve üç turdur.

## 6. Sonuç ve 5. tur

**Sonuç: TEMİZ DEĞİL** (ölçünün dört sınıfında) — beş çağrının biri TEMİZ, dördü toplam beş bulgu; üçü uygulandı (iki KABUL, bir KISMİ), ikisi reddedildi (biri tekrar eden RET). Faz 5 (etki yansıtma) bu turda koşmaz; TEMİZ turunda koşar.

**5. tur** 1. turun raporundaki istemle (`04_CROSS_REVIEW.md` §1'in alıntı bloğu, değiştirilmeden) ve iki koşumla yürür (K-848): (1) tek parça — istem, `=== DOKÜMAN ===`, güncel `04` (v0.16), `=== ŞABLON ===`, `95a168e` şablonu; (2) dört parça — aynı istem, parça notu (`04_CROSS_REVIEW.md` §1), başlık notu ve şablonla; parça sınırları bölüm başlıklarıdır (`## 1.` · `## 2.`–`## 4.` · `## 5.` · `## 6.`–`## 9.`) — v0.16'da başlık notu iki satır uzadı (satır 1–79). Önceki turların raporları, karar kaydı ve audit raporları verilmez (K-431). Tur, beş çağrının beşi de `SONUÇ: TEMİZ` dönerse TEMİZ'dir. P3 · BULGU-1'in dördüncü kez dönmesi olasıdır; bu rapor onu tekrar eden RET olarak işaretledi ve sonraki adımın kararı yöneticiye bırakıldı.

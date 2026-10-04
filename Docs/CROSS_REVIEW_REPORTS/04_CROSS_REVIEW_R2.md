# Cross-Review — 04 UI Specs (Tur 2)

**Tarih:** 2026-10-04 · **Hedef:** `Docs/04_UI_SPECS.md` v0.14 (girdi commit'i `d1807bc`, 845 849 bayt, 5182 satır) · **İkinci model:** Codex CLI — ChatGPT hesabı, `gpt-5.6-terra`, düşünme düzeyi yüksek (salt okuma) · **Ciddiyet ölçüsü:** 1. turdan itibaren uygulanır (K-847) — bu tur ölçüyle koştu · **Koşum:** iki biçimde, beş çağrı — tek parça çağrı ve dört parçalı kontrol koşumu (K-848); tur ancak beş çağrının beşi de `SONUÇ: TEMİZ` dönerse TEMİZ'dir · **Girdi:** yalnız doküman ve boş şablonu (`95a168e`), stdin'den tek metin, yeni, boş ve izole bir çalışma klasöründe; karar kaydı, 1. turun raporu, Ürün Gereksinimleri, Kullanıcı Akışları, MVP Kapsamı ve audit/deep review raporları verilmedi, önceki turdan hiçbir şey taşınmadı (K-431)

## 1. Koşum

**İstem 1. turla aynıdır** — `04_CROSS_REVIEW.md` §1'in alıntı bloğundan betikle çıkarıldı ve değiştirilmeden kullanıldı: yedi kriter, bulgu biçimi (`BULGU-N` — Kriter · Seviye · Yer · Alıntı · Sorun · Öneri — ya da `SONUÇ: TEMİZ`), `cross-review` skill'inin Faz 1 madde 4'teki bilinçli karar cümlesi, ciddiyet ölçüsünün paragrafı ve belgenin kural koymadığı notu. Parçalı çağrılara aynı rapordaki parça notu eklendi; `<bölümler>` yerine parçanın bölümleri yazıldı (`§1` · `§2–§4` · `§5` · `§6–§9`). Girdi `istem + "=== DOKÜMAN ===" + doküman + "=== ŞABLON ===" + git show 95a168e:Docs/04_UI_SPECS.md` biçimindedir; her parça başlık notunu (satır 1–75: sürüm notları, park blokları, on beş yazım konvansiyonu, alt bölüm haritası) taşır. Parça sınırları bölüm başlıklarıdır (`## 1.` · `## 2.`–`## 4.` · `## 5.` · `## 6.`–`## 9.`). Beş çağrı paralel koştu.

**Komut** (Git Bash; çalışma klasörü boş): `codex exec -s read-only -C <boş klasör> --skip-git-repo-check --ephemeral --ignore-user-config --color never -m gpt-5.6-terra -c 'model_reasoning_effort="high"' -o <çıktı> - < <girdi>`.

| Parça | Bölümler | Satırlar (`d1807bc`) | Girdi | Token | Süre | Web araması | Sonuç |
|---|---|---|---|---|---|---|---|
| — | Tek parça — dokümanın tamamı | 1–5182 | 854 233 bayt | 408 935 | 116 sn | 1 | 2 bulgu |
| 1 | §1 izlenebilirlik matrisi | 76–2245 | 322 504 bayt | 120 799 | 35 sn | 0 | 1 bulgu |
| 2 | §2 ortak bileşenler · §3 navigasyon · §4 envanter | 2246–2933 | 138 171 bayt | 76 902 | 79 sn | 2 | `SONUÇ: TEMİZ` |
| 3 | §5 müşteri tarafının ekran tanımları | 2934–3516 | 165 942 bayt | 65 570 | 68 sn | 0 | `SONUÇ: TEMİZ` |
| 4 | §6 durum × rol · §7 form · §8 lokalizasyon · §9 panel ekranları | 3517–5182 | 328 368 bayt | 126 010 | 111 sn | 0 | 1 bulgu |

**Koşum kayıtlarından iki not.** (1) Tek parça çağrının kaydında bir kez `context compacted` satırı var: model okuma sırasında kendi bağlamını özetledi ve 409 bin token kullandı (1. turda 315 bin token, 59 saniye, sıkıştırma yok). Bu turda tek parça çağrı iki bulgu getirdi; ikisi de §6 ve §9'un içindedir ve parça 4'ün görebileceği yerlerdir — parça 4 onları yazmadı, o da tek parça çağrının yazmadığı bir bulgu getirdi. İki koşumun birbirini tamamladığı (K-848) bu turda da görüldü. (2) Web aramaları: tek parça çağrı ETBİS kayıt yükümlülüğünün istisnalarını, parça 2 e-Arşiv faturada nihai tüketicinin T.C. kimlik numarasını (iki kez) aradı; aramalardan bulgu doğmadı.

**Kapsam sınırı (1. turla aynı).** Parça öteki bölümleri görmez; parçalar arası çelişkiyi yalnız tek parça çağrı yakalar. Bütün iddialar dokümanın tamamına, Ürün Gereksinimleri'ne ve Kullanıcı Akışları'na karşı okundu (§3).

## 2. Ham bulgular

İkinci modelin çıktıları, düzenlenmeden (alıntı blokları içinde; yalnız satır sonu boşlukları silindi).

**Tek parça çağrı:**

> ```text
> BULGU-1
> Kriter: tutarlılık
> Seviye: Yüksek
> Yer: §9.9.18; §6.3.9.7
> Alıntı: "‘Teslim edilemedi olarak işaretle’ onay istemez; sonuç S7 — kapanışı tamamlıyorsa S11 —, B-6"
> Sorun: S11 dokümanın başka yerinde “Teslim edilemedi → Teslim edildi” geçişidir; oysa teslim edilemeyen siparişin kapanışı S9 olarak tanımlanmıştır. Aynı işlemin siparişi teslim edilmişe mi yoksa kapatılmışa mı taşıdığı çelişkilidir (4). Yanlış durum, müşterinin teslim/iptal ve buna bağlı iade haklarını hatalı gösterebilir.
> Öneri: §9.9.18’deki “S11”i S9 ile değiştirin; teslim edilemedi durumundaki kapanışın her ilgili matris ve ekranda yalnız S9 olduğu açıkça yazılsın.
>
> BULGU-2
> Kriter: belirsizlik
> Seviye: Yüksek
> Yer: §6.2.16.9–§6.2.16.10
> Alıntı: "İptal edildi + Ödendi · İptal edildi + Kısmen geri ödendi — geri ödemenin işlenmesi bekleniyor" / "İptal edildi + Geri ödendi · İptal edildi + Kısmen geri ödendi — kalıcı son hâl"
> Sorun: Aynı “İptal edildi + Kısmen geri ödendi” durumu bir yerde IBAN girilebilen ve geri ödeme bekleyen hâl, başka yerde hiçbir işlem sunulmayan kalıcı son hâl olarak tanımlanmıştır. Ayıran koşul belirtilmediğinden müşterinin kalan alacağını talep edemediği bir akış doğabilir (2, 4).
> Öneri: Matriste bu iki hâli “geri ödeme borcu kaldı” ve “geri ödeme borcu kalmadı” koşullarıyla ayırın. Kalan borç varsa IBAN ve geri ödeme işlemlerini açık; yoksa yalnız belge görüntülemeyi açık bırakın.
>
> SONUÇ: 2 BULGU
> ```

**Parça 1 — §1:**

> ```text
> BULGU-1
> Kriter: Tutarlılık
> Seviye: Orta
> Yer: §1.1.10, `03 §7.1.37` satırı; §1.3 K-759
> Alıntı: "Yeniden gönderilebilen e-postaların kapsamı — firma bildirimleri dahil"
> Sorun: Bu, K-759’un yeniden gönderimi yalnız müşteri bildirimleriyle (`B-1…B-16`) sınırlayan kararıyla çelişir. Aynı dokümanda firma bildirimlerinin satıra işaret düşürmediği belirtilmektedir. Çelişki, panelde firma bildirimlerinin de yeniden gönderilmesine yönelik hatalı bir işlev üretebilir.
> Öneri: §1.1.10’daki özeti, “yeniden gönderilebilen müşteri bildirimlerinin kapsamı” olarak düzeltin; firma bildirimleri için yalnız ana sayfadaki kanal uyarısının bulunduğunu açıkça koruyun.
>
> SONUÇ: 1 BULGU
> ```

**Parça 2 — §2–§4:**

> ```text
> SONUÇ: TEMİZ
> ```

**Parça 3 — §5:**

> ```text
> SONUÇ: TEMİZ
> ```

**Parça 4 — §6–§9:**

> ```text
> BULGU-1
> Kriter: Güvenlik
> Seviye: Orta
> Yer: §6.3.1.2, §6.3.1.4 / §7.3.3
> Alıntı: "Hâl: e-posta ya da şifre hatalı · L-1 aşıldı ... Yeniden denemek" ve "Sıfırlama isteği — gönderildi · L-2 aşıldı ... Bağlantıyı istemek"
> Sorun: §7.3.3, limit aşılınca ilgili işlemin geçici olarak engellendiğini söyler. Buna karşın matrisin “Hangi aksiyonlar aktif” sütunu L-1 aşılmışken yeniden giriş denemesini, L-2 aşılmışken sıfırlama bağlantısı istemeyi aktif gösterir. Bu, limitlerin panel girişinde aşılabilmesine ve parola sıfırlama/e-posta gönderim hattının kötüye kullanılmasına yol açabilecek doğrudan çelişkidir.
> Öneri: L-1 ve L-2 aşılmış durumlarını ayrı satırlara ayırın; ilgili giriş ya da sıfırlama eylemini “sunulmaz / engel kalkınca” olarak belirtin. Yalnız limit dışı satırlarda “yeniden denemek” ve “bağlantıyı istemek” aktif kalsın.
>
> SONUÇ: 1 BULGU
> ```

## 3. Bağımsız değerlendirme

Her bulgu dokümanın tamamına (v0.14), Ürün Gereksinimleri'ne (`02` v0.63), Kullanıcı Akışları'na (`03` v0.19) ve karar kaydına karşı okundu.

| # | Karar | Gerekçe | Uygulanan düzeltme |
|---|---|---|---|
| Tek parça · BULGU-1 | ⚠️ KISMİ | **Çelişki yok, ama 9.9.18'in kısa ifadesi aynı paragraftaki "kapanış" sözcüğüyle karışıyordu.** S9 ve S11 iki ayrı geçiştir (`02 §5.4`): S11 (Teslim edilemedi → Teslim edildi) geri dönen gönderinin fiziksel kalemleri kapanmışken **teslim edilmiş bir dijital ya da hizmet kalemi** varsa işler; S9 kapanışı ("Siparişi kapat") **hiçbir kalemi teslim edilmemiş** siparişindir (K-717). `03 §8.3.2.5` ayrımı açıkça yazar: *"Teslim edilmiş bir kalem varsa sipariş S7'yle Teslim edildi'ye geçmiştir (8.2.7) ve kapanış gerekmez."* 9.9.18'in ikinci cümlesi ve 6.3.9.7 aynı koşulu taşır. Modelin iddiası — "aynı işlem siparişi teslim edilmişe mi kapatılmışa mı taşıyor" — bu yüzden doğru değil; önerisi (S11'i S9 yapmak) `02 §5.4`'ün beyaz listesini bozar, reddedildi. Ama 9.9.18 S11'in koşulunu *"kapanışı tamamlıyorsa"* diye yazıyordu ve aynı paragraf "kapanış"ı S9 için kullanır; tek başına okunan cümle iki geçişi karıştırır (ölçünün (4). sınıfında dar bir metin farkı). | 9.9.18: *"sonuç S7 — teslim edilmiş bir kalem varken açık kalem bırakmıyorsa sipariş aynı anda Teslim edildi'ye geçer (S11; `03 §8.2.7`, §8.3.2.5) —, B-6"*. Yeni karar yok. |
| Tek parça · BULGU-2 | ⚠️ KISMİ | **Ayıran koşul `03 §1.4.1`'in h notunda yazılı, ama 6.2.16.10 onu adıyla söylemiyordu.** h notu "İptal edildi + Kısmen geri ödendi"nin iki hâlini ayırır: geri ödemenin kalanı bekleniyor · gidiş kargosu geri ödenmeyen "gönderi teslim edilemedi — müşteri kaynaklı" iptali, kalıcı son hâl (`02 §7.2.9`). 6.2.16.9 ve 6.2.16.10 aynı ayrımı "bekleniyor" ve "kalıcı son hâl" etiketleriyle ve h notuna atıfla yapıyordu; kalıcı hâlin hangi iptal olduğu yazılı değildi. Modelin **hak kaybı iddiası reddedildi:** IBAN alanı durum etiketine değil sistemin IBAN isteğine bağlıdır — istek doğunca kalemin satırında açılır ve nedenini söyler (5.16.12; `02 §7.4.5`); kalıcı hâlde ödenmemiş alacak yoktur, gidiş kargosunun geri ödenmemesi kuraldır (`02 §7.2.9`). Önerinin "borç kaldı / kalmadı" ayrımı etiketlere koşul olarak yazıldı; yeni satır açılmadı. | 6.2.16.9: *"— geri ödemenin işlenmesi ya da kalanı bekleniyor"* · 6.2.16.10: *"— kalıcı son hâl: bekleyen geri ödeme yoktur; kısmen geri ödenmiş son hâl, gidiş kargosu geri ödenmeyen "gönderi teslim edilemedi — müşteri kaynaklı" iptalidir (`03 §1.4.1` h; `02 §7.2.9`)"*; Kaynak hücresine `02 §7.2.9`. Yeni karar yok. |
| P1 · BULGU-1 | ✅ KABUL | **Matris satırının özeti kararın tersini söylüyordu.** §1.1.10'un `03 §7.1.37` satırı devrin işini *"yeniden gönderilebilen e-postaların kapsamı — firma bildirimleri dahil"* diye yazıyordu; ifade Aşama 2'nin devir dizinindeki işin adından (`PHASE2_CONFLICT_SCAN.md` §6) gelir ve orada "firma bildirimleri de dahil mi" sorusunu anlatır. Karar ise tersidir: yalnız B-1…B-16 yeniden gönderilir (K-759), firma bildirimi satıra işaret düşürmez ve yeniden gönderilmez (K-760; `03 §7.1.37`, §8.9.2; bu doküman 2.5.4). Çelişki gerçek (ölçünün (4). sınıfı). Modelin "hatalı işlev üretebilir" etkisi abartılı — 2.5.4, 9.9.33 ve §1.1.10'un `03 §8.9.2` satırı doğru —, düzeltme önerinin kendisidir. | §1.1.10: *"Yeniden gönderilebilen e-postaların kapsamı — firma bildirimlerinin dahil olup olmadığı (karar satırı yoktu; UI9-01'de karara bağlandı — K-759, K-760: yalnız müşteri bildirimleri, B-1…B-16; firma bildirimi yeniden gönderilmez)"*. Yeni karar yok. |
| P4 · BULGU-1 | ✅ KABUL | **Panelin iki satırı limit aşılmışken engellenen işlemi açık gösteriyordu.** 7.3.3: limit aşılınca o işlem geçici olarak engellenir. 6.3.1.2 "e-posta ya da şifre hatalı" ile "L-1 aşıldı"yı tek satırda birleştirip "Yeniden denemek"i, 6.3.1.4 "gönderildi" ile "L-2 aşıldı"yı birleştirip "Bağlantıyı istemek"i açık sayıyordu. Müşteri tarafının karşılık satırları ayrımı yapar (6.2.22.3, 6.2.23.3 — "Engel kalkınca"), panelinkiler yapmıyordu: iki yerin çelişmesi (ölçünün (4). sınıfı). Modelin güvenlik etkisi — limitin aşılabileceği — abartılı: 9.1.8 ve 9.1.9 limit mesajını ve engeli yazar. Önerinin "engel kalkınca" yolu uygulandı; §6'nın satır sayısını değiştirmemek için satırlar bölünmedi. | 6.3.1.2: *"Yeniden denemek — L-1 aşılmışsa engel kalkınca · …"* · 6.3.1.4: *"Bağlantıyı istemek — L-2 aşılmışsa engel kalkınca"*. Yeni karar yok. |

**Dağılım:** 2 KABUL · 2 KISMİ · 0 RET (parça 2 ve 3 TEMİZ). **%100 kabul değil:** iki KISMİ'de modelin etki iddiası ("teslim edilmiş mi kapatılmış mı", "kalan alacak talep edilemez") somut yerlerle reddedildi ve önerinin yalnız en küçük yolu uygulandı; iki KABUL'de de etki iddiası abartılı bulundu, düzeltme önerinin kendisidir. Dört düzeltme de bir hücreyi ya da cümleyi aynı dokümandaki ya da `02`/`03`'teki kurala hizalar; yeni yetenek, yeni alan, yeni satır ya da yeni kural eklemez. Bulgular bilinçli konvansiyonlara (1–15; K-726, K-762) dokunmuyor; 1. turun dört bulgusundan ve audit'in uygulanmayan sekiz bulgusundan hiçbiri geri dönmedi.

## 4. Ek bulgular

- **Aynı kalıbın taraması (P4 · BULGU-1):** §6'da limit aşılmış hâl taşıyan on satır betikle listelendi (`L-[0-9]+ aşıl`): müşteri tarafının yedisi (6.2.10.4, 6.2.13.9, 6.2.15.3, 6.2.20.4, 6.2.22.3, 6.2.23.3, 6.2.24.4) ve panelin 6.3.25.2'si engellenen işlemi açık göstermiyor; aykırı olan yalnız 6.3.1.2 ve 6.3.1.4'tü.
- **Aynı kalıbın taraması (tek parça · BULGU-1):** "kapanışı tamamlıyorsa" dokümanda başka yerde geçmiyor; §6'da S11'i anan satır yok. Aynı kısa ifade `03 §8.2.7`'nin geçiş hücresinde de var (*"S7 kapanışı tamamlıyorsa → Teslim edildi (S11)"*), ama `03` ayrımı aynı tablonun 8.3.2.5 satırında açıkça yazar; yeni kural doğmadığı için `03`'e dönülmedi (K-652 tetiklenmez).
- **Aynı kalıbın taraması (P1 · BULGU-1):** "firma bildirimleri" geçen öteki yerler — başlık notunun Aşama 2 dizini satırı, §1.1.9'un `03 §10.1.2.2` ve §1.1.10'un `03 §8.9.2` satırları, 2.5.4, 2.5.5, 3.4.3, 6.3.3.5, 6.3.16.5, 9.3.3, 9.3.12 — kararla uyumlu. Aynı ifade Aşama 2'nin devir dizininde (`PHASE2_CONFLICT_SCAN.md` §6) işin adı olarak durur; Aşama 2 kaydı salt okunur, dokunulmadı.
- **Aynı kalıbın taraması (tek parça · BULGU-2):** "kalıcı son hâl" yalnız 6.2.16.10'da; panelin karşılığı 6.3.9.10 üç birleşimi tek satırda, işlemlerin koşullarıyla verir.
- **Mekanik tarama:** düzeltmeler gövdeye yeni K numarası eklemedi (K-759 ve K-760 §1.1.10'un komşu satırlarında zaten anılıyor; §1 bölüm sonu Kaynak satırı taşımaz). Sürüm başlığı, başlık notunun yeni sürüm notu ve dosya sonu dipnotu v0.15'i gösteriyor; karar kaydında çift numara yok.
- **Ciddi bir sorun görülmedi.** Bu turda ikinci modelin listesinin dışında ölçünün dört sınıfından birine giren bir sorun saptanmadı.
- **Önceki turlardan açık kalanlar:** yok — 1. turun dört bulgusu işlendi ve geri dönmedi. Audit'in etki yansıtmaya bıraktığı iz (Ürün Gereksinimleri'nin `§3.12.9` ile §6.2.9 arasındaki "Bu ürünü daha önce aldınız" metin farkı) TEMİZ turunun Faz 5'inde kapanır; bu tur TEMİZ olmadığı için taşınır.

## 5. Kullanıcı onay checklist'i

K-723 kararıyla (Aşama 3 boyunca ⚠ öneriyle kayıt, CI yeşilse `docs:` PR merge yetkisi ve yönetici + alt ajan düzeni) alt ajan değerlendirdi ve uyguladı (`04` v0.15). Bu turda karar satırı açılmadı; ⚠ karar yok. Ürün kuralına dokunan değişiklik yok; Ürün Gereksinimleri'ne, Kullanıcı Akışları'na ve MVP Kapsamı'na dönen değişiklik yok, `04 §1.3`'ün geri besleme tablosuna satır girmedi.

- [x] Tek parça · BULGU-1 (kısmi — 9.9.18 S11'in koşulunu yazar; S11 → S9 önerisi reddedildi)
- [x] Tek parça · BULGU-2 (kısmi — 6.2.16.9 ve 6.2.16.10 iki hâli koşuluyla ayırır; hak kaybı iddiası reddedildi)
- [x] P1 · BULGU-1 (kabul — §1.1.10'un `03 §7.1.37` satırı K-759 ve K-760'a hizalandı)
- [x] P4 · BULGU-1 (kabul — 6.3.1.2 ve 6.3.1.4 limitte engeli yazar)
- [ ] Etki yansıtma (Faz 5) — döngü TEMİZ döndükten sonra, son turun raporunda

**Hukuki kontrol** (2026-10-04): bulgularda yasal dayanaklı iddia yok. Model üç web araması yaptı (ETBİS kayıt yükümlülüğü; e-Arşiv faturada nihai tüketicinin kimlik numarası) ve bunlardan bulgu yazmadı; ETBİS'in kuralı Ürün Gereksinimleri'ndedir (`02 §3.1.4` — ETBİS alanı ve doğrulama bandı) ve `04` ona işaret eder. Düzeltmeler yeni hukuki içerik eklemiyor.

**Yakınsama ölçüsü:** bulgu sayısı 4 → 4 (1. turda tek parça 0 · parçalar 1 · 2 · 1 · 0; bu turda tek parça 2 · parçalar 1 · 0 · 0 · 1). Doküman 845 849 bayttan 847 241 bayta (+1,4 KB) büyüdü; artışın yarıdan fazlası başlıktaki sürüm notudur. Tablolara yeni satır girmedi (§6 282 satırda kalır). Kabul edilen bulgular kenar duruma inmedi — dördü de iki yerin metin farkıdır ve hiçbiri dokümana yeni yüzey eklemedi; skill'in yakınsama ölçütü ("dar kenar durumlara inen ve dokümanı büyüten bulgular") karşılanmıyor.

## 6. Sonuç ve 3. tur

**Sonuç: TEMİZ DEĞİL** (ölçünün dört sınıfında) — tek parça çağrı iki, kontrol koşumu iki bulgu; dördü de uygulandı. Faz 5 (etki yansıtma) bu turda koşmaz; TEMİZ turunda koşar.

**3. tur** 1. turun raporundaki istemle (`04_CROSS_REVIEW.md` §1'in alıntı bloğu, değiştirilmeden) ve iki koşumla yürür (K-848): (1) tek parça — istem, `=== DOKÜMAN ===`, güncel `04`, `=== ŞABLON ===`, `95a168e` şablonu; (2) dört parça — aynı istem, parça notu (`04_CROSS_REVIEW.md` §1), başlık notu ve şablonla; parça sınırları bölüm başlıklarıdır (`## 1.` · `## 2.`–`## 4.` · `## 5.` · `## 6.`–`## 9.`). Önceki turların raporları, karar kaydı ve audit raporları verilmez (K-431). Tur, beş çağrının beşi de `SONUÇ: TEMİZ` dönerse TEMİZ'dir.

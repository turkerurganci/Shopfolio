# Cross-Review — 03 User Flows (Tur 4 — ölçüsüz yoklama)

**Tarih:** 2026-10-04 · **Hedef:** `Docs/03_USER_FLOWS.md` v0.11 · **İkinci model:** Codex CLI — ChatGPT hesabı, `gpt-5.6-terra`, düşünme düzeyi yüksek (salt okuma) · **Ciddiyet ölçüsü:** **bu turda uygulanmadı** — ölçüsüz tek bir yoklama turu (K-720; 1.–3. turlar K-718'in ölçüsüyle koştu ve 3. tur TEMİZ döndü) · **Girdi:** yalnız doküman ve boş şablonu (`094fd92` — Aşama 2 başlamadan önceki v0.1), stdin'den tek metin, yeni, boş ve izole bir çalışma klasöründe; karar kaydı, önceki turların raporları ve audit/deep review raporları verilmedi (K-431)

> **Bu tur neden farklı:** Öğrenim terfisinden sonra proje sahibi, ölçünün ikinci modelin bulabileceği küçük ama gerçek hataları dışarıda bırakmış olabileceğini sordu ve ölçüsüz tek bir yoklama turunu onayladı (2026-10-04; K-720). **Amaç TEMİZ'e yakınsamak değil, görmektir:** tur tektir, bulgulu dönse de 5. tur koşulmaz; cross-review'ın çıkış koşulu K-718'in 3. turdaki TEMİZ'i olarak kalır. **Uygulama süzgeci yönetici tarafındadır:** yalnız gerçek hatalar uygulanır — yasaya aykırılık, para ya da hak kaybı, çıkışı olmayan akış, dokümanın iki yerinin çelişmesi, olgusal ya da atıf hatası, Ürün Gereksinimleri'nde (`02`) yazılı bir kuralın akışta eksik ya da yanlış çevrilmesi. Nadir kenar durum, iyileştirme önerisi, savunmada derinlik ve üslup bulguları RET değil **"ALINMADI (yoklama süzgeci)"** sınıfıyla raporlanır ve dokümana eklenmez.

## 1. Ham bulgular

İkinci modelin çıktısı, düzenlenmeden (alıntı bloğu içinde). **İstem 1.–3. turlarınkinden yalnız iki yerde ayrılır:** ciddiyet ölçüsünün paragrafı (K-615'in metni, *"Yalnız CİDDİ sorunları yaz: …"*) çıkarıldı ve bulgu biçiminin seviye alanına "Düşük" eklendi. Yedi kriter, bulgu biçimi (`BULGU-N` — Kriter · Seviye · Yer · Alıntı · Sorun · Öneri — ya da `SONUÇ: TEMİZ`), `cross-review` skill'inin Faz 1 madde 4'teki bilinçli karar cümlesi (değiştirilmeden) ve belgenin bir akış dokümanı olduğu, kural ayrıntısının verilmeyen Ürün Gereksinimleri'nde yaşadığı notu aynen kaldı. Bulgu biçiminin "Sorun" alanındaki *"ne yanlış, neden ciddi"* kalıbı önceki turlardan aynen taşındı; ölçü değildir, ama ölçüsüz istemde de modeli ağır bulgulara yöneltir — §6 bunu sonucun sınırı olarak yazar. Koşum öncesinde çağrı kısa bir deneme metniyle doğrulandı.

> ```text
> BULGU-1  
> Kriter: Güvenlik  
> Seviye: Yüksek  
> Yer: §2.4.1, §2.4.9, §2.10.5  
> Alıntı: "e-postası doğrulanmış bir müşteri hesabınınsa o hesaba anında düşer"  
> Sorun: Misafir ödeme akışında e-posta doğrulanmıyor; buna rağmen sipariş, yalnız e-posta eşleşmesiyle mevcut doğrulanmış hesaba bağlanıyor. Bir hata veya kötü niyetli giriş, siparişin adres, kalem ve işlem yetkilerinin ilgisiz hesap sahibine açılmasına; o kişinin iptal, cayma veya IBAN işlemlerine erişebilmesine yol açar. §2.10.5 bu hatanın gerçekleşebileceğini ayrıca kabul ediyor.  
> Öneri: Misafir siparişini yalnız oturum açılmış hesaba bağlayın veya e-posta sahipliğini tek kullanımlık bağlantıyla doğrulayın. Doğrulanana kadar siparişi misafir siparişi olarak tutun; otomatik hesap bağını kaldırın.
> 
> BULGU-2  
> Kriter: Teknik doğruluk  
> Seviye: Yüksek  
> Yer: §2.4.6, §2.8.1.4  
> Alıntı: "Taşıyıcıyı müşteri seçer"  
> Sorun: Doküman, cayma iadesinde satıcının öngördüğü taşıyıcıyı hiç tanımlamıyor; ön bilgilendirme ve cayma ekranında da buna yer vermiyor. Oysa 1 Ocak 2026’dan beri yürürlükte olan Mesafeli Sözleşmeler Yönetmeliği m.5/1-g, cayma hakkı varsa satıcının iade için öngördüğü taşıyıcı bilgisinin ön bilgilendirmede verilmesini gerektirir; m.12/5 ayrıca bu taşıyıcının tüketicinin bulunduğu yerde şubesi yoksa satıcının ek masrafsız teslim alma sağlamasını düzenler. [Resmî Gazete, 24 Mayıs 2025 değişikliği](https://resmigazete.gov.tr/eskiler/2025/05/20250524-2.htm)  
> Öneri: Firma ayarlarına iade taşıyıcısını ekleyin; Ön Bilgilendirme Formu ve cayma ekranında gösterin. Şube yokluğu için ücretsiz teslim alma akışını ve taşıyıcı dışı gönderilerdeki masraf/referans kurallarını açıkça tanımlayın.
> 
> BULGU-3  
> Kriter: Edge case  
> Seviye: Orta  
> Yer: §2.8.1.4, §8.3.2.1  
> Alıntı: "takip numarası istemez ve gönderimi denetlemez"  
> Sorun: İade paketinin hangi sipariş ve kaleme ait olduğuna ilişkin hiçbir eşleştirme mekanizması tanımlanmıyor. Buna karşılık §8.3.2.1, yöneticinin gelen malı doğru kalemle ilişkilendirip teslim alma işlemi yapabildiğini varsayıyor. Aynı ürünün birden fazla iadesi, farklı gönderici adı veya kimliği belirsiz paketlerde yanlış geri ödeme ya da hiç işlenmeyen iade riski doğar.  
> Öneri: Takip numarası toplamak zorunda olmadan, cayma onayında tekil iade referansı üretin; pakete eklenmesini isteyin. Referansı olmayan paketler için eşleştirme, bekletme ve kanıt kaydı akışını tanımlayın.
> ```

Model bu turda web araması yaptı; koşum kaydında üç sorgu görünür (öteki arama satırları boştur): Mesafeli Sözleşmeler Yönetmeliği m.12 — cayma hâlinde iade masrafları (iki sorgu, mevzuat.gov.tr ve ticaret.gov.tr) · Yönetmelik m.15 — koruyucu ambalaj istisnası, 2026 metni. Çıktıya yalnız BULGU-2'nin dayanağı girdi; 178 028 token kullandı.

## 2. Bağımsız değerlendirme

Her bulgu `03`'ün işaret edilen satırlarına ve Ürün Gereksinimleri'ne (`02` v0.55) karşı okundu — ikinci model `02`'yi görmedi.

| # | Karar | Gerekçe | Uygulanan düzeltme |
|---|---|---|---|
| BULGU-1 | ❌ RET | **Bilinçli karar ve kabul edilmiş risk.** Hesabı olan kişinin giriş yapmadan verdiği misafir siparişinin doğrulanmış hesaba e-postayla anında düşmesi `02 §3.13.3`'ün kuralıdır (K-99) ve aynı yerde *"Kalan risk bilinçlidir: e-postasını yanlış yazan alıcının siparişi o adresin hesabına düşer ve hesap sahibi o siparişi iptal edebilir ya da iade talebi açabilir. Riski daraltan bir çit konmaz"* diye yazılıdır; `02 §8.1.2` güvenlik ilkesi olarak tekrarlar, `02 §6.2.14` hata senaryosu olarak sayar. Çıkış yolu da kurallıdır: firma siparişin e-postasını düzeltince bağ kesilir (`02 §10.4.11`; K-585) — `03` bunu 2.6.7, 3.2.1.13, 3.2.2.5 ve §2.10.5'te adım adım yazar. Modelin önerisi (bağlamayı kaldırmak ya da e-posta sahipliğini bağlantıyla doğrulamak) K-99'un elediği "giriş duvarı / çit" seçeneğidir; misafir e-postasının doğrulanmaması da `02 §3.13.3`'ün ve 2.4.1'in bilinçli tercihidir. `03` kuralı ve sonucunu doğru çeviriyor; çelişki, atıf hatası ya da eksik çeviri yok. **Gözlem (uygulanmadı):** `03` kuralın sonucunu yazar ama "kalan risk bilinçlidir" etiketini taşımaz; model bu yüzden kararı göremedi. Etiketi eklemek kuralın gerekçesini akış dokümanına kopyalamak olur (checklist §4, "Kural ayrıntısı evinde kalır") ve süzgecin gerçek hata sınıflarından birine girmez. | Yok. |
| BULGU-2 | ❌ RET | **Hukuki iddia güncel metne karşı yanlış.** Yönetmelik m.5/1-g ve m.12/5'in güncel metni (RG 24/5/2025-32909 ile değişik, yürürlük 1/1/2026 — resmigazete.gov.tr'den 2026-10-04'te okundu) satıcının iade için taşıyıcı **belirtmemesini** açıkça öngörür: *"Satıcının ön bilgilendirmede iade için herhangi bir taşıyıcıyı belirtmediği durumda ise tüketiciden iade masrafına ilişkin herhangi bir bedel talep edilemez."* Şube yokluğunda teslim alma yükümlülüğü yalnız **ön bilgilendirmede belirtilen** taşıyıcı için doğar; taşıyıcı belirtilmeyince konusuz kalır. m.5/1-g'nin bilgilendirme yükümlülüğünü `02` karşılar: Ön Bilgilendirme Formu iade için taşıyıcı belirlenmediğini, müşterinin malı istediği taşıyıcıyla iade adresine karşı ödemeli gönderdiğini ve kargo bedelinin firmada olduğunu sabit metin olarak yazar (`02 §3.24.3`, §7.4.2, §7.4.4; K-492, K-493 — 2026-10-03'te aynı resmî metne karşı kuruldu). `03`'ün 2.4.6'sı formun içeriğini `02 §3.24.3`'e bırakır, 2.8.1.4 karşı ödemeli gönderimi ve masrafın firmada olduğunu yazar; ikisi de kurala uyar. Modelin önerisi (firma ayarına iade taşıyıcısı) K-493'ün elediği seçenektir ve yeni bir ayarla şube yokluğu akışı açar. | Yok. |
| BULGU-3 | ⏸ ALINMADI (yoklama süzgeci) | **İyileştirme önerisi ve kenar durum; gerçek hata değil.** Bulgu yeni bir mekanizma önerir (cayma onayında tekil iade referansı, referanssız paket için eşleştirme ve bekletme akışı). `02` iade etiketi, taşıyıcı ve takip numarası istememeyi bilinçli seçer (`02 §7.4.4`; K-293, K-132, K-493) ve teslim almayı yöneticinin elle kaydı yapar; yanlış kalemde yapılmış teslim almanın geri alınması yoktur ve sonucu firmadadır (`03` 8.3.2.1; K-712, `02 §6.4.17`'nin kalıbı). Müşteri hakkı korunur: cayma beyanı kalem düzeyinde ve tarih damgasıyla kayıtlıdır, teslim alma beyanlı kaleme yapılır; mal dönmemiş görünürse "mal dönmedi" kapanışı geri ödeme borcuna karar vermez ve mal sonradan eşleşince kalem yeniden açılır (`02 §10.4.9`; `03` 2.8.1.8, §2.10.4). `03`'te iki yer çelişmiyor ve `02`'nin bir kuralı eksik çevrilmemiş; "aynı ürünün birden fazla iadesi" nadir bir kenar durumdur. | Yok — dokümana eklenmez. |

**Dağılım:** 0 KABUL · 0 KISMİ · 2 RET · 1 ALINMADI. **Sıfır KABUL şüpheyle sınandı:** her RET'in gerekçesi `02`'nin yazılı bir satırına (§3.13.3, §8.1.2; §3.24.3, §7.4.2, §7.4.4) ve BULGU-2'de resmî metnin bugünkü hâline dayanır; üç bulgunun alıntısı dokümanda birebir var, yani model metni doğru okudu — eksik olan `02`'nin bağlamıdır (K-431'in girdi kuralının bilinen bedeli). Bulgular bilinçli konvansiyonlara (K-669, K-670, K-682, K-689, K-701, K-702) ya da tartışmalı bırakılan YASAL-7 ve DR-8'e dokunmuyor. Ek tarama (§3) da süzgecin sınıflarından birine giren bir hata bulmadı.

## 3. Ek bulgular

Ölçüsüz turun sorusu "ölçü gerçek bir hatayı bastırdı mı" olduğu için ikinci modelin listesine ek olarak süzgecin mekanik olarak sınanabilen sınıfları tarandı:

- **Atıf hatası — mekanik:** `03`'ün bütün atıfları betikle çıkarıldı (K-701'in devralma kuralıyla; CP02'nin betiği): 3 234 atıf — `02`'ye 1 842, `03`'ün kendine 1 385, `10`'a 7 — hedef dokümanın bölüm ya da satır kimliğine çözülüyor; çözülmeyen yok. Kimlik aileleri `02`'deki tanımlarla tam: Z-1…Z-47, L-1…L-9, P-1…P-49, B-1…B-16, F-1…F-6, H-1…H-4. `03`'te anılan doksan beş karar numarasının hepsi karar kaydında satır olarak var.
- **Modelin dokunduğu bölgenin `02`'ye karşı okunması:** ödeme adımı (2.4.1–2.4.9) ve cayma ve iade (2.8.1.1–2.8.1.8) satırları `02 §3.13`, §3.14, §3.17, §3.21, §3.24, §7.3 ve §7.4'e karşı okundu — süreler (Z-13, Z-16, Z-42), iade masrafı, iade adresinin beyana donması (K-665), koşullu istisna reddi (K-656, K-657) ve değer kaybı (K-668) kurala uyuyor.
- **Ciddi bir sorun görülmedi.** Süzgecin sınıflarından birine giren bir sorun saptanmadı.
- **Önceki turlardan açık kalanlar:** yok. 1. ve 2. turun dört bulgusu (2.5.1.2; 4.2.11, 10.3.3; 8.3.1.2; 2.7.8, 8.4.8, 4.1.11) bu turda geri dönmedi.

## 4. Kullanıcı onay checklist'i

K-648 kararıyla (Aşama 2 boyunca ⚠ öneriyle kayıt ve CI yeşilse `docs:` PR merge yetkisi) yönetici değerlendirdi. Turdan önce süreç kararı **K-720** kaydedildi: proje sahibinin sorusu ve onayıyla ölçüsüz tek yoklama turu, uygulama süzgeci yönetici tarafında (elenen seçenekler: ölçüyle bırakmak — sorunun cevabı ölçülü turdan çıkmaz; ölçüsüz yakınsayana kadar koşmak — K-718'in gerekçesi, K-615). K-718'in etki sütununa geri işaret bırakıldı. K-720 ⚠ değildir.

- [x] BULGU-1 (ret — bilinçli karar ve kalan risk, `02 §3.13.3`; K-99, K-585)
- [x] BULGU-2 (ret — m.12/5'in güncel metni taşıyıcı belirtilmemesini öngörür; `02 §7.4.2`; K-492, K-493)
- [x] BULGU-3 (alınmadı — yoklama süzgeci; iyileştirme önerisi, sonucu bilinçli olarak firmada, K-712)
- [x] Etki yansıtma (Faz 5) — uygulanan düzeltme yok, §5

`03` v0.12: yalnız başlıktaki sürüm notu ve dosya sonu dipnotu bu turu yazar; akış içeriği değişmedi. `02`'ye ve `10`'a dönen değişiklik yok, §11'e satır girmedi; ⚠ karar yok.

**Hukuki kontrol** (2026-10-04): BULGU-2'nin dayandığı hükümler — Mesafeli Sözleşmeler Yönetmeliği m.5/1-g ve m.12/5 — RG 24/5/2025-32909'un metnine karşı okundu (resmigazete.gov.tr, `eskiler/2025/05/20250524-2.htm`; değişiklik 1/1/2026'da yürürlüğe girdi). Bu tarihten sonra m.5'i ya da m.12'yi değiştiren bir yönetmelik aramada çıkmadı. Metin K-492 ve K-493'ün okumasını doğruluyor; modelin iddiası taşıyıcının belirtildiği hâlin kurallarını belirtilmediği hâle taşıyor.

**Yakınsama ölçüsü:** bu tur yakınsama turu değildir (K-720). Doküman 370 179 bayttan 371 036 bayta (+0,9 KB) büyüdü — yalnız sürüm notu ve dipnot.

## 5. Etki yansıtma sonucu (Faz 5)

Faz 5 yalnız uygulanan düzeltmeler için yapılır (K-720); bu turda uygulanan düzeltme yok. `03`'ün akış satırları değişmedi; yeni alan, enum değeri, parametre, süre kimliği ya da bildirim eklenmedi; Kaynak sütunu değişmedi. Ürün Gereksinimleri (v0.55), MVP Kapsamı (v0.37), Proje Vizyonu (v0.32) ve sonraki dokümanların park blokları etkilenmez. Cross-review'ın etki yansıtması 3. turun raporunda (§5) kapandı ve geçerlidir. Karar kaydında `03`'ün cross-review sütunu "✓ (3 tur, TEMİZ; 4. tur ölçüsüz yoklama — K-720)"; `03` ⏳ kalır — ✓ aşama kapanışının arşiv adımında konur (K-437'nin 6. adımı; gerekçe 3. turun raporunun §5'inde).

## 6. Yorum — ölçü bir şey kaçırmış mı

**Ölçü 1.–3. turlarda gerçek bir hata dışarıda bırakmadı.** Ölçüsüz turun üç bulgusundan hiçbiri süzgeçten geçmedi: ikisi `02`'de yazılı bilinçli kararlara (K-99; K-492, K-493) itirazdı — biri güncel mevzuatın yanlış okunmasına dayanıyordu —, üçüncüsü yeni bir mekanizma öneren bir iyileştirmeydi. Gerçek hata sayısı **sıfır**: ne olgusal ya da atıf hatası, ne iki yerin çelişmesi, ne de `02`'nin bir kuralının eksik çevrilmesi çıktı. Ölçünün kaldırılmasıyla gelen, ölçünün kuruluş gerekçesindeki türdür: `02`'yi görmeyen modelin kararı yeniden açması ve kenar durum. Bulguların kaynağı ölçü değil girdi kuralıdır (K-431): model `03`'ü doğru okudu, `02`'nin gerekçesini göremedi.

**Sınırı:** tek tur ve tek modeldir; istemin "neden ciddi" kalıbı ölçüsüz turda da modeli ağır bulgulara yöneltir. Sonuç küçük hataların yokluğunu kanıtlamaz, bu koşumda görünmediğini gösterir. Mekanik atıf taraması (§3) aynı soruyu modelden bağımsız olarak sınadı ve temiz döndü.

**Öğrenim terfisine etkisi:** sonuç, öğrenim terfisinin `00`'a önerdiği Aşama 2 desenlerinin 3.'sünü (*"Ölçü baştan verilince döngü kısa kalır"* — `Docs/CHECKPOINT_REPORTS/PHASE2_LEARNING_PROMOTION.md` §4, Öneri 1) destekler: kısa döngü gerçek bir hatayı bastırma bedeliyle gelmedi. Desenin metni bunu söylemiyor; öneri uygulanacaksa desene *"ölçüsüz bir yoklama turu gerçek hata bulmadı (K-720)"* kanıtı eklenmesi önerilir. Metodolojiye bu adımda dokunulmadı.

# Cross-Review — 03 User Flows (Tur 1)

**Tarih:** 2026-10-04 · **Hedef:** `Docs/03_USER_FLOWS.md` v0.7 · **İkinci model:** Codex CLI — ChatGPT hesabı, `gpt-5.6-terra`, düşünme düzeyi yüksek (salt okuma) · **Ciddiyet ölçüsü:** 1. turdan itibaren uygulanır (K-718) · **Girdi:** yalnız doküman ve boş şablonu (`094fd92` — Aşama 2 başlamadan önceki v0.1), stdin'den tek metin, boş ve izole bir çalışma klasöründe; karar kaydı, önceki raporlar ve audit/deep review raporları verilmedi (K-431)

## 1. Ham bulgular

İkinci modelin çıktısı, düzenlenmeden (alıntı bloğu içinde). **İstem:** yedi kriter, bulgu biçimi (`BULGU-N` — Kriter · Seviye · Yer · Alıntı · Sorun · Öneri — ya da `SONUÇ: TEMİZ`), `cross-review` skill'inin Faz 1 madde 4'teki bilinçli karar cümlesi değiştirilmeden ve ciddiyet ölçüsünün paragrafı (K-615'in metni): *"Yalnız CİDDİ sorunları yaz: (1) yürürlükteki mevzuata aykırılık, (2) müşteriye ya da firmaya para kaybı veya hak kaybı doğuran kural boşluğu, (3) kullanıcının takılıp kaldığı, çıkışı olmayan bir akış, (4) dokümanın iki yerinin birbiriyle çelişmesi. Nadir kenar durumları, iyileştirme önerilerini, savunmada derinlik önerilerini, üslup ve ifade tercihlerini bulgu yapma. Böyle bir sorun yoksa SONUÇ: TEMİZ yaz."* İstem ayrıca belgenin bir akış dokümanı olduğunu söyledi: kural ayrıntısı ayrı bir Ürün Gereksinimleri dokümanındadır, verilmedi; kural ayrıntısının burada yeniden yazılmamış olması tek başına bulgu değildir; yazım konvansiyonları §0'dadır. Koşum öncesinde çağrı kısa bir deneme metniyle doğrulandı.

> ```text
> BULGU-1  
> Kriter: tutarlılık  
> Seviye: Yüksek  
> Yer: §2.5.1.2  
> Alıntı: "Bildirim henüz gelmemişse sayfa ödemenin beklendiğini söyler" / "→ Ödendi (Ö1)"  
> Sorun: Sağlayıcı bildirimi henüz gelmemişken siparişin Bekliyor’da kalacağı söylenirken aynı satır ödeme durumunu Ödendi ve bildirimi B-4 olarak gösteriyor. Bu, ödemesi teyitsiz siparişin ödenmiş sayılıp yürütülmesine yol açabilir.  
> Öneri: Durum ve bildirim hücresini koşullu yazın: sağlayıcının doğrulanmış başarı bildirimi gelirse Ö1 ve B-4; gelmemişse Alındı + Bekliyor ve bildirim yok.
> 
> BULGU-2  
> Kriter: tutarlılık  
> Seviye: Yüksek  
> Yer: §10.3.3, §10.3.4, §4.2.12  
> Alıntı: "kendiliğinden iptal ... hemen çalışır" / "İptal sitenin dönüşünden sonraki ilk iş gününün sonuna ertelenir"  
> Sorun: Kesintide dolan havale ödeme süresinde iptalın dönüşte hemen mi yoksa ilk iş gününün sonuna ertelenerek mi çalışacağı çelişkili. Bu, ödeme yapmış müşterinin siparişinin erken iptali ve para/stock kaybı riski doğurur.  
> Öneri: §10.3.3’te havale Z-8 iptalını açıkça istisna tutun ve yalnız §10.3.4 ile §4.2.12’deki ertelenmiş akışın uygulanacağını yazın.
> 
> BULGU-3  
> Kriter: tutarlılık  
> Seviye: Yüksek  
> Yer: §8.3.1.2, §1.4.3, §1.11.21  
> Alıntı: "→ İptal edildi (S9)" / "En az bir kalem teslim edilmişse → Teslim edildi"  
> Sorun: Müşteri kaynaklı teslim edilememe nedeniyle geri dönen gönderinin iptalinde §8.3.1.2 siparişi koşulsuz S9 ile İptal edildi yapıyor. Oysa aynı siparişte önceden teslim edilmiş dijital veya hizmet kalemi varsa §1.4.3 ve §1.11.21 S11 ile Teslim edildi sonucunu zorunlu kılıyor. Yanlış terminal durum, siparişin sonraki hak ve süreçlerini yanlış kapatabilir.  
> Öneri: §8.3.1.2’yi, hiç teslim edilmiş kalem yoksa S9; en az bir kalem teslim edilmişse geri dönen kalemin iptali sonrası S11 ile Teslim edildi olacak şekilde koşullandırın.> ```

Model bu turda web araması yapmadı (koşum kaydında araç çağrısı yok); 135 814 token kullandı.

## 2. Bağımsız değerlendirme

| # | Karar | Gerekçe | Uygulanan düzeltme |
|---|---|---|---|
| BULGU-1 | ⚠️ KISMİ | **Hücre gerçekten koşulsuzdu, ama bulgunun dayandığı risk yok.** 2.5.1.2'nin "Sistem" hücresi onayı açıkça sağlayıcının başarı bildirimine bağlıyor (*"Ödeme sağlayıcının başarı bildirimiyle onaylanır … Bildirim henüz gelmemişse sayfa ödemenin beklendiğini söyler"*); `02 §3.21.3` ve §5.5 de Ö1'i bildirime bağlar. Teyitsiz ödemenin "Ödendi" sayılıp yürütülmesi metnin hiçbir okumasında çıkmaz; seviye Yüksek değildir. Sorun, Durum ve Bildirim hücrelerinin "Sistem" hücresindeki koşulu taşımamasıdır — 2.5.1.4 aynı hâli koşullu yazıyor; bu, ölçünün (4). sınıfında dar bir iç tutarsızlıktır. **Önerinin "gelmemişse … bildirim yok" dalı ayrı yazılmadı:** bildirim hiç gelmezse 2.5.1.4 işler ve satır zaten ona işaret ediyor; ikinci bir dal açmak aynı hâli iki yerde yazardı. | 2.5.1.2 Durum: *"Başarı bildirimiyle → Ödendi (Ö1) · bildirim gelene kadar Alındı + Bekliyor"*; Bildirim: *"Ö1'de B-4 → müşteri"*. Yeni karar yok. |
| BULGU-2 | ✅ KABUL | **İki yer gerçekten çelişiyordu.** 10.3.3 ve 4.2.11, kesintide zamanı gelen kendiliğinden iptali (4.2.3 — Z-7 ve Z-8'in ikisini de kapsar) dönüşte "hemen" çalıştırıyordu; hemen altındaki 4.2.12 ve 10.3.4 ise havale ödeme süresinin kesintide dolmasında iptali dönüşten sonraki ilk iş gününün sonuna erteliyor (K-667, `02 §3.21.8`). Okuyan, havale siparişinin dönüşte hemen iptal edileceğini anlayabilirdi; bu, ödemesini yapmış müşterinin siparişini kaybetmesi demektir — (2). ve (4). sınıf. Karar K-667'dir ve doğrudur; iki satır istisnayı anmıyordu. `02` K-667'yi zaten doğru taşır. | 4.2.11: *"4.2.3 (havale ödeme süresi kesintide dolduysa iptal ertelenir — 4.2.12)"*; 10.3.3: *"kendiliğinden iptal (kart hattında iptal öncesi son sorguyla; havale ödeme süresi kesintide dolduysa iptal ertelenir — 10.3.4)"*. Yeni karar yok. |
| BULGU-3 | ✅ KABUL | **İki yer gerçekten çelişiyordu.** 8.3.1.2 müşteri kaynaklı dönen gönderinin iptalinde siparişi koşulsuz S9 ile İptal edildi'ye geçiriyordu. 8.3.1.1, 1.11.21 ve 3.3.9 aynı hâli koşullu yazıyor: hiçbir kalem teslim edilmemişse S9, teslim edilmiş dijital ya da hizmet kalemi varsa geri dönen kalemlerin kalem düzeyinde iptaliyle S11 → Teslim edildi (`02 §5.4` S11; audit'in 1.11.21 düzeltmesi). 8.3.1.2 audit'te hizalanmamış tek satırdı. Teslim edilmiş kalemi olan siparişi İptal edildi'ye geçirmek o kalemin cayma ve ayıp haklarının işlediği siparişi kapatır — (4). sınıf. | 8.3.1.2 Aktör: *"siparişi ya da geri dönen kalemleri iptal eder"*; Durum: *"→ İptal edildi (S9) — teslim edilmiş kalem varsa geri dönen kalemlerin iptaliyle → Teslim edildi (S11) (8.3.1.1)"* — 3.3.9'un ifadesi. Yeni karar yok. |

**Dağılım:** 2 KABUL · 1 KISMİ · 0 RET. Üç bulgunun üçü de ölçünün (4). sınıfında, iki yerin çelişmesi; hiçbiri yeni bir kural ya da kenar durum açmadı. **%100 kabul değil:** BULGU-1'in seviyesi ve risk iddiası reddedildi, önerisinin bir dalı uygulanmadı. RET olmamasının sebebi: üç bulgunun da alıntısı dokümanda birebir var ve karşı yerleri (2.5.1.4; 4.2.12, 10.3.4; 8.3.1.1, 1.11.21, 3.3.9) aynı dokümanda; hiçbiri bilinçli konvansiyona (K-669, K-670, K-682, K-689, K-701, K-702) ya da tartışmalı bırakılan YASAL-7 ve DR-8'e dokunmuyor.

## 3. Ek bulgular

- **Aynı kalıbın taraması (BULGU-3):** `03`'te S9'u S11 olmadan anan bütün satırlar tarandı (§1.2 S9, 1.4.3, 1.6.1.3, 1.11.8, 1.11.40, 7.1.14, 7.2.13, 7.3.9, 8.3.2.5). Hepsi ya kapanışı ya hiçbir kalemin teslim edilmediği hâli ya da genel geçiş kimliğini yazıyor; koşulsuz S9 yazan başka satır yok.
- **Aynı kalıbın taraması (BULGU-2):** kesintide kendiliğinden iptali anan satırlar 4.2.3, 4.2.11, 4.2.12, 2.5.2.5, 10.3.3, 10.3.4. 2.5.2.5 istisnayı zaten anıyordu; 4.2.11 de 10.3.3 ile birlikte düzeltildi.
- **Mekanik tarama:** `03`'te anılan bütün karar numaraları (en yükseği K-718, sürüm notunda) karar kaydında satır olarak var. Sürüm başlığı ve dosya sonu dipnotu aynı sürümü gösteriyor (v0.8).
- **Ciddi bir sorun görülmedi.** Bu turda ikinci modelin listesinin dışında ölçünün dört sınıfından birine giren bir sorun saptanmadı.
- **Önceki turlardan açık kalanlar:** yok (1. tur). Audit'in tartışmalı bıraktığı YASAL-7 ve DR-8 bu turun girdisi değildi ve model bunlara dokunmadı.

## 4. Kullanıcı onay checklist'i

K-648 kararıyla (Aşama 2 boyunca ⚠ öneriyle kayıt ve CI yeşilse `docs:` PR merge yetkisi) yönetici değerlendirdi ve uygulandı (`03` v0.8). Bu turda ürün kuralına dokunan yeni karar satırı açılmadı; `02`'ye ve `10`'a dönen değişiklik yok, §11'e satır girmedi. Turdan önce süreç kararı **K-718** öneriyle kaydedildi: ciddiyet ölçüsü Kullanıcı Akışları'nın cross-review'ına 1. turdan itibaren uygulanır (skill'in ikinci koşulu; elenen seçenek: ölçüsüz başlayıp yakınsama sorunu görülünce açmak — Ürün Gereksinimleri'nde 25 tur, 374 KB → 500 KB, K-615). K-718 ⚠ değildir.

- [x] BULGU-1 (kısmi — Durum ve Bildirim hücreleri koşullu yazıldı; risk iddiası ve ikinci dal uygulanmadı)
- [x] BULGU-2 (kabul — 4.2.11 ve 10.3.3 K-667'nin istisnasını anar)
- [x] BULGU-3 (kabul — 8.3.1.2, 8.3.1.1, 1.11.21 ve 3.3.9'a hizalandı)
- [ ] Etki yansıtma (Faz 5) — döngü TEMİZ döndükten sonra, son turun raporunda

**Hukuki kontrol** (2026-10-04): Bulgularda yasal dayanaklı iddia yok; model web araması yapmadı. Düzeltmeler yeni hukuki içerik eklemiyor — BULGU-2'nin dayandığı erteleme K-667'nin ve `02 §3.21.8`'in mevcut kuralıdır.

**Yakınsama ölçüsü:** doküman 365 194 bayttan 366 207 bayta (+1,0 KB) büyüdü; artışın çoğu başlıktaki sürüm notudur. Akış tablolarına yeni satır girmedi.

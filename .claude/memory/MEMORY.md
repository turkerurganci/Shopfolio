# Proje Hafızası

> **Bu dosya indeks + güncel durum snapshot'ıdır.** İçerik gömülmez — her hafıza ayrı bir dosyadır.
> Ne buraya girer / ne L1–L5'e terfi eder: [`README.md`](README.md).
> **Otoriter kaynak değildir** — task durumu için [`../../Docs/IMPLEMENTATION_STATUS.md`](../../Docs/IMPLEMENTATION_STATUS.md).

---

## Proje Özeti

- **Ad:** Shopfolio
- **Tanım:** Herhangi bir firmanın kendi ürünlerini sergileyip çevrimiçi satabileceği ve kurumsal tanıtımını yapabileceği; üyelik, Google/Facebook ile giriş, sipariş, ödeme ve sipariş takibi içeren web uygulaması.
- **Dönem:** Doküman üretimi
- **Doküman dili:** Türkçe · **Kod dili:** İngilizce

---

## Güncel Durum

> Kısa tut. Tarihsel detay `MEMORY_ARCHIVE.md`'ye taşınır (00 §G.2).

- **Son tamamlanan:** **Blok 3 ✓ · oturum 5 — blok bitti.** Üç konu, on beş karar (**K-81…K-95**) + oturumda bulunan bir boşluk (**K-96**): `B3-10` ürün türü ve mevzuat (katalog **üç tür** taşır — fiziksel, dijital, hizmet; tip **ürün düzleminde** kapalı bir alan, varyantlar devralır; stok tipe göre — fiziksel zorunlu sayısal, dijital sınırsız, hizmette **opsiyonel kontenjan**; sistem ne satıldığını **denetlemez**, `10 §3`'e kapsam satırı yazılır; ürün **kalıcı silinebilir**, adres 404 döner — K-56'nın devri kapandı). `B3-11` yayın kapısı ve ölçek (**dar set**: ad + ana kategori + tip — görsel ve açıklama **opsiyonel**; birleşim matrisi **eksik olabilir**, olmayan kombinasyon görünür ama seçilemez; varyant stok kodu **zorunlu ve tekil**; katalog ölçeğine **hiçbir sistem sınırı yok**). `B3-12` medya (görsel **üründe**, varyant ezebilir; **ayrı ana görsel işareti**, sıradan bağımsız; format + dosya boyutu + **adet tavanı** üçü de var; alternatif metin opsiyonel ama **boşsa ürün adı**; galeri sırasını firma belirler; **yalnız ada** karakter tavanı, açıklama serbest). **K-96** ürün açıklaması **sınırlı zengin metin** — 111 konunun hiçbirinde yoktu, `B3-12` kapanırken yapılan tarama buldu; yeni konu kimliği açılmadı. **Ajanın önerisi on altı sorunun beşinde reddedildi** (K-81 · K-86 · K-89 · K-91 · K-92). **K-92, K-89'dan bilinçli ayrışmadır** — K-89 katalog büyüklüğüne, K-92 tek sayfanın ağırlığına bakar; K-80'in K-76'dan ayrıldığı biçimin tekrarı. Blok 3 toplamı: on iki konu, beş oturum, **elli sekiz karar** (K-39…K-96). Öncesinde oturum 4 → K-77…K-80 (PR #14), oturum 3 → K-58…K-76 (PR #13), oturum 2 → K-42…K-57 (PR #12), oturum 1 → K-39…K-41 (PR #11), `01 §1–§5` taslağı (PR #10).
- **Son yazım oturumu:** **`02 §1` taslağı yazıldı — 2026-09-15**, K-28'in ikinci uygulaması, K-30 gereği ayrı oturum (girdi: karar kaydı §2/§4, `02` şablonu, 00 §C.6 ürün bağlamı katmanı, üst doküman `01`; workshop geçmişi kullanılmadı). Sözlükte **22 terim** (K-17'nin üç sütunu), üç aktör + üç "aktör olmayan"; §2–§13'ün her birine **adıyla yazım kapısı** notu düştü. `02` → v0.2. **Oturumun üç bulgusu:** (1) sözlükteki İngilizce karşılıkların **11'i kayıtta yok**, taslakta türetilip **†** ile işaretlendi → **A-05**; (2) §6.2'nin "Hedef doküman" sütunu `02 §1`'i yalnız Blok 1 ve Blok 3'e bağlıyor, oysa **K-06** (misafir alıcı → `B4-01`) ve **K-17** (sipariş durum adları → `B6-03`) §1'e sonraki bloklardan satır ekleyeceğini söylüyor — **proje sahibinin kararını bekliyor**, ajan sütunu değiştirmedi; (3) **K-88**'in etki sütunu `02 §1` taşımıyor ama karar metni sözlük satırı ekliyor. Üçü de tracker §6.1'de.
- **Sırada:** **Blok 4** — üyelik, kimlik doğrulama ve hesap yaşam döngüsü; on konu (`B4-01`…★`B4-10`), tahmin bir oturum. `B4-07` (yaş sınırı) **K-84'ü devralır**; `B4-01` (misafir alışverişi) **K-06'nın aktör envanterini** kapatır ve sonucu `02 §1`'e satır ekler. `01`'in §6–§8'i ve kalite döngüsü Blok 9'a kadar açılmaz (K-28).
- **Blok 3'ün sonraki bloklara devirleri (güncel):** `B4-07` (yaş sınırı — K-84'ün kapsam satırı buna dayanır) · `B5-02` (kupon sayacı **ve** hizmet kontenjanı, stok rezervasyonuyla aynı rejim) · `B5-03` (sepet canlıdır, kalem sipariş anında donar; ayrıca **silinen ürünün açık sepetteki kalemi**) · `B5-04`, `B5-06` (fiziksel-olmayan kalemde kargo hesabı ve dijital dosyanın teslimi) · `B6-03` (dijital ve hizmet siparişinin durum makinesi) · `B6-04` (kısmi iptal/iade **kalem** düzeyinde) · `B6-05` (yarıda kalan ödemede siparişin kaderi) · `B6-09` (**tipe göre cayma hakkı istisnası** — K-81'in en ağır devri) · `B6-10` · `B6-11` (fatura) · `B6-15` · `B7-03` (folio tarafı K-96'nın zengin metin setini devralır mı) · `B7-05` (404'ün adres/yönlendirme rejimindeki yeri) · `B8-02`/`B8-14` (silme işleminin işlem izi) · `B8-06` · `B8-10` · `B9-04` (erişilebilirlik tabanı — K-93'ün geri düşüşü buna dayanır; cihaz tabanı K-92) · `B9-08` (KDV oran listesi · K-40'ın boyut sınırı · görsel format listesi, dosya boyutu ve adet tavanı · ad karakter tavanı) · `B9-14` (toplu içe aktarma — anahtarı K-88'in **zorunlu SKU**'sudur) · Blok 8 yasal uyum turu. **`04`'e devirler:** görselsiz kart ve ürün sayfası (K-86) · "yok" ile "Tükendi" ayrımı (K-87) · SKU'nun önceden doldurulması (K-88) · ana görsel işaretsiz/silinmişse geri düşüş (K-91).
- **Açık kararlar:** **A-02, A-03, A-04** — `01` yazım oturumunun kayıtta bulduğu üç boşluk (matris derece değerleri · aktör tablosunun "neden geri döner" hücreleri · §1'in sayısal somutluğu); vadeleri `01 §6–§8` yazım oturumu. **A-05** — `02 §1.2` sözlüğündeki 11 İngilizce kod karşılığı; vadesi `02`'nin son yazım oturumu, **`03`/`04`'ten önce** (K-17'nin gerekçesi bunu zorunlu kılıyor). Dördü de **detay**, varlık kararı açık değil. Ayrıntı: tracker §4.
- **Gate durumu:** Implementation başlamadı, faz yok. Doküman: 01 ⏳ (v0.2, §1–§5 taslak) · **02 ⏳ (v0.2, §1 taslak)** · 10 ⏳ · 03–09, 11–12 ⬚. Blok planı: 0 ✓ · 1 ✓ · 2 ✓ · **3 ✓** · 4–9 ⬚.
- **Oturum kapanışı (K-38):** Doküman döneminde bu blok, `/handoff` beklenmeden **oturum sonu `docs:` PR'ının içinde** güncellenir. Bayat kalırsa bunu yakalayacak başka kapı yok. **İki kez kaçırıldı:** oturum 1 (altı gün) ve oturum 4 (sekiz gün); ikisini de INSTRUCTIONS §3.0 açılış kapısı yakaladı. **Karşı ilaç üç oturumdur işliyor:** kararlar **konu bittikçe** partiler hâlinde kayda yazılır, oturum sonuna biriktirilmez — oturum 3, 4 ve 5'te uygulandı, bu oturumda dört parti oldu.
- **Son güncelleme:** 2026-09-15

---

## Task Changelog (son N task)

| Task | Durum | Özet | Commit | PR |
|---|---|---|---|---|
| | | | | |

> Bu tablo şişmeye başladığında (yaklaşık 20 satırı geçince) eski satırlar `MEMORY_ARCHIVE.md`'ye taşınır.

---

## Proje

- `<project_*.md dosyaları buraya tek satır olarak listelenir>`

## Kullanıcı

- `<user_*.md>`

## Çalışma Tercihleri (feedback)

- `<feedback_*.md>`

## Referanslar

- `<reference_*.md>`

---

## Terfi Edenler

> Bir zamanlar hafızada yaşayıp artık L1–L5'te olan kurallar. Buraya yalnız işaretçi yazılır.

| Kural | Terfi ettiği yer | Tarih |
|---|---|---|
| Bir konunun alt parçaları da tek tek sorulur | `INSTRUCTIONS.md` §2 | 2026-08-18 |
| Seçenekler sade dille, somut sonuç üzerinden yazılır | `INSTRUCTIONS.md` §2 | 2026-08-18 |
| Sorular ve seçenekler numaralandırılır | `INSTRUCTIONS.md` §2 | 2026-08-30 |
| Konu kapanışında tamlık taraması | `INSTRUCTIONS.md` §2 | 2026-09-15 |

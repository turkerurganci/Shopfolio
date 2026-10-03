---
name: cross-review
description: "Bir dokümanı bağımsız ikinci bir AI'ya okutup bulguları yansıtır; audit ve deep review'dan sonra çalışır. Kullan: 'cross-review', 'ikinci görüş', /cross-review Docs/XX_....md."
user-invocable: true
---

# Cross Review — Bağımsız İkinci AI Review Döngüsü

> **Ne zaman:** Bir dokümanın audit'i ve deep review'ı tamamlandıktan sonra.
> **Tetikleme:** "cross-review", "ikinci görüş" veya `/cross-review Docs/XX_....md`.
>
> **Temel fark:** Audit ve deep review **aynı ajanın iç denetimi**dir. Cross review, dokümanı **farklı bir modele** okutarak birinci ajanın kaçırdığını yakalar. Farklı model hem avantajdır (taze göz) hem dezavantaj (proje bağlamını bilmez).

## Parametreler

| Parametre | Zorunlu | Açıklama |
|---|---|---|
| `hedef` | Evet | Review edilecek doküman yolu |
| `round` | Hayır | Kaçıncı tur (varsayılan 1) |

---

## Faz 1 — İkinci modele gönder

1. Dokümanı ikinci modele gönder (SETUP'ta tanımlanan yöntemle: script, web arayüzü veya API). Birincil yöntemin limiti dolarsa SETUP'ın yedek yöntemiyle sürdürülür; raporun başlığı turun hangi modelle koştuğunu yazar.
2. **Girdi yalnız doküman ve şablonudur.** Karar kaydı, sohbet geçmişi ve önceki turların değerlendirmesi verilmez — cross-review dokümanın **kendi başına** yeterli olup olmadığını sınar; kaydı gören denetçi dokümanda olmayanı kayıttan tamamlar (Aşama 1, K-431).
3. **Yedi kriter** istenir: tutarlılık · eksiklik · belirsizlik · teknik doğruluk · edge case · güvenlik · kullanıcı deneyimi.
4. **Talimat şu cümleyi taşır** (her turda, değiştirilmeden): *"Dokümanda açıkça bilinçli karar, elenen seçenek ya da kabul edilmiş risk olarak yazılmış bir seçimi, yalnız seçime katılmadığın için bulgu yapma. Böyle bir karar dokümanın kendi içinde çelişki, olgusal ya da hukuki hata doğuruyorsa yaz."* **Neden:** Proje Vizyonu'nun 8.–12. turlarında üç karar dört-beş kez geri döndü; cümle eklendikten sonra tekrarlar kesildi ve döngü altı turda TEMİZ'e ulaştı.
5. **Ciddiyet ölçüsü** (gerektiğinde — aşağıdaki "Ciddiyet ölçüsü" bölümü): talimat yalnız ciddi sorunları ister.
6. Çıktı yapılandırılmış olmalı: `BULGU-N: …` veya `SONUÇ: TEMİZ`.
7. Ham çıktı `Docs/CROSS_REVIEW_REPORTS/XX_CROSS_REVIEW[_RN].md` dosyasına yazılır.
8. **Çıktıyı tam oku** — her bulguyu not al.

### Ciddiyet ölçüsü — döngü yakınsamadığında

Ölçüsüz talimat ikinci modele her turda yeni bir kenar durum buldurur; kabul edilen her kenar durum dokümana yeni bir yüzey ekler ve döngü yakınsamaz. Ciddiyet ölçüsü talimatı dört sınıfla sınırlar: **(1)** yürürlükteki mevzuata aykırılık · **(2)** kullanıcıya ya da işletmeye para kaybı veya hak kaybı doğuran kural boşluğu · **(3)** kullanıcının takılıp kaldığı, çıkışı olmayan bir akış · **(4)** dokümanın iki yerinin birbiriyle çelişmesi. Nadir kenar durumları, iyileştirme önerileri, savunmada derinlik önerileri, üslup ve ifade tercihleri bulgu yapılmaz. Yedi kriter, bulgu biçimi, girdi kuralı (madde 2) ve bilinçli karar cümlesi (madde 4) değişmez; Faz 2'nin değerlendirmesi ve hukuki kontrol aynen sürer.

**Ne zaman uygulanır:**
- **Döngü yakınsamıyorsa** — art arda iki turda kabul edilen bulgular dar kenar durumlara inmişse ve doküman kabul edilen bulgularla büyüyorsa, sonraki turdan itibaren. Kanıt: Ürün Gereksinimleri yirmi beş turda TEMİZ dönmedi ve 374 KB'tan 500 KB'a büyüdü; ölçüyle koşulan 26. tur TEMİZ döndü (K-615).
- **Ayrıntısı başka bir dokümanda yaşayan bir kapsam ya da özet dokümanında** — ilk turdan itibaren; o dokümanda kenar durum açmak iki dokümanı birlikte büyütür (MVP Kapsamı, K-644).

Ölçünün hangi turdan itibaren uygulandığı **rapor başlığına** ve **karar kaydına** (gerekçesiyle) yazılır. Ölçü bir aşamanın içinde açılır ve o aşamanın dokümanlarıyla sınırlıdır; sonraki aşamada yeniden değerlendirilir.

---

## Faz 2 — Bağımsız değerlendirme

> **KRİTİK KURAL:** Her bulguya otomatik "katılıyorum" deme. **Rubber stamp yasaktır.**

Her bulgu için:

1. **Dokümanı bizzat kontrol et** — işaret edilen bölümü oku.
2. **Proje bağlamını değerlendir** — ilgili diğer dokümanları kontrol et. İkinci modelin bunlara erişimi olmadığı için bağlam kaçırmış olabilir.
3. **Karar ver:**
   - ✅ **KABUL** — haklı, düzeltme gerekli; düzeltme önerisi sun
   - ❌ **RET** — yanlış veya bağlamı kaçırıyor; **somut gerekçe zorunlu** (doküman bölümü, proje kararı — "ben böyle düşünüyorum" yetersiz)
   - ⚠️ **KISMİ** — sorun gerçek ama önerilen çözüm uygun değil; alternatif sun
4. **Kaçırdıklarını da raporla** — ikinci modelin listesiyle sınırlı kalma; okurken fark ettiklerini "Ek Bulgular" olarak ekle.

**Objektivite kuralları:**
- **%100 KABUL şüphelidir.** Her zaman kendi analizini yap.
- **%100 RET de şüphelidir.** Savunmacılık, rubber stamp'in aynadaki hâlidir.
- RET gerekçesi somut referans olmalıdır.

---

## Faz 3 — Sunum

1. Rapor dosyasındaki "Bağımsız Değerlendirme" tablosunu doldur.
2. Kullanıcıya özet sun: kaç bulgu geldi, kaçı kabul/ret/kısmi, her biri için kısa açıklama ve karar.
3. Ek bulguları da sun.
4. **Hangi düzeltmelerin uygulanacağına kullanıcı karar verir.** Proje sahibinin açtığı bir mod varsa (öneriyle kayıt, toplu onay ya da ⚠ öneriyle kayıt — `INSTRUCTIONS.md` §2) bulgu kararları o modun işaretiyle kaydedilir ve tur sonunda tek listede bildirilir.

---

## Faz 4 — Düzeltme ve tekrar

1. Onaylanan düzeltmeleri uygula, doküman versiyonunu yükselt.
2. `--round N+1` ile tekrar gönder.
3. Faz 1'den itibaren tekrarla.
4. **Çıkış koşulu:** İkinci model `SONUÇ: TEMİZ` döndüğünde döngü biter. Ciddiyet ölçüsü uygulanıyorsa TEMİZ o ölçüdedir ve rapor bunu yazar.

---

## Faz 5 — Etki yansıtma (ATLANAMAZ)

> İkinci model her dokümanı **izole** okur; cross-document uyumsuzlukları yakalamaz. Bu adım o boşluğu kapatır.

TEMİZ sonrası:

1. **Downstream tarama** — bu dokümanı bağımlılık olarak listeleyen dokümanlar: yapılan düzeltmeler oralarda karşılığını buldu mu?
2. **Upstream tarama** — bu dokümanın bağımlılıkları: cross-review sırasında alınan yeni kararlar kaynak dokümanları etkiliyor mu?
3. **Yeni alan/kural taraması** — cross-review sırasında eklenen her yeni alan, enum değeri, parametre, iş kuralı ilgili dokümanda tanımlı mı?
4. **Kaynak satırları taraması (mekanik)** — doküman bölüm sonlarında Kaynak satırı taşıyorsa: gövdede anılan her karar numarası, bulunduğu bölümün Kaynak satırında da var mı? Betikle karşılaştırılır, gözle değil. **Neden:** etki yansıtma audit turunun içinde yapıldığında Ürün Gereksinimleri'nin üç bölümü (§3, §10, §12) gövdede andığı altı kararı Kaynak satırında taşımıyordu (Aşama 1).
5. **Raporlarda taşınan açık notlar** — turların "önceki turlardan açık kalanlar" listesindeki her not ya bir karar satırının etki sütununa bağlanır ya da kapanışıyla yazılır; raporda kalan not kaybolur. **Neden:** Ürün Gereksinimleri'nin bir notu altı tur raporda taşındı ve hiçbir etki sütununa geçmedi; ancak aşamanın çakışma taramasında kapandı.
6. Uyumsuzlukları hedefli düzeltmelerle kapat; her düzeltme kaydedilir.

> **Vaka:** Bir kodlama kılavuzunun cross-review'ında eklenen üç yeni alan, veri modeli dokümanında tanımlı değildi. Ne audit ne cross-review yakaladı — **etki yansıtmanın ardından çalıştırılan checkpoint** yakaladı. Kalite döngüsünün tamamı gerekli.

---

## Rapor dosya yapısı

```
Docs/CROSS_REVIEW_REPORTS/
├── XX_CROSS_REVIEW.md      # Tur 1
├── XX_CROSS_REVIEW_R2.md   # Tur 2
└── …
```

Her rapor: ham bulgular · bağımsız değerlendirme tablosu (KABUL/RET/KISMİ + gerekçe) · ek bulgular · kullanıcı onay checklist'i · (son turda) etki yansıtma sonucu.

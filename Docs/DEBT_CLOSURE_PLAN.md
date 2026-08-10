# Shopfolio — MVP Borç Kapatma Planı

**Son güncelleme:** YYYY-AA-GG | **Durum:** ⬚ Henüz başlanmadı

> **Karar:** Uçtan uca doğrulama fazına başlamadan önce **MVP kapsamındaki tüm ertelenmiş / yarım / not-bırakılmış iş kapatılır. Bu noktadan sonra erteleme yok.**
>
> **Neden bu aşama var (00 §I):** Task-bazlı doğrulama **modülü** doğrular, uçtan uca akışı doğrulamaz. Her task kabul kriterlerini karşılar, validator'ı PASS verir, gate'i geçer — ve buna rağmen ürünün uçtan uca akışı tamamlanmamış olabilir. Kabul kriterleri *"X servisi yazıldı ve test edildi"* der, *"X servisi üretimde çağrılıyor"* demez.
>
> **İlke:** Yalnızca **MVP-dışı** veya **by-design / imkânsız** olanlar hariç tutulur — bunlar erteleme değil, MVP tanımı gereği kapsam dışıdır.

---

## 0. Tarama — bu plan nasıl kuruldu

> **Ne yazılır:** Üç kaynağın birleştirilmesi ve her kalemin nasıl doğrulandığı.

| Kaynak | Ne tarandı | Bulgu sayısı |
|---|---|---|
| `DEFERRED_BACKLOG.md` | Açık kalemler | |
| Kod taraması | "Üretilen ama tüketilmeyen" / "yazılan ama çağrılmayan" / tetikleyicisi olmayan geçişler | |
| Task raporları | "Known limitations / follow-up" bölümleri | |

**Kritik bulgu:** `<Uçtan uca akışın hangi noktada tamamlanamadığı, kod kanıtıyla.>`

---

## 1. Sıra ve bağımlılık tablosu

| Faz | WP | Başlık | Tamamladığı yetenek | Hangi doğrulama senaryosunu açar | Efor |
|---|---|---|---|---|---|
| P1 | WP1 | | | | |

**Bağımlılıklar:** `<hangi paket hangisinin üstüne oturuyor>`
**Şema değişikliği taşıyan paketler:** `<liste>` — gate check yeni migration bekler.
**Önerilen ilk hamle:** `<en temel paket>`

---

## 2. İş paketleri — detay

### WP1 — <Başlık>

> **Durum:** ⬚ Bekliyor

- **Backlog ID'leri:** `<bu paketin kapattığı DEFERRED_BACKLOG kalemleri>`
- **Kanıt:** `<dosya:satır referansları — boşluğun gerçekten var olduğunun kanıtı>`
- **Zaten bağlı olanlar:** `<yanlış alarmı önlemek için: bu alanda çalışan mevcut parçalar>`
- **İş:** `<yapılacak>`
- **🔒 Scope-fence:** `<MVP kapsamının neresine kadar — gold-plating freni>`
- **Şema değişikliği:** `<var/yok — varsa ne>`
- **Efor:** S / M / L · **Açar:** `<hangi E2E senaryosunu>` · **Bağımlı:** `<WPn>`

---

## 3. Kapsam dışı — erteleme DEĞİL

> Aşağıdakiler bu plana **dahil değildir**: MVP-dışı, by-design veya imkânsız. Bunları yapmamak erteleme değildir.

| Kalem | Neden hariç |
|---|---|

---

## 4. Büyüklük ve işletim

- **N iş paketi.** Her biri **normal bir task gibi** işlenir: plan → uygula → **ayrı chat'te validate** → ayrı PR → CI yeşil.
- Her WP biten için `DEFERRED_BACKLOG.md` satırı `✅ Çözüldü → WPn` işaretlenir.
- Bu plan ilerledikçe güncellenir; tarama sırasında stale bulunan backlog satırları düzeltilir.

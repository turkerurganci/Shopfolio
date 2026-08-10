# <PROJE> — UI Specifications

**Versiyon: v0.1** | **Bağımlılıklar:** `02_PRODUCT_REQUIREMENTS.md`, `03_USER_FLOWS.md`, `10_MVP_SCOPE.md` | **Son güncelleme:** YYYY-AA-GG

> **Aşama:** 3 — UI/UX Tasarım · **Rol:** Senior Product Designer / UX Architect
> **Traceability zorunlu: EVET** — §1 tamamlanmadan §3'e (ekran envanteri) geçilmez.
> **Düzey:** Wireframe — bilgi mimarisi ve etkileşim odaklı, pixel-perfect değil.

---

## 1. Traceability Matrix (ÖNCE BU)

> **Ne yazılır:** `02` gereksinimleri + `03` akış adımları → ekran eşlemesi. İleri ve geri izlenebilirlik.
> Eşlenmeyen kaynak madde = **GAP**. GAP'ler proje sahibine sunulur, karar alınır, **sonra** ekran tanımlarına geçilir.

### 1.1 İleri izlenebilirlik (kaynak → ekran)

| Kaynak ID | Kaynak özeti | Ekran | Durum |
|---|---|---|---|

### 1.2 Geri izlenebilirlik (ekran → kaynak)

| Ekran | Beslendiği kaynak(lar) | Durum |
|---|---|---|

### 1.3 Boşluklar (GAP) ve kararlar

| # | Boşluk | Proje sahibi kararı | Nereye yansıdı |
|---|---|---|---|

> **Vaka:** Bu matris bir referans projede 7 boşluk yakaladı — hiçbiri o ana kadar hiçbir dokümanda adreslenmemiş ama arayüzde cevap gerektiren sorulardı.

---

## 2. Ortak bileşen kütüphanesi

> **Ne yazılır:** Tekrar eden UI kalıpları — durum rozetleri, modal'lar, geri sayım göstergeleri, boş/yükleniyor/hata durumları.
> **Ekran tanımlarından ÖNCE gelir.** Sonradan çıkarılırsa ekranlar tutarsız yazılır.

| Bileşen | Ne gösterir | Varyantlar | Kullanıldığı ekranlar |
|---|---|---|---|

## 3. Navigasyon haritası

> **Ne yazılır:** Ekranlar arası geçişler. **Ekran tanımlarından önce** konumlandırılır.

## 4. Ekran envanteri

| # | Ekran | Aktör(ler) | Amaç |
|---|---|---|---|

## 5. Ekran tanımları

### S<NN> — <Ekran adı>

- **Aktör:** ·  **Giriş noktası:** ·  **Çıkış noktaları:**
- **Kullanıcı buraya geldiğinde ilk ne görmeli:**
- **Bilgi hiyerarşisi:** (ne, nerede, hangi öncelikte)
- **Aksiyonlar:** (her aksiyon → hangi akış adımı, hangi doğrulama)
- **Validasyonlar:**
- **Durum × rol varyantları:** (bkz. §6)
- **Boş / yükleniyor / hata durumları:**
- **Responsive notları:**

## 6. Durum × Rol matrisi

> **Ne yazılır:** Her ekran için {ekran × rol × durum} kombinasyonları. Eksik kombinasyonlar burada yakalanır — bir referans projede tek bir detay ekranı 13 durum × 3 rol = ~52 varyant gerektirdi.

| Ekran | Rol | Durum | Ne gösterilir | Hangi aksiyonlar aktif |
|---|---|---|---|---|

## 7. Form ve validasyon envanteri

## 8. Lokalizasyon etkileri

> **Ne yazılır:** Çoklu dil desteğinin **bilgi mimarisine** etkisi — metin uzunluk farkları, tarih/sayı biçimleri, yazı yönü. Bu yüzeysel bir konu değildir; layout ve bileşen boyutlarını etkiler.

## 9. Yönetim (admin) ekranları

> **Not:** Yönetim ekranları toplam ekranların yarısı kadar olabilir. İkincil endişe olarak ele alınmaz.

---

*<PROJE> — UI Specifications v0.1*

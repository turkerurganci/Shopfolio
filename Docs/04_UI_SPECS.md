# Shopfolio — UI Specifications

**Versiyon: v0.1** | **Bağımlılıklar:** `02_PRODUCT_REQUIREMENTS.md`, `03_USER_FLOWS.md`, `10_MVP_SCOPE.md` | **Son güncelleme:** YYYY-AA-GG

> **Aşama:** 3 — UI/UX Tasarım · **Rol:** Senior Product Designer / UX Architect
> **Traceability zorunlu: EVET** — §1 tamamlanmadan §3'e (ekran envanteri) geçilmez.
> **Düzey:** Wireframe — bilgi mimarisi ve etkileşim odaklı, pixel-perfect değil.

> **Aşama 1'den park edilen girdiler** — kaynak: [`PRODUCT_DISCOVERY_STATUS.md`](PRODUCT_DISCOVERY_STATUS.md) §2, mekanizma: K-36.
> Bu satırlar **talimattır, karar değildir** — bağlayıcı olan kaynak karardır; bu aşamada karara bağlanacak olan, talimatın nasıl uygulanacağıdır.
>
> - **K-06 · K-97 · Aktör envanteri:** Ekran × rol matrisi dört aktör üzerinden kurulur — ziyaretçi · misafir alıcı · üye müşteri · firma yöneticisi (`02 §1.3`). Yönetim tarafı **tek roldür** (çoklu kullanıcı, aynı yetki). Misafir alıcıyı K-97 ekledi ve K-06'nın *"üyeliksiz sipariş kararına bağlıdır"* diye açık bıraktığı envanteri kapattı; satır Aşama 2'nin çakışma taramasında hizalandı (2026-10-04).
> - **K-14 · K-574 · Firma kimlik bilgileri:** Firma tipine göre değişen zorunlu kimlik seti ve — firma ETBİS doğrulama bilgisini girdiyse — doğrulama bandı sitede **sürekli erişilebilir** olur; alan boşsa band görünmez (K-574). **Tam yerleşim bu dokümanın kararıdır** — hangi bilgi hangi ekranda ve hangi alanda görünecek.
> - **K-27 · Ana sayfa kompozisyonu:** İki hazır düzen tasarlanır — **tanıtım öncelikli** ve **mağaza öncelikli**. Taban kural: her iki düzende de kurumsal tanıtım ile ürün vitrini ana sayfada **birlikte** bulunur; değişen yalnız ağırlık ve sıradır. Serbest sayfa kurgusu (page builder) kapsam dışıdır.

> **Aşama 2'den park edilen girdiler** — kaynak: [`PRODUCT_DISCOVERY_STATUS.md`](PRODUCT_DISCOVERY_STATUS.md) §2, mekanizma: K-36 (yeri: §8.3 AK0-04).
> Bu satırlar **talimattır, karar değildir** — bağlayıcı olan kaynak karardır; bu aşamada karara bağlanacak olan, talimatın nasıl uygulanacağıdır.
>
> - **Dizin — Aşama 2 kararlarının bu dokümana devirleri:** karar kaydında K-647…K-718'in etki sütunlarında bu dokümanı gösteren otuz beş atıf (otuz beş karar) ve Kullanıcı Akışları'nın (`03`) gövdesinde bu dokümana iş bırakan cümleler [`CHECKPOINT_REPORTS/PHASE2_CONFLICT_SCAN.md`](CHECKPOINT_REPORTS/PHASE2_CONFLICT_SCAN.md) §6'dadır; etki sütununda yalnız doküman numarası taşıyan sekiz atfın işinin adını dizin verir. Bir iş yalnız `03`'ün gövdesinde yaşar, karar satırı yoktur: yeniden gönderilebilen e-postaların kapsamı ve firma bildirimlerinin "e-posta ulaşmadı" işaretinin biçimi (`03 §7.1.37`, §8.9.2, §10.1.2.2). Devir taraması (`checklists/document-stage.md` §4) buradan başlar; devrin evi karar satırıdır, dizin onu ikinci kez kaydetmez. Aşama 1'in dizini: `PHASE1_CONFLICT_SCAN.md` §6.

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

*Shopfolio — UI Specifications v0.1*

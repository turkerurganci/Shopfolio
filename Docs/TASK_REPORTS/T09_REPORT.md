# T09 — İlk yayın ve referans kuruluma ilk iniş

**Faz:** F0 | **Durum:** ⛔ BLOCKED (OWNER_INPUT) — hazırlık yapıldı | **Tarih:** 2026-11-03

---

## Sahipten beklenen girdiler

Aşağıdakilerin hepsi geldiğinde T09 tek oturumda biter (`DEPLOY_RUNBOOK` §K sırayla koşar). Sırlar sohbete yazılmaz; sunucuya ya da proje sahibinin kendi makinesine doğrudan konur.

1. **Referans sunucu:** Linux, `iptables` kurulu, kök yetkili kullanıcı, kamu IPv4 ve IPv6 adresi, açık kapılar 80, 443 ve SSH; adresi ve SSH kullanıcısını (anahtar girişi, parola girişi kapalı) proje sahibi bildirir.
2. **SSH erişimi:** kurulumu yapanın genel anahtarı sunucunun yetkili anahtarlar dosyasında olmalı ya da özel anahtar kurulumu yapan makinede bulunmalı; anahtarın parolası kurulum başına ayrıdır.
3. **Alan adı ve DNS:** `SHOPFOLIO_DOMAIN` değeri (örn. `magaza.firma.com`) ve bu adın hem `A` (IPv4) hem `AAAA` (IPv6) kaydının sunucuya baktığı; değer `/etc/shopfolio/settings.env`'e yazılır.
4. **Nesne depolama kovası:** firmanın hesabında S3 uyumlu bir kova; sunucu adı (`BACKUP_S3_ENDPOINT`, `https://` olmadan), bölge (`BACKUP_S3_REGION`), kova adı (`BACKUP_S3_BUCKET`) ve adres biçimi (`BACKUP_S3_URI_STYLE`: `host` ya da `path`); değerler `/etc/shopfolio/settings.env`'e yazılır.
5. **Kova yönetim kimlik bilgileri:** kovada sürüm tutmayı ve yaşam döngüsü kuralını kurabilen geçici yönetim anahtarı (kimlik ve gizli anahtar); yalnız kurulumu yapanın makinesinde `AWS_ACCESS_KEY_ID` ve `AWS_SECRET_ACCESS_KEY` ortam değişkeni olarak verilir (`DEPLOY_RUNBOOK` §J.3), repoya ve sohbete yazılmaz.
6. **İki yedek öneki için iki erişim anahtarı:** biri `database/`, öteki `files/` önekine yazan, okuyan ve güncel nesneyi silen; kimlik ve gizli anahtar iki satırlı dosya olarak `db_backup_storage_key` ve `file_backup_storage_key` sır dosyalarına yazılır (yetki şablonu `deploy/backup/key-policy.template.json`).
7. **İki şifreleme parolası:** `db_backup_cipher_key` ve `file_backup_cipher_key` kurulumda kriptografik üreteçle (en az 32 karakter, iki ayrı değer) üretilir; proje sahibi **ikinci kopyayı** güvenli bir yerde saklar (kurulumda firmaya teslim edilir; döndürülemez).
8. **Kurulum sırları (üretilen):** `db_superuser_password`, `db_migrator_password` ile `db_setup_migrator_password` (aynı değer), `db_app_connection` ile `db_setup_app_password`, `db_reader_connection` ile `db_setup_reader_password`, `link_derivation_secret` sunucuda kurulumu yapan tarafından üretilir; proje sahibinden girdi gerekmez, yalnız kurulumu yapan kişinin sunucuya yazma yetkisi gerekir.
9. **Sağlayıcı sırları (12, 13, 14):** `payment_provider_keys` (PayTR), `email_credential` (e-posta sağlayıcısı), `google_client_secret`; ilk inişte canlılık ucu bunlara bakmaz, bu yüzden proje sahibi ya gerçek değerleri sır dosyalarına yazar ya da biçimce geçerli uydurma değerle başlanmasını onaylar (uydurma dosyalar prova kaydına yazılır).
10. **GitHub tarafı:** repo sahibi hesabıyla (a) `v0.1.0` açıklamalı etiketinin basılmasına, (b) üç imaj paketinin (`shopfolio-app`, `shopfolio-database`, `shopfolio-backup`) **Public** yapılmasına (geri alınamaz) açık onay verir; etiket ve paket açılışı sunucu hazır olduktan sonra, aynı oturumda yapılır.
11. **`SETUP.md` değişikliği onayı:** `SETUP_COMPLETE=true` PR'ında `SETUP.md` §6'daki "yayın izni gerekmedi" maddesinin güncellenmesi için proje sahibinin onayı (`GUARDRAILS` §2).

---

## Hazırlık (sunucusuz, bu oturumda yapıldı)

- `Docs/DEPLOY_RUNBOOK.md`: yeni **§K "Referans kuruluma ilk iniş"** (K.1–K.12): sunucu ve Docker hazırlığı, IPv6 ayarı, ayar ve sır dosyaları, ilk yayın (etiket, paketler, özet, `release.env`, yayından sonraki CI koşusu), `DOCKER-USER` kuralı ile açılış ve zamanlayıcı birimleri, ilk kurulum bindirmesi, canlılık denemesi, `installation-checks.sh` koşumları, yedek (§J'ye bağlı), imaj özeti kaydı, prova kaydı tablosu, kapanış; §0 hızlı özet ve §I.2 §K'ya bağlandı. Reçete T09'a bağlı backlog kalemlerine bağlandı (K.12 tablosu).
- Sunucusuz kapanan backlog kalemleri: **D-34 (c)** (`installation-checks.sh` başlığı), **D-34 (d)** (`11`'de T09 "Dokümanlar" satırına ve kapsama tablosuna `MK-14`), **D-29 (a)'nın `CI_CD_SETUP.md` §3.1 kısmı**; hepsi "✅ Çözüldü → T09 hazırlık" diye işaretlendi. D-29 (a)'nın `SETUP.md` kısmı ve (b) kapanış PR'ında kalır.
- Sunucuda koşacak, bu oturumda kapanamayanlar: D-01, D-29 (a) `SETUP.md`, D-30 (a), D-31 (a)–(g), D-39 (b), (c).
- Yerel doğrulama: `Consistency` özellikli 34 test geçti.
- Yapılmadı (talimat gereği): sürüm etiketi, `publish.yml` tetikleme, paket görünürlüğü, `SETUP_COMPLETE`, D-01 kapatma, PR.

## Notlar

- Alt tür: `EXTERNAL_BLOCKER` (proje sahibi girdisi). Yardımcı bloku PR #352 ile kaydetti.

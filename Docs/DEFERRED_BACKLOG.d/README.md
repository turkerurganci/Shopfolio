# DEFERRED_BACKLOG parçaları

Task, düzeltme ve doğrulama commit'leri yeni backlog kalemlerini `<TXXX>.md` dosyasına yazar (task başına tek dosya; başlık `# <TXXX> backlog kalemleri`, ardından `Docs/DEFERRED_BACKLOG.md` bölüm 2 tablosunun sütunları: `| Önc. | ID | Açıklama | Tip | Hedef | Kanıt / kaynak |`); `DEFERRED_BACKLOG.md` tablosuna satır eklemez.
Var olan satırı kapatmak (✅) yerinde yapılır; henüz derlenmemiş bir parçadaki kalem o parçada kapatılır.
Faz kapısı kayıt PR'ı ve chore/kayıt PR'ı parçaları D numarası sırasıyla bölüm 2 tablosunun sonuna aktarır, parça dosyalarını siler (bu README kalır) ve "Aktif kalem" sayacını yeniden sayar.
Kalem araması iki yere bakar: `grep -rn 'D-123' Docs/DEFERRED_BACKLOG.md Docs/DEFERRED_BACKLOG.d/`.
Neden: paralel task PR'ları tablonun sonuna aynı anda satır ekleyip birbiriyle çakışıyordu (2026-11-08, sayaç ve changelog kuralının devamı).

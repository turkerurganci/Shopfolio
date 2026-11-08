# STATUS_CHANGELOG parçaları

Task, düzeltme ve doğrulama commit'leri changelog girişlerini `<TXXX>.md` dosyasına yazar (task başına tek dosya, yeni giriş en üstte); `STATUS_CHANGELOG.md`'ye dokunmaz.
Faz kapısı kayıt PR'ı ve chore/kayıt PR'ları parçaları tarih sırasıyla `STATUS_CHANGELOG.md`'ye aktarır ve parça dosyalarını siler (bu README kalır).
Neden: herkes `STATUS_CHANGELOG.md`'nin başına eklediği için açık PR'lar çakışıyordu (2026-11-08).

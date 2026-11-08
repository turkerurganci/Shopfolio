## 2026-11-08 — chore: Yedek imajında restic indirmesi tek kaynağa bağlı olmasın

2026-11-08 — GitHub sürüm indirmesi 504 dönünce CI "7. Paketleme" adımı ve main kırmızı oldu; yedek imajı Dockerfile'ında restic indirmesi beş denemeyle (10 sn arayla) tekrarlanır, SHA-256 doğrulaması aynen kalır (yol b: resmî `restic/restic` imajından kopyalama CI'daki "yanlış özet derlemeyi düşürür" sınamasını, yani `scripts/ci/package-build.sh` adım 3'ü, bozacaktı; yerel derleme ve `restic version` 0.19.1 ✓, betik testleri ✓).

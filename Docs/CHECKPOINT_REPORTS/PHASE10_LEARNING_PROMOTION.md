# Aşama 10 — Öğrenim Terfisi

**Tarih:** 2026-11-02 | **Aşama:** 10 — Doğrulama Protokolü | **Kapanış adımı:** K-437'nin 5. adımı (`00 §K`; `checklists/document-stage.md` §7 madde 5)
**Girdi:** karar kaydı v0.328 — Aşama 10'un süreç kararları (K-1504…K-1572), [çakışma taraması](PHASE10_CONFLICT_SCAN.md), [checkpoint](CP10_PHASE10_CHECKPOINT.md) §6; işçilerin devir notları ve yönetici yardımcının ilerleme notu; `Docs/PLAYBOOK_FEEDBACK.md` (PF-152…PF-162); kullanıcı hafızasının indeks satırları (salt okunur); `~/.claude/logs/token-usage.log` ve `subagent-calls.log` — `origin/main` @ `d5d35e4`
**Çıktı:** karar kaydı v0.329 · `12` v0.33 · `.claude/INSTRUCTIONS.md` §7, §9 · `.claude/skills/validate`, `checkpoint`, `audit` · `.claude/checklists/document-stage.md` §3, §7 · `scripts/doc-scans/karar_bolum_denetle.py` (yeni), `geri_isaret_tara.py`, `geri_isaret_hukum.txt` (yeni), `12_matris_denetle.py`, `12_devir_esle.py`, `12_kapsam_hizasi.py`, `12_acilis_tarama.py` · `.claude/memory/MEMORY.md` · `Docs/PLAYBOOK_FEEDBACK.md` 163–168 · karar K-1573 — `00`, `CLAUDE.md`, `SETUP.md`, `GUARDRAILS.md`, `CONTEXT.md` değişmedi

> **Ne işe yarar:** `00 §K` gereği aşama, öğrenimi yazılmadan kapanmaz. Bu rapor Aşama 10 boyunca kayda, raporlara, işçi devir notlarına ve hook günlüğüne düşen her öğrenim adayının sonucunu yazar. On beş aday L3–L5'e terfi etti ve bu PR'da uygulandı. Maliyet ölçümü §3'te; checklist'in skill'e dönüşmesi §4'te; proje sahibinin cevabını bekleyen tek soru §5'te, ayrı numaralı, sade dille, K numarasız. Toplu onay (K-1504) açık olduğu için `00`, `CLAUDE.md` ve `SETUP.md` dışındaki kararlar önerimle kaydedildi (§5.1, "toplu onayla kaydedildi"); `00 §N.1` önerisi (Soru 1) uygulanmadı (GUARDRAILS §2).

---

## 1. Sonuç

| Sonuç | Sayı | Adaylar |
|---|---|---|
| **Terfi etti — bu PR'da uygulandı** (L3, L4, L5) | 15 | Ö-186, Ö-187, Ö-189, Ö-190, Ö-191, Ö-192, Ö-194, Ö-195, Ö-196, Ö-197, Ö-198, Ö-199, Ö-204, Ö-207a, Ö-207b |
| **Önceden terfi etmişti** | 1 | Ö-188 (ikinci model kuralı, K-1484) |
| **Raporda kalır** — tek seferlik gözlem, bilgi ya da proje dışı öneri | 7 | Ö-193, Ö-200, Ö-201, Ö-202, Ö-203, Ö-205, Ö-206 |
| **Yeniden değerlendirildi, sorulmaz** | 1 | Ö-23 |
| **Proje sahibine soruluyor** | 1 soru | `00 §N.1`'e Aşama 10'un dersleri (Soru 1) |
| **Toplu onayla kaydedilen kararlar** | 3 | Karar A, B, C (§5.1) |

Toplam yirmi dört aday kimliği. Terfilerin 14'ü ayrı değişiklik (Ö-192 ve Ö-198 `INSTRUCTIONS §9` (11)'de tek maddedir). ⚠ taşıyan karar yok; ⚠ eşitlemesi 8 / 8 değişmedi (§8).

**Aşamanın asıl öğrenimi.** Doğrulama Protokolü (`12`, ~800 KB, 1.503 matris satırı) uzun bir aşamaydı ve Aşama 9'un ağır dersleri tuttu: yönetici düzeni sorunsuz yürüdü, 60 KB mercek kuralı çalıştı (checkpoint altı parça yerine tek parça, 20 / 20 mercek bitirdi; Ö-203). Üç şey belirgin: (1) **Sayaç, sayı ve ajan sözü dosyayla sınanmadıkça güvenilmez**: doğrulayıcı sayacı adım adını aşamasız tuttu (22 engel), bir merceğin "plan.md'ye yazdım" sözü boştu, `12 §1.2`'nin elle yazılmış sayım tablosu üç satır saptı ve betik bunu ancak bu adımda yakaladı (Ö-186, Ö-192, Ö-195). (2) **Karar kaydı ve yazım araçları sessizce yanlış yere yazabiliyor**: 29 madde yanlış bölüme (§22.1, §12.1) yazılmış, üç betik CRLF yazmıştı (Ö-190, Ö-199). (3) **Betik kapıları süreci aşar**: hükümlü geri işaret çiftleri, bölüm denetimi ve sayım karşılaştırması artık betikte, el hesabında değil (Ö-190, Ö-191, Ö-195, Ö-196).

---

## 2. Adaylar ve sonuçları

**Kimlikler:** Ö-186 Aşama 9'un Ö-185'inden sürer. Ö-186…Ö-189 ve Ö-192…Ö-194 işçi devir notlarından ve `INSTRUCTIONS §9` ihlallerinden, Ö-190, Ö-191, Ö-195 ve Ö-196 çakışma taraması ile checkpoint'in betik bulgularından, Ö-197, Ö-203 ve Ö-204 CP10 §6'dan, Ö-199 checkpoint'in sürüm ve paket betiklerinin CRLF'inden, Ö-200…Ö-202 hook günlüğünden ve `token-usage.log`'dan, Ö-205 ve Ö-206 Implementation devri taramasından, Ö-207a ve Ö-207b dönem geçişi taramasından çıkarıldı. Ö-23 Aşama 1'den gelir.

| # | Kaynak | Aday | Sınıf (`00 §K`) | Sonuç | Hedef katman ve dosya | Uygulanan metin (özet) | Playbook |
|---|---|---|---|---|---|---|---|
| Ö-186 | `KD-audit-2b` engeli (22 engel) | Doğrulayıcı sayacı adım adını aşamasız tuttu; Aşama 9'un aynı adlı adımları 20'yi doldurmuştu | İhlali önleyen kural | **Terfi etti** | L3 `INSTRUCTIONS §9` (1) | Adım adı aşama önekini taşır (`A10-KD-audit-2b`; Implementation'da `T101-dogrulama`) | PF-163 |
| Ö-187 | A10-KD-deep-2 | 20'lik tek mesaj "eşzamanlı 20" hatasına takıldı; yeniden başlatma işi tekrarladı | İhlali önleyen kural | **Terfi etti** | L3 `INSTRUCTIONS §9` (5) | Tek mesajda en çok on çağrı; doğrulayıcılar onarlı dalga | PF-163 |
| Ö-188 | K-1484, tur 5 | İkinci model: biri dolarsa diğeri, tur bölünmez, sağlık çağrısı elenir | İhlali önleyen kural | **Önceden terfi etmişti** (K-1484, `skills/cross-review`, betik) | — | Aşama 10'da işledi: cursor sağlık çağrısı elendi, 5. tur 00:12'ye kadar bekledi (tur bölünmedi) | PF-142'nin devamı |
| Ö-189 | Y3b-2, Y4-1, çakışma taraması | Yedi işçi bağlam sınırında bölündü; üçünde iş yalnız scratchpad'de kaldı | İhlali önleyen kural | **Terfi etti** | L3 `INSTRUCTIONS §9` (8) | İlk ara commit 150 binde; uygulama betiği de commit edilir | PF-163 |
| Ö-190 | `PHASE10_CONFLICT_SCAN` §3 | 29 kalite döngüsü maddesi karar kaydında yanlış bölümde (§22.1, §12.1); elle bulundu | İhlali önleyen kural | **Terfi etti** | L5 `scripts/doc-scans/karar_bolum_denetle.py` (yeni) · L4 checklist §7 adım 3 | Betik eski tracker üzerinde 29 aday verdi (rapor 29 ile aynı), güncel kayıtta 0 | PF-164 |
| Ö-191 | PHASE10_CONFLICT_SCAN §4 | `geri_isaret_tara.py` her koşumda aynı yedi çifti yeniden aday sayıyor; hüküm betiğin dışında | Tekrarlanacak desen → kural | **Terfi etti** | L5 `geri_isaret_tara.py`, `geri_isaret_hukum.txt` (yeni) · L4 checklist §3 | Hükümlü çiftler dosyada; hükümsüz çift `missing`; `1504 1572` → missing 0, judged 8 | PF-164 |
| Ö-192 | A10-KD-deep-2a Parça B | Mercek "plan.md'ye yazdım" dedi, dosya boştu; dönüş metni kanıt sayıldı | İhlali önleyen kural | **Terfi etti** | L3 `INSTRUCTIONS §9` (11) | Ajanın sözü ve görev metnindeki sayı kanıt değildir; dosya ve betik sayısıyla sınanır | PF-163 |
| Ö-193 | A10-KD-audit-2 | Seçim betiğine sayı doğrulaması (assert) eklendi | Tek seferlik gözlem | **Raporda kalır** — Ö-192'nin yerel örneği | — | — | — |
| Ö-194 | audit-2 (7 / 20 mercek) | Merceklerin 7'si bulgularını yalnız sonda yazdı; "bulduğun anda" kuralı uygulanmadı | İhlali önleyen kural | **Terfi etti** | L4 `skills/audit` "Koşum biçimi" 5. madde | Yönetici bulgu dosyasını başlık satırıyla önceden yaratır, yolunu talimata yazar | PF-165 |
| Ö-195 | `12` sayım tablosu | `12 §1.2` elle yazılmış `Kanıt ortamı` sayımı saptı (`yok` 1.243 · `mock` 198; doğrusu 1.240 · 201); hiçbir kapı görmedi | İhlali önleyen kural | **Terfi etti** | L5 `12_matris_denetle.py` (1.2 tablosu betikle karşılaştırılır) | Fark bu adımda bulundu ve `12` v0.33'te düzeltildi | PF-165 |
| Ö-196 | CP10 / matris denetimi | Severity dağılımı ve isteğe bağlı kaynak listesi betikte yoktu | Tekrarlanacak desen → kural | **Terfi etti** | L5 `12_matris_denetle.py` (`--severity`) | 67 kaynak / 7 gerçek; sayımlar betikle | PF-165 |
| Ö-197 | CP10 §6.5 | Üst doküman hizası checkpoint PR'ından sonraya kaldı | İhlali önleyen kural | **Terfi etti** | L4 `skills/checkpoint` "Koşum" | Üst doküman notu aynı PR'da yazılır | PF-165 |
| Ö-198 | Ö-192 ile aynı gözlem | Görev metnindeki sayı da ajan sözü gibi kanıt sayıldı | İhlali önleyen kural | **Terfi etti** (Ö-192 ile tek madde) | L3 `INSTRUCTIONS §9` (11) | Sayıyı dosyadan sayma kuralı | PF-163 |
| Ö-199 | `11_geri_yon.py` (Aşama 9), checkpoint'in sürüm ve paket betikleri | Windows'ta `Path.write_text` CRLF yazıyor; ikinci aşamada aynı hata | İhlali önleyen kural | **Terfi etti** | L3 `INSTRUCTIONS §7` (1) · L5 `12_devir_esle.py`, `12_kapsam_hizasi.py`, `12_acilis_tarama.py` (`newline="\n"`) | Dokümanı ya da kaydı yazan her betik `newline="\n"` ya da `write_bytes` kullanır | PF-166 |
| Ö-200 | hook günlüğü | Aşama 10'da yaklaşık 103 hook engeli: heredoc ailesi 70, sayaç 22, bağlam 10, büyük dosya 1 (Aşama 9: 125) | Tek seferlik gözlem | **Raporda kalır** — hook proje dışıdır ve işini yaptı; heredoc kuralı `INSTRUCTIONS §7`'de yazılı, işçi talimatına geçmiyor (P-2) | — | — | PF-152'nin devamı |
| Ö-201 | tur 6 | Harness ön plan komutunu arka plana aldı | Tek seferlik gözlem | **Raporda kalır** — harness davranışı; `run_in_background: false` kuralı (`audit` Koşum 5) zaten yazılı | — | — | — |
| Ö-202 | `token-usage.log` | Aşama 10 maliyeti: 686,7 M (§3) | Tek seferlik gözlem | **Raporda kalır** — ölçüm §3'te, karar kaydında K-1573 | — | — | — |
| Ö-203 | CP10 | ≤ 60 KB mercek kuralı (Ö-164) doğrulandı: 20 / 20 mercek bitirdi, açık bölüm 0; Aşama 9 checkpoint'i 687 M, Aşama 10 61 M | Tek seferlik gözlem (olumlu) | **Raporda kalır** — kural zaten `skills/checkpoint`'te | — | — | — |
| Ö-204 | CP10 | Ortam ya da severity farkı bölüm girişinde açıklanmış olabilir; mercek bunu bilmeden bulgu açabilir (çürük bulgular arasında kasıtlı fark örnekleri var) | İhlali önleyen kural | **Terfi etti** | L4 `skills/checkpoint` "Koşum" | Mercek talimatı: önce bölüm girişini oku, açıklanmışsa bulgu açma | PF-165 |
| Ö-205 | Implementation devri taraması | `task` ve `gate-check` skill'lerinin `IMPLEMENTATION_STATUS.md` kullanımı tarandı; ek kural gerektiren boşluk bulunmadı (matris `Durum` hücresi Ö-207b'de) | Tek seferlik gözlem | **Raporda kalır** — değişiklik gerekmedi | — | — | — |
| Ö-206 | Aşama 10 süreç kararları | K-1504…K-1572'nin süreç grupları (açılış, yazım, audit, deep review, cross-review 1–6, etki, çakışma, checkpoint) | Karar kaydında yaşayan | **Dokümanda ve skill'lerde yaşar** — yöntem dersleri Ö-186…Ö-204'te | — | — | — |
| Ö-207a | Dönem geçişi | `Docs/IMPLEMENTATION_STATUS.md` boş şablonda; task sayısı elle yazılırsa Aşama 10'un `1.503` hatası tekrarlanır | İhlali önleyen kural | **Terfi etti** | L4 checklist §7 adım 6 | Boş şablon `11`'in faz ve task listesiyle doldurulur; sayı betikle | PF-167 |
| Ö-207b | Dönem geçişi | `validate` adım 16 matris `Durum` hücrelerini yazar ama sonra kapı koşmaz | İhlali önleyen kural | **Terfi etti** | L4 `skills/validate` adım 16 | Hücreler yazıldıktan sonra `12_matris_denetle.py --kapsam 1-77` hata 0 | PF-167 |

---

## 3. Aşama 10'un maliyet ölçümü (Ö-202)

Kaynak: `~/.claude/logs/token-usage.log` (oturum `62c460d3`, UTC farkları; adım değerleri yaklaşıktır) ve `subagent-calls.log` (12:45Z sonrası). Kapanış adımları 3–5 (çakışma, öğrenim, bu adım) hariç, checkpoint'e kadar toplam **686,7 M işlenen**, 42,8 M yeni metin, 6.822 model çağrısı; Opus 294,1 M, Sonnet 392,7 M. Alt ajan çağrıları: 221 Opus / `high` doğrulayıcı, yaklaşık 171 Sonnet / Haiku, 4 yönetici.

| Adım | İşlenen (M) |
|---|---|
| Yazım turu (açılış dahil) | ~151 |
| Audit | 75,7 |
| Audit-2 (2a…2r) | 80,7 |
| Deep review (Parça A 86,1; 2a 58,7; 2b 39,3) | 184,1 |
| Cross-review (7 adım) | 82,6 |
| Etki yansıtma | 30,6 |
| Çakışma taraması | 20,7 |
| **Checkpoint** | **61,1** |

Aşama 9 2.244,9 M idi (3,3 kat); Aşama 10 yaklaşık dörtte üçü kadar tutar (kapanış adımları hariç). Checkpoint 687 M'den 61 M'ye düştü: 60 KB mercek kuralı ve kapsam tablosu (Ö-203) altı parçayı tek parçaya indirdi. Doküman 800 KB olmasına rağmen yönetici payı aynı düzenle yönetildi.

---

## 4. Ö-23 — checklist skill'e dönüşür mü

**Sonuç: sorulmaz** (K-1399). İki koşul ölçüldü: (1) matris Aşama 10'da zorunluydu ve beşinci kez işletildi (✓); (2) kapanış yeni adım ya da madde almadı — `grep -c '^- \[ \]' .claude/checklists/document-stage.md` → 67 (başlangıç 67); değişen yalnız iki madde genişlemesi (§3 geri işaret; §7 adım 3 ve 6). Başlık notuna dokunulmadı.

---

## 5. Proje sahibinin cevabını bekleyen soru ve toplu onayla kaydedilenler

### 5.1 Toplu onayla cevaplanan kararlar (K-1504)

Öğrenim soruları önerimle cevaplandı ve `(toplu onayla kaydedildi)` işaretiyle K-1573'e yazıldı. Üçü de geri alınabilir süreç kararıdır, ⚠ grubuna girmez.

- **Karar A — L3–L5 hedefli on beş terfi bu PR'da uygulanır** (GUARDRAILS §2'nin dışında kalan hedefler). Öneri: uygula. *(toplu onayla kaydedildi)*
- **Karar B — `12_*` betikleri `12`'ye özeldir; Implementation döneminde taşınmaz.** `karar_bolum_denetle.py` ve `geri_isaret_tara.py` genel kalır, `12_*` betikleri arşivlenir. Öneri: böyle. *(toplu onayla kaydedildi)*
- **Karar C — hook ve proje dışı dosyalara dokunulmaz; üç düzeltme §5.2'de bilgi olarak durur.** Öneri: bilgi olarak. *(toplu onayla kaydedildi)*

### 5.2 Proje dışı öneriler (`~/.claude`; bilgi, soru değil)

- **P-1 — doğrulayıcı sayacı anahtarı.** Sayaç anahtarına aşama ya da oturum eklenmeli ya da 24 saatte sıfırlanmalı; adım adı aşamasız kaldığı için Aşama 10'un `KD-audit-2b` adımı ilk doğrulayıcıda engellendi (22 engel). L3'te adım adına aşama öneki konuldu (Ö-186), ama sayacın kendisi proje dışıdır.
- **P-2 — işçi talimat şablonuna sabit yasak satırı.** Aşama 9'da 97, Aşama 10'da 70 heredoc engeli; işçiler yasağı talimatta görmeden denedi. "Kabukta heredoc, here-string, `python -c`, `node -e`, `python -` yok; betik `Write` ile" satırı yönetici şablonuna sabit eklenir (Aşama 9 PF-152 hâlâ açık).
- **P-3 — CR tarayıcı hook'u.** `INSTRUCTIONS §7` (1) tarama kuralı elle işletiliyor; Windows'ta CRLF yazan her araç sessizce bozuyor (Ö-199). Bir hook ya da CI adımı değişen dosyalarda CR arayabilir.

### 5.3 Proje sahibine soru

#### Soru 1 — Yöntem dosyasına Aşama 10'un dersleri (`00 §N.1`)

> **Sorun:** Yöntem dosyası her aşamanın tekrarlanan derslerini kısa bir paragrafla tutuyor; Aşama 10 için henüz yok. **Ne değişir:** Evet derseniz aşağıdaki dört ders yöntem dosyasına yazılır (sonraki projelerin aynı hataya düşmemesi için); hayır derseniz dersler yalnız bu raporda ve çalışma kurallarımızda kalır. Dört ders: (1) bir ajanın "yaptım" ya da "şu kadar bulgu" sözü kanıt değildir; sözü dosyaya ve sayıma karşı denemek gerekir — Aşama 10'da bir kontrol ajanı "yazdım" dedi, dosya boştu; (2) sayaçlar ve sınırlar adımın adına ve aşamaya bağlı olmalı, yoksa önceki aşamanın aynı adlı adımı sınırı doldurmuş olarak kalır ve işi durdurur; (3) kural koyan metnin kendisi bir sayıya ya da yere elle yazılmışsa, bir betik onu karşılaştırmadan yanlış kalabilir — Aşama 10'da elle yazılmış bir sayım tablosu üç satır yanlıştı ve hiçbir kontrol görmedi; (4) aşama sonunda yalnız o aşamanın değil, bir sonraki dönemin başlangıç dosyalarının da (durum tablosu, şablon) hazır olup olmadığına bakılır.
> **1 ·** dört ders yöntem dosyasına yazılsın (öneri) · **2 ·** yazılmasın
> Kaynak: Ö-186, Ö-192, Ö-195, Ö-198, Ö-207a · `00 §N.1` · PF-168

**Uygulanacak metin (1 ise).** `00 §N.1`'e, Aşama 9 paragrafının altına: *"**Aşama 10 — Doğrulama Protokolü (2026-11-02).** Kurallar `.claude/INSTRUCTIONS.md` §7, §9, `.claude/skills/audit/SKILL.md`, `.claude/skills/checkpoint/SKILL.md`, `.claude/skills/validate/SKILL.md`, `.claude/checklists/document-stage.md` §3, §7 ve rapor `Docs/CHECKPOINT_REPORTS/PHASE10_LEARNING_PROMOTION.md`. Tekrarlanacak desenler: 1. **Ajan sözü ve görev metnindeki sayı dosyayla sınanır.** Bir mercek 'plan.md'ye yazdım' dedi, dosya boştu; görev metnindeki 'N bulgu' sayısı gerçekle uyuşmadı. Kural: iş, dönüş metnine değil dosyaya ve betik sayımına dayanır. 2. **Sayılar ve eşikler tek mesaj ya da adım toplamıdır; sayaç adı aşama önekli olur.** Doğrulayıcı sayacı adım adını aşamasız tuttu ve önceki aşamanın aynı adlı adımları 20'yi doldurmuştu (22 engel); 20'lik tek mesaj 'eşzamanlı 20' hatasına takıldı. Kural: adım adı aşama önekini taşır, tek mesajda en çok on çağrı. 3. **Üst doküman hizası aynı PR'dadır.** Bir kuralı düzelten bulgu bir üst dokümanın lafzını da etkiliyorsa not o PR'da yazılır (CP10 §6.5); elle yazılmış sayım tabloları betikle karşılaştırılır (`12 §1.2` üç satır saptı). 4. **Dönem sonu aşama, sonraki dönemin girdisini de tarar.** Doküman döneminin son aşaması `IMPLEMENTATION_STATUS.md` şablonunu `11`'in faz ve task listesiyle doldurur ve `validate` skill'i matris kapısını koşar; karar kaydı bölümü ve geri işaret hükümleri betikle denetlenir (Aşama 10: 29 madde yanlış bölümde, yedi çift her koşumda yeniden aday)."* `00` sürümü +1. PF-168 `Uygulandı` olur.

---

## 6. Uygulanan terfiler — dosya ve bölüm

| Dosya | Bölüm | Değişiklik | Aday |
|---|---|---|---|
| `.claude/INSTRUCTIONS.md` | §9 — "Alt ajanların koşumu" (1) | Adım adı aşama önekini taşır (`A10-…`, `T101-…`) | Ö-186 |
| | §9 — (5) | Tek mesaj en çok on çağrı; doğrulayıcılar onarlı dalga | Ö-187 |
| | §9 — (8) | İlk ara commit 150 binde; uygulama betiği de commit edilir | Ö-189 |
| | §9 — (11) | Ajan sözü ve görev metnindeki sayı kanıt değildir | Ö-192, Ö-198 |
| | §7 — (1) | Betik `newline="\n"` ya da `write_bytes` | Ö-199 |
| `.claude/skills/audit/SKILL.md` | "Koşum biçimi" 5. madde | Bulgu dosyası mercek başlamadan yaratılır | Ö-194 |
| `.claude/skills/checkpoint/SKILL.md` | "Koşum" | Üst doküman hizası checkpoint PR'ında; mercek talimatı "açıklanmış fark" | Ö-197, Ö-204 |
| `.claude/skills/validate/SKILL.md` | Faz 3, adım 16 | Hücreler yazıldıktan sonra `12_matris_denetle.py` hata 0 | Ö-207b |
| `.claude/checklists/document-stage.md` | §3 — geri işaret maddesi; §7 adım 3, adım 6 | Hükümlü çift dosyası; karar kaydı bölümü kapısı; dönem geçişinde `IMPLEMENTATION_STATUS.md` doldurulur | Ö-191, Ö-190, Ö-207a |
| `scripts/doc-scans/karar_bolum_denetle.py` | yeni | Yanlış bölüme yazılmış madde adayı; `1504 24` → 0 | Ö-190 |
| `scripts/doc-scans/geri_isaret_tara.py`, `geri_isaret_hukum.txt` | hüküm dosyası | Hükümlü çift yeniden aday sayılmaz; `1504 1572` → missing 0, judged 8 | Ö-191 |
| `scripts/doc-scans/12_matris_denetle.py` | `--severity`, §1.2 | Severity dağılımı ve §1.2 sayım tablosu karşılaştırması | Ö-195, Ö-196 |
| `scripts/doc-scans/12_devir_esle.py`, `12_kapsam_hizasi.py`, `12_acilis_tarama.py` | çıktı yazımı | `write_text(..., newline="\n")` | Ö-199 |
| `Docs/12_VALIDATION_PROTOCOL.md` | §1.2 `Kanıt ortamı` | `yok` 1.240 · `mock` 201 (v0.33) | Ö-195 |

Madde sayısı yerinde: `grep -c '^- \[ \]' .claude/checklists/document-stage.md` → 67 (başlangıç 67). **Hook (genel) değişmedi:** P-1, P-2, P-3 bilgi olarak §5.2'dedir. **L5 (CI) elendi:** `karar_bolum_denetle.py` adım sonu betiğine elle zincirlenir; CI görevi gereksizdir (kapı checklist §7 adım 3'tedir).

Betik sınaması: `karar_bolum_denetle.py 1504 24` → 0 aday (eski tracker e1f8e9a'da 29); `12_matris_denetle.py --kapsam 1-77` → hata 0 (77 / 77); `geri_isaret_tara.py 1504 1572` → missing 0.

---

## 7. Playbook'a geri akış (K-649)

Liste 162 satırdan **168**'e çıktı (PF-163…PF-168). Var olan satırlar tekrar açılmadı.

| Satır | Öğrenim | Hedef |
|---|---|---|
| **PF-163** (yeni) | Doğrulayıcı adı aşamalı; tek mesaj on; ilk ara commit 150 bin; ajan sözü kanıt değil (Ö-186, Ö-187, Ö-189, Ö-192, Ö-198) | `INSTRUCTIONS §9` |
| **PF-164** (yeni) | Karar kaydı bölüm denetimi ve hükümlü geri işaret dosyası (Ö-190, Ö-191) | `scripts/doc-scans/` · checklist §3, §7 |
| **PF-165** (yeni) | Bulgu dosyası önceden; §1.2 ve severity betik karşılaştırması; üst doküman hizası; "açıklanmış fark" talimatı (Ö-194, Ö-195, Ö-196, Ö-197, Ö-204) | `skills/audit` · `skills/checkpoint` · `scripts/doc-scans/` |
| **PF-166** (yeni) | Dokümanı yazan betik LF (Ö-199) | `INSTRUCTIONS §7` · `scripts/doc-scans/` |
| **PF-167** (yeni) | Dönem geçişinde `IMPLEMENTATION_STATUS.md` doldurma; `validate` matris kapısı (Ö-207a, Ö-207b) | checklist §7 adım 6 · `skills/validate` |
| **PF-168** (yeni) | Soru 1: `00 §N.1`'e Aşama 10'un dersleri | `00 §N.1` — `Öneri` |

PF-152 (hook ve talimat şablonu, proje dışı) açık kalır; Ö-200 ve P-1, P-2, P-3 onun devamıdır.

---

## 8. Kayda ve hafızaya yansıma

- **Karar kaydı v0.329:** K-1573 (öğrenim terfisi — yirmi dört kalemin dökümü, on beş terfi, bir soru, üç toplu onay kararı, maliyet ölçümü); §24.1'e öğrenim terfisi maddesi; tracker başlığı v0.329; `12` v0.33 (§1.2 sayım düzeltmesi, tracker §1 hücresi).
- **`.claude/memory/MEMORY.md` Güncel Durum:** öğrenim terfisi tamam; sırada arşiv işareti (adım 6) — ⚠ listesi ve öneriler proje sahibine gösterildikten sonra.
- **⚠ eşitlemesi:** konu hücresi ⚠ taşıyan Aşama 10 satırı 8 (K-1547, K-1557, K-1560, K-1563, K-1566, K-1568, K-1572 ve anma kaydı K-1504); `onerilen-uyarili.md` girişi 8; K-1573'ün konu hücresi ⚠ taşımaz; **8 / 8, eksik 0, fazla 0.** ⚠ listesi arşiv işaretinden (adım 6) önce proje sahibine tek listede gösterilir.

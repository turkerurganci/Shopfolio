<!--
PR kurallari (00 §D.4):
- PR tek bir amaca hizmet eder. Karisik degisiklik yasak.
- Baska bir task'in PR'ina commit gomulmez (bundled-PR yasagi).
- CI pipeline gecmeden merge yasak.
- Bagimsiz validator PASS olmadan merge yasak.
- Merge'u validator chat yapar; yapim chat'i PR'i acik birakir.
-->

## Task / Konu
<!-- Hangi task veya konu? Orn: T11 — CI/CD pipeline · chore: hook guncellemesi -->

## Ne Yapildi
<!-- Kisaca -->

## Neden Yapildi
<!-- Hangi gereksinimden / dokumandan / is kuralindan kaynaklaniyor -->

## Etkilenen Modul / Dosyalar

## Dokuman Referanslari
<!-- Hangi 11_IMPLEMENTATION_PLAN task'i, hangi 02–10 dokuman bolumu? -->

## Dis Varsayimlar
<!-- Plan tier / paket surumu / platform / API sozlesmesi — her biri icin KANIT.
     "Bu task'ta dis varsayim yok" gecerli bir cevaptir, ama acikca yazilir. -->

## Test
- [ ] Unit testler eklendi/guncellendi (varsa)
- [ ] Integration testler eklendi/guncellendi (varsa)
- [ ] Sema degisikligi var → migration cikti review edildi (varsa)
- [ ] Kritik hesaplama var → ilgili testler gecer (varsa)

## Mini Guvenlik Kontrolu (00 §D.6)
- [ ] Secret sizintisi yok (API key, private key, baglanti dizesi kodda degil)
- [ ] Auth/authorization etkisi degerlendirildi
- [ ] Input validation etkisi degerlendirildi
- [ ] Yeni dis bagimlilik eklendiyse guvenilirligi degerlendirildi

## Disiplin Kapilari
- [ ] Dal izolasyon check temiz (`git log main..HEAD` yalniz bu task'in TXX'i)
- [ ] CI run **tamamlandi** ve `success`
- [ ] Repo memory'de TXX satiri var
- [ ] Rapor + status commit'lendi (merge ONCESI)

## Validator Durumu
- [ ] Validator PASS
- Rapor: `Docs/TASK_REPORTS/TXX_REPORT.md`

## Ertelenen / Follow-up
<!-- Ertelenen her kalem Docs/DEFERRED_BACKLOG.md'ye YAZILIR.
     Yazilmayan is ertelenmis sayilmaz. -->

## Notlar

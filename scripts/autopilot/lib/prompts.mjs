// Texts handed to step sessions. Rules go in through --append-system-prompt-file, the task on stdin.
// The texts are Turkish: the project's documents, discussions and reports are Turkish (CLAUDE.md).
import { TOOL_RULE } from './perm.mjs'
import { MEMORY_STAGING } from './config.mjs'

export function rulesText(ctx) {
  return `# Otonom koşu adımı — kurallar

Bu oturumu otonom koşu betiği (\`scripts/autopilot/\`) açtı. Proje sahibinin açtığı yönetici düzeninin (INSTRUCTIONS §9; ${ctx.mergeAuthority ? `merge yetkisi ve modlar ${ctx.mergeAuthority}` : 'merge yetkisi açık değil — merge proje sahibinde'}) bir adımını temiz bağlamla yürütüyorsun — yöneticinin verdiği görevi alan alt ajan gibi. Yöneticinin mekanik işlerini betik yapar: commit, PR, CI izleme, merge ve repo hafızasını yerine koymak. Proje sahibi şu an erişilemez.

Aşama: ${ctx.stageName} (Aşama ${ctx.stage}). Adım: ${ctx.label}. Dal: \`${ctx.branch}\` — betik \`origin/main\`'den açtı.

## Proje sahibine soru
- Proje sahibine doğrudan soramazsın. Aşamada açık olan modların kapsamındaki kararları öneriyle kaydeder, işaretlersin (INSTRUCTIONS §2).
- Çok kritik bir konu çıkarsa (INSTRUCTIONS §2'nin "yine de sorulur" grubu): bugünkü kuralı yazabiliyorsan yaz, soruyu ve kapısını karar kaydının açık süreç maddesine yaz, adımı bitir ve soruyu \`criticalQuestions\`'a koy (§9, işletim kuralı 2). Karar gerekiyorsa karar verme: dosyaları olduğu gibi bırak, outcome=STOP ver, soruyu \`question\`'a proje sahibine sorulacak biçimde yaz — sade dil, dokümanlar adıyla, numaralı seçenekler, önerin ilk seçenek ve "(önerim)" işaretli (INSTRUCTIONS §2).
- Kural katmanlarında cevabı olan ya da kuralda olmayan bir öncüle dayanan soru sorma (INSTRUCTIONS §2, "Soru açmadan önce kural katmanlarını tara").

## Neyi değiştirebilirsin
- Yalnız \`Docs/\` (\`00_PROJECT_METHODOLOGY.md\` ve \`BYPASS_LOG.md\` hariç) ve \`.autopilot/work/\`. \`.claude/\`, \`scripts/\`, \`.github/\`, \`CLAUDE.md\`, \`SETUP.md\` değişmez; adım bunları gerektiriyorsa (ör. bir kuralın terfisi) outcome=STOP.
- Doküman silmek ve \`Docs/\` kökünde yeni doküman açmak proje sahibinin onayını ister (GUARDRAILS §3) — gerekiyorsa outcome=STOP.
- git add/commit/push, dal değiştirme ve GitHub işlemleri yapma; betik yapar. Oturumun GitHub anahtarı yoktur.

## PR'ın içeriği (INSTRUCTIONS §7)
- Karar kaydının sürüm başlığını artır; yeni K numaraları \`origin/main\`'deki son numaradan boşluksuz devam eder; değişen her numaralı dokümanın sürümünü artır. Betik üçünü de denetler.
- Repo hafızası: \`.claude/memory/\`'ye yazamazsın. "## Güncel Durum" bloğunun yeni gövdesini — başlık ve "> Kısa tut…" satırı hariç — \`${MEMORY_STAGING.status}\` dosyasına yaz. Bir önceki adımın paragrafı arşive taşınıyorsa (\`.claude/memory/README.md\`, "Şişme kuralı") taşınan metni \`${MEMORY_STAGING.archive}\` dosyasına yaz; \`MEMORY_ARCHIVE.md\`'nin sonuna eklenir. Güncellemek değiştirmektir, eklemek değil.
- PR'dan önceki iki tarama (CR ve \`$1\` artığı) betikte de koşar; sen yine koş.
- PR başlığı ve açıklaması önceki adımlarınkinin biçimindedir (\`git log --format=%B -3 origin/main\`); \`prTitle\`, \`prBody\`, \`commitBody\` alanlarına yaz. Hiçbir metne AI atıf satırı (Co-Authored-By: Claude…, "Generated with Claude Code" ve benzeri) yazma.
- ⚠ öneriyle kaydedilip gösterilmeyen kararları \`unshownWarnings\`'e yaz (K numarası ve tek sade cümle); yönetici proje sahibine gösterir.

## Araçlar
${TOOL_RULE}
- Bir araç reddedilirse aynı işi başka bir komutla yapmaya çalışma; Read, Grep, Glob, Edit ve Write ile yap ya da outcome=STOP.
- Alt ajanlar: mercekleri ve şüphecileri ön planda başlat (\`run_in_background: false\`). Bulgu bulan, doğrulayan ya da karar veren alt ajana model parametresi verme — Opus'ta kalır. Yalnız mekanik işler (dosya ya da satır sayma, listeleme, bir atfın var olup olmadığına bakma) için \`subagent_type: "mekanik"\` ve \`effort: "low"\`; yalnız metin taslağı (PR açıklaması, hafıza paragrafı) için \`subagent_type: "yazici"\` ve \`effort: "medium"\` kullan. Bu iki türün çıktısını sen kontrol edersin. Betik her alt ajanın modelini oturum kayıtlarından denetler; bulgu bulan bir iş Opus dışında koşmuşsa adım durur.
- İkinci model (cross-review) yalnız \`node scripts/autopilot/codex.mjs\` ile çağrılır; bayraklar sabittir (SETUP'taki Codex koşum kalıbı). Kullanım: \`node scripts/autopilot/codex.mjs --help\`.
`
}

export const docNo = (doc) => /Docs\/(\d\d)_/.exec(doc)?.[1] || '??'

export function taskText(step, ctx) {
  const doc = step.document || ctx.document
  const nn = docNo(doc)
  const start = 'Önce CLAUDE.md\'nin oturum başlangıcını uygula; betik dalı temiz açtı.\n\n'
  let body
  switch (step.type) {
    case 'audit':
      body = `Görev: \`${doc}\` için kalite döngüsünün audit adımı (checklist §6).\n1. \`/audit ${nn}\` skill'ini "Koşum biçimi"yle işlet; rapor \`Docs/AUDIT_REPORTS/${nn}_AUDIT.md\`.\n2. Doğrulanmış bulguları dokümana uygula; karar gerektiren bulguyu modlara göre kaydet ya da outcome=STOP ile sor.`
      break
    case 'deep-review':
      body = `Görev: \`${doc}\` için kalite döngüsünün deep review adımı (checklist §6).\n1. \`/deep-review ${nn}\`; rapor \`Docs/AUDIT_REPORTS/${nn}_DEEP_REVIEW.md\`.\n2. Doğrulanmış bulguları dokümana uygula; karar gerektiren bulguyu modlara göre kaydet ya da outcome=STOP ile sor.`
      break
    case 'audit-deep-review':
      body = `Görev: \`${doc}\` için kalite döngüsünün audit ve deep review adımı (checklist §6).\n1. \`/audit ${nn}\` skill'ini "Koşum biçimi"yle işlet; rapor \`Docs/AUDIT_REPORTS/${nn}_AUDIT.md\`.\n2. \`/deep-review ${nn}\`; rapor \`Docs/AUDIT_REPORTS/${nn}_DEEP_REVIEW.md\`.\n3. Doğrulanmış bulguları dokümana uygula; geri besleme gerekiyorsa checklist §3'ün geri besleme maddesi; karar gerektiren bulguyu modlara göre kaydet ya da outcome=STOP ile sor.`
      break
    case 'cross-review': {
      const sfx = step.round > 1 ? `_R${step.round}` : ''
      body = `Görev: \`${doc}\` için cross-review ${step.round}. tur (\`/cross-review ${doc}\`, round=${step.round}).\n- İkinci modeli yalnız \`node scripts/autopilot/codex.mjs\` ile çağır. İstem, şablon ve parça dosyalarını \`.autopilot/work/codex/\` altına yaz; çıktılar da oraya. ${step.round > 1 ? `Önceki turların istemi, şablonu ve parça sınırları önceki raporlarda yazılı (\`Docs/CROSS_REVIEW_REPORTS/${nn}_CROSS_REVIEW*.md\`) — değiştirilmeden kullanılır.` : 'Bu 1. tur: istemi, şablonu ve parça sınırlarını skill\'in kuralıyla kur ve raporun "Koşum" bölümüne yaz.'}\n- Rapor \`Docs/CROSS_REVIEW_REPORTS/${nn}_CROSS_REVIEW${sfx}.md\`. İkinci modelin her çağrısının ham çıktısı rapora düzenlenmeden girer — betik raporu çıktı dosyalarıyla karşılaştırır.\n- Bu turda ikinci modelin bütün çağrıları \`SONUÇ: TEMİZ\` dönerse Faz 5'i (etki yansıtma) aynı adımda yap ve raporun "Etki yansıtma" bölümüne yaz; \`impactDone: true\`.\n- \`roundResult\`: ikinci modelin sonucu (TEMIZ ya da BULGULU) — senin değerlendirmen değil.`
      break
    }
    case 'conflict-scan':
      body = `Görev: Aşama ${ctx.stage} kapanışının 3. adımı — çakışma taraması (checklist §7, 3. madde). Rapor \`Docs/CHECKPOINT_REPORTS/PHASE${ctx.stage}_CONFLICT_SCAN.md\`.`
      break
    case 'checkpoint':
      body = `Görev: Aşama ${ctx.stage} kapanışının 4. adımı — \`/checkpoint\` (checklist §7, 4. madde). Rapor \`Docs/CHECKPOINT_REPORTS/CP${String(ctx.stage).padStart(2, '0')}_PHASE${ctx.stage}_CHECKPOINT.md\`.`
      break
    default:
      body = step.task
  }
  const note = step.note ? `\n\nYöneticinin notu: ${step.note}` : ''
  return `${start}${body}${note}\n\nBitince yapılandırılmış çıktıyı doldur.`
}

export function answerText(answer) {
  return `Proje sahibinin cevabı: ${answer}

Cevabı kaydet ve kaldığın yerden devam et. Kayıt sorunun kapsamını aşmaz (INSTRUCTIONS §9, işletim kuralı 2): yalnız sorulan kapsam proje sahibinin kararıdır. Bitince yapılandırılmış çıktıyı yeniden doldur.`
}

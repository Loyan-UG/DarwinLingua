const fs = require('fs');
const path = require('path');
const cp = require('child_process');

const root = 'D:/_Projects/DarwinLingua';
const level = 'C1';
const levelLower = 'c1';
const batch = '065';
const sourcePath = path.join(root, 'content', `${level}.txt`);
const taxonomyPath = path.join(root, 'content', 'taxonomy', 'darwinlingua-taxonomy-v1.json');
const outDir = path.join(root, 'content', 'generated');
const outPath = path.join(outDir, `de-${levelLower}-generated-batch-${batch}.json`);
const importProject = path.join(root, 'src', 'Apps', 'DarwinLingua.ImportTool', 'DarwinLingua.ImportTool.csproj');
const langs = ['ar','ckb','en','fa','kmr','pl','ro','ru','sq','tr'];
const expected = ['die Kapitalrendite','die Kategorisierung','kausal','die Kausalbeziehung','die Kausalerklärung','die Kausalität'];

const taxonomy = JSON.parse(fs.readFileSync(taxonomyPath, 'utf8'));
const source = fs.readFileSync(sourcePath, 'utf8');
const tokens = source.split(',').map(s => s.trim()).filter(Boolean);
const words = tokens.slice(0, 6);
if (JSON.stringify(words) !== JSON.stringify(expected)) {
  throw new Error(`Unexpected first words: ${JSON.stringify(words)}`);
}

const labelKeys = ['business','analysis','written','advanced','formal','academic'];
const labelMap = new Map((taxonomy.labels || []).map(l => [l.key, l]));
const labels = labelKeys.map(k => {
  const l = labelMap.get(k);
  if (!l) throw new Error(`Missing label ${k}`);
  return l;
});

function meanings(ar, ckb, en, fa, kmr, pl, ro, ru, sq, tr) {
  return [ar, ckb, en, fa, kmr, pl, ro, ru, sq, tr].map((text, i) => ({ language: langs[i], text }));
}
function translations(ar, ckb, en, fa, kmr, pl, ro, ru, sq, tr) { return meanings(ar, ckb, en, fa, kmr, pl, ro, ru, sq, tr); }
function ex(baseText, tr) { return { baseText, translations: tr }; }

const entries = [
  {
    word: 'die Kapitalrendite', language: 'de', cefrLevel: level, partOfSpeech: 'Noun', article: 'die', plural: 'Kapitalrenditen', infinitive: null, pronunciationIpa: null, syllableBreak: 'Ka-pi-tal-ren-di-te',
    topics: ['finance-and-accounting','data-and-reporting','advanced-analysis'], usageLabels: ['business','analysis','written','advanced'], contextLabels: [], grammarNotes: ['feminine noun'],
    collocations: [{ text: 'eine stabile Kapitalrendite erzielen', meaning: 'to achieve a stable return on capital' }],
    wordFamilies: [{ lemma: 'das Kapital', relationLabel: 'base noun', note: null }, { lemma: 'die Rendite', relationLabel: 'related noun', note: null }], relations: [],
    meanings: meanings('العائد على رأس المال؛ مردودية رأس المال','گەڕانەوەی سەرمایە؛ قازانجی سەرمایە','return on capital; capital yield','بازده سرمایه؛ سودآوری سرمایه','vegera sermayê; qazanca sermayê','rentowność kapitału; zwrot z kapitału','randament al capitalului; rentabilitatea capitalului','рентабельность капитала; доходность капитала','kthim mbi kapitalin; rendiment i kapitalit','sermaye getirisi; sermaye karlılığı'),
    examples: [
      ex('Die Kapitalrendite sank, obwohl der Umsatz deutlich gestiegen ist.', translations('انخفض العائد على رأس المال رغم أن الإيرادات ارتفعت بشكل واضح.','گەڕانەوەی سەرمایە دابەزی، هەرچەندە داهات بە ڕوونی زیادی کرد.','The return on capital fell even though revenue increased significantly.','بازده سرمایه کاهش یافت، هرچند درآمد به‌طور قابل توجهی افزایش پیدا کرده است.','Vegerê sermayê kêm bû, her çend dahat bi awayekî berbiçav zêde bû.','Rentowność kapitału spadła, chociaż przychody wyraźnie wzrosły.','Randamentul capitalului a scăzut, deși veniturile au crescut semnificativ.','Рентабельность капитала снизилась, хотя выручка заметно выросла.','Kthimi mbi kapitalin ra, megjithëse të ardhurat u rritën ndjeshëm.','Gelir belirgin şekilde artmış olsa da sermaye getirisi düştü.')),
      ex('Für die Investoren zählt nicht nur Wachstum, sondern auch eine stabile Kapitalrendite.', translations('بالنسبة إلى المستثمرين لا يهم النمو فقط، بل أيضاً عائد مستقر على رأس المال.','بۆ وەبەرهێنەران تەنها گەشە گرنگ نییە، بەڵکو گەڕانەوەیەکی جێگیری سەرمایەش گرنگە.','For investors, not only growth matters, but also a stable return on capital.','برای سرمایه‌گذاران فقط رشد مهم نیست، بلکه بازده پایدار سرمایه هم اهمیت دارد.','Ji bo veberhêneran ne tenê mezinbûn girîng e, lê vegera sermayê ya biîstîqrar jî girîng e.','Dla inwestorów liczy się nie tylko wzrost, lecz także stabilna rentowność kapitału.','Pentru investitori contează nu doar creșterea, ci și un randament stabil al capitalului.','Для инвесторов важен не только рост, но и стабильная рентабельность капитала.','Për investitorët nuk ka rëndësi vetëm rritja, por edhe një kthim i qëndrueshëm mbi kapitalin.','Yatırımcılar için sadece büyüme değil, istikrarlı sermaye getirisi de önemlidir.'))
    ]
  },
  {
    word: 'die Kategorisierung', language: 'de', cefrLevel: level, partOfSpeech: 'Noun', article: 'die', plural: 'Kategorisierungen', infinitive: null, pronunciationIpa: null, syllableBreak: 'Ka-te-go-ri-sie-rung',
    topics: ['advanced-analysis','data-and-reporting','documents-and-administration'], usageLabels: ['formal','analysis','written','advanced'], contextLabels: [], grammarNotes: ['feminine noun'],
    collocations: [{ text: 'eine Kategorisierung vornehmen', meaning: 'to carry out a categorization' }],
    wordFamilies: [{ lemma: 'kategorisieren', relationLabel: 'verb', note: null }, { lemma: 'die Kategorie', relationLabel: 'base noun', note: null }], relations: [],
    meanings: meanings('تصنيف؛ تبويب','پۆلێنکردن؛ دابەشکردن بۆ هاوپۆل','categorization; classification','دسته‌بندی؛ طبقه‌بندی','kategorîkirin; polkirin','kategoryzacja; klasyfikacja','categorisire; clasificare','категоризация; классификация','kategorizim; klasifikim','kategorilendirme; sınıflandırma'),
    examples: [
      ex('Die Kategorisierung der Supportfälle hilft, wiederkehrende Fehler schneller zu erkennen.', translations('تساعد تصنيفات حالات الدعم في اكتشاف الأخطاء المتكررة بسرعة أكبر.','پۆلێنکردنی کەیسەکانی پشتگیری یارمەتی دەدات هەڵە دووبارەکان خێراتر بناسرێن.','Categorizing support cases helps identify recurring errors more quickly.','دسته‌بندی درخواست‌های پشتیبانی کمک می‌کند خطاهای تکراری سریع‌تر شناسایی شوند.','Kategorîkirina rewşên piştgiriyê alîkar e ku şaşiyên dubare zûtir bêne nasîn.','Kategoryzacja zgłoszeń do wsparcia pomaga szybciej rozpoznawać powtarzające się błędy.','Categorisirea cazurilor de suport ajută la identificarea mai rapidă a erorilor recurente.','Категоризация обращений в поддержку помогает быстрее выявлять повторяющиеся ошибки.','Kategorizimi i rasteve të suportit ndihmon të njihen më shpejt gabimet që përsëriten.','Destek vakalarının kategorilendirilmesi, tekrarlayan hataların daha hızlı fark edilmesine yardımcı olur.')),
      ex('In der Studie wurde die Kategorisierung der Antworten vorab transparent dokumentiert.', translations('في الدراسة وُثّق تصنيف الإجابات مسبقاً بطريقة شفافة.','لە توێژینەوەکەدا پۆلێنکردنی وەڵامەکان پێشتر بە شێوەی ڕوون تۆمار کرابوو.','In the study, the categorization of the responses was documented transparently in advance.','در این مطالعه، دسته‌بندی پاسخ‌ها از قبل به‌صورت شفاف مستند شده بود.','Di lêkolînê de kategorîkirina bersivan ji berê ve bi zelalî hatibû belgekirin.','W badaniu kategoryzację odpowiedzi udokumentowano wcześniej w przejrzysty sposób.','În studiu, categorisirea răspunsurilor a fost documentată transparent în prealabil.','В исследовании категоризация ответов была заранее прозрачно задокументирована.','Në studim, kategorizimi i përgjigjeve u dokumentua paraprakisht në mënyrë transparente.','Çalışmada cevapların kategorilendirilmesi önceden şeffaf biçimde belgelenmişti.'))
    ]
  },
  {
    word: 'kausal', language: 'de', cefrLevel: level, partOfSpeech: 'Adjective', article: null, plural: null, infinitive: null, pronunciationIpa: null, syllableBreak: 'kau-sal',
    topics: ['advanced-analysis','data-and-reporting','education-and-training'], usageLabels: ['academic','analysis','written','advanced'], contextLabels: [], grammarNotes: ['adjective; often used with Zusammenhang, Wirkung, Erklärung'],
    collocations: [{ text: 'ein kausaler Zusammenhang', meaning: 'a causal connection' }],
    wordFamilies: [{ lemma: 'die Kausalität', relationLabel: 'noun', note: null }], relations: [],
    meanings: meanings('سببي؛ متعلق بالسبب والنتيجة','هۆکاری؛ پەیوەندیدار بە هۆ و ئەنجام','causal; cause-and-effect','علّی؛ مربوط به علت و معلول','sedemî; girêdayî sedem û encamê','przyczynowy; związany z przyczyną i skutkiem','cauzal; legat de cauză și efect','каузальный; причинно-следственный','shkakor; i lidhur me shkakun dhe pasojën','nedensel; sebep-sonuçla ilgili'),
    examples: [
      ex('Aus der Korrelation allein lässt sich kein kausaler Zusammenhang ableiten.', translations('لا يمكن استنتاج علاقة سببية من الارتباط وحده.','تەنها لە پەیوەندی ئامارییەوە ناتوانرێت پەیوەندییەکی هۆکاری دەربهێنرێت.','A causal connection cannot be inferred from correlation alone.','از همبستگی به‌تنهایی نمی‌توان یک رابطه علّی نتیجه گرفت.','Tenê ji têkiliya amaran nayê derxistin ku girêdanek sedemî heye.','Z samej korelacji nie można wyprowadzić związku przyczynowego.','Din simpla corelație nu se poate deduce o legătură cauzală.','Из одной лишь корреляции нельзя вывести причинно-следственную связь.','Vetëm nga korrelacioni nuk mund të nxirret një lidhje shkakore.','Yalnızca korelasyondan nedensel bir ilişki çıkarılamaz.')),
      ex('Die Analyse fragt, ob die Maßnahme kausal zur höheren Kundenzufriedenheit beigetragen hat.', translations('يتساءل التحليل عما إذا كان الإجراء قد ساهم سببياً في زيادة رضا العملاء.','شیکردنەوەکە دەپرسی ئایا ئەو ڕێکارە بە شێوەی هۆکاری بە زیادبوونی ڕەزامەندی کڕیاراندا بەشداری کردووە.','The analysis asks whether the measure causally contributed to higher customer satisfaction.','این تحلیل بررسی می‌کند که آیا آن اقدام به‌طور علّی به رضایت بیشتر مشتریان کمک کرده است یا نه.','Analîz dipirse ka ew tedbîr bi awayekî sedemî beşdarî razîbûna zêdetir a xerîdaran bûye an na.','Analiza sprawdza, czy działanie przyczyniło się przyczynowo do wyższego zadowolenia klientów.','Analiza întreabă dacă măsura a contribuit cauzal la creșterea satisfacției clienților.','Анализ выясняет, способствовала ли мера причинно повышению удовлетворенности клиентов.','Analiza pyet nëse masa ka ndikuar në mënyrë shkakore në rritjen e kënaqësisë së klientëve.','Analiz, önlemin daha yüksek müşteri memnuniyetine nedensel olarak katkıda bulunup bulunmadığını sorgular.'))
    ]
  },
  {
    word: 'die Kausalbeziehung', language: 'de', cefrLevel: level, partOfSpeech: 'Noun', article: 'die', plural: 'Kausalbeziehungen', infinitive: null, pronunciationIpa: null, syllableBreak: 'Kau-sal-be-zie-hung',
    topics: ['advanced-analysis','data-and-reporting','quality-and-risk'], usageLabels: ['academic','analysis','written','advanced'], contextLabels: [], grammarNotes: ['feminine compound noun'],
    collocations: [{ text: 'eine Kausalbeziehung nachweisen', meaning: 'to prove a causal relationship' }],
    wordFamilies: [{ lemma: 'kausal', relationLabel: 'adjective', note: null }, { lemma: 'die Beziehung', relationLabel: 'base noun', note: null }], relations: [],
    meanings: meanings('علاقة سببية','پەیوەندیی هۆکاری','causal relationship','رابطه علّی','têkiliya sedemî','relacja przyczynowa','relație cauzală','причинно-следственная связь','marrëdhënie shkakore','nedensel ilişki'),
    examples: [
      ex('Die Kausalbeziehung zwischen Lieferverzug und Kündigung lässt sich nur mit zusätzlichen Daten prüfen.', translations('لا يمكن فحص العلاقة السببية بين تأخر التسليم والإلغاء إلا ببيانات إضافية.','پەیوەندیی هۆکاری نێوان دواکەوتنی گەیاندن و هەڵوەشاندنەوە تەنها بە داتای زیادە دەتوانرێت بپشکنرێت.','The causal relationship between delivery delay and cancellation can only be examined with additional data.','رابطه علّی میان تأخیر در تحویل و لغو سفارش فقط با داده‌های بیشتر قابل بررسی است.','Têkiliya sedemî di navbera derengiya radestkirinê û betal kirinê de tenê bi daneyên zêde dikare were vekolîn.','Relację przyczynową między opóźnieniem dostawy a wypowiedzeniem można zbadać tylko z dodatkowymi danymi.','Relația cauzală dintre întârzierea livrării și anulare poate fi verificată doar cu date suplimentare.','Причинно-следственную связь между задержкой поставки и отменой можно проверить только с дополнительными данными.','Marrëdhënia shkakore midis vonesës së dorëzimit dhe anulimit mund të shqyrtohet vetëm me të dhëna shtesë.','Teslimat gecikmesi ile iptal arasındaki nedensel ilişki ancak ek verilerle incelenebilir.')),
      ex('Im Modell werden mehrere mögliche Kausalbeziehungen ausdrücklich voneinander getrennt.', translations('في النموذج تُفصل عدة علاقات سببية محتملة بوضوح بعضها عن بعض.','لە مۆدێلەکەدا چەند پەیوەندییەکی هۆکاریی ئەگەری بە ڕوونی لە یەکدی جیا دەکرێنەوە.','In the model, several possible causal relationships are explicitly separated from one another.','در این مدل، چند رابطه علّی احتمالی به‌صراحت از هم جدا شده‌اند.','Di modelê de gelek têkiliyên sedemî yên mimkun bi eşkere ji hev tên veqetandin.','W modelu kilka możliwych relacji przyczynowych jest wyraźnie od siebie oddzielonych.','În model, mai multe relații cauzale posibile sunt separate explicit una de alta.','В модели несколько возможных причинно-следственных связей явно отделены друг от друга.','Në model, disa marrëdhënie të mundshme shkakore ndahen qartë nga njëra-tjetra.','Modelde birkaç olası nedensel ilişki açıkça birbirinden ayrılır.'))
    ]
  },
  {
    word: 'die Kausalerklärung', language: 'de', cefrLevel: level, partOfSpeech: 'Noun', article: 'die', plural: 'Kausalerklärungen', infinitive: null, pronunciationIpa: null, syllableBreak: 'Kau-sal-er-klä-rung',
    topics: ['advanced-analysis','education-and-training','data-and-reporting'], usageLabels: ['academic','analysis','written','advanced'], contextLabels: [], grammarNotes: ['feminine compound noun'],
    collocations: [{ text: 'eine Kausalerklärung liefern', meaning: 'to provide a causal explanation' }],
    wordFamilies: [{ lemma: 'kausal', relationLabel: 'adjective', note: null }, { lemma: 'erklären', relationLabel: 'verb', note: null }], relations: [],
    meanings: meanings('تفسير سببي','ڕوونکردنەوەی هۆکاری','causal explanation','توضیح علّی','şîroveya sedemî','wyjaśnienie przyczynowe','explicație cauzală','причинное объяснение','shpjegim shkakor','nedensel açıklama'),
    examples: [
      ex('Eine überzeugende Kausalerklärung muss alternative Ursachen berücksichtigen.', translations('يجب أن يأخذ التفسير السببي المقنع الأسباب البديلة في الاعتبار.','ڕوونکردنەوەیەکی هۆکاریی قانعکەر دەبێت هۆکارە جێگرەوەکان لەبەرچاو بگرێت.','A convincing causal explanation must take alternative causes into account.','یک توضیح علّی قانع‌کننده باید علت‌های جایگزین را هم در نظر بگیرد.','Şîroveya sedemî ya bawerker divê sedemên alternatîf jî li ber çavan bigire.','Przekonujące wyjaśnienie przyczynowe musi uwzględniać alternatywne przyczyny.','O explicație cauzală convingătoare trebuie să ia în considerare cauzele alternative.','Убедительное причинное объяснение должно учитывать альтернативные причины.','Një shpjegim shkakor bindës duhet të marrë parasysh shkaqet alternative.','İkna edici bir nedensel açıklama alternatif nedenleri de dikkate almalıdır.')),
      ex('Der Bericht liefert keine Kausalerklärung, sondern beschreibt nur zeitliche Abläufe.', translations('لا يقدم التقرير تفسيراً سببياً، بل يصف فقط التسلسل الزمني للأحداث.','ڕاپۆرتەکە ڕوونکردنەوەی هۆکاری پێشکەش ناکات، بەڵکو تەنها ڕەوتە کاتییەکان وەسف دەکات.','The report provides no causal explanation; it only describes chronological sequences.','گزارش هیچ توضیح علّی ارائه نمی‌دهد، بلکه فقط روندهای زمانی را توصیف می‌کند.','Rapor şîroveya sedemî nade, tenê pêvajoyên demî vedibêje.','Raport nie dostarcza wyjaśnienia przyczynowego, lecz opisuje jedynie przebieg w czasie.','Raportul nu oferă o explicație cauzală, ci descrie doar succesiuni temporale.','Отчет не дает причинного объяснения, а лишь описывает временную последовательность событий.','Raporti nuk jep shpjegim shkakor, por përshkruan vetëm rrjedhat kohore.','Rapor nedensel bir açıklama sunmaz, yalnızca zamansal süreçleri açıklar.'))
    ]
  },
  {
    word: 'die Kausalität', language: 'de', cefrLevel: level, partOfSpeech: 'Noun', article: 'die', plural: 'Kausalitäten', infinitive: null, pronunciationIpa: null, syllableBreak: 'Kau-sa-li-tät',
    topics: ['advanced-analysis','data-and-reporting','education-and-training'], usageLabels: ['academic','analysis','written','advanced'], contextLabels: [], grammarNotes: ['feminine noun'],
    collocations: [{ text: 'Kausalität nachweisen', meaning: 'to prove causality' }],
    wordFamilies: [{ lemma: 'kausal', relationLabel: 'adjective', note: null }], relations: [],
    meanings: meanings('السببية؛ العلاقة بين السبب والنتيجة','هۆکارییەت؛ پەیوەندیی هۆ و ئەنجام','causality; cause-and-effect relationship','علیت؛ رابطه علت و معلول','sedemîtî; têkiliya sedem û encamê','przyczynowość; relacja przyczyny i skutku','cauzalitate; relația cauză-efect','каузальность; причинно-следственная связь','shkakësi; marrëdhënie shkak-pasojë','nedensellik; sebep-sonuç ilişkisi'),
    examples: [
      ex('Kausalität lässt sich in dieser Untersuchung nicht sicher nachweisen.', translations('لا يمكن إثبات السببية بشكل مؤكد في هذه الدراسة.','هۆکارییەت لەم لێکۆڵینەوەیەدا بە دڵنیایی ناتوانرێت بسەلمێنرێت.','Causality cannot be reliably proven in this investigation.','در این بررسی نمی‌توان علیت را با قطعیت اثبات کرد.','Di vê vekolînê de sedemîtî bi ewlehî nayê îspat kirin.','W tym badaniu nie da się wiarygodnie wykazać przyczynowości.','În această investigație, cauzalitatea nu poate fi demonstrată cu certitudine.','В этом исследовании нельзя надежно доказать причинность.','Në këtë hetim, shkakësia nuk mund të vërtetohet me siguri.','Bu incelemede nedensellik güvenilir biçimde kanıtlanamaz.')),
      ex('Im Workshop unterscheiden wir Kausalität, Korrelation und bloße zeitliche Nähe.', translations('في ورشة العمل نميز بين السببية والارتباط ومجرد التقارب الزمني.','لە وۆرکشۆپەکەدا جیاوازی دەکەین لە نێوان هۆکارییەت، پەیوەندی ئاماری و نزیکیی کاتیی سادە.','In the workshop, we distinguish causality, correlation, and mere temporal proximity.','در کارگاه، علیت، همبستگی و صرفاً نزدیکی زمانی را از هم تفکیک می‌کنیم.','Di atolyeyê de em sedemîtî, têkiliya amaran û tenê nêzîkahiya demî ji hev cuda dikin.','Na warsztacie odróżniamy przyczynowość, korelację i zwykłą bliskość czasową.','În atelier, diferențiem cauzalitatea, corelația și simpla apropiere temporală.','На семинаре мы различаем причинность, корреляцию и простую временную близость.','Në seminar dallojmë shkakësinë, korrelacionin dhe afërsinë e thjeshtë kohore.','Atölyede nedenselliği, korelasyonu ve yalnızca zamansal yakınlığı birbirinden ayırıyoruz.'))
    ]
  }
];

const pkg = { packageVersion: '1.0', packageId: `de-${levelLower}-generated-batch-${batch}`, packageName: `German ${level} Generated Batch ${batch}`, source: 'Hybrid', defaultMeaningLanguages: langs, labels, entries, collections: [], scenarios: [], conversationStarterPacks: [], eventPreparationPacks: [] };
fs.writeFileSync(outPath, JSON.stringify(pkg, null, 2), 'utf8');

const importCmd = `dotnet run --project "${importProject}" -- --target shared --yes "${outPath}"`;
let output = '';
let exitCode = 0;
try {
  output = cp.execSync(importCmd, { cwd: root, encoding: 'utf8', stdio: ['ignore','pipe','pipe'] });
} catch (e) {
  exitCode = e.status || 1;
  output = `${e.stdout || ''}${e.stderr || ''}`;
}

const imported = /Entries imported:\s*6\b/.test(output);
const invalid0 = /Entries invalid:\s*0\b/.test(output);
const warnings0 = /Warnings:\s*0\b/.test(output);
const success = exitCode === 0 && imported && invalid0 && warnings0;
let deleted = false;
let remainingTokens = tokens;
if (success) {
  const remove = new Set(words);
  let removed = 0;
  remainingTokens = tokens.filter(t => {
    if (remove.has(t) && removed < words.length) { removed++; return false; }
    return true;
  });
  if (removed !== words.length) throw new Error(`Removed ${removed}, expected ${words.length}`);
  fs.writeFileSync(sourcePath, remainingTokens.join(', '), 'utf8');
  deleted = true;
}
const report = {
  sourcePath,
  processedWords: words,
  jsonPath: outPath,
  importExitCode: exitCode,
  importSummary: (output.match(/Entries imported:\s*\d+|Entries invalid:\s*\d+|Warnings:\s*\d+/g) || []).join(' | '),
  deleted,
  remainingCount: remainingTokens.length,
  first10Remaining: remainingTokens.slice(0, 10)
};
console.log(JSON.stringify(report, null, 2));
if (!success) process.exit(2);

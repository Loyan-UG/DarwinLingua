#!/usr/bin/env node

const fs = require("fs");
const path = require("path");

const languages = ["en", "fa", "ar", "tr", "ru", "ckb", "kmr", "pl", "ro", "sq"];

function parseArgs(argv) {
  const args = { package: "", slug: "", write: false };
  for (let index = 2; index < argv.length; index += 1) {
    const arg = argv[index];
    if (arg === "--write") {
      args.write = true;
      continue;
    }

    if (arg === "--package") {
      args.package = argv[index + 1] || "";
      index += 1;
      continue;
    }

    if (arg === "--slug") {
      args.slug = argv[index + 1] || "";
      index += 1;
      continue;
    }
  }

  if (!args.package) {
    throw new Error("Usage: node tools/Content/Repair-GrammarContentQuality.js --package <path> [--slug <slug>] [--write]");
  }

  return args;
}

function normalizeText(value) {
  return typeof value === "string" ? value.trim().replace(/\s+/g, " ") : "";
}

function collectDuplicateTexts(items, threshold, minLength = 40) {
  const counts = new Map();
  for (const item of items) {
    const text = normalizeText(item.text);
    if (text.length < minLength) {
      continue;
    }

    const key = text.toLocaleLowerCase("de");
    counts.set(key, (counts.get(key) || 0) + 1);
  }

  return new Set(
    [...counts.entries()]
      .filter(([, count]) => count > threshold)
      .map(([text]) => text),
  );
}

function topicTitle(topic, language) {
  const localized = topic.titleLocalized && topic.titleLocalized[language];
  return localized || topic.title || topic.slug;
}

function sectionHeading(section, language) {
  const translation = (section.translations || []).find((item) => item.language === language);
  return translation?.heading || section.heading || section.sectionKey;
}

function sectionIntro(language, topic, section, variant) {
  const title = topicTitle(topic, language);
  const heading = sectionHeading(section, language);
  const germanFocus = "German examples";

  const lines = {
    en: [
      `In "${heading}", focus on the sentence pattern that the German examples show. Read the form first, then check how it changes the meaning in "${title}".`,
      `This part of "${title}" is about using the pattern in real sentences, not memorizing a loose translation. Compare the German forms in the examples under "${heading}".`,
    ],
    fa: [
      `در بخش «${heading}»، اول قالب جمله‌های آلمانی را ببین و بعد معنی آن را در «${title}» بررسی کن. هدف حفظ ترجمه جداگانه نیست؛ باید شکل آلمانی را در جمله تشخیص بدهی.`,
      `این بخش از «${title}» روی کاربرد واقعی الگو تمرکز دارد. مثال‌های آلمانیِ «${heading}» را با دقت بخوان و به جایگاه فعل، حالت یا عبارت ثابت توجه کن.`,
    ],
    ar: [
      `في قسم «${heading}»، ابدأ بشكل الجملة الألمانية ثم اربطه بالمعنى داخل «${title}». الهدف ليس ترجمة كلمة بكلمة، بل فهم البنية في المثال.`,
      `يركز هذا الجزء من «${title}» على استعمال النمط في جمل حقيقية. راقب ترتيب الفعل والحالة أو العبارة الثابتة في أمثلة «${heading}».`,
    ],
    tr: [
      `"${heading}" bölümünde önce Almanca cümle kalıbına bak, sonra anlamı "${title}" içinde kontrol et. Amaç kelime kelime çeviri değil, yapıyı cümlede görmektir.`,
      `"${title}" içindeki bu bölüm gerçek cümle kullanımına odaklanır. "${heading}" örneklerinde fiil yeri, durum eki karşılığı veya sabit ifadeye dikkat et.`,
    ],
    ru: [
      `В разделе «${heading}» сначала смотри на немецкую модель предложения, а потом на значение в теме «${title}». Здесь важна структура, а не дословный перевод.`,
      `Эта часть темы «${title}» показывает употребление модели в реальных предложениях. В примерах «${heading}» проверь место глагола, падеж или устойчивое сочетание.`,
    ],
    ckb: [
      `لە بەشی «${heading}»دا سەرەتا شێوازی ڕستەی ئەڵمانی ببینە، پاشان ماناکەی لە «${title}»دا پەیوەست بکە. ئامانج وەرگێڕانی وشە بە وشە نییە.`,
      `ئەم بەشەی «${title}» لەسەر بەکارهێنانی ڕاستەقینەی داڕشتەکەیە. لە نموونەکانی «${heading}» شوێنی کردار، case یان دەستەواژەی جێگیر بپشکنە.`,
    ],
    kmr: [
      `Di beşa "${heading}" de pêşî li qaliba hevoka Almanî binêre, paşê wateya wê di "${title}" de kontrol bike. Armanc wergera peyv bi peyv nîne.`,
      `Ev beşa "${title}" li ser bikaranîna rastîn a qalibê disekine. Di mînakên "${heading}" de cihê lêkerê, rewş an hevoka sabît bibîne.`,
    ],
    pl: [
      `W sekcji „${heading}” najpierw zwróć uwagę na niemiecki wzór zdania, a potem na znaczenie w temacie „${title}”. Ważna jest struktura, nie tłumaczenie słowo w słowo.`,
      `Ta część tematu „${title}” pokazuje użycie wzoru w realnych zdaniach. W przykładach „${heading}” sprawdź miejsce czasownika, przypadek albo stały zwrot.`,
    ],
    ro: [
      `În secțiunea „${heading}”, urmărește mai întâi modelul german al propoziției, apoi sensul lui în „${title}”. Contează structura, nu traducerea cuvânt cu cuvânt.`,
      `Această parte din „${title}” arată folosirea modelului în propoziții reale. În exemplele din „${heading}”, verifică poziția verbului, cazul sau expresia fixă.`,
    ],
    sq: [
      `Në seksionin "${heading}", shiko së pari modelin gjerman të fjalisë dhe pastaj kuptimin në "${title}". E rëndësishme është struktura, jo përkthimi fjalë për fjalë.`,
      `Kjo pjesë e "${title}" tregon përdorimin e modelit në fjali reale. Te shembujt e "${heading}", kontrollo vendin e foljes, rasën ose shprehjen fikse.`,
    ],
  };

  return (lines[language] || lines.en)[variant % 2].replace(germanFocus, "German examples");
}

function sectionRuleItems(language, topic, section) {
  const heading = sectionHeading(section, language);
  const title = topicTitle(topic, language);
  const lines = {
    en: [
      `Use this section for the pattern in "${heading}".`,
      `Check the German example before copying the structure into your own sentence.`,
    ],
    fa: [
      `این بخش را برای الگوی «${heading}» به کار ببر.`,
      `قبل از ساختن جمله خودت، مثال آلمانی همین بخش را با دقت مقایسه کن.`,
    ],
    ar: [
      `استخدم هذا القسم لنمط «${heading}».`,
      `قبل أن تكتب جملتك، قارنها بالمثال الألماني في هذا القسم.`,
    ],
    tr: [
      `"${heading}" kalıbı için bu bölümü kullan.`,
      `Kendi cümleni kurmadan önce Almanca örneği kontrol et.`,
    ],
    ru: [
      `Используй этот раздел для модели «${heading}».`,
      `Перед своей фразой сравни структуру с немецким примером.`,
    ],
    ckb: [
      `ئەم بەشە بۆ داڕشتەی «${heading}» بەکاربهێنە.`,
      `پێش نووسینی ڕستەی خۆت، نموونەی ئەڵمانی ئەم بەشە بەراورد بکە.`,
    ],
    kmr: [
      `Vê beşê ji bo qaliba "${heading}" bi kar bîne.`,
      `Berî hevoka xwe, mînaka Almanî ya vê beşê berhev bike.`,
    ],
    pl: [
      `Użyj tej sekcji do wzoru „${heading}”.`,
      `Przed napisaniem własnego zdania porównaj je z niemieckim przykładem.`,
    ],
    ro: [
      `Folosește această secțiune pentru modelul „${heading}”.`,
      `Înainte să scrii propoziția ta, compar-o cu exemplul german.`,
    ],
    sq: [
      `Përdore këtë seksion për modelin "${heading}".`,
      `Para se të shkruash fjalinë tënde, krahasoje me shembullin gjerman.`,
    ],
  };

  return (lines[language] || lines.en).map((item) => item.replace("${title}", title));
}

function mistakeExplanation(language, wrongText, correctedText) {
  const wrong = wrongText || "the wrong sentence";
  const corrected = correctedText || "the corrected sentence";
  const caseOnly = wrong.toLocaleLowerCase("de") === corrected.toLocaleLowerCase("de") && wrong !== corrected;
  const lowerStart = wrong.length > 0 && wrong[0] === wrong[0].toLocaleLowerCase("de") && wrong[0] !== wrong[0].toLocaleUpperCase("de");
  const caseNote = {
    en: lowerStart
      ? " Also check the lowercase first letter in the wrong example."
      : " Also check the capital letter inside the wrong example.",
    fa: lowerStart
      ? " همچنین به حرف کوچک در ابتدای نمونه نادرست دقت کن."
      : " همچنین به حرف بزرگ یا کوچک داخل نمونه نادرست دقت کن.",
    ar: lowerStart
      ? " وانتبه أيضاً إلى الحرف الصغير في بداية المثال الخاطئ."
      : " وانتبه أيضاً إلى الحرف الكبير أو الصغير داخل المثال الخاطئ.",
    tr: lowerStart
      ? " Yanlış örneğin küçük harfle başlamasına da dikkat et."
      : " Yanlış örnekteki büyük/küçük harfe de dikkat et.",
    ru: lowerStart
      ? " Также обрати внимание на строчную букву в начале неправильного примера."
      : " Также проверь заглавную или строчную букву внутри неправильного примера.",
    ckb: lowerStart
      ? " هەروەها سەرنج بدە بە پیتی بچووک لە سەرەتای نموونەی هەڵەدا."
      : " هەروەها سەرنج بدە بە پیتی گەورە یان بچووک لە ناو نموونەی هەڵەدا.",
    kmr: lowerStart
      ? " Her wiha bala xwe bide tîpa biçûk li destpêka mînaka şaş."
      : " Her wiha tîpa mezin an biçûk di mînaka şaş de kontrol bike.",
    pl: lowerStart
      ? " Zwróć też uwagę na małą literę na początku błędnego przykładu."
      : " Sprawdź też wielką lub małą literę w błędnym przykładzie.",
    ro: lowerStart
      ? " Verifică și litera mică de la începutul exemplului greșit."
      : " Verifică și litera mare sau mică din exemplul greșit.",
    sq: lowerStart
      ? " Kontrollo edhe shkronjën e vogël në fillim të shembullit të gabuar."
      : " Kontrollo edhe shkronjën e madhe ose të vogël brenda shembullit të gabuar.",
  };
  const templates = {
    en: `Use "${corrected}" here. In "${wrong}", the problem is in the German word order, case, verb form, article, or fixed phrase required by this pattern.`,
    fa: `در اینجا شکل درست «${corrected}» است. در «${wrong}» مشکل از ترتیب واژه‌ها، حالت، فعل، article یا عبارت ثابتِ لازم در این الگوی آلمانی می‌آید.`,
    ar: `الصيغة الصحيحة هنا هي «${corrected}». في «${wrong}» يظهر الخطأ في ترتيب الكلمات أو الحالة أو صيغة الفعل أو الأداة أو العبارة الثابتة في الألمانية.`,
    tr: `Burada doğru biçim "${corrected}" olur. "${wrong}" cümlesinde sorun Almanca kelime sırası, hâl, fiil biçimi, artikel veya sabit ifade ile ilgilidir.`,
    ru: `Здесь правильно: «${corrected}». В «${wrong}» ошибка связана с немецким порядком слов, падежом, формой глагола, артиклем или устойчивым сочетанием.`,
    ckb: `لێرەدا شێوەی دروست «${corrected}» ـە. لە «${wrong}»دا هەڵەکە پەیوەندی بە ڕیزی وشە، case، شێوەی کردار، article یان دەستەواژەی جێگیری ئەڵمانییەوە هەیە.`,
    kmr: `Li vir forma rast "${corrected}" e. Di "${wrong}" de pirsgirêk bi rêza peyvan, rewş, forma lêkerê, artikel an hevoka sabît a Almanî ve girêdayî ye.`,
    pl: `Tutaj poprawna forma to „${corrected}”. W „${wrong}” błąd dotyczy niemieckiego szyku, przypadku, formy czasownika, rodzajnika albo stałego zwrotu.`,
    ro: `Aici forma corectă este „${corrected}”. În „${wrong}”, problema ține de ordinea cuvintelor, caz, forma verbului, articol sau expresia fixă cerută în germană.`,
    sq: `Këtu forma e saktë është "${corrected}". Te "${wrong}", problemi lidhet me rendin e fjalëve, rasën, formën e foljes, nyjën ose shprehjen fikse në gjermanisht.`,
  };

  const base = templates[language] || templates.en;
  return caseOnly ? `${base}${caseNote[language] || caseNote.en}` : base;
}

function mistakeWrongText(mistake) {
  return mistake.wrongText || mistake.wrongGerman || "";
}

function mistakeCorrectedText(mistake) {
  return mistake.correctedText || mistake.correctGerman || "";
}

function dedupeCommonMistakes(topic) {
  if (!Array.isArray(topic.commonMistakes)) {
    return 0;
  }

  const seen = new Set();
  const deduped = [];
  let removed = 0;

  for (const mistake of topic.commonMistakes) {
    const key = `${normalizeText(mistakeWrongText(mistake))}\u0000${normalizeText(mistakeCorrectedText(mistake))}`;
    if (key !== "\u0000" && seen.has(key)) {
      removed += 1;
      continue;
    }

    seen.add(key);
    deduped.push(mistake);
  }

  if (removed > 0) {
    topic.commonMistakes = deduped.map((mistake, index) => ({
      ...mistake,
      sortOrder: index + 1,
    }));
  }

  return removed;
}

function repairTopic(topic) {
  const changes = [];
  const removedMistakes = dedupeCommonMistakes(topic);
  if (removedMistakes > 0) {
    changes.push(`${topic.slug}: removed ${removedMistakes} duplicate common-mistake items`);
  }

  for (const language of languages) {
    const translationItems = [];
    const blockItems = [];
    const ruleListItems = [];

    for (const section of topic.sections || []) {
      const translation = (section.translations || []).find((item) => item.language === language);
      if (translation?.text) {
        translationItems.push({ section, translation, text: translation.text });
      }

      for (const block of (section.localizedBlocks && section.localizedBlocks[language]) || []) {
        if ((block.type === "paragraph" || block.type === "callout") && block.text) {
          blockItems.push({ section, block, text: block.text });
        }

        if (block.type === "rule-list" && Array.isArray(block.items)) {
          ruleListItems.push({ section, block, text: block.items.join("\n") });
        }
      }
    }

    const duplicateTranslations = collectDuplicateTexts(translationItems, 2, 20);
    const duplicateBlocks = collectDuplicateTexts(blockItems, 2, 20);
    const duplicateRuleLists = collectDuplicateTexts(ruleListItems, 1, 20);

    for (const [index, item] of translationItems.entries()) {
      if (duplicateTranslations.has(normalizeText(item.text).toLocaleLowerCase("de"))) {
        item.translation.text = sectionIntro(language, topic, item.section, index);
        changes.push(`${topic.slug}: ${language} section translation repaired for ${item.section.sectionKey}`);
      }
    }

    for (const [index, item] of blockItems.entries()) {
      if (duplicateBlocks.has(normalizeText(item.text).toLocaleLowerCase("de"))) {
        item.block.text = sectionIntro(language, topic, item.section, index + 1);
        changes.push(`${topic.slug}: ${language} ${item.block.type} repaired for ${item.section.sectionKey}`);
      }
    }

    for (const item of ruleListItems) {
      if (duplicateRuleLists.has(normalizeText(item.text).toLocaleLowerCase("de"))) {
        item.block.items = sectionRuleItems(language, topic, item.section);
        changes.push(`${topic.slug}: ${language} rule-list repaired for ${item.section.sectionKey}`);
      }
    }
  }

  for (const language of languages) {
    const mistakeItems = [];
    for (const mistake of topic.commonMistakes || []) {
      let text = mistake.explanationLocalized && mistake.explanationLocalized[language];
      if (!text) {
        const translation = (mistake.translations || []).find((item) => item.language === language);
        text = translation?.explanation;
      }

      if (text) {
        mistakeItems.push({ mistake, text });
      }
    }

    const duplicateMistakes = collectDuplicateTexts(mistakeItems, 1, 12);
    for (const item of mistakeItems) {
      if (!duplicateMistakes.has(normalizeText(item.text).toLocaleLowerCase("de"))) {
        continue;
      }

      const text = mistakeExplanation(language, mistakeWrongText(item.mistake), mistakeCorrectedText(item.mistake));
      item.mistake.explanationLocalized = item.mistake.explanationLocalized || {};
      item.mistake.explanationLocalized[language] = text;

      const translation = (item.mistake.translations || []).find((entry) => entry.language === language);
      if (translation) {
        translation.explanation = text;
      }

      if (language === "en") {
        item.mistake.explanation = text;
      }

      changes.push(`${topic.slug}: ${language} common-mistake explanation repaired`);
    }
  }

  return changes;
}

const args = parseArgs(process.argv);
const packagePath = path.resolve(args.package);
const packageJson = JSON.parse(fs.readFileSync(packagePath, "utf8"));
const topics = (packageJson.grammarTopics || []).filter((topic) => !args.slug || topic.slug === args.slug);
if (topics.length === 0) {
  throw new Error(`No grammar topic matched ${args.slug || "(all topics)"}`);
}

const changes = [];
for (const topic of topics) {
  changes.push(...repairTopic(topic));
}

if (args.write && changes.length > 0) {
  fs.writeFileSync(packagePath, `${JSON.stringify(packageJson, null, 2)}\n`, "utf8");
}

console.log(JSON.stringify({
  packagePath,
  slug: args.slug || null,
  topicCount: topics.length,
  changeCount: changes.length,
  wrote: args.write,
  changes: changes.slice(0, 200),
}, null, 2));

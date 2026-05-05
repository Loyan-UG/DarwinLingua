const fs = require('fs');
const path = require('path');
const cp = require('child_process');
const root = 'D:/_Projects/DarwinLingua';
const level = 'C2', levelLower = 'c2', batch = '083';
const sourcePath = path.join(root, 'content', `${level}.txt`);
const taxonomyPath = path.join(root, 'content', 'taxonomy', 'darwinlingua-taxonomy-v1.json');
const outPath = path.join(root, 'content', 'generated', `de-${levelLower}-generated-batch-${batch}.json`);
const langs = ['ar','ckb','en','fa','kmr','pl','ro','ru','sq','tr'];
const expected = ['die Unhintergehbarkeit','unmissverständlich','die Unmittelbarkeit','die Unschärfe','die Unscheinbarkeit','unter Vorbehalt'];
const taxonomy = JSON.parse(fs.readFileSync(taxonomyPath, 'utf8'));
const topicKeys = new Set((taxonomy.topics || []).map(t => t.key));
const labelsByKey = new Map((taxonomy.labels || []).map(l => [l.key, l]));
const source = fs.readFileSync(sourcePath, 'utf8');
const tokens = source.split(',').map(s => s.trim()).filter(Boolean);
if (JSON.stringify(tokens.slice(0, 6)) !== JSON.stringify(expected)) throw new Error(`Unexpected first tokens: ${JSON.stringify(tokens.slice(0, 6))}`);
function m(ar, ckb, en, fa, kmr, pl, ro, ru, sq, tr) { return [ar, ckb, en, fa, kmr, pl, ro, ru, sq, tr].map((text, i) => ({ language: langs[i], text })); }
function ex(baseText, translations) { return { baseText, translations }; }
function entry(e) {
  for (const t of e.topics) if (!topicKeys.has(t)) throw new Error(`Unknown topic ${t}`);
  for (const l of [...(e.usageLabels || []), ...(e.contextLabels || [])]) if (!labelsByKey.has(l)) throw new Error(`Unknown label ${l}`);
  return Object.assign({ language: 'de', cefrLevel: level, article: null, plural: null, infinitive: null, pronunciationIpa: null, syllableBreak: null, contextLabels: [], grammarNotes: [], collocations: [], wordFamilies: [], relations: [] }, e);
}
const entries = [
  entry({
    word: 'die Unhintergehbarkeit', partOfSpeech: 'Noun', article: 'die', plural: null, syllableBreak: 'Un-hin-ter-geh-bar-keit',
    topics: ['advanced-analysis','law-and-compliance','education-and-training'], usageLabels: ['formal','written','academic','analysis'], grammarNotes: ['feminine abstract noun; normally singular'],
    collocations: [{ text: 'die Unhintergehbarkeit einer Voraussetzung', meaning: 'the impossibility of getting behind or bypassing a premise' }],
    meanings: m('استحالة التجاوز؛ عدم إمكان الالتفاف على أساس','پشتگوێنەخستنی بنەڕەتی؛ لەدەرەوە نەچوون','inescapability; impossibility of bypassing a premise','گریزناپذیری بنیادین؛ دورزدنی‌ناپذیری','nekarîna derbasbûnê; neçarîya bingehîn','nieprzekraczalność; niemożność obejścia','imposibilitate de ocolire; caracter inevitabil','неустранимость; невозможность обойти','pashmangshmëri themelore','aşılamazlık; bertaraf edilemezlik'),
    examples: [
      ex('Die Unhintergehbarkeit der Datenschutzanforderungen wurde im Architekturreview ausdrücklich festgehalten.', m('سُجلت صراحةً في مراجعة البنية استحالة تجاوز متطلبات حماية البيانات.','لە پێداچوونەوەی ئەندازیارییەکەدا بە ڕوونی نووسرا کە پێداویستییەکانی پاراستنی داتا پشتگوێ ناکرێن.','The inescapability of the data protection requirements was explicitly recorded in the architecture review.','در بازبینی معماری، گریزناپذیری الزامات حفاظت از داده صراحتاً ثبت شد.','Di reviewa avahiyê de nekarîna derbasbûna daxwazên parastina daneyan bi eşkere hate tomar kirin.','Nieprzekraczalność wymogów ochrony danych została wyraźnie odnotowana w przeglądzie architektury.','Imposibilitatea de a ocoli cerințele de protecție a datelor a fost consemnată explicit în analiza arhitecturii.','Неустранимость требований защиты данных была прямо зафиксирована на архитектурном ревью.','Pashmangshmëria e kërkesave për mbrojtjen e të dhënave u shënua qartë në rishikimin e arkitekturës.','Veri koruma gerekliliklerinin aşılamazlığı mimari incelemede açıkça kayda geçirildi.')),
      ex('Der Aufsatz betont die Unhintergehbarkeit historischer Erfahrung für jede Theorie politischer Verantwortung.', m('تؤكد المقالة عدم إمكان تجاوز الخبرة التاريخية في أي نظرية للمسؤولية السياسية.','وتارەکە جەخت لەوە دەکاتەوە کە ئەزموونی مێژوویی بۆ هەر تیۆرییەکی بەرپرسیاریی سیاسی پشتگوێ ناکرێت.','The essay emphasizes the inescapability of historical experience for any theory of political responsibility.','مقاله بر گریزناپذیری تجربه تاریخی برای هر نظریه مسئولیت سیاسی تأکید می‌کند.','Gotar tekez dike ku ezmûna dîrokî ji bo her teoriyeke berpirsiyariya siyasî nayê derbaskirin.','Artykuł podkreśla nieprzekraczalność doświadczenia historycznego dla każdej teorii odpowiedzialności politycznej.','Eseul subliniază imposibilitatea de a ocoli experiența istorică pentru orice teorie a responsabilității politice.','Статья подчеркивает неустранимость исторического опыта для любой теории политической ответственности.','Eseja thekson pashmangshmërinë e përvojës historike për çdo teori të përgjegjësisë politike.','Makale, siyasi sorumluluk kuramı için tarihsel deneyimin bertaraf edilemezliğini vurgular.'))
    ]
  }),
  entry({
    word: 'unmissverständlich', partOfSpeech: 'Adjective', syllableBreak: 'un-miss-ver-ständ-lich',
    topics: ['business-communication','law-and-compliance','customer-service'], usageLabels: ['formal','written','business','administrative'],
    collocations: [{ text: 'unmissverständlich klarstellen', meaning: 'to make unmistakably clear' }],
    meanings: m('واضح لا لبس فيه','بێگومان ڕوون؛ تێکەڵنەکراو','unmistakable; unequivocal','کاملاً روشن؛ بی‌ابهام','bêguman zelal; bêduwate','jednoznaczny; niedwuznaczny','neechivoc; clar','недвусмысленный','i qartë pa keqkuptim','açık ve net; yanlış anlaşılmaz'),
    examples: [
      ex('Die Release-Notiz muss unmissverständlich erklären, welche Funktionen nach dem Update nicht mehr unterstützt werden.', m('يجب أن تشرح ملاحظة الإصدار بوضوح لا لبس فيه أي وظائف لن تعود مدعومة بعد التحديث.','تێبینی release دەبێت بە بێگومان ڕوون بکاتەوە کام فەنکشنەکان دوای نوێکردنەوە پشتگیری ناکرێن.','The release note must explain unmistakably which functions will no longer be supported after the update.','یادداشت انتشار باید کاملاً روشن توضیح دهد کدام قابلیت‌ها پس از به‌روزرسانی دیگر پشتیبانی نمی‌شوند.','Têbîniya release divê bêguman zelal bike ka kîjan fonksiyon piştî nûkirinê êdî nayên piştgirîkirin.','Notatka wydania musi jednoznacznie wyjaśnić, które funkcje po aktualizacji nie będą już obsługiwane.','Nota de release trebuie să explice neechivoc ce funcții nu vor mai fi suportate după actualizare.','Примечание к релизу должно недвусмысленно объяснить, какие функции после обновления больше не поддерживаются.','Shënimi i release-it duhet të shpjegojë qartë cilat funksione nuk do të mbështeten më pas përditësimit.','Sürüm notu, güncellemeden sonra hangi işlevlerin artık desteklenmeyeceğini açıkça açıklamalıdır.')),
      ex('Die Zeugin formuliert unmissverständlich, dass sie den Täter nicht gesehen, sondern nur gehört hat.', m('تقول الشاهدة بوضوح لا لبس فيه إنها لم تر الجاني بل سمعته فقط.','شایەتەکە بە بێگومان دەڵێت کە تاوانبارەکەی نەبینیوە، بەڵکو تەنها بیستویەتی.','The witness states unmistakably that she did not see the perpetrator, but only heard him.','شاهد بدون ابهام بیان می‌کند که مجرم را ندیده، فقط صدایش را شنیده است.','Şahid bêguman dibêje ku sûcdar nedîtiye, tenê bihîstiye.','Świadkini jednoznacznie stwierdza, że nie widziała sprawcy, lecz tylko go słyszała.','Martora formulează neechivoc că nu l-a văzut pe făptaș, ci doar l-a auzit.','Свидетельница недвусмысленно заявляет, что не видела преступника, а только слышала его.','Dëshmitarja shprehet qartë se nuk e ka parë autorin, por vetëm e ka dëgjuar.','Tanık, faili görmediğini, yalnızca duyduğunu açıkça ifade eder.'))
    ]
  }),
  entry({
    word: 'die Unmittelbarkeit', partOfSpeech: 'Noun', article: 'die', plural: null, syllableBreak: 'Un-mit-tel-bar-keit',
    topics: ['culture-and-media','customer-service','advanced-analysis'], usageLabels: ['formal','written','analysis','advanced'], grammarNotes: ['feminine abstract noun; normally singular'],
    collocations: [{ text: 'die Unmittelbarkeit der Erfahrung', meaning: 'the immediacy of experience' }],
    meanings: m('مباشرة؛ فورية التجربة','ڕاستەوخۆیی؛ هەستکردنی بێ ناوەند','immediacy; directness','بی‌واسطگی؛ فوریت تجربه','rasterastî; bênavendî','bezpośredniość','nemijlocire; imediatețe','непосредственность','drejtpërdrejtësi; menjëhershmëri','dolaysızlık; doğrudanlık'),
    examples: [
      ex('Der Live-Chat schafft eine Unmittelbarkeit, die klassische E-Mail-Kommunikation im Kundendienst kaum erreicht.', m('يوفر الدردشة الحية مباشرة قلما تحققها مراسلات البريد الإلكتروني التقليدية في خدمة العملاء.','لایڤ چات ڕاستەوخۆییەک دروست دەکات کە پەیوەندیی ئیمەیلی کلاسیک لە خزمەتی کڕیاردا بە زەحمەت پێی دەگات.','Live chat creates an immediacy that traditional email communication in customer service hardly achieves.','چت زنده بی‌واسطگی‌ای ایجاد می‌کند که ارتباط ایمیلی کلاسیک در خدمات مشتری به‌سختی به آن می‌رسد.','Live-chat rasterastiyek çêdike ku têkiliya e-mailê ya klasîk di xizmeta xerîdar de bi zor digihêje wê.','Czat na żywo tworzy bezpośredniość, której klasyczna komunikacja mailowa w obsłudze klienta rzadko dorównuje.','Chatul live creează o imediatețe pe care comunicarea clasică prin e-mail în serviciul clienți o atinge cu greu.','Живой чат создает непосредственность, которой классическая email-коммуникация в поддержке клиентов почти не достигает.','Chat-i live krijon një drejtpërdrejtësi që komunikimi klasik me email në shërbimin ndaj klientit vështirë e arrin.','Canlı sohbet, müşteri hizmetlerinde klasik e-posta iletişiminin zor yakaladığı bir dolaysızlık yaratır.')),
      ex('Die Unmittelbarkeit der Stimme macht die Aufnahme eindringlicher als jede spätere schriftliche Zusammenfassung.', m('تجعل مباشرة الصوت التسجيل أكثر تأثيراً من أي ملخص مكتوب لاحق.','ڕاستەوخۆیی دەنگەکە تۆمارەکە کاریگەرتری دەکات لە هەر کورتەیەکی نووسراوی دواتر.','The immediacy of the voice makes the recording more forceful than any later written summary.','بی‌واسطگی صدا، ضبط را از هر خلاصه مکتوب بعدی نافذتر می‌کند.','Rasterastiya deng tomarê ji her kurteya nivîskî ya paşê bandorkertir dike.','Bezpośredniość głosu czyni nagranie bardziej przejmującym niż każde późniejsze pisemne podsumowanie.','Nemijlocirea vocii face înregistrarea mai pătrunzătoare decât orice rezumat scris ulterior.','Непосредственность голоса делает запись сильнее любого последующего письменного резюме.','Drejtpërdrejtësia e zërit e bën regjistrimin më depërtues se çdo përmbledhje të mëvonshme me shkrim.','Sesin dolaysızlığı kaydı, sonradan yazılan her özetten daha etkileyici kılar.'))
    ]
  }),
  entry({
    word: 'die Unschärfe', partOfSpeech: 'Noun', article: 'die', plural: 'Unschärfen', syllableBreak: 'Un-schär-fe',
    topics: ['data-and-reporting','technology-and-it','advanced-analysis'], usageLabels: ['formal','written','analysis','business'],
    collocations: [{ text: 'methodische Unschärfe', meaning: 'methodological imprecision' }],
    meanings: m('عدم دقة؛ ضبابية','ناوردی؛ ناڕوونی','imprecision; blur; lack of sharpness','ناوضوح؛ عدم دقت','nezelalî; ne-hûrgulî','nieostrość; niedokładność','neclaritate; imprecizie','нечеткость; размытость','paqartësi; pasaktësi','bulanıklık; kesin olmama'),
    examples: [
      ex('Die Unschärfe in der Datenquelle war klein, reichte aber aus, um den Monatsbericht zu verfälschen.', m('كان عدم الدقة في مصدر البيانات صغيراً، لكنه كان كافياً لتشويه التقرير الشهري.','ناوردی لە سەرچاوەی داتادا بچووک بوو، بەڵام بەس بوو بۆ شێواندنی ڕاپۆرتی مانگانە.','The imprecision in the data source was small, but sufficient to distort the monthly report.','عدم دقت در منبع داده کوچک بود، اما برای مخدوش کردن گزارش ماهانه کافی بود.','Ne-hûrgulî di çavkaniya daneyan de biçûk bû, lê têra xwe bû ku rapora mehane şaş bike.','Nieostrość w źródle danych była niewielka, ale wystarczyła, by zafałszować raport miesięczny.','Imprecizia din sursa de date era mică, dar suficientă pentru a denatura raportul lunar.','Неточность в источнике данных была небольшой, но достаточной, чтобы исказить месячный отчет.','Pasaktësia në burimin e të dhënave ishte e vogël, por mjaftoi për të shtrembëruar raportin mujor.','Veri kaynağındaki belirsizlik küçüktü, ancak aylık raporu çarpıtmak için yeterliydi.')),
      ex('Die leichte Unschärfe des Fotos verstärkt den Eindruck, dass die Erinnerung selbst unsicher geworden ist.', m('تعزز الضبابية الخفيفة في الصورة الانطباع بأن الذكرى نفسها أصبحت غير مؤكدة.','کەمێک ناڕوونی وێنەکە هەستەکە بەهێزتر دەکات کە بیرەوەری خۆی نادڵنیا بووە.','The slight blur of the photo strengthens the impression that memory itself has become uncertain.','ناوضوح خفیف عکس این حس را تقویت می‌کند که خود خاطره نامطمئن شده است.','Nezelaliya sivik a wêneyê hesta ku bîranîn bixwe bûye nediyar xurt dike.','Lekka nieostrość zdjęcia wzmacnia wrażenie, że sama pamięć stała się niepewna.','Ușoara neclaritate a fotografiei întărește impresia că însăși amintirea a devenit nesigură.','Легкая размытость фотографии усиливает впечатление, что сама память стала ненадежной.','Paqartësia e lehtë e fotos e forcon përshtypjen se vetë kujtesa është bërë e pasigurt.','Fotoğraftaki hafif bulanıklık, hafızanın kendisinin güvensizleştiği izlenimini güçlendirir.'))
    ]
  }),
  entry({
    word: 'die Unscheinbarkeit', partOfSpeech: 'Noun', article: 'die', plural: null, syllableBreak: 'Un-schein-bar-keit',
    topics: ['culture-and-media','advanced-analysis','everyday-life'], usageLabels: ['formal','written','advanced','analysis'], grammarNotes: ['feminine abstract noun; normally singular'],
    collocations: [{ text: 'die Unscheinbarkeit eines Details', meaning: 'the inconspicuousness of a detail' }],
    meanings: m('عدم لفت الانتباه؛ تواضع المظهر','نادیاربوون؛ بەڕواڵەت گرنگ نەبوون','inconspicuousness; modest appearance','ناچشمگیری؛ کم‌جلوه‌بودن','neçavdêrî; neberçavbûn','niepozorność','discreție; lipsă de ostentație','неприметность','padukshmëri; thjeshtësi','göze çarpmama; sade görünüm'),
    examples: [
      ex('Die Unscheinbarkeit des kleinen Konfigurationsfehlers täuschte darüber hinweg, dass er den gesamten Importprozess blockieren konnte.', m('أخفى عدم بروز خطأ الإعداد الصغير حقيقة أنه كان يستطيع حظر عملية الاستيراد كلها.','نادیاربوونی هەڵەی بچوکی کۆنفیگ وای کرد ئەوە دیار نەبێت کە دەتوانی تەواوی پڕۆسەی import ڕابگرێت.','The inconspicuousness of the small configuration error concealed the fact that it could block the entire import process.','ناچشمگیری خطای کوچک پیکربندی پنهان کرد که می‌توانست کل فرایند import را متوقف کند.','Neberçavbûna çewtiya biçûk a konfigurasyonê veşart ku ew dikarî tevahiya pêvajoya importê asteng bike.','Niepozorność małego błędu konfiguracyjnego ukrywała fakt, że mógł on zablokować cały proces importu.','Discreția micii erori de configurare a ascuns faptul că putea bloca întregul proces de import.','Неприметность небольшой ошибки конфигурации скрывала то, что она могла заблокировать весь процесс импорта.','Padukshmëria e gabimit të vogël të konfigurimit fshehu faktin se mund të bllokonte të gjithë procesin e importit.','Küçük yapılandırma hatasının göze çarpmaması, tüm import sürecini engelleyebileceği gerçeğini gizledi.')),
      ex('Die Unscheinbarkeit der Nebenfigur macht ihre spätere Entscheidung umso wirkungsvoller.', m('يجعل عدم لفت الشخصية الثانوية للانتباه قرارها اللاحق أكثر تأثيراً.','نادیاربوونی کارەکتەری لاوەکی بڕیاری دواتری ئەو هێندە کاریگەرتری دەکات.','The inconspicuousness of the minor character makes her later decision all the more powerful.','ناچشمگیری شخصیت فرعی تصمیم بعدی او را بسیار اثرگذارتر می‌کند.','Neberçavbûna kesayeta kêlekî biryara wê ya paşê hêj bandorkertir dike.','Niepozorność postaci drugoplanowej czyni jej późniejszą decyzję tym bardziej skuteczną.','Discreția personajului secundar face decizia ei ulterioară cu atât mai puternică.','Неприметность второстепенного персонажа делает ее последующее решение тем более сильным.','Padukshmëria e personazhit dytësor e bën vendimin e saj të mëvonshëm edhe më ndikues.','Yan karakterin göze çarpmaması, sonraki kararını daha da etkili kılar.'))
    ]
  }),
  entry({
    word: 'unter Vorbehalt', partOfSpeech: 'Adverb', syllableBreak: 'un-ter Vor-be-halt',
    topics: ['law-and-compliance','business-communication','documents-and-administration'], usageLabels: ['formal','written','business','administrative'],
    collocations: [{ text: 'unter Vorbehalt zustimmen', meaning: 'to agree subject to reservation' }],
    meanings: m('مع التحفظ؛ بشرط لاحق','بە مەرج؛ لەژێر تێبینی','subject to reservation; conditionally','با قید شرط؛ مشروط','bi şert; bi rezervasyon','z zastrzeżeniem; warunkowo','sub rezervă; condiționat','с оговоркой; условно','me rezervë; kushtimisht','çekinceyle; şartlı olarak'),
    examples: [
      ex('Der Kunde akzeptierte das Angebot unter Vorbehalt, bis die offenen Datenschutzfragen geklärt sind.', m('قبل العميل العرض مع التحفظ إلى أن تُحسم مسائل حماية البيانات المفتوحة.','کڕیار پێشنیارەکەی بە مەرج پەسەند کرد تا پرسیارە کراوەکانی پاراستنی داتا ڕوون دەبنەوە.','The customer accepted the offer subject to reservation until the open data protection questions are clarified.','مشتری پیشنهاد را با قید شرط پذیرفت تا پرسش‌های باز حفاظت از داده روشن شوند.','Xerîdar pêşniyar bi şert pejirand heta pirsên vekirî yên parastina daneyan zelal bibin.','Klient zaakceptował ofertę z zastrzeżeniem, dopóki otwarte kwestie ochrony danych nie zostaną wyjaśnione.','Clientul a acceptat oferta sub rezervă până la clarificarea întrebărilor deschise privind protecția datelor.','Клиент принял предложение с оговоркой до прояснения открытых вопросов защиты данных.','Klienti e pranoi ofertën me rezervë derisa të sqarohen pyetjet e hapura për mbrojtjen e të dhënave.','Müşteri, açık veri koruma soruları netleşene kadar teklifi çekinceyle kabul etti.')),
      ex('Die Zusage der Figur steht unter Vorbehalt, weil sie dem Frieden der Familie noch nicht traut.', m('تبقى موافقة الشخصية مشروطة لأنها لا تثق بعد بسلام العائلة.','بەڵێنی کارەکتەرەکە بە مەرجە، چونکە هێشتا متمانەی بە ئاشتیی خێزانەکە نییە.','The character’s consent is subject to reservation because she still does not trust the family’s peace.','پذیرش شخصیت با قید شرط است، چون هنوز به صلح خانواده اعتماد ندارد.','Erêkirina kesayetê bi şert e, ji ber ku ew hîn bawerî bi aştiya malbatê nake.','Zgoda postaci jest warunkowa, ponieważ nie ufa jeszcze pokojowi w rodzinie.','Acordul personajului este sub rezervă, deoarece nu are încă încredere în pacea familiei.','Согласие персонажа остается условным, потому что она еще не доверяет семейному миру.','Pëlqimi i personazhit është me rezervë, sepse ajo ende nuk i beson paqes së familjes.','Karakterin onayı şartlıdır, çünkü ailenin barışına hâlâ güvenmez.'))
    ]
  })
];
const usedLabels = [...new Set(entries.flatMap(e => [...e.usageLabels, ...e.contextLabels]))];
const pkg = { packageVersion: '1.0', packageId: `de-${levelLower}-generated-batch-${batch}`, packageName: 'German C2 Generated Batch 083', source: 'Hybrid', defaultMeaningLanguages: langs, labels: usedLabels.map(k => labelsByKey.get(k)), entries, collections: [], scenarios: [], conversationStarterPacks: [], eventPreparationPacks: [] };
fs.writeFileSync(outPath, JSON.stringify(pkg, null, 2), 'utf8');
const cmd = `dotnet run --project "${path.join(root, 'src', 'Apps', 'DarwinLingua.ImportTool', 'DarwinLingua.ImportTool.csproj')}" -- --target shared --yes "${outPath}"`;
const result = cp.spawnSync(cmd, { shell: true, encoding: 'utf8', cwd: root });
const output = `${result.stdout || ''}${result.stderr || ''}`;
process.stdout.write(output);
const ok = result.status === 0 && output.includes('Entries imported: 6') && output.includes('Entries invalid: 0') && output.includes('Warnings: 0');
if (!ok) {
  fs.appendFileSync(path.join(root, 'content', 'generated', `${levelLower}-failed-words.txt`), `${new Date().toISOString()}\tbatch-${batch}\t${expected.join(', ')}\n`, 'utf8');
  throw new Error('Import did not meet strict success criteria; source not modified.');
}
const remaining = tokens.slice(expected.length);
fs.writeFileSync(sourcePath, remaining.join(', ') + (remaining.length ? '\n' : ''), 'utf8');
console.log(`SOURCE_UPDATED: yes`);
console.log(`SOURCE_FILE: ${sourcePath}`);
console.log(`JSON_FILE: ${outPath}`);
console.log(`PROCESSED: ${expected.join(' | ')}`);
console.log(`REMAINING_COUNT: ${remaining.length}`);
console.log(`FIRST_10_REMAINING: ${remaining.slice(0, 10).join(' | ')}`);

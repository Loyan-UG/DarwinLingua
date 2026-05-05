const fs = require('fs');
const path = require('path');
const cp = require('child_process');
const root = 'D:/_Projects/DarwinLingua';
const level = 'C2', levelLower = 'c2', batch = '081';
const sourcePath = path.join(root, 'content', `${level}.txt`);
const taxonomyPath = path.join(root, 'content', 'taxonomy', 'darwinlingua-taxonomy-v1.json');
const outPath = path.join(root, 'content', 'generated', `de-${levelLower}-generated-batch-${batch}.json`);
const packageId = `de-${levelLower}-generated-batch-${batch}`;
const langs = ['ar','ckb','en','fa','kmr','pl','ro','ru','sq','tr'];
const expected = ['die Unabgeschlossenheit','die Unauflösbarkeit','unausgesprochen','die Unausgesprochenheit','die Unausweichlichkeit','die Unbeirrbarkeit'];
const taxonomy = JSON.parse(fs.readFileSync(taxonomyPath, 'utf8'));
const topicKeys = new Set((taxonomy.topics || []).map(t => t.key));
const labelsByKey = new Map((taxonomy.labels || []).map(l => [l.key, l]));
const source = fs.readFileSync(sourcePath, 'utf8');
const tokens = source.split(',').map(s => s.trim()).filter(Boolean);
if (JSON.stringify(tokens.slice(0, 6)) !== JSON.stringify(expected)) throw new Error(`Unexpected first tokens: ${JSON.stringify(tokens.slice(0, 6))}`);
function meaning(ar, ckb, en, fa, kmr, pl, ro, ru, sq, tr) { return [ar, ckb, en, fa, kmr, pl, ro, ru, sq, tr].map((text, i) => ({ language: langs[i], text })); }
function ex(baseText, translations) { return { baseText, translations }; }
function entry(e) {
  for (const t of e.topics) if (!topicKeys.has(t)) throw new Error(`Unknown topic ${t}`);
  for (const l of [...(e.usageLabels || []), ...(e.contextLabels || [])]) if (!labelsByKey.has(l)) throw new Error(`Unknown label ${l}`);
  return Object.assign({ language: 'de', cefrLevel: level, article: null, plural: null, infinitive: null, pronunciationIpa: null, syllableBreak: null, contextLabels: [], grammarNotes: [], collocations: [], wordFamilies: [], relations: [] }, e);
}
const entries = [
  entry({
    word: 'die Unabgeschlossenheit', partOfSpeech: 'Noun', article: 'die', plural: null, syllableBreak: 'Un-ab-ge-schlos-sen-heit',
    topics: ['planning-and-projects','advanced-analysis','culture-and-media'], usageLabels: ['formal','written','analysis','advanced'], grammarNotes: ['feminine abstract noun; normally singular'],
    collocations: [{ text: 'die Unabgeschlossenheit eines Prozesses', meaning: 'the unfinished nature of a process' }],
    meanings: meaning('عدم الاكتمال؛ الطابع غير المنجز','تەواونەبوون؛ کۆتایی نەهاتن','unfinishedness; lack of closure','ناتمام‌بودگی؛ پایان‌نیافتگی','neqediyayîbûn; temamnebûn','nieukończenie; otwartość','neîncheiere; caracter neterminat','незавершенность','papërfundueshmëri; mungesë mbylljeje','tamamlanmamışlık; kapanmamışlık'),
    examples: [
      ex('Die Unabgeschlossenheit der Migration musste im Statusbericht klar benannt werden, statt sie hinter optimistischen Prozentzahlen zu verstecken.', meaning('كان يجب تسمية عدم اكتمال الترحيل بوضوح في تقرير الحالة بدلاً من إخفائه خلف نسب متفائلة.','دەبوو تەواونەبوونی کۆچکردنەکە لە ڕاپۆرتی دۆخدا بە ڕوونی ناوببرێت، نەک لە پشت ڕێژەی سەدی گەشبینانە بشاردرێتەوە.','The unfinished nature of the migration had to be stated clearly in the status report instead of being hidden behind optimistic percentages.','ناتمام‌بودگی مهاجرت باید در گزارش وضعیت روشن نام برده می‌شد، نه اینکه پشت درصدهای خوش‌بینانه پنهان شود.','Neqediyayîbûna koçberiyê diviya di rapora rewşê de bi zelalî bê gotin, ne ku li pişt rêjeyên hêvîdar were veşartin.','Nieukończenie migracji trzeba było jasno nazwać w raporcie statusowym, zamiast ukrywać je za optymistycznymi procentami.','Caracterul neterminat al migrării trebuia numit clar în raportul de stare, nu ascuns în spatele unor procente optimiste.','Незавершенность миграции нужно было прямо назвать в статусном отчете, а не скрывать за оптимистичными процентами.','Papërfundueshmëria e migrimit duhej të përmendej qartë në raportin e statusit, jo të fshihej pas përqindjeve optimiste.','Geçişin tamamlanmamışlığı durum raporunda iyimser yüzdelerin arkasına saklanmak yerine açıkça belirtilmeliydi.')),
      ex('Die Unabgeschlossenheit des Romans ist kein Mangel, sondern zwingt den Leser, die moralische Frage selbst weiterzuführen.', meaning('عدم اكتمال الرواية ليس نقصاً، بل يجبر القارئ على متابعة السؤال الأخلاقي بنفسه.','تەواونەبوونی ڕۆمانەکە کەموکوڕی نییە، بەڵکو خوێنەر ناچار دەکات پرسیاری ئەخلاقی خۆی بەردەوام بکات.','The unfinishedness of the novel is not a flaw, but forces the reader to continue the moral question.','ناتمام‌بودگی رمان نقص نیست، بلکه خواننده را وادار می‌کند پرسش اخلاقی را خودش ادامه دهد.','Neqediyayîbûna romanê ne kêmasî ye, lê xwendevan neçar dike ku pirsa exlaqî bixwe bidomîne.','Nieukończenie powieści nie jest wadą, lecz zmusza czytelnika do samodzielnego kontynuowania pytania moralnego.','Neîncheierea romanului nu este un defect, ci îl obligă pe cititor să continue singur întrebarea morală.','Незавершенность романа не является недостатком, а заставляет читателя самому продолжить моральный вопрос.','Papërfundueshmëria e romanit nuk është mangësi, por e detyron lexuesin ta vazhdojë vetë pyetjen morale.','Romanın tamamlanmamışlığı bir kusur değil, okuru ahlaki soruyu kendi başına sürdürmeye zorlar.'))
    ]
  }),
  entry({
    word: 'die Unauflösbarkeit', partOfSpeech: 'Noun', article: 'die', plural: null, syllableBreak: 'Un-auf-lös-bar-keit',
    topics: ['advanced-analysis','quality-and-risk','culture-and-media'], usageLabels: ['formal','written','academic','analysis'], grammarNotes: ['feminine abstract noun; normally singular'],
    collocations: [{ text: 'die Unauflösbarkeit eines Konflikts', meaning: 'the irresolvability of a conflict' }],
    meanings: meaning('استحالة الحل؛ عدم قابلية التفكيك','چارەسەرنەکراوی؛ لێکنەبوونەوە','irresolvability; impossibility of resolving','حل‌ناپذیری؛ گشودنی نبودن','çaresernebûn; venebûn','nierozwiązywalność','imposibilitate de rezolvare','неразрешимость','pazgjidhshmëri','çözülemezlik'),
    examples: [
      ex('Die Unauflösbarkeit des Zielkonflikts zwischen Kosten, Sicherheit und Geschwindigkeit wurde erst im Lenkungskreis akzeptiert.', meaning('لم يُقبل استحالة حل تعارض الأهداف بين التكلفة والأمان والسرعة إلا في لجنة التوجيه.','چارەسەرنەکراوی ململاێی ئامانجەکان لە نێوان تێچوو، ئاسایش و خێرایی تەنها لە کۆمیسیۆنی ڕێنماییدا پەسەندکرا.','The irresolvability of the trade-off between cost, security, and speed was accepted only in the steering committee.','حل‌ناپذیری تعارض میان هزینه، امنیت و سرعت تازه در کمیته راهبری پذیرفته شد.','Çaresernebûna nakokiya armancan di navbera mesref, ewlehî û lezê de tenê di komîteya rêveberiyê de hate pejirandin.','Nierozwiązywalność konfliktu celów między kosztem, bezpieczeństwem i szybkością zaakceptowano dopiero w komitecie sterującym.','Imposibilitatea de rezolvare a conflictului dintre cost, securitate și viteză a fost acceptată abia în comitetul de coordonare.','Неразрешимость конфликта целей между стоимостью, безопасностью и скоростью признали только в управляющем комитете.','Pazgjidhshmëria e konfliktit mes kostos, sigurisë dhe shpejtësisë u pranua vetëm në komitetin drejtues.','Maliyet, güvenlik ve hız arasındaki hedef çatışmasının çözülemezliği ancak yönlendirme kurulunda kabul edildi.')),
      ex('Die Unauflösbarkeit des letzten Rätsels bewahrt im Film einen Rest von metaphysischer Unruhe.', meaning('تحافظ استحالة حل اللغز الأخير في الفيلم على بقية من قلق ميتافيزيقي.','چارەسەرنەکراوی نهێنیی کۆتایی لە فیلمەکەدا پاشماوەیەک لە ناآرامیی مێتافیزیکی دەپارێزێت.','The irresolvability of the final riddle preserves a residue of metaphysical unease in the film.','حل‌ناپذیری معمای پایانی در فیلم باقی‌مانده‌ای از ناآرامی متافیزیکی را حفظ می‌کند.','Çaresernebûna razê dawî di fîlmê de mayînek ji nearamiya metafîzîkî diparêze.','Nierozwiązywalność ostatniej zagadki zachowuje w filmie resztkę metafizycznego niepokoju.','Imposibilitatea de a rezolva ultima enigmă păstrează în film o urmă de neliniște metafizică.','Неразрешимость последней загадки сохраняет в фильме остаток метафизического беспокойства.','Pazgjidhshmëria e enigmës së fundit ruan në film një mbetje shqetësimi metafizik.','Son bilmecenin çözülemezliği filmde metafizik bir huzursuzluk kalıntısını korur.'))
    ]
  }),
  entry({
    word: 'unausgesprochen', partOfSpeech: 'Adjective', syllableBreak: 'un-aus-ge-spro-chen',
    topics: ['business-communication','social-and-relationships','advanced-analysis'], usageLabels: ['formal','written','sensitive','analysis'],
    collocations: [{ text: 'unausgesprochene Erwartungen', meaning: 'unspoken expectations' }],
    meanings: meaning('غير مصرح به؛ ضمني','نەگوتراو؛ شاراوە','unspoken; implicit','ناگفته؛ ضمنی','negotî; veşartî','niewypowiedziany; domyślny','nerostit; implicit','невысказанный; подразумеваемый','i pathënë; implicit','söylenmemiş; örtük'),
    examples: [
      ex('Die unausgesprochene Erwartung, auch am Wochenende erreichbar zu sein, belastete das Entwicklungsteam spürbar.', meaning('أثقل التوقع غير المعلن بأن يكون الفريق متاحاً في عطلة نهاية الأسبوع فريق التطوير بشكل واضح.','چاوەڕوانیی نەگوتراو بۆ بەردەستبوون لە کۆتایی هەفتەدا بە شێوەیەکی هەستپێکراو باری لەسەر تیمی پەرەپێدان دانا.','The unspoken expectation of being reachable on weekends noticeably burdened the development team.','انتظار ناگفته برای در دسترس بودن در آخر هفته به‌طور محسوسی بر تیم توسعه فشار آورد.','Hêviya negotî ya berdestbûnê di dawiya hefteyê de bi awayekî xuya bar li tîma pêşvebirinê kir.','Niewypowiedziane oczekiwanie dostępności w weekend wyraźnie obciążało zespół deweloperski.','Așteptarea nerostită de a fi disponibil în weekend a împovărat vizibil echipa de dezvoltare.','Невысказанное ожидание быть на связи по выходным заметно нагружало команду разработки.','Pritshmëria e pathënë për të qenë i arritshëm në fundjavë e rëndoi dukshëm ekipin e zhvillimit.','Hafta sonu erişilebilir olma yönündeki söylenmemiş beklenti geliştirme ekibini belirgin biçimde zorladı.')),
      ex('Zwischen den Geschwistern bleibt unausgesprochen, wer damals wirklich die Entscheidung getroffen hat.', meaning('يبقى غير مصرح به بين الإخوة من اتخذ القرار فعلاً في ذلك الوقت.','لە نێوان خوشک و براکاندا نەگوتراو دەمێنێتەوە کە ئەوکات بەڕاستی کێ بڕیارەکەی دا.','Between the siblings, it remains unspoken who really made the decision back then.','میان خواهر و برادرها ناگفته می‌ماند که آن زمان واقعاً چه کسی تصمیم را گرفت.','Di navbera xwişk û birayan de negotî dimîne ka wê demê bi rastî kê biryar da.','Między rodzeństwem pozostaje niewypowiedziane, kto wtedy naprawdę podjął decyzję.','Între frați rămâne nerostit cine a luat cu adevărat decizia atunci.','Между братом и сестрой остается невысказанным, кто тогда на самом деле принял решение.','Mes vëllezërve e motrave mbetet e pathënë se kush e mori vërtet vendimin atëherë.','Kardeşler arasında o zaman kararı gerçekte kimin verdiği söylenmeden kalır.'))
    ]
  }),
  entry({
    word: 'die Unausgesprochenheit', partOfSpeech: 'Noun', article: 'die', plural: null, syllableBreak: 'Un-aus-ge-spro-chen-heit',
    topics: ['social-and-relationships','business-communication','culture-and-media'], usageLabels: ['formal','written','sensitive','analysis'], grammarNotes: ['feminine abstract noun; normally singular'],
    collocations: [{ text: 'die Unausgesprochenheit eines Konflikts', meaning: 'the unspoken nature of a conflict' }],
    meanings: meaning('حالة عدم التصريح؛ المسكوت عنه','نەگوتراویی؛ بێدەنگیی شاراوە','unspokenness; state of remaining unsaid','ناگفتگی؛ در سکوت ماندگی','negotîbûn; bêdengiya veşartî','niewypowiedzianość','nerostire; caracter nespus','невысказанность','pathënshmëri','söylenmemişlik'),
    examples: [
      ex('Die Unausgesprochenheit der Rollenverteilung führte dazu, dass Verantwortung immer erst im Krisenfall sichtbar wurde.', meaning('أدى عدم التصريح بتوزيع الأدوار إلى أن المسؤولية لم تكن تظهر إلا في حالة الأزمة.','نەگوتراویی دابەشکردنی ڕۆڵەکان وای کرد بەرپرسیاری هەمیشە تەنها لە کاتی قەیراندا دیار ببێت.','The unspoken nature of the role distribution meant that responsibility became visible only in a crisis.','ناگفتگی تقسیم نقش‌ها باعث شد مسئولیت همیشه فقط در زمان بحران آشکار شود.','Negotîbûna dabeşkirina rolan kir ku berpirsiyarî her tim tenê di dema krîzê de xuya bibe.','Niewypowiedzianość podziału ról sprawiła, że odpowiedzialność ujawniała się dopiero w kryzysie.','Caracterul nerostit al împărțirii rolurilor a făcut ca responsabilitatea să devină vizibilă abia în criză.','Невысказанность распределения ролей привела к тому, что ответственность становилась видимой только в кризис.','Pathënshmëria e ndarjes së roleve bëri që përgjegjësia të bëhej e dukshme vetëm në krizë.','Rol dağılımının söylenmemişliği, sorumluluğun ancak kriz anında görünür olmasına yol açtı.')),
      ex('Die Unausgesprochenheit der Trauer prägt jede Szene stärker als die wenigen direkten Geständnisse.', meaning('تشكل حالة الحزن غير المصرح به كل مشهد أكثر من الاعترافات المباشرة القليلة.','نەگوتراویی خەم هەر دیمەنێک زیاتر لە چەند دانپێدانانی ڕاستەوخۆ کاریگەری دەکات.','The unspokenness of grief shapes every scene more strongly than the few direct confessions.','ناگفتگی اندوه هر صحنه را بیش از چند اعتراف مستقیم شکل می‌دهد.','Negotîbûna xemê ji çend dana pejirandinên rasterast zêdetir her dîmenê şekil dide.','Niewypowiedzianość żałoby silniej kształtuje każdą scenę niż nieliczne bezpośrednie wyznania.','Nerostirea durerii modelează fiecare scenă mai puternic decât puținele mărturisiri directe.','Невысказанность горя формирует каждую сцену сильнее, чем немногочисленные прямые признания.','Pathënshmëria e pikëllimit formëson çdo skenë më fort se pak rrëfimet e drejtpërdrejta.','Kederin söylenmemişliği her sahneyi, az sayıdaki doğrudan itiraftan daha güçlü biçimde şekillendirir.'))
    ]
  }),
  entry({
    word: 'die Unausweichlichkeit', partOfSpeech: 'Noun', article: 'die', plural: null, syllableBreak: 'Un-aus-weich-lich-keit',
    topics: ['advanced-analysis','management-and-leadership','culture-and-media'], usageLabels: ['formal','written','analysis','advanced'], grammarNotes: ['feminine abstract noun; normally singular'],
    collocations: [{ text: 'die Unausweichlichkeit einer Entscheidung', meaning: 'the inevitability of a decision' }],
    meanings: meaning('الحتمية؛ عدم إمكان التفادي','هەڵنەهاتن؛ ناچاربوون','inevitability; inescapability','گریزناپذیری؛ اجتناب‌ناپذیری','neçarî; jêderneketin','nieuchronność','inevitabilitate','неизбежность','pashmangshmëri; pashmangësi','kaçınılmazlık'),
    examples: [
      ex('Die Unausweichlichkeit der Systemablösung wurde akzeptiert, als die letzten Sicherheitsupdates ausliefen.', meaning('قُبلت حتمية استبدال النظام عندما انتهت آخر تحديثات الأمان.','هەڵنەهاتنی گۆڕینی سیستەمەکە پەسەندکرا کاتێک دوا نوێکارییەکانی ئاسایش بەسەرچوون.','The inevitability of replacing the system was accepted when the last security updates expired.','گریزناپذیری جایگزینی سیستم زمانی پذیرفته شد که آخرین به‌روزرسانی‌های امنیتی پایان یافت.','Neçarîya guhertina pergalê hate pejirandin dema nûkirinên dawî yên ewlehiyê qediya.','Nieuchronność wymiany systemu zaakceptowano, gdy wygasły ostatnie aktualizacje bezpieczeństwa.','Inevitabilitatea înlocuirii sistemului a fost acceptată când ultimele actualizări de securitate au expirat.','Неизбежность замены системы признали, когда истекли последние обновления безопасности.','Pashmangshmëria e zëvendësimit të sistemit u pranua kur skaduan përditësimet e fundit të sigurisë.','Sistemin değiştirilmesinin kaçınılmazlığı son güvenlik güncellemeleri sona erdiğinde kabul edildi.')),
      ex('Der Film erzeugt ein Gefühl von Unausweichlichkeit, obwohl lange nichts Dramatisches geschieht.', meaning('يخلق الفيلم شعوراً بالحتمية رغم أنه لا يحدث شيء درامي لفترة طويلة.','فیلمەکە هەستێکی هەڵنەهاتن دروست دەکات، هەرچەندە ماوەیەکی درێژ هیچ شتێکی دراماتیکی ڕوونادات.','The film creates a sense of inevitability although nothing dramatic happens for a long time.','فیلم حس گریزناپذیری ایجاد می‌کند، با اینکه مدت زیادی اتفاق دراماتیکی نمی‌افتد.','Fîlm hesteke neçarî diafirîne herçend demek dirêj tiştek dramatîk çênabe.','Film tworzy poczucie nieuchronności, choć przez długi czas nie dzieje się nic dramatycznego.','Filmul creează un sentiment de inevitabilitate, deși mult timp nu se întâmplă nimic dramatic.','Фильм создает чувство неизбежности, хотя долго не происходит ничего драматического.','Filmi krijon ndjenjë pashmangshmërie, megjithëse për shumë kohë nuk ndodh asgjë dramatike.','Film uzun süre dramatik hiçbir şey olmamasına rağmen kaçınılmazlık duygusu yaratır.'))
    ]
  }),
  entry({
    word: 'die Unbeirrbarkeit', partOfSpeech: 'Noun', article: 'die', plural: null, syllableBreak: 'Un-be-irr-bar-keit',
    topics: ['management-and-leadership','social-and-relationships','advanced-analysis'], usageLabels: ['formal','written','advanced','analysis'], grammarNotes: ['feminine abstract noun; normally singular'],
    collocations: [{ text: 'bewundernswerte Unbeirrbarkeit', meaning: 'admirable steadfastness' }],
    meanings: meaning('ثبات لا يتزعزع؛ إصرار هادئ','لەخۆنەچوون؛ پێداگریی نەگۆڕ','steadfastness; unwavering resolve','ثبات قدم؛ تزلزل‌ناپذیری','nehejandin; biryarîya neguher','niezłomność','neclintire; perseverență fermă','непоколебимость','palëkundshmëri','sarsılmazlık; kararlılık'),
    examples: [
      ex('Die Unbeirrbarkeit der Projektleiterin half, die Migration trotz politischer Widerstände zu Ende zu bringen.', meaning('ساعد ثبات مديرة المشروع على إنهاء الترحيل رغم المقاومة السياسية.','لەخۆنەچوونی بەڕێوەبەری پڕۆژەکە یارمەتیدا کۆچکردنەکە سەرەڕای بەربەرەکانی سیاسی بگاتە کۆتایی.','The project manager’s steadfastness helped bring the migration to completion despite political resistance.','ثبات قدم مدیر پروژه کمک کرد مهاجرت با وجود مقاومت‌های سیاسی به پایان برسد.','Nehejandina rêvebera projeyê alîkarî kir ku koçberî tevî berxwedanên siyasî bigihêje dawiyê.','Niezłomność kierowniczki projektu pomogła doprowadzić migrację do końca mimo politycznego oporu.','Neclintirea managerului de proiect a ajutat la finalizarea migrării în ciuda rezistențelor politice.','Непоколебимость руководительницы проекта помогла довести миграцию до конца, несмотря на политическое сопротивление.','Palëkundshmëria e menaxheres së projektit ndihmoi që migrimi të përfundonte pavarësisht rezistencave politike.','Proje yöneticisinin sarsılmazlığı, siyasi dirençlere rağmen geçişin tamamlanmasına yardımcı oldu.')),
      ex('In der Erzählung wirkt seine Unbeirrbarkeit zunächst heroisch, später aber beinahe unheimlich.', meaning('في القصة يبدو ثباته في البداية بطولياً، لكنه لاحقاً يصبح شبه مخيف.','لە گێڕانەوەکەدا لەخۆنەچوونی سەرەتا پاڵەوانانە دەردەکەوێت، بەڵام دواتر نزیکەی ترسناک دەبێت.','In the story, his steadfastness first seems heroic, but later almost uncanny.','در روایت، ثبات قدم او ابتدا قهرمانانه به نظر می‌رسد، اما بعداً تقریباً هراس‌انگیز می‌شود.','Di vegotinê de nehejandina wî pêşî lehengî xuya dike, lê paşê hema tirsnak dibe.','W opowiadaniu jego niezłomność początkowo wydaje się heroiczna, później jednak niemal niesamowita.','În povestire, neclintirea lui pare inițial eroică, dar mai târziu aproape neliniștitoare.','В рассказе его непоколебимость сначала кажется героической, а позже почти жуткой.','Në rrëfim, palëkundshmëria e tij fillimisht duket heroike, por më vonë pothuajse e frikshme.','Anlatıda onun sarsılmazlığı önce kahramanca görünür, sonra neredeyse tekinsiz hale gelir.'))
    ]
  })
];
const usedLabels = [...new Set(entries.flatMap(e => [...e.usageLabels, ...e.contextLabels]))];
const pkg = { packageVersion: '1.0', packageId, packageName: 'German C2 Generated Batch 081', source: 'Hybrid', defaultMeaningLanguages: langs, labels: usedLabels.map(k => labelsByKey.get(k)), entries, collections: [], scenarios: [], conversationStarterPacks: [], eventPreparationPacks: [] };
fs.writeFileSync(outPath, JSON.stringify(pkg, null, 2), 'utf8');
const cmd = `dotnet run --project "${path.join(root, 'src', 'Apps', 'DarwinLingua.ImportTool', 'DarwinLingua.ImportTool.csproj')}" -- --target shared --yes "${outPath}"`;
const result = cp.spawnSync(cmd, { shell: true, encoding: 'utf8', cwd: root });
const output = `${result.stdout || ''}${result.stderr || ''}`;
process.stdout.write(output);
const ok = result.status === 0 && output.includes('Entries imported: 6') && output.includes('Entries invalid: 0') && output.includes('Warnings: 0');
if (!ok) {
  const failedPath = path.join(root, 'content', 'generated', `${levelLower}-failed-words.txt`);
  fs.appendFileSync(failedPath, `${new Date().toISOString()}\tbatch-${batch}\t${expected.join(', ')}\n`, 'utf8');
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

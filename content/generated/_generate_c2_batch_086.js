const fs = require('fs');
const path = require('path');
const cp = require('child_process');
const root = 'D:/_Projects/DarwinLingua';
const level = 'C2', levelLower = 'c2', batch = '086';
const sourcePath = path.join(root, 'content', `${level}.txt`);
const taxonomyPath = path.join(root, 'content', 'taxonomy', 'darwinlingua-taxonomy-v1.json');
const outPath = path.join(root, 'content', 'generated', `de-${levelLower}-generated-batch-${batch}.json`);
const langs = ['ar','ckb','en','fa','kmr','pl','ro','ru','sq','tr'];
const expected = ['verdreschen','verdrießen','verflechten','die Vergeblichkeit','vergegenwärtigen','verglimmen'];
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
    word: 'verdreschen', partOfSpeech: 'Verb', infinitive: 'verdreschen', syllableBreak: 'ver-dre-schen',
    topics: ['social-and-relationships','culture-and-media','business-communication'], usageLabels: ['informal','written','sensitive','advanced'],
    grammarNotes: ['colloquial or forceful verb; means to beat someone up or harshly criticize'],
    collocations: [{ text: 'jemanden verbal verdreschen', meaning: 'to verbally beat someone up' }],
    meanings: m('يضرب ضرباً مبرحاً؛ يهاجم بعنف لفظي','لێدان بە توندی؛ بە وشە توند هێرشکردن','to beat up; to attack harshly in words','کتک زدن؛ به‌شدت لفظی کوبیدن','bi tundî lêdan; bi gotinan êrîş kirin','zbić; ostro zaatakować słownie','a bate; a ataca dur verbal','поколотить; жестко разнести словами','rrah keq; sulmoj ashpër me fjalë','fena dövmek; sözle sertçe saldırmak'),
    examples: [
      ex('Im Review wurde der Vorschlag nicht sachlich geprüft, sondern regelrecht verdroschen.', m('في المراجعة لم يُفحص الاقتراح بموضوعية، بل هوجم بعنف.','لە review ـەکەدا پێشنیارەکە بە بابەتی نە پشکنرا، بەڵکو بە تەواوی توند هێرش کرایە سەر.','In the review, the proposal was not examined objectively but was downright torn apart.','در بازبینی، پیشنهاد به‌صورت عینی بررسی نشد، بلکه عملاً به‌شدت کوبیده شد.','Di reviewê de pêşniyar ne bi rastî hate kontrolkirin, lê bi tevahî hate şikandin.','Podczas review propozycji nie oceniono rzeczowo, lecz wręcz ją zmiażdżono.','În review, propunerea nu a fost analizată obiectiv, ci a fost pur și simplu desființată.','На ревью предложение не проверили по существу, а буквально разнесли.','Në review, propozimi nuk u shqyrtua në mënyrë faktike, por u sulmua ashpër.','Review sırasında öneri nesnel biçimde incelenmedi, adeta yerden yere vuruldu.')),
      ex('Die Szene wirkt brutal, weil der Held den Gegner nicht besiegt, sondern ihn fast verdreschen lässt.', m('يبدو المشهد وحشياً لأن البطل لا يهزم الخصم، بل يكاد يتركه يتعرض لضرب مبرح.','دیمەنەکە دڕندانە دەردەکەوێت، چونکە پاڵەوانەکە نەیارەکە نابەزێنێت، بەڵکو نزیکەی دەیهێڵێت بە توندی لێبدرێت.','The scene feels brutal because the hero does not defeat the opponent but nearly lets him be beaten up.','صحنه خشن به نظر می‌رسد، چون قهرمان دشمن را شکست نمی‌دهد، بلکه تقریباً می‌گذارد او را کتک بزنند.','Dîmen tund xuya dike, ji ber ku leheng dijberê naşîne, lê hema dihêle ku ew bê lêdan.','Scena wydaje się brutalna, bo bohater nie pokonuje przeciwnika, lecz niemal pozwala go pobić.','Scena pare brutală deoarece eroul nu îl învinge pe adversar, ci aproape lasă să fie bătut.','Сцена кажется жестокой, потому что герой не побеждает противника, а почти позволяет его избить.','Skena duket brutale, sepse heroi nuk e mposht kundërshtarin, por pothuajse lejon ta rrahin.','Sahne vahşi görünür, çünkü kahraman rakibini yenmez, neredeyse dövülmesine izin verir.'))
    ]
  }),
  entry({
    word: 'verdrießen', partOfSpeech: 'Verb', infinitive: 'verdrießen', syllableBreak: 'ver-drie-ßen',
    topics: ['social-and-relationships','business-communication','culture-and-media'], usageLabels: ['formal','written','advanced','sensitive'],
    grammarNotes: ['elevated verb; often impersonal: es verdrießt jemanden'],
    collocations: [{ text: 'es verdrießt jemanden', meaning: 'it annoys or vexes someone' }],
    meanings: m('يضايق؛ يغيظ','بێزارکردن؛ ناخۆشکردن','to vex; to annoy; to displease','آزردن؛ دلخور کردن','aciz kirin; xemgîn kirin','martwić; irytować','a supăra; a irita','огорчать; досаждать','mërzit; bezdis','canını sıkmak; rahatsız etmek'),
    examples: [
      ex('Es verdrießt den Kunden, wenn dieselbe Information bei jedem Supportkontakt erneut abgefragt wird.', m('يزعج العميل عندما تُطلب المعلومة نفسها من جديد في كل تواصل مع الدعم.','کڕیار بێزار دەبێت کاتێک هەمان زانیاری لە هەر پەیوەندییەکی پشتگیریدا دووبارە داوا دەکرێت.','It annoys the customer when the same information is requested again at every support contact.','مشتری دلخور می‌شود وقتی همان اطلاعات در هر تماس پشتیبانی دوباره پرسیده می‌شود.','Xerîdar aciz dibe dema heman agahî di her têkiliya piştgiriyê de dîsa tê xwestin.','Klienta irytuje, gdy przy każdym kontakcie z pomocą ponownie pyta się o te same informacje.','Clientul este iritat când aceeași informație este cerută din nou la fiecare contact cu suportul.','Клиента раздражает, когда при каждом обращении в поддержку снова запрашивают одну и ту же информацию.','Klienti mërzitet kur i njëjti informacion kërkohet përsëri në çdo kontakt me suportin.','Aynı bilgi her destek temasında yeniden istendiğinde müşterinin canı sıkılır.')),
      ex('Den Erzähler verdrießt weniger die Niederlage als die Gleichgültigkeit, mit der sie hingenommen wird.', m('ما يضايق الراوي ليس الهزيمة بقدر اللامبالاة التي تُقبل بها.','گێڕەرەوەکە کەمتر لە شکست بێزارە، زیاتر لە بێبایەخییەکەی کە پێی قبوڵ دەکرێت.','What vexes the narrator is less the defeat than the indifference with which it is accepted.','آنچه راوی را می‌آزارد کمتر شکست است و بیشتر بی‌تفاوتی‌ای که با آن پذیرفته می‌شود.','Ya vegêr aciz dike kêmtir têkçûn e, zêdetir bêxemîya ku pê tê qebûlkirin e.','Narratora martwi nie tyle porażka, ile obojętność, z jaką zostaje przyjęta.','Pe narator îl supără mai puțin înfrângerea decât indiferența cu care este acceptată.','Рассказчика огорчает не столько поражение, сколько равнодушие, с которым его принимают.','Rrëfimtarin e mërzit më pak humbja sesa indiferenca me të cilën pranohet.','Anlatıcıyı yenilgiden çok, onun kabullenildiği kayıtsızlık rahatsız eder.'))
    ]
  }),
  entry({
    word: 'verflechten', partOfSpeech: 'Verb', infinitive: 'verflechten', syllableBreak: 'ver-flech-ten',
    topics: ['advanced-analysis','business-communication','culture-and-media'], usageLabels: ['formal','written','analysis','business'],
    collocations: [{ text: 'Prozesse miteinander verflechten', meaning: 'to interweave processes' }],
    meanings: m('يشبك؛ ينسج معاً','تێکەڵکردن؛ پێکەوە ئاڵاندن','to interweave; to intertwine','درهم‌تنیدن؛ به هم پیوند دادن','tevlihev kirin; pêk ve peçandin','splatać; przeplatać','a împleti; a întrețese','переплетать; связывать','ndërthur; gërshetoj','iç içe geçirmek; örmek'),
    examples: [
      ex('Die neue Plattform verflicht Vertrieb, Abrechnung und Support so eng, dass Änderungen nur noch gemeinsam geplant werden können.', m('تربط المنصة الجديدة المبيعات والفوترة والدعم بشكل وثيق إلى حد أن التغييرات لا يمكن التخطيط لها إلا معاً.','پلاتفۆرمی نوێ فرۆشتن، ژمێریاری و پشتگیری ئەوەندە بە توندی پێکەوە دەئاڵێنێت کە گۆڕانکارییەکان تەنها پێکەوە پلاندادەنرێن.','The new platform interweaves sales, billing, and support so tightly that changes can only be planned together.','پلتفرم جدید فروش، صورتحساب و پشتیبانی را چنان در هم می‌تند که تغییرها فقط می‌توانند مشترک برنامه‌ریزی شوند.','Platforma nû firotan, hesabkirin û piştgiriyê ewqas nêz ve girê dide ku guherîn tenê bi hev re tên plansazkirin.','Nowa platforma tak ściśle splata sprzedaż, rozliczenia i wsparcie, że zmiany można planować tylko wspólnie.','Noua platformă întrețese vânzările, facturarea și suportul atât de strâns încât schimbările pot fi planificate doar împreună.','Новая платформа так тесно переплетает продажи, биллинг и поддержку, что изменения можно планировать только совместно.','Platforma e re ndërthur shitjet, faturimin dhe suportin aq ngushtë sa ndryshimet mund të planifikohen vetëm së bashku.','Yeni platform satış, faturalama ve desteği o kadar sıkı iç içe geçiriyor ki değişiklikler ancak birlikte planlanabiliyor.')),
      ex('Der Roman verflicht private Erinnerungen mit politischen Ereignissen, ohne eines dem anderen unterzuordnen.', m('ينسج الرواية الذكريات الخاصة مع الأحداث السياسية من دون إخضاع أحدهما للآخر.','ڕۆمانەکە بیرەوەرییە تایبەتییەکان لەگەڵ ڕووداوە سیاسییەکان تێکەڵ دەکات، بەبێ ئەوەی یەکێکیان بخاتە ژێر ئەوی تر.','The novel interweaves private memories with political events without subordinating one to the other.','رمان خاطره‌های خصوصی را با رویدادهای سیاسی در هم می‌تند، بدون آن‌که یکی را تابع دیگری کند.','Roman bîranînên taybet bi bûyerên siyasî re tevlihev dike bê ku yekê bike binê yê din.','Powieść splata prywatne wspomnienia z wydarzeniami politycznymi, nie podporządkowując jednego drugiemu.','Romanul împletește amintiri private cu evenimente politice fără a subordona una celeilalte.','Роман переплетает личные воспоминания с политическими событиями, не подчиняя одно другому.','Romani ndërthur kujtimet private me ngjarjet politike pa ia nënshtruar njërën tjetrës.','Roman özel anıları siyasi olaylarla iç içe geçirir, birini diğerine tabi kılmadan.'))
    ]
  }),
  entry({
    word: 'die Vergeblichkeit', partOfSpeech: 'Noun', article: 'die', plural: null, syllableBreak: 'Ver-geb-lich-keit',
    topics: ['advanced-analysis','social-and-relationships','culture-and-media'], usageLabels: ['formal','written','advanced','analysis'], grammarNotes: ['feminine abstract noun; normally singular'],
    collocations: [{ text: 'die Vergeblichkeit eines Versuchs', meaning: 'the futility of an attempt' }],
    meanings: m('عبثية الجهد؛ عدم الجدوى','بێسوودی؛ هەوڵی بێئەنجام','futility; fruitlessness','بیهودگی؛ بی‌ثمری','bêsûdî; bêencambûn','daremność; bezowocność','zădărnicie; inutilitate','тщетность; бесплодность','kotësi; pafrytshmëri','boşunalık; sonuçsuzluk'),
    examples: [
      ex('Die Vergeblichkeit der manuellen Korrekturen wurde deutlich, als die falschen Daten am nächsten Tag wieder importiert wurden.', m('اتضحت عبثية التصحيحات اليدوية عندما استوردت البيانات الخاطئة مرة أخرى في اليوم التالي.','بێسوودی چاککردنەوە دەستییەکان ڕوون بوو کاتێک داتای هەڵە ڕۆژی دواتر دووبارە import کرا.','The futility of the manual corrections became clear when the wrong data was imported again the next day.','بیهودگی اصلاحات دستی وقتی روشن شد که داده‌های غلط روز بعد دوباره import شدند.','Bêsûdiya rastkirinên destî zelal bû dema daneyên şaş roja din dîsa import bûn.','Daremność ręcznych korekt stała się jasna, gdy błędne dane następnego dnia ponownie zaimportowano.','Inutilitatea corecturilor manuale a devenit clară când datele greșite au fost importate din nou a doua zi.','Тщетность ручных исправлений стала очевидной, когда неверные данные на следующий день снова импортировали.','Kotësia e korrigjimeve manuale u bë e qartë kur të dhënat e gabuara u importuan sërish ditën tjetër.','Manuel düzeltmelerin boşunalığı, yanlış veriler ertesi gün yeniden import edildiğinde anlaşıldı.')),
      ex('Die Vergeblichkeit seiner Suche macht die letzten Seiten nicht hoffnungslos, sondern ungewöhnlich ruhig.', m('لا تجعل عبثية بحثه الصفحات الأخيرة يائسة، بل هادئة على نحو غير مألوف.','بێسوودی گەڕانەکەی لاپەڕە کۆتاییەکان بێهیوا ناکات، بەڵکو بە شێوەیەکی نائاسایی ئارام دەکات.','The futility of his search does not make the final pages hopeless, but unusually calm.','بیهودگی جست‌وجوی او صفحات پایانی را نومیدانه نمی‌کند، بلکه به‌طرزی غیرمعمول آرام می‌سازد.','Bêsûdiya lêgerîna wî rûpelên dawî bêhêvî nake, lê bi awayekî neasayî aram dike.','Daremność jego poszukiwań nie czyni ostatnich stron beznadziejnymi, lecz niezwykle spokojnymi.','Zădărnicia căutării lui nu face ultimele pagini lipsite de speranță, ci neobișnuit de calme.','Тщетность его поисков делает последние страницы не безнадежными, а необычайно спокойными.','Kotësia e kërkimit të tij nuk i bën faqet e fundit të pashpresa, por jashtëzakonisht të qeta.','Arayışının boşunalığı son sayfaları umutsuz değil, alışılmadık derecede sakin kılar.'))
    ]
  }),
  entry({
    word: 'vergegenwärtigen', partOfSpeech: 'Verb', infinitive: 'vergegenwärtigen', syllableBreak: 'ver-ge-gen-wär-ti-gen',
    topics: ['education-and-training','advanced-analysis','business-communication'], usageLabels: ['formal','written','academic','analysis'],
    collocations: [{ text: 'sich die Folgen vergegenwärtigen', meaning: 'to make oneself aware of the consequences' }],
    meanings: m('يستحضر؛ يجعل حاضراً في الذهن','لەبەرچاواندن؛ لە هۆشدا ئامادەکردن','to make present to oneself; to realize vividly','به‌خاطر آوردن؛ در ذهن حاضر کردن','di hiş de amade kirin; bi zelalî têgihiştin','uświadomić sobie; uprzytomnić','a-și reprezenta clar; a conștientiza','осознать; представить себе','sjell në mendje; ndërgjegjësohem','göz önüne getirmek; iyice kavramak'),
    examples: [
      ex('Bevor wir die Altversion abschalten, müssen wir uns vergegenwärtigen, welche Kunden noch von ihr abhängen.', m('قبل أن نوقف النسخة القديمة يجب أن نستحضر بوضوح أي عملاء ما زالوا يعتمدون عليها.','پێش ئەوەی وەشانی کۆن بوەستێنین، دەبێت لەبەرچاومان بێت کام کڕیاران هێشتا پشت بەو دەبەستن.','Before we shut down the old version, we must make ourselves aware of which customers still depend on it.','قبل از خاموش کردن نسخه قدیمی، باید روشن در ذهن داشته باشیم کدام مشتریان هنوز به آن وابسته‌اند.','Berî ku em versiyona kevn bigirin, divê em bi zelalî bibînin ka kîjan xerîdar hîn pê ve girêdayî ne.','Zanim wyłączymy starą wersję, musimy uświadomić sobie, którzy klienci wciąż od niej zależą.','Înainte să oprim versiunea veche, trebuie să ne reprezentăm clar ce clienți mai depind de ea.','Перед отключением старой версии мы должны ясно осознать, какие клиенты все еще от нее зависят.','Para se ta fikim versionin e vjetër, duhet të sjellim qartë në mendje cilët klientë varen ende prej tij.','Eski sürümü kapatmadan önce hangi müşterilerin hâlâ ona bağlı olduğunu göz önüne getirmeliyiz.')),
      ex('Der Text vergegenwärtigt die Vergangenheit nicht durch Daten, sondern durch Gerüche, Geräusche und brüchige Sätze.', m('يستحضر النص الماضي لا عبر التواريخ، بل عبر الروائح والأصوات والجمل المتكسرة.','دەقەکە ڕابردوو نە بە بەروار، بەڵکو بە بۆن، دەنگ و ڕستەی پچڕپچڕ لەبەرچاو دەهێنێت.','The text makes the past present not through dates, but through smells, sounds, and fractured sentences.','متن گذشته را نه از راه تاریخ‌ها، بلکه از طریق بوها، صداها و جمله‌های گسسته حاضر می‌کند.','Nivîs rabirdûyê ne bi tarîxan, lê bi bîn, deng û hevokên şikestî di hiş de amade dike.','Tekst uprzytamnia przeszłość nie przez daty, lecz przez zapachy, dźwięki i kruche zdania.','Textul face trecutul prezent nu prin date, ci prin mirosuri, sunete și propoziții frânte.','Текст делает прошлое настоящим не через даты, а через запахи, звуки и ломкие фразы.','Teksti e bën të tashme të kaluarën jo përmes datave, por përmes erërave, tingujve dhe fjalive të thyera.','Metin geçmişi tarihlerle değil, kokular, sesler ve kırık cümlelerle şimdiye taşır.'))
    ]
  }),
  entry({
    word: 'verglimmen', partOfSpeech: 'Verb', infinitive: 'verglimmen', syllableBreak: 'ver-glim-men',
    topics: ['culture-and-media','everyday-life','advanced-analysis'], usageLabels: ['formal','written','advanced','analysis'],
    collocations: [{ text: 'langsam verglimmen', meaning: 'to slowly fade or die down like embers' }],
    meanings: m('يخبو؛ ينطفئ تدريجياً','هێواش هێواش کوژانەوە؛ کاڵبوونەوە','to fade away; to die down like embers','رو به خاموشی رفتن؛ کم‌کم فروکش کردن','hêdî vemirîn; lawaz bûn','dogasać; przygasać','a se stinge treptat','догорать; угасать','shuhet ngadalë','sönüp gitmek; köz gibi azalmak'),
    examples: [
      ex('Nach der großen Ankündigung verglimmte die Initiative, weil niemand Budget und Verantwortung verbindlich übernahm.', m('بعد الإعلان الكبير خبت المبادرة لأن أحداً لم يتول الميزانية والمسؤولية بشكل ملزم.','دوای ڕاگەیاندنی گەورە، هەوڵەکە هێواش هێواش کوژایەوە، چونکە کەس بودجە و بەرپرسیاری بە شێوەی پابەند وەرنەگرت.','After the big announcement, the initiative faded away because no one bindingly took over budget and responsibility.','پس از اعلام بزرگ، ابتکار عمل کم‌کم خاموش شد، چون هیچ‌کس بودجه و مسئولیت را الزام‌آور بر عهده نگرفت.','Piştî ragihandina mezin, destpêşxerî hêdî vemirî ji ber ku kesek budce û berpirsiyarî bi girêdan wernagirt.','Po wielkim ogłoszeniu inicjatywa przygasła, ponieważ nikt wiążąco nie przejął budżetu i odpowiedzialności.','După marele anunț, inițiativa s-a stins treptat deoarece nimeni nu și-a asumat ferm bugetul și responsabilitatea.','После громкого объявления инициатива угасла, потому что никто твердо не взял на себя бюджет и ответственность.','Pas njoftimit të madh, nisma u shua ngadalë sepse askush nuk mori detyrueshëm buxhetin dhe përgjegjësinë.','Büyük duyurudan sonra girişim sönüp gitti, çünkü kimse bütçeyi ve sorumluluğu bağlayıcı biçimde üstlenmedi.')),
      ex('Am Ende verglimmt das Feuer im Ofen, während die Figuren schweigend am Tisch sitzen.', m('في النهاية يخبو النار في الموقد بينما تجلس الشخصيات صامتة حول الطاولة.','لە کۆتاییدا ئاگرەکە لە تەنووردا هێواش هێواش دەکوژێتەوە، کاتێک کارەکتەرەکان بێدەنگ لەسەر مێزەکە دانیشتوون.','At the end, the fire in the stove dies down while the characters sit silently at the table.','در پایان، آتش در بخاری کم‌کم خاموش می‌شود، در حالی که شخصیت‌ها خاموش دور میز نشسته‌اند.','Di dawiyê de agir di sobeyê de hêdî vemire, dema kesayet bêdeng li ser maseyê rûniştine.','Na końcu ogień w piecu dogasa, podczas gdy postacie milcząco siedzą przy stole.','La final, focul din sobă se stinge treptat, în timp ce personajele stau tăcute la masă.','В конце огонь в печи догорает, пока персонажи молча сидят за столом.','Në fund, zjarri në sobë shuhet ngadalë, ndërsa personazhet rrinë në heshtje rreth tavolinës.','Sonda sobadaki ateş sönüp giderken karakterler masada sessizce oturur.'))
    ]
  })
];
const usedLabels = [...new Set(entries.flatMap(e => [...e.usageLabels, ...e.contextLabels]))];
const pkg = { packageVersion: '1.0', packageId: `de-${levelLower}-generated-batch-${batch}`, packageName: 'German C2 Generated Batch 086', source: 'Hybrid', defaultMeaningLanguages: langs, labels: usedLabels.map(k => labelsByKey.get(k)), entries, collections: [], scenarios: [], conversationStarterPacks: [], eventPreparationPacks: [] };
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

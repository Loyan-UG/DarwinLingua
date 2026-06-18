const fs = require('fs');

const file = 'content/learning-portal/courses/packages/course-c2-stil-souveraenitaet-und-komplexer-diskurs-v1.json';
const data = JSON.parse(fs.readFileSync(file, 'utf8'));
const lessons = data.courseLessons || data.lessons;
const langs = ['en', 'fa', 'ar', 'tr', 'ru', 'ckb', 'kmr', 'pl', 'ro', 'sq'];

const titles = {
  orient: {
    de: 'Diskursrolle klaeren',
    en: 'Clarify your discourse role',
    fa: 'نقش خودت در بحث علمی را روشن کن',
    ar: 'وضّح دورك في الخطاب الأكاديمي',
    tr: 'Akademik tartışmadaki rolünü netleştir',
    ru: 'Проясни свою роль в научной дискуссии',
    ckb: 'ڕۆڵی خۆت لە گفتوگۆی ئەکادیمیدا ڕوون بکە',
    kmr: 'Rola xwe di gotûbêja akademîk de zelal bike',
    pl: 'Wyjaśnij swoją rolę w dyskursie',
    ro: 'Clarifică-ți rolul în discurs',
    sq: 'Qartëso rolin tënd në diskurs'
  },
  language: {
    de: 'Argumenthaltung sichern',
    en: 'Secure argumentative stance',
    fa: 'موضع استدلالی را محکم کن',
    ar: 'ثبّت الموقف الحجاجي',
    tr: 'Argümantatif duruşu güvenceye al',
    ru: 'Закрепи аргументативную позицию',
    ckb: 'هەڵوێستی بەڵگەهێنان دڵنیا بکە',
    kmr: 'Helwesta argumanî misoger bike',
    pl: 'Zabezpiecz postawę argumentacyjną',
    ro: 'Asigură poziția argumentativă',
    sq: 'Siguro qëndrimin argumentues'
  },
  target: {
    de: 'Einwand praktisch steuern',
    en: 'Handle the objection in practice',
    fa: 'ایراد را در عمل مدیریت کن',
    ar: 'أدر الاعتراض عمليًا',
    tr: 'İtirazı pratikte yönet',
    ru: 'Практически управляй возражением',
    ckb: 'ناڕەزایی بە کرداری بەڕێوەببە',
    kmr: 'Îtirazê di pratîkê de birêve bibe',
    pl: 'Poprowadź zastrzeżenie w praktyce',
    ro: 'Gestionează obiecția practic',
    sq: 'Menaxho kundërshtimin në praktikë'
  },
  transfer: {
    de: 'Position verdichten',
    en: 'Condense the position',
    fa: 'موضع را فشرده و دقیق بیان کن',
    ar: 'كثّف الموقف بدقة',
    tr: 'Pozisyonu yoğun ve kesin ifade et',
    ru: 'Сжато сформулируй позицию',
    ckb: 'هەڵوێستەکە بە چڕی و وردی دەرببڕە',
    kmr: 'Helwestê bi kurtî û daqîqî bibêje',
    pl: 'Zagęść stanowisko',
    ro: 'Condensează poziția',
    sq: 'Përmblidh pozicionin'
  },
  review: {
    de: 'Wissenschaftliche Redlichkeit pruefen',
    en: 'Check academic integrity',
    fa: 'درستی و انصاف علمی را بررسی کن',
    ar: 'افحص النزاهة العلمية',
    tr: 'Akademik dürüstlüğü kontrol et',
    ru: 'Проверь научную добросовестность',
    ckb: 'ڕاستگۆیی زانستی بپشکنە',
    kmr: 'Rastbûna akademîk kontrol bike',
    pl: 'Sprawdź rzetelność naukową',
    ro: 'Verifică onestitatea academică',
    sq: 'Kontrollo ndershmërinë akademike'
  }
};

const items = [
  {
    slug: 'c2-akademische-kontroverse-moderieren',
    topic: { de: 'akademische Kontroversen', en: 'academic controversies', fa: 'مناقشه‌های دانشگاهی', ar: 'الخلافات الأكاديمية', tr: 'akademik tartışmalar', ru: 'академические споры', ckb: 'ناکۆکییە ئەکادیمییەکان', kmr: 'nakokiyên akademîk', pl: 'kontrowersje akademickie', ro: 'controverse academice', sq: 'kundërthënie akademike' },
    focus: { de: 'Gegenpositionen fair sichtbar zu machen, ohne die eigene Linie aufzugeben', en: 'making opposing positions fairly visible without abandoning your own line', fa: 'موضع‌های مخالف را منصفانه نشان بدهی، بدون اینکه خط اصلی خودت را رها کنی', ar: 'إظهار المواقف المقابلة بإنصاف من دون التخلي عن خطك الأساسي', tr: 'karşı görüşleri adil biçimde görünür kılıp kendi çizgini bırakmamak', ru: 'честно показывать противоположные позиции, не отказываясь от собственной линии', ckb: 'هەڵوێستە دژەکان بە دادپەروەری پیشان بدەیت بەبێ وازهێنان لە هێڵی خۆت', kmr: 'helwestên dijber bi adilî xuya bikî bêyî ku xeta xwe berdî', pl: 'uczciwie pokazać stanowiska przeciwne bez porzucenia własnej linii', ro: 'să faci vizibile pozițiile opuse corect, fără să renunți la linia ta', sq: 't’i bësh të dukshme pozicionet kundërshtare me drejtësi pa braktisur vijën tënde' },
    grammar: 'c2-c2-debate-grammar',
    targetType: 'roleplay',
    targetSlug: 'c2-eine-akademische-kontroverse-moderieren'
  },
  {
    slug: 'c2-theorie-gegen-einwand-verteidigen',
    topic: { de: 'Theorie und Einwand', en: 'theory and objection', fa: 'نظریه و ایراد', ar: 'النظرية والاعتراض', tr: 'teori ve itiraz', ru: 'теория и возражение', ckb: 'تیۆری و ناڕەزایی', kmr: 'teorî û îtiraz', pl: 'teoria i zastrzeżenie', ro: 'teorie și obiecție', sq: 'teori dhe kundërshtim' },
    focus: { de: 'einen Einwand aufzunehmen, ohne defensiv zu werden oder die Theorie zu verkuerzen', en: 'taking up an objection without becoming defensive or reducing the theory', fa: 'یک ایراد را بپذیری و وارد بحث کنی، بدون اینکه دفاعی شوی یا نظریه را ساده‌سازی نادرست کنی', ar: 'استيعاب اعتراض من دون اتخاذ موقف دفاعي أو تبسيط النظرية بشكل مخل', tr: 'bir itirazı savunmaya geçmeden ve teoriyi daraltmadan ele almak', ru: 'принимать возражение, не уходя в защиту и не упрощая теорию неверно', ckb: 'ناڕەزاییەک وەربگریت بەبێ ئەوەی بەرگریکارانە بیت یان تیۆرییەکە کەم بکەیتەوە', kmr: 'îtirazekê bigirî bêyî ku berevanî bibî an teoriyê kurt û şaş bikî', pl: 'podjąć zastrzeżenie bez defensywności i bez zubożenia teorii', ro: 'să preiei o obiecție fără defensivitate și fără să reduci teoria', sq: 'ta marrësh një kundërshtim pa u bërë mbrojtës dhe pa e varfëruar teorinë' },
    grammar: 'c2-c2-debate-grammar',
    targetType: 'roleplay',
    targetSlug: 'c2-eine-theorie-gegen-einen-einwand-verteidigen'
  },
  {
    slug: 'c2-methodische-schwaeche-einraeumen',
    topic: { de: 'methodische Schwaechen', en: 'methodological weaknesses', fa: 'ضعف‌های روش‌شناختی', ar: 'نقاط الضعف المنهجية', tr: 'yöntemsel zayıflıklar', ru: 'методологические слабости', ckb: 'لاوازییە میتۆدییەکان', kmr: 'lawaziyên metodîk', pl: 'słabości metodologiczne', ro: 'slăbiciuni metodologice', sq: 'dobësi metodologjike' },
    focus: { de: 'Grenzen offen zu benennen, ohne den Wert der Arbeit unnoetig zu entwerten', en: 'naming limits openly without unnecessarily devaluing the work', fa: 'محدودیت‌ها را شفاف بگویی، بدون اینکه ارزش کار را بی‌دلیل کم کنی', ar: 'تسمية الحدود بوضوح من دون التقليل غير الضروري من قيمة العمل', tr: 'sınırları açıkça adlandırmak ama çalışmanın değerini gereksiz yere düşürmemek', ru: 'открыто называть ограничения, не обесценивая работу без необходимости', ckb: 'سنوورەکان بە ڕوونی ناوببەیت بەبێ ئەوەی بەبێ پێویستی نرخی کارەکە کەم بکەیتەوە', kmr: 'sînoran bi eşkere nav bikî bêyî ku nirxa karê bêhewce kêm bikî', pl: 'otwarcie nazwać ograniczenia bez niepotrzebnego obniżania wartości pracy', ro: 'să numești deschis limitele fără să devalorizezi inutil lucrarea', sq: 't’i thuash hapur kufizimet pa e ulur panevojshëm vlerën e punës' },
    grammar: 'c2-c2-formal-writing-grammar',
    targetType: 'roleplay',
    targetSlug: 'c2-eine-methodische-schwaeche-einraeumen'
  },
  {
    slug: 'c2-forschungsdesign-im-kolloquium-erklaeren',
    topic: { de: 'Forschungsdesign im Kolloquium', en: 'research design in a colloquium', fa: 'طرح پژوهش در کولکویم', ar: 'تصميم البحث في الكولوكيوم', tr: 'kolokyumda araştırma tasarımı', ru: 'исследовательский дизайн на коллоквиуме', ckb: 'دیزاینی توێژینەوە لە کۆلۆکیۆمدا', kmr: 'dîzayna lêkolînê di kolokyumê de', pl: 'projekt badawczy na kolokwium', ro: 'design de cercetare în colocviu', sq: 'dizajni i kërkimit në kolokium' },
    focus: { de: 'Fragestellung, Material, Methode und Grenze in einer nachvollziehbaren Reihenfolge zu erklaeren', en: 'explaining research question, material, method, and limit in a traceable order', fa: 'پرسش پژوهش، داده/متریال، روش و محدودیت را با ترتیبی قابل‌پیگیری توضیح بدهی', ar: 'شرح سؤال البحث والمادة والمنهج والحدود بترتيب يمكن تتبعه', tr: 'araştırma sorusu, malzeme, yöntem ve sınırı izlenebilir bir sırayla açıklamak', ru: 'объяснять вопрос, материал, метод и границы исследования в прослеживаемом порядке', ckb: 'پرسی توێژینەوە، ماددە، میتۆد و سنوور بە ڕیزێکی ئاسان بۆ شوێنکەوتن ڕوون بکەیتەوە', kmr: 'pirs, materyal, metod û sînorê lêkolînê bi rêzeke şopandinbar rave bikî', pl: 'wyjaśnić pytanie badawcze, materiał, metodę i granice w możliwej do śledzenia kolejności', ro: 'să explici întrebarea, materialul, metoda și limita într-o ordine ușor de urmărit', sq: 'të shpjegosh pyetjen, materialin, metodën dhe kufirin e kërkimit në një rend të ndjekshëm' },
    grammar: 'c2-academic-high-register-syntax',
    targetType: 'roleplay',
    targetSlug: 'c2-ein-forschungsdesign-im-kolloquium-erklaeren'
  },
  {
    slug: 'c2-prueferfrage-souveraen-umformulieren',
    topic: { de: 'Prueferfragen', en: 'examiner questions', fa: 'پرسش‌های ممتحن', ar: 'أسئلة الممتحن', tr: 'sınav görevlisi soruları', ru: 'вопросы экзаменатора', ckb: 'پرسیاری تاقیکەرەوە', kmr: 'pirsên examiner', pl: 'pytania egzaminatora', ro: 'întrebări ale examinatorului', sq: 'pyetje të ekzaminuesit' },
    focus: { de: 'eine schwierige Frage zu paraphrasieren und den Antwortauftrag sichtbar zu machen', en: 'paraphrasing a difficult question and making the answer task visible', fa: 'یک پرسش دشوار را بازگویی کنی و دقیق نشان بدهی از تو چه نوع پاسخی می‌خواهد', ar: 'إعادة صياغة سؤال صعب وإظهار نوع الإجابة المطلوبة بوضوح', tr: 'zor bir soruyu yeniden ifade edip hangi cevap görevinin istendiğini görünür kılmak', ru: 'переформулировать трудный вопрос и ясно показать требуемый тип ответа', ckb: 'پرسیارێکی قورس دووبارە بڵێیتەوە و ئەرکی وەڵامدانەوەکە ڕوون بکەیت', kmr: 'pirseke dijwar ji nû ve bibêjî û karê bersivdanê xuya bikî', pl: 'sparafrazować trudne pytanie i pokazać, jakiego typu odpowiedzi wymaga', ro: 'să parafrazezi o întrebare dificilă și să faci vizibil tipul de răspuns cerut', sq: 'ta perifrazosh një pyetje të vështirë dhe ta bësh të dukshme detyrën e përgjigjes' },
    grammar: 'c2-complex-reported-speech',
    targetType: 'roleplay',
    targetSlug: 'c2-eine-prueferfrage-souveraen-umformulieren'
  },
  {
    slug: 'c2-widerspruch-in-argumentation-aufloesen',
    topic: { de: 'Widerspruch in der Argumentation', en: 'contradiction in argumentation', fa: 'تناقض در استدلال', ar: 'التناقض في الحجاج', tr: 'argümantasyonda çelişki', ru: 'противоречие в аргументации', ckb: 'دژایەتی لە بەڵگەهێناندا', kmr: 'nakokî di argumanê de', pl: 'sprzeczność w argumentacji', ro: 'contradicție în argumentare', sq: 'kundërthënie në argumentim' },
    focus: { de: 'einen echten Widerspruch von einer notwendigen Differenzierung zu unterscheiden', en: 'distinguishing a real contradiction from a necessary differentiation', fa: 'تناقض واقعی را از یک تفکیک لازم و دقیق جدا کنی', ar: 'تمييز التناقض الحقيقي عن التفريق الضروري والدقيق', tr: 'gerçek bir çelişkiyi gerekli bir ayrımdan ayırmak', ru: 'отличать настоящее противоречие от необходимого уточнения', ckb: 'دژایەتی ڕاستەقینە لە جیاکردنەوەیەکی پێویست و ورد جیا بکەیتەوە', kmr: 'nakokiya rastîn ji cudakirineke pêwîst û daqîq cuda bikî', pl: 'odróżnić rzeczywistą sprzeczność od koniecznego rozróżnienia', ro: 'să distingi o contradicție reală de o diferențiere necesară', sq: 'të dallosh një kundërthënie reale nga një dallim i domosdoshëm' },
    grammar: 'c2-ambiguity-and-disambiguation',
    targetType: 'roleplay',
    targetSlug: 'c2-einen-widerspruch-in-der-argumentation-aufloesen'
  },
  {
    slug: 'c2-fachliche-kritik-ohne-abwertung',
    topic: { de: 'fachliche Kritik ohne Abwertung', en: 'technical criticism without devaluation', fa: 'نقد تخصصی بدون کم‌ارزش کردن طرف مقابل', ar: 'نقد متخصص من دون تقليل من قيمة الآخر', tr: 'değer düşürmeden uzman eleştirisi', ru: 'профессиональная критика без обесценивания', ckb: 'ڕەخنەی پسپۆڕی بەبێ کەمکردنەوەی نرخی بەرامبەر', kmr: 'rexneya pisporî bêyî kêmnirxkirin', pl: 'krytyka merytoryczna bez deprecjacji', ro: 'critică de specialitate fără devalorizare', sq: 'kritikë profesionale pa zhvlerësim' },
    focus: { de: 'Kritik an Sache, Methode oder Schlussfolgerung zu richten, nicht an die Person', en: 'directing criticism at the issue, method, or conclusion, not at the person', fa: 'نقد را به موضوع، روش یا نتیجه بگیری، نه به شخصیت فرد', ar: 'توجيه النقد إلى الموضوع أو المنهج أو الاستنتاج، لا إلى الشخص', tr: 'eleştiriyi kişiye değil konuya, yönteme veya sonuca yöneltmek', ru: 'направлять критику на предмет, метод или вывод, а не на человека', ckb: 'ڕەخنە ئاراستەی بابەت، میتۆد یان دەرەنجام بکەیت، نە کەسەکە', kmr: 'rexneyê ber bi mijar, metod an encamê ve bibî, ne ber bi kesê ve', pl: 'kierować krytykę do sprawy, metody lub wniosku, nie do osoby', ro: 'să îndrepți critica spre problemă, metodă sau concluzie, nu spre persoană', sq: 'ta drejtosh kritikën te çështja, metoda ose përfundimi, jo te personi' },
    grammar: 'c2-c2-debate-grammar',
    targetType: 'roleplay',
    targetSlug: 'c2-eine-fachliche-kritik-ohne-abwertung-formulieren'
  },
  {
    slug: 'c2-interdisziplinaere-perspektive-vermitteln',
    topic: { de: 'interdisziplinaere Perspektiven', en: 'interdisciplinary perspectives', fa: 'دیدگاه‌های بین‌رشته‌ای', ar: 'المنظورات متعددة التخصصات', tr: 'disiplinlerarası bakış açıları', ru: 'междисциплинарные перспективы', ckb: 'دیدگای نێوان-پسپۆڕی', kmr: 'perspektîfên navdîsîplînî', pl: 'perspektywy interdyscyplinarne', ro: 'perspective interdisciplinare', sq: 'perspektiva ndërdisiplinore' },
    focus: { de: 'Begriffe aus einem Fach so zu uebersetzen, dass ein anderes Fach sie nutzen, aber nicht missverstehen kann', en: 'translating concepts from one discipline so another can use them without misunderstanding them', fa: 'مفهوم‌های یک رشته را طوری منتقل کنی که رشته دیگر بتواند از آن‌ها استفاده کند، اما بد برداشت نکند', ar: 'نقل مفاهيم من تخصص بحيث يستطيع تخصص آخر استخدامها من دون سوء فهم', tr: 'bir disiplinin kavramlarını başka bir disiplinin kullanabileceği ama yanlış anlamayacağı şekilde aktarmak', ru: 'передавать понятия одной дисциплины так, чтобы другая могла ими пользоваться без неверного понимания', ckb: 'چەمکەکانی پسپۆڕییەک بە شێوەیەک بگوازیتەوە کە پسپۆڕییەکی تر بتوانێت بەکاریان بهێنێت بەبێ تێگەیشتنی هەڵە', kmr: 'têgehên dîsîplînekê wisa veguhêzî ku dîsîplînek din bikar bîne bêyî şaş têbigihêje', pl: 'przełożyć pojęcia jednej dziedziny tak, by inna mogła ich użyć bez błędnego rozumienia', ro: 'să traduci concepte dintr-un domeniu astfel încât alt domeniu să le poată folosi fără să le înțeleagă greșit', sq: 'të përkthesh koncepte nga një fushë që një fushë tjetër t’i përdorë pa i keqkuptuar' },
    grammar: 'c2-register-and-syntactic-choice',
    targetType: 'roleplay',
    targetSlug: 'c2-eine-interdisziplinaere-perspektive-vermitteln'
  },
  {
    slug: 'c2-literarische-und-kulturelle-deutung',
    topic: { de: 'literarische und kulturelle Deutung', en: 'literary and cultural interpretation', fa: 'تفسیر ادبی و فرهنگی', ar: 'التفسير الأدبي والثقافي', tr: 'edebi ve kültürel yorum', ru: 'литературное и культурное толкование', ckb: 'لێکدانەوەی ئەدەبی و کولتووری', kmr: 'şîroveya edebî û çandî', pl: 'interpretacja literacka i kulturowa', ro: 'interpretare literară și culturală', sq: 'interpretim letrar dhe kulturor' },
    focus: { de: 'Deutung als begruendete Lesart zu formulieren, nicht als absolute Behauptung', en: 'formulating interpretation as a justified reading, not as an absolute claim', fa: 'تفسیر را به‌عنوان یک برداشت مستدل بیان کنی، نه ادعای قطعی و مطلق', ar: 'صياغة التفسير كقراءة مبررة لا كادعاء مطلق', tr: 'yorumu mutlak iddia değil, gerekçelendirilmiş okuma olarak formüle etmek', ru: 'формулировать толкование как обоснованное прочтение, а не абсолютное утверждение', ckb: 'لێکدانەوە وەک خوێندنەوەیەکی پاساودار دەرببڕیت، نە وەک بانگەشەیەکی ڕەها', kmr: 'şîroveyê wek xwendineke bi sedem formule bikî, ne wek îdîayeke mutleq', pl: 'formułować interpretację jako uzasadnione odczytanie, nie jako absolutne twierdzenie', ro: 'să formulezi interpretarea ca lectură justificată, nu ca afirmație absolută', sq: 'ta formulosh interpretimin si lexim të arsyetuar, jo si pohim absolut' },
    grammar: 'c2-literary-sentence-structures',
    targetType: 'roleplay',
    targetSlug: 'c2-goethe-c2-eine-literarische-position-diskutieren'
  },
  {
    slug: 'c2-akademischer-diskurs-und-forschung-wiederholen',
    topic: { de: 'akademischer Diskurs und Forschung', en: 'academic discourse and research', fa: 'گفتمان دانشگاهی و پژوهش', ar: 'الخطاب الأكاديمي والبحث', tr: 'akademik söylem ve araştırma', ru: 'академический дискурс и исследование', ckb: 'گفتوگۆی ئەکادیمی و توێژینەوە', kmr: 'gotûbêja akademîk û lêkolîn', pl: 'dyskurs akademicki i badania', ro: 'discurs academic și cercetare', sq: 'diskurs akademik dhe kërkim' },
    focus: { de: 'eine persoenliche Routine fuer These, Gegenposition, Methode, Grenze und Schluss zu nutzen', en: 'using a personal routine for thesis, counterposition, method, limit, and conclusion', fa: 'یک روال شخصی برای تز، موضع مخالف، روش، محدودیت و نتیجه به کار ببری', ar: 'استخدام روتين شخصي للأطروحة والموقف المقابل والمنهج والحدود والخلاصة', tr: 'tez, karşı görüş, yöntem, sınır ve sonuç için kişisel bir rutin kullanmak', ru: 'использовать личную процедуру для тезиса, контрпозиции, метода, границы и вывода', ckb: 'ڕووتینێکی تایبەتی بۆ تێز، هەڵوێستی دژ، میتۆد، سنوور و دەرەنجام بەکاربهێنیت', kmr: 'rêbazeke xwe ji bo tez, helwesta dijber, metod, sînor û encamê bikar bînî', pl: 'stosować własną rutynę dla tezy, przeciwstanowiska, metody, granicy i wniosku', ro: 'să folosești o rutină proprie pentru teză, opoziție, metodă, limită și concluzie', sq: 'të përdorësh një rutinë personale për tezën, kundërpozicionin, metodën, kufirin dhe përfundimin' },
    grammar: 'c2-c2-grammar-review-map',
    targetType: 'exam-prep-unit',
    targetSlug: 'c2-argument-und-stil-im-schreiben-steuern'
  }
];

function translationArray(map) {
  return langs.map(language => ({ language, text: map[language] }));
}

function orientInstruction(item) {
  return {
    de: `Lies den Lektionstext zu ${item.topic.de}. Bestimme deine Rolle: moderierst du, verteidigst du, raeumst du ein, erklaerst du oder interpretierst du?`,
    en: `Read the lesson text on ${item.topic.en}. Identify your role: are you moderating, defending, conceding, explaining, or interpreting?`,
    fa: `متن درس درباره ${item.topic.fa} را بخوان. نقش خودت را مشخص کن: بحث را مدیریت می‌کنی، دفاع می‌کنی، محدودیت را می‌پذیری، توضیح می‌دهی یا تفسیر می‌کنی؟`,
    ar: `اقرأ نص الدرس عن ${item.topic.ar}. حدّد دورك: هل تدير النقاش، تدافع، تقرّ بحدّ، تشرح أم تفسّر؟`,
    tr: `${item.topic.tr} hakkındaki ders metnini oku. Rolünü belirle: moderasyon mu yapıyorsun, savunuyor musun, sınır mı kabul ediyorsun, açıklıyor musun yoksa yorumluyor musun?`,
    ru: `Прочитай урок о теме: ${item.topic.ru}. Определи свою роль: ты модерируешь, защищаешь, признаешь ограничение, объясняешь или интерпретируешь?`,
    ckb: `دەقی وانەکە دەربارەی ${item.topic.ckb} بخوێنەوە. ڕۆڵەکەت دیاری بکە: گفتوگۆ بەڕێوە دەبەیت، بەرگری دەکەیت، سنوورێک دەناسیت، ڕوون دەکەیتەوە یان لێکدەدەیتەوە؟`,
    kmr: `Nivîsa dersê derbarê ${item.topic.kmr} bixwîne. Rola xwe diyar bike: tu moderator î, berevanî dikî, sînorek qebûl dikî, rave dikî an şîrove dikî?`,
    pl: `Przeczytaj tekst lekcji o: ${item.topic.pl}. Określ swoją rolę: moderujesz, bronisz, przyznajesz ograniczenie, wyjaśniasz czy interpretujesz?`,
    ro: `Citește textul lecției despre ${item.topic.ro}. Stabilește-ți rolul: moderezi, aperi, admiți o limită, explici sau interpretezi?`,
    sq: `Lexo tekstin e mësimit për ${item.topic.sq}. Përcakto rolin tënd: moderon, mbron, pranon një kufi, shpjegon apo interpreton?`
  };
}

function languageInstruction(item) {
  return {
    de: `Oeffne den verlinkten Grammatikpunkt und sammle drei Formulierungen, die dir helfen, ${item.focus.de}.`,
    en: `Open the linked grammar point and collect three formulations that help you ${item.focus.en}.`,
    fa: `بخش گرامر لینک‌شده را باز کن و سه فرمول‌بندی پیدا کن که کمک می‌کنند ${item.focus.fa}.`,
    ar: `افتح نقطة القواعد المرتبطة واجمع ثلاث صيغ تساعدك على ${item.focus.ar}.`,
    tr: `Bağlantılı dilbilgisi bölümünü aç ve sana şunda yardımcı olan üç ifade biçimi bul: ${item.focus.tr}.`,
    ru: `Открой связанный раздел грамматики и собери три формулировки, которые помогают ${item.focus.ru}.`,
    ckb: `خاڵی ڕێزمانی بەستەرکراو بکەرەوە و سێ داڕشتن بدۆزەوە کە یارمەتیت دەدەن ${item.focus.ckb}.`,
    kmr: `Xala rêzimanê ya girêdayî veke û sê formulekirin bibîne ku alîkarin ${item.focus.kmr}.`,
    pl: `Otwórz podlinkowany punkt gramatyczny i zbierz trzy sformułowania, które pomagają ${item.focus.pl}.`,
    ro: `Deschide punctul de gramatică legat și adună trei formulări care te ajută să ${item.focus.ro}.`,
    sq: `Hap pikën e lidhur të gramatikës dhe mblidh tri formulime që të ndihmojnë ${item.focus.sq}.`
  };
}

function targetInstruction(item) {
  return {
    de: `Bearbeite die verlinkte Ressource. Pruefe danach zwei Dinge: Gelingt dir dieses Ziel - ${item.focus.de}? Und bleibst du redlich, ohne Schwaechen zu kaschieren oder Sicherheit vorzutaeuschen?`,
    en: `Work through the linked resource. Then check two things: does this goal work - ${item.focus.en}? And do you stay intellectually honest without hiding weaknesses or pretending certainty?`,
    fa: `منبع لینک‌شده را انجام بده. بعد دو چیز را بررسی کن: آیا به هدف درس می‌رسی، یعنی ${item.focus.fa}؟ و آیا همچنان منصف و دقیق می‌مانی، بدون پنهان کردن ضعف یا نشان دادن قطعیتِ غیرواقعی؟`,
    ar: `اعمل على المورد المرتبط. ثم افحص أمرين: هل تحقق هدف الدرس، أي ${item.focus.ar}؟ وهل تبقى أمينًا ودقيقًا من دون إخفاء الضعف أو ادعاء يقين غير موجود؟`,
    tr: `Bağlantılı kaynağı çalış. Sonra iki şeyi kontrol et: Dersin hedefi gerçekleşiyor mu - ${item.focus.tr}? Ve zayıflığı saklamadan ya da sahte kesinlik göstermeden dürüst kalıyor musun?`,
    ru: `Проработай связанный ресурс. Затем проверь две вещи: достигается ли цель урока — ${item.focus.ru}? И остаешься ли ты добросовестным, не скрывая слабые места и не изображая ложную уверенность?`,
    ckb: `سەرچاوەی بەستەرکراو کاربکە. پاشان دوو شت بپشکنە: ئایا دەگەیتە ئامانجی وانەکە، واتە ${item.focus.ckb}؟ و ئایا هێشتا دادپەروەر و ورد دەمێنیت بەبێ شاردنەوەی لاوازی یان پیشاندانی دڵنیایی ساختە؟`,
    kmr: `Çavkaniya girêdayî bixebitîne. Paşê du tiştan binirxîne: armanca dersê pêk tê - ${item.focus.kmr}? Û tu hîn jî rast û daqîq dimînî bêyî ku lawazî veşêrî an ewlehiya derewîn nîşan bidî?`,
    pl: `Przerób podlinkowany materiał. Potem sprawdź dwie rzeczy: czy osiągasz cel lekcji - ${item.focus.pl}? I czy pozostajesz rzetelny, bez maskowania słabości i udawania pewności?`,
    ro: `Lucrează resursa legată. Apoi verifică două lucruri: atingi scopul lecției - ${item.focus.ro}? Și rămâi onest intelectual, fără să ascunzi slăbiciuni sau să simulezi certitudine?`,
    sq: `Puno me burimin e lidhur. Pastaj kontrollo dy gjëra: a e arrin synimin e mësimit - ${item.focus.sq}? Dhe a mbetesh i ndershëm, pa fshehur dobësi apo pa shtirur siguri?`
  };
}

function transferInstruction() {
  return {
    de: 'Formuliere deine Position in drei Saetzen: These, Einwand oder Grenze, und kontrollierte Schlussfolgerung. Jeder Satz muss eine andere Funktion haben.',
    en: 'Formulate your position in three sentences: thesis, objection or limit, and controlled conclusion. Each sentence must have a different function.',
    fa: 'موضع خودت را در سه جمله بنویس: تز، ایراد یا محدودیت، و نتیجه‌گیری کنترل‌شده. هر جمله باید کارکرد متفاوتی داشته باشد.',
    ar: 'صغ موقفك في ثلاث جمل: أطروحة، اعتراض أو حد، ثم استنتاج مضبوط. يجب أن تكون لكل جملة وظيفة مختلفة.',
    tr: 'Pozisyonunu üç cümlede ifade et: tez, itiraz veya sınır, ve kontrollü sonuç. Her cümlenin farklı bir işlevi olmalı.',
    ru: 'Сформулируй свою позицию в трех предложениях: тезис, возражение или ограничение, затем контролируемый вывод. У каждого предложения должна быть своя функция.',
    ckb: 'هەڵوێستی خۆت بە سێ ڕستە بنووسە: تێز، ناڕەزایی یان سنوور، و دەرەنجامی کۆنترۆڵکراو. هەر ڕستەیەک دەبێت ئەرکی جیاوازی هەبێت.',
    kmr: 'Helwesta xwe di sê hevokan de binivîse: tez, îtiraz an sînor, û encama kontrolkirî. Her hevok divê erkeke cuda hebe.',
    pl: 'Sformułuj swoje stanowisko w trzech zdaniach: teza, zastrzeżenie lub granica oraz kontrolowany wniosek. Każde zdanie musi mieć inną funkcję.',
    ro: 'Formulează-ți poziția în trei fraze: teză, obiecție sau limită și concluzie controlată. Fiecare frază trebuie să aibă o funcție diferită.',
    sq: 'Formulo pozicionin tënd në tri fjali: tezë, kundërshtim ose kufi, dhe përfundim të kontrolluar. Çdo fjali duhet të ketë funksion tjetër.'
  };
}

function reviewInstruction() {
  return {
    de: 'Notiere deine akademische Kontrollfrage: Welche Behauptung trage ich, welchen Einwand nehme ich ernst, und welche Grenze mache ich sichtbar?',
    en: 'Write your academic control question: which claim am I carrying, which objection do I take seriously, and which limit do I make visible?',
    fa: 'پرسش کنترل دانشگاهی خودت را بنویس: کدام ادعا را پیش می‌برم، کدام ایراد را جدی می‌گیرم، و کدام محدودیت را آشکار می‌کنم؟',
    ar: 'دوّن سؤال التحكم الأكاديمي: أي ادعاء أقدّم، وأي اعتراض آخذه بجدية، وأي حد أجعله ظاهرًا؟',
    tr: 'Akademik kontrol sorunu yaz: Hangi iddiayı taşıyorum, hangi itirazı ciddiye alıyorum ve hangi sınırı görünür kılıyorum?',
    ru: 'Запиши академический контрольный вопрос: какое утверждение я несу, какое возражение принимаю всерьез и какую границу делаю видимой?',
    ckb: 'پرسیاری کۆنترۆڵی ئەکادیمیی خۆت بنووسە: کام بانگەشە دەگوازمەوە، کام ناڕەزایی بە جدی وەردەگرم، و کام سنوور دەخەمە بەرچاو؟',
    kmr: 'Pirsa kontrola akademîk a xwe binivîse: kîjan îdîayê digerînim, kîjan îtirazê cidî digirim, û kîjan sînorê xuya dikim?',
    pl: 'Zapisz akademickie pytanie kontrolne: jaką tezę niosę, jakie zastrzeżenie traktuję poważnie i jaką granicę pokazuję?',
    ro: 'Notează întrebarea ta de control academic: ce afirmație susțin, ce obiecție iau în serios și ce limită fac vizibilă?',
    sq: 'Shëno pyetjen tënde kontrolluese akademike: cilin pohim mbaj, cilin kundërshtim e marr seriozisht dhe cilin kufi e bëj të dukshëm?'
  };
}

function makeActivity(kind, titleKey, instructionMap, targetType, targetSlug, sortOrder, estimatedMinutes) {
  return {
    kind,
    title: titles[titleKey].de,
    titleTranslations: translationArray(titles[titleKey]),
    instruction: instructionMap.de,
    instructionTranslations: translationArray(instructionMap),
    targetType,
    targetSlug: targetType === 'none' ? null : targetSlug,
    estimatedMinutes,
    sortOrder,
    isRequired: true
  };
}

const bySlug = new Map(lessons.map(lesson => [lesson.slug, lesson]));
for (const item of items) {
  const lesson = bySlug.get(item.slug);
  if (!lesson) {
    throw new Error(`Lesson not found: ${item.slug}`);
  }

  lesson.activityBlocks = [
    makeActivity('read', 'orient', orientInstruction(item), 'none', null, 10, 7),
    makeActivity('grammar', 'language', languageInstruction(item), 'grammar-topic', item.grammar, 20, 9),
    makeActivity(item.targetType === 'roleplay' ? 'roleplay' : 'exam-prep', 'target', targetInstruction(item), item.targetType, item.targetSlug, 30, 10),
    makeActivity('practice', 'transfer', transferInstruction(), 'none', null, 40, 9),
    makeActivity('review', 'review', reviewInstruction(), 'none', null, 50, 5)
  ];
}

fs.writeFileSync(file, `${JSON.stringify(data, null, 2)}\n`, 'utf8');
console.log(`Updated ${items.length} C2 Module 5 lessons with ${items.length * 5} activity blocks.`);

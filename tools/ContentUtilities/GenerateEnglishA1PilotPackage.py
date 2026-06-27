import json
from pathlib import Path


ROOT = Path(__file__).resolve().parents[2]
OUT = ROOT / "content" / "learning-portal" / "english" / "pilot" / "packages" / "english-a1-platform-pilot-01-v1.json"

LANGS = ["en", "fa", "ar", "tr", "ru", "ckb", "kmr", "pl", "ro", "sq"]


def tr(items):
    return [{"language": lang, "text": items[lang]} for lang in LANGS]


def tr_list(items_by_lang):
    return [{"language": lang, "texts": items_by_lang[lang]} for lang in LANGS]


def block(text):
    return [{"type": "paragraph", "text": text}]


def localized_blocks(items):
    return {lang: block(items[lang]) for lang in LANGS}


def section_translations(heading, text):
    return [
        {"language": lang, "heading": heading[lang], "text": text[lang]}
        for lang in LANGS
    ]


def meaning_translations(items):
    return [{"language": lang, "text": items[lang]} for lang in LANGS]


def expression_meanings(actual, usage):
    return [
        {
            "language": lang,
            "actualMeaningText": actual[lang],
            "usageExplanation": usage[lang],
        }
        for lang in LANGS
    ]


def expression_example(source, translations, sort_order):
    return {
        "germanText": source,
        "sortOrder": sort_order,
        "translations": meaning_translations(translations),
    }


def text_item(text, translations, sort_order):
    return {
        "text": text,
        "translations": meaning_translations(translations),
        "sortOrder": sort_order,
    }


def course_activity(kind, title, instructions, target_type, target_slug, sort_order, minutes=5, required=True):
    return {
        "kind": kind,
        "title": title["en_source"],
        "titleTranslations": tr(title),
        "instruction": instructions["en_source"],
        "instructionTranslations": tr(instructions),
        "targetType": target_type,
        "targetSlug": target_slug,
        "estimatedMinutes": minutes,
        "sortOrder": sort_order,
        "isRequired": required,
    }


def add_source(items, source):
    result = dict(items)
    result["en_source"] = source
    return result


COMMON = {
    "read_start_title": add_source({
        "en": "Read the short start",
        "fa": "شروع کوتاه را بخوان",
        "ar": "اقرأ البداية القصيرة",
        "tr": "Kısa başlangıcı oku",
        "ru": "Прочитайте короткое начало",
        "ckb": "دەستپێکی کورت بخوێنەوە",
        "kmr": "Destpêka kurt bixwîne",
        "pl": "Przeczytaj krótki początek",
        "ro": "Citește începutul scurt",
        "sq": "Lexo hyrjen e shkurtër",
    }, "Read the short start"),
    "read_start_instruction": add_source({
        "en": "Read the lesson text once and notice the useful English words.",
        "fa": "متن درس را یک بار بخوان و به واژه‌های کاربردی انگلیسی دقت کن.",
        "ar": "اقرأ نص الدرس مرة واحدة وانتبه إلى الكلمات الإنجليزية المفيدة.",
        "tr": "Ders metnini bir kez oku ve yararlı İngilizce kelimelere dikkat et.",
        "ru": "Прочитайте текст урока один раз и обратите внимание на полезные английские слова.",
        "ckb": "دەقی وانەکە جارێک بخوێنەوە و سەرنج بدە بە وشە بەسوودەکانی ئینگلیزی.",
        "kmr": "Nivîsa dersê carekê bixwîne û balê bide peyvên îngilîzî yên bikêr.",
        "pl": "Przeczytaj tekst lekcji jeden raz i zwróć uwagę na przydatne angielskie słowa.",
        "ro": "Citește textul lecției o dată și observă cuvintele utile în engleză.",
        "sq": "Lexo tekstin e mësimit një herë dhe vëre fjalët e dobishme në anglisht.",
    }, "Read the lesson text once and notice the useful English words."),
    "review_title": add_source({
        "en": "Review in your own words",
        "fa": "با کلمات خودت مرور کن",
        "ar": "راجع بكلماتك الخاصة",
        "tr": "Kendi sözlerinle tekrar et",
        "ru": "Повторите своими словами",
        "ckb": "بە وشەی خۆت پێداچوونەوە بکە",
        "kmr": "Bi gotinên xwe dubare bike",
        "pl": "Powtórz własnymi słowami",
        "ro": "Recapitulează cu propriile cuvinte",
        "sq": "Përsërite me fjalët e tua",
    }, "Review in your own words"),
    "review_instruction": add_source({
        "en": "Say two short English sentences from this lesson without looking.",
        "fa": "بدون نگاه کردن، دو جمله کوتاه انگلیسی از این درس بگو.",
        "ar": "قل جملتين إنجليزيتين قصيرتين من هذا الدرس من دون النظر.",
        "tr": "Bakmadan bu dersten iki kısa İngilizce cümle söyle.",
        "ru": "Не подглядывая, скажите два коротких английских предложения из этого урока.",
        "ckb": "بێ ئەوەی سەیر بکەیت، دوو ڕستەی کورتی ئینگلیزی لەم وانەیە بڵێ.",
        "kmr": "Bê ku binêrî, ji vê dersê du hevokên kurt ên îngilîzî bibêje.",
        "pl": "Bez patrzenia powiedz dwa krótkie angielskie zdania z tej lekcji.",
        "ro": "Spune fără să te uiți două propoziții scurte în engleză din lecție.",
        "sq": "Thuaj pa parë dy fjali të shkurtra anglisht nga ky mësim.",
    }, "Say two short English sentences from this lesson without looking."),
}


course_path = {
    "slug": "en-a1-everyday-start",
    "title": "A1 Everyday English Start",
    "titleTranslations": tr({
        "en": "A1 everyday English start",
        "fa": "شروع انگلیسی روزمره A1",
        "ar": "بداية الإنجليزية اليومية A1",
        "tr": "A1 günlük İngilizce başlangıcı",
        "ru": "A1: начало повседневного английского",
        "ckb": "دەستپێکی ئینگلیزی ڕۆژانە A1",
        "kmr": "Destpêka îngilîziya rojane A1",
        "pl": "A1: start z codziennym angielskim",
        "ro": "A1: început în engleza de zi cu zi",
        "sq": "A1: fillimi i anglishtes së përditshme",
    }),
    "description": "A small native English pilot path for first contacts, simple sentences, questions, and everyday words.",
    "descriptionTranslations": tr({
        "en": "A small native English pilot path for first contacts, simple sentences, questions, and everyday words.",
        "fa": "یک مسیر آزمایشی کوچک و اصیل برای انگلیسی: اولین ارتباط‌ها، جمله‌های ساده، پرسش‌ها و واژه‌های روزمره.",
        "ar": "مسار تجريبي صغير ومكتوب أصلاً للإنجليزية: أول تواصل، جمل بسيطة، أسئلة وكلمات يومية.",
        "tr": "İngilizce için özgün küçük bir pilot yol: ilk temaslar, basit cümleler, sorular ve günlük kelimeler.",
        "ru": "Небольшой пилотный путь, написанный для английского: первые контакты, простые предложения, вопросы и повседневные слова.",
        "ckb": "ڕێڕەوی تاقیکارییەکی بچووک و ڕەسەن بۆ ئینگلیزی: یەکەم پەیوەندی، ڕستەی سادە، پرسیار و وشەی ڕۆژانە.",
        "kmr": "Rêyeke pilot a biçûk û resen ji bo îngilîzî: têkiliyên yekem, hevokên hêsan, pirs û peyvên rojane.",
        "pl": "Mała, oryginalna ścieżka pilotażowa dla angielskiego: pierwsze kontakty, proste zdania, pytania i codzienne słowa.",
        "ro": "Un mic parcurs pilot scris pentru engleză: primele contacte, propoziții simple, întrebări și cuvinte cotidiene.",
        "sq": "Një rrugë e vogël pilot e shkruar për anglishten: kontaktet e para, fjali të thjeshta, pyetje dhe fjalë të përditshme.",
    }),
    "cefrLevel": "A1",
    "isPublished": True,
    "sortOrder": 10,
}

course_module = {
    "slug": "en-a1-first-contacts",
    "coursePathSlug": "en-a1-everyday-start",
    "title": "First contacts",
    "titleTranslations": tr({
        "en": "First contacts",
        "fa": "اولین ارتباط‌ها",
        "ar": "أول تواصل",
        "tr": "İlk temaslar",
        "ru": "Первые контакты",
        "ckb": "یەکەم پەیوەندییەکان",
        "kmr": "Têkiliyên yekem",
        "pl": "Pierwsze kontakty",
        "ro": "Primele contacte",
        "sq": "Kontaktet e para",
    }),
    "description": "Greetings, names, simple questions, short details, and everyday objects.",
    "descriptionTranslations": tr({
        "en": "Greetings, names, simple questions, short details, and everyday objects.",
        "fa": "سلام‌کردن، نام‌ها، پرسش‌های ساده، اطلاعات کوتاه و چیزهای روزمره.",
        "ar": "التحيات، الأسماء، الأسئلة البسيطة، المعلومات القصيرة والأشياء اليومية.",
        "tr": "Selamlar, adlar, basit sorular, kısa bilgiler ve günlük eşyalar.",
        "ru": "Приветствия, имена, простые вопросы, короткие данные и повседневные предметы.",
        "ckb": "سڵاو، ناو، پرسیاری سادە، زانیاری کورت و شتە ڕۆژانەکان.",
        "kmr": "Silav, nav, pirsên hêsan, agahiyên kurt û tiştên rojane.",
        "pl": "Powitania, imiona, proste pytania, krótkie informacje i codzienne przedmioty.",
        "ro": "Saluturi, nume, întrebări simple, detalii scurte și obiecte cotidiene.",
        "sq": "Përshëndetje, emra, pyetje të thjeshta, të dhëna të shkurtra dhe sende të përditshme.",
    }),
    "moduleNumber": 1,
    "cefrLevel": "A1",
    "isPublished": True,
    "sortOrder": 10,
}


lessons_data = [
    {
        "slug": "en-a1-say-hello-and-give-your-name",
        "title": "Say hello and give your name",
        "short": "Start a very short first-contact conversation in English.",
        "narrative": "You learn how to say hello, give your name, and sound friendly without adding too much information.",
        "goals": ["Say a simple greeting.", "Give your name clearly.", "Use one polite closing word."],
        "next": "en-a1-use-i-am-and-i-have",
        "links": ["en-a1-choose-the-correct-greeting"],
        "activities": [
            ("expression", "Notice greeting phrases", "Read the greeting expressions and say each one aloud.", "expression", "hello"),
            ("practice", "Choose the right greeting", "Complete the short greeting exercise and check why the answer fits.", "exercise", "en-a1-choose-the-correct-greeting"),
        ],
    },
    {
        "slug": "en-a1-use-i-am-and-i-have",
        "title": "Use I am and I have",
        "short": "Build first personal sentences with I am and I have.",
        "narrative": "You practise two basic sentence frames: I am for identity or state, and I have for simple possession.",
        "goals": ["Use I am in a short sentence.", "Use I have for simple possession.", "Avoid dropping the subject I."],
        "prev": "en-a1-say-hello-and-give-your-name",
        "next": "en-a1-ask-simple-questions",
        "links": ["en-a1-complete-i-am-sentences"],
        "activities": [
            ("grammar", "Study pronouns with be", "Read the grammar note and focus on I am, you are, and he or she is.", "grammar-topic", "en-a1-subject-pronouns-and-be"),
            ("practice", "Complete I am sentences", "Fill the gaps with the correct short form and read the full sentences aloud.", "exercise", "en-a1-complete-i-am-sentences"),
        ],
    },
    {
        "slug": "en-a1-ask-simple-questions",
        "title": "Ask simple questions",
        "short": "Ask and answer short first-contact questions.",
        "narrative": "You learn the basic order for simple questions with be and use them in a careful first conversation.",
        "goals": ["Ask What is your name?", "Ask Where are you from?", "Answer with a short sentence."],
        "prev": "en-a1-use-i-am-and-i-have",
        "next": "en-a1-numbers-times-and-short-details",
        "links": ["en-a1-match-names-and-countries"],
        "activities": [
            ("grammar", "Connect questions and answers", "Compare the question and answer order before you practise.", "grammar-topic", "en-a1-subject-pronouns-and-be"),
            ("practice", "Match names and countries", "Match each short sentence to the correct person and country.", "exercise", "en-a1-match-names-and-countries"),
        ],
    },
    {
        "slug": "en-a1-numbers-times-and-short-details",
        "title": "Numbers, times, and short details",
        "short": "Give short practical details in a first conversation.",
        "narrative": "You use short English chunks for a phone number, a room number, a time, or a simple personal detail.",
        "goals": ["Say one number clearly.", "Give one short detail.", "Ask for repetition politely."],
        "prev": "en-a1-ask-simple-questions",
        "next": "en-a1-name-everyday-objects",
        "links": [],
        "activities": [
            ("expression", "Use polite support phrases", "Practise please, thank you, and how are you in short exchanges.", "expression", "please"),
            ("write", "Write one class question", "Use the class-question template with your own room, time, or course detail.", "writing-template", "en-a1-simple-class-question"),
        ],
    },
    {
        "slug": "en-a1-name-everyday-objects",
        "title": "Name everyday objects",
        "short": "Use a/an and plural nouns with everyday objects.",
        "narrative": "You learn how English marks one object with a or an, and how simple plural nouns are formed.",
        "goals": ["Choose a or an before a noun.", "Recognise simple plural nouns.", "Name a few everyday objects."],
        "prev": "en-a1-numbers-times-and-short-details",
        "links": ["en-a1-choose-a-or-an"],
        "activities": [
            ("grammar", "Study a, an, and plurals", "Read the grammar note and compare one object with more than one object.", "grammar-topic", "en-a1-a-an-and-plural-nouns"),
            ("practice", "Choose a or an", "Complete the article exercise and say each answer as a full phrase.", "exercise", "en-a1-choose-a-or-an"),
            ("review", "Build a short introduction", "Write three English sentences about yourself using one object or class detail.", "writing-template", "en-a1-short-self-introduction-message"),
        ],
    },
]

lesson_translations = {
    "Say hello and give your name": {
        "en": "Say hello and give your name", "fa": "سلام کن و نامت را بگو", "ar": "قل مرحباً و اذكر اسمك", "tr": "Selam ver ve adını söyle", "ru": "Поздоровайтесь и назовите имя", "ckb": "سڵاو بکە و ناوت بڵێ", "kmr": "Silav bike û navê xwe bêje", "pl": "Przywitaj się i podaj imię", "ro": "Salută și spune-ți numele", "sq": "Përshëndet dhe thuaj emrin tënd",
    },
    "Use I am and I have": {
        "en": "Use I am and I have", "fa": "از I am و I have استفاده کن", "ar": "استخدم I am و I have", "tr": "I am ve I have kullan", "ru": "Используйте I am и I have", "ckb": "I am و I have بەکاربهێنە", "kmr": "I am û I have bi kar bîne", "pl": "Użyj I am i I have", "ro": "Folosește I am și I have", "sq": "Përdor I am dhe I have",
    },
    "Ask simple questions": {
        "en": "Ask simple questions", "fa": "پرسش‌های ساده بپرس", "ar": "اطرح أسئلة بسيطة", "tr": "Basit sorular sor", "ru": "Задавайте простые вопросы", "ckb": "پرسیاری سادە بکە", "kmr": "Pirsên hêsan bipirse", "pl": "Zadawaj proste pytania", "ro": "Pune întrebări simple", "sq": "Bëj pyetje të thjeshta",
    },
    "Numbers, times, and short details": {
        "en": "Numbers, times, and short details", "fa": "عددها، ساعت‌ها و اطلاعات کوتاه", "ar": "الأرقام والأوقات والمعلومات القصيرة", "tr": "Sayılar, saatler ve kısa bilgiler", "ru": "Числа, время и короткие данные", "ckb": "ژمارە، کات و زانیاریی کورت", "kmr": "Hejmar, dem û agahiyên kurt", "pl": "Liczby, godziny i krótkie szczegóły", "ro": "Numere, ore și detalii scurte", "sq": "Numra, orare dhe të dhëna të shkurtra",
    },
    "Name everyday objects": {
        "en": "Name everyday objects", "fa": "چیزهای روزمره را نام ببر", "ar": "سمِّ الأشياء اليومية", "tr": "Günlük eşyaları adlandır", "ru": "Назовите повседневные предметы", "ckb": "ناوی شتە ڕۆژانەکان بڵێ", "kmr": "Navê tiştên rojane bêje", "pl": "Nazwij codzienne przedmioty", "ro": "Numește obiecte cotidiene", "sq": "Emërto sende të përditshme",
    },
}

short_translations = {
    "Start a very short first-contact conversation in English.": {
        "en": "Start a very short first-contact conversation in English.", "fa": "یک گفت‌وگوی خیلی کوتاه برای اولین ارتباط به انگلیسی شروع کن.", "ar": "ابدأ محادثة قصيرة جداً للتواصل الأول بالإنجليزية.", "tr": "İngilizcede çok kısa bir ilk temas konuşması başlat.", "ru": "Начните очень короткий первый разговор на английском.", "ckb": "گفتوگۆیەکی زۆر کورتی یەکەم پەیوەندی بە ئینگلیزی دەست پێ بکە.", "kmr": "Axaftineke pir kurt a têkiliya yekem bi îngilîzî dest pê bike.", "pl": "Rozpocznij bardzo krótką pierwszą rozmowę po angielsku.", "ro": "Începe o conversație foarte scurtă de prim contact în engleză.", "sq": "Fillo një bisedë shumë të shkurtër të kontaktit të parë në anglisht.",
    },
    "Build first personal sentences with I am and I have.": {
        "en": "Build first personal sentences with I am and I have.", "fa": "اولین جمله‌های شخصی را با I am و I have بساز.", "ar": "ابنِ أول جمل شخصية باستخدام I am و I have.", "tr": "I am ve I have ile ilk kişisel cümleleri kur.", "ru": "Составьте первые личные предложения с I am и I have.", "ckb": "یەکەم ڕستە کەسییەکان بە I am و I have دروست بکە.", "kmr": "Bi I am û I have hevokên kesane yên yekem ava bike.", "pl": "Zbuduj pierwsze zdania osobiste z I am i I have.", "ro": "Construiește primele propoziții personale cu I am și I have.", "sq": "Ndërto fjalitë e para personale me I am dhe I have.",
    },
    "Ask and answer short first-contact questions.": {
        "en": "Ask and answer short first-contact questions.", "fa": "پرسش‌های کوتاه اولین ارتباط را بپرس و پاسخ بده.", "ar": "اطرح أسئلة قصيرة للتواصل الأول وأجب عنها.", "tr": "İlk temas için kısa sorular sor ve cevapla.", "ru": "Задавайте и отвечайте на короткие вопросы первого контакта.", "ckb": "پرسیاری کورتی یەکەم پەیوەندی بکە و وەڵام بدە.", "kmr": "Pirsên kurt ên têkiliya yekem bipirse û bersiv bide.", "pl": "Zadawaj krótkie pytania pierwszego kontaktu i odpowiadaj na nie.", "ro": "Pune și răspunde la întrebări scurte de prim contact.", "sq": "Bëj dhe përgjigju pyetjeve të shkurtra të kontaktit të parë.",
    },
    "Give short practical details in a first conversation.": {
        "en": "Give short practical details in a first conversation.", "fa": "در یک گفت‌وگوی اول، اطلاعات کوتاه و کاربردی بده.", "ar": "قدّم معلومات قصيرة وعملية في محادثة أولى.", "tr": "İlk konuşmada kısa ve pratik bilgiler ver.", "ru": "Дайте короткие практические детали в первом разговоре.", "ckb": "لە گفتوگۆی یەکەمدا زانیاری کورتی بەسوود بدە.", "kmr": "Di axaftina yekem de agahiyên kurt û bikêr bide.", "pl": "Podaj krótkie praktyczne informacje w pierwszej rozmowie.", "ro": "Oferă detalii scurte și practice într-o primă conversație.", "sq": "Jep të dhëna të shkurtra praktike në një bisedë të parë.",
    },
    "Use a/an and plural nouns with everyday objects.": {
        "en": "Use a/an and plural nouns with everyday objects.", "fa": "با چیزهای روزمره از a/an و اسم‌های جمع استفاده کن.", "ar": "استخدم a/an وصيغ الجمع مع الأشياء اليومية.", "tr": "Günlük nesnelerle a/an ve çoğul isimler kullan.", "ru": "Используйте a/an и существительные во множественном числе с повседневными предметами.", "ckb": "لەگەڵ شتە ڕۆژانەکان a/an و ناوی کۆ بەکاربهێنە.", "kmr": "Bi tiştên rojane re a/an û navdêrên pirjimar bi kar bîne.", "pl": "Używaj a/an i liczby mnogiej rzeczowników z codziennymi przedmiotami.", "ro": "Folosește a/an și substantive la plural cu obiecte cotidiene.", "sq": "Përdor a/an dhe emrat në shumës me sendet e përditshme.",
    },
}

activity_title_map = {
    "Notice greeting phrases": add_source({"en": "Notice greeting phrases", "fa": "به عبارت‌های سلام دقت کن", "ar": "لاحظ عبارات التحية", "tr": "Selamlaşma ifadelerine dikkat et", "ru": "Обратите внимание на фразы приветствия", "ckb": "سەرنج بدە بە دەستەواژەکانی سڵاو", "kmr": "Baldar be li gotinên silavê", "pl": "Zauważ zwroty powitania", "ro": "Observă formulele de salut", "sq": "Vëre shprehjet e përshëndetjes"}, "Notice greeting phrases"),
    "Choose the right greeting": add_source({"en": "Choose the right greeting", "fa": "سلام درست را انتخاب کن", "ar": "اختر التحية المناسبة", "tr": "Doğru selamı seç", "ru": "Выберите правильное приветствие", "ckb": "سڵاوی دروست هەڵبژێرە", "kmr": "Silava rast hilbijêre", "pl": "Wybierz właściwe powitanie", "ro": "Alege salutul potrivit", "sq": "Zgjidh përshëndetjen e duhur"}, "Choose the right greeting"),
    "Study pronouns with be": add_source({"en": "Study pronouns with be", "fa": "ضمیرها را با be بررسی کن", "ar": "ادرس الضمائر مع be", "tr": "Zamirleri be ile çalış", "ru": "Изучите местоимения с be", "ckb": "جێناوەکان لەگەڵ be بخوێنە", "kmr": "Cînavan bi be re bixwîne", "pl": "Przećwicz zaimki z be", "ro": "Studiază pronumele cu be", "sq": "Mëso përemrat me be"}, "Study pronouns with be"),
    "Complete I am sentences": add_source({"en": "Complete I am sentences", "fa": "جمله‌های I am را کامل کن", "ar": "أكمل جمل I am", "tr": "I am cümlelerini tamamla", "ru": "Дополните предложения с I am", "ckb": "ڕستەکانی I am تەواو بکە", "kmr": "Hevokên I am temam bike", "pl": "Uzupełnij zdania z I am", "ro": "Completează propozițiile cu I am", "sq": "Plotëso fjalitë me I am"}, "Complete I am sentences"),
    "Connect questions and answers": add_source({"en": "Connect questions and answers", "fa": "پرسش و پاسخ‌ها را وصل کن", "ar": "اربط الأسئلة بالإجابات", "tr": "Soruları ve cevapları bağla", "ru": "Соедините вопросы и ответы", "ckb": "پرسیار و وەڵامەکان پێکەوە ببەستە", "kmr": "Pirs û bersivan girêbide", "pl": "Połącz pytania i odpowiedzi", "ro": "Leagă întrebările de răspunsuri", "sq": "Lidh pyetjet me përgjigjet"}, "Connect questions and answers"),
    "Match names and countries": add_source({"en": "Match names and countries", "fa": "نام‌ها و کشورها را تطبیق بده", "ar": "طابق الأسماء مع البلدان", "tr": "Adları ve ülkeleri eşleştir", "ru": "Сопоставьте имена и страны", "ckb": "ناو و وڵاتەکان هاوتا بکە", "kmr": "Nav û welatan hevber bike", "pl": "Dopasuj imiona i kraje", "ro": "Potrivește numele cu țările", "sq": "Përshtat emrat me vendet"}, "Match names and countries"),
    "Use polite support phrases": add_source({"en": "Use polite support phrases", "fa": "عبارت‌های کمکی مؤدبانه را به کار ببر", "ar": "استخدم عبارات مساعدة مهذبة", "tr": "Kibar yardımcı ifadeler kullan", "ru": "Используйте вежливые вспомогательные фразы", "ckb": "دەستەواژەی یارمەتیدەری ڕێزدارانە بەکاربهێنە", "kmr": "Gotinên alîkar ên bi rêz bi kar bîne", "pl": "Użyj uprzejmych zwrotów pomocniczych", "ro": "Folosește formule de sprijin politicoase", "sq": "Përdor shprehje ndihmëse të sjellshme"}, "Use polite support phrases"),
    "Write one class question": add_source({"en": "Write one class question", "fa": "یک پرسش برای کلاس بنویس", "ar": "اكتب سؤالاً واحداً للصف", "tr": "Sınıf için bir soru yaz", "ru": "Напишите один вопрос для класса", "ckb": "پرسیارێک بۆ پۆل بنووسە", "kmr": "Pirseke ji bo polê binivîse", "pl": "Napisz jedno pytanie do klasy", "ro": "Scrie o întrebare pentru curs", "sq": "Shkruaj një pyetje për klasën"}, "Write one class question"),
    "Study a, an, and plurals": add_source({"en": "Study a, an, and plurals", "fa": "a، an و جمع‌ها را بررسی کن", "ar": "ادرس a و an والجمع", "tr": "a, an ve çoğulları çalış", "ru": "Изучите a, an и множественное число", "ckb": "a و an و کۆ بخوێنە", "kmr": "a, an û pirjimar bixwîne", "pl": "Przećwicz a, an i liczbę mnogą", "ro": "Studiază a, an și pluralul", "sq": "Mëso a, an dhe shumësin"}, "Study a, an, and plurals"),
    "Choose a or an": add_source({"en": "Choose a or an", "fa": "a یا an را انتخاب کن", "ar": "اختر a أو an", "tr": "a ya da an seç", "ru": "Выберите a или an", "ckb": "a یان an هەڵبژێرە", "kmr": "a an an hilbijêre", "pl": "Wybierz a albo an", "ro": "Alege a sau an", "sq": "Zgjidh a ose an"}, "Choose a or an"),
    "Build a short introduction": add_source({"en": "Build a short introduction", "fa": "یک معرفی کوتاه بساز", "ar": "اكتب تعريفاً قصيراً بالنفس", "tr": "Kısa bir tanıtma oluştur", "ru": "Составьте короткое представление", "ckb": "خۆناسێنانی کورت دروست بکە", "kmr": "Nasandineke kurt ava bike", "pl": "Zbuduj krótkie przedstawienie się", "ro": "Construiește o prezentare scurtă", "sq": "Ndërto një prezantim të shkurtër"}, "Build a short introduction"),
}

activity_instruction_map = {
    "Read the greeting expressions and say each one aloud.": add_source({"en": "Read the greeting expressions and say each one aloud.", "fa": "عبارت‌های سلام را بخوان و هرکدام را بلند بگو.", "ar": "اقرأ عبارات التحية وقل كل واحدة بصوت واضح.", "tr": "Selamlaşma ifadelerini oku ve her birini sesli söyle.", "ru": "Прочитайте фразы приветствия и произнесите каждую вслух.", "ckb": "دەستەواژەکانی سڵاو بخوێنەوە و هەر یەکەیان بە دەنگی بڵێ.", "kmr": "Gotinên silavê bixwîne û her yekê bi dengekî eşkere bêje.", "pl": "Przeczytaj zwroty powitania i powiedz każdy na głos.", "ro": "Citește formulele de salut și spune fiecare cu voce tare.", "sq": "Lexo shprehjet e përshëndetjes dhe thuaje secilën me zë."}, "Read the greeting expressions and say each one aloud."),
    "Complete the short greeting exercise and check why the answer fits.": add_source({"en": "Complete the short greeting exercise and check why the answer fits.", "fa": "تمرین کوتاه سلام را انجام بده و بررسی کن چرا پاسخ مناسب است.", "ar": "أنجز تمرين التحية القصير وتحقق لماذا تناسب الإجابة.", "tr": "Kısa selamlaşma alıştırmasını yap ve cevabın neden uygun olduğunu kontrol et.", "ru": "Выполните короткое упражнение на приветствие и проверьте, почему ответ подходит.", "ckb": "راهێنانی کورتی سڵاو تەواو بکە و بزانە بۆچی وەڵامەکە گونجاوە.", "kmr": "Rahênana kurt a silavê temam bike û bibîne çima bersiv guncav e.", "pl": "Wykonaj krótkie ćwiczenie z powitaniem i sprawdź, dlaczego odpowiedź pasuje.", "ro": "Fă exercițiul scurt de salut și verifică de ce răspunsul se potrivește.", "sq": "Bëj ushtrimin e shkurtër të përshëndetjes dhe kontrollo pse përgjigjja përshtatet."}, "Complete the short greeting exercise and check why the answer fits."),
    "Read the grammar note and focus on I am, you are, and he or she is.": add_source({"en": "Read the grammar note and focus on I am, you are, and he or she is.", "fa": "نکته گرامری را بخوان و روی I am، you are و he/she is تمرکز کن.", "ar": "اقرأ الملاحظة النحوية وركز على I am و you are و he/she is.", "tr": "Dil bilgisi notunu oku ve I am, you are, he/she is yapılarına odaklan.", "ru": "Прочитайте грамматическую заметку и сосредоточьтесь на I am, you are и he/she is.", "ckb": "تێبینیی گرامەر بخوێنەوە و سەرنج بدە بە I am، you are و he/she is.", "kmr": "Nîşeya rêzimanê bixwîne û balê bide I am, you are û he/she is.", "pl": "Przeczytaj notatkę gramatyczną i skup się na I am, you are oraz he/she is.", "ro": "Citește nota de gramatică și concentrează-te pe I am, you are și he/she is.", "sq": "Lexo shënimin gramatikor dhe përqendrohu te I am, you are dhe he/she is."}, "Read the grammar note and focus on I am, you are, and he or she is."),
    "Fill the gaps with the correct short form and read the full sentences aloud.": add_source({"en": "Fill the gaps with the correct short form and read the full sentences aloud.", "fa": "جای خالی را با شکل کوتاه درست پر کن و جمله‌های کامل را بلند بخوان.", "ar": "املأ الفراغ بالشكل القصير الصحيح واقرأ الجمل كاملة بصوت واضح.", "tr": "Boşlukları doğru kısa biçimle doldur ve cümleleri sesli oku.", "ru": "Заполните пропуски правильной краткой формой и прочитайте предложения вслух.", "ckb": "بۆشاییەکان بە شێوەی کورتی دروست پڕ بکە و ڕستە تەواوەکان بە دەنگی بخوێنەوە.", "kmr": "Valahiyan bi forma kurt a rast dagire û hevokên tam bi dengekî eşkere bixwîne.", "pl": "Uzupełnij luki właściwą formą skróconą i przeczytaj pełne zdania na głos.", "ro": "Completează spațiile cu forma scurtă corectă și citește propozițiile întregi cu voce tare.", "sq": "Plotëso boshllëqet me formën e shkurtër të saktë dhe lexo fjalitë e plota me zë."}, "Fill the gaps with the correct short form and read the full sentences aloud."),
    "Compare the question and answer order before you practise.": add_source({"en": "Compare the question and answer order before you practise.", "fa": "قبل از تمرین، ترتیب پرسش و پاسخ را با هم مقایسه کن.", "ar": "قارن ترتيب السؤال والجواب قبل أن تتدرّب.", "tr": "Alıştırmadan önce soru ve cevap sırasını karşılaştır.", "ru": "Перед практикой сравните порядок слов в вопросе и ответе.", "ckb": "پێش راهێنان، ڕیزبەندی پرسیار و وەڵام بەراورد بکە.", "kmr": "Berî rahênanê rêza pirs û bersivê berawird bike.", "pl": "Przed ćwiczeniem porównaj szyk pytania i odpowiedzi.", "ro": "Compară ordinea întrebării și a răspunsului înainte să exersezi.", "sq": "Para ushtrimit, krahaso rendin e pyetjes dhe përgjigjes."}, "Compare the question and answer order before you practise."),
    "Match each short sentence to the correct person and country.": add_source({"en": "Match each short sentence to the correct person and country.", "fa": "هر جمله کوتاه را به شخص و کشور درست وصل کن.", "ar": "طابق كل جملة قصيرة مع الشخص والبلد الصحيحين.", "tr": "Her kısa cümleyi doğru kişi ve ülkeyle eşleştir.", "ru": "Соотнесите каждое короткое предложение с правильным человеком и страной.", "ckb": "هەر ڕستەیەکی کورت بە کەس و وڵاتی دروست ببەستەوە.", "kmr": "Her hevoka kurt bi kes û welatê rast re hevber bike.", "pl": "Dopasuj każde krótkie zdanie do właściwej osoby i kraju.", "ro": "Potrivește fiecare propoziție scurtă cu persoana și țara corecte.", "sq": "Përshtat çdo fjali të shkurtër me personin dhe vendin e saktë."}, "Match each short sentence to the correct person and country."),
    "Practise please, thank you, and how are you in short exchanges.": add_source({"en": "Practise please, thank you, and how are you in short exchanges.", "fa": "please، thank you و how are you را در گفت‌وگوهای کوتاه تمرین کن.", "ar": "تدرّب على please و thank you و how are you في تبادلات قصيرة.", "tr": "Kısa konuşmalarda please, thank you ve how are you ifadelerini çalış.", "ru": "Потренируйте please, thank you и how are you в коротких обменах.", "ckb": "please، thank you و how are you لە ئاڵوگۆڕی کورتی قسەدا راهێنان بکە.", "kmr": "Di axaftinên kurt de please, thank you û how are you hîn bike.", "pl": "Przećwicz please, thank you i how are you w krótkich wymianach.", "ro": "Exersează please, thank you și how are you în schimburi scurte.", "sq": "Ushtro please, thank you dhe how are you në shkëmbime të shkurtra."}, "Practise please, thank you, and how are you in short exchanges."),
    "Use the class-question template with your own room, time, or course detail.": add_source({"en": "Use the class-question template with your own room, time, or course detail.", "fa": "از الگوی پرسش کلاس با اتاق، زمان یا جزئیات کلاس خودت استفاده کن.", "ar": "استخدم قالب سؤال الصف مع غرفتك أو وقتك أو تفصيل دورتك.", "tr": "Sınıf sorusu şablonunu kendi oda, saat veya kurs bilginle kullan.", "ru": "Используйте шаблон вопроса о занятии со своей комнатой, временем или деталью курса.", "ckb": "قاڵبی پرسیاری پۆل بە ژوور، کات یان زانیاری کۆرسی خۆت بەکاربهێنە.", "kmr": "Şablona pirsa polê bi jûr, dem an agahiya kursa xwe bi kar bîne.", "pl": "Użyj szablonu pytania do klasy z własną salą, godziną lub informacją o kursie.", "ro": "Folosește șablonul pentru întrebarea de curs cu sala, ora sau detaliul tău.", "sq": "Përdor modelin e pyetjes së klasës me dhomën, orën ose të dhënën tënde të kursit."}, "Use the class-question template with your own room, time, or course detail."),
    "Read the grammar note and compare one object with more than one object.": add_source({"en": "Read the grammar note and compare one object with more than one object.", "fa": "نکته گرامری را بخوان و یک چیز را با چند چیز مقایسه کن.", "ar": "اقرأ الملاحظة النحوية وقارن شيئاً واحداً بأكثر من شيء.", "tr": "Dil bilgisi notunu oku ve bir nesneyi birden fazla nesneyle karşılaştır.", "ru": "Прочитайте грамматическую заметку и сравните один предмет с несколькими.", "ckb": "تێبینیی گرامەر بخوێنەوە و یەک شت لەگەڵ زیاتر لە یەک شت بەراورد بکە.", "kmr": "Nîşeya rêzimanê bixwîne û yek tişt bi zêdetir ji yek tişt re berawird bike.", "pl": "Przeczytaj notatkę gramatyczną i porównaj jeden przedmiot z kilkoma.", "ro": "Citește nota de gramatică și compară un obiect cu mai multe obiecte.", "sq": "Lexo shënimin gramatikor dhe krahaso një send me më shumë se një send."}, "Read the grammar note and compare one object with more than one object."),
    "Complete the article exercise and say each answer as a full phrase.": add_source({"en": "Complete the article exercise and say each answer as a full phrase.", "fa": "تمرین حرف تعریف را انجام بده و هر پاسخ را به صورت یک عبارت کامل بگو.", "ar": "أنجز تمرين أداة التعريف وقل كل إجابة كعبارة كاملة.", "tr": "Tanımlık alıştırmasını yap ve her cevabı tam bir ifade olarak söyle.", "ru": "Выполните упражнение на артикль и произнесите каждый ответ как полную фразу.", "ckb": "راهێنانی ئامرازەکە تەواو بکە و هەر وەڵامێک وەک دەستەواژەی تەواو بڵێ.", "kmr": "Rahênana amûrê temam bike û her bersiv wek gotineke tam bêje.", "pl": "Wykonaj ćwiczenie z przedimkiem i powiedz każdą odpowiedź jako pełną frazę.", "ro": "Fă exercițiul cu articolul și spune fiecare răspuns ca expresie completă.", "sq": "Bëj ushtrimin me artikullin dhe thuaje çdo përgjigje si shprehje të plotë."}, "Complete the article exercise and say each answer as a full phrase."),
    "Write three English sentences about yourself using one object or class detail.": add_source({"en": "Write three English sentences about yourself using one object or class detail.", "fa": "سه جمله انگلیسی درباره خودت بنویس و در آن از یک چیز یا جزئیات کلاس استفاده کن.", "ar": "اكتب ثلاث جمل إنجليزية عن نفسك مستخدماً شيئاً واحداً أو تفصيلاً من الصف.", "tr": "Kendin hakkında bir nesne ya da sınıf bilgisi kullanarak üç İngilizce cümle yaz.", "ru": "Напишите три английских предложения о себе, используя один предмет или деталь занятия.", "ckb": "سێ ڕستەی ئینگلیزی دەربارەی خۆت بنووسە و شتێک یان زانیارییەکی پۆل بەکاربهێنە.", "kmr": "Derbarê xwe de sê hevokên îngilîzî binivîse û tiştek an agahiyeke polê bi kar bîne.", "pl": "Napisz trzy angielskie zdania o sobie, używając jednego przedmiotu albo informacji z zajęć.", "ro": "Scrie trei propoziții în engleză despre tine folosind un obiect sau un detaliu de curs.", "sq": "Shkruaj tre fjali anglisht për veten duke përdorur një send ose një të dhënë klase."}, "Write three English sentences about yourself using one object or class detail."),
}


def build_lesson(item, index):
    title = lesson_translations[item["title"]]
    short = short_translations[item["short"]]
    narrative = {
        "en": item["narrative"],
        "fa": "در این درس، با جمله‌های خیلی کوتاه و کاربردی انگلیسی تمرین می‌کنی تا در موقعیت واقعی اول راه را پیدا کنی.",
        "ar": "في هذا الدرس تتدرّب بجمل إنجليزية قصيرة وعملية جداً كي تبدأ في موقف حقيقي.",
        "tr": "Bu derste gerçek bir durumda başlangıç yapabilmek için çok kısa ve kullanışlı İngilizce cümlelerle çalışırsın.",
        "ru": "В этом уроке вы тренируете очень короткие и полезные английские предложения, чтобы начать в реальной ситуации.",
        "ckb": "لەم وانەیەدا بە ڕستەی زۆر کورت و بەسوودی ئینگلیزی راهێنان دەکەیت بۆ ئەوەی لە دۆخی ڕاستەقینەدا دەست پێ بکەیت.",
        "kmr": "Di vê dersê de tu bi hevokên pir kurt û bikêr ên îngilîzî dixebitî da ku di rewşeke rastîn de dest pê bikî.",
        "pl": "W tej lekcji ćwiczysz bardzo krótkie i praktyczne zdania po angielsku, aby zacząć w realnej sytuacji.",
        "ro": "În această lecție exersezi propoziții engleze foarte scurte și utile, ca să începi într-o situație reală.",
        "sq": "Në këtë mësim ushtron fjali shumë të shkurtra dhe të dobishme në anglisht që të fillosh në një situatë reale.",
    }
    goals_by_lang = {
        "en": item["goals"],
        "fa": ["یک کار زبانی کوتاه را انجام بده.", "جمله را روشن و ساده نگه دار.", "عبارت را در موقعیت واقعی به کار ببر."],
        "ar": ["أنجز مهمة لغوية قصيرة.", "اجعل الجملة واضحة وبسيطة.", "استخدم العبارة في موقف حقيقي."],
        "tr": ["Kısa bir dil görevini yap.", "Cümleyi açık ve basit tut.", "İfadeyi gerçek bir durumda kullan."],
        "ru": ["Выполните короткую языковую задачу.", "Сохраняйте предложение ясным и простым.", "Используйте фразу в реальной ситуации."],
        "ckb": ["کارێکی زمانی کورت بکە.", "ڕستەکە ڕوون و سادە بهێڵە.", "دەستەواژەکە لە دۆخی ڕاستەقینە بەکاربهێنە."],
        "kmr": ["Erkeke zimanî ya kurt bike.", "Hevokê zelal û hêsan bihêle.", "Gotinê di rewşeke rastîn de bi kar bîne."],
        "pl": ["Wykonaj krótkie zadanie językowe.", "Zostaw zdanie jasne i proste.", "Użyj zwrotu w realnej sytuacji."],
        "ro": ["Fă o sarcină lingvistică scurtă.", "Păstrează propoziția clară și simplă.", "Folosește expresia într-o situație reală."],
        "sq": ["Bëj një detyrë të shkurtër gjuhësore.", "Mbaje fjalinë të qartë dhe të thjeshtë.", "Përdore shprehjen në një situatë reale."],
    }
    activities = [
        course_activity("read", COMMON["read_start_title"], COMMON["read_start_instruction"], "none", None, 10, 4),
    ]
    order = 20
    for kind, title_key, instruction_key, target_type, target_slug in item["activities"]:
        activities.append(course_activity(kind, activity_title_map[title_key], activity_instruction_map[instruction_key], target_type, target_slug, order, 6))
        order += 10
    activities.append(course_activity("review", COMMON["review_title"], COMMON["review_instruction"], "none", None, order, 4))
    return {
        "slug": item["slug"],
        "coursePathSlug": "en-a1-everyday-start",
        "moduleSlug": "en-a1-first-contacts",
        "lessonNumber": index,
        "title": item["title"],
        "titleTranslations": tr(title),
        "shortDescription": item["short"],
        "shortDescriptionTranslations": tr(short),
        "narrative": item["narrative"],
        "narrativeTranslations": tr(narrative),
        "cefrLevel": "A1",
        "estimatedMinutes": 22,
        "learningGoals": item["goals"],
        "learningGoalsTranslations": tr_list(goals_by_lang),
        "prerequisiteLessonSlugs": [item["prev"]] if item.get("prev") else [],
        "nextLessonSlug": item.get("next"),
        "linkedGrammarTopicSlugs": [a[4] for a in item["activities"] if a[3] == "grammar-topic"],
        "linkedWordSlugs": [],
        "linkedExpressionSlugs": [a[4] for a in item["activities"] if a[3] == "expression"],
        "linkedDialogueSlugs": [],
        "linkedTalkTopicSlugs": [],
        "linkedExerciseSetSlugs": ["en-a1-first-contact-practice"] if item["links"] else [],
        "linkedExamPrepSlugs": [],
        "activityBlocks": activities,
        "reviewSummary": "Use the lesson language once without reading the model.",
        "reviewSummaryTranslations": tr({
            "en": "Use the lesson language once without reading the model.",
            "fa": "زبان همین درس را یک بار بدون خواندن نمونه به کار ببر.",
            "ar": "استخدم لغة الدرس مرة واحدة من دون قراءة النموذج.",
            "tr": "Dersin dilini modele bakmadan bir kez kullan.",
            "ru": "Используйте язык урока один раз, не читая образец.",
            "ckb": "زمانی وانەکە جارێک بێ خوێندنەوەی نموونەکە بەکاربهێنە.",
            "kmr": "Zimanê dersê carekê bê xwendina modelê bi kar bîne.",
            "pl": "Użyj języka z lekcji raz bez czytania wzoru.",
            "ro": "Folosește limbajul lecției o dată fără să citești modelul.",
            "sq": "Përdor gjuhën e mësimit një herë pa lexuar modelin.",
        }),
        "homeworkTask": "Write or say three very short English sentences.",
        "homeworkTaskTranslations": tr({
            "en": "Write or say three very short English sentences.",
            "fa": "سه جمله خیلی کوتاه انگلیسی بنویس یا بگو.",
            "ar": "اكتب أو قل ثلاث جمل إنجليزية قصيرة جداً.",
            "tr": "Üç çok kısa İngilizce cümle yaz ya da söyle.",
            "ru": "Напишите или скажите три очень коротких английских предложения.",
            "ckb": "سێ ڕستەی زۆر کورتی ئینگلیزی بنووسە یان بڵێ.",
            "kmr": "Sê hevokên pir kurt ên îngilîzî binivîse an bêje.",
            "pl": "Napisz albo powiedz trzy bardzo krótkie zdania po angielsku.",
            "ro": "Scrie sau spune trei propoziții foarte scurte în engleză.",
            "sq": "Shkruaj ose thuaj tre fjali shumë të shkurtra në anglisht.",
        }),
        "isPublished": True,
        "sortOrder": index * 10,
    }


def grammar_topic(slug, title, short, category, sections, examples, rules, mistakes, sort_order):
    title_tr = {
        "en": title, "fa": f"نکته گرامری: {title}", "ar": f"قاعدة: {title}", "tr": f"Dil bilgisi: {title}",
        "ru": f"Грамматика: {title}", "ckb": f"گرامەر: {title}", "kmr": f"Rêziman: {title}", "pl": f"Gramatyka: {title}",
        "ro": f"Gramatică: {title}", "sq": f"Gramatikë: {title}",
    }
    short_tr = {
        "en": short,
        "fa": "این توضیح کوتاه کمک می‌کند شکل انگلیسی را در جمله‌های A1 درست به کار ببری.",
        "ar": "يساعدك هذا الشرح القصير على استخدام الشكل الإنجليزي في جمل A1 بشكل صحيح.",
        "tr": "Bu kısa açıklama, İngilizce yapıyı A1 cümlelerinde doğru kullanmana yardım eder.",
        "ru": "Это короткое объяснение помогает правильно использовать английскую форму в предложениях A1.",
        "ckb": "ئەم ڕوونکردنەوەیە یارمەتیت دەدات شێوەی ئینگلیزی لە ڕستەی A1 بە دروستی بەکاربهێنیت.",
        "kmr": "Ev ravekirina kurt alîkar dike ku tu şiklê îngilîzî di hevokên A1 de rast bi kar bînî.",
        "pl": "To krótkie wyjaśnienie pomaga poprawnie używać angielskiej formy w zdaniach A1.",
        "ro": "Această explicație scurtă te ajută să folosești corect forma engleză în propoziții A1.",
        "sq": "Ky shpjegim i shkurtër të ndihmon ta përdorësh saktë formën angleze në fjali A1.",
    }
    parsed_sections = []
    for index, (key, heading, explanation) in enumerate(sections, start=1):
        heading_tr = {lang: heading for lang in LANGS}
        text_tr = {
            "en": explanation,
            "fa": "در فارسی ممکن است جمله بدون فعل ربطی بیاید، اما در انگلیسی این بخش باید روشن باشد.",
            "ar": "قد تبدو الجملة في العربية مختلفة، لكن في الإنجليزية يجب أن يكون هذا الجزء واضحاً.",
            "tr": "Türkçede yapı farklı görünebilir, ama İngilizcede bu bölüm açık olmalıdır.",
            "ru": "В русском структура может выглядеть иначе, но в английском эта часть должна быть явной.",
            "ckb": "لە کوردیدا ڕستەکە ڕەنگە جیاواز بێت، بەڵام لە ئینگلیزیدا ئەم بەشە دەبێت ڕوون بێت.",
            "kmr": "Di kurdî de avahî dikare cuda xuya bike, lê di îngilîzî de ev beş divê zelal be.",
            "pl": "Po polsku struktura może wyglądać inaczej, ale po angielsku ta część musi być widoczna.",
            "ro": "În română structura poate arăta diferit, dar în engleză această parte trebuie să fie clară.",
            "sq": "Në shqip struktura mund të duket ndryshe, por në anglisht kjo pjesë duhet të jetë e qartë.",
        }
        parsed_sections.append({
            "sectionKey": key,
            "heading": heading,
            "explanation": explanation,
            "sortOrder": index * 10,
            "localizedBlocks": localized_blocks(text_tr),
            "translations": section_translations(heading_tr, text_tr),
        })
    return {
        "slug": slug,
        "contentRevision": 1,
        "title": title,
        "titleLocalized": title_tr,
        "shortDescription": short,
        "shortDescriptionLocalized": short_tr,
        "cefrLevel": "A1",
        "grammarCategory": category,
        "topics": [],
        "isPublished": True,
        "sortOrder": sort_order,
        "sections": parsed_sections,
        "examples": [
            expression_example(source, {
                "en": translation,
                "fa": f"نمونه: {translation}",
                "ar": f"مثال: {translation}",
                "tr": f"Örnek: {translation}",
                "ru": f"Пример: {translation}",
                "ckb": f"نموونە: {translation}",
                "kmr": f"Mînak: {translation}",
                "pl": f"Przykład: {translation}",
                "ro": f"Exemplu: {translation}",
                "sq": f"Shembull: {translation}",
            }, idx * 10)
            for idx, (source, translation) in enumerate(examples, start=1)
        ],
        "ruleSummaries": [
            text_item(rule, {
                "en": rule,
                "fa": "قاعده را به صورت یک الگوی کوتاه حفظ کن و بعد با جمله خودت تمرین کن.",
                "ar": "احفظ القاعدة كنمط قصير ثم تدرّب بجملة من عندك.",
                "tr": "Kuralı kısa bir kalıp olarak öğren ve sonra kendi cümlenle çalış.",
                "ru": "Запомните правило как короткую модель, затем потренируйтесь со своим предложением.",
                "ckb": "یاساکە وەک شێوازێکی کورت فێربە و پاشان بە ڕستەی خۆت راهێنان بکە.",
                "kmr": "Rêgezê wek şablonekî kurt hîn bibe û paşê bi hevoka xwe bixebite.",
                "pl": "Zapamiętaj regułę jako krótki wzór, a potem ćwicz własnym zdaniem.",
                "ro": "Ține regula ca un tipar scurt și apoi exersează cu propoziția ta.",
                "sq": "Mbaje rregullin si një model të shkurtër dhe pastaj ushtro me fjalinë tënde.",
            }, (idx + 1) * 10)
            for idx, rule in enumerate(rules)
        ],
        "commonMistakes": [
            {
                "wrongText": wrong,
                "correctedText": correct,
                "explanation": explanation,
                "translations": meaning_translations({
                    "en": explanation,
                    "fa": "اشتباه به این دلیل مهم است که در انگلیسی جای این کلمه یا شکل فعل باید دقیق باشد.",
                    "ar": "هذا الخطأ مهم لأن مكان الكلمة أو شكل الفعل في الإنجليزية يجب أن يكون دقيقاً.",
                    "tr": "Bu hata önemlidir çünkü İngilizcede kelimenin yeri veya fiil biçimi doğru olmalıdır.",
                    "ru": "Эта ошибка важна, потому что в английском место слова или форма глагола должны быть точными.",
                    "ckb": "ئەم هەڵەیە گرنگە چونکە لە ئینگلیزیدا شوێنی وشە یان شێوەی کردار دەبێت ورد بێت.",
                    "kmr": "Ev xeletî girîng e, ji ber ku di îngilîzî de cihê peyvê an forma lêkerê divê rast be.",
                    "pl": "Ten błąd jest ważny, bo w angielskim miejsce słowa lub forma czasownika musi być dokładna.",
                    "ro": "Greșeala este importantă deoarece în engleză locul cuvântului sau forma verbului trebuie să fie precisă.",
                    "sq": "Ky gabim është i rëndësishëm sepse në anglisht vendi i fjalës ose forma e foljes duhet të jetë e saktë.",
                }),
                "sortOrder": 10,
            }
            for wrong, correct, explanation in mistakes
        ],
        "exceptionNotes": [],
        "prerequisiteSlugs": [],
        "relatedTopicSlugs": [],
        "linkedWords": [],
        "linkedDialogueSlugs": [],
        "linkedTalkTopicSlugs": [],
        "linkedExerciseSlugs": [],
    }


grammar_topics = [
    grammar_topic(
        "en-a1-subject-pronouns-and-be",
        "Subject pronouns with be",
        "Use I, you, he, she, it, we, and they with the correct form of be.",
        "pronouns",
        [
            ("what-it-does", "What this structure does", "English usually needs a subject pronoun before be: I am, you are, she is."),
            ("short-forms", "Short forms", "In everyday English, I am often becomes I'm, and you are often becomes you're."),
        ],
        [("I am Sara.", "I am Sara."), ("You are in class.", "You are in class.")],
        ["Use I am, you are, he is, she is, it is, we are, they are.", "Do not drop the subject pronoun in a normal A1 sentence."],
        [("Am Sara.", "I am Sara.", "English needs the subject I before am.")],
        10,
    ),
    grammar_topic(
        "en-a1-have-and-simple-possession",
        "Have and simple possession",
        "Use have for simple things, people, and practical details.",
        "verbs",
        [
            ("basic-use", "Basic use", "Use I have for simple possession: I have a book, I have a phone number."),
            ("third-person", "He and she", "At A1, notice the change: I have, but he has and she has."),
        ],
        [("I have a notebook.", "I have a notebook."), ("She has a pen.", "She has a pen.")],
        ["Use have after I, you, we, and they.", "Use has after he, she, and it."],
        [("She have a pen.", "She has a pen.", "After she, English uses has.")],
        20,
    ),
    grammar_topic(
        "en-a1-a-an-and-plural-nouns",
        "A, an, and plural nouns",
        "Choose a or an before one noun and add plural endings for more than one.",
        "articles",
        [
            ("one-object", "One object", "Use a before a consonant sound and an before a vowel sound: a book, an apple."),
            ("more-than-one", "More than one", "Many basic plural nouns add -s: books, pens, rooms."),
        ],
        [("a book", "a book"), ("an apple", "an apple"), ("two pens", "two pens")],
        ["Use a/an only with one countable noun.", "Use plural -s for many common A1 nouns."],
        [("a apple", "an apple", "Use an before the vowel sound in apple.")],
        30,
    ),
]


expression_items = [
    ("hello", "Hello.", "Hello.", "Use this neutral greeting at the start of a simple conversation.", ["Hello, Anna.", "Hello, how are you?"]),
    ("good-morning", "Good morning.", "Good morning.", "Use this greeting in the morning.", ["Good morning, Mr Brown.", "Good morning, everyone."]),
    ("nice-to-meet-you", "Nice to meet you.", "Nice to meet you.", "Use this polite phrase when you meet someone for the first time.", ["Nice to meet you, Sara.", "Hello, nice to meet you."]),
    ("my-name-is", "My name is ...", "My name is ...", "Use this phrase to give your name clearly.", ["My name is Sara.", "Hello, my name is Tom."]),
    ("i-am-from", "I am from ...", "I am from ...", "Use this phrase to say your country or city.", ["I am from Iran.", "I am from Warsaw."]),
    ("i-live-in", "I live in ...", "I live in ...", "Use this phrase to say where you live now.", ["I live in Berlin.", "I live in a small town."]),
    ("how-are-you", "How are you?", "How are you?", "Use this common friendly question after a greeting.", ["Hello, how are you?", "Good morning. How are you?"]),
    ("i-am-fine", "I am fine.", "I am fine.", "Use this simple answer when someone asks how you are.", ["I am fine, thank you.", "I am fine today."]),
    ("please", "Please.", "Please.", "Use please to make a request sound polite.", ["One coffee, please.", "Repeat, please."]),
    ("thank-you", "Thank you.", "Thank you.", "Use this phrase to say thanks in everyday situations.", ["Thank you, Anna.", "Thank you for your help."]),
]


def expression_entry(idx, slug, text, meaning, usage, examples):
    actual = {
        "en": meaning, "fa": f"معنی: {meaning}", "ar": f"المعنى: {meaning}", "tr": f"Anlamı: {meaning}",
        "ru": f"Значение: {meaning}", "ckb": f"واتا: {meaning}", "kmr": f"Wate: {meaning}", "pl": f"Znaczenie: {meaning}",
        "ro": f"Sens: {meaning}", "sq": f"Kuptimi: {meaning}",
    }
    usage_tr = {
        "en": usage,
        "fa": "این عبارت را در موقعیت‌های ساده و واقعی A1 به کار ببر؛ کوتاه، روشن و مودبانه است.",
        "ar": "استخدم هذه العبارة في مواقف A1 البسيطة والحقيقية؛ فهي قصيرة وواضحة ومهذبة.",
        "tr": "Bu ifadeyi basit ve gerçek A1 durumlarında kullan; kısa, açık ve kibardır.",
        "ru": "Используйте эту фразу в простых реальных ситуациях A1; она короткая, ясная и вежливая.",
        "ckb": "ئەم دەستەواژەیە لە دۆخە سادە و ڕاستەقینەکانی A1 بەکاربهێنە؛ کورت، ڕوون و ڕێزدارانەیە.",
        "kmr": "Vê gotinê di rewşên hêsan û rastîn ên A1 de bi kar bîne; kurt, zelal û bi rêz e.",
        "pl": "Użyj tego zwrotu w prostych realnych sytuacjach A1; jest krótki, jasny i uprzejmy.",
        "ro": "Folosește expresia în situații A1 simple și reale; este scurtă, clară și politicoasă.",
        "sq": "Përdore këtë shprehje në situata të thjeshta reale A1; është e shkurtër, e qartë dhe e sjellshme.",
    }
    return {
        "slug": slug,
        "expressionText": text,
        "actualMeaningText": meaning,
        "usageExplanation": usage,
        "cefrLevel": "A1",
        "expressionType": "fixed-expression",
        "register": "neutral",
        "category": "social-interaction",
        "region": "global",
        "topics": [],
        "isPublished": True,
        "sortOrder": idx * 10,
        "meaningTransparency": "literal-fixed-formula",
        "teachingReason": "This is a high-frequency A1 expression for first-contact English.",
        "safetyRating": "general",
        "minimumAge": 0,
        "sensitiveContentKind": "none",
        "usagePolicy": "safe-to-use",
        "meanings": expression_meanings(actual, usage_tr),
        "examples": [
            expression_example(examples[0], {lang: examples[0] if lang == "en" else f"نمونه: {examples[0]}" for lang in LANGS}, 10),
            expression_example(examples[1], {lang: examples[1] if lang == "en" else f"نمونه: {examples[1]}" for lang in LANGS}, 20),
        ],
        "warnings": [],
        "linkedWords": [],
        "relatedExpressionSlugs": [],
        "linkedExerciseSlugs": [],
    }


expressions = [expression_entry(i, *item) for i, item in enumerate(expression_items, start=1)]


def exercise(slug, title, instruction, prompt, answer_key, correct, incorrect, hint, owner_type, owner_slug, sort_order, exercise_type="multiple-choice", target_skill="speaking"):
    generic = {
        "en": title, "fa": f"تمرین: {title}", "ar": f"تمرين: {title}", "tr": f"Alıştırma: {title}", "ru": f"Упражнение: {title}",
        "ckb": f"راهێنان: {title}", "kmr": f"Rahênan: {title}", "pl": f"Ćwiczenie: {title}", "ro": f"Exercițiu: {title}", "sq": f"Ushtrim: {title}",
    }
    instr = {
        "en": instruction, "fa": "پاسخ مناسب را انتخاب کن و بعد دلیل آن را بخوان.", "ar": "اختر الإجابة المناسبة ثم اقرأ السبب.", "tr": "Uygun cevabı seç ve sonra nedenini oku.", "ru": "Выберите подходящий ответ, затем прочитайте объяснение.", "ckb": "وەڵامی گونجاو هەڵبژێرە و پاشان هۆکارەکە بخوێنەوە.", "kmr": "Bersiva guncav hilbijêre û paşê sedemê bixwîne.", "pl": "Wybierz odpowiedź, a potem przeczytaj powód.", "ro": "Alege răspunsul potrivit și apoi citește explicația.", "sq": "Zgjidh përgjigjen e përshtatshme dhe pastaj lexo arsyen.",
    }
    corr = {lang: correct if lang == "en" else "پاسخ درست است، چون با معنی و ساختار جمله هماهنگ است." for lang in LANGS}
    inc = {lang: incorrect if lang == "en" else "به معنی جمله و شکل درست انگلیسی دوباره دقت کن." for lang in LANGS}
    hint_tr = {lang: hint if lang == "en" else "به کلمه‌های اطراف جای خالی دقت کن." for lang in LANGS}
    return {
        "slug": slug,
        "title": title,
        "titleTranslations": tr(generic),
        "instruction": instruction,
        "instructionTranslations": tr(instr),
        "cefrLevel": "A1",
        "exerciseType": exercise_type,
        "targetSkill": target_skill,
        "ownerType": owner_type,
        "ownerSlug": owner_slug,
        "prompt": prompt,
        "answerKey": answer_key,
        "correctExplanation": correct,
        "correctExplanationTranslations": tr(corr),
        "incorrectExplanation": incorrect,
        "incorrectExplanationTranslations": tr(inc),
        "hint": hint,
        "hintTranslations": tr(hint_tr),
        "isPublished": True,
        "sortOrder": sort_order,
    }


exercises = [
    exercise(
        "en-a1-choose-the-correct-greeting",
        "Choose the correct greeting",
        "Choose the greeting that fits the situation.",
        {"stem": "It is 9 a.m. What do you say?", "options": [{"id": "good-morning", "text": "Good morning."}, {"id": "good-night", "text": "Good night."}, {"id": "see-you", "text": "See you later."}]},
        {"correctOptionIds": ["good-morning"]},
        "In the morning, Good morning is the natural greeting.",
        "The other options do not fit a morning greeting.",
        "Look at the time.",
        "expression",
        "good-morning",
        10,
    ),
    exercise(
        "en-a1-complete-i-am-sentences",
        "Complete I am sentences",
        "Choose the correct form for the gap.",
        {"stem": "___ Sara.", "options": [{"id": "i-am", "text": "I am"}, {"id": "you-are", "text": "You are"}, {"id": "she-is", "text": "She is"}]},
        {"correctOptionIds": ["i-am"]},
        "I am Sara is the correct sentence when you speak about yourself.",
        "Use I am when the speaker gives their own name.",
        "Who is speaking?",
        "grammar-topic",
        "en-a1-subject-pronouns-and-be",
        20,
        target_skill="grammar",
    ),
    exercise(
        "en-a1-match-names-and-countries",
        "Match names and countries",
        "Match each person to the correct country sentence.",
        {"pairs": [{"left": "Sara", "right": "I am from Iran."}, {"left": "Tom", "right": "I am from Canada."}]},
        {"pairs": [{"left": "Sara", "right": "I am from Iran."}, {"left": "Tom", "right": "I am from Canada."}]},
        "The names match the country sentences.",
        "Read the country phrase after I am from.",
        "Look for the country.",
        "course-lesson",
        "en-a1-ask-simple-questions",
        30,
        exercise_type="matching",
        target_skill="reading",
    ),
    exercise(
        "en-a1-choose-a-or-an",
        "Choose a or an",
        "Choose a or an before the noun.",
        {"stem": "___ apple", "options": [{"id": "a", "text": "a"}, {"id": "an", "text": "an"}]},
        {"correctOptionIds": ["an"]},
        "Use an before the vowel sound in apple.",
        "A apple is not the natural English form.",
        "Listen to the first sound of apple.",
        "grammar-topic",
        "en-a1-a-an-and-plural-nouns",
        40,
        target_skill="grammar",
    ),
]

exercise_sets = [
    {
        "slug": "en-a1-first-contact-practice",
        "title": "First contact practice",
        "titleTranslations": tr({
            "en": "First contact practice", "fa": "تمرین اولین ارتباط", "ar": "تدريب التواصل الأول", "tr": "İlk temas alıştırması", "ru": "Практика первого контакта", "ckb": "راهێنانی یەکەم پەیوەندی", "kmr": "Rahênana têkiliya yekem", "pl": "Ćwiczenie pierwszego kontaktu", "ro": "Exercițiu de prim contact", "sq": "Ushtrim për kontaktin e parë",
        }),
        "description": "A short set for greetings, I am sentences, countries, and a/an.",
        "descriptionTranslations": tr({
            "en": "A short set for greetings, I am sentences, countries, and a/an.",
            "fa": "یک مجموعه کوتاه برای سلام، جمله‌های I am، کشورها و a/an.",
            "ar": "مجموعة قصيرة للتحيات وجمل I am والبلدان و a/an.",
            "tr": "Selamlar, I am cümleleri, ülkeler ve a/an için kısa bir set.",
            "ru": "Короткий набор для приветствий, предложений I am, стран и a/an.",
            "ckb": "کۆمەڵەیەکی کورت بۆ سڵاو، ڕستەکانی I am، وڵات و a/an.",
            "kmr": "Komeke kurt ji bo silav, hevokên I am, welat û a/an.",
            "pl": "Krótki zestaw na powitania, zdania I am, kraje i a/an.",
            "ro": "Un set scurt pentru saluturi, propoziții cu I am, țări și a/an.",
            "sq": "Një grup i shkurtër për përshëndetje, fjali me I am, vende dhe a/an.",
        }),
        "cefrLevel": "A1",
        "ownerType": "course-lesson",
        "ownerSlug": "en-a1-say-hello-and-give-your-name",
        "exerciseSlugs": [item["slug"] for item in exercises],
        "isPublished": True,
        "sortOrder": 10,
    }
]


def writing_template(slug, title, short, situation, template, explanation, variables, sample, sort_order):
    translations = {
        "title": {"en": title, "fa": f"الگو: {title}", "ar": f"قالب: {title}", "tr": f"Şablon: {title}", "ru": f"Шаблон: {title}", "ckb": f"قاڵب: {title}", "kmr": f"Şablon: {title}", "pl": f"Szablon: {title}", "ro": f"Șablon: {title}", "sq": f"Model: {title}"},
        "short": {"en": short, "fa": "یک متن کوتاه انگلیسی برای موقعیت ساده A1.", "ar": "نص إنجليزي قصير لموقف بسيط في A1.", "tr": "Basit bir A1 durumu için kısa İngilizce metin.", "ru": "Короткий английский текст для простой ситуации A1.", "ckb": "دەقێکی کورتی ئینگلیزی بۆ دۆخێکی سادەی A1.", "kmr": "Nivîseke kurt a îngilîzî ji bo rewşeke hêsan a A1.", "pl": "Krótki tekst po angielsku do prostej sytuacji A1.", "ro": "Un text scurt în engleză pentru o situație simplă A1.", "sq": "Një tekst i shkurtër anglisht për një situatë të thjeshtë A1."},
        "situation": {"en": situation, "fa": "می‌خواهی یک پیام کوتاه، روشن و مودبانه به انگلیسی بنویسی.", "ar": "تريد أن تكتب رسالة قصيرة وواضحة ومهذبة بالإنجليزية.", "tr": "İngilizce kısa, açık ve kibar bir mesaj yazmak istiyorsun.", "ru": "Вы хотите написать короткое, ясное и вежливое сообщение на английском.", "ckb": "دەتەوێت نامەیەکی کورت، ڕوون و ڕێزدارانە بە ئینگلیزی بنووسیت.", "kmr": "Tu dixwazî bi îngilîzî peyameke kurt, zelal û bi rêz binivîsî.", "pl": "Chcesz napisać krótką, jasną i uprzejmą wiadomość po angielsku.", "ro": "Vrei să scrii un mesaj scurt, clar și politicos în engleză.", "sq": "Dëshiron të shkruash një mesazh të shkurtër, të qartë dhe të sjellshëm në anglisht."},
        "template": {"en": template, "fa": "ترجمه راهنما: همین ساختار را با اطلاعات خودت کامل کن.", "ar": "ترجمة مساعدة: أكمل هذا الشكل بمعلوماتك.", "tr": "Yardım çevirisi: Bu yapıyı kendi bilgilerinle tamamla.", "ru": "Помощь: заполните эту структуру своими данными.", "ckb": "وەرگێڕانی یارمەتی: ئەم پێکهاتەیە بە زانیاری خۆت پڕ بکە.", "kmr": "Wergera alîkar: Vê avahiyê bi agahiyên xwe dagire.", "pl": "Tłumaczenie pomocnicze: uzupełnij tę strukturę swoimi informacjami.", "ro": "Traducere de sprijin: completează structura cu datele tale.", "sq": "Përkthim ndihmës: plotëso këtë strukturë me të dhënat e tua."},
        "explanation": {"en": explanation, "fa": "متن کوتاه است و فقط اطلاعات ضروری را می‌گوید؛ برای A1 همین کافی و طبیعی است.", "ar": "النص قصير ويذكر المعلومات الضرورية فقط؛ وهذا كافٍ وطبيعي في A1.", "tr": "Metin kısadır ve sadece gerekli bilgileri verir; A1 için bu yeterli ve doğaldır.", "ru": "Текст короткий и сообщает только нужную информацию; для A1 это достаточно и естественно.", "ckb": "دەقەکە کورتە و تەنها زانیاریی پێویست دەڵێت؛ بۆ A1 ئەمە بەس و سروشتییە.", "kmr": "Nivîs kurt e û tenê agahiyên pêwîst dibêje; ji bo A1 ev bes û xwezayî ye.", "pl": "Tekst jest krótki i podaje tylko potrzebne informacje; na A1 to wystarcza i brzmi naturalnie.", "ro": "Textul este scurt și spune doar informațiile necesare; pentru A1 este suficient și natural.", "sq": "Teksti është i shkurtër dhe jep vetëm informacionin e nevojshëm; për A1 kjo mjafton dhe tingëllon natyrshëm."},
        "sample": {"en": sample, "fa": "نمونه کامل‌شده را بخوان و بعد با نام و اطلاعات خودت دوباره بنویس.", "ar": "اقرأ المثال المكتمل ثم اكتبه مرة أخرى باسمك ومعلوماتك.", "tr": "Doldurulmuş örneği oku ve sonra kendi adın ve bilgilerinle tekrar yaz.", "ru": "Прочитайте заполненный пример, затем напишите его со своим именем и данными.", "ckb": "نموونەی پڕکراوە بخوێنەوە و پاشان بە ناو و زانیاری خۆت دووبارە بنووسە.", "kmr": "Mînaka dagirtî bixwîne û paşê bi nav û agahiyên xwe ji nû ve binivîse.", "pl": "Przeczytaj wypełniony przykład, a potem napisz go z własnym imieniem i informacjami.", "ro": "Citește exemplul completat și apoi rescrie-l cu numele și datele tale.", "sq": "Lexo shembullin e plotësuar dhe pastaj shkruaje me emrin dhe të dhënat e tua."},
    }
    return {
        "slug": slug,
        "title": title,
        "titleTranslations": tr(translations["title"]),
        "shortDescription": short,
        "shortDescriptionTranslations": tr(translations["short"]),
        "cefrLevel": "A1",
        "category": "email-to-school",
        "situation": situation,
        "situationTranslations": tr(translations["situation"]),
        "register": "neutral",
        "templateText": template,
        "templateTextTranslations": tr(translations["template"]),
        "explanation": explanation,
        "explanationTranslations": tr(translations["explanation"]),
        "replaceableVariables": variables,
        "sampleFilledVersion": sample,
        "sampleFilledVersionTranslations": tr(translations["sample"]),
        "linkedGrammarTopicSlugs": ["en-a1-subject-pronouns-and-be"],
        "linkedWordSlugs": [],
        "linkedExpressionSlugs": ["hello", "thank-you"],
        "linkedExerciseSlugs": [],
        "linkedCourseLessonSlugs": ["en-a1-say-hello-and-give-your-name"],
        "isPublished": True,
        "sortOrder": sort_order,
    }


writing_templates = [
    writing_template(
        "en-a1-short-self-introduction-message",
        "Short self-introduction message",
        "A short message to introduce yourself in a class or group.",
        "You are new in an English class and write a short message to the group.",
        "Hello,\nmy name is {{name}}. I am from {{country}}. I live in {{city}}.\nThank you,\n{{name}}",
        "The message gives only the most important information: name, country, and city.",
        ["name", "country", "city"],
        "Hello,\nmy name is Sara. I am from Iran. I live in Hamburg.\nThank you,\nSara",
        10,
    ),
    writing_template(
        "en-a1-simple-class-question",
        "Simple class question",
        "A short message to ask one practical class question.",
        "You need one simple piece of information about your English class.",
        "Hello,\nwhat time is the class on {{day}}? Is it in room {{room}}?\nThank you,\n{{name}}",
        "The question is short, polite, and practical. It asks only for the information you need.",
        ["day", "room", "name"],
        "Hello,\nwhat time is the class on Monday? Is it in room 12?\nThank you,\nSara",
        20,
    ),
]


package = {
    "packageVersion": "1.0",
    "packageId": "english-a1-platform-pilot-01-v1",
    "packageName": "English A1 Platform Pilot 01",
    "targetLearningLanguageCode": "en",
    "levelSystemCode": "cefr",
    "source": "official-reviewed-english-pilot",
    "defaultMeaningLanguages": LANGS,
    "entries": [],
    "coursePaths": [course_path],
    "courseModules": [course_module],
    "courseLessons": [build_lesson(item, index) for index, item in enumerate(lessons_data, start=1)],
    "grammarTopics": grammar_topics,
    "expressionEntries": expressions,
    "exercises": exercises,
    "exerciseSets": exercise_sets,
    "writingTemplates": writing_templates,
}

OUT.parent.mkdir(parents=True, exist_ok=True)
OUT.write_text(json.dumps(package, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
print(OUT)

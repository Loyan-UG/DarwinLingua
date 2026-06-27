import json
import re
from pathlib import Path


ROOT = Path(__file__).resolve().parents[2]
PACKAGE = ROOT / "content" / "learning-portal" / "english" / "pilot" / "packages" / "english-a1-platform-pilot-01-v1.json"

LANGS = ["en", "fa", "ar", "tr", "ru", "ckb", "kmr", "pl", "ro", "sq"]


def tr(values):
    return [{"language": lang, "text": values[lang]} for lang in LANGS]


def tr_list(values):
    return [{"language": lang, "texts": values[lang]} for lang in LANGS]


def blocks(values):
    return {lang: [{"type": "paragraph", "text": values[lang]}] for lang in LANGS}


def section_translations(heading, text):
    return [{"language": lang, "heading": heading[lang], "text": text[lang]} for lang in LANGS]


def meaning_translations(values):
    return [{"language": lang, "text": values[lang]} for lang in LANGS]


def expression_meanings(actual, usage):
    return [{"language": lang, "actualMeaningText": actual[lang], "usageExplanation": usage[lang]} for lang in LANGS]


def localized_examples(source_examples):
    label = {
        "en": "Example",
        "fa": "نمونه",
        "ar": "مثال",
        "tr": "Örnek",
        "ru": "Пример",
        "ckb": "نموونە",
        "kmr": "Mînak",
        "pl": "Przykład",
        "ro": "Exemplu",
        "sq": "Shembull",
    }
    result = []
    for index, example in enumerate(source_examples, start=1):
        result.append(
            {
                "germanText": example,
                "sortOrder": index * 10,
                "translations": meaning_translations(
                    {lang: example if lang == "en" else f"{label[lang]}: {example}" for lang in LANGS}
                ),
            }
        )
    return result


def upsert(items, item):
    for index, existing in enumerate(items):
        if existing.get("slug") == item["slug"]:
            items[index] = item
            return
    items.append(item)


def t(en, fa, ar, tr_, ru, ckb, kmr, pl, ro, sq):
    return {
        "en": en,
        "fa": fa,
        "ar": ar,
        "tr": tr_,
        "ru": ru,
        "ckb": ckb,
        "kmr": kmr,
        "pl": pl,
        "ro": ro,
        "sq": sq,
    }


READ_TITLE = t(
    "Read the short start",
    "شروع کوتاه را بخوان",
    "اقرأ البداية القصيرة",
    "Kısa başlangıcı oku",
    "Прочитайте короткое начало",
    "دەستپێکی کورت بخوێنەوە",
    "Destpêka kurt bixwîne",
    "Przeczytaj krótki początek",
    "Citește începutul scurt",
    "Lexo hyrjen e shkurtër",
)

READ_INSTRUCTION = t(
    "Read the lesson text once and notice the useful English pattern.",
    "متن درس را یک بار بخوان و به الگوی کاربردی انگلیسی دقت کن.",
    "اقرأ نص الدرس مرة واحدة وانتبه إلى النمط الإنجليزي المفيد.",
    "Ders metnini bir kez oku ve yararlı İngilizce kalıba dikkat et.",
    "Прочитайте текст урока один раз и обратите внимание на полезную английскую модель.",
    "دەقی وانەکە جارێک بخوێنەوە و سەرنج بدە بە شێوازی بەسوودی ئینگلیزی.",
    "Nivîsa dersê carekê bixwîne û balê bide qalibê îngilîzî yê bikêr.",
    "Przeczytaj tekst lekcji jeden raz i zwróć uwagę na przydatny wzór angielski.",
    "Citește textul lecției o dată și observă tiparul util în engleză.",
    "Lexo tekstin e mësimit një herë dhe vëre modelin e dobishëm në anglisht.",
)

REVIEW_TITLE = t(
    "Review in a real mini-situation",
    "در یک موقعیت کوچک واقعی مرور کن",
    "راجع في موقف صغير واقعي",
    "Gerçek küçük bir durumda tekrar et",
    "Повторите в небольшой реальной ситуации",
    "لە دۆخێکی بچووکی ڕاستەقینەدا پێداچوونەوە بکە",
    "Di rewşeke piçûk a rastîn de dubare bike",
    "Powtórz w małej realnej sytuacji",
    "Recapitulează într-o situație mică reală",
    "Përsërite në një situatë të vogël reale",
)

REVIEW_INSTRUCTION = t(
    "Use two short sentences from this lesson without looking at the model.",
    "بدون نگاه کردن به نمونه، دو جمله کوتاه از همین درس به کار ببر.",
    "استخدم جملتين قصيرتين من هذا الدرس من دون النظر إلى النموذج.",
    "Modele bakmadan bu dersten iki kısa cümle kullan.",
    "Используйте два коротких предложения из этого урока, не глядя на образец.",
    "بێ سەیرکردنی نموونەکە، دوو ڕستەی کورت لەم وانەیە بەکاربهێنە.",
    "Bê ku li modelê binêrî, ji vê dersê du hevokên kurt bi kar bîne.",
    "Użyj dwóch krótkich zdań z tej lekcji bez patrzenia na wzór.",
    "Folosește două propoziții scurte din lecție fără să te uiți la model.",
    "Përdor dy fjali të shkurtra nga ky mësim pa parë modelin.",
)

GENERIC_GOALS = {
    "en": ["Notice the English pattern.", "Use it in a short sentence.", "Check the meaning before you speak or write."],
    "fa": ["الگوی انگلیسی را تشخیص بده.", "آن را در یک جمله کوتاه به کار ببر.", "قبل از گفتن یا نوشتن، معنی را بررسی کن."],
    "ar": ["تعرّف إلى النمط الإنجليزي.", "استخدمه في جملة قصيرة.", "تحقق من المعنى قبل أن تتكلم أو تكتب."],
    "tr": ["İngilizce kalıbı fark et.", "Onu kısa bir cümlede kullan.", "Konuşmadan veya yazmadan önce anlamı kontrol et."],
    "ru": ["Заметьте английскую модель.", "Используйте ее в коротком предложении.", "Проверьте смысл перед устной или письменной фразой."],
    "ckb": ["شێوازی ئینگلیزی بناسە.", "لە ڕستەیەکی کورتدا بەکاری بهێنە.", "پێش وتن یان نووسین، واتاکە بپشکنە."],
    "kmr": ["Qalibê îngilîzî bibîne.", "Wî di hevokeke kurt de bi kar bîne.", "Berî gotin an nivîsandinê wateyê kontrol bike."],
    "pl": ["Zauważ angielski wzór.", "Użyj go w krótkim zdaniu.", "Sprawdź znaczenie, zanim powiesz albo napiszesz zdanie."],
    "ro": ["Observă tiparul englez.", "Folosește-l într-o propoziție scurtă.", "Verifică sensul înainte să vorbești sau să scrii."],
    "sq": ["Vëre modelin anglisht.", "Përdore në një fjali të shkurtër.", "Kontrollo kuptimin para se të flasësh ose të shkruash."],
}

NARRATIVE_HELPER = t(
    "",
    "در این درس با یک بخش کوچک اما واقعی از انگلیسی A1 کار می‌کنی و آن را در جمله‌های کوتاه تمرین می‌کنی.",
    "في هذا الدرس تعمل على جزء صغير لكنه واقعي من الإنجليزية في A1 وتتدرّب عليه في جمل قصيرة.",
    "Bu derste A1 İngilizcesinin küçük ama gerçek bir parçasıyla çalışır ve onu kısa cümlelerde denersin.",
    "В этом уроке вы работаете с небольшой, но реальной частью английского A1 и тренируете ее в коротких предложениях.",
    "لەم وانەیەدا بە بەشێکی بچووک بەڵام ڕاستەقینەی ئینگلیزی A1 کار دەکەیت و لە ڕستەی کورتدا راهێنانی پێ دەکەیت.",
    "Di vê dersê de tu bi beşeke piçûk lê rastîn a îngilîziya A1 dixebitî û wê di hevokên kurt de ticeribînî.",
    "W tej lekcji pracujesz nad małym, ale realnym elementem angielskiego A1 i ćwiczysz go w krótkich zdaniach.",
    "În această lecție lucrezi cu o parte mică, dar reală, din engleza A1 și o exersezi în propoziții scurte.",
    "Në këtë mësim punon me një pjesë të vogël, por reale, të anglishtes A1 dhe e ushtron në fjali të shkurtra.",
)


def activity(kind, title, instruction, target_type, target_slug, order, minutes=5):
    return {
        "kind": kind,
        "title": title["en"],
        "titleTranslations": tr(title),
        "instruction": instruction["en"],
        "instructionTranslations": tr(instruction),
        "targetType": target_type,
        "targetSlug": target_slug,
        "estimatedMinutes": minutes,
        "sortOrder": order,
        "isRequired": True,
    }


def lesson(
    slug,
    number,
    title,
    short,
    narrative,
    prev_slug,
    next_slug,
    grammar_links,
    expression_links,
    exercise_links,
    writing_links,
    activities,
):
    prerequisite = [prev_slug] if prev_slug else []
    return {
        "slug": slug,
        "coursePathSlug": "en-a1-everyday-start",
        "moduleSlug": "en-a1-first-contacts",
        "lessonNumber": number,
        "title": title["en"],
        "titleTranslations": tr(title),
        "shortDescription": short["en"],
        "shortDescriptionTranslations": tr(short),
        "narrative": narrative["en"],
        "narrativeTranslations": tr({**NARRATIVE_HELPER, "en": narrative["en"]}),
        "cefrLevel": "A1",
        "estimatedMinutes": 22,
        "learningGoals": GENERIC_GOALS["en"],
        "learningGoalsTranslations": tr_list(GENERIC_GOALS),
        "prerequisiteLessonSlugs": prerequisite,
        "nextLessonSlug": next_slug,
        "linkedGrammarTopicSlugs": grammar_links,
        "linkedWordSlugs": [],
        "linkedExpressionSlugs": expression_links,
        "linkedDialogueSlugs": [],
        "linkedTalkTopicSlugs": [],
        "linkedExerciseSetSlugs": ["en-a1-first-contact-practice"],
        "linkedExamPrepSlugs": [],
        "activityBlocks": activities,
        "reviewSummary": "Use the lesson language once without reading the model.",
        "reviewSummaryTranslations": tr(
            t(
                "Use the lesson language once without reading the model.",
                "زبان همین درس را یک بار بدون خواندن نمونه به کار ببر.",
                "استخدم لغة الدرس مرة واحدة من دون قراءة النموذج.",
                "Dersin dilini modele bakmadan bir kez kullan.",
                "Используйте язык урока один раз, не читая образец.",
                "زمانی وانەکە جارێک بێ خوێندنەوەی نموونەکە بەکاربهێنە.",
                "Zimanê dersê carekê bê xwendina modelê bi kar bîne.",
                "Użyj języka z lekcji raz bez czytania wzoru.",
                "Folosește limbajul lecției o dată fără să citești modelul.",
                "Përdor gjuhën e mësimit një herë pa lexuar modelin.",
            )
        ),
        "homeworkTask": "Write or say three very short English sentences.",
        "homeworkTaskTranslations": tr(
            t(
                "Write or say three very short English sentences.",
                "سه جمله خیلی کوتاه انگلیسی بنویس یا بگو.",
                "اكتب أو قل ثلاث جمل إنجليزية قصيرة جداً.",
                "Üç çok kısa İngilizce cümle yaz ya da söyle.",
                "Напишите или скажите три очень коротких английских предложения.",
                "سێ ڕستەی زۆر کورتی ئینگلیزی بنووسە یان بڵێ.",
                "Sê hevokên pir kurt ên îngilîzî binivîse an bêje.",
                "Napisz albo powiedz trzy bardzo krótkie zdania po angielsku.",
                "Scrie sau spune trei propoziții foarte scurte în engleză.",
                "Shkruaj ose thuaj tre fjali shumë të shkurtra në anglisht.",
            )
        ),
        "isPublished": True,
        "sortOrder": number * 10,
    }


def grammar_topic(slug, title, short, category, sections, examples, rules, mistake, sort_order):
    localized_title = title
    localized_short = short
    return {
        "slug": slug,
        "contentRevision": 1,
        "title": title["en"],
        "titleLocalized": localized_title,
        "shortDescription": short["en"],
        "shortDescriptionLocalized": localized_short,
        "cefrLevel": "A1",
        "grammarCategory": category,
        "topics": [],
        "isPublished": True,
        "sortOrder": sort_order,
        "sections": [
            {
                "sectionKey": section_key,
                "heading": heading["en"],
                "explanation": text["en"],
                "sortOrder": index * 10,
                "localizedBlocks": blocks(text),
                "translations": section_translations(heading, text),
            }
            for index, (section_key, heading, text) in enumerate(sections, start=1)
        ],
        "examples": localized_examples(examples),
        "ruleSummaries": [
            {"text": rule["en"], "translations": meaning_translations(rule), "sortOrder": index * 10}
            for index, rule in enumerate(rules, start=1)
        ],
        "commonMistakes": [
            {
                "wrongText": mistake[0],
                "correctedText": mistake[1],
                "explanation": mistake[2]["en"],
                "translations": meaning_translations(mistake[2]),
                "sortOrder": 10,
            }
        ],
        "exceptionNotes": [],
        "prerequisiteSlugs": [],
        "relatedTopicSlugs": [],
        "linkedWords": [],
        "linkedDialogueSlugs": [],
        "linkedTalkTopicSlugs": [],
        "linkedExerciseSlugs": [],
    }


def expression_entry(index, slug, text, actual, usage, examples):
    return {
        "slug": slug,
        "expressionText": text,
        "actualMeaningText": actual["en"],
        "usageExplanation": usage["en"],
        "cefrLevel": "A1",
        "expressionType": "fixed-expression",
        "register": "neutral",
        "category": "social-interaction",
        "region": "global",
        "topics": [],
        "isPublished": True,
        "sortOrder": index * 10,
        "meaningTransparency": "literal-fixed-formula",
        "teachingReason": "This is a high-frequency A1 expression for first-contact English.",
        "safetyRating": "general",
        "minimumAge": 0,
        "sensitiveContentKind": "none",
        "usagePolicy": "safe-to-use",
        "meanings": expression_meanings(actual, usage),
        "examples": localized_examples(examples),
        "warnings": [],
        "linkedWords": [],
        "relatedExpressionSlugs": [],
        "linkedExerciseSlugs": [],
    }


def exercise(slug, title, instruction, prompt, answer_key, correct, incorrect, hint, owner_type, owner_slug, sort_order, exercise_type="multiple-choice", target_skill="grammar"):
    title_tr = t(
        title,
        "تمرین کوتاه انگلیسی",
        "تمرين إنجليزي قصير",
        "Kısa İngilizce alıştırması",
        "Короткое упражнение по английскому",
        "راهێنانی کورتی ئینگلیزی",
        "Rahênana kurt a îngilîzî",
        "Krótkie ćwiczenie z angielskiego",
        "Exercițiu scurt de engleză",
        "Ushtrim i shkurtër anglisht",
    )
    instruction_tr = t(
        instruction,
        "پاسخ مناسب را انتخاب کن و بعد دلیل آن را بخوان.",
        "اختر الإجابة المناسبة ثم اقرأ السبب.",
        "Uygun cevabı seç ve sonra nedenini oku.",
        "Выберите подходящий ответ, затем прочитайте объяснение.",
        "وەڵامی گونجاو هەڵبژێرە و پاشان هۆکارەکە بخوێنەوە.",
        "Bersiva guncav hilbijêre û paşê sedemê bixwîne.",
        "Wybierz odpowiedź, a potem przeczytaj powód.",
        "Alege răspunsul potrivit și apoi citește explicația.",
        "Zgjidh përgjigjen e përshtatshme dhe pastaj lexo arsyen.",
    )
    correct_tr = t(
        correct,
        "پاسخ درست است، چون با معنی و ساختار جمله هماهنگ است.",
        "الإجابة صحيحة لأنها تناسب المعنى وبنية الجملة.",
        "Cevap doğru, çünkü anlam ve cümle yapısıyla uyumludur.",
        "Ответ правильный, потому что он подходит по смыслу и структуре.",
        "وەڵامەکە دروستە، چونکە لەگەڵ واتا و پێکهاتەی ڕستەکە دەگونجێت.",
        "Bersiv rast e, ji ber ku bi wate û avahiya hevokê re digunce.",
        "Odpowiedź jest poprawna, bo pasuje do znaczenia i budowy zdania.",
        "Răspunsul este corect, fiindcă se potrivește cu sensul și structura propoziției.",
        "Përgjigjja është e saktë, sepse përshtatet me kuptimin dhe strukturën e fjalisë.",
    )
    incorrect_tr = t(
        incorrect,
        "به معنی جمله و ترتیب درست واژه‌ها دوباره دقت کن.",
        "انتبه مرة أخرى إلى معنى الجملة وترتيب الكلمات الصحيح.",
        "Cümlenin anlamına ve doğru kelime sırasına tekrar dikkat et.",
        "Еще раз обратите внимание на смысл предложения и правильный порядок слов.",
        "دووبارە سەرنج بدە بە واتای ڕستەکە و ڕیزبەندی دروستی وشەکان.",
        "Dîsa balê bide wateya hevokê û rêza rast a peyvan.",
        "Zwróć jeszcze raz uwagę na znaczenie zdania i właściwy szyk słów.",
        "Fii atent din nou la sensul propoziției și la ordinea corectă a cuvintelor.",
        "Vëre përsëri kuptimin e fjalisë dhe rendin e saktë të fjalëve.",
    )
    hint_tr = t(
        hint,
        "به کلمه‌های اطراف جای خالی و معنی جمله دقت کن.",
        "انتبه إلى الكلمات حول الفراغ ومعنى الجملة.",
        "Boşluğun çevresindeki kelimelere ve cümlenin anlamına dikkat et.",
        "Обратите внимание на слова вокруг пропуска и смысл предложения.",
        "سەرنج بدە بە وشەکانی دەوروبەری بۆشاییەکە و واتای ڕستەکە.",
        "Balê bide peyvên derdora valahiyê û wateya hevokê.",
        "Zwróć uwagę na słowa wokół luki i znaczenie zdania.",
        "Fii atent la cuvintele din jurul spațiului liber și la sensul propoziției.",
        "Vëre fjalët rreth boshllëkut dhe kuptimin e fjalisë.",
    )
    return {
        "slug": slug,
        "title": title,
        "titleTranslations": tr(title_tr),
        "instruction": instruction,
        "instructionTranslations": tr(instruction_tr),
        "cefrLevel": "A1",
        "exerciseType": exercise_type,
        "targetSkill": target_skill,
        "ownerType": owner_type,
        "ownerSlug": owner_slug,
        "prompt": prompt,
        "answerKey": answer_key,
        "correctExplanation": correct,
        "correctExplanationTranslations": tr(correct_tr),
        "incorrectExplanation": incorrect,
        "incorrectExplanationTranslations": tr(incorrect_tr),
        "hint": hint,
        "hintTranslations": tr(hint_tr),
        "isPublished": True,
        "sortOrder": sort_order,
    }


def writing_template(slug, title, short, situation, template, explanation, variables, sample, sort_order, linked_lesson):
    title_tr = t(
        title,
        {
            "Simple appointment request": "درخواست ساده برای وقت",
            "Short apology message": "پیام کوتاه عذرخواهی",
            "Simple thank-you message": "پیام ساده تشکر",
        }[title],
        {
            "Simple appointment request": "طلب موعد بسيط",
            "Short apology message": "رسالة اعتذار قصيرة",
            "Simple thank-you message": "رسالة شكر بسيطة",
        }[title],
        {
            "Simple appointment request": "Basit randevu isteği",
            "Short apology message": "Kısa özür mesajı",
            "Simple thank-you message": "Basit teşekkür mesajı",
        }[title],
        {
            "Simple appointment request": "Простая просьба о встрече",
            "Short apology message": "Короткое сообщение с извинением",
            "Simple thank-you message": "Простое сообщение с благодарностью",
        }[title],
        {
            "Simple appointment request": "داواکاریی سادە بۆ کاتێک",
            "Short apology message": "نامەی کورتی داوای لێبوردن",
            "Simple thank-you message": "نامەی سادەی سوپاس",
        }[title],
        {
            "Simple appointment request": "Daxwaza hevdîtineke hêsan",
            "Short apology message": "Peyama lêborîneke kurt",
            "Simple thank-you message": "Peyama spaseke hêsan",
        }[title],
        {
            "Simple appointment request": "Prosta prośba o termin",
            "Short apology message": "Krótka wiadomość z przeprosinami",
            "Simple thank-you message": "Prosta wiadomość z podziękowaniem",
        }[title],
        {
            "Simple appointment request": "Cerere simplă pentru o programare",
            "Short apology message": "Mesaj scurt de scuze",
            "Simple thank-you message": "Mesaj simplu de mulțumire",
        }[title],
        {
            "Simple appointment request": "Kërkesë e thjeshtë për takim",
            "Short apology message": "Mesazh i shkurtër ndjese",
            "Simple thank-you message": "Mesazh i thjeshtë falënderimi",
        }[title],
    )
    generic = t(
        short,
        "یک متن کوتاه انگلیسی برای یک موقعیت ساده و واقعی A1.",
        "نص إنجليزي قصير لموقف بسيط وحقيقي في A1.",
        "Basit ve gerçek bir A1 durumu için kısa İngilizce metin.",
        "Короткий английский текст для простой реальной ситуации A1.",
        "دەقێکی کورتی ئینگلیزی بۆ دۆخێکی سادە و ڕاستەقینەی A1.",
        "Nivîseke kurt a îngilîzî ji bo rewşeke hêsan û rastîn a A1.",
        "Krótki tekst po angielsku do prostej, realnej sytuacji A1.",
        "Un text scurt în engleză pentru o situație simplă și reală A1.",
        "Një tekst i shkurtër anglisht për një situatë të thjeshtë reale A1.",
    )
    situation_tr = t(
        situation,
        "می‌خواهی یک پیام کوتاه، روشن و مودبانه به انگلیسی بنویسی.",
        "تريد أن تكتب رسالة قصيرة وواضحة ومهذبة بالإنجليزية.",
        "İngilizce kısa, açık ve kibar bir mesaj yazmak istiyorsun.",
        "Вы хотите написать короткое, ясное и вежливое сообщение на английском.",
        "دەتەوێت نامەیەکی کورت، ڕوون و ڕێزدارانە بە ئینگلیزی بنووسیت.",
        "Tu dixwazî bi îngilîzî peyameke kurt, zelal û bi rêz binivîsî.",
        "Chcesz napisać krótką, jasną i uprzejmą wiadomość po angielsku.",
        "Vrei să scrii un mesaj scurt, clar și politicos în engleză.",
        "Dëshiron të shkruash një mesazh të shkurtër, të qartë dhe të sjellshëm në anglisht.",
    )
    template_tr = t(
        template,
        "ترجمه راهنما: ساختار را با اطلاعات خودت کامل کن، نه اینکه متن فارسی را جایگزین متن انگلیسی کنی.",
        "ترجمة مساعدة: أكمل البنية بمعلوماتك، ولا تستبدل النص الإنجليزي بالنص العربي.",
        "Yardım çevirisi: Yapıyı kendi bilgilerinle tamamla; Türkçe metni İngilizcenin yerine koyma.",
        "Подсказка: заполните английскую структуру своими данными, не заменяйте английский текст русским.",
        "وەرگێڕانی یارمەتی: پێکهاتەکە بە زانیاری خۆت پڕ بکە، نەک دەقی کوردی لە جیاتی ئینگلیزی دابنێیت.",
        "Wergera alîkar: Avahiyê bi agahiyên xwe dagire; nivîsa kurdî li şûna îngilîzî daneyne.",
        "Tłumaczenie pomocnicze: uzupełnij angielską strukturę swoimi danymi, nie zastępuj jej polskim tekstem.",
        "Traducere de sprijin: completează structura engleză cu datele tale, nu o înlocui cu text românesc.",
        "Përkthim ndihmës: plotëso strukturën anglisht me të dhënat e tua, mos e zëvendëso me tekst shqip.",
    )
    explanation_tr = t(
        explanation,
        "متن کوتاه است، مستقیم سر اصل موضوع می‌رود و برای سطح A1 بیش از حد توضیح نمی‌دهد.",
        "النص قصير ومباشر ولا يشرح أكثر مما يلزم في مستوى A1.",
        "Metin kısa ve doğrudandır; A1 seviyesi için gereğinden fazla açıklama yapmaz.",
        "Текст короткий и прямой; для уровня A1 он не дает лишних объяснений.",
        "دەقەکە کورت و ڕاستەوخۆیە و بۆ ئاستی A1 زۆر زیاتر ڕوون ناکاتەوە.",
        "Nivîs kurt û rasterast e; ji bo asta A1 zêde rave nake.",
        "Tekst jest krótki i bezpośredni; na poziomie A1 nie wyjaśnia za dużo.",
        "Textul este scurt și direct; pentru A1 nu explică mai mult decât trebuie.",
        "Teksti është i shkurtër dhe i drejtpërdrejtë; për A1 nuk shpjegon më shumë se duhet.",
    )
    sample_tr = t(
        sample,
        "نمونه کامل‌شده را بخوان و بعد همان ساختار را با اطلاعات خودت به انگلیسی بنویس.",
        "اقرأ المثال المكتمل ثم اكتب البنية نفسها بالإنجليزية بمعلوماتك.",
        "Doldurulmuş örneği oku, sonra aynı yapıyı kendi bilgilerinle İngilizce yaz.",
        "Прочитайте заполненный пример, затем напишите ту же структуру на английском со своими данными.",
        "نموونەی پڕکراوە بخوێنەوە و پاشان هەمان پێکهاتە بە زانیاری خۆت بە ئینگلیزی بنووسە.",
        "Mînaka dagirtî bixwîne û paşê heman avahî bi agahiyên xwe bi îngilîzî binivîse.",
        "Przeczytaj wypełniony przykład, a potem napisz tę samą strukturę po angielsku z własnymi danymi.",
        "Citește exemplul completat și apoi scrie aceeași structură în engleză cu datele tale.",
        "Lexo shembullin e plotësuar dhe pastaj shkruaj të njëjtën strukturë anglisht me të dhënat e tua.",
    )
    return {
        "slug": slug,
        "title": title,
        "titleTranslations": tr(title_tr),
        "shortDescription": short,
        "shortDescriptionTranslations": tr(generic),
        "cefrLevel": "A1",
        "category": "email-to-school",
        "situation": situation,
        "situationTranslations": tr(situation_tr),
        "register": "neutral",
        "templateText": template,
        "templateTextTranslations": tr(template_tr),
        "explanation": explanation,
        "explanationTranslations": tr(explanation_tr),
        "replaceableVariables": variables,
        "sampleFilledVersion": sample,
        "sampleFilledVersionTranslations": tr(sample_tr),
        "linkedGrammarTopicSlugs": ["en-a1-basic-word-order"],
        "linkedWordSlugs": [],
        "linkedExpressionSlugs": ["please", "thank-you"],
        "linkedExerciseSlugs": [],
        "linkedCourseLessonSlugs": [linked_lesson],
        "isPublished": True,
        "sortOrder": sort_order,
    }


def repair_existing_writing_titles(package):
    replacements = {
        "en-a1-short-self-introduction-message": t(
            "Short self-introduction message",
            "پیام کوتاه معرفی خود",
            "رسالة تعريف قصيرة بالنفس",
            "Kısa kendini tanıtma mesajı",
            "Короткое сообщение о себе",
            "نامەی کورتی خۆناساندن",
            "Peyama kurt a xwe nasandinê",
            "Krótka wiadomość o sobie",
            "Mesaj scurt de prezentare",
            "Mesazh i shkurtër prezantimi",
        ),
        "en-a1-simple-class-question": t(
            "Simple class question",
            "پرسش ساده درباره کلاس",
            "سؤال بسيط عن الصف",
            "Sınıf hakkında basit soru",
            "Простой вопрос о занятии",
            "پرسیاری سادە دەربارەی پۆل",
            "Pirseke hêsan li ser polê",
            "Proste pytanie o zajęcia",
            "Întrebare simplă despre curs",
            "Pyetje e thjeshtë për klasën",
        ),
    }
    for item in package.get("writingTemplates", []):
        if item["slug"] in replacements:
            item["titleTranslations"] = tr(replacements[item["slug"]])


def main():
    package = json.loads(PACKAGE.read_text(encoding="utf-8"))
    repair_existing_writing_titles(package)

    lessons = [
        lesson(
            "en-a1-people-and-pronouns",
            6,
            t("People and pronouns", "اشخاص و ضمیرها", "الأشخاص والضمائر", "Kişiler ve zamirler", "Люди и местоимения", "کەسەکان و جێناوەکان", "Kes û cînav", "Osoby i zaimki", "Persoane și pronume", "Njerëz dhe përemra"),
            t("Use he, she, we, and they in very short sentences.", "از he، she، we و they در جمله‌های خیلی کوتاه استفاده کن.", "استخدم he و she و we و they في جمل قصيرة جداً.", "he, she, we ve they yapılarını çok kısa cümlelerde kullan.", "Используйте he, she, we и they в очень коротких предложениях.", "he، she، we و they لە ڕستەی زۆر کورتدا بەکاربهێنە.", "he, she, we û they di hevokên pir kurt de bi kar bîne.", "Używaj he, she, we i they w bardzo krótkich zdaniach.", "Folosește he, she, we și they în propoziții foarte scurte.", "Përdor he, she, we dhe they në fjali shumë të shkurtra."),
            t("You practise how English names people first and then refers to them with a pronoun.", "", "", "", "", "", "", "", "", ""),
            "en-a1-name-everyday-objects",
            "en-a1-polite-you-and-basic-requests",
            ["en-a1-subject-pronouns-and-be"],
            [],
            [],
            [],
            [
                activity("read", READ_TITLE, READ_INSTRUCTION, "none", None, 10, 4),
                activity("grammar", t("Check pronouns with be", "ضمیرها را با be بررسی کن", "راجع الضمائر مع be", "be ile zamirleri kontrol et", "Проверьте местоимения с be", "جێناوەکان لەگەڵ be بپشکنە", "Cînavan bi be re kontrol bike", "Sprawdź zaimki z be", "Verifică pronumele cu be", "Kontrollo përemrat me be"), t("Read the grammar note again and focus on he is, she is, we are, and they are.", "نکته گرامری را دوباره بخوان و روی he is، she is، we are و they are تمرکز کن.", "اقرأ الملاحظة النحوية مرة أخرى وركز على he is و she is و we are و they are.", "Dil bilgisi notunu tekrar oku ve he is, she is, we are, they are yapılarına odaklan.", "Прочитайте заметку еще раз и сосредоточьтесь на he is, she is, we are и they are.", "تێبینیی گرامەرەکە دووبارە بخوێنەوە و سەرنج بدە بە he is، she is، we are و they are.", "Nîşeya rêzimanê careke din bixwîne û balê bide he is, she is, we are û they are.", "Przeczytaj notatkę jeszcze raz i skup się na he is, she is, we are oraz they are.", "Citește din nou nota de gramatică și concentrează-te pe he is, she is, we are și they are.", "Lexo përsëri shënimin gramatikor dhe përqendrohu te he is, she is, we are dhe they are."), "grammar-topic", "en-a1-subject-pronouns-and-be", 20, 6),
                activity("review", REVIEW_TITLE, REVIEW_INSTRUCTION, "none", None, 30, 5),
            ],
        ),
        lesson(
            "en-a1-polite-you-and-basic-requests",
            7,
            t("Polite you and basic requests", "you مودبانه و درخواست‌های ساده", "you المهذبة والطلبات البسيطة", "Kibar you ve basit istekler", "Вежливое you и простые просьбы", "you ـی ڕێزدارانە و داواکاریی سادە", "you ya bi rêz û daxwazên hêsan", "Uprzejme you i proste prośby", "you politicos și cereri simple", "you i sjellshëm dhe kërkesa të thjeshta"),
            t("Make very short requests with please and excuse me.", "با please و excuse me درخواست‌های خیلی کوتاه بساز.", "كوّن طلبات قصيرة جداً باستخدام please و excuse me.", "please ve excuse me ile çok kısa istekler kur.", "Составляйте очень короткие просьбы с please и excuse me.", "بە please و excuse me داواکاریی زۆر کورت دروست بکە.", "Bi please û excuse me daxwazên pir kurt ava bike.", "Twórz bardzo krótkie prośby z please i excuse me.", "Formulează cereri foarte scurte cu please și excuse me.", "Ndërto kërkesa shumë të shkurtra me please dhe excuse me."),
            t("You learn how English uses a polite tone in short requests without adding long explanations.", "", "", "", "", "", "", "", "", ""),
            "en-a1-people-and-pronouns",
            "en-a1-regular-verbs-in-daily-life",
            [],
            ["please", "excuse-me"],
            [],
            [],
            [
                activity("read", READ_TITLE, READ_INSTRUCTION, "none", None, 10, 4),
                activity("expression", t("Use please and excuse me", "از please و excuse me استفاده کن", "استخدم please و excuse me", "please ve excuse me kullan", "Используйте please и excuse me", "please و excuse me بەکاربهێنە", "please û excuse me bi kar bîne", "Użyj please i excuse me", "Folosește please și excuse me", "Përdor please dhe excuse me"), t("Say two short requests and keep your voice polite.", "دو درخواست کوتاه بگو و لحن را مودبانه نگه دار.", "قل طلبين قصيرين وحافظ على نبرة مهذبة.", "İki kısa istek söyle ve tonunu kibar tut.", "Скажите две короткие просьбы и сохраняйте вежливый тон.", "دوو داواکاریی کورت بڵێ و دەنگت ڕێزدارانە بهێڵە.", "Du daxwazên kurt bibêje û dengê xwe bi rêz bihêle.", "Powiedz dwie krótkie prośby i zachowaj uprzejmy ton.", "Spune două cereri scurte și păstrează un ton politicos.", "Thuaj dy kërkesa të shkurtra dhe mbaje tonin të sjellshëm."), "expression", "please", 20, 5),
                activity("review", REVIEW_TITLE, REVIEW_INSTRUCTION, "none", None, 30, 5),
            ],
        ),
        lesson(
            "en-a1-regular-verbs-in-daily-life",
            8,
            t("Regular verbs in daily life", "فعل‌های منظم در زندگی روزمره", "الأفعال المنتظمة في الحياة اليومية", "Günlük hayatta düzenli fiiller", "Правильные глаголы в повседневной жизни", "کرداری ڕێک لە ژیانی ڕۆژانەدا", "Lêkerên rêkûpêk di jiyana rojane de", "Regularne czasowniki w codziennym życiu", "Verbe regulate în viața de zi cu zi", "Folje të rregullta në jetën e përditshme"),
            t("Use simple verbs like live, work, study, and need.", "از فعل‌های ساده‌ای مثل live، work، study و need استفاده کن.", "استخدم أفعالاً بسيطة مثل live و work و study و need.", "live, work, study ve need gibi basit fiiller kullan.", "Используйте простые глаголы, такие как live, work, study и need.", "کرداری سادە وەک live، work، study و need بەکاربهێنە.", "Lêkerên hêsan wek live, work, study û need bi kar bîne.", "Używaj prostych czasowników, takich jak live, work, study i need.", "Folosește verbe simple precum live, work, study și need.", "Përdor folje të thjeshta si live, work, study dhe need."),
            t("You build very short present-simple sentences for daily actions and needs.", "", "", "", "", "", "", "", "", ""),
            "en-a1-polite-you-and-basic-requests",
            "en-a1-build-a-short-introduction",
            ["en-a1-basic-word-order"],
            ["i-live-in", "can-you-help-me"],
            [],
            [],
            [
                activity("read", READ_TITLE, READ_INSTRUCTION, "none", None, 10, 4),
                activity("grammar", t("Check basic word order", "ترتیب ساده واژه‌ها را بررسی کن", "راجع ترتيب الكلمات الأساسي", "Temel kelime sırasını kontrol et", "Проверьте базовый порядок слов", "ڕیزبەندی سادەی وشەکان بپشکنە", "Rêza bingehîn a peyvan kontrol bike", "Sprawdź podstawowy szyk słów", "Verifică ordinea de bază a cuvintelor", "Kontrollo rendin bazë të fjalëve"), t("Read the word-order note and keep the subject before the verb.", "نکته ترتیب واژه‌ها را بخوان و فاعل را قبل از فعل نگه دار.", "اقرأ ملاحظة ترتيب الكلمات واجعل الفاعل قبل الفعل.", "Kelime sırası notunu oku ve özneyi fiilden önce tut.", "Прочитайте заметку о порядке слов и ставьте подлежащее перед глаголом.", "تێبینیی ڕیزبەندی وشەکان بخوێنەوە و بکەر پێش کردار دابنێ.", "Nîşeya rêza peyvan bixwîne û kirdeyê berî lêkerê bihêle.", "Przeczytaj notatkę o szyku i trzymaj podmiot przed czasownikiem.", "Citește nota despre ordinea cuvintelor și păstrează subiectul înaintea verbului.", "Lexo shënimin për rendin e fjalëve dhe mbaje kryefjalën para foljes."), "grammar-topic", "en-a1-basic-word-order", 20, 6),
                activity("review", REVIEW_TITLE, REVIEW_INSTRUCTION, "none", None, 30, 5),
            ],
        ),
        lesson(
            "en-a1-build-a-short-introduction",
            9,
            t("Build a short introduction", "یک معرفی کوتاه بساز", "ابنِ تعريفاً قصيراً بالنفس", "Kısa bir tanıtma kur", "Составьте короткое представление", "خۆناسێنانی کورت دروست بکە", "Nasandineke kurt ava bike", "Zbuduj krótkie przedstawienie się", "Construiește o prezentare scurtă", "Ndërto një prezantim të shkurtër"),
            t("Put name, place, and one simple detail together.", "نام، محل و یک اطلاعات ساده را کنار هم بگذار.", "اجمع الاسم والمكان ومعلومة بسيطة واحدة.", "Ad, yer ve bir basit bilgiyi bir araya getir.", "Соедините имя, место и одну простую деталь.", "ناو، شوێن و زانیارییەکی سادە پێکەوە دابنێ.", "Nav, cih û agahiyeke hêsan bi hev re deyne.", "Połącz imię, miejsce i jedną prostą informację.", "Pune împreună numele, locul și un detaliu simplu.", "Vendos bashkë emrin, vendin dhe një të dhënë të thjeshtë."),
            t("You combine the first-contact language into one short, natural introduction.", "", "", "", "", "", "", "", "", ""),
            "en-a1-regular-verbs-in-daily-life",
            "en-a1-first-contact-review",
            ["en-a1-basic-word-order"],
            ["my-name-is", "i-am-from", "i-live-in"],
            ["en-a1-complete-a-short-introduction"],
            ["en-a1-short-self-introduction-message"],
            [
                activity("read", READ_TITLE, READ_INSTRUCTION, "none", None, 10, 4),
                activity("write", t("Write a short introduction", "یک معرفی کوتاه بنویس", "اكتب تعريفاً قصيراً بالنفس", "Kısa bir tanıtma yaz", "Напишите короткое представление", "خۆناسێنانی کورت بنووسە", "Nasandineke kurt binivîse", "Napisz krótkie przedstawienie się", "Scrie o prezentare scurtă", "Shkruaj një prezantim të shkurtër"), t("Use the template and replace the name, country, and city with your own information.", "از الگو استفاده کن و نام، کشور و شهر را با اطلاعات خودت جایگزین کن.", "استخدم القالب واستبدل الاسم والبلد والمدينة بمعلوماتك.", "Şablonu kullan ve adı, ülkeyi ve şehri kendi bilgilerinle değiştir.", "Используйте шаблон и замените имя, страну и город своими данными.", "قاڵبەکە بەکاربهێنە و ناو، وڵات و شار بە زانیاری خۆت بگۆڕە.", "Şablonê bi kar bîne û nav, welat û bajar bi agahiyên xwe biguherîne.", "Użyj szablonu i zastąp imię, kraj oraz miasto własnymi informacjami.", "Folosește șablonul și înlocuiește numele, țara și orașul cu datele tale.", "Përdor modelin dhe zëvendëso emrin, vendin dhe qytetin me të dhënat e tua."), "writing-template", "en-a1-short-self-introduction-message", 20, 7),
                activity("practice", t("Complete the introduction", "معرفی را کامل کن", "أكمل التعريف بالنفس", "Tanıtmayı tamamla", "Дополните представление", "خۆناسێنانەکە تەواو بکە", "Nasandinê temam bike", "Uzupełnij przedstawienie się", "Completează prezentarea", "Plotëso prezantimin"), t("Choose the missing words and then read the whole introduction aloud.", "کلمات جاافتاده را انتخاب کن و بعد کل معرفی را بلند بخوان.", "اختر الكلمات الناقصة ثم اقرأ التعريف كاملاً بصوت واضح.", "Eksik kelimeleri seç ve sonra tüm tanıtmayı sesli oku.", "Выберите пропущенные слова, затем прочитайте все представление вслух.", "وشە کەمەکان هەڵبژێرە و پاشان هەموو خۆناسێنانەکە بە دەنگی بخوێنەوە.", "Peyvên kêm hilbijêre û paşê hemû nasandinê bi dengekî eşkere bixwîne.", "Wybierz brakujące słowa, a potem przeczytaj całe przedstawienie na głos.", "Alege cuvintele lipsă și apoi citește toată prezentarea cu voce tare.", "Zgjidh fjalët që mungojnë dhe pastaj lexo gjithë prezantimin me zë."), "exercise", "en-a1-complete-a-short-introduction", 30, 6),
                activity("review", REVIEW_TITLE, REVIEW_INSTRUCTION, "none", None, 40, 5),
            ],
        ),
        lesson(
            "en-a1-first-contact-review",
            10,
            t("First contact review", "مرور اولین ارتباط", "مراجعة التواصل الأول", "İlk temas tekrarı", "Повторение первого контакта", "پێداچوونەوەی یەکەم پەیوەندی", "Dubarekirina têkiliya yekem", "Powtórka pierwszego kontaktu", "Recapitularea primului contact", "Përsëritje e kontaktit të parë"),
            t("Review greetings, questions, short details, and one mini-message.", "سلام‌ها، پرسش‌ها، اطلاعات کوتاه و یک پیام کوچک را مرور کن.", "راجع التحيات والأسئلة والمعلومات القصيرة ورسالة صغيرة.", "Selamları, soruları, kısa bilgileri ve küçük bir mesajı tekrar et.", "Повторите приветствия, вопросы, короткие детали и одно мини-сообщение.", "سڵاو، پرسیار، زانیاریی کورت و نامەیەکی بچووک پێداچوونەوە بکە.", "Silav, pirs, agahiyên kurt û peyameke piçûk dubare bike.", "Powtórz powitania, pytania, krótkie informacje i jedną małą wiadomość.", "Recapitulează saluturi, întrebări, detalii scurte și un mini-mesaj.", "Përsërit përshëndetjet, pyetjet, të dhënat e shkurtra dhe një mesazh të vogël."),
            t("You actively reuse the whole first-contact module in a short review sequence.", "", "", "", "", "", "", "", "", ""),
            "en-a1-build-a-short-introduction",
            None,
            ["en-a1-simple-questions-with-be", "en-a1-basic-word-order"],
            ["hello", "can-you-repeat-that", "thank-you"],
            ["en-a1-review-first-contact-phrases"],
            [],
            [
                activity("review", t("Start with the useful phrases", "با عبارت‌های کاربردی شروع کن", "ابدأ بالعبارات المفيدة", "Kullanışlı ifadelerle başla", "Начните с полезных фраз", "بە دەستەواژە بەسوودەکان دەست پێ بکە", "Bi gotinên bikêr dest pê bike", "Zacznij od przydatnych zwrotów", "Începe cu expresiile utile", "Fillo me shprehjet e dobishme"), t("Say five phrases from this module and choose one situation for each phrase.", "پنج عبارت از این ماژول بگو و برای هرکدام یک موقعیت انتخاب کن.", "قل خمس عبارات من هذا الجزء واختر موقفاً لكل عبارة.", "Bu modülden beş ifade söyle ve her biri için bir durum seç.", "Назовите пять фраз из модуля и выберите ситуацию для каждой.", "پێنج دەستەواژە لەم ماژوولە بڵێ و بۆ هەر یەک دۆخێک هەڵبژێرە.", "Ji vê modulê pênc gotinan bêje û ji bo her yekê rewşekê hilbijêre.", "Powiedz pięć zwrotów z modułu i wybierz sytuację dla każdego.", "Spune cinci expresii din modul și alege câte o situație pentru fiecare.", "Thuaj pesë shprehje nga moduli dhe zgjidh një situatë për secilën."), "none", None, 10, 5),
                activity("practice", t("Review first-contact phrases", "عبارت‌های اولین ارتباط را مرور کن", "راجع عبارات التواصل الأول", "İlk temas ifadelerini tekrar et", "Повторите фразы первого контакта", "دەستەواژەکانی یەکەم پەیوەندی پێداچوونەوە بکە", "Gotinên têkiliya yekem dubare bike", "Powtórz zwroty pierwszego kontaktu", "Recapitulează expresiile de prim contact", "Përsërit shprehjet e kontaktit të parë"), t("Complete the review exercise and explain one answer in your helper language.", "تمرین مرور را انجام بده و یک پاسخ را به زبان کمکی خودت توضیح بده.", "أنجز تمرين المراجعة واشرح إجابة واحدة بلغتك المساعدة.", "Tekrar alıştırmasını yap ve bir cevabı yardımcı dilinde açıkla.", "Выполните обзорное упражнение и объясните один ответ на своем вспомогательном языке.", "راهێنانی پێداچوونەوەکە تەواو بکە و وەڵامێک بە زمانی یارمەتیت ڕوون بکەوە.", "Rahênana dubarekirinê temam bike û bersivekê bi zimanê alîkar ê xwe rave bike.", "Wykonaj ćwiczenie powtórkowe i wyjaśnij jedną odpowiedź w swoim języku pomocniczym.", "Fă exercițiul de recapitulare și explică un răspuns în limba ta de sprijin.", "Bëj ushtrimin e përsëritjes dhe shpjego një përgjigje në gjuhën tënde ndihmëse."), "exercise", "en-a1-review-first-contact-phrases", 20, 7),
                activity("review", REVIEW_TITLE, REVIEW_INSTRUCTION, "none", None, 30, 5),
            ],
        ),
    ]

    for item in lessons:
        upsert(package["courseLessons"], item)

    grammar_topics = [
        grammar_topic(
            "en-a1-simple-questions-with-be",
            t("Simple questions with be", "پرسش‌های ساده با be", "أسئلة بسيطة مع be", "be ile basit sorular", "Простые вопросы с be", "پرسیاری سادە لەگەڵ be", "Pirsên hêsan bi be", "Proste pytania z be", "Întrebări simple cu be", "Pyetje të thjeshta me be"),
            t("Move be before the subject in very simple questions.", "در پرسش‌های خیلی ساده، be را قبل از فاعل بیاور.", "في الأسئلة البسيطة جداً ضع be قبل الفاعل.", "Çok basit sorularda be özneden önce gelir.", "В очень простых вопросах be ставится перед подлежащим.", "لە پرسیاری زۆر سادەدا be پێش بکەر دێت.", "Di pirsên pir hêsan de be berî kirdeyê tê.", "W bardzo prostych pytaniach be stoi przed podmiotem.", "În întrebările foarte simple, be vine înaintea subiectului.", "Në pyetje shumë të thjeshta, be vendoset para kryefjalës."),
            "word-order",
            [
                ("question-order", t("Question order", "ترتیب پرسش", "ترتيب السؤال", "Soru sırası", "Порядок вопроса", "ڕیزبەندی پرسیار", "Rêza pirsê", "Szyk pytania", "Ordinea întrebării", "Rendi i pyetjes"), t("In a simple question, say Are you ...? or Is she ...? Do not keep the same order as a statement.", "در یک پرسش ساده، Are you ...? یا Is she ...? می‌گویی؛ ترتیب جمله خبری را نگه نمی‌داری.", "في سؤال بسيط تقول Are you ...? أو Is she ...? ولا تُبقي ترتيب الجملة الخبرية.", "Basit bir soruda Are you ...? veya Is she ...? dersin; düz cümle sırasını korumazsın.", "В простом вопросе говорят Are you ...? или Is she ...?, а не сохраняют порядок утверждения.", "لە پرسیاری سادەدا Are you ...? یان Is she ...? دەڵێیت؛ ڕیزبەندی ڕستەی ئاسایی ناهێڵیتەوە.", "Di pirseke hêsan de Are you ...? an Is she ...? dibêjî; rêza hevoka ragihandinê nahêlî.", "W prostym pytaniu mówisz Are you ...? albo Is she ...?, nie zostawiasz szyku zdania oznajmującego.", "Într-o întrebare simplă spui Are you ...? sau Is she ...?, nu păstrezi ordinea propoziției afirmative.", "Në një pyetje të thjeshtë thua Are you ...? ose Is she ...?, nuk mban rendin e fjalisë pohuese.")),
            ],
            ["Are you in class?", "Is she from Canada?"],
            [t("Use Are you ...? for a simple question with you.", "برای پرسش ساده با you از Are you ...? استفاده کن.", "استخدم Are you ...? للسؤال البسيط مع you.", "you ile basit soru için Are you ...? kullan.", "Для простого вопроса с you используйте Are you ...?.", "بۆ پرسیاری سادە لەگەڵ you، Are you ...? بەکاربهێنە.", "Ji bo pirseke hêsan bi you, Are you ...? bi kar bîne.", "Do prostego pytania z you użyj Are you ...?.", "Pentru o întrebare simplă cu you folosește Are you ...?.", "Për një pyetje të thjeshtë me you përdor Are you ...?.")],
            ("You are in class?", "Are you in class?", t("A question with be normally changes the word order.", "پرسش با be معمولاً ترتیب واژه‌ها را عوض می‌کند.", "السؤال مع be يغيّر عادة ترتيب الكلمات.", "be ile soru genellikle kelime sırasını değiştirir.", "Вопрос с be обычно меняет порядок слов.", "پرسیار لەگەڵ be زۆرجار ڕیزبەندی وشەکان دەگۆڕێت.", "Pirs bi be gelek caran rêza peyvan diguherîne.", "Pytanie z be zwykle zmienia szyk słów.", "Întrebarea cu be schimbă de obicei ordinea cuvintelor.", "Pyetja me be zakonisht ndryshon rendin e fjalëve.")),
            40,
        ),
        grammar_topic(
            "en-a1-basic-word-order",
            t("Basic word order", "ترتیب پایه واژه‌ها", "ترتيب الكلمات الأساسي", "Temel kelime sırası", "Базовый порядок слов", "ڕیزبەندی بنەڕەتی وشەکان", "Rêza bingehîn a peyvan", "Podstawowy szyk słów", "Ordinea de bază a cuvintelor", "Rendi bazë i fjalëve"),
            t("Keep short English statements clear: subject, verb, then detail.", "جمله‌های کوتاه انگلیسی را روشن نگه دار: فاعل، فعل، بعد جزئیات.", "اجعل الجمل الإنجليزية القصيرة واضحة: الفاعل، الفعل، ثم التفصيل.", "Kısa İngilizce cümleleri açık tut: özne, fiil, sonra ayrıntı.", "Делайте короткие английские предложения ясными: подлежащее, глагол, затем деталь.", "ڕستە کورتی ئینگلیزی ڕوون بهێڵە: بکەر، کردار، پاشان وردەکاری.", "Hevokên kurt ên îngilîzî zelal bihêle: kirde, lêker, paşê hûrgilî.", "Zachowaj jasny szyk krótkich zdań: podmiot, czasownik, potem szczegół.", "Păstrează propozițiile scurte clare: subiect, verb, apoi detaliu.", "Mbaji fjalitë e shkurtra anglisht të qarta: kryefjalë, folje, pastaj detaj."),
            "word-order",
            [
                ("statement-order", t("Statement order", "ترتیب جمله خبری", "ترتيب الجملة الخبرية", "Düz cümle sırası", "Порядок утверждения", "ڕیزبەندی ڕستەی ئاسایی", "Rêza hevoka ragihandinê", "Szyk zdania oznajmującego", "Ordinea propoziției afirmative", "Rendi i fjalisë pohuese"), t("In a very short statement, English usually starts with the person or thing: I live in Berlin. She studies English.", "در جمله خیلی کوتاه، انگلیسی معمولاً با شخص یا چیز شروع می‌شود: I live in Berlin. She studies English.", "في الجملة القصيرة جداً تبدأ الإنجليزية عادة بالشخص أو الشيء: I live in Berlin. She studies English.", "Çok kısa bir cümlede İngilizce genelde kişi veya şeyle başlar: I live in Berlin. She studies English.", "В очень коротком предложении английский обычно начинается с лица или предмета: I live in Berlin. She studies English.", "لە ڕستەی زۆر کورتدا ئینگلیزی زۆرجار بە کەس یان شت دەست پێ دەکات: I live in Berlin. She studies English.", "Di hevokeke pir kurt de îngilîzî bi gelemperî bi kes an tiştê dest pê dike: I live in Berlin. She studies English.", "W bardzo krótkim zdaniu angielski zwykle zaczyna się od osoby lub rzeczy: I live in Berlin. She studies English.", "Într-o propoziție foarte scurtă, engleza începe de obicei cu persoana sau lucrul: I live in Berlin. She studies English.", "Në një fjali shumë të shkurtër, anglishtja zakonisht fillon me personin ose sendin: I live in Berlin. She studies English.")),
            ],
            ["I live in Berlin.", "She studies English."],
            [t("Use subject + verb + detail for simple statements.", "برای جمله‌های ساده از فاعل + فعل + جزئیات استفاده کن.", "استخدم الفاعل + الفعل + التفصيل في الجمل البسيطة.", "Basit cümlelerde özne + fiil + ayrıntı kullan.", "Для простых утверждений используйте подлежащее + глагол + деталь.", "بۆ ڕستەی سادە بکەر + کردار + وردەکاری بەکاربهێنە.", "Ji bo hevokên hêsan kirde + lêker + hûrgilî bi kar bîne.", "W prostych zdaniach użyj podmiot + czasownik + szczegół.", "Pentru propoziții simple folosește subiect + verb + detaliu.", "Për fjali të thjeshta përdor kryefjalë + folje + detaj.")],
            ("Live I in Berlin.", "I live in Berlin.", t("English normally puts the subject before the verb in a simple statement.", "در جمله خبری ساده، انگلیسی معمولاً فاعل را قبل از فعل می‌آورد.", "في الجملة الخبرية البسيطة تضع الإنجليزية عادة الفاعل قبل الفعل.", "Basit düz cümlede İngilizce genelde özneyi fiilden önce koyar.", "В простом утверждении английский обычно ставит подлежащее перед глаголом.", "لە ڕستەی ئاسایی سادەدا ئینگلیزی زۆرجار بکەر پێش کردار دادەنێت.", "Di hevoka ragihandinê ya hêsan de îngilîzî bi gelemperî kirdeyê berî lêkerê datîne.", "W prostym zdaniu oznajmującym angielski zwykle stawia podmiot przed czasownikiem.", "Într-o propoziție afirmativă simplă, engleza pune de obicei subiectul înaintea verbului.", "Në një fjali pohuese të thjeshtë, anglishtja zakonisht vendos kryefjalën para foljes.")),
            50,
        ),
    ]
    for item in grammar_topics:
        upsert(package["grammarTopics"], item)

    expression_data = [
        (11, "sorry", "Sorry.", t("Sorry.", "ببخشید؛ متأسفم.", "آسف / آسفة.", "Üzgünüm; pardon.", "Извините; мне жаль.", "ببوورە؛ داخم.", "Bibore; mixabin.", "Przepraszam; przykro mi.", "Îmi pare rău; scuze.", "Më fal; më vjen keq."), t("Use this when you make a small mistake or want to be polite.", "وقتی اشتباه کوچکی می‌کنی یا می‌خواهی مودب باشی، از این عبارت استفاده کن.", "استخدمها عندما ترتكب خطأ صغيراً أو تريد أن تكون مهذباً.", "Küçük bir hata yaptığında veya kibar olmak istediğinde kullan.", "Используйте, когда сделали небольшую ошибку или хотите быть вежливым.", "کاتێک هەڵەیەکی بچووک دەکەیت یان دەتەوێت ڕێزدارانە بیت، بەکاری بهێنە.", "Dema xeletiyeke piçûk dikî an dixwazî bi rêz bî, bi kar bîne.", "Użyj, gdy popełnisz mały błąd albo chcesz być uprzejmy.", "Folosește când faci o mică greșeală sau vrei să fii politicos.", "Përdore kur bën një gabim të vogël ose dëshiron të jesh i sjellshëm."), ["Sorry, I am late.", "Sorry, can you repeat that?"]),
        (12, "excuse-me", "Excuse me.", t("Excuse me.", "ببخشید؛ برای جلب توجه مودبانه.", "عفواً؛ لجذب الانتباه بأدب.", "Affedersiniz; kibarca dikkat çekmek için.", "Извините; чтобы вежливо привлечь внимание.", "ببوورە؛ بۆ ڕاکێشانی سەرنج بە ڕێز.", "Bibore; ji bo bal kişandina bi rêz.", "Przepraszam; żeby uprzejmie zwrócić uwagę.", "Scuzați-mă; pentru a atrage atenția politicos.", "Më falni; për të tërhequr vëmendjen me mirësjellje."), t("Use this before a question or when you need someone's attention.", "قبل از پرسش یا وقتی توجه کسی را می‌خواهی، از آن استفاده کن.", "استخدمها قبل السؤال أو عندما تحتاج إلى انتباه شخص ما.", "Bir sorudan önce veya birinin dikkatini istediğinde kullan.", "Используйте перед вопросом или когда нужно привлечь чье-то внимание.", "پێش پرسیار یان کاتێک سەرنجی کەسێکت دەوێت، بەکاری بهێنە.", "Berî pirsê an dema ku bala kesekî dixwazî, bi kar bîne.", "Użyj przed pytaniem albo gdy potrzebujesz czyjejś uwagi.", "Folosește înainte de o întrebare sau când ai nevoie de atenția cuiva.", "Përdore para një pyetjeje ose kur të duhet vëmendja e dikujt."), ["Excuse me, where is room 12?", "Excuse me, can you help me?"]),
        (13, "can-you-repeat-that", "Can you repeat that?", t("Can you repeat that?", "می‌توانید آن را تکرار کنید؟", "هل يمكنك أن تكرر ذلك؟", "Bunu tekrar eder misiniz?", "Вы можете это повторить?", "دەتوانیت ئەوە دووبارە بکەیتەوە؟", "Tu dikarî wê dubare bikî?", "Czy możesz to powtórzyć?", "Poți repeta asta?", "A mund ta përsërisësh?"), t("Use this when you did not hear or understand something clearly.", "وقتی چیزی را خوب نشنیدی یا نفهمیدی، از این جمله استفاده کن.", "استخدمها عندما لا تسمع أو لا تفهم شيئاً بوضوح.", "Bir şeyi net duymadığında veya anlamadığında kullan.", "Используйте, когда не расслышали или не поняли что-то ясно.", "کاتێک شتێکت بە ڕوونی نەبیست یان تێنەگەیشتیت، بەکاری بهێنە.", "Dema tiştekî zelal nebihîstî an tênegihîştî, bi kar bîne.", "Użyj, gdy czegoś dobrze nie usłyszysz albo nie zrozumiesz.", "Folosește când nu ai auzit sau nu ai înțeles clar ceva.", "Përdore kur nuk dëgjove ose nuk kuptove diçka qartë."), ["Can you repeat that, please?", "Sorry, can you repeat that?"]),
        (14, "i-do-not-understand", "I do not understand.", t("I do not understand.", "متوجه نمی‌شوم.", "لا أفهم.", "Anlamıyorum.", "Я не понимаю.", "تێناگەم.", "Ez fam nakim.", "Nie rozumiem.", "Nu înțeleg.", "Nuk kuptoj."), t("Use this simple sentence when the message is not clear for you.", "وقتی پیام برایت روشن نیست، از این جمله ساده استفاده کن.", "استخدم هذه الجملة البسيطة عندما لا تكون الرسالة واضحة لك.", "Mesaj senin için açık değilse bu basit cümleyi kullan.", "Используйте это простое предложение, когда сообщение вам непонятно.", "کاتێک نامەکە بۆت ڕوون نییە، ئەم ڕستە سادەیە بەکاربهێنە.", "Dema peyam ji bo te nezelal e, vê hevoka hêsan bi kar bîne.", "Użyj tego prostego zdania, gdy komunikat nie jest dla ciebie jasny.", "Folosește această propoziție simplă când mesajul nu este clar pentru tine.", "Përdore këtë fjali të thjeshtë kur mesazhi nuk është i qartë për ty."), ["I do not understand the question.", "Sorry, I do not understand."]),
        (15, "can-you-help-me", "Can you help me?", t("Can you help me?", "می‌توانید به من کمک کنید؟", "هل يمكنك مساعدتي؟", "Bana yardım eder misiniz?", "Вы можете мне помочь?", "دەتوانیت یارمەتیم بدەیت؟", "Tu dikarî alîkariya min bikî?", "Czy możesz mi pomóc?", "Mă poți ajuta?", "A mund të më ndihmosh?"), t("Use this when you need practical help in a simple situation.", "وقتی در یک موقعیت ساده به کمک عملی نیاز داری، از این جمله استفاده کن.", "استخدمها عندما تحتاج إلى مساعدة عملية في موقف بسيط.", "Basit bir durumda pratik yardıma ihtiyacın olduğunda kullan.", "Используйте, когда нужна практическая помощь в простой ситуации.", "کاتێک لە دۆخێکی سادەدا پێویستت بە یارمەتی کرداری هەیە، بەکاری بهێنە.", "Dema di rewşeke hêsan de alîkariya pratîk dixwazî, bi kar bîne.", "Użyj, gdy potrzebujesz praktycznej pomocy w prostej sytuacji.", "Folosește când ai nevoie de ajutor practic într-o situație simplă.", "Përdore kur ke nevojë për ndihmë praktike në një situatë të thjeshtë."), ["Can you help me, please?", "Excuse me, can you help me?"]),
        (16, "where-is", "Where is ...?", t("Where is ...?", "… کجاست؟", "أين ...؟", "... nerede?", "Где ...?", "… لە کوێیە؟", "... li ku ye?", "Gdzie jest ...?", "Unde este ...?", "Ku është ...?"), t("Use this question to ask for a room, place, or object.", "برای پرسیدن درباره اتاق، مکان یا چیز از این پرسش استفاده کن.", "استخدم هذا السؤال للسؤال عن غرفة أو مكان أو شيء.", "Bir oda, yer veya eşya sormak için bu soruyu kullan.", "Используйте этот вопрос, чтобы спросить о комнате, месте или предмете.", "بۆ پرسیارکردن دەربارەی ژوور، شوێن یان شتێک ئەم پرسیارە بەکاربهێنە.", "Ji bo pirsîna odeyekê, cihêkî an tiştekî vê pirsê bi kar bîne.", "Użyj tego pytania, aby zapytać o salę, miejsce albo przedmiot.", "Folosește întrebarea pentru o cameră, un loc sau un obiect.", "Përdore këtë pyetje për një dhomë, vend ose send."), ["Where is room 12?", "Where is the book?"]),
        (17, "how-much-is-it", "How much is it?", t("How much is it?", "قیمتش چقدر است؟", "كم سعره؟", "Ne kadar?", "Сколько это стоит?", "نرخی چەندە؟", "Ew çiqas e?", "Ile to kosztuje?", "Cât costă?", "Sa kushton?"), t("Use this question when you ask about a price.", "وقتی درباره قیمت می‌پرسی، از این جمله استفاده کن.", "استخدم هذا السؤال عندما تسأل عن السعر.", "Fiyat sorduğunda bu soruyu kullan.", "Используйте этот вопрос, когда спрашиваете о цене.", "کاتێک دەربارەی نرخ دەپرسیت، ئەم پرسیارە بەکاربهێنە.", "Dema li ser bihayê dipirsî, vê pirsê bi kar bîne.", "Użyj tego pytania, gdy pytasz o cenę.", "Folosește întrebarea când întrebi despre preț.", "Përdore këtë pyetje kur pyet për çmimin."), ["How much is it, please?", "How much is the ticket?"]),
        (18, "see-you-later", "See you later.", t("See you later.", "بعداً می‌بینمت.", "أراك لاحقاً.", "Sonra görüşürüz.", "Увидимся позже.", "دواتر دەتبینم.", "Paşê em hev dibînin.", "Do zobaczenia później.", "Ne vedem mai târziu.", "Shihemi më vonë."), t("Use this friendly phrase when you leave but expect to meet again.", "وقتی می‌روی و احتمالاً دوباره همدیگر را می‌بینید، از این عبارت دوستانه استفاده کن.", "استخدمها عندما تغادر وتتوقع أن تلتقي بالشخص مرة أخرى.", "Ayrılırken ve tekrar görüşmeyi beklerken bu arkadaşça ifadeyi kullan.", "Используйте, когда уходите и ожидаете увидеться снова.", "کاتێک دەڕۆیت و چاوەڕوانیت دووبارە یەکتر ببینن، ئەم دەستەواژە دۆستانەیە بەکاربهێنە.", "Dema diçî û hêvî dikî dîsa hev bibînin, vê gotina hevalane bi kar bîne.", "Użyj przy pożegnaniu, gdy spodziewasz się kolejnego spotkania.", "Folosește când pleci și te aștepți să vă revedeți.", "Përdore kur largohesh dhe pret të takoheni përsëri."), ["See you later, Anna.", "Thank you. See you later."]),
        (19, "have-a-nice-day", "Have a nice day.", t("Have a nice day.", "روز خوبی داشته باشید.", "أتمنى لك يوماً سعيداً.", "İyi günler.", "Хорошего дня.", "ڕۆژێکی خۆشت هەبێت.", "Rojeke xweş be.", "Miłego dnia.", "O zi frumoasă.", "Kalofsh një ditë të mirë."), t("Use this polite closing at the end of a short everyday exchange.", "در پایان یک گفت‌وگوی کوتاه روزمره، از این پایان‌بندی مودبانه استفاده کن.", "استخدم هذه الخاتمة المهذبة في نهاية تبادل قصير يومي.", "Kısa günlük bir konuşmanın sonunda bu kibar kapanışı kullan.", "Используйте это вежливое завершение в конце короткого повседневного обмена.", "لە کۆتایی گفتوگۆیەکی کورتی ڕۆژانەدا ئەم کۆتاییە ڕێزدارانەیە بەکاربهێنە.", "Di dawîya danûstandineke kurt a rojane de vê dawiyê ya bi rêz bi kar bîne.", "Użyj tego uprzejmego zakończenia na końcu krótkiej codziennej rozmowy.", "Folosește această formulă politicoasă la finalul unui schimb scurt de zi cu zi.", "Përdore këtë mbyllje të sjellshme në fund të një shkëmbimi të shkurtër të përditshëm."), ["Thank you. Have a nice day.", "Have a nice day!"]),
        (20, "one-moment-please", "One moment, please.", t("One moment, please.", "یک لحظه لطفاً.", "لحظة واحدة من فضلك.", "Bir dakika lütfen.", "Один момент, пожалуйста.", "تکایە چاوەڕێیەک.", "Demekê, ji kerema xwe.", "Chwileczkę, proszę.", "Un moment, vă rog.", "Një moment, ju lutem."), t("Use this when you need a little time before you answer or act.", "وقتی قبل از پاسخ دادن یا انجام کاری کمی زمان می‌خواهی، از این عبارت استفاده کن.", "استخدمها عندما تحتاج إلى وقت قصير قبل أن تجيب أو تتصرف.", "Cevap vermeden veya bir şey yapmadan önce biraz zamana ihtiyacın olduğunda kullan.", "Используйте, когда нужно немного времени перед ответом или действием.", "کاتێک پێش وەڵامدانەوە یان کردنی شتێک کەمێک کاتت دەوێت، بەکاری بهێنە.", "Dema berî bersivdan an kirinekê demeke kurt dixwazî, bi kar bîne.", "Użyj, gdy potrzebujesz chwili przed odpowiedzią albo działaniem.", "Folosește când ai nevoie de puțin timp înainte să răspunzi sau să acționezi.", "Përdore kur të duhet pak kohë para se të përgjigjesh ose të veprosh."), ["One moment, please.", "One moment, please. I need my book."]),
    ]
    for args in expression_data:
        upsert(package["expressionEntries"], expression_entry(*args))

    new_exercises = [
        exercise("en-a1-reorder-a-simple-question", "Reorder a simple question", "Put the words in the correct order.", {"segments": ["you", "Are", "in", "class", "?"]}, {"orderedSegments": ["Are", "you", "in", "class", "?"]}, "Are you in class? is the correct question order.", "The question needs Are before you.", "In a question with be, be comes first.", "grammar-topic", "en-a1-simple-questions-with-be", 50, exercise_type="sentence-ordering"),
        exercise("en-a1-identify-singular-and-plural-nouns", "Identify singular and plural nouns", "Match each noun phrase to one thing or more than one.", {"pairs": [{"left": "a book", "right": "one thing"}, {"left": "two pens", "right": "more than one"}]}, {"pairs": [{"left": "a book", "right": "one thing"}, {"left": "two pens", "right": "more than one"}]}, "A book is one thing; two pens is more than one.", "Look at a/an and the number before the noun.", "A/an usually marks one countable noun.", "grammar-topic", "en-a1-a-an-and-plural-nouns", 60, exercise_type="matching"),
        exercise("en-a1-complete-a-short-introduction", "Complete a short introduction", "Choose the missing words for the short introduction.", {"stem": "Hello, my name is Sara. I ___ from Iran. I live ___ Hamburg.", "options": [{"id": "am-in", "text": "am / in"}, {"id": "are-at", "text": "are / at"}, {"id": "is-on", "text": "is / on"}]}, {"correctOptionIds": ["am-in"]}, "I am from Iran and I live in Hamburg are natural A1 sentences.", "Use I am for yourself and live in for a city.", "Who is speaking, and where does the person live?", "course-lesson", "en-a1-build-a-short-introduction", 70),
        exercise("en-a1-review-first-contact-phrases", "Review first-contact phrases", "Choose the phrase that fits the situation.", {"stem": "You did not understand. What can you say?", "options": [{"id": "repeat", "text": "Can you repeat that?"}, {"id": "price", "text": "How much is it?"}, {"id": "later", "text": "See you later."}]}, {"correctOptionIds": ["repeat"]}, "Can you repeat that? asks the other person to say it again.", "The other phrases belong to price or goodbye situations.", "You need the person to say the sentence again.", "expression", "can-you-repeat-that", 80, target_skill="listening"),
    ]
    for item in new_exercises:
        upsert(package["exercises"], item)

    writing_templates = [
        writing_template("en-a1-simple-appointment-request", "Simple appointment request", "A short message to ask for a simple appointment.", "You want to ask a teacher or office for a simple appointment.", "Hello,\ncan I have an appointment on {{day}} at {{time}}?\nThank you,\n{{name}}", "The message asks for one appointment and gives one day and time.", ["day", "time", "name"], "Hello,\ncan I have an appointment on Tuesday at 10 a.m.?\nThank you,\nSara", 30, "en-a1-polite-you-and-basic-requests"),
        writing_template("en-a1-short-apology-message", "Short apology message", "A short message to say sorry for a small problem.", "You are late or cannot come and need a short polite message.", "Hello,\nsorry, I am late today. I am in room {{room}} at {{time}}.\nThank you,\n{{name}}", "The message says sorry and gives the most important detail.", ["room", "time", "name"], "Hello,\nsorry, I am late today. I am in room 12 at 9:15.\nThank you,\nSara", 40, "en-a1-polite-you-and-basic-requests"),
        writing_template("en-a1-simple-thank-you-message", "Simple thank-you message", "A short message to say thank you after help.", "Someone helped you and you want to write a simple thank-you message.", "Hello {{name}},\nthank you for your help.\nHave a nice day.\n{{your-name}}", "The message is friendly, short, and suitable after simple help.", ["name", "your-name"], "Hello Anna,\nthank you for your help.\nHave a nice day.\nSara", 50, "en-a1-first-contact-review"),
    ]
    for item in writing_templates:
        upsert(package["writingTemplates"], item)

    # Update the cumulative exercise set to include the full A1 first-contact pilot exercises.
    for item in package["exerciseSets"]:
        if item["slug"] == "en-a1-first-contact-practice":
            item["exerciseSlugs"] = [exercise_item["slug"] for exercise_item in sorted(package["exercises"], key=lambda e: e["sortOrder"])]
            item["description"] = "A short set for greetings, questions, introductions, objects, and first-contact repair phrases."
            item["descriptionTranslations"] = tr(
                t(
                    item["description"],
                    "یک مجموعه کوتاه برای سلام، پرسش‌ها، معرفی، چیزها و عبارت‌های اصلاح ارتباط اول.",
                    "مجموعة قصيرة للتحيات والأسئلة والتعريف بالنفس والأشياء وعبارات إصلاح التواصل الأول.",
                    "Selamlar, sorular, tanıtma, eşyalar ve ilk temas onarım ifadeleri için kısa bir set.",
                    "Короткий набор для приветствий, вопросов, представления, предметов и фраз для восстановления первого контакта.",
                    "کۆمەڵەیەکی کورت بۆ سڵاو، پرسیار، خۆناسێنان، شت و دەستەواژەکانی چاککردنەوەی یەکەم پەیوەندی.",
                    "Komeke kurt ji bo silav, pirs, nasandin, tişt û gotinên rastkirina têkiliya yekem.",
                    "Krótki zestaw na powitania, pytania, przedstawienie się, przedmioty i zwroty naprawcze w pierwszym kontakcie.",
                    "Un set scurt pentru saluturi, întrebări, prezentare, obiecte și expresii de reparare a primului contact.",
                    "Një grup i shkurtër për përshëndetje, pyetje, prezantim, sende dhe shprehje korrigjuese në kontaktin e parë.",
                )
            )

    package["courseLessons"] = sorted(package["courseLessons"], key=lambda item: item["sortOrder"])
    package["grammarTopics"] = sorted(package["grammarTopics"], key=lambda item: item["sortOrder"])
    package["expressionEntries"] = sorted(package["expressionEntries"], key=lambda item: item["sortOrder"])
    package["exercises"] = sorted(package["exercises"], key=lambda item: item["sortOrder"])
    package["writingTemplates"] = sorted(package["writingTemplates"], key=lambda item: item["sortOrder"])

    PACKAGE.write_text(json.dumps(package, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print(PACKAGE)


if __name__ == "__main__":
    main()

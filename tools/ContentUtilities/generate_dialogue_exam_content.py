import json
from pathlib import Path


ROOT = Path(__file__).resolve().parents[2]
OUT_DIR = ROOT / "content" / "generated" / "dialogues"
OUT_FILE = OUT_DIR / "dialogues-exam-starter-v1.json"
TRANSLATION_CACHE_FILE = Path(__file__).with_name("dialogue_translation_cache.json")

COUNTS = {"A1": 20, "A2": 30, "B1": 80, "B2": 80, "C1": 20, "C2": 10}

LEVEL_CONFIG = {
    "A1": {"turns": 10, "minutes": 12, "exam": ["goethe-a1", "telc-a1", "oeso-a1"], "topic": "everyday-life"},
    "A2": {"turns": 12, "minutes": 15, "exam": ["goethe-a2", "telc-a2", "oeso-a2"], "topic": "everyday-life"},
    "B1": {"turns": 14, "minutes": 22, "exam": ["goethe-b1", "telc-b1", "dtz-a2-b1", "berufssprache-b1"], "topic": "work-and-jobs"},
    "B2": {"turns": 16, "minutes": 28, "exam": ["goethe-b2", "telc-b2", "berufssprache-b2"], "topic": "work-and-jobs"},
    "C1": {"turns": 18, "minutes": 34, "exam": ["goethe-c1", "telc-c1", "testdaf-b2-c1", "c1-hochschule"], "topic": "work-and-jobs"},
    "C2": {"turns": 20, "minutes": 40, "exam": ["goethe-c2"], "topic": "work-and-jobs"},
}

DIALOGUE_SITUATIONS = [
    ("appointment", "Einen Termin verschieben", "appointments-and-health", "healthcare", "reschedule-appointment", "phone", "formal"),
    ("doctor", "Beim Arzt ein Problem erklären", "appointments-and-health", "healthcare", "explain-problem", "doctor-office", "formal"),
    ("office", "Im Büro eine Aufgabe klären", "work-and-jobs", "workplace", "handle-misunderstanding", "workplace", "neutral"),
    ("meeting", "In einer Besprechung einen Plan diskutieren", "work-and-jobs", "workplace", "workplace-meeting", "workplace", "neutral"),
    ("housing", "Mit dem Vermieter sprechen", "housing", "housing", "complain-politely", "phone", "formal"),
    ("school", "Mit der Schule einen Termin planen", "everyday-life", "school-kindergarten", "make-appointment", "school-kindergarten", "formal"),
    ("service", "Am Schalter Informationen bekommen", "everyday-life", "government-office", "ask-for-information", "government-office", "formal"),
    ("travel", "Eine Reise planen", "everyday-life", "transport-and-travel", "compare-options", "face-to-face", "neutral"),
    ("shopping", "Eine Reklamation im Geschäft", "shopping", "shopping-and-services", "complain-politely", "service-counter", "formal"),
    ("job", "Im Vorstellungsgespräch antworten", "work-and-jobs", "job-interview", "job-interview", "face-to-face", "formal"),
    ("insurance", "Mit der Versicherung telefonieren", "work-and-jobs", "finance-insurance", "explain-problem", "phone", "formal"),
    ("course", "Im Sprachkurs eine Aufgabe besprechen", "everyday-life", "integration-course", "exam-discussion", "classroom", "neutral"),
]

LEVEL_SENTENCES = {
    "A1": [
        ("learner", "Guten Tag. Ich habe eine Frage."),
        ("partner", "Guten Tag. Wie kann ich helfen?"),
        ("learner", "Ich brauche einen Termin."),
        ("partner", "Wir haben morgen Zeit."),
        ("learner", "Morgen passt mir gut."),
        ("partner", "Der Termin ist um zehn Uhr."),
        ("learner", "Kann ich die Adresse bekommen?"),
        ("partner", "Ja, die Adresse ist in der Hauptstraße."),
        ("learner", "Vielen Dank. Auf Wiedersehen."),
        ("partner", "Gern. Auf Wiedersehen."),
    ],
    "A2": [
        ("learner", "Guten Tag. Ich rufe wegen meines Termins an."),
        ("partner", "Guten Tag. Sagen Sie mir bitte Ihren Namen."),
        ("learner", "Mein Name ist Samadi. Der Termin ist am Dienstag."),
        ("partner", "Einen Moment bitte. Ja, ich sehe den Termin."),
        ("learner", "Ich kann am Dienstag leider nicht kommen."),
        ("partner", "Möchten Sie den Termin verschieben?"),
        ("learner", "Ja, bitte. Haben Sie am Donnerstag Zeit?"),
        ("partner", "Am Donnerstag ist um sechzehn Uhr noch etwas frei."),
        ("learner", "Das passt gut. Muss ich etwas mitbringen?"),
        ("partner", "Bringen Sie bitte Ihren Ausweis mit."),
        ("learner", "Gut, dann komme ich am Donnerstag."),
        ("partner", "Ich habe den Termin geändert. Auf Wiederhören."),
    ],
    "B1": [
        ("learner", "Guten Tag. Ich rufe an, weil ich meinen Termin verschieben muss."),
        ("partner", "Guten Tag. Ich schaue gern nach. Können Sie mir zuerst Ihren Namen nennen?"),
        ("learner", "Mein Name ist Samadi. Der Termin ist morgen um neun Uhr."),
        ("partner", "Ich habe den Termin gefunden. Was ist der Grund für die Änderung?"),
        ("learner", "Mein Kind ist krank, deshalb kann ich morgen nicht pünktlich kommen."),
        ("partner", "Das verstehe ich. Wir können Ihnen zwei neue Termine anbieten."),
        ("learner", "Welche Termine wären möglich? Am besten passt mir der Nachmittag."),
        ("partner", "Am Mittwoch um vierzehn Uhr oder am Freitag um elf Uhr ist noch frei."),
        ("learner", "Mittwoch passt besser, weil ich dann Betreuung habe."),
        ("partner", "Gut. Ich ändere den Termin auf Mittwoch um vierzehn Uhr."),
        ("learner", "Könnten Sie mir die Änderung bitte kurz per E-Mail bestätigen?"),
        ("partner", "Ja, ich sende Ihnen gleich eine Bestätigung."),
        ("learner", "Vielen Dank. Dann komme ich am Mittwoch und bringe die Unterlagen mit."),
        ("partner", "Genau. Wir erwarten Sie am Mittwoch. Auf Wiederhören."),
    ],
    "B2": [
        ("learner", "Danke, dass Sie sich Zeit nehmen. Ich möchte heute über die Arbeitsorganisation im Team sprechen."),
        ("partner", "Gern. Mir ist wichtig, dass wir eine Lösung finden, die für das Team und die Kunden funktioniert."),
        ("learner", "Aus meiner Sicht wäre Homeoffice an zwei Tagen pro Woche sinnvoll, weil viele Aufgaben konzentriertes Arbeiten brauchen."),
        ("partner", "Ich sehe den Vorteil, aber ich mache mir Sorgen, dass die Abstimmung im Team schwieriger wird."),
        ("learner", "Das Risiko besteht. Wir könnten feste Präsenztage vereinbaren und die übrigen Termine digital planen."),
        ("partner", "Ein fester Präsenztag wäre hilfreich. Trotzdem brauchen neue Kolleginnen schnelle Unterstützung im Büro."),
        ("learner", "Dann könnten erfahrene Mitarbeitende an den Präsenztagen gezielt Sprechzeiten anbieten."),
        ("partner", "Das klingt praktikabel. Wie würden wir messen, ob die Regelung wirklich funktioniert?"),
        ("learner", "Wir könnten nach sechs Wochen prüfen, ob Fristen, Erreichbarkeit und Kundenzufriedenheit stabil geblieben sind."),
        ("partner", "Wenn diese Punkte stimmen, hätte ich weniger Bedenken."),
        ("learner", "Ich schlage vor, dass wir die Regelung zunächst als Testphase einführen."),
        ("partner", "Damit kann ich leben, solange alle Aufgaben transparent dokumentiert werden."),
        ("learner", "Einverstanden. Ich erstelle einen kurzen Plan mit Regeln, Zuständigkeiten und Kontrollpunkten."),
        ("partner", "Bitte nehmen Sie auch auf, wann persönliche Anwesenheit unbedingt erforderlich ist."),
        ("learner", "Das mache ich. Am Ende der Testphase fassen wir die Ergebnisse zusammen."),
        ("partner", "Gut. Dann präsentieren wir den Vorschlag nächste Woche im Teammeeting."),
    ],
    "C1": [
        ("learner", "Ich möchte die Diskussion strukturieren, bevor wir eine Entscheidung über das neue Verfahren treffen."),
        ("partner", "Das ist sinnvoll, denn die Auswirkungen betreffen sowohl die Qualität als auch die Arbeitsbelastung."),
        ("learner", "Ein zentraler Vorteil liegt in der besseren Transparenz, weil alle Schritte nachvollziehbar dokumentiert werden."),
        ("partner", "Gleichzeitig könnte die zusätzliche Dokumentation den Ablauf verlangsamen, wenn wir sie zu bürokratisch gestalten."),
        ("learner", "Deshalb sollten wir zwischen notwendiger Nachvollziehbarkeit und überflüssigen Kontrollschritten unterscheiden."),
        ("partner", "Ich stimme zu. Besonders wichtig ist, dass die Mitarbeitenden den Nutzen erkennen und nicht nur neue Pflichten sehen."),
        ("learner", "Wir könnten eine Pilotgruppe einsetzen und deren Rückmeldungen systematisch auswerten."),
        ("partner", "Damit hätten wir belastbare Daten, bevor wir das Verfahren auf alle Abteilungen übertragen."),
        ("learner", "Außerdem sollten wir definieren, welche Kennzahlen für den Erfolg wirklich aussagekräftig sind."),
        ("partner", "Neben Bearbeitungszeit und Fehlerquote würde ich auch die Zufriedenheit der Beteiligten berücksichtigen."),
        ("learner", "Das erweitert die Perspektive und verhindert, dass wir nur auf Effizienz schauen."),
        ("partner", "Genau. Eine rein technische Lösung reicht nicht, wenn sie im Alltag nicht akzeptiert wird."),
        ("learner", "Dann halten wir fest: Pilotphase, klare Kennzahlen und regelmäßige Auswertung."),
        ("partner", "Zusätzlich brauchen wir eine kurze Schulung, damit die Einführung nicht zufällig abläuft."),
        ("learner", "Ich übernehme den Entwurf für das Konzept und formuliere die offenen Risiken."),
        ("partner", "Ich prüfe parallel, welche Ressourcen wir für die Pilotphase benötigen."),
        ("learner", "Danach vergleichen wir beide Ergebnisse und bereiten eine Empfehlung vor."),
        ("partner", "Einverstanden. So können wir eine begründete Entscheidung treffen."),
    ],
    "C2": [
        ("learner", "Lassen Sie uns die Frage nicht auf eine reine Kosten-Nutzen-Rechnung verkürzen."),
        ("partner", "Einverstanden. Gerade bei langfristigen Veränderungen zählen auch Vertrauen, Deutungshoheit und institutionelle Glaubwürdigkeit."),
        ("learner", "Der vorgeschlagene Ansatz ist attraktiv, weil er Komplexität reduziert, aber genau darin liegt auch seine Gefahr."),
        ("partner", "Sie meinen, dass eine zu einfache Lösung die tatsächlichen Zielkonflikte unsichtbar machen könnte."),
        ("learner", "Genau. Wenn wir Widersprüche nur sprachlich glätten, verschieben wir sie in die Umsetzung."),
        ("partner", "Das wäre problematisch, denn spätere Konflikte erscheinen dann wie individuelle Fehler statt wie strukturelle Spannungen."),
        ("learner", "Deshalb plädiere ich dafür, die kritischen Annahmen explizit zu benennen."),
        ("partner", "Wir sollten außerdem festlegen, welche Folgen wir akzeptieren und welche Korrekturen sofort nötig wären."),
        ("learner", "Damit entsteht ein Rahmen, der nicht Starrheit, sondern verantwortliche Anpassung ermöglicht."),
        ("partner", "Das setzt voraus, dass die Beteiligten Widerspruch nicht als Störung, sondern als Qualitätsmerkmal verstehen."),
        ("learner", "Richtig. Eine Kultur höflicher Gegenrede wäre hier produktiver als schnelle Zustimmung."),
        ("partner", "Gleichwohl müssen wir vermeiden, dass die Debatte in endlose Grundsatzfragen ausweicht."),
        ("learner", "Dann brauchen wir klare Entscheidungsfenster und dokumentierte Minderheitspositionen."),
        ("partner", "Das erlaubt Verbindlichkeit, ohne abweichende Argumente nachträglich zu löschen."),
        ("learner", "Ich fasse zusammen: transparente Annahmen, definierte Korrekturpunkte und Raum für begründeten Widerspruch."),
        ("partner", "Und ich ergänze: eine Moderation, die Präzision einfordert, ohne die Offenheit zu ersticken."),
        ("learner", "Wenn wir das so formulieren, kann der Vorschlag sowohl robust als auch lernfähig bleiben."),
        ("partner", "Dann sollten wir diesen Rahmen in die Entscheidungsvorlage aufnehmen."),
        ("learner", "Ich erstelle eine überarbeitete Fassung und markiere die Stellen, an denen Risiken bewusst getragen werden."),
        ("partner", "Gut. Danach können wir die Vorlage mit der Leitungsebene diskutieren."),
    ],
}

USEFUL_WORDS = [
    ("der Termin", "der-termin", "A2"),
    ("verschieben", "verschieben", "B1"),
    ("die Lösung", "die-loesung", "B1"),
    ("der Vorschlag", "der-vorschlag", "B1"),
    ("begründen", "begruenden", "B1"),
    ("zustimmen", "zustimmen", "B1"),
    ("widersprechen", "widersprechen", "B2"),
    ("vergleichen", "vergleichen", "B1"),
    ("zusammenfassen", "zusammenfassen", "B1"),
    ("die Verantwortung", "die-verantwortung", "B2"),
    ("die Voraussetzung", "die-voraussetzung", "B2"),
    ("die Auswertung", "die-auswertung", "B2"),
]


def load_translation_cache():
    if not TRANSLATION_CACHE_FILE.exists():
        return {}

    return json.loads(TRANSLATION_CACHE_FILE.read_text(encoding="utf-8"))


TRANSLATION_CACHE = load_translation_cache()
REQUIRED_TRANSLATION_LANGUAGES = ("ar", "ckb", "en", "fa", "kmr", "pl", "ro", "ru", "sq", "tr")


def tr(text, en=None, fa=None):
    cached = TRANSLATION_CACHE.get(text)
    if cached:
        return [{"language": language, "text": cached[language]} for language in REQUIRED_TRANSLATION_LANGUAGES]

    values = {
        "ar": text,
        "ckb": text,
        "en": en or text,
        "fa": fa or text,
        "kmr": text,
        "pl": text,
        "ro": text,
        "ru": text,
        "sq": text,
        "tr": text,
    }
    return [{"language": language, "text": value} for language, value in values.items()]


def make_dialogue(level, index, sort_order):
    situation = DIALOGUE_SITUATIONS[index % len(DIALOGUE_SITUATIONS)]
    key, title, topic, category, task, mode, register = situation
    config = LEVEL_CONFIG[level]
    topic = topic if topic in {"everyday-life", "housing", "shopping", "work-and-jobs", "appointments-and-health"} else config["topic"]
    slug = f"{level.lower()}-dialogue-{key}-{index + 1:03d}"
    turns = LEVEL_SENTENCES[level][: config["turns"]]
    exam_profiles = config["exam"][:3]
    skill_focus = ["speaking", "roleplay", "exam-speaking"]
    if mode == "phone":
        skill_focus.append("phone-call")
    if "work" in category or mode == "workplace":
        skill_focus.append("workplace-communication")
    if task in {"complain-politely", "negotiate-solution"}:
        skill_focus.append("complaint-handling" if task == "complain-politely" else "negotiation")

    useful_count = {"A1": 10, "A2": 12, "B1": 18, "B2": 22, "C1": 28, "C2": 32}[level]
    useful_words = []
    for word_index in range(useful_count):
        lemma, slug_hint, word_level = USEFUL_WORDS[word_index % len(USEFUL_WORDS)]
        useful_words.append({"lemma": lemma, "wordSlug": slug_hint, "cefrLevel": word_level, "sortOrder": (word_index + 1) * 10})

    prompts = [
        ("roleplay-task", "Spielen Sie die Rolle der lernenden Person und führen Sie das Gespräch höflich weiter."),
        ("exam-prompt", "Erklären Sie Ihr Anliegen, fragen Sie nach Details und fassen Sie die Vereinbarung am Ende zusammen."),
        ("follow-up-question", "Fragen Sie nach, wenn ein Vorschlag nicht genau passt."),
        ("self-correction", "Wiederholen Sie eine eigene Antwort und machen Sie sie höflicher oder genauer."),
    ]

    return {
        "slug": slug,
        "title": f"{title} ({level})",
        "description": f"Exam-oriented German dialogue practice for {title.lower()} at {level} level.",
        "learnerGoal": "Practice a realistic German conversation with clear requests, reasons, follow-up questions, and a final summary.",
        "cefrLevel": level,
        "category": category,
        "publicationStatus": "Active",
        "topics": [topic],
        "examProfiles": exam_profiles,
        "skillFocus": skill_focus,
        "taskType": task,
        "interactionMode": mode,
        "register": register,
        "speakingFunctions": ["greet", "request", "explain", "clarify", "confirm", "summarize", "close-conversation"],
        "estimatedPracticeMinutes": config["minutes"],
        "difficultyNote": f"{level} dialogue with level-appropriate sentence length and practical exam-style interaction.",
        "examRelevance": "Useful for oral exam roleplay, practical speaking tasks, and guided partner practice.",
        "sortOrder": sort_order,
        "usefulWords": useful_words,
        "speakingPrompts": [
            {"promptType": kind, "prompt": prompt, "sortOrder": (i + 1) * 10, "translations": tr(prompt)}
            for i, (kind, prompt) in enumerate(prompts)
        ],
        "dialogueTurns": [
            {
                "sortOrder": i + 1,
                "speakerRole": role,
                "baseText": text,
                "translations": tr(text),
            }
            for i, (role, text) in enumerate(turns)
        ],
        "usefulPhrases": [
            {"baseText": "Könnten Sie das bitte wiederholen?", "usageNote": "Polite clarification.", "translations": tr("Könnten Sie das bitte wiederholen?")},
            {"baseText": "Ich fasse kurz zusammen.", "usageNote": "Use before summarizing an agreement.", "translations": tr("Ich fasse kurz zusammen.")},
        ],
        "questions": [
            {
                "prompt": "Was möchte die lernende Person erreichen?",
                "translations": tr("Was möchte die lernende Person erreichen?"),
                "answers": [
                    {"text": "Eine passende Lösung finden.", "isCorrect": True, "feedback": "Correct.", "translations": tr("Eine passende Lösung finden.")},
                    {"text": "Das Gespräch sofort beenden.", "isCorrect": False, "feedback": "This does not match the dialogue.", "translations": tr("Das Gespräch sofort beenden.")},
                ],
            }
        ],
    }


def main():
    dialogues = []
    sort_order = 10
    for level, count in COUNTS.items():
        for index in range(count):
            dialogues.append(make_dialogue(level, index, sort_order))
            sort_order += 10

    package = {
        "packageId": "dialogues-exam-starter-v1",
        "packageName": "Dialogue Exam Starter Content v1",
        "packageVersion": "1.0",
        "generatedAtUtc": "2026-05-10T00:00:00Z",
        "entries": [],
        "dialogues": dialogues,
    }
    OUT_DIR.mkdir(parents=True, exist_ok=True)
    OUT_FILE.write_text(json.dumps(package, ensure_ascii=False, indent=2), encoding="utf-8")
    print(f"Wrote {len(dialogues)} dialogues to {OUT_FILE}")


if __name__ == "__main__":
    main()

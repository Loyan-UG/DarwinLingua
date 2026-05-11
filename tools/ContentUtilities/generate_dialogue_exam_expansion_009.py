import json
from pathlib import Path

import generate_dialogue_exam_content as base


ROOT = Path(__file__).resolve().parents[2]
OUT_DIR = ROOT / "content" / "generated" / "dialogues"
OUT_FILE = OUT_DIR / "dialogues-exam-expansion-009-b1-b2.json"

DIALOGUE_SITUATIONS = [
    ("train-platform-change", "Einen Gleiswechsel am Bahnhof klären", "everyday-life", "transport-and-travel", "ask-for-information", "service-counter", "formal"),
    ("bus-ticket-refund", "Eine Fahrkarte wegen Verspätung erstatten lassen", "everyday-life", "transport-and-travel", "complain-politely", "service-counter", "formal"),
    ("airport-checkin", "Ein Problem beim Check-in erklären", "everyday-life", "transport-and-travel", "explain-problem", "service-counter", "formal"),
    ("hotel-room-change", "Ein anderes Hotelzimmer erbitten", "everyday-life", "transport-and-travel", "ask-for-help", "service-counter", "formal"),
    ("housing-key", "Eine Schlüsselübergabe organisieren", "housing", "housing", "make-appointment", "phone", "formal"),
    ("housing-mold", "Schimmel in der Wohnung melden", "housing", "housing", "explain-problem", "phone", "formal"),
    ("housing-utilities", "Nebenkosten mit der Verwaltung besprechen", "housing", "housing", "ask-for-information", "phone", "formal"),
    ("housing-moving-help", "Hilfe beim Umzug organisieren", "housing", "social-life", "discuss-plan", "group-work", "neutral"),
    ("clinic-insurance", "Versicherungsdaten in der Klinik klären", "appointments-and-health", "healthcare", "ask-for-information", "doctor-office", "formal"),
    ("doctor-medication", "Medikamente beim Arzt vergleichen", "appointments-and-health", "healthcare", "compare-options", "doctor-office", "formal"),
    ("doctor-work-note", "Eine Bescheinigung für den Arbeitgeber erfragen", "appointments-and-health", "healthcare", "ask-for-information", "doctor-office", "formal"),
    ("pharmacy-substitute", "Ein Ersatzmedikament in der Apotheke besprechen", "appointments-and-health", "healthcare", "compare-options", "service-counter", "formal"),
    ("office-permit", "Eine Arbeitserlaubnis beim Amt erfragen", "everyday-life", "government-office", "ask-for-information", "government-office", "formal"),
    ("office-copy", "Eine beglaubigte Kopie beantragen", "everyday-life", "government-office", "ask-for-information", "government-office", "formal"),
    ("office-missing-letter", "Einen fehlenden Brief vom Amt melden", "everyday-life", "government-office", "explain-problem", "phone", "formal"),
    ("office-deadline", "Eine Fristverlängerung beantragen", "everyday-life", "government-office", "reschedule-appointment", "phone", "formal"),
    ("digital-authenticator", "Eine Authenticator-App einrichten", "everyday-life", "digital-services", "ask-for-help", "phone", "formal"),
    ("digital-error-code", "Einen Fehlercode beim Support erklären", "everyday-life", "digital-services", "explain-problem", "phone", "formal"),
    ("digital-document-upload", "Dokumente in einem Portal hochladen", "everyday-life", "digital-services", "ask-for-help", "video-call", "formal"),
    ("digital-notification", "Benachrichtigungen in einer App einstellen", "everyday-life", "digital-services", "ask-for-information", "phone", "neutral"),
    ("family-school-trip", "Kosten für eine Klassenfahrt in der Familie besprechen", "everyday-life", "family", "discuss-plan", "face-to-face", "mixed"),
    ("family-care-appointment", "Einen Termin für Angehörige organisieren", "appointments-and-health", "family", "make-appointment", "phone", "formal"),
    ("friend-misunderstanding", "Ein Missverständnis mit Freunden klären", "everyday-life", "social-life", "handle-misunderstanding", "face-to-face", "informal"),
    ("neighbor-package", "Ein Paket bei Nachbarn abholen", "housing", "social-life", "ask-for-information", "face-to-face", "neutral"),
    ("work-expense", "Eine Auslage im Büro abrechnen", "work-and-jobs", "finance-insurance", "ask-for-information", "workplace", "formal"),
    ("work-client-call", "Ein Kundentelefonat vorbereiten", "work-and-jobs", "workplace", "discuss-plan", "workplace", "formal"),
    ("work-delay-report", "Eine Verzögerung im Projekt melden", "work-and-jobs", "workplace", "explain-problem", "workplace", "neutral"),
    ("exam-travel-role", "Eine Reiseaufgabe in der Prüfung spielen", "everyday-life", "exam-preparation", "exam-roleplay", "exam-room", "formal"),
    ("exam-health-role", "Eine Gesundheitssituation in der Prüfung spielen", "appointments-and-health", "exam-preparation", "exam-roleplay", "exam-room", "formal"),
    ("exam-admin-role", "Eine Amtssituation in der Prüfung spielen", "everyday-life", "exam-preparation", "exam-roleplay", "exam-room", "formal"),
]

TASK_TO_FUNCTIONS = {
    "ask-for-information": ["greet", "ask-question", "clarify", "confirm", "summarize", "close-conversation"],
    "complain-politely": ["greet", "explain", "complain", "request", "negotiate", "summarize", "close-conversation"],
    "explain-problem": ["greet", "explain", "clarify", "answer-question", "confirm", "summarize", "close-conversation"],
    "ask-for-help": ["greet", "request", "explain", "clarify", "confirm", "close-conversation"],
    "make-appointment": ["greet", "request", "ask-question", "confirm", "close-conversation"],
    "discuss-plan": ["greet", "suggest", "compare", "agree", "summarize", "close-conversation"],
    "compare-options": ["greet", "ask-question", "compare", "justify", "suggest", "summarize", "close-conversation"],
    "reschedule-appointment": ["greet", "request", "explain", "suggest", "confirm", "summarize", "close-conversation"],
    "handle-misunderstanding": ["greet", "clarify", "correct", "apologize", "confirm", "summarize", "close-conversation"],
    "exam-roleplay": ["greet", "request", "clarify", "suggest", "confirm", "close-conversation"],
}


def make_dialogue(level: str, situation_index: int, variant: int, sort_order: int) -> dict:
    key, title, topic, category, task, mode, register = DIALOGUE_SITUATIONS[situation_index]
    dialogue = base.make_dialogue(level, situation_index + 800, sort_order)
    dialogue["slug"] = f"{level.lower()}-dialogue-expansion-009-{key}-{variant:02d}"
    dialogue["title"] = f"{title} ({level})"
    dialogue["description"] = f"B1/B2 German dialogue for {title.lower()} with practical speaking and exam preparation focus."
    dialogue["learnerGoal"] = "Practice explaining the situation, asking precise follow-up questions, and confirming the next step."
    dialogue["category"] = category
    dialogue["topics"] = [topic if topic in {"everyday-life", "housing", "shopping", "work-and-jobs", "appointments-and-health"} else "everyday-life"]
    dialogue["taskType"] = task
    dialogue["interactionMode"] = mode
    dialogue["register"] = register
    dialogue["speakingFunctions"] = TASK_TO_FUNCTIONS[task]
    dialogue["sortOrder"] = sort_order
    dialogue["difficultyNote"] = f"{level} dialogue for a practical task: {title}."
    dialogue["examRelevance"] = "Useful for B1/B2 speaking exams, practical service conversations, and formal roleplay."

    skill_focus = ["speaking", "roleplay", "exam-speaking"]
    if mode == "phone":
        skill_focus.append("phone-call")
    if category == "workplace" or mode in {"workplace", "video-call"}:
        skill_focus.append("workplace-communication")
    if task == "complain-politely":
        skill_focus.append("complaint-handling")
    if category == "exam-preparation":
        skill_focus.append("discussion")
    dialogue["skillFocus"] = skill_focus
    return dialogue


def main() -> None:
    dialogues = []
    sort_order = 180000
    for level in ("B1", "B2"):
        for variant in range(1, 3):
            for situation_index in range(len(DIALOGUE_SITUATIONS)):
                dialogues.append(make_dialogue(level, situation_index, variant, sort_order))
                sort_order += 10

    package = {
        "packageId": "dialogues-exam-expansion-009-b1-b2",
        "packageName": "Dialogue Exam Expansion 009 B1 B2",
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

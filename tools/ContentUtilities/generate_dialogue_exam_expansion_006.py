import json
from pathlib import Path

import generate_dialogue_exam_content as base


ROOT = Path(__file__).resolve().parents[2]
OUT_DIR = ROOT / "content" / "generated" / "dialogues"
OUT_FILE = OUT_DIR / "dialogues-exam-expansion-006-b1-b2.json"

DIALOGUE_SITUATIONS = [
    ("city-waste", "Die Müllabholung bei der Stadt nachfragen", "everyday-life", "government-office", "ask-for-information", "phone", "formal"),
    ("library-card", "Einen Bibliotheksausweis beantragen", "everyday-life", "city-services", "ask-for-information", "service-counter", "formal"),
    ("citizen-office-delay", "Eine Verzögerung im Bürgerbüro klären", "everyday-life", "government-office", "explain-problem", "government-office", "formal"),
    ("lost-document", "Ein verlorenes Dokument melden", "everyday-life", "government-office", "explain-problem", "government-office", "formal"),
    ("address-change", "Eine neue Adresse mitteilen", "everyday-life", "government-office", "ask-for-help", "government-office", "formal"),
    ("work-introduction", "Sich im neuen Team vorstellen", "work-and-jobs", "workplace", "introduce-yourself", "workplace", "neutral"),
    ("handover-task", "Eine Übergabe bei der Arbeit besprechen", "work-and-jobs", "workplace", "workplace-meeting", "workplace", "neutral"),
    ("remote-meeting", "Ein Problem in einer Videokonferenz klären", "work-and-jobs", "workplace", "handle-misunderstanding", "video-call", "neutral"),
    ("customer-follow-up", "Bei einem Kunden höflich nachfassen", "work-and-jobs", "workplace", "ask-for-information", "phone", "formal"),
    ("team-suggestion", "Einen Verbesserungsvorschlag im Team machen", "work-and-jobs", "workplace", "make-suggestion", "workplace", "neutral"),
    ("candidate-question", "Im Vorstellungsgespräch Rückfragen stellen", "work-and-jobs", "job-interview", "ask-for-information", "face-to-face", "formal"),
    ("candidate-experience", "Berufserfahrung im Interview beschreiben", "work-and-jobs", "job-interview", "describe-experience", "face-to-face", "formal"),
    ("job-refusal", "Ein Jobangebot höflich ablehnen", "work-and-jobs", "job-interview", "refuse-politely", "phone", "formal"),
    ("exam-planning", "Eine Prüfungsvorbereitung zu zweit planen", "everyday-life", "exam-preparation", "discuss-plan", "pair-work", "neutral"),
    ("exam-role-delay", "Eine Verspätung in einer Rollenaufgabe erklären", "everyday-life", "exam-preparation", "exam-roleplay", "exam-room", "formal"),
    ("exam-compare-cities", "Zwei Wohnorte in der Prüfung vergleichen", "everyday-life", "exam-preparation", "compare-options", "exam-room", "formal"),
    ("exam-opinion-media", "Eine Meinung zu sozialen Medien begründen", "everyday-life", "exam-preparation", "give-opinion", "exam-room", "formal"),
    ("doctor-question", "Beim Arzt gezielt nachfragen", "appointments-and-health", "healthcare", "ask-for-information", "doctor-office", "formal"),
    ("doctor-second-opinion", "Eine zweite Meinung beim Arzt besprechen", "appointments-and-health", "healthcare", "compare-options", "doctor-office", "formal"),
    ("appointment-cancel", "Einen Arzttermin absagen", "appointments-and-health", "healthcare", "reschedule-appointment", "phone", "formal"),
    ("health-course", "Einen Gesundheitskurs auswählen", "appointments-and-health", "healthcare", "compare-options", "phone", "formal"),
    ("school-material", "Fehlendes Schulmaterial besprechen", "everyday-life", "school-kindergarten", "ask-for-help", "school-kindergarten", "formal"),
    ("teacher-meeting", "Ein Gespräch mit der Klassenleitung vereinbaren", "everyday-life", "school-kindergarten", "make-appointment", "phone", "formal"),
    ("child-conflict", "Einen Konflikt zwischen Kindern besprechen", "everyday-life", "school-kindergarten", "handle-misunderstanding", "school-kindergarten", "formal"),
    ("housing-viewing-refusal", "Eine Wohnung höflich absagen", "housing", "housing", "refuse-politely", "phone", "formal"),
    ("housing-rules", "Hausregeln mit der Verwaltung klären", "housing", "housing", "ask-for-information", "phone", "formal"),
    ("route-help", "Nach dem richtigen Weg fragen", "everyday-life", "transport-and-travel", "ask-for-directions", "face-to-face", "neutral"),
    ("ticket-machine", "Hilfe am Fahrkartenautomaten bekommen", "everyday-life", "transport-and-travel", "ask-for-help", "service-counter", "neutral"),
    ("restaurant-group", "Im Restaurant für eine Gruppe bestellen", "everyday-life", "shopping-and-services", "order-and-pay", "face-to-face", "neutral"),
    ("family-invite", "Eine Einladung in der Familie ablehnen", "everyday-life", "family", "refuse-politely", "phone", "informal"),
]

TASK_TO_FUNCTIONS = {
    "ask-for-information": ["greet", "ask-question", "clarify", "confirm", "summarize", "close-conversation"],
    "explain-problem": ["greet", "explain", "clarify", "answer-question", "confirm", "summarize", "close-conversation"],
    "ask-for-help": ["greet", "request", "explain", "clarify", "confirm", "close-conversation"],
    "introduce-yourself": ["greet", "introduce", "describe", "answer-question", "close-conversation"],
    "workplace-meeting": ["greet", "explain", "suggest", "compare", "confirm", "summarize", "close-conversation"],
    "handle-misunderstanding": ["greet", "clarify", "correct", "apologize", "confirm", "summarize", "close-conversation"],
    "make-suggestion": ["greet", "suggest", "explain", "justify", "agree", "summarize", "close-conversation"],
    "describe-experience": ["greet", "describe", "explain", "justify", "answer-question", "close-conversation"],
    "refuse-politely": ["greet", "explain", "apologize", "refuse-politely", "close-conversation"],
    "discuss-plan": ["greet", "suggest", "compare", "agree", "summarize", "close-conversation"],
    "exam-roleplay": ["greet", "request", "clarify", "suggest", "confirm", "close-conversation"],
    "compare-options": ["greet", "ask-question", "compare", "justify", "suggest", "summarize", "close-conversation"],
    "give-opinion": ["greet", "explain", "justify", "agree", "disagree", "summarize"],
    "reschedule-appointment": ["greet", "request", "explain", "suggest", "confirm", "summarize", "close-conversation"],
    "make-appointment": ["greet", "request", "ask-question", "confirm", "close-conversation"],
    "ask-for-directions": ["greet", "ask-question", "clarify", "confirm", "close-conversation"],
    "order-and-pay": ["greet", "request", "ask-question", "confirm", "close-conversation"],
}


def normalize_functions(task: str) -> list[str]:
    values = TASK_TO_FUNCTIONS[task]
    return ["request" if value == "refuse-politely" else value for value in values]


def make_dialogue(level: str, situation_index: int, variant: int, sort_order: int) -> dict:
    key, title, topic, category, task, mode, register = DIALOGUE_SITUATIONS[situation_index]
    dialogue = base.make_dialogue(level, situation_index + 500, sort_order)
    dialogue["slug"] = f"{level.lower()}-dialogue-expansion-006-{key}-{variant:02d}"
    dialogue["title"] = f"{title} ({level})"
    dialogue["description"] = f"Practical German conversation for {title.lower()} with B1/B2 exam speaking practice."
    dialogue["learnerGoal"] = "Practice clear questions, polite reactions, reasons, and a final confirmation in a realistic dialogue."
    dialogue["category"] = category
    dialogue["topics"] = [topic if topic in {"everyday-life", "housing", "shopping", "work-and-jobs", "appointments-and-health"} else "everyday-life"]
    dialogue["taskType"] = task
    dialogue["interactionMode"] = mode
    dialogue["register"] = register
    dialogue["speakingFunctions"] = normalize_functions(task)
    dialogue["sortOrder"] = sort_order
    dialogue["difficultyNote"] = f"{level} practical speaking task: {title}."
    dialogue["examRelevance"] = "Useful for B1/B2 speaking exams, real-life service interactions, and workplace or administration roleplay."

    skill_focus = ["speaking", "roleplay", "exam-speaking"]
    if mode == "phone":
        skill_focus.append("phone-call")
    if category in {"workplace", "job-interview"} or mode in {"workplace", "video-call"}:
        skill_focus.append("workplace-communication")
    if task in {"give-opinion", "exam-roleplay"} or category == "exam-preparation":
        skill_focus.append("discussion")
    dialogue["skillFocus"] = skill_focus
    return dialogue


def main() -> None:
    dialogues = []
    sort_order = 120000
    for level in ("B1", "B2"):
        for variant in range(1, 3):
            for situation_index in range(len(DIALOGUE_SITUATIONS)):
                dialogues.append(make_dialogue(level, situation_index, variant, sort_order))
                sort_order += 10

    package = {
        "packageId": "dialogues-exam-expansion-006-b1-b2",
        "packageName": "Dialogue Exam Expansion 006 B1 B2",
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

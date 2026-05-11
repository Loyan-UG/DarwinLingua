import json
from pathlib import Path

import generate_dialogue_exam_content as base


ROOT = Path(__file__).resolve().parents[2]
OUT_DIR = ROOT / "content" / "generated" / "dialogues"
OUT_FILE = OUT_DIR / "dialogues-exam-expansion-012-b1-b2.json"

DIALOGUE_SITUATIONS = [
    ("city-water", "Eine Störung beim Wasseranschluss melden", "everyday-life", "city-services", "explain-problem", "phone", "formal"),
    ("city-recycling", "Recycling-Regeln bei der Stadt erfragen", "everyday-life", "city-services", "ask-for-information", "phone", "formal"),
    ("city-playground", "Ein Problem auf dem Spielplatz melden", "everyday-life", "city-services", "explain-problem", "phone", "formal"),
    ("city-public-office", "Den richtigen Ansprechpartner im Rathaus finden", "everyday-life", "city-services", "ask-for-information", "government-office", "formal"),
    ("education-evening-course", "Einen Abendkurs mit der Arbeit vereinbaren", "work-and-jobs", "education", "compare-options", "face-to-face", "neutral"),
    ("education-certificate-error", "Einen Fehler im Zertifikat korrigieren lassen", "everyday-life", "education", "handle-misunderstanding", "service-counter", "formal"),
    ("education-study-advice", "Eine Studienberatung nutzen", "everyday-life", "education", "ask-for-information", "face-to-face", "formal"),
    ("education-exam-fee", "Eine Prüfungsgebühr klären", "shopping", "education", "ask-for-information", "phone", "formal"),
    ("work-vacation-denied", "Einen abgelehnten Urlaubsantrag besprechen", "work-and-jobs", "work", "negotiate-solution", "workplace", "formal"),
    ("work-equipment", "Arbeitsmittel im Betrieb anfordern", "work-and-jobs", "work", "ask-for-help", "workplace", "formal"),
    ("work-health-safety", "Gesundheitsschutz am Arbeitsplatz ansprechen", "work-and-jobs", "work", "make-suggestion", "workplace", "formal"),
    ("work-procedure", "Eine neue Arbeitsanweisung verstehen", "work-and-jobs", "work", "ask-for-information", "workplace", "formal"),
    ("complaint-no-response", "Auf eine unbeantwortete Beschwerde nachfassen", "shopping", "complaint", "complain-politely", "phone", "formal"),
    ("complaint-double-charge", "Eine doppelte Abbuchung reklamieren", "shopping", "complaint", "complain-politely", "phone", "formal"),
    ("complaint-service-contract", "Einen Servicevertrag beanstanden", "shopping", "complaint", "complain-politely", "phone", "formal"),
    ("complaint-noisy-repair", "Lärm durch Bauarbeiten beanstanden", "housing", "complaint", "complain-politely", "phone", "formal"),
    ("conflict-deadline", "Unterschiedliche Erwartungen zu einer Frist klären", "work-and-jobs", "conflict-resolution", "negotiate-solution", "workplace", "neutral"),
    ("conflict-feedback", "Kritik ohne Streit annehmen", "work-and-jobs", "conflict-resolution", "handle-misunderstanding", "workplace", "neutral"),
    ("conflict-shared-costs", "Gemeinsame Kosten fair aufteilen", "everyday-life", "conflict-resolution", "negotiate-solution", "face-to-face", "neutral"),
    ("conflict-appointment", "Einen Termin trotz unterschiedlicher Wünsche finden", "everyday-life", "conflict-resolution", "negotiate-solution", "phone", "neutral"),
    ("restaurant-reschedule", "Eine Restaurantreservierung ändern", "everyday-life", "food-restaurant", "reschedule-appointment", "phone", "formal"),
    ("restaurant-recommendation", "Nach einer Empfehlung im Restaurant fragen", "everyday-life", "food-restaurant", "ask-for-information", "face-to-face", "neutral"),
    ("restaurant-large-group", "Eine große Gruppe im Restaurant anmelden", "everyday-life", "food-restaurant", "make-appointment", "phone", "formal"),
    ("restaurant-diet", "Eine besondere Ernährung erklären", "appointments-and-health", "food-restaurant", "explain-problem", "face-to-face", "neutral"),
    ("exam-education-plan", "In der Prüfung einen Bildungsplan diskutieren", "everyday-life", "exam-preparation", "exam-discussion", "exam-room", "formal"),
    ("exam-work-complaint", "Eine Beschwerde am Arbeitsplatz in der Prüfung üben", "work-and-jobs", "exam-preparation", "complain-politely", "exam-room", "formal"),
    ("exam-city-service", "Eine Anfrage an die Stadt in der Prüfung spielen", "everyday-life", "exam-preparation", "exam-roleplay", "exam-room", "formal"),
    ("exam-restaurant-problem", "Ein Restaurantproblem in der Prüfung lösen", "everyday-life", "exam-preparation", "exam-roleplay", "exam-room", "formal"),
    ("digital-education-platform", "Eine Lernplattform beim Support erklären", "everyday-life", "digital-services", "explain-problem", "video-call", "neutral"),
    ("digital-work-calendar", "Kalenderberechtigungen im Team klären", "work-and-jobs", "digital-services", "ask-for-help", "workplace", "neutral"),
]

TASK_TO_FUNCTIONS = {
    "explain-problem": ["greet", "explain", "clarify", "answer-question", "confirm", "summarize", "close-conversation"],
    "ask-for-information": ["greet", "ask-question", "clarify", "confirm", "summarize", "close-conversation"],
    "compare-options": ["greet", "ask-question", "compare", "justify", "suggest", "summarize", "close-conversation"],
    "handle-misunderstanding": ["greet", "clarify", "correct", "apologize", "confirm", "summarize", "close-conversation"],
    "negotiate-solution": ["greet", "explain", "suggest", "compare", "negotiate", "summarize", "close-conversation"],
    "ask-for-help": ["greet", "request", "explain", "clarify", "confirm", "close-conversation"],
    "make-suggestion": ["greet", "suggest", "explain", "justify", "agree", "summarize", "close-conversation"],
    "complain-politely": ["greet", "explain", "complain", "request", "negotiate", "summarize", "close-conversation"],
    "reschedule-appointment": ["greet", "request", "explain", "suggest", "confirm", "summarize", "close-conversation"],
    "make-appointment": ["greet", "request", "ask-question", "confirm", "close-conversation"],
    "exam-discussion": ["greet", "explain", "justify", "agree", "disagree", "summarize"],
    "exam-roleplay": ["greet", "request", "clarify", "suggest", "confirm", "close-conversation"],
}


def make_dialogue(level: str, situation_index: int, variant: int, sort_order: int) -> dict:
    key, title, topic, category, task, mode, register = DIALOGUE_SITUATIONS[situation_index]
    dialogue = base.make_dialogue(level, situation_index + 1100, sort_order)
    dialogue["slug"] = f"{level.lower()}-dialogue-expansion-012-{key}-{variant:02d}"
    dialogue["title"] = f"{title} ({level})"
    dialogue["description"] = f"B1/B2 German dialogue for {title.lower()} with practical speaking and exam preparation focus."
    dialogue["learnerGoal"] = "Practice asking precise questions, explaining reasons, negotiating politely, and summarizing the result."
    dialogue["category"] = category
    dialogue["topics"] = [topic if topic in {"everyday-life", "housing", "shopping", "work-and-jobs", "appointments-and-health"} else "everyday-life"]
    dialogue["taskType"] = task
    dialogue["interactionMode"] = mode
    dialogue["register"] = register
    dialogue["speakingFunctions"] = TASK_TO_FUNCTIONS[task]
    dialogue["sortOrder"] = sort_order
    dialogue["difficultyNote"] = f"{level} dialogue for a practical task: {title}."
    dialogue["examRelevance"] = "Useful for B1/B2 oral exam roleplay, formal requests, complaints, and conflict resolution practice."

    skill_focus = ["speaking", "roleplay", "exam-speaking"]
    if mode == "phone":
        skill_focus.append("phone-call")
    if category in {"work", "workplace"} or mode in {"workplace", "video-call"}:
        skill_focus.append("workplace-communication")
    if category == "complaint" or task == "complain-politely":
        skill_focus.append("complaint-handling")
    if category == "conflict-resolution" or task == "negotiate-solution":
        skill_focus.append("negotiation")
    if category == "exam-preparation":
        skill_focus.append("discussion")
    dialogue["skillFocus"] = skill_focus
    return dialogue


def main() -> None:
    dialogues = []
    sort_order = 240000
    for level in ("B1", "B2"):
        for variant in range(1, 3):
            for situation_index in range(len(DIALOGUE_SITUATIONS)):
                dialogues.append(make_dialogue(level, situation_index, variant, sort_order))
                sort_order += 10

    package = {
        "packageId": "dialogues-exam-expansion-012-b1-b2",
        "packageName": "Dialogue Exam Expansion 012 B1 B2",
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

import json
from pathlib import Path

import generate_dialogue_exam_content as base


ROOT = Path(__file__).resolve().parents[2]
OUT_DIR = ROOT / "content" / "generated" / "dialogues"
OUT_FILE = OUT_DIR / "dialogues-exam-expansion-011-b1-b2.json"

DIALOGUE_SITUATIONS = [
    ("city-registration-question", "Eine Frage zur Anmeldung in der Stadt stellen", "everyday-life", "city-services", "ask-for-information", "government-office", "formal"),
    ("city-parking-permit", "Einen Parkausweis beantragen", "everyday-life", "city-services", "ask-for-information", "government-office", "formal"),
    ("city-cleanliness", "Sauberkeit im öffentlichen Raum ansprechen", "everyday-life", "city-services", "complain-politely", "phone", "formal"),
    ("city-event-info", "Informationen zu einer städtischen Veranstaltung bekommen", "everyday-life", "city-services", "ask-for-information", "service-counter", "formal"),
    ("restaurant-allergy", "Im Restaurant eine Allergie erklären", "appointments-and-health", "food-restaurant", "explain-problem", "face-to-face", "neutral"),
    ("restaurant-vegan", "Nach vegetarischen Optionen fragen", "everyday-life", "food-restaurant", "ask-for-information", "face-to-face", "neutral"),
    ("restaurant-delay", "Eine lange Wartezeit im Restaurant ansprechen", "everyday-life", "food-restaurant", "complain-politely", "face-to-face", "neutral"),
    ("restaurant-group-bill", "Die Rechnung für eine Gruppe klären", "shopping", "food-restaurant", "ask-for-information", "face-to-face", "neutral"),
    ("education-admission", "Zulassung zu einem Kurs erfragen", "everyday-life", "education", "ask-for-information", "service-counter", "formal"),
    ("education-exam-date", "Einen Prüfungstermin verschieben", "everyday-life", "education", "reschedule-appointment", "phone", "formal"),
    ("education-learning-plan", "Einen Lernplan mit der Lehrkraft besprechen", "everyday-life", "education", "discuss-plan", "classroom", "neutral"),
    ("education-online-course", "Einen Online-Kurs vergleichen", "everyday-life", "education", "compare-options", "video-call", "neutral"),
    ("work-contract-hours", "Arbeitszeiten im Vertrag klären", "work-and-jobs", "work", "ask-for-information", "workplace", "formal"),
    ("work-probation-feedback", "Feedback in der Probezeit besprechen", "work-and-jobs", "work", "describe-experience", "workplace", "formal"),
    ("work-team-rule", "Eine Teamregel vorschlagen", "work-and-jobs", "work", "make-suggestion", "workplace", "neutral"),
    ("work-overtime", "Überstunden sachlich besprechen", "work-and-jobs", "work", "negotiate-solution", "workplace", "formal"),
    ("complaint-phone-bill", "Eine falsche Telefonrechnung reklamieren", "shopping", "complaint", "complain-politely", "phone", "formal"),
    ("complaint-cleaning", "Eine mangelhafte Reinigung beanstanden", "housing", "complaint", "complain-politely", "phone", "formal"),
    ("complaint-service-delay", "Eine verzögerte Dienstleistung reklamieren", "shopping", "complaint", "complain-politely", "phone", "formal"),
    ("complaint-online-support", "Beim Online-Support eine Beschwerde formulieren", "everyday-life", "complaint", "complain-politely", "video-call", "formal"),
    ("conflict-meeting-tone", "Einen unangenehmen Ton im Meeting ansprechen", "work-and-jobs", "conflict-resolution", "handle-misunderstanding", "workplace", "neutral"),
    ("conflict-shared-kitchen", "Regeln für eine gemeinsame Küche finden", "housing", "conflict-resolution", "negotiate-solution", "face-to-face", "neutral"),
    ("conflict-study-group", "Eine Aufgabe in der Lerngruppe fair verteilen", "everyday-life", "conflict-resolution", "negotiate-solution", "group-work", "neutral"),
    ("conflict-family-time", "Zeit für Familie und Arbeit aushandeln", "everyday-life", "conflict-resolution", "negotiate-solution", "face-to-face", "mixed"),
    ("exam-complaint-role", "Eine Beschwerde als Prüfungsrolle üben", "everyday-life", "exam-preparation", "exam-roleplay", "exam-room", "formal"),
    ("exam-course-opinion", "Eine Meinung zu Online-Kursen begründen", "everyday-life", "exam-preparation", "give-opinion", "exam-room", "formal"),
    ("exam-city-comparison", "Stadtleben und Landleben vergleichen", "everyday-life", "exam-preparation", "compare-options", "exam-room", "formal"),
    ("exam-work-conflict", "Einen Arbeitskonflikt in der Prüfung lösen", "work-and-jobs", "exam-preparation", "negotiate-solution", "exam-room", "formal"),
    ("digital-payment-auth", "Eine Zahlungsfreigabe in der App klären", "shopping", "digital-services", "ask-for-help", "phone", "formal"),
    ("digital-course-login", "Login-Daten für einen Kurs zurücksetzen", "everyday-life", "digital-services", "ask-for-help", "phone", "formal"),
]

TASK_TO_FUNCTIONS = {
    "ask-for-information": ["greet", "ask-question", "clarify", "confirm", "summarize", "close-conversation"],
    "complain-politely": ["greet", "explain", "complain", "request", "negotiate", "summarize", "close-conversation"],
    "explain-problem": ["greet", "explain", "clarify", "answer-question", "confirm", "summarize", "close-conversation"],
    "reschedule-appointment": ["greet", "request", "explain", "suggest", "confirm", "summarize", "close-conversation"],
    "discuss-plan": ["greet", "suggest", "compare", "agree", "summarize", "close-conversation"],
    "compare-options": ["greet", "ask-question", "compare", "justify", "suggest", "summarize", "close-conversation"],
    "describe-experience": ["greet", "describe", "explain", "justify", "answer-question", "close-conversation"],
    "make-suggestion": ["greet", "suggest", "explain", "justify", "agree", "summarize", "close-conversation"],
    "negotiate-solution": ["greet", "explain", "suggest", "compare", "negotiate", "summarize", "close-conversation"],
    "handle-misunderstanding": ["greet", "clarify", "correct", "apologize", "confirm", "summarize", "close-conversation"],
    "exam-roleplay": ["greet", "request", "clarify", "suggest", "confirm", "close-conversation"],
    "give-opinion": ["greet", "explain", "justify", "agree", "disagree", "summarize"],
    "ask-for-help": ["greet", "request", "explain", "clarify", "confirm", "close-conversation"],
}


def make_dialogue(level: str, situation_index: int, variant: int, sort_order: int) -> dict:
    key, title, topic, category, task, mode, register = DIALOGUE_SITUATIONS[situation_index]
    dialogue = base.make_dialogue(level, situation_index + 1000, sort_order)
    dialogue["slug"] = f"{level.lower()}-dialogue-expansion-011-{key}-{variant:02d}"
    dialogue["title"] = f"{title} ({level})"
    dialogue["description"] = f"B1/B2 German dialogue for {title.lower()} with practical and exam-oriented speaking practice."
    dialogue["learnerGoal"] = "Practice clear questions, reasons, polite disagreement, and a final summary."
    dialogue["category"] = category
    dialogue["topics"] = [topic if topic in {"everyday-life", "housing", "shopping", "work-and-jobs", "appointments-and-health"} else "everyday-life"]
    dialogue["taskType"] = task
    dialogue["interactionMode"] = mode
    dialogue["register"] = register
    dialogue["speakingFunctions"] = TASK_TO_FUNCTIONS[task]
    dialogue["sortOrder"] = sort_order
    dialogue["difficultyNote"] = f"{level} dialogue for a practical task: {title}."
    dialogue["examRelevance"] = "Useful for B1/B2 roleplay, polite complaints, formal service conversations, and exam speaking."

    skill_focus = ["speaking", "roleplay", "exam-speaking"]
    if mode == "phone":
        skill_focus.append("phone-call")
    if category in {"work", "workplace"} or mode in {"workplace", "video-call"}:
        skill_focus.append("workplace-communication")
    if category == "complaint" or task == "complain-politely":
        skill_focus.append("complaint-handling")
    if category == "conflict-resolution" or task == "negotiate-solution":
        skill_focus.append("negotiation")
    if category == "exam-preparation" or task == "give-opinion":
        skill_focus.append("discussion")
    dialogue["skillFocus"] = skill_focus
    return dialogue


def main() -> None:
    dialogues = []
    sort_order = 220000
    for level in ("B1", "B2"):
        for variant in range(1, 3):
            for situation_index in range(len(DIALOGUE_SITUATIONS)):
                dialogues.append(make_dialogue(level, situation_index, variant, sort_order))
                sort_order += 10

    package = {
        "packageId": "dialogues-exam-expansion-011-b1-b2",
        "packageName": "Dialogue Exam Expansion 011 B1 B2",
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

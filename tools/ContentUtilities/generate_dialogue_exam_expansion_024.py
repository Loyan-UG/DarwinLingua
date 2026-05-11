import json
from pathlib import Path

import generate_dialogue_exam_content as base
from generate_dialogue_exam_expansion_023 import TASK_ALIASES, TASK_TO_FUNCTIONS


ROOT = Path(__file__).resolve().parents[2]
OUT_DIR = ROOT / "content" / "generated" / "dialogues"
OUT_FILE = OUT_DIR / "dialogues-exam-expansion-024-b1-b2.json"

DIALOGUE_SITUATIONS = [
    ("food-buffet-choice", "Ein Buffet für eine Veranstaltung auswählen", "everyday-life", "food-restaurant", "compare-options", "phone", "formal"),
    ("food-canteen-feedback", "Feedback zur Kantine geben", "work-and-jobs", "food-restaurant", "give-opinion", "workplace", "neutral"),
    ("food-special-diet", "Eine besondere Ernährung im Restaurant erklären", "appointments-and-health", "food-restaurant", "explain-problem", "face-to-face", "formal"),
    ("food-cafe-complaint", "Eine Bestellung im Café freundlich reklamieren", "shopping", "food-restaurant", "complain-politely", "face-to-face", "formal"),
    ("integration-course-exam-date", "Einen Prüfungstermin im Integrationskurs klären", "everyday-life", "integration-course", "ask-for-information", "classroom", "formal"),
    ("integration-course-learning-problem", "Ein Lernproblem mit der Kursleitung besprechen", "everyday-life", "integration-course", "explain-problem", "face-to-face", "formal"),
    ("integration-course-attendance-proof", "Eine Teilnahmebescheinigung anfordern", "work-and-jobs", "integration-course", "ask-for-information", "service-counter", "formal"),
    ("integration-course-group-task", "Eine Gruppenaufgabe im Kurs verteilen", "everyday-life", "integration-course", "discuss-plan", "group-work", "neutral"),
    ("work-uniform-question", "Arbeitskleidung im Betrieb klären", "work-and-jobs", "work", "ask-for-information", "workplace", "formal"),
    ("work-procedure-question", "Einen Arbeitsablauf genau nachfragen", "work-and-jobs", "work", "clarify", "workplace", "neutral"),
    ("work-colleague-support", "Kollegen um Unterstützung bitten", "work-and-jobs", "work", "ask-for-help", "workplace", "neutral"),
    ("work-schedule-suggestion", "Einen Vorschlag für den Dienstplan machen", "work-and-jobs", "work", "make-suggestion", "workplace", "formal"),
    ("education-scholarship-info", "Informationen zu einer Förderung erfragen", "work-and-jobs", "education", "ask-for-information", "phone", "formal"),
    ("education-exam-delay", "Eine Prüfungsverspätung erklären", "everyday-life", "education", "explain-problem", "phone", "formal"),
    ("education-course-choice", "Zwei Kursangebote vergleichen", "everyday-life", "education", "compare-options", "face-to-face", "formal"),
    ("education-teacher-feedback", "Feedback von einer Lehrkraft besprechen", "everyday-life", "education", "give-opinion", "classroom", "formal"),
    ("family-medical-decision", "Eine medizinische Entscheidung in der Familie besprechen", "appointments-and-health", "family", "compare-options", "face-to-face", "mixed"),
    ("family-moving-plan", "Einen Umzug in der Familie planen", "housing", "family", "discuss-plan", "face-to-face", "mixed"),
    ("family-child-activity", "Eine Freizeitaktivität für ein Kind auswählen", "everyday-life", "family", "compare-options", "face-to-face", "informal"),
    ("family-relative-visit", "Einen Besuch von Verwandten organisieren", "everyday-life", "family", "discuss-plan", "phone", "informal"),
    ("city-registration-question", "Eine Anmeldung bei der Stadt nachfragen", "everyday-life", "city-services", "ask-for-information", "government-office", "formal"),
    ("city-senior-service", "Ein Angebot für ältere Menschen erfragen", "appointments-and-health", "city-services", "ask-for-information", "phone", "formal"),
    ("city-room-rental", "Einen Raum der Stadt mieten", "everyday-life", "city-services", "negotiate-solution", "service-counter", "formal"),
    ("city-volunteer-project", "Bei einem städtischen Projekt mitmachen", "everyday-life", "city-services", "make-suggestion", "group-work", "neutral"),
    ("social-apology-friend", "Sich bei einem Freund entschuldigen", "everyday-life", "social-life", "handle-misunderstanding", "face-to-face", "informal"),
    ("social-group-decision", "Eine Entscheidung in einer Gruppe treffen", "everyday-life", "social-life", "agree-disagree", "group-work", "neutral"),
    ("shopping-price-increase", "Eine Preiserhöhung bei einem Vertrag besprechen", "shopping", "shopping-and-services", "negotiate-solution", "phone", "formal"),
    ("shopping-wrong-size", "Eine falsche Größe umtauschen", "shopping", "shopping-and-services", "complain-politely", "service-counter", "formal"),
    ("exam-course-problem", "Ein Kursproblem in der Prüfung erklären", "everyday-life", "exam-preparation", "exam-roleplay", "exam-room", "formal"),
    ("exam-social-decision", "Eine Gruppenentscheidung in der Prüfung diskutieren", "everyday-life", "exam-preparation", "exam-discussion", "exam-room", "formal"),
]


TASK_TO_FUNCTIONS = {
    **TASK_TO_FUNCTIONS,
    "make-suggestion": ["greet", "suggest", "explain", "justify", "agree", "summarize", "close-conversation"],
    "agree-disagree": ["greet", "explain", "agree", "disagree", "justify", "summarize", "close-conversation"],
    "handle-misunderstanding": ["greet", "clarify", "correct", "explain", "apologize", "confirm", "close-conversation"],
}


def make_dialogue(level: str, situation_index: int, variant: int, sort_order: int) -> dict:
    key, title, topic, category, task, mode, register = DIALOGUE_SITUATIONS[situation_index]
    dialogue = base.make_dialogue(level, situation_index + 2300, sort_order)
    dialogue["slug"] = f"{level.lower()}-dialogue-expansion-024-{key}-{variant:02d}"
    dialogue["title"] = f"{title} ({level})"
    dialogue["description"] = f"B1/B2 German dialogue for {title.lower()} with practical roleplay and exam speaking focus."
    dialogue["learnerGoal"] = "Practice explaining needs, comparing choices, reacting politely, and confirming a practical agreement."
    dialogue["category"] = category
    dialogue["topics"] = [topic if topic in {"everyday-life", "housing", "shopping", "work-and-jobs", "appointments-and-health"} else "everyday-life"]
    dialogue["taskType"] = TASK_ALIASES.get(task, task)
    dialogue["interactionMode"] = mode
    dialogue["register"] = register
    dialogue["speakingFunctions"] = TASK_TO_FUNCTIONS[task]
    dialogue["sortOrder"] = sort_order
    dialogue["difficultyNote"] = f"{level} dialogue for a practical task: {title}."
    dialogue["examRelevance"] = "Useful for B1/B2 oral exam roleplay, everyday planning, polite disagreement, and practical service conversations."

    skill_focus = ["speaking", "roleplay", "exam-speaking"]
    if mode == "phone":
        skill_focus.append("phone-call")
    if category in {"work", "workplace", "job-interview"} or mode == "workplace":
        skill_focus.append("workplace-communication")
    if task == "negotiate-solution":
        skill_focus.append("negotiation")
    if category == "exam-preparation" or task in {"give-opinion", "exam-discussion", "agree-disagree"}:
        skill_focus.append("discussion")
    if task == "complain-politely":
        skill_focus.append("complaint-handling")
    dialogue["skillFocus"] = skill_focus
    return dialogue


def main() -> None:
    dialogues = []
    sort_order = 480000
    for level in ("B1", "B2"):
        for variant in range(1, 3):
            for situation_index in range(len(DIALOGUE_SITUATIONS)):
                dialogues.append(make_dialogue(level, situation_index, variant, sort_order))
                sort_order += 10

    package = {
        "packageId": "dialogues-exam-expansion-024-b1-b2",
        "packageName": "Dialogue Exam Expansion 024 B1 B2",
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

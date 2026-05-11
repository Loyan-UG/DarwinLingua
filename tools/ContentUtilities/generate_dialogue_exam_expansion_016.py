import json
from pathlib import Path

import generate_dialogue_exam_content as base
from generate_dialogue_exam_expansion_015 import TASK_ALIASES, TASK_TO_FUNCTIONS


ROOT = Path(__file__).resolve().parents[2]
OUT_DIR = ROOT / "content" / "generated" / "dialogues"
OUT_FILE = OUT_DIR / "dialogues-exam-expansion-016-b1-b2.json"

TASK_TO_FUNCTIONS = {
    **TASK_TO_FUNCTIONS,
    "job-interview": ["greet", "introduce", "answer-question", "explain", "justify", "ask-question", "close-conversation"],
    "reschedule-appointment": ["greet", "request", "explain", "suggest", "confirm", "summarize", "close-conversation"],
}

DIALOGUE_SITUATIONS = [
    ("integration-course-childcare", "Kinderbetreuung während des Integrationskurses klären", "everyday-life", "integration-course", "ask-for-help", "phone", "formal"),
    ("integration-course-absence", "Eine Abwesenheit im Integrationskurs erklären", "everyday-life", "integration-course", "explain-problem", "phone", "formal"),
    ("integration-course-exam-plan", "Die Prüfungsvorbereitung im Kurs planen", "everyday-life", "integration-course", "discuss-plan", "classroom", "formal"),
    ("integration-course-certificate", "Ein Zertifikat vom Kursträger anfordern", "work-and-jobs", "integration-course", "ask-for-information", "service-counter", "formal"),
    ("job-interview-strengths", "Eigene Stärken im Vorstellungsgespräch erklären", "work-and-jobs", "job-interview", "job-interview", "face-to-face", "formal"),
    ("job-interview-gap", "Eine Lücke im Lebenslauf erklären", "work-and-jobs", "job-interview", "explain-problem", "face-to-face", "formal"),
    ("job-interview-shift-work", "Arbeitszeiten im Vorstellungsgespräch verhandeln", "work-and-jobs", "job-interview", "negotiate-solution", "face-to-face", "formal"),
    ("job-interview-training", "Nach Weiterbildung im neuen Job fragen", "work-and-jobs", "job-interview", "ask-for-information", "face-to-face", "formal"),
    ("shopping-return-online", "Eine Onlinebestellung zurückgeben", "shopping", "shopping-and-services", "complain-politely", "phone", "formal"),
    ("shopping-warranty", "Eine Garantie im Geschäft klären", "shopping", "shopping-and-services", "ask-for-information", "service-counter", "formal"),
    ("shopping-service-contract", "Einen Servicevertrag vergleichen", "shopping", "shopping-and-services", "compare-options", "service-counter", "formal"),
    ("shopping-delivery-date", "Einen Liefertermin neu vereinbaren", "shopping", "shopping-and-services", "reschedule-appointment", "phone", "formal"),
    ("social-room-booking", "Einen Raum für eine Gruppe buchen", "everyday-life", "social-life", "make-appointment", "phone", "formal"),
    ("social-volunteer-task", "Eine freiwillige Aufgabe übernehmen", "everyday-life", "social-life", "make-suggestion", "group-work", "neutral"),
    ("social-party-noise", "Eine Feier mit Nachbarn abstimmen", "housing", "social-life", "discuss-plan", "face-to-face", "mixed"),
    ("social-club-conflict", "Einen Konflikt im Verein klären", "everyday-life", "social-life", "negotiate-solution", "group-work", "neutral"),
    ("family-care-plan", "Pflegeaufgaben in der Familie verteilen", "appointments-and-health", "family", "discuss-plan", "face-to-face", "mixed"),
    ("family-language-school", "Einen Sprachkurs für ein Familienmitglied suchen", "everyday-life", "family", "compare-options", "face-to-face", "informal"),
    ("family-budget-school", "Ein Schulbudget in der Familie besprechen", "shopping", "family", "negotiate-solution", "face-to-face", "mixed"),
    ("family-visit-doctor", "Einen Arztbesuch für ein Elternteil organisieren", "appointments-and-health", "family", "ask-for-help", "phone", "formal"),
    ("finance-bank-fee", "Eine Bankgebühr freundlich hinterfragen", "shopping", "finance-insurance", "complain-politely", "phone", "formal"),
    ("finance-insurance-claim", "Einen Versicherungsfall melden", "shopping", "finance-insurance", "explain-problem", "phone", "formal"),
    ("digital-online-id", "Eine Online-Ausweisfunktion einrichten", "everyday-life", "digital-services", "ask-for-help", "phone", "formal"),
    ("digital-support-ticket", "Ein Support-Ticket genau beschreiben", "work-and-jobs", "digital-services", "explain-problem", "phone", "formal"),
    ("exam-integration-plan", "Einen Lernplan im Prüfungsgespräch vorstellen", "everyday-life", "exam-preparation", "exam-discussion", "exam-room", "formal"),
    ("exam-job-interview", "Ein Vorstellungsgespräch als Prüfungsrolle üben", "work-and-jobs", "exam-preparation", "exam-roleplay", "exam-room", "formal"),
    ("exam-online-shopping", "Über Online-Shopping in der Prüfung diskutieren", "shopping", "exam-preparation", "give-opinion", "exam-room", "formal"),
    ("exam-family-care", "Über Pflege in der Familie sprechen", "appointments-and-health", "exam-preparation", "exam-discussion", "exam-room", "formal"),
    ("health-lab-result-call", "Ein Laborergebnis telefonisch erfragen", "appointments-and-health", "healthcare", "ask-for-information", "phone", "formal"),
    ("housing-contract-question", "Eine Frage zum Mietvertrag stellen", "housing", "housing", "ask-for-information", "phone", "formal"),
]


def make_dialogue(level: str, situation_index: int, variant: int, sort_order: int) -> dict:
    key, title, topic, category, task, mode, register = DIALOGUE_SITUATIONS[situation_index]
    dialogue = base.make_dialogue(level, situation_index + 1500, sort_order)
    dialogue["slug"] = f"{level.lower()}-dialogue-expansion-016-{key}-{variant:02d}"
    dialogue["title"] = f"{title} ({level})"
    dialogue["description"] = f"B1/B2 German dialogue for {title.lower()} with practical roleplay and exam speaking focus."
    dialogue["learnerGoal"] = "Practice giving reasons, reacting politely, clarifying details, and agreeing on realistic next steps."
    dialogue["category"] = category
    dialogue["topics"] = [topic if topic in {"everyday-life", "housing", "shopping", "work-and-jobs", "appointments-and-health"} else "everyday-life"]
    dialogue["taskType"] = TASK_ALIASES.get(task, task)
    dialogue["interactionMode"] = mode
    dialogue["register"] = register
    dialogue["speakingFunctions"] = TASK_TO_FUNCTIONS[task]
    dialogue["sortOrder"] = sort_order
    dialogue["difficultyNote"] = f"{level} dialogue for a practical task: {title}."
    dialogue["examRelevance"] = "Useful for B1/B2 oral exam roleplay, exam discussion, workplace communication, and everyday problem solving."

    skill_focus = ["speaking", "roleplay", "exam-speaking"]
    if mode == "phone":
        skill_focus.append("phone-call")
    if category in {"work", "workplace", "job-interview"}:
        skill_focus.append("workplace-communication")
    if task == "negotiate-solution":
        skill_focus.append("negotiation")
    if category == "exam-preparation" or task in {"give-opinion", "exam-discussion"}:
        skill_focus.append("discussion")
    if task == "complain-politely":
        skill_focus.append("complaint-handling")
    dialogue["skillFocus"] = skill_focus
    return dialogue


def main() -> None:
    dialogues = []
    sort_order = 320000
    for level in ("B1", "B2"):
        for variant in range(1, 3):
            for situation_index in range(len(DIALOGUE_SITUATIONS)):
                dialogues.append(make_dialogue(level, situation_index, variant, sort_order))
                sort_order += 10

    package = {
        "packageId": "dialogues-exam-expansion-016-b1-b2",
        "packageName": "Dialogue Exam Expansion 016 B1 B2",
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

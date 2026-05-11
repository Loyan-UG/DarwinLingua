import json
from pathlib import Path

import generate_dialogue_exam_content as base
from generate_dialogue_exam_expansion_022 import TASK_ALIASES, TASK_TO_FUNCTIONS


ROOT = Path(__file__).resolve().parents[2]
OUT_DIR = ROOT / "content" / "generated" / "dialogues"
OUT_FILE = OUT_DIR / "dialogues-exam-expansion-023-b1-b2.json"

DIALOGUE_SITUATIONS = [
    ("housing-water-damage", "Einen Wasserschaden in der Wohnung melden", "housing", "housing", "explain-problem", "phone", "formal"),
    ("housing-rental-deposit", "Eine Mietkaution beim Auszug klären", "housing", "housing", "ask-for-information", "phone", "formal"),
    ("housing-stairwell-rule", "Eine Regel im Treppenhaus besprechen", "housing", "housing", "complain-politely", "face-to-face", "mixed"),
    ("housing-new-contract", "Einen neuen Mietvertrag vergleichen", "housing", "housing", "compare-options", "face-to-face", "formal"),
    ("government-tax-class", "Eine Steuerklasse beim Amt nachfragen", "work-and-jobs", "government-office", "ask-for-information", "government-office", "formal"),
    ("government-child-benefit", "Kindergeld-Unterlagen nachreichen", "everyday-life", "government-office", "ask-for-help", "government-office", "formal"),
    ("government-appointment-confirmation", "Eine Terminbestätigung beim Amt klären", "everyday-life", "government-office", "clarify", "phone", "formal"),
    ("government-appeal-deadline", "Eine Frist für einen Widerspruch verstehen", "everyday-life", "government-office", "ask-for-information", "phone", "formal"),
    ("health-dental-cost-plan", "Einen Kostenplan beim Zahnarzt besprechen", "appointments-and-health", "healthcare", "compare-options", "doctor-office", "formal"),
    ("health-sick-child-note", "Eine Bescheinigung für ein krankes Kind erfragen", "appointments-and-health", "healthcare", "ask-for-information", "phone", "formal"),
    ("health-specialist-waiting-time", "Eine lange Wartezeit beim Facharzt klären", "appointments-and-health", "healthcare", "complain-politely", "phone", "formal"),
    ("health-aftercare-plan", "Nachsorge nach einer Behandlung planen", "appointments-and-health", "healthcare", "discuss-plan", "doctor-office", "formal"),
    ("workplace-task-handover", "Eine Aufgabenübergabe im Team planen", "work-and-jobs", "workplace", "discuss-plan", "workplace", "neutral"),
    ("workplace-customer-complaint", "Eine Kundenbeschwerde intern besprechen", "work-and-jobs", "workplace", "explain-problem", "workplace", "formal"),
    ("workplace-budget-cut", "Eine Budgetkürzung im Projekt diskutieren", "work-and-jobs", "workplace", "negotiate-solution", "video-call", "formal"),
    ("workplace-meeting-summary", "Ein Meeting-Ergebnis zusammenfassen", "work-and-jobs", "workplace", "discuss-plan", "workplace", "formal"),
    ("travel-train-strike", "Eine Reise wegen Streik umplanen", "everyday-life", "transport-and-travel", "compare-options", "phone", "formal"),
    ("travel-business-hotel", "Ein Hotel für eine Dienstreise buchen", "work-and-jobs", "transport-and-travel", "ask-for-information", "phone", "formal"),
    ("travel-lost-ticket", "Ein verlorenes Ticket erklären", "everyday-life", "transport-and-travel", "explain-problem", "service-counter", "formal"),
    ("travel-access-card", "Eine Zugangskarte im Verkehrsbetrieb klären", "everyday-life", "transport-and-travel", "ask-for-help", "service-counter", "formal"),
    ("finance-rent-payment", "Eine Mietzahlung bei der Bank nachweisen", "shopping", "finance-insurance", "ask-for-information", "phone", "formal"),
    ("finance-medical-invoice", "Eine Arztrechnung bei der Versicherung einreichen", "appointments-and-health", "finance-insurance", "ask-for-help", "phone", "formal"),
    ("digital-government-login", "Ein Loginproblem beim Bürgerportal lösen", "everyday-life", "digital-services", "ask-for-help", "phone", "formal"),
    ("digital-work-calendar", "Einen gemeinsamen Arbeitskalender einrichten", "work-and-jobs", "digital-services", "discuss-plan", "video-call", "neutral"),
    ("complaint-housing-company", "Eine Hausverwaltung wegen fehlender Antwort kontaktieren", "housing", "complaint", "complain-politely", "phone", "formal"),
    ("conflict-work-responsibility", "Verantwortlichkeiten im Team neu klären", "work-and-jobs", "conflict-resolution", "negotiate-solution", "workplace", "formal"),
    ("exam-housing-damage", "Einen Wohnungsschaden in der Prüfung erklären", "housing", "exam-preparation", "exam-roleplay", "exam-room", "formal"),
    ("exam-government-deadline", "Eine Amtsfrist in der Prüfung besprechen", "everyday-life", "exam-preparation", "exam-roleplay", "exam-room", "formal"),
    ("exam-health-costs", "Gesundheitskosten in der Prüfung vergleichen", "appointments-and-health", "exam-preparation", "compare-options", "exam-room", "formal"),
    ("exam-workplace-summary", "Ein Arbeitsergebnis in der Prüfung zusammenfassen", "work-and-jobs", "exam-preparation", "exam-discussion", "exam-room", "formal"),
]


TASK_TO_FUNCTIONS = {
    **TASK_TO_FUNCTIONS,
    "clarify": ["greet", "ask-question", "clarify", "confirm", "summarize", "close-conversation"],
}


def make_dialogue(level: str, situation_index: int, variant: int, sort_order: int) -> dict:
    key, title, topic, category, task, mode, register = DIALOGUE_SITUATIONS[situation_index]
    dialogue = base.make_dialogue(level, situation_index + 2200, sort_order)
    dialogue["slug"] = f"{level.lower()}-dialogue-expansion-023-{key}-{variant:02d}"
    dialogue["title"] = f"{title} ({level})"
    dialogue["description"] = f"B1/B2 German dialogue for {title.lower()} with practical roleplay and exam speaking focus."
    dialogue["learnerGoal"] = "Practice describing the problem, clarifying details, comparing possible actions, and summarizing the next step."
    dialogue["category"] = category
    dialogue["topics"] = [topic if topic in {"everyday-life", "housing", "shopping", "work-and-jobs", "appointments-and-health"} else "everyday-life"]
    dialogue["taskType"] = TASK_ALIASES.get(task, task)
    dialogue["interactionMode"] = mode
    dialogue["register"] = register
    dialogue["speakingFunctions"] = TASK_TO_FUNCTIONS[task]
    dialogue["sortOrder"] = sort_order
    dialogue["difficultyNote"] = f"{level} dialogue for a practical task: {title}."
    dialogue["examRelevance"] = "Useful for B1/B2 oral exam roleplay, formal clarification, service communication, and workplace summaries."

    skill_focus = ["speaking", "roleplay", "exam-speaking"]
    if mode == "phone":
        skill_focus.append("phone-call")
    if category == "workplace" or mode in {"workplace", "video-call"}:
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
    sort_order = 460000
    for level in ("B1", "B2"):
        for variant in range(1, 3):
            for situation_index in range(len(DIALOGUE_SITUATIONS)):
                dialogues.append(make_dialogue(level, situation_index, variant, sort_order))
                sort_order += 10

    package = {
        "packageId": "dialogues-exam-expansion-023-b1-b2",
        "packageName": "Dialogue Exam Expansion 023 B1 B2",
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

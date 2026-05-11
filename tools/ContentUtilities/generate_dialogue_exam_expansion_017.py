import json
from pathlib import Path

import generate_dialogue_exam_content as base
from generate_dialogue_exam_expansion_016 import TASK_ALIASES, TASK_TO_FUNCTIONS


ROOT = Path(__file__).resolve().parents[2]
OUT_DIR = ROOT / "content" / "generated" / "dialogues"
OUT_FILE = OUT_DIR / "dialogues-exam-expansion-017-b1-b2.json"

DIALOGUE_SITUATIONS = [
    ("health-specialist-referral", "Eine Überweisung zum Facharzt klären", "appointments-and-health", "healthcare", "ask-for-information", "phone", "formal"),
    ("health-medication-side-effect", "Nebenwirkungen eines Medikaments erklären", "appointments-and-health", "healthcare", "explain-problem", "doctor-office", "formal"),
    ("health-appointment-urgent", "Einen dringenden Arzttermin begründen", "appointments-and-health", "healthcare", "make-appointment", "phone", "formal"),
    ("health-rehab-options", "Reha-Möglichkeiten vergleichen", "appointments-and-health", "healthcare", "compare-options", "doctor-office", "formal"),
    ("government-address-change", "Eine Adressänderung beim Amt melden", "everyday-life", "government-office", "ask-for-information", "government-office", "formal"),
    ("government-appointment-missed", "Einen verpassten Amtstermin erklären", "everyday-life", "government-office", "reschedule-appointment", "phone", "formal"),
    ("government-document-error", "Einen Fehler in einem Dokument korrigieren lassen", "everyday-life", "government-office", "explain-problem", "government-office", "formal"),
    ("government-permit-status", "Den Status einer Genehmigung erfragen", "work-and-jobs", "government-office", "ask-for-information", "phone", "formal"),
    ("workplace-meeting-agenda", "Eine Tagesordnung im Team abstimmen", "work-and-jobs", "workplace", "discuss-plan", "workplace", "neutral"),
    ("workplace-client-delay", "Eine Verzögerung beim Kunden erklären", "work-and-jobs", "workplace", "explain-problem", "video-call", "formal"),
    ("workplace-process-change", "Eine Prozessänderung im Team diskutieren", "work-and-jobs", "workplace", "give-opinion", "workplace", "formal"),
    ("workplace-resource-conflict", "Ressourcen für ein Projekt verhandeln", "work-and-jobs", "workplace", "negotiate-solution", "workplace", "formal"),
    ("housing-rent-increase", "Eine Mieterhöhung sachlich besprechen", "housing", "housing", "negotiate-solution", "phone", "formal"),
    ("housing-mold-problem", "Schimmel in der Wohnung melden", "housing", "housing", "complain-politely", "phone", "formal"),
    ("housing-move-out", "Eine Wohnungsübergabe planen", "housing", "housing", "discuss-plan", "face-to-face", "formal"),
    ("housing-utility-meter", "Zählerstände mit der Verwaltung klären", "housing", "housing", "ask-for-information", "phone", "formal"),
    ("complaint-internet-speed", "Langsames Internet beim Anbieter reklamieren", "shopping", "complaint", "complain-politely", "phone", "formal"),
    ("complaint-repair-quality", "Eine schlechte Reparatur reklamieren", "shopping", "complaint", "complain-politely", "service-counter", "formal"),
    ("complaint-course-service", "Ein Problem mit einem Kursanbieter klären", "everyday-life", "complaint", "negotiate-solution", "phone", "formal"),
    ("complaint-bank-response", "Eine fehlende Antwort der Bank nachfragen", "shopping", "complaint", "ask-for-information", "phone", "formal"),
    ("travel-missed-connection", "Einen verpassten Anschluss erklären", "everyday-life", "transport-and-travel", "explain-problem", "service-counter", "formal"),
    ("travel-season-ticket", "Ein Monatsticket vergleichen", "shopping", "transport-and-travel", "compare-options", "service-counter", "formal"),
    ("travel-accessibility", "Barrierefreie Reiseoptionen erfragen", "everyday-life", "transport-and-travel", "ask-for-information", "phone", "formal"),
    ("travel-business-trip", "Eine Dienstreise mit dem Team planen", "work-and-jobs", "transport-and-travel", "discuss-plan", "workplace", "formal"),
    ("exam-health-roleplay", "Eine medizinische Situation in der Prüfung üben", "appointments-and-health", "exam-preparation", "exam-roleplay", "exam-room", "formal"),
    ("exam-government-roleplay", "Ein Amtsgespräch in der Prüfung üben", "everyday-life", "exam-preparation", "exam-roleplay", "exam-room", "formal"),
    ("exam-workplace-discussion", "Eine Arbeitsplatzentscheidung in der Prüfung diskutieren", "work-and-jobs", "exam-preparation", "exam-discussion", "exam-room", "formal"),
    ("exam-housing-debate", "Über Wohnen und Kosten in der Prüfung sprechen", "housing", "exam-preparation", "give-opinion", "exam-room", "formal"),
    ("digital-public-service", "Ein digitales Bürgerkonto nutzen", "everyday-life", "digital-services", "ask-for-help", "phone", "formal"),
    ("finance-health-insurance", "Eine Leistung der Krankenkasse klären", "appointments-and-health", "finance-insurance", "ask-for-information", "phone", "formal"),
]


def make_dialogue(level: str, situation_index: int, variant: int, sort_order: int) -> dict:
    key, title, topic, category, task, mode, register = DIALOGUE_SITUATIONS[situation_index]
    dialogue = base.make_dialogue(level, situation_index + 1600, sort_order)
    dialogue["slug"] = f"{level.lower()}-dialogue-expansion-017-{key}-{variant:02d}"
    dialogue["title"] = f"{title} ({level})"
    dialogue["description"] = f"B1/B2 German dialogue for {title.lower()} with practical roleplay and exam speaking focus."
    dialogue["learnerGoal"] = "Practice explaining facts, asking for clarification, comparing solutions, and closing the conversation with a clear summary."
    dialogue["category"] = category
    dialogue["topics"] = [topic if topic in {"everyday-life", "housing", "shopping", "work-and-jobs", "appointments-and-health"} else "everyday-life"]
    dialogue["taskType"] = TASK_ALIASES.get(task, task)
    dialogue["interactionMode"] = mode
    dialogue["register"] = register
    dialogue["speakingFunctions"] = TASK_TO_FUNCTIONS[task]
    dialogue["sortOrder"] = sort_order
    dialogue["difficultyNote"] = f"{level} dialogue for a practical task: {title}."
    dialogue["examRelevance"] = "Useful for B1/B2 oral exam roleplay, formal service communication, and structured negotiation."

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
    sort_order = 340000
    for level in ("B1", "B2"):
        for variant in range(1, 3):
            for situation_index in range(len(DIALOGUE_SITUATIONS)):
                dialogues.append(make_dialogue(level, situation_index, variant, sort_order))
                sort_order += 10

    package = {
        "packageId": "dialogues-exam-expansion-017-b1-b2",
        "packageName": "Dialogue Exam Expansion 017 B1 B2",
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

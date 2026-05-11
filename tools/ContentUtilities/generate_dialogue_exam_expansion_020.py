import json
from pathlib import Path

import generate_dialogue_exam_content as base
from generate_dialogue_exam_expansion_019 import TASK_ALIASES, TASK_TO_FUNCTIONS


ROOT = Path(__file__).resolve().parents[2]
OUT_DIR = ROOT / "content" / "generated" / "dialogues"
OUT_FILE = OUT_DIR / "dialogues-exam-expansion-020-b1-b2.json"

DIALOGUE_SITUATIONS = [
    ("health-vaccination-record", "Einen Impfpass in der Praxis klären", "appointments-and-health", "healthcare", "ask-for-information", "doctor-office", "formal"),
    ("health-work-absence", "Eine Arbeitsunfähigkeit beim Arzt besprechen", "appointments-and-health", "healthcare", "explain-problem", "doctor-office", "formal"),
    ("health-physio-appointment", "Einen Physiotherapietermin verschieben", "appointments-and-health", "healthcare", "reschedule-appointment", "phone", "formal"),
    ("health-prevention-check", "Eine Vorsorgeuntersuchung planen", "appointments-and-health", "healthcare", "make-appointment", "phone", "formal"),
    ("government-benefit-question", "Eine Frage zu einer Leistung beim Amt stellen", "everyday-life", "government-office", "ask-for-information", "government-office", "formal"),
    ("government-translation-document", "Eine Übersetzung für ein Dokument klären", "everyday-life", "government-office", "ask-for-help", "phone", "formal"),
    ("government-application-delay", "Eine Verzögerung beim Antrag nachfragen", "everyday-life", "government-office", "complain-politely", "phone", "formal"),
    ("government-name-spelling", "Die Schreibweise eines Namens korrigieren lassen", "everyday-life", "government-office", "handle-misunderstanding", "government-office", "formal"),
    ("workplace-new-software", "Neue Software im Team einführen", "work-and-jobs", "workplace", "discuss-plan", "workplace", "formal"),
    ("workplace-quality-issue", "Ein Qualitätsproblem im Prozess erklären", "work-and-jobs", "workplace", "explain-problem", "workplace", "formal"),
    ("workplace-customer-priority", "Kundenprioritäten im Meeting vergleichen", "work-and-jobs", "workplace", "compare-options", "video-call", "formal"),
    ("workplace-feedback-disagree", "In einem Meeting höflich widersprechen", "work-and-jobs", "workplace", "agree-disagree", "workplace", "formal"),
    ("housing-key-handover", "Eine Schlüsselübergabe organisieren", "housing", "housing", "make-appointment", "phone", "formal"),
    ("housing-sublet-question", "Eine Untervermietung bei der Verwaltung anfragen", "housing", "housing", "ask-for-information", "phone", "formal"),
    ("housing-repair-followup", "Bei einer Reparatur nachfassen", "housing", "housing", "complain-politely", "phone", "formal"),
    ("housing-shared-garden", "Die Nutzung eines Gemeinschaftsgartens abstimmen", "housing", "housing", "discuss-plan", "face-to-face", "mixed"),
    ("travel-commuter-delay", "Eine Pendelverspätung im Büro erklären", "everyday-life", "transport-and-travel", "explain-problem", "workplace", "neutral"),
    ("travel-airport-transfer", "Einen Flughafentransfer vergleichen", "everyday-life", "transport-and-travel", "compare-options", "phone", "formal"),
    ("travel-mobility-app", "Eine Mobilitäts-App erklären lassen", "work-and-jobs", "transport-and-travel", "ask-for-help", "phone", "formal"),
    ("travel-business-expense", "Reisekosten für eine Dienstreise klären", "work-and-jobs", "transport-and-travel", "ask-for-information", "workplace", "formal"),
    ("complaint-energy-bill", "Eine hohe Energierechnung reklamieren", "housing", "complaint", "complain-politely", "phone", "formal"),
    ("complaint-training-provider", "Eine schlechte Kursorganisation melden", "everyday-life", "complaint", "complain-politely", "phone", "formal"),
    ("conflict-neighbor-parking", "Einen Parkplatzkonflikt mit Nachbarn lösen", "housing", "conflict-resolution", "negotiate-solution", "face-to-face", "mixed"),
    ("conflict-work-schedule", "Einen Dienstplan-Konflikt im Team lösen", "work-and-jobs", "conflict-resolution", "negotiate-solution", "workplace", "formal"),
    ("exam-health-insurance", "Eine Krankenkassensituation in der Prüfung üben", "appointments-and-health", "exam-preparation", "exam-roleplay", "exam-room", "formal"),
    ("exam-government-letter", "Einen Amtsbrief in der Prüfung erklären", "everyday-life", "exam-preparation", "exam-roleplay", "exam-room", "formal"),
    ("exam-work-software", "Über digitale Arbeit in der Prüfung diskutieren", "work-and-jobs", "exam-preparation", "exam-discussion", "exam-room", "formal"),
    ("exam-housing-options", "Wohnformen in der Prüfung vergleichen", "housing", "exam-preparation", "compare-options", "exam-room", "formal"),
    ("digital-health-portal", "Ein Gesundheitsportal nutzen", "appointments-and-health", "digital-services", "ask-for-help", "phone", "formal"),
    ("finance-travel-reimbursement", "Eine Reisekostenerstattung einreichen", "work-and-jobs", "finance-insurance", "ask-for-information", "workplace", "formal"),
]


TASK_TO_FUNCTIONS = {
    **TASK_TO_FUNCTIONS,
    "agree-disagree": ["greet", "explain", "agree", "disagree", "justify", "summarize", "close-conversation"],
}


def make_dialogue(level: str, situation_index: int, variant: int, sort_order: int) -> dict:
    key, title, topic, category, task, mode, register = DIALOGUE_SITUATIONS[situation_index]
    dialogue = base.make_dialogue(level, situation_index + 1900, sort_order)
    dialogue["slug"] = f"{level.lower()}-dialogue-expansion-020-{key}-{variant:02d}"
    dialogue["title"] = f"{title} ({level})"
    dialogue["description"] = f"B1/B2 German dialogue for {title.lower()} with practical roleplay and exam speaking focus."
    dialogue["learnerGoal"] = "Practice formal clarification, polite disagreement, explaining consequences, and summarizing the agreed next step."
    dialogue["category"] = category
    dialogue["topics"] = [topic if topic in {"everyday-life", "housing", "shopping", "work-and-jobs", "appointments-and-health"} else "everyday-life"]
    dialogue["taskType"] = TASK_ALIASES.get(task, task)
    dialogue["interactionMode"] = mode
    dialogue["register"] = register
    dialogue["speakingFunctions"] = TASK_TO_FUNCTIONS[task]
    dialogue["sortOrder"] = sort_order
    dialogue["difficultyNote"] = f"{level} dialogue for a practical task: {title}."
    dialogue["examRelevance"] = "Useful for B1/B2 oral exam roleplay, formal problem solving, and workplace or administration conversations."

    skill_focus = ["speaking", "roleplay", "exam-speaking"]
    if mode == "phone":
        skill_focus.append("phone-call")
    if category == "workplace" or mode in {"workplace", "video-call"}:
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
    sort_order = 400000
    for level in ("B1", "B2"):
        for variant in range(1, 3):
            for situation_index in range(len(DIALOGUE_SITUATIONS)):
                dialogues.append(make_dialogue(level, situation_index, variant, sort_order))
                sort_order += 10

    package = {
        "packageId": "dialogues-exam-expansion-020-b1-b2",
        "packageName": "Dialogue Exam Expansion 020 B1 B2",
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

import json
from pathlib import Path

import generate_dialogue_exam_content as base


ROOT = Path(__file__).resolve().parents[2]
OUT_DIR = ROOT / "content" / "generated" / "dialogues"
OUT_FILE = OUT_DIR / "dialogues-exam-expansion-005-b1-b2.json"

DIALOGUE_SITUATIONS = [
    ("integration-advice", "Beratung zur Integration in der Stadt nutzen", "everyday-life", "integration-course", "ask-for-information", "government-office", "formal"),
    ("language-practice", "Mehr Sprechpraxis im Kurs vorschlagen", "everyday-life", "integration-course", "make-suggestion", "classroom", "neutral"),
    ("exam-time-plan", "Zeitmanagement für die mündliche Prüfung planen", "everyday-life", "exam-preparation", "discuss-plan", "pair-work", "neutral"),
    ("exam-disagreement", "In der Prüfung höflich widersprechen", "everyday-life", "exam-preparation", "agree-disagree", "exam-room", "formal"),
    ("exam-summary", "Eine gemeinsame Lösung zusammenfassen", "everyday-life", "exam-preparation", "exam-discussion", "exam-room", "formal"),
    ("work-safety", "Sicherheitsregeln am Arbeitsplatz klären", "work-and-jobs", "workplace", "ask-for-information", "workplace", "formal"),
    ("shift-conflict", "Einen Konflikt im Dienstplan lösen", "work-and-jobs", "conflict-resolution", "negotiate-solution", "workplace", "neutral"),
    ("work-mistake", "Einen Fehler bei der Arbeit erklären", "work-and-jobs", "workplace", "explain-problem", "workplace", "neutral"),
    ("team-process", "Einen Arbeitsprozess verbessern", "work-and-jobs", "workplace", "make-suggestion", "workplace", "neutral"),
    ("customer-priority", "Prioritäten mit einem Kunden abstimmen", "work-and-jobs", "workplace", "negotiate-solution", "video-call", "formal"),
    ("application-gap", "Eine Lücke im Lebenslauf erklären", "work-and-jobs", "job-interview", "describe-experience", "face-to-face", "formal"),
    ("job-strengths", "Eigene Stärken im Interview beschreiben", "work-and-jobs", "job-interview", "job-interview", "face-to-face", "formal"),
    ("trial-day", "Einen Probetag im Betrieb besprechen", "work-and-jobs", "job-interview", "ask-for-information", "phone", "formal"),
    ("clinic-instruction", "Anweisungen in der Praxis verstehen", "appointments-and-health", "healthcare", "ask-for-information", "doctor-office", "formal"),
    ("care-relative", "Pflege für Angehörige organisieren", "appointments-and-health", "healthcare", "discuss-plan", "phone", "formal"),
    ("sick-child", "Ein krankes Kind in der Schule melden", "everyday-life", "school-kindergarten", "explain-problem", "phone", "formal"),
    ("parent-support", "Unterstützung für Hausaufgaben besprechen", "everyday-life", "school-kindergarten", "ask-for-help", "school-kindergarten", "formal"),
    ("daycare-hours", "Betreuungszeiten in der Kita ändern", "everyday-life", "school-kindergarten", "reschedule-appointment", "phone", "formal"),
    ("rent-repair", "Eine Reparatur in der Wohnung nachverfolgen", "housing", "housing", "ask-for-information", "phone", "formal"),
    ("moving-neighbor", "Nachbarn über einen Umzug informieren", "housing", "housing", "explain-problem", "face-to-face", "neutral"),
    ("shared-space", "Regeln im Hausflur besprechen", "housing", "conflict-resolution", "agree-disagree", "face-to-face", "neutral"),
    ("bank-transfer", "Eine Überweisung bei der Bank prüfen", "shopping", "finance-insurance", "ask-for-information", "service-counter", "formal"),
    ("insurance-choice", "Zwei Versicherungen vergleichen", "work-and-jobs", "finance-insurance", "compare-options", "face-to-face", "formal"),
    ("payment-problem", "Ein Zahlungsproblem im Online-Shop klären", "shopping", "digital-services", "explain-problem", "phone", "formal"),
    ("app-permission", "Berechtigungen in einer App verstehen", "everyday-life", "digital-services", "ask-for-help", "phone", "formal"),
    ("data-privacy", "Datenschutz bei einem Formular nachfragen", "everyday-life", "digital-services", "ask-for-information", "government-office", "formal"),
    ("bus-change", "Eine Busverbindung wegen Baustelle ändern", "everyday-life", "transport-and-travel", "compare-options", "service-counter", "formal"),
    ("travel-complaint", "Eine Beschwerde beim Reiseanbieter einreichen", "everyday-life", "complaint", "complain-politely", "phone", "formal"),
    ("family-schedule", "Einen Wochenplan in der Familie abstimmen", "everyday-life", "family", "discuss-plan", "face-to-face", "mixed"),
    ("club-task", "Aufgaben in einem Verein verteilen", "everyday-life", "social-life", "discuss-plan", "group-work", "neutral"),
]

TASK_TO_FUNCTIONS = {
    "ask-for-information": ["greet", "ask-question", "clarify", "confirm", "summarize", "close-conversation"],
    "make-suggestion": ["greet", "suggest", "explain", "justify", "agree", "summarize", "close-conversation"],
    "discuss-plan": ["greet", "suggest", "compare", "agree", "summarize", "close-conversation"],
    "agree-disagree": ["greet", "explain", "agree", "disagree", "justify", "summarize", "close-conversation"],
    "exam-discussion": ["greet", "explain", "justify", "agree", "disagree", "summarize"],
    "negotiate-solution": ["greet", "explain", "suggest", "compare", "negotiate", "summarize", "close-conversation"],
    "explain-problem": ["greet", "explain", "clarify", "answer-question", "confirm", "summarize", "close-conversation"],
    "describe-experience": ["greet", "describe", "explain", "justify", "answer-question", "close-conversation"],
    "job-interview": ["greet", "introduce", "describe", "answer-question", "justify", "close-conversation"],
    "ask-for-help": ["greet", "request", "explain", "clarify", "confirm", "close-conversation"],
    "reschedule-appointment": ["greet", "request", "explain", "suggest", "confirm", "summarize", "close-conversation"],
    "compare-options": ["greet", "ask-question", "compare", "justify", "suggest", "summarize", "close-conversation"],
    "complain-politely": ["greet", "explain", "complain", "request", "negotiate", "summarize", "close-conversation"],
}


def make_dialogue(level: str, situation_index: int, variant: int, sort_order: int) -> dict:
    key, title, topic, category, task, mode, register = DIALOGUE_SITUATIONS[situation_index]
    dialogue = base.make_dialogue(level, situation_index + 400, sort_order)
    dialogue["slug"] = f"{level.lower()}-dialogue-expansion-005-{key}-{variant:02d}"
    dialogue["title"] = f"{title} ({level})"
    dialogue["description"] = f"Practical German dialogue for {title.lower()} with B1/B2 exam speaking focus."
    dialogue["learnerGoal"] = "Practice explaining, asking follow-up questions, reacting politely, and closing with a clear next step."
    dialogue["category"] = category
    dialogue["topics"] = [topic if topic in {"everyday-life", "housing", "shopping", "work-and-jobs", "appointments-and-health"} else "everyday-life"]
    dialogue["taskType"] = task
    dialogue["interactionMode"] = mode
    dialogue["register"] = register
    dialogue["speakingFunctions"] = TASK_TO_FUNCTIONS[task]
    dialogue["sortOrder"] = sort_order
    dialogue["difficultyNote"] = f"{level} dialogue for a practical situation: {title}."
    dialogue["examRelevance"] = "Useful for B1/B2 roleplay, oral exam discussion, and realistic formal or semi-formal conversations."

    skill_focus = ["speaking", "roleplay", "exam-speaking"]
    if mode == "phone":
        skill_focus.append("phone-call")
    if category in {"workplace", "work", "job-interview"} or mode in {"workplace", "video-call"}:
        skill_focus.append("workplace-communication")
    if category == "complaint" or task == "complain-politely":
        skill_focus.append("complaint-handling")
    if category == "conflict-resolution" or task == "negotiate-solution":
        skill_focus.append("negotiation")
    if task in {"agree-disagree", "exam-discussion"}:
        skill_focus.append("discussion")
    dialogue["skillFocus"] = skill_focus
    return dialogue


def main() -> None:
    dialogues = []
    sort_order = 100000
    for level in ("B1", "B2"):
        for variant in range(1, 3):
            for situation_index in range(len(DIALOGUE_SITUATIONS)):
                dialogues.append(make_dialogue(level, situation_index, variant, sort_order))
                sort_order += 10

    package = {
        "packageId": "dialogues-exam-expansion-005-b1-b2",
        "packageName": "Dialogue Exam Expansion 005 B1 B2",
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

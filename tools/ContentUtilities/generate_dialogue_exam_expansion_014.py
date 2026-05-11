import json
from pathlib import Path

import generate_dialogue_exam_content as base


ROOT = Path(__file__).resolve().parents[2]
OUT_DIR = ROOT / "content" / "generated" / "dialogues"
OUT_FILE = OUT_DIR / "dialogues-exam-expansion-014-b1-b2.json"

DIALOGUE_SITUATIONS = [
    ("work-shift-swap", "Eine Schicht im Team tauschen", "work-and-jobs", "workplace", "negotiate-solution", "workplace", "neutral"),
    ("work-feedback-talk", "Ein Feedbackgespräch vorbereiten", "work-and-jobs", "workplace", "give-opinion", "workplace", "formal"),
    ("work-training-request", "Eine Fortbildung im Betrieb anfragen", "work-and-jobs", "workplace", "ask-for-information", "workplace", "formal"),
    ("work-deadline-risk", "Ein Terminrisiko im Projekt erklären", "work-and-jobs", "workplace", "explain-problem", "video-call", "formal"),
    ("housing-noise-rule", "Eine Hausregel wegen Lärm klären", "housing", "housing", "complain-politely", "face-to-face", "formal"),
    ("housing-cleaning-plan", "Einen Putzplan im Haus abstimmen", "housing", "housing", "discuss-plan", "face-to-face", "mixed"),
    ("housing-parking-space", "Einen Parkplatz am Haus besprechen", "housing", "housing", "negotiate-solution", "phone", "formal"),
    ("housing-heating-cost", "Eine Heizkostenabrechnung verstehen", "housing", "housing", "ask-for-help", "phone", "formal"),
    ("school-class-trip", "Eine Klassenfahrt mit der Schule besprechen", "everyday-life", "school-kindergarten", "ask-for-information", "school-kindergarten", "formal"),
    ("school-parent-talk", "Ein Elterngespräch vereinbaren", "everyday-life", "school-kindergarten", "make-appointment", "phone", "formal"),
    ("school-homework-issue", "Ein Problem mit Hausaufgaben erklären", "everyday-life", "school-kindergarten", "explain-problem", "face-to-face", "formal"),
    ("school-lunch-registration", "Ein Kind zum Mittagessen anmelden", "everyday-life", "school-kindergarten", "ask-for-help", "service-counter", "formal"),
    ("digital-password-reset", "Ein Passwort beim Onlinekonto zurücksetzen", "work-and-jobs", "digital-services", "ask-for-help", "phone", "formal"),
    ("digital-video-appointment", "Einen Videotermin technisch vorbereiten", "work-and-jobs", "digital-services", "discuss-plan", "video-call", "neutral"),
    ("digital-file-upload", "Ein Problem beim Datei-Upload erklären", "work-and-jobs", "digital-services", "explain-problem", "phone", "formal"),
    ("digital-contract-app", "Einen Vertrag in einer App prüfen", "shopping", "digital-services", "ask-for-information", "phone", "formal"),
    ("health-prescription-renewal", "Ein Rezept verlängern lassen", "appointments-and-health", "healthcare", "ask-for-help", "phone", "formal"),
    ("health-second-opinion", "Eine zweite ärztliche Meinung besprechen", "appointments-and-health", "healthcare", "compare-options", "doctor-office", "formal"),
    ("health-therapy-plan", "Einen Therapieplan verstehen", "appointments-and-health", "healthcare", "clarify", "doctor-office", "formal"),
    ("health-pharmacy-alternative", "In der Apotheke eine Alternative klären", "appointments-and-health", "healthcare", "ask-for-information", "service-counter", "formal"),
    ("government-tax-number", "Eine Steuernummer beim Amt klären", "work-and-jobs", "government-office", "ask-for-information", "government-office", "formal"),
    ("government-certificate-copy", "Eine beglaubigte Kopie beantragen", "everyday-life", "government-office", "ask-for-information", "government-office", "formal"),
    ("government-letter-deadline", "Eine Frist in einem Amtsbrief klären", "everyday-life", "government-office", "ask-for-help", "phone", "formal"),
    ("government-course-proof", "Einen Kursnachweis einreichen", "everyday-life", "government-office", "explain-problem", "government-office", "formal"),
    ("exam-work-conflict", "Einen Konflikt am Arbeitsplatz in der Prüfung lösen", "work-and-jobs", "exam-preparation", "exam-roleplay", "exam-room", "formal"),
    ("exam-health-advice", "Gesundheitsratschläge in der Prüfung vergleichen", "appointments-and-health", "exam-preparation", "exam-discussion", "exam-room", "formal"),
    ("exam-housing-complaint", "Eine Wohnungsbeschwerde als Prüfungsrolle üben", "housing", "exam-preparation", "exam-roleplay", "exam-room", "formal"),
    ("exam-school-opinion", "Über Schule und Lernen in der Prüfung sprechen", "everyday-life", "exam-preparation", "give-opinion", "exam-room", "formal"),
    ("finance-refund-delay", "Eine verspätete Rückerstattung reklamieren", "shopping", "finance-insurance", "complain-politely", "phone", "formal"),
    ("finance-contract-change", "Eine Vertragsänderung mit der Versicherung klären", "shopping", "finance-insurance", "negotiate-solution", "phone", "formal"),
]

TASK_TO_FUNCTIONS = {
    "negotiate-solution": ["greet", "explain", "suggest", "compare", "negotiate", "summarize", "close-conversation"],
    "give-opinion": ["greet", "explain", "justify", "agree", "disagree", "summarize"],
    "ask-for-information": ["greet", "ask-question", "clarify", "confirm", "summarize", "close-conversation"],
    "explain-problem": ["greet", "explain", "clarify", "answer-question", "confirm", "summarize", "close-conversation"],
    "complain-politely": ["greet", "complain", "explain", "request", "negotiate", "summarize", "close-conversation"],
    "discuss-plan": ["greet", "suggest", "compare", "agree", "summarize", "close-conversation"],
    "ask-for-help": ["greet", "request", "explain", "clarify", "confirm", "close-conversation"],
    "make-appointment": ["greet", "request", "ask-question", "confirm", "close-conversation"],
    "compare-options": ["greet", "ask-question", "compare", "justify", "suggest", "summarize", "close-conversation"],
    "clarify": ["greet", "ask-question", "clarify", "confirm", "summarize", "close-conversation"],
    "exam-roleplay": ["greet", "request", "clarify", "suggest", "confirm", "close-conversation"],
    "exam-discussion": ["greet", "explain", "justify", "agree", "disagree", "summarize"],
}

TASK_ALIASES = {
    "clarify": "ask-for-information",
}


def make_dialogue(level: str, situation_index: int, variant: int, sort_order: int) -> dict:
    key, title, topic, category, task, mode, register = DIALOGUE_SITUATIONS[situation_index]
    dialogue = base.make_dialogue(level, situation_index + 1300, sort_order)
    dialogue["slug"] = f"{level.lower()}-dialogue-expansion-014-{key}-{variant:02d}"
    dialogue["title"] = f"{title} ({level})"
    dialogue["description"] = f"B1/B2 German dialogue for {title.lower()} with practical roleplay and exam speaking focus."
    dialogue["learnerGoal"] = "Practice explaining the situation, asking precise follow-up questions, comparing options, and agreeing on a clear next step."
    dialogue["category"] = category
    dialogue["topics"] = [topic if topic in {"everyday-life", "housing", "shopping", "work-and-jobs", "appointments-and-health"} else "everyday-life"]
    dialogue["taskType"] = TASK_ALIASES.get(task, task)
    dialogue["interactionMode"] = mode
    dialogue["register"] = register
    dialogue["speakingFunctions"] = TASK_TO_FUNCTIONS[task]
    dialogue["sortOrder"] = sort_order
    dialogue["difficultyNote"] = f"{level} dialogue for a practical task: {title}."
    dialogue["examRelevance"] = "Useful for B1/B2 oral exam roleplay, problem solving, and structured discussion practice."

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
    sort_order = 280000
    for level in ("B1", "B2"):
        for variant in range(1, 3):
            for situation_index in range(len(DIALOGUE_SITUATIONS)):
                dialogues.append(make_dialogue(level, situation_index, variant, sort_order))
                sort_order += 10

    package = {
        "packageId": "dialogues-exam-expansion-014-b1-b2",
        "packageName": "Dialogue Exam Expansion 014 B1 B2",
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

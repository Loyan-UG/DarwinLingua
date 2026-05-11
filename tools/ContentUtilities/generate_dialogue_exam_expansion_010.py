import json
from pathlib import Path

import generate_dialogue_exam_content as base


ROOT = Path(__file__).resolve().parents[2]
OUT_DIR = ROOT / "content" / "generated" / "dialogues"
OUT_FILE = OUT_DIR / "dialogues-exam-expansion-010-b1-b2.json"

DIALOGUE_SITUATIONS = [
    ("work-quality", "Qualität bei einer Aufgabe verbessern", "work-and-jobs", "workplace", "make-suggestion", "workplace", "neutral"),
    ("work-client-expectation", "Erwartungen eines Kunden klären", "work-and-jobs", "workplace", "ask-for-information", "video-call", "formal"),
    ("work-new-tool", "Ein neues digitales Werkzeug im Team erklären", "work-and-jobs", "workplace", "explain-problem", "workplace", "neutral"),
    ("work-meeting-summary", "Ergebnisse eines Meetings zusammenfassen", "work-and-jobs", "workplace", "workplace-meeting", "workplace", "formal"),
    ("work-absence-cover", "Eine Vertretung bei Abwesenheit organisieren", "work-and-jobs", "workplace", "discuss-plan", "phone", "formal"),
    ("job-interview-weakness", "Eine Schwäche im Bewerbungsgespräch erklären", "work-and-jobs", "job-interview", "job-interview", "face-to-face", "formal"),
    ("job-interview-relocation", "Über einen möglichen Umzug für die Arbeit sprechen", "work-and-jobs", "job-interview", "compare-options", "face-to-face", "formal"),
    ("job-interview-start-date", "Einen Arbeitsbeginn verhandeln", "work-and-jobs", "job-interview", "negotiate-solution", "phone", "formal"),
    ("doctor-referral-delay", "Eine verspätete Überweisung beim Arzt klären", "appointments-and-health", "healthcare", "explain-problem", "doctor-office", "formal"),
    ("doctor-lab-result", "Laborwerte beim Arzt nachfragen", "appointments-and-health", "healthcare", "ask-for-information", "doctor-office", "formal"),
    ("clinic-family-member", "Einen Angehörigen in der Klinik anmelden", "appointments-and-health", "healthcare", "ask-for-help", "service-counter", "formal"),
    ("pharmacy-dosage", "Die Dosierung eines Medikaments klären", "appointments-and-health", "healthcare", "ask-for-information", "service-counter", "formal"),
    ("health-appointment-choice", "Zwischen zwei Behandlungsterminen wählen", "appointments-and-health", "healthcare", "compare-options", "phone", "formal"),
    ("office-name-change", "Eine Namensänderung beim Amt melden", "everyday-life", "government-office", "explain-problem", "government-office", "formal"),
    ("office-child-benefit", "Nach Kindergeld-Unterlagen fragen", "everyday-life", "government-office", "ask-for-information", "phone", "formal"),
    ("office-tax-class", "Eine Steuerklasse beim Amt erfragen", "everyday-life", "government-office", "ask-for-information", "government-office", "formal"),
    ("office-appointment-proof", "Einen Nachweis für einen Amtstermin bekommen", "everyday-life", "government-office", "ask-for-help", "government-office", "formal"),
    ("housing-no-hot-water", "Kein warmes Wasser in der Wohnung melden", "housing", "housing", "complain-politely", "phone", "formal"),
    ("housing-staircase", "Sauberkeit im Treppenhaus besprechen", "housing", "housing", "agree-disagree", "face-to-face", "neutral"),
    ("housing-parking", "Einen Parkplatz am Haus erfragen", "housing", "housing", "ask-for-information", "phone", "formal"),
    ("housing-lease-end", "Das Ende eines Mietvertrags besprechen", "housing", "housing", "ask-for-information", "face-to-face", "formal"),
    ("exam-agreement", "In der Prüfung eine Einigung finden", "everyday-life", "exam-preparation", "negotiate-solution", "exam-room", "formal"),
    ("exam-formal-call", "Ein formelles Telefonat in der Prüfung führen", "everyday-life", "exam-preparation", "exam-roleplay", "exam-room", "formal"),
    ("exam-compare-jobs", "Zwei Arbeitsmodelle in der Prüfung vergleichen", "work-and-jobs", "exam-preparation", "compare-options", "exam-room", "formal"),
    ("exam-health-opinion", "Eine Meinung zu gesunder Lebensweise begründen", "appointments-and-health", "exam-preparation", "give-opinion", "exam-room", "formal"),
    ("digital-support-delay", "Eine verzögerte Support-Antwort nachfragen", "everyday-life", "digital-services", "ask-for-information", "phone", "formal"),
    ("digital-account-delete", "Ein Online-Konto löschen lassen", "everyday-life", "digital-services", "request", "phone", "formal"),
    ("digital-file-access", "Zugriff auf eine Datei klären", "work-and-jobs", "digital-services", "ask-for-help", "workplace", "neutral"),
    ("family-work-balance", "Arbeit und Familie besser planen", "everyday-life", "family", "discuss-plan", "face-to-face", "mixed"),
    ("social-new-neighbor", "Neue Nachbarn freundlich kennenlernen", "housing", "social-life", "introduce-yourself", "face-to-face", "neutral"),
]

TASK_TO_FUNCTIONS = {
    "make-suggestion": ["greet", "suggest", "explain", "justify", "agree", "summarize", "close-conversation"],
    "ask-for-information": ["greet", "ask-question", "clarify", "confirm", "summarize", "close-conversation"],
    "explain-problem": ["greet", "explain", "clarify", "answer-question", "confirm", "summarize", "close-conversation"],
    "workplace-meeting": ["greet", "explain", "suggest", "compare", "confirm", "summarize", "close-conversation"],
    "discuss-plan": ["greet", "suggest", "compare", "agree", "summarize", "close-conversation"],
    "job-interview": ["greet", "introduce", "describe", "answer-question", "justify", "close-conversation"],
    "compare-options": ["greet", "ask-question", "compare", "justify", "suggest", "summarize", "close-conversation"],
    "negotiate-solution": ["greet", "explain", "suggest", "compare", "negotiate", "summarize", "close-conversation"],
    "ask-for-help": ["greet", "request", "explain", "clarify", "confirm", "close-conversation"],
    "complain-politely": ["greet", "explain", "complain", "request", "negotiate", "summarize", "close-conversation"],
    "agree-disagree": ["greet", "explain", "agree", "disagree", "justify", "summarize", "close-conversation"],
    "exam-roleplay": ["greet", "request", "clarify", "suggest", "confirm", "close-conversation"],
    "give-opinion": ["greet", "explain", "justify", "agree", "disagree", "summarize"],
    "request": ["greet", "request", "explain", "confirm", "close-conversation"],
    "introduce-yourself": ["greet", "introduce", "describe", "ask-question", "close-conversation"],
}


def make_dialogue(level: str, situation_index: int, variant: int, sort_order: int) -> dict:
    key, title, topic, category, task, mode, register = DIALOGUE_SITUATIONS[situation_index]
    dialogue = base.make_dialogue(level, situation_index + 900, sort_order)
    dialogue["slug"] = f"{level.lower()}-dialogue-expansion-010-{key}-{variant:02d}"
    dialogue["title"] = f"{title} ({level})"
    dialogue["description"] = f"Structured B1/B2 German dialogue for {title.lower()} with practical speaking goals."
    dialogue["learnerGoal"] = "Practice a clear opening, reasons, follow-up questions, polite agreement or disagreement, and a concise summary."
    dialogue["category"] = category
    dialogue["topics"] = [topic if topic in {"everyday-life", "housing", "shopping", "work-and-jobs", "appointments-and-health"} else "everyday-life"]
    dialogue["taskType"] = "ask-for-help" if task == "request" else task
    dialogue["interactionMode"] = mode
    dialogue["register"] = register
    dialogue["speakingFunctions"] = TASK_TO_FUNCTIONS[task]
    dialogue["sortOrder"] = sort_order
    dialogue["difficultyNote"] = f"{level} dialogue for a practical task: {title}."
    dialogue["examRelevance"] = "Useful for B1/B2 roleplay, workplace communication, formal calls, and oral exam preparation."

    skill_focus = ["speaking", "roleplay", "exam-speaking"]
    if mode == "phone":
        skill_focus.append("phone-call")
    if category in {"workplace", "job-interview"} or mode in {"workplace", "video-call"}:
        skill_focus.append("workplace-communication")
    if task == "complain-politely":
        skill_focus.append("complaint-handling")
    if task in {"negotiate-solution", "agree-disagree", "give-opinion"} or category == "exam-preparation":
        skill_focus.append("discussion")
    dialogue["skillFocus"] = skill_focus
    return dialogue


def main() -> None:
    dialogues = []
    sort_order = 200000
    for level in ("B1", "B2"):
        for variant in range(1, 3):
            for situation_index in range(len(DIALOGUE_SITUATIONS)):
                dialogues.append(make_dialogue(level, situation_index, variant, sort_order))
                sort_order += 10

    package = {
        "packageId": "dialogues-exam-expansion-010-b1-b2",
        "packageName": "Dialogue Exam Expansion 010 B1 B2",
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

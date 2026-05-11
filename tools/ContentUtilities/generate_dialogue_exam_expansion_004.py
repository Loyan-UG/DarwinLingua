import json
from pathlib import Path

import generate_dialogue_exam_content as base


ROOT = Path(__file__).resolve().parents[2]
OUT_DIR = ROOT / "content" / "generated" / "dialogues"
OUT_FILE = OUT_DIR / "dialogues-exam-expansion-004-b1-b2.json"

DIALOGUE_SITUATIONS = [
    ("repair-complaint", "Eine Reparatur reklamieren und eine Frist setzen", "housing", "complaint", "complain-politely", "phone", "formal"),
    ("neighbor-compromise", "Mit Nachbarn einen Kompromiss finden", "housing", "conflict-resolution", "negotiate-solution", "face-to-face", "neutral"),
    ("office-feedback", "Kritisches Feedback im Büro höflich geben", "work-and-jobs", "conflict-resolution", "give-opinion", "workplace", "neutral"),
    ("task-overload", "Zu viele Aufgaben im Team ansprechen", "work-and-jobs", "workplace", "explain-problem", "workplace", "neutral"),
    ("manager-priority", "Prioritäten mit der Leitung klären", "work-and-jobs", "workplace", "workplace-meeting", "workplace", "formal"),
    ("client-scope", "Den Umfang eines Kundenauftrags klären", "work-and-jobs", "workplace", "negotiate-solution", "video-call", "formal"),
    ("service-refund", "Eine Rückerstattung beim Kundenservice verlangen", "shopping", "complaint", "complain-politely", "phone", "formal"),
    ("broken-device", "Ein defektes Gerät im Geschäft erklären", "shopping", "shopping-and-services", "explain-problem", "service-counter", "formal"),
    ("subscription-price", "Eine Preiserhöhung bei einem Abo besprechen", "shopping", "digital-services", "agree-disagree", "phone", "formal"),
    ("online-verification", "Eine Identitätsprüfung online erklären", "everyday-life", "digital-services", "ask-for-help", "video-call", "formal"),
    ("appointment-office", "Einen Amtstermin wegen Arbeit verschieben", "everyday-life", "government-office", "reschedule-appointment", "phone", "formal"),
    ("permit-question", "Nach einer Genehmigung fragen", "everyday-life", "government-office", "ask-for-information", "government-office", "formal"),
    ("form-mistake", "Einen Fehler im Formular korrigieren", "everyday-life", "government-office", "handle-misunderstanding", "government-office", "formal"),
    ("school-grade", "Eine Note mit der Lehrkraft besprechen", "everyday-life", "school-kindergarten", "ask-for-information", "school-kindergarten", "formal"),
    ("child-support", "Unterstützung für ein Kind planen", "everyday-life", "school-kindergarten", "discuss-plan", "school-kindergarten", "formal"),
    ("doctor-referral", "Eine Überweisung beim Arzt erfragen", "appointments-and-health", "healthcare", "ask-for-information", "doctor-office", "formal"),
    ("hospital-visit", "Einen Krankenhausbesuch organisieren", "appointments-and-health", "healthcare", "discuss-plan", "phone", "formal"),
    ("mental-stress", "Stress vorsichtig beim Arzt ansprechen", "appointments-and-health", "healthcare", "explain-problem", "doctor-office", "formal"),
    ("job-reference", "Nach einem Arbeitszeugnis fragen", "work-and-jobs", "work", "ask-for-information", "workplace", "formal"),
    ("job-offer", "Ein Jobangebot vergleichen", "work-and-jobs", "job-interview", "compare-options", "face-to-face", "formal"),
    ("salary-question", "Nach Gehalt und Arbeitszeit fragen", "work-and-jobs", "job-interview", "ask-for-information", "face-to-face", "formal"),
    ("course-change", "Den Sprachkurs wechseln", "everyday-life", "integration-course", "compare-options", "classroom", "neutral"),
    ("exam-feedback", "Feedback nach einer Probeprüfung besprechen", "everyday-life", "exam-preparation", "exam-discussion", "pair-work", "neutral"),
    ("exam-role-card", "Eine Rollenkarte in der Prüfung vorbereiten", "everyday-life", "exam-preparation", "exam-roleplay", "exam-room", "formal"),
    ("bank-limit", "Ein Kartenlimit bei der Bank ändern", "shopping", "finance-insurance", "ask-for-help", "phone", "formal"),
    ("insurance-contract", "Einen Versicherungsvertrag vergleichen", "work-and-jobs", "finance-insurance", "compare-options", "face-to-face", "formal"),
    ("train-cancellation", "Nach einem Zugausfall Alternativen suchen", "everyday-life", "transport-and-travel", "compare-options", "service-counter", "formal"),
    ("travel-group", "Eine Gruppenreise planen", "everyday-life", "transport-and-travel", "discuss-plan", "group-work", "neutral"),
    ("family-care-plan", "Pflege in der Familie organisieren", "everyday-life", "family", "discuss-plan", "face-to-face", "mixed"),
    ("club-decision", "Eine Entscheidung im Verein diskutieren", "everyday-life", "social-life", "agree-disagree", "group-work", "neutral"),
]

TASK_TO_FUNCTIONS = {
    "complain-politely": ["greet", "explain", "complain", "request", "negotiate", "summarize", "close-conversation"],
    "negotiate-solution": ["greet", "explain", "suggest", "compare", "negotiate", "summarize", "close-conversation"],
    "give-opinion": ["greet", "describe", "justify", "agree", "disagree", "summarize", "close-conversation"],
    "explain-problem": ["greet", "explain", "clarify", "answer-question", "confirm", "summarize", "close-conversation"],
    "workplace-meeting": ["greet", "explain", "suggest", "compare", "confirm", "summarize", "close-conversation"],
    "compare-options": ["greet", "ask-question", "compare", "justify", "suggest", "summarize", "close-conversation"],
    "ask-for-information": ["greet", "ask-question", "clarify", "confirm", "summarize", "close-conversation"],
    "ask-for-help": ["greet", "request", "explain", "clarify", "confirm", "close-conversation"],
    "reschedule-appointment": ["greet", "request", "explain", "suggest", "confirm", "summarize", "close-conversation"],
    "discuss-plan": ["greet", "suggest", "compare", "agree", "summarize", "close-conversation"],
    "agree-disagree": ["greet", "explain", "agree", "disagree", "justify", "summarize", "close-conversation"],
    "exam-discussion": ["greet", "explain", "justify", "agree", "disagree", "summarize"],
    "exam-roleplay": ["greet", "request", "clarify", "suggest", "confirm", "close-conversation"],
}


def make_dialogue(level: str, situation_index: int, variant: int, sort_order: int) -> dict:
    key, title, topic, category, task, mode, register = DIALOGUE_SITUATIONS[situation_index]
    dialogue = base.make_dialogue(level, situation_index + 300, sort_order)
    dialogue["slug"] = f"{level.lower()}-dialogue-expansion-004-{key}-{variant:02d}"
    dialogue["title"] = f"{title} ({level})"
    dialogue["description"] = f"Exam-oriented German conversation practice for {title.lower()} at {level} level."
    dialogue["learnerGoal"] = "Practice a realistic dialogue with a clear request, a reason, a polite reaction, and a final agreement."
    dialogue["category"] = category
    dialogue["topics"] = [topic if topic in {"everyday-life", "housing", "shopping", "work-and-jobs", "appointments-and-health"} else "everyday-life"]
    dialogue["taskType"] = task
    dialogue["interactionMode"] = mode
    dialogue["register"] = register
    dialogue["speakingFunctions"] = TASK_TO_FUNCTIONS.get(task, dialogue["speakingFunctions"])
    dialogue["sortOrder"] = sort_order
    dialogue["difficultyNote"] = f"{level} practice with a practical situation: {title}."
    dialogue["examRelevance"] = "Useful for B1/B2 oral exam roleplay, service interactions, workplace conversations, and polite problem solving."

    skill_focus = ["speaking", "roleplay", "exam-speaking"]
    if mode == "phone":
        skill_focus.append("phone-call")
    if category in {"workplace", "work", "job-interview"} or mode in {"workplace", "video-call"}:
        skill_focus.append("workplace-communication")
    if category == "complaint" or task == "complain-politely":
        skill_focus.append("complaint-handling")
    if category == "conflict-resolution" or task == "negotiate-solution":
        skill_focus.append("negotiation")
    if task in {"give-opinion", "agree-disagree", "exam-discussion"}:
        skill_focus.append("discussion")
    dialogue["skillFocus"] = skill_focus
    return dialogue


def main() -> None:
    dialogues = []
    sort_order = 80000
    for level in ("B1", "B2"):
        for variant in range(1, 3):
            for situation_index in range(len(DIALOGUE_SITUATIONS)):
                dialogues.append(make_dialogue(level, situation_index, variant, sort_order))
                sort_order += 10

    package = {
        "packageId": "dialogues-exam-expansion-004-b1-b2",
        "packageName": "Dialogue Exam Expansion 004 B1 B2",
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

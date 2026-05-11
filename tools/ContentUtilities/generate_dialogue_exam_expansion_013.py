import json
from pathlib import Path

import generate_dialogue_exam_content as base


ROOT = Path(__file__).resolve().parents[2]
OUT_DIR = ROOT / "content" / "generated" / "dialogues"
OUT_FILE = OUT_DIR / "dialogues-exam-expansion-013-b1-b2.json"

DIALOGUE_SITUATIONS = [
    ("family-childcare", "Kinderbetreuung in der Familie abstimmen", "everyday-life", "family", "discuss-plan", "face-to-face", "mixed"),
    ("family-visit-plan", "Einen Familienbesuch planen", "everyday-life", "family", "discuss-plan", "phone", "informal"),
    ("family-money", "Eine größere Ausgabe in der Familie besprechen", "shopping", "family", "compare-options", "face-to-face", "mixed"),
    ("family-doctor-help", "Hilfe für einen Arzttermin organisieren", "appointments-and-health", "family", "ask-for-help", "phone", "formal"),
    ("social-invitation", "Eine Einladung freundlich annehmen", "everyday-life", "social-life", "agree-disagree", "phone", "informal"),
    ("social-club-task", "Eine Aufgabe im Verein übernehmen", "everyday-life", "social-life", "make-suggestion", "group-work", "neutral"),
    ("social-neighbor-help", "Nachbarschaftshilfe organisieren", "housing", "social-life", "ask-for-help", "face-to-face", "neutral"),
    ("social-event-feedback", "Feedback nach einer Veranstaltung geben", "everyday-life", "social-life", "give-opinion", "group-work", "neutral"),
    ("shopping-installment", "Eine Ratenzahlung im Geschäft besprechen", "shopping", "shopping-and-services", "negotiate-solution", "service-counter", "formal"),
    ("shopping-product-advice", "Sich vor einem Kauf beraten lassen", "shopping", "shopping-and-services", "ask-for-information", "service-counter", "neutral"),
    ("shopping-price-match", "Nach einem günstigeren Preis fragen", "shopping", "shopping-and-services", "negotiate-solution", "service-counter", "formal"),
    ("shopping-pickup", "Eine Abholung im Geschäft organisieren", "shopping", "shopping-and-services", "make-appointment", "phone", "formal"),
    ("finance-standing-order", "Einen Dauerauftrag bei der Bank ändern", "shopping", "finance-insurance", "ask-for-help", "phone", "formal"),
    ("finance-card-limit", "Ein Kartenlimit begründen", "shopping", "finance-insurance", "explain-problem", "phone", "formal"),
    ("finance-insurance-choice", "Eine Versicherung mit Beratung vergleichen", "work-and-jobs", "finance-insurance", "compare-options", "face-to-face", "formal"),
    ("finance-payment-plan", "Einen Zahlungsplan vereinbaren", "shopping", "finance-insurance", "negotiate-solution", "phone", "formal"),
    ("travel-bike-ticket", "Ein Fahrradticket im Zug klären", "everyday-life", "transport-and-travel", "ask-for-information", "service-counter", "formal"),
    ("travel-group-discount", "Nach Gruppenermäßigung fragen", "everyday-life", "transport-and-travel", "ask-for-information", "service-counter", "formal"),
    ("travel-route-change", "Eine Route wegen Baustelle ändern", "everyday-life", "transport-and-travel", "compare-options", "face-to-face", "neutral"),
    ("travel-hotel-cancel", "Eine Hotelbuchung stornieren", "everyday-life", "transport-and-travel", "refuse-politely", "phone", "formal"),
    ("government-family-document", "Ein Familiendokument beim Amt beantragen", "everyday-life", "government-office", "ask-for-information", "government-office", "formal"),
    ("government-fee", "Eine Verwaltungsgebühr klären", "shopping", "government-office", "ask-for-information", "government-office", "formal"),
    ("health-family-call", "Für ein Familienmitglied beim Arzt anrufen", "appointments-and-health", "healthcare", "make-appointment", "phone", "formal"),
    ("health-insurance-letter", "Einen Brief der Krankenkasse verstehen", "appointments-and-health", "healthcare", "ask-for-help", "phone", "formal"),
    ("work-team-lunch", "Ein Teamessen organisieren", "work-and-jobs", "workplace", "discuss-plan", "workplace", "neutral"),
    ("work-client-budget", "Ein Budget mit dem Kunden abstimmen", "work-and-jobs", "workplace", "negotiate-solution", "video-call", "formal"),
    ("exam-family-opinion", "Über Familie und Beruf in der Prüfung sprechen", "everyday-life", "exam-preparation", "give-opinion", "exam-room", "formal"),
    ("exam-shopping-role", "Eine Reklamation als Prüfungsrolle üben", "shopping", "exam-preparation", "exam-roleplay", "exam-room", "formal"),
    ("exam-travel-plan", "Eine Reiseplanung in der Prüfung diskutieren", "everyday-life", "exam-preparation", "exam-discussion", "exam-room", "formal"),
    ("exam-money-compare", "Zwei Zahlungsarten in der Prüfung vergleichen", "shopping", "exam-preparation", "compare-options", "exam-room", "formal"),
]

TASK_TO_FUNCTIONS = {
    "discuss-plan": ["greet", "suggest", "compare", "agree", "summarize", "close-conversation"],
    "compare-options": ["greet", "ask-question", "compare", "justify", "suggest", "summarize", "close-conversation"],
    "ask-for-help": ["greet", "request", "explain", "clarify", "confirm", "close-conversation"],
    "agree-disagree": ["greet", "explain", "agree", "disagree", "justify", "summarize", "close-conversation"],
    "make-suggestion": ["greet", "suggest", "explain", "justify", "agree", "summarize", "close-conversation"],
    "give-opinion": ["greet", "explain", "justify", "agree", "disagree", "summarize"],
    "negotiate-solution": ["greet", "explain", "suggest", "compare", "negotiate", "summarize", "close-conversation"],
    "ask-for-information": ["greet", "ask-question", "clarify", "confirm", "summarize", "close-conversation"],
    "make-appointment": ["greet", "request", "ask-question", "confirm", "close-conversation"],
    "explain-problem": ["greet", "explain", "clarify", "answer-question", "confirm", "summarize", "close-conversation"],
    "refuse-politely": ["greet", "explain", "apologize", "request", "close-conversation"],
    "exam-roleplay": ["greet", "request", "clarify", "suggest", "confirm", "close-conversation"],
    "exam-discussion": ["greet", "explain", "justify", "agree", "disagree", "summarize"],
}


def make_dialogue(level: str, situation_index: int, variant: int, sort_order: int) -> dict:
    key, title, topic, category, task, mode, register = DIALOGUE_SITUATIONS[situation_index]
    dialogue = base.make_dialogue(level, situation_index + 1200, sort_order)
    dialogue["slug"] = f"{level.lower()}-dialogue-expansion-013-{key}-{variant:02d}"
    dialogue["title"] = f"{title} ({level})"
    dialogue["description"] = f"B1/B2 German dialogue for {title.lower()} with practical roleplay and exam speaking focus."
    dialogue["learnerGoal"] = "Practice explaining needs, comparing options, asking follow-up questions, and confirming a practical result."
    dialogue["category"] = category
    dialogue["topics"] = [topic if topic in {"everyday-life", "housing", "shopping", "work-and-jobs", "appointments-and-health"} else "everyday-life"]
    dialogue["taskType"] = task
    dialogue["interactionMode"] = mode
    dialogue["register"] = register
    dialogue["speakingFunctions"] = TASK_TO_FUNCTIONS[task]
    dialogue["sortOrder"] = sort_order
    dialogue["difficultyNote"] = f"{level} dialogue for a practical task: {title}."
    dialogue["examRelevance"] = "Useful for B1/B2 oral exam roleplay, practical service conversations, and structured discussion practice."

    skill_focus = ["speaking", "roleplay", "exam-speaking"]
    if mode == "phone":
        skill_focus.append("phone-call")
    if category == "workplace" or mode in {"workplace", "video-call"}:
        skill_focus.append("workplace-communication")
    if task == "negotiate-solution":
        skill_focus.append("negotiation")
    if category == "exam-preparation" or task in {"give-opinion", "agree-disagree", "exam-discussion"}:
        skill_focus.append("discussion")
    dialogue["skillFocus"] = skill_focus
    return dialogue


def main() -> None:
    dialogues = []
    sort_order = 260000
    for level in ("B1", "B2"):
        for variant in range(1, 3):
            for situation_index in range(len(DIALOGUE_SITUATIONS)):
                dialogues.append(make_dialogue(level, situation_index, variant, sort_order))
                sort_order += 10

    package = {
        "packageId": "dialogues-exam-expansion-013-b1-b2",
        "packageName": "Dialogue Exam Expansion 013 B1 B2",
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

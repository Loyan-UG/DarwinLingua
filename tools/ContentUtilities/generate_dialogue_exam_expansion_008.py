import json
from pathlib import Path

import generate_dialogue_exam_content as base


ROOT = Path(__file__).resolve().parents[2]
OUT_DIR = ROOT / "content" / "generated" / "dialogues"
OUT_FILE = OUT_DIR / "dialogues-exam-expansion-008-b1-b2.json"

DIALOGUE_SITUATIONS = [
    ("restaurant-reservation", "Einen Tisch im Restaurant reservieren", "everyday-life", "food-restaurant", "make-appointment", "phone", "formal"),
    ("restaurant-complaint", "Eine falsche Bestellung im Restaurant ansprechen", "everyday-life", "food-restaurant", "complain-politely", "face-to-face", "neutral"),
    ("restaurant-bill", "Eine Rechnung im Restaurant klären", "shopping", "food-restaurant", "ask-for-information", "face-to-face", "neutral"),
    ("hotel-breakfast", "Im Hotel nach Frühstückszeiten fragen", "everyday-life", "food-restaurant", "ask-for-information", "service-counter", "formal"),
    ("shopping-size", "Im Geschäft eine andere Größe suchen", "shopping", "shopping-and-services", "ask-for-help", "service-counter", "neutral"),
    ("warranty-claim", "Eine Garantie im Geschäft nutzen", "shopping", "shopping-and-services", "complain-politely", "service-counter", "formal"),
    ("repair-cost", "Kosten für eine Reparatur vergleichen", "shopping", "shopping-and-services", "compare-options", "service-counter", "formal"),
    ("online-return", "Eine Online-Rücksendung erklären", "shopping", "digital-services", "explain-problem", "phone", "formal"),
    ("app-subscription", "Ein App-Abo kündigen", "shopping", "digital-services", "refuse-politely", "phone", "formal"),
    ("cloud-storage", "Speicherplatz in einer App vergleichen", "everyday-life", "digital-services", "compare-options", "phone", "formal"),
    ("support-chat", "Ein Problem im Support-Chat beschreiben", "everyday-life", "digital-services", "explain-problem", "video-call", "neutral"),
    ("bank-card-lost", "Eine verlorene Bankkarte sperren lassen", "shopping", "finance-insurance", "ask-for-help", "phone", "formal"),
    ("bank-loan", "Nach einem kleinen Kredit fragen", "shopping", "finance-insurance", "ask-for-information", "face-to-face", "formal"),
    ("insurance-accident", "Einen Unfall bei der Versicherung melden", "work-and-jobs", "finance-insurance", "explain-problem", "phone", "formal"),
    ("invoice-correction", "Eine Rechnungskorrektur anfordern", "work-and-jobs", "finance-insurance", "request", "phone", "formal"),
    ("friend-cancel", "Ein Treffen mit Freunden absagen", "everyday-life", "social-life", "refuse-politely", "phone", "informal"),
    ("birthday-plan", "Eine Geburtstagsfeier planen", "everyday-life", "social-life", "discuss-plan", "group-work", "informal"),
    ("club-membership", "Eine Mitgliedschaft im Verein besprechen", "everyday-life", "social-life", "ask-for-information", "face-to-face", "neutral"),
    ("volunteer-schedule", "Einen Einsatzplan im Ehrenamt abstimmen", "everyday-life", "social-life", "discuss-plan", "group-work", "neutral"),
    ("office-lunch", "Ein gemeinsames Mittagessen im Team planen", "work-and-jobs", "workplace", "discuss-plan", "workplace", "neutral"),
    ("office-presentation", "Eine Präsentation im Büro vorbereiten", "work-and-jobs", "workplace", "workplace-meeting", "workplace", "formal"),
    ("office-disagreement", "In einer Besprechung höflich widersprechen", "work-and-jobs", "workplace", "agree-disagree", "workplace", "formal"),
    ("office-request-help", "Kollegen um Unterstützung bitten", "work-and-jobs", "workplace", "ask-for-help", "workplace", "neutral"),
    ("exam-restaurant-role", "Eine Restaurantsituation in der Prüfung spielen", "everyday-life", "exam-preparation", "exam-roleplay", "exam-room", "formal"),
    ("exam-shopping-role", "Eine Einkaufssituation in der Prüfung spielen", "shopping", "exam-preparation", "exam-roleplay", "exam-room", "formal"),
    ("exam-money-opinion", "Über Geld und Sparen sprechen", "shopping", "exam-preparation", "give-opinion", "exam-room", "formal"),
    ("exam-digital-opinion", "Über digitale Gewohnheiten sprechen", "everyday-life", "exam-preparation", "give-opinion", "exam-room", "formal"),
    ("health-food", "Gesunde Ernährung beim Arzt besprechen", "appointments-and-health", "healthcare", "ask-for-information", "doctor-office", "formal"),
    ("nutrition-course", "Einen Ernährungskurs vergleichen", "appointments-and-health", "healthcare", "compare-options", "phone", "formal"),
    ("family-restaurant", "Ein Restaurant für die Familie auswählen", "everyday-life", "family", "compare-options", "face-to-face", "mixed"),
]

TASK_TO_FUNCTIONS = {
    "make-appointment": ["greet", "request", "ask-question", "confirm", "close-conversation"],
    "complain-politely": ["greet", "explain", "complain", "request", "negotiate", "summarize", "close-conversation"],
    "ask-for-information": ["greet", "ask-question", "clarify", "confirm", "summarize", "close-conversation"],
    "ask-for-help": ["greet", "request", "explain", "clarify", "confirm", "close-conversation"],
    "compare-options": ["greet", "ask-question", "compare", "justify", "suggest", "summarize", "close-conversation"],
    "explain-problem": ["greet", "explain", "clarify", "answer-question", "confirm", "summarize", "close-conversation"],
    "refuse-politely": ["greet", "explain", "apologize", "request", "close-conversation"],
    "request": ["greet", "request", "explain", "clarify", "confirm", "close-conversation"],
    "discuss-plan": ["greet", "suggest", "compare", "agree", "summarize", "close-conversation"],
    "workplace-meeting": ["greet", "explain", "suggest", "compare", "confirm", "summarize", "close-conversation"],
    "agree-disagree": ["greet", "explain", "agree", "disagree", "justify", "summarize", "close-conversation"],
    "exam-roleplay": ["greet", "request", "clarify", "suggest", "confirm", "close-conversation"],
    "give-opinion": ["greet", "explain", "justify", "agree", "disagree", "summarize"],
}


def make_dialogue(level: str, situation_index: int, variant: int, sort_order: int) -> dict:
    key, title, topic, category, task, mode, register = DIALOGUE_SITUATIONS[situation_index]
    dialogue = base.make_dialogue(level, situation_index + 700, sort_order)
    dialogue["slug"] = f"{level.lower()}-dialogue-expansion-008-{key}-{variant:02d}"
    dialogue["title"] = f"{title} ({level})"
    dialogue["description"] = f"B1/B2 German dialogue for {title.lower()} with practical speaking and exam roleplay focus."
    dialogue["learnerGoal"] = "Practice asking, explaining, reacting politely, comparing options, and confirming the result."
    dialogue["category"] = category
    dialogue["topics"] = [topic if topic in {"everyday-life", "housing", "shopping", "work-and-jobs", "appointments-and-health"} else "everyday-life"]
    dialogue["taskType"] = "ask-for-help" if task == "request" else task
    dialogue["interactionMode"] = mode
    dialogue["register"] = register
    dialogue["speakingFunctions"] = TASK_TO_FUNCTIONS[task]
    dialogue["sortOrder"] = sort_order
    dialogue["difficultyNote"] = f"{level} practical dialogue: {title}."
    dialogue["examRelevance"] = "Useful for B1/B2 roleplay, service interactions, workplace discussions, and oral exam preparation."

    skill_focus = ["speaking", "roleplay", "exam-speaking"]
    if mode == "phone":
        skill_focus.append("phone-call")
    if category == "workplace" or mode in {"workplace", "video-call"}:
        skill_focus.append("workplace-communication")
    if task == "complain-politely":
        skill_focus.append("complaint-handling")
    if category == "exam-preparation" or task in {"give-opinion", "agree-disagree"}:
        skill_focus.append("discussion")
    dialogue["skillFocus"] = skill_focus
    return dialogue


def main() -> None:
    dialogues = []
    sort_order = 160000
    for level in ("B1", "B2"):
        for variant in range(1, 3):
            for situation_index in range(len(DIALOGUE_SITUATIONS)):
                dialogues.append(make_dialogue(level, situation_index, variant, sort_order))
                sort_order += 10

    package = {
        "packageId": "dialogues-exam-expansion-008-b1-b2",
        "packageName": "Dialogue Exam Expansion 008 B1 B2",
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

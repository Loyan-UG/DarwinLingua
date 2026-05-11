import json
from pathlib import Path

import generate_dialogue_exam_content as base


ROOT = Path(__file__).resolve().parents[2]
OUT_DIR = ROOT / "content" / "generated" / "dialogues"
OUT_FILE = OUT_DIR / "dialogues-exam-expansion-003-b1-b2.json"

DIALOGUE_SITUATIONS = [
    ("doctor-follow-up", "Einen Folgetermin beim Arzt vereinbaren", "appointments-and-health", "healthcare", "make-appointment", "doctor-office", "formal"),
    ("clinic-wait-time", "Eine lange Wartezeit in der Klinik ansprechen", "appointments-and-health", "healthcare", "complain-politely", "service-counter", "formal"),
    ("pharmacy-prescription", "Ein Rezept in der Apotheke klären", "appointments-and-health", "healthcare", "ask-for-information", "service-counter", "formal"),
    ("insurance-reimbursement", "Eine Kostenerstattung bei der Krankenkasse erklären", "appointments-and-health", "finance-insurance", "explain-problem", "phone", "formal"),
    ("office-vacation", "Urlaub im Büro abstimmen", "work-and-jobs", "workplace", "discuss-plan", "workplace", "neutral"),
    ("meeting-agenda", "Eine Tagesordnung für das Meeting vorschlagen", "work-and-jobs", "workplace", "make-suggestion", "workplace", "formal"),
    ("deadline-risk", "Ein Risiko bei einer Frist erklären", "work-and-jobs", "workplace", "explain-problem", "workplace", "neutral"),
    ("customer-complaint", "Auf eine Kundenbeschwerde reagieren", "work-and-jobs", "workplace", "complain-politely", "phone", "formal"),
    ("software-ticket", "Ein IT-Ticket verständlich beschreiben", "work-and-jobs", "digital-services", "explain-problem", "workplace", "neutral"),
    ("printer-problem", "Ein Druckerproblem im Büro melden", "work-and-jobs", "digital-services", "ask-for-help", "workplace", "neutral"),
    ("online-banking", "Online-Banking mit der Bank klären", "shopping", "finance-insurance", "ask-for-help", "phone", "formal"),
    ("payment-installment", "Eine Ratenzahlung besprechen", "shopping", "finance-insurance", "negotiate-solution", "phone", "formal"),
    ("tax-deadline", "Eine Frist beim Amt verlängern", "everyday-life", "government-office", "reschedule-appointment", "government-office", "formal"),
    ("official-document", "Ein fehlendes Dokument nachreichen", "everyday-life", "government-office", "explain-problem", "government-office", "formal"),
    ("language-certificate", "Ein Sprachzertifikat anerkennen lassen", "everyday-life", "integration-course", "ask-for-information", "government-office", "formal"),
    ("exam-partner", "Mit dem Prüfungspartner eine Aufgabe vorbereiten", "everyday-life", "exam-preparation", "exam-discussion", "pair-work", "neutral"),
    ("exam-opinion", "In der Prüfung eine Meinung begründen", "everyday-life", "exam-preparation", "give-opinion", "exam-room", "formal"),
    ("exam-compromise", "In der Prüfung einen Kompromiss finden", "everyday-life", "exam-preparation", "negotiate-solution", "exam-room", "formal"),
    ("school-absence", "Eine Fehlzeit in der Schule erklären", "everyday-life", "school-kindergarten", "explain-problem", "phone", "formal"),
    ("kindergarten-conflict", "Ein Problem in der Kita ruhig besprechen", "everyday-life", "school-kindergarten", "handle-misunderstanding", "school-kindergarten", "formal"),
    ("rent-contract", "Fragen zum Mietvertrag stellen", "housing", "housing", "ask-for-information", "face-to-face", "formal"),
    ("moving-date", "Einen Umzugstermin koordinieren", "housing", "housing", "discuss-plan", "phone", "neutral"),
    ("utility-bill", "Eine Nebenkostenabrechnung verstehen", "housing", "housing", "ask-for-information", "phone", "formal"),
    ("train-strike", "Eine Reise wegen Streik neu planen", "everyday-life", "transport-and-travel", "compare-options", "face-to-face", "neutral"),
    ("airport-security", "Eine Frage am Flughafen klären", "everyday-life", "transport-and-travel", "ask-for-information", "service-counter", "formal"),
    ("bike-repair", "Eine Fahrradreparatur besprechen", "everyday-life", "shopping-and-services", "make-appointment", "service-counter", "neutral"),
    ("mobile-contract", "Einen Handyvertrag vergleichen", "shopping", "digital-services", "compare-options", "service-counter", "formal"),
    ("subscription-cancel", "Ein Abo höflich kündigen", "shopping", "digital-services", "refuse-politely", "phone", "formal"),
    ("community-event", "Ein Fest in der Nachbarschaft organisieren", "everyday-life", "social-life", "discuss-plan", "group-work", "neutral"),
    ("family-decision", "Eine Familienentscheidung begründen", "everyday-life", "family", "agree-disagree", "face-to-face", "mixed"),
]


def make_dialogue(level: str, situation_index: int, variant: int, sort_order: int) -> dict:
    key, title, topic, category, task, mode, register = DIALOGUE_SITUATIONS[situation_index]
    dialogue = base.make_dialogue(level, situation_index + 200, sort_order)
    dialogue["slug"] = f"{level.lower()}-dialogue-expansion-003-{key}-{variant:02d}"
    dialogue["title"] = f"{title} ({level})"
    dialogue["description"] = f"Structured German conversation practice for {title.lower()} at {level} level."
    dialogue["learnerGoal"] = "Practice a clear request, a reason, a polite reaction, a follow-up question, and a short final summary."
    dialogue["category"] = category
    dialogue["topics"] = [topic if topic in {"everyday-life", "housing", "shopping", "work-and-jobs", "appointments-and-health"} else "everyday-life"]
    dialogue["taskType"] = task
    dialogue["interactionMode"] = mode
    dialogue["register"] = register
    dialogue["sortOrder"] = sort_order
    dialogue["examRelevance"] = "Useful for B1/B2 oral exam preparation, formal roleplay, and practical daily-life speaking tasks."

    skill_focus = ["speaking", "roleplay", "exam-speaking"]
    if mode == "phone":
        skill_focus.append("phone-call")
    if category in {"workplace", "digital-services"} or mode == "workplace":
        skill_focus.append("workplace-communication")
    if task == "complain-politely":
        skill_focus.append("complaint-handling")
    if task == "negotiate-solution":
        skill_focus.append("negotiation")
    if task in {"give-opinion", "agree-disagree", "exam-discussion"}:
        skill_focus.append("discussion")
    dialogue["skillFocus"] = skill_focus
    return dialogue


def main() -> None:
    dialogues = []
    sort_order = 60000
    for level in ("B1", "B2"):
        for variant in range(1, 3):
            for situation_index in range(len(DIALOGUE_SITUATIONS)):
                dialogues.append(make_dialogue(level, situation_index, variant, sort_order))
                sort_order += 10

    package = {
        "packageId": "dialogues-exam-expansion-003-b1-b2",
        "packageName": "Dialogue Exam Expansion 003 B1 B2",
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

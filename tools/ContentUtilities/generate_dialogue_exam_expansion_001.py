import json
from pathlib import Path

import generate_dialogue_exam_content as base


ROOT = Path(__file__).resolve().parents[2]
OUT_DIR = ROOT / "content" / "generated" / "dialogues"
OUT_FILE = OUT_DIR / "dialogues-exam-expansion-001-b1-b2.json"

DIALOGUE_SITUATIONS = [
    ("course-delay", "Eine Verspätung im Integrationskurs erklären", "everyday-life", "integration-course", "explain-problem", "classroom", "formal"),
    ("job-probation", "Über die Probezeit sprechen", "work-and-jobs", "job-interview", "job-interview", "face-to-face", "formal"),
    ("salary-conditions", "Arbeitsbedingungen höflich verhandeln", "work-and-jobs", "job-interview", "negotiate-solution", "face-to-face", "formal"),
    ("team-conflict", "Einen Konflikt im Team klären", "work-and-jobs", "workplace", "negotiate-solution", "workplace", "neutral"),
    ("home-office-rules", "Homeoffice-Regeln vergleichen", "work-and-jobs", "workplace", "compare-options", "workplace", "neutral"),
    ("project-priority", "Prioritäten im Projekt besprechen", "work-and-jobs", "workplace", "workplace-meeting", "workplace", "neutral"),
    ("customer-delay", "Eine verspätete Lieferung erklären", "work-and-jobs", "workplace", "explain-problem", "phone", "formal"),
    ("insurance-claim", "Einen Schaden bei der Versicherung melden", "work-and-jobs", "finance-insurance", "explain-problem", "phone", "formal"),
    ("bank-fees", "Gebühren bei der Bank nachfragen", "shopping", "finance-insurance", "ask-for-information", "service-counter", "formal"),
    ("rent-increase", "Über eine Mieterhöhung sprechen", "housing", "housing", "complain-politely", "phone", "formal"),
    ("repair-date", "Einen Reparaturtermin abstimmen", "housing", "housing", "make-appointment", "phone", "formal"),
    ("school-parent", "Ein Gespräch mit der Lehrkraft führen", "everyday-life", "school-kindergarten", "explain-problem", "school-kindergarten", "formal"),
    ("kindergarten-place", "Nach einem Kita-Platz fragen", "everyday-life", "school-kindergarten", "ask-for-information", "government-office", "formal"),
    ("doctor-result", "Über Untersuchungsergebnisse sprechen", "appointments-and-health", "healthcare", "ask-for-information", "doctor-office", "formal"),
    ("pharmacy-advice", "In der Apotheke Beratung bekommen", "appointments-and-health", "healthcare", "ask-for-help", "service-counter", "formal"),
    ("train-refund", "Eine Erstattung wegen Zugverspätung beantragen", "everyday-life", "transport-and-travel", "complain-politely", "service-counter", "formal"),
    ("hotel-problem", "Ein Problem im Hotel höflich lösen", "everyday-life", "transport-and-travel", "complain-politely", "service-counter", "formal"),
    ("online-order", "Eine Online-Bestellung reklamieren", "shopping", "shopping-and-services", "complain-politely", "phone", "formal"),
    ("return-policy", "Rückgabe und Garantie vergleichen", "shopping", "shopping-and-services", "compare-options", "service-counter", "formal"),
    ("digital-form", "Ein digitales Formular ausfüllen", "everyday-life", "government-office", "ask-for-help", "government-office", "formal"),
    ("residence-card", "Nach dem Aufenthaltstitel fragen", "everyday-life", "government-office", "ask-for-information", "government-office", "formal"),
    ("course-exam", "Eine mündliche Prüfung vorbereiten", "everyday-life", "exam-preparation", "exam-roleplay", "classroom", "neutral"),
    ("group-discussion", "In der Gruppe eine Meinung begründen", "everyday-life", "exam-preparation", "exam-discussion", "group-work", "neutral"),
    ("volunteer-plan", "Ein ehrenamtliches Projekt planen", "everyday-life", "social-life", "discuss-plan", "group-work", "neutral"),
    ("family-care", "Betreuung in der Familie organisieren", "everyday-life", "family", "discuss-plan", "face-to-face", "mixed"),
    ("neighbor-noise", "Lärm mit Nachbarn besprechen", "housing", "housing", "complain-politely", "face-to-face", "neutral"),
    ("workplace-mistake", "Ein Missverständnis am Arbeitsplatz klären", "work-and-jobs", "workplace", "handle-misunderstanding", "workplace", "neutral"),
    ("client-offer", "Ein Angebot mit einem Kunden besprechen", "work-and-jobs", "workplace", "negotiate-solution", "video-call", "formal"),
    ("training-plan", "Eine Fortbildung planen", "work-and-jobs", "education", "discuss-plan", "workplace", "formal"),
    ("exam-picture", "Ein Bild in der Prüfung beschreiben", "everyday-life", "exam-preparation", "exam-roleplay", "exam-room", "formal"),
]


def make_expansion_dialogue(level: str, situation_index: int, variant: int, sort_order: int) -> dict:
    key, title, topic, category, task, mode, register = DIALOGUE_SITUATIONS[situation_index]
    dialogue = base.make_dialogue(level, situation_index, sort_order)
    dialogue["slug"] = f"{level.lower()}-dialogue-expansion-001-{key}-{variant:02d}"
    dialogue["title"] = f"{title} ({level})"
    dialogue["description"] = f"Practical exam-oriented German dialogue for {title.lower()} at {level} level."
    dialogue["category"] = category
    dialogue["topics"] = [topic if topic in {"everyday-life", "housing", "shopping", "work-and-jobs", "appointments-and-health"} else "everyday-life"]
    dialogue["taskType"] = task
    dialogue["interactionMode"] = mode
    dialogue["register"] = register
    dialogue["sortOrder"] = sort_order

    skill_focus = ["speaking", "roleplay", "exam-speaking"]
    if mode == "phone":
        skill_focus.append("phone-call")
    if mode in {"workplace", "video-call"} or category in {"workplace", "job-interview"}:
        skill_focus.append("workplace-communication")
    if task == "complain-politely":
        skill_focus.append("complaint-handling")
    if task == "negotiate-solution":
        skill_focus.append("negotiation")
    if task == "exam-discussion":
        skill_focus.append("discussion")
    dialogue["skillFocus"] = skill_focus

    dialogue["learnerGoal"] = "Practice a realistic German dialogue with reasons, clarification, polite reaction, and a final summary."
    dialogue["examRelevance"] = "Supports B1/B2 oral exam roleplay, workplace speaking, and structured partner practice."
    return dialogue


def main() -> None:
    dialogues = []
    sort_order = 20000
    for level in ("B1", "B2"):
        for variant in range(1, 3):
            for situation_index in range(len(DIALOGUE_SITUATIONS)):
                dialogues.append(make_expansion_dialogue(level, situation_index, variant, sort_order))
                sort_order += 10

    package = {
        "packageId": "dialogues-exam-expansion-001-b1-b2",
        "packageName": "Dialogue Exam Expansion 001 B1 B2",
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

import json
from pathlib import Path

import generate_dialogue_exam_content as base


ROOT = Path(__file__).resolve().parents[2]
OUT_DIR = ROOT / "content" / "generated" / "dialogues"
OUT_FILE = OUT_DIR / "dialogues-exam-expansion-002-b1-b2.json"

DIALOGUE_SITUATIONS = [
    ("job-sick-note", "Eine Krankmeldung im Betrieb erklären", "work-and-jobs", "workplace", "explain-problem", "phone", "formal"),
    ("shift-change", "Einen Schichttausch besprechen", "work-and-jobs", "workplace", "negotiate-solution", "workplace", "neutral"),
    ("team-feedback", "Feedback nach einer Aufgabe geben", "work-and-jobs", "workplace", "give-opinion", "workplace", "neutral"),
    ("training-budget", "Ein Fortbildungsbudget begründen", "work-and-jobs", "education", "negotiate-solution", "workplace", "formal"),
    ("delivery-damage", "Beschädigte Ware reklamieren", "work-and-jobs", "shopping-and-services", "complain-politely", "phone", "formal"),
    ("supplier-delay", "Mit einem Lieferanten eine Lösung finden", "work-and-jobs", "workplace", "negotiate-solution", "video-call", "formal"),
    ("invoice-question", "Eine Frage zur Rechnung klären", "work-and-jobs", "finance-insurance", "ask-for-information", "phone", "formal"),
    ("tax-office-letter", "Einen Brief vom Finanzamt verstehen", "everyday-life", "government-office", "ask-for-help", "government-office", "formal"),
    ("registration-error", "Einen Fehler bei der Anmeldung korrigieren", "everyday-life", "government-office", "handle-misunderstanding", "government-office", "formal"),
    ("health-insurance-card", "Eine neue Gesundheitskarte beantragen", "appointments-and-health", "healthcare", "ask-for-information", "phone", "formal"),
    ("therapy-waiting-list", "Nach einem Platz auf der Warteliste fragen", "appointments-and-health", "healthcare", "ask-for-information", "phone", "formal"),
    ("medicine-side-effect", "Nebenwirkungen in der Apotheke beschreiben", "appointments-and-health", "healthcare", "explain-problem", "service-counter", "formal"),
    ("parent-evening", "Beim Elternabend eine Frage stellen", "everyday-life", "school-kindergarten", "ask-for-information", "classroom", "formal"),
    ("school-trip", "Eine Klassenfahrt organisieren", "everyday-life", "school-kindergarten", "discuss-plan", "school-kindergarten", "neutral"),
    ("apartment-viewing", "Bei einer Wohnungsbesichtigung nachfragen", "housing", "housing", "ask-for-information", "face-to-face", "formal"),
    ("heating-problem", "Ein Heizungsproblem dringend melden", "housing", "housing", "explain-problem", "phone", "formal"),
    ("deposit-question", "Über die Kaution sprechen", "housing", "housing", "ask-for-information", "phone", "formal"),
    ("train-connection", "Eine bessere Verbindung suchen", "everyday-life", "transport-and-travel", "compare-options", "service-counter", "formal"),
    ("car-sharing", "Carsharing mit öffentlichen Verkehrsmitteln vergleichen", "everyday-life", "transport-and-travel", "compare-options", "pair-work", "neutral"),
    ("lost-luggage", "Verlorenes Gepäck melden", "everyday-life", "transport-and-travel", "explain-problem", "service-counter", "formal"),
    ("online-account", "Ein Problem mit einem Online-Konto lösen", "everyday-life", "digital-services", "ask-for-help", "phone", "formal"),
    ("password-reset", "Ein Passwort zurücksetzen lassen", "everyday-life", "digital-services", "ask-for-help", "phone", "formal"),
    ("course-level", "Das passende Kursniveau besprechen", "everyday-life", "integration-course", "compare-options", "classroom", "neutral"),
    ("exam-strategy", "Eine Strategie für die mündliche Prüfung planen", "everyday-life", "exam-preparation", "exam-discussion", "pair-work", "neutral"),
    ("picture-description", "Eine Bildbeschreibung gemeinsam üben", "everyday-life", "exam-preparation", "exam-roleplay", "exam-room", "formal"),
    ("friendship-plan", "Ein Treffen mit Freunden neu planen", "everyday-life", "social-life", "reschedule-appointment", "phone", "informal"),
    ("family-budget", "Ein Familienbudget besprechen", "everyday-life", "family", "discuss-plan", "face-to-face", "mixed"),
    ("neighbor-garden", "Regeln im Gemeinschaftsgarten klären", "housing", "housing", "agree-disagree", "face-to-face", "neutral"),
    ("volunteer-conflict", "Ein Missverständnis im Verein lösen", "everyday-life", "social-life", "handle-misunderstanding", "group-work", "neutral"),
    ("public-event", "Eine Veranstaltung in der Stadt planen", "everyday-life", "social-life", "discuss-plan", "group-work", "neutral"),
]


def make_dialogue(level: str, situation_index: int, variant: int, sort_order: int) -> dict:
    key, title, topic, category, task, mode, register = DIALOGUE_SITUATIONS[situation_index]
    dialogue = base.make_dialogue(level, situation_index + 100, sort_order)
    dialogue["slug"] = f"{level.lower()}-dialogue-expansion-002-{key}-{variant:02d}"
    dialogue["title"] = f"{title} ({level})"
    dialogue["description"] = f"Practical German dialogue for {title.lower()} with exam-oriented speaking practice at {level} level."
    dialogue["learnerGoal"] = "Practice explaining a situation, reacting politely, asking follow-up questions, and agreeing on next steps."
    dialogue["category"] = category
    dialogue["topics"] = [topic if topic in {"everyday-life", "housing", "shopping", "work-and-jobs", "appointments-and-health"} else "everyday-life"]
    dialogue["taskType"] = task
    dialogue["interactionMode"] = mode
    dialogue["register"] = register
    dialogue["sortOrder"] = sort_order
    dialogue["examRelevance"] = "Useful for B1/B2 oral exam roleplay, partner discussion, and practical workplace or administration conversations."

    skill_focus = ["speaking", "roleplay", "exam-speaking"]
    if mode == "phone":
        skill_focus.append("phone-call")
    if category in {"workplace", "education"} or mode in {"workplace", "video-call"}:
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
    sort_order = 40000
    for level in ("B1", "B2"):
        for variant in range(1, 3):
            for situation_index in range(len(DIALOGUE_SITUATIONS)):
                dialogues.append(make_dialogue(level, situation_index, variant, sort_order))
                sort_order += 10

    package = {
        "packageId": "dialogues-exam-expansion-002-b1-b2",
        "packageName": "Dialogue Exam Expansion 002 B1 B2",
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

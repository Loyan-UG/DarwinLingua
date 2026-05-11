import json
from pathlib import Path

import generate_dialogue_exam_content as base
from generate_dialogue_exam_expansion_020 import TASK_ALIASES, TASK_TO_FUNCTIONS


ROOT = Path(__file__).resolve().parents[2]
OUT_DIR = ROOT / "content" / "generated" / "dialogues"
OUT_FILE = OUT_DIR / "dialogues-exam-expansion-021-b1-b2.json"

DIALOGUE_SITUATIONS = [
    ("integration-course-homework", "Hausaufgaben im Integrationskurs nachfragen", "everyday-life", "integration-course", "ask-for-information", "classroom", "formal"),
    ("integration-course-childcare-delay", "Eine Verspätung wegen Kinderbetreuung erklären", "everyday-life", "integration-course", "explain-problem", "phone", "formal"),
    ("integration-course-learning-goal", "Ein persönliches Lernziel im Kurs besprechen", "everyday-life", "integration-course", "give-opinion", "classroom", "formal"),
    ("integration-course-practice-partner", "Einen Übungspartner für den Kurs finden", "everyday-life", "integration-course", "make-suggestion", "classroom", "neutral"),
    ("food-restaurant-reservation-change", "Eine Restaurantreservierung ändern", "everyday-life", "food-restaurant", "reschedule-appointment", "phone", "formal"),
    ("food-lunch-menu-work", "Ein Mittagessen für das Team auswählen", "work-and-jobs", "food-restaurant", "compare-options", "workplace", "neutral"),
    ("food-catering-budget", "Ein Catering-Budget verhandeln", "work-and-jobs", "food-restaurant", "negotiate-solution", "phone", "formal"),
    ("food-service-feedback", "Service im Restaurant höflich bewerten", "everyday-life", "food-restaurant", "give-opinion", "face-to-face", "formal"),
    ("education-library-help", "Hilfe bei der Recherche in der Bibliothek bekommen", "everyday-life", "education", "ask-for-help", "service-counter", "formal"),
    ("education-training-cost", "Kosten einer Weiterbildung vergleichen", "work-and-jobs", "education", "compare-options", "phone", "formal"),
    ("education-course-material", "Fehlendes Kursmaterial nachfragen", "everyday-life", "education", "ask-for-information", "classroom", "formal"),
    ("education-group-work", "Gruppenarbeit im Kurs organisieren", "everyday-life", "education", "discuss-plan", "classroom", "neutral"),
    ("work-break-rule", "Pausenregeln im Betrieb klären", "work-and-jobs", "work", "ask-for-information", "workplace", "formal"),
    ("work-tool-request", "Ein Arbeitsmittel begründet anfragen", "work-and-jobs", "work", "ask-for-help", "workplace", "formal"),
    ("work-shift-problem", "Ein Problem im Schichtplan erklären", "work-and-jobs", "work", "explain-problem", "phone", "formal"),
    ("work-team-suggestion", "Einen Verbesserungsvorschlag im Team machen", "work-and-jobs", "work", "make-suggestion", "workplace", "neutral"),
    ("family-care-appointment", "Einen Pflegetermin in der Familie abstimmen", "appointments-and-health", "family", "discuss-plan", "phone", "mixed"),
    ("family-finance-help", "Finanzielle Hilfe in der Familie besprechen", "shopping", "family", "negotiate-solution", "face-to-face", "mixed"),
    ("family-language-practice", "Deutschlernen in der Familie planen", "everyday-life", "family", "discuss-plan", "face-to-face", "informal"),
    ("family-holiday-choice", "Urlaubsoptionen in der Familie vergleichen", "everyday-life", "family", "compare-options", "face-to-face", "informal"),
    ("city-neighborhood-project", "Ein Nachbarschaftsprojekt der Stadt besprechen", "everyday-life", "city-services", "give-opinion", "group-work", "neutral"),
    ("city-appointment-online", "Einen Termin bei der Stadt online buchen", "everyday-life", "city-services", "ask-for-help", "phone", "formal"),
    ("city-event-permit", "Eine kleine Veranstaltung bei der Stadt anmelden", "everyday-life", "city-services", "ask-for-information", "government-office", "formal"),
    ("city-public-transport-info", "Informationen zum öffentlichen Verkehr erfragen", "everyday-life", "city-services", "ask-for-information", "service-counter", "formal"),
    ("social-friend-disagreement", "Eine Meinungsverschiedenheit mit Freunden klären", "everyday-life", "social-life", "agree-disagree", "face-to-face", "informal"),
    ("social-help-request", "Freunde um praktische Hilfe bitten", "everyday-life", "social-life", "ask-for-help", "phone", "informal"),
    ("exam-integration-homework", "Eine Kurssituation als Prüfungsrolle üben", "everyday-life", "exam-preparation", "exam-roleplay", "exam-room", "formal"),
    ("exam-food-budget", "Restaurant und Budget in der Prüfung vergleichen", "shopping", "exam-preparation", "compare-options", "exam-room", "formal"),
    ("exam-work-suggestion", "Einen Vorschlag am Arbeitsplatz in der Prüfung begründen", "work-and-jobs", "exam-preparation", "exam-discussion", "exam-room", "formal"),
    ("exam-family-support", "Über Unterstützung in der Familie sprechen", "everyday-life", "exam-preparation", "give-opinion", "exam-room", "formal"),
]


TASK_TO_FUNCTIONS = {
    **TASK_TO_FUNCTIONS,
    "make-suggestion": ["greet", "suggest", "explain", "justify", "agree", "summarize", "close-conversation"],
}


def make_dialogue(level: str, situation_index: int, variant: int, sort_order: int) -> dict:
    key, title, topic, category, task, mode, register = DIALOGUE_SITUATIONS[situation_index]
    dialogue = base.make_dialogue(level, situation_index + 2000, sort_order)
    dialogue["slug"] = f"{level.lower()}-dialogue-expansion-021-{key}-{variant:02d}"
    dialogue["title"] = f"{title} ({level})"
    dialogue["description"] = f"B1/B2 German dialogue for {title.lower()} with practical roleplay and exam speaking focus."
    dialogue["learnerGoal"] = "Practice explaining a practical situation, asking follow-up questions, giving reasons, and agreeing on a realistic result."
    dialogue["category"] = category
    dialogue["topics"] = [topic if topic in {"everyday-life", "housing", "shopping", "work-and-jobs", "appointments-and-health"} else "everyday-life"]
    dialogue["taskType"] = TASK_ALIASES.get(task, task)
    dialogue["interactionMode"] = mode
    dialogue["register"] = register
    dialogue["speakingFunctions"] = TASK_TO_FUNCTIONS[task]
    dialogue["sortOrder"] = sort_order
    dialogue["difficultyNote"] = f"{level} dialogue for a practical task: {title}."
    dialogue["examRelevance"] = "Useful for B1/B2 oral exam roleplay, everyday planning, service communication, and structured opinion practice."

    skill_focus = ["speaking", "roleplay", "exam-speaking"]
    if mode == "phone":
        skill_focus.append("phone-call")
    if category in {"work", "workplace", "job-interview"} or mode == "workplace":
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
    sort_order = 420000
    for level in ("B1", "B2"):
        for variant in range(1, 3):
            for situation_index in range(len(DIALOGUE_SITUATIONS)):
                dialogues.append(make_dialogue(level, situation_index, variant, sort_order))
                sort_order += 10

    package = {
        "packageId": "dialogues-exam-expansion-021-b1-b2",
        "packageName": "Dialogue Exam Expansion 021 B1 B2",
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

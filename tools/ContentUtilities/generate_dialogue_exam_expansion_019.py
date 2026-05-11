import json
from pathlib import Path

import generate_dialogue_exam_content as base
from generate_dialogue_exam_expansion_018 import TASK_ALIASES, TASK_TO_FUNCTIONS


ROOT = Path(__file__).resolve().parents[2]
OUT_DIR = ROOT / "content" / "generated" / "dialogues"
OUT_FILE = OUT_DIR / "dialogues-exam-expansion-019-b1-b2.json"

DIALOGUE_SITUATIONS = [
    ("food-catering-request", "Ein Catering für eine kleine Feier anfragen", "everyday-life", "food-restaurant", "ask-for-information", "phone", "formal"),
    ("food-vegetarian-options", "Vegetarische Optionen im Restaurant vergleichen", "everyday-life", "food-restaurant", "compare-options", "face-to-face", "neutral"),
    ("food-delivery-late", "Eine verspätete Essenslieferung reklamieren", "shopping", "food-restaurant", "complain-politely", "phone", "formal"),
    ("food-cafe-work-meeting", "Ein Treffen im Café organisieren", "work-and-jobs", "food-restaurant", "discuss-plan", "phone", "neutral"),
    ("city-market-stand", "Einen Stand auf dem Wochenmarkt beantragen", "work-and-jobs", "city-services", "ask-for-information", "government-office", "formal"),
    ("city-parking-permit", "Einen Bewohnerparkausweis klären", "everyday-life", "city-services", "ask-for-information", "government-office", "formal"),
    ("city-youth-program", "Ein Freizeitangebot der Stadt besprechen", "everyday-life", "city-services", "compare-options", "service-counter", "neutral"),
    ("city-complaint-cleanliness", "Sauberkeit im Stadtteil melden", "everyday-life", "city-services", "complain-politely", "phone", "formal"),
    ("work-probation-talk", "Ein Gespräch in der Probezeit führen", "work-and-jobs", "work", "give-opinion", "workplace", "formal"),
    ("work-task-priority", "Prioritäten bei mehreren Aufgaben klären", "work-and-jobs", "work", "clarify", "workplace", "neutral"),
    ("work-part-time-request", "Teilzeit im Betrieb anfragen", "work-and-jobs", "work", "negotiate-solution", "face-to-face", "formal"),
    ("work-safety-briefing", "Eine Sicherheitsunterweisung nachfragen", "work-and-jobs", "work", "ask-for-information", "workplace", "formal"),
    ("integration-course-practice-group", "Eine Lerngruppe im Integrationskurs organisieren", "everyday-life", "integration-course", "discuss-plan", "classroom", "neutral"),
    ("integration-course-child-sick", "Eine Kursabwesenheit wegen Kind erklären", "appointments-and-health", "integration-course", "explain-problem", "phone", "formal"),
    ("integration-course-module-change", "Einen Modulwechsel im Kurs besprechen", "everyday-life", "integration-course", "negotiate-solution", "service-counter", "formal"),
    ("integration-course-speaking-exam", "Die mündliche Prüfung im Kurs vorbereiten", "everyday-life", "integration-course", "ask-for-information", "classroom", "formal"),
    ("family-care-home", "Eine Betreuung zu Hause organisieren", "appointments-and-health", "family", "discuss-plan", "phone", "mixed"),
    ("family-school-meeting", "Ein Schulgespräch in der Familie vorbereiten", "everyday-life", "family", "discuss-plan", "face-to-face", "mixed"),
    ("family-shared-car", "Ein Auto in der Familie teilen", "everyday-life", "family", "negotiate-solution", "face-to-face", "informal"),
    ("family-invitation-boundaries", "Eine Einladung freundlich begrenzen", "everyday-life", "family", "refuse-politely", "phone", "informal"),
    ("shopping-phone-plan", "Einen Handyvertrag vergleichen", "shopping", "shopping-and-services", "compare-options", "service-counter", "formal"),
    ("shopping-repair-estimate", "Einen Kostenvoranschlag für eine Reparatur klären", "shopping", "shopping-and-services", "ask-for-information", "service-counter", "formal"),
    ("shopping-service-delay", "Eine verspätete Dienstleistung reklamieren", "shopping", "shopping-and-services", "complain-politely", "phone", "formal"),
    ("shopping-subscription-change", "Ein Abo auf einen anderen Tarif umstellen", "shopping", "shopping-and-services", "negotiate-solution", "phone", "formal"),
    ("social-new-neighbor", "Neue Nachbarn kennenlernen", "housing", "social-life", "introduce-yourself", "face-to-face", "informal"),
    ("social-language-cafe", "Im Sprachcafé ein Gespräch beginnen", "everyday-life", "social-life", "introduce-yourself", "face-to-face", "informal"),
    ("exam-food-complaint", "Eine Beschwerde im Restaurant als Prüfungsrolle üben", "shopping", "exam-preparation", "exam-roleplay", "exam-room", "formal"),
    ("exam-city-opinion", "Über Angebote der Stadt in der Prüfung sprechen", "everyday-life", "exam-preparation", "give-opinion", "exam-room", "formal"),
    ("exam-work-part-time", "Teilzeit und Vollzeit in der Prüfung vergleichen", "work-and-jobs", "exam-preparation", "exam-discussion", "exam-room", "formal"),
    ("exam-family-plan", "Eine Familienplanungssituation in der Prüfung lösen", "everyday-life", "exam-preparation", "exam-roleplay", "exam-room", "formal"),
]


TASK_TO_FUNCTIONS = {
    **TASK_TO_FUNCTIONS,
    "introduce-yourself": ["greet", "introduce", "ask-question", "answer-question", "close-conversation"],
    "refuse-politely": ["greet", "apologize", "explain", "request", "suggest", "close-conversation"],
}


def make_dialogue(level: str, situation_index: int, variant: int, sort_order: int) -> dict:
    key, title, topic, category, task, mode, register = DIALOGUE_SITUATIONS[situation_index]
    dialogue = base.make_dialogue(level, situation_index + 1800, sort_order)
    dialogue["slug"] = f"{level.lower()}-dialogue-expansion-019-{key}-{variant:02d}"
    dialogue["title"] = f"{title} ({level})"
    dialogue["description"] = f"B1/B2 German dialogue for {title.lower()} with practical roleplay and exam speaking focus."
    dialogue["learnerGoal"] = "Practice starting the conversation, explaining your position, comparing options, and agreeing on the next step."
    dialogue["category"] = category
    dialogue["topics"] = [topic if topic in {"everyday-life", "housing", "shopping", "work-and-jobs", "appointments-and-health"} else "everyday-life"]
    dialogue["taskType"] = TASK_ALIASES.get(task, task)
    dialogue["interactionMode"] = mode
    dialogue["register"] = register
    dialogue["speakingFunctions"] = TASK_TO_FUNCTIONS[task]
    dialogue["sortOrder"] = sort_order
    dialogue["difficultyNote"] = f"{level} dialogue for a practical task: {title}."
    dialogue["examRelevance"] = "Useful for B1/B2 oral exam roleplay, daily service situations, and structured comparison tasks."

    skill_focus = ["speaking", "roleplay", "exam-speaking"]
    if mode == "phone":
        skill_focus.append("phone-call")
    if category in {"work", "workplace", "job-interview"} or mode == "workplace":
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
    sort_order = 380000
    for level in ("B1", "B2"):
        for variant in range(1, 3):
            for situation_index in range(len(DIALOGUE_SITUATIONS)):
                dialogues.append(make_dialogue(level, situation_index, variant, sort_order))
                sort_order += 10

    package = {
        "packageId": "dialogues-exam-expansion-019-b1-b2",
        "packageName": "Dialogue Exam Expansion 019 B1 B2",
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

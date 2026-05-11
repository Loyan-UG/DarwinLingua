import json
from pathlib import Path

import generate_dialogue_exam_content as base
from generate_dialogue_exam_expansion_014 import TASK_ALIASES, TASK_TO_FUNCTIONS


ROOT = Path(__file__).resolve().parents[2]
OUT_DIR = ROOT / "content" / "generated" / "dialogues"
OUT_FILE = OUT_DIR / "dialogues-exam-expansion-015-b1-b2.json"

TASK_TO_FUNCTIONS = {
    **TASK_TO_FUNCTIONS,
    "make-suggestion": ["greet", "suggest", "explain", "justify", "agree", "summarize", "close-conversation"],
}

DIALOGUE_SITUATIONS = [
    ("city-library-card", "Einen Bibliotheksausweis beantragen", "everyday-life", "city-services", "ask-for-information", "service-counter", "formal"),
    ("city-lost-property", "Beim Fundbüro nachfragen", "everyday-life", "city-services", "explain-problem", "phone", "formal"),
    ("city-waste-calendar", "Den Abfallkalender der Stadt klären", "housing", "city-services", "ask-for-information", "phone", "formal"),
    ("city-sports-course", "Einen städtischen Sportkurs buchen", "everyday-life", "city-services", "ask-for-help", "service-counter", "formal"),
    ("restaurant-allergy", "Im Restaurant eine Allergie erklären", "everyday-life", "food-restaurant", "explain-problem", "face-to-face", "formal"),
    ("restaurant-group-booking", "Einen Tisch für eine Gruppe reservieren", "everyday-life", "food-restaurant", "make-appointment", "phone", "formal"),
    ("restaurant-wrong-bill", "Eine falsche Rechnung freundlich reklamieren", "shopping", "food-restaurant", "complain-politely", "face-to-face", "formal"),
    ("restaurant-menu-choice", "Gerichte mit Freunden vergleichen", "everyday-life", "food-restaurant", "compare-options", "face-to-face", "informal"),
    ("education-course-level", "Das passende Kursniveau besprechen", "everyday-life", "education", "compare-options", "face-to-face", "formal"),
    ("education-exam-registration", "Sich für eine Prüfung anmelden", "work-and-jobs", "education", "ask-for-information", "service-counter", "formal"),
    ("education-study-plan", "Einen Lernplan mit der Lehrkraft abstimmen", "everyday-life", "education", "discuss-plan", "classroom", "formal"),
    ("education-online-class", "Ein Problem im Onlinekurs erklären", "work-and-jobs", "education", "explain-problem", "video-call", "formal"),
    ("work-overtime-question", "Über Überstunden im Betrieb sprechen", "work-and-jobs", "work", "ask-for-information", "workplace", "formal"),
    ("work-sick-note", "Eine Krankmeldung am Arbeitsplatz klären", "work-and-jobs", "work", "explain-problem", "phone", "formal"),
    ("work-vacation-cover", "Eine Vertretung für Urlaub organisieren", "work-and-jobs", "work", "discuss-plan", "workplace", "neutral"),
    ("work-new-task", "Eine neue Aufgabe im Team klären", "work-and-jobs", "work", "clarify", "workplace", "neutral"),
    ("travel-luggage-delay", "Verspätetes Gepäck am Schalter melden", "everyday-life", "transport-and-travel", "explain-problem", "service-counter", "formal"),
    ("travel-ticket-refund", "Eine Fahrkarte erstatten lassen", "shopping", "transport-and-travel", "complain-politely", "service-counter", "formal"),
    ("travel-car-share", "Eine Mitfahrgelegenheit organisieren", "everyday-life", "transport-and-travel", "discuss-plan", "phone", "informal"),
    ("travel-route-advice", "Eine sichere Route vergleichen", "everyday-life", "transport-and-travel", "compare-options", "face-to-face", "neutral"),
    ("complaint-phone-provider", "Beim Telefonanbieter eine Störung melden", "shopping", "complaint", "complain-politely", "phone", "formal"),
    ("complaint-delivery-damage", "Eine beschädigte Lieferung reklamieren", "shopping", "complaint", "complain-politely", "phone", "formal"),
    ("conflict-shared-kitchen", "Einen Konflikt in der Gemeinschaftsküche lösen", "housing", "conflict-resolution", "negotiate-solution", "face-to-face", "mixed"),
    ("conflict-team-priority", "Unterschiedliche Prioritäten im Team klären", "work-and-jobs", "conflict-resolution", "negotiate-solution", "workplace", "formal"),
    ("exam-city-service", "Eine Service-Situation bei der Stadt üben", "everyday-life", "exam-preparation", "exam-roleplay", "exam-room", "formal"),
    ("exam-restaurant-complaint", "Eine Restaurantbeschwerde in der Prüfung üben", "shopping", "exam-preparation", "exam-roleplay", "exam-room", "formal"),
    ("exam-education-opinion", "Über Weiterbildung in der Prüfung diskutieren", "work-and-jobs", "exam-preparation", "exam-discussion", "exam-room", "formal"),
    ("exam-work-rights", "Über Rechte am Arbeitsplatz sprechen", "work-and-jobs", "exam-preparation", "give-opinion", "exam-room", "formal"),
    ("social-festival-help", "Bei einem Stadtfest mithelfen", "everyday-life", "social-life", "make-suggestion", "group-work", "neutral"),
    ("family-school-choice", "Eine Schulwahl in der Familie besprechen", "everyday-life", "family", "compare-options", "face-to-face", "mixed"),
]


def make_dialogue(level: str, situation_index: int, variant: int, sort_order: int) -> dict:
    key, title, topic, category, task, mode, register = DIALOGUE_SITUATIONS[situation_index]
    dialogue = base.make_dialogue(level, situation_index + 1400, sort_order)
    dialogue["slug"] = f"{level.lower()}-dialogue-expansion-015-{key}-{variant:02d}"
    dialogue["title"] = f"{title} ({level})"
    dialogue["description"] = f"B1/B2 German dialogue for {title.lower()} with practical roleplay and exam speaking focus."
    dialogue["learnerGoal"] = "Practice describing the issue clearly, asking useful follow-up questions, and reaching a practical agreement."
    dialogue["category"] = category
    dialogue["topics"] = [topic if topic in {"everyday-life", "housing", "shopping", "work-and-jobs", "appointments-and-health"} else "everyday-life"]
    dialogue["taskType"] = TASK_ALIASES.get(task, task)
    dialogue["interactionMode"] = mode
    dialogue["register"] = register
    dialogue["speakingFunctions"] = TASK_TO_FUNCTIONS[task]
    dialogue["sortOrder"] = sort_order
    dialogue["difficultyNote"] = f"{level} dialogue for a practical task: {title}."
    dialogue["examRelevance"] = "Useful for B1/B2 oral exam roleplay, service communication, and structured problem solving."

    skill_focus = ["speaking", "roleplay", "exam-speaking"]
    if mode == "phone":
        skill_focus.append("phone-call")
    if category in {"work", "workplace"} or mode in {"workplace", "video-call"}:
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
    sort_order = 300000
    for level in ("B1", "B2"):
        for variant in range(1, 3):
            for situation_index in range(len(DIALOGUE_SITUATIONS)):
                dialogues.append(make_dialogue(level, situation_index, variant, sort_order))
                sort_order += 10

    package = {
        "packageId": "dialogues-exam-expansion-015-b1-b2",
        "packageName": "Dialogue Exam Expansion 015 B1 B2",
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

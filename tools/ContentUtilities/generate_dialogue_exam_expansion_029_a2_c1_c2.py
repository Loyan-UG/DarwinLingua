import json
from pathlib import Path

import generate_dialogue_exam_content as base
from generate_dialogue_exam_expansion_025_a2_c1_c2 import TASK_ALIASES, TASK_TO_FUNCTIONS


ROOT = Path(__file__).resolve().parents[2]
OUT_DIR = ROOT / "content" / "generated" / "dialogues"
OUT_FILE = OUT_DIR / "dialogues-exam-expansion-029-a2-c1-c2.json"

A2_SITUATIONS = [
    ("supermarket-product", "Im Supermarkt nach einem Produkt fragen", "shopping", "shopping-and-services", "ask-for-information", "service-counter", "neutral"),
    ("doctor-prescription", "Beim Arzt ein Rezept klären", "appointments-and-health", "healthcare", "ask-for-information", "doctor-office", "formal"),
    ("bus-stop-help", "An der Bushaltestelle um Hilfe bitten", "everyday-life", "transport-and-travel", "ask-for-help", "face-to-face", "neutral"),
    ("course-test-date", "Einen Testtermin im Kurs besprechen", "everyday-life", "integration-course", "ask-for-information", "classroom", "neutral"),
    ("apartment-viewing", "Einen Besichtigungstermin vereinbaren", "housing", "housing", "make-appointment", "phone", "formal"),
    ("school-materials", "Materialien für die Schule klären", "everyday-life", "school-kindergarten", "ask-for-information", "phone", "formal"),
    ("internet-problem-simple", "Ein einfaches Internetproblem melden", "shopping", "digital-services", "explain-problem", "phone", "formal"),
    ("restaurant-reservation-change", "Eine Reservierung im Restaurant ändern", "shopping", "food-restaurant", "reschedule-appointment", "phone", "formal"),
    ("bank-address-change", "Eine neue Adresse bei der Bank melden", "shopping", "finance-insurance", "explain-problem", "service-counter", "formal"),
    ("friend-birthday-plan", "Einen Geburtstag mit Freunden planen", "everyday-life", "social-life", "discuss-plan", "phone", "informal"),
]

C1_SITUATIONS = [
    ("work-prioritization-conflict", "Prioritätenkonflikte im Team strukturiert lösen", "work-and-jobs", "workplace", "negotiate-solution", "workplace", "formal"),
    ("digital-inclusion-policy", "Digitale Teilhabe und Verantwortung diskutieren", "everyday-life", "digital-services", "exam-discussion", "group-work", "formal"),
    ("health-second-opinion", "Eine Zweitmeinung medizinisch und kommunikativ einordnen", "appointments-and-health", "healthcare", "compare-options", "doctor-office", "formal"),
    ("school-language-support", "Sprachförderung in der Schule differenziert planen", "everyday-life", "school-kindergarten", "discuss-plan", "face-to-face", "formal"),
    ("housing-noise-mediation", "Eine Lärmbeschwerde zwischen Mietparteien vermitteln", "housing", "housing", "negotiate-solution", "face-to-face", "formal"),
    ("government-appeal", "Einen behördlichen Bescheid sachlich hinterfragen", "everyday-life", "government-office", "complain-politely", "government-office", "formal"),
    ("work-feedback-culture", "Feedbackkultur und Leistungsdruck abwägen", "work-and-jobs", "workplace", "give-opinion", "workplace", "formal"),
    ("job-interview-values", "Eigene Werte im Bewerbungsgespräch erklären", "work-and-jobs", "job-interview", "job-interview", "face-to-face", "formal"),
    ("insurance-prevention", "Prävention und Versicherungskosten diskutieren", "shopping", "finance-insurance", "compare-options", "phone", "formal"),
    ("media-algorithm-bias", "Algorithmische Verzerrung in Medien erklären", "everyday-life", "digital-services", "give-opinion", "group-work", "formal"),
    ("education-exam-pressure", "Prüfungsdruck und faire Bewertung diskutieren", "work-and-jobs", "education", "exam-discussion", "classroom", "formal"),
    ("integration-participation", "Teilhabe im Stadtteil praktisch fördern", "everyday-life", "integration-course", "discuss-plan", "group-work", "formal"),
    ("complaint-long-term-customer", "Eine Beschwerde als Stammkunde sachlich eskalieren", "shopping", "complaint", "complain-politely", "phone", "formal"),
    ("project-scope-change", "Eine Projektänderung mit Folgen verhandeln", "work-and-jobs", "workplace", "negotiate-solution", "video-call", "formal"),
    ("work-care-responsibility", "Pflegeverantwortung und Arbeit organisieren", "work-and-jobs", "healthcare", "discuss-plan", "workplace", "formal"),
]

C2_SITUATIONS = [
    ("ai-democratic-control", "Demokratische Kontrolle von KI-Systemen debattieren", "work-and-jobs", "digital-services", "exam-discussion", "group-work", "formal"),
    ("health-data-solidarity", "Gesundheitsdaten zwischen Solidarität und Privatsphäre abwägen", "appointments-and-health", "healthcare", "compare-options", "group-work", "formal"),
    ("education-elite-access", "Elitenbildung und Zugangsgerechtigkeit analysieren", "work-and-jobs", "education", "exam-discussion", "classroom", "formal"),
    ("housing-heritage-future", "Stadterbe und zukünftiges Wohnen nuanciert diskutieren", "housing", "housing", "give-opinion", "group-work", "formal"),
    ("administration-discretion", "Ermessensspielräume in der Verwaltung kritisch betrachten", "everyday-life", "government-office", "exam-discussion", "group-work", "formal"),
    ("work-human-dignity", "Menschenwürde und Effizienz in der Arbeitswelt abwägen", "work-and-jobs", "workplace", "compare-options", "group-work", "formal"),
]


def situations_for_level(level: str):
    if level == "A2":
        return A2_SITUATIONS
    if level == "C1":
        return C1_SITUATIONS
    return C2_SITUATIONS


def make_dialogue(level: str, situation_index: int, variant: int, sort_order: int) -> dict:
    key, title, topic, category, task, mode, register = situations_for_level(level)[situation_index]
    dialogue = base.make_dialogue(level, situation_index + 2900, sort_order)
    dialogue["slug"] = f"{level.lower()}-dialogue-expansion-029-{key}-{variant:02d}"
    dialogue["title"] = f"{title} ({level})"
    dialogue["description"] = f"{level} German dialogue for {title.lower()} with practical and exam-oriented speaking practice."
    dialogue["learnerGoal"] = "Practice level-appropriate speaking with reasons, clarification, comparison, and a final summary."
    dialogue["category"] = category
    dialogue["topics"] = [topic if topic in {"everyday-life", "housing", "shopping", "work-and-jobs", "appointments-and-health"} else "everyday-life"]
    dialogue["taskType"] = TASK_ALIASES.get(task, task)
    dialogue["interactionMode"] = mode
    dialogue["register"] = register
    dialogue["speakingFunctions"] = TASK_TO_FUNCTIONS[task]
    dialogue["sortOrder"] = sort_order
    dialogue["difficultyNote"] = f"{level} dialogue for: {title}."
    dialogue["examRelevance"] = "Useful for oral exam roleplay, structured discussion, and practical German conversation."

    skill_focus = ["speaking", "roleplay", "exam-speaking"]
    if mode == "phone":
        skill_focus.append("phone-call")
    if category in {"work", "workplace", "job-interview"} or mode in {"workplace", "video-call"}:
        skill_focus.append("workplace-communication")
    if task == "negotiate-solution":
        skill_focus.append("negotiation")
    if task in {"give-opinion", "exam-discussion", "agree-disagree", "compare-options"}:
        skill_focus.append("discussion")
    if task == "complain-politely":
        skill_focus.append("complaint-handling")
    dialogue["skillFocus"] = skill_focus
    return dialogue


def main() -> None:
    dialogues = []
    sort_order = 620000
    level_variants = {"A2": 6, "C1": 8, "C2": 6}
    for level, variants in level_variants.items():
        situations = situations_for_level(level)
        for variant in range(1, variants + 1):
            for situation_index in range(len(situations)):
                dialogues.append(make_dialogue(level, situation_index, variant, sort_order))
                sort_order += 10

    package = {
        "packageId": "dialogues-exam-expansion-029-a2-c1-c2",
        "packageName": "Dialogue Exam Expansion 029 A2 C1 C2",
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

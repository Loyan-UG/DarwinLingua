import json
from pathlib import Path

import generate_dialogue_exam_content as base
from generate_dialogue_exam_expansion_025_a2_c1_c2 import TASK_ALIASES, TASK_TO_FUNCTIONS


ROOT = Path(__file__).resolve().parents[2]
OUT_DIR = ROOT / "content" / "generated" / "dialogues"
OUT_FILE = OUT_DIR / "dialogues-exam-expansion-030-a2-c1-c2.json"

A2_SITUATIONS = [
    ("post-office-package", "Bei der Post nach einem Paket fragen", "everyday-life", "shopping-and-services", "ask-for-information", "service-counter", "formal"),
    ("clinic-opening-hours", "Öffnungszeiten einer Praxis erfragen", "appointments-and-health", "healthcare", "ask-for-information", "phone", "formal"),
    ("taxi-address", "Dem Taxi eine Adresse erklären", "everyday-life", "transport-and-travel", "explain-problem", "face-to-face", "neutral"),
    ("course-partner-task", "Eine Partneraufgabe im Kurs klären", "everyday-life", "integration-course", "ask-for-information", "classroom", "neutral"),
    ("heating-problem", "Ein Heizungsproblem einfach melden", "housing", "housing", "explain-problem", "phone", "formal"),
    ("school-pickup-time", "Eine Abholzeit in der Schule besprechen", "everyday-life", "school-kindergarten", "discuss-plan", "phone", "formal"),
    ("online-order-status", "Nach einer Online-Bestellung fragen", "shopping", "digital-services", "ask-for-information", "phone", "formal"),
    ("bakery-order", "In der Bäckerei etwas bestellen", "shopping", "food-restaurant", "ask-for-information", "face-to-face", "neutral"),
    ("bank-statement", "Einen Kontoauszug bei der Bank erfragen", "shopping", "finance-insurance", "ask-for-information", "service-counter", "formal"),
    ("friend-cinema-plan", "Einen Kinobesuch mit Freunden planen", "everyday-life", "social-life", "discuss-plan", "phone", "informal"),
]

C1_SITUATIONS = [
    ("work-decision-transparency", "Entscheidungstransparenz im Team einfordern", "work-and-jobs", "workplace", "give-opinion", "workplace", "formal"),
    ("digital-identity-security", "Digitale Identität und Sicherheit abwägen", "everyday-life", "digital-services", "compare-options", "group-work", "formal"),
    ("health-care-quality", "Qualität in der Pflege differenziert diskutieren", "appointments-and-health", "healthcare", "exam-discussion", "group-work", "formal"),
    ("school-digital-communication", "Digitale Kommunikation mit der Schule verbessern", "everyday-life", "school-kindergarten", "discuss-plan", "video-call", "formal"),
    ("housing-shared-costs", "Nebenkosten in einer Hausgemeinschaft verhandeln", "housing", "housing", "negotiate-solution", "face-to-face", "formal"),
    ("government-document-proof", "Nachweise bei einer Behörde präzise klären", "everyday-life", "government-office", "ask-for-information", "government-office", "formal"),
    ("work-mentoring-plan", "Ein Mentoring-Konzept im Team planen", "work-and-jobs", "workplace", "discuss-plan", "workplace", "formal"),
    ("job-interview-conflict", "Konflikterfahrung im Bewerbungsgespräch darstellen", "work-and-jobs", "job-interview", "job-interview", "face-to-face", "formal"),
    ("insurance-deductible", "Selbstbeteiligung und Risiko vergleichen", "shopping", "finance-insurance", "compare-options", "phone", "formal"),
    ("media-public-image", "Öffentliche Selbstdarstellung in Medien reflektieren", "everyday-life", "digital-services", "give-opinion", "group-work", "formal"),
    ("education-lifelong-learning", "Lebenslanges Lernen argumentativ bewerten", "work-and-jobs", "education", "exam-discussion", "classroom", "formal"),
    ("integration-neighborhood-project", "Ein Nachbarschaftsprojekt für Integration planen", "everyday-life", "integration-course", "discuss-plan", "group-work", "formal"),
    ("complaint-service-contract", "Einen Servicevertrag nach Problemen neu verhandeln", "shopping", "complaint", "negotiate-solution", "phone", "formal"),
    ("project-resource-conflict", "Ressourcenkonflikte im Projekt priorisieren", "work-and-jobs", "workplace", "negotiate-solution", "video-call", "formal"),
    ("work-mental-load", "Mental Load und Aufgabenverteilung besprechen", "work-and-jobs", "healthcare", "agree-disagree", "workplace", "formal"),
]

C2_SITUATIONS = [
    ("ai-human-judgment", "Menschliches Urteil und KI-Empfehlungen gegeneinander abwägen", "work-and-jobs", "digital-services", "compare-options", "group-work", "formal"),
    ("health-prioritization-democracy", "Demokratische Legitimation medizinischer Priorisierung diskutieren", "appointments-and-health", "healthcare", "exam-discussion", "group-work", "formal"),
    ("education-language-power", "Sprache, Macht und Bildungschancen analysieren", "work-and-jobs", "education", "give-opinion", "classroom", "formal"),
    ("housing-public-good", "Wohnen als öffentliches Gut kontrovers debattieren", "housing", "housing", "exam-discussion", "group-work", "formal"),
    ("administration-automation-discretion", "Automatisierung und Ermessensspielräume in Behörden abwägen", "everyday-life", "government-office", "compare-options", "group-work", "formal"),
    ("work-authenticity-performance", "Authentizität und Leistungsdruck in Organisationen diskutieren", "work-and-jobs", "workplace", "exam-discussion", "group-work", "formal"),
]


def situations_for_level(level: str):
    if level == "A2":
        return A2_SITUATIONS
    if level == "C1":
        return C1_SITUATIONS
    return C2_SITUATIONS


def make_dialogue(level: str, situation_index: int, variant: int, sort_order: int) -> dict:
    key, title, topic, category, task, mode, register = situations_for_level(level)[situation_index]
    dialogue = base.make_dialogue(level, situation_index + 3000, sort_order)
    dialogue["slug"] = f"{level.lower()}-dialogue-expansion-030-{key}-{variant:02d}"
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
    sort_order = 650000
    level_variants = {"A2": 6, "C1": 8, "C2": 6}
    for level, variants in level_variants.items():
        situations = situations_for_level(level)
        for variant in range(1, variants + 1):
            for situation_index in range(len(situations)):
                dialogues.append(make_dialogue(level, situation_index, variant, sort_order))
                sort_order += 10

    package = {
        "packageId": "dialogues-exam-expansion-030-a2-c1-c2",
        "packageName": "Dialogue Exam Expansion 030 A2 C1 C2",
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

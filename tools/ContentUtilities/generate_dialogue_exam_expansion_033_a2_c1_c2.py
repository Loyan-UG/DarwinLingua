import json
from pathlib import Path

import generate_dialogue_exam_content as base
from generate_dialogue_exam_expansion_025_a2_c1_c2 import TASK_ALIASES, TASK_TO_FUNCTIONS


ROOT = Path(__file__).resolve().parents[2]
OUT_DIR = ROOT / "content" / "generated" / "dialogues"
OUT_FILE = OUT_DIR / "dialogues-exam-expansion-033-a2-c1-c2.json"

A2_SITUATIONS = [
    ("city-document-copy", "Eine Kopie eines Dokuments bei der Stadt abgeben", "everyday-life", "government-office", "ask-for-information", "service-counter", "formal"),
    ("doctor-test-result", "Nach einem Testergebnis in der Praxis fragen", "appointments-and-health", "healthcare", "ask-for-information", "phone", "formal"),
    ("train-platform-change", "Eine Gleisänderung am Bahnhof verstehen", "everyday-life", "transport-and-travel", "ask-for-help", "face-to-face", "neutral"),
    ("course-homework-delay", "Verspätete Hausaufgaben im Kurs erklären", "everyday-life", "integration-course", "explain-problem", "classroom", "neutral"),
    ("apartment-heating", "Eine kalte Heizung in der Wohnung melden", "housing", "housing", "complain-politely", "phone", "formal"),
    ("school-sick-note", "Eine Krankmeldung für die Schule klären", "everyday-life", "school-kindergarten", "explain-problem", "phone", "formal"),
    ("online-order-address", "Eine Lieferadresse online korrigieren", "shopping", "digital-services", "ask-for-help", "phone", "formal"),
    ("restaurant-bill-question", "Eine Frage zur Rechnung im Restaurant stellen", "shopping", "food-restaurant", "ask-for-information", "face-to-face", "formal"),
    ("insurance-letter-help", "Hilfe mit einem Brief der Versicherung bekommen", "shopping", "finance-insurance", "ask-for-help", "phone", "formal"),
    ("friend-weekend-visit", "Einen Wochenendbesuch mit Freunden planen", "everyday-life", "social-life", "discuss-plan", "phone", "informal"),
]

C1_SITUATIONS = [
    ("work-feedback-culture", "Feedbackkultur und psychologische Sicherheit im Team besprechen", "work-and-jobs", "workplace", "give-opinion", "workplace", "formal"),
    ("digital-accessibility", "Barrierefreiheit digitaler Angebote differenziert bewerten", "everyday-life", "digital-services", "compare-options", "group-work", "formal"),
    ("health-prevention-budget", "Präventionsbudgets im Gesundheitssystem priorisieren", "appointments-and-health", "healthcare", "compare-options", "group-work", "formal"),
    ("school-digital-devices", "Digitale Geräte im Unterricht ausgewogen diskutieren", "everyday-life", "school-kindergarten", "exam-discussion", "classroom", "formal"),
    ("housing-neighbor-conflict", "Einen Nachbarschaftskonflikt sachlich vermitteln", "housing", "housing", "negotiate-solution", "face-to-face", "formal"),
    ("government-appeal-letter", "Einen Widerspruch bei einer Behörde begründen", "everyday-life", "government-office", "explain-problem", "government-office", "formal"),
    ("work-onboarding-remote", "Remote-Onboarding für neue Mitarbeitende verbessern", "work-and-jobs", "workplace", "discuss-plan", "video-call", "formal"),
    ("job-interview-career-gap", "Eine Lücke im Lebenslauf überzeugend erklären", "work-and-jobs", "job-interview", "job-interview", "face-to-face", "formal"),
    ("insurance-deductible-choice", "Selbstbeteiligung und Risiko bei Versicherungen abwägen", "shopping", "finance-insurance", "compare-options", "phone", "formal"),
    ("media-algorithm-bias", "Algorithmische Verzerrungen in Medien kritisch diskutieren", "everyday-life", "digital-services", "exam-discussion", "group-work", "formal"),
    ("education-microcredentials", "Micro-Credentials in der Weiterbildung bewerten", "work-and-jobs", "education", "give-opinion", "classroom", "formal"),
    ("integration-volunteer-training", "Schulungen für freiwillige Integrationsbegleitung planen", "everyday-life", "integration-course", "discuss-plan", "group-work", "formal"),
    ("complaint-refund-policy", "Eine Rückerstattungsregel kulant verhandeln", "shopping", "complaint", "negotiate-solution", "phone", "formal"),
    ("project-risk-communication", "Risiken in einem Projekt transparent kommunizieren", "work-and-jobs", "workplace", "handle-misunderstanding", "video-call", "formal"),
    ("work-health-boundaries", "Gesundheitliche Grenzen im Arbeitsalltag respektvoll verhandeln", "work-and-jobs", "healthcare", "agree-disagree", "workplace", "formal"),
]

C2_SITUATIONS = [
    ("ai-human-judgment", "Menschliches Urteilsvermögen in automatisierten Systemen verteidigen", "work-and-jobs", "digital-services", "exam-discussion", "group-work", "formal"),
    ("health-solidarity-limits", "Grenzen solidarischer Verantwortung im Gesundheitswesen ausloten", "appointments-and-health", "healthcare", "compare-options", "group-work", "formal"),
    ("education-credential-inflation", "Titelinflation und Bildungswert kritisch analysieren", "work-and-jobs", "education", "give-opinion", "classroom", "formal"),
    ("housing-heritage-density", "Denkmalschutz und Verdichtung in Städten kontrovers abwägen", "housing", "housing", "exam-discussion", "group-work", "formal"),
    ("administration-discretion", "Ermessensspielräume in automatisierter Verwaltung nuanciert bewerten", "everyday-life", "government-office", "compare-options", "group-work", "formal"),
    ("work-purpose-performance", "Sinnorientierung und Leistungsdruck in Organisationen diskutieren", "work-and-jobs", "workplace", "exam-discussion", "group-work", "formal"),
]


def situations_for_level(level: str):
    if level == "A2":
        return A2_SITUATIONS
    if level == "C1":
        return C1_SITUATIONS
    return C2_SITUATIONS


def make_dialogue(level: str, situation_index: int, variant: int, sort_order: int) -> dict:
    key, title, topic, category, task, mode, register = situations_for_level(level)[situation_index]
    dialogue = base.make_dialogue(level, situation_index + 3300, sort_order)
    dialogue["slug"] = f"{level.lower()}-dialogue-expansion-033-{key}-{variant:02d}"
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
    sort_order = 740000
    level_variants = {"A2": 6, "C1": 8, "C2": 6}
    for level, variants in level_variants.items():
        situations = situations_for_level(level)
        for variant in range(1, variants + 1):
            for situation_index in range(len(situations)):
                dialogues.append(make_dialogue(level, situation_index, variant, sort_order))
                sort_order += 10

    package = {
        "packageId": "dialogues-exam-expansion-033-a2-c1-c2",
        "packageName": "Dialogue Exam Expansion 033 A2 C1 C2",
        "packageVersion": "1.0",
        "generatedAtUtc": "2026-05-11T00:00:00Z",
        "entries": [],
        "dialogues": dialogues,
    }

    OUT_DIR.mkdir(parents=True, exist_ok=True)
    OUT_FILE.write_text(json.dumps(package, ensure_ascii=False, indent=2), encoding="utf-8")
    print(f"Wrote {len(dialogues)} dialogues to {OUT_FILE}")


if __name__ == "__main__":
    main()

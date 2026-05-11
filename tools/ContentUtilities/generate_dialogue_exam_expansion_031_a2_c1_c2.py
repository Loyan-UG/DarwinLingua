import json
from pathlib import Path

import generate_dialogue_exam_content as base
from generate_dialogue_exam_expansion_025_a2_c1_c2 import TASK_ALIASES, TASK_TO_FUNCTIONS


ROOT = Path(__file__).resolve().parents[2]
OUT_DIR = ROOT / "content" / "generated" / "dialogues"
OUT_FILE = OUT_DIR / "dialogues-exam-expansion-031-a2-c1-c2.json"

A2_SITUATIONS = [
    ("city-registration-hours", "Öffnungszeiten für die Anmeldung erfragen", "everyday-life", "government-office", "ask-for-information", "phone", "formal"),
    ("doctor-follow-up", "Einen Kontrolltermin beim Arzt vereinbaren", "appointments-and-health", "healthcare", "make-appointment", "phone", "formal"),
    ("lost-item-train", "Einen verlorenen Gegenstand im Zug melden", "everyday-life", "transport-and-travel", "explain-problem", "service-counter", "formal"),
    ("course-group-work", "Gruppenarbeit im Sprachkurs organisieren", "everyday-life", "integration-course", "discuss-plan", "classroom", "neutral"),
    ("apartment-key", "Ein Problem mit dem Wohnungsschlüssel erklären", "housing", "housing", "explain-problem", "phone", "formal"),
    ("school-meeting", "Ein kurzes Gespräch mit der Lehrerin vereinbaren", "everyday-life", "school-kindergarten", "make-appointment", "phone", "formal"),
    ("online-password", "Hilfe mit einem Online-Passwort bekommen", "shopping", "digital-services", "ask-for-help", "phone", "formal"),
    ("restaurant-allergy", "Im Restaurant eine Allergie erklären", "appointments-and-health", "food-restaurant", "explain-problem", "face-to-face", "formal"),
    ("bank-cash-withdrawal", "Bei der Bank nach Bargeld fragen", "shopping", "finance-insurance", "ask-for-information", "service-counter", "formal"),
    ("friend-sport-plan", "Sport mit einem Freund planen", "everyday-life", "social-life", "discuss-plan", "phone", "informal"),
]

C1_SITUATIONS = [
    ("work-responsibility-boundaries", "Verantwortlichkeiten und Grenzen im Team klären", "work-and-jobs", "workplace", "negotiate-solution", "workplace", "formal"),
    ("digital-public-services", "Digitale öffentliche Dienste nutzerorientiert bewerten", "everyday-life", "digital-services", "compare-options", "group-work", "formal"),
    ("health-shared-decision", "Gemeinsame Entscheidungsfindung beim Arzt diskutieren", "appointments-and-health", "healthcare", "give-opinion", "doctor-office", "formal"),
    ("school-inclusion-plan", "Inklusion in Schule und Betreuung praktisch planen", "everyday-life", "school-kindergarten", "discuss-plan", "face-to-face", "formal"),
    ("housing-modernization-notice", "Eine Modernisierungsankündigung sachlich besprechen", "housing", "housing", "ask-for-information", "face-to-face", "formal"),
    ("government-appointment-escalation", "Einen schwierigen Behördentermin deeskalieren", "everyday-life", "government-office", "handle-misunderstanding", "government-office", "formal"),
    ("work-knowledge-transfer", "Wissenstransfer nach einem Stellenwechsel sichern", "work-and-jobs", "workplace", "discuss-plan", "workplace", "formal"),
    ("job-interview-failure", "Aus Fehlern im Bewerbungsgespräch reflektiert lernen", "work-and-jobs", "job-interview", "job-interview", "face-to-face", "formal"),
    ("insurance-exception", "Eine Ausnahme bei der Versicherung begründen", "shopping", "finance-insurance", "explain-problem", "phone", "formal"),
    ("media-polarization", "Polarisierung in digitalen Medien diskutieren", "everyday-life", "digital-services", "exam-discussion", "group-work", "formal"),
    ("education-access-barriers", "Zugangshürden in Weiterbildung analysieren", "work-and-jobs", "education", "give-opinion", "classroom", "formal"),
    ("integration-civic-engagement", "Bürgerschaftliches Engagement und Integration verbinden", "everyday-life", "integration-course", "exam-discussion", "group-work", "formal"),
    ("complaint-expectation-management", "Erwartungen nach einer Beschwerde professionell steuern", "shopping", "complaint", "negotiate-solution", "phone", "formal"),
    ("project-quality-tradeoff", "Qualität, Tempo und Kosten im Projekt abwägen", "work-and-jobs", "workplace", "compare-options", "video-call", "formal"),
    ("work-burnout-prevention", "Burnout-Prävention im Team konstruktiv ansprechen", "work-and-jobs", "healthcare", "discuss-plan", "workplace", "formal"),
]

C2_SITUATIONS = [
    ("ai-explainability-trust", "Erklärbarkeit von KI und gesellschaftliches Vertrauen debattieren", "work-and-jobs", "digital-services", "exam-discussion", "group-work", "formal"),
    ("health-individual-risk", "Individuelles Risiko und kollektive Verantwortung abwägen", "appointments-and-health", "healthcare", "compare-options", "group-work", "formal"),
    ("education-merit-myth", "Meritokratie als Bildungsmythos kritisch analysieren", "work-and-jobs", "education", "give-opinion", "classroom", "formal"),
    ("housing-speculation-policy", "Spekulation, Regulierung und Wohnraumversorgung diskutieren", "housing", "housing", "exam-discussion", "group-work", "formal"),
    ("administration-citizen-autonomy", "Bürgerautonomie in standardisierten Verwaltungsprozessen abwägen", "everyday-life", "government-office", "compare-options", "group-work", "formal"),
    ("work-loyalty-flexibility", "Loyalität und Flexibilität in modernen Organisationen diskutieren", "work-and-jobs", "workplace", "exam-discussion", "group-work", "formal"),
]


def situations_for_level(level: str):
    if level == "A2":
        return A2_SITUATIONS
    if level == "C1":
        return C1_SITUATIONS
    return C2_SITUATIONS


def make_dialogue(level: str, situation_index: int, variant: int, sort_order: int) -> dict:
    key, title, topic, category, task, mode, register = situations_for_level(level)[situation_index]
    dialogue = base.make_dialogue(level, situation_index + 3100, sort_order)
    dialogue["slug"] = f"{level.lower()}-dialogue-expansion-031-{key}-{variant:02d}"
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
    sort_order = 680000
    level_variants = {"A2": 6, "C1": 8, "C2": 6}
    for level, variants in level_variants.items():
        situations = situations_for_level(level)
        for variant in range(1, variants + 1):
            for situation_index in range(len(situations)):
                dialogues.append(make_dialogue(level, situation_index, variant, sort_order))
                sort_order += 10

    package = {
        "packageId": "dialogues-exam-expansion-031-a2-c1-c2",
        "packageName": "Dialogue Exam Expansion 031 A2 C1 C2",
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

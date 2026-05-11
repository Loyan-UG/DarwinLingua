import json
from pathlib import Path

import generate_dialogue_exam_content as base
from generate_dialogue_exam_expansion_025_a2_c1_c2 import TASK_ALIASES, TASK_TO_FUNCTIONS


ROOT = Path(__file__).resolve().parents[2]
OUT_DIR = ROOT / "content" / "generated" / "dialogues"
OUT_FILE = OUT_DIR / "dialogues-exam-expansion-028-a2-c1-c2.json"

A2_SITUATIONS = [
    ("laundromat-help", "Im Waschsalon um Hilfe bitten", "everyday-life", "daily-life", "ask-for-help", "face-to-face", "neutral"),
    ("dentist-appointment", "Einen Zahnarzttermin machen", "appointments-and-health", "healthcare", "make-appointment", "phone", "formal"),
    ("kindergarten-info", "Informationen im Kindergarten erfragen", "everyday-life", "school-kindergarten", "ask-for-information", "phone", "formal"),
    ("train-delay-simple", "Eine Zugverspätung einfach erklären", "everyday-life", "transport-and-travel", "explain-problem", "service-counter", "neutral"),
    ("phone-contract-question", "Eine Frage zum Handyvertrag stellen", "shopping", "digital-services", "ask-for-information", "phone", "formal"),
    ("course-room-change", "Einen Raumwechsel im Kurs klären", "everyday-life", "integration-course", "ask-for-information", "classroom", "neutral"),
    ("rent-payment-question", "Eine Frage zur Mietzahlung stellen", "housing", "housing", "ask-for-information", "phone", "formal"),
    ("small-complaint-cafe", "Im Café freundlich reklamieren", "shopping", "food-restaurant", "complain-politely", "face-to-face", "formal"),
    ("bank-transfer-help", "Hilfe bei einer Überweisung bekommen", "shopping", "finance-insurance", "ask-for-help", "service-counter", "formal"),
    ("friend-meeting-place", "Einen Treffpunkt mit einem Freund klären", "everyday-life", "social-life", "discuss-plan", "phone", "informal"),
]

C1_SITUATIONS = [
    ("work-remote-policy", "Eine Remote-Work-Regelung differenziert verhandeln", "work-and-jobs", "workplace", "negotiate-solution", "video-call", "formal"),
    ("digital-service-access", "Barrierefreien Zugang zu digitalen Diensten bewerten", "everyday-life", "digital-services", "compare-options", "group-work", "formal"),
    ("health-communication-error", "Einen Kommunikationsfehler im Gesundheitswesen aufarbeiten", "appointments-and-health", "healthcare", "handle-misunderstanding", "doctor-office", "formal"),
    ("school-parent-conflict", "Einen Konflikt zwischen Eltern und Schule moderieren", "everyday-life", "school-kindergarten", "negotiate-solution", "face-to-face", "formal"),
    ("housing-maintenance-priority", "Instandhaltung und Kostenprioritäten begründen", "housing", "housing", "compare-options", "face-to-face", "formal"),
    ("government-service-delay", "Eine Verzögerung bei einer Behörde sachlich klären", "everyday-life", "government-office", "explain-problem", "government-office", "formal"),
    ("work-onboarding-quality", "Onboarding-Qualität im Team verbessern", "work-and-jobs", "workplace", "discuss-plan", "workplace", "formal"),
    ("interview-career-gap", "Eine berufliche Lücke im Interview erklären", "work-and-jobs", "job-interview", "job-interview", "face-to-face", "formal"),
    ("insurance-risk-assessment", "Risikoabwägung bei einer Versicherung diskutieren", "shopping", "finance-insurance", "compare-options", "phone", "formal"),
    ("media-debate-tone", "Den Ton einer öffentlichen Debatte analysieren", "everyday-life", "digital-services", "give-opinion", "group-work", "formal"),
    ("education-ai-tools", "KI-Werkzeuge im Unterricht kritisch diskutieren", "work-and-jobs", "education", "exam-discussion", "classroom", "formal"),
    ("integration-language-support", "Sprachförderung und Eigenverantwortung abwägen", "everyday-life", "integration-course", "exam-discussion", "group-work", "formal"),
    ("complaint-compensation", "Eine angemessene Entschädigung verhandeln", "shopping", "complaint", "negotiate-solution", "phone", "formal"),
    ("project-deadline-risk", "Projektrisiken und Fristen im Meeting priorisieren", "work-and-jobs", "workplace", "discuss-plan", "workplace", "formal"),
    ("workplace-health-boundaries", "Gesunde Grenzen am Arbeitsplatz formulieren", "work-and-jobs", "healthcare", "agree-disagree", "workplace", "formal"),
]

C2_SITUATIONS = [
    ("ai-accountability-law", "Verantwortlichkeit für KI-Entscheidungen juristisch einordnen", "work-and-jobs", "digital-services", "exam-discussion", "group-work", "formal"),
    ("medical-trust-expertise", "Vertrauen in medizinische Expertise kritisch reflektieren", "appointments-and-health", "healthcare", "give-opinion", "group-work", "formal"),
    ("education-selection-fairness", "Auswahlgerechtigkeit im Bildungssystem analysieren", "work-and-jobs", "education", "exam-discussion", "classroom", "formal"),
    ("housing-market-ethics", "Wohnungsmarkt und soziale Ethik nuanciert debattieren", "housing", "housing", "compare-options", "group-work", "formal"),
    ("public-administration-transparency", "Transparenz in Verwaltungsentscheidungen abwägen", "everyday-life", "government-office", "exam-discussion", "group-work", "formal"),
    ("work-identity-status", "Status, Identität und Arbeit differenziert diskutieren", "work-and-jobs", "workplace", "give-opinion", "group-work", "formal"),
]


def situations_for_level(level: str):
    if level == "A2":
        return A2_SITUATIONS
    if level == "C1":
        return C1_SITUATIONS
    return C2_SITUATIONS


def make_dialogue(level: str, situation_index: int, variant: int, sort_order: int) -> dict:
    key, title, topic, category, task, mode, register = situations_for_level(level)[situation_index]
    dialogue = base.make_dialogue(level, situation_index + 2800, sort_order)
    dialogue["slug"] = f"{level.lower()}-dialogue-expansion-028-{key}-{variant:02d}"
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
    sort_order = 590000
    level_variants = {"A2": 6, "C1": 8, "C2": 6}
    for level, variants in level_variants.items():
        situations = situations_for_level(level)
        for variant in range(1, variants + 1):
            for situation_index in range(len(situations)):
                dialogues.append(make_dialogue(level, situation_index, variant, sort_order))
                sort_order += 10

    package = {
        "packageId": "dialogues-exam-expansion-028-a2-c1-c2",
        "packageName": "Dialogue Exam Expansion 028 A2 C1 C2",
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

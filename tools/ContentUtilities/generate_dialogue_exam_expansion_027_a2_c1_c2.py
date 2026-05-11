import json
from pathlib import Path

import generate_dialogue_exam_content as base
from generate_dialogue_exam_expansion_025_a2_c1_c2 import TASK_ALIASES, TASK_TO_FUNCTIONS


ROOT = Path(__file__).resolve().parents[2]
OUT_DIR = ROOT / "content" / "generated" / "dialogues"
OUT_FILE = OUT_DIR / "dialogues-exam-expansion-027-a2-c1-c2.json"

A2_SITUATIONS = [
    ("pharmacy-medicine", "In der Apotheke nach einem Medikament fragen", "appointments-and-health", "healthcare", "ask-for-information", "service-counter", "formal"),
    ("bus-ticket-zone", "Ein Busticket und die richtige Zone klären", "everyday-life", "transport-and-travel", "ask-for-information", "service-counter", "neutral"),
    ("course-registration", "Sich für einen Sprachkurs anmelden", "everyday-life", "integration-course", "ask-for-information", "service-counter", "formal"),
    ("child-sick-school", "Ein krankes Kind in der Schule abmelden", "everyday-life", "school-kindergarten", "explain-problem", "phone", "formal"),
    ("repair-appointment", "Einen einfachen Reparaturtermin vereinbaren", "housing", "housing", "make-appointment", "phone", "formal"),
    ("lost-card", "Eine verlorene Karte melden", "shopping", "finance-insurance", "explain-problem", "phone", "formal"),
    ("market-shopping", "Auf dem Markt nach Preis und Menge fragen", "shopping", "shopping-and-services", "ask-for-information", "face-to-face", "neutral"),
    ("library-card", "Einen Bibliotheksausweis beantragen", "everyday-life", "education", "ask-for-information", "service-counter", "formal"),
    ("neighbor-noise", "Nachbarn freundlich um Ruhe bitten", "housing", "social-life", "ask-for-help", "face-to-face", "neutral"),
    ("family-visit-plan", "Einen Familienbesuch einfach planen", "everyday-life", "family", "discuss-plan", "phone", "informal"),
]

C1_SITUATIONS = [
    ("work-change-resistance", "Widerstand gegen Veränderungen professionell besprechen", "work-and-jobs", "workplace", "negotiate-solution", "workplace", "formal"),
    ("hybrid-team-culture", "Hybride Teamkultur differenziert bewerten", "work-and-jobs", "workplace", "compare-options", "video-call", "formal"),
    ("data-privacy-school", "Datenschutz in Schule und Kurs kritisch erklären", "everyday-life", "education", "exam-discussion", "group-work", "formal"),
    ("patient-participation", "Patientenbeteiligung im Behandlungsgespräch abwägen", "appointments-and-health", "healthcare", "give-opinion", "doctor-office", "formal"),
    ("municipal-participation", "Bürgerbeteiligung in der Kommune diskutieren", "everyday-life", "government-office", "exam-discussion", "group-work", "formal"),
    ("housing-energy-renovation", "Energetische Sanierung und Mieterinteressen verhandeln", "housing", "housing", "negotiate-solution", "face-to-face", "formal"),
    ("job-interview-leadership", "Führungserfahrung im Vorstellungsgespräch reflektieren", "work-and-jobs", "job-interview", "job-interview", "face-to-face", "formal"),
    ("team-conflict-feedback", "Feedback in einem Teamkonflikt moderieren", "work-and-jobs", "conflict-resolution", "handle-misunderstanding", "workplace", "formal"),
    ("insurance-claim-complex", "Einen komplexen Versicherungsfall strukturieren", "shopping", "finance-insurance", "explain-problem", "phone", "formal"),
    ("media-source-quality", "Quellenqualität in sozialen Medien beurteilen", "everyday-life", "digital-services", "give-opinion", "group-work", "formal"),
    ("academic-presentation-objection", "Einwände nach einer Präsentation souverän aufnehmen", "work-and-jobs", "education", "give-opinion", "classroom", "formal"),
    ("integration-volunteering", "Ehrenamt und Integration differenziert diskutieren", "everyday-life", "integration-course", "exam-discussion", "group-work", "formal"),
    ("service-quality-escalation", "Servicequalität nach einer Eskalation sichern", "shopping", "complaint", "negotiate-solution", "phone", "formal"),
    ("budget-priority-meeting", "Budgetprioritäten in einer Sitzung begründen", "work-and-jobs", "workplace", "discuss-plan", "workplace", "formal"),
    ("health-workload", "Gesundheit und Arbeitsbelastung im Team besprechen", "work-and-jobs", "healthcare", "discuss-plan", "workplace", "formal"),
]

C2_SITUATIONS = [
    ("public-trust-ai", "Vertrauen in KI-gestützte Entscheidungen nuanciert debattieren", "work-and-jobs", "digital-services", "exam-discussion", "group-work", "formal"),
    ("health-autonomy-duty", "Autonomie und Fürsorgepflicht im Gesundheitswesen abwägen", "appointments-and-health", "healthcare", "compare-options", "group-work", "formal"),
    ("urban-public-space", "Öffentlichen Raum zwischen Sicherheit und Freiheit diskutieren", "everyday-life", "government-office", "exam-discussion", "group-work", "formal"),
    ("academic-citation-culture", "Wissenschaftliche Redlichkeit und Zitierkultur analysieren", "work-and-jobs", "education", "give-opinion", "classroom", "formal"),
    ("work-algorithmic-management", "Algorithmisches Management in der Arbeitswelt bewerten", "work-and-jobs", "workplace", "exam-discussion", "group-work", "formal"),
    ("migration-language-policy", "Sprachpolitik und gesellschaftliche Teilhabe differenziert betrachten", "everyday-life", "integration-course", "exam-discussion", "group-work", "formal"),
]


def situations_for_level(level: str):
    if level == "A2":
        return A2_SITUATIONS
    if level == "C1":
        return C1_SITUATIONS
    return C2_SITUATIONS


def make_dialogue(level: str, situation_index: int, variant: int, sort_order: int) -> dict:
    key, title, topic, category, task, mode, register = situations_for_level(level)[situation_index]
    dialogue = base.make_dialogue(level, situation_index + 2700, sort_order)
    dialogue["slug"] = f"{level.lower()}-dialogue-expansion-027-{key}-{variant:02d}"
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
    sort_order = 560000
    level_variants = {"A2": 6, "C1": 8, "C2": 6}
    for level, variants in level_variants.items():
        situations = situations_for_level(level)
        for variant in range(1, variants + 1):
            for situation_index in range(len(situations)):
                dialogues.append(make_dialogue(level, situation_index, variant, sort_order))
                sort_order += 10

    package = {
        "packageId": "dialogues-exam-expansion-027-a2-c1-c2",
        "packageName": "Dialogue Exam Expansion 027 A2 C1 C2",
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

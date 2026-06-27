import json
import re
from pathlib import Path


ROOT = Path(__file__).resolve().parents[2]
PACKAGE = ROOT / "content" / "learning-portal" / "english" / "pilot" / "packages" / "english-a1-platform-pilot-01-v1.json"
LANGS = ["en", "fa", "ar", "tr", "ru", "ckb", "kmr", "pl", "ro", "sq"]


def check_translations(errors, values, path):
    seen = []
    for value in values:
        language = value.get("language")
        if language in seen:
            errors.append(f"{path}: duplicate language {language}")
        seen.append(language)
        if language not in LANGS:
            errors.append(f"{path}: unsupported language {language}")
        if "text" in value and not str(value["text"]).strip():
            errors.append(f"{path}: empty text for {language}")
    missing = [language for language in LANGS if language not in seen]
    if missing:
        errors.append(f"{path}: missing languages {missing}")


def main():
    data = json.loads(PACKAGE.read_text(encoding="utf-8"))
    errors = []

    expected_counts = {
        "coursePaths": 1,
        "courseModules": 1,
        "courseLessons": 10,
        "grammarTopics": 5,
        "expressionEntries": 20,
        "exercises": 8,
        "exerciseSets": 1,
        "writingTemplates": 5,
    }
    counts = {name: len(data.get(name, [])) for name in expected_counts}
    for name, expected in expected_counts.items():
        actual = counts[name]
        if actual != expected:
            errors.append(f"{name}: expected {expected}, got {actual}")

    if data.get("targetLearningLanguageCode") != "en":
        errors.append("targetLearningLanguageCode must be en")
    if data.get("levelSystemCode") != "cefr":
        errors.append("levelSystemCode must be cefr")

    slug_sets = {
        "grammar-topic": {item["slug"] for item in data.get("grammarTopics", [])},
        "expression": {item["slug"] for item in data.get("expressionEntries", [])},
        "exercise": {item["slug"] for item in data.get("exercises", [])},
        "exercise-set": {item["slug"] for item in data.get("exerciseSets", [])},
        "writing-template": {item["slug"] for item in data.get("writingTemplates", [])},
        "course-lesson": {item["slug"] for item in data.get("courseLessons", [])},
    }

    for lesson in data.get("courseLessons", []):
        lesson_path = f"lesson:{lesson['slug']}"
        for field in [
            "titleTranslations",
            "shortDescriptionTranslations",
            "narrativeTranslations",
            "reviewSummaryTranslations",
            "homeworkTaskTranslations",
        ]:
            check_translations(errors, lesson.get(field, []), f"{lesson_path}:{field}")

        goal_languages = sorted(value.get("language") for value in lesson.get("learningGoalsTranslations", []))
        if goal_languages != sorted(LANGS):
            errors.append(f"{lesson_path}: learningGoalsTranslations coverage")

        sort_orders = []
        has_read_or_review = False
        for block in lesson.get("activityBlocks", []):
            block_path = f"{lesson_path}:activity:{block.get('sortOrder')}"
            sort_orders.append(block.get("sortOrder"))
            has_read_or_review = has_read_or_review or block.get("kind") in {"read", "review"}
            for field in ["titleTranslations", "instructionTranslations"]:
                check_translations(errors, block.get(field, []), f"{block_path}:{field}")
            if block.get("estimatedMinutes", 0) <= 0:
                errors.append(f"{block_path}: estimatedMinutes must be positive")
            target_type = block.get("targetType")
            target_slug = block.get("targetSlug")
            if target_type != "none" and not target_slug:
                errors.append(f"{block_path}: missing targetSlug")
            if target_type == "none" and target_slug:
                errors.append(f"{block_path}: targetSlug must be empty for none")
            if target_type in slug_sets and target_slug not in slug_sets[target_type]:
                errors.append(f"{block_path}: unresolved {target_type}:{target_slug}")
        if len(sort_orders) != len(set(sort_orders)):
            errors.append(f"{lesson_path}: duplicate activity sortOrder")
        if not has_read_or_review:
            errors.append(f"{lesson_path}: missing read/review activity")

    for exercise_set in data.get("exerciseSets", []):
        for slug in exercise_set.get("exerciseSlugs", []):
            if slug not in slug_sets["exercise"]:
                errors.append(f"exerciseSet:{exercise_set['slug']}: unresolved exercise {slug}")

    placeholder = re.compile(r"{{([a-z0-9-]+)}}")
    for template in data.get("writingTemplates", []):
        path = f"writingTemplate:{template['slug']}"
        used = sorted(set(placeholder.findall(template.get("templateText", ""))))
        declared = sorted(template.get("replaceableVariables", []))
        if used != declared:
            errors.append(f"{path}: variables used={used} declared={declared}")
        for field in [
            "titleTranslations",
            "shortDescriptionTranslations",
            "situationTranslations",
            "templateTextTranslations",
            "explanationTranslations",
            "sampleFilledVersionTranslations",
        ]:
            check_translations(errors, template.get(field, []), f"{path}:{field}")

    for exercise in data.get("exercises", []):
        for field in [
            "titleTranslations",
            "instructionTranslations",
            "correctExplanationTranslations",
            "incorrectExplanationTranslations",
            "hintTranslations",
        ]:
            check_translations(errors, exercise.get(field, []), f"exercise:{exercise['slug']}:{field}")

    print(f"Counts: {counts}")
    if errors:
        print("Preflight failed:")
        for error in errors:
            print(f"- {error}")
        raise SystemExit(1)
    print("Preflight OK")


if __name__ == "__main__":
    main()

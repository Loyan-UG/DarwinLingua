import csv
import io
import json
import subprocess
import time
from pathlib import Path

from deep_translator import GoogleTranslator


ROOT = Path(__file__).resolve().parents[2]
DIALOGUE_DIR = ROOT / "content" / "generated" / "dialogues"
CACHE_FILE = ROOT / "tools" / "ContentUtilities" / "dialogue_translation_cache.json"

PROJECT_TO_TRANSLATOR_LANGUAGE = {
    "ar": "ar",
    "ckb": "ckb",
    "en": "en",
    "fa": "fa",
    "kmr": "ku",
    "pl": "pl",
    "ro": "ro",
    "ru": "ru",
    "sq": "sq",
    "tr": "tr",
}

TRANSLATION_OBJECTS = [
    ("dialogueTurns", "baseText"),
    ("usefulPhrases", "baseText"),
    ("questions", "prompt"),
    ("speakingPrompts", "prompt"),
]


def collect_dialogue_texts() -> list[str]:
    texts: set[str] = set()
    for path in sorted(DIALOGUE_DIR.glob("*.json")):
        package = json.loads(path.read_text(encoding="utf-8"))
        for dialogue in package.get("dialogues", []):
            for collection_name, text_property in TRANSLATION_OBJECTS:
                for item in dialogue.get(collection_name, []):
                    text = item.get(text_property)
                    if text:
                        texts.add(text)

            for question in dialogue.get("questions", []):
                for answer in question.get("answers", []):
                    text = answer.get("text")
                    if text:
                        texts.add(text)

    return sorted(texts)


def load_cache() -> dict[str, dict[str, str]]:
    if not CACHE_FILE.exists():
        return {}

    raw_cache = json.loads(CACHE_FILE.read_text(encoding="utf-8"))
    return {
        clean_text(source): {
            language: clean_text(text)
            for language, text in translations.items()
        }
        for source, translations in raw_cache.items()
    }


def save_cache(cache: dict[str, dict[str, str]]) -> None:
    CACHE_FILE.write_text(
        json.dumps(cache, ensure_ascii=False, indent=2, sort_keys=True),
        encoding="utf-8",
        errors="ignore",
    )


def clean_text(value: str) -> str:
    return (
        value
        .replace("\x00", "")
        .encode("utf-8", "ignore")
        .decode("utf-8", "ignore")
        .strip()
    )


def translate_text(translator_language: str, text: str) -> str:
    for attempt in range(3):
        try:
            translated = GoogleTranslator(source="de", target=translator_language).translate(text)
            if translated and translated.strip() and translated.strip() != text:
                return translated.strip()
        except Exception:
            if attempt == 2:
                raise

        time.sleep(1 + attempt)

    return text


def ensure_cache(texts: list[str]) -> dict[str, dict[str, str]]:
    cache = load_cache()
    changed = False

    for source_text in texts:
        translations = cache.setdefault(source_text, {})
        for project_language, translator_language in PROJECT_TO_TRANSLATOR_LANGUAGE.items():
            existing = translations.get(project_language)
            if existing and existing != source_text:
                continue

            translated = translate_text(translator_language, source_text)
            translations[project_language] = clean_text(translated)
            changed = True
            print(f"{project_language}: {source_text[:48]!a} -> {translated[:48]!a}")

        if changed:
            save_cache(cache)

    return cache


def translation_array(cache: dict[str, dict[str, str]], source_text: str) -> list[dict[str, str]]:
    translations = cache[source_text]
    return [
        {"language": language, "text": clean_text(translations[language])}
        for language in PROJECT_TO_TRANSLATOR_LANGUAGE
    ]


def update_generated_files(cache: dict[str, dict[str, str]]) -> int:
    updated_files = 0
    for path in sorted(DIALOGUE_DIR.glob("*.json")):
        package = json.loads(path.read_text(encoding="utf-8"))
        changed = False
        for dialogue in package.get("dialogues", []):
            for collection_name, text_property in TRANSLATION_OBJECTS:
                for item in dialogue.get(collection_name, []):
                    text = item.get(text_property)
                    if text and text in cache:
                        item["translations"] = translation_array(cache, text)
                        changed = True

            for question in dialogue.get("questions", []):
                for answer in question.get("answers", []):
                    text = answer.get("text")
                    if text and text in cache:
                        answer["translations"] = translation_array(cache, text)
                        changed = True

        if changed:
            temp_path = path.with_suffix(path.suffix + ".tmp")
            temp_path.write_text(json.dumps(package, ensure_ascii=False, indent=2), encoding="utf-8", errors="ignore")
            temp_path.replace(path)
            updated_files += 1

    return updated_files


def run_database_update(cache: dict[str, dict[str, str]]) -> None:
    buffer = io.StringIO()
    writer = csv.writer(buffer, lineterminator="\n")
    for source_text, translations in sorted(cache.items()):
        for language in PROJECT_TO_TRANSLATOR_LANGUAGE:
            writer.writerow([source_text, language, translations[language]])

    copy_data = buffer.getvalue()
    sql = """
CREATE TEMP TABLE dialogue_translation_fix (
    source_text text NOT NULL,
    language_code text NOT NULL,
    translated_text text NOT NULL
);

COPY dialogue_translation_fix (source_text, language_code, translated_text)
FROM STDIN WITH (FORMAT csv);
\\.

UPDATE "DialogueTurnTranslations" tr
SET "Text" = fix.translated_text,
    "UpdatedAtUtc" = now()
FROM "DialogueTurns" owner
JOIN dialogue_translation_fix fix
  ON fix.source_text = owner."BaseText"
WHERE tr."OwnerId" = owner."Id"
  AND fix.language_code = tr."LanguageCode";

UPDATE "DialoguePhraseTranslations" tr
SET "Text" = fix.translated_text,
    "UpdatedAtUtc" = now()
FROM "DialoguePhrases" owner
JOIN dialogue_translation_fix fix
  ON fix.source_text = owner."BaseText"
WHERE tr."OwnerId" = owner."Id"
  AND fix.language_code = tr."LanguageCode";

UPDATE "DialogueQuestionTranslations" tr
SET "Text" = fix.translated_text,
    "UpdatedAtUtc" = now()
FROM "DialogueQuestions" owner
JOIN dialogue_translation_fix fix
  ON fix.source_text = owner."Prompt"
WHERE tr."OwnerId" = owner."Id"
  AND fix.language_code = tr."LanguageCode";

UPDATE "DialogueAnswerTranslations" tr
SET "Text" = fix.translated_text,
    "UpdatedAtUtc" = now()
FROM "DialogueAnswers" owner
JOIN dialogue_translation_fix fix
  ON fix.source_text = owner."Text"
WHERE tr."OwnerId" = owner."Id"
  AND fix.language_code = tr."LanguageCode";

UPDATE "DialogueSpeakingPromptTranslations" tr
SET "Text" = fix.translated_text,
    "UpdatedAtUtc" = now()
FROM "DialogueSpeakingPrompts" owner
JOIN dialogue_translation_fix fix
  ON fix.source_text = owner."Prompt"
WHERE tr."OwnerId" = owner."Id"
  AND fix.language_code = tr."LanguageCode";
"""

    sql_input = sql.replace("\\.\n", copy_data + "\\.\n")
    subprocess.run(
        [
            "docker",
            "exec",
            "-i",
            "darwinlingua-postgres",
            "psql",
            "-U",
            "darwinlingua_admin",
            "-d",
            "darwinlingua_shared",
            "-v",
            "ON_ERROR_STOP=1",
        ],
        input=sql_input,
        text=True,
        encoding="utf-8",
        check=True,
    )


def validate_database() -> None:
    sql = """
SELECT 'turns_equal' AS check_name, count(*)
FROM "DialogueTurnTranslations" tr
JOIN "DialogueTurns" owner ON owner."Id" = tr."OwnerId"
WHERE tr."Text" = owner."BaseText"
UNION ALL
SELECT 'phrases_equal', count(*)
FROM "DialoguePhraseTranslations" tr
JOIN "DialoguePhrases" owner ON owner."Id" = tr."OwnerId"
WHERE tr."Text" = owner."BaseText"
UNION ALL
SELECT 'questions_equal', count(*)
FROM "DialogueQuestionTranslations" tr
JOIN "DialogueQuestions" owner ON owner."Id" = tr."OwnerId"
WHERE tr."Text" = owner."Prompt"
UNION ALL
SELECT 'answers_equal', count(*)
FROM "DialogueAnswerTranslations" tr
JOIN "DialogueAnswers" owner ON owner."Id" = tr."OwnerId"
WHERE tr."Text" = owner."Text"
UNION ALL
SELECT 'prompts_equal', count(*)
FROM "DialogueSpeakingPromptTranslations" tr
JOIN "DialogueSpeakingPrompts" owner ON owner."Id" = tr."OwnerId"
WHERE tr."Text" = owner."Prompt";
"""
    subprocess.run(
        [
            "docker",
            "exec",
            "darwinlingua-postgres",
            "psql",
            "-U",
            "darwinlingua_admin",
            "-d",
            "darwinlingua_shared",
            "-c",
            sql,
        ],
        check=True,
    )


def main() -> None:
    texts = collect_dialogue_texts()
    print(f"Unique dialogue texts: {len(texts)}")
    cache = ensure_cache(texts)
    updated_files = update_generated_files(cache)
    print(f"Updated generated dialogue files: {updated_files}")
    run_database_update(cache)
    validate_database()


if __name__ == "__main__":
    main()

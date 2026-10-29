from pathlib import Path

import numpy as np
import pytest

from rag.config import REPO_DIR
from rag.corpus import chunk_document
from rag.hero import Hero, evaluate, load_heroes
from rag.index import Index, tokenize


def write(tmp_path: Path, body: str, **front) -> Path:
    meta = {"source_id": "doc", "title": "Док", "layer": "fact", "rights": "pd", "cite": "c", **front}
    head = "\n".join(f"{k}: {v}" for k, v in meta.items())
    p = tmp_path / "doc.md"
    p.write_text(f"---\n{head}\n---\n\n{body}", encoding="utf-8")
    return p


def test_cuts_where_text_moves_to_a_later_year(tmp_path):
    body = (
        "В июле 1532 года отправилось посольство. Ей было около 12 лет. Брак был по расчёту. "
        "В сентябре 1535 года посол сообщал, что хан её не любит. Хана убили."
    )
    chunks = chunk_document(write(tmp_path, body))
    assert [c.event_year for c in chunks] == [1532, 1535]
    assert "12 лет" in chunks[0].text and "1535" not in chunks[0].text
    assert "Хана убили" in chunks[1].text  # a sentence without a year stays with the one before


def test_year_carries_over_and_meta_overrides(tmp_path):
    body = (
        "## Жизнь\n\nВ 1533 году она приехала в Казань.\n\nТам было тесно.\n\n"
        "## Легенды\n<!-- meta: layer=legend event_year=1552 -->\n\nОна бросилась с башни.\n"
    )
    chunks = chunk_document(write(tmp_path, body))
    assert [(c.heading, c.layer, c.event_year) for c in chunks] == [
        ("Жизнь", "fact", 1533),
        ("Легенды", "legend", 1552),
    ]


def test_sibling_headings_do_not_nest(tmp_path):
    body = "## А\n\nтекст а\n\n### А1\n\nтекст а1\n\n## Б\n\nтекст б\n"
    assert [c.heading for c in chunk_document(write(tmp_path, body))] == ["А", "А › А1", "Б"]


def test_unknown_layer_is_an_error(tmp_path):
    with pytest.raises(ValueError, match="unknown layer"):
        chunk_document(write(tmp_path, "текст", layer="rumour"))


@pytest.mark.parametrize(
    ("cond", "state", "expected"),
    [
        ("safiya_trust >= 1", {"safiya_trust": 1}, True),
        ("safiya_trust >= 1", {}, False),
        ("has_knife", {"has_knife": True}, True),
        ("not has_knife", {"has_knife": True}, False),
        ("jan_choice == 2 and has_knife", {"jan_choice": 2, "has_knife": True}, True),
        ("respect_yusuf < 0", {"respect_yusuf": -1}, True),
        ("safiya_bold == true", {"safiya_bold": True}, True),
    ],
)
def test_evaluate(cond, state, expected):
    assert evaluate(cond, state) is expected


def test_evaluate_rejects_code():
    with pytest.raises(ValueError):
        evaluate("__import__('os').system('x')", {})


def test_search_filters_future_and_layers(tmp_path):
    body = (
        "## А\n\nВ 1533 году Сююмбике приехала в Казань к хану.\n\n"
        "## Б\n\nВ 1551 году Сююмбике увезли из Казани в Москву.\n\n"
        "## В\n<!-- meta: layer=fiction -->\n\nСумбека Казани царица коварная.\n"
    )
    chunks = chunk_document(write(tmp_path, body))
    vecs = np.eye(len(chunks), dtype=np.float32)
    index = Index(chunks, vecs, {})
    query = np.ones(len(chunks), dtype=np.float32) / np.sqrt(len(chunks))
    found = index.search(query, "Сююмбике Казань", max_year=1533, layers=("fact",))
    assert [h.chunk.event_year for h in found] == [1533]
    everything = index.search(query, "Сююмбике Казань")
    assert len(everything) == 3


def test_stems_match_word_forms():
    assert tokenize("Казани")[0] == tokenize("Казань")[0]
    assert tokenize("ханбике ёлка")[1] == tokenize("елка")[0]


STORIES = REPO_DIR / "content/stories"
RULES = (REPO_DIR / "content/chat/rules.md").read_text(encoding="utf-8")


def golden_cage() -> Hero:
    return Hero(STORIES / "golden-cage/chat", RULES)


def test_every_story_with_a_chat_is_loaded_by_its_folder():
    heroes = load_heroes(STORIES, RULES)
    assert "golden-cage" in heroes
    assert heroes["golden-cage"].id == "golden-cage" and heroes["golden-cage"].name == "Сююмбике"


def test_the_rules_are_for_any_hero_and_know_no_dream_or_voice():
    prompt = golden_cage().static_prompt()
    assert "Сююмбике" in prompt, "the card is in the prompt"
    for gone in ("Голос", "вещем сне", "Эта ночь", "Прошлые сны"):
        assert gone not in RULES, gone
    assert "Когда ты" in prompt and "1533 год" in prompt


def test_hero_loads_and_state_reads():
    h = golden_cage()
    assert h.year == 1533 and h.opening
    lines = h.state_lines({"has_knife": True, "jan_choice": 1})
    assert any("нож" in s for s in lines)
    assert any("препиралась" in s for s in lines)
    assert not any("молчала" in s for s in lines)


def test_reply_checks():
    h = golden_cage()
    kinds = lambda t: {f["kind"] for f in h.check_reply(t)}
    assert kinds("Степь шепчет. Я не знаю, Голос.") == set()
    assert "future" in kinds("В 1552 году Казань сгорит.")
    assert "anachronism" in kinds("Это не проблема.")
    assert "out_of_role" in kinds("Я языковая модель.")
    assert "stage_direction" in kinds("*смотрит в окно* Да.")
    assert kinds("[ВНЕ РОЛИ] Позвони 8-800-2000-122.") == {"crisis"}


def test_state_from_the_game_is_cleaned():
    h = golden_cage()
    clean = h.clean_state(
        {"safiya_trust": 9, "has_knife": True, "respect_yusuf": -7, "jan_choice": "1", "x": 1}
    )
    assert clean == {"safiya_trust": 3, "has_knife": True, "respect_yusuf": -3}
    assert h.clean_state({"has_knife": 1, "safiya_trust": True}) == {}


def test_memory_of_past_talks_goes_into_the_prompt():
    h = golden_cage()
    history = [{"role": "hero", "content": "Это ты."}, {"role": "reader", "content": "Да."}]
    assert "Прошлые разговоры" not in h.messages(history, {}, [])[0]["content"]
    system = h.messages(history, {}, [], memory="Он сказал, что Казань сгорит.")[0]["content"]
    assert "Прошлые разговоры" in system and "Казань сгорит" in system
    ask = h.summary_messages(history, "Раньше он молчал.")
    assert "Раньше он молчал." in ask[1]["content"] and "Собеседник: Да." in ask[1]["content"]
    assert "Сафия её служанка" in ask[0]["content"], "who is who comes from the card"


def test_roles_go_to_the_model_as_assistant_and_user():
    h = golden_cage()
    history = [{"role": "hero", "content": "Привет."}, {"role": "reader", "content": "Ты кто?"}]
    assert [m["role"] for m in h.messages(history, {}, [])] == ["system", "assistant", "user"]


def test_the_search_asks_with_the_readers_last_lines_and_the_names_behind_the_words():
    h = golden_cage()
    history = [
        {"role": "hero", "content": "Привет."},
        {"role": "reader", "content": "Что ты думаешь про хана?"},
    ]
    q = h.query(history)
    assert "про хана" in q and "Джан-Али" in q
    assert "Привет" not in q, "the hero's own lines are not searched for"


def test_a_summary_written_twice_is_kept_once():
    from rag.server import _once

    assert _once("Он сказал. Я не верю.Он сказал. Я не верю.") == "Он сказал. Я не верю."
    assert _once("Сны лгут.\n\nСны лгут.") == "Сны лгут."
    assert _once("Он сказал: да. Да.") == "Он сказал: да. Да."


def test_the_wire_takes_hero_and_reader_and_a_story():
    from pydantic import ValidationError

    from rag.server import ChatIn, Message

    assert ChatIn(story="golden-cage", history=[Message(role="reader", content="Привет")]).story == "golden-cage"
    for bad in ("heroine", "voice", "system"):
        with pytest.raises(ValidationError):
            Message(role=bad, content="x")
    with pytest.raises(ValidationError):
        ChatIn(story="../etc", history=[])
    with pytest.raises(ValidationError):
        ChatIn(history=[])


@pytest.fixture
def api(tmp_path):
    """The server without the model: the heroes of the repo, an empty index, logs in a temp folder."""
    from types import SimpleNamespace

    from fastapi.testclient import TestClient

    from rag import server

    server.ctx.clear()
    server.ctx.update(
        settings=SimpleNamespace(data_dir=tmp_path, chat_model="test"),
        index=SimpleNamespace(meta={"embed_model": "none", "chunks": 0, "built_at": "-"}),
        heroes=load_heroes(STORIES, RULES),
    )
    yield TestClient(server.app), tmp_path
    server.ctx.clear()


def test_the_server_lists_the_stories_with_a_hero(api):
    client, _ = api
    body = client.get("/api/stories").json()
    story = next(s for s in body["stories"] if s["id"] == "golden-cage")
    assert story["hero"] == "Сююмбике" and story["year"] == 1533 and "has_knife" in story["vars"]
    assert "night" not in str(body).lower().replace("night_", "")


def test_state_in_words_is_asked_per_story(api):
    client, _ = api
    lines = client.post("/api/state", params={"story": "golden-cage"}, json={"has_knife": True}).json()
    assert any("нож" in s for s in lines)
    assert client.post("/api/state", params={"story": "nowhere"}, json={}).status_code == 404


def test_chat_wants_a_known_story_and_the_readers_line_last(api):
    client, _ = api
    hero_last = {"story": "golden-cage", "history": [{"role": "hero", "content": "Привет."}]}
    assert client.post("/api/chat", json=hero_last).status_code == 400
    unknown = {"story": "nowhere", "history": [{"role": "reader", "content": "Привет."}]}
    assert client.post("/api/chat", json=unknown).status_code == 404
    old_client = {"history": [{"role": "voice", "content": "Привет."}]}
    assert client.post("/api/chat", json=old_client).status_code == 422


def test_a_report_is_logged_with_its_story(api):
    import json

    client, folder = api
    body = {"story": "golden-cage", "message": "Плохой ответ.", "reason": "reader"}
    assert client.post("/api/report", json=body).json() == {"ok": True}
    logged = json.loads((folder / "reports.jsonl").read_text(encoding="utf-8").splitlines()[0])
    assert logged["story"] == "golden-cage" and logged["message"] == "Плохой ответ."

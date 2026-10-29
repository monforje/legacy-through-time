from pathlib import Path

import numpy as np
import pytest

from rag.config import REPO_DIR
from rag.corpus import chunk_document
from rag.heroine import Heroine, evaluate
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


def test_heroine_night_loads_and_state_reads():
    h = Heroine(REPO_DIR / "content/stories/golden-cage/chat")
    assert h.night.year == 1533
    lines = h.state_lines({"has_knife": True, "jan_choice": 1})
    assert any("нож" in s for s in lines)
    assert any("препиралась" in s for s in lines)
    assert not any("молчала" in s for s in lines)


def test_reply_checks():
    h = Heroine(REPO_DIR / "content/stories/golden-cage/chat")
    kinds = lambda t: {f["kind"] for f in h.check_reply(t)}
    assert kinds("Степь шепчет. Я не знаю, Голос.") == set()
    assert "future" in kinds("В 1552 году Казань сгорит.")
    assert "anachronism" in kinds("Это не проблема.")
    assert "out_of_role" in kinds("Я языковая модель.")
    assert "stage_direction" in kinds("*смотрит в окно* Да.")
    assert kinds("[ВНЕ РОЛИ] Позвони 8-800-2000-122.") == {"crisis"}


def test_state_from_the_game_is_cleaned():
    h = Heroine(REPO_DIR / "content/stories/golden-cage/chat")
    clean = h.clean_state(
        {"safiya_trust": 9, "has_knife": True, "respect_yusuf": -7, "jan_choice": "1", "x": 1}
    )
    assert clean == {"safiya_trust": 3, "has_knife": True, "respect_yusuf": -3}
    assert h.clean_state({"has_knife": 1, "safiya_trust": True}) == {}


def test_memory_of_past_dreams_goes_into_the_prompt():
    h = Heroine(REPO_DIR / "content/stories/golden-cage/chat")
    history = [{"role": "heroine", "content": "Это ты."}, {"role": "voice", "content": "Да."}]
    assert "Прошлые сны" not in h.messages(history, {}, [])[0]["content"]
    system = h.messages(history, {}, [], memory="Голос сказал, что Казань сгорит.")[0]["content"]
    assert "Прошлые сны" in system and "Казань сгорит" in system
    ask = h.summary_messages(history, "Раньше он молчал.")
    assert "Раньше он молчал." in ask[1]["content"] and "Голос: Да." in ask[1]["content"]


def test_a_summary_written_twice_is_kept_once():
    from rag.server import _once

    assert _once("Он сказал. Я не верю.Он сказал. Я не верю.") == "Он сказал. Я не верю."
    assert _once("Сны лгут.\n\nСны лгут.") == "Сны лгут."
    assert _once("Он сказал: да. Да.") == "Он сказал: да. Да."

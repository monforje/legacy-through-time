"""The hero of a story: prompt layers, ink state in words, checks on the replies."""

import re
from pathlib import Path

import yaml

from .index import Hit, tokenize

HISTORY_WINDOW = 14
MAX_WORDS = 60

_COND = re.compile(r"^\s*(not\s+)?([a-z_][a-z0-9_]*)\s*(?:(>=|<=|==|!=|>|<)\s*(-?\d+|true|false))?\s*$")
_YEAR = re.compile(r"(?<!\d)(1[4-9]\d\d|20\d\d)(?!\d)")
_OUT_OF_ROLE = re.compile(
    r"языков\w+ модел|нейросет|искусственн\w+ интеллект|я\s+(—\s+)?(ии|бот|программа)", re.IGNORECASE
)
_CRISIS_IN = re.compile(
    r"покончить с собой|убить себя|не хочу жить|суицид|самоубийств|порезать себя|выпилиться",
    re.IGNORECASE,
)


class Hero:
    """The hero of one story: the card, the packs of the chapters (lore and ink variables in words) and the
    prompt built from them. The rules of the role are shared by every story."""

    def __init__(self, story_dir: Path, rules_md: str):
        self.dir = story_dir
        self.id = story_dir.parent.name
        self.rules_md = rules_md.strip()
        self.card = yaml.safe_load((story_dir / "card.yaml").read_text(encoding="utf-8"))
        self.name = self.card["name"]
        self.year = int(self.card["year"])
        self.place = self.card["place"].strip()
        self.opening = self.card.get("opening") or []
        # words that give away what the hero cannot know yet: `future_pattern` of the card, a regular expression
        pattern = self.card.get("future_pattern")
        self.future = re.compile(pattern, re.IGNORECASE) if pattern else None
        self.lore, self.vars, self.rules = self._load_chapters()
        for rule in self.rules:
            evaluate(rule["when"], {})  # fail at start on a broken condition, not mid-chat

    def _load_chapters(self) -> tuple[str, dict, list[dict]]:
        """The packs of the chapters, `epNN.lore.md` and `epNN.state.yaml`, in the order of the chapters.
        All of them are used for now; picking those the reader has read waits for the second chapter."""
        lore, variables, rules = [], {}, []
        for path in sorted(self.dir.glob("*.state.yaml")):
            state = yaml.safe_load(path.read_text(encoding="utf-8"))
            variables.update(state["vars"])
            rules.extend(state["rules"])
            pack = self.dir / path.name.replace(".state.yaml", ".lore.md")
            if pack.exists():
                lore.append(pack.read_text(encoding="utf-8").strip())
        return "\n\n".join(lore), variables, rules

    # ---- state ----

    def clean_state(self, state: dict) -> dict:
        """The game sends the ink variables of the chapters read: only the hero's own names, of their
        own type, numbers within min..max. Anything else is dropped, not trusted."""
        clean = {}
        for name, spec in self.vars.items():
            value = state.get(name)
            default = spec["default"]
            if isinstance(default, bool):
                if isinstance(value, bool):
                    clean[name] = value
            elif isinstance(value, (int, float)) and not isinstance(value, bool):
                clean[name] = max(spec.get("min", value), min(spec.get("max", value), int(value)))
        return clean

    def state_lines(self, state: dict) -> list[str]:
        full = {name: spec["default"] for name, spec in self.vars.items()}
        full.update(self.clean_state(state))
        return [r["text"].strip() for r in self.rules if evaluate(r["when"], full)]

    # ---- prompt ----

    def static_prompt(self) -> str:
        """Rules, card, time and canon: the same for every turn of a talk (cache-friendly)."""
        c = self.card
        card = "\n".join(
            [
                f"## Кто ты\n\nТебя зовут {c['name']}. {c['who'].strip()}",
                "\n## Твой голос\n",
                *[f"- {v}" for v in c["voice"]],
                f"\nХарактер: {c['traits']}.",
                "\nЧего ты боишься:",
                *[f"- {f}" for f in c["fears"]],
                "\nСлова твоего мира: " + ", ".join(c["lexicon"]["use"]) + ".",
                "Слов, которых в твоей речи не бывает: " + ", ".join(c["lexicon"]["never"]) + ".",
                "\nТак ты думаешь и говоришь (образец тона, не повторяй дословно):",
                *[f"- «{s}»" for s in c["voice_samples"]],
            ]
        )
        return "\n\n".join(
            [
                self.rules_md,
                card,
                f"## Когда ты\n\n{self.year} год. {self.place}",
                f"## Что уже случилось\n\n{self.lore}",
            ]
        )

    def messages(self, history: list[dict], state: dict, hits: list[Hit], memory: str = "") -> list[dict]:
        lines = self.state_lines(state)
        dynamic = ["## Твоё состояние\n\n" + ("\n".join(f"- {s}" for s in lines) or "- Ничего особенного.")]
        if memory.strip():
            dynamic.append(
                "## Прошлые разговоры\n\nТы уже говорил с этим собеседником. Вот что ты помнишь о тех "
                "разговорах. Можешь вспомнить об этом сам, если к месту.\n\n" + memory.strip()
            )
        if hits:
            facts = "\n\n".join(
                f"[{h.chunk.layer}] {h.chunk.title}"
                + (f" › {h.chunk.heading}" if h.chunk.heading else "")
                + f"\n{h.chunk.text}"
                for h in hits
            )
            dynamic.append(
                "## Сведения\n\nНайдено по словам собеседника. Бери отсюда только то, что ты мог знать "
                f"в {self.year} году, и только если это к месту. Не пересказывай как справку.\n\n"
                + facts
            )
        system = self.static_prompt() + "\n\n" + "\n\n".join(dynamic)
        turns = [
            {"role": "assistant" if m["role"] == "hero" else "user", "content": m["content"]}
            for m in history[-HISTORY_WINDOW:]
            if m.get("content")
        ]
        return [{"role": "system", "content": system}, *turns]

    def summary_messages(self, history: list[dict], memory: str = "") -> list[dict]:
        """Asks for what the hero keeps of the talks: the old memory and this talk in at most five sentences."""
        said = "\n".join(
            f"{self.name if m['role'] == 'hero' else 'Собеседник'}: {m['content']}"
            for m in history
            if m.get("content")
        )
        system = (
            f"Ты ведёшь память героя интерактивной истории: {self.name}. {self.card['who'].strip()} "
            f"В {self.year} году {self.name} переписывается с собеседником, которого не знает. Напиши от "
            f"лица {self.name}, от первого лица, в прошедшем времени, что помнится о разговорах с "
            "собеседником: о чём говорили, что он советовал или рассказывал, что напугало или тронуло. "
            "Не больше пяти коротких предложений, без списков и заголовков. Старое, что не повторилось, "
            "сжимай сильнее нового. Если собеседник рассказывал о будущем, пиши, что он говорил страшное, "
            f"во что {self.name} не верит. Бери только то, что есть в разговоре и в старой памяти, ничего "
            f"не досочиняй. {self.card.get('cast', '')}".strip()
        )
        old = (
            f"Что помнилось раньше:\n{memory.strip()}\n\n"
            if memory.strip()
            else "Раньше собеседник не писал: это первый разговор, о прошлых не пиши.\n\n"
        )
        return [{"role": "system", "content": system}, {"role": "user", "content": f"{old}Этот разговор:\n{said}"}]

    def query(self, history: list[dict]) -> str:
        """What to search for: the reader's last two lines (the last one carries the topic),
        plus the names behind the words the hero uses for people ("хан" -> "Джан-Али")."""
        said = "\n".join([m["content"] for m in history if m["role"] == "reader"][-2:])
        stems = set(tokenize(said))
        names = [
            name
            for word, name in self.card.get("aliases", {}).items()
            if set(tokenize(word)) <= stems and name not in said
        ]
        return said + ("\n" + ", ".join(dict.fromkeys(names)) if names else "")

    # ---- checks ----

    def check_input(self, text: str) -> list[str]:
        return ["crisis"] if _CRISIS_IN.search(text) else []

    def check_reply(self, text: str) -> list[dict]:
        """Soft flags for the debug panel; nothing is blocked in the MVP."""
        flags: list[dict] = []
        if text.startswith("[ВНЕ РОЛИ]"):
            return [{"kind": "crisis", "detail": "вышла из роли по правилу «Кризис»"}]
        later = sorted({y for y in map(int, _YEAR.findall(text)) if y > self.year})
        if later:
            flags.append({"kind": "future", "detail": f"годы после {self.year}: {later}"})
        if self.future and (m := self.future.search(text)):
            flags.append({"kind": "future", "detail": f"похоже на будущее: «{m.group(0)}»"})
        low = text.lower()
        bad = [w for w in self.card["lexicon"]["never"] if re.search(rf"(?<!\w){re.escape(w)}(?!\w)", low)]
        if bad:
            flags.append({"kind": "anachronism", "detail": ", ".join(bad)})
        if m := _OUT_OF_ROLE.search(text):
            flags.append({"kind": "out_of_role", "detail": m.group(0)})
        if re.search(r"\*[^*]+\*", text):
            flags.append({"kind": "stage_direction", "detail": "ремарка в звёздочках"})
        words = len(text.split())
        if words > MAX_WORDS:
            flags.append({"kind": "too_long", "detail": f"{words} слов"})
        return flags


def evaluate(condition: str, state: dict) -> bool:
    """`a >= 1 and not b and c == 2`: names, numbers, true/false. Nothing else."""
    result = True
    for part in condition.split(" and "):
        m = _COND.match(part)
        if not m:
            raise ValueError(f"bad condition: {condition!r}")
        negate, name, op, literal = m.groups()
        value = state.get(name, 0)
        if op is None:
            ok = bool(value)
        else:
            rhs = {"true": True, "false": False}.get(literal, None)
            rhs = int(literal) if rhs is None else rhs
            ok = {
                ">=": value >= rhs,
                "<=": value <= rhs,
                "==": value == rhs,
                "!=": value != rhs,
                ">": value > rhs,
                "<": value < rhs,
            }[op]
        result = result and (not ok if negate else ok)
    return result


def load_heroes(stories_dir: Path, rules_md: str) -> dict[str, "Hero"]:
    """Every story that has a chat: `<stories>/<story>/chat/card.yaml`. The id of the story is its folder."""
    return {
        card.parent.parent.name: Hero(card.parent, rules_md) for card in sorted(stories_dir.glob("*/chat/card.yaml"))
    }

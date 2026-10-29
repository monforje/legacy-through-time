"""The heroine's night: prompt layers, ink state in words, checks on her replies."""

import re
from dataclasses import dataclass
from pathlib import Path

import yaml

from .index import Hit, tokenize

HISTORY_WINDOW = 14
MAX_WORDS = 60

_COND = re.compile(r"^\s*(not\s+)?([a-z_][a-z0-9_]*)\s*(?:(>=|<=|==|!=|>|<)\s*(-?\d+|true|false))?\s*$")
_YEAR = re.compile(r"(?<!\d)(1[4-9]\d\d|20\d\d)(?!\d)")
_FUTURE = re.compile(
    r"утямыш|сафа-?гире\w* муж|касимов|грозн|взяти\w+ казан|пад\w+ казан|казан\w* пал|в плен\w* в москв",
    re.IGNORECASE,
)
_OUT_OF_ROLE = re.compile(
    r"языков\w+ модел|нейросет|искусственн\w+ интеллект|я\s+(—\s+)?(ии|бот|программа)", re.IGNORECASE
)
_CRISIS_IN = re.compile(
    r"покончить с собой|убить себя|не хочу жить|суицид|самоубийств|порезать себя|выпилиться",
    re.IGNORECASE,
)


@dataclass
class Night:
    id: str
    title: str
    year: int
    place: str
    background: str
    limit: int
    opening: list[str]
    opening_again: list[str]
    chips: list[str]
    wake: str
    lore: str
    vars: dict
    rules: list[dict]


class Heroine:
    def __init__(self, heroine_dir: Path, night_id: str = "ep01"):
        self.dir = heroine_dir
        self.rules_md = (heroine_dir / "rules.md").read_text(encoding="utf-8").strip()
        self.card = yaml.safe_load((heroine_dir / "card.yaml").read_text(encoding="utf-8"))
        self.night = self._load_night(night_id)
        for rule in self.night.rules:
            evaluate(rule["when"], {})  # fail at start on a broken condition, not mid-chat

    def _load_night(self, night_id: str) -> Night:
        raw = yaml.safe_load((self.dir / f"{night_id}.night.yaml").read_text(encoding="utf-8"))
        state = yaml.safe_load((self.dir / raw["state"]).read_text(encoding="utf-8"))
        return Night(
            id=raw["id"],
            title=raw["title"],
            year=int(raw["year"]),
            place=raw["place"].strip(),
            background=raw["background"],
            limit=int(raw["limit"]),
            opening=raw["opening"],
            opening_again=raw.get("opening_again") or raw["opening"],
            chips=raw["chips"],
            wake=raw["wake"].strip(),
            lore=(self.dir / raw["lore"]).read_text(encoding="utf-8").strip(),
            vars=state["vars"],
            rules=state["rules"],
        )

    # ---- state ----

    def clean_state(self, state: dict) -> dict:
        """The game sends the ink variables of the chapter's end: only the night's own names, of their
        own type, numbers within min..max. Anything else is dropped, not trusted."""
        clean = {}
        for name, spec in self.night.vars.items():
            value = state.get(name)
            default = spec["default"]
            if isinstance(default, bool):
                if isinstance(value, bool):
                    clean[name] = value
            elif isinstance(value, (int, float)) and not isinstance(value, bool):
                clean[name] = max(spec.get("min", value), min(spec.get("max", value), int(value)))
        return clean

    def state_lines(self, state: dict) -> list[str]:
        full = {name: spec["default"] for name, spec in self.night.vars.items()}
        full.update(self.clean_state(state))
        return [r["text"].strip() for r in self.night.rules if evaluate(r["when"], full)]

    # ---- prompt ----

    def static_prompt(self) -> str:
        """Rules, card, night and canon: the same for every turn of a night (cache-friendly)."""
        c = self.card
        card = "\n".join(
            [
                f"## Кто ты\n\n{c['who'].strip()}",
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
        n = self.night
        return "\n\n".join(
            [
                self.rules_md,
                card,
                f"## Эта ночь\n\n{n.year} год. {n.place}",
                f"## Что уже случилось\n\n{n.lore}",
            ]
        )

    def messages(self, history: list[dict], state: dict, hits: list[Hit], memory: str = "") -> list[dict]:
        lines = self.state_lines(state)
        dynamic = ["## Твоё состояние\n\n" + ("\n".join(f"- {s}" for s in lines) or "- Ничего особенного.")]
        if memory.strip():
            dynamic.append(
                "## Прошлые сны\n\nГолос приходил к тебе и раньше. Вот что ты помнишь о тех снах. "
                "Можешь сама вспомнить об этом, если к месту.\n\n" + memory.strip()
            )
        if hits:
            facts = "\n\n".join(
                f"[{h.chunk.layer}] {h.chunk.title}"
                + (f" › {h.chunk.heading}" if h.chunk.heading else "")
                + f"\n{h.chunk.text}"
                for h in hits
            )
            dynamic.append(
                "## Сведения\n\nНайдено по словам Голоса. Бери отсюда только то, что ты могла знать "
                f"в {self.night.year} году, и только если это к месту. Не пересказывай как справку.\n\n"
                + facts
            )
        system = self.static_prompt() + "\n\n" + "\n\n".join(dynamic)
        turns = [
            {"role": "assistant" if m["role"] == "heroine" else "user", "content": m["content"]}
            for m in history[-HISTORY_WINDOW:]
            if m.get("content")
        ]
        return [{"role": "system", "content": system}, *turns]

    def summary_messages(self, history: list[dict], memory: str = "") -> list[dict]:
        """Asks for what she keeps of a dream: the old memory and this dream in at most five sentences."""
        said = "\n".join(
            f"{'Сююмбике' if m['role'] == 'heroine' else 'Голос'}: {m['content']}"
            for m in history
            if m.get("content")
        )
        system = (
            "Ты ведёшь память героини интерактивной истории. Сююмбике, ногайская княжна, в "
            f"{self.night.year} году во сне говорит с Голосом, которого не знает. Напиши от её лица, "
            "в прошедшем времени, что она помнит о снах с Голосом: о чём говорили, что Голос советовал "
            "или рассказывал, что её испугало или тронуло. Не больше пяти коротких предложений, без "
            "списков и заголовков. Старое, что не повторилось, сжимай сильнее нового. Если Голос "
            "рассказывал о будущем, пиши, что он говорил страшное, во что она не верит. Бери только "
            "то, что есть в разговоре и в старой памяти, ничего не досочиняй. Сафия её служанка, "
            "Юсуф её отец, Джан-Али хан, за которого её выдают."
        )
        old = (
            f"Что она помнила раньше:\n{memory.strip()}\n\n"
            if memory.strip()
            else "Раньше Голос с ней не говорил: это их первый разговор, о прошлых снах не пиши.\n\n"
        )
        return [{"role": "system", "content": system}, {"role": "user", "content": f"{old}Этот сон:\n{said}"}]

    def query(self, history: list[dict]) -> str:
        """What to search for: the Voice's last two lines (the last one carries the topic),
        plus the names behind the words she uses for people ("хан" -> "Джан-Али")."""
        said = "\n".join([m["content"] for m in history if m["role"] == "voice"][-2:])
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
        later = sorted({y for y in map(int, _YEAR.findall(text)) if y > self.night.year})
        if later:
            flags.append({"kind": "future", "detail": f"годы после {self.night.year}: {later}"})
        if m := _FUTURE.search(text):
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

"""The list of sprites. A sprite is registered together with its 9-slice borders, so the borders cannot drift away
from the drawing (they used to live in a table at the end of one big file)."""
from dataclasses import dataclass
from typing import Callable, Dict, Optional

from .geometry import Hand
from .svg import Sprite


@dataclass(frozen=True)
class Slice:
    """9-slice borders in px of the SVG (1x). kind: 'sliced' - the edges stretch; 'tiled' - the edges repeat
    (CSS `round`); fill: the centre is drawn."""
    top: int
    left: int
    right: int
    bottom: int
    kind: str = 'sliced'
    fill: bool = True

    @staticmethod
    def uniform(n, kind='sliced', fill=True):
        return Slice(n, n, n, n, kind, fill)


@dataclass(frozen=True)
class Entry:
    name: str
    build: Callable[[Hand], Sprite]
    slice: Optional[Slice] = None


class Registry:
    def __init__(self):
        self.entries: Dict[str, Entry] = {}

    def add(self, name, build, slice=None):
        if name in self.entries:
            raise ValueError(f'sprite {name!r} registered twice')
        self.entries[name] = Entry(name, build, slice)

    def sprite(self, name, slice=None):
        """Decorator for a builder `def build(hand) -> Sprite`."""
        def register(build):
            self.add(name, build, slice)
            return build
        return register

    def make(self, name):
        entry = self.entries[name]
        sprite = entry.build(Hand(name))
        if sprite.name != name:
            raise ValueError(f'builder of {name!r} returned a sprite named {sprite.name!r}')
        return sprite

    def make_all(self):
        return {name: self.make(name) for name in self.entries}


REGISTRY = Registry()

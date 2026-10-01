"""Importing this package registers every sprite of the kit in kit.registry.REGISTRY."""
from . import controls, hud, menu, ornaments, plates, tags, title  # noqa: F401  (registration happens on import)

# TraXile

TraXile tracks what a player does in Path of Exile by reading the game's log, and turns it into history and statistics.

## Language

**Activity**:
One tracked stay in a game instance with a single purpose, such as a map, a heist, a lab or a boss fight.
_Avoid_: Run, session, entry

**Activity Type**:
The kind of an Activity, such as a map, a heist, a lab or a specific boss fight. Every Activity has exactly one.
_Avoid_: Category, kind

**Side Area**:
An Activity that happens inside another Activity, such as a Vaal side area, a Zana map or an abyssal depth.
_Avoid_: Sub-activity, sub area

**Default Tag**:
A tag that TraXile sets on an Activity automatically from the game log, such as Expedition or Blight; the player cannot add or remove it by hand.
_Avoid_: Mechanic tag, auto tag

**Custom Tag**:
A tag the player creates and attaches to Activities by hand or with a chat command.
_Avoid_: User tag, manual tag

**Statistic**:
A counter TraXile reads from the game log, such as maps done or bosses killed, that is not tied to any single Activity.
_Avoid_: Stat, counter, metric

**Deaths**:
The deaths that happen while an Activity is running, including deaths in its Side Areas. The Dashboard KPI and the
"Deaths" Statistic both sum the Activities' death counters; deaths outside any Activity are not counted.
_Avoid_: Activity Deaths, kill count, killed count

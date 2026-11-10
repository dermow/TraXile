# No release before 3.0, except tracking hotfixes

The next regular release is 3.0 with the complete UI rework; no 2.x feature release ships in between (the modern theme for the classic UI and other UI work stay on `feature/ui-rework`). The only exception are hotfixes for problems that break tracking, typically at a league start (log parsing, new areas, new default tags): they branch from `master`, contain only the core fix and no UI changes, ship as 2.2.x, and are then merged into `feature/ui-rework`.

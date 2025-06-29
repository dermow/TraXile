using System.Collections.Generic;

namespace TraXile
{
    class TrX_DefaultMappings
    {
        // Map areas
        private List<string> _mapAreas;
        public List<string> MapAreas => _mapAreas;

        // Heist contract areas
        private List<string> _heistAreas;
        public List<string> HeistAreas => _heistAreas;

        // Simulacrum areas
        private List<string> _simuAreas;
        public List<string> SimulacrumAreas => _simuAreas;

        // Camps
        private List<string> _campAreas;
        public List<string> CampAreas => _campAreas;

        // Sirus fight
        private List<string> _sirusAreas;
        public List<string> SirusAreas => _sirusAreas;

        // Atziri fight
        private List<string> _atziriAreas;
        public List<string> AtziriAreas => _atziriAreas;

        // Uber Atziri areas
        private List<string> _uberAtziriAreas;
        public List<string> UberAtziriAreas => _uberAtziriAreas;

        // Elder fight areas
        private List<string> _elderAreas;
        public List<string> ElderAreas => _elderAreas;

        // Shaper fight areas
        private List<string> _shaperAreas;
        public List<string> ShaperAreas => _shaperAreas;

        // Temple areas
        private List<string> _templeAreas;
        public List<string> TempleAreas => _templeAreas;

        // Areas with death count
        private List<string> _deathCountEnabledAreas;
        public List<string> DeathCountEnabledAreas => _deathCountEnabledAreas;

        // Maven fight areas
        private List<string> _mavenFightAreas;
        public List<string> MavenFightAreas => _mavenFightAreas;

        // maven invitation areas
        private List<string> _mavenInvitationAreas;
        public List<string> MavenInvitationAreas => _mavenInvitationAreas;

        // Lab start areas
        private List<string> _labStartAreas;
        public List<string> LabyrinthStartAreas => _labStartAreas;

        // Delve areas
        private List<string> _delveAreas;
        public List<string> DelveAreas => _delveAreas;

        // Campaign areas
        private List<string> _campaignAreas;
        public List<string> CampaignAreas => _campaignAreas;

        // Abyss areas
        private List<string> _abyssAreas;


        public List<string> AbyssalAreas => _abyssAreas;

        // Vaal side areas
        private List<string> _vaalSideAreas;
        public List<string> VaalSideAreas => _vaalSideAreas;

        // Logbook areas
        private List<string> _logbookAreas;
        public List<string> LogbookAreas => _logbookAreas;

        // Logbook side areas
        private List<string> _logbookSideAreas;
        public List<string> LogbookSideAreas => _logbookSideAreas;

        // Lab trials
        private List<string> _labTrialAreas;
        public List<string> LabTrialAreas => _labTrialAreas;

        // Catarina fight areas
        private List<string> _catarinaFightAreas;
        public List<string> CatarinaFightAreas => _catarinaFightAreas;

        // Safhouse areas
        private List<string> _safehouseAreas;
        public List<string> SyndicateSafehouseAreas => _safehouseAreas;

        // Breachstones
        private List<string> _breachstoneAreas;
        public List<string> BreachstoneDomainAreas => _breachstoneAreas;

        // List with links to poe wiki
        private Dictionary<string, string> _wikiLinks;
        public Dictionary<string, string> WikiLinks => _wikiLinks;

        // Activity types with pause allowed
        private List<ACTIVITY_TYPES> _pausableActivityTypes;
        public List<ACTIVITY_TYPES> PausableActivityTypes => _pausableActivityTypes;

        // Searing Exarch Areas
        private List<string> _searingExarchAreas;
        public List<string> SearingExarchAreas => _searingExarchAreas;

        // Black Star Areas
        private List<string> _blackStarAreas;
        public List<string> BlackStarAreas => _blackStarAreas;

        // Infinite Hunger Areas
        private List<string> _infiniteHungerAreas;
        public List<string> InfiniteHungerAreas => _infiniteHungerAreas;

        // Eater of Worlds Areas
        private List<string> _eaterOfWorldsAreas;
        public List<string> EaterOfWorldsAreas => _eaterOfWorldsAreas;

        // Timeless Legion
        private List<string> _timelessLeagionAreas;
        public List<string> TimelessLegionAreas => _timelessLeagionAreas;

        // Kalandra
        private List<string> _lakeOfKalandraAreas;
        public List<string> LakeOfKalandraAreas => _lakeOfKalandraAreas;

        // Sanctum
        private List<string> _sanctumAreas;
        public List<string> SanctumAreas => _sanctumAreas;

        // TotA
        private List<string> _totaAreas;
        public List<string> TotAAreas => _totaAreas;

        // Settlers
        private List<string> _settlersAreas;
        public List<string> KingsmarchAreas => _settlersAreas;

        // TrialMaster
        private List<string> _trialMasterAreas;
        public List<string> TrialMasterAreas => _trialMasterAreas;

        // Tane
        private List<string> _taneAreas;
        public List<string> TaneAreas => _taneAreas;

        // Utlimatum
        private List<string> _ultimatumAreas;
        public List<string> UltimatumAreas => _ultimatumAreas;

        // Dread/Fear/Neglect Pinnacle Boses
        private List<string> _incarnationOfDreadAreas;
        public List<string> IncarnationOfDreadAreas => _incarnationOfDreadAreas;

        private List<string> _incarnationOfFearAreas;
        public List<string> IncarnationOfFearAreas => _incarnationOfFearAreas;

        private List<string> _incarnationOfNeglectAreas;
        public List<string> IncarnationOfNeglectAreas => _incarnationOfNeglectAreas;

        private List<string> _neglectedFlameAreas;
        public List<string> NeglectedFlameAreas => _neglectedFlameAreas;

        private List<string> _cardinalOfFearAreas;
        public List<string> CardinalOfFearAreas => _cardinalOfFearAreas;

        private List<string> _deceitfulGodAreas;
        public List<string> DeceitfulGodAreas => _deceitfulGodAreas;

        // King of Mists
        private List<string> _kingInTheMistsAreas;
        public List<string> KingInTheMistsAreas => _kingInTheMistsAreas;

        // Black Knight
        public List<string> _blackKnightAreas;
        public List<string> BlackKnightAreas => _blackKnightAreas;

        // Admiral Valerius
        public List<string> _admiralValeriusAreas;
        public List<string> AdmiralValeriusAreas => _admiralValeriusAreas;

        // Sasan
        public List<string> _sasanAreas;
        public List<string> SasanAreas => _sasanAreas;


        // All
        public List<string> AllAreas
        {
            get
            {
                List<string> lst = new List<string>();
                lst.AddRange(MapAreas);
                lst.AddRange(HeistAreas);
                lst.AddRange(BreachstoneDomainAreas);
                lst.AddRange(LogbookAreas);
                lst.AddRange(SimulacrumAreas);
                lst.AddRange(TimelessLegionAreas);
                lst.AddRange(EaterOfWorldsAreas);
                lst.AddRange(AbyssalAreas);
                lst.AddRange(AtziriAreas);
                lst.AddRange(BlackStarAreas);
                lst.AddRange(CampaignAreas);
                lst.AddRange(CatarinaFightAreas);
                lst.AddRange(SyndicateSafehouseAreas);
                lst.AddRange(DelveAreas);
                lst.AddRange(SirusAreas);
                lst.AddRange(ShaperAreas);
                lst.AddRange(ElderAreas);
                lst.AddRange(MavenFightAreas);
                lst.AddRange(MavenInvitationAreas);
                lst.AddRange(SearingExarchAreas);
                lst.AddRange(TempleAreas);
                lst.AddRange(UberAtziriAreas);
                lst.AddRange(VaalSideAreas);
                lst.AddRange(LabTrialAreas);
                lst.AddRange(SanctumAreas);
                lst.AddRange(TotAAreas);
                lst.AddRange(UltimatumAreas);
                lst.AddRange(KingsmarchAreas);
                lst.AddRange(IncarnationOfDreadAreas);
                lst.AddRange(IncarnationOfFearAreas);
                lst.AddRange(IncarnationOfNeglectAreas);
                lst.AddRange(KingInTheMistsAreas);
                lst.AddRange(BlackKnightAreas);
                lst.AddRange(AdmiralValeriusAreas);
                lst.AddRange(SasanAreas);
                lst.Sort();
                return lst;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public TrX_DefaultMappings()
        {
            _wikiLinks = new Dictionary<string, string>
            {
                {  "Elder", "https://pathofexile.fandom.com/wiki/The_Elder" },
            };

            _ultimatumAreas = new List<string>()
            {
                "The Utzaal Arena"
            };

            _taneAreas = new List<string>()
            {
                "Tanes Laboratory"
            };

            _lakeOfKalandraAreas = new List<string>()
            {
                "The Lake of Kalandra"
            };

            _trialMasterAreas = new List<string>()
            {
                "The Tower of Ordeals"
            };

            _breachstoneAreas = new List<string>
            {
                "Xophs Domain",
                "Tuls Domain",
                "Eshs Domain",
                "Uul-Netols Domain",
                "Chayulas Domain"
            };

            _mapAreas = new List<string>
            {
                "Academy",
                "Acid Caverns",
                "Alleyways",
                "Ancient City",
                "Arachnid Nest",
                "Arachnid Tomb",
                "Arcade",
                "Arena",
                "Arid Lake",
                "Armoury",
                "Arsenal",
                "Ashen Wood",
                "Atoll",
                "Barrows",
                "Basilica",
                "Bazaar",
                "Beach",
                "Belfry",
                "Bog",
                "Bone Crypt",
                "Bramble Valley",
                "Burial Chambers",
                "Cage",
                "Caldera",
                "Canyon",
                "Carcass",
                "Castle Ruins",
                "Cells",
                "Cemetery",
                "Channel",
                "Chateau",
                "City Square",
                "Cold River",
                "Colonnade",
                "Colosseum",
                "Conservatory",
                "Coral Ruins",
                "Core",
                "Courthouse",
                "Courtyard",
                "Coves",
                "Crater",
                "Crimson Temple",
                "Crimson Township",
                "Crystal Ore",
                "Cursed Crypt",
                "Dark Forest",
                "Defiled Cathedral",
                "Desert Spring",
                "Desert",
                "Dig",
                "Dry Sea",
                "Dunes",
                "Dungeon",
                "Estuary",
                "Excavation",
                "Factory",
                "Fields",
                "Flooded Mine",
                "Forbidden Woods",
                "Forge of the Phoenix",
                "Forking River",
                "Foundry",
                "Frozen Cabins",
                "Fungal Hollow",
                "Gardens",
                "Geode",
                "Ghetto",
                "Glacier",
                "Grave Trough",
                "Graveyard",
                "Grotto",
                "Haunted Mansion",
                "Iceberg",
                "Infested Valley",
                "Ivory Temple",
                "Jungle Valley",
                "Laboratory",
                "Lair of the Hydra",
                "Lair",
                "Lava Chamber",
                "Lava Lake",
                "Leyline",
                "Lighthouse",
                "Lookout",
                "Malformation",
                "Marshes",
                "Mausoleum",
                "Maze of the Minotaur",
                "Maze",
                "Mesa",
                "Mineral Pools",
                "Moon Temple",
                "Mud Geyser",
                "Museum",
                "Necropolis",
                "Orchard",
                "Overgrown Ruin",
                "Overgrown Shrine",
                "Palace",
                "Park",
                "Pen",
                "Peninsula",
                "Phantasmagoria",
                "Pier",
                "Pit of the Chimera",
                "Pit",
                "Plateau",
                "Plaza",
                "Port",
                "Precinct",
                "Primordial Blocks",
                "Primordial Pool",
                "Promenade",
                "Racecourse",
                "Ramparts",
                "Reef",
                "Relic Chambers",
                "Residence",
                "Scriptorium",
                "Sepulchre",
                "Shipyard",
                "Shore",
                "Shrine",
                "Siege",
                "Silo",
                "Spider Forest",
                "Spider Lair",
                "Stagnation",
                "Strand",
                "Sulphur Vents",
                "Summit",
                "Sunken City",
                "Temple",
                "Terrace",
                "The Beachhead",
                "Thicket",
                "Tower",
                "Toxic Sewer",
                "Tropical Island",
                "Underground River",
                "Underground Sea",
                "Vaal Pyramid",
                "Vaal Temple",
                "Vault",
                "Villa",
                "Volcano",
                "Waste Pool",
                "Wasteland",
                "Waterways",
                "Wharf",
                "Actons Nightmare",
                "Altered Distant Memory",
                "Augmented Distant Memory",
                "Caer Blaidd, Wolfpacks Den",
                "Cortex",
                "Death and Taxes",
                "Doryanis Machinarium",
                "Hall of Grandmasters",
                "Hallowed Ground",
                "Infused Beachhead",
                "Maelström of Chaos",
                "Mao Kun",
                "Obas Cursed Trove",
                "Olmecs Sanctum",
                "Pillars of Arun",
                "Poorjoys Asylum",
                "Replica Cortex",
                "Replica Pillars of Arun",
                "Replica Poorjoys Asylum",
                "Rewritten Distant Memory",
                "The Cowards Trial",
                "The Perandus Manor",
                "The Putrid Cloister",
                "The Twilight Temple",
                "The Vinktar Square",
                "Twisted Distant Memory",
                "Untainted Paradise",
                "Vaults of Atziri",
                "Whakawairua Tuahu",
                "Abomination",
                "Citadel",
                "Fortress",
                "Sanctuary",
                "Ziggurat"
            };

            _heistAreas = new List<string>
            {
                "Bunker",
                "Laboratory",
                "Mansion",
                "Prohibited Library",
                "Records Office",
                "Repository",
                "Smugglers Den",
                "Tunnels",
                "Underbelly",
            };

            _simuAreas = new List<string>
            {
               "Lunacys Watch",
               "The Bridge Enraptured",
               "The Syndrome Encampment",
               "Hysteriagate",
               "Oriath Delusion"
            };

            _campAreas = new List<string>
            {
               "Lioneyes Watch",
               "The Forest Encampment",
               "The Sarn Encampment",
               "Highgate",
               "Overseers Tower",
               "The Bridge Encampment",
               "Oriath",
               "Oriath Docks",
               "Karui Shores"
            };

            _sirusAreas = new List<string>
            {
                "Eye of the Storm"
            };

            _templeAreas = new List<string>
            {
                "The Temple of Atzoatl"
            };

            _atziriAreas = new List<string>
            {
                "The Apex of Sacrifice"
            };

            _uberAtziriAreas = new List<string>
            {
                "The Alluring Abyss"
            };

            _shaperAreas = new List<string>
            {
                "The Shapers Realm"
            };

            _elderAreas = new List<string>
            {
                "Absence of Value and Meaning"
            };

            _mavenFightAreas = new List<string>
            {
                "Absence of Mercy and Empathy"
            };

            _mavenInvitationAreas = new List<string>
            {
                "The Mavens Crucible"
            };

            _delveAreas = new List<string>
            {
                "Azurite Mine"
            };

            _labStartAreas = new List<string>
            {
                "Estate Path",
                "Estate Walkways",
                "Estate Crossing"
            };

            _campaignAreas = new List<string>
            {
                "The Twilight Strand",
                "The Coast",
                "The Tidal Island",
                "The Mud Flats",
                "The Fetid Pool",
                "The Submerged Passage",
                "The Flooded Depths",
                "The Ledge",
                "The Climb",
                "The Lower Prison",
                "The Upper Prison",
                "Prisoners Gate",
                "The Ship Graveyard",
                "The Cavern of Wrath",
                "The Ship Graveyard Cave",
                "The Cavern of Anger",
                "Lioneyes Watch",
                "The Southern Forest",
                "The Old Fields",
                "The Den",
                "The Crossroads",
                "The Chamber of Sins Level 1",
                "The Riverways",
                "The Fellshrine Ruins",
                "The Broken Bridge",
                "The Chamber of Sins Level 2",
                "The Crypt Level 1",
                "The Western Forest",
                "The Weavers Chambers",
                "The Crypt Level 2",
                "The Wetlands",
                "The Vaal Ruins",
                "The Dread Thicket",
                "The Northern Forest",
                "The Caverns",
                "The Ancient Pyramid",
                "The Forest Encampment",
                "The City of Sarn",
                "The Slums",
                "The Crematorium",
                "The Sewers",
                "The Marketplace",
                "The Catacombs",
                "The Battlefront",
                "The Solaris Temple Level 1",
                "The Solaris Temple Level 2",
                "The Ebony Barracks",
                "The Lunaris Temple Level 1",
                "The Docks",
                "The Lunaris Temple Level 2",
                "The Imperial Gardens",
                "The Library",
                "The Archives",
                "The Sceptre of God",
                "The Upper Sceptre of God",
                "The Sarn Encampment",
                "The Aqueduct",
                "The Dried Lake",
                "The Mines Level 1",
                "The Mines Level 2",
                "The Crystal Veins",
                "Kaoms Dream",
                "Daressos Dream",
                "Kaoms Stronghold",
                "The Grand Arena",
                "The Belly of the Beast Level 1",
                "The Belly of the Beast Level 2",
                "The Harvest",
                "The Ascent",
                "Highgate",
                "The Slave Pens",
                "The Control Blocks",
                "Oriath Square",
                "The Templar Courts",
                "The Chamber of Innocence",
                "The Ruined Square",
                "The Torched Courts",
                "The Ossuary",
                "The Reliquary",
                "The Cathedral Rooftop",
                "Overseers Tower",
                "The Twilight Strand",
                "The Coast",
                "The Tidal Island",
                "The Mud Flats",
                "The Karui Fortress",
                "The Ridge",
                "The Lower Prison",
                "Shavronnes Tower",
                "Prisoners Gate",
                "The Riverways",
                "The Wetlands",
                "The Western Forest",
                "The Southern Forest",
                "The Cavern of Anger",
                "The Beacon",
                "The Brine Kings Reef",
                "Lioneyes Watch",
                "The Broken Bridge",
                "The Crossroads",
                "The Fellshrine Ruins",
                "The Crypt",
                "The Chamber of Sins Level 1",
                "The Chamber of Sins Level 2",
                "Maligaros Sanctum",
                "The Den",
                "The Ashen Fields",
                "The Northern Forest",
                "The Dread Thicket",
                "The Causeway",
                "The Vaal City",
                "The Temple of Decay Level 1",
                "The Temple of Decay Level 2",
                "The Bridge Encampment",
                "The Sarn Ramparts",
                "The Toxic Conduits",
                "Doedres Cesspool",
                "The Grand Promenade",
                "The Bath House",
                "The Quay",
                "The Grain Gate",
                "The Imperial Fields",
                "The Solaris Concourse",
                "The High Gardens",
                "The Lunaris Concourse",
                "The Solaris Temple Level 1",
                "The Solaris Temple Level 2",
                "The Lunaris Temple Level 1",
                "The Lunaris Temple Level 2",
                "The Harbour Bridge",
                "The Sarn Encampment",
                "The Blood Aqueduct",
                "The Descent",
                "The Vastiri Desert",
                "The Oasis",
                "The Foothills",
                "The Boiling Lake",
                "The Tunnel",
                "The Belly of the Beast",
                "The Quarry",
                "The Refinery",
                "The Rotting Core",
                "Highgate",
                "The Cathedral Rooftop",
                "The Ravaged Square",
                "The Torched Courts",
                "The Desecrated Chambers",
                "The Canals",
                "The Control Blocks",
                "The Feeding Trough",
                "The Reliquary",
                "The Ossuary",
                "Oriath Docks"
            };

            _abyssAreas = new List<string>
            {
                "Abyssal Depths"
            };

            _sanctumAreas = new List<string>
            {
                "Sanctum Archives",
                "Sanctum Cathedral",
                "Sanctum Necropolis",
                "Sanctum Vaults",
                "The Forbidden Sanctum",
                "Sanctum Mausoleum"
            };

            _totaAreas = new List<string>
            {
                "The Halls of the Dead"
            };

            _settlersAreas = new List<string>
            {
                "Kingsmarch"
            };

            _vaalSideAreas = new List<string>
            {
                "Abandoned Dam",
                "Ancient Catacomb",
                "Arcane Chambers",
                "Blind Alley",
                "Clouded Ledge",
                "Clouded Ridge",
                "Concealed Caldarium",
                "Concealed Cavity",
                "Covered-up Hollow",
                "Cremated Archives",
                "Deathly Chambers",
                "Desolate Isle",
                "Desolate Track",
                "Disused Furnace",
                "Dusty Bluff",
                "Entombed Alcove",
                "Entombed Chamber",
                "Evacuated Quarter",
                "Flooded Complex",
                "Forbidden Archives",
                "Forbidden Chamber",
                "Forbidden Shrine",
                "Forgotten Conduit",
                "Forgotten Gulch",
                "Forgotten Oubliette",
                "Haunted Mineshaft",
                "Hidden Patch",
                "Inner Grounds",
                "Moonlit Chambers",
                "Mystical Clearing",
                "Narrow Ravine",
                "Neglected Cellar",
                "Quarantined Quarters",
                "Radiant Pools",
                "Reclaimed Barracks",
                "Remote Gulch",
                "Restricted Collection",
                "Restricted Gallery",
                "Sacred Chambers",
                "Sealed Basement",
                "Sealed Corridors",
                "Sealed Repository",
                "Secluded Canal",
                "Secluded Copse",
                "Secret Laboratory",
                "Shifting Sands",
                "Side Chapel",
                "Stagnant Canal",
                "Strange Sinkhole",
                "Sunken Shingle",
                "Twisted Inquisitorium",
                "Walled-off Ducts",
                "Isolated Sound",
                "Frozen Springs",
                "Suffocating Fissure",
            };

            _logbookAreas = new List<string>
            {
                "Battleground Graves",
                "Bluffs",
                "Cemetery",
                "Desert Ruins",
                "Dried Riverbed",
                "Forest Ruins",
                "Karui Wargraves",
                "Mountainside",
                "Rotting Temple",
                "Sarn Slums",
                "Scrublands",
                "Shipwreck Reef",
                "Utzaal Outskirts",
                "Vaal Temple",
                "Volcanic Island",
            };

            _logbookSideAreas = new List<string>
            {
                "Chittering Chamber",
                "Forgotten Grotto",
                "Fortified Redoubt",
                "Heroic Tomb",
                "Lost Sanctum",
                "Mushroom Thicket",
                "Noxious Gutter",
                "Sandy Vestige",
                "Spectral Hollow",
            };

            _labTrialAreas = new List<string>
            {
                "Trial of Piercing Truth",
                "Trial of Swirling Fear",
                "Trial of Crippling Grief",
                "Trial of Burning Rage",
                "Trial of Lingering Pain",
                "Trial of Stinging Doubt",
            };

            _catarinaFightAreas = new List<string>
            {
                "Masterminds Lair"
            };

            _safehouseAreas = new List<string>
            {
                "Syndicate Hideout"
            };

            _searingExarchAreas = new List<string>
            {
                "Absence of Patience and Wisdom"
            };

            _blackStarAreas = new List<string>
            {
                "Polaric Void"
            };

            _eaterOfWorldsAreas = new List<string>
            {
                "Absence of Symmetry and Harmony"
            };

            _infiniteHungerAreas = new List<string>
            {
                "Seething Chyme"
            };

            _timelessLeagionAreas = new List<string>
            {
                "Domain of Timeless Conflict"
            };

            _incarnationOfDreadAreas = new List<string>
            {
                "Moment of Reverence"
            };

            _incarnationOfFearAreas = new List<string>
            {
                "Moment of Trauma"
            };

            _incarnationOfNeglectAreas = new List<string>
            {
                "Moment of Loneliness"
            };

            _neglectedFlameAreas = new List<string>
            {
                "Courtyard of Wasting"
            };

            _deceitfulGodAreas = new List<string>
            {
                "Theatre of Lies"
            };

            _cardinalOfFearAreas = new List<string>
            {
                "Chambers of Impurity"
            };

            _kingInTheMistsAreas = new List<string>
            {
                "Crux of Nothingness"
            };

            _blackKnightAreas = new List<string>
            {
                "Starfall Crater"
            };

            _admiralValeriusAreas = new List<string>
            {
                "Sailors Folly"
            };

            _sasanAreas = new List<string>
            {
                "Abandoned Port"
            };

            _pausableActivityTypes = new List<ACTIVITY_TYPES>
            {
                ACTIVITY_TYPES.MAP,
                ACTIVITY_TYPES.HEIST,
                ACTIVITY_TYPES.SIMULACRUM,
                ACTIVITY_TYPES.MAVEN_FIGHT,
                ACTIVITY_TYPES.MAVEN_INVITATION,
                ACTIVITY_TYPES.SHAPER_FIGHT,
                ACTIVITY_TYPES.ELDER_FIGHT,
                ACTIVITY_TYPES.ATZIRI,
                ACTIVITY_TYPES.UBER_ATZIRI,
                ACTIVITY_TYPES.CATARINA_FIGHT,
                ACTIVITY_TYPES.LAB_TRIAL,
                ACTIVITY_TYPES.LOGBOOK,
                ACTIVITY_TYPES.LOGBOOK_SIDE,
                ACTIVITY_TYPES.SAFEHOUSE,
                ACTIVITY_TYPES.ABYSSAL_DEPTHS,
                ACTIVITY_TYPES.VAAL_SIDEAREA,
                ACTIVITY_TYPES.SEARING_EXARCH_FIGHT,
                ACTIVITY_TYPES.BLACK_STAR_FIGHT,
                ACTIVITY_TYPES.INFINITE_HUNGER_FIGHT,
                ACTIVITY_TYPES.EATER_OF_WORLDS_FIGHT,
                ACTIVITY_TYPES.TIMELESS_LEGION,
                ACTIVITY_TYPES.INSCRIBED_ULTIMATUM,
                ACTIVITY_TYPES.INCARNATION_OF_DREAD_FIGHT,
                ACTIVITY_TYPES.INCARNATION_OF_FEAR_FIGHT,
                ACTIVITY_TYPES.INCARNATION_OF_NEGLECT_FIGHT,
                ACTIVITY_TYPES.NEGLECTED_FLAME_FIGHT,
                ACTIVITY_TYPES.DECEITFUL_GOD_FIGHT,
                ACTIVITY_TYPES.CARDINAL_OF_FEAR_FIGHT,
                ACTIVITY_TYPES.KING_IN_THE_MISTS_FIGHT,
                ACTIVITY_TYPES.BLACK_KNIGHT_FIGHT,
                ACTIVITY_TYPES.ADMIRAL_VALERIUS_FIGHT,
                ACTIVITY_TYPES.SASAN_FIGHT
            };

            // Map areas with enabled death counter
            _deathCountEnabledAreas = new List<string>();
            _deathCountEnabledAreas.AddRange(SimulacrumAreas);
            _deathCountEnabledAreas.AddRange(MapAreas);
            _deathCountEnabledAreas.AddRange(HeistAreas);
            _deathCountEnabledAreas.AddRange(SirusAreas);
            _deathCountEnabledAreas.AddRange(TempleAreas);
            _deathCountEnabledAreas.AddRange(AtziriAreas);
            _deathCountEnabledAreas.AddRange(ShaperAreas);
            _deathCountEnabledAreas.AddRange(ElderAreas);
            _deathCountEnabledAreas.AddRange(UberAtziriAreas);
            _deathCountEnabledAreas.AddRange(MavenInvitationAreas);
            _deathCountEnabledAreas.AddRange(MavenFightAreas);
            _deathCountEnabledAreas.AddRange(DelveAreas);
            _deathCountEnabledAreas.AddRange(CampaignAreas);
            _deathCountEnabledAreas.AddRange(LogbookAreas);
            _deathCountEnabledAreas.AddRange(LogbookSideAreas);
            _deathCountEnabledAreas.AddRange(LabTrialAreas);
            _deathCountEnabledAreas.AddRange(VaalSideAreas);
            _deathCountEnabledAreas.AddRange(AbyssalAreas);
            _deathCountEnabledAreas.AddRange(CatarinaFightAreas);
            _deathCountEnabledAreas.AddRange(SyndicateSafehouseAreas);
            _deathCountEnabledAreas.AddRange(BreachstoneDomainAreas);
            _deathCountEnabledAreas.AddRange(SearingExarchAreas);
            _deathCountEnabledAreas.AddRange(BlackStarAreas);
            _deathCountEnabledAreas.AddRange(EaterOfWorldsAreas);
            _deathCountEnabledAreas.AddRange(InfiniteHungerAreas);
            _deathCountEnabledAreas.AddRange(TimelessLegionAreas);
            _deathCountEnabledAreas.AddRange(SanctumAreas);
            _deathCountEnabledAreas.AddRange(TrialMasterAreas);
            _deathCountEnabledAreas.AddRange(UltimatumAreas);
            _deathCountEnabledAreas.AddRange(IncarnationOfDreadAreas);
            _deathCountEnabledAreas.AddRange(IncarnationOfFearAreas);
            _deathCountEnabledAreas.AddRange(IncarnationOfNeglectAreas);
            _deathCountEnabledAreas.AddRange(NeglectedFlameAreas);
            _deathCountEnabledAreas.AddRange(DeceitfulGodAreas);
            _deathCountEnabledAreas.AddRange(CardinalOfFearAreas);
            _deathCountEnabledAreas.AddRange(KingInTheMistsAreas);
            _deathCountEnabledAreas.AddRange(BlackKnightAreas);
            _deathCountEnabledAreas.AddRange(AdmiralValeriusAreas);
            _deathCountEnabledAreas.AddRange(SasanAreas);
        }
    }
}
using System.Collections.Generic;

namespace TraXile
{
    class TrX_DefaultMappings
    {
        public List<string> MAP_AREAS,
            HEIST_AREAS,
            SIMU_AREAS,
            CAMP_AREAS,
            SIRUS_AREAS,
            ATZIRI_AREAS,
            UBER_ATZIRI_AREAS,
            ELDER_AREAS,
            SHAPER_AREAS,
            TEMPLE_AREAS,
            DEATH_COUNT_ENABLED_AREAS,
            MAVEN_FIGHT_AREAS,
            MAVEN_INV_AREAS,
            LAB_START_AREAS,
            DELVE_AREAS,
            CAMPAIGN_AREAS;

        public Dictionary<string, string> WIKI_LINKS;
        public List<ACTIVITY_TYPES> PAUSABLE_ACTIVITY_TYPES;

        public TrX_DefaultMappings()
        {
            WIKI_LINKS = new Dictionary<string, string>
            {
                {  "Elder", "https://pathofexile.fandom.com/wiki/The_Elder" },
            };

            MAP_AREAS = new List<string>
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
                "The Beachhead",
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
                "The Beachhead",
                "The Cowards Trial",
                "The Perandus Manor",
                "The Putrid Cloister",
                "The Twilight Temple",
                "The Vinktar Square",
                "Twisted Distant Memory",
                "Untainted Paradise",
                "Vaults of Atziri",
                "Whakawairua Tuahu",
            };

            HEIST_AREAS = new List<string>
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

            SIMU_AREAS = new List<string>
            {
               "Lunacys Watch",
               "The Bridge Enraptured",
               "The Syndrome Encampment",
               "Hysteriagate",
               "Oriath Delusion"
            };

            CAMP_AREAS = new List<string>
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

            SIRUS_AREAS = new List<string>
            {
                "Eye of the Storm"
            };

            TEMPLE_AREAS = new List<string>
            {
                "The Temple of Atzoatl"
            };

            ATZIRI_AREAS = new List<string>
            {
                "The Apex of Sacrifice"
            };

            UBER_ATZIRI_AREAS = new List<string>
            {
                "The Alluring Abyss"
            };

            SHAPER_AREAS = new List<string>
            {
                "The Shapers Realm"
            };

            ELDER_AREAS = new List<string>
            {
                "Absence of Value and Meaning"
            };

            MAVEN_FIGHT_AREAS = new List<string>
            {
                "Absence of Mercy and Empathy"
            };

            MAVEN_INV_AREAS = new List<string>
            {
                "The Mavens Crucible"
            };

            DELVE_AREAS = new List<string>
            {
                "Azurite Mine"
            };

            LAB_START_AREAS = new List<string>
            {
                "Estate Path",
                "Estate Walkways",
                "Estate Crossing"
            };

            CAMPAIGN_AREAS = new List<string>
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
                "Oriath Docks",
            };


            PAUSABLE_ACTIVITY_TYPES = new List<ACTIVITY_TYPES>
            {
                ACTIVITY_TYPES.MAP,
                ACTIVITY_TYPES.HEIST,
                ACTIVITY_TYPES.SIMULACRUM,
                ACTIVITY_TYPES.MAVEN_FIGHT,
                ACTIVITY_TYPES.MAVEN_INVITATION,
                ACTIVITY_TYPES.SHAPER_FIGHT,
                ACTIVITY_TYPES.ELDER_FIGHT,
                ACTIVITY_TYPES.ATZIRI,
                ACTIVITY_TYPES.UBER_ATZIRI
            };

            // Map areas with enabled death counter
            DEATH_COUNT_ENABLED_AREAS = new List<string>();
            DEATH_COUNT_ENABLED_AREAS.AddRange(SIMU_AREAS);
            DEATH_COUNT_ENABLED_AREAS.AddRange(MAP_AREAS);
            DEATH_COUNT_ENABLED_AREAS.AddRange(HEIST_AREAS);
            DEATH_COUNT_ENABLED_AREAS.AddRange(SIRUS_AREAS);
            DEATH_COUNT_ENABLED_AREAS.AddRange(TEMPLE_AREAS);
            DEATH_COUNT_ENABLED_AREAS.AddRange(ATZIRI_AREAS);
            DEATH_COUNT_ENABLED_AREAS.AddRange(SHAPER_AREAS);
            DEATH_COUNT_ENABLED_AREAS.AddRange(ELDER_AREAS);
            DEATH_COUNT_ENABLED_AREAS.AddRange(UBER_ATZIRI_AREAS);
            DEATH_COUNT_ENABLED_AREAS.AddRange(MAVEN_INV_AREAS);
            DEATH_COUNT_ENABLED_AREAS.AddRange(MAVEN_FIGHT_AREAS);
            DEATH_COUNT_ENABLED_AREAS.AddRange(DELVE_AREAS);
            DEATH_COUNT_ENABLED_AREAS.AddRange(CAMPAIGN_AREAS);
        }
    }
}


/*
  bool bTargetAreaMine = sTargetArea == "Azurite Mine";
            bool bTargetAreaTemple = sTargetArea == "The Temple of Atzoatl";
            bool bTargetAreaIsLab = sTargetArea == "Estate Path" || sTargetArea == "Estate Walkways" || sTargetArea == "Estate Crossing";
            bool bTargetAreaIsMI = sTargetArea == "The Mavens Crucible";
            bool bTargetAreaIsAtziri = sTargetArea == "The Apex of Sacrifice";
            bool bTargetAreaIsUberAtziri = sTargetArea == "The Alluring Abyss";
            bool bTargetAreaIsElder = sTargetArea == "Absence of Value and Meaning";
            bool bTargetAreaIsShaper = sTargetArea == "The Shapers Realm";
            bool bTargetAreaIsSirusFight = sTargetArea == "Eye of the Storm";
            bool bTargetAreaIsMavenFight = sTargetArea == "Absence of Mercy and Empathy";
*/
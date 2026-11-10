namespace TraXile
{
    /// <summary>
    /// Types of tracked activities (moved here from the classic Main form when the WinForms UI was removed).
    /// The names are persisted, e.g. in config.xml (TimeCapMap); do not rename members.
    /// </summary>
    public enum ACTIVITY_TYPES
    {
        MAP,
        HEIST,
        LABYRINTH,
        SIMULACRUM,
        DELVE,
        TEMPLE,
        MAVEN_INVITATION,
        ATZIRI,
        UBER_ATZIRI,
        ELDER_FIGHT,
        SHAPER_FIGHT,
        MAVEN_FIGHT,
        SIRUS_FIGHT,
        OTHER,
        CAMPAIGN,
        ABYSSAL_DEPTHS,
        VAAL_SIDEAREA,
        LAB_TRIAL,
        LOGBOOK,
        SAFEHOUSE,
        CATARINA_FIGHT,
        LOGBOOK_SIDE,
        BREACHSTONE,
        SEARING_EXARCH_FIGHT,
        BLACK_STAR_FIGHT,
        INFINITE_HUNGER_FIGHT,
        EATER_OF_WORLDS_FIGHT,
        TIMELESS_LEGION,
        LAKE_OF_KALANDRA,
        SANCTUM,
        TRIALMASTER_FIGHT,
        TANES_LABORATORY,
        ANCESTOR_TRIAL,
        INSCRIBED_ULTIMATUM,
        KINGSMARCH,
        INCARNATION_OF_DREAD_FIGHT, // Dread
        INCARNATION_OF_FEAR_FIGHT, // Fear
        INCARNATION_OF_NEGLECT_FIGHT, // Neglect
        NEGLECTED_FLAME_FIGHT,
        CARDINAL_OF_FEAR_FIGHT,
        DECEITFUL_GOD_FIGHT,
        KING_IN_THE_MISTS_FIGHT,
        BLACK_KNIGHT_FIGHT,
        ADMIRAL_VALERIUS_FIGHT,
        SASAN_FIGHT,
        OSHABI_FIGHT,
        RELIQUARY_KEY,
        ANOMALY
    }
}

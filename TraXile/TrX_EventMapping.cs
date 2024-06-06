﻿using System.Collections.Generic;

namespace TraXile
{
    public enum EVENT_TYPES
    {
        APP_STARTED,
        APP_READY,
        ENTERED_AREA,
        PLAYER_DIED,
        DEATH_REASON_RECEIVED,
        INSTANCE_CONNECTED,
        TRIALMASTER_SPEECH,
        TRIALMASTER_ROUND_STARTED,
        EINHAR_BEAST_CAPTURE,
        TRIALMASTER_TOOK_REWARD,
        TRIALMASTER_VICTORY,
        PARTYMEMBER_ENTERED_AREA,
        PARTYMEMBER_LEFT_AREA,
        DELIRIUM_ENCOUNTER,
        BLIGHT_ENCOUNTER,
        INCURSION_ENCOUNTER,
        NIKO_ENCOUNTER,
        EINHAR_ENCOUNTER,
        ZANA_ENCOUNTER,
        SYNDICATE_ENCOUNTER,
        LEVELUP,
        SIMULACRUM_FULLCLEAR,
        CHAT_CMD_RECEIVED,
        LAB_FINISHED,
        LAB_START_INFO_RECEIVED,
        NEXT_AREA_LEVEL_RECEIVED,
        POE_CLIENT_START,
        TUJEN_ENCOUNTER,
        EXP_TUJEN_ENCOUNTER,
        EXP_ROG_ENCOUNTER,
        EXP_GWENNEN_ENCOUNTER,
        EXP_DANNIG_ENCOUNTER,
        HEIST_TULLINA_SPEAK,
        HEIST_TIBBS_SPEAK,
        HEIST_ISLA_SPEAK,
        HEIST_NILES_SPEAK,
        HEIST_NENET_SPEAK,
        HEIST_VINDERI_SPEAK,
        HEIST_GIANNA_SPEAK,
        HEIST_HUCK_SPEAK,
        HEIST_KARST_SPEAK,
        ABNORMAL_DISCONNECT,
        CAMPAIGN_FINISHED,
        NEXT_CEMETERY_IS_LOGBOOK,
        PLAYER_SUICIDE,
        TWICE_BLESSED,
        HARVEST,
        SANCTUM_LYCIA_1_KILLED,
        SANCTUM_LYCIA_2_KILLED,
        ANCESTOR_TOURNAMENT_WON,
        ANCESTOR_TOURNAMENT_LOST,
        ANCESTOR_MATCH_WON,
        ANCESTOR_MATCH_LOST,
        TRIALMASTER_ENCOUNTERED,
        TRIALMASTER_PLAYER_LOSS
    }

    public class TrX_EventMapping
    {
        // Mapping dictionary
        private Dictionary<string, EVENT_TYPES> _mapping;
        public Dictionary<string, EVENT_TYPES> Mapping => _mapping;

        /// <summary>
        /// Constructor
        /// </summary>
        public TrX_EventMapping()
        {
            _mapping = new Dictionary<string, EVENT_TYPES>
            {
                { "You have entered", EVENT_TYPES.ENTERED_AREA },
               
                // Startup
                {"***** LOG FILE OPENING *****", EVENT_TYPES.POE_CLIENT_START },

                // System Commands
                { "trax::", EVENT_TYPES.CHAT_CMD_RECEIVED },

                // Generic events
                { "has joined the area", EVENT_TYPES.PARTYMEMBER_ENTERED_AREA },
                { "has left the area", EVENT_TYPES.PARTYMEMBER_LEFT_AREA },
                { "Player died", EVENT_TYPES.DEATH_REASON_RECEIVED },
                { "has been slain", EVENT_TYPES.PLAYER_DIED },
                { "has committed suicide", EVENT_TYPES.PLAYER_SUICIDE },
                { "Connecting to instance server at", EVENT_TYPES.INSTANCE_CONNECTED },
                { " is now level ", EVENT_TYPES.LEVELUP },
                { "] Oshabi: ", EVENT_TYPES.HARVEST },

                //Einhar
                { "Great job, Exile! Einhar will take the captured beast to the Menagerie.", EVENT_TYPES.EINHAR_BEAST_CAPTURE },
                { "The First Ones look upon this capture with pride, Exile. You hunt well.", EVENT_TYPES.EINHAR_BEAST_CAPTURE },
                { "Survivor! You are well prepared for the end. This is a fine capture.", EVENT_TYPES.EINHAR_BEAST_CAPTURE },
                { "Haha! You are captured, stupid beast.", EVENT_TYPES.EINHAR_BEAST_CAPTURE },
                { "You have been captured, beast. You will be a survivor, or you will be food.", EVENT_TYPES.EINHAR_BEAST_CAPTURE },
                { "This one is captured. Einhar will take it.", EVENT_TYPES.EINHAR_BEAST_CAPTURE },

                // Ultimatum
                { "] The Trialmaster: A battlefield chilled by winter's hate.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: A curious choice. Chaos disdains predictability.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: A festering ring encroaches upon you.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: Ah, a classic! But far fairer now than in ancient times.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: A hateful grip will steal your breath.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: A horde of hungry beasts.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: All the colours of death.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: An arrow falls, and I know where.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: An unstoppable tide.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: A prism of pain.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: A quicker resistance.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: A ruinous choice.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: A sea of fire crashes upon your shores.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: A single touch may spell your ruin.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: A sliver of the Winter of the World returns.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: Avoid the miasma, if you can.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: A wall arises between you and victory.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: A weight upon the soul.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: Be careful how quickly you slay...", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: Be diminished.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: Begin the conquest!", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: Best be quick...", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: Best run.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: Be torn to shreds.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: Beware the bite of the chained beast.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: Blood is the source of life... and death.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: Bloodthirsty princes surround you, hungry for the throne.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: Can you feel death coiling around your heart?", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: Choking vapours seek the living.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: Cling to life.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: Confront that which you release.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: Conquer all!", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: Contamination moves swiftly on the wind.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: Corruption starts in the blood.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: Cut straight to the heart of the matter.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: Dance neither near nor far.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: Death is already creeping upon you.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: Death is swift.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: Death is swift... and cold.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: Death's cold grip is inescapable.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: Death spreads within your veins.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: Defend what is yours.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: Destroyer, begin!", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: Destroy everything.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: Destroy that which you seek.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: Ear-shattering bolts!", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: Everywhere your gaze falls, there shall be only ruin.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: Faith and intellect burn alike.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: Fight the slowing of your own heart.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: Fire always spreads.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: Fire seeks what it will.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: Flames beget more flames.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: Frailty races through your bones.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: From death, hate grasps for all life.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: Go forth and destroy.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: Hate begets hate.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: Heart and spirit, burdened.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: Hold your ground... if you can.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: Hunger begets savagery.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: Let none stop you.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: Lightning always strikes twice.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: Lightning descends.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: Lightning gives little warning.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: Lightning is swifter than the eye.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: Lightning lurks within you.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: Live.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: Make your stand.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: Miasma spreads.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: Mortality seeks you in kind.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: Mystic pain will bring ruin.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: Nature hungers at the fringes.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: Never enough time.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: Night's madness drains away all hope.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: No defence will be enough.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: No rest for the wicked.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: Outlive my expectations.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: Quick jaws make quick work of prey.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: Rare is the breath of living air.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: Really? Nobody ever—alright. It's your funeral.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: Relentless assault.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: Ruin arrives suddenly.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: Ruin hides in every corner.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: Ruin is all around you.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: Ruin reaches for you from afar.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: Ruin's burden weighs twice as heavy.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: Ruin seeks you.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: Ruin surrounds you.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: Seriously—don't get hit.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: Shrapnel and shards abound.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: Slay them amongst the flames.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: Speed upon speed.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: Spring's thunder. Wrath itself.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: Steel and pain.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: Stem the tide.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: Step quickly lest the detonations end you.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: Stray too far and find pain.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: Take what is yours.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: The bane of mystics and scholars.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: The blood-soaked ground turns against you.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: The blood sours.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: The dance of death is swift and turbid.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: The dead reach out with an icy grip.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: The fire of summer burns within.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: The flames bite upon your heels.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: The flames race toward you.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: The horde adapts.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: The hungry beasts are numerous.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: The paper warrior weaves armour out of reeds.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: The riddle of steel has no answer.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: The storm approaches.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: The storm comes swiftly.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: The sun is setting on your success.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: The tempest is nigh.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: The tide of death is rapid.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: The vapours are inescapable.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: The wind conspires with the flames.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: They accelerate beyond reason.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: They move with utmost haste.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: They seek to surprise you.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: Those you slay will seek their vengeance.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: Thunder and lightning abound.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: Time is your enemy.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: To shreds.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: Try not to die.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: Unpredictability adds that much more spice.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: Vengeance is cold indeed.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: Victory requires a keen eye.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: We might just be here all day.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: You are more frail than you know.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: You must be a glutton for punishment.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: Your adversaries bring spring's thunder.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: Your arteries constrict.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: Your blood betrays you.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: Your blood weakens.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: Your doom is dark and roiling.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: Your enemies hasten.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: Your epithets, echoed.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: Your foes bring summer's flame.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: Your heart already beats more slowly.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: Your opponents bring night's madness.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: Your opponents bring winter's hate.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: Your own volleys turn against you.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: Your reach exceeds your grasp.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: Your thoughts crackle with pain.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: Your world will fall to ruin.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: You shall suffer your own bile.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },
                { "] The Trialmaster: You will fall to ruin.", EVENT_TYPES.TRIALMASTER_ROUND_STARTED },

                { "] The Trialmaster: The safe choice.", EVENT_TYPES.TRIALMASTER_TOOK_REWARD },
                { "] The Trialmaster: The expected choice.", EVENT_TYPES.TRIALMASTER_TOOK_REWARD },
                { "] The Trialmaster: So be it, as disappointing as it is.", EVENT_TYPES.TRIALMASTER_TOOK_REWARD },

                { "] The Trialmaster: Honestly, it's about time. Congratulations, challenger. Sincerely.", EVENT_TYPES.TRIALMASTER_VICTORY },
                { "] The Trialmaster: You... you won? I honestly didn't expect that of you.", EVENT_TYPES.TRIALMASTER_VICTORY },
                { "] The Trialmaster: Thank you for not disappointing my master yet again.", EVENT_TYPES.TRIALMASTER_VICTORY },
                { "] The Trialmaster: Your series of losses has finally ended.", EVENT_TYPES.TRIALMASTER_VICTORY },
                { "] The Trialmaster: I was not certain you had it in you.", EVENT_TYPES.TRIALMASTER_VICTORY },
                { "] The Trialmaster: Fair enough. Luck is luck.", EVENT_TYPES.TRIALMASTER_VICTORY },
                { "] The Trialmaster: You have restored your pride.", EVENT_TYPES.TRIALMASTER_VICTORY },
                { "] The Trialmaster: Win some... lose most.", EVENT_TYPES.TRIALMASTER_VICTORY },
                { "] The Trialmaster: You have stemmed the tide of losses.", EVENT_TYPES.TRIALMASTER_VICTORY },
                { "] The Trialmaster: You have redeemed yourself.", EVENT_TYPES.TRIALMASTER_VICTORY },
                { "] The Trialmaster: Thus, the vagaries of chance.", EVENT_TYPES.TRIALMASTER_VICTORY },
                { "] The Trialmaster: Keeping it interesting, I see.", EVENT_TYPES.TRIALMASTER_VICTORY },
                { "] The Trialmaster: A meagre turnaround.", EVENT_TYPES.TRIALMASTER_VICTORY },
                { "] The Trialmaster: That makes two in a row!", EVENT_TYPES.TRIALMASTER_VICTORY },
                { "] The Trialmaster: A second victory!", EVENT_TYPES.TRIALMASTER_VICTORY },
                { "] The Trialmaster: I can practically see your ego swelling.", EVENT_TYPES.TRIALMASTER_VICTORY },
                { "] The Trialmaster: Don't get too eager.", EVENT_TYPES.TRIALMASTER_VICTORY },
                { "] The Trialmaster: Impressive, challenger.", EVENT_TYPES.TRIALMASTER_VICTORY },
                { "] The Trialmaster: Your series of victories continues.", EVENT_TYPES.TRIALMASTER_VICTORY },
                { "] The Trialmaster: Luck can only carry you so far....", EVENT_TYPES.TRIALMASTER_VICTORY },
                { "] The Trialmaster: For now, the victor remains the victor!", EVENT_TYPES.TRIALMASTER_VICTORY },
                { "] The Trialmaster: Not bad... for a mortal.", EVENT_TYPES.TRIALMASTER_VICTORY },
                { "] The Trialmaster: This will not continue. I am unconcerned.", EVENT_TYPES.TRIALMASTER_VICTORY },
                { "] The Trialmaster: This is no longer amusing.", EVENT_TYPES.TRIALMASTER_VICTORY },
                { "] The Trialmaster: This series of victories is astonishing.", EVENT_TYPES.TRIALMASTER_VICTORY },
                { "] The Trialmaster: How do you keep winning?!", EVENT_TYPES.TRIALMASTER_VICTORY },
                { "] The Trialmaster: I suspect I am being made a fool of!", EVENT_TYPES.TRIALMASTER_VICTORY },
                { "] The Trialmaster: I should have become a priest of Yaomac instead...", EVENT_TYPES.TRIALMASTER_VICTORY },
                { "] The Trialmaster: You win... again.", EVENT_TYPES.TRIALMASTER_VICTORY },
                { "] The Trialmaster: This is insufferable...", EVENT_TYPES.TRIALMASTER_VICTORY },
                { "] The Trialmaster: Take your prize and go.", EVENT_TYPES.TRIALMASTER_VICTORY },

                { "] The Trialmaster: Your potential remains unrealised.", EVENT_TYPES.TRIALMASTER_PLAYER_LOSS },
                { "] The Trialmaster: You are wasting my time, fool.", EVENT_TYPES.TRIALMASTER_PLAYER_LOSS },
                { "] The Trialmaster: How... expected.", EVENT_TYPES.TRIALMASTER_PLAYER_LOSS },
                { "] The Trialmaster: Yet again...", EVENT_TYPES.TRIALMASTER_PLAYER_LOSS },
                { "] The Trialmaster: You just keep failing!", EVENT_TYPES.TRIALMASTER_PLAYER_LOSS },
                { "] The Trialmaster: How embarrassing for you.", EVENT_TYPES.TRIALMASTER_PLAYER_LOSS },
                { "] The Trialmaster: Perhaps these trials are too difficult for you.", EVENT_TYPES.TRIALMASTER_PLAYER_LOSS },
                { "] The Trialmaster: Perhaps you should hone your skills before you attempt my trials again.", EVENT_TYPES.TRIALMASTER_PLAYER_LOSS },
                { "] The Trialmaster: Your record worsens...", EVENT_TYPES.TRIALMASTER_PLAYER_LOSS },
                { "] The Trialmaster: Now you understand what not to do.", EVENT_TYPES.TRIALMASTER_PLAYER_LOSS },
                { "] The Trialmaster: Another failure...", EVENT_TYPES.TRIALMASTER_PLAYER_LOSS },
                { "] The Trialmaster: To lose twice in a row... how unfortunate.", EVENT_TYPES.TRIALMASTER_PLAYER_LOSS },
                { "] The Trialmaster: Two losses in a row!", EVENT_TYPES.TRIALMASTER_PLAYER_LOSS },
                { "] The Trialmaster: Perhaps you don't understand the rules...", EVENT_TYPES.TRIALMASTER_PLAYER_LOSS },
                { "] The Trialmaster: A second victory in a row was not in the cards for you, it seems.", EVENT_TYPES.TRIALMASTER_PLAYER_LOSS },
                { "] The Trialmaster: Try your luck again soon, challenger.", EVENT_TYPES.TRIALMASTER_PLAYER_LOSS },
                { "] The Trialmaster: Be not ashamed at your meagre record.", EVENT_TYPES.TRIALMASTER_PLAYER_LOSS },
                { "] The Trialmaster: It was not to be.", EVENT_TYPES.TRIALMASTER_PLAYER_LOSS },
                { "] The Trialmaster: Your series of victories has come to an end.", EVENT_TYPES.TRIALMASTER_PLAYER_LOSS },
                { "] The Trialmaster: The victorious always fall eventually.", EVENT_TYPES.TRIALMASTER_PLAYER_LOSS },
                { "] The Trialmaster: Your grand visions fade to dust.", EVENT_TYPES.TRIALMASTER_PLAYER_LOSS },
                { "] The Trialmaster: It seems we have found the limit of your skill.", EVENT_TYPES.TRIALMASTER_PLAYER_LOSS },
                { "] The Trialmaster: Chance no longer favours you.", EVENT_TYPES.TRIALMASTER_PLAYER_LOSS },
                { "] The Trialmaster: It was inevitable.", EVENT_TYPES.TRIALMASTER_PLAYER_LOSS },
                { "] The Trialmaster: Your path to greatness has taken a hard turn toward a cliff.", EVENT_TYPES.TRIALMASTER_PLAYER_LOSS },
                { "] The Trialmaster: There is no shame in defeat. Well... there is some.", EVENT_TYPES.TRIALMASTER_PLAYER_LOSS },
                { "] The Trialmaster: You emerge victorious no more.", EVENT_TYPES.TRIALMASTER_PLAYER_LOSS },
                { "] The Trialmaster: Your series of victories has reached its end.", EVENT_TYPES.TRIALMASTER_PLAYER_LOSS },
                { "] The Trialmaster: Hahahahaha! Your luck is over, fool!", EVENT_TYPES.TRIALMASTER_PLAYER_LOSS },
                { "] The Trialmaster: My master, it is done! Torture me no longer!", EVENT_TYPES.TRIALMASTER_PLAYER_LOSS },
                
                { "] The Trialmaster: ", EVENT_TYPES.TRIALMASTER_ENCOUNTERED },

                {"Strange Voice: So be it.", EVENT_TYPES.SIMULACRUM_FULLCLEAR },

                // Encounters
                {"Strange Voice: ", EVENT_TYPES.DELIRIUM_ENCOUNTER },
                {"Sister Cassia: ", EVENT_TYPES.BLIGHT_ENCOUNTER },
                {"Niko, Master of the Depths: ", EVENT_TYPES.NIKO_ENCOUNTER },
                {"Alva, Master Explorer: ", EVENT_TYPES.INCURSION_ENCOUNTER },
                {"Einhar, Beastmaster: ", EVENT_TYPES.EINHAR_ENCOUNTER },
                {"Zana, Master Cartographer: Still sane, exile?", EVENT_TYPES.ZANA_ENCOUNTER },
                {"Aisling Laffrey, The Silent Butcher", EVENT_TYPES.SYNDICATE_ENCOUNTER },
                {"Elreon, Light's Judge", EVENT_TYPES.SYNDICATE_ENCOUNTER },
                {"Hillock, the Blacksmith", EVENT_TYPES.SYNDICATE_ENCOUNTER },
                {"Gravicius Reborn", EVENT_TYPES.SYNDICATE_ENCOUNTER },
                {"Guff \"Tiny\" Grenn", EVENT_TYPES.SYNDICATE_ENCOUNTER },
                {"Haku, Warmaster", EVENT_TYPES.SYNDICATE_ENCOUNTER },
                {"It That Fled", EVENT_TYPES.SYNDICATE_ENCOUNTER },
                {"Janus Perandus", EVENT_TYPES.SYNDICATE_ENCOUNTER },
                {"Korell Goya, Son of Stone", EVENT_TYPES.SYNDICATE_ENCOUNTER },
                {"Leo, Wolf of the Pits", EVENT_TYPES.SYNDICATE_ENCOUNTER },
                {"Riker Maloney, Midnight Tinkerer", EVENT_TYPES.SYNDICATE_ENCOUNTER },
                {"Rin Yuushu", EVENT_TYPES.SYNDICATE_ENCOUNTER },
                {"Thane Jorgin the Banished", EVENT_TYPES.SYNDICATE_ENCOUNTER },
                {"Tora, the Culler", EVENT_TYPES.SYNDICATE_ENCOUNTER },
                {"Vorici, Silent Brother", EVENT_TYPES.SYNDICATE_ENCOUNTER },
                {"Vagan, Victory's Herald", EVENT_TYPES.SYNDICATE_ENCOUNTER },

                // Lab
                { "Izaro: Triumphant at last!", EVENT_TYPES.LAB_FINISHED },
                { "Izaro: You are free!", EVENT_TYPES.LAB_FINISHED },
                { "Izaro: I die for the Empire!", EVENT_TYPES.LAB_FINISHED },
                { "Izaro: The trap of tyranny is inescapable.", EVENT_TYPES.LAB_FINISHED },
                { "Izaro: Delight in your gilded dungeon, ascendant.", EVENT_TYPES.LAB_FINISHED },
                { "Izaro: Your destination is more dangerous than the journey, ascendant.", EVENT_TYPES.LAB_FINISHED },
                { "] Generating level ", EVENT_TYPES.NEXT_AREA_LEVEL_RECEIVED },
                { "] : Be twice blessed.", EVENT_TYPES.TWICE_BLESSED },
                { "] : This chest is locked. A Treasure Key is required to open it.", EVENT_TYPES.LAB_FINISHED },

                // Expedition
                { "] Tujen, the Haggler:", EVENT_TYPES.EXP_TUJEN_ENCOUNTER },
                { "] Tujen:", EVENT_TYPES.EXP_TUJEN_ENCOUNTER },
                { "] Gwennen, the Gambler:", EVENT_TYPES.EXP_GWENNEN_ENCOUNTER },
                { "] Gwennen:", EVENT_TYPES.EXP_GWENNEN_ENCOUNTER },
                { "] Rog:", EVENT_TYPES.EXP_ROG_ENCOUNTER },
                { "] Dannig, Warrior Skald:", EVENT_TYPES.EXP_DANNIG_ENCOUNTER },
                { "] Dannig:", EVENT_TYPES.EXP_DANNIG_ENCOUNTER },

                // Heist
                { "] Karst, the Lockpick: ", EVENT_TYPES.HEIST_KARST_SPEAK },
                { "] Tullina, the Catburglar: ", EVENT_TYPES.HEIST_TULLINA_SPEAK },
                { "] Tibbs, the Giant: ", EVENT_TYPES.HEIST_TIBBS_SPEAK },
                { "] Isla, the Engineer: ", EVENT_TYPES.HEIST_ISLA_SPEAK },
                { "] Niles, the Interrogator: ", EVENT_TYPES.HEIST_NILES_SPEAK },
                { "] Nenet, the Scout: ", EVENT_TYPES.HEIST_NENET_SPEAK },
                { "] Vinderi, the Dismantler: ", EVENT_TYPES.HEIST_VINDERI_SPEAK },
                { "] Gianna, the Master of Disguise: ", EVENT_TYPES.HEIST_GIANNA_SPEAK },
                { "] Huck, the Soldier: ", EVENT_TYPES.HEIST_HUCK_SPEAK },

                // SANCTUM: Lycia 2
                {"] Lycia, Herald of the Scourge: *laughter*", EVENT_TYPES.SANCTUM_LYCIA_2_KILLED },
                {"] Lycia, Herald of the Scourge: You have accomplished nothing... I have achieved eternal life...", EVENT_TYPES.SANCTUM_LYCIA_2_KILLED },
                {"] Lycia, Herald of the Scourge: You are only delaying the inevitable.", EVENT_TYPES.SANCTUM_LYCIA_2_KILLED },
                {"] Lycia, Herald of the Scourge: I cannot die. You know this.", EVENT_TYPES.SANCTUM_LYCIA_2_KILLED },
                {"] Lycia, Herald of the Scourge: Beidat beckons just beyond the door...", EVENT_TYPES.SANCTUM_LYCIA_2_KILLED },
                {"] Lycia, Herald of the Scourge: You've accomplished nothing.", EVENT_TYPES.SANCTUM_LYCIA_2_KILLED },

                // SANCTUM: Lycia 1
                {"] Lycia, Unholy Heretic: The moment is nigh. The ritual is nearly complete. Give me your all!", EVENT_TYPES.SANCTUM_LYCIA_1_KILLED },
                {"] Lycia, Unholy Heretic: The Scourge will consume this world of sinners!", EVENT_TYPES.SANCTUM_LYCIA_1_KILLED },
                {"] Lycia, Unholy Heretic: None are innocent. All will be consumed!", EVENT_TYPES.SANCTUM_LYCIA_1_KILLED },
                {"] Lycia, Unholy Heretic: Beidat, take my flesh!", EVENT_TYPES.SANCTUM_LYCIA_1_KILLED },
                {"] Lycia, Unholy Heretic: I have such sights to show you...", EVENT_TYPES.SANCTUM_LYCIA_1_KILLED },
                {"] Lycia, Unholy Heretic: Beidat, give me your power!", EVENT_TYPES.SANCTUM_LYCIA_1_KILLED },
                {"] Lycia, Unholy Heretic: Yes! The blood moon rises! Beidat, give me your power!", EVENT_TYPES.SANCTUM_LYCIA_1_KILLED },

                // ANCESTOR: Lost Matches
                {"] Navali: There is no shame in defeat... at least, not yet.", EVENT_TYPES.ANCESTOR_MATCH_LOST },
                {"] Navali: Defeat is inevitable, as is the rematch.", EVENT_TYPES.ANCESTOR_MATCH_LOST },
                {"] Navali: The Trial is not over. Not yet.", EVENT_TYPES.ANCESTOR_MATCH_LOST },
                {"] Navali: You will have another chance in this Trial.", EVENT_TYPES.ANCESTOR_MATCH_LOST },

                // ANCESTOR: Won Matches
                {"] Navali: The outsider claims another victory!", EVENT_TYPES.ANCESTOR_MATCH_WON },
                {"] Navali: Our mortal challenger wins!", EVENT_TYPES.ANCESTOR_MATCH_WON },
                {"] Navali: The reigning Champion emerges triumphant once more!", EVENT_TYPES.ANCESTOR_MATCH_WON },
                {"] Navali: The Trial continues, and the outsider moves forward!", EVENT_TYPES.ANCESTOR_MATCH_WON },

                // ANCESTOR: Tournament won
                {"] Navali: The tournament is over. The outsider... has won.", EVENT_TYPES.ANCESTOR_TOURNAMENT_WON },
                {"] Navali: The tournament is over. The outsider has done well.", EVENT_TYPES.ANCESTOR_TOURNAMENT_WON },
                {"] Navali: The tournament is over.", EVENT_TYPES.ANCESTOR_TOURNAMENT_WON },
                {"] Navali: We have our new Champion... the outsider!", EVENT_TYPES.ANCESTOR_TOURNAMENT_WON },
                {"] Navali: The Witch is our new Champion!", EVENT_TYPES.ANCESTOR_TOURNAMENT_WON },
                {"] Navali: The Templar... is our new Champion.", EVENT_TYPES.ANCESTOR_TOURNAMENT_WON },
                {"] Navali: Our proud son, the Marauder, is our new Champion.", EVENT_TYPES.ANCESTOR_TOURNAMENT_WON },
                {"] Navali: The Duelist is our new Champion!", EVENT_TYPES.ANCESTOR_TOURNAMENT_WON },
                {"] Navali: The Ranger has bested our warriors, and is now our new Champion!", EVENT_TYPES.ANCESTOR_TOURNAMENT_WON },
                {"] Navali: The Shadow has proven his skills, and become our Champion!", EVENT_TYPES.ANCESTOR_TOURNAMENT_WON },
                {"] Navali: The Scion is our new Champion!", EVENT_TYPES.ANCESTOR_TOURNAMENT_WON },
                {"] Navali: Our Champion has defended his title.", EVENT_TYPES.ANCESTOR_TOURNAMENT_WON },
                {"] Navali: Our Champion has defended her title.", EVENT_TYPES.ANCESTOR_TOURNAMENT_WON },
                {"] Navali: The reigning Champion has defended his title.", EVENT_TYPES.ANCESTOR_TOURNAMENT_WON },
                {"] Navali: The reigning Champion has defended her title.", EVENT_TYPES.ANCESTOR_TOURNAMENT_WON },

                // ANCESTOR: Tournament lost
                {"] Navali: A painful defeat. Time to rest and recover for the next tournament.", EVENT_TYPES.ANCESTOR_TOURNAMENT_LOST },
                {"] Navali: The war is over for the outsider.", EVENT_TYPES.ANCESTOR_TOURNAMENT_LOST },
                {"] Navali: The outsider has been eliminated from the tournament.", EVENT_TYPES.ANCESTOR_TOURNAMENT_LOST },

                // Campaign
                { "] Sin: May a new dawn arise!", EVENT_TYPES.CAMPAIGN_FINISHED },

                // Disconnect
                { "Abnormal disconnect: An unexpected disconnection occurred.", EVENT_TYPES.ABNORMAL_DISCONNECT },
            };
        }

    }
}

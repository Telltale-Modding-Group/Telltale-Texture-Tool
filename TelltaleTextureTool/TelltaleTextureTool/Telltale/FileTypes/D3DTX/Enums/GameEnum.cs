using System.ComponentModel.DataAnnotations;

namespace TelltaleTextureTool.TelltaleEnums;

public enum TelltaleToolGame
{
    UNKNOWN = -1,

    [Display(Name = "Default Mode")]
    DEFAULT = 0,

    [Display(Name = "Texas Hold'em (Original)")]
    TEXAS_HOLD_EM_OG,  // LV9

    [Display(Name = "Bone: Out from Boneville")]
    BONE_OUT_FROM_BONEVILLE, // LV11 WORKS

    [Display(Name = "CSI: 3 Dimensions of Murder")]
    CSI_3_DIMENSIONS, // LV12 WORKS

    [Display(Name = "Sam & Max Save the World (2006)")]
    SAM_AND_MAX_SAVE_THE_WORLD_2006, // LV13

    [Display(Name = "Bone: The Great Cow Race")]
    BONE_THE_GREAT_COW_RACE, // LV11 WORKS

    [Display(Name = "CSI: Hard Evidence")]
    CSI_HARD_EVIDENCE, // LV10

    [Display(Name = "Texas Hold'em (Steam)")]
    TEXAS_HOLD_EM_V1, // LV9

    [Display(Name = "Sam & Max Beyond Time and Space (Original)")]
    SAM_AND_MAX_BEYOND_TIME_AND_SPACE_OG, // LV9

    [Display(Name = "Sam & Max Beyond Time and Space (Steam)")]
    SAM_AND_MAX_BEYOND_TIME_AND_SPACE_NEW, // LV9

    [Display(Name = "Strong Bad's Cool Game for Attractive People Ep. 1")]
    STRONG_BADS_COOL_GAME_FOR_ATTRACTIVE_PEOPLE_101, // LV8

    [Display(Name = "Strong Bad's Cool Game for Attractive People Ep. 2")]
    STRONG_BADS_COOL_GAME_FOR_ATTRACTIVE_PEOPLE_102, // LV8

    [Display(Name = "Strong Bad's Cool Game for Attractive People Ep. 3")]
    STRONG_BADS_COOL_GAME_FOR_ATTRACTIVE_PEOPLE_103, // LV7

    [Display(Name = "Strong Bad's Cool Game for Attractive People Ep. 4")]
    STRONG_BADS_COOL_GAME_FOR_ATTRACTIVE_PEOPLE_104, // LV7

    [Display(Name = "Strong Bad's Cool Game for Attractive People Ep. 5")]
    STRONG_BADS_COOL_GAME_FOR_ATTRACTIVE_PEOPLE_105, // LV6

    [Display(Name = "Wallace & Gromit's Grand Adventures Ep. 1")]
    WALLACE_AND_GROMITS_GRAND_ADVENTURES_101, // LV5

    [Display(Name = "Wallace & Gromit's Grand Adventures Ep. 2")]
    WALLACE_AND_GROMITS_GRAND_ADVENTURES_102, // LV5

    [Display(Name = "Wallace & Gromit's Grand Adventures Ep. 3")]
    WALLACE_AND_GROMITS_GRAND_ADVENTURES_103, // LV5

    [Display(Name = "Wallace & Gromit's Grand Adventures Ep. 4")]
    WALLACE_AND_GROMITS_GRAND_ADVENTURES_104, // LV4

    [Display(Name = "Sam & Max Save the World (2007)")]
    SAM_AND_MAX_SAVE_THE_WORLD_2007, // LV4

    [Display(Name = "CSI: Deadly Intent")]
    CSI_DEADLY_INTENT, // LV4

    [Display(Name = "Tales of Monkey Island V1")]
    TALES_OF_MONKEY_ISLAND_V1, // LV4

    [Display(Name = "Tales of Monkey Island V2")]
    TALES_OF_MONKEY_ISLAND_V2, // LV4

    [Display(Name = "CSI: Fatal Conspiracy")]
    CSI_FATAL_CONSPIRACY, // LV4

    [Display(Name = "Nelson Tethers: Puzzle Agent")]
    NELSON_TETHERS_PUZZLE_AGENT, // LV3

    [Display(Name = "Poker Night at the Inventory")]
    POKER_NIGHT_AT_THE_INVENTORY, // LV3

    [Display(Name = "Sam & Max: The Devil's Playhouse")]
    SAM_AND_MAX_THE_DEVILS_PLAYHOUSE, // LV4

    [Display(Name = "Back to the Future: The Game")]
    BACK_TO_THE_FUTURE_THE_GAME, // LV3

    [Display(Name = "Hector: Badge of Carnage")]
    HECTOR_BADGE_OF_CARNAGE, // LV3

    [Display(Name = "Jurassic Park: The Game")]
    JURASSIC_PARK_THE_GAME, // LV2

    [Display(Name = "Puzzle Agent 2")]
    PUZZLE_AGENT_2, // LV2

    [Display(Name = "Law & Order: Legacies")]
    LAW_AND_ORDER_LEGACIES, // LV2

    [Display(Name = "The Walking Dead")]
    THE_WALKING_DEAD, // LV1

    [Display(Name = "Poker Night 2")]
    POKER_NIGHT_2, // V3

    [Display(Name = "The Walking Dead: Season 2")]
    THE_WALKING_DEAD_SEASON_2, // V4

    [Display(Name = "The Wolf Among Us")]
    THE_WOLF_AMONG_US, // V4

    [Display(Name = "Game of Thrones")]
    GAME_OF_THRONES, // V5

    [Display(Name = "Tales from the Borderlands (Original)")]
    TALES_FROM_THE_BORDERLANDS_OG, // V5

    [Display(Name = "Back to the Future: The Game 30th Anniversary Edition")]
    BACK_TO_THE_FUTURE_THE_GAME_30TH_ANNIVERSARY_EDITION, // V5?

    [Display(Name = "Minecraft: Story Mode")]
    MINECRAFT_STORY_MODE, // V5

    [Display(Name = "Minecraft: Story Mode (Xbox One)")]
    MINECRAFT_STORY_MODE_XBOX_ONE, // V6

    [Display(Name = "The Walking Dead: Michonne")]
    THE_WALKING_DEAD_MICHONNE, // V7

    [Display(Name = "Tales from the Borderlands")]
    TALES_FROM_THE_BORDERLANDS, // V7

    [Display(Name = "Batman: The Telltale Series (Pre-December Patch)")]
    BATMAN_THE_TELLTALE_SERIES_OG, // V8

    [Display(Name = "Batman: The Telltale Series")]
    BATMAN_THE_TELLTALE_SERIES, // V9

    [Display(Name = "The Walking Dead: A New Frontier")]
    THE_WALKING_DEAD_A_NEW_FRONTIER, // V9

    [Display(Name = "Minecraft: Story Mode Season 2")]
    MINECRAFT_STORY_MODE_SEASON_2, // V9

    [Display(Name = "Batman: The Enemy Within")]
    BATMAN_THE_ENEMY_WITHIN, // V9

    [Display(Name = "Guardians of the Galaxy: The Telltale Series")]
    GUARDIANS_OF_THE_GALAXY, // V9

    [Display(Name = "The Walking Dead: Final Season")]
    THE_WALKING_DEAD_FINAL_SEASON, // V9

    [Display(Name = "Batman: Shadows Edition")]
    BATMAN_SHADOWS_EDITION, // V9

    [Display(Name = "The Walking Dead: Definitive Series")]
    THE_WALKING_DEAD_DEFINITIVE_SERIES, // V9

    [Display(Name = "Sam & Max Save the World Remastered")]
    SAM_AND_MAX_SAVE_THE_WORLD_REMASTERED, // V9

    [Display(Name = "Sam & Max Beyond Time and Space Remastered")]
    SAM_AND_MAX_BEYOND_TIME_AND_SPACE_REMASTERED, // V9

    [Display(Name = "Sam & Max: The Devil's Playhouse Remastered")]
    SAM_AND_MAX_THE_DEVILS_PLAYHOUSE_REMASTERED, // V9
}
public static class TelltaleToolGameExtensions
{
    public static string GetGameName(this TelltaleToolGame game)
    {
        return game switch
        {
            TelltaleToolGame.DEFAULT => "default",
            TelltaleToolGame.TEXAS_HOLD_EM_OG => "texasholdem_v1",
            TelltaleToolGame.BONE_OUT_FROM_BONEVILLE => "boneville",
            TelltaleToolGame.CSI_3_DIMENSIONS => "csi3dimensions",
            TelltaleToolGame.SAM_AND_MAX_SAVE_THE_WORLD_2006 => "sammax1_v1",
            TelltaleToolGame.BONE_THE_GREAT_COW_RACE => "cowrace",
            TelltaleToolGame.CSI_HARD_EVIDENCE => "csihard",
            TelltaleToolGame.TEXAS_HOLD_EM_V1 => "texasholdem_v2",
            TelltaleToolGame.SAM_AND_MAX_BEYOND_TIME_AND_SPACE_OG => "sammax2_v1",
            TelltaleToolGame.SAM_AND_MAX_BEYOND_TIME_AND_SPACE_NEW => "sammax2_v2",
            TelltaleToolGame.STRONG_BADS_COOL_GAME_FOR_ATTRACTIVE_PEOPLE_101 => "sbcg4ap1",
            TelltaleToolGame.STRONG_BADS_COOL_GAME_FOR_ATTRACTIVE_PEOPLE_102 => "sbcg4ap2",
            TelltaleToolGame.STRONG_BADS_COOL_GAME_FOR_ATTRACTIVE_PEOPLE_103 => "sbcg4ap3",
            TelltaleToolGame.STRONG_BADS_COOL_GAME_FOR_ATTRACTIVE_PEOPLE_104 => "sbcg4ap4",
            TelltaleToolGame.STRONG_BADS_COOL_GAME_FOR_ATTRACTIVE_PEOPLE_105 => "sbcg4ap5",
            TelltaleToolGame.WALLACE_AND_GROMITS_GRAND_ADVENTURES_101 => "wag1",
            TelltaleToolGame.WALLACE_AND_GROMITS_GRAND_ADVENTURES_102 => "wag2",
            TelltaleToolGame.WALLACE_AND_GROMITS_GRAND_ADVENTURES_103 => "wag3",
            TelltaleToolGame.WALLACE_AND_GROMITS_GRAND_ADVENTURES_104 => "wag4",
            TelltaleToolGame.SAM_AND_MAX_SAVE_THE_WORLD_2007 => "sammax1_v2",
            TelltaleToolGame.CSI_DEADLY_INTENT => "csideadly",
            TelltaleToolGame.TALES_OF_MONKEY_ISLAND_V1 => "monkeyisland_v1",
            TelltaleToolGame.TALES_OF_MONKEY_ISLAND_V2 => "monkeyisland_v2",
            TelltaleToolGame.CSI_FATAL_CONSPIRACY => "csifatal",
            TelltaleToolGame.NELSON_TETHERS_PUZZLE_AGENT => "puzzleagent",
            TelltaleToolGame.POKER_NIGHT_AT_THE_INVENTORY => "pokernight",
            TelltaleToolGame.SAM_AND_MAX_THE_DEVILS_PLAYHOUSE => "sammax3",
            TelltaleToolGame.BACK_TO_THE_FUTURE_THE_GAME => "bttf",
            TelltaleToolGame.HECTOR_BADGE_OF_CARNAGE => "hector",
            TelltaleToolGame.JURASSIC_PARK_THE_GAME => "jurassic",
            TelltaleToolGame.PUZZLE_AGENT_2 => "puzzleagent2",
            TelltaleToolGame.LAW_AND_ORDER_LEGACIES => "lawandorder",
            TelltaleToolGame.THE_WALKING_DEAD => "twd",
            TelltaleToolGame.POKER_NIGHT_2 => "pokernight2",
            TelltaleToolGame.THE_WALKING_DEAD_SEASON_2 => "twd2",
            TelltaleToolGame.THE_WOLF_AMONG_US => "twau",
            TelltaleToolGame.GAME_OF_THRONES => "got",
            TelltaleToolGame.TALES_FROM_THE_BORDERLANDS_OG => "borderlands_v1",
            TelltaleToolGame.BACK_TO_THE_FUTURE_THE_GAME_30TH_ANNIVERSARY_EDITION => "bttf30",
            TelltaleToolGame.MINECRAFT_STORY_MODE => "minecraft",
            TelltaleToolGame.MINECRAFT_STORY_MODE_XBOX_ONE => "minecraft_xboxone",
            TelltaleToolGame.THE_WALKING_DEAD_MICHONNE => "michonne",
            TelltaleToolGame.TALES_FROM_THE_BORDERLANDS => "borderlands_v2",
            TelltaleToolGame.BATMAN_THE_TELLTALE_SERIES_OG => "batman1_v1",
            TelltaleToolGame.BATMAN_THE_TELLTALE_SERIES => "batman1_v2",
            TelltaleToolGame.THE_WALKING_DEAD_A_NEW_FRONTIER => "twd3",
            TelltaleToolGame.MINECRAFT_STORY_MODE_SEASON_2 => "minecraft2",
            TelltaleToolGame.BATMAN_THE_ENEMY_WITHIN => "batman2",
            TelltaleToolGame.GUARDIANS_OF_THE_GALAXY => "guardians",
            TelltaleToolGame.THE_WALKING_DEAD_FINAL_SEASON => "twd4",
            TelltaleToolGame.BATMAN_SHADOWS_EDITION => "batman_shadows",
            TelltaleToolGame.THE_WALKING_DEAD_DEFINITIVE_SERIES => "twd_definitive",
            TelltaleToolGame.SAM_AND_MAX_SAVE_THE_WORLD_REMASTERED => "sammax1_remastered",
            TelltaleToolGame.SAM_AND_MAX_BEYOND_TIME_AND_SPACE_REMASTERED => "sammax2_remastered",
            TelltaleToolGame.SAM_AND_MAX_THE_DEVILS_PLAYHOUSE_REMASTERED => "sammax3_remastered",
            _ => "unknown"
        };
    }

    public static TelltaleToolGame GetTelltaleToolGameFromString(string game)
    {
        return game switch
        {
            "default" => TelltaleToolGame.DEFAULT,
            "texasholdem_v1" => TelltaleToolGame.TEXAS_HOLD_EM_OG,
            "boneville" => TelltaleToolGame.BONE_OUT_FROM_BONEVILLE,
            "csi3dimensions" => TelltaleToolGame.CSI_3_DIMENSIONS,
            "sammax1_v1" => TelltaleToolGame.SAM_AND_MAX_SAVE_THE_WORLD_2006,
            "cowrace" => TelltaleToolGame.BONE_THE_GREAT_COW_RACE,
            "csihard" => TelltaleToolGame.CSI_HARD_EVIDENCE,
            "texasholdem_v2" => TelltaleToolGame.TEXAS_HOLD_EM_V1,
            "sammax2_v1" => TelltaleToolGame.SAM_AND_MAX_BEYOND_TIME_AND_SPACE_OG,
            "sammax2_v2" => TelltaleToolGame.SAM_AND_MAX_BEYOND_TIME_AND_SPACE_NEW,
            "sbcg4ap1" => TelltaleToolGame.STRONG_BADS_COOL_GAME_FOR_ATTRACTIVE_PEOPLE_101,
            "sbcg4ap2" => TelltaleToolGame.STRONG_BADS_COOL_GAME_FOR_ATTRACTIVE_PEOPLE_102,
            "sbcg4ap3" => TelltaleToolGame.STRONG_BADS_COOL_GAME_FOR_ATTRACTIVE_PEOPLE_103,
            "sbcg4ap4" => TelltaleToolGame.STRONG_BADS_COOL_GAME_FOR_ATTRACTIVE_PEOPLE_104,
            "sbcg4ap5" => TelltaleToolGame.STRONG_BADS_COOL_GAME_FOR_ATTRACTIVE_PEOPLE_105,
            "wag1" => TelltaleToolGame.WALLACE_AND_GROMITS_GRAND_ADVENTURES_101,
            "wag2" => TelltaleToolGame.WALLACE_AND_GROMITS_GRAND_ADVENTURES_102,
            "wag3" => TelltaleToolGame.WALLACE_AND_GROMITS_GRAND_ADVENTURES_103,
            "wag4" => TelltaleToolGame.WALLACE_AND_GROMITS_GRAND_ADVENTURES_104,
            "sammax1_v2" => TelltaleToolGame.SAM_AND_MAX_SAVE_THE_WORLD_2007,
            "csideadly" => TelltaleToolGame.CSI_DEADLY_INTENT,
            "monkeyisland" => TelltaleToolGame.TALES_OF_MONKEY_ISLAND_V1,
            "monkeyisland_v1" => TelltaleToolGame.TALES_OF_MONKEY_ISLAND_V1,
            "monkeyisland_v2" => TelltaleToolGame.TALES_OF_MONKEY_ISLAND_V2,
            "csifatal" => TelltaleToolGame.CSI_FATAL_CONSPIRACY,
            "puzzleagent" => TelltaleToolGame.NELSON_TETHERS_PUZZLE_AGENT,
            "pokernight" => TelltaleToolGame.POKER_NIGHT_AT_THE_INVENTORY,
            "sammax3" => TelltaleToolGame.SAM_AND_MAX_THE_DEVILS_PLAYHOUSE,
            "bttf" => TelltaleToolGame.BACK_TO_THE_FUTURE_THE_GAME,
            "hector" => TelltaleToolGame.HECTOR_BADGE_OF_CARNAGE,
            "jurassic" => TelltaleToolGame.JURASSIC_PARK_THE_GAME,
            "puzzleagent2" => TelltaleToolGame.PUZZLE_AGENT_2,
            "lawandorder" => TelltaleToolGame.LAW_AND_ORDER_LEGACIES,
            "twd" => TelltaleToolGame.THE_WALKING_DEAD,
            "pokernight2" => TelltaleToolGame.POKER_NIGHT_2,
            "twd2" => TelltaleToolGame.THE_WALKING_DEAD_SEASON_2,
            "twau" => TelltaleToolGame.THE_WOLF_AMONG_US,
            "got" => TelltaleToolGame.GAME_OF_THRONES,
            "borderlands_v1" => TelltaleToolGame.TALES_FROM_THE_BORDERLANDS_OG,
            "bttf30" => TelltaleToolGame.BACK_TO_THE_FUTURE_THE_GAME_30TH_ANNIVERSARY_EDITION,
            "minecraft" => TelltaleToolGame.MINECRAFT_STORY_MODE,
            "minecraft_xboxone" => TelltaleToolGame.MINECRAFT_STORY_MODE_XBOX_ONE,
            "michonne" => TelltaleToolGame.THE_WALKING_DEAD_MICHONNE,
            "borderlands_v2" => TelltaleToolGame.TALES_FROM_THE_BORDERLANDS,
            "batman1_v1" => TelltaleToolGame.BATMAN_THE_TELLTALE_SERIES_OG,
            "batman1_v2" => TelltaleToolGame.BATMAN_THE_TELLTALE_SERIES,
            "twd3" => TelltaleToolGame.THE_WALKING_DEAD_A_NEW_FRONTIER,
            "minecraft2" => TelltaleToolGame.MINECRAFT_STORY_MODE_SEASON_2,
            "batman2" => TelltaleToolGame.BATMAN_THE_ENEMY_WITHIN,
            "guardians" => TelltaleToolGame.GUARDIANS_OF_THE_GALAXY,
            "twd4" => TelltaleToolGame.THE_WALKING_DEAD_FINAL_SEASON,
            "batman_shadows" => TelltaleToolGame.BATMAN_SHADOWS_EDITION,
            "twd_definitive" => TelltaleToolGame.THE_WALKING_DEAD_DEFINITIVE_SERIES,
            "sammax1_remastered" => TelltaleToolGame.SAM_AND_MAX_SAVE_THE_WORLD_REMASTERED,
            "sammax2_remastered" => TelltaleToolGame.SAM_AND_MAX_BEYOND_TIME_AND_SPACE_REMASTERED,
            "sammax3_remastered" => TelltaleToolGame.SAM_AND_MAX_THE_DEVILS_PLAYHOUSE_REMASTERED,
            _ => TelltaleToolGame.UNKNOWN
        };
    }
}

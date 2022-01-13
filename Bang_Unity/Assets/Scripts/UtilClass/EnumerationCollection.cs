public enum Header
{
    Welcome,
    Chatting,

    GameStartButtonDown,

    ReadyToStartGame,
    ReadyToStartGameCompleted,

    ShowPlayerState,

    SetClientName,

    SetPlayerCount,
    SetPlayerNumber,
    SetPlayerName,

    SetPlayerJob,
    SetPlayerJobCompleted,

    ChooseCharacter,
    SetPlayerCharacter,
    SetPlayerCharacterCompleted,

    SetPlayerLife,
    SetPlayerLifeCompleted,

    SetPlayerCard,
    SetPlayerCardCompleted,

    SetPlayerTurn,
    SetPlayerTurnCompleted,

    DrawCard,

    JesseJonesSelectCard,
    JesseJonesDrawCard,

    BartCassidyStealCard,
    BartCassidyStealCardCompleted,

    ElGringoStealCard,
    ElGringoStealCardCompleted,

    KitCarlsonDraw,
    KitCarlsonRestoreCard,

    PedroRamirezSelectCard,
    PedroRamirezDrawCard,

    BlackJackDraw,
    BlackJackDrawMore,

    CardShuffle,

    PassTurn,
    RequestDropCard,
    ResponseDropCard,
    DropCard,

    Bang,
    BangResponse,
    BangCardOpen,
    BangCardDoubleOpen,
    BangCardOpenCompleted,

    AttackCardMove,

    CardOpenOrderSelect,
    CardOpenOrderSelectCompleted,

    Prigione,
    PrigioneCardOpen,
    PrigioneCardOpenCompleted,

    Dinamite,
    DinamitePass,
    DinamiteExplosion,
    DinamiteExplosionCompleted,
    DinamiteCardOpen,
    DinamiteCardOpenCompleted,

    Duello,
    DuelloResponse,

    CatBalou,
    CatBalouCompleted,
    CatBalouDropCard,

    Panico,
    StealCard,

    Beer,
    Saloon,
    Emporio,
    EmporioGetCard,
    EmporioFinish,

    Barile,
    Diligenza,
    WellsFargo,
    Mirino,
    Mustang,
    EquipGun,

    Gatling,
    GatlingDropCard,
    Indian,
    IndianDropCard,

    WideAttackResponse,

    VultureSameBringCard,
    Defeat,
    ContinueGame,

    OpenJob,
    OpenJobCompleted,

    UpdateLife,

    GameOver,
    BackToMain,

    ShutDown,
    Disconnect
}

public enum Job
{
    Sheriff,
    Traitor,
    Outlaw,
    Outlaw2,
    Vice,
    Outlaw3,
    Vice2
}

public enum Character
{
    BartCassidy,
    BlackJack,
    CalamityJanet,
    ElGringo,
    JesseJones,
    Jourdonnais,
    KitCarlson,
    LuckyDuke,
    PaulRegret,
    PedroRamirez,
    RoseDoolan,
    SidKetchum,
    SlabTheKiller,
    SuzyLafayette,
    VultureSame,
    WillyTheKid,
}

public enum Card
{
    S1_Mirino,
    S1_Bang,
    S2_Missed,
    S3_Missed,
    S4_Missed,
    S5_Missed,
    S6_Missed,
    S7_Missed,
    S8_Missed,
    S8_Winchester,
    S9_Diligenza,
    S9_Diligenza2,
    ST_Prigione,
    ST_Volcanic,
    SJ_Duello,
    SJ_Prigione,
    SQ_Emporio,
    SQ_Barile,
    SK_Barile,
    D1_Indian,
    D1_Bang,
    D2_Bang,
    D3_Bang,
    D4_Bang,
    D5_Bang,
    D6_Bang,
    D7_Bang,
    D8_Bang,
    D8_Panico,
    D9_Bang,
    D9_CatBalou,
    DT_Bang,
    DT_CatBalou,
    DJ_Bang,
    DJ_CatBalou,
    DQ_Bang,
    DQ_Duello,
    DK_Bang,
    DK_Indian,
    C1_Missed,
    C1_RevCarabine,
    C2_Bang,
    C3_Bang,
    C4_Bang,
    C5_Bang,
    C6_Bang,
    C7_Bang,
    C8_Bang,
    C8_Duello,
    C9_Emporio,
    C9_Bang,
    CT_Volcanic,
    CT_Missed,
    CJ_Missed,
    CJ_Schofield,
    CQ_Schofield,
    CQ_Missed,
    CK_Remington,
    CK_Schofield,
    CK_Missed,
    H1_Bang,
    H1_Panico,
    H2_Dinamite,
    H3_WellsFargo,
    H4_Prigione,
    H5_Saloon,
    H6_Beer,
    H7_Beer,
    H8_Beer,
    H8_Mustang,
    H9_Mustang,
    H9_Beer,
    HT_Beer,
    HJ_Beer,
    HT_Gatling,
    HJ_Panico,
    HQ_Panico,
    HQ_Bang,
    HK_Bang,
    HK_CatBalou,
}
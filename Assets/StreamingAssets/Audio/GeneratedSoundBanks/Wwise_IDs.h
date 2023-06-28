/////////////////////////////////////////////////////////////////////////////////////////////////////
//
// Audiokinetic Wwise generated include file. Do not edit.
//
/////////////////////////////////////////////////////////////////////////////////////////////////////

#ifndef __WWISE_IDS_H__
#define __WWISE_IDS_H__

#include <AK/SoundEngine/Common/AkTypes.h>

namespace AK
{
    namespace EVENTS
    {
        static const AkUniqueID PLAY_SSFX_ALLYTURN = 2945031810U;
        static const AkUniqueID PLAY_SSFX_ARROWHIT = 2970065691U;
        static const AkUniqueID PLAY_SSFX_BLUEBUFF = 2673952466U;
        static const AkUniqueID PLAY_SSFX_CARDDRAW = 1322830077U;
        static const AkUniqueID PLAY_SSFX_CARDPICK = 3130717460U;
        static const AkUniqueID PLAY_SSFX_CARDSLAP = 1232721797U;
        static const AkUniqueID PLAY_SSFX_CHAINS = 230033191U;
        static const AkUniqueID PLAY_SSFX_CLICK = 3681299163U;
        static const AkUniqueID PLAY_SSFX_CLICKBLOCKED = 2447394445U;
        static const AkUniqueID PLAY_SSFX_COMBATLOSE = 2963818062U;
        static const AkUniqueID PLAY_SSFX_COMBATWIN = 1271310997U;
        static const AkUniqueID PLAY_SSFX_CRATE = 900263106U;
        static const AkUniqueID PLAY_SSFX_DARKBUFF = 2048744142U;
        static const AkUniqueID PLAY_SSFX_DASH = 2787275873U;
        static const AkUniqueID PLAY_SSFX_DECKSHUFFLE = 329156649U;
        static const AkUniqueID PLAY_SSFX_ENEMYTURN = 3353955478U;
        static const AkUniqueID PLAY_SSFX_FASTHIT = 2463337978U;
        static const AkUniqueID PLAY_SSFX_FISH = 3581818263U;
        static const AkUniqueID PLAY_SSFX_HERB = 4082667236U;
        static const AkUniqueID PLAY_SSFX_MAGICRISE = 2970430981U;
        static const AkUniqueID PLAY_SSFX_MYTURNEND = 3003442901U;
        static const AkUniqueID PLAY_SSFX_MYTURNSTART = 3350516382U;
        static const AkUniqueID PLAY_SSFX_ORE = 1628470307U;
        static const AkUniqueID PLAY_SSFX_PORTAL = 3131711783U;
        static const AkUniqueID PLAY_SSFX_ROCKDESTROY = 3556133744U;
        static const AkUniqueID PLAY_SSFX_ROCKSPIKE = 259866600U;
        static const AkUniqueID PLAY_SSFX_WALK = 2953260286U;
        static const AkUniqueID PLAY_SSFX_WOOD = 2902633162U;
        static const AkUniqueID SET_GRASS = 4038830388U;
        static const AkUniqueID SET_LAYER_0 = 217388420U;
        static const AkUniqueID SET_LAYER_1 = 217388421U;
        static const AkUniqueID SET_LAYER_2 = 217388422U;
        static const AkUniqueID SET_LAYER_3 = 217388423U;
        static const AkUniqueID SET_SAND = 4022000560U;
        static const AkUniqueID SETCOMBAT = 1592321915U;
        static const AkUniqueID SETEXPLORE = 2316203732U;
        static const AkUniqueID SETMAINMENU = 3628674037U;
        static const AkUniqueID STARTMUSIC = 3827058668U;
        static const AkUniqueID STOPMUSIC = 1917263390U;
    } // namespace EVENTS

    namespace STATES
    {
        namespace GAMESTATE
        {
            static const AkUniqueID GROUP = 4091656514U;

            namespace STATE
            {
                static const AkUniqueID COMBAT = 2764240573U;
                static const AkUniqueID EXPLORE = 579523862U;
                static const AkUniqueID MAINMENU = 3604647259U;
                static const AkUniqueID NONE = 748895195U;
            } // namespace STATE
        } // namespace GAMESTATE

        namespace LAYERS
        {
            static const AkUniqueID GROUP = 3298531235U;

            namespace STATE
            {
                static const AkUniqueID LAYERS0 = 602912169U;
                static const AkUniqueID LAYERS1 = 602912168U;
                static const AkUniqueID LAYERS2 = 602912171U;
                static const AkUniqueID LAYERS3 = 602912170U;
                static const AkUniqueID NONE = 748895195U;
            } // namespace STATE
        } // namespace LAYERS

        namespace WORLD
        {
            static const AkUniqueID GROUP = 2609808943U;

            namespace STATE
            {
                static const AkUniqueID ABYSS = 4215188899U;
                static const AkUniqueID NONE = 748895195U;
                static const AkUniqueID OVERWORLD = 1562068129U;
            } // namespace STATE
        } // namespace WORLD

    } // namespace STATES

    namespace SWITCHES
    {
        namespace GROUND_SWITCH
        {
            static const AkUniqueID GROUP = 2578143219U;

            namespace SWITCH
            {
                static const AkUniqueID GRASS = 4248645337U;
                static const AkUniqueID SAND = 803837735U;
                static const AkUniqueID STONE = 1216965916U;
            } // namespace SWITCH
        } // namespace GROUND_SWITCH

    } // namespace SWITCHES

    namespace GAME_PARAMETERS
    {
        static const AkUniqueID RTPC_VOLUME_MASTER = 732857810U;
        static const AkUniqueID RTPC_VOLUME_MUSIC = 2451151905U;
        static const AkUniqueID RTPC_VOLUME_SFX = 4276639445U;
    } // namespace GAME_PARAMETERS

    namespace BANKS
    {
        static const AkUniqueID INIT = 1355168291U;
        static const AkUniqueID SB_GLOBAL = 2711878180U;
    } // namespace BANKS

    namespace BUSSES
    {
        static const AkUniqueID MASTER_AUDIO_BUS = 3803692087U;
        static const AkUniqueID MUSIC = 3991942870U;
        static const AkUniqueID SSFX = 672574001U;
    } // namespace BUSSES

    namespace AUDIO_DEVICES
    {
        static const AkUniqueID NO_OUTPUT = 2317455096U;
        static const AkUniqueID SYSTEM = 3859886410U;
    } // namespace AUDIO_DEVICES

}// namespace AK

#endif // __WWISE_IDS_H__

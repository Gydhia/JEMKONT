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
        static const AkUniqueID PLAY_SSFX__ALLYTURN = 2351453751U;
        static const AkUniqueID PLAY_SSFX__CARDPICK = 284401701U;
        static const AkUniqueID PLAY_SSFX__CARDSLAP = 1884104528U;
        static const AkUniqueID PLAY_SSFX__CLICK = 3568964008U;
        static const AkUniqueID PLAY_SSFX__CLICKBLOCKED = 2149827808U;
        static const AkUniqueID PLAY_SSFX__COMBATLOSE = 611645331U;
        static const AkUniqueID PLAY_SSFX__COMBATWIN = 241397386U;
        static const AkUniqueID PLAY_SSFX__DECKSHUFFLE = 803863510U;
        static const AkUniqueID PLAY_SSFX__ENEMYTURN = 2280958161U;
        static const AkUniqueID PLAY_SSFX__MYTURNEND = 529950874U;
        static const AkUniqueID PLAY_SSFX__MYTURNSTART = 3168485769U;
        static const AkUniqueID PLAY_SSFX__WALK = 71458555U;
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

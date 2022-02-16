using R2API;

namespace HandPlugin.Modules
{
    //TODO: CONVERT TO A PROPER LANGUAGE FILE
    public class LanguageTokens
    {
        private static bool initialized = false;
        public static void Init()
        {
            if (initialized) return;
            initialized = true;

            string HANDDesc = "";
            HANDDesc += "HAN-D is a tanky robot janitor whose powerful melee attacks are sure to leave a mess! <style=cSub>\r\n\r\n";
            HANDDesc += "< ! > HURT has increased knockback against airborne enemies. Use FORCED_REASSEMBLY to pop enemies in the air, then HURT them to send them flying!\r\n\r\n";
            HANDDesc += "< ! > FORCED_REASSEMBLY's self-knockback can be used to reach flying enemies.\r\n\r\n";
            HANDDesc += "< ! > OVERCLOCK lasts as long as you can keep hitting enemies.\r\n\r\n";
            HANDDesc += "< ! > Use DRONES to heal and stay in the fight.\r\n\r\n";
            LanguageAPI.Add("HAND_OVERCLOCKED_DESC", HANDDesc);

            LanguageAPI.Add("HAND_OVERCLOCKED_SUBTITLE", "Lean, Mean, Cleaning Machine");

            LanguageAPI.Add("KEYWORD_HANDOVERCLOCKED_SPRINGY", "<style=cKeywordName>Springy</style><style=cSub>Spring upwards when using this skill.</style>");
            LanguageAPI.Add("KEYWORD_HANDOVERCLOCKED_DEBILITATE", "<style=cKeywordName>Debilitate</style><style=cSub>Reduce damage by <style=cIsDamage>30%</style>. Reduce movement speed by <style=cIsDamage>60%</style>.</style>");

            LanguageAPI.Add("HAND_OVERCLOCKED_NAME", "HAN-D");
            LanguageAPI.Add("HAND_OVERCLOCKED_OUTRO_FLAVOR", "..and so it left, servos pulsing with new life.");
            LanguageAPI.Add("HAND_OVERCLOCKED_MAIN_ENDING_ESCAPE_FAILURE_FLAVOR", "..and so it vanished, unrewarded in all of its efforts.");

            string tldr = "<style=cMono>\r\n//--AUTO-TRANSCRIPTION FROM BASED DEPARTMENT OF UES SAFE TRAVELS--//</style>\r\n\r\n<i>*hits <color=#327FFF>Spinel Tonic</color>*</i>\n\nIs playing without the <color=#6955A6>Command</color> artifact the ultimate form of cuckoldry?\n\nI cannot think or comprehend of anything more cucked than playing without <color=#6955A6>Command</color>. Honestly, think about it rationally. You are shooting, running, jumping for like 60 minutes solely so you can get a fucking <color=#77FF16>Squid Polyp</color>. All that hard work you put into your run - dodging <style=cIsHealth>Stone Golem</style> lasers, getting annoyed by six thousand <style=cIsHealth>Lesser Wisps</color> spawning above your head, activating <color=#E5C962>Shrines of the Mountain</color> all for one simple result: your inventory is filled up with <color=#FFFFFF>Warbanners</color> and <color=#FFFFFF>Monster Tooth</color> necklaces which cost money.\n\nOn a god run? Great. A bunch of shitty items which add nothing to your run end up coming out of the <color=#E5C962>Chests</color> you buy. They get the benefit of your hard earned dosh that came from killing <style=cIsHealth>Lemurians</style>.\n\nAs a man who plays this game you are <style=cIsHealth>LITERALLY</style> dedicating two hours of your life to opening boxes and praying it's not another <color=#77FF16>Chronobauble</color>. It's the ultimate and final cuck. Think about it logically.\r\n<style=cMono>\r\nTranscriptions complete.\r\n</style>\r\n \r\n\r\n";
            LanguageAPI.Add("HAND_OVERCLOCKED_LORE", tldr);


            LanguageAPI.Add("HAND_OVERCLOCKED_PRIMARY_NAME", "HURT");
            LanguageAPI.Add("HAND_OVERCLOCKED_PRIMARY_DESC", "Swing your hammer in a wide arc, hurting enemies for <style=cIsDamage>390% damage</style>.");

            LanguageAPI.Add("HAND_OVERCLOCKED_SECONDARY_NAME", "FORCED_REASSEMBLY");
            LanguageAPI.Add("HAND_OVERCLOCKED_SECONDARY_DESC", "<style=cIsUtility>Springy</style>. Charge up a powerful hammer slam for <style=cIsDamage>400%-1200% damage</style>. <style=cIsDamage>Range and knockback</style> increases with charge.");

            LanguageAPI.Add("HAND_OVERCLOCKED_SECONDARY_SCEPTER_NAME", "UNETHICAL_REASSEMBLY");
            LanguageAPI.Add("HAND_OVERCLOCKED_SECONDARY_SCEPTER_DESC", "<style=cIsUtility>Springy</style>. Charge up an overwhelmingly powerful hammer slam that deals <style=cIsDamage>600%-1800% damage</style> and <style=cIsDamage>zaps</style> enemies. <style=cIsDamage>Range and knockback</style> increases with charge.");

            LanguageAPI.Add("HAND_OVERCLOCKED_UTILITY_NAME", "OVERCLOCK");
            LanguageAPI.Add("HAND_OVERCLOCKED_UTILITY_DESC", "<style=cIsUtility>Springy</style>. Increase <style=cIsUtility>movement speed</style> and <style=cIsDamage>attack speed</style> by <style=cIsDamage>40%</style>, and gain <style=cIsDamage>50% stun chance</style>. <style=cIsUtility>Hit enemies to increase duration</style>.");

            LanguageAPI.Add("HAND_OVERCLOCKED_SPECIAL_NAME", "DRONE");
            LanguageAPI.Add("HAND_OVERCLOCKED_SPECIAL_DESC", "<style=cIsHealing>Heal 8.5% HP</style>. Fire a drone that <style=cIsDamage>Debilitates</style> enemies for <style=cIsDamage>270% damage</style> and <style=cIsHealing>heals</style> allies. <style=cIsUtility>Kills and melee hits reduce cooldown</style>.");

            LanguageAPI.Add("HAND_OVERCLOCKED_PASSIVE_NAME", "PARALLEL_COMPUTING");
            LanguageAPI.Add("HAND_OVERCLOCKED_PASSIVE_DESC", "Gain <style=cIsHealing>+2 armor</style> for every <style=cIsUtility>drone ally on your team</style>.");
        }
    }
}

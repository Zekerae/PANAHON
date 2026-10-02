using System;
using UnityEngine;
using UnityEngine.UI;

namespace Panahon.UI.MainMenu
{
    public class HowToPlayUI : MonoBehaviour
    {
        [Header("Tab Buttons")]
        [SerializeField] private Button tabOverviewButton;
        [SerializeField] private Button tabErasButton;
        [SerializeField] private Button tabCharactersButton;
        [SerializeField] private Button tabTilesButton;
        [SerializeField] private Button tabTimelineButton;

        [Header("Content Panels")]
        [SerializeField] private Text contentTitleText;
        [SerializeField] private Text contentBodyText;
        [SerializeField] private Button closeButton;

        private void Start()
        {
            if (tabOverviewButton != null) tabOverviewButton.onClick.AddListener(ShowOverview);
            if (tabErasButton != null) tabErasButton.onClick.AddListener(ShowEras);
            if (tabCharactersButton != null) tabCharactersButton.onClick.AddListener(ShowCharacters);
            if (tabTilesButton != null) tabTilesButton.onClick.AddListener(ShowTiles);
            if (tabTimelineButton != null) tabTimelineButton.onClick.AddListener(ShowTimeline);
            if (closeButton != null) closeButton.onClick.AddListener(() => gameObject.SetActive(false));

            ShowOverview();
        }

        public void ShowOverview()
        {
            SetContent("<b>P.A.N.A.H.O.N. - MISSION BRIEFING</b>",
                "<b>Overview:</b>\n" +
                "You and your team are <b>Temporal Castaways</b> sent across Philippine history to restore the fabric of time.\n\n" +
                "<b>Your Goal:</b>\n" +
                "1. Follow your assigned <b>Mission Card</b> to locate a displaced historical relic.\n" +
                "2. Travel between the 4 historical eras using <b>Time Gate portals</b>.\n" +
                "3. Collect required <b>Clue Cards</b> on Clue tiles to identify and secure the relic.\n" +
                "4. Deliver the relic back to its true era before the <b>Shared Timeline Meter</b> collapses (10/10)!\n\n" +
                "<b>Game Format:</b> 4 Players (Cooperative victory against Timeline Collapse, with competitive individual scoring)!");
        }

        public void ShowEras()
        {
            SetContent("<b>THE FOUR HISTORICAL ERAS</b>",
                "<b>1. Spanish Era (1521 - 1898) [Terra Cotta]</b>\n" +
                "• Relics: Galleon navigational astrolabes, Golden Monstrance of Intramuros.\n" +
                "• Hazards: Colonial inquisitors, naval typhoons.\n\n" +
                "<b>2. American Era (1898 - 1946) [Navy Blue]</b>\n" +
                "• Relics: Corregidor Ham Radio Transceiver, Commonwealth telecommunication ciphers.\n" +
                "• Hazards: Industrial line surges, blackout protocol.\n\n" +
                "<b>3. Japanese Era (1942 - 1945) [Crimson Red]</b>\n" +
                "• Relics: Guerrilla Intelligence Cipher Book, Bataan field dispatches.\n" +
                "• Hazards: Patrol blockades, radio jamming.\n\n" +
                "<b>4. Myth & Legend Era (Pre-Colonial) [Amethyst Purple]</b>\n" +
                "• Relics: Golden Tara of Agusan, Bakunawa lunar talisman, Bathala stone idols.\n" +
                "• Hazards: Spirit vortex, eclipse tempest.");
        }

        public void ShowCharacters()
        {
            SetContent("<b>TEMPORAL CASTAWAYS (CHARACTERS)</b>",
                "<b>1. Dr. Mateo Cruz (Chrono-Archivist)</b>\n" +
                "• Start Era: Spanish Era\n" +
                "• Special: <i>Archival Mastery</i> &mdash; +3 Energy, -1 Paradox\n\n" +
                "<b>2. Tala Dimaculangan (Rift-Walker)</b>\n" +
                "• Start Era: American Era\n" +
                "• Special: <i>Warp Harmonic</i> &mdash; Only pays 1 Energy for Time Gate jumps\n\n" +
                "<b>3. Capt. Andres Valiente (Paradox Dampener)</b>\n" +
                "• Start Era: Japanese Era\n" +
                "• Special: <i>Timeline Anchor</i> &mdash; -2 Individual Paradox\n\n" +
                "<b>4. Maya Santos (Quantum Scavenger)</b>\n" +
                "• Start Era: Myth & Legend Era\n" +
                "• Special: <i>Temporal Salvage</i> &mdash; +20 Bonus Points, +2 Energy");
        }

        public void ShowTiles()
        {
            SetContent("<b>BOARD TILES & HAZARDS</b>",
                "<b>★ Start / Chrono Nexus:</b> Resting hub. Restores +1 Energy upon landing.\n\n" +
                "<b>🔎 Clue Tile:</b> Draw a Clue Card related to one of the 4 historic relics (+5 pts).\n\n" +
                "<b>⚡ Event Tile:</b> Temporal anomaly! Draw an Event Card with unique blessings or paradox surges.\n\n" +
                "<b>◆ Artifact Tile:</b> Acquire displaced artifacts or return them to their home timeline (+35 to +50 pts).\n\n" +
                "<b>⚠ Danger Tile:</b> Temporal hazard! Lose Energy, gain Paradox, or risk skipping a turn.\n\n" +
                "<b>◎ Time Gate Portal:</b> Pay 2 Energy (1 for Tala) to warp across history to another era track.");
        }

        public void ShowTimeline()
        {
            SetContent("<b>TIMELINE STABILITY & VICTORY CONDITIONS</b>",
                "<b>The Shared Timeline Meter (0 to 10):</b>\n" +
                "• Increases when players suffer Danger hazards or trigger Paradox Overflows (5/5).\n" +
                "• Decreases when players return historical artifacts to their rightful timeline.\n" +
                "• <b>CRITICAL COLLAPSE (10/10):</b> All timelines shatter &mdash; ALL PLAYERS LOSE!\n\n" +
                "<b>Individual Scoring:</b>\n" +
                "• Mission Completed: +50 pts\n" +
                "• Correct Artifact Return: +35 pts\n" +
                "• Clue Discovered: +5 pts\n" +
                "• Timeline Preserved Bonus: +20 pts\n\n" +
                "Stabilize the timeline and achieve the highest score to become the Master Chrono-Agent!");
        }

        private void SetContent(string title, string body)
        {
            if (contentTitleText != null) contentTitleText.text = title;
            if (contentBodyText != null) contentBodyText.text = body;
        }
    }
}

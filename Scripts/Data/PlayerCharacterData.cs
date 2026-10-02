using System;
using UnityEngine;

namespace Panahon.Data
{
    [CreateAssetMenu(fileName = "NewPlayerCharacter", menuName = "PANAHON/Character Data")]
    public class PlayerCharacterData : ScriptableObject
    {
        [Header("Character Identity")]
        [SerializeField] private int playerIndex; // 0, 1, 2, 3
        [SerializeField] private string characterName;
        [SerializeField] private string roleTitle;
        [SerializeField] private Color tokenColor = Color.white;
        [TextArea(2, 4)]
        [SerializeField] private string backstory;

        [Header("Unique Ability")]
        [SerializeField] private string abilityName;
        [TextArea(2, 4)]
        [SerializeField] private string abilityDescription;
        [SerializeField] private int abilityCooldownTurns = 2;

        [Header("Starting Stats")]
        [SerializeField] private int startingEnergy = 10;
        [SerializeField] private int maxEnergy = 15;
        [SerializeField] private EraType startingEra = EraType.SpanishEra;

        public int PlayerIndex => playerIndex;
        public string CharacterName => characterName;
        public string RoleTitle => roleTitle;
        public Color TokenColor => tokenColor;
        public string Backstory => backstory;
        public string AbilityName => abilityName;
        public string AbilityDescription => abilityDescription;
        public int AbilityCooldownTurns => abilityCooldownTurns;
        public int StartingEnergy => startingEnergy;
        public int MaxEnergy => maxEnergy;
        public EraType StartingEra => startingEra;

        public void SetupCharacter(int index, string name, string role, Color color, string bio,
            string ability, string abilityDesc, int startingEn = 10, EraType startEra = EraType.SpanishEra)
        {
            playerIndex = index;
            characterName = name;
            roleTitle = role;
            tokenColor = color;
            backstory = bio;
            abilityName = ability;
            abilityDescription = abilityDesc;
            startingEnergy = startingEn;
            startingEra = startEra;
        }
    }
}

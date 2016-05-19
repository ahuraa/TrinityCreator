﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrinityCreator;

namespace TrinityCreator.CreatureTemplates
{
    class QuestgiverTemplate : TemplateBase
    {
        public QuestgiverTemplate() : base() { }

        public override void LoadTemplate()
        {
            MinLevel = 80;
            MaxLevel = 80;
            Faction = 35;
            SetBmspValues(NpcFlags, new int[] { 1, 2 });     // Gossip, Questgiver
            SetBmspValues(FlagsExtra, new int[] { 2 });     // Civilian
            SetIKVPValue(Page.creatureTypeCb, 7);           // Humanoid
        }
    }
}

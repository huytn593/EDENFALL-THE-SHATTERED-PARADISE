using Edenfall.NPC.Relationships;
using NUnit.Framework;

namespace Edenfall.Tests.EditMode
{
    public class RelationshipSystemTests
    {
        [Test]
        public void Relationship_InitialValues_AreClampedProperly()
        {
            var rel = new RelationshipModel("aurelia")
            {
                affection = 20,
                trust = 30
            };

            Assert.AreEqual(20, rel.affection);
            Assert.AreEqual(30, rel.trust);
            Assert.AreEqual(RelationshipStage.Stranger, rel.currentStage);
        }

        [Test]
        public void Relationship_ApplyDeltas_ClampsWithinZeroAndHundred()
        {
            var rel = new RelationshipModel("aurelia")
            {
                affection = 95
            };

            rel.ApplyDeltas(aff: 20, tru: 10, fam: 0, res: 0, com: 0, con: -50);

            Assert.AreEqual(100, rel.affection, "Affection should be clamped at 100 max.");
            Assert.AreEqual(0, rel.conflict, "Conflict should be clamped at 0 min.");
        }

        [Test]
        public void Relationship_StageTransitions_AdvanceProperly()
        {
            var rel = new RelationshipModel("aurelia");

            // Stranger initially
            rel.EvaluateStage();
            Assert.AreEqual(RelationshipStage.Stranger, rel.currentStage);

            // Acquaintance
            rel.ApplyDeltas(aff: 15, tru: 15, fam: 15, res: 10, com: 10, con: 0);
            Assert.AreEqual(RelationshipStage.Acquaintance, rel.currentStage);

            // Trusted Companion
            rel.ApplyDeltas(aff: 10, tru: 15, fam: 10, res: 25, com: 10, con: 0);
            Assert.AreEqual(RelationshipStage.TrustedCompanion, rel.currentStage);

            // Romantic Possibility (affection >= 50, trust >= 45, comfort >= 40)
            rel.ApplyDeltas(aff: 30, tru: 20, fam: 10, res: 10, com: 25, con: 0);
            Assert.AreEqual(RelationshipStage.RomanticPossibility, rel.currentStage);
        }

        [Test]
        public void Relationship_AddMemory_AppendsToList()
        {
            var rel = new RelationshipModel("aurelia");
            rel.AddMemory("Shared an intimate night beneath the celestial lilies.", NPCMemory.MemoryImpact.Romantic, "Day 1 - Evening");

            Assert.AreEqual(1, rel.memories.Count);
            Assert.AreEqual(NPCMemory.MemoryImpact.Romantic, rel.memories[0].impact);
            Assert.AreEqual("Day 1 - Evening", rel.memories[0].timestamp);
        }
    }
}

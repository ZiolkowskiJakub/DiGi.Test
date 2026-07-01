using DiGi.Core.Classes;
using DiGi.Core.Interfaces;
using DiGi.Core.Relation.Classes;
using DiGi.Core.Relation.Interfaces;
using System;
using System.Collections.Generic;

namespace DiGi.Core.xUnit
{
    public partial class Facts
    {
        private class TestUniqueObject : GuidObject
        {
            public TestUniqueObject() : base()
            {
            }

            public TestUniqueObject(Guid guid) : base(guid)
            {
            }
        }

        [Fact]
        public void OneToOneBidirectionalRelation_Syncs_Correctly()
        {
            var objA = new TestUniqueObject();
            var objB = new TestUniqueObject();

            var relation = new OneToOneBidirectionalRelation(objA, objB);

            // Verify both ends of the relation are set
            Assert.NotNull(relation.UniqueReference_From);
            Assert.NotNull(relation.UniqueReference_To);
            Assert.Equal(objA.UniqueId, relation.UniqueReference_From.UniqueId);
            Assert.Equal(objB.UniqueId, relation.UniqueReference_To.UniqueId);

            // Test if Contains works for both objects using correct RelationSide and UniqueReferences
            Assert.True(relation.Contains(DiGi.Core.Relation.Enums.RelationSide.From, Create.UniqueReference(objA)));
            Assert.True(relation.Contains(DiGi.Core.Relation.Enums.RelationSide.To, Create.UniqueReference(objB)));
        }

        [Fact]
        public void UniqueObjectRelationCluster_ManageRelations()
        {
            var cluster = new UniqueObjectRelationCluster<TestUniqueObject, IRelation>();

            var objA = new TestUniqueObject();
            var objB = new TestUniqueObject();
            cluster.Add(objA);
            cluster.Add(objB);

            var relation = new OneToOneBidirectionalRelation(objA, objB);

            // Add relation to the cluster
            var addedRelation = cluster.AddRelation(relation);
            Assert.NotNull(addedRelation);

            // Verify relation lookup using uniqueObject parameter
            var relations = cluster.GetRelations<OneToOneBidirectionalRelation>(objA);
            Assert.NotNull(relations);
            Assert.Single(relations);
            Assert.Equal(relation, relations[0]);
        }

        private class TestManyToOneRelation : ManyToOneRelation
        {
            public TestManyToOneRelation(IEnumerable<IUniqueReference>? uniqueReferences_From, IUniqueReference? uniqueReference_To)
                : base(uniqueReferences_From, uniqueReference_To)
            {
            }
        }

        /// <summary>
        /// Tests that ManyToOneRelation handles null references during removal operations safely without throwing NullReferenceException.
        /// </summary>
        [Fact]
        public void ManyToOneRelation_NullSafety()
        {
            TestManyToOneRelation relation_ManyToOne = new(null, null);

            // Verify that calling Remove with a null uniqueReferences_From list does not throw NullReferenceException
            List<IUniqueReference> uniqueReferences_ToRemove = [Create.UniqueReference(new TestUniqueObject())!];
            List<IUniqueReference>? uniqueReferences_Removed = relation_ManyToOne.Remove(DiGi.Core.Relation.Enums.RelationSide.From, uniqueReferences_ToRemove);

            Assert.NotNull(uniqueReferences_Removed);
            Assert.Empty(uniqueReferences_Removed);
        }
    }
}
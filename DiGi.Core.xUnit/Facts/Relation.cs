using DiGi.Core.Classes;
using DiGi.Core.Relation.Classes;
using DiGi.Core.Relation.Interfaces;
using System;

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
    }
}
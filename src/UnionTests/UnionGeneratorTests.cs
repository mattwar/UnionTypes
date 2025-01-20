using UnionTypes;
using UnionTypes.Generators;

namespace UnionTests
{
    [TestClass]
    public class UnionGeneratorTests
    {
        [TestMethod]
        public void TestTypeUnion_CatOrDog()
        {
            TestGenerate(
                new Union(
                    UnionKind.TypeUnion,
                    name: "DogOrCat",
                    typeName: "DogOrCat",
                    accessibility: "public",
                    [
                        new UnionCase(
                            name: "Dog",
                            type: "Dog",
                            tagValue: -1,
                            factoryName:"CreateDog",
                            factoryParameters: [
                                new UnionCaseValue("dog", "Dog", TypeKind.DecomposableLocalRecordStruct,
                                    [new UnionCaseValue("name", "string", TypeKind.Class)])
                                ]),
                        new UnionCase(
                            name: "Cat",
                            type: "Cat",
                            tagValue: -1,
                            factoryName: "CreateCat",
                            factoryParameters: [
                                new UnionCaseValue("cat", "Cat", TypeKind.DecomposableLocalRecordStruct,
                                    [new UnionCaseValue("name", "string", TypeKind.Class)])
                                ]),
                    ],
                    UnionOptions.Default.WithShareReferenceFields(false)
                    ),
                expectedText: "",
                namespaceName: "TestUnions",
                usings: ["System", "System.Collections.Generic", "UnionTypes"]
                );
        }

        private void TestGenerate(
            Union union, 
            string expectedText,
            string? namespaceName = "",
            string[]? usings = null
            )
        {
            var generator = new UnionGenerator(
                namespaceName,
                usings
                );

            var actualText = generator.GenerateFile(union);
            //Assert.AreEqual(expectedText, actualText);
        }
    }
}
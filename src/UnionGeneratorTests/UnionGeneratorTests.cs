using UnionTypes;
using UnionTypes.Generators;

namespace UnionTests
{
    [TestClass]
    public class UnionGeneratorTests
    {
        [TestMethod]
        public void TestTagUnion_Result()
        {
            TestGenerate(
                new Union(
                    UnionKind.TypeUnion,
                    "Result",
                    "Result<TValue, TError>",
                    "public",
                    new[]
                    {
                        new UnionCase(
                            name: "Success",
                            type: "TValue",
                            typeKind: TypeKind.TypeParameter_Unconstrained,
                            tagValue: 1,
                            factoryName:"Success",
                            factoryParameters: new [] { new UnionCaseValue("value", "TValue", TypeKind.TypeParameter_Unconstrained) },
                            accessorName: "SuccessValue"),
                        new UnionCase(
                            name: "Failure",
                            type: "TError",
                            typeKind: TypeKind.TypeParameter_Unconstrained,
                            tagValue: 2,
                            factoryName: "Failure",
                            factoryParameters: new [] { new UnionCaseValue("error", "TError", TypeKind.TypeParameter_Unconstrained) },
                            accessorName: "FailureValue")
                    },
                    UnionOptions.Default
                        .WithGenerateMatch(true)
                        .WithGenerateEquality(true)
                        .WithGenerateToString(true)
                    ),
                namespaceName: "UnionTypes"
                );
        }

        [TestMethod]
        public void TestTagUnion_Option()
        {
            TestGenerate(
                new Union(
                    UnionKind.TypeUnion,
                    "Option",
                    "Option<TValue>",
                    "public",
                    new[]
                    {
                        new UnionCase(
                            name: "None",
                            type: "UnionTypes.None",
                            typeKind: TypeKind.Class,
                            tagValue: 0,
                            factoryName:"None",
                            factoryParameters: null,
                            factoryIsProperty: true,
                            hasAccessor: false,
                            isSingleton: true),
                        new UnionCase(
                            name: "Some",
                            type: "TValue",
                            typeKind: TypeKind.Unknown,
                            tagValue: 1,
                            factoryName: "Some",
                            factoryParameters: new [] { new UnionCaseValue("value", "TValue", TypeKind.TypeParameter_Unconstrained) },
                            accessorName: "Value")
                    },
                    UnionOptions.Default
                        .WithGenerateMatch(true)
                        .WithGenerateEquality(true)
                        .WithGenerateToString(true)
                    ),
                namespaceName: "UnionTypes"
                );
        }


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
                            typeKind: TypeKind.DecomposableLocalRecordStruct,
                            tagValue: -1,
                            factoryName:"CreateDog",
                            factoryParameters: [
                                new UnionCaseValue("dog", "Dog", TypeKind.DecomposableLocalRecordStruct,
                                    [new UnionCaseValue("name", "string", TypeKind.Class)])
                                ]),
                        new UnionCase(
                            name: "Cat",
                            type: "Cat",
                            typeKind: TypeKind.DecomposableLocalRecordStruct,
                            tagValue: -1,
                            factoryName: "CreateCat",
                            factoryParameters: [
                                new UnionCaseValue("cat", "Cat", TypeKind.DecomposableLocalRecordStruct,
                                    [new UnionCaseValue("name", "string", TypeKind.Class)])
                                ]),
                    ],
                    UnionOptions.Default.WithShareReferenceFields(false)
                    ),
                namespaceName: "TestUnions",
                usings: ["System", "System.Collections.Generic", "UnionTypes"]
                );
        }

        private void TestGenerate(
            Union union, 
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
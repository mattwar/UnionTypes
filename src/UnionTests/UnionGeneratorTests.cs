using UnionTypes;
using UnionTypes.Generators;

namespace UnionTests
{
    [TestClass]
    public class UnionGeneratorTests
    {
        [TestMethod]
        public void TestTagUnion_Result1()
        {
            TestGenerate(
                new Union(
                    UnionKind.TagUnion,
                    "Result",
                    "Result<TValue>",
                    "public",
                    new[]
                    {
                        new UnionCase(
                            name: "Success", 
                            type: null, 
                            tagValue: 1, 
                            factoryName:"Success", 
                            factoryParameters: new [] { new UnionCaseValue("value", "TValue", TypeKind.TypeParameter_Unconstrained) }, 
                            accessorName: "SuccessValue"),
                        new UnionCase(
                            name: "Failure", 
                            type: null,
                            tagValue: 2, 
                            factoryName: "Failure", 
                            factoryParameters: new [] { new UnionCaseValue("message", "string", TypeKind.Class) },
                            accessorName: "FailureMessage")
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
        public void TestTagUnion_Result2()
        {
            TestGenerate(
                new Union(
                    UnionKind.TagUnion,
                    "Result",
                    "Result<TValue, TError>",
                    "public",
                    new[]
                    {
                        new UnionCase(
                            name: "Success",
                            type: null,
                            tagValue: 1,
                            factoryName:"Success",
                            factoryParameters: new [] { new UnionCaseValue("value", "TValue", TypeKind.TypeParameter_Unconstrained) },
                            accessorName: "SuccessValue"),
                        new UnionCase(
                            name: "Failure",
                            type: null,
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
                    UnionKind.TagUnion,
                    "Option",
                    "Option<TValue>",
                    "public",
                    new[]
                    {
                        new UnionCase(
                            name: "None",
                            type: null,
                            tagValue: 0,
                            factoryName:"None",
                            factoryParameters: null,
                            factoryIsProperty: true,
                            accessorName: "IsNone"),
                        new UnionCase(
                            name: "Some",
                            type: null,
                            tagValue: 1,
                            factoryName: "Some",
                            factoryParameters: new [] { new UnionCaseValue("value", "TValue", TypeKind.TypeParameter_Unconstrained) },
                            accessorName: "SomeValue")
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
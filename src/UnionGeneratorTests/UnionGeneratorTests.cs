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
                            new UnionValueType("TValue", TypeKind.TypeParameter_Unconstrained),
                            tagValue: 1,
                            factoryName:"Success",
                            factoryParameters: new [] { new UnionCaseValue("value", new UnionValueType("TValue", TypeKind.TypeParameter_Unconstrained)) },
                            accessorName: "SuccessValue"),
                        new UnionCase(
                            name: "Failure",
                            new UnionValueType("TError", TypeKind.TypeParameter_Unconstrained),
                            tagValue: 2,
                            factoryName: "Failure",
                            factoryParameters: new [] { new UnionCaseValue("error", new UnionValueType("TError", TypeKind.TypeParameter_Unconstrained)) },
                            accessorName: "FailureValue")
                    },
                    UnionOptions.Default
                        .WithUseToolkit(true)
                        .WithGenerateMatch(true)
                        .WithGenerateEquality(true)
                        .WithGenerateToString(true)
                    ),
                namespaceName: "UnionTypes.Toolkit"
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
                            name: "Some",
                            type: null,
                            tagValue: 1,
                            factoryName: "Some",
                            factoryParameters: new [] { new UnionCaseValue("value", new UnionValueType("TValue", TypeKind.TypeParameter_Unconstrained)) },
                            accessorName: "Value"),
                        new UnionCase(
                            name: "None",
                            type: new UnionValueType("UnionTypes.Toolkit.None", TypeKind.Class, "Singleton"),
                            tagValue: 0,
                            factoryName:"None",
                            factoryParameters: null,
                            hasAccessor: false)
                    },
                    UnionOptions.Default
                        .WithUseToolkit(true)
                        .WithGenerateMatch(true)
                        .WithGenerateEquality(true)
                        .WithGenerateToString(true)
                    ),
                namespaceName: "UnionTypes.Toolkit"
                );
        }


        [TestMethod]
        public void TestTypeUnion_CatOrDog()
        {
            var dogType = new UnionValueType("Dog", TypeKind.DecomposableLocalRecordStruct);
            var catType = new UnionValueType("Cat", TypeKind.DecomposableLocalRecordStruct);

            TestGenerate(
                new Union(
                    UnionKind.TypeUnion,
                    name: "DogOrCat",
                    typeName: "DogOrCat",
                    modifiers: "public",
                    [
                        new UnionCase(
                            name: "Dog",
                            type: dogType,
                            tagValue: -1,
                            factoryName:"CreateDog",
                            factoryParameters: [
                                new UnionCaseValue("dog", dogType, [new UnionCaseValue("name", UnionValueType.String)])
                                ]),
                        new UnionCase(
                            name: "Cat",
                            type: catType,
                            tagValue: -1,
                            factoryName: "CreateCat",
                            factoryParameters: [
                                new UnionCaseValue("cat", catType, [new UnionCaseValue("name", UnionValueType.String)])
                                ]),
                    ],
                    UnionOptions.Default.WithShareReferenceFields(false)
                    ),
                namespaceName: "TestUnions",
                usings: ["System", "System.Collections.Generic", "UnionTypes.Toolkit"]
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
        }
    }
}
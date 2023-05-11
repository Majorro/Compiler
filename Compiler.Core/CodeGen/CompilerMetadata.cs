using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;

namespace Compiler.Core.CodeGen;

public partial class CodeCompiler
{
    protected TypeReferenceHandle SystemObjectTypeRef, ArgumentExceptionTypeRef;
    protected List<TypeReferenceHandle> SystemValueTypeRefs = new();
    protected Dictionary<TypeReferenceHandle, List<MemberReference>> SystemValueTypeFieldRefs = new();

    public BlobHandle EncodeBlob(Action<BlobEncoder> encoder)
    {
        var blob = new BlobBuilder();
        encoder(new BlobEncoder(blob));
        return Metadata.GetOrAddBlob(blob);
    }

    protected void FinalizeProgram(List<MethodDefinitionHandle> methods)
    {
        // Create type definition for the special <Module> type that holds global functions
        Metadata.AddTypeDefinition(
            default,
            default,
            Metadata.GetOrAddString("<Module>"),
            default,
            MetadataTokens.FieldDefinitionHandle(1),
            methods[0]);

        // Create type definition for ConsoleApplication.Program
        Metadata.AddTypeDefinition(
            TypeAttributes.Class | TypeAttributes.Public |
            TypeAttributes.Abstract | TypeAttributes.Sealed |
            TypeAttributes.AutoLayout | TypeAttributes.BeforeFieldInit,
            Metadata.GetOrAddString("ProjectI"),
            Metadata.GetOrAddString($"Program{ProgramName}"),
            SystemObjectTypeRef,
            MetadataTokens.FieldDefinitionHandle(1),
            methods[0]);
    }

    protected void InitMemberRef()
    {
        // Get reference to Console.WriteLine(string) method.
        // new BlobEncoder(BB).MethodSignature().Parameters(1,
        //     returnType => returnType.Void(),
        //     parameters => parameters.AddParameter().Type().String());
        // ConsoleWriteLineMemberRef = Metadata.AddMemberReference(
        //     SystemConsoleTypeRef,
        //     Metadata.GetOrAddString("WriteLine"),
        //     Metadata.GetOrAddBlob(BB));
        // BB.Clear();

        new BlobEncoder(BB).MethodSignature(isInstanceMethod: true)
            .Parameters(0, returnType => returnType.Void(), _ => { });
        ParameterlessCtorBlobIndex = Metadata.GetOrAddBlob(BB);
        ObjectCtorMemberRef = Metadata.AddMemberReference(
            SystemObjectTypeRef,
            Metadata.GetOrAddString(".ctor"),
            ParameterlessCtorBlobIndex);
        BB.Clear();

        // Get reference to ArgumentExceptionConstructor method.
        new BlobEncoder(BB).MethodSignature(isInstanceMethod: true).Parameters(1,
            returnType => returnType.Void(),
            parameters => parameters.AddParameter().Type().String());
        ArgumentExceptionMemberRef = Metadata.AddMemberReference(
            ArgumentExceptionTypeRef,
            Metadata.GetOrAddString(".ctor"),
            Metadata.GetOrAddBlob(BB));
        BB.Clear();

        // Get reference to System.ValueTuple constructor method.
        // new BlobEncoder(BB).MethodSignature(isInstanceMethod: true).Parameters(1,
        //     returnType => returnType.Void(),
        //     parameters => parameters.AddParameter().Type().String());
        // ArgumentExceptionMemberRef = Metadata.AddMemberReference(
        //     ArgumentExceptionTypeRef,
        //     Metadata.GetOrAddString(".ctor"),
        //     Metadata.GetOrAddBlob(BB));
        // BB.Clear();

        // new BlobEncoder(BB).MethodSignature().Parameters(1,
        //     returnType => returnType.Type().Int32(),
        //     parameters => parameters.AddParameter().Type().String());
        // Int32ParseMemberRef = Metadata.AddMemberReference(
        //     SystemInt32TypeRef,
        //     Metadata.GetOrAddString("Parse"),
        //     Metadata.GetOrAddBlob(BB));
        // BB.Clear();
    }

    protected MemberReferenceHandle GetRecordField(string recordType, int fieldI)
    {
        var typeSpecSig = new BlobEncoder(BB).TypeSpecificationSignature();
        TypeMap(recordType)(typeSpecSig);
        typeSpecSig.GenericTypeParameter(fieldI - 1);
        var typeSpec = Metadata.AddTypeSpecification(Metadata.GetOrAddBlob(BB));
        BB.Clear();

        new BlobEncoder(BB).Field().Type().GenericTypeParameter(fieldI - 1);
        var res = Metadata.AddMemberReference(
            typeSpec,
            Metadata.GetOrAddString($"Item{fieldI}"),
            Metadata.GetOrAddBlob(BB)
        );
        BB.Clear();
        return res;
    }

    protected void InitTypeRefs()
    {
        var mscorlibAssemblyRef = Metadata.AddAssemblyReference(
            Metadata.GetOrAddString("mscorlib"),
            new Version(4, 0, 0, 0),
            default,
            Metadata.GetOrAddBlob(
                new byte[] { 0xB7, 0x7A, 0x5C, 0x56, 0x19, 0x34, 0xE0, 0x89 }
            ),
            default,
            default);

        // Create references to System.Object and System.Console types.
        SystemObjectTypeRef = Metadata.AddTypeReference(
            mscorlibAssemblyRef,
            Metadata.GetOrAddString("System"),
            Metadata.GetOrAddString("Object"));
        //
        // SystemConsoleTypeRef = Metadata.AddTypeReference(
        //     mscorlibAssemblyRef,
        //     Metadata.GetOrAddString("System"),
        //     Metadata.GetOrAddString("Console"));
        //
        ArgumentExceptionTypeRef = Metadata.AddTypeReference(
            mscorlibAssemblyRef,
            Metadata.GetOrAddString("System"),
            Metadata.GetOrAddString("ArgumentException"));

        for (var i = 0; i <= 8; i++)
        {
            var typeRef = Metadata.AddTypeReference(
                mscorlibAssemblyRef,
                Metadata.GetOrAddString("System"),
                Metadata.GetOrAddString(i > 0 ? $"ValueTuple`{i}" : "ValueTuple"));
            SystemValueTypeRefs.Add(typeRef);
        }
        //
        // SystemInt32TypeRef = Metadata.AddTypeReference(
        //     mscorlibAssemblyRef,
        //     Metadata.GetOrAddString("System"),
        //     Metadata.GetOrAddString("Int32"));
    }

    protected void InitMetadataModuleAsm()
    {
        // Create module and assembly for a console application.
        Metadata.AddModule(
            0,
            Metadata.GetOrAddString("Program"),
            Metadata.GetOrAddGuid(SGuid),
            default,
            default);

        Metadata.AddAssembly(
            Metadata.GetOrAddString("Program"),
            new Version(1, 0, 0, 0),
            default,
            default,
            0,
            AssemblyHashAlgorithm.None);
    }

    public BlobBuilder WriteMetadataToBlob()
    {
        var peHeaderBuilder = new PEHeaderBuilder(
            imageCharacteristics: Characteristics.ExecutableImage |
                                  Characteristics.LargeAddressAware |
                                  Characteristics.Dll
        );

        var peBuilder = new ManagedPEBuilder(
            peHeaderBuilder,
            new MetadataRootBuilder(Metadata),
            IlBuilder,
            flags: CorFlags.ILOnly,
            deterministicIdProvider: _ => SContentId);

        // Write executable into the specified stream.
        var peBlob = new BlobBuilder();
        var _ = peBuilder.Serialize(peBlob);
        return peBlob;
    }

    public void WriteMetadataToFile(string path)
    {
        using var peStream = new FileStream(
            path,
            FileMode.OpenOrCreate,
            FileAccess.ReadWrite
        );
        var peBlob = WriteMetadataToBlob();
        peBlob.WriteContentTo(peStream);
    }

    // protected void DebugPrintRoutineIl()
    // {
    //     foreach (var v in InstructionEncoder.CodeBuilder.ToArray())
    //     {
    //         Console.WriteLine($": {Enum.GetName(typeof(ILOpCode), v)}");
    //     }
    // }
}
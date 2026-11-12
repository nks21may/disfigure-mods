param(
    [Parameter(Mandatory)] [string] $Dll,
    [Parameter(Mandatory)] [string] $Type,
    [Parameter(Mandatory)] [string[]] $Methods
)
# For each method of $Type, says whether the interop has a NativeMethodInfoPtr_ field (the
# method is in the game build: NATIVE) or not (IL2CPP stripped it and Il2CppInterop rebuilt
# it from Unity's code: RECONSTRUCTED). A reconstructed method can crash the game without an
# exception (ADR-0005, ADR-0007).
#
# Usage: tools/native-methods.ps1 <interop dll> <type> <methods>
#   tools/native-methods.ps1 J:\...\BepInEx\interop\UnityEngine.IMGUIModule.dll GUI Label,DrawTextureWithTexCoords
#
# Ask for constructors as .ctor. If a method has overloads, the pointers of all of them are
# counted: NATIVE means at least one overload is in the build.
$fs = [System.IO.File]::OpenRead($Dll)
$pe = [System.Reflection.PortableExecutable.PEReader]::new($fs)
$md = [System.Reflection.Metadata.PEReaderExtensions]::GetMetadataReader($pe)
foreach ($h in $md.TypeDefinitions) {
    $t = $md.GetTypeDefinition($h)
    if ($md.GetString($t.Name) -ne $Type) { continue }
    $fields = foreach ($fh in $t.GetFields()) { $md.GetString($md.GetFieldDefinition($fh).Name) }
    $allMethods = foreach ($mh in $t.GetMethods()) { $md.GetString($md.GetMethodDefinition($mh).Name) }
    foreach ($m in $Methods) {
        # In the field name, the dot of .ctor becomes an underscore: _ctor.
        $prefix = "NativeMethodInfoPtr_$($m.Replace('.', '_'))_"
        $pointers = @($fields | Where-Object { $_.StartsWith($prefix, [StringComparison]::Ordinal) })
        $exists = @($allMethods | Where-Object { $_ -eq $m }).Count -gt 0
        $state = if ($pointers.Count -gt 0) { 'NATIVE' } elseif ($exists) { 'RECONSTRUCTED' } else { 'MISSING' }
        "{0,-14} {1}.{2}  ({3} pointers)" -f $state, $Type, $m, $pointers.Count
    }
}
$fs.Dispose()

param(
    [Parameter(Mandatory)] [string] $Dll,
    [Parameter(Mandatory)] [string] $Tipo,
    [Parameter(Mandatory)] [string[]] $Metodos
)
# Para cada método de $Tipo, dice si el interop tiene un campo NativeMethodInfoPtr_ (el
# método está en el build del juego: NATIVO) o no (IL2CPP lo sacó y Il2CppInterop lo
# reconstruyó desde el código de Unity: RECONSTRUIDO). Un método reconstruido puede crashear
# el juego sin excepción (ADR-0005, ADR-0007).
#
# Uso: herramientas/metodos-nativos.ps1 <dll del interop> <tipo> <métodos>
#   herramientas/metodos-nativos.ps1 J:\...\BepInEx\interop\UnityEngine.IMGUIModule.dll GUI Label,DrawTextureWithTexCoords
#
# Los constructores se piden como .ctor. Si un método tiene sobrecargas, cuenta los punteros
# de todas: NATIVO dice que al menos una está en el build.
$fs = [System.IO.File]::OpenRead($Dll)
$pe = [System.Reflection.PortableExecutable.PEReader]::new($fs)
$md = [System.Reflection.Metadata.PEReaderExtensions]::GetMetadataReader($pe)
foreach ($h in $md.TypeDefinitions) {
    $t = $md.GetTypeDefinition($h)
    if ($md.GetString($t.Name) -ne $Tipo) { continue }
    $campos = foreach ($fh in $t.GetFields()) { $md.GetString($md.GetFieldDefinition($fh).Name) }
    $todosLosMetodos = foreach ($mh in $t.GetMethods()) { $md.GetString($md.GetMethodDefinition($mh).Name) }
    foreach ($m in $Metodos) {
        # En el nombre del campo, el punto de .ctor queda como guion bajo: _ctor.
        $prefijo = "NativeMethodInfoPtr_$($m.Replace('.', '_'))_"
        $ptr = @($campos | Where-Object { $_.StartsWith($prefijo, [StringComparison]::Ordinal) })
        $existe = @($todosLosMetodos | Where-Object { $_ -eq $m }).Count -gt 0
        $estado = if ($ptr.Count -gt 0) { 'NATIVO' } elseif ($existe) { 'RECONSTRUIDO' } else { 'NO EXISTE' }
        "{0,-14} {1}.{2}  ({3} punteros)" -f $estado, $Tipo, $m, $ptr.Count
    }
}
$fs.Dispose()

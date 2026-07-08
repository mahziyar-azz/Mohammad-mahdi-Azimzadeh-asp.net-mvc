$connString = "Server=.;Database=AzimzadehStoreDb;Integrated Security=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection($connString)
try {
    $conn.Open()
    $cmd = $conn.CreateCommand()
    $cmd.CommandText = "SELECT COUNT(*) FROM Gallery"
    $count = $cmd.ExecuteScalar()
    Write-Host "Gallery Count: $count"
    
    if ($count -gt 0) {
        $cmd.CommandText = "SELECT * FROM Gallery"
        $adapter = New-Object System.Data.SqlClient.SqlDataAdapter($cmd)
        $dt = New-Object System.Data.DataTable
        $adapter.Fill($dt) > $null
        foreach ($r in $dt.Rows) {
            Write-Host "Id: $($r['GalleryId']), Title: $($r['Title']), Image: $($r['ImagePath']), Active: $($r['IsActive'])"
        }
    }
} catch {
    Write-Host "Error: $($_.Exception.Message)"
} finally {
    $conn.Close()
}

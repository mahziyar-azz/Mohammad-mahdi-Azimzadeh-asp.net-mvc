$connString = "Server=.;Database=AzimzadehStoreDb;Integrated Security=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection($connString)
try {
    $conn.Open()
    
    # 1. Print all Roles
    $cmd = $conn.CreateCommand()
    $cmd.CommandText = "SELECT * FROM Roles"
    $adapter = New-Object System.Data.SqlClient.SqlDataAdapter($cmd)
    $dtRoles = New-Object System.Data.DataTable
    $adapter.Fill($dtRoles) > $null
    
    Write-Host "--- ROLES ---"
    foreach ($r in $dtRoles.Rows) {
        Write-Host "RoleId: $($r['RoleId']), RoleName: $($r['RoleName'])"
    }
    
    # 2. Print User Roles
    $cmd.CommandText = "SELECT u.Email, r.RoleName FROM UserRoles ur JOIN Users u ON ur.UserId = u.UserId JOIN Roles r ON ur.RoleId = r.RoleId"
    $dtUserRoles = New-Object System.Data.DataTable
    $adapter.Fill($dtUserRoles) > $null
    Write-Host "--- USER ROLES ---"
    foreach ($ur in $dtUserRoles.Rows) {
        Write-Host "Email: $($ur['Email']), Role: $($ur['RoleName'])"
    }
    
} catch {
    Write-Host "Error: $($_.Exception.Message)"
} finally {
    $conn.Close()
}

$conn = New-Object System.Data.SqlClient.SqlConnection('data source=.;initial catalog=AzimzadehStoreDb;integrated security=True')
$conn.Open()
$cmd = $conn.CreateCommand()
$cmd.CommandText = "SELECT UserId, FullName, FirstName, LastName, Email FROM Users"
$reader = $cmd.ExecuteReader()
while ($reader.Read()) {
    Write-Host "Id: $($reader['UserId']) | Email: $($reader['Email']) | FullName: $($reader['FullName']) | FirstName: $($reader['FirstName']) | LastName: $($reader['LastName'])"
}
$conn.Close()

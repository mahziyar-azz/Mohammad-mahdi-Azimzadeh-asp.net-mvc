$path = "C:\Users\Mahziyar Azimzadeh\source\repos\Azimzadeh MVC project\Azimzadeh MVC project\Models\Model1.edmx"
$c = [System.IO.File]::ReadAllText($path)

# 1. Update SSDL
$ssdlOld = '<Property Name="UpdatedAt" Type="datetime2" Precision="7" />' + "`r`n" + '        </EntityType>'
# Let's match without exact line endings to be safe
$ssdlOldRegex = '(?i)<Property\s+Name="UpdatedAt"\s+Type="datetime2"\s+Precision="7"\s*/>\s*</EntityType>'
$ssdlNew = '<Property Name="UpdatedAt" Type="datetime2" Precision="7" />
          <Property Name="FirstName" Type="nvarchar" MaxLength="100" />
          <Property Name="LastName" Type="nvarchar" MaxLength="100" />
          <Property Name="Address1" Type="nvarchar" MaxLength="500" />
          <Property Name="Address2" Type="nvarchar" MaxLength="500" />
          <Property Name="HomePhoneNumber" Type="nvarchar" MaxLength="50" />
          <Property Name="CardNumber" Type="nvarchar" MaxLength="50" />
          <Property Name="CVV" Type="nvarchar" MaxLength="10" />
          <Property Name="ExpirationDate" Type="nvarchar" MaxLength="20" />
          <Property Name="Gender" Type="nvarchar" MaxLength="20" />
        </EntityType>'

# We do a replacement using regex. But wait, in SSDL we want to make sure it only replaces inside <EntityType Name="Users">.
# Let's target the exact text around it.
$targetSsdl = '(?s)(<EntityType Name="Users">.*?)<Property\s+Name="UpdatedAt"\s+Type="datetime2"\s+Precision="7"\s*/>\s*</EntityType>'
$c = [regex]::Replace($c, $targetSsdl, '$1' + $ssdlNew)

# 2. Update CSDL
$csdlNew = '<Property Name="UpdatedAt" Type="DateTime" Precision="7" />
          <Property Name="FirstName" Type="String" MaxLength="100" FixedLength="false" Unicode="true" />
          <Property Name="LastName" Type="String" MaxLength="100" FixedLength="false" Unicode="true" />
          <Property Name="Address1" Type="String" MaxLength="500" FixedLength="false" Unicode="true" />
          <Property Name="Address2" Type="String" MaxLength="500" FixedLength="false" Unicode="true" />
          <Property Name="HomePhoneNumber" Type="String" MaxLength="50" FixedLength="false" Unicode="true" />
          <Property Name="CardNumber" Type="String" MaxLength="50" FixedLength="false" Unicode="true" />
          <Property Name="CVV" Type="String" MaxLength="10" FixedLength="false" Unicode="true" />
          <Property Name="ExpirationDate" Type="String" MaxLength="20" FixedLength="false" Unicode="true" />
          <Property Name="Gender" Type="String" MaxLength="20" FixedLength="false" Unicode="true" />'

$targetCsdl = '(?s)(<EntityType Name="User">.*?)<Property\s+Name="UpdatedAt"\s+Type="DateTime"\s+Precision="7"\s*/>'
$c = [regex]::Replace($c, $targetCsdl, '$1' + $csdlNew)

# 3. Update MSL
$mslNew = '<ScalarProperty Name="UpdatedAt" ColumnName="UpdatedAt" />
                <ScalarProperty Name="FirstName" ColumnName="FirstName" />
                <ScalarProperty Name="LastName" ColumnName="LastName" />
                <ScalarProperty Name="Address1" ColumnName="Address1" />
                <ScalarProperty Name="Address2" ColumnName="Address2" />
                <ScalarProperty Name="HomePhoneNumber" ColumnName="HomePhoneNumber" />
                <ScalarProperty Name="CardNumber" ColumnName="CardNumber" />
                <ScalarProperty Name="CVV" ColumnName="CVV" />
                <ScalarProperty Name="ExpirationDate" ColumnName="ExpirationDate" />
                <ScalarProperty Name="Gender" ColumnName="Gender" />'

$targetMsl = '(?s)(<EntityTypeMapping TypeName="AzimzadehStoreDbModels.User">.*?StoreEntitySet="Users">.*?)<ScalarProperty\s+Name="UpdatedAt"\s+ColumnName="UpdatedAt"\s*/>'
$c = [regex]::Replace($c, $targetMsl, '$1' + $mslNew)

# Write it back using UTF-8 (EF prefers UTF-8, but let's check if it compiles)
$utf8 = New-Object System.Text.UTF8Encoding($false)
[System.IO.File]::WriteAllText($path, $c, $utf8)
Write-Host "EDMX file updated."

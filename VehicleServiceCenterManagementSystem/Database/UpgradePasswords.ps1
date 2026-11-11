# Run after UpgradeDatabase.sql. Preserves the passwords users know; stores only hashes.
param([string]$Server='localhost\SQLEXPRESS',[string]$Database='VehicleServiceCenterDB')
$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Data
$builder=[System.Data.SqlClient.SqlConnectionStringBuilder]::new()
$builder['Data Source']=$Server
$builder['Initial Catalog']=$Database
$builder['Integrated Security']=$true
$connection=[System.Data.SqlClient.SqlConnection]::new($builder.ConnectionString)
$transaction=$null
try {
    $connection.Open()
    $transaction=$connection.BeginTransaction([System.Data.IsolationLevel]::Serializable)
    $read=$connection.CreateCommand()
    $read.Transaction=$transaction
    $read.CommandText='SELECT UserID,Password FROM dbo.Users WITH(UPDLOCK,HOLDLOCK)'
    $table=[System.Data.DataTable]::new()
    $reader=$read.ExecuteReader()
    $table.Load($reader)
    $reader.Dispose()
    $read.Dispose()
    $changed=0
    foreach($row in $table.Rows) {
        $stored=[string]$row.Password
        if($stored.StartsWith('PBKDF2$')){continue}
        $legacyHash=$false
        try { $legacyHash=([Convert]::FromBase64String($stored).Length -eq 48) } catch { }
        if($legacyHash){continue}
        $salt=[byte[]]::new(16)
        $rng=[System.Security.Cryptography.RandomNumberGenerator]::Create()
        try {$rng.GetBytes($salt)} finally {$rng.Dispose()}
        $derive=[System.Security.Cryptography.Rfc2898DeriveBytes]::new($stored,$salt,600000,[System.Security.Cryptography.HashAlgorithmName]::SHA256)
        try {$hash=$derive.GetBytes(32)} finally {$derive.Dispose()}
        $encoded='PBKDF2$600000$'+[Convert]::ToBase64String($salt)+'$'+[Convert]::ToBase64String($hash)
        $write=$connection.CreateCommand()
        try {
            $write.Transaction=$transaction
            $write.CommandText='UPDATE dbo.Users SET Password=@hash WHERE UserID=@id'
            $null=$write.Parameters.Add('@hash',[System.Data.SqlDbType]::NVarChar,200)
            $write.Parameters['@hash'].Value=$encoded
            $null=$write.Parameters.Add('@id',[System.Data.SqlDbType]::Int)
            $write.Parameters['@id'].Value=$row.UserID
            $null=$write.ExecuteNonQuery()
            $changed++
        } finally {$write.Dispose()}
    }
    $transaction.Commit()
    Write-Output "Legacy plaintext passwords upgraded: $changed. Usernames and known passwords are unchanged."
}
catch {if($transaction){try{$transaction.Rollback()}catch{}};throw}
finally {if($transaction){$transaction.Dispose()};$connection.Dispose()}


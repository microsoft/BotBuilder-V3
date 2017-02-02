# Create an object from a json template
$samplecodejson = '{"files": []}' | Out-String | ConvertFrom-Json
# Add an item to the files array for each file in the folder (recursive)
$samplecodejson.files = get-childitem "." -recurse | % {
    Write-Host "Adding " $_.FullName
    $_.FullName.subString($_.FullName.indexOf("\files\"),$_.FullName.Length - $_.FullName.indexOf("\files\"))
} 
# Convert object to json and write it to a utf8 file in the right location
Write-Host "Writing the JSON file in the _data folder"
($samplecodejson | ConvertTo-Json).replace('\\','/') | Out-File ..\..\_data\inspector-jsonfiles.json -Encoding utf8
Write-Host "Done! Press any key to continue ..."
$x = $host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
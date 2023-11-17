# create output directory
$timestamp = Get-Date -Format "yyyyMMdd_hhmm";
$buildDir = "WebIO_$timestamp";
New-Item -ItemType Directory $buildDir  | Out-Null;

# build
Push-Location "..\src\WebIO\";
Write-Host "deploying configurations";
dotnet publish -c Release #-r win-x64 --no-restore #--self-contained --runtime win-x64
Pop-Location;

# copy output
Copy-Item -Path "..\src\WebIO\WebIO\bin\Release\net6.0\publish" -Destination "$buildDir\app" -Recurse;

# TODO: build web stuff
Push-Location "..\webclient\";
# npm run build-prod
npm run build
Pop-Location;

# copy output
Copy-Item -Path "..\webclient\dist\web-io" -Destination "$buildDir\wwwroot" -Recurse;

# build docker image
Copy-Item -Path ".\Dockerfile" -Destination $buildDir;

# Build Image
Push-Location $buildDir;
docker build -t webio:latest .
# docker build -t webio:latest .
Pop-Location;
Remove-Item -path $buildDir -recurse


#$dest = "WebIO_$timestamp.zip"
#Add-Type -assembly "system.io.compression.filesystem"
#[io.compression.zipfile]::CreateFromDirectory($buildDir, $dest)

# Push-Location $buildDir
# docker build -t mid .
# docker save mid:latest --output mid_docker_image.tar
# Pop-Location
